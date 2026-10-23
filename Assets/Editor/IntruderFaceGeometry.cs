using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Authoring only. Start with the existing body's silhouette/weights, recover
// the scanned head, then sculpt a contained, off-axis jaw and unequal orbit.
// This is rest geometry only: all articulation remains owned by performance.
public static class IntruderFaceGeometry
{
    public static Mesh Create(Mesh body, Mesh scan)
    {
        ValidatePair(body, scan);
        var result = UnityEngine.Object.Instantiate(body);
        result.name = "IntruderFace / locked asymmetric jaw";
        var vertices = body.vertices;
        var normals = body.normals;
        var tangents = body.tangents;
        var rest = scan.vertices;
        var scanNormals = scan.normals;
        var scanTangents = scan.tangents;
        var eyes = new HashSet<int>(scan.GetTriangles(1));
        for (int i = 0; i < vertices.Length; i++)
        {
            float weight = Recovery(rest[i]);
            if (weight == 0) continue;
            // The old blanket deformation also squeezed the eyeballs. Restore
            // both whole globes; their original weights still drive gaze.
            if (eyes.Contains(i))
            {
                vertices[i] = rest[i]; normals[i] = scanNormals[i]; tangents[i] = scanTangents[i];
                continue;
            }
            vertices[i] = Vector3.Lerp(vertices[i], Sculpt(rest[i]), weight);
            // Transform the imported smooth normal/tangent through the sculpt.
            // RecalculateNormals would split the head at UV seams again.
            Vector3 t = ((Vector3)scanTangents[i]).normalized;
            Vector3 b = Vector3.Cross(scanNormals[i], t).normalized;
            const float epsilon = .0001f;
            Vector3 dt = (Sculpt(rest[i] + t * epsilon) - Sculpt(rest[i] - t * epsilon)).normalized;
            Vector3 db = (Sculpt(rest[i] + b * epsilon) - Sculpt(rest[i] - b * epsilon)).normalized;
            Vector3 n = Vector3.Cross(dt, db).normalized;
            normals[i] = Vector3.Slerp(normals[i], n, weight).normalized;
            Vector3 tangent = Vector3.Lerp((Vector3)tangents[i], dt, weight);
            tangent = (tangent - normals[i] * Vector3.Dot(tangent, normals[i])).normalized;
            tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, scanTangents[i].w);
        }
        result.vertices = vertices;
        result.normals = normals;
        result.tangents = tangents;
        result.SetUVs(2, rest.ToList());
        result.RecalculateBounds();
        return result;
    }

    public static void ValidatePair(Mesh body, Mesh scan)
    {
        // Editor mesh access is available even with runtime Read/Write disabled.
        // Validate the actual streams without changing the source import flags.
        if (!body || !scan
            || scan.vertexCount != 12216 || body.vertexCount != scan.vertexCount
            || body.subMeshCount != 2 || scan.subMeshCount != 2
            || body.blendShapeCount != 0 || scan.blendShapeCount != 0
            || body.normals.Length != body.vertexCount || scan.normals.Length != scan.vertexCount
            || body.tangents.Length != body.vertexCount || scan.tangents.Length != scan.vertexCount
            || !body.bindposes.SequenceEqual(scan.bindposes)
            || !body.boneWeights.SequenceEqual(scan.boneWeights)
            || !body.uv.SequenceEqual(scan.uv))
            throw new InvalidOperationException("IntruderFace needs the Editor-accessible Nathan/Embestida pair with identical topology, UV, bindposes and weights.");
        for (int sub = 0; sub < 2; sub++)
            if (!body.GetTriangles(sub).SequenceEqual(scan.GetTriangles(sub)))
                throw new InvalidOperationException("IntruderFace: body topology changed; inspect before applying.");
        if (scan.GetTriangles(1).Distinct().Count() != 454)
            throw new InvalidOperationException("IntruderFace: expected the separate 454-vertex eye submesh.");
    }

    static float Recovery(Vector3 p) => Smooth(1.575f, 1.625f, p.y) * (1 - Smooth(.12f, .16f, Mathf.Abs(p.x)));

    static Vector3 Sculpt(Vector3 p)
    {
        Vector3 q = p;
        float front = Smooth(-.025f, .055f, p.z);
        float faceWidth = 1 - Smooth(.075f, .115f, Mathf.Abs(p.x));

        // One coherent mandibular cage, not several overlapping cheek dents.
        // Turn the lower face about its rear hinge while keeping nose, skull,
        // ears and globes at the scan positions. The chin leaves the nasal axis
        // by about 11 mm; the complete lip/inner-mouth region follows the cage.
        float jaw = Smooth(1.584f, 1.615f, p.y)
            * (1 - Smooth(1.648f, 1.686f, p.y)) * front * faceWidth;
        Vector3 hinge = new Vector3(0, 1.657f, .012f);
        Vector3 turned = Quaternion.AngleAxis(-6.5f, Vector3.up) * (p - hinge) + hinge;
        q += (turned - p + new Vector3(-.0015f, -.0025f, .0035f)) * jaw;

        // A broad loaded masseter on negative scan-X is the principal asymmetry.
        // Keep cheek volume above it; the opposing side is taut, not hollowed.
        float loaded = Region(p, new Vector3(-.057f, 1.635f, .051f), new Vector3(.039f, .050f, .060f));
        float held = Region(p, new Vector3(.055f, 1.639f, .051f), new Vector3(.036f, .043f, .057f));
        q += new Vector3(-.0085f, -.0015f, .0035f) * loaded;
        q += new Vector3(.0020f, .0010f, .0010f) * held;

        // The scanned lip closure is around y=1.650, not the philtrum at 1.666.
        // Remap its whole curve to a compressed, descending arch. The smooth
        // cage includes the commissures, lip rolls and oral interior together,
        // keeping upper/lower lips and interior under the same spatial mapping.
        float u = Mathf.Clamp(p.x / .028f, -1, 1);
        float sourceSeam = 1.650f + .0012f * u * u;
        float seamOffset = p.y - sourceSeam;
        float mouthWidth = 1 - Smooth(.030f, .057f, Mathf.Abs(p.x));
        float mouth = mouthWidth * (1 - Smooth(.012f, .033f, Mathf.Abs(seamOffset)))
            * Smooth(.040f, .082f, p.z);
        float lowerCorner = .0085f * u * u + .0015f * Mathf.Max(0, -u);
        float press = 1 - Smooth(.004f, .014f, Mathf.Abs(seamOffset));
        q.y -= (lowerCorner + .38f * seamOffset * press) * mouth;
        q.x -= p.x * .07f * mouth;
        q.z -= .0015f * press * mouth;

        // Lower the inner brow as a continuous wedge over one orbit. Preserve
        // the lateral rim and bridge rather than painting a black socket.
        float browL = Region(p, new Vector3(-.026f, 1.741f, .095f), new Vector3(.036f, .028f, .038f));
        float browR = Region(p, new Vector3(.025f, 1.742f, .095f), new Vector3(.034f, .026f, .038f));
        // Fade the brow wedge before the lid rim so the separate lid
        // compression cannot fold that rim over the lower lid.
        float aboveLid = Smooth(1.721f, 1.737f, p.y);
        q += new Vector3(.0015f, -.0070f, .0020f) * browL * aboveLid;
        q += new Vector3(-.0005f, -.0025f, .0010f) * browR * aboveLid;

        // Negative-X eye is narrowed while the other stays watchful. All eye
        // vertices themselves are restored exactly from the scan in Create.
        float upperL = G(p, new Vector3(-.0309f, 1.7238f, .106f), new Vector3(.020f, .0048f, .012f));
        float upperR = G(p, new Vector3(.0278f, 1.7240f, .105f), new Vector3(.020f, .0048f, .012f));
        float lowerL = G(p, new Vector3(-.0309f, 1.7133f, .105f), new Vector3(.019f, .005f, .012f));
        float lowerR = G(p, new Vector3(.0278f, 1.7138f, .104f), new Vector3(.019f, .005f, .012f));
        q.y += -.0035f * upperL - .0015f * upperR + .0015f * lowerL + .0006f * lowerR;

        // Carry the loaded side into the upper neck. The neck's base, clothing,
        // shoulders and every body vertex below Recovery's boundary stay put.
        float neck = Region(p, new Vector3(-.032f, 1.598f, .038f), new Vector3(.023f, .050f, .045f));
        q += new Vector3(-.0030f, 0, .0025f) * neck;
        SeatLid(ref q, p, new Vector3(-.030894f, 1.719280f, .087987f), new Vector3(.017418f, .017351f, .017994f), upperL + lowerL + browL);
        SeatLid(ref q, p, new Vector3(.027798f, 1.719722f, .086991f), new Vector3(.017367f, .017317f, .017997f), upperR + lowerR + browR);
        return q;
    }

    // Compact, smooth volume cage. It has an exact zero boundary and a broad
    // interior; unlike multiplying gaussian amplitudes it does not accumulate
    // distant deformation across the whole face.
    static float Region(Vector3 p, Vector3 center, Vector3 radius)
    {
        Vector3 d = p - center;
        float r = Mathf.Sqrt(d.x * d.x / (radius.x * radius.x)
            + d.y * d.y / (radius.y * radius.y) + d.z * d.z / (radius.z * radius.z));
        return 1 - Smooth(.20f, 1, r);
    }

    static void SeatLid(ref Vector3 p, Vector3 source, Vector3 center, Vector3 radius, float influence)
    {
        if (influence <= .04f || source.z <= center.z) return;
        // Slide each tissue layer around the unchanged globe while preserving
        // its scanned radial depth. Pushing every inner-rim vertex onto the
        // globe's front surface would fold the eyelid's return faces outward.
        Vector3 rest = source - center;
        float shell = rest.x * rest.x / (radius.x * radius.x)
            + rest.y * rest.y / (radius.y * radius.y) + rest.z * rest.z / (radius.z * radius.z);
        float x = (p.x - center.x) / radius.x, y = (p.y - center.y) / radius.y;
        float inside = shell - x * x - y * y;
        if (inside > 0)
        {
            float front = center.z + radius.z * Mathf.Sqrt(inside);
            p.z = Mathf.Lerp(p.z, front, Smooth(.04f, .18f, influence));
        }
    }

    static float G(Vector3 p, Vector3 center, Vector3 width)
    {
        Vector3 d = p - center;
        return Mathf.Exp(-2 * (d.x * d.x / (width.x * width.x) + d.y * d.y / (width.y * width.y) + d.z * d.z / (width.z * width.z)));
    }
    static float Smooth(float start, float end, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(start, end, value));
}

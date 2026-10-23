using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Authoring only. Start with the existing body's silhouette/weights, recover
// the scanned head, then sculpt millimetres of compression, never a bone pose.
public static class IntruderFaceGeometry
{
    public static Mesh Create(Mesh body, Mesh scan)
    {
        ValidatePair(body, scan);
        var result = UnityEngine.Object.Instantiate(body);
        result.name = "IntruderFace / adult volume and compressed lids";
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
        if (!body || !scan || !body.isReadable || !scan.isReadable
            || scan.vertexCount != 12216 || body.vertexCount != scan.vertexCount
            || body.subMeshCount != 2 || scan.subMeshCount != 2
            || body.blendShapeCount != 0 || scan.blendShapeCount != 0
            || body.normals.Length != body.vertexCount || scan.normals.Length != scan.vertexCount
            || body.tangents.Length != body.vertexCount || scan.tangents.Length != scan.vertexCount
            || !body.bindposes.SequenceEqual(scan.bindposes)
            || !body.boneWeights.SequenceEqual(scan.boneWeights)
            || !body.uv.SequenceEqual(scan.uv))
            throw new InvalidOperationException("IntruderFace needs the existing readable Nathan/Embestida pair with identical topology, UV, bindposes and weights.");
        for (int sub = 0; sub < 2; sub++)
            if (!body.GetTriangles(sub).SequenceEqual(scan.GetTriangles(sub)))
                throw new InvalidOperationException("IntruderFace: body topology changed; inspect before applying.");
        if (scan.GetTriangles(1).Distinct().Count() != 454)
            throw new InvalidOperationException("IntruderFace: expected the separate 454-vertex eye submesh.");
    }

    static float Recovery(Vector3 p) => Smooth(1.575f, 1.625f, p.y) * (1 - Smooth(.12f, .16f, Mathf.Abs(p.x)));

    static Vector3 Sculpt(Vector3 p)
    {
        // Fully recovered cheek/jaw/skull volumes remove the former 10-26 mm
        // overlapping dents and 22 mm lip drag. Keep the asymmetry anatomical.
        Vector3 d = Vector3.zero;
        float leftMalar = G(p, new Vector3(-.046f, 1.689f, .099f), new Vector3(.028f, .027f, .030f));
        float rightMalar = G(p, new Vector3(.044f, 1.689f, .098f), new Vector3(.028f, .027f, .030f));
        d.z += leftMalar * .0018f + rightMalar * .0012f;
        float masseter = G(p, new Vector3(-.052f, 1.638f, .060f), new Vector3(.028f, .032f, .038f));
        d.x -= masseter * .0014f;
        d.z += masseter * .0012f;

        // Mouth remains closed/resting in geometry. Only B owns jaw opening.
        // Small unequal corner depressions remove the scan's upward mouth arc;
        // the central lip retains volume instead of stretching into a slash.
        float leftCorner = G(p, new Vector3(-.023f, 1.664f, .111f), new Vector3(.012f, .013f, .019f));
        float rightCorner = G(p, new Vector3(.022f, 1.664f, .112f), new Vector3(.012f, .013f, .019f));
        d.y -= leftCorner * .0024f + rightCorner * .0013f;
        float lowerLip = G(p, new Vector3(-.001f, 1.658f, .122f), new Vector3(.021f, .006f, .014f));
        d.y += lowerLip * .0007f;
        d.z += lowerLip * .0010f;

        // Brow tissue compresses over the original socket; no enlarged eyes.
        float browL = G(p, new Vector3(-.020f, 1.741f, .106f), new Vector3(.022f, .010f, .023f));
        float browR = G(p, new Vector3(.019f, 1.741f, .105f), new Vector3(.022f, .010f, .023f));
        d.y -= browL * .0015f + browR * .0008f;
        d.z += browL * .0012f + browR * .0008f;

        float upperL = G(p, new Vector3(-.0309f, 1.7238f, .106f), new Vector3(.020f, .0038f, .010f));
        float upperR = G(p, new Vector3(.0278f, 1.7240f, .105f), new Vector3(.020f, .0038f, .010f));
        float lowerL = G(p, new Vector3(-.0309f, 1.7133f, .105f), new Vector3(.019f, .004f, .009f));
        float lowerR = G(p, new Vector3(.0278f, 1.7138f, .104f), new Vector3(.019f, .004f, .009f));
        d.y += -.0016f * upperL - .0009f * upperR + .0005f * lowerL + .0007f * lowerR;
        d.z += .0006f * (upperL + upperR + lowerL + lowerR);
        Vector3 q = p + d;
        // Constrain the moved lids to the original globe envelope (measured
        // from its own submesh); closing a lid must not push skin inside an eye.
        SeatLid(ref q, new Vector3(-.030894f, 1.719280f, .087987f), new Vector3(.017418f, .017351f, .017994f), upperL + lowerL);
        SeatLid(ref q, new Vector3(.027798f, 1.719722f, .086991f), new Vector3(.017367f, .017317f, .017997f), upperR + lowerR);
        return q;
    }

    static void SeatLid(ref Vector3 p, Vector3 center, Vector3 radius, float influence)
    {
        if (influence <= .04f || p.z <= center.z) return;
        float x = (p.x - center.x) / radius.x, y = (p.y - center.y) / radius.y;
        float inside = 1 - x * x - y * y;
        if (inside > 0)
        {
            float front = center.z + radius.z * Mathf.Sqrt(inside) + .00025f;
            p.z += Mathf.Max(0, front - p.z) * Smooth(.04f, .18f, influence);
        }
    }

    static float G(Vector3 p, Vector3 center, Vector3 width)
    {
        Vector3 d = p - center;
        return Mathf.Exp(-2 * (d.x * d.x / (width.x * width.x) + d.y * d.y / (width.y * width.y) + d.z * d.z / (width.z * width.z)));
    }
    static float Smooth(float start, float end, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(start, end, value));
}

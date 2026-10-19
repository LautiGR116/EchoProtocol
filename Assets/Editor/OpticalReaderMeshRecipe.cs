using System;
using System.Collections.Generic;
using UnityEngine;

// Original, editor-invoked static geometry recipe. No MonoBehaviour, scene mutation,
// AssetDatabase calls, collider, light, playback or material-state writer.
// Build only for the six real OpticalReader indicators with Finish's cylinder pose.
public static class OpticalReaderMeshRecipe
{
    const int Sides = 48;
    const float LegacyIndicatorOffset = .009f;

    public sealed class Geometry
    {
        // Put only the three fixed meshes under a new visual root with this pose.
        public Vector3 position;
        public Quaternion rotation;
        public Mesh housing, bezel, lens;
        // Assign to the existing activeIndicator MeshFilter. Keep its exact transform.
        public Mesh activeRing;
    }

    public static Geometry BuildForFinishIndicator(Transform indicator, string suffix)
    {
        if (!indicator) throw new ArgumentNullException(nameof(indicator));
        Vector3 scale = indicator.lossyScale;
        if (Vector3.Distance(scale, new Vector3(.17f, .007f, .17f)) > .0001f ||
            Vector3.Dot(indicator.up, Vector3.left) < .9999f)
            throw new ArgumentException("Expected the preserved Finish optical-cylinder pose; inspect this reader before replacing its mesh.");

        var result = new Geometry
        {
            position = indicator.position + Vector3.right * LegacyIndicatorOffset,
            rotation = Quaternion.LookRotation(Vector3.left, Vector3.up),
            housing = RoundedHousing("Optical housing " + suffix),
            // Radius and frontward depth, metres. The ring has a true central bore.
            bezel = Lathe("Optical hollow bezel " + suffix, new[]
            {
                new Vector2(.1255f, -.035f), new Vector2(.1275f, -.033f),
                new Vector2(.1275f, .013f), new Vector2(.123f, .018f),
                new Vector2(.097f, .018f), new Vector2(.092f, .013f),
                new Vector2(.092f, -.010f), new Vector2(.070f, -.014f),
                new Vector2(.070f, -.033f), new Vector2(.089f, -.035f)
            }, false),
            // An opaque, shallow optical cap, not transparent layered glass.
            lens = Lathe("Optical fixed lens " + suffix, new[]
            {
                new Vector2(0f, -.007f), new Vector2(.020f, -.00740f),
                new Vector2(.040f, -.00861f), new Vector2(.060f, -.01062f),
                new Vector2(.074f, -.0125f), new Vector2(.074f, -.016f),
                new Vector2(0f, -.016f)
            }, true)
        };
        result.activeRing = Lathe("Optical recessed active ring " + suffix, new[]
        {
            new Vector2(.085f, -.014f), new Vector2(.084f, -.004f),
            new Vector2(.074f, -.004f), new Vector2(.073f, -.014f)
        }, false);

        // Bake head-local metres into the old indicator's non-uniform mesh space.
        // For a current Finish pose, frontward head Z maps to old local Y.
        Matrix4x4 headToIndicator = indicator.worldToLocalMatrix *
            Matrix4x4.TRS(result.position, result.rotation, Vector3.one);
        TransformMesh(result.activeRing, headToIndicator);
        return result;
    }

    // Optional integration path: keep the old housing/bezel/neutral-lens GameObjects,
    // MeshFilters, transforms and any colliders. Bake a fixed head-space mesh into
    // that existing filter's local coordinates before assigning sharedMesh.
    // Call once per generated mesh; activeRing is ALREADY baked for its indicator.
    public static void BakeFixedMeshToExistingTransform(Mesh fixedMesh, Geometry head,
        Transform preservedFilterTransform)
    {
        if (!fixedMesh || !preservedFilterTransform || head == null)
            throw new ArgumentException("A generated fixed mesh, head and preserved filter transform are required.");
        if (preservedFilterTransform.localToWorldMatrix.determinant <= 0f)
            throw new ArgumentException("Expected a non-mirrored, non-degenerate Finish visual transform.");
        TransformMesh(fixedMesh, preservedFilterTransform.worldToLocalMatrix *
            Matrix4x4.TRS(head.position, head.rotation, Vector3.one));
    }

    static Mesh RoundedHousing(string name)
    {
        // Matches the old box: width .34, height .40, depth .17, centre Z -.12.
        var data = new MeshData();
        Vector3 half = new Vector3(.17f, .20f, .085f);
        const float bevel = .0035f;
        for (int axis = 0; axis < 3; axis++)
        for (int sign = -1; sign <= 1; sign += 2)
        {
            int u = (axis + 1) % 3, v = (axis + 2) % 3;
            float[] us = { -half[u], -half[u] + bevel, half[u] - bevel, half[u] };
            float[] vs = { -half[v], -half[v] + bevel, half[v] - bevel, half[v] };
            for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                int first = data.vertices.Count;
                for (int corner = 0; corner < 4; corner++)
                {
                    int ca = corner == 1 || corner == 2 ? a + 1 : a;
                    int cb = corner >= 2 ? b + 1 : b;
                    Vector3 p = Vector3.zero; p[axis] = sign * half[axis];
                    p[u] = us[ca]; p[v] = vs[cb];
                    Vector3 inner = new Vector3(
                        Mathf.Clamp(p.x, -half.x + bevel, half.x - bevel),
                        Mathf.Clamp(p.y, -half.y + bevel, half.y - bevel),
                        Mathf.Clamp(p.z, -half.z + bevel, half.z - bevel));
                    Vector3 normal = (p - inner).normalized;
                    data.Add(inner + normal * bevel + Vector3.back * .12f, normal,
                        new Vector2(us[ca] / (half[u] * 2f) + .5f,
                            vs[cb] / (half[v] * 2f) + .5f));
                }
                data.Triangle(first, first + 1, first + 2);
                data.Triangle(first, first + 2, first + 3);
            }
        }
        return data.Finish(name);
    }

    static Mesh Lathe(string name, Vector2[] profile, bool smooth)
    {
        // Keep the radial section counterclockwise, so its right normal is outward.
        float area = 0f;
        for (int i = 0; i < profile.Length; i++)
        {
            Vector2 a = profile[i], b = profile[(i + 1) % profile.Length];
            area += a.x * b.y - b.x * a.y;
        }
        if (area < 0f) Array.Reverse(profile);
        var data = new MeshData();
        for (int row = 0; row < profile.Length; row++)
        {
            int next = (row + 1) % profile.Length;
            if (profile[row].x == 0f && profile[next].x == 0f) continue;
            Vector2 segmentNormal = SectionNormal(profile[row], profile[next]);
            int first = data.vertices.Count;
            for (int side = 0; side <= Sides; side++)
            for (int end = 0; end < 2; end++)
            {
                int index = end == 0 ? row : next;
                Vector2 p = profile[index];
                Vector2 n = smooth ? SmoothSectionNormal(profile, index) : segmentNormal;
                float angle = (float)side / Sides * Mathf.PI * 2f;
                float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
                data.Add(new Vector3(p.x * c, p.x * s, p.y),
                    new Vector3(n.x * c, n.x * s, n.y),
                    new Vector2((float)side / Sides, (float)index / (profile.Length - 1)));
            }
            for (int side = 0; side < Sides; side++)
            {
                int a = first + side * 2;
                data.Triangle(a, a + 2, a + 3);
                data.Triangle(a, a + 3, a + 1);
            }
        }
        return data.Finish(name);
    }

    static Vector2 SectionNormal(Vector2 a, Vector2 b)
    {
        Vector2 tangent = b - a;
        return new Vector2(tangent.y, -tangent.x).normalized;
    }

    static Vector2 SmoothSectionNormal(Vector2[] profile, int index)
    {
        int before = (index + profile.Length - 1) % profile.Length;
        int after = (index + 1) % profile.Length;
        Vector2 normal = (SectionNormal(profile[before], profile[index]) +
            SectionNormal(profile[index], profile[after])).normalized;
        if (profile[index].x == 0f) normal = new Vector2(0f, Mathf.Sign(normal.y));
        return normal;
    }

    static void TransformMesh(Mesh mesh, Matrix4x4 transform)
    {
        var vertices = mesh.vertices; var normals = mesh.normals;
        Matrix4x4 normalTransform = transform.inverse.transpose;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.MultiplyPoint3x4(vertices[i]);
            normals[i] = normalTransform.MultiplyVector(normals[i]).normalized;
        }
        mesh.vertices = vertices; mesh.normals = normals;
        mesh.RecalculateBounds(); mesh.RecalculateTangents();
    }

    sealed class MeshData
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Vector3> normals = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        readonly List<int> triangles = new List<int>();
        public void Add(Vector3 p, Vector3 normal, Vector2 texcoord)
        { vertices.Add(p); normals.Add(normal); uv.Add(texcoord); }
        public void Triangle(int a, int b, int c)
        {
            Vector3 cross = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
            if (cross.sqrMagnitude < 1e-15f) return; // Axis poles: omit degenerate half-quads.
            if (Vector3.Dot(cross, normals[a] + normals[b] + normals[c]) < 0f)
            { int swap = b; b = c; c = swap; }
            triangles.Add(a); triangles.Add(b); triangles.Add(c);
        }
        public Mesh Finish(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }
    }
}

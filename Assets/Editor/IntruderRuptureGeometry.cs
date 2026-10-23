using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Editor-only sculpt from installed R5. The actual scan lip boundary, not a
// painted oval, becomes the aperture. No imported source or rig is edited.
public static class IntruderRuptureGeometry
{
    sealed class Edge
    {
        public int a, b, count;
    }

    public static Mesh Create(Mesh r5)
    {
        ValidateSource(r5);
        var original = r5.vertices;
        var rest = new List<Vector3>(); r5.GetUVs(2, rest);
        var triangles = r5.GetTriangles(0);
        int n = r5.vertexCount;
        var parent = Enumerable.Range(0, n).ToArray();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Join(parent, triangles[i], triangles[i + 1]);
            Join(parent, triangles[i], triangles[i + 2]);
        }
        var groups = triangles.Distinct().GroupBy(i => Root(parent, i));
        var oral = new HashSet<int>(); var upperTeeth = new HashSet<int>(); var lowerTeeth = new HashSet<int>();
        foreach (var group in groups)
        {
            var ids = group.ToArray();
            if (!ids.All(i => Mathf.Abs(rest[i].x) < .04f && rest[i].y > 1.62f && rest[i].y < 1.69f && rest[i].z < .118f)) continue;
            foreach (int i in ids) oral.Add(i);
            // Detached upper/lower dentition remains scan textured and rigid.
            // The old closed-mouth bag is replaced with a deep continuous wall.
            if (ids.Min(i => rest[i].y) > 1.646f) foreach (int i in ids) upperTeeth.Add(i);
            else if (ids.Max(i => rest[i].y) < 1.654f) foreach (int i in ids) lowerTeeth.Add(i);
        }
        Require(upperTeeth.Count > 300 && lowerTeeth.Count > 400 && oral.Count > 1200,
            "Expected the separate R5 dentition and closed oral bag.");

        // Weld only for adjacency/normals. UV seams and every original vertex
        // remain in the exported mesh, including their original bone weights.
        var canonical = new int[n]; var positions = new Dictionary<Vector3Int, int>();
        for (int i = 0; i < n; i++)
        {
            var p = rest[i]; var key = new Vector3Int(Mathf.RoundToInt(p.x * 1000000), Mathf.RoundToInt(p.y * 1000000), Mathf.RoundToInt(p.z * 1000000));
            if (!positions.TryGetValue(key, out int j)) positions.Add(key, j = i);
            canonical[i] = j;
        }
        var retained = new List<int>(); var edges = new Dictionary<long, Edge>();
        for (int t = 0; t < triangles.Length; t += 3)
        {
            int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
            if (!oral.Contains(a))
            {
                retained.AddRange(new[] { a, b, c });
                AddEdge(edges, canonical[a], canonical[b]);
                AddEdge(edges, canonical[b], canonical[c]);
                AddEdge(edges, canonical[c], canonical[a]);
            }
            else if (upperTeeth.Contains(a) || lowerTeeth.Contains(a)) retained.AddRange(new[] { a, b, c });
        }
        var rimEdges = edges.Values.Where(e => e.count == 1 && IsMouth(rest[e.a]) && IsMouth(rest[e.b])).ToArray();
        Require(rimEdges.Length >= 20 && rimEdges.Length <= 40, "Expected one recoverable scanned mouth boundary.");
        var rim = OrderedLoop(rimEdges);
        int left = rim.OrderBy(i => rest[i].x).First(), right = rim.OrderByDescending(i => rest[i].x).First();
        // Which lip is upper comes from adjacent skin, not y: closed scan lips
        // overlap by ~1 mm, so splitting them by a horizontal plane tears them.
        var upper = UpperArc(rim, left, right, edges.Values, rest);
        float leftX = rest[left].x, rightX = rest[right].x, midX = (leftX + rightX) * .5f;
        var delta = new Vector3[n]; var baseline = new Vector3[n]; var pinned = new HashSet<int>(rim);
        var neighbours = new Dictionary<int, List<int>>();
        foreach (var e in edges.Values)
        {
            if (!neighbours.TryGetValue(e.a, out var a)) neighbours[e.a] = a = new List<int>(); a.Add(e.b);
            if (!neighbours.TryGetValue(e.b, out var b)) neighbours[e.b] = b = new List<int>(); b.Add(e.a);
        }
        foreach (int i in neighbours.Keys)
        {
            Vector3 p = rest[i];
            float jaw = Smooth(1.545f, 1.603f, p.y) * (1 - Smooth(1.630f, 1.673f, p.y))
                * Smooth(.005f, .060f, p.z) * (1 - Smooth(.045f, .085f, Mathf.Abs(p.x)));
            baseline[i] = delta[i] = new Vector3(-.005f, -.039f, -.002f) * jaw;
            if (p.y <= 1.545f || p.y >= 1.690f || Mathf.Abs(p.x) >= .085f || p.z <= .005f) pinned.Add(i);
        }
        foreach (int i in rim)
        {
            float u = Mathf.InverseLerp(leftX, rightX, rest[i].x) * 2 - 1;
            float arch = Mathf.Sqrt(Mathf.Max(0, 1 - u * u));
            bool top = upper.Contains(i);
            var target = new Vector3(-.013f + (rest[i].x - midX) * .70f - (top ? 0 : .006f * arch),
                1.624f + (top ? .031f : -.034f) * arch, original[i].z);
            delta[i] = target - original[i];
        }
        // Screened surface relaxation transfers the displacement into cheek,
        // chin and neck along skin connectivity; the cranium/orbits stay R5.
        var next = new Vector3[n];
        for (int pass = 0; pass < 120; pass++)
        {
            foreach (var pair in neighbours)
            {
                int i = pair.Key;
                if (pinned.Contains(i)) { next[i] = delta[i]; continue; }
                Vector3 sum = Vector3.zero; float total = 0;
                foreach (int j in pair.Value)
                {
                    float w = 1 / Mathf.Max(.001f, Vector3.Distance(rest[i], rest[j]));
                    sum += delta[j] * w; total += w;
                }
                float anchor = total * .035f;
                next[i] = (sum + baseline[i] * anchor) / (total + anchor);
            }
            var swap = delta; delta = next; next = swap;
        }
        var vertices = original.ToList(); var normals = r5.normals.ToList(); var tangents = r5.tangents.ToList();
        var uv = r5.uv.ToList(); var weights = r5.boneWeights.ToList();
        var oralUV = r5.uv2.Length == n ? r5.uv2.ToList() : Enumerable.Repeat(Vector2.zero, n).ToList();
        for (int i = 0; i < n; i++)
        {
            if (!oral.Contains(i)) vertices[i] += delta[canonical[i]];
            else if (upperTeeth.Contains(i) || lowerTeeth.Contains(i))
            {
                bool top = upperTeeth.Contains(i); var p = original[i];
                p.x = -.013f + (rest[i].x - midX) * .64f - (top ? 0 : .006f);
                p.y += top ? .004f : -.051f;
                p.z -= .003f;
                vertices[i] = p;
            }
        }
        // Actual lip contour determines the wall, with its exact perimeter
        // weights at the join. Five recessed rings form wet lip roll, cheek
        // wall, palate/tongue floor and a closed dark back, not a floating cap.
        if (SignedArea(rim, vertices) < 0) rim.Reverse();
        var inside = new List<int>();
        Vector3 centre = new Vector3(-.016f, 1.621f, .062f);
        float[] scale = { 1, .96f, .91f, .80f, .56f, .20f };
        float[] depth = { 0, .003f, .011f, .026f, .043f, .054f };
        int first = vertices.Count, count = rim.Count;
        for (int ring = 0; ring < scale.Length; ring++)
        for (int k = 0; k < count; k++)
        {
            int source = rim[k]; Vector3 p = vertices[source];
            p.x = Mathf.Lerp(centre.x, p.x, scale[ring]);
            p.y = Mathf.Lerp(centre.y, p.y, scale[ring]);
            p.z -= depth[ring];
            // A broad raised tongue floor gives the lower cavity a physical
            // volume at oblique pump angles, without adding a dangling prop.
            float floor = Mathf.Clamp01((centre.y - p.y) / .025f);
            if (ring > 1) p.y += .004f * floor * Mathf.Sin(ring * Mathf.PI / 5);
            vertices.Add(p); normals.Add(normals[source]); tangents.Add(tangents[source]);
            uv.Add(uv[source]); rest.Add(rest[source]); weights.Add(weights[source]);
            oralUV.Add(new Vector2((float)k / count, (float)ring / (scale.Length - 1)));
        }
        for (int ring = 0; ring < scale.Length - 1; ring++)
        for (int k = 0; k < count; k++)
        {
            int a = first + ring * count + k, b = first + ring * count + (k + 1) % count;
            int c = a + count, d = b + count;
            inside.AddRange(new[] { a, b, c, b, d, c });
        }
        int back = first + (scale.Length - 1) * count;
        for (int k = 1; k < count - 1; k++) inside.AddRange(new[] { back, back + k, back + k + 1 });
        RebuildChangedNormals(vertices, normals, tangents, retained, canonical, original);
        for (int k = 0; k < count; k++) normals[first + k] = normals[rim[k]];
        RebuildInteriorNormals(vertices, normals, tangents, inside, first, count);
        var result = UnityEngine.Object.Instantiate(r5);
        result.name = "Intruder rupture / forced narrow aperture";
        result.vertices = vertices.ToArray(); result.normals = normals.ToArray(); result.tangents = tangents.ToArray();
        result.uv = uv.ToArray(); result.SetUVs(1, oralUV); result.SetUVs(2, rest);
        result.boneWeights = weights.ToArray(); result.subMeshCount = 3;
        result.SetTriangles(retained, 0); result.SetTriangles(r5.GetTriangles(1), 1); result.SetTriangles(inside, 2);
        result.RecalculateBounds();
        ValidateResult(r5, result);
        return result;
    }

    public static void ValidateSource(Mesh mesh)
    {
        var rest = new List<Vector3>(); if (mesh) mesh.GetUVs(2, rest);
        Require(mesh && mesh.vertexCount == 12216 && mesh.subMeshCount == 2 && mesh.blendShapeCount == 0
            && mesh.normals.Length == mesh.vertexCount && mesh.tangents.Length == mesh.vertexCount
            && mesh.uv.Length == mesh.vertexCount && rest.Count == mesh.vertexCount
            && mesh.boneWeights.Length == mesh.vertexCount, "Rupture needs the installed R5 Nathan streams, not an imported scan or another actor.");
    }
    public static void ValidateResult(Mesh source, Mesh result)
    {
        ValidateSource(source); int n = source.vertexCount;
        Require(result && result.subMeshCount == 3 && result.vertexCount > n && result.vertexCount < n + 300
            && result.bindposes.SequenceEqual(source.bindposes) && result.GetTriangles(1).SequenceEqual(source.GetTriangles(1))
            && result.boneWeights.Take(n).SequenceEqual(source.boneWeights) && result.uv.Take(n).SequenceEqual(source.uv)
            && result.GetTriangles(2).Length > 600, "Rupture lost original UVs/weights/eyes or its connected cavity.");
        var a = source.vertices; var b = result.vertices; var rest = new List<Vector3>(); source.GetUVs(2, rest);
        for (int i = 0; i < b.Length; i++) Require(Finite(b[i].x) && Finite(b[i].y) && Finite(b[i].z), "Non-finite rupture geometry.");
        for (int i = 0; i < n; i++)
            if (rest[i].y <= 1.545f || rest[i].y >= 1.690f || Mathf.Abs(rest[i].x) >= .085f || rest[i].z <= .005f)
                Require((a[i] - b[i]).sqrMagnitude < 1e-12f, "Rupture affected the body, cranium or orbit outside its lower-face cage.");
    }
    static bool IsMouth(Vector3 p) => Mathf.Abs(p.x) < .04f && p.y > 1.64f && p.y < 1.66f && p.z > .09f;
    static void AddEdge(Dictionary<long, Edge> edges, int a, int b)
    {
        long key = ((long)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
        if (!edges.TryGetValue(key, out var e)) edges[key] = e = new Edge { a = a, b = b };
        e.count++;
    }
    static List<int> OrderedLoop(Edge[] edges)
    {
        var links = new Dictionary<int, List<int>>();
        foreach (var e in edges)
        {
            if (!links.TryGetValue(e.a, out var a)) links[e.a] = a = new List<int>(); a.Add(e.b);
            if (!links.TryGetValue(e.b, out var b)) links[e.b] = b = new List<int>(); b.Add(e.a);
        }
        Require(links.All(p => p.Value.Count == 2), "Mouth boundary is not a single closed loop.");
        int first = links.Keys.Min(), previous = -1, current = first; var loop = new List<int>();
        do
        {
            loop.Add(current); var link = links[current]; int next = link[0] == previous ? link[1] : link[0];
            previous = current; current = next;
            Require(loop.Count <= edges.Length, "Mouth boundary did not close.");
        } while (current != first);
        Require(loop.Count == edges.Length, "More than one oral boundary was found."); return loop;
    }
    static HashSet<int> UpperArc(List<int> loop, int left, int right, IEnumerable<Edge> edges, List<Vector3> rest)
    {
        var arc = new HashSet<int>(); int index = loop.IndexOf(left);
        while (loop[index] != right) { arc.Add(loop[index]); index = (index + 1) % loop.Count; } arc.Add(right);
        var other = new HashSet<int>(loop.Where(i => !arc.Contains(i) || i == left || i == right));
        var graph = new Dictionary<int, List<int>>();
        foreach (var e in edges)
        {
            if (!graph.TryGetValue(e.a, out var a)) graph[e.a] = a = new List<int>(); a.Add(e.b);
            if (!graph.TryGetValue(e.b, out var b)) graph[e.b] = b = new List<int>(); b.Add(e.a);
        }
        // Follow several skin rings past the folded vermilion. The first
        // upper-lip return actually points down in this closed scan, so a
        // one-triangle sign test would swap upper and lower and cross the lips.
        float mid = (rest[left].x + rest[right].x) * .5f;
        float Score(HashSet<int> side)
        {
            var seeds = new HashSet<int>(side.Where(i => Mathf.Abs(rest[i].x - mid) < .013f));
            var seen = new HashSet<int>(loop); var front = seeds;
            for (int depth = 0; depth < 4; depth++)
            {
                var next = new HashSet<int>();
                foreach (int i in front) foreach (int j in graph[i]) if (seen.Add(j)) next.Add(j);
                front = next;
            }
            Require(front.Count > 0, "Cannot identify scanned upper/lower lip skin.");
            return front.Average(i => rest[i].y) - seeds.Average(i => rest[i].y);
        }
        return Score(arc) > Score(other) ? arc : other;
    }
    static void RebuildChangedNormals(List<Vector3> p, List<Vector3> normals, List<Vector4> tangents, List<int> triangles, int[] canonical, Vector3[] original)
    {
        var sum = new Vector3[original.Length];
        for (int t = 0; t < triangles.Count; t += 3)
        {
            int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
            Vector3 n = Vector3.Cross(p[b] - p[a], p[c] - p[a]);
            sum[canonical[a]] += n; sum[canonical[b]] += n; sum[canonical[c]] += n;
        }
        for (int i = 0; i < original.Length; i++)
            if ((p[i] - original[i]).sqrMagnitude > 1e-12f && sum[canonical[i]].sqrMagnitude > 1e-14f)
                SetNormal(i, sum[canonical[i]].normalized, normals, tangents);
    }
    static void RebuildInteriorNormals(List<Vector3> p, List<Vector3> normals, List<Vector4> tangents, List<int> triangles, int first, int count)
    {
        var sum = new Vector3[p.Count - first];
        for (int t = 0; t < triangles.Count; t += 3)
        {
            int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
            Vector3 n = Vector3.Cross(p[b] - p[a], p[c] - p[a]);
            sum[a - first] += n; sum[b - first] += n; sum[c - first] += n;
        }
        for (int i = first; i < p.Count; i++)
            SetNormal(i, i < first + count ? normals[i] : sum[i - first].normalized, normals, tangents);
    }
    static void SetNormal(int i, Vector3 n, List<Vector3> normals, List<Vector4> tangents)
    {
        Vector3 t = (Vector3)tangents[i]; t = (t - n * Vector3.Dot(t, n)).normalized;
        normals[i] = n; tangents[i] = new Vector4(t.x, t.y, t.z, tangents[i].w);
    }
    static float SignedArea(List<int> loop, List<Vector3> p)
    { float sum = 0; for (int i = 0; i < loop.Count; i++) { var a = p[loop[i]]; var b = p[loop[(i + 1) % loop.Count]]; sum += a.x * b.y - b.x * a.y; } return sum; }
    static int Root(int[] p, int i) { while (p[i] != i) { p[i] = p[p[i]]; i = p[i]; } return i; }
    static void Join(int[] p, int a, int b) => p[Root(p, b)] = Root(p, a);
    static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
    static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}

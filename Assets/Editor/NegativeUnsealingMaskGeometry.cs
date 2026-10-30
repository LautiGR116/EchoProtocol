using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Fresh rigid buffers in Head-local space. The source V2 bind matrix is read
// only; its generated meshes and all licensed character buffers stay intact.
public static class NegativeUnsealingMaskGeometry
{
    sealed class Builder
    {
        readonly List<Vector3> points = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        readonly List<int> indices = new List<int>();
        readonly Matrix4x4 toHead;
        readonly Vector3 pivot;
        public Builder(Matrix4x4 matrix, Vector3 localPivot) { toHead = matrix; pivot = localPivot; }
        public void Grid(int rows, int columns, Func<float,float,Vector3> shape, bool reverse = false)
        {
            int first = points.Count;
            for (int y = 0; y <= rows; y++) for (int x = 0; x <= columns; x++)
            {
                float u = x / (float)columns, v = y / (float)rows;
                Vector3 p = toHead.MultiplyPoint3x4(shape(u,v)) - pivot;
                Require(Finite(p), "Invalid generated mask position.");
                points.Add(p); uv.Add(new Vector2(u,v));
            }
            for (int y = 0; y < rows; y++) for (int x = 0; x < columns; x++)
            {
                int a = first + y * (columns + 1) + x, b = a + columns + 1;
                if (reverse) indices.AddRange(new[] { a,b,a+1,a+1,b,b+1 });
                else indices.AddRange(new[] { a,a+1,b,a+1,b+1,b });
            }
        }
        public void Tube(Vector3 a, Vector3 b, float radius)
        {
            Vector3 axis = (b-a).normalized;
            Vector3 side = Vector3.Cross(axis, Vector3.forward).normalized;
            if (side.sqrMagnitude < .1f) side = Vector3.Cross(axis, Vector3.up).normalized;
            Vector3 other = Vector3.Cross(axis, side).normalized;
            Grid(3,7,(u,v) => Vector3.Lerp(a,b,v) + (side * Mathf.Cos(u*Mathf.PI*2) + other * Mathf.Sin(u*Mathf.PI*2)) * radius, true);
        }
        public Mesh Finish(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(points); mesh.SetUVs(0,uv); mesh.SetTriangles(indices,0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            Validate(mesh); return mesh;
        }
    }

    static Vector3 Face(float u, float v)
    {
        // Identical closed silhouette to the validated Negative V2 shell.
        float x = u*2-1;
        float profile = Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((v-.51f)/.53f,2)));
        float width = .024f+.104f*profile;
        float seam = .007f*Mathf.Exp(-Mathf.Pow((x+.28f)*13,2))*Mathf.Sin(v*Mathf.PI);
        return new Vector3(-.018f+x*width-.012f*(1-v),1.515f+v*.405f+x*.012f,
            .178f+.061f*(1-x*x)*Mathf.Pow(Mathf.Max(0,Mathf.Sin(v*Mathf.PI)),.4f)-seam);
    }
    // Irregular vertical break, not an eye/mouth graphic or a flat center cut.
    static float Split(float v) => .495f+.028f*Mathf.Sin(v*15)+.012f*Mathf.Sin(v*51);
    public static Vector3 Pivot(Matrix4x4 toHead, bool left) => toHead.MultiplyPoint3x4(Face(left?0:1,.51f)-Vector3.forward*.013f);

    public static Mesh Plate(Matrix4x4 toHead, bool left)
    {
        var b = new Builder(toHead,Pivot(toHead,left));
        Vector3 Surface(float u,float v) => Face(left?Mathf.Lerp(0,Split(v),u):Mathf.Lerp(Split(v),1,u),v);
        b.Grid(40,20,Surface);
        b.Grid(40,20,(u,v)=>Surface(u,v)-Vector3.forward*.016f,true);
        foreach (float edge in new[] {0f,1f})
            b.Grid(40,1,(u,v)=>Surface(edge,v)-Vector3.forward*(u*.016f),edge==1);
        foreach (float edge in new[] {0f,1f})
            b.Grid(1,20,(u,v)=>Surface(u,edge)-Vector3.forward*(v*.016f),edge==0);
        return b.Finish(left?"Unsealing / closed left ceramic fracture":"Unsealing / closed right ceramic fracture");
    }

    public static Mesh Cavity(Matrix4x4 toHead)
    {
        var b = new Builder(toHead,Vector3.zero);
        b.Grid(18,40,(u,v)=>
        {
            float a=u*Mathf.PI*2;
            float radius=.02f+.98f*v;
            return new Vector3(-.023f+Mathf.Cos(a)*.103f*radius,
                1.72f+Mathf.Sin(a)*.179f*radius,.17f+.037f*Mathf.Pow(radius,2));
        },true);
        return b.Finish("Unsealing / recessed opaque mask interior");
    }
    public static Mesh Fibers(Matrix4x4 toHead)
    {
        var b=new Builder(toHead,Vector3.zero);
        for(int i=0;i<7;i++)
        {
            float y=1.59f+i*.038f;
            float width=.037f+.013f*Mathf.Sin(i*.8f);
            var a=new Vector3(-.023f-width,y,.207f);
            var end=new Vector3(-.023f+width,y+.012f*Mathf.Sin(i*1.7f),.205f);
            var mid=Vector3.Lerp(a,end,.5f)+new Vector3(0,-.008f,.004f);
            b.Tube(a,mid,.0018f+(i%3)*.00035f);b.Tube(mid,end,.0018f+(i%3)*.00035f);
        }
        return b.Finish("Unsealing / taut interior fibers");
    }

    public static void Validate(Mesh mesh)
    {
        Require(mesh && mesh.vertexCount>0,"Missing rigid mask mesh.");
        Require(mesh.vertices.All(Finite),mesh.name+": non-finite vertices.");
        Require(mesh.normals.Length==mesh.vertexCount && mesh.normals.All(Finite),mesh.name+": invalid normals.");
        Require(mesh.tangents.Length==mesh.vertexCount && mesh.tangents.All(v=>Finite(v.x)&&Finite(v.y)&&Finite(v.z)&&Finite(v.w)),mesh.name+": invalid tangents.");
        Require(mesh.uv.Length==mesh.vertexCount && mesh.uv.All(v=>Finite(v.x)&&Finite(v.y)),mesh.name+": invalid UVs.");
        Require(Finite(mesh.bounds.center)&&Finite(mesh.bounds.extents),mesh.name+": invalid bounds.");
        Require(mesh.triangles.All(i=>i>=0&&i<mesh.vertexCount),mesh.name+": invalid indices.");
    }
    static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
    static bool Finite(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z);
    static void Require(bool condition,string message) { if(!condition)throw new InvalidOperationException(message); }
}

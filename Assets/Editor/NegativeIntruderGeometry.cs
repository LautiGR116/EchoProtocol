using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Original deterministic topology in Nathan's bind space. The generated meshes
// copy its licensed bind matrices and therefore stay in the private art folder.
public static class NegativeIntruderGeometry
{
    public sealed class Rig
    {
        public Matrix4x4[] binds;
        public Transform[] bones;
        public int hip, spine, chest, neck, head, la, le, lh, ra, re, rh, lt, rt;
        public Vector3 Point(int bone)=>binds[bone].inverse.MultiplyPoint3x4(Vector3.zero);
        public int Named(string suffix)=>Array.FindIndex(bones,b=>b.name.EndsWith(suffix,StringComparison.Ordinal));
    }
    sealed class Sculpt
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<BoneWeight> weights=new List<BoneWeight>();
        readonly List<int> triangles=new List<int>();
        public int Count=>vertices.Count;
        public void Grid(int rows,int columns,Func<float,float,Vector3> shape,Func<Vector3,BoneWeight> skin,bool reverse=false)
        {
            int first=vertices.Count;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float u=x/(float)columns,v=y/(float)rows;Vector3 p=shape(u,v);
                vertices.Add(p);uv.Add(new Vector2(u,v));weights.Add(skin(p));
            }
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {
                int a=first+y*(columns+1)+x,b=a+columns+1;
                if(reverse)triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                else triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});
            }
        }
        public Mesh Finish(string name,Matrix4x4[] binds)
        {
            // Validate before handing data to native Mesh/skin buffers. In
            // particular sin(PI) can be slightly negative in float arithmetic.
            Require(vertices.All(Finite)&&uv.All(p=>Finite(p.x)&&Finite(p.y)),name+": invalid generated positions/UVs.");
            ValidateSkin(weights,binds,name);
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);
            mesh.SetTriangles(triangles,0);mesh.boneWeights=weights.ToArray();mesh.bindposes=binds;
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();Validate(mesh);return mesh;
        }
    }
    static BoneWeight One(int bone)=>new BoneWeight{boneIndex0=bone,weight0=1};
    static BoneWeight Blend(int a,int b,float weight)=>new BoneWeight{boneIndex0=a,boneIndex1=b,weight0=1-Mathf.Clamp01(weight),weight1=Mathf.Clamp01(weight)};
    static float Smooth(float a,float b,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,value));

    public static Mesh HandsAndBoots(Mesh source)
    {
        Validate(source);
        var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Negative / retained hands and continuous legs under coat";
        var rest=new List<Vector3>();source.GetUVs(2,rest);
        var vertices=rest.Count==source.vertexCount?rest.ToArray():source.vertices;
        bool Keep(int i)=>vertices[i].y<.99f||(Mathf.Abs(vertices[i].x)>.695f&&vertices[i].y>1.25f&&vertices[i].y<1.64f);
        for(int s=0;s<mesh.subMeshCount;s++)
        {
            var old=source.GetTriangles(s);var kept=new List<int>();
            for(int t=0;t<old.Length;t+=3)
                if(Keep(old[t])&&Keep(old[t+1])&&Keep(old[t+2]))kept.AddRange(new[]{old[t],old[t+1],old[t+2]});
            mesh.SetTriangles(kept,s);
        }
        // Vertices/bone weights stay untouched; only hidden triangles disappear.
        // The face, eyes, scalp and shirt cannot leak. Complete legs stay under
        // the split coat so stepping cannot expose a gap above the grounded boot.
        mesh.bounds=source.bounds;Validate(mesh);return mesh;
    }

    public static Mesh Mask(Rig r)
    {
        var b=new Sculpt();
        Vector3 Face(float u,float v)
        {
            float x=u*2-1;
            float profile=Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((v-.51f)/.53f,2)));
            float width=.024f+.104f*profile;
            // A tall off-axis cast shell, with one shallow mould seam. No eye
            // sockets, painted face, mouth hole, or recognizable human features.
            float seam=.007f*Mathf.Exp(-Mathf.Pow((x+.28f)*13,2))*Mathf.Sin(v*Mathf.PI);
            return new Vector3(-.018f+x*width-.012f*(1-v),1.515f+v*.405f+x*.012f,
                .178f+.061f*(1-x*x)*Mathf.Pow(Mathf.Max(0,Mathf.Sin(v*Mathf.PI)),.4f)-seam);
        }
        b.Grid(30,32,Face,p=>One(r.head));
        b.Grid(30,32,(u,v)=>Face(u,v)-Vector3.forward*.016f,p=>One(r.head),true);
        // Closed side thickness and top/bottom rims are actual geometry.
        foreach(float edge in new[]{0f,1f})
            b.Grid(30,1,(u,v)=>Face(edge,v)-Vector3.forward*(u*.016f),p=>One(r.head),edge==1);
        foreach(float edge in new[]{0f,1f})
            b.Grid(1,32,(u,v)=>Face(u,edge)-Vector3.forward*(v*.016f),p=>One(r.head),edge==0);
        return b.Finish("Negative / solid asymmetric ivory shell",r.binds);
    }

    public static Mesh Hood(Rig r)
    {
        var b=new Sculpt();
        float[] ys={1.42f,1.50f,1.61f,1.75f,1.88f,1.955f};
        float[] widths={.235f,.215f,.178f,.185f,.158f,.055f};
        float[] depths={.184f,.194f,.167f,.18f,.155f,.060f};
        Vector3 Ring(float u,float v)
        {
            float k=v*(ys.Length-1);int i=Mathf.Min(ys.Length-2,Mathf.FloorToInt(k));float t=k-i;
            float a=u*Mathf.PI*2,y=Mathf.Lerp(ys[i],ys[i+1],t);
            float fold=.004f*Mathf.Cos(a*7+.3f)*Mathf.Sin(v*Mathf.PI);
            return new Vector3(-.018f+Mathf.Cos(a)*(Mathf.Lerp(widths[i],widths[i+1],t)+fold),
                y+.018f*Mathf.Cos(a)*Smooth(1.65f,1.9f,y),
                -.026f+Mathf.Sin(a)*(Mathf.Lerp(depths[i],depths[i+1],t)+fold));
        }
        b.Grid(35,48,Ring,p=>Blend(r.chest,r.head,Smooth(1.47f,1.64f,p.y)),true);
        b.Grid(1,48,(u,v)=>Vector3.Lerp(Ring(u,1),new Vector3(-.018f,1.959f,-.026f),v),p=>One(r.head),true);
        return b.Finish("Negative / closed cowl with weighted neck",r.binds);
    }

    public static Mesh Coat(Rig r)
    {
        var b=new Sculpt();
        float[] ys={.93f,1.04f,1.19f,1.36f,1.48f,1.535f};
        float[] widths={.235f,.224f,.242f,.286f,.309f,.163f};
        float[] depths={.178f,.177f,.188f,.192f,.18f,.135f};
        BoneWeight Trunk(Vector3 p)=>p.y<1.22f?Blend(r.hip,r.spine,Smooth(.94f,1.22f,p.y)):Blend(r.spine,r.chest,Smooth(1.22f,1.44f,p.y));
        b.Grid(28,64,(u,v)=>
        {
            float k=v*(ys.Length-1);int i=Mathf.Min(ys.Length-2,Mathf.FloorToInt(k));float t=k-i;
            float a=u*Mathf.PI*2,y=Mathf.Lerp(ys[i],ys[i+1],t);
            // Millimetre folds carried by complete cross sections. Never the
            // old unsupported planar bib with centimetre ridges at the waist.
            float fold=.004f*Mathf.Cos(a*9+.4f)*Mathf.Sin(v*Mathf.PI);
            return new Vector3(Mathf.Cos(a)*(Mathf.Lerp(widths[i],widths[i+1],t)+fold),
                y+.018f*Mathf.Max(0,-Mathf.Cos(a))*Smooth(1.3f,1.48f,y),
                -.015f+Mathf.Sin(a)*(Mathf.Lerp(depths[i],depths[i+1],t)+fold));
        },Trunk,true);
        // Two substantial curved tails articulate from the hip into their own
        // thigh. Open front/back seams let the stride pass without rubber folds.
        foreach(int side in new[]{-1,1})
        {
            int thigh=side<0?r.lt:r.rt;
            b.Grid(24,32,(u,v)=>
            {
                float a=-Mathf.PI*.5f+u*Mathf.PI;
                float split=Smooth(.15f,.9f,v)*.022f;
                float width=Mathf.Lerp(.231f,.277f,v);
                float depth=Mathf.Lerp(.179f,.212f,v);
                float fold=.004f*Mathf.Sin(u*Mathf.PI*6+.4f)*Mathf.Sin(v*Mathf.PI);
                return new Vector3(side*(Mathf.Cos(a)*(width+fold)+split),
                    Mathf.Lerp(.99f,side<0?.34f:.40f,v)+.014f*Mathf.Sin(u*Mathf.PI),
                    -.015f+Mathf.Sin(a)*(depth+fold));
            },p=>Blend(r.hip,thigh,Smooth(.99f,.40f,p.y)*.68f),side<0);
        }
        Sleeve(b,r,r.la,r.le,r.lh);Sleeve(b,r,r.ra,r.re,r.rh);
        return b.Finish("Negative / continuous coat, articulated split tails and sleeves",r.binds);
    }

    static void Sleeve(Sculpt b,Rig r,int shoulder,int elbow,int wrist)
    {
        Vector3 a=r.Point(shoulder),e=r.Point(elbow),h=r.Point(wrist);
        Vector3 forward=(h-a).normalized;
        Vector3 up=Vector3.ProjectOnPlane(Vector3.up,forward).normalized;
        Vector3 side=Vector3.Cross(forward,up).normalized;
        b.Grid(24,24,(u,v)=>
        {
            float t=v*2;Vector3 centre=t<1?Vector3.Lerp(a,e,t):Vector3.Lerp(e,h,t-1);
            float radius=t<1?Mathf.Lerp(.112f,.079f,t):Mathf.Lerp(.079f,.053f,t-1);
            float angle=u*Mathf.PI*2;
            float fold=.003f*Mathf.Sin(angle*5+v*4)*Mathf.Sin(v*Mathf.PI);
            return centre+(up*Mathf.Cos(angle)+side*Mathf.Sin(angle))*(radius+fold);
        },p=>
        {
            float along=Vector3.Dot(p-a,forward)/Mathf.Max(.1f,Vector3.Distance(a,h));
            return along<.5f?Blend(shoulder,elbow,Smooth(.32f,.57f,along)):Blend(elbow,wrist,Smooth(.85f,1,along));
        });
    }

    public static Mesh Fingers(Rig r)
    {
        var b=new Sculpt();
        foreach(string side in new[]{"l","r"})foreach(string finger in new[]{"index","middle","ring","pinky"})
        {
            int end=r.Named("_"+finger+"_03_"+side),previous=r.Named("_"+finger+"_02_"+side);
            if(end<0||previous<0)continue;
            Vector3 a=r.Point(end),direction=(a-r.Point(previous)).normalized;
            Vector3 up=Vector3.ProjectOnPlane(Vector3.up,direction).normalized;
            if(up.sqrMagnitude<.01f)up=Vector3.ProjectOnPlane(Vector3.forward,direction).normalized;
            Vector3 across=Vector3.Cross(direction,up).normalized;
            float length=finger=="middle"?.058f:finger=="pinky"?.040f:.049f;
            b.Grid(8,16,(u,v)=>
            {
                float angle=u*Mathf.PI*2,radius=Mathf.Lerp(.009f,.004f,v);
                if(v>.84f)radius*=Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((v-.84f)/.16f,2)));
                return a+direction*(v*length-.006f)+up*(.004f*Mathf.Sin(v*Mathf.PI))
                    +(up*Mathf.Cos(angle)+across*Mathf.Sin(angle))*radius;
            },p=>One(end));
        }
        return b.Finish("Negative / eight blunt articulated ivory fingertip sleeves",r.binds);
    }

    public static void Validate(Mesh mesh)
    {
        Require(mesh,"Missing Negative mesh.");
        Require(mesh.vertexCount>0&&mesh.vertices.All(Finite),mesh.name+": invalid vertices.");
        Require(mesh.normals.Length==mesh.vertexCount&&mesh.normals.All(Finite),mesh.name+": invalid normals.");
        Require(mesh.tangents.Length==mesh.vertexCount&&mesh.tangents.All(Finite),mesh.name+": invalid tangents.");
        Require(mesh.uv.Length==mesh.vertexCount&&mesh.uv.All(v=>Finite(v.x)&&Finite(v.y)),mesh.name+": invalid UVs.");
        Require(Finite(mesh.bounds.center)&&Finite(mesh.bounds.extents),mesh.name+": invalid bounds.");
        Require(mesh.boneWeights.Length==mesh.vertexCount,mesh.name+": incomplete skin weights.");
        ValidateSkin(mesh.boneWeights,mesh.bindposes,mesh.name);
        for(int s=0;s<mesh.subMeshCount;s++)
            Require(mesh.GetIndices(s).All(i=>i>=0&&i<mesh.vertexCount),mesh.name+": invalid indices.");
    }
    static void ValidateSkin(IEnumerable<BoneWeight> weights,Matrix4x4[] binds,string name)
    {
        Require(binds!=null&&binds.Length>0,name+": missing bind matrices.");
        foreach(var matrix in binds)
        {
            for(int i=0;i<16;i++)Require(Finite(matrix[i]),name+": invalid bind matrix.");
            Require(Finite(matrix.determinant)&&Mathf.Abs(matrix.determinant)>.000001f,name+": singular bind matrix.");
        }
        foreach(var w in weights)
        {
            var values=new[]{w.weight0,w.weight1,w.weight2,w.weight3};
            var indices=new[]{w.boneIndex0,w.boneIndex1,w.boneIndex2,w.boneIndex3};
            Require(values.All(v=>Finite(v)&&v>=0)&&Mathf.Abs(values.Sum()-1)<.002f,name+": invalid skin weight.");
            for(int i=0;i<4;i++)if(values[i]>0)Require(indices[i]>=0&&indices[i]<binds.Length,name+": invalid skin index.");
        }
    }
    static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    static bool Finite(Vector3 value)=>Finite(value.x)&&Finite(value.y)&&Finite(value.z);
    static bool Finite(Vector4 value)=>Finite(value.x)&&Finite(value.y)&&Finite(value.z)&&Finite(value.w);
    static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}

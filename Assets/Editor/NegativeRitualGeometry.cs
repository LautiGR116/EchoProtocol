using System;
using System.Collections.Generic;
using UnityEngine;

// Original geometric studies, with no scanned or licensed character data.
// All dimensions are adult scale. Surfaces are opaque cast/fabric reliefs.
public static class NegativeRitualGeometry
{
    sealed class Builder
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector2> uv = new List<Vector2>();
        public readonly List<int> indices = new List<int>();
        public void Grid(int rows, int columns, Func<float,float,Vector3> shape, bool reverse=false)
        {
            int first=vertices.Count;
            for(int y=0;y<=rows;y++) for(int x=0;x<=columns;x++)
            {
                float u=x/(float)columns,v=y/(float)rows;var p=shape(u,v);
                RequireFinite(p,"generated vertex");vertices.Add(p);uv.Add(new Vector2(u,v));
            }
            for(int y=0;y<rows;y++) for(int x=0;x<columns;x++)
            {
                int a=first+y*(columns+1)+x,b=a+columns+1;
                if(reverse) indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                else indices.AddRange(new[]{a,a+1,b,a+1,b+1,b});
            }
        }
        public void Tube(Vector3 a,Vector3 b,float radius,int sides=6)
        {
            Vector3 axis=(b-a).normalized;
            if((b-a).sqrMagnitude<.0000001f) return;
            Vector3 side=Vector3.Cross(axis,Mathf.Abs(Vector3.Dot(axis,Vector3.up))>.95f?Vector3.right:Vector3.up).normalized;
            Vector3 other=Vector3.Cross(axis,side).normalized;
            Grid(1,sides,(u,v)=>Vector3.Lerp(a,b,v)+radius*(side*Mathf.Cos(u*Mathf.PI*2)+other*Mathf.Sin(u*Mathf.PI*2)),true);
        }
        public void Ellipsoid(Vector3 center,Vector3 radius,int rows=10,int columns=16)
        {
            Grid(rows,columns,(u,v)=>
            {
                float lat=Mathf.PI*v,lon=Mathf.PI*2*u;
                return center+Vector3.Scale(radius,new Vector3(Mathf.Sin(lat)*Mathf.Cos(lon),Mathf.Cos(lat),Mathf.Sin(lat)*Mathf.Sin(lon)));
            });
        }
        public Mesh Finish(string name)
        {
            var mesh=new Mesh{name=name};
            if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();Validate(mesh);return mesh;
        }
    }

    static float G(float x,float y,float cx,float cy,float sx,float sy)
    {float dx=(x-cx)/sx,dy=(y-cy)/sy;return Mathf.Exp(-(dx*dx+dy*dy)*.5f);}
    static float Capsule(float x,float y,Vector2 a,Vector2 b,float width)
    {
        var p=new Vector2(x,y);var d=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
        float distance=(p-Vector2.Lerp(a,b,t)).magnitude/width;return Mathf.Exp(-distance*distance*.5f);
    }
    public static float ReliefDepth(float x,float y,int variant)
    {
        float shift=variant==1?.13f:variant==2?-.11f:0;
        float edge=Mathf.SmoothStep(0,1,Mathf.Clamp01((.83f-Mathf.Abs(x))/.16f))
            *Mathf.SmoothStep(0,1,Mathf.Clamp01((1.28f-Mathf.Abs(y))/.16f));
        float head=.24f*G(x,y,-.07f+shift,.66f,.17f,.23f);
        float face=.08f*G(x,y,-.07f+shift,.64f,.065f,.14f);
        float torso=.22f*G(x,y,shift,-.02f,.31f,.42f)+.09f*G(x,y,shift,-.52f,.24f,.2f);
        float clavicle=.07f*Capsule(x,y,new Vector2(-.29f+shift,.3f),new Vector2(.28f+shift,.29f),.07f);
        float ribs=.025f*Mathf.Pow(Mathf.Max(0,Mathf.Cos((y+.05f)*43)),2)*G(x,y,shift,-.12f,.3f,.26f);
        float arms=.10f*Capsule(x,y,new Vector2(-.3f+shift,.26f),new Vector2(-.53f,.43f),.09f)
            +.11f*Capsule(x,y,new Vector2(.3f+shift,.25f),new Vector2(.55f,-.05f),.095f);
        float palms=.16f*G(x,y,-.52f,.53f,.105f,.12f)+.15f*G(x,y,.55f,.08f,.105f,.12f);
        float digits=0;
        for(int i=0;i<4;i++)
        {
            float len=.19f+.055f*Mathf.Sin((i+1)*.7f);
            digits+=.12f*Capsule(x,y,new Vector2(-.63f+i*.063f,.59f),new Vector2(-.65f+i*.067f,.59f+len),.022f);
            digits+=.115f*Capsule(x,y,new Vector2(.45f+i*.063f,.15f),new Vector2(.43f+i*.067f,.15f+len),.022f);
        }
        digits+=.1f*Capsule(x,y,new Vector2(-.45f,.51f),new Vector2(-.33f,.62f),.032f);
        digits+=.1f*Capsule(x,y,new Vector2(.48f,.04f),new Vector2(.35f,.11f),.032f);
        float taut=.006f*Mathf.Sin(x*42+y*16)*Mathf.Sin(y*23-1)*edge;
        // Keep the fully expressed profile below 28 cm so it never reaches
        // the nearby service route, even at runtime depth scale 1.
        return .006f+.62f*edge*(head+face+torso+clavicle+ribs+arms+palms+digits+taut);
    }
    public static Mesh Membrane(int variant)
    {
        var b=new Builder();
        b.Grid(112,80,(u,v)=>{float x=(u-.5f)*1.66f,y=(v-.5f)*2.56f;return new Vector3(x,y,ReliefDepth(x,y,variant));});
        return b.Finish("Ritual / adult impression " + variant);
    }
    public static Mesh MembraneStitches(int variant)
    {
        var b=new Builder();
        for(int i=0;i<22;i++)
        {
            float y=-1.12f+i*.102f,x=.05f+.018f*Mathf.Sin(i*.7f);
            Vector3 a=new Vector3(x-.035f,y-.02f,ReliefDepth(x-.035f,y-.02f,variant)+.009f);
            Vector3 c=new Vector3(x+.035f,y+.02f,ReliefDepth(x+.035f,y+.02f,variant)+.009f);
            b.Tube(a,c,.006f,5);
        }
        return b.Finish("Ritual / sutured cast seam " + variant);
    }
    static readonly float[] BodyY={0,.16f,.46f,.85f,1.15f,1.47f,1.7f,1.82f,1.92f,2.11f,2.25f};
    static readonly float[] BodyW={.14f,.18f,.24f,.28f,.32f,.37f,.39f,.26f,.15f,.16f,.09f};
    static readonly float[] BodyD={.13f,.16f,.19f,.22f,.24f,.25f,.23f,.18f,.15f,.17f,.095f};
    static Vector3 CocoonPoint(float u,float v)
    {
        float y=v*2.25f;int i=0;while(i<BodyY.Length-2&&y>BodyY[i+1])i++;
        float t=Mathf.InverseLerp(BodyY[i],BodyY[i+1],y);float angle=u*Mathf.PI*2;
        float w=Mathf.Lerp(BodyW[i],BodyW[i+1],t),d=Mathf.Lerp(BodyD[i],BodyD[i+1],t);
        float fold=.008f*Mathf.Sin(angle*9+y*4)+.006f*Mathf.Cos(angle*15-y*5);
        float binding=.025f*Mathf.Exp(-Mathf.Pow(Mathf.Sin(y*8.2f)*4,2));
        return new Vector3(Mathf.Cos(angle)*(w+fold-binding)+.035f*Mathf.Sin(y*1.8f),y,
            Mathf.Sin(angle)*(d+fold-binding));
    }
    public static Mesh Cocoon()
    {
        var b=new Builder();b.Grid(76,40,CocoonPoint,true);
        // A turned, open hem with real thickness surrounds the emerging feet.
        // Double-sided fabric also keeps the interior from vanishing below it.
        b.Grid(2,40,(u,v)=>
        {
            var rim=CocoonPoint(u,0);
            return new Vector3(rim.x*Mathf.Lerp(1,.73f,v),v*.035f,rim.z*Mathf.Lerp(1,.73f,v));
        });
        b.Grid(1,40,(u,v)=>Vector3.Lerp(CocoonPoint(u,1),new Vector3(0,2.25f,0),v),true);
        return b.Finish("Ritual / opaque suspended adult cast");
    }
    public static Mesh CocoonSeams()
    {
        var b=new Builder();
        for(int i=0;i<26;i++)
        {
            float y=.13f+i*.077f;var p=CocoonPoint(.25f,y/2.25f);
            b.Tube(p+new Vector3(-.04f,-.019f,.012f),p+new Vector3(.04f,.019f,.012f),.006f,5);
        }
        for(int ring=0;ring<5;ring++)
        {
            float v=(.3f+ring*.37f)/2.25f;
            for(int j=0;j<32;j++)
            {var a=CocoonPoint(j/32f,v);var c=CocoonPoint((j+1)/32f,v);b.Tube(a*1.004f,c*1.004f,.013f,5);}
        }
        return b.Finish("Ritual / restraint bands and stitched front");
    }
    public static Mesh CastExtremities()
    {
        var b=new Builder();
        // Two narrow cast feet emerge below the wrapping. These are carved
        // solids, with separate toe forms rather than a flat decal or hand scan.
        for(int s=-1;s<=1;s+=2)
        {
            var ankle=new Vector3(s*.105f,-.025f,.03f);
            b.Ellipsoid(ankle,new Vector3(.058f,.14f,.065f));
            b.Ellipsoid(ankle+new Vector3(0,-.12f,.067f),new Vector3(.069f,.068f,.135f));
            for(int toe=0;toe<4;toe++)b.Ellipsoid(ankle+new Vector3(-.043f+toe*.029f,-.145f,.178f),new Vector3(.019f,.026f,.04f),6,10);
        }
        return b.Finish("Ritual / paired ivory cast extremities");
    }
    public static Mesh CalciumFragments()
    {
        var b=new Builder();
        // An authored arc at the wall foot; no random runtime schedule.
        for(int i=0;i<23;i++)
        {
            float a=i*2.39996f;float r=.14f+Mathf.Sqrt(i/23f)*.67f;
            var center=new Vector3(Mathf.Cos(a)*r,.018f,Mathf.Sin(a)*r*.42f);
            b.Ellipsoid(center,new Vector3(.02f+(i%4)*.009f,.014f,.027f+(i%3)*.01f),3,5);
        }
        return b.Finish("Ritual / calcium fragments at cast foot");
    }
    public static Mesh Cable(Vector3 start,Vector3 end,float radius=.009f)
    {var b=new Builder();b.Tube(start,end,radius,7);return b.Finish("Ritual / tension cable");}
    public static void Validate(Mesh mesh)
    {
        if(!mesh||mesh.vertexCount==0)throw new InvalidOperationException("Empty ritual geometry.");
        foreach(var p in mesh.vertices)RequireFinite(p,mesh.name+" vertex");
        foreach(var n in mesh.normals)RequireFinite(n,mesh.name+" normal");
        foreach(var t in mesh.tangents) {RequireFinite(new Vector3(t.x,t.y,t.z),mesh.name+" tangent");if(!Finite(t.w))throw new InvalidOperationException("Invalid tangent sign.");}
        foreach(var u in mesh.uv)if(!Finite(u.x)||!Finite(u.y))throw new InvalidOperationException("Invalid ritual UV.");
        foreach(int index in mesh.triangles)if(index<0||index>=mesh.vertexCount)throw new InvalidOperationException("Invalid ritual triangle.");
        RequireFinite(mesh.bounds.center,mesh.name+" bounds");RequireFinite(mesh.bounds.extents,mesh.name+" bounds");
    }
    static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
    static void RequireFinite(Vector3 v,string label)
    {if(!Finite(v.x)||!Finite(v.y)||!Finite(v.z))throw new InvalidOperationException(label+" must be finite.");}
}

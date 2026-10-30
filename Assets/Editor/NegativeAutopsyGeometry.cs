using System;
using System.Collections.Generic;
using UnityEngine;

// Original sculpted anatomy, built into fresh meshes. No shared/native skin
// buffers or earlier mesh assets are changed by this generator.
public static class NegativeAutopsyGeometry
{
    public static Vector3 FlapPivot(int side)=>new Vector3(side*.25f,-.85f,.12f);
    sealed class Form
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int> indices=new List<int>();
        public void Grid(int rows,int columns,Func<float,float,Vector3> sample,bool reverse=false,Func<float,float,bool> omit=null)
        {
            int start=vertices.Count;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {float u=x/(float)columns,v=y/(float)rows;var p=sample(u,v);Finite(p);vertices.Add(p);uv.Add(new Vector2(u,v));}
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {
                if(omit!=null&&omit((x+.5f)/columns,(y+.5f)/rows))continue;
                int a=start+y*(columns+1)+x,b=a+columns+1;
                if(reverse)indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                else indices.AddRange(new[]{a,a+1,b,a+1,b+1,b});
            }
        }
        public void Sweep(Func<float,Vector3> curve,Func<float,float> radius,int rows=22,int sides=10,float flatten=1)
        {
            Grid(rows,sides,(u,v)=>
            {
                Vector3 p=curve(v),d=(curve(Mathf.Min(1,v+.002f))-curve(Mathf.Max(0,v-.002f))).normalized;
                Vector3 side=Vector3.Cross(d,Mathf.Abs(Vector3.Dot(d,Vector3.up))>.94f?Vector3.forward:Vector3.up).normalized;
                Vector3 normal=Vector3.Cross(d,side).normalized;
                return p+(side*Mathf.Cos(u*Mathf.PI*2)+normal*(Mathf.Sin(u*Mathf.PI*2)*flatten))*radius(v);
            });
        }
        public void Strand(Vector3 a,Vector3 b,float r,int sides=8)
        {if((b-a).sqrMagnitude<.000001f)throw new InvalidOperationException("Degenerate autopsy strand");Sweep(t=>Vector3.Lerp(a,b,t),t=>r,4,sides);}
        public Mesh End(string name)
        {
            var m=new Mesh{name=name};if(vertices.Count>65535)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(vertices);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();Validate(m);return m;
        }
    }
    static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
    {float s=1-t;return s*s*s*a+3*s*s*t*b+3*s*t*t*c+t*t*t*d;}
    static float G(float x,float y,float cx,float cy,float sx,float sy)
    {return Mathf.Exp(-.5f*(Mathf.Pow((x-cx)/sx,2)+Mathf.Pow((y-cy)/sy,2)));}
    static Vector3 Shroud(float angle,float y)
    {
        float v=Mathf.InverseLerp(-2.12f,-.59f,y);
        float w=.21f+.13f*Mathf.Exp(-Mathf.Pow((v-.83f)/.17f,2))-.045f*Mathf.Exp(-Mathf.Pow((v-.08f)/.14f,2));
        float d=.15f+.045f*Mathf.Exp(-Mathf.Pow((v-.64f)/.3f,2));
        float fold=.011f*Mathf.Sin(angle*9+v*10)+.005f*Mathf.Cos(angle*17-v*7);
        return new Vector3(Mathf.Cos(angle)*(w+fold)+.035f*Mathf.Sin(v*3),y,Mathf.Sin(angle)*(d+fold)+.06f*Mathf.Sin(v*2));
    }
    public static Mesh BackAndLowerShroud()
    {
        var b=new Form();
        b.Grid(82,64,(u,v)=>Shroud(u*Mathf.PI*2,Mathf.Lerp(-2.12f,-.59f,v)),true,
            (u,v)=> {float y=Mathf.Lerp(-2.12f,-.59f,v);return y> -1.54f&&y<-.68f&&Mathf.Sin(u*Mathf.PI*2)>.18f;});
        // Thick rim at the lower hem, preserving the visible separate feet.
        b.Grid(3,64,(u,v)=>{var p=Shroud(u*Mathf.PI*2,-2.12f);return new Vector3(p.x*Mathf.Lerp(1,.82f,v),p.y+.025f*v,p.z*Mathf.Lerp(1,.82f,v));});
        return b.End("Autopsy / retained back and lower shroud");
    }
    static Vector3 FlapPoint(int side,float u,float v)
    {
        float y=Mathf.Lerp(-1.56f,-.66f,v),w=.27f+.075f*Mathf.Sin(v*Mathf.PI*.86f);
        float edge=.008f+.013f*Mathf.Sin(v*47)+.006f*Mathf.Sin(v*119);
        float x=side*Mathf.Lerp(w,edge,u);
        float curve=.17f+.060f*Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow(x/.37f,2)));
        float fold=.012f*Mathf.Sin(v*37+u*3)+.007f*Mathf.Sin(u*29-v*8);
        return new Vector3(x,y,curve+fold)-FlapPivot(side);
    }
    public static Mesh ChestFlap(int side)
    {
        var b=new Form();
        b.Grid(70,36,(u,v)=>FlapPoint(side,u,v),side>0);
        b.Grid(70,36,(u,v)=>FlapPoint(side,u,v)-Vector3.forward*.025f,side<0);
        // Both torn free edge and outside edge have visible material thickness.
        foreach(float edge in new[]{0f,1f})b.Grid(70,2,(u,v)=>FlapPoint(side,edge,v)-Vector3.forward*(u*.025f),side>0);
        foreach(float end in new[]{0f,1f})b.Grid(2,36,(u,v)=>FlapPoint(side,u,end)-Vector3.forward*(v*.025f),side>0);
        return b.End("Autopsy / thick torn chest flap "+side);
    }
    public static Mesh FlapSleeve(int side)
    {
        var b=new Form();Vector3 pivot=FlapPivot(side);
        Vector3 a=side<0?new Vector3(-.29f,-.76f,.06f):new Vector3(.30f,-.77f,.04f);
        Vector3 elbow=side<0?new Vector3(-.21f,-1.17f,.13f):new Vector3(.24f,-1.25f,.12f);
        Vector3 wrist=side<0?new Vector3(.14f,-.99f,.20f):new Vector3(-.08f,-1.40f,.20f);
        b.Sweep(t=>Vector3.Lerp(a,elbow,t)-pivot,t=>Mathf.Lerp(.074f,.047f,t),18,16,.75f);
        b.Sweep(t=>Vector3.Lerp(elbow,wrist,t)-pivot,t=>Mathf.Lerp(.047f,.032f,t),18,16,.75f);
        return b.End("Autopsy / wrapped folded arm "+side);
    }
    public static Mesh TornSutures(int side)
    {
        var b=new Form();
        for(int i=0;i<20;i++)
        {
            float v=.03f+i*.048f;Vector3 p=FlapPoint(side,1,v);
            b.Sweep(t=>p+new Vector3(-side*.034f*t,.024f*Mathf.Sin(t*2.5f),.012f*Mathf.Sin(t*Mathf.PI)),t=>Mathf.Lerp(.004f,.001f,t),8,6);
        }
        return b.End("Autopsy / separated sutures "+side);
    }
    public static Mesh ChestCavity()
    {
        var b=new Form();
        b.Grid(36,64,(u,v)=>
        {
            float a=u*Mathf.PI*2;
            return new Vector3(Mathf.Sin(a)*.255f*v,-1.10f+Mathf.Cos(a)*.435f*v,-.083f+.245f*v*v+.008f*Mathf.Sin(a*7)*v);
        });
        return b.End("Autopsy / deep concave thoracic chamber");
    }
    public static Mesh RecessedTissue()
    {
        var b=new Form();
        for(int side=-1;side<=1;side+=2)
        b.Grid(38,34,(u,v)=>
        {
            float angle=u*Mathf.PI*2,y=Mathf.Lerp(-1.43f,-.77f,v);
            float taper=Mathf.Max(0,Mathf.Sin(v*Mathf.PI));
            float w=.066f*taper;
            return new Vector3(side*.085f+Mathf.Cos(angle)*w,y,Mathf.Sin(angle)*.025f*taper-.025f+.013f*Mathf.Sin(y*27+angle*3));
        },true);
        // A central tether gives the empty cavity a readable depth reference.
        b.Sweep(t=>new Vector3(.014f*Mathf.Sin(t*8),Mathf.Lerp(-1.48f,-.73f,t),-.025f),t=>.017f+.008f*Mathf.Sin(t*12),38,10,.7f);
        return b.End("Autopsy / recessed dark thoracic tissue");
    }
    public static Mesh Ribs()
    {
        var b=new Form();
        for(int side=-1;side<=1;side+=2)for(int i=0;i<6;i++)
        {
            float y=-.77f-i*.108f+(side>0?.013f:0),width=.16f+.07f*Mathf.Sin((i+.8f)/6.8f*Mathf.PI);
            var a=new Vector3(side*.035f,y,-.035f);
            var c1=new Vector3(side*(width+.055f),y+.014f,-.025f);
            var c2=new Vector3(side*(width+.022f),y-.042f,.225f);
            var d=new Vector3(side*(.036f+.008f*Mathf.Sin(i*2.1f)),y-.064f,.197f);
            b.Sweep(t=>Bezier(a,c1,c2,d,t),t=>Mathf.Lerp(.016f,.004f,Mathf.Pow(t,3))*(1+.13f*Mathf.Sin(t*17+i)),28,10,.78f);
        }
        return b.End("Autopsy / twelve exposed broken ribs");
    }
    public static Mesh NewHarness()
    {
        var b=new Form();
        b.Strand(Vector3.zero,new Vector3(-.18f,-.64f,.02f),.016f);
        b.Strand(Vector3.zero,new Vector3(.19f,-.65f,.02f),.016f);
        foreach(float y in new[]{-1.65f,-1.89f})for(int i=0;i<40;i++)
        {var a=Shroud(i/40f*Mathf.PI*2,y);var c=Shroud((i+1)/40f*Mathf.PI*2,y);a.x*=1.025f;a.z*=1.025f;c.x*=1.025f;c.z*=1.025f;b.Strand(a,c,.011f,6);}
        return b.End("Autopsy / shoulder suspension and lower bindings");
    }
    static readonly float[] HeadY={.06f,.10f,.16f,.23f,.30f,.37f,.43f,.465f};
    static readonly float[] HeadW={.082f,.115f,.147f,.163f,.153f,.146f,.110f,.003f};
    static readonly float[] HeadD={.066f,.080f,.095f,.108f,.109f,.105f,.084f,.003f};
    static float Profile(float[] values,float y)
    {int i=0;while(i<HeadY.Length-2&&y>HeadY[i+1])i++;return Mathf.Lerp(values[i],values[i+1],Mathf.InverseLerp(HeadY[i],HeadY[i+1],y));}
    static float FaceDepth(float x,float y)
    {
        float surface=Profile(HeadD,y)*Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow(x/Profile(HeadW,y),2)));
        float nose=.057f*G(x,y,.002f,.235f,.023f,.055f)+.024f*G(x,y,0,.27f,.014f,.073f);
        float brow=.021f*(G(x,y,-.066f,.333f,.054f,.019f)+G(x,y,.066f,.330f,.054f,.019f));
        float cheeks=.019f*(G(x,y,-.105f,.239f,.030f,.037f)+G(x,y,.105f,.235f,.030f,.037f));
        float mouth=.025f*G(x,y,0,.139f,.071f,.025f);
        return surface+nose+brow+cheeks+mouth;
    }
    static Vector3 HeadPoint(float u,float v)
    {
        float a=u*Mathf.PI*2,y=Mathf.Lerp(HeadY[0],HeadY[HeadY.Length-1],v);
        float x=Mathf.Sin(a)*Profile(HeadW,y),front=Mathf.Cos(a);
        float z=front>=0?FaceDepth(x,y):front*Profile(HeadD,y);
        return new Vector3(x,y,z);
    }
    static bool FaceHole(float u,float v)
    {
        var p=HeadPoint(u,v);if(Mathf.Cos(u*Mathf.PI*2)<.55f)return false;
        bool left=Mathf.Pow((p.x+.065f)/.040f,2)+Mathf.Pow((p.y-.299f)/.028f,2)<1;
        bool right=Mathf.Pow((p.x-.065f)/.041f,2)+Mathf.Pow((p.y-.296f)/.027f,2)<1;
        bool mouth=p.y<.132f&&Mathf.Abs(p.x)<.082f;
        return left||right||mouth;
    }
    public static Mesh AdultFace()
    {
        var b=new Form();
        // Profiled cranium, cheek planes, bridge and orbital cutouts. The face
        // is not a primitive sphere or a texture drawn over a smooth mask.
        b.Grid(84,100,HeadPoint,false,FaceHole);
        for(int side=-1;side<=1;side+=2)
        b.Grid(4,48,(u,v)=>
        {
            float angle=u*Mathf.PI*2,r=Mathf.Lerp(1.15f,.91f,v),cy=side<0?.299f:.296f;
            float x=side*.065f+Mathf.Cos(angle)*.0405f*r,y=cy+Mathf.Sin(angle)*.0275f*r;
            return new Vector3(x,y,FaceDepth(x,y)+.008f*Mathf.Sin(v*Mathf.PI));
        });
        // Small ear plates are part of the same sculpted adult cranium.
        for(int side=-1;side<=1;side+=2)
        b.Grid(16,22,(u,v)=>
        {
            float a=u*Mathf.PI*2,r=Mathf.Sin(v*Mathf.PI);
            return new Vector3(side*(.150f+.020f*r*Mathf.Cos(a)),.238f+.058f*r*Mathf.Sin(a),-.012f+.018f*Mathf.Cos(v*Mathf.PI));
        });
        return b.End("Autopsy / adult face with open orbits and nasal bridge");
    }
    public static Mesh FaceCavities()
    {
        var b=new Form();
        for(int side=-1;side<=1;side+=2)
        b.Grid(14,40,(u,v)=>
        {
            float a=u*Mathf.PI*2,cy=side<0?.299f:.296f;
            return new Vector3(side*.065f+Mathf.Cos(a)*.039f*v,cy+Mathf.Sin(a)*.026f*v,.021f+.075f*v*v);
        },true);
        b.Grid(18,32,(u,v)=>new Vector3((u-.5f)*.18f,.075f+v*.067f,.056f+.012f*Mathf.Cos(u*3)),false);
        return b.End("Autopsy / recessed eye and mouth chambers");
    }
    public static Mesh LowerJaw()
    {
        var b=new Form();
        b.Sweep(t=>
        {
            float a=Mathf.Lerp(-Mathf.PI*.5f,Mathf.PI*.5f,t);
            return new Vector3(Mathf.Sin(a)*.123f,-.020f+.055f*Mathf.Pow(Mathf.Sin(a),2),Mathf.Cos(a)*.115f+.008f);
        },t=>.018f+.005f*Mathf.Sin(t*Mathf.PI),44,14,1.35f);
        for(int side=-1;side<=1;side+=2)
            b.Sweep(t=>Vector3.Lerp(new Vector3(side*.121f,.037f,.006f),new Vector3(side*.114f,-.015f,.055f),t),t=>.015f,12,10,.62f);
        return b.End("Autopsy / hinged adult mandible");
    }
    public static Mesh Teeth(bool upper)
    {
        var b=new Form();
        for(int i=0;i<12;i++)
        {
            float a=Mathf.Lerp(-1.22f,1.22f,i/11f),height=.023f+.008f*Mathf.Sin(i*1.74f+.8f);
            Vector3 center=new Vector3(Mathf.Sin(a)*.101f,upper?.148f:.020f,Mathf.Cos(a)*.117f+.009f);
            Quaternion rot=Quaternion.Euler(0,a*Mathf.Rad2Deg,i%3==0?5:-3);
            b.Grid(7,8,(u,v)=>
            {
                float angle=u*Mathf.PI*2,shape=.82f+.18f*Mathf.Sin(v*Mathf.PI);
                float w=.0078f*(i==2||i==9?1.13f:1);
                return center+rot*new Vector3(Mathf.Cos(angle)*w*shape,(upper?-1:1)*height*v,Mathf.Sin(angle)*.008f*shape);
            },!upper);
        }
        return b.End(upper?"Autopsy / irregular upper dentition":"Autopsy / irregular lower dentition");
    }
    public static Mesh Neck()
    {
        var b=new Form();
        b.Sweep(t=>new Vector3(.009f*Mathf.Sin(t*4),Mathf.Lerp(-.10f,.095f,t),-.012f),t=>.052f+.008f*Mathf.Sin(t*3),20,24,.78f);
        return b.End("Autopsy / adult neck under the mandible");
    }
    public static void Validate(Mesh mesh)
    {
        if(!mesh)throw new InvalidOperationException("Missing autopsy mesh");
        NegativeRitualGeometry.Validate(mesh);
        if(mesh.vertexCount==0||mesh.triangles.Length==0)throw new InvalidOperationException("Empty autopsy mesh");
    }
    static void Finite(Vector3 p)
    {if(float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.x)||float.IsInfinity(p.y)||float.IsInfinity(p.z))throw new InvalidOperationException("Non-finite autopsy vertex");}
}

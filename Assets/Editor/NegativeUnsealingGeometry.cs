using System;
using System.Collections.Generic;
using UnityEngine;

// Original adult-scale sculpture. New buffers only; no previous meshes are edited.
public static class NegativeUnsealingGeometry
{
    sealed class Sculpt
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int> triangles=new List<int>();
        public void Grid(int rows,int columns,Func<float,float,Vector3> shape,bool reverse=false)
        {
            int start=vertices.Count;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {float u=x/(float)columns,v=y/(float)rows;var p=shape(u,v);Finite(p);vertices.Add(p);uv.Add(new Vector2(u,v));}
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {
                int a=start+y*(columns+1)+x,b=a+columns+1;
                if(reverse)triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                else triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});
            }
        }
        public void Egg(Vector3 p,Vector3 size,Quaternion rotation,int rows=14,int columns=24)
        {
            Grid(rows,columns,(u,v)=>
            {
                float lat=Mathf.PI*v,lon=2*Mathf.PI*u;
                return p+rotation*Vector3.Scale(size,new Vector3(Mathf.Sin(lat)*Mathf.Cos(lon),Mathf.Cos(lat),Mathf.Sin(lat)*Mathf.Sin(lon)));
            });
        }
        public void Limb(Vector3 a,Vector3 b,float radiusA,float radiusB,float depth=1,int sides=12)
        {
            Vector3 axis=(b-a).normalized;
            if((b-a).sqrMagnitude<.000001f)throw new InvalidOperationException("Degenerate sculpted limb");
            Vector3 side=Vector3.Cross(axis,Mathf.Abs(Vector3.Dot(axis,Vector3.forward))>.93f?Vector3.up:Vector3.forward).normalized;
            Vector3 normal=Vector3.Cross(axis,side).normalized;
            Grid(7,sides,(u,v)=>
            {
                float radius=Mathf.Lerp(radiusA,radiusB,v)*(1+.055f*Mathf.Sin(v*Mathf.PI));
                return Vector3.Lerp(a,b,v)+side*(radius*Mathf.Cos(u*2*Mathf.PI))+normal*(radius*depth*Mathf.Sin(u*2*Mathf.PI));
            },true);
        }
        public Mesh Finish(string name)
        {
            var mesh=new Mesh{name=name};
            if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();Validate(mesh);return mesh;
        }
    }
    static readonly float[] TorsoY={-.66f,-.50f,-.28f,-.02f,.23f,.43f,.54f};
    static readonly float[] TorsoW={.21f,.24f,.20f,.23f,.30f,.32f,.15f};
    static readonly float[] TorsoD={.052f,.065f,.055f,.063f,.073f,.055f,.035f};
    static float At(float[] value,float y)
    {
        int i=0;while(i<TorsoY.Length-2&&y>TorsoY[i+1])i++;
        return Mathf.Lerp(value[i],value[i+1],Mathf.InverseLerp(TorsoY[i],TorsoY[i+1],y));
    }
    public static Mesh CaptiveTorso()
    {
        var b=new Sculpt();
        b.Grid(68,44,(u,v)=>
        {
            float y=Mathf.Lerp(TorsoY[0],TorsoY[TorsoY.Length-1],v),a=u*2*Mathf.PI;
            float front=Mathf.Max(0,Mathf.Sin(a));
            float rib=.009f*Mathf.Pow(Mathf.Max(0,Mathf.Cos((y+.02f)*38)),3)*Mathf.Exp(-Mathf.Pow((y-.12f)/.34f,2));
            return new Vector3(Mathf.Cos(a)*At(TorsoW,y)-.025f*Mathf.Sin(v*Mathf.PI),y,Mathf.Sin(a)*At(TorsoD,y)+rib*front);
        },true);
        // Shoulder to elbow to raised wrist, a contracted adult pose.
        b.Limb(new Vector3(-.29f,.39f,0),new Vector3(-.43f,.07f,.035f),.078f,.048f,.62f);
        b.Limb(new Vector3(-.43f,.07f,.035f),new Vector3(-.48f,.44f,.115f),.048f,.031f,.65f);
        b.Limb(new Vector3(.29f,.39f,0),new Vector3(.45f,.05f,.04f),.08f,.047f,.62f);
        b.Limb(new Vector3(.45f,.05f,.04f),new Vector3(.49f,.41f,.115f),.047f,.032f,.65f);
        b.Limb(new Vector3(-.15f,-.56f,0),new Vector3(-.30f,-.86f,.05f),.105f,.075f,.65f);
        b.Limb(new Vector3(.15f,-.56f,0),new Vector3(.29f,-.78f,.075f),.105f,.075f,.65f);
        b.Limb(new Vector3(-.30f,-.86f,.05f),new Vector3(-.13f,-1.06f,.045f),.068f,.04f,.65f);
        b.Limb(new Vector3(.29f,-.78f,.075f),new Vector3(.12f,-1.01f,.03f),.068f,.04f,.65f);
        return b.Finish("Unsealing / contracted adult cast");
    }
    public static Mesh CaptiveHead()
    {
        var b=new Sculpt();
        // The neck is the origin so the authored head turn pivots anatomically.
        b.Limb(new Vector3(0,-.08f,-.025f),new Vector3(0,.095f,-.015f),.06f,.065f,.7f);
        b.Grid(32,40,(u,v)=>
        {
            float lat=Mathf.PI*v,lon=2*Mathf.PI*u;
            float y=Mathf.Cos(lat)*.215f+.18f;
            float width=.154f*(.87f+.13f*Mathf.Sin(lat));
            float z=Mathf.Sin(lat)*Mathf.Sin(lon)*.087f;
            // A blank cast without eye/mouth holes; the displaced cheek breaks symmetry.
            float nose=.020f*Mathf.Exp(-Mathf.Pow((u-.25f)/.055f,2)-Mathf.Pow((v-.55f)/.18f,2));
            return new Vector3(Mathf.Sin(lat)*Mathf.Cos(lon)*width+.016f*Mathf.Sin(v*3),y,z+nose);
        });
        return b.Finish("Unsealing / turned blank adult face");
    }
    public static Mesh Hand(int side)
    {
        var b=new Sculpt();
        b.Egg(Vector3.zero,new Vector3(.067f,.094f,.025f),Quaternion.Euler(0,0,side*8),12,20);
        for(int i=0;i<4;i++)
        {
            float x=(i-1.5f)*.035f, length=new[]{.153f,.181f,.169f,.134f}[i];
            Vector3 a=new Vector3(x,.058f,.004f),joint=new Vector3(x*1.27f,.058f+length*.58f,.010f),tip=new Vector3(x*1.5f,.058f+length,.020f);
            b.Limb(a,joint,.014f,.011f,.8f,9);b.Limb(joint,tip,.011f,.007f,.85f,9);
            b.Egg(tip,new Vector3(.008f,.012f,.008f),Quaternion.identity,7,10);
        }
        var thumbA=new Vector3(-side*.052f,-.015f,.01f);var thumbB=new Vector3(-side*.111f,.025f,.024f);var thumbC=new Vector3(-side*.132f,.087f,.032f);
        b.Limb(thumbA,thumbB,.021f,.016f,.85f,10);b.Limb(thumbB,thumbC,.016f,.009f,.85f,10);
        return b.Finish("Unsealing / long adult hand " + side);
    }
    public static Mesh CaptiveBindings()
    {
        var b=new Sculpt();
        foreach(float y in new[]{-.5f,-.21f,.12f})
            for(int i=0;i<18;i++)
            {
                float a=Mathf.PI*i/18,c=Mathf.PI*(i+1)/18;
                b.Limb(new Vector3(Mathf.Cos(a)*(At(TorsoW,y)+.008f),y,Mathf.Sin(a)*(At(TorsoD,y)+.01f)),new Vector3(Mathf.Cos(c)*(At(TorsoW,y)+.008f),y,Mathf.Sin(c)*(At(TorsoD,y)+.01f)),.008f,.008f,1,6);
            }
        for(int i=0;i<12;i++)
        {float y=-.43f+i*.075f;b.Limb(new Vector3(-.022f,y-.01f,At(TorsoD,y)+.016f),new Vector3(.026f,y+.014f,At(TorsoD,y)+.017f),.005f,.005f,1,5);}
        return b.Finish("Unsealing / embedded restraints and chest closure");
    }
    public static Mesh Veil(int side)
    {
        var b=new Sculpt();
        b.Grid(80,42,(u,v)=>
        {
            float y=(v-.5f)*2.48f;
            float tear=.018f+.016f*Mathf.Sin(v*43)+.009f*Mathf.Sin(v*117);
            float panelX=side*Mathf.Lerp(.82f,tear,u);
            float z=.015f*Mathf.Sin(u*7+v*4)*Mathf.Sin(u*Mathf.PI)+.018f*Mathf.Pow(u,4)*Mathf.Sin(v*69);
            return new Vector3(panelX-side*.82f,y,z);
        },side>0);
        return b.Finish("Unsealing / torn pressure veil " + side);
    }
    public static Mesh VeilEdge(int side)
    {
        var b=new Sculpt();
        for(int i=0;i<22;i++)
        {
            float y=-1.16f+i*.109f,v=y/2.48f+.5f;
            float x=side*(.018f+.016f*Mathf.Sin(v*43)+.009f*Mathf.Sin(v*117)-.82f);
            b.Limb(new Vector3(x,y,0),new Vector3(x-side*.025f,y+.035f,.022f),.006f,.002f,1,5);
        }
        return b.Finish("Unsealing / torn veil filaments " + side);
    }
    static Vector3 HangingPoint(float u,float v)
    {
        float y=Mathf.Lerp(-2.12f,-.59f,v),a=u*Mathf.PI*2;
        float waist=.21f+.13f*Mathf.Exp(-Mathf.Pow((v-.83f)/.17f,2))-.045f*Mathf.Exp(-Mathf.Pow((v-.08f)/.14f,2));
        float depth=.15f+.045f*Mathf.Exp(-Mathf.Pow((v-.64f)/.3f,2));
        float knee=.025f*Mathf.Cos(a*2)*Mathf.Exp(-Mathf.Pow((v-.3f)/.18f,2));
        float fold=.011f*Mathf.Sin(a*9+v*10)+.005f*Mathf.Cos(a*17-v*7);
        return new Vector3(Mathf.Cos(a)*(waist+fold)+.035f*Mathf.Sin(v*3),y,Mathf.Sin(a)*(depth+fold+knee)+.06f*Mathf.Sin(v*2));
    }
    public static Mesh HangingBody()
    {
        var b=new Sculpt();b.Grid(76,44,HangingPoint,true);
        // Slumped head, narrow neck, angular shoulders and a folded arm beneath the cloth.
        b.Egg(new Vector3(-.065f,-.39f,.07f),new Vector3(.146f,.222f,.143f),Quaternion.Euler(11,0,-16),22,28);
        b.Limb(new Vector3(-.035f,-.60f,.03f),new Vector3(-.06f,-.52f,.05f),.075f,.067f,1,16);
        b.Limb(new Vector3(-.29f,-.76f,.06f),new Vector3(-.21f,-1.17f,.13f),.074f,.047f,.75f);
        b.Limb(new Vector3(-.21f,-1.17f,.13f),new Vector3(.14f,-.99f,.19f),.047f,.032f,.75f);
        b.Limb(new Vector3(.30f,-.77f,.04f),new Vector3(.24f,-1.25f,.12f),.075f,.05f,.75f);
        b.Limb(new Vector3(.24f,-1.25f,.12f),new Vector3(-.08f,-1.40f,.18f),.05f,.033f,.75f);
        return b.Finish("Unsealing / suspended contracted adult shroud");
    }
    public static Mesh HangingFeet()
    {
        var b=new Sculpt();
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*.078f;
            b.Limb(new Vector3(x,-2.02f,0),new Vector3(x,-2.24f,.03f),.048f,.039f,.8f,12);
            b.Egg(new Vector3(x,-2.285f,.07f),new Vector3(.051f,.061f,.14f),Quaternion.Euler(0,side*9,0),12,20);
            for(int i=0;i<5;i++)b.Egg(new Vector3(x+(i-2)*.020f,-2.29f,.191f-Mathf.Abs(i-1)*.011f),new Vector3(.013f,.025f,.032f),Quaternion.identity,7,10);
        }
        return b.Finish("Unsealing / adult feet under the hem");
    }
    public static Mesh HangingBindings()
    {
        var b=new Sculpt();
        foreach(float v in new[]{.1f,.28f,.49f,.72f,.9f})
        for(int i=0;i<32;i++)
        {var a=HangingPoint(i/32f,v);var c=HangingPoint((i+1)/32f,v);a.x*=1.02f;a.z*=1.025f;c.x*=1.02f;c.z*=1.025f;b.Limb(a,c,.012f,.012f,1,6);}
        for(int i=0;i<22;i++)
        {float v=.06f+i*.04f;var p=HangingPoint(.25f,v);b.Limb(p+new Vector3(-.024f,-.018f,.01f),p+new Vector3(.029f,.015f,.014f),.0045f,.0045f,1,5);}
        // The load-bearing upper harness, visibly attached to the cable pivot.
        b.Limb(new Vector3(0,0,0),new Vector3(-.18f,-.64f,.02f),.016f,.016f,1,8);
        b.Limb(new Vector3(0,0,0),new Vector3(.19f,-.65f,.02f),.016f,.016f,1,8);
        return b.Finish("Unsealing / suspended harness and closures");
    }
    public static Mesh Cable()
    {var b=new Sculpt();b.Limb(Vector3.zero,Vector3.down,.014f,.014f,1,10);return b.Finish("Unsealing / extensible unit suspension");}
    public static void Validate(Mesh mesh)
    {
        if(!mesh)throw new InvalidOperationException("Missing unsealing mesh");
        NegativeRitualGeometry.Validate(mesh);
        if(mesh.vertexCount==0||mesh.triangles.Length==0)throw new InvalidOperationException("Empty unsealing mesh");
    }
    static void Finite(Vector3 v)
    {if(float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.x)||float.IsInfinity(v.y)||float.IsInfinity(v.z))throw new InvalidOperationException("Non-finite unsealing vertex");}
}

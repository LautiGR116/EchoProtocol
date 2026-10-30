using System;
using System.Collections.Generic;
using System.Linq;
using EchoProtocol.Campaign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Original sculpted interior, expressed in the existing shell's bind space.
// The licensed actor, its recorded counterpart and its buffers remain intact.
public static class NegativeAutopsyThroatIntegration
{
    const string Folder="Assets/_Game/Art/Embodiment/RenderpeoplePrivate/NegativeIntruder/";
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().name!="EchoProtocol_Negative")
            throw new InvalidOperationException("Open Negative in Edit Mode.");
        var mask=All<NegativeUnsealingMask>().Single();
        var existing=mask.GetComponentInChildren<NegativeAutopsyThroat>(true);
        var shell=mask.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r=>r.name=="Negative / ivory shell");
        var surface=mask.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Unsealing / ceramic plates and recessed core");
        var head=surface.parent;
        var bind=shell.sharedMesh.bindposes[Array.IndexOf(shell.bones,head)];
        var pivot=bind.MultiplyPoint3x4(new Vector3(-.023f,1.72f,.198f));
        var throat=MeshAsset("AutopsyThroat-v2.asset",()=>BuildThroat(bind,pivot));
        var teeth=MeshAsset("AutopsyTeeth-v1.asset",()=>BuildTeeth(bind,pivot));
        var well=MeshAsset("AutopsyWell-v2.asset",()=>BuildWell(bind,pivot));
        var dark=MaterialAsset("AutopsyThroat-v2.mat",new Color(.38f,.075f,.05f),.16f);
        var black=MaterialAsset("AutopsyWell-v2.mat",new Color(.004f,.001f,.001f),0);
        var ivory=MaterialAsset("AutopsyTeeth-v1.mat",new Color(.49f,.39f,.27f),.42f);
        var root=existing?existing.transform:new GameObject("Autopsy / throat behind ceramic").transform;
        if(!existing){Undo.RegisterCreatedObjectUndo(root.gameObject,"Sculpt present throat");root.SetParent(head,false);root.localPosition=pivot;}
        Surface(root,"Autopsy / nested throat folds",throat,dark);
        Surface(root,"Autopsy / inward bone roots",teeth,ivory);
        Surface(root,"Autopsy / opaque inner well",well,black);
        foreach(var old in surface.GetComponentsInChildren<Renderer>(true))
            if(old.name=="Unsealing / recessed interior"||old.name=="Unsealing / inner binding fibers")
            {Undo.RecordObject(old,"Replace flat interior");old.enabled=false;}
        var movement=existing?existing:Undo.AddComponent<NegativeAutopsyThroat>(root.gameObject);
        var so=new SerializedObject(movement);
        so.FindProperty("mask").objectReferenceValue=mask;
        so.FindProperty("session").objectReferenceValue=All<EchoProtocol.Presentation.GameSession>().Single();
        so.FindProperty("encounter").objectReferenceValue=All<MachineCoverEncounter>().Single();
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
    static Mesh BuildThroat(Matrix4x4 bind,Vector3 pivot)
    {
        var b=new Builder(bind,pivot);
        // A continuous deep funnel with asymmetric annular tissue folds. Its
        // bottom seals the head beneath; it is not a graphic pasted on a face.
        b.Grid(64,64,(u,v)=>
        {
            float a=u*Mathf.PI*2,r=.015f+.985f*v;
            float crease=.005f*Mathf.Sin(v*13+Mathf.Sin(a*3)*1.8f+Mathf.Sin(a*7)*.8f)*Mathf.Sin(v*Mathf.PI)
                +.003f*Mathf.Sin(a*9+v*5)*r;
            float x=-.023f+Mathf.Cos(a)*.102f*r*(1+.035f*Mathf.Sin(a*5));
            float y=1.72f+Mathf.Sin(a)*.177f*r;
            float z=.173f+.038f*r*r+crease;
            return new Vector3(x,y,z);
        });
        // Anchored mucosal strands curve away from the open center.
        for(int side=-1;side<=1;side+=2)for(int i=0;i<6;i++)
        {
            float y=1.59f+i*.048f;
            var a=new Vector3(-.023f+side*.081f,y,.205f);
            var c=new Vector3(-.023f+side*.048f,y+.015f,.219f);
            var d=new Vector3(-.023f+side*.03f,y-.008f,.19f);
            b.Tube(a,c,.006f,.004f);b.Tube(c,d,.004f,.001f);
        }
        return b.Finish("Autopsy / recessed folded throat");
    }
    static Mesh BuildWell(Matrix4x4 bind,Vector3 pivot)
    {
        var b=new Builder(bind,pivot);
        b.Grid(12,40,(u,v)=>new Vector3(-.023f+Mathf.Cos(u*Mathf.PI*2)*.028f*v,
            1.72f+Mathf.Sin(u*Mathf.PI*2)*.062f*v,.183f+.001f*v));
        return b.Finish("Autopsy / lightless inner well");
    }
    static Mesh BuildTeeth(Matrix4x4 bind,Vector3 pivot)
    {
        var b=new Builder(bind,pivot);
        for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++)
        {
            float t=i/6f,y=1.58f+t*.278f;
            float w=.061f+.025f*Mathf.Sin(t*Mathf.PI);
            var a=new Vector3(-.023f+side*w,y,.206f);
            var middle=a+new Vector3(-side*.015f,.007f*(i%2==0?1:-1),.018f);
            var tip=middle+new Vector3(-side*(.011f+.005f*Mathf.Sin(i*2)),.014f*(i%2==0?-1:1),-.008f);
            b.Tube(a,middle,.009f,.006f);b.Tube(middle,tip,.006f,.0008f);
        }
        return b.Finish("Autopsy / irregular inward bone roots");
    }
    sealed class Builder
    {
        readonly List<Vector3> vertices=new List<Vector3>();readonly List<Vector2> uv=new List<Vector2>();readonly List<int> triangles=new List<int>();
        readonly Matrix4x4 bind;readonly Vector3 pivot;
        public Builder(Matrix4x4 b,Vector3 p){bind=b;pivot=p;}
        public void Grid(int rows,int columns,Func<float,float,Vector3> shape)
        {
            int first=vertices.Count;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {float u=x/(float)columns,v=y/(float)rows;vertices.Add(bind.MultiplyPoint3x4(shape(u,v))-pivot);uv.Add(new Vector2(u,v));}
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {int a=first+y*(columns+1)+x,b=a+columns+1;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
        }
        public void Tube(Vector3 a,Vector3 b,float r0,float r1)
        {
            var axis=(b-a).normalized;var side=Vector3.Cross(axis,Vector3.forward).normalized;
            if(side.sqrMagnitude<.1f)side=Vector3.Cross(axis,Vector3.up).normalized;
            var other=Vector3.Cross(axis,side).normalized;
            Grid(4,10,(u,v)=>Vector3.Lerp(a,b,v)+(side*Mathf.Cos(u*2*Mathf.PI)+other*Mathf.Sin(u*2*Mathf.PI))*Mathf.Lerp(r0,r1,v));
        }
        public Mesh Finish(string name)
        {
            var m=new Mesh{name=name};m.SetVertices(vertices);m.SetUVs(0,uv);m.SetTriangles(triangles,0);
            m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();NegativeUnsealingMaskGeometry.Validate(m);return m;
        }
    }
    static Mesh MeshAsset(string name,Func<Mesh> create)
    {var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+name);if(!mesh){mesh=create();AssetDatabase.CreateAsset(mesh,Folder+name);}NegativeUnsealingMaskGeometry.Validate(mesh);return mesh;}
    static Material MaterialAsset(string name,Color color,float smoothness)
    {
        var m=AssetDatabase.LoadAssetAtPath<Material>(Folder+name);if(m)return m;
        m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Cull",0);
        if(name.Contains("Throat")||name.Contains("Well"))
        {m.SetFloat("_SpecularHighlights",0);m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");m.SetFloat("_EnvironmentReflections",0);m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");}
        if(name.Contains("Throat"))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/NegativeUnsealing/WaxedMortuaryCanvas.png"));
        AssetDatabase.CreateAsset(m,Folder+name);return m;
    }
    static void Surface(Transform parent,string name,Mesh mesh,Material mat)
    {
        var found=parent.Cast<Transform>().SingleOrDefault(t=>t.name==name);
        var g=found?found.gameObject:new GameObject(name);if(!found)g.transform.SetParent(parent,false);
        var f=g.GetComponent<MeshFilter>();if(!f)f=g.AddComponent<MeshFilter>();f.sharedMesh=mesh;
        var r=g.GetComponent<MeshRenderer>();if(!r)r=g.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;
    }
    static T[] All<T>()where T:Component=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
}

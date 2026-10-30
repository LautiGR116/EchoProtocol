using System;
using System.Linq;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class NegativeRitualEnvironment
{
    public const string RootName="Negative / ritual apparatus";
    const string Marker="Ritual / authored environment";
    const string Folder="Assets/_Game/Art/NegativeRitual";
    static Transform root;
    static Material membrane,cloth,ivory,steel,thread;

    [MenuItem("Echo Protocol/El Negativo/Apply ritual environment to open scene")]
    public static void ApplyToOpenScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)
            throw new InvalidOperationException("Apply outside Play after compilation.");
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="EchoProtocol_Negative")throw new InvalidOperationException("Requires the open Negative scene; no scene is opened or saved.");
        var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==RootName);
        if(existing&&existing.GetComponentsInChildren<Transform>(true).Any(t=>t.name==Marker))
        {Debug.Log(DescribeOpenScene());return;}
        EnsureFolder(Folder);
        membrane=Paint("Tensioned cast",new Color(.23f,.205f,.174f),.08f,.52f);
        cloth=Paint("Stitched graphite",new Color(.065f,.058f,.052f),.05f,.27f);
        ivory=Paint("Calcium ivory",new Color(.62f,.56f,.43f),.03f,.29f);
        steel=Paint("Surgical frame",new Color(.21f,.23f,.21f),.68f,.5f);
        thread=Paint("Dark suture",new Color(.055f,.021f,.014f),.15f,.48f);
        var mesh0=Asset("Impression-0.asset",()=>NegativeRitualGeometry.Membrane(0));
        var mesh1=Asset("Impression-1.asset",()=>NegativeRitualGeometry.Membrane(1));
        var mesh2=Asset("Impression-2.asset",()=>NegativeRitualGeometry.Membrane(2));
        var seam0=Asset("Impression-seam-0.asset",()=>NegativeRitualGeometry.MembraneStitches(0));
        var seam1=Asset("Impression-seam-1.asset",()=>NegativeRitualGeometry.MembraneStitches(1));
        var seam2=Asset("Impression-seam-2.asset",()=>NegativeRitualGeometry.MembraneStitches(2));
        var cocoon=Asset("Suspended-cast.asset",NegativeRitualGeometry.Cocoon);
        var seams=Asset("Suspended-seams.asset",NegativeRitualGeometry.CocoonSeams);
        var feet=Asset("Cast-extremities.asset",NegativeRitualGeometry.CastExtremities);
        var debris=Asset("Calcium-fragments.asset",NegativeRitualGeometry.CalciumFragments);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Compose ritual chamber");
        try
        {
            root=existing?existing.transform:new GameObject(RootName).transform;
            if(!existing)Undo.RegisterCreatedObjectUndo(root.gameObject,"Create ritual apparatus");
            var contents=new GameObject(Marker).transform;contents.SetParent(root,false);
            Undo.RegisterCreatedObjectUndo(contents.gameObject,"Create ritual environment");
            Panel(contents,"Ritual / west impression 01",new Vector3(-7.28f,2.35f,-33.1f),mesh1,seam1,"IV / 013",false);
            Panel(contents,"Ritual / west impression 02",new Vector3(-7.28f,2.35f,-45.1f),mesh2,seam2,"IV / 019",false);
            Panel(contents,"Ritual / close membrane",new Vector3(-5.6f,1.65f,-38.25f),mesh0,seam0,"IV / 017",true);
            Panel(contents,"Ritual / impact panel",new Vector3(-5.45f,1.8f,-42.3f),mesh2,seam2,"IV / 018",true);
            var positions=new[]{new Vector3(6f,4.35f,-31.7f),new Vector3(-6.2f,4.35f,-40.5f),new Vector3(6.05f,4.35f,-45.2f)};
            for(int i=0;i<positions.Length;i++)
            {
                var g=new GameObject("Ritual / suspended cast "+(i+1)).transform;g.SetParent(contents,false);
                g.rotation=Quaternion.Euler(i==1?-8:5,i==0?-40:i==1?50:-25,i==1?-5:4);
                // The parent is the upper suspension pivot for an authored sway.
                // Mesh coordinates stay shared; no per-instance mesh copies.
                const float scale=.88f, attachment=2.16f;
                g.position=positions[i]+g.rotation*(Vector3.up*attachment*scale);
                foreach(var part in new[]{
                    MeshObject(g,"Ritual / opaque cast skin",cocoon,cloth,ShadowCastingMode.On),
                    MeshObject(g,"Ritual / bands and sutures",seams,ivory),
                    MeshObject(g,"Ritual / cast feet",feet,ivory)})
                {part.transform.localPosition=Vector3.down*attachment*scale;part.transform.localScale=Vector3.one*scale;}
                for(int side=-1;side<=1;side+=2)
                {
                    var start=g.TransformPoint(new Vector3(side*.13f*scale,0,0));
                    var end=new Vector3(start.x+side*.2f,7.65f,start.z);
                    var cable=Asset("Suspension-"+i+"-"+side+".asset",()=>NegativeRitualGeometry.Cable(start,end));
                    MeshObject(contents,"Ritual / fixed suspension cable",cable,steel);
                }
                Tag(g,"Ritual / specimen tag","IV / 0"+(21+i),new Vector3(0,(1.3f-attachment)*scale,.30f*scale),.019f);
            }
            foreach(float z in new[]{-33.1f,-38.25f,-42.3f,-45.1f})
            {
                var d=MeshObject(contents,"Ritual / cast residue",debris,ivory);
                d.transform.position=new Vector3(z>-37?-7.1f:z<-44?-7.1f:-5.55f,.015f,z);
            }
            EditorSceneManager.MarkSceneDirty(scene);AssetDatabase.SaveAssets();Undo.CollapseUndoOperations(group);
            Debug.Log(DescribeOpenScene()+" Scene UNSAVED; no audio, lights or collisions added.",root);
        }
        catch{Undo.RevertAllDownToGroup(group);throw;}
    }
    static void Panel(Transform parent,string name,Vector3 point,Mesh surface,Mesh seam,string number,bool moving)
    {
        var p=new GameObject(name).transform;p.SetParent(parent,false);p.position=point;p.rotation=Quaternion.Euler(0,90,0);
        // Local +Z faces the aisle (+world X). Only relief depth is animated;
        // the backing and anchor stay fixed so a bulge has a visible cause.
        Box(p,"Ritual / rigid backing",new Vector3(0,0,-.035f),new Vector3(1.86f,2.76f,.08f),cloth);
        for(int side=-1;side<=1;side+=2)
        {
            Box(p,"Ritual / tension frame",new Vector3(side*.9f,0,.02f),new Vector3(.085f,2.82f,.14f),steel);
            Box(p,"Ritual / tension frame",new Vector3(0,side*1.36f,.02f),new Vector3(1.86f,.085f,.14f),steel);
        }
        string reliefName=name=="Ritual / close membrane"?"Ritual / close relief":name=="Ritual / impact panel"?"Ritual / impact relief":"Ritual / wall relief";
        var relief=MeshObject(p,reliefName,surface,membrane);
        if(moving)relief.transform.localScale=new Vector3(1,1,.14f);
        MeshObject(relief.transform,"Ritual / stretched stitching",seam,thread);
        Tag(p,"Ritual / cast plate",number,new Vector3(0,1.44f,.095f),.022f);
    }
    static Material Paint(string name,Color color,float metallic,float smoothness)
    {
        string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetColor("_Color",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);
        m.SetFloat("_Cull",0);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
        EditorUtility.SetDirty(m);return m;
    }
    static Mesh Asset(string file,Func<Mesh> create)
    {
        string path=Folder+"/"+file;var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(m){NegativeRitualGeometry.Validate(m);return m;}
        m=create();NegativeRitualGeometry.Validate(m);AssetDatabase.CreateAsset(m,path);return m;
    }
    static GameObject MeshObject(Transform parent,string name,Mesh mesh,Material material,ShadowCastingMode shadow=ShadowCastingMode.Off)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;
        var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadow;r.receiveShadows=true;return g;
    }
    static void Box(Transform parent,string name,Vector3 p,Vector3 scale,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);
        g.transform.localPosition=p;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());
        var r=g.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;
    }
    static void Tag(Transform parent,string name,string text,Vector3 position,float size)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localRotation=Quaternion.Euler(0,180,0);
        var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=80;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;
        t.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/Art/Folio/Fonts/IBMPlexSans-Medium.ttf");t.color=new Color(.68f,.64f,.5f);
        if(!t.font)throw new InvalidOperationException("Existing Plex font is required.");
        g.GetComponent<Renderer>().sharedMaterial=t.font.material;
        var d=g.AddComponent<DiegeticText>();var so=new SerializedObject(d);
        so.FindProperty("depthTestedShader").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Shaders/DiegeticText.shader");so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void EnsureFolder(string path)
    {var parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){if(!AssetDatabase.IsValidFolder(current+"/"+parts[i]))AssetDatabase.CreateFolder(current,parts[i]);current+="/"+parts[i];}}
    public static string DescribeOpenScene()
    {
        var scene=SceneManager.GetActiveScene();var g=scene.GetRootGameObjects().SingleOrDefault(x=>x.name==RootName);
        if(!g)return "Ritual environment absent.";
        var marker=g.GetComponentsInChildren<Transform>(true).SingleOrDefault(t=>t.name==Marker);
        if(!marker)return "Ritual environment marker absent.";
        var meshes=marker.GetComponentsInChildren<MeshFilter>(true).Select(m=>m.sharedMesh).Distinct().ToArray();
        foreach(var m in meshes)NegativeRitualGeometry.Validate(m);
        int renderers=marker.GetComponentsInChildren<Renderer>(true).Length;
        int colliders=marker.GetComponentsInChildren<Collider>(true).Length;
        if(colliders!=0||renderers>=120)throw new InvalidOperationException("Ritual environment exceeds its collision/render budget.");
        return "Ritual environment: "+renderers+" renderers, "+meshes.Length+" finite meshes, zero colliders; four framed impressions and three suspended adult casts.";
    }
}

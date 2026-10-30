using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Installs new original sculpture only. Runtime, audio, lights and the old
// membrane's visibility belong to the central unsealing sequence installer.
public static class NegativeUnsealingEnvironment
{
    public const string RootName="Negative / unsealing apparatus";
    const string Folder="Assets/_Game/Art/NegativeUnsealing";
    static Material ivory,veil,voidMaterial,restraint,inner,shroud;
    [MenuItem("Echo Protocol/El Negativo/Build unsealing apparatus")]
    public static void ApplyToOpenScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)
            throw new InvalidOperationException("Finish compilation and exit Play first.");
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="EchoProtocol_Negative")throw new InvalidOperationException("Open Negative; this installer does not open or save scenes.");
        if(scene.GetRootGameObjects().Any(g=>g.name==RootName))throw new InvalidOperationException("Unsealing sculpture already exists; edit that installation.");
        EnsureFolder(Folder);
        ivory=Paint("Pressed ivory",new Color(.53f,.435f,.31f),.015f,.42f);
        veil=Paint("Torn wax membrane",new Color(.28f,.19f,.13f),.02f,.55f);
        voidMaterial=Paint("Depth of chamber",new Color(.026f,.014f,.011f),.02f,.20f);
        restraint=Paint("Oxidized restraint",new Color(.19f,.12f,.058f),.68f,.42f);
        inner=Paint("Dark embedded closure",new Color(.105f,.025f,.018f),.05f,.50f);
        shroud=Paint("Tensioned mortuary cloth",new Color(.31f,.255f,.19f),.015f,.36f);
        Mesh body=Asset("Captive-adult.asset",NegativeUnsealingGeometry.CaptiveTorso);
        Mesh head=Asset("Captive-head.asset",NegativeUnsealingGeometry.CaptiveHead);
        Mesh leftHand=Asset("Adult-left-hand.asset",()=>NegativeUnsealingGeometry.Hand(-1));
        Mesh rightHand=Asset("Adult-right-hand.asset",()=>NegativeUnsealingGeometry.Hand(1));
        Mesh bindings=Asset("Captive-bindings.asset",NegativeUnsealingGeometry.CaptiveBindings);
        Mesh leftVeil=Asset("Left-veil.asset",()=>NegativeUnsealingGeometry.Veil(-1));
        Mesh rightVeil=Asset("Right-veil.asset",()=>NegativeUnsealingGeometry.Veil(1));
        Mesh leftEdge=Asset("Left-torn-edge.asset",()=>NegativeUnsealingGeometry.VeilEdge(-1));
        Mesh rightEdge=Asset("Right-torn-edge.asset",()=>NegativeUnsealingGeometry.VeilEdge(1));
        Mesh hanging=Asset("Suspended-adult.asset",NegativeUnsealingGeometry.HangingBody);
        Mesh feet=Asset("Suspended-feet.asset",NegativeUnsealingGeometry.HangingFeet);
        Mesh harness=Asset("Suspended-harness.asset",NegativeUnsealingGeometry.HangingBindings);
        Mesh cable=Asset("Lowering-unit-cable.asset",NegativeUnsealingGeometry.Cable);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Create unsealing sculpture");
        try
        {
            var root=Node(null,RootName,Vector3.zero);
            Undo.RegisterCreatedObjectUndo(root.gameObject,"Create unsealing sculpture");
            var chamber=Node(root,"Unsealing / chamber",new Vector3(-5.45f,1.8f,-42.3f));
            chamber.localRotation=Quaternion.Euler(0,90,0);
            // The old rigid backing may remain: all visible geometry lies in
            // front of it. Root only needs to hide the old impact relief mesh.
            Box(chamber,"Unsealing / cavity back",new Vector3(0,0,.035f),new Vector3(1.62f,2.53f,.045f),voidMaterial);
            foreach(int side in new[]{-1,1})
            {
                Box(chamber,"Unsealing / inner side",new Vector3(side*.795f,0,.145f),new Vector3(.055f,2.51f,.26f),inner);
                Box(chamber,"Unsealing / inner sill",new Vector3(0,side*1.235f,.145f),new Vector3(1.62f,.05f,.26f),inner);
                Box(chamber,"Unsealing / cast retention bar",new Vector3(side*.60f,.35f,.14f),new Vector3(.025f,1.35f,.055f),restraint);
            }
            var captive=Node(chamber,"Unsealing / captive",new Vector3(0,-.05f,.13f));
            MeshObject(captive,"Unsealing / contracted torso and limbs",body,ivory,ShadowCastingMode.On);
            MeshObject(captive,"Unsealing / torso restraints",bindings,inner);
            var neck=Node(captive,"Unsealing / captive head",new Vector3(0,.64f,.06f));
            neck.localRotation=Quaternion.Euler(0,-15,-12);
            MeshObject(neck,"Unsealing / blank adult casting",head,ivory,ShadowCastingMode.On);
            var lh=Node(captive,"Unsealing / captive left hand",new Vector3(-.48f,.44f,.115f));
            lh.localRotation=Quaternion.Euler(0,-6,12);MeshObject(lh,"Unsealing / exposed left fingers",leftHand,ivory);
            var rh=Node(captive,"Unsealing / captive right hand",new Vector3(.49f,.41f,.115f));
            rh.localRotation=Quaternion.Euler(0,5,-9);MeshObject(rh,"Unsealing / exposed right fingers",rightHand,ivory);
            var lv=Node(chamber,"Unsealing / left veil",new Vector3(-.82f,0,.30f));
            MeshObject(lv,"Unsealing / left membrane",leftVeil,veil);MeshObject(lv,"Unsealing / left cut filaments",leftEdge,inner);
            var rv=Node(chamber,"Unsealing / right veil",new Vector3(.82f,0,.30f));
            MeshObject(rv,"Unsealing / right membrane",rightVeil,veil);MeshObject(rv,"Unsealing / right cut filaments",rightEdge,inner);

            // Two feet end at y4.204 in the saved scene; a 1.85m descent ends
            // above y2.35. The cable is separate, fixed at the ceiling, and its
            // unit down-axis is stretched by the runtime owner with the drop.
            var suspension=Node(root,"Unsealing / hanging body",new Vector3(1.15f,6.55f,-43.6f));
            // Roll is expressed by individual anatomy, not the whole root, to
            // retain the exact vertical clearance contract during a drop.
            suspension.localRotation=Quaternion.Euler(0,-40,0);
            MeshObject(suspension,"Unsealing / hanging adult shroud",hanging,shroud,ShadowCastingMode.On);
            MeshObject(suspension,"Unsealing / harness and chest stitches",harness,restraint);
            MeshObject(suspension,"Unsealing / bare adult feet",feet,ivory);
            var handA=Node(suspension,"Unsealing / hanging left hand",new Vector3(.14f,-.99f,.20f));
            handA.localRotation=Quaternion.Euler(0,0,-95);handA.localScale=Vector3.one*.88f;
            MeshObject(handA,"Unsealing / hanging left fingers",leftHand,ivory);
            var handB=Node(suspension,"Unsealing / hanging right hand",new Vector3(-.08f,-1.40f,.20f));
            handB.localRotation=Quaternion.Euler(0,0,95);handB.localScale=Vector3.one*.88f;
            MeshObject(handB,"Unsealing / hanging right fingers",rightHand,ivory);
            var cord=Node(root,"Unsealing / lowering cable",new Vector3(1.15f,7.65f,-43.6f));
            cord.localScale=new Vector3(1,1.10f,1);MeshObject(cord,"Unsealing / loaded suspension cable",cable,restraint);
            Box(root,"Unsealing / ceiling winch socket",new Vector3(1.15f,7.69f,-43.6f),new Vector3(.28f,.1f,.28f),restraint);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log(DescribeOpenScene()+" Scene UNSAVED; existing panel, audio and lighting not modified.",root);
        }
        catch{Undo.RevertAllDownToGroup(group);throw;}
    }
    public static string DescribeOpenScene()
    {
        var scene=SceneManager.GetActiveScene();var root=scene.GetRootGameObjects().SingleOrDefault(g=>g.name==RootName);
        if(!root)return "Unsealing absent.";
        var meshes=root.GetComponentsInChildren<MeshFilter>(true).Select(m=>m.sharedMesh).Distinct().ToArray();
        foreach(var mesh in meshes)NegativeUnsealingGeometry.Validate(mesh);
        var renderers=root.GetComponentsInChildren<Renderer>(true);
        int colliders=root.GetComponentsInChildren<Collider>(true).Length,shadows=renderers.Count(r=>r.shadowCastingMode!=ShadowCastingMode.Off);
        var chamber=root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Unsealing / chamber");
        float front=chamber.GetComponentsInChildren<Renderer>(true).Max(r=>r.bounds.max.x)-chamber.position.x;
        var hanging=root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Unsealing / hanging body");
        float lowest=hanging.GetComponentsInChildren<Renderer>(true).Min(r=>r.bounds.min.y);
        if(colliders!=0||renderers.Length>=65||shadows>3||front>.4f||lowest-hanging.position.y < -2.36f)throw new InvalidOperationException("Unsealing sculpture exceeds geometry/clearance contract.");
        return "Unsealing: "+renderers.Length+" renderers, "+meshes.Length+" finite meshes, "+shadows+" shadow casters, zero colliders; panel front "+front.ToString("F3")+"m; saved feet y"+lowest.ToString("F3")+".";
    }
    static Transform Node(Transform parent,string name,Vector3 position)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
    static void MeshObject(Transform parent,string name,Mesh mesh,Material material,ShadowCastingMode shadow=ShadowCastingMode.Off)
    {var t=Node(parent,name,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadow;r.receiveShadows=true;}
    static void Box(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());var r=g.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;}
    static Material Paint(string name,Color color,float metal,float smooth)
    {
        string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Cull",0);
        EditorUtility.SetDirty(m);return m;
    }
    static Mesh Asset(string name,Func<Mesh> create)
    {string path=Folder+"/"+name;var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m){NegativeUnsealingGeometry.Validate(m);return m;}m=create();NegativeUnsealingGeometry.Validate(m);AssetDatabase.CreateAsset(m,path);return m;}
    static void EnsureFolder(string path)
    {var parts=path.Split('/');string at=parts[0];for(int i=1;i<parts.Length;i++){if(!AssetDatabase.IsValidFolder(at+"/"+parts[i]))AssetDatabase.CreateFolder(at,parts[i]);at+="/"+parts[i];}}
}

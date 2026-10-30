using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Adds original geometry to existing present-world sculptures. The runtime
// installer owns the new pivots. This tool never opens/saves a scene or plays.
public static class NegativeAutopsyEnvironment
{
    const string Folder="Assets/_Game/Art/NegativeAutopsy";
    const string Marker="Negative / autopsy apparatus";
    const string HangingGroup="Autopsy / hanging anatomy";
    const string CaptiveGroup="Autopsy / captive anatomy";
    static Material cloth,skin,bone,tissue,dark,thread,metal;
    [MenuItem("Echo Protocol/El Negativo/Build autopsy anatomy")]
    public static void ApplyToOpenScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)
            throw new InvalidOperationException("Finish compilation and exit Play before installing anatomy.");
        var scene=SceneManager.GetActiveScene();
        if(scene.name!="EchoProtocol_Negative")throw new InvalidOperationException("Open Negative; this tool does not open or save it.");
        if(All<Transform>().Any(t=>t.name==Marker))throw new InvalidOperationException("Autopsy apparatus already installed. Edit that installation.");
        var hanging=Named("Unsealing / hanging body");
        var captiveHead=Named("Unsealing / captive head");
        var oldBody=Named("Unsealing / hanging adult shroud").GetComponent<Renderer>();
        var oldHarness=Named("Unsealing / harness and chest stitches").GetComponent<Renderer>();
        var oldFace=Named("Unsealing / blank adult casting").GetComponent<Renderer>();
        var leftHand=Named("Unsealing / hanging left hand");
        var rightHand=Named("Unsealing / hanging right hand");
        if(!oldBody||!oldHarness||!oldFace)throw new InvalidOperationException("Expected current Unsealing renderers.");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/NegativeUnsealing/WaxedMortuaryCanvas.png");
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        if(!texture||!shader)throw new InvalidOperationException("Requires the saved original mortuary canvas and URP Lit.");
        EnsureFolder(Folder);
        cloth=Paint("Opened waxed shroud",new Color(.40f,.30f,.22f),.34f,texture);
        skin=Paint("Desiccated adult surface",new Color(.59f,.46f,.32f),.39f,texture);
        bone=Paint("Exposed old ivory",new Color(.83f,.72f,.53f),.32f,texture);
        tissue=Paint("Recessed thoracic tissue",new Color(.23f,.038f,.027f),.47f,texture);
        dark=Paint("Deep orbital and thoracic cavity",new Color(.012f,.007f,.005f),.16f,null);
        thread=Paint("Stretched dark sutures",new Color(.11f,.029f,.018f),.41f,null);
        metal=Paint("Old load bearing binding",new Color(.17f,.12f,.063f),.45f,null);metal.SetFloat("_Metallic",.55f);EditorUtility.SetDirty(metal);
        Mesh back=Asset("Retained-shroud.asset",NegativeAutopsyGeometry.BackAndLowerShroud);
        Mesh cavity=Asset("Thoracic-cavity.asset",NegativeAutopsyGeometry.ChestCavity);
        Mesh flesh=Asset("Recessed-tissue.asset",NegativeAutopsyGeometry.RecessedTissue);
        Mesh ribs=Asset("Twelve-ribs.asset",NegativeAutopsyGeometry.Ribs);
        Mesh harness=Asset("Open-chest-harness.asset",NegativeAutopsyGeometry.NewHarness);
        Mesh leftFlap=Asset("Left-chest-flap.asset",()=>NegativeAutopsyGeometry.ChestFlap(-1));
        Mesh rightFlap=Asset("Right-chest-flap.asset",()=>NegativeAutopsyGeometry.ChestFlap(1));
        Mesh leftSleeve=Asset("Left-bound-arm.asset",()=>NegativeAutopsyGeometry.FlapSleeve(-1));
        Mesh rightSleeve=Asset("Right-bound-arm.asset",()=>NegativeAutopsyGeometry.FlapSleeve(1));
        Mesh leftThread=Asset("Left-cut-sutures.asset",()=>NegativeAutopsyGeometry.TornSutures(-1));
        Mesh rightThread=Asset("Right-cut-sutures.asset",()=>NegativeAutopsyGeometry.TornSutures(1));
        Mesh face=Asset("Adult-orbital-face.asset",NegativeAutopsyGeometry.AdultFace);
        Mesh sockets=Asset("Deep-face-cavities.asset",NegativeAutopsyGeometry.FaceCavities);
        Mesh jaw=Asset("Hinged-adult-mandible.asset",NegativeAutopsyGeometry.LowerJaw);
        Mesh upperTeeth=Asset("Upper-irregular-teeth.asset",()=>NegativeAutopsyGeometry.Teeth(true));
        Mesh lowerTeeth=Asset("Lower-irregular-teeth.asset",()=>NegativeAutopsyGeometry.Teeth(false));
        Mesh neck=Asset("Adult-neck.asset",NegativeAutopsyGeometry.Neck);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Build present autopsy anatomy");
        try
        {
            var marker=Node(null,Marker,Vector3.zero);Undo.RegisterCreatedObjectUndo(marker.gameObject,"Autopsy marker");
            var anatomy=Node(hanging,HangingGroup,Vector3.zero);Undo.RegisterCreatedObjectUndo(anatomy.gameObject,"Hanging anatomy");
            var captive=Node(captiveHead,CaptiveGroup,Vector3.zero);Undo.RegisterCreatedObjectUndo(captive.gameObject,"Captive anatomy");
            foreach(var r in new[]{oldBody,oldHarness,oldFace}){Undo.RecordObject(r,"Replace cast surface");r.enabled=false;EditorUtility.SetDirty(r);}
            MeshObject(anatomy,"Autopsy / retained back and legs",back,cloth);
            MeshObject(anatomy,"Autopsy / thoracic depth",cavity,dark);
            MeshObject(anatomy,"Autopsy / recessed tissue",flesh,tissue);
            MeshObject(anatomy,"Autopsy / exposed rib cage",ribs,bone,ShadowCastingMode.On);
            MeshObject(anatomy,"Autopsy / upper harness and lower bands",harness,metal);
            var left=Node(anatomy,"Autopsy / left chest flap",NegativeAutopsyGeometry.FlapPivot(-1));
            var right=Node(anatomy,"Autopsy / right chest flap",NegativeAutopsyGeometry.FlapPivot(1));
            MeshObject(left,"Autopsy / left heavy chest flap",leftFlap,cloth);
            MeshObject(right,"Autopsy / right heavy chest flap",rightFlap,cloth);
            MeshObject(left,"Autopsy / left wrapped forearm",leftSleeve,cloth);
            MeshObject(right,"Autopsy / right wrapped forearm",rightSleeve,cloth);
            MeshObject(left,"Autopsy / left loose sutures",leftThread,thread);
            MeshObject(right,"Autopsy / right loose sutures",rightThread,thread);
            // Preserve the original fingers. Their folded arms and cloth open
            // together, revealing the cavity without leaving unsupported hands.
            Undo.SetTransformParent(leftHand,left,"Attach preserved hand to left flap");
            Undo.SetTransformParent(rightHand,right,"Attach preserved hand to right flap");
            var hangingNeck=Node(anatomy,"Autopsy / hanging neck",new Vector3(-.04f,-.64f,.04f));
            hangingNeck.localRotation=Quaternion.Euler(22,0,-12);
            AddHead(hangingNeck,"Autopsy / hanging jaw",face,sockets,jaw,upperTeeth,lowerTeeth,neck,true);
            // The current captive head still owns the motion; its child jaw has
            // a separate anatomical hinge in that unchanged local coordinate frame.
            AddHead(captive,"Autopsy / captive jaw",face,sockets,jaw,upperTeeth,lowerTeeth,neck,false);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log(DescribeOpenScene()+" Scene UNSAVED. Chest/head/jaw animation remains the runtime owner's responsibility.",marker);
        }
        catch{Undo.RevertAllDownToGroup(group);throw;}
    }
    static void AddHead(Transform parent,string jawName,Mesh face,Mesh sockets,Mesh jaw,Mesh top,Mesh bottom,Mesh neck,bool castShadow)
    {
        MeshObject(parent,"Autopsy / exposed adult neck",neck,skin);
        MeshObject(parent,"Autopsy / profiled adult face",face,skin,castShadow?ShadowCastingMode.On:ShadowCastingMode.Off);
        MeshObject(parent,"Autopsy / recessed orbits and mouth",sockets,dark);
        MeshObject(parent,"Autopsy / upper irregular teeth",top,bone);
        var hinge=Node(parent,jawName,new Vector3(0,.075f,.015f));
        hinge.localRotation=Quaternion.Euler(3,0,0);
        MeshObject(hinge,"Autopsy / lower adult jaw",jaw,skin);
        MeshObject(hinge,"Autopsy / lower irregular teeth",bottom,bone);
    }
    public static string DescribeOpenScene()
    {
        var groups=All<Transform>().Where(t=>t.name==HangingGroup||t.name==CaptiveGroup).ToArray();
        if(groups.Length!=2)throw new InvalidOperationException("Expected both installed anatomy groups.");
        var filters=groups.SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).ToArray();
        var meshes=filters.Select(f=>f.sharedMesh).Distinct().ToArray();foreach(var mesh in meshes)NegativeAutopsyGeometry.Validate(mesh);
        var renderers=groups.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).ToArray();
        int colliders=groups.Sum(g=>g.GetComponentsInChildren<Collider>(true).Length);
        int shadows=renderers.Count(r=>r.enabled&&r.shadowCastingMode!=ShadowCastingMode.Off);
        if(renderers.Length>=50||colliders!=0||shadows>2)throw new InvalidOperationException("Autopsy anatomy exceeds its renderer/shadow/collision contract.");
        return "Autopsy anatomy: "+renderers.Length+" renderers (including two preserved hands), "+meshes.Length+" finite meshes, "+shadows+" shadow casters, zero colliders; twelve ribs, two chest flaps and two hinged adult jaws.";
    }
    static T[] All<T>()where T:Component=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Transform Named(string name)=>All<Transform>().Single(t=>t.name==name);
    static Transform Node(Transform parent,string name,Vector3 position)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
    static void MeshObject(Transform parent,string name,Mesh mesh,Material material,ShadowCastingMode shadow=ShadowCastingMode.Off)
    {var t=Node(parent,name,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadow;r.receiveShadows=true;}
    static Material Paint(string name,Color color,float smooth,Texture2D texture)
    {string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",0);m.SetFloat("_Cull",0);m.SetTexture("_BaseMap",texture);m.SetTextureScale("_BaseMap",Vector2.one);EditorUtility.SetDirty(m);return m;}
    static Mesh Asset(string name,Func<Mesh> make)
    {string path=Folder+"/"+name;var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m){NegativeAutopsyGeometry.Validate(m);return m;}m=make();NegativeAutopsyGeometry.Validate(m);AssetDatabase.CreateAsset(m,path);return m;}
    static void EnsureFolder(string path)
    {var parts=path.Split('/');string at=parts[0];for(int i=1;i<parts.Length;i++){if(!AssetDatabase.IsValidFolder(at+"/"+parts[i]))AssetDatabase.CreateFolder(at,parts[i]);at+="/"+parts[i];}}
}

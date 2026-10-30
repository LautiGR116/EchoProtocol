using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class NegativeUnsealingMaskIntegration
{
    const string Folder="Assets/_Game/Art/Embodiment/RenderpeoplePrivate/NegativeIntruder/";
    const string RootName="Unsealing / ceramic plates and recessed core";

    [MenuItem("Echo Protocol/El Negativo/Apply unsealing mask to present actor")]
    public static void Apply()
    {
        var scene=SceneManager.GetActiveScene();
        Require(!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling,
            "Finish compilation and exit Play before configuring the mask.");
        Require(scene.IsValid()&&scene.isLoaded&&scene.name=="EchoProtocol_Negative",
            "Requires the already open Negative scene. This installer never opens or saves it.");
        var identity=All<NegativeIntruderIdentity>().Single();
        var performance=identity.GetComponent<MachineThreatPerformance>();
        var rush=identity.GetComponent<EmbestidaRush>();
        Require(identity.revision==2&&identity.silhouette&&performance&&rush,"Requires the validated V2 present identity and actual contact owner.");
        var fields=new SerializedObject(performance);
        var anatomy=fields.FindProperty("anatomy").objectReferenceValue as RecordedCharacterMotion;
        var head=fields.FindProperty("head").objectReferenceValue as Transform;
        Require(anatomy&&anatomy.IsPresentThreat&&head&&!anatomy.GetComponentInParent<EchoPlayback>()
            &&!anatomy.GetComponentInParent<LoopActor>(),"Only the present actor may receive a split mask.");
        var previous=identity.silhouette.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r=>r.name=="Negative / ivory shell");
        NegativeIntruderGeometry.Validate(previous.sharedMesh);
        int index=Array.IndexOf(previous.bones,head);
        Require(index>=0&&previous.sharedMesh.boneWeights.All(w=>w.boneIndex0==index&&w.weight0>.9999f
            &&w.weight1==0&&w.weight2==0&&w.weight3==0),"The V2 shell must be rigidly weighted to Head.");
        if(identity.GetComponent<NegativeUnsealingMask>())
        {
            Debug.Log(DescribeOpenScene());return;
        }
        Require(!head.GetComponentsInChildren<Transform>(true).Any(t=>t.name==RootName),"Preserve an incomplete unsealing hierarchy for review.");
        var matrix=previous.sharedMesh.bindposes[index];
        Require(Mathf.Abs(matrix.determinant)>.000001f,"The Head bind transform must be invertible.");
        // Bone-to-world * bind * vertex is exactly the one-bone skin equation.
        // Rigid child vertices use bind*vertex and therefore retain the same
        // closed silhouette for every future head pose and root transform.
        var left=Asset("UnsealingLeft-v4.asset",()=>NegativeUnsealingMaskGeometry.Plate(matrix,true));
        var right=Asset("UnsealingRight-v4.asset",()=>NegativeUnsealingMaskGeometry.Plate(matrix,false));
        var cavity=Asset("UnsealingCavity-v4.asset",()=>NegativeUnsealingMaskGeometry.Cavity(matrix));
        var fibers=Asset("UnsealingFibers-v4.asset",()=>NegativeUnsealingMaskGeometry.Fibers(matrix));
        var dark=MaterialAsset("UnsealingInterior-v4.mat",new Color(.035f,.008f,.005f),.34f);
        var thread=MaterialAsset("UnsealingFibers-v4.mat",new Color(.27f,.085f,.04f),.48f);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Install contact-led ceramic fracture");
        try
        {
            var root=new GameObject(RootName).transform;root.SetParent(head,false);Undo.RegisterCreatedObjectUndo(root.gameObject,"Install split mask");
            var lp=New(root,"Unsealing / left ceramic hinge",NegativeUnsealingMaskGeometry.Pivot(matrix,true));
            var rp=New(root,"Unsealing / right ceramic hinge",NegativeUnsealingMaskGeometry.Pivot(matrix,false));
            Surface(lp,"Unsealing / left ceramic plate",left,previous.sharedMaterial);
            Surface(rp,"Unsealing / right ceramic plate",right,previous.sharedMaterial);
            Surface(root,"Unsealing / recessed interior",cavity,dark);
            Surface(root,"Unsealing / inner binding fibers",fibers,thread);
            var mask=Undo.AddComponent<NegativeUnsealingMask>(identity.gameObject);
            var so=new SerializedObject(mask);
            Ref(so,"session",All<GameSession>().Single());Ref(so,"encounter",All<MachineCoverEncounter>().Single());
            Ref(so,"rush",rush);Ref(so,"output",All<SessionAudio>().Single());Ref(so,"leftPlate",lp);Ref(so,"rightPlate",rp);
            so.FindProperty("hingeAxis").vector3Value=matrix.MultiplyVector(Vector3.up).normalized;
            so.FindProperty("leftOpenOffset").vector3Value=matrix.MultiplyVector(new Vector3(-.008f,-.009f,0));
            so.FindProperty("rightOpenOffset").vector3Value=matrix.MultiplyVector(new Vector3(.006f,.005f,0));
            so.FindProperty("contactDelay").floatValue=.14f;so.FindProperty("openingSeconds").floatValue=.12f;
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RecordObject(previous,"Preserve V2 shell behind split replacement");previous.enabled=false;
            EditorUtility.SetDirty(previous);PrefabUtility.RecordPrefabInstancePropertyModifications(previous);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log(DescribeOpenScene()+" Scene UNSAVED. Optional fracture source is assigned by the central audio integration.",mask);
        }
        catch { Undo.RevertAllDownToGroup(group);throw; }
    }

    public static string DescribeOpenScene()
    {
        var root=All<Transform>().SingleOrDefault(t=>t.name==RootName);
        if(!root)return "Unsealing mask absent.";
        foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))NegativeUnsealingMaskGeometry.Validate(f.sharedMesh);
        Require(root.GetComponentsInChildren<Collider>(true).Length==0,"The split surface must not add physical obstacles.");
        return "Unsealing mask: four finite rigid Head-local meshes, no colliders, 38/32-degree hinges; actual contact +0.14s starts a 0.12s opening.";
    }
    static Transform New(Transform parent,string name,Vector3 p)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;return g.transform;}
    static void Surface(Transform parent,string name,Mesh mesh,Material material)
    {
        var t=New(parent,name,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
    }
    static Mesh Asset(string name,Func<Mesh> create)
    {
        string path=Folder+name;var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(old){NegativeUnsealingMaskGeometry.Validate(old);return old;}
        Require(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)),"Preserve an incompatible asset: "+path);
        var mesh=create();NegativeUnsealingMaskGeometry.Validate(mesh);AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static Material MaterialAsset(string name,Color color,float smooth)
    {
        string path=Folder+name;var old=AssetDatabase.LoadAssetAtPath<Material>(path);if(old)return old;
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);
        m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Cull",0);AssetDatabase.CreateAsset(m,path);return m;
    }
    static T[] All<T>()where T:Component=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static void Ref(SerializedObject so,string name,Object value)=>so.FindProperty(name).objectReferenceValue=value;
    static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}

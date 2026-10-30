using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Opt-in present-actor replacement. No auto execution, scene loading/saving,
// source-scan edits, Echo edits, package download or audio/loudness changes.
public static class NegativeIntruderIntegration
{
    const string Output="Assets/_Game/Art/Embodiment/RenderpeoplePrivate/NegativeIntruder/";
    const int Revision=2;

    [MenuItem("Echo Protocol/El Negativo/Apply present intruder")]
    public static void Apply()
    {
        var scene=SceneManager.GetActiveScene();
        Require(!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling,"Stop Play and finish import first.");
        Require(scene.IsValid()&&scene.isLoaded&&(scene.path.EndsWith("/EchoProtocol_Incursion.unity",StringComparison.Ordinal)
            ||scene.path.EndsWith("/EchoProtocol_Negative.unity",StringComparison.Ordinal)),"Use the open Incursion or Negative scene; this tool never opens/saves it.");
        var threats=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MachineCoverThreat>(true)).ToArray();
        Require(threats.Length==1,"Expected exactly one present intruder.");
        var threat=threats[0];var performance=threat.GetComponent<MachineThreatPerformance>();
        Require(performance,"Missing present performance.");
        var p=new SerializedObject(performance);
        var anatomy=Ref<RecordedCharacterMotion>(p,"anatomy");
        Require(anatomy&&anatomy.IsPresentThreat&&!anatomy.GetComponentInParent<EchoPlayback>()
            &&!anatomy.GetComponentInParent<LoopActor>(),"Never apply the Negative identity to a recorded body.");
        var surfaces=threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r=>r.name=="RecordedAdultSurface").ToArray();
        Require(surfaces.Length==1,"Expected one original present surface.");
        var surface=surfaces[0];var prior=threat.GetComponent<NegativeIntruderIdentity>();
        if(prior)
        {
            if(prior.revision==1){RepairGeneratedGeometry();return;}
            Require(prior.revision==Revision&&prior.body==surface&&prior.silhouette,"Existing Negative installation is incomplete; preserve it for review.");
            NegativeIntruderGeometry.Validate(surface.sharedMesh);
            foreach(var renderer in prior.silhouette.GetComponentsInChildren<SkinnedMeshRenderer>(true))NegativeIntruderGeometry.Validate(renderer.sharedMesh);
            Debug.Log("El Negativo already installed; saved geometry and scene composition preserved.",threat);return;
        }
        // Native saved meshes disable runtime Read/Write; outside Play the
        // Editor can still read these streams. Never change the source flag.
        Require(surface.sharedMesh&&surface.bones.All(b=>b)
            &&surface.bones.Length==surface.sharedMesh.bindposes.Length,"Licensed Nathan bind data required.");
        var rig=RigFor(surface,performance,surface.sharedMesh);
        var shader=Shader.Find("Universal Render Pipeline/Lit");Require(shader,"URP Lit shader required.");
        EnsureFolder(Output.TrimEnd('/'));
        // All generated bind data and retained scan triangles stay private. New
        // objects/meshes are assigned rather than rewriting a shared Face asset.
        var body=Asset("HandsAndBoots-v2.asset",()=>NegativeIntruderGeometry.HandsAndBoots(surface.sharedMesh));
        var mask=Asset("Mask-v2.asset",()=>NegativeIntruderGeometry.Mask(rig));
        var hood=Asset("Hood-v2.asset",()=>NegativeIntruderGeometry.Hood(rig));
        var coat=Asset("Coat-v2.asset",()=>NegativeIntruderGeometry.Coat(rig));
        var fingers=Asset("Fingertips-v2.asset",()=>NegativeIntruderGeometry.Fingers(rig));
        var clothTexture=Asset("CoatedFabric.asset",()=>Texture(false));
        var maskTexture=Asset("CastIvory.asset",()=>Texture(true));
        var cloth=Asset("CoatedBlack.mat",()=>Material(shader,"Negative / coated black fabric",new Color(.050f,.057f,.052f),.25f,clothTexture,true));
        var ivory=Asset("IvoryShell.mat",()=>Material(shader,"Negative / aged ivory shell",new Color(.79f,.755f,.64f),.31f,maskTexture,false));
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Install El Negativo silhouette");
        GameObject root=null;
        try
        {
            root=new GameObject("El Negativo / closed mask and articulated coat");root.transform.SetParent(surface.transform,false);
            Surface(root.transform,"Negative / ivory shell",mask,ivory,surface);
            Surface(root.transform,"Negative / closed cowl",hood,cloth,surface);
            Surface(root.transform,"Negative / split heavy coat",coat,cloth,surface);
            Surface(root.transform,"Negative / articulated ivory fingertips",fingers,ivory,surface);
            Undo.RegisterCreatedObjectUndo(root,"Install El Negativo silhouette");
            var receipt=Undo.AddComponent<NegativeIntruderIdentity>(threat.gameObject);
            receipt.revision=Revision;receipt.body=surface;receipt.priorBody=surface.sharedMesh;
            receipt.priorMaterials=surface.sharedMaterials;receipt.silhouette=root;
            Undo.RecordObject(surface,"Conceal present head and everyday clothing");surface.sharedMesh=body;
            foreach(var old in threat.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Embestida / torn rubber bib"))
            {Undo.RecordObject(old.gameObject,"Hide superseded bib");old.gameObject.SetActive(false);}
            Undo.RecordObject(performance,"Set Negative present posture");
            p.FindProperty("negativeIdentity").objectReferenceValue=receipt;p.ApplyModifiedProperties();
            EditorUtility.SetDirty(receipt);PrefabUtility.RecordPrefabInstancePropertyModifications(surface);
            PrefabUtility.RecordPrefabInstancePropertyModifications(performance);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log("El Negativo installed; scene UNSAVED. Closed ivory shell, black cowl, continuous coat with sleeves/tails and articulated fingertips. "
                +"Private bind-space assets saved; original mesh/materials retained in receipt. Actor root, feet, collisions, Echo and audio unchanged. "
                +"Review one near view and actual movement centrally before saving.",threat);
        }
        catch{Undo.RevertAllDownToGroup(group);if(root)Object.DestroyImmediate(root);throw;}
    }

    static NegativeIntruderGeometry.Rig RigFor(SkinnedMeshRenderer surface,MachineThreatPerformance performance,Mesh source)
    {
        NegativeIntruderGeometry.Validate(source);
        var p=new SerializedObject(performance);
        var rig=new NegativeIntruderGeometry.Rig{binds=source.bindposes,bones=surface.bones};
        int Bone(string field)
        {
            var t=Ref<Transform>(p,field);int i=Array.IndexOf(rig.bones,t);
            Require(t&&i>=0,"Missing present bone: "+field);return i;
        }
        rig.hip=Bone("hips");rig.spine=Bone("spine");rig.chest=Bone("chest");rig.neck=Bone("neck");rig.head=Bone("head");
        rig.la=Bone("leftArm");rig.le=Bone("leftForearm");rig.lh=Bone("leftHand");
        rig.ra=Bone("rightArm");rig.re=Bone("rightForearm");rig.rh=Bone("rightHand");rig.lt=Bone("leftThigh");rig.rt=Bone("rightThigh");
        return rig;
    }

    [MenuItem("Echo Protocol/El Negativo/Repair generated geometry before Play")]
    public static void RepairGeneratedGeometry()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling,"Repair outside Play, after import.");
        var scene=SceneManager.GetActiveScene();
        Require(scene.IsValid()&&scene.isLoaded&&(scene.path.EndsWith("/EchoProtocol_Incursion.unity",StringComparison.Ordinal)
            ||scene.path.EndsWith("/EchoProtocol_Negative.unity",StringComparison.Ordinal)),"Repair only the open Incursion/Negative scene.");
        var receipts=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<NegativeIntruderIdentity>(true)).ToArray();
        Require(receipts.Length==1,"Expected one Negative receipt; no scene was changed.");
        var receipt=receipts[0];var body=receipt.body;var performance=receipt.GetComponent<MachineThreatPerformance>();
        Require(body&&receipt.priorBody&&receipt.silhouette&&performance&&receipt.GetComponent<MachineCoverThreat>()
            &&!receipt.GetComponentInParent<EchoPlayback>(),"Missing original present-actor recovery references.");
        var rig=RigFor(body,performance,receipt.priorBody);
        var replacements=new[]
        {
            Asset("Mask-v2.asset",()=>NegativeIntruderGeometry.Mask(rig)),
            Asset("Hood-v2.asset",()=>NegativeIntruderGeometry.Hood(rig)),
            Asset("Coat-v2.asset",()=>NegativeIntruderGeometry.Coat(rig)),
            Asset("Fingertips-v2.asset",()=>NegativeIntruderGeometry.Fingers(rig))
        };
        var names=new[]{"Negative / ivory shell","Negative / closed cowl","Negative / split heavy coat","Negative / articulated ivory fingertips"};
        var all=receipt.silhouette.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var targets=names.Select(name=>all.Single(r=>r.name==name)).ToArray();
        var retained=Asset("HandsAndBoots-v2.asset",()=>NegativeIntruderGeometry.HandsAndBoots(receipt.priorBody));
        // Assign new validated native meshes. Never CopySerialized into the
        // broken v1 skin buffers, enable the actor, or mutate the source scan.
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Repair Negative mesh buffers");
        try
        {
            Undo.RecordObject(body,"Replace retained body buffer");body.sharedMesh=retained;
            for(int i=0;i<targets.Length;i++)
            {
                Undo.RecordObject(targets[i],"Replace Negative mesh buffer");targets[i].sharedMesh=replacements[i];
                var bounds=body.localBounds;bounds.Encapsulate(replacements[i].bounds);bounds.Expand(.24f);targets[i].localBounds=bounds;
                PrefabUtility.RecordPrefabInstancePropertyModifications(targets[i]);
            }
            Undo.RecordObject(receipt,"Validate repaired Negative geometry");receipt.revision=Revision;
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log("Negative v2 geometry validated and rebound to five fresh meshes. Actor remains in its prior active state; scene UNSAVED. "
                +"Unreferenced v1 generated assets retained; source scan and original recovery mesh untouched. Save before Play.",receipt);
        }
        catch{Undo.RevertAllDownToGroup(group);throw;}
    }

    static void Surface(Transform parent,string name,Mesh mesh,Material material,SkinnedMeshRenderer source)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);
        var r=go.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=mesh;r.sharedMaterial=material;
        r.bones=source.bones;r.rootBone=source.rootBone;r.quality=SkinQuality.Bone4;
        r.updateWhenOffscreen=true;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
        var bounds=source.localBounds;bounds.Encapsulate(mesh.bounds);bounds.Expand(.24f);r.localBounds=bounds;
    }
    static Material Material(Shader shader,string name,Color color,float smoothness,Texture2D map,bool cloth)
    {
        var m=new Material(shader){name=name};m.SetColor("_BaseColor",color);m.SetTexture("_BaseMap",map);
        m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Metallic",0);m.SetFloat("_Cull",cloth?0:2);
        return m;
    }
    static Texture2D Texture(bool ivory)
    {
        const int size=256;var t=new Texture2D(size,size,TextureFormat.RGBA32,true,false)
            {name=ivory?"Negative / cast ceramic surface":"Negative / coated textile weave",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float u=x/(float)size,v=y/(float)size;
            float weave=(Mathf.Sin(x*Mathf.PI*.5f)+Mathf.Sin(y*Mathf.PI*.5f))*.012f;
            float streak=Mathf.Sin(u*37+Mathf.Sin(v*9)*.8f)*Mathf.Sin(u*81+v*2)*.027f;
            float value=ivory?.96f+streak*.55f:.92f+weave+streak;
            pixels[y*size+x]=new Color(value,value*.993f,value*.978f,1);
        }
        t.SetPixels(pixels);t.Apply(true,false);return t;
    }
    static T Asset<T>(string name,Func<T> create) where T:Object
    {
        string path=Output+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);
        if(old){if(old is Mesh existing)NegativeIntruderGeometry.Validate(existing);return old;}
        Require(!System.IO.File.Exists(path),"Unrecognized existing private asset: "+path);
        var item=create();if(item is Mesh mesh)NegativeIntruderGeometry.Validate(mesh);
        AssetDatabase.CreateAsset(item,path);return item;
    }
    static void EnsureFolder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        int i=path.LastIndexOf('/');string parent=path.Substring(0,i);EnsureFolder(parent);AssetDatabase.CreateFolder(parent,path.Substring(i+1));
    }
    static T Ref<T>(SerializedObject s,string name)where T:Object=>s.FindProperty(name).objectReferenceValue as T;
    static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}

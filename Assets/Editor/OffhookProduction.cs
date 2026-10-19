using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using Object=UnityEngine.Object;

public static class OffhookProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Offhook.unity";
    const string Art="Assets/_Game/Art/Telephone";
    static void Ref(Object o,string n,Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static Transform Any(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
    static Material Mat(string n,Color linear,float rough,float metal)
    {string path=Art+"/M_"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",linear.gamma);m.SetFloat("_Smoothness",1-rough);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;}
    static Transform Anchor(string n,Transform parent,Vector3 p,Quaternion q){var t=new GameObject(n).transform;t.SetParent(parent);t.SetPositionAndRotation(p,q);return t;}
    [MenuItem("Echo Protocol/Author physical house telephone")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(PorterProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        var root=new GameObject("Offhook / optional house telephone").transform;
        Any("Residence Telephone").gameObject.SetActive(false);
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="Residence TableBelongings"&&Vector3.Distance(t.position,new Vector3(-4.4f,.76f,-51.9f))<.8f))t.gameObject.SetActive(false);
        var path="Assets/_Game/Art/Models/Telephone/EP_PH_RotaryTelephone.fbx";
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!prefab)throw new Exception("Missing original telephone FBX");
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name="Offhook original rotary telephone";g.transform.SetParent(root);
        var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.SetPositionAndRotation(new Vector3(-4.4f,.760f,-51.9f),Quaternion.Euler(0,90,0)*g.transform.rotation);
        var mats=new Dictionary<string,Material>{
            ["PH_Bakelite"]=Mat("PH_Bakelite",new Color(.034f,.025f,.019f),.31f,0),
            ["PH_Rim"]=Mat("PH_Rim",new Color(.068f,.050f,.034f),.37f,0),
            ["PH_Rubber"]=Mat("PH_Rubber",new Color(.013f,.015f,.012f),.66f,0),
            ["PH_Nickel"]=Mat("PH_Nickel",new Color(.29f,.27f,.22f),.34f,.78f),
            ["PH_Ivory"]=Mat("PH_Ivory",new Color(.66f,.56f,.37f),.57f,0),
            ["PH_Ink"]=Mat("PH_Ink",new Color(.024f,.031f,.027f),.60f,0)};
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mats[m.name]).ToArray();
        Func<string,Transform> part=n=>g.GetComponentsInChildren<Transform>().Single(t=>t.name==n);
        var handset=part("TelephoneHandset");var rest=Anchor("Offhook handset cradle pose",root,handset.position,handset.rotation);
        var destination=Anchor("Offhook handset table pose",root,part("RotaryTelephone__HANDSET_OFFHOOK").position,Quaternion.AngleAxis(90,Vector3.up)*handset.rotation);
        var end=Anchor("Offhook handset cord endpoint",handset,part("RotaryTelephone__CORD_HANDSET_REST").position,Quaternion.identity);
        var guide=Anchor("Offhook cord tabletop bend",root,new Vector3(-4.45f,.783f,-51.64f),Quaternion.identity);
        var cordGO=new GameObject("Offhook attached coiled cord");cordGO.transform.SetParent(root);var cord=cordGO.AddComponent<LineRenderer>();cord.sharedMaterial=mats["PH_Rubber"];cord.widthMultiplier=.0025f;cord.numCornerVertices=2;cord.numCapVertices=2;cord.useWorldSpace=true;cord.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;cord.receiveShadows=true;cord.generateLightingData=true;
        var house=Object.FindFirstObjectByType<HouseMemory>();var phone=Object.FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(h=>h.Kind==HouseInteractionKind.Telephone);
        // Existing center-camera IInteractable and house state remain authoritative.
        phone.transform.position=new Vector3(-4.4f,.90f,-51.84f);phone.transform.rotation=Quaternion.identity;phone.transform.localScale=new Vector3(.33f,.30f,.53f);
        var audioGO=new GameObject("Offhook moving handset contacts");audioGO.transform.SetParent(handset,false);var audio=audioGO.AddComponent<AudioSource>();audio.playOnAwake=false;audio.loop=false;audio.volume=.38f;audio.spatialBlend=1;audio.rolloffMode=AudioRolloffMode.Logarithmic;audio.minDistance=.65f;audio.maxDistance=5;audio.dopplerLevel=0;
        var hso=new SerializedObject(house);var ring=(AudioSource)hso.FindProperty("telephoneAudio").objectReferenceValue;audio.outputAudioMixerGroup=ring.outputAudioMixerGroup;
        hso.FindProperty("telephoneRing").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Telephone/TelephoneRing.wav");hso.FindProperty("receiverClick").objectReferenceValue=null;hso.ApplyModifiedPropertiesWithoutUndo();
        ring.transform.position=part("RotaryTelephone__CORD_BASE").position;ring.volume=.30f;ring.dopplerLevel=0;
        var controller=root.gameObject.AddComponent<HouseTelephone>();Ref(controller,"house",house);Ref(controller,"session",Object.FindFirstObjectByType<GameSession>());Ref(controller,"handset",handset);Ref(controller,"rest",rest);Ref(controller,"offhook",destination);Ref(controller,"cordBase",part("RotaryTelephone__CORD_BASE"));Ref(controller,"cordHandset",end);Ref(controller,"cordGuide",guide);Ref(controller,"cord",cord);Ref(controller,"contactAudio",audio);Ref(controller,"lift",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Telephone/HandsetLift.wav"));Ref(controller,"setDown",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Telephone/HandsetSetDown.wav"));Ref(phone,"telephone",controller);
        foreach(var file in new[]{"TelephoneRing","HandsetLift","HandsetSetDown"})
        {var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/_Game/Audio/Clips/Telephone/"+file+".wav");importer.forceToMono=true;var sample=importer.defaultSampleSettings;sample.loadType=AudioClipLoadType.DecompressOnLoad;sample.compressionFormat=AudioCompressionFormat.PCM;sample.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=sample;importer.SaveAndReimport();}
        var campaign=Object.FindFirstObjectByType<CampaignDirector>();var cs=new SerializedObject(campaign);cs.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Offhook.v1";cs.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Offhook authored: optional present-tense handset, same house consequence and immutable Echo. Verify ray, cord, motion, pause and restored review state before build.");
    }
}

using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class NegativeUnsealingIntegration
{
    [MenuItem("Echo Protocol/Negative/Connect unsealing in open scene")]
    public static void ApplyToOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || SceneManager.GetActiveScene().name != "EchoProtocol_Negative")
            throw new InvalidOperationException("Open Negative in Edit Mode after compilation.");
        if (All<NegativeUnsealingSequence>().Any()) throw new InvalidOperationException("Unsealing is already installed; edit it in place.");
        NegativeUnsealingAudioIntegration.ImportClips();
        var root = Named("Negative / unsealing apparatus");
        var chamber = Named("Unsealing / chamber");
        var head = Named("Unsealing / captive head");
        var hand = Named("Unsealing / captive right hand");
        var hanging = Named("Unsealing / hanging body");
        var oldPanel = Named("Ritual / impact panel");
        foreach (var renderer in oldPanel.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name == "Ritual / impact relief" || renderer.name == "Ritual / stretched stitching" || renderer.name == "Ritual / rigid backing")
            { Undo.RecordObject(renderer,"Expose unsealing chamber"); renderer.enabled = false; }
        }
        var sequence = Undo.AddComponent<NegativeUnsealingSequence>(root.gameObject);
        var so = new SerializedObject(sequence);
        var player = All<FirstPersonController>().Single();
        Ref(so,"session",All<GameSession>().Single());
        Ref(so,"encounter",All<MachineCoverEncounter>().Single());
        Ref(so,"ritual",All<NegativeRitualSequence>().Single());
        Ref(so,"output",All<SessionAudio>().Single());
        Ref(so,"player",player.transform); Ref(so,"view",player.GetComponentInChildren<Camera>(true));
        Ref(so,"leftVeil",Named("Unsealing / left veil")); Ref(so,"rightVeil",Named("Unsealing / right veil"));
        Ref(so,"captiveHead",head); Ref(so,"captiveHand",hand);
        Ref(so,"hangingBody",hanging); Ref(so,"loweringCable",Named("Unsealing / lowering cable"));
        Ref(so,"captiveLight",Lamp(root,"Unsealing / cavity practical",new Vector3(-4.7f,2.9f,-41.6f),head.position,.12f,4.5f,new Color(.9f,.66f,.44f)));
        Ref(so,"suspendedLight",Lamp(root,"Unsealing / descent practical",new Vector3(2.7f,5.6f,-42.8f),new Vector3(1.15f,3.8f,-43.6f),.75f,7f,new Color(.65f,.75f,.72f)));
        Refs(so,"cues",new[] {
            NegativeUnsealingAudioIntegration.CreateSource(chamber,"Unsealing / veil detachment",NegativeUnsealingAudioIntegration.Cue.Peel,chamber.position),
            NegativeUnsealingAudioIntegration.CreateSource(hand,"Unsealing / close tendon",NegativeUnsealingAudioIntegration.Cue.Tendon,hand.position),
            NegativeUnsealingAudioIntegration.CreateSource(hanging,"Unsealing / stressed lowering harness",NegativeUnsealingAudioIntegration.Cue.Hoist,hanging.position)
        });
        so.ApplyModifiedPropertiesWithoutUndo();
        foreach(var tone in All<CampaignRoomTone>())
        { var mix=new SerializedObject(tone); Ref(mix,"negativeUnsealing",sequence); mix.ApplyModifiedPropertiesWithoutUndo(); }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Unsealing connected to real wall strike, proximity and committed return traversal. Scene UNSAVED.",sequence);
    }

    // New original surface, imported separately from any licensed actor asset.
    public static void ApplySurfaceTexture()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Apply surface outside Play.");
        const string path="Assets/_Game/Art/NegativeUnsealing/WaxedMortuaryCanvas.png";
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if(!texture)throw new InvalidOperationException("Import the saved generated surface first.");
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Default; importer.sRGBTexture=true; importer.mipmapEnabled=true;
        importer.wrapMode=TextureWrapMode.Repeat; importer.maxTextureSize=1024; importer.anisoLevel=4; importer.SaveAndReimport();
        foreach(var folder in new[]{"Assets/_Game/Art/NegativeUnsealing","Assets/_Game/Art/NegativeRitual"})
        foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{folder}))
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if(mat.name.IndexOf("cast",StringComparison.OrdinalIgnoreCase)<0 && mat.name.IndexOf("shroud",StringComparison.OrdinalIgnoreCase)<0
                && mat.name.IndexOf("veil",StringComparison.OrdinalIgnoreCase)<0 && mat.name.IndexOf("anatom",StringComparison.OrdinalIgnoreCase)<0)continue;
            mat.SetTexture("_BaseMap",texture); mat.SetTextureScale("_BaseMap",Vector2.one);
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();
    }

    static Light Lamp(Transform root,string name,Vector3 position,Vector3 target,float intensity,float range,Color color)
    {
        var g=new GameObject(name); Undo.RegisterCreatedObjectUndo(g,"Connect unsealing lighting");g.transform.SetParent(root,true);
        g.transform.position=position;g.transform.rotation=Quaternion.LookRotation(target-position);
        var light=g.AddComponent<Light>();light.type=LightType.Spot;light.spotAngle=90;light.innerSpotAngle=52;
        light.intensity=intensity;light.range=range;light.color=color;light.shadows=LightShadows.None;return light;
    }
    static T[] All<T>() where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Transform Named(string name)=>All<Transform>().Single(t=>t.name==name);
    static void Ref(SerializedObject so,string name,Object value)=>so.FindProperty(name).objectReferenceValue=value;
    static void Refs(SerializedObject so,string name,Object[] values)
    {var a=so.FindProperty(name);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];}
}

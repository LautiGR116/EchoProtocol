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

public static class NegativeAutopsyIntegration
{
    [MenuItem("Echo Protocol/Negative/Connect Autopsy in open scene")]
    public static void ApplyToOpenScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().name!="EchoProtocol_Negative")
            throw new InvalidOperationException("Open Negative in Edit Mode after compilation.");
        if(All<NegativeAutopsySequence>().Any())throw new InvalidOperationException("Autopsy is already installed; edit it in place.");
        NegativeAutopsyAudioIntegration.ImportClips();
        // Explicit names supplied by the saved original-geometry installer.
        var hanging=Named("Unsealing / hanging body");var neck=Named("Autopsy / hanging neck");
        var captive=Named("Unsealing / captive head");
        var root=new GameObject("Negative / autopsy response");Undo.RegisterCreatedObjectUndo(root,"Connect Autopsy");
        var sequence=Undo.AddComponent<NegativeAutopsySequence>(root);
        var so=new SerializedObject(sequence);var player=All<FirstPersonController>().Single();
        Ref(so,"session",All<GameSession>().Single());Ref(so,"encounter",All<MachineCoverEncounter>().Single());
        Ref(so,"unsealing",All<NegativeUnsealingSequence>().Single());Ref(so,"output",All<SessionAudio>().Single());
        Ref(so,"player",player.transform);Ref(so,"view",player.GetComponentInChildren<Camera>(true));
        Ref(so,"leftChestFlap",Named("Autopsy / left chest flap"));Ref(so,"rightChestFlap",Named("Autopsy / right chest flap"));
        Ref(so,"hangingNeck",neck);Ref(so,"hangingJaw",Named("Autopsy / hanging jaw"));Ref(so,"captiveJaw",Named("Autopsy / captive jaw"));
        var lamp=new GameObject("Autopsy / thorax practical");lamp.transform.SetParent(hanging,false);
        lamp.transform.localPosition=new Vector3(-.5f,-.75f,1.05f);
        lamp.transform.localRotation=Quaternion.LookRotation(new Vector3(0,-1.08f,.10f)-lamp.transform.localPosition);
        var light=lamp.AddComponent<Light>();light.type=LightType.Spot;light.spotAngle=80;light.innerSpotAngle=55;
        light.color=new Color(.85f,.64f,.43f);light.intensity=.15f;light.range=3;light.shadows=LightShadows.None;
        Ref(so,"cavityLight",light);
        var cues=new[]{
            NegativeAutopsyAudioIntegration.CreateSource(hanging,"Autopsy / tearing chest sutures",NegativeAutopsyAudioIntegration.Cue.Suture,hanging.TransformPoint(new Vector3(0,-1,.22f))),
            NegativeAutopsyAudioIntegration.CreateSource(hanging,"Autopsy / opening thorax",NegativeAutopsyAudioIntegration.Cue.Thorax,hanging.TransformPoint(new Vector3(0,-1.1f,.18f))),
            NegativeAutopsyAudioIntegration.CreateSource(captive,"Autopsy / releasing jaw",NegativeAutopsyAudioIntegration.Cue.Jaw,captive.position),
            NegativeAutopsyAudioIntegration.CreateSource(neck,"Autopsy / empty exhalation",NegativeAutopsyAudioIntegration.Cue.AfterBreath,neck.TransformPoint(new Vector3(0,.12f,.12f)))
        };
        var array=so.FindProperty("cues");array.arraySize=cues.Length;
        for(int i=0;i<cues.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=cues[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        foreach(var tone in All<CampaignRoomTone>()){var mix=new SerializedObject(tone);Ref(mix,"negativeAutopsy",sequence);mix.ApplyModifiedPropertiesWithoutUndo();}
        // Put the stationary return practical on the side seen from the route.
        var descent=Named("Unsealing / descent practical");Undo.RecordObject(descent,"Light return-side anatomy");
        descent.position=new Vector3(3.1f,3.6f,-43.5f);descent.rotation=Quaternion.LookRotation(new Vector3(4.2f,1.8f,-44.8f)-descent.position);
        NegativeAutopsyThroatIntegration.Apply();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Autopsy connected to actual unsealing, descent and proximity. Scene UNSAVED.",sequence);
    }
    static T[] All<T>()where T:Component=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Transform Named(string name)=>All<Transform>().Single(t=>t.name==name);
    static void Ref(SerializedObject so,string name,UnityEngine.Object value)=>so.FindProperty(name).objectReferenceValue=value;
}

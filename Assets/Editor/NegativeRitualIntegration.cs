using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class NegativeRitualIntegration
{
    [MenuItem("Echo Protocol/Negative/Connect ritual in open Negative scene")]
    public static void ApplyToOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || SceneManager.GetActiveScene().name != "EchoProtocol_Negative")
            throw new InvalidOperationException("Open Negative in Edit Mode after compilation. This installer does not open or save a scene.");
        if (All<NegativeRitualSequence>().Any()) throw new InvalidOperationException("Ritual already connected. Edit the existing installation.");
        var root = Named("Negative / ritual apparatus");
        var close = Named("Ritual / close relief");
        var impact = Named("Ritual / impact relief");
        var panel = Named("Ritual / impact panel");
        var forms = Enumerable.Range(1, 3).Select(i => Named("Ritual / suspended cast " + i)).ToArray();
        var room = All<NegativeRoomSequence>().Single();
        var encounter = All<MachineCoverEncounter>().Single();
        var sequence = Undo.AddComponent<NegativeRitualSequence>(root.gameObject);
        var so = new SerializedObject(sequence);
        Ref(so, "session", All<GameSession>().Single());
        Ref(so, "encounter", encounter);
        Ref(so, "threat", All<MachineCoverThreat>().Single());
        Ref(so, "crank", All<MachineCoverInteraction>().Single(c => c.Operation == MachineCoverInteraction.Kind.Crank));
        Ref(so, "room", room);
        Ref(so, "output", All<SessionAudio>().Single());
        Ref(so, "closeRelief", close); Ref(so, "impactRelief", impact); Ref(so, "impactPanel", panel);
        Refs(so, "suspendedForms", forms);
        Ref(so, "closeLight", Lamp(root, "Ritual / close grazing light", new Vector3(-4.65f, 2.9f, -37.55f), close.position, .32f, 4.2f));
        Ref(so, "impactLight", Lamp(root, "Ritual / impact grazing light", new Vector3(-4.7f, 2.9f, -41.4f), impact.position, .25f, 4.5f));
        // High fixtures describe the hanging forms without raising room exposure.
        for (int i = 0; i < forms.Length; i++)
            Lamp(root, "Ritual / suspension practical " + (i+1), forms[i].position + new Vector3(i%2==0?1.2f:-1.2f,-1,-.7f), forms[i].position + Vector3.down*1.7f, 1.2f, 4.2f);
        NegativeRitualAudioIntegration.ImportClips();
        var cues = new[]
        {
            Source(close, "Ritual / pressure entering cloth", NegativeRitualAudioIntegration.Cue.PressureInhale, close.position),
            Source(close, "Ritual / close material dragged", NegativeRitualAudioIntegration.Cue.CloseDrag, close.position),
            Source(panel, "Ritual / struck plate", NegativeRitualAudioIntegration.Cue.WallStrike, panel.position),
            Source(forms[0], "Ritual / ceiling load", NegativeRitualAudioIntegration.Cue.OverheadStrain, forms[0].position+Vector3.down*.8f),
            Source(forms[2], "Ritual / distant answering frame", NegativeRitualAudioIntegration.Cue.RemoteAnswer, forms[2].position+Vector3.down*.8f)
        };
        Refs(so, "cues", cues); so.ApplyModifiedPropertiesWithoutUndo();
        foreach(var tone in All<CampaignRoomTone>())
        { var mix = new SerializedObject(tone); Ref(mix, "negativeRitual", sequence); mix.ApplyModifiedPropertiesWithoutUndo(); }

        // Replace the three old room sounds; do not stack an additional library
        // over the same physical gesture. Existing transforms retain placement.
        var roomSo = new SerializedObject(room);
        var old = roomSo.FindProperty("cues");
        var kinds = new[] { NegativeRitualAudioIntegration.Cue.SteelCompression, NegativeRitualAudioIntegration.Cue.DryLatch, NegativeRitualAudioIntegration.Cue.PressureChoke };
        for (int i = 0; i < 3; i++)
        {
            var previous = (AudioSource)old.GetArrayElementAtIndex(i).objectReferenceValue;
            var next = Source(previous.transform.parent, previous.name + " / ritual master", kinds[i], previous.transform.position);
            old.GetArrayElementAtIndex(i).objectReferenceValue = next;
            Undo.DestroyObjectImmediate(previous.gameObject);
        }
        roomSo.FindProperty("silenceCueSeconds").floatValue = .43f;
        roomSo.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Negative ritual connected: actual crank deformation, first-silence strike, suspended load and physical distant answer. Eight routed masters; scene UNSAVED.", sequence);
    }
    static AudioSource Source(Transform parent, string name, NegativeRitualAudioIntegration.Cue cue, Vector3 p)
        => NegativeRitualAudioIntegration.CreateSource(parent, name, cue, p);
    static Light Lamp(Transform parent, string name, Vector3 p, Vector3 target, float intensity, float range)
    {
        var g = new GameObject(name); Undo.RegisterCreatedObjectUndo(g,"Connect Negative ritual");
        g.transform.SetParent(parent,true); g.transform.position=p; g.transform.rotation=Quaternion.LookRotation(target-p);
        var l=g.AddComponent<Light>(); l.type=LightType.Spot; l.spotAngle=75; l.innerSpotAngle=38;
        l.range=range; l.intensity=intensity; l.color=new Color(.79f,.57f,.38f); l.shadows=LightShadows.None;
        return l;
    }
    static T[] All<T>() where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Transform Named(string name) => All<Transform>().Single(t=>t.name==name);
    static void Ref(SerializedObject so,string name,Object value) => so.FindProperty(name).objectReferenceValue=value;
    static void Refs(SerializedObject so,string name,Object[] values)
    { var p=so.FindProperty(name);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; }
}

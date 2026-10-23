using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderPresence;
using EchoProtocol.Campaign.P3Intrusion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Configure the existing local lights and emitters; no scene/model/clip assets
// are created, opened or saved. Existing contact owners retain every Play call.
public static class IntruderPresenceIntegration
{
    const string RootName = "IntruderPresence / local contrast";
    const string UndoName = "Intruder local light and contact-led sound";
    const float P3Intensity = 1.05f;
    sealed class Context
    {
        public Scene scene;
        public IntruderStaging staging;
        public MachineCoverPresence presence;
        public Light p3, pump, bounce, lamp;
        public AudioSource effort, breath, huntBody, p3Contact, support, impact;
        public IntruderPresenceSetup installed;
    }

    [MenuItem("Echo Protocol/Intruder presence/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "Intruder presence is intact; Apply is a no-op."
            : "Intruder presence ready: four existing local lights, three source gains, no new playback. Scene remains untouched.", c.staging);
    }

    [MenuItem("Echo Protocol/Intruder presence/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("Intruder presence already applied; nothing changed.", c.staging); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(UndoName);
        GameObject root = null;
        try
        {
            root = new GameObject(RootName); root.transform.SetParent(c.staging.transform.parent, false);
            var setup = root.AddComponent<IntruderPresenceSetup>();
            setup.staging = c.staging; setup.presence = c.presence;
            setup.previousP3Intensity = new SerializedObject(c.staging).FindProperty("p3RevealIntensity").floatValue;
            // Fixed side keys: light crosses cheek/nose and fingers; it neither
            // follows the face nor points from the Player camera. Both are on
            // the actor's side of the relevant physical boundary.
            setup.lights = new[]
            {
                Key(c.p3, new Vector3(-6.10f, 2.17f, 35.28f), new Vector3(-5.56f, 1.48f, 36.02f),
                    new Color(.82f, .86f, .80f), P3Intensity, 3.1f, 94, 55),
                Key(c.pump, new Vector3(-4.90f, 2.20f, -44.08f), new Vector3(-4.08f, 1.42f, -44.74f),
                    new Color(.82f, .86f, .80f), c.pump.intensity, 3.4f, 94, 55),
                Level(c.bounce, .18f), Level(c.lamp, 2.8f)
            };
            setup.sounds = new[] { Gain(c.effort, .31f), Gain(c.breath, .36f), Gain(c.huntBody, .30f) };
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Record(setup);
            foreach (var light in setup.lights) light.applied.Write(light.source);
            foreach (var sound in setup.sounds) sound.source.volume = sound.applied;
            Profile(c.staging, true, P3Intensity); Profile(c.presence, true);
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Intruder presence applied; scene UNSAVED. Side light replaces low frontal fill; physical contact leads effort/breath without a repeated launch vocal or continuous friction. Check one P3 contact and pump entry in the central Editor. Undo or Remove restores original local values.", c.staging);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group); if (root) Object.DestroyImmediate(root); throw;
        }
    }

    [MenuItem("Echo Protocol/Intruder presence/3. Remove from current scene")]
    public static void RemoveFromCurrentScene()
    {
        var c = Validate();
        if (!c.installed) { Debug.Log("Intruder presence is not installed; nothing changed."); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Remove intruder presence");
        try
        {
            var setup = c.installed; Record(setup);
            foreach (var light in setup.lights) light.before.Write(light.source);
            foreach (var sound in setup.sounds) sound.source.volume = sound.before;
            Profile(c.staging, false, setup.previousP3Intensity); Profile(c.presence, false);
            Undo.DestroyObjectImmediate(setup.gameObject);
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Debug.Log("Intruder presence removed; original local settings restored. Scene remains UNSAVED.", c.staging);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play before integrating presence.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded && c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal),
            "Use the already open Incursion scene. This operation never opens/saves a scene.");
        c.staging = All<IntruderStaging>(c.scene).Single(); c.presence = c.staging.GetComponent<MachineCoverPresence>();
        Require(c.presence && c.presence.enabled && c.staging.enabled, "Missing existing live-body presence owner.");
        var s = new SerializedObject(c.staging); var p = new SerializedObject(c.presence);
        var rush = Get<EmbestidaRush>(s, "embestida"); var p3 = Get<P3BodyIntrusion>(s, "p3Intrusion");
        Require(rush && p3 && p3.IsConfigured && rush.transform == c.staging.transform,
            "Requires the existing P3 intrusion and Embestida on the same present body.");
        var r = new SerializedObject(rush); var glass = new SerializedObject(p3);
        c.p3 = Get<Light>(s, "p3Practical"); c.pump = Get<Light>(p, "threatPractical");
        c.bounce = All<Light>(c.scene).Single(l => l.name == "P3 / returned alcove wall reflection");
        c.lamp = All<Light>(c.scene).Single(l => l.name == "A witness warm lamp");
        Require(c.p3 && c.pump && c.p3.type == LightType.Spot && c.pump.type == LightType.Spot
            && c.p3.enabled && c.pump.enabled && c.bounce.enabled && c.lamp.enabled,
            "Expected the two existing spot keys and P3 local lamp/reflection.");
        c.effort = Get<AudioSource>(r, "effort"); c.breath = Get<AudioSource>(r, "breathing");
        c.impact = Get<AudioSource>(r, "contact"); c.huntBody = Get<AudioSource>(p, "huntBody");
        c.p3Contact = Get<AudioSource>(glass, "contact"); c.support = Get<AudioSource>(glass, "support");
        Require(c.breath == Get<AudioSource>(p, "huntBreathing") && c.breath == Get<AudioSource>(s, "breath"),
            "The pump breath must remain ONE shared source; no competing reproduction may be installed.");
        foreach (var source in new[] { c.effort, c.breath, c.impact, c.huntBody, c.p3Contact, c.support })
            Require(source && source.gameObject.scene == c.scene && source.enabled && source.clip && source.clip.channels == 1
                && source.outputAudioMixerGroup && source.outputAudioMixerGroup.name == "WorldSFX"
                && Mathf.Approximately(source.spatialBlend, 1) && !source.ignoreListenerPause && !source.ignoreListenerVolume,
                "Expected existing mono, spatial WorldSFX emitters respecting listener pause/volume.");
        Require(c.effort.clip.name == "Embestida_ExpelledEffort" && c.breath.clip.name == "Embestida_BodyBreath",
            "Review changed voice/breath provenance before applying this mix.");
        Require(Contains(p, "spatialSources", c.effort) && Contains(p, "spatialSources", c.breath)
            && Contains(p, "spatialSources", c.huntBody), "Presence must retain acoustic ownership of these sources.");
        var installed = All<IntruderPresenceSetup>(c.scene).ToArray();
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        Require(installed.Length <= 1, "Duplicate presence installation; nothing changed.");
        if (installed.Length == 0)
        {
            Require(roots.Length == 0 && !s.FindProperty("contactLedPresentation").boolValue
                && !p.FindProperty("contactLedPresentation").boolValue, "Orphaned presence settings; resolve before applying.");
            Require(Vector3.Distance(c.bounce.transform.position, new Vector3(-6.3f, 1.3f, 35.65f)) < .05f
                && Vector3.Distance(c.lamp.transform.position, new Vector3(-4.16f, 1.16f, 36.48f)) < .05f,
                "P3 local lights moved; review the close hand illumination before applying.");
            return c;
        }
        c.installed = installed[0]; var setup = c.installed;
        Require(roots.Length == 1 && roots[0] == setup.transform && setup.transform.parent == c.staging.transform.parent
            && setup.revision == 1 && setup.staging == c.staging && setup.presence == c.presence
            && setup.lights != null && setup.lights.Length == 4 && setup.sounds != null && setup.sounds.Length == 3
            && s.FindProperty("contactLedPresentation").boolValue && p.FindProperty("contactLedPresentation").boolValue
            && Mathf.Approximately(s.FindProperty("p3RevealIntensity").floatValue, P3Intensity),
            "Partial/modified presence installation; nothing changed.");
        Require(setup.lights.Select(e => e.source).SequenceEqual(new[] { c.p3, c.pump, c.bounce, c.lamp })
            && setup.sounds.Select(e => e.source).SequenceEqual(new[] { c.effort, c.breath, c.huntBody }),
            "Presence references changed; review before applying or removing.");
        foreach (var light in setup.lights) Require(light.source && light.applied.Matches(light.source), "A local light was edited after integration; preserve/reconcile it before removal.");
        foreach (var sound in setup.sounds) Require(sound.source && Mathf.Approximately(sound.source.volume, sound.applied), "A source gain was edited after integration; preserve/reconcile it before removal.");
        return c;
    }

    static IntruderPresenceSetup.LightEdit Level(Light source, float intensity)
    {
        var state = IntruderPresenceSetup.LightState.Read(source); var after = state; after.intensity = intensity;
        return new IntruderPresenceSetup.LightEdit { source = source, before = state, applied = after };
    }
    static IntruderPresenceSetup.LightEdit Key(Light source, Vector3 position, Vector3 aim, Color color, float intensity, float range, float outer, float inner)
    {
        var edit = Level(source, intensity); var after = edit.applied;
        after.position = source.transform.parent ? source.transform.parent.InverseTransformPoint(position) : position;
        var worldRotation = Quaternion.LookRotation(aim - position);
        after.rotation = source.transform.parent ? Quaternion.Inverse(source.transform.parent.rotation) * worldRotation : worldRotation;
        after.color = color; after.range = range; after.outer = outer; after.inner = inner;
        after.shadows = LightShadows.Soft; after.bias = .015f; after.normalBias = .06f;
        edit.applied = after; return edit;
    }
    static IntruderPresenceSetup.AudioEdit Gain(AudioSource source, float gain)
        => new IntruderPresenceSetup.AudioEdit { source = source, before = source.volume, applied = Mathf.Min(source.volume, gain) };
    static void Profile(Object target, bool enabled, float p3Intensity = -1)
    {
        var s = new SerializedObject(target); s.FindProperty("contactLedPresentation").boolValue = enabled;
        if (p3Intensity >= 0) s.FindProperty("p3RevealIntensity").floatValue = p3Intensity;
        s.ApplyModifiedProperties(); PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }
    static void Record(IntruderPresenceSetup setup)
    {
        Undo.RecordObjects(new Object[] { setup.staging, setup.presence }
            .Concat(setup.lights.SelectMany(l => new Object[] { l.source, l.source.transform }))
            .Concat(setup.sounds.Select(a => (Object)a.source)).ToArray(), UndoName);
    }
    static bool Contains(SerializedObject s, string field, Object item)
    { var a = s.FindProperty(field); return Enumerable.Range(0, a.arraySize).Any(i => a.GetArrayElementAtIndex(i).objectReferenceValue == item); }
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static T Get<T>(SerializedObject s, string field) where T : Object => s.FindProperty(field)?.objectReferenceValue as T;
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

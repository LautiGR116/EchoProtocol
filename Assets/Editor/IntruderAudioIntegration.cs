using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.P3Intrusion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Upgrade the saved round-four installation in place. Existing contact owners,
// source gains, routing, light receipts and actor references remain untouched.
public static class IntruderAudioIntegration
{
    const string RootName = "IntruderDreadAudio / discrete body phrase";
    const string UndoName = "Upgrade intruder body audio to round five";
    const string AudioRoot = "Assets/_Game/Audio/IntruderDread/IntruderDread_";
    sealed class Context
    {
        public Scene scene;
        public IntruderStaging staging;
        public MachineCoverPresence presence;
        public EmbestidaRush rush;
        public P3BodyIntrusion p3;
        public AudioSource effort, breath, contact;
        public AudioClip contained, expelled, recovery, dry;
        public IntruderDreadAudioSetup installed;
    }

    [MenuItem("Echo Protocol/Intruder audio/1. Validate round-five upgrade")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "Intruder audio round five is intact; Upgrade is a no-op."
            : "Intruder audio ready: four traced mono cuts, three existing emitters, explicit breathing handoff. No scene changes.", c.presence);
    }

    [MenuItem("Echo Protocol/Intruder audio/2. Upgrade current scene")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("Intruder audio round five already installed; nothing changed.", c.presence); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(UndoName);
        GameObject root = null;
        try
        {
            root = new GameObject(RootName); root.transform.SetParent(c.staging.transform.parent, false);
            var setup = root.AddComponent<IntruderDreadAudioSetup>();
            setup.presence = c.presence; setup.rush = c.rush;
            setup.previousDiscreteBreathing = new SerializedObject(c.presence).FindProperty("discreteBreathing").boolValue;
            setup.sources = new[] { Source(c.effort, c.expelled), Source(c.breath, c.recovery), Source(c.contact, c.dry) };
            setup.references = new[]
            {
                Reference(c.presence, "containedBreath", c.contained),
                Reference(c.presence, "recoveryBreath", c.recovery),
                Reference(c.presence, "launchEffort", c.expelled),
                Reference(c.presence, "reacquireEffort", c.expelled),
                Reference(c.presence, "searchBreath", c.contained),
                Reference(c.presence, "dreadP3", c.p3),
                Reference(c.rush, "discreteBreathOwner", c.presence)
            };
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Record(setup);
            foreach (var edit in setup.sources) { edit.applied.Write(edit.source); PrefabUtility.RecordPrefabInstancePropertyModifications(edit.source); }
            foreach (var edit in setup.references) Write(edit, edit.applied);
            Discrete(c.presence, true);
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Intruder audio upgraded; scene UNSAVED. Dry contact remains owned by EmbestidaRush; brief native-pitch effort follows, with single contained/recovery breaths and silent holds. No new sources, gains, lights or vocal loop. Undo or Roll back restores round four. Central compilation/listening still required.", c.presence);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group); if (root) Object.DestroyImmediate(root); throw;
        }
    }

    [MenuItem("Echo Protocol/Intruder audio/3. Roll back current scene")]
    public static void RollBackCurrentScene()
    {
        var c = Validate();
        if (!c.installed) { Debug.Log("Intruder audio round five is not installed; nothing changed."); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Roll back intruder audio to round four");
        try
        {
            var setup = c.installed; Record(setup);
            foreach (var edit in setup.sources) { edit.before.Write(edit.source); PrefabUtility.RecordPrefabInstancePropertyModifications(edit.source); }
            foreach (var edit in setup.references) Write(edit, edit.before);
            Discrete(c.presence, setup.previousDiscreteBreathing);
            Undo.DestroyObjectImmediate(setup.gameObject);
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Debug.Log("Round-four audio references, pitch, loop and breath ownership restored. Scene UNSAVED; lights and their round-four receipt were not changed.", c.presence);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play before upgrading intruder audio.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded && c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal),
            "Use the already open Incursion scene; this operation never opens or saves scenes.");
        c.staging = All<IntruderStaging>(c.scene).Single();
        c.presence = c.staging.GetComponent<MachineCoverPresence>();
        Require(c.presence && c.presence.enabled && c.staging.enabled, "Expected the existing present actor and presence owner.");
        var s = new SerializedObject(c.staging); var p = new SerializedObject(c.presence);
        c.rush = Get<EmbestidaRush>(s, "embestida"); c.p3 = Get<P3BodyIntrusion>(s, "p3Intrusion");
        Require(c.rush && c.rush.enabled && c.rush.transform == c.staging.transform && c.p3 && c.p3.IsConfigured,
            "Requires the saved P3 and pump performances on the same present actor.");
        var r = new SerializedObject(c.rush);
        Require(r.FindProperty("discreteBreathOwner") != null,
            "Coordinator hook required in EmbestidaRush: serialized MachineCoverPresence discreteBreathOwner, then skip only legacy sustain playback when its OwnsDiscreteBreathing is true. See Docs/INTRUDER_AUDIO_ROUND5.md.");
        Require(p.FindProperty("contactLedPresentation").boolValue && s.FindProperty("contactLedPresentation").boolValue,
            "Requires the installed round-four contact-led presentation. Do not rerun historical generators.");
        c.effort = Get<AudioSource>(r, "effort"); c.breath = Get<AudioSource>(r, "breathing"); c.contact = Get<AudioSource>(r, "contact");
        Require(c.breath == Get<AudioSource>(p, "huntBreathing") && c.breath == Get<AudioSource>(s, "breath"),
            "The shared body breath must remain ONE existing emitter.");
        foreach (var source in new[] { c.effort, c.breath, c.contact, Get<AudioSource>(p, "huntVoice"), Get<AudioSource>(p, "huntBody") })
            Require(source && source.gameObject.scene == c.scene && source.enabled && source.clip
                && source.clip.channels == 1 && source.outputAudioMixerGroup && source.outputAudioMixerGroup.name == "WorldSFX"
                && Mathf.Approximately(source.spatialBlend, 1) && !source.playOnAwake
                && !source.ignoreListenerPause && !source.ignoreListenerVolume,
                "Expected existing spatial mono WorldSFX sources respecting master volume/pause.");
        Require(c.effort != c.breath && c.effort != c.contact && c.breath != c.contact
            && c.breath.transform.IsChildOf(c.staging.transform), "Body/contact emitters must retain their separate physical roles.");
        foreach (var source in new[] { c.effort, c.breath, Get<AudioSource>(p, "huntVoice"), Get<AudioSource>(p, "huntBody") })
            Require(Contains(p, "spatialSources", source), "Retain existing acoustic ownership of voice/body sources.");
        c.contained = Clip("ContainedBreath"); c.expelled = Clip("ExpelledEffort");
        c.recovery = Clip("RecoveryBreath"); c.dry = Clip("DryContact");
        var installs = All<IntruderDreadAudioSetup>(c.scene).ToArray();
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        Require(installs.Length <= 1, "Duplicate round-five audio receipt; nothing changed.");
        if (installs.Length == 0)
        {
            Require(roots.Length == 0 && !p.FindProperty("discreteBreathing").boolValue
                && !Get<Object>(p, "containedBreath") && !Get<Object>(p, "recoveryBreath") && !Get<Object>(p, "dreadP3")
                && !Get<Object>(r, "discreteBreathOwner"), "Orphaned/custom discrete-breath settings; reconcile before upgrading.");
            Require(c.effort.clip.name == "Embestida_ExpelledEffort" && c.breath.clip.name == "Embestida_BodyBreath"
                && c.contact.clip.name == "Embestida_HousingContact", "Expected round-four audio masters; preserve/review changed clips first.");
            return c;
        }
        c.installed = installs[0]; var receipt = c.installed;
        Require(roots.Length == 1 && roots[0] == receipt.transform && receipt.transform.parent == c.staging.transform.parent
            && receipt.revision == 1 && receipt.presence == c.presence && receipt.rush == c.rush
            && receipt.sources != null && receipt.sources.Length == 3 && receipt.references != null && receipt.references.Length == 7
            && p.FindProperty("discreteBreathing").boolValue, "Partial/modified audio installation; nothing changed.");
        Require(receipt.sources.Select(e => e.source).SequenceEqual(new[] { c.effort, c.breath, c.contact }), "Audio receipt sources changed.");
        foreach (var edit in receipt.sources) Require(edit.applied.Matches(edit.source), "An upgraded source was edited; preserve/reconcile before rollback.");
        foreach (var edit in receipt.references)
            Require(edit.target && Get<Object>(new SerializedObject(edit.target), edit.field) == edit.applied,
                "An upgraded audio reference was edited; preserve/reconcile before rollback.");
        Require(c.effort.clip == c.expelled && c.breath.clip == c.recovery && c.contact.clip == c.dry
            && Get<Object>(r, "discreteBreathOwner") == c.presence && Get<Object>(p, "dreadP3") == c.p3,
            "Audio ownership or installed clip differs from round five.");
        return c;
    }

    static AudioClip Clip(string suffix)
    {
        string path = AudioRoot + suffix + ".wav";
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        // Unity's WebGL importer converts these 48 kHz masters to 44.1 kHz.
        // Validate the effective platform rate without changing the Editor target.
        bool validRate = clip && (clip.frequency == 48000
            || (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL && clip.frequency == 44100));
        Require(clip && clip.channels == 1 && validRate && clip.length < 1 && importer,
            "Missing/invalid traced round-five mono clip for the active platform: " + path);
        var settings = importer.defaultSampleSettings;
        Require(settings.compressionFormat == AudioCompressionFormat.PCM && settings.loadType == AudioClipLoadType.DecompressOnLoad
            && settings.preloadAudioData && !importer.loadInBackground, "Use the committed short-clip PCM/preload settings: " + path);
        return clip;
    }
    static IntruderDreadAudioSetup.SourceEdit Source(AudioSource source, AudioClip clip)
        => new IntruderDreadAudioSetup.SourceEdit { source = source, before = IntruderDreadAudioSetup.SourceState.Read(source),
            applied = new IntruderDreadAudioSetup.SourceState { clip = clip, pitch = 1, loop = false } };
    static IntruderDreadAudioSetup.ReferenceEdit Reference(Object target, string field, Object value)
        => new IntruderDreadAudioSetup.ReferenceEdit { target = target, field = field,
            before = Get<Object>(new SerializedObject(target), field), applied = value };
    static void Write(IntruderDreadAudioSetup.ReferenceEdit edit, Object value)
    {
        var s = new SerializedObject(edit.target); s.FindProperty(edit.field).objectReferenceValue = value;
        s.ApplyModifiedProperties(); PrefabUtility.RecordPrefabInstancePropertyModifications(edit.target);
    }
    static void Discrete(MachineCoverPresence presence, bool value)
    {
        var s = new SerializedObject(presence); s.FindProperty("discreteBreathing").boolValue = value;
        s.ApplyModifiedProperties(); PrefabUtility.RecordPrefabInstancePropertyModifications(presence);
    }
    static void Record(IntruderDreadAudioSetup setup)
        => Undo.RecordObjects(new Object[] { setup.presence, setup.rush }.Concat(setup.sources.Select(e => (Object)e.source)).ToArray(), UndoName);
    static bool Contains(SerializedObject s, string field, Object value)
    { var a = s.FindProperty(field); return Enumerable.Range(0, a.arraySize).Any(i => a.GetArrayElementAtIndex(i).objectReferenceValue == value); }
    static T Get<T>(SerializedObject s, string field) where T : Object => s.FindProperty(field)?.objectReferenceValue as T;
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

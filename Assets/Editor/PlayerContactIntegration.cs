using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Additive, repeatable integration of Player-owned references on the open scene.
// Never opens/saves a scene or edits world geometry, Echo data or global settings.
public static class PlayerContactIntegration
{
    const string RootName = "PlayerContact / foot support";
    const string SourceName = "Present Player / alternate foot contact";
    sealed class Context
    {
        public Scene scene;
        public FirstPersonController player;
        public PlayerInteractor interactor;
        public PlayerEchoClearance clearance;
        public PlayerFootsteps footsteps;
        public AudioSource source, alternate;
        public InteractionSurface[] surfaces;
        public GameObject root;
        public bool installed;
    }

    [MenuItem("Echo Protocol/Player contact/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed
            ? "Player contact references valid; Apply is a no-op. Runtime and listening checks remain in the central Editor."
            : "Player contact ready: one additional foot source and explicit Player references. No geometry, controls, clips or mixer gains change. Scene will remain unsaved.", c.player);
    }

    [MenuItem("Echo Protocol/Player contact/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("Player contact already configured; no changes.", c.player); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Integrate Player contact");
        try
        {
            c.root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(c.root, "Create Player contact support");
            SceneManager.MoveGameObjectToScene(c.root, c.scene);
            // Static scene root: footfall tails must stay where they landed.
            var foot = new GameObject(SourceName);
            Undo.RegisterCreatedObjectUndo(foot, "Create second foot emitter");
            foot.transform.SetParent(c.root.transform, false);
            foot.transform.position = c.source.transform.position;
            c.alternate = Undo.AddComponent<AudioSource>(foot);
            EditorUtility.CopySerialized(c.source, c.alternate);
            c.alternate.playOnAwake = false;
            c.alternate.loop = false;
            c.alternate.clip = null;

            Undo.RecordObject(c.footsteps, "Wire Player foot support");
            var steps = new SerializedObject(c.footsteps);
            Ref(steps, "echoClearance", c.clearance);
            Ref(steps, "alternateSource", c.alternate);
            steps.FindProperty("firstContactDistance").floatValue = .42f;
            steps.ApplyModifiedProperties();
            Undo.RecordObject(c.interactor, "Reference moving interaction surfaces");
            var focus = new SerializedObject(c.interactor);
            var surfaces = focus.FindProperty("movingSurfaces");
            surfaces.arraySize = c.surfaces.Length;
            for (int i = 0; i < c.surfaces.Length; i++) surfaces.GetArrayElementAtIndex(i).objectReferenceValue = c.surfaces[i];
            focus.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(c.footsteps);
            PrefabUtility.RecordPrefabInstancePropertyModifications(c.interactor);
            if (!Validate().installed) throw new InvalidOperationException("Player contact final reference validation failed.");
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Player contact integrated with Undo. Scene remains unsaved: review lateral Echo contact, moving/occluded focus and two floor materials before saving. No scene or project settings were changed outside Player contact.", c.player);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            throw;
        }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            throw new InvalidOperationException("Finish compilation/import and stop Play before Player contact integration.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || c.scene.path != "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity")
            throw new InvalidOperationException("Open the current Incursion scene first. This operation never opens or replaces a scene.");
        c.player = One<FirstPersonController>(c.scene);
        c.interactor = One<PlayerInteractor>(c.scene);
        c.clearance = One<PlayerEchoClearance>(c.scene);
        c.footsteps = One<PlayerFootsteps>(c.scene);
        var controller = c.player.GetComponent<CharacterController>();
        if (!controller || c.player.transform.lossyScale != Vector3.one
            || c.interactor.gameObject != c.player.gameObject || c.clearance.gameObject != c.player.gameObject
            || c.footsteps.gameObject != c.player.gameObject || !c.clearance.enabled || !c.footsteps.enabled || !c.interactor.enabled)
            throw new InvalidOperationException("Expected the existing Player capsule and its three enabled contact components on the same unscaled object.");
        var player = new SerializedObject(c.player);
        if (!Mathf.Approximately(player.FindProperty("lookSensitivity").floatValue, .15f)
            || !Mathf.Approximately(player.FindProperty("moveSpeed").floatValue, 4f))
            throw new InvalidOperationException("Player controls differ from the approved .15 sensitivity / 4 m/s baseline; integration will not overwrite them.");
        var body = new SerializedObject(c.clearance);
        var loop = Get<LoopController>(body, "loop"); var session = Get<GameSession>(body, "session");
        if (Get<FirstPersonController>(body, "player") != c.player || !loop || !session
            || loop.gameObject.scene != c.scene || session.gameObject.scene != c.scene)
            throw new InvalidOperationException("Clearance must reference the current Player, loop and session.");
        var focus = new SerializedObject(c.interactor);
        var camera = Get<Camera>(focus, "viewCamera");
        if (!camera || !camera.transform.IsChildOf(c.player.transform) || !Mathf.Approximately(c.interactor.InteractionDistance, 3f))
            throw new InvalidOperationException("Expected the Player camera and unchanged 3 m interaction reach.");
        var steps = new SerializedObject(c.footsteps);
        if (Get<FirstPersonController>(steps, "player") != c.player || Get<CharacterController>(steps, "controller") != controller
            || !Mathf.Approximately(steps.FindProperty("stride").floatValue, 1.4f))
            throw new InvalidOperationException("Footsteps must retain the current Player, capsule and 1.4 m cadence.");
        foreach (string name in new[] { "vinyl", "timber" })
        {
            var clips = steps.FindProperty(name);
            if (clips.arraySize != 3 || Enumerable.Range(0, clips.arraySize).Any(i => !(clips.GetArrayElementAtIndex(i).objectReferenceValue is AudioClip)))
                throw new InvalidOperationException("Restore the existing three vinyl and three timber clips; this integration imports none.");
        }
        var zones = steps.FindProperty("timberZones");
        if (zones.arraySize == 0 || Enumerable.Range(0, zones.arraySize).Any(i =>
            !(zones.GetArrayElementAtIndex(i).objectReferenceValue is BoxCollider zone) || zone.gameObject.scene != c.scene || !zone.isTrigger))
            throw new InvalidOperationException("Existing timber zones must be assigned scene volumes; their geometry will not be edited.");
        c.source = Get<AudioSource>(steps, "source");
        if (!c.source || c.source.gameObject.scene != c.scene || !c.source.enabled || c.source.transform.IsChildOf(c.player.transform)
            || c.source.loop || c.source.playOnAwake || c.source.clip || !c.source.outputAudioMixerGroup
            || c.source.outputAudioMixerGroup.name != "WorldSFX" || c.source.ignoreListenerPause)
            throw new InvalidOperationException("Existing stationary, non-looping Player source with WorldSFX routing and listener pause is required.");
        c.surfaces = All<InteractionSurface>(c.scene).OrderBy(s => s.GetInstanceID()).ToArray();
        if (c.surfaces.Length == 0 || c.surfaces.Any(s => !s.GetComponent<Collider>()
            || !(Get<MonoBehaviour>(new SerializedObject(s), "owner") is IInteractable)))
            throw new InvalidOperationException("Interaction surfaces need their existing physical Collider and explicit IInteractable owner.");

        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        c.alternate = Get<AudioSource>(steps, "alternateSource");
        var assignedClearance = Get<PlayerEchoClearance>(steps, "echoClearance");
        var assignedSurfaces = focus.FindProperty("movingSurfaces");
        bool anyInstallation = roots.Length != 0 || c.alternate || assignedClearance || assignedSurfaces.arraySize != 0;
        if (!anyInstallation) return c;
        if (roots.Length != 1 || roots[0].parent || !c.alternate || c.alternate == c.source
            || c.alternate.transform.parent != roots[0] || c.alternate.name != SourceName || !c.alternate.isActiveAndEnabled
            || roots[0].GetComponentsInChildren<AudioSource>(true).Length != 1
            || assignedClearance != c.clearance || assignedSurfaces.arraySize != c.surfaces.Length
            || !Mathf.Approximately(steps.FindProperty("firstContactDistance").floatValue, .42f)
            || c.surfaces.Any(s => !Enumerable.Range(0, assignedSurfaces.arraySize).Any(i => assignedSurfaces.GetArrayElementAtIndex(i).objectReferenceValue == s))
            || c.alternate.outputAudioMixerGroup != c.source.outputAudioMixerGroup
            || !Mathf.Approximately(c.alternate.volume, c.source.volume)
            || !Mathf.Approximately(c.alternate.spatialBlend, c.source.spatialBlend)
            || c.alternate.rolloffMode != c.source.rolloffMode || c.alternate.loop || c.alternate.playOnAwake || c.alternate.clip
            || c.alternate.ignoreListenerPause)
            throw new InvalidOperationException("Partial or edited Player contact installation found. Review it or Undo; nothing was changed.");
        c.root = roots[0].gameObject; c.installed = true;
        return c;
    }

    static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static T One<T>(Scene scene) where T : Component
    {
        var found = All<T>(scene);
        if (found.Length != 1) throw new InvalidOperationException("Expected exactly one " + typeof(T).Name + " in the active scene.");
        return found[0];
    }
    static T Get<T>(SerializedObject fields, string name) where T : Object => fields.FindProperty(name).objectReferenceValue as T;
    static void Ref(SerializedObject fields, string name, Object value) => fields.FindProperty(name).objectReferenceValue = value;
}

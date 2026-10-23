using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Campaign.P3Intrusion;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Only wires the existing present actor. Never loads/saves a scene, creates
// a model/material, edits audio, or samples animation on the Editor's rig.
public static class IntruderActingIntegration
{
    sealed class Context
    {
        public Scene scene;
        public IntruderStaging owner;
        public MachineThreatPerformance performance;
        public EmbestidaRush rush;
        public P3BodyIntrusion p3;
        public IntruderActingFocus acting;
        public Transform viewer, leftEye, rightEye;
    }

    [MenuItem("Echo Protocol/Intruder acting/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.acting ? "Intruder acting: round 4 wiring valid; Apply is a no-op."
            : "Intruder acting: original present rig, P3 contact targets and pump ready. Apply is reversible and leaves the scene unsaved. Actual wrist/foot reach still needs the central Editor check.", c.owner);
    }

    [MenuItem("Echo Protocol/Intruder acting/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.acting) { Debug.Log("Intruder acting already installed; nothing changed.", c.owner); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Intruder round 4 acting");
        try
        {
            c.acting = Undo.AddComponent<IntruderActingFocus>(c.owner.gameObject);
            var focus = new SerializedObject(c.acting);
            Ref(focus, "leftEye", c.leftEye); Ref(focus, "rightEye", c.rightEye);
            focus.ApplyModifiedProperties();
            Wire(c, c.acting, c.viewer, true);
            if (!c.acting.IsConfigured) throw new InvalidOperationException("Present-actor eyes were not wired. Rolling back.");
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Intruder acting installed; scene UNSAVED. P3: load, unequal hand arrival, left-hand release, held gaze. Pump: diagonal load/contact/recovery keys. Undo or the rollback menu restores the previous acting. No scene, audio or model assets were written.", c.owner);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    [MenuItem("Echo Protocol/Intruder acting/3. Restore previous acting")]
    public static void RestorePreviousActing()
    {
        var c = Validate();
        if (!c.acting) { Debug.Log("Intruder round 4 acting is not installed; nothing changed.", c.owner); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Restore previous intruder acting");
        try
        {
            Wire(c, null, null, false);
            Undo.DestroyObjectImmediate(c.acting);
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Previous intruder acting restored; scene UNSAVED. Face materials, audio, targets and all other work remain as authored.", c.owner);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static void Wire(Context c, IntruderActingFocus acting, Transform viewer, bool authored)
    {
        Undo.RecordObjects(new Object[] { c.performance, c.p3, c.rush }, "Wire exclusive present-actor performance");
        var p = new SerializedObject(c.performance); Ref(p, "acting", acting); p.ApplyModifiedProperties();
        var i = new SerializedObject(c.p3); Ref(i, "acting", acting); Ref(i, "viewer", viewer); i.ApplyModifiedProperties();
        var r = new SerializedObject(c.rush); r.FindProperty("authoredActing").boolValue = authored; r.ApplyModifiedProperties();
        foreach (var item in new Object[] { c.performance, c.p3, c.rush }) PrefabUtility.RecordPrefabInstancePropertyModifications(item);
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before integrating acting.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Use the Incursion scene already open. This integrator never opens or saves scenes.");
        c.owner = All<IntruderStaging>(c.scene).Single();
        var o = new SerializedObject(c.owner);
        c.performance = Get<MachineThreatPerformance>(o, "performance");
        c.rush = Get<EmbestidaRush>(o, "embestida");
        c.p3 = Get<P3BodyIntrusion>(o, "p3Intrusion");
        var view = Get<Camera>(o, "view"); c.viewer = view ? view.transform : null;
        var controller = c.owner.GetComponent<CharacterController>();
        if (!c.performance || !c.rush || !c.p3 || !c.viewer || !controller || !controller.enabled
            || !c.owner.isActiveAndEnabled || !c.performance.isActiveAndEnabled || !c.rush.isActiveAndEnabled || !c.p3.isActiveAndEnabled
            || c.performance.transform != c.owner.transform || c.rush.transform != c.owner.transform
            || !c.p3.IsConfigured || All<P3BodyIntrusion>(c.scene).Count() != 1)
            throw new InvalidOperationException("Install/retain round 3 P3 on the one present actor before applying acting. No installation was changed.");
        var p = new SerializedObject(c.performance); var i = new SerializedObject(c.p3);
        var anatomy = Get<RecordedCharacterMotion>(p, "anatomy");
        if (!anatomy || !anatomy.IsPresentThreat || !anatomy.ModelRoot || anatomy.Joints == null
            || anatomy.Joints.Any(t => !t) || !anatomy.transform.IsChildOf(c.owner.transform)
            || anatomy.GetComponentInParent<EchoPlayback>() || anatomy.GetComponentInParent<LoopActor>()
            || c.owner.GetComponentsInChildren<Animator>(true).Any(a => a.enabled)
            || c.owner.GetComponentsInChildren<Animation>(true).Any(a => a.enabled)
            || Get<RecordedCharacterMotion>(i, "anatomy") != anatomy || Get<MachineThreatPerformance>(i, "performance") != c.performance
            || Get<IntruderStaging>(i, "owner") != c.owner || Get<CharacterController>(i, "controller") != controller)
            throw new InvalidOperationException("Acting requires the original exclusive present rig, with no competing animation writer.");
        foreach (string joint in new[] { "hips", "spine", "chest", "neck", "head", "jaw", "leftThigh", "leftShin", "leftFoot", "rightThigh", "rightShin", "rightFoot", "leftArm", "leftForearm", "leftHand", "rightArm", "rightForearm", "rightHand" })
        {
            var bone = Get<Transform>(p, joint);
            if (!bone || !anatomy.Joints.Contains(bone) || !bone.IsChildOf(anatomy.ModelRoot))
                throw new InvalidOperationException("Missing exclusive actor joint: " + joint);
            if (i.FindProperty(joint) != null && Get<Transform>(i, joint) != bone)
                throw new InvalidOperationException("P3/performance disagree on joint " + joint);
        }
        c.leftEye = Joint(anatomy, "eye_l"); c.rightEye = Joint(anatomy, "eye_r");
        CheckPalm(i, "highPalm", new Vector3(-6.2425f, 1.43f, 36.28f), anatomy);
        CheckPalm(i, "lowPalm", new Vector3(-6.2425f, 1.40f, 35.82f), anatomy);
        var stop = Get<Transform>(i, "stop");
        if (!stop || Vector3.Distance(stop.position, new Vector3(-5.68f, .03f, 36.02f)) > .001f)
            throw new InvalidOperationException("P3 stop changed; retain the collision-checked round 3 stop.");
        foreach (string name in new[] { "contact", "support" })
        {
            var source = Get<AudioSource>(i, name);
            if (!source || !source.clip || !source.outputAudioMixerGroup || source.outputAudioMixerGroup.name != "WorldSFX")
                throw new InvalidOperationException("P3 needs its existing routed " + name + " source.");
        }
        var installed = All<IntruderActingFocus>(c.scene).ToArray();
        var performanceFocus = Get<IntruderActingFocus>(p, "acting"); var p3Focus = Get<IntruderActingFocus>(i, "acting");
        if (installed.Length == 0)
        {
            if (performanceFocus || p3Focus || Get<Transform>(i, "viewer") || c.rush.UsesAuthoredActing)
                throw new InvalidOperationException("Partial acting configuration; resolve it or Undo before applying.");
            return c;
        }
        if (installed.Length != 1) throw new InvalidOperationException("More than one round 4 gaze owner; nothing changed.");
        c.acting = installed[0]; var f = new SerializedObject(c.acting);
        if (c.acting.transform != c.owner.transform || !c.acting.isActiveAndEnabled || !c.acting.IsConfigured
            || performanceFocus != c.acting || p3Focus != c.acting || Get<Transform>(i, "viewer") != c.viewer
            || Get<Transform>(f, "leftEye") != c.leftEye || Get<Transform>(f, "rightEye") != c.rightEye || !c.rush.UsesAuthoredActing)
            throw new InvalidOperationException("Partial/altered acting installation. Resolve it or Undo; Apply never overwrites it.");
        return c;
    }

    static void CheckPalm(SerializedObject p3, string name, Vector3 position, RecordedCharacterMotion anatomy)
    {
        var palm = p3.FindProperty(name); var target = palm.FindPropertyRelative("target").objectReferenceValue as Transform;
        if (!target || Vector3.Distance(target.position, position) > .001f)
            throw new InvalidOperationException("Keep the corrected round 3 " + name + " target; do not judge contact from root distance.");
        var arm = palm.FindPropertyRelative("arm");
        foreach (string part in new[] { "upper", "lower", "end" })
            if (!(arm.FindPropertyRelative(part).objectReferenceValue is Transform bone) || !anatomy.Joints.Contains(bone))
                throw new InvalidOperationException("Missing " + name + " chain joint: " + part);
    }
    static Transform Joint(RecordedCharacterMotion anatomy, string name)
        => anatomy.Joints.Single(t => t.name == name || t.name.EndsWith("_" + name, StringComparison.Ordinal));
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static T Get<T>(SerializedObject s, string name) where T : Object => s.FindProperty(name)?.objectReferenceValue as T;
    static void Ref(SerializedObject s, string name, Object value) => s.FindProperty(name).objectReferenceValue = value;
}

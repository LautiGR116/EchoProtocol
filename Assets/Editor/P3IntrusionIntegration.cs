using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.P3Intrusion;
using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Additive to the current scene: no scene loading/saving, rig/model/material
// edits, duplicated actor or changes to any puzzle/pump object.
public static class P3IntrusionIntegration
{
    public const string RootName = "P3Intrusion / glass arrest";
    static readonly Vector3 Initial = new Vector3(-4.55f, .03f, 35.8f);
    static readonly Vector3 Arrest = new Vector3(-5.68f, .03f, 36.02f);
    static readonly Vector3 HighPalm = new Vector3(-6.2425f, 1.43f, 36.28f);
    // Keep the brace within Nathan's actual arm reach through contact/settle.
    static readonly Vector3 LowPalm = new Vector3(-6.2425f, 1.40f, 35.82f);
    const string ContactPath = "Assets/_Game/Audio/Clips/Impact/Threat_FrameContact.wav";
    const string SupportPath = "Assets/_Game/Audio/Clips/Impact/Threat_Step1.wav";

    sealed class Context
    {
        public Scene scene;
        public IntruderStaging owner;
        public MachineThreatPerformance performance;
        public MachineCoverThreat threat;
        public MachineCoverEncounter encounter;
        public GameSession session;
        public RecordedCharacterMotion anatomy;
        public CharacterController controller;
        public Transform stand;
        public BoxCollider glass;
        public AudioSource oldContact;
        public AudioClip contact, support;
        public P3BodyIntrusion installed;
    }

    [MenuItem("Echo Protocol/P3 intrusion/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        Context c = Validate();
        Debug.Log(c.installed ? "P3 intrusion: installation valid; Apply is a no-op."
            : "P3 intrusion: original actor, authored glass and sources ready. Verify actual collision in Play after B. Apply adds only the P3 performance and its targets/sources.", c.owner);
    }

    [MenuItem("Echo Protocol/P3 intrusion/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        Context c = Validate();
        if (c.installed) { Debug.Log("P3 intrusion already installed; nothing changed.", c.owner); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("P3 body intrusion against existing glass");
        GameObject root = null;
        try
        {
            root = Node(c.stand.parent, RootName, Vector3.zero);
            var stop = Node(root.transform, "Body arrest / behind solid plinth", Arrest).transform;
            stop.rotation = Quaternion.Euler(0, -90, 0);
            var high = Node(root.transform, "Right palm / first contact", HighPalm).transform;
            var low = Node(root.transform, "Left palm / lower brace", LowPalm).transform;
            high.rotation = Quaternion.LookRotation(Vector3.left, Quaternion.AngleAxis(-12, Vector3.left) * Vector3.up);
            low.rotation = Quaternion.LookRotation(Vector3.left, Quaternion.AngleAxis(18, Vector3.left) * Vector3.up);
            var contact = Sound(root.transform, "Glass frame / body contact", HighPalm, c.contact, c.oldContact, .48f);
            var support = Sound(root.transform, "Lead sole / single support", Arrest, c.support, c.oldContact, .22f);
            var act = root.AddComponent<P3BodyIntrusion>();
            var s = new SerializedObject(act);
            Ref(s, "owner", c.owner); Ref(s, "session", c.session); Ref(s, "encounter", c.encounter);
            Ref(s, "threat", c.threat); Ref(s, "performance", c.performance); Ref(s, "controller", c.controller);
            Ref(s, "anatomy", c.anatomy); Ref(s, "stop", stop); Ref(s, "glazing", c.glass);
            Ref(s, "contact", contact); Ref(s, "support", support);
            var p = new SerializedObject(c.performance);
            foreach (string name in new[] { "hips", "spine", "chest", "neck", "head", "jaw" }) Ref(s, name, Get<Transform>(p, name));
            CopyArray(p, s, "brows"); CopyArray(p, s, "mouthCorners");
            Leg(s.FindProperty("leftLeg"), p, "left"); Leg(s.FindProperty("rightLeg"), p, "right");
            Hand(s.FindProperty("highPalm"), c.anatomy, p, "right", "r", high);
            Hand(s.FindProperty("lowPalm"), c.anatomy, p, "left", "l", low);
            s.ApplyModifiedPropertiesWithoutUndo();
            if (!act.IsConfigured) throw new InvalidOperationException("P3 wiring failed. Rolling back the additive installation.");

            // Register the completed hierarchy so Undo/Redo includes all its
            // components/references. Only this optional owner link is modified.
            Undo.RegisterCreatedObjectUndo(root, "Create P3 intrusion targets and sources");
            Undo.RecordObject(c.owner, "Assign exclusive P3 pose owner");
            var owner = new SerializedObject(c.owner); Ref(owner, "p3Intrusion", act); owner.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(c.owner);
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("P3 intrusion installed; scene UNSAVED. One existing body, 1.15 m approach, two palms and held stillness. Original scrape is bypassed. Undo restores the prior presentation. Central Play check: real Echo returns to A, then pump entry.", c.owner);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (root) Object.DestroyImmediate(root);
            throw;
        }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before integrating P3.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Use the existing Incursion scene. This operation never opens or saves a scene.");
        c.owner = All<IntruderStaging>(c.scene).Single();
        var o = new SerializedObject(c.owner);
        c.performance = Get<MachineThreatPerformance>(o, "performance"); c.threat = Get<MachineCoverThreat>(o, "threat");
        c.encounter = Get<MachineCoverEncounter>(o, "encounter"); c.session = Get<GameSession>(o, "session");
        c.stand = Get<Transform>(o, "p3Stand"); c.oldContact = Get<AudioSource>(o, "p3Contact");
        c.controller = c.owner.GetComponent<CharacterController>();
        var relay = Get<RecombinationRelay>(o, "returnRelay");
        if (!c.performance || !c.threat || !c.encounter || !c.session || !c.stand || !c.oldContact || !c.controller
            || !relay || !relay.RequiresReturnWitness || !Get<Camera>(o, "view") || !Get<Transform>(o, "player")
            || !Get<Light>(o, "p3Practical") || !c.owner.isActiveAndEnabled || !c.threat.isActiveAndEnabled
            || !c.performance.isActiveAndEnabled || !c.controller.enabled || !c.oldContact.outputAudioMixerGroup
            || c.oldContact.outputAudioMixerGroup.name != "WorldSFX")
            throw new InvalidOperationException("P3 needs the original present actor, return witness, camera and WorldSFX source.");
        if (c.performance.transform != c.owner.transform || c.threat.transform != c.owner.transform
            || c.owner.transform.lossyScale != Vector3.one || !c.stand.parent || c.stand.parent.lossyScale != Vector3.one
            || Vector3.Distance(c.stand.position, Initial) > .015f || Quaternion.Angle(c.stand.rotation, Quaternion.Euler(0, -90, 0)) > .1f
            || Mathf.Abs(c.controller.radius - .4f) > .001f || c.controller.skinWidth > .036f
            || Mathf.Abs(c.controller.height - 1.75f) > .001f || Vector3.Distance(c.controller.center, new Vector3(0, .88f, 0)) > .001f)
            throw new InvalidOperationException("P3 original stand/actor/controller changed. Review the short approach before applying.");
        var p = new SerializedObject(c.performance);
        c.anatomy = Get<RecordedCharacterMotion>(p, "anatomy");
        if (!c.anatomy || !c.anatomy.enabled || !c.anatomy.IsPresentThreat || !c.anatomy.ModelRoot
            || !c.anatomy.transform.IsChildOf(c.owner.transform) || c.anatomy.GetComponentInParent<EchoPlayback>()
            || c.anatomy.GetComponentInParent<LoopActor>() || c.anatomy.Joints == null || c.anatomy.Joints.Any(t => !t)
            || c.owner.GetComponentsInChildren<Animator>(true).Any(a => a.enabled)
            || c.owner.GetComponentsInChildren<Animation>(true).Any(a => a.enabled))
            throw new InvalidOperationException("The one present Nathan rig must have explicit joints and no competing Animator/recorded actor.");
        foreach (string name in new[] { "hips", "spine", "chest", "neck", "head", "jaw", "leftThigh", "leftShin", "leftFoot", "rightThigh", "rightShin", "rightFoot", "leftArm", "leftForearm", "leftHand", "rightArm", "rightForearm", "rightHand" })
        {
            var joint = Get<Transform>(p, name);
            if (!joint || !joint.IsChildOf(c.anatomy.ModelRoot) || !c.anatomy.Joints.Contains(joint))
                throw new InvalidOperationException("Missing present-actor joint: " + name);
        }
        foreach (string suffix in new[] { "l", "r" }) foreach (string finger in new[] { "index_01_", "middle_01_", "pinky_01_" }) Joint(c.anatomy, finger + suffix);
        var glazing = o.FindProperty("p3Glazing");
        c.glass = Enumerable.Range(0, glazing.arraySize).Select(i => glazing.GetArrayElementAtIndex(i).objectReferenceValue as BoxCollider)
            .Single(b => b && b.enabled && b.name == "A sealed glass");
        // P3's gated decor is inactive in Edit Mode. Collider.bounds and
        // ClosestPoint cannot validate it there; use the authored local box.
        if (c.glass.isTrigger || Mathf.Abs(AuthoredBounds(c.glass).max.x - -6.2825f) > .005f
            || Vector3.Distance(AuthoredClosestPoint(c.glass, HighPalm), HighPalm) > .045f
            || Vector3.Distance(AuthoredClosestPoint(c.glass, LowPalm), LowPalm) > .045f
            || Physics.GetIgnoreLayerCollision(c.owner.gameObject.layer, c.glass.gameObject.layer))
            throw new InvalidOperationException("The authored A glass must remain solid with both wrists just inside its east face.");
        // Actual controller clearance/collision belongs to the central Play
        // check after B activates the decor. Never activate it for preflight.
        c.contact = AssetDatabase.LoadAssetAtPath<AudioClip>(ContactPath); c.support = AssetDatabase.LoadAssetAtPath<AudioClip>(SupportPath);
        if (!c.contact || !c.support || c.contact.channels != 1 || c.support.channels != 1)
            throw new InvalidOperationException("Import the existing licensed mono Impact contacts; no new audio download is required.");
        c.installed = Get<P3BodyIntrusion>(o, "p3Intrusion");
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        var acts = All<P3BodyIntrusion>(c.scene).ToArray();
        if (roots.Length > 0 || acts.Length > 0 || c.installed)
        {
            if (roots.Length != 1 || acts.Length != 1 || !c.installed || acts[0] != c.installed
                || roots[0] != c.installed.transform || roots[0].parent != c.stand.parent || !c.installed.isActiveAndEnabled || !c.installed.IsConfigured)
                throw new InvalidOperationException("Partial/modified P3 installation. Undo or resolve it before applying; nothing changed.");
            var installed = new SerializedObject(c.installed);
            if (Get<IntruderStaging>(installed, "owner") != c.owner || Get<RecordedCharacterMotion>(installed, "anatomy") != c.anatomy
                || Get<BoxCollider>(installed, "glazing") != c.glass || Vector3.Distance(Get<Transform>(installed, "stop").position, Arrest) > .001f
                || Vector3.Distance(installed.FindProperty("highPalm").FindPropertyRelative("target").objectReferenceValue is Transform h ? h.position : Vector3.zero, HighPalm) > .001f
                || Vector3.Distance(installed.FindProperty("lowPalm").FindPropertyRelative("target").objectReferenceValue is Transform l ? l.position : Vector3.zero, LowPalm) > .001f
                || Get<AudioSource>(installed, "contact").clip != c.contact || Get<AudioSource>(installed, "support").clip != c.support)
                throw new InvalidOperationException("P3 references/stop changed; review before playing.");
        }
        return c;
    }

    static Bounds AuthoredBounds(BoxCollider box)
    {
        Vector3 half = box.size * .5f;
        var bounds = new Bounds(box.transform.TransformPoint(box.center), Vector3.zero);
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    bounds.Encapsulate(box.transform.TransformPoint(box.center + new Vector3(half.x * x, half.y * y, half.z * z)));
        return bounds;
    }

    static Vector3 AuthoredClosestPoint(BoxCollider box, Vector3 worldPoint)
    {
        Vector3 local = box.transform.InverseTransformPoint(worldPoint), half = box.size * .5f;
        Vector3 min = box.center - half, max = box.center + half;
        return box.transform.TransformPoint(new Vector3(Mathf.Clamp(local.x, min.x, max.x),
            Mathf.Clamp(local.y, min.y, max.y), Mathf.Clamp(local.z, min.z, max.z)));
    }

    static void Leg(SerializedProperty limb, SerializedObject p, string side)
    { Limb(limb, Get<Transform>(p, side + "Thigh"), Get<Transform>(p, side + "Shin"), Get<Transform>(p, side + "Foot")); }
    static void Limb(SerializedProperty limb, Transform upper, Transform lower, Transform end)
    { limb.FindPropertyRelative("upper").objectReferenceValue = upper; limb.FindPropertyRelative("lower").objectReferenceValue = lower; limb.FindPropertyRelative("end").objectReferenceValue = end; }
    static void Hand(SerializedProperty palm, RecordedCharacterMotion anatomy, SerializedObject p, string side, string suffix, Transform target)
    {
        Limb(palm.FindPropertyRelative("arm"), Get<Transform>(p, side + "Arm"), Get<Transform>(p, side + "Forearm"), Get<Transform>(p, side + "Hand"));
        palm.FindPropertyRelative("target").objectReferenceValue = target;
        palm.FindPropertyRelative("left").boolValue = side == "left";
        palm.FindPropertyRelative("index").objectReferenceValue = Joint(anatomy, "index_01_" + suffix);
        palm.FindPropertyRelative("middle").objectReferenceValue = Joint(anatomy, "middle_01_" + suffix);
        palm.FindPropertyRelative("little").objectReferenceValue = Joint(anatomy, "pinky_01_" + suffix);
        var digits = anatomy.Joints.Where(t => t.name.EndsWith("_" + suffix, StringComparison.Ordinal)
            && new[] { "index_", "middle_", "ring_", "pinky_", "thumb_" }.Any(prefix => t.name.StartsWith(prefix, StringComparison.Ordinal) || t.name.Contains("_" + prefix))).ToArray();
        var array = palm.FindPropertyRelative("digits"); array.arraySize = digits.Length;
        for (int i = 0; i < digits.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = digits[i];
    }
    static void CopyArray(SerializedObject from, SerializedObject to, string name)
    {
        var a = from.FindProperty(name); var b = to.FindProperty(name); b.arraySize = a.arraySize;
        for (int i = 0; i < a.arraySize; i++) b.GetArrayElementAtIndex(i).objectReferenceValue = a.GetArrayElementAtIndex(i).objectReferenceValue;
    }
    static AudioSource Sound(Transform parent, string name, Vector3 at, AudioClip clip, AudioSource template, float gain)
    {
        var s = Node(parent, name, at).AddComponent<AudioSource>();
        s.clip = clip; s.playOnAwake = s.loop = false; s.spatialBlend = 1; s.dopplerLevel = 0;
        s.outputAudioMixerGroup = template.outputAudioMixerGroup; s.volume = gain;
        s.minDistance = .9f; s.maxDistance = 6; s.rolloffMode = AudioRolloffMode.Linear; s.reverbZoneMix = .04f;
        s.ignoreListenerPause = s.ignoreListenerVolume = s.bypassListenerEffects = false;
        return s;
    }
    static GameObject Node(Transform parent, string name, Vector3 position)
    { var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position; return go; }
    static Transform Joint(RecordedCharacterMotion anatomy, string name) => anatomy.Joints.Single(t => t.name == name || t.name.EndsWith("_" + name, StringComparison.Ordinal));
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static T Get<T>(SerializedObject s, string name) where T : Object => s.FindProperty(name)?.objectReferenceValue as T;
    static void Ref(SerializedObject s, string name, Object value) => s.FindProperty(name).objectReferenceValue = value;
}

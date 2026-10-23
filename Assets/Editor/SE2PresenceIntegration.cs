using System;
using System.Linq;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Horror.SE2Presence;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Operates only on the current Incursion SE2. No scene loading/saving, rig asset
// edits, P2 writes, light toggles or changes to the presentation/shadow gates.
public static class SE2PresenceIntegration
{
    public const string RootName = "SE2Presence / glass pressure";
    const string AudioFolder = "Assets/_Game/Audio/SE2Presence/";
    const string MaterialFolder = "Assets/_Game/Materials/SE2Presence";
    const float PaneX = 4.65f, FigureZ = 29.48f;
    static readonly Vector3 FigurePosition = new Vector3(5.08f, .025f, FigureZ);

    sealed class Context
    {
        public Scene scene;
        public DomesticPresence owner;
        public RecordedCharacterMotion anatomy;
        public VS1ParallelFigureEvent passage;
        public GameObject figure;
        public BoxCollider envelope, originalGlass;
        public AudioSource oldSource;
        public AudioClip seat, slip;
        public SkinnedMeshRenderer surface;
        public SE2GlassPressure installed;
        public Transform chest, neck, head;
    }

    [MenuItem("Echo Protocol/SE2 presence/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "SE2 glass pressure: valid installation; Apply is a no-op."
            : "SE2 glass pressure: ready. Existing reveal/shadow envelopes will remain unchanged. Scene will remain unsaved.", c.owner);
    }

    [MenuItem("Echo Protocol/SE2 presence/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        Context c = Validate();
        if (c.installed) { Debug.Log("SE2 glass pressure is already installed; nothing changed.", c.owner); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("SE2 adult against glass");
        GameObject root = null;
        var createdMaterials = new System.Collections.Generic.List<string>();
        try
        {
            Material pane = Material("M_SE2PressureGlass", new Color(.62f, .71f, .68f, .055f), true, createdMaterials);
            Material edge = Material("M_SE2PressureEdge", new Color(.26f, .30f, .29f, 1), false, createdMaterials);
            root = Node(c.owner.transform, RootName, Vector3.zero);

            // This fixed interior safety pane is present in BOTH room states.
            // It keeps the adult inside the existing x >= 4.595 change envelope.
            // Narrow edge/clips only: the body, not a new prop, owns the image.
            var paneObject = Box(root.transform, "Fixed inner safety glazing", new Vector3(PaneX, 1.32f, FigureZ),
                new Vector3(.018f, 2.10f, 1.17f), pane, true);
            paneObject.layer = 2; // Ignore Raycast: solid transparent glass, like the frontage exception.
            foreach (float z in new[] { FigureZ - .585f, FigureZ + .585f })
            {
                Box(root.transform, "Glazing edge", new Vector3(PaneX, 1.32f, z), new Vector3(.023f, 2.1f, .008f), edge, false);
                foreach (float y in new[] { .40f, 2.22f })
                    Box(root.transform, "Retaining clip", new Vector3(PaneX, y, z), new Vector3(.055f, .07f, .055f), edge, false);
            }

            // Wrist centre sits behind the inside face, allowing the scanned
            // palm's thickness; the wrist bone itself is not the contact surface.
            var high = Node(root.transform, "Left palm / weight", new Vector3(PaneX + .030f, 1.48f, FigureZ - .24f)).transform;
            var low = Node(root.transform, "Right palm / brace", new Vector3(PaneX + .030f, 1.04f, FigureZ + .23f)).transform;
            high.rotation = Quaternion.LookRotation(Vector3.left, Quaternion.AngleAxis(-9, Vector3.left) * Vector3.up);
            low.rotation = Quaternion.LookRotation(Vector3.left, Quaternion.AngleAxis(14, Vector3.left) * Vector3.up);

            // A constant, bounded side wash makes skin and palms readable. It is
            // never tied to discovery and cannot pop in or cast changing shadows.
            var wash = Node(root.transform, "Fixed inspection spill", new Vector3(4.79f, 2.20f, FigureZ - .50f));
            var light = wash.AddComponent<Light>();
            light.type = LightType.Spot; light.color = new Color(.70f, .78f, .74f);
            light.intensity = .7f; light.range = 2f; light.spotAngle = 77f; light.innerSpotAngle = 42f;
            light.shadows = LightShadows.None;
            wash.transform.LookAt(new Vector3(4.88f, 1.45f, FigureZ));
            Box(root.transform, "Inspection strip housing", new Vector3(4.78f, 2.24f, FigureZ - .50f), new Vector3(.07f, .035f, .17f), edge, false);

            var sound = Node(root.transform, "Palm contact / present world", high.position).AddComponent<AudioSource>();
            sound.playOnAwake = false; sound.loop = false; sound.spatialBlend = 1; sound.dopplerLevel = 0;
            sound.outputAudioMixerGroup = c.oldSource.outputAudioMixerGroup;
            sound.volume = Mathf.Min(.65f, c.oldSource.volume);
            sound.minDistance = 1.1f; sound.maxDistance = 6f; sound.rolloffMode = AudioRolloffMode.Linear;
            sound.ignoreListenerPause = sound.ignoreListenerVolume = sound.bypassListenerEffects = false;
            sound.reverbZoneMix = .05f;

            // Move only the already separate present actor, never the shared
            // model/prefab, presentation root, optional look target or Player.
            Undo.RecordObject(c.anatomy.transform, "Place present adult behind inner pane");
            c.anatomy.transform.SetPositionAndRotation(FigurePosition, Quaternion.Euler(0, 270f, 0));
            PrefabUtility.RecordPrefabInstancePropertyModifications(c.anatomy.transform);
            foreach (var renderer in c.anatomy.GetComponentsInChildren<Renderer>(true))
            {
                Undo.RecordObject(renderer, "Confine SE2 changing silhouette");
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }

            var pressure = root.AddComponent<SE2GlassPressure>();
            var s = new SerializedObject(pressure);
            Ref(s, "anatomy", c.anatomy); Ref(s, "chest", c.chest); Ref(s, "neck", c.neck); Ref(s, "head", c.head);
            Ref(s, "surface", c.surface); Ref(s, "contactSound", sound); Ref(s, "seatClip", c.seat); Ref(s, "slipClip", c.slip);
            Palm(s.FindProperty("highPalm"), c.anatomy, "left", "l", high);
            Palm(s.FindProperty("lowPalm"), c.anatomy, "right", "r", low);
            var lids = new SerializedObject(c.owner).FindProperty("upperEyelids");
            var targetLids = s.FindProperty("upperEyelids"); targetLids.arraySize = lids.arraySize;
            for (int i = 0; i < lids.arraySize; i++) targetLids.GetArrayElementAtIndex(i).objectReferenceValue = lids.GetArrayElementAtIndex(i).objectReferenceValue;
            s.ApplyModifiedPropertiesWithoutUndo();
            if (!pressure.IsConfigured) throw new InvalidOperationException("SE2 pressure wiring failed; undoing installation.");
            // Capture the complete new hierarchy, including components/targets,
            // before linking the owner. Redo must restore a working installation.
            Undo.RegisterCreatedObjectUndo(root, "Create SE2 pressure composition");
            Undo.RecordObject(c.owner, "Use present glass contact performance");
            var owner = new SerializedObject(c.owner); Ref(owner, "glassPressure", pressure); owner.ApplyModifiedProperties();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = c.anatomy.gameObject;
            Debug.Log("SE2 pressure installed, scene UNSAVED. Natural discovery + one palm slide; no gate changes. Check fingertips/face against pane and the oblique exit in Play. Undo restores the prior composition.", c.owner);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (root) Object.DestroyImmediate(root);
            foreach (string path in createdMaterials) AssetDatabase.DeleteAsset(path);
            throw;
        }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before applying SE2 presence.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Open the existing Incursion scene. This operation never opens or saves scenes.");
        c.owner = One<DomesticPresence>(c.scene);
        var o = new SerializedObject(c.owner);
        c.anatomy = Get<RecordedCharacterMotion>(o, "anatomy"); c.passage = Get<VS1ParallelFigureEvent>(o, "passage");
        c.oldSource = Get<AudioSource>(o, "frameSound"); c.originalGlass = Get<BoxCollider>(o, "observationGlass");
        c.chest = Get<Transform>(o, "chest"); c.neck = Get<Transform>(o, "neck"); c.head = Get<Transform>(o, "head");
        if (!c.anatomy || !c.passage || !c.originalGlass || !c.oldSource || !c.oldSource.outputAudioMixerGroup
            || !c.chest || !c.neck || !c.head || !c.owner.isActiveAndEnabled || !c.anatomy.enabled)
            throw new InvalidOperationException("The current static SE2 adult and its explicit discovery references are required.");
        var p = new SerializedObject(c.passage);
        c.figure = Get<GameObject>(p, "figureRoot"); c.envelope = Get<BoxCollider>(p, "changeEnvelope");
        if (!p.FindProperty("physicalTraversal").boolValue || !c.figure || !c.envelope
            || !c.anatomy.transform.IsChildOf(c.figure.transform) || c.anatomy.IsPresentThreat
            || c.anatomy.GetComponentInParent<EchoPlayback>() || c.anatomy.GetComponentInParent<LoopActor>())
            throw new InvalidOperationException("SE2 must be a separate present adult inside the physical passage, never a Player/Echo actor.");
        if (c.anatomy.gameObject.scene != c.scene || c.passage.gameObject.scene != c.scene || c.originalGlass.gameObject.scene != c.scene
            || Vector3.Distance(c.originalGlass.bounds.center, new Vector3(4f, 1.65f, 29.1f)) > .025f
            || c.anatomy.transform.lossyScale != Vector3.one || c.owner.transform.lossyScale != Vector3.one)
            throw new InvalidOperationException("SE2 coordinates/scale changed; review the composition before applying.");
        // The entire new moving body/contact box fits the already protected
        // presentation volume. No request to widen gates or arbitrary camera look.
        Bounds staging = new Bounds(new Vector3(5.10f, 1.18f, FigureZ), new Vector3(.96f, 2.26f, 1.1f));
        if (!c.envelope.bounds.Contains(staging.min) || !c.envelope.bounds.Contains(staging.max)
            || !c.envelope.enabled || Physics.GetIgnoreLayerCollision(0, 2))
            throw new InvalidOperationException("Existing hidden-change envelope or Default/IgnoreRaycast collision cannot safely contain this composition.");
        c.surface = c.anatomy.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "RecordedAdultSurface");
        if (!c.surface.sharedMesh || c.surface.sharedMaterials.Length < 2 || !c.surface.sharedMaterials[0]
            || !c.surface.sharedMaterials[0].HasProperty("_BaseColor")) throw new InvalidOperationException("Original Eric surface/maps are required; no private assets are copied.");
        foreach (string side in new[] { "l", "r" })
            foreach (string joint in new[] { "index_01_", "middle_01_", "pinky_01_" }) Joint(c.anatomy, joint + side);
        foreach (string side in new[] { "left", "right" })
            foreach (string limb in new[] { "Arm", "Forearm", "Hand" })
                if (!Get<Transform>(new SerializedObject(c.anatomy), side + limb)) throw new InvalidOperationException("Adult hand chain incomplete.");
        c.seat = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "SE2_PalmSeat.wav");
        c.slip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "SE2_HandSlip.wav");
        if (!c.seat || !c.slip) throw new InvalidOperationException("Import the two small SE2Presence audio masters first.");
        if (!Shader.Find("Universal Render Pipeline/Lit")) throw new InvalidOperationException("Existing URP Lit shader missing.");
        c.installed = Get<SE2GlassPressure>(o, "glassPressure");
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        var components = All<SE2GlassPressure>(c.scene).ToArray();
        if (roots.Length != 0 || components.Length != 0 || c.installed)
        {
            if (roots.Length != 1 || components.Length != 1 || !c.installed || components[0] != c.installed
                || roots[0] != c.installed.transform || roots[0].parent != c.owner.transform || !c.installed.IsConfigured
                || !c.installed.isActiveAndEnabled || Get<RecordedCharacterMotion>(new SerializedObject(c.installed), "anatomy") != c.anatomy
                || Vector3.Distance(c.anatomy.transform.position, FigurePosition) > .001f
                || Quaternion.Angle(c.anatomy.transform.rotation, Quaternion.Euler(0, 270, 0)) > .01f
                || c.anatomy.GetComponentsInChildren<Renderer>(true).Any(r => r.shadowCastingMode != ShadowCastingMode.Off))
                throw new InvalidOperationException("Partial/modified SE2 pressure installation. Resolve or Undo it before reapplying; nothing changed.");
            var pane = roots[0].Find("Fixed inner safety glazing");
            if (!pane || !pane.GetComponent<BoxCollider>() || pane.gameObject.layer != 2 || !pane.GetComponent<BoxCollider>().enabled
                || Mathf.Abs(pane.position.x - PaneX) > .001f) throw new InvalidOperationException("SE2 safety pane changed; inspect before playing.");
        }
        return c;
    }

    static void Palm(SerializedProperty p, RecordedCharacterMotion adult, string side, string suffix, Transform target)
    {
        var a = new SerializedObject(adult);
        p.FindPropertyRelative("upper").objectReferenceValue = Get<Transform>(a, side + "Arm");
        p.FindPropertyRelative("lower").objectReferenceValue = Get<Transform>(a, side + "Forearm");
        p.FindPropertyRelative("hand").objectReferenceValue = Get<Transform>(a, side + "Hand");
        p.FindPropertyRelative("index").objectReferenceValue = Joint(adult, "index_01_" + suffix);
        p.FindPropertyRelative("middle").objectReferenceValue = Joint(adult, "middle_01_" + suffix);
        p.FindPropertyRelative("little").objectReferenceValue = Joint(adult, "pinky_01_" + suffix);
        p.FindPropertyRelative("target").objectReferenceValue = target;
        p.FindPropertyRelative("left").boolValue = side == "left";
        var digits = adult.GetComponentsInChildren<Transform>(true).Where(t => t.name.EndsWith("_" + suffix, StringComparison.Ordinal)
            && new[] { "index_", "middle_", "ring_", "pinky_", "thumb_" }.Any(prefix => t.name.StartsWith(prefix, StringComparison.Ordinal))).ToArray();
        var array = p.FindPropertyRelative("digits"); array.arraySize = digits.Length;
        for (int i = 0; i < digits.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = digits[i];
    }

    static Material Material(string name, Color color, bool transparent, System.Collections.Generic.List<string> created)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing) return existing; // A repeat after Undo reuses this SE2-only material.
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/_Game/Materials", "SE2Presence");
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", transparent ? .75f : .3f);
        if (transparent)
        {
            m.SetFloat("_Surface", 1); m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0);
            m.SetFloat("_Cull", (float)CullMode.Off); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent"); m.renderQueue = (int)RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
        }
        AssetDatabase.CreateAsset(m, path); created.Add(path); return m;
    }

    static GameObject Node(Transform parent, string name, Vector3 position)
    { var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.position = position; return g; }
    static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.position = position; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        var r = g.GetComponent<Renderer>(); r.sharedMaterial = material; r.shadowCastingMode = ShadowCastingMode.Off;
        return g;
    }
    static Transform Joint(RecordedCharacterMotion adult, string name) => adult.GetComponentsInChildren<Transform>(true).Single(t => t.name == name);
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static T One<T>(Scene scene) where T : Component => All<T>(scene).Single();
    static T Get<T>(SerializedObject s, string name) where T : Object => s.FindProperty(name)?.objectReferenceValue as T;
    static void Ref(SerializedObject s, string name, Object value) => s.FindProperty(name).objectReferenceValue = value;
}

using System;
using System.Linq;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Operates on the coordinator's already-open Incursion scene. No scene IO,
// collider changes, reader changes, imported assets or project settings.
public static class P2CausalIntegration
{
    const string RootName = "P2 Causal / lateral witness";
    const string UndoName = "Install P2 lateral causal witness";
    static readonly Vector3 PanelCentre = new Vector3(-1.40f, 1.57f, 24.25f);

    sealed class Context
    {
        public Scene scene;
        public P2AirlockPresentation presentation;
        public BlindAlignmentRelay relay;
        public OpticalReader reader;
        public SerializedObject fields;
        public Transform root, platform, entranceLeaf, exitLeaf, entryOpening, exitOpening;
        public TextMesh cause;
        public P2CausalFeedback existing;
        public Material bezel, metal, pilot;
        public AudioSource stationContact;
    }

    [MenuItem("Echo Protocol/P2 Causal/Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.existing ? "P2 lateral witness is installed and connected; no changes."
            : "P2 causal preflight passed. Apply adds presentation and relocates only the existing post repeaters; scene stays unsaved.", c.presentation);
    }

    [MenuItem("Echo Protocol/P2 Causal/Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.existing) { Debug.Log("P2 lateral witness already installed; no changes.", c.existing); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        GameObject root = null, footprint = null;
        try
        {
            root = new GameObject(RootName);
            root.transform.SetParent(c.root, false);
            var feedback = root.AddComponent<P2CausalFeedback>();

            // At the reader the player faces east. The witness faces south:
            // reading it requires a turn while BOTH feet remain on the live post.
            // Door 1 remains in the same view, left of this case. Door 2's actual
            // movement is repeated here because the original corner hides it.
            Box(root.transform, "Witness case", PanelCentre, new Vector3(1.10f, .98f, .07f), c.bezel);
            Box(root.transform, "Wall bracket", new Vector3(-.87f, 1.17f, 24.29f), new Vector3(.64f, .055f, .09f), c.metal);
            Box(root.transform, "Case lower rail", new Vector3(-1.40f, 1.10f, 24.195f), new Vector3(1.04f, .025f, .035f), c.metal);
            MoveRepeater(c, "entrance", c.entryOpening, -1.68f);
            MoveRepeater(c, "exit", c.exitOpening, -1.12f);
            Move(c.cause.transform, new Vector3(-1.40f, 1.99f, 24.185f), Quaternion.identity);
            // Retains the already-authored causal sentence, font and status text.
            Move(c.stationContact.transform, new Vector3(-1.40f, 1.55f, 24.16f), Quaternion.identity);

            // Two visible armatures expose the same current reader state. The
            // one by the optic moves across its face; the lateral one moves right.
            var optic = Child(root.transform, "Optic contact pivot", new Vector3(-.705f, 1.28f, 23.46f));
            optic.transform.rotation = Quaternion.Euler(0, 90, 0);
            var readerArmature = LocalBox(optic.transform, "Optic contact", Vector3.zero, new Vector3(.10f, .13f, .035f), c.metal);
            var witness = Child(root.transform, "Witness contact pivot", new Vector3(-1.75f, 1.29f, 24.18f));
            var witnessArmature = LocalBox(witness.transform, "Gaze contact", Vector3.zero, new Vector3(.10f, .10f, .035f), c.metal);
            Box(root.transform, "Gaze contact stop", new Vector3(-1.64f, 1.29f, 24.18f), new Vector3(.03f, .15f, .035f), c.metal);
            var power = Box(root.transform, "Continuous presence supply", new Vector3(-1.40f, 1.17f, 24.185f), new Vector3(.96f, .035f, .025f), c.pilot);
            var gaze = Box(root.transform, "Reader contact path", new Vector3(-1.53f, 1.29f, 24.185f), new Vector3(.18f, .025f, .025f), c.pilot);
            Box(root.transform, "Transfer lock socket", new Vector3(-1.33f, 1.73f, 24.185f), new Vector3(.05f, .13f, .04f), c.bezel);
            var boltPivot = Child(root.transform, "Transfer bolt pivot", new Vector3(-1.43f, 1.73f, 24.15f));
            var bolt = LocalBox(boltPivot.transform, "Actual exit release bolt", Vector3.zero, new Vector3(.17f, .055f, .035f), c.metal);

            // A fixed conduit physically joins the reader and lateral case;
            // no moving arrows or timed demonstration that could promise a solve.
            Box(root.transform, "Reader conduit down", new Vector3(-.71f, 1.03f, 23.46f), new Vector3(.025f, .36f, .025f), c.metal);
            Box(root.transform, "Reader conduit turn", new Vector3(-.71f, .86f, 23.86f), new Vector3(.025f, .025f, .80f), c.metal);
            Box(root.transform, "Witness conduit join", new Vector3(-.785f, .86f, 24.25f), new Vector3(.15f, .025f, .025f), c.metal);
            Box(root.transform, "Witness conduit rise", new Vector3(-.86f, 1.00f, 24.25f), new Vector3(.025f, .28f, .025f), c.metal);

            // Physical footprint corners communicate the powered area through
            // both orientations. They follow the already-moving platform.
            footprint = Child(root.transform, "Post turn footprint", Vector3.zero);
            footprint.transform.SetParent(c.platform, false);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                LocalBox(footprint.transform, "Footprint horizontal", new Vector3(x * .33f, .52f, z * .42f), new Vector3(.18f, .045f, .018f), c.metal);
                LocalBox(footprint.transform, "Footprint vertical", new Vector3(x * .41f, .52f, z * .35f), new Vector3(.018f, .045f, .16f), c.metal);
            }
            var spillObject = Child(root.transform, "Witness fixed service light", new Vector3(-1.40f, 2.09f, 23.85f));
            var spill = spillObject.AddComponent<Light>();
            spill.type = LightType.Point; spill.color = new Color(.78f, .82f, .72f);
            spill.intensity = .38f; spill.range = 1.7f; spill.shadows = LightShadows.None;

            var f = new SerializedObject(feedback);
            Set(f, "relay", c.relay); Set(f, "reader", c.reader);
            Set(f, "readerArmature", readerArmature.transform); Set(f, "witnessArmature", witnessArmature.transform);
            Set(f, "transferBolt", bolt.transform); Set(f, "powerRail", power); Set(f, "gazeRail", gaze);
            f.ApplyModifiedPropertiesWithoutUndo();
            if (!feedback.IsConfigured) throw new InvalidOperationException("P2 witness references failed final validation.");
            // Register completed hierarchies, including their final components
            // and references, so Redo recreates the fully configured witness.
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Undo.RegisterCreatedObjectUndo(footprint, UndoName);
            Undo.RecordObject(c.presentation, UndoName);
            c.fields.Update(); Set(c.fields, "causalFeedback", feedback); c.fields.ApplyModifiedProperties();
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("P2 lateral witness installed. Turn on the post to watch Door 1 close beside its linked witness. Existing reader/door sounds and panel release confirm the real mechanism. Scene remains unsaved; review in the central Editor.", feedback);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (root) Object.DestroyImmediate(root);
            if (footprint) Object.DestroyImmediate(footprint);
            throw;
        }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before P2 integration.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || string.IsNullOrEmpty(c.scene.path))
            throw new InvalidOperationException("Open the saved Incursion scene first; this operation never opens or saves scenes.");
        c.presentation = One<P2AirlockPresentation>(c.scene);
        c.fields = new SerializedObject(c.presentation);
        c.relay = Get<BlindAlignmentRelay>(c.fields, "relay");
        if (!c.relay || !c.relay.RequiresPoweredClosure || c.relay.gameObject.scene != c.scene || !c.relay.isActiveAndEnabled)
            throw new InvalidOperationException("Requires the active powered two-door Incursion P2 relay.");
        var relayFields = new SerializedObject(c.relay);
        c.reader = Get<OpticalReader>(relayFields, "alignmentReader");
        c.platform = Get<Transform>(c.fields, "platform");
        c.root = c.relay.transform.parent;
        if (!c.root || c.root.name != "VS1 P2 Blind Alignment" || c.root.lossyScale != Vector3.one
            || Quaternion.Angle(c.root.rotation, Quaternion.identity) > .01f)
            throw new InvalidOperationException("P2 root was moved or scaled; review the local composition before applying.");
        At(c.platform, new Vector3(-1.5f, .045f, 23.1f));
        At(c.reader ? c.reader.transform : null, new Vector3(-1.5f, 0, 23.1f));
        if (Vector3.Angle(c.reader.transform.forward, Vector3.right) > .1f
            || !c.reader.isActiveAndEnabled || !c.reader.GetComponent<BoxCollider>().isTrigger)
            throw new InvalidOperationException("Expected the east-facing active P2 reader.");
        var entry = Get<SlidingDoor>(c.fields, "entranceGate");
        var exit = Get<SlidingDoor>(c.fields, "exitGate");
        At(entry ? entry.transform : null, new Vector3(-2.75f, 0, 25));
        At(exit ? exit.transform : null, new Vector3(-.6f, 0, 29.1f));
        if (entry != Get<SlidingDoor>(relayFields, "controlledGate") || exit != Get<SlidingDoor>(relayFields, "onwardGate"))
            throw new InvalidOperationException("P2 presentation does not refer to its relay's actual two gates.");
        c.entranceLeaf = Get<Transform>(c.fields, "entranceLeaf"); c.exitLeaf = Get<Transform>(c.fields, "exitLeaf");
        CheckLeaf(entry, c.entranceLeaf, 3.35f); CheckLeaf(exit, c.exitLeaf, 3.05f);
        foreach (string name in new[] { "interior", "transferBoundary", "entranceSafety", "onwardSafety", "acousticInterior" })
        {
            var volume = Get<BoxCollider>(relayFields, name);
            if (!volume || !volume.enabled || !volume.gameObject.activeInHierarchy || volume.gameObject.scene != c.scene)
                throw new InvalidOperationException("Required P2 volume missing: " + name);
        }
        c.entryOpening = Named<Transform>(c.root, "P2 entry miniature recessed opening");
        c.exitOpening = Named<Transform>(c.root, "P2 exit miniature recessed opening");
        c.cause = Named<TextMesh>(c.root, "P2 interlock cause / visible before recording");
        c.stationContact = Get<AudioSource>(c.fields, "stationContact");
        if (!c.stationContact || !c.stationContact.outputAudioMixerGroup || c.stationContact.outputAudioMixerGroup.name != "WorldSFX")
            throw new InvalidOperationException("Existing P2 contactor must retain its WorldSFX output.");
        foreach (string field in new[] { "entranceMimic", "exitMimic", "entrancePilot", "exitPilot", "entranceStatus", "exitStatus" })
        {
            var part = Get<Component>(c.fields, field);
            if (!part || !part.gameObject.activeInHierarchy || part.transform.parent != c.relay.transform
                || part.GetComponentsInChildren<Collider>(true).Length != 0)
                throw new InvalidOperationException("Expected collider-free P2 post presentation: " + field);
        }

        var installed = All<P2CausalFeedback>(c.scene);
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        c.existing = Get<P2CausalFeedback>(c.fields, "causalFeedback");
        if (installed.Length != 0 || roots.Length != 0 || c.existing)
        {
            if (installed.Length != 1 || roots.Length != 1 || !c.existing || installed[0] != c.existing
                || roots[0] != c.existing.transform || c.existing.transform.parent != c.root
                || !c.existing.IsConfigured || !c.existing.isActiveAndEnabled
                || c.existing.GetComponentsInChildren<Collider>(true).Length != 0)
                throw new InvalidOperationException("Partial/foreign P2 witness found. Resolve it or Undo before applying; nothing changed.");
            var installedFields = new SerializedObject(c.existing);
            if (Get<BlindAlignmentRelay>(installedFields, "relay") != c.relay || Get<OpticalReader>(installedFields, "reader") != c.reader)
                throw new InvalidOperationException("Installed witness is linked to a different mechanism.");
            At(Get<Transform>(c.fields, "entranceMimic"), new Vector3(-1.68f, 1.72f, 24.19f));
            At(Get<Transform>(c.fields, "exitMimic"), new Vector3(-1.12f, 1.72f, 24.19f));
            return c;
        }
        c.bezel = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Readers/M_Bezel.mat");
        c.metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Overhaul/M_OV_DullBrass.mat");
        c.pilot = Get<Renderer>(c.fields, "stationPilot")?.sharedMaterial;
        if (!c.bezel || !c.metal || !c.pilot) throw new InvalidOperationException("Required existing P2 materials are unavailable.");
        return c;
    }

    static void MoveRepeater(Context c, string prefix, Transform opening, float x)
    {
        Quaternion edge = Quaternion.Euler(0, 90, 0);
        Move(opening, new Vector3(x, 1.78f, 24.22f), edge);
        Move(Get<Transform>(c.fields, prefix + "Mimic"), new Vector3(x, 1.72f, 24.19f), edge);
        Move(Get<Renderer>(c.fields, prefix + "Pilot").transform, new Vector3(x, 1.57f, 24.185f), edge);
        Move(Get<TextMesh>(c.fields, prefix + "Status").transform, new Vector3(x, 1.48f, 24.185f), Quaternion.identity);
    }

    static void CheckLeaf(SlidingDoor door, Transform leaf, float travel)
    {
        var fields = new SerializedObject(door);
        var body = Get<Rigidbody>(fields, "doorBody");
        var closed = fields.FindProperty("closedLocalPosition").vector3Value;
        var open = fields.FindProperty("openLocalPosition").vector3Value;
        if (!body || body.transform != leaf || !body.isKinematic || leaf.parent != door.transform
            || Vector3.Distance(leaf.localPosition, closed) > .02f || Vector3.Distance(open - closed, Vector3.up * travel) > .02f)
            throw new InvalidOperationException("Expected the closed original P2 leaf/travel. Exit Play and restore the editor scene state.");
    }

    static void At(Transform t, Vector3 expected)
    { if (!t || Vector3.Distance(t.position, expected) > .025f) throw new InvalidOperationException("P2 layout changed near " + expected + "; review before integrating."); }
    static void Move(Transform t, Vector3 position, Quaternion rotation)
    { Undo.RecordObject(t, UndoName); t.SetPositionAndRotation(position, rotation); }
    static GameObject Child(Transform parent, string name, Vector3 position)
    { var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.position = position; return g; }
    static Renderer Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.position = position; g.transform.localScale = size;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        var r = g.GetComponent<Renderer>(); r.sharedMaterial = material;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return r;
    }
    static Renderer LocalBox(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        var r = Box(parent, name, parent.TransformPoint(position), size, material);
        r.transform.localRotation = Quaternion.identity; return r;
    }
    static T[] All<T>(Scene s) where T : Component => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static T One<T>(Scene s) where T : Component => All<T>(s).Single(t => t.gameObject.activeInHierarchy);
    static T Named<T>(Transform root, string name) where T : Component => root.GetComponentsInChildren<T>(true).Single(t => t.name == name);
    static T Get<T>(SerializedObject s, string name) where T : Object => s.FindProperty(name)?.objectReferenceValue as T;
    static void Set(SerializedObject s, string name, Object value)
    { (s.FindProperty(name) ?? throw new MissingFieldException(name)).objectReferenceValue = value; }
}

using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Additive house-only integration on the currently open scene. Never opens,
/// replaces or saves a scene, changes build settings, or authors other chapters.
/// All validation happens before the single undoable mutation group.
/// </summary>
public static class HouseOccupiedIntegration
{
    public const string RootName = "House occupied / study pressure";
    const string ClipFolder = "Assets/_Game/Audio/Clips/HouseOccupied/";
    const string DoorAsset = "Assets/_Game/Art/Models/Overhaul/EP_OV_DomesticDoor.fbx";

    sealed class Context
    {
        public Scene scene;
        public HouseMemory house;
        public GameSession session;
        public FirstPersonController player;
        public BoxCollider glass, lower;
        public HouseOccupiedPresence existing;
        public AudioSource telephonePerformance;
        public AudioMixerGroup effects;
        public AudioClip weight, contact, retreat;
        public GameObject doorModel;
        public Material timber, edge, brass;
        public Bounds opening;
    }

    [MenuItem("Echo Protocol/House occupied/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.existing
            ? "House occupied: installed references valid. Apply is a no-op."
            : "House occupied: ready. Only a new child of HouseMemory and its occupiedPresence reference will change; save the scene yourself after review.", c.house);
    }

    [MenuItem("Echo Protocol/House occupied/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.existing)
        {
            Debug.Log("House occupied already installed; no objects, settings or references changed.", c.existing);
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Integrate occupied house only");
        GameObject root = null;
        try
        {
            root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create domestic sequence");
            root.transform.SetParent(c.house.transform, false);
            // Authored world positions derive from the actual, validated study opening.
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            var sequence = Undo.AddComponent<HouseOccupiedPresence>(root);
            Bounds b = c.opening;
            float face = b.min.x - .08f;
            float north = b.max.z - .02f, south = b.min.z + .02f;
            float centre = (north + south) * .5f;
            float width = (north - south - .025f) * .5f;
            float height = b.max.y - .035f;

            // Reuse the authored domestic door mesh, its panels, bevels and handles.
            // These are NEW copies, never the exit door or another chapter's prefab.
            Leaf(root.transform, c, "Fixed north leaf", face, north, width, height);
            var moving = Leaf(root.transform, c, "Loaded south leaf", face, south, width, height);
            // Opposed hinges: both handles meet at the centre. The moving SOUTH
            // leaf remains ahead of the forward camera at the contact endpoint.
            moving.localRotation = Quaternion.Euler(0, 180f, 0);
            Box(root.transform, "Study centre stop", new Vector3(face + .06f, height * .5f, centre),
                new Vector3(.09f, height, .038f), c.edge);
            foreach (float z in new[] { north + .025f, south - .025f })
                Box(root.transform, "Occupied study jamb", new Vector3(face + .025f, height * .5f, z),
                    new Vector3(.14f, height + .08f, .075f), c.timber);
            Box(root.transform, "Occupied study head", new Vector3(face + .025f, height + .045f, centre),
                new Vector3(.14f, .09f, north - south + .12f), c.timber);
            // A fixed door-chain plate gives the abrupt limit a physical cause.
            Box(root.transform, "Fixed catch plate", new Vector3(face - .04f, 1.49f, centre - .08f),
                new Vector3(.022f, .13f, .075f), c.brass);

            // Stable local illumination is independently owned. No flicker, global
            // light array, exposure or power override; matches existing service residue.
            Box(root.transform, "Study service indicator housing",
                new Vector3(face - .10f, 2.19f, north + .10f), new Vector3(.11f, .12f, .18f), c.brass);
            var lightObject = Child(root.transform, "Study service spill",
                new Vector3(face - .42f, 2.10f, centre + .15f));
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point; light.color = new Color(.72f, .78f, .69f);
            light.intensity = .65f; light.range = 3.4f; light.shadows = LightShadows.None;

            var far = Source(root.transform, "Weight inside study", new Vector3(b.max.x + 1.3f, .12f, centre - .55f), c.effects, .30f, 12f);
            var close = Source(moving, "Leaf contact", new Vector3(face - .08f, 1.15f, south + width * .78f), c.effects, .36f, 12f);
            var retreat = Source(root.transform, "Friction beyond study", new Vector3(b.max.x + .65f, .65f, centre + .15f), c.effects, .24f, 11f);

            // The north bedroom contains the existing key. The return volume spans
            // the whole hall, so forward walking (4m/s) meets the loaded leaf after
            // the .7s preparation, not after passing it. No camera/gaze condition.
            var advance = Volume(root.transform, "Bedroom approach / real movement",
                new Vector3(3.1f, 1.1f, -52.1f), new Vector3(4.4f, 3f, 5.8f));
            var key = Volume(root.transform, "Key room / listening",
                new Vector3(3.55f, 1.1f, -50.4f), new Vector3(3.2f, 3f, 2.6f));
            var returning = Volume(root.transform, "Key return / whole hall",
                new Vector3(0f, 1.1f, (north + 3.45f + south - 1.35f) * .5f),
                new Vector3(1.85f, 3f, north + 3.45f - south + 1.35f));

            var fields = new SerializedObject(sequence);
            Ref(fields, "house", c.house); Ref(fields, "session", c.session); Ref(fields, "player", c.player.transform);
            Ref(fields, "advanceVolume", advance); Ref(fields, "keyApproachVolume", key); Ref(fields, "returnVolume", returning);
            Ref(fields, "pressurePanel", moving); Ref(fields, "distantSource", far); Ref(fields, "contactSource", close); Ref(fields, "retreatSource", retreat);
            Ref(fields, "weightClip", c.weight); Ref(fields, "contactClip", c.contact); Ref(fields, "frictionClip", c.retreat);
            Ref(fields, "telephonePerformance", c.telephonePerformance);
            fields.FindProperty("pressureOffset").vector3Value = new Vector3(-.035f, 0, 0);
            fields.FindProperty("pressureEuler").vector3Value = new Vector3(0, -10f, 0);
            fields.ApplyModifiedPropertiesWithoutUndo();
            if (!sequence.IsConfigured) throw new InvalidOperationException("New domestic references failed final validation.");

            Undo.RecordObject(c.house, "Assign exclusive domestic presence owner");
            var houseFields = new SerializedObject(c.house);
            Ref(houseFields, "occupiedPresence", sequence);
            houseFields.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("House occupied integrated. The old apparition is suppressed by HouseMemory. Scene remains unsaved for review; use Save after the domestic playtest. No other chapter or project setting was changed.", sequence);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            // Children were created only beneath this task-owned root.
            if (root) Object.DestroyImmediate(root);
            throw;
        }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before domestic integration.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || string.IsNullOrEmpty(c.scene.path))
            throw new InvalidOperationException("Open the saved current production scene first. This operation never opens another scene.");
        c.house = One<HouseMemory>(c.scene);
        if (!c.house.isActiveAndEnabled || c.house.transform.lossyScale != Vector3.one || Quaternion.Angle(c.house.transform.rotation, Quaternion.identity) > .01f)
            throw new InvalidOperationException("House layout changed: review world-space domestic authoring before applying.");
        var h = new SerializedObject(c.house);
        c.session = Get<GameSession>(h, "session"); c.player = Get<FirstPersonController>(h, "player");
        var exit = Get<HouseSwingDoor>(h, "exitDoor"); var view = Get<Camera>(h, "view");
        if (!c.session || !c.player || !exit || !view || c.player.gameObject.scene != c.scene || c.session.gameObject.scene != c.scene
            || exit.gameObject.scene != c.scene || view.gameObject.scene != c.scene)
            throw new InvalidOperationException("House player, session, view or exit reference is missing or belongs to another scene.");
        c.glass = Named<BoxCollider>(c.house.transform, "Study fixed glazing");
        c.lower = Named<BoxCollider>(c.house.transform, "Study closed lower door");
        if (!c.glass.enabled || !c.lower.enabled || c.glass.isTrigger || c.lower.isTrigger)
            throw new InvalidOperationException("Study must retain its physical closed partition.");
        c.opening = c.glass.bounds; c.opening.Encapsulate(c.lower.bounds);
        if (Mathf.Abs(c.opening.center.x - 1f) > .08f || Mathf.Abs(c.opening.center.z + 58.3f) > .08f
            || c.opening.size.z < 2.65f || c.opening.size.z > 2.95f || c.opening.size.y < 2f)
            throw new InvalidOperationException("Expected the broadened Study opening at x1/z-58.3. Integrate on Incursion or its current successor; do not force an older layout.");
        var interactions = c.house.GetComponentsInChildren<HouseInteraction>(true).Where(i => i.House == c.house).ToArray();
        var key = interactions.SingleOrDefault(i => i.Kind == HouseInteractionKind.Key);
        if (!key || Vector3.Distance(key.transform.position, new Vector3(3.6f, .83f, -49.6f)) > .35f
            || !interactions.Any(i => i.Kind == HouseInteractionKind.Telephone))
            throw new InvalidOperationException("Key/telephone layout changed; review the three domestic volumes before applying.");

        var allSequences = c.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<HouseOccupiedPresence>(true)).ToArray();
        var roots = c.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).Where(t => t.name == RootName).ToArray();
        c.existing = Get<HouseOccupiedPresence>(h, "occupiedPresence");
        if (allSequences.Length != 0 || roots.Length != 0 || c.existing)
        {
            if (allSequences.Length != 1 || roots.Length != 1 || !c.existing || allSequences[0] != c.existing
                || roots[0] != c.existing.transform || c.existing.transform.parent != c.house.transform || !c.existing.IsConfigured || !c.existing.isActiveAndEnabled
                || c.existing.GetComponentsInChildren<AudioSource>(true).Any(a => !a.isActiveAndEnabled)
                || c.existing.GetComponentsInChildren<BoxCollider>(true).Any(v => !v.enabled || !v.gameObject.activeInHierarchy)
                || Get<HouseMemory>(new SerializedObject(c.existing), "house") != c.house)
                throw new InvalidOperationException("Partial or foreign house installation found. Resolve references or Undo it; nothing was changed.");
            return c;
        }
        c.weight = Clip("Occupied_Weight.wav"); c.contact = Clip("Occupied_Contact.wav"); c.retreat = Clip("Occupied_Retreat.wav");
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer");
        c.effects = mixer ? mixer.FindMatchingGroups("WorldSFX").SingleOrDefault(g => g.name == "WorldSFX") : null;
        if (!c.effects) throw new InvalidOperationException("Existing WorldSFX output is missing; the integration never changes the mixer.");
        c.doorModel = AssetDatabase.LoadAssetAtPath<GameObject>(DoorAsset);
        var mesh = c.doorModel ? c.doorModel.GetComponentsInChildren<MeshFilter>(true).SingleOrDefault(f => f.name == "DoorLeafPivot") : null;
        if (!mesh || !mesh.sharedMesh || !mesh.GetComponent<MeshRenderer>()) throw new InvalidOperationException("Original domestic door leaf asset is missing.");
        var materials = mesh.GetComponent<MeshRenderer>().sharedMaterials;
        foreach (var material in materials) ProjectMaterial(material.name);
        c.timber = ProjectMaterial("OV_PaintedTimber"); c.edge = ProjectMaterial("OV_WalnutEdge"); c.brass = ProjectMaterial("OV_DullBrass");
        // Read the existing line's source; never modify ResonanceSequence or its emitter.
        var resonance = One<ResonanceSequence>(c.scene);
        var line = Get<ResonanceEmitter>(new SerializedObject(resonance), "phoneLine");
        c.telephonePerformance = line ? Get<AudioSource>(new SerializedObject(line), "source") : null;
        if (!c.telephonePerformance) throw new InvalidOperationException("Existing receiver performance source is required to keep early cues clear of the call.");
        return c;
    }

    static Transform Leaf(Transform parent, Context c, string name, float face, float north, float width, float height)
    {
        var pivot = Child(parent, name, new Vector3(face, .025f, north)).transform;
        var fit = Child(pivot, "Original domestic timber and hardware", Vector3.zero).transform;
        fit.localPosition = Vector3.zero;
        var model = Object.Instantiate(c.doorModel, fit, false);
        model.name = "Reused Overhaul door mesh";
        model.transform.rotation = Quaternion.Euler(0, 90, 0) * model.transform.rotation;
        var leaf = model.GetComponentsInChildren<MeshFilter>(true).Single(f => f.name == "DoorLeafPivot");
        foreach (var r in model.GetComponentsInChildren<MeshRenderer>(true))
            if (r.gameObject != leaf.gameObject) Object.DestroyImmediate(r.gameObject);
        var renderer = leaf.GetComponent<MeshRenderer>();
        renderer.sharedMaterials = renderer.sharedMaterials.Select(m => ProjectMaterial(m.name)).ToArray();
        Bounds before = renderer.bounds;
        if (before.size.y < .5f || before.size.z < .3f || before.size.x > .5f)
            throw new InvalidOperationException("Domestic door import axes changed; integration rolled back.");
        fit.localScale = new Vector3(1f, height / before.size.y, width / before.size.z);
        Bounds fitted = renderer.bounds;
        model.transform.position += new Vector3(face - fitted.center.x, .025f - fitted.min.y, north - fitted.max.z);
        return pivot;
    }

    static GameObject Child(Transform parent, string name, Vector3 world)
    { var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.position = world; return g; }
    static GameObject Box(Transform parent, string name, Vector3 world, Vector3 size, Material material)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.position = world; g.transform.localScale = size;
        Object.DestroyImmediate(g.GetComponent<Collider>()); g.GetComponent<Renderer>().sharedMaterial = material; return g;
    }
    static BoxCollider Volume(Transform parent, string name, Vector3 centre, Vector3 size)
    { var v = Child(parent, name, centre).AddComponent<BoxCollider>(); v.isTrigger = true; v.size = size; return v; }
    static AudioSource Source(Transform parent, string name, Vector3 point, AudioMixerGroup group, float gain, float distance)
    {
        var a = Child(parent, name, point).AddComponent<AudioSource>();
        a.playOnAwake = false; a.loop = false; a.spatialBlend = 1f; a.dopplerLevel = 0f;
        a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = 1.5f; a.maxDistance = distance;
        a.volume = gain; a.outputAudioMixerGroup = group; a.reverbZoneMix = 0f;
        a.ignoreListenerPause = a.ignoreListenerVolume = a.bypassListenerEffects = false; return a;
    }
    static T One<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
        .SelectMany(g => g.GetComponentsInChildren<T>(true)).Single(c => c.gameObject.activeInHierarchy);
    static T Named<T>(Transform root, string name) where T : Component => root.GetComponentsInChildren<T>(true).Single(c => c.name == name);
    static T Get<T>(SerializedObject o, string field) where T : Object => o.FindProperty(field)?.objectReferenceValue as T;
    static void Ref(SerializedObject o, string field, Object value)
    {
        var p = o.FindProperty(field) ?? throw new MissingFieldException(field);
        p.objectReferenceValue = value;
    }
    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(ClipFolder + name)
        ?? throw new InvalidOperationException("Import required domestic clip: " + name);
    static Material ProjectMaterial(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Overhaul/M_" + name + ".mat")
        ?? throw new InvalidOperationException("Existing URP domestic material missing: " + name);
}

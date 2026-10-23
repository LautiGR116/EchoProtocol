using System;
using System.Linq;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Horror.SE1Pressure;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Current-scene integration only. Creates one undoable hierarchy and three
// explicit optional links. Never opens/saves scenes or modifies shared assets.
public static class SE1PressureIntegration
{
    const string RootName = "SE1Pressure / captive observation shutters";
    const string UndoName = "SE1 pressure approaching shutters";
    const string StopPath = "Assets/_Game/Audio/SE1Pressure/SE1_PlateStop.wav";
    static readonly float[] BayZ = { 3f, 7f, 11f, 15f };

    sealed class Context
    {
        public Scene scene;
        public VS1StrongEventOne owner;
        public VS1DomesticDoorReveal door;
        public VS1DomesticThresholdCheckpoint checkpoint;
        public CharacterController player;
        public Transform recall, leaf;
        public BoxCollider leafCollider;
        public BoxCollider[] glass;
        public AudioSource oldSource;
        public AudioClip far, middle, stop;
        public Material steel, dark;
        public SE1PressureSequence installed;
        public Bounds sweep;
    }

    [MenuItem("Echo Protocol/SE1 pressure/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "SE1 pressure is complete; Apply is a no-op."
            : "SE1 pressure preflight passed. Four captive bays, local reserve and capsule clearance can be added. Scene stays unsaved.", c.owner);
    }

    [MenuItem("Echo Protocol/SE1 pressure/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("SE1 pressure already installed; nothing changed.", c.owner); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        GameObject root = null;
        try
        {
            root = Node(c.owner.transform, RootName, Vector3.zero);
            var sequence = root.AddComponent<SE1PressureSequence>();
            var fields = new SerializedObject(sequence);
            Ref(fields, "owner", c.owner);
            Ref(fields, "checkpoint", c.checkpoint);
            var bays = fields.FindProperty("bays");
            bays.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                var mount = Node(root.transform, "Captive bay " + (i + 1), new Vector3(-.032f, 0, BayZ[i]));
                // Cassette/rails sit in the existing header and glass barrier.
                // No colliders or P1 reader/fixture edits. Fully open slats are
                // above the optical sightline; the two near bays close together.
                Box(mount.transform, "Header cassette", new Vector3(0, 3.045f, 0), new Vector3(.052f, .24f, 3.57f), c.dark);
                foreach (float side in new[] { -1f, 1f })
                    Box(mount.transform, "Captive guide", new Vector3(0, 1.70f, side * 1.785f), new Vector3(.052f, 2.52f, .038f), c.dark);
                Box(mount.transform, "Physical lower stop", new Vector3(0, .48f, 0), new Vector3(.052f, .05f, 3.57f), c.steel);
                var bay = bays.GetArrayElementAtIndex(i);
                var blades = bay.FindPropertyRelative("blades");
                blades.arraySize = 14;
                for (int j = 0; j < 14; j++)
                {
                    var blade = Box(mount.transform, "Linked blade " + j,
                        new Vector3(0, 2.88f + j * .017f, 0), new Vector3(.018f, .170f, 3.52f), c.steel);
                    blades.GetArrayElementAtIndex(j).objectReferenceValue = blade.transform;
                    // Folded return gives the otherwise flat shutter a physical
                    // lip; it travels with its slat and stays inside the glass.
                    var lip = Box(blade.transform, "Folded lip", Vector3.zero, Vector3.one, c.dark);
                    lip.transform.localPosition = new Vector3(.80f, -.39f, 0);
                    lip.transform.localScale = new Vector3(.28f, .065f, 1f);
                }
                var emitter = Node(mount.transform, "Lower frame contact", new Vector3(-.032f, .52f, BayZ[i]));
                var audio = emitter.AddComponent<AudioSource>();
                audio.clip = i == 0 ? c.far : i == 1 ? c.middle : c.stop;
                audio.outputAudioMixerGroup = c.oldSource.outputAudioMixerGroup;
                audio.playOnAwake = audio.loop = false;
                audio.spatialBlend = 1f; audio.dopplerLevel = 0f;
                audio.rolloffMode = AudioRolloffMode.Linear;
                audio.minDistance = 1.4f; audio.maxDistance = 18f;
                audio.volume = i == 0 ? .42f : i == 1 ? .46f : .34f;
                audio.priority = 120;
                audio.ignoreListenerPause = audio.ignoreListenerVolume = audio.bypassListenerEffects = false;
                bay.FindPropertyRelative("contact").objectReferenceValue = audio;
                bay.FindPropertyRelative("contactLeadSeconds").floatValue = i == 0 ? .18f : i == 1 ? .44f : 0f;
            }

            // Stable local reserve, never a strobe. The first two graze the
            // final moving surfaces AND floor; the last two articulate the
            // existing north turn and the original domestic door approach.
            var lamps = fields.FindProperty("reserveLights"); lamps.arraySize = 4;
            var intensities = fields.FindProperty("reserveIntensities"); intensities.arraySize = 4;
            Vector3[] positions = { new Vector3(1.35f, 2.65f, 11f), new Vector3(1.35f, 2.65f, 15f),
                new Vector3(1.65f, 2.65f, 18.25f), new Vector3(-2.75f, 2.65f, 19.2f) };
            Vector3[] targets = { new Vector3(0, 1.25f, 11.3f), new Vector3(0, 1.25f, 15.3f),
                new Vector3(-1f, .1f, 18.3f), new Vector3(-2.75f, .2f, 20f) };
            float[] levels = { .85f, .85f, .65f, .45f };
            for (int i = 0; i < 4; i++)
            {
                var fitting = Node(root.transform, "Fixed reserve fitting " + (i + 1), positions[i]);
                fitting.transform.rotation = Quaternion.LookRotation(targets[i] - positions[i]);
                Box(fitting.transform, "Shielded case", Vector3.zero, new Vector3(.16f, .10f, .12f), c.dark);
                // A slender fixed ceiling stem establishes a visible source.
                var stem = Node(root.transform, "Reserve stem " + (i + 1), new Vector3(positions[i].x, 2.92f, positions[i].z));
                Box(stem.transform, "Support", Vector3.zero, new Vector3(.025f, .54f, .025f), c.steel);
                var lamp = fitting.AddComponent<Light>();
                lamp.type = LightType.Spot; lamp.color = new Color(1f, .63f, .30f);
                lamp.range = i < 2 ? 3.8f : 4.6f;
                lamp.spotAngle = i < 2 ? 108f : 76f;
                lamp.innerSpotAngle = i < 2 ? 70f : 48f;
                lamp.shadows = LightShadows.None; lamp.intensity = 0f;
                lamps.GetArrayElementAtIndex(i).objectReferenceValue = lamp;
                intensities.GetArrayElementAtIndex(i).floatValue = levels[i];
            }
            var clearanceObject = Node(root.transform, "Domestic slam sweep clearance", c.sweep.center);
            var clearance = clearanceObject.AddComponent<BoxCollider>();
            clearanceObject.layer = 2; // metadata trigger, never a route blocker
            clearance.isTrigger = true; clearance.size = c.sweep.size;
            fields.ApplyModifiedPropertiesWithoutUndo();
            if (!sequence.IsConfigured) throw new InvalidOperationException("SE1 pressure final references are incomplete.");

            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Undo.RecordObjects(new Object[] { c.owner, c.door }, UndoName);
            var owner = new SerializedObject(c.owner);
            Ref(owner, "pressureSequence", sequence); owner.ApplyModifiedProperties();
            var door = new SerializedObject(c.door);
            Ref(door, "safetyPlayer", c.player); Ref(door, "slamSweepClearance", clearance);
            door.ApplyModifiedProperties();
            Physics.SyncTransforms();
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("SE1 pressure installed, scene UNSAVED. Existing Echo gate -> far/mid/near captive closures -> warm retreat. The original domestic slam waits for a clear capsule sweep. Undo removes the hierarchy and restores all three links. Central Editor compilation/short Play and human listening remain required.", sequence);
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
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play before integrating SE1.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Requires the already-open Incursion scene; no scene will be opened or saved.");
        c.owner = One<VS1StrongEventOne>(c.scene);
        c.checkpoint = One<VS1DomesticThresholdCheckpoint>(c.scene);
        var owner = new SerializedObject(c.owner);
        var checkpoint = new SerializedObject(c.checkpoint);
        c.door = Get<VS1DomesticDoorReveal>(owner, "domesticDoorReveal");
        var player = Get<Transform>(owner, "player");
        c.player = player ? player.GetComponent<CharacterController>() : null;
        var relay = Get<ParallelObservationRelay>(owner, "relay");
        var loop = Get<LoopController>(owner, "loopController");
        c.recall = Get<Transform>(checkpoint, "recallPoint");
        c.oldSource = Get<AudioSource>(owner, "laneRelaySource");
        if (!c.door || !c.player || !c.player.enabled || !relay || !loop || !c.oldSource || !c.oldSource.clip
            || !c.oldSource.outputAudioMixerGroup || c.oldSource.outputAudioMixerGroup.name != "WorldSFX"
            || Get<VS1DomesticDoorReveal>(checkpoint, "doorReveal") != c.door
            || Get<Transform>(checkpoint, "player") != player || Get<LoopController>(checkpoint, "loopController") != loop
            || !c.owner.isActiveAndEnabled || !c.door.isActiveAndEnabled || !relay.isActiveAndEnabled
            || !c.checkpoint.GetComponent<BoxCollider>().isTrigger)
            throw new InvalidOperationException("The active original SE1/P1, WorldSFX, Player and shared domestic checkpoint references are required.");
        At(c.owner.transform, Vector3.zero);
        Identity(c.owner.transform);
        At(Get<Transform>(owner, "knockTrigger"), new Vector3(3.35f, 0, 10));
        At(Get<Transform>(owner, "laneCheckTarget"), new Vector3(-2.75f, 1.35f, 14.5f));
        At(c.recall, new Vector3(-2.75f, .03f, 22.05f));
        for (int i = 0; i < 3; i++)
        {
            At(Element<Light>(owner, "laneSequenceLights", i)?.transform, new Vector3(-2.75f, 3.01f, 4 + i * 5));
            At(Element<Light>(owner, "serviceSequenceLights", i)?.transform, new Vector3(2.5f, 3.01f, 4 + i * 5));
        }
        if (owner.FindProperty("facilityBlackoutLights").arraySize != 12 || owner.FindProperty("facilityBlackoutPanels").arraySize != 12)
            throw new InvalidOperationException("SE1's original twelve-light consequence must be intact.");
        if (All<AudioListener>(c.scene).Count(l => l.enabled && l.gameObject.activeInHierarchy) != 1
            || !All<SessionAudio>(c.scene).Any(a => a.isActiveAndEnabled && a.GetComponent<AudioListener>().enabled))
            throw new InvalidOperationException("Requires the original single listener with session audio options.");
        c.glass = new BoxCollider[4];
        for (int i = 0; i < 4; i++)
        {
            c.glass[i] = Named<BoxCollider>(c.scene, "Observation Glass " + (i + 2));
            var glass = c.glass[i];
            At(glass.transform, new Vector3(0, 1.65f, BayZ[i])); Identity(glass.transform);
            if (!glass.enabled || glass.isTrigger || Vector3.Distance(glass.center, Vector3.zero) > .001f
                || Vector3.Distance(glass.size, new Vector3(.12f, 2.65f, 3.65f)) > .001f
                || Physics.GetIgnoreLayerCollision(c.player.gameObject.layer, glass.gameObject.layer))
                throw new InvalidOperationException("Original solid glass must protect the complete captive shutter path.");
        }
        var door = new SerializedObject(c.door);
        c.leaf = Get<Transform>(door, "movingLeaf");
        c.leafCollider = Get<BoxCollider>(door, "leafCollider");
        if (!c.leaf || !c.leafCollider || c.leafCollider.transform != c.leaf || !c.leafCollider.enabled || c.leafCollider.isTrigger
            || c.leaf.localScale != Vector3.one || Quaternion.Angle(c.leaf.localRotation, Quaternion.identity) > .01f
            || Vector3.Distance(c.leaf.lossyScale, new Vector3(1.45f, 1.15f, 1f)) > .01f
            || Mathf.Abs(door.FindProperty("openAngle").floatValue - 76) > .01f)
            throw new InvalidOperationException("Expected the original fitted domestic leaf and 76-degree opening.");
        At(c.leaf, new Vector3(-3.49f, 0, 20.02f));
        c.sweep = SweepBounds(c.leaf, c.leafCollider);
        Bounds recalledCapsule = new Bounds(c.recall.TransformPoint(c.player.center),
            new Vector3(c.player.radius * 2, c.player.height, c.player.radius * 2) + Vector3.one * .16f);
        if (c.sweep.Intersects(recalledCapsule))
            throw new InvalidOperationException("Domestic recall capsule overlaps the slam sweep; review before installation.");

        c.far = Asset<AudioClip>("Assets/_Game/Audio/Clips/Resonance/Relay_Strain.wav");
        c.middle = Asset<AudioClip>("Assets/_Game/Audio/Clips/Resonance/Latch_Release.wav");
        c.stop = Asset<AudioClip>(StopPath);
        c.steel = Asset<Material>("Assets/_Game/Art/Reception/M_TI_FoldedSteel.mat");
        c.dark = Asset<Material>("Assets/_Game/Art/Readers/M_Bezel.mat");
        if (new[] { c.far, c.middle, c.stop }.Any(clip => clip.channels != 1 || clip.length > 1f))
            throw new InvalidOperationException("Expected the small mono CC0 contact masters.");

        c.installed = Get<SE1PressureSequence>(owner, "pressureSequence");
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        var sequences = All<SE1PressureSequence>(c.scene).ToArray();
        if (c.installed || roots.Length != 0 || sequences.Length != 0)
        {
            if (!c.installed || roots.Length != 1 || sequences.Length != 1 || sequences[0] != c.installed
                || roots[0] != c.installed.transform || roots[0].parent != c.owner.transform
                || !c.installed.IsConfigured || !c.installed.isActiveAndEnabled)
                throw new InvalidOperationException("Partial/foreign SE1 pressure installation. Undo or repair it before applying; no changes made.");
            var installed = new SerializedObject(c.installed);
            if (Get<VS1StrongEventOne>(installed, "owner") != c.owner
                || Get<VS1DomesticThresholdCheckpoint>(installed, "checkpoint") != c.checkpoint
                || Get<CharacterController>(door, "safetyPlayer") != c.player)
                throw new InvalidOperationException("SE1 pressure ownership or capsule reference differs.");
            var sweep = Get<BoxCollider>(door, "slamSweepClearance");
            var colliders = c.installed.GetComponentsInChildren<Collider>(true);
            if (!sweep || colliders.Length != 1 || colliders[0] != sweep || !sweep.isTrigger || !sweep.enabled
                || Vector3.Distance(sweep.bounds.center, c.sweep.center) > .01f || Vector3.Distance(sweep.bounds.size, c.sweep.size) > .01f)
                throw new InvalidOperationException("SE1 contains an unexpected blocker or an altered slam-clearance envelope.");
            CheckCaptiveBlades(installed, c);
        }
        else if (Get<CharacterController>(door, "safetyPlayer") || Get<BoxCollider>(door, "slamSweepClearance"))
            throw new InvalidOperationException("An existing door safety owner needs review; it will not be overwritten.");
        return c;
    }

    static void CheckCaptiveBlades(SerializedObject fields, Context c)
    {
        for (int i = 0; i < 4; i++)
        {
            var bay = fields.FindProperty("bays").GetArrayElementAtIndex(i);
            var blades = bay.FindPropertyRelative("blades");
            for (int j = 0; j < blades.arraySize; j++)
            {
                var blade = (Transform)blades.GetArrayElementAtIndex(j).objectReferenceValue;
                At(blade, new Vector3(-.032f, 2.88f + j * .017f, BayZ[i]));
                if (Vector3.Distance(blade.lossyScale, new Vector3(.018f, .170f, 3.52f)) > .001f
                    || Quaternion.Angle(blade.rotation, Quaternion.identity) > .01f)
                    throw new InvalidOperationException("A captive shutter blade was resized or rotated outside its protected track.");
                // The entire downward sweep (including the folded lip) is in
                // the old glass x/z slab. Upper storage is in its solid header.
                Bounds glass = c.glass[i].bounds;
                var atStop = blade.position + Vector3.down * (j * .192f);
                if (atStop.y - .085f < glass.min.y || atStop.x - .009f < glass.min.x
                    || atStop.x + .017f > glass.max.x || atStop.z - 1.76f < glass.min.z || atStop.z + 1.76f > glass.max.z)
                    throw new InvalidOperationException("A captive shutter stroke escapes the original glass barrier.");
            }
        }
    }

    static Bounds SweepBounds(Transform pivot, BoxCollider leaf)
    {
        float radius = 0, low = float.PositiveInfinity, high = float.NegativeInfinity;
        for (int x = -1; x <= 1; x += 2)
        for (int y = -1; y <= 1; y += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            var corner = leaf.center + Vector3.Scale(leaf.size * .5f, new Vector3(x, y, z));
            radius = Mathf.Max(radius, new Vector2(corner.x, corner.z).magnitude);
            low = Mathf.Min(low, corner.y); high = Mathf.Max(high, corner.y);
        }
        // Sweep in the hinge's local coordinates BEFORE the original parent's
        // nonuniform 1.45/1.15/1 scale. A world-radius sphere would needlessly
        // cover the safe recall origin. This encloses every intermediate angle.
        Bounds bounds = new Bounds(pivot.TransformPoint(new Vector3(-radius, low, -radius)), Vector3.zero);
        for (int x = -1; x <= 1; x += 2)
        for (int y = 0; y <= 1; y++)
        for (int z = -1; z <= 1; z += 2)
            bounds.Encapsulate(pivot.TransformPoint(new Vector3(x * radius, y == 0 ? low : high, z * radius)));
        bounds.Expand(.24f);
        return bounds;
    }

    static GameObject Node(Transform parent, string name, Vector3 worldPosition)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.transform.position = worldPosition; return go;
    }
    static Renderer Box(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false); go.transform.localPosition = localPosition; go.transform.localScale = size;
        var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
        return renderer;
    }
    static void At(Transform t, Vector3 expected)
    { if (!t || Vector3.Distance(t.position, expected) > .025f) throw new InvalidOperationException("SE1 layout changed at " + expected + "; review before applying."); }
    static void Identity(Transform t)
    { if (t.lossyScale != Vector3.one || Quaternion.Angle(t.rotation, Quaternion.identity) > .01f) throw new InvalidOperationException("SE1 composition requires the original world orientation and scale."); }
    static T Asset<T>(string path) where T : Object
    { var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (!asset) throw new InvalidOperationException("Required existing asset is not imported: " + path); return asset; }
    static T Get<T>(SerializedObject o, string name) where T : Object => o.FindProperty(name).objectReferenceValue as T;
    static T Element<T>(SerializedObject o, string name, int index) where T : Object => o.FindProperty(name).GetArrayElementAtIndex(index).objectReferenceValue as T;
    static void Ref(SerializedObject o, string name, Object value) => o.FindProperty(name).objectReferenceValue = value;
    static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static T One<T>(Scene scene) where T : Behaviour
    {
        var candidates = All<T>(scene).Where(c => c.isActiveAndEnabled).ToArray();
        if (candidates.Length != 1) throw new InvalidOperationException("Expected exactly one active " + typeof(T).Name);
        return candidates[0];
    }
    static T Named<T>(Scene scene, string name) where T : Component
    {
        var candidates = All<T>(scene).Where(c => c.name == name && c.gameObject.activeInHierarchy).ToArray();
        if (candidates.Length != 1) throw new InvalidOperationException("Expected exactly one active " + name);
        return candidates[0];
    }
}

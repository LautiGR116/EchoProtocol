using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.FinalReception;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>6B only. Uses the currently loaded scene; never opens or saves it.</summary>
public static class FinalReceptionIntegration
{
    public const string RootName = "FinalReception / receiving recess";
    public const string InsertName = "FinalReception / opaque leaf insert";
    const string UndoName = "Compose final 6B reception";
    const float HingeVolume = .065f, InteriorIntensity = .8f, ExteriorIntensity = .85f;

    sealed class Context
    {
        public Scene scene;
        public FinalDoorMemory review;
        public HouseSwingDoor door;
        public Transform facing, hinge, origin;
        public BoxCollider outside, witness, leaf;
        public CharacterController player;
        public AudioSource movement, latch;
        public Light interior, exterior;
        public Vector3 ground;
        public Quaternion rotation;
        public Material joinery, walnut, brass, dark, lens;
        public FinalReceptionComposition installed;
    }

    [MenuItem("Echo Protocol/Final reception/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "FinalReception installed and consistent; Apply is a no-op."
            : "FinalReception ready: opaque moving insert, deep receiving recess, open rear portal and local light/motor balance. No scene will be opened or saved.", c.review);
    }

    [MenuItem("Echo Protocol/Final reception/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("FinalReception already installed; nothing changed.", c.installed); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        GameObject root = null, insert = null;
        try
        {
            root = new GameObject(RootName);
            root.transform.SetParent(c.review.transform, false);
            root.transform.SetPositionAndRotation(c.ground, c.rotation);
            var composition = root.AddComponent<FinalReceptionComposition>();

            // The glazing no longer previews the person before the Player opens.
            // This opaque collider is never whitelisted as transparent. Its XZ
            // footprint fits inside the lower leaf used by door safety checks.
            insert = new GameObject(InsertName);
            insert.transform.SetParent(c.hinge, false);
            insert.transform.localPosition = Vector3.zero;
            Box(insert.transform, "Solid upper timber panel", new Vector3(.48f, 1.525f, 0),
                new Vector3(.79f, .88f, .060f), c.joinery);
            foreach (float z in new[] { -.0308f, .0308f })
                Detail(insert.transform, "Flush panel inset", new Vector3(.48f, 1.525f, z),
                    new Vector3(.64f, .72f, .001f), c.walnut);

            // A shallow receiving bay, open behind the Player. The side passage
            // starts at z=2.1 in this basis, so its existing approach is retained.
            // 2.46m clear width > witness 1.65 + capsule .64 + 2*skin .03.
            foreach (float side in new[] { -1f, 1f })
            {
                Box(root.transform, "Receiving recess / timber side", new Vector3(side * 1.30f, 1.12f, 1.15f),
                    new Vector3(.14f, 2.24f, 1.90f), c.walnut);
                // Narrow jambs leave the existing origin plaque at x=1.22,
                // z=.13 visible and reachable from inside the recess.
                Box(root.transform, "Receiving recess / front return", new Vector3(side * .57f, 1.12f, .14f),
                    new Vector3(.10f, 2.24f, .10f), c.joinery);
                Detail(root.transform, "Recess dado", new Vector3(side * 1.224f, .92f, 1.15f),
                    new Vector3(.012f, .045f, 1.90f), c.brass);
                Detail(root.transform, "Recess vertical seam", new Vector3(side * 1.224f, 1.12f, 1.72f),
                    new Vector3(.010f, 2.22f, .018f), c.dark);
            }
            Box(root.transform, "Receiving recess / low soffit", new Vector3(0, 2.32f, 1.13f),
                new Vector3(2.74f, .16f, 2.04f), c.walnut);
            Box(root.transform, "Receiving recess / forward lintel", new Vector3(0, 2.20f, .14f),
                new Vector3(1.18f, .08f, .10f), c.joinery);

            // Low, flush perspective lines invite a step toward the real leaf.
            // They neither qualify a pose nor restrict where the Player may stand.
            foreach (float side in new[] { -1f, 1f })
                Detail(root.transform, "Worn threshold inlay", new Vector3(side * .48f, .009f, 1.08f),
                    new Vector3(.018f, .003f, .88f), c.brass);
            Detail(root.transform, "Near threshold seam", new Vector3(0, .009f, .67f),
                new Vector3(.98f, .003f, .025f), c.dark);

            // A second open architectural plane just beyond the actual stop.
            // No backdrop across the route: portería and departure remain visible
            // through the centre, with the silhouette in front of this plane.
            foreach (float side in new[] { -1f, 1f })
            {
                Box(root.transform, "Exterior depth portal / jamb", new Vector3(side * 1.30f, 1.12f, -2.05f),
                    new Vector3(.14f, 2.24f, .30f), c.dark);
                Box(root.transform, "Exterior depth portal / return", new Vector3(side * 1.30f, 1.12f, -1.58f),
                    new Vector3(.14f, 2.24f, .64f), c.walnut);
            }
            Box(root.transform, "Exterior depth portal / header", new Vector3(0, 2.32f, -2.05f),
                new Vector3(2.74f, .16f, .30f), c.dark);

            // Fixed practicals, no reveal trigger, tracking, flash or global
            // exposure. The brighter side is on the face actually facing 6B.
            var face = Practical(root.transform, "Receiving side wash", new Vector3(.96f, 2.10f, -.68f),
                new Vector3(0, 1.40f, -1.35f), new Color(.83f, .85f, .73f), 1.6f, 2.65f, 78f, c);
            var handle = Practical(root.transform, "Interior latch graze", new Vector3(.93f, 1.95f, .39f),
                new Vector3(.35f, 1.05f, .03f), new Color(1f, .79f, .53f), .5f, 1.8f, 70f, c);

            var fields = new SerializedObject(composition);
            Set(fields, "review", c.review); Set(fields, "door", c.door); Set(fields, "leafInsert", insert);
            Set(fields, "faceLight", face); Set(fields, "handleLight", handle);
            Set(fields, "hingeSource", c.movement); Float(fields, "originalHingeVolume", c.movement.volume);
            Set(fields, "originalInteriorLight", c.interior); Float(fields, "originalInteriorIntensity", c.interior.intensity);
            Set(fields, "originalExteriorLight", c.exterior); Float(fields, "originalExteriorIntensity", c.exterior.intensity);
            fields.ApplyModifiedPropertiesWithoutUndo();

            // Register completed hierarchies so Redo preserves all descendants,
            // materials, colliders, bindings and rollback data.
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Undo.RegisterCreatedObjectUndo(insert, UndoName);
            Undo.RecordObjects(new Object[] { c.movement, c.interior, c.exterior }, UndoName);
            c.movement.volume = HingeVolume;
            c.interior.intensity = InteriorIntensity;
            c.exterior.intensity = ExteriorIntensity;
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("FinalReception applied. The closed leaf hides the arrival; opening reveals the original silent body between two close architectural planes. Original origin, zones, take, handles and progress are unchanged. Scene remains unsaved.", composition);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (insert) Object.DestroyImmediate(insert);
            if (root) Object.DestroyImmediate(root);
            throw;
        }
    }

    [MenuItem("Echo Protocol/Final reception/3. Remove from current scene")]
    public static void RemoveFromCurrentScene()
    {
        // Also usable after saving/reopening the scene. Refuse removal if a
        // later author changed this installation's gain/light values.
        var c = Validate();
        if (!c.installed) { Debug.Log("FinalReception is not installed; nothing changed."); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Remove final 6B reception");
        try
        {
            var installed = c.installed;
            Undo.RecordObjects(new Object[] { c.movement, c.interior, c.exterior }, UndoName);
            c.movement.volume = installed.OriginalHingeVolume;
            c.interior.intensity = installed.OriginalInteriorIntensity;
            c.exterior.intensity = installed.OriginalExteriorIntensity;
            Undo.DestroyObjectImmediate(installed.LeafInsert);
            Undo.DestroyObjectImmediate(installed.gameObject);
            EditorSceneManager.MarkSceneDirty(c.scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("FinalReception removed; original local light and hinge gains restored. Scene remains unsaved.", c.review);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play before final reception integration.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded && !string.IsNullOrEmpty(c.scene.path), "Open the saved Incursion scene first. This tool never opens or saves a scene.");
        c.review = One<FinalDoorMemory>(c.scene);
        Require(c.review.isActiveAndEnabled && c.review.RequiresDirectEncounter && c.review.UsesArrivalPresentation,
            "Requires the active, direct Incursion arrival reception.");
        var review = new SerializedObject(c.review);
        c.facing = Get<Transform>(review, "outsideFacingTransform");
        c.origin = Get<Transform>(review, "origin");
        c.outside = Get<BoxCollider>(review, "outsideZone");
        c.witness = Get<BoxCollider>(review, "insideWitnessZone");
        c.door = Get<HouseSwingDoor>(review, "domesticDoor");
        Require(c.facing && c.origin && c.outside && c.witness && c.door, "Missing original 6B spatial references.");
        c.ground = c.facing.position - Vector3.up;
        c.rotation = c.facing.rotation;
        Require(Vector3.Angle(c.facing.up, Vector3.up) < .1f && Unit(c.review.transform) && Unit(c.facing), "Expected an unscaled, upright 6B frame.");
        var door = new SerializedObject(c.door);
        var body = Get<Rigidbody>(door, "hingeBody");
        c.leaf = Get<BoxCollider>(door, "solidLeaf");
        c.player = Get<CharacterController>(door, "playerBody");
        c.movement = Get<AudioSource>(door, "movementAudio");
        c.latch = Get<AudioSource>(door, "latchAudio");
        Require(body && body.isKinematic && c.leaf && c.player && c.door.isActiveAndEnabled, "Missing real kinematic door/leaf/Player.");
        c.hinge = body.transform;
        Require(c.hinge == c.door.transform && Unit(c.hinge) && Quaternion.Angle(c.hinge.rotation, c.rotation) < .1f,
            "Door must be in its original CLOSED Editor pose.");
        At(c, c.hinge, new Vector3(-.48f, 0, 0));
        At(c, c.origin, new Vector3(0, .03f, 1.3f));
        At(c, c.outside.transform, new Vector3(0, 1, -1.35f));
        At(c, c.witness.transform, new Vector3(0, 1, 1.25f));
        Require(Near(c.outside.size, new Vector3(1.1f, 2.2f, .7f)) && Near(c.witness.size, new Vector3(1.65f, 2.2f, 1.25f)) &&
            c.outside.center == Vector3.zero && c.witness.center == Vector3.zero && c.outside.isTrigger && c.witness.isTrigger &&
            Unit(c.outside.transform) && Unit(c.witness.transform), "6B evidence zones differ from the reviewed baseline.");
        Require(Quaternion.Angle(c.outside.transform.rotation, c.rotation) < .1f && Quaternion.Angle(c.witness.transform.rotation, c.rotation) < .1f,
            "Evidence zones must retain the door frame's orientation.");
        Require(c.player.radius <= .32f && c.player.skinWidth <= .03f && c.player.height <= 1.8f,
            "Player capsule changed; recheck the receiving recess clearance first.");
        Require(c.leaf.enabled && !c.leaf.isTrigger && c.leaf.transform.IsChildOf(c.hinge) &&
            Near(c.leaf.transform.localPosition, new Vector3(.48f, .55f, 0)) &&
            Near(Vector3.Scale(c.leaf.size, c.leaf.transform.localScale), new Vector3(.96f, 1.10f, .065f)),
            "Door lower safety footprint changed; the opaque insert must remain within it.");
        Require(Mathf.Abs(door.FindProperty("openAngle").floatValue - 112f) < .01f,
            "Expected the 112 degree exterior opening; recheck the new portal against any changed sweep.");
        Require(review.FindProperty("opaqueDirectWitness").boolValue && Get<Material>(review, "directWitnessMaterial"), "Requires the original opaque body witness material.");
        Require(review.FindProperty("sightMask").intValue == -1, "Witness must continue checking opaque geometry on the default layer.");
        foreach (var source in new[] { c.movement, c.latch })
            Require(source && source.outputAudioMixerGroup && source.outputAudioMixerGroup.name == "WorldSFX" &&
                source.spatialBlend == 1f && !source.ignoreListenerPause && !source.ignoreListenerVolume && source.dopplerLevel == 0f,
                "Door sources must retain their 3D WorldSFX/listener/pause routing.");
        Require(Get<AudioClip>(door, "movementClip") && Get<AudioClip>(door, "unlockClip") && Get<AudioClip>(door, "openEndpointClip"), "Missing original licensed door sounds.");
        c.interior = Named<Light>(c.review.transform, "Inside final practical");
        c.exterior = Named<Light>(c.review.transform, "Arrival exterior practical");
        Require(c.interior && c.exterior && c.interior.type == LightType.Spot && c.exterior.type == LightType.Spot,
            "Missing original 6B local practicals.");
        var feedback = One<FinalArrivalPresentation>(c.scene);
        var feedbackFields = new SerializedObject(feedback);
        Require(Get<FinalDoorMemory>(feedbackFields, "review") == c.review, "Reception completion feedback must remain linked to this review.");
        foreach (string name in new[] { "receivingLamp", "porterLamp", "keyLamp", "exitLamp" })
            Require(Get<Light>(feedbackFields, name) != c.interior && Get<Light>(feedbackFields, name) != c.exterior,
                "Cannot take intensity ownership from arrival confirmation.");

        var installed = All<FinalReceptionComposition>(c.scene);
        var named = All<Transform>(c.scene).Where(t => t.name == RootName || t.name == InsertName).ToArray();
        Require(installed.Length <= 1, "Multiple FinalReception owners found; no changes made.");
        if (installed.Length == 1)
        {
            c.installed = installed[0];
            var owner = c.installed;
            Require(owner.IsConfigured && owner.isActiveAndEnabled && owner.Review == c.review && owner.Door == c.door &&
                owner.transform.parent == c.review.transform && owner.name == RootName && named.Length == 2 &&
                owner.LeafInsert.name == InsertName && owner.LeafInsert.transform.parent == c.hinge &&
                owner.HingeSource == c.movement && owner.OriginalInteriorLight == c.interior && owner.OriginalExteriorLight == c.exterior,
                "Partial/foreign FinalReception installation. Undo or resolve its bindings before applying.");
            At(c, owner.transform, Vector3.zero);
            Require(Quaternion.Angle(owner.transform.rotation, c.rotation) < .1f && Unit(owner.transform), "Receiving recess moved or scaled.");
            var panel = Named<BoxCollider>(owner.LeafInsert.transform, "Solid upper timber panel");
            Require(panel.enabled && !panel.isTrigger && panel.gameObject.layer == 0 &&
                Near(panel.transform.localPosition, new Vector3(.48f, 1.525f, 0)) &&
                Near(Vector3.Scale(panel.size, panel.transform.localScale), new Vector3(.79f, .88f, .060f)),
                "Opaque moving insert changed or no longer blocks witness rays.");
            Require(owner.GetComponentsInChildren<BoxCollider>(true).Length == 13 &&
                owner.GetComponentsInChildren<Light>(true).Length == 2 && owner.LeafInsert.GetComponentsInChildren<BoxCollider>(true).Length == 1,
                "Incomplete FinalReception geometry; do not duplicate it.");
            Require(Mathf.Abs(c.movement.volume - HingeVolume) < .0001f &&
                Mathf.Abs(c.interior.intensity - InteriorIntensity) < .0001f && Mathf.Abs(c.exterior.intensity - ExteriorIntensity) < .0001f,
                "Installed reception values changed after integration. Preserve that work before rollback or reapplying.");
            return c;
        }
        Require(named.Length == 0, "Orphaned FinalReception geometry found; no changes made.");
        c.joinery = Material("Assets/_Game/Art/Threshold/M_Joinery.mat");
        c.walnut = Material("Assets/_Game/Art/Threshold/M_FurnitureWalnut.mat");
        c.brass = Material("Assets/_Game/Art/Threshold/M_Brass.mat");
        c.dark = Material("Assets/_Game/Art/Contraprueba/M_BlackenedSteel.mat");
        c.lens = Material("Assets/_Game/Art/Threshold/M_WitnessPilot.mat");
        return c;
    }

    static Light Practical(Transform root, string name, Vector3 p, Vector3 target, Color color,
        float intensity, float range, float angle, Context c)
    {
        Box(root, name + " / steel housing", p + Vector3.up * .05f, new Vector3(.25f, .10f, .16f), c.dark);
        Detail(root, name + " / visible diffuser", p - Vector3.up * .006f, new Vector3(.21f, .012f, .12f), c.lens);
        var node = new GameObject(name);
        node.transform.SetParent(root, false);
        node.transform.localPosition = p - Vector3.up * .025f;
        node.transform.localRotation = Quaternion.LookRotation(target - p);
        var light = node.AddComponent<Light>();
        light.type = LightType.Spot; light.color = color; light.intensity = intensity;
        light.range = range; light.spotAngle = angle; light.innerSpotAngle = angle * .65f;
        light.shadows = LightShadows.None; light.bounceIntensity = 0f;
        return light;
    }

    static Renderer Box(Transform parent, string name, Vector3 p, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent, false);
        go.transform.localPosition = p; go.transform.localScale = size;
        var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
        return renderer;
    }
    static void Detail(Transform parent, string name, Vector3 p, Vector3 size, Material material)
    { var r = Box(parent, name, p, size, material); Object.DestroyImmediate(r.GetComponent<Collider>()); }
    static Material Material(string path)
    { var m = AssetDatabase.LoadAssetAtPath<Material>(path); Require(m, "Missing existing material: " + path); return m; }
    static void At(Context c, Transform t, Vector3 p)
    { Require(t && Near(Quaternion.Inverse(c.rotation) * (t.position - c.ground), p), "6B layout changed near " + p + "; review composition before integration."); }
    static bool Near(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < .015f;
    static bool Unit(Transform t) => Vector3.Distance(t.lossyScale, Vector3.one) < .001f;
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static T One<T>(Scene scene) where T : Component => All<T>(scene).Single();
    static T Named<T>(Transform root, string name) where T : Component => root.GetComponentsInChildren<T>(true).Single(t => t.name == name);
    static T Get<T>(SerializedObject fields, string name) where T : Object => fields.FindProperty(name)?.objectReferenceValue as T;
    static void Set(SerializedObject fields, string name, Object value) => fields.FindProperty(name).objectReferenceValue = value;
    static void Float(SerializedObject fields, string name, float value) => fields.FindProperty(name).floatValue = value;
}

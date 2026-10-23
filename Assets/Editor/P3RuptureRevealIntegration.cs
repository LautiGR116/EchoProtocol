using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderReveal;
using EchoProtocol.Campaign.P3Intrusion;
using EchoProtocol.Campaign.P3RupturePresentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Upgrade the saved R5 presentation, independently of the R6 face/performance.
// Never opens/saves a scene or writes B's targets, rig, root or reserved jamb.
public static class P3RuptureRevealIntegration
{
    const string RootName = "P3Rupture / short reveal and physical tail";
    const string UndoName = "P3 rupture reveal round six";
    const string DryPath = "Assets/_Game/Audio/IntruderDread/IntruderDread_DryContact.wav";
    const string R5ContactPath = "Assets/_Game/Audio/Clips/Impact/Threat_FrameContact.wav";
    sealed class Context
    {
        public Scene scene;
        public IntruderStaging staging;
        public MachineCoverPresence presence;
        public P3BodyIntrusion body;
        public IntruderRevealSetup previous;
        public P3RuptureRevealSetup installed;
        public Light key, fill;
        public Transform backing, jamb;
        public AudioSource contact;
        public AudioClip dry;
    }

    [MenuItem("Echo Protocol/P3 rupture reveal round 6/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "P3 R6 reveal intact; Upgrade is a no-op."
            : "P3 R5 ready: two existing lights, one visual backing, one existing contact source. Reserved jamb and Echo lane untouched. Scene unchanged.", c.staging);
    }

    [MenuItem("Echo Protocol/P3 rupture reveal round 6/2. Upgrade current scene")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("P3 R6 reveal already installed; nothing changed.", c.staging); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(UndoName);
        GameObject root = null;
        try
        {
            root = new GameObject(RootName); root.transform.SetParent(c.staging.transform.parent, false);
            var r = root.AddComponent<P3RuptureRevealSetup>();
            r.staging = c.staging; r.presence = c.presence; r.roundFive = c.previous;
            // The moving face enters a FIXED near pool. No flash, head-follow,
            // global dimming or contact-driven disappearance conceals the face.
            var key = LightEdit(c.key); var values = key.applied;
            values.values.position = c.key.transform.parent.InverseTransformPoint(new Vector3(-6.05f, 2.42f, 36.78f));
            values.values.rotation = Quaternion.Inverse(c.key.transform.parent.rotation)
                * Quaternion.LookRotation(new Vector3(-5.94f, 1.42f, 36.49f) - new Vector3(-6.05f, 2.42f, 36.78f));
            values.values.range = 2.55f; values.values.outer = 104; values.values.inner = 56;
            key.applied = values;
            var fill = LightEdit(c.fill); values = fill.applied;
            values.values.intensity = .31f; // Retain detail on the unlit cheek/orbit.
            fill.applied = values;
            r.lights = new[] { key, fill };

            // Reuse the floral backing as a SHORT side return behind B's fixed
            // jamb. x[-6.16,-5.60], z[36.948,36.972]; the recorded central lane
            // stays open. Neither the grip at z36.7975 nor the close face is
            // behind this piece. Its former portrait-like rectangle disappears.
            var before = IntruderRevealSetup.TransformState.Read(c.backing); var after = before;
            after.position = c.backing.parent.InverseTransformPoint(new Vector3(-5.88f, 1.61f, 36.96f));
            after.rotation = Quaternion.Inverse(c.backing.parent.rotation) * Quaternion.Euler(0, 90, 0);
            after.scale = new Vector3(.024f, 2.92f, .56f);
            r.cheek = new IntruderRevealSetup.PropEdit
            { target = c.backing, parent = c.backing.parent, before = before, applied = after };
            r.cheekRenderer = c.backing.GetComponent<MeshRenderer>();
            r.cheekMesh = c.backing.GetComponent<MeshFilter>();
            r.previousMaterials = r.cheekRenderer.sharedMaterials;
            r.appliedMaterials = c.jamb.GetComponent<MeshRenderer>().sharedMaterials;
            r.contact = c.contact; r.previousContact = c.contact.clip; r.appliedContact = c.dry;
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Record(r);
            foreach (var e in r.lights) e.applied.Write(e.source);
            r.cheek.applied.Write(r.cheek.target);
            r.cheekRenderer.sharedMaterials = r.appliedMaterials;
            r.contact.clip = r.appliedContact;
            SetAudio(r, true);
            Dirty(r);
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("P3 R6 reveal upgraded; scene UNSAVED. Short physical side reveal, fixed near-face light, dry single frame contact then one breath. B remains the only contact/pose owner. Pump, Echo, camera and functional geometry untouched. Central image/listening still required.", c.staging);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (root) Object.DestroyImmediate(root);
            throw;
        }
    }

    [MenuItem("Echo Protocol/P3 rupture reveal round 6/3. Roll back current scene")]
    public static void RollBackCurrentScene()
    {
        var c = Validate();
        if (!c.installed) { Debug.Log("P3 R6 reveal is not installed; nothing changed."); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Roll back P3 rupture reveal");
        try
        {
            var r = c.installed; Record(r);
            foreach (var e in r.lights) e.before.Write(e.source);
            r.cheek.before.Write(r.cheek.target);
            r.cheekRenderer.sharedMaterials = r.previousMaterials;
            r.contact.clip = r.previousContact;
            SetAudio(r, false); Dirty(r);
            Undo.DestroyObjectImmediate(r.gameObject);
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Debug.Log("P3 R5 reveal/audio restored; scene UNSAVED. Face and acting upgrades remain independently installed.", c.staging);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play before using the R6 reveal integrator.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded
            && c.scene.path.EndsWith("/EchoProtocol_Incursion.unity", StringComparison.Ordinal),
            "Use the already open Incursion scene. No scene will be opened or saved.");
        c.staging = All<IntruderStaging>(c.scene).Single();
        c.presence = c.staging.GetComponent<MachineCoverPresence>();
        c.previous = All<IntruderRevealSetup>(c.scene).Single();
        var s = new SerializedObject(c.staging);
        c.body = Get<P3BodyIntrusion>(s, "p3Intrusion");
        c.key = Get<Light>(s, "p3Practical");
        c.fill = Named<Light>(c.scene, "P3 / returned alcove wall reflection");
        c.backing = Named<Transform>(c.scene, "A domestic papered backing");
        c.jamb = All<Transform>(c.scene).Single(t => t.name == "A sealed window jamb" && t.position.z > 36);
        Require(c.staging.enabled && c.presence && c.presence.enabled && c.body && c.body.enabled && c.body.IsConfigured
            && c.previous.revision == 5 && c.previous.staging == c.staging
            && c.previous.lights != null && c.previous.lights.Length == 6
            && c.previous.props != null && c.previous.props.Length == 2,
            "Requires the existing R5 staging, acting, reveal and discrete-audio installation. Do not rerun old generators.");
        Require(c.key && c.fill && c.key.enabled && c.fill.enabled && c.key.type == LightType.Spot
            && c.previous.lights[0].source == c.key && c.previous.lights[2].source == c.fill
            && Mathf.Abs(s.FindProperty("p3RevealIntensity").floatValue - c.previous.appliedP3Intensity) < .0001f,
            "The R5 P3 light owners/references changed. Preserve/reconcile before applying.");
        foreach (var l in new[] { c.key, c.fill })
            Require(l.gameObject.scene == c.scene && l.transform.parent && l.transform.parent.lossyScale == Vector3.one,
                "P3 lights require their original unscaled scene parents.");
        Require(c.backing.GetComponent<MeshRenderer>() && c.backing.GetComponent<MeshFilter>()
            && c.backing.GetComponent<MeshFilter>().sharedMesh
            && Vector3.Distance(c.backing.GetComponent<MeshFilter>().sharedMesh.bounds.size, Vector3.one) < .001f
            && c.backing.GetComponentsInChildren<Collider>(true).Length == 0
            && c.backing.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 && c.backing.childCount == 0
            && c.backing.parent && c.backing.parent.lossyScale == Vector3.one,
            "The backing must remain a standalone visual piece, without collider, script or children.");
        Require(c.previous.props[0].target == c.backing && c.previous.props[1].target == c.jamb
            // The campaign keeps R1 inactive in the saved scene until arrival.
            && c.previous.props[1].applied.Matches(c.jamb) && c.jamb.gameObject.activeSelf
            && c.jamb.GetComponent<MeshRenderer>() && c.jamb.GetComponent<MeshRenderer>().enabled
            && c.jamb.GetComponent<MeshRenderer>().sharedMaterials.Length == 1
            && c.jamb.GetComponent<MeshRenderer>().sharedMaterial,
            "B's reserved north jamb changed. This upgrade never edits that grip edge.");
        var body = new SerializedObject(c.body); var presence = new SerializedObject(c.presence);
        c.contact = Get<AudioSource>(body, "contact");
        Require(c.contact && c.contact.gameObject.scene == c.scene && c.contact.outputAudioMixerGroup
            && c.contact.outputAudioMixerGroup.name == "WorldSFX" && c.contact.spatialBlend == 1
            && !c.contact.loop && !c.contact.playOnAwake && !c.contact.ignoreListenerPause && !c.contact.ignoreListenerVolume
            && c.contact.pitch == 1 && c.contact.volume <= .48f
            && presence.FindProperty("discreteBreathing").boolValue
            && Get<P3BodyIntrusion>(presence, "dreadP3") == c.body && Get<AudioClip>(presence, "recoveryBreath"),
            "P3 requires its one routed physical contact and R5 breath owner, with comfortable existing gain.");
        c.dry = AssetDatabase.LoadAssetAtPath<AudioClip>(DryPath);
        Require(c.dry && c.dry.channels == 1 && c.dry.length > .15f && c.dry.length < .19f,
            "Missing traced mono DryContact master. Keep the existing audio import settings.");
        var receipts = All<P3RuptureRevealSetup>(c.scene).ToArray();
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        Require(receipts.Length <= 1, "Duplicate R6 reveal receipts; reconcile before proceeding.");
        if (receipts.Length == 0)
        {
            Require(roots.Length == 0 && !presence.FindProperty("ruptureP3Presentation").boolValue
                && !Get<Object>(s, "p3RuptureReveal")
                && !Get<Object>(presence, "ruptureP3Staging") && !Get<Object>(presence, "ruptureP3Contact"),
                "Orphaned/custom R6 audio wiring; preserve/reconcile before applying.");
            foreach (int i in new[] { 0, 2 })
            {
                var e = c.previous.lights[i];
                Require(e.source.transform.parent == e.parent && e.applied.Matches(e.source), "A P3 R5 light changed; nothing written.");
            }
            Require(c.previous.props[0].applied.Matches(c.backing)
                && c.previous.props[0].parent == c.backing.parent
                && c.contact.clip == AssetDatabase.LoadAssetAtPath<AudioClip>(R5ContactPath),
                "R5 backing/contact changed. Review before replacing the local presentation.");
            return c;
        }
        c.installed = receipts[0]; var r = c.installed;
        Require(roots.Length == 1 && roots[0] == r.transform && r.transform.parent == c.staging.transform.parent
            && r.revision == 6 && r.staging == c.staging && r.presence == c.presence && r.roundFive == c.previous
            && r.lights != null && r.lights.Length == 2
            && r.lights.Select(e => e.source).SequenceEqual(new[] { c.key, c.fill })
            && r.cheek.target == c.backing && r.contact == c.contact && r.previousContact && r.appliedContact == c.dry
            && r.cheekRenderer == c.backing.GetComponent<MeshRenderer>()
            && r.cheekMesh == c.backing.GetComponent<MeshFilter>()
            && Get<P3RuptureRevealSetup>(s, "p3RuptureReveal") == r
            && r.previousMaterials != null && r.previousMaterials.Length > 0
            && r.appliedMaterials != null && r.appliedMaterials.Length == 1
            && r.cheekRenderer.sharedMaterials.SequenceEqual(r.appliedMaterials)
            && c.contact.clip == r.appliedContact && presence.FindProperty("ruptureP3Presentation").boolValue
            && Get<IntruderStaging>(presence, "ruptureP3Staging") == c.staging
            && Get<AudioSource>(presence, "ruptureP3Contact") == c.contact,
            "Partial/edited R6 reveal receipt or audio wiring; nothing written.");
        foreach (var e in r.lights)
            Require(e.source.transform.parent == e.parent && e.applied.Matches(e.source),
                "A R6 light changed after installation. Preserve/reconcile before rollback.");
        Require(r.cheek.target.parent == r.cheek.parent && r.cheek.applied.Matches(r.cheek.target),
            "The R6 side return changed after installation. Preserve/reconcile before rollback.");
        return c;
    }

    static IntruderRevealSetup.LightEdit LightEdit(Light light)
    {
        var state = IntruderRevealSetup.LightState.Read(light);
        return new IntruderRevealSetup.LightEdit { source = light, parent = light.transform.parent, before = state, applied = state };
    }
    static void SetAudio(P3RuptureRevealSetup r, bool active)
    {
        var s = new SerializedObject(r.presence);
        s.FindProperty("ruptureP3Presentation").boolValue = active;
        s.FindProperty("ruptureP3Staging").objectReferenceValue = active ? r.staging : null;
        s.FindProperty("ruptureP3Contact").objectReferenceValue = active ? r.contact : null;
        s.ApplyModifiedProperties();
        s = new SerializedObject(r.staging);
        s.FindProperty("p3RuptureReveal").objectReferenceValue = active ? r : null;
        s.ApplyModifiedProperties();
    }
    static Object[] Edited(P3RuptureRevealSetup r) => new Object[] { r.staging, r.presence, r.contact, r.cheek.target, r.cheekRenderer }
        .Concat(r.lights.SelectMany(e => new Object[] { e.source, e.source.transform })).ToArray();
    static void Record(P3RuptureRevealSetup r) => Undo.RecordObjects(Edited(r), UndoName);
    static void Dirty(P3RuptureRevealSetup r)
    { foreach (var o in Edited(r)) { EditorUtility.SetDirty(o); PrefabUtility.RecordPrefabInstancePropertyModifications(o); } }
    static T Get<T>(SerializedObject s, string name) where T : Object => s.FindProperty(name).objectReferenceValue as T;
    static T Named<T>(Scene scene, string name) where T : Component => All<T>(scene).Single(t => t.name == name);
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

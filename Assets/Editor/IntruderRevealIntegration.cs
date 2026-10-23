using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderPresence;
using EchoProtocol.Campaign.IntruderReveal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Upgrade the installed round-four scene, without rerunning its installer or
// modifying its receipt. All edits are scene-local, explicit and reversible.
public static class IntruderRevealIntegration
{
    const string RootName = "IntruderReveal / threshold and divided alcove";
    const string UndoName = "Intruder reveal round five";
    const float P3Intensity = 1.35f;
    sealed class Context
    {
        public Scene scene;
        public IntruderStaging staging;
        public IntruderPresenceSetup previous;
        public IntruderRevealSetup installed;
        public Light p3, pump, bounce, lamp, work, service;
        public Transform backing, jamb;
        public Light[] Lights => new[] { p3, pump, bounce, lamp, work, service };
        public Transform[] Props => new[] { backing, jamb };
    }

    [MenuItem("Echo Protocol/Intruder reveal round 5/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "Round-five reveal intact; Upgrade is a no-op."
            : "Round-four lights and two visual props ready. Upgrade will reuse six lights; no new light, collider or event. Scene untouched.", c.staging);
    }

    [MenuItem("Echo Protocol/Intruder reveal round 5/2. Upgrade current scene")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("Round-five reveal already installed; nothing changed.", c.staging); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(UndoName);
        GameObject root = null;
        try
        {
            root = new GameObject(RootName); root.transform.SetParent(c.staging.transform.parent, false);
            var receipt = root.AddComponent<IntruderRevealSetup>();
            receipt.staging = c.staging; receipt.roundFour = c.previous;
            receipt.previousP3Intensity = P3Level(c.staging); receipt.appliedP3Intensity = P3Intensity;
            // A fixed lateral key reaches the advancing body, fingers and face,
            // but ends before the floral rear wall. It never tracks the head.
            receipt.lights = new[]
            {
                Key(c.p3, new Vector3(-6.06f, 2.28f, 36.78f), new Vector3(-5.76f, 1.43f, 35.98f),
                    new Color(.83f, .89f, .88f), P3Intensity, 2.55f, 96, 58),
                // Across the threshold AND landing: the existing intensity
                // envelope stays with Presence (.25 idle / 1.65 active).
                Key(c.pump, new Vector3(-5.12f, 2.12f, -44.12f), new Vector3(-4.26f, 1.38f, -45.20f),
                    new Color(.85f, .90f, .87f), c.pump.intensity, 3.8f, 88, 56),
                Fill(c.bounce, new Color(.77f, .84f, .83f), .24f, 1.85f),
                // Keep the lamp at its real fitting. Warmth belongs to the
                // domestic corner/recorded adult, not a frontal portrait fill.
                Key(c.lamp, c.lamp.transform.position, new Vector3(-4.94f, 1.12f, 35.03f),
                    c.lamp.color, c.lamp.intensity, 2.7f, 82, 50, false),
                // Same source, gain and causal envelope; keep the return control
                // legible while removing its broad spill across the visitor.
                Key(c.work, c.work.transform.position, new Vector3(-6.59f, 1.17f, 36.24f),
                    c.work.color, c.work.intensity, c.work.range, 46, 30, false),
                // Reuse the existing ceiling fitting as an oblique wash into the
                // aperture. The leaf itself occludes it until physically open.
                Key(c.service, c.service.transform.position, new Vector3(-4.48f, 1.22f, -45.95f),
                    new Color(.88f, .79f, .63f), 2.8f, 3.7f, 88, 52)
            };
            receipt.props = new[]
            {
                // Retain domestic context behind the real Echo; expose the
                // existing darker structural wall behind the present intruder.
                Prop(c.backing, new Vector3(-3.916f, 1.61f, 34.78f), new Vector3(.024f, 2.92f, 1.42f)),
                // Existing decorative north reveal becomes a broad oblique
                // edge on the return approach. Face/palm lane remains clear.
                Prop(c.jamb, new Vector3(-6.3f, 1.76f, 36.94f), new Vector3(.28f, 1.98f, .285f))
            };
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            Record(receipt);
            foreach (var edit in receipt.lights) edit.applied.Write(edit.source);
            foreach (var edit in receipt.props) edit.applied.Write(edit.target);
            SetP3Level(c.staging, P3Intensity);
            RecordPrefabEdits(receipt);
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Round-five reveal upgraded; scene UNSAVED. Divided P3 backdrop, wider north reveal and fixed threshold light. Existing animation, audio, control/escape geometry and event timing untouched. Validate P3 + pump once centrally; Undo or Roll back restores round-four visual values.", c.staging);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (root) Object.DestroyImmediate(root);
            throw;
        }
    }

    [MenuItem("Echo Protocol/Intruder reveal round 5/3. Roll back current scene")]
    public static void RollBackCurrentScene()
    {
        var c = Validate();
        if (!c.installed) { Debug.Log("Round-five reveal is not installed; nothing changed."); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Roll back intruder reveal");
        try
        {
            var receipt = c.installed; Record(receipt);
            foreach (var edit in receipt.lights) edit.before.Write(edit.source);
            foreach (var edit in receipt.props) edit.before.Write(edit.target);
            SetP3Level(c.staging, receipt.previousP3Intensity);
            RecordPrefabEdits(receipt);
            Undo.DestroyObjectImmediate(receipt.gameObject);
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Debug.Log("Round-four reveal values restored; scene UNSAVED. Other round-five upgrades remain installed.", c.staging);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play before upgrading/rolling back reveal.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded
            && c.scene.path.EndsWith("/EchoProtocol_Incursion.unity", StringComparison.Ordinal),
            "Use the already open Incursion scene. This tool never opens or saves scenes.");
        c.staging = All<IntruderStaging>(c.scene).Single();
        c.previous = All<IntruderPresenceSetup>(c.scene).Single();
        var s = new SerializedObject(c.staging);
        var presence = c.staging.GetComponent<MachineCoverPresence>();
        Require(c.staging.enabled && presence && c.previous.revision == 1 && c.previous.staging == c.staging
            && c.previous.presence == presence && c.previous.lights != null && c.previous.lights.Length == 4,
            "Requires the saved round-four presence installation on the same body. Do not rerun historical generators.");
        c.p3 = s.FindProperty("p3Practical").objectReferenceValue as Light;
        c.pump = new SerializedObject(presence).FindProperty("threatPractical").objectReferenceValue as Light;
        c.bounce = Named<Light>(c.scene, "P3 / returned alcove wall reflection");
        c.lamp = Named<Light>(c.scene, "A witness warm lamp");
        c.work = Named<Light>(c.scene, "A return work light");
        c.service = Named<Light>(c.scene, "Service / last practical");
        c.backing = Named<Transform>(c.scene, "A domestic papered backing");
        c.jamb = All<Transform>(c.scene).Single(t => t.name == "A sealed window jamb" && t.position.z > 36);
        Require(c.Lights.All(l => l && l.enabled && l.gameObject.scene == c.scene && l.transform.parent
                && l.transform.parent.lossyScale == Vector3.one)
            && c.previous.lights.Select(e => e.source).SequenceEqual(c.Lights.Take(4)),
            "The six local lights or round-four references changed; nothing written.");
        foreach (var prop in c.Props)
            Require(prop.GetComponent<MeshRenderer>() && prop.GetComponent<MeshFilter>()
                && prop.GetComponentsInChildren<Collider>(true).Length == 0
                && prop.GetComponentsInChildren<MonoBehaviour>(true).Length == 0
                && prop.childCount == 0 && prop.parent && prop.parent.lossyScale == Vector3.one,
                "Expected standalone visual backing/jamb with no collider, behaviour or child. Functional architecture must stay untouched.");
        var installed = All<IntruderRevealSetup>(c.scene).ToArray();
        var roots = All<Transform>(c.scene).Where(t => t.name == RootName).ToArray();
        Require(installed.Length <= 1, "Duplicate reveal receipts; preserve/reconcile before proceeding.");
        if (installed.Length == 0)
        {
            Require(roots.Length == 0 && Mathf.Abs(P3Level(c.staging) - c.previous.lights[0].applied.intensity) < .0001f,
                "Orphaned/edited reveal profile; nothing written.");
            foreach (var edit in c.previous.lights)
                Require(edit.source && edit.applied.Matches(edit.source), "A round-four light was edited. Preserve/reconcile before upgrading.");
            Require(c.p3.type == LightType.Spot && c.pump.type == LightType.Spot && c.work.type == LightType.Spot
                && c.service.type == LightType.Point && Mathf.Abs(c.service.intensity - 1.6f) < .001f
                && Vector3.Distance(c.service.transform.position, new Vector3(-3.5f, 2.5f, -45.4f)) < .005f
                && Vector3.Distance(c.backing.position, new Vector3(-3.916f, 1.61f, 35.52f)) < .005f
                && Vector3.Distance(c.backing.localScale, new Vector3(.024f, 2.92f, 2.9f)) < .005f
                && Vector3.Distance(c.jamb.position, new Vector3(-6.3f, 1.76f, 37.05f)) < .005f,
                "The authored local fittings/backdrop moved; review before applying this composition.");
            return c;
        }
        c.installed = installed[0]; var r = c.installed;
        Require(roots.Length == 1 && roots[0] == r.transform && r.transform.parent == c.staging.transform.parent
            && r.revision == 5 && r.staging == c.staging && r.roundFour == c.previous
            && r.lights != null && r.lights.Length == 6 && r.props != null && r.props.Length == 2
            && r.lights.Select(e => e.source).SequenceEqual(c.Lights)
            && r.props.Select(e => e.target).SequenceEqual(c.Props)
            && Mathf.Abs(P3Level(c.staging) - r.appliedP3Intensity) < .0001f,
            "Partial/modified round-five receipt; nothing written.");
        foreach (var e in r.lights)
            Require(e.source.transform.parent == e.parent && e.applied.Matches(e.source),
                "An upgraded light changed after installation; preserve/reconcile before rollback.");
        foreach (var e in r.props)
            Require(e.target.parent == e.parent && e.applied.Matches(e.target),
                "An upgraded visual prop changed after installation; preserve/reconcile before rollback.");
        return c;
    }

    static IntruderRevealSetup.LightEdit Fill(Light light, Color color, float intensity, float range)
    {
        var before = IntruderRevealSetup.LightState.Read(light); var after = before;
        after.values.color = color; after.values.intensity = intensity; after.values.range = range;
        return new IntruderRevealSetup.LightEdit { source = light, parent = light.transform.parent, before = before, applied = after };
    }
    static IntruderRevealSetup.LightEdit Key(Light light, Vector3 position, Vector3 aim, Color color,
        float intensity, float range, float outer, float inner, bool shadows = true)
    {
        var edit = Fill(light, color, intensity, range); var after = edit.applied;
        after.type = LightType.Spot;
        after.values.position = light.transform.parent.InverseTransformPoint(position);
        after.values.rotation = Quaternion.Inverse(light.transform.parent.rotation) * Quaternion.LookRotation(aim - position);
        after.values.outer = outer; after.values.inner = inner;
        if (shadows) { after.values.shadows = LightShadows.Soft; after.values.bias = .015f; after.values.normalBias = .06f; }
        edit.applied = after; return edit;
    }
    static IntruderRevealSetup.PropEdit Prop(Transform target, Vector3 position, Vector3 scale)
    {
        var before = IntruderRevealSetup.TransformState.Read(target); var after = before;
        after.position = target.parent.InverseTransformPoint(position); after.scale = scale;
        return new IntruderRevealSetup.PropEdit { target = target, parent = target.parent, before = before, applied = after };
    }
    static Object[] Edited(IntruderRevealSetup r) => new Object[] { r.staging }
        .Concat(r.lights.SelectMany(e => new Object[] { e.source, e.source.transform }))
        .Concat(r.props.Select(e => (Object)e.target)).ToArray();
    static void Record(IntruderRevealSetup r) => Undo.RecordObjects(Edited(r), UndoName);
    static void RecordPrefabEdits(IntruderRevealSetup r)
    { foreach (var target in Edited(r)) { EditorUtility.SetDirty(target); PrefabUtility.RecordPrefabInstancePropertyModifications(target); } }
    static float P3Level(IntruderStaging target) => new SerializedObject(target).FindProperty("p3RevealIntensity").floatValue;
    static void SetP3Level(IntruderStaging target, float value)
    { var s = new SerializedObject(target); s.FindProperty("p3RevealIntensity").floatValue = value; s.ApplyModifiedProperties(); }
    static T Named<T>(Scene scene, string name) where T : Component => All<T>(scene).Single(t => t.name == name);
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

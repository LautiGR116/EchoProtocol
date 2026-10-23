using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Campaign.P3Intrusion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Explicit R5 -> R6 P3 upgrade. Never opens/saves a scene, edits the model,
// samples a pose or writes pump/audio/presentation configuration.
public static class P3RupturePerformanceIntegration
{
    static readonly Vector3 StopR5 = new Vector3(-5.68f, .03f, 36.02f);
    static readonly Vector3 HandR5 = new Vector3(-6.2425f, 1.43f, 36.28f);
    static readonly Vector3 StopR6 = new Vector3(-5.72f, .03f, 36.34f);
    static readonly Quaternion HandR6Rotation = Quaternion.LookRotation(Vector3.left, Vector3.back);
    sealed class Context
    {
        public Scene scene;
        public P3BodyIntrusion p3;
        public Transform stop, hand;
        public MeshFilter frame;
        public P3RupturePerformanceReceipt receipt;
    }

    [MenuItem("Echo Protocol/P3 rupture performance/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.receipt ? "P3 R6 performance installed; no changes."
            : "P3 R5 ready for opt-in R6 jamb grip. Central Play must check reach/acting. No changes.", c.p3);
    }

    [MenuItem("Echo Protocol/P3 rupture performance/2. Upgrade from round 5")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate();
        if (c.receipt) { Debug.Log("P3 R6 already installed; no changes.", c.p3); return; }
        // Derive wrist from the actual saved scan's knuckles, without posing it.
        var s = new SerializedObject(c.p3);
        var palm = s.FindProperty("highPalm");
        var arm = palm.FindPropertyRelative("arm");
        var end = (Transform)arm.FindPropertyRelative("end").objectReferenceValue;
        var index = (Transform)palm.FindPropertyRelative("index").objectReferenceValue;
        var middle = (Transform)palm.FindPropertyRelative("middle").objectReferenceValue;
        var little = (Transform)palm.FindPropertyRelative("little").objectReferenceValue;
        Vector3 fingers = end.InverseTransformDirection(middle.position - end.position).normalized;
        Vector3 across = end.InverseTransformDirection(index.position - little.position).normalized;
        Quaternion orientation = Quaternion.Inverse(Quaternion.LookRotation(Vector3.Cross(fingers, across).normalized, fingers));
        Quaternion handRotation = HandR6Rotation * orientation;
        var digits = palm.FindPropertyRelative("digits");
        float maxZ = float.NegativeInfinity;
        int chains = 0, thumbs = 0;
        for (int n = 0; n < digits.arraySize; n++)
        {
            var bone = digits.GetArrayElementAtIndex(n).objectReferenceValue as Transform;
            if (!bone || !bone.name.Contains("_01_")) continue;
            if (bone.childCount == 0 || bone.GetChild(0).childCount == 0 || bone.GetChild(0).GetChild(0).childCount == 0)
                throw new InvalidOperationException("R6 needs the complete existing finger chains, including tips.");
            if (bone.name.Contains("thumb")) { thumbs++; continue; }
            Vector3 offset = handRotation * (Quaternion.Inverse(end.rotation) * (bone.position - end.position));
            maxZ = Mathf.Max(maxZ, offset.z); chains++;
        }
        if (chains != 4 || thumbs != 1) throw new InvalidOperationException("Expected four existing finger chains and thumb. Nothing changed.");
        Vector3 wrist = new Vector3(-6.12f, 1.30f, 36.7975f - .023f - maxZ);
        if (wrist.z < 36.86f || wrist.z > 36.94f)
            throw new InvalidOperationException("Saved hand differs from R5. Review its neutral orientation before upgrading.");

        Mutate(c, "Upgrade P3 to R6 rupture performance", () =>
        {
            c.receipt = Undo.AddComponent<P3RupturePerformanceReceipt>(c.p3.gameObject);
            Undo.RecordObject(c.receipt, "Record P3 R5 anchors");
            c.receipt.owner = c.p3; c.receipt.stop = c.stop; c.receipt.hand = c.hand; c.receipt.frame = c.frame;
            c.receipt.stopParent = c.stop.parent; c.receipt.handParent = c.hand.parent;
            c.receipt.previousStop = c.stop.position; c.receipt.previousStopRotation = c.stop.rotation;
            c.receipt.previousHand = c.hand.position; c.receipt.previousHandRotation = c.hand.rotation;
            c.receipt.installedHand = wrist;
            c.stop.position = StopR6;
            c.hand.SetPositionAndRotation(wrist, HandR6Rotation);
            s.Update(); s.FindProperty("rupturePerformance").boolValue = true;
            s.FindProperty("gripFrame").objectReferenceValue = c.frame; s.ApplyModifiedProperties();
        });
    }

    [MenuItem("Echo Protocol/P3 rupture performance/3. Restore round 5")]
    public static void RestoreRound5()
    {
        var c = Validate();
        if (!c.receipt) { Debug.Log("P3 already uses R5; no changes.", c.p3); return; }
        Mutate(c, "Restore P3 round 5 performance", () =>
        {
            c.stop.SetPositionAndRotation(c.receipt.previousStop, c.receipt.previousStopRotation);
            c.hand.SetPositionAndRotation(c.receipt.previousHand, c.receipt.previousHandRotation);
            var s = new SerializedObject(c.p3);
            s.FindProperty("rupturePerformance").boolValue = false;
            s.FindProperty("gripFrame").objectReferenceValue = null; s.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(c.receipt); c.receipt = null;
        });
    }

    static void Mutate(Context c, string label, Action apply)
    {
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(label);
        try
        {
            Undo.RecordObjects(new Object[] { c.p3, c.stop, c.hand }, label);
            apply();
            foreach (var obj in new Object[] { c.p3, c.stop, c.hand }) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            if (c.receipt) PrefabUtility.RecordPrefabInstancePropertyModifications(c.receipt);
            Validate();
            EditorSceneManager.MarkSceneDirty(c.scene); Undo.CollapseUndoOperations(group);
            Debug.Log(label + "; scene UNSAVED. Undo or the R6 restore menu reverses only B's P3 changes. "
                + "No pump, audio, mesh, camera, puzzle or Echo changes.", c.p3);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before integrating P3 R6.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Use Incursion already open. This integrator never opens or saves scenes.");
        c.p3 = All<P3BodyIntrusion>(c.scene).Single();
        var s = new SerializedObject(c.p3);
        c.stop = Get<Transform>(s, "stop");
        c.hand = s.FindProperty("highPalm").FindPropertyRelative("target").objectReferenceValue as Transform;
        var acting = Get<IntruderActingFocus>(s, "acting");
        var owner = Get<IntruderStaging>(s, "owner");
        var controller = Get<CharacterController>(s, "controller");
        var glass = Get<BoxCollider>(s, "glazing");
        if (!c.p3.enabled || !c.p3.IsConfigured || !acting || !acting.UsesContainedViolence || !owner
            || Get<P3BodyIntrusion>(new SerializedObject(owner), "p3Intrusion") != c.p3
            || !controller || !controller.enabled || Mathf.Abs(controller.radius - .4f) > .001f || controller.skinWidth > .036f
            || !c.stop || !c.hand || !glass || !glass.enabled || glass.isTrigger)
            throw new InvalidOperationException("R6 needs the complete, enabled R5 present actor/P3/controller/solid glazing installation.");
        var stand = Get<Transform>(new SerializedObject(owner), "p3Stand");
        if (!stand || Vector3.Distance(stand.position, new Vector3(-4.55f, .03f, 35.8f)) > .001f
            || Quaternion.Angle(stand.rotation, Quaternion.Euler(0, -90, 0)) > .1f)
            throw new InvalidOperationException("The R5 hidden stand changed. Review the approach first.");
        c.frame = All<MeshFilter>(c.scene).Single(f => f.name == "A sealed window jamb"
            && Vector3.Distance(f.transform.position, new Vector3(-6.3f, 1.76f, 36.94f)) < .001f);
        var renderer = c.frame.GetComponent<MeshRenderer>();
        if (!c.frame.sharedMesh || !renderer || !renderer.enabled
            || Vector3.Distance(c.frame.transform.lossyScale, new Vector3(.28f, 1.98f, .285f)) > .001f
            || Quaternion.Angle(c.frame.transform.rotation, Quaternion.identity) > .1f
            || Vector3.Distance(c.frame.sharedMesh.bounds.size, Vector3.one) > .001f
            || c.frame.sharedMesh.bounds.center.sqrMagnitude > .000001f)
            throw new InvalidOperationException("Keep the existing R5 north jamb geometry. Its east/south corner owns the grip.");
        Vector3 glassFace = glass.transform.TransformPoint(glass.center + Vector3.right * glass.size.x * .5f);
        if (Mathf.Abs(glassFace.x + 6.2825f) > .001f || Vector3.Dot(glass.transform.right, Vector3.right) < .999f)
            throw new InvalidOperationException("A glazing plane changed; review clearance before integrating.");
        c.receipt = c.p3.GetComponent<P3RupturePerformanceReceipt>();
        if (All<P3RupturePerformanceReceipt>(c.scene).Count() != (c.receipt ? 1 : 0))
            throw new InvalidOperationException("Conflicting R6 receipt. Nothing changed.");
        if (c.receipt)
        {
            if (!c.p3.UsesRupturePerformance || c.receipt.owner != c.p3 || c.receipt.frame != c.frame
                || Get<MeshFilter>(s, "gripFrame") != c.frame || c.receipt.stop != c.stop || c.receipt.hand != c.hand
                || c.receipt.stopParent != c.stop.parent || c.receipt.handParent != c.hand.parent
                || Vector3.Distance(c.stop.position, StopR6) > .001f
                || Vector3.Distance(c.hand.position, c.receipt.installedHand) > .001f
                || Quaternion.Angle(c.hand.rotation, HandR6Rotation) > .1f)
                throw new InvalidOperationException("Partial/modified R6 acting; resolve its owned anchors or Undo before upgrading/restoring.");
        }
        else if (c.p3.UsesRupturePerformance || Get<MeshFilter>(s, "gripFrame")
            || Vector3.Distance(c.stop.position, StopR5) > .001f || Vector3.Distance(c.hand.position, HandR5) > .001f)
            throw new InvalidOperationException("R6 upgrade expects the unchanged R5 stop and right-hand target, with no partial opt-in.");
        return c;
    }

    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
    static T Get<T>(SerializedObject s, string field) where T : Object => s.FindProperty(field)?.objectReferenceValue as T;
}

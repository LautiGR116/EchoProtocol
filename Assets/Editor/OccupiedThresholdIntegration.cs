using System;
using System.Linq;
using EchoProtocol.Campaign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit installation into the already open scene. No initialize-on-load,
// scene load/save, imported asset edit or hidden runtime anchor generation.
public static class OccupiedThresholdIntegration
{
    [MenuItem("Echo Protocol/One shot/Install occupied threshold")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before composing the threshold.");
        var scene=SceneManager.GetActiveScene();
        if(!scene.path.EndsWith("/EchoProtocol_Incursion.unity",StringComparison.Ordinal))
            throw new InvalidOperationException("Use the already open Incursion scene.");
        var staging=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<IntruderStaging>(true)).Single();
        if(staging.HasOccupiedThreshold)
        {Debug.Log("Occupied threshold already wired; authored positions preserved.",staging);return;}
        var s=new SerializedObject(staging);
        if(s.FindProperty("occupiedDoorStand").objectReferenceValue||s.FindProperty("occupiedDoorHand").objectReferenceValue
            ||s.FindProperty("occupiedDoorContact").objectReferenceValue)
            throw new InvalidOperationException("Partial threshold references exist; preserve and finish that composition.");
        var threat=staging.GetComponent<MachineCoverThreat>();
        var start=new SerializedObject(threat).FindProperty("start").objectReferenceValue as Transform;
        var hand=s.FindProperty("breachHand").objectReferenceValue as Transform;
        var reference=s.FindProperty("cornerContact").objectReferenceValue as AudioSource;
        if(!staging.Embestida||!start||!hand||!reference||!reference.clip||!reference.outputAudioMixerGroup)
            throw new InvalidOperationException("Requires the existing start, frame hand and routed contact sound.");
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Compose occupied threshold");
        GameObject root=null;
        try
        {
            root=new GameObject("One shot / occupied threshold");
            root.transform.SetParent(staging.transform.parent,false);
            var stand=new GameObject("Occupied threshold / body stand").transform;stand.SetParent(root.transform,false);
            var palm=new GameObject("Occupied threshold / palm on frame").transform;palm.SetParent(root.transform,false);
            palm.SetPositionAndRotation(hand.position,hand.rotation);
            Vector3 approach=hand.position-start.position;approach.y=0;
            if(approach.sqrMagnitude<.01f)approach=start.forward;
            // Default remains on the existing actor side, within arm's reach.
            // Central composition can adjust it; repeated Apply keeps edits.
            Vector3 point=hand.position-approach.normalized*.55f;point.y=start.position.y;
            stand.SetPositionAndRotation(point,start.rotation);
            var contact=palm.gameObject.AddComponent<AudioSource>();EditorUtility.CopySerialized(reference,contact);
            contact.playOnAwake=false;contact.loop=false;contact.ignoreListenerPause=false;
            contact.volume=Mathf.Min(reference.volume,.32f);
            contact.spatialBlend=1;contact.dopplerLevel=0;
            Undo.RegisterCreatedObjectUndo(root,"Compose occupied threshold");
            Undo.RecordObject(staging,"Compose occupied threshold");
            s.FindProperty("occupiedDoorStand").objectReferenceValue=stand;
            s.FindProperty("occupiedDoorHand").objectReferenceValue=palm;
            s.FindProperty("occupiedDoorContact").objectReferenceValue=contact;
            s.FindProperty("occupiedDoorAngle").floatValue=22;
            s.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
            Debug.Log("Occupied threshold wired, scene UNSAVED. Compose face/hand through the 22-degree leaf from the crank approach. "
                +"Second recorded run earns the approach; actual palm contact owns one sound. "
                +"Move the body/palm markers only within the existing alcove/frame. No automatic save.",staging);
        }
        catch{Undo.RevertAllDownToGroup(group);if(root)UnityEngine.Object.DestroyImmediate(root);throw;}
    }
}

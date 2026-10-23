using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Round 5 is an explicit opt-in on the already installed round 4 actor.
// The sole scene edit is this flag; rollback retains all round 4 wiring.
public static class IntruderPerformanceIntegration
{
    const string FlagName = "containedViolence";

    [MenuItem("Echo Protocol/Intruder performance/1. Validate round 4 installation")]
    public static void ValidateCurrentScene()
    {
        var focus = ValidateInstallation();
        Debug.Log("Intruder performance: round 4 installation valid; round 5 is "
            + (focus.UsesContainedViolence ? "enabled." : "available for explicit upgrade.")
            + " No scene changes.", focus);
    }

    [MenuItem("Echo Protocol/Intruder performance/2. Upgrade to round 5")]
    public static void UpgradeCurrentScene() => SetRound5(true);

    [MenuItem("Echo Protocol/Intruder performance/3. Restore round 4 performance")]
    public static void RestoreRound4Performance() => SetRound5(false);

    static void SetRound5(bool enabled)
    {
        var focus = ValidateInstallation();
        if (focus.UsesContainedViolence == enabled)
        {
            Debug.Log("Intruder performance: round " + (enabled ? "5" : "4")
                + " already selected; nothing changed.", focus);
            return;
        }

        string operation = enabled ? "Upgrade intruder performance to round 5"
            : "Restore round 4 intruder performance";
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(operation);
        try
        {
            Undo.RecordObject(focus, operation);
            var serialized = new SerializedObject(focus);
            Flag(serialized).boolValue = enabled;
            serialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(focus);
            if (focus.UsesContainedViolence != enabled)
                throw new InvalidOperationException("Intruder performance opt-in was not applied. Rolling back.");

            EditorSceneManager.MarkSceneDirty(focus.gameObject.scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log(operation + "; scene UNSAVED. Only the contained-violence flag changed. "
                + "Use Undo or the opposite performance menu to reverse it. "
                + "Round 4 wiring, contact targets, materials, lighting and audio are retained.", focus);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            throw;
        }
    }

    static IntruderActingFocus ValidateInstallation()
    {
        // This checks Incursion outside Play, the exclusive present rig, all
        // actor/P3/performance references, eyes, controller and contact targets.
        IntruderActingIntegration.ValidateCurrentScene();
        var scene = SceneManager.GetActiveScene();
        var installed = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<IntruderActingFocus>(true)).ToArray();
        if (installed.Length != 1)
            throw new InvalidOperationException("Round 5 requires the complete round 4 IntruderActing installation. Nothing changed.");

        var focus = installed[0];
        if (!focus.isActiveAndEnabled || !focus.IsConfigured || !focus.GetComponent<IntruderStaging>())
            throw new InvalidOperationException("Round 5 requires the enabled round 4 focus on the present intruder. Nothing changed.");
        Flag(new SerializedObject(focus));
        return focus;
    }

    static SerializedProperty Flag(SerializedObject serialized)
    {
        var flag = serialized.FindProperty(FlagName);
        if (flag == null || flag.propertyType != SerializedPropertyType.Boolean)
            throw new InvalidOperationException("The contained-violence runtime flag is missing. Compile the round 5 scripts before upgrading.");
        return flag;
    }
}

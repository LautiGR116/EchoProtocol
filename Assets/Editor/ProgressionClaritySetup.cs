using System;
using System.Linq;
using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Wires read-only guidance to the existing owners. No scene is opened or saved,
// no object is created and no puzzle, recording or timing setting is changed.
public static class ProgressionClaritySetup
{
    [MenuItem("Echo Protocol/Progression clarity/Apply to open Incursion scene")]
    public static void ApplyToOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before wiring progression guidance.");
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || !scene.path.EndsWith("EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Open the Incursion scene first. This command never opens or saves scenes.");
        GameSession session = All<GameSession>(scene).Single();
        var sessionData = new SerializedObject(session);
        ProtocolGuidance guidance = Get<ProtocolGuidance>(sessionData, "protocolGuidance");
        InteractionFocusHUD hud = Get<InteractionFocusHUD>(sessionData, "interactionFocusHud");
        var data = new SerializedObject(guidance);
        var first = new SerializedObject(Get<ParallelObservationRelay>(data, "firstRelay"));
        var second = new SerializedObject(Get<BlindAlignmentRelay>(data, "alignmentRelay"));
        var third = new SerializedObject(Get<RecombinationRelay>(data, "returnRelay"));
        // Resolve every required source before the first write.
        var references = new (string property, Object value)[]
        {
            ("loop", Get<LoopController>(sessionData, "loop")),
            ("recorder", Get<EchoRecorder>(sessionData, "recorder")),
            ("firstReaderA", Get<OpticalReader>(first, "echoReaderA")),
            ("firstReaderB", Get<OpticalReader>(first, "echoReaderB")),
            ("firstReaderC", Get<OpticalReader>(first, "playerServiceReader")),
            ("alignmentReader", Get<OpticalReader>(second, "alignmentReader")),
            ("returnReaderA", Get<OpticalReader>(third, "readerA")),
            ("returnReaderB", Get<OpticalReader>(third, "readerB"))
        };
        Undo.RecordObjects(new Object[] { guidance, hud }, "Wire contextual progression clarity");
        foreach (var pair in references) data.FindProperty(pair.property).objectReferenceValue = pair.value;
        bool changed = data.ApplyModifiedProperties();
        var hudData = new SerializedObject(hud);
        hudData.FindProperty("guidance").objectReferenceValue = guidance;
        changed |= hudData.ApplyModifiedProperties();
        if (changed)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(guidance);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hud);
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log(changed ? "Progression clarity wired to existing readers and recording. Save the scene after the focused check."
            : "Progression clarity references already match. No changes.", guidance);
    }

    static T Get<T>(SerializedObject owner, string property) where T : Object
    {
        var value = owner.FindProperty(property)?.objectReferenceValue as T;
        if (!value) throw new InvalidOperationException(owner.targetObject.name + " is missing " + property + ".");
        return value;
    }

    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
}

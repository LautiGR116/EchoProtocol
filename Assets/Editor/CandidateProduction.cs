using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;

public static class CandidateProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Candidate.unity";
    static void Bind(UnityEngine.Object owner,string name,UnityEngine.Object value)
    {
        if(!value)throw new InvalidOperationException("Missing "+name);
        var so=new SerializedObject(owner);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();
    }
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static T FindIncludingInactive<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    public static void RepairPassageBinding()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before repair.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!=ScenePath)throw new InvalidOperationException("Open Candidate first.");
        var actual=UnityEngine.Object.FindObjectsByType<VS1ParallelFigureEvent>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Single(e=>e.UsesPhysicalTraversal && e.enabled && e.gameObject.activeInHierarchy);
        Bind(Find<ProtocolGuidance>(),"passage",actual);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Candidate guidance now references the active physical SE2 event, not the disabled historical study.");
    }
    [MenuItem("Echo Protocol/Author delivery candidate")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before candidate authoring.");
        if(File.Exists(ScenePath))throw new InvalidOperationException("Candidate already exists; inspect rather than overwrite.");
        var scene=EditorSceneManager.OpenScene(SE2WalkthroughProduction.ScenePath);
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new IOException("Could not save candidate.");
        var guidance=new GameObject("Candidate / optional first-act notes").AddComponent<ProtocolGuidance>();
        Bind(guidance,"firstRelay",Find<ParallelObservationRelay>());
        Bind(guidance,"firstEvent",Find<VS1StrongEventOne>());
        Bind(guidance,"domesticCheckpoint",Find<VS1DomesticThresholdCheckpoint>());
        Bind(guidance,"alignmentRelay",Find<BlindAlignmentRelay>());
        Bind(guidance,"passage",Find<VS1ParallelFigureEvent>());
        Bind(guidance,"returnPassage",Find<VS1OmittedPassageEvent>());
        Bind(guidance,"recoveryOrigin",FindIncludingInactive<VS1RecallAnchor>());
        Bind(guidance,"returnRelay",FindIncludingInactive<RecombinationRelay>());
        Bind(Find<GameSession>(),"protocolGuidance",guidance);
        var campaign=new SerializedObject(Find<CampaignDirector>());campaign.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Candidate.v1";campaign.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Candidate authored from Walkthrough: same physical campaign, isolated save namespace, explicit passive contextual notes. Verify input, pause, context and layout before build.");
    }
}

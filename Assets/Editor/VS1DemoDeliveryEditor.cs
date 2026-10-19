using System;
using System.IO;
using System.Linq;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VS1DemoDeliveryEditor
{
    private const string ScenePath = "Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity";
    private const string BuildPath = "Builds/Demo-20260928-01/EchoProtocol-Demo-20260928-01.app";

    public static void AuthorScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Authoring requires Edit Mode.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var evidence = UnityEngine.Object.FindFirstObjectByType<VS1DenialEvidence>();
        var player = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
        var loop = UnityEngine.Object.FindFirstObjectByType<LoopController>();
        if (evidence == null || player == null || loop == null)
            throw new InvalidOperationException("VS1 D1 or Player is missing.");
        var thought = evidence.GetComponent<SD1SubtitleDisplay>();
        if (thought == null || evidence.GetComponent<VS1DemoEnding>() != null)
            throw new InvalidOperationException("D1 thought missing or ending already authored.");

        var ending = evidence.gameObject.AddComponent<VS1DemoEnding>();
        var serialized = new SerializedObject(ending);
        serialized.FindProperty("evidence").objectReferenceValue = evidence;
        serialized.FindProperty("thought").objectReferenceValue = thought;
        serialized.FindProperty("playerController").objectReferenceValue = player;
        serialized.FindProperty("loop").objectReferenceValue = loop;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("DEMO DELIVERY: D1 ending authored and saved with explicit references.");
    }

    public static void BuildMacOS()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Build requires Edit Mode.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var evidence = UnityEngine.Object.FindFirstObjectByType<VS1DenialEvidence>();
        var ending = evidence == null ? null : evidence.GetComponent<VS1DemoEnding>();
        if (ending == null || EditorBuildSettings.scenes.Count(s => s.enabled) != 1
            || EditorBuildSettings.scenes.First(s => s.enabled).path != ScenePath)
            throw new InvalidOperationException("Demo ending or build scene is missing.");
        Directory.CreateDirectory(Path.GetDirectoryName(BuildPath));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });
        var summary = report.summary;
        string details = $"Unity: {Application.unityVersion}\nScene: {scene.path}\n"
            + $"Result: {summary.result}\nErrors: {summary.totalErrors}\n"
            + $"Warnings: {summary.totalWarnings}\nBytes: {summary.totalSize}\n"
            + $"Duration: {summary.totalTime}\nPath: {summary.outputPath}\n";
        File.WriteAllText("Builds/Demo-20260928-01/build-report.txt", details);
        Debug.Log("DEMO DELIVERY BUILD\n" + details);
        if (summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("macOS build failed.");
    }
}

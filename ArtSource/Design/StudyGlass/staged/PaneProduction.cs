// Candidate authoring only; not yet copied to Assets or executed.
using System;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PaneProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Pane.unity";
    public const string RootName = "Pane / sealed study glass";
    const string MaterialPath = "Assets/_Game/Art/Pane/M_StudySealedGlass.mat";

    [MenuItem("Echo Protocol/Author study glass optics candidate")]
    public static void Author()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play before authoring Pane");
        var scene = EditorSceneManager.OpenScene(StudyProduction.ScenePath);
        if (scene.GetRootGameObjects().Any(g => g.name == RootName)) throw new Exception("Study already contains the Pane candidate");
        var pane = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Single(r => r.name == "Study fixed glazing");
        var source = pane.sharedMaterial;
        if (!source || source.shader.name != "Universal Render Pipeline/Lit" || source.GetFloat("_Surface") != 1f)
            throw new Exception("Expected the existing transparent URP Lit glass");
        if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath)) throw new Exception("Preserve and review the previous candidate material before reauthoring");
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not derive Pane");
        Directory.CreateDirectory("Assets/_Game/Art/Pane");
        AssetDatabase.Refresh();
        // First comparison changes direct optics only. A seal-film texture remains
        // a separate proposed variant until the existing lights prove insufficient.
        var material = new Material(source) { name = "M_StudySealedGlass" };
        material.SetFloat("_SpecularHighlights", 1f);
        material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
        material.SetFloat("_Smoothness", .78f);
        AssetDatabase.CreateAsset(material, MaterialPath);
        pane.sharedMaterial = material;
        new GameObject(RootName);
        var director = new SerializedObject(Object.FindFirstObjectByType<CampaignDirector>());
        director.FindProperty("checkpointKey").stringValue = "EP.CampaignCheckpoint.Pane.v1";
        director.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save Pane");
        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Pane optics candidate authored. Inspect both actual house branches before selecting; no geometry, source, power, Echo or gaze contract changed.");
    }
}

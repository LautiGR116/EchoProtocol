using System;
using System.Linq;
using EchoProtocol.Horror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VS1HubOrientationEditor
{
    private const string ScenePath = "Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity";

    public static void AuthorScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before authoring the Hub sign.");

        var scene = EditorSceneManager.OpenScene(ScenePath);
        var evidence = UnityEngine.Object.FindFirstObjectByType<VS1DenialEvidence>();
        if (evidence == null)
            throw new InvalidOperationException("D1 scene state is missing.");

        var presentation = new SerializedObject(evidence)
            .FindProperty("presentation").objectReferenceValue as GameObject;
        if (presentation == null)
            throw new InvalidOperationException("D1 presentation is missing.");

        var source = UnityEngine.Object.FindObjectsByType<TextMesh>(
            FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name == "P3 threshold identity" && t.gameObject.scene == scene);
        if (source == null)
            throw new InvalidOperationException("Existing physical label style is missing.");

        var existing = presentation.transform.Find("Hub service revision sign");
        var sign = existing == null
            ? UnityEngine.Object.Instantiate(source.gameObject, presentation.transform)
            : existing.gameObject;
        sign.name = "Hub service revision sign";
        sign.transform.SetPositionAndRotation(new Vector3(4.97f, 1.75f, -5.55f),
            Quaternion.Euler(0f, 90f, 0f));
        var label = sign.GetComponent<TextMesh>();
        label.text = "REVISION";
        label.characterSize = 0.10f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        if (existing == null)
            Undo.RegisterCreatedObjectUndo(sign, "Add post-R3 Hub revision sign");
        EditorSceneManager.SaveScene(scene);
        Debug.Log("HUB ORIENTATION: one physical revision sign saved under the post-R3 D1 presentation.");
    }
}

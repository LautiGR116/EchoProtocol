using System;
using System.Collections.Generic;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Optional successor. Adds two filters and explicit architecture references only.</summary>
public static class RoomsProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Rooms.unity";
    public const string RootName = "Rooms / domestic acoustic boundaries";

    // Exact existing Residence geometry names, never substring matching "wall"
    // against imported props. No new collider, material, mesh or sound is created.
    static readonly HashSet<string> PartitionNames = new HashSet<string>
    {
        "Entrance bridge side", "Entrance bridge floor", "Entrance bridge ceiling",
        "Entrance wall", "Entrance lintel", "Exterior wall", "Window parapet", "Window head",
        "Rear exterior", "Room division", "Domestic hall partition", "Doorway structural lintel",
        "Rear room wall", "Rear hall wall", "Exit wall", "Exit structural lintel",
        "Residence continuous timber", "Residence plaster ceiling",
        "Study closed lower door", "Study fixed glazing", "6B exit leaf",
        "Service connection wall", "Service connection floor", "Service connection ceiling"
    };

    static T One<T>() where T : Component => Object.FindObjectsByType<T>(FindObjectsSortMode.None)
        .Single(c => c.gameObject.scene == SceneManager.GetActiveScene());

    static void Ref(SerializedObject target, string field, Object value)
    {
        var property = target.FindProperty(field);
        if (property == null) throw new Exception("Missing explicit field: " + field);
        property.objectReferenceValue = value;
    }

    static AudioLowPassFilter AddFilter(AudioSource source)
    {
        var filter = source.gameObject.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = 22000f;
        filter.lowpassResonanceQ = 1f;
        filter.enabled = true;
        return filter;
    }

    [MenuItem("Echo Protocol/Author domestic room acoustics")]
    public static void Author()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play before authoring Rooms");
        var scene = EditorSceneManager.OpenScene(RecordsProduction.ScenePath);
        var roots = scene.GetRootGameObjects();
        if (roots.Any(g => g.name == RootName)) throw new Exception("Records already contains a Rooms root");
        var residence = roots.Single(g => g.name == "Residence / habitable reconstruction");
        var house = One<HouseMemory>();
        var houseFields = new SerializedObject(house);
        var telephone = houseFields.FindProperty("telephoneAudio").objectReferenceValue as AudioSource;
        var refrigerator = houseFields.FindProperty("kitchenApplianceAudio").objectReferenceValue as AudioSource;
        var view = houseFields.FindProperty("view").objectReferenceValue as Camera;
        var session = houseFields.FindProperty("session").objectReferenceValue as GameSession;
        if (!telephone || !refrigerator || telephone == refrigerator || !view || !session)
            throw new Exception("House source/listener/session references are incomplete");
        foreach (var source in new[] { telephone, refrigerator })
            if (source.GetComponents<AudioSource>().Length != 1 || source.GetComponent<AudioLowPassFilter>()
                || source.bypassEffects || source.spatialBlend < .999f)
                throw new Exception("Expected one physical source with no existing low-pass on " + source.name);

        var partitions = residence.GetComponentsInChildren<Collider>(true)
            .Where(c => c.enabled && c.gameObject.activeInHierarchy && !c.isTrigger && PartitionNames.Contains(c.name))
            .OrderBy(c => c.name, StringComparer.Ordinal).ThenBy(c => c.transform.position.x)
            .ThenBy(c => c.transform.position.z).ToArray();
        var missing = PartitionNames.Where(n => !partitions.Any(c => c.name == n)).ToArray();
        if (missing.Length != 0) throw new Exception("Domestic architecture changed; review missing partitions: " + string.Join(", ", missing));
        if (partitions.Any(c => !(c is BoxCollider))) throw new Exception("Review unexpected non-box domestic architecture before authoring");
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not derive Rooms scene");

        var root = new GameObject(RootName);
        var treatment = root.AddComponent<HouseRoomAcoustics>();
        var fields = new SerializedObject(treatment);
        Ref(fields, "session", session); Ref(fields, "listener", view.transform);
        Ref(fields, "telephoneSource", telephone); Ref(fields, "telephoneFilter", AddFilter(telephone));
        Ref(fields, "refrigeratorSource", refrigerator); Ref(fields, "refrigeratorFilter", AddFilter(refrigerator));
        var array = fields.FindProperty("architecturalPartitions"); array.arraySize = partitions.Length;
        for (int i = 0; i < partitions.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = partitions[i];
        fields.FindProperty("clearCutoffHz").floatValue = 22000f;
        fields.FindProperty("obstructedCutoffHz").floatValue = 1600f;
        fields.FindProperty("transitionSeconds").floatValue = .20f;
        fields.ApplyModifiedPropertiesWithoutUndo();

        var campaign = new SerializedObject(One<CampaignDirector>());
        campaign.FindProperty("checkpointKey").stringValue = "EP.CampaignCheckpoint.Rooms.v1";
        campaign.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save derived Rooms scene");
        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Rooms authored: two optional low-pass filters, " + partitions.Length
            + " existing architectural colliders, unchanged playback/gain/position. Compile, inspect and compare actual room approaches before acceptance; listening remains pending.");
    }
}

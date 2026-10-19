// Optional successor authored after the preserved Presence checkpoint.
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EchoProtocol.Campaign;
using Object=UnityEngine.Object;

public static class EncounterProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Encounter.unity";
    static TextMesh Label(string name)=>Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(t=>t.name==name);
    static void Fit(TextMesh label,string text,float width,float maximumSize)
    {
        label.text=text;label.characterSize=maximumSize;
        float actual=label.GetComponent<Renderer>().localBounds.size.x;
        if(actual>width)label.characterSize*=width/actual;
        EditorUtility.SetDirty(label);
    }
    [MenuItem("Echo Protocol/Author direct final encounter")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        var scene=EditorSceneManager.OpenScene(PresenceProduction.ScenePath);
        EditorSceneManager.SaveScene(scene,ScenePath);
        var review=Object.FindFirstObjectByType<FinalDoorMemory>();var rs=new SerializedObject(review);
        rs.FindProperty("requireDirectEncounter").boolValue=true;
        rs.FindProperty("directWitnessMaterial").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>(PresenceProduction.MaterialPath);
        rs.FindProperty("headWitnessHeight").floatValue=1.64f;
        var zone=(BoxCollider)rs.FindProperty("insideWitnessZone").objectReferenceValue;
        zone.transform.position=new Vector3(0,1,-78.05f);zone.transform.rotation=Quaternion.identity;zone.transform.localScale=Vector3.one;
        zone.center=Vector3.zero;zone.size=new Vector3(1.6f,2.2f,.90f);
        rs.ApplyModifiedPropertiesWithoutUndo();
        Fit(Label("Review origin text"),"6B / ORIGEN INTERIOR",.72f,.024f);
        Fit(Label("Interior revision instructions"),"REGISTRÁ TU LLEGADA\nVOLVÉ CON R\nABRÍ PARA RECIBIRTE",.78f,.024f);
        Fit(Label("Porter take status"),"REGISTRO DE SESIÓN",.62f,.0067f);
        Label("Porter take duration").text="6B / POSICIÓN Y ORIENTACIÓN";
        Label("Porter take action").text="";
        var director=Object.FindFirstObjectByType<CampaignDirector>();var ds=new SerializedObject(director);
        ds.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Encounter.v1";ds.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Encounter saved as an opt-in successor. Verify real door preparation, clear head/chest visibility, short valid take, rejected glass/origin views and human-facing composition before build.");
    }
}

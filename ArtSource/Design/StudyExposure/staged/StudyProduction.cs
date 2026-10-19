// Prepared successor. Not integrated or camera-selected yet.
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using Object=UnityEngine.Object;

public static class StudyProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Study.unity";
    public const string RootName="Study / natural return exposure";
    static Transform residence;
    static GameObject At(string name,float x,float z)
    {
        var items=residence.GetComponentsInChildren<Transform>(true).Where(t=>t.name==name&&Mathf.Abs(t.position.x-x)<.012f&&Mathf.Abs(t.position.z-z)<.012f).ToArray();
        if(items.Length!=1)throw new Exception("Expected one "+name+" at "+x+","+z+"; found "+items.Length);
        return items[0].gameObject;
    }
    static void MoveAndResize(GameObject go,float z,float length,int localAxis=2)
    {
        var p=go.transform.position;p.z=z;go.transform.position=p;
        var scale=go.transform.localScale;scale[localAxis]=length;go.transform.localScale=scale;
        var renderer=go.GetComponent<Renderer>();var mapping=go.GetComponent<SurfaceMapping>();
        if(renderer&&mapping)
        {
            var size=renderer.bounds.size;float metres=go.name=="Domestic hall partition"||go.name=="Doorway structural lintel"?1.6f:3f;
            mapping.Configure(renderer,new Vector2(Mathf.Max(size.x,size.z)/metres,size.y/metres));
        }
    }
    [MenuItem("Echo Protocol/Author natural study return")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring Study");
        var scene=EditorSceneManager.OpenScene(RoomsProduction.ScenePath);
        residence=scene.GetRootGameObjects().Single(g=>g.name=="Residence / habitable reconstruction").transform;
        if(scene.GetRootGameObjects().Any(g=>g.name==RootName))throw new Exception("Rooms unexpectedly already contains Study successor");
        // Resolve every piece before deriving. Keep component identities so the
        // explicit Rooms acoustic and House sight references track the actual wall.
        var wall=At("Domestic hall partition",1,-56.6f);
        var skirt=At("Domestic hall partition skirting",1,-56.6f);
        var cornice=At("Domestic hall partition cornice",1,-56.6f);
        var lintel=At("Doorway structural lintel",1,-58.8f);
        var northFrame=At("Painted door architrave",1,-57.872f);
        var frameHead=At("Painted door architrave head",1,-58.8f);
        var lower=At("Study closed lower door",1,-58.8f);
        var glass=At("Study fixed glazing",1,-58.8f);
        var house=residence.GetComponent<HouseMemory>();var fields=new SerializedObject(house);
        var body=(GameObject)fields.FindProperty("presenceRoot").objectReferenceValue;
        var safety=(BoxCollider)fields.FindProperty("presenceVisibilityVolume").objectReferenceValue;
        var source=(AudioSource)fields.FindProperty("presenceAudio").objectReferenceValue;
        if(!body||!safety||!source||!glass.GetComponent<BoxCollider>())throw new Exception("Study references incomplete");
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new Exception("Could not derive Study");
        new GameObject(RootName);
        MoveAndResize(wall,-56.1f,1.6f);MoveAndResize(skirt,-56.1f,1.63f);MoveAndResize(cornice,-56.1f,1.635f);
        MoveAndResize(lintel,-58.3f,2.8f);
        var framePosition=northFrame.transform.position;framePosition.z=-56.872f;northFrame.transform.position=framePosition;
        MoveAndResize(frameHead,-58.3f,2.95f,0);
        MoveAndResize(lower,-58.3f,2.75f);MoveAndResize(glass,-58.3f,2.77f);
        // The finite visibility volume must not spawn remote cast shadows.
        // This is only the environmental figure; the recorded Echo is untouched.
        foreach(var renderer in body.GetComponentsInChildren<MeshRenderer>(true))
            if(renderer.enabled)renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var bodyPosition=body.transform.position;bodyPosition.x=1.65f;body.transform.position=bodyPosition;
        var safetyPosition=safety.transform.position;safetyPosition.x=1.65f;safetyPosition.y=.97f;safety.transform.position=safetyPosition;
        var safetySize=safety.size;safetySize.y=2f;safety.size=safetySize;
        var sourcePosition=source.transform.position;sourcePosition.x=1.65f;source.transform.position=sourcePosition;
        var director=Object.FindFirstObjectByType<CampaignDirector>();var ds=new SerializedObject(director);
        ds.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Study.v1";ds.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new Exception("Could not save Study");
        AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Study exposure candidate saved. Geometry/actor framing remains a hypothesis until real straight-return camera and collision checks; no forced gaze, new exit gate, recorded channel or human acceptance.");
    }
}

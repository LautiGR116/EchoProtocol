using System;
using System.Linq;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Opt-in only. This command never opens, saves or replaces a scene.
public static class NegativePresentationSetup
{
    [MenuItem("Echo Protocol/El Negativo/Apply presentation to open scene")]
    public static void ApplyToOpenScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before applying El Negativo presentation.");
        Scene scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||!scene.isLoaded||!(scene.name=="EchoProtocol_Incursion"||scene.name=="EchoProtocol_Negative"))
            throw new InvalidOperationException("Open Incursion or EchoProtocol_Negative first. No scene is opened or saved by this command.");
        var session=All<GameSession>(scene).Single();
        var data=new SerializedObject(session);
        var ui=data.FindProperty("identity").objectReferenceValue as FolioUI;
        var hud=data.FindProperty("interactionFocusHud").objectReferenceValue as InteractionFocusHUD;
        if(!ui||!hud||!data.FindProperty("campaign").objectReferenceValue)
            throw new InvalidOperationException("El Negativo requires the existing Folio, interaction HUD and campaign references.");
        var source=new SerializedObject(hud).FindProperty("view").objectReferenceValue as Camera;
        if(!source)throw new InvalidOperationException("InteractionFocusHUD must reference the Player camera.");
        var camera=data.FindProperty("negativeMenuCamera").objectReferenceValue as Camera;
        if(!camera)camera=All<Camera>(scene).FirstOrDefault(c=>c.name=="NegativeMenuCamera");
        bool created=!camera;
        if(created)
        {
            var go=new GameObject("NegativeMenuCamera");
            SceneManager.MoveGameObjectToScene(go,scene);
            Undo.RegisterCreatedObjectUndo(go,"Create El Negativo title camera");
            camera=Undo.AddComponent<Camera>(go);
            camera.CopyFrom(source);
            camera.transform.SetPositionAndRotation(new Vector3(0,2.4f,-29.4f),Quaternion.LookRotation(new Vector3(1.8f,3.2f,-38)-new Vector3(0,2.4f,-29.4f)));
            camera.fieldOfView=62;
            camera.depth=source.depth+10;
            camera.targetTexture=null;
            var extra=Undo.AddComponent<UniversalAdditionalCameraData>(go);
            var sourceExtra=source.GetComponent<UniversalAdditionalCameraData>();
            if(sourceExtra)
            {
                extra.renderPostProcessing=sourceExtra.renderPostProcessing;
                extra.volumeLayerMask=sourceExtra.volumeLayerMask;
                extra.antialiasing=sourceExtra.antialiasing;
                extra.antialiasingQuality=sourceExtra.antialiasingQuality;
            }
            extra.volumeTrigger=camera.transform;
        }
        Undo.RecordObject(session,"Enable El Negativo presentation");
        data.FindProperty("negativePresentation").boolValue=true;
        data.FindProperty("negativeMenuCamera").objectReferenceValue=camera;
        data.FindProperty("editionLabel").stringValue="EL NEGATIVO";
        bool changed=data.ApplyModifiedProperties();
        if(camera.enabled){Undo.RecordObject(camera,"Disable edit-time title camera");camera.enabled=false;changed=true;}
        if(created||changed)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(session);
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log("El Negativo presentation enabled. NegativeMenuCamera is independent of Player/audio; adjust its composition if needed. Existing camera transforms are preserved. Save manually after review.",session);
    }

    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T:Component
        =>scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<T>(true));
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using Object=UnityEngine.Object;

public static class PresenceProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Presence.unity";
    public const string MaterialPath="Assets/_Game/Art/Presence/M_RecordedPresence.mat";
    public const string DepthMaterialPath="Assets/_Game/Art/Presence/M_RecordedSelfDepth.mat";
    public const string PrefabPath="Assets/_Game/Prefabs/Presence/EchoPresence.prefab";
    static void Ref(Object target,string field,Object value){var so=new SerializedObject(target);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}

    [MenuItem("Echo Protocol/Author recorded presence presentation")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        var shader=Shader.Find("EchoProtocol/RecordedPresence");if(!shader)throw new Exception("RecordedPresence shader must be imported first");
        Directory.CreateDirectory("Assets/_Game/Art/Presence");Directory.CreateDirectory("Assets/_Game/Prefabs/Presence");AssetDatabase.Refresh();
        var scene=EditorSceneManager.OpenScene(ReserveProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,MaterialPath);}else material.shader=shader;
        material.SetColor("_Color",new Color(.14f,.68f,.64f,1));material.SetFloat("_BodyOpacity",.90f);material.SetFloat("_EdgeOpacity",.03f);material.SetFloat("_WorldLightGain",.65f);material.SetFloat("_RecordLevel",.32f);material.SetFloat("_NearFadeStart",.35f);material.SetFloat("_NearFadeEnd",.95f);material.renderQueue=3000;EditorUtility.SetDirty(material);
        var depthShader=Shader.Find("EchoProtocol/RecordedSelfDepth");if(!depthShader)throw new Exception("RecordedSelfDepth shader must be imported first");
        var depth=AssetDatabase.LoadAssetAtPath<Material>(DepthMaterialPath);
        if(!depth){depth=new Material(depthShader);AssetDatabase.CreateAsset(depth,DepthMaterialPath);}else depth.shader=depthShader;
        depth.SetFloat("_NearFadeStart",.35f);depth.SetFloat("_NearFadeEnd",.95f);depth.renderQueue=2990;EditorUtility.SetDirty(depth);
        var loop=Object.FindFirstObjectByType<LoopController>();var ls=new SerializedObject(loop);var original=(EchoPlayback)ls.FindProperty("echoPrefab").objectReferenceValue;
        var body=(GameObject)PrefabUtility.InstantiatePrefab(original.gameObject);body.name="EchoPresence";
        int changed=0;
        foreach(var renderer in body.GetComponentsInChildren<Renderer>(true))
        {
            if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
            renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=true;changed++;
            var filter=renderer.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)throw new Exception("Recorded self depth requires the existing rigid MeshFilter");
            var proxy=new GameObject("Recorded self depth");proxy.layer=renderer.gameObject.layer;proxy.transform.SetParent(renderer.transform,false);
            proxy.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
            var depthRenderer=proxy.AddComponent<MeshRenderer>();depthRenderer.sharedMaterials=renderer.sharedMaterials.Select(_=>depth).ToArray();depthRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;depthRenderer.receiveShadows=false;
        }
        if(changed==0){Object.DestroyImmediate(body);throw new Exception("No enabled recorded-body renderer found");}
        var prefab=PrefabUtility.SaveAsPrefabAsset(body,PrefabPath);Object.DestroyImmediate(body);Ref(loop,"echoPrefab",prefab.GetComponent<EchoPlayback>());
        var director=Object.FindFirstObjectByType<CampaignDirector>();var cs=new SerializedObject(director);cs.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Presence.v1";cs.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Presence candidate saved: "+changed+" recorded-body renderers; unchanged prefab geometry, pose playback and recording channels. Compare actual game cameras before accepting.");
    }
}

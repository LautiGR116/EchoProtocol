using System;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class WalkthroughFinishProduction
{
 static Transform Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(t=>t.name==n);
 public static void MoveLampAside()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before scene changes");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.path!=SE2WalkthroughProduction.ScenePath)throw new InvalidOperationException("Open Walkthrough first");
  var lamp=Named("SE2 original / FloorLamp");var light=Named("SE2 actual standing lamp light");
  var delta=new Vector3(5.90f,0,30.27f)-lamp.position;lamp.position+=delta;light.position+=delta;
  Named("SE2 permanent floor lamp contact").position=new Vector3(5.90f,.70f,30.27f);
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  Debug.Log("Walkthrough lamp moved beside the chair, with its socket ending at north wall z31.17; same light/gain and fixed collision.");
 }

 static Material Mat(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/PressurePlant/M_"+n+".mat");
 static GameObject Model(string n,Transform parent,Vector3 position,float yaw)
 {
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/SE2Service/EP_SE2_"+n+".fbx");
  var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name="SE2 original service / "+n;g.transform.SetParent(parent,false);
  var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0)*g.transform.localRotation);
  foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Mat(m.name)??throw new Exception("Missing material "+m.name)).ToArray();return g;
 }
 static GameObject Box(Transform p,string n,Vector3 point,Vector3 size,Material m)
 {
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.position=point;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
 }
 static void Practical(Transform parent,string n,Vector3 point,float gain)
 {
  Box(parent,n+" housing",new Vector3(point.x,2.86f,point.z),new Vector3(.88f,.065f,.22f),Named("Service fixture backing").GetComponent<Renderer>().sharedMaterial);
  var lens=Box(parent,n+" diffuser",new Vector3(point.x,2.821f,point.z),new Vector3(.79f,.012f,.14f),Named("Service fixture opal").GetComponent<Renderer>().sharedMaterial);
  var g=new GameObject(n+" source");g.transform.SetParent(parent,false);g.transform.position=point;
  var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(.72f,.80f,.77f);light.intensity=gain;light.range=4.2f;light.shadows=LightShadows.Soft;light.shadowBias=.015f;light.shadowNormalBias=.04f;
  var budget=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<EchoProtocol.Presentation.LightShadowBudget>());var list=budget.FindProperty("lights");int k=list.arraySize;list.arraySize++;list.GetArrayElementAtIndex(k).objectReferenceValue=light;budget.ApplyModifiedPropertiesWithoutUndo();
 }
 public static void RefineFirstViews()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.path!=SE2WalkthroughProduction.ScenePath)throw new InvalidOperationException("Open Walkthrough");
  var shared=Named("SE2 permanent shell, collision and sources");
  if(shared.Find("SE2 south refuge practical source"))throw new InvalidOperationException("Refinement already authored");
  var cold=Named("SE2 Cold Empty Service Chamber");
  var leaf=Named("SE2 original / DomesticDoor").GetComponentsInChildren<Transform>(true).Single(t=>t.name=="DoorLeafPivot");leaf.RotateAround(leaf.position,Vector3.up,-26f);
  var e=UnityEngine.Object.FindFirstObjectByType<EchoProtocol.Horror.VS1ParallelFigureEvent>();var es=new SerializedObject(e);
  var figure=((GameObject)es.FindProperty("figureRoot").objectReferenceValue).transform;figure.position+=new Vector3(0,0,.30f);
  var target=new GameObject("SE2 actual figure centre for optional observation").transform;target.SetParent(figure,false);target.localPosition=new Vector3(0,1.25f,0);es.FindProperty("lookTarget").objectReferenceValue=target;es.ApplyModifiedPropertiesWithoutUndo();
  Practical(shared,"SE2 south refuge practical",new Vector3(3.02f,2.64f,25f),1.4f);
  Practical(shared,"SE2 north return practical",new Vector3(5f,2.64f,32.7f),1.2f);
  Model("ServiceCase",cold,new Vector3(5.18f,0,30.40f),90);Model("ServiceTable",cold,new Vector3(6.50f,0,27.33f),0);
  Named("SE2 cold cabinet matches chair contact").gameObject.SetActive(false);Named("SE2 cold service table matches console").gameObject.SetActive(false);
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  Debug.Log("Walkthrough first views refined: two fixed practicals, visible58-degree rear leaf, figure +.30Z, original service case/table replacing provisional cubes. Review cameras and collision required.");
 }

 public static void GuardVisibleShadowsAndSeparateFigure()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.path!=SE2WalkthroughProduction.ScenePath)throw new InvalidOperationException("Open Walkthrough");
  var e=UnityEngine.Object.FindFirstObjectByType<EchoProtocol.Horror.VS1ParallelFigureEvent>();var so=new SerializedObject(e);
  if(so.FindProperty("secondaryChangeEnvelopes").arraySize!=0)throw new InvalidOperationException("Shadow receiver guard already authored");
  var g=new GameObject("SE2 changing shadow receivers in approach");g.transform.SetParent(Named(SE2WalkthroughProduction.RootName),false);g.transform.position=new Vector3(1.325f,1.6f,29.1f);
  var volume=g.AddComponent<BoxCollider>();volume.isTrigger=true;volume.size=new Vector3(5.55f,3.2f,2.2f);
  var envelopes=so.FindProperty("secondaryChangeEnvelopes");envelopes.arraySize=1;envelopes.GetArrayElementAtIndex(0).objectReferenceValue=volume;so.ApplyModifiedPropertiesWithoutUndo();
  // The existing ceiling practical supplies a low back wall wash. It stays fixed
  // and active in both states; no extra light or authored gain change.
  var light=Named("SE2 rear room low practical").GetComponent<Light>();light.transform.LookAt(new Vector3(11.94f,1.35f,29.1f));light.spotAngle=110;light.innerSpotAngle=77;
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  Debug.Log("SE2 hidden change now also guards the complete approach shadow receivers. Existing rear practical aims at the actual rear wall to separate the body silhouette. Inspect before acceptance.");
 }
}

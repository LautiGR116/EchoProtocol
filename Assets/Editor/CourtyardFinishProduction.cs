using System;
using System.Linq;
using System.IO;
using EchoProtocol.Presentation;
using EchoProtocol.Audio;
using EchoProtocol.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class CourtyardFinishProduction
{
 const string Evidence="Docs/VisualEvidence/2026-10-01-Courtyard";
 static Transform Root=>GameObject.Find(HouseCourtyardProduction.RootName).transform;
 public static void LightExterior()
 {
  if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=HouseCourtyardProduction.ScenePath || GameObject.Find("Courtyard / indirect night light"))throw new Exception("Fresh Courtyard edit scene required");
  var root=new GameObject("Courtyard / indirect night light").transform;root.SetParent(Root,false);
  var west=Source(root,"Courtyard western sky contribution",new Vector3(-6.75f,3.8f,-54.3f),new Color(.54f,.65f,.76f),4.5f,12);
  var south=Source(root,"Courtyard southern sky contribution",new Vector3(-2.8f,3.4f,-63.4f),new Color(.60f,.68f,.76f),3.5f,8);
  var budget=new SerializedObject(Object.FindFirstObjectByType<LightShadowBudget>());
  foreach(string field in new[]{"lights","residenceLights"}){var a=budget.FindProperty(field);int i=a.arraySize;a.arraySize+=2;a.GetArrayElementAtIndex(i).objectReferenceValue=west;a.GetArrayElementAtIndex(i+1).objectReferenceValue=south;}budget.ApplyModifiedPropertiesWithoutUndo();
  Save();File.AppendAllText(Evidence+"/refinements.txt","Camera-driven first-layout correction: two local cool sky contributions, west1.4/range12 and south.9/range8, real punctual shadows and residence shadow budget. No global ambient, house supply or motor-source changes. Initial black forward views retained under first-layout.\n");
 }

 public static void IntegrateAir()
 {
  if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=HouseCourtyardProduction.ScenePath || Object.FindFirstObjectByType<CourtyardAir>())throw new Exception("Unconfigured Courtyard edit scene required");
  string path="Assets/_Game/Audio/Clips/Courtyard/AMB_CourtyardNight_LP.wav";
  var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=false;importer.SaveAndReimport();
  var acoustics=new SerializedObject(Object.FindFirstObjectByType<HouseRoomAcoustics>());
  var originalSource=(AudioSource)acoustics.FindProperty("refrigeratorSource").objectReferenceValue;
  var root=new GameObject("Courtyard / outside air");root.transform.SetParent(Root);root.transform.position=new Vector3(-7,3.4f,-63.6f);
  var source=root.AddComponent<AudioSource>();source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);source.loop=true;source.playOnAwake=false;source.spatialBlend=1;source.dopplerLevel=0;source.spread=75;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=3;source.maxDistance=16;source.volume=.16f;source.outputAudioMixerGroup=originalSource.outputAudioMixerGroup;
  var south=Region("Courtyard outside air south",new Vector3(-3.7f,1.4f,-63.55f),new Vector3(9.4f,3.2f,2.1f));
  var west=Region("Courtyard outside air west",new Vector3(-6.9f,1.4f,-57.55f),new Vector3(3,3.2f,14.1f));
  var air=new SerializedObject(root.AddComponent<CourtyardAir>());
  air.FindProperty("session").objectReferenceValue=Object.FindFirstObjectByType<GameSession>();
  air.FindProperty("listener").objectReferenceValue=Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>().transform;
  air.FindProperty("exteriorSource").objectReferenceValue=source;
  var regions=air.FindProperty("exteriorRegions");regions.arraySize=2;regions.GetArrayElementAtIndex(0).objectReferenceValue=south;regions.GetArrayElementAtIndex(1).objectReferenceValue=west;
  var old=acoustics.FindProperty("architecturalPartitions");var parts=air.FindProperty("architecturalPartitions");parts.arraySize=old.arraySize;for(int i=0;i<old.arraySize;i++)parts.GetArrayElementAtIndex(i).objectReferenceValue=old.GetArrayElementAtIndex(i).objectReferenceValue;
  air.ApplyModifiedPropertiesWithoutUndo();Save();
  File.AppendAllText(Evidence+"/refinements.txt","One provisional present-tense exterior source integrated: ahriik481964 CC0 preview, traced+18dB/1s-loop overlap, fixed3D source.16, same Ambience mixer, two outside regions and inherited52 real architectural partitions; pause/listener processing retained. No household/Echo source ownership changes. Listening EN PRUEBA.\n");
 }
 static BoxCollider Region(string name,Vector3 p,Vector3 size){var g=new GameObject(name);g.transform.SetParent(Root);g.transform.position=p;var c=g.AddComponent<BoxCollider>();c.isTrigger=true;c.size=size;return c;}

 static Light Source(Transform parent,string name,Vector3 position,Color color,float level,float range)
 {var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=position;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=level;l.range=range;l.shadows=LightShadows.Soft;l.shadowBias=.03f;l.shadowNormalBias=.1f;return l;}
 static void Save(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
}

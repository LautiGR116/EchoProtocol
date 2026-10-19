using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class CourtyardRainwaterProduction
{
 const string Art="Assets/_Game/Art/Courtyard";
 static Transform root,sources;static Material mineral,steel;static readonly List<GameObject> tiles=new List<GameObject>(),metals=new List<GameObject>();
 public static void Integrate()
 {
  if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=HouseCourtyardProduction.ScenePath || GameObject.Find("Courtyard / rainwater and paving"))throw new Exception("Unconfigured Courtyard edit scene required");
  int collisions=Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
  foreach(string part in new[]{"Downpipe27","GutterStraight1m","GutterOutlet1m","MineralTile0744","Drain1m"})if(!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Courtyard/EP_CY_"+part+".fbx"))throw new Exception("Missing original kit "+part);
  root=new GameObject("Courtyard / rainwater and paving").transform;root.SetParent(GameObject.Find(HouseCourtyardProduction.RootName).transform,false);
  sources=new GameObject("Editable source placements / combined below").transform;sources.SetParent(root,false);tiles.Clear();metals.Clear();
  mineral=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Overhaul/M_Plaster.mat")){name="M_CourtyardMineralPaving"};mineral.SetColor("_BaseColor",new Color(.56f,.58f,.54f));mineral.SetFloat("_Smoothness",.19f);AssetDatabase.CreateAsset(mineral,Art+"/M_MineralPaving.mat");
  steel=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Registration/M_OfficeSteel.mat");
  // The physics floor stays continuous. A recessed visual bedding plane reveals real joints/slots.
  foreach(string n in new[]{"Courtyard south ground","Courtyard west ground"}){var old=GameObject.Find(n);old.GetComponent<Renderer>().enabled=false;var g=Object.Instantiate(old,root);g.name=n+" / recessed bedding";g.GetComponent<Renderer>().enabled=true;g.transform.position+=Vector3.down*.048f;}
  GameObject.Find("Courtyard drain beside facade").SetActive(false);
  // Full western paving, cut accurately on either side of the real drainage channel.
  float[] xs={-8.025f,-7.275f,-6.525f,-6.0265f,-5.5845f};float[] widths={.744f,.744f,.744f,.245f,.363f};
  for(int row=0;row<16;row++)for(int col=0;col<xs.Length;col++)Tile(new Vector3(xs[col],.002f,-62.125f+row*.75f),widths[col],.744f);
  // The southern strip uses narrow cut courses; no large slab crosses the turn or door threshold.
  float southWidth=9.4f/13;
  for(int row=0;row<3;row++)for(int col=0;col<13;col++)Tile(new Vector3(-8.4f+southWidth*(col+.5f),.002f,-64.6f+.7f*(row+.5f)),southWidth-.006f,.694f);
  for(int i=0;i<12;i++)metals.Add(Place("Drain1m",new Vector3(-5.835f,.004f,-62+i),270));
  var pipe=Place("Downpipe27",new Vector3(-5.535f,0,-60),270);metals.Add(pipe);
  GameObject outlet=null;
  for(int i=0;i<12;i++){float z=-62+i;var g=Place(z==-60?"GutterOutlet1m":"GutterStraight1m",new Vector3(-5.535f,2.7f,z),270);metals.Add(g);if(z==-60)outlet=g;}
  var inlet=Anchor(pipe,"Downpipe27__INLET");var bottom=Anchor(outlet,"GutterOutlet1m__BOTTOM");
  if(Vector3.Distance(inlet,bottom)>.002f || Vector3.Distance(inlet,new Vector3(-5.645f,2.7f,-60))>.002f)throw new Exception("Rainwater importer basis/connection incorrect: "+inlet+" / "+bottom);
  // Closed visible ends, flashing and damp-resistant wall footing belong to the real envelope.
  foreach(float z in new[]{-62.503f,-50.497f})Box("Gutter folded end cap",new Vector3(-5.645f,2.738f,z),new Vector3(.145f,.077f,.006f),steel);
  Box("Residence continuous roof flashing",new Vector3(-5.405f,2.783f,-56.5f),new Vector3(.36f,.035f,12),steel);
  var baseMat=new Material(AssetDatabase.LoadAssetAtPath<Material>(Art+"/M_ExteriorPlaster.mat")){name="M_CourtyardFooting"};baseMat.SetColor("_BaseColor",new Color(.23f,.26f,.25f));AssetDatabase.CreateAsset(baseMat,Art+"/M_Footing.mat");
  Box("Residence mineral exterior footing",new Vector3(-5.549f,.105f,-56.5f),new Vector3(.036f,.21f,12),baseMat);
  Box("Courtyard boundary mineral footing",new Vector3(-8.287f,.105f,-59.75f),new Vector3(.036f,.21f,9.7f),baseMat);
  Box("Courtyard south mineral footing",new Vector3(-3.7f,.105f,-64.487f),new Vector3(9.4f,.21f,.036f),baseMat);
  int tileCount=tiles.Count;
  Combine("Courtyard bevelled mineral paving",tiles,mineral,true);Combine("Courtyard connected rainwater metal",metals,steel,false);sources.gameObject.SetActive(false);
  if(Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=collisions)throw new Exception("Visual kit changed collision components");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
  File.AppendAllText("Docs/VisualEvidence/2026-10-01-Courtyard/refinements.txt","Original Blender kit: "+tileCount+" bevelled cut pavers and connected gutter/downpipe/drain in two combined meshes, source instances retained inactive; original physics floor unchanged. Imported pipe/gutter anchor match <2mm; true slots over recessed visual bedding. House reserve/door/Echo rules untouched.\n");
 }
 static GameObject Place(string part,Vector3 p,float yaw)
 {var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Courtyard/EP_CY_"+part+".fbx");var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Courtyard kit / "+part;g.transform.SetParent(sources);var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterial=part=="MineralTile0744"?mineral:steel;return g;}
 static Vector3 Anchor(GameObject g,string n)=>g.GetComponentsInChildren<Transform>().Single(t=>t.name==n).position;
 static void Tile(Vector3 p,float width,float depth){var g=Place("MineralTile0744",p,0);var pivot=new GameObject("Paving cut").transform;pivot.SetParent(sources,false);pivot.position=p;g.transform.SetParent(pivot,true);pivot.localScale=new Vector3(width/.744f,1,depth/.744f);tiles.Add(g);}
 static void Box(string n,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m;}
 static void Combine(string name,List<GameObject> items,Material material,bool planar)
 {
  var list=new List<CombineInstance>();
  foreach(var g in items)foreach(var f in g.GetComponentsInChildren<MeshFilter>())
  {var matrix=root.worldToLocalMatrix*f.transform.localToWorldMatrix;list.Add(new CombineInstance{mesh=f.sharedMesh,transform=matrix});}
  var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(list.ToArray(),true,true);
  if(planar){var vertices=mesh.vertices;mesh.uv=vertices.Select(v=>new Vector2(v.x/2.5f,v.z/2.5f)).ToArray();mesh.RecalculateTangents();}
  var positions=mesh.vertices;var normals=mesh.normals;var tris=mesh.triangles;
  for(int i=0;i<tris.Length;i+=3)if(Vector3.Dot(Vector3.Cross(positions[tris[i+1]]-positions[tris[i]],positions[tris[i+2]]-positions[tris[i]]),normals[tris[i]]+normals[tris[i+1]]+normals[tris[i+2]])<0)throw new Exception("Combined mesh winding opposes outward normals: "+name);
  mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Art+"/"+name.Replace(' ','_')+".asset");
  var result=new GameObject(name);result.transform.SetParent(root,false);result.AddComponent<MeshFilter>().sharedMesh=mesh;result.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
}

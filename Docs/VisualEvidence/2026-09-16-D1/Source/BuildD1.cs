using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using EchoProtocol.Echo;using EchoProtocol.Horror;using System.Linq;
internal class CommandScript:IRunCommand{
 ExecutionResult r;Transform root,pres;Material trim;
 GameObject F(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.gameObject.scene==SceneManager.GetActiveScene()&&t.name==n).gameObject;
 GameObject G(string n,Transform p){var g=new GameObject(n);g.transform.SetParent(p,false);r.RegisterObjectCreation(g);return g;}
 GameObject B(string n,Vector3 pos,Vector3 scale,Material mat,bool solid){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(pres,false);g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)r.DestroyObject(g.GetComponent<Collider>());r.RegisterObjectCreation(g);return g;}
 BoxCollider Zone(string n,Vector3 pos,Vector3 size){var g=G(n,root);g.transform.position=pos;var c=g.AddComponent<BoxCollider>();c.isTrigger=true;c.size=size;return c;}
 TextMesh Label(string n,string text,Vector3 pos,float yaw,float size){var g=Object.Instantiate(F("P3 threshold identity"),pres);g.name=n;g.transform.position=pos;g.transform.rotation=Quaternion.Euler(0,yaw,0);var t=g.GetComponent<TextMesh>();t.text=text;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;r.RegisterObjectCreation(g);return t;}
 void Ref(Object o,string name,Object value){var so=new SerializedObject(o);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
 public void Execute(ExecutionResult result){r=result;if(EditorApplication.isPlaying||GameObject.Find("VS1 D1 Recorded Evidence")!=null)throw new System.Exception("Unsafe or already authored");root=G("VS1 D1 Recorded Evidence",null).transform;pres=G("D1 presentation after Hub arrival",root).transform;
 trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/M_VS1_VT1_Trim.mat");
 B("D1 service origin station",new Vector3(.22f,1.25f,-2.25f),new Vector3(.22f,1.1f,1.2f),trim,true);
 B("D1 record observation panel",new Vector3(-.22f,1.12f,-1),new Vector3(.22f,.5f,1.2f),trim,false);
 var station=Label("D1 origin instructions","REVISION\n[E] Fijar origen",new Vector3(.34f,1.55f,-2.25f),270,.012f);
 var recording=Label("D1 observation instructions","OBSERVACION",new Vector3(-.34f,1.13f,-1),90,.009f);
 var recall=G("D1 committed service origin",root).transform;recall.position=new Vector3(.9f,.03f,-1);recall.rotation=Quaternion.Euler(0,270,0);
 var facing=G("D1 recorded body facing",root).transform;facing.rotation=Quaternion.Euler(0,90,0);
 var pause=Zone("D1 real recorded pause zone",new Vector3(-1,1,-1),new Vector3(1.1f,2.5f,1.7f));
 var observer=Zone("D1 witness service zone",new Vector3(1.2f,1,-1),new Vector3(1.8f,2.5f,2.4f));
 var panel=F("Cold Ceiling Panel Hub 1").GetComponent<Renderer>().sharedMaterial;
 foreach(float x in new[]{-1.2f,1.2f}){B("D1 steady inspection housing",new Vector3(x,3.1f,-1),new Vector3(.7f,.1f,1.3f),trim,false);B("D1 steady inspection diffuser",new Vector3(x,3.03f,-1),new Vector3(.55f,.025f,1.1f),panel,false);var g=G("D1 steady local inspection light",pres);g.transform.position=new Vector3(x,2.7f,-1);var light=g.AddComponent<Light>();light.type=LightType.Point;light.range=6;light.intensity=1.25f;light.color=new Color(.65f,.8f,1);light.shadows=LightShadows.None;}
 var thought=root.gameObject.AddComponent<SD1SubtitleDisplay>();var d=root.gameObject.AddComponent<VS1DenialEvidence>();
 Ref(d,"hubReturn",Object.FindFirstObjectByType<VS1HubReturnGate>());Ref(d,"arrivalInterior",F("R3 arrival clear of rear").GetComponent<BoxCollider>());Ref(d,"arrivalClear",F("R3 Player fully inside original Hub").GetComponent<BoxCollider>());
 var player=F("Player");Ref(d,"player",player.GetComponent<LoopActor>());Ref(d,"playerBody",player.GetComponent<CharacterController>());Ref(d,"viewCamera",player.GetComponentInChildren<Camera>());Ref(d,"loop",F("Temporal Loop VS1 Foundation").GetComponent<LoopController>());Ref(d,"recorder",player.GetComponent<EchoRecorder>());Ref(d,"observationGlass",F("Observation Glass 1").GetComponent<Collider>());
 Ref(d,"presentation",pres.gameObject);Ref(d,"recallPoint",recall);Ref(d,"pauseZone",pause);Ref(d,"observerZone",observer);Ref(d,"pauseFacing",facing);Ref(d,"stationLabel",station);Ref(d,"recordingLabel",recording);Ref(d,"thought",thought);
 var so=new SerializedObject(d);var beds=so.FindProperty("ambienceBeds");beds.arraySize=2;beds.GetArrayElementAtIndex(0).objectReferenceValue=F("Fluorescent Bed").GetComponent<AudioSource>();beds.GetArrayElementAtIndex(1).objectReferenceValue=F("Distant Machinery Bed").GetComponent<AudioSource>();so.ApplyModifiedPropertiesWithoutUndo();pres.gameObject.SetActive(false);
 EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());r.Log("D1 authored and saved; existing route unchanged; new station only appears after actual R3 Hub arrival.");
 }}

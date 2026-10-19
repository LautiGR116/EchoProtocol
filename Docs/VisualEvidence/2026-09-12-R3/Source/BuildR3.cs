using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using EchoProtocol.Echo;using EchoProtocol.Horror;using EchoProtocol.Puzzle;using System.Linq;
internal class CommandScript:IRunCommand{
 ExecutionResult r;Transform root;Material wall,floor,trim,glass,panel;
 GameObject F(string n)=>Resources.FindObjectsOfTypeAll<Transform>().First(t=>t.gameObject.scene==SceneManager.GetActiveScene()&&t.name==n).gameObject;
 GameObject G(string n,Transform p=null){var g=new GameObject(n);g.transform.SetParent(p??root,false);r.RegisterObjectCreation(g);return g;}
 GameObject B(string n,Vector3 p,Vector3 s,Material m,bool solid=true,Transform parent=null){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent??root,false);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)r.DestroyObject(g.GetComponent<Collider>());r.RegisterObjectCreation(g);return g;}
 void Ref(Object o,string n,Object v){var so=new SerializedObject(o);so.FindProperty(n).objectReferenceValue=v;so.ApplyModifiedPropertiesWithoutUndo();}
 SlidingDoor Door(string n,float x,float z,float width){var g=G(n);g.transform.position=new Vector3(x,0,z);g.transform.rotation=Quaternion.Euler(0,90,0);var b=B(n+" solid leaf",new Vector3(x,1.45f,z),new Vector3(.22f,2.9f,width),trim,true);b.transform.SetParent(g.transform,true);var rb=b.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;var d=g.AddComponent<SlidingDoor>();Ref(d,"doorBody",rb);var so=new SerializedObject(d);so.FindProperty("closedLocalPosition").vector3Value=new Vector3(0,1.45f,0);so.FindProperty("openLocalPosition").vector3Value=new Vector3(0,4.5f,0);so.FindProperty("moveSpeed").floatValue=3;so.ApplyModifiedPropertiesWithoutUndo();B(n+" header",new Vector3(x,3.08f,z),new Vector3(.4f,.44f,width+.2f),trim);return d;}
 BoxCollider Volume(string n,Vector3 p,Vector3 s){var g=G(n);g.transform.position=p;var c=g.AddComponent<BoxCollider>();c.size=s;c.isTrigger=true;return c;}
 void Fixture(float x,float z,Transform p=null,float intensity=1.1f){B("R3 clinical housing",new Vector3(x,3.1f,z),new Vector3(.7f,.1f,1.3f),trim,false,p);B("R3 cool diffuser",new Vector3(x,3.03f,z),new Vector3(.55f,.025f,1.1f),panel,false,p);var g=G("R3 local cool fill",p);g.transform.position=new Vector3(x,2.75f,z);var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=7;l.intensity=intensity;l.color=new Color(.65f,.8f,1);l.shadows=LightShadows.None;}
 void Label(string n,string text,Vector3 p,float yaw,float size){var template=F("P3 threshold identity");var g=Object.Instantiate(template,root);g.name=n;g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,yaw,0);var t=g.GetComponent<TextMesh>();t.text=text;t.characterSize=size;r.RegisterObjectCreation(g);}
 public void Execute(ExecutionResult result){r=result;if(GameObject.Find("VS1 R3 Hub Return")!=null)throw new System.Exception("Already built");root=new GameObject("VS1 R3 Hub Return").transform;r.RegisterObjectCreation(root.gameObject);
 wall=F("P3 lanes ceiling").GetComponent<Renderer>().sharedMaterial;floor=F("Hub Floor").GetComponent<Renderer>().sharedMaterial;trim=F("P3 opaque separation").GetComponent<Renderer>().sharedMaterial;glass=F("P3 distant hub sealed glazing").GetComponent<Renderer>().sharedMaterial;panel=F("Cold Ceiling Panel Hub 1").GetComponent<Renderer>().sharedMaterial;
 var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/M_VS1_VT1_Trim.mat");trim=dark;
 // Only two old end walls become apertures; keep both originals for inspection.
 foreach(var n in new[]{"P3 long return west end","Hub West Wall"}){var g=F(n);r.RegisterObjectModification(g);g.SetActive(false);}
 // West perimeter spine, clear x=-19.8..-17.3, z=-10.7..48.1.
 B("R3 spine floor",new Vector3(-18.6f,-.1f,18.7f),new Vector3(2.8f,.2f,59),floor);
 B("R3 spine ceiling",new Vector3(-18.6f,3.3f,18.7f),new Vector3(3,.2f,59.2f),wall);
 B("R3 spine west wall",new Vector3(-20,1.6f,18.7f),new Vector3(.2f,3.2f,59.2f),wall);
 B("R3 spine east wall",new Vector3(-17.2f,1.6f,17.6f),new Vector3(.2f,3.2f,51.6f),wall);
 B("R3 north end",new Vector3(-18.6f,1.6f,48.2f),new Vector3(3,3.2f,.2f),wall);
 B("R3 south end",new Vector3(-18.6f,1.6f,-10.8f),new Vector3(3,3.2f,.2f),wall);
 B("R3 P3 exit south pier",new Vector3(-17.2f,1.6f,43.85f),new Vector3(.2f,3.2f,1.7f),wall);
 var exit=Door("R3 permanently unlocked exit",-17.2f,46.4f,3.4f);
 // Transverse return and automatic one-way vestibule (-9..-5.1).
 B("R3 arrival floor",new Vector3(-11.1f,-.1f,-9.5f),new Vector3(12.2f,.2f,2.6f),floor);
 B("R3 arrival ceiling",new Vector3(-11.1f,3.3f,-9.5f),new Vector3(12.4f,.2f,2.8f),wall);
 B("R3 arrival north wall",new Vector3(-11.15f,1.6f,-8.2f),new Vector3(12.3f,3.2f,.2f),wall);
 B("R3 arrival south wall",new Vector3(-11.15f,1.6f,-10.8f),new Vector3(12.3f,3.2f,.2f),wall);
 var rear=Door("R3 arrival rear",-9.2f,-9.5f,2.4f);var front=Door("R3 Hub exit only",-5.1f,-9.5f,2.4f);
 B("R3 Hub west south pier",new Vector3(-5.1f,1.6f,-11.5f),new Vector3(.2f,3.2f,1.4f),wall);
 B("R3 Hub west middle pier",new Vector3(-5.1f,1.6f,-7.35f),new Vector3(.2f,3.2f,1.7f),wall);
 B("R3 Hub west north pier",new Vector3(-5.1f,1.6f,-4.25f),new Vector3(.2f,3.2f,.7f),wall);
 B("R3 sealed observation glass",new Vector3(-5.1f,1.6f,-5.55f),new Vector3(.12f,3.2f,1.9f),glass);
 B("R3 observation sill",new Vector3(-5.06f,.48f,-5.55f),new Vector3(.22f,.96f,1.9f),wall);
 // A permanently sealed shallow observation bay; only one distant textile reading.
 B("R3 observation floor",new Vector3(-7.3f,-.1f,-5.55f),new Vector3(4.4f,.2f,1.9f),floor);
 B("R3 observation ceiling",new Vector3(-7.3f,3.3f,-5.55f),new Vector3(4.4f,.2f,2.1f),wall);
 B("R3 observation back",new Vector3(-9.5f,1.6f,-5.55f),new Vector3(.2f,3.2f,2.1f),wall);
 foreach(float z in new[]{-6.5f,-4.6f})B("R3 observation side",new Vector3(-7.3f,1.6f,z),new Vector3(4.4f,3.2f,.2f),wall);
 var residual=G("R3 single distant curtain residue");var textile=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/M_VS1_VT1_Textile.mat");
 for(int i=0;i<7;i++)B("R3 static curtain fold",new Vector3(-9.22f+(i%2)*.035f,1.65f,-6.03f+i*.16f),new Vector3(.08f,2.4f,.18f),textile,false,residual.transform);
 var glow=G("R3 residual warm reflection",residual.transform);glow.transform.position=new Vector3(-8.7f,2.3f,-5.5f);var warm=glow.AddComponent<Light>();warm.type=LightType.Point;warm.range=3;warm.intensity=.65f;warm.color=new Color(1,.68f,.4f);warm.shadows=LightShadows.None;residual.SetActive(false);
 for(float z=-8;z<48;z+=5)Fixture(-18.6f,z);Fixture(-14,-9.5f);Fixture(-10.5f,-9.5f);Fixture(-7,-9.5f);
 // Reuse the Hub's existing clinical fixtures as sources for local residual fill.
 // SE1's original blackout lights/panels and their arrays remain untouched.
 var lights=G("R3 Hub arrival local light");foreach(float x in new[]{-3f,1f}){var g=G("R3 existing Hub fixture bounce",lights.transform);g.transform.position=new Vector3(x,2.8f,-8);var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=7;l.intensity=1.05f;l.color=new Color(.65f,.8f,1);l.shadows=LightShadows.None;}lights.SetActive(false);
 Label("R3 payoff wayfinding","HUB  /  RETURN",new Vector3(-17.04f,2.5f,46.4f),90,.031f);Label("R3 return wayfinding","HUB  >",new Vector3(-19.87f,1.8f,-8.9f),90,.033f);Label("R3 Hub reverse marking","RETURN ACCESS\nEXIT ONLY",new Vector3(-4.94f,2.5f,-9.5f),90,.026f);
 var trigger=Volume("R3 actual Player final crossing",new Vector3(-5.05f,0,43.7f),new Vector3(2.3f,2.6f,1.2f));trigger.center=Vector3.up;var gate=trigger.gameObject.AddComponent<VS1HubReturnGate>();
 var inside=Volume("R3 arrival clear of rear",new Vector3(-6.85f,1,-9.5f),new Vector3(1.8f,2.4f,2.3f));var clear=Volume("R3 Player fully inside original Hub",new Vector3(-3.6f,1,-9.5f),new Vector3(1.4f,2.4f,2.4f));
 Ref(gate,"player",F("Player").GetComponent<LoopActor>());Ref(gate,"playerBody",F("Player").GetComponent<CharacterController>());Ref(gate,"loop",F("Temporal Loop VS1 Foundation").GetComponent<LoopController>());Ref(gate,"puzzle",F("P3 Recombination Relay").GetComponent<RecombinationRelay>());Ref(gate,"finalPuzzleGate",F("P3 Gate B final hold").GetComponent<SlidingDoor>());Ref(gate,"returnGate",exit);Ref(gate,"arrivalRear",rear);Ref(gate,"arrivalFront",front);Ref(gate,"arrivalInterior",inside);Ref(gate,"arrivalClear",clear);Ref(gate,"residualRoot",residual);Ref(gate,"arrivalLightRoot",lights);
 EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());r.Log("R3 authored with Editor API and saved; original boundaries retained inactive.");}}

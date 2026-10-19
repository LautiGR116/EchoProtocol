using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using EchoProtocol.Horror;
using System.Linq;
internal class CommandScript : IRunCommand {
 ExecutionResult result; Transform root; Material wall,floor,trim,glass,light,idle,cyan; 
 GameObject Find(string n)=>Resources.FindObjectsOfTypeAll<Transform>().First(t=>t.gameObject.scene==SceneManager.GetActiveScene()&&t.name==n).gameObject;
 Material Mat(string p)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/"+p+".mat");
 GameObject Group(string n,Transform parent){var g=new GameObject(n);g.transform.SetParent(parent,false);result.RegisterObjectCreation(g);return g;}
 GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=true,Transform parent=null){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent??root,false);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)result.DestroyObject(g.GetComponent<Collider>());result.RegisterObjectCreation(g);return g;}
 void Ref(Object o,string n,Object v){var so=new SerializedObject(o);so.FindProperty(n).objectReferenceValue=v;so.ApplyModifiedPropertiesWithoutUndo();}
 SlidingDoor Door(string n,float x,float z,float width,Transform parent=null){var g=Group(n,parent??root);g.transform.position=new Vector3(x,0,z);var b=Box(n+" solid panel",new Vector3(x,1.45f,z),new Vector3(width,2.9f,.22f),trim,true,g.transform);var rb=b.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;var d=g.AddComponent<SlidingDoor>();Ref(d,"doorBody",rb);var so=new SerializedObject(d);so.FindProperty("closedLocalPosition").vector3Value=new Vector3(0,1.45f,0);so.FindProperty("openLocalPosition").vector3Value=new Vector3(0,4.5f,0);so.FindProperty("moveSpeed").floatValue=3;so.ApplyModifiedPropertiesWithoutUndo();Box(n+" header",new Vector3(x,3.1f,z),new Vector3(width+.2f,.4f,.4f),trim);return d;}
 void Label(string n,string text,Vector3 p,float yaw,float size,Transform parent=null){var g=Group(n,parent??root);g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,yaw,0);var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.7f,.83f,.85f);}
 void Fixture(float x,float z){Box("P3 panel housing",new Vector3(x,3.16f,z),new Vector3(1.8f,.14f,.65f),trim,false);Box("P3 stable diffuser",new Vector3(x,3.075f,z),new Vector3(1.6f,.035f,.48f),light,false);var g=Group("P3 panel-owned cool light",root);g.transform.position=new Vector3(x,2.95f,z);g.transform.rotation=Quaternion.Euler(90,0,0);var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=6;l.intensity=1.5f;l.color=new Color(.65f,.8f,1);l.shadows=LightShadows.None;}
 OpticalReader Reader(string n,float z){var g=Group(n,root);g.transform.position=new Vector3(-7.8f,0,z);g.transform.rotation=Quaternion.Euler(0,90,0);var c=g.AddComponent<BoxCollider>();c.isTrigger=true;c.center=new Vector3(0,1,0);c.size=new Vector3(1.4f,2,1.6f);Box(n+" housing",new Vector3(-6.43f,1.55f,z),new Vector3(.16f,.55f,.5f),trim,false);Box(n+" neutral lens",new Vector3(-6.53f,1.55f,z),new Vector3(.025f,.29f,.3f),idle,false);var a=Box(n+" aligned lens",new Vector3(-6.55f,1.55f,z),new Vector3(.025f,.25f,.26f),cyan,false);a.SetActive(false);var r=g.AddComponent<OpticalReader>();Ref(r,"activeIndicator",a);var so=new SerializedObject(r);so.FindProperty("qualificationSeconds").floatValue=.35f;so.ApplyModifiedPropertiesWithoutUndo();return r;}
 public void Execute(ExecutionResult result){this.result=result;if(GameObject.Find("VS1 R2 Return Gallery")!=null)throw new System.Exception("Already built");
 wall=Mat("VT1/M_VS1_VT1_ClinicWall");floor=Mat("VT1/M_VS1_VT1_ClinicFloor");trim=Mat("VT1/M_VS1_VT1_Trim");glass=Mat("SE2/M_SE2_ClearBarrier");light=Mat("M_VS1_LightPanel_Cool");idle=Mat("M_VS1_P1_ReaderIdle");cyan=Mat("M_VS1_P1_ReaderActive");
 var hub=Find("R1 Clinical Observation Hub");root=Group("VS1 R2 Return Gallery",hub.transform).transform;
 // Replace only the north R1 wall: sealed observation window plus one exit.
 var old=Find("R1 North wall");result.RegisterObjectModification(old);old.SetActive(false);
 Box("R2 R1 north sealed glass",new Vector3(-8.8f,1.6f,30.2f),new Vector3(3.3f,3.2f,.12f),glass);
 Box("R2 R1 north solid pier",new Vector3(-6.8f,1.6f,30.2f),new Vector3(.7f,3.2f,.2f),wall);
 Box("R2 R1 north east pier",new Vector3(-4.3f,1.6f,30.2f),new Vector3(.5f,3.2f,.2f),wall);
 var access=Door("R2 Anchor gallery access",-5.5f,30.2f,1.9f);
 // Vestibule, two lanes, and a long transverse payoff; all support is continuous.
 Box("P3 vestibule floor",new Vector3(-6.5f,-.1f,32.1f),new Vector3(5.4f,.2f,4),floor);
 Box("P3 vestibule ceiling",new Vector3(-6.5f,3.3f,32.1f),new Vector3(5.6f,.2f,4.2f),wall);
 Box("P3 vestibule west",new Vector3(-9.2f,1.6f,32.1f),new Vector3(.2f,3.2f,4),wall);
 Box("P3 vestibule east",new Vector3(-3.8f,1.6f,32.1f),new Vector3(.2f,3.2f,4),wall);
 Box("P3 vestibule south west",new Vector3(-8.05f,1.6f,30.2f),new Vector3(2.3f,3.2f,.2f),wall);
 Box("P3 vestibule south east",new Vector3(-4.0f,1.6f,30.2f),new Vector3(.4f,3.2f,.2f),wall);
 Box("P3 lanes floor",new Vector3(-6.5f,-.1f,39.3f),new Vector3(5.4f,.2f,10.6f),floor);
 Box("P3 lanes ceiling",new Vector3(-6.5f,3.3f,39.3f),new Vector3(5.6f,.2f,10.8f),wall);
 Box("P3 lane west boundary",new Vector3(-9.2f,1.6f,39.3f),new Vector3(.2f,3.2f,10.6f),wall);
 Box("P3 alternative east boundary",new Vector3(-3.8f,1.6f,39.3f),new Vector3(.2f,3.2f,10.6f),wall);
 Box("P3 A observation glass",new Vector3(-6.3f,1.6f,35.5f),new Vector3(.12f,3.2f,3),glass);
 Box("P3 opaque separation",new Vector3(-6.3f,1.6f,39.65f),new Vector3(.2f,3.2f,5.3f),wall);
 Box("P3 B observation glass",new Vector3(-6.3f,1.6f,43.45f),new Vector3(.12f,3.2f,2.3f),glass);
 Box("P3 recording lane terminal wall",new Vector3(-7.75f,1.6f,44.6f),new Vector3(3.1f,3.2f,.2f),wall);
 var a=Reader("P3 Reader A",35.2f);var b=Reader("P3 Reader B",43f);
 var ga=Door("P3 Gate A alternative route",-5.05f,35.2f,2.3f);var gb=Door("P3 Gate B final hold",-5.05f,43f,2.3f);
 var indicator=Box("P3 A authorization retained",new Vector3(-5.05f,2.85f,35.05f),new Vector3(.8f,.1f,.035f),cyan,false);indicator.SetActive(false);
 var relay=Group("P3 Recombination Relay",root).AddComponent<RecombinationRelay>();Ref(relay,"readerA",a);Ref(relay,"readerB",b);Ref(relay,"routeGate",ga);Ref(relay,"finalGate",gb);Ref(relay,"authorizationIndicator",indicator);
 Box("P3 long return floor",new Vector3(-10.5f,-.1f,46.4f),new Vector3(13.4f,.2f,3.6f),floor);
 Box("P3 long return ceiling",new Vector3(-10.5f,3.3f,46.4f),new Vector3(13.6f,.2f,3.8f),wall);
 Box("P3 long return north wall",new Vector3(-10.5f,1.6f,48.2f),new Vector3(13.6f,3.2f,.2f),wall);
 Box("P3 long return east end",new Vector3(-3.8f,1.6f,46.4f),new Vector3(.2f,3.2f,3.6f),wall);
 Box("P3 long return west end",new Vector3(-17.2f,1.6f,46.4f),new Vector3(.2f,3.2f,3.6f),wall);
 Box("P3 distant hub sealed glazing",new Vector3(-13.2f,1.6f,44.6f),new Vector3(8f,3.2f,.12f),glass);
 for(float x=-16.8f;x< -9.2f;x+=2.4f){Box("P3 observation mullion",new Vector3(x,1.6f,44.68f),new Vector3(.06f,3.2f,.09f),trim,false);}
 Box("P3 distant glazing base",new Vector3(-13.2f,.3f,44.7f),new Vector3(8,.6f,.18f),wall);
 Box("P3 quiet residual textile",new Vector3(-15.8f,.012f,46.4f),new Vector3(1.5f,.02f,1.4f),Mat("VT1/M_VS1_VT1_Textile"),false);
 for(float x=-16;x<-3.8f;x+=3){Fixture(x,46.4f);Box("P3 repeating north frame",new Vector3(x,1.6f,48.06f),new Vector3(.075f,3.2f,.1f),trim,false);}
 Fixture(-5.5f,32.1f);Fixture(-7.8f,35.5f);Fixture(-5.05f,36.5f);Fixture(-7.8f,40.5f);Fixture(-5.05f,40.5f);Fixture(-5.05f,43.7f);
 Label("P3 threshold identity","RETURN GALLERY",new Vector3(-5.5f,2.55f,30.03f),180,.035f);
 Label("P3 A identifier","A",new Vector3(-6.57f,2.1f,35.2f),270,.055f);Label("P3 B identifier","B",new Vector3(-6.57f,2.1f,43),270,.055f);
 // Existing station owns E. Keep its geometry and all R1 local lights.
 var station=Find("R1 Central observation station");var anchor=station.AddComponent<VS1RecallAnchor>();result.RegisterObjectModification(station);
 var screen=Find("R1 Monitor housing");screen.AddComponent<BoxCollider>();
 var display=Box("R2 stable anchor confirmation",new Vector3(-7.215f,1.48f,27.8f),new Vector3(.02f,.3f,.6f),cyan,false,station.transform);display.SetActive(false);
 var point=Group("R2 R1 recall point",station.transform).transform;point.position=new Vector3(-5.7f,.03f,27.8f);point.rotation=Quaternion.Euler(0,315,0);
 Ref(anchor,"recoveryHub",Find("R1 Player return access trigger").GetComponent<VS1RecoveryHubGate>());Ref(anchor,"loop",Find("Temporal Loop VS1 Foundation").GetComponent<LoopController>());Ref(anchor,"player",Find("Player").GetComponent<LoopActor>());Ref(anchor,"recallPoint",point);Ref(anchor,"activeDisplay",display);Ref(anchor,"galleryAccess",access);
 Label("R2 station instruction","RECALL\n[E] ANCHOR",new Vector3(-7.195f,1.5f,27.8f),90,.014f,station.transform);
 var loop=Find("Temporal Loop VS1 Foundation").GetComponent<LoopController>();var sl=new SerializedObject(loop);var arr=sl.FindProperty("resetTargets");foreach(var target in new MonoBehaviour[]{a,b,ga,gb,relay}){int i=arr.arraySize;arr.InsertArrayElementAtIndex(i);arr.GetArrayElementAtIndex(i).objectReferenceValue=target;}sl.ApplyModifiedPropertiesWithoutUndo();
 EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());result.Log("R2 authored and saved through Editor; original north wall retained inactive.");
 }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using EchoProtocol.Player;
using EchoProtocol.Puzzle;
using Object=UnityEngine.Object;

// A separate, recoverable scene. All scene, material and prefab writes use Editor APIs.
public static class ThresholdProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Threshold.unity";
    const string Art="Assets/_Game/Art/Threshold";
    static Transform root; static TextMesh template; static FinalDoorMemory review;
    static Material plaster,timber,floor,trim,metal,black,glass,paper,brass,pilot;
    static readonly List<Light> lights=new List<Light>();
    static void Ref(Object o,string n,Object value){var so=new SerializedObject(o);so.FindProperty(n).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Set(Object o,string n,object value){var so=new SerializedObject(o);var p=so.FindProperty(n);if(value is bool b)p.boolValue=b;else if(value is float f)p.floatValue=f;else if(value is int i)p.intValue=i;else if(value is string s)p.stringValue=s;else if(value is Vector3 v)p.vector3Value=v;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Arr(Object o,string n,IEnumerable<Object> values){var so=new SerializedObject(o);var p=so.FindProperty(n);var a=values.ToArray();p.arraySize=a.Length;for(int i=0;i<a.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=a[i];so.ApplyModifiedPropertiesWithoutUndo();}
    static T Find<T>() where T:Object=>Object.FindFirstObjectByType<T>();
    static Material Load(string path)=>AssetDatabase.LoadAssetAtPath<Material>(path);
    static Material Mat(string name,Color color,float smooth=.25f,float metallic=0)
    {var path=Art+"/M_"+name+".mat";var m=Load(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);return m;}
    static Material Fabric(string name,Material from,string family,Color color,float normal,float repeats)
    {string path=Art+"/M_"+name+".mat";var m=Load(path);if(!m){m=from?new Material(from):new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+family+"_Diffuse.png"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+family+"_NormalGL.png"));m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+family+"_MetalSmooth.png"));m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_BumpScale",normal);m.SetFloat("_Smoothness",1);m.SetTextureScale("_BaseMap",Vector2.one*repeats);return m;}
    static GameObject Box(string name,Vector3 p,Vector3 size,Material m,bool solid=true)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());if(m&&m.GetTexture("_BaseMap")&&(m==plaster||m==timber||m==floor||m==paper))g.AddComponent<SurfaceMapping>().Configure(g.GetComponent<Renderer>(),size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/2.5f,size.z/2.5f):new Vector2(Mathf.Max(size.x,size.z)/2.5f,size.y/2.5f));return g;}
    static void Wall(string name,float x,float z,float w,float d,Material m=null)
    {Box(name,new Vector3(x,1.36f,z),new Vector3(w,2.72f,d),m?m:plaster);Box(name+" skirting",new Vector3(x,.075f,z),new Vector3(w+.024f,.15f,d+.024f),trim,false);Box(name+" picture rail",new Vector3(x,2.35f,z),new Vector3(w+.03f,.035f,d+.03f),trim,false);}
    static TextMesh Label(string name,string text,Vector3 p,float yaw,float width,Color color,float size=.02f)
    {var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=text;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;float w=t.GetComponent<Renderer>().localBounds.size.x;if(w>width)t.characterSize*=width/w;return t;}
    static BoxCollider Zone(string name,Vector3 p,Vector3 size)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;var c=g.AddComponent<BoxCollider>();c.isTrigger=true;c.size=size;return c;}
    static FinalDoorInteraction Interaction(string name,FinalDoorInteractionKind kind,Vector3 p,Vector3 size)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;g.AddComponent<BoxCollider>().size=size;var i=g.AddComponent<FinalDoorInteraction>();Ref(i,"review",review);Set(i,"kind",(int)kind);return i;}
    static FinalDoorInteraction Bind(GameObject g,FinalDoorInteractionKind kind)
    {var i=g.AddComponent<FinalDoorInteraction>();Ref(i,"review",review);Set(i,"kind",(int)kind);return i;}
    static Light Pool(string name,Vector3 p,Vector3 aim,Color color,float intensity,float range,float angle=95)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;g.transform.LookAt(aim);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.spotAngle=angle;l.innerSpotAngle=angle*.68f;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.10f;lights.Add(l);return l;}
    static AudioSource Sound(string name,Vector3 p)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=1;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Linear;a.minDistance=1;a.maxDistance=7;a.volume=.14f;a.outputAudioMixerGroup=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();return a;}
    static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
    static GameObject Model(string name,Vector3 p,float yaw=0)
    {var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Overhaul/EP_OV_"+name+".fbx");var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Threshold original / "+name;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>materials.TryGetValue(m.name,out var v)?v:metal).ToArray();return g;}
    static void Frame(Vector3 p,float width)
    {for(int s=-1;s<=1;s+=2)Box("6B painted architrave",p+new Vector3(s*(width/2+.035f),1.10f,0),new Vector3(.095f,2.20f,.23f),trim,false);Box("6B lintel trim",p+Vector3.up*2.175f,new Vector3(width+.17f,.095f,.23f),trim,false);}
    [MenuItem("Echo Protocol/Author threshold reconstruction")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(ResidenceProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");
        foreach(var g in scene.GetRootGameObjects().Where(g=>g.name.StartsWith("Campaign /")||g.name.StartsWith("Finish /")))foreach(Transform child in g.transform)if(child.position.z<=-74.2f)child.gameObject.SetActive(false);
        root=new GameObject("Threshold / del otro lado").transform;lights.Clear();
        plaster=Load("Assets/_Game/Art/Overhaul/M_Plaster.mat");timber=Load("Assets/_Game/Art/Overhaul/M_Walnut.mat");floor=Load("Assets/_Game/Art/Finish/M_Linoleum.mat");paper=Load("Assets/_Game/Art/Overhaul/M_FloralPaper.mat");glass=Load("Assets/_Game/Art/Finish/M_ObservationGlass.mat");trim=Mat("Joinery",new Color(.33f,.36f,.30f),.3f);metal=Mat("OldMetal",new Color(.14f,.17f,.16f),.48f,.7f);brass=Mat("Brass",new Color(.38f,.29f,.13f),.55f,.72f);black=Mat("Bakelite",new Color(.018f,.025f,.024f),.25f);pilot=Mat("WitnessPilot",new Color(.12f,.65f,.52f));pilot.shader=Shader.Find("Universal Render Pipeline/Unlit");
        IntegrateMaterials();
        var player=Find<FirstPersonController>();var session=Find<GameSession>();var director=Find<CampaignDirector>();var loop=Find<LoopController>();
        review=root.gameObject.AddComponent<FinalDoorMemory>();Ref(review,"house",Find<HouseMemory>());Ref(review,"session",session);Ref(review,"player",player);Ref(review,"view",player.GetComponentInChildren<Camera>());Ref(review,"loop",loop);Ref(review,"recorder",Find<EchoRecorder>());
        // One continuous ground plane; the left passage turns around a genuinely opaque domestic interior.
        Box("Threshold structural floor",new Vector3(-1,-.10f,-79),new Vector3(7,.20f,10),floor);
        Box("Threshold ceiling",new Vector3(-1,2.79f,-79),new Vector3(7,.14f,10),plaster);
        Box("Interior timber inlay",new Vector3(0,.003f,-76.5f),new Vector3(4.9f,.008f,4.9f),timber,false);
        Wall("East perimeter",2.5f,-76.5f,.18f,5);
        Box("Porter recess floor",new Vector3(3.6f,-.10f,-81.5f),new Vector3(2.2f,.20f,5),floor);Box("Porter recess ceiling",new Vector3(3.6f,2.79f,-81.5f),new Vector3(2.2f,.14f,5),plaster);Wall("Porter recess north",3.6f,-79,2.2f,.18f);Wall("Porter recess east",4.7f,-81.5f,.18f,5);Wall("West perimeter",-4.5f,-79,.18f,10);
        Wall("Entry east wall",1.83f,-74,1.34f,.18f);Wall("Entry west wall",-2.83f,-74,3.34f,.18f);
        Box("Entry head",new Vector3(0,2.47f,-74),new Vector3(2.32f,.5f,.18f),plaster);
        Wall("Interior north partition",-2.5f,-74.55f,.18f,1.1f,paper);
        Wall("Interior south partition",-2.5f,-77.95f,.18f,2.1f,paper);
        Box("Side passage header",new Vector3(-2.5f,2.47f,-76),new Vector3(.18f,.5f,1.8f),paper);
        Wall("6B wall left",-1.53f,-79,1.94f,.18f,paper);Wall("6B wall right",1.53f,-79,1.94f,.18f,paper);
        Box("6B structural header",new Vector3(0,2.455f,-79),new Vector3(1.12f,.53f,.18f),paper);Frame(new Vector3(0,0,-79),1.04f);
        // The leaf has real transparent glazing, and only that pane is transparent to witness rays.
        var hinge=new GameObject("6B review door / hinge");hinge.transform.SetParent(root);hinge.transform.position=new Vector3(-.48f,0,-79);var rb=hinge.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.Interpolate;
        var lower=Box("6B lower opaque leaf",new Vector3(0,.55f,-79),new Vector3(.96f,1.10f,.065f),trim);lower.transform.SetParent(hinge.transform,true);
        for(int side=-1;side<=1;side+=2){var stile=Box("6B solid stile",new Vector3(side*.43f,1.585f,-79),new Vector3(.10f,.97f,.065f),trim);stile.transform.SetParent(hinge.transform,true);}
        var top=Box("6B solid top rail",new Vector3(0,2.02f,-79),new Vector3(.96f,.14f,.065f),trim);top.transform.SetParent(hinge.transform,true);
        var pane=Box("6B clear visor",new Vector3(0,1.525f,-79),new Vector3(.76f,.85f,.022f),glass);pane.transform.SetParent(hinge.transform,true);
        foreach(float z in new[]{-.045f,.045f}){var inset=Box("6B lower recessed panel",new Vector3(0,.60f,-79+z),new Vector3(.72f,.71f,.027f),materials["OV_Walnut"] ,false);inset.transform.SetParent(hinge.transform,true);var handle=Box("6B latch lever",new Vector3(.35f,1.02f,-79+z*2),new Vector3(.15f,.028f,.03f),brass,false);handle.transform.SetParent(hinge.transform,true);}
        var door=hinge.AddComponent<HouseSwingDoor>();Ref(door,"hingeBody",rb);Ref(door,"solidLeaf",lower.GetComponent<BoxCollider>());Ref(door,"playerBody",player.GetComponent<CharacterController>());Set(door,"openAngle",105f);Set(door,"degreesPerSecond",50f);Bind(hinge,FinalDoorInteractionKind.OpenDoor);Ref(review,"domesticDoor",door);Arr(review,"transparentSightColliders",new[]{pane.GetComponent<Collider>()});
        Ref(door,"movementAudio",Sound("6B review hinge sound",hinge.transform.position+Vector3.up));Ref(door,"movementClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav"));Ref(door,"latchAudio",Sound("6B review latch",new Vector3(.35f,1,-79)));Ref(door,"unlockClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Finish/Relay.wav"));Ref(door,"openEndpointClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav"));
        // The recorded arrival is authored by the Player in the exterior hall, then watched from within.
        var origin=new GameObject("6B review origin");origin.transform.SetParent(root);origin.transform.SetPositionAndRotation(new Vector3(0,.03f,-76),Quaternion.Euler(0,180,0));Ref(review,"origin",origin.transform);
        Ref(review,"outsideZone",Zone("6B outside stillness",new Vector3(0,1,-80.25f),new Vector3(1.6f,2.2f,1.2f)));
        var facing=new GameObject("Facing the front of 6B");facing.transform.SetParent(root);facing.transform.SetPositionAndRotation(new Vector3(0,1,-79),Quaternion.identity);Ref(review,"outsideFacingTransform",facing.transform);
        Ref(review,"insideWitnessZone",Zone("Inside 6B / actual witness",new Vector3(0,1,-77.3f),new Vector3(2.8f,2.2f,2.8f)));
        var originPanel=Box("Review origin instrument",new Vector3(2.35f,1.45f,-75.7f),new Vector3(.10f,.68f,.88f),metal);Bind(originPanel,FinalDoorInteractionKind.BeginReview);
        Label("Review origin text","6B / REVISIÓN",new Vector3(2.289f,1.65f,-75.7f),90,.70f,new Color(.6f,.78f,.70f));
        Label("Interior revision instructions","CORREDOR LATERAL\nREGISTRÁ TU LLEGADA\nMIRÁ LA PUERTA. ESPERÁ.\nR: VOLVER / RESTABLECER",new Vector3(2.289f,1.38f,-75.7f),90,.74f,new Color(.58f,.66f,.60f),.027f);
        Label("6B outside identity","6B",new Vector3(.79f,1.7f,-79.11f),0,.18f,new Color(.75f,.70f,.53f),.07f);
        for(int side=-1;side<=1;side+=2)Box("Exterior registration edge",new Vector3(side*.57f,.011f,-80.25f),new Vector3(.023f,.004f,.78f),brass,false);
        Label("Outside pause instruction","FRENTE A 6B\nPermanecer hasta la señal",new Vector3(-.85f,1.35f,-79.11f),0,.55f,new Color(.58f,.61f,.52f),.022f);
        var ready=Box("Current take ready indicator",new Vector3(-.85f,1.12f,-79.112f),new Vector3(.065f,.03f,.014f),pilot,false);Ref(review,"recordingPilot",ready);
        // Porter's work surface is physically off the central route; the tray remains a separate reachable object.
        var counter=Model("ReceptionCounter",new Vector3(2.65f,0,-82),270);counter.transform.localScale*=.8f;
        Box("Porter counter collision",new Vector3(2.05f,.44f,-82),new Vector3(.75f,.88f,1.96f),black).GetComponent<Renderer>().enabled=false;
        Model("KeyMailCabinet",new Vector3(4.56f,1.3f,-81.9f),270);
        var terminal=Box("Original conservation terminal",new Vector3(2.12f,1.08f,-82.6f),new Vector3(.12f,.34f,.38f),metal);Bind(terminal,FinalDoorInteractionKind.Preserve);
        Label("Conservation terminal engraved","ORIGINAL",new Vector3(2.049f,1.13f,-82.6f),90,.27f,new Color(.58f,.71f,.64f));
        var accepted=Box("Original verification pilot",new Vector3(2.048f,1.01f,-82.6f),new Vector3(.014f,.025f,.08f),pilot,false);Ref(review,"terminalPilot",accepted);
        var tray=Model("KeyConsole",new Vector3(-1.4f,.1f,-83.62f));
        var trayKey=tray.GetComponentsInChildren<Transform>().First(t=>t.name=="Key").gameObject;Ref(review,"keyInTray",trayKey);
        Interaction("Return key to porter's tray",FinalDoorInteractionKind.ReturnKey,new Vector3(-1.4f,.93f,-83.62f),new Vector3(.55f,.20f,.36f));
        Box("Porter key shelf collision",new Vector3(-1.4f,.48f,-83.62f),new Vector3(.79f,.90f,.39f),trim).GetComponent<Renderer>().enabled=false;
        Label("Key return identification","PORTERÍA / DEVOLUCIÓN",new Vector3(-1.4f,1.27f,-83.891f),180,.67f,new Color(.59f,.62f,.52f));
        var note=Interaction("Mara final note",FinalDoorInteractionKind.Document,new Vector3(-1.75f,1.45f,-78.86f),new Vector3(.32f,.42f,.04f));
        Box("Mara final note paper",note.transform.position+Vector3.back*.03f,new Vector3(.30f,.40f,.012f),Mat("NotePaper",new Color(.73f,.68f,.52f)),false);
        Set(note,"documentTitle","Del otro lado");Set(note,"documentBody","A.,\n\nNo te pedí que recordaras mis palabras exactas. Te pregunté por qué esperaste del otro lado. Yo también puedo estar recordándolo mal.\n\nSi conservás otra toma, dejala entera. Después devolvé la llave.\n\nMara\n\n[Documento aportado. No forma parte de la grabación del Echo.]");
        // A real last threshold rather than a trigger at the conservation button.
        Wall("Departure front left",-2.6f,-84,3.8f,.18f);Wall("Departure front right",2.7f,-84,4f,.18f);
        Box("Departure header",new Vector3(0,2.47f,-84),new Vector3(1.42f,.5f,.18f),plaster);
        var exitRoot=new GameObject("Porter exit door mechanism");exitRoot.transform.SetParent(root);exitRoot.transform.position=new Vector3(0,0,-84);
        var exitLeaf=Box("Porter sliding exit",new Vector3(0,1.08f,-84),new Vector3(1.4f,2.16f,.10f),trim);exitLeaf.transform.SetParent(exitRoot.transform,true);var exitRb=exitLeaf.AddComponent<Rigidbody>();exitRb.isKinematic=true;exitRb.useGravity=false;exitRb.interpolation=RigidbodyInterpolation.Interpolate;
        var exit=exitRoot.AddComponent<SlidingDoor>();Ref(exit,"doorBody",exitRb);Set(exit,"closedLocalPosition",new Vector3(0,1.08f,0));Set(exit,"openLocalPosition",new Vector3(-1.50f,1.08f,0));Set(exit,"moveSpeed",.75f);Ref(review,"exitDoor",exit);
        var mechanism=Sound("Porter exit physical sound",new Vector3(0,1.2f,-84));var doorSound=mechanism.gameObject.AddComponent<EchoProtocol.Audio.SlidingDoorAudio>();Ref(doorSound,"door",exit);Ref(doorSound,"doorBody",exitRb);Ref(doorSound,"mechanismSource",mechanism);Ref(doorSound,"movementLoopClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav"));Ref(doorSound,"endpointClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav"));Set(doorSound,"mechanismVolume",.14f);
        Box("Departure floor",new Vector3(0,-.10f,-87),new Vector3(2.4f,.20f,6),timber);Box("Departure ceiling",new Vector3(0,2.79f,-87),new Vector3(2.4f,.14f,6),plaster);Wall("Departure left",-1.2f,-87,.18f,6);Wall("Departure right",1.2f,-87,.18f,6);Wall("Departure end",0,-90,2.4f,.18f);
        Ref(review,"departureZone",Zone("Actual departure after returning key",new Vector3(0,1,-88),new Vector3(2,2.2f,2)));
        Ref(review,"audioSource",Sound("Final current-world contact",new Vector3(.95f,1.3f,-82.6f)));var contact=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Finish/Relay.wav");Ref(review,"preserveClip",contact);Ref(review,"keyReturnClip",contact);
        Pool("Inside final practical",new Vector3(1.9f,2.5f,-76.8f),new Vector3(0,.7f,-78),new Color(1,.78f,.52f),2.2f,5.5f,100);
        Pool("Review instrument task light",new Vector3(1.95f,2.4f,-75.3f),new Vector3(2.35f,1.3f,-75.7f),new Color(1,.83f,.6f),1.3f,3,85);
        Pool("Lateral passage practical",new Vector3(-3.5f,2.55f,-77.2f),new Vector3(-3.5f,.1f,-78.4f),new Color(.65f,.75f,.73f),1.6f,5,105);
        Pool("Arrival exterior practical",new Vector3(.65f,2.45f,-81.4f),new Vector3(0,1,-80.15f),new Color(.67f,.78f,.77f),2.4f,5,100);
        Pool("Porter desk pool",new Vector3(2.6f,2.5f,-82),new Vector3(1.7f,.7f,-82.6f),new Color(1,.81f,.55f),2.8f,4.8f,90);
        Pool("Departure residual",new Vector3(0,2.45f,-88),new Vector3(0,.3f,-86),new Color(.66f,.70f,.62f),1.1f,5.5f,100);
        foreach(var l in lights){Box("Practical ceiling rose",l.transform.position+Vector3.up*.05f,new Vector3(.25f,.08f,.25f),trim,false);var ld=new SerializedObject(l.GetUniversalAdditionalLightData());ld.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;ld.ApplyModifiedPropertiesWithoutUndo();}
        var budget=Find<LightShadowBudget>();var old=new SerializedObject(budget).FindProperty("lights");var all=new List<Object>();for(int i=0;i<old.arraySize;i++)all.Add(old.GetArrayElementAtIndex(i).objectReferenceValue);all.AddRange(lights);Arr(budget,"lights",all);
        Ref(session,"finalDoorMemory",review);Set(session,"editionLabel","ARCHIVO 6B / DEL OTRO LADO");Ref(director,"finalDoor",review);Ref(director,"returnWithoutReview",Find<VS1HubReturnGate>());Set(director,"checkpointKey","EP.ThresholdCheckpoint.v1");Find<VS1DenialEvidence>().enabled=false;
        Ref(Find<EchoProtocol.Audio.CampaignRoomTone>(),"finalDoor",review);Ref(Find<SessionAudio>(),"finalDoor",review);
        var ds=new SerializedObject(director);ds.FindProperty("origins").GetArrayElementAtIndex(3).objectReferenceValue=origin.transform;ds.ApplyModifiedPropertiesWithoutUndo();
        foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(t.name.Contains("D1")||t.name=="Denial station label")t.gameObject.SetActive(false);
        ReplaceFigure(loop);
        // Residence references remain their own edition. Only this scene's instances receive new furniture surfaces.
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("Threshold authored. Requires real route, camera, console and checkpoint verification.");
    }
    static void IntegrateMaterials()
    {
        foreach(var path in Directory.GetFiles(Art+"/Textures","*.png")){var i=AssetImporter.GetAtPath(path) as TextureImporter;i.wrapMode=TextureWrapMode.Repeat;i.mipmapEnabled=true;i.anisoLevel=8;i.maxTextureSize=1024;i.sRGBTexture=path.Contains("Diffuse");if(path.Contains("NormalGL"))i.textureType=TextureImporterType.NormalMap;i.SaveAndReimport();}
        var walnut=Fabric("FurnitureWalnut",null,"WalnutContinuous",Color.white,.6f,1);
        var linen=Fabric("Linen",null,"LinenPlain",new Color(.77f,.72f,.59f),.8f,2);
        var chair=Fabric("ChairLinen",null,"LinenPlain",new Color(.40f,.48f,.42f),.6f,3);
        materials.Clear();foreach(var path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Art/Overhaul"}).Select(AssetDatabase.GUIDToAssetPath)){var m=Load(path);if(m.name.StartsWith("M_OV_"))materials[m.name.Substring(2)]=m;}
        materials["OV_Walnut"]=materials["OV_WalnutEdge"]=walnut;materials["OV_Linen"]=linen;materials["OV_ChairFabric"]=chair;materials["OV_Glass"]=glass;
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name.StartsWith("Residence original /")))
            foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.sharedMaterials=r.sharedMaterials.Select(m=>m.name=="M_Walnut"?walnut:m.name=="M_OV_Linen"?linen:m.name=="M_OV_ChairFabric"?chair:m).ToArray();
    }
    static void ReplaceFigure(LoopController loop)
    {
        var mesh=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Threshold/EP_AdultRecordedSurface.fbx");
        var current=(EchoPlayback)new SerializedObject(loop).FindProperty("echoPrefab").objectReferenceValue;
        var echo=Object.Instantiate(current.gameObject);echo.name="Echo / adult recorded surface";
        foreach(var r in echo.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        var body=(GameObject)PrefabUtility.InstantiatePrefab(mesh);body.transform.SetParent(echo.transform,false);body.transform.localPosition=Vector3.zero;
        var surface=Mat("EchoSurface",new Color(.16f,.58f,.56f));surface.shader=Shader.Find("EchoProtocol/RecordedSurface");surface.SetColor("_Color",new Color(.16f,.58f,.56f));
        foreach(var r in body.GetComponentsInChildren<Renderer>()){r.sharedMaterials=r.sharedMaterials.Select(_=>surface).ToArray();r.shadowCastingMode=ShadowCastingMode.Off;}
        Directory.CreateDirectory("Assets/_Game/Prefabs/Threshold");var prefab=PrefabUtility.SaveAsPrefabAsset(echo,"Assets/_Game/Prefabs/Threshold/EchoThreshold.prefab");Object.DestroyImmediate(echo);Ref(loop,"echoPrefab",prefab.GetComponent<EchoPlayback>());
        var house=Find<HouseMemory>();var presence=(GameObject)new SerializedObject(house).FindProperty("presenceRoot").objectReferenceValue;
        foreach(var r in presence.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        var figure=(GameObject)PrefabUtility.InstantiatePrefab(mesh);figure.name="Study adult silhouette";figure.transform.SetParent(presence.transform,false);figure.transform.localPosition=Vector3.zero;figure.transform.localRotation=Quaternion.Euler(0,270,0)*figure.transform.localRotation;
        var cloth=Mat("PresenceWool",new Color(.038f,.049f,.047f),.06f);var skin=Mat("PresenceSkin",new Color(.15f,.135f,.115f),.12f);
        foreach(var r in figure.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name.Contains("Skin")?skin:cloth).ToArray();
    }
}

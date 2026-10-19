using System;
using System.IO;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using Object=UnityEngine.Object;

public static class OvernightProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol.unity";
    public const string Delivery="Builds/EchoProtocol-20260930";
    private static Transform root;
    private static TextMesh labelTemplate;
    private static Material wall,floor,trim,wood,panel;
    private static T Find<T>() where T:Object=>Object.FindFirstObjectByType<T>();
    private static GameObject Named(string name)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==name).gameObject;
    private static Material Mat(string file)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/"+file+".mat");
    private static void Set(Object o,string field,Object value) {var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    private static void Array(Object o,string field,Object[] values){var s=new SerializedObject(o);var p=s.FindProperty(field);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
    private static GameObject Box(string name,Vector3 p,Vector3 size,Material m,bool solid=true,Transform parent=null)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent?parent:root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    private static TextMesh Label(string name,string words,Vector3 p,float yaw,float size,Transform parent=null)
    {var t=Object.Instantiate(labelTemplate,parent?parent:root);t.name=name;t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.characterSize=size*.4f;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.72f,.85f,.83f);return t;}
    private static Light Light(string name,Vector3 p,Color color,float intensity,float range)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;return l;}
    [MenuItem("Echo Protocol/Author complete edition")]
    public static void Author()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before authoring.");
        if(File.Exists(ScenePath))throw new InvalidOperationException("Production scene exists. Edit it deliberately; do not overwrite.");
        var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity");
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));AssetDatabase.Refresh();EditorSceneManager.SaveScene(scene,ScenePath);
        wall=Mat("M_VS1_VT1_ClinicWall");floor=Mat("M_VS1_VT1_ClinicFloor");trim=Mat("M_VS1_VT1_Trim");wood=Mat("M_VS1_VT1_DomesticWood");
        panel=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/M_VS1_LightPanel_Cool.mat");
        labelTemplate=Named("P3 threshold identity").GetComponent<TextMesh>();
        root=new GameObject("Complete Edition - presentation and exit").transform;
        var player=Find<FirstPersonController>();var loop=Find<LoopController>();var recorder=player.GetComponent<EchoRecorder>();var evidence=Find<VS1DenialEvidence>();
        evidence.GetComponent<VS1DemoEnding>().enabled=false;
        var session=root.gameObject.AddComponent<GameSession>();
        var output=player.GetComponentInChildren<Camera>().gameObject.AddComponent<SessionAudio>();
        var terminal=Box("Final archive terminal",new Vector3(2.25f,1.25f,-11.85f),new Vector3(1.35f,1.2f,.28f),trim);
        var finale=terminal.AddComponent<ProtocolFinale>();
        var terminalLabel=Label("Archive terminal instructions","SALIDA\nRegistro pendiente",new Vector3(2.25f,1.5f,-11.68f),180,.075f,terminal.transform);
        // Instantiate under scaled primitives would distort text; preserve its authored world scale.
        terminalLabel.transform.SetParent(root,true);
        var lamp=Light("Archive ready lamp",new Vector3(2.25f,2.3f,-11.3f),new Color(.4f,.85f,.8f),1.4f,4).gameObject;
        var south=Named("Hub South Wall");south.SetActive(false);
        Box("Exit south left pier",new Vector3(-3.3f,1.6f,-12.1f),new Vector3(3.6f,3.2f,.2f),wall);
        Box("Exit south right pier",new Vector3(3.3f,1.6f,-12.1f),new Vector3(3.6f,3.2f,.2f),wall);
        Box("Exit header",new Vector3(0,3.02f,-12.1f),new Vector3(3, .36f,.35f),trim);
        var gateRoot=new GameObject("Final exit door");gateRoot.transform.SetParent(root);gateRoot.transform.position=new Vector3(0,0,-12.1f);
        var leaf=Box("Final solid exit leaf",new Vector3(0,1.43f,-12.1f),new Vector3(2.96f,2.86f,.22f),trim,true,gateRoot.transform);
        var rb=leaf.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
        var door=gateRoot.AddComponent<SlidingDoor>();Set(door,"doorBody",rb);
        var so=new SerializedObject(door);so.FindProperty("closedLocalPosition").vector3Value=new Vector3(0,1.43f,0);so.FindProperty("openLocalPosition").vector3Value=new Vector3(0,4.5f,0);so.FindProperty("moveSpeed").floatValue=1.2f;so.ApplyModifiedPropertiesWithoutUndo();
        Label("Exit identity","SALIDA / 00",new Vector3(0,2.65f,-11.94f),180,.10f);
        Box("Exit passage floor",new Vector3(0,-.1f,-18.1f),new Vector3(3.2f,.2f,12),floor);
        Box("Exit passage ceiling",new Vector3(0,3.15f,-18.1f),new Vector3(3.4f,.2f,12.2f),wall);
        Box("Exit passage west wall",new Vector3(-1.7f,1.5f,-18.1f),new Vector3(.2f,3.2f,12.2f),wall);
        Box("Exit passage east wall",new Vector3(1.7f,1.5f,-18.1f),new Vector3(.2f,3.2f,12.2f),wall);
        Box("Exit sealed end",new Vector3(0,1.5f,-24.2f),new Vector3(3.6f,3.2f,.2f),wood);
        var lights=new Light[3];var panels=new Renderer[3];
        for(int i=0;i<3;i++)
        {
            float z=-14-i*2.5f;
            Box("Exit frame left "+i,new Vector3(-1.5f,1.4f,z),new Vector3(.14f,2.8f,.17f),trim,false);
            Box("Exit frame right "+i,new Vector3(1.5f,1.4f,z),new Vector3(.14f,2.8f,.17f),trim,false);
            Box("Exit frame header "+i,new Vector3(0,2.82f,z),new Vector3(3.14f,.14f,.17f),trim,false);
            panels[i]=Box("Exit practical "+i,new Vector3(0,3.02f,z),new Vector3(.45f,.025f,1.2f),panel,false).GetComponent<Renderer>();
            lights[i]=Light("Exit light "+i,new Vector3(0,2.6f,z),new Color(.58f,.76f,.82f),1.1f,4.8f);
        }
        var textile=Mat("M_VS1_VT1_Textile");
        Box("Final domestic runner",new Vector3(0,.014f,-21.4f),new Vector3(1.25f,.025f,5.4f),textile,false);
        Box("Final domestic left trim",new Vector3(-1.48f,1.3f,-23.8f),new Vector3(.14f,2.6f,.2f),wood,false);
        Box("Final domestic right trim",new Vector3(1.48f,1.3f,-23.8f),new Vector3(.14f,2.6f,.2f),wood,false);
        Light("Final amber aperture",new Vector3(0,2.1f,-23.2f),new Color(1,.65f,.34f),2.4f,7);
        Label("Final passage words","NO HACE FALTA\nVOLVER A EMPEZAR",new Vector3(0,1.6f,-24.05f),180,.09f);
        var zone=new GameObject("Exit completion volume");zone.transform.SetParent(root);zone.transform.position=new Vector3(0,1,-22.95f);var box=zone.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(3,2,1.5f);
        var group=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();
        var contactObject=new GameObject("Final physical relay source");contactObject.transform.SetParent(root);contactObject.transform.position=terminal.transform.position;var contact=contactObject.AddComponent<AudioSource>();contact.playOnAwake=false;contact.spatialBlend=1;contact.volume=.24f;contact.minDistance=2;contact.maxDistance=14;contact.outputAudioMixerGroup=group;contact.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/VS1/SFX_VS1_FrameRelayStrain.wav");
        var motor=gateRoot.AddComponent<AudioSource>();motor.outputAudioMixerGroup=group;motor.playOnAwake=false;var doorAudio=gateRoot.AddComponent<SlidingDoorAudio>();
        Set(doorAudio,"door",door);Set(doorAudio,"doorBody",rb);Set(doorAudio,"mechanismSource",motor);Set(doorAudio,"movementLoopClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav"));Set(doorAudio,"endpointClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav"));
        Set(finale,"evidence",evidence);Set(finale,"thought",evidence.GetComponent<SD1SubtitleDisplay>());Set(finale,"player",player);Set(finale,"loop",loop);Set(finale,"recorder",recorder);Set(finale,"exitDoor",door);Set(finale,"terminalText",terminalLabel);Set(finale,"readyLamp",lamp);Set(finale,"contactSource",contact);Set(finale,"session",session);Set(finale,"finishZone",box);Array(finale,"corridorLights",lights);Array(finale,"corridorPanels",panels);
        Array(finale,"ambience",Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(a=>a.loop&&a.outputAudioMixerGroup&&a.outputAudioMixerGroup.name=="Ambience").Cast<Object>().ToArray());
        Set(session,"player",player);Set(session,"interactor",player.GetComponent<PlayerInteractor>());Set(session,"loop",loop);Set(session,"recorder",recorder);Set(session,"audioOutput",output);Set(session,"finale",finale);
        // The first composition now explains the two routes and the recording contract.
        Box("Protocol introduction board",new Vector3(-.05f,1.75f,-4.04f),new Vector3(3.65f,1.65f,.06f),trim,false);
        Label("Protocol introduction","ECHO PROTOCOL\n\nRegistramos tu recorrido. No tu memoria.",new Vector3(-.05f,2.02f,-4.08f),0,.092f);
        Label("Protocol start rules","01  MIRÁ LOS LECTORES HASTA VER CIAN\n02  PULSÁ R PARA VOLVER Y REPRODUCIR\n03  COOPERÁ DESDE EL OTRO LADO",new Vector3(-.05f,1.23f,-4.08f),0,.051f);
        Label("Register lane identity","01 / REGISTRO",new Vector3(-2.75f,2.75f,-3.99f),0,.078f);
        Label("Service lane identity","02 / COOPERACIÓN",new Vector3(2.5f,2.75f,-3.99f),0,.075f);
        Label("P1 A rule","A\nMIRÁ EL LECTOR",new Vector3(-.35f,2.08f,3),90,.07f);
        Label("P1 B rule","B\nREGISTRÁ AMBOS. LUEGO [R]",new Vector3(-.35f,2.08f,11),90,.055f);
        Label("P1 service rule","C\nVOS DE ESTE LADO",new Vector3(4.27f,2.08f,10),90,.06f);
        Label("P2 hold instruction","MANTENÉ LA MIRADA\nDejá tiempo para cruzar. Después [R].",new Vector3(-.66f,2.05f,23.1f),90,.052f);
        Light("Introduction board reflected fill",new Vector3(0,2.5f,-6.0f),new Color(.6f,.78f,.8f),.55f,4f);
        // Shallow non-colliding panel seams and skirting add scale without changing traversal.
        for(int side=-1;side<=1;side+=2)
        {
            Box("Hub skirting "+side,new Vector3(side*4.97f,.12f,-8),new Vector3(.035f,.24f,7.8f),trim,false);
            for(int i=0;i<4;i++)Box("Hub panel seam "+side+" "+i,new Vector3(side*4.98f,1.6f,-4.8f-i*1.8f),new Vector3(.02f,3.05f,.018f),trim,false);
        }
        // Terminal label parent is intentionally unscaled; it remains a readable world label.
        terminalLabel.transform.localScale=Vector3.one;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        AssetDatabase.SaveAssets();Debug.Log("COMPLETE EDITION authored: "+ScenePath);
    }
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build requires Edit Mode.");
        Directory.CreateDirectory(Delivery);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=Delivery+"/EchoProtocol.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
        var s=report.summary;File.WriteAllText(Delivery+"/build-report.txt",$"Result: {s.result}\nErrors: {s.totalErrors}\nWarnings: {s.totalWarnings}\nSize: {s.totalSize}\nTime: {s.totalTime}\nUnity: {Application.unityVersion}\nScene: {ScenePath}\n");
        if(s.result!=BuildResult.Succeeded)throw new InvalidOperationException("Build failed.");
        Debug.Log("COMPLETE EDITION BUILD SUCCEEDED "+s.outputPath);
    }
}

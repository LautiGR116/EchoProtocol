using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using Object=UnityEngine.Object;

// Opt-in successor. Recovery and its build remain independently recoverable.
public static class IsolationProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Isolation.unity";
    const string Art="Assets/_Game/Art/Isolation";
    static Transform root;static Material steel,opal,ink;
    static readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
    static readonly List<Light> newLights=new List<Light>();
    static Transform Any(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
    static void Ref(Object o,string key,Object value){var s=new SerializedObject(o);s.FindProperty(key).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Arr(Object o,string key,IEnumerable<Object> values){var s=new SerializedObject(o);var a=s.FindProperty(key);var v=values.ToArray();a.arraySize=v.Length;for(int i=0;i<v.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=v[i];s.ApplyModifiedPropertiesWithoutUndo();}
    static Material Mat(string n,Color c,float smooth=.3f,float metallic=0)
    {var p=Art+"/M_"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);EditorUtility.SetDirty(m);return m;}
    static GameObject Box(string n,Vector3 p,Vector3 size,Material m,bool solid=false)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static Transform Point(string n,Vector3 p){var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;return g.transform;}
    static Transform Mark(GameObject model,string n)=>model.GetComponentsInChildren<Transform>().First(t=>t.name==n);
    static GameObject Model(string n,Vector3 p)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/IsolationControl/EP_IC_"+n+".fbx");if(!asset)throw new Exception("Missing isolation model "+n);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);PrefabUtility.UnpackPrefabInstance(g,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);g.name="Isolation original / "+n;g.transform.SetParent(root);var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,270,0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mats[m.name]).ToArray();return g;
    }
    static TextMesh Text(string n,string words,Vector3 p,float yaw,float size)
    {var template=Any("Pressure contract").GetComponent<TextMesh>();var t=Object.Instantiate(template,root);t.name=n;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.color=new Color(.58f,.65f,.57f);t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;return t;}
    static Transform MovingPivot(GameObject model,string mesh,string marker,string from,string to,float angle,out Vector3 axis)
    {
        var moving=Mark(model,mesh);var centre=Mark(model,marker).position;var start=Mark(model,from).position;var end=Mark(model,to).position;
        if(Vector3.Distance(moving.position,centre)>.002f)throw new Exception("Moving mesh pivot mismatch "+mesh);
        axis=Vector3.Cross(start-centre,end-centre).normalized;
        if(angle>0&&Mathf.Abs(angle)>180)axis=-axis;
        var pivot=Point("Isolation live "+mesh,centre);moving.SetParent(pivot,true);
        var probe=Point("Isolation motion endpoint check",start);probe.SetParent(pivot,true);pivot.localRotation=Quaternion.AngleAxis(angle,axis);
        if(Vector3.Distance(probe.position,end)>.003f)throw new Exception("Motion endpoint mismatch "+mesh+": "+Vector3.Distance(probe.position,end));
        pivot.localRotation=Quaternion.identity;Object.DestroyImmediate(probe.gameObject);return pivot;
    }
    static AudioSource Source(string n,Vector3 p,float near,float far,float pitch=1)
    {var g=Point(n,p).gameObject;var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=1;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Linear;a.minDistance=near;a.maxDistance=far;a.pitch=pitch;a.outputAudioMixerGroup=AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();return a;}
    static AudioClip Clip(string p)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/"+p);
    static void Track(Renderer r,Light l,Color color,Color emission,int index=0)
    {var t=r.gameObject.AddComponent<PracticalEmission>();var s=new SerializedObject(t);s.FindProperty("surface").objectReferenceValue=r;s.FindProperty("source").objectReferenceValue=l;s.FindProperty("materialIndex").intValue=index;s.FindProperty("litColor").colorValue=color;s.FindProperty("emission").colorValue=emission;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Emission(Material m,Color color){m.SetColor("_EmissionColor",color);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);}
    static void HideLegacy()
    {
        var stations=Object.FindObjectsByType<CampaignStation>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(s=>s.Kind==StationKind.PressureOrigin||s.Kind==StationKind.ValveA||s.Kind==StationKind.ValveB).ToArray();
        foreach(var station in stations)
        {
            var position=station.transform.position;
            foreach(var f in Object.FindObjectsByType<StationFeedback>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {var so=new SerializedObject(f);if(so.FindProperty("station").objectReferenceValue!=station)continue;var needle=so.FindProperty("needle").objectReferenceValue as Transform;if(needle)needle.gameObject.SetActive(false);f.gameObject.SetActive(false);}
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {if((t.name=="Finish ValveControl"||t.name.StartsWith("Terminal "))&&Vector3.Distance(t.position,position)<.30f)t.gameObject.SetActive(false);}
            var display=new SerializedObject(station).FindProperty("display").objectReferenceValue as TextMesh;if(display)display.gameObject.SetActive(false);station.gameObject.SetActive(false);
        }
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {if(t.name=="Pressure footprint"||t.name=="Pressure facing"||(t.name=="Painted register corner"&&t.position.z< -30&&t.position.z> -38))t.gameObject.SetActive(false);}
        var old=Object.FindFirstObjectByType<PressureWorkAudio>();if(old)old.enabled=false;
    }
    static void EmergencyRoute()
    {
        // A small, persistent route circuit. It never participates in service shutdown.
        foreach(var p in new[]{new Vector3(1.0f,.38f,-35.9f),new Vector3(1.0f,.38f,-40.3f),new Vector3(1.0f,.38f,-43.9f),new Vector3(1.58f,.38f,-47.82f)})
        {
            Box("Isolation emergency floor shoe",new Vector3(p.x,.035f,p.z),new Vector3(.22f,.07f,.34f),steel);
            Box("Isolation emergency pedestal",new Vector3(p.x,.19f,p.z),new Vector3(.065f,.31f,.085f),steel);
            Box("Isolation emergency enclosure",p,new Vector3(.10f,.15f,.27f),steel);
            Box("Isolation emergency diffuser",p+Vector3.right*.059f,new Vector3(.012f,.09f,.19f),opal);
            var source=Point("Isolation retained floor light",p+Vector3.right*.10f).gameObject.AddComponent<Light>();source.type=LightType.Point;source.color=new Color(.70f,.81f,.71f);source.intensity=1.1f;source.range=4.3f;newLights.Add(source);
        }
        Text("Isolation route destination","PASO / 6B",new Vector3(0,2.95f,-47.84f),180,.046f);
    }
    static void InstrumentReserve(float z,float targetHeight)
    {
        // A shielded instrument lamp shares the reserve circuit, not the motor.
        var housing=new Vector3(5.50f,1.918f,z);
        Box("Isolation instrument lamp bracket",new Vector3(5.67f,1.94f,z),new Vector3(.39f,.045f,.07f),steel);
        Box("Isolation instrument lamp hood",housing,new Vector3(.25f,.065f,.36f),steel);
        var diffuser=Box("Isolation instrument lamp diffuser",housing+Vector3.down*.036f,new Vector3(.18f,.008f,.27f),opal).GetComponent<Renderer>();
        var l=Point("Isolation reserve instrument light",housing+new Vector3(-.045f,-.045f,0)).gameObject.AddComponent<Light>();
        l.type=LightType.Spot;l.transform.rotation=Quaternion.LookRotation(new Vector3(5.78f,targetHeight,z)-l.transform.position);l.color=new Color(.70f,.81f,.71f);l.intensity=.85f;l.range=1.65f;l.spotAngle=85;l.innerSpotAngle=45;newLights.Add(l);
        Track(diffuser,l,opal.color,opal.GetColor("_EmissionColor"));
    }
    [MenuItem("Echo Protocol/Author physical service isolation")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(RecoveryProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        root=new GameObject("Isolation / physical service shutdown").transform;mats.Clear();newLights.Clear();Materials();
        steel=Mat("ServiceSteel",new Color(.11f,.15f,.13f),.42f,.4f);ink=Mat("UnlitGlass",new Color(.018f,.026f,.022f));opal=Mat("EmergencyOpal",new Color(.36f,.48f,.37f));Emission(opal,new Color(.20f,.35f,.24f));
        var director=Object.FindFirstObjectByType<CampaignDirector>();var ds=new SerializedObject(director);var player=Object.FindFirstObjectByType<FirstPersonController>();var session=Object.FindFirstObjectByType<GameSession>();HideLegacy();
        var panel=Model("IsolationPanel",new Vector3(5.9f,1.5f,-36.5f));Model("ControlPedestal",new Vector3(5.9f,1.5f,-36.5f));
        var isolation=root.gameObject.AddComponent<PressureIsolation>();Ref(director,"pressureIsolation",isolation);
        Box("Isolation control interaction body",new Vector3(5.9f,1.5f,-36.5f),new Vector3(.22f,.68f,.53f),ink,true).GetComponent<Renderer>().enabled=false;
        // All reachable control colliders share the component's root for its grip LOS test.
        var control=MovingPivot(panel,"IsolationLever","IsolationPanel__LEVER_PIVOT","IsolationPanel__LEVER_GRIP_MARCHA","IsolationPanel__LEVER_GRIP_AISLADO",70,out var controlAxis);
        var grip=Point("Isolation current grip",Mark(panel,"IsolationPanel__LEVER_GRIP_MARCHA").position);grip.SetParent(control,true);
        var needles=new List<Transform>();Vector3 needleAxis=Vector3.zero;
        foreach(var p in new[]{Mark(panel,"IsolationPanel__GAUGE_MOUNT").position,new Vector3(5.918f,1.75f,-44)})
        {
            var gauge=Model("PressureGauge",p);var needle=MovingPivot(gauge,"PressureNeedle","PressureGauge__NEEDLE_PIVOT","PressureGauge__NEEDLE_TIP_HIGH","PressureGauge__NEEDLE_TIP_ZERO",-270,out needleAxis);
            // From the actual front-facing Unity camera, the exported HIGH is
            // lower-left. Begin at the other endpoint so zero reads conventionally
            // at lower-left, descending through the upper arc rather than the gap.
            needle.localRotation=Quaternion.AngleAxis(-270,needleAxis);needles.Add(needle);
            Text("Isolation gauge unit","bar",Mark(gauge,"PressureGauge__UNIT_LABEL").position+Vector3.left*.003f,90,.0018f).color=new Color(.025f,.035f,.024f);
            var centre=Mark(gauge,"PressureGauge__NEEDLE_PIVOT").position;
            var zero=Mark(gauge,"PressureGauge__NEEDLE_TIP_HIGH").position;
            Text("Isolation gauge zero","0",centre+(zero-centre).normalized*.062f+Vector3.left*.003f,90,.0018f).color=new Color(.025f,.035f,.024f);
        }
        Text("Isolation upper detent","MARCHA",Mark(panel,"IsolationPanel__LABEL_MARCHA").position+Vector3.left*.003f,90,.0033f).color=new Color(.045f,.055f,.043f);
        Text("Isolation lower detent","AISLADO",Mark(panel,"IsolationPanel__LABEL_AISLADO").position+Vector3.left*.003f,90,.0033f).color=new Color(.045f,.055f,.043f);
        Text("Isolation control contract","AISLAMIENTO",new Vector3(5.77f,2.14f,-36.5f),90,.008f);
        Box("Isolation control contract plate",new Vector3(5.79f,2.14f,-36.5f),new Vector3(.025f,.29f,.63f),steel);
        foreach(float offset in new[]{-.22f,.22f})Box("Isolation control header bracket",new Vector3(5.825f,1.985f,-36.5f+offset),new Vector3(.045f,.37f,.035f),steel);
        Box("Isolation automatic relief mount",new Vector3(6.02f,1.60f,-44),new Vector3(.20f,.57f,.42f),steel);
        Box("Isolation relief support",new Vector3(6.15f,.70f,-44),new Vector3(.075f,1.4f,.075f),steel);
        Box("Isolation relief feed",new Vector3(6.68f,1.85f,-44),new Vector3(1.24f,.06f,.06f),steel);
        Text("Isolation automatic relief identity","PURGA\nAUTOMÁTICA",new Vector3(5.79f,2.15f,-44),90,.008f);
        Box("Isolation relief identity plate",new Vector3(5.815f,2.15f,-44),new Vector3(.025f,.25f,.50f),steel);
        foreach(float offset in new[]{-.16f,.16f})Box("Isolation relief identity bracket",new Vector3(5.92f,1.98f,-44+offset),new Vector3(.23f,.35f,.035f),steel);
        var entry=Point("Isolation physical entry",new Vector3(0,1.1f,-29.2f)).gameObject.AddComponent<BoxCollider>();entry.isTrigger=true;entry.size=new Vector3(15.5f,2.4f,3);
        var origin=Point("Isolation safe continuation",new Vector3(0,.03f,-29));origin.rotation=Quaternion.Euler(0,180,0);
        var door=(SlidingDoor)ds.FindProperty("doors").GetArrayElementAtIndex(1).objectReferenceValue;
        var doorAudio=Point("Isolation physical bulkhead sound",new Vector3(0,2.85f,-48)).gameObject.AddComponent<SlidingDoorAudio>();var doorSource=Source("Isolation bulkhead motor",doorAudio.transform.position,2,14);
        Ref(doorAudio,"door",door);Ref(doorAudio,"doorBody",new SerializedObject(door).FindProperty("doorBody").objectReferenceValue);Ref(doorAudio,"mechanismSource",doorSource);Ref(doorAudio,"movementLoopClip",Clip("Mechanisms/SFX_DoorMovement_LP.wav"));Ref(doorAudio,"endpointClip",Clip("Mechanisms/SFX_DoorEndpoint.wav"));
        var doorSound=new SerializedObject(doorAudio);doorSound.FindProperty("mechanismVolume").floatValue=.13f;doorSound.ApplyModifiedPropertiesWithoutUndo();
        Ref(isolation,"director",director);Ref(isolation,"session",session);Ref(isolation,"player",player);Ref(isolation,"playerInput",player.GetComponent<PlayerInput>());Ref(isolation,"view",player.GetComponentInChildren<Camera>());Ref(isolation,"loop",Object.FindFirstObjectByType<LoopController>());Ref(isolation,"recorder",Object.FindFirstObjectByType<EchoRecorder>());Ref(isolation,"entryZone",entry);Ref(isolation,"manualControl",control);Ref(isolation,"gripPoint",grip);Ref(isolation,"continuationOrigin",origin);Ref(isolation,"exitDoor",door);Ref(isolation,"exitDoorAudio",doorAudio);Arr(isolation,"pressureNeedles",needles);
        var config=new SerializedObject(isolation);config.FindProperty("controlLocalAxis").vector3Value=controlAxis;config.FindProperty("needleLocalAxis").vector3Value=needleAxis;config.FindProperty("controlTravelDegrees").floatValue=70;config.FindProperty("lowNeedleAngle").floatValue=270;config.FindProperty("highNeedleAngle").floatValue=0;config.ApplyModifiedPropertiesWithoutUndo();
        var work=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.name=="Practical light"&&l.transform.position.z< -27&&l.transform.position.z> -48).ToArray();Arr(isolation,"workLights",work);Arr(isolation,"workLampEmissions",Array.Empty<Object>());
        foreach(var light in work)
        {
            light.color=new Color(.76f,.83f,.75f);
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>Vector3.Distance(r.bounds.center,light.transform.position)<1f))
            for(int i=0;i<r.sharedMaterials.Length;i++)if(r.sharedMaterials[i]&&r.sharedMaterials[i].HasProperty("_EmissionColor")&&r.sharedMaterials[i].GetColor("_EmissionColor").maxColorComponent>.01f&&!r.GetComponent<PracticalEmission>())Track(r,light,r.sharedMaterials[i].color,r.sharedMaterials[i].GetColor("_EmissionColor"),i);
        }
        var audio=root.gameObject.AddComponent<PressureIsolationAudio>();Ref(audio,"isolation",isolation);Ref(audio,"session",session);var motor=(AudioSource)ds.FindProperty("machinery").objectReferenceValue;motor.transform.position=new Vector3(6.89f,.55f,-36.5f);motor.playOnAwake=false;motor.spatialBlend=1;motor.dopplerLevel=0;motor.rolloffMode=AudioRolloffMode.Linear;motor.minDistance=1.5f;motor.maxDistance=18;
        Ref(audio,"motorSource",motor);Ref(audio,"controlSource",Source("Isolation spindle friction",new Vector3(5.7f,1.5f,-36.5f),1,4,.9f));Ref(audio,"latchSource",Source("Isolation physical detent",new Vector3(5.7f,1.5f,-36.5f),1,7));Ref(audio,"purgeSource",Source("Isolation automatic purge B",new Vector3(7.3f,1.85f,-44),1,7));
        Ref(audio,"motorLoopClip",Clip("Finish/PressureBed.wav"));Ref(audio,"controlLoopClip",Clip("Mechanisms/SFX_DoorMovement_LP.wav"));Ref(audio,"latchClip",Clip("Finish/Relay.wav"));Ref(audio,"purgeLoopClip",Clip("Overhaul/ValveFlow_LP.wav"));
        var boundary=new SerializedObject(Object.FindFirstObjectByType<ReturnRecordingBoundary>());boundary.FindProperty("followupObjective").stringValue="Volvé al vestíbulo. Bajá al subsuelo.";boundary.ApplyModifiedPropertiesWithoutUndo();ds.Update();ds.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Isolation.v1";ds.ApplyModifiedPropertiesWithoutUndo();
        EmergencyRoute();InstrumentReserve(-36.5f,1.5f);InstrumentReserve(-44,1.73f);var budget=new SerializedObject(Object.FindFirstObjectByType<LightShadowBudget>());var array=budget.FindProperty("lights");int count=array.arraySize;array.arraySize+=newLights.Count;for(int i=0;i<newLights.Count;i++)array.GetArrayElementAtIndex(count+i).objectReferenceValue=newLights[i];budget.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Isolation authored; inspect manual motion, gauge arcs, light ownership and actual player route before claiming playable.");
    }
    static void Materials()
    {
        mats["IC_Enamel"]=Mat("IC_Enamel",new Color(0.1350f,0.1700f,0.1480f).gamma,0.4300f,0.3000f);
        mats["IC_DoorEnamel"]=Mat("IC_DoorEnamel",new Color(0.2550f,0.2730f,0.2180f).gamma,0.4000f,0.1800f);
        mats["IC_EdgeMetal"]=Mat("IC_EdgeMetal",new Color(0.2550f,0.2680f,0.2330f).gamma,0.5700f,0.7700f);
        mats["IC_DarkMetal"]=Mat("IC_DarkMetal",new Color(0.0610f,0.0740f,0.0640f).gamma,0.4500f,0.5800f);
        mats["IC_Bakelite"]=Mat("IC_Bakelite",new Color(0.0300f,0.0190f,0.0110f).gamma,0.6000f,0.0000f);
        mats["IC_Rubber"]=Mat("IC_Rubber",new Color(0.0110f,0.0130f,0.0110f).gamma,0.1600f,0.0000f);
        mats["IC_DialIvory"]=Mat("IC_DialIvory",new Color(0.6700f,0.6530f,0.5420f).gamma,0.2100f,0.0000f);
        mats["IC_DialInk"]=Mat("IC_DialInk",new Color(0.0100f,0.0130f,0.0100f).gamma,0.3000f,0.0000f);
        mats["IC_Needle"]=Mat("IC_Needle",new Color(0.2050f,0.0520f,0.0210f).gamma,0.5300f,0.2000f);
        mats["IC_Brass"]=Mat("IC_Brass",new Color(0.2600f,0.1810f,0.0810f).gamma,0.5100f,0.7000f);
    }
}

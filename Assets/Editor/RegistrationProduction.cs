using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Presentation;
using Object = UnityEngine.Object;

// All authoring goes through the Editor. Reception remains a recoverable scene.
public static class RegistrationProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Registration.unity";
    const string Art="Assets/_Game/Art/Registration";
    static Transform root;
    static TextMesh template;
    static Material plaster,timber,floor,joinery,metal,glass,paper,black;
    static readonly Dictionary<string,Material> slots=new Dictionary<string,Material>();
    static Material Load(string p)=>AssetDatabase.LoadAssetAtPath<Material>(p);
    static Material Mat(string n,Color c,float smooth=.25f,float metallic=0)
    {var p=Art+"/M_"+n+".mat";var m=Load(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);return m;}
    static GameObject Box(string n,Vector3 p,Vector3 size,Material m,bool solid=false)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());if(m==plaster||m==floor||m==timber)g.AddComponent<SurfaceMapping>().Configure(g.GetComponent<Renderer>(),size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/2.5f,size.z/2.5f):new Vector2(Mathf.Max(size.x,size.z)/2.5f,size.y/2.5f));return g;}
    static TextMesh Text(string name,string words,Vector3 p,float yaw,float width,float size=.026f)
    {var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.69f,.71f,.60f);float w=t.GetComponent<Renderer>().localBounds.size.x;if(w>width)t.characterSize*=width/w;return t;}
    static GameObject Model(string family,string name,Vector3 p,float yaw)
    {
        string prefix=family=="ReceptionLife"?"RL":"OV";
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/"+family+"/EP_"+prefix+"_"+name+".fbx");
        if(!asset)throw new Exception("Missing original model "+family+"/"+name);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name="Registration original / "+name;g.transform.SetParent(root);
        var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;
        g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>slots.TryGetValue(m.name,out var value)?value:throw new Exception("Unmapped material "+m.name)).ToArray();
        return g;
    }
    static Light Lamp(string name,Vector3 p,Vector3 aim,float strength,float range)
    {
        Box(name+" housing",p+Vector3.up*.045f,new Vector3(.45f,.085f,.24f),metal);
        var opal=Mat("WarmOpal",new Color(.68f,.61f,.44f));opal.EnableKeyword("_EMISSION");opal.SetColor("_EmissionColor",new Color(.72f,.56f,.32f)*.35f);opal.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;opal.EnableKeyword("_EMISSION");EditorUtility.SetDirty(opal);
        var face=Box(name+" diffuser",p,new Vector3(.38f,.018f,.18f),opal);
        var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p-Vector3.up*.06f;g.transform.LookAt(aim);
        var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=new Color(1,.79f,.52f);l.intensity=strength;l.range=range;l.spotAngle=105;l.innerSpotAngle=75;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.10f;
        var track=face.AddComponent<PracticalEmission>();var so=new SerializedObject(track);so.FindProperty("source").objectReferenceValue=l;so.FindProperty("surface").objectReferenceValue=face.GetComponent<Renderer>();so.ApplyModifiedPropertiesWithoutUndo();return l;
    }
    static void Materials()
    {
        plaster=Load("Assets/_Game/Art/Overhaul/M_Plaster.mat");floor=Load("Assets/_Game/Art/Finish/M_Linoleum.mat");timber=Load("Assets/_Game/Art/Threshold/M_FurnitureWalnut.mat");
        joinery=Mat("OfficePaint",new Color(.25f,.28f,.23f),.31f);metal=Mat("OfficeSteel",new Color(.105f,.12f,.10f),.45f,.5f);paper=Mat("OfficePaper",new Color(.67f,.61f,.45f),.12f);black=Mat("OfficeBakelite",new Color(.025f,.032f,.027f),.22f);
        var gp=Art+"/M_OfficeGlass.mat";glass=Load(gp);if(!glass){glass=new Material(Load("Assets/_Game/Art/Finish/M_ObservationGlass.mat"));AssetDatabase.CreateAsset(glass,gp);}glass.SetColor("_BaseColor",new Color(.43f,.50f,.43f,.07f));
        slots.Clear();foreach(var path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Art/Overhaul"}).Select(AssetDatabase.GUIDToAssetPath)){var m=Load(path);if(m.name.StartsWith("M_OV_"))slots[m.name.Substring(2)]=m;}
        slots["OV_Walnut"]=slots["OV_WalnutEdge"]=timber;
        LifeMaterials();
    }
    static void LifeMaterials()
    {
        // Manifest linear colors, converted to Unity Inspector color convention.
        slots["RL_ChairCloth"]=Mat("RL_ChairCloth",new Color(0.22f,0.155f,0.088f).gamma,0.100f,0f);
        slots["RL_ChairWelt"]=Mat("RL_ChairWelt",new Color(0.104f,0.084f,0.049f).gamma,0.160f,0f);
        slots["RL_Armrest"]=Mat("RL_Armrest",new Color(0.047f,0.041f,0.025f).gamma,0.420f,0f);
        slots["RL_BlackSteel"]=Mat("RL_BlackSteel",new Color(0.035f,0.044f,0.035f).gamma,0.520f,0.6f);
        slots["RL_WornSteel"]=Mat("RL_WornSteel",new Color(0.205f,0.225f,0.202f).gamma,0.550f,0.76f);
        slots["RL_Rubber"]=Mat("RL_Rubber",new Color(0.011f,0.016f,0.012f).gamma,0.170f,0f);
        slots["RL_JacketWool"]=Mat("RL_JacketWool",new Color(0.104f,0.125f,0.104f).gamma,0.070f,0f);
        slots["RL_JacketLining"]=Mat("RL_JacketLining",new Color(0.035f,0.046f,0.036f).gamma,0.390f,0f);
        slots["RL_JacketEdge"]=Mat("RL_JacketEdge",new Color(0.058f,0.072f,0.056f).gamma,0.080f,0f);
        slots["RL_HornButton"]=Mat("RL_HornButton",new Color(0.034f,0.029f,0.018f).gamma,0.600f,0f);
        slots["RL_HangerWood"]=Mat("RL_HangerWood",new Color(0.17f,0.088f,0.034f).gamma,0.520f,0f);
        slots["RL_ClockEnamel"]=Mat("RL_ClockEnamel",new Color(0.214f,0.247f,0.207f).gamma,0.560f,0.18f);
        slots["RL_ClockDial"]=Mat("RL_ClockDial",new Color(0.61f,0.598f,0.476f).gamma,0.290f,0f);
        slots["RL_ClockInk"]=Mat("RL_ClockInk",new Color(0.014f,0.02f,0.015f).gamma,0.400f,0f);
    }
    public static int RepairVisibleEmission()
    {
        var renderers=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
        var replacements=new Dictionary<Material,Material>();
        foreach(var old in renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m&&m.shader.name=="Universal Render Pipeline/Lit"&&m.HasProperty("_EmissionColor")&&!m.IsKeywordEnabled("_EMISSION")&&m.GetColor("_EmissionColor").maxColorComponent>.01f).Distinct())
        {
            string p=Art+"/M_Emissive_"+old.name+".mat";var m=Load(p);if(!m){m=new Material(old);AssetDatabase.CreateAsset(m,p);}
            m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);replacements[old]=m;
        }
        foreach(var r in renderers)r.sharedMaterials=r.sharedMaterials.Select(m=>m&&replacements.TryGetValue(m,out var replacement)?replacement:m).ToArray();
        return replacements.Count;
    }
    public static void ConnectReturnDoorSound()
    {
        var owner=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Registration / inhabited administrative booth");
        foreach(string name in new[]{"R3 arrival rear","R3 Hub exit only","R3 permanently unlocked exit"})
        {
            if(Object.FindObjectsByType<EchoProtocol.Audio.SlidingDoorAudio>(FindObjectsSortMode.None).Any(a=>a.name==name+" physical motor"))continue;
            var door=GameObject.Find(name).GetComponent<EchoProtocol.Puzzle.SlidingDoor>();
            var body=(Rigidbody)new SerializedObject(door).FindProperty("doorBody").objectReferenceValue;
            if(!body||!body.isKinematic)throw new Exception("Missing authored body for "+name);
            var g=new GameObject(name+" physical motor");g.transform.SetParent(owner);g.transform.position=door.transform.position+Vector3.up*2.75f;
            var source=g.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=1;source.dopplerLevel=0;
            source.outputAudioMixerGroup=AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();
            var audio=g.AddComponent<EchoProtocol.Audio.SlidingDoorAudio>();var so=new SerializedObject(audio);
            so.FindProperty("door").objectReferenceValue=door;so.FindProperty("doorBody").objectReferenceValue=body;so.FindProperty("mechanismSource").objectReferenceValue=source;
            so.FindProperty("movementLoopClip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav");
            so.FindProperty("endpointClip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav");
            so.FindProperty("mechanismVolume").floatValue=.13f;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    [MenuItem("Echo Protocol/Author inhabited registration")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before registration authoring");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(ReceptionProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        root=new GameObject("Registration / inhabited administrative booth").transform;
        template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");Materials();
        var formerOpal=Load("Assets/_Game/Art/Reception/M_InstitutionOpal.mat");var fixedOpal=Load(Art+"/M_InstitutionOpal.mat");if(!fixedOpal){fixedOpal=new Material(formerOpal);AssetDatabase.CreateAsset(fixedOpal,Art+"/M_InstitutionOpal.mat");}fixedOpal.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;fixedOpal.EnableKeyword("_EMISSION");EditorUtility.SetDirty(fixedOpal);
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))r.sharedMaterials=r.sharedMaterials.Select(m=>m==formerOpal?fixedOpal:m).ToArray();
        // The first obsolete glass bay lies entirely within the new non-walkable office.
        // Preserve all original collision and recording references, including the glass at A/B.
        GameObject.Find("Hub North Center").GetComponent<Renderer>().enabled=false;
        foreach(var r in GameObject.Find("Observation Frame Bay -1").GetComponentsInChildren<Renderer>())r.enabled=false;
        GameObject.Find("Observation Glass 1").GetComponent<Renderer>().enabled=false;
        GameObject.Find("Finish ReceptionDesk").SetActive(false);
        var oldCollision=GameObject.Find("Reception desk solid").GetComponent<BoxCollider>();oldCollision.enabled=false;
        GameObject.Find("Institution brass letters").SetActive(false);
        Box("Office structural floor",new Vector3(-.05f,-.075f,-1.50f),new Vector3(3.32f,.15f,4.90f),floor,true);
        Box("Office ceiling",new Vector3(-.05f,3.08f,-1.5f),new Vector3(3.32f,.16f,4.90f),plaster,true);
        foreach(float x in new[]{-1.71f,1.61f})
        {
            Box("Office side enclosure",new Vector3(x,1.5f,-1.5f),new Vector3(.12f,3,4.90f),plaster,true);
            Box("Office side painted dado",new Vector3(x,.48f,-1.5f),new Vector3(.135f,.96f,4.9f),joinery);
            Box("Office side rail",new Vector3(x,.98f,-1.5f),new Vector3(.16f,.035f,4.9f),timber);
        }
        Box("Office back wall",new Vector3(-.05f,1.5f,.94f),new Vector3(3.44f,3,.12f),plaster,true);
        Box("Office rear dado",new Vector3(-.05f,.48f,.864f),new Vector3(3.24f,.96f,.03f),joinery);
        Box("Office rear rail",new Vector3(-.05f,.98f,.846f),new Vector3(3.26f,.04f,.055f),timber);
        // The counter itself closes the lower front; glazing sits above its worktop.
        foreach(float x in new[]{-1.69f,1.59f})Box("Registration front pier",new Vector3(x,1.50f,-3.92f),new Vector3(.24f,3,.22f),plaster);
        Box("Registration front header",new Vector3(-.05f,2.86f,-3.92f),new Vector3(3.16f,.40f,.22f),plaster);
        Box("Office glazing",new Vector3(-.05f,1.885f,-3.90f),new Vector3(3.02f,1.55f,.024f),glass);
        foreach(float x in new[]{-1.57f,.47f,1.47f})Box("Office window upright",new Vector3(x,1.885f,-3.928f),new Vector3(.065f,1.59f,.075f),joinery);
        foreach(float y in new[]{1.085f,2.67f})Box("Office window rail",new Vector3(-.05f,y,-3.928f),new Vector3(3.10f,.065f,.075f),joinery);
        // A small counter opening gives the letter a physical address at the window.
        Box("Correspondence slot trim",new Vector3(.95f,1.10f,-3.97f),new Vector3(.68f,.035f,.16f),metal);
        var counter=Model("Overhaul","ReceptionCounter",new Vector3(0,0,-4.10f),180);
        Box("Registration counter front collision",new Vector3(0,.535f,-4.30f),new Vector3(2.40f,1.07f,.90f),black,true).GetComponent<Renderer>().enabled=false;
        Box("Registration counter return collision",new Vector3(-.97f,.52f,-3.30f),new Vector3(.44f,1.04f,.94f),black,true).GetComponent<Renderer>().enabled=false;
        var letter=GameObject.Find("Finish Clipboard");letter.transform.position=new Vector3(.35f,1.102f,-4.39f);
        GameObject.Find("Mara letter / reception").transform.position=letter.transform.position;
        var belongings=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Finish TableBelongings"&&t.position.z> -10);
        belongings.position=new Vector3(-.49f,1.071f,-4.10f);
        Model("ReceptionLife","ClericalChair",new Vector3(.27f,0,-2.37f),205);
        Model("ReceptionLife","CoatRackAndJacket",new Vector3(1.12f,0,.23f),210);
        Model("ReceptionLife","WallClock0317",new Vector3(.74f,2.31f,.826f),180);
        Model("Overhaul","KeyMailCabinet",new Vector3(-.79f,1.28f,.81f),180);
        // Closed staff access is articulated in the left enclosure, beyond the two public paths.
        Box("Office staff door",new Vector3(-1.636f,1.065f,-.65f),new Vector3(.035f,2.13f,.82f),joinery);
        foreach(float z in new[]{-1.10f,-.20f})Box("Office staff architrave",new Vector3(-1.616f,1.10f,z),new Vector3(.055f,2.20f,.065f),timber);
        Box("Office staff lintel",new Vector3(-1.616f,2.18f,-.65f),new Vector3(.055f,.07f,.965f),timber);
        Box("Office staff handle",new Vector3(-1.58f,1,-.96f),new Vector3(.045f,.025f,.14f),metal);
        // A shallow files ledge supports the mail cabinet; irregular papers establish use.
        Box("Office rear work ledge",new Vector3(-.65f,.82f,.58f),new Vector3(1.68f,.06f,.54f),timber);
        foreach(float x in new[]{-1.35f,.05f})Box("Office ledge bracket",new Vector3(x,.63f,.76f),new Vector3(.035f,.34f,.16f),metal);
        for(int i=0;i<4;i++){var file=Box("Pending paper file",new Vector3(-.94f+i*.20f,.93f,.54f),new Vector3(.16f,.16f,.27f),paper);file.transform.rotation=Quaternion.Euler(0,i*3-6,0);}
        Text("Registration institutional identity","E C H O  /  REVISIÓN",new Vector3(-.05f,2.86f,-4.047f),0,2.60f,.035f);
        Text("Office glazing identity","RECEPCIÓN",new Vector3(.95f,1.85f,-3.952f),0,.77f,.023f);
        var a=Lamp("Office warm overhead",new Vector3(-.63f,2.88f,-1.58f),new Vector3(-.4f,1.25f,.70f),4.2f,5.1f);
        var b=Lamp("Registration counter task light",new Vector3(.95f,2.54f,-3.74f),new Vector3(.15f,.85f,-4.38f),1.50f,3.2f);
        Box("Counter light wall arm",new Vector3(.95f,2.55f,-3.86f),new Vector3(.04f,.04f,.25f),metal);
        var budget=Object.FindFirstObjectByType<LightShadowBudget>();var bs=new SerializedObject(budget);var list=bs.FindProperty("lights");int n=list.arraySize;list.arraySize+=2;list.GetArrayElementAtIndex(n).objectReferenceValue=a;list.GetArrayElementAtIndex(n+1).objectReferenceValue=b;bs.ApplyModifiedPropertiesWithoutUndo();
        RepairVisibleEmission();
        ConnectReturnDoorSound();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Registration office authored; inspect first-use camera, letter ray, two public paths and return before checkpoint.");
    }
}

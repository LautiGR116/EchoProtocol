using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using EchoProtocol.Player;
using EchoProtocol.Audio;
using Object=UnityEngine.Object;

// A recoverable production branch. Finish is loaded read-only, then saved to a new scene.
public static class ResidenceProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Residence.unity";
    const string Art="Assets/_Game/Art/Overhaul";
    static Transform root; static TextMesh template; static CampaignDirector director;
    static Material paper,wood,paint,trim,steel,ivory,cloth,dark,ceiling,tile,glass;
    static readonly List<Light> shadowLights=new List<Light>();
    static void Ref(Object o,string n,Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Arr(Object o,string n,IEnumerable<Object> values){var s=new SerializedObject(o);var p=s.FindProperty(n);var v=values.ToArray();p.arraySize=v.Length;for(int i=0;i<v.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=v[i];s.ApplyModifiedPropertiesWithoutUndo();}
    static void Set(Object o,string n,object value){var s=new SerializedObject(o);var p=s.FindProperty(n);if(value is bool b)p.boolValue=b;else if(value is int i)p.intValue=i;else if(value is float f)p.floatValue=f;else if(value is string t)p.stringValue=t;else if(value is Vector3 v)p.vector3Value=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static Material Mat(string n,Color c,float smooth=.25f,float metal=0)
    {string path=Art+"/M_"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);return m;}
    static Material Copy(string name,string source,Color tint)
    {var path=Art+"/M_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(AssetDatabase.LoadAssetAtPath<Material>(source));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",tint);return m;}
    static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=true)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());if(m&&m.GetTexture("_BaseMap")){var repeat=s.y<Mathf.Min(s.x,s.z)?new Vector2(s.x/2.5f,s.z/2.5f):new Vector2(Mathf.Max(s.x,s.z)/(m==paper?1.6f:3f),s.y/(m==paper?1.6f:3f));g.AddComponent<SurfaceMapping>().Configure(g.GetComponent<Renderer>(),repeat);}return g;}
    static void Wall(string n,float x,float z,float w,float d,Material material=null)
    {
        Box(n,new Vector3(x,1.36f,z),new Vector3(w,2.72f,d),material?material:paper);
        Box(n+" skirting",new Vector3(x,.075f,z),new Vector3(w+.03f,.15f,d+.03f),wood,false);
        Box(n+" cornice",new Vector3(x,2.61f,z),new Vector3(w+.035f,.06f,d+.035f),trim,false);
    }
    static TextMesh Label(string n,string words,Vector3 p,float yaw,float width,Color color)
    {var t=Object.Instantiate(template,root);t.name=n;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=64;t.characterSize=.018f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;float w=t.GetComponent<Renderer>().localBounds.size.x;if(w>width)t.characterSize*=width/w;return t;}
    static GameObject Prop(string type,Vector3 p,float yaw=0,bool solid=true,string family="Campaign")
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/"+family+"/EP_"+type+".fbx");if(!source)throw new Exception("Missing model "+family+"/"+type);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Residence "+type;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw+(family=="Finish"&&(type=="Clipboard"||type=="WallSconce")?180:0),0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name.Contains("Wood")?wood:m.name.Contains("Fabric")||m.name.Contains("Cloth")?cloth:m.name.Contains("Ivory")||m.name.Contains("Enamel")?ivory:m.name.Contains("Black")?dark:m.name.Contains("Glass")?glass:steel).ToArray();
        if(solid){var renderers=g.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);var c=new GameObject(type+" simple collision");c.transform.SetParent(root);c.transform.position=bounds.center;var box=c.AddComponent<BoxCollider>();box.size=bounds.size;}
        return g;
    }
    static Light Light(string n,Vector3 p,Color color,float intensity,float range,Vector3? aim=null,float angle=100)
    {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;var l=g.AddComponent<Light>();l.type=aim.HasValue?LightType.Spot:LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.Soft;l.shadowBias=.012f;l.shadowNormalBias=.12f;if(aim.HasValue){g.transform.LookAt(aim.Value);l.spotAngle=angle;l.innerSpotAngle=angle*.75f;}shadowLights.Add(l);return l;}
    static Light Practical(string n,Vector3 p,float intensity,float range)
    {
        var model=Prop("LampTable",p,0,true);var l=Light(n,p+new Vector3(0,1.22f,0),new Color(1,.84f,.65f),intensity,range);
        var shade=Mat("GlowingLinenShade",new Color(.70f,.59f,.40f),.12f);shade.EnableKeyword("_EMISSION");shade.SetColor("_EmissionColor",new Color(.25f,.19f,.10f));
        foreach(var r in model.GetComponentsInChildren<Renderer>()){
            var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]==ivory){mats[i]=shade;var tracking=r.gameObject.AddComponent<PracticalEmission>();Ref(tracking,"source",l);Ref(tracking,"surface",r);Set(tracking,"materialIndex",i);var s=new SerializedObject(tracking);s.FindProperty("litColor").colorValue=new Color(.7f,.59f,.4f);s.FindProperty("emission").colorValue=new Color(.25f,.19f,.10f);s.ApplyModifiedPropertiesWithoutUndo();}r.sharedMaterials=mats;
        }return l;
    }
    static void OpeningWall(float x,float start,float end,float opening,float width=.98f)
    {float a=opening+width/2,b=opening-width/2;Wall("Domestic hall partition",x,(start+a)/2,.18f,start-a);Wall("Domestic hall partition",x,(end+b)/2,.18f,b-end);Box("Doorway structural lintel",new Vector3(x,2.435f,opening),new Vector3(.18f,.57f,width),paper);Frame(new Vector3(x,0,opening),90,width);}
    static void Frame(Vector3 p,float yaw,float width)
    {for(int side=-1;side<=1;side+=2){var g=Box("Painted door architrave",Vector3.zero,new Vector3(.095f,2.2f,.23f),trim,false);g.transform.SetPositionAndRotation(p+Quaternion.Euler(0,yaw,0)*new Vector3(side*(width/2+.028f),1.1f,0),Quaternion.Euler(0,yaw,0));}var top=Box("Painted door architrave head",p+Vector3.up*2.17f,new Vector3(width+.15f,.09f,.23f),trim,false);top.transform.rotation=Quaternion.Euler(0,yaw,0);}
    static void Window(float x,float z,float facing)
    {
        // Real opening; no opaque wall hidden behind the window. A separate facade supplies exterior depth.
        var p=new Vector3(x,0,z);Own("DressedWindow",new Vector3(x,0,z),facing);
        var cold=new Color(.51f,.62f,.74f);var inward=x<0?Vector3.right:Vector3.left;
        Light("Courtyard reflected spill",new Vector3(x,2.05f,z)+inward*.1f,cold,2.6f,7,new Vector3(x,2.05f,z)+inward*4+Vector3.down*1.7f,85);
        var exterior=Mat("CourtyardNight",new Color(.65f,.72f,.8f),.1f);exterior.shader=Shader.Find("Universal Render Pipeline/Unlit");exterior.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/CourtyardNight.png"));
        var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="Courtyard opposite facade";q.transform.SetParent(root);q.transform.SetPositionAndRotation(new Vector3(x+(x<0?-4:4),2.5f,z),Quaternion.Euler(0,x<0?270:90,0));q.transform.localScale=new Vector3(12,8,1);q.GetComponent<Renderer>().sharedMaterial=exterior;Object.DestroyImmediate(q.GetComponent<Collider>());
        // Deep reveal and sill make the flat distant image an exterior, not a luminous wall panel.
        Box("Window sill",new Vector3(x,.85f,z),new Vector3(.42f,.07f,1.9f),trim);
    }
    static void ExteriorWall(float x,float start,float end,float window)
    {float w=1.8f;Wall("Exterior wall",x,(start+window+w/2)/2,.22f,start-window-w/2);Wall("Exterior wall",x,(end+window-w/2)/2,.22f,window-w/2-end);Box("Window parapet",new Vector3(x,.43f,window),new Vector3(.22f,.86f,w),paper);Box("Window head",new Vector3(x,2.59f,window),new Vector3(.22f,.26f,w),paper);Window(x,window,x<0?90:270);}
    static AudioSource Audio(string n,Vector3 p,string clip,bool looping=false,float gain=.15f)
    {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;var a=g.AddComponent<AudioSource>();var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer");a.outputAudioMixerGroup=mixer.FindMatchingGroups(looping?"Ambience":"WorldSFX").First();a.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Overhaul/"+clip+".wav");a.playOnAwake=false;a.loop=looping;a.spatialBlend=1;a.minDistance=1;a.maxDistance=7;a.rolloffMode=AudioRolloffMode.Linear;a.volume=gain;return a;}
    [MenuItem("Echo Protocol/Author residence overhaul")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(FinishProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        director=Object.FindFirstObjectByType<CampaignDirector>();template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");
        // Keep every previous reference recoverable but remove the old pavilion from this edition.
        foreach(var parent in scene.GetRootGameObjects().Where(g=>g.name.StartsWith("Campaign /")||g.name.StartsWith("Finish /")))foreach(Transform child in parent.transform)
            if(child.position.z<=-49&&child.position.z>=-74.2f && child.name!="Station / HouseOrigin")child.gameObject.SetActive(false);
        root=new GameObject("Residence / habitable reconstruction").transform;shadowLights.Clear();
        paper=Mat("FloralPaper",new Color(.82f,.83f,.77f),.1f);paper.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/FloralWallpaper.png"));wood=Copy("Walnut","Assets/_Game/Art/Finish/M_Walnut.mat",new Color(.68f,.6f,.48f));paint=Copy("Plaster","Assets/_Game/Art/Finish/M_ClinicPaint.mat",new Color(.74f,.77f,.72f));
        trim=Mat("PaintedTrim",new Color(.45f,.43f,.36f),.34f);steel=Mat("OldMetal",new Color(.2f,.22f,.21f),.36f,.55f);ivory=Mat("Enamel",new Color(.76f,.70f,.55f),.3f);cloth=Mat("TobaccoCloth",new Color(.29f,.19f,.11f),.06f);dark=Mat("Bakelite",new Color(.025f,.029f,.028f),.32f);ceiling=Mat("Ceiling",new Color(.6f,.60f,.54f),.05f);tile=Mat("KitchenTile",new Color(.43f,.46f,.38f),.48f);glass=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Finish/M_ObservationGlass.mat");OwnMaterials();
        Box("Entrance bridge floor",new Vector3(0,-.10f,-48.5f),new Vector3(2.4f,.2f,1),wood);
        Box("Entrance bridge ceiling",new Vector3(0,2.79f,-48.5f),new Vector3(2.4f,.14f,1),ceiling);
        for(int side=-1;side<=1;side+=2)Wall("Entrance bridge side",side*1.2f,-48.5f,.2f,1,paint);
        Box("Residence continuous timber",new Vector3(0,-.10f,-55.75f),new Vector3(10.8f,.2f,13.5f),wood);
        Box("Residence plaster ceiling",new Vector3(0,2.79f,-55.75f),new Vector3(10.8f,.14f,13.5f),ceiling);
        for(int side=-1;side<=1;side+=2){Wall("Entrance wall",side*3.16f,-49,4.28f,.2f);ExteriorWall(side*5.4f,-49,-55.3f,-52);Wall("Rear exterior",side*5.4f,-57.9f,.2f,5.2f);Wall("Room division",side*3.22f,-55.3f,4.36f,.16f);OpeningWall(side*1.0f,-49,-55.3f,side<0?-52.3f:-53.3f);OpeningWall(side*1.0f,-55.3f,-60.6f,side<0?-58.2f:-58.8f,side<0?.98f:1.8f);Wall("Rear room wall",side*3.22f,-60.6f,4.36f,.18f);Wall("Rear hall wall",side*1,-61.55f,.18f,1.9f);Wall("Exit wall",side*.775f,-62.5f,.45f,.18f);}
        Box("Entrance lintel",new Vector3(0,2.47f,-49),new Vector3(2.02f,.5f,.2f),paper);Frame(new Vector3(0,0,-49),0,1.98f);
        // The former 25 m exhibition house is now a compact dwelling with a separate service return.
        Box("Service connection floor",new Vector3(0,-.10f,-68.25f),new Vector3(2.0f,.2f,11.5f),paint);Box("Service connection ceiling",new Vector3(0,2.79f,-68.25f),new Vector3(2.2f,.14f,11.5f),ceiling);
        for(int side=-1;side<=1;side+=2)Wall("Service connection wall",side*1.1f,-68.25f,.2f,11.5f,paint);
        Light("Service passage fixture",new Vector3(0,2.45f,-68.5f),new Color(.63f,.71f,.69f),1.4f,6);Prop("Fluorescent",new Vector3(0,2.6f,-68.5f),0,false);
        // Groups with close surfaces and offset sightlines, each establishing a different room function.
        Prop("TelephoneTable",new Vector3(-4.4f,0,-51.9f));Prop("Telephone",new Vector3(-4.4f,.76f,-51.9f),35,false);
        Own("Armchair",new Vector3(-3.85f,0,-54.15f),25,true);Prop("Bookshelf",new Vector3(-2.05f,0,-54.95f),0);
        Box("Parlor woven rug",new Vector3(-3.5f,.012f,-52.75f),new Vector3(2.5f,.02f,2.65f),cloth,false);
        Prop("TableBelongings",new Vector3(-4.38f,.775f,-51.95f),25,false,"Finish");
        var parlor=Practical("Parlor table lamp",new Vector3(-4.75f,0,-54.5f),2.5f,4.8f);
        Prop("Bed",new Vector3(3.35f,0,-53.4f),180);Prop("Locker",new Vector3(4.95f,0,-54.4f),270);
        var bedroom=Practical("Bedroom reserve lamp",new Vector3(4.7f,0,-50.4f),2.9f,5.2f);
        var console=Own("KeyConsole",new Vector3(3.415f,0,-49.6f),180,true);
        foreach(var t in console.GetComponentsInChildren<Transform>())if(t.name=="Key")t.gameObject.SetActive(false);
        Own("OpenSuitcase",new Vector3(4.4f,0,-49.8f),165,true);
        Prop("Kitchen",new Vector3(-3.65f,0,-60.1f));Prop("TelephoneTable",new Vector3(-3.3f,0,-56.4f));Prop("TableBelongings",new Vector3(-3.3f,.775f,-56.4f),110,false,"Finish");
        Box("Kitchen floor",new Vector3(-3.22f,.012f,-57.95f),new Vector3(4.18f,.024f,5.1f),tile,false);
        for(int i=0;i<11;i++)Box("Backsplash grout",new Vector3(-5.15f+i*.35f,1.28f,-60.46f),new Vector3(.007f,.58f,.015f),trim,false);
        Box("Kitchen backsplash",new Vector3(-3.25f,1.28f,-60.48f),new Vector3(4.1f,.58f,.018f),ivory,false);
        Prop("Fluorescent",new Vector3(-3.4f,2.52f,-58.5f),0,false);
        var kitchen=Light("Kitchen reserve fixture",new Vector3(-3.4f,2.30f,-58.5f),new Color(.90f,.91f,.75f),2.3f,5.8f);
        var hall=Light("Hall reserve light",new Vector3(.68f,2.05f,-50.5f),new Color(1,.76f,.46f),2.0f,5.3f);Prop("WallSconce",new Vector3(.86f,2.05f,-50.5f),270,false,"Finish");
        Light("Hall residual from service",new Vector3(0,2.3f,-61.2f),new Color(.43f,.54f,.58f),.32f,6);
        // A closed study is visible through its fixed glass panel. No figure can be walked through.
        Box("Study closed lower door",new Vector3(1, .45f,-58.8f),new Vector3(.075f,.9f,1.75f),trim);
        var studyGlass=Box("Study fixed glazing",new Vector3(1,1.53f,-58.8f),new Vector3(.045f,1.25f,1.68f),glass);
        Prop("ArchiveCabinet",new Vector3(4.9f,0,-59.5f),270);Prop("Bookshelf",new Vector3(4.7f,0,-56.0f),180);Prop("Trolley",new Vector3(3.7f,0,-58.2f),20);
        Light("Study window residual",new Vector3(4.8f,2.3f,-58.8f),new Color(.47f,.55f,.60f),1.1f,5);
        WireHouse(hall,kitchen,bedroom,parlor,studyGlass.GetComponent<Collider>());
        WireAudio();
        // Only this new scene's shadow allocation includes the new lights.
        var budget=Object.FindFirstObjectByType<LightShadowBudget>();var old=new SerializedObject(budget).FindProperty("lights");var all=new List<Object>();for(int i=0;i<old.arraySize;i++)all.Add(old.GetArrayElementAtIndex(i).objectReferenceValue);all.AddRange(shadowLights);Arr(budget,"lights",all);
        var shadowRegion=new GameObject("Residence enclosed shadow region");shadowRegion.transform.SetParent(root);shadowRegion.transform.position=new Vector3(0,1.5f,-55.75f);var region=shadowRegion.AddComponent<BoxCollider>();region.isTrigger=true;region.size=new Vector3(12,5,17);Ref(budget,"enclosedResidence",region);Arr(budget,"residenceLights",shadowLights);
        foreach(var l in shadowLights){var s=new SerializedObject(l.GetUniversalAdditionalLightData());s.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;s.ApplyModifiedPropertiesWithoutUndo();}
        Ref(director,"house",root.GetComponent<HouseMemory>());
        Set(Object.FindFirstObjectByType<GameSession>(),"editionLabel","ARCHIVO 6B / LA CASA");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("Residence authored: compact house, light reserve, key and physical exit. Requires route inspection.");
    }
    static HouseInteraction Interaction(HouseMemory house,HouseInteractionKind kind,Vector3 p,Vector3 size)
    {var g=new GameObject("House interaction / "+kind);g.transform.SetParent(root);g.transform.position=p;var c=g.AddComponent<BoxCollider>();c.size=size;var h=g.AddComponent<HouseInteraction>();Ref(h,"house",house);Set(h,"kind",(int)kind);return h;}
    static GameObject Socket(HouseMemory house,HouseInteractionKind kind,Vector3 p,float yaw,string title)
    {
        var housing=Box("Reserve socket / "+title,p,new Vector3(.26f,.36f,.09f),ivory);housing.transform.rotation=Quaternion.Euler(0,yaw,0);
        var back=Box("Empty reserve cradle",p-housing.transform.forward*.055f,new Vector3(.15f,.21f,.014f),dark,false);back.transform.rotation=housing.transform.rotation;
        var fuse=Box("Single reserve / "+title,p-housing.transform.forward*.09f,new Vector3(.115f,.19f,.075f),steel,false);fuse.transform.rotation=housing.transform.rotation;
        var handle=Box("Reserve ceramic grip",fuse.transform.position-housing.transform.forward*.06f,new Vector3(.08f,.045f,.025f),ivory,false);handle.transform.rotation=housing.transform.rotation;handle.transform.SetParent(fuse.transform,true);
        var h=housing.AddComponent<HouseInteraction>();Ref(h,"house",house);Set(h,"kind",(int)kind);
        Label("Reserve socket label",title,p-housing.transform.forward*.055f+Vector3.down*.145f,yaw,.20f,new Color(.12f,.15f,.13f));
        return fuse;
    }
    static readonly Dictionary<string,Material> ownMaterials=new Dictionary<string,Material>();
    static void OwnMaterials()
    {
        ownMaterials["OV_Walnut"]=Mat("OV_Walnut",new Color(0.105f,0.049f,0.023f).gamma,0.660f,0f);
        ownMaterials["OV_WalnutEdge"]=Mat("OV_WalnutEdge",new Color(0.16f,0.081f,0.037f).gamma,0.680f,0f);
        ownMaterials["OV_PaintedTimber"]=Mat("OV_PaintedTimber",new Color(0.225f,0.254f,0.217f).gamma,0.530f,0f);
        ownMaterials["OV_Linen"]=Mat("OV_Linen",new Color(0.54f,0.49f,0.37f).gamma,0.070f,0f);
        ownMaterials["OV_ChairFabric"]=Mat("OV_ChairFabric",new Color(0.18f,0.23f,0.2f).gamma,0.090f,0f);
        ownMaterials["OV_Piping"]=Mat("OV_Piping",new Color(0.075f,0.105f,0.085f).gamma,0.220f,0f);
        ownMaterials["OV_DullBrass"]=Mat("OV_DullBrass",new Color(0.32f,0.22f,0.1f).gamma,0.660f,0.72f);
        ownMaterials["OV_BlackenedSteel"]=Mat("OV_BlackenedSteel",new Color(0.033f,0.039f,0.037f).gamma,0.530f,0.72f);
        ownMaterials["OV_Porcelain"]=Mat("OV_Porcelain",new Color(0.7f,0.67f,0.57f).gamma,0.730f,0f);
        ownMaterials["OV_Glass"]=Mat("OV_Glass",new Color(0.045f,0.083f,0.087f).gamma,0.850f,0.1f);
        ownMaterials["OV_Leather"]=Mat("OV_Leather",new Color(0.12f,0.055f,0.026f).gamma,0.420f,0f);
        ownMaterials["OV_ClothingBlue"]=Mat("OV_ClothingBlue",new Color(0.13f,0.19f,0.24f).gamma,0.060f,0f);
        ownMaterials["OV_ClothingCream"]=Mat("OV_ClothingCream",new Color(0.52f,0.48f,0.38f).gamma,0.080f,0f);
        ownMaterials["OV_Paper"]=Mat("OV_Paper",new Color(0.61f,0.59f,0.47f).gamma,0.160f,0f);
        ownMaterials["OV_Rubber"]=Mat("OV_Rubber",new Color(0.009f,0.012f,0.011f).gamma,0.270f,0f);
        ownMaterials["OV_Walnut"]=wood;ownMaterials["OV_WalnutEdge"]=wood;
        ownMaterials["OV_Glass"]=glass;
    }
    static GameObject Own(string type,Vector3 p,float yaw=0,bool solid=false)
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Overhaul/EP_OV_"+type+".fbx");
        var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Residence original / "+type;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>ownMaterials.TryGetValue(m.name,out var replacement)?replacement:steel).ToArray();
        if(solid){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var c=new GameObject(type+" collision");c.transform.SetParent(root);c.transform.position=b.center;c.AddComponent<BoxCollider>().size=b.size;}
        return g;
    }
    static void WireHouse(Light hall,Light kitchen,Light bedroom,Light general,Collider sightGlass)
    {
        var session=Object.FindFirstObjectByType<GameSession>();var player=Object.FindFirstObjectByType<FirstPersonController>();
        var house=root.gameObject.AddComponent<HouseMemory>();Ref(house,"session",session);Ref(house,"player",player);Ref(house,"view",player.GetComponentInChildren<Camera>());
        Arr(house,"generalLights",new[]{hall,kitchen,bedroom,general});Arr(house,"hallLights",new[]{hall});Arr(house,"kitchenLights",new[]{kitchen});Arr(house,"bedroomLights",new[]{bedroom});
        Ref(house,"hallFuse",Socket(house,HouseInteractionKind.HallSocket,new Vector3(.88f,1.3f,-50.8f),90,"RECIBIDOR"));
        Ref(house,"kitchenFuse",Socket(house,HouseInteractionKind.KitchenSocket,new Vector3(-5.15f,1.3f,-57.5f),270,"COCINA"));
        Ref(house,"bedroomFuse",Socket(house,HouseInteractionKind.BedroomSocket,new Vector3(5.15f,1.3f,-53f),90,"DORMITORIO"));
        Interaction(house,HouseInteractionKind.Telephone,new Vector3(-4.4f,1.0f,-51.9f),new Vector3(.36f,.30f,.35f));
        var key=Interaction(house,HouseInteractionKind.Key,new Vector3(3.6f,.83f,-49.6f),new Vector3(.24f,.15f,.24f));
        var keyBody=Box("6B brass key",key.transform.position,new Vector3(.025f,.018f,.16f),Mat("KeyBrass",new Color(.47f,.34f,.13f),.48f,.8f),false);keyBody.transform.SetParent(key.transform,true);
        var bow=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bow.name="Key bow";bow.transform.SetParent(key.transform,false);bow.transform.localPosition=new Vector3(0,0,-.09f);bow.transform.localScale=new Vector3(.06f,.009f,.06f);bow.GetComponent<Renderer>().sharedMaterial=keyBody.GetComponent<Renderer>().sharedMaterial;Object.DestroyImmediate(bow.GetComponent<Collider>());
        var tag=Box("6B key paper tag",key.transform.position+new Vector3(.075f,-.01f,-.07f),new Vector3(.09f,.013f,.065f),ivory,false);tag.transform.SetParent(key.transform,true);Ref(house,"keyWorld",key.gameObject);
        var paperNote=Prop("Clipboard",new Vector3(-2.4f,1.5f,-49.15f),180,false,"Finish");
        var note=Interaction(house,HouseInteractionKind.Document,new Vector3(-2.4f,1.5f,-49.22f),new Vector3(.3f,.42f,.045f));
        Set(note,"documentTitle","La luz del recibidor");Set(note,"documentBody","A.,\n\nDejé una reserva en el recibidor. Si vuelve a cortarse la corriente, podés llevarla a cualquiera de las tres cajas. Hay una sola.\n\nLa llave está junto a la maleta. Todavía no terminé de guardar todo.\n\nAl salir, dejá encendida la luz de la entrada. Desde el patio sé si llegaste.\n\nMara");
        // A household exit opens about a real hinge, keeping its collision for the full motion.
        Frame(new Vector3(0,0,-62.5f),0,1.04f);Box("Exit structural lintel",new Vector3(0,2.43f,-62.5f),new Vector3(1.12f,.58f,.18f),paper);
        var hinge=new GameObject("6B exit / left hinge");hinge.transform.SetParent(root);hinge.transform.position=new Vector3(-.49f,0,-62.5f);var body=hinge.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.interpolation=RigidbodyInterpolation.Interpolate;
        var leaf=Box("6B exit leaf",new Vector3(0,1.04f,-62.5f),new Vector3(.98f,2.08f,.055f),trim);leaf.transform.SetParent(hinge.transform,true);
        foreach(float y in new[]{.5f,1.45f}){var panel=Box("Raised domestic door panel",new Vector3(0,y,-62.46f),new Vector3(.72f,.65f,.025f),wood,false);panel.transform.SetParent(hinge.transform,true);}
        var handle=Box("Exit lever",new Vector3(.35f,1.03f,-62.42f),new Vector3(.15f,.025f,.065f),steel,false);handle.transform.SetParent(hinge.transform,true);
        var door=hinge.AddComponent<HouseSwingDoor>();Ref(door,"hingeBody",body);Ref(door,"solidLeaf",leaf.GetComponent<BoxCollider>());Ref(door,"playerBody",player.GetComponent<CharacterController>());Set(door,"openAngle",95f);Set(door,"degreesPerSecond",58f);Ref(house,"exitDoor",door);
        var doorInteract=hinge.AddComponent<HouseInteraction>();Ref(doorInteract,"house",house);Set(doorInteract,"kind",(int)HouseInteractionKind.ExitDoor);
        var phone=Audio("Telephone at window",new Vector3(-4.4f,1,-51.9f),"",false,.20f);Ref(house,"telephoneAudio",phone);Ref(house,"telephoneRing",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Campaign/Telephone.wav"));
        var contact=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Finish/Relay.wav");Ref(house,"receiverClick",contact);Ref(house,"fuseRemoval",contact);Ref(house,"fuseInsertion",contact);Ref(house,"keyPickup",contact);Ref(house,"lockedDoor",contact);
        Ref(house,"objectAudio",Audio("Present object contact",Vector3.zero,"",false,.12f));
        var appliance=Audio("Kitchen refrigerator",new Vector3(-4.85f,.8f,-60.1f),"DomesticFridge_LP",true,.10f);Ref(house,"kitchenApplianceAudio",appliance);Ref(house,"kitchenApplianceClip",appliance.clip);
        var movement=Audio("6B hinge movement",hinge.transform.position+Vector3.up,"",true,.17f);var latch=Audio("6B latch",new Vector3(.35f,1,-62.45f),"",false,.14f);Ref(door,"movementAudio",movement);Ref(door,"movementClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav"));Ref(door,"latchAudio",latch);Ref(door,"unlockClip",contact);Ref(door,"openEndpointClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav"));
        var presence=new GameObject("Study presence / held behind glass");presence.transform.SetParent(root);presence.transform.position=new Vector3(2.5f,0,-58.8f);var figure=Prop("RecordedFigure",presence.transform.position,270,false);figure.transform.SetParent(presence.transform,true);foreach(var r in figure.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>dark).ToArray();
        var volume=new GameObject("Study visibility safety volume");volume.transform.SetParent(root);volume.transform.position=new Vector3(2.5f,1.0f,-58.8f);var bounds=volume.AddComponent<BoxCollider>();bounds.isTrigger=true;bounds.size=new Vector3(.75f,1.9f,.75f);Ref(house,"presenceRoot",presence);Ref(house,"presenceVisibilityVolume",bounds);Arr(house,"transparentSightColliders",new[]{sightGlass});presence.SetActive(false);
        var weight=Audio("Study floorboard weight",new Vector3(2.5f,.10f,-58.8f),"FloorboardWeight",false,.15f);Ref(house,"presenceAudio",weight);Ref(house,"presenceArrival",weight.clip);
        // Existing chapter entry is repositioned to the domestic scale; the old house remains inactive.
        var entry=Object.FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).First(s=>s.Kind==StationKind.HouseOrigin);entry.transform.position=new Vector3(.78f,1.4f,-49.6f);entry.transform.localScale=Vector3.one;entry.GetComponent<BoxCollider>().size=new Vector3(.14f,.40f,.32f);entry.SetCaption("6B / ENTRAR");
        Box("6B entry plaque",new Vector3(.86f,1.4f,-49.6f),new Vector3(.035f,.28f,.24f),steel,false);Label("6B entry identity","6B",new Vector3(.83f,1.44f,-49.6f),90,.13f,new Color(.70f,.73f,.61f));
        Ref(session,"houseMemory",house);
    }
    static void WireAudio()
    {
        var feet=Object.FindFirstObjectByType<PlayerFootsteps>();Arr(feet,"timber",Enumerable.Range(0,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Overhaul/FootWood"+i+".wav")));
        var zones=new List<Object>();
        var oldZones=new SerializedObject(feet).FindProperty("timberZones");for(int i=0;i<oldZones.arraySize;i++){var zone=oldZones.GetArrayElementAtIndex(i).objectReferenceValue as BoxCollider;if(zone&&zone.gameObject.activeInHierarchy)zones.Add(zone);}
        foreach(var area in new[]{new Vector4(0,-52.15f,10.7f,6.3f),new Vector4(0,-58.85f,1.82f,7.3f),new Vector4(3.2f,-57.95f,4.2f,5.3f)}){var g=new GameObject("Residence timber footstep zone");g.transform.SetParent(root);g.transform.position=new Vector3(area.x,1,area.y);var z=g.AddComponent<BoxCollider>();z.isTrigger=true;z.size=new Vector3(area.z,2.4f,area.w);zones.Add(z);}
        Arr(feet,"timberZones",zones);
        var feedback=root.gameObject.AddComponent<PressureWorkAudio>();Ref(feedback,"director",director);Ref(feedback,"session",Object.FindFirstObjectByType<GameSession>());
        var flow=Audio("Pressure valve working flow",new Vector3(6,1.5f,-36),"ValveFlow_LP",true,.14f);flow.outputAudioMixerGroup=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();
        var discharge=Audio("Pressure valve discharge",flow.transform.position,"ValveDischarge",false,.20f);Ref(feedback,"flowSource",flow);Ref(feedback,"dischargeSource",discharge);Ref(feedback,"flowLoopClip",flow.clip);Ref(feedback,"dischargeClip",discharge.clip);Ref(director,"pressureWorkAudio",feedback);
        foreach(var a in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))a.dopplerLevel=0;
    }
}

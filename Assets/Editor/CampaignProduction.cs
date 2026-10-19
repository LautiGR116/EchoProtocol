using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class CampaignProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Campaign.unity";
    const string Art="Assets/_Game/Art/Campaign";
    static Transform root;static TextMesh template;static CampaignDirector director;
    static Material wall,floor,steel,wood,paper,ceiling,lightMat,glass,ink;
    static Dictionary<string,Material> modelMaterials=new Dictionary<string,Material>();
    static List<CampaignStation> stations=new List<CampaignStation>();
    static AudioMixerGroup sfxGroup;
    static GameObject Named(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n).gameObject;
    static void Ref(Object o,string name,Object value){var so=new SerializedObject(o);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Arr(Object o,string name,IEnumerable<Object> values){var a=values.ToArray();var so=new SerializedObject(o);var p=so.FindProperty(name);p.arraySize=a.Length;for(int i=0;i<a.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=a[i];so.ApplyModifiedPropertiesWithoutUndo();}
    static void Value(Object o,string name,string value){var so=new SerializedObject(o);so.FindProperty(name).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=true)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());if(m&&m.GetTexture("_BaseMap")){var repeat=s.y<Mathf.Min(s.x,s.z)?new Vector2(s.x/2,s.z/2):new Vector2(Mathf.Max(s.x,s.z)/2,s.y/2);g.AddComponent<SurfaceMapping>().Configure(g.GetComponent<Renderer>(),repeat);}return g;}
    static Material Material(string name,Color color,float smooth=.2f,float metal=0)
    {string path=Art+"/M_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("Universal Render Pipeline/Lit");m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);return m;}
    static Material Surface(string name,Color color,int kind,float smooth)
    {
        var m=Material(name,Color.white,smooth);string path=Art+"/T_"+name+".png";
        if(!File.Exists(path))
        {
            var t=new Texture2D(512,512,TextureFormat.RGB24,false);var pixels=new Color[512*512];
            for(int y=0;y<512;y++)for(int x=0;x<512;x++)
            {
                float noise=Mathf.PerlinNoise(x*.081f+17,y*.081f+13)*.1f+Mathf.PerlinNoise(x*.013f,y*.013f)*.11f;
                float v=.83f+noise;
                if(kind==0){if(x%128<2||y%128<2)v*=.46f;if((x/128+y/128)%2==0)v*=.90f;v*=1-Mathf.PerlinNoise(x*.02f+25,y*.02f)*.09f;}
                if(kind==1){v*=1-Mathf.Pow(Mathf.PerlinNoise(x*.04f,y*.008f),5)*.27f;if(y<130)v*=.68f;}
                if(kind==2){if(x%128<3||y%128<3)v*=.48f;if(Mathf.PerlinNoise(x*1.9f,y*1.9f)>.72f)v*=.55f;}
                if(kind==3){v=.65f+.35f*Mathf.PerlinNoise(x*.10f,y*.009f);if(x%85<2||y%256<2)v*=.65f;}
                if(kind==4){float fx=(x%64-32)/32f,fy=(y%96-48)/48f;float floral=Mathf.Sin(fx*6)*Mathf.Sin(fy*5);if(floral>.5f)v*=.67f;if(x%64<1)v*=.85f;}
                pixels[y*512+x]=new Color(color.r*v,color.g*v,color.b*v);
            }
            t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.wrapMode=TextureWrapMode.Repeat;imp.anisoLevel=4;imp.SaveAndReimport();
        }
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));return m;
    }
    static TextMesh Label(string n,string words,Vector3 p,float yaw,float width=2.2f)
    {
        var t=Object.Instantiate(template,root);t.name=n;t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=64;t.characterSize=.033f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.77f,.86f,.8f);
        Ref(t.GetComponent<DiegeticText>(),"depthTestedShader",Shader.Find("EchoProtocol/OneSidedText"));
        var bounds=t.GetComponent<Renderer>().localBounds;if(bounds.size.x>width)t.characterSize*=width/bounds.size.x;
        return t;
    }
    static Light Lamp(string name,Vector3 p,bool warm,float power=2,float range=7)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=warm?new Color(1,.65f,.32f):new Color(.48f,.72f,.74f);l.intensity=power;l.range=range;l.shadows=LightShadows.Soft;return l;}
    static Light Practical(float x,float z,bool warm=false,float h=2.8f)
    {Prop("Fluorescent",new Vector3(x,h+.10f,z),0,false);return Lamp("Practical light",new Vector3(x,h-.14f,z),warm,4.4f,8);}
    static GameObject Prop(string type,Vector3 p,float yaw=0,bool solid=true)
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Campaign/EP_"+type+".fbx");
        var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Campaign "+type;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<MeshRenderer>())
        {
            r.sharedMaterials=r.sharedMaterials.Select(m=>type=="RecordedFigure"?modelMaterials["Echo"]:modelMaterials.TryGetValue(m.name,out var replacement)?replacement:steel).ToArray();
            if(solid){var mc=r.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;}
        }
        return g;
    }
    static void Painting(Vector3 p,float yaw,float width)
    {
        var holder=new GameObject("Framed courtyard painting");holder.transform.SetParent(root);holder.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));
        var m=Material("CourtyardPainting",Color.white,.12f);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/CourtyardPainting.png"));
        var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="Original courtyard canvas";q.transform.SetParent(holder.transform,false);q.transform.localScale=new Vector3(width,width*2/3,1);q.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(q.GetComponent<Collider>());
        float height=width*2/3;
        for(int side=-1;side<=1;side+=2)
        {
            var a=Box("Painting carved side",Vector3.zero,new Vector3(.075f,height+.15f,.10f),wood,false);a.transform.SetParent(holder.transform,false);a.transform.localPosition=new Vector3(side*(width/2+.0375f),0,.015f);
            var b=Box("Painting carved rail",Vector3.zero,new Vector3(width+.15f,.075f,.10f),wood,false);b.transform.SetParent(holder.transform,false);b.transform.localPosition=new Vector3(0,side*(height/2+.0375f),.015f);
        }
    }
    static BoxCollider Zone(string name,Vector3 center,Vector3 size)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=center;var b=g.AddComponent<BoxCollider>();b.isTrigger=true;b.size=size;return b;}
    static Transform Origin(string name,Vector3 p,float yaw=180)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));return g.transform;}
    static CampaignStation Station(StationKind kind,string text,Vector3 p,float yaw=180)
    {
        var g=Box("Station / "+kind,p,new Vector3(.85f,.68f,.14f),steel);
        g.transform.rotation=Quaternion.Euler(0,yaw,0);
        var d=g.AddComponent<CampaignStation>();Ref(d,"director",director);var so=new SerializedObject(d);so.FindProperty("kind").enumValueIndex=(int)kind;so.FindProperty("caption").stringValue=text;so.ApplyModifiedPropertiesWithoutUndo();
        var label=Label("Station legend / "+kind,text,p-g.transform.forward*.085f,yaw,.77f);Ref(d,"display",label);stations.Add(d);return d;
    }
    static SlidingDoor Door(string n,Vector3 p,float width)
    {
        var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;
        var leaf=Box(n+" steel shutter",p+Vector3.up*1.4f,new Vector3(width,2.8f,.18f),steel);leaf.transform.SetParent(g.transform,true);var rb=leaf.AddComponent<Rigidbody>();rb.useGravity=false;rb.isKinematic=true;
        var d=g.AddComponent<SlidingDoor>();Ref(d,"doorBody",rb);var s=new SerializedObject(d);s.FindProperty("closedLocalPosition").vector3Value=new Vector3(0,1.4f,0);s.FindProperty("openLocalPosition").vector3Value=new Vector3(0,4.4f,0);s.FindProperty("moveSpeed").floatValue=1.5f;s.ApplyModifiedPropertiesWithoutUndo();
        for(int side=-1;side<=1;side+=2)Box(n+" track",p+new Vector3(side*(width*.5f+.08f),1.45f,0),new Vector3(.12f,2.9f,.25f),steel);
        Box(n+" transom",p+Vector3.up*2.92f,new Vector3(width+.25f,.18f,.25f),steel);
        for(int i=0;i<10;i++){var rib=Box(n+" shutter rib",p+new Vector3(0,.16f+i*.27f,-.105f),new Vector3(width-.1f,.023f,.02f),ink,false);rib.transform.SetParent(leaf.transform,true);}
        return d;
    }
    static void Room(string name,float x,float z,float w,float length,bool home=false)
    {
        Box(name+" floor",new Vector3(x,-.12f,z),new Vector3(w,.24f,length),home?wood:floor);
        Box(name+" ceiling",new Vector3(x,3.16f,z),new Vector3(w,.18f,length),ceiling);
        for(int side=-1;side<=1;side+=2)
        {
            Box(name+" perimeter",new Vector3(x+side*(w/2+.1f),1.5f,z),new Vector3(.2f,3.2f,length+.2f),home?paper:wall);
            Box(name+" skirting",new Vector3(x+side*(w/2-.03f),.13f,z),new Vector3(.07f,.26f,length),home?wood:steel,false);
            Box(name+" mid rail",new Vector3(x+side*(w/2-.055f),1.05f,z),new Vector3(.07f,.11f,length),home?wood:steel,false);
        }
    }
    static void EndWall(string n,float z,float width,float opening=2.5f)
    {float side=(width-opening)/2;for(int s=-1;s<=1;s+=2)Box(n,new Vector3(s*(opening/2+side/2),1.5f,z),new Vector3(side,3.2f,.2f),wall);Box(n+" lintel",new Vector3(0,3,z),new Vector3(opening,.4f,.2f),wall);}
    static void Pipe(string n,Vector3 a,Vector3 b,float radius)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(root);g.transform.position=(a+b)/2;g.transform.up=(b-a).normalized;g.transform.localScale=new Vector3(radius*2,(b-a).magnitude/2,radius*2);g.GetComponent<Renderer>().sharedMaterial=steel;Object.DestroyImmediate(g.GetComponent<Collider>());}
    static AudioSource Audio(string n,Vector3 p,AudioClip clip,bool loop=false)
    {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;var a=g.AddComponent<AudioSource>();a.outputAudioMixerGroup=sfxGroup;a.clip=clip;a.loop=loop;a.playOnAwake=loop;a.spatialBlend=1;a.rolloffMode=AudioRolloffMode.Linear;a.minDistance=1.5f;a.maxDistance=18;a.volume=.2f;return a;}
    static AudioClip Sound(string n)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Campaign/"+n+".wav");
    [MenuItem("Echo Protocol/Author campaign expansion")]
    public static void Author()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop play first");
        var scene=EditorSceneManager.OpenScene(OvernightProduction.ScenePath);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        wall=Surface("AgedPaint",new Color(.59f,.66f,.62f),1,.17f);floor=Surface("Vinyl",new Color(.21f,.27f,.25f),0,.25f);ceiling=Surface("AcousticTile",new Color(.64f,.68f,.6f),2,.1f);
        wood=Surface("Timber",new Color(.32f,.17f,.071f),3,.3f);paper=Surface("Wallpaper",new Color(.58f,.47f,.30f),4,.12f);
        steel=Material("OxidizedSteel",new Color(.19f,.25f,.24f),.37f,.7f);ink=Material("Rubber",new Color(.019f,.024f,.022f),.12f);
        lightMat=Material("WarmDiffuser",new Color(.72f,.60f,.36f),.2f);lightMat.EnableKeyword("_EMISSION");lightMat.SetColor("_EmissionColor",new Color(.4f,.28f,.11f));
        glass=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/M_VS1_VT1_Glass.mat");
        modelMaterials.Clear();modelMaterials["Steel"]=steel;modelMaterials["Enamel"]=wall;modelMaterials["Wood"]=wood;modelMaterials["Fabric"]=Surface("Upholstery",new Color(.29f,.18f,.1f),4,.07f);modelMaterials["Ivory"]=lightMat;modelMaterials["Black"]=ink;modelMaterials["Glass"]=glass;modelMaterials["Brass"]=Material("Brass",new Color(.38f,.25f,.10f),.4f,.8f);modelMaterials["Echo"]=Material("EchoFigure",new Color(.15f,.5f,.47f),.3f,.15f);modelMaterials["Echo"].EnableKeyword("_EMISSION");modelMaterials["Echo"].SetColor("_EmissionColor",new Color(.035f,.16f,.15f));
        modelMaterials["Echo"].shader=Shader.Find("EchoProtocol/RecordedSurface");modelMaterials["Echo"].SetColor("_Color",new Color(.18f,.62f,.60f,1));
        template=Named("P3 threshold identity").GetComponent<TextMesh>();
        root=new GameObject("Campaign / authored expansion").transform;director=root.gameObject.AddComponent<CampaignDirector>();stations.Clear();
        var player=Object.FindFirstObjectByType<FirstPersonController>();var loop=Object.FindFirstObjectByType<LoopController>();var session=Object.FindFirstObjectByType<GameSession>();var d1=Object.FindFirstObjectByType<VS1DenialEvidence>();var finale=Object.FindFirstObjectByType<ProtocolFinale>();
        var oldFinal=new SerializedObject(finale);var entryDoor=(SlidingDoor)oldFinal.FindProperty("exitDoor").objectReferenceValue;
        Ref(session,"campaign",director);Value(session,"editionLabel","CAMPAÑA / REGISTROS 01–05");Ref(session,"finale",null);Object.DestroyImmediate(finale);
        foreach(string n in new[]{"Exit completion volume","Exit sealed end","Final passage words","Final remembered domestic door","Final domestic left trim","Final domestic right trim","Final amber aperture","Archive terminal instructions","Archive ready lamp"})
        {var t=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name==n);if(t)t.gameObject.SetActive(false);}
        Named("Final archive terminal").SetActive(false);
        Station(StationKind.Descent,"REGISTRO 02 / SUBSUELO\nCompletar revisión",new Vector3(2.25f,1.45f,-11.85f));
        Label("Descent threshold identity","REGISTRO 02 / PRESIÓN",new Vector3(0,2.6f,-23.7f),180,2.7f);
        // Service vestibule: corridor continues seamlessly from the preserved first act.
        Room("Sublevel vestibule",0,-26,3.2f,4);
        var origins=new[]{Origin("Pressure recall origin",new Vector3(0,.03f,-29)),Origin("House recall origin",new Vector3(0,.03f,-51)),Origin("Gallery recall origin",new Vector3(0,.03f,-77)),Origin("Archive recall origin",new Vector3(0,.03f,-109))};
        Room("Pressure chamber",0,-38,16,20);EndWall("Pressure north",-28,16,3.2f);EndWall("Pressure south",-48,16);
        Station(StationKind.PressureOrigin,"02 / PRESIÓN\n[E] Establecer origen",new Vector3(1.1f,1.4f,-28.1f),0);
        Label("Pressure contract","EL ECHO MANTIENE LA CÁMARA\nVOS CEBÁS LAS DOS VÁLVULAS\n\nRegistrá 30 segundos mirando la marca azul.\n[R] · Cruzá y mantené la mirada en cada válvula.",new Vector3(-3,1.9f,-28.15f),0,3.6f);
        // Blue pressure bay faces a west-wall sensor; a full-height grille divides the machinery.
        var pressureZone=Zone("Pressure Echo volume",new Vector3(-5.7f,1,-34),new Vector3(2,2.4f,2.3f));
        Box("Pressure footprint",new Vector3(-5.7f,.013f,-34),new Vector3(1.8f,.025f,2),Material("RecordingMark",new Color(.10f,.33f,.31f),.25f),false);
        Label("Pressure facing","MIRÁ AQUÍ / 30 s",new Vector3(-7.88f,1.65f,-34),270,2.2f);
        for(int i=0;i<7;i++)Pipe("Pressure intake",new Vector3(-7.7f,2.75f,-29-i*2.5f),new Vector3(7.7f,2.75f,-29-i*2.5f),.05f);
        for(int i=0;i<4;i++){Prop("Locker",new Vector3(-7.65f,0,-38-i*2.1f),90);Prop("PressurePump",new Vector3(6.6f,0,-32-i*3.7f),270);}
        Prop("Trolley",new Vector3(-2.8f,0,-42),20);
        Box("Central pressure baffle",new Vector3(0,1.35f,-39),new Vector3(1.7f,2.7f,6),steel);
        for(int i=0;i<9;i++){Box("Baffle vent",new Vector3(0,1.3f,-36.02f),new Vector3(1.4f,.035f,.025f),ink,false).transform.position+=Vector3.up*(i*.12f-.4f);}
        for(int i=0;i<4;i++)Pipe("Vertical pressure riser",new Vector3(.95f,.15f,-36.7f-i*1.45f),new Vector3(.95f,2.8f,-36.7f-i*1.45f),.09f);
        Station(StationKind.ValveA,"VÁLVULA A\n[E] Cebar / 6 s",new Vector3(5.9f,1.5f,-36.5f),90);
        Station(StationKind.ValveB,"VÁLVULA B\n[E] Cebar / 6 s",new Vector3(5.9f,1.5f,-44),90);
        var pressureLights=new[]{Practical(3,-35),Practical(3,-43)};Practical(-5.7f,-34);Practical(0,-29);
        var doors=new List<SlidingDoor>{entryDoor,Door("Pressure bulkhead",new Vector3(0,0,-48),2.5f)};
        // The house has a central corridor and four occupied rooms, each with a real doorway.
        Room("Domestic reconstruction",0,-61,16,26,true);EndWall("House north",-49,16);EndWall("House south",-74,16);
        Station(StationKind.HouseOrigin,"03 / RECONSTRUCCIÓN\nREGISTRO EN PAUSA\n[E] Entrar",new Vector3(1.25f,1.4f,-50.3f),90);
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*1.85f;
            foreach(var segment in new[]{new Vector2(-51.5f,5),new Vector2(-58,4),new Vector2(-65.5f,5),new Vector2(-72,4)})
                Box("House corridor wall",new Vector3(x,1.5f,segment.x),new Vector3(.16f,3.1f,segment.y),paper);
            Box("House room partition",new Vector3(side*4.95f,1.5f,-61),new Vector3(6.1f,3.1f,.15f),paper);
            for(int i=0;i<2;i++){float z=-55-i*14;for(int j=-1;j<=1;j+=2)Box("Domestic door jamb",new Vector3(x,1.3f,z+j*1.1f),new Vector3(.24f,2.6f,.13f),wood);Box("Domestic door head",new Vector3(x,2.65f,z),new Vector3(.24f,.13f,2.3f),wood);}
        }
        Painting(new Vector3(-6.25f,1.9f,-60.87f),180,2.1f);
        Painting(new Vector3(1.75f,1.8f,-58.1f),90,1.3f);
        Painting(new Vector3(5.5f,1.9f,-49.15f),0,1.7f);
        var chair=Prop("Armchair",new Vector3(-4.1f,0,-54.4f),150);
        Prop("Armchair",new Vector3(-6.5f,0,-57.7f),45);Prop("TelephoneTable",new Vector3(-6.5f,0,-54),0);Prop("Telephone",new Vector3(-6.5f,.76f,-54),0,false);Prop("LampTable",new Vector3(-7.2f,0,-57.2f),0);
        Box("Domestic woven rug",new Vector3(-4.8f,.016f,-56.5f),new Vector3(4.4f,.025f,3.5f),modelMaterials["Fabric"],false);
        Station(StationKind.Telephone,"TELÉFONO\n[E] Atender",new Vector3(-7.7f,1.45f,-54),270);
        Prop("Bed",new Vector3(5.6f,0,-55.7f),0);Prop("LampTable",new Vector3(7,0,-53.7f));Prop("Locker",new Vector3(7.6f,0,-58.5f),270);
        Prop("Kitchen",new Vector3(-6.8f,0,-68.7f),90);Prop("Kitchen",new Vector3(-5.5f,0,-72.8f),0);Prop("LampTable",new Vector3(-3.7f,0,-66.8f));
        for(int i=0;i<3;i++)Prop("ArchiveCabinet",new Vector3(7.5f,0,-65-i*2.7f),270);
        Prop("Trolley",new Vector3(4.9f,0,-69),20);
        Station(StationKind.NoteArrival,"HOJA 01\n[E] Leer",new Vector3(-3.3f,1.45f,-60.85f),180);
        Station(StationKind.NoteWorkOrder,"HOJA 02\n[E] Leer",new Vector3(-3.5f,1.45f,-73.8f),180);
        Station(StationKind.NoteConsent,"HOJA 03\n[E] Leer",new Vector3(4.4f,1.45f,-73.8f),180);
        Station(StationKind.CircuitKitchen,"COCINA\n[E] Comprobar / 4 s",new Vector3(-7.75f,1.5f,-65.5f),270);
        Station(StationKind.CircuitBedroom,"DORMITORIO\n[E] Comprobar / 4 s",new Vector3(7.75f,1.5f,-55.6f),90);
        Station(StationKind.CircuitHall,"RECIBIDOR\n[E] Comprobar / 4 s",new Vector3(.7f,1.5f,-49.2f),0);
        var homeLights=new[]{Lamp("Kitchen lamp",new Vector3(-4.2f,1.7f,-66.8f),true,3.3f,7),Lamp("Bedroom lamp",new Vector3(6.9f,1.8f,-53.7f),true,3.3f,7),Lamp("Entrance lamp",new Vector3(0,2.5f,-52),true,1.3f,9)};
        Lamp("Parlor lamp",new Vector3(-7.2f,1.45f,-57.2f),true,2.4f,6);Lamp("Archive room spill",new Vector3(5.2f,2.6f,-69),false,.6f,7);Lamp("House emergency fixture",new Vector3(0,2.7f,-62),false,.35f,13);
        Prop("Bookshelf",new Vector3(-7.6f,0,-59.2f),90);Prop("Bookshelf",new Vector3(2.6f,0,-73.5f),0);
        Prop("DomesticWindow",new Vector3(-7.82f,1.65f,-56),90,false);Prop("DomesticWindow",new Vector3(7.82f,1.65f,-57.5f),270,false);
        Prop("Clock",new Vector3(1.2f,2.6f,-49.13f),0,false);
        for(int i=0;i<5;i++)Prop("CeilingVent",new Vector3(1.1f,3.03f,-30-i*3),0,false);
        var presence=new GameObject("Domestic presence / physical reconstruction");presence.transform.SetParent(root);presence.transform.position=new Vector3(0,0,-53.2f);
        var presenceBody=Prop("RecordedFigure",presence.transform.position,180,false);presenceBody.transform.SetParent(presence.transform,true);foreach(var r in presenceBody.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>ink).ToArray();
        var presenceCollider=presence.AddComponent<BoxCollider>();presenceCollider.center=new Vector3(0,.9f,0);presenceCollider.size=new Vector3(.72f,1.8f,.55f);
        var rim=Lamp("Presence backlight",new Vector3(0,1.9f,-51.8f),true,1.4f,4);rim.transform.SetParent(presence.transform,true);presence.SetActive(false);
        doors.Add(Door("House rear shutter",new Vector3(0,0,-74),2.5f));
        // Parallel gallery: west is recordable rehearsal; east requires live co-presence.
        Room("Gallery entry",0,-76.5f,8,5);Room("Companion gallery",0,-92,8,28);EndWall("Gallery end",-106,8,2.5f);
        Station(StationKind.GalleryOrigin,"04 / ACOMPAÑAMIENTO\n[E] Establecer origen",new Vector3(3.8f,1.45f,-77),90);
        Box("Gallery divider plinth",new Vector3(0,.45f,-91.5f),new Vector3(.22f,.9f,25),wall);
        Box("Gallery divider glass",new Vector3(0,1.75f,-91.5f),new Vector3(.045f,1.7f,25),glass);
        Box("Gallery divider head",new Vector3(0,2.8f,-91.5f),new Vector3(.25f,.35f,25),steel);
        var echoBays=new List<BoxCollider>();var playerBays=new List<BoxCollider>();var galleryLights=new List<Light>();
        for(int i=0;i<3;i++)
        {
            float z=-83-i*8;
            echoBays.Add(Zone("Recorded bay "+i,new Vector3(-2.2f,1,z),new Vector3(2.4f,2.4f,2.5f)));
            playerBays.Add(Zone("Companion bay "+i,new Vector3(2.2f,1,z),new Vector3(2.4f,2.4f,2.5f)));
            Box("Record floor mark "+i,new Vector3(-2.2f,.014f,z),new Vector3(1.9f,.025f,1.5f),Material("RecordingMark",new Color(.10f,.33f,.31f),.25f),false);
            Box("Companion floor mark "+i,new Vector3(2.2f,.014f,z),new Vector3(1.9f,.025f,1.5f),lightMat,false);
            Label("Bay recording rule "+i,(i+1)+" / MIRÁ AL VIDRIO\nREGISTRÁ UNA PAUSA DE 8 s",new Vector3(-.16f,.68f,z),90,1.8f);
            Label("Bay companion rule "+i,(i+1)+" / ESPERÁ A TU ECHO",new Vector3(.16f,.68f,z),270,1.8f);
            doors.Add(Door("Companion shutter "+i,new Vector3(2,0,z-3),3.8f));
            galleryLights.Add(Practical(-2,z));Practical(2,z-3);
            for(int side=-1;side<=1;side+=2)Box("Observation mullion",new Vector3(.10f*side,1.8f,z-2),new Vector3(.12f,2.5f,.16f),steel,false);
            Box("Opaque observation panel",new Vector3(0,1.8f,z-4),new Vector3(.2f,2.65f,1.8f),wall);
        }
        // Recording lane ends here: the final room is accessed only through the third paired shutter.
        Box("West gallery sealed end",new Vector3(-2,1.5f,-103.8f),new Vector3(4,3,.2f),wall);
        Label("Record lane return","FIN DEL ENSAYO\n[R] Y ACOMPAÑÁ POR LA DERECHA",new Vector3(-2,1.8f,-103.65f),180,3.1f);
        // Wide archive, recordable bay behind window with physical return path around its north end.
        Room("Final archive",0,-115.5f,12,19);EndWall("Archive north",-106,12,2.5f);EndWall("Archive south",-125,12);
        Station(StationKind.ArchiveOrigin,"05 / ARCHIVO\n[E] Establecer origen",new Vector3(4.8f,1.5f,-107.2f),0);
        for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)Prop("ArchiveCabinet",new Vector3(side*5.5f,0,-110-i*2.8f),side==1?270:90);
        Box("Archive witness sill",new Vector3(0,.35f,-116),new Vector3(.2f,.7f,9),wall);
        Box("Archive witness glass",new Vector3(0,1.5f,-116),new Vector3(.07f,2.8f,9),glass);
        for(int i=0;i<4;i++)Box("Archive mullion",new Vector3(0,1.5f,-112-i*2.8f),new Vector3(.15f,3,.10f),steel);
        var finalZone=Zone("Final recorded stand",new Vector3(-2.5f,1,-115),new Vector3(2,2.4f,2));
        Box("Final record mark",new Vector3(-2.5f,.014f,-115),new Vector3(1.8f,.025f,1.8f),Material("RecordingMark",new Color(.10f,.33f,.31f),.25f),false);
        Box("Final witness mark",new Vector3(2.5f,.014f,-115),new Vector3(1.8f,.025f,1.8f),lightMat,false);
        Label("Final capture rule","REGISTRÁ 12 s MIRANDO EL VIDRIO\n[R] · OBSERVÁ DESDE LA MARCA BLANCA",new Vector3(-.15f,.84f,-115),90,2.2f);
        Label("Final witness rule","EL RECORRIDO ES UN HECHO\nLA INTENCIÓN NO ESTÁ GRABADA",new Vector3(.15f,.84f,-116),270,2.2f);
        Station(StationKind.ArchiveLetter,"CARTA APORTADA\n[E] Leer",new Vector3(4.9f,1.45f,-120.6f),90);
        Station(StationKind.Preserve,"TOMA ACTUAL PENDIENTE\nVerificá tu recorrido",new Vector3(2.6f,1.5f,-123.8f),180);
        Practical(-3,-111);Practical(3,-115);Practical(-3,-120);Lamp("Archive exit amber",new Vector3(0,2.6f,-124),true,1,5);
        doors.Add(Door("Final archive exit",new Vector3(0,0,-125),2.5f));
        Room("Departure",0,-130,3.2f,10,true);Box("Departure end",new Vector3(0,1.5f,-135),new Vector3(3.4f,3.2f,.2f),paper);
        Prop("ClinicalDoor",new Vector3(0,0,-134.8f),180,false);Lamp("Departure table glow",new Vector3(0,2.5f,-133),true,3,9);
        Label("Departure final words","PUEDO IRME\nSIN BORRARLO",new Vector3(0,2.8f,-134.65f),180,2);
        var departure=Zone("Departure completion",new Vector3(0,1,-133.3f),new Vector3(3,2.4f,2));
        sfxGroup=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer").FindMatchingGroups("WorldSFX").First();
        var footsteps=player.gameObject.AddComponent<EchoProtocol.Audio.PlayerFootsteps>();Ref(footsteps,"controller",player.GetComponent<CharacterController>());Ref(footsteps,"player",player);
        var stepSource=Audio("Present Player foot contacts",player.transform.position,null);stepSource.volume=.16f;stepSource.minDistance=1;stepSource.maxDistance=6;Ref(footsteps,"source",stepSource);
        Arr(footsteps,"vinyl",Enumerable.Range(0,3).Select(i=>Sound("StepVinyl"+i)));Arr(footsteps,"timber",Enumerable.Range(0,3).Select(i=>Sound("StepWood"+i)));
        Arr(footsteps,"timberZones",new[]{Zone("Domestic floor sound",new Vector3(0,1,-61.5f),new Vector3(16,3,25)),Zone("Departure floor sound",new Vector3(0,1,-130),new Vector3(3.2f,3,10))});
        var machinery=Audio("Pressure machinery",new Vector3(6,1,-39),Sound("PressureBed"),true);
        var cues=Enumerable.Range(0,4).Select(i=>Audio("Campaign cue "+i,Vector3.zero,null)).ToArray();
        Ref(director,"player",player);Ref(director,"view",player.GetComponentInChildren<Camera>());Ref(director,"loop",loop);Ref(director,"recorder",player.GetComponent<EchoRecorder>());Ref(director,"session",session);Ref(director,"evidence",d1);Ref(director,"thought",d1.GetComponent<SD1SubtitleDisplay>());
        Arr(director,"origins",origins);Arr(director,"doors",doors);Ref(director,"pressureZone",pressureZone);Arr(director,"echoBays",echoBays);Arr(director,"playerBays",playerBays);Ref(director,"archiveEchoZone",finalZone);Ref(director,"departureZone",departure);Arr(director,"stations",stations);Arr(director,"pressureLights",pressureLights);Arr(director,"homeLights",homeLights);Arr(director,"galleryLights",galleryLights);Ref(director,"homePresence",presence);Ref(director,"homeChair",chair.transform);Ref(director,"machinery",machinery);Arr(director,"cues",cues);Arr(director,"sounds",new[]{Sound("Relay"),Sound("Telephone"),Sound("Wire"),Sound("WallContact"),Sound("CloseMovement"),Sound("Archive")});
        // The same deterministic body representation is used in every chapter. No animation channel is invented.
        var lp=new SerializedObject(loop);var original=(EchoPlayback)lp.FindProperty("echoPrefab").objectReferenceValue;
        var echo=Object.Instantiate(original);echo.name="Echo Campaign";foreach(var r in echo.GetComponentsInChildren<Renderer>())r.enabled=false;
        var figure=Prop("RecordedFigure",Vector3.zero,0,false);figure.transform.SetParent(echo.transform,false);figure.transform.localPosition=Vector3.zero;
        Directory.CreateDirectory("Assets/_Game/Prefabs/Campaign");var prefab=PrefabUtility.SaveAsPrefabAsset(echo.gameObject,"Assets/_Game/Prefabs/Campaign/EchoCampaign.prefab");Object.DestroyImmediate(echo.gameObject);Ref(loop,"echoPrefab",prefab.GetComponent<EchoPlayback>());
        // Dress the learned institution too, so its first impression belongs to this campaign.
        foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)))
        {
            if(r.transform.IsChildOf(root)||!r.sharedMaterial)continue;
            var n=r.sharedMaterial.name;
            if(n=="M_VS1_VT1_ClinicWall")r.sharedMaterial=wall;
            if(n=="M_VS1_VT1_ClinicFloor")r.sharedMaterial=floor;
            if(n=="M_VS1_VT1_DomesticWood")r.sharedMaterial=wood;
            var filter=r.GetComponent<MeshFilter>();if(filter&&filter.sharedMesh&&filter.sharedMesh.name=="Cube"&&(r.sharedMaterial==wall||r.sharedMaterial==floor||r.sharedMaterial==wood))
            {var size=r.bounds.size;var repeat=size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/2,size.z/2):new Vector2(Mathf.Max(size.x,size.z)/2,size.y/2);var mapping=r.GetComponent<SurfaceMapping>();if(!mapping)mapping=r.gameObject.AddComponent<SurfaceMapping>();mapping.Configure(r,repeat);}
        }
        Prop("Locker",new Vector3(-4.45f,0,-6.3f),90);Prop("Locker",new Vector3(-4.45f,0,-7.5f),90);Prop("Trolley",new Vector3(4.05f,0,-8.9f),0);Prop("LampTable",new Vector3(-3.8f,0,23.2f));
        for(int i=0;i<6;i++){Prop("Fluorescent",new Vector3(-2.75f,2.95f,-2+i*4),0,false);Prop("Fluorescent",new Vector3(2.75f,2.95f,-2+i*4),0,false);}
        var cameraData=player.GetComponentInChildren<Camera>().GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();if(cameraData){cameraData.renderPostProcessing=true;cameraData.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;}
        RenderSettings.reflectionIntensity=.22f;RenderSettings.ambientLight=new Color(.11f,.125f,.125f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.018f;RenderSettings.fogColor=new Color(.026f,.043f,.045f);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("CAMPAIGN AUTHORED "+stations.Count+" stations, "+doors.Count+" doors, 4 new chapters; "+scene.path);
    }
    public static void Inspect()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int missing=0;foreach(var g in scene.GetRootGameObjects())foreach(var t in g.GetComponentsInChildren<Transform>(true))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
        var objects=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var lines=new List<string>{"Scene: "+scene.path,"Missing scripts: "+missing,"Objects: "+objects.Length};
        foreach(var r in objects.Where(t=>t.name.StartsWith("Campaign ")).SelectMany(t=>t.GetComponentsInChildren<MeshRenderer>()).Distinct().Take(24))lines.Add(r.name+" bounds="+r.bounds.ToString());
        File.WriteAllLines("Docs/VisualEvidence/2026-09-30-Campaign/scene-inspection.txt",lines);Debug.Log(string.Join("\n",lines));
    }
    public static void CaptureViews()
    {
        var original=Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>();
        var go=Object.Instantiate(original.gameObject);go.name="Temporary campaign inspection camera";
        foreach(var c in go.GetComponents<MonoBehaviour>())if(!(c is UnityEngine.Rendering.Universal.UniversalAdditionalCameraData))Object.DestroyImmediate(c);
        var listener=go.GetComponent<AudioListener>();if(listener)Object.DestroyImmediate(listener);
        var camera=go.GetComponent<Camera>();camera.enabled=false;camera.fieldOfView=70;
        var positions=new[]{new Vector3(0,1.65f,-29),new Vector3(-1,1.65f,-37),new Vector3(0,1.65f,-51),new Vector3(-2.5f,1.65f,-56),new Vector3(-3,1.65f,-66),new Vector3(0,1.65f,-78),new Vector3(2,1.65f,-91),new Vector3(3,1.65f,-110)};
        var directions=new[]{new Vector3(-4,1.6f,-38),new Vector3(6,1.3f,-42),new Vector3(0,1.5f,-65),new Vector3(-6,1.2f,-55),new Vector3(-6,1.1f,-70),new Vector3(0,1.6f,-100),new Vector3(-2,1.6f,-99),new Vector3(-2,1.6f,-120)};
        var labels=Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None);var oldMaterials=labels.Select(t=>t.GetComponent<Renderer>().sharedMaterial).ToArray();var temporary=new List<Material>();
        foreach(var t in labels){var m=new Material(Shader.Find("EchoProtocol/DiegeticText"));m.mainTexture=t.font.material.mainTexture;t.GetComponent<Renderer>().sharedMaterial=m;temporary.Add(m);}
        var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;camera.targetTexture=rt;
        for(int i=0;i<positions.Length;i++){camera.transform.position=positions[i];camera.transform.LookAt(directions[i]);camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("Docs/VisualEvidence/2026-09-30-Campaign/view-"+i+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
        for(int i=0;i<labels.Length;i++)labels[i].GetComponent<Renderer>().sharedMaterial=oldMaterials[i];foreach(var m in temporary)Object.DestroyImmediate(m);
        camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);Debug.Log("Eight camera views saved");
    }

}

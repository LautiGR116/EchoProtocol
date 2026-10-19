using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using Object=UnityEngine.Object;

public static class FirstActProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_FirstAct.unity";
    public const string RootName="First Act / perception and return";
    const string Art="Assets/_Game/Art/FirstAct";
    static Transform root,domestic,cold;
    static Material plaster,wood,trim,metal,fabric,wallpaper;
    static readonly Dictionary<string,Material> slots=new Dictionary<string,Material>();
    static readonly List<Light> lights=new List<Light>();
    static Transform Any(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
    static Material Load(string p)=>AssetDatabase.LoadAssetAtPath<Material>(p);
    static Material Mat(string n,Color c,float smooth=.25f,float metallic=0)
    {var p=Art+"/M_"+n+".mat";var m=Load(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);EditorUtility.SetDirty(m);return m;}
    static void Ref(Object o,string n,Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static GameObject Box(Transform parent,string n,Vector3 p,Vector3 size,Material m)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());if(m==plaster||m==wood||m==wallpaper)g.AddComponent<SurfaceMapping>().Configure(g.GetComponent<Renderer>(),size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/2.5f,size.z/2.5f):new Vector2(Mathf.Max(size.x,size.z)/2.5f,size.y/2.5f));return g;}
    static GameObject Model(Transform parent,string family,string prefix,string name,Vector3 p,float yaw)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/"+family+"/EP_"+prefix+"_"+name+".fbx");if(!asset)throw new Exception("Missing "+name);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name="SE2 original / "+name;g.transform.SetParent(parent,false);g.transform.localPosition=p;
        var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.localRotation=Quaternion.Euler(0,yaw,0)*g.transform.localRotation;
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>slots.TryGetValue(m.name,out var v)?v:throw new Exception("Unmapped "+m.name)).ToArray();return g;
    }
    static Light Spot(Transform parent,string name,Vector3 position,Vector3 aim,Color color,float gain,float range,float angle)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.LookAt(parent.TransformPoint(aim));var l=g.AddComponent<Light>();l.type=LightType.Spot;l.spotAngle=angle;l.innerSpotAngle=angle*.7f;l.color=color;l.intensity=gain;l.range=range;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.08f;lights.Add(l);return l;}
    static void Emission(Material m,Color c){m.SetColor("_EmissionColor",c);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);}
    static void Track(Renderer surface,int index,Light light,Color color,Color emission)
    {var t=surface.gameObject.AddComponent<PracticalEmission>();var s=new SerializedObject(t);s.FindProperty("surface").objectReferenceValue=surface;s.FindProperty("source").objectReferenceValue=light;s.FindProperty("materialIndex").intValue=index;s.FindProperty("litColor").colorValue=color;s.FindProperty("emission").colorValue=emission;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Materials()
    {
        plaster=Load("Assets/_Game/Art/Overhaul/M_Plaster.mat");wood=Load("Assets/_Game/Art/Threshold/M_FurnitureWalnut.mat");fabric=Load("Assets/_Game/Art/Threshold/M_Linen.mat");wallpaper=Load("Assets/_Game/Art/Overhaul/M_FloralPaper.mat");
        trim=Mat("DomesticJoinery",new Color(.28f,.29f,.23f),.32f);metal=Mat("ServiceEnamel",new Color(.13f,.16f,.15f),.40f,.28f);
        slots.Clear();foreach(var p in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Art/Overhaul"}).Select(AssetDatabase.GUIDToAssetPath)){var m=Load(p);if(m.name.StartsWith("M_OV_"))slots[m.name.Substring(2)]=m;}
        slots["OV_Walnut"]=slots["OV_WalnutEdge"]=wood;slots["OV_ChairFabric"]=Load("Assets/_Game/Art/Threshold/M_ChairLinen.mat");slots["OV_Linen"]=fabric;slots["OV_Glass"]=Mat("NightWindow",new Color(.018f,.035f,.038f),.18f);
        slots["DL_BaseEnamel"]=Mat("DL_BaseEnamel",new Color(0.052f,0.067f,0.052f).gamma,0.510f,0.36f);
        slots["DL_Brass"]=Mat("DL_Brass",new Color(0.27f,0.188f,0.088f).gamma,0.580f,0.75f);
        slots["DL_Walnut"]=Mat("DL_Walnut",new Color(0.1f,0.052f,0.024f).gamma,0.500f,0f);
        slots["DL_ShadeLinen"]=Mat("DL_ShadeLinen",new Color(0.49f,0.425f,0.306f).gamma,0.120f,0f);
        slots["DL_ShadeBinding"]=Mat("DL_ShadeBinding",new Color(0.285f,0.248f,0.18f).gamma,0.160f,0f);
        slots["DL_InnerDiffuser"]=Mat("DL_InnerDiffuser",new Color(0.69f,0.64f,0.5f).gamma,0.170f,0f);
        slots["DL_OpalBulb"]=Mat("DL_OpalBulb",new Color(0.79f,0.735f,0.58f).gamma,0.640f,0f);
        slots["DL_Rubber"]=Mat("DL_Rubber",new Color(0.012f,0.016f,0.012f).gamma,0.230f,0f);
        slots["DL_Bakelite"]=Mat("DL_Bakelite",new Color(0.047f,0.037f,0.022f).gamma,0.610f,0f);
        slots["DL_SocketIvory"]=Mat("DL_SocketIvory",new Color(0.38f,0.385f,0.295f).gamma,0.510f,0f);
    }
    static void DomesticRoom(GameObject figure)
    {
        foreach(Transform child in domestic)if(child.gameObject!=figure)child.gameObject.SetActive(false);
        foreach(var r in figure.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        Box(domestic,"Parlor continuous floor",new Vector3(0,-.065f,4),new Vector3(4.2f,.13f,8),wood);
        Box(domestic,"Parlor ceiling",new Vector3(0,2.80f,4),new Vector3(4.2f,.16f,8),plaster);
        foreach(float x in new[]{-2.10f,2.10f})
        {
            Box(domestic,"Parlor side wall",new Vector3(x,1.36f,4),new Vector3(.14f,2.72f,8),plaster);
            Box(domestic,"Parlor lower panel",new Vector3(x,.45f,4),new Vector3(.17f,.9f,8),trim);
            Box(domestic,"Parlor dado rail",new Vector3(x,.92f,4),new Vector3(.19f,.04f,8),wood);
            Box(domestic,"Parlor skirting",new Vector3(x,.075f,4),new Vector3(.19f,.15f,8),wood);
        }
        Box(domestic,"Rear room wall",new Vector3(0,1.36f,8),new Vector3(4.2f,2.72f,.14f),wallpaper);
        // Actual partition: a narrow domestic threshold, no repeated freestanding frames.
        Box(domestic,"Parlor partition left",new Vector3(-1.20f,1.36f,3.68f),new Vector3(1.80f,2.72f,.17f),wallpaper);
        Box(domestic,"Parlor partition right",new Vector3(1.46f,1.36f,3.68f),new Vector3(1.28f,2.72f,.17f),wallpaper);
        Box(domestic,"Parlor partition head",new Vector3(.26f,2.47f,3.68f),new Vector3(1.12f,.50f,.17f),wallpaper);
        foreach(var span in new[]{new Vector2(-1.20f,1.80f),new Vector2(1.46f,1.28f)})
        {Box(domestic,"Partition lower panel",new Vector3(span.x,.45f,3.575f),new Vector3(span.y,.90f,.045f),trim);Box(domestic,"Partition skirting",new Vector3(span.x,.075f,3.54f),new Vector3(span.y,.15f,.045f),wood);}
        var doorway=Model(domestic,"Overhaul","OV","DomesticDoor",new Vector3(.26f,0,3.68f),180);
        var leaf=doorway.GetComponentsInChildren<Transform>().First(t=>t.name=="DoorLeafPivot");leaf.RotateAround(leaf.position,domestic.up,-78f);
        Model(domestic,"Overhaul","OV","Armchair",new Vector3(-1.17f,0,1.64f),154);
        Model(domestic,"Overhaul","OV","KeyConsole",new Vector3(1.77f,0,2.5f),270);
        Model(domestic,"Overhaul","OV","DressedWindow",new Vector3(.66f,0,7.87f),180);
        // Figure belongs to the current world. Nothing is added to Echo channels.
        figure.transform.localPosition=new Vector3(.20f,0,5.18f);
        var adult=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Threshold/EP_AdultRecordedSurface.fbx");
        var body=(GameObject)PrefabUtility.InstantiatePrefab(adult);body.name="SE2 adult current-world figure";body.transform.SetParent(figure.transform,false);body.transform.localPosition=Vector3.zero;
        var bs=body.transform.localScale;bs.x=-bs.x;body.transform.localScale=bs;body.transform.localRotation=Quaternion.Euler(0,170,0)*body.transform.localRotation;
        foreach(var r in body.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Load(m.name.Contains("Skin")?"Assets/_Game/Art/Threshold/M_PresenceSkin.mat":"Assets/_Game/Art/Threshold/M_PresenceWool.mat")).ToArray();
        // Lamp cable terminates in the left wall: source back(-Z) rotates toward -X.
        var lamp=Model(domestic,"DomesticLighting","DL","FloorLamp",new Vector3(-1.17f,0,2.20f),90);
        var source=lamp.GetComponentsInChildren<Transform>().First(t=>t.name.EndsWith("__BULB_SOURCE"));
        var lg=new GameObject("SE2 actual standing lamp light");lg.transform.SetParent(domestic,false);lg.transform.position=source.position;
        var lampLight=lg.AddComponent<Light>();lampLight.type=LightType.Point;lampLight.color=new Color(1,.81f,.57f);lampLight.intensity=2.3f;lampLight.range=4.5f;lampLight.shadows=LightShadows.Soft;lampLight.shadowBias=.015f;lampLight.shadowNormalBias=.08f;lights.Add(lampLight);
        foreach(var r in lamp.GetComponentsInChildren<Renderer>())for(int i=0;i<r.sharedMaterials.Length;i++)if(new[]{slots["DL_ShadeLinen"],slots["DL_InnerDiffuser"],slots["DL_OpalBulb"]}.Contains(r.sharedMaterials[i]))
        {r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;var m=r.sharedMaterials[i];var c=new Color(1,.76f,.43f)*(m==slots["DL_OpalBulb"]?1.3f:.32f);Emission(m,c);Track(r,i,lampLight,m.GetColor("_BaseColor"),c);}
        // A window-side pool separates the back room without an amber rectangle.
        Spot(domestic,"SE2 window residual",new Vector3(1.10f,2.30f,7.55f),new Vector3(-.65f,1.25f,4.8f),new Color(.62f,.73f,.76f),2.6f,5.3f,105);
        var practical=Box(domestic,"Rear ceiling practical",new Vector3(-.90f,2.67f,6.08f),new Vector3(.30f,.055f,.30f),metal);
        var pool=Spot(domestic,"SE2 rear room low practical",new Vector3(-.90f,2.60f,6.08f),new Vector3(.22f,1.12f,5.2f),new Color(.91f,.82f,.66f),1.1f,3.8f,100);
        var opal=Mat("RearRoomOpal",new Color(.60f,.56f,.44f));Emission(opal,new Color(.50f,.44f,.29f));var face=Box(domestic,"Rear practical opal",new Vector3(-.90f,2.63f,6.08f),new Vector3(.23f,.014f,.23f),opal);Track(face.GetComponent<Renderer>(),0,pool,new Color(.60f,.56f,.44f),new Color(.50f,.44f,.29f));
    }
    static void ColdRoom()
    {
        foreach(var r in cold.GetComponentsInChildren<Renderer>(true))if(r.name.StartsWith("Cold ")&&!r.name.Contains("strip"))r.sharedMaterial=Load("Assets/_Game/Art/Finish/M_ClinicPaint.mat");
        var l=cold.GetComponentsInChildren<Light>(true).First();l.type=LightType.Spot;l.transform.localPosition=new Vector3(0,2.7f,1.4f);l.transform.LookAt(cold.TransformPoint(new Vector3(0,1.6f,3.2f)));l.spotAngle=110;l.innerSpotAngle=80;l.color=new Color(.71f,.81f,.79f);l.intensity=2.2f;l.range=4.2f;l.shadows=LightShadows.Soft;lights.Add(l);
        Box(cold,"Service wall base",new Vector3(0,.12f,3.20f),new Vector3(4.8f,.24f,.045f),metal);
        foreach(float x in new[]{-2.39f,2.39f})Box(cold,"Service side base",new Vector3(x,.12f,1.65f),new Vector3(.045f,.24f,3.2f),metal);
        Box(cold,"Vent mounted surround",new Vector3(0,1.81f,3.265f),new Vector3(1.29f,.64f,.035f),metal);
        var opal=Mat("ServiceOpal",new Color(.64f,.72f,.66f));Emission(opal,new Color(.51f,.65f,.57f));
        var fixture=Box(cold,"Service fixture backing",new Vector3(0,2.72f,1.4f),new Vector3(1.05f,.09f,.24f),metal);
        var face=Box(cold,"Service fixture opal",new Vector3(0,2.67f,1.4f),new Vector3(.95f,.012f,.19f),opal);Track(face.GetComponent<Renderer>(),0,l,new Color(.64f,.72f,.66f),new Color(.51f,.65f,.57f));
        cold.Find("Cold strip").gameObject.SetActive(false);
    }
    static void ReturnElbow()
    {
        var south=Any("R1 Return west wall south");south.position=new Vector3(-4.05f,1.6f,25.55f);south.localScale=new Vector3(.2f,3.2f,1.1f);
        var north=Any("R1 Return west wall north");north.position=new Vector3(-4.05f,1.6f,28.95f);north.localScale=new Vector3(.2f,3.2f,2.3f);
        var sides=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="R1 Portal trim").OrderBy(t=>t.position.z).ToArray();if(sides.Length!=2)throw new Exception("Expected two R1 portal sides");
        for(int i=0;i<2;i++){var p=sides[i].position;p.z=i==0?26.06f:27.84f;sides[i].position=p;}
        foreach(string name in new[]{"R1 Portal lintel","R1 Flush clinical threshold"}){var t=Any(name);var p=t.position;p.z=26.95f;t.position=p;var s=t.localScale;s.z=name.Contains("lintel")?1.86f:1.66f;t.localScale=s;}
        Any("SE3 Repeated Doorframe 2").position+=Vector3.forward*.15f;Any("SE3 Repeated Doorframe 3").position-=Vector3.forward*.30f;
        // The old decorative console and dado occupied the new opening.
        Any("SE3 Restrained Domestic Fragments").position+=Vector3.forward*1.8f;
        foreach(string name in new[]{"Partial domestic wall panel","Domestic chair rail"})
        {
            var t=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==name&&x.parent.name=="SE3 Domestic Overlap");
            var r=t.GetComponent<Renderer>();var b=r.bounds;t.gameObject.SetActive(false);
            foreach(var ends in new[]{new Vector2(b.min.z,26.10f),new Vector2(27.80f,b.max.z)})
                Box(t.parent,name+" beside R1 opening",t.parent.InverseTransformPoint(new Vector3(b.center.x,b.center.y,(ends.x+ends.y)*.5f)),new Vector3(b.size.x,b.size.y,ends.y-ends.x),r.sharedMaterial);
        }
    }
    static void RefinePresentation()
    {
        var etching=Mat("GlassSafetyMark",new Color(.18f,.24f,.23f),.14f);
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name.StartsWith("SE2 Eye Level Glass Etching")))
        {if(t.name.EndsWith("1")){t.gameObject.SetActive(false);continue;}var p=t.position;p.y=1.03f;t.position=p;var size=t.localScale;size.y=.004f;t.localScale=size;var r=t.GetComponent<Renderer>();r.sharedMaterial=etching;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.transform.parent&&r.transform.parent.name=="Sealed Observation Window"&&r.name!="SE2 Sealed Glass"))r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        Emission(slots["OV_Glass"],new Color(.045f,.070f,.08f));
        Box(domestic,"Parlor entry lintel",new Vector3(0,2.98f,.075f),new Vector3(4.25f,.52f,.18f),plaster);
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.name.StartsWith("SE2 Clinical")))l.color=new Color(.69f,.78f,.77f);
        var overlay=Any("SE3 Domestic Overlap");
        foreach(var r in overlay.GetComponentsInChildren<Renderer>(true))
        {if(r.name.Contains("Doorframe")||r.name.Contains("frame")||r.name.Contains("rail")||r.name.Contains("console")||r.name.Contains("Console"))r.sharedMaterial=wood;}
        foreach(var l in overlay.GetComponentsInChildren<Light>(true))l.color=new Color(1,.82f,.60f);
    }
    [MenuItem("Echo Protocol/Author first act perspective and return")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before first-act authoring");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(RegistrationProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        root=new GameObject(RootName).transform;lights.Clear();Materials();
        var e=Object.FindFirstObjectByType<VS1ParallelFigureEvent>();var so=new SerializedObject(e);
        cold=((GameObject)so.FindProperty("coldRoomRoot").objectReferenceValue).transform;domestic=((GameObject)so.FindProperty("domesticRoomRoot").objectReferenceValue).transform;
        var figure=(GameObject)so.FindProperty("figureRoot").objectReferenceValue;
        DomesticRoom(figure);ColdRoom();ReturnElbow();RefinePresentation();
        so.FindProperty("maxObservationDistanceToGlass").floatValue=3.5f;so.ApplyModifiedPropertiesWithoutUndo();
        var budget=new SerializedObject(Object.FindFirstObjectByType<LightShadowBudget>());var list=budget.FindProperty("lights");int n=list.arraySize;list.arraySize+=lights.Count;for(int i=0;i<lights.Count;i++)list.GetArrayElementAtIndex(n+i).objectReferenceValue=lights[i];budget.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("First Act authored: inspect cold/domestic views at three distances, figure clearance and physical SE3 return before route validation.");
    }
}

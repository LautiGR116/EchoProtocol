using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Presentation;
using EchoProtocol.Horror;
using Object=UnityEngine.Object;

// Scene authoring only. FirstAct remains the tested recovery checkpoint.
public static class RecoveryProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Recovery.unity";
    const string Art="Assets/_Game/Art/Recovery";
    static Transform room,root;
    static Material paint,wood,metal,opal,paper;
    static readonly List<Light> lights=new List<Light>();
    static Transform Any(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
    static Material Load(string p)=>AssetDatabase.LoadAssetAtPath<Material>(p);
    static Material Mat(string n,Color c,float smooth=.3f,float metallic=0)
    {var p=Art+"/M_"+n+".mat";var m=Load(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);EditorUtility.SetDirty(m);return m;}
    static Material Copy(string n,Material source)
    {var p=Art+"/M_"+n+".mat";var m=Load(p);if(!m){m=new Material(source);AssetDatabase.CreateAsset(m,p);}return m;}
    static void Emission(Material m,Color c){m.SetColor("_EmissionColor",c);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);}
    static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=false)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static Light Spot(string n,Vector3 p,Vector3 aim,Color c,float power,float range,float angle)
    {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;g.transform.LookAt(aim);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=c;l.intensity=power;l.range=range;l.spotAngle=angle;l.innerSpotAngle=angle*.72f;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.07f;lights.Add(l);return l;}
    static void Track(Renderer r,Light l,Color c,Color e,int index=0)
    {var t=r.gameObject.AddComponent<PracticalEmission>();var s=new SerializedObject(t);s.FindProperty("surface").objectReferenceValue=r;s.FindProperty("source").objectReferenceValue=l;s.FindProperty("litColor").colorValue=c;s.FindProperty("emission").colorValue=e;s.FindProperty("materialIndex").intValue=index;s.ApplyModifiedPropertiesWithoutUndo();}
    static TextMesh Label(string n,string words,Vector3 p,float yaw,float size)
    {var template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");var t=Object.Instantiate(template,root);t.name=n;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.54f,.60f,.55f);return t;}
    static GameObject Model(string path,string n,Vector3 p,float yaw,Func<Material,Material> mapping)
    {var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing "+path);var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name=n;g.transform.SetParent(root);var s=g.transform.localScale;s.x=-s.x;g.transform.localScale=s;g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0)*g.transform.rotation);foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(mapping).ToArray();return g;}
    static void Room()
    {
        foreach(var t in room.GetComponentsInChildren<Transform>(true))
        {
            if(t.name=="R1 Clinical cabinet"||t.name=="R1 Cabinet handle"||t.name=="R1 panel joint"||t.name=="R1 Station worktop"||t.name=="R1 Station inset"||t.name=="R1 Station solid plinth")t.gameObject.SetActive(false);
            if(t.name=="R1 panel-owned stable light"||t.name=="R1 recessed light housing"||t.name=="R1 stable cool diffuser")t.gameObject.SetActive(false);
            var r=t.GetComponent<Renderer>();if(!r)continue;
            if(t.name=="R1 South wall"||t.name=="R1 Ceiling")r.sharedMaterial=paint;
            if(t.name=="R1 Continuous floor")r.sharedMaterial=Load("Assets/_Game/Art/Campaign/M_Vinyl.mat");
            if(t.name.StartsWith("R1 Observation")&&!t.name.Contains("glass"))r.sharedMaterial=metal;
        }
        // One clear circulation pool, one work pool. No flat four-panel wash.
        foreach(var f in new[]{new Vector3(-5.55f,3.05f,28.7f),new Vector3(-8.55f,3.05f,26.7f)})
        {
            var housing=Box("Recovery retained fluorescent housing",f,new Vector3(.24f,.075f,1.12f),metal);
            var face=Box("Recovery opal panel",f-Vector3.up*.045f,new Vector3(.18f,.014f,1.02f),opal);
            var aim=f.x>-7?new Vector3(-5.6f,0,28.2f):new Vector3(-7.8f,.8f,27.5f);
            var l=Spot("Recovery local working light",f-Vector3.up*.07f,aim,new Color(.76f,.83f,.76f),f.x>-7?3.0f:4.0f,5.2f,105);
            Track(face.GetComponent<Renderer>(),l,new Color(.56f,.63f,.55f),new Color(.31f,.39f,.31f));
        }
        var bg=new GameObject("Recovery ceiling indirect return");bg.transform.SetParent(root);bg.transform.position=new Vector3(-7.55f,2.85f,27.60f);var bounce=bg.AddComponent<Light>();bounce.type=LightType.Point;bounce.color=new Color(.74f,.80f,.72f);bounce.intensity=1.4f;bounce.range=5.6f;lights.Add(bounce);
        foreach(float x in new[]{-9.45f,-8.75f})
        {
            var g=Model("Assets/_Game/Art/Models/Campaign/EP_ArchiveCabinet.fbx","Recovery individual filing cabinet",new Vector3(x,0,29.66f),180,m=>m.name.Contains("Ivory")?paper:m.name.Contains("Steel")?metal:paint);
            Box("Recovery cabinet collision",new Vector3(x,.74f,29.66f),new Vector3(.59f,1.48f,.66f),metal,true).GetComponent<Renderer>().enabled=false;
        }
        Box("Recovery north files shelf",new Vector3(-8.95f,1.82f,29.89f),new Vector3(1.75f,.045f,.38f),wood);
        foreach(float x in new[]{-9.61f,-8.3f})Box("Recovery files shelf bracket",new Vector3(x,1.66f,30.015f),new Vector3(.03f,.3f,.20f),metal);
        for(int i=0;i<5;i++){var f=Box("Recovery pending folio",new Vector3(-9.40f+i*.23f,1.97f,29.87f),new Vector3(.19f,.24f,.25f),paper);f.transform.rotation=Quaternion.Euler(0,i%2==0?3:-2,0);}
        Label("Recovery archive shelf identity","REVISIONES  /  03",new Vector3(-8.98f,2.23f,30.035f),0,.022f);
        Box("Recovery south notice board",new Vector3(-8.20f,1.68f,25.025f),new Vector3(1.28f,.81f,.035f),wood);
        for(int i=0;i<3;i++)
        {
            var f=Box("Recovery posted work sheet",new Vector3(-8.61f+i*.40f,1.70f,25.049f),new Vector3(.29f,.46f,.004f),paper);f.transform.rotation=Quaternion.Euler(0,0,i==1?-3:2);
            for(int row=0;row<8;row++)
            {var mark=Box("Recovery printed schedule line",Vector3.zero,new Vector3(row==0?.19f:.12f+((row+i)%3)*.025f,row==0?.010f:.003f,.002f),metal);mark.transform.position=f.transform.position+f.transform.rotation*new Vector3(-.015f,.155f-row*.038f,.004f);mark.transform.rotation=f.transform.rotation;}
        }
        var anchor=Object.FindFirstObjectByType<VS1RecallAnchor>(FindObjectsInactive.Include);var so=new SerializedObject(anchor);so.FindProperty("neutralStationLabel").stringValue="FIJAR ORIGEN\n[E]";so.FindProperty("activeStationLabel").stringValue="ORIGEN\nFIJADO";so.ApplyModifiedPropertiesWithoutUndo();
        ((TextMesh)so.FindProperty("statusLabel").objectReferenceValue).text="FIJAR ORIGEN\n[E]";
        var neutral=Mat("MonitorIdle",new Color(.012f,.020f,.018f),.2f);var active=Mat("MonitorConfirmed",new Color(.025f,.10f,.075f),.22f);Emission(active,new Color(.014f,.11f,.080f));Any("R1 Neutral inactive display").GetComponent<Renderer>().sharedMaterial=neutral;((GameObject)so.FindProperty("activeDisplay").objectReferenceValue).GetComponent<Renderer>().sharedMaterial=active;
    }
    static void FurnishOffice()
    {
        var slots=new Dictionary<string,Material>();
        slots["RO_Wood"]=Mat("RO_Wood",new Color(0.1600f,0.0910f,0.0430f).gamma,0.4400f,0.0000f);
        slots["RO_WoodEdge"]=Mat("RO_WoodEdge",new Color(0.1050f,0.0630f,0.0320f).gamma,0.4500f,0.0000f);
        slots["RO_Enamel"]=Mat("RO_Enamel",new Color(0.0750f,0.0980f,0.0800f).gamma,0.5000f,0.3200f);
        slots["RO_WornMetal"]=Mat("RO_WornMetal",new Color(0.2000f,0.2200f,0.1970f).gamma,0.5700f,0.7600f);
        slots["RO_DrawerFace"]=Mat("RO_DrawerFace",new Color(0.2350f,0.2520f,0.2110f).gamma,0.4000f,0.1200f);
        slots["RO_DarkRubber"]=Mat("RO_DarkRubber",new Color(0.0120f,0.0160f,0.0120f).gamma,0.2200f,0.0000f);
        slots["RO_Bakelite"]=Mat("RO_Bakelite",new Color(0.0390f,0.0340f,0.0230f).gamma,0.5700f,0.0000f);
        slots["RO_MonitorEnamel"]=Mat("RO_MonitorEnamel",new Color(0.2300f,0.2600f,0.2240f).gamma,0.4700f,0.1500f);
        slots["RO_BezelDark"]=Mat("RO_BezelDark",new Color(0.0250f,0.0340f,0.0290f).gamma,0.4100f,0.0000f);
        slots["RO_LabelBlank"]=Mat("RO_LabelBlank",new Color(0.4700f,0.4480f,0.3480f).gamma,0.1400f,0.0000f);
        slots["RO_LampDiffuser"]=Mat("RO_LampDiffuser",new Color(0.7000f,0.6520f,0.4900f).gamma,0.2200f,0.0000f);
        slots["RO_Brass"]=Mat("RO_Brass",new Color(0.2600f,0.1820f,0.0850f).gamma,0.5600f,0.7300f);
        slots["RO_Wood"]=wood;slots["RO_WoodEdge"]=wood;
        var desk=Model("Assets/_Game/Art/Models/RecoveryOffice/EP_RO_ReviewDesk.fbx","Recovery original review desk",new Vector3(-7.55f,0,27.8f),90,m=>slots[m.name]);
        Box("Recovery desk solid envelope",new Vector3(-7.55f,.38f,27.8f),new Vector3(.79f,.76f,1.65f),metal,true).GetComponent<Renderer>().enabled=false;
        // The existing display, text, interaction collider and checkpoint remain exact.
        Any("R1 Monitor housing").GetComponent<Renderer>().enabled=false;Any("R1 Monitor stem").GetComponent<Renderer>().enabled=false;
        Model("Assets/_Game/Art/Models/RecoveryOffice/EP_RO_ReviewMonitor.fbx","Recovery original monitor casing",new Vector3(-7.23f,1.48f,27.8f),90,m=>slots[m.name]);
        var chair=Model("Assets/_Game/Art/Models/ReceptionLife/EP_RL_ClericalChair.fbx","Recovery clerk chair",new Vector3(-6.88f,0,26.62f),300,m=>Load("Assets/_Game/Art/Registration/M_"+m.name+".mat"));
        Box("Recovery chair collision",new Vector3(-6.88f,.48f,26.62f),new Vector3(.66f,.96f,.66f),metal,true).GetComponent<Renderer>().enabled=false;
        var lampPlace=desk.GetComponentsInChildren<Transform>().First(t=>t.name=="ReviewDesk__TASKLAMP_PLACEMENT").position;
        var lamp=Model("Assets/_Game/Art/Models/RecoveryOffice/EP_RO_TaskLamp.fbx","Recovery original jointed lamp",lampPlace,90,m=>slots[m.name]);
        var source=lamp.GetComponentsInChildren<Transform>().First(t=>t.name=="TaskLamp__LIGHT_SOURCE");var aim=lamp.GetComponentsInChildren<Transform>().First(t=>t.name=="TaskLamp__LIGHT_AIM_TIP");
        var light=Spot("Recovery desk task pool",source.position,aim.position,new Color(1,.84f,.61f),2.3f,2.4f,100);
        var diffuser=lamp.GetComponentsInChildren<Renderer>().First(r=>r.name=="TaskLightDiffuser");diffuser.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        Emission(slots["RO_LampDiffuser"],new Color(.66f,.50f,.29f));Track(diffuser,light,slots["RO_LampDiffuser"].color,new Color(.66f,.50f,.29f));
        // Work in progress without another lore prompt or interaction.
        for(int i=0;i<3;i++){var form=Box("Recovery reviewed form",new Vector3(-7.36f,.766f+i*.002f,28.31f),new Vector3(.30f,.001f,.22f),paper);form.transform.rotation=Quaternion.Euler(0,8+i*2,0);}
        Box("Recovery desk pen",new Vector3(-7.3f,.777f,28.12f),new Vector3(.14f,.007f,.007f),metal);
    }
    static void DesaturateReturn()
    {
        var overlay=Any("SE3 Domestic Overlap");
        foreach(var r in overlay.GetComponentsInChildren<Renderer>(true))
        {if(r.sharedMaterials.Length==1&&r.sharedMaterial.name=="M_Walnut")r.sharedMaterial=wood;}
        foreach(var l in overlay.GetComponentsInChildren<Light>(true)){l.color=new Color(1,.89f,.73f);l.intensity*=.72f;}
        // The extreme amber was mostly inherited from the original domestic room.
        Any("Domestic Warm Light").GetComponent<Light>().color=new Color(1,.86f,.65f);
        Any("Domestic Threshold Spill").GetComponent<Light>().color=new Color(1,.82f,.58f);
        // Replace only the visible cubic practical with a layered shallow fitting.
        var old=Any("Far warm practical");var p=old.position;old.GetComponent<Renderer>().enabled=false;
        var previous=root;root=overlay;
        Box("Return practical enamel tray",p+Vector3.up*.02f,new Vector3(.40f,.045f,.19f),metal);
        var f=Box("Return practical opal face",p-Vector3.up*.01f,new Vector3(.35f,.02f,.14f),opal);
        Track(f.GetComponent<Renderer>(),Any("SE3 Warm Far End").GetComponent<Light>(),new Color(.65f,.61f,.48f),new Color(.50f,.41f,.25f));root=previous;
        // Scene-local shade materials retain brightness without flooding the room.
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(r.name!="ShadeOuter"&&r.name!="ShadeInner")continue;
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)if(mats[i].name=="M_DL_ShadeLinen"||mats[i].name=="M_DL_InnerDiffuser")
            {var m=Copy("SE2_"+mats[i].name,mats[i]);Emission(m,new Color(.62f,.49f,.32f));mats[i]=m;foreach(var tracking in r.GetComponents<PracticalEmission>()){var s=new SerializedObject(tracking);if(s.FindProperty("materialIndex").intValue==i){s.FindProperty("emission").colorValue=new Color(.62f,.49f,.32f);s.ApplyModifiedPropertiesWithoutUndo();}}}
            r.sharedMaterials=mats;
        }
    }
    [MenuItem("Echo Protocol/Author recovery observation office")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(FirstActProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        room=Any("R1 Clinical Observation Hub");root=new GameObject("Recovery / staffed records review").transform;root.SetParent(room);lights.Clear();
        paint=Copy("OfficeWall",Load("Assets/_Game/Art/Finish/M_ClinicPaint.mat"));paint.SetColor("_BaseColor",new Color(.38f,.42f,.38f));EditorUtility.SetDirty(paint);
        wood=Load("Assets/_Game/Art/Threshold/M_FurnitureWalnut.mat");metal=Mat("Enamel",new Color(.12f,.16f,.14f),.37f,.28f);paper=Mat("ArchivePaper",new Color(.55f,.50f,.37f),.15f);
        opal=Mat("Opal",new Color(.56f,.63f,.55f));Emission(opal,new Color(.31f,.39f,.31f));Room();FurnishOffice();DesaturateReturn();
        var budget=new SerializedObject(Object.FindFirstObjectByType<LightShadowBudget>());var list=budget.FindProperty("lights");int n=list.arraySize;list.arraySize+=lights.Count;for(int i=0;i<lights.Count;i++)list.GetArrayElementAtIndex(n+i).objectReferenceValue=lights[i];budget.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Recovery room authored. Desk/monitor import and camera inspection pending; do not claim completion.");
    }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Presentation;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using Object=UnityEngine.Object;

// Successor checkpoint: legible first use and a return through an actual records department.
public static class ReceptionProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Reception.unity";
    const string Art="Assets/_Game/Art/Reception";
    static Transform root; static TextMesh template;
    static Material paint,trim,wood,metal,card,glass,ink,lightFace;
    static Material Load(string path)=>AssetDatabase.LoadAssetAtPath<Material>(path);
    static void Ref(Object target,string field,Object value){var s=new SerializedObject(target);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static Material Mat(string name,Color c,float smooth=.25f,float metallic=0)
    {var path=Art+"/M_"+name+".mat";var m=Load(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);return m;}
    static GameObject Box(string name,Vector3 p,Vector3 s,Material m,bool solid=false)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static TextMesh Text(string name,string words,Vector3 p,float yaw,float width,float size=.032f)
    {var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.63f,.70f,.62f);float w=t.GetComponent<Renderer>().localBounds.size.x;if(w>width)t.characterSize*=width/w;return t;}
    static void Plate(string name,string words,Vector3 p,float yaw,float width,float height)
    {var g=Box(name+" backing",p,new Vector3(width,height,.025f),ink);g.transform.rotation=Quaternion.Euler(0,yaw,0);Text(name,words,p-g.transform.forward*.017f,yaw,width-.09f,.036f);}
    static void Shelf(float z)
    {
        // Bolted steel bays carry labelled paper cartons, visibly separate from electronic recordings.
        foreach(float x in new[]{-23.30f,-22.50f})foreach(float dz in new[]{-.82f,.82f})Box("Records rack upright",new Vector3(x,1.2f,z+dz),new Vector3(.04f,2.4f,.04f),metal);
        for(int tier=0;tier<4;tier++)
        {
            float y=.27f+tier*.58f;Box("Records rack shelf",new Vector3(-22.9f,y,z),new Vector3(.88f,.035f,1.70f),metal);
            for(int file=0;file<4;file++)
            {
                float fz=z-.60f+file*.4f;float h=file==2?.34f:.41f;
                Box("Paper archive carton",new Vector3(-22.85f,y+h*.5f+.02f,fz),new Vector3(.65f,h,.34f),card);
                Box("Paper archive lid",new Vector3(-22.85f,y+h+.032f,fz),new Vector3(.67f,.032f,.36f),card);
                Box("Carton index label",new Vector3(-22.519f,y+h*.55f,fz),new Vector3(.006f,.10f,.17f),paint);
                Box("Carton handle",new Vector3(-22.515f,y+h*.80f,fz),new Vector3(.006f,.025f,.10f),ink);
            }
        }
    }
    static Light Lamp(float z)
    {var p=new Vector3(-21.35f,2.8f,z);Box("Archive suspended fixture",p+Vector3.up*.045f,new Vector3(.55f,.07f,1.2f),metal);Box("Archive opal diffuser",p,new Vector3(.47f,.025f,1.08f),lightFace);var g=new GameObject("Archive shelf illumination");g.transform.SetParent(root);g.transform.position=p-Vector3.up*.10f;g.transform.LookAt(new Vector3(-22.85f,1.1f,z));var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=new Color(1,.82f,.59f);l.intensity=2.2f;l.range=5;l.spotAngle=110;l.innerSpotAngle=80;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.10f;return l;}
    static void Instruments()
    {
        var m=new Dictionary<string,Material>{
            {"TI_Enamel",Mat("TI_Enamel",new Color(.264f,.299f,.264f).gamma,.45f,.12f)},
            {"TI_DarkFace",Mat("TI_DarkFace",new Color(.035f,.049f,.042f).gamma,.58f,.2f)},
            {"TI_Rubber",Mat("TI_Rubber",new Color(.014f,.02f,.016f).gamma,.13f)},
            {"TI_FoldedSteel",Mat("TI_FoldedSteel",new Color(.17f,.204f,.177f).gamma,.52f,.68f)},
            {"TI_Fastener",Mat("TI_Fastener",new Color(.32f,.343f,.3f).gamma,.60f,.8f)},
            {"TI_ExposedEdge",Mat("TI_ExposedEdge",new Color(.2f,.229f,.2f).gamma,.45f,.65f)},
            {"TI_UseMark",Mat("TI_UseMark",new Color(.09f,.098f,.075f).gamma,.24f,.1f)}};
        foreach(var pair in new[]{new[]{"WallReviewHousing","Review origin instrument"},new[]{"DeskPreserveHousing","Original conservation terminal"}})
        {
            var target=GameObject.Find(pair[1]);if(!target)throw new Exception("Missing physical control "+pair[1]);target.GetComponent<Renderer>().enabled=false;
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/ThresholdInstruments/EP_TI_"+pair[0]+".fbx");if(!asset)throw new Exception("Missing instrument shell "+pair[0]);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(asset);model.name="Physical instrument / "+pair[0];model.transform.SetParent(root);var scale=model.transform.localScale;scale.x=-scale.x;model.transform.localScale=scale;model.transform.SetPositionAndRotation(target.transform.position,Quaternion.Euler(0,270,0)*model.transform.rotation);
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(v=>m[v.name]).ToArray();
        }
    }
    [MenuItem("Echo Protocol/Author reception and records return")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring reception");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(PressurePlantProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        root=new GameObject("Reception / legible protocol and records return").transform;
        template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");
        paint=Load("Assets/_Game/Art/Finish/M_ClinicPaint.mat");wood=Load("Assets/_Game/Art/Threshold/M_OV_Walnut.mat");if(!wood)wood=Load("Assets/_Game/Art/Overhaul/M_Walnut.mat");glass=Load("Assets/_Game/Art/Finish/M_ObservationGlass.mat");
        trim=Mat("ArchiveJoinery",new Color(.12f,.17f,.15f),.35f);metal=Mat("ShelfEnamel",new Color(.17f,.19f,.17f),.33f,.28f);card=Mat("PaperCarton",new Color(.35f,.28f,.19f),.12f);ink=Mat("Bakelite",new Color(.023f,.029f,.026f),.18f);lightFace=Mat("ArchiveOpal",new Color(.7f,.65f,.50f));lightFace.EnableKeyword("_EMISSION");lightFace.SetColor("_EmissionColor",new Color(.8f,.65f,.42f)*.5f);
        // A spatial three-step contract, readable where the decisions occur.
        foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="Register lane identity"||t.name=="Service lane identity"))text.gameObject.SetActive(false);
        Plate("Start recording instruction","REGISTRO\nMIRÁ A Y B",new Vector3(-2.75f,2.52f,-4.15f),0,1.16f,.48f);
        Plate("Cooperate after recall instruction","ACCESO\nCOOPERÁ CON TU RECORRIDO",new Vector3(2.50f,2.52f,-4.15f),0,1.72f,.48f);
        foreach(float x in new[]{-3.15f,-2.35f,1.9f,3.1f})Box("Protocol sign suspension",new Vector3(x,2.93f,-4.15f),new Vector3(.014f,.34f,.014f),metal);
        Plate("At B recall instruction","R / VOLVER\nY REPRODUCIR",new Vector3(-.365f,1.22f,11),90,.73f,.40f);
        var p2=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="P2 hold instruction");p2.text="LA PUERTA NECESITA\nUNA MIRADA CONTINUA";p2.characterSize=.029f;
        // The fixtures remain on their actual SE1 circuits, with neutral opal instead of blue blocks.
        var firstActOpal=Mat("InstitutionOpal",new Color(.48f,.54f,.48f),.3f);firstActOpal.EnableKeyword("_EMISSION");firstActOpal.SetColor("_EmissionColor",new Color(.36f,.47f,.39f));
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Cold Ceiling Panel")||r.name.StartsWith("Service Ceiling Panel")))
        {
            var b=r.bounds;r.sharedMaterial=firstActOpal;var l=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(l=>l.name=="Panel Light - "+r.name);
            if(l){l.color=new Color(.74f,.82f,.74f);var tracking=r.gameObject.AddComponent<PracticalEmission>();Ref(tracking,"source",l);Ref(tracking,"surface",r);}
            foreach(float side in new[]{-1f,1f}){Box("Institution fixture metal rim",b.center+new Vector3(side*(b.extents.x+.02f),.01f,0),new Vector3(.04f,.085f,b.size.z+.08f),metal);Box("Institution fixture metal rim",b.center+new Vector3(0,.01f,side*(b.extents.z+.02f)),new Vector3(b.size.x,.085f,.04f),metal);}
            Box("Institution diffuser retaining bar",b.center+Vector3.down*.035f,new Vector3(b.size.x,.018f,.03f),metal);
        }
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.name=="R3 existing Hub fixture bounce"))l.color=new Color(.74f,.82f,.74f);
        var director=Object.FindFirstObjectByType<CampaignDirector>();var boundary=root.gameObject.AddComponent<ReturnRecordingBoundary>();
        Ref(boundary,"returnGate",Object.FindFirstObjectByType<VS1HubReturnGate>());Ref(boundary,"campaign",director);Ref(boundary,"session",Object.FindFirstObjectByType<GameSession>());Ref(boundary,"player",Object.FindFirstObjectByType<FirstPersonController>());Ref(boundary,"loop",Object.FindFirstObjectByType<LoopController>());Ref(boundary,"recorder",Object.FindFirstObjectByType<EchoRecorder>());Ref(director,"returnRecordingBoundary",boundary);
        foreach(var old in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="R3 payoff wayfinding"))old.gameObject.SetActive(false);
        Plate("Return end of recording","FIN DE REGISTRO\nR INACTIVO / ORIGEN EN SUBSUELO",new Vector3(-17.02f,2.35f,46.40f),270,1.32f,.46f);
        var se1=Object.FindFirstObjectByType<VS1StrongEventOne>();var roomTone=Object.FindFirstObjectByType<EchoProtocol.Audio.CampaignRoomTone>();Ref(se1,"fluorescentCircuitSource",Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(a=>a.name=="Fluorescent Bed"));Ref(roomTone,"director",director);Ref(roomTone,"firstActEvent",se1);
        // Keep the tested solid boundary; glass replaces the visible middle of the west wall.
        var oldWall=GameObject.Find("R3 spine west wall");if(!oldWall||!oldWall.GetComponent<Collider>())throw new Exception("Expected R3 solid spine wall");oldWall.GetComponent<Renderer>().enabled=false;
        Box("Return west south wall",new Vector3(-20,1.6f,1.6f),new Vector3(.20f,3.2f,24.8f),paint);
        Box("Return west north wall",new Vector3(-20,1.6f,38.1f),new Vector3(.20f,3.2f,20.2f),paint);
        Box("Records window solid sill",new Vector3(-20,.42f,21),new Vector3(.20f,.84f,14),trim);
        Box("Records window lintel",new Vector3(-20,2.85f,21),new Vector3(.20f,.70f,14),paint);
        Box("Records window sill cap",new Vector3(-19.98f,.86f,21),new Vector3(.34f,.06f,14.05f),wood);
        Box("Records fixed glazing",new Vector3(-20,1.68f,21),new Vector3(.02f,1.61f,13.96f),glass);
        foreach(float z in new[]{14f,17.5f,21f,24.5f,28f})Box("Records glazing mullion",new Vector3(-19.97f,1.68f,z),new Vector3(.13f,1.66f,.07f),trim);
        Box("Records glazing transom",new Vector3(-19.97f,2.16f,21),new Vector3(.13f,.055f,14),trim);
        Box("Records room floor",new Vector3(-22,-.10f,21),new Vector3(4,.20f,14),Load("Assets/_Game/Art/Finish/M_Linoleum.mat"));
        Box("Records room ceiling",new Vector3(-22,3.10f,21),new Vector3(4,.20f,14),paint);
        Box("Records back wall",new Vector3(-24,1.5f,21),new Vector3(.2f,3,14),paint);
        foreach(float z in new[]{14f,28f})Box("Records end wall",new Vector3(-22,1.5f,z),new Vector3(4,3,.20f),paint);
        foreach(float z in new[]{15.5f,18f,20.5f,23f,25.5f})Shelf(z);
        Lamp(17.5f);Lamp(24.5f);
        Plate("Paper records identity","ARCHIVO EN PAPEL\nCONSULTA EN RECEPCIÓN",new Vector3(-19.88f,1.78f,27.25f),270,1.18f,.42f);
        // Three clear spans, not a procession of identical light boxes.
        foreach(float z in new[]{28.1f,13.9f,7f})
        {Box("Return transverse ceiling beam",new Vector3(-18.6f,3.04f,z),new Vector3(2.80f,.32f,.22f),trim);foreach(float x in new[]{-19.84f,-17.36f})Box("Return structural pilaster",new Vector3(x,1.45f,z),new Vector3(.11f,2.90f,.22f),trim);}
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.name=="R3 local cool fill"))
        {float z=l.transform.position.z;if(z>=14&&z<=28){l.intensity=.9f;l.color=new Color(.70f,.78f,.77f);}else if(z<8){l.color=new Color(.92f,.81f,.64f);l.intensity=1.8f;}}
        foreach(float x in new[]{-19.89f,-17.31f})
        {Box("Reception return lower timber",new Vector3(x,.47f,-.3f),new Vector3(.02f,.94f,14.6f),wood);Box("Reception return dado rail",new Vector3(x,.96f,-.3f),new Vector3(.045f,.04f,14.6f),trim);}
        Plate("Reception visible at return end","RECEPCIÓN  >",new Vector3(-18.60f,1.80f,-10.67f),180,1.65f,.33f);
        var budget=Object.FindFirstObjectByType<LightShadowBudget>();var so=new SerializedObject(budget);var list=so.FindProperty("lights");var added=root.GetComponentsInChildren<Light>();int n=list.arraySize;list.arraySize+=added.Length;for(int i=0;i<added.Length;i++)list.GetArrayElementAtIndex(n+i).objectReferenceValue=added[i];so.ApplyModifiedPropertiesWithoutUndo();
        Instruments();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Reception and records return authored. Original passage colliders preserved; actual-camera legibility and continuous return require inspection.");
    }
}

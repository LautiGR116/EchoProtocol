using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using EchoProtocol.Player;
using Object=UnityEngine.Object;

public static class FinishProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Finish.unity";
    const string Art="Assets/_Game/Art/Finish";
    static Transform root;static Material paint,wood,floor,paper,steel,ivory,cloth,black;static TextMesh template;
    static void Ref(Object o,string key,Object v){var s=new SerializedObject(o);s.FindProperty(key).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static Material Plain(string name,Color color,float smooth=.25f,float metallic=0)
    {string p=Art+"/M_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);return m;}
    static Material Pbr(string name,string source,Color tint,float smooth,float normal=.35f)
    {var m=Plain(name,tint,smooth);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+source+"_Diffuse.jpg"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+source+"_nor_gl.jpg"));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",normal);m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+source+"_MetalSmooth.png"));m.EnableKeyword("_METALLICSPECGLOSSMAP");return m;}
    static GameObject Box(string name,Vector3 p,Vector3 size,Material m)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static GameObject Model(string type,Vector3 p,float yaw=0)
    {var path="Assets/_Game/Art/Models/Finish/EP_"+type+".fbx";var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Finish "+type;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw+(type=="ValveControl"||type=="FuseBox"||type=="Clipboard"||type=="WallSconce"?180:0),0)*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name=="Wood"?wood:m.name=="Ivory"?ivory:m.name=="Enamel"?paint:m.name=="Black"?black:steel).ToArray();return g;}
    static TextMesh Label(string name,string text,Vector3 p,float yaw,float width,Color color)
    {var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=text;t.fontSize=72;t.characterSize=.021f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;var size=t.GetComponent<Renderer>().localBounds.size.x;if(size>width)t.characterSize*=width/size;return t;}
    static Light Spot(string name,Vector3 p,Vector3 target,Color c,float intensity,float range,float angle=95)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;g.transform.LookAt(target);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=c;l.intensity=intensity;l.range=range;l.spotAngle=angle;l.innerSpotAngle=angle*.75f;l.shadows=LightShadows.Soft;l.shadowBias=.025f;l.shadowNormalBias=.15f;return l;}
    static void Map(Renderer r,float metres)
    {var s=r.bounds.size;var mapping=r.GetComponent<SurfaceMapping>();if(!mapping)mapping=r.gameObject.AddComponent<SurfaceMapping>();mapping.Configure(r,s.y<Mathf.Min(s.x,s.z)?new Vector2(s.x/metres,s.z/metres):new Vector2(Mathf.Max(s.x,s.z)/metres,s.y/metres));}
    [MenuItem("Echo Protocol/Author finish pass")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play first");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        var scene=EditorSceneManager.OpenScene(CampaignProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        foreach(var path in Directory.GetFiles(Art+"/Textures").Where(p=>p.EndsWith(".jpg")||p.EndsWith(".png")))
        {var i=(TextureImporter)AssetImporter.GetAtPath(path);i.wrapMode=TextureWrapMode.Repeat;i.anisoLevel=8;i.maxTextureSize=1024;if(path.Contains("nor_gl")){i.textureType=TextureImporterType.NormalMap;i.sRGBTexture=false;}else if(path.Contains("Rough")||path.Contains("MetalSmooth"))i.sRGBTexture=false;i.SaveAndReimport();}
        paint=Pbr("ClinicPaint","painted_plaster_wall",new Color(.80f,.87f,.84f),.24f,.18f);
        floor=Pbr("Linoleum","old_linoleum_flooring_01",new Color(.46f,.60f,.65f),.52f,.32f);
        wood=Pbr("Walnut","wood_floor_worn",new Color(.56f,.50f,.44f),.36f,.35f);
        paper=Pbr("PaperedWall","decrepit_wallpaper",new Color(.78f,.75f,.66f),.14f,.19f);
        steel=Plain("PatinatedSteel",new Color(.18f,.21f,.20f),.42f,.68f);ivory=Plain("BoneEnamel",new Color(.68f,.65f,.55f),.36f);cloth=Plain("Herringbone",new Color(.19f,.145f,.115f),.03f);black=Plain("Carbon",new Color(.018f,.022f,.021f),.18f);
        var plaster=Plain("DomesticCeiling",new Color(.42f,.43f,.39f),.08f);
        root=new GameObject("Finish / materiality and inhabited detail").transform;
        var originalRenderers=Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach(var r in originalRenderers)
        {
            r.sharedMaterials=r.sharedMaterials.Select(m=>!m?m:m.name=="M_AgedPaint"||m.name=="M_VS1_VT1_ClinicWall"?paint:m.name=="M_Vinyl"||m.name=="M_VS1_VT1_ClinicFloor"?floor:m.name=="M_Timber"||m.name=="M_VS1_VT1_DomesticWood"?wood:m.name=="M_Wallpaper"?paper:m.name=="M_Upholstery"?cloth:m.name=="M_OxidizedSteel"?steel:m).ToArray();
            var f=r.GetComponent<MeshFilter>();if(f&&f.sharedMesh&&f.sharedMesh.name=="Cube"&&(r.sharedMaterial==paint||r.sharedMaterial==paper||r.sharedMaterial==floor||r.sharedMaterial==wood))Map(r,r.sharedMaterial==paper?3.8f:r.sharedMaterial==wood?2.5f:3.2f);
            if(r.name=="Domestic reconstruction ceiling"||r.name=="Departure ceiling")r.sharedMaterial=plaster;
            if(r.name=="House north"||r.name=="House south")r.sharedMaterial=paper;
        }
        var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        template=all.First(t=>t.name=="Pressure contract").GetComponent<TextMesh>();
        // Silence instruction walls. Contextual help contains the complete mechanical contract.
        foreach(var t in all)
        {
            if(t.GetComponent<TextMesh>()&&(t.name.Contains("contract")||t.name.Contains("facing")||t.name.Contains("Bay recording rule")||t.name.Contains("Bay companion rule")||t.name.Contains("Final capture rule")||t.name.Contains("Final witness rule")||t.name=="Departure final words"||t.name=="Record lane return"))t.gameObject.SetActive(false);
            if(t.name=="Protocol introduction"||t.name=="Protocol introduction board"||t.name=="Protocol start rules")t.gameObject.SetActive(false);
        }
        // Transform interaction proxies into objects. Colliders retain their tested, generous E-ray bounds.
        var director=Object.FindFirstObjectByType<CampaignDirector>();
        foreach(var station in Object.FindObjectsByType<CampaignStation>(FindObjectsSortMode.None))
        {
            var so=new SerializedObject(station);var display=(TextMesh)so.FindProperty("display").objectReferenceValue;if(display)display.gameObject.SetActive(false);
            station.GetComponent<Renderer>().enabled=false;var k=station.Kind;var p=station.transform.position;float yaw=station.transform.eulerAngles.y;GameObject prop=null;
            bool doc=k==StationKind.NoteArrival||k==StationKind.NoteWorkOrder||k==StationKind.NoteConsent||k==StationKind.ArchiveLetter;
            if(doc){prop=Model("Clipboard",p,yaw);Label("Document date",k==StationKind.NoteArrival?"03:17":k==StationKind.NoteWorkOrder?"6B / MANTENIMIENTO":k==StationKind.NoteConsent?"A. VERA":"M.",p-station.transform.forward*.025f+Vector3.up*.16f,yaw,.22f,new Color(.13f,.12f,.10f));}
            else if(k==StationKind.Telephone)
            { // A discrete plate marks the telephone extension; the phone itself remains on its table.
                station.transform.position=new Vector3(-6.5f,1.03f,-54);station.GetComponent<BoxCollider>().size=new Vector3(.72f,.52f,1.2f);
            }
            else if(k==StationKind.ValveA||k==StationKind.ValveB)prop=Model("ValveControl",p,yaw);
            else if(k==StationKind.CircuitKitchen||k==StationKind.CircuitBedroom||k==StationKind.CircuitHall)prop=Model("FuseBox",p,yaw);
            else
            {var face=Box("Terminal enamel housing",p,new Vector3(.48f,.42f,.1f),steel);face.transform.rotation=station.transform.rotation;var screen=Box("Terminal glass",p-station.transform.forward*.059f,new Vector3(.37f,.23f,.012f),black);screen.transform.rotation=station.transform.rotation;
                var title=k==StationKind.Preserve?"ARCHIVO / ORIGINAL":k==StationKind.HouseOrigin?"6B / RECONSTRUCCIÓN":k==StationKind.Descent?"02 / SUBSUELO":k==StationKind.GalleryOrigin?"04 / ACOMPAÑAR":k==StationKind.ArchiveOrigin?"05 / REVISIÓN":"02 / PRESIÓN";
                Label("Terminal engraving",title,p-station.transform.forward*.071f+Vector3.up*.04f,yaw,.33f,new Color(.37f,.69f,.61f));
            }
            if(!doc && k!=StationKind.Telephone)
            {
                var led=GameObject.CreatePrimitive(PrimitiveType.Sphere);led.name="Physical pilot light";led.transform.SetParent(root);led.transform.position=p-station.transform.forward*.17f+Vector3.up*(k==StationKind.ValveA||k==StationKind.ValveB?.38f:-.12f);led.transform.localScale=Vector3.one*.025f;Object.DestroyImmediate(led.GetComponent<Collider>());led.GetComponent<Renderer>().sharedMaterial=Plain("Pilot",new Color(.15f,.58f,.46f));var feedback=led.AddComponent<StationFeedback>();Ref(feedback,"station",station);Ref(feedback,"director",director);Ref(feedback,"pilot",led.GetComponent<Renderer>());
                if(k==StationKind.ValveA||k==StationKind.ValveB){var needle=Box("Live pressure needle",led.transform.position,new Vector3(.006f,.105f,.006f),black);needle.transform.rotation=station.transform.rotation;Ref(feedback,"needle",needle.transform);}
            }
        }
        // Single numerical marks replace paragraph-sized bay signage.
        for(int i=0;i<3;i++)for(int sign=-1;sign<=1;sign+=2)Label("Etched gallery bay",new[]{"I","II","III"}[i],new Vector3(sign*.17f,.72f,-83-i*8),sign<0?90:270,.16f,new Color(.55f,.59f,.54f));
        AuthorOpticalInstruments(all,director);
        Label("Final stand identifier","05",new Vector3(-.15f,.85f,-115),90,.2f,new Color(.6f,.7f,.63f));
        // Floor registration marks are paint, not floating solid-colored slabs.
        foreach(var mark in all.Where(t=>t.name=="Pressure footprint"||t.name.StartsWith("Record floor mark")||t.name.StartsWith("Companion floor mark")||t.name=="Final record mark"||t.name=="Final witness mark"))
        {var r=mark.GetComponent<Renderer>();var b=r.bounds;var m=r.sharedMaterial;mark.gameObject.SetActive(false);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {Box("Painted register corner",new Vector3(b.center.x+x*(b.size.x/2-.19f),.006f,b.center.z+z*b.size.z/2),new Vector3(.38f,.002f,.035f),m);Box("Painted register corner",new Vector3(b.center.x+x*b.size.x/2,.006f,b.center.z+z*(b.size.z/2-.19f)),new Vector3(.035f,.002f,.38f),m);}}
        // Remove point-light specular discs from observation glass; the frame and tint retain its boundary.
        var originalGlass=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/VS1/VT1/M_VS1_VT1_Glass.mat");var glazing=AssetDatabase.LoadAssetAtPath<Material>(Art+"/M_ObservationGlass.mat");if(!glazing){glazing=new Material(originalGlass);AssetDatabase.CreateAsset(glazing,Art+"/M_ObservationGlass.mat");}glazing.SetFloat("_SpecularHighlights",0);glazing.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        foreach(var r in originalRenderers)r.sharedMaterials=r.sharedMaterials.Select(m=>m==originalGlass?glazing:m).ToArray();
        // Lower wall panels give domestic rooms scale; they do not obstruct any route.
        var dado=Plain("DomesticDado",new Color(.115f,.155f,.137f),.34f);
        foreach(var t in all.Where(t=>t.name=="House corridor wall"||t.name=="House room partition"||t.name=="Domestic reconstruction perimeter"))
        {var size=t.GetComponent<Renderer>().bounds.size;var pos=t.position;pos.y=.48f;var g=Box("Painted domestic dado",pos,new Vector3(size.x+.014f,.96f,size.z+.014f),dado);
            var trim=Box("Domestic dado cap",pos+Vector3.up*.50f,new Vector3(size.x+.038f,.043f,size.z+.038f),wood);}
        // Human-scale objects establish use instead of another repeated locker.
        var desk=Model("ReceptionDesk",new Vector3(0,0,-4.55f),180);var collision=new GameObject("Reception desk solid");collision.transform.SetParent(root);collision.transform.position=new Vector3(0,.42f,-4.55f);var solid=collision.AddComponent<BoxCollider>();solid.size=new Vector3(2,.85f,.72f);Label("Institution brass letters","E C H O   /   REVISIÓN",new Vector3(0,2.05f,-4.08f),0,1.8f,new Color(.48f,.55f,.49f));Model("TableBelongings",new Vector3(-.45f,.889f,-4.55f),180);
        Model("TableBelongings",new Vector3(-6.35f,.775f,-54.05f),25);Model("TableBelongings",new Vector3(6.95f,.76f,-53.67f),15);Model("TableBelongings",new Vector3(-5.0f,.99f,-72.85f),0);
        foreach(var t in all.Where(t=>t.name=="Campaign Locker"&&t.position.z< -60&&t.position.z> -74))t.gameObject.SetActive(false);
        var letter=Model("Clipboard",new Vector3(.35f,1.03f,-4.64f),0);letter.transform.rotation=Quaternion.Euler(52,0,0)*letter.transform.rotation;
        var letterTarget=new GameObject("Mara letter / reception");letterTarget.transform.SetParent(root);letterTarget.transform.position=new Vector3(.35f,1.03f,-4.64f);var lc=letterTarget.AddComponent<BoxCollider>();lc.size=new Vector3(.45f,.32f,.32f);var ls=letterTarget.AddComponent<CampaignStation>();Ref(ls,"director",director);var lso=new SerializedObject(ls);lso.FindProperty("kind").enumValueIndex=(int)StationKind.IntroLetter;lso.FindProperty("caption").stringValue="Carta de Mara";lso.ApplyModifiedPropertiesWithoutUndo();
        // Match visible practicals to lights, avoid the ceiling-wide point-light wash.
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(l.name=="Practical light") {l.type=LightType.Point;l.intensity=3.5f;l.range=8.2f;l.color=new Color(.71f,.79f,.76f);l.shadowBias=.025f;l.shadowNormalBias=.15f;}
            if(l.name.Contains("lamp")||l.name=="Presence backlight"){l.color=new Color(1,.79f,.54f);l.shadowNormalBias=.14f;l.shadowBias=.02f;}
        }
        foreach(var p in new[]{new Vector3(-1.7f,2.15f,-58),new Vector3(1.7f,2.15f,-65),new Vector3(-1.7f,2.15f,-71)})
        {float yaw=p.x<0?270:90;Model("WallSconce",p,yaw);Spot("Domestic sconce pool",p+(p.x<0?Vector3.right:Vector3.left)*.21f,p+Vector3.down*2,new Color(1,.76f,.48f),1.2f,4.5f,120);}
        Spot("Reception downlight",new Vector3(0,2.85f,-5.5f),new Vector3(0,.8f,-4.5f),new Color(.71f,.78f,.72f),2.2f,7,100);
        // Moonlight gives the window an exterior and the furniture a cold edge.
        Spot("Parlor window spill",new Vector3(-7.6f,2.15f,-56),new Vector3(-3,.3f,-57),new Color(.33f,.44f,.57f),1.6f,8,70);
        Spot("Bedroom window spill",new Vector3(7.6f,2.15f,-57.5f),new Vector3(3,.2f,-56),new Color(.33f,.44f,.57f),1.5f,7,65);
        RenderSettings.fogDensity=.012f;RenderSettings.fogColor=new Color(.024f,.031f,.035f);RenderSettings.ambientLight=new Color(.115f,.125f,.13f);RenderSettings.reflectionIntensity=.35f;
        // Fluorescent tubes and cloth lampshades visibly own their illumination.
        var luminous=Plain("FluorescentGlass",new Color(.67f,.8f,.76f));luminous.EnableKeyword("_EMISSION");luminous.SetColor("_EmissionColor",new Color(.72f,.88f,.81f)*1.5f);
        var shade=Plain("LinenShade",new Color(.64f,.54f,.38f));shade.EnableKeyword("_EMISSION");shade.SetColor("_EmissionColor",new Color(.21f,.145f,.07f));
        foreach(var t in all.Where(t=>t.name=="Campaign Fluorescent"||t.name=="Campaign LampTable"))foreach(var r in t.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m&&m.name=="M_WarmDiffuser"?(t.name.Contains("Fluorescent")?luminous:shade):m).ToArray();
        // Route wayfinding is printed on a small plaque, not a tutorial billboard.
        foreach(var t in all.Where(t=>t.GetComponent<TextMesh>()))
        {var text=t.GetComponent<TextMesh>();if(t.name=="P1 A rule")text.text="A";if(t.name=="P1 B rule")text.text="B";if(t.name=="P1 service rule")text.text="C";if(t.name=="P2 hold instruction")text.text="PRESENCIA CONTINUA";if(t.name=="Register lane identity")text.text="01 / REGISTRO";if(t.name=="Service lane identity")text.text="02 / ACCESO";if(t.name=="Exit identity"){text.text="SUBSUELO";text.characterSize*=.55f;}if(new[]{"P1 A rule","P1 B rule","P1 service rule","P3 A identifier","P3 B identifier"}.Contains(t.name))text.characterSize*=.5f;}
        foreach(var t in all.Where(t=>t.name=="Campaign Fluorescent"||t.name=="Campaign LampTable"))foreach(var r in t.GetComponentsInChildren<Renderer>())for(int index=0;index<r.sharedMaterials.Length;index++)if(r.sharedMaterials[index]==luminous||r.sharedMaterials[index]==shade)
        {var nearest=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(l=>Vector3.Distance(l.transform.position,t.position)).First();var tracking=r.gameObject.AddComponent<PracticalEmission>();Ref(tracking,"source",nearest);Ref(tracking,"surface",r);var st=new SerializedObject(tracking);st.FindProperty("materialIndex").intValue=index;if(r.sharedMaterials[index]==shade){st.FindProperty("litColor").colorValue=new Color(.64f,.54f,.38f);st.FindProperty("emission").colorValue=new Color(.21f,.145f,.07f);}st.ApplyModifiedPropertiesWithoutUndo();}
        WireRecordedAudio();
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var ld=new SerializedObject(l.GetUniversalAdditionalLightData());ld.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;ld.ApplyModifiedPropertiesWithoutUndo();}
        var budget=root.gameObject.AddComponent<LightShadowBudget>();var budgetData=new SerializedObject(budget);var shadowLights=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.type!=LightType.Directional&&l.shadows!=LightShadows.None).ToArray();var array=budgetData.FindProperty("lights");array.arraySize=shadowLights.Length;for(int i=0;i<shadowLights.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=shadowLights[i];budgetData.ApplyModifiedPropertiesWithoutUndo();
        SetupRenderer();
        var session=Object.FindFirstObjectByType<GameSession>();var ses=new SerializedObject(session);ses.FindProperty("editionLabel").stringValue="ARCHIVO 6B / SESIÓN DE REVISIÓN";ses.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};Debug.Log("Finish scene authored with physical controls, PBR and localized lighting.");
    }
    static GameObject Disk(string name,Vector3 p,float diameter,float depth,Material m)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(root);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,0,90));g.transform.localScale=new Vector3(diameter,depth*.5f,diameter);g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void AuthorOpticalInstruments(Transform[] all,CampaignDirector director)
    {
        var idle=Plain("OpticalIdle",new Color(.18f,.115f,.04f),.72f);var live=Plain("OpticalAligned",new Color(.12f,.58f,.51f),.65f);live.shader=Shader.Find("Universal Render Pipeline/Unlit");live.SetColor("_BaseColor",new Color(.15f,.85f,.73f));
        foreach(var reader in Object.FindObjectsByType<EchoProtocol.Puzzle.OpticalReader>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var data=new SerializedObject(reader);var active=(GameObject)data.FindProperty("activeIndicator").objectReferenceValue;if(!active)continue;
            var p=active.transform.position;
            foreach(var t in all.Where(t=>t.GetComponent<Renderer>()&&(t.name.Contains("Housing")||t.name.Contains("housing")||t.name.Contains("Idle Lens")||t.name.Contains("neutral lens"))))
                if(Vector3.Distance(t.position,p)<.4f)t.GetComponent<Renderer>().enabled=false;
            Box("Optical enamel body",p+Vector3.right*.12f,new Vector3(.17f,.40f,.34f),steel);
            Disk("Optical bezel",p+Vector3.right*.018f,.255f,.055f,black);Disk("Optical neutral lens",p,.184f,.01f,idle);
            var donor=Disk("Temporary lens mesh",p,.17f,.014f,live);active.GetComponent<MeshFilter>().sharedMesh=donor.GetComponent<MeshFilter>().sharedMesh;active.transform.SetPositionAndRotation(p-Vector3.right*.009f,donor.transform.rotation);active.transform.localScale=donor.transform.localScale;active.GetComponent<Renderer>().sharedMaterial=live;Object.DestroyImmediate(donor);
            for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)Disk("Optical case fastener",p+new Vector3(.03f,y*.16f,z*.125f),.012f,.01f,ivory);
        }
        // P–02 follows the same directional alignment language. Player preview never creates pressure.
        var q=new Vector3(-7.87f,1.60f,-34);
        Box("Pressure reader enamel body",q,new Vector3(.14f,.40f,.34f),steel);
        var ring=Disk("Pressure reader bezel",q+Vector3.right*.10f,.255f,.055f,black);
        var lens=Disk("Pressure alignment lens",q+Vector3.right*.132f,.18f,.013f,live);
        var feedback=lens.AddComponent<PressureAlignmentFeedback>();Ref(feedback,"director",director);Ref(feedback,"surface",lens.GetComponent<Renderer>());
        Label("Pressure control identifier","P—02",q+new Vector3(.15f,.31f,0),270,.23f,new Color(.55f,.70f,.65f));
    }
    static void WireRecordedAudio()
    {
        string path="Assets/_Game/Audio/Clips/Finish/";
        foreach(var file in Directory.GetFiles(path,"*.wav")){var imp=(AudioImporter)AssetImporter.GetAtPath(file);if(imp==null)continue;var settings=imp.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;imp.defaultSampleSettings=settings;imp.forceToMono=true;imp.SaveAndReimport();}
        var d=Object.FindFirstObjectByType<CampaignDirector>();var ds=new SerializedObject(d);var sounds=ds.FindProperty("sounds");foreach(var pair in new[]{(0,"Relay"),(2,"Relay"),(3,"WallContact"),(4,"CloseMovement")})sounds.GetArrayElementAtIndex(pair.Item1).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(path+pair.Item2+".wav");ds.ApplyModifiedPropertiesWithoutUndo();
        var room=root.gameObject.AddComponent<EchoProtocol.Audio.CampaignRoomTone>();Ref(room,"player",Object.FindFirstObjectByType<FirstPersonController>());var rt=new SerializedObject(room);var beds=Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(a=>a.loop&&a.outputAudioMixerGroup&&a.outputAudioMixerGroup.name=="Ambience").ToArray();var rp=rt.FindProperty("institution");rp.arraySize=beds.Length;for(int i=0;i<beds.Length;i++)rp.GetArrayElementAtIndex(i).objectReferenceValue=beds[i];rt.ApplyModifiedPropertiesWithoutUndo();
        var machine=(AudioSource)ds.FindProperty("machinery").objectReferenceValue;machine.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path+"PressureBed.wav");
        var feet=Object.FindFirstObjectByType<EchoProtocol.Audio.PlayerFootsteps>();var fs=new SerializedObject(feet);for(int i=0;i<3;i++){fs.FindProperty("vinyl").GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(path+"FootVinyl"+i+".wav");fs.FindProperty("timber").GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(path+"FootWood"+i+".wav");}fs.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetupRenderer()
    {
        string path=Art+"/FinishRenderer.asset";
        if(!File.Exists(path))AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset",path);
        var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
        if(!data.rendererFeatures.Any(f=>f is ScreenSpaceAmbientOcclusion))
        {var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Contact occlusion";AssetDatabase.AddObjectToAsset(ao,data);var s=new SerializedObject(ao);var p=s.FindProperty("m_Settings");p.FindPropertyRelative("Intensity").floatValue=1.25f;p.FindPropertyRelative("Radius").floatValue=.20f;p.FindPropertyRelative("DirectLightingStrength").floatValue=.1f;p.FindPropertyRelative("Downsample").boolValue=true;p.FindPropertyRelative("AOMethod").enumValueIndex=1;s.ApplyModifiedPropertiesWithoutUndo();data.rendererFeatures.Add(ao);ao.Create();EditorUtility.SetDirty(data);}
        var rp=new SerializedObject(GraphicsSettings.currentRenderPipeline);var list=rp.FindProperty("m_RendererDataList");int index=-1;for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).objectReferenceValue==data)index=i;if(index<0){index=list.arraySize;list.arraySize++;list.GetArrayElementAtIndex(index).objectReferenceValue=data;rp.ApplyModifiedPropertiesWithoutUndo();}
        Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>().GetUniversalAdditionalCameraData().SetRenderer(index);
    }    public static void CaptureViews()
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
        for(int i=0;i<positions.Length;i++){camera.transform.position=positions[i];camera.transform.LookAt(directions[i]);camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("Docs/VisualEvidence/2026-09-30-Finish/view-"+i+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
        for(int i=0;i<labels.Length;i++)labels[i].GetComponent<Renderer>().sharedMaterial=oldMaterials[i];foreach(var m in temporary)Object.DestroyImmediate(m);
        camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);Debug.Log("Eight camera views saved");
    }

}

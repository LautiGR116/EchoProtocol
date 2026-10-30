using System;
using System.Collections.Generic;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Original architecture. The existing collision layout and all recorded channels
// are retained; this explicit installer only operates on the separate candidate.
public static class NegativeArchitectureIntegration
{
    const string RootName = "Negative / cathedral of pressure";
    const string Folder = "Assets/_Game/Art/Negative";
    static Transform root;
    static Material graphite, ivory, steel, glow, black, brass;

    [MenuItem("Echo Protocol/Negative/Build architecture in open Negative scene")]
    public static void ApplyToOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Exit Play and finish compilation first.");
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "EchoProtocol_Negative")
            throw new InvalidOperationException("Open the separate Negative scene. This tool does not open or save scenes.");
        if (scene.GetRootGameObjects().Any(g => g.name == RootName))
            throw new InvalidOperationException("Architecture already exists; edit it in place instead of stacking another installation.");
        var encounter = All<MachineCoverEncounter>().Single();
        var session = All<GameSession>().Single();
        var loop = All<LoopController>().Single();
        var output = All<SessionAudio>().Single();
        var player = new SerializedObject(encounter).FindProperty("player").objectReferenceValue as Component;
        var camera = player.GetComponentInChildren<Camera>(true);
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer");
        var bus = mixer.FindMatchingGroups("WorldSFX").Single(g => g.name == "WorldSFX");
        var oldLights = All<Light>().ToArray();
        var oldRenderers = All<MeshRenderer>().ToArray();
        EnsureFolder(Folder);
        graphite = Paint("Graphite", new Color(.055f,.065f,.066f), .22f, .43f);
        ivory = Paint("Ivory", new Color(.65f,.66f,.60f), .14f, .28f);
        steel = Paint("Steel", new Color(.18f,.20f,.20f), .7f, .5f);
        black = Paint("Black", new Color(.018f,.023f,.024f), .2f, .2f);
        brass = Paint("Ochre", new Color(.42f,.29f,.105f), .6f, .4f);
        glow = Paint("Work emission", new Color(.65f,.55f,.35f), .1f, .3f);
        glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor", new Color(2.2f,1.05f,.24f));
        EditorUtility.SetDirty(glow);
        AddConcreteGrain(graphite);
        root = new GameObject(RootName).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Negative architecture");

        // Replace the low tiled room by a vault. Preserve the floor, perimeter,
        // baffles and traversal colliders; only the obsolete ceiling is removed.
        foreach (var r in oldRenderers)
        {
            var n = r.name;
            if (n == "Pressure chamber ceiling")
            {
                Undo.RecordObject(r.gameObject,"Open vault"); r.gameObject.SetActive(false); continue;
            }
            if ((n == "Pressure intake" || n == "Campaign CeilingVent") && InRoom(r.bounds.center))
            { Undo.RecordObject(r.gameObject,"Clear low ceiling fittings"); r.gameObject.SetActive(false); continue; }
            if (n == "Pressure chamber floor" || n.StartsWith("Pressure chamber perimeter") ||
                n == "Central pressure baffle" || n == "ServiceCabinet_Geometry" ||
                n.StartsWith("Service / opaque") || n.StartsWith("Service / rear sound") ||
                n.StartsWith("Service / return south") || n.StartsWith("Return / opaque"))
            { Undo.RecordObject(r,"Paint chamber"); r.sharedMaterial = graphite; }
        }
        foreach (var text in All<TextMesh>().ToArray())
        {
            if (text.name == "Coverage / entry identity" || text.name == "Coverage / pump and pressure explanation" ||
                text.name == "Coverage / crank instruction" || text.name == "Coverage / exit release contract")
            {
                var plate = All<Transform>().FirstOrDefault(t => t.name == text.name + " / physical enamel plate");
                text.gameObject.SetActive(false); if (plate) plate.gameObject.SetActive(false);
            }
        }

        Box("Vault / upper left wall",new Vector3(-8.12f,5.5f,-38),new Vector3(.28f,5f,20),graphite);
        Box("Vault / upper right wall",new Vector3(8.12f,5.5f,-38),new Vector3(.28f,5f,20),graphite);
        Box("Vault / roof",new Vector3(0,8.1f,-38),new Vector3(16.5f,.3f,20.3f),black);
        Box("Vault / south end",new Vector3(0,5.5f,-48.12f),new Vector3(16.4f,5f,.24f),graphite);
        Box("Vault / north end",new Vector3(0,5.5f,-27.9f),new Vector3(16.4f,5f,.24f),graphite);
        var arch = ArchMesh();
        for (int i=0;i<7;i++)
        {
            var g = MeshObject("Vault / continuous rib " + (i+1),arch,ivory);
            g.transform.position = new Vector3(0,0,-29.5f-i*2.9f);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Vault / rib foot",new Vector3(side*7.45f,.65f,g.transform.position.z),new Vector3(.65f,1.3f,.7f),steel);
                Box("Vault / rib socket",new Vector3(side*7.45f,3f,g.transform.position.z),new Vector3(.54f,.32f,.6f),brass);
            }
        }
        for (int side=-1;side<=1;side+=2)
        {
            CylinderBetween("Vault / longitudinal main",new Vector3(side*6.9f,6.1f,-28.3f),new Vector3(side*6.9f,6.1f,-47.7f),.17f,steel);
            CylinderBetween("Vault / cable run",new Vector3(side*6.6f,5.85f,-28.3f),new Vector3(side*6.6f,5.85f,-47.7f),.075f,brass);
        }

        // The large central silhouette extends the existing baffle upward, so
        // no new physical obstacle is added to the authored escape route.
        Box("Resonator / vertical core",new Vector3(2.75f,4.45f,-39),new Vector3(1.4f,3.5f,5.6f),black);
        for(int i=0;i<9;i++)
        {
            float z=-36.35f-i*.66f;
            Box("Resonator / pale blade",new Vector3(2.75f,4.65f,z),new Vector3(1.85f,3.9f,.11f),ivory);
            Box("Resonator / crown spine",new Vector3(2.75f,6.68f,z),new Vector3(.65f,.32f,.2f),brass);
        }
        Box("Resonator / head",new Vector3(2.75f,6.65f,-39),new Vector3(1.9f,.24f,6),steel);
        CylinderBetween("Resonator / pressure trunk",new Vector3(2.75f,6.9f,-39),new Vector3(2.75f,8,-39),.25f,steel);
        for(int i=0;i<2;i++)
        {
            float z=-33f-i*10f;
            CylinderBetween("Reservoir / black vessel",new Vector3(6.65f,.25f,z),new Vector3(6.65f,5.5f,z),.65f,graphite);
            for(int j=0;j<6;j++) CylinderBetween("Reservoir / collar",new Vector3(6.65f,.6f+j*.88f,z),new Vector3(6.65f,.69f+j*.88f,z),.7f,steel);
            Box("Reservoir / measurement strip",new Vector3(5.98f,2.9f,z),new Vector3(.025f,4.1f,.12f),ivory);
            for(int j=0;j<12;j++) Box("Reservoir / measurement tick",new Vector3(5.96f,1+j*.31f,z+.12f),new Vector3(.025f,.025f,.18f),ivory);
            CylinderBetween("Reservoir / crown pipe",new Vector3(6.65f,5.5f,z),new Vector3(6.65f,6.1f,z),.22f,brass);
        }

        var shutters = new List<Transform>(); var panels = new List<Renderer>();
        for(int i=0;i<6;i++)
        {
            float z=-30.1f-i*3f;
            Box("Listening wall / recess",new Vector3(-7.84f,2.35f,z),new Vector3(.16f,3.7f,2.7f),black);
            var screen=Box("Listening wall / lightbox",new Vector3(-7.72f,2.35f,z),new Vector3(.06f,2.7f,2.35f),glow);
            panels.Add(screen.GetComponent<Renderer>());
            var panel=Box("Listening wall / shutter " + (i+1),new Vector3(-7.58f,5.8f,z),new Vector3(.16f,3.2f,2.6f),steel);
            shutters.Add(panel.transform);
            for(int j=0;j<7;j++)
            {
                var slat=Box("Shutter / heavy folded seam",new Vector3(-7.46f,4.4f+j*.43f,z),new Vector3(.09f,.07f,2.58f),black);
                slat.transform.SetParent(panel.transform,true);
            }
            for(int side=-1;side<=1;side+=2)
                Box("Listening wall / track",new Vector3(-7.43f,4.1f,z+side*1.4f),new Vector3(.12f,7.5f,.07f),brass);
        }

        var ambient = new List<Light>(); var work = new List<Light>(); var reserve = new List<Light>();
        foreach(var l in oldLights)
        {
            if(!InRoom(l.transform.position)) continue;
            bool protectedLamp=l.name.Contains("reserve") || l.name.Contains("exit") || l.name.StartsWith("Return /") ||
                l.name.Contains("pilot") || l.name.StartsWith("Presence /");
            if(protectedLamp) { reserve.Add(l); continue; }
            if(l.name == "Practical light" || l.name.Contains("reflection") || l.name.Contains("retained floor"))
            { l.intensity *= .45f; ambient.Add(l); }
            else if(l.name == "Service / maintained work light" || l.name == "Service / last practical" || l.name == "Pump / reflected control light")
            { l.color=new Color(.91f,.86f,.70f); work.Add(l); }
        }
        for(int i=0;i<3;i++)
        {
            var l=Lamp("Vault / diffuse work pool",new Vector3(-1.5f,6.6f,-31.8f-i*6),new Color(.76f,.84f,.84f),3.6f,12);
            ambient.Add(l);
            Box("Vault / luminous practical",l.transform.position+Vector3.up*.15f,new Vector3(1.5f,.08f,.3f),glow);
        }
        var task=Lamp("Resonator / working light",new Vector3(4.6f,4.5f,-35.3f),new Color(1,.83f,.56f),3.5f,10);
        work.Add(task);
        var head=Lamp("Negative / inspection lamp",camera.transform.position,new Color(.84f,.89f,1f),2.8f,11);
        head.transform.SetParent(camera.transform,false); head.transform.localPosition=new Vector3(.12f,-.13f,.08f);
        head.transform.localRotation=Quaternion.identity; head.type=LightType.Spot; head.spotAngle=63; head.innerSpotAngle=24;
        head.shadows=LightShadows.Soft; head.shadowStrength=.85f; head.shadowBias=.025f; head.shadowNormalBias=.15f;
        reserve.Add(head);

        Label("Negative / chamber identity","03",new Vector3(-5.8f,5.6f,-47.95f),Quaternion.identity,.16f,ivory.color);
        Label("Negative / chamber name","CÁMARA DEL NEGATIVO",new Vector3(-5.8f,4.05f,-47.94f),Quaternion.identity,.026f,ivory.color);
        Label("Negative / work identity","01  /  GRABAR",new Vector3(5.8f,2.6f,-35.98f),Quaternion.Euler(0,-90,0),.025f,Color.white);
        Label("Negative / cooperation identity","02  /  COOPERAR",new Vector3(-2.55f,2.8f,-39.8f),Quaternion.Euler(0,90,0),.022f,Color.white);

        var sequence=root.gameObject.AddComponent<NegativeRoomSequence>();
        var so=new SerializedObject(sequence);
        Ref(so,"session",session); Ref(so,"encounter",encounter); Ref(so,"loop",loop); Ref(so,"audioOutput",output);
        Refs(so,"shutters",shutters.Cast<Object>().ToArray());
        var offsets=so.FindProperty("closedLocalOffsets"); offsets.arraySize=shutters.Count;
        var angles=so.FindProperty("closedLocalAngles"); angles.arraySize=shutters.Count;
        // Shutters are primitive-scaled parents; positions remain world-local to
        // the unscaled room root. Their decorative seams move with them.
        for(int i=0;i<shutters.Count;i++) { offsets.GetArrayElementAtIndex(i).vector3Value=new Vector3(0,-3,0); angles.GetArrayElementAtIndex(i).vector3Value=Vector3.zero; }
        Refs(so,"ambientLights",ambient.Cast<Object>().ToArray()); Refs(so,"workLights",work.Cast<Object>().ToArray());
        Refs(so,"protectedLights",reserve.Cast<Object>().ToArray()); Refs(so,"luminousPanels",panels.Cast<Object>().ToArray());
        var clips=new[]{"Mechanisms/SFX_DoorMovement_LP","Mechanisms/SFX_DoorEndpoint","Intruder/Intruder_DoorPressure"};
        var cues=new AudioSource[3];
        for(int i=0;i<3;i++)
        {
            var g=new GameObject("Shutter / physical cue " + i); g.transform.SetParent(shutters[i==2?2:0],false);
            cues[i]=g.AddComponent<AudioSource>(); cues[i].clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/"+clips[i]+".wav");
            cues[i].outputAudioMixerGroup=bus; cues[i].playOnAwake=false; cues[i].spatialBlend=1; cues[i].dopplerLevel=0;
            cues[i].volume=i==0?.17f:.23f; cues[i].minDistance=1.8f; cues[i].maxDistance=19; cues[i].rolloffMode=AudioRolloffMode.Linear;
        }
        Refs(so,"cues",cues.Cast<Object>().ToArray()); so.ApplyModifiedPropertiesWithoutUndo();
        PostProcess();
        RenderSettings.fog=true; RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.016f;
        RenderSettings.fogColor=new Color(.035f,.043f,.047f);
        EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets();
        Debug.Log("Negative architecture installed: 8m vault, seven curved ribs, resonator, reservoirs, six causal shutters, protected inspection light and separate ACES profile. Scene UNSAVED.",root);
    }

    static void PostProcess()
    {
        string path=Folder+"/NegativeVolume.asset";
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if(!profile) { profile=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile,path); }
        var tone=profile.Add<Tonemapping>(true); tone.mode.value=TonemappingMode.ACES;
        var color=profile.Add<ColorAdjustments>(true); color.postExposure.value=.2f; color.contrast.value=18; color.saturation.value=-28;
        var bloom=profile.Add<Bloom>(true); bloom.intensity.value=.3f; bloom.threshold.value=1.15f; bloom.scatter.value=.55f;
        var vignette=profile.Add<Vignette>(true); vignette.intensity.value=.18f; vignette.smoothness.value=.55f;
        var grain=profile.Add<FilmGrain>(true); grain.type.value=FilmGrainLookup.Thin1; grain.intensity.value=.12f; grain.response.value=.8f;
        foreach(var effect in profile.components) if(!AssetDatabase.Contains(effect)) AssetDatabase.AddObjectToAsset(effect,profile);
        EditorUtility.SetDirty(profile);
        var volume=new GameObject("Negative / optical treatment").AddComponent<Volume>(); volume.transform.SetParent(root);
        volume.isGlobal=true; volume.priority=80; volume.weight=1; volume.sharedProfile=profile;
    }
    static IEnumerable<T> All<T>() where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
    static bool InRoom(Vector3 p) => p.z < -28 && p.z > -48 && Mathf.Abs(p.x)<8.3f;
    static void EnsureFolder(string path) { var parts=path.Split('/'); string at=parts[0]; for(int i=1;i<parts.Length;i++) { if(!AssetDatabase.IsValidFolder(at+"/"+parts[i])) AssetDatabase.CreateFolder(at,parts[i]); at+="/"+parts[i]; } }
    static Material Paint(string name,Color color,float metal,float smooth)
    {
        string path=Folder+"/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m) { m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path); }
        m.SetColor("_BaseColor",color); m.SetFloat("_Metallic",metal); m.SetFloat("_Smoothness",smooth); EditorUtility.SetDirty(m); return m;
    }
    static void AddConcreteGrain(Material material)
    {
        string path=Folder+"/ConcreteGrain.png";
        if(!System.IO.File.Exists(path))
        {
            var t=new Texture2D(256,256,TextureFormat.RGB24,false); var pixels=new Color[256*256];
            for(int y=0;y<256;y++) for(int x=0;x<256;x++)
            { float v=.66f+.22f*Mathf.PerlinNoise(x*.067f,y*.067f)+.1f*Mathf.PerlinNoise(x*.45f+21,y*.45f+17); pixels[y*256+x]=new Color(v,v,v); }
            t.SetPixels(pixels);t.Apply();System.IO.File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
        }
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));material.SetTextureScale("_BaseMap",new Vector2(3,3));EditorUtility.SetDirty(material);
    }
    static GameObject Box(string name,Vector3 p,Vector3 size,Material m)
    { var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.position=p;g.transform.localScale=size;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m;return g; }
    static void CylinderBetween(string name,Vector3 a,Vector3 b,float radius,Material m)
    { var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(root,false);g.transform.position=(a+b)*.5f;g.transform.up=(b-a).normalized;g.transform.localScale=new Vector3(radius*2,Vector3.Distance(a,b)*.5f,radius*2);Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m; }
    static GameObject MeshObject(string name,Mesh mesh,Material m)
    { var g=new GameObject(name);g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;return g; }
    static Mesh ArchMesh()
    {
        string path=Folder+"/ContinuousRib.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
        var points=new List<Vector2>{new Vector2(-7.45f,0),new Vector2(-7.45f,3)};
        for(int i=1;i<=48;i++) { float a=Mathf.PI-i*Mathf.PI/48;points.Add(new Vector2(Mathf.Cos(a)*7.45f,3+Mathf.Sin(a)*4.65f)); }
        points.Add(new Vector2(7.45f,0));var vertices=new List<Vector3>();var tris=new List<int>();
        for(int i=0;i<points.Count;i++)
        {
            Vector2 tangent=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)]).normalized;
            Vector2 normal=new Vector2(-tangent.y,tangent.x)*.17f;
            for(int j=0;j<4;j++) { var p=points[i]+normal*(j<2?1:-1);vertices.Add(new Vector3(p.x,p.y,(j==0||j==3)?-.23f:.23f)); }
            if(i==0)continue;
            for(int j=0;j<4;j++) { int a=(i-1)*4+j,b=(i-1)*4+(j+1)%4,c=i*4+(j+1)%4,d=i*4+j;tris.AddRange(new[]{a,b,c,a,c,d}); }
        }
        var mesh=new Mesh{name="Negative continuous structural rib"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static Light Lamp(string name,Vector3 p,Color c,float intensity,float range)
    { var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;return l; }
    static void Label(string name,string text,Vector3 p,Quaternion rotation,float size,Color color)
    {
        var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.SetPositionAndRotation(p,rotation);
        var t=g.AddComponent<TextMesh>();t.text=text;t.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/Art/Folio/Fonts/IBMPlexSans-Medium.ttf");
        t.fontSize=100;t.characterSize=size;t.color=color;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;
        g.GetComponent<Renderer>().sharedMaterial=t.font.material;
        var d=g.AddComponent<DiegeticText>();var s=new SerializedObject(d);Ref(s,"depthTestedShader",AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Shaders/DiegeticText.shader"));s.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Ref(SerializedObject so,string field,Object value) => so.FindProperty(field).objectReferenceValue=value;
    static void Refs(SerializedObject so,string field,Object[] values) {var p=so.FindProperty(field);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];}
}

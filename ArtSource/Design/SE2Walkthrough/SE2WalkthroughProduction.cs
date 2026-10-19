// Staged Editor author. Copy to Assets/Editor only after review. No Unity run here.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using Object = UnityEngine.Object;

public static class SE2WalkthroughProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Walkthrough.unity";
    public const string RootName = "SE2 / physical domestic passage";
    const string SourcePath = "Assets/_Game/Scenes/Production/EchoProtocol_Courtyard.unity";
    static Transform root, shared, cold, domestic;
    static Material clinic, plaster, timber, wallpaper, metal;

    static void Require(bool yes, string why) { if (!yes) throw new InvalidOperationException(why); }
    static Transform Named(string name)
    {
        var a = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.name == name).ToArray();
        Require(a.Length == 1, "Expected one " + name + ", found " + a.Length);
        return a[0];
    }
    static Object Ref(Object o, string field) => new SerializedObject(o).FindProperty(field).objectReferenceValue;
    static void SetRef(Object o, string field, Object v)
    { var s = new SerializedObject(o); var p = s.FindProperty(field); Require(p != null, "Missing field " + field); p.objectReferenceValue = v; s.ApplyModifiedPropertiesWithoutUndo(); }
    static Transform Node(string name, Transform parent)
    { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
    static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, bool physical)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, true); g.transform.SetPositionAndRotation(center, Quaternion.identity);
        g.transform.localScale = size; var r = g.GetComponent<Renderer>(); r.sharedMaterial = mat;
        if (!physical) Object.DestroyImmediate(g.GetComponent<Collider>());
        if (mat) g.AddComponent<SurfaceMapping>().Configure(r, size.y < Mathf.Min(size.x, size.z)
            ? new Vector2(size.x / 3.2f, size.z / 3.2f)
            : new Vector2(Mathf.Max(size.x, size.z) / 3.2f, size.y / 3.2f));
        return g;
    }
    static BoxCollider Volume(string name, Vector3 center, Vector3 size, Transform parent, bool trigger = true)
    {
        var t = Node(name, parent); t.position = center;
        var b = t.gameObject.AddComponent<BoxCollider>(); b.isTrigger = trigger; b.size = size; return b;
    }
    static void Floor(string name, float x0, float x1, float z0, float z1)
    { Box(shared, name, new Vector3((x0+x1)*.5f, -.10f, (z0+z1)*.5f), new Vector3(x1-x0,.20f,z1-z0), clinic, true); }
    static void WallX(string name, float x, float z0, float z1, float height = 2.9f)
    { Box(shared, name, new Vector3(x,height*.5f,(z0+z1)*.5f), new Vector3(.20f,height,z1-z0), plaster,true); }
    static void WallZ(string name, float z, float x0, float x1, float height = 2.9f)
    { Box(shared, name,new Vector3((x0+x1)*.5f,height*.5f,z),new Vector3(x1-x0,height,.20f),plaster,true); }
    static void Cover(string name, float x0,float x1,float z0,float z1,float height = 2.9f)
    { Box(shared,name,new Vector3((x0+x1)*.5f,height+.08f,(z0+z1)*.5f),new Vector3(x1-x0,.16f,z1-z0),plaster,true); }
    static void Move(Transform t, Vector3 position) { t.position = position; }
    static void Retire(string name) { Named(name).gameObject.SetActive(false); }
    static int[] ResetIDs(LoopController loop)
    {
        var a = new SerializedObject(loop).FindProperty("resetTargets");
        return Enumerable.Range(0,a.arraySize).Select(i=>a.GetArrayElementAtIndex(i).objectReferenceValue.GetInstanceID()).ToArray();
    }

    [MenuItem("Echo Protocol/Author SE2 physical domestic passage")]
    public static void Author()
    {
        Require(!EditorApplication.isPlaying,"Stop Play before authoring");
        Require(File.Exists(SourcePath),"Missing Courtyard checkpoint");
        Require(!File.Exists(ScenePath),"Destination exists; this author never overwrites a reviewed scene");
        var scene = EditorSceneManager.OpenScene(SourcePath);
        var e = Object.FindFirstObjectByType<VS1ParallelFigureEvent>();
        Require(e != null,"Missing current SE2");
        var loop = (LoopController)Ref(e,"loopController"); var resetIDs = ResetIDs(loop);
        var relay = Ref(e,"alignmentRelay"); var cameraRef = Ref(e,"playerView"); var glassRef = Ref(e,"visibilityGlass");
        var se3 = Object.FindFirstObjectByType<VS1OmittedPassageEvent>();
        var se3Position = se3.transform.position; var se3Rotation = se3.transform.rotation;
        var player = Object.FindFirstObjectByType<FirstPersonController>().GetComponent<LoopActor>();
        Require(player && player.Kind == LoopActorKind.Player,"Missing actual Player actor");
        // Resolve every source before changing the scene. Existing identities survive.
        cold = ((GameObject)Ref(e,"coldRoomRoot")).transform;
        domestic = ((GameObject)Ref(e,"domesticRoomRoot")).transform;
        var figure = ((GameObject)Ref(e,"figureRoot")).transform;
        var chair = Named("SE2 original / Armchair"); var console = Named("SE2 original / KeyConsole");
        var door = Named("SE2 original / DomesticDoor"); var window = Named("SE2 original / DressedWindow");
        var lamp = Named("SE2 original / FloorLamp");
        var lampLight = Named("SE2 actual standing lamp light");
        var roomLights = domestic.GetComponentsInChildren<Light>(true).Where(l => l.gameObject.activeSelf).ToArray();
        var coldLight = Named("Cold service fill");
        var coldFixture = Named("Service fixture backing"); var coldOpal = Named("Service fixture opal");
        var rearFixture = Named("Rear ceiling practical"); var rearOpal = Named("Rear practical opal");
        clinic = Named("Cold floor").GetComponent<Renderer>().sharedMaterial;
        timber = Named("Parlor continuous floor").GetComponent<Renderer>().sharedMaterial;
        wallpaper = Named("Parlor partition left").GetComponent<Renderer>().sharedMaterial;
        plaster = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Overhaul/M_Plaster.mat");
        metal = coldFixture.GetComponent<Renderer>().sharedMaterial;
        Require(clinic && timber && wallpaper && plaster && metal,"Missing existing finish materials");
        int audioCount = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
        int lightCount = Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;

        root = Node(RootName,null); shared = Node("SE2 permanent shell, collision and sources",root);
        Named("VS1 SE2 Impossible Domestic Chamber").position += Vector3.left * 4f;
        // Prevent root activation from toggling lights, fixed furniture contact or the rear door.
        foreach(var l in roomLights) l.transform.SetParent(shared,true);
        foreach(var t in new[]{coldLight,coldFixture,coldOpal,rearFixture,rearOpal,lamp,door}) t.SetParent(shared,true);
        foreach(Transform child in cold) child.gameObject.SetActive(false);
        foreach(Transform child in domestic)
            if(child!=figure && child!=chair && child!=console && child!=window) child.gameObject.SetActive(false);
        foreach(var r in figure.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode=ShadowCastingMode.Off;
        // Original rear room/body and imported furniture remain, with explicit staging poses.
        Move(chair,new Vector3(5.12f,0,30.40f));
        // Apply +116 relative yaw: original canonical world heading244 becomes90 (+X).
        chair.Rotate(Vector3.up,-154f,Space.World);
        Move(console,new Vector3(6.50f,0,27.33f));
        var lampDelta = new Vector3(4.90f,0,29.42f)-lamp.position;
        lamp.position+=lampDelta; lampLight.position+=lampDelta;
        // Preserve the model's importer rotation. Cable contact still needs the first camera.
        var leaf = door.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="DoorLeafPivot");
        leaf.RotateAround(leaf.position,Vector3.up,46f); // original -78 -> -32 degrees, fixed ajar.
        foreach(var mf in door.GetComponentsInChildren<MeshFilter>(true))
        {
            Require(mf.GetComponent<Collider>()==null,"Rear doorway unexpectedly has collision");
            var c=mf.gameObject.AddComponent<MeshCollider>(); c.sharedMesh=mf.sharedMesh;
        }
        Volume("SE2 common chair contact",new Vector3(5.12f,.56f,30.4f),new Vector3(1f,1.12f,1f),shared,false);
        Volume("SE2 common console contact",new Vector3(6.50f,.425f,27.33f),new Vector3(.82f,.85f,.42f),shared,false);
        Volume("SE2 permanent floor lamp contact",new Vector3(4.90f,.70f,29.42f),new Vector3(.32f,1.4f,.32f),shared,false);

        foreach(string n in new[]{"P2 East Leg Floor","P2 East Leg Ceiling","P2 East Leg South Wall","P2 Return North Wall"}) Retire(n);
        Floor("SE2 shortened approach floor",-1.45f,4f,28f,30.2f);
        Cover("SE2 shortened approach ceiling",-1.45f,4f,28f,30.2f,3.15f);
        WallZ("SE2 approach south west",28f,-1.45f,1.55f,3.2f);
        WallZ("SE2 approach south east",28f,3.05f,4.11f,3.2f);
        WallZ("SE2 return north west",30.2f,-4.15f,1.55f,3.2f);
        WallZ("SE2 return north east",30.2f,3.05f,4.11f,3.2f);
        // Actual floor, ceiling, outside faces and contact never swap.
        Volume("SE2 room continuous support",new Vector3(8f,-.10f,29.1f),new Vector3(8f,.20f,4.2f),shared,false);
        Box(shared,"SE2 fixed front threshold",new Vector3(4.30f,-.05f,29.1f),new Vector3(.60f,.10f,4.2f),clinic,false);
        Cover("SE2 fixed room ceiling",4f,12f,27f,31.2f,2.72f);
        WallX("SE2 exterior back",12f,27f,31.2f,2.72f);
        WallX("SE2 front north cheek",4f,30.125f,31.2f,2.72f);
        WallZ("SE2 south front jamb",27f,4f,4.2f,2.72f);
        WallZ("SE2 south side beyond entry",27f,5.6f,12f,2.72f);
        WallZ("SE2 north front wall",31.2f,4f,5.8f,2.72f);
        WallZ("SE2 north rear jamb",31.2f,7.2f,12f,2.72f);
        Box(shared,"SE2 south doorway head",new Vector3(4.9f,2.46f,27f),new Vector3(1.4f,.52f,.2f),plaster,true);
        Box(shared,"SE2 north doorway head",new Vector3(6.5f,2.46f,31.2f),new Vector3(1.4f,.52f,.2f),plaster,true);
        WallX("SE2 rear partition south",7.68f,27f,28.28f,2.72f);
        WallX("SE2 rear partition north",7.68f,29.4f,31.2f,2.72f);
        Box(shared,"SE2 rear partition head",new Vector3(7.68f,2.47f,28.84f),new Vector3(.17f,.5f,1.12f),plaster,true);

        // South refuge: a genuine 1.30m passage beside one solid screen.
        Floor("SE2 south upper landing",1.5f,4.07f,27f,28.1f);
        Floor("SE2 south lower landing",2.3f,5.7f,23.9f,27f);
        WallX("SE2 south upper west",1.5f,27f,27.9f);
        WallZ("SE2 offset west landing shoulder",27f,1.5f,2.3f);
        WallX("SE2 refuge west",2.3f,23.9f,27f);
        WallZ("SE2 refuge south",23.9f,2.3f,5.7f);
        WallX("SE2 entry passage east",5.7f,23.9f,26.9f);
        var screen=Box(shared,"SE2 solid reveal screen",new Vector3(3.885f,1.45f,27f),new Vector3(.37f,2.9f,3.5f),plaster,true).GetComponent<BoxCollider>();
        Cover("SE2 south refuge ceiling",2.3f,5.7f,23.9f,27f);
        Cover("SE2 south upper ceiling",1.5f,4.07f,27f,28.1f);
        // North return is static: no new locked leaf or arbitrary completion gate.
        Floor("SE2 north near landing",1.5f,3.9f,30.1f,31.2f);
        Floor("SE2 north return floor",1.5f,7.3f,31.2f,33.45f);
        WallX("SE2 north return west",1.5f,30.3f,33.45f);
        WallZ("SE2 north return rear",33.45f,1.5f,7.3f);
        WallX("SE2 north return east",7.3f,31.2f,33.45f);
        Box(shared,"SE2 north return cheek",new Vector3(3.885f,1.45f,31.175f),new Vector3(.37f,2.9f,2.15f),plaster,true);
        Cover("SE2 north near ceiling",1.5f,3.9f,30.1f,31.2f);
        Cover("SE2 north return ceiling",1.5f,7.3f,31.2f,33.45f);

        // All changing geometry lies inside x>=4.60. Common front strip is deliberate.
        foreach(var state in new[]{cold,domestic})
        {
            var floorMat=state==cold?clinic:timber;
            Box(state,"SE2 state floor",new Vector3(8.3f,-.035f,29.1f),new Vector3(7.4f,.07f,4.04f),floorMat,false);
            var finish=state==cold?clinic:wallpaper;
            // The open vanos have no finish across them.
            Box(state,"SE2 south interior finish",new Vector3(8.77f,1.35f,27.115f),new Vector3(6.30f,2.7f,.018f),finish,false);
            Box(state,"SE2 north front interior finish",new Vector3(5.20f,1.35f,31.085f),new Vector3(1.20f,2.7f,.018f),finish,false);
            Box(state,"SE2 north rear interior finish",new Vector3(9.60f,1.35f,31.085f),new Vector3(4.76f,2.7f,.018f),finish,false);
        }
        Box(cold,"SE2 clinical rear service cover",new Vector3(7.79f,1.35f,29.1f),new Vector3(.025f,2.7f,4.04f),clinic,false);
        Box(cold,"SE2 cold cabinet matches chair contact",new Vector3(5.12f,.56f,30.4f),new Vector3(1f,1.12f,1f),metal,false);
        Box(cold,"SE2 cold service table matches console",new Vector3(6.5f,.425f,27.33f),new Vector3(.82f,.85f,.42f),metal,false);

        Move(Named("SE2 Clinical approach pool"),new Vector3(0f,2.65f,29.1f));
        Move(Named("SE2 Clinical approach practical"),new Vector3(0f,3.1f,29.1f));
        Move(Named("SE2 Clinical barrier pool"),new Vector3(2.7f,2.65f,29.1f));
        Move(Named("SE2 Clinical barrier practical"),new Vector3(2.7f,3.1f,29.1f));
        e.transform.SetParent(root,true); e.transform.SetPositionAndRotation(new Vector3(1.55f,0,29.1f),Quaternion.identity);
        var arming=e.GetComponent<BoxCollider>();arming.center=new Vector3(0,1.1f,0);arming.size=new Vector3(4.7f,2.2f,2);arming.isTrigger=true;
        var entry=Volume("SE2 physical interior visit",new Vector3(6.1f,1.1f,29.1f),new Vector3(1.7f,2.2f,1.8f),root);
        var exit=Volume("SE2 physical return to approach",new Vector3(2.3f,1.1f,29.1f),new Vector3(1.5f,2.2f,1.6f),root);
        var pocket=Volume("SE2 reveal refuge",new Vector3(3.04f,1.2f,25.65f),new Vector3(.64f,2,.5f),root);
        var envelope=Volume("SE2 entire changing presentation",new Vector3(8.34f,1.34f,29.1f),new Vector3(7.48f,2.88f,4.4f),root);
        var es=new SerializedObject(e);es.FindProperty("physicalTraversal").boolValue=true;
        es.FindProperty("walkthroughPlayer").objectReferenceValue=player;
        es.FindProperty("traversalEntry").objectReferenceValue=entry;es.FindProperty("traversalExit").objectReferenceValue=exit;
        es.FindProperty("revealPocket").objectReferenceValue=pocket;es.FindProperty("changeEnvelope").objectReferenceValue=envelope;
        var screens=es.FindProperty("opaqueScreens");screens.arraySize=1;screens.GetArrayElementAtIndex(0).objectReferenceValue=screen;
        es.ApplyModifiedPropertiesWithoutUndo();
        var timberZone=Volume("SE2 domestic actual floor contact",new Vector3(8.3f,1f,29.1f),new Vector3(7.4f,2f,4.04f),domestic);
        var steps=Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(x=>x.GetType().Name=="PlayerFootsteps");
        var ss=new SerializedObject(steps);var zones=ss.FindProperty("timberZones");int zn=zones.arraySize;zones.arraySize++;zones.GetArrayElementAtIndex(zn).objectReferenceValue=timberZone;ss.ApplyModifiedPropertiesWithoutUndo();
        var session=Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(x=>x.GetType().Name=="GameSession");
        var sessionSO=new SerializedObject(session);var key=sessionSO.FindProperty("campaignCheckpointKey");
        Require(key!=null,"Confirm current GameSession checkpoint key field before saving");key.stringValue="EP.CampaignCheckpoint.Walkthrough.v1";sessionSO.ApplyModifiedPropertiesWithoutUndo();

        Require(Ref(e,"loopController")==loop && Ref(e,"alignmentRelay")==relay && Ref(e,"playerView")==cameraRef && Ref(e,"visibilityGlass")==glassRef,"Existing SE2 refs changed");
        Require(ResetIDs(loop).SequenceEqual(resetIDs),"resetTargets changed");
        Require(se3.transform.position==se3Position && se3.transform.rotation==se3Rotation,"SE3 moved");
        Require(Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==audioCount,"AudioSource count changed");
        Require(Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==lightCount,"Light count changed");
        // Fail before saving if reused/imported changing geometry escaped the declared guard.
        Physics.SyncTransforms();var eb=envelope.bounds;
        foreach(var state in new[]{cold,domestic}) foreach(var r in state.GetComponentsInChildren<Renderer>(true))
        {
            if(!r.enabled || !LocalActive(r.transform,state))continue;
            var b=r.bounds;Require(eb.Contains(b.min)&&eb.Contains(b.max),"Changing renderer outside envelope: "+r.name+" "+b);
        }
        EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene,ScenePath),"Save failed");AssetDatabase.SaveAssets();
        Debug.Log("SE2 walkthrough authored; refs/P2/resetTargets/SE3 preserved; sources="+audioCount+" lights="+lightCount+"; chamber -4X; fixed32deg rear leaf. First camera must select cold/common light, refuge, furniture, door gap and actual passage.");
    }
    static bool LocalActive(Transform t,Transform state)
    {for(var p=t;p&&p!=state;p=p.parent)if(!p.gameObject.activeSelf)return false;return true;}
}

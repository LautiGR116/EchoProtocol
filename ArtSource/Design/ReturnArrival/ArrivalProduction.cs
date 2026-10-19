// Staged Editor author. Import/compile/run only after preserving the current checkpoint.
// Geometry/placement only: no new runtime rules, sound, lights or recorded channels.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class ArrivalProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Arrival.unity";
    public const string RootName = "Arrival / remembered reception";
    const string Art = "Assets/_Game/Art/Arrival";
    const string Report = "Docs/VisualEvidence/2026-10-01-Arrival/authoring.txt";
    static readonly Vector3 Front = new Vector3(-5.1f,0,-9.5f);
    static readonly Vector3 Forward = new Vector3(1,0,1).normalized;
    static readonly Quaternion Turn = Quaternion.Euler(0,-45,0);
    static readonly Vector3 Junction = Front - Forward * 7;
    static readonly Vector3 Bend = new Vector3(-18.6f,0,Junction.z + Junction.x + 18.6f);
    static readonly Vector3[] Route = { new Vector3(-18.6f,0,7), Bend, Junction, Front };
    static Scene scene;
    static Transform visuals, solids;
    static Material paint, floor, timber, trim;
    static T[] All<T>() where T : Component => Object.FindObjectsByType<T>(FindObjectsInactive.Include,
        FindObjectsSortMode.None).Where(x => x && x.gameObject.scene == scene).ToArray();
    static Transform Named(string name) => All<Transform>().Single(t => t.name == name);
    static Transform At(string name, Vector3 p) => All<Transform>().Single(t => t.name == name && Vector3.Distance(t.position,p)<.01f);
    static T Ref<T>(Object o,string field) where T:Object => new SerializedObject(o).FindProperty(field).objectReferenceValue as T;
    static void Require(bool b,string reason) { if(!b)throw new InvalidOperationException(reason); }
    static string Json(Object o) => EditorJsonUtility.ToJson(o);
    static string V(Vector3 p) => p.ToString("F5");
    static bool Under(Transform t, IEnumerable<Transform> roots) => roots.Any(r => t==r || t.IsChildOf(r));
    static Material MaterialOn(string name) => Named(name).GetComponent<Renderer>().sharedMaterial;
    static Vector3 TurnPoint(Vector3 p) => Front + Turn*(p-Front);
    static Transform Group(string name,Transform parent)
    { var t=new GameObject(name).transform;t.SetParent(parent,false);return t; }

    [MenuItem("Echo Protocol/Author oblique R3 arrival")]
    public static void Author()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before Arrival authoring.");
        Require(!File.Exists(ScenePath)&&!Directory.Exists(Art)&&!File.Exists(Report),"Arrival output already exists; preserve it instead of overwriting.");
        Require(Enumerable.Range(0,SceneManager.sceneCount).All(i=>!SceneManager.GetSceneAt(i).isDirty),"Save open scenes first.");
        scene=EditorSceneManager.OpenScene(OrientationProduction.ScenePath);
        var gate=All<VS1HubReturnGate>().Single();var director=All<CampaignDirector>().Single();
        var rear=Ref<SlidingDoor>(gate,"arrivalRear");var front=Ref<SlidingDoor>(gate,"arrivalFront");
        var interior=Ref<BoxCollider>(gate,"arrivalInterior");var clear=Ref<BoxCollider>(gate,"arrivalClear");
        var occupancy=Ref<BoxCollider>(gate,"arrivalOccupancy");var guide=Ref<BoxCollider>(director,"returnArrivalZone");
        Require(rear&&front&&interior&&clear&&occupancy&&guide,"Missing explicit arrival contract.");
        Require(Vector3.Distance(rear.transform.position,new Vector3(-9.2f,0,-9.5f))<.001f &&
            Vector3.Distance(front.transform.position,Front)<.001f,"Arrival baseline moved.");
        Require(Quaternion.Angle(rear.transform.rotation,Quaternion.Euler(0,90,0))<.01f &&
            Quaternion.Angle(front.transform.rotation,Quaternion.Euler(0,90,0))<.01f,"Unexpected door basis.");
        Require(All<VS1DenialEvidence>().All(d=>!d.isActiveAndEnabled),"Legacy D1 must remain disabled.");
        var volumes=new[]{interior,clear,occupancy,guide};
        var expected=new[]{new Vector3(-6.85f,1,-9.5f),new Vector3(-3.6f,1,-9.5f),new Vector3(-7.15f,1,-9.5f),new Vector3(-3.45f,1,-9.5f)};
        for(int i=0;i<volumes.Length;i++) Require(volumes[i].isTrigger&&volumes[i].enabled&&
            Vector3.Distance(volumes[i].transform.position,expected[i])<.001f&&
            Quaternion.Angle(volumes[i].transform.rotation,Quaternion.identity)<.01f&&
            Vector3.Distance(volumes[i].transform.lossyScale,Vector3.one)<.001f,"Unexpected volume "+volumes[i].name);
        var motors=new[]{rear,front}.Select(d=>All<SlidingDoorAudio>().Single(a=>Ref<SlidingDoor>(a,"door")==d)).ToArray();
        for(int i=0;i<motors.Length;i++)
        {
            var door=i==0?rear:front;var source=Ref<AudioSource>(motors[i],"mechanismSource");
            Require(source&&source.transform==motors[i].transform&&source.outputAudioMixerGroup&&
                Ref<Rigidbody>(motors[i],"doorBody")==Ref<Rigidbody>(door,"doorBody")&&
                Vector3.Distance(source.transform.position,door.transform.position+Vector3.up*2.75f)<.001f,
                "Motor must be the explicitly wired fixed source at its existing header.");
        }
        var rigid=new List<Transform>{rear.transform,front.transform,interior.transform,clear.transform,
            occupancy.transform,guide.transform,Named("R3 arrival rear header"),Named("R3 Hub exit only header"),
            motors[0].transform,motors[1].transform};
        rigid.AddRange(Fixture(-10.5f,-9.5f));rigid.AddRange(Fixture(-7,-9.5f));
        Require(rigid.All(t=>Vector3.Distance(t.parent.lossyScale,Vector3.one)<.001f) &&
            rigid.All(t=>!rigid.Any(other=>other!=t&&t.IsChildOf(other))),"Rigid roots overlap or inherit scale.");
        var fixtureA=Fixture(-18.6f,-8);var fixtureB=Fixture(-14,-9.5f);
        var resize=new[]{Named("R3 spine floor"),Named("R3 spine ceiling"),Named("R3 spine east wall"),
            Named("Return west south wall"),Named("West original south perimeter"),Named("Hub skirting -1")};
        Require(!Named("R3 spine west wall").GetComponent<Collider>().enabled &&
            !Named("R3 spine west wall").GetComponent<Renderer>().enabled,"Records west-wall retirement changed.");
        Require(!Named("Hub West Wall").gameObject.activeSelf&&!Named("Hub South Wall").gameObject.activeSelf,
            "Do not reactivate the original full Hub walls.");
        var retire=new[]{"R3 south end","R3 arrival floor","R3 arrival ceiling","R3 arrival north wall",
            "R3 arrival south wall","R3 Hub west south pier","R3 return wayfinding",
            "Reception visible at return end","Reception visible at return end backing","Hub panel seam -1 2","Hub panel seam -1 3"}.Select(Named)
            .Concat(All<Transform>().Where(t=>t.name=="Reception return lower timber"||t.name=="Reception return dado rail")).ToArray();
        Require(retire.Length==15,"Unexpected number of obsolete southern-envelope objects.");
        paint=MaterialOn("Return west south wall");floor=MaterialOn("R3 spine floor");
        timber=All<Renderer>().First(r=>r.name=="Reception return lower timber").sharedMaterial;
        trim=All<Renderer>().First(r=>r.name=="Reception return dado rail").sharedMaterial;
        Require(paint&&floor&&timber&&trim,"Missing inherited materials.");

        var marking=Named("R3 Hub reverse marking").GetComponent<TextMesh>();
        Require(marking&&marking.font&&marking.GetComponent<DiegeticText>()&&Shader.Find("EchoProtocol/PrintedText")&&
            AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Orientation/M_ReturnGalleryEnamel.mat"),"Missing physical-sign support.");
        var moving=rigid.Concat(fixtureA).Concat(fixtureB).Concat(new[]{marking.transform}).ToArray();
        var transforms=All<Transform>();var world=transforms.ToDictionary(t=>t,t=>(new Pose(t.position,t.rotation),t.lossyScale));
        var active=transforms.ToDictionary(t=>t,t=>t.gameObject.activeSelf);
        // Compare every original runtime component; only the namespace and mapping on resized
        // static surfaces are intentionally changed. Door local targets/body settings stay byte-identical.
        var unchanged=All<MonoBehaviour>().Where(c=>c!=director&&c.transform!=marking.transform&&!(c is SurfaceMapping&&resize.Contains(c.transform))).Cast<Object>()
            .Concat(All<Light>()).Concat(All<AudioSource>()).Concat(All<Rigidbody>()).Concat(All<Renderer>().Where(r=>r.transform!=marking.transform))
            .Concat(All<Collider>().Where(c=>c.transform!=resize[4])).Distinct().ToDictionary(c=>c,Json);
        var directorBefore=Json(director);var key=new SerializedObject(director).FindProperty("checkpointKey").stringValue;
        int audioCount=All<AudioSource>().Length,lightCount=All<Light>().Length,colliderCount=All<Collider>().Length;
        var report=new StringBuilder("Arrival authoring only; no Play/input/listening/human-experience evidence.\n");
        report.AppendLine(OrientationProduction.ScenePath+" -> "+ScenePath);
        Require(EditorSceneManager.SaveScene(scene,ScenePath),"Could not derive Arrival.");
        AssetDatabase.CreateFolder("Assets/_Game/Art","Arrival");
        var root=Group(RootName,null);visuals=Group("Visible architecture",root);solids=Group("Physical architecture",root);
        var assembly=Group("Existing arrival interlock / rigid placement",root);assembly.position=Front;
        foreach(var t in rigid)t.SetParent(assembly,true);
        assembly.rotation=Turn;
        MoveFixture(fixtureA,Bend+new Vector3(1,0,-1).normalized*3,135);
        MoveFixture(fixtureB,Bend+new Vector3(1,0,-1).normalized*8,135);
        foreach(var t in retire)t.gameObject.SetActive(false);
        ResizeZ(resize[0],7,48.2f);ResizeZ(resize[1],7,48.3f);ResizeZ(resize[2],7,43.4f);
        ResizeZ(resize[3],7,14); // Renderer only; its explicit Records collider remains separate.
        ResizeZ(resize[5],-8.2f,-4.1f); // Keep the Hub skirting only on its surviving north wall.
        var perimeter=resize[4].GetComponent<BoxCollider>();
        Require(perimeter&&Vector3.Distance(resize[4].lossyScale,Vector3.one)<.001f,"Unexpected Records perimeter scale.");
        resize[4].position=new Vector3(-20,1.6f,10.5f);perimeter.size=new Vector3(.2f,3.2f,7);
        Corridor();PhysicalMarking(marking);
        var config=new SerializedObject(director);config.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Arrival.v1";
        config.ApplyModifiedPropertiesWithoutUndo();Physics.SyncTransforms();

        Require(All<AudioSource>().Length==audioCount&&All<Light>().Length==lightCount,"Unexpected sound/light count change.");
        foreach(var pair in unchanged)Require(Json(pair.Key)==pair.Value,"Serialized contract changed: "+pair.Key.name+" / "+pair.Key.GetType().Name);
        // Prove the only CampaignDirector change by temporarily comparing its original namespace.
        config.Update();config.FindProperty("checkpointKey").stringValue=key;config.ApplyModifiedPropertiesWithoutUndo();
        Require(Json(director)==directorBefore,"CampaignDirector changed beyond checkpoint namespace.");
        config.Update();config.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Arrival.v1";config.ApplyModifiedPropertiesWithoutUndo();
        foreach(var t in transforms)
        {
            if(!retire.Contains(t))Require(t.gameObject.activeSelf==active[t],"Unexpected active-state change: "+t.name);
            if(!Under(t,moving)&&!resize.Contains(t))Require(Vector3.Distance(t.position,world[t].Item1.position)<.0001f&&
                Quaternion.Angle(t.rotation,world[t].Item1.rotation)<.01f&&Vector3.Distance(t.lossyScale,world[t].Item2)<.0001f,
                "Unrelated pose changed: "+t.name);
        }
        foreach(var t in rigid)
        { Require(Vector3.Distance(t.position,TurnPoint(world[t].Item1.position))<.0001f,"Rigid position drift: "+t.name);
          report.AppendLine(t.name+": "+V(world[t].Item1.position)+" -> "+V(t.position)+"; yaw "+t.eulerAngles.y.ToString("F3")); }
        foreach(var t in fixtureA.Concat(fixtureB))report.AppendLine(t.name+": "+V(world[t].Item1.position)+" -> "+V(t.position));
        foreach(var t in resize)report.AppendLine("Trimmed "+t.name+" position="+V(t.position)+" scale="+V(t.lossyScale));
        report.AppendLine("Retired originals: "+string.Join(", ",retire.Select(t=>t.name)));
        foreach(var volume in volumes)report.AppendLine(volume.name+" size="+V(volume.size)+" centre="+V(volume.center)+"; same Collider reference");
        foreach(var door in new[]{rear,front})
        {
            var body=Ref<Rigidbody>(door,"doorBody");
            Require(body.transform.parent==door.transform,"Door body parent changed.");
            report.AppendLine(door.name+" keeps Rigidbody, closed/open local targets, movement speed and actual-motion sound references.");
        }
        report.AppendLine("Original component serializations preserved: "+unchanged.Count+"; lights "+lightCount+" -> "+All<Light>().Length+
            "; AudioSources "+audioCount+" -> "+All<AudioSource>().Length+"; colliders "+colliderCount+" -> "+All<Collider>().Length+" (obsolete objects inactive).");
        report.AppendLine("No root movement for residualRoot, arrivalLightRoot, Registration, P3 crossing or R1. Archive unchanged except documented southern perimeter trim z[7,14].");
        report.AppendLine("Centreline: "+string.Join(" -> ",Route.Select(V))+" -> "+V(Front+Forward*2.3f)+". Clear wall width 2.60m, dado minimum 2.503m / existing door aperture 2.40m.");
        report.AppendLine("Old suspended reverse marking replaced in-place by RETORNO / SOLO SALIDA on opaque Hub-facing header plate; PrintedText owner wired, no emission/new light.");
        report.AppendLine("Checkpoint "+key+" -> EP.CampaignCheckpoint.Arrival.v1; no PlayerPrefs writes. Requires local-box Inside/AtReviewAnnouncement already integrated by root.");
        report.AppendLine("Pending actual-input checks: corners/floor, interlock retreat/re-entry, real motor positions, guide once, final camera toward original booth, initial Hub reverse face. No recognition or audibility claim.");
        EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Could not save Arrival.");AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory(Path.GetDirectoryName(Report));File.WriteAllText(Report,report.ToString());
        Debug.Log("Arrival derived; inspect "+Report+" and actual-input cameras before selection.");
    }

    [MenuItem("Echo Protocol/Clean Arrival old Hub trim")]
    public static void CleanOldHubTrim()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before trimming Arrival.");
        scene=SceneManager.GetActiveScene();Require(scene.path==ScenePath,"Trim cleanup belongs to Arrival.");
        foreach(string name in new[]{"Hub panel seam -1 2","Hub panel seam -1 3"})
        { var t=Named(name);Require(!t.GetComponent<Collider>(),"Old seam unexpectedly has collision.");t.gameObject.SetActive(false); }
        var skirting=Named("Hub skirting -1");Require(!skirting.GetComponent<Collider>(),"Old skirting unexpectedly has collision.");
        ResizeZ(skirting,-8.2f,-4.1f);
        EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Could not save trim cleanup.");AssetDatabase.SaveAssets();
        File.AppendAllText(Report,"Camera-driven finish: retired noncolliding Hub panel seam -1 2/3 and shortened Hub skirting -1 to z[-8.2,-4.1], removing remnants across the new portal. No physical envelope or state changes.\n");
    }

    static void PhysicalMarking(TextMesh label)
    {
        var enamel=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Orientation/M_ReturnGalleryEnamel.mat");
        var rotation=Quaternion.LookRotation(-Forward);
        Box("Hub-face return plate",Front+Forward*.214f+Vector3.up*3.07f,new Vector3(1.60f,.23f,.026f),rotation,enamel,false);
        label.text="RETORNO · SOLO SALIDA";label.fontSize=72;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
        label.color=new Color(.07f,.09f,.066f);label.transform.rotation=Quaternion.identity;
        label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
        var renderer=label.GetComponent<Renderer>();var size=renderer.bounds.size;
        Require(size.x>.0001f&&size.y>.0001f,"Return label has no measurable bounds.");
        label.characterSize*=Mathf.Min(1.45f/size.x,.12f/size.y);
        label.transform.SetParent(visuals,true);label.transform.SetPositionAndRotation(Front+Forward*.229f+Vector3.up*3.07f,rotation);
        var shader=Shader.Find("EchoProtocol/PrintedText");
        var ink=new Material(shader){name="M_ArrivalInk",mainTexture=label.font.material.mainTexture};
        AssetDatabase.CreateAsset(ink,Art+"/M_ArrivalInk.mat");renderer.sharedMaterial=ink;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var owner=new SerializedObject(label.GetComponent<DiegeticText>());owner.FindProperty("depthTestedShader").objectReferenceValue=shader;owner.ApplyModifiedPropertiesWithoutUndo();
    }

    static Transform[] Fixture(float x,float z) => new[]{At("R3 clinical housing",new Vector3(x,3.1f,z)),
        At("R3 cool diffuser",new Vector3(x,3.03f,z)),At("R3 local cool fill",new Vector3(x,2.75f,z))};
    static void MoveFixture(Transform[] fixture,Vector3 p,float yaw)
    { foreach(var t in fixture){t.position=new Vector3(p.x,t.position.y,p.z);t.rotation=Quaternion.Euler(0,yaw,0)*t.rotation;} }
    static void ResizeZ(Transform t,float min,float max)
    {
        Require(Quaternion.Angle(t.rotation,Quaternion.identity)<.01f,"Unexpected axis-aligned strip: "+t.name);
        t.position=new Vector3(t.position.x,t.position.y,(min+max)*.5f);var size=t.localScale;size.z=max-min;t.localScale=size;
        var mapping=t.GetComponent<SurfaceMapping>();
        if(mapping)mapping.Configure(t.GetComponent<Renderer>(),size.y<Mathf.Min(size.x,size.z)
            ? new Vector2(size.x/3.2f,size.z/3.2f) : new Vector2(Mathf.Max(size.x,size.z)/3.2f,size.y/3.2f));
    }
    static Vector3[] Side(float halfWidth,float sign)
    {
        var result=new Vector3[Route.Length];
        for(int i=0;i<Route.Length;i++)
        {
            var before=(Route[Mathf.Max(i,1)]-Route[Mathf.Max(i-1,0)]).normalized;
            var after=(Route[Mathf.Min(i+1,Route.Length-1)]-Route[Mathf.Min(i,Route.Length-2)]).normalized;
            var n0=Vector3.Cross(before,Vector3.up);var n1=Vector3.Cross(after,Vector3.up);var m=(n0+n1).normalized;
            result[i]=Route[i]+m*(halfWidth*sign/Vector3.Dot(m,n1));
        }
        return result;
    }
    static void Corridor()
    {
        var left=Side(1.4f,1);var right=Side(1.4f,-1);
        for(int i=0;i<Route.Length-1;i++)
        { Wall("Approach left "+i,left[i],left[i+1],1);Wall("Approach right "+i,right[i],right[i+1],-1); }
        Wall("Hub north portal return",left[3],new Vector3(-5.1f,0,-8.2f),0);
        Wall("Hub southwest portal return",right[3],new Vector3(-3.75f,0,-12.1f),0);
        foreach(var door in new[]{Front-Forward*4.1f,Front})foreach(float side in new[]{-1f,1f})
            Box("Existing door aperture jamb",door+Vector3.Cross(Forward,Vector3.up)*(side*1.25f)+Vector3.up*1.45f,
                new Vector3(.10f,2.9f,.35f),Quaternion.LookRotation(Forward),trim,true);
        // Mitered segment prisms meet exactly. The final apron is clipped at x=-5,
        // where the existing Hub slabs begin, so there are no coplanar floor overlays.
        var l=Side(1.5f,1).ToList();var r=Side(1.5f,-1).ToList();
        l.Add(l.Last()+Forward*1.5f);r.Add(r.Last()+Forward*1.5f);
        var polys=new List<List<Vector3>>();
        for(int i=0;i<l.Count-1;i++)
        { var poly=ClipWest(new List<Vector3>{l[i],l[i+1],r[i+1],r[i]});if(poly.Count>=3)polys.Add(poly); }
        Slab("Approach floor",polys,-.2f,0,floor);Slab("Approach ceiling",polys,3.2f,3.4f,paint);
    }
    static void Wall(string name,Vector3 a,Vector3 b,int insideSide)
    {
        var delta=b-a;var q=Quaternion.LookRotation(delta);float length=delta.magnitude;
        Box(name,(a+b)*.5f+Vector3.up*1.6f,new Vector3(.2f,3.2f,length+.04f),q,paint,true);
        if(insideSide==0)return;
        // The former lower timber is reused as continuous architecture, no new signs.
        var inward=q*Vector3.right*insideSide;
        Box(name+" / lower timber",(a+b)*.5f+inward*.111f+Vector3.up*.47f,new Vector3(.02f,.94f,length),q,timber,false);
        Box(name+" / dado rail",(a+b)*.5f+inward*.126f+Vector3.up*.96f,new Vector3(.045f,.04f,length),q,trim,false);
    }
    static void Box(string name,Vector3 position,Vector3 size,Quaternion rotation,Material mat,bool collision)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(visuals,false);
        go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=size;Object.DestroyImmediate(go.GetComponent<Collider>());
        var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=mat;
        if(mat==paint||mat==timber)go.AddComponent<SurfaceMapping>().Configure(renderer,new Vector2(size.z/3.2f,size.y/3.2f));
        if(collision){var c=Group(name+" / solid",solids);c.SetPositionAndRotation(position,rotation);c.gameObject.AddComponent<BoxCollider>().size=size;}
    }
    static List<Vector3> ClipWest(List<Vector3> input)
    {
        var result=new List<Vector3>();var previous=input.Last();bool was=previous.x<=-5;
        foreach(var current in input)
        { bool now=current.x<=-5;if(now!=was)result.Add(Vector3.Lerp(previous,current,(-5-previous.x)/(current.x-previous.x)));
          if(now)result.Add(current);previous=current;was=now; }
        return result;
    }
    static void Slab(string name,List<List<Vector3>> polygons,float bottom,float top,Material mat)
    {
        var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
        Action<Vector3,Vector3,Vector3,Vector3> triangle=(a,b,c,n)=>{
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),n)<0){var swap=b;b=c;c=swap;}
            foreach(var p in new[]{a,b,c}){triangles.Add(vertices.Count);vertices.Add(p);uv.Add(new Vector2(p.x,p.z)/3.2f);} };
        foreach(var p in polygons)
        {
            var centre=p.Aggregate(Vector3.zero,(sum,v)=>sum+v)/p.Count;
            for(int i=1;i<p.Count-1;i++)
            { triangle(p[0]+Vector3.up*top,p[i]+Vector3.up*top,p[i+1]+Vector3.up*top,Vector3.up);
              triangle(p[0]+Vector3.up*bottom,p[i]+Vector3.up*bottom,p[i+1]+Vector3.up*bottom,Vector3.down); }
            for(int i=0;i<p.Count;i++)
            { var a=p[i];var b=p[(i+1)%p.Count];var normal=(a+b)*.5f-centre;
              triangle(a+Vector3.up*bottom,b+Vector3.up*bottom,b+Vector3.up*top,normal);
              triangle(a+Vector3.up*bottom,b+Vector3.up*top,a+Vector3.up*top,normal); }
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name.Replace(' ','_')+".asset");
        var visual=Group(name,visuals);visual.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;visual.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
        var solid=Group(name+" / solid",solids);solid.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
}

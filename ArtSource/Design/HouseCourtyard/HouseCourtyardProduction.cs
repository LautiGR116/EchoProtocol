// Staged Editor author. The integrator copies this only after preserving Observation.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class HouseCourtyardProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Courtyard.unity";
    public const string RootName="Courtyard / real house exterior";
    public const string FinalGroupName="Courtyard / relocated complete final";
    const string Art="Assets/_Game/Art/Courtyard";
    const string Report="Docs/VisualEvidence/2026-10-01-Courtyard/authoring.txt";
    static readonly Vector3 OldEntry=new Vector3(0,0,-74),NewEntry=new Vector3(-8.4f,0,-52.4f);
    static readonly Quaternion Turn=Quaternion.Euler(0,90,0);
    static Scene scene;static Transform visuals,solids;static Material plaster,ground,metal;
    static readonly List<Collider> newPartitions=new List<Collider>();
    static T[] All<T>() where T:Component=>Object.FindObjectsByType<T>(FindObjectsInactive.Include,
        FindObjectsSortMode.None).Where(t=>t&&t.gameObject.scene==scene).ToArray();
    static Transform Named(string name)=>All<Transform>().Single(t=>t.name==name);
    static Transform At(string name,Vector3 p)=>All<Transform>().Single(t=>t.name==name&&Vector3.Distance(t.position,p)<.005f);
    static T Ref<T>(Object owner,string field) where T:Object=>new SerializedObject(owner).FindProperty(field).objectReferenceValue as T;
    static void Ref(Object owner,string field,Object value)
    {var s=new SerializedObject(owner);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Require(bool good,string reason){if(!good)throw new InvalidOperationException(reason);}
    static bool Under(Transform t,IEnumerable<Transform> roots)=>roots.Any(r=>t==r||t.IsChildOf(r));
    static string Json(Object value)=>EditorJsonUtility.ToJson(value);
    static string V(Vector3 value)=>value.ToString("F5");
    public static Vector3 FromThresholdPoint(Vector3 point)=>NewEntry+Turn*(point-OldEntry);
    public static Quaternion FromThresholdRotation(Quaternion rotation)=>Turn*rotation;
    static Transform Group(string name,Transform parent=null)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
    static Object[] Refs(Object owner,string name)
    {var a=new SerializedObject(owner).FindProperty(name);return Enumerable.Range(0,a.arraySize).Select(i=>a.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();}
    static void Refs(Object owner,string name,IEnumerable<Object> values)
    {var s=new SerializedObject(owner);var a=s.FindProperty(name);var v=values.ToArray();a.arraySize=v.Length;for(int i=0;i<v.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=v[i];s.ApplyModifiedPropertiesWithoutUndo();}

    [MenuItem("Echo Protocol/Author real house courtyard")]
    public static void Author()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before courtyard authoring.");
        Require(!File.Exists(ScenePath)&&!Directory.Exists(Art)&&!File.Exists(Report),"Courtyard output exists; preserve instead of overwriting.");
        Require(Enumerable.Range(0,SceneManager.sceneCount).All(i=>!SceneManager.GetSceneAt(i).isDirty),"Save open scenes first.");
        scene=EditorSceneManager.OpenScene(ObservationProduction.ScenePath);
        var final=All<FinalDoorMemory>().Single();var house=All<HouseMemory>().Single();
        var director=All<CampaignDirector>().Single();var tone=All<CampaignRoomTone>().Single();
        var acoustics=All<HouseRoomAcoustics>().Single();var shadow=All<LightShadowBudget>().Single();
        Require(new SerializedObject(tone).FindProperty("finalReviewRegion")!=null,
            "Integrate the optional CampaignRoomTone.finalReviewRegion field before authoring.");
        Require(!Ref<BoxCollider>(tone,"finalReviewRegion"),"Unexpected pre-existing final ambience region.");
        var residence=Named("Residence / habitable reconstruction");
        var movingRoots=new[]{Named("Threshold / del otro lado"),Named("Porter / inhabited conservation station")};
        Require(movingRoots.All(t=>!t.parent&&Quaternion.Angle(t.rotation,Quaternion.identity)<.01f&&
            Vector3.Distance(t.lossyScale,Vector3.one)<.0001f),"Final roots must retain their original world basis.");
        Require(final.RequiresDirectEncounter&&Ref<HouseMemory>(final,"house")==house&&Ref<FinalDoorMemory>(tone,"finalDoor")==final,
            "Expected current direct Encounter and exact house/room-tone references.");
        var origin=Ref<Transform>(final,"origin");var witness=Ref<BoxCollider>(final,"insideWitnessZone");
        var outside=Ref<BoxCollider>(final,"outsideZone");var facing=Ref<Transform>(final,"outsideFacingTransform");
        var reviewDoor=Ref<HouseSwingDoor>(final,"domesticDoor");var exitDoor=Ref<SlidingDoor>(final,"exitDoor");
        Require(origin&&witness&&outside&&facing&&reviewDoor&&exitDoor&&
            Vector3.Distance(origin.position,new Vector3(0,.03f,-76))<.001f&&
            Vector3.Distance(witness.transform.position,new Vector3(0,1,-78.05f))<.001f&&
            Vector3.Distance(witness.size,new Vector3(1.6f,2.2f,.9f))<.001f&&
            Refs(director,"origins")[3]==origin,"Final origin/witness/Continue baseline changed.");
        AuditFinalReferences(final,movingRoots,reviewDoor,exitDoor);
        var serviceLight=Named("Service passage fixture").GetComponent<Light>();
        var serviceModel=At("Residence Fluorescent",new Vector3(0,2.6f,-68.5f));
        Require(serviceLight&&Vector3.Distance(serviceLight.transform.position,new Vector3(0,2.45f,-68.5f))<.001f&&
            serviceLight.type==LightType.Point&&serviceModel.parent==residence,"Service practical baseline changed.");
        var westFacade=At("Courtyard opposite facade",new Vector3(-9.4f,2.5f,-52));
        var retired=All<Transform>().Where(t=>t.parent==residence&&t.name.StartsWith("Service connection "))
            .Concat(new[]{westFacade}).ToArray();
        Require(retired.Length==9&&retired.All(t=>t.gameObject.activeSelf),"Expected eight connector pieces plus the western flat backdrop.");
        var residenceRegion=Ref<BoxCollider>(shadow,"enclosedResidence");
        Require(residenceRegion&&Vector3.Distance(residenceRegion.transform.position,new Vector3(0,1.5f,-55.75f))<.001f&&
            Vector3.Distance(residenceRegion.size,new Vector3(12,5,17))<.001f,"Residence shadow region baseline changed.");
        var oldAcoustics=Refs(acoustics,"architecturalPartitions").Cast<Collider>().ToArray();
        var oldResidenceLights=Refs(shadow,"residenceLights");var oldBudgetLights=Refs(shadow,"lights");
        var finalNearLights=new[]{Named("Inside final practical").GetComponent<Light>(),Named("Review instrument task light").GetComponent<Light>()};
        Require(finalNearLights.All(l=>l&&Under(l.transform,movingRoots))&&oldResidenceLights.Contains(serviceLight),
            "Expected final near lights and the already-budgeted service practical.");

        var transforms=All<Transform>();var poses=transforms.ToDictionary(t=>t,t=>(new Pose(t.position,t.rotation),t.lossyScale));
        var active=transforms.ToDictionary(t=>t,t=>t.gameObject.activeSelf);
        var moving=movingRoots.Concat(new[]{serviceLight.transform,serviceModel,residenceRegion.transform}).ToArray();
        var unchanged=All<MonoBehaviour>().Where(c=>c!=director&&c!=tone&&c!=acoustics&&c!=shadow).Cast<Object>()
            .Concat(All<Collider>().Where(c=>c!=residenceRegion)).Concat(All<Light>()).Concat(All<AudioSource>())
            .Concat(All<Rigidbody>()).Concat(All<Renderer>()).Distinct().ToDictionary(o=>o,Json);
        string oldDirector=Json(director),oldTone=Json(tone),oldAcousticJson=Json(acoustics),oldShadow=Json(shadow);
        string key=new SerializedObject(director).FindProperty("checkpointKey").stringValue;
        int audioCount=All<AudioSource>().Length,lightCount=All<Light>().Length;
        var report=new StringBuilder("Courtyard authoring only. No Play, camera, listening or human-experience verification.\n");
        Require(EditorSceneManager.SaveScene(scene,ScenePath),"Could not derive Courtyard.");
        AssetDatabase.CreateFolder("Assets/_Game/Art","Courtyard");
        var root=Group(RootName);visuals=Group("Visible courtyard",root);solids=Group("Physical courtyard",root);
        newPartitions.Clear();
        plaster=CopyMaterial("ExteriorPlaster","Assets/_Game/Art/Overhaul/M_Plaster.mat",new Color(.40f,.42f,.39f),.16f);
        ground=CopyMaterial("MineralGround","Assets/_Game/Art/Overhaul/M_Plaster.mat",new Color(.20f,.22f,.20f),.12f);
        metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Registration/M_OfficeSteel.mat");Require(metal,"Missing existing dull steel.");

        // Group while still in the old world basis; only then translate/rotate everything.
        var finalGroup=Group(FinalGroupName,root);finalGroup.position=OldEntry;
        foreach(var t in movingRoots)t.SetParent(finalGroup,true);
        var finalRegion=Group("Courtyard final ambience region",finalGroup).gameObject.AddComponent<BoxCollider>();
        finalRegion.isTrigger=true;finalRegion.center=new Vector3(.1f,1.35f,-8);finalRegion.size=new Vector3(9.4f,3.4f,16);
        finalGroup.SetPositionAndRotation(NewEntry,Turn);
        Ref(tone,"finalReviewRegion",finalRegion);
        foreach(var t in retired)t.gameObject.SetActive(false);
        serviceLight.transform.position=new Vector3(-7.15f,2.45f,-57.6f);
        serviceModel.position=new Vector3(-7.15f,2.6f,-57.6f);

        // Continuous L-shaped ground. Upper faces are exactly at inherited y=0.
        Box("Courtyard south ground",new Vector3(-3.7f,-.1f,-63.55f),new Vector3(9.4f,.2f,2.1f),ground,true);
        Box("Courtyard west ground",new Vector3(-6.9f,-.1f,-56.5f),new Vector3(3,.2f,12),ground,true);
        Wall("Courtyard south boundary",new Vector3(-3.7f,1.36f,-64.6f),new Vector3(9.4f,2.72f,.18f));
        Wall("Courtyard east return",new Vector3(1,1.36f,-63.55f),new Vector3(.18f,2.72f,2.1f));
        Wall("Courtyard west continuation",new Vector3(-8.4f,1.36f,-59.75f),new Vector3(.18f,2.72f,9.7f));
        Wall("Courtyard north boundary",new Vector3(-6.9f,1.36f,-50.5f),new Vector3(3,2.72f,.18f));
        Wall("Residence rear west enclosure",new Vector3(-5.4f,1.36f,-61.5f),new Vector3(.22f,2.72f,2));
        Wall("Residence rear facade",new Vector3(-3.2f,1.36f,-62.5f),new Vector3(4.4f,2.72f,.18f));
        // The inherited west wall has floral paper on its outside; clad only its real masses.
        foreach(var t in All<Transform>().Where(t=>t.parent==residence&&Mathf.Abs(t.position.x+5.4f)<.005f&&
            new[]{"Exterior wall","Window parapet","Window head","Rear exterior"}.Contains(t.name)))
        {
            var bounds=t.GetComponent<Renderer>().bounds;
            Box("Exterior plaster / "+t.name,new Vector3(bounds.min.x-.012f,bounds.center.y,bounds.center.z),
                new Vector3(.024f,bounds.size.y,bounds.size.z),plaster,false);
        }
        // Physical glazing uses the existing model for rendering; it never opens a route through the window.
        var glass=Group("Courtyard sealed parlor glass",solids);glass.position=new Vector3(-5.4f,1.66f,-52);
        var glassCollider=glass.gameObject.AddComponent<BoxCollider>();glassCollider.size=new Vector3(.035f,1.6f,1.8f);newPartitions.Add(glassCollider);
        // A small wall-fixed canopy gives the reused service fitting a credible support.
        Box("Courtyard service canopy",new Vector3(-7.15f,2.82f,-57.6f),new Vector3(2.5f,.12f,1.4f),metal,false);
        foreach(float z in new[]{-58.18f,-57.02f})
            Box("Courtyard canopy wall bracket",new Vector3(-8.29f,2.56f,z),new Vector3(.16f,.46f,.065f),metal,false);
        Box("Courtyard drain beside facade",new Vector3(-5.62f,.006f,-56.5f),new Vector3(.065f,.009f,11.6f),metal,false);

        // Outside cameras must not remove domestic shadows or allow the newly neighbouring final light to leak.
        residenceRegion.transform.position=new Vector3(-1.275f,1.5f,-56);residenceRegion.size=new Vector3(14.55f,5,17.5f);
        Refs(shadow,"residenceLights",oldResidenceLights.Concat(finalNearLights).Distinct());
        var keptPartitions=oldAcoustics.Where(c=>c&&!Under(c.transform,retired)).ToArray();
        Refs(acoustics,"architecturalPartitions",keptPartitions.Cast<Object>().Concat(newPartitions));
        var d=new SerializedObject(director);d.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Courtyard.v1";d.ApplyModifiedPropertiesWithoutUndo();
        Physics.SyncTransforms();

        Require(All<AudioSource>().Length==audioCount&&All<Light>().Length==lightCount,"Unexpected new audio/light source.");
        foreach(var pair in unchanged)Require(Json(pair.Key)==pair.Value,"Changed original component: "+pair.Key.name+" / "+pair.Key.GetType().Name);
        foreach(var t in transforms)
        {
            Require(t.gameObject.activeSelf==(retired.Contains(t)?false:active[t]),"Unexpected activeSelf: "+t.name);
            if(Under(t,movingRoots))Require(Vector3.Distance(t.position,FromThresholdPoint(poses[t].Item1.position))<.001f&&
                Quaternion.Angle(t.rotation,FromThresholdRotation(poses[t].Item1.rotation))<.02f&&
                Vector3.Distance(t.lossyScale,poses[t].Item2)<.001f,"Non-rigid final movement: "+t.name);
            else if(!Under(t,moving))Require(Vector3.Distance(t.position,poses[t].Item1.position)<.001f&&
                Quaternion.Angle(t.rotation,poses[t].Item1.rotation)<.02f&&Vector3.Distance(t.lossyScale,poses[t].Item2)<.001f,"Unrelated pose changed: "+t.name);
        }
        // Exact serialized comparisons with just each intentionally changed field temporarily restored.
        d.Update();d.FindProperty("checkpointKey").stringValue=key;d.ApplyModifiedPropertiesWithoutUndo();Require(Json(director)==oldDirector,"Director changed beyond key.");
        d.Update();d.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Courtyard.v1";d.ApplyModifiedPropertiesWithoutUndo();
        Ref(tone,"finalReviewRegion",null);Require(Json(tone)==oldTone,"RoomTone changed beyond final region.");Ref(tone,"finalReviewRegion",finalRegion);
        var newAcousticRefs=Refs(acoustics,"architecturalPartitions");Refs(acoustics,"architecturalPartitions",oldAcoustics);Require(Json(acoustics)==oldAcousticJson,"Acoustics changed beyond architectural refs.");Refs(acoustics,"architecturalPartitions",newAcousticRefs);
        var newShadowRefs=Refs(shadow,"residenceLights");Refs(shadow,"residenceLights",oldResidenceLights);Require(Json(shadow)==oldShadow,"Shadow budget changed beyond residence refs.");Refs(shadow,"residenceLights",newShadowRefs);
        Require(Refs(shadow,"lights").SequenceEqual(oldBudgetLights)&&Refs(director,"origins")[3]==origin,"Original light budget or final origin reference changed.");
        Require(new Bounds(finalRegion.center,finalRegion.size).Contains(finalRegion.transform.InverseTransformPoint(origin.position+Vector3.up)),"Final region omits origin.");
        Require(!new Bounds(finalRegion.center,finalRegion.size).Contains(finalRegion.transform.InverseTransformPoint(new Vector3(-6.85f,1,-52.4f))),"Final ambience region extends into courtyard.");
        report.AppendLine(ObservationProduction.ScenePath+" -> "+ScenePath+"; checkpoint="+"EP.CampaignCheckpoint.Courtyard.v1");
        report.AppendLine("Rigid map T(x,y,z)=(z+65.6,y,-52.4-x); yaw+90. Roots: "+string.Join(" | ",movingRoots.Select(t=>t.name)));
        report.AppendLine("Preserved "+unchanged.Count+" component serializations; "+transforms.Length+" original transforms audited. No gameplay thresholds, recordings, lights/audio gains, prefs or reset-target changes.");
        report.AppendLine("Final origin="+V(origin.position)+"; witness="+V(witness.transform.position)+"; outside="+V(outside.transform.position)+"; hinge="+V(reviewDoor.transform.position)+"; exit="+V(exitDoor.transform.position));
        report.AppendLine("Acoustic partitions "+oldAcoustics.Length+" -> "+newAcousticRefs.Length+"; new closed window is explicit. House sources/filter owners unchanged.");
        report.AppendLine("AudioSources "+audioCount+" -> "+All<AudioSource>().Length+"; lights "+lightCount+" -> "+All<Light>().Length+"; reused service fitting="+V(serviceLight.transform.position));
        report.AppendLine("Compact approach20.58m vs11.5m before portal; theoretical delta2.27s at4m/s, not measured player pace. New final region holds .20; return-key .015 remains runtime priority.");
        report.AppendLine("Pending real camera through parlor/hall, Hall-vs-other-light comparison, exterior shadows/collision, moved-final actual-input/Continue and subjective listening. LeaveHouse still stops unanswered phone at review; assess audibility honestly.");
        EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Could not save Courtyard.");AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory(Path.GetDirectoryName(Report));File.WriteAllText(Report,report.ToString());
        Debug.Log("Courtyard candidate authored; inspect actual exterior and final approach before selection. "+Report);
    }

    static void AuditFinalReferences(FinalDoorMemory final,Transform[] roots,HouseSwingDoor reviewDoor,SlidingDoor exit)
    {
        foreach(string field in new[]{"origin","outsideZone","outsideFacingTransform","insideWitnessZone","domesticDoor","terminalPilot",
            "recordingPilot","exitDoor","departureZone","keyInTray","audioSource"})
        {
            var value=new SerializedObject(final).FindProperty(field).objectReferenceValue;
            var t=value is GameObject go?go.transform:value is Component c?c.transform:null;
            Require(t&&Under(t,roots),"Spatial final reference outside rigid roots: "+field);
        }
        foreach(var c in Refs(final,"transparentSightColliders").Cast<Collider>())Require(c&&Under(c.transform,roots),"Final sight collider outside group.");
        foreach(string field in new[]{"hingeBody","solidLeaf","movementAudio","latchAudio"})
            Require(Under(Ref<Component>(reviewDoor,field).transform,roots),"Review hinge dependency outside roots: "+field);
        var motor=All<SlidingDoorAudio>().Single(a=>Ref<SlidingDoor>(a,"door")==exit);
        Require(Under(motor.transform,roots)&&Under(Ref<Rigidbody>(exit,"doorBody").transform,roots)&&
            Under(Ref<AudioSource>(motor,"mechanismSource").transform,roots),"Exit body/motor outside final roots.");
        foreach(var interaction in All<FinalDoorInteraction>().Where(i=>Ref<FinalDoorMemory>(i,"review")==final))
            Require(Under(interaction.transform,roots),"Final interaction outside moved roots: "+interaction.name);
        foreach(var display in All<FinalRecordDisplay>().Where(d=>Ref<FinalDoorMemory>(d,"review")==final))
            foreach(string field in new[]{"heading","detail","action"})Require(Under(Ref<TextMesh>(display,field).transform,roots),"Final display text outside roots.");
    }
    static Material CopyMaterial(string name,string source,Color color,float smoothness)
    {
        var original=AssetDatabase.LoadAssetAtPath<Material>(source);Require(original,"Missing original material "+source);
        var m=new Material(original){name="M_Courtyard"+name};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smoothness);
        AssetDatabase.CreateAsset(m,Art+"/M_"+name+".mat");return m;
    }
    static void Wall(string name,Vector3 p,Vector3 size)
    {
        Box(name,p,size,plaster,true);
        Box(name+" / coping",new Vector3(p.x,2.745f,p.z),new Vector3(size.x+.045f,.05f,size.z+.045f),metal,false);
    }
    static void Box(string name,Vector3 p,Vector3 size,Material material,bool collision)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetPositionAndRotation(p,Quaternion.identity);
        g.transform.localScale=size;g.transform.SetParent(visuals,true);Object.DestroyImmediate(g.GetComponent<Collider>());
        var renderer=g.GetComponent<Renderer>();renderer.sharedMaterial=material;
        if(material==plaster||material==ground)g.AddComponent<SurfaceMapping>().Configure(renderer,
            size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/3.2f,size.z/3.2f):new Vector2(Mathf.Max(size.x,size.z)/3.2f,size.y/3.2f));
        if(collision){var t=Group(name+" / solid",solids);t.position=p;var c=t.gameObject.AddComponent<BoxCollider>();c.size=size;newPartitions.Add(c);}
    }
}

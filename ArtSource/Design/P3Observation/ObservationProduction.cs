// Editor authoring candidate only. Derive Arrival; no runtime source modifications.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class ObservationProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Observation.unity";
    public const string RootName="Observation / paired observation gallery";
    public const float RecordingX=-5.05f, ServiceX=-7.8f;
    const string Art="Assets/_Game/Art/Observation";
    const string Report="Docs/VisualEvidence/2026-10-01-Observation/authoring.txt";
    const float DividerX=-6.3f;
    static Scene scene;
    static Transform visuals,solids;
    static Material paint,glass,frame,enamel,inset,ink;
    static T[] All<T>() where T:Component=>Object.FindObjectsByType<T>(FindObjectsInactive.Include,
        FindObjectsSortMode.None).Where(t=>t&&t.gameObject.scene==scene).ToArray();
    static Transform Named(string name)=>All<Transform>().Single(t=>t.name==name);
    static Transform At(string name,Vector3 p)=>All<Transform>().Single(t=>t.name==name&&Vector3.Distance(t.position,p)<.002f);
    static T Ref<T>(Object owner,string field) where T:Object=>new SerializedObject(owner).FindProperty(field).objectReferenceValue as T;
    static void Ref(Object owner,string field,Object value)
    {var s=new SerializedObject(owner);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Require(bool test,string why){if(!test)throw new InvalidOperationException(why);}
    static string Json(Object value)=>EditorJsonUtility.ToJson(value);
    static string V(Vector3 value)=>value.ToString("F5");
    static bool Under(Transform t,IEnumerable<Transform> roots)=>roots.Any(r=>r==t||t.IsChildOf(r));
    static Transform Group(string name,Transform parent)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
    static MonoBehaviour[] ResetTargets(LoopController loop)
    {var p=new SerializedObject(loop).FindProperty("resetTargets");return Enumerable.Range(0,p.arraySize).Select(i=>p.GetArrayElementAtIndex(i).objectReferenceValue as MonoBehaviour).ToArray();}

    [MenuItem("Echo Protocol/Author paired observation gallery")]
    public static void Author()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before Observation authoring.");
        Require(!File.Exists(ScenePath)&&!Directory.Exists(Art)&&!File.Exists(Report),"Observation already exists; preserve instead of overwriting.");
        Require(Enumerable.Range(0,SceneManager.sceneCount).All(i=>!SceneManager.GetSceneAt(i).isDirty),"Save open scenes before deriving Observation.");
        scene=EditorSceneManager.OpenScene(ArrivalProduction.ScenePath);
        var relay=All<RecombinationRelay>().Single();var r3=All<VS1HubReturnGate>().Single();
        var loop=Ref<LoopController>(relay,"loop");var director=All<CampaignDirector>().Single();
        var gallery=Named("VS1 R2 Return Gallery");var anchor=All<VS1RecallAnchor>().Single();
        var recall=Ref<Transform>(anchor,"recallPoint");
        var readers=new[]{Ref<OpticalReader>(relay,"readerA"),Ref<OpticalReader>(relay,"readerB")};
        var gates=new[]{Ref<SlidingDoor>(relay,"routeGate"),Ref<SlidingDoor>(relay,"finalGate")};
        var authorization=Ref<GameObject>(relay,"authorizationIndicator");
        Require(loop&&recall&&readers.All(r=>r)&&gates.All(g=>g)&&authorization&&
            Ref<RecombinationRelay>(r3,"puzzle")==relay&&Ref<SlidingDoor>(r3,"finalPuzzleGate")==gates[1]&&
            Ref<LoopController>(r3,"loop")==loop&&Ref<LoopController>(anchor,"loop")==loop,"P3/R1/R3 references disagree.");
        Require(Vector3.Distance(gallery.lossyScale,Vector3.one)<.0001f&&Quaternion.Angle(gallery.rotation,Quaternion.identity)<.01f,
            "P3 architecture must retain its unit world basis.");
        Require(Vector3.Distance(recall.position,new Vector3(-5.7f,.03f,27.8f))<.001f&&
            Vector3.Distance(r3.transform.position,new Vector3(-5.05f,0,43.7f))<.001f,"Recall/crossing baseline changed.");
        var oldReset=ResetTargets(loop);
        foreach(var c in readers.Cast<MonoBehaviour>().Concat(gates).Concat(new[]{relay}))
            Require(oldReset.Count(t=>t==c)==1,"Original P3 reset target absent/duplicated: "+c.name);
        var bodies=gates.Select(g=>Ref<Rigidbody>(g,"doorBody")).ToArray();
        var headers=gates.Select(g=>Named(g.name+" header")).ToArray();
        var indicators=readers.Select(r=>Ref<GameObject>(r,"activeIndicator")).ToArray();
        var heads=new Transform[2][];var labels=new TextMesh[2];
        for(int i=0;i<2;i++)
        {
            float z=i==0?35.2f:43;
            Require(Vector3.Distance(readers[i].transform.position,new Vector3(-7.8f,0,z))<.001f&&
                Quaternion.Angle(readers[i].transform.rotation,Quaternion.Euler(0,90,0))<.01f&&
                Vector3.Distance(gates[i].transform.position,new Vector3(-5.05f,0,z))<.001f&&
                bodies[i]&&bodies[i].isKinematic&&bodies[i].transform.parent==gates[i].transform&&
                Vector3.Distance(bodies[i].transform.localScale,new Vector3(2.3f,2.9f,.22f))<.001f,
                "Unexpected reader/gate/body baseline at "+z);
            Require(!All<SlidingDoorAudio>().Any(a=>Ref<SlidingDoor>(a,"door")==gates[i]),"P3 already has a motor owner.");
            heads[i]=HeadRoots(readers[i],z);labels[i]=Named("P3 "+(i==0?"A":"B")+" identifier").GetComponent<TextMesh>();
            Require(labels[i]&&labels[i].font&&labels[i].GetComponent<DiegeticText>(),"Missing original A/B lettering owner.");
        }
        var panes=new[]{Named("P3 A observation glass"),Named("P3 B observation glass")};
        var core=Named("P3 opaque separation");var terminal=Named("P3 recording lane terminal wall");
        Require(panes.All(p=>p.GetComponent<BoxCollider>()&&p.GetComponent<BoxCollider>().enabled)&&
            core.GetComponent<BoxCollider>().enabled&&Vector3.Distance(core.lossyScale,new Vector3(.2f,3.2f,5.3f))<.001f&&
            Vector3.Distance(terminal.position,new Vector3(-7.75f,1.6f,44.6f))<.001f,"P3 separation/terminal baseline changed.");
        var fixtureSets=new[]{Fixture(-7.8f,35.5f),Fixture(-7.8f,40.5f),Fixture(-5.05f,36.5f),Fixture(-5.05f,40.5f),Fixture(-5.05f,43.7f)};
        // Reuse the exact traceable R3 masters/bus/settings; keep its sources untouched.
        var donor=All<SlidingDoorAudio>().Single(a=>a.name=="R3 arrival rear physical motor");
        var donorSource=Ref<AudioSource>(donor,"mechanismSource");
        var movement=Ref<AudioClip>(donor,"movementLoopClip");var endpoint=Ref<AudioClip>(donor,"endpointClip");
        Require(donorSource&&donorSource.outputAudioMixerGroup&&movement&&endpoint&&
            AssetDatabase.GetAssetPath(movement)=="Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav"&&
            AssetDatabase.GetAssetPath(endpoint)=="Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav","R3 recorded masters/bus changed; inspect provenance before authoring.");
        paint=core.GetComponent<Renderer>().sharedMaterial;glass=panes[0].GetComponent<Renderer>().sharedMaterial;
        Require(paint&&glass&&Shader.Find("Universal Render Pipeline/Lit")&&Shader.Find("EchoProtocol/PrintedText"),"Missing inherited material/shader.");

        var movers=heads.SelectMany(h=>h).Concat(readers.Select(r=>r.transform)).Concat(gates.Select(g=>g.transform))
            .Concat(headers).Concat(fixtureSets.SelectMany(f=>f)).Concat(new[]{authorization.transform,r3.transform,terminal}).ToArray();
        var transforms=All<Transform>();var pose=transforms.ToDictionary(t=>t,t=>(new Pose(t.position,t.rotation),t.lossyScale));
        var active=transforms.ToDictionary(t=>t,t=>t.gameObject.activeSelf);
        var changedRenderers=panes.Select(t=>t.GetComponent<Renderer>()).Concat(new[]{core.GetComponent<Renderer>(),terminal.GetComponent<Renderer>()})
            .Concat(bodies.Select(b=>b.GetComponent<Renderer>())).Concat(headers.Select(h=>h.GetComponent<Renderer>())).Concat(labels.Select(l=>l.GetComponent<Renderer>())).ToArray();
        var unchanged=All<MonoBehaviour>().Where(c=>c!=loop&&c!=director&&!labels.Any(l=>l.transform==c.transform))
            .Cast<Object>().Concat(All<Collider>().Where(c=>c.transform!=core))
            .Concat(All<AudioSource>()).Concat(All<Light>()).Concat(All<Rigidbody>())
            .Concat(All<Renderer>().Where(r=>!changedRenderers.Contains(r))).Distinct().ToDictionary(o=>o,Json);
        string oldLoop=Json(loop),oldDirector=Json(director);int lights=All<Light>().Length,audio=All<AudioSource>().Length;
        string oldKey=new SerializedObject(director).FindProperty("checkpointKey").stringValue;
        var report=new StringBuilder("Observation authoring only; no Play, recording, camera, listening or human-play evidence.\n");
        report.AppendLine(ArrivalProduction.ScenePath+" -> "+ScenePath);
        Require(EditorSceneManager.SaveScene(scene,ScenePath),"Could not derive Observation.");
        AssetDatabase.CreateFolder("Assets/_Game/Art","Observation");
        Group(RootName,null); // Active marker survives title and derived scenes; no behavior on it.
        var owned=Group("Observation / P3 architecture and sound",gallery);
        visuals=Group("Visible architecture",owned);solids=Group("Physical architecture",owned);
        frame=Material("Frame",new Color(.075f,.095f,.088f),.18f,.32f);
        enamel=Material("DoorEnamel",new Color(.27f,.30f,.27f),.10f,.34f);
        inset=Material("DoorInset",new Color(.19f,.22f,.20f),.10f,.29f);
        ink=new Material(Shader.Find("EchoProtocol/PrintedText")){name="M_ObservationInk",mainTexture=labels[0].font.material.mainTexture};
        AssetDatabase.CreateAsset(ink,Art+"/M_ObservationInk.mat");
        var owners=new SlidingDoorAudio[2];
        for(int i=0;i<2;i++)
        {
            float z=i==0?35.2f:43;
            // Geometry turns around its actual mounting plane, while the accepted pose
            // translates to the centre of the opposite lane. No reflection/negative scale.
            var head=Group("Observation / reader "+(i==0?"A":"B")+" mounted assembly",gallery);
            head.position=new Vector3(DividerX,0,z);foreach(var t in heads[i])t.SetParent(head,true);head.rotation=Quaternion.Euler(0,180,0);
            readers[i].transform.SetPositionAndRotation(new Vector3(RecordingX,0,z),Quaternion.Euler(0,270,0));
            gates[i].transform.position+=Vector3.left*2.75f;headers[i].position+=Vector3.left*2.75f;
            headers[i].localScale=new Vector3(3.10f,.4f,.4f);headers[i].GetComponent<Renderer>().sharedMaterial=frame;
            AuthorDoor(gates[i],bodies[i],i);Window(i,z,panes[i]);PrintedIdentifier(labels[i],z);
            owners[i]=Motor(gates[i],bodies[i],owned,donor,donorSource,movement,endpoint);
        }
        authorization.transform.position=new Vector3(ServiceX,3.07f,35.2f-.24f);
        r3.transform.position+=Vector3.left*2.75f; // Only the real P3 crossing root, never the R3 arrival volumes.
        terminal.position=new Vector3(RecordingX,1.6f,44.6f);terminal.localScale=new Vector3(2.5f,3.2f,.2f);
        terminal.GetComponent<Renderer>().sharedMaterial=paint;
        core.GetComponent<Renderer>().enabled=false;core.GetComponent<BoxCollider>().enabled=false;
        Box("Closed technical core",new Vector3(DividerX,1.6f,39.65f),new Vector3(.55f,3.2f,5.3f),paint,true);
        // These are shallow closed service access panels, not interactable puzzles.
        foreach(float side in new[]{-1f,1f})
        {
            Box("Technical core / recessed access face",new Vector3(DividerX+side*.283f,1.65f,39.65f),new Vector3(.016f,2.35f,4.80f),enamel,false);
            Box("Technical core / lower plinth",new Vector3(DividerX+side*.291f,.16f,39.65f),new Vector3(.03f,.32f,5.30f),frame,false);
        }
        for(int i=0;i<fixtureSets.Length;i++)foreach(var t in fixtureSets[i])
            t.position=new Vector3(i<2?RecordingX:ServiceX,t.position.y,t.position.z);
        var loopConfig=new SerializedObject(loop);var targets=loopConfig.FindProperty("resetTargets");targets.arraySize=oldReset.Length+2;
        for(int i=0;i<2;i++)targets.GetArrayElementAtIndex(oldReset.Length+i).objectReferenceValue=owners[i];loopConfig.ApplyModifiedPropertiesWithoutUndo();
        var config=new SerializedObject(director);config.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Observation.v1";config.ApplyModifiedPropertiesWithoutUndo();
        Physics.SyncTransforms();

        Require(All<Light>().Length==lights&&All<AudioSource>().Length==audio+2,"Unexpected light/source count.");
        foreach(var pair in unchanged)Require(Json(pair.Key)==pair.Value,"Changed original contract: "+pair.Key.name+" / "+pair.Key.GetType().Name);
        foreach(var t in transforms)
        {
            Require(t.gameObject.activeSelf==active[t],"Original activeSelf changed: "+t.name);
            if(!Under(t,movers))Require(Vector3.Distance(t.position,pose[t].Item1.position)<.0001f&&
                Quaternion.Angle(t.rotation,pose[t].Item1.rotation)<.01f&&Vector3.Distance(t.lossyScale,pose[t].Item2)<.0001f,"Unrelated world pose changed: "+t.name);
        }
        Require(ResetTargets(loop).Take(oldReset.Length).SequenceEqual(oldReset)&&ResetTargets(loop).Skip(oldReset.Length).SequenceEqual(owners.Cast<MonoBehaviour>()),
            "Reset order changed; motor resets must follow original doors and relay.");
        // Compare complete serialized contracts with only their intentional new fields removed.
        loopConfig.Update();loopConfig.FindProperty("resetTargets").arraySize=oldReset.Length;loopConfig.ApplyModifiedPropertiesWithoutUndo();
        Require(Json(loop)==oldLoop,"Loop changed beyond the two appended motor reset owners.");
        loopConfig.Update();targets=loopConfig.FindProperty("resetTargets");targets.arraySize=oldReset.Length+2;
        for(int i=0;i<2;i++)targets.GetArrayElementAtIndex(oldReset.Length+i).objectReferenceValue=owners[i];loopConfig.ApplyModifiedPropertiesWithoutUndo();
        config.Update();config.FindProperty("checkpointKey").stringValue=oldKey;config.ApplyModifiedPropertiesWithoutUndo();Require(Json(director)==oldDirector,"Director changed beyond key.");
        config.Update();config.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Observation.v1";config.ApplyModifiedPropertiesWithoutUndo();
        foreach(var t in movers.Distinct())report.AppendLine(t.name+": "+V(pose[t].Item1.position)+" -> "+V(t.position)+"; yaw="+t.eulerAngles.y.ToString("F3"));
        report.AppendLine("R1 recall remains "+V(recall.position)+" / "+recall.rotation.ToString("F6")+"; original reader tolerance/qualification/mask and activeIndicator refs, relay, gate bodies/targets and R3 references preserved.");
        report.AppendLine("Preserved "+unchanged.Count+" original component serializations. Reset targets "+oldReset.Length+" -> "+ResetTargets(loop).Length+"; original order unchanged, motor A/B appended after doors/relay.");
        report.AppendLine("Lights "+lights+" -> "+All<Light>().Length+"; AudioSources "+audio+" -> "+All<AudioSource>().Length+". New fixed motors at "+V(owners[0].transform.position)+" / "+V(owners[1].transform.position));
        report.AppendLine("Masters: "+AssetDatabase.GetAssetPath(movement)+" / "+AssetDatabase.GetAssetPath(endpoint)+"; bus="+donorSource.outputAudioMixerGroup.name+"; volume="+new SerializedObject(donor).FindProperty("mechanismVolume").floatValue+". No new clip, voice, bed or Echo channel.");
        report.AppendLine("Terminal sealed east x[-6.30,-3.80] at z44.60; opaque core z[37,42.3], same continuous glass colliders at A/B. Physical core leaves2.125m east before shallow plinth (~2.094m local minimum),2.525m west. Door bodies2.3m; fixed jamb opening2.33m.");
        report.AppendLine("Namespace "+oldKey+" -> EP.CampaignCheckpoint.Observation.v1; no PlayerPrefs writes. Pending cameras and actual-input solve/Player-only preview race/R during movement; no perceptual approval.");
        EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Could not save Observation.");AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory(Path.GetDirectoryName(Report));File.WriteAllText(Report,report.ToString());
        RefineB();
        Debug.Log("Observation authored; inspect real entry/cooperation cameras before selection. "+Report);
    }


    [MenuItem("Echo Protocol/Refine observation B composition")]
    public static void RefineB()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before B composition.");
        scene=SceneManager.GetActiveScene();Require(scene.path==ScenePath,"Observation must be active.");
        var relay=All<RecombinationRelay>().Single();var b=Ref<OpticalReader>(relay,"readerB");var door=Ref<SlidingDoor>(relay,"finalGate");
        Require(Mathf.Abs(door.transform.position.z-43)<.001f,"B refinement already applied or unexpected baseline.");
        var header=Named(door.name+" header");var owner=All<SlidingDoorAudio>().Single(a=>Ref<SlidingDoor>(a,"door")==door);
        var roots=new List<Transform>{door.transform,header,owner.transform,Named("Observation / reader B mounted assembly"),Named("Observation B / instrument pier"),Named("Observation / mounted B")};
        foreach(string suffix in new[]{"west jamb","east jamb","fixed guide"})
            roots.AddRange(All<Transform>().Where(t=>t.name==door.name+" / "+suffix||t.name==door.name+" / "+suffix+" / solid"));
        Require(roots.Count==12,"Expected B leaf, header, motor, optical assembly, lettering plate, pier and six jamb/guide roots.");
        var light=Fixture(RecordingX,40.5f);string before=Json(b);var position=b.transform.position;var rotation=b.transform.rotation;
        foreach(var t in roots)t.position+=Vector3.forward*.35f;
        foreach(var t in light)t.position=new Vector3(t.position.x,t.position.y,43.15f);
        Require(Json(b)==before&&b.transform.position==position&&b.transform.rotation==rotation,"B input volume/rules changed.");
        Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Could not save B composition.");
        File.AppendAllText(Report,"\nCamera correction after first focused PASS41: B leaf/header/jambs/guide/motor and optical assembly/pier/label plate move +.35Z to43.35. B trigger/required yaw remain(-5.05,0,43)/270; R3 crossing stays43.7. Existing east fixture40.5 moves to43.15; no light/source addition, level or rule change. New camera/input evidence required.\n");
    }

    static Transform[] HeadRoots(OpticalReader reader,float z)
    {
        var roots=new List<Transform>{Ref<GameObject>(reader,"activeIndicator").transform,Named(reader.name+" housing"),Named(reader.name+" neutral lens"),
            At("Optical enamel body",new Vector3(-6.43f,1.55f,z)),At("Optical bezel",new Vector3(-6.532f,1.55f,z)),
            At("Optical neutral lens",new Vector3(-6.55f,1.55f,z)),Named("P3 "+(z<40?"A":"B")+" identifier")};
        roots.AddRange(All<Transform>().Where(t=>t.name=="Optical case fastener"&&Mathf.Abs(t.position.x+6.52f)<.002f&&Mathf.Abs(t.position.z-z)<.13f));
        Require(roots.Count==11&&roots.All(t=>!roots.Any(o=>o!=t&&t.IsChildOf(o))),"Expected ten optical parts plus original identifier for "+reader.name);
        return roots.ToArray();
    }
    static Transform[] Fixture(float x,float z)=>new[]{At("P3 panel housing",new Vector3(x,3.16f,z)),
        At("P3 stable diffuser",new Vector3(x,3.075f,z)),At("P3 panel-owned cool light",new Vector3(x,2.95f,z))};
    static Material Material(string name,Color color,float metallic,float smoothness)
    {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="M_Observation"+name};m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);AssetDatabase.CreateAsset(m,Art+"/M_"+name+".mat");return m;}
    static void Window(int i,float z,Transform original)
    {
        original.GetComponent<Renderer>().enabled=false; // Original full, sealed collider remains the physical partition.
        float first=i==0?34:42.3f,last=i==0?37:44.6f,mid=(first+last)*.5f,length=last-first;
        string n="Observation "+(i==0?"A":"B");
        Box(n+" / masonry base",new Vector3(DividerX,.39f,mid),new Vector3(.2f,.78f,length),paint,false);
        Box(n+" / fixed clear pane",new Vector3(DividerX,1.73f,mid),new Vector3(.035f,1.90f,length),glass,false);
        Box(n+" / lintel",new Vector3(DividerX,2.94f,mid),new Vector3(.22f,.52f,length),paint,false);
        Box(n+" / sealed sill",new Vector3(DividerX,.795f,mid),new Vector3(.25f,.055f,length),frame,false);
        foreach(float edge in new[]{first+.04f,last-.04f})
            Box(n+" / side jamb",new Vector3(DividerX,1.73f,edge),new Vector3(.22f,1.9f,.08f),frame,false);
        // A narrow opaque instrument support, leaving the oblique head/torso sightline
        // from the before-gate waiting position through the neighbouring clear pane.
        Box(n+" / instrument pier",new Vector3(DividerX,1.73f,z),new Vector3(.2f,1.9f,.46f),enamel,false);
    }
    static void PrintedIdentifier(TextMesh label,float z)
    {
        Box("Observation / mounted "+label.text.Trim(),new Vector3(-6.19f,2.1f,z),new Vector3(.024f,.27f,.32f),frame,false);
        label.transform.rotation=Quaternion.identity;label.fontSize=72;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
        label.color=new Color(.72f,.75f,.67f);label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
        ink.mainTexture=label.font.material.mainTexture;EditorUtility.SetDirty(ink);
        var r=label.GetComponent<Renderer>();var bounds=r.bounds.size;Require(bounds.x>.00001f&&bounds.y>.00001f,"Identifier glyph bounds missing.");
        label.characterSize*=Mathf.Min(.19f/bounds.x,.18f/bounds.y);
        label.transform.SetPositionAndRotation(new Vector3(-6.176f,2.1f,z),Quaternion.Euler(0,270,0));
        r.sharedMaterial=ink;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        Ref(label.GetComponent<DiegeticText>(),"depthTestedShader",ink.shader);
    }
    static void AuthorDoor(SlidingDoor door,Rigidbody body,int index)
    {
        float z=door.transform.position.z;
        var mesh=BeveledLeaf(new Vector3(2.3f,2.9f,.22f),.006f);mesh.name="Original observation leaf "+index;
        AssetDatabase.CreateAsset(mesh,Art+"/DoorLeaf"+index+".asset");body.GetComponent<MeshFilter>().sharedMesh=mesh;body.GetComponent<Renderer>().sharedMaterial=enamel;
        foreach(float side in new[]{-1f,1f})
        {
            // These visual panels inherit only the existing moving Rigidbody; no new collider/controller.
            Box(door.name+" / pressed inset",new Vector3(ServiceX,1.52f,z+side*.116f),new Vector3(2.02f,2.40f,.016f),inset,false,body.transform);
            Box(door.name+" / foot rail",new Vector3(ServiceX,.18f,z+side*.122f),new Vector3(2.22f,.20f,.02f),frame,false,body.transform);
        }
        Box(door.name+" / west jamb",new Vector3(-9.1325f,1.6f,z),new Vector3(.335f,3.2f,.44f),paint,true);
        Box(door.name+" / east jamb",new Vector3(-6.4175f,1.6f,z),new Vector3(.435f,3.2f,.44f),paint,true);
        foreach(float x in new[]{-8.974f,-6.626f})
            Box(door.name+" / fixed guide",new Vector3(x,1.48f,z),new Vector3(.045f,2.96f,.29f),frame,false);
    }
    static SlidingDoorAudio Motor(SlidingDoor door,Rigidbody body,Transform parent,SlidingDoorAudio donor,AudioSource donorSource,AudioClip movement,AudioClip endpoint)
    {
        var g=Group(door.name+" physical motor",parent);g.position=door.transform.position+Vector3.up*3.05f;
        var source=g.gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=1;source.dopplerLevel=0;
        source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=2;source.maxDistance=14;source.outputAudioMixerGroup=donorSource.outputAudioMixerGroup;
        source.ignoreListenerPause=false;source.volume=new SerializedObject(donor).FindProperty("mechanismVolume").floatValue;
        var owner=g.gameObject.AddComponent<SlidingDoorAudio>();var s=new SerializedObject(owner);
        s.FindProperty("door").objectReferenceValue=door;s.FindProperty("doorBody").objectReferenceValue=body;s.FindProperty("mechanismSource").objectReferenceValue=source;
        s.FindProperty("movementLoopClip").objectReferenceValue=movement;s.FindProperty("endpointClip").objectReferenceValue=endpoint;
        s.FindProperty("mechanismVolume").floatValue=source.volume;s.FindProperty("edgeFadeDuration").floatValue=new SerializedObject(donor).FindProperty("edgeFadeDuration").floatValue;
        s.ApplyModifiedPropertiesWithoutUndo();return owner;
    }
    static void Box(string name,Vector3 p,Vector3 size,Material material,bool collision,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=p;go.transform.localScale=size;
        go.transform.SetParent(parent?parent:visuals,true);Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;
        if(material==paint)go.AddComponent<SurfaceMapping>().Configure(go.GetComponent<Renderer>(),new Vector2(Mathf.Max(size.x,size.z)/3.2f,size.y/3.2f));
        if(collision){var solid=Group(name+" / solid",solids);solid.position=p;solid.gameObject.AddComponent<BoxCollider>().size=size;}
    }
    // Original bevelled box in the inherited unit-cube leaf mesh coordinates.
    // Metric bevel is baked per axis; the existing non-uniform Transform/collider stay intact.
    static Mesh BeveledLeaf(Vector3 size,float bevel)
    {
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
        {
            int u=(axis+1)%3,v=(axis+2)%3;
            float[] us={-.5f,-.5f+bevel/size[u],.5f-bevel/size[u],.5f};
            float[] vs={-.5f,-.5f+bevel/size[v],.5f-bevel/size[v],.5f};
            for(int a=0;a<3;a++)for(int b=0;b<3;b++)
            {
                int first=vertices.Count;
                for(int corner=0;corner<4;corner++)
                {
                    int ca=corner==1||corner==2?a+1:a,cb=corner>=2?b+1:b;
                    var p=Vector3.zero;p[axis]=sign*.5f;p[u]=us[ca];p[v]=vs[cb];var inner=p;
                    for(int k=0;k<3;k++)inner[k]=Mathf.Clamp(p[k],-.5f+bevel/size[k],.5f-bevel/size[k]);
                    var n=Vector3.Scale(p-inner,size).normalized;var position=inner;
                    for(int k=0;k<3;k++)position[k]+=n[k]*bevel/size[k];
                    vertices.Add(position);normals.Add(Vector3.Scale(n,size).normalized);uv.Add(new Vector2(us[ca]+.5f,vs[cb]+.5f));
                }
                if(sign>0)triangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
                else triangles.AddRange(new[]{first,first+2,first+1,first,first+3,first+2});
            }
        }
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
    }
}

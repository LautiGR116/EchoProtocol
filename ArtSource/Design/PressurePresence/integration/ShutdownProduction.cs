using System;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Scene-local composition candidate. Runtime isolation and its source owners remain unchanged.
public static class ShutdownProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Shutdown.unity";
    public const string RootName="Shutdown / machine and operator";
    const string Art="Assets/_Game/Art/Shutdown";
    public static void PrintPhysicalLabels()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)
            throw new Exception("Select Shutdown in Edit Mode");
        var owner=Object.FindFirstObjectByType<PressureIsolation>();
        var labels=owner.GetComponentsInChildren<TextMesh>(true);
        var path=Art+"/M_IsolationPrintedLabels.mat";
        var ink=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!ink){ink=new Material(Shader.Find("EchoProtocol/PrintedText")){name="M_IsolationPrintedLabels"};ink.mainTexture=labels.First().font.material.mainTexture;AssetDatabase.CreateAsset(ink,path);}
        foreach(var label in labels)
        {
            label.GetComponent<Renderer>().sharedMaterial=ink;
            var adapter=label.GetComponent<DiegeticText>();
            if(!adapter)throw new Exception("Physical label lost its atlas owner: "+label.name);
            var settings=new SerializedObject(adapter);settings.FindProperty("depthTestedShader").objectReferenceValue=ink.shader;settings.ApplyModifiedPropertiesWithoutUndo();
        }
        // The P2 plate inherited the same Awake material override from its template.
        var p2=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(t=>t.name=="P2 hold instruction");
        var p2Settings=new SerializedObject(p2.GetComponent<DiegeticText>());
        p2Settings.FindProperty("depthTestedShader").objectReferenceValue=ink.shader;p2Settings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(owner.gameObject.scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(owner.gameObject.scene);
        Debug.Log("Shutdown physical plate ink now follows actual incident light on "+labels.Length+" labels.");
    }
    public static void IntegrateRotor()
    {
        if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath ||
            Object.FindFirstObjectByType<PressureMotorRotor>())throw new Exception("Select the unconfigured Shutdown candidate in Edit Mode");
        var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var old=all.Single(t=>t.name=="Plant / MotorSkid"&&Mathf.Abs(t.position.z+36.5f)<.01f);
        var isolation=Object.FindFirstObjectByType<PressureIsolation>();
        string contract=EditorJsonUtility.ToJson(isolation);
        int colliders=Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/Shutdown/EP_SD_MotorSkid.fbx");
        if(!source)throw new Exception("Import the stable original ShutdownMotor export first");
        var g=(GameObject)PrefabUtility.InstantiatePrefab(source);
        g.name="Shutdown / operating motor A";g.transform.SetParent(old.parent);
        g.transform.SetPositionAndRotation(old.position,old.rotation);g.transform.localScale=old.localScale;
        foreach(var renderer in g.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>
            {
                var existing=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/PressurePlant/M_"+m.name+".mat");
                if(existing)return existing;
                bool matte=m.name=="SD_RotorMatte",patina=m.name=="SD_RotorPatina";
                if(!matte&&!patina)throw new Exception("Unknown original motor material: "+m.name);
                var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name="M_"+m.name;
                material.SetColor("_BaseColor",(matte?new Color(.078f,.09f,.08f):new Color(.054f,.067f,.058f)).gamma);
                material.SetFloat("_Smoothness",matte?.23f:.16f);material.SetFloat("_Metallic",matte?.14f:.08f);
                AssetDatabase.CreateAsset(material,Art+"/"+material.name+".mat");return material;
            }).ToArray();
        }
        var children=g.GetComponentsInChildren<Transform>();
        var rotor=children.Single(t=>t.name=="ShutdownRotor");
        var pivot=children.Single(t=>t.name=="MotorSkid__ROTOR_PIVOT");
        var axis=children.Single(t=>t.name=="MotorSkid__ROTOR_AXIS_POS");
        if(Vector3.Distance(rotor.position,new Vector3(6.208f,.535f,-36.5f))>.003f ||
            Vector3.Distance(rotor.position,pivot.position)>.001f ||
            Vector3.Angle(axis.position-pivot.position,Vector3.right)>.1f)
            throw new Exception("Imported rotor pivot/basis disagrees with the existing connected pump");
        var oldBounds=old.GetComponentInChildren<Renderer>().bounds;
        var bounds=g.GetComponentsInChildren<Renderer>().First().bounds;
        foreach(var renderer in g.GetComponentsInChildren<Renderer>())bounds.Encapsulate(renderer.bounds);
        if(Vector3.Distance(bounds.center,oldBounds.center)>.003f||Vector3.Distance(bounds.size,oldBounds.size)>.003f)
            throw new Exception("Replacement changes the existing pump envelope");
        var adapter=g.AddComponent<PressureMotorRotor>();var so=new SerializedObject(adapter);
        so.FindProperty("isolation").objectReferenceValue=isolation;
        so.FindProperty("session").objectReferenceValue=Object.FindFirstObjectByType<GameSession>();
        so.FindProperty("rotor").objectReferenceValue=rotor;
        so.FindProperty("localAxis").vector3Value=rotor.InverseTransformVector(axis.position-pivot.position).normalized;
        so.ApplyModifiedPropertiesWithoutUndo();
        old.gameObject.SetActive(false);
        // Aim the original housing and diffuser with their retained spot, not a floating beam.
        var enclosure=all.Single(t=>t.name=="Isolation emergency enclosure"&&Vector3.Distance(t.position,new Vector3(3.69f,1.08f,-36.15f))<.01f);
        var diffuser=all.Single(t=>t.name=="Isolation emergency diffuser"&&Vector3.Distance(t.position,new Vector3(3.749f,1.08f,-36.15f))<.01f);
        var lamp=all.Single(t=>t.name=="Isolation retained floor light"&&Vector3.Distance(t.position,new Vector3(3.8f,1.08f,-36.15f))<.01f);
        var aim=Quaternion.FromToRotation(Vector3.right,lamp.forward);
        enclosure.rotation=aim*enclosure.rotation;diffuser.position=enclosure.position+aim*Vector3.right*.059f;
        diffuser.rotation=aim*diffuser.rotation;lamp.position=enclosure.position+aim*Vector3.right*.11f;
        lamp.GetComponent<Light>().intensity=2.4f; // Camera-selected local machine light; no added source or global ambient change.
        if(EditorJsonUtility.ToJson(isolation)!=contract ||
            Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=colliders)
            throw new Exception("Visual integration changed isolation rules/references or collision components");
        EditorSceneManager.MarkSceneDirty(g.scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(g.scene);
        Debug.Log("Shutdown motor integrated: original connected envelope and colliders retained; one presentation-only rotor.");
    }
    [MenuItem("Echo Protocol/Author visible machine shutdown")]
    public static void Author()
    {
        if(EditorApplication.isPlaying||File.Exists(ScenePath)||Directory.Exists(Art))throw new Exception("Stop Play and preserve existing Shutdown before authoring again");
        var scene=EditorSceneManager.OpenScene(ReadersProduction.ScenePath);
        var isolation=Object.FindFirstObjectByType<PressureIsolation>();
        var transforms=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var panel=transforms.Single(t=>t.name=="Isolation original / IsolationPanel");
        var pedestal=transforms.Single(t=>t.name=="Isolation original / ControlPedestal");
        var cabinet=transforms.Single(t=>t.name=="Plant / ServiceCabinet");
        var baffle=transforms.Single(t=>t.name=="Central pressure baffle");
        var bodies=transforms.Where(t=>t.parent==isolation.transform && t.position.x>5f && t.position.z> -37f && t.position.z< -36f).ToArray();
        var configBefore=EditorJsonUtility.ToJson(isolation);
        if(bodies.Length<18||!bodies.Contains(panel)||!bodies.Contains(pedestal))throw new Exception("Incomplete control cluster");
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new Exception("Could not derive Shutdown");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var root=new GameObject(RootName).transform;
        // Keep the grip/collider hierarchy inside its existing owner. CanReachGrip
        // intentionally ignores only that owner's authored control colliders.
        var cluster=new GameObject("Shutdown / oblique isolation assembly").transform;cluster.SetParent(isolation.transform);
        cluster.position=new Vector3(5.9f,1.5f,-36.5f);cluster.rotation=Quaternion.identity;
        ShortenPedestal(pedestal);
        foreach(var t in bodies)t.SetParent(cluster,true);
        cluster.SetPositionAndRotation(new Vector3(5.75f,1.35f,-35.9f),Quaternion.Euler(0,15,0));
        if(configBefore!=EditorJsonUtility.ToJson(isolation))throw new Exception("Isolation serialized rule/reference changed");
        cabinet.position+=Vector3.right*2.75f;baffle.position+=Vector3.right*2.75f;
        // First reserve fixture becomes a cabinet-mounted machine light. Its
        // original Light reference and reserve power ownership are retained.
        var first=transforms.Where(t=>t.parent==isolation.transform && t.name.StartsWith("Isolation emergency") && Mathf.Abs(t.position.z+35.9f)<.01f).ToArray();
        var enclosure=first.Single(t=>t.name=="Isolation emergency enclosure");
        var diffuser=first.Single(t=>t.name=="Isolation emergency diffuser");
        var shoe=first.Single(t=>t.name=="Isolation emergency floor shoe");
        var post=first.Single(t=>t.name=="Isolation emergency pedestal");
        enclosure.position=new Vector3(3.69f,1.08f,-36.15f);diffuser.position=enclosure.position+Vector3.right*.059f;
        shoe.gameObject.SetActive(false);post.gameObject.SetActive(false);
        var light=transforms.Single(t=>t.name=="Isolation retained floor light"&&Mathf.Abs(t.position.z+35.9f)<.01f).GetComponent<Light>();
        light.transform.position=new Vector3(3.8f,1.08f,-36.15f);light.transform.LookAt(new Vector3(6.174f,.535f,-36.5f));
        light.type=LightType.Spot;light.spotAngle=90;light.innerSpotAngle=55;
        var bracket=GameObject.CreatePrimitive(PrimitiveType.Cube);bracket.name="Shutdown reserve cabinet bracket";bracket.transform.SetParent(root);bracket.transform.position=new Vector3(3.62f,1.08f,-36.15f);bracket.transform.localScale=new Vector3(.16f,.045f,.12f);bracket.GetComponent<Renderer>().sharedMaterial=enclosure.GetComponent<Renderer>().sharedMaterial;Object.DestroyImmediate(bracket.GetComponent<Collider>());
        MoveFloorFixture(transforms,isolation.transform,-40.3f,new Vector3(3.72f,.38f,-39.7f));
        MoveFloorFixture(transforms,isolation.transform,-43.9f,new Vector3(3.72f,.38f,-43.9f));
        var director=new SerializedObject(Object.FindFirstObjectByType<CampaignDirector>());director.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Shutdown.v1";director.ApplyModifiedPropertiesWithoutUndo();
        Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory("Docs/VisualEvidence/2026-10-01-Shutdown");
        File.WriteAllText("Docs/VisualEvidence/2026-10-01-Shutdown/authoring.txt","Layout candidate from Readers; "+bodies.Length+" original control-cluster roots grouped under PressureIsolation owner before transforming. Control anchor (5.75,1.35,-35.9), yaw+15. Pedestal middle shortens15cm; floor base retained. Cabinet and same collider move+2.75X; same pumps/receiver/pipes/source anchors retained. First reserve fixture becomes cabinet light; two others move to aisle. Rules/reference serialization of PressureIsolation unchanged. Camera/actual input verification pending. No rotor integrated yet.\n");
        Debug.Log("Shutdown composition authored. Camera, reach, floor/collision and actual isolation still require verification.");
    }
    static float LiftBeforeLowering(float y)=>.15f*(1-Mathf.InverseLerp(.30f,1.10f,y));
    static void ShortenPedestal(Transform pedestal)
    {
        foreach(var f in pedestal.GetComponentsInChildren<MeshFilter>())
        {
            var mesh=Object.Instantiate(f.sharedMesh);mesh.name=f.sharedMesh.name+" shortened for Shutdown";var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++){var p=f.transform.TransformPoint(vertices[i]);p.y+=LiftBeforeLowering(p.y);vertices[i]=f.transform.InverseTransformPoint(p);}
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Art+"/"+f.name+".asset");f.sharedMesh=mesh;
        }
        foreach(var t in pedestal.GetComponentsInChildren<Transform>().Where(t=>t.childCount==0&&t.name.StartsWith("ControlPedestal__")))
        {var p=t.position;p.y+=LiftBeforeLowering(p.y);t.position=p;}
    }
    static void MoveFloorFixture(Transform[] all,Transform owner,float oldZ,Vector3 enclosure)
    {
        var delta=enclosure-new Vector3(1,.38f,oldZ);
        foreach(var t in all.Where(t=>t.parent==owner&&Mathf.Abs(t.position.z-oldZ)<.01f&&
            (t.name.StartsWith("Isolation emergency")||t.name=="Isolation retained floor light")))t.position+=delta;
    }
}

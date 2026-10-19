using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Presentation;
using EchoProtocol.Campaign;
using Object=UnityEngine.Object;

// A new recoverable scene with concrete machinery; the tested controls and occlusion remain explicit.
public static class PressurePlantProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Plant.unity";
    const string Art="Assets/_Game/Art/PressurePlant";
    static Transform root;
    static readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
    static void M(string name,Color c,float smooth,float metallic)
    {string p=Art+"/M_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);mats[name]=m;}
    static GameObject Model(string name,Vector3 p,Quaternion q)
    // Unity's left-handed FBX import reflects source X. Restore the declared port basis before placement.
    {var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Models/PressurePlant/EP_PP_"+name+".fbx");if(!source)throw new Exception("Missing PressurePlant model "+name);var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Plant / "+name;g.transform.SetParent(root);var scale=g.transform.localScale;scale.x=-scale.x;g.transform.localScale=scale;g.transform.SetPositionAndRotation(p,q*g.transform.rotation);foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mats[m.name]).ToArray();return g;}
    static void Solid(string name,Vector3 p,Vector3 size)
    {var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;g.AddComponent<BoxCollider>().size=size;}
    static void BareTube(Vector3 a,Vector3 b)
    {float length=Vector3.Distance(a,b);if(length<.001f)return;var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Plant / connected straight barrel";g.transform.SetParent(root);g.transform.SetPositionAndRotation((a+b)*.5f,Quaternion.FromToRotation(Vector3.up,(b-a).normalized));g.transform.localScale=new Vector3(.18f,length*.5f,.18f);g.GetComponent<Renderer>().sharedMaterial=mats["PP_PipePaint"];Object.DestroyImmediate(g.GetComponent<Collider>());}
    static void Tube(Vector3 a,Vector3 b)
    {var direction=(b-a).normalized;float length=Vector3.Distance(a,b);var q=Quaternion.FromToRotation(Vector3.right,direction);while(length>=2.01f){Model("PipeStraight2m",a+direction,q);a+=direction*2;length-=2;}if(length>=.60f){Model("PipeAdaptor300",a+direction*.15f,q);Model("PipeAdaptor300",b-direction*.15f,q);BareTube(a+direction*.3f,b-direction*.3f);}else BareTube(a,b);}
    static Quaternion PipeBasis(Vector3 axis,Vector3 branch)=>Quaternion.LookRotation(-branch,Vector3.Cross(-branch,axis));
    static Vector3 Corner(Vector3 inlet,Vector3 incoming,Vector3 outgoing)
    {Model("PipeElbow90",inlet,PipeBasis(incoming,outgoing));return inlet+.5f*(incoming+outgoing);}
    static void Support(Vector3 p,Vector3 axis,Vector3 towardWall)
    {Model("PipeWallSupport",p,PipeBasis(axis,towardWall));}
    static void Label(string name,string words,Vector3 p,float yaw,float width)
    {var template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.characterSize=.018f;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.57f,.63f,.56f);float w=t.GetComponent<Renderer>().localBounds.size.x;if(w>width)t.characterSize*=width/w;}
    [MenuItem("Echo Protocol/Author connected pressure plant")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(ThresholdProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);
        root=new GameObject("Plant / connected pressure machinery").transform;mats.Clear();
        // Manifest values are scene-local materials; no Finish/Residence material is overwritten.
        M("PP_ServiceEnamel",new Color(0.285f,0.305f,0.271f).gamma,0.43f,0.12f);
        M("PP_ReplacementPanel",new Color(0.31f,0.321f,0.291f).gamma,0.37f,0.1f);
        M("PP_ReceiverPaint",new Color(0.15f,0.209f,0.219f).gamma,0.46f,0.13f);
        M("PP_MotorPaint",new Color(0.095f,0.137f,0.143f).gamma,0.49f,0.15f);
        M("PP_PipePaint",new Color(0.048f,0.061f,0.056f).gamma,0.44f,0.24f);
        M("PP_CastIron",new Color(0.078f,0.094f,0.086f).gamma,0.4f,0.7f);
        M("PP_MachinedSteel",new Color(0.25f,0.28f,0.26f).gamma,0.61f,0.8f);
        M("PP_FastenerZinc",new Color(0.39f,0.405f,0.37f).gamma,0.62f,0.82f);
        M("PP_Rubber",new Color(0.016f,0.021f,0.018f).gamma,0.13f,0f);
        M("PP_Recess",new Color(0.021f,0.027f,0.024f).gamma,0.11f,0f);
        M("PP_Oxidation",new Color(0.17f,0.079f,0.034f).gamma,0.13f,0.05f);
        M("PP_PlateIvory",new Color(0.49f,0.477f,0.373f).gamma,0.33f,0.05f);
        var baffle=GameObject.Find("Central pressure baffle");if(!baffle||!baffle.GetComponent<BoxCollider>())throw new Exception("Expected solid tested baffle");baffle.GetComponent<Renderer>().enabled=false;
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(t.name=="Baffle vent"||t.name=="Pressure intake"||t.name=="Vertical pressure riser"||t.name=="Campaign PressurePump")t.gameObject.SetActive(false);
        Model("ServiceCabinet",new Vector3(0,0,-39),Quaternion.identity);
        Model("Receiver",new Vector3(0,0,-34.9f),Quaternion.identity);
        Solid("Plant receiver simple collision",new Vector3(0,1.17f,-34.9f),new Vector3(1.30f,2.34f,1.35f));
        // Drain enters the actual slab beside the receiver rather than ending in open space.
        var drain=Corner(new Vector3(.76f,.84f,-34.9f),Vector3.right,Vector3.down);Tube(drain,new Vector3(drain.x,-.04f,drain.z));Solid("Plant receiver low side connection collision",new Vector3(1.0f,.50f,-34.9f),new Vector3(.85f,1,.40f));
        // Receiver outlet crosses above the circulation and feeds the eastern collector.
        var high=Corner(new Vector3(0,2.35f,-34.9f),Vector3.up,Vector3.right);
        var dropStart=new Vector3(6.8f,2.85f,-34.9f);Tube(high,dropStart);var down=Corner(dropStart,Vector3.right,Vector3.down);
        // Collector branches: one receiver connection and two actual pump discharges.
        var branches=new[]{-31f,-34.9f,-36.5f,-44f,-46.5f};float collectorHeight=1.85f;float collectorX=7.3f;
        Model("PipeTee",new Vector3(collectorX,collectorHeight,-34.9f),PipeBasis(Vector3.forward,Vector3.up));
        foreach(float z in new[]{-36.5f,-44f})
        {
            Model("PipeTee",new Vector3(collectorX,collectorHeight,z),PipeBasis(Vector3.forward,Vector3.down));
            Model("MotorSkid",new Vector3(6.89f,0,z),Quaternion.identity);Solid("Connected pump simple collision",new Vector3(6.89f,.43f,z),new Vector3(1.55f,.86f,.80f));
            Tube(new Vector3(7.3f,1.01f,z),new Vector3(7.3f,1.35f,z));
            // Horizontal suction flange ends inside the east wall penetration.
            Model("PipeAdaptor300",new Vector3(7.83f,.535f,z),Quaternion.identity);
        }
        for(int i=0;i<branches.Length-1;i++){float a=branches[i]-(i==0?0:.5f);float b=branches[i+1]+(i==branches.Length-2?0:.5f);Tube(new Vector3(collectorX,collectorHeight,a),new Vector3(collectorX,collectorHeight,b));}
        Model("PipeBlindCap",new Vector3(collectorX,collectorHeight,branches[0]),Quaternion.FromToRotation(Vector3.right,Vector3.forward));Model("PipeBlindCap",new Vector3(collectorX,collectorHeight,branches[branches.Length-1]),Quaternion.FromToRotation(Vector3.right,Vector3.back));
        // Physical brackets terminate on continuous stand-offs into the wall.
        for(float z=-32;z> -46;z-=3)
        {
            Support(new Vector3(7.3f,collectorHeight,z),Vector3.forward,Vector3.right);
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Plant collector wall stand-off";g.transform.SetParent(root);g.transform.position=new Vector3(7.725f,collectorHeight,z);g.transform.localScale=new Vector3(.30f,.18f,.18f);g.GetComponent<Renderer>().sharedMaterial=mats["PP_CastIron"];Object.DestroyImmediate(g.GetComponent<Collider>());
        }
        // A pair of hangers visibly supports the high receiver feed.
        foreach(float x in new[]{2.0f,4.8f})
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Receiver feed ceiling hanger";g.transform.SetParent(root);g.transform.position=new Vector3(x,3.02f,-34.9f);g.transform.localScale=new Vector3(.018f,.10f,.018f);g.GetComponent<Renderer>().sharedMaterial=mats["PP_MachinedSteel"];Object.DestroyImmediate(g.GetComponent<Collider>());}
        Label("Receiver maintenance identity","P—02",new Vector3(-.1f,1.35f,-34.165f),180,.35f);
        var guide=new GameObject("Return / illuminated subsuelo direction");guide.transform.SetParent(root);
        var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.name="Subsuelo destination plate";plate.transform.SetParent(guide.transform);plate.transform.position=new Vector3(1.55f,2.14f,-11.82f);plate.transform.localScale=new Vector3(1.45f,.30f,.035f);plate.GetComponent<Renderer>().sharedMaterial=mats["PP_Recess"];Object.DestroyImmediate(plate.GetComponent<Collider>());
        Label("Actual return destination","SUBSUELO  >",new Vector3(1.55f,2.14f,-11.79f),180,1.18f);var guideText=root.Find("Actual return destination");guideText.SetParent(guide.transform,true);guideText.GetComponent<TextMesh>().color=new Color(.60f,.82f,.69f);
        var arrival=new GameObject("Return / actual arrival zone");arrival.transform.SetParent(root);arrival.transform.position=new Vector3(-3.45f,1,-9.5f);var zone=arrival.AddComponent<BoxCollider>();zone.isTrigger=true;zone.size=new Vector3(2.4f,2.2f,2.3f);
        var campaignSO=new SerializedObject(Object.FindFirstObjectByType<CampaignDirector>());campaignSO.FindProperty("returnArrivalZone").objectReferenceValue=zone;campaignSO.FindProperty("returnDescentGuide").objectReferenceValue=guide;campaignSO.ApplyModifiedPropertiesWithoutUndo();guide.SetActive(false);
        var session=Object.FindFirstObjectByType<GameSession>();var so=new SerializedObject(session);so.FindProperty("editionLabel").stringValue="ARCHIVO 6B / DEL OTRO LADO";so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Connected pressure plant authored in new scene. Existing baffle collider and valve controls preserved; route and camera inspection required.");
    }
}

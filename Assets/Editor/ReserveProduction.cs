using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using Object=UnityEngine.Object;

public static class ReserveProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Reserve.unity";
    const string Art="Assets/_Game/Art/Reserve";
    const float Scale=1.15f;
    static Transform root;
    static Dictionary<string,Material> mats;
    static void Ref(Object o,string n,Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static Material Mat(string n,Color linear,float rough,float metal)
    {string p=Art+"/M_"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",linear.gamma);m.SetFloat("_Smoothness",1-rough);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;}
    static void Materials()
    {
        mats=new Dictionary<string,Material>{
            ["RU_Enamel"]=Mat("RU_Enamel",new Color(.14f,.18f,.145f),.54f,.25f),
            ["RU_Edge"]=Mat("RU_Edge",new Color(.075f,.095f,.070f),.50f,.34f),
            ["RU_Bakelite"]=Mat("RU_Bakelite",new Color(.027f,.025f,.019f),.42f,0),
            ["RU_Rubber"]=Mat("RU_Rubber",new Color(.013f,.016f,.014f),.80f,0),
            ["RU_Nickel"]=Mat("RU_Nickel",new Color(.27f,.28f,.23f),.41f,.76f),
            ["RU_Porcelain"]=Mat("RU_Porcelain",new Color(.59f,.55f,.42f),.41f,0),
            ["RU_Label"]=Mat("RU_Label",new Color(.46f,.43f,.32f),.80f,0),
            ["RU_Ink"]=Mat("RU_Ink",new Color(.045f,.055f,.039f),.76f,0),
            ["RU_Lens"]=Mat("RU_Lens",new Color(.32f,.13f,.018f),.32f,0),
            ["FIG_Skin"]=Mat("FIG_Skin",new Color(.37f,.255f,.182f),.65f,0),
            ["FIG_Jacket"]=Mat("FIG_Jacket",new Color(.11f,.15f,.14f),.86f,0),
            ["FIG_Stitch"]=Mat("FIG_Stitch",new Color(.060f,.078f,.070f),.89f,0),
            ["RG_Nail"]=Mat("RG_Nail",new Color(.40f,.29f,.225f),.52f,0)};
    }
    static GameObject Model(string type,Transform parent,Vector3 position,Quaternion placement)
    {
        string path="Assets/_Game/Art/Models/ReserveUnit/EP_RU_"+type+".fbx";var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing "+path);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name="Reserve original / "+type;g.transform.SetParent(parent);var s=g.transform.localScale;s*=Scale;s.x=-s.x;g.transform.localScale=s;g.transform.SetPositionAndRotation(position,placement*g.transform.rotation);
        foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mats[m.name]).ToArray();return g;
    }
    static Transform Anchor(GameObject g,string name)=>g.GetComponentsInChildren<Transform>(true).Single(t=>t.name==name);
    static TextMesh Label(string name,string words,Transform parent,Vector3 p,Quaternion rotation,float width,float size)
    {
        var template=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="Pressure contract");var t=Object.Instantiate(template,root);t.name=name;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,rotation);t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.07f,.09f,.066f);float actual=t.GetComponent<Renderer>().bounds.size.magnitude;if(actual>width)t.characterSize*=width/actual;t.transform.SetParent(parent,true);Ref(t.GetComponent<DiegeticText>(),"depthTestedShader",Shader.Find("EchoProtocol/PrintedText"));return t;
    }
    static void CourtyardBounce()
    {
        var budget=Object.FindFirstObjectByType<LightShadowBudget>();
        // Approximate the otherwise absent indirect patio light at the two open
        // room returns. Fixed world lights, with real shadows; no battery emitter.
        foreach(var p in new[]{new Vector3(-.83f,1.25f,-52.3f),new Vector3(.83f,1.25f,-53.3f)})
        {
            var g=new GameObject("Courtyard diffuse bounce at open return");g.transform.SetParent(root);g.transform.position=p;
            var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(.57f,.66f,.74f);light.intensity=.48f;light.range=4.5f;light.shadows=LightShadows.Soft;light.shadowBias=.012f;light.shadowNormalBias=.05f;light.shadowCustomResolution=256;
            var so=new SerializedObject(budget);foreach(var field in new[]{"lights","residenceLights"}){var a=so.FindProperty(field);a.arraySize++;a.GetArrayElementAtIndex(a.arraySize-1).objectReferenceValue=light;}so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    static void Dock(HouseMemory house,HouseInteractionKind kind,string title,string field)
    {
        var interaction=Object.FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(h=>h.Kind==kind);var original=interaction.transform;var oldPosition=original.position;var facing=-original.forward;
        var old=(GameObject)new SerializedObject(house).FindProperty(field).objectReferenceValue;old.SetActive(false);interaction.GetComponent<Renderer>().enabled=false;
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>(t.name=="Empty reserve cradle"||t.name=="Reserve socket label")&&Vector3.Distance(t.position,oldPosition)<.4f))t.gameObject.SetActive(false);
        // Seat exterior-room bases against the actual plaster, and keep the bedroom
        // unit clear of both the window curtain and the wardrobe.
        if(kind==HouseInteractionKind.KitchenSocket)original.position=new Vector3(-5.242f,oldPosition.y,oldPosition.z);
        if(kind==HouseInteractionKind.BedroomSocket)original.position=new Vector3(5.242f,oldPosition.y,-53.45f);
        var p=original.position;
        var collider=interaction.GetComponent<BoxCollider>();collider.center=new Vector3(0,0,-.4f);collider.size=new Vector3(1,1,2.1f);
        var dock=Model("ReserveDock",root,p+original.forward*.045f,Quaternion.LookRotation(facing));dock.name="Reserve base / "+title;
        var cell=Model("ReservePack",root,Anchor(dock,"ReserveDock__DOCKED_PACK_BOTTOM").position,Quaternion.LookRotation(facing));cell.name="Reserve unit / "+title;
        Label("Reserve room / "+title,title,dock.transform,Anchor(dock,"ReserveDock__LABEL_CENTRE").position+facing*.001f,original.rotation,.140f,.005f);
        Label("Reserve unit identity","RESERVA",cell.transform,Anchor(cell,"ReservePack__LABEL_CENTRE").position+facing*.001f,original.rotation,.079f,.0035f);
        Ref(house,field,cell);cell.SetActive(field=="hallFuse");
    }
    [MenuItem("Echo Protocol/Author portable house reserve")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");
        // Carry grip is integrated only after its source review; never hide a missing asset behind a placeholder.
        const string gripPath="Assets/_Game/Art/Models/ReserveGrip/EP_ReserveGrip.fbx";
        var gripAsset=AssetDatabase.LoadAssetAtPath<GameObject>(gripPath);if(!gripAsset)throw new Exception("Original carry grip must be reviewed and imported first");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(OffhookProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);root=new GameObject("Reserve / physical portable power").transform;Materials();
        var house=Object.FindFirstObjectByType<HouseMemory>();Dock(house,HouseInteractionKind.HallSocket,"RECIBIDOR","hallFuse");Dock(house,HouseInteractionKind.KitchenSocket,"COCINA","kitchenFuse");Dock(house,HouseInteractionKind.BedroomSocket,"DORMITORIO","bedroomFuse");CourtyardBounce();
        var player=Object.FindFirstObjectByType<FirstPersonController>();var camera=player.GetComponentInChildren<Camera>();var holder=new GameObject("Reserve carried by current Player");holder.transform.SetParent(camera.transform,false);holder.transform.localPosition=new Vector3(.26f,-.39f,.55f);holder.transform.localRotation=Quaternion.Euler(8,170,-8);
        // Corrected source front+Z faces back towards camera after the placement rotation.
        var cell=Model("ReservePack",holder.transform,holder.transform.position,holder.transform.rotation);
        var grip=(GameObject)PrefabUtility.InstantiatePrefab(gripAsset);grip.name="Reserve current Player grip";grip.transform.SetParent(holder.transform);var scale=grip.transform.localScale;scale*=Scale;scale.x=-scale.x;grip.transform.localScale=scale;grip.transform.SetPositionAndRotation(holder.transform.position,holder.transform.rotation*grip.transform.rotation);
        foreach(var r in grip.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mats[m.name]).ToArray();
        Label("Carried reserve identity","RESERVA",cell.transform,Anchor(cell,"ReservePack__LABEL_CENTRE").position+holder.transform.forward*.001f,holder.transform.rotation*Quaternion.Euler(0,180,0),.079f,.0035f);
        foreach(var r in holder.GetComponentsInChildren<Renderer>(true)){r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=true;r.gameObject.layer=LayerMask.NameToLayer("Ignore Raycast");}
        var presenter=holder.AddComponent<CarriedReservePresentation>();Ref(presenter,"view",camera);Ref(presenter,"player",player);Ref(presenter,"session",Object.FindFirstObjectByType<GameSession>());Ref(house,"carriedFuse",holder);holder.SetActive(false);
        var campaign=Object.FindFirstObjectByType<CampaignDirector>();Ref(campaign,"firstActLighting",Object.FindFirstObjectByType<EchoProtocol.Horror.VS1StrongEventOne>());var cs=new SerializedObject(campaign);cs.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Reserve.v1";cs.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Reserve authored: inspect real pickup/placement, empty bases, carrying at walls and continued ownership after departure before build.");
    }
}

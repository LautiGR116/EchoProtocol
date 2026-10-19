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

// Additive successor to Isolation, with the same gameplay and saved-state contract.
public static class PorterProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Porter.unity";
    const string Art="Assets/_Game/Art/Porter";
    static Transform root;static Material steel,paper,ink,wood;static readonly List<Light> lights=new List<Light>();
    static Transform Any(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
    static Material Load(string p)=>AssetDatabase.LoadAssetAtPath<Material>(p);
    static Material Mat(string n,Color c){var p=Art+"/M_"+n+".mat";var m=Load(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=c;m.SetFloat("_Smoothness",.25f);EditorUtility.SetDirty(m);return m;}
    static void Ref(Object o,string n,Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
    static GameObject Box(string n,Vector3 p,Vector3 size,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static GameObject Clone(string source,string name,Vector3 p,float rotationDelta)
    {var s=Any(source);var g=Object.Instantiate(s.gameObject,root);g.name=name;g.SetActive(true);g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,rotationDelta,0)*s.rotation);return g;}
    static TextMesh Text(string n,string words,Vector3 p,float yaw,float size,float maxWidth)
    {var t=Object.Instantiate(Any("Pressure contract").GetComponent<TextMesh>(),root);t.name=n;t.gameObject.SetActive(true);t.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));t.transform.localScale=Vector3.one;t.text=words;t.fontSize=72;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.53f,.68f,.58f);float width=t.GetComponent<Renderer>().localBounds.size.x;if(width>maxWidth)t.characterSize*=maxWidth/width;return t;}
    static Light Spot(string n,Vector3 p,Vector3 aim,Color c,float power,float range,float angle)
    {var g=new GameObject(n);g.transform.SetParent(root);g.transform.position=p;g.transform.LookAt(aim);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=c;l.intensity=power;l.range=range;l.spotAngle=angle;l.innerSpotAngle=angle*.65f;l.shadowBias=.015f;l.shadowNormalBias=.07f;lights.Add(l);return l;}
    static void Track(Renderer r,Light l,Color e){var t=r.gameObject.AddComponent<PracticalEmission>();Ref(t,"surface",r);Ref(t,"source",l);var s=new SerializedObject(t);s.FindProperty("litColor").colorValue=r.sharedMaterial.color;s.FindProperty("emission").colorValue=e;s.ApplyModifiedPropertiesWithoutUndo();}
    [MenuItem("Echo Protocol/Author inhabited conservation porter")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring");Directory.CreateDirectory(Art);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(IsolationProduction.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);lights.Clear();root=new GameObject("Porter / inhabited conservation station").transform;
        steel=Mat("DeskSteel",new Color(.085f,.11f,.09f));paper=Mat("LedgerPaper",new Color(.57f,.52f,.41f));ink=Mat("LedgerInk",new Color(.14f,.16f,.12f));wood=Load("Assets/_Game/Art/Threshold/M_FurnitureWalnut.mat");
        var review=Object.FindFirstObjectByType<FinalDoorMemory>();var interaction=Any("Original conservation terminal");interaction.GetComponent<Renderer>().enabled=false;interaction.position=new Vector3(2.16f,1.6f,-82.6f);interaction.localScale=new Vector3(.15f,.57f,.85f);
        Any("Conservation terminal engraved").gameObject.SetActive(false);
        Any("Physical instrument / DeskPreserveHousing").gameObject.SetActive(false);
        Clone("Recovery original monitor casing","Porter original conservation monitor",interaction.position,180);
        var screen=Mat("ConservationScreen",new Color(.012f,.027f,.022f));screen.SetColor("_EmissionColor",new Color(.009f,.028f,.019f));screen.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;screen.EnableKeyword("_EMISSION");EditorUtility.SetDirty(screen);
        Box("Porter actual record screen",new Vector3(2.153f,1.6f,-82.6f),new Vector3(.012f,.38f,.69f),screen);
        var heading=Text("Porter take status","SIN TOMA OBSERVADA",new Vector3(2.139f,1.713f,-82.6f),90,.0067f,.62f);
        var detail=Text("Porter take duration","REVISIÓN / 6B\nREGISTRO DE SESIÓN",new Vector3(2.139f,1.607f,-82.6f),90,.0049f,.61f);
        var action=Text("Porter take action","OBSERVÁ DESDE 6B",new Vector3(2.139f,1.476f,-82.6f),90,.0062f,.61f);
        var display=root.gameObject.AddComponent<FinalRecordDisplay>();Ref(display,"review",review);Ref(display,"heading",heading);Ref(display,"detail",detail);Ref(display,"action",action);
        var pilot=Any("Original verification pilot");pilot.position=new Vector3(2.138f,1.435f,-82.36f);pilot.localScale=new Vector3(.012f,.009f,.038f);
        Text("Porter conservation identity","CONSERVACIÓN / 6B",new Vector3(1.66f,.72f,-82),90,.012f,1.05f).color=new Color(.35f,.40f,.33f);
        // The work surface has space for a logbook and a lamp, separate from the screen.
        var lamp=Clone("Recovery original jointed lamp","Porter original articulated task lamp",new Vector3(2.35f,.90f,-81.30f),180);
        foreach(var prior in lamp.GetComponentsInChildren<PracticalEmission>(true))Object.DestroyImmediate(prior);
        var source=lamp.GetComponentsInChildren<Transform>().First(t=>t.name=="TaskLamp__LIGHT_SOURCE");var aim=lamp.GetComponentsInChildren<Transform>().First(t=>t.name=="TaskLamp__LIGHT_AIM_TIP");
        var pool=Spot("Porter desk task pool",source.position,aim.position,new Color(1,.85f,.66f),2.2f,2.7f,110);
        var diffuser=lamp.GetComponentsInChildren<Renderer>().First(r=>r.name=="TaskLightDiffuser");diffuser.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;Track(diffuser,pool,new Color(.66f,.50f,.29f));
        Box("Porter open ledger cover",new Vector3(2.15f,.909f,-81.85f),new Vector3(.37f,.018f,.52f),steel);
        Box("Porter ledger pages",new Vector3(2.15f,.922f,-81.85f),new Vector3(.35f,.008f,.49f),paper);
        Box("Porter ledger spine",new Vector3(2.15f,.928f,-81.85f),new Vector3(.012f,.004f,.49f),ink);
        for(int i=0;i<7;i++)Box("Porter ruled entry",new Vector3(2.15f,.928f,-82.04f+i*.054f),new Vector3(.30f,.001f,.0015f),ink);
        Box("Porter pen",new Vector3(2.19f,.938f,-81.93f),new Vector3(.013f,.008f,.15f),steel);
        Clone("Recovery clerk chair","Porter vacated clerk chair",new Vector3(3.30f,0,-82.35f),-30);
        Clone("Recovery individual filing cabinet","Porter daily archive",new Vector3(4.22f,0,-80.13f),90);
        Box("Porter rear painted dado",new Vector3(4.592f,.5f,-81.50f),new Vector3(.025f,.99f,4.7f),Load("Assets/_Game/Art/Registration/M_OfficePaint.mat"));
        Box("Porter rear dado rail",new Vector3(4.565f,1.015f,-81.50f),new Vector3(.065f,.04f,4.7f),wood);
        Text("Porter archive identity","SESIONES / ARCHIVO",new Vector3(4.46f,2.40f,-81.85f),90,.011f,1.22f);
        // A practical rear pool makes the recess a room instead of a terminal in a void.
        Any("Porter desk pool").GetComponent<Light>().enabled=false;
        var ceiling=new Vector3(3.15f,2.67f,-81.25f);var opal=Mat("CeilingOpal",new Color(.48f,.50f,.42f));opal.SetColor("_EmissionColor",new Color(.23f,.24f,.18f));opal.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;opal.EnableKeyword("_EMISSION");EditorUtility.SetDirty(opal);
        Box("Porter ceiling tray",ceiling+Vector3.up*.04f,new Vector3(.30f,.08f,1.25f),steel);
        var face=Box("Porter ceiling opal face",ceiling,new Vector3(.24f,.017f,1.17f),opal).GetComponent<Renderer>();
        var room=Spot("Porter workroom ceiling pool",ceiling-Vector3.up*.04f,new Vector3(3.35f,.15f,-81.5f),new Color(.83f,.86f,.75f),2.8f,4.1f,110);Track(face,room,new Color(.23f,.24f,.18f));
        var screenLight=Spot("Porter monitor case reflection",new Vector3(1.8f,2.45f,-82.35f),new Vector3(2.16f,1.32f,-82.6f),new Color(1,.86f,.69f),.75f,2.2f,70);
        Box("Porter monitor task housing",screenLight.transform.position+Vector3.up*.025f,new Vector3(.18f,.05f,.18f),steel);
        var budget=new SerializedObject(Object.FindFirstObjectByType<LightShadowBudget>());var array=budget.FindProperty("lights");int first=array.arraySize;array.arraySize+=lights.Count;for(int i=0;i<lights.Count;i++)array.GetArrayElementAtIndex(first+i).objectReferenceValue=lights[i];budget.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Debug.Log("Porter authored. Verify actual conservation ray, complete metadata states and camera composition before build.");
    }
}

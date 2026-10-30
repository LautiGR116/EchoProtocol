using System;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Authored markings describe real routes. They never authorize a reader, move
// an actor, block a ray, or alter the recorded take. Explicit current-scene tool.
public static class OneShotSpatialIntegration
{
    const string RootName = "OneShot / paired routes and maintained escape";
    const string Folder = "Assets/_Game/Materials/OneShot";
    const string UndoName = "Compose paired routes and pressure room";

    [MenuItem("Echo Protocol/One shot/Apply spatial composition to open Incursion")]
    public static void ApplyToOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Finish compilation and exit Play first.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity")
            throw new InvalidOperationException("Requires the already-open Incursion scene; never opens or saves it.");
        if (scene.GetRootGameObjects().Any(x => x.name == RootName))
        { Debug.Log("One-shot spatial composition already installed. No changes."); return; }
        var threat = All<MachineCoverThreat>(scene).Single();
        var template = All<TextMesh>(scene).Single(x => x.name == "P1 / suspended A arrival flag");
        var allLights = All<Light>(scene).ToArray();
        var bib = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .SingleOrDefault(x => x.name == "Embestida / torn rubber bib");
        var depthShader = new SerializedObject(template.GetComponent<DiegeticText>())
            .FindProperty("depthTestedShader").objectReferenceValue as Shader;
        if (!template.font || !depthShader) throw new InvalidOperationException("Existing Folio world font/shader required.");
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new InvalidOperationException("URP Lit shader required.");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Game/Materials", "OneShot");
        var echo = Paint("Recorded_route", new Color(.63f, .79f, .73f), shader);
        var present = Paint("Present_route", new Color(.91f, .88f, .79f), shader);
        var dark = Paint("Sign_backing", new Color(.045f, .065f, .061f), shader);

        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(UndoName);
        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, UndoName);
        try
        {
            // Double dashed recorded route; single continuous present route.
            // Physical lane geometry, width and all reader volumes stay intact.
            for (float z = -2.5f; z < 11f; z += 1.25f)
            {
                Stripe(root.transform, "Recorded route / left", new Vector3(-3.58f,.012f,z), new Vector3(-3.58f,.012f,Mathf.Min(z+.72f,11)), .045f, echo);
                Stripe(root.transform, "Recorded route / right", new Vector3(-3.45f,.012f,z), new Vector3(-3.45f,.012f,Mathf.Min(z+.72f,11)), .045f, echo);
            }
            Stripe(root.transform, "Present route", new Vector3(2.48f,.012f,-2.5f), new Vector3(2.48f,.012f,10), .065f, present);
            Station(root.transform, "A / record authorization", new Vector3(-2.75f, .014f, 3), echo, false);
            Station(root.transform, "B / hold recorded presence", new Vector3(-2.75f, .014f, 11), echo, true);
            Station(root.transform, "C / hold present presence", new Vector3(3.35f, .014f, 10), present, true);
            Arrow(root.transform, new Vector3(-3.51f,.016f,8.7f), Vector3.forward, echo);
            Arrow(root.transform, new Vector3(2.48f,.016f,7.7f), Vector3.forward, present);
            Flag(root.transform, "B / recorded lane flag", "B", "SOSTENER", new Vector3(-2.75f,2.17f,9.25f), echo, dark, template, depthShader);
            Flag(root.transform, "C / present lane flag", "C", "COOPERAR", new Vector3(3.35f,2.17f,8.25f), present, dark, template, depthShader);

            // The lateral exit has a real dogleg around the existing blind wing.
            // Paint follows that clearance instead of inviting a diagonal into it.
            var route = new[] {new Vector3(-2.30f,.014f,-43.55f),new Vector3(-1.40f,.014f,-41.35f),
                new Vector3(0,.014f,-41.35f),new Vector3(0,.014f,-43.60f),new Vector3(3.30f,.014f,-43.60f),
                new Vector3(3.30f,.014f,-46.85f),new Vector3(0,.014f,-46.85f),new Vector3(0,.014f,-48.60f)};
            for(int i=0;i<route.Length-1;i++)
            {
                var d=route[i+1]-route[i];
                Arrow(root.transform,route[i]+d*.62f,d.normalized,present);
            }

            // Use existing physical practicals; no extra fill lights, post FX,
            // flash or increased sound level. Warm work, cool machinery, mint exit.
            foreach (var lamp in allLights)
            {
                var n=lamp.name;
                if(n == "Pump / reflected control light" || n == "Pressure / maintained cabinet reflection")
                { Undo.RecordObject(lamp,UndoName); lamp.color=new Color(.54f,.76f,.79f); }
                else if(n == "Service / maintained work light")
                { Undo.RecordObject(lamp,UndoName); lamp.color=new Color(.94f,.70f,.43f); lamp.intensity=1.55f; }
                else if(n == "Service / last practical")
                { Undo.RecordObject(lamp,UndoName); lamp.color=new Color(1f,.68f,.36f); }
                else if(n.StartsWith("Return / maintained") || n == "Return / immediate escape light")
                { Undo.RecordObject(lamp,UndoName); lamp.color=new Color(.58f,.82f,.72f); }
            }
            if(bib)
            {
                // Keep the private mesh and all rig data recoverable. The added
                // corrugated apron obscures the scanned torso at close range.
                Undo.RecordObject(bib.gameObject,UndoName); bib.gameObject.SetActive(false);
            }
            EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group);
            Debug.Log("Paired routes, A/B/C spatial marks, physical escape arrows and pressure-room light separation installed. Deformed apron hidden; private source retained. Scene UNSAVED.",root);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Material Paint(string name,Color color,Shader shader)
    {
        string path=Folder+"/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material) return material;
        material=new Material(shader){name=name}; material.SetColor("_BaseColor",color);
        material.SetFloat("_Smoothness",.18f); material.SetFloat("_Metallic",0);
        AssetDatabase.CreateAsset(material,path); return material;
    }
    static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material material)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=name;
        o.transform.SetParent(parent,false); o.transform.position=position; o.transform.localScale=size;
        Object.DestroyImmediate(o.GetComponent<Collider>());
        var r=o.GetComponent<MeshRenderer>(); r.sharedMaterial=material; r.shadowCastingMode=ShadowCastingMode.Off;
        return o;
    }
    static void Stripe(Transform root,string name,Vector3 a,Vector3 b,float width,Material material)
    {
        var o=Box(root,name,(a+b)*.5f,new Vector3(width,.004f,Vector3.Distance(a,b)),material);
        o.transform.rotation=Quaternion.LookRotation(b-a,Vector3.up);
    }
    static void Arrow(Transform root,Vector3 at,Vector3 direction,Material material)
    {
        var side=Vector3.Cross(Vector3.up,direction)*.19f;
        Stripe(root,"Maintained route / arrow",at-direction*.25f+side,at,.055f,material);
        Stripe(root,"Maintained route / arrow",at-direction*.25f-side,at,.055f,material);
    }
    static void Station(Transform root,string name,Vector3 at,Material material,bool hold)
    {
        // Latched A is a closed frame. B/C are open brackets: remain in place.
        Stripe(root,name,at+new Vector3(-.6f,0,-.45f),at+new Vector3(-.6f,0,.45f),.045f,material);
        Stripe(root,name,at+new Vector3(.6f,0,-.45f),at+new Vector3(.6f,0,.45f),.045f,material);
        foreach(float z in new[]{-.45f,.45f})
            if(!hold) Stripe(root,name,at+new Vector3(-.6f,0,z),at+new Vector3(.6f,0,z),.045f,material);
            else foreach(float x in new[]{-.6f,.35f})
                Stripe(root,name,at+new Vector3(x,0,z),at+new Vector3(x+.25f,0,z),.045f,material);
    }
    static void Flag(Transform root,string name,string letter,string caption,Vector3 position,Material accent,Material backing,TextMesh template,Shader shader)
    {
        var plate=Box(root,name,position,new Vector3(.78f,.46f,.035f),backing);
        Box(root,name+" / identity",position+new Vector3(-.36f,0,-.023f),new Vector3(.035f,.42f,.008f),accent);
        Box(root,name+" / ceiling stem",position+new Vector3(0,.49f,0),new Vector3(.02f,.52f,.02f),backing);
        var o=new GameObject(name+" / text");o.transform.SetParent(root,false);o.transform.position=position+new Vector3(0,.06f,-.025f);
        var label=o.AddComponent<TextMesh>();label.font=template.font;label.fontSize=64;label.characterSize=.016f;
        label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=letter;label.color=accent.GetColor("_BaseColor");
        o.GetComponent<MeshRenderer>().sharedMaterial=template.GetComponent<MeshRenderer>().sharedMaterial;
        var dt=o.AddComponent<DiegeticText>();var so=new SerializedObject(dt);so.FindProperty("depthTestedShader").objectReferenceValue=shader;so.ApplyModifiedPropertiesWithoutUndo();
        var captionObject=Object.Instantiate(o,root);captionObject.name=name+" / instruction";
        captionObject.transform.position=position+new Vector3(0,-.14f,-.025f);
        var small=captionObject.GetComponent<TextMesh>();small.characterSize=.0033f;small.text=caption;small.color=new Color(.91f,.88f,.79f);
    }
    static System.Collections.Generic.IEnumerable<T> All<T>(Scene scene) where T:Component
        =>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
}

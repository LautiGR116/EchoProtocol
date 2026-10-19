using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Editor authoring candidate only. Integrator imports/compiles this file before use.
public static class OrientationProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Orientation.unity";
    public const string RootName = "Orientation / R1 workstation placement";
    const string Art = "Assets/_Game/Art/Orientation";
    const string ReportPath = "Docs/VisualEvidence/2026-10-01-Orientation/authoring.txt";
    static Scene scene;
    static T[] All<T>() where T : Component => Object.FindObjectsByType<T>(
        FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.gameObject.scene == scene).ToArray();
    static Transform Named(string name) => All<Transform>().Single(t => t.name == name);
    static Transform At(string name, Vector3 p) => All<Transform>().Single(t => t.name == name && Vector3.Distance(t.position, p) < .01f);
    static T Ref<T>(Object owner, string name) where T : Object =>
        new SerializedObject(owner).FindProperty(name).objectReferenceValue as T;
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static string Json(Object value) => EditorJsonUtility.ToJson(value);
    static string V(Vector3 value) => value.ToString("F6");
    static bool SamePose(Transform t, Pose p) => Vector3.Distance(t.position, p.position) < .00001f && Quaternion.Angle(t.rotation, p.rotation) < .01f;
    static MonoBehaviour InteractionOwner(Collider c) => c.GetComponentsInParent<MonoBehaviour>(true).FirstOrDefault(x => x is IInteractable);

    [MenuItem("Echo Protocol/Author R1 orientation composition")]
    public static void Author()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play before authoring Orientation.");
        Require(!File.Exists(ScenePath) && !Directory.Exists(Art) && !File.Exists(ReportPath),
            "Orientation output already exists; preserve/review it instead of overwriting.");
        Require(Enumerable.Range(0, SceneManager.sceneCount).All(i => !SceneManager.GetSceneAt(i).isDirty),
            "Save the currently open scenes before deriving Orientation.");
        scene = EditorSceneManager.OpenScene(ShutdownProduction.ScenePath);
        var room = Named("R1 Clinical Observation Hub");
        var station = Named("R1 Central observation station");
        var anchor = station.GetComponent<VS1RecallAnchor>();
        Require(anchor, "Existing R1 anchor is missing.");
        var recall = Ref<Transform>(anchor, "recallPoint");
        var gate = Ref<VS1RecoveryHubGate>(anchor, "recoveryHub");
        var monitor = Named("R1 Monitor housing").GetComponent<BoxCollider>();
        var budget = All<LightShadowBudget>().Single();
        var director = All<CampaignDirector>().Single();
        Require(recall && gate && monitor && Ref<GameObject>(gate, "hubRoot") == room.gameObject,
            "R1 anchor/gate/collider references disagree with the documented room.");
        Require(Vector3.Distance(room.lossyScale,Vector3.one) < .0001f,
            "The workstation's room parent must retain unit world scale.");
        var activeDisplay = Ref<GameObject>(anchor,"activeDisplay");
        var stationText = Ref<TextMesh>(anchor,"statusLabel");
        Require(activeDisplay && stationText && activeDisplay.name == "R2 stable anchor confirmation"
            && stationText.name == "R2 station instruction" && activeDisplay.transform.IsChildOf(station)
            && stationText.transform.IsChildOf(station) && Named("R1 Neutral inactive display").IsChildOf(station)
            && Named("R1 Monitor stem").IsChildOf(station), "Display/text/stem no longer travel with the station root.");
        Require(recall.name == "R2 R1 recall point" && recall.IsChildOf(station)
            && Vector3.Distance(recall.position, new Vector3(-5.7f,.03f,27.8f)) < .001f
            && Quaternion.Angle(recall.rotation, Quaternion.Euler(0,315,0)) < .01f,
            "Recall point has changed; do not silently replace its world pose.");
        Require(monitor.enabled && monitor.gameObject.activeSelf && !monitor.GetComponent<Renderer>().enabled
            && InteractionOwner(monitor) == anchor, "Keep the hidden monitor collider as the existing E target.");
        foreach (string n in new[] { "R1 Station worktop", "R1 Station inset", "R1 Station solid plinth",
            "R1 panel-owned stable light", "R1 recessed light housing", "R1 stable cool diffuser" })
        {
            var legacy = All<Transform>().Where(t => t.name == n && t.IsChildOf(room)).ToArray();
            Require(legacy.Length > 0 && legacy.All(t => !t.gameObject.activeSelf),
                "Legacy object missing or unexpectedly active: " + n);
        }

        string[] siblingNames = { "Recovery original review desk", "Recovery desk solid envelope",
            "Recovery original monitor casing", "Recovery original jointed lamp", "Recovery desk task pool",
            "Recovery desk pen", "Recovery clerk chair", "Recovery chair collision" };
        var siblings = siblingNames.Select(Named).Concat(All<Transform>().Where(t => t.name == "Recovery reviewed form")).ToArray();
        Require(siblings.Length == 11 && siblings.All(t => t.IsChildOf(room) && !t.IsChildOf(station)),
            "Expected eleven separate workstation roots, including three reviewed forms.");
        var movers = new[] { station }.Concat(siblings).ToArray();
        Require(movers.All(t => !movers.Any(other => other != t && t.IsChildOf(other))), "Workstation roots overlap.");
        var desk = Named("Recovery original review desk");
        Require(Vector3.Distance(desk.position, new Vector3(-7.55f,0,27.8f)) < .001f, "Desk baseline moved.");
        var lamp = Named("Recovery original jointed lamp");
        var task = Named("Recovery desk task pool").GetComponent<Light>();
        var lampSource = lamp.GetComponentsInChildren<Transform>(true).Single(t => t.name == "TaskLamp__LIGHT_SOURCE");
        var lampAim = lamp.GetComponentsInChildren<Transform>(true).Single(t => t.name == "TaskLamp__LIGHT_AIM_TIP");
        Require(task && Vector3.Distance(task.transform.position, lampSource.position) < .001f,
            "Task lamp/source baseline is misaligned.");
        var fixtureA = Fixture(new Vector3(-5.55f,3.05f,28.7f));
        var fixtureB = Fixture(new Vector3(-8.55f,3.05f,26.7f));
        var bounce = Named("Recovery ceiling indirect return").GetComponent<Light>();
        Require(bounce && Mathf.Abs(bounce.range - 5.6f) < .001f, "Indirect-return baseline changed.");
        var label = Named("P3 threshold identity").GetComponent<TextMesh>();
        var textOwner = label.GetComponent<DiegeticText>();
        var header = Named("R2 Anchor gallery access header");
        var printed = Shader.Find("EchoProtocol/PrintedText");
        var enamel = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Recovery/M_Enamel.mat");
        Require(label.font && textOwner && printed && enamel
            && Vector3.Distance(header.position, new Vector3(-5.5f,3.1f,30.2f)) < .001f
            && Vector3.Distance(header.lossyScale, new Vector3(2.1f,.4f,.4f)) < .001f,
            "Physical gallery-sign support/font/shader baseline is missing.");

        var recallPose = new Pose(recall.position, recall.rotation);
        var movePoses = movers.ToDictionary(t => t, t => new Pose(t.position,t.rotation));
        var activeStates = All<Transform>().ToDictionary(t => t, t => t.gameObject.activeSelf);
        var colliders = All<Collider>();
        var colliderPoses = colliders.ToDictionary(c => c, c => new Pose(c.transform.position,c.transform.rotation));
        var colliderOwners = colliders.ToDictionary(c => c, InteractionOwner);
        var fixedColliders = colliders.Where(c => !movers.Any(t => c.transform == t || c.transform.IsChildOf(t))).ToArray();
        var audio = All<AudioSource>();
        var audioPoses = audio.ToDictionary(a => a, a => new Pose(a.transform.position,a.transform.rotation));
        var lights = All<Light>();
        var unchanged = new Object[] { anchor, gate, budget }.Concat(All<PracticalEmission>())
            .Concat(colliders).Concat(audio).Concat(lights.Where(l => l != bounce)).ToDictionary(o => o, Json);
        var report = new StringBuilder("Orientation scene-authoring checks only; no Play/input/camera or human evaluation.\n");
        report.AppendLine("Source: " + ShutdownProduction.ScenePath + " -> " + ScenePath);
        foreach (string field in new[] { "recoveryHub", "loop", "player", "recallPoint", "activeDisplay", "statusLabel", "galleryAccess" })
        { var target = Ref<Object>(anchor,field); Require(target, "Missing anchor reference: " + field); report.AppendLine("anchor." + field + " = " + target.name + " [instance " + target.GetInstanceID() + "]; preserved by serialization comparison."); }
        report.AppendLine("Recall BEFORE " + V(recall.position) + " / " + recall.rotation.ToString("F7"));

        // Save-as first, so subsequent edits cannot save over the Shutdown checkpoint.
        Require(EditorSceneManager.SaveScene(scene,ScenePath), "Could not derive Orientation.");
        AssetDatabase.CreateFolder("Assets/_Game/Art","Orientation");
        var placement = new GameObject(RootName).transform;
        placement.SetParent(room,true);
        placement.SetPositionAndRotation(new Vector3(-7.55f,0,27.8f),Quaternion.identity);
        placement.localScale = Vector3.one;
        foreach (var t in movers) t.SetParent(placement,true);
        placement.SetPositionAndRotation(new Vector3(-7.5f,0,28.5f),Quaternion.Euler(0,30,0));
        recall.SetPositionAndRotation(recallPose.position,recallPose.rotation);

        AimFixture(fixtureA,new Vector3(-5.55f,3.05f,28.7f),new Vector3(-5.55f,.8f,29.7f));
        AimFixture(fixtureB,new Vector3(-7.3f,3.05f,27.1f),new Vector3(-7.6f,1,29.4f));
        bounce.transform.position = new Vector3(-6.65f,2.85f,27.6f);
        bounce.range = 4.2f;
        PhysicalSign(label,textOwner,header,printed,enamel);
        PhysicalShelfIdentity();
        var config = new SerializedObject(director);
        string oldKey = config.FindProperty("checkpointKey").stringValue;
        config.FindProperty("checkpointKey").stringValue = "EP.CampaignCheckpoint.Orientation.v1";
        config.ApplyModifiedPropertiesWithoutUndo();
        Physics.SyncTransforms();

        Require(All<Collider>().Length == colliders.Length && All<AudioSource>().Length == audio.Length
            && All<Light>().Length == lights.Length, "Unexpected collider/audio/light count change.");
        foreach (var pair in unchanged) Require(Json(pair.Key) == pair.Value, "Serialized contract changed: " + pair.Key.name);
        foreach (var pair in activeStates) Require(pair.Key.gameObject.activeSelf == pair.Value, "Authored active state changed: " + pair.Key.name);
        foreach (var c in colliders) Require(InteractionOwner(c) == colliderOwners[c], "Interaction ancestry changed: " + c.name);
        foreach (var c in fixedColliders) Require(SamePose(c.transform,colliderPoses[c]), "Unrelated collider moved: " + c.name);
        foreach (var a in audio) Require(SamePose(a.transform,audioPoses[a]), "Audio source moved: " + a.name);
        Require(SamePose(recall,recallPose) && Ref<Transform>(anchor,"recallPoint") == recall, "Recall world pose/reference changed.");
        Require(Vector3.Distance(task.transform.position,lampSource.position) < .001f
            && Vector3.Angle(task.transform.forward,lampAim.position-lampSource.position) < .1f,
            "Task light detached from the physical lamp.");
        foreach (var pair in movePoses) report.AppendLine(pair.Key.name + ": " + V(pair.Value.position) + " -> " + V(pair.Key.position));
        report.AppendLine("Recall AFTER  " + V(recall.position) + " / " + recall.rotation.ToString("F7") + "; same reference, pose within 10 micrometres / .01 degree.");
        report.AppendLine("Counts BEFORE/AFTER: colliders " + colliders.Length + "/" + All<Collider>().Length
            + "; AudioSources " + audio.Length + "/" + All<AudioSource>().Length + "; Lights " + lights.Length + "/" + All<Light>().Length + ".");
        report.AppendLine("Preserved " + unchanged.Count + " component serializations; all collider interaction ancestry; " + fixedColliders.Length + " unrelated collider poses; all audio poses; all prior activeSelf states.");
        report.AppendLine("Existing A/B lamps repositioned/aimed; existing indirect-return range 5.6 -> 4.2. No new light or source; PracticalEmission and shadow-budget references unchanged.");
        report.AppendLine("P3 plate: (-5.5,3.035,29.989), size (1.75,.20,.026); PrintedText owner and atlas assigned. Text bounds " + V(label.GetComponent<Renderer>().bounds.size) + ".");
        report.AppendLine("Checkpoint namespace: " + oldKey + " -> EP.CampaignCheckpoint.Orientation.v1; no PlayerPrefs written. Gameplay recall world pose unchanged.");
        report.AppendLine("Pending: neutral/closed-door camera, actual approach/E/P3, physical plate illumination and recovery experience. Existing virtual views do not validate those outcomes.");
        EditorSceneManager.MarkSceneDirty(scene);
        Require(EditorSceneManager.SaveScene(scene), "Could not save authored Orientation.");
        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true) };
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath,report.ToString());
        Debug.Log("Orientation authored with preserved R1 rules/recall/interaction ancestry. Review " + ReportPath + "; actual input and camera inspection remain pending.");
    }

    [MenuItem("Echo Protocol/Finish Orientation shelf identity")]
    public static void FinishShelfIdentity()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play before editing the shelf.");
        scene = SceneManager.GetActiveScene();
        Require(scene.path == ScenePath, "Shelf finish belongs to Orientation only.");
        PhysicalShelfIdentity();
        EditorSceneManager.MarkSceneDirty(scene);
        Require(EditorSceneManager.SaveScene(scene), "Could not save shelf finish.");
        AssetDatabase.SaveAssets();
    }
    static void PhysicalShelfIdentity()
    {
        Require(!All<Transform>().Any(t => t.name == "Orientation / shelf index plate"), "Shelf plate already exists.");
        var shelf = Named("Recovery north files shelf");
        var label = Named("Recovery archive shelf identity").GetComponent<TextMesh>();
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "Orientation / shelf index plate";
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        plate.transform.SetPositionAndRotation(new Vector3(-9.10f,1.82f,29.696f),Quaternion.identity);
        plate.transform.localScale = new Vector3(.75f,.10f,.012f);
        plate.transform.SetParent(shelf,true);
        plate.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/M_ReturnGalleryEnamel.mat");
        label.transform.SetPositionAndRotation(new Vector3(-9.10f,1.82f,29.688f),Quaternion.identity);
        label.text = "REVISIONES / 03"; label.fontSize = 72;
        label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
        label.color = new Color(.07f,.09f,.066f);
        label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
        var renderer = label.GetComponent<Renderer>();
        var size = renderer.bounds.size;
        Require(size.x > .0001f && size.y > .0001f, "Shelf text has no bounds.");
        label.characterSize *= Mathf.Min(.65f/size.x,.055f/size.y);
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/M_ReturnGalleryInk.mat");
        var settings = new SerializedObject(label.GetComponent<DiegeticText>());
        settings.FindProperty("depthTestedShader").objectReferenceValue = Shader.Find("EchoProtocol/PrintedText");
        settings.ApplyModifiedPropertiesWithoutUndo();
        Require(renderer.bounds.size.x <= .651f && renderer.bounds.size.y <= .056f, "Shelf ink exceeds the plate.");
    }

    // Housing, diffuser and source have duplicate names; resolve by source geometry
    // and verify the PracticalEmission reference before changing any transform.
    static Transform[] Fixture(Vector3 p)
    {
        var housing = At("Recovery retained fluorescent housing",p);
        var face = At("Recovery opal panel",p-Vector3.up*.045f);
        var source = At("Recovery local working light",p-Vector3.up*.07f);
        Require(Ref<Light>(face.GetComponent<PracticalEmission>(),"source") == source.GetComponent<Light>(),
            "Fluorescent diffuser/source relationship changed.");
        return new[] { housing,face,source };
    }
    static void AimFixture(Transform[] fixture, Vector3 p, Vector3 aim)
    {
        var delta = p-fixture[0].position;
        foreach (var t in fixture) t.position += delta;
        fixture[2].LookAt(aim);
    }
    static void PhysicalSign(TextMesh label, DiegeticText owner, Transform header, Shader shader, Material source)
    {
        var enamel = new Material(source) { name = "M_ReturnGalleryEnamel" };
        enamel.SetColor("_BaseColor",new Color(.55f,.57f,.49f));
        enamel.SetFloat("_Metallic",.05f); enamel.SetFloat("_Smoothness",.25f);
        enamel.SetColor("_EmissionColor",Color.black); enamel.DisableKeyword("_EMISSION");
        AssetDatabase.CreateAsset(enamel,Art + "/M_ReturnGalleryEnamel.mat");
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "Orientation / physical return gallery plate";
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        plate.transform.SetPositionAndRotation(new Vector3(-5.5f,3.035f,29.989f),Quaternion.identity);
        plate.transform.localScale = new Vector3(1.75f,.20f,.026f);
        plate.transform.SetParent(header,true);
        plate.GetComponent<Renderer>().sharedMaterial = enamel;
        label.transform.position = new Vector3(-5.5f,3.035f,29.974f);
        label.text = "GALERÍA DE RETORNO"; label.fontSize = 72;
        label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
        label.color = new Color(.07f,.09f,.066f);
        label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
        var renderer = label.GetComponent<Renderer>();
        var size = renderer.bounds.size;
        Require(size.x > .0001f && size.y > .0001f, "Gallery text did not produce measurable bounds.");
        label.characterSize *= Mathf.Min(1.55f/size.x,.115f/size.y);
        var ink = new Material(shader) { name = "M_ReturnGalleryInk", mainTexture = label.font.material.mainTexture };
        AssetDatabase.CreateAsset(ink,Art + "/M_ReturnGalleryInk.mat");
        renderer.sharedMaterial = ink;
        var settings = new SerializedObject(owner);
        settings.FindProperty("depthTestedShader").objectReferenceValue = shader;
        settings.ApplyModifiedPropertiesWithoutUndo();
        Require(renderer.bounds.size.x <= 1.551f && renderer.bounds.size.y <= .116f,
            "Gallery ink exceeds its physical plate. Inspect the font before saving.");
    }
}

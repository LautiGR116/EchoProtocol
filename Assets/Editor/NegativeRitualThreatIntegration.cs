using System;
using System.Linq;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Opt-in Negative upgrade. Surface maps are original deterministic drawings;
// all private v2 skin buffers and the recorded adult remain untouched.
public static class NegativeRitualThreatIntegration
{
    const string Output = "Assets/_Game/Art/Embodiment/RenderpeoplePrivate/NegativeIntruder/";
    const string UndoName = "Negative ritual performance and surface";

    [MenuItem("Echo Protocol/El Negativo/Apply ritual threat performance")]
    public static void Apply()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling,
            "Finish import and exit Play before applying the present-actor upgrade.");
        var scene = SceneManager.GetActiveScene();
        Require(scene.IsValid() && scene.isLoaded && scene.path.EndsWith("/EchoProtocol_Negative.unity", StringComparison.Ordinal),
            "Apply only to the already open Negative scene. This tool never opens or saves scenes.");
        var receipts = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NegativeIntruderIdentity>(true)).ToArray();
        Require(receipts.Length == 1, "Expected one installed Negative identity.");
        var identity = receipts[0];
        var performance = identity.GetComponent<MachineThreatPerformance>();
        var presence = identity.GetComponent<MachineCoverPresence>();
        Require(identity.revision == 2 && identity.silhouette && identity.body && performance && presence,
            "Requires the validated v2 silhouette and its present performance/presence owners.");
        var fields = new SerializedObject(performance);
        var anatomy = fields.FindProperty("anatomy").objectReferenceValue as RecordedCharacterMotion;
        Require(anatomy && anatomy.IsPresentThreat && !anatomy.GetComponentInParent<EchoPlayback>()
            && !anatomy.GetComponentInParent<LoopActor>(), "Do not change recorded anatomy.");
        var light = new SerializedObject(presence).FindProperty("threatPractical").objectReferenceValue as Light;
        Require(light && light.gameObject.scene == scene && light.type == LightType.Spot,
            "The existing present-body spot practical is required; no light will be created.");
        var renderers = identity.silhouette.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var mask = renderers.Single(r => r.name == "Negative / ivory shell");
        var cowl = renderers.Single(r => r.name == "Negative / closed cowl");
        var coat = renderers.Single(r => r.name == "Negative / split heavy coat");
        var fingers = renderers.Single(r => r.name == "Negative / articulated ivory fingertips");
        NegativeIntruderGeometry.Validate(identity.body.sharedMesh);
        foreach (var renderer in renderers) NegativeIntruderGeometry.Validate(renderer.sharedMesh);
        Require(AssetDatabase.IsValidFolder(Output.TrimEnd('/')), "The private Negative asset folder must already exist.");
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        Require(shader, "URP Lit shader required.");
        var ceramic = Asset("RitualCeramic-v3.asset", () => Texture(0));
        var weave = Asset("RitualWeave-v3.asset", () => Texture(1));
        var binding = Asset("RitualCowlBinding-v3.asset", () => Texture(2));
        var ivory = Asset("RitualIvory-v3.mat", () => Surface(shader, "Negative / scored cast ivory v3",
            new Color(.80f, .765f, .67f), .29f, ceramic, false));
        var textile = Asset("RitualFabric-v3.mat", () => Surface(shader, "Negative / waxed woven coat v3",
            new Color(.145f, .132f, .112f), .41f, weave, true));
        var collar = Asset("RitualCowl-v3.mat", () => Surface(shader, "Negative / bound dark cowl v3",
            new Color(.12f, .103f, .087f), .34f, binding, true));
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        try
        {
            foreach (var renderer in new[] { mask, cowl, coat, fingers }) Undo.RecordObject(renderer, UndoName);
            mask.sharedMaterial = ivory;
            fingers.sharedMaterial = ivory;
            cowl.sharedMaterial = collar;
            coat.sharedMaterial = textile;
            Undo.RecordObject(performance, UndoName);
            fields.FindProperty("negativeRitualPerformance").boolValue = true;
            fields.ApplyModifiedProperties();
            Undo.RecordObject(light, UndoName);
            Undo.RecordObject(light.transform, UndoName);
            // One fixed oblique fitting covers shoulders through knees, even
            // when work lights go out. Presence retains its actual contact dip
            // and every intensity value. No tracking fill or extra shadow map.
            light.transform.rotation = Quaternion.LookRotation(new Vector3(-4.26f, 1.03f, -45.20f) - light.transform.position);
            light.color = new Color(.92f, .80f, .64f);
            light.range = 4.5f;
            light.spotAngle = 94;
            light.innerSpotAngle = 62;
            foreach (var target in new Object[] { mask, cowl, coat, fingers, performance, light, light.transform })
            {
                EditorUtility.SetDirty(target);
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Negative ritual performance applied; scene UNSAVED. Held closed mask, one contact-led articulation, "
                + "shoulder-led approach, original v3 surface maps and existing warm practical. "
                + "No mesh buffers, foot/hand constraints, root motion, collision, Echo or audio changed. "
                + "One central Editor encounter is pending; fear and performance require human judgment.", identity);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Material Surface(Shader shader, string name, Color tint, float smoothness, Texture2D map, bool cloth)
    {
        var result = new Material(shader) { name = name };
        result.SetColor("_BaseColor", tint);
        result.SetTexture("_BaseMap", map);
        result.SetFloat("_Smoothness", smoothness);
        result.SetFloat("_Metallic", 0);
        result.SetFloat("_Cull", cloth ? 0 : 2);
        return result;
    }

    static Texture2D Texture(int type)
    {
        const int size = 512;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, false)
        {
            name = type == 0 ? "Negative / ivory cast, fine fracture and aged underside" :
                type == 1 ? "Negative / fine woven oilcloth and vertical wear" : "Negative / bound cowl and dark thread",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 4
        };
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float u = x / (float)(size - 1), v = y / (float)(size - 1);
            float grain = Mathf.PerlinNoise(u * 191 + 7.4f, v * 173 + 19.7f);
            Color color;
            if (type == 0)
            {
                float mottling = Mathf.PerlinNoise(u * 15 + 6, v * 19 + 2);
                color = new Color(.83f, .81f, .75f) * (.94f + grain * .075f + mottling * .11f);
                // One irregular cast fracture; it never outlines eyes, a mouth
                // or a conventional human face. The closed v2 volume survives.
                float seam = .34f + .16f * v + .014f * Mathf.Sin(v * 27) + .006f * Mathf.Sin(v * 71);
                float distance = Mathf.Abs(u - seam);
                float length = Smooth(.07f, .15f, v) * (1 - Smooth(.77f, .88f, v));
                float dirt = (1 - Smooth(.002f, .019f, distance)) * length;
                float score = (1 - Smooth(.0009f, .0038f, distance)) * length;
                color = Color.Lerp(color, new Color(.38f, .29f, .205f), dirt * .38f);
                color = Color.Lerp(color, new Color(.145f, .07f, .045f), score * .79f);
                float underside = (1 - Smooth(.025f, .15f, v)) * (.48f + .52f * mottling);
                color = Color.Lerp(color, new Color(.24f, .092f, .064f), underside * .52f);
                float chip = Smooth(.76f, .91f, grain) * (1 - Smooth(.025f, .07f, Mathf.Min(u, 1 - u)));
                color = Color.Lerp(color, new Color(.38f, .30f, .22f), chip * .65f);
            }
            else
            {
                float fiber = .5f + .5f * Mathf.Sin(u * Mathf.PI * 380);
                float cross = .5f + .5f * Mathf.Sin(v * Mathf.PI * 310);
                float runnels = Mathf.PerlinNoise(u * 39 + 9, v * 3.5f + 5);
                float wear = .64f + .13f * fiber + .035f * cross + .10f * grain + .10f * runnels;
                color = new Color(wear, wear * .975f, wear * .93f);
                if (type == 2)
                {
                    // The lowest cowl ring reads as a dark bound collar. Short
                    // stitches follow the seam; these are textile marks only.
                    float band = (1 - Smooth(.08f, .135f, v));
                    color = Color.Lerp(color, new Color(.36f, .13f, .085f), band * .68f);
                    float line = (1 - Smooth(.004f, .010f, Mathf.Abs(v - .095f)));
                    float stitch = line * Smooth(.45f, .72f, Mathf.Sin(u * Mathf.PI * 78));
                    color = Color.Lerp(color, new Color(.65f, .51f, .35f), stitch * .64f);
                }
            }
            color.a = 1;
            Require(Finite(color.r) && Finite(color.g) && Finite(color.b), "Non-finite surface sample.");
            pixels[y * size + x] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply(true, false);
        return texture;
    }

    static T Asset<T>(string file, Func<T> create) where T : Object
    {
        string path = Output + file;
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing) return existing;
        Require(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)), "Preserve incompatible existing asset: " + path);
        var asset = create();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
    static float Smooth(float from, float to, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(from, to, value));
    static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

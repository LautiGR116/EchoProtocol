// Integrated Editor authoring; original proposal remains in ArtSource/Design/RecordsPassage.
// Derives a new scene; no material, mesh, texture, clip or prefab is modified.
// Human composition and listening remain unverified until real cameras/input.
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using Object = UnityEngine.Object;

public static class RecordsProduction
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Records.unity";
    public const string RootName = "Records / physical return passage";
    const string FluorescentPath = "Assets/_Game/Audio/Clips/Ambience/AMB_Fluorescent_LP.wav";
    const string FluorescentGuid = "ab6c64bce0c5b40e4b2cf92ff8852472";
    const float WestPlane = -20f, EntryZ = 27f, ExitZ = 15f, AisleX = -21.15f;
    const float OpeningWidth = 1.8f;
    static Transform visuals, solids, sound;
    static Material paint, trim, wood, glass;

    // Suggested INPUT-DRIVEN route, replacing only (-18.6,29)->(-18.6,13).
    // Feet Y=.03 is descriptive, not a request to ResetPose or teleport.
    // The continuous probe should walk each point with its existing real inputs.
    // (x,z,yaw): (-18.6,29,180), (-18.6,27,180), (-21.15,27,270),
    //            (-21.15,24.5,180), (-21.15,17.5,180), (-21.15,15,180),
    //            (-18.6,15,90), (-18.6,13,180), then original route to z7/-9.5.
    // Added orthogonal centreline length: 2 * (21.15-18.6) = 5.1m;
    // at 4m/s, +1.275s of translation (camera turns/control are not yet measured).
    // Frames at each aperture consume .08m in total: clear opening=1.72m.
    // Aisle between shelf edge x=-22.43 and facade x=-20.10: 2.33m;
    // the projecting timber cap reduces its local clear width to 2.28m.
    // Suggested actual cameras, eyes Y~1.63, vertical FOV60, pitch0:
    //   Entry (-18.6,29.0), yaw180: solid partition ahead, warm opening on right.
    //   Inside (-21.15,24.5), yaw180: nearby racks and inhabited working aisle.
    //   Lateral view (-21.15,21), yaw150: south gallery through retained glazing;
    //   this is not the already-walked northern route or proof of recognition.
    //   Exit (-21.15,15), yaw90: aperture back to gallery; then look south.
    // No fixed-camera runtime or new interaction/event is authored.

    static GameObject[] Named(string name) => Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None)
        .Where(t => t.gameObject.scene == SceneManager.GetActiveScene() && t.name == name)
        .Select(t => t.gameObject).ToArray();

    static GameObject One(string name)
    {
        var found = Named(name);
        if (found.Length != 1) throw new Exception("Expected exactly one scene object: " + name + "; found " + found.Length);
        return found[0];
    }

    static Material MaterialOn(string name)
    {
        var renderer = One(name).GetComponent<Renderer>();
        if (!renderer || !renderer.sharedMaterial) throw new Exception("Missing reusable material on " + name);
        return renderer.sharedMaterial;
    }

    static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
    }

    static GameObject Visual(string name, Vector3 position, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(visuals, false);
        go.transform.position = position; go.transform.localScale = size;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
        if (material == paint || material == wood)
            go.AddComponent<SurfaceMapping>().Configure(renderer,
                new Vector2(Mathf.Max(size.x, size.z) / (material == wood ? 2.5f : 3.2f), size.y / (material == wood ? 2.5f : 3.2f)));
        return go;
    }

    static BoxCollider Solid(string name, Vector3 position, Vector3 size)
    {
        var go = new GameObject(name); go.transform.SetParent(solids, false); go.transform.position = position;
        var collider = go.AddComponent<BoxCollider>(); collider.size = size; return collider;
    }

    static void WallSolid(string name, float firstZ, float lastZ)
    {
        if (lastZ <= firstZ) throw new Exception("Invalid wall span " + name);
        Solid(name, new Vector3(WestPlane, 1.6f, (firstZ + lastZ) * .5f), new Vector3(.2f, 3.2f, lastZ - firstZ));
    }

    static void WindowSpan(string name, float firstZ, float lastZ)
    {
        float middle = (firstZ + lastZ) * .5f, length = lastZ - firstZ;
        Visual(name + " / sill", new Vector3(WestPlane, .42f, middle), new Vector3(.20f, .84f, length), trim);
        Visual(name + " / timber cap", new Vector3(-19.98f, .86f, middle), new Vector3(.34f, .06f, length), wood);
        Solid(name + " / timber cap collision", new Vector3(-19.98f, .86f, middle), new Vector3(.34f, .06f, length));
        Visual(name + " / fixed pane", new Vector3(WestPlane, 1.68f, middle), new Vector3(.02f, 1.61f, length), glass);
        Visual(name + " / transom", new Vector3(-19.97f, 2.16f, middle), new Vector3(.13f, .055f, length), trim);
        // This is the facade's solid wall/closed window section, including its
        // frame. The pane remains genuinely blocked by physics; no invisible bypass.
        WallSolid(name + " / facade collision", firstZ, lastZ);
    }

    static void Portal(float centreZ, string name)
    {
        foreach (float z in new[] { centreZ - OpeningWidth * .5f, centreZ + OpeningWidth * .5f })
        {
            Visual(name + " / jamb", new Vector3(WestPlane, 1.25f, z), new Vector3(.24f, 2.5f, .08f), trim);
            Solid(name + " / jamb collision", new Vector3(WestPlane, 1.25f, z), new Vector3(.24f, 2.5f, .08f));
        }
        Visual(name + " / head", new Vector3(WestPlane, 2.465f, centreZ), new Vector3(.24f, .07f, OpeningWidth), trim);
        Solid(name + " / head collision", new Vector3(WestPlane, 2.465f, centreZ), new Vector3(.24f, .07f, OpeningWidth));
        // No sill, door, trigger or key: clear floor and headroom >2.4m.
    }

    static void AuthorBoundary()
    {
        // Reception kept this full-height collider after hiding its renderer.
        // Disable only that obsolete collision. Existing perimeter rendering stays.
        var oldWall = One("R3 spine west wall");
        var oldCollider = oldWall.GetComponent<BoxCollider>();
        if (!oldCollider || !oldCollider.enabled || oldWall.GetComponent<Renderer>().enabled)
            throw new Exception("Expected the hidden, solid R3 west boundary retained by Reception");
        oldCollider.enabled = false;

        // These four continuous render strips would otherwise cross both openings.
        // Preserve originals inactive in the derived scene; all five old mullions
        // and the continuous high lintel remain in place.
        foreach (string name in new[] { "Records window solid sill", "Records window sill cap", "Records fixed glazing", "Records glazing transom" })
            One(name).SetActive(false);

        // Exact old collision extent was z[-10.9,48.3]. Three facade spans leave
        // openings z[14.1,15.9] and [26.1,27.9]. Jambs give net width1.72m.
        WallSolid("West original south perimeter", -10.9f, 14f);
        WallSolid("West original north perimeter", 28f, 48.3f);
        WindowSpan("Records south fixed end", 14f, ExitZ - OpeningWidth * .5f);
        WindowSpan("Records retained central window", ExitZ + OpeningWidth * .5f, EntryZ - OpeningWidth * .5f);
        WindowSpan("Records north fixed end", EntryZ + OpeningWidth * .5f, 28f);
        Solid("Records existing lintel collision", new Vector3(WestPlane, 2.85f, 21f), new Vector3(.20f, .70f, 14f));
        Portal(EntryZ, "Records north open portal"); Portal(ExitZ, "Records south open portal");

        // The old archive was scenery only. Add an actual envelope and coarse
        // rack collision independently of the many carton/shelf render pieces.
        Solid("Records floor collision", new Vector3(-22, -.10f, 21), new Vector3(4, .20f, 14));
        Solid("Records ceiling collision", new Vector3(-22, 3.10f, 21), new Vector3(4, .20f, 14));
        Solid("Records back wall collision", new Vector3(-24, 1.5f, 21), new Vector3(.20f, 3, 14));
        foreach (float z in new[] { 14f, 28f })
            Solid("Records end wall collision", new Vector3(-22, 1.5f, z), new Vector3(4, 3, .20f));
        foreach (float z in new[] { 15.5f, 18f, 20.5f, 23f, 25.5f })
            Solid("Records rack collision", new Vector3(-22.9f, 1.20f, z), new Vector3(.94f, 2.40f, 1.76f));

        // A permanent opaque circulation partition closes the former straight
        // shortcut. Its revoco, timber base and crown visibly meet both walls,
        // floor and ceiling. It is architecture, never an invisible restriction.
        Visual("Gallery circulation partition", new Vector3(-18.6f, 1.6f, 25.4f), new Vector3(2.8f, 3.2f, .20f), paint);
        Solid("Gallery partition collision", new Vector3(-18.6f, 1.6f, 25.4f), new Vector3(2.8f, 3.2f, .20f));
        foreach (float z in new[] { 25.292f, 25.508f })
        {
            Visual("Gallery partition timber base", new Vector3(-18.6f, .45f, z), new Vector3(2.8f, .90f, .018f), wood);
            Visual("Gallery partition dado", new Vector3(-18.6f, .93f, z), new Vector3(2.8f, .035f, .036f), trim);
        }
        Visual("Gallery partition head trim", new Vector3(-18.6f, 3.08f, 25.4f), new Vector3(2.8f, .16f, .25f), trim);

        // Reuse the ordinary archive identity, formerly floating in the new
        // northern opening. No route puzzle, new instruction or added sign.
        var label = One("Paper records identity");
        var backing = One("Paper records identity backing");
        backing.transform.SetPositionAndRotation(new Vector3(-18.6f, 1.78f, 25.515f), Quaternion.Euler(0, 180, 0));
        label.transform.SetPositionAndRotation(new Vector3(-18.6f, 1.78f, 25.542f), Quaternion.Euler(0, 180, 0));
    }

    static void AuthorFixtureSound(AudioClip clip, AudioMixerGroup group)
    {
        // AUD-AMB-002: JoelMcDaniel, Fluorescent Light Buzzing, CC0.
        // https://freesound.org/people/JoelMcDaniel/sounds/830440/
        // Selected loop/importer/hash ledger: Docs/AUDIO_ASSET_LEDGER.md.
        // Reuse the accepted mono12.85s master; do not generate/modify audio.
        var lamps = Named("Archive shelf illumination").Select(o => o.GetComponent<Light>()).OrderBy(l => l.transform.position.z).ToArray();
        if (lamps.Length != 2 || lamps.Any(l => !l || !l.enabled || l.intensity <= 0))
            throw new Exception("Expected two powered, physical archive lamps");
        foreach (var lamp in lamps)
        {
            var go = new GameObject("Records actual fixture hum / " + lamp.transform.position.z.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
            go.transform.SetParent(sound, false);
            // Existing opal diffuser y2.80, light emitter y2.70. The sound sits
            // on the fixture, not the camera, route trigger or Echo.
            go.transform.position = lamp.transform.position + Vector3.up * .10f;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip; source.outputAudioMixerGroup = group;
            // .06 compensates the real master RMS and local distance: near the
            // fixture it approximately matches the historical .05 2D bed.
            // See the adjacent README; this is an initial mix, not audition.
            source.playOnAwake = true; source.loop = true; source.volume = .06f;
            source.spatialBlend = 1f; source.dopplerLevel = 0f; source.priority = 200;
            source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = .5f; source.maxDistance = 4.5f;
            source.ignoreListenerPause = false;
            // Static powered fixtures in this edition: no new runtime writer,
            // SE1 ownership, chapter mute, timed cue or loop-reset participation.
        }
        // Keep the existing housing and real lamp; aim the north light onto its
        // approach floor so the open side is legible from the former straight.
        lamps[1].transform.LookAt(new Vector3(AisleX, .25f, 26.4f));
        // The far opening initially read as a dark terminal wall. Let the existing
        // southern fixture lead the floor into its aperture, without extra gain.
        lamps[0].transform.LookAt(new Vector3(-20.4f, .25f, 15.5f));
    }

    [MenuItem("Echo Protocol/Author physical records return")]
    public static void Author()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play before authoring Records");
        if (AssetDatabase.AssetPathToGUID(FluorescentPath) != FluorescentGuid)
            throw new Exception("The traced fluorescent asset/importer identity changed; review the ledger before authoring");
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(FluorescentPath);
        if (!clip || clip.channels != 1 || Mathf.Abs(clip.length - 12.85f) > .05f)
            throw new Exception("Expected the existing traced mono12.85s fluorescent loop");
        var importer = AssetImporter.GetAtPath(FluorescentPath) as AudioImporter;
        if (!importer || !importer.defaultSampleSettings.preloadAudioData)
            throw new Exception("Expected the reviewed preloaded fluorescent importer");
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MX_Main.mixer");
        if (!mixer) throw new Exception("Missing existing main mixer");
        var ambience = mixer.FindMatchingGroups("Ambience").Single(g => g.name == "Ambience");

        var scene = EditorSceneManager.OpenScene(EncounterProduction.ScenePath);
        if (Named(RootName).Length != 0) throw new Exception("Encounter unexpectedly already contains the Records authoring root");
        // Fail before deriving if the existing archive no longer matches this brief.
        paint = MaterialOn("Return west north wall"); trim = MaterialOn("Records window solid sill");
        wood = MaterialOn("Records window sill cap"); glass = MaterialOn("Records fixed glazing");
        if (Named("Records rack shelf").Length != 20 || Named("Records end wall").Length != 2)
            throw new Exception("Expected five four-tier archive racks and two end walls");
        foreach (string name in new[] { "Records room floor", "Records room ceiling", "Records back wall", "Paper records identity", "Paper records identity backing" }) One(name);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not derive Records scene");
        var root = new GameObject(RootName).transform;
        visuals = Group("Presentation / no collision", root);
        solids = Group("Physical boundary / no renderers", root);
        sound = Group("Powered fixture sound", root);
        AuthorBoundary(); AuthorFixtureSound(clip, ambience);
        // Former scenery cubes had one stretched UV tile over the whole room.
        // Match the existing corridor's3.2m material repeat on the occupied envelope.
        foreach (string name in new[] { "Records room floor", "Records room ceiling", "Records back wall", "Records end wall", "Return west south wall", "Return west north wall" })
        foreach (var go in Named(name))
        {
            var renderer=go.GetComponent<Renderer>(); var size=renderer.bounds.size;
            var mapping=go.GetComponent<SurfaceMapping>();if(!mapping)mapping=go.AddComponent<SurfaceMapping>();
            mapping.Configure(renderer,size.y<Mathf.Min(size.x,size.z)?new Vector2(size.x/3.2f,size.z/3.2f):new Vector2(Mathf.Max(size.x,size.z)/3.2f,size.y/3.2f));
        }
        var director = Object.FindFirstObjectByType<CampaignDirector>();
        if (!director) throw new Exception("Missing campaign director in Encounter successor");
        var state = new SerializedObject(director);
        state.FindProperty("checkpointKey").stringValue = "EP.CampaignCheckpoint.Records.v1";
        state.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Records successor authored: static archive through-route, separate physical envelope, retained arrival interlock and two traced fixture loops. Real cameras, controls and listening still require review; source geometry estimates are not Play evidence.");
    }
}

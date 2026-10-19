#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

/// <summary>
/// Public-API save fixtures, genuine PlayerPrefs round trips and fresh-scene continuation.
/// Synthetic Input System events exercise the resumed recorder. This is not a campaign playthrough.
/// </summary>
public sealed class ThresholdCheckpointProbe : MonoBehaviour
{
    string ExpectedKey;
    const string LegacyKey = "EP.CampaignCheckpoint.v1";
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly string[] Cases = { "general", "hall", "kitchen", "bedroom", "carried" };
    readonly List<Preference> preferences = new List<Preference>();
    readonly List<Preference> legacyPreferences = new List<Preference>();
    readonly List<string> completedCases = new List<string>();
    CampaignDirector campaign;
    GameSession session;
    FirstPersonController player;
    PlayerInput playerInput;
    EchoRecorder recorder;
    LoopController loop;
    HouseMemory house;
    FinalDoorMemory review;
    Transform origin;
    HouseSwingDoor houseExit, reviewDoor;
    SlidingDoor finalExit;
    Keyboard keyboard;
    Mouse mouse;
    InputDevice[] previousDevices;
    string previousScheme;
    bool previousAutoSwitch, devicesConfigured, expectedPause, resumeDown;
    bool running, preferenceBackupReady, cleanupDone, oldSuppressSaves;
    InputSettings.EditorInputBehaviorInPlayMode oldInputBehavior;
    Key[] keys = Array.Empty<Key>();
    string mode, scenePath, output, step = "initialization", activeCase, runtimeError;
    int checks, reloads, fixturePlacements, focusResumes;
    float startedAt;
    bool[] initialGeneral, initialHall, initialKitchen, initialBedroom;

    [Serializable] sealed class Preference { public string key; public bool exists; public int value; }
    [Serializable] sealed class Result
    {
        public string status, mode, scene, lastStep, error, scope;
        public string[] completedCases;
        public int checks, sceneReloads, explicitFixturePlacements, unexpectedFocusResumes;
        public float elapsedRealtime;
        public bool preferencesRestored, suppressSavesRestored;
    }

    public string Status { get; private set; } = "starting";
    public string CurrentStep => step;

    /// <param name="mode">all: four reserve locations plus unanswered/general control; otherwise general, hall, kitchen, bedroom or carried.</param>
    public static void Run(string mode = "all")
    {
        if (!Application.isPlaying) throw new InvalidOperationException("ThresholdCheckpointProbe requires prepared Play Mode.");
        if (mode != "all" && !Cases.Contains(mode)) throw new ArgumentException("Use all, general, hall, kitchen, bedroom or carried.");
        if (FindObjectsByType<ThresholdCheckpointProbe>(FindObjectsSortMode.None).Any(p => p.running))
            throw new InvalidOperationException("A checkpoint probe is already running.");
        var probe = new GameObject("Temporary Threshold checkpoint verification").AddComponent<ThresholdCheckpointProbe>();
        DontDestroyOnLoad(probe.gameObject);
        probe.mode = mode;
        string edition = SceneManager.GetActiveScene().name.Replace("EchoProtocol_", "");
        probe.ExpectedKey = edition == "Threshold" ? "EP.ThresholdCheckpoint.v1" : "EP.CampaignCheckpoint." + edition + ".v1";
        probe.output = Path.Combine("Docs/VisualEvidence/2026-10-01-" + edition, "checkpoint-" + mode);
        Directory.CreateDirectory(Path.GetDirectoryName(probe.output));
        probe.running = true;
        probe.StartCoroutine(probe.Guarded(probe.Execute()));
    }

    IEnumerator Guarded(IEnumerator root)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(root);
        while (stack.Count > 0)
        {
            object yielded;
            try
            {
                if (!stack.Peek().MoveNext()) { stack.Pop(); continue; }
                yielded = stack.Peek().Current;
            }
            catch (Exception exception) { Finish(false, exception.ToString()); yield break; }
            if (yielded is IEnumerator nested) { stack.Push(nested); continue; }
            yield return yielded;
        }
    }

    IEnumerator Execute()
    {
        yield return null;
        startedAt = Time.realtimeSinceStartup;
        File.WriteAllText(output + ".checks.txt", "ISOLATED CHECKPOINT VERIFICATION. API fixtures and scene reloads; synthetic movement/R only after Continue. No human-play claim.\n");
        File.Delete(output + ".result.json");
        scenePath = SceneManager.GetActiveScene().path;
        ResolveReferences();
        Check(!string.IsNullOrEmpty(scenePath) && Application.CanStreamedLevelBeLoaded(scenePath), "active scene can be reloaded through the actual player scene path");
        Check(Field<string>(campaign, "checkpointKey") == ExpectedKey, "authored edition checkpoint is isolated from the old campaign key");
        oldSuppressSaves = campaign.SuppressSaves;
        oldInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        foreach (string suffix in new[] { "", ".answered", ".light", ".isolated" })
        {
            preferences.Add(ReadPreference(ExpectedKey + suffix));
            legacyPreferences.Add(ReadPreference(LegacyKey + suffix));
        }
        preferenceBackupReady = true;
        Application.logMessageReceived += ReceiveLog;
        foreach (string selected in mode == "all" ? Cases : new[] { mode })
        {
            activeCase = selected;
            yield return ReloadFreshScene();
            yield return ProduceRealSave(selected);
            yield return ReloadFreshScene();
            yield return ContinueSavedCase(selected);
            completedCases.Add(selected);
        }
        step = "restore_original_preferences";
        Finish(true, "");
    }

    void ResolveReferences()
    {
        campaign = FindFirstObjectByType<CampaignDirector>(); session = FindFirstObjectByType<GameSession>();
        player = FindFirstObjectByType<FirstPersonController>(); loop = FindFirstObjectByType<LoopController>();
        house = FindFirstObjectByType<HouseMemory>(); review = FindFirstObjectByType<FinalDoorMemory>();
        Check(campaign && campaign.enabled && session && player && loop && house && house.enabled && review && review.enabled,
            "configured Threshold campaign, house, review and Player systems exist");
        playerInput = player.GetComponent<PlayerInput>(); recorder = player.GetComponent<EchoRecorder>();
        origin = Field<Transform>(review, "origin"); houseExit = Field<HouseSwingDoor>(house, "exitDoor");
        reviewDoor = Field<HouseSwingDoor>(review, "domesticDoor"); finalExit = Field<SlidingDoor>(review, "exitDoor");
        Check(playerInput && recorder && origin && houseExit && reviewDoor && finalExit, "recording and physical threshold references are assigned");
        Check(Field<FinalDoorMemory>(campaign, "finalDoor") == review && Field<FinalDoorMemory>(session, "finalDoorMemory") == review,
            "director and session use this scene's new final act");
        Check(Field<HouseMemory>(review, "house") == house && Field<Transform[]>(campaign, "origins")[3] == origin,
            "continuation uses the real house and the same final recall origin");
    }

    IEnumerator ReloadFreshScene()
    {
        step = activeCase + "/fresh_scene";
        if (session) session.SetPaused(true);
        ReleaseDevices();
        var loading = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
        Check(loading != null, "fresh scene load was accepted");
        while (!loading.isDone) yield return null;
        yield return null; // GameSession.Start must establish the initial menu before Continue.
        ResolveReferences(); reloads++;
        campaign.SuppressSaves = true;
        Check(Field<string>(campaign, "checkpointKey") == ExpectedKey, "reloaded edition keeps its own checkpoint namespace");
        Check(session.IsPaused && !session.IsEnding && !review.Active && !house.HasKey, "reload starts a fresh session instead of reusing completed runtime state");
        initialGeneral = LightState("generalLights"); initialHall = LightState("hallLights");
        initialKitchen = LightState("kitchenLights"); initialBedroom = LightState("bedroomLights");
        ConfigureDevices();
        CheckRuntime();
    }

    IEnumerator ProduceRealSave(string selected)
    {
        step = selected + "/legitimate_save_fixture";
        bool answered = selected != "general"; HousePowerLocation location = Location(selected);
        var beforeSuppressedEntry = new[] { ReadPreference(ExpectedKey), ReadPreference(ExpectedKey + ".answered"), ReadPreference(ExpectedKey + ".light") };
        session.BeginOrResume(); expectedPause = false;
        campaign.PrepareHouseProbe();
        var station = FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s => s.Kind == StationKind.HouseOrigin);
        Check(station.CanInteract(player.gameObject), "explicit prior-pressure fixture permits the actual house entry API");
        station.Interact(player.gameObject);
        Check(house.Active && !loop.enabled && !recorder.IsRecording, "house entry really suspends Echo recording");
        Check(beforeSuppressedEntry.All(PreferenceUnchanged), "SuppressSaves prevents house-entry preference writes");
        Check(!review.EnterReview() && !review.Active, "review cannot be entered before the physical key is acquired");
        if (answered) house.Use(HouseInteractionKind.Telephone, player.gameObject, player.transform.position);
        if (location != HousePowerLocation.Hall)
        {
            house.Use(HouseInteractionKind.HallSocket, player.gameObject, player.transform.position);
            if (location != HousePowerLocation.Carried)
                house.Use(location == HousePowerLocation.Kitchen ? HouseInteractionKind.KitchenSocket : HouseInteractionKind.BedroomSocket,
                    player.gameObject, player.transform.position);
        }
        house.Use(HouseInteractionKind.Key, player.gameObject, player.transform.position);
        house.Use(HouseInteractionKind.ExitDoor, player.gameObject, player.transform.position);
        Check(house.HasKey && house.HasExited && house.HasAnswered == answered && house.PowerLocation == location,
            "public house choices earn the intended phone, reserve and key state");
        // One labelled placement per save fixture. No placement or fake recording is used after Continue.
        player.ResetPose(origin.position, origin.rotation); fixturePlacements++;
        campaign.SuppressSaves = false;
        Check(review.EnterReview(), "actual final entry accepts the legitimately obtained key");
        yield return Wait(.12f);
        Check(campaign.Chapter == CampaignChapter.Archive && campaign.HasSavedCheckpoint && campaign.SavedChapterTitle == "Del otro lado",
            "the real director commits the new Archive checkpoint and menu title");
        Check(PlayerPrefs.GetInt(ExpectedKey, -1) == (int)CampaignChapter.Archive &&
            PlayerPrefs.GetInt(ExpectedKey + ".answered", -1) == (answered ? 1 : 0) && PlayerPrefs.GetInt(ExpectedKey + ".light", -1) == (int)location,
            "saved preferences contain the actual domestic decisions");
        Check(loop.LastRecording == null && loop.ActiveEcho == null, "saving at the final origin does not manufacture an Echo");
        Check(legacyPreferences.All(PreferenceUnchanged), "Threshold entry leaves old campaign continuation untouched");
        campaign.SuppressSaves = true;
    }

    IEnumerator ContinueSavedCase(string selected)
    {
        step = selected + "/actual_continue_api";
        bool answered = selected != "general"; HousePowerLocation location = Location(selected);
        var saved = new[] { ReadPreference(ExpectedKey), ReadPreference(ExpectedKey + ".answered"), ReadPreference(ExpectedKey + ".light") };
        Check(campaign.HasSavedCheckpoint && campaign.SavedChapterTitle == "Del otro lado", "fresh initial menu recognizes the saved final review");
        session.BeginOrResume(); expectedPause = false;
        Check(campaign.RestoreCheckpoint(), "the same public RestoreCheckpoint API used by Continue succeeds");
        Check(campaign.Chapter == CampaignChapter.Archive && review.Active && !house.Active && !session.IsPaused,
            "Continue enters the actual new final review and ends domestic interactions");
        Check(house.HasKey && house.HasExited && house.HasAnswered == answered && house.PowerLocation == location,
            "restoration retains only earned key and domestic decisions");
        Check(campaign.DocumentsRead == 0 && campaign.CircuitsRestored == 0 && campaign.GalleryPairs == 0 && !campaign.HasFinalWitness,
            "new final continuation invents no document reads, old circuits, gallery pairs or old witness");
        Check(Vector3.Distance(player.transform.position, origin.position) < .015f &&
            Vector3.Distance(loop.RecallSpawnPosition, origin.position) < .001f && Quaternion.Angle(loop.RecallSpawnRotation, origin.rotation) < .01f,
            "Player and RECALL begin at the authored interior origin");
        Check(loop.enabled && recorder.IsArmed && recorder.CapturedFrameCount == 0 && loop.LastRecording == null && loop.ActiveEcho == null,
            "continuation arms an empty recorder with no saved or fabricated take");
        CheckFreshFinal(); CheckHousePresentation(answered, location);
        yield return Wait(.25f);
        var sonic = FindFirstObjectByType<EchoProtocol.Audio.ResonanceSequence>();
        if (sonic)
        {
            Check(sonic.BaselineEstablished && sonic.CallCommitted && sonic.PressureCommitted,
                "Resonance fresh Continue establishes spent call and shutdown without re-enacting them");
            Check(FindObjectsByType<EchoProtocol.Audio.ResonanceEmitter>(FindObjectsSortMode.None).All(e =>
                e.name.Contains("air in fixed riser") || e.PlayCount == 0),
                "fresh 6B checkpoint emits no earlier voice, strain or recognition music");
        }

        Check(recorder.IsArmed && recorder.CapturedFrameCount == 0 && loop.LastRecording == null, "standing at restored origin does not capture the restoration teleport");
        var exterior = FindFirstObjectByType<EchoProtocol.Audio.CourtyardAir>();
        if (exterior) Check(exterior.AppliedGain == 0f && exterior.RegionGain == 0f, "Continue in the relocated final leaves outdoor ambience silent");
        Check(!review.CanUse(FinalDoorInteractionKind.Preserve, player.gameObject) && !review.CanUse(FinalDoorInteractionKind.ReturnKey, player.gameObject),
            "fresh restoration cannot skip witness, preservation or key return");
        review.Use(FinalDoorInteractionKind.Preserve, player.gameObject, player.transform.position);
        review.Use(FinalDoorInteractionKind.ReturnKey, player.gameObject, player.transform.position);
        CheckFreshFinal();

        step = selected + "/pause_and_recall_recovery";
        int attempts = loop.CompletedLoopCount;
        expectedPause = true; session.SetPaused(true);
        yield return Press(Key.R);
        Check(session.IsPaused && loop.CompletedLoopCount == attempts && loop.LastRecording == null && review.Fade == 0,
            "R while paused neither records nor advances the fresh ending");
        session.BeginOrResume(); expectedPause = false;
        Check(player.enabled && loop.enabled && recorder.IsArmed, "resume restores Player and armed RECALL after continuation");
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == attempts && loop.LastRecording == null, "R without movement cannot manufacture an empty take");
        Vector3 initialPose = player.transform.position;
        yield return Hold(Key.D, .20f);
        Check(HorizontalDistance(player.transform.position, initialPose) > recorder.StartMovementThreshold * 2 && recorder.HasRecordingStarted,
            "real synthetic movement starts a new recording after restoration");
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == attempts + 1 && loop.ActiveEcho && loop.LastRecording != null &&
            ReferenceEquals(loop.LastRecording, loop.ActiveEcho.Recording), "one real R creates one authentic new snapshot and matching Echo");
        EchoRecording first = loop.LastRecording;
        var firstFrames = Enumerable.Range(0, first.FrameCount).Select(i => first[i]).ToArray();
        Check(first.FrameCount > 2 && HorizontalDistance(first[0].Position, origin.position) < .03f &&
            Enumerable.Range(0, first.FrameCount).Any(i => HorizontalDistance(first[i].Position, first[0].Position) > .04f),
            "the resumed snapshot contains the origin and the actual new displacement");
        Check(HorizontalDistance(player.transform.position, origin.position) < .03f && !review.HasWitnessed && review.QualifiedPauseCount == 0,
            "RECALL recovers the origin while an inside-only take earns no final witness");
        review.Use(FinalDoorInteractionKind.OpenDoor, player.gameObject, reviewDoor.transform.position);
        Check(reviewDoor.IsOpenRequested, "new review door remains manually recoverable after Continue");
        yield return Hold(Key.A, .18f);
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == attempts + 2 && !ReferenceEquals(first, loop.LastRecording) &&
            !reviewDoor.IsOpenRequested && !reviewDoor.IsFullyOpen && houseExit.IsOpenRequested,
            "another real R replaces the take and resets only 6B, preserving the completed house exit");
        Check(first.FrameCount == firstFrames.Length && Enumerable.Range(0, firstFrames.Length).All(i => SameFrame(first[i], firstFrames[i])),
            "later movement and RECALL leave every frame of the previous snapshot immutable");
        EchoRecording current = loop.LastRecording; Vector3 currentPose = player.transform.position;
        Check(!campaign.RestoreCheckpoint() && ReferenceEquals(current, loop.LastRecording) &&
            Vector3.Distance(currentPose, player.transform.position) < .001f,
            "a second Continue call during active review is rejected without erasing its real recording or moving the Player");
        Check(!review.HasWitnessed && !review.OriginalPreserved && !review.KeyReturned && !review.IsComplete && !session.IsEnding && !finalExit.IsOpenRequested,
            "restored play still requires a real exterior approach and witness before physical completion");
        Check(saved.All(PreferenceUnchanged) && legacyPreferences.All(PreferenceUnchanged), "recall recovery does not overwrite either edition's saved decisions");
        CheckRuntime();
    }

    void CheckFreshFinal()
    {
        Check(!review.HasWitnessed && review.WitnessedRecording == null && review.ReviewedRecording == null && review.PreservedOriginal == null &&
            !review.OriginalPreserved && !review.KeyReturned && !review.IsComplete && !review.HasStartedDeparture && review.Fade == 0 && !session.IsEnding,
            "final witness, preserved reference, key return, departure and completion all start fresh");
        Check(!reviewDoor.IsOpenRequested && !finalExit.IsOpenRequested && !Field<GameObject>(review, "keyInTray").activeInHierarchy && houseExit.IsOpenRequested,
            "physical doors and key tray distinguish the completed house from the uncompleted final review");
    }

    void CheckHousePresentation(bool answered, HousePowerLocation location)
    {
        Check(!Field<GameObject>(house, "keyWorld").activeInHierarchy, "restored key no longer remains in the house as a duplicate pickup");
        string[] meshes = { "hallFuse", "kitchenFuse", "bedroomFuse" };
        Check(meshes.Select((name, index) => Field<GameObject>(house, name)).Select((mesh, index) => mesh && mesh.activeInHierarchy == (index == (int)location)).All(v => v),
            "only the chosen socket contains its reserve, or all sockets are empty while carried");
        var carriedMesh = Field<GameObject>(house, "carriedFuse");
        Check(!carriedMesh || carriedMesh.activeSelf == (location == HousePowerLocation.Carried), "optional carried reserve representation matches inventory");
        if (answered)
        {
            // Authored lamps may belong to both the general supply and a reserve circuit.
            var reserveLights = Field<Light[]>(house, "hallLights").Concat(Field<Light[]>(house, "kitchenLights")).Concat(Field<Light[]>(house, "bedroomLights")).ToArray();
            Check(Field<Light[]>(house, "generalLights").Where(l => !reserveLights.Contains(l)).All(l => l && !l.enabled),
                "answered phone retains the loss of every general-only lamp");
            Check(LightState("hallLights").All(v => v == (location == HousePowerLocation.Hall)) &&
                LightState("kitchenLights").All(v => v == (location == HousePowerLocation.Kitchen)) &&
                LightState("bedroomLights").All(v => v == (location == HousePowerLocation.Bedroom)),
                "only the chosen reserve circuit is lit, or all are dark while carried");
        }
        else
            Check(LightState("generalLights").SequenceEqual(initialGeneral) && LightState("hallLights").SequenceEqual(initialHall) &&
                LightState("kitchenLights").SequenceEqual(initialKitchen) && LightState("bedroomLights").SequenceEqual(initialBedroom),
                "unanswered continuation preserves the authored general-light state rather than inventing a blackout");
    }

    bool[] LightState(string name)
    {
        var lights = Field<Light[]>(house, name);
        Check(lights.Length > 0 && lights.All(l => l && l.gameObject.activeInHierarchy), name + " has active authored fixtures");
        return lights.Select(l => l.enabled).ToArray();
    }
    static HousePowerLocation Location(string value) => value == "kitchen" ? HousePowerLocation.Kitchen :
        value == "bedroom" ? HousePowerLocation.Bedroom : value == "carried" ? HousePowerLocation.Carried : HousePowerLocation.Hall;
    static float HorizontalDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    static bool SameFrame(EchoFrame a, EchoFrame b) => a.Timestamp == b.Timestamp && a.Position.Equals(b.Position) && a.Rotation.Equals(b.Rotation);
    static T Field<T>(object target, string name) => (T)(target.GetType().GetField(name, Private)
        ?? throw new MissingFieldException(target.GetType().Name, name)).GetValue(target);
    static Preference ReadPreference(string key) => new Preference { key = key, exists = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetInt(key) };
    static bool PreferenceUnchanged(Preference p) => PlayerPrefs.HasKey(p.key) == p.exists && (!p.exists || PlayerPrefs.GetInt(p.key) == p.value);

    void ConfigureDevices()
    {
        previousDevices = playerInput.devices.ToArray(); previousScheme = playerInput.currentControlScheme;
        previousAutoSwitch = playerInput.neverAutoSwitchControlSchemes;
        keyboard = InputSystem.AddDevice<Keyboard>("ThresholdCheckpointKeyboard"); mouse = InputSystem.AddDevice<Mouse>("ThresholdCheckpointMouse");
        devicesConfigured = true; expectedPause = true;
        playerInput.neverAutoSwitchControlSchemes = true;
        playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }

    void InputUpdate()
    {
        if (!running || !devicesConfigured || !session || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        Key[] held = keys;
        if (session.IsPaused && !expectedPause && !session.IsEnding)
        {
            if (!resumeDown) focusResumes++;
            resumeDown = !resumeDown; held = resumeDown ? new[] { Key.Enter } : Array.Empty<Key>();
        }
        else resumeDown = false;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(held));
        InputSystem.QueueStateEvent(mouse, new MouseState());
    }
    IEnumerator Wait(float seconds) { keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(seconds); CheckRuntime(); }
    IEnumerator Hold(Key key, float seconds)
    {
        keys = new[] { key }; yield return new WaitForSecondsRealtime(seconds);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.10f); CheckRuntime();
    }
    IEnumerator Press(Key key) { yield return Hold(key, .08f); }
    void ReceiveLog(string message, string stack, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message + "\n" + stack; }
    void CheckRuntime() { if (!string.IsNullOrEmpty(runtimeError)) throw new InvalidOperationException("Unity runtime error: " + runtimeError); }
    void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(step + ": " + message);
        checks++; File.AppendAllText(output + ".checks.txt", "PASS [" + step + "] " + message + "\n");
    }

    void ReleaseDevices()
    {
        keys = Array.Empty<Key>(); InputSystem.onBeforeUpdate -= InputUpdate;
        if (!devicesConfigured) return;
        devicesConfigured = false;
        if (playerInput)
        {
            playerInput.neverAutoSwitchControlSchemes = previousAutoSwitch;
            var usable = previousDevices.Where(d => d != null && d.added).ToArray();
            if (!string.IsNullOrEmpty(previousScheme) && usable.Length > 0)
                try { playerInput.SwitchCurrentControlScheme(previousScheme, usable); }
                catch (Exception exception) { Debug.LogWarning("Checkpoint probe input restoration: " + exception.Message); }
        }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }

    void Cleanup()
    {
        if (cleanupDone) return; cleanupDone = true; running = false;
        Application.logMessageReceived -= ReceiveLog;
        if (session) session.SetPaused(true);
        ReleaseDevices();
        if (!preferenceBackupReady) return;
        InputSystem.settings.editorInputBehaviorInPlayMode = oldInputBehavior;
        foreach (var pref in preferences)
            if (pref.exists) PlayerPrefs.SetInt(pref.key, pref.value); else PlayerPrefs.DeleteKey(pref.key);
        PlayerPrefs.Save();
        if (campaign) campaign.SuppressSaves = oldSuppressSaves;
    }

    void Finish(bool success, string error)
    {
        if (Status.StartsWith("PASS") || Status.StartsWith("FAIL")) return;
        Cleanup();
        bool preferencesRestored = preferenceBackupReady && preferences.All(PreferenceUnchanged) && legacyPreferences.All(PreferenceUnchanged);
        bool suppressionRestored = campaign && campaign.SuppressSaves == oldSuppressSaves;
        if (success && (!preferencesRestored || !suppressionRestored)) { success = false; error = "Probe cleanup failed to restore prior preferences or SuppressSaves."; }
        Status = success ? "PASS" : "FAIL " + step + ": " + error;
        File.AppendAllText(output + ".checks.txt", Status + "\n");
        File.WriteAllText(output + ".result.json", JsonUtility.ToJson(new Result
        {
            status = success ? "PASS" : "FAIL", mode = mode, scene = scenePath, lastStep = step, error = error,
            completedCases = completedCases.ToArray(), checks = checks, sceneReloads = reloads, explicitFixturePlacements = fixturePlacements,
            unexpectedFocusResumes = focusResumes, elapsedRealtime = Time.realtimeSinceStartup - startedAt,
            preferencesRestored = preferencesRestored, suppressSavesRestored = suppressionRestored,
            scope = "Isolated checkpoint verification: public APIs earn each saved domestic state; fresh scene reloads exercise the real Continue APIs using PlayerPrefs. " +
                "The four reserve locations and optional unanswered/general control are separate fixtures. Synthetic movement/R verifies resumed recording. " +
                "No prior campaign play, final-route completion, human fear, visual quality or listening claim. Original preferences are restored; the last fixture is left paused."
        }, true));
        if (success) Debug.Log("Threshold checkpoint " + mode + " PASS"); else Debug.LogWarning(Status);
    }
    void OnDestroy() { Cleanup(); }
}
#endif

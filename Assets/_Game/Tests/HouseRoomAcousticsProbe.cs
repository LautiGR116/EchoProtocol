#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// One labelled previous-chapter/lighting/pose fixture, then actual Input System
// movement, view, E and Escape. No audio setters or playback simulation.
[DefaultExecutionOrder(500)]
public sealed class HouseRoomAcousticsProbe : MonoBehaviour
{
    const string Output = "Docs/VisualEvidence/2026-10-01-Rooms/house-acoustics";
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    readonly List<string> checks = new List<string>();
    readonly HashSet<int> transitionValues = new HashSet<int>();
    HouseRoomAcoustics acoustics; HouseMemory house; HouseTelephone handset;
    CampaignDirector campaign; GameSession session; FirstPersonController player;
    Camera view; PlayerInput input; PlayerInteractor interactor; LoopController loop; EchoRecorder recorder;
    AudioSource telephone, refrigerator; AudioLowPassFilter telephoneFilter, refrigeratorFilter;
    SourceSnapshot telephoneBefore, refrigeratorBefore;
    Keyboard keyboard; Mouse mouse; InputDevice[] previousDevices; string previousScheme;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    bool previousAutoSwitch, oldSuppress, fixtureStarted, configured, running, finished, expectedPause, pausedMidTransition;
    Key[] keys = Array.Empty<Key>(); float yaw = 180f, pitch, sensitivity, started, masterBefore;
    string step = "initial_fixture", runtimeError; int poseResets, fixtureCount;
    StreamWriter trace;
    public string Status { get; private set; } = "starting";

    sealed class SourceSnapshot
    {
        public AudioClip clip;
        public UnityEngine.Audio.AudioMixerGroup mixer;
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public float volume, pitch, blend, near, far, doppler;
        public bool loop, playOnAwake, ignorePause, bypassEffects;
        public AudioRolloffMode rolloff;
        public SourceSnapshot(AudioSource source)
        {
            clip = source.clip; mixer = source.outputAudioMixerGroup; parent = source.transform.parent;
            position = source.transform.position; rotation = source.transform.rotation;
            volume = source.volume; pitch = source.pitch; blend = source.spatialBlend;
            near = source.minDistance; far = source.maxDistance; doppler = source.dopplerLevel;
            loop = source.loop; playOnAwake = source.playOnAwake; ignorePause = source.ignoreListenerPause;
            bypassEffects = source.bypassEffects; rolloff = source.rolloffMode;
        }
        public bool Matches(AudioSource source) => source && source.clip == clip && source.outputAudioMixerGroup == mixer
            && source.transform.parent == parent && Vector3.Distance(source.transform.position, position) < .0001f
            && Quaternion.Angle(source.transform.rotation, rotation) < .001f
            && Mathf.Abs(source.volume - volume) < .00001f && Mathf.Abs(source.pitch - pitch) < .00001f
            && Mathf.Approximately(source.spatialBlend, blend) && Mathf.Approximately(source.minDistance, near)
            && Mathf.Approximately(source.maxDistance, far) && Mathf.Approximately(source.dopplerLevel, doppler)
            && source.loop == loop && source.playOnAwake == playOnAwake && source.ignoreListenerPause == ignorePause
            && source.bypassEffects == bypassEffects && source.rolloffMode == rolloff;
    }

    public static void Run()
    {
        if (!Application.isPlaying) throw new Exception("Run from fresh Play in Rooms");
        if (FindFirstObjectByType<HouseRoomAcousticsProbe>()) throw new Exception("A Rooms acoustic probe already exists; use fresh Play");
        var probe = new GameObject("Temporary Rooms acoustic route").AddComponent<HouseRoomAcousticsProbe>();
        probe.StartCoroutine(probe.Guarded(probe.Execute()));
    }

    static T Field<T>(object target, string name) => (T)(target.GetType().GetField(name, Private)
        ?? throw new MissingFieldException(target.GetType().Name, name)).GetValue(target);
    void Check(bool value, string message) { if (!value) throw new Exception(message); checks.Add("PASS " + message); }
    IEnumerator Guarded(IEnumerator sequence)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(sequence);
        while (stack.Count > 0)
        {
            object next;
            try { if (!stack.Peek().MoveNext()) { stack.Pop(); continue; } next = stack.Peek().Current; }
            catch (Exception error) { Finish(false, error.ToString()); yield break; }
            if (next is IEnumerator child) stack.Push(child); else yield return next;
        }
    }

    IEnumerator Execute()
    {
        started = Time.realtimeSinceStartup; Directory.CreateDirectory(Output);
        // Only remove files belonging to this isolated probe; do not mix stale
        // cameras with a failed/new attempt or touch any campaign evidence.
        foreach (var file in Directory.GetFiles(Output, "rooms-*.png")) File.Delete(file);
        File.Delete(Output + "/result.txt");
        File.WriteAllText(Output + "/cameras.txt", "Camera renders omit OnGUI. One initial fixture, then physical input only. No listening claim.\n");
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.EndsWith("EchoProtocol_Rooms.unity"), "probe uses the opt-in Rooms successor");
        acoustics = FindFirstObjectByType<HouseRoomAcoustics>(); house = FindFirstObjectByType<HouseMemory>();
        handset = FindFirstObjectByType<HouseTelephone>(); campaign = FindFirstObjectByType<CampaignDirector>();
        session = FindFirstObjectByType<GameSession>(); player = FindFirstObjectByType<FirstPersonController>();
        Check(acoustics && acoustics.enabled && house && handset && campaign && session && player, "required actual runtime components exist");
        view = player.GetComponentInChildren<Camera>(); input = player.GetComponent<PlayerInput>();
        interactor = player.GetComponent<PlayerInteractor>(); loop = FindFirstObjectByType<LoopController>(); recorder = player.GetComponent<EchoRecorder>();
        Check(!house.Active && !house.HasAnswered && !house.HasKey && !house.HasExited && !handset.IsOffHook, "fresh fixture begins before all domestic interactions");
        telephone = Field<AudioSource>(house, "telephoneAudio"); refrigerator = Field<AudioSource>(house, "kitchenApplianceAudio");
        telephoneFilter = Field<AudioLowPassFilter>(acoustics, "telephoneFilter"); refrigeratorFilter = Field<AudioLowPassFilter>(acoustics, "refrigeratorFilter");
        Check(Field<AudioSource>(acoustics, "telephoneSource") == telephone && Field<AudioSource>(acoustics, "refrigeratorSource") == refrigerator
            && telephone != refrigerator && telephoneFilter.gameObject == telephone.gameObject && refrigeratorFilter.gameObject == refrigerator.gameObject,
            "treatment references the two actual owner sources and matching filters");
        Check(Field<Transform>(acoustics, "listener") == view.transform && Field<GameSession>(acoustics, "session") == session,
            "treatment uses the actual Player camera and session");
        Check(Field<float>(acoustics, "clearCutoffHz") == 22000f && Field<float>(acoustics, "obstructedCutoffHz") == 1600f
            && Mathf.Approximately(Field<float>(acoustics, "transitionSeconds"), .20f), "authored provisional filter settings are 22000/1600 Hz and 0.20 s");
        var partitions = Field<Collider[]>(acoustics, "architecturalPartitions");
        Check(partitions.Length == 47 && partitions.Distinct().Count() == 47
            && partitions.All(c => c && c.enabled && !c.isTrigger && c is BoxCollider && c.gameObject.activeInHierarchy),
            "47 distinct existing architectural BoxColliders are explicitly assigned");

        oldSuppress = campaign.SuppressSaves; campaign.SuppressSaves = true; fixtureStarted = true;
        // SOLE FIXTURE: a completed prior act and physical placement at the house
        // entrance. No fixture sets house progress, source state or filter values.
        session.BeginOrResume(); campaign.PrepareHouseProbe();
        var firstAct = FindFirstObjectByType<VS1StrongEventOne>();
        Check(firstAct && firstAct.RestoreCompletedLighting() && firstAct.FluorescentPowerFraction == 0f,
            "explicit initial fixture restores completed first-act lighting");
        player.ResetPose(new Vector3(0, .03f, -49.5f), Quaternion.Euler(0, 180, 0)); fixtureCount++;
        Configure(); player.PoseReset += ObservePoseReset; Application.logMessageReceived += ReceiveLog;
        running = true; Status = "running";
        trace = new StreamWriter(Output + "/filter.csv");
        trace.WriteLine("frame,realtime,simulation,step,eye_x,eye_y,eye_z,yaw,pitch,phone_blocked,phone_hz,fridge_blocked,fridge_hz,phone_playing,fridge_playing,phone_volume,fridge_volume,paused,answered");
        yield return Wait(.25f);
        step = "actual_house_entry";
        yield return UseTarget(FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s => s.Kind == StationKind.HouseOrigin));
        Check(house.Active && campaign.Chapter == CampaignChapter.House && !loop.enabled && !recorder.IsRecording,
            "actual E enters the house and its owner suspends recording");
        Check(refrigerator.isPlaying && !house.HasAnswered && house.PowerLocation == HousePowerLocation.Hall,
            "initial general supply powers the actual refrigerator without a probe playback call");
        telephoneBefore = new SourceSnapshot(telephone); refrigeratorBefore = new SourceSnapshot(refrigerator); masterBefore = AudioListener.volume;

        step = "hall_phone_blocked"; yield return Go(0, -54f, 180); yield return Wait(.3f);
        ExpectPhone(true); Capture("01-hall-wall");
        step = "phone_portal_transition"; yield return WalkPhonePortalWithPause();
        yield return Wait(.3f); ExpectPhone(false);
        Check(transitionValues.Count >= 2, "actual approach produced at least two distinct intermediate cutoff samples, not an endpoint jump");
        Check(pausedMidTransition, "real Escape froze and resumed an intermediate filter transition");
        Capture("03-hall-open-path");

        step = "inside_parlor"; yield return Go(-3f, -52.35f, 270); yield return Wait(.3f);
        ExpectPhone(false); Capture("04-inside-parlor");
        yield return Go(0, -52.35f, 90);
        step = "hall_between_rooms"; yield return Go(0, -56f, 180); yield return Wait(.3f);
        ExpectPhone(true); ExpectRefrigerator(true); Capture("05-hall-both-walls");
        step = "kitchen_portal"; yield return Go(0, -58.2f, 180); yield return Wait(.3f);
        ExpectRefrigerator(false); yield return Face(270); Capture("06-kitchen-open-path");
        step = "inside_kitchen"; yield return Go(-3.5f, -58.2f, 270); yield return Wait(.3f);
        ExpectRefrigerator(false); ExpectPhone(true); Capture("07-inside-kitchen");
        Check(refrigerator.isPlaying, "owner keeps the refrigerator running throughout the powered traversal");
        step = "return_hall"; yield return Go(0, -58.2f, 90); yield return Go(0, -56f, 0); yield return Wait(.3f);
        ExpectPhone(true); ExpectRefrigerator(true); Capture("08-return-obstructed");

        step = "return_to_phone"; yield return Go(0, -52.35f, 0); yield return Go(-3f, -52.35f, 270); yield return Go(-3.45f, -51.9f, 270);
        var interaction = FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(i => i.Kind == HouseInteractionKind.Telephone);
        yield return Aim(TargetPoint(interaction)); yield return Wait(.3f); ExpectPhone(false);
        CheckTarget(interaction);
        // Observe a fresh owner-scheduled ring onset so E demonstrably interrupts
        // ringing. Never invoke Play/Stop or edit the ring timer from the probe.
        yield return Until(() => !telephone.isPlaying, 10f, "Current telephone ring did not end");
        yield return Until(() => telephone.isPlaying, 12f, "Owner did not schedule the next unanswered ring");
        Check(!house.HasAnswered && refrigerator.isPlaying, "real ring onset occurs while the house is unanswered and kitchen powered");
        Capture("09-ringing-before-answer");
        step = "actual_answer"; keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.06f);
        Check(telephone.isPlaying, "telephone is still ringing immediately before the actual E press");
        keys = new[] { Key.E }; yield return new WaitForSecondsRealtime(.10f);
        keys = Array.Empty<Key>(); yield return Wait(.8f);
        Check(house.HasAnswered && handset.IsOffHook && !handset.IsMoving && !telephone.isPlaying && !refrigerator.isPlaying,
            "actual E stops the ring and kitchen supply through their existing owner, with receiver off hook");
        Check(house.PowerLocation == HousePowerLocation.Hall && !house.HasKey && !house.HasExited,
            "answer changes no reserve placement, key or exit state");
        Check(!Field<Light[]>(house, "kitchenLights").Any(l => l && l.enabled), "the same owner consequence removes kitchen light power");
        AssertSourceOwnership(); Check(true, "every observed frame preserved source anchors, references, authored audio settings and master volume");
        Check(fixtureCount == 1 && poseResets == 0, "entire traversal used one initial fixture and no later pose reset");
        Capture("10-owner-power-off");
        Finish(true, "");
    }

    IEnumerator WalkPhonePortalWithPause()
    {
        yield return Face(0); float deadline = Time.realtimeSinceStartup + 12f;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(0, -52.35f)) > .10f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Portal transition route blocked");
            if (!pausedMidTransition && acoustics.TelephoneCutoffHz > 1601f && acoustics.TelephoneCutoffHz < 21999f)
            {
                yield return PauseDuringTransition();
                continue;
            }
            MoveToward(0, -52.35f); yield return null;
        }
        keys = Array.Empty<Key>();
        if (!pausedMidTransition) throw new Exception("No intermediate sample available for real pause; rerun with adequate frame cadence. Mid-transition pause is NOT verified.");
    }

    IEnumerator PauseDuringTransition()
    {
        expectedPause = true; keys = new[] { Key.Escape };
        float deadline = Time.realtimeSinceStartup + 2f;
        while (!session.IsPaused)
        { if (Time.realtimeSinceStartup > deadline) throw new Exception("Escape did not open pause"); yield return null; }
        keys = Array.Empty<Key>();
        float phoneHz = acoustics.TelephoneCutoffHz, fridgeHz = acoustics.RefrigeratorCutoffHz, at = Time.time;
        Vector3 pose = player.transform.position;
        Check(phoneHz > 1601f && phoneHz < 21999f && AudioListener.pause, "actual menu pause catches an intermediate cutoff and pauses listener audio");
        keys = new[] { Key.W, Key.R }; yield return new WaitForSecondsRealtime(.35f); keys = Array.Empty<Key>();
        Check(Mathf.Abs(phoneHz - acoustics.TelephoneCutoffHz) < .01f && Mathf.Abs(fridgeHz - acoustics.RefrigeratorCutoffHz) < .01f
            && Mathf.Approximately(Time.time, at) && Vector3.Distance(pose, player.transform.position) < .01f && poseResets == 0,
            "paused W/R cannot advance the filters, simulation clock, Player or recording origin");
        Capture("02-paused-intermediate");
        yield return Press(Key.Escape);
        Check(!session.IsPaused && player.enabled && !AudioListener.pause, "second real Escape restores control and listener without automatic resume");
        expectedPause = false; pausedMidTransition = true;
    }

    void ExpectPhone(bool blocked) => Check(acoustics.TelephoneObstructed == blocked
        && Mathf.Abs(acoustics.TelephoneCutoffHz - (blocked ? 1600f : 22000f)) < 2f, step + ": telephone settles at the expected geometric endpoint");
    void ExpectRefrigerator(bool blocked) => Check(acoustics.RefrigeratorObstructed == blocked
        && Mathf.Abs(acoustics.RefrigeratorCutoffHz - (blocked ? 1600f : 22000f)) < 2f, step + ": refrigerator settles at the expected geometric endpoint");
    void AssertSourceOwnership()
    {
        if (!telephoneBefore.Matches(telephone) || !refrigeratorBefore.Matches(refrigerator)
            || Field<AudioSource>(house, "telephoneAudio") != telephone || Field<AudioSource>(house, "kitchenApplianceAudio") != refrigerator
            || Field<AudioSource>(acoustics, "telephoneSource") != telephone || Field<AudioSource>(acoustics, "refrigeratorSource") != refrigerator
            || Field<AudioLowPassFilter>(acoustics, "telephoneFilter") != telephoneFilter || Field<AudioLowPassFilter>(acoustics, "refrigeratorFilter") != refrigeratorFilter
            || Mathf.Abs(AudioListener.volume - masterBefore) > .00001f)
            throw new Exception("A source reference, anchor, authored audio setting or master volume changed during the acoustic traversal");
    }
    void ObservePoseReset() { poseResets++; }
    void ReceiveLog(string message, string stack, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message + "\n" + stack; }
    void LateUpdate()
    {
        if (!running) return;
        try
        {
            if (!string.IsNullOrEmpty(runtimeError)) throw new Exception(runtimeError);
            if (Time.realtimeSinceStartup - started > 100f) throw new Exception("Acoustic route exceeded 100 seconds");
            if (session.IsPaused && !expectedPause) throw new Exception("Unexpected pause/focus loss; no cursor override or automatic resume");
            if (player.transform.position.y < -.2f || poseResets != 0) throw new Exception("Floor support or physical pose continuity failed");
            if (telephoneBefore != null) AssertSourceOwnership();
            if (step == "phone_portal_transition" && !session.IsPaused && acoustics.TelephoneCutoffHz > 1601f && acoustics.TelephoneCutoffHz < 21999f)
                transitionValues.Add(Mathf.RoundToInt(acoustics.TelephoneCutoffHz));
            if (trace == null) return;
            var p = view.transform.position;
            trace.WriteLine(string.Join(",", Time.frameCount, F(Time.realtimeSinceStartup), F(Time.time), step,
                F(p.x), F(p.y), F(p.z), F(player.transform.eulerAngles.y), F(Field<float>(player, "pitch")),
                acoustics.TelephoneObstructed, F(acoustics.TelephoneCutoffHz), acoustics.RefrigeratorObstructed, F(acoustics.RefrigeratorCutoffHz),
                telephone.isPlaying, refrigerator.isPlaying, F(telephone.volume), F(refrigerator.volume), session.IsPaused, house.HasAnswered));
            trace.Flush();
        }
        catch (Exception error) { Finish(false, error.ToString()); StopAllCoroutines(); }
    }
    static string F(float value) => value.ToString("F4", Invariant);

    IEnumerator Go(float x, float z, float facing)
    {
        yield return Face(facing); float deadline = Time.realtimeSinceStartup + 12f;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(x, z)) > .1f)
        { if (Time.realtimeSinceStartup > deadline) throw new Exception("Blocked at " + player.transform.position + " toward " + x + "," + z); MoveToward(x, z); yield return null; }
        yield return Wait(.12f);
    }
    void MoveToward(float x, float z)
    {
        Vector3 local = player.transform.InverseTransformDirection(new Vector3(x, player.transform.position.y, z) - player.transform.position);
        var held = new List<Key>();
        if (local.z > .06f) held.Add(Key.W); else if (local.z < -.06f) held.Add(Key.S);
        if (local.x > .06f) held.Add(Key.D); else if (local.x < -.06f) held.Add(Key.A);
        keys = held.ToArray();
    }
    IEnumerator Face(float targetYaw, float targetPitch = 0)
    {
        yaw = targetYaw; pitch = Mathf.Clamp(targetPitch, -84f, 84f); keys = Array.Empty<Key>();
        float deadline = Time.realtimeSinceStartup + 5f;
        while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f || Mathf.Abs(Field<float>(player, "pitch") - pitch) > .6f)
        { if (Time.realtimeSinceStartup > deadline) throw new Exception("Mouse aim failed; keep Game view focused. No cursor-state override."); yield return null; }
        yield return Wait(.10f);
    }
    IEnumerator Aim(Vector3 target)
    {
        Vector3 d = target - view.transform.position;
        yield return Face(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg);
    }
    Vector3 TargetPoint(Component component)
    {
        var collider = component.GetComponentsInChildren<Collider>().Where(c => c.enabled && c.gameObject.activeInHierarchy && !c.isTrigger)
            .OrderBy(c => Vector3.Distance(c.bounds.center, view.transform.position)).FirstOrDefault();
        return collider ? collider.bounds.center : component.transform.position;
    }
    void CheckTarget(Component component)
    {
        var expected = component as IInteractable;
        Check(expected != null && interactor.TryGetTarget(out var target) && ReferenceEquals(target, expected)
            && expected.CanInteract(player.gameObject), "actual center-camera ray and owner authorize " + component.name);
    }
    IEnumerator UseTarget(Component component) { yield return Aim(TargetPoint(component)); CheckTarget(component); yield return Press(Key.E); }
    IEnumerator Press(Key key)
    {
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.06f);
        keys = new[] { key }; yield return new WaitForSecondsRealtime(.10f);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.12f);
    }
    IEnumerator Wait(float duration) { keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(duration); }
    IEnumerator Until(Func<bool> condition, float seconds, string error)
    {
        keys = Array.Empty<Key>(); float deadline = Time.realtimeSinceStartup + seconds;
        while (!condition()) { if (Time.realtimeSinceStartup > deadline) throw new Exception(error); yield return null; }
    }
    void Configure()
    {
        previousDevices = input.devices.ToArray(); previousScheme = input.currentControlScheme; previousAutoSwitch = input.neverAutoSwitchControlSchemes;
        oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode; sensitivity = Field<float>(player, "lookSensitivity");
        Check(sensitivity > 0, "actual mouse sensitivity is positive");
        keyboard = InputSystem.AddDevice<Keyboard>("RoomsRouteKeyboard"); mouse = InputSystem.AddDevice<Mouse>("RoomsRouteMouse");
        configured = true; input.neverAutoSwitchControlSchemes = true; input.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }
    void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { delta = session.IsPaused ? Vector2.zero : new Vector2(
            Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12f, 12f) / sensitivity,
            -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10f, 10f) / sensitivity) });
    }
    void Capture(string label)
    {
        var old = view.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(1280, 720, 24); var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            view.targetTexture = rt; view.Render(); RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
            File.WriteAllBytes(Output + "/rooms-" + label + ".png", texture.EncodeToPNG());
            File.AppendAllText(Output + "/cameras.txt", label + " eye=" + view.transform.position.ToString("F3")
                + " euler=" + view.transform.eulerAngles.ToString("F2") + " phone=" + F(acoustics.TelephoneCutoffHz)
                + "Hz/blocked=" + acoustics.TelephoneObstructed + " fridge=" + F(acoustics.RefrigeratorCutoffHz)
                + "Hz/blocked=" + acoustics.RefrigeratorObstructed + " paused=" + session.IsPaused + "\n");
        }
        finally { view.targetTexture = old; RenderTexture.active = active; Destroy(rt); Destroy(texture); }
    }
    void Finish(bool pass, string error)
    {
        if (finished) return; finished = true; Status = pass ? "PASS" : "FAIL " + error;
        Cleanup();
        File.WriteAllText(Output + "/checks.txt", "One explicit initial prior-act/lighting/pose fixture; subsequent real synthetic Input System route/E/Escape. No full campaign, audition, human navigation, fear or comfort claim. Final cleanup intentionally pauses the session.\n"
            + string.Join("\n", checks) + "\n" + Status);
        File.WriteAllText(Output + "/result.txt", Status + "\nchecks=" + checks.Count + " fixtureCount=" + fixtureCount
            + " laterPoseResets=" + poseResets + " midTransitionPause=" + pausedMidTransition + " intermediateValues=" + transitionValues.Count
            + " elapsed=" + F(Time.realtimeSinceStartup - started));
        Debug.Log("Rooms acoustic route " + Status);
    }
    void Cleanup()
    {
        running = false; keys = Array.Empty<Key>(); InputSystem.onBeforeUpdate -= InputUpdate;
        Application.logMessageReceived -= ReceiveLog; if (player) player.PoseReset -= ObservePoseReset;
        trace?.Dispose(); trace = null;
        if (fixtureStarted) { fixtureStarted = false; if (campaign) campaign.SuppressSaves = oldSuppress; if (session) session.SetPaused(true); }
        if (!configured) return; configured = false; InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
        if (input) { input.neverAutoSwitchControlSchemes = previousAutoSwitch; if (previousDevices.Length > 0 && !string.IsNullOrEmpty(previousScheme)) input.SwitchCurrentControlScheme(previousScheme, previousDevices); }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }
    void OnDestroy() { Cleanup(); }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>Isolated final-act evidence. Public house APIs prepare prior state once; all subsequent actions use input devices.</summary>
public sealed class FinalDoorRouteProbe : MonoBehaviour
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    FirstPersonController player;
    PlayerInteractor interactor;
    PlayerInput playerInput;
    EchoRecorder recorder;
    LoopController loop;
    GameSession session;
    CampaignDirector campaign;
    HouseMemory house;
    FinalDoorMemory review;
    HouseSwingDoor domesticDoor;
    SlidingDoor exitDoor;
    Camera view;
    Keyboard keyboard;
    Mouse mouse;
    InputDevice[] originalDevices;
    string originalScheme;
    bool originalAutoSwitch, oldSuppressSaves, saveCaptured, devicesReady, cleanedUp, running, expectedPause, resumeDown;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    Key[] keys = Array.Empty<Key>();
    float yaw, pitch, sensitivity, nextLog, startedAt;
    string mode, step = "fixture", output;
    int checks, fixtureCount, playbackFrames, focusResumes;
    StreamWriter log;
    readonly List<string> screenshots = new List<string>();
    readonly List<Snapshot> snapshots = new List<Snapshot>();
    public string Status { get; private set; } = "starting";
    public string CurrentStep => step;

    sealed class Snapshot { public EchoRecording recording; public EchoFrame[] frames; }
    [Serializable] sealed class Result
    {
        public string mode, status, scene, lastStep, scope, error;
        public int checks, initialPoseFixtures, immutableSnapshots, playbackFramesChecked, unexpectedPauseResumes;
        public float elapsedRealtime;
        public bool witnessed, preserved, keyReturned, complete;
        public string[] screenshots;
    }

    public static void Run(string mode = "full")
    {
        if (!new[] { "full", "visual", "rules", "wrongfacing", "short" }.Contains(mode))
            throw new ArgumentException("FinalDoorRouteProbe modes: full, visual, rules, wrongfacing, short. Each requires fresh Play Mode.");
        var old = FindObjectsByType<FinalDoorRouteProbe>(FindObjectsSortMode.None);
        if (old.Any(p => p.running)) throw new InvalidOperationException("A final-door route is already running.");
        foreach (var probe in old) Destroy(probe.gameObject);
        var instance = new GameObject("Temporary final-door input verification").AddComponent<FinalDoorRouteProbe>();
        instance.mode = mode;
        instance.output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/VisualEvidence/2026-10-01-FinalDoor/"));
        Directory.CreateDirectory(instance.output);
        instance.StartCoroutine(instance.Guarded(instance.Execute()));
    }

    static T Field<T>(object owner, string name)
    {
        var field = owner.GetType().GetField(name, Private);
        if (field == null) throw new MissingFieldException(owner.GetType().Name, name);
        return (T)field.GetValue(owner);
    }

    IEnumerator Guarded(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(routine);
        while (stack.Count > 0)
        {
            object yielded = null; bool more = false; Exception error = null;
            try { more = stack.Peek().MoveNext(); if (more) yielded = stack.Peek().Current; }
            catch (Exception exception) { error = exception; }
            if (error != null) { Finish(false, error.Message); yield break; }
            if (!more) { stack.Pop(); continue; }
            if (yielded is IEnumerator nested) { stack.Push(nested); continue; }
            yield return yielded;
        }
    }

    IEnumerator Execute()
    {
        yield return null;
        startedAt = Time.realtimeSinceStartup;
        File.WriteAllText(PathFor("checks.txt"), "ISOLATED FINAL ACT: initial public-API house fixture and one pose placement; synthetic input thereafter.\n");
        File.WriteAllText(PathFor("cameras.txt"), "Actual Player camera; offscreen Camera renders omit OnGUI.\n");
        File.Delete(PathFor("result.json")); File.Delete(PathFor("result.txt"));
        log = new StreamWriter(PathFor("csv"));
        log.WriteLine("realtime,time,step,x,y,z,yaw,pitch,recalls,echoTime,qualified,witnessProgress,witnessed,preserved,keyReturned,fade,paused");
        player = FindFirstObjectByType<FirstPersonController>(); campaign = FindFirstObjectByType<CampaignDirector>();
        house = FindFirstObjectByType<HouseMemory>(); review = FindFirstObjectByType<FinalDoorMemory>();
        session = FindFirstObjectByType<GameSession>(); loop = FindFirstObjectByType<LoopController>();
        Check(player && campaign && house && review && session && loop, "required final-act and house systems exist");
        interactor = player.GetComponent<PlayerInteractor>(); playerInput = player.GetComponent<PlayerInput>(); recorder = player.GetComponent<EchoRecorder>();
        view = Field<Camera>(review, "view"); domesticDoor = Field<HouseSwingDoor>(review, "domesticDoor"); exitDoor = Field<SlidingDoor>(review, "exitDoor");
        Check(interactor && playerInput && recorder && view && domesticDoor && exitDoor, "physical Player, view and door references exist");
        Check(!review.Active && !review.HasWitnessed && !review.OriginalPreserved && !review.KeyReturned && !review.IsComplete, "final act begins fresh");
        oldSuppressSaves = campaign.SuppressSaves; saveCaptured = true; campaign.SuppressSaves = true;
        // Explicit fixture only. These mutations do not claim that the earlier house was played.
        session.BeginOrResume(); campaign.PrepareHouseProbe(); house.EnterHouse();
        house.Use(HouseInteractionKind.Key, player.gameObject, Field<GameObject>(house, "keyWorld").transform.position);
        house.Use(HouseInteractionKind.ExitDoor, player.gameObject, Field<HouseSwingDoor>(house, "exitDoor").transform.position);
        Check(house.HasKey && house.HasExited, "public house fixture acquires the key and unlocks its exit without reflection writes");
        player.ResetPose(new Vector3(0, .03f, -75), Quaternion.Euler(0, 180, 0)); fixtureCount++;
        ConfigureDevices(); running = true; Status = "running"; yaw = 180; pitch = 0;
        yield return Wait(.2f);
        step = "commit_final_origin";
        Vector3 before = player.transform.position; int oldRevision = loop.CheckpointRevision;
        yield return Use(FinalDoorInteractionKind.BeginReview);
        Check(review.Active && !house.Active && loop.enabled && recorder.IsRecording, "real E transfers from house to active review");
        Check(Vector3.Distance(before, player.transform.position) < .03f && loop.CheckpointRevision == oldRevision + 1,
            "review commits exactly one checkpoint without teleporting the Player");
        Check(Vector3.Distance(loop.RecallSpawnPosition, new Vector3(0, .03f, -76)) < .1f && loop.LastRecording == null && loop.ActiveEcho == null,
            "interior origin clears obsolete recordings instead of inventing a take");
        Check(!domesticDoor.IsOpenRequested && !exitDoor.IsOpenRequested && !Field<GameObject>(review, "keyInTray").activeInHierarchy,
            "domestic leaf, final exit and return tray begin in their honest initial states");
        CheckEarlyGuards();
        yield return Capture("00-origin");

        if (mode == "wrongfacing" || mode == "rules")
        {
            step = "wrong_facing_take";
            yield return RecordApproach(180, 3.1f);
            Check(review.QualifiedPauseCount == 0 && !review.HasWitnessed, "a stationary take facing away from 6B has no qualifying pause");
            yield return ObserveRejectedTake();
            if (mode == "wrongfacing") { yield return CompleteEvidence(); yield break; }
        }
        if (mode == "short" || mode == "rules")
        {
            step = "short_take";
            yield return RecordApproach(0, .25f);
            Check(review.QualifiedPauseCount == 0 && !review.HasWitnessed, "a correctly facing but sub-two-second take is rejected");
            yield return ObserveRejectedTake();
            if (mode == "short") { yield return CompleteEvidence(); yield break; }
        }

        step = "authentic_approach";
        yield return RecordApproach(0, 4.2f);
        Check(review.QualifiedPauseCount > 0 && ReferenceEquals(review.ReviewedRecording, loop.LastRecording), "actual immutable approach includes a qualified pause");
        yield return WitnessArrival(mode == "rules");
        EchoRecording firstWitness = review.WitnessedRecording;
        Check(ReferenceEquals(firstWitness, loop.LastRecording), "witness stores the exact currently replayed snapshot");
        yield return MenuPause(true);

        if (mode == "rules")
        {
            step = "recall_invalidates_witness";
            yield return Recall();
            Check(!ReferenceEquals(firstWitness, loop.LastRecording) && !review.HasWitnessed && review.WitnessedRecording == null,
                "new R replaces the take and invalidates the previous witness reference");
            CheckEarlyGuards();
            Check(!domesticDoor.IsOpenRequested && !domesticDoor.IsFullyOpen, "R restores the review door only after placing the Player at the safe interior origin");
            step = "replacement_authentic_take";
            yield return RecordApproach(0, 4.2f);
            yield return WitnessArrival(false);
            Check(!ReferenceEquals(firstWitness, review.WitnessedRecording), "replacement witness belongs to the newly recorded approach");
        }

        step = "cross_beside_recording";
        yield return Go(0, -81.8f, 180);
        Check(player.transform.position.z < -81.5f, "real CharacterController passes through 6B into the outer archive");
        EchoRecording witnessed = review.WitnessedRecording;
        yield return Use(FinalDoorInteractionKind.Preserve);
        Check(review.OriginalPreserved && ReferenceEquals(review.PreservedOriginal, witnessed) && ReferenceEquals(loop.LastRecording, witnessed),
            "preservation keeps the exact witnessed immutable object");
        Check(!loop.enabled && !recorder.IsRecording && !review.KeyReturned && !exitDoor.IsOpenRequested,
            "preservation stops new recording but requires the distinct key-return action");
        yield return Capture("05-original-preserved");
        Vector3 preservedPose = player.transform.position; int preservedLoops = loop.CompletedLoopCount;
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == preservedLoops && Vector3.Distance(player.transform.position, preservedPose) < .025f && ReferenceEquals(review.PreservedOriginal, witnessed),
            "R after preservation cannot replace the original or teleport the Player");
        step = "return_physical_key";
        yield return Go(0, -82.7f, 180);
        yield return Use(FinalDoorInteractionKind.ReturnKey);
        Check(review.KeyReturned && Field<GameObject>(review, "keyInTray").activeInHierarchy && exitDoor.IsOpenRequested,
            "real E returns the physical key and requests the final mechanical exit");
        Check(!review.HasStartedDeparture && review.Fade == 0f && !session.IsEnding, "key return alone does not complete the game");
        yield return Capture("06-key-returned");
        yield return Until(() => exitDoor.IsFullyOpen, 8, "final exit did not finish its physical movement");
        step = "physical_departure";
        yield return Go(0, -88, 180);
        Check(review.HasStartedDeparture && review.Fade > 0f && !review.IsComplete && player.enabled,
            "actual departure crossing starts the fade while retaining Player control");
        if (mode == "rules")
        {
            expectedPause = true; yield return Press(Key.Escape); Check(session.IsPaused, "departure can be paused with Escape");
            float fadeAtPause = review.Fade; yield return Wait(.25f);
            Check(Mathf.Approximately(fadeAtPause, review.Fade), "pause freezes the actual departure fade");
            yield return Press(Key.Escape); Check(!session.IsPaused, "departure resumes through the actual pause control"); expectedPause = false;
        }
        yield return Until(() => review.IsComplete, 8, "departure did not complete after the authored fade");
        Check(session.IsEnding && session.IsPaused && review.Fade >= .999f && ReferenceEquals(review.PreservedOriginal, witnessed),
            "physical departure reaches the real final menu while retaining the same original");
        yield return Capture("07-ending-menu");
        yield return CompleteEvidence();
    }

    IEnumerator RecordApproach(float facing, float hold)
    {
        yield return Go(0, -76, 0);
        yield return Go(-3.5f, -76, 270);
        yield return Go(-3.5f, -81.8f, 180);
        yield return Go(0, -81.8f, 90);
        yield return Go(0, -80.25f, 0);
        yield return Face(facing, 0);
        yield return Wait(hold);
        if (facing == 0 && hold > 2) Check(review.RecordingReady, "live pose guidance responds to the actual recorded pause");
        yield return Recall();
        Check(Vector3.Distance(player.transform.position, loop.RecallSpawnPosition) < .1f, "R returns through the real loop to the established interior origin");
    }

    IEnumerator Recall()
    {
        int count = loop.CompletedLoopCount;
        Check(recorder.HasRecordingStarted, "the upcoming R has an actual movement recording to finish");
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == count + 1 && loop.LastRecording != null && loop.ActiveEcho != null,
            "one real R produces exactly one immutable take and active playback");
        Track(loop.LastRecording);
    }

    IEnumerator ObserveRejectedTake()
    {
        yield return Go(0, -77.3f, 180);
        yield return Face(180, 0);
        yield return Until(() => loop.ActiveEcho == null || loop.ActiveEcho.IsComplete, loop.LastRecording.Duration + 4, "rejected take never reached its real end");
        Check(!review.HasWitnessed && !review.OriginalPreserved && !review.KeyReturned && !review.HasStartedDeparture,
            "an observed rejected take cannot authorize preservation, key return or ending");
        CheckEarlyGuards();
        yield return Capture(step + "-rejected");
    }

    IEnumerator WitnessArrival(bool interruptObservation)
    {
        yield return Go(0, -77.3f, 180);
        // For the partial-pause case, do not let the arrival itself silently finish the witness.
        yield return Face(interruptObservation ? 90 : 180, 0);
        var outside = Field<BoxCollider>(review, "outsideZone");
        yield return Until(() => loop.ActiveEcho && !loop.ActiveEcho.IsComplete && outside.bounds.Contains(loop.ActiveEcho.transform.position + Vector3.up),
            loop.LastRecording.Duration + 3f, "recorded Player did not reach the actual exterior bay");
        if (!interruptObservation) yield return Capture("01-recording-at-door");
        if (!domesticDoor.IsOpenRequested) yield return Use(FinalDoorInteractionKind.OpenDoor);
        Check(domesticDoor.IsOpenRequested, "the present Player can open 6B independently of witness authorization");
        yield return Face(180, 0);
        if (interruptObservation)
        {
            yield return Until(() => review.WitnessProgress > .02f || review.HasWitnessed, 5, "no actual visible witness interval began");
            Check(!review.HasWitnessed, "partial witness is captured before completion");
            expectedPause = true; yield return Press(Key.Escape);
            Check(session.IsPaused && review.WitnessProgress == 0f && !review.HasWitnessed, "pause breaks an incomplete contiguous witness interval");
            float echoAt = loop.ActiveEcho.ElapsedTime; yield return Wait(.25f);
            Check(Mathf.Approximately(echoAt, loop.ActiveEcho.ElapsedTime), "pause freezes actual playback during the interrupted observation");
            yield return Press(Key.Escape); Check(!session.IsPaused, "observation resumes through Escape"); expectedPause = false;
        }
        yield return Until(() => review.HasWitnessed, 10, "a real qualified exterior pause was not witnessed through the open door");
        Check(ReferenceEquals(review.WitnessedRecording, loop.LastRecording), "continuous visible body observation qualifies only the current immutable take");
        yield return Capture("02-witnessed-from-inside");
        yield return Until(() => domesticDoor.IsFullyOpen, 8, "domestic door did not reach the authored open angle");
    }

    IEnumerator MenuPause(bool alreadyWitnessed)
    {
        step = "keyboard_pause"; expectedPause = true;
        EchoRecording reference = review.WitnessedRecording;
        yield return Press(Key.Escape); Check(session.IsPaused, "Escape opens the final-act pause menu");
        Vector3 p = player.transform.position; float at = Time.time; int loops = loop.CompletedLoopCount;
        keys = new[] { Key.W, Key.R }; yield return new WaitForSecondsRealtime(.25f); keys = Array.Empty<Key>();
        Check(Mathf.Approximately(at, Time.time) && Vector3.Distance(p, player.transform.position) < .01f && loops == loop.CompletedLoopCount,
            "paused keyboard movement and R leave clock, pose and recording identity unchanged");
        yield return Capture("03-pause-menu");
        yield return Press(Key.Escape); Check(!session.IsPaused, "Escape restores final-act control"); expectedPause = false;
        if (alreadyWitnessed) Check(review.HasWitnessed && ReferenceEquals(reference, review.WitnessedRecording), "a completed witness survives pause only for its identical snapshot");
    }

    void CheckEarlyGuards()
    {
        Check(!review.CanUse(FinalDoorInteractionKind.Preserve, player.gameObject), "preserve rejects Player without a current witness");
        Check(!review.CanUse(FinalDoorInteractionKind.ReturnKey, player.gameObject), "key return rejects Player before preservation");
        Check(!review.CanUse(FinalDoorInteractionKind.OpenDoor, gameObject), "unrelated actor cannot use the domestic door");
        if (loop.ActiveEcho) Check(!review.CanUse(FinalDoorInteractionKind.Preserve, loop.ActiveEcho.gameObject), "Echo actor cannot preserve a recording");
    }

    void ConfigureDevices()
    {
        originalScheme = playerInput.currentControlScheme; originalDevices = playerInput.devices.ToArray();
        originalAutoSwitch = playerInput.neverAutoSwitchControlSchemes; oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        sensitivity = Field<float>(player, "lookSensitivity"); Check(sensitivity > 0, "actual look sensitivity is nonzero");
        keyboard = InputSystem.AddDevice<Keyboard>("FinalDoorTestKeyboard"); mouse = InputSystem.AddDevice<Mouse>("FinalDoorTestMouse"); devicesReady = true;
        playerInput.neverAutoSwitchControlSchemes = true; playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse); playerInput.ActivateInput();
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }

    void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        var frameKeys = keys;
        if (session.IsPaused && !expectedPause && !session.IsEnding)
        {
            if (!resumeDown) focusResumes++; resumeDown = !resumeDown;
            frameKeys = resumeDown ? new[] { Key.Enter } : Array.Empty<Key>();
        }
        else resumeDown = false;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(frameKeys));
        InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(
            Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12, 12) / sensitivity,
            -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10, 10) / sensitivity) });
        if (player.enabled && !session.IsPaused) player.GetType().GetField("cursorIsLocked", Private).SetValue(player, true);
    }

    void LateUpdate()
    {
        if (!running) return;
        string error = null;
        if (player.transform.position.y < -.2f) error = "Player fell below the authored floor";
        var echo = loop.ActiveEcho;
        if (echo && echo.gameObject.activeInHierarchy && !echo.IsComplete)
        {
            if (!ReferenceEquals(echo.Recording, loop.LastRecording)) error = "ActiveEcho recording differs from LastRecording reference";
            else
            {
                Track(echo.Recording);
                echo.Recording.Sample(echo.ElapsedTime, out Vector3 p, out Quaternion r);
                var body = echo.GetComponent<Rigidbody>();
                Vector3 actualPosition = body ? body.position : echo.transform.position;
                Quaternion actualRotation = body ? body.rotation : echo.transform.rotation;
                if (Vector3.Distance(p, actualPosition) > .025f || Quaternion.Angle(r, actualRotation) > .2f) error = "Playback body diverged from its immutable sampled pose";
                playbackFrames++;
            }
        }
        if (error != null) { Finish(false, error); StopAllCoroutines(); return; }
        if (Time.realtimeSinceStartup < nextLog) return;
        nextLog = Time.realtimeSinceStartup + .1f; Vector3 position = player.transform.position;
        log.WriteLine(string.Join(",", Time.realtimeSinceStartup.ToString("F3", Invariant), Time.time.ToString("F3", Invariant), step,
            position.x.ToString("F3", Invariant), position.y.ToString("F3", Invariant), position.z.ToString("F3", Invariant),
            player.transform.eulerAngles.y.ToString("F2", Invariant), Field<float>(player, "pitch").ToString("F2", Invariant), loop.CompletedLoopCount,
            (echo ? echo.ElapsedTime : -1).ToString("F3", Invariant), review.QualifiedPauseCount, review.WitnessProgress.ToString("F3", Invariant),
            review.HasWitnessed, review.OriginalPreserved, review.KeyReturned, review.Fade.ToString("F3", Invariant), session.IsPaused));
        log.Flush();
    }

    void Track(EchoRecording recording)
    {
        if (snapshots.Any(snapshot => ReferenceEquals(snapshot.recording, recording))) return;
        var copy = new EchoFrame[recording.FrameCount];
        for (int i = 0; i < copy.Length; i++) copy[i] = recording[i];
        snapshots.Add(new Snapshot { recording = recording, frames = copy });
    }

    IEnumerator CompleteEvidence()
    {
        foreach (var snapshot in snapshots)
        {
            Check(snapshot.recording.FrameCount == snapshot.frames.Length, "immutable snapshot retains its original frame count");
            bool same = true;
            for (int i = 0; i < snapshot.frames.Length; i++)
            {
                EchoFrame a = snapshot.recording[i], b = snapshot.frames[i];
                if (!a.Timestamp.Equals(b.Timestamp) || !a.Position.Equals(b.Position) || !a.Rotation.Equals(b.Rotation)) { same = false; break; }
            }
            Check(same, "all timestamps, positions and rotations of the captured snapshot remain identical");
        }
        Check(playbackFrames > 0 && snapshots.Count > 0, "live playback was audited rather than simulated by direct state flags");
        yield return Wait(.2f);
        foreach (string image in screenshots) Check(File.Exists(Path.Combine(output, image)), "capture exists: " + image);
        step = "done"; Finish(true, "");
    }

    IEnumerator Use(FinalDoorInteractionKind kind)
    {
        var component = FindObjectsByType<FinalDoorInteraction>(FindObjectsSortMode.None).Single(s => s.Kind == kind);
        var collider = component.GetComponentsInChildren<Collider>().Where(c => c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy)
            .OrderBy(c => Vector3.Distance(c.bounds.center, view.transform.position)).FirstOrDefault();
        Vector3 point = collider ? collider.bounds.center : component.transform.position;
        Check(Vector3.Distance(view.transform.position, point) <= interactor.InteractionDistance + .05f, kind + " lies within actual interaction range");
        Vector3 d = point - view.transform.position;
        yield return Face(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg);
        Check(interactor.TryGetTarget(out var target) && ReferenceEquals(target, component), "actual camera ray reaches " + kind);
        Check(component.CanInteract(player.gameObject), kind + " accepts the real Player in the correct phase");
        yield return Press(Key.E);
    }

    IEnumerator Go(float x, float z, float facing)
    {
        yield return Face(facing, 0); float deadline = Time.realtimeSinceStartup + 22;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(x, z)) > .1f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Physical route blocked at " + player.transform.position.ToString("F2") + " toward " + x + "," + z);
            Vector3 local = player.transform.InverseTransformDirection(new Vector3(x, player.transform.position.y, z) - player.transform.position);
            var move = new List<Key>();
            if (local.z > .06f) move.Add(Key.W); else if (local.z < -.06f) move.Add(Key.S);
            if (local.x > .06f) move.Add(Key.D); else if (local.x < -.06f) move.Add(Key.A);
            keys = move.ToArray(); yield return null;
        }
        keys = Array.Empty<Key>(); yield return Wait(.1f);
    }

    IEnumerator Face(float targetYaw, float targetPitch)
    {
        yaw = targetYaw; pitch = Mathf.Clamp(targetPitch, -84, 84); keys = Array.Empty<Key>();
        float deadline = Time.realtimeSinceStartup + 5;
        while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f || Mathf.Abs(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch)) > .6f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Synthetic mouse failed to reach the real camera angle");
            yield return null;
        }
        yield return Wait(.08f);
    }

    IEnumerator Press(Key key)
    {
        keys = new[] { key }; yield return new WaitForSecondsRealtime(.08f);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.09f);
    }
    IEnumerator Wait(float seconds) { keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(seconds); }
    IEnumerator Until(Func<bool> predicate, float seconds, string error)
    {
        keys = Array.Empty<Key>(); float deadline = Time.realtimeSinceStartup + seconds;
        while (!predicate()) { if (Time.realtimeSinceStartup > deadline) throw new Exception(error); yield return null; }
    }

    IEnumerator Capture(string label)
    {
        keys = Array.Empty<Key>(); yield return null;
        string name = mode + "-" + label; string gameName = name + "-Game.png";
        File.Delete(Path.Combine(output, gameName)); ScreenCapture.CaptureScreenshot(Path.Combine(output, gameName)); screenshots.Add(gameName);
        var oldTarget = view.targetTexture; var oldActive = RenderTexture.active;
        var target = new RenderTexture(1280, 720, 24); var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            view.targetTexture = target; view.Render(); RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
            string cameraName = name + "-Camera.png"; File.WriteAllBytes(Path.Combine(output, cameraName), pixels.EncodeToPNG()); screenshots.Add(cameraName);
            File.AppendAllText(PathFor("cameras.txt"), name + " position=" + view.transform.position.ToString("F3") + " euler=" + view.transform.eulerAngles.ToString("F2") +
                " FOV=" + view.fieldOfView + " paused=" + session.IsPaused + " step=" + step + "\n");
        }
        finally { view.targetTexture = oldTarget; RenderTexture.active = oldActive; Destroy(target); Destroy(pixels); }
        yield return Wait(.18f);
    }

    string PathFor(string suffix) => Path.Combine(output, mode + "." + suffix);
    void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(step + ": " + message);
        checks++; File.AppendAllText(PathFor("checks.txt"), "PASS " + message + "\n");
    }
    void Finish(bool success, string error)
    {
        if (Status.StartsWith("PASS") || Status.StartsWith("FAIL")) return;
        Status = success ? "PASS" : "FAIL " + step + ": " + error; running = false;
        var result = new Result
        {
            mode = mode, status = success ? "PASS" : "FAIL", error = error, lastStep = step,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, checks = checks, initialPoseFixtures = fixtureCount,
            immutableSnapshots = snapshots.Count, playbackFramesChecked = playbackFrames, unexpectedPauseResumes = focusResumes,
            elapsedRealtime = Time.realtimeSinceStartup - startedAt,
            scope = "Isolated final act. Initial public-API house fixture plus one initial Player placement; all subsequent gameplay mutations use synthetic Input System events. " +
                ((mode == "wrongfacing" || mode == "short") ? "This mode checks rejected takes only, not successful completion. " : "This mode attempts the physical route, witness, preservation, key return and ending. ") +
                "No earlier-campaign, human fear, first-play clarity or listening claim. Camera images omit UI; Game images include it.",
            witnessed = review && review.HasWitnessed, preserved = review && review.OriginalPreserved,
            keyReturned = review && review.KeyReturned, complete = review && review.IsComplete, screenshots = screenshots.ToArray()
        };
        File.WriteAllText(PathFor("result.json"), JsonUtility.ToJson(result, true)); File.WriteAllText(PathFor("result.txt"), Status + "\n");
        if (!success) File.AppendAllText(PathFor("checks.txt"), Status + "\n");
        Cleanup();
        if (success) Debug.Log("FinalDoor " + mode + " PASS"); else Debug.LogWarning("FinalDoor " + Status);
    }
    void Cleanup()
    {
        if (cleanedUp) return; cleanedUp = true; running = false; keys = Array.Empty<Key>();
        InputSystem.onBeforeUpdate -= InputUpdate; log?.Dispose(); log = null;
        if (campaign && saveCaptured) campaign.SuppressSaves = oldSuppressSaves;
        if (!devicesReady) return;
        InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
        if (playerInput)
        {
            playerInput.neverAutoSwitchControlSchemes = originalAutoSwitch;
            var available = originalDevices.Where(device => device != null && device.added).ToArray();
            if (!string.IsNullOrEmpty(originalScheme) && available.Length > 0)
                try { playerInput.SwitchCurrentControlScheme(originalScheme, available); }
                catch (Exception exception) { Debug.LogWarning("Final-door probe could not restore original input pairing: " + exception.Message); }
        }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }
    void OnDestroy() { Cleanup(); }
}
#endif

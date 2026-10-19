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
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>Isolated boundary fixture plus real input and a real-time wait longer than the recorder limit. Does not claim a P3 solve.</summary>
public sealed class ReturnRecordingBoundaryProbe : MonoBehaviour
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    FirstPersonController player; PlayerInput playerInput; PlayerInteractor interactor;
    GameSession session; CampaignDirector campaign; LoopController loop; EchoRecorder recorder;
    ReturnRecordingBoundary boundary; VS1HubReturnGate returnGate;
    Keyboard keyboard; Mouse mouse; InputDevice[] originalDevices;
    string originalScheme, output, step = "fixture", runtimeError;
    bool originalAutoSwitch, oldSuppress, saveCaptured, devicesReady, running, cleanedUp, expectedPause = true, verifyPressure;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    Key[] keys = Array.Empty<Key>(); EchoRecording original; EchoPlayback originalEcho; EchoFrame[] frames;
    float startedAt, requestedWait, actualWait, yaw, pitch, sensitivity, limit, interval, nextLog;
    int checks, playbackFrames, countAtBoundary, attemptAtBoundary, fixturePlacements;
    StreamWriter log;
    public string Status { get; private set; } = "starting";
    public string CurrentStep => step;
    [Serializable] sealed class Result
    {
        public string status, error, lastStep, scene, scope;
        public int checks, playbackFramesChecked, immutableFrames, fixturePlacements, bypassedBoundaryFixtures;
        public float elapsedRealtime, originalLimit, requestedWait, actualSimulationWait;
        public bool pressureRearmChecked, priorSaveSuppressionRestored;
    }
    public static void Run(float waitSeconds = 65f, bool verifyPressure = true)
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Run from fresh prepared Play Mode at the title.");
        if (FindObjectsByType<ReturnRecordingBoundaryProbe>(FindObjectsSortMode.None).Any(p => p.running)) throw new InvalidOperationException("Boundary probe already running.");
        var probe = new GameObject("Temporary isolated return-boundary verification").AddComponent<ReturnRecordingBoundaryProbe>();
        probe.requestedWait = waitSeconds; probe.verifyPressure = verifyPressure;
        probe.output = Path.Combine("Docs/VisualEvidence/2026-10-01-Threshold", "return-recording-boundary");
        Directory.CreateDirectory(Path.GetDirectoryName(probe.output));
        probe.StartCoroutine(probe.Guarded(probe.Execute()));
    }
    IEnumerator Guarded(IEnumerator root)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(root);
        while (stack.Count > 0)
        {
            object yielded;
            try { if (!stack.Peek().MoveNext()) { stack.Pop(); continue; } yielded = stack.Peek().Current; }
            catch (Exception exception) { Finish(false, exception.ToString()); yield break; }
            if (yielded is IEnumerator nested) { stack.Push(nested); continue; }
            yield return yielded;
        }
    }
    IEnumerator Execute()
    {
        yield return null; startedAt = Time.realtimeSinceStartup;
        File.WriteAllText(output + ".checks.txt", "ISOLATED FIXTURE: boundary's physical-R3 guard is explicitly bypassed after a genuine input recording. No completed P3 or full-route claim.\n");
        File.Delete(output + ".result.json");
        player = FindFirstObjectByType<FirstPersonController>();
        Check(player, "the authored real Player exists");
        playerInput = player.GetComponent<PlayerInput>(); interactor = player.GetComponent<PlayerInteractor>();
        session = FindFirstObjectByType<GameSession>(); campaign = FindFirstObjectByType<CampaignDirector>();
        loop = FindFirstObjectByType<LoopController>(); recorder = player.GetComponent<EchoRecorder>(); boundary = FindFirstObjectByType<ReturnRecordingBoundary>();
        Check(player && playerInput && interactor && session && campaign && loop && recorder && boundary && boundary.enabled, "authored boundary and recording systems exist");
        returnGate = Field<VS1HubReturnGate>(boundary, "returnGate");
        Check(returnGate && Field<CampaignDirector>(boundary, "campaign") == campaign && Field<LoopController>(boundary, "loop") == loop && Field<ReturnRecordingBoundary>(campaign, "returnRecordingBoundary") == boundary,
            "boundary and campaign share the explicit real R3 and loop references");
        Check(session.IsPaused && !session.HasStarted && campaign.Chapter == CampaignChapter.Protocol && !returnGate.IsOpen && !boundary.HasCommitted,
            "fresh title contains no prior R3 or boundary completion");
        oldSuppress = campaign.SuppressSaves; saveCaptured = true; campaign.SuppressSaves = true;
        limit = recorder.MaxRecordingDuration; interval = recorder.SampleInterval;
        ConfigureDevices(); Application.logMessageReceived += ReceiveLog;
        log = new StreamWriter(output + ".csv"); log.WriteLine("realtime,time,step,x,y,z,recalls,attempt,recording,frames,suspended,echoComplete,checkpoint");
        running = true; Status = "running"; yaw = player.transform.eulerAngles.y; pitch = Field<float>(player, "pitch");
        yield return Press(Key.Enter); Check(!session.IsPaused && loop.enabled, "real Enter starts the fresh session"); expectedPause = false;
        yield return Hold(Key.D, .25f); Check(recorder.HasRecordingStarted && !boundary.HasCommitted, "before R3, ordinary movement still records normally");
        int attempts = loop.CompletedLoopCount;
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == attempts + 1 && loop.ActiveEcho && loop.LastRecording != null, "ordinary R still creates a real immutable take before the boundary");
        original = loop.LastRecording; originalEcho = loop.ActiveEcho;
        frames = Enumerable.Range(0, original.FrameCount).Select(i => original[i]).ToArray();
        yield return Hold(Key.A, .16f); Check(recorder.HasRecordingStarted, "there is an unfinished new attempt to cancel at the boundary");
        step = "explicit_boundary_fixture";
        Vector3 before = player.transform.position; int revision = loop.CheckpointRevision;
        countAtBoundary = loop.CompletedLoopCount; attemptAtBoundary = loop.CurrentAttemptNumber;
        Check(boundary.BeginIsolatedSuspensionProbe(), "labelled editor fixture invokes the real suspension operation without opening or claiming R3");
        Check(!returnGate.IsOpen && boundary.HasCommitted && boundary.IsSuspended && boundary.SuspensionCount == 1,
            "fixture bypass is visible and bounded to the recording boundary");
        Check(Vector3.Distance(before, player.transform.position) < .001f && loop.CheckpointRevision == revision && loop.CompletedLoopCount == countAtBoundary,
            "suspension neither teleports, commits a checkpoint nor performs RECALL");
        Check(ReferenceEquals(original, loop.LastRecording) && ReferenceEquals(originalEcho, loop.ActiveEcho) && ReferenceEquals(original, boundary.RecordingAtBoundary),
            "boundary preserves both the original recording and its actual Echo object");
        Check(!loop.enabled && !recorder.IsRecording && recorder.CapturedFrameCount == 0 && campaign.IsReturnRecordingSuspended,
            "only the unfinished new attempt is discarded and R is explicitly suspended");
        var signal = Field<GameObject>(boundary, "endOfRecordingSignal");
        Check(!signal || signal.activeInHierarchy, "optional physical recording-boundary signal is visible");
        Check(campaign.StatusText.Contains("R INACTIVO") && campaign.Objective.Contains("subsuelo"), "help exposes the inactive R and the next explicit recording origin");
        yield return Hold(Key.D, .16f);
        Check(!recorder.IsRecording && recorder.CapturedFrameCount == 0, "walking outside the recording area cannot silently start a new take");
        yield return Press(Key.R); CheckPreserved();
        step = "pause_at_boundary"; expectedPause = true; yield return Press(Key.Escape);
        Check(session.IsPaused, "Escape pauses the suspended return"); float pausedAt = Time.time;
        yield return Press(Key.R); Check(Mathf.Approximately(pausedAt, Time.time), "paused R advances neither clock nor return");
        yield return Press(Key.Escape); Check(!session.IsPaused, "Escape resumes normally"); expectedPause = false;
        CheckPreserved();
        step = "wait_beyond_original_limit";
        float startSimulation = Time.time, startReal = Time.realtimeSinceStartup;
        float requiredWait = Mathf.Max(Mathf.Max(requestedWait, 61f), limit + 1f);
        Vector3 stablePose = player.transform.position;
        while (Time.time - startSimulation < requiredWait)
        {
            if (Time.realtimeSinceStartup - startReal > requiredWait + 15) throw new Exception("Simulation did not advance through the real-time suspension test.");
            keys = Array.Empty<Key>(); yield return null;
        }
        actualWait = Time.time - startSimulation;
        Check(actualWait > 60 && actualWait > limit && Vector2.Distance(new Vector2(stablePose.x, stablePose.z), new Vector2(player.transform.position.x, player.transform.position.z)) < .01f,
            "more than the original recording limit elapsed in live simulation without a recall teleport");
        yield return Press(Key.R); CheckPreserved();
        Check(originalEcho && originalEcho.IsComplete && !originalEcho.gameObject.activeInHierarchy,
            "original Echo finishes at its own recorded duration without being destroyed or prolonged by suspension");
        Check(recorder.MaxRecordingDuration == limit && recorder.SampleInterval == interval, "recording cap and sample cadence were never modified");
        CheckSnapshot();
        if (verifyPressure)
        {
            step = "explicit_pressure_fixture";
            // Separate, labelled chapter/placement fixture, not evidence of having walked the return.
            campaign.BeginExpansionProbe();
            player.ResetPose(new Vector3(1.1f, .03f, -29.6f), Quaternion.identity); fixturePlacements++;
            var station = FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s => s.Kind == StationKind.PressureOrigin);
            var collider = station.GetComponent<Collider>();
            Vector3 d = collider.bounds.center - player.GetComponentInChildren<Camera>().transform.position;
            yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; pitch = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
            float deadline = Time.realtimeSinceStartup + 5;
            while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f || Mathf.Abs(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch)) > .6f)
            { if (Time.realtimeSinceStartup > deadline) throw new Exception("Pressure fixture camera aim failed; keep Game view focused."); yield return null; }
            Check(interactor.TryGetTarget(out var target) && ReferenceEquals(target, station) && station.CanInteract(player.gameObject), "actual camera ray reaches the real pressure origin in the labelled fixture");
            yield return Press(Key.E);
            Check(campaign.Chapter == CampaignChapter.Pressure && loop.enabled && recorder.IsArmed && !boundary.IsSuspended && boundary.SuspensionCount == 1,
                "real E at PressureOrigin owns rearming and the boundary cannot suspend it again");
            Check(loop.CheckpointRevision == revision + 1 && loop.LastRecording == null && loop.ActiveEcho == null,
                "only the explicit new checkpoint clears the old take through its existing deterministic contract");
            Check(!signal || !signal.activeSelf, "old boundary signal releases on the new checkpoint");
            yield return Hold(Key.D, .20f); Check(recorder.HasRecordingStarted, "actual movement starts the new pressure recording");
            int previous = loop.CompletedLoopCount; yield return Press(Key.R);
            Check(loop.CompletedLoopCount == previous + 1 && loop.ActiveEcho && loop.LastRecording != null && !ReferenceEquals(original, loop.LastRecording),
                "ordinary R works again with a newly recorded pressure take");
            CheckSnapshot();
        }
        step = "done"; Finish(true, "");
    }
    void CheckPreserved()
    {
        Check(boundary.IsSuspended && !loop.enabled && !recorder.IsRecording && loop.CompletedLoopCount == countAtBoundary && loop.CurrentAttemptNumber == attemptAtBoundary,
            "manual R and elapsed time cannot create another attempt outside the recording area");
        Check(ReferenceEquals(original, loop.LastRecording) && ReferenceEquals(originalEcho, loop.ActiveEcho), "suspended return retains exact LastRecording and ActiveEcho references");
    }
    void CheckSnapshot() { Check(original.FrameCount == frames.Length && Enumerable.Range(0, frames.Length).All(i => SameFrame(original[i], frames[i])), "every original timestamp, position and rotation remains immutable"); }
    static bool SameFrame(EchoFrame a, EchoFrame b) => a.Timestamp.Equals(b.Timestamp) && a.Position.Equals(b.Position) && a.Rotation.Equals(b.Rotation);
    static T Field<T>(object target, string name) => (T)(target.GetType().GetField(name, Private) ?? throw new MissingFieldException(target.GetType().Name, name)).GetValue(target);
    void ConfigureDevices()
    {
        originalDevices = playerInput.devices.ToArray(); originalScheme = playerInput.currentControlScheme; originalAutoSwitch = playerInput.neverAutoSwitchControlSchemes;
        oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode; sensitivity = Field<float>(player, "lookSensitivity");
        if (sensitivity <= 0) throw new InvalidOperationException("A positive configured mouse sensitivity is required for real input aiming.");
        keyboard = InputSystem.AddDevice<Keyboard>("ReturnBoundaryKeyboard"); mouse = InputSystem.AddDevice<Mouse>("ReturnBoundaryMouse"); devicesReady = true;
        playerInput.neverAutoSwitchControlSchemes = true; playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }
    void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { delta = session.IsPaused ? Vector2.zero : new Vector2(
            Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12, 12) / sensitivity,
            -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10, 10) / sensitivity) });
    }
    void LateUpdate()
    {
        if (!running) return;
        string error = runtimeError;
        if (session.IsPaused && !expectedPause && !session.IsEnding) error = "Unexpected pause; no hidden focus recovery was attempted.";
        if (Time.realtimeSinceStartup - startedAt > Mathf.Max(requestedWait, limit) + 100) error = "Isolated boundary test timed out.";
        var echo = loop.ActiveEcho;
        if (echo && echo.gameObject.activeInHierarchy)
        {
            echo.Recording.Sample(echo.ElapsedTime, out Vector3 p, out Quaternion r); var body = echo.GetComponent<Rigidbody>();
            if (!ReferenceEquals(echo.Recording, loop.LastRecording) || !body || Vector3.Distance(p, body.position) > .025f || Quaternion.Angle(r, body.rotation) > .2f) error = "Echo playback diverged from its immutable sampled pose.";
            playbackFrames++;
        }
        if (boundary.IsSuspended && original != null && (!ReferenceEquals(original, loop.LastRecording) || loop.CompletedLoopCount != countAtBoundary)) error = "Suspended return replaced a recording or performed RECALL.";
        if (!string.IsNullOrEmpty(error)) { Finish(false, error); StopAllCoroutines(); return; }
        if (Time.realtimeSinceStartup < nextLog) return; nextLog = Time.realtimeSinceStartup + .5f;
        Vector3 position = player.transform.position;
        log.WriteLine(string.Join(",", Time.realtimeSinceStartup.ToString("F3", Invariant), Time.time.ToString("F3", Invariant), step,
            position.x.ToString("F3", Invariant), position.y.ToString("F3", Invariant), position.z.ToString("F3", Invariant), loop.CompletedLoopCount,
            loop.CurrentAttemptNumber, recorder.IsRecording, recorder.CapturedFrameCount, boundary.IsSuspended, echo && echo.IsComplete, loop.CheckpointRevision)); log.Flush();
    }
    void ReceiveLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message + "\n" + stack; }
    IEnumerator Press(Key key) { yield return Hold(key, .10f); }
    IEnumerator Hold(Key key, float seconds)
    {
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.06f);
        keys = new[] { key }; yield return new WaitForSecondsRealtime(seconds);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.10f);
    }
    void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(step + ": " + message); checks++; File.AppendAllText(output + ".checks.txt", "PASS [" + step + "] " + message + "\n"); }
    void Finish(bool success, string error)
    {
        if (Status.StartsWith("PASS") || Status.StartsWith("FAIL")) return;
        Status = success ? "PASS" : "FAIL " + step + ": " + error; Cleanup();
        File.AppendAllText(output + ".checks.txt", Status + "\n");
        File.WriteAllText(output + ".result.json", JsonUtility.ToJson(new Result
        {
            status = success ? "PASS" : "FAIL", error = error, lastStep = step, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
            checks = checks, playbackFramesChecked = playbackFrames, immutableFrames = frames?.Length ?? 0, fixturePlacements = fixturePlacements, bypassedBoundaryFixtures = boundary && boundary.HasCommitted ? 1 : 0,
            elapsedRealtime = Time.realtimeSinceStartup - startedAt, originalLimit = limit, requestedWait = requestedWait, actualSimulationWait = actualWait,
            pressureRearmChecked = success && verifyPressure, priorSaveSuppressionRestored = campaign && saveCaptured && campaign.SuppressSaves == oldSuppress,
            scope = "Isolated recording-boundary mechanics: an explicit editor fixture bypasses only physical R3 eligibility after an authentic input-generated take. " +
                "No P3/full-route proof. Wait uses real advancing simulation beyond the unchanged recording limit. Optional PressureOrigin verification uses one labelled chapter/pose fixture, then actual mouse/E/movement/R. " +
                "No fabricated snapshot, hidden pause resume or human play/listening claim."
        }, true));
        if (success) Debug.Log("Return recording boundary probe PASS"); else Debug.LogWarning(Status);
    }
    void Cleanup()
    {
        if (cleanedUp) return; cleanedUp = true; running = false; keys = Array.Empty<Key>();
        InputSystem.onBeforeUpdate -= InputUpdate; Application.logMessageReceived -= ReceiveLog; log?.Dispose(); log = null;
        if (campaign && saveCaptured) campaign.SuppressSaves = oldSuppress;
        if (!devicesReady) return;
        InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
        if (playerInput)
        {
            playerInput.neverAutoSwitchControlSchemes = originalAutoSwitch;
            var available = originalDevices.Where(d => d != null && d.added).ToArray();
            if (!string.IsNullOrEmpty(originalScheme) && available.Length > 0)
                try { playerInput.SwitchCurrentControlScheme(originalScheme, available); }
                catch (Exception exception) { Debug.LogWarning("Boundary probe input restoration: " + exception.Message); }
        }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }
    void OnDestroy() { Cleanup(); }
}
#endif

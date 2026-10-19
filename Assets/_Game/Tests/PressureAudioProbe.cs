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
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// One previous-chapter fixture, then genuine Input System movement, mouse, E,
/// Escape and R. Run in a fresh Play Mode session with PressureWorkAudio wired.
/// Source state and immutable Echo checks are objective evidence, not listening.
/// </summary>
[DefaultExecutionOrder(10000)]
public sealed class PressureAudioProbe : MonoBehaviour
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private FirstPersonController player;
    private PlayerInteractor interactor;
    private PlayerInput playerInput;
    private EchoRecorder recorder;
    private LoopController loop;
    private CampaignDirector campaign;
    private GameSession session;
    private PressureWorkAudio pressureAudio;
    private Camera view;
    private AudioSource flow, discharge;
    private Keyboard keyboard;
    private Mouse mouse;
    private InputDevice[] originalDevices;
    private string originalScheme, output, step = "fixture";
    private bool originalAutoSwitch, oldSuppressSaves, savesCaptured, inputConfigured, cleanedUp;
    private bool running, expectedPause, focusResumeKeyDown, haveSourceSample, tailAfterExitObserved;
    private bool trackFinalTail, finalTailEnded;
    private InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    private Key[] keys = Array.Empty<Key>();
    private float yaw, pitch, sensitivity, startedAt, nextLog, validSince = -1, invalidSince = -1;
    private float fadeDuration, flowVolume, finalTailExpectedSeconds;
    private double finalTailStartDsp;
    private int finalTailLastSample;
    private Vector3 finalTailPosition;
    private int checks, fixtureCount, focusResumes, echoPoseSamples, baselineDischarges, baselineValves;
    private Vector3 priorFlowPosition;
    private StreamWriter log;
    private SourceConfiguration flowConfiguration, dischargeConfiguration;
    private readonly List<Snapshot> recordings = new List<Snapshot>();
    public string Status { get; private set; } = "starting";
    public string CurrentStep => step;

    private sealed class Snapshot
    {
        public EchoRecording recording;
        public EchoFrame[] frames;
    }

    private sealed class SourceConfiguration
    {
        public Transform parent;
        public UnityEngine.Audio.AudioMixerGroup mixer;
        public float min, max, blend, doppler, pitch;
        public AudioRolloffMode rolloff;
        public bool ignorePause;
        public static SourceConfiguration Capture(AudioSource source) => new SourceConfiguration
        {
            parent = source.transform.parent, mixer = source.outputAudioMixerGroup,
            min = source.minDistance, max = source.maxDistance, blend = source.spatialBlend,
            doppler = source.dopplerLevel, pitch = source.pitch, rolloff = source.rolloffMode,
            ignorePause = source.ignoreListenerPause
        };
        public bool Matches(AudioSource source) => parent == source.transform.parent && mixer == source.outputAudioMixerGroup &&
            min == source.minDistance && max == source.maxDistance && blend == source.spatialBlend &&
            doppler == source.dopplerLevel && pitch == source.pitch && rolloff == source.rolloffMode &&
            ignorePause == source.ignoreListenerPause;
    }

    [Serializable]
    private sealed class Result
    {
        public string status, error, scene, lastStep, evidenceScope;
        public int checks, initialPoseFixtures, unexpectedPauseResumes, echoPoseSamples, immutableRecordings;
        public int completedValves, discharges, recalls;
        public float elapsedRealtime;
        public bool dischargeTailObservedAfterExit, finalDischargeEndedNaturally;
    }

    public static void Run()
    {
        var old = FindObjectsByType<PressureAudioProbe>(FindObjectsSortMode.None);
        if (old.Any(probe => probe.running)) throw new InvalidOperationException("Pressure audio verification is already running.");
        foreach (var probe in old) Destroy(probe.gameObject);
        var instance = new GameObject("Temporary pressure audio input verification").AddComponent<PressureAudioProbe>();
        instance.output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/VisualEvidence/2026-10-01-Residence/"));
        Directory.CreateDirectory(instance.output);
        instance.StartCoroutine(instance.Guarded(instance.Execute()));
    }

    private static T Field<T>(object owner, string name)
    {
        var info = owner.GetType().GetField(name, Private);
        if (info == null) throw new MissingFieldException(owner.GetType().Name, name);
        return (T)info.GetValue(owner);
    }

    private IEnumerator Guarded(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0)
        {
            object yielded = null;
            bool more = false;
            Exception error = null;
            try { more = stack.Peek().MoveNext(); if (more) yielded = stack.Peek().Current; }
            catch (Exception exception) { error = exception; }
            if (error != null) { Finish(false, error.ToString()); yield break; }
            if (!more) { stack.Pop(); continue; }
            if (yielded is IEnumerator nested) { stack.Push(nested); continue; }
            yield return yielded;
        }
    }

    private IEnumerator Execute()
    {
        yield return null;
        startedAt = Time.realtimeSinceStartup;
        File.WriteAllText(PathFor("checks.txt"), "One isolated previous-chapter fixture; subsequent gameplay uses synthetic Input System devices. No listening or human-play evidence.\n");
        File.Delete(PathFor("result.json"));
        File.Delete(PathFor("result.txt"));
        log = new StreamWriter(PathFor("csv"));
        log.WriteLine("realtime,time,step,chapter,playerX,playerY,playerZ,pressure,working,valves,discharges,paused,listenerPaused,flowPlaying,flowVolume,flowSamples,flowX,flowY,flowZ,dischargePlaying,dischargeSamples,dischargeX,dischargeY,dischargeZ,recalls,echoTime");
        player = FindFirstObjectByType<FirstPersonController>();
        campaign = FindFirstObjectByType<CampaignDirector>();
        session = FindFirstObjectByType<GameSession>();
        loop = FindFirstObjectByType<LoopController>();
        pressureAudio = FindFirstObjectByType<PressureWorkAudio>();
        Check(player && campaign && session && loop && pressureAudio && pressureAudio.isActiveAndEnabled,
            "the scene has an enabled PressureWorkAudio and its actual campaign systems");
        interactor = player.GetComponent<PlayerInteractor>();
        playerInput = player.GetComponent<PlayerInput>();
        recorder = player.GetComponent<EchoRecorder>();
        view = Field<Camera>(interactor, "viewCamera");
        flow = Field<AudioSource>(pressureAudio, "flowSource");
        discharge = Field<AudioSource>(pressureAudio, "dischargeSource");
        Check(interactor && playerInput && recorder && view && flow && discharge && flow != discharge,
            "Player and both distinct pressure sources are explicitly available");
        Check(Field<CampaignDirector>(pressureAudio, "director") == campaign && Field<GameSession>(pressureAudio, "session") == session,
            "pressure audio references this actual director and session");
        Check(flow.clip && discharge.clip && flow.loop && !discharge.loop && !flow.playOnAwake && !discharge.playOnAwake,
            "authored clips are assigned with deliberate loop and startup state");
        Check(!flow.ignoreListenerPause && !discharge.ignoreListenerPause && flow.spatialBlend > .99f && discharge.spatialBlend > .99f,
            "both world sources participate in listener pause and spatial playback");
        Check(discharge.rolloffMode == AudioRolloffMode.Linear && Mathf.Abs(discharge.maxDistance - 7f) < .01f && discharge.pitch > 0,
            "the valve discharge has the authored seven-metre linear falloff and forward playback");
        Check(campaign.ValvesDone == 0 && !campaign.IsWorking && pressureAudio.DischargeCount == 0,
            "fresh session has no completed valves, active work or startup discharge");
        baselineDischarges = pressureAudio.DischargeCount;
        baselineValves = campaign.ValvesDone;
        fadeDuration = Field<float>(pressureAudio, "fadeDuration");
        flowVolume = Field<float>(pressureAudio, "flowVolume");
        flowConfiguration = SourceConfiguration.Capture(flow);
        dischargeConfiguration = SourceConfiguration.Capture(discharge);

        // This is the sole fixture. No station, recorder, valve, pressure or audio state is injected.
        oldSuppressSaves = campaign.SuppressSaves;
        savesCaptured = true;
        campaign.SuppressSaves = true;
        session.BeginOrResume();
        campaign.BeginExpansionProbe();
        player.ResetPose(new Vector3(0, .03f, -26), Quaternion.Euler(0, 180, 0));
        fixtureCount++;
        ConfigureDevices();
        yaw = 180; pitch = 0;
        running = true; Status = "running";
        yield return Wait(.25f);
        Check(campaign.Chapter == CampaignChapter.Descent, "fixture stops before the genuine pressure-origin interaction");
        AssertSilent("startup and previous-chapter baseline");

        step = "enter_pressure";
        yield return Go(0, -29.6f, 180);
        yield return UseStation(StationKind.PressureOrigin, 1.1f, -29.6f, 0);
        Check(campaign.Chapter == CampaignChapter.Pressure && recorder.IsRecording, "real E enters pressure and starts recording");
        yield return Wait(.3f);
        AssertSilent("chapter entry is a baseline, not a valve release");

        step = "unpowered_valve";
        yield return UseStation(StationKind.ValveA, 4.2f, -36.5f, 90);
        yield return Wait(.4f);
        Check(campaign.Pressure < .01f && !campaign.IsWorking && campaign.ValvesDone == 0, "unpowered E rejects valve work");
        AssertSilent("unpowered interaction");

        step = "record_interruption_take";
        yield return RecordHold(30f);
        Check(campaign.Pressure < .01f, "the real Player holding the blue chamber supplies no pressure");
        AssertSilent("Player-only chamber hold");
        yield return Recall();
        yield return ReachValveA();
        yield return UseStation(StationKind.ValveA, 4.2f, -36.5f, 90);
        yield return Wait(.75f);
        CheckWorkingFlow(StationKind.ValveA);

        step = "pause_during_work";
        yield return PauseAndResume(flow);
        CheckWorkingFlow(StationKind.ValveA);
        Check(pressureAudio.DischargeCount == baselineDischarges, "work pause and resume create no discharge");

        step = "look_away_cancels";
        Vector3 firstValveSource = flow.transform.position;
        yield return Face(0, 0);
        yield return Wait(fadeDuration + .12f);
        Check(!campaign.IsWorking && campaign.ValvesDone == 0, "real mouse look cancels unfinished work");
        AssertSilent("look-away cancellation after authored fade");
        Check(Vector3.Distance(flow.transform.position, firstValveSource) < .001f, "cancellation leaves the source at the physical valve");

        step = "recall_during_work";
        yield return UseStation(StationKind.ValveA, 4.2f, -36.5f, 90);
        yield return Wait(.4f);
        CheckWorkingFlow(StationKind.ValveA);
        yield return Recall();
        yield return Wait(fadeDuration + .15f);
        Check(!campaign.IsWorking && campaign.ValvesDone == 0, "real R placement cancels the unfinished valve without granting progress");
        AssertSilent("RECALL cancellation after authored fade");
        Check(Vector3.Distance(flow.transform.position, firstValveSource) < .001f, "RECALL moves the Player, not the valve source");

        step = "record_success_take";
        yield return RecordHold(36f);
        yield return Recall();
        yield return ReachValveA();

        step = "complete_valve_a";
        yield return CompleteValve(StationKind.ValveA, 4.2f, -36.5f, 1);
        step = "pause_during_discharge";
        yield return PauseAndResume(discharge);
        Check(pressureAudio.DischargeCount == baselineDischarges + 1, "pausing a real release resumes its tail without a second discharge");
        Vector3 releasePosition = discharge.transform.position;
        yield return Press(Key.E);
        Check(campaign.ValvesDone == 1 && pressureAudio.DischargeCount == baselineDischarges + 1 && !campaign.IsWorking,
            "repeated real E on a completed valve cannot replay its discharge");
        yield return Face(180, 0);
        Check(Vector3.Distance(discharge.transform.position, releasePosition) < .001f, "turning the camera leaves the release at its physical valve");
        yield return Wait(discharge.clip.length + .25f);
        AssertSilent("first completed discharge ends naturally without another interaction");

        step = "complete_valve_b";
        yield return CompleteValve(StationKind.ValveB, 4.2f, -44, 2);
        Check(pressureAudio.DischargeCount - baselineDischarges == 2, "two genuine valve successes emit exactly two discharges");

        step = "leave_pressure";
        Vector3 finalFlowPosition = flow.transform.position;
        Vector3 finalDischargePosition = discharge.transform.position;
        yield return Go(0, -46, 270);
        yield return Go(0, -49.6f, 180);
        // Residence's entry panel is at z=-49.6; resolve the current collider for the actual E ray.
        yield return UseTarget(Station(StationKind.HouseOrigin));
        Check(campaign.Chapter == CampaignChapter.House && !loop.enabled && !recorder.IsRecording,
            "real E leaves pressure for the house and suspends recording");
        tailAfterExitObserved |= discharge.isPlaying;
        yield return Wait(fadeDuration + .2f);
        Check(!flow.isPlaying && flow.volume < .0001f && pressureAudio.DischargeCount == baselineDischarges + 2,
            "leaving pressure stops flow without generating a new discharge; an existing spatial tail may continue");
        int recallsAtExit = loop.CompletedLoopCount;
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == recallsAtExit && pressureAudio.DischargeCount == baselineDischarges + 2,
            "house R cannot produce a phantom pressure release");
        Check(Vector3.Distance(flow.transform.position, finalFlowPosition) < .001f &&
            Vector3.Distance(discharge.transform.position, finalDischargePosition) < .001f,
            "chapter exit and disabled R leave both sources at the last physical valve");
        step = "final_discharge_natural_tail";
        float tailDeadline = Time.realtimeSinceStartup + discharge.clip.length / discharge.pitch + 2;
        while (discharge.isPlaying)
        {
            if (Time.realtimeSinceStartup > tailDeadline) throw new Exception("The non-looping final discharge did not end naturally.");
            yield return null;
        }
        yield return Wait(.2f);
        AssertSilent("the final physical release ends naturally, including any tail after leaving pressure");
        Check(finalTailEnded, "the final discharge cursor was monitored until its natural duration, with no restart or early chapter cut");
        Check(flowConfiguration.Matches(flow) && dischargeConfiguration.Matches(discharge),
            "work audio preserves authored mixer routing, distances, spatial blend, Doppler, pitch and source parents");
        Check(recordings.Count == 3 && echoPoseSamples > 30, "three real immutable takes were monitored during playback");
        ValidateSnapshots();
        Check(fixtureCount == 1, "the complete probe used only one initial pose fixture");
        step = "done";
        Finish(true, "");
    }

    private IEnumerator RecordHold(float duration)
    {
        int recalls = loop.CompletedLoopCount;
        // Return north of the chamber partition before crossing into the recording lane.
        yield return Go(3.8f, -30.3f, 0);
        yield return Go(-5.7f, -30.3f, 270);
        yield return Go(-5.7f, -34, 270);
        Check(recorder.ElapsedTime + duration < recorder.MaxRecordingDuration - 1,
            "the real chamber hold fits inside the recording limit without an automatic RECALL");
        yield return Wait(duration);
        Check(loop.CompletedLoopCount == recalls, "the chamber take had no automatic or unrequested RECALL");
    }

    private IEnumerator ReachValveA()
    {
        yield return Go(3.8f, -32, 180);
        float deadline = Time.realtimeSinceStartup + 18;
        while (campaign.Pressure < .95f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Genuine recorded chamber hold did not supply pressure.");
            yield return null;
        }
        Check(loop.ActiveEcho && campaign.Pressure >= .95f, "the actual recorded Echo supplies pressure");
    }

    private IEnumerator CompleteValve(StationKind kind, float x, float z, int expectedValves)
    {
        yield return UseStation(kind, x, z, 90);
        yield return Wait(fadeDuration + .12f);
        CheckWorkingFlow(kind);
        float deadline = Time.realtimeSinceStartup + 9;
        while (campaign.ValvesDone < expectedValves)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Sustained real valve work did not complete: " + kind);
            yield return null;
        }
        yield return Wait(.08f);
        Check(campaign.ValvesDone == expectedValves && pressureAudio.DischargeCount == baselineDischarges + expectedValves,
            kind + " advances one real valve and exactly one discharge");
        Check(discharge.isPlaying && discharge.timeSamples > 0 && discharge.volume > 0,
            kind + " actually starts the authored discharge source");
        Check(Vector3.Distance(discharge.transform.position, Station(kind).transform.position) < .001f,
            kind + " releases from the completed station after WorkingStation clears");
        if (kind == StationKind.ValveB)
        {
            // Capture the actual playback cursor before walking away; never force a source state.
            trackFinalTail = true;
            finalTailPosition = discharge.transform.position;
            finalTailLastSample = discharge.timeSamples;
            finalTailExpectedSeconds = discharge.clip.length / discharge.pitch;
            finalTailStartDsp = AudioSettings.dspTime - discharge.timeSamples / (double)discharge.clip.frequency / discharge.pitch;
        }
        yield return Wait(fadeDuration + .1f);
        Check(!campaign.IsWorking && !flow.isPlaying && flow.volume < .0001f, kind + " stops its flow after completion fade");
    }

    private IEnumerator PauseAndResume(AudioSource activeSource)
    {
        Check(activeSource.isPlaying && activeSource.timeSamples > 0, "the source is advancing before the intentional pause");
        expectedPause = true;
        yield return Press(Key.Escape);
        Check(session.IsPaused && AudioListener.pause && Time.timeScale == 0, "real Escape pauses simulation and both world sources");
        float time = Time.time, work = campaign.WorkProgress;
        float echoTime = loop.ActiveEcho ? loop.ActiveEcho.ElapsedTime : -1;
        int count = pressureAudio.DischargeCount, recalls = loop.CompletedLoopCount, samples = activeSource.timeSamples;
        Vector3 playerPosition = player.transform.position, flowPosition = flow.transform.position, dischargePosition = discharge.transform.position;
        float volume = flow.volume;
        keys = new[] { Key.W, Key.R };
        yield return new WaitForSecondsRealtime(.55f);
        keys = Array.Empty<Key>();
        Check(Time.time == time && campaign.WorkProgress == work && loop.CompletedLoopCount == recalls && pressureAudio.DischargeCount == count,
            "paused W and R leave the clock, work, recalls and discharges unchanged");
        Check((!loop.ActiveEcho || loop.ActiveEcho.ElapsedTime == echoTime) && Vector3.Distance(playerPosition, player.transform.position) < .001f,
            "pause freezes the actual Echo and Player positions");
        Check(activeSource.timeSamples == samples && flow.volume == volume && Vector3.Distance(flowPosition, flow.transform.position) < .001f &&
            Vector3.Distance(dischargePosition, discharge.transform.position) < .001f,
            "pause freezes the source cursor, envelope and both world positions");
        yield return Press(Key.Escape);
        expectedPause = false;
        Check(!session.IsPaused && !AudioListener.pause && player.enabled, "real Escape restores simulation and Player control");
        Check(activeSource.isPlaying && activeSource.timeSamples > samples, "resume continues the existing audio cursor instead of restarting it");
        Check(pressureAudio.DischargeCount == count, "resume does not manufacture a new discharge");
    }

    private IEnumerator Recall()
    {
        int count = loop.CompletedLoopCount, dischargeCount = pressureAudio.DischargeCount;
        yield return Press(Key.R);
        // Match the target look to the real checkpoint only after the R edge; no direct pose mutation.
        yaw = loop.RecallSpawnRotation.eulerAngles.y; pitch = 0;
        Check(loop.CompletedLoopCount == count + 1 && loop.LastRecording != null && loop.ActiveEcho,
            "real R completes one take and instantiates actual playback");
        Check(pressureAudio.DischargeCount == dischargeCount, "RECALL itself emits no pressure discharge");
        RememberSnapshot(loop.LastRecording);
    }

    private void CheckWorkingFlow(StationKind kind)
    {
        Check(campaign.IsWorking && campaign.WorkingStation == Station(kind) && campaign.Pressure > .8f,
            kind + " has genuine, powered, observed work");
        Check(pressureAudio.IsFlowActive && flow.isPlaying && Mathf.Abs(flow.volume - flowVolume) < .001f,
            kind + " reaches the authored flow gain after its fade");
        Check(Vector3.Distance(flow.transform.position, Station(kind).transform.position) < .001f,
            kind + " positions flow at the real working station");
    }

    private void AssertSilent(string reason)
    {
        Check(!flow.isPlaying && flow.volume < .0001f && !discharge.isPlaying, reason + ": both pressure sources are silent");
        Check(pressureAudio.DischargeCount - baselineDischarges == campaign.ValvesDone - baselineValves,
            reason + ": release count matches actual completed valves");
    }

    private void LateUpdate()
    {
        if (!running) return;
        try
        {
            Require(player.transform.position.y >= -.2f, "Player fell below the authored floor.");
            Require(pressureAudio.DischargeCount - baselineDischarges == campaign.ValvesDone - baselineValves,
                "Discharge count diverged from genuine valve progression.");
            bool valid = session.HasStarted && campaign.Chapter == CampaignChapter.Pressure && campaign.IsWorking &&
                campaign.WorkingStation && campaign.Pressure > .8f;
            if (!session.IsPaused)
            {
                if (valid)
                {
                    invalidSince = -1;
                    if (validSince < 0) validSince = Time.time;
                    Require(Vector3.Distance(flow.transform.position, campaign.WorkingStation.transform.position) < .001f,
                        "Working flow left its station.");
                    if (Time.time - validSince > fadeDuration + .08f)
                        Require(flow.isPlaying && Mathf.Abs(flow.volume - flowVolume) < .001f, "Valid sustained work has no completed flow fade.");
                }
                else
                {
                    validSince = -1;
                    if (invalidSince < 0) invalidSince = Time.time;
                    if (Time.time - invalidSince > fadeDuration + .08f)
                        Require(!flow.isPlaying && flow.volume < .0001f, "Flow outlived invalid work beyond the authored fade.");
                    if (haveSourceSample)
                        Require(Vector3.Distance(priorFlowPosition, flow.transform.position) < .001f, "Inactive source followed Player movement or RECALL.");
                }
            }
            else Require(AudioListener.pause, "Session pause did not pause world audio.");
            Require(flow.volume >= 0 && flow.volume <= flowVolume + .001f, "Flow exceeded its authored gain.");
            if (trackFinalTail)
            {
                Require(Vector3.Distance(discharge.transform.position, finalTailPosition) < .001f,
                    "The final physical discharge moved away from its valve.");
                if (!session.IsPaused)
                {
                    if (discharge.isPlaying)
                    {
                        Require(!finalTailEnded, "The final discharge restarted after it had naturally ended.");
                        Require(discharge.timeSamples >= finalTailLastSample, "The final discharge cursor restarted while leaving pressure.");
                        finalTailLastSample = discharge.timeSamples;
                        if (campaign.Chapter != CampaignChapter.Pressure) tailAfterExitObserved = true;
                    }
                    else if (!finalTailEnded)
                    {
                        // Allow one DSP block / frame of scheduling tolerance; source Stop on chapter
                        // change must not masquerade as the authored tail reaching its end.
                        Require(AudioSettings.dspTime - finalTailStartDsp >= finalTailExpectedSeconds - .15f,
                            "The final discharge was cut before its natural duration.");
                        finalTailEnded = true;
                    }
                }
            }
            priorFlowPosition = flow.transform.position; haveSourceSample = true;
            if (loop.ActiveEcho && loop.ActiveEcho.gameObject.activeInHierarchy)
            {
                var echo = loop.ActiveEcho;
                Require(ReferenceEquals(echo.Recording, loop.LastRecording), "Playback replaced its immutable recording.");
                echo.Recording.Sample(echo.ElapsedTime, out var position, out var rotation);
                var body = echo.GetComponent<Rigidbody>();
                Require(body && Vector3.Distance(body.position, position) < .015f && Quaternion.Angle(body.rotation, rotation) < .15f,
                    "Physical Echo pose differs from its recorded sample.");
                echoPoseSamples++;
            }
            if (Time.realtimeSinceStartup < nextLog) return;
            nextLog = Time.realtimeSinceStartup + .1f;
            ValidateSnapshots();
            var fp = flow.transform.position; var dp = discharge.transform.position; var pp = player.transform.position;
            log.WriteLine(string.Join(",", new[]
            {
                Number(Time.realtimeSinceStartup), Number(Time.time), step, campaign.Chapter.ToString(), Number(pp.x), Number(pp.y), Number(pp.z), Number(campaign.Pressure),
                campaign.IsWorking.ToString(), campaign.ValvesDone.ToString(), pressureAudio.DischargeCount.ToString(), session.IsPaused.ToString(),
                AudioListener.pause.ToString(), flow.isPlaying.ToString(), Number(flow.volume), flow.timeSamples.ToString(),
                Number(fp.x), Number(fp.y), Number(fp.z), discharge.isPlaying.ToString(), discharge.timeSamples.ToString(),
                Number(dp.x), Number(dp.y), Number(dp.z), loop.CompletedLoopCount.ToString(),
                loop.ActiveEcho ? Number(loop.ActiveEcho.ElapsedTime) : ""
            }));
            log.Flush();
        }
        catch (Exception exception) { Finish(false, exception.ToString()); StopAllCoroutines(); }
    }

    private static string Number(float value) => value.ToString("F4", Invariant);
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

    private void RememberSnapshot(EchoRecording recording)
    {
        if (recordings.Any(snapshot => ReferenceEquals(snapshot.recording, recording))) return;
        var frames = new EchoFrame[recording.FrameCount];
        for (int i = 0; i < frames.Length; i++) frames[i] = recording[i];
        recordings.Add(new Snapshot { recording = recording, frames = frames });
    }

    private void ValidateSnapshots()
    {
        foreach (var snapshot in recordings)
        {
            Require(snapshot.recording.FrameCount == snapshot.frames.Length, "Recorded frame count changed.");
            for (int i = 0; i < snapshot.frames.Length; i++)
            {
                var before = snapshot.frames[i]; var after = snapshot.recording[i];
                Require(before.Timestamp.Equals(after.Timestamp) && before.Position.Equals(after.Position) && before.Rotation.Equals(after.Rotation),
                    "Recorded frame data changed after playback began.");
            }
        }
    }

    private void ConfigureDevices()
    {
        originalScheme = playerInput.currentControlScheme;
        originalDevices = playerInput.devices.ToArray();
        originalAutoSwitch = playerInput.neverAutoSwitchControlSchemes;
        oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        sensitivity = Field<float>(player, "lookSensitivity");
        Require(sensitivity > 0, "A nonzero look sensitivity is required.");
        keyboard = InputSystem.AddDevice<Keyboard>("PressureAudioTestKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("PressureAudioTestMouse");
        inputConfigured = true;
        playerInput.neverAutoSwitchControlSchemes = true;
        playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        playerInput.ActivateInput();
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }

    private void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        var frameKeys = keys;
        if (session.IsPaused && !expectedPause && !session.IsEnding)
        {
            // Editor focus recovery uses an actual Enter edge and is disclosed in the result.
            if (!focusResumeKeyDown) focusResumes++;
            focusResumeKeyDown = !focusResumeKeyDown;
            frameKeys = focusResumeKeyDown ? new[] { Key.Enter } : Array.Empty<Key>();
        }
        else focusResumeKeyDown = false;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(frameKeys));
        InputSystem.QueueStateEvent(mouse, new MouseState
        {
            delta = new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12, 12) / sensitivity,
                -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10, 10) / sensitivity)
        });
        // Same harness accommodation as ResidenceRouteProbe: tools can take Editor focus.
        if (player.enabled && !session.IsPaused) player.GetType().GetField("cursorIsLocked", Private).SetValue(player, true);
    }

    private CampaignStation Station(StationKind kind) => FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(station => station.Kind == kind);

    private IEnumerator UseStation(StationKind kind, float x, float z, float facing)
    {
        yield return Go(x, z, facing);
        yield return UseTarget(Station(kind));
    }

    private IEnumerator UseTarget(CampaignStation station)
    {
        var collider = station.GetComponentsInChildren<Collider>().Where(item => item.enabled && item.gameObject.activeInHierarchy && !item.isTrigger)
            .OrderBy(item => Vector3.Distance(view.transform.position, item.bounds.center)).FirstOrDefault();
        Vector3 point = collider ? collider.bounds.center : station.transform.position;
        Vector3 direction = point - view.transform.position;
        yield return Face(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg,
            -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg);
        Check(interactor.TryGetTarget(out var target) && ReferenceEquals(target, station), "actual camera ray reaches " + station.Kind);
        Check(station.CanInteract(player.gameObject), "actual station accepts interaction: " + station.Kind);
        yield return Press(Key.E);
    }

    private IEnumerator Go(float x, float z, float facing)
    {
        yield return Face(facing, 0);
        float deadline = Time.realtimeSinceStartup + 25;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(x, z)) > .1f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Physical route blocked at " + player.transform.position + " toward " + x + "," + z);
            var local = player.transform.InverseTransformDirection(new Vector3(x, player.transform.position.y, z) - player.transform.position);
            var movement = new List<Key>();
            if (local.z > .06f) movement.Add(Key.W); else if (local.z < -.06f) movement.Add(Key.S);
            if (local.x > .06f) movement.Add(Key.D); else if (local.x < -.06f) movement.Add(Key.A);
            keys = movement.ToArray();
            yield return null;
        }
        yield return Wait(.12f);
    }

    private IEnumerator Face(float targetYaw, float targetPitch)
    {
        yaw = targetYaw; pitch = Mathf.Clamp(targetPitch, -84, 84); keys = Array.Empty<Key>();
        float deadline = Time.realtimeSinceStartup + 5;
        while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f ||
            Mathf.Abs(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch)) > .6f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Real mouse look did not reach the requested camera angle.");
            yield return null;
        }
        yield return Wait(.12f);
    }

    private IEnumerator Press(Key key)
    {
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.08f);
        keys = new[] { key }; yield return new WaitForSecondsRealtime(.12f);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.12f);
    }

    private IEnumerator Wait(float seconds)
    {
        keys = Array.Empty<Key>();
        float until = Time.time + seconds, deadline = Time.realtimeSinceStartup + seconds + 20;
        while (Time.time < until)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Simulation wait timed out, paused=" + session.IsPaused);
            yield return null;
        }
    }

    private string PathFor(string suffix) => Path.Combine(output, "pressure-audio." + suffix);
    private void Check(bool condition, string message)
    {
        Require(condition, step + ": " + message);
        checks++;
        File.AppendAllText(PathFor("checks.txt"), "PASS " + message + "\n");
    }

    private void Finish(bool success, string error)
    {
        if (Status.StartsWith("PASS") || Status.StartsWith("FAIL")) return;
        Status = success ? "PASS" : "FAIL " + step + ": " + error;
        running = false;
        var result = new Result
        {
            status = success ? "PASS" : "FAIL", error = error, lastStep = step,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
            checks = checks, initialPoseFixtures = fixtureCount, unexpectedPauseResumes = focusResumes,
            echoPoseSamples = echoPoseSamples, immutableRecordings = recordings.Count,
            completedValves = campaign ? campaign.ValvesDone : -1, discharges = pressureAudio ? pressureAudio.DischargeCount : -1,
            recalls = loop ? loop.CompletedLoopCount : -1, elapsedRealtime = Time.realtimeSinceStartup - startedAt,
            dischargeTailObservedAfterExit = tailAfterExitObserved, finalDischargeEndedNaturally = finalTailEnded,
            evidenceScope = "One BeginExpansionProbe and initial Player pose; subsequent gameplay uses Input System keyboard/mouse, actual collision, recording and station rays. Source state/cursors/positions and recorded pose immutability only. Physical discharge may continue after leaving pressure, fixed at the valve with authored seven-metre linear falloff; the result separately records whether the route actually observed that tail after exit. No mix, listening, comfort, fear, human play or checkpoint-restore coverage."
        };
        File.WriteAllText(PathFor("result.json"), JsonUtility.ToJson(result, true));
        File.WriteAllText(PathFor("result.txt"), Status + "\n");
        if (!success) File.AppendAllText(PathFor("checks.txt"), Status + "\n");
        Cleanup();
        if (success) Debug.Log("Pressure audio input verification PASS"); else Debug.LogWarning("Pressure audio " + Status);
    }

    private void Cleanup()
    {
        if (cleanedUp) return;
        cleanedUp = true; running = false; keys = Array.Empty<Key>();
        InputSystem.onBeforeUpdate -= InputUpdate;
        log?.Dispose(); log = null;
        if (campaign && savesCaptured) campaign.SuppressSaves = oldSuppressSaves;
        if (!inputConfigured) return;
        InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
        if (playerInput)
        {
            playerInput.neverAutoSwitchControlSchemes = originalAutoSwitch;
            var available = originalDevices.Where(device => device != null && device.added).ToArray();
            if (!string.IsNullOrEmpty(originalScheme) && available.Length > 0)
            {
                try { playerInput.SwitchCurrentControlScheme(originalScheme, available); }
                catch (Exception exception) { Debug.LogWarning("Pressure probe could not restore input pairing: " + exception.Message); }
            }
        }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }

    private void OnDestroy() { Cleanup(); }
}
#endif

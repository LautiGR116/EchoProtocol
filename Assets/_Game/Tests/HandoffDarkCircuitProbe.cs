#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using UnityEngine;

/// <summary>
/// Focused state regression after an independently reached Pressure handoff.
/// snapshot is read-only; dark-circuit-fixture temporarily sets twelve existing
/// light intensities to zero, then restores them. Neither mode drives gameplay.
/// No full-blackout-route or listening claim follows from this short test.
/// </summary>
[DefaultExecutionOrder(20000)]
public sealed class HandoffDarkCircuitProbe : MonoBehaviour
{
    private const float Duration = 2f;
    private const float Epsilon = .0001f;
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private CampaignDirector director;
    private FirstPersonController player;
    private CampaignRoomTone roomTone;
    private VS1StrongEventOne firstAct;
    private AudioSource fluorescent, machinery;
    private Light[] lights;
    private float[] originalIntensities;
    private int initialFirstWrites, initialRoomWrites, frames, settledFrames, checks;
    private float elapsed, realtimeStart, initialFluorescentGain;
    private bool running, fixtureApplied, finished;
    private string mode, output;
    public string Status { get; private set; } = "starting";

    [Serializable]
    private sealed class Result
    {
        public string status, error, mode, scene, evidenceScope;
        public int checks, frames, settledFrames, fixtureLightCount, firstActWritesBefore, firstActWritesAfter, campaignWriteDelta, releaseCount;
        public float scaledSecondsObserved, elapsedRealtime, initialFluorescentGain, fluorescentGain, machineryGain, poweredFraction;
        public bool fullBlackoutRouteClaim, fixtureRestored;
    }

    public static void Run(string mode = "snapshot")
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Run only in Play Mode after reaching Pressure.");
        if (mode != "snapshot" && mode != "dark-circuit-fixture")
            throw new ArgumentException("Modes: snapshot, dark-circuit-fixture.");
        if (FindObjectsByType<HandoffDarkCircuitProbe>(FindObjectsSortMode.None).Any(probe => !probe.finished))
            throw new InvalidOperationException("A dark-circuit handoff probe is already running.");
        var probe = new GameObject("Temporary dark-circuit handoff observer").AddComponent<HandoffDarkCircuitProbe>();
        probe.mode = mode;
    }

    private static T Field<T>(object owner, string name)
    {
        var field = owner.GetType().GetField(name, Private);
        if (field == null) throw new MissingFieldException(owner.GetType().Name, name);
        return (T)field.GetValue(owner);
    }

    private void Start()
    {
        realtimeStart = Time.realtimeSinceStartup;
        output = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Docs/VisualEvidence/2026-10-01-Reception/handoff-dark-circuit-" + mode));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output + ".checks.txt", "Focused post-handoff source-state regression. No full-route or listening claim. Mode: " + mode + "\n");
        File.Delete(output + ".result.json");
        try
        {
            firstAct = FindFirstObjectByType<VS1StrongEventOne>();
            roomTone = FindFirstObjectByType<CampaignRoomTone>();
            director = FindFirstObjectByType<CampaignDirector>();
            player = FindFirstObjectByType<FirstPersonController>();
            Check(firstAct && roomTone && director && player && firstAct.isActiveAndEnabled && roomTone.isActiveAndEnabled,
                "actual enabled mix owners, director and Player exist");
            Check(director.Chapter == CampaignChapter.Pressure && player.transform.position.z < -26f,
                "independent route or checkpoint already reached Pressure beyond the first-act region");
            Check(firstAct.AmbienceControlReleased && roomTone.OwnsAmbience && firstAct.AmbienceReleaseCount == 1,
                "one-way handoff has already occurred; this probe does not release either owner");
            Check(Field<VS1StrongEventOne>(roomTone, "firstActEvent") == firstAct && Field<CampaignDirector>(roomTone, "director") == director,
                "campaign owner references this exact first act and chapter director");
            fluorescent = firstAct.FluorescentCircuitSource;
            var beds = Field<AudioSource[]>(firstAct, "ambienceSources");
            var campaignBeds = Field<AudioSource[]>(roomTone, "institution");
            machinery = beds.SingleOrDefault(source => source && source != fluorescent && source.name == "Distant Machinery Bed");
            Check(fluorescent && machinery && campaignBeds.Contains(fluorescent) && campaignBeds.Contains(machinery),
                "fluorescent circuit and distinct distant machinery are the two shared configured sources");
            lights = Field<Light[]>(firstAct, "facilityBlackoutLights");
            Check(lights != null && lights.Length == 12 && lights.All(light => light) && lights.Distinct().Count() == 12,
                "twelve distinct existing circuit lights are configured");
            if (mode == "dark-circuit-fixture")
            {
                originalIntensities = lights.Select(light => light.intensity).ToArray();
                fixtureApplied = true;
                foreach (var light in lights) light.intensity = 0f;
                Check(true, "FIXTURE: twelve light intensities set to zero; original values will be restored after observation");
            }
            else Check(true, "SNAPSHOT: no light or gameplay state was changed");
            RequireDarkCircuit();
            initialFirstWrites = firstAct.AmbienceWriteCount;
            initialRoomWrites = roomTone.AmbienceWriteCount;
            initialFluorescentGain = fluorescent.volume;
            running = true; Status = "running";
        }
        catch (Exception exception) { Finish(false, exception.Message); }
    }

    private void LateUpdate()
    {
        if (!running) return;
        try
        {
            Require(Time.realtimeSinceStartup - realtimeStart < 30f, "Timed out waiting for two unpaused seconds; this probe never resumes the session.");
            Require(firstAct.AmbienceControlReleased && roomTone.OwnsAmbience && firstAct.AmbienceReleaseCount == 1,
                "Handoff was reclaimed or duplicated.");
            Require(firstAct.AmbienceWriteCount == initialFirstWrites, "SE1 wrote a shared source after releasing control.");
            Require(director.Chapter == CampaignChapter.Pressure && player.transform.position.z < -26f,
                "Player left the Pressure regression region during observation.");
            RequireDarkCircuit();
            if (Time.timeScale <= 0f || Time.deltaTime <= 0f) return;
            elapsed += Time.deltaTime; frames++;
            if (elapsed >= 1.5f)
            {
                Require(fluorescent.volume <= Epsilon, "A de-energized fluorescent circuit regained source gain after handoff.");
                Require(machinery.volume > Epsilon && machinery.isPlaying && !machinery.mute,
                    "Independent distant machinery was muted or stopped with the fluorescent circuit.");
                settledFrames++;
            }
            if (elapsed < Duration) return;
            Check(frames >= 2 && settledFrames >= 1 && roomTone.AmbienceWriteCount - initialRoomWrites >= 2,
                "observed two seconds of real campaign Update writes");
            Check(firstAct.AmbienceWriteCount == initialFirstWrites, "SE1 write count remained constant throughout");
            Check(fluorescent.volume <= Epsilon && firstAct.FluorescentPowerFraction <= Epsilon,
                "de-energized fluorescent source remained at zero after settling");
            Check(machinery.volume > Epsilon && machinery.isPlaying && !machinery.mute,
                "independent machinery source retained positive gain and playback");
            Finish(true, "");
        }
        catch (Exception exception) { Finish(false, exception.Message); }
    }

    private void RequireDarkCircuit()
    {
        Require(lights.All(light => light && (!light.enabled || !light.gameObject.activeInHierarchy || light.intensity <= Epsilon)) &&
            firstAct.FluorescentPowerFraction <= Epsilon, "The required twelve-light dark circuit is not present.");
    }

    private static void Require(bool value, string error) { if (!value) throw new InvalidOperationException(error); }
    private void Check(bool value, string message)
    {
        Require(value, message); checks++;
        File.AppendAllText(output + ".checks.txt", "PASS " + message + "\n");
    }

    private void RestoreFixture()
    {
        if (!fixtureApplied) return;
        for (int i = 0; i < lights.Length; i++) if (lights[i]) lights[i].intensity = originalIntensities[i];
        fixtureApplied = false;
    }

    private void Finish(bool success, string error)
    {
        if (finished) return;
        finished = true; running = false; Status = success ? "PASS" : "FAIL: " + error;
        var result = new Result
        {
            status = Status, error = error, mode = mode, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
            checks = checks, frames = frames, settledFrames = settledFrames, fixtureLightCount = originalIntensities == null ? 0 : originalIntensities.Length,
            firstActWritesBefore = initialFirstWrites, firstActWritesAfter = firstAct ? firstAct.AmbienceWriteCount : -1,
            campaignWriteDelta = roomTone ? roomTone.AmbienceWriteCount - initialRoomWrites : 0,
            releaseCount = firstAct ? firstAct.AmbienceReleaseCount : 0, scaledSecondsObserved = elapsed,
            elapsedRealtime = Time.realtimeSinceStartup - realtimeStart, initialFluorescentGain = initialFluorescentGain,
            fluorescentGain = fluorescent ? fluorescent.volume : -1, machineryGain = machinery ? machinery.volume : -1,
            poweredFraction = firstAct ? firstAct.FluorescentPowerFraction : -1, fullBlackoutRouteClaim = false,
            evidenceScope = "Two-second Pressure snapshot or explicitly labelled twelve-light fixture after independently established handoff. No full-blackout-route, Echo playback, listening, timbre or human fear claim. Probe never changes recordings, chapter, Player pose, audio source values, timeScale, AudioListener settings or preferences."
        };
        RestoreFixture(); result.fixtureRestored = !fixtureApplied;
        File.WriteAllText(output + ".result.json", JsonUtility.ToJson(result, true));
        if (success) Debug.Log("Handoff dark circuit " + mode + " PASS");
        else Debug.LogWarning("Handoff dark circuit " + mode + " " + Status);
    }

    private void OnDisable() { RestoreFixture(); }
}
#endif

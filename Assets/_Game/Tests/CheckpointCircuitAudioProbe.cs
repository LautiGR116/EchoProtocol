#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;

/// <summary>
/// Isolated checkpoint-equivalent fixtures, not a saved-game round trip.
/// Run after starting a fresh session. Leaves the runtime in the final-review
/// fixture; stop Play afterward. Never writes preferences or saves scenes.
/// </summary>
[DefaultExecutionOrder(20000)]
public sealed class CheckpointCircuitAudioProbe : MonoBehaviour
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private CampaignDirector director;
    private CampaignRoomTone roomTone;
    private VS1StrongEventOne firstAct;
    private FirstPersonController player;
    private GameSession session;
    private HouseMemory house;
    private FinalDoorMemory finalDoor;
    private AudioSource fluorescent, machinery;
    private Transform[] origins;
    private Light[] lights;
    private bool finished, suppressionCaptured, originalSuppression;
    private float began;
    private int checks, fixturePlacements;
    private string output, step = "initialization";
    private readonly List<CaseResult> cases = new List<CaseResult>();
    private readonly List<IntPreference> preferences = new List<IntPreference>();
    private bool hadVolume;
    private float volumePreference, listenerVolume;
    public string Status { get; private set; } = "starting";

    [Serializable] private sealed class CaseResult
    {
        public string chapter;
        public float observedSeconds, fluorescentGain, poweredFraction, machineryGain;
        public int campaignWrites, unchangedFirstActWrites, releaseCount;
    }
    private sealed class IntPreference { public string key; public bool exists; public int value; }
    [Serializable] private sealed class Result
    {
        public string status, error, scene, evidenceScope;
        public int checks, fixturePlacements;
        public float elapsedRealtime;
        public bool preferencesUnchanged, suppressionRestored, actualSavedGameRestoreTested;
        public CaseResult[] cases;
    }

    public static void Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Run in a fresh, started Play Mode session.");
        if (FindObjectsByType<CheckpointCircuitAudioProbe>(FindObjectsSortMode.None).Any(probe => !probe.finished))
            throw new InvalidOperationException("A checkpoint circuit probe is already active.");
        new GameObject("Temporary checkpoint circuit fixtures").AddComponent<CheckpointCircuitAudioProbe>();
    }

    private static T Field<T>(object owner, string name)
    {
        var field = owner.GetType().GetField(name, Private);
        if (field == null) throw new MissingFieldException(owner.GetType().Name, name);
        return (T)field.GetValue(owner);
    }

    private void Start()
    {
        began = Time.realtimeSinceStartup;
        output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/VisualEvidence/2026-10-01-Reception/checkpoint-circuit-audio"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output + ".checks.txt", "ISOLATED CHECKPOINT-EQUIVALENT FIXTURES. No save/Continue round trip, full route or listening claim.\n");
        File.Delete(output + ".result.json");
        Status = "running";
        StartCoroutine(Guarded(Execute()));
    }

    private IEnumerator Guarded(IEnumerator root)
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
            catch (Exception exception) { Finish(false, exception.Message); yield break; }
            if (yielded is IEnumerator nested) stack.Push(nested); else yield return yielded;
        }
    }

    private IEnumerator Execute()
    {
        yield return null;
        director = FindFirstObjectByType<CampaignDirector>(); roomTone = FindFirstObjectByType<CampaignRoomTone>();
        firstAct = FindFirstObjectByType<VS1StrongEventOne>(); player = FindFirstObjectByType<FirstPersonController>();
        session = FindFirstObjectByType<GameSession>(); house = FindFirstObjectByType<HouseMemory>();
        finalDoor = FindFirstObjectByType<FinalDoorMemory>();
        Check(director && roomTone && firstAct && player && session && house && finalDoor,
            "configured campaign, audio owners and final-review systems exist");
        Check(session.HasStarted && !session.IsPaused && director.Chapter == CampaignChapter.Protocol &&
            !firstAct.AmbienceControlReleased && !roomTone.OwnsAmbience,
            "fresh session was started independently; this probe never resumes or pauses it");
        fluorescent = firstAct.FluorescentCircuitSource;
        machinery = Field<AudioSource[]>(roomTone, "institution").SingleOrDefault(source => source && source.name == "Distant Machinery Bed");
        lights = Field<Light[]>(firstAct, "facilityBlackoutLights"); origins = Field<Transform[]>(director, "origins");
        Check(fluorescent && machinery && origins.Length == 4 && lights.Length == 12 &&
            Field<VS1StrongEventOne>(roomTone, "firstActEvent") == firstAct && Field<CampaignDirector>(roomTone, "director") == director,
            "explicit sources, chapter director and all twelve lights match the production configuration");
        Check(LightsPowered() && fluorescent.volume > .0001f && machinery.isPlaying,
            "positive control: initial first-act lights and fluorescent are powered before fixture placement");
        CapturePreferences();
        originalSuppression = director.SuppressSaves; suppressionCaptured = true; director.SuppressSaves = true;

        step = "Pressure fixture";
        director.BeginExpansionProbe();
        Place(0); Interact(StationKind.PressureOrigin);
        yield return Observe(CampaignChapter.Pressure);

        step = "House fixture";
        director.PrepareHouseProbe();
        Place(1); Interact(StationKind.HouseOrigin);
        yield return Observe(CampaignChapter.House);

        step = "Final-review fixture";
        house.RestoreAtReview(false, HousePowerLocation.Hall);
        Place(3);
        Check(finalDoor.EnterReview(), "labelled domestic checkpoint fixture enters the actual final-review API");
        yield return null; // CampaignDirector owns its transition to Archive.
        yield return Observe(CampaignChapter.Archive);
        Check(PreferencesUnchanged(), "checkpoint, volume, dynamics and speaker preferences were never changed");
        Finish(true, "");
    }

    private void Place(int origin)
    {
        player.ResetPose(origins[origin].position, origins[origin].rotation); fixturePlacements++;
        Check(true, "FIXTURE: Player placed at authored origin " + origin + "; this is not a traversed route");
    }
    private void Interact(StationKind kind)
    {
        var station = FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(value => value.Kind == kind);
        Check(station.CanInteract(player.gameObject), "labelled prerequisite fixture permits actual station API " + kind);
        station.Interact(player.gameObject);
    }

    private IEnumerator Observe(CampaignChapter chapter)
    {
        // Allow real Update to perform the one-time handoff, never invoke it here.
        yield return null; yield return null;
        Check(director.Chapter == chapter && firstAct.AmbienceControlReleased && roomTone.OwnsAmbience && firstAct.AmbienceReleaseCount == 1,
            chapter + " owns the beds after an actual Update handoff");
        int firstWrites = firstAct.AmbienceWriteCount, campaignWrites = roomTone.AmbienceWriteCount;
        float elapsed = 0f, realtime = Time.realtimeSinceStartup;
        while (elapsed < 2f)
        {
            yield return null;
            Require(Time.realtimeSinceStartup - realtime < 30f, "Observation stalled; the probe never resumes a paused session.");
            Require(director.Chapter == chapter && firstAct.AmbienceWriteCount == firstWrites && firstAct.AmbienceReleaseCount == 1,
                "Chapter changed or SE1 reclaimed the bed after handoff.");
            Require(LightsPowered(), "The old circuit lost power; this would no longer exercise the checkpoint leak.");
            Require(PreferencesUnchanged() && AudioListener.volume == listenerVolume, "Preferences or global volume changed.");
            if (session.IsPaused || Time.deltaTime <= 0) continue;
            elapsed += Time.deltaTime;
            if (elapsed >= 1f)
            {
                Require(fluorescent.volume <= .0001f, "First-act 2D fluorescent leaked into " + chapter + " while distant lights retained power.");
                Require(machinery.volume > .0001f && machinery.isPlaying && !machinery.mute,
                    "The independent distant machinery was silenced with the fluorescent.");
            }
        }
        Check(roomTone.AmbienceWriteCount - campaignWrites >= 2, chapter + " passed two seconds of live audio-state observation");
        cases.Add(new CaseResult { chapter = chapter.ToString(), observedSeconds = elapsed, fluorescentGain = fluorescent.volume,
            poweredFraction = firstAct.FluorescentPowerFraction, machineryGain = machinery.volume,
            campaignWrites = roomTone.AmbienceWriteCount - campaignWrites, unchangedFirstActWrites = firstWrites,
            releaseCount = firstAct.AmbienceReleaseCount });
    }

    private bool LightsPowered() => lights.All(light => light && light.enabled && light.gameObject.activeInHierarchy && light.intensity > .0001f)
        && firstAct.FluorescentPowerFraction > .99f;
    private void CapturePreferences()
    {
        string checkpoint = Field<string>(director, "checkpointKey");
        foreach (string key in new[] { checkpoint, checkpoint + ".answered", checkpoint + ".light", "EP.Reduced", "EP.Speakers" })
            preferences.Add(new IntPreference { key = key, exists = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetInt(key) });
        hadVolume = PlayerPrefs.HasKey("EP.Volume"); volumePreference = PlayerPrefs.GetFloat("EP.Volume"); listenerVolume = AudioListener.volume;
    }
    private bool PreferencesUnchanged() => preferences.All(value => PlayerPrefs.HasKey(value.key) == value.exists && PlayerPrefs.GetInt(value.key) == value.value)
        && PlayerPrefs.HasKey("EP.Volume") == hadVolume && PlayerPrefs.GetFloat("EP.Volume") == volumePreference;
    private static void Require(bool condition, string error) { if (!condition) throw new InvalidOperationException(error); }
    private void Check(bool condition, string message)
    {
        Require(condition, step + ": " + message); checks++;
        File.AppendAllText(output + ".checks.txt", "PASS [" + step + "] " + message + "\n");
    }
    private void RestoreSuppression() { if (suppressionCaptured && director) director.SuppressSaves = originalSuppression; }
    private void Finish(bool success, string error)
    {
        if (finished) return;
        finished = true; Status = success ? "PASS" : "FAIL: " + error; RestoreSuppression();
        File.WriteAllText(output + ".result.json", JsonUtility.ToJson(new Result
        {
            status = Status, error = error, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
            checks = checks, fixturePlacements = fixturePlacements, elapsedRealtime = Time.realtimeSinceStartup - began,
            preferencesUnchanged = suppressionCaptured && PreferencesUnchanged(), suppressionRestored = !suppressionCaptured || director && director.SuppressSaves == originalSuppression,
            actualSavedGameRestoreTested = false, cases = cases.ToArray(),
            evidenceScope = "Three labelled API/pose fixtures reproduce powered initial first-act lights at Pressure, House and final-review entry. Live CampaignRoomTone updates are observed; no preferences, global volume, timeScale, source values or scene assets are written. Save/Continue round trip, full route, Echo fidelity and listening are not tested. Runtime is left in the final fixture; stop Play afterward."
        }, true));
        if (success) Debug.Log("Checkpoint circuit audio PASS"); else Debug.LogWarning("Checkpoint circuit audio " + Status);
    }
    private void OnDisable() { StopAllCoroutines(); RestoreSuppression(); }
}
#endif

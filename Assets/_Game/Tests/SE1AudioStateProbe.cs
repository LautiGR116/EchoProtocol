#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Audio;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using UnityEngine;

/// <summary>
/// Read-only observer; never prepares gameplay, moves the Player or fires audio.
/// Run("watch") alongside the existing route before SE1; it ends at the campaign
/// handoff. Run("blackout") or Run("handoff") checks the current state for one second.
/// This is source/state evidence, not a listening assessment.
/// </summary>
[DefaultExecutionOrder(20000)]
public sealed class SE1AudioStateProbe : MonoBehaviour
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private VS1StrongEventOne firstAct;
    private CampaignRoomTone campaignMix;
    private AudioSource fluorescent;
    private Light[] circuitLights;
    private AudioSource[] beds;
    private float[] bedLevels;
    private PracticalEmission[] circuitDiffusers;
    private MaterialPropertyBlock materialState;
    private int previousFirstWrites, previousCampaignWrites, releasedWriteCount, checks, frames;
    private bool running, sawBlackout, sawRelease;
    private float began, stableSince = -1;
    private string mode, output;
    public string Status { get; private set; } = "starting";

    [Serializable]
    private sealed class Result
    {
        public string status, mode, scene, error, evidenceScope;
        public int checks, monitoredFrames, firstActWrites, campaignWrites, releaseCount;
        public bool blackoutObserved, handoffObserved;
        public float fluorescentGain, poweredFraction, elapsedRealtime;
    }

    public static void Run(string mode = "watch")
    {
        if (mode != "watch" && mode != "blackout" && mode != "handoff")
            throw new ArgumentException("Modes: watch, blackout, handoff.");
        if (FindObjectsByType<SE1AudioStateProbe>(FindObjectsSortMode.None).Any(probe => probe.running))
            throw new InvalidOperationException("An SE1 audio observer is already running.");
        var instance = new GameObject("Temporary SE1 circuit audio observer").AddComponent<SE1AudioStateProbe>();
        instance.mode = mode;
    }

    private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);

    private void Start()
    {
        began = Time.realtimeSinceStartup;
        output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/VisualEvidence/2026-10-01-Threshold/se1-audio-" + mode));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output + ".checks.txt", "Read-only circuit and ownership observer. No gameplay fixtures or listening evidence.\n");
        File.Delete(output + ".result.json");
        try
        {
            materialState = new MaterialPropertyBlock();
            firstAct = FindFirstObjectByType<VS1StrongEventOne>();
            campaignMix = FindFirstObjectByType<CampaignRoomTone>();
            Check(firstAct && campaignMix, "both actual mix owners exist");
            fluorescent = firstAct.FluorescentCircuitSource;
            circuitLights = Field<Light[]>(firstAct, "facilityBlackoutLights");
            beds = Field<AudioSource[]>(firstAct, "ambienceSources");
            bedLevels = Field<float[]>(firstAct, "ambienceBaseVolumes");
            circuitDiffusers = FindObjectsByType<PracticalEmission>(FindObjectsSortMode.None)
                .Where(diffuser => circuitLights.Contains(Field<Light>(diffuser, "source"))).ToArray();
            Check(fluorescent && circuitLights.Length == 12 && beds.Any(bed => bed && bed != fluorescent),
                "explicit fluorescent circuit and separate recovering machinery bed exist");
            Check(Field<VS1StrongEventOne>(campaignMix, "firstActEvent") == firstAct && Field<EchoProtocol.Campaign.CampaignDirector>(campaignMix, "director"),
                "campaign mix explicitly references this first act and its chapter director");
            Check(circuitDiffusers.Length > 0, "visible diffusers explicitly follow the blackout circuit");
            previousFirstWrites = firstAct.AmbienceWriteCount;
            previousCampaignWrites = campaignMix.AmbienceWriteCount;
            if (mode == "blackout") Check(firstAct.Phase == VS1StrongEventOnePhase.Recovery && !firstAct.AmbienceControlReleased,
                "blackout snapshot starts after the real reveal and before handoff");
            if (mode == "handoff") Check(firstAct.AmbienceControlReleased && campaignMix.OwnsAmbience,
                "handoff snapshot starts after a real chapter transition");
            if (mode == "watch") Check(!firstAct.AmbienceControlReleased, "watch begins before the one-way handoff");
            running = true; Status = "running";
        }
        catch (Exception exception) { Finish(false, exception.Message); }
    }

    private void LateUpdate()
    {
        if (!running) return;
        try
        {
            if (Time.realtimeSinceStartup - began > 1800) throw new Exception("Observation timed out before the existing route reached its required state.");
            int firstWrites = firstAct.AmbienceWriteCount, campaignWrites = campaignMix.AmbienceWriteCount;
            Require(!(firstWrites > previousFirstWrites && campaignWrites > previousCampaignWrites),
                "Both components wrote the same institutional beds during one frame.");
            previousFirstWrites = firstWrites; previousCampaignWrites = campaignWrites; frames++;
            if (firstAct.AmbienceControlReleased)
            {
                Require(campaignMix.OwnsAmbience && firstAct.AmbienceReleaseCount == 1, "Handoff is missing, duplicated or reclaimed.");
                if (!sawRelease)
                {
                    sawRelease = true; releasedWriteCount = firstWrites; stableSince = Time.time;
                    Check(true, "one-way handoff reached the campaign owner");
                }
                Require(firstWrites == releasedWriteCount, "SE1 wrote a bed after releasing control.");
            }
            else if (firstAct.Phase == VS1StrongEventOnePhase.Recovery)
            {
                Require(!campaignMix.OwnsAmbience, "Campaign mix took ownership before leaving the first act.");
                Require(circuitLights.All(light => light && (!light.enabled || !light.gameObject.activeInHierarchy || light.intensity <= .0001f)) &&
                    firstAct.FluorescentPowerFraction <= .0001f, "The completed blackout still has an energized facility light.");
                Require(fluorescent.volume <= .0001f, "The de-energized fluorescent circuit still has source gain.");
                foreach (var diffuser in circuitDiffusers)
                {
                    Require(diffuser.CurrentIntensityFactor <= .0001f, "A diffuser still considers a zero-intensity source lit.");
                    var surface = Field<Renderer>(diffuser, "surface");
                    Require(surface, "A circuit diffuser has no surface.");
                    materialState.Clear(); surface.GetPropertyBlock(materialState, Field<int>(diffuser, "materialIndex"));
                    Color emission = materialState.GetColor("_EmissionColor");
                    Color baseColor = materialState.GetColor("_BaseColor");
                    Require(Mathf.Max(emission.r, Mathf.Max(emission.g, emission.b)) <= .0001f &&
                        Mathf.Abs(baseColor.r - .065f) < .0001f && Mathf.Abs(baseColor.g - .073f) < .0001f && Mathf.Abs(baseColor.b - .07f) < .0001f,
                        "A blacked-out diffuser still has luminous emission or lit base color.");
                }
                for (int i = 0; i < beds.Length; i++)
                    if (beds[i] && beds[i] != fluorescent)
                        Require(Mathf.Abs(beds[i].volume - bedLevels[i]) <= .001f, "Distant machinery did not recover independently of the fluorescent circuit.");
                if (!sawBlackout)
                {
                    sawBlackout = true; stableSince = Time.time;
                    Check(true, "all twelve lights and their diffusers are dark, fluorescent gain is zero and distant machinery recovered");
                }
            }
            if (Time.timeScale <= 0 || stableSince < 0 || Time.time - stableSince < 1f) return;
            if (mode == "blackout" && sawBlackout || mode == "handoff" && sawRelease || mode == "watch" && sawBlackout && sawRelease)
            {
                Check(true, "observed frames contain no overlapping mix writers");
                Finish(true, "");
            }
        }
        catch (Exception exception) { Finish(false, exception.Message); }
    }

    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private void Check(bool value, string message)
    {
        Require(value, message); checks++;
        File.AppendAllText(output + ".checks.txt", "PASS " + message + "\n");
    }

    private void Finish(bool success, string error)
    {
        if (Status == "PASS" || Status.StartsWith("FAIL")) return;
        running = false; Status = success ? "PASS" : "FAIL: " + error;
        var result = new Result
        {
            status = Status, mode = mode, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, error = error,
            checks = checks, monitoredFrames = frames, firstActWrites = firstAct ? firstAct.AmbienceWriteCount : -1,
            campaignWrites = campaignMix ? campaignMix.AmbienceWriteCount : -1, releaseCount = firstAct ? firstAct.AmbienceReleaseCount : -1,
            blackoutObserved = sawBlackout, handoffObserved = sawRelease, fluorescentGain = fluorescent ? fluorescent.volume : -1,
            poweredFraction = firstAct ? firstAct.FluorescentPowerFraction : -1, elapsedRealtime = Time.realtimeSinceStartup - began,
            evidenceScope = "Passive observation of actual light/source states and per-frame mix writes during an independently driven route. No fixture, playback injection, source mutation, listening, timbre or human fear evidence. Snapshot modes cover only their named phase."
        };
        File.WriteAllText(output + ".result.json", JsonUtility.ToJson(result, true));
        if (success) Debug.Log("SE1 audio " + mode + " PASS"); else Debug.LogWarning("SE1 audio " + mode + " " + Status);
    }
}
#endif

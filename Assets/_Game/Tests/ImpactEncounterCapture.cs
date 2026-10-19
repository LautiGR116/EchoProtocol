#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEngine;

// Verification-only observer. Reads the actual composited Player view and the
// post-SessionAudio listener stream during a normal Input System route.
public sealed class ImpactEncounterCapture : MonoBehaviour
{
    readonly object sync = new object();
    readonly List<object> marks = new List<object>();
    float[] samples;
    int used, channels, rate, frame;
    bool live;
    string output, lastStep;
    StreamWriter frames;
    ThresholdCampaignProbe probe;
    MachineCoverEncounter encounter;

    public static void Begin(string directory)
    {
        var listener = FindFirstObjectByType<SessionAudio>();
        var capture = listener.gameObject.AddComponent<ImpactEncounterCapture>();
        capture.output = Path.GetFullPath(directory);
        Directory.CreateDirectory(capture.output + "/frames");
        capture.rate = AudioSettings.outputSampleRate;
        capture.samples = new float[capture.rate * 2 * 240];
        capture.live = true;
        capture.frames = new StreamWriter(capture.output + "/frames.jsonl");
        capture.encounter = FindFirstObjectByType<MachineCoverEncounter>();
        capture.StartCoroutine(capture.CaptureViews());
    }
    void OnAudioFilterRead(float[] data, int count)
    {
        lock (sync)
        {
            if (!live) return;
            channels = count;
            int n = Math.Min(data.Length, samples.Length - used);
            Array.Copy(data, 0, samples, used, n);
            used += n;
        }
    }
    double AudioTime { get { lock(sync) return channels > 0 ? (double)used / channels / rate : 0; } }
    IEnumerator CaptureViews()
    {
        float next = 0;
        while (live)
        {
            yield return new WaitForEndOfFrame();
            if(!Application.isPlaying)yield break;
            probe = probe ? probe : FindFirstObjectByType<ThresholdCampaignProbe>();
            if (!probe) continue;
            string step = probe.CurrentStep;
            if (step != lastStep)
            {
                marks.Add(new { step, seconds = AudioTime, realtime = Time.realtimeSinceStartup, listenerVolume=AudioListener.volume,
                    reduced=GetComponent<SessionAudio>().ReducedDynamics, mono=GetComponent<SessionAudio>().Speakers });
                lastStep = step;
            }
            if (probe.Status == "PASS" || probe.Status.StartsWith("FAIL")) { Save(probe.Status); yield break; }
            if (Time.realtimeSinceStartup < next) continue;
            // Retain the preparation/contact, warning/rush, retreat and failure.
            if (!(step.Contains("coverage_") || step.Contains("cover"))) continue;
            if (step == "coverage_record_first_noise" || step == "coverage_record_silence" || step == "coverage_record_return_noise") continue;
            next = Time.realtimeSinceStartup + 1f / 12;
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            string file = "frames/" + frame.ToString("D5") + ".jpg";
            File.WriteAllBytes(output + "/" + file, tex.EncodeToJPG(86));
            Destroy(tex);
            var view = Camera.main;
            frames.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new { frame=frame++, file, seconds=AudioTime, gameTime=Time.time, step, width=Screen.width, height=Screen.height, camera=view ? view.transform.position.ToString("F3") : "", pressure=encounter.Pressure01, covered=encounter.Covered, failed=encounter.Failed, intention=encounter.GetComponentInChildren<MachineCoverThreat>(true).Phase.ToString(),actor=encounter.GetComponentInChildren<MachineCoverThreat>(true).transform.position.ToString("F3") }));
            frames.Flush();
        }
    }
    void Save(string status)
    {
        lock(sync)
        {
            live = false;
            frames?.Dispose(); frames = null;
            if (channels < 1 || used == 0) throw new Exception("Listener produced no captured audio");
            using (var w = new BinaryWriter(File.Create(output + "/listener.wav")))
            {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+used*2);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);
                w.Write((short)channels);w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(used*2);
                for(int i=0;i<used;i++) w.Write((short)Mathf.RoundToInt(Mathf.Clamp(samples[i],-1,1)*32767));
            }
            File.WriteAllText(output+"/capture.json",Newtonsoft.Json.JsonConvert.SerializeObject(new { status,rate,channels,samples=used,frames=frame,marks,listenerVolume=AudioListener.volume,reduced=GetComponent<SessionAudio>().ReducedDynamics,mono=GetComponent<SessionAudio>().Speakers,scope="Actual post-SessionAudio stereo signal and composited Player view; normal input route. Device SPL, performance with capture disabled, comfort and fear require separate assessment." },Newtonsoft.Json.Formatting.Indented));
            samples = null;
        }
    }
    void OnDestroy() { if(live && !string.IsNullOrEmpty(output)) Save("interrupted"); }
}
#endif

#if ECHO_NATIVE_VERIFY && !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
// Separate QA Player only. Delivery contains neither bootstrap nor input probe.
public sealed class EchoNativeVerification : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartRequested()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-verify") < 0) return;
        var g = new GameObject("Explicit native verification runner");
        DontDestroyOnLoad(g); g.AddComponent<EchoNativeVerification>();
    }
    static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
    IEnumerator Start()
    {
        string destination = Argument("--ep-evidence");
        if (string.IsNullOrEmpty(destination)) throw new Exception("Explicit evidence destination required");
        string output = Path.GetFullPath(destination); Directory.CreateDirectory(output);
        yield return new WaitForSecondsRealtime(3);
        // Optional launch gate allows actual native-window focus to be prepared
        // before Enter. It never resumes gameplay or bypasses focus notifications.
        string ready = Argument("--ep-ready-file");
        float launchDeadline = Time.realtimeSinceStartup + 180, stable = 0;
        while (!string.IsNullOrEmpty(ready) && stable < 2)
        {
            if (Time.realtimeSinceStartup > launchDeadline)
            { File.WriteAllText(Path.Combine(output,"native-launch-timeout.txt"),"No focused ready window; no campaign was started."); Application.Quit(4); yield break; }
            stable = File.Exists(ready) && Application.isFocused ? stable + Time.unscaledDeltaTime : 0;
            yield return null;
        }
        File.WriteAllText(Path.Combine(output,"native-start.json"), JsonUtility.ToJson(new Stamp {status="starting",platform=Application.platform.ToString(),focused=Application.isFocused,version=Application.unityVersion}));
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-cover-speakers") >= 0)
        {
            var mix = FindFirstObjectByType<EchoProtocol.Presentation.SessionAudio>();
            mix.Speakers = true; mix.ReducedDynamics = true;
            File.WriteAllText(Path.Combine(output,"audio-fixture.json"),"{\"scope\":\"Explicit nonpersistent output fixture: actual mono and reduced-dynamics processor. Gameplay still uses normal controls; preferences are not saved.\",\"mono\":true,\"reduced\":true}");
        }
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-folio-ui") >= 0)
        {
            var test = new GameObject("Explicit Folio presentation verification").AddComponent<FolioPresentationProbe>();
            test.Run(output);
            while(test.Status=="running")yield return new WaitForSecondsRealtime(.25f);
            Application.Quit(test.Status=="PASS"?0:2);yield break;
        }
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-lighting") >= 0) LightingRouteObserver.Begin(output, Argument("--ep-lighting-reference"));
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-resonance") >= 0) ResonanceRouteObserver.Begin(output);
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-characters") >= 0) EmbodimentRouteObserver.Begin(output,Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-characters-no-video") < 0);
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-impact") >= 0) ImpactEncounterCapture.Begin(Path.Combine(output,"audiovisual"));
        ThresholdCampaignProbe.OutputDirectoryOverride = output;
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-cover") >= 0)
            ThresholdCampaignProbe.RunCoverage(Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-cover-failure") >= 0);
        else ThresholdCampaignProbe.Run();
        float deadline = Time.realtimeSinceStartup + 1200;
        bool accessibilityRetry = Array.IndexOf(Environment.GetCommandLineArgs(), "--ep-impact-accessibility-retry") >= 0, switched = false;
        while (Time.realtimeSinceStartup < deadline)
        {
            var p = FindFirstObjectByType<ThresholdCampaignProbe>();
            var cover = FindFirstObjectByType<EchoProtocol.Campaign.MachineCoverEncounter>();
            if(accessibilityRetry&&!switched&&cover&&cover.FailureCount>0)
            {
                var mix=FindFirstObjectByType<EchoProtocol.Presentation.SessionAudio>();mix.Speakers=true;mix.ReducedDynamics=true;switched=true;
                File.WriteAllText(Path.Combine(output,"audio-fixture.json"),"{\"scope\":\"Nonpersistent mono/reduced-dynamics output on the actual failure retry; first attack uses saved normal stereo. No gameplay state changes.\",\"mono\":true,\"reduced\":true}");
            }
            if (p && (p.Status == "PASS" || p.Status.StartsWith("FAIL")))
            {
                File.WriteAllText(Path.Combine(output,"native-player.json"),JsonUtility.ToJson(new Stamp {status=p.Status,version=Application.unityVersion,platform=Application.platform.ToString(),scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,processor=SystemInfo.processorType,graphics=SystemInfo.graphicsDeviceName,focused=Application.isFocused}));
                yield return new WaitForSecondsRealtime(.5f); Application.Quit(p.Status == "PASS" ? 0 : 2); yield break;
            }
            yield return new WaitForSecondsRealtime(.25f);
        }
        File.WriteAllText(Path.Combine(output,"native-timeout.txt"),"Bounded native test deadline exceeded"); Application.Quit(3);
    }
    [Serializable] sealed class Stamp {public string status,version,platform,scene,processor,graphics; public bool focused;}
}
#endif

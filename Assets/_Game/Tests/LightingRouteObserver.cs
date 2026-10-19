#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using UnityEngine;

// Passive QA only: normal route motion is never moved or resumed by this observer.
// Exact comparison renders temporarily borrow baseline camera poses, then restore
// them in the same synchronous call, without advancing gameplay or physics.
public sealed class LightingRouteObserver : MonoBehaviour
{
    [Serializable] public sealed class Pose { public string label, state; public Vector3 position; public Quaternion rotation; public float fov; }
    string output, reference; float nextCapture, ignoreUntil; int sequence, shadowChecks; bool finished;
    Camera view; GameSession session; ThresholdCampaignProbe probe;
    readonly List<float> frameMs = new List<float>(); readonly List<Pose> poses = new List<Pose>();
    readonly Dictionary<string,Pose> baseline = new Dictionary<string,Pose>();
    StreamWriter motion; RenderTexture target; Texture2D pixels;
    public static void Begin(string destination, string baselineDirectory = null)
    {
        var g=new GameObject("Temporary passive lighting survey");g.AddComponent<LightingRouteObserver>().Prepare(destination,baselineDirectory);
    }
    void Prepare(string destination,string baselineDirectory)
    {
        output=Path.GetFullPath(destination);reference=baselineDirectory;Directory.CreateDirectory(output);Directory.CreateDirectory(output+"/motion");Directory.CreateDirectory(output+"/paired");
        if(!string.IsNullOrEmpty(reference))foreach(var file in Directory.GetFiles(reference+"/paired","*.json")){var p=JsonUtility.FromJson<Pose>(File.ReadAllText(file));baseline[p.label]=p;}
        motion=new StreamWriter(output+"/motion.csv");motion.WriteLine("sequence,time,step,x,y,z,yaw,pitch,frameMs,state");
        target=new RenderTexture(640,360,24);pixels=new Texture2D(640,360,TextureFormat.RGB24,false);
        ThresholdCampaignProbe.ViewCaptured+=CapturePoint;
    }
    string State()
    {
        var c=FindFirstObjectByType<CampaignDirector>();var h=FindFirstObjectByType<HouseMemory>();var p=FindFirstObjectByType<PressureIsolation>();var a=FindFirstObjectByType<ContrapruebaAtmosphere>();
        return $"{c?.Chapter}|house:{h?.Active}/{h?.HasAnswered}/{h?.PowerLocation}|pressure:{p?.IsLatched}/{p?.IsDrained}|P3:{(a?a.MachineLevel.ToString("F2",CultureInfo.InvariantCulture):"absent")}";
    }
    void LateUpdate()
    {
        if(finished)return;
        probe=probe?probe:FindFirstObjectByType<ThresholdCampaignProbe>();view=view?view:Camera.main;session=session?session:FindFirstObjectByType<GameSession>();
        if(!probe||!view||!session)return;
        if(probe.Status.StartsWith("PASS")||probe.Status.StartsWith("FAIL")){Finish(probe.Status);return;}
        if(session.IsPaused)return;
        if(Time.realtimeSinceStartup>ignoreUntil)frameMs.Add(Time.unscaledDeltaTime*1000);
        if(Time.realtimeSinceStartup<nextCapture)return;nextCapture=Time.realtimeSinceStartup+1;ignoreUntil=Time.realtimeSinceStartup+.25f;
        Render(view,output+"/motion/"+sequence.ToString("D4")+".png",target,pixels);
        var t=view.transform;motion.WriteLine(string.Join(",",sequence++,Time.realtimeSinceStartup.ToString("F3",CultureInfo.InvariantCulture),probe.CurrentStep,t.position.x.ToString("F3",CultureInfo.InvariantCulture),t.position.y.ToString("F3",CultureInfo.InvariantCulture),t.position.z.ToString("F3",CultureInfo.InvariantCulture),t.eulerAngles.y.ToString("F2",CultureInfo.InvariantCulture),t.eulerAngles.x.ToString("F2",CultureInfo.InvariantCulture),(Time.unscaledDeltaTime*1000).ToString("F2",CultureInfo.InvariantCulture),State()));motion.Flush();
    }
    void CapturePoint(string label,Camera camera)
    {
        ignoreUntil=Time.realtimeSinceStartup+.5f;
        var p=new Pose{label=label,state=State(),position=camera.transform.position,rotation=camera.transform.rotation,fov=camera.fieldOfView};poses.Add(p);
        File.WriteAllText(output+"/paired/"+label+".json",JsonUtility.ToJson(p,true));
        var pos=camera.transform.position;var rot=camera.transform.rotation;var fov=camera.fieldOfView;
        var rt=new RenderTexture(1280,720,24);var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            if(baseline.TryGetValue(label,out var old)) {camera.transform.SetPositionAndRotation(old.position,old.rotation);camera.fieldOfView=old.fov;File.WriteAllText(output+"/paired/"+label+".comparison.json",JsonUtility.ToJson(old,true));}
            Render(camera,output+"/paired/"+label+".png",rt,tex);
            // Supplementary local directions at the same real approach position.
            foreach(int degrees in new[]{-90,90,180}){camera.transform.rotation=Quaternion.AngleAxis(degrees,Vector3.up)*(baseline.TryGetValue(label,out var b)?b.rotation:rot);Render(camera,output+"/paired/"+label+"-turn"+degrees+".png",rt,tex);}
        }
        finally {camera.transform.SetPositionAndRotation(pos,rot);camera.fieldOfView=fov;Destroy(rt);Destroy(tex);}
    }
    void Render(Camera camera,string path,RenderTexture rt,Texture2D tex)
    {
        var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();VerifyShadowSelection(camera);RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
        finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;}
    }
    void VerifyShadowSelection(Camera camera)
    {
        var budget=FindFirstObjectByType<LightShadowBudget>();if(!budget)return;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var all=((Light[])typeof(LightShadowBudget).GetField("lights",flags).GetValue(budget)).Where(l=>l).Distinct().ToArray();
        var desired=new HashSet<Light>(all.Where(l=>l.enabled&&l.gameObject.activeInHierarchy&&(l.transform.position-camera.transform.position).sqrMagnitude<144).OrderBy(l=>(l.transform.position-camera.transform.position).sqrMagnitude).Take(4));
        var region=(BoxCollider)typeof(LightShadowBudget).GetField("enclosedResidence",flags).GetValue(budget);
        var enclosed=(Light[])typeof(LightShadowBudget).GetField("residenceLights",flags).GetValue(budget);
        if(region&&region.bounds.Contains(camera.transform.position)&&enclosed!=null)foreach(var l in enclosed)if(l&&l.enabled&&l.gameObject.activeInHierarchy)desired.Add(l);
        foreach(var l in all)if((l.shadows==LightShadows.Soft)!=desired.Contains(l))throw new Exception("Shadow ownership differs from original selection: "+l.name);
        shadowChecks++;
    }
    void Finish(string status)
    {
        finished=true;motion?.Dispose();motion=null;frameMs.Sort();
        float Percentile(float p)=>frameMs.Count>0?frameMs[Mathf.Min(frameMs.Count-1,Mathf.FloorToInt(frameMs.Count*p))]:0;
        File.WriteAllText(output+"/survey.json",JsonUtility.ToJson(new Summary{status=status,frames=frameMs.Count,shadowSelectionsVerified=shadowChecks,motionViews=sequence,pairedViews=poses.Count,p50=Percentile(.5f),p95=Percentile(.95f),p99=Percentile(.99f),mean=frameMs.Count>0?frameMs.Average():0,device=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,scope="Passive real Player camera motion at 1 Hz. Paired renders use exact baseline camera poses plus declared supplementary camera turns; simulation does not advance during those renders. Frame times exclude paused gameplay and 0.25/0.5 seconds around capture work. This is end-to-end frame cadence, not an isolated GPU benchmark or human display approval."},true));
    }
    [Serializable] sealed class Summary{public string status,scope,device;public int frames,shadowSelectionsVerified,motionViews,pairedViews,width,height;public float p50,p95,p99,mean;}
    void OnDestroy(){ThresholdCampaignProbe.ViewCaptured-=CapturePoint;if(!finished&&output!=null)Finish("interrupted");if(target)Destroy(target);if(pixels)Destroy(pixels);}
}
#endif

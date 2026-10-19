#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using EchoProtocol.Echo;
using EchoProtocol.Campaign;
using EchoProtocol.Presentation;

// Passive character QA. A disabled, invisible reference body is an explicit
// animation fixture only; no Player pose, take, puzzle or input is changed.
public sealed class EmbodimentRouteObserver : MonoBehaviour
{
    string output; LoopController loop; GameSession session; Camera view; ThresholdCampaignProbe probe;
    EchoPlayback owner; EchoBodyRig reference; RecordedCharacterMotion referenceMotion;
    float nextFrame, pauseTime; int videoFrames, checkedFrames, pausedFrames, pauses, comparisons;
    float maxPositionError, maxRotationError, maxPauseDrift;
    bool finished, wasPaused;
    bool captureVideo=true;
    Quaternion[] frozenRotations; Vector3[] frozenPositions;
    readonly Dictionary<int,float> capturedPerTake = new Dictionary<int,float>();
    readonly List<float> frameTimes = new List<float>();
    Transform playerRoot; Collider[] transparent; readonly RaycastHit[] sight=new RaycastHit[64];
    RenderTexture target; Texture2D pixels; StreamWriter video;
    public static void Begin(string destination,bool videoCapture=true)
    {
        var g=new GameObject("Temporary recorded character verification");
        var observer=g.AddComponent<EmbodimentRouteObserver>();observer.captureVideo=videoCapture;observer.Prepare(destination);
    }
    void Prepare(string destination)
    {
        output=Path.GetFullPath(destination);Directory.CreateDirectory(output);Directory.CreateDirectory(output+"/character-motion");Directory.CreateDirectory(output+"/characters");
        loop=FindFirstObjectByType<LoopController>();session=FindFirstObjectByType<GameSession>();view=Camera.main;
        playerRoot=FindFirstObjectByType<EchoProtocol.Player.FirstPersonController>().transform;
        transparent=(Collider[])typeof(FinalDoorMemory).GetField("transparentSightColliders",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(FindFirstObjectByType<FinalDoorMemory>());
        var prefab=(EchoPlayback)typeof(LoopController).GetField("echoPrefab",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(loop);
        var g=Instantiate(prefab.gameObject);g.name="Explicit invisible pose reference";
        foreach(var r in g.GetComponentsInChildren<Renderer>(true))r.forceRenderingOff=true;
        foreach(var c in g.GetComponentsInChildren<Collider>(true))c.enabled=false;
        g.SetActive(false);reference=g.GetComponent<EchoBodyRig>();referenceMotion=reference.CharacterMotion;
        if(!referenceMotion||!referenceMotion.Initialize())throw new Exception("Articulated candidate/reference missing");
        target=new RenderTexture(960,540,24);pixels=new Texture2D(960,540,TextureFormat.RGB24,false);
        video=new StreamWriter(output+"/character-motion.csv");video.WriteLine("frame,realtime,gameTime,step,echoElapsed,subject,playerDistance,gait,turnPhase,turnRate");
        ThresholdCampaignProbe.ViewCaptured+=CapturePoint;
        File.WriteAllText(output+"/character-scope.txt","Actual normal-input route. Pixel captures use the live Player camera. Same-frame old-body comparisons are labeled visual fixtures and never replace the active Echo. An invisible inactive reference checks recorded local joint matrices. No Player placement, take substitution or automatic resume.\n");
    }
    void LateUpdate()
    {
        if(finished)return;probe=probe?probe:FindFirstObjectByType<ThresholdCampaignProbe>();if(!probe)return;
        if(probe.Status=="PASS"||probe.Status.StartsWith("FAIL")){Finish(probe.Status);return;}
        try
        {
            var echo=loop.ActiveEcho;
            if(echo&&echo.gameObject.activeInHierarchy&&echo.GetComponent<EchoBodyRig>().CharacterMotion)
            {
                var rig=echo.GetComponent<EchoBodyRig>();var motion=rig.CharacterMotion;
                if(owner!=echo){owner=echo;wasPaused=false;frozenRotations=null;}
                if(session.IsPaused)
                {
                    if(!wasPaused){pauseTime=Time.realtimeSinceStartup;wasPaused=true;frozenRotations=null;pauses++;}
                    if(Time.realtimeSinceStartup-pauseTime>.08f)
                    {
                        if(frozenRotations==null){frozenRotations=motion.Joints.Select(t=>t.localRotation).ToArray();frozenPositions=motion.Joints.Select(t=>t.localPosition).ToArray();}
                        else for(int i=0;i<motion.Joints.Length;i++)
                        {
                            float delta=Vector3.Distance(frozenPositions[i],motion.Joints[i].localPosition);maxPauseDrift=Mathf.Max(maxPauseDrift,delta);
                            if(delta>.0001f||Quaternion.Angle(frozenRotations[i],motion.Joints[i].localRotation)>.12f)throw new Exception("A recorded visual joint moved during the real menu pause.");
                        }
                        pausedFrames++;
                    }
                }
                else
                {
                    wasPaused=false;frozenRotations=null;checkedFrames++;
                    if(checkedFrames%19==0)
                    {
                        reference.transform.SetPositionAndRotation(echo.transform.position,echo.transform.rotation);
                        var pose=echo.Recording.SampleBodyPose(echo.ElapsedTime);
                        reference.Apply(new RecordedBodyPose(2.7f,3,-12,new Vector2(-3,1),7,145));
                        reference.Apply(pose);
                        var c=FindFirstObjectByType<MachineCoverEncounter>();bool held=c&&c.Active&&echo.Recording.SampleMachineGrip(echo.ElapsedTime,c.Machine.TargetId);
                        reference.ApplyMachineGrip(held);
                        for(int i=0;i<motion.Joints.Length;i++)
                        {
                            float p=Vector3.Distance(motion.Joints[i].localPosition,referenceMotion.Joints[i].localPosition);
                            float r=Quaternion.Angle(motion.Joints[i].localRotation,referenceMotion.Joints[i].localRotation);
                            maxPositionError=Mathf.Max(maxPositionError,p);maxRotationError=Mathf.Max(maxRotationError,r);
                            if(p>.0002f||r>.15f)throw new Exception("Visible recording does not match independently resampled joints: "+motion.Joints[i].name+" position "+p+" angle "+r);
                        }
                    }
                }
            }
            if(session.IsPaused)return;
            frameTimes.Add(Time.unscaledDeltaTime*1000);
            if(!captureVideo)return;
            if(Time.realtimeSinceStartup<nextFrame)return;nextFrame=Time.realtimeSinceStartup+1/15f;
            var threat=FindFirstObjectByType<MachineCoverThreat>();
            bool pursuing=threat&&threat.Released;
            Transform subject=pursuing?threat.transform:echo&&echo.gameObject.activeInHierarchy?echo.transform:null;
            if(!subject)return;
            var screen=view.WorldToViewportPoint(subject.position+Vector3.up*1.35f);
            float distance=Vector3.Distance(view.transform.position,subject.position+Vector3.up*1.35f);
            if(screen.z<=view.nearClipPlane||screen.x<-.1f||screen.x>1.1f||screen.y<-.1f||screen.y>1.1f||distance>10)return;
            if(!Visible(subject,1.35f)&&!Visible(subject,1.68f))return;
            int id=pursuing?-1:echo.GetInstanceID();capturedPerTake.TryGetValue(id,out float seconds);
            if(seconds>(pursuing?12:7))return;capturedPerTake[id]=seconds+1/15f;
            Render(view,output+"/character-motion/"+videoFrames.ToString("D5")+".png",target,pixels);
            var body=subject.GetComponentInChildren<EchoBodyRig>().AppliedPose;
            video.WriteLine(string.Join(",",videoFrames++,Time.realtimeSinceStartup.ToString("F5",CultureInfo.InvariantCulture),Time.time.ToString("F5",CultureInfo.InvariantCulture),probe.CurrentStep,echo?echo.ElapsedTime:-1,pursuing?"present_pursuer":"recorded_echo",distance,body.GaitPhase,body.TurnPhase,body.TurnRate));video.Flush();
        }
        catch(Exception e){Finish("FAIL: "+e.Message);Debug.LogError(e.ToString(),this);}
    }
    bool Visible(Transform subject,float height)
    {
        Vector3 point=subject.position+Vector3.up*height,offset=point-view.transform.position;
        Vector3 v=view.WorldToViewportPoint(point);if(v.z<=view.nearClipPlane||v.x<0||v.x>1||v.y<0||v.y>1)return false;
        int n=Physics.RaycastNonAlloc(view.transform.position,offset.normalized,sight,offset.magnitude,~0,QueryTriggerInteraction.Ignore);
        if(n>=sight.Length)return false;
        for(int i=0;i<n;i++){var c=sight[i].collider;if(c.transform.IsChildOf(subject)||c.transform.IsChildOf(playerRoot)||transparent.Contains(c))continue;return false;}
        return true;
    }
    void CapturePoint(string label,Camera camera)
    {
        if(!new[]{"P1-reader-A-playback","01-P1","Observation-B-cooperation","SE3-entry","SE3-codo","21-Echo-at-6B","22-witnessed","Contraprueba-first-presence","Contraprueba-close-return","coverage-grip-replay"}.Any(s=>label.EndsWith(s,StringComparison.Ordinal)))return;
        var echo=loop.ActiveEcho;if(!echo||!echo.gameObject.activeInHierarchy)return;
        var rt=new RenderTexture(1280,720,24);var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
        GameObject old=null;var renderers=echo.GetComponentsInChildren<Renderer>();
        try
        {
            Render(camera,output+"/characters/"+label+"-after.png",rt,tex);
#if UNITY_EDITOR
            var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Contraprueba/EchoContraprueba.prefab");
#else
            GameObject prefab=null; // Baseline needs explicit QA reference; never included in delivery solely for comparison.
#endif
            if(prefab)
            {
                old=Instantiate(prefab,echo.transform.position,echo.transform.rotation);old.name="Explicit before comparison fixture";
                foreach(var col in old.GetComponentsInChildren<Collider>())col.enabled=false;
                foreach(var skin in old.GetComponentsInChildren<SkinnedMeshRenderer>())skin.forceMatrixRecalculationPerRender=true;
                var rig=old.GetComponent<EchoBodyRig>();rig.Apply(echo.Recording.SampleBodyPose(echo.ElapsedTime));
                foreach(var r in renderers)r.forceRenderingOff=true;
                Render(camera,output+"/characters/"+label+"-before.png",rt,tex);comparisons++;
            }
        }
        finally{foreach(var r in renderers)r.forceRenderingOff=false;if(old)DestroyImmediate(old);Destroy(rt);Destroy(tex);}
    }
    static void Render(Camera camera,string path,RenderTexture rt,Texture2D tex)
    {
        var old=camera.targetTexture;var active=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;}
    }
    void Finish(string status)
    {
        finished=true;video?.Dispose();video=null;frameTimes.Sort();
        File.WriteAllText(output+"/character-result.json",JsonUtility.ToJson(new Result{status=status,checkedFrames=checkedFrames,pausedFrames=pausedFrames,pauses=pauses,videoFrames=videoFrames,comparisons=comparisons,maxPosition=maxPositionError,maxAngle=maxRotationError,pauseDrift=maxPauseDrift,frameMean=frameTimes.Count>0?frameTimes.Average():0,frameP95=frameTimes.Count>0?frameTimes[Mathf.Min(frameTimes.Count-1,(int)(frameTimes.Count*.95f))]:0,scope="Instrumented route; frame cadence includes video capture and reference work. Character matrix and pause checks are objective; human naturalness/fear are unapproved."},true));
    }
    [Serializable]sealed class Result{public string status,scope;public int checkedFrames,pausedFrames,pauses,videoFrames,comparisons;public float maxPosition,maxAngle,pauseDrift,frameMean,frameP95;}
    void OnDestroy(){ThresholdCampaignProbe.ViewCaptured-=CapturePoint;if(!finished&&output!=null)Finish("interrupted");if(reference)Destroy(reference.gameObject);if(target)Destroy(target);if(pixels)Destroy(pixels);}
}
#endif

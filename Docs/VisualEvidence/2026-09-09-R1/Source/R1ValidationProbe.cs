#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using EchoProtocol.Player;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Puzzle;

public sealed class R1ValidationProbe : MonoBehaviour
{
    const string Out = "/tmp/echo-vs1-r1/";
    FirstPersonController player; LoopController loop; VS1OmittedPassageEvent se3;
    VS1ParallelFigureEvent se2; VS1RecoveryHubGate hub; BlindAlignmentRelay p2;
    Keyboard keyboard; Mouse mouse; Key[] keys=Array.Empty<Key>(); float yaw;
    string mode,step="setup"; StreamWriter log; bool running; float nextLog;
    public string Status {get;private set;}="starting";
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    public static void Run(string mode) {var g=new GameObject("Temporary R1 validation");var p=g.AddComponent<R1ValidationProbe>();p.mode=mode;p.StartCoroutine(p.Execute());}
    void InputUpdate() {
        if(!running || InputState.currentUpdateType!=InputUpdateType.Dynamic)return;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        float delta=Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/.15f;
        InputSystem.QueueStateEvent(mouse,new MouseState {delta=new Vector2(delta,0)});
        // The Editor may lose focus to tools; permit look input exactly as a locked Game view.
        player.GetType().GetField("cursorIsLocked",Private).SetValue(player,true);
    }
    void LateUpdate() {
        if(!running)return;
        if(player.transform.position.y < -.2f) {Fail("Player fell below floor");return;}
        if(loop.ActiveEcho!=null && loop.ActiveEcho.gameObject.activeInHierarchy) {
            var e=loop.ActiveEcho; e.Recording.Sample(e.ElapsedTime,out var ep,out var er);var body=e.GetComponent<Rigidbody>();
            if(Vector3.Distance(ep,body.position)>.01f || Quaternion.Angle(er,body.rotation)>.1f) {Fail("Echo pose differs from immutable recording");return;}
            if(!ReferenceEquals(e.Recording,loop.LastRecording)){Fail("Echo snapshot replaced");return;}
        }
        if(Time.time>=nextLog) {nextLog=Time.time+.1f;var p=player.transform.position;log.WriteLine($"{Time.time:F3},{step},{p.x:F3},{p.y:F3},{p.z:F3},{player.transform.eulerAngles.y:F2},{se2.CurrentPhase},{se2.QualifiedReadSeconds:F3},{se3.CurrentPhase},{hub.IsOpen},{p2.IsEchoHoldingGate},{loop.CompletedLoopCount}");log.Flush();}
    }
    void Fail(string why){Status="FAIL "+step+": "+why; Debug.LogWarning("R1 HARNESS "+Status);File.WriteAllText(Out+mode+".result.txt",Status);running=false;keys=Array.Empty<Key>();StopAllCoroutines();}
    void Check(bool ok,string why){if(!ok)throw new Exception(step+": "+why); File.AppendAllText(Out+mode+".checks.txt","PASS "+why+"\n");}
    IEnumerator Wait(float seconds){keys=Array.Empty<Key>();yield return new WaitForSeconds(seconds);}
    IEnumerator Face(float target){yaw=target;keys=Array.Empty<Key>();float t=Time.time;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>1){if(Time.time-t>5)throw new Exception("look timeout");yield return null;}yield return new WaitForSeconds(.12f);}
    IEnumerator Go(float x,float z,float facing) {
        yield return Face(facing); float start=Time.time; Vector3 target=new Vector3(x,0,z);
        while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.10f){
            if(Time.time-start>10){keys=Array.Empty<Key>();Fail("movement timeout at "+player.transform.position+" toward "+target);yield break;}
            var local=player.transform.InverseTransformDirection(target-player.transform.position);var list=new List<Key>();
            if(local.z>.065f)list.Add(Key.W);else if(local.z<-.065f)list.Add(Key.S);if(local.x>.065f)list.Add(Key.D);else if(local.x<-.065f)list.Add(Key.A);keys=list.ToArray();yield return null;
        } keys=Array.Empty<Key>();yield return new WaitForSeconds(.12f);
    }
    IEnumerator Recall(){int count=loop.CompletedLoopCount;keys=new[]{Key.R};yield return new WaitForSeconds(.10f);keys=Array.Empty<Key>();yield return new WaitForSeconds(.35f);Check(loop.CompletedLoopCount==count+1,"real R completed once");Check(Vector3.Distance(loop.RecallSpawnPosition,new Vector3(-2.75f,.03f,22.05f))<.01f && loop.CheckpointRevision==1,"domestic checkpoint unchanged");}
    void Shot(string name) {
        var cam=player.GetComponentInChildren<Camera>();var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Out+name+".png",tex.EncodeToPNG());
        File.AppendAllText(Out+"cameras.txt",name+" position="+cam.transform.position.ToString("F3")+" euler="+cam.transform.eulerAngles.ToString("F2")+" FOV="+cam.fieldOfView+"\n");cam.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(tex);
    }
    IEnumerator Execute() {
        yield return null;
        player=FindFirstObjectByType<FirstPersonController>();loop=FindFirstObjectByType<LoopController>();se2=FindFirstObjectByType<VS1ParallelFigureEvent>();se3=FindFirstObjectByType<VS1OmittedPassageEvent>();hub=FindFirstObjectByType<VS1RecoveryHubGate>();p2=FindFirstObjectByType<BlindAlignmentRelay>();
        File.WriteAllText(Out+mode+".checks.txt","");log=new StreamWriter(Out+mode+".csv");log.WriteLine("time,step,x,y,z,yaw,se2,reading,se3,hub,echoHold,recalls");
        var one=FindFirstObjectByType<VS1StrongEventOne>();one.enabled=false;
        foreach(var l in Field<Light[]>(one,"facilityBlackoutLights"))l.intensity=0;
        foreach(var r in Field<Renderer[]>(one,"facilityBlackoutPanels")){var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);b.SetColor("_EmissionColor",Color.black);r.SetPropertyBlock(b);}
        RenderSettings.ambientLight*=.0015f;RenderSettings.ambientIntensity*=.0015f;RenderSettings.reflectionIntensity*=.0015f;
        var cp=FindFirstObjectByType<VS1DomesticThresholdCheckpoint>();cp.GetComponent<BoxCollider>().enabled=false;
        var point=Field<Transform>(cp,"recallPoint");player.ResetPose(point.position,point.rotation);loop.SetRecallCheckpoint(point);
        keyboard=InputSystem.AddDevice<Keyboard>("R1TestKeyboard");mouse=InputSystem.AddDevice<Mouse>("R1TestMouse");
        var pi=player.GetComponent<PlayerInput>();pi.neverAutoSwitchControlSchemes=true;pi.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);pi.ActivateInput();
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate+=InputUpdate;running=true;Status="running";yaw=0;
        yield return Wait(.5f);
        Check(!hub.IsOpen && !se3.IsComplete,"fresh baseline sealed");
        if(mode=="no_echo") {
            step="closed_p2";yield return Face(0);keys=new[]{Key.W};yield return new WaitForSeconds(2);keys=Array.Empty<Key>();Check(player.transform.position.z<24.7f,"closed P2 gate physically blocks Player");Check(!hub.IsOpen&&!se3.IsComplete,"incomplete P2 SE2 SE3 cannot open hub");yield return Recall();Check(!hub.IsOpen&&!se3.IsComplete,"incomplete recall retains clinical baseline");
        } else {
            step="record_hold";yield return Go(-2.75f,23.05f,90);yield return Wait(mode=="positive"?42:15);Check(p2.IsPlayerPreviewActive,"P2 Player preview");yield return Recall();
            step="echo_gate";yield return Wait(1.5f);Check(p2.IsEchoHoldingGate,"truthful Echo holds P2");yield return Go(-2.75f,29.03f,0);
            step="before_return_view";yield return Face(270);Shot(mode+"_01_before");Check(!hub.IsOpen,"outbound route remains original wall");
            // Collision against original wall before the event.
            keys=new[]{Key.W};yield return new WaitForSeconds(.6f);keys=Array.Empty<Key>();Check(player.transform.position.x>-3.75f,"baseline west wall blocks access");
            yield return Go(-2.75f,29.03f,0);yield return Go(-.45f,29.03f,90);
            step="se2_read";yield return Wait(mode=="positive"||mode=="armed"?5:.85f);
            if(mode!="positive"&&mode!="armed")Check(se2.CurrentPhase!=VS1ParallelFigureEvent.EventPhase.Complete,"SE2 reading still incomplete");
            else Check(se2.CurrentPhase==VS1ParallelFigureEvent.EventPhase.Complete,"SE2 completed through actual visible reading");
            if(mode=="armed"){step="armed_recall";yield return Recall();Check(se2.CurrentPhase==VS1ParallelFigureEvent.EventPhase.Complete&&!se3.IsComplete&&!hub.IsOpen,"completed SE2 recalled before real SE3 return keeps hub sealed");goto Finished;}
            step="return";yield return Go(-2.75f,29.03f,270);yield return Face(270);Shot(mode+"_02_after");
            if(mode!="positive"){
                Check(!hub.IsOpen&&!se3.IsComplete,"incomplete return retains baseline");yield return Recall();Check(!hub.IsOpen&&!se3.IsComplete&&!se2.IsDomesticVisible,"incomplete recall restores cold SE2 and clinical return");
            }else{
                Check(se3.IsComplete&&hub.IsOpen&&hub.OpeningCount==1,"valid SE3 return opens hub once");
                step="enter_hub";yield return Go(-5.5f,29.0f,270);yield return Face(240);Shot("R1_03_hub");
                yield return Go(-9.45f,29f,270);yield return Go(-9.45f,27.55f,180);yield return Face(270);Shot("R1_04_residual");
                step="glass_collision";yield return Face(270);keys=new[]{Key.W};yield return new WaitForSeconds(.7f);keys=Array.Empty<Key>();Check(player.transform.position.x>-10.1f,"observation glass blocks Player");
                step="around_station";yield return Go(-9.55f,26.0f,180);yield return Go(-5.4f,26.0f,90);yield return Face(45);Shot("R1_05_exit");yield return Go(-5.4f,29f,0);
                step="two_way_return";yield return Go(-2.75f,29f,90);yield return Go(-5.4f,29f,270);Check(hub.OpeningCount==1,"two-way access remains open without retrigger");
                step="completed_recall";yield return Recall();Check(hub.IsOpen&&se3.IsComplete&&se2.IsDomesticVisible,"completed R preserves hub SE3 SE2");
                yield return Face(0);keys=new[]{Key.W};yield return new WaitForSeconds(2);keys=Array.Empty<Key>();Check(player.transform.position.z<24.7f,"P2 closes after R without reader hold; no hub bypass");
            }
        }
        Finished:
        step="done";Status="PASS";File.WriteAllText(Out+mode+".result.txt","PASS\n");running=false;keys=Array.Empty<Key>();InputSystem.QueueStateEvent(keyboard,new KeyboardState());log.Flush();Debug.Log("R1 "+mode+" PASS");
    }
    void OnDestroy(){running=false;InputSystem.onBeforeUpdate-=InputUpdate;log?.Dispose();if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
}

#endif

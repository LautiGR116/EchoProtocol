#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using EchoProtocol.Player;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Puzzle;

public sealed class D1ValidationProbe : MonoBehaviour
{
    const string Out = "/tmp/echo-vs1-d1/";
    VS1DenialEvidence d1; FirstPersonController player; LoopController loop; VS1OmittedPassageEvent se3;
    VS1ParallelFigureEvent se2; VS1RecoveryHubGate hub; BlindAlignmentRelay p2;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Keyboard keyboard; Mouse mouse; Key[] keys=Array.Empty<Key>(); float yaw;
    VS1HubReturnGate r3; SlidingDoor exit, rear, front;
    VS1RecallAnchor anchor; RecombinationRelay p3; OpticalReader a,b; SlidingDoor ga,gb;
    string mode,step="setup"; StreamWriter log; bool running; float nextLog;
    public string Status {get;private set;}="starting";
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    public static void Run(string mode) {var g=new GameObject("Temporary R1 validation");var p=g.AddComponent<D1ValidationProbe>();p.mode=mode;p.StartCoroutine(p.Execute());}
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
        if((rear.IsOpenRequested && front.IsOpenRequested) || (!rear.IsFullyClosed && !front.IsFullyClosed && front.IsOpenRequested)){Fail("Arrival interlock violated");return;}
        if(player.transform.position.y < -.2f) {Fail("Player fell below floor");return;}
        if(loop.ActiveEcho!=null && loop.ActiveEcho.gameObject.activeInHierarchy) {
            var e=loop.ActiveEcho; e.Recording.Sample(e.ElapsedTime,out var ep,out var er);var body=e.GetComponent<Rigidbody>();
            if(Vector3.Distance(ep,body.position)>.01f || Quaternion.Angle(er,body.rotation)>.1f) {Fail("Echo pose differs from immutable recording");return;}
            if(!ReferenceEquals(e.Recording,loop.LastRecording)){Fail("Echo snapshot replaced");return;}
        }
        if(Time.time>=nextLog) {nextLog=Time.time+.1f;var p=player.transform.position;log.WriteLine($"{Time.time:F3},{step},{p.x:F3},{p.y:F3},{p.z:F3},{player.transform.eulerAngles.y:F2},{se2.CurrentPhase},{se2.QualifiedReadSeconds:F3},{se3.CurrentPhase},{hub.IsOpen},{p2.IsEchoHoldingGate},{loop.CompletedLoopCount}");log.Flush();}
    }
    void Fail(string why){Status="FAIL "+step+": "+why; Debug.LogWarning("R1 HARNESS "+Status);File.WriteAllText(Out+mode+".result.txt",Status);running=false;keys=Array.Empty<Key>();StopAllCoroutines();}
    void Check(bool ok,string why){if(!ok){Fail(why);throw new Exception(step+": "+why);} File.AppendAllText(Out+mode+".checks.txt","PASS "+why+"\n");}
    IEnumerator Wait(float seconds){keys=Array.Empty<Key>();yield return new WaitForSeconds(seconds);}
    IEnumerator Face(float target){yaw=target;keys=Array.Empty<Key>();float t=Time.time;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>1){if(Time.time-t>5)throw new Exception("look timeout");yield return null;}yield return new WaitForSeconds(.12f);}
    IEnumerator Go(float x,float z,float facing) {
        yield return Face(facing); float start=Time.time; Vector3 target=new Vector3(x,0,z);
        while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.10f){
            if(Time.time-start>25){keys=Array.Empty<Key>();Fail("movement timeout at "+player.transform.position+" toward "+target);yield break;}
            var local=player.transform.InverseTransformDirection(target-player.transform.position);var list=new List<Key>();
            if(local.z>.065f)list.Add(Key.W);else if(local.z<-.065f)list.Add(Key.S);if(local.x>.065f)list.Add(Key.D);else if(local.x<-.065f)list.Add(Key.A);keys=list.ToArray();yield return null;
        } keys=Array.Empty<Key>();yield return new WaitForSeconds(.12f);
    }
    IEnumerator Recall(){int count=loop.CompletedLoopCount;keys=new[]{Key.R};yield return new WaitForSeconds(.10f);keys=Array.Empty<Key>();yield return new WaitForSeconds(.15f);Check(loop.CompletedLoopCount==count+1,"real R completed once");}
    IEnumerator PressE(){keys=new[]{Key.E};yield return new WaitForSeconds(.1f);keys=Array.Empty<Key>();yield return Wait(.1f);}
    IEnumerator RecordP3(string kind,float hold){
        step="record_"+kind;yield return Go(-5.5f,32.5f,0);yield return Go(-7.8f,32.5f,270);
        yield return Go(-7.8f,35.2f,kind=="no_a"?0:90);yield return Wait(.7f);
        if(kind!="no_a")Check(a.IsPlayerAligned&&!p3.IsAuthorized,"A Player preview does not latch");
        yield return Go(-7.8f,43f,0);yield return Face(kind=="wrong_b"||kind=="no_b"?270:90);yield return Wait(hold);
        if(kind!="wrong_b"&&kind!="no_b")Check(b.IsPlayerAligned,"B Player preview acquired");
        yield return Recall();Check(!p3.IsAuthorized&&!ga.IsOpenRequested&&!gb.IsOpenRequested,"R clears both P3 gates and authorization");
        Check(loop.CheckpointRevision==2 && Vector3.Distance(loop.RecallSpawnPosition,new Vector3(-5.7f,.03f,27.8f))<.01f,"R1 checkpoint persists");
        Check(hub.IsOpen&&se3.IsComplete&&se2.IsDomesticVisible&&anchor.IsActivated,"SE2 SE3 R1 and anchor persist");
        loop.LastRecording.Sample(0,out var p,out var q);Check(p.x< -4.5f&&p.z<30.1f&&p.z>25,"Echo begins with actual R1 recording pose");
    }
    IEnumerator RecordD1(float facing,float hold){
        yield return Go(2.5f,-1,90);yield return Go(2.5f,-5.5f,180);yield return Go(-2.5f,-5.5f,270);yield return Go(-2.5f,-1,0);yield return Go(-1.3f,-1,facing);yield return Face(facing);yield return Wait(hold);
    }
    void Shot(string name) {
        if(name.StartsWith("D1_"))ScreenCapture.CaptureScreenshot(Out+name+"_Game.png");
        var cam=player.GetComponentInChildren<Camera>();var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Out+name+".png",tex.EncodeToPNG());
        File.AppendAllText(Out+"cameras.txt",name+" position="+cam.transform.position.ToString("F3")+" euler="+cam.transform.eulerAngles.ToString("F2")+" FOV="+cam.fieldOfView+"\n");cam.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(tex);
    }
    IEnumerator Execute() {
        yield return null;
        d1=FindFirstObjectByType<VS1DenialEvidence>();player=FindFirstObjectByType<FirstPersonController>();loop=FindFirstObjectByType<LoopController>();se2=FindFirstObjectByType<VS1ParallelFigureEvent>();se3=FindFirstObjectByType<VS1OmittedPassageEvent>();hub=FindFirstObjectByType<VS1RecoveryHubGate>();p2=FindFirstObjectByType<BlindAlignmentRelay>();anchor=FindFirstObjectByType<VS1RecallAnchor>(FindObjectsInactive.Include);p3=FindFirstObjectByType<RecombinationRelay>(FindObjectsInactive.Include);a=Field<OpticalReader>(p3,"readerA");b=Field<OpticalReader>(p3,"readerB");ga=Field<SlidingDoor>(p3,"routeGate");gb=Field<SlidingDoor>(p3,"finalGate");
        r3=FindFirstObjectByType<VS1HubReturnGate>();exit=Field<SlidingDoor>(r3,"returnGate");rear=Field<SlidingDoor>(r3,"arrivalRear");front=Field<SlidingDoor>(r3,"arrivalFront");
        File.WriteAllText(Out+mode+".checks.txt","");log=new StreamWriter(Out+mode+".csv");log.WriteLine("time,step,x,y,z,yaw,se2,reading,se3,hub,echoHold,recalls");
        Check(!d1.IsAvailable&&!d1.IsActivated&&!d1.HasWitnessed&&!Field<GameObject>(d1,"presentation").activeSelf,"fresh D1 unavailable and hidden");Shot("D1_00_fresh_Hub");Check(!r3.IsOpen&&exit.IsFullyClosed&&front.IsFullyClosed,"fresh R3 and Hub reverse gate solid and closed");
        var one=FindFirstObjectByType<VS1StrongEventOne>();one.enabled=false;
        foreach(var l in Field<Light[]>(one,"facilityBlackoutLights"))l.intensity=0;
        foreach(var r in Field<Renderer[]>(one,"facilityBlackoutPanels")){var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);b.SetColor("_EmissionColor",Color.black);r.SetPropertyBlock(b);}
        RenderSettings.ambientLight*=.0015f;RenderSettings.ambientIntensity*=.0015f;RenderSettings.reflectionIntensity*=.0015f;
        var cp=FindFirstObjectByType<VS1DomesticThresholdCheckpoint>();cp.GetComponent<BoxCollider>().enabled=false;
        var point=Field<Transform>(cp,"recallPoint");player.ResetPose(point.position,point.rotation);loop.SetRecallCheckpoint(point);
        keyboard=InputSystem.AddDevice<Keyboard>("D1TestKeyboard");mouse=InputSystem.AddDevice<Mouse>("D1TestMouse");
        var pi=player.GetComponent<PlayerInput>();pi.neverAutoSwitchControlSchemes=true;pi.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);pi.ActivateInput();
        oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate+=InputUpdate;running=true;Status="running";yaw=0;
        yield return Wait(.5f);
        Check(!hub.IsOpen && !se3.IsComplete&&!p3.gameObject.activeInHierarchy&&!anchor.CanInteract(player.gameObject),"fresh baseline sealed, P3 inactive and anchor rejects Player");
        step="P2_record";yield return Go(-2.75f,23.05f,90);yield return Wait(22);Check(p2.IsPlayerPreviewActive,"P2 Player preview preserved");yield return Recall();
        yield return Wait(1.5f);Check(p2.IsEchoHoldingGate,"real Echo holds P2");yield return Go(-2.75f,29.03f,0);
        Check(!hub.IsOpen,"R1 remains sealed before SE3");yield return Go(-.45f,29.03f,90);yield return Wait(5);
        Check(se2.CurrentPhase==VS1ParallelFigureEvent.EventPhase.Complete,"real SE2 visible reading complete");
        yield return Go(-2.75f,29.03f,270);Check(se3.IsComplete&&hub.IsOpen,"genuine SE3 return opens R1");
        yield return Go(-5.7f,29f,270);yield return Go(-5.7f,27.8f,270);yield return Face(270);Shot("R2_01_anchor_baseline");
        Check(anchor.CanInteract(player.gameObject),"actual Player can activate anchor after valid return");
        var before=player.transform.position;int loops=loop.CompletedLoopCount;
        yield return PressE();Check(anchor.IsActivated,"existing E ray activates physical station");
        Check(Vector3.Distance(before,player.transform.position)<.02f&&loops==loop.CompletedLoopCount,"anchor activation neither teleports nor performs RECALL");
        Check(loop.ActiveEcho==null&&loop.LastRecording==null&&player.GetComponent<EchoRecorder>().IsArmed&&player.GetComponent<EchoRecorder>().CapturedFrameCount==0,"anchor discards old Echo and history and immediately rearms empty recorder");
        yield return PressE();Check(loop.CheckpointRevision==2,"repeat E cannot recommit");Shot("R2_02_anchor_active");
        yield return RecordP3("positive",25);
        step="follow_A";yield return Go(-5.05f,34.25f,0);yield return Wait(3);
        Check(p3.IsAuthorized,"genuine Echo A authorizes route");yield return Go(-5.05f,39.5f,0);
        yield return Wait(3);Check(p3.IsEchoHoldingGate&&b.IsEchoAligned,"genuine B hold after A");
        Check(!r3.IsOpen&&exit.IsFullyClosed,"A plus B alone never opens R3 before Player crossing");
        // Actor guard exercised under a genuinely qualified P3 solution. This
        // callback test complements actual Echo-only physical crossing below.
        r3.SendMessage("OnTriggerEnter",loop.ActiveEcho.GetComponent<Collider>());
        r3.SendMessage("OnTriggerStay",loop.ActiveEcho.GetComponent<Collider>());
        Check(!r3.IsOpen,"Echo collider rejected even while genuine A/B are qualified");
        yield return Go(-5.05f,43.1f,0);Shot("R3_02_final_zone_closed");
        Check(!r3.IsOpen,"Player has not crossed final plane");
        yield return Go(-5.05f,46.35f,0);Check(r3.IsOpen&&r3.OpeningCount==1,"real Player final crossing unlocks R3 once");Check(!d1.IsAvailable,"P3 unlock alone does not activate D1");
        yield return Go(-15.5f,46.35f,270);yield return Face(270);yield return Wait(1);Shot("R3_03_gate_open");
        step="physical_return";yield return Go(-18.6f,46.35f,270);yield return Face(180);Shot("R3_07_continuous_return");yield return Go(-18.6f,-9.5f,180);yield return Go(-7.7f,-9.5f,90);yield return Go(-10f,-9.5f,270);yield return Wait(1.5f);
        Check(!front.IsOpenRequested,"early retreat leaves Hub side closed");
        yield return Go(-7f,-9.5f,90);yield return Wait(2.3f);
        Check(rear.IsFullyClosed&&front.IsFullyOpen,"arrival opens Hub only after solid rear closes");
        yield return Go(-4.15f,-9.5f,270);keys=new[]{Key.W};yield return new WaitForSeconds(.45f);keys=Array.Empty<Key>();yield return Wait(1.5f);
        Check(player.transform.position.x>-5.2f||rear.IsFullyClosed,"immediate reverse attempt never opens route beyond rear");
        yield return Go(-3.5f,-9.5f,45);yield return Face(45);Shot("R3_04_Hub_arrival");yield return Wait(2);
        Check(front.IsFullyClosed,"Hub side closes behind Player");
        Check(d1.IsAvailable&&!d1.IsActivated&&loop.CheckpointRevision==2,"actual Hub arrival enables D1 but retains R1 origin");
        step="D1_station";yield return Go(2.5f,-6,90);yield return Go(2.5f,-2.25f,0);yield return Go(.95f,-2.25f,270);yield return Face(270);Shot("D1_01_station");
        before=player.transform.position;loops=loop.CompletedLoopCount;yield return PressE();
        Check(d1.IsActivated&&loop.CheckpointRevision==3,"real E ray commits D1 station");
        Check(Vector3.Distance(before,player.transform.position)<.03f&&loops==loop.CompletedLoopCount,"D1 E neither teleports nor recalls");
        Check(loop.LastRecording==null&&loop.ActiveEcho==null,"D1 E clears old recording and Echo");yield return PressE();Check(loop.CheckpointRevision==3,"D1 repeat E cannot recommit");
        step="D1_empty";loops=loop.CompletedLoopCount;keys=new[]{Key.R};yield return new WaitForSeconds(.1f);yield return Wait(.2f);Check(loop.CompletedLoopCount==loops&&!d1.HasWitnessed,"empty R retains existing no-recording behavior");
        step="D1_premature";yield return Go(1.5f,-2.25f,90);yaw=270;yield return Recall();Check(!d1.IsReviewing&&!d1.HasWitnessed&&d1.QualifiedPauseCount==0,"premature moving R cannot deliver denial");
        Check(Vector3.Distance(player.transform.position,new Vector3(.9f,.03f,-1))<.1f,"R returns to D1 service origin");
        step="D1_wrong_facing";yield return RecordD1(270,1.7f);yaw=270;yield return Recall();Check(d1.QualifiedPauseCount==0&&!d1.HasWitnessed,"stationary wrong body orientation rejected");
        step="D1_no_pause";yield return RecordD1(90,0);yaw=270;yield return Recall();Check(d1.QualifiedPauseCount==0&&!d1.HasWitnessed,"crossing without sustained pause rejected");
        step="D1_look_away";yield return RecordD1(90,2);yaw=90;yield return Recall();Check(d1.QualifiedPauseCount>0&&d1.IsReviewing,"genuine immutable pause qualifies");float duration=loop.LastRecording.Duration;yield return Wait(duration+1);
        Check(!d1.HasWitnessed&&!d1.IsReviewing&&Mathf.Approximately(d1.AmbienceMultiplier,1),"look away lets Echo end without denial and restores ambience");
        step="D1_interrupt";yield return RecordD1(90,2);yaw=270;yield return Recall();yield return Go(1.15f,-1,270);float until=Time.time+loop.LastRecording.Duration+1;while(d1.AmbienceMultiplier>=.99f&&Time.time<until)yield return null;
        Check(d1.AmbienceMultiplier<.99f&&!d1.HasWitnessed,"visible real approach subtracts ambience before denial");yield return Recall();Check(!d1.HasWitnessed&&!d1.IsReviewing&&Mathf.Approximately(d1.AmbienceMultiplier,1),"R during approach clears review and restores ambience");
        step="D1_positive";yield return RecordD1(90,2);Shot("D1_02_real_pause");yaw=270;yield return Recall();
        Check(d1.IsReviewing&&d1.QualifiedPauseCount>0,"positive R reviews current immutable pause");
        var recorded=loop.LastRecording;recorded.Sample(0,out var actualStart,out var actualRotation);Check(Vector3.Distance(actualStart,new Vector3(.9f,.03f,-1))<.12f,"recording begins at actual D1 origin");
        var rows=new List<string>();for(int f=0;f<recorded.FrameCount;f++){var v=recorded[f];rows.Add(string.Join(",",new[]{v.Timestamp,v.Position.x,v.Position.y,v.Position.z,v.Rotation.x,v.Rotation.y,v.Rotation.z,v.Rotation.w}.Select(n=>n.ToString("R",System.Globalization.CultureInfo.InvariantCulture))));}File.WriteAllLines(Out+"actual-d1-recording.csv",rows);
        until=Time.time+recorded.Duration+1;while(!d1.HasWitnessed&&Time.time<until)yield return null;
        Check(d1.HasWitnessed&&d1.DeliveryCount==1&&Field<SD1SubtitleDisplay>(d1,"thought").IsShowing,"visible reproduced real pause delivers thought exactly once");Shot("D1_03_witness");
        Check(d1.AmbienceMultiplier<1,"ambience subtracted during visible close reproduction");yield return Wait(3);
        Check(!d1.IsReviewing&&Mathf.Approximately(d1.AmbienceMultiplier,1),"normal Echo end restores ambience");
        step="D1_five_recalls";for(int i=0;i<5;i++){yield return Go(1.3f,-1,270);yield return Recall();Check(d1.HasWitnessed&&d1.DeliveryCount==1&&loop.CheckpointRevision==3,"five R preserve witness and origin without repeat delivery");Check(!p3.IsAuthorized&&!ga.IsOpenRequested&&!gb.IsOpenRequested&&r3.IsOpen&&r3.OpeningCount==1,"D1 R preserves P3 reset and R3 once-only state");}
        step="D1_auto_timer";yield return Go(1.3f,-1,270);loops=loop.CompletedLoopCount;yield return Wait(61);
        Check(loop.CompletedLoopCount==loops+1&&d1.HasWitnessed&&d1.DeliveryCount==1&&loop.CheckpointRevision==3,"unchanged 60 second automatic R preserves D1 completion");
        Finished:
        step="done";Status="PASS";File.WriteAllText(Out+mode+".result.txt","PASS\n");running=false;keys=Array.Empty<Key>();InputSystem.QueueStateEvent(keyboard,new KeyboardState());log.Flush();Debug.Log("D1 "+mode+" PASS");
    }
    void OnDestroy(){InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;running=false;InputSystem.onBeforeUpdate-=InputUpdate;log?.Dispose();if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
}

#endif

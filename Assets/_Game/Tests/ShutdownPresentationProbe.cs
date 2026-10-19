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
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit initial pose/lighting/recording-state fixture, then real input only.
// One initial prior-act/pose fixture; physical chapter entry and E/WASD/mouse thereafter.
public sealed class ShutdownPresentationProbe : MonoBehaviour
{
    const string Output="Docs/VisualEvidence/2026-10-01-Shutdown/layout";
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    FirstPersonController player; Camera view; GameSession session; CampaignDirector campaign; PlayerInput input;
    PressureIsolation isolation; PressureIsolationAudio isolationAudio;
    Keyboard keyboard; Mouse mouse; InputDevice[] previousDevices; string previousScheme;
    bool previousAutoSwitch,oldSuppress,configured,running,finished,expectedPause;
    Transform rotor; float nextRotorSample; int resets;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    Key[] keys=Array.Empty<Key>(); float yaw=180,pitch,sensitivity,started;string runtimeError;
    readonly List<string> checks=new List<string>();
    public string Status{get;private set;}="starting";
    public static void Run(){if(!Application.isPlaying||FindFirstObjectByType<ShutdownPresentationProbe>())throw new Exception("Fresh Play required");var p=new GameObject("Temporary shutdown presentation route").AddComponent<ShutdownPresentationProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
    static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
    IEnumerator Guarded(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
    IEnumerator Execute()
    {
        Directory.CreateDirectory(Output);File.Delete(Output+"/result.txt");File.WriteAllText(Output+"/cameras.txt","One initial prior-act/lighting/pose fixture; actual physical input thereafter. Camera.Render while stopped; no human discovery, listening or frame-time claim.\n");
        player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();isolation=FindFirstObjectByType<PressureIsolation>();isolationAudio=FindFirstObjectByType<PressureIsolationAudio>();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.EndsWith("EchoProtocol_Shutdown.unity"),"authored Shutdown successor selected");
        Check(isolation.GetComponentsInChildren<TextMesh>(true).All(t=>t.GetComponent<Renderer>().sharedMaterial.shader.name=="EchoProtocol/PrintedText"),"physical label runtime atlas owners retain the selected lit ink shader");
        rotor=Field<Transform>(FindFirstObjectByType<PressureMotorRotor>(),"rotor");
        var titleRotation=rotor.localRotation;yield return Wait(.2f);
        Check(!session.HasStarted&&session.IsPaused&&Quaternion.Angle(titleRotation,rotor.localRotation)<.001f,"rotor remains still at the actual fresh title");
        File.WriteAllText(Output+"/rotor.csv","time,deltaTime,motor,pressure,angle,paused\n");
        oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();campaign.BeginExpansionProbe();
        Check(FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting(),"sole initial fixture restores completed first-act lighting");
        player.ResetPose(new Vector3(0,.03f,-26),Quaternion.Euler(0,180,0));
        Configure();player.PoseReset+=ObserveReset;Application.logMessageReceived+=ReceiveLog;running=true;Status="running";started=Time.realtimeSinceStartup;
        var runningRotation=rotor.localRotation;yield return Wait(.123f);
        Check(!isolation.Active&&isolation.MotorLevel01==1&&Quaternion.Angle(runningRotation,rotor.localRotation)>1,"actual rotor is already running before physical service entry");
        yield return Wait(.2f);Capture("00-entry");yield return Go(0,-29.6f,180);
        Check(isolation.Active&&campaign.Chapter==CampaignChapter.Pressure,"real physical entry activates isolation and its chapter");
        yield return Go(4.4f,-30.3f,90);yield return Go(4.4f,-33,180);Capture("01-approach");
        yield return Go(4.4f,-35.45f,180);yield return Aim(Field<Transform>(isolation,"gripPoint").position);yield return Wait(.2f);Capture("02-ready");
        Check(isolation.CanInteract(player.gameObject),"new oblique control can be gripped through the actual interaction ray");
        keys=new[]{Key.E};yield return new WaitForSecondsRealtime(.10f);yield return Wait(.6f);
        Check(!isolation.IsLatched&&isolation.HandleFraction<.001f,"short real E does not latch and the handle returns");
        keys=new[]{Key.E};float deadline=Time.realtimeSinceStartup+4;
        while(!isolation.IsLatched){if(Time.realtimeSinceStartup>deadline)throw new Exception("Physical grip could not finish its full arc");yield return null;}
        keys=Array.Empty<Key>();Capture("03-latched");
        Check(isolation.CommitCount==1&&isolationAudio.LatchPlayCount==1&&isolationAudio.PurgeStartCount==1,"one real sustained E owns one detent and purge");
        yield return Wait(.2f);
        expectedPause=true;keys=new[]{Key.Escape};yield return new WaitForSecondsRealtime(.10f);keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.06f);
        Check(session.IsPaused&&isolation.MotorLevel01>0&&isolation.MotorLevel01<1,"real Escape pauses the actual motor during coast-down");
        var pausedRotation=rotor.localRotation;float pausedMotor=isolation.MotorLevel01;
        yield return new WaitForSecondsRealtime(.25f);
        Check(Quaternion.Angle(pausedRotation,rotor.localRotation)<.001f&&isolation.MotorLevel01==pausedMotor,"real menu pause freezes both the rotor and its owning motor envelope");
        keys=new[]{Key.Escape};yield return new WaitForSecondsRealtime(.10f);keys=Array.Empty<Key>();expectedPause=false;
        Check(!session.IsPaused,"real Escape resumes after the declared rotor pause");
        yield return Wait(3.3f);Capture("04-stopped");
        var stoppedRotation=rotor.localRotation;yield return Wait(.25f);
        Check(Quaternion.Angle(stoppedRotation,rotor.localRotation)<.001f,"drained rotor retains its actual stop angle");
        Check(isolation.IsDrained&&isolation.MotorLevel01==0&&isolationAudio.MachineSourcesStopped,"the actual motor/pressure owners settle to stopped without synthetic state writes");
        Check(Field<Light[]>(isolation,"workLights").All(l=>!l.enabled),"the work circuit is dark after the detent");
        Check(Field<EchoProtocol.Puzzle.SlidingDoor>(isolation,"exitDoor").IsOpenRequested,"actual drainage requests opening of the physical exit");
        yield return Go(4.65f,-36.8f,180);Capture("05-aisle-entry");yield return Go(4.65f,-40.2f,180);Capture("06-between-machines");
        yield return Go(4.65f,-44,180);Capture("07-second-machine");yield return Go(0,-46,270);yield return Face(180);Capture("08-exit");Check(Field<EchoProtocol.Puzzle.SlidingDoor>(isolation,"exitDoor").IsFullyOpen,"exit finishes its original physical travel before crossing");yield return Go(0,-49.5f,180);
        Check(player.transform.position.z< -49.3f&&player.transform.position.y>-.05f,"the new physical aisle and original open exit are traversable");
        Check(resets==0,"no pose reset after the sole initial fixture");Check(string.IsNullOrEmpty(runtimeError),"no runtime error was logged during the route");Finish(true,"");
    }
    void ObserveReset()=>resets++;
    void ReceiveLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)runtimeError=message;}
    IEnumerator Aim(Vector3 target){Vector3 d=target-view.transform.position;yield return Face(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg);}
    IEnumerator Go(float x,float z,float facing)
    {
        yield return Face(facing);float deadline=Time.realtimeSinceStartup+10;
        while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.1f)
        {
            if(Time.realtimeSinceStartup>deadline)throw new Exception("Blocked at "+player.transform.position+" toward "+x+","+z);
            Vector3 local=player.transform.InverseTransformDirection(new Vector3(x,player.transform.position.y,z)-player.transform.position);var move=new List<Key>();
            if(local.z>.06f)move.Add(Key.W);else if(local.z<-.06f)move.Add(Key.S);if(local.x>.06f)move.Add(Key.D);else if(local.x<-.06f)move.Add(Key.A);keys=move.ToArray();yield return null;
        }
        keys=Array.Empty<Key>();yield return Wait(.1f);
    }
    IEnumerator Face(float target,float targetPitch=0){yaw=target;pitch=targetPitch;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch")-pitch)>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse failed to face target: actual="+view.transform.eulerAngles+" requested="+yaw+","+pitch);yield return null;}yield return Wait(.08f);}
    IEnumerator Wait(float seconds){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(seconds);}
    void Configure()
    {
        previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");
        keyboard=InputSystem.AddDevice<Keyboard>("ShutdownRouteKeyboard");mouse=InputSystem.AddDevice<Mouse>("ShutdownRouteMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;
    }
    void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,Mathf.Clamp(Field<float>(player,"pitch")-pitch,-10,10)/sensitivity)});}
    void LateUpdate(){if(!running)return;if((session.IsPaused&&!expectedPause)||player.transform.position.y<-.2f){Finish(false,"Unexpected pause or loss of floor; no hidden recovery");StopAllCoroutines();}
        if(Time.realtimeSinceStartup>=nextRotorSample){nextRotorSample=Time.realtimeSinceStartup+.04f;File.AppendAllText(Output+"/rotor.csv",string.Join(",",Time.time.ToString("F5",CultureInfo.InvariantCulture),Time.deltaTime.ToString("F5",CultureInfo.InvariantCulture),isolation.MotorLevel01.ToString("F5",CultureInfo.InvariantCulture),isolation.Pressure01.ToString("F5",CultureInfo.InvariantCulture),Field<float>(FindFirstObjectByType<PressureMotorRotor>(),"angle").ToString("F5",CultureInfo.InvariantCulture),session.IsPaused)+"\n");}
    }
    void Capture(string label)
    {
        var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(Output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}
    }
    void Finish(bool pass,string error)
    {
        if(finished)return;finished=true;Status=pass?"PASS":"FAIL "+error;float elapsed=Time.realtimeSinceStartup-started;Cleanup();
        File.WriteAllText(Output+"/checks.txt","One explicit initial chapter/lighting/pose fixture; synthetic physical input thereafter. No whole campaign, human perception/listening or performance claim.\n"+string.Join("\n",checks)+"\n"+Status);
        File.WriteAllText(Output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+" fixtures=1 laterPoseResets="+resets);Debug.Log("Shutdown layout "+Status);
    }
    void Cleanup(){running=false;if(player)player.PoseReset-=ObserveReset;Application.logMessageReceived-=ReceiveLog;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;if(previousDevices.Length>0&&!string.IsNullOrEmpty(previousScheme))input.SwitchCurrentControlScheme(previousScheme,previousDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
    void OnDestroy(){Cleanup();}
}
#endif

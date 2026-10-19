#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Audio;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit initial prior-event/room/pose fixture, then physical keyboard/mouse.
// P3 geometry/visibility/audio with real input after one declared R1 fixture; not a full campaign.
public sealed class ObservationGalleryProbe : MonoBehaviour
{
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 FirstPersonController player;Camera view;GameSession session;CampaignDirector campaign;PlayerInput input;PlayerInteractor interactor;
 VS1RecallAnchor anchor;VS1RecoveryHubGate hub;LoopController loop;
 Keyboard keyboard;Mouse mouse;InputDevice[] oldDevices;string oldScheme,output,error;
 bool oldAuto,oldSuppress,configured,running,finished;int resets;float yaw=270,pitch,sensitivity,started;
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();
 readonly List<string> checks=new List<string>();
 string mode;RecombinationRelay relay;OpticalReader a,b;SlidingDoor gateA,gateB;SlidingDoorAudio motorA,motorB;VS1HubReturnGate exit;
 EchoRecording take;EchoFrame[] immutable;bool allowPause;

 public string Status{get;private set;}="starting";
 public static void Run(string mode="survey"){if(mode!="survey"&&mode!="bypass")throw new Exception("survey or bypass required");if(!Application.isPlaying||FindFirstObjectByType<ObservationGalleryProbe>())throw new Exception("Fresh Play required");var p=new GameObject("Temporary P3 observation input route").AddComponent<ObservationGalleryProbe>();p.mode=mode;p.StartCoroutine(p.Guarded(p.Execute()));}
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
 void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
 IEnumerator Guarded(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
 IEnumerator Execute()
 {
  string edition=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace("EchoProtocol_","");
  output="Docs/VisualEvidence/2026-10-01-"+edition+"/observation-"+mode;Directory.CreateDirectory(output);File.Delete(output+"/result.txt");
  File.WriteAllText(output+"/cameras.txt","One initial R1/lighting/Player pose fixture, then real input. Survey directions authored; no first-use or listening claim.\n");
  player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();interactor=player.GetComponent<PlayerInteractor>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();loop=FindFirstObjectByType<LoopController>();
  anchor=FindFirstObjectByType<VS1RecallAnchor>(FindObjectsInactive.Include);hub=FindFirstObjectByType<VS1RecoveryHubGate>();
  relay=FindFirstObjectByType<RecombinationRelay>(FindObjectsInactive.Include);a=Field<OpticalReader>(relay,"readerA");b=Field<OpticalReader>(relay,"readerB");gateA=Field<SlidingDoor>(relay,"routeGate");gateB=Field<SlidingDoor>(relay,"finalGate");exit=FindFirstObjectByType<VS1HubReturnGate>();
  motorA=FindObjectsByType<SlidingDoorAudio>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(m=>Field<SlidingDoor>(m,"door")==gateA);
  motorB=FindObjectsByType<SlidingDoorAudio>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(m=>Field<SlidingDoor>(m,"door")==gateB);
  oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();
  Check(FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting(),"initial fixture restores completed first-act lighting");
  Field<GameObject>(hub,"hubRoot").SetActive(true);Field<GameObject>(hub,"accessRoot").SetActive(true);Field<GameObject>(hub,"domesticResidualRoot").SetActive(true);Field<GameObject>(hub,"baselineRoot").SetActive(false);
  typeof(VS1RecoveryHubGate).GetField("<IsOpen>k__BackingField",Private).SetValue(hub,true);
  player.ResetPose(new Vector3(-5.7f,.03f,27.8f),Quaternion.Euler(0,270,0));
  Configure();player.PoseReset+=ObserveReset;Application.logMessageReceived+=Log;running=true;started=Time.realtimeSinceStartup;Status="running";
  var target=anchor.GetComponentsInChildren<Collider>().Where(c=>c.enabled&&!c.isTrigger&&c.gameObject.activeInHierarchy).OrderBy(c=>Vector3.Distance(view.transform.position,c.bounds.center)).First();
  yield return Aim(target.bounds.center);Check(interactor.TryGetTarget(out var realTarget)&&ReferenceEquals(realTarget,anchor),"real camera ray reaches R1 station");yield return Press(Key.E);
  Check(anchor.IsActivated&&loop.ActiveEcho==null,"real E commits R1 and starts an empty take");
  Check(Mathf.Abs(a.transform.position.x+5.05f)<.05f&&Mathf.Abs(b.transform.position.x+5.05f)<.05f,"both recording stations occupy the east lane");
  Check(Mathf.Abs(Mathf.DeltaAngle(a.transform.eulerAngles.y,270))<.01f&&Mathf.Abs(Mathf.DeltaAngle(b.transform.eulerAngles.y,270))<.01f,"both actual readers require west-facing body yaw");
  Check(!gateA.IsOpenRequested&&!gateB.IsOpenRequested&&!motorA.IsMotorActive&&!motorB.IsMotorActive,"both mechanisms begin closed and silent");
  var ma=motorA.transform.position;var mb=motorB.transform.position;
  yield return Go(-5.5f,32.5f,0);Capture("00-entry-forward");yield return Go(-5.05f,33.3f,0);Capture("01-recording-approach");
  yield return Go(-5.05f,35.2f,0);yield return Wait(.5f);
  Check(!a.IsPlayerAligned&&!gateA.IsOpenRequested,"wrong-facing Player at A does not preview the door");Capture("02-A-forward");
  yield return Face(270);yield return Wait(.6f);Check(a.IsPlayerAligned&&!relay.IsAuthorized&&gateA.IsOpenRequested,"A Player preview opens only temporarily, without authorization");Capture("03-A-reading");
  if(mode=="bypass")
  {
   yield return Go(-5.05f,33.2f,180);yield return Go(-7.8f,33.2f,270);yield return Face(0);yield return Hold(Key.W,1.6f);
   Check(player.transform.position.z<35f&&!relay.IsAuthorized&&!exit.IsOpen,"fastest surveyed return around divider cannot cross A on Player preview");Capture("04-preview-race-blocked");
   yield return Go(-7.8f,33.2f,180);yield return Go(-5.05f,33.2f,90);yield return Go(-5.05f,35.2f,0);yield return Face(270);yield return Hold(Key.W,.9f);
   Check(player.transform.position.x>-6.1f,"A sealed glazing physically blocks lateral passage");
   yield return Go(-5.05f,39.5f,0);yield return Face(270);yield return Hold(Key.W,.9f);
   Check(player.transform.position.x>-6.0f,"opaque central body physically separates the lanes");Capture("05-core-blocked");
   yield return Go(-5.05f,43,0);yield return Face(270);yield return Wait(.6f);
   Check(b.IsPlayerAligned&&!relay.IsAuthorized&&gateB.IsOpenRequested,"B Player preview never fabricates A authorization");yield return Hold(Key.W,.9f);
   Check(player.transform.position.x>-6.1f,"B sealed glazing physically blocks lateral bypass");
   yield return Go(-5.05f,43,90);yield return Face(0);yield return Hold(Key.W,1.2f);
   Check(player.transform.position.z<44.5f&&!exit.IsOpen,"east terminal wall prevents walking behind B or reaching R3");Capture("06-terminal-blocked");
   Check(loop.LastRecording==null&&loop.ActiveEcho==null&&resets==0,"negative route used no Echo, RECALL or later fixture");
  }
  else
  {
   yield return Wait(3f);yield return Go(-5.05f,43,0);Capture("04-B-forward");yield return Aim(Field<GameObject>(b,"activeIndicator").transform.position);yield return Wait(.6f);
   Check(b.IsPlayerAligned&&!relay.IsAuthorized,"B is a real Player preview within the same recorded path");Capture("05-B-reading");yield return Wait(20f);
   int prior=loop.CompletedLoopCount;yield return Press(Key.R);Check(loop.CompletedLoopCount==prior+1&&loop.ActiveEcho&&loop.LastRecording!=null,"real R creates exactly one take");
   take=loop.LastRecording;immutable=Enumerable.Range(0,take.FrameCount).Select(i=>take[i]).ToArray();
   Check(!relay.IsAuthorized&&gateA.IsFullyClosed&&gateB.IsFullyClosed,"R resets both gates before real Echo qualification");
   yield return Go(-5.5f,32.5f,0);yield return Go(-7.8f,32.5f,270);yield return Go(-7.8f,34.2f,0);
   yield return Until(()=>a.IsEchoAligned,12,"actual recorded A never qualifies");yield return Aim(loop.ActiveEcho.transform.position+Vector3.up*1.35f);Capture("06-Echo-A-through-window");CheckSight(true,"A");
   Check(relay.IsAuthorized&&gateA.IsOpenRequested,"actual Echo A latches cooperation route");
   yield return Until(()=>gateA.IsFullyOpen,4,"A failed to finish travel");yield return Go(-7.8f,39.2f,0);yield return Aim(loop.ActiveEcho.transform.position+Vector3.up*1.35f);Capture("07-Echo-hidden-by-core");CheckSight(false,"core");
   yield return Go(-7.8f,42.05f,0);yield return Until(()=>b.IsEchoAligned,8,"actual recorded B never qualifies");yield return Aim(loop.ActiveEcho.transform.position+Vector3.up*1.35f);Capture("08-B-first-glimpse");yield return Go(-7.8f,42.6f,0);yield return Aim(loop.ActiveEcho.transform.position+Vector3.up*1.35f);Capture("09-Echo-B-near-door");CheckSight(true,"B");
   Check(relay.IsEchoHoldingGate&&gateB.IsOpenRequested&&!exit.IsOpen,"real B holds its door before Player crosses R3");
   yield return Until(()=>gateB.IsFullyOpen,4,"B failed to finish travel");
   yield return Wait(.2f);
   int startsA=motorA.MotorStartCount,startsB=motorB.MotorStartCount;int endsA=motorA.EndpointCueCount,endsB=motorB.EndpointCueCount;
   Check(startsA>0&&startsB>0&&endsA>0&&endsB>0,"both real leaves drove movement and endpoint sources");
   var e=loop.ActiveEcho;
   allowPause=true;yield return Press(Key.Escape);Check(session.IsPaused,"real Escape opens pause");float time=e.ElapsedTime;var pose=e.transform.position;yield return Wait(.25f);Check(e.ElapsedTime==time&&Vector3.Distance(e.transform.position,pose)<.001f,"pause freezes the actual separated Echo");yield return Press(Key.Escape);allowPause=false;Check(!session.IsPaused,"real Escape resumes");
   CheckTake();prior=loop.CompletedLoopCount;yield return Press(Key.R);yield return Wait(.2f);
   Check(loop.CompletedLoopCount==prior+1&&!relay.IsAuthorized&&gateA.IsFullyClosed&&gateB.IsFullyClosed,"second real R clears authorization and resets both leaves");
   Check(motorA.MotorStartCount==startsA&&motorB.MotorStartCount==startsB&&motorA.EndpointCueCount==endsA&&motorB.EndpointCueCount==endsB&&!motorA.IsMotorActive&&!motorB.IsMotorActive,"RECALL reset does not fake physical motor travel or endpoint cues");
   CheckTake();Check(resets==2,"only the two requested RECALLs reset Player pose after initial fixture");
  }
  Check(Vector3.Distance(ma,motorA.transform.position)<.0001f&&Vector3.Distance(mb,motorB.transform.position)<.0001f,"mechanism sources stay fixed to physical headers");
  Check(string.IsNullOrEmpty(error),"no gameplay errors during focused input route");Finish(true,"");
 }
 IEnumerator Press(Key k){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.06f);keys=new[]{k};yield return new WaitForSecondsRealtime(.1f);keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.12f);}
 IEnumerator Hold(Key k,float t){keys=new[]{k};yield return new WaitForSecondsRealtime(t);keys=Array.Empty<Key>();yield return Wait(.08f);}
 IEnumerator Until(Func<bool> c,float seconds,string message){keys=Array.Empty<Key>();float until=Time.realtimeSinceStartup+seconds;while(!c()){if(Time.realtimeSinceStartup>until)throw new Exception(message);yield return null;}}
 void CheckTake(){if(take==null)return;Check(take.FrameCount==immutable.Length,"take retains its frame count");for(int i=0;i<immutable.Length;i++)if(take[i].Timestamp!=immutable[i].Timestamp||take[i].Position!=immutable[i].Position||take[i].Rotation!=immutable[i].Rotation)throw new Exception("Take changed at "+i);Check(true,"every original position/yaw/timestamp remains immutable");}
 void CheckSight(bool visible,string label)
 {
  var echo=loop.ActiveEcho;Check(echo&&echo.gameObject.activeInHierarchy&&!echo.IsComplete,label+" uses live recorded Echo");
  foreach(float h in new[]{1.15f,1.65f})
  {
   Vector3 target=echo.transform.position+Vector3.up*h;Vector3 d=target-view.transform.position,vp=view.WorldToViewportPoint(target);
   var hits=Physics.RaycastAll(view.transform.position,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore).Where(x=>!x.collider.transform.IsChildOf(player.transform)&&!x.collider.transform.IsChildOf(echo.transform)).OrderBy(x=>x.distance).ToArray();
   File.AppendAllText(output+"/sightlines.txt",label+" height="+h+" hits="+string.Join(" | ",hits.Select(x=>x.collider.name))+"\n");
   bool clear=hits.All(x=>x.collider.name.IndexOf("glass",StringComparison.OrdinalIgnoreCase)>=0);
   Check(vp.z>0&&vp.x>.02f&&vp.x<.98f&&vp.y>.02f&&vp.y<.98f,label+" body sample lies inside authored camera view");
   Check(visible?clear:!clear,label+" "+h+"m sample "+(visible?"has no opaque obstruction":"is occluded by physical architecture"));
  }
 }
 void ObserveReset()=>resets++;
 void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)error=m;}
 IEnumerator Aim(Vector3 p){var d=p-view.transform.position;yield return Face(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg);}
 IEnumerator Go(float x,float z,float facing){yield return Face(facing);float deadline=Time.realtimeSinceStartup+12;while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.1f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Blocked at "+player.transform.position+" toward "+x+","+z);var d=player.transform.InverseTransformDirection(new Vector3(x,player.transform.position.y,z)-player.transform.position);var v=new List<Key>();if(d.z>.06f)v.Add(Key.W);else if(d.z<-.06f)v.Add(Key.S);if(d.x>.06f)v.Add(Key.D);else if(d.x<-.06f)v.Add(Key.A);keys=v.ToArray();yield return null;}yield return Wait(.1f);}
 IEnumerator Face(float y,float p=0){yaw=y;pitch=p;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch")-pitch)>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse target not reached");yield return null;}yield return Wait(.08f);}
 IEnumerator Wait(float t){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(t);}
 void Configure(){oldDevices=input.devices.ToArray();oldScheme=input.currentControlScheme;oldAuto=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");keyboard=InputSystem.AddDevice<Keyboard>("P3SurveyKeyboard");mouse=InputSystem.AddDevice<Mouse>("P3SurveyMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
 void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,Mathf.Clamp(Field<float>(player,"pitch")-pitch,-10,10)/sensitivity)});}
 void LateUpdate(){if(running&&((session.IsPaused&&!allowPause)||player.transform.position.y<-.2f)){Finish(false,"Unexpected pause or lost floor; no hidden recovery");StopAllCoroutines();}}
 void Capture(string label){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
 void Finish(bool pass,string reason){if(finished)return;finished=true;Status=pass?"PASS":"FAIL "+reason;float elapsed=Time.realtimeSinceStartup-started;Cleanup();File.WriteAllText(output+"/checks.txt",string.Join("\n",checks)+"\n"+Status);File.WriteAllText(output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+" initialFixtures=1 requestedRecallPoseResets="+resets);Debug.Log("P3 observation "+Status);}
 void Cleanup(){running=false;Application.logMessageReceived-=Log;if(player)player.PoseReset-=ObserveReset;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=oldAuto;if(oldDevices.Length>0&&!string.IsNullOrEmpty(oldScheme))input.SwitchCurrentControlScheme(oldScheme,oldDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
 void OnDestroy(){Cleanup();}
}
#endif

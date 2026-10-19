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
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// One explicit earned-return/lighting/pose fixture; subsequent route and pause use real input.
public sealed class ReturnArrivalProbe : MonoBehaviour
{
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 FirstPersonController player;Camera view;GameSession session;CampaignDirector campaign;PlayerInput input;
 VS1HubReturnGate gate;SlidingDoor rear,front;BoxCollider interior,clear;GameObject guide;
 Keyboard keyboard;Mouse mouse;InputDevice[] oldDevices;string oldScheme,output,error;
 bool oldAuto,oldSuppress,configured,running,finished,allowPause,retreatMode,trackingRetreat,retreatSawClosed;int resets;float yaw=180,sensitivity,started;
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();Vector3[] route;
 readonly List<string> checks=new List<string>();
 public string Status{get;private set;}="starting";
 public static void Run(Vector3[] approach,bool retreat=false)
 {
  if(!Application.isPlaying||FindFirstObjectByType<ReturnArrivalProbe>())throw new Exception("Fresh Play required");
  var p=new GameObject("Temporary return arrival input route").AddComponent<ReturnArrivalProbe>();p.route=approach;p.retreatMode=retreat;p.StartCoroutine(p.Guarded(p.Execute()));
 }
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
 void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
 IEnumerator Guarded(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
 IEnumerator Execute()
 {
  string edition=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace("EchoProtocol_","");
  output="Docs/VisualEvidence/2026-10-01-"+edition+(retreatMode?"/arrival-retreat-survey":"/arrival-input-survey");Directory.CreateDirectory(output);File.Delete(output+"/result.txt");
  File.WriteAllText(output+"/cameras.txt","One initial earned-return/lighting/pose fixture; no solved P3 claim. Subsequent input physical; viewpoints follow movement direction. Camera renders omit OnGUI.\n");
  player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();gate=FindFirstObjectByType<VS1HubReturnGate>();
  rear=Field<SlidingDoor>(gate,"arrivalRear");front=Field<SlidingDoor>(gate,"arrivalFront");interior=Field<BoxCollider>(gate,"arrivalInterior");clear=Field<BoxCollider>(gate,"arrivalClear");guide=Field<GameObject>(campaign,"returnDescentGuide");
  oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();
  Check(FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting(),"initial fixture restores completed first-act lighting");
  FindFirstObjectByType<LoopController>().enabled=false;player.GetComponent<EchoRecorder>().CancelRecording();
  typeof(VS1HubReturnGate).GetField("<IsOpen>k__BackingField",Private).SetValue(gate,true);
  Field<GameObject>(gate,"residualRoot").SetActive(true);Field<GameObject>(gate,"arrivalLightRoot").SetActive(true);
  player.ResetPose(new Vector3(-18.6f,.03f,7),Quaternion.Euler(0,180,0));
  Configure();player.PoseReset+=ObserveReset;Application.logMessageReceived+=Log;running=true;started=Time.realtimeSinceStartup;Status="running";
  var frontOrigin=front.transform.position;var rearOrigin=rear.transform.position;var forward=(frontOrigin-rearOrigin).normalized;float heading=Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg;
  var frontBody=Field<Rigidbody>(front,"doorBody");var rearBody=Field<Rigidbody>(rear,"doorBody");
  var owners=FindObjectsByType<SlidingDoorAudio>(FindObjectsSortMode.None);var rearAudio=owners.SingleOrDefault(a=>Field<SlidingDoor>(a,"door")==rear);var frontAudio=owners.SingleOrDefault(a=>Field<SlidingDoor>(a,"door")==front);
  Check(rearAudio&&frontAudio,"both moved doors retain their physical movement-audio owners");
  var sourceR=Field<AudioSource>(rearAudio,"mechanismSource");var sourceF=Field<AudioSource>(frontAudio,"mechanismSource");var sourceRPose=new Pose(sourceR.transform.position,sourceR.transform.rotation);var sourceFPose=new Pose(sourceF.transform.position,sourceF.transform.rotation);
  yield return Wait(.15f);Capture("00-lower-return");
  for(int i=0;i<route.Length;i++)
  {
   yield return Go(route[i]);Capture("01-approach-"+i);
   if(retreatMode&&i+1<route.Length){var d=route[i+1]-player.transform.position;yield return Face(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg);Capture("01-turn-"+i);}
  }
  Check(rear.IsFullyOpen&&front.IsFullyClosed&&gate.ArrivalState=="Receiving","approach receives Player through rear while Hub leaf remains closed");
  Check(!guide.activeInHierarchy,"remote unlock and approach do not announce descent");
  yield return Face(heading);Capture("02-receiving");
  yield return Go(interior.transform.TransformPoint(interior.center));
  Check(gate.ArrivalState=="SealingRear"&&!rear.IsOpenRequested,"actual interior occupancy starts rear sealing");
  yield return Face(heading);Capture("03-rear-sealing");
  allowPause=true;yield return Press(Key.Escape);Check(session.IsPaused&&!rear.IsFullyClosed&&!rear.IsFullyOpen,"real Esc pauses the interlock during rear movement");
  var rp=rearBody.position;var fp=frontBody.position;var state=gate.ArrivalState;var pp=player.transform.position;
  yield return Wait(.35f);
  Check(Vector3.Distance(rp,rearBody.position)<.0001f&&Vector3.Distance(fp,frontBody.position)<.0001f&&gate.ArrivalState==state&&Vector3.Distance(pp,player.transform.position)<.0001f,"pause freezes both physical leaves, interlock phase and Player");
  yield return Press(Key.Escape);Check(!session.IsPaused,"real Esc resumes the interlock");allowPause=false;
  yield return Until(()=>front.IsOpenRequested,5,"front release not requested");
  Check(rear.IsFullyClosed,"front release waits for actual rear closure");Capture("04-front-starts-opening");
  yield return Until(()=>front.IsFullyOpen,4,"front leaf never completed its travel");Capture("05-front-open-forward");
  Check(!guide.activeInHierarchy,"interior and front release do not prematurely reveal descent guide");
  if(retreatMode)yield return RetreatDuringClose(forward,heading);
  yield return Go(clear.transform.TransformPoint(clear.center));yield return Face(heading);Capture("06-hub-forward");
  yield return Until(()=>front.IsFullyClosed,4,"front did not close after actual Hub clear");
  Check(guide.activeInHierarchy,"actual Hub-clear arrival reveals the existing local descent guide");
  Check(!FindFirstObjectByType<VS1DenialEvidence>().enabled,"legacy denial sequence remains disabled");
  Check(sourceR.transform.position==sourceRPose.position&&Quaternion.Angle(sourceR.transform.rotation,sourceRPose.rotation)<.001f&&sourceF.transform.position==sourceFPose.position&&Quaternion.Angle(sourceF.transform.rotation,sourceFPose.rotation)<.001f,"door movement and pause retain both source world anchors");
  Check(rearAudio.MotorStartCount>=2&&frontAudio.MotorStartCount>=2,"both leaves generated actual opening and closing movement events");
  yield return Face(heading+180);var beforeBack=player.transform.position;keys=new[]{Key.W};yield return new WaitForSecondsRealtime(1f);yield return Wait(.1f);
  Check(Vector3.Dot(player.transform.position-frontOrigin,forward)>.22f&&Vector3.Distance(beforeBack,player.transform.position)>.1f&&!front.IsOpenRequested,"closed Hub leaf physically prevents reverse entry without moving the Player back");Capture("07-closed-behind");
  yield return Face(heading);yield return Go(clear.transform.TransformPoint(clear.center));Capture("08-return-facing-reception");
  Check(resets==0,"no Player pose resets after the initial return fixture");Check(string.IsNullOrEmpty(error),"no runtime error during the arrival input route");Finish(true,"");
 }
 IEnumerator RetreatDuringClose(Vector3 forward,float heading)
 {
  yield return Face(heading);trackingRetreat=true;retreatSawClosed=false;
  keys=new[]{Key.W};yield return Until(()=>gate.ArrivalState=="SealingFront",3,"forward movement did not trigger front sealing");
  // Reverse immediately: no camera render, wait or turn while crossing the descending leaf.
  keys=new[]{Key.S};var thought=Field<SD1SubtitleDisplay>(campaign,"thought");float announcementEnd=Field<float>(thought,"hideAtTime");
  Check(guide.activeInHierarchy&&Field<bool>(campaign,"availableAnnounced"),"initial Hub crossing announced descent before immediate retreat");
  var target=interior.transform.TransformPoint(interior.center);float deadline=Time.realtimeSinceStartup+3;
  while(Vector3.Dot(player.transform.position-target,forward)>.1f)
  {if(Time.realtimeSinceStartup>deadline)throw new Exception("Physical retreat did not return through the descending front leaf");yield return null;}
  yield return Wait(.05f);
  var occupancy=Field<BoxCollider>(gate,"arrivalOccupancy");
  Check(new Bounds(occupancy.center,occupancy.size).Contains(occupancy.transform.InverseTransformPoint(player.transform.position+Vector3.up)),"real backward input returns inside the rotated occupancy box");
  Check(rear.IsFullyClosed&&!rear.IsOpenRequested,"rear stays sealed while Player retreats through front");
  yield return Until(()=>retreatSawClosed&&front.IsFullyOpen&&gate.ArrivalState=="Releasing",6,"occupied vestibule did not close then reopen its front");
  trackingRetreat=false;
  Check(guide.activeInHierarchy&&Field<bool>(campaign,"availableAnnounced")&&Mathf.Approximately(announcementEnd,Field<float>(thought,"hideAtTime")),"retreat and re-release preserve guide without replaying its announcement");
  Capture("05-retreated-front-reopened");
 }
 void ObserveReset()=>resets++;
 void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)error=m;}
 IEnumerator Until(Func<bool> condition,float timeout,string message){float deadline=Time.realtimeSinceStartup+timeout;while(!condition()){if(Time.realtimeSinceStartup>deadline)throw new Exception(message);yield return null;}}
 IEnumerator Press(Key key){keys=new[]{key};yield return new WaitForSecondsRealtime(.1f);yield return Wait(.12f);}
 IEnumerator Go(Vector3 target){var delta=target-player.transform.position;yield return Face(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg);float deadline=Time.realtimeSinceStartup+18;while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(target.x,target.z))>.1f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Blocked at "+player.transform.position+" toward "+target);var d=player.transform.InverseTransformDirection(target-player.transform.position);var v=new List<Key>();if(d.z>.06f)v.Add(Key.W);else if(d.z<-.06f)v.Add(Key.S);if(d.x>.06f)v.Add(Key.D);else if(d.x<-.06f)v.Add(Key.A);keys=v.ToArray();yield return null;}yield return Wait(.1f);}
 IEnumerator Face(float y){yaw=y;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch"))>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse target not reached");yield return null;}yield return Wait(.08f);}
 IEnumerator Wait(float t){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(t);}
 void Configure(){oldDevices=input.devices.ToArray();oldScheme=input.currentControlScheme;oldAuto=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");keyboard=InputSystem.AddDevice<Keyboard>("ArrivalSurveyKeyboard");mouse=InputSystem.AddDevice<Mouse>("ArrivalSurveyMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
 void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,Mathf.Clamp(Field<float>(player,"pitch"),-10,10)/sensitivity)});}
 void LateUpdate()
 {
  if(!running)return;
  if((session.IsPaused&&!allowPause)||player.transform.position.y<-.2f){Finish(false,"Unexpected pause or lost floor; no hidden recovery");StopAllCoroutines();return;}
  if((rear.IsOpenRequested&&front.IsOpenRequested)||(!rear.IsFullyClosed&&!front.IsFullyClosed))
  {Finish(false,"Physical interlock violated between checkpoints");StopAllCoroutines();return;}
  if(trackingRetreat&&front.IsFullyClosed)retreatSawClosed=true;
 }
 void Capture(string label){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
 void Finish(bool pass,string reason){if(finished)return;finished=true;Status=pass?"PASS":"FAIL "+reason;float elapsed=Time.realtimeSinceStartup-started;Cleanup();File.WriteAllText(output+"/checks.txt",string.Join("\n",checks)+"\n"+Status);File.WriteAllText(output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+" initialFixtures=1 laterPoseResets="+resets);Debug.Log("Return arrival "+Status);}
 void Cleanup(){running=false;Application.logMessageReceived-=Log;if(player)player.PoseReset-=ObserveReset;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=oldAuto;if(oldDevices.Length>0&&!string.IsNullOrEmpty(oldScheme))input.SwitchCurrentControlScheme(oldScheme,oldDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
 void OnDestroy(){Cleanup();}
}
#endif

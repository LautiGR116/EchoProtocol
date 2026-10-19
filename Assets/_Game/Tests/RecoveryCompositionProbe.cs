#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
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
// Selects R1 presentation and interaction in isolation; not an earned first-act run.
public sealed class RecoveryCompositionProbe : MonoBehaviour
{
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 FirstPersonController player;Camera view;GameSession session;CampaignDirector campaign;PlayerInput input;PlayerInteractor interactor;
 VS1RecallAnchor anchor;VS1RecoveryHubGate hub;LoopController loop;Transform recall;
 Keyboard keyboard;Mouse mouse;InputDevice[] oldDevices;string oldScheme,output,error;
 bool oldAuto,oldSuppress,configured,running,finished;int resets;float yaw=270,pitch,sensitivity,started;
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();
 readonly List<string> checks=new List<string>();
 public string Status{get;private set;}="starting";
 public static void Run(){if(!Application.isPlaying||FindFirstObjectByType<RecoveryCompositionProbe>())throw new Exception("Fresh Play required");var p=new GameObject("Temporary R1 composition input route").AddComponent<RecoveryCompositionProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
 void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
 IEnumerator Guarded(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
 IEnumerator Execute()
 {
  string edition=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace("EchoProtocol_","");
  output="Docs/VisualEvidence/2026-10-01-"+edition+"/r1-input-survey";Directory.CreateDirectory(output);File.Delete(output+"/result.txt");
  File.WriteAllText(output+"/cameras.txt","One initial room/lighting/Player pose fixture. Subsequent input physical; camera directions authored for survey, not first-use evidence.\n");
  player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();interactor=player.GetComponent<PlayerInteractor>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();loop=FindFirstObjectByType<LoopController>();
  anchor=FindFirstObjectByType<VS1RecallAnchor>(FindObjectsInactive.Include);hub=FindFirstObjectByType<VS1RecoveryHubGate>();recall=Field<Transform>(anchor,"recallPoint");
  oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();
  Check(FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting(),"initial fixture restores completed first-act lighting");
  Field<GameObject>(hub,"hubRoot").SetActive(true);Field<GameObject>(hub,"accessRoot").SetActive(true);Field<GameObject>(hub,"domesticResidualRoot").SetActive(true);Field<GameObject>(hub,"baselineRoot").SetActive(false);
  typeof(VS1RecoveryHubGate).GetField("<IsOpen>k__BackingField",Private).SetValue(hub,true);
  player.ResetPose(new Vector3(-2.7f,.03f,27.015f),Quaternion.Euler(0,270,0));
  Configure();player.PoseReset+=ObserveReset;Application.logMessageReceived+=Log;running=true;started=Time.realtimeSinceStartup;Status="running";
  var originalRecallPosition=recall.position;var originalRecallRotation=recall.rotation;
  var door=Field<SlidingDoor>(anchor,"galleryAccess");
  Check(!anchor.IsActivated&&door.IsFullyClosed&&!Field<GameObject>(anchor,"activeDisplay").activeSelf,"survey starts with neutral station and physically closed gallery access");
  yield return Wait(.15f);Capture("00-corner");yield return Go(-4.3f,26.95f,270);Capture("01-entry-west");yield return Face(315);Capture("02-entry-northwest");
  yield return Go(-5.7f,27.8f,270);
  var target=anchor.GetComponentsInChildren<Collider>().Where(c=>c.enabled&&!c.isTrigger&&c.gameObject.activeInHierarchy).OrderBy(c=>Vector3.Distance(view.transform.position,c.bounds.center)).First();
  yield return Aim(target.bounds.center);Capture("03-ready");
  Check(interactor.TryGetTarget(out var realTarget)&&ReferenceEquals(realTarget,anchor)&&anchor.CanInteract(player.gameObject),"actual camera ray reaches the monitor and accepts E");
  int revision=loop.CheckpointRevision,recalls=loop.CompletedLoopCount;var playerPose=player.transform.position;
  keys=new[]{Key.E};yield return new WaitForSecondsRealtime(.10f);yield return Wait(.18f);
  Check(anchor.IsActivated&&door.IsOpenRequested&&Field<GameObject>(anchor,"activeDisplay").activeSelf,"actual E confirms the station and requests its existing gallery door");
  Check(loop.CheckpointRevision==revision+1&&loop.CompletedLoopCount==recalls&&Vector3.Distance(playerPose,player.transform.position)<.001f,"E commits exactly one origin without RECALL or Player placement");
  Check(Vector3.Distance(recall.position,originalRecallPosition)<.0001f&&Quaternion.Angle(recall.rotation,originalRecallRotation)<.001f,"the authored recall Transform stays fixed throughout use");
  Capture("04-confirmed");yield return Face(0);Capture("05-turn-to-gallery");
  yield return Go(-5.5f,29.35f,0);yield return Go(-5.5f,32.5f,0);Capture("06-gallery-entry");
  Check(door.IsFullyOpen,"original gallery door finishes its physical travel before passage");
  yield return Go(-7.8f,32.5f,270);yield return Go(-7.8f,35.2f,0);
  Check(player.transform.position.z>35.05f&&player.transform.position.y>-.05f,"existing P3 approach is physically reachable with no relocated floor");
  Check(resets==0,"no pose reset after the initial room fixture");Check(string.IsNullOrEmpty(error),"no runtime error during the input survey");Finish(true,"");
 }
 void ObserveReset()=>resets++;
 void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)error=m;}
 IEnumerator Aim(Vector3 p){var d=p-view.transform.position;yield return Face(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg);}
 IEnumerator Go(float x,float z,float facing){yield return Face(facing);float deadline=Time.realtimeSinceStartup+12;while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.1f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Blocked at "+player.transform.position+" toward "+x+","+z);var d=player.transform.InverseTransformDirection(new Vector3(x,player.transform.position.y,z)-player.transform.position);var v=new List<Key>();if(d.z>.06f)v.Add(Key.W);else if(d.z<-.06f)v.Add(Key.S);if(d.x>.06f)v.Add(Key.D);else if(d.x<-.06f)v.Add(Key.A);keys=v.ToArray();yield return null;}yield return Wait(.1f);}
 IEnumerator Face(float y,float p=0){yaw=y;pitch=p;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch")-pitch)>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse target not reached");yield return null;}yield return Wait(.08f);}
 IEnumerator Wait(float t){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(t);}
 void Configure(){oldDevices=input.devices.ToArray();oldScheme=input.currentControlScheme;oldAuto=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");keyboard=InputSystem.AddDevice<Keyboard>("R1SurveyKeyboard");mouse=InputSystem.AddDevice<Mouse>("R1SurveyMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
 void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,Mathf.Clamp(Field<float>(player,"pitch")-pitch,-10,10)/sensitivity)});}
 void LateUpdate(){if(running&&(session.IsPaused||player.transform.position.y<-.2f)){Finish(false,"Unexpected pause or lost floor; no hidden recovery");StopAllCoroutines();}}
 void Capture(string label){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
 void Finish(bool pass,string reason){if(finished)return;finished=true;Status=pass?"PASS":"FAIL "+reason;float elapsed=Time.realtimeSinceStartup-started;Cleanup();File.WriteAllText(output+"/checks.txt",string.Join("\n",checks)+"\n"+Status);File.WriteAllText(output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+" initialFixtures=1 laterPoseResets="+resets);Debug.Log("R1 composition "+Status);}
 void Cleanup(){running=false;Application.logMessageReceived-=Log;if(player)player.PoseReset-=ObserveReset;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=oldAuto;if(oldDevices.Length>0&&!string.IsNullOrEmpty(oldScheme))input.SwitchCurrentControlScheme(oldScheme,oldDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
 void OnDestroy(){Cleanup();}
}
#endif

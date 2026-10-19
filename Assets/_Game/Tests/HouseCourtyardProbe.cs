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
// One initial prior-house fixture, then real interactions and free exterior traversal. No whole-campaign claim.
public sealed class HouseCourtyardProbe : MonoBehaviour
{
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 FirstPersonController player;Camera view;GameSession session;CampaignDirector campaign;PlayerInput input;PlayerInteractor interactor;
 HouseMemory house;HouseSwingDoor exit;FinalDoorMemory review;CampaignRoomTone tone;bool hall;
 Keyboard keyboard;Mouse mouse;InputDevice[] oldDevices;string oldScheme,output,error;
 bool oldAuto,oldSuppress,configured,running,finished,expectedPause;
 CourtyardAir air;AudioSource airSource;Vector3 airAnchor;int resets;float yaw=270,pitch,sensitivity,started;
 InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();
 readonly List<string> checks=new List<string>();
 public string Status{get;private set;}="starting";
 public static void Run(bool leaveHallOn=true){if(!Application.isPlaying||FindFirstObjectByType<HouseCourtyardProbe>())throw new Exception("Fresh Play required");var p=new GameObject("Temporary real courtyard route").AddComponent<HouseCourtyardProbe>();p.hall=leaveHallOn;p.StartCoroutine(p.Guarded(p.Execute()));}
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
 void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
 IEnumerator Guarded(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
 IEnumerator Execute()
 {
  string edition=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace("EchoProtocol_","");
  output="Docs/VisualEvidence/2026-10-01-"+edition+"/courtyard-"+(hall?"hall":"bedroom");Directory.CreateDirectory(output);File.Delete(output+"/result.txt");
  File.WriteAllText(output+"/cameras.txt","One initial prior-house/lighting/Player pose fixture. Then actual input. Both branches answer phone, get key and choose the actual reserve location. View directions authored for survey, not spontaneous recognition.\n");
  player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();interactor=player.GetComponent<PlayerInteractor>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();
  house=FindFirstObjectByType<HouseMemory>();exit=Field<HouseSwingDoor>(house,"exitDoor");review=FindFirstObjectByType<FinalDoorMemory>();tone=FindFirstObjectByType<CampaignRoomTone>();air=FindFirstObjectByType<CourtyardAir>();if(air){airSource=Field<AudioSource>(air,"exteriorSource");airAnchor=airSource.transform.position;}
  oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();campaign.PrepareHouseProbe();
  Check(FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting(),"initial fixture restores completed first-act lighting");
  player.ResetPose(new Vector3(0,.03f,-49.5f),Quaternion.Euler(0,180,0));yaw=180;
  Configure();player.PoseReset+=ObserveReset;Application.logMessageReceived+=Log;running=true;started=Time.realtimeSinceStartup;Status="running";
  Check(GameObject.Find("Courtyard / real house exterior"),"real courtyard successor is active");
  yield return UseTarget(FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s=>s.Kind==StationKind.HouseOrigin));
  Check(house.Active&&!review.Active,"actual E enters the house before review");
  if(air)Check(air.AppliedGain==0,"dedicated outside air is silent inside the real house");
  yield return Go(0,-52.3f,180);yield return Go(-2,-52.3f,270);yield return Go(-3.45f,-51.9f,270);yield return Use(HouseInteractionKind.Telephone);yield return Wait(.8f);
  Check(house.HasAnswered,"actual E answers and removes the general supply");
  yield return Go(-2,-52.3f,90);yield return Go(0,-52.3f,90);
  if(!hall){yield return Go(0,-50.8f,0);yield return Use(HouseInteractionKind.HallSocket);Check(house.PowerLocation==HousePowerLocation.Carried,"actual E removes the sole hall reserve");}
  yield return Go(0,-53.3f,180);yield return Go(1.8f,-53.3f,90);yield return Go(1.8f,-50.8f,0);
  if(!hall){yield return Go(4.6f,-51.25f,90);yield return Go(4.6f,-53,180);yield return Use(HouseInteractionKind.BedroomSocket);yield return Go(4.6f,-51.25f,0);}
  yield return Go(3.2f,-50.8f,270);yield return Use(HouseInteractionKind.Key);Check(house.HasKey,"actual E takes the original key");
  yield return Go(1.8f,-50.8f,270);yield return Go(1.8f,-53.3f,180);yield return Go(0,-53.3f,270);yield return Go(0,-61.1f,180);
  yield return Use(HouseInteractionKind.ExitDoor);yield return Until(()=>exit.IsFullyOpen,8,"house exit failed physical opening");
  Check(house.HasExited&&house.PowerLocation==(hall?HousePowerLocation.Hall:HousePowerLocation.Bedroom)&&house.HallLightLeftOn==hall,"departure preserves the real chosen room and light state");
  yield return Go(0,-63.9f,180);Capture("00-exit-forward");yield return Face(270);Capture("01-south-turn");
  yield return Go(-6.3f,-63.9f,270);yield return Go(-6.85f,-60,0);Capture("02-west-approach");
  if(air){yield return Wait(.3f);Check(air.RegionGain>.99f&&!air.DirectPathBlocked&&Mathf.Abs(air.AppliedGain-.16f)<.001f,"one actual outside source has clear path and full regional gain after the corner");
   expectedPause=true;yield return Press(Key.Escape);yield return Until(()=>session.IsPaused,2,"actual Escape did not pause exterior");
   float volume=air.AppliedGain;int sample=airSource.timeSamples;yield return Wait(.35f);Check(Mathf.Abs(air.AppliedGain-volume)<.00001f&&airSource.timeSamples==sample,"actual pause freezes exterior gain and audio cursor");
   yield return Press(Key.Escape);yield return Until(()=>!session.IsPaused,2,"actual Escape did not resume exterior");expectedPause=false;
   yield return Face(90);yield return Wait(.25f);Check(airSource.transform.position==airAnchor&&Mathf.Abs(air.AppliedGain-.16f)<.001f,"turning the real Player does not move or repan an artificial source anchor");yield return Face(0);}

  yield return Go(-6.85f,-54.2f,0);Capture("03-window-enters-forward");yield return Go(-6.85f,-52.3f,0);yield return Aim(new Vector3(.91f,.95f,-52.27f));Capture("04-real-hall-through-window");
  Check(house.HallLightLeftOn==hall&&house.Active&&!review.Active,"outside view retains the actual house circuit and has no new gaze gate");
  Vector3 d=new Vector3(.91f,.95f,-52.27f)-view.transform.position;
  File.WriteAllText(output+"/sightline.txt",string.Join("\n",Physics.RaycastAll(view.transform.position,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Select(h=>h.collider.name+" at "+h.point))+"\nRendered view must be inspected; ray hits alone are not visual proof.\n");
  yield return Go(-6.85f,-52.3f,0);keys=new[]{Key.W};yield return new WaitForSecondsRealtime(.9f);yield return Wait(.1f);
  Check(player.transform.position.z< -50.65f,"real north boundary prevents leaving the courtyard floor");Capture("05-north-boundary");
  // Walk back through the same physical opening before entering review; no relocation.
  yield return Go(-6.85f,-60,180);yield return Go(-6.3f,-63.9f,180);yield return Go(0,-63.9f,90);yield return Go(0,-61.1f,0);
  Check(house.Active&&exit.IsFullyOpen&&house.HallLightLeftOn==hall,"free return to the original house remains possible before review");
  if(air){yield return Wait(.3f);Check(air.AppliedGain==0&&air.RegionGain==0,"free return mutes exterior source inside the house without changing house owners");}
  yield return Go(0,-63.9f,180);yield return Go(-6.3f,-63.9f,270);yield return Go(-6.85f,-60,0);yield return Go(-6.85f,-52.3f,0);yield return Face(270);Capture("06-porter-approach");
  yield return Go(-8.4f,-52.4f,270);
  var origin=Field<Transform>(review,"origin");var rotation=origin.rotation*Quaternion.Euler(0,-180,0);var target=origin.position+rotation*new Vector3(0,0,.3f);
  yield return Go(target.x,target.z,270);var before=player.transform.position;
  yield return UseTarget(FindObjectsByType<FinalDoorInteraction>(FindObjectsSortMode.None).Single(i=>i.Kind==FinalDoorInteractionKind.BeginReview));
  Check(review.Active&&!house.Active&&Vector3.Distance(before,player.transform.position)<.025f,"actual moved porter station begins review without Player placement");
  yield return Wait(.8f);var region=Field<BoxCollider>(tone,"finalReviewRegion");
  Check(region&&new Bounds(region.center,region.size).Contains(region.transform.InverseTransformPoint(player.transform.position+Vector3.up)),"actual moved final origin lies in authored local-frame ambience volume");
  var beds=Field<AudioSource[]>(tone,"institution");var levels=Field<float[]>(tone,"levels");var isolated=Field<AudioSource[]>(tone,"isolatedMotorBeds");var first=FindFirstObjectByType<VS1StrongEventOne>();int observed=0;
  for(int i=0;i<beds.Length;i++)if(beds[i]&&levels[i]>.001f&&!isolated.Contains(beds[i])&&beds[i]!=first.FluorescentCircuitSource){Check(Mathf.Abs(beds[i].volume-levels[i]*.20f)<.001f,"moved review preserves the institutional level for "+beds[i].name);observed++;}
  Check(observed>0,"at least one actual institutional room-air source was measured");
  Check(house.HallLightLeftOn==hall,"review commits the same real domestic light consequence");if(air)Check(air.AppliedGain==0&&airSource.transform.position==airAnchor,"moved review is outside the exterior source region");Capture("07-relocated-review-origin");
  Check(resets==0,"no pose reset after initial fixture");Check(string.IsNullOrEmpty(error),"no gameplay error in courtyard route");Finish(true,"");
 }
 IEnumerator Use(HouseInteractionKind kind){yield return UseTarget(FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(i=>i.Kind==kind));}
 IEnumerator UseTarget(Component component)
 {
  var c=component.GetComponentsInChildren<Collider>().Where(v=>v.enabled&&v.gameObject.activeInHierarchy&&!v.isTrigger).OrderBy(v=>Vector3.Distance(view.transform.position,v.bounds.center)).FirstOrDefault();
  yield return Aim(c?c.bounds.center:component.transform.position);var expected=component as IInteractable;
  Check(expected!=null&&interactor.TryGetTarget(out var target)&&ReferenceEquals(target,expected)&&expected.CanInteract(player.gameObject),"actual ray authorizes "+component.name);
  yield return Press(Key.E);
 }
 IEnumerator Press(Key k){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.06f);keys=new[]{k};yield return new WaitForSecondsRealtime(.1f);yield return Wait(.12f);}
 IEnumerator Until(Func<bool> c,float seconds,string why){keys=Array.Empty<Key>();float end=Time.realtimeSinceStartup+seconds;while(!c()){if(Time.realtimeSinceStartup>end)throw new Exception(why);yield return null;}}
 void ObserveReset()=>resets++;
 void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)error=m;}
 IEnumerator Aim(Vector3 p){var d=p-view.transform.position;yield return Face(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg);}
 IEnumerator Go(float x,float z,float facing){yield return Face(facing);float deadline=Time.realtimeSinceStartup+12;while(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(x,z))>.1f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Blocked at "+player.transform.position+" toward "+x+","+z);var d=player.transform.InverseTransformDirection(new Vector3(x,player.transform.position.y,z)-player.transform.position);var v=new List<Key>();if(d.z>.06f)v.Add(Key.W);else if(d.z<-.06f)v.Add(Key.S);if(d.x>.06f)v.Add(Key.D);else if(d.x<-.06f)v.Add(Key.A);keys=v.ToArray();yield return null;}yield return Wait(.1f);}
 IEnumerator Face(float y,float p=0){yaw=y;pitch=p;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch")-pitch)>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse target not reached");yield return null;}yield return Wait(.08f);}
 IEnumerator Wait(float t){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(t);}
 void Configure(){oldDevices=input.devices.ToArray();oldScheme=input.currentControlScheme;oldAuto=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");keyboard=InputSystem.AddDevice<Keyboard>("CourtyardKeyboard");mouse=InputSystem.AddDevice<Mouse>("CourtyardMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
 void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,Mathf.Clamp(Field<float>(player,"pitch")-pitch,-10,10)/sensitivity)});}
 void LateUpdate(){if(running&&((session.IsPaused&&!expectedPause)||player.transform.position.y<-.2f)){Finish(false,"Unexpected pause or lost floor; no hidden recovery");StopAllCoroutines();}}
 void Capture(string label){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
 void Finish(bool pass,string reason){if(finished)return;finished=true;Status=pass?"PASS":"FAIL "+reason;float elapsed=Time.realtimeSinceStartup-started;Cleanup();File.WriteAllText(output+"/checks.txt",string.Join("\n",checks)+"\n"+Status);File.WriteAllText(output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+" initialFixtures=1 laterPoseResets="+resets);Debug.Log("House courtyard "+Status);}
 void Cleanup(){running=false;Application.logMessageReceived-=Log;if(player)player.PoseReset-=ObserveReset;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=oldAuto;if(oldDevices.Length>0&&!string.IsNullOrEmpty(oldScheme))input.SwitchCurrentControlScheme(oldScheme,oldDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
 void OnDestroy(){Cleanup();}
}
#endif

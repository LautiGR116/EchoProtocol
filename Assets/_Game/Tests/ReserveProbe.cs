#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Focused visual ownership and input evidence. Positions/prior chapter are explicit fixtures.
public sealed class ReserveProbe:MonoBehaviour
{
 const string Output="Docs/VisualEvidence/2026-10-01-Reserve";
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 readonly List<string> checks=new List<string>();int poseFixtures;HouseMemory house;CampaignDirector campaign;GameSession session;FirstPersonController player;PlayerInput input;PlayerInteractor interactor;Camera view;GameObject carried;CarriedReservePresentation presenter;
 Keyboard keyboard;Mouse mouse;InputDevice[] previousDevices;string previousScheme;bool previousAutoSwitch,oldSuppress,running,configured;InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();float mousePitch;public string Status{get;private set;}="starting";
 public static void Run(){if(!Application.isPlaying)throw new Exception("Fresh Play required");var p=new GameObject("Temporary reserve verification").AddComponent<ReserveProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
 void Check(bool ok,string message){if(!ok)throw new Exception(message);checks.Add("PASS "+message);}
 IEnumerator Guarded(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
 IEnumerator Execute()
 {
  Directory.CreateDirectory(Output);house=FindFirstObjectByType<HouseMemory>();campaign=FindFirstObjectByType<CampaignDirector>();session=FindFirstObjectByType<GameSession>();player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();interactor=player.GetComponent<PlayerInteractor>();carried=Field<GameObject>(house,"carriedFuse");presenter=carried.GetComponent<CarriedReservePresentation>();
  Check(carried&&presenter&&carried.transform.parent==view.transform,"actual camera has the explicit carried representation and presenter");
  Check(!carried.activeInHierarchy&&!carried.GetComponentsInChildren<Collider>(true).Any(),"carried model starts hidden and contains no collider");
  Check(carried.GetComponentsInChildren<Light>(true).Length==0&&carried.GetComponentsInChildren<AudioSource>(true).Length==0,"carried unit adds no free light or transport audio source");
  Check(carried.GetComponentsInChildren<Renderer>(true).All(r=>r.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.Off&&r.gameObject.layer==2),"carried surfaces cast no giant shadow and remain on Ignore Raycast layer");
  oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();campaign.PrepareHouseProbe();house.EnterHouse();house.Use(HouseInteractionKind.Telephone,player.gameObject,Vector3.zero);
  Configure();running=true;yield return Pose(new Vector3(0,.03f,-50.8f),90);yield return Aim(Target(HouseInteractionKind.HallSocket));Capture("01-hall-occupied");
  yield return Use(HouseInteractionKind.HallSocket);CheckState(HousePowerLocation.Carried);yield return Wait(.2f);Capture("02-hall-empty-carried");
  session.SetPaused(true);var pos=carried.transform.localPosition;yield return Wait(.2f);Check(carried.activeInHierarchy&&Vector3.Distance(pos,carried.transform.localPosition)<.0001f,"pause preserves the held reserve and its presentation pose");session.BeginOrResume();
  // A clear centre-room view exposes the actual holding composition.
  yield return Pose(new Vector3(0,.03f,-54.5f),180);yield return Wait(.3f);Check(presenter.LoweredFraction<.05f,"clear view permits the authored carrying pose");Capture("03-carried-clear");
  // Move beside the east kitchen partition; forward/side obstruction lowers the prop.
  yield return Pose(new Vector3(4.80f,.03f,-53.8f),90);yield return Wait(.35f);Check(presenter.LoweredFraction>.60f,"nearby solid wall lowers the grip without changing reserve state");CheckState(HousePowerLocation.Carried);Capture("04-carried-at-wall");
  yield return Pose(new Vector3(-4.2f,.03f,-57.5f),270);yield return Use(HouseInteractionKind.KitchenSocket);CheckState(HousePowerLocation.Kitchen);Capture("05-kitchen-occupied");yield return Use(HouseInteractionKind.KitchenSocket);CheckState(HousePowerLocation.Carried);
  yield return Pose(new Vector3(4.6f,.03f,-53.45f),90);yield return Use(HouseInteractionKind.BedroomSocket);CheckState(HousePowerLocation.Bedroom);Capture("06-bedroom-occupied");yield return Use(HouseInteractionKind.BedroomSocket);CheckState(HousePowerLocation.Carried);
  house.Use(HouseInteractionKind.Key,player.gameObject,Vector3.zero);house.Use(HouseInteractionKind.ExitDoor,player.gameObject,Vector3.zero);house.LeaveHouse();Check(carried.activeInHierarchy&&house.PowerLocation==HousePowerLocation.Carried,"leaving the house keeps ownership and carried representation");
  // Fresh-review restoration API explicitly preserves the carried choice without replaying contacts.
  house.RestoreAtReview(true,HousePowerLocation.Carried);CheckState(HousePowerLocation.Carried);Check(!house.Active&&house.HasKey&&house.HasExited,"review restoration preserves earned state while the house remains inactive");
  var review=FindFirstObjectByType<FinalDoorMemory>();yield return Pose(new Vector3(0,.03f,-76),180);Check(review.EnterReview(),"explicit final origin fixture accepts the earned key");
  keys=new[]{Key.W};yield return new WaitForSecondsRealtime(.35f);keys=Array.Empty<Key>();yield return Wait(.2f);yield return Press(Key.R,.08f);yield return Wait(.12f);
  var loop=FindFirstObjectByType<LoopController>();Check(loop.LastRecording!=null&&loop.ActiveEcho,"actual W/R makes an ordinary real take while carrying");
  Check(carried.activeInHierarchy&&house.PowerLocation==HousePowerLocation.Carried,"real final RECALL preserves current Player reserve ownership");
  Check(loop.ActiveEcho.GetComponentsInChildren<CarriedReservePresentation>(true).Length==0&&!loop.ActiveEcho.GetComponentsInChildren<Transform>(true).Any(t=>t.name.Contains("ReservePack")||t.name.Contains("Reserve current Player")),"Echo does not duplicate the present Player carried reserve or hand");Capture("07-final-carried");Finish(true,"");
 }
 void CheckState(HousePowerLocation expected){Check(house.PowerLocation==expected,"reserve state is "+expected);int count=0;string[] fields={"hallFuse","kitchenFuse","bedroomFuse","carriedFuse"};for(int i=0;i<fields.Length;i++){var g=Field<GameObject>(house,fields[i]);Check(g&&g.activeInHierarchy==(i==(int)expected),fields[i]+" follows the single owner");if(g.activeInHierarchy)count++;}Check(count==1,"exactly one physical reserve exists");}
 Component Target(HouseInteractionKind kind)=>FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(h=>h.Kind==kind);
 IEnumerator Pose(Vector3 p,float yaw){player.ResetPose(p,Quaternion.Euler(0,yaw,0));poseFixtures++;yield return Wait(.18f);}
 IEnumerator Aim(Component c){var collider=c.GetComponent<Collider>();var dir=collider.bounds.center-view.transform.position;float desired=-Mathf.Atan2(dir.y,new Vector2(dir.x,dir.z).magnitude)*Mathf.Rad2Deg;mousePitch=(Field<float>(player,"pitch")-desired)/Field<float>(player,"lookSensitivity");yield return Wait(.18f);}
 IEnumerator Use(HouseInteractionKind kind){var c=Target(kind);yield return Aim(c);Check(interactor.TryGetTarget(out var target)&&ReferenceEquals(target,c),"actual center ray reaches "+kind);Check(target.CanInteract(player.gameObject),"real Player can use "+kind);yield return Press(Key.E,.09f);yield return Wait(.18f);}
 void Capture(string name){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Output+"/"+name+".png",t.EncodeToPNG());}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
 void Configure(){previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;keyboard=InputSystem.AddDevice<Keyboard>("ReserveTestKeyboard");mouse=InputSystem.AddDevice<Mouse>("ReserveTestMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
 void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(0,mousePitch)});mousePitch=0;}
 IEnumerator Wait(float seconds){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(seconds);}
 IEnumerator Press(Key key,float seconds){keys=new[]{key};yield return new WaitForSecondsRealtime(seconds);keys=Array.Empty<Key>();yield return null;}
 void Finish(bool pass,string error){Status=pass?"PASS":"FAIL "+error;Cleanup();File.WriteAllText(Output+"/reserve-checks.txt","Scope: explicit prior house/answer/key/exit/final-review API fixtures and "+poseFixtures+" pose fixtures. Actual E/W/R Input System actions; not a continuous playthrough, human acceptance or listening evidence.\n"+string.Join("\n",checks)+"\n"+Status);Debug.Log("Reserve probe "+Status);}
 void Cleanup(){running=false;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;if(previousDevices.Length>0&&!string.IsNullOrEmpty(previousScheme))input.SwitchCurrentControlScheme(previousScheme,previousDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
 void OnDestroy(){Cleanup();}
}
#endif

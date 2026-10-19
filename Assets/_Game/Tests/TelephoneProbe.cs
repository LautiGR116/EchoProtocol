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

// Focused interaction and presentation evidence, with an explicit prior-act/pose fixture.
public sealed class TelephoneProbe:MonoBehaviour
{
    const string Output="Docs/VisualEvidence/2026-10-01-Offhook";
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    readonly List<string> checks=new List<string>();
    HouseTelephone phone;HouseMemory house;CampaignDirector campaign;GameSession session;FirstPersonController player;HouseInteraction interaction;PlayerInput input;PlayerInteractor interactor;Camera view;
    Keyboard keyboard;Mouse mouse;InputDevice[] previousDevices;string previousScheme;bool previousAutoSwitch,oldSuppress,running,configured;InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    Key[] keys=Array.Empty<Key>();float mousePitch;public string Status{get;private set;}="starting";
    public static void Run(){if(!Application.isPlaying)throw new Exception("Fresh Play required");var p=new GameObject("Temporary telephone verification").AddComponent<TelephoneProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
    static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    void Check(bool ok,string message){if(!ok)throw new Exception(message);checks.Add("PASS "+message);}
    IEnumerator Guarded(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
    IEnumerator Execute()
    {
        Directory.CreateDirectory(Output);phone=FindFirstObjectByType<HouseTelephone>();house=FindFirstObjectByType<HouseMemory>();campaign=FindFirstObjectByType<CampaignDirector>();session=FindFirstObjectByType<GameSession>();player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();interactor=player.GetComponent<PlayerInteractor>();interaction=FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(h=>h.Kind==HouseInteractionKind.Telephone);
        Check(phone&&phone.enabled&&house&&campaign&&session,"explicit telephone runtime is configured and enabled");
        Check(!phone.IsOffHook&&!phone.IsMoving&&!house.HasAnswered,"fresh telephone is on hook and unanswered");
        Check(!phone.CanUse(player.gameObject),"title and inactive chapter reject telephone use");
        oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();campaign.PrepareHouseProbe();house.EnterHouse();
        player.ResetPose(new Vector3(-3.45f,.03f,-51.9f),Quaternion.Euler(0,270,0));
        Configure();running=true;yield return Wait(.2f);
        Vector3 dir=interaction.transform.position-view.transform.position;float desired=-Mathf.Atan2(dir.y,new Vector2(dir.x,dir.z).magnitude)*Mathf.Rad2Deg;mousePitch=(Field<float>(player,"pitch")-desired)/Field<float>(player,"lookSensitivity");yield return Wait(.25f);
        Check(interactor.TryGetTarget(out var target)&&ReferenceEquals(target,interaction),"actual center-camera ray reaches existing HouseInteraction");
        Check(!phone.CanUse(gameObject),"foreign actor cannot operate handset");
        yield return Wait(3.1f);var ring=Field<AudioSource>(house,"telephoneAudio");
        Check(ring.isPlaying&&ring.spatialBlend==1&&ring.dopplerLevel==0,"actual ring source plays spatially at the phone base");
        Capture("01-ringing");yield return Press(Key.E,.1f);yield return Wait(.12f);
        Check(house.HasAnswered&&phone.IsOffHook&&phone.IsMoving&&!ring.isPlaying,"real E stops ring, commits house answer and starts physical pickup");
        Check(house.PowerLocation==HousePowerLocation.Hall&&!house.HasKey&&!house.HasExited,"answer preserves the sole hall reserve and fabricates no key or exit");
        Check(!phone.CanUse(player.gameObject),"repeated E cannot reverse an unfinished handset motion");
        var handset=Field<Transform>(phone,"handset");session.SetPaused(true);var pausedPose=handset.position;var pausedRotation=handset.rotation;yield return Wait(.3f);
        Check(Vector3.Distance(pausedPose,handset.position)<.00001f&&Quaternion.Angle(pausedRotation,handset.rotation)<.001f,"pause freezes both physical handset position and rotation");
        Check(!phone.CanUse(player.gameObject)&&AudioListener.pause,"paused handset interaction and audio respect session pause");
        session.BeginOrResume();yield return Wait(.8f);var offhook=Field<Transform>(phone,"offhook");
        Check(!phone.IsMoving&&phone.IsOffHook&&Vector3.Distance(handset.position,offhook.position)<.0001f&&Quaternion.Angle(handset.rotation,offhook.rotation)<.01f,"resumed pickup settles on exact authored tabletop pose");
        CheckCord();Capture("02-offhook-reserve");
        var before=player.transform.position;yield return Press(Key.R,.1f);yield return Wait(.1f);
        Check(Vector3.Distance(before,player.transform.position)<.02f&&phone.IsOffHook&&house.HasAnswered,"house R does not rewind the handset or invent a recording");
        yield return Press(Key.E,.1f);yield return Wait(.8f);var rest=Field<Transform>(phone,"rest");
        Check(!phone.IsOffHook&&!phone.IsMoving&&Vector3.Distance(handset.position,rest.position)<.0001f&&Quaternion.Angle(handset.rotation,rest.rotation)<.01f,"second real E returns handset to its physical cradle");
        Check(house.HasAnswered&&!ring.isPlaying&&house.PowerLocation==HousePowerLocation.Hall,"hanging up does not restore mains or restart the call");CheckCord();Capture("03-hungup-reserve");
        yield return Press(Key.E,.1f);yield return Wait(.8f);
        Check(phone.IsOffHook&&house.HasAnswered&&!ring.isPlaying,"later pickup remains optional and does not repeat the call");
        house.LeaveHouse();Check(!phone.CanUse(player.gameObject),"leaving the house disables phone interaction");
        Finish(true,"");
    }
    void CheckCord(){var line=Field<LineRenderer>(phone,"cord");var start=Field<Transform>(phone,"cordBase");var end=Field<Transform>(phone,"cordHandset");Check(line.positionCount==145&&Vector3.Distance(line.GetPosition(0),start.position)<.0001f&&Vector3.Distance(line.GetPosition(144),end.position)<.0001f,"coiled cord endpoints remain attached to base and moving handset");Check(Enumerable.Range(0,line.positionCount).All(i=>line.GetPosition(i).y>.760f),"all sampled cord vertices remain above the actual tabletop");}
    void Capture(string name){var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Output+"/"+name+".png",t.EncodeToPNG());}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}}
    void Configure(){previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;keyboard=InputSystem.AddDevice<Keyboard>("TelephoneTestKeyboard");mouse=InputSystem.AddDevice<Mouse>("TelephoneTestMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
    void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(0,mousePitch)});mousePitch=0;}
    IEnumerator Wait(float seconds){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(seconds);}
    IEnumerator Press(Key key,float seconds){keys=new[]{key};yield return new WaitForSecondsRealtime(seconds);keys=Array.Empty<Key>();yield return null;}
    void Finish(bool pass,string error){Status=pass?"PASS":"FAIL "+error;Cleanup();File.WriteAllText(Output+"/telephone-checks.txt","Scope: one explicit prior-act fixture and one Player pose fixture. Actual E/R and player-camera ray thereafter. Objective evidence, no human listening or fear claim.\n"+string.Join("\n",checks)+"\n"+Status);Debug.Log("Telephone probe "+Status);}
    void Cleanup(){running=false;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;if(previousDevices.Length>0&&!string.IsNullOrEmpty(previousScheme))input.SwitchCurrentControlScheme(previousScheme,previousDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
    void OnDestroy(){Cleanup();}
}
#endif

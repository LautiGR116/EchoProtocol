#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using EchoProtocol.Campaign;using EchoProtocol.Interaction;using EchoProtocol.Presentation;using UnityEngine;
// Passive observer of displayed owner versus actual dispatch throughout a real route.
[DefaultExecutionOrder(500)]
public sealed class InteractionConsistencyObserver:MonoBehaviour {
 PlayerInteractor input;InteractionFocusHUD hud;GameSession session;string path;bool done;int pausedFrames;readonly List<object> receipts=new List<object>();
 public static void StartAt(string path){var p=new GameObject("Temporary interaction presentation observer").AddComponent<InteractionConsistencyObserver>();p.path=path;p.input=FindFirstObjectByType<PlayerInteractor>();p.hud=FindFirstObjectByType<InteractionFocusHUD>();p.session=FindFirstObjectByType<GameSession>();p.input.Interacted+=p.Accepted;}
 void Accepted(IInteractable target){if(done)return;try{
  if(!ReferenceEquals(hud.DisplayedTarget,target))throw new Exception("Displayed owner differs from real E target: "+(target as Component)?.name+"; displayed "+(hud.DisplayedTarget as Component)?.name);
  if(input.LastActivationFrame!=Time.frameCount)throw new Exception("Dispatch did not use this frame's selection");
  string kind=target is HouseInteraction h?h.Kind.ToString():target is FinalDoorInteraction f?f.Kind.ToString():target is CampaignStation s?s.Kind.ToString():target.GetType().Name;
  receipts.Add(new{frame=Time.frameCount,type=target.GetType().Name,kind,owner=(target as Component).name,prompt=hud.DisplayedPrompt});Save("RUNNING","");
 }catch(Exception e){Save("FAIL",e.Message);done=true;Debug.LogError(e.Message);}}
 void LateUpdate(){if(done||!input)return;if(session.IsPaused){pausedFrames++;if(input.HasUsableFocus){Save("FAIL","usable focus survived session pause");done=true;}}
  var route=FindFirstObjectByType<ThresholdCampaignProbe>();if(route&&route.Status=="PASS"){Save("PASS","");done=true;}else if(route&&route.Status.StartsWith("FAIL")){Save("FAIL","parent campaign route failed");done=true;}}
 void Save(string status,string error){File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(new{status,error,scope="Passive observer on input-driven campaign; displayed owner must equal each actual E dispatch. No interactions, poses or state changes injected by this observer. Pause clears usable focus. No human feel claim.",pausedFrames,accepted=receipts.Count,receipts},Newtonsoft.Json.Formatting.Indented));}
 void OnDestroy(){if(input)input.Interacted-=Accepted;}
}
#endif

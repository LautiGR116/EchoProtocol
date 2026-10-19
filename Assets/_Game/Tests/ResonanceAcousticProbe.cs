#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using UnityEngine;
using EchoProtocol.Audio;using EchoProtocol.Campaign;using EchoProtocol.Player;using EchoProtocol.Presentation;
// Explicit acoustic fixtures complement, never stand in for, the continuous native route.
public sealed class ResonanceAcousticProbe:MonoBehaviour
{
    public string Status{get;private set;}="starting";
    GameSession session;SessionAudio mix;FirstPersonController player;ResonanceSequence sequence;ResonanceEmitter voice,line;ResonanceRouteObserver capture;
    CampaignDirector campaign;HouseTelephone phone;HouseMemory house;readonly List<string> checks=new List<string>();readonly List<object> marks=new List<object>();
    string output="Docs/AudioEvidence/2026-10-08-Resonance/acoustics";bool oldMono,oldReduced,oldSaves;float oldVolume;bool initialized,done;
    public static void Run(){if(!Application.isPlaying)throw new Exception("Fresh play mode required");new GameObject("Explicit Resonance acoustic fixtures").AddComponent<ResonanceAcousticProbe>().StartCoroutine(RunGuarded());}
    static IEnumerator RunGuarded(){var p=FindFirstObjectByType<ResonanceAcousticProbe>();var stack=new Stack<IEnumerator>();stack.Push(p.Execute());while(stack.Count>0){object o;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}o=stack.Peek().Current;}catch(Exception e){p.Finish(false,e.ToString());yield break;}if(o is IEnumerator nested)stack.Push(nested);else yield return o;}}
    void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add(text);}
    void Mark(string name){capture.FixtureStep=name;marks.Add(new{name,seconds=capture.CapturedSeconds});}
    IEnumerator Wait(float seconds){yield return new WaitForSecondsRealtime(seconds);Check(!session.IsPaused,"explicit acoustic wait has not silently lost focus");}
    IEnumerator Execute(){
        Directory.CreateDirectory(output);session=FindFirstObjectByType<GameSession>();mix=FindFirstObjectByType<SessionAudio>();player=FindFirstObjectByType<FirstPersonController>();sequence=FindFirstObjectByType<ResonanceSequence>();campaign=FindFirstObjectByType<CampaignDirector>();house=FindFirstObjectByType<HouseMemory>();phone=FindFirstObjectByType<HouseTelephone>();
        var emitters=FindObjectsByType<ResonanceEmitter>(FindObjectsSortMode.None);voice=emitters.Single(e=>e.name.Contains("distant voice"));line=emitters.Single(e=>e.name.Contains("live line"));
        oldMono=mix.Speakers;oldReduced=mix.ReducedDynamics;oldVolume=AudioListener.volume;oldSaves=campaign.SuppressSaves;initialized=true;campaign.SuppressSaves=true;
        session.BeginOrResume();yield return Wait(.2f);sequence.enabled=false;capture=ResonanceRouteObserver.Begin(Path.GetFullPath(output));AudioListener.volume=.7f;
        // Fixed physical shaft, same listener position and clip; only orientation/mix changes.
        foreach(var mode in new[]{"stereo-right","stereo-left","mono","reduced"}){
            voice.StopNow();mix.Speakers=mode=="mono";mix.ReducedDynamics=mode=="reduced";
            player.ResetPose(new Vector3(4.5f,.03f,-40.0f),Quaternion.Euler(0,mode=="stereo-left"?180:0,0));
            yield return Wait(.3f);Mark(mode);voice.Play();yield return Wait(4.7f);
            Check(voice.Source.spatialBlend==1&&voice.Source.transform.parent!=player.transform,"shaft stays in world space in "+mode);
            Check(voice.Obstructed&&voice.Cutoff<1800,"real plant perimeter filters distant shaft in "+mode);
        }
        mix.Speakers=false;mix.ReducedDynamics=false;Mark("pause-playing-source");voice.Play();yield return Wait(.65f);session.SetPaused(true);yield return new WaitForSecondsRealtime(.15f);int at=voice.Source.timeSamples;int count=voice.PlayCount;yield return new WaitForSecondsRealtime(.6f);
        Check(Math.Abs(voice.Source.timeSamples-at)<=32&&voice.PlayCount==count,"pause freezes sample position and cannot replay the voice");session.BeginOrResume();yield return Wait(.35f);Check(voice.Source.timeSamples>at,"resume continues the original voice position");voice.StopNow();
        // Public chapter fixture, then the actual telephone state owner and handset.
        sequence.enabled=true;campaign.PrepareHouseProbe();var origin=FindObjectsByType<CampaignStation>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(s=>s.Kind==StationKind.HouseOrigin);campaign.Use(origin);
        player.ResetPose(new Vector3(-3.3f,.03f,-51.9f),Quaternion.Euler(0,270,0));yield return Wait(.4f);Check(house.Active&&!house.HasAnswered,"fixture enters a fresh unanswered house");
        Mark("answer-live-line");phone.Use(player.gameObject);yield return Wait(4);Check(sequence.CallCommitted&&line.PlayCount==1&&line.IsPlaying,"answer schedules exactly one live line at the physical receiver");
        Mark("hang-up");phone.Use(player.gameObject);yield return Wait(.9f);Check(!line.IsPlaying&&line.AppliedGain==0,"hanging up fades and stops the line without a hard cut");
        Mark("lift-dead-line");phone.Use(player.gameObject);yield return Wait(2);Check(line.PlayCount==1&&!line.IsPlaying,"lifting again does not loop or repeat the performance");
        Check(line.transform.IsChildOf((Transform)typeof(HouseTelephone).GetField("handset",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(phone)),"voice is at the actual moving handset, never at the camera or Echo");Finish(true,"");
    }
    void Finish(bool ok,string error){if(done)return;done=true;Status=ok?"PASS":"FAIL "+error;if(capture)capture.Finish(Status);if(initialized){mix.Speakers=oldMono;mix.ReducedDynamics=oldReduced;AudioListener.volume=oldVolume;campaign.SuppressSaves=oldSaves;sequence.enabled=true;}File.WriteAllText(output+"/checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status=Status,checks,marks,scope="Four explicit Player pose/orientation fixtures, direct source playback, public house chapter fixture and telephone API. Real renderer/mixer/listener and physical walls; no human listening or continuous-route claim. User mix and save-suppression restored."},Newtonsoft.Json.Formatting.Indented));if(!ok)Debug.LogError(Status);}
}
#endif

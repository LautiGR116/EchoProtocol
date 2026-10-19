#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Audio;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Isolated persistence test: explicit chapter/pose fixtures, actual E detent and
// PlayerPrefs writes, fresh scene loads and the public APIs used by Continue.
// The separate continuous probe verifies the earned route without any fixtures.
public sealed class IsolationCheckpointProbe:MonoBehaviour
{
    string KeyName;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    readonly List<Preference> saved=new List<Preference>();
    readonly List<Preference> untouched=new List<Preference>();
    sealed class Preference{public string key;public bool exists;public int value;}
    [Serializable] sealed class Result{public string status,scene,error,scope;public int checks,reloads,chapterFixtures,poseFixtures;public bool preferencesRestored;}
    CampaignDirector campaign;GameSession session;PressureIsolation isolation;PressureIsolationAudio audio;
    FirstPersonController player;PlayerInput input;EchoRecorder recorder;LoopController loop;
    Keyboard keyboard;Mouse mouse;InputDevice[] previousDevices;string previousScheme;bool previousAutoSwitch,configured,running,backedUp,cleaned;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();
    string scene,output,step,error;int checks,reloads,chapterFixtures,poseFixtures;
    public string Status{get;private set;}="starting";
    public static void Run(){if(!Application.isPlaying)throw new Exception("Requires fresh Play Mode");var p=new GameObject("Temporary Isolation checkpoint verification").AddComponent<IsolationCheckpointProbe>();DontDestroyOnLoad(p.gameObject);p.StartCoroutine(p.Guarded(p.Execute()));}
    IEnumerator Guarded(IEnumerator r){var s=new Stack<IEnumerator>();s.Push(r);while(s.Count>0){object next;try{if(!s.Peek().MoveNext()){s.Pop();continue;}next=s.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)s.Push(child);else yield return next;}}
    IEnumerator Execute()
    {
        yield return null;scene=SceneManager.GetActiveScene().path;string edition=SceneManager.GetActiveScene().name.Replace("EchoProtocol_","");KeyName="EP.CampaignCheckpoint."+edition+".v1";output="Docs/VisualEvidence/2026-10-01-"+edition+"/checkpoint";
        Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output+".checks.txt","ISOLATED PERSISTENCE: explicit chapter and pose fixtures. Not continuous play or human acceptance.\n");
        Resolve();Check(session.IsPaused&&!session.HasStarted,"fresh title starts the test");
        oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
        foreach(var suffix in new[]{"",".isolated",".answered",".light"})saved.Add(Read(KeyName+suffix));
        foreach(var key in new[]{"EP.CampaignCheckpoint.v1","EP.ThresholdCheckpoint.v1","EP.CampaignCheckpoint.Readers.v1"})foreach(var suffix in new[]{"",".isolated",".answered",".light"})untouched.Add(Read(key+suffix));
        backedUp=true;running=true;Application.logMessageReceived+=Log;
        foreach(bool latched in new[]{false,true})
        {
            step=latched?"latched save":"running save";yield return Reload();
            session.BeginOrResume();campaign.SuppressSaves=false;campaign.BeginExpansionProbe();chapterFixtures++;
            Check(campaign.BeginPressureIsolation(),"explicit prior-act fixture enters the actual isolation API");
            Check(PlayerPrefs.GetInt(KeyName,-1)==(int)CampaignChapter.Pressure&&PlayerPrefs.GetInt(KeyName+".isolated",-1)==0,"physical entry owner writes running pressure checkpoint");
            if(latched)
            {
                var grip=Field<Transform>(isolation,"gripPoint");var camera=player.GetComponentInChildren<Camera>();
                var position=FindFirstObjectByType<PressureMotorRotor>()?new Vector3(4.4f,.03f,-35.45f):new Vector3(4.5f,.03f,-36.5f);var direction=grip.position-(position+(camera.transform.position-player.transform.position));
                player.ResetPose(position,Quaternion.LookRotation(new Vector3(direction.x,0,direction.z)));poseFixtures++;
                yield return Wait(.15f);
                // Pitch is supplied through the same synthetic mouse delta path as the continuous test.
                float desired=-Mathf.Atan2(direction.y,new Vector2(direction.x,direction.z).magnitude)*Mathf.Rad2Deg;
                float pitch=Field<float>(player,"pitch");float sensitivity=Field<float>(player,"lookSensitivity");
                mousePitchDelta=(pitch-desired)/sensitivity;
                yield return Wait(.15f);keys=new[]{Key.E};yield return new WaitForSecondsRealtime(1.95f);keys=Array.Empty<Key>();yield return Wait(.08f);
                Check(isolation.IsLatched&&!isolation.IsDrained&&isolation.CommitCount==1,"actual sustained E commits while the pressure is still draining");
                Check(PlayerPrefs.GetInt(KeyName+".isolated",-1)==1,"live detent writes the latched save before drainage ends");
                Check(audio.LatchPlayCount==1&&audio.PurgeStartCount==1,"live save was accompanied by exactly one physical latch and purge");
            }
            Check(untouched.All(Unchanged),"writing the new edition leaves legacy checkpoint namespaces untouched");
            yield return Reload();step=latched?"latched Continue":"running Continue";
            Check(campaign.HasSavedCheckpoint&&campaign.SavedChapterTitle=="Subsuelo / aislamiento","fresh title recognizes the actual persisted checkpoint");
            session.BeginOrResume();Check(campaign.RestoreCheckpoint(),"the public Continue restoration API succeeds");yield return Wait(.18f);
            var door=Field<SlidingDoor>(isolation,"exitDoor");var lights=Field<Light[]>(isolation,"workLights");
            Check(campaign.Chapter==CampaignChapter.Pressure&&isolation.Active&&!session.IsPaused,"Continue enters active pressure gameplay");
            Check(isolation.IsLatched==latched&&isolation.IsDrained==latched&&Mathf.Approximately(isolation.HandleFraction,latched?1:0)&&Mathf.Approximately(isolation.Pressure01,latched?0:1),"fresh continuation settles to running or fully drained canonical state");
            Check((latched?door.IsFullyOpen:door.IsFullyClosed)&&door.IsOpenRequested==latched&&lights.All(l=>l.enabled==!latched),"physical passage and work circuit match the restored detent");
            Check(Vector3.Distance(player.transform.position,isolation.ContinuationOrigin.position)<.04f,"continuation uses the safe authored entry location");
            Check(!loop.enabled&&!recorder.IsRecording&&loop.LastRecording==null&&loop.ActiveEcho==null,"fresh continuation fabricates no saved Echo and leaves recording suspended");
            Check(audio.LatchPlayCount==0&&audio.PurgeStartCount==0&&isolation.CommitCount==0&&isolation.DrainCompletionCount==0,"Continue does not replay latch, purge or live completion events");
            Check(latched?audio.MachineSourcesStopped:Field<AudioSource>(audio,"motorSource").isPlaying,"settled restored audio is silent when isolated or running when powered");
            var sonic=FindFirstObjectByType<ResonanceSequence>();
            if(sonic){Check(sonic.BaselineEstablished&&sonic.PressureCommitted==latched,"Resonance Continue treats restored shutdown as baseline");
                Check(FindObjectsByType<ResonanceEmitter>(FindObjectsSortMode.None).Where(e=>!e.name.Contains("air in fixed riser")).All(e=>e.PlayCount==0),"restored plant never repeats dramatic voices or structural transients");}

            var visual=FindFirstObjectByType<PressureMotorRotor>();
            if(visual){var rotor=Field<Transform>(visual,"rotor");var beforeRotation=rotor.localRotation;yield return Wait(.123f);Check(latched?Quaternion.Angle(beforeRotation,rotor.localRotation)<.001f:Quaternion.Angle(beforeRotation,rotor.localRotation)>1,"fresh Continue restores the real motor presentation without a replayed coast");}
            var before=player.transform.position;keys=new[]{Key.R};yield return new WaitForSecondsRealtime(.1f);yield return Wait(.15f);
            Check(loop.CompletedLoopCount==0&&loop.LastRecording==null&&Vector3.Distance(before,player.transform.position)<.02f,"actual R after Continue cannot teleport or fabricate a take");
            session.SetPaused(true);yield return Wait(.12f);session.BeginOrResume();yield return Wait(.18f);
            Check(!loop.enabled&&!recorder.IsRecording&&isolation.IsLatched==latched&&audio.LatchPlayCount==0&&audio.PurgeStartCount==0,"pause and resume preserve suspension and do not replay shutdown");
            Check(PlayerPrefs.GetInt(KeyName+".isolated",-1)==(latched?1:0),"Continue and paused input preserve the saved result");
        }
        Finish(true,"");
    }
    void Resolve(){campaign=FindFirstObjectByType<CampaignDirector>();session=FindFirstObjectByType<GameSession>();isolation=FindFirstObjectByType<PressureIsolation>();audio=FindFirstObjectByType<PressureIsolationAudio>();player=FindFirstObjectByType<FirstPersonController>();input=player.GetComponent<PlayerInput>();recorder=player.GetComponent<EchoRecorder>();loop=FindFirstObjectByType<LoopController>();Check(campaign&&isolation&&audio&&isolation.enabled&&audio.enabled&&Field<string>(campaign,"checkpointKey")==KeyName,"opt-in Isolation scene and checkpoint namespace are configured");}
    IEnumerator Reload(){if(session)session.SetPaused(true);Release();var op=SceneManager.LoadSceneAsync(scene);while(!op.isDone)yield return null;yield return null;Resolve();reloads++;Check(session.IsPaused&&!session.HasStarted,"scene reload starts at a fresh title");Configure();}
    float mousePitchDelta;
    void Configure(){previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;keyboard=InputSystem.AddDevice<Keyboard>("IsolationCheckpointKeyboard");mouse=InputSystem.AddDevice<Mouse>("IsolationCheckpointMouse");configured=true;input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.onBeforeUpdate+=InputUpdate;}
    void InputUpdate(){if(!running||!configured||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(0,mousePitchDelta)});mousePitchDelta=0;}
    IEnumerator Wait(float s){keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(s);if(!string.IsNullOrEmpty(error))throw new Exception(error);}
    static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,Private).GetValue(o);
    static Preference Read(string k)=>new Preference{key=k,exists=PlayerPrefs.HasKey(k),value=PlayerPrefs.GetInt(k)};
    static bool Unchanged(Preference p)=>PlayerPrefs.HasKey(p.key)==p.exists&&(!p.exists||PlayerPrefs.GetInt(p.key)==p.value);
    void Check(bool ok,string text){if(!ok)throw new Exception(step+": "+text);checks++;File.AppendAllText(output+".checks.txt","PASS ["+step+"] "+text+"\n");}
    void Log(string m,string st,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)error=m+"\n"+st;}
    void Release(){keys=Array.Empty<Key>();InputSystem.onBeforeUpdate-=InputUpdate;if(!configured)return;configured=false;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;var usable=previousDevices.Where(d=>d!=null&&d.added).ToArray();if(!string.IsNullOrEmpty(previousScheme)&&usable.Length>0)input.SwitchCurrentControlScheme(previousScheme,usable);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
    void Cleanup(){if(cleaned)return;cleaned=true;running=false;Application.logMessageReceived-=Log;if(session)session.SetPaused(true);Release();if(!backedUp)return;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;foreach(var p in saved)if(p.exists)PlayerPrefs.SetInt(p.key,p.value);else PlayerPrefs.DeleteKey(p.key);PlayerPrefs.Save();}
    void Finish(bool pass,string message){Cleanup();bool restored=backedUp&&saved.All(Unchanged)&&untouched.All(Unchanged);pass&=restored;Status=pass?"PASS":"FAIL "+message;File.WriteAllText(output+".result.json",JsonUtility.ToJson(new Result{status=Status,scene=scene,error=message,checks=checks,reloads=reloads,chapterFixtures=chapterFixtures,poseFixtures=poseFixtures,preferencesRestored=restored,scope="Isolated API chapter fixtures, one pose fixture, actual E/PlayerPrefs writes and fresh scene continuation. No continuous route or human listening claim."},true));File.AppendAllText(output+".checks.txt",Status+"\n");}
    void OnDestroy(){Cleanup();}
}
#endif

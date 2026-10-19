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
// This isolates circulation and presentation; it never claims a solved P3.
public sealed class RecordsPassageProbe : MonoBehaviour
{
    const string Output="Docs/VisualEvidence/2026-10-01-Records/route";
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    FirstPersonController player; Camera view; GameSession session; CampaignDirector campaign; PlayerInput input; PlayerFootsteps feet;
    Keyboard keyboard; Mouse mouse; InputDevice[] previousDevices; string previousScheme; bool previousAutoSwitch,oldSuppress,configured,running;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    Key[] keys=Array.Empty<Key>(); float yaw=180,sensitivity,started;
    AudioSource[] lamps; Vector3[] anchors;
    readonly List<string> checks=new List<string>();
    public string Status{get;private set;}="starting";
    public static void Run(){if(!Application.isPlaying)throw new Exception("Fresh Play required");var p=new GameObject("Temporary physical records route").AddComponent<RecordsPassageProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
    static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
    IEnumerator Guarded(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
    IEnumerator Execute()
    {
        Directory.CreateDirectory(Output); File.Delete(Output+"/result.txt"); File.WriteAllText(Output+"/cameras.txt", "Actual Player camera after one initial fixture; input-only poses thereafter.\n");
        player=FindFirstObjectByType<FirstPersonController>(); view=player.GetComponentInChildren<Camera>(); input=player.GetComponent<PlayerInput>(); feet=player.GetComponent<PlayerFootsteps>();
        session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.EndsWith("EchoProtocol_Records.unity"),"isolated route uses the authored Records successor");
        oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();
        var firstAct=FindFirstObjectByType<VS1StrongEventOne>();
        Check(firstAct.RestoreCompletedLighting()&&firstAct.FluorescentPowerFraction==0f,"explicit fixture restores the completed first-act lighting consequence");
        // Explicit initial fixture only. The full campaign verifies actual R3 ownership.
        FindFirstObjectByType<LoopController>().enabled=false;player.GetComponent<EchoRecorder>().CancelRecording();
        player.ResetPose(new Vector3(-18.6f,.03f,30f),Quaternion.Euler(0,180,0));
        lamps=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(s=>s.name.StartsWith("Records actual fixture hum /")).OrderBy(s=>s.transform.position.z).ToArray();
        Check(lamps.Length==2,"two sources exist at the actual archive luminaires");anchors=lamps.Select(s=>s.transform.position).ToArray();
        foreach(var lamp in lamps) Check(lamp.clip&&lamp.clip.channels==1&&Mathf.Approximately(lamp.volume,.06f)&&lamp.isPlaying&&lamp.loop&&lamp.spatialBlend==1&&lamp.dopplerLevel==0&&!lamp.ignoreListenerPause&&lamp.rolloffMode==AudioRolloffMode.Linear&&lamp.maxDistance==4.5f&&lamp.outputAudioMixerGroup.name=="Ambience","fixture hum uses real mono looping world source and existing ambience bus");
        var physicalLamps=FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.name=="Archive shelf illumination").OrderBy(l=>l.transform.position.z).ToArray();
        Check(physicalLamps.Length==2,"the two reused physical luminaires exist");
        for(int i=0;i<lamps.Length;i++)Check(Vector3.Distance(lamps[i].transform.position,physicalLamps[i].transform.position+Vector3.up*.10f)<.001f,"hum source "+i+" is anchored at its real opal diffuser");
        Configure();running=true;Status="running";started=Time.realtimeSinceStartup;yield return Wait(.2f);Capture("00-approach");
        int contacts=feet.ContactCount;
        yield return Go(-18.6f,27,180);yield return Face(270);Capture("01-entry-aperture");
        yield return Go(-21.15f,27,270);yield return Go(-21.15f,24.5f,180);Capture("02-interior-aisle");
        yield return Face(60);Capture("03-north-gallery-view");
        yield return Go(-21.15f,21,180);yield return Face(145);Capture("04-gallery-through-glass");
        int stoppedContacts=feet.ContactCount; Vector3 stopped=player.transform.position;
        yield return Face(270);yield return Wait(.4f);yield return Face(90);yield return Wait(.4f);
        Check(feet.ContactCount==stoppedContacts&&Vector3.Distance(stopped,player.transform.position)<.025f,"stopping and turning makes no footstep or involuntary movement");
        Check(feet.ContactCount>contacts,"physical walking through the archive produces actual Player contacts");
        for(int i=0;i<lamps.Length;i++)Check(Vector3.Distance(anchors[i],lamps[i].transform.position)<.001f&&lamps[i].isPlaying,"turning leaves fixture source "+i+" playing at its authored anchor");
        yield return Go(-21.15f,18,180);Capture("05-exit-approach");yield return Go(-21.15f,15,180);yield return Face(90);Capture("05-south-exit");
        yield return Go(-18.6f,15,90);yield return Go(-18.6f,13,180);Capture("06-return-to-gallery");
        Check(player.transform.position.y>-.05f,"first forward traversal keeps physical floor support");
        // Backtracking remains an ordinary physical route before the unchanged arrival lock.
        yield return Go(-18.6f,15,0);yield return Go(-21.15f,15,270);yield return Go(-21.15f,27,0);yield return Go(-18.6f,27,90);yield return Go(-18.6f,30,0);
        Check(player.transform.position.z>29.8f,"both apertures allow genuine backtracking without resets");
        // Walk directly into the visible partition to confirm that its visual limit is solid.
        yield return Go(-18.6f,26.2f,180);float beforePush=player.transform.position.z;keys=new[]{Key.W};yield return Wait(.4f);keys=Array.Empty<Key>();
        Check(beforePush-player.transform.position.z>.08f&&player.transform.position.z>25.5f&&player.transform.position.z<26.2f,"visible gallery partition physically blocks the obsolete shortcut");
        Capture("07-solid-partition");
        yield return Go(-18.6f,27,0);yield return Go(-21.15f,27,270);yield return Go(-21.15f,15,180);yield return Go(-18.6f,15,90);yield return Go(-18.6f,13,180);
        Check(player.transform.position.y>-.05f,"repeated forward traversal remains supported and open");
        for(int i=0;i<lamps.Length;i++)Check(Vector3.Distance(anchors[i],lamps[i].transform.position)<.001f,"source anchor "+i+" survives every traversal");
        Finish(true,"");
    }
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
    IEnumerator Face(float target){yaw=target;keys=Array.Empty<Key>();float deadline=Time.realtimeSinceStartup+5;while(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw))>.6f||Mathf.Abs(Field<float>(player,"pitch"))>.6f){if(Time.realtimeSinceStartup>deadline)throw new Exception("Mouse failed to face target");yield return null;}yield return Wait(.08f);}
    IEnumerator Wait(float seconds){yield return new WaitForSecondsRealtime(seconds);}
    void Configure()
    {
        previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;sensitivity=Field<float>(player,"lookSensitivity");
        keyboard=InputSystem.AddDevice<Keyboard>("RecordsRouteKeyboard");mouse=InputSystem.AddDevice<Mouse>("RecordsRouteMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;
    }
    void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{delta=session.IsPaused?Vector2.zero:new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y,yaw),-12,12)/sensitivity,-Mathf.Clamp(Field<float>(player,"pitch"),-10,10)/sensitivity)});}
    void LateUpdate(){if(!running)return;if(session.IsPaused||player.transform.position.y<-.2f){Finish(false,"Unexpected pause or loss of floor; no hidden recovery");StopAllCoroutines();}}
    void Capture(string label)
    {
        var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Output+"/"+label+".png",t.EncodeToPNG());File.AppendAllText(Output+"/cameras.txt",label+" position="+view.transform.position+" rotation="+view.transform.eulerAngles+"\n");}finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}
    }
    void Finish(bool pass,string error)
    {
        Status=pass?"PASS":"FAIL "+error;float elapsed=Time.realtimeSinceStartup-started;Cleanup();
        File.WriteAllText(Output+"/checks.txt","Explicit initial pose/completed-lighting/suspended-recording fixture; synthetic physical input thereafter. No P3 solve, whole campaign, audibility or human experience claim. Camera renders omit OnGUI.\n"+string.Join("\n",checks)+"\n"+Status);
        File.WriteAllText(Output+"/result.txt",Status+"\nchecks="+checks.Count+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture));Debug.Log("Records passage probe "+Status);
    }
    void Cleanup(){running=false;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;if(previousDevices.Length>0&&!string.IsNullOrEmpty(previousScheme))input.SwitchCurrentControlScheme(previousScheme,previousDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
    void OnDestroy(){Cleanup();}
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Horror;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit visual fixture: one actual input-generated take, paused camera poses.
// It does not claim first-play clarity, continuous navigation or fear evidence.
public sealed class PresenceSurfaceProbe:MonoBehaviour
{
    const string Output="Docs/VisualEvidence/2026-10-01-Presence/surface";
    FirstPersonController player;Camera view;GameSession session;CampaignDirector campaign;LoopController loop;PlayerInput input;
    Keyboard keyboard;Mouse mouse;InputDevice[] previousDevices;string previousScheme;bool previousAutoSwitch,oldSuppress,configured,running;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;Key[] keys=Array.Empty<Key>();
    readonly List<string> checks=new List<string>();int fixtures;public string Status{get;private set;}="starting";
    public static void Run(){if(!Application.isPlaying)throw new Exception("Fresh Play required");var p=new GameObject("Temporary recorded surface verification").AddComponent<PresenceSurfaceProbe>();p.StartCoroutine(p.Guarded(p.Execute()));}
    void Check(bool ok,string message){if(!ok)throw new Exception(message);checks.Add("PASS "+message);}
    IEnumerator Guarded(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
    IEnumerator Execute()
    {
        Directory.CreateDirectory(Output);player=FindFirstObjectByType<FirstPersonController>();view=player.GetComponentInChildren<Camera>();input=player.GetComponent<PlayerInput>();session=FindFirstObjectByType<GameSession>();campaign=FindFirstObjectByType<CampaignDirector>();loop=FindFirstObjectByType<LoopController>();
        oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;session.BeginOrResume();
        player.ResetPose(new Vector3(-2.7f,.03f,1.4f),Quaternion.Euler(0,90,0));fixtures++;
        var checkpoint=new GameObject("Temporary visual fixture origin");checkpoint.transform.SetPositionAndRotation(player.transform.position,player.transform.rotation);
        Check(loop.SetRecallCheckpoint(checkpoint.transform),"explicit initial fixture starts a genuine empty recorder");Destroy(checkpoint);
        Configure();running=true;keys=new[]{Key.W};yield return new WaitForSecondsRealtime(.20f);keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(1.2f);
        keys=new[]{Key.R};yield return new WaitForSecondsRealtime(.08f);keys=Array.Empty<Key>();yield return null;session.SetPaused(true);
        var echo=loop.ActiveEcho;var take=loop.LastRecording;
        Check(echo&&echo.gameObject.activeInHierarchy&&take!=null&&ReferenceEquals(take,echo.Recording),"actual W/hold/R produces the live immutable take");
        var frames=Enumerable.Range(0,take.FrameCount).Select(i=>take[i]).ToArray();
        var renderers=echo.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
        var body=renderers.Single(r=>r.sharedMaterial.shader.name=="EchoProtocol/RecordedPresence");var depth=renderers.Single(r=>r.sharedMaterial.shader.name=="EchoProtocol/RecordedSelfDepth");
        var bodyMesh=body.GetComponent<MeshFilter>().sharedMesh;var depthMesh=depth.GetComponent<MeshFilter>().sharedMesh;
        Check(bodyMesh==depthMesh&&body.transform.localToWorldMatrix==depth.transform.localToWorldMatrix,"color and self-depth use the same rigid mesh and world transform");
        Check(depth.sharedMaterial.renderQueue<body.sharedMaterial.renderQueue,"self depth precedes color in the transparent queue");
        Check(depth.GetComponents<Collider>().Length==0&&depth.GetComponents<AudioSource>().Length==0&&depth.GetComponents<Light>().Length==0,"depth proxy creates no gameplay collider, light or sound");
        Check(depth.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.Off,"depth proxy casts no physical shadow");
        Check(depth.sharedMaterial.GetFloat("_NearFadeStart")==body.sharedMaterial.GetFloat("_NearFadeStart")&&depth.sharedMaterial.GetFloat("_NearFadeEnd")==body.sharedMaterial.GetFloat("_NearFadeEnd"),"both surfaces share the original near-fade distances");
        var center=echo.transform.position;
        float[] distances={2.4f,1.05f,.8f,.6f,.4f,.34f,.2f};
        foreach(float distance in distances)
        {
            player.ResetPose(center+Vector3.right*distance,Quaternion.Euler(0,270,0));fixtures++;
            yield return null;
            Capture("front-"+distance.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
        }
        // Below fade-start, proxy must not mask transparent panes behind it.
        var cameraData=view.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();var post=cameraData.renderPostProcessing;
        // Remove stochastic postprocessing only for the numeric zero-fade test.
        // The preceding approach captures retain the actual game postprocessing.
        cameraData.renderPostProcessing=false;
        var withInvisible=Capture("hidden-enabled");body.enabled=false;depth.enabled=false;var without=Capture("hidden-disabled");body.enabled=true;depth.enabled=true;cameraData.renderPostProcessing=post;
        long total=0;int maximum=0,changed=0;for(int i=0;i<withInvisible.Length;i++){int d=Math.Abs(withInvisible[i].r-without[i].r)+Math.Abs(withInvisible[i].g-without[i].g)+Math.Abs(withInvisible[i].b-without[i].b);total+=d;maximum=Math.Max(maximum,d);if(d>0)changed++;}
        File.WriteAllText(Output+"/hidden-pixels.txt","Same paused camera, postprocessing disabled only for this numeric equivalence; RGB24 readback. enabled invisible surfaces vs disabled. Changed="+changed+" maximum RGB sum="+maximum+" mean RGB sum="+(double)total/withInvisible.Length+"\n");
        Check(maximum<=3&&(double)total/withInvisible.Length<.01,"fully faded surfaces match disabled surfaces within 1 RGB step per channel");
        // Reverse side puts the observation glass behind the recorded body.
        player.ResetPose(center-Vector3.right*1.3f,Quaternion.Euler(0,90,0));fixtures++;yield return null;Capture("back-glass-behind");
        // The real partition must occlude the Echo; no proxy should punch through it.
        player.ResetPose(new Vector3(2.9f,.03f,3.75f),Quaternion.Euler(0,270,0));fixtures++;yield return null;Capture("real-partition");
        Check(ReferenceEquals(take,loop.LastRecording)&&ReferenceEquals(take,echo.Recording)&&frames.Length==take.FrameCount,"camera fixtures never replace the actual take");
        for(int i=0;i<frames.Length;i++)Check(frames[i].Timestamp==take[i].Timestamp&&frames[i].Position==take[i].Position&&frames[i].Rotation==take[i].Rotation,"immutable frame "+i);
        // A second explicit pose/checkpoint fixture samples the actual warm
        // return corridor. Restore only the authored completed lighting state.
        session.BeginOrResume();
        FindFirstObjectByType<VS1StrongEventOne>().RestoreCompletedLighting();
        player.ResetPose(new Vector3(-2.7f,.03f,22.974f),Quaternion.Euler(0,90,0));fixtures++;
        checkpoint=new GameObject("Temporary warm visual fixture origin");checkpoint.transform.SetPositionAndRotation(player.transform.position,player.transform.rotation);
        Check(loop.SetRecallCheckpoint(checkpoint.transform),"warm fixture clears the old take through the public checkpoint API");Destroy(checkpoint);
        keys=new[]{Key.W};yield return new WaitForSecondsRealtime(.20f);keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.4f);
        keys=new[]{Key.R};yield return new WaitForSecondsRealtime(.08f);keys=Array.Empty<Key>();yield return null;session.SetPaused(true);
        echo=loop.ActiveEcho;take=loop.LastRecording;
        Check(echo&&echo.gameObject.activeInHierarchy&&take!=null&&ReferenceEquals(take,echo.Recording),"warm view uses another actual input-generated take");
        renderers=echo.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();body=renderers.Single(r=>r.sharedMaterial.shader.name=="EchoProtocol/RecordedPresence");depth=renderers.Single(r=>r.sharedMaterial.shader.name=="EchoProtocol/RecordedSelfDepth");
        player.ResetPose(new Vector3(-2.692f,.03f,27.015f),Quaternion.Euler(0,180,0));fixtures++;yield return null;
        var gate=FindObjectsByType<EchoProtocol.Puzzle.SlidingDoor>(FindObjectsSortMode.None).Single(d=>d.GetComponentsInChildren<Transform>(true).Any(tr=>tr.name=="P2 Blind Alignment Gate Body"));
        gate.SetOpenImmediately(true);Check(gate.IsFullyOpen,"explicit warm geometry fixture opens the existing P2 gate");
        Capture("warm-selected-surface");
        Check(ReferenceEquals(take,loop.LastRecording)&&ReferenceEquals(take,echo.Recording),"warm visual fixture retains the genuine take");
        Finish(true,"");
    }
    Color32[] Capture(string label)
    {
        var old=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{view.targetTexture=rt;view.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Output+"/"+label+".png",t.EncodeToPNG());return t.GetPixels32();}
        finally{view.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(t);}
    }
    void Configure(){previousDevices=input.devices.ToArray();previousScheme=input.currentControlScheme;previousAutoSwitch=input.neverAutoSwitchControlSchemes;oldBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;keyboard=InputSystem.AddDevice<Keyboard>("PresenceSurfaceKeyboard");mouse=InputSystem.AddDevice<Mouse>("PresenceSurfaceMouse");input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",keyboard,mouse);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;configured=true;InputSystem.onBeforeUpdate+=InputUpdate;}
    void InputUpdate(){if(!running||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState());}
    void Finish(bool pass,string error){Status=pass?"PASS":"FAIL "+error;Cleanup();File.WriteAllText(Output+"/checks.txt","Scope: cold and warm initial pose/checkpoint fixtures, actual W/hold/R in each, authored completed-lighting restore for warm, then paused camera pose fixtures (total "+fixtures+"). No fabricated recording. Camera.Render excludes OnGUI. Visual/occlusion evidence only; not human approval.\n"+string.Join("\n",checks)+"\n"+Status);Debug.Log("Presence surface probe "+Status);}
    void Cleanup(){running=false;InputSystem.onBeforeUpdate-=InputUpdate;if(campaign)campaign.SuppressSaves=oldSuppress;if(session)session.SetPaused(true);if(!configured)return;configured=false;InputSystem.settings.editorInputBehaviorInPlayMode=oldBehavior;if(input){input.neverAutoSwitchControlSchemes=previousAutoSwitch;if(previousDevices.Length>0&&!string.IsNullOrEmpty(previousScheme))input.SwitchCurrentControlScheme(previousScheme,previousDevices);}if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);}
    void OnDestroy(){Cleanup();}
}
#endif

#if UNITY_EDITOR
using System.Collections;
using System.IO;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class SessionPauseProbe : MonoBehaviour
{
    private Keyboard keyboard;
    private Mouse mouse;
    private Key[] keys=System.Array.Empty<Key>();
    private InputSettings.EditorInputBehaviorInPlayMode prior;
    public static void Run()=>new GameObject("Temporary pause regression").AddComponent<SessionPauseProbe>().StartCoroutine(Test());
    private static IEnumerator Test()
    {
        var probe=FindFirstObjectByType<SessionPauseProbe>();
        var player=FindFirstObjectByType<FirstPersonController>();
        var session=FindFirstObjectByType<GameSession>();
        probe.keyboard=InputSystem.AddDevice<Keyboard>("SessionPauseTest");probe.mouse=InputSystem.AddDevice<Mouse>("SessionPauseMouse");
        var input=player.GetComponent<PlayerInput>();input.neverAutoSwitchControlSchemes=true;input.SwitchCurrentControlScheme("Keyboard&Mouse",probe.keyboard,probe.mouse);
        probe.prior=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate+=probe.Feed;
        session.BeginOrResume();
        yield return new WaitForSecondsRealtime(.2f);
        probe.keys=new[]{Key.W};yield return new WaitForSecondsRealtime(.3f);probe.keys=System.Array.Empty<Key>();yield return new WaitForSecondsRealtime(.2f);
        probe.keys=new[]{Key.Escape};yield return new WaitForSecondsRealtime(.15f);probe.keys=System.Array.Empty<Key>();yield return new WaitForSecondsRealtime(.1f);
        Check(session.IsPaused&&!player.enabled&&Time.timeScale==0f,"Escape enters real pause");
        float elapsed=player.GetComponent<EchoRecorder>().ElapsedTime;
        yield return new WaitForSecondsRealtime(.4f);
        Check(Mathf.Approximately(elapsed,player.GetComponent<EchoRecorder>().ElapsedTime),"paused recording does not advance");
        probe.keys=new[]{Key.Escape};yield return new WaitForSecondsRealtime(.15f);probe.keys=System.Array.Empty<Key>();yield return new WaitForSecondsRealtime(.1f);
        Check(!session.IsPaused&&player.enabled&&Time.timeScale==1f,"Escape resumes controller");
        // Model a focus regain: Escape must not have revoked the controller's lock preference.
        player.SendMessage("OnApplicationFocus",true);
        Check(player.UsesSessionPause&&Cursor.lockState==CursorLockMode.Locked&&!Cursor.visible,"resume keeps cursor-lock preference");
        session.SetPaused(true);
        File.WriteAllText("Docs/VisualEvidence/2026-09-30-CompleteEdition/pause-cursor-checks.txt","PASS native Input System Escape pauses\nPASS recording freezes\nPASS second Escape resumes controller\nPASS focus regain retains locked cursor preference\n");
        Debug.Log("SESSION PAUSE/CURSOR PASS");Destroy(probe.gameObject);
    }
    private void Feed(){if(InputState.currentUpdateType==InputUpdateType.Dynamic)InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
    private static void Check(bool ok,string message){if(!ok)throw new System.Exception(message);}
    private void OnDestroy(){InputSystem.onBeforeUpdate-=Feed;InputSystem.settings.editorInputBehaviorInPlayMode=prior;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);}
}
#endif

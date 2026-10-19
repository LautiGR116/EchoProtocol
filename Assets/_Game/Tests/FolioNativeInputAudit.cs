#if ECHO_NATIVE_VERIFY && !UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-1000)]
public sealed class FolioNativeInputAudit:MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartAudit(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--ep-input-audit")>=0)new GameObject("Native modifier evidence").AddComponent<FolioNativeInputAudit>();}
 void Update(){var k=Keyboard.current;if(k!=null&&k.tabKey.wasPressedThisFrame)File.AppendAllText("/tmp/folio-native-modifiers.txt",$"Input frame={Time.frameCount} shift={k.shiftKey.isPressed} left={k.leftShiftKey.isPressed} right={k.rightShiftKey.isPressed} pressed={k.leftShiftKey.wasPressedThisFrame}/{k.rightShiftKey.wasPressedThisFrame}\n");}
 void OnGUI(){var e=Event.current;if(e.type==EventType.KeyDown)File.AppendAllText("/tmp/folio-native-modifiers.txt",$"GUI frame={Time.frameCount} code={e.keyCode} flags={e.modifiers} shift={e.shift}\n");}
}
#endif

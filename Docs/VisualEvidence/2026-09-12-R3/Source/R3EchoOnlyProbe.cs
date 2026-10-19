#if UNITY_EDITOR
using System;using System.Collections;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using EchoProtocol.Echo;using EchoProtocol.Horror;
public sealed class R3EchoOnlyProbe:MonoBehaviour{
 int contacts;EchoPlayback echo;VS1HubReturnGate gate;
 void OnTriggerEnter(Collider c){if(c.GetComponentInParent<LoopActor>()?.Kind==LoopActorKind.Echo)contacts++;}
 public static void Run(){var gate=FindFirstObjectByType<VS1HubReturnGate>();var p=gate.gameObject.AddComponent<R3EchoOnlyProbe>();p.gate=gate;p.StartCoroutine(p.Test());}
 IEnumerator Test(){var lines=File.ReadAllLines("/tmp/echo-vs1-r3/actual-return-recording.csv");var frames=lines.Select(l=>{var v=l.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();return new EchoFrame(v[0],new Vector3(v[1],v[2],v[3]),new Quaternion(v[4],v[5],v[6],v[7]));});var recording=new EchoRecording(frames);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Echo/PF_EchoGreybox.prefab");if(prefab==null)throw new Exception("Echo prefab not found");echo=Instantiate(prefab).GetComponent<EchoPlayback>();echo.Initialize(recording);float start=Time.time;while(Time.time-start<recording.Duration+1){if(gate.IsOpen)throw new Exception("Echo alone opened R3");yield return null;}File.WriteAllText("/tmp/echo-vs1-r3/echo-only.txt",contacts>0&&!gate.IsOpen?"PASS real EchoPlayback crossed final trigger using unmodified frames captured from actual Player return; contacts="+contacts:"FAIL no Echo physical contact");Destroy(echo.gameObject);}
}
#endif

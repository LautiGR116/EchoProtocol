using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using EchoProtocol.Echo;using EchoProtocol.Horror;using System.Linq;using System.IO;
internal class CommandScript:IRunCommand{public void Execute(ExecutionResult r){
 var lines=new System.Collections.Generic.List<string>();var scene=SceneManager.GetActiveScene();lines.Add("scene="+scene.path+" dirty="+scene.isDirty+" play="+EditorApplication.isPlaying+" compiling="+EditorApplication.isCompiling+" target="+EditorUserBuildSettings.activeBuildTarget);
 var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.gameObject.scene==scene).ToArray();int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));lines.Add("missing_scripts="+missing);if(missing!=0)throw new System.Exception("Missing scripts");
 var d=Object.FindFirstObjectByType<VS1DenialEvidence>();var so=new SerializedObject(d);var it=so.GetIterator();int refs=0;while(it.NextVisible(true)){if(it.propertyType==SerializedPropertyType.ObjectReference){refs++;if(it.objectReferenceValue==null)throw new System.Exception("D1 unassigned "+it.propertyPath);lines.Add(it.propertyPath+"="+it.objectReferenceValue.name);}}
 var loop=Object.FindFirstObjectByType<LoopController>();var ls=new SerializedObject(loop);lines.Add("reset_targets="+ls.FindProperty("resetTargets").arraySize);if(ls.FindProperty("resetTargets").arraySize!=29)throw new System.Exception("Reset list changed");
 foreach(var b in EditorBuildSettings.scenes)lines.Add("build_scene="+b.path+" enabled="+b.enabled);
 lines.Add("D1_explicit_refs="+refs);File.WriteAllLines("Builds/D1-20260916-01/Evidence/editor-audit.txt",lines);r.Log(string.Join("\n",lines));
}}

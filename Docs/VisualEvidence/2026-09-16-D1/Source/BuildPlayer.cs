using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;using System.Linq;using System.IO;
internal class CommandScript:IRunCommand{public void Execute(ExecutionResult r){
 if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle in Edit Mode");
 if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneOSX)throw new System.Exception("Keep existing platform");
 string output="Builds/D1-20260916-01/EchoProtocol-D1-20260916-01.app";
 var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
 if(scenes.Length!=1||!scenes[0].EndsWith("VS1_ParallelReturn_Blockout.unity"))throw new System.Exception("Unexpected build scenes");
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
 var summary=report.summary;string text="Build ID: D1-20260916-01\nUnity: "+Application.unityVersion+"\nScene: "+scenes[0]+"\nTarget: "+summary.platform+"\nResult: "+summary.result+"\nErrors: "+summary.totalErrors+"\nWarnings: "+summary.totalWarnings+"\nBytes: "+summary.totalSize+"\nDuration: "+summary.totalTime+"\nPath: "+summary.outputPath;
 File.WriteAllText("Builds/D1-20260916-01/Evidence/build-report.txt",text);r.Log(text);if(summary.result!=BuildResult.Succeeded)throw new System.Exception("Build did not succeed");
}}

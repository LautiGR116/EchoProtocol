using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Explicit command-line recovery/build entry points; never run automatically.
public static class IncursionDeliveryBuild
{
 public static void BuildRelease()
 {
  var args=Environment.GetCommandLineArgs();
  int outputIndex=Array.IndexOf(args,"-buildOutput");
  if(outputIndex<0||outputIndex+1>=args.Length)
   throw new ArgumentException("Pass -buildOutput with a new macOS .app destination.");
  string output=Path.GetFullPath(args[outputIndex+1]);
  if(!output.EndsWith(".app",StringComparison.OrdinalIgnoreCase))
   throw new ArgumentException("The release destination must end in .app.");
  if(Directory.Exists(output))throw new IOException("Refusing to overwrite an existing app: "+output);
  string symbols=PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone);
  if(symbols.Contains("ECHO_NATIVE_VERIFY"))throw new Exception("QA defines must be removed before delivery");
  Directory.CreateDirectory(Path.GetDirectoryName(output));
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
   scenes=new[]{"Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity"},
   locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.DetailedBuildReport});
  var result=new{scene="EchoProtocol_Incursion",result=report.summary.result.ToString(),
   totalErrors=report.summary.totalErrors,totalWarnings=report.summary.totalWarnings,
   seconds=report.summary.totalTime.TotalSeconds,symbols,
   errors=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content).ToArray()};
  File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"build-report.json"),
   Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Release build failed");
  Debug.Log("Echo Protocol release build succeeded: "+output);
 }
 public static void RestoreSymbols()
 {
  PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone,"SENTIS_ANALYTICS_ENABLED;APP_UI_EDITOR_ONLY");
  AssetDatabase.SaveAssets();Debug.Log("Incursion: original Standalone defines restored.");
 }
 public static void Build()
 {
  string symbols=PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone);
  if(symbols.Contains("ECHO_NATIVE_VERIFY"))throw new Exception("QA defines must be removed before delivery");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity"},locationPathName="Builds/EchoProtocol-Incursion-20261011/EchoProtocol.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.DetailedBuildReport});
  var result=new{result=report.summary.result.ToString(),totalErrors=report.summary.totalErrors,totalWarnings=report.summary.totalWarnings,seconds=report.summary.totalTime.TotalSeconds,symbols,errors=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content).ToArray()};
  File.WriteAllText("Docs/VisualEvidence/2026-10-11-Incursion/build-delivery.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Delivery build failed");
  Debug.Log("Incursion delivery Succeeded");
 }
}

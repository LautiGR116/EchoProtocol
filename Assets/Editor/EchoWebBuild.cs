using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EchoWebBuild
{
    public const string ScenePath = "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity";
    public const string PipelinePath = "Assets/Settings/Web_RPAsset.asset";
    public const string RendererPath = "Assets/Settings/Web_Renderer.asset";

    [MenuItem("Echo Protocol/Web/Configure release")]
    public static void Configure()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            throw new InvalidOperationException("Switch the Editor to WebGL before configuring the web release.");
        var target = NamedBuildTarget.WebGL;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.initialMemorySize = 256;
        PlayerSettings.WebGL.maximumMemorySize = 2048;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.template = "PROJECT:EchoWeb";
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Medium);
        PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
        var defines = PlayerSettings.GetScriptingDefineSymbols(target).Split(';')
            .Where(d => !string.IsNullOrWhiteSpace(d) && d != "ECHO_NATIVE_VERIFY")
            .Append("APP_UI_EDITOR_ONLY").Distinct();
        PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
#if UNITY_WEBGL
        // Keep iteration time bounded. LTO can be evaluated after human playtests.
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSize;
#endif
        if (!File.Exists(RendererPath)) AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset", RendererPath);
        if (!File.Exists(PipelinePath)) AssetDatabase.CopyAsset("Assets/Settings/PC_RPAsset.asset", PipelinePath);
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        var rendererData = new SerializedObject(renderer);
        rendererData.FindProperty("m_RenderingMode").intValue = (int)RenderingMode.Forward;
        rendererData.FindProperty("m_UseNativeRenderPass").boolValue = false;
        rendererData.ApplyModifiedPropertiesWithoutUndo();
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        var pipelineData = new SerializedObject(pipeline);
        pipelineData.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue = renderer;
        pipelineData.ApplyModifiedPropertiesWithoutUndo();
        pipeline.renderScale = .85f;
        pipeline.maxAdditionalLightsCount = 8;
        pipeline.mainLightShadowmapResolution = 1024;
        pipeline.additionalLightsShadowmapResolution = 1024;
        pipeline.shadowDistance = 30;
        EditorUtility.SetDirty(pipeline);
        ConfigureQuality(pipeline);
        AssetDatabase.SaveAssets();
        Debug.Log("Echo web settings saved: gzip, native decompression, 256–2048 MB memory, Medium stripping, WebGL2, Web-only Forward quality.");
    }

    private static void ConfigureQuality(UniversalRenderPipelineAsset pipeline)
    {
        var settings = new SerializedObject(QualitySettings.GetQualitySettings());
        var levels = settings.FindProperty("m_QualitySettings");
        int web = Array.IndexOf(QualitySettings.names, "Web");
        if (web < 0)
        {
            // The current last level is PC; Unity duplicates it when growing this array.
            if (QualitySettings.names.Last() != "PC")
                throw new InvalidOperationException("Expected PC as the last quality level before adding Web.");
            web = levels.arraySize++;
        }
        var defaults = settings.FindProperty("m_PerPlatformDefaultQuality");
        var platforms = Enumerable.Range(0, defaults.arraySize)
            .Select(i => defaults.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue).ToArray();
        for (int i = 0; i < levels.arraySize; i++)
        {
            var level = levels.GetArrayElementAtIndex(i);
            var excluded = level.FindPropertyRelative("excludedTargetPlatforms");
            if (i == web)
            {
                level.FindPropertyRelative("name").stringValue = "Web";
                level.FindPropertyRelative("customRenderPipeline").objectReferenceValue = pipeline;
                var others = platforms.Where(p => p != "WebGL").ToArray();
                excluded.arraySize = others.Length;
                for (int j = 0; j < others.Length; j++) excluded.GetArrayElementAtIndex(j).stringValue = others[j];
            }
            else if (!Enumerable.Range(0, excluded.arraySize).Any(j => excluded.GetArrayElementAtIndex(j).stringValue == "WebGL"))
                excluded.GetArrayElementAtIndex(excluded.arraySize++).stringValue = "WebGL";
        }
        for (int i = 0; i < defaults.arraySize; i++)
        {
            var pair = defaults.GetArrayElementAtIndex(i);
            if (pair.FindPropertyRelative("first").stringValue == "WebGL") pair.FindPropertyRelative("second").intValue = web;
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings.targetObject);
    }

    public static void Build()
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-buildOutput");
        string output = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Builds/EchoProtocol-Web-20261019";
        BuildTo(output);
    }

    public static void BuildTo(string output)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            throw new InvalidOperationException("The Editor must target WebGL.");
        if (PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL).Contains("ECHO_NATIVE_VERIFY"))
            throw new InvalidOperationException("QA code must not be included in the public player.");
        if (Directory.Exists(output)) throw new IOException("Use a new output folder; previous builds are preserved.");
        Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath }, target = BuildTarget.WebGL,
            locationPathName = output, options = BuildOptions.DetailedBuildReport
        });
        var summary = new { scene = ScenePath, result = report.summary.result.ToString(),
            errors = report.summary.totalErrors, warnings = report.summary.totalWarnings,
            seconds = report.summary.totalTime.TotalSeconds, bytes = report.summary.totalSize,
            errorMessages = report.steps.SelectMany(s => s.messages)
                .Where(m => m.type == LogType.Error || m.type == LogType.Exception).Select(m => m.content).ToArray() };
        // Keep build reports beside the web folder, outside the upload archive.
        File.WriteAllText(output + "-report.json", Newtonsoft.Json.JsonConvert.SerializeObject(summary, Newtonsoft.Json.Formatting.Indented));
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Echo web build failed.");
        Debug.Log("Echo web build succeeded: " + output);
    }
}

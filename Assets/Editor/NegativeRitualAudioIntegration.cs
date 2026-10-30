using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Explicit physical emitters for the authored ritual sequence. The caller owns
/// timing, movement, scene saving and undo grouping; this factory never plays.
/// </summary>
public static class NegativeRitualAudioIntegration
{
    public enum Cue
    {
        SteelCompression, WallStrike, CloseDrag, OverheadStrain,
        DryLatch, PressureInhale, PressureChoke, RemoteAnswer
    }

    const string ClipRoot = "Assets/_Game/Audio/Clips/NegativeRitual/NR_";
    const string MixerPath = "Assets/_Game/Audio/MX_Main.mixer";
    const string UndoName = "Configure Negative ritual audio";

    public static float RecommendedGain(Cue cue)
    {
        switch (cue)
        {
            case Cue.SteelCompression: return .16f;
            case Cue.WallStrike: return .30f;
            case Cue.CloseDrag: return .17f;
            case Cue.OverheadStrain: return .14f;
            case Cue.DryLatch: return .14f;
            case Cue.PressureInhale: return .14f;
            case Cue.PressureChoke: return .22f;
            case Cue.RemoteAnswer: return .16f;
            default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }
    }

    public static AudioClip Clip(Cue cue)
    {
        RecommendedGain(cue); // Validate enum before constructing an asset path.
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipRoot + cue + ".wav");
        if (!clip || clip.channels != 1 || clip.frequency != 48000)
            throw new InvalidOperationException("Expected the committed mono 48 kHz Negative ritual master: " + cue);
        return clip;
    }

    /// <summary>Run once after import, before creating scene emitters.</summary>
    public static void ImportClips()
    {
        RequireEditMode();
        foreach (Cue cue in Enum.GetValues(typeof(Cue)))
        {
            string path = ClipRoot + cue + ".wav";
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) throw new InvalidOperationException("Missing audio importer: " + path);
            var sample = importer.defaultSampleSettings;
            bool changed = sample.loadType != AudioClipLoadType.DecompressOnLoad
                || sample.compressionFormat != AudioCompressionFormat.PCM
                || sample.sampleRateSetting != AudioSampleRateSetting.PreserveSampleRate
                || !sample.preloadAudioData || importer.forceToMono || importer.loadInBackground;
            if (changed)
            {
                sample.loadType = AudioClipLoadType.DecompressOnLoad;
                sample.compressionFormat = AudioCompressionFormat.PCM;
                sample.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                sample.preloadAudioData = true;
                importer.defaultSampleSettings = sample;
                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.SaveAndReimport();
            }
            Clip(cue);
        }
    }

    /// <summary>
    /// Creates or updates one explicitly named direct child. Negative gain uses
    /// the authored default; explicit gain can lower that level. Does not scan
    /// or modify any other scene source, mixer, listener or runtime owner.
    /// </summary>
    public static AudioSource CreateSource(Transform parent, string name, Cue cue,
        Vector3 worldPosition, float gain = -1f)
    {
        RequireEditMode();
        if (!parent) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Use an explicit direct child name.", nameof(name));
        if (float.IsNaN(gain) || float.IsInfinity(gain))
            throw new ArgumentOutOfRangeException(nameof(gain));

        var clip = Clip(cue);
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        var group = mixer ? mixer.FindMatchingGroups("WorldSFX").SingleOrDefault(g => g.name == "WorldSFX") : null;
        if (!group) throw new InvalidOperationException("Existing WorldSFX mixer group is required.");

        var existing = parent.Cast<Transform>().Where(t => t.name == name).ToArray();
        if (existing.Length > 1) throw new InvalidOperationException("Duplicate ritual emitter: " + name);
        Transform child;
        AudioSource source;
        if (existing.Length == 0)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            go.transform.SetParent(parent, false);
            child = go.transform;
            source = Undo.AddComponent<AudioSource>(go);
        }
        else
        {
            child = existing[0];
            var sources = child.GetComponents<AudioSource>();
            if (sources.Length != 1)
                throw new InvalidOperationException("Existing ritual emitter must contain exactly one AudioSource: " + name);
            source = sources[0];
            Undo.RecordObjects(new UnityEngine.Object[] { child, source }, UndoName);
        }

        child.position = worldPosition;
        source.clip = clip;
        source.outputAudioMixerGroup = group;
        source.volume = gain < 0 ? RecommendedGain(cue) : Mathf.Clamp(gain, 0, RecommendedGain(cue));
        source.playOnAwake = false;
        source.loop = false;
        source.pitch = 1;
        source.panStereo = 0;
        source.spatialBlend = 1;
        source.spatialize = false;
        source.dopplerLevel = 0;
        source.spread = 0;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1.3f;
        source.maxDistance = cue == Cue.RemoteAnswer || cue == Cue.OverheadStrain ? 18f : 12f;
        source.reverbZoneMix = 0; // Tail is authored into the master, never stacked with zone reverb.
        source.priority = cue == Cue.WallStrike || cue == Cue.PressureChoke ? 48 : 100;
        source.ignoreListenerPause = false;
        source.ignoreListenerVolume = false;
        source.bypassEffects = false;
        source.bypassListenerEffects = false;
        source.bypassReverbZones = false;
        source.mute = false;
        source.enabled = true;
        EditorUtility.SetDirty(source);
        EditorUtility.SetDirty(child);
        PrefabUtility.RecordPrefabInstancePropertyModifications(source);
        return source;
    }

    static void RequireEditMode()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Configure Negative ritual audio outside Play Mode.");
    }
}

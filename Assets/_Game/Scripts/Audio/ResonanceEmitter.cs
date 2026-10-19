using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // One present-world source, or an explicitly non-diegetic score source.
    // No source is parented to, timed from, or claims to reproduce an Echo.
    [DisallowMultipleComponent]
    public sealed class ResonanceEmitter : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener;
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioLowPassFilter spectralBoundary;
        [SerializeField] private Collider[] partitions = new Collider[0];
        [SerializeField, Range(0, 1)] private float authoredGain = .3f;
        [SerializeField, Range(0, 1)] private float obstructedGain = .56f;
        [SerializeField] private float blockedCutoff = 1350f;
        [SerializeField] private float clearCutoff = 13000f;
        [SerializeField] private bool internalScore;
        private float level, target, pathGain = 1, logCutoff, inspectAt;
        private bool ready;
        public int PlayCount { get; private set; }
        public bool Obstructed { get; private set; }
        public bool IsPlaying => source && source.isPlaying;
        public float AppliedGain => source ? source.volume : 0;
        public float Cutoff => spectralBoundary ? spectralBoundary.cutoffFrequency : clearCutoff;
        public AudioSource Source => source;

        private void Awake()
        {
            ready = session && listener && source && source.clip && source.outputAudioMixerGroup
                && !source.ignoreListenerPause && !source.ignoreListenerVolume && !source.bypassListenerEffects
                && (internalScore ? source.spatialBlend == 0 : source.spatialBlend == 1 && spectralBoundary);
            if (!ready) { Debug.LogError("ResonanceEmitter requires an explicitly routed source, listener, session and physical acoustic boundary.", this); enabled = false; return; }
            source.playOnAwake = false; source.Stop(); source.volume = 0;
            logCutoff = Mathf.Log(clearCutoff);
        }

        public void Play()
        {
            if (!ready || session.IsPaused) return;
            source.Stop(); level = 0; target = 1; pathGain = 1; inspectAt = 0;
            source.volume = 0; source.Play(); PlayCount++;
        }
        public void SetLevel(float value) { target = Mathf.Clamp01(value); }
        public void FadeOut() { target = 0; }
        public void StopNow() { if (source) source.Stop(); target = level = 0; if (source) source.volume = 0; }

        private void LateUpdate()
        {
            if (!ready || session.IsPaused || Time.deltaTime <= 0) return;
            if (!source.isPlaying) return;
            if (!internalScore && Time.time >= inspectAt)
            {
                inspectAt = Time.time + .08f;
                Vector3 delta = source.transform.position - listener.position;
                float distance = delta.magnitude; Obstructed = false;
                // Explicit real architectural colliders only. No people, furniture,
                // trigger volumes or invisible acoustic collision surfaces.
                if (distance > .06f && distance < source.maxDistance + 1)
                {
                    var ray = new Ray(listener.position + delta.normalized * .025f, delta.normalized);
                    foreach (var p in partitions)
                        if (p && p.enabled && !p.isTrigger && p.gameObject.activeInHierarchy
                            && p.Raycast(ray, out _, distance - .05f)) { Obstructed = true; break; }
                }
                pathGain = Obstructed ? obstructedGain : 1;
            }
            level = Mathf.MoveTowards(level, target, Time.deltaTime / (target > level ? .075f : .25f));
            float intended = authoredGain * level * pathGain;
            source.volume = Mathf.MoveTowards(source.volume, intended, authoredGain * Time.deltaTime / .12f);
            if (spectralBoundary && !internalScore)
            {
                Vector3 direction = (source.transform.position - listener.position).normalized;
                float rear = Mathf.Clamp01(-Vector3.Dot(listener.forward, direction));
                // Gentle head-shadow cue, not an HRTF claim. Mono retains all content.
                float cutoff = Obstructed ? blockedCutoff : Mathf.Lerp(clearCutoff, clearCutoff * .55f, rear);
                logCutoff = Mathf.MoveTowards(logCutoff, Mathf.Log(cutoff), Time.deltaTime * 15);
                spectralBoundary.cutoffFrequency = Mathf.Exp(logCutoff);
            }
            if (target == 0 && level == 0 && source.volume < .0001f) source.Stop();
        }
        private void OnDisable() { StopNow(); }
    }
}

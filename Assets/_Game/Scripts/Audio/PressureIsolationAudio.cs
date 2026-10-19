using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Owns four physical sources; neither recordings nor the mix bus.
    // PressureIsolation (-90) advances before this LateUpdate (200).
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    public sealed class PressureIsolationAudio : MonoBehaviour
    {
        [Header("Explicit system and physical sources")]
        [SerializeField] private PressureIsolation isolation;
        [SerializeField] private GameSession session;
        [SerializeField] private AudioSource motorSource;
        [SerializeField] private AudioSource controlSource;
        [SerializeField] private AudioSource latchSource;
        [SerializeField] private AudioSource purgeSource;

        [Header("Existing traced clips")]
        [SerializeField] private AudioClip motorLoopClip;
        [SerializeField] private AudioClip controlLoopClip;
        [SerializeField] private AudioClip latchClip;
        [SerializeField] private AudioClip purgeLoopClip;

        [Header("Provisional mix: requires listening")]
        [SerializeField, Range(0f, 1f)] private float motorGain = .20f;
        [SerializeField, Range(0f, 1f)] private float controlGain = .07f;
        [SerializeField, Range(0f, 1f)] private float latchGain = .13f;
        [SerializeField, Range(0f, 1f)] private float purgeGain = .14f;
        [SerializeField, Range(.25f, 1f)] private float stoppedMotorPitchRatio = .65f;
        [SerializeField, Min(.005f)] private float controlFadeSeconds = .05f;

        private bool ready;
        private int observedCommit, observedRestore;
        private float previousHandle, controlEnvelope, motorAuthoredPitch;

        public int MotorStartCount { get; private set; }
        public int ControlStartCount { get; private set; }
        public int LatchPlayCount { get; private set; }
        public int PurgeStartCount { get; private set; }
        public bool MachineSourcesStopped => ready && !motorSource.isPlaying && !controlSource.isPlaying
            && !latchSource.isPlaying && !purgeSource.isPlaying;

        private void Awake()
        {
            if (!isolation || !session || !motorLoopClip || !controlLoopClip || !latchClip || !purgeLoopClip || !DistinctSources())
            {
                Debug.LogError("PressureIsolationAudio requires the isolation, session, four distinct physical sources and four traced clips.", this);
                enabled = false;
                return;
            }
            motorAuthoredPitch = motorSource.pitch;
            ConfigurePlayback(motorSource, motorLoopClip, true);
            ConfigurePlayback(controlSource, controlLoopClip, true);
            ConfigurePlayback(latchSource, latchClip, false);
            ConfigurePlayback(purgeSource, purgeLoopClip, true);
            ready = true;
            EstablishBaseline();
        }

        private bool DistinctSources()
        {
            var sources = new[] { motorSource, controlSource, latchSource, purgeSource };
            for (int i = 0; i < sources.Length; i++)
            {
                if (!sources[i]) return false;
                for (int j = 0; j < i; j++) if (sources[i] == sources[j]) return false;
            }
            return true;
        }

        private static void ConfigurePlayback(AudioSource source, AudioClip clip, bool looping)
        {
            // Transform, mixer, spatial blend, attenuation and distances remain authored.
            source.playOnAwake = false;
            source.loop = looping;
            source.clip = clip;
        }

        private void OnEnable() { if (ready) EstablishBaseline(); }
        private void Start() { if (ready) EstablishBaseline(); }

        private void EstablishBaseline()
        {
            observedCommit = isolation.CommitCount;
            observedRestore = isolation.RestoreRevision;
            previousHandle = isolation.HandleFraction;
            StopOwnedSources();
        }

        private void LateUpdate()
        {
            if (!ready) return;
            // Restored state is a new baseline, never a live latch or purge impulse.
            if (isolation.RestoreRevision != observedRestore) EstablishBaseline();
            if (session.IsPaused || Time.timeScale <= 0f) return;
            if (!session.HasStarted || !isolation.isActiveAndEnabled)
            {
                EstablishBaseline();
                return;
            }

            int commit = isolation.CommitCount;
            bool liveLatch = commit > observedCommit && isolation.IsLatched;
            observedCommit = commit;
            if (liveLatch)
            {
                latchSource.volume = latchGain;
                latchSource.Play();
                LatchPlayCount++;
            }

            // The plant can run before entering its service volume. Once latched,
            // the model owns the coast-down; leaving the chapter never rearms it.
            float motorLevel = Mathf.Clamp01(isolation.MotorLevel01);
            motorSource.pitch = motorAuthoredPitch * Mathf.Lerp(stoppedMotorPitchRatio, 1f, motorLevel);
            motorSource.volume = motorGain * motorLevel;
            if (motorLevel > 0f)
            {
                if (!motorSource.isPlaying) { motorSource.Play(); MotorStartCount++; }
            }
            else StopAtZero(motorSource);

            // Friction follows actual handle displacement, including its physical
            // spring return. It never reverses the recording or runs on paused time.
            float handle = isolation.HandleFraction;
            bool moving = isolation.Active && !isolation.IsLatched && Mathf.Abs(handle - previousHandle) > .00001f;
            previousHandle = handle;
            controlEnvelope = Mathf.MoveTowards(controlEnvelope, moving ? 1f : 0f, Time.deltaTime / controlFadeSeconds);
            controlSource.volume = controlGain * controlEnvelope;
            if (controlEnvelope > 0f)
            {
                if (!controlSource.isPlaying) { controlSource.Play(); ControlStartCount++; }
            }
            else StopAtZero(controlSource);

            // Valve B emits where it is mounted while real pressure decays. There
            // is no four-second discharge tail after the three-second drain ends.
            bool purging = isolation.Active && isolation.IsLatched && !isolation.IsDrained;
            float pressure = purging ? Mathf.Clamp01(isolation.Pressure01) : 0f;
            purgeSource.volume = purgeGain * pressure;
            if (pressure > 0f)
            {
                if (!purgeSource.isPlaying) { purgeSource.Play(); PurgeStartCount++; }
            }
            else StopAtZero(purgeSource);
        }

        private static void StopAtZero(AudioSource source)
        {
            source.volume = 0f;
            if (source.isPlaying) source.Stop();
        }

        private void StopOwnedSources()
        {
            controlEnvelope = 0f;
            if (motorSource) { motorSource.Stop(); motorSource.volume = 0f; }
            if (controlSource) { controlSource.Stop(); controlSource.volume = 0f; }
            if (latchSource) { latchSource.Stop(); latchSource.volume = 0f; }
            if (purgeSource) { purgeSource.Stop(); purgeSource.volume = 0f; }
        }

        private void OnDisable() { if (ready) StopOwnedSources(); }

        private void OnValidate()
        {
            motorGain = Mathf.Clamp01(motorGain); controlGain = Mathf.Clamp01(controlGain);
            latchGain = Mathf.Clamp01(latchGain); purgeGain = Mathf.Clamp01(purgeGain);
            stoppedMotorPitchRatio = Mathf.Clamp(stoppedMotorPitchRatio, .25f, 1f);
            controlFadeSeconds = Mathf.Max(.005f, controlFadeSeconds);
        }
    }
}

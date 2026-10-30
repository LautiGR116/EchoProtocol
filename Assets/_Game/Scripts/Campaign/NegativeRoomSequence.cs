using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // The room changes in the present. This component never reads or writes
    // body samples, invents Echo audio, resets a take, or controls the camera.
    [DefaultExecutionOrder(300), DisallowMultipleComponent]
    public sealed class NegativeRoomSequence : MonoBehaviour
    {
        public enum RoomStage { Teaching, Sealed, Listening }

        [SerializeField] private GameSession session;
        [SerializeField] private MachineCoverEncounter encounter;
        [SerializeField] private LoopController loop;
        [SerializeField] private SessionAudio audioOutput;
        [Header("Present-world shutters / visual geometry only")]
        [SerializeField] private Transform[] shutters;
        [SerializeField] private Vector3[] closedLocalOffsets;
        [SerializeField] private Vector3[] closedLocalAngles;
        [SerializeField, Min(.5f)] private float closeSeconds = 2.4f;
        [SerializeField, Min(0)] private float shutterStagger = .12f;
        [Header("Lighting / reserve, exit and reading lights are protected")]
        [SerializeField] private Light[] ambientLights;
        [SerializeField] private Light[] workLights;
        [SerializeField] private Light[] protectedLights;
        [SerializeField] private Renderer[] luminousPanels;
        [SerializeField, ColorUsage(false, true)] private Color emissionColor = new Color(2.2f, 1.05f, .24f);
        [SerializeField, Min(.1f)] private float blackoutSeconds = 1.2f;
        [SerializeField, Min(.1f)] private float workRiseSeconds = .45f;
        [SerializeField, Min(.1f)] private float workFallSeconds = .8f;
        [Header("Dedicated physical emitters / existing credited clips")]
        [Tooltip("0: moving shutter friction, 1: seated shutter contact, 2: first replay-silence panel movement. Each source belongs to its visible panel.")]
        [SerializeField] private AudioSource[] cues;
        [SerializeField, Min(0)] private int silencePanelIndex = 2;
        [SerializeField] private Vector3 silenceSettleOffset = new Vector3(0, -.18f, 0);
        [SerializeField, Min(.2f)] private float silenceSettleSeconds = 1.15f;
        [Tooltip("Optional physical pressure-cue cutoff before the separate ritual panel strikes. Zero retains the full clip.")]
        [SerializeField, Min(0)] private float silenceCueSeconds;

        private Vector3[] openPositions;
        private Quaternion[] openRotations;
        private float[] ambientLevels, workLevels, cueLevels;
        private MaterialPropertyBlock emission;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private bool ready, closureContactPlayed, sawReplayRun, wasRunning;
        private float closingTime, ambient01 = 1, settlingTime;

        public RoomStage Stage { get; private set; }
        public float ShutterClosure01 => Mathf.Clamp01(closingTime / ClosureDuration);
        public float WorkLight01 { get; private set; }
        public bool SilenceResponsePlayed { get; private set; }
        private float ClosureDuration => closeSeconds + Mathf.Max(0, (shutters == null ? 0 : shutters.Length) - 1) * shutterStagger;

        private void Awake()
        {
            ready = session && encounter && loop && audioOutput && shutters != null && shutters.Length > 0
                && closedLocalOffsets != null && closedLocalOffsets.Length == shutters.Length
                && closedLocalAngles != null && closedLocalAngles.Length == shutters.Length
                && ambientLights != null && workLights != null && cues != null && cues.Length == 3;
            if (ready)
            {
                foreach (var shutter in shutters) ready &= shutter != null;
                foreach (var cue in cues) ready &= cue && cue.clip && cue.outputAudioMixerGroup;
                // Moving render geometry must never silently become a new gate.
                foreach (var shutter in shutters)
                    if (shutter && shutter.GetComponentsInChildren<Collider>(true).Length > 0) ready = false;
            }
            if (!ready)
            {
                Debug.LogError("NegativeRoomSequence requires its explicit room references, collider-free shutters and three routed physical cues.", this);
                enabled = false; return;
            }
            openPositions = new Vector3[shutters.Length];
            openRotations = new Quaternion[shutters.Length];
            for (int i = 0; i < shutters.Length; i++)
            {
                openPositions[i] = shutters[i].localPosition;
                openRotations[i] = shutters[i].localRotation;
            }
            ambientLevels = ReadLightLevels(ambientLights);
            workLevels = ReadLightLevels(workLights);
            cueLevels = new float[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                var source = cues[i];
                cueLevels[i] = Mathf.Min(.3f, source.volume);
                source.playOnAwake = false; source.loop = i == 0;
                source.spatialBlend = 1; source.dopplerLevel = 0;
                source.ignoreListenerPause = false; source.bypassListenerEffects = false;
                source.Stop();
            }
            emission = new MaterialPropertyBlock();
            loop.LoopCompleted += OnLoopCompleted;
            ApplyPresentation();
        }

        private float[] ReadLightLevels(Light[] lights)
        {
            var levels = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++) levels[i] = lights[i] ? lights[i].intensity : 0;
            return levels;
        }

        private void OnLoopCompleted()
        {
            if (!ready || !encounter.Active) return;
            if (Stage == RoomStage.Sealed) Stage = RoomStage.Listening;
            // A deliberate replay reset is not the recorded pump falling silent.
            // The room remains sealed; only the new real run arms its silence.
            sawReplayRun = false;
            wasRunning = encounter.Machine.Held;
        }

        private void LateUpdate()
        {
            if (!ready || !session.HasStarted || !encounter.Active) return;
            if (encounter.Failed) { foreach (var source in cues) source.Stop(); return; }
            if (session.IsPaused || Time.deltaTime <= 0) return;
            if (encounter.Complete && Stage != RoomStage.Listening)
            {
                // Loading an already completed room must not replay the lesson
                // or leave the architecture open. No historical cue is invented.
                Stage = RoomStage.Listening; closingTime = ClosureDuration;
                ambient01 = 0; closureContactPlayed = SilenceResponsePlayed = true;
                settlingTime = silenceSettleSeconds;
                cues[0].Stop();
            }
            if (Stage == RoomStage.Teaching && encounter.PreparationPhase > 0 && !encounter.Machine.PlayerHeld)
            {
                Stage = RoomStage.Sealed;
                PlayCue(0);
            }
            if (Stage != RoomStage.Teaching)
            {
                closingTime = Mathf.Min(ClosureDuration, closingTime + Time.deltaTime);
                if (closingTime >= ClosureDuration && !closureContactPlayed)
                {
                    closureContactPlayed = true;
                    cues[0].Stop(); PlayCue(1);
                }
                if (cues[0].isPlaying)
                {
                    float onset = Mathf.Clamp01(closingTime / .15f);
                    float tail = Mathf.Clamp01((ClosureDuration - closingTime) / .25f);
                    cues[0].volume = CueLevel(0) * onset * tail;
                }
            }

            bool running = encounter.Machine.Held;
            if (Stage == RoomStage.Listening && !encounter.Failed && !encounter.Complete)
            {
                if (encounter.Machine.ReplayHeld) sawReplayRun = true;
                if (sawReplayRun && wasRunning && !running && !SilenceResponsePlayed)
                {
                    SilenceResponsePlayed = true;
                    PlayCue(2);
                }
            }
            wasRunning = running;
            if (SilenceResponsePlayed) settlingTime = Mathf.Min(silenceSettleSeconds, settlingTime + Time.deltaTime);
            if (SilenceResponsePlayed && silenceCueSeconds > 0 && cues[2].isPlaying)
            {
                cues[2].volume = CueLevel(2) * Mathf.Clamp01((silenceCueSeconds - settlingTime) / .12f);
                if (settlingTime >= silenceCueSeconds) cues[2].Stop();
            }
            float ambientTarget = Stage == RoomStage.Teaching ? 1 : Stage == RoomStage.Sealed ? .18f : 0;
            ambient01 = Mathf.MoveTowards(ambient01, ambientTarget, Time.deltaTime / blackoutSeconds);
            WorkLight01 = Mathf.MoveTowards(WorkLight01, running ? 1 : 0,
                Time.deltaTime / (running ? workRiseSeconds : workFallSeconds));
            ApplyPresentation();
        }

        private void ApplyPresentation()
        {
            for (int i = 0; i < shutters.Length; i++)
            {
                float closure = Mathf.SmoothStep(0, 1, Mathf.Clamp01((closingTime - i * shutterStagger) / closeSeconds));
                Vector3 settle = i == silencePanelIndex && SilenceResponsePlayed
                    ? silenceSettleOffset * Mathf.SmoothStep(0, 1, settlingTime / silenceSettleSeconds) : Vector3.zero;
                shutters[i].localPosition = openPositions[i] + closedLocalOffsets[i] * closure + settle;
                shutters[i].localRotation = openRotations[i] * Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(closedLocalAngles[i]), closure);
            }
            ApplyLights(ambientLights, ambientLevels, ambient01);
            ApplyLights(workLights, workLevels, WorkLight01);
            if (luminousPanels == null) return;
            float panelLevel = Stage == RoomStage.Listening ? WorkLight01 : Mathf.Max(.18f * ambient01, WorkLight01);
            foreach (var panel in luminousPanels)
            {
                if (!panel) continue;
                panel.GetPropertyBlock(emission);
                emission.SetColor(EmissionColor, emissionColor * panelLevel);
                panel.SetPropertyBlock(emission);
            }
        }

        private void ApplyLights(Light[] lights, float[] levels, float scale)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (!lights[i] || IsProtected(lights[i])) continue;
                lights[i].intensity = levels[i] * scale;
            }
        }

        private bool IsProtected(Light light)
        {
            if (protectedLights != null) foreach (var reserved in protectedLights) if (reserved == light) return true;
            return false;
        }

        private float CueLevel(int index) => cueLevels[index] * (audioOutput.ReducedDynamics ? .75f : 1);
        private void PlayCue(int index) { cues[index].volume = CueLevel(index); cues[index].Play(); }
        private void OnDisable()
        {
            if (cues != null) foreach (var source in cues) if (source) source.Stop();
        }
        private void OnDestroy() { if (loop) loop.LoopCompleted -= OnLoopCompleted; }
    }
}

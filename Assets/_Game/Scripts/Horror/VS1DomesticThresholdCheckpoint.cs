using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VS1DomesticThresholdCheckpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private LoopController loopController;
        [SerializeField] private Transform player;
        [SerializeField] private Transform recallPoint;

        [Header("Door Slam")]
        [SerializeField] private VS1DomesticDoorReveal doorReveal;
        [SerializeField] private AudioSource slamSource;
        [Tooltip("Optional recorded contact; legacy scenes retain their original procedural sound.")]
        [SerializeField] private AudioClip authoredSlam;
        [SerializeField, Min(0f)] private float slamDelaySeconds = 1.1f;
        [SerializeField, Range(0f, 1f)] private float slamVolume = 0.58f;

        private const int SampleRate = 48000;
        private const float SlamClipDuration = 0.38f;

        private float elapsed;
        private bool slamRequested;

        public bool IsCheckpointCommitted { get; private set; }

        public bool HasSlammed { get; private set; }

        public int CommitCount { get; private set; }

        public int SlamPlayCount { get; private set; }

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            if (!trigger.isTrigger
                || loopController == null
                || player == null
                || recallPoint == null
                || doorReveal == null
                || slamSource == null)
            {
                Debug.LogError(
                    "VS1DomesticThresholdCheckpoint requires a trigger, loop, Player, recall point, door, and slam source.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSlamSource();
            slamSource.clip = authoredSlam ? authoredSlam : CreateSlamClip();
        }

        private void Update()
        {
            if (!IsCheckpointCommitted || HasSlammed)
            {
                return;
            }

            if (!slamRequested)
            {
                elapsed += Time.deltaTime;
                if (elapsed < slamDelaySeconds)
                {
                    return;
                }

                slamRequested = doorReveal.BeginSlamClose();
                return;
            }

            if (!doorReveal.IsFullyClosedAfterSlam)
            {
                return;
            }

            slamSource.Play();
            HasSlammed = true;
            SlamPlayCount++;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCheckpointCommitted
                || (other.transform != player
                    && !other.transform.IsChildOf(player)))
            {
                return;
            }

            if (!loopController.SetRecallCheckpoint(recallPoint))
            {
                return;
            }

            IsCheckpointCommitted = true;
            CommitCount++;
            elapsed = 0f;
        }

        private void ConfigureSlamSource()
        {
            slamSource.playOnAwake = false;
            slamSource.loop = false;
            slamSource.spatialBlend = 1f;
            slamSource.dopplerLevel = 0f;
            slamSource.priority = 120;
            slamSource.rolloffMode = AudioRolloffMode.Linear;
            slamSource.minDistance = 4f;
            slamSource.maxDistance = 22f;
            slamSource.volume = slamVolume;
        }

        private static AudioClip CreateSlamClip()
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * SlamClipDuration);
            float[] samples = new float[sampleCount];
            uint noiseState = 0x6E624EB7u;
            float filteredNoise = 0f;

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)SampleRate;
                float progress = time / SlamClipDuration;
                float bodyEnvelope = Mathf.Exp(-11f * time);
                float slapEnvelope = Mathf.Exp(-34f * time);
                float frequency = Mathf.Lerp(92f, 47f, progress);
                float body = Mathf.Sin(2f * Mathf.PI * frequency * time)
                    * bodyEnvelope;

                noiseState = noiseState * 1664525u + 1013904223u;
                float white = ((noiseState >> 8) / 8388607.5f) - 1f;
                filteredNoise = Mathf.Lerp(filteredNoise, white, 0.32f);
                float wood = filteredNoise * slapEnvelope;
                float panel = Mathf.Sin(2f * Mathf.PI * 238f * time)
                    * Mathf.Exp(-16f * time);
                float latch = Mathf.Sin(2f * Mathf.PI * 910f * time)
                    * Mathf.Exp(-52f * time);

                samples[index] = Mathf.Clamp(
                    (body * 0.44f) + (wood * 0.34f)
                    + (panel * 0.18f) + (latch * 0.08f),
                    -0.58f,
                    0.58f);
            }

            AudioClip clip = AudioClip.Create(
                "VS1 Domestic Door Slam (Runtime)",
                sampleCount,
                1,
                SampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (slamSource != null)
            {
                slamSource.Stop();
            }

            elapsed = 0f;
            slamRequested = false;
            IsCheckpointCommitted = false;
            HasSlammed = false;
            CommitCount = 0;
            SlamPlayCount = 0;
        }

        private void OnValidate()
        {
            slamDelaySeconds = Mathf.Max(0f, slamDelaySeconds);
            slamVolume = Mathf.Clamp01(slamVolume);
        }
    }
}

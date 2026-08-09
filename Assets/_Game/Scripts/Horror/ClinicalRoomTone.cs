using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class ClinicalRoomTone : MonoBehaviour
    {
        private const float LoopDuration = 4f;

        [SerializeField] private EnvironmentalDiscrepancyController discrepancy;
        [SerializeField, Range(0f, 0.5f)] private float baselineVolume = 0.14f;
        [SerializeField, Min(0.05f)] private float silenceFadeDuration = 0.8f;

        private AudioSource roomToneSource;
        private AudioClip generatedRoomTone;
        private bool isFading;

        public bool IsSilenced { get; private set; }

        public float CurrentVolume => roomToneSource == null
            ? 0f
            : roomToneSource.volume;

        public bool IsPlaying => roomToneSource != null && roomToneSource.isPlaying;

        private void Awake()
        {
            roomToneSource = GetComponent<AudioSource>();
            if (discrepancy == null)
            {
                Debug.LogError(
                    "ClinicalRoomTone requires an environmental discrepancy reference.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            generatedRoomTone = CreateRoomToneClip();
            roomToneSource.clip = generatedRoomTone;
        }

        private void OnEnable()
        {
            if (discrepancy != null)
            {
                discrepancy.Revealed += BeginSilenceFade;

                if (roomToneSource != null && discrepancy.IsRevealed)
                {
                    SilenceImmediately();
                }
            }
        }

        private void Start()
        {
            if (!enabled)
            {
                return;
            }

            if (discrepancy.IsRevealed)
            {
                SilenceImmediately();
                return;
            }

            roomToneSource.volume = baselineVolume;
            roomToneSource.Play();
        }

        private void Update()
        {
            if (!isFading)
            {
                return;
            }

            float fadeSpeed = baselineVolume / silenceFadeDuration;
            roomToneSource.volume = Mathf.MoveTowards(
                roomToneSource.volume,
                0f,
                fadeSpeed * Time.unscaledDeltaTime);

            if (roomToneSource.volume > Mathf.Epsilon)
            {
                return;
            }

            roomToneSource.Stop();
            isFading = false;
            IsSilenced = true;
        }

        private void OnDisable()
        {
            if (discrepancy != null)
            {
                discrepancy.Revealed -= BeginSilenceFade;
            }
        }

        private void OnDestroy()
        {
            if (generatedRoomTone != null)
            {
                Destroy(generatedRoomTone);
            }
        }

        private void ConfigureSource()
        {
            roomToneSource.playOnAwake = false;
            roomToneSource.loop = true;
            roomToneSource.spatialBlend = 0f;
            roomToneSource.dopplerLevel = 0f;
            roomToneSource.panStereo = 0f;
            roomToneSource.priority = 200;
            roomToneSource.volume = baselineVolume;
        }

        private AudioClip CreateRoomToneClip()
        {
            int sampleRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
            int sampleCount = Mathf.CeilToInt(sampleRate * LoopDuration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float slowModulation = 0.88f
                    + 0.12f * Mathf.Sin(2f * Mathf.PI * 0.25f * time);
                float hum = 0.42f * Sine(50f, time)
                    + 0.38f * Sine(100f, time)
                    + 0.13f * Sine(200f, time)
                    + 0.04f * Sine(400f, time)
                    + 0.025f * Sine(173f, time)
                    + 0.018f * Sine(257f, time);

                samples[index] = hum * slowModulation * 0.7f;
            }

            AudioClip clip = AudioClip.Create(
                "M6.1 Clinical Room Tone",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void BeginSilenceFade()
        {
            if (!IsSilenced)
            {
                isFading = true;
            }
        }

        private void SilenceImmediately()
        {
            roomToneSource.volume = 0f;
            roomToneSource.Stop();
            isFading = false;
            IsSilenced = true;
        }

        private static float Sine(float frequency, float time)
        {
            return Mathf.Sin(2f * Mathf.PI * frequency * time);
        }

        private void OnValidate()
        {
            baselineVolume = Mathf.Clamp(baselineVolume, 0f, 0.5f);
            silenceFadeDuration = Mathf.Max(0.05f, silenceFadeDuration);
        }
    }
}

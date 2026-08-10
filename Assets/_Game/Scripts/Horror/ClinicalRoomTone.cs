using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class ClinicalRoomTone : MonoBehaviour
    {
        [SerializeField] private EnvironmentalDiscrepancyController discrepancy;
        [SerializeField] private AudioClip facilityAirClip;
        [SerializeField, Range(0f, 0.5f)] private float baselineVolume = 0.14f;
        [SerializeField, Min(0.05f)] private float silenceFadeDuration = 0.8f;

        private AudioSource roomToneSource;
        private bool isFading;

        public bool IsSilenced { get; private set; }

        public float CurrentVolume => roomToneSource == null
            ? 0f
            : roomToneSource.volume;

        public bool IsPlaying => roomToneSource != null && roomToneSource.isPlaying;

        private void Awake()
        {
            roomToneSource = GetComponent<AudioSource>();
            if (discrepancy == null || facilityAirClip == null)
            {
                Debug.LogError(
                    "ClinicalRoomTone requires discrepancy and facility air references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            roomToneSource.clip = facilityAirClip;
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

        private void OnValidate()
        {
            baselineVolume = Mathf.Clamp(baselineVolume, 0f, 0.5f);
            silenceFadeDuration = Mathf.Max(0.05f, silenceFadeDuration);
        }
    }
}

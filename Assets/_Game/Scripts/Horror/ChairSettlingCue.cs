using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class ChairSettlingCue : MonoBehaviour
    {
        private const float ClipDuration = 0.9f;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private AudioSource settlingSource;

        [Header("Cue")]
        [SerializeField, Min(0.5f)] private float triggerDistance = 1.3f;
        [SerializeField, Range(0f, 0.25f)] private float sourceVolume = 0.1f;

        private AudioClip generatedSettlingCue;

        public bool HasPlayed { get; private set; }

        public int PlayCount { get; private set; }

        private void Awake()
        {
            if (player == null || settlingSource == null)
            {
                Debug.LogError(
                    "ChairSettlingCue requires explicit Player and AudioSource references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            generatedSettlingCue = CreateSettlingClip();
            settlingSource.clip = generatedSettlingCue;
        }

        private void Update()
        {
            if (HasPlayed || !player.gameObject.activeInHierarchy)
            {
                return;
            }

            Vector3 offset = player.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > triggerDistance * triggerDistance)
            {
                return;
            }

            HasPlayed = true;
            PlayCount++;
            settlingSource.Play();
        }

        private void OnDisable()
        {
            if (settlingSource != null)
            {
                settlingSource.Stop();
            }
        }

        private void OnDestroy()
        {
            if (generatedSettlingCue != null)
            {
                Destroy(generatedSettlingCue);
            }
        }

        private void ConfigureSource()
        {
            settlingSource.playOnAwake = false;
            settlingSource.loop = false;
            settlingSource.spatialBlend = 1f;
            settlingSource.dopplerLevel = 0f;
            settlingSource.panStereo = 0f;
            settlingSource.priority = 180;
            settlingSource.rolloffMode = AudioRolloffMode.Linear;
            settlingSource.minDistance = 1.25f;
            settlingSource.maxDistance = 8f;
            settlingSource.volume = sourceVolume;
        }

        private static AudioClip CreateSettlingClip()
        {
            int sampleRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
            int sampleCount = Mathf.RoundToInt(sampleRate * ClipDuration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float normalizedTime = time / ClipDuration;
                float remaining = ClipDuration - time;
                float attack = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(time / 0.03f));
                float release = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(remaining / 0.28f));
                float decay = Mathf.Exp(-1.8f * time);

                float phaseCycles = 190f * time
                    + 0.5f * (125f - 190f) * time * normalizedTime;
                float phase = 2f * Mathf.PI * phaseCycles;
                float creak = 0.56f * Mathf.Sin(phase)
                    + 0.15f * Mathf.Sin(phase * 2.03f)
                    + 0.06f * Mathf.Sin(phase * 3.91f);
                float texture = Mathf.Sin(2f * Mathf.PI * 37f * time)
                    * Mathf.Sin(2f * Mathf.PI * 83f * time)
                    * 0.08f;

                samples[index] = (creak + texture) * attack * release * decay * 0.55f;
            }

            AudioClip clip = AudioClip.Create(
                "M6.3 Chair Settling",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnValidate()
        {
            triggerDistance = Mathf.Max(0.5f, triggerDistance);
            sourceVolume = Mathf.Clamp(sourceVolume, 0f, 0.25f);
        }
    }
}

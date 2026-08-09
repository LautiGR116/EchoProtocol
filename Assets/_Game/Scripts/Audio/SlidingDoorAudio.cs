using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Audio
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SlidingDoorAudio : MonoBehaviour, ILoopResettable
    {
        private const float MotorClipDuration = 0.5f;
        private const float EndpointClipDuration = 0.14f;

        [Header("References")]
        [SerializeField] private SlidingDoor door;
        [SerializeField] private Rigidbody doorBody;
        [SerializeField] private AudioSource mechanismSource;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float mechanismVolume = 0.2f;
        [SerializeField, Min(0.005f)] private float edgeFadeDuration = 0.02f;

        private AudioClip motorClip;
        private AudioClip endpointClip;
        private bool motorActive;
        private bool fadingOut;
        private Vector3 previousBodyPosition;

        public bool IsMotorActive => motorActive;

        public int MotorStartCount { get; private set; }

        public int EndpointCueCount { get; private set; }

        private void Awake()
        {
            if (door == null || doorBody == null || mechanismSource == null)
            {
                Debug.LogError(
                    "SlidingDoorAudio requires explicit door, door body, and AudioSource references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            int sampleRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
            motorClip = CreateMotorClip(sampleRate);
            endpointClip = CreateEndpointClip(sampleRate);
            previousBodyPosition = doorBody.position;
            ResetLoopState();
        }

        private void Update()
        {
            if (motorActive && !fadingOut)
            {
                mechanismSource.volume = Mathf.MoveTowards(
                    mechanismSource.volume,
                    mechanismVolume,
                    mechanismVolume * Time.unscaledDeltaTime / edgeFadeDuration);
                return;
            }

            if (!fadingOut)
            {
                return;
            }

            mechanismSource.volume = Mathf.MoveTowards(
                mechanismSource.volume,
                0f,
                mechanismVolume * Time.unscaledDeltaTime / edgeFadeDuration);

            if (mechanismSource.volume > Mathf.Epsilon)
            {
                return;
            }

            mechanismSource.Stop();
            mechanismSource.clip = endpointClip;
            mechanismSource.loop = false;
            mechanismSource.volume = mechanismVolume;
            mechanismSource.Play();
            fadingOut = false;
            EndpointCueCount++;
        }

        private void FixedUpdate()
        {
            Vector3 currentBodyPosition = doorBody.position;
            bool doorMoved = Vector3.SqrMagnitude(
                currentBodyPosition - previousBodyPosition) > 0.00000001f;
            previousBodyPosition = currentBodyPosition;

            if (doorMoved)
            {
                if (!motorActive)
                {
                    StartMotor();
                }

                return;
            }

            if (motorActive && (door.IsFullyOpen || door.IsFullyClosed))
            {
                motorActive = false;
                fadingOut = true;
            }
        }

        public void ResetLoopState()
        {
            motorActive = false;
            fadingOut = false;

            if (mechanismSource == null)
            {
                return;
            }

            mechanismSource.Stop();
            mechanismSource.clip = motorClip;
            mechanismSource.loop = true;
            mechanismSource.volume = mechanismVolume;
            previousBodyPosition = doorBody == null
                ? Vector3.zero
                : doorBody.position;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void OnDestroy()
        {
            if (motorClip != null)
            {
                Destroy(motorClip);
            }

            if (endpointClip != null)
            {
                Destroy(endpointClip);
            }
        }

        private void StartMotor()
        {
            fadingOut = false;
            motorActive = true;
            mechanismSource.Stop();
            mechanismSource.clip = motorClip;
            mechanismSource.loop = true;
            mechanismSource.volume = 0f;
            mechanismSource.Play();
            MotorStartCount++;
        }

        private void ConfigureSource()
        {
            mechanismSource.playOnAwake = false;
            mechanismSource.loop = true;
            mechanismSource.spatialBlend = 1f;
            mechanismSource.dopplerLevel = 0f;
            mechanismSource.panStereo = 0f;
            mechanismSource.priority = 160;
            mechanismSource.rolloffMode = AudioRolloffMode.Linear;
            mechanismSource.minDistance = 2f;
            mechanismSource.maxDistance = 14f;
            mechanismSource.volume = mechanismVolume;
        }

        private static AudioClip CreateMotorClip(int sampleRate)
        {
            int sampleCount = Mathf.RoundToInt(sampleRate * MotorClipDuration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float modulation = 0.82f
                    + 0.18f * Sine(4f, time);
                float motor = 0.42f * Sine(70f, time)
                    + 0.23f * Sine(140f, time)
                    + 0.08f * Sine(280f, time);
                samples[index] = motor * modulation;
            }

            AudioClip clip = AudioClip.Create(
                "M6.2 Door Motor",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateEndpointClip(int sampleRate)
        {
            int sampleCount = Mathf.RoundToInt(sampleRate * EndpointClipDuration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float remaining = EndpointClipDuration - time;
                float attack = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / 0.005f));
                float release = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(remaining / 0.02f));
                float decay = Mathf.Exp(-18f * time);
                float impact = 0.58f * Sine(85f, time)
                    + 0.16f * Sine(480f, time);
                samples[index] = impact * attack * release * decay;
            }

            AudioClip clip = AudioClip.Create(
                "M6.2 Door Endpoint",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float Sine(float frequency, float time)
        {
            return Mathf.Sin(2f * Mathf.PI * frequency * time);
        }

        private void OnValidate()
        {
            mechanismVolume = Mathf.Clamp01(mechanismVolume);
            edgeFadeDuration = Mathf.Max(0.005f, edgeFadeDuration);
        }
    }
}

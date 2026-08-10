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
        [Header("References")]
        [SerializeField] private SlidingDoor door;
        [SerializeField] private Rigidbody doorBody;
        [SerializeField] private AudioSource mechanismSource;
        [SerializeField] private AudioClip movementLoopClip;
        [SerializeField] private AudioClip endpointClip;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float mechanismVolume = 0.2f;
        [SerializeField, Min(0.005f)] private float edgeFadeDuration = 0.02f;

        private bool motorActive;
        private bool fadingOut;
        private Vector3 previousBodyPosition;

        public bool IsMotorActive => motorActive;

        public int MotorStartCount { get; private set; }

        public int EndpointCueCount { get; private set; }

        private void Awake()
        {
            if (door == null
                || doorBody == null
                || mechanismSource == null
                || movementLoopClip == null
                || endpointClip == null)
            {
                Debug.LogError(
                    "SlidingDoorAudio requires explicit door, body, source, movement, and endpoint references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
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
            mechanismSource.clip = movementLoopClip;
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

        private void StartMotor()
        {
            fadingOut = false;
            motorActive = true;
            mechanismSource.Stop();
            mechanismSource.clip = movementLoopClip;
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

        private void OnValidate()
        {
            mechanismVolume = Mathf.Clamp01(mechanismVolume);
            edgeFadeDuration = Mathf.Max(0.005f, edgeFadeDuration);
        }
    }
}

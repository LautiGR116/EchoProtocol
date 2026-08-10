using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class PressurePlateAudio : MonoBehaviour, ILoopResettable
    {
        [Header("References")]
        [SerializeField] private PressurePlate plate;
        [SerializeField] private AudioSource plateSource;
        [SerializeField] private AudioClip pressClip;
        [SerializeField] private AudioClip releaseClip;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float cueVolume = 0.14f;

        public int PressCount { get; private set; }

        public int ReleaseCount { get; private set; }

        private void Awake()
        {
            if (plate == null
                || plateSource == null
                || pressClip == null
                || releaseClip == null)
            {
                Debug.LogError(
                    "PressurePlateAudio requires plate, source, press, and release references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            ResetLoopState();
        }

        private void OnEnable()
        {
            if (plate != null)
            {
                plate.PressedChanged += HandlePressedChanged;
            }
        }

        private void OnDisable()
        {
            if (plate != null)
            {
                plate.PressedChanged -= HandlePressedChanged;
            }

            if (plateSource != null)
            {
                plateSource.Stop();
            }
        }

        public void ResetLoopState()
        {
            if (plateSource != null)
            {
                plateSource.Stop();
            }
        }

        private void HandlePressedChanged(bool isPressed)
        {
            plateSource.Stop();
            plateSource.clip = isPressed ? pressClip : releaseClip;
            plateSource.volume = cueVolume;
            plateSource.Play();

            if (isPressed)
            {
                PressCount++;
            }
            else
            {
                ReleaseCount++;
            }
        }

        private void ConfigureSource()
        {
            plateSource.playOnAwake = false;
            plateSource.loop = false;
            plateSource.spatialBlend = 1f;
            plateSource.dopplerLevel = 0f;
            plateSource.panStereo = 0f;
            plateSource.priority = 170;
            plateSource.rolloffMode = AudioRolloffMode.Linear;
            plateSource.minDistance = 1.25f;
            plateSource.maxDistance = 8f;
            plateSource.volume = cueVolume;
        }

        private void OnValidate()
        {
            cueVolume = Mathf.Clamp01(cueVolume);
        }
    }
}

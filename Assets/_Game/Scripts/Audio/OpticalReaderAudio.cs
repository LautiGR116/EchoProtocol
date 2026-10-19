using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class OpticalReaderAudio : MonoBehaviour, ILoopResettable
    {
        [Header("References")]
        [SerializeField] private OpticalReader reader;
        [SerializeField] private AudioSource readerSource;
        [SerializeField] private AudioClip alignedClip;
        [SerializeField] private AudioClip releasedClip;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float cueVolume = 0.16f;

        public int AlignedCueCount { get; private set; }

        public int ReleasedCueCount { get; private set; }

        private void Awake()
        {
            if (reader == null
                || readerSource == null
                || alignedClip == null
                || releasedClip == null)
            {
                Debug.LogError(
                    "OpticalReaderAudio requires reader, source, aligned, and released references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
            ResetLoopState();
        }

        private void OnEnable()
        {
            if (reader != null)
            {
                reader.AlignmentChanged += HandleAlignmentChanged;
            }
        }

        private void OnDisable()
        {
            if (reader != null)
            {
                reader.AlignmentChanged -= HandleAlignmentChanged;
            }

            if (readerSource != null)
            {
                readerSource.Stop();
            }
        }

        public void ResetLoopState()
        {
            if (readerSource != null)
            {
                readerSource.Stop();
            }
        }

        private void HandleAlignmentChanged(bool isAligned)
        {
            readerSource.Stop();
            readerSource.clip = isAligned ? alignedClip : releasedClip;
            readerSource.volume = cueVolume;
            readerSource.Play();

            if (isAligned)
            {
                AlignedCueCount++;
            }
            else
            {
                ReleasedCueCount++;
            }
        }

        private void ConfigureSource()
        {
            readerSource.playOnAwake = false;
            readerSource.loop = false;
            readerSource.spatialBlend = 1f;
            readerSource.dopplerLevel = 0f;
            readerSource.panStereo = 0f;
            readerSource.priority = 165;
            readerSource.rolloffMode = AudioRolloffMode.Linear;
            readerSource.minDistance = 1.25f;
            readerSource.maxDistance = 8f;
            readerSource.volume = cueVolume;
        }

        private void OnValidate()
        {
            cueVolume = Mathf.Clamp01(cueVolume);
        }
    }
}

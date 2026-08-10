using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class RecallCompletionAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LoopController loopController;
        [SerializeField] private AudioSource recallSource;
        [SerializeField] private AudioClip completionClip;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float cueVolume = 0.12f;

        public int RecallPlayCount { get; private set; }

        private void Awake()
        {
            if (loopController == null
                || recallSource == null
                || completionClip == null)
            {
                Debug.LogError(
                    "RecallCompletionAudio requires loop, source, and clip references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureSource();
        }

        private void OnEnable()
        {
            if (loopController != null)
            {
                loopController.LoopCompleted += HandleLoopCompleted;
            }
        }

        private void OnDisable()
        {
            if (loopController != null)
            {
                loopController.LoopCompleted -= HandleLoopCompleted;
            }

            if (recallSource != null)
            {
                recallSource.Stop();
            }
        }

        private void HandleLoopCompleted()
        {
            recallSource.Stop();
            recallSource.clip = completionClip;
            recallSource.volume = cueVolume;
            recallSource.Play();
            RecallPlayCount++;
        }

        private void ConfigureSource()
        {
            recallSource.playOnAwake = false;
            recallSource.loop = false;
            recallSource.spatialBlend = 0f;
            recallSource.dopplerLevel = 0f;
            recallSource.panStereo = 0f;
            recallSource.priority = 170;
            recallSource.volume = cueVolume;
        }

        private void OnValidate()
        {
            cueVolume = Mathf.Clamp01(cueVolume);
        }
    }
}

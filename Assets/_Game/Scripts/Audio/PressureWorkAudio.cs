using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Present-tense machinery follows real work. It is not a recorded Echo channel.
    [DisallowMultipleComponent]
    public sealed class PressureWorkAudio : MonoBehaviour
    {
        [Header("Explicit references")]
        [SerializeField] private CampaignDirector director;
        [SerializeField] private GameSession session;
        [SerializeField] private AudioSource flowSource;
        [SerializeField] private AudioSource dischargeSource;
        [SerializeField] private AudioClip flowLoopClip;
        [SerializeField] private AudioClip dischargeClip;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float flowVolume = 0.14f;
        [SerializeField, Range(0f, 1f)] private float dischargeVolume = 0.20f;
        [SerializeField, Min(0.005f)] private float fadeDuration = 0.15f;

        private bool ready;
        private bool wasWorking;
        private float flowEnvelope;
        private int observedValves;
        private Vector3 lastWorkPosition;

        public bool IsFlowActive => flowSource && flowSource.isPlaying && flowSource.volume > 0f;
        public int DischargeCount { get; private set; }

        private void Awake()
        {
            if (!director || !session || !flowSource || !dischargeSource
                || flowSource == dischargeSource || !flowLoopClip || !dischargeClip)
            {
                Debug.LogError("PressureWorkAudio requires an explicit director, session, two distinct sources and both clips.", this);
                enabled = false;
                return;
            }

            // Routing, attenuation, spatial blend and distance remain authored on
            // the dedicated sources. This component only owns their playback.
            flowSource.playOnAwake = false;
            flowSource.loop = true;
            flowSource.clip = flowLoopClip;
            dischargeSource.playOnAwake = false;
            dischargeSource.loop = false;
            dischargeSource.clip = dischargeClip;
            ready = true;
        }

        private void OnEnable()
        {
            if (!ready) return;
            EstablishBaseline();
        }

        private void Start()
        {
            // Start-time changes from a checkpoint are a baseline, not a release.
            if (ready) EstablishBaseline();
        }

        private void EstablishBaseline()
        {
            observedValves = director.ValvesDone;
            wasWorking = false;
            StopSources();
        }

        private void LateUpdate()
        {
            // LateUpdate observes CampaignDirector after it advances or cancels
            // work. Completion clears WorkingStation, so retain its last position.
            int valves = director.ValvesDone;
            bool pressureChapter = director.Chapter == CampaignChapter.Pressure;

            if (session.IsPaused)
            {
                // Ordinary pause uses GameSession's AudioListener.pause. Do not
                // restart loops or discard a paused discharge on resume.
                if (valves != observedValves || !pressureChapter)
                {
                    observedValves = valves;
                    wasWorking = false;
                    StopSources();
                }
                return;
            }

            bool completedWork = session.HasStarted && pressureChapter && wasWorking
                && valves == observedValves + 1;
            observedValves = valves;
            if (completedWork)
            {
                dischargeSource.transform.position = lastWorkPosition;
                dischargeSource.volume = dischargeVolume;
                dischargeSource.Play();
                DischargeCount++;
            }

            bool working = session.HasStarted && pressureChapter && director.IsWorking
                && director.Pressure > 0.8f && director.WorkingStation;
            if (working)
            {
                lastWorkPosition = director.WorkingStation.transform.position;
                flowSource.transform.position = lastWorkPosition;
                if (!flowSource.isPlaying)
                {
                    flowSource.volume = 0f;
                    flowSource.Play();
                }
            }

            flowEnvelope = Mathf.MoveTowards(flowEnvelope, working ? 1f : 0f, Time.deltaTime / fadeDuration);
            flowSource.volume = flowVolume * flowEnvelope;
            if (!working && flowEnvelope <= 0f && flowSource.isPlaying)
                flowSource.Stop();

            wasWorking = working;
        }

        private void OnDisable()
        {
            wasWorking = false;
            StopSources();
        }

        private void StopSources()
        {
            flowEnvelope = 0f;
            if (flowSource)
            {
                flowSource.Stop();
                flowSource.volume = 0f;
            }
            if (dischargeSource) dischargeSource.Stop();
        }

        private void OnValidate()
        {
            flowVolume = Mathf.Clamp01(flowVolume);
            dischargeVolume = Mathf.Clamp01(dischargeVolume);
            fadeDuration = Mathf.Max(0.005f, fadeDuration);
        }
    }
}

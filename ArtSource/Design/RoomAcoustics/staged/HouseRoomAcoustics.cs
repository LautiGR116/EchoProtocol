using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    /// <summary>
    /// Optional spectral boundaries for two present-tense domestic sources.
    /// Owns only the two explicit low-pass cutoffs. Playback, gain, position,
    /// distance attenuation, circuit state and recorded data retain their owners.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HouseRoomAcoustics : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener;
        [SerializeField] private AudioSource telephoneSource;
        [SerializeField] private AudioLowPassFilter telephoneFilter;
        [SerializeField] private AudioSource refrigeratorSource;
        [SerializeField] private AudioLowPassFilter refrigeratorFilter;
        [Tooltip("Only real domestic walls, closed glazing, floors, ceilings and the exit leaf. Never furniture, apparatus, Player, Echo or triggers.")]
        [SerializeField] private Collider[] architecturalPartitions = new Collider[0];
        [SerializeField, Range(10f, 22000f)] private float clearCutoffHz = 22000f;
        [SerializeField, Range(10f, 22000f)] private float obstructedCutoffHz = 1600f;
        [Tooltip("Seconds for the full clear/obstructed transition in logarithmic frequency.")]
        [SerializeField, Min(.01f)] private float transitionSeconds = .20f;

        private bool ready;
        private float telephoneLogHz, refrigeratorLogHz;

        public bool TelephoneObstructed { get; private set; }
        public bool RefrigeratorObstructed { get; private set; }
        public float TelephoneCutoffHz => telephoneFilter ? telephoneFilter.cutoffFrequency : 0f;
        public float RefrigeratorCutoffHz => refrigeratorFilter ? refrigeratorFilter.cutoffFrequency : 0f;

        private void Awake()
        {
            ready = session && listener && telephoneSource && refrigeratorSource
                && telephoneSource != refrigeratorSource
                && ValidFilter(telephoneSource, telephoneFilter)
                && ValidFilter(refrigeratorSource, refrigeratorFilter)
                && architecturalPartitions != null && architecturalPartitions.Length > 0;
            if (!ready)
            {
                Debug.LogError("HouseRoomAcoustics requires two distinct physical sources, a matching enabled filter on each source, a listener, session and explicit architectural partitions.", this);
                enabled = false;
                return;
            }
            SetInitialCutoffs();
        }

        private static bool ValidFilter(AudioSource source, AudioLowPassFilter filter)
        {
            return source && filter && filter.gameObject == source.gameObject
                && filter.enabled && !source.bypassEffects && source.spatialBlend >= .999f;
        }

        private void OnEnable() { if (ready) SetInitialCutoffs(); }

        private void SetInitialCutoffs()
        {
            ReadObstruction();
            telephoneLogHz = Mathf.Log(TelephoneObstructed ? obstructedCutoffHz : clearCutoffHz);
            refrigeratorLogHz = Mathf.Log(RefrigeratorObstructed ? obstructedCutoffHz : clearCutoffHz);
            WriteCutoffs();
        }

        private void LateUpdate()
        {
            if (!ready || session.IsPaused || Time.timeScale <= 0f) return;
            ReadObstruction();
            float clearLog = Mathf.Log(clearCutoffHz), blockedLog = Mathf.Log(obstructedCutoffHz);
            float step = Mathf.Abs(clearLog - blockedLog) * Time.deltaTime / transitionSeconds;
            if (Mathf.Approximately(clearLog, blockedLog))
            {
                telephoneLogHz = refrigeratorLogHz = clearLog;
                WriteCutoffs();
                return;
            }
            telephoneLogHz = Mathf.MoveTowards(telephoneLogHz, TelephoneObstructed ? blockedLog : clearLog, step);
            refrigeratorLogHz = Mathf.MoveTowards(refrigeratorLogHz, RefrigeratorObstructed ? blockedLog : clearLog, step);
            WriteCutoffs();
        }

        private void ReadObstruction()
        {
            TelephoneObstructed = Obstructed(telephoneSource.transform.position);
            RefrigeratorObstructed = Obstructed(refrigeratorSource.transform.position);
        }

        private bool Obstructed(Vector3 sourcePosition)
        {
            Vector3 offset = sourcePosition - listener.position;
            float distance = offset.magnitude;
            if (distance <= .03f) return false;
            Vector3 direction = offset / distance;
            var ray = new Ray(listener.position + direction * .015f, direction);
            // Query only the author-provided physical architecture. Collider.Raycast
            // cannot be intercepted by a hand, receiver, cupboard or Echo collider.
            foreach (var partition in architecturalPartitions)
                if (partition && partition.enabled && !partition.isTrigger
                    && partition.gameObject.activeInHierarchy
                    && partition.Raycast(ray, out _, distance - .03f)) return true;
            return false;
        }

        private void WriteCutoffs()
        {
            telephoneFilter.cutoffFrequency = Mathf.Clamp(Mathf.Exp(telephoneLogHz), obstructedCutoffHz, clearCutoffHz);
            refrigeratorFilter.cutoffFrequency = Mathf.Clamp(Mathf.Exp(refrigeratorLogHz), obstructedCutoffHz, clearCutoffHz);
        }

        private void OnDisable()
        {
            // Removing this optional treatment returns its sources to their clear
            // spectrum. It never stops, starts, mutes or rearms a source.
            if (!ready) return;
            if (telephoneFilter) telephoneFilter.cutoffFrequency = clearCutoffHz;
            if (refrigeratorFilter) refrigeratorFilter.cutoffFrequency = clearCutoffHz;
        }

        private void OnValidate()
        {
            clearCutoffHz = Mathf.Clamp(clearCutoffHz, 10f, 22000f);
            obstructedCutoffHz = Mathf.Clamp(obstructedCutoffHz, 10f, clearCutoffHz);
            transitionSeconds = Mathf.Max(.01f, transitionSeconds);
        }
    }
}

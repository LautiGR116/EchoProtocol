using System;
using UnityEngine;

namespace EchoProtocol.Horror.SE1Pressure
{
    // Captive observation shutters, not a door across the Player's path.
    // SE1 alone advances these stages, after the original Echo has finished.
    // Not a loop-reset target: RECALL cannot replay the material strike.
    [DisallowMultipleComponent]
    public sealed class SE1PressureSequence : MonoBehaviour
    {
        [Serializable]
        private sealed class Bay
        {
            public Transform[] blades;
            public AudioSource contact;
            public float contactLeadSeconds;
            [NonSerialized] public Vector3[] open;
            [NonSerialized] public bool sounded;

            public bool Configured
            {
                get
                {
                    if (blades == null || blades.Length != 14 || !contact || !contact.clip
                        || !contact.outputAudioMixerGroup || contact.outputAudioMixerGroup.name != "WorldSFX"
                        || contact.playOnAwake || contact.loop || contact.ignoreListenerPause
                        || contact.ignoreListenerVolume || contact.bypassListenerEffects
                        || contact.spatialBlend != 1f || contact.dopplerLevel != 0f) return false;
                    foreach (var blade in blades) if (!blade) return false;
                    return true;
                }
            }

            public void Cache()
            {
                if (open != null) return;
                open = new Vector3[blades.Length];
                for (int i = 0; i < blades.Length; i++) open[i] = blades[i].localPosition;
            }

            public void Pose(float progress)
            {
                Cache();
                // Each linked slat unfolds from the header cassette. All x/z
                // travel stays inside the original solid glazing, even at stop.
                for (int i = 0; i < blades.Length; i++)
                    blades[i].localPosition = open[i] + Vector3.down * (i * .192f * progress);
            }
        }

        [SerializeField] private VS1StrongEventOne owner;
        [SerializeField] private VS1DomesticThresholdCheckpoint checkpoint;
        [SerializeField] private Bay[] bays;
        [SerializeField] private Light[] reserveLights;
        [SerializeField] private float[] reserveIntensities;
        private float elapsed, reserve;
        private bool restored;

        public int StageIndex { get; private set; } = -1;
        public int ContactCount { get; private set; }
        public bool StageAtStop => StageIndex >= 0 && elapsed >= StrokeSeconds;
        public float HoldSeconds => StageIndex == 0 ? .62f : StageIndex == 1 ? .36f : .45f;
        private float StrokeSeconds => StageIndex == 0 ? .55f : StageIndex == 1 ? .46f : .22f;

        public bool IsConfigured
        {
            get
            {
                if (!owner || !checkpoint || bays == null || bays.Length != 4
                    || reserveLights == null || reserveLights.Length != 4
                    || reserveIntensities == null || reserveIntensities.Length != reserveLights.Length) return false;
                foreach (var bay in bays) if (bay == null || !bay.Configured) return false;
                foreach (var lamp in reserveLights) if (!lamp) return false;
                return true;
            }
        }

        private void Awake()
        {
            if (!IsConfigured)
            {
                Debug.LogError("SE1 pressure requires four captive bays, original SE1/checkpoint and four reserve lights.", this);
                enabled = false;
                return;
            }
            foreach (var bay in bays) bay.Cache();
            SetReserve(0f);
        }

        public void BeginStage(int index)
        {
            if (!enabled || restored || index != StageIndex + 1 || index < 0 || index > 2) return;
            StageIndex = index;
            elapsed = 0f;
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            bool guiding = StageIndex >= 0 && !restored && !checkpoint.IsCheckpointCommitted
                && !owner.AmbienceControlReleased;
            reserve = Mathf.MoveTowards(reserve, guiding ? 1f : 0f, Time.deltaTime / .4f);
            SetReserve(reserve);
            if (StageIndex < 0 || restored || StageAtStop) return;

            elapsed = Mathf.Min(elapsed + Time.deltaTime, StrokeSeconds);
            float travel = Mathf.Clamp01(elapsed / StrokeSeconds);
            float weighted = travel * travel;
            int first = StageIndex;
            int last = StageIndex == 2 ? 3 : first;
            for (int i = first; i <= last; i++)
            {
                var bay = bays[i];
                bay.Pose(weighted);
                if (!bay.sounded && elapsed >= StrokeSeconds - bay.contactLeadSeconds)
                {
                    bay.sounded = true;
                    bay.contact.Play();
                    ContactCount++;
                }
            }
        }

        // Later-chapter restore settles the environment silently. No catch-up
        // sounds, animated doors, extra wait, or unrecorded Echo performance.
        public void RestoreConsequence()
        {
            if (!IsConfigured) return;
            restored = true;
            StageIndex = 2;
            elapsed = StrokeSeconds;
            foreach (var bay in bays)
            {
                bay.contact.Stop();
                bay.sounded = true;
                bay.Pose(1f);
            }
            reserve = 0f;
            SetReserve(0f);
        }

        private void SetReserve(float level)
        {
            for (int i = 0; i < reserveLights.Length; i++)
                reserveLights[i].intensity = reserveIntensities[i] * level;
        }

        private void OnDisable()
        {
            if (bays == null) return;
            foreach (var bay in bays) if (bay != null && bay.contact) bay.contact.Stop();
            // Deliberately retain stage and sounded flags; disable/RECALL is
            // never permission to rearm an already committed physical event.
        }
    }
}

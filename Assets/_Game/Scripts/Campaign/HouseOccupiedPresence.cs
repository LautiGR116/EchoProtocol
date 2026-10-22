using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum HouseOccupiedPhase { Dormant, Recovery, Listening, ReturnArmed, Pressure, Residue, Retired }

    /// <summary>
    /// Three authored interventions behind the study partition. This component owns
    /// only its new panel and three new sound sources; it never owns house progression,
    /// the telephone, room power, the exit leaf, the Player camera or an Echo channel.
    /// </summary>
    [DefaultExecutionOrder(400)]
    [DisallowMultipleComponent]
    public sealed class HouseOccupiedPresence : MonoBehaviour
    {
        [Header("Explicit domestic context")]
        [SerializeField] private HouseMemory house;
        [SerializeField] private GameSession session;
        [SerializeField] private Transform player;
        [SerializeField] private BoxCollider advanceVolume;
        [SerializeField] private BoxCollider keyApproachVolume;
        [SerializeField] private BoxCollider returnVolume;

        [Header("Exclusive visual leaf; the existing sealed partition retains collision")]
        [SerializeField] private Transform pressurePanel;
        [SerializeField] private Vector3 pressureOffset = new Vector3(-.06f, 0f, 0f);
        [SerializeField] private Vector3 pressureEuler = new Vector3(0f, 11f, 0f);
        [SerializeField, Range(0f, .5f)] private float residualFraction = .18f;

        [Header("Exclusive physical sources, routed through existing effects output")]
        [SerializeField] private AudioSource distantSource;
        [SerializeField] private AudioSource contactSource;
        [SerializeField] private AudioSource retreatSource;
        [SerializeField] private AudioClip weightClip;
        [SerializeField] private AudioClip frictionClip;
        [SerializeField] private AudioClip contactClip;
        [Tooltip("Read-only reference to the existing receiver performance. Early activity waits for its quiet; nothing stops or changes the call.")]
        [SerializeField] private AudioSource telephonePerformance;

        [Header("Authored timing in active, unpaused house seconds")]
        [SerializeField, Min(3f)] private float recoverySeconds = 5f;
        [SerializeField, Min(1f)] private float cueGap = 4f;
        [SerializeField, Min(.1f)] private float preparationSeconds = .7f;
        [SerializeField, Min(.05f)] private float attackSeconds = .13f;
        [SerializeField, Min(.1f)] private float holdSeconds = .65f;
        [SerializeField, Min(.1f)] private float releaseSeconds = 1.8f;

        private bool ready, entered, actionObserved, previousAnswered, retreatPlayed;
        private HousePowerLocation previousPower;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private float quietUntil, pressureTime, frictionAt;

        public HouseOccupiedPhase Phase { get; private set; } = HouseOccupiedPhase.Dormant;
        public bool WeightPlayed { get; private set; }
        public bool FrictionPlayed { get; private set; }
        public bool ContactPlayed { get; private set; }
        public bool ContactCancelled { get; private set; }
        public float SequenceTime { get; private set; }
        public float PressureTime => pressureTime;
        public int CueCount { get; private set; }

        // This is deliberately independent of Awake ordering. HouseMemory can safely
        // select one presence owner before either component's Awake has run.
        public bool IsConfigured => house && session && player && pressurePanel && pressurePanel != transform && pressurePanel.IsChildOf(transform)
            && ValidVolume(advanceVolume) && ValidVolume(keyApproachVolume) && ValidVolume(returnVolume)
            && advanceVolume != keyApproachVolume && advanceVolume != returnVolume && keyApproachVolume != returnVolume
            && OwnedSource(distantSource) && OwnedSource(contactSource) && OwnedSource(retreatSource)
            && distantSource != contactSource && distantSource != retreatSource && contactSource != retreatSource
            && weightClip && frictionClip && contactClip;

        private bool ValidVolume(BoxCollider volume) => volume && volume.isTrigger && volume.enabled
            && volume.transform.IsChildOf(transform) && volume.gameObject.activeInHierarchy;
        private bool OwnedSource(AudioSource source) => source && source.transform.IsChildOf(transform)
            && source.outputAudioMixerGroup && source.spatialBlend >= .999f
            && !source.ignoreListenerPause && !source.ignoreListenerVolume && !source.bypassListenerEffects;

        private void Awake()
        {
            ClampTimings();
            ready = IsConfigured;
            if (!ready)
            {
                Debug.LogError("HouseOccupiedPresence requires explicit domestic context, three trigger volumes, its own panel, three distinct 3D sources on mixer output that respect listener settings, and three clips.", this);
                enabled = false;
                return;
            }
            restPosition = pressurePanel.localPosition;
            restRotation = pressurePanel.localRotation;
            PrepareSource(distantSource);
            PrepareSource(contactSource);
            PrepareSource(retreatSource);
        }

        private static void PrepareSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.ignoreListenerPause = false;
            source.ignoreListenerVolume = false;
            source.bypassListenerEffects = false;
            source.dopplerLevel = 0f;
            source.Stop();
        }

        private void LateUpdate()
        {
            if (!ready || Phase == HouseOccupiedPhase.Retired) return;
            if (!house.isActiveAndEnabled || !house.Active)
            {
                if (entered || house.HasExited) Retire();
                return;
            }
            if (house.HasExited && !ContactPlayed)
            {
                ContactCancelled = true;
                Retire();
                return;
            }
            if (session.IsPaused || !session.HasStarted || Time.deltaTime <= 0f) return;
            if (!entered)
            {
                entered = true;
                previousAnswered = house.HasAnswered;
                previousPower = house.PowerLocation;
                // First-frame interactions can precede this LateUpdate. Their real
                // consequence is sufficient; a missed bool edge must not disarm it.
                actionObserved = house.HasAnswered || house.PowerLocation != HousePowerLocation.Hall;
                if (house.HasAnswered) quietUntil = SequenceTime + 1.5f;
                Phase = HouseOccupiedPhase.Recovery;
            }
            SequenceTime += Time.deltaTime;
            if (house.HasAnswered != previousAnswered || house.PowerLocation != previousPower)
            {
                actionObserved = true;
                // Leave the pickup, electrical consequence and scheduled phone onset
                // their own space. The existing call starts .8 s after first answer.
                quietUntil = Mathf.Max(quietUntil, SequenceTime + 1.5f);
            }
            previousAnswered = house.HasAnswered;
            previousPower = house.PowerLocation;
            actionObserved |= Contains(advanceVolume) || Contains(keyApproachVolume) || house.HasKey;

            if (Phase == HouseOccupiedPhase.Pressure || Phase == HouseOccupiedPhase.Residue)
            {
                UpdatePressure();
                return;
            }
            if (SequenceTime < recoverySeconds) return;
            Phase = house.HasKey ? HouseOccupiedPhase.ReturnArmed : HouseOccupiedPhase.Listening;

            // The key return has priority. Fast travel does not queue every unfinished
            // cue into a burst, and looking at (or ignoring) the leaf changes no rule.
            if (house.HasKey && Contains(returnVolume) && !Contains(keyApproachVolume))
            {
                BeginPressure();
                return;
            }
            // Visual load continues throughout the deliberately quiet sound gap.
            // Keeping it above the cue guard avoids jumping after the silence.
            if (FrictionPlayed)
                Pose(.045f * Mathf.SmoothStep(0f, 1f, (SequenceTime - frictionAt) / 1.3f));
            if ((telephonePerformance && telephonePerformance.isPlaying) || SequenceTime < quietUntil) return;
            if (!WeightPlayed && actionObserved)
            {
                WeightPlayed = true;
                Play(distantSource, weightClip);
                quietUntil = SequenceTime + weightClip.length + cueGap;
            }
            else if (WeightPlayed && !FrictionPlayed && Contains(keyApproachVolume))
            {
                FrictionPlayed = true;
                frictionAt = SequenceTime;
                Play(contactSource, frictionClip);
                quietUntil = SequenceTime + frictionClip.length + cueGap;
            }
        }

        private bool Contains(BoxCollider volume) => volume.gameObject.activeInHierarchy && volume.enabled
            && new Bounds(volume.center, volume.size).Contains(volume.transform.InverseTransformPoint(player.position));

        private void BeginPressure()
        {
            Phase = HouseOccupiedPhase.Pressure;
            pressureTime = 0f;
            // Only these owned cues are stopped. Telephone, fridge and environmental
            // sounds keep their original owners and power consequences.
            distantSource.Stop();
            contactSource.Stop();
            retreatSource.Stop();
        }

        private void UpdatePressure()
        {
            if (!ContactPlayed && !Contains(returnVolume))
            {
                // A fast crossing, retreat, or a checkpoint placement during pause
                // cannot leave a delayed contact waiting behind the Player.
                ContactCancelled = true;
                Retire();
                return;
            }
            if (retreatPlayed) return;
            // Use the same scaled time as Player movement: slowing this timeline
            // independently would move the contact behind a Player at low frame rates.
            // Session pause contributes exactly zero; no wall-clock deadline exists.
            pressureTime += Time.deltaTime;
            if (pressureTime < preparationSeconds)
            {
                Pose(Mathf.Lerp(.045f, .11f, Mathf.SmoothStep(0f, 1f, pressureTime / preparationSeconds)));
                return;
            }
            if (!ContactPlayed)
            {
                ContactPlayed = true;
                Play(contactSource, contactClip);
            }
            float afterPreparation = pressureTime - preparationSeconds;
            if (afterPreparation < attackSeconds)
            {
                // Fast loaded movement then a restrained recoil, not a camera shake.
                Pose(Mathf.Lerp(.11f, 1f, afterPreparation / attackSeconds));
                return;
            }
            float afterAttack = afterPreparation - attackSeconds;
            if (afterAttack < holdSeconds)
            {
                Pose(1f - .07f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(afterAttack / holdSeconds)));
                return;
            }
            Phase = HouseOccupiedPhase.Residue;
            float release = Mathf.Clamp01((afterAttack - holdSeconds) / releaseSeconds);
            Pose(Mathf.Lerp(1f, residualFraction, Mathf.SmoothStep(0f, 1f, release)));
            if (release >= 1f && !retreatPlayed)
            {
                retreatPlayed = true;
                if (!house.HasExited) Play(retreatSource, frictionClip);
            }
        }

        private void Pose(float amount)
        {
            pressurePanel.localPosition = restPosition + pressureOffset * amount;
            pressurePanel.localRotation = restRotation * Quaternion.Euler(pressureEuler * amount);
        }

        private void Play(AudioSource source, AudioClip clip)
        {
            if (!source.isActiveAndEnabled) return;
            source.PlayOneShot(clip);
            CueCount++;
        }

        /// <summary>HouseMemory calls this for chapter departure and final-review restores.
        /// No saved checkpoint can restart a partially consumed performance.</summary>
        public void Retire()
        {
            if (ready)
            {
                StopOwnedAudio();
                Pose(ContactPlayed ? residualFraction : 0f);
            }
            Phase = HouseOccupiedPhase.Retired;
        }

        private void StopOwnedAudio()
        {
            if (distantSource) distantSource.Stop();
            if (contactSource) contactSource.Stop();
            if (retreatSource) retreatSource.Stop();
        }

        private void ClampTimings()
        {
            recoverySeconds = Mathf.Max(3f, recoverySeconds);
            cueGap = Mathf.Max(1f, cueGap);
            preparationSeconds = Mathf.Max(.1f, preparationSeconds);
            attackSeconds = Mathf.Max(.05f, attackSeconds);
            holdSeconds = Mathf.Max(.1f, holdSeconds);
            releaseSeconds = Mathf.Max(.1f, releaseSeconds);
            residualFraction = Mathf.Clamp(residualFraction, 0f, .5f);
        }

        private void OnValidate() => ClampTimings();

        private void OnDisable()
        {
            if (ready)
            {
                StopOwnedAudio();
                Pose(0f);
            }
            if (entered) Phase = HouseOccupiedPhase.Retired;
        }
    }
}

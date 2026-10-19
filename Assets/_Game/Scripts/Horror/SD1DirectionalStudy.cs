using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    public enum SD1StudyPhase
    {
        AwaitingFirstSuccess,
        StableBaseline,
        KnockDelivered,
        FootstepsDelivered,
        BreathingDelivered,
        AwaitingWhisper,
        Recovery
    }

    [DisallowMultipleComponent]
    public sealed class SD1DirectionalStudy : MonoBehaviour
    {
        [Header("Trust References")]
        [SerializeField] private LoopController loopController;
        [SerializeField] private PrototypePuzzleGoal puzzleGoal;
        [SerializeField] private Transform player;

        [Header("Authored Triggers")]
        [SerializeField] private Transform knockTrigger;
        [SerializeField] private Transform footstepsTrigger;
        [SerializeField] private Transform breathingTrigger;
        [SerializeField] private Transform whisperTrigger;
        [SerializeField, Min(0.5f)] private float triggerRadius = 2.75f;
        [SerializeField, Range(-1f, 1f)] private float minimumForwardAlignment = 0.45f;
        [SerializeField, Min(0f)] private float postRecallGrace = 2.5f;

        [Header("World-Locked Cues")]
        [SerializeField] private AudioSource knockSource;
        [SerializeField] private AudioSource footstepsSource;
        [SerializeField] private AudioSource breathingSource;
        [SerializeField] private Transform footstepsStart;
        [SerializeField] private Transform footstepsEnd;

        [Header("Head-Locked Whisper")]
        [SerializeField] private AudioSource whisperSource;
        [SerializeField] private SD1SubtitleDisplay subtitleDisplay;
        [SerializeField] private string spanishSubtitle = "¿Me escuchás?";

        [Header("Delayed Consequence")]
        [SerializeField] private Light consequenceLight;
        [SerializeField, Min(0f)] private float consequenceDelay = 2f;
        [SerializeField, Min(0.1f)] private float consequenceFadeDuration = 1.5f;
        [SerializeField, Range(0f, 1f)] private float consequenceIntensityMultiplier = 0.38f;

        private int startCompletedLoopCount;
        private float lastLoopCompletedTime = float.NegativeInfinity;
        private float footstepsElapsed;
        private float breathingEndedTime = float.PositiveInfinity;
        private float consequenceStartIntensity;
        private bool footstepsAreMoving;
        private bool consequenceIsFading;

        public SD1StudyPhase Phase { get; private set; } = SD1StudyPhase.AwaitingFirstSuccess;

        public int KnockPlayCount { get; private set; }

        public int FootstepsPlayCount { get; private set; }

        public int BreathingPlayCount { get; private set; }

        public int WhisperPlayCount { get; private set; }

        public bool VisualConsequenceApplied { get; private set; }

        public bool HasWhisperClip => whisperSource != null && whisperSource.clip != null;

        public bool HasBreathingClip => breathingSource != null
            && breathingSource.clip != null;

        public int CurrentStudyLoop => Phase == SD1StudyPhase.AwaitingFirstSuccess
            ? 0
            : Mathf.Max(0, loopController.CompletedLoopCount - startCompletedLoopCount);

        public float SecondsSinceLastLoop => Time.unscaledTime - lastLoopCompletedTime;

        private void Awake()
        {
            if (loopController == null
                || puzzleGoal == null
                || player == null
                || knockTrigger == null
                || footstepsTrigger == null
                || breathingTrigger == null
                || whisperTrigger == null
                || knockSource == null
                || footstepsSource == null
                || breathingSource == null
                || footstepsStart == null
                || footstepsEnd == null
                || consequenceLight == null)
            {
                Debug.LogError(
                    "SD1DirectionalStudy requires explicit trust, trigger, cue, path, and consequence references.",
                    this);
                enabled = false;
                return;
            }

            consequenceStartIntensity = consequenceLight.intensity;
            ConfigureWorldSource(knockSource);
            ConfigureWorldSource(footstepsSource);
            ConfigureWorldSource(breathingSource);
            ConfigureWhisperSource();
        }

        private void OnEnable()
        {
            if (puzzleGoal != null)
            {
                puzzleGoal.Completed += HandleFirstPuzzleSuccess;
            }

            if (loopController != null)
            {
                loopController.LoopCompleted += HandleLoopCompleted;
            }
        }

        private void OnDisable()
        {
            if (puzzleGoal != null)
            {
                puzzleGoal.Completed -= HandleFirstPuzzleSuccess;
            }

            if (loopController != null)
            {
                loopController.LoopCompleted -= HandleLoopCompleted;
            }

            StopOwnedSources();
        }

        private void Update()
        {
            UpdateMovingFootsteps();
            UpdateVisualConsequence();

            if (!enabled
                || Phase == SD1StudyPhase.AwaitingFirstSuccess
                || Phase == SD1StudyPhase.Recovery
                || Time.unscaledTime - lastLoopCompletedTime < postRecallGrace)
            {
                return;
            }

            int studyLoop = CurrentStudyLoop;
            switch (Phase)
            {
                case SD1StudyPhase.StableBaseline:
                    if (studyLoop >= 1 && IsAtAuthoredTrigger(knockTrigger, false))
                    {
                        DeliverKnock();
                    }
                    break;

                case SD1StudyPhase.KnockDelivered:
                    if (studyLoop >= 2 && IsAtAuthoredTrigger(footstepsTrigger, true))
                    {
                        DeliverFootsteps();
                    }
                    break;

                case SD1StudyPhase.FootstepsDelivered:
                    if (studyLoop >= 3
                        && HasBreathingClip
                        && IsAtAuthoredTrigger(breathingTrigger, false))
                    {
                        DeliverBreathing();
                    }
                    break;

                case SD1StudyPhase.AwaitingWhisper:
                    if (studyLoop >= 4
                        && HasWhisperClip
                        && IsAtAuthoredTrigger(whisperTrigger, true))
                    {
                        DeliverWhisper();
                    }
                    break;
            }
        }

        private void HandleFirstPuzzleSuccess()
        {
            if (Phase != SD1StudyPhase.AwaitingFirstSuccess)
            {
                return;
            }

            startCompletedLoopCount = loopController.CompletedLoopCount;
            Phase = SD1StudyPhase.StableBaseline;
        }

        private void HandleLoopCompleted()
        {
            lastLoopCompletedTime = Time.unscaledTime;
        }

        private bool IsAtAuthoredTrigger(Transform trigger, bool requireFacing)
        {
            Vector3 offset = player.position - trigger.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > triggerRadius * triggerRadius)
            {
                return false;
            }

            if (!requireFacing)
            {
                return true;
            }

            Vector3 playerForward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
            Vector3 requiredForward = Vector3.ProjectOnPlane(trigger.forward, Vector3.up).normalized;
            return Vector3.Dot(playerForward, requiredForward) >= minimumForwardAlignment;
        }

        private void DeliverKnock()
        {
            knockSource.Stop();
            knockSource.Play();
            KnockPlayCount++;
            Phase = SD1StudyPhase.KnockDelivered;
        }

        private void DeliverFootsteps()
        {
            footstepsElapsed = 0f;
            footstepsSource.transform.position = footstepsStart.position;
            footstepsSource.Stop();
            footstepsSource.Play();
            footstepsAreMoving = true;
            FootstepsPlayCount++;
            Phase = SD1StudyPhase.FootstepsDelivered;
        }

        private void DeliverBreathing()
        {
            breathingSource.Stop();
            breathingSource.Play();
            breathingEndedTime = Time.unscaledTime + breathingSource.clip.length;
            BreathingPlayCount++;
            Phase = SD1StudyPhase.BreathingDelivered;
        }

        private void DeliverWhisper()
        {
            whisperSource.Stop();
            whisperSource.Play();
            WhisperPlayCount++;

            if (subtitleDisplay != null)
            {
                subtitleDisplay.Show(spanishSubtitle, whisperSource.clip.length);
            }

            Phase = SD1StudyPhase.Recovery;
        }

        private void UpdateMovingFootsteps()
        {
            if (!footstepsAreMoving)
            {
                return;
            }

            footstepsElapsed += Time.unscaledDeltaTime;
            float duration = Mathf.Max(0.01f, footstepsSource.clip.length);
            float progress = Mathf.Clamp01(footstepsElapsed / duration);
            footstepsSource.transform.position = Vector3.Lerp(
                footstepsStart.position,
                footstepsEnd.position,
                Mathf.SmoothStep(0f, 1f, progress));

            if (progress >= 1f)
            {
                footstepsAreMoving = false;
            }
        }

        private void UpdateVisualConsequence()
        {
            if (!VisualConsequenceApplied
                && Phase == SD1StudyPhase.BreathingDelivered
                && Time.unscaledTime >= breathingEndedTime + consequenceDelay)
            {
                consequenceIsFading = true;
                Phase = SD1StudyPhase.AwaitingWhisper;
            }

            if (!consequenceIsFading)
            {
                return;
            }

            float targetIntensity = consequenceStartIntensity * consequenceIntensityMultiplier;
            float fadeSpeed = Mathf.Abs(consequenceStartIntensity - targetIntensity)
                / consequenceFadeDuration;
            consequenceLight.intensity = Mathf.MoveTowards(
                consequenceLight.intensity,
                targetIntensity,
                fadeSpeed * Time.unscaledDeltaTime);

            if (Mathf.Approximately(consequenceLight.intensity, targetIntensity))
            {
                consequenceIsFading = false;
                VisualConsequenceApplied = true;
            }
        }

        private static void ConfigureWorldSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.panStereo = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
        }

        private void ConfigureWhisperSource()
        {
            if (whisperSource == null)
            {
                return;
            }

            whisperSource.playOnAwake = false;
            whisperSource.loop = false;
            whisperSource.spatialBlend = 0f;
            whisperSource.dopplerLevel = 0f;
            whisperSource.panStereo = 0.55f;
        }

        private void StopOwnedSources()
        {
            if (knockSource != null)
            {
                knockSource.Stop();
            }

            if (footstepsSource != null)
            {
                footstepsSource.Stop();
            }

            if (breathingSource != null)
            {
                breathingSource.Stop();
            }

            if (whisperSource != null)
            {
                whisperSource.Stop();
            }
        }

        private void OnValidate()
        {
            triggerRadius = Mathf.Max(0.5f, triggerRadius);
            postRecallGrace = Mathf.Max(0f, postRecallGrace);
            consequenceDelay = Mathf.Max(0f, consequenceDelay);
            consequenceFadeDuration = Mathf.Max(0.1f, consequenceFadeDuration);
            consequenceIntensityMultiplier = Mathf.Clamp01(consequenceIntensityMultiplier);
        }
    }
}

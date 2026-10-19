using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class ParallelObservationRelay : MonoBehaviour, ILoopResettable
    {
        [Header("Readers")]
        [SerializeField] private OpticalReader echoReaderA;
        [SerializeField] private OpticalReader echoReaderB;
        [SerializeField] private OpticalReader playerServiceReader;

        [Header("Gates")]
        [SerializeField] private SlidingDoor gateA;
        [SerializeField] private SlidingDoor gateB;

        [Header("Readability")]
        [SerializeField] private GameObject echoAIndicator;
        [SerializeField] private GameObject echoBIndicator;
        [SerializeField] private GameObject playerIndicator;
        [SerializeField] private GameObject completeIndicator;
        [Header("Live cooperation")]
        [SerializeField] private bool requireSimultaneousPresence;
        [SerializeField, Min(.1f)] private float cooperationSeconds = .65f;
        private float cooperationTime;
        private bool previouslyHoldingB;
        public bool RequiresSimultaneousPresence => requireSimultaneousPresence;
        public float CooperationFraction => Mathf.Clamp01(cooperationTime / cooperationSeconds);
        public int MissedWindows { get; private set; }

        public bool EchoAAuthorized { get; private set; }

        public bool EchoBAuthorized { get; private set; }

        public bool IsComplete { get; private set; }

        public bool IsGateAPreviewActive => echoReaderA != null
            && echoReaderA.IsPlayerAligned
            && !EchoAAuthorized;

        private void Awake()
        {
            if (echoReaderA == null
                || echoReaderB == null
                || playerServiceReader == null
                || echoReaderA == echoReaderB
                || echoReaderA == playerServiceReader
                || echoReaderB == playerServiceReader
                || gateA == null
                || gateB == null
                || gateA == gateB
                || echoAIndicator == null
                || echoBIndicator == null
                || playerIndicator == null
                || completeIndicator == null)
            {
                Debug.LogError(
                    "ParallelObservationRelay requires three distinct readers, two distinct gates, and four indicators.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            if (echoReaderA.IsEchoAligned)
            {
                EchoAAuthorized = true;
            }

            if (requireSimultaneousPresence)
            {
                EchoBAuthorized = echoReaderB.IsEchoAligned;
                if (previouslyHoldingB && !EchoBAuthorized && !IsComplete) MissedWindows++;
                previouslyHoldingB = EchoBAuthorized;
            }
            else if (echoReaderB.IsEchoAligned)
            {
                EchoBAuthorized = true;
            }

            if (!IsComplete
                && EchoAAuthorized
                && EchoBAuthorized
                && playerServiceReader.IsPlayerAligned)
            {
                cooperationTime += Time.fixedDeltaTime;
                if (!requireSimultaneousPresence || cooperationTime >= cooperationSeconds)
                    IsComplete = true;
            }
            else if (!IsComplete) cooperationTime = 0;

            RefreshOutputs();
        }

        public void ResetLoopState()
        {
            EchoAAuthorized = false;
            EchoBAuthorized = false;
            IsComplete = false;
            cooperationTime = 0;
            previouslyHoldingB = false;
            RefreshOutputs();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void RefreshOutputs()
        {
            SetIndicatorState(echoAIndicator, EchoAAuthorized);
            SetIndicatorState(echoBIndicator, EchoBAuthorized);
            SetIndicatorState(
                playerIndicator,
                playerServiceReader != null && playerServiceReader.IsPlayerAligned);
            SetIndicatorState(completeIndicator, IsComplete);

            if (gateA != null)
            {
                gateA.SetOpen(EchoAAuthorized || IsGateAPreviewActive);
            }

            if (gateB != null)
            {
                gateB.SetOpen(IsComplete);
            }
        }

        private static void SetIndicatorState(GameObject indicator, bool isActive)
        {
            if (indicator != null && indicator.activeSelf != isActive)
            {
                indicator.SetActive(isActive);
            }
        }
    }
}

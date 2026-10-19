using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VS1OmittedPassageEvent : MonoBehaviour
    {
        public enum EventPhase { AwaitingSE2, AwaitingReturn, Revealed }

        [Header("Optional study — EN PRUEBA")]
        [SerializeField] private bool studyEnabled = true;
        [SerializeField] private VS1ParallelFigureEvent completedObservation;
        [SerializeField] private LoopController loopController;
        [SerializeField] private LoopActor player;

        [Header("Presentation only; keep route collision outside these roots")]
        [SerializeField] private GameObject clinicalBaselineRoot;
        [SerializeField] private GameObject domesticOverlayRoot;

        private BoxCollider returnVolume;
        private Vector3 previousLocalPosition;
        private bool hasPreviousPosition;

        public EventPhase CurrentPhase { get; private set; }
        public int RevealCount { get; private set; }
        public bool IsComplete => CurrentPhase == EventPhase.Revealed;
        public bool IsOverlayVisible => domesticOverlayRoot != null
            && domesticOverlayRoot.activeInHierarchy;
        public bool IsClinicalVisible => clinicalBaselineRoot != null
            && clinicalBaselineRoot.activeInHierarchy;

        private void Awake()
        {
            returnVolume = GetComponent<BoxCollider>();
            if (!returnVolume.isTrigger || completedObservation == null
                || loopController == null || player == null
                || player.Kind != LoopActorKind.Player
                || clinicalBaselineRoot == null || domesticOverlayRoot == null
                || clinicalBaselineRoot == domesticOverlayRoot)
            {
                Debug.LogError("VS1OmittedPassageEvent requires a return trigger, SE2, loop, actual Player actor, and distinct presentation roots.", this);
                enabled = false;
                return;
            }

            SetPresentation(false);
        }

        private void OnEnable()
        {
            hasPreviousPosition = false;
            if (loopController != null) loopController.LoopCompleted += HandleRecall;
        }

        private void OnDisable()
        {
            if (loopController != null) loopController.LoopCompleted -= HandleRecall;
            hasPreviousPosition = false;
            SetPresentation(false);
        }

        private void Update()
        {
            if (!studyEnabled || !player.isActiveAndEnabled)
            {
                hasPreviousPosition = false;
                SetPresentation(false);
                return;
            }

            bool observationComplete = completedObservation.CurrentPhase
                == VS1ParallelFigureEvent.EventPhase.Complete;
            if (CurrentPhase != EventPhase.Revealed)
                CurrentPhase = observationComplete ? EventPhase.AwaitingReturn : EventPhase.AwaitingSE2;

            Vector3 position = transform.InverseTransformPoint(player.transform.position)
                - returnVolume.center;
            // +local Z is the authored return direction. Sample only the explicitly
            // assigned Player: an Echo entering this volume cannot fire the study.
            if (observationComplete && CurrentPhase == EventPhase.AwaitingReturn
                && hasPreviousPosition && previousLocalPosition.z < 0f && position.z >= 0f)
            {
                float fraction = -previousLocalPosition.z / (position.z - previousLocalPosition.z);
                Vector3 crossing = Vector3.Lerp(previousLocalPosition, position, fraction);
                Vector3 halfSize = returnVolume.size * 0.5f;
                if (Mathf.Abs(crossing.x) <= halfSize.x && Mathf.Abs(crossing.y) <= halfSize.y)
                {
                    CurrentPhase = EventPhase.Revealed;
                    RevealCount++;
                }
            }

            previousLocalPosition = position;
            hasPreviousPosition = true;
            SetPresentation(CurrentPhase == EventPhase.Revealed);
        }

        private void HandleRecall()
        {
            // RECALL changes Player position, never counts as walking through the
            // plane, and never erases a revealed architectural consequence.
            hasPreviousPosition = false;
        }

        private void SetPresentation(bool reveal)
        {
            if (clinicalBaselineRoot != null && clinicalBaselineRoot.activeSelf == reveal)
                clinicalBaselineRoot.SetActive(!reveal);
            if (domesticOverlayRoot != null && domesticOverlayRoot.activeSelf != reveal)
                domesticOverlayRoot.SetActive(reveal);
        }
    }
}

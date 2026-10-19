using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VS1ParallelFigureEvent : MonoBehaviour
    {
        public enum EventPhase
        {
            AwaitingCrossing,
            AwaitingLook,
            Reading,
            Complete,
            Traversing
        }

        [Header("Gate")]
        [SerializeField] private LoopController loopController;
        [SerializeField] private BlindAlignmentRelay alignmentRelay;
        [SerializeField] private Transform playerView;

        [Header("Environmental states")]
        [SerializeField] private GameObject coldRoomRoot;
        [SerializeField] private GameObject domesticRoomRoot;
        [SerializeField] private GameObject figureRoot;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Collider visibilityGlass;

        [Header("Observation")]
        [SerializeField, Range(5f, 60f)] private float lookTolerance = 40f;
        [SerializeField, Min(0f)] private float lookQualificationSeconds = 0.2f;
        [SerializeField, Range(2f, 4f)] private float clearReadingSeconds = 3.2f;
        [Tooltip("Optional maximum Camera distance to the actual sealed glass surface. Zero preserves the original unrestricted-distance reading.")]
        [SerializeField, Min(0f)] private float maxObservationDistanceToGlass;

        [Header("Optional physical passage; legacy scenes keep glass observation")]
        [SerializeField] private bool physicalTraversal;
        [SerializeField] private LoopActor walkthroughPlayer;
        [SerializeField] private BoxCollider traversalEntry;
        [SerializeField] private BoxCollider traversalExit;
        [SerializeField] private BoxCollider revealPocket;
        [SerializeField] private BoxCollider[] additionalRevealPockets;
        [Tooltip("Stable volume enclosing every surface that changes. Exterior light spill must also be hidden by the authored shell.")]
        [SerializeField] private BoxCollider changeEnvelope;
        [Tooltip("Additional fixed receivers affected by changing shadows, such as the approach outside the observation glass.")]
        [SerializeField] private BoxCollider[] secondaryChangeEnvelopes;
        [SerializeField] private BoxCollider[] opaqueScreens;

        private readonly RaycastHit[] visibilityHits = new RaycastHit[24];
        private Camera playerCamera;
        private float lookQualificationElapsed;
        private FirstPersonController walkthroughController;
        private Vector3 previousWalkPosition;
        private bool hasWalkPosition;
        private bool enteredPassage;
        private bool presentationCommitted;

        public bool UsesPhysicalTraversal => physicalTraversal;
        public bool PresentationCommitted => presentationCommitted;
        public bool HasEnteredPassageThisAttempt => enteredPassage;
        public bool ObservedPresentation => IsDomesticVisible && QualifiedReadSeconds >= lookQualificationSeconds;
        public int TraversalEntryCount { get; private set; }
        public int TraversalExitCount { get; private set; }
        public string LastOcclusionMethod { get; private set; }

        public EventPhase CurrentPhase { get; private set; }

        public int AcceptedCrossingCount { get; private set; }

        public int AppearanceCount { get; private set; }

        public int CompletionCount { get; private set; }

        public float QualifiedReadSeconds { get; private set; }
        public float MaximumObservationDistanceToGlass => maxObservationDistanceToGlass;

        public bool IsColdRoomVisible => coldRoomRoot != null
            && coldRoomRoot.activeInHierarchy;

        public bool IsDomesticVisible => domesticRoomRoot != null
            && domesticRoomRoot.activeInHierarchy;

        public bool IsFigureVisible => figureRoot != null
            && figureRoot.activeInHierarchy;

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            playerCamera = playerView != null
                ? playerView.GetComponent<Camera>()
                : null;
            if (!trigger.isTrigger
                || loopController == null
                || alignmentRelay == null
                || playerView == null
                || playerCamera == null
                || coldRoomRoot == null
                || domesticRoomRoot == null
                || coldRoomRoot == domesticRoomRoot
                || figureRoot == null
                || lookTarget == null
                || (!physicalTraversal && (visibilityGlass == null || visibilityGlass.isTrigger))
                || (physicalTraversal && !ValidatePassage()))
            {
                Debug.LogError(
                    "VS1ParallelFigureEvent requires its trigger, loop, P2, Player camera, distinct presentations, figure and target; use either legacy solid glass or a fully configured physical passage.",
                    this);
                enabled = false;
                return;
            }

            ResetIncompleteAttempt();
        }

        private void OnEnable()
        {
            if (loopController != null)
            {
                loopController.LoopCompleted += HandleLoopCompleted;
            }
            if (walkthroughController != null) walkthroughController.PoseReset += InvalidateWalkPosition;
        }

        private void OnDisable()
        {
            if (loopController != null)
            {
                loopController.LoopCompleted -= HandleLoopCompleted;
            }
            if (walkthroughController != null) walkthroughController.PoseReset -= InvalidateWalkPosition;
            hasWalkPosition = false;

            if (!Application.isPlaying)
            {
                return;
            }

            // Physical completion does not imply that an offscreen transformation was
            // presented. Disabling this event must never manufacture that appearance.
            if (physicalTraversal) return;

            if (CurrentPhase == EventPhase.Complete)
            {
                SetDomesticPresentation(true);
            }
            else
            {
                ResetIncompleteAttempt();
            }
        }

        private void Update()
        {
            if (physicalTraversal)
            {
                UpdatePassage();
                return;
            }
            if (CurrentPhase == EventPhase.AwaitingLook)
            {
                if (!IsPlayerLookingThroughGlass())
                {
                    lookQualificationElapsed = 0f;
                    return;
                }

                lookQualificationElapsed += Time.deltaTime;
                if (lookQualificationElapsed < lookQualificationSeconds)
                {
                    return;
                }

                SetDomesticPresentation(true);
                AppearanceCount++;
                CurrentPhase = EventPhase.Reading;
                return;
            }

            if (CurrentPhase != EventPhase.Reading
                || !IsPlayerLookingThroughGlass())
            {
                return;
            }

            QualifiedReadSeconds = Mathf.Min(
                clearReadingSeconds,
                QualifiedReadSeconds + Time.deltaTime);
            if (QualifiedReadSeconds < clearReadingSeconds)
            {
                return;
            }

            CompletionCount++;
            CurrentPhase = EventPhase.Complete;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryAcceptCrossing(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAcceptCrossing(other);
        }

        private void TryAcceptCrossing(Collider other)
        {
            if ((physicalTraversal && Time.timeScale <= 0f) || CurrentPhase != EventPhase.AwaitingCrossing
                || !alignmentRelay.CanAdmitEchoCrossing)
            {
                return;
            }

            LoopActor actor = other.GetComponentInParent<LoopActor>();
            if (actor == null
                || !actor.isActiveAndEnabled
                || actor.Kind != LoopActorKind.Player
                || (physicalTraversal && actor != walkthroughPlayer))
            {
                return;
            }

            lookQualificationElapsed = 0f;
            AcceptedCrossingCount++;
            CurrentPhase = physicalTraversal ? EventPhase.Traversing : EventPhase.AwaitingLook;
        }

        private bool IsPlayerLookingThroughGlass()
        {
            // The approach is measured to the authored solid glass, not to the figure
            // or its room target. Moving either visual element cannot change this boundary.
            if (maxObservationDistanceToGlass > 0f && Vector3.Distance(playerView.position,
                visibilityGlass.ClosestPoint(playerView.position)) > maxObservationDistanceToGlass)
            {
                return false;
            }

            Vector3 viewportPoint = playerCamera.WorldToViewportPoint(lookTarget.position);
            if (viewportPoint.z <= 0f
                || viewportPoint.x < 0f || viewportPoint.x > 1f
                || viewportPoint.y < 0f || viewportPoint.y > 1f)
            {
                return false;
            }

            Vector3 toTarget = lookTarget.position - playerView.position;
            float distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon
                || Vector3.Angle(playerView.forward, toTarget / distance) > lookTolerance)
            {
                return false;
            }

            int hitCount = Physics.RaycastNonAlloc(
                playerView.position,
                toTarget / distance,
                visibilityHits,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount == visibilityHits.Length)
            {
                // A full buffer cannot establish that the entire sightline is clear.
                return false;
            }

            bool crossedGlass = false;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = visibilityHits[index];
                if (hit.collider == visibilityGlass)
                {
                    crossedGlass = true;
                    continue;
                }

                if (hit.transform != figureRoot.transform
                    && !hit.transform.IsChildOf(figureRoot.transform))
                {
                    return false;
                }
            }

            return crossedGlass;
        }

        private void HandleLoopCompleted()
        {
            if (CurrentPhase != EventPhase.Complete)
            {
                ResetIncompleteAttempt();
            }
        }

        private void ResetIncompleteAttempt()
        {
            lookQualificationElapsed = 0f;
            if (!physicalTraversal || !presentationCommitted) QualifiedReadSeconds = 0f;
            CurrentPhase = EventPhase.AwaitingCrossing;
            enteredPassage = false;
            hasWalkPosition = false;
            // A revealed environment remains real on RECALL; only traversal eligibility
            // rearms. Neither a new take nor a missed view reverses the physical room.
            SetDomesticPresentation(physicalTraversal && presentationCommitted);
        }

        private bool ValidatePassage()
        {
            if (walkthroughPlayer == null || walkthroughPlayer.Kind != LoopActorKind.Player
                || !playerView.IsChildOf(walkthroughPlayer.transform)
                || !IsTrigger(traversalEntry) || !IsTrigger(traversalExit)
                || !IsTrigger(revealPocket) || !IsTrigger(changeEnvelope)
                || opaqueScreens == null || opaqueScreens.Length == 0) return false;
            walkthroughController = walkthroughPlayer.GetComponent<FirstPersonController>();
            if (walkthroughController == null) return false;
            foreach (BoxCollider screen in opaqueScreens)
                if (screen == null || screen.isTrigger) return false;
            if (additionalRevealPockets != null)
                foreach (BoxCollider pocket in additionalRevealPockets)
                    if (!IsTrigger(pocket)) return false;
            if (secondaryChangeEnvelopes != null)
                foreach (BoxCollider envelope in secondaryChangeEnvelopes)
                    if (!IsTrigger(envelope)) return false;
            return true;
        }

        private static bool IsTrigger(BoxCollider volume) => volume != null && volume.isTrigger;
        private static bool Contains(BoxCollider volume, Vector3 point) => volume.enabled
            && volume.gameObject.activeInHierarchy
            && new Bounds(volume.center, volume.size).Contains(volume.transform.InverseTransformPoint(point));
        private void InvalidateWalkPosition() => hasWalkPosition = false;

        private static bool Entered(BoxCollider volume, Vector3 from, Vector3 to)
        {
            if (!volume.enabled || !volume.gameObject.activeInHierarchy || Contains(volume, from)) return false;
            Vector3 delta = to - from;
            return Contains(volume, to) || (delta.sqrMagnitude > .000001f
                && volume.Raycast(new Ray(from, delta.normalized), out RaycastHit hit, delta.magnitude));
        }

        private void UpdatePassage()
        {
            if (!walkthroughPlayer.isActiveAndEnabled)
            {
                hasWalkPosition = false;
                return;
            }
            Vector3 position = walkthroughPlayer.transform.position + Vector3.up;
            if (Time.timeScale <= 0f || !walkthroughController.isActiveAndEnabled)
            {
                // Keep a stationary baseline so movement on the first resumed frame
                // can cross a boundary. No progress is sampled while controls are blocked.
                previousWalkPosition = position;
                hasWalkPosition = true;
                return;
            }
            bool admitted = CurrentPhase == EventPhase.Traversing || CurrentPhase == EventPhase.Complete;
            bool inPocket = Contains(revealPocket, position);
            if (!inPocket && additionalRevealPockets != null)
                foreach (BoxCollider pocket in additionalRevealPockets)
                    if (Contains(pocket, position)) { inPocket = true; break; }
            if (admitted && !presentationCommitted && inPocket
                && EntireChangeIsHidden())
            {
                SetDomesticPresentation(true);
                presentationCommitted = true;
                AppearanceCount++;
            }

            // PoseReset invalidates the prior sample, including same-distance teleports.
            // Only a continuous physical entry followed by a physical exit earns progress.
            if (CurrentPhase == EventPhase.Traversing && hasWalkPosition)
            {
                if (!enteredPassage && Entered(traversalEntry, previousWalkPosition, position))
                {
                    enteredPassage = true;
                    TraversalEntryCount++;
                }
                if (enteredPassage && Entered(traversalExit, previousWalkPosition, position))
                {
                    TraversalExitCount++;
                    CompletionCount++;
                    CurrentPhase = EventPhase.Complete;
                }
            }
            previousWalkPosition = position;
            hasWalkPosition = true;
            if (presentationCommitted && IsLookingAtPresentedFigure())
                QualifiedReadSeconds += Time.deltaTime;
        }

        private bool EntireChangeIsHidden()
        {
            if (!EnvelopeIsHidden(changeEnvelope, out string method)) return false;
            int volumes = 1;
            if (secondaryChangeEnvelopes != null)
                foreach (BoxCollider envelope in secondaryChangeEnvelopes)
                {
                    if (!EnvelopeIsHidden(envelope, out string extra)) return false;
                    method += "; " + extra;
                    volumes++;
                }
            LastOcclusionMethod = volumes + " presentation/shadow volumes: " + method;
            return true;
        }

        private bool EnvelopeIsHidden(BoxCollider envelope, out string method)
        {
            method = null;
            if (!envelope.enabled || !envelope.gameObject.activeInHierarchy) return false;
            if (Contains(envelope, playerView.position)) return false;
            Bounds bounds = envelope.bounds;
            if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerCamera), bounds))
            {
                method = "entire envelope outside camera frustum";
                return true;
            }
            // A single convex, opaque box must cover the whole envelope. Independent
            // corner hits on different walls cannot establish full occlusion between them.
            foreach (BoxCollider screen in opaqueScreens)
            {
                if (!screen.enabled || !screen.gameObject.activeInHierarchy
                    || screen.bounds.Intersects(bounds) || screen.bounds.Contains(playerView.position)) continue;
                bool hidden = true;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = envelope.center + Vector3.Scale(envelope.size * .5f,
                        new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    Vector3 ray = envelope.transform.TransformPoint(point) - playerView.position;
                    if (!screen.Raycast(new Ray(playerView.position, ray.normalized), out RaycastHit hit, ray.magnitude)
                        || hit.distance >= ray.magnitude - .01f) { hidden = false; break; }
                }
                if (hidden)
                {
                    method = "whole envelope behind " + screen.name;
                    return true;
                }
            }
            return false;
        }

        private bool IsLookingAtPresentedFigure()
        {
            Vector3 toTarget = lookTarget.position - playerView.position;
            Vector3 viewport = playerCamera.WorldToViewportPoint(lookTarget.position);
            if (viewport.z <= 0 || viewport.x < 0 || viewport.x > 1 || viewport.y < 0 || viewport.y > 1
                || Vector3.Angle(playerView.forward, toTarget) > lookTolerance) return false;
            int count = Physics.RaycastNonAlloc(playerView.position, toTarget.normalized, visibilityHits,
                toTarget.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == visibilityHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Transform hit = visibilityHits[i].transform;
                if (visibilityHits[i].collider == visibilityGlass) continue;
                if (hit != figureRoot.transform && !hit.IsChildOf(figureRoot.transform)) return false;
            }
            return true;
        }

        private void SetDomesticPresentation(bool revealDomestic)
        {
            if (coldRoomRoot != null)
            {
                coldRoomRoot.SetActive(!revealDomestic);
            }

            if (domesticRoomRoot != null)
            {
                domesticRoomRoot.SetActive(revealDomestic);
            }

            if (figureRoot != null)
            {
                figureRoot.SetActive(revealDomestic);
            }
        }

        private void OnValidate()
        {
            lookTolerance = Mathf.Clamp(lookTolerance, 5f, 60f);
            lookQualificationSeconds = Mathf.Max(0f, lookQualificationSeconds);
            clearReadingSeconds = Mathf.Clamp(clearReadingSeconds, 2f, 4f);
            maxObservationDistanceToGlass = Mathf.Max(0f, maxObservationDistanceToGlass);
        }
    }
}

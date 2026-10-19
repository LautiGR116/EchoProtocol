using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    // One authored P3 exit and its one-way arrival at the original Hub.
    // Unlock persists; the two arrival leaves never provide a reverse route.
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class VS1HubReturnGate : MonoBehaviour
    {
        [SerializeField] private LoopActor player;
        [SerializeField] private CharacterController playerBody;
        [SerializeField] private LoopController loop;
        [SerializeField] private RecombinationRelay puzzle;
        [SerializeField] private SlidingDoor finalPuzzleGate;
        [SerializeField] private SlidingDoor returnGate;
        [SerializeField] private SlidingDoor arrivalRear;
        [SerializeField] private SlidingDoor arrivalFront;
        [SerializeField] private BoxCollider arrivalInterior;
        [SerializeField] private BoxCollider arrivalClear;
        [SerializeField] private BoxCollider arrivalOccupancy;
        [SerializeField] private GameObject residualRoot;
        [SerializeField] private GameObject arrivalLightRoot;

        private bool sampled;
        private float previousZ;
        private bool qualified;
        private enum ArrivalPhase { Receiving, SealingRear, Releasing, SealingFront }
        private ArrivalPhase arrival;
        public bool IsOpen { get; private set; }
        public int OpeningCount { get; private set; }
        public string ArrivalState => arrival.ToString();

        private void Awake()
        {
            if (!GetComponent<BoxCollider>().isTrigger || player == null
                || player.Kind != LoopActorKind.Player || playerBody == null
                || playerBody.GetComponent<LoopActor>() != player || loop == null
                || puzzle == null || finalPuzzleGate == null || returnGate == null
                || arrivalRear == null || arrivalFront == null || arrivalRear == arrivalFront
                || arrivalInterior == null || arrivalClear == null || arrivalOccupancy == null
                || residualRoot == null || arrivalLightRoot == null)
            {
                Debug.LogError("VS1HubReturnGate requires the real Player, P3, solid exit/arrival doors and explicit arrival volumes/presentation.", this);
                enabled = false;
                return;
            }
            residualRoot.SetActive(false);
            arrivalLightRoot.SetActive(false);
        }

        private void OnEnable()
        {
            sampled = false;
            if (loop != null) loop.LoopCompleted += OnRecall;
        }

        private void OnDisable()
        {
            if (loop != null) loop.LoopCompleted -= OnRecall;
            sampled = false;
        }

        private void OnRecall()
        {
            sampled = false;
            // Seal the Hub side before permitting another forward traversal.
            arrival = ArrivalPhase.SealingFront;
            arrivalFront.SetOpen(false);
            arrivalRear.SetOpen(false);
        }

        private void OnTriggerEnter(Collider other) => ObserveCrossing(other);
        private void OnTriggerStay(Collider other) => ObserveCrossing(other);
        private void OnTriggerExit(Collider other)
        {
            if (other == playerBody) sampled = false;
        }

        private void ObserveCrossing(Collider other)
        {
            if (IsOpen || other != playerBody || !player.isActiveAndEnabled
                || !playerBody.enabled || other.GetComponentInParent<LoopActor>() != player)
                return;
            float z = transform.InverseTransformPoint(player.transform.position).z;
            bool valid = puzzle.isActiveAndEnabled && puzzle.IsAuthorized
                && puzzle.CanCrossExit && finalPuzzleGate.IsFullyOpen
                && (puzzle.RequiresPairedTransfer || (loop.ActiveEcho != null && loop.ActiveEcho.gameObject.activeInHierarchy));
            // Actual trigger occupancy on consecutive physics observations, on
            // both sides of the final plane; R invalidates the previous sample.
            if (sampled && qualified && valid
                && previousZ < 0 && z >= 0)
            {
                IsOpen = true;
                OpeningCount++;
                returnGate.SetOpen(true);
                residualRoot.SetActive(true);
                arrivalLightRoot.SetActive(true);
            }
            previousZ = z;
            qualified = valid;
            sampled = true;
        }

        private bool Inside(BoxCollider volume)
        {
            if (!player.isActiveAndEnabled || !playerBody.enabled || !volume.enabled
                || !volume.gameObject.activeInHierarchy) return false;
            // An oblique vestibule must use its actual box, not the larger world AABB.
            Vector3 localPoint = volume.transform.InverseTransformPoint(player.transform.position + Vector3.up);
            return new Bounds(volume.center, volume.size).Contains(localPoint);
        }

        private void FixedUpdate()
        {
            if (!IsOpen) return;
            returnGate.SetOpen(true);
            switch (arrival)
            {
                case ArrivalPhase.Receiving:
                    arrivalFront.SetOpen(false);
                    arrivalRear.SetOpen(arrivalFront.IsFullyClosed);
                    if (Inside(arrivalInterior)) arrival = ArrivalPhase.SealingRear;
                    break;
                case ArrivalPhase.SealingRear:
                    arrivalRear.SetOpen(false);
                    if (!Inside(arrivalOccupancy)) arrival = ArrivalPhase.Receiving;
                    else if (arrivalRear.IsFullyClosed) arrival = ArrivalPhase.Releasing;
                    break;
                case ArrivalPhase.Releasing:
                    arrivalRear.SetOpen(false);
                    arrivalFront.SetOpen(arrivalRear.IsFullyClosed);
                    if (Inside(arrivalClear)) arrival = ArrivalPhase.SealingFront;
                    break;
                case ArrivalPhase.SealingFront:
                    arrivalFront.SetOpen(false);
                    arrivalRear.SetOpen(false);
                    if (arrivalFront.IsFullyClosed) arrival = Inside(arrivalOccupancy)
                        ? ArrivalPhase.Releasing : ArrivalPhase.Receiving;
                    break;
            }
        }
    }
}

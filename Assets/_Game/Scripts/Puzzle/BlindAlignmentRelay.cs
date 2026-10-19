using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class BlindAlignmentRelay : MonoBehaviour, ILoopResettable
    {
        [Header("Cause and consequence")]
        [SerializeField] private OpticalReader alignmentReader;
        [SerializeField] private SlidingDoor controlledGate;

        [Header("Optional powered interlock — Incursion P2")]
        [SerializeField] private bool requirePoweredClosure;
        [SerializeField] private SlidingDoor onwardGate;
        [SerializeField] private Transform player;
        [SerializeField] private BoxCollider interior;
        [SerializeField] private BoxCollider transferBoundary;
        [SerializeField] private BoxCollider entranceSafety;
        [SerializeField] private BoxCollider onwardSafety;
        [SerializeField] private BoxCollider acousticInterior;

        public bool RequiresPoweredClosure => requirePoweredClosure;
        public bool IsPowered => alignmentReader != null
            && (alignmentReader.IsPlayerPresent || alignmentReader.IsEchoPresent);
        public bool IsEchoPowering => alignmentReader != null && alignmentReader.IsEchoPresent;
        public bool IsPlayerInside => Contains(interior);
        public bool IsEntranceClosed => controlledGate && controlledGate.IsFullyClosed;
        public bool IsExitAuthorized => requirePoweredClosure && IsPowered
            && !alignmentReader.HasFacingPresence && !IsGateAuthorized && IsEntranceClosed;
        public bool IsPlayerSeparated => requirePoweredClosure && IsEntranceClosed && Contains(acousticInterior);
        public bool HasTransferred { get; private set; }
        public bool CanAdmitEchoCrossing => requirePoweredClosure ? HasTransferred : IsEchoHoldingGate;

        public bool IsPlayerPreviewActive => alignmentReader != null
            && alignmentReader.IsPlayerAligned;

        public bool IsEchoHoldingGate => alignmentReader != null
            && alignmentReader.IsEchoAligned;

        public bool IsGateAuthorized => IsPlayerPreviewActive || IsEchoHoldingGate;

        public int EchoHoldAcquisitionCount { get; private set; }

        private bool wasEchoHoldingGate;

        private void Awake()
        {
            if (alignmentReader == null || controlledGate == null)
            {
                Debug.LogError(
                    "BlindAlignmentRelay requires an optical reader and a controlled gate.",
                    this);
                enabled = false;
                return;
            }

            if (requirePoweredClosure && (!onwardGate || !player || !interior
                || !transferBoundary || !entranceSafety || !onwardSafety || !acousticInterior))
            {
                Debug.LogError("Powered P2 requires an onward gate, Player, interior, transfer, acoustic and safety volumes.", this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            bool echoIsHoldingGate = IsEchoHoldingGate;
            if (echoIsHoldingGate && !wasEchoHoldingGate)
            {
                EchoHoldAcquisitionCount++;
            }

            wasEchoHoldingGate = echoIsHoldingGate;
            // The same real occupant supplies power while its recorded orientation
            // changes. No success timer, replay extension or invented Echo action.
            bool entranceOpen = IsGateAuthorized;
            if (requirePoweredClosure && !entranceOpen && Contains(entranceSafety)
                && !controlledGate.IsFullyClosed)
                entranceOpen = true;
            controlledGate.SetOpen(entranceOpen);
            if (!requirePoweredClosure) return;

            bool exitAuthorized = IsExitAuthorized;
            if (!HasTransferred && exitAuthorized && IsEchoPowering
                && onwardGate.IsFullyOpen && Contains(transferBoundary))
                HasTransferred = true;

            // Occupied thresholds stop a descending leaf, but cannot grant SE2
            // admission after a failed take. Admission requires the powered crossing.
            onwardGate.SetOpen(exitAuthorized || (Contains(onwardSafety)
                && !onwardGate.IsFullyClosed));
        }

        private bool Contains(BoxCollider volume) => player && volume && volume.enabled
            && volume.gameObject.activeInHierarchy && new Bounds(volume.center, volume.size)
                .Contains(volume.transform.InverseTransformPoint(player.position + Vector3.up));

        public void ResetLoopState()
        {
            wasEchoHoldingGate = false;
            EchoHoldAcquisitionCount = 0;
            HasTransferred = false;

            if (controlledGate != null)
            {
                controlledGate.SetOpen(false);
            }
            if (requirePoweredClosure && onwardGate) onwardGate.ResetLoopState();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }
    }
}

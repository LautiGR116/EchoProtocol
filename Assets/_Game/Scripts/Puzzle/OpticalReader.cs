using System;
using System.Collections.Generic;
using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [Flags]
    public enum OpticalReaderActorMask
    {
        Player = 1,
        Echo = 2,
        Both = Player | Echo
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class OpticalReader : MonoBehaviour, ILoopResettable
    {
        [Header("Detection")]
        [SerializeField] private OpticalReaderActorMask acceptedActors =
            OpticalReaderActorMask.Both;
        [SerializeField, Range(5f, 90f)] private float facingTolerance = 35f;
        [SerializeField, Min(0f)] private float qualificationSeconds = 0.25f;

        [Header("Readability")]
        [SerializeField] private GameObject activeIndicator;

        private readonly Dictionary<Collider, LoopActor> colliderOccupants =
            new Dictionary<Collider, LoopActor>();
        private readonly Dictionary<LoopActor, int> actorOccupancyCounts =
            new Dictionary<LoopActor, int>();
        private readonly Dictionary<LoopActor, float> facingDurations =
            new Dictionary<LoopActor, float>();
        private readonly HashSet<LoopActor> alignedActors =
            new HashSet<LoopActor>();
        private readonly List<Collider> staleColliders = new List<Collider>();
        private readonly List<LoopActor> trackedActors = new List<LoopActor>();

        private BoxCollider triggerVolume;

        public bool IsAligned => alignedActors.Count > 0;

        // A powered interlock must not release its exit during the short read
        // qualification interval when an occupant is already facing this reader.
        public bool HasFacingPresence
        {
            get
            {
                foreach (LoopActor actor in actorOccupancyCounts.Keys)
                    if (actor != null && actor.isActiveAndEnabled
                        && IsFacingRequiredDirection(actor.transform)) return true;
                return false;
            }
        }

        public bool IsPlayerAligned => IsKindAligned(LoopActorKind.Player);

        public bool IsEchoAligned => IsKindAligned(LoopActorKind.Echo);

        public bool IsPlayerPresent => IsKindPresent(LoopActorKind.Player);

        public bool IsEchoPresent => IsKindPresent(LoopActorKind.Echo);

        public int OccupyingActorCount => actorOccupancyCounts.Count;

        public event Action<bool> AlignmentChanged;

        private void Awake()
        {
            triggerVolume = GetComponent<BoxCollider>();
            if (!triggerVolume.isTrigger || activeIndicator == null)
            {
                Debug.LogError(
                    "OpticalReader requires a trigger BoxCollider and an active indicator.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            PurgeStaleColliders();
            RefreshAlignment();
        }

        private void OnTriggerEnter(Collider other)
        {
            TrackCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TrackCollider(other);
        }

        private void OnTriggerExit(Collider other)
        {
            RemoveCollider(other);
        }

        public void ResetLoopState()
        {
            foreach (LoopActor actor in actorOccupancyCounts.Keys)
            {
                if (actor != null)
                {
                    actor.BecameUnavailable -= HandleActorUnavailable;
                }
            }

            colliderOccupants.Clear();
            actorOccupancyCounts.Clear();
            facingDurations.Clear();
            alignedActors.Clear();
            staleColliders.Clear();
            trackedActors.Clear();
            SetIndicatorState(false, true);
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void TrackCollider(Collider other)
        {
            if (other == null || colliderOccupants.ContainsKey(other))
            {
                return;
            }

            LoopActor actor = other.GetComponentInParent<LoopActor>();
            if (actor == null
                || !actor.isActiveAndEnabled
                || !Accepts(actor.Kind))
            {
                return;
            }

            colliderOccupants.Add(other, actor);
            actorOccupancyCounts.TryGetValue(actor, out int colliderCount);
            if (colliderCount == 0)
            {
                actor.BecameUnavailable += HandleActorUnavailable;
                facingDurations[actor] = 0f;
            }

            actorOccupancyCounts[actor] = colliderCount + 1;
        }

        private void RemoveCollider(Collider occupantCollider)
        {
            if (!colliderOccupants.TryGetValue(occupantCollider, out LoopActor actor))
            {
                return;
            }

            colliderOccupants.Remove(occupantCollider);
            int remainingColliderCount = actorOccupancyCounts[actor] - 1;
            if (remainingColliderCount > 0)
            {
                actorOccupancyCounts[actor] = remainingColliderCount;
                return;
            }

            actorOccupancyCounts.Remove(actor);
            facingDurations.Remove(actor);
            alignedActors.Remove(actor);
            if (actor != null)
            {
                actor.BecameUnavailable -= HandleActorUnavailable;
            }

            SetIndicatorState(IsAligned, false);
        }

        private void HandleActorUnavailable(LoopActor unavailableActor)
        {
            staleColliders.Clear();
            foreach (KeyValuePair<Collider, LoopActor> occupant in colliderOccupants)
            {
                if (occupant.Value == unavailableActor)
                {
                    staleColliders.Add(occupant.Key);
                }
            }

            for (int index = 0; index < staleColliders.Count; index++)
            {
                RemoveCollider(staleColliders[index]);
            }
        }

        private void PurgeStaleColliders()
        {
            staleColliders.Clear();
            foreach (KeyValuePair<Collider, LoopActor> occupant in colliderOccupants)
            {
                Collider occupantCollider = occupant.Key;
                LoopActor actor = occupant.Value;
                if (occupantCollider == null
                    || actor == null
                    || !occupantCollider.enabled
                    || !occupantCollider.gameObject.activeInHierarchy
                    || !actor.isActiveAndEnabled
                    || !triggerVolume.bounds.Intersects(occupantCollider.bounds))
                {
                    staleColliders.Add(occupantCollider);
                }
            }

            for (int index = 0; index < staleColliders.Count; index++)
            {
                RemoveCollider(staleColliders[index]);
            }
        }

        private void RefreshAlignment()
        {
            bool wasAligned = IsAligned;
            trackedActors.Clear();
            trackedActors.AddRange(actorOccupancyCounts.Keys);

            for (int index = 0; index < trackedActors.Count; index++)
            {
                LoopActor actor = trackedActors[index];
                if (actor == null)
                {
                    continue;
                }

                if (IsFacingRequiredDirection(actor.transform))
                {
                    float duration = facingDurations[actor] + Time.fixedDeltaTime;
                    facingDurations[actor] = duration;
                    if (duration >= qualificationSeconds)
                    {
                        alignedActors.Add(actor);
                    }
                }
                else
                {
                    facingDurations[actor] = 0f;
                    alignedActors.Remove(actor);
                }
            }

            SetIndicatorState(IsAligned, wasAligned == IsAligned);
        }

        private bool IsFacingRequiredDirection(Transform actorTransform)
        {
            Vector3 actorForward = Vector3.ProjectOnPlane(
                actorTransform.forward,
                Vector3.up).normalized;
            Vector3 requiredForward = Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up).normalized;
            return actorForward.sqrMagnitude > 0.99f
                && requiredForward.sqrMagnitude > 0.99f
                && Vector3.Angle(actorForward, requiredForward) <= facingTolerance;
        }

        private bool IsKindAligned(LoopActorKind kind)
        {
            foreach (LoopActor actor in alignedActors)
            {
                if (actor != null && actor.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsKindPresent(LoopActorKind kind)
        {
            foreach (LoopActor actor in actorOccupancyCounts.Keys)
                if (actor != null && actor.isActiveAndEnabled && actor.Kind == kind)
                    return true;
            return false;
        }

        private bool Accepts(LoopActorKind kind)
        {
            OpticalReaderActorMask actorMask = kind == LoopActorKind.Player
                ? OpticalReaderActorMask.Player
                : OpticalReaderActorMask.Echo;
            return (acceptedActors & actorMask) != 0;
        }

        private void SetIndicatorState(bool isActive, bool suppressEvent)
        {
            bool previousState = activeIndicator != null && activeIndicator.activeSelf;
            if (activeIndicator != null && previousState != isActive)
            {
                activeIndicator.SetActive(isActive);
            }

            if (!suppressEvent && previousState != isActive)
            {
                AlignmentChanged?.Invoke(isActive);
            }
        }

        private void OnValidate()
        {
            facingTolerance = Mathf.Clamp(facingTolerance, 5f, 90f);
            qualificationSeconds = Mathf.Max(0f, qualificationSeconds);
        }
    }
}

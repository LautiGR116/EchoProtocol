using System.Collections.Generic;
using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PressurePlate : MonoBehaviour, ILoopResettable
    {
        [Header("References")]
        [SerializeField] private SlidingDoor controlledDoor;
        [SerializeField] private Transform plateVisual;
        [SerializeField] private GameObject activeIndicator;

        [Header("Visual State")]
        [SerializeField] private Vector3 releasedLocalPosition;
        [SerializeField] private Vector3 pressedLocalPosition = new Vector3(0f, -0.08f, 0f);

        private readonly Dictionary<Collider, LoopActor> colliderOccupants =
            new Dictionary<Collider, LoopActor>();
        private readonly Dictionary<LoopActor, int> actorOccupancyCounts =
            new Dictionary<LoopActor, int>();
        private readonly List<Collider> staleColliders = new List<Collider>();

        private BoxCollider triggerVolume;

        public bool IsPressed { get; private set; }

        public int OccupyingActorCount => actorOccupancyCounts.Count;

        public event System.Action<bool> PressedChanged;

        private void Awake()
        {
            triggerVolume = GetComponent<BoxCollider>();
            if (!triggerVolume.isTrigger
                || plateVisual == null
                || activeIndicator == null)
            {
                Debug.LogError(
                    "PressurePlate requires a trigger BoxCollider, plate visual, and indicator.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            PurgeStaleColliders();
        }

        private void OnTriggerEnter(Collider other)
        {
            TrackCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TrackCollider(other);
        }

        private void TrackCollider(Collider other)
        {
            if (colliderOccupants.ContainsKey(other))
            {
                return;
            }

            LoopActor actor = other.GetComponentInParent<LoopActor>();
            if (actor == null || !actor.isActiveAndEnabled)
            {
                return;
            }

            colliderOccupants.Add(other, actor);
            actorOccupancyCounts.TryGetValue(actor, out int colliderCount);
            if (colliderCount == 0)
            {
                actor.BecameUnavailable += HandleActorUnavailable;
            }

            actorOccupancyCounts[actor] = colliderCount + 1;
            RefreshPressedState();
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
            staleColliders.Clear();
            SetPressed(false, true);
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void PurgeStaleColliders()
        {
            if (colliderOccupants.Count == 0)
            {
                return;
            }

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

        private void RemoveCollider(Collider occupantCollider)
        {
            if (!colliderOccupants.TryGetValue(occupantCollider, out LoopActor actor))
            {
                return;
            }

            colliderOccupants.Remove(occupantCollider);
            int remainingColliderCount = actorOccupancyCounts[actor] - 1;
            if (remainingColliderCount <= 0)
            {
                actorOccupancyCounts.Remove(actor);
                if (actor != null)
                {
                    actor.BecameUnavailable -= HandleActorUnavailable;
                }
            }
            else
            {
                actorOccupancyCounts[actor] = remainingColliderCount;
            }

            RefreshPressedState();
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

        private void RefreshPressedState()
        {
            SetPressed(actorOccupancyCounts.Count > 0, false);
        }

        private void SetPressed(bool shouldBePressed, bool forceVisualRefresh)
        {
            bool stateChanged = IsPressed != shouldBePressed;
            if (!forceVisualRefresh && !stateChanged)
            {
                return;
            }

            IsPressed = shouldBePressed;

            if (plateVisual != null)
            {
                plateVisual.localPosition = shouldBePressed
                    ? pressedLocalPosition
                    : releasedLocalPosition;
            }

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(shouldBePressed);
            }

            if (controlledDoor != null)
            {
                controlledDoor.SetOpen(shouldBePressed);
            }

            if (stateChanged && !forceVisualRefresh)
            {
                PressedChanged?.Invoke(IsPressed);
            }
        }
    }
}

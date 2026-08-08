using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypePuzzleGoal : MonoBehaviour, ILoopResettable
    {
        [SerializeField] private GameObject completionIndicator;

        private BoxCollider triggerVolume;

        public bool IsComplete { get; private set; }

        private void Awake()
        {
            triggerVolume = GetComponent<BoxCollider>();
            if (!triggerVolume.isTrigger || completionIndicator == null)
            {
                Debug.LogError(
                    "PrototypePuzzleGoal requires a trigger BoxCollider and completion indicator.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void OnTriggerEnter(Collider other)
        {
            LoopActor actor = other.GetComponentInParent<LoopActor>();
            if (actor == null
                || !actor.isActiveAndEnabled
                || actor.Kind != LoopActorKind.Player)
            {
                return;
            }

            SetComplete(true);
        }

        public void ResetLoopState()
        {
            SetComplete(false);
        }

        private void SetComplete(bool isComplete)
        {
            IsComplete = isComplete;

            if (completionIndicator != null)
            {
                completionIndicator.SetActive(isComplete);
            }
        }
    }
}

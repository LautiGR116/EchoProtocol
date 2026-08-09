using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    public sealed class EnvironmentalDiscrepancyController : MonoBehaviour
    {
        [SerializeField] private LoopController loopController;
        [SerializeField] private PrototypePuzzleGoal puzzleGoal;
        [SerializeField] private GameObject discrepancyRoot;

        private bool revealArmed;

        public event System.Action Revealed;

        public bool IsRevealArmed => revealArmed && !IsRevealed;

        public bool IsRevealed { get; private set; }

        private void Awake()
        {
            if (loopController == null
                || puzzleGoal == null
                || discrepancyRoot == null
                || discrepancyRoot == gameObject)
            {
                Debug.LogError(
                    "EnvironmentalDiscrepancyController requires a loop, a goal, and a separate discrepancy root.",
                    this);
                enabled = false;
                return;
            }

            SetRevealed(false);
        }

        private void OnEnable()
        {
            if (puzzleGoal != null)
            {
                puzzleGoal.Completed += HandleGoalCompleted;
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
                puzzleGoal.Completed -= HandleGoalCompleted;
            }

            if (loopController != null)
            {
                loopController.LoopCompleted -= HandleLoopCompleted;
            }
        }

        private void HandleGoalCompleted()
        {
            if (!IsRevealed)
            {
                revealArmed = true;
            }
        }

        private void HandleLoopCompleted()
        {
            if (!revealArmed || IsRevealed)
            {
                return;
            }

            SetRevealed(true);
            revealArmed = false;
        }

        private void SetRevealed(bool isRevealed)
        {
            bool didReveal = isRevealed && !IsRevealed;
            IsRevealed = isRevealed;

            if (discrepancyRoot.activeSelf != isRevealed)
            {
                discrepancyRoot.SetActive(isRevealed);
            }

            if (didReveal)
            {
                Revealed?.Invoke();
            }
        }
    }
}

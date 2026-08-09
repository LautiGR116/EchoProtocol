using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    public enum SequentialPlateDoorPhase
    {
        AwaitingFirst,
        AwaitingSecond,
        HoldingOpen
    }

    [DisallowMultipleComponent]
    public sealed class SequentialPlateDoorCondition : MonoBehaviour, ILoopResettable
    {
        [Header("Sequence")]
        [SerializeField] private PressurePlate firstPlate;
        [SerializeField] private PressurePlate secondPlate;
        [SerializeField] private SlidingDoor controlledDoor;

        [Header("Readability")]
        [SerializeField] private GameObject firstStepIndicator;
        [SerializeField] private GameObject secondStepIndicator;

        private bool wasFirstPressed;
        private bool wasSecondPressed;

        public SequentialPlateDoorPhase Phase { get; private set; }

        public bool IsSatisfied => Phase == SequentialPlateDoorPhase.HoldingOpen
            && secondPlate != null
            && secondPlate.IsPressed;

        private void Awake()
        {
            if (firstPlate == null
                || secondPlate == null
                || firstPlate == secondPlate
                || controlledDoor == null
                || firstStepIndicator == null
                || secondStepIndicator == null)
            {
                Debug.LogError(
                    "SequentialPlateDoorCondition requires two distinct plates, one door, and two indicators.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            bool isFirstPressed = firstPlate.IsPressed;
            bool isSecondPressed = secondPlate.IsPressed;
            bool firstWasJustPressed = isFirstPressed && !wasFirstPressed;
            bool secondWasJustPressed = isSecondPressed && !wasSecondPressed;

            switch (Phase)
            {
                case SequentialPlateDoorPhase.AwaitingFirst:
                    if (firstWasJustPressed)
                    {
                        Phase = SequentialPlateDoorPhase.AwaitingSecond;
                    }
                    break;

                case SequentialPlateDoorPhase.AwaitingSecond:
                    if (secondWasJustPressed)
                    {
                        Phase = SequentialPlateDoorPhase.HoldingOpen;
                    }
                    break;

                case SequentialPlateDoorPhase.HoldingOpen:
                    if (!isSecondPressed)
                    {
                        ReturnToStart();
                    }
                    break;
            }

            RefreshOutputs();
            wasFirstPressed = isFirstPressed;
            wasSecondPressed = isSecondPressed;
        }

        public void ResetLoopState()
        {
            ReturnToStart();
            wasFirstPressed = firstPlate != null && firstPlate.IsPressed;
            wasSecondPressed = secondPlate != null && secondPlate.IsPressed;
            RefreshOutputs();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void ReturnToStart()
        {
            Phase = SequentialPlateDoorPhase.AwaitingFirst;
        }

        private void RefreshOutputs()
        {
            SetIndicatorState(
                firstStepIndicator,
                Phase != SequentialPlateDoorPhase.AwaitingFirst);
            SetIndicatorState(secondStepIndicator, IsSatisfied);

            if (controlledDoor != null)
            {
                controlledDoor.SetOpen(IsSatisfied);
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

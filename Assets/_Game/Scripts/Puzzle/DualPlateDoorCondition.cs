using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class DualPlateDoorCondition : MonoBehaviour, ILoopResettable
    {
        [Header("Condition")]
        [SerializeField] private PressurePlate plateA;
        [SerializeField] private PressurePlate plateB;
        [SerializeField] private SlidingDoor controlledDoor;

        [Header("Readability")]
        [SerializeField] private GameObject plateAIndicator;
        [SerializeField] private GameObject plateBIndicator;

        public bool IsSatisfied => plateA != null
            && plateB != null
            && plateA.IsPressed
            && plateB.IsPressed;

        private void Awake()
        {
            if (plateA == null
                || plateB == null
                || plateA == plateB
                || controlledDoor == null
                || plateAIndicator == null
                || plateBIndicator == null)
            {
                Debug.LogError(
                    "DualPlateDoorCondition requires two distinct plates, one door, and two indicators.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            RefreshState();
        }

        public void ResetLoopState()
        {
            SetIndicatorState(plateAIndicator, false);
            SetIndicatorState(plateBIndicator, false);

            if (controlledDoor != null)
            {
                controlledDoor.SetOpen(false);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                ResetLoopState();
            }
        }

        private void RefreshState()
        {
            bool isPlateAActive = plateA.IsPressed;
            bool isPlateBActive = plateB.IsPressed;

            SetIndicatorState(plateAIndicator, isPlateAActive);
            SetIndicatorState(plateBIndicator, isPlateBActive);
            controlledDoor.SetOpen(isPlateAActive && isPlateBActive);
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

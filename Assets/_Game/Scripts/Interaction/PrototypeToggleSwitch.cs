using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Interaction
{
    public sealed class PrototypeToggleSwitch : MonoBehaviour, IInteractable, ILoopResettable
    {
        [Header("References")]
        [SerializeField] private Transform leverVisual;
        [SerializeField] private GameObject indicator;

        [Header("States")]
        [SerializeField] private bool startsOn;
        [SerializeField] private Vector3 offEulerAngles = new Vector3(-25f, 0f, 0f);
        [SerializeField] private Vector3 onEulerAngles = new Vector3(25f, 0f, 0f);

        public bool IsOn { get; private set; }

        public int InteractionCount { get; private set; }

        private void Awake()
        {
            if (leverVisual == null || indicator == null)
            {
                Debug.LogError(
                    "PrototypeToggleSwitch requires a lever visual and an indicator.",
                    this);
                enabled = false;
                return;
            }

            SetState(startsOn);
        }

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            InteractionCount++;
            SetState(!IsOn);
        }

        public void SetState(bool isOn)
        {
            IsOn = isOn;

            if (leverVisual != null)
            {
                leverVisual.localRotation = Quaternion.Euler(
                    isOn ? onEulerAngles : offEulerAngles);
            }

            if (indicator != null)
            {
                indicator.SetActive(isOn);
            }
        }

        public void ResetLoopState()
        {
            SetState(startsOn);
        }
    }
}

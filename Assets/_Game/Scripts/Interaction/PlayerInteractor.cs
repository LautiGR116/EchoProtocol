using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Interaction
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string InteractActionName = "Interact";

        [Header("References")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerInput playerInput;

        [Header("Raycast")]
        [SerializeField, Min(0f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        private InputAction interactAction;

        public float InteractionDistance => interactionDistance;

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (viewCamera == null)
            {
                viewCamera = GetComponentInChildren<Camera>();
            }

            InputActionMap playerActionMap = playerInput?.actions?.FindActionMap(PlayerActionMapName);
            interactAction = playerActionMap?.FindAction(InteractActionName);

            if (playerInput == null || viewCamera == null || interactAction == null)
            {
                Debug.LogError(
                    "PlayerInteractor requires PlayerInput, a child Camera, and the Player/Interact action.",
                    this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (interactAction.WasPressedThisFrame())
            {
                TryInteract();
            }
        }

        public bool TryInteract()
        {
            if (!TryGetTarget(out IInteractable target) || !target.CanInteract(gameObject))
            {
                return false;
            }

            target.Interact(gameObject);
            return true;
        }

        public bool TryGetTarget(out IInteractable target)
        {
            target = null;

            if (viewCamera == null)
            {
                return false;
            }

            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactionLayers,
                    triggerInteraction))
            {
                return false;
            }

            target = hit.collider.GetComponentInParent<IInteractable>();
            return target != null;
        }

        private void OnValidate()
        {
            interactionDistance = Mathf.Max(0f, interactionDistance);
        }
    }
}

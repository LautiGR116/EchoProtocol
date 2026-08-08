using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string MoveActionName = "Move";
        private const string LookActionName = "Look";

        [Header("References")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerInput playerInput;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVerticalSpeed = -2f;

        [Header("Look")]
        [SerializeField, Min(0f)] private float lookSensitivity = 0.15f;
        [SerializeField, Range(1f, 89f)] private float verticalLookLimit = 85f;

        private CharacterController characterController;
        private InputAction moveAction;
        private InputAction lookAction;
        private float pitch;
        private float verticalVelocity;
        private bool cursorIsLocked;
        private bool wantsCursorLocked;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (viewCamera == null)
            {
                viewCamera = GetComponentInChildren<Camera>();
            }

            if (playerInput.actions == null || viewCamera == null)
            {
                Debug.LogError(
                    "FirstPersonController requires a PlayerInput action asset and a child Camera.",
                    this);
                enabled = false;
                return;
            }

            InputActionMap playerActionMap = playerInput.actions.FindActionMap(PlayerActionMapName);
            moveAction = playerActionMap?.FindAction(MoveActionName);
            lookAction = playerActionMap?.FindAction(LookActionName);

            if (moveAction == null || lookAction == null)
            {
                Debug.LogError(
                    "FirstPersonController could not find Player/Move and Player/Look actions.",
                    this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (playerInput == null || moveAction == null || lookAction == null)
            {
                return;
            }

            playerInput.ActivateInput();
            SetCursorPreference(true);
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }

            wantsCursorLocked = false;
            ApplyCursorState(false);
        }

        private void Update()
        {
            UpdateCursorPreference();
            ApplyLook();
            ApplyMovement();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            ApplyCursorState(hasFocus && wantsCursorLocked);
        }

        private void ApplyMovement()
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            Vector3 horizontalVelocity = transform.right * moveInput.x
                + transform.forward * moveInput.y;

            if (horizontalVelocity.sqrMagnitude > 1f)
            {
                horizontalVelocity.Normalize();
            }

            horizontalVelocity *= moveSpeed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedVerticalSpeed;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void ApplyLook()
        {
            if (!cursorIsLocked)
            {
                return;
            }

            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            transform.Rotate(Vector3.up, lookInput.x * lookSensitivity, Space.Self);

            pitch = Mathf.Clamp(
                pitch - lookInput.y * lookSensitivity,
                -verticalLookLimit,
                verticalLookLimit);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateCursorPreference()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorPreference(false);
                return;
            }

            if (!cursorIsLocked
                && Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorPreference(true);
            }
        }

        private void SetCursorPreference(bool shouldLock)
        {
            wantsCursorLocked = shouldLock;
            ApplyCursorState(shouldLock && Application.isFocused);
        }

        private void ApplyCursorState(bool shouldLock)
        {
            cursorIsLocked = shouldLock;
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }

        public void ResetPose(Vector3 position, Quaternion rotation)
        {
            bool controllerWasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            characterController.enabled = controllerWasEnabled;

            verticalVelocity = 0f;
            pitch = 0f;
            viewCamera.transform.localRotation = Quaternion.identity;
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            lookSensitivity = Mathf.Max(0f, lookSensitivity);
            verticalLookLimit = Mathf.Clamp(verticalLookLimit, 1f, 89f);
            groundedVerticalSpeed = Mathf.Min(groundedVerticalSpeed, 0f);
            gravity = Mathf.Min(gravity, 0f);
        }
    }
}

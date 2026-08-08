using EchoProtocol.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    public sealed class LoopController : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string ResetLoopActionName = "ResetLoop";

        [Header("Player")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private Transform playerSpawn;

        [Header("Echo")]
        [SerializeField] private EchoPlayback echoPrefab;
        [SerializeField] private Transform echoContainer;

        [Header("Loop State")]
        [SerializeField] private MonoBehaviour[] resetTargets;

        private InputAction resetLoopAction;
        private ILoopResettable[] loopResetTargets;

        public int CurrentAttemptNumber { get; private set; } = 1;

        public int CompletedLoopCount { get; private set; }

        public EchoRecording LastRecording { get; private set; }

        public EchoPlayback ActiveEcho { get; private set; }

        private void Awake()
        {
            InputActionMap playerActionMap = playerInput?.actions?.FindActionMap(PlayerActionMapName);
            resetLoopAction = playerActionMap?.FindAction(ResetLoopActionName);

            if (!TryResolveResetTargets())
            {
                enabled = false;
                return;
            }

            if (playerInput == null
                || playerController == null
                || recorder == null
                || playerSpawn == null
                || echoPrefab == null
                || resetLoopAction == null)
            {
                Debug.LogError(
                    "LoopController requires player references, a spawn, an Echo prefab, and Player/ResetLoop.",
                    this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (enabled)
            {
                recorder.BeginRecording();
            }
        }

        private void Update()
        {
            if (resetLoopAction.WasPressedThisFrame() || recorder.ReachedDurationLimit)
            {
                CompleteLoop();
            }
        }

        public bool CompleteLoop()
        {
            EchoRecording completedRecording = recorder.FinishRecording();
            if (completedRecording == null)
            {
                return false;
            }

            RemoveActiveEcho();
            ResetLoopState();
            playerController.ResetPose(playerSpawn.position, playerSpawn.rotation);

            Transform parent = echoContainer == null ? transform : echoContainer;
            EchoPlayback echo = Instantiate(echoPrefab, parent);
            echo.name = $"Echo Attempt {CurrentAttemptNumber}";

            if (!echo.Initialize(completedRecording))
            {
                Destroy(echo.gameObject);
                recorder.BeginRecording();
                return false;
            }

            LastRecording = completedRecording;
            ActiveEcho = echo;
            CompletedLoopCount++;
            CurrentAttemptNumber++;
            recorder.BeginRecording();
            return true;
        }

        private void RemoveActiveEcho()
        {
            if (ActiveEcho == null)
            {
                return;
            }

            ActiveEcho.gameObject.SetActive(false);
            Destroy(ActiveEcho.gameObject);
            ActiveEcho = null;
        }

        private bool TryResolveResetTargets()
        {
            if (resetTargets == null || resetTargets.Length == 0)
            {
                loopResetTargets = System.Array.Empty<ILoopResettable>();
                return true;
            }

            loopResetTargets = new ILoopResettable[resetTargets.Length];
            for (int index = 0; index < resetTargets.Length; index++)
            {
                MonoBehaviour resetTarget = resetTargets[index];
                if (resetTarget is ILoopResettable loopResetTarget)
                {
                    loopResetTargets[index] = loopResetTarget;
                    continue;
                }

                Debug.LogError(
                    $"LoopController reset target at index {index} must implement ILoopResettable.",
                    this);
                return false;
            }

            return true;
        }

        private void ResetLoopState()
        {
            for (int index = 0; index < loopResetTargets.Length; index++)
            {
                loopResetTargets[index].ResetLoopState();
            }
        }
    }
}

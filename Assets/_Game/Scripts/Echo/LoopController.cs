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
        [SerializeField] private RecordedMachineControl[] machineControls;

        [Header("Loop State")]
        [SerializeField] private MonoBehaviour[] resetTargets;

        private InputAction resetLoopAction;
        private ILoopResettable[] loopResetTargets;
        private Vector3 recallSpawnPosition;
        private Quaternion recallSpawnRotation;

        public event System.Action LoopCompleted;

        public int CurrentAttemptNumber { get; private set; } = 1;

        public int CompletedLoopCount { get; private set; }
        public int PlaybackRestartCount { get; private set; }

        public EchoRecording LastRecording { get; private set; }

        public EchoPlayback ActiveEcho { get; private set; }

        public Vector3 RecallSpawnPosition => recallSpawnPosition;

        public Quaternion RecallSpawnRotation => recallSpawnRotation;

        public int CheckpointRevision { get; private set; }

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
                return;
            }

            recallSpawnPosition = playerSpawn.position;
            recallSpawnRotation = playerSpawn.rotation;
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
            if (playerController.MovementSuppressed) return;
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
            playerController.ResetPose(recallSpawnPosition, recallSpawnRotation);

            Transform parent = echoContainer == null ? transform : echoContainer;
            EchoPlayback echo = Instantiate(echoPrefab, parent);
            echo.name = $"Echo Attempt {CurrentAttemptNumber}";

            if (!echo.Initialize(completedRecording, machineControls))
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
            LoopCompleted?.Invoke();
            return true;
        }

        // Retry a completed take without editing any recorded sample or event.
        // Called deliberately from the encounter's failure menu.
        public bool ReplayLastRecording()
        {
            if (LastRecording == null || LastRecording.Duration <= 0) return false;
            RemoveActiveEcho(); ResetLoopState();
            playerController.ResetPose(recallSpawnPosition, recallSpawnRotation);
            var echo = Instantiate(echoPrefab, echoContainer ? echoContainer : transform);
            echo.name = "Echo / repeat same take";
            if (!echo.Initialize(LastRecording, machineControls)) { Destroy(echo.gameObject); return false; }
            ActiveEcho = echo; PlaybackRestartCount++; recorder.CancelRecording(); recorder.BeginRecording();
            LoopCompleted?.Invoke(); return true;
        }

        public bool SetRecallCheckpoint(Transform checkpoint)
        {
            if (!enabled || checkpoint == null)
            {
                return false;
            }

            recallSpawnPosition = checkpoint.position;
            recallSpawnRotation = checkpoint.rotation;
            CheckpointRevision++;

            RemoveActiveEcho();
            LastRecording = null;
            recorder.CancelRecording();
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

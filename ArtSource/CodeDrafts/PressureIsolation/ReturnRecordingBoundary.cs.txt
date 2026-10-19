using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    /// <summary>
    /// Optional end of the P3 recording area. Preserves the completed immutable take and its playback;
    /// discards only the unfinished new attempt and prevents automatic/manual RECALL on the return.
    /// A later explicit recording checkpoint owns rearming. Legacy scenes do not contain this component.
    /// </summary>
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class ReturnRecordingBoundary : MonoBehaviour
    {
        [SerializeField] private VS1HubReturnGate returnGate;
        [SerializeField] private CampaignDirector campaign;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [Tooltip("Optional physical FIN DE REGISTRO / R INACTIVO sign and pilot at the P3 exit. Include its emissive surface and light in this group.")]
        [SerializeField] private GameObject endOfRecordingSignal;

        [SerializeField] private string followupObjective = "Volvé al vestíbulo. El próximo origen está en el subsuelo.";
        private bool initialized, suspended;
        public bool HasCommitted { get; private set; }
        public bool IsSuspended => suspended && loop && loop.CheckpointRevision == CheckpointRevisionAtBoundary;
        public int SuspensionCount { get; private set; }
        public int CheckpointRevisionAtBoundary { get; private set; }
        public EchoRecording RecordingAtBoundary { get; private set; }
        public string StatusText => "FIN DE REGISTRO · R INACTIVO";
        public string Objective => followupObjective;

        private void Awake()
        {
            if (!returnGate || !campaign || !session || !player || !loop || !recorder)
            {
                Debug.LogError("ReturnRecordingBoundary requires explicit R3, campaign, session, Player, loop and recorder references.", this);
                enabled = false;
                return;
            }
            initialized = true;
            if (endOfRecordingSignal) endOfRecordingSignal.SetActive(false);
        }

        private void Update()
        {
            if (!initialized) return;
            if (HasCommitted)
            {
                if (!suspended) return;
                // PressureOrigin (or an explicit restored checkpoint) has already enabled the loop
                // and armed its own new take. Never intercept or manufacture that recording.
                if (loop.CheckpointRevision != CheckpointRevisionAtBoundary)
                {
                    suspended = false;
                    if (endOfRecordingSignal) endOfRecordingSignal.SetActive(false);
                    return;
                }
                if (!session.IsPaused)
                {
                    // Also protects a pause/resume on the exact crossing frame: GameSession runs
                    // first and may restore its previously enabled loop; the limit check runs later.
                    loop.enabled = false;
                    if (recorder.IsRecording) recorder.CancelRecording();
                }
                return;
            }
            if (session.IsPaused || !player.isActiveAndEnabled || campaign.Chapter != CampaignChapter.Protocol || !returnGate.IsOpen) return;
            SuspendAtBoundary();
        }

        private void SuspendAtBoundary()
        {
            RecordingAtBoundary = loop.LastRecording;
            CheckpointRevisionAtBoundary = loop.CheckpointRevision;
            HasCommitted = true;
            suspended = true;
            SuspensionCount++;
            loop.enabled = false;
            recorder.CancelRecording();
            if (endOfRecordingSignal) endOfRecordingSignal.SetActive(true);
            // Do not call CompleteLoop, SetRecallCheckpoint, ResetPose or touch ActiveEcho.
        }

#if UNITY_EDITOR
        /// <summary>
        /// Explicit isolated-test fixture. Bypasses only the physical R3 crossing requirement;
        /// it neither opens R3 nor creates a recording. Never evidence of a solved P3/full route.
        /// </summary>
        public bool BeginIsolatedSuspensionProbe()
        {
            if (!initialized || !isActiveAndEnabled || HasCommitted || session.IsPaused || !player.isActiveAndEnabled ||
                campaign.Chapter != CampaignChapter.Protocol || loop.LastRecording == null || loop.ActiveEcho == null) return false;
            SuspendAtBoundary();
            return true;
        }
#endif
    }
}

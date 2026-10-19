using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    public sealed class VS1RecallAnchor : MonoBehaviour, IInteractable
    {
        [SerializeField] private VS1RecoveryHubGate recoveryHub;
        [SerializeField] private LoopController loop;
        [SerializeField] private LoopActor player;
        [SerializeField] private Transform recallPoint;
        [SerializeField] private GameObject activeDisplay;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private SlidingDoor galleryAccess;
        [Header("Optional station labels")]
        [SerializeField, TextArea(1, 3)] private string neutralStationLabel = "RECALL\n[E] ANCHOR";
        [SerializeField, TextArea(1, 3)] private string activeStationLabel = "RECALL\nANCHOR ACTIVE";

        public bool IsActivated { get; private set; }

        private void Awake()
        {
            if (recoveryHub == null || loop == null || player == null
                || player.Kind != LoopActorKind.Player || recallPoint == null
                || activeDisplay == null || statusLabel == null || galleryAccess == null)
            {
                Debug.LogError("VS1RecallAnchor requires the hub, loop, actual Player, recall point, display and gallery access.", this);
                enabled = false;
                return;
            }
            activeDisplay.SetActive(false);
            statusLabel.text = neutralStationLabel;
        }

        public bool CanInteract(GameObject interactor) => isActiveAndEnabled
            && !IsActivated && recoveryHub.IsOpen && loop.isActiveAndEnabled
            && player.isActiveAndEnabled && interactor != null
            && interactor.GetComponentInParent<LoopActor>() == player;

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor) || !loop.SetRecallCheckpoint(recallPoint))
                return;

            // Same temporal boundary as the domestic checkpoint. Recording starts
            // at the real interaction pose; no invented frame or Player teleport.
            IsActivated = true;
            activeDisplay.SetActive(true);
            statusLabel.text = activeStationLabel;
            galleryAccess.SetOpen(true);
        }
        // Session-persistent; neither this anchor nor its access door is reset.
    }
}

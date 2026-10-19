using EchoProtocol.Interaction;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum FinalDoorInteractionKind { BeginReview, OpenDoor, Preserve, ReturnKey, Document }

    [DisallowMultipleComponent]
    public sealed class FinalDoorInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private FinalDoorInteractionKind kind;
        [SerializeField] private string documentTitle;
        [SerializeField, TextArea(4, 14)] private string documentBody;

        public FinalDoorInteractionKind Kind => kind;
        public FinalDoorMemory Review => review;
        public string Prompt => review ? review.GetPrompt(kind) : "";

        public bool CanInteract(GameObject interactor)
        {
            return review && review.CanUse(kind, interactor);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;
            if (kind == FinalDoorInteractionKind.Document)
                review.ReadDocument(interactor, documentTitle, documentBody);
            else review.Use(kind, interactor, transform.position);
        }
    }
}

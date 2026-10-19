using EchoProtocol.Interaction;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum HouseInteractionKind { Telephone, HallSocket, KitchenSocket, BedroomSocket, Key, ExitDoor, Document }

    [DisallowMultipleComponent]
    public sealed class HouseInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private HouseMemory house;
        [SerializeField] private HouseInteractionKind kind;
        [SerializeField] private HouseTelephone telephone;
        [SerializeField] private string documentTitle;
        [SerializeField, TextArea(4, 14)] private string documentBody;

        public HouseInteractionKind Kind => kind;
        public HouseMemory House => house;
        public string Prompt => kind == HouseInteractionKind.Telephone && telephone ? telephone.Prompt : house ? house.GetPrompt(kind) : "";

        public bool CanInteract(GameObject interactor)
        {
            return kind == HouseInteractionKind.Telephone && telephone
                ? telephone.CanUse(interactor) : house && house.CanUse(kind, interactor);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;
            if (kind == HouseInteractionKind.Telephone && telephone) telephone.Use(interactor);
            else if (kind == HouseInteractionKind.Document) house.ReadDocument(interactor, documentTitle, documentBody);
            else house.Use(kind, interactor, transform.position);
        }
    }
}

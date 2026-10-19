using EchoProtocol.Interaction;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum StationKind { Descent, PressureOrigin, ValveA, ValveB, HouseOrigin, NoteArrival, NoteWorkOrder, NoteConsent, Telephone, CircuitKitchen, CircuitBedroom, CircuitHall, GalleryOrigin, ArchiveOrigin, Preserve, ArchiveLetter, IntroLetter }
    public sealed class CampaignStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private CampaignDirector director;
        [SerializeField] private StationKind kind;
        [SerializeField] private TextMesh display;
        [SerializeField] private string caption;
        public StationKind Kind=>kind;
        public string Caption=>caption;
        public string Prompt=>kind==StationKind.Descent?"Habilitar paso al subsuelo":kind==StationKind.Telephone?"Atender":kind==StationKind.NoteArrival||kind==StationKind.NoteWorkOrder||kind==StationKind.NoteConsent||kind==StationKind.ArchiveLetter||kind==StationKind.IntroLetter?"Leer":kind==StationKind.ValveA||kind==StationKind.ValveB?"Cebar válvula":kind==StationKind.CircuitKitchen||kind==StationKind.CircuitBedroom||kind==StationKind.CircuitHall?"Comprobar circuito":kind==StationKind.Preserve?"Conservar original":"Activar estación";
        public void SetCaption(string value) { caption=value; if(display)display.text=value; }
        public bool CanInteract(GameObject who)=>director && director.CanUse(this,who);
        public void Interact(GameObject who) {if(CanInteract(who))director.Use(this);}
    }
}

using EchoProtocol.Interaction;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class TemporalRelayTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private RecombinationRelay relay;
        [SerializeField] private bool finalReceiver;
        [SerializeField] private TextMesh status;
        [SerializeField] private Renderer pilot;
        [SerializeField] private AudioSource contact;
        private MaterialPropertyBlock block;
        private float rejectedAt = -10;
        public bool FinalReceiver => finalReceiver;
        public string Prompt => finalReceiver ? "Transferir el circuito en B" : "Recibir presencia en A";
        public bool CanInteract(GameObject actor) => relay && relay.isActiveAndEnabled
            && relay.RequiresPairedTransfer && !(finalReceiver ? (relay.RequiresReturnWitness?relay.IsSecondLinkAccepted:relay.IsTransferComplete) : relay.IsAuthorized);
        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor)) return;
            if(!relay.ConfirmTransfer(finalReceiver)) rejectedAt=Time.time;
            if (contact) contact.Play();
        }
        private void Awake()
        {
            block = new MaterialPropertyBlock();
            if (!relay || !status || !pilot) { Debug.LogError("Temporal terminal needs relay, status and pilot.", this); enabled=false; }
        }
        private void LateUpdate()
        {
            bool accepted=finalReceiver?(relay.RequiresReturnWitness?relay.IsSecondLinkAccepted:relay.IsTransferComplete):relay.IsAuthorized;
            bool live=finalReceiver?relay.EchoAtB:relay.EchoAtA;
            string id=finalReceiver?"B":"A";
            status.text=accepted?(finalReceiver&&relay.RequiresReturnWitness?"B / TRANSFERIDO\nRECIBIR EL RETORNO EN A":id+" / ACEPTADO\nPASO ASEGURADO")
                :finalReceiver&&!relay.IsAuthorized?"B / SIN ENLACE\nPRIMERO CONFIRMAR A"
                :live?id+" / PRESENCIA RECIBIDA\nE / CONFIRMAR"
                :Time.time-rejectedAt<1.6f?"SIN PRESENCIA EN "+id+"\nEL ENLACE SIGUE CERRADO"
                :id+" / ESPERANDO PRESENCIA\nLECTOR AL OTRO LADO";
            Color c=accepted?new Color(.23f,.66f,.43f):live?new Color(.2f,.75f,.78f):new Color(.62f,.35f,.12f);
            pilot.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetColor("_EmissionColor",c*.4f);pilot.SetPropertyBlock(block);
        }
    }
}

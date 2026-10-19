using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class ReturnWitnessControl : MonoBehaviour, IInteractable
    {
        [SerializeField] private RecombinationRelay relay;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerInput input;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GameSession session;
        [SerializeField] private TextMesh status;
        [SerializeField] private Transform lever;
        [SerializeField] private AudioSource contact;
        InputAction action; Quaternion rest; bool holding;
        public bool IsHolding=>holding;
        public float Fraction=>relay?relay.ReturnProgress:0;
        public string Prompt=>relay && relay.IsSecondLinkAccepted?"Recibir la contraprueba en A":"Recibir el retorno · falta transferir en B";
        void Awake()
        {
            action=input?input.actions.FindActionMap("Player")?.FindAction("Interact"):null;
            if(!relay||!interactor||!player||!session||!status||!lever||action==null){Debug.LogError("Return control requires all explicit references.",this);enabled=false;return;}
            rest=lever.localRotation;player.PoseReset+=ResetGrip;
        }
        void OnDestroy(){if(player)player.PoseReset-=ResetGrip;}
        void OnDisable(){ResetGrip();}
        void OnApplicationFocus(bool focus){if(!focus)ResetGrip();}
        void ResetGrip(){holding=false;if(relay)relay.CancelReturn();}
        public bool CanInteract(GameObject actor)=>isActiveAndEnabled && relay && relay.isActiveAndEnabled && relay.RequiresReturnWitness && !relay.IsTransferComplete;
        public void Interact(GameObject actor)
        {
            if(!CanInteract(actor)||session.IsPaused)return;
            if(relay.CanReceiveReturn&&action.IsPressed())holding=true;
            if(contact)contact.Play();
        }
        void LateUpdate()
        {
            if(session.IsPaused||!player.isActiveAndEnabled||!action.IsPressed()||!ReferenceEquals(interactor.FocusedTarget,this))ResetGrip();
            if(holding)
            {
                if(!relay.CanReceiveReturn)ResetGrip();
                else if(relay.AdvanceReturn(Time.deltaTime)){holding=false;if(contact)contact.Play();}
            }
            lever.localRotation=rest*Quaternion.Euler(0,0,-50*Fraction);
            status.text=relay.IsTransferComplete?"CONTRAPRUEBA COMPLETA\nSALIDA HABILITADA"
                :!relay.IsSecondLinkAccepted?"03 / RECIBIR EL RETORNO\nDESPUÉS DE TRANSFERIR EN B"
                :!relay.EchoAtA?"03 / ESPERANDO EN A\nLA TOMA DEBE VOLVER"
                :"03 / PRESENCIA RECIBIDA\nMANTENER E · "+Mathf.RoundToInt(Fraction*100)+" %";
        }
    }
}

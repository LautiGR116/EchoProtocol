using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    // Present-world apparatus. The recorder and Echo never own these sources.
    public sealed class ContrapruebaAtmosphere : MonoBehaviour, EchoProtocol.Echo.ILoopResettable
    {
        [SerializeField] private RecombinationRelay relay;
        [SerializeField] private GameSession session;
        [SerializeField] private AudioSource ventilation, relayContact, shutterMotor;
        [SerializeField] private Transform rotor, observationShutter;
        [SerializeField] private Light[] clinicalLights, returnLights;
        [SerializeField] private Light witnessLamp;
        [SerializeField] private Renderer[] machinePilots;
        [SerializeField] private TextMesh circuitStatus;
        float[] clinicalBase,returnBase;
        float lampBase,quiet,shutterOpen=1;
        Vector3 shutterRest;
        bool transferSeen,completeSeen,wasMoving;
        MaterialPropertyBlock block;
        public float MachineLevel=>1-quiet;
        public float ShutterOpen=>shutterOpen;
        public int TransferContacts {get;private set;}
        public int CompletionContacts {get;private set;}
        public void ResetLoopState()
        {
            if(clinicalBase==null)return;
            quiet=0;shutterOpen=1;transferSeen=completeSeen=wasMoving=false;
            observationShutter.localPosition=shutterRest+Vector3.up*2.25f;
            for(int i=0;i<clinicalBase.Length;i++)clinicalLights[i].intensity=clinicalBase[i];
            for(int i=0;i<returnBase.Length;i++)returnLights[i].intensity=returnBase[i]*.38f;
            witnessLamp.intensity=lampBase*.08f;ventilation.volume=.13f;
            if(shutterMotor)shutterMotor.Stop();if(relayContact)relayContact.Stop();
        }
        void Awake()
        {
            if(!relay||!session||!ventilation||!relayContact||!shutterMotor||!rotor||!observationShutter||!witnessLamp||!circuitStatus)
            {Debug.LogError("Contraprueba apparatus needs its explicit references.",this);enabled=false;return;}
            clinicalBase=new float[clinicalLights.Length];returnBase=new float[returnLights.Length];
            for(int i=0;i<clinicalBase.Length;i++)clinicalBase[i]=clinicalLights[i].intensity;
            for(int i=0;i<returnBase.Length;i++)returnBase[i]=returnLights[i].intensity;
            lampBase=witnessLamp.intensity;shutterRest=observationShutter.localPosition;
            block=new MaterialPropertyBlock();ventilation.Play();
        }
        void Update()
        {
            if(session.IsPaused||Time.deltaTime<=0)return;
            if(!relay.IsSecondLinkAccepted){transferSeen=false;completeSeen=false;}
            if(relay.IsSecondLinkAccepted&&!transferSeen){transferSeen=true;TransferContacts++;relayContact.Play();}
            if(relay.IsTransferComplete&&!completeSeen){completeSeen=true;CompletionContacts++;relayContact.Play();}
            quiet=Mathf.MoveTowards(quiet,relay.IsSecondLinkAccepted?1:0,Time.deltaTime/2.8f);
            ventilation.volume=.13f*(1-quiet); // Subtraction, no transient gain boost.
            rotor.Rotate(Vector3.forward,225*(1-quiet)*Time.deltaTime,Space.Self);
            for(int i=0;i<clinicalBase.Length;i++)clinicalLights[i].intensity=clinicalBase[i]*Mathf.Lerp(1,.11f,quiet);
            for(int i=0;i<returnBase.Length;i++)returnLights[i].intensity=returnBase[i]*Mathf.Lerp(.38f,1,quiet);
            witnessLamp.intensity=lampBase*Mathf.Lerp(.08f,1,quiet);
            float goal=!relay.IsAuthorized||relay.IsTransferComplete?1:relay.ReturnProgress;
            float before=shutterOpen;shutterOpen=Mathf.MoveTowards(shutterOpen,goal,Time.deltaTime*.9f);
            observationShutter.localPosition=shutterRest+Vector3.up*(shutterOpen*2.25f);
            bool moving=Mathf.Abs(before-shutterOpen)>.00001f;
            if(moving&&!wasMoving)shutterMotor.Play();
            if(!moving&&wasMoving)shutterMotor.Stop();wasMoving=moving;
            foreach(var pilot in machinePilots){pilot.GetPropertyBlock(block);block.SetColor("_EmissionColor",new Color(.22f,.48f,.40f)*(1-quiet));pilot.SetPropertyBlock(block);}
            circuitStatus.text=relay.IsTransferComplete?"A  —  B  —  A\nCONTRAPRUEBA CONSERVADA\nRETORNO / ARCHIVO"
                :relay.IsSecondLinkAccepted?"A  —  B  —  [A]\nCIRCUITO TRANSFERIDO\nRECIBIR EL RETORNO"
                :relay.IsAuthorized?"A  —  [B]  —  A\nPRESENCIA ACEPTADA\nTRANSFERIR EN B"
                :"[A]  —  B  —  A\nRECIBIR · TRANSFERIR · VOLVER";
        }
        void OnDisable(){if(ventilation)ventilation.Stop();if(shutterMotor)shutterMotor.Stop();if(relayContact)relayContact.Stop();}
    }
}

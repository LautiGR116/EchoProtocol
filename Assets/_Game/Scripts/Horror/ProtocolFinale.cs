using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Horror
{
    // The environment responds to acceptance; the saved Echo recording remains untouched.
    public sealed class ProtocolFinale : MonoBehaviour, IInteractable
    {
        [SerializeField] private VS1DenialEvidence evidence;
        [SerializeField] private SD1SubtitleDisplay thought;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private SlidingDoor exitDoor;
        [SerializeField] private TextMesh terminalText;
        [SerializeField] private GameObject readyLamp;
        [SerializeField] private Light[] corridorLights;
        [SerializeField] private Renderer[] corridorPanels;
        [SerializeField] private AudioSource contactSource;
        [SerializeField] private AudioSource[] ambience;
        [SerializeField] private GameSession session;
        [SerializeField] private BoxCollider finishZone;
        private bool available,finished;
        private float elapsed,finishFade;
        private int stage;
        private float[] bedVolumes;
        public bool IsAvailable=>available;
        public bool HasCommitted {get;private set;}
        public bool IsFinished=>finished;
        public int ExtinguishedCount=>stage;
        private void Awake()
        {
            if(!evidence||!thought||!player||!loop||!recorder||!exitDoor||!terminalText||!readyLamp||!contactSource||!session||!finishZone||corridorLights.Length!=3||corridorPanels.Length!=3)
            {Debug.LogError("ProtocolFinale requires complete explicit scene references.",this);enabled=false;return;}
            readyLamp.SetActive(false);terminalText.text="SALIDA\nRegistro pendiente";
            bedVolumes=new float[ambience.Length];
        }
        public bool CanInteract(GameObject who)=>enabled&&available&&!HasCommitted&&who==player.gameObject;
        public void Interact(GameObject who)
        {
            if(!CanInteract(who))return;
            HasCommitted=true;loop.enabled=false;recorder.CancelRecording();
            // Stop D1's ambience ownership before taking our own snapshot.
            evidence.enabled=false;
            for(int i=0;i<ambience.Length;i++)bedVolumes[i]=ambience[i].volume;
            terminalText.text="REGISTRO CONSERVADO\nSalida habilitada";
            thought.Show("Sí. Me detuve ahí.",3.4f);contactSource.Play();
        }
        private void Update()
        {
            if(session.IsPaused)return;
            if(!available && evidence.HasWitnessed && !thought.IsShowing)
            {available=true;readyLamp.SetActive(true);terminalText.text="CONSERVAR REGISTRO\n[E] Finalizar sesión";thought.Show("La salida está en el vestíbulo.",4f);}
            if(!HasCommitted)return;
            elapsed+=Time.deltaTime;
            if(elapsed>=2.5f)exitDoor.SetOpen(true);
            for(int i=0;i<ambience.Length;i++)ambience[i].volume=bedVolumes[i]*Mathf.Lerp(1f,.08f,Mathf.Clamp01(elapsed/5f));
            float z=player.transform.position.z;
            if(stage<3&&z < -15.5f-stage*2.5f)
            {
                corridorLights[stage].enabled=false;
                var block=new MaterialPropertyBlock();corridorPanels[stage].GetPropertyBlock(block);block.SetColor("_EmissionColor",Color.black);corridorPanels[stage].SetPropertyBlock(block);
                contactSource.transform.position=corridorLights[stage].transform.position;contactSource.Play();stage++;
            }
            if(!finished&&finishZone.bounds.Contains(player.transform.position+Vector3.up))
            {finished=true;player.enabled=false;thought.Show("Puedo irme sin borrarlo.",3.2f);}
            if(finished)
            {
                finishFade+=Time.deltaTime;
                if(finishFade>=5f)session.Finish();
            }
        }
        private void OnGUI()
        {
            if(!finished)return;
            GUI.depth=-1100;var c=GUI.color;GUI.color=new Color(0,0,0,Mathf.Clamp01((finishFade-2.8f)/2f));
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=c;
        }
    }
}

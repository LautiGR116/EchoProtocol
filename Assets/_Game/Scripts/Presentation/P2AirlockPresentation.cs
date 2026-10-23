using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Current mechanism feedback only; the Echo supplies its actual position
    // and recorded orientation through the reader, never an authored performance.
    [DisallowMultipleComponent, DefaultExecutionOrder(150)]
    public sealed class P2AirlockPresentation : MonoBehaviour, ILoopResettable
    {
        [SerializeField] private BlindAlignmentRelay relay;
        [SerializeField] private Transform platform;
        [SerializeField] private Renderer stationPilot;
        [SerializeField] private Renderer interiorPilot;
        [SerializeField] private TextMesh stationStatus;
        [SerializeField] private TextMesh interiorStatus;
        [Header("Two-door causal display, visible from the post")]
        [SerializeField] private SlidingDoor entranceGate, exitGate;
        [SerializeField] private Transform entranceLeaf, exitLeaf, entranceMimic, exitMimic;
        [SerializeField] private Renderer entrancePilot, exitPilot, entranceDoorPilot, exitDoorPilot;
        [SerializeField] private TextMesh entranceStatus, exitStatus, entranceDoorStatus, exitDoorStatus;
        [SerializeField] private Light interiorLight;
        [SerializeField] private AudioSource stationHum;
        [SerializeField] private AudioLowPassFilter humFilter;
        [SerializeField] private AudioSource stationContact;
        [SerializeField] private AudioSource interiorContact;
        [SerializeField] private AudioClip powerOn;
        [SerializeField] private AudioClip powerOff;
        [SerializeField] private AudioClip exitReady;
        [SerializeField, Range(0f,1f)] private float humLevel = .12f;
        [SerializeField] private P2CausalFeedback causalFeedback;
        private Vector3 platformRest;
        private Vector3 entranceRest, exitRest;
        private float entranceClosedY, exitClosedY;
        private bool wasPowered, wasReady;
        private int previousStation = -1, previousInterior = -1, previousEntrance = -1, previousExit = -1;
        private MaterialPropertyBlock properties;
        public int PowerTransitions { get; private set; }
        public int ReadyCues { get; private set; }

        private void Awake()
        {
            if (!relay || !relay.RequiresPoweredClosure || !platform || !stationPilot
                || !interiorPilot || !stationStatus || !interiorStatus || !interiorLight
                || !stationHum || !stationHum.clip || !humFilter || !stationContact
                || !interiorContact || !powerOn || !powerOff || !exitReady)
            {
                Debug.LogError("P2 presentation requires its powered relay, physical indicators and current-world sources.", this);
                enabled = false;
                return;
            }
            platformRest = platform.localPosition;
            if (!entranceGate || !exitGate || !entranceLeaf || !exitLeaf || !entranceMimic || !exitMimic
                || !entrancePilot || !exitPilot || !entranceDoorPilot || !exitDoorPilot
                || !entranceStatus || !exitStatus || !entranceDoorStatus || !exitDoorStatus)
            { Debug.LogError("P2 needs its actual two doors and linked post/door indicators.", this); enabled = false; return; }
            entranceRest = entranceMimic.localPosition; exitRest = exitMimic.localPosition;
            entranceClosedY = entranceLeaf.localPosition.y; exitClosedY = exitLeaf.localPosition.y;
            properties = new MaterialPropertyBlock();
            stationHum.volume = 0f;
            stationHum.Play();
            ResetLoopState();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            bool powered = relay.IsPowered, ready = relay.IsExitAuthorized;
            if (powered != wasPowered)
            {
                stationContact.PlayOneShot(powered ? powerOn : powerOff, .65f);
                // The inside repeater belongs to the mechanism, not to the Echo.
                if (!powered && relay.IsPlayerInside) interiorContact.PlayOneShot(powerOff, .7f);
                PowerTransitions++;
            }
            if (ready && !wasReady)
            {
                (relay.IsPlayerInside ? interiorContact : stationContact).PlayOneShot(exitReady, .7f);
                ReadyCues++;
            }
            wasPowered = powered; wasReady = ready;
            platform.localPosition = Vector3.MoveTowards(platform.localPosition,
                platformRest + (powered ? Vector3.down * .055f : Vector3.zero), Time.deltaTime * .3f);
            stationHum.volume = Mathf.MoveTowards(stationHum.volume, powered ? humLevel : 0f, Time.deltaTime * .6f);
            humFilter.cutoffFrequency = Mathf.MoveTowards(humFilter.cutoffFrequency,
                relay.IsPlayerSeparated ? 850f : 16000f, Time.deltaTime * 30000f);

            int station = powered ? 1 : 0;
            // Separate a gaze-held entrance from a moving closure and an
            // occupied safety threshold. Waiting cannot resolve held gaze.
            int entrance = !powered ? 0 : relay.IsGateAuthorized ? (entranceGate.IsFullyOpen ? 2 : 1)
                : entranceGate.IsOpenRequested && !entranceGate.IsFullyClosed ? 3
                : !entranceGate.IsFullyClosed ? 4 : 5;
            int exit = !powered ? 0 : ready ? (exitGate.IsFullyOpen ? 3 : 2) : 1;
            int inside = !powered ? 0 : ready ? 2 : 1;
            if (station != previousStation)
            {
                stationStatus.text = powered ? "ALIMENTADO" : "SIN ALIMENTACIÓN";
                Pilot(stationPilot, powered ? new Color(1f,.55f,.12f) : new Color(.16f,.18f,.20f));
                previousStation = station;
            }
            if (entrance != previousEntrance || exit != previousExit)
            {
                string entryText = entrance == 0 ? "1 SIN ALIMENTACIÓN" : entrance == 1 ? "1 ABRIENDO"
                    : entrance == 2 ? "1 ABIERTA POR MIRADA" : entrance == 3 ? "1 UMBRAL OCUPADO"
                    : entrance == 4 ? "1 CERRANDO" : "1 CERRADA";
                string exitText = exit == 0 ? "2 SIN ALIMENTACIÓN" : exit == 1 ? "2 BLOQUEADA POR 1"
                    : exit == 2 ? "2 ABRIENDO" : "2 PASO LIBRE";
                entranceStatus.text = entranceDoorStatus.text = entryText;
                exitStatus.text = exitDoorStatus.text = exitText;
                interiorStatus.text = !powered ? "SIN ALIMENTACIÓN\nR · RECALL"
                    : entrance == 3 ? "1 UMBRAL OCUPADO\nDESPEJÁ LA ENTRADA"
                    : entrance == 4 ? "1 CERRANDO\n2 ESPERA CIERRE 1"
                    : ready ? "1 CERRADA\n" + (exit == 3 ? "< 2 PASO LIBRE" : "2 ABRIENDO")
                    : "1 ABIERTA POR MIRADA\n2 BLOQUEADA POR 1";
                Color amber = new Color(1f,.55f,.12f), green = new Color(.25f,.85f,.72f), red = new Color(.55f,.16f,.08f);
                Pilot(entrancePilot, !powered ? red : entrance == 5 ? green : amber);
                Pilot(exitPilot, !powered ? red : ready ? green : amber * .35f);
                Pilot(entranceDoorPilot, !powered ? red : entrance == 5 ? green : amber);
                Pilot(exitDoorPilot, !powered ? red : ready ? green : amber * .35f);
                previousEntrance = entrance; previousExit = exit;
            }
            // The miniature shutters repeat real leaf travel, including power
            // loss and safety holds, rather than illustrating a timed recipe.
            entranceMimic.localPosition = entranceRest + Vector3.up * (.17f * Mathf.Clamp01((entranceLeaf.localPosition.y - entranceClosedY) / 3.35f));
            exitMimic.localPosition = exitRest + Vector3.up * (.17f * Mathf.Clamp01((exitLeaf.localPosition.y - exitClosedY) / 3.05f));
            if (inside != previousInterior)
            {
                Color color = inside == 2 ? new Color(.25f,.85f,.72f) : inside == 1
                    ? new Color(1f,.55f,.12f) : new Color(.45f,.12f,.07f);
                Pilot(interiorPilot, color);
                interiorStatus.color = inside == 0 ? new Color(.9f,.55f,.35f) : new Color(.9f,.9f,.75f);
                // The unpowered contactor retains an independent emergency
                // practical so its RECALL instruction stays readable.
                interiorLight.color = inside == 0 ? new Color(.7f,.76f,.81f) : color;
                previousInterior = inside;
            }
            interiorLight.intensity = Mathf.MoveTowards(interiorLight.intensity,
                ready ? 1.3f : .65f, Time.deltaTime * 2f);
        }

        private void Pilot(Renderer lens, Color color)
        {
            properties ??= new MaterialPropertyBlock();
            lens.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_EmissionColor", color * 1.5f);
            lens.SetPropertyBlock(properties);
        }

        public void ResetLoopState()
        {
            wasPowered = wasReady = false;
            previousStation = previousInterior = previousEntrance = previousExit = -1;
            if (stationContact) stationContact.Stop();
            if (interiorContact) interiorContact.Stop();
            if (stationHum) stationHum.volume = 0f;
            if (humFilter) humFilter.cutoffFrequency = 16000f;
            if (platform) platform.localPosition = platformRest;
            // The existing loop participant also resets its optional physical
            // witness. No new recorder, playback or session ownership is needed.
            if (causalFeedback) causalFeedback.ResetLoopState();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (stationHum) stationHum.Stop();
            if (stationContact) stationContact.Stop();
            if (interiorContact) interiorContact.Stop();
        }
    }
}

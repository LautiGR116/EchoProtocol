using EchoProtocol.Echo;
using EchoProtocol.Audio;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    [DefaultExecutionOrder(220), DisallowMultipleComponent]
    public sealed class MachineCoverEncounter : MonoBehaviour, ILoopResettable
    {
        [SerializeField] private CampaignDirector director;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private SD1SubtitleDisplay thought;
        [SerializeField] private BoxCollider entry;
        [SerializeField] private BoxCollider exit;
        [SerializeField] private BoxCollider gateSafety;
        [SerializeField] private Transform origin;
        [SerializeField] private RecordedMachineControl machine;
        [SerializeField] private MachineCoverInteraction crank, release;
        [SerializeField] private SlidingDoor serviceGate, returnGate, exitDoor;
        [SerializeField] private SlidingDoorAudio[] gateAudio;
        [SerializeField] private MachineCoverThreat threat;
        [SerializeField] private AudioSource motor, purge, contact;
        [SerializeField] private Transform rotor, needle;
        [SerializeField] private TextMesh controlText, gateText, exitText;
        [SerializeField] private Light motorPilot, servicePilot;
        [SerializeField] private float pressureRise = .8f, pressureFall = 1.5f;
        [SerializeField, Range(0,1)] private float motorLevel=.23f, purgeLevel=.12f;
        bool ready, wasRunning;
        float taughtRun, motorEnvelope, rotorAngle;
        Quaternion needleRest, rotorRest;
        public bool Active { get; private set; }
        public bool Failed { get; private set; }
        public bool Complete { get; private set; }
        public bool RuleObserved { get; private set; }
        public bool BoltReleased { get; private set; }
        public bool ExitReleased { get; private set; }
        public bool HideEncounterHud => Active && threat && threat.PerformanceEnabled && threat.Released && !Failed && !Complete;
        public float Pressure01 { get; private set; }
        public bool Covered => motorEnvelope >= .75f;
        public bool MachineRunning => machine.Held;
        public float PresenceAmbience => threat && threat.Intruder ? threat.Intruder.AmbienceMultiplier : 1;
        public int FailureCount { get; private set; }
        public int RejectedWork { get; private set; }
        public Transform Origin => origin;
        public RecordedMachineControl Machine => machine;
        public LoopController Loop => loop;
        public EchoProtocol.Interaction.IInteractable HoldingInteraction => machine.PlayerHeld ? machine : crank.IsHolding ? crank : release.IsHolding ? release : null;
        public string ProgressStamp => RuleObserved + "/" + BoltReleased + "/" + ExitReleased + "/" + Complete;
        public string Objective => Complete ? "La compuerta quedó abierta. Cruzá y seguí hacia la casa."
            : Failed ? "Te alcanzó en el pasillo. Tu toma sigue entera."
            : ExitReleased ? "Salí por el paso lateral iluminado y cruzá la compuerta."
            : BoltReleased ? "La reja abre cuando baja la presión. La palanca de salida está del otro lado."
            : loop.ActiveEcho ? "Aflojá el cierre junto a la reja mientras la bomba tape los golpes."
            : RuleObserved ? "La bomba cubre los golpes, pero su presión cierra la reja. Prepará tu recorrido. R / RECALL."
            : "Probá el mando de la bomba. Mirá qué cambia en la reja de servicio.";
        public string StatusText => Complete ? "SALIDA ASEGURADA"
            : Covered ? "BOMBA EN MARCHA · TUS GOLPES QUEDAN CUBIERTOS"
            : Pressure01 > .15f ? "PRESIÓN BAJANDO"
            : "BOMBA DETENIDA · REJA SIN PRESIÓN";
        void Awake()
        {
            ready = director && session && player && loop && recorder && entry && exit && gateSafety && origin
                && machine && crank && release && serviceGate && returnGate && exitDoor && threat && motor && motor.clip && purge && contact
                && rotor && needle && controlText && gateText && exitText && motorPilot && servicePilot
                && gateAudio != null && gateAudio.Length == 3 && gateAudio[0] && gateAudio[1] && gateAudio[2];
            if (!ready) { Debug.LogError("Machine cover encounter has incomplete physical references.", this); enabled = false; return; }
            motor.playOnAwake = false; motor.loop = true; motor.volume = 0; motor.Stop();
            needleRest = needle.localRotation;
            rotorRest = rotor.localRotation;
        }
        public bool Enter()
        {
            if (!ready || session.IsPaused || !player.isActiveAndEnabled) return false;
            Active = true; Complete = false; Failed = false;
            loop.enabled = true;
            if (!loop.SetRecallCheckpoint(origin)) return false;
            ResetLoopState(); return true;
        }
        public void RestoreState(bool complete)
        {
            Active = true; loop.enabled = true; loop.SetRecallCheckpoint(origin); ResetLoopState();
            Complete = complete; BoltReleased = ExitReleased = complete; RuleObserved = complete;
            exitDoor.SetOpenImmediately(complete); serviceGate.SetOpenImmediately(complete); returnGate.SetOpenImmediately(complete);
            foreach(var audio in gateAudio)audio.ResetLoopState();
            if (complete) { loop.enabled = false; recorder.CancelRecording(); }
            UpdateLabels(); ApplyInstruments(complete);
        }
        public void ResetLoopState()
        {
            if (!Active || Complete) return;
            Failed = false; BoltReleased = ExitReleased = false; Pressure01 = motorEnvelope = 0;
            machine.ResetControl(); crank.ResetWork(); release.ResetWork(); threat.ResetThreat();
            motor.Stop(); motor.volume = 0; purge.Stop(); contact.Stop(); wasRunning = false;
            serviceGate.SetOpenImmediately(false); returnGate.SetOpenImmediately(false); exitDoor.SetOpenImmediately(false);
            foreach(var audio in gateAudio)audio.ResetLoopState();
            UpdateLabels(); ApplyInstruments(false);
        }
        public void AcceptWork(MachineCoverInteraction.Kind kind)
        {
            if (kind == MachineCoverInteraction.Kind.Crank)
            {
                BoltReleased = true; contact.transform.position = crank.transform.position; contact.Play();
                thought.Show("Cierre destrabado. La reja abrirá cuando baje la presión.", 4);
            }
            else
            {
                ExitReleased = true; exitDoor.SetOpen(true); returnGate.SetOpen(true); contact.transform.position = release.transform.position; contact.Play();
                threat.Release(); if(!threat.Intruder)thought.Show("Salida liberada. Se abrió el paso lateral. Seguí su luz.", 3.5f);
            }
            UpdateLabels();
        }
        public void ExplainNoCover()
        { RejectedWork++; thought.Show("Los golpes llegan al fondo. La bomba puede taparlos mientras trabajás aquí.", 4); }
        public void PrimeThreat(){if(!ExitReleased&&!Failed)threat.PrimePresence();}
        public void CatchPlayer()
        {
            if (Failed || Complete || !Active) return;
            Failed = true; FailureCount++;
            session.SetPaused(true);
        }
        public bool Retry(bool sameTake)
        {
            if (!Failed) return false;
            Failed = false; session.BeginOrResume();
            if (sameTake && loop.LastRecording != null) return loop.ReplayLastRecording();
            ResetLoopState(); loop.enabled = true; loop.SetRecallCheckpoint(origin);
            player.ResetPose(origin.position, origin.rotation); return true;
        }
        bool Inside(BoxCollider zone)
        { return new Bounds(zone.center, zone.size).Contains(zone.transform.InverseTransformPoint(player.transform.position + Vector3.up)); }
        void LateUpdate()
        {
            if (!ready || !session.HasStarted || session.IsPaused) return;
            if (!Active && director.Chapter == CampaignChapter.Descent && Inside(entry)) director.BeginMachineCover();
            if (!Active || Failed) return;
            bool running = machine.Held;
            if (running) taughtRun += Time.deltaTime;
            else if (taughtRun >= 1.5f) RuleObserved = true;
            Pressure01 = Mathf.MoveTowards(Pressure01, running ? 1 : 0, Time.deltaTime / (running ? pressureRise : pressureFall));
            motorEnvelope = Mathf.MoveTowards(motorEnvelope, running ? 1 : 0, Time.deltaTime / (running ? .18f : .3f));
            if (motorEnvelope > 0 && !motor.isPlaying) motor.Play();
            motor.volume = motorLevel * motorEnvelope;
            if (motorEnvelope == 0 && motor.isPlaying) motor.Stop();
            if (wasRunning && !running && Pressure01 > 0) purge.Play();
            purge.volume = Mathf.MoveTowards(purge.volume, !running ? purgeLevel * Pressure01 : 0, Time.deltaTime * .8f);
            if (purge.volume <= .0001f && purge.isPlaying) purge.Stop();
            wasRunning = running;
            bool requested = ExitReleased || BoltReleased && Pressure01 <= .15f;
            // A physical safety edge prevents closing the grille on a body.
            serviceGate.SetOpen(requested || (serviceGate.IsOpenRequested && Inside(gateSafety)));
            rotorAngle += 360 * motorEnvelope * Time.deltaTime;
            ApplyInstruments(requested);
            UpdateLabels();
            if (!Complete && ExitReleased && Inside(exit))
            {
                // Securing the route never truncates a recorded grip. The real
                // motor continues until the Echo releases, ends, or is explicitly
                // dismissed by the next recording checkpoint.
                Complete = true; threat.Settle();
                loop.enabled = false; recorder.CancelRecording();
                serviceGate.SetOpen(true); returnGate.SetOpen(true); exitDoor.SetOpen(true); director.CommitMachineCover();
                thought.Show("La compuerta quedó abierta. Seguí hacia la casa.", 4);
            }
        }
        void ApplyInstruments(bool passageOpen)
        {
            rotor.localRotation = rotorRest * Quaternion.AngleAxis(rotorAngle, Vector3.right);
            needle.localRotation = needleRest * Quaternion.AngleAxis(Mathf.Lerp(270, 0, Pressure01), Vector3.right);
            motorPilot.intensity = machine.Held ? 1.8f : .12f;
            servicePilot.color = passageOpen ? new Color(.42f, .82f, .61f) : new Color(.92f, .4f, .15f);
        }
        void UpdateLabels()
        {
            controlText.text = "BOMBA / MANDO\n" + (machine.Held ? "MARCHA" : "SOLTAR = DESCARGAR") + "\nEL REGISTRO CONSERVA EL MANDO";
            gateText.text = ExitReleased ? "RETÉN ACTIVADO\nPASO DE REGRESO ASEGURADO" : BoltReleased ? (Pressure01 > .15f ? "CIERRE DESTRABADO\nESPERAR DESCARGA" : "REJA SIN PRESIÓN\nPALANCA DE SALIDA AL FONDO") : "REJA DE SERVICIO\nCIERRE ATASCADO · USAR MANIVELA";
            exitText.text = ExitReleased ? "SALIDA LIBERADA\nVOLVER A LA COMPUERTA" : "SALIDA / LIBERACIÓN MANUAL";
        }
        public string Hint(int level)
        {
            if (Complete) return Objective;
            if (ExitReleased) return level==0 ? "La palanca abrió otro paso a tu izquierda. Escuchá de dónde viene la figura antes de salir."
                : "Volvé por el paso lateral iluminado, a la izquierda cuando mirás la palanca. Desde ahí, la compuerta abierta queda al fondo. La bomba atrae su atención, pero si te ve puede perseguirte.";
            if (BoltReleased) return level==0 ? "El cierre ya está destrabado. Mirá cómo baja el manómetro cuando tu Echo suelta el mando."
                : "Cruzá la reja cuando termine la descarga. La palanca para liberar la salida está al fondo del pasillo de servicio. Conservá una segunda marcha de la bomba para tu regreso.";
            if (level == 0) return "El ruido y la presión hacen cosas distintas. Mirá el manómetro y escuchá el pasillo cuando soltás el mando.";
            if (level == 1) return "Tu Echo puede mantener la bomba mientras aflojás el cierre. Después necesitás silencio para entrar, y ruido de nuevo para volver sin llamar su atención.";
            return "Grabá el mando sostenido unos 12 s, soltalo unos 7 s y sostenelo otros 20 s. Soltá y pulsá R. Aflojá la manivela junto a la reja durante la primera marcha; cruzá cuando baje la presión y liberá la salida al fondo. Durante la segunda marcha, salí por el paso lateral iluminado y cruzá la compuerta. Los tiempos se cuentan desde que llegás al mando.";
        }
    }
}

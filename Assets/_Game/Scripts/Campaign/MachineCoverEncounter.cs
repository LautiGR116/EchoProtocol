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
        // A suggested, conservative plan for this room, not a replay modifier.
        // The HUD, physical cues and focused route use these same durations.
        public const float FirstRunSeconds = 8;
        public const float QuietWindowSeconds = 5;
        public const float ReturnRunSeconds = 16;
        [SerializeField, Min(QuietWindowSeconds)] private float preparationQuietSeconds = QuietWindowSeconds;
        [SerializeField, Min(ReturnRunSeconds)] private float preparationReturnSeconds = ReturnRunSeconds;
        public float SuggestedQuietSeconds => Mathf.Max(QuietWindowSeconds, preparationQuietSeconds);
        public float SuggestedReturnSeconds => Mathf.Max(ReturnRunSeconds, preparationReturnSeconds);
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
        PreparationState preparing, savedPlan;
        struct PreparationState
        {
            public bool HasEvents, Held, Ready;
            public int Phase, Interruption;
            public float PhaseStarted, At, QuietTarget, ReturnTarget;
            public float Elapsed => Mathf.Max(0, At - PhaseStarted);
            public float Target => Phase == 0 ? FirstRunSeconds : Phase == 1 ? QuietTarget : ReturnTarget;
            public float Fraction => Ready ? 1 : !HasEvents || Phase == 0 && !Held ? 0 : Mathf.Clamp01(Elapsed / Target);
            public int Remaining => Mathf.Max(0, Mathf.CeilToInt(Target - Elapsed));
        }
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
        public bool IsPreparingTake => Active && !Complete && !Failed
            && (preparing.HasEvents || !BoltReleased && (!loop.ActiveEcho || loop.ActiveEcho.IsComplete));
        public int PreparationPhase => preparing.Phase;
        public float PreparationFraction => preparing.Fraction;
        public bool CurrentTakeReady => preparing.Ready;
        // Usable means the suggested three windows actually exist in the take.
        // It is coaching, not an unlock or a guarantee of a successful escape.
        public bool HasUsableTake => savedPlan.Ready;
        public bool CanReplayLastTake => loop && loop.LastRecording != null && loop.LastRecording.Duration > 0;
        // An ended Echo is presented as preparation, but its useful original
        // can still be deliberately replayed. Never discard a new pump plan or
        // turn this recovery into a way to escape an already released threat.
        public bool CanRestartPreparedTake => ready && Active && !Complete && !Failed && !ExitReleased && !threat.Released
            && !preparing.HasEvents && CanReplayLastTake && HasUsableTake;
        public float WorkFraction => !BoltReleased ? crank.Fraction : !ExitReleased ? release.Fraction : Complete ? 1 : 0;
        public string PreparationTitle => preparing.Ready ? "Toma preparada · RECALL"
            : preparing.Phase == 2 ? "3 · Ruido para el regreso"
            : preparing.Phase == 1 ? "2 · Soltá y dejá descargar"
            : "1 · Ruido para aflojar";
        public string PreparationDetail
        {
            get
            {
                if (preparing.Ready) return machine.PlayerHeld
                    ? "El plan ya está grabado. Soltá E y pulsá R para usarlo desde la manivela."
                    : "Pulsá R. Tu Echo hará la secuencia; vos vas a la manivela de la reja.";
                if (CanRestartPreparedTake && loop.ActiveEcho && loop.ActiveEcho.IsComplete)
                    return "Tu Echo terminó, pero la toma sirve. Esc → Reusar toma de la bomba vuelve al origen con esa misma secuencia.";
                if (!recorder || !recorder.HasRecordingStarted) return "Acercate al mando de la bomba. Caminar inicia la toma; mantené E para grabar la primera marcha.";
                if (preparing.Phase == 1) return preparing.Remaining > 0
                    ? $"Dejá E suelta: faltan {preparing.Remaining} s de pausa. El silencio permitirá cruzar la reja."
                    : "La pausa ya alcanza. Volvé a mantener E en el mando para grabar el ruido del regreso.";
                if (preparing.Phase == 2) return preparing.Remaining > 0
                    ? $"Mantené E: faltan {preparing.Remaining} s para cubrir el regreso. Podés mirar alrededor sin soltar."
                    : "El regreso ya tiene tiempo. Soltá E para cerrar el plan; después pulsá R.";
                if (preparing.Held) return preparing.Remaining > 0
                    ? $"Mantené E: faltan {preparing.Remaining} s para trabajar la manivela. Después vas a soltar."
                    : "Primera marcha suficiente. Soltá E y observá cómo baja el manómetro.";
                return preparing.Interruption == 2 ? "El tramo de regreso quedó corto. Mantené E para preparar una nueva secuencia en esta toma."
                    : preparing.Interruption == 1 ? "La marcha quedó corta. Volvé a mantener E; la barra cuenta sólo el mando que realmente grabás."
                    : "Mantené E en el mando. Dejá ruido grabado para aflojar el cierre desde el otro lado.";
            }
        }
        public string TakeStatus => !CanReplayLastTake ? "Todavía no hay una toma guardada."
            : HasUsableTake ? Failed ? "La secuencia está completa. Reintentá el recorrido con la misma toma."
                : "Toma con ruido, descarga y regreso. El Echo repite lo que grabaste."
            : !savedPlan.HasEvents ? "Esta toma no contiene el mando de la bomba. Prepará una nueva."
            : savedPlan.Interruption == 2 || savedPlan.Phase == 2 ? "El regreso quedó corto o sin soltar. Prepará una segunda marcha más larga."
            : savedPlan.Phase == 1 ? "La toma tiene la primera marcha; falta completar la pausa y el ruido del regreso."
            : savedPlan.Held && savedPlan.Elapsed >= FirstRunSeconds ? "La toma no tiene una descarga suficiente. Soltá el mando durante la fase de pausa."
            : "La primera marcha quedó corta. Prepará una nueva toma siguiendo las tres fases.";
        public string ProgressStamp => RuleObserved + "/" + PreparationPhase + "/" + CurrentTakeReady + "/" + HasUsableTake
            + "/" + BoltReleased + "/" + ExitReleased + "/" + Complete;
        public bool HasUpcomingPumpRun
        {
            get
            {
                if(!loop||!machine)return false;
                var echo=loop.ActiveEcho;
                if(!echo||echo.IsComplete||echo.Recording==null)return false;
                for(int i=0;i<echo.Recording.MachineGripCount;i++)
                {
                    var grip=echo.Recording.MachineGrip(i);
                    if(grip.Target==machine.TargetId&&grip.Held&&grip.Timestamp>echo.ElapsedTime)return true;
                }
                return false;
            }
        }
        public string EscapeInstruction
        {
            get
            {
                if(Complete)return "Compuerta asegurada. Cruzá y seguí hacia la casa.";
                if(Failed)return "Tu toma sigue entera. Podés reintentar con el mismo Echo.";
                if(!threat||!threat.Released)return "La salida está al otro lado del paso lateral iluminado.";
                if(threat.IsCapturing)return "Te alcanzó. Tu toma se conserva para el reintento.";
                if(threat.ListeningAfterPump)return "La bomba se apagó. Conservá el tabique entre él y vos y acercate a la compuerta.";
                if(threat.AttractedToMachine)return "Va hacia la bomba. Seguí por el paso lateral y cruzá la compuerta iluminada.";
                if(threat.Phase==MachineCoverThreat.Intention.Search)
                {
                    if(Covered)return "Quedate detrás del tabique: el ruido de la bomba está desviando su atención.";
                    return HasUpcomingPumpRun
                        ? "Quedate detrás del tabique. Esperá el próximo arranque que grabaste."
                        : "Tu toma ya no tiene otro arranque. Mantené distancia; R permite preparar una nueva.";
                }
                return "Entrá al paso lateral iluminado. Dejá el tabique entre él y vos.";
            }
        }
        public string Objective => Complete ? "La compuerta quedó abierta. Cruzá y seguí hacia la casa."
            : Failed ? TakeStatus
            : ExitReleased ? EscapeInstruction
            : IsPreparingTake ? PreparationDetail
            : BoltReleased ? (release.IsHolding ? "Mantené E en la palanca hasta liberar la salida. El paso lateral se abrirá a tu lado."
                : Pressure01>.15f
                ? "Cierre destrabado. Esperá que tu Echo suelte el mando; la presión debe bajar para cruzar."
                : "Cruzá la reja y buscá la palanca al fondo. Necesitás otra marcha grabada para cubrir el regreso.")
            : loop.ActiveEcho && !loop.ActiveEcho.IsComplete ? ReplayWorkInstruction
            : PreparationDetail;
        string ReplayWorkInstruction => Covered
            ? crank.Fraction > 0 ? "Seguí con E en la manivela. El avance queda guardado si soltás para mirar."
                : "La bomba tapa los golpes. Mantené E en la manivela junto a la reja. Podés soltar sin perder el avance."
            : HasUpcomingPumpRun || machine.ReplayHeld
                ? crank.Fraction > 0 ? "El cierre conserva tu avance. Esperá el próximo ruido de la bomba para seguir con E."
                    : "Tu Echo va a encender la bomba. Acercate a la manivela de la reja; esperá el ruido para mantener E."
                : "Esta toma ya no tiene otra marcha. Volvé al mando para grabar una nueva secuencia; la guía muestra cada fase.";
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
            preparing = default;
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
        // Called only by an explicit menu action. The UI can defer this call
        // until its keys are released; the recording and checkpoint stay intact.
        public bool RestartPreparedTake()
        {
            if (!CanRestartPreparedTake) return false;
            if (session.IsPaused) session.BeginOrResume();
            return loop.ReplayLastRecording();
        }
        bool Inside(BoxCollider zone)
        { return new Bounds(zone.center, zone.size).Contains(zone.transform.InverseTransformPoint(player.transform.position + Vector3.up)); }
        // Reads captured grip events directly. Looking at the pump, an animated
        // hand, or another Echo holding it never counts as the Player recording
        // this action. Pausing cannot add time: both sources use take timestamps.
        PreparationState ReadPreparation(EchoRecording take)
        {
            var state = new PreparationState { QuietTarget = SuggestedQuietSeconds, ReturnTarget = SuggestedReturnSeconds };
            bool live = take == null;
            int count = live ? recorder.CapturedMachineGripCount : take.MachineGripCount;
            state.At = live ? recorder.ElapsedTime : take.Duration;
            for (int i = 0; i < count; i++)
            {
                var grip = live ? recorder.CapturedMachineGrip(i) : take.MachineGrip(i);
                if (grip.Target != machine.TargetId || grip.Timestamp > state.At || grip.Held == state.Held) continue;
                state.HasEvents = true;
                if (!state.Ready)
                {
                    float duration = grip.Timestamp - state.PhaseStarted;
                    if (grip.Held)
                    {
                        if (state.Phase == 1 && duration >= SuggestedQuietSeconds) state.Phase = 2;
                        else
                        {
                            if (state.Phase == 1) state.Interruption = 1;
                            state.Phase = 0;
                        }
                        state.PhaseStarted = grip.Timestamp;
                    }
                    else if (state.Phase == 2 && duration >= SuggestedReturnSeconds)
                    {
                        state.Phase = 3; state.Ready = true; state.Interruption = 0;
                    }
                    else if (state.Phase == 0 && duration >= FirstRunSeconds)
                    {
                        state.Phase = 1; state.PhaseStarted = grip.Timestamp; state.Interruption = 0;
                    }
                    else
                    {
                        state.Interruption = state.Phase == 2 ? 2 : 1;
                        state.Phase = 0; state.PhaseStarted = grip.Timestamp;
                    }
                }
                state.Held = grip.Held;
            }
            return state;
        }
        void LateUpdate()
        {
            if (!ready) return;
            preparing = ReadPreparation(null);
            savedPlan = loop.LastRecording == null ? default : ReadPreparation(loop.LastRecording);
            if (!session.HasStarted || session.IsPaused) return;
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
            controlText.text = "BOMBA / MANDO\n" + (IsPreparingTake ? preparing.Ready ? "TOMA PREPARADA · R" : "PLAN " + (preparing.Phase + 1) + "/3 · " + (machine.PlayerHeld ? "GRABANDO MARCHA" : "MANDO SUELTO") : machine.Held ? "MARCHA" : "SOLTAR = DESCARGAR") + "\nE · MANTENER / SOLTAR";
            gateText.text = ExitReleased ? "RETÉN ACTIVADO\nPASO DE REGRESO ASEGURADO" : BoltReleased ? (Pressure01 > .15f ? "CIERRE DESTRABADO\nESPERAR DESCARGA" : "REJA SIN PRESIÓN\nPALANCA DE SALIDA AL FONDO")
                : crank.Fraction > 0 ? "MANIVELA · " + Mathf.RoundToInt(crank.Fraction * 100) + "%\nAVANCE RETENIDO · SEGUIR CON RUIDO" : "REJA DE SERVICIO\nCIERRE ATASCADO · USAR MANIVELA";
            exitText.text = ExitReleased ? "SALIDA LIBERADA\nVOLVER A LA COMPUERTA" : "SALIDA / LIBERACIÓN MANUAL";
        }
        public string Hint(int level)
        {
            if (Complete) return Objective;
            if (Failed) return TakeStatus;
            if (ExitReleased) return level==0 ? EscapeInstruction
                : "El paso lateral iluminado está a la izquierda cuando mirás la palanca. Usá el tabique para ocultarte. Si grabaste otra marcha de la bomba, esperá su ruido: lo atrae después de perderte de vista. Cruzá la compuerta cuando se desvíe. Si la toma terminó, podés usar R para preparar otra.";
            if (BoltReleased) return level==0 ? "El cierre ya está destrabado. Mirá cómo baja el manómetro cuando tu Echo suelta el mando."
                : "Cruzá la reja cuando termine la descarga. La palanca para liberar la salida está al fondo del pasillo de servicio. Conservá una segunda marcha de la bomba para tu regreso.";
            if (level == 0) return "El ruido y la presión hacen cosas distintas. Mirá el manómetro y escuchá el pasillo cuando soltás el mando.";
            if (level == 1) return "Tu Echo puede mantener la bomba mientras aflojás el cierre. Después necesitás silencio para entrar, y ruido de nuevo para volver sin llamar su atención.";
            return "Seguí las tres fases del mando: ruido para aflojar, pausa para cruzar y ruido para volver. La barra cuenta sólo lo que grabás. Cuando indique toma preparada, pulsá R. Trabajá la manivela con ruido; su avance se conserva si soltás. Cruzá al descargar y liberá la salida al fondo. Volvé por el paso lateral iluminado.";
        }
    }
}

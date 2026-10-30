using EchoProtocol.Horror;
using EchoProtocol.Echo;
using EchoProtocol.Campaign;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Read-only goals and notes. Observes real recording, readers and puzzle state;
    // it never advances a puzzle, extends a pause or writes an Echo recording.
    [DisallowMultipleComponent]
    public sealed class ProtocolGuidance : MonoBehaviour
    {
        [SerializeField] private ParallelObservationRelay firstRelay;
        [SerializeField] private VS1StrongEventOne firstEvent;
        [SerializeField] private VS1DomesticThresholdCheckpoint domesticCheckpoint;
        [SerializeField] private BlindAlignmentRelay alignmentRelay;
        [SerializeField] private VS1ParallelFigureEvent passage;
        [SerializeField] private VS1OmittedPassageEvent returnPassage;
        [SerializeField] private VS1RecallAnchor recoveryOrigin;
        [SerializeField] private RecombinationRelay returnRelay;

        [SerializeField] private CampaignDirector campaign;
        [SerializeField] private HouseMemory house;
        [SerializeField] private FinalDoorMemory finalReview;
        [SerializeField] private VS1HubReturnGate hubReturn;

        [Header("Read-only preparation guidance")]
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private OpticalReader firstReaderA, firstReaderB, firstReaderC;
        [SerializeField] private OpticalReader alignmentReader, returnReaderA, returnReaderB;
        private int observedAttempt = -1, observedCheckpoint = -1;
        private string observedStage = "";
        private float firstASeconds, firstBSeconds, alignmentFacingSeconds, alignmentAwaySeconds;
        private float returnASeconds, returnBSeconds, returnFinalASeconds;

        public bool HasLiveEcho => loop && loop.ActiveEcho && !loop.ActiveEcho.IsComplete;
        public bool HasEndedEcho => loop && loop.ActiveEcho && loop.ActiveEcho.IsComplete;
        public string StageTitle
        {
            get
            {
                switch (StageKey)
                {
                    case "P1": return "Cooperar con tu Echo";
                    case "P2": return "Las dos puertas";
                    case "P3": return "El circuito de retorno";
                    case "Cobertura": return "La máquina que te cubre";
                    case "Casa": return "La casa / llave de 6B";
                    case "Patio": case "6B": return "Recibir tu llegada";
                    case "Retorno": return "Volver a recepción";
                    case "Subsuelo": return "Bajar al subsuelo";
                    case "Origen": return "Un nuevo registro";
                    case "SE1": return "El pasillo de servicio";
                    default: return "Continuar el recorrido";
                }
            }
        }

        // Thresholds below are coaching suggestions, not qualification rules.
        // Only time actually spent recording in the matching real reader counts.
        public int PreparationStep => StageKey == "P1" ? (firstBSeconds >= 12 ? 2 : firstASeconds > 0 ? 1 : 0)
            : StageKey == "P2" ? (alignmentAwaySeconds >= 7 ? 2 : alignmentFacingSeconds >= 7 ? 1 : 0)
            : StageKey == "P3" ? (returnFinalASeconds >= 10 ? 3 : returnBSeconds >= 10 ? 2 : returnASeconds >= 6 ? 1 : 0) : 0;

        private void Update()
        {
            if (!loop || !recorder || Time.timeScale <= 0) return;
            string stage = StageKey;
            if (observedAttempt != loop.CurrentAttemptNumber || observedCheckpoint != loop.CheckpointRevision || observedStage != stage)
            {
                observedAttempt = loop.CurrentAttemptNumber; observedCheckpoint = loop.CheckpointRevision; observedStage = stage;
                firstASeconds = firstBSeconds = alignmentFacingSeconds = alignmentAwaySeconds = 0;
                returnASeconds = returnBSeconds = returnFinalASeconds = 0;
            }
            if (!loop.isActiveAndEnabled || !recorder.IsRecording || !recorder.HasRecordingStarted) return;
            float dt = Time.deltaTime;
            if (stage == "P1")
            {
                if (firstReaderA && firstReaderA.IsPlayerAligned) firstASeconds += dt;
                if (firstBSeconds < 12) firstBSeconds = firstASeconds > 0 && firstReaderB && firstReaderB.IsPlayerAligned ? firstBSeconds + dt : 0;
            }
            else if (stage == "P2" && alignmentReader)
            {
                if (alignmentFacingSeconds < 7) alignmentFacingSeconds = alignmentReader.IsPlayerAligned ? alignmentFacingSeconds + dt : 0;
                if (alignmentAwaySeconds < 7) alignmentAwaySeconds = alignmentReader.IsPlayerPresent && !alignmentReader.IsPlayerFacing
                    && alignmentFacingSeconds >= 7 ? alignmentAwaySeconds + dt : 0;
            }
            else if (stage == "P3")
            {
                bool atA = returnReaderA && returnReaderA.IsPlayerAligned;
                bool atB = returnReaderB && returnReaderB.IsPlayerAligned;
                if (returnASeconds < 6) returnASeconds = atA ? returnASeconds + dt : 0;
                if (returnBSeconds < 10) returnBSeconds = returnASeconds >= 6 && atB ? returnBSeconds + dt : 0;
                if (returnFinalASeconds < 10) returnFinalASeconds = returnBSeconds >= 10 && atA ? returnFinalASeconds + dt : 0;
            }
        }

        // The HUD uses one short action; H holds the fuller explanation.
        public string NextAction
        {
            get
            {
                if (!loop || !recorder) return Context;
                switch (StageKey)
                {
                    case "P1":
                        if (HasLiveEcho) return firstRelay.EchoAAuthorized
                            ? firstRelay.EchoBAuthorized ? "Mirá el lector C, del lado de servicio, mientras tu Echo mira B."
                                : "Cruzá el acceso de servicio y buscá C. Tu Echo tiene que llegar al lector B."
                            : "Esperá que tu Echo mire A. Eso abre el acceso de servicio para vos.";
                        if (PreparationStep == 2) return "Grabaste A y una pausa en B. Pulsá R: volvés al inicio y tu Echo repite el recorrido.";
                        if (PreparationStep == 1) return firstReaderB && firstReaderB.IsPlayerAligned
                            ? $"Mantené la mirada en B: faltan {Remaining(12, firstBSeconds)} s de pausa sugerida. Después R crea tu Echo."
                            : "Andá al lector B y quedate mirándolo unos 12 segundos. Esa pausa te da tiempo para llegar a C.";
                        return HasEndedEcho ? "La toma terminó. Grabá otra: mirá A y después dejá una pausa más larga mirando B."
                            : recorder.IsArmed ? "Movete hasta el lector A: eso inicia la grabación. Mirá su aro hasta que encienda."
                            : "Acercate al lector A y miralo hasta que el aro encienda. Tus movimientos ya se están grabando.";
                    case "P2":
                        if (alignmentRelay.HasTransferred) return "Cruzaste las dos puertas. Seguí hacia la estancia del fondo.";
                        if (!alignmentRelay.RequiresPoweredClosure) return alignmentRelay.IsEchoHoldingGate
                            ? "Cruzá la puerta mientras tu Echo mantenga la mirada en el lector."
                            : "Grabá una pausa mirando el lector. Pulsá R y cruzá mientras tu Echo lo mire.";
                        if (alignmentRelay.IsPlayerInside)
                            return !alignmentRelay.IsPowered ? "El puesto perdió energía. Pulsá R para volver al origen y grabá las dos pausas sin abandonar el puesto."
                                : alignmentRelay.IsGateAuthorized ? "Tu Echo todavía mira el lector: puerta 1 abierta. Esperá que retire la mirada para que abra 2."
                                : !alignmentRelay.IsEntranceClosed ? "La puerta 1 está cerrando. Cuando termine, cruzá por la puerta 2."
                                : "Puerta 1 cerrada y puesto alimentado. Cruzá por la puerta 2.";
                        if (HasLiveEcho && alignmentRelay.IsGateAuthorized) return "Tu Echo abre la puerta 1. Cruzala ahora; después esperá el cierre para que abra 2.";
                        if (HasLiveEcho && alignmentRelay.IsEchoPowering) return "La puerta 1 ya cerró. Si no entraste, necesitás grabar una pausa inicial más larga mirando el lector.";
                        if (HasLiveEcho) return "Esperá junto a la puerta 1: se abre cuando tu Echo llega al puesto y mira el lector.";
                        if (PreparationStep == 2) return "Registraste ambas pausas sin salir del puesto. Pulsá R y cruzá la puerta 1 cuando tu Echo la abra.";
                        if (PreparationStep == 1) return alignmentReader && alignmentReader.IsPlayerPresent && !alignmentReader.IsPlayerFacing
                            ? $"No salgas del puesto. Mirá a un lado: faltan {Remaining(7, alignmentAwaySeconds)} s de pausa sugerida. Después pulsá R."
                            : "Sin salir del puesto, dejá de mirar el lector y esperá 7 segundos. Ese giro cerrará 1 y permitirá abrir 2.";
                        return alignmentReader && alignmentReader.IsPlayerAligned
                            ? $"Mantené la mirada: faltan {Remaining(7, alignmentFacingSeconds)} s de pausa sugerida. Después mirá a un lado sin salir del puesto."
                            : "Parate en el puesto y mirá el lector durante 7 segundos. Después tendrás que mirar a un lado sin moverte.";
                    case "P3":
                        if (!returnRelay.RequiresPairedTransfer) return Context;
                        if (returnRelay.IsTransferComplete) return "El circuito quedó recibido. Cruzá el paso de retorno hacia el archivo.";
                        if (HasLiveEcho)
                        {
                            if (returnRelay.IsSecondLinkAccepted) return returnRelay.CanReceiveReturn
                                ? "Tu Echo volvió a A. Mantené E en el mando de retorno, junto a la primera ventana."
                                : "Volvé por el paso técnico al mando de retorno de A. Esperá allí la vuelta de tu Echo.";
                            if (returnRelay.IsAuthorized) return returnRelay.EchoAtB
                                ? "Tu Echo está en B. Pulsá E en la recepción B, del otro lado del vidrio."
                                : "Cruzá el paso técnico hasta la recepción B. Allí confirmás con E cuando llegue tu Echo.";
                            return returnRelay.EchoAtA ? "Tu Echo está en A. Pulsá E en la recepción A, al otro lado del vidrio."
                                : "Buscá la recepción A al otro lado del vidrio y esperá la llegada de tu Echo.";
                        }
                        if (!returnRelay.RequiresReturnWitness) return Context;
                        if (PreparationStep == 3) return "Grabaste A → B → A con sus pausas. Pulsá R y recibí a tu Echo desde el otro lado del vidrio.";
                        if (PreparationStep == 2) return returnReaderA && returnReaderA.IsPlayerAligned
                            ? $"Mantené la mirada en A: faltan {Remaining(10, returnFinalASeconds)} s de pausa sugerida para recibir el retorno. Después pulsá R."
                            : "Volvé al lector A y miralo durante 10 segundos. Esta última pausa permite recibir el retorno.";
                        if (PreparationStep == 1) return returnReaderB && returnReaderB.IsPlayerAligned
                            ? $"Mantené la mirada en B: faltan {Remaining(10, returnBSeconds)} s de pausa sugerida. Después volvé al lector A."
                            : "Seguí al lector B y miralo durante 10 segundos. Después vas a volver a A en esta misma toma.";
                        if (returnReaderA && returnReaderA.IsPlayerAligned)
                            return $"Mantené la mirada en A: faltan {Remaining(6, returnASeconds)} s de pausa sugerida. Después andá al lector B.";
                        return HasEndedEcho ? "La toma terminó antes de completar el circuito. Regrabá A → B → A con pausas de 6, 10 y 10 segundos."
                            : "Desde el origen, andá al lector A y miralo durante 6 segundos para empezar la toma del circuito.";
                    case "Casa":
                        if (house.HasKey) return "Tenés la llave. Abrí la puerta de salida con E; podés devolver la reserva al recibidor antes de irte.";
                        if (!house.HasAnswered) return "Buscá el teléfono que suena y atendé con E. La llave de 6B está en la casa.";
                        return house.PowerLocation == HousePowerLocation.Carried ? "Llevá la reserva al dormitorio y colocala con E para iluminar la búsqueda de la llave."
                            : house.PowerLocation == HousePowerLocation.Bedroom ? "Revisá el dormitorio iluminado. Apuntá a la llave de 6B y tomala con E."
                            : "Retirá con E la reserva de luz de su base y llevala al dormitorio para buscar la llave.";
                    case "6B": return finalReview.Objective;
                    case "Patio": return "Cruzá el patio siguiendo RECEPCIÓN. En 6B, prepará el puesto con E.";
                    case "Cobertura": return campaign.CoverEncounter.Objective;
                    case "Retorno": return "Seguí el archivo hasta recepción. Junto al paso SUBSUELO, activá la estación con E.";
                    case "Subsuelo": return campaign.Objective;
                    case "Origen": return "Entrá en la oficina del regreso. Activá el puesto ORIGEN con E para iniciar el siguiente registro.";
                    case "Estancia": return "Rodeá el vidrio cerrado por un acceso lateral y cruzá la estancia.";
                    case "SE3": return "Seguí el pasillo de regreso hacia la instalación.";
                    case "SE1": return firstEvent.Phase == VS1StrongEventOnePhase.Recovery
                        ? "Entrá por la puerta iluminada del fondo. El umbral fija tu próximo punto de regreso."
                        : "Avanzá por el pasillo de servicio hacia el extremo acristalado.";
                    default: return Context;
                }
            }
        }

        private static int Remaining(float target, float elapsed) => Mathf.Max(0, Mathf.CeilToInt(target - elapsed));

        public OpticalReader OccupiedReader
        {
            get
            {
                switch (StageKey)
                {
                    case "P1": return Present(firstReaderA) ? firstReaderA : Present(firstReaderB) ? firstReaderB : Present(firstReaderC) ? firstReaderC : null;
                    case "P2": return Present(alignmentReader) ? alignmentReader : null;
                    case "P3": return Present(returnReaderA) ? returnReaderA : Present(returnReaderB) ? returnReaderB : null;
                    default: return null;
                }
            }
        }
        private static bool Present(OpticalReader reader) => reader && reader.isActiveAndEnabled && reader.IsPlayerPresent;
        public string OpticalPrompt
        {
            get
            {
                OpticalReader reader = OccupiedReader;
                if (!reader) return "";
                if (reader == alignmentReader && alignmentRelay.RequiresPoweredClosure && !reader.HasFacingPresence)
                    return "Puesto alimentado · sin miradas al lector · la puerta 1 puede cerrar";
                string name = reader == firstReaderA || reader == returnReaderA ? "A" : reader == firstReaderB || reader == returnReaderB ? "B" : reader == firstReaderC ? "C" : "del puesto";
                return reader.IsPlayerAligned ? "Lector " + name + " · mirada detectada · mantenela"
                    : "Estás en el lector " + name + " · girá hacia el aro para activarlo";
            }
        }

        public string StageKey => campaign && campaign.CoverEncounter && campaign.Chapter==CampaignChapter.Pressure ? "Cobertura"
            : finalReview && finalReview.Active ? "6B"
            : house && house.HasExited ? "Patio" : house && house.Active ? "Casa"
            : campaign && campaign.Chapter >= CampaignChapter.Descent ? "Subsuelo"
            : hubReturn && hubReturn.IsOpen ? "Retorno" : recoveryOrigin.IsActivated ? "P3"
            : returnPassage.IsComplete ? "Origen" : passage.CurrentPhase == VS1ParallelFigureEvent.EventPhase.Complete ? "SE3"
            : passage.UsesPhysicalTraversal && passage.CurrentPhase==VS1ParallelFigureEvent.EventPhase.Traversing ? "Estancia"
            : domesticCheckpoint.IsCheckpointCommitted ? "P2" : firstRelay.IsComplete ? "SE1" : "P1";
        public bool CanOfferHint => (!campaign || !campaign.HideHudForPresence)
            && !(campaign && campaign.CoverEncounter && campaign.CoverEncounter.HideEncounterHud);
        public bool IsPuzzleStage => StageKey=="Cobertura" || StageKey=="P1" || StageKey=="P2" || StageKey=="P3" || StageKey=="Casa" || StageKey=="6B" || StageKey=="Subsuelo" || StageKey=="Retorno";
        public int RejectedAttempts => StageKey=="Cobertura" ? campaign.CoverEncounter.FailureCount+campaign.CoverEncounter.RejectedWork : StageKey=="P1" ? firstRelay.MissedWindows : StageKey=="P3" ? returnRelay.RejectedConfirmations+returnRelay.InterruptedReturns : 0;
        // Stable milestones, never timers or objective prose. Repeated states do
        // not postpone an automatic hint forever when the Player is stuck.
        public string ProgressStamp
        {
            get
            {
                string attempt = loop ? loop.CheckpointRevision + "/" + loop.CurrentAttemptNumber + "/" : "";
                string preparation = PreparationStep + "/" + HasLiveEcho + "/" + HasEndedEcho;
                switch (StageKey)
                {
                    case "Cobertura": return attempt + campaign.CoverEncounter.ProgressStamp;
                    case "P1": return attempt + preparation + "/" + firstRelay.EchoAAuthorized + "/" + firstRelay.EchoBAuthorized + "/" + firstRelay.IsComplete;
                    case "P2": return attempt + preparation + "/" + alignmentRelay.IsPlayerInside + "/" + alignmentRelay.IsPowered + "/" + alignmentRelay.IsEntranceClosed + "/" + alignmentRelay.HasTransferred;
                    case "P3": return attempt + preparation + "/" + returnRelay.IsAuthorized + "/" + returnRelay.IsSecondLinkAccepted + "/" + returnRelay.IsTransferComplete;
                    case "Casa": return house.HasAnswered + "/" + house.PowerLocation + "/" + house.HasKey + "/" + house.HasExited;
                    case "6B": return attempt + finalReview.RecordingReady + "/" + finalReview.ReceptionInProgress + "/" + finalReview.HasWitnessed + "/" + finalReview.OriginalPreserved + "/" + finalReview.KeyReturned;
                    case "SE1": return firstEvent.Phase.ToString();
                    default: return StageKey;
                }
            }
        }
        public string Hint(int level)
        {
            switch(StageKey)
            {
                case "Cobertura":return campaign.CoverEncounter.Hint(level);
                case "P1":
                    if(!firstRelay.RequiresSimultaneousPresence)return Context;
                    if(level==0)return NextAction;
                    if(level==1)return firstRelay.EchoAAuthorized
                        ? "C necesita coincidir con el Echo en B. Tu pausa grabada en B es el tiempo que tenés para llegar."
                        : "Podés registrar una lectura de A y una pausa frente a B. Después de RECALL, el lado de servicio queda para vos.";
                    return "Recorré A y miralo hasta que encienda. Andá a B, miralo y quedate unos segundos. Pulsá R. Tomá el acceso de servicio y mirá C mientras el Echo siga en B. Si llegás tarde, grabá una pausa algo mayor.";
                case "P3":
                    if(returnRelay.RequiresReturnWitness)
                    {
                        if(level==0)return NextAction;
                        if(level==1)return "Una misma toma debe detenerse en A, seguir a B y volver a A. Desde el lado de recepción, cada presencia habilita una acción distinta. Dejá tiempo para recorrer las salas.";
                        return "Desde el origen, grabá una pausa en A, otra en B y una última en A. Como punto de partida, probá 6, 10 y 10 segundos. Pulsá R. Recibí A con E, cruzá el paso técnico y transferí B con E. Volvé a A y mantené E en el mando de retorno mientras tu Echo esté allí. La salida queda asegurada; no hace falta correr después.";
                    }
                    if(!returnRelay.RequiresPairedTransfer)return Context;
                    if(level==0)return returnRelay.IsAuthorized ? "El primer enlace quedó aceptado. La siguiente ventana tiene otra recepción." : "Los lectores y sus mandos están a lados opuestos del vidrio. Mirá qué recibe cada uno.";
                    if(level==1)return "El Echo aporta la presencia. Vos confirmás con E desde el otro lado. Cada enlace aceptado conserva su apertura hasta RECALL.";
                    return "Grabá una pausa de unos segundos mirando A, caminá hasta B y grabá otra pausa mirándolo. Pulsá R, tomá el otro lado y confirmá A mientras el Echo esté allí. Avanzá y confirmá B durante su segunda pausa. La salida queda abierta; no hace falta correr.";
                case "P2":
                    if(alignmentRelay.RequiresPoweredClosure)
                    {
                        if(level==0)return NextAction;
                        if(level==1)return "Al apartar la mirada, 1 cierra. Sin una presencia en el puesto, 2 pierde alimentación.";
                        return "El Echo puede conservar la alimentación mientras vos cruzás. La toma necesita una pausa mirando el lector y otra mirando afuera, sin abandonar el puesto. R permite recuperar un intento.";
                    }
                    if(level==0)return "La señal de este lector cambia al retirar la mirada.";
                    if(level==1)return "La apertura dura lo que dura la presencia grabada. El trayecto hacia la puerta ocurre durante esa pausa.";
                    return "Mirá el lector hasta que se active y permanecé unos segundos. Pulsá R y atravesá la puerta mientras el Echo mantenga la lectura. Una toma demasiado corta puede reemplazarse.";
                case "Casa":
                    if(level==0)return NextAction;
                    if(level==1)return "La luz del dormitorio permite revisar la llave. La reserva puede retirarse de una base y llevarse a otra.";
                    return house.Objective+"\nRetirá la reserva del recibidor, colocala en el dormitorio y revisá la llave. Podés recuperar la reserva y decidir dónde dejar la luz antes de abrir la salida.";
                case "6B":
                    if(finalReview.UsesArrivalPresentation)
                    {
                        if(finalReview.HasWitnessed || finalReview.OriginalPreserved || finalReview.KeyReturned)return finalReview.Objective;
                        if(level==0)return finalReview.Objective;
                        if(level==1)return "Estás revisando tu llegada de esta sesión. Registrala afuera y recibila desde adentro.";
                        return "E prepara la recepción. Salí por el pasillo lateral, mirá la puerta hasta TOMA LISTA y pulsá R. Abrí 6B con E y esperá la llegada. Después, conservá la toma en la pantalla de portería, devolvé la llave y salí.";
                    }
                    if(level==0)return finalReview.HasWitnessed ? "El puesto de portería conserva lo que acabás de revisar." : "El origen y la llegada están a lados distintos de 6B. Sus placas muestran el estado real.";
                    if(level==1)return "La llegada necesita una pausa frente a la puerta. Después de RECALL, la recibís desde adentro abriendo 6B.";
                    return "Fijá el origen interior con E. Usá el corredor lateral para llegar afuera; mirá 6B hasta que diga PAUSA ACEPTADA. Pulsá R, abrí la puerta desde adentro y observá la llegada. Conservá el original en portería y devolvé la llave.";
                case "Subsuelo":
                    if(level==0)return "El mando de aislamiento y su indicador muestran si la instalación sigue en marcha.";
                    if(level==1)return "Este mando se mantiene presionado. Al soltarlo antes de completar, vuelve a su posición.";
                    return "Mantené E sobre el mando hasta aislar. Esperá el vaciado indicado y seguí el paso junto a la maquinaria detenida.";
                case "Retorno":
                    if(level==0)return "El archivo comunica este regreso con la recepción conocida.";
                    if(level==1)return "En recepción hay una estación junto al paso marcado SUBSUELO.";
                    return "Seguí el archivo hasta recepción, activá con E la estación SUBSUELO y cruzá el paso habilitado.";
                default:return Context;
            }
        }

        public string Context
        {
            get
            {
                if(campaign&&campaign.CoverEncounter&&campaign.Chapter==CampaignChapter.Pressure)return campaign.CoverEncounter.Objective;
                if (finalReview && finalReview.Active)
                {
                    if(finalReview.UsesArrivalPresentation)return finalReview.Objective;
                    if (finalReview.KeyReturned) return "La llave quedó en portería. El paso de salida está abierto.";
                    if (finalReview.OriginalPreserved) return "La toma ya está conservada. Portería recibe también la llave.";
                    if (finalReview.HasWitnessed) return "Observaste la llegada de la toma actual. El puesto de conservación puede aceptarla.";
                    if (finalReview.RecordingReady) return "6B aceptó tu permanencia. Esa llegada ya puede revisarse desde el origen interior.";
                    if (finalReview.ReviewedRecording != null && finalReview.QualifiedPauseCount > 0)
                        return "Estás del lado de quien recibe. La puerta tiene un mando interior.\nLa revisión necesita una vista directa de la llegada.";
                    if (finalReview.ReviewedRecording != null)
                        return "La toma no contiene una permanencia válida frente a 6B.\nLa placa y su señal confirman cuándo la lectura queda aceptada.";
                    return "El origen de la revisión está adentro; la llegada se registra afuera.\nEl corredor lateral comunica ambos lados de 6B.";
                }
                if (house && house.HasExited && finalReview && finalReview.UsesArrivalPresentation)
                    return "El patio lleva a 6B. Seguí RECEPCIÓN y prepará el puesto con E.";
                if (house && house.HasExited)
                    return "El patio comunica la casa con el vestíbulo de revisión de 6B.\nLa estación interior está identificada como ORIGEN.";
                if (house && house.Active) return house.Objective;
                if (campaign && campaign.Chapter >= CampaignChapter.Descent)
                    return campaign.Objective + "\n\n" + campaign.StatusText;
                if (hubReturn && hubReturn.IsOpen)
                    return "El registro del corredor terminó. El retorno pasa por el archivo y lleva a recepción.\nLa siguiente estación está junto al paso marcado SUBSUELO.";
                if (recoveryOrigin.IsActivated)
                {
                    if(returnRelay.RequiresReturnWitness)return returnRelay.IsTransferComplete
                        ? "La contraprueba quedó conservada. El paso de retorno conduce al archivo."
                        :returnRelay.IsSecondLinkAccepted?"B transfirió el circuito. El mando de retorno está junto a la primera ventana."
                        :returnRelay.IsAuthorized?"A recibió la presencia. El paso técnico comunica con la sala B."
                        :"Contraprueba compara una misma toma desde el otro lado. El esquema físico muestra dónde empieza y dónde termina el circuito.";
                    return returnRelay.IsAuthorized
                        ? "A ya autorizó el acceso. Observá cuánto dura la presencia en B durante la toma."
                        : "A autoriza un paso; B lo sostiene mientras haya mirada.\nEl vidrio permite reconocer el mismo recorrido desde sus dos lados.";
                }
                if (returnPassage.IsComplete)
                    return "El regreso tiene ahora un paso hacia una oficina.\nORIGEN identifica el punto donde comienza otro registro.";
                if (passage.CurrentPhase == VS1ParallelFigureEvent.EventPhase.Complete)
                    return "La estancia ya quedó recorrida. El pasillo continúa de regreso hacia la instalación.";
                if (passage.UsesPhysicalTraversal && passage.CurrentPhase == VS1ParallelFigureEvent.EventPhase.Traversing)
                    return "El vidrio del frente está cerrado. Los dos accesos laterales comunican la estancia con el pasillo.";
                if (domesticCheckpoint.IsCheckpointCommitted)
                {
                    if(alignmentRelay.RequiresPoweredClosure)
                        return alignmentRelay.HasTransferred ? "El circuito permitió el cruce. El paso sigue hacia la estancia."
                            : alignmentRelay.IsPlayerInside ? alignmentRelay.IsPowered
                                ? alignmentRelay.IsEntranceClosed ? "1 cerrada. La alimentación habilita el paso 2."
                                    : alignmentRelay.IsGateAuthorized ? "La mirada mantiene 1 abierta y bloquea 2." : "1 está cerrando. Su cierre habilita 2."
                                : "Sin alimentación. R vuelve al origen; registrá ambas pausas sin abandonar el puesto."
                            : "Probá el puesto: el lector abre 1; su cierre habilita 2 mientras conserve alimentación.";
                    return alignmentRelay.IsEchoHoldingGate
                        ? "La puerta tiene una presencia sosteniéndola. El paso sigue hacia la estancia del fondo."
                        : "La señal se apaga cuando se retira la mirada. La apertura dura lo que dura esa presencia.";
                }
                if (firstEvent.Phase == VS1StrongEventOnePhase.Recovery)
                    return "La puerta iluminada del fondo comunica con otro lugar. El umbral fija un nuevo origen.";
                if (firstRelay.IsComplete)
                    return "La cooperación quedó aceptada. Desde el paso de servicio se ve el extremo acristalado del recorrido.";
                return firstRelay.EchoAAuthorized
                    ? "El acceso reconoce la toma que está repitiéndose. El otro lado del vidrio tiene un lector propio."
                    : "Los lectores responden a tu orientación, no a E.\nSu aro cian confirma una lectura. Observá qué puerta cambia y qué se ve tras el vidrio.";
            }
        }

        private void Awake()
        {
            if (!firstRelay || !firstEvent || !domesticCheckpoint || !alignmentRelay
                || !passage || !returnPassage || !recoveryOrigin || !returnRelay)
            {
                Debug.LogError("ProtocolGuidance requires explicit first-act references.", this);
                enabled = false;
            }
        }
    }
}

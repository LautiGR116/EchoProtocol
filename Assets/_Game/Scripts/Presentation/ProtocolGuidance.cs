using EchoProtocol.Horror;
using EchoProtocol.Campaign;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Optional first-act notes. Reads the real puzzle state; never advances it.
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

        public string StageKey => campaign && campaign.CoverEncounter && campaign.Chapter==CampaignChapter.Pressure ? "Cobertura"
            : finalReview && finalReview.Active ? "6B"
            : house && house.HasExited ? "Patio" : house && house.Active ? "Casa"
            : campaign && campaign.Chapter >= CampaignChapter.Descent ? "Subsuelo"
            : hubReturn && hubReturn.IsOpen ? "Retorno" : recoveryOrigin.IsActivated ? "P3"
            : returnPassage.IsComplete ? "Origen" : passage.CurrentPhase == VS1ParallelFigureEvent.EventPhase.Complete ? "SE3"
            : passage.UsesPhysicalTraversal && passage.CurrentPhase==VS1ParallelFigureEvent.EventPhase.Traversing ? "Estancia"
            : domesticCheckpoint.IsCheckpointCommitted ? "P2" : firstRelay.IsComplete ? "SE1" : "P1";
        public bool CanOfferHint => !campaign || !campaign.HideHudForPresence;
        public bool IsPuzzleStage => StageKey=="Cobertura" || StageKey=="P1" || StageKey=="P2" || StageKey=="P3" || StageKey=="Casa" || StageKey=="6B" || StageKey=="Subsuelo" || StageKey=="Retorno";
        public int RejectedAttempts => StageKey=="Cobertura" ? campaign.CoverEncounter.FailureCount+campaign.CoverEncounter.RejectedWork : StageKey=="P1" ? firstRelay.MissedWindows : StageKey=="P3" ? returnRelay.RejectedConfirmations+returnRelay.InterruptedReturns : 0;
        public string ProgressStamp => StageKey=="Cobertura" ? campaign.CoverEncounter.ProgressStamp : StageKey=="P1" ? firstRelay.EchoAAuthorized+"/"+firstRelay.EchoBAuthorized+"/"+firstRelay.IsComplete
            : StageKey=="P3" ? returnRelay.IsAuthorized+"/"+returnRelay.IsSecondLinkAccepted+"/"+returnRelay.IsTransferComplete
            : StageKey=="6B" ? finalReview.RecordingReady+"/"+finalReview.HasWitnessed+"/"+finalReview.OriginalPreserved+"/"+finalReview.KeyReturned
            : StageKey=="Casa" ? house.Objective : Context;
        public string Hint(int level)
        {
            switch(StageKey)
            {
                case "Cobertura":return campaign.CoverEncounter.Hint(level);
                case "P1":
                    if(!firstRelay.RequiresSimultaneousPresence)return Context;
                    if(level==0)return "Compará los estados de A y B. Uno conserva la lectura; el otro necesita una presencia continua.";
                    if(level==1)return firstRelay.EchoAAuthorized
                        ? "C necesita coincidir con el Echo en B. Tu pausa grabada en B es el tiempo que tenés para llegar."
                        : "Podés registrar una lectura de A y una pausa frente a B. Después de RECALL, el lado de servicio queda para vos.";
                    return "Recorré A y miralo hasta que encienda. Andá a B, miralo y quedate unos segundos. Pulsá R. Tomá el acceso de servicio y mirá C mientras el Echo siga en B. Si llegás tarde, grabá una pausa algo mayor.";
                case "P3":
                    if(returnRelay.RequiresReturnWitness)
                    {
                        if(level==0)return returnRelay.IsSecondLinkAccepted?"La transferencia apagó las máquinas. Seguí el cable de retorno hasta la primera ventana.":"El esquema del circuito vuelve al punto de partida. Compará sus tres estaciones con los dos lugares de registro.";
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
                        if(level==0)return "Probá el puesto antes de grabar: alimenta ambas puertas. El lector abre 1; el cierre de 1 libera 2.";
                        if(level==1)return "Al apartar la mirada, 1 cierra. Sin una presencia en el puesto, 2 pierde alimentación.";
                        return "El Echo puede conservar la alimentación mientras vos cruzás. La toma necesita una pausa mirando el lector y otra mirando afuera, sin abandonar el puesto. R permite recuperar un intento.";
                    }
                    if(level==0)return "La señal de este lector cambia al retirar la mirada.";
                    if(level==1)return "La apertura dura lo que dura la presencia grabada. El trayecto hacia la puerta ocurre durante esa pausa.";
                    return "Mirá el lector hasta que se active y permanecé unos segundos. Pulsá R y atravesá la puerta mientras el Echo mantenga la lectura. Una toma demasiado corta puede reemplazarse.";
                case "Casa":
                    if(level==0)return "La reserva portátil alimenta un solo lugar. Observá qué queda disponible al moverla.";
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

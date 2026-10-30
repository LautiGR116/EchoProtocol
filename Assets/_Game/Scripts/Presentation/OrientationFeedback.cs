using EchoProtocol.Campaign;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Physical displays report existing state only. They never qualify an actor,
    // advance a puzzle, alter a take or open a door.
    [DisallowMultipleComponent]
    public sealed class OrientationFeedback : MonoBehaviour
    {
        [SerializeField] private ParallelObservationRelay relay;
        [SerializeField] private OpticalReader readerA;
        [SerializeField] private OpticalReader readerB;
        [SerializeField] private TextMesh statusA;
        [SerializeField] private TextMesh statusB;
        [SerializeField] private TextMesh statusC;
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private TextMesh interiorStatus;
        [SerializeField] private TextMesh exteriorStatus;
        [SerializeField] private HouseSwingDoor receivedDoor;
        [SerializeField] private TextMesh doorStatus;

        public string InteriorText => interiorStatus ? interiorStatus.text : "";
        public string ExteriorText => exteriorStatus ? exteriorStatus.text : "";
        public string CooperationText => statusC ? statusC.text : "";

        private void Awake()
        {
            if (!relay || !readerA || !readerB || !statusA || !statusB || !statusC ||
                !review || !interiorStatus || !exteriorStatus || !receivedDoor || !doorStatus)
            {
                Debug.LogError("OrientationFeedback requires explicit puzzle, review and physical text references.", this);
                enabled = false;
            }
        }
        private static void Write(TextMesh surface, string value)
        { if (surface.text != value) surface.text = value; }
        private static readonly Color Retained = new Color(.63f,.79f,.73f);
        private static readonly Color Sustained = new Color(.94f,.74f,.43f);
        private static readonly Color Waiting = new Color(.74f,.76f,.70f);
        private static void Signal(TextMesh surface,string value,Color color)
        {
            Write(surface,value);
            if(surface.color!=color)surface.color=color;
        }
        private void LateUpdate()
        {
            // A latches until RECALL; B requires a presence right now. Words and
            // brackets distinguish those rules without relying on hue or blinking.
            Signal(statusA, relay.EchoAAuthorized ? "[A] / ACCESO RETENIDO\nCRUZÁ A SERVICIO"
                : readerA.IsPlayerAligned ? "A / MIRADA LOCAL\nSEGUÍ HASTA B" : "A / MIRÁ EL ARO\nSE ACTIVA SIN PULSAR E",
                relay.EchoAAuthorized?Retained:Waiting);
            Signal(statusB, relay.IsComplete ? "[B] / TAREA CUMPLIDA\nCOOPERACIÓN ACEPTADA" : relay.RequiresSimultaneousPresence
                ? (relay.EchoBAuthorized ? "| B | ECHO PRESENTE\nVOS MIRÁ C" : readerB.IsPlayerAligned ? "| B | MANTENÉ MIRADA\nGRABÁ UNA PAUSA AQUÍ" : "| B | PRESENCIA\nMIRÁ Y QUEDATE AQUÍ")
                : relay.EchoBAuthorized ? "[B] / TOMA ACEPTADA"
                : readerB.IsPlayerAligned ? "B / LECTURA LOCAL" : "B / LECTOR DE MIRADA",
                relay.IsComplete?Retained:relay.RequiresSimultaneousPresence?Sustained:relay.EchoBAuthorized?Retained:Waiting);
            Signal(statusC, relay.IsComplete ? "[A + B + C]\nCOOPERACIÓN ACEPTADA"
                : relay.RequiresSimultaneousPresence && relay.EchoAAuthorized
                    ? (relay.EchoBAuthorized ? "B PRESENTE / MIRAR C\nCOINCIDENCIA " + Mathf.RoundToInt(relay.CooperationFraction*100) + "%" : "ESPERÁ A TU ECHO EN B\nDESPUÉS MIRÁ EL ARO C")
                : relay.RequiresSimultaneousPresence ? "C / TU LADO DEL VIDRIO\nECHO EN B + VOS EN C"
                : relay.EchoAAuthorized && relay.EchoBAuthorized ? "A + B ACEPTADOS\nC / LECTURA DISPONIBLE"
                : "C / ACCESO\nREQUIERE A + B EN LA TOMA",
                relay.IsComplete?Retained:relay.RequiresSimultaneousPresence?Sustained:Waiting);
            if (review.UsesArrivalPresentation)
            {
                Write(interiorStatus, review.KeyReturned ? "LLAVE RECIBIDA\nSALIDA ABIERTA"
                    : review.OriginalPreserved ? "TOMA CONSERVADA\nDEVOLVÉ LA LLAVE"
                    : review.HasWitnessed ? "LLEGADA OBSERVADA\nCONSERVÁ LA TOMA EN PORTERÍA"
                    : !review.Active ? "E / PREPARAR RECEPCIÓN"
                    : review.ReceptionInProgress ? "ABRÍ 6B\nESPERÁ TU LLEGADA"
                    : "REGISTRÁ TU LLEGADA\nSALÍ POR EL PASILLO LATERAL");
                Write(doorStatus, review.KeyReturned ? "SALIDA ABIERTA"
                    : review.OriginalPreserved ? "TOMA CONSERVADA\nDEVOLVÉ LA LLAVE"
                    : review.HasWitnessed ? "LLEGADA OBSERVADA\nCRUZÁ A PORTERÍA"
                    : review.ReceptionInProgress ? (receivedDoor.IsOpenRequested ? "ESPERÁ TU LLEGADA" : "E / ABRIR 6B\nRECIBÍ TU LLEGADA")
                    : "6B / RECEPCIÓN");
                Write(exteriorStatus, review.RecordingReady ? "TOMA LISTA\nR / VOLVER ADENTRO"
                    : "MIRÁ LA PUERTA\nESPERÁ LA SEÑAL");
                return;
            }
            Write(interiorStatus, review.OriginalPreserved ? "ORIGINAL CONSERVADO\nPORTERÍA / DEVOLUCIÓN"
                : review.HasWitnessed ? "LLEGADA OBSERVADA\nCONSERVACIÓN EN PORTERÍA"
                : !review.Active ? "ORIGEN INTERIOR\nE / FIJAR PUNTO"
                : review.ReviewedRecording != null && review.QualifiedPauseCount > 0
                    ? "RECIBIR DESDE ADENTRO\nMANDO DE PUERTA / E"
                    : "ORIGEN FIJADO\nLLEGADA / CORREDOR LATERAL");
            Write(doorStatus, !review.Active ? "ORIGEN REQUERIDO" : receivedDoor.IsOpenRequested ? "6B / ABIERTA" : "E / ABRIR 6B");
            Write(exteriorStatus, review.RecordingReady ? "PAUSA ACEPTADA\nR / REVISAR"
                : "6B / MIRADA\nMANTENER MIRADA");
        }
    }
}

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
        private void LateUpdate()
        {
            Write(statusA, relay.EchoAAuthorized ? "A / TOMA ACEPTADA\nPASO AUTORIZADO"
                : readerA.IsPlayerAligned ? "A / LECTURA LOCAL\nPASO DE PRUEBA" : "A / LECTOR DE MIRADA\nCIAN = LECTURA");
            Write(statusB, relay.RequiresSimultaneousPresence
                ? (relay.EchoBAuthorized ? "B / PRESENCIA RECIBIDA\nCOINCIDIR CON C" : readerB.IsPlayerAligned ? "B / LECTURA LOCAL\nDURA MIENTRAS MIRÁS" : "B / PRESENCIA CONTINUA\nSE APAGA AL RETIRARSE")
                : relay.EchoBAuthorized ? "B / TOMA ACEPTADA"
                : readerB.IsPlayerAligned ? "B / LECTURA LOCAL" : "B / LECTOR DE MIRADA");
            Write(statusC, relay.IsComplete ? "A + B + C\nCOOPERACIÓN ACEPTADA"
                : relay.RequiresSimultaneousPresence && relay.EchoAAuthorized
                    ? (relay.EchoBAuthorized ? "B PRESENTE / MIRAR C\nCOINCIDENCIA " + Mathf.RoundToInt(relay.CooperationFraction*100) + "%" : "A ACEPTADA / ESPERANDO B\nC REQUIERE PRESENCIA SIMULTÁNEA")
                : relay.RequiresSimultaneousPresence ? "C / ACCESO\nPRIMERO A · LUEGO B + C"
                : relay.EchoAAuthorized && relay.EchoBAuthorized ? "A + B ACEPTADOS\nC / LECTURA DISPONIBLE"
                : "C / ACCESO\nREQUIERE A + B EN LA TOMA");
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

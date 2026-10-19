using EchoProtocol.Campaign;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Read-only presentation of the actual session take. No recording is copied,
    // qualified, restored or preserved here; the existing review owns those acts.
    [DisallowMultipleComponent]
    public sealed class FinalRecordDisplay : MonoBehaviour
    {
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private TextMesh heading;
        [SerializeField] private TextMesh detail;
        [SerializeField] private TextMesh action;
        public string VisibleHeading => heading ? heading.text : "";
        private string lastHeading, lastDetail, lastAction;

        private void Awake()
        {
            if (!review || !heading || !detail || !action)
            {
                Debug.LogError("FinalRecordDisplay requires the real review and three authored text surfaces.", this);
                enabled = false;
            }
        }
        private void LateUpdate()
        {
            var take = review.OriginalPreserved ? review.PreservedOriginal : review.WitnessedRecording;
            string title = review.OriginalPreserved ? "ORIGINAL CONSERVADO" : review.HasWitnessed ? "TOMA OBSERVADA" : "SIN TOMA OBSERVADA";
            string body = take != null ? $"{take.Duration:0.0} s   /   SESIÓN ACTUAL\nPOSICIÓN Y ORIENTACIÓN" : "REVISIÓN / 6B\nREGISTRO DE SESIÓN";
            string instruction = review.KeyReturned ? "LLAVE RECIBIDA" : review.OriginalPreserved ? "DEVOLVÉ LA LLAVE" : review.HasWitnessed ? "E  CONSERVAR ORIGINAL" : "OBSERVÁ DESDE 6B";
            if (review.RequiresDirectEncounter)
            {
                if (!review.OriginalPreserved) title = review.HasWitnessed ? "TOMA ACTUAL" : "REGISTRO DE SESIÓN";
                if (take == null) body = "6B / POSICIÓN Y ORIENTACIÓN";
                if (!review.OriginalPreserved && !review.HasWitnessed) instruction = "";
            }
            if (lastHeading != title) heading.text = lastHeading = title;
            if (lastDetail != body) detail.text = lastDetail = body;
            if (lastAction != instruction) action.text = lastAction = instruction;
        }
    }
}

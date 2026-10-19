using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class RecombinationRelay : MonoBehaviour, ILoopResettable
    {
        [SerializeField] private OpticalReader readerA;
        [SerializeField] private OpticalReader readerB;
        [SerializeField] private LoopController loop;
        [SerializeField] private SlidingDoor routeGate;
        [SerializeField] private SlidingDoor finalGate;
        [SerializeField] private GameObject authorizationIndicator;
        [Header("Paired transfer")]
        [SerializeField] private bool requirePairedTransfer;
        [SerializeField] private bool requireReturnWitness;
        [SerializeField, Min(.5f)] private float returnHoldSeconds = 1.25f;
        public bool RequiresPairedTransfer => requirePairedTransfer;
        public bool RequiresReturnWitness => requireReturnWitness;
        public bool IsSecondLinkAccepted { get; private set; }
        public float ReturnProgress { get; private set; }
        public int InterruptedReturns { get; private set; }
        public bool CanReceiveReturn => requireReturnWitness && IsSecondLinkAccepted && EchoAtA;
        public bool IsTransferComplete { get; private set; }
        public int RejectedConfirmations { get; private set; }
        public int AcceptedConfirmations { get; private set; }
        public bool EchoAtA => readerA && readerA.IsEchoAligned;
        public bool EchoAtB => readerB && readerB.IsEchoAligned;
        public bool CanCrossExit => requirePairedTransfer ? IsTransferComplete : IsEchoHoldingGate;
        public string LastResponse { get; private set; } = "";
        public float ResponseTime { get; private set; }

        public bool IsAuthorized { get; private set; }
        public bool IsEchoHoldingGate => IsAuthorized && readerB != null
            && readerB.IsEchoAligned;

        private void Awake()
        {
            if (loop == null || readerA == null || readerB == null || readerA == readerB
                || routeGate == null || finalGate == null || routeGate == finalGate
                || authorizationIndicator == null)
            {
                Debug.LogError("RecombinationRelay requires two distinct readers, two distinct gates and its authorization indicator.", this);
                enabled = false;
                return;
            }
            ResetLoopState();
        }

        private void FixedUpdate()
        {
            if (requirePairedTransfer) { RefreshOutputs(); return; }
            if (loop.ActiveEcho == null || !loop.ActiveEcho.gameObject.activeInHierarchy)
                IsAuthorized = false;
            else if (readerA.IsEchoAligned)
                IsAuthorized = true;
            RefreshOutputs();
        }

        private void RefreshOutputs()
        {
            authorizationIndicator.SetActive(IsAuthorized);
            // Retain P1's temporary Player preview; only Echo A latches.
            routeGate.SetOpen(IsAuthorized || readerA.IsPlayerAligned);
            // B is a live hold, never a latch. A Player can preview B from the
            // sealed recording lane but cannot reach either gated route from it.
            finalGate.SetOpen((requirePairedTransfer ? IsTransferComplete : IsEchoHoldingGate) || readerB.IsPlayerAligned);
        }

        // The present Player confirms a live optical reading. The Echo supplies
        // only recorded position/yaw; no E press or terminal action is replayed.
        public bool ConfirmTransfer(bool final)
        {
            if (!requirePairedTransfer || !isActiveAndEnabled) return false;
            bool already = final ? (requireReturnWitness ? IsSecondLinkAccepted : IsTransferComplete) : IsAuthorized;
            bool accepted = !already && (final ? IsAuthorized && EchoAtB : EchoAtA);
            if (accepted)
            {
                if (final) { IsSecondLinkAccepted=true; IsTransferComplete=!requireReturnWitness; } else IsAuthorized = true;
                AcceptedConfirmations++;
                LastResponse = final ? (requireReturnWitness ? "TRANSFERENCIA ACEPTADA · RECIBIR EL RETORNO EN A" : "TRANSFERENCIA ACEPTADA · SALIDA ASEGURADA") : "ENLACE ACEPTADO · ACCESO ASEGURADO";
            }
            else if (!already)
            {
                RejectedConfirmations++;
                LastResponse = final && !IsAuthorized ? "FALTA CONFIRMAR EL PRIMER ENLACE"
                    : "SIN PRESENCIA EN EL LECTOR " + (final ? "B" : "A");
            }
            ResponseTime = Time.time;
            RefreshOutputs();
            return accepted;
        }

        // Present-Player work only. B cannot coincide with A; accepting B first
        // proves the recorded body actually left A before this return reading.
        public bool AdvanceReturn(float seconds)
        {
            if(!isActiveAndEnabled || !CanReceiveReturn || IsTransferComplete || seconds<=0) { CancelReturn(); return false; }
            ReturnProgress=Mathf.Min(1,ReturnProgress+seconds/returnHoldSeconds);
            if(ReturnProgress<1)return false;
            IsTransferComplete=true;AcceptedConfirmations++;
            LastResponse="CONTRAPRUEBA COMPLETA · RETORNO HABILITADO";ResponseTime=Time.time;
            RefreshOutputs();return true;
        }
        public void CancelReturn()
        {
            if(IsTransferComplete)return;
            if(ReturnProgress>0)InterruptedReturns++;
            ReturnProgress=0;
        }

        public void ResetLoopState()
        {
            IsAuthorized = false;
            IsTransferComplete = false;
            IsSecondLinkAccepted = false;
            ReturnProgress = 0;
            LastResponse = "";
            if (authorizationIndicator != null) authorizationIndicator.SetActive(false);
            if (routeGate != null) routeGate.SetOpen(false);
            if (finalGate != null) finalGate.SetOpen(false);
        }

        private void OnDisable()
        {
            if (Application.isPlaying) ResetLoopState();
        }
    }
}

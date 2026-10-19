using System;
using EchoProtocol.Audio;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Campaign
{
    // Opt-in service isolation. Mechanical motion belongs to the present Player;
    // this component never commits, fabricates, copies or edits an Echo recording.
    [DefaultExecutionOrder(-90), DisallowMultipleComponent]
    public sealed class PressureIsolation : MonoBehaviour, IInteractable
    {
        [Header("Explicit chapter and input references")]
        [SerializeField] private CampaignDirector director;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera view;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private BoxCollider entryZone;

        [Header("Physical control and canonical continuation pose")]
        [SerializeField] private Transform manualControl;
        [SerializeField] private Transform gripPoint;
        [SerializeField] private Transform continuationOrigin;
        [SerializeField] private Vector3 controlLocalAxis = Vector3.forward;
        [SerializeField] private float controlTravelDegrees = 70f;
        [SerializeField, Min(.5f)] private float turnSeconds = 1.8f;
        [SerializeField, Min(.1f)] private float returnSeconds = .45f;
        [SerializeField, Min(.5f)] private float gripReach = 1.65f;
        [SerializeField] private LayerMask gripSightMask = ~0;

        [Header("Actual pressure instruments and passage")]
        [SerializeField] private Transform[] pressureNeedles = new Transform[0];
        [SerializeField] private Vector3 needleLocalAxis = Vector3.forward;
        [SerializeField] private float lowNeedleAngle = -105f;
        [SerializeField] private float highNeedleAngle = 105f;
        [SerializeField, Min(1f)] private float drainSeconds = 3f;
        [SerializeField, Min(.1f)] private float motorStopSeconds = 2.2f;
        [SerializeField] private SlidingDoor exitDoor;
        [SerializeField] private SlidingDoorAudio exitDoorAudio;

        [Header("Only the service work circuit; never emergency or domestic light")]
        [SerializeField] private Light[] workLights = new Light[0];
        [SerializeField] private Renderer[] workLampEmissions = new Renderer[0];

        private InputAction interact;
        private bool ready, holding, requireRelease, ownsRecordingSuspension;
        private int suspensionRevision;
        private float handleFraction, drainElapsed;
        private Quaternion initialControlRotation;
        private Quaternion[] initialNeedleRotations;
        private bool[] initialLightStates;
        private Color[] initialEmission;
        private MaterialPropertyBlock emissionBlock;
        private readonly RaycastHit[] gripHits = new RaycastHit[32];

        public bool Active { get; private set; }
        public bool IsHolding => holding;
        public bool IsLatched { get; private set; }
        public bool IsDrained { get; private set; }
        public float HandleFraction => handleFraction;
        public float Pressure01 => !IsLatched ? 1f : IsDrained ? 0f : Mathf.Clamp01(1f - drainElapsed / drainSeconds);
        public float MotorLevel01 => !IsLatched ? 1f : IsDrained ? 0f : Mathf.Clamp01(1f - drainElapsed / motorStopSeconds);
        public int CommitCount { get; private set; }
        public int DrainCompletionCount { get; private set; }
        public int RestoreRevision { get; private set; }
        public Transform ContinuationOrigin => continuationOrigin;
        public string Prompt => "Mantener para aislar la instalación";
        public string Objective => IsDrained ? "Cruzá la maquinaria. La compuerta del fondo quedó liberada."
            : IsLatched ? "La presión está cayendo. Podés soltar el mando y avanzar."
            : "El mando de aislamiento está junto a la primera bomba.";
        public string StatusText => IsDrained ? "INSTALACIÓN AISLADA · REGISTRO SUSPENDIDO"
            : IsLatched ? "DESPRESURIZANDO" : "INSTALACIÓN EN MARCHA · REGISTRO SUSPENDIDO";
        public event Action IsolationCommitted;
        public event Action PressureDrained;

        private void Awake()
        {
            interact = playerInput ? playerInput.actions?.FindActionMap("Player")?.FindAction("Interact") : null;
            if (!director || !session || !player || !view || !loop || !recorder || !entryZone ||
                !manualControl || !gripPoint || !continuationOrigin || !exitDoor || interact == null)
            {
                Debug.LogError("PressureIsolation requires explicit chapter, Player input, spatial, control and door references.", this);
                enabled = false;
                return;
            }
            initialControlRotation = manualControl.localRotation;
            initialNeedleRotations = new Quaternion[pressureNeedles.Length];
            for (int i = 0; i < pressureNeedles.Length; i++)
                if (pressureNeedles[i]) initialNeedleRotations[i] = pressureNeedles[i].localRotation;
            initialLightStates = new bool[workLights.Length];
            for (int i = 0; i < workLights.Length; i++) initialLightStates[i] = workLights[i] && workLights[i].enabled;
            initialEmission = new Color[workLampEmissions.Length];
            for (int i = 0; i < workLampEmissions.Length; i++)
                if (workLampEmissions[i] && workLampEmissions[i].sharedMaterial &&
                    workLampEmissions[i].sharedMaterial.HasProperty("_EmissionColor"))
                    initialEmission[i] = workLampEmissions[i].sharedMaterial.GetColor("_EmissionColor");
            emissionBlock = new MaterialPropertyBlock();
            ready = true;
            ApplyInstruments();
        }

        public bool EnterService()
        {
            if (!ready || !isActiveAndEnabled || session.IsPaused || !player.isActiveAndEnabled) return false;
            if (Active) return true;
            Active = true;
            CaptureSuspension();
            holding = false;
            requireRelease = interact.IsPressed();
            exitDoor.SetOpen(false);
            ApplyWorkCircuit();
            return true;
        }

        // Called only by the chapter checkpoint owner. A saved detent restores
        // the settled result, never an arbitrary half-turn or half-drained tank.
        public bool RestoreState(bool latched)
        {
            if (!ready || !isActiveAndEnabled) return false;
            Active = true;
            IsLatched = IsDrained = latched;
            holding = false;
            requireRelease = true;
            handleFraction = latched ? 1f : 0f;
            drainElapsed = latched ? drainSeconds : 0f;
            CaptureSuspension();
            ApplyWorkCircuit();
            ApplyInstruments();
            exitDoor.SetOpenImmediately(latched);
            if (exitDoorAudio) exitDoorAudio.ResetLoopState();
            RestoreRevision++;
            // No commit/drain events, counter increments, latch or purge cues.
            return true;
        }

        private void CaptureSuspension()
        {
            suspensionRevision = loop.CheckpointRevision;
            ownsRecordingSuspension = true;
            loop.enabled = false;
            if (recorder.IsRecording) recorder.CancelRecording();
        }

        private void Update()
        {
            if (!ready) return;
            // Also handles a fresh Continue, where R3's runtime boundary has
            // never fired. Runs after GameSession and before LoopController.
            if (ownsRecordingSuspension)
            {
                if (loop.CheckpointRevision != suspensionRevision) ownsRecordingSuspension = false;
                else if (!session.IsPaused)
                {
                    loop.enabled = false;
                    if (recorder.IsRecording) recorder.CancelRecording();
                }
            }
            if (!Active && !session.IsPaused && player.isActiveAndEnabled &&
                director.Chapter == CampaignChapter.Descent && InsideEntry())
                director.BeginPressureIsolation();
        }

        private void LateUpdate()
        {
            if (!ready || !Active) return;
            // A document can open in PlayerInteractor.Update. Decide whether
            // to move/commit only after that Update has had a chance to pause.
            if (session.IsPaused || !player.isActiveAndEnabled)
            {
                holding = false;
                requireRelease = true;
                return;
            }
            if (!interact.IsPressed()) requireRelease = false;
            if (IsLatched)
            {
                if (!IsDrained)
                {
                    drainElapsed = Mathf.Min(drainSeconds, drainElapsed + Time.deltaTime);
                    if (drainElapsed >= drainSeconds)
                    {
                        IsDrained = true;
                        exitDoor.SetOpen(true);
                        DrainCompletionCount++;
                        PressureDrained?.Invoke();
                    }
                }
            }
            else
            {
                if (holding && (requireRelease || !interact.IsPressed() || interact.WasReleasedThisFrame() ||
                    director.Chapter != CampaignChapter.Pressure || !CanReachGrip())) holding = false;
                handleFraction = Mathf.MoveTowards(handleFraction, holding ? 1f : 0f,
                    Time.deltaTime / (holding ? turnSeconds : returnSeconds));
                if (holding && handleFraction >= 1f) CommitIsolation();
            }
            ApplyInstruments();
        }

        public bool CanInteract(GameObject who) => ready && isActiveAndEnabled && Active && !IsLatched &&
            !holding && handleFraction <= .001f && !requireRelease && !session.IsPaused &&
            player.isActiveAndEnabled && who == player.gameObject && director.Chapter == CampaignChapter.Pressure && CanReachGrip();

        public void Interact(GameObject who)
        {
            if (CanInteract(who) && interact.WasPressedThisFrame() && interact.IsPressed()) holding = true;
        }

        private bool CanReachGrip()
        {
            Vector3 direction = gripPoint.position - view.transform.position;
            float distance = direction.magnitude;
            if (distance > gripReach || distance < .001f) return false;
            // The ray tests physical reach to the hand contact, not camera
            // forward: looking over a shoulder does not cancel a held lever.
            int count = Physics.RaycastNonAlloc(view.transform.position, direction / distance, gripHits,
                distance, gripSightMask, QueryTriggerInteraction.Ignore);
            if (count == gripHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var hit = gripHits[i].transform;
                if (hit && !hit.IsChildOf(transform) && !hit.IsChildOf(player.transform)) return false;
            }
            return true;
        }

        private bool InsideEntry()
        {
            Vector3 p = entryZone.transform.InverseTransformPoint(player.transform.position + Vector3.up) - entryZone.center;
            Vector3 h = entryZone.size * .5f;
            return Mathf.Abs(p.x) <= h.x && Mathf.Abs(p.y) <= h.y && Mathf.Abs(p.z) <= h.z;
        }

        private void CommitIsolation()
        {
            IsLatched = true;
            holding = false;
            handleFraction = 1f;
            drainElapsed = 0f;
            ApplyWorkCircuit();
            CommitCount++;
            director.CommitPressureIsolation();
            IsolationCommitted?.Invoke();
        }

        private void ApplyWorkCircuit()
        {
            for (int i = 0; i < workLights.Length; i++)
                if (workLights[i]) workLights[i].enabled = !IsLatched && initialLightStates[i];
            for (int i = 0; i < workLampEmissions.Length; i++)
            {
                var surface = workLampEmissions[i];
                if (!surface) continue;
                surface.GetPropertyBlock(emissionBlock);
                emissionBlock.SetColor("_EmissionColor", IsLatched ? Color.black : initialEmission[i]);
                surface.SetPropertyBlock(emissionBlock);
            }
        }

        private void ApplyInstruments()
        {
            manualControl.localRotation = initialControlRotation * Quaternion.AngleAxis(controlTravelDegrees * handleFraction, controlLocalAxis.normalized);
            for (int i = 0; i < pressureNeedles.Length; i++)
                if (pressureNeedles[i]) pressureNeedles[i].localRotation = initialNeedleRotations[i] *
                    Quaternion.AngleAxis(Mathf.Lerp(lowNeedleAngle, highNeedleAngle, Pressure01), needleLocalAxis.normalized);
        }

        private void OnDisable() { holding = false; requireRelease = true; }
        private void OnValidate()
        {
            turnSeconds = Mathf.Max(.5f, turnSeconds); returnSeconds = Mathf.Max(.1f, returnSeconds);
            gripReach = Mathf.Max(.5f, gripReach); drainSeconds = Mathf.Max(1f, drainSeconds);
            motorStopSeconds = Mathf.Clamp(motorStopSeconds, .1f, drainSeconds);
            if (controlLocalAxis.sqrMagnitude < .01f) controlLocalAxis = Vector3.forward;
            if (needleLocalAxis.sqrMagnitude < .01f) needleLocalAxis = Vector3.forward;
        }
    }
}

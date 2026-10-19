using System.Collections.Generic;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    /// <summary>
    /// Reviews an immutable, actually recorded approach from the other side of a domestic door.
    /// Door opening and returning the key are present-Player actions, never new Echo channels.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FinalDoorMemory : MonoBehaviour
    {
        [Header("Actual Player, chapter and immutable recording")]
        [SerializeField] private HouseMemory house;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera view;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;

        [Header("Authored spatial contract")]
        [SerializeField] private Transform origin;
        [SerializeField] private BoxCollider outsideZone;
        [SerializeField] private Transform outsideFacingTransform;
        [SerializeField] private BoxCollider insideWitnessZone;
        [SerializeField] private HouseSwingDoor domesticDoor;
        [Tooltip("Only genuinely transparent surfaces. Never include the opaque domestic door leaf.")]
        [SerializeField] private Collider[] transparentSightColliders = new Collider[0];
        [SerializeField] private LayerMask sightMask = ~0;

        [Header("Optional direct encounter; legacy scenes leave this disabled")]
        [SerializeField] private bool requireDirectEncounter = false;
        [Tooltip("The visible RecordedPresence color material, never its ColorMask0 depth material. Shared by the spawned Echo prefab.")]
        [SerializeField] private Material directWitnessMaterial;
        [SerializeField, Min(.1f)] private float headWitnessHeight = 1.64f;

        [Header("Physical completion")]
        [SerializeField] private GameObject terminalPilot;
        [SerializeField] private GameObject recordingPilot;
        [SerializeField] private SlidingDoor exitDoor;
        [SerializeField] private BoxCollider departureZone;
        [SerializeField] private GameObject keyInTray;

        [Header("Evidence thresholds")]
        [SerializeField, Min(2f)] private float minimumPause = 2f;
        [SerializeField, Range(0f, .12f)] private float maximumStillSpeed = .12f;
        [SerializeField, Range(1f, 35f)] private float facingTolerance = 35f;
        [SerializeField, Min(.6f)] private float witnessSeconds = .6f;
        [SerializeField, Min(5f)] private float fadeSeconds = 5f;
        [SerializeField, Min(.1f)] private float bodyWitnessHeight = 1.3f;

        [Header("Optional current-world interaction sounds")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip enterClip;
        [SerializeField] private AudioClip preserveClip;
        [SerializeField] private AudioClip keyReturnClip;

        private bool initialized, liveTracking, liveReady, departureStarted;
        private Vector3 previousLivePosition;
        private float livePause, visiblePause, departureTime;
        private EchoRecording reviewedRecording, witnessedRecording, preservedOriginal;
        private EchoPlayback rendererOwner;
        private Renderer[] echoRenderers;
        private readonly List<Vector2> qualifiedPauses = new List<Vector2>();
        private readonly RaycastHit[] sightHits = new RaycastHit[64];
        private readonly List<Material> witnessMaterials = new List<Material>(12);
        private const string DirectColorPass = "RecordedPresenceForward";
        private static readonly int BodyOpacityId = Shader.PropertyToID("_BodyOpacity");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int NearFadeStartId = Shader.PropertyToID("_NearFadeStart");

        public bool Active { get; private set; }
        public bool RequiresDirectEncounter => requireDirectEncounter;
        public bool HasWitnessed => witnessedRecording != null &&
            (OriginalPreserved ? ReferenceEquals(preservedOriginal, witnessedRecording) : ReferenceEquals(loop.LastRecording, witnessedRecording));
        public bool OriginalPreserved => preservedOriginal != null;
        public bool KeyReturned { get; private set; }
        public bool IsComplete { get; private set; }
        public bool HasStartedDeparture => departureStarted;
        public float Fade => departureStarted ? Mathf.Clamp01(departureTime / fadeSeconds) : 0f;
        public float WitnessProgress => Mathf.Clamp01(visiblePause / witnessSeconds);
        public bool RecordingReady => liveReady;
        public int QualifiedPauseCount => qualifiedPauses.Count;
        public EchoRecording ReviewedRecording => reviewedRecording;
        public EchoRecording WitnessedRecording => witnessedRecording;
        public EchoRecording PreservedOriginal => preservedOriginal;
        private bool CurrentPlaybackMatches => loop && loop.ActiveEcho && loop.ActiveEcho.isActiveAndEnabled &&
            !loop.ActiveEcho.IsComplete && ReferenceEquals(loop.ActiveEcho.Recording, reviewedRecording) &&
            ReferenceEquals(loop.LastRecording, reviewedRecording);

        public string Objective
        {
            get
            {
                if (IsComplete) return "El original quedó conservado. La llave quedó en portería.";
                if (KeyReturned) return "Atravesá la última puerta.";
                if (OriginalPreserved) return "Devolvé la llave en la bandeja de portería.";
                if (HasWitnessed) return requireDirectEncounter ? "Conservá esta toma en portería."
                    : "Conservá esta toma en el terminal, al otro lado de la puerta.";
                if (!Active) return "Fijá el origen de revisión dentro del vestíbulo de 6B.";
                if (liveReady) return requireDirectEncounter ? "R vuelve a 6B y restablece la puerta."
                    : "La pausa quedó en este recorrido. R vuelve al vestíbulo y restablece la puerta para revisarla.";
                if (qualifiedPauses.Count > 0 && CurrentPlaybackMatches) return requireDirectEncounter
                    ? "Abrí 6B desde adentro y observá tu llegada."
                    : "Observá la llegada desde el interior. Podés mirar por el visor o abrir 6B con E.";
                if (reviewedRecording != null && !CurrentPlaybackMatches) return requireDirectEncounter
                    ? "La toma terminó sin observarse desde el umbral. Repetí la llegada y prepará la puerta al volver con R."
                    : "La toma terminó sin verificarse. Grabá otra aproximación y observala desde dentro al volver con R.";
                if (reviewedRecording != null && qualifiedPauses.Count == 0) return "Esta toma no incluye la pausa frente a 6B. Repetí el recorrido y detenete mirando la puerta antes de R.";
                return requireDirectEncounter
                    ? "Llegá a 6B por el corredor lateral. Al encenderse la señal, R vuelve al interior."
                    : "Salí por el corredor lateral y detenete frente a 6B, mirando la puerta. R vuelve al origen interior y restablece 6B.";
            }
        }

        public string StatusText => KeyReturned ? "LLAVE DEVUELTA"
            : OriginalPreserved ? "ORIGINAL CONSERVADO"
            : HasWitnessed ? (requireDirectEncounter ? "TOMA ACTUAL OBSERVADA · POSICIÓN Y ORIENTACIÓN"
                : "TOMA ACTUAL VERIFICADA · POSICIÓN Y ORIENTACIÓN")
            : liveReady ? "PAUSA REGISTRADA · R PARA REVISAR"
            : reviewedRecording != null && !CurrentPlaybackMatches ? "TOMA FINALIZADA · REGISTRO DISPONIBLE"
            : qualifiedPauses.Count > 0 ? "REVISIÓN DE LA TOMA ACTUAL"
            : Active ? "ORIGEN INTERIOR · POSICIÓN Y ORIENTACIÓN" : "REVISIÓN DISPONIBLE";

        private void Awake()
        {
            if (!house || !session || !player || !view || !loop || !recorder || !origin || !outsideZone ||
                !outsideFacingTransform || !insideWitnessZone || !domesticDoor || !exitDoor || !departureZone || !keyInTray)
            {
                Debug.LogError("FinalDoorMemory requires explicit house, Player, recording, spatial, door and physical-key references.", this);
                enabled = false;
                return;
            }
            if (requireDirectEncounter && (!directWitnessMaterial || !directWitnessMaterial.shader ||
                directWitnessMaterial.FindPass(DirectColorPass) < 0 ||
                !directWitnessMaterial.HasProperty(BodyOpacityId) || !directWitnessMaterial.HasProperty(ColorId)))
            {
                Debug.LogError("Direct 6B observation requires the explicit RecordedPresence color material, not its depth proxy.", this);
                enabled = false;
                return;
            }
            initialized = true;
            keyInTray.SetActive(false);
            if (terminalPilot) terminalPilot.SetActive(false);
            if (recordingPilot) recordingPilot.SetActive(false);
            exitDoor.SetOpen(false);
        }

        private void OnEnable()
        {
            if (loop) loop.LoopCompleted += OnRecall;
        }

        private void OnDisable()
        {
            if (loop) loop.LoopCompleted -= OnRecall;
            visiblePause = 0f;
        }

        /// <summary>Commits an explicit interior RECALL origin, without teleporting or inventing an earlier take.</summary>
        public bool EnterReview()
        {
            if (!initialized || !isActiveAndEnabled || session.IsPaused || IsComplete || !player.isActiveAndEnabled || !house.HasKey) return false;
            if (Active) return true;
            bool wasEnabled = loop.enabled;
            loop.enabled = true;
            if (!loop.SetRecallCheckpoint(origin)) { loop.enabled = wasEnabled; return false; }
            Active = true;
            house.LeaveHouse();
            ClearUnpreservedEvidence();
            PlayInteraction(enterClip, origin.position);
            return true;
        }

        public bool CanUse(FinalDoorInteractionKind kind, GameObject who)
        {
            if (!initialized || !isActiveAndEnabled || session.IsPaused || IsComplete ||
                !player.isActiveAndEnabled || who != player.gameObject) return false;
            if (kind == FinalDoorInteractionKind.BeginReview)
                return !Active && house.HasKey;
            if (!Active || departureStarted) return false;
            switch (kind)
            {
                case FinalDoorInteractionKind.OpenDoor: return !domesticDoor.IsOpenRequested;
                case FinalDoorInteractionKind.Preserve: return HasWitnessed && !OriginalPreserved;
                case FinalDoorInteractionKind.ReturnKey: return OriginalPreserved && !KeyReturned && house.HasKey;
                case FinalDoorInteractionKind.Document: return true;
                default: return false;
            }
        }

        public string GetPrompt(FinalDoorInteractionKind kind)
        {
            switch (kind)
            {
                case FinalDoorInteractionKind.BeginReview: return "Fijar origen interior";
                case FinalDoorInteractionKind.OpenDoor: return "Abrir 6B";
                case FinalDoorInteractionKind.Preserve: return "Conservar este original";
                case FinalDoorInteractionKind.ReturnKey: return "Devolver la llave";
                case FinalDoorInteractionKind.Document: return "Leer";
                default: return "";
            }
        }

        public void Use(FinalDoorInteractionKind kind, GameObject who, Vector3 sourcePosition)
        {
            if (!CanUse(kind, who)) return;
            switch (kind)
            {
                case FinalDoorInteractionKind.BeginReview:
                    EnterReview();
                    break;
                case FinalDoorInteractionKind.OpenDoor:
                    domesticDoor.Open();
                    break;
                case FinalDoorInteractionKind.Preserve:
                    // Keep the exact object that was actually witnessed, never a copy or new recording.
                    if (!ReferenceEquals(witnessedRecording, loop.LastRecording)) return;
                    preservedOriginal = witnessedRecording;
                    loop.enabled = false;
                    recorder.CancelRecording();
                    PlayInteraction(preserveClip, sourcePosition);
                    RefreshPilots();
                    break;
                case FinalDoorInteractionKind.ReturnKey:
                    KeyReturned = true;
                    keyInTray.SetActive(true);
                    exitDoor.SetOpen(true);
                    PlayInteraction(keyReturnClip, sourcePosition);
                    break;
            }
        }

        public void ReadDocument(GameObject who, string title, string body)
        {
            if (CanUse(FinalDoorInteractionKind.Document, who)) session.ShowDocument(title, body);
        }

        private void Update()
        {
            if (!Active || IsComplete) return;
            if (!OriginalPreserved && !ReferenceEquals(reviewedRecording, loop.LastRecording)) RefreshCurrentRecording();
            if (session.IsPaused || !player.isActiveAndEnabled)
            {
                // Looking before and after a pause is not one contiguous observation.
                visiblePause = 0f;
                liveTracking = false;
                livePause = 0f;
                return;
            }

            if (KeyReturned)
            {
                if (!departureStarted && Inside(departureZone, player.transform.position)) departureStarted = true;
                if (departureStarted)
                {
                    departureTime += Time.deltaTime;
                    if (departureTime >= fadeSeconds)
                    {
                        IsComplete = true;
                        session.Finish();
                    }
                }
                return;
            }
            if (OriginalPreserved) return;
            UpdateLivePreview();
            UpdateWitness();
            RefreshPilots();
        }

        private void OnRecall()
        {
            if (!Active || OriginalPreserved) return;
            // LoopCompleted fires after the Player has already returned to the safe interior origin.
            // Only this review leaf resets; the completed house exit is a separate persistent object.
            domesticDoor.ResetClosedForRecall();
            RefreshCurrentRecording();
        }

        private void RefreshCurrentRecording()
        {
            visiblePause = 0f;
            livePause = 0f;
            liveTracking = false;
            liveReady = false;
            if (!ReferenceEquals(witnessedRecording, loop.LastRecording)) witnessedRecording = null;
            reviewedRecording = loop.LastRecording;
            FindRecordedPauses(reviewedRecording);
            RefreshPilots();
        }

        private void ClearUnpreservedEvidence()
        {
            reviewedRecording = null;
            witnessedRecording = null;
            qualifiedPauses.Clear();
            visiblePause = 0f;
            livePause = 0f;
            liveTracking = false;
            liveReady = false;
            RefreshPilots();
        }

        private bool ValidOutsidePose(Vector3 position, Quaternion rotation)
        {
            return Inside(outsideZone, position) &&
                Mathf.Abs(Mathf.DeltaAngle(rotation.eulerAngles.y, outsideFacingTransform.eulerAngles.y)) <= facingTolerance;
        }

        private static bool Inside(BoxCollider zone, Vector3 footPosition)
        {
            Vector3 local = zone.transform.InverseTransformPoint(footPosition + Vector3.up) - zone.center;
            Vector3 half = zone.size * .5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        private void FindRecordedPauses(EchoRecording recording)
        {
            qualifiedPauses.Clear();
            if (recording == null) return;
            bool inRun = false;
            float start = 0f, end = 0f;
            float maximumGap = recorder.SampleInterval * 1.5f;
            for (int i = 0; i < recording.FrameCount; i++)
            {
                EchoFrame frame = recording[i];
                bool valid = ValidOutsidePose(frame.Position, frame.Rotation);
                float dt = i > 0 ? frame.Timestamp - recording[i - 1].Timestamp : 0f;
                bool contiguous = inRun && valid && dt > 0f && dt <= maximumGap &&
                    Vector3.Distance(frame.Position, recording[i - 1].Position) <= maximumStillSpeed * dt;
                if (!contiguous)
                {
                    if (inRun && end - start >= minimumPause - .0001f) qualifiedPauses.Add(new Vector2(start, end));
                    inRun = valid;
                    start = frame.Timestamp;
                }
                end = frame.Timestamp;
            }
            if (inRun && end - start >= minimumPause - .0001f) qualifiedPauses.Add(new Vector2(start, end));
        }

        private void UpdateLivePreview()
        {
            Vector3 position = player.transform.position;
            bool valid = recorder.IsRecording && recorder.HasRecordingStarted && ValidOutsidePose(position, player.transform.rotation);
            bool stationary = liveTracking && Time.deltaTime > 0f &&
                Vector3.Distance(position, previousLivePosition) / Time.deltaTime <= maximumStillSpeed;
            livePause = valid && stationary ? livePause + Time.deltaTime : 0f;
            liveTracking = valid;
            previousLivePosition = position;
            // Guidance only: the completed immutable recording is checked independently after R.
            if (livePause >= minimumPause + recorder.SampleInterval * 2f) liveReady = true;
        }

        private void UpdateWitness()
        {
            if (HasWitnessed) return;
            EchoPlayback echo = loop.ActiveEcho;
            if (!echo || !echo.isActiveAndEnabled || echo.IsComplete || reviewedRecording == null ||
                !ReferenceEquals(echo.Recording, reviewedRecording) || !ReferenceEquals(loop.LastRecording, reviewedRecording))
            {
                visiblePause = 0f;
                return;
            }
            bool atRecordedPause = false;
            foreach (var pause in qualifiedPauses)
                if (echo.ElapsedTime >= pause.x && echo.ElapsedTime <= pause.y) { atRecordedPause = true; break; }

            var body = echo.GetComponent<Rigidbody>();
            Vector3 position = body ? body.position : echo.transform.position;
            Quaternion rotation = body ? body.rotation : echo.transform.rotation;
            // A qualifying timestamp alone cannot authorize a displaced or substituted playback object.
            reviewedRecording.Sample(echo.ElapsedTime, out Vector3 recordedPosition, out Quaternion recordedRotation);
            bool conforming = Vector3.Distance(position, recordedPosition) <= .025f && Quaternion.Angle(rotation, recordedRotation) <= .2f;
            bool visible = atRecordedPause && conforming && ValidOutsidePose(position, rotation) &&
                Inside(insideWitnessZone, player.transform.position) &&
                (!requireDirectEncounter || domesticDoor.IsOpenRequested) && BodyVisible(echo, position);
            visiblePause = visible ? visiblePause + Time.deltaTime : 0f;
            if (visiblePause >= witnessSeconds) witnessedRecording = reviewedRecording;
        }

        private bool BodyVisible(EchoPlayback echo, Vector3 bodyPosition)
        {
            if (rendererOwner != echo)
            {
                rendererOwner = echo;
                echoRenderers = echo.GetComponentsInChildren<Renderer>(true);
            }
            bool renderedBody = false;
            foreach (var surface in echoRenderers)
                if (surface && surface.enabled && surface.gameObject.activeInHierarchy &&
                    (view.cullingMask & (1 << surface.gameObject.layer)) != 0 &&
                    (!requireDirectEncounter || IsDirectColorSurface(surface)))
                {
                    renderedBody = true;
                    break;
                }
            // An unobstructed physics ray to an invisible/disabled playback object is not a witness.
            if (!renderedBody) return false;
            if (!BodyPointVisible(echo, bodyPosition + Vector3.up * bodyWitnessHeight, !requireDirectEncounter)) return false;
            return !requireDirectEncounter || BodyPointVisible(echo, bodyPosition + Vector3.up * headWitnessHeight, false);
        }

        private bool IsDirectColorSurface(Renderer surface)
        {
            // A depth proxy can remain enabled when its sibling color renderer is
            // disabled. Whitelist the authored color material, not Renderer.enabled
            // or a GameObject name. No material instances or per-frame arrays.
            if (surface.forceRenderingOff || surface.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly ||
                !directWitnessMaterial || !directWitnessMaterial.shader || !directWitnessMaterial.shader.isSupported ||
                directWitnessMaterial.FindPass(DirectColorPass) < 0 ||
                !directWitnessMaterial.GetShaderPassEnabled(DirectColorPass) ||
                !directWitnessMaterial.HasProperty(BodyOpacityId) || !directWitnessMaterial.HasProperty(ColorId) ||
                directWitnessMaterial.GetFloat(BodyOpacityId) <= .0001f || directWitnessMaterial.GetColor(ColorId).a <= .0001f)
                return false;
            // The authored near band stays beyond .95m. Also reject a fully faded
            // display if an author changes that band later; do not count its proxy.
            if (directWitnessMaterial.HasProperty(NearFadeStartId))
            {
                Vector3 offset = surface.transform.position - view.transform.position;
                offset.y = 0f;
                if (offset.magnitude <= directWitnessMaterial.GetFloat(NearFadeStartId)) return false;
            }
            witnessMaterials.Clear();
            surface.GetSharedMaterials(witnessMaterials);
            foreach (var material in witnessMaterials)
                if (material == directWitnessMaterial) return true;
            return false;
        }

        private bool BodyPointVisible(EchoPlayback echo, Vector3 target, bool allowAuthoredGlass)
        {
            Vector3 direction = target - view.transform.position;
            Vector3 viewport = view.WorldToViewportPoint(target);
            if (viewport.z <= view.nearClipPlane || viewport.z >= view.farClipPlane ||
                viewport.x < .06f || viewport.x > .94f || viewport.y < .06f || viewport.y > .94f) return false;
            float distance = direction.magnitude;
            if (distance < .01f) return false;
            int count = Physics.RaycastNonAlloc(view.transform.position, direction / distance, sightHits,
                distance, sightMask, QueryTriggerInteraction.Ignore);
            if (count >= sightHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Collider collider = sightHits[i].collider;
                if (!collider || collider.transform.IsChildOf(player.transform) ||
                    collider.transform.IsChildOf(echo.transform) || (allowAuthoredGlass && IsTransparent(collider))) continue;
                return false;
            }
            return true;
        }

        private bool IsTransparent(Collider collider)
        {
            foreach (var transparent in transparentSightColliders) if (transparent == collider) return true;
            return false;
        }

        private void RefreshPilots()
        {
            if (terminalPilot) terminalPilot.SetActive(HasWitnessed || OriginalPreserved);
            if (recordingPilot) recordingPilot.SetActive(Active && !OriginalPreserved && liveReady);
        }

        private void PlayInteraction(AudioClip clip, Vector3 position)
        {
            if (!audioSource || !audioSource.isActiveAndEnabled || !clip) return;
            audioSource.Stop();
            audioSource.transform.position = position;
            audioSource.PlayOneShot(clip);
        }

        private void OnValidate()
        {
            minimumPause = Mathf.Max(2f, minimumPause);
            maximumStillSpeed = Mathf.Clamp(maximumStillSpeed, 0f, .12f);
            facingTolerance = Mathf.Clamp(facingTolerance, 1f, 35f);
            witnessSeconds = Mathf.Max(.6f, witnessSeconds);
            fadeSeconds = Mathf.Max(5f, fadeSeconds);
            bodyWitnessHeight = Mathf.Max(.1f, bodyWitnessHeight);
            headWitnessHeight = Mathf.Max(bodyWitnessHeight + .1f, headWitnessHeight);
        }
    }
}

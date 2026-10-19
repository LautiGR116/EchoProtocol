using System.Collections.Generic;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using UnityEngine;

namespace EchoProtocol.Horror
{
    // One post-R3 beat. Reads the current immutable snapshot; never edits playback.
    [DisallowMultipleComponent]
    public sealed class VS1DenialEvidence : MonoBehaviour, IInteractable
    {
        [Header("Existing route")]
        [SerializeField] private VS1HubReturnGate hubReturn;
        [SerializeField] private BoxCollider arrivalInterior;
        [SerializeField] private BoxCollider arrivalClear;
        [SerializeField] private LoopActor player;
        [SerializeField] private CharacterController playerBody;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private Collider observationGlass;

        [Header("Local scene")]
        [SerializeField] private GameObject presentation;
        [SerializeField] private Transform recallPoint;
        [SerializeField] private BoxCollider pauseZone;
        [SerializeField] private BoxCollider observerZone;
        [SerializeField] private Transform pauseFacing;
        [SerializeField] private TextMesh stationLabel;
        [SerializeField] private TextMesh recordingLabel;
        [SerializeField] private SD1SubtitleDisplay thought;
        [SerializeField] private AudioSource[] ambienceBeds;

        [Header("Recorded evidence")]
        [SerializeField, Min(0.2f)] private float minimumPause = 1f;
        [SerializeField, Min(0.01f)] private float stillRadius = 0.08f;
        [SerializeField, Min(0f)] private float maximumStillSpeed = 0.15f;
        [SerializeField, Range(5f, 60f)] private float facingTolerance = 30f;
        [SerializeField, Min(0.1f)] private float witnessSeconds = 0.45f;
        [SerializeField, Range(0f, 1f)] private float ambienceFloor = 0.25f;

        private readonly List<Vector2> pauses = new List<Vector2>();
        private EchoRecording reviewedRecording;
        private bool enteredArrival;
        private bool reviewing;
        private bool liveStill;
        private bool liveReady;
        private Vector3 liveOrigin;
        private Vector3 livePrevious;
        private float liveDuration;
        private float visiblePause;
        private float[] savedVolumes;
        private bool ownsMix;
        private float mix = 1f;

        public bool IsAvailable { get; private set; }
        public bool IsActivated { get; private set; }
        public bool HasWitnessed { get; private set; }
        public bool IsReviewing => reviewing;
        public int QualifiedPauseCount => pauses.Count;
        public int DeliveryCount { get; private set; }
        public float AmbienceMultiplier => mix;

        private void Awake()
        {
            if (hubReturn == null || arrivalInterior == null || arrivalClear == null
                || player == null || player.Kind != LoopActorKind.Player || playerBody == null
                || playerBody.GetComponent<LoopActor>() != player || viewCamera == null
                || loop == null || recorder == null || observationGlass == null
                || presentation == null || recallPoint == null || pauseZone == null
                || observerZone == null || pauseFacing == null || stationLabel == null
                || recordingLabel == null || thought == null || ambienceBeds == null
                || ambienceBeds.Length == 0 || System.Array.Exists(ambienceBeds, a => a == null))
            {
                Debug.LogError("VS1DenialEvidence requires explicit route, Player, snapshot, presentation and ambience references.", this);
                enabled = false;
                return;
            }
            savedVolumes = new float[ambienceBeds.Length];
            presentation.SetActive(false);
            ShowAnchor();
        }

        private void OnEnable()
        {
            if (loop != null) loop.LoopCompleted += OnRecall;
        }

        private void OnDisable()
        {
            if (loop != null) loop.LoopCompleted -= OnRecall;
            RestoreAmbience();
            if (thought != null) thought.Show(string.Empty, 0f);
        }

        private bool Inside(BoxCollider zone, Vector3 footPosition)
            => zone.bounds.Contains(footPosition + Vector3.up);

        private bool ActualPlayerAvailable => player.isActiveAndEnabled && playerBody.enabled;

        private void Update()
        {
            if (!ActualPlayerAvailable)
            {
                visiblePause = 0f;
                RestoreAmbience();
                return;
            }
            if (!IsAvailable)
            {
                // Two physical arrival volumes in order, not merely P3's exit unlock.
                if (!hubReturn.IsOpen) return;
                if (Inside(arrivalInterior, player.transform.position)) enteredArrival = true;
                if (!enteredArrival || !Inside(arrivalClear, player.transform.position)) return;
                IsAvailable = true;
                presentation.SetActive(true);
            }
            if (!IsActivated) return;

            if (reviewing)
                UpdateReview();
            else if (!HasWitnessed)
                UpdateLivePreview();
        }

        public bool CanInteract(GameObject interactor) => isActiveAndEnabled
            && IsAvailable && !IsActivated && ActualPlayerAvailable && loop.isActiveAndEnabled
            && interactor != null && interactor.GetComponentInParent<LoopActor>() == player;

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor) || !loop.SetRecallCheckpoint(recallPoint)) return;
            IsActivated = true;
            ShowRecordingInstruction();
        }

        private bool ValidPose(Vector3 position, Quaternion rotation)
            => Inside(pauseZone, position)
                && Vector3.Angle(rotation * Vector3.forward, pauseFacing.forward) <= facingTolerance;

        private void UpdateLivePreview()
        {
            Vector3 p = player.transform.position;
            bool valid = recorder.IsRecording && recorder.HasRecordingStarted
                && ValidPose(p, player.transform.rotation);
            float speed = liveStill && Time.deltaTime > 0f
                ? Vector3.Distance(p, livePrevious) / Time.deltaTime : 0f;
            if (!valid || !liveStill || Vector3.Distance(p, liveOrigin) > stillRadius
                || speed > maximumStillSpeed)
            {
                liveDuration = 0f;
                liveOrigin = p;
                liveStill = valid;
            }
            else liveDuration += Time.deltaTime;
            livePrevious = p;
            // Preview only. The immutable snapshot is independently checked after R.
            if (!liveReady && liveDuration >= minimumPause + recorder.SampleInterval * 2f)
            {
                liveReady = true;
                recordingLabel.text = "REGISTRO LISTO\n[R] Revisar recorrido";
                stationLabel.text = "ORIGEN ACTIVO\n[R] Revisar recorrido";
            }
        }

        private void OnRecall()
        {
            enteredArrival = false;
            if (!IsActivated) return;
            RestoreAmbience();
            thought.Show(string.Empty, 0f);
            visiblePause = 0f;
            liveStill = false;
            liveReady = false;
            liveDuration = 0f;
            reviewing = false;
            reviewedRecording = null;
            pauses.Clear();
            if (HasWitnessed) return;

            reviewedRecording = loop.LastRecording;
            FindRecordedPauses(reviewedRecording);
            if (pauses.Count == 0)
            {
                reviewedRecording = null;
                ShowRetry("Falta registrar la pausa");
                return;
            }
            reviewing = true;
            stationLabel.text = "REVISION\nObserva el otro lado";
            recordingLabel.text = "REVISION EN CURSO";
        }

        private void FindRecordedPauses(EchoRecording recording)
        {
            if (recording == null) return;
            bool inRun = false;
            Vector3 origin = Vector3.zero;
            float start = 0f, end = 0f;
            float maximumGap = recorder.SampleInterval * 1.5f;
            for (int i = 0; i < recording.FrameCount; i++)
            {
                EchoFrame frame = recording[i];
                bool valid = ValidPose(frame.Position, frame.Rotation);
                float dt = i > 0 ? frame.Timestamp - recording[i - 1].Timestamp : 0f;
                bool contiguous = inRun && valid && dt > 0f && dt <= maximumGap
                    && Vector3.Distance(frame.Position, origin) <= stillRadius
                    && Vector3.Distance(frame.Position, recording[i - 1].Position) <= maximumStillSpeed * dt;
                if (!contiguous)
                {
                    if (inRun && end - start >= minimumPause - 0.0001f)
                        pauses.Add(new Vector2(start, end));
                    inRun = valid;
                    origin = frame.Position;
                    start = frame.Timestamp;
                }
                end = frame.Timestamp;
            }
            if (inRun && end - start >= minimumPause - 0.0001f)
                pauses.Add(new Vector2(start, end));
        }

        private void UpdateReview()
        {
            EchoPlayback echo = loop.ActiveEcho;
            if (echo == null || !echo.gameObject.activeInHierarchy || echo.IsComplete
                || !ReferenceEquals(loop.LastRecording, reviewedRecording)
                || !ReferenceEquals(echo.Recording, reviewedRecording))
            {
                reviewing = false;
                reviewedRecording = null;
                RestoreAmbience();
                if (!HasWitnessed) ShowRetry("Volve a registrar la pausa");
                return;
            }

            bool visible = EchoVisible(echo);
            bool near = Vector3.Distance(player.transform.position, echo.transform.position) < 3f;
            ApplyAmbience(visible && near ? ambienceFloor : 1f);
            bool atPause = false;
            foreach (Vector2 pause in pauses)
                if (echo.ElapsedTime >= pause.x && echo.ElapsedTime <= pause.y)
                {
                    atPause = true;
                    break;
                }
            visiblePause = visible && atPause ? visiblePause + Time.deltaTime : 0f;
            if (!HasWitnessed && visiblePause >= witnessSeconds)
            {
                HasWitnessed = true;
                DeliveryCount++;
                thought.Show("No me detuve ahí.", 3.5f);
                stationLabel.text = "REVISION COMPLETA";
                recordingLabel.text = "REVISION COMPLETA";
            }
        }

        private bool EchoVisible(EchoPlayback echo)
        {
            if (!Inside(observerZone, player.transform.position)) return false;
            Vector3 target = echo.transform.position + Vector3.up * 1.4f;
            Vector3 direction = target - viewCamera.transform.position;
            Vector3 viewport = viewCamera.WorldToViewportPoint(target);
            if (viewport.z <= 0f || viewport.x < 0.08f || viewport.x > 0.92f
                || viewport.y < 0.08f || viewport.y > 0.92f
                || Vector3.Angle(viewCamera.transform.forward, direction) > 45f) return false;
            foreach (RaycastHit hit in Physics.RaycastAll(viewCamera.transform.position,
                direction.normalized, direction.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == observationGlass || hit.collider == playerBody
                    || hit.collider.transform.IsChildOf(echo.transform)) continue;
                return false;
            }
            return true;
        }

        private void ApplyAmbience(float target)
        {
            if (!ownsMix && target >= 1f) return;
            if (!ownsMix)
            {
                for (int i = 0; i < ambienceBeds.Length; i++) savedVolumes[i] = ambienceBeds[i].volume;
                ownsMix = true;
            }
            mix = Mathf.MoveTowards(mix, target, Time.deltaTime * (target < mix ? 2f : 0.8f));
            for (int i = 0; i < ambienceBeds.Length; i++) ambienceBeds[i].volume = savedVolumes[i] * mix;
            if (mix >= 1f) RestoreAmbience();
        }

        private void RestoreAmbience()
        {
            if (ownsMix)
                for (int i = 0; i < ambienceBeds.Length; i++)
                    if (ambienceBeds[i] != null) ambienceBeds[i].volume = savedVolumes[i];
            ownsMix = false;
            mix = 1f;
        }

        private void ShowAnchor()
        {
            stationLabel.text = "REVISION\n[E] Fijar origen";
            recordingLabel.text = "OBSERVACION";
        }

        private void ShowRecordingInstruction()
        {
            stationLabel.text = "ORIGEN ACTIVO\nRegistra tu recorrido\nal otro lado del vidrio";
            recordingLabel.text = "OBSERVACION\nDetenete frente al vidrio\ny mira al otro lado";
        }

        private void ShowRetry(string reason)
        {
            stationLabel.text = "ORIGEN ACTIVO\n" + reason;
            recordingLabel.text = "OBSERVACION\nDetenete frente al vidrio\ny mira al otro lado";
        }
    }
}

using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    public sealed class VS1DomesticDoorReveal : MonoBehaviour
    {
        [SerializeField] private Transform movingLeaf;
        [SerializeField] private Collider leafCollider;
        [SerializeField] private AudioSource creakSource;
        [SerializeField, Range(15f, 110f)] private float openAngle = 76f;
        [SerializeField, Min(0.5f)] private float openDuration = 5.2f;
        [SerializeField, Range(0.1f, 3f)] private float startPitch = 0.72f;
        [SerializeField, Range(0.1f, 3f)] private float endPitch = 0.58f;
        [SerializeField, Min(0.08f)] private float slamCloseDuration = 0.18f;
        [Header("Optional SE1 pressure clearance")]
        [SerializeField] private CharacterController safetyPlayer;
        [SerializeField] private BoxCollider slamSweepClearance;

        private Quaternion closedLocalRotation;
        private Quaternion openLocalRotation;
        private float elapsed;

        public bool IsOpening { get; private set; }

        public bool IsFullyOpen { get; private set; }

        public bool IsSlammingClosed { get; private set; }

        public bool IsFullyClosedAfterSlam { get; private set; }

        public int BeginOpenCount { get; private set; }

        public float OpenProgress => IsFullyOpen
            ? 1f
            : Mathf.Clamp01(elapsed / openDuration);

        private void Awake()
        {
            if (movingLeaf == null
                || leafCollider == null
                || creakSource == null
                || creakSource.clip == null)
            {
                Debug.LogError(
                    "VS1DomesticDoorReveal requires a moving leaf, collider, and creak source.",
                    this);
                enabled = false;
                return;
            }

            closedLocalRotation = movingLeaf.localRotation;
            openLocalRotation = closedLocalRotation
                * Quaternion.Euler(0f, -openAngle, 0f);
            ConfigureSource();
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            if (IsSlammingClosed)
            {
                UpdateSlamClose();
                return;
            }

            if (!IsOpening)
            {
                return;
            }

            elapsed = Mathf.Min(openDuration, elapsed + Time.deltaTime);
            float progress = Mathf.Clamp01(elapsed / openDuration);
            float eased = progress * progress * (3f - 2f * progress);
            movingLeaf.localRotation = Quaternion.Slerp(
                closedLocalRotation,
                openLocalRotation,
                eased);
            creakSource.pitch = Mathf.Lerp(startPitch, endPitch, eased);

            if (progress < 1f)
            {
                return;
            }

            IsOpening = false;
            IsFullyOpen = true;
            creakSource.Stop();
        }

        public void BeginOpen()
        {
            if (!enabled || IsOpening || IsFullyOpen || IsSlammingClosed
                || IsFullyClosedAfterSlam)
            {
                return;
            }

            elapsed = 0f;
            IsOpening = true;
            BeginOpenCount++;
            creakSource.pitch = startPitch;
            creakSource.Play();
        }

        public bool BeginSlamClose()
        {
            if (!enabled || !IsFullyOpen || IsSlammingClosed
                || IsFullyClosedAfterSlam || Time.deltaTime <= 0f || IsPlayerInSlamSweep())
            {
                return false;
            }

            creakSource.Stop();
            elapsed = 0f;
            IsOpening = false;
            IsFullyOpen = false;
            IsSlammingClosed = true;
            return true;
        }

        private void UpdateSlamClose()
        {
            // A return into the doorway pauses closure, including mid-stroke.
            // Keep control and current leaf pose; never push the capsule out.
            if (IsPlayerInSlamSweep()) return;
            elapsed = Mathf.Min(
                slamCloseDuration,
                elapsed + Time.deltaTime);
            float progress = Mathf.Clamp01(elapsed / slamCloseDuration);
            float accelerated = progress * progress;
            movingLeaf.localRotation = Quaternion.Slerp(
                openLocalRotation,
                closedLocalRotation,
                accelerated);

            if (progress < 1f)
            {
                return;
            }

            movingLeaf.localRotation = closedLocalRotation;
            IsSlammingClosed = false;
            IsFullyClosedAfterSlam = true;
        }

        private bool IsPlayerInSlamSweep()
        {
            if (!safetyPlayer || !slamSweepClearance) return false;
            if (!safetyPlayer.enabled || !safetyPlayer.gameObject.activeInHierarchy) return true;
            Bounds body = safetyPlayer.bounds;
            body.Expand(.16f);
            return slamSweepClearance.bounds.Intersects(body);
        }

        private void ConfigureSource()
        {
            creakSource.playOnAwake = false;
            creakSource.loop = true;
            creakSource.spatialBlend = 1f;
            creakSource.dopplerLevel = 0f;
            creakSource.rolloffMode = AudioRolloffMode.Linear;
            creakSource.minDistance = 3f;
            creakSource.maxDistance = 20f;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (movingLeaf != null)
            {
                movingLeaf.localRotation = closedLocalRotation;
            }

            if (creakSource != null)
            {
                creakSource.Stop();
                creakSource.pitch = startPitch;
            }

            elapsed = 0f;
            IsOpening = false;
            IsFullyOpen = false;
            IsSlammingClosed = false;
            IsFullyClosedAfterSlam = false;
            BeginOpenCount = 0;
        }

        private void OnValidate()
        {
            openAngle = Mathf.Clamp(openAngle, 15f, 110f);
            openDuration = Mathf.Max(0.5f, openDuration);
            startPitch = Mathf.Clamp(startPitch, 0.1f, 3f);
            endPitch = Mathf.Clamp(endPitch, 0.1f, 3f);
            slamCloseDuration = Mathf.Max(0.08f, slamCloseDuration);
        }
    }
}

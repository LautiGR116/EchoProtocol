using UnityEngine;

namespace EchoProtocol.Campaign
{
    /// <summary>A persistent, one-way mechanical opening. This is not an Echo action.</summary>
    [DisallowMultipleComponent]
    public sealed class HouseSwingDoor : MonoBehaviour
    {
        [Header("Explicit hinge and collision references")]
        [SerializeField] private Rigidbody hingeBody;
        [SerializeField] private BoxCollider solidLeaf;
        [SerializeField] private CharacterController playerBody;
        [Header("Authored opening")]
        [SerializeField] private Vector3 localHingeAxis = Vector3.up;
        [SerializeField, Range(-170f, 170f)] private float openAngle = 105f;
        [SerializeField, Min(1f)] private float degreesPerSecond = 42f;
        [Header("Optional causal sound")]
        [SerializeField] private AudioSource movementAudio;
        [SerializeField] private AudioClip movementClip;
        [SerializeField] private AudioSource latchAudio;
        [SerializeField] private AudioClip unlockClip;
        [SerializeField] private AudioClip openEndpointClip;

        private Quaternion closedLocalRotation;
        private Quaternion openLocalRotation;
        private bool initialized, endpointPlayed;

        public bool IsOpenRequested { get; private set; }
        public bool IsFullyOpen => initialized && Quaternion.Angle(hingeBody.transform.localRotation, openLocalRotation) < .2f;
        public bool IsObstructed { get; private set; }

        private void Awake()
        {
            if (!hingeBody || !solidLeaf || !playerBody || !hingeBody.isKinematic || solidLeaf.isTrigger || !solidLeaf.enabled ||
                (solidLeaf.transform != hingeBody.transform && !solidLeaf.transform.IsChildOf(hingeBody.transform)))
            {
                Debug.LogError("HouseSwingDoor requires a kinematic hinge, its solid BoxCollider and the Player CharacterController.", this);
                enabled = false;
                return;
            }

            closedLocalRotation = hingeBody.transform.localRotation;
            openLocalRotation = closedLocalRotation * Quaternion.AngleAxis(openAngle, localHingeAxis.normalized);
            initialized = true;
            if (movementAudio) { movementAudio.playOnAwake = false; movementAudio.Stop(); }
        }

        public bool Open()
        {
            if (!initialized || !enabled) return false;
            if (IsOpenRequested) return true;
            IsOpenRequested = true;
            if (latchAudio && unlockClip) latchAudio.PlayOneShot(unlockClip);
            return true;
        }

        /// <summary>Explicit review baseline, called only after RECALL has moved the Player clear.</summary>
        public void ResetClosedForRecall()
        {
            if (!initialized) return;
            StopMovementSound();
            if (latchAudio) latchAudio.Stop();
            IsOpenRequested = false;
            IsObstructed = false;
            endpointPlayed = false;
            hingeBody.transform.localRotation = closedLocalRotation;
            hingeBody.rotation = hingeBody.transform.rotation;
            Physics.SyncTransforms();
        }

        private void FixedUpdate()
        {
            if (!initialized || !IsOpenRequested) return;
            if (IsFullyOpen)
            {
                StopMovementSound();
                IsObstructed = false;
                if (!endpointPlayed)
                {
                    endpointPlayed = true;
                    if (latchAudio && openEndpointClip) latchAudio.PlayOneShot(openEndpointClip);
                }
                return;
            }

            Quaternion parentRotation = hingeBody.transform.parent ? hingeBody.transform.parent.rotation : Quaternion.identity;
            Quaternion target = parentRotation * openLocalRotation;
            // A bounded step also prevents a slow frame from sweeping through the Player.
            float step = Mathf.Min(2f, degreesPerSecond * Time.fixedDeltaTime);
            Quaternion next = Quaternion.RotateTowards(hingeBody.rotation, target, step);
            Quaternion delta = next * Quaternion.Inverse(hingeBody.rotation);
            Vector3 nextLeafPosition = hingeBody.position + delta * (solidLeaf.transform.position - hingeBody.position);
            Quaternion nextLeafRotation = delta * solidLeaf.transform.rotation;
            IsObstructed = playerBody.enabled && Physics.ComputePenetration(
                solidLeaf, nextLeafPosition, nextLeafRotation,
                playerBody, playerBody.transform.position, playerBody.transform.rotation,
                out _, out float overlap) && overlap > .001f;

            if (IsObstructed) { StopMovementSound(); return; }
            hingeBody.MoveRotation(next);
            if (movementAudio && movementClip && !movementAudio.isPlaying)
            {
                movementAudio.clip = movementClip;
                movementAudio.loop = true;
                movementAudio.Play();
            }
        }

        private void StopMovementSound()
        {
            if (movementAudio && movementAudio.isPlaying) movementAudio.Stop();
        }

        private void OnDisable() { StopMovementSound(); }

        private void OnValidate()
        {
            degreesPerSecond = Mathf.Max(1f, degreesPerSecond);
            if (localHingeAxis.sqrMagnitude < .001f) localHingeAxis = Vector3.up;
        }
    }
}

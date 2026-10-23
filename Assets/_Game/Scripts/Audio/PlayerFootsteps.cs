using UnityEngine;
using EchoProtocol.Player;

namespace EchoProtocol.Audio
{
    // Current Player contact sounds are environmental feedback, never a replayed Echo channel.
    [DefaultExecutionOrder(320), DisallowMultipleComponent]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioSource alternateSource;
        [SerializeField] private PlayerEchoClearance echoClearance;
        [SerializeField] private AudioClip[] vinyl;
        [SerializeField] private AudioClip[] timber;
        [SerializeField] private BoxCollider[] timberZones;
        [SerializeField, Min(.2f)] private float stride = 1.4f;
        [SerializeField, Min(.1f)] private float firstContactDistance = .42f;

        private readonly RaycastHit[] groundHits = new RaycastHit[12];
        private Vector3 previous;
        private float travelled;
        private float idleTime;
        private float nextContactTime;
        private bool starting = true;
        private int resetFrame = -1;

        public int ContactCount { get; private set; }

        private void Awake()
        {
            if (!controller || !player || !source || !ValidClips(vinyl) || !ValidClips(timber)
                || (alternateSource && alternateSource == source))
            {
                Debug.LogError("PlayerFootsteps requires explicit controller, Player, six contact clips, and distinct foot sources when a second source is assigned.", this);
                enabled = false;
                return;
            }
            player.PoseReset += ResetMotion;
        }

        private void OnEnable() => ResetMotion();
        private void OnDisable() => ResetMotion();
        private void OnDestroy()
        {
            if (player) player.PoseReset -= ResetMotion;
        }

        private static bool ValidClips(AudioClip[] clips)
        {
            if (clips == null || clips.Length != 3) return false;
            foreach (var clip in clips) if (!clip) return false;
            return true;
        }

        private void ResetMotion()
        {
            previous = transform.position;
            travelled = idleTime = 0f;
            starting = true;
            resetFrame = Time.frameCount;
            // A tail belongs at its original floor contact, never at a new
            // RECALL origin or after returning from a suspended session.
            if (source) source.Stop();
            if (alternateSource && alternateSource != source) alternateSource.Stop();
        }

        private void LateUpdate()
        {
            Vector3 position = transform.position;
            Vector3 delta = position - previous;
            previous = position;
            if (Time.deltaTime <= 0f || AudioListener.pause || !player.isActiveAndEnabled
                || !controller.enabled || player.MovementSuppressed)
            {
                ResetMotion();
                return;
            }
            if (resetFrame == Time.frameCount) return;

            // Preserve the discontinuity guard for placements outside ResetPose.
            if (delta.magnitude > 1f)
            {
                ResetMotion();
                return;
            }
            delta.y = 0f;
            // Sample after actual movement and live-body clearance. Count only
            // voluntary distance that survives that correction: neither being
            // pushed nor repeatedly moving into a body and being pushed back
            // may accumulate a walking cadence while the Player stays still.
            Vector3 voluntaryDelta = delta;
            if (echoClearance && echoClearance.AppliedDisplacementFrame == Time.frameCount)
            {
                Vector3 correction = echoClearance.AppliedDisplacementThisFrame;
                correction.y = 0f;
                voluntaryDelta -= correction;
            }
            float distance = voluntaryDelta.magnitude;
            if (distance > .00001f)
                distance = Mathf.Clamp(Vector3.Dot(delta, voluntaryDelta / distance), 0f, distance);
            float speed = distance / Time.deltaTime;
            if (speed < .08f)
            {
                idleTime += Time.deltaTime;
                if (idleTime >= .14f)
                {
                    travelled = 0f;
                    starting = true;
                }
                return;
            }
            idleTime = 0f;

            // A downward probe establishes support independently of the last
            // CharacterController.Move flags (a lateral clearance Move may
            // replace those flags). It also places sound on the real floor.
            float side = (ContactCount & 1) == 0 ? -1f : 1f;
            Vector3 foot = controller.bounds.center + transform.right * (side * .1f);
            foot.y = controller.bounds.min.y;
            if (!TryGroundContact(foot, out Vector3 contact))
            {
                travelled = 0f;
                starting = true;
                return;
            }

            travelled += distance;
            float threshold = starting ? Mathf.Min(firstContactDistance, stride) : stride;
            if (travelled < threshold) return;
            // A fresh short start responds sooner, but repeated start/stop taps
            // cannot produce more contacts than the existing full-speed gait.
            if (Time.time < nextContactTime)
            {
                travelled = Mathf.Min(travelled, threshold);
                return;
            }
            // No catch-up queue after a slow frame. Only the contact happening
            // now is audible; subsequent frames retain a bounded stride phase.
            travelled = Mathf.Repeat(travelled - threshold, stride);
            starting = false;
            var clips = IsTimber(contact) ? timber : vinyl;
            // With the explicitly wired second foot, a .44 s timber tail stays
            // at its support when the other foot lands .35 s later at full pace.
            AudioSource contactSource = side > 0f && alternateSource ? alternateSource : source;
            contactSource.transform.position = contact + Vector3.up * .025f;
            // Conservative weight response: slow wall-sliding/creeping gets
            // lighter, while normal 4 m/s movement retains the existing gain.
            float weight = Mathf.Lerp(.5f, 1f, Mathf.Clamp01(speed / 4f));
            contactSource.PlayOneShot(clips[ContactCount % clips.Length], weight);
            nextContactTime = Time.time + stride / 4f;
            ContactCount++;
        }

        private bool TryGroundContact(Vector3 foot, out Vector3 contact)
        {
            const float probeLift = .2f;
            float groundTolerance = Mathf.Max(.05f, controller.skinWidth + .025f);
            int count = Physics.RaycastNonAlloc(foot + Vector3.up * probeLift, Vector3.down,
                groundHits, probeLift + groundTolerance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            contact = default;
            // A full buffer cannot identify the nearest support reliably.
            if (count == groundHits.Length) return false;
            float nearest = float.PositiveInfinity;
            float minimumUp = Mathf.Cos(Mathf.Min(controller.slopeLimit, 85f) * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = groundHits[i];
                if (!hit.collider || hit.collider == controller
                    || hit.collider.transform.IsChildOf(player.transform)
                    || hit.normal.y < minimumUp || hit.distance >= nearest) continue;
                nearest = hit.distance;
                contact = hit.point;
            }
            return nearest < float.PositiveInfinity;
        }

        private bool IsTimber(Vector3 contact)
        {
            if (timberZones == null) return false;
            // Existing authored volumes include the floor. Test the actual
            // alternating support, just above its surface, rather than the
            // Player's chest crossing a material boundary first.
            Vector3 sample = contact + Vector3.up * .025f;
            foreach (var zone in timberZones)
            {
                if (zone && zone.enabled && zone.gameObject.activeInHierarchy
                    && new Bounds(zone.center, zone.size).Contains(zone.transform.InverseTransformPoint(sample)))
                    return true;
            }
            return false;
        }

        private void OnValidate()
        {
            stride = Mathf.Max(.2f, stride);
            firstContactDistance = Mathf.Clamp(firstContactDistance, .1f, stride);
        }
    }
}

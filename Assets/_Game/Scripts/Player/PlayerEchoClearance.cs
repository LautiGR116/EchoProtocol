using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Player
{
    // Only the present Player yields. Recorded roots, joints and samples are untouched.
    [DefaultExecutionOrder(280), DisallowMultipleComponent]
    public sealed class PlayerEchoClearance : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private LoopController loop;
        [SerializeField] private GameSession session;
        [SerializeField, Min(.1f)] private float clearance = .64f;
        const float SeparationSpeed = 6f;
        const float MaximumStep = .12f;
        const float ContactPadding = .006f;
        readonly RaycastHit[] obstacles = new RaycastHit[64];
        CharacterController controller;
        EchoPlayback owner;
        Transform chest;
        bool leftOrigin;
        int slideSide;
        public int Contacts { get; private set; }
        public event System.Action Displaced;
        public Vector3 AppliedDisplacementThisFrame { get; private set; }
        public int AppliedDisplacementFrame { get; private set; } = -1;

        void Awake()
        {
            controller = player ? player.GetComponent<CharacterController>() : null;
            if (!player || !loop || !session || !controller)
            { Debug.LogError("Player body clearance requires explicit Player, loop and session.", this); enabled = false; return; }
            player.PoseReset += ResetContact;
        }
        void OnDestroy() { if (player) player.PoseReset -= ResetContact; }
        void ResetContact()
        {
            owner = null; chest = null; leftOrigin = false; slideSide = 0;
            AppliedDisplacementThisFrame = Vector3.zero;
            AppliedDisplacementFrame = -1;
        }
        void OnDisable() { ResetContact(); }

        void LateUpdate()
        {
            AppliedDisplacementFrame = Time.frameCount;
            AppliedDisplacementThisFrame = Vector3.zero;
            if (Time.deltaTime <= 0f || session.IsPaused || !player.isActiveAndEnabled
                || player.MovementSuppressed || !controller.enabled) return;
            var echo = loop.ActiveEcho;
            if (!echo || echo.IsComplete || !echo.gameObject.activeInHierarchy) { ResetContact(); return; }
            if (owner != echo)
            {
                owner = echo; chest = null; leftOrigin = false; slideSide = 0;
                var rig = echo.GetComponent<EchoBodyRig>();
                if (rig && rig.CharacterMotion) foreach (var joint in rig.CharacterMotion.Joints)
                    if (joint && joint.name.EndsWith("spine_03", System.StringComparison.Ordinal)) { chest = joint; break; }
            }
            Vector3 center = chest ? chest.position : echo.transform.position;
            if (Mathf.Abs(center.y - player.transform.position.y) > 2.1f) return;
            Vector3 offset = player.transform.position - center; offset.y = 0f;
            float distance = offset.magnitude;
            // RECALL intentionally shares the origin. Enable contact only after leaving it.
            if (!leftOrigin) { if (distance > clearance + .12f) leftOrigin = true; return; }
            if (distance >= clearance) { if (distance > clearance + .04f) slideSide = 0; return; }

            Vector3 away = distance > .001f ? offset / distance : -echo.transform.forward;
            away.y = 0f; away.Normalize();
            float budget = Mathf.Min(SeparationSpeed * Time.deltaTime, MaximumStep);
            Vector3 best = Candidate(offset, away, budget);
            float bestDistance = (offset + best).sqrMagnitude;
            // A wall can consume the entire radial push. Test diagonal and tangent
            // escape on the same side until contact ends, instead of flipping every frame.
            if (best.magnitude + .001f < Mathf.Min(clearance - distance, budget))
            {
                int selectedSide = 0;
                if (slideSide != 0) EvaluateSide(offset, away, budget, slideSide, ref best, ref bestDistance, ref selectedSide);
                if (slideSide == 0 || best.sqrMagnitude < .000001f)
                {
                    EvaluateSide(offset, away, budget, 1, ref best, ref bestDistance, ref selectedSide);
                    EvaluateSide(offset, away, budget, -1, ref best, ref bestDistance, ref selectedSide);
                }
                if (selectedSide != 0) slideSide = selectedSide;
            }
            if (best.sqrMagnitude < .000001f) return;
            Vector3 before = player.transform.position;
            // CharacterController remains the final collision authority; no Transform jump,
            // capsule resize, geometry edit or movement of the Echo to manufacture space.
            controller.Move(best);
            Vector3 moved = player.transform.position - before;
            float remaining = budget - moved.magnitude;
            // The sweep is advisory: skin/contact offsets can still make Move
            // consume a seemingly free push. Spend only the remaining distance
            // on one tangent attempt, never another full radial shove.
            if (remaining > .002f && moved.sqrMagnitude < best.sqrMagnitude * .25f)
            {
                offset = player.transform.position - center; offset.y = 0f;
                if (offset.sqrMagnitude < clearance * clearance)
                {
                    away = offset.sqrMagnitude > .000001f ? offset.normalized : away;
                    Vector3 tangent = Vector3.Cross(Vector3.up, away);
                    Vector3 escape = Vector3.zero;
                    float escapedDistance = offset.sqrMagnitude;
                    int chosen = 0;
                    if (slideSide != 0)
                        Evaluate(Candidate(offset, tangent * slideSide, remaining), offset, slideSide,
                            ref escape, ref escapedDistance, ref chosen);
                    if (escape.sqrMagnitude < .000001f)
                    {
                        Evaluate(Candidate(offset, tangent, remaining), offset, 1, ref escape, ref escapedDistance, ref chosen);
                        Evaluate(Candidate(offset, -tangent, remaining), offset, -1, ref escape, ref escapedDistance, ref chosen);
                    }
                    if (escape.sqrMagnitude > .000001f)
                    { controller.Move(escape); slideSide = chosen; }
                }
            }
            AppliedDisplacementThisFrame = player.transform.position - before;
            if (AppliedDisplacementThisFrame.sqrMagnitude > .000001f)
            { Contacts++; Displaced?.Invoke(); }
        }

        void EvaluateSide(Vector3 offset, Vector3 away, float budget, int side,
            ref Vector3 best, ref float bestDistance, ref int selectedSide)
        {
            Vector3 tangent = Vector3.Cross(Vector3.up, away) * side;
            Evaluate(Candidate(offset, (away + tangent).normalized, budget), offset, side, ref best, ref bestDistance, ref selectedSide);
            Evaluate(Candidate(offset, tangent, budget), offset, side, ref best, ref bestDistance, ref selectedSide);
        }
        static void Evaluate(Vector3 candidate, Vector3 offset, int side,
            ref Vector3 best, ref float bestDistance, ref int selectedSide)
        {
            float distance = (offset + candidate).sqrMagnitude;
            if (distance <= bestDistance + .000001f) return;
            best = candidate; bestDistance = distance; selectedSide = side;
        }
        Vector3 Candidate(Vector3 offset, Vector3 direction, float budget)
        {
            // Distance along this direction needed to reach the clearance circle.
            float projection = Vector3.Dot(offset, direction);
            float needed = Mathf.Sqrt(Mathf.Max(0f, projection * projection + clearance * clearance - offset.sqrMagnitude)) - projection;
            float length = Mathf.Min(budget, needed);
            Vector3 scale = controller.transform.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float skin = controller.skinWidth * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfSegment = Mathf.Max(0f, controller.height * Mathf.Abs(scale.y) * .5f - radius);
            Vector3 center = controller.transform.TransformPoint(controller.center);
            // Contract by the controller's skin so a grounded capsule does not see
            // its supporting floor as a horizontal blocker at distance zero.
            float castRadius = Mathf.Max(.01f, radius - skin);
            int count = Physics.CapsuleCastNonAlloc(center + Vector3.up * halfSegment,
                center - Vector3.up * halfSegment, castRadius, direction, obstacles,
                length + ContactPadding, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (count == obstacles.Length) return Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                var hit = obstacles[i]; var shape = hit.collider;
                if (!shape || shape.transform.IsChildOf(player.transform) || shape.transform.IsChildOf(owner.transform)
                    || Physics.GetIgnoreLayerCollision(controller.gameObject.layer, shape.gameObject.layer)
                    || Physics.GetIgnoreCollision(controller, shape)) continue;
                if (Vector3.Dot(hit.normal, direction) >= -.001f) continue;
                length = Mathf.Min(length, Mathf.Max(0f, hit.distance - ContactPadding));
            }
            return direction * length;
        }
    }
}

using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Horror
{
    // A static person in the present. No EchoPlayback, take or recorded channel
    // participates in this composition or its single discovery accent.
    [DefaultExecutionOrder(80), DisallowMultipleComponent]
    public sealed class DomesticPresence : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private VS1ParallelFigureEvent passage;
        [SerializeField] private Transform playerView;
        [SerializeField] private RecordedCharacterMotion anatomy;
        [SerializeField] private CharacterController body;
        [SerializeField] private Transform frameGrip, chest, neck, head, contactArm, contactForearm, contactHand;
        [SerializeField] private Transform[] upperEyelids;
        [SerializeField] private Collider observationGlass;
        [SerializeField] private AudioSource frameSound;
        [SerializeField] private AudioClip frameClip;
        private Camera view;
        private Transform playerBody;
        private bool ready, posed;
        private float visibleTime;
        private readonly RaycastHit[] visibilityHits = new RaycastHit[24];
        public bool ImpactPlayed { get; private set; }
        public int ImpactCount { get; private set; }
        public bool PoseReady => posed;

        private void Start()
        {
            view = playerView ? playerView.GetComponent<Camera>() : null;
            var player = playerView ? playerView.GetComponentInParent<EchoProtocol.Player.FirstPersonController>() : null;
            playerBody = player ? player.transform : null;
            ready = session && passage && view && playerBody && anatomy && body && frameGrip && chest && neck && head
                && contactArm && contactForearm && contactHand && frameSound && frameClip
                && frameSound.outputAudioMixerGroup && frameSound.spatialBlend >= .999f
                && !frameSound.ignoreListenerPause && !frameSound.ignoreListenerVolume;
            if (!ready)
            { Debug.LogError("SE2 static person needs explicit current-world pose, camera and routed discovery sound.", this); enabled = false; }
        }

        private void LateUpdate()
        {
            if (!ready || session.IsPaused || Time.deltaTime <= 0 || !passage.PresentationCommitted
                || !anatomy.gameObject.activeInHierarchy) return;
            if (!posed) PreparePose();
            if (!posed || ImpactPlayed) return;
            // Broad visible-body evidence, not a gaze cone or a face-centre
            // target. Transparent frontage remains physically solid.
            int visible = Visible(head.position) ? 1 : 0;
            visible += Visible(chest.position) ? 1 : 0;
            visible += Visible(chest.position - Vector3.up * .3f) ? 1 : 0;
            visible += Visible(chest.position + anatomy.ModelRoot.right * .13f) ? 1 : 0;
            visibleTime = visible >= 2 ? visibleTime + Mathf.Min(Time.deltaTime, .05f) : 0;
            if (visibleTime < .07f) return;
            ImpactPlayed = true; ImpactCount++;
            frameSound.PlayOneShot(frameClip);
        }

        private bool Visible(Vector3 point)
        {
            Vector3 v = view.WorldToViewportPoint(point);
            if (v.z <= 0 || v.z > 7.5f || v.x < .025f || v.x > .975f || v.y < .025f || v.y > .975f) return false;
            Vector3 ray = point - playerView.position;
            int count = Physics.RaycastNonAlloc(playerView.position, ray.normalized, visibilityHits,
                ray.magnitude - .02f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == visibilityHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var c = visibilityHits[i].collider;
                if (c == observationGlass || c == body || c.transform.IsChildOf(anatomy.transform)
                    || c.transform.IsChildOf(playerBody)) continue;
                return false;
            }
            return true;
        }

        private void PreparePose()
        {
            anatomy.Apply(new RecordedBodyPose(0, 0, 0, Vector2.zero));
            if (!anatomy.IsReady) return;
            Transform model = anatomy.ModelRoot;
            Vector3 left = model.InverseTransformPoint(anatomy.LeftFoot.position);
            Vector3 right = model.InverseTransformPoint(anatomy.RightFoot.position);
            left.x -= .018f; right.x += .018f;
            left.z += .035f; right.z -= .035f;
            left.y -= body.transform.position.y; right.y -= body.transform.position.y;
            var support = new RecordedLocomotion(left, right, Quaternion.Euler(0,-4,0), Quaternion.Euler(0,4,0),
                new Vector3(-.023f,-.035f,0), new Vector2(2.2f,3), 0);
            anatomy.Apply(new RecordedBodyPose(0, 0, 0, Vector2.zero, locomotion: support));
            Rotate(chest, model.up, 2);
            Rotate(neck, model.forward, -4.5f);
            Rotate(head, model.right, 7);
            Rotate(head, model.up, 2);
            if (upperEyelids != null) foreach (var lid in upperEyelids) if (lid) lid.position -= model.up * .0008f;
            float upper = Vector3.Distance(contactArm.position, contactForearm.position);
            float lower = Vector3.Distance(contactForearm.position, contactHand.position);
            Solve(contactArm, contactForearm, contactHand, frameGrip.position,
                contactArm.position - Vector3.up * .4f - model.right * .1f + model.forward * .04f, upper, lower);
            contactHand.rotation = frameGrip.rotation;
            posed = true;
        }

        private void OnDisable() { if (frameSound) frameSound.Stop(); }
        private static void Rotate(Transform joint, Vector3 axis, float angle) => joint.rotation = Quaternion.AngleAxis(angle, axis) * joint.rotation;
        private static void Solve(Transform upperBone, Transform lowerBone, Transform end, Vector3 target, Vector3 hint, float upper, float lower)
        {
            Vector3 origin = upperBone.position, offset = target - origin;
            float distance = Mathf.Clamp(offset.magnitude, Mathf.Abs(upper-lower)+.001f, upper+lower-.001f);
            Vector3 direction = offset.normalized, bend = Vector3.ProjectOnPlane(hint-origin,direction).normalized;
            if (bend.sqrMagnitude < .01f) bend = Vector3.ProjectOnPlane(Vector3.forward,direction).normalized;
            float along = (upper*upper+distance*distance-lower*lower)/(2*distance);
            Vector3 middle = origin+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            upperBone.rotation = Quaternion.FromToRotation(lowerBone.position-origin,middle-origin)*upperBone.rotation;
            lowerBone.rotation = Quaternion.FromToRotation(end.position-lowerBone.position,target-lowerBone.position)*lowerBone.rotation;
        }
    }
}

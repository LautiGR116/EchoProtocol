using UnityEngine;

namespace EchoProtocol.Campaign
{
    // Present actor only. The root follows a physical approach; contact, rather
    // than a presentation timer, starts the authored recoil and recovery.
    [DisallowMultipleComponent]
    public sealed class EmbestidaRush : MonoBehaviour
    {
        [SerializeField] Transform threshold, impactStand, impactHand, impactPanel;
        [SerializeField] AudioSource contact, effort, breathing;
        [SerializeField] MachineCoverThreat threat;
        Vector3 panelRest;
        Vector3 entryOrigin;
        float contactAt = -1, speed, preparation;
        bool crossed;
        public bool HasContact => contactAt >= 0;
        public float ContactAge => HasContact ? Time.time - contactAt : -1;
        public float ContactWeight => HasContact ? 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .65f, ContactAge)) : 0;
        public Transform Hand => impactHand;
        public float Preparation => preparation;
        public float HandWeight => HasContact ? ContactWeight : Mathf.SmoothStep(0, .92f, Mathf.InverseLerp(.85f, .18f, Vector3.Distance(transform.position, impactStand.position)));
        public float PursuitMultiplier => HasContact ? Mathf.Lerp(.58f, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.66f, 2.5f, ContactAge))) : 1;
        public bool ProtectReaction => !HasContact || ContactAge < 1.65f;
        public float MaskingGain => !HasContact ? .24f : Mathf.Lerp(.18f, 1, Mathf.SmoothStep(0, 1, ContactAge / .8f));
        public int Contacts { get; private set; }

        void Awake()
        {
            if (!threshold || !impactStand || !impactHand || !impactPanel || !contact || !effort || !breathing || !threat)
            { Debug.LogError("La embestida requires approach, physical contact and routed body sounds.", this); enabled = false; return; }
            panelRest = impactPanel.localPosition;
        }

        public void ResetAct()
        {
            contactAt = -1; speed = preparation = 0; crossed = false;
            if (impactPanel) impactPanel.localPosition = panelRest;
            foreach (var source in new[] { contact, effort, breathing }) if (source) source.Stop();
        }

        public void Prepare(float weight) => preparation = weight;
        public void Begin()
        {
            ResetAct(); entryOrigin = transform.position;
        }

        public bool Advance(float age, CharacterController controller)
        {
            if (!enabled) return true;
            float dt = Time.deltaTime;
            if (!HasContact)
            {
                // One shoulder clears the leaf, then the body accelerates across
                // the short diagonal into the steel beside the release lever.
                if (age < .24f) return false;
                var target = crossed ? impactStand.position : threshold.position;
                Vector3 d = target - transform.position; d.y = 0;
                speed = Mathf.MoveTowards(speed, crossed ? 5.4f : 4.5f, dt * 18);
                if (d.sqrMagnitude > .0001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), 340 * dt);
                controller.Move(Vector3.ClampMagnitude(d, speed * dt) + Vector3.down * 2 * dt);
                if (!crossed && d.magnitude < .10f) crossed = true;
                if (crossed && Vector3.Distance(new Vector3(transform.position.x, impactStand.position.y, transform.position.z), impactStand.position) < .09f)
                {
                    contactAt = Time.time; Contacts++; contact.Play();
                    // The metal owns the transient. The expelled effort follows
                    // the compressed chest, rather than announcing the reveal.
                    effort.PlayDelayed(.075f);
                }
                return false;
            }
            float a = ContactAge;
            // Short backwards yield of the whole mass, through collision. The
            // planted arm stays on the housing while the trailing foot catches up.
            if (a < .20f)
            {
                Vector3 recoil = entryOrigin - impactStand.position; recoil.y = 0;
                controller.Move(recoil.normalized * (.45f * (1 - a / .20f)) * dt);
            }
            Quaternion recover = Quaternion.LookRotation(impactStand.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, recover, 140 * dt);
            return a >= .66f;
        }

        void LateUpdate()
        {
            if (!enabled || Time.deltaTime <= 0) return;
            if (impactPanel)
            {
                float a = ContactAge;
                float displacement = a >= 0 && a < .42f ? Mathf.Sin(a * 63) * Mathf.Exp(-a * 13) * .017f : 0;
                impactPanel.localPosition = panelRest + Vector3.forward * displacement;
            }
            bool sustain = threat.Released && HasContact && ContactAge > .24f && !threat.IsCapturing && threat.Phase != MachineCoverThreat.Intention.Resolved;
            if (sustain && !breathing.isPlaying) breathing.Play();
            if (!sustain && breathing.isPlaying) breathing.Stop();
        }

        // Hand-authored compression / collision / weight recovery keys. World
        // hand and foot constraints are solved afterwards by the existing rig.
        public void Pose(Transform root, Transform hips, Transform spine, Transform chest,
            Transform neck, Transform head, Transform jaw, Transform leftArm, Transform rightArm,
            Transform leftForearm, Transform rightForearm, MachineCoverThreat.Intention phase, float age)
        {
            bool entry = phase == MachineCoverThreat.Intention.Entry;
            float hit = ContactWeight;
            float drive = entry && !HasContact ? Mathf.SmoothStep(0, 1, age / .38f) : 0;
            float pursuit = phase == MachineCoverThreat.Intention.Charge ? 1 : 0;
            // Permanent unequal compression belongs to this character, including
            // its silhouette through the door. It is not an Echo pose offset.
            hips.position += root.right * -.035f - root.up * (.085f + drive * .07f + hit * .12f);
            Turn(spine, root.right, 13 + drive * 21 + hit * 23);
            Turn(chest, root.forward, -11 - drive * 12 + hit * 17);
            Turn(chest, root.up, -8 + drive * 19 - hit * 16);
            Turn(neck, root.right, -9 - drive * 12 - hit * 14);
            Turn(head, root.forward, 7 + hit * -10);
            Turn(jaw, root.right, 4 + hit * 9);
            Turn(leftArm, root.right, -14 - pursuit * 12 - drive * 24);
            Turn(leftForearm, root.right, -38 - drive * 24);
            Turn(rightArm, root.forward, -13);
            Turn(rightForearm, root.right, -18 - hit * 35);
        }
        static void Turn(Transform t, Vector3 axis, float angle) => t.rotation = Quaternion.AngleAxis(angle, axis) * t.rotation;
        void OnDisable() { if (contact) contact.Stop(); if (effort) effort.Stop(); if (breathing) breathing.Stop(); }
    }
}

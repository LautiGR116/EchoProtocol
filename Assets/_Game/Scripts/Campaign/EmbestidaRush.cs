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
        [SerializeField] bool authoredActing;
        public bool UsesAuthoredActing => authoredActing;
        Vector3 panelRest;
        Vector3 entryOrigin;
        float contactAt = -1, abortedAt = -1, speed, preparation;
        float closestRemaining = float.PositiveInfinity, progressAt;
        bool crossed;
        public bool HasContact => contactAt >= 0;
        public bool ApproachAborted => abortedAt >= 0;
        public float ContactAge => HasContact ? Time.time - contactAt : -1;
        public float RecoveryAge => HasContact ? ContactAge : ApproachAborted ? Time.time - abortedAt : -1;
        public float ContactWeight => HasContact ? 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .65f, ContactAge)) : 0;
        public Transform Hand => impactHand;
        public float Preparation => preparation;
        public float HandWeight => ApproachAborted ? 0 : HasContact ? ContactWeight : Mathf.SmoothStep(0, .92f, Mathf.InverseLerp(.85f, .18f, PlanarDistance(transform.position, impactStand.position)));
        public float PursuitMultiplier => RecoveryAge >= 0 ? Mathf.Lerp(.58f, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.66f, 2.5f, RecoveryAge))) : 1;
        public bool ProtectReaction => RecoveryAge < 1.65f;
        public float MaskingGain => RecoveryAge < 0 ? .24f : Mathf.Lerp(.18f, 1, Mathf.SmoothStep(0, 1, RecoveryAge / .8f));
        public int Contacts { get; private set; }
        public int AbortedApproaches { get; private set; }

        void Awake()
        {
            if (!threshold || !impactStand || !impactHand || !impactPanel || !contact || !effort || !breathing || !threat)
            { Debug.LogError("La embestida requires approach, physical contact and routed body sounds.", this); enabled = false; return; }
            panelRest = impactPanel.localPosition;
        }

        public void ResetAct()
        {
            contactAt = abortedAt = -1; speed = preparation = 0; crossed = false;
            closestRemaining = float.PositiveInfinity; progressAt = Time.time;
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
            if (ApproachAborted) return true;
            float dt = Time.deltaTime;
            if (dt <= 0) return false;
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
                float remaining = PlanarDistance(transform.position, target);
                if (remaining < closestRemaining - .025f)
                { closestRemaining = remaining; progressAt = Time.time; }
                if (!crossed && remaining < .10f)
                {
                    crossed = true;
                    closestRemaining = float.PositiveInfinity; progressAt = Time.time;
                }
                if (crossed && PlanarDistance(transform.position, impactStand.position) < .09f)
                {
                    contactAt = Time.time; Contacts++; contact.Play();
                    // The metal owns the transient. The expelled effort follows
                    // the compressed chest, rather than announcing the reveal.
                    effort.PlayDelayed(.075f);
                }
                // A player or a moved collider can obstruct either authored point.
                // Yield to the collision-respecting pursuit instead of waiting
                // forever in Entry. An interrupted approach is NOT a steel hit:
                // no contact sound, panel shake or planted hand is fabricated.
                if (!HasContact && ((age >= 1.1f && Time.time - progressAt > .75f) || age >= 3.5f))
                {
                    abortedAt = Time.time; AbortedApproaches++;
                    speed = preparation = 0;
                    return true;
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
            bool sustain = threat.Released && RecoveryAge > .24f && !threat.IsCapturing && threat.Phase != MachineCoverThreat.Intention.Resolved;
            if (sustain && !breathing.isPlaying) breathing.Play();
            if (!sustain && breathing.isPlaying) breathing.Stop();
        }

        struct ActingPose
        {
            public Vector3 hip, torso, look, leftArm, rightArm;
            public Vector2 elbows;
            public static ActingPose Blend(ActingPose a, ActingPose b, float weight) => new ActingPose
            {
                hip = Vector3.Lerp(a.hip, b.hip, weight),
                torso = Vector3.Lerp(a.torso, b.torso, weight),
                look = Vector3.Lerp(a.look, b.look, weight),
                leftArm = Vector3.Lerp(a.leftArm, b.leftArm, weight),
                rightArm = Vector3.Lerp(a.rightArm, b.rightArm, weight),
                elbows = Vector2.Lerp(a.elbows, b.elbows, weight)
            };
        }

        // These are whole-pose keys, interpolated from the restored anatomy.
        // torso = spine pitch / chest yaw / chest roll; look counterbalances
        // through the neck while the performance retains its directed gaze.
        // World hand and foot constraints are solved afterwards by that rig.
        public void Pose(Transform root, Transform hips, Transform spine, Transform chest,
            Transform neck, Transform head, Transform jaw, Transform leftArm, Transform rightArm,
            Transform leftForearm, Transform rightForearm, MachineCoverThreat.Intention phase, float age,
            bool containedViolence = false)
        {
            if (!authoredActing)
            {
                LegacyPose(root, hips, spine, chest, neck, head, jaw, leftArm, rightArm,
                    leftForearm, rightForearm, phase, age);
                return;
            }
            if (containedViolence)
            {
                ContainedPose(root, hips, spine, chest, neck, head, jaw, leftArm, rightArm,
                    leftForearm, rightForearm, phase, age);
                return;
            }
            bool entry = phase == MachineCoverThreat.Intention.Entry;
            var quiet = new ActingPose
            {
                torso = new Vector3(3, -3, 0), look = new Vector3(-3, 0, 0),
                leftArm = new Vector3(-5, 0, 5), rightArm = new Vector3(3, 0, -3),
                elbows = new Vector2(-22, -11)
            };
            var drive = new ActingPose
            {
                hip = new Vector3(-.018f, -.045f, .014f),
                torso = new Vector3(25, 16, -11), look = new Vector3(-19, -3, 5),
                leftArm = new Vector3(-28, 0, 9), rightArm = new Vector3(-8, 0, -12),
                elbows = new Vector2(-64, -27)
            };
            var impact = new ActingPose
            {
                hip = new Vector3(.012f, -.075f, -.012f),
                torso = new Vector3(29, -11, 8), look = new Vector3(-23, 2, -4),
                leftArm = new Vector3(-14, 0, 10), rightArm = new Vector3(-9, 0, -7),
                elbows = new Vector2(-48, -53)
            };
            var settled = new ActingPose
            {
                torso = new Vector3(7, -5, -2), look = new Vector3(-5, 0, 1),
                leftArm = new Vector3(-12, 0, 6), rightArm = new Vector3(3, 0, -4),
                elbows = new Vector2(-37, -17)
            };
            bool pursuing = phase == MachineCoverThreat.Intention.Charge || phase == MachineCoverThreat.Intention.Reacquire;
            ActingPose pose = quiet;
            if (pursuing)
            {
                pose = settled;
                pose.torso.x = 13; pose.look.x = -9;
                pose.leftArm.x = -23; pose.rightArm.x = 7;
                pose.elbows = new Vector2(-53, -24);
            }
            else if (phase == MachineCoverThreat.Intention.MachineContact)
            {
                // One shoulder listens against the casing; its palm is still
                // placed by the existing reach-aware IK after this pose.
                pose = ActingPose.Blend(quiet, settled, Mathf.SmoothStep(0, 1, age / .22f));
                pose.torso.y = -9; pose.look.z = 3;
            }

            if (entry && !HasContact && !ApproachAborted)
            {
                // Load during the existing .24 s reaction hold, then send that
                // same diagonal through the doorway. The root remains owned
                // exclusively by Advance and its CharacterController.
                float load = Mathf.SmoothStep(0, 1, age / .19f);
                float launch = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.24f, .43f, age));
                pose = ActingPose.Blend(quiet, drive, load);
                pose.hip.z = Mathf.Lerp(-.022f * load, drive.hip.z, launch);
                pose.torso.x = Mathf.Lerp(17 * load + quiet.torso.x * (1 - load), drive.torso.x, launch);
            }
            else if (HasContact && ContactAge < 1.25f && phase != MachineCoverThreat.Intention.Capture)
            {
                // Contact first arrests the leading shoulder, then the trunk
                // yields and unwinds. Hold the recovered shape briefly instead
                // of immediately falling back into a generic running cycle.
                float a = ContactAge;
                ActingPose arrest = ActingPose.Blend(drive, impact, Mathf.SmoothStep(0, 1, a / .055f));
                ActingPose recover = ActingPose.Blend(arrest, settled,
                    Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .64f, a)));
                pose = ActingPose.Blend(recover, pose,
                    Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.86f, 1.25f, a)));
                float effortPulse = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.045f, .095f, a))
                    * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.14f, .29f, a)));
                Turn(jaw, root.right, effortPulse * 8);
            }
            else if (ApproachAborted && RecoveryAge < .32f && phase != MachineCoverThreat.Intention.Capture)
            {
                // An obstructed approach sheds the loaded posture without
                // impersonating the steel contact or opening the jaw.
                pose = ActingPose.Blend(drive, pose, Mathf.SmoothStep(0, 1, RecoveryAge / .32f));
            }

            ApplyKey(pose, root, hips, spine, chest, neck, head, leftArm, rightArm, leftForearm, rightForearm);
        }

        void ContainedPose(Transform root, Transform hips, Transform spine, Transform chest,
            Transform neck, Transform head, Transform jaw, Transform leftArm, Transform rightArm,
            Transform leftForearm, Transform rightForearm, MachineCoverThreat.Intention phase, float age)
        {
            var held = new ActingPose
            {
                hip = new Vector3(-.012f, -.065f, -.02f),
                torso = new Vector3(19, -12, -8), look = new Vector3(-15, 0, -3),
                leftArm = new Vector3(-7, 0, 8), rightArm = new Vector3(5, 0, -6),
                elbows = new Vector2(-55, -28)
            };
            var loaded = new ActingPose
            {
                hip = new Vector3(-.035f, -.12f, -.045f),
                torso = new Vector3(27, -20, -16), look = new Vector3(-23, 0, -5),
                leftArm = new Vector3(-6, 0, 12), rightArm = new Vector3(-23, 0, -14),
                elbows = new Vector2(-70, -48)
            };
            var drive = new ActingPose
            {
                hip = new Vector3(-.02f, -.085f, .025f),
                torso = new Vector3(37, 13, -14), look = new Vector3(-29, -3, -4),
                leftArm = new Vector3(-29, 0, 11), rightArm = new Vector3(-13, 0, -12),
                elbows = new Vector2(-72, -36)
            };
            var impact = new ActingPose
            {
                hip = new Vector3(.025f, -.14f, -.035f),
                torso = new Vector3(34, -15, 10), look = new Vector3(-27, 2, -5),
                leftArm = new Vector3(-18, 0, 12), rightArm = new Vector3(-12, 0, -9),
                elbows = new Vector2(-62, -58)
            };
            var recovered = new ActingPose
            {
                hip = new Vector3(.02f, -.095f, .005f),
                torso = new Vector3(24, -15, -9), look = new Vector3(-19, 0, -3),
                leftArm = new Vector3(-19, 0, 9), rightArm = new Vector3(9, 0, -7),
                elbows = new Vector2(-61, -39)
            };
            bool entry = phase == MachineCoverThreat.Intention.Entry;
            bool pursuing = phase == MachineCoverThreat.Intention.Charge || phase == MachineCoverThreat.Intention.Reacquire;
            ActingPose pose = held;
            if (pursuing)
            {
                pose = recovered;
                pose.torso.x = 27; pose.torso.y = -10; pose.look.x = -22;
                pose.leftArm.x = -27; pose.rightArm.x = 13;
            }
            else if (phase == MachineCoverThreat.Intention.MachineContact)
            {
                pose = ActingPose.Blend(held, recovered, Mathf.SmoothStep(0, 1, age / .22f));
                pose.torso.y = -17;
            }
            if (entry && !HasContact && !ApproachAborted)
            {
                // Load quickly, hold the weight, then release it through the
                // existing collision-driven travel. No faster pursuit/reaction.
                ActingPose coil = ActingPose.Blend(held, loaded, Mathf.SmoothStep(0, 1, age / .11f));
                pose = ActingPose.Blend(coil, drive,
                    Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.24f, .39f, age)));
            }
            else if (HasContact && ContactAge < 1.25f && phase != MachineCoverThreat.Intention.Capture)
            {
                float a = ContactAge;
                ActingPose arrest = ActingPose.Blend(drive, impact, Mathf.SmoothStep(0, 1, a / .065f));
                ActingPose recover = ActingPose.Blend(arrest, recovered,
                    Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .60f, a)));
                pose = ActingPose.Blend(recover, pose,
                    Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.90f, 1.25f, a)));
                // Brief clenched effort, not a sustained open-mouth expression.
                float effortPulse = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.045f, .095f, a))
                    * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.14f, .26f, a)));
                Turn(jaw, root.right, effortPulse * 4);
            }
            else if (ApproachAborted && RecoveryAge < .32f && phase != MachineCoverThreat.Intention.Capture)
                pose = ActingPose.Blend(drive, pose, Mathf.SmoothStep(0, 1, RecoveryAge / .32f));
            ApplyKey(pose, root, hips, spine, chest, neck, head, leftArm, rightArm, leftForearm, rightForearm);
        }

        static void ApplyKey(ActingPose pose, Transform root, Transform hips, Transform spine, Transform chest,
            Transform neck, Transform head, Transform leftArm, Transform rightArm,
            Transform leftForearm, Transform rightForearm)
        {
            hips.position += root.right * pose.hip.x + root.up * pose.hip.y + root.forward * pose.hip.z;
            Turn(spine, root.right, pose.torso.x);
            Turn(chest, root.up, pose.torso.y); Turn(chest, root.forward, pose.torso.z);
            Turn(neck, root.right, pose.look.x); Turn(neck, root.up, pose.look.y);
            Turn(head, root.forward, pose.look.z);
            Turn(leftArm, root.right, pose.leftArm.x); Turn(leftArm, root.forward, pose.leftArm.z);
            Turn(rightArm, root.right, pose.rightArm.x); Turn(rightArm, root.forward, pose.rightArm.z);
            Turn(leftForearm, root.right, pose.elbows.x); Turn(rightForearm, root.right, pose.elbows.y);
        }
        void LegacyPose(Transform root, Transform hips, Transform spine, Transform chest,
            Transform neck, Transform head, Transform jaw, Transform leftArm, Transform rightArm,
            Transform leftForearm, Transform rightForearm, MachineCoverThreat.Intention phase, float age)
        {
            bool entry = phase == MachineCoverThreat.Intention.Entry;
            float hit = ContactWeight;
            float drive = entry && !HasContact && !ApproachAborted ? Mathf.SmoothStep(0, 1, age / .38f) : 0;
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
        static float PlanarDistance(Vector3 a, Vector3 b)
        { a.y = b.y; return Vector3.Distance(a, b); }
        static void Turn(Transform t, Vector3 axis, float angle) => t.rotation = Quaternion.AngleAxis(angle, axis) * t.rotation;
        void OnDisable() { if (contact) contact.Stop(); if (effort) effort.Stop(); if (breathing) breathing.Stop(); }
    }
}

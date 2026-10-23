using System;
using EchoProtocol.Echo;
using EchoProtocol.Campaign.IntruderActing;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign.P3Intrusion
{
    // Called ONLY by IntruderStaging's P3 branch. No Update, recording access,
    // pursuit, camera write or independent pose writer. The pump retains its actor.
    [DisallowMultipleComponent]
    public sealed class P3BodyIntrusion : MonoBehaviour
    {
        [Serializable]
        public sealed class Limb
        {
            public Transform upper, lower, end;
            [NonSerialized] public float a, b;
            [NonSerialized] public Vector3 start, rest;
            [NonSerialized] public Quaternion rotation;
            public bool Valid => upper && lower && end;
            public void Capture(Transform actor)
            {
                a = Vector3.Distance(upper.position, lower.position);
                b = Vector3.Distance(lower.position, end.position);
                start = end.position; rest = actor.InverseTransformPoint(start); rotation = end.rotation;
            }
            public void Place(Vector3 point, Vector3 hint)
            {
                Vector3 origin = upper.position, offset = point - origin;
                float distance = Mathf.Clamp(offset.magnitude, Mathf.Abs(a - b) + .001f, a + b - .001f);
                Vector3 direction = offset.sqrMagnitude > .000001f ? offset.normalized : Vector3.down;
                Vector3 bend = Vector3.ProjectOnPlane(hint - origin, direction).normalized;
                if (bend.sqrMagnitude < .01f) bend = Vector3.ProjectOnPlane(Vector3.forward, direction).normalized;
                float along = (a * a + distance * distance - b * b) / (2 * distance);
                Vector3 middle = origin + direction * along + bend * Mathf.Sqrt(Mathf.Max(0, a * a - along * along));
                upper.rotation = Quaternion.FromToRotation(lower.position - origin, middle - origin) * upper.rotation;
                lower.rotation = Quaternion.FromToRotation(end.position - lower.position, point - lower.position) * lower.rotation;
            }
        }

        [Serializable]
        public sealed class Palm
        {
            public Limb arm = new Limb();
            public Transform target, index, middle, little;
            public Transform[] digits;
            public bool left;
            Quaternion orientation;
            public bool Valid => arm.Valid && target && index && middle && little;
            public void Capture(Transform actor)
            {
                arm.Capture(actor);
                Vector3 fingers = arm.end.InverseTransformDirection(middle.position - arm.end.position).normalized;
                Vector3 across = arm.end.InverseTransformDirection(index.position - little.position).normalized;
                Vector3 normal = (left ? Vector3.Cross(across, fingers) : Vector3.Cross(fingers, across)).normalized;
                orientation = Quaternion.Inverse(Quaternion.LookRotation(normal, fingers));
            }
            public void Place(float weight, Vector3 hint)
            {
                arm.Place(Vector3.Lerp(arm.end.position, target.position, weight), hint);
                arm.end.rotation = Quaternion.Slerp(arm.end.rotation, target.rotation * orientation, weight);
                // Retain the scan's spread, flatten curled fingers against glass.
                if (digits == null) return;
                foreach (var digit in digits)
                {
                    if (!digit || digit.childCount == 0) continue;
                    Vector3 direction = digit.GetChild(0).position - digit.position;
                    Vector3 flat = Vector3.ProjectOnPlane(direction, target.forward);
                    if (flat.sqrMagnitude > .000001f)
                        digit.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(direction, flat), weight) * digit.rotation;
                }
            }

            public void Press(float weight, Vector3 hint, float roll)
            {
                Place(weight, hint);
                // Present the side of a compact hand, bearing through its heel.
                // Turn during the reach, never twist a planted palm afterwards.
                arm.end.rotation = Quaternion.AngleAxis(roll * weight, target.forward) * arm.end.rotation;
                if (digits == null || middle.childCount == 0) return;
                Vector3 along = Vector3.ProjectOnPlane(middle.GetChild(0).position - middle.position, target.forward).normalized;
                if (along.sqrMagnitude < .01f) return;
                foreach (var digit in digits)
                {
                    if (!digit || digit.childCount == 0 || !digit.name.Contains("_01_")) continue;
                    Vector3 direction = digit.GetChild(0).position - digit.position;
                    float gather = digit.name.Contains("thumb") ? .18f : .82f;
                    digit.rotation = Quaternion.Slerp(Quaternion.identity,
                        Quaternion.FromToRotation(direction, along), weight * gather) * digit.rotation;
                }
            }
        }

        [SerializeField] IntruderStaging owner;
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] MachineCoverThreat threat;
        [SerializeField] MachineThreatPerformance performance;
        [SerializeField] CharacterController controller;
        [SerializeField] RecordedCharacterMotion anatomy;
        [SerializeField] Transform hips, spine, chest, neck, head, jaw, stop;
        [SerializeField] Transform[] brows, mouthCorners;
        [SerializeField] IntruderActingFocus acting;
        [SerializeField] Transform viewer;
        [SerializeField] BoxCollider glazing;
        [SerializeField] Limb leftLeg = new Limb(), rightLeg = new Limb();
        [SerializeField] Palm highPalm = new Palm(), lowPalm = new Palm { left = true };
        [SerializeField] AudioSource contact, support;
        [SerializeField, Min(.1f)] float recognitionSeconds = .32f;
        [SerializeField, Min(.2f)] float advanceSeconds = .46f;

        bool Authored => acting && acting.isActiveAndEnabled;
        bool Contained => Authored && acting.UsesContainedViolence;
        float CompressSeconds => Contained ? .26f : Authored ? .18f : .12f;
        float RecoverSeconds => Contained ? .62f : Authored ? .68f : .48f;
        Transform actor;
        Transform[] joints;
        Vector3[] positions;
        Quaternion[] rotations;
        Vector3 origin;
        Quaternion originRotation;
        float visibleFor, age, contactAge, actualProgress;
        bool triggered, contacted, blocked, supportPlayed, reportedInvalid;
        public bool OwnsPose { get; private set; }
        public bool HasContact => contacted;
        public bool ApproachBlocked => blocked;
        public bool Settled => contacted && contactAge >= RecoverSeconds;
        public int Contacts { get; private set; }
        public float RightPalmError => highPalm.Valid ? Vector3.Distance(highPalm.arm.end.position, highPalm.target.position) : float.PositiveInfinity;
        public float LeftPalmError => lowPalm.Valid ? Vector3.Distance(lowPalm.arm.end.position, lowPalm.target.position) : float.PositiveInfinity;
        public float ContactRightPalmError { get; private set; }
        public float ContactLeftPalmError { get; private set; }
        public float MaxSupportError { get; private set; }
        public bool BraceReleased => Authored && contacted && contactAge >= (Contained ? .42f : .55f);
        public bool IsConfigured => owner && session && encounter && threat && performance && controller && anatomy
            && anatomy.IsPresentThreat && anatomy.ModelRoot && hips && spine && chest && neck && head && jaw && stop
            && (!acting || (acting.IsConfigured && viewer && acting.transform == owner.transform))
            && glazing && glazing.enabled && !glazing.isTrigger && highPalm.Valid && lowPalm.Valid && leftLeg.Valid && rightLeg.Valid
            && contact && contact.clip && support && support.clip && contact.outputAudioMixerGroup && support.outputAudioMixerGroup
            && !contact.loop && !support.loop && !contact.ignoreListenerPause && !support.ignoreListenerPause
            && !contact.ignoreListenerVolume && !support.ignoreListenerVolume && contact.spatialBlend == 1 && support.spatialBlend == 1
            && controller.transform == owner.transform && threat.transform == owner.transform
            && anatomy.transform.IsChildOf(owner.transform) && !anatomy.GetComponentInParent<EchoPlayback>();

        bool AcquirePose()
        {
            if (!isActiveAndEnabled || !encounter || !threat || encounter.Active || threat.Released) return false;
            if (!IsConfigured)
            {
                if (!reportedInvalid) Debug.LogError("P3 intrusion needs its one present actor, solid glass, explicit rig and routed sources. Retaining the prior P3 presentation.", this);
                reportedInvalid = true; return false;
            }
            // MachineThreatPerformance is call-driven. Staging stops calling its
            // Witness pose for the whole lease; the threat cannot run until pump.
            performance.ResetPerformance();
            anatomy.SetPresentIntent(-1); anatomy.Apply(default(RecordedBodyPose));
            if (!anatomy.IsReady) return false;
            actor = owner.transform; joints = anatomy.Joints;
            positions = new Vector3[joints.Length]; rotations = new Quaternion[joints.Length];
            for (int i = 0; i < joints.Length; i++) { positions[i] = joints[i].localPosition; rotations[i] = joints[i].localRotation; }
            origin = actor.position; originRotation = actor.rotation;
            leftLeg.Capture(actor); rightLeg.Capture(actor); highPalm.Capture(actor); lowPalm.Capture(actor);
            if (acting) acting.CaptureNeutral(actor, head);
            OwnsPose = true;
            ApplyPose(0, 0);
            return true;
        }

        // Seeing the real return at A is sufficient. Never waits for the take's
        // ending or changes it. After launch looking away cannot postpone a hit.
        public bool Present(float delta, bool visible, bool actualEchoAtA)
        {
            if (!isActiveAndEnabled || !encounter || !threat || encounter.Active || threat.Released) { ReleasePose(); return false; }
            if (!OwnsPose && !AcquirePose()) return false;
            if (session.IsPaused || delta <= 0) return true;
            if (!triggered)
            {
                visibleFor = visible && actualEchoAtA ? visibleFor + delta : 0;
                if (visibleFor >= recognitionSeconds) { triggered = true; age = 0; }
            }
            else age += delta;

            float travelAge = Mathf.Max(0, age - CompressSeconds);
            if (triggered && !contacted && !blocked && age >= CompressSeconds)
            {
                float u = Ease(travelAge / advanceSeconds);
                Vector3 target = Vector3.Lerp(origin, stop.position, u);
                // Leave the table lamp beside the original stand before moving
                // diagonally toward the viewer. No collider is disabled/ignored.
                target.z = Mathf.Lerp(origin.z, stop.position.z, Ease(Mathf.InverseLerp(.28f, 1f, u)));
                Vector3 d = target - actor.position; d.y = 0;
                controller.Move(d + Vector3.down * 2 * delta);
                float total = Vector3.Distance(new Vector3(origin.x, 0, origin.z), new Vector3(stop.position.x, 0, stop.position.z));
                actualProgress = Mathf.Clamp01(Vector3.Distance(new Vector3(actor.position.x, 0, actor.position.z), new Vector3(origin.x, 0, origin.z)) / Mathf.Max(.001f, total));
                // An obstruction is a stopped approach, never a fake glass hit.
                if (travelAge >= advanceSeconds + .22f && PlanarDistance(actor.position, stop.position) > .055f) blocked = true;
            }
            if (contacted) contactAge += delta;
            ApplyPose(travelAge, triggered ? Ease(age / CompressSeconds) : 0);

            if (triggered && !blocked && !supportPlayed && travelAge >= advanceSeconds * .82f && actualProgress > .85f)
            {
                supportPlayed = true; support.transform.position = rightLeg.end.position; support.Play();
            }
            if (triggered && !contacted && !blocked && travelAge >= advanceSeconds
                && PlanarDistance(actor.position, stop.position) <= .055f && PalmsReached())
            {
                ContactRightPalmError = RightPalmError; ContactLeftPalmError = LeftPalmError;
                contacted = true; contactAge = 0; Contacts++;
                contact.transform.position = highPalm.target.position; contact.Play();
            }
            if (triggered && !contacted && travelAge > advanceSeconds + .3f) blocked = true;
            return true;
        }

        bool PalmsReached()
        {
            return glazing.enabled && Vector3.Distance(highPalm.arm.end.position, highPalm.target.position) < .045f
                && Vector3.Distance(lowPalm.arm.end.position, lowPalm.target.position) < .045f
                && Vector3.Distance(glazing.ClosestPoint(highPalm.target.position), highPalm.target.position) < .055f
                && Vector3.Distance(glazing.ClosestPoint(lowPalm.target.position), lowPalm.target.position) < .055f;
        }

        void ApplyPose(float travelAge, float compress)
        {
            if (Contained) { ApplyContainedPose(); return; }
            if (Authored) { ApplyAuthoredPose(compress); return; }
            ApplyLegacyPose(travelAge, compress);
        }

        void ApplyContainedPose()
        {
            RestoreJoints();
            float progress = actualProgress;
            float charge = triggered ? Ease(age / .12f) : 0;
            float load = charge * (1 - Ease(progress / .30f));
            float drive = Ease(progress / .22f) * (1 - Ease(Mathf.InverseLerp(.60f, 1, progress)));
            float hit = contacted ? Ease(contactAge / .045f) * (1 - Ease(Mathf.InverseLerp(.06f, .22f, contactAge))) : 0;
            float bearing = contacted ? Ease(Mathf.InverseLerp(.18f, RecoverSeconds, contactAge)) : 0;
            float release = contacted ? Ease(Mathf.InverseLerp(.18f, .42f, contactAge)) : 0;
            // Three legible shapes: coiled stillness, a shoulder-led invasion,
            // then weight kept against the glass. The actual contact posture
            // retains the proven shoulder origin and both wrist targets.
            float contactShape = Ease(Mathf.InverseLerp(.65f, 1, progress));
            hips.position += actor.right * Mathf.Lerp(-.03f, .035f, bearing)
                - actor.up * (.065f + .045f * charge + .035f * load + .022f * hit + .018f * bearing)
                + actor.forward * (-.04f * load + .015f * bearing);
            Turn(spine, actor.right, Mathf.Lerp(20, 13, contactShape) + 10 * load + 19 * drive + 8 * hit + 22 * bearing);
            Turn(chest, actor.forward, -9 - 7 * load + 6 * hit - 4 * bearing);
            Turn(chest, actor.up, -9 - 12 * load + 12 * drive - 9 * bearing);
            Turn(neck, actor.right, -12 - 12 * load - 13 * drive - 8 * hit - 15 * bearing);
            Turn(head, actor.forward, -4 - 3 * load + 2 * bearing);
            Turn(jaw, actor.right, 3 * hit);
            Turn(highPalm.arm.upper, actor.right, 7 - 37 * drive);
            Turn(highPalm.arm.lower, actor.right, -42 - 20 * load);
            Turn(lowPalm.arm.upper, actor.right, 8 - 13 * drive);
            Turn(lowPalm.arm.lower, actor.right, -26 - 20 * load);
            AuthoredFoot(rightLeg, 0, .76f, .12f, .11f);
            AuthoredFoot(leftLeg, .17f, 1, -.18f, .065f);
            float first = blocked ? 0 : Ease(Mathf.InverseLerp(.38f, .96f, progress));
            float brace = blocked ? 0 : Ease(Mathf.InverseLerp(.74f, 1, progress));
            if (first > 0)
                highPalm.Press(first, highPalm.arm.upper.position + actor.right * .24f - actor.up * .42f, -78);
            if (brace > 0)
            {
                Vector3 hint = lowPalm.arm.upper.position - actor.right * .26f - actor.up * .42f;
                lowPalm.Press(brace, hint, 48);
                if (release > 0)
                {
                    Vector3 withdrawn = actor.TransformPoint(new Vector3(-.23f, .86f, .12f));
                    lowPalm.arm.Place(Vector3.Lerp(lowPalm.target.position, withdrawn, release), hint);
                    lowPalm.arm.end.rotation = Quaternion.Slerp(lowPalm.arm.end.rotation, lowPalm.arm.rotation, release);
                }
            }
            int beat = !triggered ? 51 : !contacted ? 52 : contactAge < .22f ? 53 : 54;
            acting.Pose(actor, neck, head, viewer.position, beat, Time.deltaTime,
                contacted ? .14f : .08f, !triggered || Settled);
        }

        void ApplyAuthoredPose(float compress)
        {
            RestoreJoints();
            float progress = actualProgress;
            float release = contacted ? Ease(Mathf.InverseLerp(.16f, .55f, contactAge)) : 0;
            float settle = contacted ? Ease(contactAge / RecoverSeconds) : 0;
            float bearing = contacted ? Ease(Mathf.InverseLerp(.24f, RecoverSeconds, contactAge)) : 0;
            // Load holds both soles. One diagonal drive ends at the old proven
            // contact posture, then changes to a one-hand hold. No cyclic motion.
            float load = compress * (1 - Ease(progress / .34f));
            float drive = Ease(progress / .26f) * (1 - Ease(Mathf.InverseLerp(.58f, 1, progress)));
            float impactYield = contacted ? Ease(contactAge / .055f) * (1 - Ease(Mathf.InverseLerp(.055f, .24f, contactAge))) : 0;
            hips.position += actor.right * Mathf.Lerp(.025f, -.03f, Mathf.Max(compress, progress))
                - Vector3.up * (.065f + .045f * compress + .018f * load + .018f * impactYield - .025f * settle + .02f * bearing);
            // Stay loaded into the planted right hand after the left releases.
            // Only the recovered hold changes; contact/targets/root stay intact.
            Turn(spine, actor.right, 13 + 9 * load + 15 * drive + 5 * impactYield - 3 * settle + 13 * bearing);
            Turn(chest, actor.forward, -9 - 6 * load + 3 * impactYield);
            Turn(chest, actor.up, -9 - 10 * load + 10 * drive - 2 * settle - 6 * bearing);
            Turn(neck, actor.right, -12 - 8 * load - 10 * drive - 4 * impactYield - 10 * bearing);
            Turn(head, actor.forward, 5 + 4 * load - 4 * settle);
            Turn(head, actor.right, 3 - 3 * settle + 2 * bearing);
            // A single expelled effort; the mouth returns to authored neutral.
            Turn(jaw, actor.right, 5 * impactYield);
            Turn(highPalm.arm.upper, actor.right, -8 - 32 * drive);
            Turn(highPalm.arm.lower, actor.right, -22 - 19 * load);
            Turn(lowPalm.arm.upper, actor.right, 5 - 9 * drive);
            Turn(lowPalm.arm.lower, actor.right, -8 - 12 * load);
            if (brows != null) for (int i = 0; i < brows.Length; i++) if (brows[i])
                brows[i].position -= actor.up * ((i == 0 ? .002f : .001f) * Mathf.Max(load, impactYield));

            // Actual root travel drives two unequal support arcs. If collision
            // arrests the body, the feet stop advancing toward the remote stop.
            AuthoredFoot(rightLeg, 0, .76f, .12f, .11f);
            AuthoredFoot(leftLeg, .17f, 1, -.18f, .065f);
            float first = blocked ? 0 : Ease(Mathf.InverseLerp(.38f, .96f, progress));
            float brace = blocked ? 0 : Ease(Mathf.InverseLerp(.74f, 1, progress));
            if (first > 0)
            {
                highPalm.Place(first, highPalm.arm.upper.position + actor.right * .30f - actor.up * .30f);
                // Rotate within the glass plane, not away from it: a diagonal
                // brace instead of an upright greeting. The wrist stays pinned.
                Turn(highPalm.arm.end, highPalm.target.forward, -28 * bearing);
            }
            if (brace > 0)
            {
                Vector3 hint = lowPalm.arm.upper.position - actor.right * .26f - actor.up * .42f;
                lowPalm.Place(brace, hint);
                if (release > 0)
                {
                    // The left wrist has already made real contact. It peels off
                    // and falls beside the lower ribs while the right stays planted.
                    Vector3 rest = actor.TransformPoint(new Vector3(-.24f, 1.01f, .12f));
                    lowPalm.arm.Place(Vector3.Lerp(lowPalm.target.position, rest, release), hint);
                    lowPalm.arm.end.rotation = Quaternion.Slerp(lowPalm.arm.end.rotation, lowPalm.arm.rotation, release);
                }
            }
            int beat = !triggered ? 1 : !contacted ? 2 : 3;
            acting.Pose(actor, neck, head, viewer.position, beat, Time.deltaTime,
                contacted ? .16f : .035f, !triggered || Settled, -5 * bearing);
        }

        void AuthoredFoot(Limb leg, float from, float until, float stagger, float lift)
        {
            float u = triggered ? Mathf.InverseLerp(from, until, actualProgress) : 0;
            Vector3 destination = stop.position + originRotation * leg.rest + stop.forward * stagger;
            destination.y = leg.start.y;
            Vector3 point = Vector3.Lerp(leg.start, destination, Ease(u));
            point.y += 4 * u * (1 - u) * lift;
            if (blocked) point = actor.TransformPoint(leg.rest);
            leg.Place(point, leg.upper.position + actor.forward * .65f);
            leg.end.rotation = leg.rotation;
            if (u <= 0 || u >= 1) MaxSupportError = Mathf.Max(MaxSupportError, Vector3.Distance(leg.end.position, point));
        }

        void ApplyLegacyPose(float travelAge, float compress)
        {
            RestoreJoints();
            float drive = triggered && !contacted && !blocked ? Mathf.Sin(Mathf.PI * Mathf.Clamp01(travelAge / advanceSeconds)) : 0;
            float hit = contacted ? 1 - Ease(contactAge / .21f) : 0;
            float settle = contacted ? Ease(contactAge / RecoverSeconds) : 0;
            float reach = blocked ? 0 : Ease(Mathf.InverseLerp(.32f, .98f, actualProgress));
            hips.position += actor.right * -.03f - Vector3.up * (.065f + .045f * compress + .04f * hit - .025f * settle);
            Turn(spine, actor.right, 13 + 14 * drive + 9 * hit - 4 * settle);
            Turn(chest, actor.forward, -9 + 7 * hit);
            Turn(chest, actor.up, -9 + 12 * drive - 4 * settle);
            Turn(neck, actor.right, -12 - 7 * drive - 9 * hit);
            Turn(head, actor.forward, 9 - 5 * hit);
            Turn(head, actor.right, 5 - 7 * settle);
            Turn(jaw, actor.right, 3 + 7 * hit + 2 * settle);
            Turn(highPalm.arm.upper, actor.right, -18 - 30 * drive);
            Turn(highPalm.arm.lower, actor.right, -38);
            Turn(lowPalm.arm.upper, actor.right, -12 - 18 * drive);
            Turn(lowPalm.arm.lower, actor.right, -25);
            if (mouthCorners != null) foreach (var corner in mouthCorners) if (corner) corner.position -= Vector3.up * .007f;
            if (brows != null) foreach (var brow in brows) if (brow) brow.position -= Vector3.up * .004f;

            // One lead support and a trailing catch, not a repeating walk cycle.
            // Targets are world-space; the stationary foot does not skate with root.
            Foot(rightLeg, travelAge, 0, advanceSeconds * .82f, .14f, .14f);
            Foot(leftLeg, travelAge, advanceSeconds * .28f, advanceSeconds + .13f, -.20f, .09f);
            if (reach > 0)
            {
                highPalm.Place(reach, highPalm.arm.upper.position + actor.right * .35f - Vector3.up * .27f);
                lowPalm.Place(reach, lowPalm.arm.upper.position - actor.right * .32f - Vector3.up * .40f);
            }
        }

        void Foot(Limb leg, float time, float from, float until, float stagger, float lift)
        {
            float u = triggered ? Mathf.InverseLerp(from, until, time) : 0;
            Vector3 destination = stop.position + originRotation * leg.rest + stop.forward * stagger;
            destination.y = leg.start.y;
            // A blocked approach settles under its actual body, not at a remote
            // authored target. No stretched legs through an intervening object.
            if (blocked) destination = actor.TransformPoint(leg.rest);
            Vector3 point = Vector3.Lerp(leg.start, destination, Ease(u));
            if (!blocked) point.y += Mathf.Sin(u * Mathf.PI) * lift;
            leg.Place(point, leg.upper.position + actor.forward * .65f);
            leg.end.rotation = leg.rotation;
        }

        void RestoreJoints()
        {
            if (joints == null) return;
            for (int i = 0; i < joints.Length; i++) if (joints[i])
            { joints[i].localPosition = positions[i]; joints[i].localRotation = rotations[i]; }
        }

        public void ReleasePose()
        {
            if (contact) contact.Stop(); if (support) support.Stop();
            if (!OwnsPose) return;
            OwnsPose = false;
            RestoreJoints();
            // Do NOT restore actor/world position: ResetThreat already placed it
            // at the pump before calling ResetPump. Reset feet at that new place.
            if (performance) performance.ResetPerformance();
        }
        void OnDisable() => ReleasePose();
        static float Ease(float t) => Mathf.SmoothStep(0, 1, Mathf.Clamp01(t));
        static float PlanarDistance(Vector3 a, Vector3 b) { a.y = b.y; return Vector3.Distance(a, b); }
        static void Turn(Transform joint, Vector3 axis, float degrees) => joint.rotation = Quaternion.AngleAxis(degrees, axis) * joint.rotation;
    }
}

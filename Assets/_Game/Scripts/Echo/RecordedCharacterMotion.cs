using UnityEngine;

namespace EchoProtocol.Echo
{
    // Clip time, weight, foot placement and involuntary movement come only from
    // explicitly captured presentation state. No Animator, world clock, gaze
    // target, physics query, random choice or live input runs on an Echo.
    [DisallowMultipleComponent]
    public sealed class RecordedCharacterMotion : MonoBehaviour
    {
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Transform[] joints;
        [SerializeField] private AnimationClip quietStance, walkingCycle;
        [SerializeField] private Transform hips, spine, chest, neck, head;
        [SerializeField] private Transform jaw;
        [SerializeField] private Transform[] upperEyelids, lowerEyelids;
        [SerializeField] private Transform leftThigh, leftShin, leftFoot, rightThigh, rightShin, rightFoot;
        [SerializeField] private Transform leftArm, leftForearm, leftHand, rightArm, rightForearm, rightHand;
        [SerializeField] private Transform[] rightGripFingers;
        [SerializeField] private bool presentThreat;
        Quaternion[] quietRotations;
        Vector3[] quietPositions;
        Vector3 leftContact, rightContact, rootPosition;
        Quaternion leftFootOrientation, rightFootOrientation, rootRotation;
        float leftUpperLength, leftLowerLength, rightUpperLength, rightLowerLength;
        float threatIntent, threatAge;
        bool ready;
        RecordedBodyPose sampledPose;
        public bool IsReady => ready;
        public bool IsPresentThreat => presentThreat;
        public Transform ModelRoot => modelRoot;
        public Transform LeftFoot => leftFoot;
        public Transform RightFoot => rightFoot;
        public Transform[] Joints => joints;

        public static float StrideForSpeed(float speed)
            => Mathf.Lerp(1.46f, 2.32f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.7f, 3.5f, speed)));

        void Awake() => Initialize();
        public bool Initialize()
        {
            if (ready) return true;
            if (!modelRoot || !quietStance || !walkingCycle || joints == null || joints.Length < 16 ||
                !hips || !spine || !chest || !neck || !head || !leftThigh || !leftShin || !leftFoot ||
                !rightThigh || !rightShin || !rightFoot || !leftArm || !leftForearm || !leftHand ||
                !rightArm || !rightForearm || !rightHand)
            { Debug.LogError("Recorded character requires its authored clips and explicit anatomy references.", this); enabled = false; return false; }
            foreach (var joint in joints) if (!joint)
            { Debug.LogError("Recorded character has an unassigned pose joint.", this); enabled = false; return false; }
            rootPosition = modelRoot.localPosition; rootRotation = modelRoot.localRotation;
            quietStance.SampleAnimation(modelRoot.gameObject, 0);
            quietPositions = new Vector3[joints.Length]; quietRotations = new Quaternion[joints.Length];
            for (int i = 0; i < joints.Length; i++) { quietPositions[i] = joints[i].localPosition; quietRotations[i] = joints[i].localRotation; }
            leftContact = modelRoot.InverseTransformPoint(leftFoot.position); rightContact = modelRoot.InverseTransformPoint(rightFoot.position);
            leftFootOrientation = Quaternion.Inverse(modelRoot.rotation) * leftFoot.rotation;
            rightFootOrientation = Quaternion.Inverse(modelRoot.rotation) * rightFoot.rotation;
            leftUpperLength = Vector3.Distance(leftThigh.position, leftShin.position);
            leftLowerLength = Vector3.Distance(leftShin.position, leftFoot.position);
            rightUpperLength = Vector3.Distance(rightThigh.position, rightShin.position);
            rightLowerLength = Vector3.Distance(rightShin.position, rightFoot.position);
            ready = true; return true;
        }

        public void SetPresentIntent(float releaseAge)
        {
            if (!presentThreat) return;
            threatAge = Mathf.Max(0, releaseAge);
            threatIntent = releaseAge < 0 ? 0 : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .85f, releaseAge));
        }

        public void Apply(RecordedBodyPose pose)
        {
            if (!enabled || !Initialize()) return;
            sampledPose = pose;
            for (int i = 0; i < joints.Length; i++) { joints[i].localPosition = quietPositions[i]; joints[i].localRotation = quietRotations[i]; }
            modelRoot.localPosition = rootPosition; modelRoot.localRotation = rootRotation;
            if (!pose.Captured) return;
            float speed = pose.Velocity.magnitude;
            bool supported = pose.Locomotion.Captured && !presentThreat;
            float moving = supported ? pose.Locomotion.Blend : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.025f, .45f, speed));
            float running = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.7f, 3.5f, speed));
            float cycle = Mathf.Repeat(pose.GaitPhase / (Mathf.PI * 2), 1);
            walkingCycle.SampleAnimation(modelRoot.gameObject, cycle * walkingCycle.length);
            for (int i = 0; i < joints.Length; i++)
            {
                joints[i].localPosition = Vector3.Lerp(quietPositions[i], joints[i].localPosition, moving);
                joints[i].localRotation = Quaternion.Slerp(quietRotations[i], joints[i].localRotation, moving);
            }
            modelRoot.localPosition = rootPosition; modelRoot.localRotation = rootRotation;
            if (supported)
            {
                ApplyCapturedSupport(pose, moving, running);
                return;
            }
            Vector2 travel = speed > .01f ? pose.Velocity / speed : Vector2.up;
            float breath = Mathf.Sin(pose.BreathPhase);
            hips.position -= modelRoot.up * ((.025f + .055f * running) * moving + .017f * threatIntent);
            float legHeading = Mathf.Atan2(travel.x, Mathf.Max(.4f, Mathf.Abs(travel.y))) * Mathf.Rad2Deg * .5f * moving;
            AddRotation(hips, modelRoot.up, legHeading);
            AddRotation(spine, modelRoot.up, -legHeading);
            AddRotation(hips, modelRoot.forward, -pose.Velocity.x * .65f);
            AddRotation(spine, modelRoot.right, breath * .35f + running * moving * 5 + (presentThreat ? 6 : 0) + threatIntent * 8);
            AddRotation(chest, modelRoot.right, (presentThreat ? 3 : 0) + threatIntent * 6);
            AddRotation(neck, modelRoot.right, -threatIntent * 4);
            AddRotation(head, modelRoot.right, pose.HeadPitch - threatIntent * 5);
            AddRotation(leftForearm, modelRoot.right, -running * moving * 24);
            AddRotation(rightForearm, modelRoot.right, -running * moving * 24);
            if (presentThreat)
            {
                AddRotation(leftArm, modelRoot.forward, threatIntent * 9);
                AddRotation(rightArm, modelRoot.forward, -threatIntent * 5);
                AddRotation(leftArm, modelRoot.right, -threatIntent * (12 + running * 16));
                AddRotation(rightArm, modelRoot.right, -threatIntent * (4 + running * 23));
                AddRotation(leftForearm, modelRoot.right, -threatIntent * 9);
                AddRotation(rightForearm, modelRoot.right, -threatIntent * 16);
            }
            float turn = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(24, 130, Mathf.Abs(pose.TurnRate))) * (1 - moving);
            float turnCycle = Mathf.Repeat(pose.TurnPhase / (Mathf.PI * 2), 1);
            Foot(leftThigh, leftShin, leftFoot, leftContact, leftFootOrientation, leftUpperLength, leftLowerLength,
                Mathf.Repeat(cycle + .48f, 1), Mathf.Repeat(turnCycle, 1), travel, speed, moving, running, turn, pose.TurnRate);
            Foot(rightThigh, rightShin, rightFoot, rightContact, rightFootOrientation, rightUpperLength, rightLowerLength,
                Mathf.Repeat(cycle + .98f, 1), Mathf.Repeat(turnCycle + .5f, 1), travel, speed, moving, running, turn, pose.TurnRate);
            hips.position += modelRoot.up * breath * .0015f;
            // An authored eyelid pulse uses the captured body phase, including
            // its frozen value during pause. It is not live eye tracking.
            float blinkPhase = Mathf.Repeat(pose.BreathPhase / (Mathf.PI * 2), 1);
            float blink = blinkPhase > .965f ? Mathf.Sin((blinkPhase - .965f) / .035f * Mathf.PI) : 0;
            if (presentThreat) blink *= 1 - threatIntent;
            MoveLids(upperEyelids, -blink * .0043f + (presentThreat ? threatIntent * .0004f : 0));
            MoveLids(lowerEyelids, blink * .0007f);
            if (presentThreat && jaw)
            {
                float voice = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, .70f, threatAge)) *
                    (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.1f, 1.8f, threatAge)));
                AddRotation(jaw, modelRoot.right, voice * threatIntent * 5);
            }
        }

        void ApplyCapturedSupport(RecordedBodyPose pose, float moving, float running)
        {
            var support = pose.Locomotion;
            float speed = pose.Velocity.magnitude;
            Vector2 travel = speed > .01f ? pose.Velocity / speed : Vector2.up;
            float heading = Mathf.Clamp(Mathf.Atan2(travel.x, Mathf.Max(.4f, Mathf.Abs(travel.y))) * Mathf.Rad2Deg * .20f, -18, 18) * moving;
            float pivot = Mathf.Clamp(-pose.TurnRate * .06f, -28, 28) * (1 - moving * .5f);
            hips.position += modelRoot.TransformVector(support.HipOffset) + modelRoot.up * Mathf.Sin(pose.BreathPhase) * .0015f;
            AddRotation(hips, modelRoot.up, heading + pivot);
            AddRotation(spine, modelRoot.up, -heading * .65f - pivot * .25f);
            AddRotation(neck, modelRoot.up, -pivot * .45f);
            AddRotation(head, modelRoot.up, -heading * .35f - pivot * .30f);
            AddRotation(hips, modelRoot.forward, support.Lean.x);
            float lean = support.Lean.y + running * moving * 3;
            AddRotation(spine, modelRoot.right, lean + Mathf.Sin(pose.BreathPhase) * .35f);
            AddRotation(neck, modelRoot.right, -lean * .3f);
            AddRotation(head, modelRoot.right, pose.HeadPitch);

            // Retain the existing authored arm performance when travelling
            // forward/backward; lateral travel uses a quieter balance pose.
            // This prevents the forward walk's arm swing cutting across a
            // sideways torso. The blend uses recorded local velocity only.
            float arms = Mathf.Lerp(.25f, 1, Mathf.Abs(travel.y));
            BlendArm(leftArm, arms); BlendArm(rightArm, arms);
            BlendArm(leftForearm, arms); BlendArm(rightForearm, arms);
            AddRotation(leftForearm, modelRoot.right, -running * moving * 18);
            AddRotation(rightForearm, modelRoot.right, -running * moving * 18);

            Vector3 left = modelRoot.TransformPoint(support.LeftFoot), right = modelRoot.TransformPoint(support.RightFoot);
            var lh = modelRoot.TransformPoint(new Vector3(Mathf.Min(support.LeftFoot.x, -.10f), .48f, .62f));
            var rh = modelRoot.TransformPoint(new Vector3(Mathf.Max(support.RightFoot.x, .10f), .48f, .62f));
            SolveLimb(leftThigh, leftShin, leftFoot, left, lh, leftUpperLength, leftLowerLength);
            SolveLimb(rightThigh, rightShin, rightFoot, right, rh, rightUpperLength, rightLowerLength);
            leftFoot.rotation = modelRoot.rotation * support.LeftRotation * leftFootOrientation;
            rightFoot.rotation = modelRoot.rotation * support.RightRotation * rightFootOrientation;
            float phase = Mathf.Repeat(pose.BreathPhase / (Mathf.PI * 2), 1);
            float blink = phase > .965f ? Mathf.Sin((phase - .965f) / .035f * Mathf.PI) : 0;
            MoveLids(upperEyelids, -blink * .0043f); MoveLids(lowerEyelids, blink * .0007f);
        }

        void BlendArm(Transform joint, float amount)
        {
            for (int i = 0; i < joints.Length; i++) if (joints[i] == joint)
            { joint.localRotation = Quaternion.Slerp(quietRotations[i], joint.localRotation, amount); return; }
        }

        void MoveLids(Transform[] lids, float height)
        {
            if (lids == null) return;
            foreach (var lid in lids) if (lid) lid.position += modelRoot.up * height;
        }

        void Foot(Transform thigh, Transform shin, Transform foot, Vector3 restContact, Quaternion orientation,
            float upper, float lower, float phase, float turnPhase, Vector2 travel, float speed, float moving, float running, float turning, float turnRate)
        {
            float stance = Mathf.Lerp(.60f, .38f, running);
            float span = StrideForSpeed(speed) * stance;
            float along, lift, pitch;
            if (phase < stance)
            {
                float u = phase / stance;
                along = span * (.5f - u); lift = 0;
                pitch = Mathf.Lerp(-9, 13, Mathf.SmoothStep(0, 1, u));
            }
            else
            {
                float u = (phase - stance) / (1 - stance);
                along = Mathf.Lerp(-span * .5f, span * .5f, Mathf.SmoothStep(0, 1, u));
                lift = Mathf.Sin(u * Mathf.PI) * Mathf.Lerp(.14f, .24f, running);
                pitch = Mathf.Sin(u * Mathf.PI) * -13;
            }
            Vector3 target = restContact + new Vector3(travel.x * along, lift, travel.y * along) * moving;
            float roll = Mathf.Sin(pitch * Mathf.Deg2Rad) * moving;
            target.y += Mathf.Max(.19f * roll, -.08f * roll);
            float turnLift = Mathf.Max(0, Mathf.Sin(turnPhase * Mathf.PI * 2));
            target.y += turnLift * .055f * turning;
            target.z += Mathf.Sin(turnPhase * Mathf.PI * 2) * .045f * turning;
            var hint = modelRoot.TransformPoint(new Vector3(restContact.x + travel.x * .30f * moving, .48f, .6f));
            SolveLimb(thigh, shin, foot, modelRoot.TransformPoint(target), hint, upper, lower);
            float heading = Mathf.Atan2(travel.x, Mathf.Max(.4f, Mathf.Abs(travel.y))) * Mathf.Rad2Deg * .5f * moving;
            foot.rotation = modelRoot.rotation * Quaternion.Euler(pitch * moving, heading + Mathf.Clamp(turnRate * -.08f, -13, 13) * turning, 0) * orientation;
        }

        public void ApplyGrip(bool held)
        {
            if (!held || !enabled || !ready || presentThreat) return;
            Vector3 point = sampledPose.HasMachineGripTarget ? sampledPose.MachineGripTarget : new Vector3(.17f, 1.24f, .91f);
            // Weight shifts toward the explicitly captured contact while both
            // planted feet retain their sampled locations. No live IK query.
            Vector3 lf=leftFoot.position, rf=rightFoot.position;
            Quaternion lq=leftFoot.rotation, rq=rightFoot.rotation;
            float span=new Vector2(point.x,point.z).magnitude;
            float reach = Mathf.Clamp01((span - .45f) / .65f);
            float far=Mathf.Clamp01((span-1.10f)/.55f);
            Vector3 direction=modelRoot.TransformDirection(new Vector3(point.x,0,point.z).normalized);
            Vector3 bendAxis=Vector3.Cross(modelRoot.up,direction);
            hips.position += direction * (.20f * reach+.32f*far) - modelRoot.up * (.025f * reach+.10f*far);
            lf+=direction*.25f*far;rf+=direction*.40f*far;
            AddRotation(spine, bendAxis, Mathf.Lerp(12,36,reach)+8*far);
            AddRotation(chest, modelRoot.right, 7);
            AddRotation(neck, modelRoot.right, -26);
            SolveLimb(leftThigh,leftShin,leftFoot,lf,modelRoot.TransformPoint(new Vector3(leftContact.x,.48f,.6f)),leftUpperLength,leftLowerLength);
            SolveLimb(rightThigh,rightShin,rightFoot,rf,modelRoot.TransformPoint(new Vector3(rightContact.x,.48f,.6f)),rightUpperLength,rightLowerLength);
            leftFoot.rotation=lq;rightFoot.rotation=rq;
            var target = modelRoot.TransformPoint(point - new Vector3(0,0,.075f));
            var hint = modelRoot.TransformPoint(new Vector3(.46f, 1.05f, .28f));
            SolveLimb(rightArm, rightForearm, rightHand, target, hint,
                Vector3.Distance(rightArm.position, rightForearm.position), Vector3.Distance(rightForearm.position, rightHand.position));
            if (rightGripFingers != null) foreach (var finger in rightGripFingers) if (finger)
                finger.localRotation *= Quaternion.AngleAxis(27, Vector3.forward);
        }

        static void AddRotation(Transform joint, Vector3 axis, float degrees)
            => joint.rotation = Quaternion.AngleAxis(degrees, axis) * joint.rotation;

        static void SolveLimb(Transform upperBone, Transform lowerBone, Transform end, Vector3 target, Vector3 hint, float upper, float lower)
        {
            Vector3 origin = upperBone.position, offset = target - origin;
            float distance = Mathf.Clamp(offset.magnitude, Mathf.Abs(upper - lower) + .001f, upper + lower - .001f);
            Vector3 direction = offset.sqrMagnitude > .00001f ? offset.normalized : Vector3.down;
            Vector3 bend = Vector3.ProjectOnPlane(hint - origin, direction).normalized;
            if (bend.sqrMagnitude < .01f) bend = Vector3.ProjectOnPlane(Vector3.forward, direction).normalized;
            float projection = (upper * upper + distance * distance - lower * lower) / (2 * distance);
            Vector3 middle = origin + direction * projection + bend * Mathf.Sqrt(Mathf.Max(0, upper * upper - projection * projection));
            upperBone.rotation = Quaternion.FromToRotation(lowerBone.position - origin, middle - origin) * upperBone.rotation;
            lowerBone.rotation = Quaternion.FromToRotation(end.position - lowerBone.position, target - lowerBone.position) * lowerBone.rotation;
        }
    }
}

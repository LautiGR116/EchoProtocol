using UnityEngine;

namespace EchoProtocol.Campaign.IntruderActing
{
    // Passive part of the present actor's performance. The current pose owner
    // restores the rig, then calls this once. No Update, Animator or Echo input.
    [DisallowMultipleComponent]
    public sealed class IntruderActingFocus : MonoBehaviour
    {
        [SerializeField] Transform leftEye, rightEye;
        [SerializeField] bool containedViolence;
        Vector3 heldPoint, previousPoint;
        Vector3 headForward, leftEyeForward, rightEyeForward;
        float holdAge, acquisitionAge, yaw, pitch;
        float fromYaw, fromPitch;
        bool calibrated;
        int beat = int.MinValue;
        public bool UsesContainedViolence => containedViolence;
        public bool IsConfigured => leftEye && rightEye && leftEye != rightEye
            && leftEye.IsChildOf(transform) && rightEye.IsChildOf(transform);

        public void ResetFocus()
        {
            beat = int.MinValue;
            holdAge = acquisitionAge = yaw = pitch = 0;
            fromYaw = fromPitch = 0;
        }

        // Called only with the owner's restored anatomy. Bone local axes are
        // not camera axes: retain the actual neutral forward of this rig.
        public void CaptureNeutral(Transform root, Transform head)
        {
            if (!IsConfigured || !root || !head) return;
            headForward = head.InverseTransformDirection(root.forward);
            leftEyeForward = leftEye.InverseTransformDirection(root.forward);
            rightEyeForward = rightEye.InverseTransformDirection(root.forward);
            calibrated = true;
        }

        public void Pose(Transform root, Transform neck, Transform head, Vector3 evidence,
            int nextBeat, float delta, float headDelay, bool canReacquire, float eyePitchBias = 0)
        {
            if (!IsConfigured || delta <= 0) return;
            if (containedViolence && calibrated)
            {
                PoseContained(root, neck, head, evidence, nextBeat, delta, headDelay, canReacquire);
                return;
            }
            holdAge += delta; acquisitionAge += delta;
            // Take a new point on an action edge. In stillness, a substantial
            // change can earn one new look after a hold; no live servo tracking.
            bool moved = canReacquire && holdAge > .85f
                && Vector3.Angle(heldPoint - head.position, evidence - head.position) > 24;
            if (beat != nextBeat || moved)
            {
                heldPoint = evidence; beat = nextBeat;
                holdAge = acquisitionAge = 0;
            }
            Vector3 local = root.InverseTransformDirection(heldPoint - head.position);
            float targetYaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -42, 42);
            float targetPitch = Mathf.Clamp(-Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg, -14, 17);
            if (acquisitionAge >= headDelay)
            {
                yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, delta * 115);
                pitch = Mathf.MoveTowardsAngle(pitch, targetPitch, delta * 75);
            }
            Turn(neck, root.up, yaw * .30f); Turn(head, root.up, yaw * .70f);
            Turn(head, root.right, pitch);
            // Eyes acquire before the head. Limits are intentionally small;
            // anatomical neutral geometry belongs to the face authoring pass.
            float eyeYaw = Mathf.Clamp(targetYaw - yaw, -12, 12);
            float eyePitch = Mathf.Clamp(targetPitch - pitch + eyePitchBias, -7, 7);
            Turn(leftEye, root.up, eyeYaw); Turn(rightEye, root.up, eyeYaw);
            Turn(leftEye, root.right, eyePitch); Turn(rightEye, root.right, eyePitch);
        }

        void PoseContained(Transform root, Transform neck, Transform head, Vector3 evidence,
            int nextBeat, float delta, float headDelay, bool canReacquire)
        {
            holdAge += delta; acquisitionAge += delta;
            bool moved = canReacquire && holdAge >= 1.05f
                && Vector3.Angle(heldPoint - head.position, evidence - head.position) > 12;
            if (beat != nextBeat || moved)
            {
                previousPoint = beat == int.MinValue ? evidence : heldPoint;
                heldPoint = evidence; beat = nextBeat;
                fromYaw = yaw; fromPitch = pitch;
                holdAge = acquisitionAge = 0;
            }
            Vector2 target = Angles(root, heldPoint - head.position);
            target.x = Mathf.Clamp(target.x, -46, 46);
            target.y = Mathf.Clamp(target.y, -18, 22);
            // An eye acquisition, a delay, then one decisive head turn. Hold
            // that world point; player motion does not continuously steer it.
            float turn = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(headDelay, headDelay + .14f, acquisitionAge));
            yaw = Mathf.LerpAngle(fromYaw, target.x, turn);
            pitch = Mathf.LerpAngle(fromPitch, target.y, turn);
            Vector2 current = Angles(root, head.TransformDirection(headForward));
            float yawCorrection = Mathf.Clamp(Mathf.DeltaAngle(current.x, yaw), -55, 55);
            Turn(neck, root.up, yawCorrection * .32f); Turn(head, root.up, yawCorrection * .68f);
            current = Angles(root, head.TransformDirection(headForward));
            // Keep the chin below the acquired point; eyes look up from under
            // the brow. Correct the posed torso's direction instead of adding
            // another yaw/pitch on top and making both eyes miss the player.
            float pitchCorrection = Mathf.Clamp(Mathf.DeltaAngle(current.y, pitch + 7), -38, 38);
            Turn(neck, root.right, pitchCorrection * .30f); Turn(head, root.right, pitchCorrection * .70f);
            float saccade = Mathf.SmoothStep(0, 1, acquisitionAge / .045f);
            Vector3 eyePoint = Vector3.Lerp(previousPoint, heldPoint, saccade);
            AimEye(leftEye, leftEyeForward, root, eyePoint);
            AimEye(rightEye, rightEyeForward, root, eyePoint);
        }

        static void AimEye(Transform eye, Vector3 forward, Transform root, Vector3 point)
        {
            Vector2 target = Angles(root, point - eye.position);
            Vector2 current = Angles(root, eye.TransformDirection(forward));
            Turn(eye, root.up, Mathf.Clamp(Mathf.DeltaAngle(current.x, target.x), -13, 13));
            current = Angles(root, eye.TransformDirection(forward));
            Turn(eye, root.right, Mathf.Clamp(Mathf.DeltaAngle(current.y, target.y), -10, 10));
        }

        static Vector2 Angles(Transform root, Vector3 direction)
        {
            Vector3 local = root.InverseTransformDirection(direction);
            return new Vector2(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg,
                -Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg);
        }

        static void Turn(Transform joint, Vector3 axis, float degrees)
            => joint.rotation = Quaternion.AngleAxis(degrees, axis) * joint.rotation;
    }
}

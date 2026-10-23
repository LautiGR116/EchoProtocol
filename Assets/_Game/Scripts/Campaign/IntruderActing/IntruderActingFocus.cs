using UnityEngine;

namespace EchoProtocol.Campaign.IntruderActing
{
    // Passive part of the present actor's performance. The current pose owner
    // restores the rig, then calls this once. No Update, Animator or Echo input.
    [DisallowMultipleComponent]
    public sealed class IntruderActingFocus : MonoBehaviour
    {
        [SerializeField] Transform leftEye, rightEye;
        Vector3 heldPoint;
        float holdAge, acquisitionAge, yaw, pitch;
        int beat = int.MinValue;
        public bool IsConfigured => leftEye && rightEye && leftEye != rightEye
            && leftEye.IsChildOf(transform) && rightEye.IsChildOf(transform);

        public void ResetFocus()
        {
            beat = int.MinValue;
            holdAge = acquisitionAge = yaw = pitch = 0;
        }

        public void Pose(Transform root, Transform neck, Transform head, Vector3 evidence,
            int nextBeat, float delta, float headDelay, bool canReacquire)
        {
            if (!IsConfigured || delta <= 0) return;
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
            float eyePitch = Mathf.Clamp(targetPitch - pitch, -7, 7);
            Turn(leftEye, root.up, eyeYaw); Turn(rightEye, root.up, eyeYaw);
            Turn(leftEye, root.right, eyePitch); Turn(rightEye, root.right, eyePitch);
        }

        static void Turn(Transform joint, Vector3 axis, float degrees)
            => joint.rotation = Quaternion.AngleAxis(degrees, axis) * joint.rotation;
    }
}

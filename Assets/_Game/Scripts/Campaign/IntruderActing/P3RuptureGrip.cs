using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Campaign.IntruderActing
{
    // Called only by P3's pose owner, after restoring the same rig. A real cube
    // jamb defines the two bearing faces; no new collider or independent clock.
    public sealed class P3RuptureGrip
    {
        readonly List<Transform> fingers = new List<Transform>(4);
        Transform thumb;
        MeshFilter frame;
        Bounds bounds;
        public Vector3 ContactPoint { get; private set; }

        public bool Capture(Transform[] digits, MeshFilter jamb)
        {
            fingers.Clear(); thumb = null; frame = jamb;
            if (!frame || !frame.sharedMesh || digits == null) return false;
            bounds = frame.sharedMesh.bounds;
            foreach (var digit in digits)
            {
                if (!digit || !digit.name.Contains("_01_") || digit.childCount == 0
                    || digit.GetChild(0).childCount == 0 || digit.GetChild(0).GetChild(0).childCount == 0) continue;
                if (digit.name.Contains("thumb")) thumb = digit;
                else fingers.Add(digit);
            }
            return fingers.Count == 4 && thumb;
        }

        public void Curl(float weight)
        {
            Vector3 corner = frame.transform.TransformPoint(new Vector3(bounds.max.x, bounds.center.y, bounds.min.z));
            // The palm faces west and fingers point south. All four knuckles
            // turn west around the jamb's south-east corner, onto its south face.
            foreach (var finger in fingers)
            {
                Transform joint = finger;
                for (int segment = 0; segment < 3; segment++)
                {
                    Transform next = joint.GetChild(0);
                    float length = Vector3.Distance(joint.position, next.position);
                    float dz = Mathf.Clamp(corner.z - (segment == 0 ? .016f : .012f) - joint.position.z,
                        -length * .7f, length * .7f);
                    Vector3 direction = new Vector3(-Mathf.Sqrt(Mathf.Max(0, length * length - dz * dz)), 0, dz);
                    Aim(joint, next, direction, weight);
                    joint = next;
                }
            }
            // Thumb opposes the hooked fingers on the east face, not a spread
            // greeting. Joint lengths and skin weights stay untouched.
            Transform t = thumb;
            for (int segment = 0; segment < 3; segment++)
            {
                Transform next = t.GetChild(0);
                float length = Vector3.Distance(t.position, next.position);
                Vector2 horizontal = Vector2.ClampMagnitude(new Vector2(corner.x + .018f - t.position.x,
                    corner.z + .035f - t.position.z), length * .8f);
                Aim(t, next, new Vector3(horizontal.x,
                    -Mathf.Sqrt(Mathf.Max(0, length * length - horizontal.sqrMagnitude)), horizontal.y), weight);
                t = next;
            }
            ContactPoint = new Vector3(corner.x, fingers[1].position.y, corner.z);
        }

        public bool Reached
        {
            get
            {
                if (!frame || fingers.Count != 4) return false;
                foreach (var finger in fingers)
                {
                    Transform tip = finger.GetChild(0).GetChild(0).GetChild(0);
                    Vector3 local = frame.transform.InverseTransformPoint(tip.position);
                    Vector3 surface = frame.transform.TransformPoint(bounds.ClosestPoint(local));
                    // Require real distal fingers beside the frame surface,
                    // outside its solid volume. A reached wrist is insufficient.
                    if (local.z >= bounds.min.z || local.x < bounds.min.x || local.x > bounds.max.x
                        || local.y < bounds.min.y || local.y > bounds.max.y
                        || Vector3.Distance(tip.position, surface) > .028f) return false;
                }
                return true;
            }
        }

        static void Aim(Transform joint, Transform next, Vector3 direction, float weight)
            => joint.rotation = Quaternion.Slerp(Quaternion.identity,
                Quaternion.FromToRotation(next.position - joint.position, direction), weight) * joint.rotation;
    }
}

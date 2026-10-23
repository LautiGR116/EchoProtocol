using System;
using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Horror.SE2Presence
{
    // Exclusively the present SE2 adult. DomesticPresence owns discovery;
    // VS1ParallelFigureEvent owns hidden activation and traversal. No take writes.
    [DisallowMultipleComponent]
    public sealed class SE2GlassPressure : MonoBehaviour
    {
        [Serializable]
        public sealed class Palm
        {
            public Transform upper, lower, hand, index, middle, little, target;
            public Transform[] digits;
            public bool left;
            [NonSerialized] public Quaternion orientation;
            [NonSerialized] public float upperLength, lowerLength;
            public bool IsConfigured => upper && lower && hand && index && middle && little && target;

            public void Capture()
            {
                upperLength = Vector3.Distance(upper.position, lower.position);
                lowerLength = Vector3.Distance(lower.position, hand.position);
                Vector3 fingers = hand.InverseTransformDirection(middle.position - hand.position).normalized;
                Vector3 across = hand.InverseTransformDirection(index.position - little.position).normalized;
                Vector3 normal = (left ? Vector3.Cross(across, fingers) : Vector3.Cross(fingers, across)).normalized;
                // Targets face the surface, fingers up; anatomy basis is measured
                // from this adult, not copied from a different skeleton's wrist.
                orientation = Quaternion.Inverse(Quaternion.LookRotation(normal, fingers));
            }

            public void Place(Vector3 offset, Vector3 elbowHint)
            {
                Solve(upper, lower, hand, target.position + offset, elbowHint, upperLength, lowerLength);
                hand.rotation = target.rotation * orientation;
                // Uncurl the actual fingers into the contact plane. Their own
                // spread survives; curled resting fingertips must not pierce it.
                if (digits != null) foreach (var digit in digits)
                {
                    if (!digit || digit.childCount == 0) continue;
                    Vector3 direction = digit.GetChild(0).position - digit.position;
                    Vector3 flat = Vector3.ProjectOnPlane(direction, target.forward);
                    if (flat.sqrMagnitude > .000001f)
                        digit.rotation = Quaternion.FromToRotation(direction, flat) * digit.rotation;
                }
            }
        }

        [SerializeField] private RecordedCharacterMotion anatomy;
        [SerializeField] private Transform chest, neck, head;
        [SerializeField] private Palm highPalm = new Palm { left = true };
        [SerializeField] private Palm lowPalm = new Palm();
        [SerializeField] private Renderer surface;
        [SerializeField] private AudioSource contactSound;
        [SerializeField] private AudioClip seatClip, slipClip;
        [SerializeField] private Transform[] upperEyelids;
        private RecordedLocomotion support;
        private MaterialPropertyBlock skin;
        private bool prepared, discovered, slipPlayed;
        private float age;
        private Color originalTint;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
        public bool IsConfigured => anatomy && chest && neck && head && surface && highPalm.IsConfigured && lowPalm.IsConfigured
            && contactSound && seatClip && slipClip && contactSound.outputAudioMixerGroup
            && contactSound.spatialBlend >= .999f && !contactSound.ignoreListenerPause && !contactSound.ignoreListenerVolume;
        public bool Settled => discovered && age >= 3.12f;
        public int SlipCount { get; private set; }

        public bool PreparePose()
        {
            if (prepared) return true;
            if (!IsConfigured) { Debug.LogError("SE2 pressure requires its exclusive adult, two palms and routed contact source.", this); return false; }
            anatomy.Apply(new RecordedBodyPose(0, 0, 0, Vector2.zero));
            if (!anatomy.IsReady) return false;
            Transform model = anatomy.ModelRoot;
            Vector3 left = model.InverseTransformPoint(anatomy.LeftFoot.position);
            Vector3 right = model.InverseTransformPoint(anatomy.RightFoot.position);
            left.x -= .025f; right.x += .025f;
            left.z += .035f; right.z -= .07f;
            // Root height is already included in the standing clip. Do not
            // subtract world y from model-local feet (raised floors stay valid).
            support = new RecordedLocomotion(left, right, Quaternion.Euler(0, -6, 0), Quaternion.Euler(0, 8, 0),
                new Vector3(-.025f, -.035f, .025f), new Vector2(-3.5f, 14f), 0);
            highPalm.Capture(); lowPalm.Capture();
            originalTint = surface.sharedMaterials[0].GetColor(BaseColor);
            skin = new MaterialPropertyBlock();
            surface.GetPropertyBlock(skin, 0);
            // Retain the licensed skin/clothes maps. This renderer alone becomes
            // cooler and more matte; never mutate the Echo's shared material.
            skin.SetColor(BaseColor, originalTint * new Color(.82f, .87f, .84f, 1));
            skin.SetFloat(Smoothness, .32f);
            surface.SetPropertyBlock(skin, 0);
            prepared = true;
            ApplyPressure(0);
            return true;
        }

        public void Discover()
        {
            if (!prepared || discovered) return;
            discovered = true;
            // The high palm seats 4 mm against its fixed stop over the attack.
            contactSound.transform.position = highPalm.target.position;
            contactSound.PlayOneShot(seatClip);
        }

        // Only called while the owner is presented and the session is advancing.
        // Looking away cannot reset or delay the performance; no proximity gate.
        public void Tick(float delta)
        {
            if (!prepared || !discovered || delta <= 0 || Settled) return;
            age += delta;
            if (!slipPlayed && age >= 2.4f)
            {
                slipPlayed = true; SlipCount++;
                contactSound.transform.position = highPalm.target.position;
                contactSound.PlayOneShot(slipClip);
            }
            ApplyPressure(age);
        }

        private void ApplyPressure(float time)
        {
            // Both feet remain planted. The two palms keep their plane while the
            // shoulder hangs beneath the high wrist. Nothing follows the camera.
            anatomy.Apply(new RecordedBodyPose(0, 0, 0, Vector2.zero, locomotion: support));
            Transform model = anatomy.ModelRoot;
            float settle = Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - 2.4f) / .72f));
            Rotate(chest, model.forward, -5.5f);
            Rotate(chest, model.right, 2.5f + settle * 1.2f);
            Rotate(neck, model.forward, 12f);
            Rotate(head, model.right, -9f);
            Rotate(head, model.up, -14f);
            if (upperEyelids != null)
                foreach (var lid in upperEyelids) if (lid) lid.position -= model.up * .0012f;
            float seat = discovered ? 1f - Mathf.SmoothStep(0, 1, Mathf.Clamp01(time / .12f)) : 1f;
            highPalm.Place(Vector3.down * (.03f * settle) - highPalm.target.forward * (.004f * seat),
                highPalm.upper.position - model.up * .35f - model.right * .35f - model.forward * .1f);
            lowPalm.Place(Vector3.zero, lowPalm.upper.position - model.up * .45f + model.right * .25f - model.forward * .12f);
        }

        public void StopSound() { if (contactSound) contactSound.Stop(); }
        private void OnDisable() => StopSound();

        private static void Rotate(Transform joint, Vector3 axis, float degrees)
            => joint.rotation = Quaternion.AngleAxis(degrees, axis) * joint.rotation;

        private static void Solve(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 hint, float a, float b)
        {
            Vector3 origin = upper.position, offset = target - origin;
            float length = Mathf.Clamp(offset.magnitude, Mathf.Abs(a - b) + .001f, a + b - .001f);
            Vector3 direction = offset.sqrMagnitude > .000001f ? offset.normalized : Vector3.down;
            Vector3 bend = Vector3.ProjectOnPlane(hint - origin, direction).normalized;
            if (bend.sqrMagnitude < .01f) bend = Vector3.ProjectOnPlane(Vector3.forward, direction).normalized;
            float along = (a * a + length * length - b * b) / (2 * length);
            Vector3 middle = origin + direction * along + bend * Mathf.Sqrt(Mathf.Max(0, a * a - along * along));
            upper.rotation = Quaternion.FromToRotation(lower.position - origin, middle - origin) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(end.position - lower.position, target - lower.position) * lower.rotation;
        }
    }
}

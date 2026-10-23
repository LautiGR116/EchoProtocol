using System;
using EchoProtocol.Campaign.IntruderPresence;
using UnityEngine;

namespace EchoProtocol.Campaign.IntruderReveal
{
    // Saved authorship/rollback data only. Existing Staging/Presence/atmosphere
    // retain their light envelopes. No Update, pose, audio or event owner here.
    [DisallowMultipleComponent]
    public sealed class IntruderRevealSetup : MonoBehaviour
    {
        [Serializable]
        public struct LightState
        {
            public IntruderPresenceSetup.LightState values;
            public LightType type;
            public float shadowStrength;
            public static LightState Read(Light light) => new LightState
            {
                values = IntruderPresenceSetup.LightState.Read(light),
                type = light.type, shadowStrength = light.shadowStrength
            };
            public void Write(Light light)
            {
                light.type = type;
                values.Write(light);
                light.shadowStrength = shadowStrength;
            }
            public bool Matches(Light light) => light && type == light.type
                && Mathf.Abs(shadowStrength - light.shadowStrength) < .0001f && values.Matches(light);
        }

        [Serializable]
        public struct TransformState
        {
            public Vector3 position, scale;
            public Quaternion rotation;
            public static TransformState Read(Transform target) => new TransformState
            { position = target.localPosition, rotation = target.localRotation, scale = target.localScale };
            public void Write(Transform target)
            { target.localPosition = position; target.localRotation = rotation; target.localScale = scale; }
            public bool Matches(Transform target) => target
                && Vector3.Distance(position, target.localPosition) < .0001f
                && Vector3.Distance(scale, target.localScale) < .0001f
                && Quaternion.Angle(rotation, target.localRotation) < .02f;
        }

        [Serializable] public struct LightEdit
        { public Light source; public Transform parent; public LightState before, applied; }
        [Serializable] public struct PropEdit
        { public Transform target, parent; public TransformState before, applied; }

        public int revision = 5;
        public IntruderStaging staging;
        public IntruderPresenceSetup roundFour;
        public float previousP3Intensity, appliedP3Intensity;
        public LightEdit[] lights;
        public PropEdit[] props;
    }
}

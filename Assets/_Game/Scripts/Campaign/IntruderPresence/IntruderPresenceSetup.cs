using System;
using EchoProtocol.Audio;
using UnityEngine;

namespace EchoProtocol.Campaign.IntruderPresence
{
    // Scene-local authorship/rollback data only. No pose, camera, light or sound
    // playback owner: the existing components still drive their real events.
    [DisallowMultipleComponent]
    public sealed class IntruderPresenceSetup : MonoBehaviour
    {
        [Serializable]
        public struct LightState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Color color;
            public float intensity, range, outer, inner, bias, normalBias;
            public LightShadows shadows;
            public static LightState Read(Light source) => new LightState
            {
                position = source.transform.localPosition, rotation = source.transform.localRotation,
                color = source.color, intensity = source.intensity, range = source.range,
                outer = source.spotAngle, inner = source.innerSpotAngle, shadows = source.shadows,
                bias = source.shadowBias, normalBias = source.shadowNormalBias
            };
            public void Write(Light source)
            {
                source.transform.localPosition = position; source.transform.localRotation = rotation;
                source.color = color; source.intensity = intensity; source.range = range;
                source.spotAngle = outer; source.innerSpotAngle = inner; source.shadows = shadows;
                source.shadowBias = bias; source.shadowNormalBias = normalBias;
            }
            public bool Matches(Light source)
            {
                var value = Read(source);
                return Vector3.Distance(position, value.position) < .0001f
                    && Quaternion.Angle(rotation, value.rotation) < .02f && color == value.color
                    && Mathf.Abs(intensity - value.intensity) < .0001f && Mathf.Abs(range - value.range) < .0001f
                    && Mathf.Abs(outer - value.outer) < .0001f && Mathf.Abs(inner - value.inner) < .0001f
                    && shadows == value.shadows && Mathf.Abs(bias - value.bias) < .0001f
                    && Mathf.Abs(normalBias - value.normalBias) < .0001f;
            }
        }
        [Serializable] public struct LightEdit
        {
            public Light source;
            public LightState before, applied;
        }
        [Serializable] public struct AudioEdit
        {
            public AudioSource source;
            public float before, applied;
        }
        public IntruderStaging staging;
        public MachineCoverPresence presence;
        public LightEdit[] lights;
        public AudioEdit[] sounds;
        public float previousP3Intensity;
        public int revision = 1;
    }
}

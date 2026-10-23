using System;
using EchoProtocol.Campaign;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Scene-local receipt only. No playback, pose, light or gain writer.
    [DisallowMultipleComponent]
    public sealed class IntruderDreadAudioSetup : MonoBehaviour
    {
        [Serializable] public struct SourceState
        {
            public AudioClip clip;
            public float pitch;
            public bool loop;
            public static SourceState Read(AudioSource source) => new SourceState
                { clip = source.clip, pitch = source.pitch, loop = source.loop };
            public void Write(AudioSource source)
                { source.clip = clip; source.pitch = pitch; source.loop = loop; }
            public bool Matches(AudioSource source) => source && source.clip == clip
                && Mathf.Approximately(source.pitch, pitch) && source.loop == loop;
        }
        [Serializable] public struct SourceEdit
        {
            public AudioSource source;
            public SourceState before, applied;
        }
        [Serializable] public struct ReferenceEdit
        {
            public UnityEngine.Object target;
            public string field;
            public UnityEngine.Object before, applied;
        }
        public int revision = 1;
        public MachineCoverPresence presence;
        public EmbestidaRush rush;
        public SourceEdit[] sources;
        public ReferenceEdit[] references;
        public bool previousDiscreteBreathing;
    }
}

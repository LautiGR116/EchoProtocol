using UnityEngine;

namespace EchoProtocol.Echo
{
    public readonly struct EchoFrame
    {
        public EchoFrame(float timestamp, Vector3 position, Quaternion rotation)
        {
            Timestamp = timestamp;
            Position = position;
            Rotation = rotation;
        }

        public float Timestamp { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }
    }
}

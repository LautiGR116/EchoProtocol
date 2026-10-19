using UnityEngine;

namespace EchoProtocol.Echo
{
    public readonly struct EchoFrame
    {
        public EchoFrame(float timestamp, Vector3 position, Quaternion rotation)
            : this(timestamp, position, rotation, default) { }

        public EchoFrame(float timestamp, Vector3 position, Quaternion rotation, RecordedBodyPose bodyPose)
        {
            Timestamp = timestamp;
            Position = position;
            Rotation = rotation;
            BodyPose = bodyPose;
        }

        public float Timestamp { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }
        public RecordedBodyPose BodyPose { get; }
    }
}

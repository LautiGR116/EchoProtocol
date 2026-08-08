using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Echo
{
    public sealed class EchoRecording
    {
        private readonly EchoFrame[] frames;

        public EchoRecording(IEnumerable<EchoFrame> sourceFrames)
        {
            if (sourceFrames == null)
            {
                throw new ArgumentNullException(nameof(sourceFrames));
            }

            List<EchoFrame> frameBuffer = new List<EchoFrame>();
            float previousTimestamp = float.NegativeInfinity;

            foreach (EchoFrame frame in sourceFrames)
            {
                if (float.IsNaN(frame.Timestamp)
                    || float.IsInfinity(frame.Timestamp)
                    || frame.Timestamp < 0f
                    || frame.Timestamp < previousTimestamp)
                {
                    throw new ArgumentException(
                        "Echo frame timestamps must be finite, non-negative, and ordered.",
                        nameof(sourceFrames));
                }

                frameBuffer.Add(frame);
                previousTimestamp = frame.Timestamp;
            }

            if (frameBuffer.Count == 0)
            {
                throw new ArgumentException(
                    "An Echo recording requires at least one frame.",
                    nameof(sourceFrames));
            }

            frames = frameBuffer.ToArray();
            Duration = frames[frames.Length - 1].Timestamp;
        }

        public int FrameCount => frames.Length;

        public float Duration { get; }

        public EchoFrame this[int index] => frames[index];

        public void Sample(float timestamp, out Vector3 position, out Quaternion rotation)
        {
            if (frames.Length == 1 || timestamp <= frames[0].Timestamp)
            {
                position = frames[0].Position;
                rotation = frames[0].Rotation;
                return;
            }

            if (timestamp >= Duration)
            {
                EchoFrame lastFrame = frames[frames.Length - 1];
                position = lastFrame.Position;
                rotation = lastFrame.Rotation;
                return;
            }

            int lowerIndex = 1;
            int upperIndex = frames.Length - 1;
            int nextFrameIndex = upperIndex;

            while (lowerIndex <= upperIndex)
            {
                int middleIndex = lowerIndex + (upperIndex - lowerIndex) / 2;
                if (frames[middleIndex].Timestamp >= timestamp)
                {
                    nextFrameIndex = middleIndex;
                    upperIndex = middleIndex - 1;
                }
                else
                {
                    lowerIndex = middleIndex + 1;
                }
            }

            EchoFrame previousFrame = frames[nextFrameIndex - 1];
            EchoFrame nextFrame = frames[nextFrameIndex];
            float frameDuration = nextFrame.Timestamp - previousFrame.Timestamp;
            float interpolation = frameDuration <= Mathf.Epsilon
                ? 1f
                : Mathf.Clamp01((timestamp - previousFrame.Timestamp) / frameDuration);

            position = Vector3.Lerp(previousFrame.Position, nextFrame.Position, interpolation);
            rotation = Quaternion.Slerp(previousFrame.Rotation, nextFrame.Rotation, interpolation);
        }
    }
}

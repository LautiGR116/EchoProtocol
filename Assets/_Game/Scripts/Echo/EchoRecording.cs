using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Echo
{
    public sealed class EchoRecording
    {
        private readonly EchoFrame[] frames;
        private readonly RecordedMachineGrip[] machineGrips;

        public EchoRecording(IEnumerable<EchoFrame> sourceFrames) : this(sourceFrames, null) { }

        public EchoRecording(IEnumerable<EchoFrame> sourceFrames, IEnumerable<RecordedMachineGrip> sourceGrips)
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
            var grips = new List<RecordedMachineGrip>();
            float last = -1;
            if (sourceGrips != null) foreach (var grip in sourceGrips)
            {
                if (float.IsNaN(grip.Timestamp) || float.IsInfinity(grip.Timestamp)
                    || grip.Timestamp < 0 || grip.Timestamp < last || grip.Timestamp > Duration || grip.Target <= 0)
                    throw new ArgumentException("Machine grip timestamps and target must belong to this take.", nameof(sourceGrips));
                grips.Add(grip); last = grip.Timestamp;
            }
            machineGrips = grips.ToArray();
        }

        public int FrameCount => frames.Length;

        public float Duration { get; }

        public EchoFrame this[int index] => frames[index];
        public int MachineGripCount => machineGrips.Length;
        public RecordedMachineGrip MachineGrip(int index) => machineGrips[index];
        public bool SampleMachineGrip(float timestamp, int target)
        {
            bool held = false;
            for (int i = 0; i < machineGrips.Length && machineGrips[i].Timestamp <= timestamp; i++)
                if (machineGrips[i].Target == target) held = machineGrips[i].Held;
            return held;
        }

        public RecordedBodyPose SampleBodyPose(float timestamp)
        {
            if(frames.Length==1||timestamp<=frames[0].Timestamp)return frames[0].BodyPose;
            if(timestamp>=Duration)return frames[frames.Length-1].BodyPose;
            int low=1,high=frames.Length-1;
            while(low<high){int mid=(low+high)/2;if(frames[mid].Timestamp<timestamp)low=mid+1;else high=mid;}
            var a=frames[low-1];var b=frames[low];
            float t=b.Timestamp>a.Timestamp?Mathf.Clamp01((timestamp-a.Timestamp)/(b.Timestamp-a.Timestamp)):1;
            return RecordedBodyPose.Lerp(a.BodyPose,b.BodyPose,t);
        }

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

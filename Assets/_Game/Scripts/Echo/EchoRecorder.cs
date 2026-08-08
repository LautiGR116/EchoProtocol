using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    public sealed class EchoRecorder : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float sampleInterval = 0.05f;
        [SerializeField, Min(1f)] private float maxRecordingDuration = 60f;

        private readonly List<EchoFrame> frames = new List<EchoFrame>();
        private float recordingStartTime;
        private float lastObservedTime;
        private Vector3 lastObservedPosition;
        private Quaternion lastObservedRotation;
        private int nextSampleIndex;

        public bool ReachedDurationLimit { get; private set; }

        public bool IsRecording { get; private set; }

        public int CapturedFrameCount => frames.Count;

        public float SampleInterval => sampleInterval;

        public float MaxRecordingDuration => maxRecordingDuration;

        public float ElapsedTime => IsRecording
            ? Mathf.Min(Time.time - recordingStartTime, maxRecordingDuration)
            : 0f;

        private void LateUpdate()
        {
            if (!IsRecording || ReachedDurationLimit)
            {
                return;
            }

            float observedTime = Time.time - recordingStartTime;
            float sampleThroughTime = Mathf.Min(observedTime, maxRecordingDuration);
            float observationDuration = observedTime - lastObservedTime;
            Vector3 observedPosition = transform.position;
            Quaternion observedRotation = transform.rotation;

            while (nextSampleIndex * sampleInterval <= sampleThroughTime)
            {
                float sampleTime = nextSampleIndex * sampleInterval;
                float interpolation = CalculateInterpolation(
                    sampleTime,
                    observationDuration);

                CaptureFrame(
                    sampleTime,
                    Vector3.Lerp(lastObservedPosition, observedPosition, interpolation),
                    Quaternion.Slerp(lastObservedRotation, observedRotation, interpolation));
                nextSampleIndex++;
            }

            if (observedTime >= maxRecordingDuration)
            {
                EchoFrame lastFrame = frames[frames.Count - 1];
                if (lastFrame.Timestamp < maxRecordingDuration)
                {
                    float interpolation = CalculateInterpolation(
                        maxRecordingDuration,
                        observationDuration);

                    CaptureFrame(
                        maxRecordingDuration,
                        Vector3.Lerp(lastObservedPosition, observedPosition, interpolation),
                        Quaternion.Slerp(lastObservedRotation, observedRotation, interpolation));
                }

                ReachedDurationLimit = true;
            }

            lastObservedTime = observedTime;
            lastObservedPosition = observedPosition;
            lastObservedRotation = observedRotation;
        }

        public void BeginRecording()
        {
            int expectedFrameCount = Mathf.CeilToInt(maxRecordingDuration / sampleInterval) + 2;
            if (frames.Capacity < expectedFrameCount)
            {
                frames.Capacity = expectedFrameCount;
            }

            frames.Clear();
            recordingStartTime = Time.time;
            lastObservedTime = 0f;
            lastObservedPosition = transform.position;
            lastObservedRotation = transform.rotation;
            nextSampleIndex = 1;
            ReachedDurationLimit = false;
            IsRecording = true;
            CaptureFrame(0f, lastObservedPosition, lastObservedRotation);
        }

        public EchoRecording FinishRecording()
        {
            if (!IsRecording)
            {
                return null;
            }

            float finalTimestamp = Mathf.Min(
                Time.time - recordingStartTime,
                maxRecordingDuration);

            EchoFrame lastFrame = frames[frames.Count - 1];
            if (finalTimestamp > lastFrame.Timestamp + Mathf.Epsilon)
            {
                CaptureFrame(finalTimestamp);
            }

            IsRecording = false;
            return new EchoRecording(frames);
        }

        public void CancelRecording()
        {
            IsRecording = false;
            ReachedDurationLimit = false;
            frames.Clear();
        }

        private void CaptureFrame(float timestamp)
        {
            CaptureFrame(timestamp, transform.position, transform.rotation);
        }

        private void CaptureFrame(
            float timestamp,
            Vector3 position,
            Quaternion rotation)
        {
            frames.Add(new EchoFrame(timestamp, position, rotation));
        }

        private float CalculateInterpolation(
            float sampleTime,
            float observationDuration)
        {
            return observationDuration <= Mathf.Epsilon
                ? 1f
                : Mathf.Clamp01(
                    (sampleTime - lastObservedTime) / observationDuration);
        }

        private void OnValidate()
        {
            sampleInterval = Mathf.Max(0.01f, sampleInterval);
            maxRecordingDuration = Mathf.Max(1f, maxRecordingDuration);
        }
    }
}

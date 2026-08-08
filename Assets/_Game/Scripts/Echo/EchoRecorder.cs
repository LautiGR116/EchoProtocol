using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    public sealed class EchoRecorder : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float sampleInterval = 0.05f;
        [SerializeField, Min(1f)] private float maxRecordingDuration = 60f;

        [Header("Start Gate")]
        [SerializeField, Min(0f)] private float startMovementThreshold = 0.02f;
        [SerializeField, Min(0f)] private float startPreRollDuration = 1f;

        private readonly List<EchoFrame> frames = new List<EchoFrame>();
        private float recordingStartTime;
        private Vector3 armedPosition;
        private Quaternion armedRotation;
        private float lastObservedTime;
        private Vector3 lastObservedPosition;
        private Quaternion lastObservedRotation;
        private int nextSampleIndex;

        public bool ReachedDurationLimit { get; private set; }

        public bool IsRecording { get; private set; }

        public bool HasRecordingStarted { get; private set; }

        public bool IsArmed => IsRecording && !HasRecordingStarted;

        public int CapturedFrameCount => frames.Count;

        public float SampleInterval => sampleInterval;

        public float MaxRecordingDuration => maxRecordingDuration;

        public float StartMovementThreshold => startMovementThreshold;

        public float StartPreRollDuration => startPreRollDuration;

        public float ElapsedTime => IsRecording && HasRecordingStarted
            ? Mathf.Min(Time.time - recordingStartTime, maxRecordingDuration)
            : 0f;

        private void LateUpdate()
        {
            if (!IsRecording || ReachedDurationLimit)
            {
                return;
            }

            float observedTime = Time.time - recordingStartTime;
            Vector3 observedPosition = transform.position;
            Quaternion observedRotation = transform.rotation;

            if (!HasRecordingStarted)
            {
                Vector2 horizontalMovement = new Vector2(
                    observedPosition.x - armedPosition.x,
                    observedPosition.z - armedPosition.z);

                if (horizontalMovement.sqrMagnitude
                    < startMovementThreshold * startMovementThreshold)
                {
                    return;
                }

                StartFromMovement(observedPosition, observedRotation);
                return;
            }

            observedTime = Time.time - recordingStartTime;
            float sampleThroughTime = Mathf.Min(observedTime, maxRecordingDuration);
            float observationDuration = observedTime - lastObservedTime;

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
            recordingStartTime = 0f;
            armedPosition = transform.position;
            armedRotation = transform.rotation;
            lastObservedTime = 0f;
            lastObservedPosition = armedPosition;
            lastObservedRotation = armedRotation;
            nextSampleIndex = 0;
            ReachedDurationLimit = false;
            IsRecording = true;
            HasRecordingStarted = false;
        }

        public EchoRecording FinishRecording()
        {
            if (!IsRecording || !HasRecordingStarted)
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
            HasRecordingStarted = false;
            return new EchoRecording(frames);
        }

        public void CancelRecording()
        {
            IsRecording = false;
            HasRecordingStarted = false;
            ReachedDurationLimit = false;
            frames.Clear();
        }

        private void StartFromMovement(
            Vector3 observedPosition,
            Quaternion observedRotation)
        {
            float preRollDuration = Mathf.Min(
                startPreRollDuration,
                Mathf.Max(0f, maxRecordingDuration - sampleInterval));

            recordingStartTime = Time.time - preRollDuration;
            frames.Clear();

            if (preRollDuration <= Mathf.Epsilon)
            {
                CaptureFrame(0f, observedPosition, observedRotation);
            }
            else
            {
                CaptureFrame(0f, armedPosition, armedRotation);
                int preRollSampleIndex = 1;

                while (preRollSampleIndex * sampleInterval < preRollDuration)
                {
                    CaptureFrame(
                        preRollSampleIndex * sampleInterval,
                        armedPosition,
                        armedRotation);
                    preRollSampleIndex++;
                }

                CaptureFrame(
                    preRollDuration,
                    observedPosition,
                    observedRotation);
            }

            nextSampleIndex = Mathf.FloorToInt(
                preRollDuration / sampleInterval) + 1;
            while (nextSampleIndex * sampleInterval
                <= preRollDuration + Mathf.Epsilon)
            {
                nextSampleIndex++;
            }

            lastObservedTime = preRollDuration;
            lastObservedPosition = observedPosition;
            lastObservedRotation = observedRotation;
            HasRecordingStarted = true;
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
            maxRecordingDuration = Mathf.Max(1f, maxRecordingDuration);
            sampleInterval = Mathf.Clamp(
                sampleInterval,
                0.01f,
                maxRecordingDuration);
            startMovementThreshold = Mathf.Max(0f, startMovementThreshold);
            startPreRollDuration = Mathf.Clamp(
                startPreRollDuration,
                0f,
                Mathf.Max(0f, maxRecordingDuration - sampleInterval));
        }
    }
}

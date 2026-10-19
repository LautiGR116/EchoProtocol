using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    public sealed class EchoRecorder : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float sampleInterval = 0.05f;
        [SerializeField, Min(1f)] private float maxRecordingDuration = 60f;
        [SerializeField] private PlayerBodyPoseSource bodyPoseSource;

        [Header("Start Gate")]
        [SerializeField, Min(0f)] private float startMovementThreshold = 0.02f;
        [SerializeField, Min(0f)] private float startPreRollDuration = 1f;

        private readonly List<EchoFrame> frames = new List<EchoFrame>();
        private readonly List<RecordedMachineGrip> machineGrips = new List<RecordedMachineGrip>();
        private float recordingStartTime;
        private Vector3 armedPosition;
        private Quaternion armedRotation;
        private float lastObservedTime;
        private Vector3 lastObservedPosition;
        private Quaternion lastObservedRotation;
        private int nextSampleIndex;
        private RecordedBodyPose armedBodyPose, lastObservedBodyPose;

        public bool ReachedDurationLimit { get; private set; }

        public bool IsRecording { get; private set; }

        public bool HasRecordingStarted { get; private set; }

        public bool IsArmed => IsRecording && !HasRecordingStarted;

        public int CapturedFrameCount => frames.Count;

        public float SampleInterval => sampleInterval;
        public bool RecordsBodyPose => bodyPoseSource != null;

        public float MaxRecordingDuration => maxRecordingDuration;

        public float StartMovementThreshold => startMovementThreshold;

        public float StartPreRollDuration => startPreRollDuration;
        public int CapturedMachineGripCount => machineGrips.Count;
        public bool CaptureMachineGrip(int target, bool held)
        {
            if (!IsRecording || !HasRecordingStarted || ReachedDurationLimit || target <= 0) return false;
            machineGrips.Add(new RecordedMachineGrip(ElapsedTime, target, held));
            return true;
        }

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
                    Quaternion.Slerp(lastObservedRotation, observedRotation, interpolation),
                    RecordedBodyPose.Lerp(lastObservedBodyPose, CurrentBodyPose, interpolation));
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
                        Quaternion.Slerp(lastObservedRotation, observedRotation, interpolation),
                        RecordedBodyPose.Lerp(lastObservedBodyPose, CurrentBodyPose, interpolation));
                }

                ReachedDurationLimit = true;
            }

            lastObservedTime = observedTime;
            lastObservedPosition = observedPosition;
            lastObservedRotation = observedRotation;
            lastObservedBodyPose = CurrentBodyPose;
        }

        public void BeginRecording()
        {
            int expectedFrameCount = Mathf.CeilToInt(maxRecordingDuration / sampleInterval) + 2;
            if (frames.Capacity < expectedFrameCount)
            {
                frames.Capacity = expectedFrameCount;
            }

            frames.Clear();
            machineGrips.Clear();
            recordingStartTime = 0f;
            armedPosition = transform.position;
            armedRotation = transform.rotation;
            armedBodyPose = CurrentBodyPose;
            lastObservedTime = 0f;
            lastObservedPosition = armedPosition;
            lastObservedRotation = armedRotation;
            lastObservedBodyPose = armedBodyPose;
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
            return new EchoRecording(frames, machineGrips);
        }

        public void CancelRecording()
        {
            IsRecording = false;
            HasRecordingStarted = false;
            ReachedDurationLimit = false;
            frames.Clear();
            machineGrips.Clear();
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
                CaptureFrame(0f, armedPosition, armedRotation, armedBodyPose);
                int preRollSampleIndex = 1;

                while (preRollSampleIndex * sampleInterval < preRollDuration)
                {
                    CaptureFrame(
                        preRollSampleIndex * sampleInterval,
                        armedPosition,
                        armedRotation, armedBodyPose);
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
            lastObservedBodyPose = CurrentBodyPose;
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
            => CaptureFrame(timestamp, position, rotation, CurrentBodyPose);

        private RecordedBodyPose CurrentBodyPose => bodyPoseSource ? bodyPoseSource.Current : default;

        private void CaptureFrame(float timestamp, Vector3 position, Quaternion rotation, RecordedBodyPose pose)
        {
            frames.Add(new EchoFrame(timestamp, position, rotation, pose));
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

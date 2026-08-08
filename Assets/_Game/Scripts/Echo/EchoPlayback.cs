using UnityEngine;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        private Rigidbody echoBody;
        private float playbackStartTime;

        public EchoRecording Recording { get; private set; }

        public bool IsInitialized => Recording != null;

        public bool IsComplete { get; private set; }

        public float ElapsedTime { get; private set; }

        private void Awake()
        {
            echoBody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (Recording == null || IsComplete)
            {
                return;
            }

            ElapsedTime = Mathf.Min(Time.time - playbackStartTime, Recording.Duration);
            Recording.Sample(ElapsedTime, out Vector3 position, out Quaternion rotation);

            if (ElapsedTime < Recording.Duration)
            {
                ApplyPose(position, rotation);
                return;
            }

            ApplyPoseImmediately(position, rotation);
            IsComplete = true;
            gameObject.SetActive(false);
        }

        public bool Initialize(EchoRecording recording)
        {
            if (recording == null)
            {
                Debug.LogError("EchoPlayback cannot initialize without a recording.", this);
                enabled = false;
                return false;
            }

            Recording = recording;
            playbackStartTime = Time.time;
            ElapsedTime = 0f;
            IsComplete = recording.Duration <= 0f;

            recording.Sample(0f, out Vector3 position, out Quaternion rotation);
            ApplyPoseImmediately(position, rotation);

            if (IsComplete)
            {
                gameObject.SetActive(false);
            }

            enabled = true;
            return true;
        }

        private void ApplyPose(Vector3 position, Quaternion rotation)
        {
            if (echoBody != null && echoBody.isKinematic)
            {
                echoBody.MovePosition(position);
                echoBody.MoveRotation(rotation);
                return;
            }

            transform.SetPositionAndRotation(position, rotation);
        }

        private void ApplyPoseImmediately(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            if (echoBody != null && echoBody.isKinematic)
            {
                Physics.SyncTransforms();
            }
        }
    }
}

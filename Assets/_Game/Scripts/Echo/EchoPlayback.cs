using UnityEngine;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        private Rigidbody echoBody;
        private float playbackStartTime;
        [SerializeField] private EchoBodyRig bodyRig;
        private RecordedMachineControl[] machineControls;

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
            if(bodyRig)bodyRig.Apply(Recording.SampleBodyPose(ElapsedTime));
            ApplyMachineGrips(ElapsedTime, ElapsedTime < Recording.Duration);

            if (ElapsedTime < Recording.Duration)
            {
                ApplyPose(position, rotation);
                return;
            }

            ApplyPoseImmediately(position, rotation);
            IsComplete = true;
            gameObject.SetActive(false);
        }

        public bool Initialize(EchoRecording recording, RecordedMachineControl[] controls = null)
        {
            if (recording == null)
            {
                Debug.LogError("EchoPlayback cannot initialize without a recording.", this);
                enabled = false;
                return false;
            }

            Recording = recording;
            machineControls = controls;
            playbackStartTime = Time.time;
            ElapsedTime = 0f;
            IsComplete = recording.Duration <= 0f;

            recording.Sample(0f, out Vector3 position, out Quaternion rotation);
            if(bodyRig)bodyRig.Apply(recording.SampleBodyPose(0));
            ApplyMachineGrips(0, !IsComplete);
            ApplyPoseImmediately(position, rotation);

            if (IsComplete)
            {
                gameObject.SetActive(false);
            }

            enabled = true;
            return true;
        }

        private void ApplyMachineGrips(float time, bool active)
        {
            bool any = false;
            if (machineControls != null) foreach (var control in machineControls)
                if (control)
                {
                    bool held = active && Recording.SampleMachineGrip(time, control.TargetId);
                    control.ApplyReplay(this, held); any |= held;
                }
            if (bodyRig) bodyRig.ApplyMachineGrip(any);
        }
        private void OnDisable()
        {
            if (machineControls != null) foreach (var control in machineControls)
                if (control) control.ReleaseReplay(this);
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

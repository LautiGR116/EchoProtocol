using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // A mechanical repeater of the live mechanism. It never requests a door,
    // synthesizes an Echo action, or substitutes a timer for the real interlock.
    [DisallowMultipleComponent, DefaultExecutionOrder(160)]
    public sealed class P2CausalFeedback : MonoBehaviour, ILoopResettable
    {
        [Header("Live mechanism")]
        [SerializeField] private BlindAlignmentRelay relay;
        [SerializeField] private OpticalReader reader;
        [Header("Matching gaze contacts and actual transfer interlock")]
        [SerializeField] private Transform readerArmature;
        [SerializeField] private Transform witnessArmature;
        [SerializeField] private Transform transferBolt;
        [SerializeField] private Renderer powerRail;
        [SerializeField] private Renderer gazeRail;
        [SerializeField] private Vector3 armatureOffset = new Vector3(.055f, 0f, 0f);
        [SerializeField] private Vector3 boltRetractionOffset = new Vector3(-.09f, 0f, 0f);
        [SerializeField, Min(.01f)] private float travelSpeed = .45f;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly Color Unpowered = new Color(.1f, .12f, .14f);
        private static readonly Color Power = new Color(.35f, .85f, .8f);
        private static readonly Color Gaze = new Color(1f, .62f, .2f);
        private Vector3 readerRest, witnessRest, boltRest;
        private MaterialPropertyBlock properties;
        private bool capturedRest;

        public bool IsConfigured => relay && relay.RequiresPoweredClosure && reader
            && readerArmature && witnessArmature && transferBolt && powerRail && gazeRail;

        private void Awake()
        {
            if (!IsConfigured)
            {
                Debug.LogError("P2 causal feedback requires its powered relay, reader, mechanical contacts and rails.", this);
                enabled = false;
                return;
            }
            CaptureRest();
            ResetLoopState();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            bool powered = relay.IsPowered;
            // Includes the qualification interval: the gaze contact must never
            // suggest that looking at the reader can release the second door.
            bool facing = powered && reader.HasFacingPresence;
            bool released = relay.IsExitAuthorized;

            Move(readerArmature, readerRest, facing ? armatureOffset : Vector3.zero);
            Move(witnessArmature, witnessRest, facing ? armatureOffset : Vector3.zero);
            Move(transferBolt, boltRest, released ? boltRetractionOffset : Vector3.zero);
            // The power rail stays steady while the separate contact changes
            // shape and position. Color is a redundant cue, never the only one.
            Tint(powerRail, powered ? Power : Unpowered);
            Tint(gazeRail, facing ? Gaze : Unpowered);
        }

        private void Move(Transform part, Vector3 rest, Vector3 offset)
        {
            // Default travel is 0.12–0.20 s; even customized offsets finish
            // within 0.3 s instead of looking like another qualification timer.
            float speed = Mathf.Max(travelSpeed,
                Mathf.Max(armatureOffset.magnitude, boltRetractionOffset.magnitude) / .3f);
            part.localPosition = Vector3.MoveTowards(part.localPosition,
                rest + offset, Time.deltaTime * speed);
        }

        private void Tint(Renderer rail, Color color)
        {
            if (!rail) return;
            properties ??= new MaterialPropertyBlock();
            rail.GetPropertyBlock(properties);
            properties.SetColor(BaseColor, color);
            properties.SetColor(EmissionColor, color * .65f);
            rail.SetPropertyBlock(properties);
        }

        private void CaptureRest()
        {
            if (capturedRest || !readerArmature || !witnessArmature || !transferBolt) return;
            readerRest = readerArmature.localPosition;
            witnessRest = witnessArmature.localPosition;
            boltRest = transferBolt.localPosition;
            capturedRest = true;
        }

        public void ResetLoopState()
        {
            // The parent may reset before this component's Awake. Capture once
            // so repeated recalls cannot progressively shift the contact poses.
            CaptureRest();
            if (capturedRest)
            {
                if (readerArmature) readerArmature.localPosition = readerRest;
                if (witnessArmature) witnessArmature.localPosition = witnessRest;
                if (transferBolt) transferBolt.localPosition = boltRest;
            }
            Tint(powerRail, Unpowered);
            Tint(gazeRail, Unpowered);
        }

    }
}

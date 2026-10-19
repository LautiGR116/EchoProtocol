using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    /// <summary>An optional physical receiver. The house owns the answered consequence;
    /// this component owns only its present-tense pose, cord and contact sounds.</summary>
    [DisallowMultipleComponent]
    public sealed class HouseTelephone : MonoBehaviour
    {
        [SerializeField] private HouseMemory house;
        [SerializeField] private GameSession session;
        [SerializeField] private Transform handset;
        [SerializeField] private Transform rest;
        [SerializeField] private Transform offhook;
        [SerializeField] private Transform cordBase;
        [SerializeField] private Transform cordHandset;
        [SerializeField] private Transform cordGuide;
        [SerializeField] private LineRenderer cord;
        [SerializeField] private AudioSource contactAudio;
        [SerializeField] private AudioClip lift;
        [SerializeField] private AudioClip setDown;
        [SerializeField, Min(.2f)] private float motionSeconds = .65f;
        [SerializeField, Min(0)] private float liftHeight = .16f;
        private bool ready, offHook, moving;
        private float elapsed;
        private Vector3 fromPosition, toPosition;
        private Quaternion fromRotation, toRotation;
        private const int CordSamples = 145;
        private readonly Vector3[] cordPoints = new Vector3[CordSamples];

        public bool IsOffHook => offHook;
        public bool IsMoving => moving;
        public string Prompt => offHook ? "Colgar el teléfono" : "Descolgar el teléfono";

        private void Awake()
        {
            ready = house && session && handset && rest && offhook && cordBase && cordHandset && cordGuide && cord && contactAudio;
            if (!ready) { Debug.LogError("HouseTelephone requires explicit handset, poses, cord, sound, house and session references.", this); enabled = false; return; }
            contactAudio.playOnAwake = false;
            contactAudio.Stop();
            handset.SetPositionAndRotation(rest.position, rest.rotation);
            cord.useWorldSpace = true;
            cord.positionCount = CordSamples;
            UpdateCord();
        }

        public bool CanUse(GameObject who) => ready && isActiveAndEnabled && !moving && house.AllowsInteraction(who);

        public void Use(GameObject who)
        {
            if (!CanUse(who)) return;
            if (!house.HasAnswered) house.Use(HouseInteractionKind.Telephone, who, cordBase.position);
            if (!house.HasAnswered) return;
            offHook = !offHook;
            fromPosition = handset.position; fromRotation = handset.rotation;
            var target = offHook ? offhook : rest;
            toPosition = target.position; toRotation = target.rotation;
            elapsed = 0; moving = true;
            contactAudio.Stop();
            var clip = offHook ? lift : setDown;
            if (clip) contactAudio.PlayOneShot(clip);
        }

        private void LateUpdate()
        {
            if (!ready || !moving || session.IsPaused) return;
            if (moving)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / motionSeconds);
                float eased = t * t * (3f - 2f * t);
                handset.SetPositionAndRotation(Vector3.Lerp(fromPosition, toPosition, eased) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * liftHeight),
                    Quaternion.Slerp(fromRotation, toRotation, eased));
                if (t >= 1f) moving = false;
            }
            UpdateCord();
        }

        private void UpdateCord()
        {
            Vector3 a = cordBase.position, b = cordGuide.position, c = cordHandset.position;
            for (int i = 0; i < CordSamples; i++)
            {
                float t = i / (float)(CordSamples - 1), u = 1f - t;
                Vector3 centre = u * u * a + 2f * u * t * b + t * t * c;
                Vector3 tangent = (u * (b - a) + t * (c - b)).normalized;
                Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized;
                if (side.sqrMagnitude < .1f) side = Vector3.right;
                Vector3 up = Vector3.Cross(side, tangent).normalized;
                float radius = .005f * Mathf.Clamp01(Mathf.Min(t, 1f - t) * 16f);
                float angle = t * Mathf.PI * 36f;
                cordPoints[i] = centre + radius * (Mathf.Cos(angle) * side + Mathf.Sin(angle) * up);
            }
            cord.SetPositions(cordPoints);
        }

        private void OnDisable() { if (contactAudio) contactAudio.Stop(); }
    }
}

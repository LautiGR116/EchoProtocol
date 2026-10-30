using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // Consequences belong to the room in the present; this never touches a
    // recording, the player camera, traversal collisions or the threat's AI.
    [DefaultExecutionOrder(340), DisallowMultipleComponent]
    public sealed class NegativeUnsealingSequence : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] NegativeRitualSequence ritual;
        [SerializeField] SessionAudio output;
        [SerializeField] Transform player;
        [SerializeField] Camera view;
        [SerializeField] Transform leftVeil, rightVeil, captiveHead, captiveHand;
        [SerializeField] Transform hangingBody, loweringCable;
        [SerializeField] Light captiveLight, suspendedLight;
        [Tooltip("Peel at the chamber, dry material at the captive hand, hoist at the visible suspension.")]
        [SerializeField] AudioSource[] cues;

        Quaternion leftRest, rightRest, headRest, headTarget, hangingRest;
        Vector3 handRest, hangingOrigin, cableScale;
        float[] gains;
        float unsealAge = -1, headAge = -1, dropAge = -1;
        bool ready, peelPlayed, interrupted, completed;
        public float Unsealed01 { get; private set; }
        public float Descent01 { get; private set; }
        public float AmbienceGain { get; private set; } = 1;
        public int Unsealings { get; private set; }
        public int HeadResponses { get; private set; }
        public int Descents { get; private set; }

        void Awake()
        {
            ready = session && encounter && ritual && output && player && view
                && leftVeil && rightVeil && captiveHead && captiveHand && hangingBody && loweringCable
                && captiveLight && suspendedLight && cues != null && cues.Length == 3;
            if (ready)
            {
                foreach (var source in cues) ready &= source && source.clip && source.outputAudioMixerGroup;
                foreach (var moving in new[] {leftVeil, rightVeil, hangingBody})
                    ready &= moving.GetComponentsInChildren<Collider>(true).Length == 0;
            }
            if (!ready)
            {
                Debug.LogError("Negative unsealing requires its explicit collider-free anatomy, routed cues and real encounter references.", this);
                enabled = false; return;
            }
            leftRest = leftVeil.localRotation; rightRest = rightVeil.localRotation;
            headRest = captiveHead.localRotation; handRest = captiveHand.localPosition;
            hangingOrigin = hangingBody.position; hangingRest = hangingBody.localRotation;
            cableScale = loweringCable.localScale;
            gains = new float[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                gains[i] = cues[i].volume;
                cues[i].playOnAwake = cues[i].loop = cues[i].ignoreListenerPause = false;
                cues[i].Stop();
            }
            ApplyForms();
        }

        void LateUpdate()
        {
            if (!ready || !session.HasStarted || !encounter.Active) return;
            // Capture also pauses. Consume pending responses before that guard
            // so a retry cannot resume or discharge a sound from the old run.
            if (encounter.Failed) { interrupted = true; StopCues(); AmbienceGain = 1; return; }
            if (session.IsPaused || Time.deltaTime <= 0) return;
            if (encounter.Complete)
            {
                if (!completed)
                {
                    completed = true; StopCues(); Unsealed01 = Descent01 = 1;
                    ApplyForms();
                }
                AmbienceGain = 1; return;
            }
            if (interrupted) return;
            float dt = Time.deltaTime;

            if (unsealAge < 0 && ritual.WallStrikes > 0) unsealAge = 0;
            if (unsealAge >= 0)
            {
                unsealAge += dt;
                if (!peelPlayed && unsealAge >= 1.1f)
                { peelPlayed = true; Unsealings++; Play(0); }
                Unsealed01 = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.1f, 2.65f, unsealAge));
            }

            Vector3 near = player.position - captiveHead.position; near.y = 0;
            if (headAge < 0 && Unsealed01 > .7f && near.sqrMagnitude < 2.4f * 2.4f)
            {
                headAge = 0; HeadResponses++;
                var direction = captiveHead.parent.InverseTransformDirection(view.transform.position - captiveHead.position);
                float yaw = Mathf.Clamp(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, -32, 32);
                float pitch = Mathf.Clamp(-Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg, -18, 28);
                headTarget = headRest * Quaternion.Euler(pitch, yaw, -19);
                Play(1);
            }
            if (headAge >= 0)
            {
                headAge += dt;
                // Hold, articulate once, then remain looking at where the
                // player was. No continuous tracking or repeated head jitter.
                float motion = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .29f, headAge));
                captiveHead.localRotation = Quaternion.Slerp(headRest, headTarget, motion);
                float reach = motion * (1 - .42f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.5f, 1.1f, headAge)));
                captiveHand.localPosition = handRest + Vector3.forward * (.035f * reach);
            }

            // Commit to the already opened return route before lowering the
            // suspended mass. Even at its low point feet stay above 2.3 m.
            var distance = player.position - hangingOrigin; distance.y = 0;
            if (dropAge < 0 && encounter.ExitReleased && player.position.x > -2.05f && distance.sqrMagnitude < 4.1f * 4.1f)
            { dropAge = 0; Descents++; Play(2); }
            if (dropAge >= 0)
            {
                dropAge += dt;
                Descent01 = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.18f, 1.25f, dropAge));
                float recoilAge = Mathf.Max(0, dropAge - 1.25f);
                float sway = dropAge < 1.25f ? 0 : Mathf.Sin(recoilAge * 2.8f) * Mathf.Exp(-recoilAge * .65f) * 4;
                hangingBody.localRotation = hangingRest * Quaternion.Euler(0, 0, sway);
            }
            ApplyForms();
            float peelBed = unsealAge >= 1.0f && unsealAge < 3.8f ? .32f : 1;
            float dropBed = dropAge >= 0 && dropAge < 2.1f ? .20f : 1;
            AmbienceGain = Mathf.Min(peelBed, dropBed);
        }

        void ApplyForms()
        {
            leftVeil.localRotation = leftRest * Quaternion.Euler(0, 70 * Unsealed01, 0);
            rightVeil.localRotation = rightRest * Quaternion.Euler(0, -70 * Unsealed01, 0);
            hangingBody.position = hangingOrigin + Vector3.down * (1.85f * Descent01);
            loweringCable.localScale = new Vector3(cableScale.x, Mathf.Max(.1f, loweringCable.position.y - hangingBody.position.y), cableScale.z);
            captiveLight.intensity = Mathf.Lerp(.12f, 2.3f, Unsealed01);
            suspendedLight.intensity = Mathf.Lerp(.75f, 2.7f, Descent01);
        }
        void Play(int i) { cues[i].volume = gains[i] * (output.ReducedDynamics ? .65f : 1); cues[i].Play(); }
        void StopCues() { if (cues != null) foreach (var source in cues) if (source) source.Stop(); }
        void OnDisable() { StopCues(); AmbienceGain = 1; }
    }
}

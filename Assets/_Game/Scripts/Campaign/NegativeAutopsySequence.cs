using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // A physical response of the two present sculptures. The suspension root
    // stays owned by Unsealing; no recording, camera or encounter rule changes.
    [DefaultExecutionOrder(360), DisallowMultipleComponent]
    public sealed class NegativeAutopsySequence : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] NegativeUnsealingSequence unsealing;
        [SerializeField] SessionAudio output;
        [SerializeField] Transform player;
        [SerializeField] Camera view;
        [SerializeField] Transform leftChestFlap, rightChestFlap;
        [SerializeField] Transform hangingNeck, hangingJaw, captiveJaw;
        [SerializeField] Light cavityLight;
        [Tooltip("0 suture at the chest, 1 thorax at the cavity, 2 captive jaw, 3 air at the hanging mouth.")]
        [SerializeField] AudioSource[] cues;

        Quaternion leftRest, rightRest, neckRest, neckTarget, hangingJawRest, captiveJawRest;
        float[] gains;
        float cavityRest, captiveAge = -1, returnAge = -1;
        bool ready, interrupted, completed, suturePlayed;

        public int CaptiveResponses { get; private set; }
        public int ThoraxOpenings { get; private set; }
        public int ReturnLooks { get; private set; }
        public int AfterBreaths { get; private set; }
        public float Open01 { get; private set; }
        public float CaptiveJaw01 { get; private set; }
        public float ReturnLook01 { get; private set; }
        public float HangingJaw01 { get; private set; }
        public float ReturnAge => returnAge;
        public float AmbienceGain { get; private set; } = 1;

        void Awake()
        {
            ready = session && encounter && unsealing && output && player && view
                && leftChestFlap && rightChestFlap && hangingNeck && hangingNeck.parent
                && hangingJaw && captiveJaw && cavityLight && cues != null && cues.Length == 4;
            if (ready)
            {
                foreach (var moving in new[] {leftChestFlap, rightChestFlap, hangingNeck, hangingJaw, captiveJaw})
                    ready &= moving.GetComponentsInChildren<Collider>(true).Length == 0;
                for (int i = 0; i < cues.Length; i++)
                {
                    ready &= cues[i] && cues[i].clip && cues[i].outputAudioMixerGroup;
                    for (int j = 0; j < i; j++) ready &= cues[i] != cues[j];
                }
            }
            if (!ready)
            {
                Debug.LogError("Autopsy requires explicit collider-free anatomy, four dedicated routed sources and the actual Unsealing encounter.", this);
                enabled = false; return;
            }
            leftRest = leftChestFlap.localRotation; rightRest = rightChestFlap.localRotation;
            neckRest = neckTarget = hangingNeck.localRotation;
            hangingJawRest = hangingJaw.localRotation; captiveJawRest = captiveJaw.localRotation;
            cavityRest = cavityLight.intensity;
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
            if (!ready) return;
            // Capture pauses in the same frame. Cancel before the pause guard,
            // consuming pending accents so retry cannot discharge an old scare.
            if (encounter.Failed)
            { interrupted = true; StopCues(); AmbienceGain = 1; return; }
            if (!session.HasStarted || !encounter.Active || session.IsPaused || Time.deltaTime <= 0) return;
            if (encounter.Complete)
            {
                if (!completed)
                {
                    completed = true; StopCues();
                    // Settle only movement that actually started. A checkpoint
                    // or a fast exit never invents a gesture or event count.
                    CaptiveJaw01 = CaptiveResponses > 0 ? 1 : 0;
                    ReturnLook01 = ReturnLooks > 0 ? 1 : 0;
                    Open01 = HangingJaw01 = ThoraxOpenings > 0 ? 1 : 0;
                    ApplyForms();
                }
                AmbienceGain = 1; return;
            }
            if (interrupted || completed) return;

            float dt = Time.deltaTime;
            if (captiveAge < 0 && unsealing.HeadResponses > 0) captiveAge = 0;
            if (captiveAge >= 0)
            {
                captiveAge += dt;
                if (CaptiveResponses == 0 && captiveAge >= .4f)
                { CaptiveResponses++; Play(2); }
                CaptiveJaw01 = Smooth(.4f, .56f, captiveAge);
            }

            Vector3 distance = player.position - hangingNeck.position; distance.y = 0;
            if (returnAge < 0 && unsealing.Descent01 > .96f && distance.sqrMagnitude < 6.25f)
            {
                returnAge = 0;
                Vector3 direction = hangingNeck.parent.InverseTransformDirection(view.transform.position - hangingNeck.position);
                float yaw = Mathf.Clamp(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, -45, 45);
                float pitch = Mathf.Clamp(-Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg, -30, 35);
                // The sampled gaze releases the saved slumped lean with one
                // lateral cant. It never tracks the player continuously.
                neckTarget = Quaternion.Euler(pitch, yaw, 14);
            }
            if (returnAge >= 0)
            {
                returnAge += dt;
                // The hoist brake lands first. This source's main fibre tear
                // occurs .45s later, as the chest begins to separate.
                if (!suturePlayed && returnAge >= .15f)
                { suturePlayed = true; Play(0); }
                if (ReturnLooks == 0 && returnAge >= .28f) ReturnLooks++;
                if (ThoraxOpenings == 0 && returnAge >= .55f)
                { ThoraxOpenings++; Play(1); }
                if (AfterBreaths == 0 && returnAge >= 1.4f)
                { AfterBreaths++; Play(3); }
                ReturnLook01 = Smooth(.28f, .44f, returnAge);
                Open01 = Smooth(.55f, 1.25f, returnAge);
                HangingJaw01 = Smooth(.65f, .82f, returnAge);
            }
            ApplyForms();
            float captiveBed = captiveAge >= .15f && captiveAge < .9f ? .45f : 1;
            float returnBed = returnAge >= 0 ? Mathf.Lerp(.1f, 1, Smooth(1.8f, 3.4f, returnAge)) : 1;
            AmbienceGain = Mathf.Min(captiveBed, returnBed);
        }

        void ApplyForms()
        {
            leftChestFlap.localRotation = leftRest * Quaternion.Euler(0, -80 * Open01, 0);
            rightChestFlap.localRotation = rightRest * Quaternion.Euler(0, 80 * Open01, 0);
            hangingNeck.localRotation = Quaternion.Slerp(neckRest, neckTarget, ReturnLook01);
            hangingJaw.localRotation = hangingJawRest * Quaternion.Euler(35 * HangingJaw01, 0, 0);
            captiveJaw.localRotation = captiveJawRest * Quaternion.Euler(32 * CaptiveJaw01, 0, 0);
            float reveal = Mathf.Lerp(cavityRest, 2.4f, Open01);
            float settle = completed ? 1 : Smooth(1.8f, 3.4f, returnAge);
            cavityLight.intensity = Mathf.Lerp(reveal, .8f, settle * Open01);
        }

        static float Smooth(float from, float to, float age) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(from, to, age));
        void Play(int index) { cues[index].volume = gains[index] * (output.ReducedDynamics ? .65f : 1); cues[index].Play(); }
        void StopCues() { if (cues != null) foreach (var source in cues) if (source) source.Stop(); }
        void OnDisable() { StopCues(); AmbienceGain = 1; }
    }
}

using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // The apparatus has physical consequences in the present. A take is never
    // inspected or edited here, and abandoning an interaction remains possible.
    [DefaultExecutionOrder(320), DisallowMultipleComponent]
    public sealed class NegativeRitualSequence : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] MachineCoverThreat threat;
        [SerializeField] MachineCoverInteraction crank;
        [SerializeField] NegativeRoomSequence room;
        [SerializeField] SessionAudio output;
        [SerializeField] Transform closeRelief, impactRelief, impactPanel;
        [SerializeField] Transform[] suspendedForms;
        [SerializeField] Light closeLight, impactLight;
        [Tooltip("0 inhale, 1 close drag, 2 wall strike, 3 overhead strain, 4 remote answer. Dedicated present-world sources.")]
        [SerializeField] AudioSource[] cues;

        Vector3 closeScale, impactScale, panelRest;
        Quaternion[] suspensionRest;
        float[] levels;
        float closeAge = -1, impactAge = -1, suspensionAge = -1, answerAge = -1;
        float closeDepth = .14f;
        bool ready, dragPlayed, impactPlayed, answerPlayed, settled;
        public int CloseReveals { get; private set; }
        public int WallStrikes { get; private set; }
        public int OverheadStrains { get; private set; }
        public int RemoteAnswers { get; private set; }
        public float CloseDepth01 => closeDepth;
        public float AmbienceGain { get; private set; } = 1;

        void Awake()
        {
            ready = session && encounter && threat && crank && room && output
                && closeRelief && impactRelief && impactPanel && closeLight && impactLight
                && suspendedForms != null && suspendedForms.Length > 0 && cues != null && cues.Length == 5;
            if (ready)
            {
                foreach (var form in suspendedForms) ready &= form && form.GetComponentsInChildren<Collider>(true).Length == 0;
                foreach (var source in cues) ready &= source && source.clip && source.outputAudioMixerGroup;
                ready &= impactPanel.GetComponentsInChildren<Collider>(true).Length == 0;
            }
            if (!ready)
            {
                Debug.LogError("Negative ritual requires its explicit, collider-free forms, routed sources and encounter references.", this);
                enabled = false; return;
            }
            // Authored reliefs are saved compressed on their local depth axis.
            closeScale = new Vector3(closeRelief.localScale.x, closeRelief.localScale.y, 1);
            impactScale = new Vector3(impactRelief.localScale.x, impactRelief.localScale.y, 1);
            panelRest = impactPanel.localPosition;
            suspensionRest = new Quaternion[suspendedForms.Length];
            for (int i = 0; i < suspensionRest.Length; i++) suspensionRest[i] = suspendedForms[i].localRotation;
            levels = new float[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                levels[i] = cues[i].volume;
                cues[i].loop = cues[i].playOnAwake = cues[i].ignoreListenerPause = false;
                cues[i].Stop();
            }
            SetDepth(closeRelief, closeScale, .14f); SetDepth(impactRelief, impactScale, .14f);
            closeLight.intensity = .32f; impactLight.intensity = .25f;
        }

        void LateUpdate()
        {
            if (!ready || !session.HasStarted || !encounter.Active || session.IsPaused || Time.deltaTime <= 0) return;
            if (encounter.Complete)
            {
                if (!settled)
                {
                    settled = true; StopCues();
                    closeDepth = 1; SetDepth(closeRelief, closeScale, 1); SetDepth(impactRelief, impactScale, .72f);
                    impactPanel.localPosition = panelRest;
                    for (int i = 0; i < suspensionRest.Length; i++) suspendedForms[i].localRotation = suspensionRest[i];
                }
                AmbienceGain = 1; return;
            }
            if (encounter.Failed) { StopCues(); AmbienceGain = 1; return; }

            float dt = Time.deltaTime;
            if (suspensionAge < 0 && room.Stage == NegativeRoomSequence.RoomStage.Listening)
            { suspensionAge = 0; OverheadStrains++; Play(3); }
            if (suspensionAge >= 0) suspensionAge += dt;

            // Actual ratchet progress exposes the form. Releasing the grip stops
            // its growth; the material remembers its deformation across RECALL.
            if (crank.IsHolding && crank.Fraction >= .18f && closeAge < 0)
            { closeAge = 0; CloseReveals++; Play(0); }
            if (closeAge >= 0)
            {
                closeAge += dt;
                if (crank.IsHolding)
                {
                    float target = Mathf.Lerp(.14f, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.18f, .86f, crank.Fraction)));
                    closeDepth = Mathf.Max(closeDepth, target);
                    if (!dragPlayed && crank.Fraction >= .48f)
                    { dragPlayed = true; cues[0].Stop(); Play(1); }
                }
                SetDepth(closeRelief, closeScale, closeDepth);
                closeLight.intensity = Mathf.Lerp(.32f, 1.45f, closeDepth);
            }

            // This is the observed replay pump stopping, not an arbitrary timer.
            // Leave its first pressure release audible, subtract the room bed,
            // then deliver one physical strike with visible material recoil.
            if (impactAge < 0 && room.SilenceResponsePlayed) impactAge = 0;
            if (impactAge >= 0)
            {
                impactAge += dt;
                if (!impactPlayed && impactAge >= .68f)
                { impactPlayed = true; WallStrikes++; cues[0].Stop(); cues[1].Stop(); cues[3].Stop(); Play(2); }
                float age = impactAge - .68f;
                float punch = age < 0 ? 0 : Mathf.SmoothStep(0, 1, age / .055f);
                float release = age < .1f ? 0 : Mathf.SmoothStep(0, 1, (age - .1f) / .9f);
                float depth = Mathf.Lerp(.14f, 1, punch) - .28f * release;
                SetDepth(impactRelief, impactScale, depth);
                float recoil = age < 0 ? 0 : Mathf.Sin(age * 24) * Mathf.Exp(-age * 6) * .065f;
                impactPanel.localPosition = panelRest + impactPanel.localRotation * Vector3.forward * recoil;
                impactLight.intensity = .25f + 1.25f * punch - .55f * release;
            }

            // The far apparatus answers only after the real intruder reaches
            // the running machine. Its source remains at that far structure.
            if (answerAge < 0 && threat.Phase == MachineCoverThreat.Intention.MachineContact && encounter.Machine.Held)
                answerAge = 0;
            if (answerAge >= 0)
            {
                answerAge += dt;
                if (!answerPlayed && answerAge >= .8f && threat.Phase == MachineCoverThreat.Intention.MachineContact)
                { answerPlayed = true; RemoteAnswers++; Play(4); }
            }

            for (int i = 0; i < suspendedForms.Length; i++)
            {
                float a = suspensionAge - i * .22f;
                float sway = a < 0 ? 0 : Mathf.Sin(a * 1.8f) * Mathf.Exp(-a * .35f) * 2.8f;
                if (impactAge >= .68f)
                {
                    float b = impactAge - .68f;
                    sway += Mathf.Sin(b * 2.25f + i * .7f) * Mathf.Exp(-b * .55f) * 1.8f;
                }
                suspendedForms[i].localRotation = suspensionRest[i] * Quaternion.Euler(0, 0, sway);
            }
            float bed = impactAge >= 0 && impactAge < .68f ? .08f
                : impactAge >= .68f && impactAge < 2.6f ? Mathf.Lerp(.08f, 1, (impactAge - .68f) / 1.92f) : 1;
            AmbienceGain = bed;
        }

        static void SetDepth(Transform form, Vector3 rest, float depth) => form.localScale = new Vector3(rest.x, rest.y, rest.z * depth);
        void Play(int index) { cues[index].volume = levels[index] * (output.ReducedDynamics ? .65f : 1); cues[index].Play(); }
        void StopCues() { if (cues != null) foreach (var source in cues) if (source) source.Stop(); }
        void OnDisable() { StopCues(); AmbienceGain = 1; }
    }
}

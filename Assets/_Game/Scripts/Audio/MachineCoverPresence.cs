using EchoProtocol.Campaign;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Authored presentation of one present actor. The recording never owns or
    // drives these sources. Occlusion preserves direction and a readable onset.
    [DefaultExecutionOrder(260), DisallowMultipleComponent]
    public sealed class MachineCoverPresence : MonoBehaviour
    {
        [SerializeField] private MachineCoverEncounter encounter;
        [SerializeField] private MachineCoverThreat threat;
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener, frameContact;
        [SerializeField] private AudioSource warning, rush, friction, score;
        [SerializeField] private AudioSource[] spatialSources;
        [SerializeField] private AudioLowPassFilter[] boundaries;
        [SerializeField] private AudioClip[] steps;
        [SerializeField] private Light threatPractical;
        [SerializeField] private Light exitPractical;
        // Optional in older editions. These are present-body emitters, with
        // separate voice, sole/intake, breathing and fixed pump-contact roles.
        [SerializeField] private AudioSource huntVoice, huntBody, huntBreathing, huntContact;
        [SerializeField] private AudioClip launchEffort, reacquireEffort, brakeFoley, searchBreath, machineContact;
        public enum HuntCue { Launch, Brake, Search, Reacquire, MachineContact }
        readonly RaycastHit[] hits = new RaycastHit[32];
        float[] gains, acousticGains;
        bool[] obstructedSources;
        Transform playerRoot;
        Vector3 previous, huntBodyRest;
        float releasedAt, inspectAt, frictionGain, scoreGain, speedNow;
        float lastCueAt = -100, breathQuietUntil = -100, listeningUntil = -100, maskUntil = -100;
        float breathGain = 1;
        bool primed, released, framePlayed, moveHuntBody;
        int stepIndex;
        HuntCue lastCue;
        public int WarningCount { get; private set; }
        public int FrameCount { get; private set; }
        public int StepCount { get; private set; }
        public int HuntCueCount { get; private set; }
        public HuntCue LastHuntCue => lastCue;
        public AudioSource CurrentVoice => huntVoice && huntVoice.isPlaying ? huntVoice : null;
        public bool Obstructed { get; private set; }
        bool HuntConfigured => huntVoice && huntBody && huntBreathing && huntContact;

        void Awake()
        {
            if (!encounter || !threat || !session || !listener || !frameContact || !warning || !rush || !friction || !score
                || spatialSources == null || boundaries == null || spatialSources.Length != boundaries.Length
                || steps == null || steps.Length < 2 || !threatPractical)
            { Debug.LogError("Machine presence requires explicit live actor, acoustic and practical references.", this); enabled = false; return; }
            var controller = listener.GetComponentInParent<EchoProtocol.Player.FirstPersonController>();
            if (!controller) { Debug.LogError("Machine presence listener must belong to the live Player.", this); enabled=false;return; }
            playerRoot = controller.transform;
            gains = new float[spatialSources.Length];
            acousticGains = new float[spatialSources.Length];
            obstructedSources = new bool[spatialSources.Length];
            for (int i=0;i<gains.Length;i++)
            {
                if (!spatialSources[i] || !boundaries[i] || !spatialSources[i].outputAudioMixerGroup)
                { Debug.LogError("Machine presence has an unwired spatial source.", this); enabled=false;return; }
                gains[i] = spatialSources[i].volume;
                acousticGains[i] = 1;
            }
            if (HuntConfigured)
            {
                huntVoice.loop = huntBody.loop = huntContact.loop = false;
                // Only the authored actor-root child can switch between sole
                // and intake height. Other/historical emitter parents stay put.
                moveHuntBody = huntBody.transform.parent == threat.transform;
                huntBodyRest = huntBody.transform.localPosition;
                if (!launchEffort || !reacquireEffort || !brakeFoley || !searchBreath || !machineContact)
                    Debug.LogError("Hunt action audio requires its five traced action clips.", this);
            }
            friction.volume = score.volume = 0;
            ResetPresence();
        }
        public void Prime()
        {
            if (!enabled || primed) return;
            primed = true; WarningCount++; warning.Play();
        }
        public void Release()
        {
            released = true; framePlayed=false; releasedAt=Time.time; previous=threat.transform.position;
        }
        public void Step(AudioSource source)
        {
            if (!enabled) { source.Play(); return; }
            // The pose owns contact timing. Speed changes the support's weight,
            // not the cadence or pitch, and never creates a free-running step.
            AudioClip clip = steps[stepIndex++ % steps.Length];
            if (HuntConfigured)
                source.PlayOneShot(clip, Mathf.Lerp(.62f, 1, Mathf.InverseLerp(.25f, 2.3f, speedNow)));
            else { source.clip = clip; source.Play(); }
            StepCount++;
        }
        public void Cue(HuntCue cue)
        {
            if (!enabled || !HuntConfigured || !released) return;
            // State edges author the sequence. This guard only prevents a
            // duplicate cue within the same transition; no repeating bark timer.
            if (cue == lastCue && Time.time - lastCueAt < .45f) return;
            lastCue = cue; lastCueAt = Time.time; HuntCueCount++;
            maskUntil = Time.time + .48f;
            switch (cue)
            {
                case HuntCue.Launch:
                    huntBody.Stop(); PlayAction(huntVoice, launchEffort);
                    breathQuietUntil = Time.time + .42f;
                    listeningUntil = -100;
                    break;
                case HuntCue.Brake:
                    if (moveHuntBody) huntBody.transform.localPosition = new Vector3(huntBodyRest.x, .12f, huntBodyRest.z);
                    PlayAction(huntBody, brakeFoley);
                    breathQuietUntil = Time.time + .30f;
                    break;
                case HuntCue.Search:
                    if (moveHuntBody) huntBody.transform.localPosition = huntBodyRest;
                    huntVoice.Stop(); PlayAction(huntBody, searchBreath);
                    // Intake, then a held listening space: he stops advertising
                    // his breath but real feet still locate the moving body.
                    breathQuietUntil = Time.time + 1.75f;
                    listeningUntil = Time.time + 1.75f;
                    break;
                case HuntCue.Reacquire:
                    huntBody.Stop(); PlayAction(huntVoice, reacquireEffort);
                    breathQuietUntil = Time.time + .48f;
                    listeningUntil = -100;
                    break;
                case HuntCue.MachineContact:
                    huntVoice.Stop(); huntBody.Stop(); PlayAction(huntContact, machineContact);
                    breathQuietUntil = Time.time + .62f;
                    listeningUntil = -100;
                    break;
            }
        }
        static void PlayAction(AudioSource source, AudioClip clip)
        {
            if (!source || !clip) return;
            source.Stop(); source.clip = clip; source.pitch = 1; source.Play();
        }
        void StopHunt()
        {
            if (huntVoice) huntVoice.Stop();
            if (huntBody) huntBody.Stop();
            if (huntContact) huntContact.Stop();
            if (huntBreathing) huntBreathing.Stop();
            breathQuietUntil = listeningUntil = maskUntil = -100;
        }
        public void Caught()
        {
            released = false; friction.Stop();score.Stop();warning.Stop();rush.Stop();
            frictionGain=scoreGain=0;
            StopHunt();
        }
        public void ResetPresence()
        {
            primed=released=framePlayed=false;stepIndex=0;frictionGain=scoreGain=speedNow=0;
            lastCueAt=-100;breathGain=1;
            StopHunt();
            if (moveHuntBody && huntBody) huntBody.transform.localPosition = huntBodyRest;
            if (warning)warning.Stop();if(rush)rush.Stop();if(friction)friction.Stop();if(score)score.Stop();
            if(threatPractical)threatPractical.intensity=.25f;
            if(exitPractical)exitPractical.intensity=.12f;
            inspectAt=0;
        }
        void LateUpdate()
        {
            if (!enabled || session.IsPaused || Time.deltaTime<=0) return;
            float speed=(threat.transform.position-previous).magnitude/Time.deltaTime;previous=threat.transform.position;
            speedNow = Mathf.MoveTowards(speedNow, speed, Time.deltaTime * 12);
            bool active=released && threat.Released && !encounter.Complete && !encounter.Failed;
            if (released && !active) { released=false; StopHunt(); }
            float age=Time.time-releasedAt;
            bool directed=threat.PerformanceEnabled;
            if(active && !framePlayed && !threat.Intruder && (directed ? threat.Phase==MachineCoverThreat.Intention.Charge : Vector3.Distance(threat.transform.position,frameContact.position)<1.05f))
            { framePlayed=true;FrameCount++;rush.Play(); }
            float motion=active?Mathf.InverseLerp(.05f,3.7f,speed):0;
            float grain=directed?.12f:.22f;
            float music=active&&age>1.4f?.25f:0;
            if(directed)music=active&&(threat.Phase==MachineCoverThreat.Intention.Charge)? .23f : active&&threat.AttractedToMachine?.025f:0;
            if(threat.Intruder)music=0;
            if(threat.Intruder&&threat.Intruder.Embestida)grain*=threat.Intruder.Embestida.MaskingGain;
            bool searching=HuntConfigured&&threat.Phase==MachineCoverThreat.Intention.Search;
            bool listening=HuntConfigured&&Time.time<listeningUntil;
            if(HuntConfigured)
            {
                // Gain subtraction works on WebGL too; the low-pass below is
                // only supplementary. Preserve real pump pressure/noise rules.
                grain*=searching?.30f:threat.AttractedToMachine?.45f:.72f;
                if(listening||Time.time<maskUntil)grain=0;
                float targetBreath=!active||Time.time<breathQuietUntil?0:searching?.26f:threat.AttractedToMachine?.48f:.85f;
                breathGain=Mathf.MoveTowards(breathGain,targetBreath,Time.deltaTime*(targetBreath<breathGain?7:2.5f));
            }
            frictionGain=Mathf.MoveTowards(frictionGain,grain*motion,Time.deltaTime*.8f);
            scoreGain=Mathf.MoveTowards(scoreGain,music,Time.deltaTime*(active?.75f:1.5f));
            if(frictionGain>0&&!friction.isPlaying)friction.Play();
            if(scoreGain>0&&!score.isPlaying)score.Play();
            score.volume=scoreGain;
            threatPractical.intensity=Mathf.MoveTowards(threatPractical.intensity,active?1.65f:.25f,Time.deltaTime*4);
            if(exitPractical)exitPractical.intensity=Mathf.MoveTowards(exitPractical.intensity,active?2.2f:threat.Intruder?1.1f:.12f,Time.deltaTime*5);
            if(Time.time>=inspectAt)
            {
                inspectAt=Time.time+.08f;Obstructed=false;
                for(int i=0;i<spatialSources.Length;i++)
                {
                    var source=spatialSources[i];Vector3 d=source.transform.position-listener.position;
                    bool blocked=false;
                    int n=Physics.RaycastNonAlloc(listener.position,d.normalized,hits,d.magnitude,~0,QueryTriggerInteraction.Ignore);
                    for(int h=0;h<n;h++)
                        if(!hits[h].transform.IsChildOf(playerRoot)&&!hits[h].transform.IsChildOf(threat.transform)
                            &&!hits[h].transform.IsChildOf(source.transform)) {blocked=true;break;}
                    if(n==hits.Length)blocked=true;
                    Obstructed|=blocked;
                    float cutoff=blocked?(d.magnitude<3?6500:3200):14500;
                    boundaries[i].cutoffFrequency=cutoff;
                    obstructedSources[i]=blocked;
                }
            }
            for(int i=0;i<spatialSources.Length;i++)
            {
                var source=spatialSources[i];
                acousticGains[i]=Mathf.MoveTowards(acousticGains[i],obstructedSources[i]?(HuntConfigured?.64f:.76f):1,Time.deltaTime*3.5f);
                float gain=source==friction?frictionGain:gains[i];
                if(source==huntBreathing)gain*=breathGain;
                source.volume=gain*acousticGains[i];
            }
            if(frictionGain<=0&&friction.isPlaying)friction.Stop();
            if(scoreGain<=0&&score.isPlaying)score.Stop();
        }
        void OnDisable(){ResetPresence();}
    }
}

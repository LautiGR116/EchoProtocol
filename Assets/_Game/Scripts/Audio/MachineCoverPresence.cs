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
        readonly RaycastHit[] hits = new RaycastHit[32];
        float[] gains;
        Vector3 previous;
        float releasedAt, inspectAt, frictionGain, scoreGain;
        bool primed, released, framePlayed;
        int stepIndex;
        public int WarningCount { get; private set; }
        public int FrameCount { get; private set; }
        public int StepCount { get; private set; }
        public bool Obstructed { get; private set; }

        void Awake()
        {
            if (!encounter || !threat || !session || !listener || !frameContact || !warning || !rush || !friction || !score
                || spatialSources == null || boundaries == null || spatialSources.Length != boundaries.Length
                || steps == null || steps.Length < 2 || !threatPractical)
            { Debug.LogError("Machine presence requires explicit live actor, acoustic and practical references.", this); enabled = false; return; }
            gains = new float[spatialSources.Length];
            for (int i=0;i<gains.Length;i++)
            {
                if (!spatialSources[i] || !boundaries[i] || !spatialSources[i].outputAudioMixerGroup)
                { Debug.LogError("Machine presence has an unwired spatial source.", this); enabled=false;return; }
                gains[i] = spatialSources[i].volume;
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
            source.clip = steps[stepIndex++ % steps.Length];
            source.Play(); StepCount++;
        }
        public void Caught()
        {
            released = false; friction.Stop();score.Stop();warning.Stop();rush.Stop();
            frictionGain=scoreGain=0;
        }
        public void ResetPresence()
        {
            primed=released=framePlayed=false;stepIndex=0;frictionGain=scoreGain=0;
            if (warning)warning.Stop();if(rush)rush.Stop();if(friction)friction.Stop();if(score)score.Stop();
            if(threatPractical)threatPractical.intensity=.25f;
            if(exitPractical)exitPractical.intensity=.12f;
            inspectAt=0;
        }
        void LateUpdate()
        {
            if (!enabled || session.IsPaused || Time.deltaTime<=0) return;
            float speed=(threat.transform.position-previous).magnitude/Time.deltaTime;previous=threat.transform.position;
            bool active=released && threat.Released && !encounter.Complete && !encounter.Failed;
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
                        if(!hits[h].transform.IsChildOf(listener.GetComponentInParent<EchoProtocol.Player.FirstPersonController>().transform)&&!hits[h].transform.IsChildOf(threat.transform)
                            &&!hits[h].transform.IsChildOf(source.transform)) {blocked=true;break;}
                    if(n==hits.Length)blocked=true;
                    Obstructed|=blocked;
                    float cutoff=blocked?(d.magnitude<3?6500:3200):14500;
                    boundaries[i].cutoffFrequency=cutoff;
                    source.volume=(source==friction?frictionGain:gains[i])*(blocked?.76f:1);
                }
            }
            if(frictionGain<=0&&friction.isPlaying)friction.Stop();
            if(scoreGain<=0&&score.isPlaying)score.Stop();
        }
        void OnDisable(){ResetPresence();}
    }
}

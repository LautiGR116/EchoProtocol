using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    [DefaultExecutionOrder(490),DisallowMultipleComponent]
    public sealed class NegativeAutopsyThroat:MonoBehaviour
    {
        [SerializeField] NegativeUnsealingMask mask;
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        Vector3 rest;
        float age=-1;
        bool interrupted;
        public int Contractions {get;private set;}
        void Awake()
        {
            rest=transform.localScale;
            if(!mask||!session||!encounter){Debug.LogError("Present throat requires explicit mask/session/encounter.",this);enabled=false;}
        }
        void LateUpdate()
        {
            if(!session.HasStarted||!encounter.Active)return;
            if(encounter.Failed){interrupted=true;return;}
            if(session.IsPaused||Time.deltaTime<=0||encounter.Complete||interrupted)return;
            if(age<0&&mask.Open01>.95f){age=0;Contractions++;}
            if(age<0)return;
            age+=Time.deltaTime;
            // One held contraction under the newly broken plates; no idle loop,
            // no voice, and no animation on the recorded body or actor's bones.
            float contract=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.36f,age));
            float release=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,1.65f,age));
            transform.localScale=rest*(1-.14f*contract+.10f*release);
        }
    }
}

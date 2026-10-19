using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // One present body, relocated only across an occluded chapter boundary.
    // This component never writes a recording or supplies an Echo performance.
    [DefaultExecutionOrder(240), DisallowMultipleComponent]
    public sealed class IntruderStaging : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] CampaignDirector campaign;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] MachineCoverThreat threat;
        [SerializeField] MachineThreatPerformance performance;
        [SerializeField] RecombinationRelay returnRelay;
        [SerializeField] Camera view;
        [SerializeField] Transform player, visual, p3Stand, p3Hand, pumpStand, pumpHand;
        [SerializeField] Transform doorHinge, doorHandle, breachHand, breachExit, cornerHand;
        [SerializeField] Collider[] p3Glazing;
        [SerializeField] AudioSource p3Contact, latch, doorImpact, breath, returnMotor, cornerContact;
        [SerializeField] AudioClip doorPressure;
        [SerializeField] EmbestidaRush embestida;
        public EmbestidaRush Embestida => embestida;
        [SerializeField] Light p3Practical;
        readonly RaycastHit[] hits = new RaycastHit[32];
        Quaternion doorRest, handleRest;
        bool ready, p3Placed, p3Retired, p3Sound, primed, pressed, burst, burstVoice, pumpPlaced;
        float preparationAt = -1, witnessAt, quiet;
        float brakeAt=-100;
        bool cornerBraced;
        public Transform PumpStand => pumpStand;
        public Transform PumpHand => pumpHand;
        public Transform BreachHand => breachHand;
        public Transform WitnessHand => p3Hand;
        public Transform CornerHand => cornerHand;
        public float BrakeWeight => 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.15f,.48f,Time.time-brakeAt));
        public bool InPumpContact => threat.Phase == MachineCoverThreat.Intention.MachineContact;
        public float AmbienceMultiplier => encounter.Active && !encounter.Complete ? quiet : 1;
        public int DiscoveryContacts { get; private set; }
        public int BreachImpacts { get; private set; }
        public bool P3Visible => p3Placed && !p3Retired;

        void Awake()
        {
            ready=session&&campaign&&encounter&&threat&&performance&&returnRelay&&view&&player&&visual
                &&p3Stand&&p3Hand&&pumpStand&&pumpHand&&doorHinge&&doorHandle&&breachHand&&breachExit
                &&p3Contact&&latch&&doorImpact&&breath&&returnMotor&&doorPressure&&p3Practical&&cornerHand&&cornerContact;
            if(!ready){Debug.LogError("El intruso requires its P3, physical door, pump and routed sound references.",this);enabled=false;return;}
            doorRest=doorHinge.localRotation;handleRest=doorHandle.localRotation;
            quiet=1;visual.gameObject.SetActive(false);p3Practical.intensity=0;
            foreach(var source in new[]{p3Contact,latch,doorImpact,breath,returnMotor})source.Stop();
        }

        public void ResetPump()
        {
            if(!ready)return;
            preparationAt=-1;primed=pressed=burst=burstVoice=false;
            brakeAt=-100;cornerBraced=false;
            if(embestida)embestida.ResetAct();
            doorHinge.localRotation=doorRest;doorHandle.localRotation=handleRest;
            latch.Stop();doorImpact.Stop();breath.Stop();returnMotor.Stop();
            quiet=1;
            // During P3 RECALL the independent visitor stays in the present.
            if(encounter.Active){p3Retired=pumpPlaced=true;visual.gameObject.SetActive(true);p3Practical.intensity=0;}
        }

        public void BeginBreach()
        {
            burst=burstVoice=false;latch.Stop();breath.Stop();
            if(embestida)embestida.Begin();else latch.Play();
        }

        public bool BraceCorner(float speed,float angle)
        {
            Vector3 d=cornerHand.position-transform.position;d.y=0;
            if(!cornerBraced&&speed>1.2f&&angle>35&&d.magnitude<1.0f)
            {cornerBraced=true;brakeAt=Time.time;cornerContact.Play();performance.ResetFeet();}
            return Time.time-brakeAt<.24f;
        }

        // Called by the existing actor owner. The controller, leaf collider and
        // actual travelled distance govern entry; no root teleport through a door.
        public bool AdvanceBreach(float age, CharacterController controller, AudioSource voice)
        {
            if(embestida)
            {
                float open=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.35f,age));
                doorHinge.localRotation=doorRest*Quaternion.Euler(0,-105*open,0);
                bool done=embestida.Advance(age,controller);
                if(embestida.HasContact&&!burst){burst=true;BreachImpacts++;}
                return done;
            }
            float crack=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.72f,age));
            float slam=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.12f,1.42f,age));
            doorHinge.localRotation=doorRest*Quaternion.Euler(0,-48*crack-57*slam,0);
            if(age>=1.12f&&!burst){burst=true;BreachImpacts++;doorImpact.Play();}
            if(age>=1.20f&&!burstVoice){burstVoice=true;voice.Play();}
            if(age<1.30f)return false;
            Vector3 d=breachExit.position-transform.position;d.y=0;
            if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(d),260*Time.deltaTime);
            controller.Move(Vector3.ClampMagnitude(d,3.8f*Time.deltaTime)+Vector3.down*2*Time.deltaTime);
            return d.magnitude<.16f;
        }

        void LateUpdate()
        {
            if(!ready||session.IsPaused||Time.deltaTime<=0)return;
            if(!encounter.Active)
            {
                // B is confirmed behind the blind service detour. Even an unusual
                // view must conceal the destination before placement is allowed.
                if(!p3Placed&&!p3Retired&&returnRelay.IsSecondLinkAccepted&&!PlaceVisible(p3Stand.position))
                {
                    MoveBody(p3Stand);p3Placed=true;witnessAt=Time.time;visual.gameObject.SetActive(true);
                    p3Practical.intensity=.8f;
                }
                if(p3Placed&&!p3Retired)
                {
                    performance.Present(MachineCoverThreat.Intention.Witness,Time.time-witnessAt,Vector3.zero,0,view.transform.position,null);
                    if(!p3Sound&&PlaceVisible(transform.position))
                    {p3Sound=true;DiscoveryContacts++;p3Contact.Play();}
                    if(campaign.Chapter!=CampaignChapter.Protocol&&Vector3.Distance(player.position,transform.position)>8&&!PlaceVisible(transform.position))
                    {p3Retired=true;visual.gameObject.SetActive(false);p3Practical.intensity=0;}
                }
                return;
            }
            if(!pumpPlaced){pumpPlaced=true;p3Retired=true;p3Practical.intensity=0;visual.gameObject.SetActive(true);}
            // The rehearsal stays safe. It is actual live manual pumping that
            // first draws the hand to the closed side door, not a scare timer.
            if(!threat.Released&&!encounter.Complete)
            {
                if(!primed&&encounter.Machine.PlayerHeld){primed=true;preparationAt=Time.time+.6f;}
                if(preparationAt>=0)
                {
                    float age=Time.time-preparationAt;
                    float push=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.8f,age))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.0f,1.5f,age)));
                    doorHandle.localRotation=handleRest*Quaternion.Euler(0,0,-32*push);
                    doorHinge.localRotation=doorRest*Quaternion.Euler(0,-(embestida?4.5f:1.4f)*push,0);
                    if(embestida)embestida.Prepare(push);
                    if(age>=0&&!pressed){pressed=true;latch.Play();}
                    if(age>=2.25f){doorImpact.PlayOneShot(doorPressure,.36f);preparationAt=-1;}
                }
            }
            if(embestida&&!threat.Released&&!encounter.Complete)
                performance.Present(MachineCoverThreat.Intention.Dormant,Time.time,Vector3.zero,0,breachHand.position,null);
            bool tense=threat.Released&&!encounter.Complete;
            float target=tense?(InPumpContact?.22f:.10f):pressed?.55f:1;
            quiet=Mathf.MoveTowards(quiet,target,Time.deltaTime*1.6f);
            // A real outlet on the same driven pump creates the diversion at the
            // exposed exit. The recording still holds only its existing control.
            float outlet=encounter.MachineRunning?.20f:0;
            returnMotor.volume=Mathf.MoveTowards(returnMotor.volume,outlet,Time.deltaTime*.8f);
            if(returnMotor.volume>0&&!returnMotor.isPlaying)returnMotor.Play();
            if(returnMotor.volume<=0&&returnMotor.isPlaying)returnMotor.Stop();
            if(!embestida)
            {
                if(InPumpContact&&!encounter.Complete){if(!breath.isPlaying)breath.Play();}
                else if(breath.isPlaying)breath.Stop();
            }
        }

        void MoveBody(Transform anchor)
        {
            var controller=GetComponent<CharacterController>();controller.enabled=false;
            transform.SetPositionAndRotation(anchor.position,anchor.rotation);controller.enabled=true;
            performance.ResetPerformance();
        }
        bool PlaceVisible(Vector3 root)
        {
            return PointVisible(root+Vector3.up*1.63f)||PointVisible(root+Vector3.up*1.15f);
        }
        bool PointVisible(Vector3 point)
        {
            Vector3 v=view.WorldToViewportPoint(point);
            if(v.z<=0||v.z>14||v.x<-.08f||v.x>1.08f||v.y<-.08f||v.y>1.08f)return false;
            Vector3 d=point-view.transform.position;
            int n=Physics.RaycastNonAlloc(view.transform.position,d.normalized,hits,d.magnitude-.06f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
            if(n==hits.Length)return false;
            for(int i=0;i<n;i++)
            {
                var c=hits[i].collider;if(c.transform.IsChildOf(transform)||c.transform.IsChildOf(player))continue;
                bool glazing=false;if(p3Glazing!=null)foreach(var glass in p3Glazing)if(c==glass){glazing=true;break;}
                if(!glazing)return false;
            }
            return true;
        }
        void OnDisable(){foreach(var source in new[]{p3Contact,latch,doorImpact,breath,returnMotor,cornerContact})if(source)source.Stop();}
    }
}

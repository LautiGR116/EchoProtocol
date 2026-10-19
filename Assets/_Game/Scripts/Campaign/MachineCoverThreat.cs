using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Audio;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // One present-time listener, on an explicitly authored, physically connected
    // route. It reacts to the live machine and live Player; it never reads a take.
    [RequireComponent(typeof(CharacterController))]
    public sealed class MachineCoverThreat : MonoBehaviour
    {
        [SerializeField] private MachineCoverEncounter encounter;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Transform machineSound, start;
        [SerializeField] private Transform[] navigationPoints;
        [SerializeField] private EchoBodyRig body;
        [SerializeField] private AudioSource cry, movement, strike;
        [SerializeField] private MachineCoverPresence presence;
        [SerializeField] private MachineThreatPerformance performance;
        [SerializeField] private IntruderStaging intruder;
        public IntruderStaging Intruder => intruder;
        [SerializeField] private Transform recognitionPoint;
        [SerializeField] private Transform entryCorner;
        public Transform RecognitionPoint => recognitionPoint;
        [SerializeField] private Camera playerView;
        [SerializeField] private AudioSource recognitionVoice, chaseVoice;
        public enum Intention { Dormant, Entry, Recognition, Charge, Listening, Capture, Resolved, Witness, MachineContact }
        public Intention Phase { get; private set; }
        public bool PerformanceEnabled => performance;
        public bool IsCapturing => Phase == Intention.Capture;
        public float PhaseAge => Time.time-phaseAt;
        public int CaptureContactCount { get; private set; }
        public float LastCaptureSeconds { get; private set; }
        float phaseAt, chargeAt, speedNow, nextVoice;
        Vector3 viewRest;
        Quaternion captureViewRotation;
        bool cornerReached;
        void Change(Intention next){Phase=next;phaseAt=Time.time;}
        [SerializeField] private float pursuitSpeed = 4.25f, listeningSpeed = 1.55f;
        [SerializeField, Min(0)] private float navigationClearance;
        [SerializeField] private float captureDistance = 1.05f;
        [SerializeField] private float initialRushSeconds=2.65f;
        CharacterController controller;
        Vector3 lastPlayer, knownPosition, destination;
        float releasedAt, inspectAt, gait, distanceWalked, lastStep;
        bool alerted, cryPlayed;
        public bool Released { get; private set; }
        public bool AttractedToMachine { get; private set; }
        public bool SeesPlayer { get; private set; }
        public int HeardPlayerCount { get; private set; }
        public int DiversionCount { get; private set; }
        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (!encounter || !session || !player || !machineSound || !start || navigationPoints == null || navigationPoints.Length < 3 || !body || !cry || !movement || !strike)
            { Debug.LogError("Present threat requires authored route, body and audio references.", this); enabled = false; return; }
            // This present-time contact belongs to the failure transition. Its
            // short tail can finish while the retry menu freezes the world.
            strike.ignoreListenerPause = true;
            ResetThreat();
        }
        public void ResetThreat()
        {
            Released = alerted = cryPlayed = false; AttractedToMachine = SeesPlayer = false;
            if (controller) controller.enabled = false;
            transform.SetPositionAndRotation(start.position, start.rotation);
            if (controller) controller.enabled = true;
            lastPlayer = player.transform.position; knownPosition = lastPlayer;
            cry.Stop(); movement.Stop(); strike.Stop(); inspectAt = 0;
            gait = distanceWalked = lastStep = 0;
            Change(Intention.Dormant); speedNow=0;
            cornerReached=false;
            player.MovementSuppressed=player.LookSuppressed=false;
            if(playerView&&performance){if(viewRest==Vector3.zero)viewRest=playerView.transform.localPosition;playerView.transform.localPosition=viewRest;}
            if(recognitionVoice)recognitionVoice.Stop();if(chaseVoice)chaseVoice.Stop();
            body.SetPresentIntent(-1);
            body.Apply(new RecordedBodyPose(0, 0, 16, Vector2.zero));
            if(presence)presence.ResetPresence();
            if(performance)performance.ResetPerformance();
            if(intruder)intruder.ResetPump();
        }
        public void PrimePresence(){if(presence&&!intruder)presence.Prime();}
        public void Release()
        {
            Released = true; releasedAt = Time.time; inspectAt = 0;
            knownPosition = player.transform.position; lastPlayer = knownPosition; alerted = true;
            if(performance){Change(Intention.Entry);performance.ResetPerformance();}
            if(presence)presence.Release();
            if(intruder)intruder.BeginBreach();
        }
        public void Settle() { Released = false; cry.Stop(); movement.Stop();if(chaseVoice)chaseVoice.Stop();Change(Intention.Resolved);if(performance){if(!intruder)performance.ResetPerformance();}else{body.SetPresentIntent(-1);body.Apply(new RecordedBodyPose(gait,0,12,Vector2.zero));} }
        bool Clear(Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            foreach (var hit in Physics.RaycastAll(a, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(player.transform)) return false;
            return true;
        }
        bool WalkClear(Vector3 a, Vector3 b)
        {
            if(navigationClearance<=0)return Clear(a,b);
            Vector3 d=b-a;
            foreach(var hit in Physics.SphereCastAll(a,navigationClearance,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(transform)&&!hit.transform.IsChildOf(player.transform))return false;
            return true;
        }
        int Nearest(Vector3 position)
        {
            int best = 0; float d = float.PositiveInfinity;
            for (int i = 0; i < navigationPoints.Length; i++)
            {
                float candidate = (navigationPoints[i].position - position).sqrMagnitude;
                if (candidate < d) { d = candidate; best = i; }
            }
            return best;
        }
        Vector3 RouteTowards(Vector3 target)
        {
            Vector3 eye = transform.position + Vector3.up;
            if (WalkClear(eye, target + Vector3.up)) return target;
            int n = navigationPoints.Length, from = Nearest(transform.position), to = Nearest(target);
            var dist = new float[n]; var prev = new int[n]; var visited = new bool[n];
            for (int i = 0; i < n; i++) { dist[i] = float.PositiveInfinity; prev[i] = -1; }
            dist[from] = 0;
            for (int k = 0; k < n; k++)
            {
                int u = -1; float best = float.PositiveInfinity;
                for (int i = 0; i < n; i++) if (!visited[i] && dist[i] < best) { best = dist[i]; u = i; }
                if (u < 0 || u == to) break;
                visited[u] = true;
                for (int v = 0; v < n; v++)
                {
                    if (v == u || visited[v] || !WalkClear(navigationPoints[u].position + Vector3.up, navigationPoints[v].position + Vector3.up)) continue;
                    float next = dist[u] + Vector3.Distance(navigationPoints[u].position, navigationPoints[v].position);
                    if (next < dist[v]) { dist[v] = next; prev[v] = u; }
                }
            }
            if (float.IsInfinity(dist[to])) return navigationPoints[from].position;
            int step = to;
            while (prev[step] >= 0 && prev[step] != from) step = prev[step];
            if (!WalkClear(eye, navigationPoints[step].position + Vector3.up)) return navigationPoints[from].position;
            return navigationPoints[step].position;
        }
        void Update()
        {
            if (!Released || encounter.Failed || encounter.Complete || session.IsPaused || Time.deltaTime <= 0) return;
            if(performance){UpdateDirected();return;}
            float since = Time.time - releasedAt;
            body.SetPresentIntent(since);
            if (!cryPlayed && since >= .7f) { cryPlayed = true; cry.Play(); }
            if (since < 1.15f) { if(body.CharacterMotion)body.Apply(new RecordedBodyPose(0,since*1.2f,16,Vector2.zero)); return; }
            Vector3 present = player.transform.position;
            bool moving = Vector3.Distance(present, lastPlayer) > .008f;
            lastPlayer = present;
            if (Time.time >= inspectAt)
            {
                inspectAt = Time.time + .12f;
                Vector3 offset = present - transform.position;
                SeesPlayer = offset.magnitude < 11 && Vector3.Dot(transform.forward, offset.normalized) > .55f
                    && Clear(transform.position + Vector3.up * 1.55f, present + Vector3.up * 1.25f);
                bool heard = !encounter.Covered && moving && offset.magnitude < 18;
                // The real exit latch first draws a short, authored rush toward
                // its operator. After that attack, the live pump can pull the
                // listener away if the Player has broken line of sight.
                bool lure = since >= initialRushSeconds && encounter.Covered && !SeesPlayer;
                if (lure && !AttractedToMachine) DiversionCount++;
                AttractedToMachine = lure;
                if (SeesPlayer || heard) { knownPosition = present; alerted = true; if (heard) HeardPlayerCount++; }
                destination = RouteTowards(lure ? machineSound.position : knownPosition);
            }
            Vector3 direction = destination - transform.position; direction.y = 0;
            float speed = AttractedToMachine ? listeningSpeed : alerted ? pursuitSpeed : 2.5f;
            if(presence)speed*=Mathf.SmoothStep(.45f,1,Mathf.InverseLerp(1.15f,1.85f,since));
            Vector3 before = transform.position;
            if (direction.magnitude > .24f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 250 * Time.deltaTime);
                controller.Move(direction.normalized * Mathf.Min(direction.magnitude, speed * Time.deltaTime) + Vector3.down * 2 * Time.deltaTime);
            }
            Vector3 actual = (transform.position - before) / Time.deltaTime;
            distanceWalked += (transform.position - before).magnitude;
            gait += actual.magnitude * (body.CharacterMotion ? Mathf.PI*2/RecordedCharacterMotion.StrideForSpeed(actual.magnitude) : 1.9f) * Time.deltaTime;
            body.Apply(new RecordedBodyPose(gait, since * 1.2f, SeesPlayer ? -5 : 12,
                new Vector2(Vector3.Dot(actual, transform.right), Vector3.Dot(actual, transform.forward))));
            float stepLength = body.CharacterMotion ? RecordedCharacterMotion.StrideForSpeed(actual.magnitude) * .5f : .72f;
            if (distanceWalked - lastStep > stepLength) { lastStep = distanceWalked;if(presence)presence.Step(movement);else movement.Play(); }
            if (Vector3.Distance(transform.position, present) < captureDistance && Clear(transform.position + Vector3.up, present + Vector3.up))
            { strike.Play();if(presence)presence.Caught(); encounter.CatchPlayer(); }
        }

        void UpdateDirected()
        {
            float dt=Time.deltaTime, age=Time.time-releasedAt;
            Vector3 present=player.transform.position, before=transform.position;
            float oldYaw=transform.eulerAngles.y;
            if(Phase==Intention.Capture)
            {
                float u=Mathf.Clamp01(PhaseAge/.95f);
                performance.Present(Phase,PhaseAge,Vector3.zero,0,present+Vector3.up*1.55f,cry);
                Quaternion face=Quaternion.LookRotation(performance.Face.position-playerView.transform.position);
                playerView.transform.rotation=Quaternion.Slerp(captureViewRotation,face,Mathf.SmoothStep(0,1,Mathf.Clamp01(u*3)))*Quaternion.Euler(0,0,Mathf.Sin(u*Mathf.PI)*-4);
                playerView.transform.localPosition=viewRest+new Vector3(.025f*Mathf.Sin(u*Mathf.PI*2),-.065f*Mathf.Sin(u*Mathf.PI),-.04f*Mathf.Sin(u*Mathf.PI));
                if(u>=1){LastCaptureSeconds=PhaseAge;encounter.CatchPlayer();}
                return;
            }
            if(Phase==Intention.Entry)
            {
                if(intruder)
                {
                    if(intruder.AdvanceBreach(PhaseAge,controller,cry))
                    {
                        if(intruder.Embestida){Change(Intention.Charge);chargeAt=Time.time;nextVoice=Time.time+1.6f;inspectAt=0;speedNow=.65f;}
                        else {Change(Intention.Recognition);speedNow=0;performance.ResetFeet();}
                        cryPlayed=true;
                    }
                }
                else
                {
                Vector3 d=(cornerReached?recognitionPoint:entryCorner).position-transform.position;d.y=0;
                float angle=Vector3.Angle(transform.forward,d);
                if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(d),300*dt);
                float entrySpeed=(cornerReached?1.6f:2.4f)*Mathf.Lerp(.28f,1,Mathf.InverseLerp(90,15,angle));
                controller.Move(Vector3.ClampMagnitude(d,entrySpeed*dt)+Vector3.down*2*dt);
                if(d.magnitude<.12f){if(!cornerReached)cornerReached=true;else{Change(Intention.Recognition);recognitionVoice.Play();speedNow=0;}}
                }
            }
            else if(Phase==Intention.Recognition)
            {
                Vector3 d=present-transform.position;d.y=0;
                transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(d),220*dt);
                if(!cryPlayed&&PhaseAge>=.40f){cryPlayed=true;recognitionVoice.Stop();cry.Play();}
                if(PhaseAge>=(intruder?.85f:1.15f)){Change(Intention.Charge);chargeAt=Time.time;nextVoice=Time.time+1.8f;inspectAt=0;}
            }
            else
            {
                bool moving=Vector3.Distance(present,lastPlayer)>.008f;lastPlayer=present;
                if(Time.time>=inspectAt)
                {
                    inspectAt=Time.time+.10f;Vector3 d=present-transform.position;
                    SeesPlayer=d.magnitude<13&&Vector3.Dot(transform.forward,d.normalized)>.35f&&Clear(transform.position+Vector3.up*1.55f,present+Vector3.up*1.25f);
                    bool heard=!encounter.Covered&&moving&&d.magnitude<18;
                    bool lure=Time.time-chargeAt>=initialRushSeconds&&encounter.Covered&&!SeesPlayer;
                    if(lure&&!AttractedToMachine){DiversionCount++;chaseVoice.Stop();recognitionVoice.Play();Change(Intention.Listening);}
                    if(!lure&&AttractedToMachine){Change(Intention.Charge);nextVoice=Time.time+.45f;}
                    AttractedToMachine=lure;
                    if(SeesPlayer||heard){knownPosition=present;if(heard)HeardPlayerCount++;}
                    destination=RouteTowards(lure?(intruder?intruder.PumpStand.position:machineSound.position):knownPosition);
                }
                Vector3 direction=destination-transform.position;direction.y=0;
                float angle=direction.sqrMagnitude>.001f?Vector3.Angle(transform.forward,direction):0;
                float targetSpeed=AttractedToMachine?listeningSpeed:pursuitSpeed;
                if(!AttractedToMachine&&intruder&&intruder.Embestida)targetSpeed*=intruder.Embestida.PursuitMultiplier;
                targetSpeed*=Mathf.Lerp(.18f,1,Mathf.InverseLerp(100,15,angle));
                targetSpeed*=Mathf.Clamp01(direction.magnitude/.85f);
                bool bracing=intruder&&intruder.BraceCorner(speedNow,angle);
                if(bracing)targetSpeed=0;
                speedNow=Mathf.MoveTowards(speedNow,targetSpeed,dt*(AttractedToMachine?3:6.5f));
                if(direction.magnitude>(intruder&&AttractedToMachine?.06f:.17f)&&!bracing)
                {
                    transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),310*dt);
                    Vector3 travel=Vector3.RotateTowards(transform.forward,direction.normalized,.20f,0);
                    controller.Move(travel*Mathf.Min(direction.magnitude,speedNow*dt)+Vector3.down*2*dt);
                }
                if(intruder&&AttractedToMachine&&Vector3.Distance(transform.position,intruder.PumpStand.position)<.08f)
                {
                    if(Phase!=Intention.MachineContact){Change(Intention.MachineContact);performance.ResetFeet();}
                    speedNow=0;transform.rotation=Quaternion.RotateTowards(transform.rotation,intruder.PumpStand.rotation,180*dt);
                }
                if(!AttractedToMachine&&Time.time>=nextVoice){chaseVoice.Play();nextVoice=Time.time+(intruder?3.8f:2.1f);}
                if(!(intruder&&intruder.Embestida&&intruder.Embestida.ProtectReaction)&&Vector3.Distance(transform.position,present)<captureDistance&&Clear(transform.position+Vector3.up,present+Vector3.up))
                {
                    Change(Intention.Capture);CaptureContactCount++;strike.Play();cry.Stop();chaseVoice.Stop();recognitionVoice.Stop();if(presence)presence.Caught();
                    player.MovementSuppressed=player.LookSuppressed=true;captureViewRotation=playerView.transform.rotation;
                    performance.ResetFeet();
                }
            }
            Vector3 actual=(transform.position-before)/dt;
            performance.Present(Phase,PhaseAge,actual,Mathf.DeltaAngle(oldYaw,transform.eulerAngles.y)/dt,
                AttractedToMachine?(intruder?intruder.PumpHand.position:machineSound.position):present+Vector3.up*1.55f,
                cry.isPlaying?cry:chaseVoice.isPlaying?chaseVoice:recognitionVoice);
            // Contacts come from the same planted-foot transition as the pose.
            int contacts=performance.ConsumeContacts();for(int i=0;i<contacts;i++){if(presence)presence.Step(movement);else movement.Play();}
        }
    }
}

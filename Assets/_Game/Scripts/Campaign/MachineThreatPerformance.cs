using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // Only the present pump actor uses this performance. No recording, replay,
    // Echo controller or captured joint data is read or changed here.
    [DisallowMultipleComponent]
    public sealed class MachineThreatPerformance : MonoBehaviour
    {
        [SerializeField] RecordedCharacterMotion anatomy;
        [SerializeField] Transform hips, spine, chest, neck, head, jaw;
        [SerializeField] Transform leftThigh, leftShin, leftFoot, rightThigh, rightShin, rightFoot;
        [SerializeField] Transform leftArm, leftForearm, leftHand, rightArm, rightForearm, rightHand;
        [SerializeField] IntruderStaging intruder;
        [SerializeField] Transform[] brows, mouthCorners;
        Transform root;
        Transform[] joints;
        Vector3[] positions;
        Quaternion[] rotations;
        Vector3 rootPosition;
        Quaternion rootRotation, leftOrientation, rightOrientation;
        Vector3 leftRest, rightRest, travelDirection;
        float lu,ll,ru,rl, la,lb,ra,rb,cycle,runBlend;
        float previousSpeed, accelerationWeight, brakeWeight, gazeYaw, gazePitch, machineHandWeight;
        bool initialized;
        struct FootSupport
        {
            public Vector3 plant, liftOff, lastTarget, recoveryTarget;
            public Quaternion plantRotation, liftRotation, lastRotation;
            public bool stance, recovering;
            public float recoveryAge, recoveryDuration;
        }
        FootSupport leftSupport, rightSupport;
        int contacts;
        float[] voiceEnvelope;
        AudioClip envelopeClip;
        public Transform Face => head;
        public float MachinePalmDistance => leftHand&&intruder?Vector3.Distance(leftHand.position,intruder.PumpHand.position):float.PositiveInfinity;
        public int PlantedContacts { get; private set; }
        public int ReachCount { get; private set; }
        public float MaxPlantDrift { get; private set; }

        void Initialize()
        {
            if(initialized)return;
            if(!anatomy||!hips||!spine||!chest||!neck||!head||!jaw||!leftThigh||!leftShin||!leftFoot||!rightThigh||!rightShin||!rightFoot||!leftArm||!leftForearm||!leftHand||!rightArm||!rightForearm||!rightHand)
            {Debug.LogError("Pump performance needs explicit anatomy.",this);enabled=false;return;}
            anatomy.SetPresentIntent(-1);anatomy.Apply(new RecordedBodyPose(0,0,0,Vector2.zero));
            root=anatomy.ModelRoot;joints=anatomy.Joints;
            positions=new Vector3[joints.Length];rotations=new Quaternion[joints.Length];
            for(int i=0;i<joints.Length;i++){positions[i]=joints[i].localPosition;rotations[i]=joints[i].localRotation;}
            rootPosition=root.localPosition;rootRotation=root.localRotation;
            leftRest=root.InverseTransformPoint(leftFoot.position);rightRest=root.InverseTransformPoint(rightFoot.position);
            leftOrientation=Quaternion.Inverse(root.rotation)*leftFoot.rotation;rightOrientation=Quaternion.Inverse(root.rotation)*rightFoot.rotation;
            lu=Vector3.Distance(leftThigh.position,leftShin.position);ll=Vector3.Distance(leftShin.position,leftFoot.position);
            ru=Vector3.Distance(rightThigh.position,rightShin.position);rl=Vector3.Distance(rightShin.position,rightFoot.position);
            la=Vector3.Distance(leftArm.position,leftForearm.position);lb=Vector3.Distance(leftForearm.position,leftHand.position);
            ra=Vector3.Distance(rightArm.position,rightForearm.position);rb=Vector3.Distance(rightForearm.position,rightHand.position);
            initialized=true;ResetFeet();
        }
        public void ResetPerformance()
        {
            Initialize();if(!initialized)return;Restore();
            cycle=runBlend=previousSpeed=accelerationWeight=brakeWeight=gazeYaw=gazePitch=machineHandWeight=0;
            contacts=0;travelDirection=root.forward;ResetFeet();
        }
        void Restore(){for(int i=0;i<joints.Length;i++){joints[i].localPosition=positions[i];joints[i].localRotation=rotations[i];}root.localPosition=rootPosition;root.localRotation=rootRotation;}
        public void ResetFeet()
        {
            if(!initialized)return;
            leftSupport=NewSupport(leftRest,leftOrientation);
            rightSupport=NewSupport(rightRest,rightOrientation);
        }
        FootSupport NewSupport(Vector3 rest,Quaternion orientation)
        {
            Vector3 point=root.TransformPoint(rest);Quaternion rotation=root.rotation*orientation;
            return new FootSupport{plant=point,liftOff=point,lastTarget=point,plantRotation=rotation,liftRotation=rotation,lastRotation=rotation,stance=true};
        }
        public int ConsumeContacts(){int n=contacts;contacts=0;return n;}
        public void Present(MachineCoverThreat.Intention phase,float age,Vector3 velocity,float turnRate,Vector3 attention,AudioSource voice,float pursuitSpeed=3.6f)
        {
            Initialize();if(!initialized)return;Restore();
            float speed=new Vector2(velocity.x,velocity.z).magnitude,dt=Time.deltaTime;
            bool searching=phase==MachineCoverThreat.Intention.Search;
            bool reacquiring=phase==MachineCoverThreat.Intention.Reacquire;
            bool charging=phase==MachineCoverThreat.Intention.Charge;
            bool listening=phase==MachineCoverThreat.Intention.Listening;
            // Urgency follows the speed configured by this encounter. A 2.1 m/s
            // chase must not retain the old 3.6 m/s walking-to-running threshold.
            // Foot timing remains distance-driven, including the faster breach.
            float locomotion=Mathf.InverseLerp(Mathf.Max(.55f,pursuitSpeed*.38f),Mathf.Max(1,pursuitSpeed),speed);
            if(searching||listening)locomotion*=.42f;
            runBlend=Mathf.MoveTowards(runBlend,locomotion,dt*4);
            float moving=Mathf.Clamp01(speed/.25f);
            float stride=Mathf.Lerp(.96f,2.05f,Mathf.InverseLerp(.15f,4.5f,speed));
            cycle=Mathf.Repeat(cycle+speed*dt/stride,1);
            if(speed>.08f)travelDirection=Vector3.ProjectOnPlane(velocity,Vector3.up).normalized;
            else if(travelDirection.sqrMagnitude<.01f)travelDirection=root.forward;
            float acceleration=dt>.00001f?(speed-previousSpeed)/dt:0;previousSpeed=speed;
            accelerationWeight=Mathf.MoveTowards(accelerationWeight,Mathf.Clamp01(acceleration/5),dt*5);
            brakeWeight=Mathf.MoveTowards(brakeWeight,Mathf.Clamp01(-acceleration/4.5f),dt*(acceleration<0?9:3));
            bool recognition=phase==MachineCoverThreat.Intention.Recognition;
            bool capture=phase==MachineCoverThreat.Intention.Capture;
            float launch=charging?1-Mathf.Clamp01(age/.45f):reacquiring?Mathf.SmoothStep(0,1,age/.30f)*.65f:0;
            float brace=recognition?(intruder?.70f+.30f*Mathf.SmoothStep(0,1,age/.85f):Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1.3f,age))):launch;
            float recoil=intruder&&recognition?1-Mathf.SmoothStep(0,1,age/.85f):0;
            float breath=Mathf.Sin(age*(searching?2.2f:5.8f))*(searching?.32f:.9f);
            bool witness=phase==MachineCoverThreat.Intention.Witness;
            bool onMachine=phase==MachineCoverThreat.Intention.MachineContact;
            float searchWeight=searching?Mathf.SmoothStep(0,1,age/.26f):0;
            float press=onMachine?Mathf.Sin(Mathf.PI*Mathf.Clamp01(Mathf.InverseLerp(.25f,.55f,age))):0;
            machineHandWeight=Mathf.MoveTowards(machineHandWeight,onMachine?1:0,dt*(onMachine?4.5f:6));
            var rush=intruder?intruder.Embestida:null;
            float squeezed=intruder&&!rush&&phase==MachineCoverThreat.Intention.Entry?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.12f,1.52f,age)):0;
            float cornerBrace=intruder?intruder.BrakeWeight:0;
            float braking=Mathf.Max(cornerBrace,brakeWeight);
            hips.position-=root.up*(runBlend*(.070f+.020f*Mathf.Cos(cycle*Mathf.PI*4))+.045f*brace+braking*.035f);
            hips.position+=root.right*(-.055f*squeezed+searchWeight*.018f)-root.up*(.16f*squeezed+(witness?.045f:0)+machineHandWeight*.055f);
            hips.position+=root.forward*(accelerationWeight*.025f-braking*.027f+press*.022f);
            Rotate(spine,root.right,runBlend*12+brace*8+breath+accelerationWeight*5);
            Rotate(spine,root.right,squeezed*25+(witness?8:0)+machineHandWeight*16+press*5-searchWeight*4);
            Rotate(spine,root.right,-braking*12+recoil*9);
            Rotate(chest,root.forward,-recoil*5);
            Rotate(chest,root.forward,squeezed*-18+(witness?-5:0));
            Rotate(chest,root.up,Mathf.Sin(cycle*Mathf.PI*2)*runBlend*6-Mathf.Clamp(turnRate*.028f,-10,10));
            Rotate(chest,root.forward,-Mathf.Clamp(turnRate*.03f,-8,8)*moving-press*3);
            Vector3 gaze=root.InverseTransformDirection(attention-head.position);
            float yaw=Mathf.Clamp(Mathf.Atan2(gaze.x,gaze.z)*Mathf.Rad2Deg,-48,48);
            float pitch=Mathf.Clamp(-Mathf.Atan2(gaze.y,new Vector2(gaze.x,gaze.z).magnitude)*Mathf.Rad2Deg,-12,18);
            // Eyes/head acquire the supplied evidence first; the shoulders catch
            // up through the actual root turn. Hidden Player position is never read.
            gazeYaw=Mathf.MoveTowardsAngle(gazeYaw,yaw,dt*(reacquiring?300:searching?72:170));
            gazePitch=Mathf.MoveTowardsAngle(gazePitch,pitch,dt*95);
            Rotate(neck,root.up,gazeYaw*.36f);Rotate(head,root.up,gazeYaw*.64f);
            Rotate(head,root.right,-runBlend*7-brace*6+gazePitch);
            Rotate(head,root.right,squeezed*13+(witness?9:0)+(intruder&&recognition?10:0));
            Rotate(head,root.forward,squeezed*16+(witness?5:0));
            Rotate(head,root.forward,(listening?7:searchWeight*8)+machineHandWeight*7);
            float articulation=Voice(voice);
            Rotate(jaw,root.right,articulation*17);Rotate(chest,root.right,articulation*-3);
            if(intruder)
            {
                Rotate(jaw,root.right,squeezed*5+(witness?2:0)+(recognition?9:0)+(onMachine?3+Mathf.Sin(age*2.5f):0));
                // Only this present actor uses the rig's face joints. Quiet
                // corners/brows keep the photographed neutral smile out of the attack.
                float tension=witness?.55f:1;
                if(mouthCorners!=null)foreach(var corner in mouthCorners)if(corner)corner.position-=root.up*(.008f*tension);
                if(brows!=null)foreach(var brow in brows)if(brow)
                {brow.position-=root.up*(.004f*tension);brow.position+=Vector3.Project(head.position-brow.position,root.right)*(.12f*tension);}
            }
            // Bent elbows and opposing shoulder drives replace sped-up walking.
            float swing=Mathf.Sin(cycle*Mathf.PI*2)*Mathf.Lerp(13,34,runBlend)*moving;
            Rotate(leftArm,root.right,swing-runBlend*8-brace*20);Rotate(rightArm,root.right,-swing-runBlend*8-brace*12);
            Rotate(leftForearm,root.right,-runBlend*54-brace*26-searchWeight*14);Rotate(rightForearm,root.right,-runBlend*60-brace*32-searchWeight*25);
            Rotate(leftArm,root.forward,runBlend*8+brace*8);Rotate(rightArm,root.forward,-runBlend*8-brace*8);
            Rotate(leftArm,root.right,braking*12-searchWeight*7);Rotate(rightArm,root.right,braking*9-searchWeight*13);
            if(rush)rush.Pose(root,hips,spine,chest,neck,head,jaw,leftArm,rightArm,leftForearm,rightForearm,phase,age);
            if(rush&&phase==MachineCoverThreat.Intention.Entry)
                Solve(leftArm,leftForearm,leftHand,root.TransformPoint(new Vector3(-.16f,1.15f,.34f)),leftArm.position-root.right*.24f-Vector3.up*.18f,la,lb);
            if(rush&&(rush.HandWeight>.01f||rush.Preparation>.01f)&&!witness&&!onMachine)
            {
                bool impact=phase==MachineCoverThreat.Intention.Entry||rush.HasContact;
                float blend=impact?rush.HandWeight:rush.Preparation;
                Transform anchor=impact?rush.Hand:intruder.BreachHand;
                Solve(rightArm,rightForearm,rightHand,Vector3.Lerp(rightHand.position,anchor.position,blend),rightArm.position+root.right*.25f-Vector3.up*.18f,ra,rb);
                rightHand.rotation=Quaternion.Slerp(rightHand.rotation,anchor.rotation,blend);
            }
            else if(intruder&&(squeezed>.01f||witness))
            {
                Transform anchor=witness?intruder.WitnessHand:intruder.BreachHand;
                bool right = witness || squeezed > .01f;
                Transform arm=right?rightArm:leftArm,fore=right?rightForearm:leftForearm,hand=right?rightHand:leftHand;
                float blend=squeezed>0?squeezed:1;
                Solve(arm,fore,hand,Vector3.Lerp(hand.position,anchor.position,blend),arm.position-Vector3.up*.38f+root.forward*.1f,right?ra:la,right?rb:lb);
                hand.rotation=Quaternion.Slerp(hand.rotation,anchor.rotation,blend);
            }
            else if(intruder&&machineHandWeight>.01f)
            {
                Transform anchor=intruder.PumpHand;
                // Palm reaches the casing before the .38 s pressure/contact beat,
                // remains there while listening, then releases as pursuit resumes.
                float reach=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(la+lb-.04f,la+lb+.23f,Vector3.Distance(leftArm.position,anchor.position)));
                float blend=machineHandWeight*reach;
                Solve(leftArm,leftForearm,leftHand,Vector3.Lerp(leftHand.position,anchor.position,blend),leftArm.position-root.right*.27f-Vector3.up*.20f,la,lb);
                leftHand.rotation=Quaternion.Slerp(leftHand.rotation,anchor.rotation,blend);
                Rotate(rightForearm,root.right,-machineHandWeight*18);
            }
            else if(cornerBrace>.01f)
            {
                Transform anchor=intruder.CornerHand;
                float reach=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(la+lb,la+lb+.20f,Vector3.Distance(leftArm.position,anchor.position)));
                float blend=cornerBrace*reach;
                Solve(leftArm,leftForearm,leftHand,Vector3.Lerp(leftHand.position,anchor.position,blend),leftArm.position-root.right*.25f-Vector3.up*.25f,la,lb);
                leftHand.rotation=Quaternion.Slerp(leftHand.rotation,anchor.rotation,blend);
            }
            if(capture)
            {
                Rotate(jaw,root.right,6);Rotate(spine,root.right,5);
                float reach=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.24f));
                Vector3 toward=(attention-transform.position);toward.y=0;toward.Normalize();Vector3 side=Vector3.Cross(Vector3.up,toward);
                Vector3 contact=attention-Vector3.up*.39f-toward*.18f;
                Solve(leftArm,leftForearm,leftHand,Vector3.Lerp(leftHand.position,contact-side*.20f,reach),leftArm.position-side*.40f+Vector3.up*.1f,la,lb);
                Solve(rightArm,rightForearm,rightHand,Vector3.Lerp(rightHand.position,contact+side*.20f,reach),rightArm.position+side*.40f+Vector3.up*.1f,ra,rb);
                foreach(var joint in joints)if(joint.name.Contains("_02_")&&(joint.name.Contains("index")||joint.name.Contains("middle")||joint.name.Contains("ring")||joint.name.Contains("pinky")))joint.localRotation*=Quaternion.Euler(0,0,reach*30);
                ReachCount++;
            }
            if(rush)foreach(var joint in joints)
                if(joint.name.Contains("_02_")&&(joint.name.Contains("index")||joint.name.Contains("middle")||joint.name.Contains("ring")||joint.name.Contains("pinky")))
                    joint.localRotation*=Quaternion.Euler(0,0,joint.name.EndsWith("_l")?24:-15);
            float stance=Mathf.Lerp(.64f,.52f,runBlend);
            Leg(true,Mathf.Repeat(cycle+.02f,1),stance,stride,moving,speed,turnRate);
            Leg(false,Mathf.Repeat(cycle+.52f,1),stance,stride,moving,speed,turnRate);
        }
        void Leg(bool left,float p,float stance,float stride,float moving,float speed,float turnRate)
        {
            Transform thigh=left?leftThigh:rightThigh,shin=left?leftShin:rightShin,foot=left?leftFoot:rightFoot;
            Vector3 rest=left?leftRest:rightRest;
            FootSupport support=left?leftSupport:rightSupport,other=left?rightSupport:leftSupport;
            Quaternion desiredRotation=root.rotation*(left?leftOrientation:rightOrientation);
            Vector3 underHip=root.TransformPoint(rest);
            Vector3 landing=underHip+travelDirection*stride*stance*.5f*moving;
            float limit=(left?lu+ll:ru+rl)-.014f;
            bool stopped=speed<.08f;
            // A stopped swing settles through a short arc; planted soles keep
            // their world orientation instead of revolving with every root turn.
            if(!support.recovering&&stopped&&!support.stance)
                BeginRecovery(ref support,underHip,.18f);
            if(!support.recovering&&support.stance&&other.stance&&!other.recovering)
            {
                bool stretched=Vector3.Distance(thigh.position,support.plant)>limit;
                bool needsSettle=stopped&&(Vector3.Distance(support.plant,underHip)>.14f||Quaternion.Angle(support.plantRotation,desiredRotation)>28);
                if(stretched||needsSettle)BeginRecovery(ref support,stopped?underHip:landing,stopped?.23f:.15f);
            }
            Vector3 target;Quaternion orientation;
            if(support.recovering)
            {
                support.recoveryAge+=Time.deltaTime;
                float u=Mathf.Clamp01(support.recoveryAge/support.recoveryDuration);
                target=Vector3.Lerp(support.liftOff,support.recoveryTarget,Mathf.SmoothStep(0,1,u))+Vector3.up*Mathf.Sin(u*Mathf.PI)*.075f;
                orientation=Quaternion.Slerp(support.liftRotation,desiredRotation,Mathf.SmoothStep(0,1,u));
                if(u>=1)Plant(ref support,target,orientation);
            }
            else if(stopped||p<stance)
            {
                if(!support.stance)Plant(ref support,landing,desiredRotation);
                target=support.plant;orientation=support.plantRotation;
            }
            else
            {
                if(support.stance){support.liftOff=support.plant;support.liftRotation=support.plantRotation;support.stance=false;}
                float u=Mathf.Clamp01((p-stance)/(1-stance));
                target=Vector3.Lerp(support.liftOff,landing,Mathf.SmoothStep(0,1,u))+Vector3.up*Mathf.Sin(u*Mathf.PI)*Mathf.Lerp(.10f,.17f,runBlend);
                orientation=Quaternion.Slerp(support.liftRotation,desiredRotation,Mathf.SmoothStep(0,1,u));
            }
            Solve(thigh,shin,foot,target,root.TransformPoint(rest+new Vector3(0,.45f,.65f)),left?lu:ru,left?ll:rl);
            foot.rotation=orientation;
            if(support.stance)MaxPlantDrift=Mathf.Max(MaxPlantDrift,Vector3.Distance(foot.position,target));
            support.lastTarget=target;support.lastRotation=orientation;
            if(left)leftSupport=support;else rightSupport=support;
        }
        static void BeginRecovery(ref FootSupport support,Vector3 target,float duration)
        {
            support.recovering=true;support.recoveryAge=0;support.recoveryDuration=duration;
            support.liftOff=support.lastTarget;support.liftRotation=support.lastRotation;support.recoveryTarget=target;support.stance=false;
        }
        void Plant(ref FootSupport support,Vector3 target,Quaternion rotation)
        {
            support.plant=target;support.plantRotation=rotation;support.stance=true;support.recovering=false;
            contacts++;PlantedContacts++;
        }
        float Voice(AudioSource source)
        {
            if(!source||!source.isPlaying||!source.clip)return 0;
            if(envelopeClip!=source.clip)
            {
                envelopeClip=source.clip;var samples=new float[envelopeClip.samples*envelopeClip.channels];envelopeClip.GetData(samples,0);
                int window=Mathf.Max(1,envelopeClip.frequency/40)*envelopeClip.channels;voiceEnvelope=new float[(samples.Length+window-1)/window];
                for(int b=0;b<voiceEnvelope.Length;b++){float sum=0;int end=Mathf.Min(samples.Length,(b+1)*window);for(int s=b*window;s<end;s++)sum+=samples[s]*samples[s];voiceEnvelope[b]=Mathf.Clamp01(Mathf.Sqrt(sum/Mathf.Max(1,end-b*window))*6);}
            }
            return voiceEnvelope[Mathf.Clamp(Mathf.FloorToInt(source.time*40),0,voiceEnvelope.Length-1)];
        }
        static void Rotate(Transform joint,Vector3 axis,float degrees)=>joint.rotation=Quaternion.AngleAxis(degrees,axis)*joint.rotation;
        static void Solve(Transform upperBone,Transform lowerBone,Transform end,Vector3 target,Vector3 hint,float upper,float lower)
        {
            Vector3 origin=upperBone.position,offset=target-origin;float distance=Mathf.Clamp(offset.magnitude,Mathf.Abs(upper-lower)+.001f,upper+lower-.001f);
            Vector3 direction=offset.sqrMagnitude>.00001f?offset.normalized:Vector3.down,bend=Vector3.ProjectOnPlane(hint-origin,direction).normalized;
            if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(Vector3.forward,direction).normalized;
            float projection=(upper*upper+distance*distance-lower*lower)/(2*distance);
            Vector3 middle=origin+direction*projection+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-projection*projection));
            upperBone.rotation=Quaternion.FromToRotation(lowerBone.position-origin,middle-origin)*upperBone.rotation;
            lowerBone.rotation=Quaternion.FromToRotation(end.position-lowerBone.position,target-lowerBone.position)*lowerBone.rotation;
        }
    }
}

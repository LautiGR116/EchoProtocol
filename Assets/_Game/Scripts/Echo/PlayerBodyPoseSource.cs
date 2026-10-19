using EchoProtocol.Player;
using UnityEngine;

namespace EchoProtocol.Echo
{
    [DefaultExecutionOrder(-50), DisallowMultipleComponent]
    public sealed class PlayerBodyPoseSource : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private FirstPersonController controller;
        [SerializeField] private bool adaptiveStride;
        [SerializeField] private RecordedMachineControl recordedMachine;
        [Header("Captured support — opt in per scene")]
        [SerializeField] private bool captureFootSupport;
        [SerializeField] private Vector3 leftNeutral = new Vector3(-.0936f,.1269f,-.0177f);
        [SerializeField] private Vector3 rightNeutral = new Vector3(.0942f,.1310f,-.0227f);
        private Vector3 previous;
        private float gait, breath, turnPhase, turnRate, previousYaw;
        private Vector2 velocity;
        private float locomotionBlend;
        private Vector3 hipOffset;
        private Vector2 lean;
        private Vector3 previousWorldVelocity;
        private FootStep leftStep, rightStep;
        private RecordedLocomotion locomotion;
        struct FootStep
        {
            public Vector3 position, start;
            public float yaw, startYaw, progress, duration, roll;
            public bool swinging, wasSwingPhase;
        }
        public RecordedBodyPose Current { get; private set; }
        void Awake()
        {
            if(!view||!controller){Debug.LogError("Body pose source needs the actual Player camera and controller.",this);enabled=false;return;}
            controller.PoseReset+=ResetPose; ResetPose();
        }
        void OnDestroy(){if(controller)controller.PoseReset-=ResetPose;}
        void ResetPose()
        {
            previous=transform.position;previousYaw=transform.eulerAngles.y;
            gait=breath=turnPhase=turnRate=locomotionBlend=0;velocity=lean=Vector2.zero;
            hipOffset=previousWorldVelocity=Vector3.zero;
            leftStep=new FootStep{position=transform.TransformPoint(leftNeutral),yaw=previousYaw};
            rightStep=new FootStep{position=transform.TransformPoint(rightNeutral),yaw=previousYaw};
            locomotion=default;
            if(captureFootSupport)CaptureSupport();
            Capture();
        }
        void LateUpdate()
        {
            if(Time.deltaTime<=0||!controller.isActiveAndEnabled){previous=transform.position;previousYaw=transform.eulerAngles.y;return;}
            Vector3 delta=transform.position-previous;delta.y=0;previous=transform.position;
            Vector3 local=transform.InverseTransformDirection(delta)/Time.deltaTime;
            velocity=Vector2.Lerp(velocity,new Vector2(local.x,local.z),1-Mathf.Exp(-18*Time.deltaTime));
            float yaw=transform.eulerAngles.y, turning=Mathf.DeltaAngle(previousYaw,yaw);previousYaw=yaw;
            turnRate=Mathf.Lerp(turnRate,turning/Time.deltaTime,1-Mathf.Exp(-18*Time.deltaTime));
            if(velocity.magnitude<.65f)turnPhase+=Mathf.Abs(turning)*(Mathf.PI*2/140f);
            float stride=captureFootSupport?SupportStride(velocity):adaptiveStride?RecordedCharacterMotion.StrideForSpeed(velocity.magnitude):2.35f;
            gait+=delta.magnitude*(Mathf.PI*2/stride);
            breath+=Time.deltaTime*1.45f;
            if(captureFootSupport)UpdateSupport(delta/Time.deltaTime);
            Capture();
        }

        static float SupportStride(Vector2 v)
        {
            float speed=v.magnitude;
            float run=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.7f,3.5f,speed));
            float side=speed>.01f?Mathf.Abs(v.x)/speed:0;
            return Mathf.Lerp(Mathf.Lerp(1.20f,2.10f,run),Mathf.Lerp(1.05f,1.30f,run),side*side);
        }

        void UpdateSupport(Vector3 worldVelocity)
        {
            float dt=Time.deltaTime, speed=velocity.magnitude;
            float moving=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.025f,.65f,speed));
            locomotionBlend=Mathf.Lerp(locomotionBlend,moving,1-Mathf.Exp(-12*dt));
            Vector2 travel=speed>.01f?velocity/speed:Vector2.up;
            float run=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.7f,3.5f,speed));
            float stance=Mathf.Lerp(Mathf.Lerp(.62f,.38f,run),Mathf.Lerp(.55f,.30f,run),Mathf.Abs(travel.x));
            float cycle=Mathf.Repeat(gait/(Mathf.PI*2),1);
            if(worldVelocity.sqrMagnitude>.0025f)
            {
                MovingFoot(ref leftStep,leftNeutral,-1,cycle,stance,travel,speed,run,dt);
                MovingFoot(ref rightStep,rightNeutral,1,Mathf.Repeat(cycle+.5f,1),stance,travel,speed,run,dt);
            }
            else
            {
                // Finish the actual swing and recover one foot at a time when
                // stopping or pivoting. This progress is captured, never replayed live.
                if(leftStep.swinging)Swing(ref leftStep,leftNeutral,0,.065f,dt);
                if(rightStep.swinging)Swing(ref rightStep,rightNeutral,0,.065f,dt);
                leftStep.wasSwingPhase=rightStep.wasSwingPhase=false;
                if(!leftStep.swinging)leftStep.roll=Mathf.MoveTowards(leftStep.roll,0,90*dt);
                if(!rightStep.swinging)rightStep.roll=Mathf.MoveTowards(rightStep.roll,0,90*dt);
                if(!leftStep.swinging&&!rightStep.swinging)
                {
                    float le=RecoveryError(leftStep,leftNeutral),re=RecoveryError(rightStep,rightNeutral);
                    if(Mathf.Max(le,re)>.065f)
                    {
                        if(le>=re)BeginSwing(ref leftStep,.22f);else BeginSwing(ref rightStep,.22f);
                    }
                }
            }
            LimitSupport(ref leftStep,leftNeutral,-1);
            LimitSupport(ref rightStep,rightNeutral,1);
            Vector3 left=transform.InverseTransformPoint(leftStep.position),right=transform.InverseTransformPoint(rightStep.position);
            Vector3 midpoint=(left+right-leftNeutral-rightNeutral)*.5f;
            Vector3 shift=new Vector3(Mathf.Clamp(midpoint.x,-.055f,.055f),
                -(.025f+.075f*run)*locomotionBlend,Mathf.Clamp(midpoint.z,-.045f,.045f));
            hipOffset=Vector3.Lerp(hipOffset,shift,1-Mathf.Exp(-12*dt));
            Vector3 acceleration=transform.InverseTransformDirection((worldVelocity-previousWorldVelocity)/Mathf.Max(dt,.001f));
            previousWorldVelocity=worldVelocity;
            var targetLean=new Vector2(Mathf.Clamp(-acceleration.x*.12f,-2.5f,2.5f),Mathf.Clamp(acceleration.z*.16f,-4.5f,4.5f));
            lean=Vector2.Lerp(lean,targetLean,1-Mathf.Exp(-8*dt));
            CaptureSupport();
        }

        void MovingFoot(ref FootStep foot,Vector3 neutral,int side,float phase,float stance,Vector2 travel,float speed,float run,float dt)
        {
            float heading=Mathf.Clamp(Mathf.Atan2(travel.x,Mathf.Max(.4f,Mathf.Abs(travel.y)))*Mathf.Rad2Deg*.22f,-20,20);
            if(phase>=stance)
            {
                if(!foot.wasSwingPhase)
                {
                    float remaining=(1-phase)*SupportStride(velocity)/Mathf.Max(speed,.15f);
                    BeginSwing(ref foot,Mathf.Clamp(remaining,.10f,.38f));
                }
                foot.wasSwingPhase=true;
                float span=SupportStride(velocity)*stance;
                Vector3 goal=neutral+new Vector3(travel.x*span*.5f,0,travel.y*span*.5f);
                // A lateral step-to gait keeps the trailing leg on its own
                // side instead of crossing both ankles under the torso.
                goal.x=side<0?Mathf.Min(goal.x,-.065f):Mathf.Max(goal.x,.065f);
                if(foot.swinging)Swing(ref foot,goal,heading,Mathf.Lerp(.095f,.17f,run),dt);
            }
            else
            {
                foot.wasSwingPhase=false;
                if(foot.swinging)
                {
                    foot.swinging=false;
                    Vector3 local=transform.InverseTransformPoint(foot.position);local.y=neutral.y;
                    foot.position=transform.TransformPoint(local);
                }
                float u=phase/stance;
                foot.roll=Mathf.Lerp(-6,10,Mathf.SmoothStep(0,1,u))*locomotionBlend;
            }
        }

        float RecoveryError(FootStep foot,Vector3 neutral)
            => Vector3.Distance(foot.position,transform.TransformPoint(neutral))+
               Mathf.Abs(Mathf.DeltaAngle(foot.yaw,transform.eulerAngles.y))*.0025f;

        static void BeginSwing(ref FootStep foot,float duration)
        { foot.start=foot.position;foot.startYaw=foot.yaw;foot.progress=0;foot.duration=duration;foot.swinging=true; }

        void Swing(ref FootStep foot,Vector3 goal,float heading,float height,float dt)
        {
            foot.progress=Mathf.Min(1,foot.progress+dt/foot.duration);
            float u=foot.progress,t=Mathf.SmoothStep(0,1,u);
            foot.position=Vector3.Lerp(foot.start,transform.TransformPoint(goal),t)+Vector3.up*(Mathf.Sin(u*Mathf.PI)*height);
            foot.yaw=Mathf.LerpAngle(foot.startYaw,transform.eulerAngles.y+heading,t);
            foot.roll=-Mathf.Sin(u*Mathf.PI)*10;
            if(u>=1){foot.swinging=false;foot.roll=0;}
        }

        void LimitSupport(ref FootStep foot,Vector3 neutral,int side)
        {
            Vector3 p=transform.InverseTransformPoint(foot.position);
            p.x=side<0?Mathf.Min(p.x,-.065f):Mathf.Max(p.x,.065f);
            Vector2 offset=Vector2.ClampMagnitude(new Vector2(p.x-neutral.x,p.z-neutral.z),.49f);
            p.x=neutral.x+offset.x;p.z=neutral.z+offset.y;
            // Clamp only unreachable supports after an abrupt real root motion.
            // The resulting authored correction is part of the recorded pose.
            foot.position=transform.TransformPoint(p);
        }

        void CaptureSupport()
        {
            Quaternion Rotation(FootStep foot)
            {
                float yaw=Mathf.Clamp(Mathf.DeltaAngle(transform.eulerAngles.y,foot.yaw),-55,55);
                return Quaternion.Euler(foot.roll,yaw,0);
            }
            Vector3 Contact(FootStep foot)
            {
                Vector3 p=transform.InverseTransformPoint(foot.position);
                float roll=Mathf.Sin(foot.roll*Mathf.Deg2Rad);
                p.y+=Mathf.Max(.19f*roll,-.08f*roll);
                return p;
            }
            locomotion=new RecordedLocomotion(Contact(leftStep),Contact(rightStep),Rotation(leftStep),Rotation(rightStep),hipOffset,lean,locomotionBlend);
        }
        void Capture()
        {
            Vector3? contact = recordedMachine && recordedMachine.TryGetPlayerGrip(out Vector3 point)
                ? transform.InverseTransformPoint(point) : (Vector3?)null;
            Current=new RecordedBodyPose(gait,breath,Mathf.DeltaAngle(0,view.transform.localEulerAngles.x)*.65f,velocity,turnPhase,turnRate,contact,locomotion);
        }
    }
}

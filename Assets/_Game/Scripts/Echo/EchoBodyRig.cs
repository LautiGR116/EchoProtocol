using UnityEngine;

namespace EchoProtocol.Echo
{
    // All articulation is a pure function of the sampled recorded pose. No
    // independent clock, noise, player tracking, gestures or AudioSources.
    [DisallowMultipleComponent]
    public sealed class EchoBodyRig : MonoBehaviour
    {
        [SerializeField] private RecordedCharacterMotion characterMotion;
        [SerializeField] private Transform modelRoot, hips, spine, head;
        [SerializeField] private Transform leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot;
        [SerializeField] private Transform leftArm,leftForearm,rightArm,rightForearm;
        private Transform[] joints;
        private Quaternion[] rest;
        private Vector3 hipRest;
        public RecordedBodyPose AppliedPose {get;private set;}
        public int AppliedSamples {get;private set;}
        public RecordedCharacterMotion CharacterMotion => characterMotion;
        void Awake()
        {
            if(characterMotion){characterMotion.Initialize();return;}
            joints=new[]{hips,spine,head,leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot,leftArm,leftForearm,rightArm,rightForearm};
            rest=new Quaternion[joints.Length];
            if(!modelRoot){Debug.LogError("Recorded rig has no coordinate root.",this);enabled=false;return;}
            for(int i=0;i<joints.Length;i++){if(!joints[i]){Debug.LogError("Recorded rig has an unassigned joint.",this);enabled=false;return;}rest[i]=joints[i].localRotation;}
            hipRest=hips.localPosition;
        }
        public void Apply(RecordedBodyPose pose)
        {
            if(characterMotion){if(!enabled)return;AppliedPose=pose;AppliedSamples++;characterMotion.Apply(pose);return;}
            if(!enabled||joints==null)return;
            AppliedPose=pose;AppliedSamples++;
            for(int i=0;i<joints.Length;i++)joints[i].localRotation=rest[i];
            hips.localPosition=hipRest;
            if(!pose.Captured)return;
            float amount=Mathf.Clamp01(pose.Velocity.magnitude/3.5f);
            Vector2 travel=pose.Velocity.sqrMagnitude>.001f?pose.Velocity.normalized:Vector2.zero;
            float sine=Mathf.Sin(pose.GaitPhase), breath=Mathf.Sin(pose.BreathPhase);
            Rotate(0,modelRoot.forward,-pose.Velocity.x*1.1f+sine*1.15f*amount);
            Rotate(1,modelRoot.right,breath*.48f-amount*2.0f);
            Rotate(2,modelRoot.right,pose.HeadPitch);
            // The imported skeleton is expressed in metres under modelRoot.
            hips.position+=modelRoot.up*((Mathf.Cos(2*pose.GaitPhase)-1)*.009f*amount+breath*.0018f);
            Leg(3,sine,amount,travel);Leg(6,-sine,amount,travel);
            Rotate(9,modelRoot.right,sine*19*amount*travel.y);
            Rotate(10,modelRoot.right,-5*amount-Mathf.Max(0,-sine)*8*amount);
            Rotate(11,modelRoot.right,-sine*19*amount*travel.y);
            Rotate(12,modelRoot.right,-5*amount-Mathf.Max(0,sine)*8*amount);
        }
        void Leg(int i,float phase,float amount,Vector2 travel)
        {
            float hip=-phase*34*amount*travel.y;
            float knee=Mathf.Max(0,phase)*Mathf.Lerp(25,48,Mathf.Abs(travel.y))*amount;
            Rotate(i,modelRoot.right,hip);Rotate(i+1,modelRoot.right,knee);
            // A sideways recorded path abducts the hips instead of running in
            // place while the root slides sideways. The captured local velocity
            // alone determines this direction; playback adds no new decision.
            joints[i].localRotation=Quaternion.AngleAxis(phase*22*amount*travel.x,
                joints[i].parent.InverseTransformDirection(modelRoot.forward))*joints[i].localRotation;
            Rotate(i+2,modelRoot.right,-hip*.35f-knee*.65f);
        }
        // Presentation is derived only from the explicitly recorded grip state.
        public void ApplyMachineGrip(bool held)
        {
            if(characterMotion){characterMotion.ApplyGrip(held);return;}
            if (!held || !enabled || joints == null) return;
            Rotate(11, modelRoot.right, -52);
            Rotate(12, modelRoot.right, -35);
        }
        public void SetPresentIntent(float releaseAge)
        { if(characterMotion)characterMotion.SetPresentIntent(releaseAge); }
        void Rotate(int i,Vector3 worldAxis,float degrees)
        {joints[i].localRotation=Quaternion.AngleAxis(degrees,joints[i].parent.InverseTransformDirection(worldAxis))*rest[i];}
    }
}

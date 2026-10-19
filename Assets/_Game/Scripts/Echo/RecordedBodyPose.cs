using UnityEngine;

namespace EchoProtocol.Echo
{
    // Authored Player-body animation state, sampled with the root. This is not
    // motion capture, eye tracking, or an audio channel. The optional machine
    // contact point is explicitly captured in Player-root coordinates.
    public readonly struct RecordedBodyPose
    {
        public readonly bool Captured;
        public readonly float GaitPhase, BreathPhase, HeadPitch;
        public readonly float TurnPhase, TurnRate;
        public readonly Vector2 Velocity;
        public readonly bool HasMachineGripTarget;
        public readonly Vector3 MachineGripTarget;
        public readonly RecordedLocomotion Locomotion;
        public RecordedBodyPose(float gait, float breath, float pitch, Vector2 velocity, float turnPhase=0, float turnRate=0, Vector3? machineGripTarget=null, RecordedLocomotion locomotion=default)
        { Captured=true; GaitPhase=gait; BreathPhase=breath; HeadPitch=pitch; Velocity=velocity; TurnPhase=turnPhase; TurnRate=turnRate; HasMachineGripTarget=machineGripTarget.HasValue; MachineGripTarget=machineGripTarget.GetValueOrDefault(); Locomotion=locomotion; }
        public static RecordedBodyPose Lerp(RecordedBodyPose a, RecordedBodyPose b, float t)
        {
            if(!a.Captured || !b.Captured) return default;
            return new RecordedBodyPose(Mathf.Lerp(a.GaitPhase,b.GaitPhase,t),
                Mathf.Lerp(a.BreathPhase,b.BreathPhase,t),Mathf.Lerp(a.HeadPitch,b.HeadPitch,t),Vector2.Lerp(a.Velocity,b.Velocity,t),
                Mathf.Lerp(a.TurnPhase,b.TurnPhase,t),Mathf.Lerp(a.TurnRate,b.TurnRate,t),
                a.HasMachineGripTarget && b.HasMachineGripTarget ? Vector3.Lerp(a.MachineGripTarget,b.MachineGripTarget,t) :
                t < .5f ? a.HasMachineGripTarget ? a.MachineGripTarget : (Vector3?)null : b.HasMachineGripTarget ? b.MachineGripTarget : (Vector3?)null,
                RecordedLocomotion.Lerp(a.Locomotion,b.Locomotion,t));
        }
    }

    // Authored foot support is solved on the live Player, then copied into the
    // take. Replay consumes these local targets; it never plants against the
    // current floor, chooses a new step, or advances a settling timer.
    public readonly struct RecordedLocomotion
    {
        public readonly bool Captured;
        public readonly Vector3 LeftFoot, RightFoot, HipOffset;
        public readonly Quaternion LeftRotation, RightRotation;
        public readonly Vector2 Lean;
        public readonly float Blend;
        public RecordedLocomotion(Vector3 left,Vector3 right,Quaternion leftRotation,Quaternion rightRotation,
            Vector3 hipOffset,Vector2 lean,float blend)
        { Captured=true;LeftFoot=left;RightFoot=right;LeftRotation=leftRotation;RightRotation=rightRotation;HipOffset=hipOffset;Lean=lean;Blend=blend; }
        public static RecordedLocomotion Lerp(RecordedLocomotion a,RecordedLocomotion b,float t)
        {
            if(!a.Captured||!b.Captured)return default;
            return new RecordedLocomotion(Vector3.Lerp(a.LeftFoot,b.LeftFoot,t),Vector3.Lerp(a.RightFoot,b.RightFoot,t),
                Quaternion.Slerp(a.LeftRotation,b.LeftRotation,t),Quaternion.Slerp(a.RightRotation,b.RightRotation,t),
                Vector3.Lerp(a.HipOffset,b.HipOffset,t),Vector2.Lerp(a.Lean,b.Lean,t),Mathf.Lerp(a.Blend,b.Blend,t));
        }
    }
}

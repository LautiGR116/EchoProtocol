using EchoProtocol.Echo;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Player
{
    // The present Player yields to the recorded body. The Echo's root, joints
    // and take stay untouched, even when a live Player tries to walk into it.
    [DefaultExecutionOrder(280), DisallowMultipleComponent]
    public sealed class PlayerEchoClearance : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private LoopController loop;
        [SerializeField] private GameSession session;
        [SerializeField] private float clearance=.64f;
        CharacterController controller;
        EchoPlayback owner;
        Transform chest;
        bool leftOrigin;
        public int Contacts { get; private set; }
        void Awake()
        {
            controller=player?player.GetComponent<CharacterController>():null;
            if(!player||!loop||!session||!controller)
            { Debug.LogError("Player body clearance requires explicit Player, loop and session.",this);enabled=false; }
        }
        void LateUpdate()
        {
            if(session.IsPaused||!player.isActiveAndEnabled||!controller.enabled)return;
            var echo=loop.ActiveEcho;
            if(!echo||echo.IsComplete||!echo.gameObject.activeInHierarchy){owner=null;return;}
            if(owner!=echo)
            {
                owner=echo;chest=null;leftOrigin=false;
                var rig=echo.GetComponent<EchoBodyRig>();
                if(rig&&rig.CharacterMotion)foreach(var joint in rig.CharacterMotion.Joints)
                    if(joint.name.EndsWith("spine_03",System.StringComparison.Ordinal)) {chest=joint;break;}
            }
            Vector3 center=chest?chest.position:echo.transform.position;
            if(Mathf.Abs(center.y-player.transform.position.y)>2.1f)return;
            Vector3 d=player.transform.position-center;d.y=0;
            float distance=d.magnitude;
            // RECALL's explicit shared origin is preserved. Once the Player has
            // stepped out, subsequent approaches respect the visible torso.
            if(!leftOrigin){if(distance>clearance+.12f)leftOrigin=true;return;}
            if(distance>=clearance)return;
            Vector3 away=distance>.001f?d/distance:-echo.transform.forward;
            controller.Move(away*Mathf.Min(clearance-distance,.18f));Contacts++;
        }
    }
}

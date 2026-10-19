using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    /// <summary>Present-tense carried prop only. HouseMemory owns its visibility;
    /// approaching a surface lowers the grip without affecting the camera or input.</summary>
    [DisallowMultipleComponent]
    public sealed class CarriedReservePresentation : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GameSession session;
        [SerializeField] private Vector3 carriedPosition = new Vector3(.26f, -.39f, .55f);
        [SerializeField] private Vector3 loweredPosition = new Vector3(.19f, -.59f, .41f);
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField, Min(.01f)] private float clearanceRadius = .16f;
        [SerializeField, Min(.1f)] private float lowerDistance = .80f;
        [SerializeField, Min(.01f)] private float fullyLoweredDistance = .30f;
        private readonly RaycastHit[] hits = new RaycastHit[32];
        private bool ready;
        private float lowered, reveal;
        public float LoweredFraction => lowered;

        private void Awake()
        {
            ready = view && player && session && transform.parent == view.transform;
            if (!ready) { Debug.LogError("CarriedReservePresentation requires the actual camera parent, Player and session.",this); enabled=false; }
        }
        private void OnEnable()
        {
            lowered=ready ? ObstructionFraction() : 0;
            reveal=0;
            ApplyPosition();
        }
        private void LateUpdate()
        {
            if (!ready || session.IsPaused) return;
            lowered=Mathf.MoveTowards(lowered,ObstructionFraction(),Time.deltaTime*7f);
            reveal=Mathf.MoveTowards(reveal,1f,Time.deltaTime*5f);
            ApplyPosition();
        }
        private void ApplyPosition() => transform.localPosition=Vector3.Lerp(carriedPosition,loweredPosition,lowered)+Vector3.down*((1f-reveal)*.24f);
        private float ObstructionFraction()
        {
            Vector3 target=view.transform.TransformPoint(carriedPosition+Vector3.up*.15f);
            Vector3 direction=(target-view.transform.position).normalized;
            int count=Physics.SphereCastNonAlloc(view.transform.position,clearanceRadius,direction,hits,lowerDistance,obstructionMask,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return 1f;
            float nearest=lowerDistance;
            for(int i=0;i<count;i++)
                if(hits[i].collider&&!hits[i].collider.transform.IsChildOf(player.transform))
                    nearest=Mathf.Min(nearest,hits[i].distance);
            return Mathf.InverseLerp(lowerDistance,fullyLoweredDistance,nearest);
        }
    }
}

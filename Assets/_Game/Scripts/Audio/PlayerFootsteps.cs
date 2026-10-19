using UnityEngine;
using EchoProtocol.Player;

namespace EchoProtocol.Audio
{
    // Current Player contact sounds are environmental feedback, never a replayed Echo channel.
    public sealed class PlayerFootsteps:MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] vinyl;
        [SerializeField] private AudioClip[] timber;
        [SerializeField] private BoxCollider[] timberZones;
        [SerializeField] private float stride=1.4f;
        private Vector3 previous;
        private float travelled;
        public int ContactCount{get;private set;}
        private void Awake()
        {
            previous=transform.position;
            if(player)player.PoseReset+=ResetMotion;
            if(!controller||!player||!source||vinyl.Length!=3||timber.Length!=3){Debug.LogError("PlayerFootsteps requires explicit controller, source and six original clips.",this);enabled=false;}
        }
        private void ResetMotion(){previous=transform.position;travelled=0;}
        private void OnDestroy(){if(player)player.PoseReset-=ResetMotion;}
        private void Update()
        {
            Vector3 p=transform.position;Vector3 delta=p-previous;previous=p;
            if(Time.timeScale<=0)return;
            // RECALL / checkpoint placement is not a physical footfall.
            if(delta.magnitude>1f){travelled=0;return;}
            delta.y=0;
            if(!controller.isGrounded || delta.sqrMagnitude<.000001f)return;
            travelled+=delta.magnitude;if(travelled<stride)return;travelled-=stride;
            bool wooden=false;foreach(var zone in timberZones)if(zone&&zone.enabled&&zone.gameObject.activeInHierarchy&&new Bounds(zone.center,zone.size).Contains(zone.transform.InverseTransformPoint(p+Vector3.up))){wooden=true;break;}
            var clips=wooden?timber:vinyl;source.transform.position=p+Vector3.up*.08f;
            source.PlayOneShot(clips[ContactCount%clips.Length]);ContactCount++;
        }
    }
}

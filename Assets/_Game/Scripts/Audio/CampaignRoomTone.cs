using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Player;
namespace EchoProtocol.Audio
{
    // Only owns the institutional beds after leaving the original act. D1 retains its first-act mix.
    public sealed class CampaignRoomTone:MonoBehaviour
    {
        [SerializeField] FirstPersonController player;
        [SerializeField] AudioSource[] institution;
        [SerializeField] FinalDoorMemory finalDoor;
        [SerializeField] CampaignDirector director;
        [SerializeField] VS1StrongEventOne firstActEvent;
        [SerializeField] PressureIsolation serviceIsolation;
        [SerializeField] MachineCoverEncounter machineCover;
        [Tooltip("Only institutional motor beds supplied by this plant; exclude room air and the local motor owned by PressureIsolationAudio.")]
        [SerializeField] AudioSource[] isolatedMotorBeds = new AudioSource[0];
        [Tooltip("Optional authored final-review volume. Uses the box in its own local frame, so a relocated/rotated portería retains its ambience mix.")]
        [SerializeField] BoxCollider finalReviewRegion;
        [SerializeField] EncounterAtmosphere encounterAtmosphere;
        float[] levels;bool entered;
        public bool OwnsAmbience=>entered;
        public int AmbienceWriteCount{get;private set;}
        void Start(){levels=new float[institution.Length];for(int i=0;i<levels.Length;i++)levels[i]=institution[i]?institution[i].volume:0f;}
        bool IsIsolatedMotor(AudioSource source)
        {foreach(var motor in isolatedMotorBeds)if(motor==source)return true;return false;}
        bool InFinalReviewRegion()
        {
            if(!finalReviewRegion || !finalReviewRegion.enabled || !finalReviewRegion.gameObject.activeInHierarchy)return false;
            Vector3 local=finalReviewRegion.transform.InverseTransformPoint(player.transform.position+Vector3.up);
            return new Bounds(finalReviewRegion.center,finalReviewRegion.size).Contains(local);
        }
        void Update()
        {
            if(!player||Time.timeScale==0)return;
            float z=player.transform.position.z;
            bool inFollowingRegion=z< -26;
            bool pastFirstAct=director
                ?director.Chapter!=CampaignChapter.Protocol&&director.Chapter!=CampaignChapter.Descent
                :inFollowingRegion;
            if(!entered&&pastFirstAct)
            {
                entered=true;
                if(firstActEvent)
                {
                    firstActEvent.ReleaseAmbienceControl();
                    // Do not write on the transfer frame: SE1 may have already
                    // updated. From the next frame onward it has relinquished.
                    return;
                }
            }
            if(!entered)return;
            bool returnedKey=finalDoor&&finalDoor.KeyReturned;
            float level=returnedKey?.015f:InFinalReviewRegion()?.20f:z< -125?.015f:z< -106?.1f:z< -74?.20f:z< -48?.055f:.35f;
            for(int i=0;i<levels.Length;i++)if(institution[i])
            {
                // Normalize the final descent to each bed's original level: the
                // threshold's .20 -> .015 transition takes about 3.7 seconds.
                // With no finalDoor reference, previous editions keep their mix.
                float rate=returnedKey?levels[i]*.05f:.10f;
                float target=levels[i]*level*(encounterAtmosphere && encounterAtmosphere.isActiveAndEnabled ? encounterAtmosphere.InstitutionGain : 1f);
                if(machineCover&&machineCover.Active)target*=machineCover.PresenceAmbience;
                // Checkpoint restores do not replay the first-act blackout.
                // Its 2D bed belongs only to that region and its chapters, even
                // when those distant lights still have their initial power.
                if(firstActEvent&&institution[i]==firstActEvent.FluorescentCircuitSource)
                    target*=!pastFirstAct&&!inFollowingRegion?firstActEvent.FluorescentPowerFraction:0f;
                if(serviceIsolation && IsIsolatedMotor(institution[i]))
                {
                    target*=serviceIsolation.MotorLevel01;
                    if(serviceIsolation.IsDrained){institution[i].volume=0f;continue;}
                }
                if(machineCover&&machineCover.Active&&IsIsolatedMotor(institution[i])){institution[i].volume=0;continue;}
                institution[i].volume=Mathf.MoveTowards(institution[i].volume,target,Time.deltaTime*rate);
            }
            AmbienceWriteCount++;
        }
    }
}

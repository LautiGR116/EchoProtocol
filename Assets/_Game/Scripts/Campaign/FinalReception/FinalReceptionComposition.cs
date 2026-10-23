using UnityEngine;

namespace EchoProtocol.Campaign.FinalReception
{
    /// <summary>
    /// Saved ownership and rollback data for the fixed 6B receiving recess.
    /// No Update: the real door carries the opaque insert, existing door audio
    /// follows its motion, and the recorded body retains its sole playback owner.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FinalReceptionComposition : MonoBehaviour
    {
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private HouseSwingDoor door;
        [SerializeField] private GameObject leafInsert;
        [SerializeField] private Light faceLight;
        [SerializeField] private Light handleLight;
        [SerializeField] private AudioSource hingeSource;
        [SerializeField] private float originalHingeVolume;
        [SerializeField] private Light originalInteriorLight;
        [SerializeField] private float originalInteriorIntensity;
        [SerializeField] private Light originalExteriorLight;
        [SerializeField] private float originalExteriorIntensity;
        [SerializeField] private int revision = 1;

        public FinalDoorMemory Review => review;
        public HouseSwingDoor Door => door;
        public GameObject LeafInsert => leafInsert;
        public AudioSource HingeSource => hingeSource;
        public float OriginalHingeVolume => originalHingeVolume;
        public Light OriginalInteriorLight => originalInteriorLight;
        public float OriginalInteriorIntensity => originalInteriorIntensity;
        public Light OriginalExteriorLight => originalExteriorLight;
        public float OriginalExteriorIntensity => originalExteriorIntensity;
        public bool IsConfigured => revision == 1 && review && door && leafInsert &&
            leafInsert.transform.IsChildOf(door.transform) && faceLight && handleLight &&
            faceLight.transform.IsChildOf(transform) && handleLight.transform.IsChildOf(transform) &&
            hingeSource && originalInteriorLight && originalExteriorLight;
    }
}

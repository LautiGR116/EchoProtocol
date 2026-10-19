using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Current-world ventilation and frame contact. Never attached to an Echo,
    // never reproduces a voice, step or recorded action, never grants progress.
    [DisallowMultipleComponent]
    public sealed class EncounterAtmosphere : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener;
        [SerializeField] private VS1ParallelFigureEvent passage;
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private BoxCollider passageRegion;
        [SerializeField] private BoxCollider finalRegion;
        [SerializeField] private AudioSource passageVent;
        [SerializeField] private AudioSource finalVent;
        [SerializeField] private AudioSource frameContact;
        [SerializeField] private AudioClip passageContact;
        [SerializeField] private Transform passageRotor;
        [SerializeField] private Transform finalRotor;
        [SerializeField] private EchoProtocol.Puzzle.BlindAlignmentRelay poweredAirlock;
        [SerializeField] private DomesticPresence presentPerson;
        [SerializeField, Range(0f, 1f)] private float passageVentGain = .16f;
        [SerializeField, Range(0f, 1f)] private float finalVentGain = .14f;
        private bool passageContactCommitted, finalQuiet;
        private float passageMotor = 1f, finalMotor = 1f;
        private bool ready;

        public float InstitutionGain { get; private set; } = 1f;
        public float PassageMotorLevel => passageMotor;
        public float FinalMotorLevel => finalMotor;
        public bool FinalQuiet => finalQuiet;
        public int PassageContactCount { get; private set; }

        private void Awake()
        {
            ready = session && listener && passage && review && passageRegion && finalRegion
                && passageVent && finalVent && frameContact && passageContact
                && passageRotor && finalRotor && passageVent != finalVent
                && passageVent.clip && finalVent.clip && passageVent.spatialBlend >= .999f
                && finalVent.spatialBlend >= .999f && frameContact.spatialBlend >= .999f;
            if (!ready)
            {
                Debug.LogError("EncounterAtmosphere requires explicit current-world sources, rotors, regions and real encounter references.", this);
                enabled = false;
                return;
            }
            passageVent.volume = finalVent.volume = 0f;
            passageVent.Play(); finalVent.Play();
        }

        private bool Inside(BoxCollider region)
        {
            if (!region.enabled || !region.gameObject.activeInHierarchy) return false;
            return new Bounds(region.center, region.size).Contains(region.transform.InverseTransformPoint(listener.position));
        }

        private void Update()
        {
            if (!ready || session.IsPaused || Time.timeScale <= 0f) return;
            // A genuinely recalled final recording changes the present setting.
            // It does not change that recording or react to an invented footfall.
            if (review.Active && review.ReviewedRecording != null) finalQuiet = true;
            passageMotor = Mathf.MoveTowards(passageMotor, passage.PresentationCommitted ? 0f : 1f, Time.deltaTime / 1.7f);
            finalMotor = Mathf.MoveTowards(finalMotor, finalQuiet ? 0f : 1f, Time.deltaTime / 1.1f);
            bool inPassage = Inside(passageRegion), inFinal = Inside(finalRegion);
            float target = inFinal && finalQuiet ? .08f : inPassage && passage.PresentationCommitted
                ? presentPerson ? (presentPerson.ImpactPlayed ? .18f : .12f) : .22f
                : poweredAirlock && poweredAirlock.IsPlayerSeparated ? .36f : 1f;
            InstitutionGain = Mathf.MoveTowards(InstitutionGain, target, Time.deltaTime / 1.25f);
            passageVent.volume = passageVentGain * passageMotor;
            finalVent.volume = finalVentGain * finalMotor;
            passageRotor.Rotate(Vector3.forward, 310f * passageMotor * Time.deltaTime, Space.Self);
            finalRotor.Rotate(Vector3.forward, 245f * finalMotor * Time.deltaTime, Space.Self);
            if (!presentPerson && !passageContactCommitted && passage.PresentationCommitted && passage.TraversalEntryCount > 0)
            {
                passageContactCommitted = true;
                PassageContactCount++;
                frameContact.PlayOneShot(passageContact);
            }
        }

        private void OnDisable()
        {
            InstitutionGain = 1f;
            if (!ready) return;
            passageVent.Stop(); finalVent.Stop(); frameContact.Stop();
        }
    }
}

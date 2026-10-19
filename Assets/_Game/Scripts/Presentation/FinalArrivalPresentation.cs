using EchoProtocol.Campaign;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Present-world reception instruments only. Never drives, retimes, extends
    // or adds channels to the recorded arrival; FinalDoorMemory owns acceptance.
    [DisallowMultipleComponent, DefaultExecutionOrder(200)]
    public sealed class FinalArrivalPresentation : MonoBehaviour
    {
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private GameSession session;
        [SerializeField] private Light receivingLamp;
        [SerializeField] private Light porterLamp;
        [SerializeField] private Light keyLamp;
        [SerializeField] private Light exitLamp;
        [SerializeField] private Renderer receivingLens;
        [SerializeField] private Renderer porterLens;
        [SerializeField] private TextMesh receivingStatus;
        [SerializeField] private TextMesh returnStatus;
        [SerializeField] private AudioSource receptionContact;
        [SerializeField] private AudioSource porterContact;
        [SerializeField] private AudioClip acceptedClip;
        [SerializeField, Min(.2f)] private float responseDelay = .45f;

        private bool ready, accepted, responsePlayed;
        private float acceptedTime, receivingLevel, porterLevel, keyLevel, exitLevel;
        private MaterialPropertyBlock lensProperties;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private readonly Color amber = new Color(1f, .68f, .28f);
        private readonly Color paper = new Color(.84f, .81f, .68f);

        private void Awake()
        {
            ready = review && session && review.UsesArrivalPresentation && receivingLamp && porterLamp && keyLamp && exitLamp
                && receivingLens && porterLens && receivingStatus && returnStatus && receptionContact && porterContact && acceptedClip;
            if (!ready)
            {
                Debug.LogError("FinalArrivalPresentation requires the explicit 6B reception, present-world lamps, displays and routed contacts.", this);
                enabled = false;
                return;
            }
            lensProperties = new MaterialPropertyBlock();
            receptionContact.playOnAwake = porterContact.playOnAwake = false;
            receptionContact.Stop(); porterContact.Stop();
        }

        private void LateUpdate()
        {
            if (!ready || session.IsPaused || Time.deltaTime <= 0) return;
            if (!review.HasWitnessed && accepted)
            {
                accepted = responsePlayed = false;
                acceptedTime = 0;
                receptionContact.Stop(); porterContact.Stop();
            }
            if (review.HasWitnessed && !accepted)
            {
                accepted = true;
                acceptedTime = 0;
                receptionContact.PlayOneShot(acceptedClip, .7f);
            }
            if (accepted)
            {
                acceptedTime += Time.deltaTime;
                if (!responsePlayed && acceptedTime >= responseDelay)
                {
                    responsePlayed = true;
                    porterContact.PlayOneShot(acceptedClip, .85f);
                }
            }
            // A short, single rise and a sustained destination, also readable
            // after a late camera turn. No flash cycle or forced camera motion.
            receivingLevel = Mathf.MoveTowards(receivingLevel, accepted ? 1f : .06f, Time.deltaTime * 3f);
            porterLevel = Mathf.MoveTowards(porterLevel, responsePlayed ? 1f : .12f, Time.deltaTime * 2f);
            keyLevel = Mathf.MoveTowards(keyLevel, review.OriginalPreserved ? 1f : .25f, Time.deltaTime * 2f);
            exitLevel = Mathf.MoveTowards(exitLevel, review.KeyReturned ? 1f : .18f, Time.deltaTime);
            receivingLamp.intensity = receivingLevel * 1.6f;
            porterLamp.intensity = porterLevel * 2.6f;
            keyLamp.intensity = keyLevel * 1.6f;
            exitLamp.intensity = exitLevel * 1.8f;
            SetLens(receivingLens, receivingLevel);
            SetLens(porterLens, porterLevel);
            receivingStatus.color = accepted ? amber : paper;
            string next = review.KeyReturned ? "LLAVE RECIBIDA\nSALIDA ABIERTA"
                : review.OriginalPreserved ? "E / DEVOLVER\nLA LLAVE"
                : "DEVOLUCIÓN / LLAVE";
            if (returnStatus.text != next) returnStatus.text = next;
        }

        private void SetLens(Renderer surface, float level)
        {
            surface.GetPropertyBlock(lensProperties);
            lensProperties.SetColor(BaseColor, Color.Lerp(new Color(.09f, .07f, .035f), amber, level));
            lensProperties.SetColor(EmissionColor, amber * level * 1.6f);
            surface.SetPropertyBlock(lensProperties);
        }

        private void OnDisable()
        {
            if (receptionContact) receptionContact.Stop();
            if (porterContact) porterContact.Stop();
        }
    }
}

using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    // A present-body surface consequence. It never writes the actor's bones,
    // movement, collision, recorded body or Echo data.
    [DefaultExecutionOrder(480), DisallowMultipleComponent]
    public sealed class NegativeUnsealingMask : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MachineCoverEncounter encounter;
        [SerializeField] EmbestidaRush rush;
        [SerializeField] SessionAudio output;
        [SerializeField] Transform leftPlate, rightPlate;
        [SerializeField] Vector3 hingeAxis = Vector3.up;
        [SerializeField] Vector3 leftOpenOffset, rightOpenOffset;
        [SerializeField] AudioSource fracture;
        [SerializeField, Min(0)] float contactDelay = .14f;
        [SerializeField, Min(.02f)] float openingSeconds = .12f;
        [SerializeField] float leftAngle = -38, rightAngle = 32;

        Quaternion leftRest, rightRest;
        Vector3 leftPosition, rightPosition;
        float age, cueLevel;
        bool ready, interrupted;
        public float Open01 { get; private set; }
        public bool HasOpened => OpeningCount > 0;
        public int OpeningCount { get; private set; }

        void Awake()
        {
            ready = session && encounter && rush && output && leftPlate && rightPlate
                && hingeAxis.sqrMagnitude > .01f && (!fracture || fracture.clip && fracture.outputAudioMixerGroup);
            if (!ready)
            {
                Debug.LogError("Unsealing mask requires explicit present-actor plates, contact and session references.", this);
                enabled = false; return;
            }
            hingeAxis.Normalize();
            leftRest = leftPlate.localRotation; rightRest = rightPlate.localRotation;
            leftPosition = leftPlate.localPosition; rightPosition = rightPlate.localPosition;
            if (fracture)
            {
                cueLevel = fracture.volume;
                fracture.loop = fracture.playOnAwake = fracture.ignoreListenerPause = false;
                fracture.Stop();
            }
        }

        void LateUpdate()
        {
            if (!ready || !session.HasStarted || !encounter.Active) return;
            // Capture pauses the session in the same frame. Consume a pending
            // opening before the pause guard so it cannot sound in the retry.
            if (encounter.Failed)
            {
                interrupted = true;
                if (fracture) fracture.Stop();
                return;
            }
            if (session.IsPaused || Time.deltaTime <= 0) return;
            if (encounter.Complete)
            {
                if (fracture) fracture.Stop();
                return;
            }
            if (interrupted) return;
            bool began = false;
            if (!HasOpened && rush.HasContact && rush.ContactAge >= contactDelay)
            {
                began = true;
                OpeningCount++;
                // ContactAge also handles a long frame without an extra delay.
                age = Mathf.Max(0, rush.ContactAge - contactDelay);
                if (fracture)
                {
                    fracture.volume = cueLevel * (output.ReducedDynamics ? .65f : 1);
                    fracture.Play();
                }
            }
            if (!HasOpened) return;
            if (!began) age += Time.deltaTime;
            Open01 = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / openingSeconds));
            leftPlate.localRotation = Quaternion.AngleAxis(leftAngle * Open01, hingeAxis) * leftRest;
            rightPlate.localRotation = Quaternion.AngleAxis(rightAngle * Open01, hingeAxis) * rightRest;
            leftPlate.localPosition = leftPosition + leftOpenOffset * Open01;
            rightPlate.localPosition = rightPosition + rightOpenOffset * Open01;
            // Intentionally no loop-reset hook: a physical fracture remains
            // open when the take is reused or the present threat resets.
        }

        void OnDisable() { if (fracture) fracture.Stop(); }
    }
}

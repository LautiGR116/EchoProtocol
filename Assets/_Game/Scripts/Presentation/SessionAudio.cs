using UnityEngine;
using EchoProtocol.Campaign;

namespace EchoProtocol.Presentation
{
    // Runs on the listener's final signal: no gain boost, bounded output and optional mono.
    [RequireComponent(typeof(AudioListener))]
    public sealed class SessionAudio : MonoBehaviour
    {
        [SerializeField] private FinalDoorMemory finalDoor;
        [SerializeField, Range(.18f,.65f)] private float normalCeiling=.65f;
        public volatile bool ReducedDynamics;
        public volatile bool Speakers;
        private float envelope;
        private float release;
        private volatile float targetFinalGain = 1f;
        private float finalGain = 1f;
        private float finalGainSmoothing;

        private void Awake()
        {
            int rate = AudioSettings.outputSampleRate;
            if (rate <= 0) rate = 48000;
            release = Mathf.Exp(-1f / (rate * .12f));
            finalGainSmoothing = 1f - Mathf.Exp(-1f / (rate * .04f));
        }

        private void Update()
        {
            // Read Unity state only on the main thread. The final screen retains
            // zero as its target; user volume, dynamics and speaker preferences
            // remain independent. Unwired editions retain unity gain.
            targetFinalGain = finalDoor ? (finalDoor.IsComplete ? 0f : 1f - Mathf.Clamp01(finalDoor.Fade)) : 1f;
        }

        private void OnAudioFilterRead(float[] data, int channels) => ProcessSamples(data, channels);
        internal void ProcessSamples(float[] data, int channels)
        {
            float finalTarget = targetFinalGain;
            for (int i = 0; i < data.Length; i += channels)
            {
                float peak = 0f, mono = 0f;
                for (int c = 0; c < channels; c++) { peak = Mathf.Max(peak, Mathf.Abs(data[i+c])); mono += data[i+c]; }
                envelope = Mathf.Max(peak, envelope * release);
                float threshold = ReducedDynamics ? .18f : normalCeiling;
                float gain = envelope > threshold ? threshold / envelope : 1f;
                // One shared envelope preserves stereo/mono balance and never
                // raises the signal. A 40 ms smoothing constant removes frame
                // steps without changing the five-second visual departure.
                finalGain = Mathf.Clamp01(finalGain + (finalTarget - finalGain) * finalGainSmoothing);
                if (finalTarget == 0f && finalGain < .0001f) finalGain = 0f;
                for (int c = 0; c < channels; c++)
                    data[i+c] = Mathf.Clamp((Speakers ? mono / channels : data[i+c]) * gain, -.85f, .85f) * finalGain;
            }
        }
    }
}

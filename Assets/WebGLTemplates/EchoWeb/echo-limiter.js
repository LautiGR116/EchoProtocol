// Matches SessionAudio.ProcessSamples. One envelope is shared by both channels;
// the processor attenuates peaks and never boosts the input signal.
class EchoLimiter extends AudioWorkletProcessor {
  static get parameterDescriptors() {
    return [
      { name: 'ceiling', defaultValue: 0.65, minValue: 0.01, maxValue: 0.85, automationRate: 'k-rate' },
      { name: 'mono', defaultValue: 0, minValue: 0, maxValue: 1, automationRate: 'k-rate' },
      { name: 'fade', defaultValue: 1, minValue: 0, maxValue: 1, automationRate: 'k-rate' }
    ];
  }
  constructor() {
    super();
    this.envelope = 0;
    this.finalGain = 1;
    this.release = Math.exp(-1 / (sampleRate * 0.12));
    this.smoothing = 1 - Math.exp(-1 / (sampleRate * 0.04));
  }
  process(inputs, outputs, parameters) {
    const input = inputs[0], output = outputs[0];
    if (!input.length || !output.length) return true;
    const ceiling = parameters.ceiling[0], mono = parameters.mono[0] >= 0.5;
    const target = parameters.fade[0];
    for (let i = 0; i < output[0].length; i++) {
      const left = input[0][i], right = (input[1] || input[0])[i];
      this.envelope = Math.max(Math.abs(left), Math.abs(right), this.envelope * this.release);
      const gain = this.envelope > ceiling ? ceiling / this.envelope : 1;
      this.finalGain += (target - this.finalGain) * this.smoothing;
      if (target === 0 && this.finalGain < 0.0001) this.finalGain = 0;
      const middle = (left + right) * 0.5;
      output[0][i] = Math.max(-0.85, Math.min(0.85, (mono ? middle : left) * gain)) * this.finalGain;
      if (output[1]) output[1][i] = Math.max(-0.85, Math.min(0.85, (mono ? middle : right) * gain)) * this.finalGain;
    }
    return true;
  }
}
registerProcessor('echo-limiter', EchoLimiter);

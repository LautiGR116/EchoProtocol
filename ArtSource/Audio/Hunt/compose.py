"""Five action-sized edits of the project's already traced CC0 masters.

No new performance, pitch change, voice generation or download. Run with the
project's Python/Numpy toolchain; Unity imports only the small final WAVs.
"""
from pathlib import Path
import hashlib
import json
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Assets/_Game/Audio/Clips/Hunt"
RATE = 48000
ledger = []


def read(relative, start, end):
    path = ROOT / relative
    with wave.open(str(path)) as source:
        assert source.getframerate() == RATE and source.getsampwidth() in (2, 3)
        raw = source.readframes(source.getnframes())
        if source.getsampwidth() == 3:
            octets = np.frombuffer(raw, "uint8").astype(np.int32).reshape(-1, 3)
            pcm = octets[:, 0] | (octets[:, 1] << 8) | (octets[:, 2] << 16)
            pcm = np.where(pcm & 0x800000, pcm - 0x1000000, pcm).astype(float) / 8388608
        else:
            pcm = np.frombuffer(raw, "<i2").astype(float) / 32768
        pcm = pcm.reshape(-1, source.getnchannels()).mean(1)
    return pcm[round(start * RATE):round(end * RATE)].copy()


def band(samples, low, high):
    # Two fixed Butterworth biquads; offline only, no runtime/native DSP needed.
    def biquad(signal, cutoff, highpass):
        w = 2 * np.pi * cutoff / RATE
        c, alpha = np.cos(w), np.sin(w) / np.sqrt(2)
        b = np.array([(1 + c) / 2, -(1 + c), (1 + c) / 2] if highpass
                     else [(1 - c) / 2, 1 - c, (1 - c) / 2]) / (1 + alpha)
        a1, a2 = -2 * c / (1 + alpha), (1 - alpha) / (1 + alpha)
        result = np.empty_like(signal)
        x1 = x2 = y1 = y2 = 0.0
        for i, x in enumerate(signal):
            y = b[0] * x + b[1] * x1 + b[2] * x2 - a1 * y1 - a2 * y2
            result[i] = y
            x2, x1, y2, y1 = x1, x, y1, y
        return result
    return biquad(biquad(samples, low, True), high, False)


def write(name, samples, peak, sources, edit, fade_in=.008, fade_out=.075):
    samples -= samples.mean()
    a, b = min(len(samples), round(fade_in * RATE)), min(len(samples), round(fade_out * RATE))
    samples[:a] *= np.linspace(0, 1, a)
    samples[-b:] *= np.linspace(1, 0, b) ** 2
    samples *= peak / max(abs(samples).max(), 1e-9)
    path = OUT / (name + ".wav")
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as output:
        output.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        output.writeframes(np.round(samples * 32767).astype("<i2").tobytes())
    ledger.append(dict(
        file=str(path.relative_to(ROOT)), sources=sources, license="CC0 1.0; existing repository source trail",
        edit=edit, seconds=len(samples) / RATE, peak=peak,
        rmsDb=float(20 * np.log10(max(np.sqrt(np.mean(samples ** 2)), 1e-9))),
        sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
    ))


voice = "ArtSource/Audio/Intruder/Decoded/272023-hq.wav"
breath = "ArtSource/Audio/Intruder/Decoded/173944-hq.wav"
metal = "ArtSource/Audio/Intruder/Decoded/556693-hq.wav"
sole = "Assets/_Game/Audio/Clips/Impact/Threat_Step1.wav"
scrape = "Assets/_Game/Audio/Clips/Impact/Threat_MovingFriction.wav"

write("Hunt_LaunchEffort", band(read(voice, .025, .40), 150, 5400), .28,
      ["aldenroth2 272023; ArtSource/Audio/Intruder/sources.json; .025-.40 s"],
      "Short existing adult expulsion on launch; original pitch/time, no periodic playback.")
write("Hunt_ReacquireEffort", band(read(voice, .16, .59), 120, 6700), .32,
      ["aldenroth2 272023; ArtSource/Audio/Intruder/sources.json; .16-.59 s"],
      "Later pressure/release from the same existing adult effort, attached to renewed pursuit, not a timed bark.", fade_in=.028)
write("Hunt_SearchIntake", band(read(breath, 6.00, 6.81), 220, 4700), .13,
      ["JohnsonBrandEditing 173944; ArtSource/Audio/Intruder/sources.json; 6.00-6.81 s"],
      "Same existing close intake as Embestida, one shortened inhalation before the actor listens; no speech or added performance.", fade_in=.025, fade_out=.15)

x = np.zeros(round(.47 * RATE))
heel = band(read(sole, .0, .26), 90, 5200)
grain = band(read(scrape, .35, .76), 230, 4200)
x[:len(heel)] += heel * .85
x[round(.055 * RATE):round(.055 * RATE) + len(grain)] += grain * .5
write("Hunt_BrakeSole", x, .29,
      ["Impact/Threat_Step1.wav: CC0 existing Overhaul/Finish floor contact trail",
       "Impact/Threat_MovingFriction.wav: AntumDeluge scrape-6.wav, CC0 existing Resonance trail"],
      "Dry planted heel plus short sole friction. One physical deceleration cue; no independent footsteps.")
write("Hunt_PumpContact", band(read(metal, .11, .57), 120, 4100), .32,
      ["NachtmahrTV 556693; ArtSource/Audio/Intruder/sources.json; .11-.57 s"],
      "Short damped housing contact, lower and shorter than entry impact; triggered only by the present hand's pump beat.")

(ROOT / "ArtSource/Audio/Hunt/masters.json").write_text(json.dumps(dict(
    date="2026-10-22", generation="None; edits of existing CC0 recordings only.",
    selectedMasters="All five WAVs below are the imported working masters. Performance/comfort remain EN PRUEBA humana.",
    masters=ledger,
), indent=2) + "\n")
for item in ledger:
    print(item["file"], item["seconds"], "s", item["rmsDb"], "dBFS RMS")

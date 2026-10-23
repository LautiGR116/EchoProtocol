#!/usr/bin/env python3
"""Rebuild two SE2-only, non-vocal contacts from existing CC0 project masters.

Python standard library only. Resolves already-local LFS objects read-only;
never downloads or materializes any input. Source~ is excluded from Unity import.
Run from anywhere: python3 Assets/_Game/Audio/SE2Presence/Source~/compose.py
"""
from pathlib import Path
import hashlib
import io
import json
import math
import struct
import subprocess
import wave

ROOT = Path(__file__).resolve().parents[5]
OUT = Path(__file__).resolve().parent.parent
RATE = 48000
INPUTS = {
    "knock": {
        "path": "Assets/_Game/Audio/Clips/SD1/SFX_SD1_DomesticKnock.wav",
        "sha256": "f8557e8591c8ea75f2a3b663427895534be7fb881b765fe416330af33e1f868a",
        "author": "SamuelGremaud",
        "title": "DOOR KNOCKING",
        "url": "https://freesound.org/people/SamuelGremaud/sounds/426736/",
        "license": "CC0 1.0",
        "evidence": "Docs/AudioEvidence/2026-08-12/FREESOUND_CC0_SD1_SNAPSHOT.md",
        "upstream_sha256": "06f1b0cb7b04108f9fff8b059daf03140c5fa90e9407d629b6d74079992be9e9",
    },
    "friction": {
        "path": "Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav",
        "sha256": "793ae3576c143031f3dac5b9e27709522d82a65711ba174cf7a00af13c7595c0",
        "author": "Rolly-SFX",
        "title": "Sliding Door Open",
        "url": "https://freesound.org/people/Rolly-SFX/sounds/626165/",
        "license": "CC0 1.0",
        "evidence": "Docs/AudioEvidence/2026-08-09/FREESOUND_CC0_SNAPSHOT.md",
        "upstream_sha256": "d3e3ec92617e27f4f21205a6a5dbee2b52d4bf6d8e26320d070d7b5142e30d8a",
    },
}


def read(key):
    item = INPUTS[key]
    data = (ROOT / item["path"]).read_bytes()
    if data.startswith(b"version https://git-lfs.github.com/spec/v1"):
        common = Path(subprocess.check_output(
            ["git", "rev-parse", "--git-common-dir"], cwd=ROOT, text=True).strip())
        if not common.is_absolute():
            common = ROOT / common
        sha = item["sha256"]
        data = (common / "lfs" / "objects" / sha[:2] / sha[2:4] / sha).read_bytes()
    if hashlib.sha256(data).hexdigest() != item["sha256"]:
        raise ValueError("Input changed: " + item["path"])
    with wave.open(io.BytesIO(data)) as wav:
        rate, channels, width = wav.getframerate(), wav.getnchannels(), wav.getsampwidth()
        raw = wav.readframes(wav.getnframes())
    if rate != RATE or width not in (2, 3):
        raise ValueError("Unexpected source format")
    values = [int.from_bytes(raw[i:i + width], "little", signed=True) / (1 << (width * 8 - 1))
              for i in range(0, len(raw), width)]
    return [sum(values[i:i + channels]) / channels for i in range(0, len(values), channels)]


def biquad(samples, hz, highpass=False):
    omega = 2 * math.pi * hz / RATE
    cosine, alpha = math.cos(omega), math.sin(omega) / math.sqrt(2)
    if highpass:
        b0, b1, b2 = (1 + cosine) / 2, -(1 + cosine), (1 + cosine) / 2
    else:
        b0, b1, b2 = (1 - cosine) / 2, 1 - cosine, (1 - cosine) / 2
    a0, a1, a2 = 1 + alpha, -2 * cosine, 1 - alpha
    b0, b1, b2, a1, a2 = (v / a0 for v in (b0, b1, b2, a1, a2))
    x1 = x2 = y1 = y2 = 0.0
    output = []
    for x0 in samples:
        y0 = b0 * x0 + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2
        output.append(y0)
        x2, x1, y2, y1 = x1, x0, y1, y0
    return output


def excerpt(key, start, end, duration, low, high):
    samples = read(key)[round(start * RATE):round(end * RATE)]
    n = round(duration * RATE)
    # One finite excerpt, never tiled or reversed. Linear resampling for the
    # small palm time expansion; friction retains its original time/pitch.
    if len(samples) != n:
        ratio = (len(samples) - 1) / (n - 1)
        stretched = []
        for i in range(n):
            cursor = i * ratio
            j = int(cursor)
            blend = cursor - j
            stretched.append(samples[j] * (1 - blend) + samples[min(j + 1, len(samples) - 1)] * blend)
        samples = stretched
    return biquad(biquad(samples, low, True), high)


def write(name, samples, fade_in, fade_out, peak_db, rms_cap_db, source, edit):
    mean = sum(samples) / len(samples)
    samples = [v - mean for v in samples]
    attack, release = round(fade_in * RATE), round(fade_out * RATE)
    for i in range(attack):
        samples[i] *= math.sin(i / (attack - 1) * math.pi / 2) ** 2
    for i in range(release):
        samples[-release + i] *= math.cos(i / (release - 1) * math.pi / 2) ** 2
    peak = max(abs(v) for v in samples)
    rms = math.sqrt(sum(v * v for v in samples) / len(samples))
    gain = min(10 ** (peak_db / 20) / peak, 10 ** (rms_cap_db / 20) / rms)
    integers = [round(v * gain * 32767) for v in samples]
    path = OUT / (name + ".wav")
    with wave.open(str(path), "wb") as wav:
        wav.setparams((1, 2, RATE, 0, "NONE", ""))
        wav.writeframes(struct.pack("<" + "h" * len(integers), *integers))
    values = [v / 32768 for v in integers]
    return {
        "file": path.name,
        "source": source,
        "edit": edit,
        "fade_in_ms": fade_in * 1000,
        "fade_out_ms": fade_out * 1000,
        "channels": 1,
        "sample_rate": RATE,
        "pcm_bits": 16,
        "seconds": len(values) / RATE,
        "bytes": path.stat().st_size,
        "peak_dbfs": 20 * math.log10(max(abs(v) for v in values)),
        "rms_dbfs": 20 * math.log10(math.sqrt(sum(v * v for v in values) / len(values))),
        "dc_mean": sum(values) / len(values),
        "first_sample": integers[0],
        "last_sample": integers[-1],
        "clipped_samples": sum(abs(v) >= 32767 for v in integers),
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
    }


def main():
    masters = [
        write("SE2_PalmSeat", excerpt("knock", .425, .680, .320, 85, 2100),
              .009, .115, -20, -32, "knock",
              "Single recorded contact 0.425-0.680 s, expanded to 0.320 s; "
              "85-2100 Hz soft band limits, no added resonance or room tail."),
        write("SE2_HandSlip", excerpt("friction", .680, 1.400, .720, 480, 3150),
              .065, .230, -25, -37, "friction",
              "One continuous friction excerpt 0.680-1.400 s, original time/pitch; "
              "480-3150 Hz soft band limits, 65 ms attack and 230 ms release, no loop."),
    ]
    manifest = {
        "date": "2026-10-23",
        "purpose": "SE2 current-world adult pressure against interior safety glass; "
                   "one weight seat at discovery, one hand slip after a quiet hold.",
        "status": "Implemented audio candidates; signal checks only. Human casting, "
                  "glass-material reading, spatial listening and comfort remain EN PRUEBA.",
        "rights": "Only existing CC0 recordings, per saved project evidence; no new "
                  "download, voice, speech, generated audio service, music or synthetic tone.",
        "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
        "runtime_contract": "Mono 3D WorldSFX one-shots at hand contact. Neither loops. "
                            "Do not raise source gain above the existing frameSound. "
                            "Keep listener pause, listener volume, mono and reduced dynamics.",
        "inputs": INPUTS,
        "masters": masters,
    }
    (OUT / "Source~" / "masters.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps(masters, indent=2))


if __name__ == "__main__":
    main()

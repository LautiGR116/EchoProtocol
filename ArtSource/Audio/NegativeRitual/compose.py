#!/usr/bin/env python3
"""Rebuild the eight non-vocal Negative ritual masters. Requires Python + NumPy.

Run from any directory: python3 ArtSource/Audio/NegativeRitual/compose.py
Existing input WAVs must be materialized with git lfs checkout first.
No downloads, voice synthesis, paid service, or runtime random scheduling.
Noise seeds describe fixed authored material, not random gameplay events.
"""
from pathlib import Path
import hashlib
import json
import math
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Assets/_Game/Audio/Clips/NegativeRitual"
RATE = 48000
DATE = "2026-10-30"

SOURCES = {
    "metal": {
        "path": "Assets/_Game/Audio/Clips/Intruder/Intruder_MetalBreach.wav",
        "author": "NachtmahrTV", "title": "Metal impact 556693",
        "url": "https://freesound.org/people/NachtmahrTV/sounds/556693/",
        "trace": "ArtSource/Audio/Intruder/masters.json",
    },
    "pressure": {
        "path": "Assets/_Game/Audio/Clips/Intruder/Intruder_DoorPressure.wav",
        "author": "alegemaate", "title": "Rattling door 364682",
        "url": "https://freesound.org/people/alegemaate/sounds/364682/",
        "trace": "ArtSource/Audio/Intruder/masters.json",
    },
    "scrape": {
        "path": "Assets/_Game/Audio/Clips/Impact/Threat_MovingFriction.wav",
        "author": "AntumDeluge", "title": "Scrapes: scrape-6.wav",
        "url": "https://opengameart.org/content/scrapes",
        "trace": "Assets/_Game/Audio/Clips/Impact/ImpactManifest.json",
    },
    "clank": {
        "path": "ArtSource/Audio/Resonance/Sources/mechanical-clank1.wav",
        "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds: mechanical-clank1.wav",
        "url": "https://opengameart.org/content/mechanical-sounds",
        "trace": "ArtSource/Audio/Resonance/Sources/download-manifest.json",
    },
    "rattle": {
        "path": "ArtSource/Audio/Resonance/Sources/mechanical-rattle1.wav",
        "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds: mechanical-rattle1.wav",
        "url": "https://opengameart.org/content/mechanical-sounds",
        "trace": "ArtSource/Audio/Resonance/Sources/download-manifest.json",
    },
    "door": {
        "path": "Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav",
        "author": "Rolly-SFX", "title": "Sliding Door Open 626165",
        "url": "https://freesound.org/people/Rolly-SFX/sounds/626165/",
        "trace": "Docs/AUDIO_ASSET_LEDGER.md",
    },
    "switch": {
        "path": "Assets/_Game/Audio/Clips/Mechanisms/SFX_PlateRelease.wav",
        "author": "se2001", "title": "Lamp Switch 503135",
        "url": "https://freesound.org/people/se2001/sounds/503135/",
        "trace": "Docs/AUDIO_ASSET_LEDGER.md",
    },
    "air": {
        "path": "Assets/_Game/Audio/Clips/Ambience/AMB_FacilityAir_LP.wav",
        "author": "HECKFRICKER", "title": "House Vent Loop 03 753542",
        "url": "https://freesound.org/people/HECKFRICKER/sounds/753542/",
        "trace": "Docs/AUDIO_ASSET_LEDGER.md",
    },
}


def read_wav(path):
    with wave.open(str(path)) as f:
        rate, channels, width = f.getframerate(), f.getnchannels(), f.getsampwidth()
        raw = f.readframes(f.getnframes())
    if width == 2:
        a = np.frombuffer(raw, "<i2").astype(np.float64) / 32768
    elif width == 3:
        b = np.frombuffer(raw, np.uint8).reshape(-1, 3).astype(np.int32)
        a = b[:, 0] | (b[:, 1] << 8) | (b[:, 2] << 16)
        a = ((a ^ 0x800000) - 0x800000).astype(np.float64) / 8388608
    else:
        raise ValueError(f"Unsupported sample width: {width} in {path}")
    a = a.reshape(-1, channels).mean(axis=1)
    if rate != RATE:
        a = np.interp(np.arange(round(len(a) * RATE / rate)) * rate / RATE, np.arange(len(a)), a)
    return a


def band(x, low=45, high=7000):
    """Smooth fourth-order amplitude slopes; padded FFT has no circular tail."""
    n = 1 << (len(x) + RATE - 1).bit_length()
    f = np.fft.rfftfreq(n, 1 / RATE)
    hp = 1 / np.sqrt(1 + (low / np.maximum(f, 0.001)) ** 8)
    lp = 1 / np.sqrt(1 + (f / high) ** 8)
    y = np.fft.irfft(np.fft.rfft(x, n) * hp * lp, n)[:len(x)]
    return y


def unit(x):
    return x / max(float(np.max(np.abs(x))), 1e-9)


def fade(x, attack=.012, release=.10):
    x = x.copy()
    a, r = min(round(attack * RATE), len(x)), min(round(release * RATE), len(x))
    if a > 0:
        x[:a] *= np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if r > 0:
        x[-r:] *= np.cos(np.linspace(0, np.pi / 2, r)) ** 2
    x[0] = x[-1] = 0
    return x


def shaped(x, points):
    t = np.arange(len(x)) / RATE
    return x * np.interp(t, [p[0] for p in points], [p[1] for p in points])


def pitched(x, speed=1.0):
    # All speed changes are on non-vocal mechanics/air. No human formants.
    return np.interp(np.arange(0, len(x) - 1, speed), np.arange(len(x)), x)


def segment(key, start=0, end=None, speed=1.0, low=45, high=7000):
    a = INPUTS[key]
    a = a[round(start * RATE):None if end is None else round(end * RATE)]
    if len(a) < 2:
        raise ValueError(f"Empty source cut: {key}/{start}/{end}")
    return unit(fade(band(pitched(a, speed), low, high), .006, .035))


def add(dst, src, time=0, gain=1):
    offset = round(time * RATE)
    n = min(len(src), len(dst) - offset)
    if n > 0:
        dst[offset:offset + n] += src[:n] * gain


def noise(duration, seed, low, high):
    x = np.random.default_rng(seed).standard_normal(round(duration * RATE))
    return unit(band(x, low, high))


def modes(duration, frequencies, decay, seed):
    # Finite, inharmonic metal plate responses, always underneath real recordings.
    t = np.arange(round(duration * RATE)) / RATE
    phases = np.random.default_rng(seed).uniform(-.12, .12, len(frequencies))
    x = np.zeros(len(t))
    for i, (f, phase) in enumerate(zip(frequencies, phases)):
        x += np.sin(2 * np.pi * (f * t + .0017 * f * np.sin(t * 3.1)) + phase) * np.exp(-t / (decay / (1 + i * .22))) / (1 + .4 * i)
    return unit(fade(band(x), .004, .17))


def room(x, amount=.15):
    # Sparse first reflections plus restrained dense diffusion, all printed mono.
    # Runtime position comes from a single explicit physical source.
    out = x.copy()
    reflected = band(x, 90, 3400)
    for dt, gain in [(.017, .63), (.031, -.39), (.057, .46), (.089, .31), (.133, -.24), (.191, .18), (.277, .13), (.391, -.09)]:
        add(out, reflected, dt, amount * gain)
    return out


def master(name, x, peak_db, sources, gain, description, cuts, attack=.008, release=.12):
    x = fade(band(x, 45, 7000), attack, release)
    # Remove DC while retaining silent endpoints; correction uses the same fade.
    correction_window = fade(np.ones(len(x)), attack, release)
    x -= correction_window * (np.sum(x) / np.sum(correction_window))
    x *= 10 ** (peak_db / 20) / max(np.max(np.abs(x)), 1e-9)
    assert np.isfinite(x).all()
    encoded = np.round(x * 32767).astype("<i2")
    path = OUT / f"NR_{name}.wav"
    with wave.open(str(path), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(encoded.tobytes())
    decoded = encoded.astype(np.float64) / 32768
    first, last = decoded[0], decoded[-1]
    peak = float(np.max(np.abs(decoded)))
    assert peak <= .5 and abs(np.mean(decoded)) < 1e-6 and first == last == 0
    spectrum = np.abs(np.fft.rfft(decoded)) ** 2
    frequencies = np.fft.rfftfreq(len(decoded), 1 / RATE)
    receipt = {
        "file": str(path.relative_to(ROOT)), "seconds": round(len(x) / RATE, 6),
        "sampleRate": RATE, "channels": 1, "pcmBits": 16, "loop": False,
        "sourceKeys": sources, "sourceCutsSeconds": cuts,
        "originalLayers": description, "sourceGain": gain,
        "peakDbFS": round(20 * math.log10(peak), 3),
        "rmsDbFS": round(20 * math.log10(max(float(np.sqrt(np.mean(decoded ** 2))), 1e-12)), 3),
        "dc": float(np.mean(decoded)), "edgeSamples": [float(first), float(last)],
        "fadeSeconds": [attack, release],
        "energyBelow45HzPercent": round(float(spectrum[frequencies < 45].sum() / spectrum.sum() * 100), 3),
        "energyAbove7000HzPercent": round(float(spectrum[frequencies > 7000].sum() / spectrum.sum() * 100), 3),
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "bytes": path.stat().st_size,
        "humanListeningApproval": "PENDING: technical signal validation does not establish fear or comfort",
    }
    print(f"{name:20} {receipt['seconds']:4.2f}s  peak {receipt['peakDbFS']:7.3f} dBFS  RMS {receipt['rmsDbFS']:7.3f} dBFS")
    return receipt


def render():
    masters = []
    x = np.zeros(round(3.2 * RATE))
    drag = segment("door", 0, 2.1, .73, 75, 3600)
    drag = shaped(drag, [(0, 0), (.22, .32), (1.2, .48), (2.25, 1), (2.7, .10), (2.87, 0)])
    add(x, drag, 0, .64)
    add(x, segment("pressure", .02, .64, .67, 120, 3000), 1.88, .19)
    add(x, modes(2.85, [103, 167, 294, 511, 912], 1.1, 31), .16, .085)
    add(x, segment("rattle", 0, .34, .84, 160, 4800), 2.75, .10)
    masters.append(master("SteelCompression", room(x, .12), -11, ["door", "pressure", "rattle"], .16,
        "Strain envelope rises into a restrained physical jam; fixed inharmonic plate modes; four sparse early reflections.",
        {"door": [0, 2.1, .73], "pressure": [.02, .64, .67], "rattle": [0, .34, .84]}))

    x = np.zeros(round(1.55 * RATE))
    add(x, segment("metal", 0, .72, 1, 60, 7000), .006, .64)
    add(x, segment("clank", 0, .63, .71, 70, 4800), .009, .22)
    add(x, modes(1.53, [67, 109, 179, 311, 547, 1033], .59, 39), .012, .30)
    add(x, shaped(segment("rattle", 0, .72, .61, 110, 2600), [(0, 0), (.13, .5), (.47, .28), (1.18, 0)]), .14, .18)
    masters.append(master("WallStrike", room(x, .27), -6.5, ["metal", "clank", "rattle"], .30,
        "One dry initial wall contact plus deep inharmonic sheet body, settling hardware and printed room tail. No sub drop or hiss layer. Author a 0.3-0.6s sound subtraction before contact.",
        {"metal": [0, .72, 1], "clank": [0, .63, .71], "rattle": [0, .72, .61]}, .004, .15))

    x = np.zeros(round(2.4 * RATE))
    scrape = segment("scrape", .04, 2.08, .91, 115, 6500)
    scrape = shaped(scrape, [(0, 0), (.12, .4), (.52, .9), (1.05, .33), (1.64, 1), (2.17, .25), (2.24, 0)])
    add(x, scrape, .02, .65)
    add(x, shaped(segment("door", .33, 1.53, .69, 75, 1800), [(0, 0), (.4, .6), (1.5, .4), (1.74, 0)]), .24, .14)
    masters.append(master("CloseDrag", room(x, .07), -12.5, ["scrape", "door"], .17,
        "Two unequal resistance peaks within one continuous coarse mechanical drag; sparse short reflections. Position/movement belongs to physical apparatus.",
        {"scrape": [.04, 2.08, .91], "door": [.33, 1.53, .69]}, .014, .15))

    x = np.zeros(round(4.8 * RATE))
    door = segment("door", .05, 2.06, .46, 80, 3100)
    door = shaped(door, [(0, 0), (.6, .16), (1.6, .57), (2.7, .32), (3.9, .85), (4.37, 0)])
    add(x, door, .08, .43)
    add(x, shaped(segment("scrape", .12, 2.0, .62, 230, 4000), [(0, 0), (.7, .6), (2.1, .9), (3.03, 0)]), 1.3, .16)
    add(x, modes(4.5, [83, 137, 251, 479, 887], 2.0, 53), .14, .035)
    masters.append(master("OverheadStrain", room(x, .30), -14.5, ["door", "scrape"], .14,
        "Slow load transfer with continuous non-vocal source material, quiet dispersive steel modes and ceiling reflections; no periodic beat.",
        {"door": [.05, 2.06, .46], "scrape": [.12, 2.0, .62]}, .065, .24))

    x = np.zeros(round(.48 * RATE))
    add(x, segment("switch", .148, .4, 1.08, 180, 6500), .006, .66)
    add(x, segment("clank", .03, .21, 1.15, 250, 4200), .039, .15)
    masters.append(master("DryLatch", x, -12, ["switch", "clank"], .14,
        "Clinical dry relay closure and low-level inner mechanism after-contact, no synthetic confirmation beep.",
        {"switch": [.148, .4, 1.08], "clank": [.03, .21, 1.15]}, .003, .055))

    x = np.zeros(round(3.6 * RATE))
    air = segment("air", 7, 10.6, 1, 65, 1800)
    air = shaped(air, [(0, 0), (.32, .08), (1.45, .72), (2.14, 1), (2.38, .18), (3.1, .055), (3.59, 0)])
    add(x, air, 0, .7)
    turbulence = shaped(noise(3.6, 79, 105, 780), [(0, 0), (1.15, .28), (1.88, .55), (2.18, .72), (2.4, .04), (3.59, 0)])
    add(x, turbulence, 0, .17)
    add(x, shaped(segment("door", .4, 1.4, .6, 65, 650), [(0, 0), (.35, .6), (1.45, .2), (1.66, 0)]), .53, .055)
    masters.append(master("PressureInhale", room(x, .10), -14.5, ["air", "door"], .14,
        "A machine chamber draws air: existing vent plus fixed band-limited turbulence seed79. Suggests breathing only through an asymmetric pressure envelope; no person, voice, vowel or recorded breath.",
        {"air": [7, 10.6, 1], "door": [.4, 1.4, .6]}, .055, .20))

    x = np.zeros(round(1.8 * RATE))
    add(x, shaped(segment("air", 11, 12.36, 1, 90, 2400), [(0, 0), (.12, .65), (.39, 1), (.47, .12), (.63, 0), (1.35, 0)]), 0, .4)
    add(x, segment("pressure", .02, .64, .72, 90, 4500), .455, .35)
    add(x, modes(1.3, [89, 153, 293, 521], .23, 83), .47, .16)
    add(x, shaped(segment("air", 12, 12.85, 1, 160, 1800), [(0, 0), (.17, .32), (.42, .18), (.85, 0)]), .92, .17)
    masters.append(master("PressureChoke", room(x, .13), -10.5, ["air", "pressure"], .22,
        "Intake is abruptly pinched by a real hinge contact; interrupted air then one weak pressure escape. Non-vocal machine event, no cry/formant/grunt.",
        {"air": [[11, 12.36, 1], [12, 12.85, 1]], "pressure": [.02, .64, .72]}, .018, .16))

    x = np.zeros(round(2.1 * RATE))
    add(x, segment("metal", .03, .73, .58, 70, 1600), .016, .42)
    add(x, modes(2.08, [74, 127, 239, 419, 769], .75, 97), .021, .17)
    add(x, shaped(segment("rattle", .08, .63, .75, 140, 1700), [(0, 0), (.12, .6), (.42, .28), (.73, 0)]), .45, .08)
    masters.append(master("RemoteAnswer", room(x, .5), -13, ["metal", "rattle"], .16,
        "One occluded mechanical consequence through a distant wall with long diffuse tail; never labelled as a recording, Echo replay or an unexplained extra channel.",
        {"metal": [.03, .73, .58], "rattle": [.08, .63, .75]}, .012, .21))
    return masters


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    INPUTS = {key: read_wav(ROOT / data["path"]) for key, data in SOURCES.items()}
    for data in SOURCES.values():
        data["sha256"] = hashlib.sha256((ROOT / data["path"]).read_bytes()).hexdigest()
        data["license"] = "CC0 1.0 as documented by the existing source ledger; not re-downloaded or re-licensed"
    receipt = {
        "version": 1, "date": DATE, "title": "El Negativo / physical ritual apparatus",
        "recipe": str(Path(__file__).resolve().relative_to(ROOT)), "numpyVersion": np.__version__,
        "rights": "Existing inputs credited CC0 1.0; new authored DSP/composition is project work. No new voice, external generation or license representation.",
        "integration": "Explicit 3D WorldSFX sources; causal sequence owns timing and transforms. Final SessionAudio limiter/master/mono/reduced-dynamics remain in control. Source gains are conservative starting points, not integrated listening approval.",
        "sources": SOURCES, "masters": render(),
    }
    (Path(__file__).parent / "masters.json").write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n")

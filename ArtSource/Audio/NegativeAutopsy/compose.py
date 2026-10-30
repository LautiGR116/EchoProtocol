#!/usr/bin/env python3
"""Rebuild four physical, non-vocal Autopsy masters with Python + NumPy.

Run from any directory. Materialize the listed existing LFS inputs first.
All irregular granulation is printed deterministically into the files; no
runtime random scare scheduler, external generation or new voice is involved.
"""
from pathlib import Path
import hashlib
import json
import math
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Assets/_Game/Audio/Clips/NegativeAutopsy"
RATE = 48000
DATE = "2026-10-30"
TRACE = "ArtSource/Audio/Resonance/Sources/download-manifest.json"
SOURCES = {
    "scrape2": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-2.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-2.wav", "url": "https://opengameart.org/content/scrapes"},
    "scrape4": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-4.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-4.wav", "url": "https://opengameart.org/content/scrapes"},
    "scrape7": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-7.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-7.wav", "url": "https://opengameart.org/content/scrapes"},
    "clunk": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-lightclunk1.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / lightclunk1.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "mechanism": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-mechanical2.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / mechanical2.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "key": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-typewriter.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / typewriter.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "air": {"path": "Assets/_Game/Audio/Clips/Ambience/AMB_FacilityAir_LP.wav", "author": "HECKFRICKER", "title": "House Vent Loop 03 / existing facility-air derivative", "url": "https://freesound.org/people/HECKFRICKER/sounds/753542/", "sourceTrace": "Docs/AUDIO_ASSET_LEDGER.md"},
}


def read_wav(path):
    with wave.open(str(path)) as f:
        rate, channels, width = f.getframerate(), f.getnchannels(), f.getsampwidth()
        raw = f.readframes(f.getnframes())
        if width == 2:
            x = np.frombuffer(raw, "<i2").astype(np.float64) / 32768
        elif width == 3:
            b = np.frombuffer(raw, np.uint8).reshape(-1, 3).astype(np.int32)
            x = b[:, 0] | (b[:, 1] << 8) | (b[:, 2] << 16)
            x = ((x ^ 0x800000) - 0x800000).astype(np.float64) / 8388608
        else:
            raise ValueError("Expected credited PCM16/24 input: " + str(path))
    x = x.reshape(-1, channels).mean(axis=1)
    if rate != RATE:
        x = np.interp(np.arange(round(len(x) * RATE / rate)) * rate / RATE, np.arange(len(x)), x)
    return x


def band(x, low=55, high=6800):
    # Padding avoids wrapping the filter response back into an initial attack.
    n = 1 << (len(x) + RATE - 1).bit_length()
    f = np.fft.rfftfreq(n, 1 / RATE)
    hp = 1 / np.sqrt(1 + (low / np.maximum(f, .001)) ** 8)
    lp = 1 / np.sqrt(1 + (f / high) ** 8)
    return np.fft.irfft(np.fft.rfft(x, n) * hp * lp, n)[:len(x)]


def unit(x):
    return x / max(float(np.max(np.abs(x))), 1e-9)


def fade(x, attack=.008, release=.10):
    x = x.copy()
    a, r = min(round(attack * RATE), len(x)), min(round(release * RATE), len(x))
    if a:
        x[:a] *= np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if r:
        x[-r:] *= np.cos(np.linspace(0, np.pi / 2, r)) ** 2
    x[0] = x[-1] = 0
    return x


def envelope(x, points):
    t = np.arange(len(x)) / RATE
    return x * np.interp(t, [p[0] for p in points], [p[1] for p in points])


def cut(key, start, end, speed=1, low=55, high=6800):
    x = INPUTS[key][round(start * RATE):round(end * RATE)]
    if len(x) < 2:
        raise ValueError(f"Empty source cut: {key}/{start}/{end}")
    x = np.interp(np.arange(0, len(x) - 1, speed), np.arange(len(x)), x)
    return unit(fade(band(x, low, high), .003, .025))


def add(dst, src, at=0, gain=1):
    offset = round(at * RATE)
    n = min(len(src), len(dst) - offset)
    if n > 0:
        dst[offset:offset + n] += src[:n] * gain


def resonance(duration, modes, decay, seed):
    # A short, finite ceramic/shell response; no sustained pure-tone cue.
    t = np.arange(round(duration * RATE)) / RATE
    phases = np.random.default_rng(seed).uniform(-.06, .06, len(modes))
    x = np.zeros(len(t))
    for i, (frequency, phase) in enumerate(zip(modes, phases)):
        x += np.sin(2 * np.pi * frequency * t + phase) * np.exp(-t / (decay / (1 + i * .38))) / (1 + i * .6)
    return unit(fade(band(x), .0015, .15))


def early_room(x, amount):
    # Small fixed reflections; a near physical surface stays dry and local.
    out = x.copy()
    reflected = band(x, 160, 3400)
    for delay, gain in [(.013, .55), (.029, -.34), (.053, .28), (.087, .19), (.139, -.14), (.207, .09)]:
        add(out, reflected, delay, amount * gain)
    return out


def master(name, x, peak_db, gain, source_cuts, choreography, notes, attack=.008, release=.15):
    x = fade(band(x), attack, release)
    window = fade(np.ones(len(x)), attack, release)
    x -= window * (np.sum(x) / np.sum(window))
    x *= 10 ** (peak_db / 20) / max(float(np.max(np.abs(x))), 1e-9)
    assert np.isfinite(x).all()
    encoded = np.round(x * 32767).astype("<i2")
    path = OUT / f"NA_{name}.wav"
    with wave.open(str(path), "wb") as f:
        f.setnchannels(1); f.setsampwidth(2); f.setframerate(RATE); f.writeframes(encoded.tobytes())
    y = encoded.astype(np.float64) / 32768
    peak = float(np.max(np.abs(y)))
    assert peak < .5 and abs(float(np.mean(y))) < 1e-6 and y[0] == y[-1] == 0
    # The 10%-of-peak arrival is an objective alignment aid, not heard onset.
    first = int(np.flatnonzero(np.abs(y) >= peak * .1)[0]) / RATE
    receipt = {
        "file": str(path.relative_to(ROOT)), "seconds": len(y) / RATE,
        "sampleRate": RATE, "channels": 1, "pcmBits": 16, "loop": False,
        "sourceCutsSeconds": source_cuts, "sourceGain": gain,
        "authoredChoreographySeconds": choreography, "composition": notes,
        "peakDbFS": round(20 * math.log10(peak), 3),
        "rmsDbFS": round(20 * math.log10(float(np.sqrt(np.mean(y ** 2)))), 3),
        "onset10PercentPeakSeconds": round(first, 6),
        "peakAtSeconds": round(int(np.argmax(np.abs(y))) / RATE, 6),
        "dc": float(np.mean(y)), "fadeSeconds": [attack, release], "edgeSamples": [float(y[0]), float(y[-1])],
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "bytes": path.stat().st_size,
        "humanListeningApproval": "PENDING: signal checks cannot approve material identity, fear, acting, localization or comfort",
    }
    print(f"{name:18} {receipt['seconds']:.2f}s peak={receipt['peakDbFS']:.2f}dBFS onset10%={first:.3f}s peakAt={receipt['peakAtSeconds']:.3f}s")
    return receipt


def at_peak(dst, src, when, gain=1):
    """Place a recorded contact's principal transient at its authored time."""
    offset = round(when * RATE) - int(np.argmax(np.abs(src)))
    if offset < 0:
        src = src[-offset:]; offset = 0
    add(dst, src, offset / RATE, gain)


def render():
    result = []
    x = np.zeros(round(2.4 * RATE))
    taut = envelope(cut("scrape2", .14, .48, .72, 180, 2200), [(0, 0), (.11, .6), (.32, .7), (.47, 0)])
    add(x, taut, .025, .065)
    at_peak(x, cut("scrape7", .05, .38, .91, 230, 6300), .45, .6)
    at_peak(x, cut("key", .002, .113, 1.25, 550, 5900), .452, .13)
    peel = envelope(cut("scrape4", .23, 1.30, .72, 130, 3700), [(0, 0), (.12, .55), (.49, .9), (.84, .36), (1.3, .22), (1.48, 0)])
    add(x, peel, .59, .23)
    for at, start, end, gain in [(.81, .47, .55, .035), (1.13, .69, .79, .026), (1.52, 1.03, 1.12, .018)]:
        add(x, cut("scrape7", start, end, .9, 450, 5000), at, gain)
    result.append(master("Suture", early_room(x, .035), -11.5, .18,
        {"scrape2": [[.14, .48, .72]], "scrape7": [[.05, .38, .91], [.47, .55, .9], [.69, .79, .9], [1.03, 1.12, .9]], "key": [[.002, .113, 1.25]], "scrape4": [[.23, 1.30, .72]]},
        {"tautFiber": [.025, .45], "mainTearPeak": .45, "clothPeel": [.59, 2.08]},
        "A taut surface loads, one close fiber knot separates, then the remaining cloth draws away with three diminishing loose fibers. Crisp recorded friction and typewriter spring detail; no vocal or injury recording.", .012, .20))

    x = np.zeros(round(3.0 * RATE))
    at_peak(x, cut("clunk", .005, .35, .54, 85, 3800), .18, .48)
    at_peak(x, cut("mechanism", .02, .25, .68, 200, 4300), .183, .16)
    add(x, resonance(1.7, [73, 129, 211, 367, 613], .16, 601), .169, .12)
    hinge = envelope(cut("scrape4", .15, 1.20, .63, 95, 2400), [(0, 0), (.13, .65), (.47, .43), (.86, .7), (1.35, .16), (1.66, 0)])
    add(x, hinge, .32, .15)
    # Cavity turbulence comes from recorded air, with an asymmetrical short escape.
    cavity = envelope(cut("air", 6.2, 8.45, 1, 90, 900), [(0, 0), (.20, .55), (.49, .8), (.8, .28), (1.55, .1), (2.24, 0)])
    add(x, cavity, .23, .055)
    at_peak(x, cut("clunk", .09, .22, .84, 350, 4600), .64, .055)
    result.append(master("Thorax", early_room(x, .11), -8.5, .24,
        {"clunk": [[.005, .35, .54], [.09, .22, .84]], "mechanism": [[.02, .25, .68]], "scrape4": [[.15, 1.20, .63]], "air": [[6.2, 8.45, 1]]},
        {"mainBodyPeak": .18, "hingeTravel": [.32, 1.99], "secondaryDryStop": .64, "cavityEscape": [.23, 2.48]},
        "A deep dry shell contact opens into a short damp cavity response, followed by strained hinge friction and low escaping chamber air. Five brief original shell modes seed601 add body without a drone or sub-drop. No human bodily performance is claimed.", .006, .24))

    x = np.zeros(round(1.8 * RATE))
    at_peak(x, cut("key", .001, .115, .79, 360, 6200), .06, .44)
    at_peak(x, cut("clunk", .015, .16, 1.21, 420, 5800), .065, .20)
    add(x, resonance(1.0, [421, 877, 1523, 2699, 4211], .085, 607), .054, .075)
    for at, start, end, speed, gain in [(.19, .48, .56, 1.1, .11), (.33, .68, .77, .91, .065), (.57, 1.03, 1.11, .83, .028)]:
        add(x, cut("scrape2", start, end, speed, 600, 5800), at, gain)
    add(x, envelope(cut("mechanism", .12, .47, .86, 190, 1600), [(0, 0), (.1, .6), (.4, 0)]), .12, .042)
    result.append(master("Jaw", early_room(x, .045), -11, .20,
        {"key": [[.001, .115, .79]], "clunk": [[.015, .16, 1.21]], "scrape2": [[.48, .56, 1.1], [.68, .77, .91], [1.03, 1.11, .83]], "mechanism": [[.12, .47, .86]]},
        {"mainReleasePeak": .06, "fallingFragments": [.19, .33, .57]},
        "A brittle joint releases once, with compact mechanical spring/clunk grains, a very short original bone-like shell response seed607 and small diminishing fragments. Deliberately dry, no sustained ringing or vocal expression.", .003, .20))

    x = np.zeros(round(3.6 * RATE))
    low = cut("air", 13.1, 16.7, 1, 95, 1000)
    high = cut("air", 9.2, 12.8, 1, 750, 3600)
    low = envelope(low, [(0, 0), (.13, .14), (.48, .8), (.72, 1), (1.18, .47), (1.59, .61), (2.25, .21), (2.93, .07), (3.59, 0)])
    high = envelope(high, [(0, 0), (.07, .06), (.33, .7), (.65, .36), (1.05, .25), (1.65, .18), (2.4, .08), (3.2, 0), (3.59, 0)])
    add(x, low, 0, .62); add(x, high, 0, .23)
    # Tiny physical throat-of-the-apparatus friction, not a voiced human throat.
    add(x, envelope(cut("scrape4", .18, .60, .67, 230, 1500), [(0, 0), (.12, .5), (.28, .18), (.62, 0)]), .51, .018)
    result.append(master("AfterBreath", early_room(x, .018), -15.5, .14,
        {"air": [[13.1, 16.7, 1], [9.2, 12.8, 1]], "scrape4": [[.18, .60, .67]]},
        {"emptyingOnset": .07, "pressureRise": [.13, .72], "unevenRelease": [.72, 2.93], "airGone": 3.59},
        "Two differently filtered sections of credited vent air form one intimate, empty chamber exhalation: brief textured opening, uneven pressure loss and almost dry disappearance. No voice, breath performer, formant synthesis or continuous horror drone.", .035, .25))
    return result


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    INPUTS = {key: read_wav(ROOT / item["path"]) for key, item in SOURCES.items()}
    for item in SOURCES.values():
        item.setdefault("sourceTrace", TRACE)
        item["license"] = "CC0 1.0 per existing repository evidence; no new licensing claim"
        item["sha256"] = hashlib.sha256((ROOT / item["path"]).read_bytes()).hexdigest()
    receipt = {
        "version": 1, "date": DATE, "title": "El Negativo / Autopsy physical consequences",
        "recipe": str(Path(__file__).resolve().relative_to(ROOT)), "numpyVersion": np.__version__,
        "rights": "Inputs credited CC0 1.0 in existing repository receipts; original DSP/composition is project work. No download, voice or external model/service used.",
        "integration": "Dedicated 3D WorldSFX sources at the visible physical emitter. Runtime owns causal timing, pause/capture cancellation and reduced-dynamics scaling. Existing master/limiter/mono remain authoritative.",
        "sources": SOURCES, "masters": render(),
    }
    (Path(__file__).parent / "masters.json").write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n")

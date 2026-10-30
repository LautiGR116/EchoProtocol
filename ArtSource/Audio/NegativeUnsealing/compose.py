#!/usr/bin/env python3
"""Rebuild four physical, non-vocal Unsealing masters with Python + NumPy.

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
OUT = ROOT / "Assets/_Game/Audio/Clips/NegativeUnsealing"
RATE = 48000
DATE = "2026-10-30"
TRACE = "ArtSource/Audio/Resonance/Sources/download-manifest.json"
SOURCES = {
    "scrape3": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-3.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-3.wav", "url": "https://opengameart.org/content/scrapes"},
    "scrape5": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-5.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-5.wav", "url": "https://opengameart.org/content/scrapes"},
    "scrape8": {"path": "ArtSource/Audio/Resonance/Decoded/scrape-8.wav", "author": "AntumDeluge", "title": "Scrapes / scrape-8.wav", "url": "https://opengameart.org/content/scrapes"},
    "clunk": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-lightclunk2.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / lightclunk2.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "mechanism": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-mechanical1.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / mechanical1.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "squeak": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-squeakyclick2.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / squeakyclick2.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
    "rattle": {"path": "ArtSource/Audio/Resonance/Sources/mechanical-rattle1.wav", "author": "Brian MacIntosh (BMacZero)", "title": "Mechanical sounds / rattle1.wav", "url": "https://opengameart.org/content/mechanical-sounds"},
}


def read_wav(path):
    with wave.open(str(path)) as f:
        rate, channels, width = f.getframerate(), f.getnchannels(), f.getsampwidth()
        if width != 2:
            raise ValueError("Expected credited PCM16 input: " + str(path))
        x = np.frombuffer(f.readframes(f.getnframes()), "<i2").astype(np.float64) / 32768
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
    path = OUT / f"NU_{name}.wav"
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


def render():
    result = []
    x = np.zeros(round(3.1 * RATE))
    drag = envelope(cut("scrape3", .08, 1.32, .48, 95, 3100), [(0, 0), (.25, .18), (.55, .72), (1.05, .34), (1.42, .8), (2.15, .27), (2.58, 0)])
    add(x, drag, .06, .52)
    # Three unequal adhesive releases within one continuous peeling action.
    for at, start, end, speed, gain in [(.32, .16, .38, .73, .26), (.71, .52, .77, .62, .37), (1.28, .89, 1.16, .83, .30)]:
        add(x, cut("scrape5", start, end, speed, 180, 5200), at, gain)
    add(x, envelope(cut("squeak", .1, .47, .55, 220, 1800), [(0, 0), (.12, .55), (.31, .28), (.67, 0)]), 1.76, .055)
    result.append(master("Peel", early_room(x, .06), -12, .18,
        {"scrape3": [[.08, 1.32, .48]], "scrape5": [[.16, .38, .73], [.52, .77, .62], [.89, 1.16, .83]], "squeak": [[.1, .47, .55]]},
        {"firstLoad": .25, "adhesionReleases": [.32, .71, 1.28], "residualFrictionEnd": 2.64},
        "Low coarse material stretches, three irregular tears release and the remaining tack draws away. Recorded scrape grains and low squeaky hardware are edited as a non-vocal membrane surface, with short dry reflections.", .018, .20))

    x = np.zeros(round(2.25 * RATE))
    for at, start, end, speed, gain in [(.08, .02, .11, 1.0, .17), (.23, .09, .19, .83, .24), (.44, .11, .29, .70, .29)]:
        add(x, cut("clunk", start, end, speed, 220, 4300), at, gain)
    add(x, envelope(cut("scrape8", .32, 1.27, .77, 200, 3800), [(0, 0), (.14, .6), (.42, .3), (.69, .8), (1.23, 0)]), .58, .21)
    add(x, envelope(cut("squeak", .07, .38, .54, 240, 2100), [(0, 0), (.09, .7), (.25, .4), (.57, 0)]), .87, .065)
    result.append(master("Tendon", early_room(x, .025), -15, .14,
        {"clunk": [[.02, .11, 1.0], [.09, .19, .83], [.11, .29, .70]], "scrape8": [[.32, 1.27, .77]], "squeak": [[.07, .38, .54]]},
        {"dryContacts": [.08, .23, .44], "drag": [.58, 1.81]},
        "Three close dry fingertip-like contacts followed by one short elastic draw. No anatomic injury recording, voice or exaggerated wet splash; event identity belongs to its visible physical source.", .006, .19))

    x = np.zeros(round(2.2 * RATE))
    add(x, cut("clunk", .01, .25, .8, 130, 3600), .025, .28)
    tension = envelope(cut("mechanism", .015, .89, .80, 80, 2800), [(0, 0), (.08, .45), (.28, .65), (.50, .25), (.78, .7), (1.02, .13), (1.09, 0)])
    add(x, tension, .1, .40)
    for at, start, end, speed, gain in [(.32, .14, .28, .73, .06), (.68, .31, .45, .61, .045), (.96, .46, .63, .75, .055)]:
        add(x, cut("rattle", start, end, speed, 210, 3500), at, gain)
    add(x, cut("clunk", .04, .27, .61, 130, 3600), 1.25, .24)
    add(x, resonance(.55, [83, 171, 309, 721], .11, 303), 1.26, .045)
    result.append(master("Hoist", early_room(x, .18), -12.5, .18,
        {"clunk": [[.01, .25, .8], [.04, .27, .61]], "mechanism": [[.015, .89, .80]], "rattle": [[.14, .28, .73], [.31, .45, .61], [.46, .63, .75]]},
        {"takeLoad": .025, "lowering": [.1, 1.2], "visibleDescent": [.18, 1.25], "brakeContact": 1.25},
        "One load-bearing mechanism: dry catch, slow unstable cable/ratchet tension, then a finite brake contact. Small original damped shell modes seed303 at the brake; no repeated loop or synthetic warning tone.", .004, .20))

    x = np.zeros(round(2.1 * RATE))
    for at, start, end, speed, gain in [(.005, .12, .17, 1.17, .38), (.017, .26, .31, 1.29, .25), (.029, .57, .63, .97, .31), (.047, .91, .98, 1.1, .15)]:
        add(x, cut("scrape8", start, end, speed, 700, 6500), at, gain)
    add(x, cut("clunk", .005, .15, 1.32, 210, 5100), .014, .20)
    add(x, resonance(1.8, [539, 991, 1571, 2389, 3461, 4913], .19, 307), .008, .16)
    for at, start, end, gain in [(.16, .22, .27, .08), (.29, .44, .51, .055), (.51, .9, .98, .035), (.68, 1.19, 1.25, .018)]:
        add(x, cut("scrape5", start, end, 1.14, 900, 6200), at, gain)
    result.append(master("CeramicFracture", early_room(x, .10), -8, .25,
        {"scrape8": [[.12, .17, 1.17], [.26, .31, 1.29], [.57, .63, .97], [.91, .98, 1.1]], "clunk": [[.005, .15, 1.32]], "scrape5": [[.22, .27, 1.14], [.44, .51, 1.14], [.9, .98, 1.14], [1.19, 1.25, 1.14]]},
        {"fractureCluster": [.005, .017, .029, .047], "looseFragments": [.16, .29, .51, .68]},
        "Clustered brittle micro-fractures with a short inharmonic ceramic response seed307 and uneven falling fragments. Low material support under the crack; no bass-drop, high hiss bed or long pure ringing tone.", .002, .22))
    return result


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    INPUTS = {key: read_wav(ROOT / item["path"]) for key, item in SOURCES.items()}
    for item in SOURCES.values():
        item["sourceTrace"] = TRACE
        item["license"] = "CC0 1.0 per existing repository evidence; no new licensing claim"
        item["sha256"] = hashlib.sha256((ROOT / item["path"]).read_bytes()).hexdigest()
    receipt = {
        "version": 1, "date": DATE, "title": "El Negativo / Unsealing physical consequences",
        "recipe": str(Path(__file__).resolve().relative_to(ROOT)), "numpyVersion": np.__version__,
        "rights": "Inputs credited CC0 1.0 in existing repository receipts; original DSP/composition is project work. No download, voice or external model/service used.",
        "integration": "Dedicated 3D WorldSFX sources at the visible physical emitter. Runtime owns causal timing, pause/capture cancellation and reduced-dynamics scaling. Existing master/limiter/mono remain authoritative.",
        "sources": SOURCES, "masters": render(),
    }
    (Path(__file__).parent / "masters.json").write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n")

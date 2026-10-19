"""Generate the deterministic prototype frame/relay cue for VS1 Strong Event 1.

The output is project-authored, mono 48 kHz / 24-bit PCM. It combines a short
relay contact, restrained gasket friction, and decaying metal-frame modes. The
target peak is -16 dBFS so the project's provisional +8 dB master stage retains
headroom when the three authored events play sequentially.
"""

from __future__ import annotations

import hashlib
import math
import random
import struct
import wave
from pathlib import Path


SAMPLE_RATE = 48_000
DURATION_SECONDS = 0.46
TARGET_PEAK_DBFS = -16.0
SEED = 1_408_2026

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parents[2]
OUTPUT_PATH = (
    PROJECT_ROOT
    / "Assets"
    / "_Game"
    / "Audio"
    / "Clips"
    / "VS1"
    / "SFX_VS1_FrameRelayStrain.wav"
)


def smoothstep(edge0: float, edge1: float, value: float) -> float:
    if edge0 == edge1:
        return 0.0
    x = max(0.0, min(1.0, (value - edge0) / (edge1 - edge0)))
    return x * x * (3.0 - 2.0 * x)


def build_samples() -> list[float]:
    rng = random.Random(SEED)
    sample_count = int(round(SAMPLE_RATE * DURATION_SECONDS))
    raw: list[float] = []
    fast_noise = 0.0
    slow_noise = 0.0

    modes = (
        (218.0, 0.26, 0.145, 0.1),
        (397.0, 0.17, 0.120, 1.7),
        (673.0, 0.11, 0.090, 2.4),
        (1049.0, 0.055, 0.060, 0.8),
    )

    for index in range(sample_count):
        time = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        fast_noise += 0.18 * (noise - fast_noise)
        slow_noise += 0.018 * (noise - slow_noise)
        band_noise = fast_noise - slow_noise

        contact = 0.0
        if time < 0.018:
            contact_env = math.exp(-time / 0.0038)
            contact = contact_env * (
                0.56 * band_noise
                + 0.30 * math.sin(2.0 * math.pi * 1_860.0 * time)
            )

        mode_time = max(0.0, time - 0.006)
        metal = 0.0
        if time >= 0.006:
            attack = smoothstep(0.006, 0.012, time)
            for frequency, amplitude, decay, phase in modes:
                metal += (
                    amplitude
                    * attack
                    * math.exp(-mode_time / decay)
                    * math.sin(2.0 * math.pi * frequency * mode_time + phase)
                )

        friction = 0.0
        if 0.035 <= time <= 0.245:
            friction_attack = smoothstep(0.035, 0.075, time)
            friction_release = 1.0 - smoothstep(0.150, 0.245, time)
            friction = 0.11 * band_noise * friction_attack * friction_release

        coil = 0.0
        if time < 0.105:
            coil = (
                0.08
                * math.exp(-time / 0.036)
                * math.sin(2.0 * math.pi * 92.0 * time)
            )

        value = math.tanh((contact + metal + friction + coil) * 1.15)
        fade_in = smoothstep(0.0, 0.0015, time)
        fade_out = 1.0 - smoothstep(DURATION_SECONDS - 0.075, DURATION_SECONDS, time)
        raw.append(value * fade_in * fade_out)

    dc = sum(raw) / len(raw)
    centered = [sample - dc for sample in raw]
    peak = max(abs(sample) for sample in centered)
    target_peak = 10.0 ** (TARGET_PEAK_DBFS / 20.0)
    scale = target_peak / peak if peak > 0.0 else 1.0
    return [max(-1.0, min(1.0, sample * scale)) for sample in centered]


def encode_pcm24(sample: float) -> bytes:
    integer = int(round(sample * ((1 << 23) - 1)))
    if integer < 0:
        integer += 1 << 24
    return struct.pack(
        "<BBB",
        integer & 0xFF,
        (integer >> 8) & 0xFF,
        (integer >> 16) & 0xFF,
    )


def main() -> None:
    samples = build_samples()
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUTPUT_PATH), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(3)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(b"".join(encode_pcm24(sample) for sample in samples))

    digest = hashlib.sha256(OUTPUT_PATH.read_bytes()).hexdigest()
    measured_peak = max(abs(sample) for sample in samples)
    measured_dbfs = 20.0 * math.log10(measured_peak)
    print(
        f"{OUTPUT_PATH}\n"
        f"duration={len(samples) / SAMPLE_RATE:.3f}s channels=1 sample_rate={SAMPLE_RATE} "
        f"bit_depth=24 peak={measured_dbfs:.2f}dBFS sha256={digest}"
    )


if __name__ == "__main__":
    main()

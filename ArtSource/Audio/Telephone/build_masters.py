"""Reproducible recorded-sound editing. NumPy + macOS afconvert, no synthesis.

Run from any directory using the project's bundled Python with NumPy.
No Assets, scenes, runtime scripts, source recordings, or previous releases are
modified. Every master remains a technically prepared listening candidate.
"""
from pathlib import Path
import hashlib, json, subprocess, wave
import numpy as np

ROOT = Path(__file__).resolve().parent
RATE = 48000
ROWS = {r["id"]: r for r in json.loads((ROOT / "sources.json").read_text())}
manifest = []
decoded = {}

def read(identity):
    if identity not in decoded:
        target = ROOT / "Decoded" / (identity + ".wav")
        target.parent.mkdir(exist_ok=True)
        subprocess.run(["/usr/bin/afconvert", str(ROOT / ROWS[identity]["local_file"]), str(target), "-f", "WAVE", "-d", "LEI16@48000"], check=True)
        with wave.open(str(target)) as wav:
            assert wav.getsampwidth() == 2 and wav.getframerate() == RATE
            x = np.frombuffer(wav.readframes(wav.getnframes()), dtype="<i2").astype(np.float64)
            x = x.reshape(-1, wav.getnchannels()).mean(axis=1) / 32768
        decoded[identity] = x
        target.unlink()
    return decoded[identity]

def cut(identity, begin, end):
    return read(identity)[round(begin * RATE):round(end * RATE)].copy()

def fades(x, attack=.006, release=.055):
    n = min(round(attack * RATE), len(x) // 2)
    if n: x[:n] *= np.linspace(0, 1, n)
    n = min(round(release * RATE), len(x) // 2)
    if n: x[-n:] *= np.linspace(1, 0, n)
    return x

def seamless(x, seconds=.24):
    n = min(round(seconds * RATE), len(x) // 4)
    weight = np.linspace(0, 1, n)
    return np.concatenate((x[n:-n], x[-n:] * (1-weight) + x[:n] * weight))

def write(name, identity, begin, end, peak, role, *, loop=False, crossfade=.24, attack=.006, release=.055):
    x = cut(identity, begin, end)
    x -= np.mean(x)
    x = seamless(x, crossfade) if loop else fades(x, attack, release)
    x *= peak / max(float(np.max(np.abs(x))), 1e-12)
    assert np.isfinite(x).all() and np.max(np.abs(x)) < .5
    raw = np.round(x * (2**23-1)).astype(np.int32)
    packed = np.stack([raw & 255, (raw >> 8) & 255, (raw >> 16) & 255], axis=-1).astype(np.uint8)
    path = ROOT / "Masters" / (name + ".wav")
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1); wav.setsampwidth(3); wav.setframerate(RATE)
        wav.writeframes(packed.tobytes())
    differences = np.abs(np.diff(x))
    entry = dict(name=name, file=str(path.relative_to(ROOT)), source_id=identity,
        source_page=ROWS[identity]["source_page"], source_sha256=ROWS[identity]["sha256"],
        license=ROWS[identity]["license"], attribution=ROWS[identity]["attribution"],
        role=role, source_interval_seconds=[begin,end],
        edits=["decode public HQ MP3 preview via afconvert to 48 kHz 16-bit PCM", "arithmetic stereo downmix where applicable", "remove segment DC",
            f"{crossfade}s boundary crossfade" if loop else f"{attack}s attack, {release}s release fades",
            f"linear normalization to {peak} peak", "export mono 48 kHz 24-bit PCM (does not restore MP3 or decode precision)"],
        duration_seconds=len(x)/RATE, loop=loop, peak_linear=float(np.max(np.abs(x))),
        peak_dbfs=float(20*np.log10(np.max(np.abs(x)))), rms_dbfs=float(20*np.log10(np.sqrt(np.mean(x*x)))),
        dc=float(np.mean(x)), clipping_samples=int(np.count_nonzero(np.abs(x)>=1)), finite=bool(np.isfinite(x).all()),
        loop_boundary_jump=float(abs(x[0]-x[-1])), internal_step_p99=float(np.quantile(differences,.99)),
        bytes=path.stat().st_size, sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        date="2026-10-01", status="prepared; not imported; no human listening approval")
    manifest.append(entry)
    print(name, f"{entry['duration_seconds']:.3f}s", f"peak {entry['peak_dbfs']:.1f}dBFS", f"RMS {entry['rms_dbfs']:.1f}dBFS")

if __name__ == "__main__":
    (ROOT / "Masters").mkdir(exist_ok=True)
    write("TelephoneRing", "556499", .12, 4.50, .26, "One actual rotary ring with natural decay; localized ringing every eight seconds before optional answer", attack=.02, release=.20)
    write("HandsetLift", "556496", .31, 2.20, .24, "First contact event in uploader-described pickup then soft hangup recording", attack=.006, release=.15)
    write("HandsetSetDown", "556496", 3.00, 4.70, .23, "Second contact event in uploader-described pickup then soft hangup recording", attack=.006, release=.15)
    (ROOT / "masters.json").write_text(json.dumps(manifest,indent=2)+"\n")

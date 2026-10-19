# Overhaul sound preparation — 2026-10-01

Prepared outside `Assets` for the sole scene integrator. Ten mono 48 kHz / 24-bit
WAV masters total **4,422,536 bytes**, from six CC0 recordings whose public HQ
previews total **1,711,163 bytes**. No service, account, payment, voice or music
generation. Downloaded previews are MP3 derivatives, **not the original lossless
recordings**. WAV conversion does not restore their lost detail.

**Objective status:** all samples finite, no clipping, highest peak −12.4 dBFS;
all three loop joins have a sample jump below their internal-step 99th percentile.
**Listening status:** this session cannot receive audio input. Material,
localization, unwanted recorded content, perceived loudness, emotional effect,
and the suitability of each crop remain unverified by ear. Source descriptions
identify ordinary foley/appliances and no speech or music, but that is indirect
evidence. These are integration candidates, not listening-approved finals.

## Audit of the Finish baseline

The original door movement/endpoint pair has a coherent physical identity and a
human source-selection history. Keep it for household doors. Preserve truthful
acquire/release timing and the RECALL contract. Facility air and fluorescent hum
provide a recognizable baseline; changing every early cue would spend time
without fixing the deeper problems.

Four concrete weaknesses are visible in the source implementation:

1. `FootWood0..2` are filtered copies of the same concrete contacts as vinyl.
   A real material change is inexpensive and perceptible on almost every step.
2. One door endpoint now supplies relays, start/finish confirmation, periodic
   circuit hints and shutter cues. Give physical processes a sustaining sound
   that explains their state; keep a small relay transient only for contacts.
3. The full domestic knock repeats behind the Player after each valve and on the
   chair change. That repetition can teach a predictable task-completion scare.
   Reserve the knock identity for the first doorway encounter. Let valves shed
   pressure and the house bear weight, with distinct causality and material.
4. Later room tone merely scales institutional beds by world Z. The house is
   quieter but does not have a local, physical sonic identity. A localized
   appliance tied to the kitchen power gives darkness an audible consequence.

These are implementation-grounded diagnoses; perceived quality is an inference.
The final telephone/archive synthesized cues are also replaceable, but sourcing
indiscriminately would be less valuable than making present actions believable.

## Integration priorities

| Master | Proposed physical use | Initial source setup / contract |
| --- | --- | --- |
| `FootWood0..2` | Actual Player shoe contacts in existing timber zones | Replace the three timber clips, retain movement/reset timing and existing conservative gain. Never put them on Echo. |
| `ValveFlow_LP` | Working pressure valve | Source at the actual working station, WorldSFX, 3D, gain ~0.14, min distance 1 m / max 7 m; 0.18 s fade in and 0.2 s interruption fade. Active only while valid work is progressing. |
| `ValveDischarge` | A completed valve releases pressure | One play at that valve, gain ~0.2; replace the repeated behind-player domestic knock here. No discharge on cancellation. |
| `WaterInPipe_LP` | Optional visible wet drain / pipe flow | Separate localized detail, gain ~0.10, max distance 6 m. Use only if there is a credible modeled source and state; do not layer automatically over ValveFlow. |
| `DrainRelease` | Optional drain completion | One play from a drain after a verified drain state. Useful for replacing a fictional electronic cue, not mandatory. |
| `DomesticFridge_LP` | Kitchen refrigerator/appliance | Ambience, 3D, min 1.2 m / max 7 m, gain ~0.12; follow actual kitchen power, fade over 0.2 s. Start powered on; cut when answering phone removes current; restore on kitchen circuit. No heartbeat hint. |
| `FloorboardWeight` | Chair/presence consequence | WorldSFX, one authored play, gain ~0.18. Prefer the physical chair's old/new position when it moves. If retained behind Player at disappearance, freeze the emitter at trigger position so rotation remains truthful. |
| `PipeStress` | Loaded frame/pipe | A restrained one-shot from an actual fixture on pressure/state change; gain ~0.16. Avoid using on every interface event. |

The gains above are **starting values**, not measured output or comfort approval.
Use the existing mono and reduced-dynamics paths. Avoid a new mixer framework,
random scheduling, automatic gain boosts or head-locked source movement.

Highest-value first integration: real timber contacts, causal valve loop and
release, the house appliance, and distinct chair material. Three optional files
(`WaterInPipe_LP`, `DrainRelease`, `PipeStress`) can remain outside Assets if the
level has no specific authored use. Do not import a palette merely to use it all.

## Sources and rights

Each current Freesound source page links to
[CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/), which permits
copying, modifying and redistributing the work, including commercial use. The
uploader descriptions are consistent with self-recorded ordinary sounds. This
does not certify ownership of every incidental recorded element. Voluntary
credits and dated sanitized evidence are in `sources.json` and `Evidence/`.

- [Footsteps on Wooden Floor.aif — ftpalad](https://freesound.org/people/ftpalad/sounds/119914/)
- [Creaking Wooden Floor [7] — SoundDesignForYou](https://freesound.org/people/SoundDesignForYou/sounds/646691/)
- [machine-hum.ogg — loopo](https://freesound.org/people/loopo/sounds/164040/)
- [Faucet + sink + drain gurgling — Breviceps](https://freesound.org/people/Breviceps/sounds/458401/)
- [metal creak — Rae_chuuu](https://freesound.org/people/Rae_chuuu/sounds/797978/)
- [gas_leak_3_samples — JohanHauskater](https://freesound.org/people/JohanHauskater/sounds/855703/)

No original master was accessed behind the account login. The public preview URL
was read directly from each page's `data-static-file-url` share attribute.
Only relevant public metadata is retained; raw page HTML is not retained.

## Reproduction and verification

`fetch_sources.py` fetches only those six explicit pages/previews, asserts the
current CC0 deed, limits individual transfers, and writes sanitized metadata and
hashes. `build_masters.py` uses NumPy and the macOS `afconvert` decoder. Source
cuts, edits, durations, DC, RMS/peak, loop joins and SHA-256 are in `masters.json`.
The original downloaded files remain unchanged.

After integration, verify real controls start/stop work sound, reset cancels work
without discharging, kitchen power owns its appliance, sources stay world-locked
through rotation, and no source duplicates through RECALL/Continue. Check both
normal and reduced output paths. Human listening still needs to assess surface
identity, crop artifacts, source distance, mask/dynamic balance and comfort.

## Causal pressure component prepared for integration

`Assets/_Game/Scripts/Audio/PressureWorkAudio.cs` now exists with explicit fields:
`director`, `session`, `flowSource`, `dischargeSource`, `flowLoopClip`,
`dischargeClip`. It owns playback only; bus, distance and spatial configuration
remain on the two dedicated sources. Default gains are 0.14 / 0.20 and both flow
edges take 0.15 s. `LateUpdate` observes final work state. A release needs a
single valve increment following observed valid pressure work; initialization,
enable, Start and chapter restores establish a baseline without release.
`DischargeCount` and `IsFlowActive` expose objective verification. Compilation
and configured-runtime verification belong to the scene integrator and have not
been performed by this audio task.

Additional static audit: the saved Finish scene has `DopplerLevel: 1` on all four
`Campaign cue` sources and `Present Player foot contacts`. The director relocates
its reusable emitters just before playing; the footstep source moves only when
a contact fires, in 1.4 m increments. Their estimated velocity can therefore
change pitch without physical source movement. This is an implementation risk,
not an observed listening defect. Set Doppler to zero on those teleporting
emitters and the new pressure sources in Overhaul; no new framework is needed.

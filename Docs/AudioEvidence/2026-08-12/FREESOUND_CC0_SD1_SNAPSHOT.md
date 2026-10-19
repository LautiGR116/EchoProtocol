# Freesound CC0 evidence snapshot — SD1

Reviewed and downloaded on `2026-08-12`. The originals remain outside the
repository; only edited prototype derivatives are imported. No account,
session, credential, or payment data is retained here.

Each canonical page displayed `Creative Commons 0` and linked to the
[CC0 1.0 deed](https://creativecommons.org/publicdomain/zero/1.0/). The user
approved the initial direction of all three official previews and specifically
requested a mildly distorted treatment for the breathing. After hearing that
treatment, the user rejected it because it read as erotic. Only the knock and
footsteps remain accepted for prototype integration.

| ID | Canonical source and first-party claim | Published/original file | Original SHA-256 | Voluntary credit |
| --- | --- | --- | --- | --- |
| `AUD-SD1-001` | [DOOR KNOCKING](https://freesound.org/people/SamuelGremaud/sounds/426736/) — SamuelGremaud; uploader describes recording the door knocks with a Zoom H4N Pro and licenses their sounds CC0 | `426736__samuelgremaud__door-knocking.wav`; stereo, `48 kHz/32-bit`, `6.279833 s`, `2,411,500` bytes | `06f1b0cb7b04108f9fff8b059daf03140c5fa90e9407d629b6d74079992be9e9` | `DOOR KNOCKING by SamuelGremaud (Freesound), CC0 1.0.` |
| `AUD-SD1-002` | [walking indoors on concrete floor 1](https://freesound.org/people/FOSSarts/sounds/740419/) — FOSSarts; uploader describes recording an adult walking indoors on concrete and states all shared sounds are public-domain/CC0 | `740419__fossarts__walking-indoors-on-concrete-floor-1.wav`; stereo, `96 kHz/32-bit float`, `3.022667 s`, `2,354,176` bytes | `38480376e8a7551cdcba438e0f2bac4a0e624ff0a217a42da8aea1855a0e3619` | `walking indoors on concrete floor 1 by FOSSarts (Freesound), CC0 1.0.` |
| `AUD-SD1-003` | [SFX BREATHING](https://freesound.org/people/LindaBarrans/sounds/800927/) — LindaBarrans; uploader identifies themself as the breathing performer and describes an AT2020+ USB recording | `800927__lindabarrans__sfx-breathing.flac`; mono, `44.1 kHz/16-bit`, `56.006531 s`, `1,410,775` bytes | `0731ee3222b1333635e9276d3670b515a2c8758bbd0a0ba51d939736d2268358` | `SFX BREATHING by LindaBarrans (Freesound), CC0 1.0.` |

The source pages and descriptions are evidence of the uploaders' claims, not an
independent warranty of ownership. The breathing is a nonverbal adult
performance and is treated as such. No source contains the planned whisper.

## Imported derivatives

| Output | Edit and level | SHA-256 |
| --- | --- | --- |
| `Assets/_Game/Audio/Clips/SD1/SFX_SD1_DomesticKnock.wav` | Downmixed; active region retained with `60 ms` pre-roll and `220 ms` tail; `8/60 ms` fades; peak `-9 dBFS`; mono `48 kHz/24-bit`, `5.933729 s` | `f8557e8591c8ea75f2a3b663427895534be7fb881b765fe416330af33e1f868a` |
| `Assets/_Game/Audio/Clips/SD1/SFX_SD1_Footsteps.wav` | Downmixed; complete take; `20/80 ms` fades; peak `-12 dBFS`; mono `48 kHz/24-bit`, `3.022667 s` | `ad9f90abfb15fed441f9899de44b84cc202023317add5334eac41878e39f03b5` |
| Deleted — `SFX_SD1_BreathingTreated.wav` | Source `41.6–50.3 s`; `75 Hz` high-pass and `4.2 kHz` low-pass; quiet `0.987x` double delayed `23 ms` at `22%`; `0.17 Hz` level drift; restrained saturation; `240/380 ms` fades; peak `-14 dBFS`; mono `48 kHz/24-bit`, `8.700 s` | `dc790f768f170ecf93cf64e154b152909fb1d6aebeffa6442137e683b1c507f4`; **rejected and removed** |

Edits used macOS `afconvert`, Python, and NumPy. Unity imports the two retained derivatives as PCM,
`Decompress On Load`, preserve sample rate, preload on, background loading off,
normalization off, and ambisonics off.

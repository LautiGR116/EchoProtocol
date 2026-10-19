# Resonance source trail

2026-10-08. New sound direction for the Lighting-derived Resonance scene.

`Sources/download-manifest.json` records primary author pages, exact downloads,
CC0 declaration, date and hashes. The cry and scream are public lossy HQ MP3
previews, decoded with macOS AudioToolbox. Other new recordings are downloadable
PCM/FLAC. The original sources remain outside Unity's Assets tree. `CC0-1.0.txt`
is the license supplied with AntumDeluge's scrapes; the same license is declared
on all selected primary source pages.

`Tools/compose.py` deterministically constructs thirteen mono 48 kHz/24-bit
masters. `masters.json` names every input, excerpt and modification and records
output hash/level/duration. Recorded human performances keep their original
pitch. Procedural modal tones are original nonverbal music, not generated voices.
The small seeded diffusion impulse is an offline fixed room tail, not a random
runtime event scheduler. All event sequences are authored from real game state.

`Decoded/` includes unused audition candidates as well as selected sources.
The short qubodup vocal grunts and low-level mikeask tired breathing were not
imported. This is a provisional artistic selection with technical evaluation;
no human ear approval is claimed. The earlier rejected LindaBarrans treatment
was not reused.

Editor authoring helpers are saved as `.cs.txt` to avoid shipping tools in the
Player. Scene, object, references and import changes went through Unity APIs.
QA capture/probes are excluded from the delivery build. Actual listener captures
and state/level evidence live in `Docs/AudioEvidence/2026-10-08-Resonance/`.

`Tools/make_listening_samples.py` cuts the final listener recording without
normalization. Individual samples are exact PCM slices; the reel adds only
10 ms edit fades and one-second silent separators. `Tools/package_delivery.py`
verifies the already-built app and ZIP; it refuses to replace an existing ZIP.
`Tools/backup_resonance.py` writes the seventh recovery layer outside the repo,
checks prerequisite hashes and reads back every file across all seven layers.
These are delivery recipes, not runtime dependencies.

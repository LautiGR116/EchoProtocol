# Audio asset ledger

This ledger is the source-of-truth for external audio considered for the first
integrated `Familiarity` proof. The repository is public. A link or a free
download is not permission to redistribute its raw file in Git.

The repository currently has no root project `LICENSE`. This ledger records
permission for individual third-party audio files; it does not grant a license
for the project as a whole.

## AD2 selection status

- Six CC0 recordings were downloaded outside `Assets`, hashed, inspected, and
  auditioned completely. They produced eight edited prototype clips.
- The user chose the domestic `Sliding Door Open` source for both movement
  option A and endpoint option B. This is the strongest creative selection in
  the set: ordinary household material inside an institutional space.
- The other seven outputs were accepted as functional prototype material, not
  as final production identity. The user's assessment was that they were not
  bad, but none was compelling. Replacement remains expected during the full
  sound-design pass.
- The industrial ambience, designed pressure plate, literal cassette rewind,
  clipped switch, and alternate automatic-door/metal-clunk candidates were
  rejected or superseded during audition. Rejection history is retained below.
- `SFX_ChairSettle` remains deferred to AD3 and is not imported by AD2.
- A tenth room-tail clip remains intentionally unsourced.

Freesound explains the licenses used on its platform in its
[official FAQ](https://freesound.org/help/faq/). `CC0` permits broad reuse, but a
license label does not prove that an uploader owned every sound in a recording.
Creative Commons provides a plain-language
[CC0 1.0 deed](https://creativecommons.org/publicdomain/zero/1.0/) and the
separate [CC0 1.0 Legal Code](https://creativecommons.org/publicdomain/zero/1.0/legalcode.en).
Credit will still be recorded voluntarily for every accepted CC0 source.

## Selection record

The published format and description below come from each source page and must
be verified again at download time.

| ID / planned output | Candidate source | Published source | Planned derivative | Status and audition gate |
| --- | --- | --- | --- | --- |
| `AUD-AMB-001`<br>`Assets/_Game/Audio/Clips/Ambience/AMB_FacilityAir_LP.wav` | [House Vent Loop 03](https://freesound.org/people/HECKFRICKER/sounds/753542/) — HECKFRICKER | WAV, stereo, `44.1 kHz/16-bit`, `41.537 s`; household floor vent; CC0 | Stable `24.0 s` loop, `0.25 s` boundary crossfade, resampled to `48 kHz/24-bit` | **Accepted for prototype.** Functional neutral air bed; production identity remains open. |
| `AUD-AMB-002`<br>`Assets/_Game/Audio/Clips/Ambience/AMB_Fluorescent_LP.wav` | [Fluorescent Light Buzzing](https://freesound.org/people/JoelMcDaniel/sounds/830440/) — JoelMcDaniel | WAV, mono, `48 kHz/24-bit`, `15.562 s`; real bulb, Zoom H6/MKE600; CC0 | Clean sections joined with `80 ms` crossfades; `12.85 s` loop and `0.25 s` boundary crossfade | **Accepted for prototype.** Restrained layer; final tone and fixture relationship remain open. |
| `AUD-AMB-003`<br>`Assets/_Game/Audio/Clips/Ambience/AMB_DistantMachinery_LP.wav` | [Machine Hum](https://freesound.org/people/AlaskaRobotics/sounds/221566/) — AlaskaRobotics | WAV, mono, `48 kHz/16-bit`, `14.832 s`; compressor hum; CC0 | `13.8 s` loop with `0.30 s` boundary crossfade, exported `48 kHz/24-bit` | **Accepted replacement for prototype.** The first industrial candidate was too active and eventful. Production identity remains open. |
| `AUD-MEC-001`<br>`Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav` | [Sliding Door Open](https://freesound.org/people/Rolly-SFX/sounds/626165/) — Rolly-SFX | FLAC, stereo, `96 kHz/24-bit`, `3.58 s`; domestic sliding door; CC0 | Downmixed; `2.10 s` loop with `0.15 s` boundary crossfade; `48 kHz/24-bit` | **Accepted — user-selected option A.** The domestic character is intentional. Must survive physical reversal without restart or reverse playback. |
| `AUD-MEC-002`<br>`Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorEndpoint.wav` | Same [Sliding Door Open](https://freesound.org/people/Rolly-SFX/sounds/626165/) source | Same original and license | Downmixed source region `2.74–3.42 s`; `5 ms` in / `40 ms` out; `48 kHz/24-bit` | **Accepted — user-selected option B.** Replaces the metallic-clunk candidate and preserves one coherent domestic object. |
| `AUD-MEC-003/004`<br>`Assets/_Game/Audio/Clips/Mechanisms/SFX_PlatePress.wav`<br>`.../SFX_PlateRelease.wav` | [Lamp Switch On and Off](https://freesound.org/people/se2001/sounds/503135/) — se2001 | WAV, stereo, `44.1 kHz/16-bit`, `5.235 s`; domestic switch; CC0 | Genuine on/off edges downmixed and trimmed to `0.48/0.52 s`; fades `5/20 ms`; `48 kHz/24-bit` | **Accepted replacement for prototype.** Same cues for Player and Echo. Designed pressure-plate candidate lacked a credible release; another switch source clipped. Final identity remains open. |
| `AUD-SYS-001`<br>`Assets/_Game/Audio/Clips/System/SFX_RecallSuccess.wav` | [Stick Whoosh 11 Reverse](https://freesound.org/people/Sadiquecat/sounds/802453/) — Sadiquecat | WAV, mono, `192 kHz/24-bit`, `0.572 s`; recorded stick gesture, already reversed/edited by its creator; CC0 | Trimmed to `0.543 s`, resampled to `48 kHz/24-bit`, fades `10/25 ms` | **Accepted replacement for prototype.** Avoids literal cassette fiction. Final RECALL signature remains open. |
| `AUD-DIS-001`<br>`Assets/_Game/Audio/Clips/Discrepancy/SFX_ChairSettle.wav` | [chairsqueak.wav](https://freesound.org/people/Alexsani/sounds/117289/) — Alexsani | WAV, stereo, `44.1 kHz/16-bit`, `0.893 s`; metal chair squeak after pressure was removed; CC0 | AD3 only | **Deferred.** Downloaded and hashed for later review, but not accepted or imported by AD2. |

`CC0` is the candidate license, not the acceptance decision. Each source can
still fail provenance, timbre, narrative, technical, or safety review.

## Acceptance record

All accepted source pages claimed `CC0 1.0`; commercial use, modification, and
public redistribution are permitted by the Legal Code linked above. Attribution
is not required but the evidence snapshot records a voluntary credit. The dated
evidence file is
`Docs/AudioEvidence/2026-08-09/FREESOUND_CC0_SNAPSHOT.md`, SHA-256
`f25fa06adb22594f141fb8bb303ebd8e29dd17f3ee6172854956ee39168a72bc`.
No speech, music, brands, or identifiable third-party performances were heard in
the complete source auditions. This is a project review, not a warranty.

| ID | Original SHA-256 | Imported derivative | Derivative SHA-256 | Edit and objective review | Owner / bus / decision |
| --- | --- | --- | --- | --- | --- |
| `AUD-AMB-001` | `67a19f1c1b06216b63d04bdb40abaf15d53cfc0a7ae5bd535d952b75a45f8062` | `AMB_FacilityAir_LP.wav`; stereo, `48 kHz/24-bit`, `24.000 s`, `6,916,096` bytes | `a0a6f8c1a3fd15f188a74ca8997d5f859f79f2fc4ab597da63747890ed612b8a` | Source `4.00–28.25 s`; `0.25 s` loop crossfade. Seam jump `0.002970`, internal-step P99 `0.013576`, DC `0.000008`; no clipping or non-finite samples. | `ClinicalRoomTone` / `Ambience`; prototype accepted by project reviewer on `2026-08-09`, replaceable in production. |
| `AUD-AMB-002` | `3c8f293cbfe1e183691e261721d600ff2197c940ef12fe42f90d65dad9252d36` | `AMB_Fluorescent_LP.wav`; mono, `48 kHz/24-bit`, `12.850 s`, `1,854,496` bytes | `b491e4f37f547156eb249e417d97f299e45e2c20453b31a652bb1ccfd8484ca8` | Clean regions joined with `80 ms` crossfades; `0.25 s` loop crossfade. Seam `0.006307`, P99 `0.021191`, DC `0.000030`; no clipping/non-finite samples. | Persistent scene source / `Ambience`; prototype accepted by project reviewer on `2026-08-09`, replaceable in production. |
| `AUD-AMB-003` | `fa362b53294f6ef8a7feec8739ac1d2c11b0248fb9a19a0c18cea34a6de0b85b` | `AMB_DistantMachinery_LP.wav`; mono, `48 kHz/24-bit`, `13.800 s`, `1,991,296` bytes | `4ea9b3098f493511c207156c68aba32e395f2fb76081c3e94fed9035fdfe2956` | Source after `0.4 s`; `0.30 s` loop crossfade. Seam `0.001145`, P99 `0.040460`, DC `0.000063`; no clipping/non-finite samples. | Persistent scene source / `Ambience`; accepted replacement for active industrial candidate, production identity open. |
| `AUD-MEC-001` | `d3e3ec92617e27f4f21205a6a5dbee2b52d4bf6d8e26320d070d7b5142e30d8a` | `SFX_DoorMovement_LP.wav`; mono, `48 kHz/24-bit`, `2.100 s`, `306,496` bytes | `793ae3576c143031f3dac5b9e27709522d82a65711ba174cf7a00af13c7595c0` | Downmix; source from `0.20 s`; `0.15 s` loop crossfade. Seam `0.009369`, P99 `0.025960`, DC `0.000143`; no clipping/non-finite samples. | `SlidingDoorAudio` / `WorldSFX`; **human-selected option A** for its domestic character on `2026-08-09`. |
| `AUD-MEC-002` | same original as `AUD-MEC-001` | `SFX_DoorEndpoint.wav`; mono, `48 kHz/24-bit`, `0.680 s`, `102,016` bytes | `4d2faeb8ef19350711e08e3edd44c4143b5666200e4e509adcc98280dacdf4cb` | Source `2.74–3.42 s`; downmix; `5 ms` fade-in and `40 ms` fade-out; no clipping/non-finite samples. | `SlidingDoorAudio` / `WorldSFX`; **human-selected option B**; metallic-clunk candidate rejected to keep one domestic object. |
| `AUD-MEC-003` | `4c4a3b1e8b7bb4ce34bd406a88d9d7269fdde4f3be8701d7295a583190280a35` | `SFX_PlatePress.wav`; mono, `48 kHz/24-bit`, `0.480 s`, `73,216` bytes | `48a3d21524823fc0571dbdd2fcfba9ca22966e0f93a45669a9f234ad8661e149` | Source `0.90–1.38 s`; downmix; `5/20 ms` fades; no clipping/non-finite samples. | `PressurePlateAudio` / `WorldSFX`; prototype accepted, final identity open. |
| `AUD-MEC-004` | same original as `AUD-MEC-003` | `SFX_PlateRelease.wav`; mono, `48 kHz/24-bit`, `0.520 s`, `78,976` bytes | `af99452667b1220a1f531bb734ff9da3b61c4da82b291b72ab90a05f7df3a2df` | Genuine off edge at source `2.86–3.38 s`; downmix; `5/20 ms` fades; no clipping/non-finite samples. | `PressurePlateAudio` / `WorldSFX`; prototype accepted, never synthesized by reversing press. |
| `AUD-SYS-001` | `0babecc2bb9989e98b12165eb504cb20044ff1b158fa0773fe26b995ee5e9415` | `SFX_RecallSuccess.wav`; mono, `48 kHz/24-bit`, `0.543 s`, `82,288` bytes | `53b76ae65fb1d1a24067749631cfc9a37426d53f16807bfae12a9f7b30e1aadc` | Source `0.015–0.558 s`; `10/25 ms` fades; no clipping/non-finite samples. | `RecallCompletionAudio` / `WorldSFX`; prototype replacement for rejected cassette cue, final RECALL signature open. |

Edits were made deterministically with macOS `afconvert`, Python `3.12.13`, and
NumPy `2.3.5`; no generative service or Student-plan credit was used. Source
downloads totaled about `15 MB`, below the `25 MB` cap. The eight derivatives
total about `10.9 MB`, below the `20 MB` source-control payload cap. Unity GUIDs,
importer state, and final `imported` status were verified in Unity `6000.3.21f1`:

| Asset | Unity GUID | Import state |
| --- | --- | --- |
| `AMB_FacilityAir_LP.wav` | `ecd8ab103edd045a4a9e666ba0f9aa2b` | `Compressed In Memory`, Vorbis `0.70`, preload on |
| `AMB_Fluorescent_LP.wav` | `ab6c64bce0c5b40e4b2cf92ff8852472` | `Compressed In Memory`, Vorbis `0.70`, preload on |
| `AMB_DistantMachinery_LP.wav` | `646cad9ac85db4c80ba43bcd1b36d31c` | `Compressed In Memory`, Vorbis `0.70`, preload on |
| `SFX_DoorMovement_LP.wav` | `917cf0d95815f4781b97251b275e8aaa` | `Decompress On Load`, PCM, preload on |
| `SFX_DoorEndpoint.wav` | `0d390331c7f394300a0228f0696ca7eb` | `Decompress On Load`, PCM, preload on |
| `SFX_PlatePress.wav` | `d2251eab18f644b88a620250731fa358` | `Decompress On Load`, PCM, preload on |
| `SFX_PlateRelease.wav` | `8575199e874d645568fb4da7ece1579e` | `Decompress On Load`, PCM, preload on |
| `SFX_RecallSuccess.wav` | `d45e8652237b4474ebfa96b67aadc250` | `Decompress On Load`, PCM, preload on |

All preserve `48 kHz`; background loading, ambisonics, and forced mono are off.
AD2 scene wiring and an objective runtime smoke test passed on `2026-08-09`:
door reversal did not restart its voice, reset was silent, plate edges were
one-for-one and reset-safe, and five successful RECALLs produced five cues with
one active Echo and no stale plate occupancy. Integrated human listening remains
approved continued prototyping on `2026-08-09`; every imported clip remains
replaceable and no production timbre is locked.

Before a row can move from `candidate` to `accepted`, record all applicable
fields. Use `rejected` with a reason instead of deleting history. Use `replaced`
when an accepted file is superseded, and `imported` only after Unity references
and importer settings are verified.

`Accepted` means that a file passed this project's conservative review. It is
not a warranty of ownership, non-infringement, exclusivity, or worldwide public
domain status. Each third-party file remains governed by its recorded terms and
must be excluded from any future blanket project license unless that license is
explicitly compatible.

- stable ledger ID and state;
- intended final path, Unity GUID, role, bus, owning component, and reason for use;
- original filename, byte size, duration, channel count, sample rate, bit depth,
  and SHA-256;
- creator, performer or recordist, uploader, claimed licensor or rightsholder,
  canonical source URL, publication ID, and download date;
- exact license name and version, canonical legal URL, review date, and explicit
  commercial-use, modification, and public raw-redistribution permissions;
- durable dated evidence of the source metadata and license declaration, plus
  its repository/private evidence path and SHA-256;
- required attribution text and where it will ship, even when the chosen credit
  is voluntary;
- restrictions or identifiable content: voice, music, brands, locations,
  editorial context, subscription terms, territory, `NC`, `ND`, or `SA`;
- original-to-derivative edit recipe, editor and version, final SHA-256, output
  format, peaks, DC/seam observations, and human audition result;
- reviewer, decision date, decision reason, and any replacement relationship.

For AI-generated material also record provider, model and version, generation
date, plan and governing terms, prompt/input rights, human edits, and output
hash. Never put account details, receipts containing personal data, tokens, or
payment information in this public file; use an opaque private evidence ID if a
purchase or grant must be retained elsewhere.

## License gate

For this public repository:

- prefer `CC0 1.0` sources with a credible first-party description;
- consider another permissive license only after its attribution, modification,
  commercial-use, and raw-redistribution obligations are recorded exactly;
- reject `NC`, `ND`, personal-use, editorial-only, unclear ownership, ripped or
  re-uploaded material, and ambiguous labels such as “free”, “royalty-free”, or
  “no copyright” without governing terms;
- do not commit an Asset Store, stock-library, subscription, or Student-plan file
  as a raw asset unless its specific license explicitly permits public source
  redistribution;
- do not use Pixabay or Mixkit as the default source for this slice: the
  [Pixabay Content License](https://pixabay.com/service/license-summary/) and
  [Mixkit terms](https://mixkit.co/terms/) reviewed on `2026-08-09` support many
  end products but create avoidable uncertainty for publishing the raw
  downloadable sound inside a public repository.

This is a conservative production policy, not legal advice.

## Delivery specification

AD2 will produce standardized production masters as WAV PCM, `48 kHz`, signed
`24-bit` integer. Resampling a `44.1 kHz/16-bit` original does not restore lost
detail; this standard only gives the authored derivatives one predictable
delivery format. The project currently uses the system output sample rate and is
not claimed to be locked to `48 kHz`.

| Targets | Unity import plan |
| --- | --- |
| Facility air, fluorescent, distant machinery | `Compressed In Memory`, Vorbis quality `70` as an initial audition setting, preload on, background loading off, ambisonic off. Change only a failing loop to PCM after a seam/warble and DSP-cost A/B test. |
| Door movement/endpoint, plate edges, RECALL, chair | `Decompress On Load`, PCM, preserve sample rate, preload on, background loading off, ambisonic off. |

- Deliver every 3D source as mono; leave Unity `Force To Mono` and automatic
  normalization off. Facility air remains stereo/2D unless audition rejects it.
- Keep Doppler at `0` and pitch at `1`. Mechanism, system, discrepancy, and the
  subtractable air sources use `Play On Awake` off and explicit component
  ownership. The two continuous non-subtractable facility beds may use
  `Play On Awake` as the narrow exception: they loop once from scene load and do
  not restart on RECALL.
- Remove DC, clipping, terminal silence, and boundary clicks. Close loops in the
  waveform with a sample-aligned edit or short crossfade; do not rely on loop
  markers that Unity may ignore.
- Do not normalize every file to `0 dBFS`. Preserve at least `6 dB` of mix
  headroom and a worst-case master peak no higher than `-6 dBFS`.
- Working slice cap: nine target clips, no derivative longer than `30 s`, source
  downloads outside the repository at most `25 MB` total, committed derivative
  WAV/LFS payload at most `20 MB`, and combined imported size at most `10 MB` as
  reported by the AudioClip Inspectors. Measure runtime memory separately in the
  Profiler during AD2. LFS changes transfer behavior, not the stored byte budget.
  A tenth room tail requires an A/B result.
- Use no Streaming or platform override for this small proof. Reconsider only
  after real asset size and profiling justify it.
- Profile audio `DSP CPU` with all three beds and the door active; Vorbis quality
  `70` is a starting point, not an approved value before audition and profiling.
- Git already routes `.wav`, `.ogg`, and `.mp3` through LFS. Check the LFS pointer
  and license record before staging; let Unity create each `.meta` file.

## Runtime ownership and reset contract

- `AMB_FacilityAir_LP` replaces only the runtime-generated clip in
  `ClinicalRoomTone`. It preserves the reveal-owned `0.8 s` fade, stays silent
  after reveal, and remains outside loop reset. Remove generation and cleanup of
  the runtime-owned clip in the same change; never destroy the imported asset.
- `AMB_Fluorescent_LP` and `AMB_DistantMachinery_LP` live on explicit persistent
  scene `AudioSource` hosts. They loop from scene load, remain outside the reset
  list, and continue across RECALL. They do not share the subtractable air
  source or inherit its reveal fade.
- `SlidingDoorAudio` owns serialized movement and endpoint clips. Preserve its
  one continuous voice while the panel physically moves, including direction
  reversal; do not reverse the clip, restart it, or create a false endpoint.
  Keep its silent `ILoopResettable` behavior after `SlidingDoor` in reset order.
- Plate audio reacts only to a real `IsPressed` edge caused by occupancy. The
  existing forced visual refresh during reset must not emit press or release.
  Each `PressurePlateAudio` implements `ILoopResettable`, follows its
  `PressurePlate` immediately in reset order, and only stops its source in
  `ResetLoopState()`; RECALL does not manufacture a release sound.
- Successful RECALL audio subscribes only to `LoopController.LoopCompleted`.
  Invalid `R` remains silent and the source is not a reset target.
- `ChairSettlingCue` owns one serialized chair clip, keeps `HasPlayed`, and stays
  outside loop reset. Replace its generated clip and runtime cleanup atomically;
  scene reload remains the only rearm.

These mappings replace procedurals in place. They do not authorize a global
audio manager, another mixer bus, or a new reset framework.

## Toolchain decision

No new MCP or external application is required for AD1.

| Tool | Observed state | Decision for this proof |
| --- | --- | --- |
| Unity Editor and Unity MCP | Installed and connected | Use in AD2 to import clips, set importer fields, wire serialized references, and inspect the mixer. Do not hand-edit scene or prefab YAML. |
| [Unity AI sound generation](https://docs.unity.com/en-us/ai/credits/credits-about) | A sound-generation capability is exposed, but model discovery did not complete in this session; available credits and any Student allocation were not verified on `2026-08-09` | Do not use in AD2. It remains an optional later exploration for an abstract RECALL identity after explicit consent, Dashboard quota/terms review, and a complete AI ledger record. Prefer credible recorded material for the room, door, and chair. |
| [Unity Student benefits](https://unity.com/products/unity-student) | The public plan reviewed on `2026-08-09` lists Unity Pro, Odin educational access, a Synty asset, Asset Store benefits, and Unity Version Control; it does not advertise a specific Unity AI credit allocation | Useful tools remain available, but none removes license review or improves this AD1 shortlist enough to justify a package import. |
| GarageBand | Installed | Suitable for rough audition and layering. Do not use its bundled loops in a public raw-asset repository unless their exact redistribution terms are cleared and logged. |
| [Audacity](https://www.audacityteam.org/) | Not installed | Preferred lightweight editor for AD2 trimming, downmixing, resampling, loop crossfades, and WAV export. Install only with user approval. |
| `ffmpeg` / SoX | Not installed | Optional later for repeatable metadata and waveform checks; unnecessary before real files exist. No MCP is needed. |
| Blender and [community Blender MCP](https://github.com/MCPBlender/blender-mcp) | Blender is not installed and no Blender MCP is configured | Defer. Blender is valuable for later 3D modelling/animation, not this audio pass. The community bridge adds a local socket and arbitrary Blender-Python execution, so it deserves a separate security/scope review only when a concrete 3D task justifies it. |

Unity's Student entitlement and AI generation are separate questions. Before any
generation, verify the current Dashboard balance and terms; estimated credit
cost is not a guarantee of inclusion. No generation or credit spend occurred in
AD1.

Freesound requires a registered session to download these candidates. If AD2
finds no authenticated session, the user must sign in manually. Automation must
never request, display, store, or commit those credentials.

## Import and commit checklist

1. Re-open the canonical source and license pages on the download date.
2. Download outside `Assets`, compute the original SHA-256, inspect metadata, and
   audition the complete unedited recording.
3. Reject speech, copyrighted music, brands, unexplained source chains, unsafe
   peaks, or a role/timbre mismatch before editing.
4. Export the derivative to its planned path, compute its SHA-256, and complete
   the acceptance record.
5. Import through Unity, verify generated `.meta`, importer values, routing,
   references, loop seams, channel behavior, and Console state.
6. Confirm no code destroys a serialized imported `AudioClip`; remove a
   procedural fallback and its runtime cleanup atomically when replacing it.
7. Exercise mute, mono, reset, five consecutive RECALLs, door reversal, and at
   least `60 s` per loop before calling the asset integrated.
8. Before staging, verify file size, Git LFS status, public license evidence,
   attribution, secret scan, and `git diff --check`. A clean-clone check follows
   the focused audio commit.

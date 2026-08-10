# Audio design

This document translates the product direction in
[`Design/Echo Protocol - Game Design Document v0.1.pdf`](Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.1.pdf)
into a small, testable audio plan. Repository behavior and playtest evidence
remain authoritative for what is implemented and validated.

## Current evidence

- **Human-approved:** the M6.1b clinical hum is audible as a stable baseline and
  its fade at the chair reveal works as intended.
- **Human-approved greybox rule:** M6.2 separates `Ambience` and `WorldSFX` and
  gives the active sliding door one deterministic 3D mechanism sound. Its
  movement-to-sound timing is approved; its procedural timbre was judged
  synthetic and remains a placeholder.
- **Objectively verified, not emotionally approved:** M6.3 plays one chair-owned
  3D cue on first Player proximity and never repeats during that session. A
  guided pass confirmed delivery but did not separately validate localization
  and produced no terror or discomfort.
- **AD2 human-approved at prototype level:**
  three CC0 facility layers, the user-selected domestic door A/B pair, truthful
  plate edges, and a successful-RECALL cue are imported and wired. Five-RECALL,
  reset, edge, and reversal checks pass. Integrated listening approved continued
  development, but every imported timbre remains explicitly replaceable.
- **Approved product direction, not implemented:** sound carries space, timing,
  presence, memory, and anomaly across five chapters. Music and loud peaks stay
  scarce.

These prototypes prove delivery, routing, and causal timing. They do not prove
that the room has a believable sonic identity or that it creates horror. The
next checkpoint is therefore a coherent soundscape, not another isolated cue or
more volume.

## Creative thesis

Audio is the architecture of memory:

1. Build a clinical world whose layers, causes, and positions become familiar.
2. Let the Player learn that world without explicitly studying it.
3. Create doubt by removing a learned layer, moving memory into an impossible
   context, or letting two incompatible spaces coexist.
4. Keep every mechanical sound and completed Echo recording truthful.

The product arc `useful Echo → unreliable Echo → feared Echo` describes the
Player's changing interpretation. It does not authorize arbitrary playback
corruption. Echoes remember correctly; the protagonist does not.

## Trust contract

- Doors, plates, goals, RECALL, Player actions, and Echo actions never lie about
  physical state.
- The same normal cause produces the same timing, position, pitch, and level
  until an authored and tested change explicitly replaces that rule.
- If audible actions are later recorded, the Echo must reproduce their original
  timestamp, position, surface, and chosen variation.
- An anomaly may subtract context, introduce memory, or create spatial doubt. It
  cannot impersonate a working mechanism or imply random Echo corruption.
- Every puzzle remains solvable and every essential narrative fact remains
  available without relying on hearing alone.

## Sonic layers

| Layer | Purpose | Rule | Current state |
| --- | --- | --- | --- |
| Functional truth | Player, Echo, RECALL, plates, doors, and goals | Causal, deterministic, spatially legible | Door, both plate edges, and successful RECALL implemented; final timbres remain open except domestic door direction |
| Facility body | Air, fluorescents, electricity, pipes, and distant machinery | Establish normality and geography before horror | Air, fluorescent, and distant machinery prototype layers implemented |
| Human memory | Wood, textile, domestic objects, melody, and voice | Invade gradually without explaining themselves | Planned; exact content TBD |
| Discrepancy | Absence, impossible context, uncertain source, or unexpected proximity | Authored, sparse, and mechanically inert | Hum subtraction and chair cue proofs |
| Music | Transition, recollection, and emotional resolution | Sparse; never a continuous danger announcement | Planned; no score implemented |
| Peak and aftermath | One earned strong event and its recovery | Rare, narratively caused, safe, and followed by reduced density | Planned; none implemented |

Room beds and music are normally 2D. Physical causes are normally 3D and fixed
to their source. Ambiguity should be authored through composition and placement
before adding generic occlusion, reverb, or adaptive-audio systems.

## Five-chapter audio grammar

| Chapter | Facility and space | Memory and music | Silence and peaks |
| --- | --- | --- | --- |
| `Protocol` | Stable, dry, clinical, and fully causal | No narrative score | No dramatic silence or loud peak |
| `Familiarity` | One known layer disappears; one quiet domestic detail enters | No evident score yet | Perceptual quiet, not total mute; no peak |
| `Recollection` | Domestic acoustics begin to invade the installation | First short fragment of a memory motif | Constructed spatial ambiguity |
| `Denial` | Facility and home coexist incompatibly while mechanisms remain exact | Motif becomes recognizable but stays sparse | First strong peak may occur after preparation |
| `Acceptance` | Both worlds converge, then lose density | Motif resolves once | Final peak or collapse; true silence may support the cut to black |

The motif's notes, instrument, owner, and meaning; the guiding voice; and the
content of both possible peaks remain TBD.

## Unit of tension

Use this sequence for an authored audio moment:

`normality → preparation → concentration → subtraction or contradiction → payoff or no-payoff → recovery`

- Normality must repeat long enough to become an unconscious reference.
- A false positive still needs setup and context; it is not a random noise.
- Routine RECALL, puzzle failure, pause, and an arbitrary timer are not scare
  triggers.
- Recovery matters. Do not stack a second event merely because the first one was
  quiet or missed.

### Silence

Most silence is perceptual: a familiar band or source disappears while quiet
room detail and truthful mechanisms remain. Digital zero is reserved for very
few narratively meaningful moments. The current hum fade proves subtraction,
but a production soundscape must leave enough residual world detail for the
Player to distinguish intention from a broken audio system.

### Strong peaks

- Working maximum for the complete 30–45 minute game: two, widely separated.
- The first soundscape slice contains no strong peak. A later variant may test
  one only after normality, subtraction, and recovery work on their own.
- A peak needs explicit narrative cause, several seconds of preparation, a
  brief event, and a clear aftermath.
- Working contrast ceiling is roughly `10–12 dB` over the local context, with at
  least `6 dB` of master headroom, a worst-case master peak no higher than
  `-6 dBFS`, and no clipping. These digital values do not prove physical
  listening safety.
- If an event works only by increasing volume, reject it.

Pain, tinnitus, blocked-ear sensation, dizziness, panic, or sustained physical
distress immediately fails a test. Emotional discomfort is intended; physical
harm is not.

## Music, voice, and RECALL

- Ambient sound carries the early experience. Continuous horror music would
  flatten tension and reveal authorial intent too early.
- `Protocol` and the first `Familiarity` proof use no narrative score. A tonal
  machine bed is still ambience, not music.
- A short recurring memory motif may begin in `Recollection`, grow clearer in
  `Denial`, and resolve once in `Acceptance`. Its source may cross the diegetic
  boundary, but that choice is TBD.
- Voice begins professional and may become intimate later. Identity, casting,
  line count, language pipeline, and processing remain TBD.
- RECALL needs an unmistakable audiovisual signature before production, but its
  exact sound and any recorded relationship to Echo actions remain TBD.
- Music or voice must never claim that an exact Echo is mechanically wrong.

## First integrated audio proof

Build a `Familiarity` audio foundation in the existing M5.2/M6 room without
adding new gameplay. It validates normality, subtraction, and recovery only; it
does not validate the complete chapter or its future domestic-memory intrusion.
Target one first-session route of roughly 5–8 minutes; the times below are
pacing targets, not global timers.

| Phase | Target | Audio contract |
| --- | --- | --- |
| `0–2 min` — normality | Enter and inspect the room | Three modest facility layers establish a believable body; the door uses credible material, friction, and endpoints; no music or anomaly |
| `2–4 min` — learning | Record, RECALL, and solve `A → B` | Plate edges, successful RECALL, door motion, positions, and Echo timing stay exact; failed RECALL remains silent; the room develops a recognizable rhythm |
| `4–5 min` — subtraction | Complete the goal and reveal the chair | One learned facility layer fades; no stinger; residual ambience and the door prove that audio still works |
| `5–7 min` — investigation | Notice and inspect the changed area | One realistic chair/material cue occurs at most once; no visual movement or gameplay consequence |
| `7–8 min` — recovery | Continue or repeat a normal action | No second scare; truthful mechanisms return to the foreground and the discrepancy remains unexplained |

For the chair cue, `approach` versus `departure after close inspection` remains
**EN PRUEBA**. The current build uses approach. The preferred next variant is a
small explicit state sequence — Player enters close range, then leaves a wider
range, then the cue is spent — because it can place the sound behind attention
without using random timing. Do not combine both variants in one pass.

Evaluate the proof in controlled variants:

1. `A`: believable normal soundscape plus subtraction.
2. `B`: `A` plus one chair timing variant.

A later post-`AD5` experiment may add one authored strong peak and recovery. It
is not a variant of the first integrated proof.

## First-pass asset budget

Limit the proof to roughly 8–10 new or replacement audio clips. The list below
describes roles, not separate runtime `AudioSource` components:

- ventilation or air;
- fluorescent or electrical texture;
- distant structure, pipe, or machinery;
- door material and movement;
- door endpoint or latch;
- optional door creak integrated into the same causal movement;
- pressure-plate press and release, reused identically for Player and Echo;
- one brief signature for successful RECALL only;
- one realistic chair/material cue;
- one reusable room tail only if it materially improves cohesion.

Prefer modular reuse, subtraction, and placement over a large sound package. Log
source, author, license, file, edit, and intended use before importing any asset.
Student-plan or AI tools may support a concrete reference or placeholder brief,
but access does not remove credit, licensing, quality, or provenance checks.

AD2 reviewed the complete source recordings and their CC0 declarations, retained
sanitized evidence and original/derivative hashes, rejected the active industrial
bed and literal cassette reference, and imported eight edited clips documented
in [`AUDIO_ASSET_LEDGER.md`](AUDIO_ASSET_LEDGER.md). The plate pair uses genuine
on/off edges rather than a reversed press. The domestic door movement A and
endpoint B are human-selected; other clips remain prototype-only pending the
integrated listening checkpoint and later production replacement.

Footsteps, breathing, voice, and music stay outside this first implementation
until their temporal and narrative contracts are defined. The RECALL cue in this
proof is only truthful loop-completion feedback, not its locked final production
identity. Procedural M6 clips remain placeholders until deliberately replaced.

## Mix and accessibility

- The implemented mixer has `Master`, `Ambience`, and `WorldSFX`. Add `Music`
  or `Voice` only when concrete content requires independent control.
- Near the intended route, a normal mechanism may sit roughly `3–9 dB` above
  the ambience that masks it, without becoming a global cue.
- Preserve at least `6 dB` of headroom and a worst-case master peak no higher
  than `-6 dBFS`. Test stereo headphones, stereo speakers, and mono before
  claiming spatial clarity.
- Planned product controls include separate ambience/effects levels, reduced
  sudden-sound or low-dynamic-range options, and visual or textual equivalents
  for meaningful non-speech cues.
- Do not change `AudioListener`, `Time.timeScale`, puzzle state, input, recording,
  or playback to produce fear.

## Implementation boundaries

- Keep one active `AudioListener`, the existing three mixer groups, small
  scene-local components, and explicit serialized references.
- Functional sound resets with its physical mechanism. Normal ambience continues
  across RECALL. The current M6.1/M6.3 discrepancy persists outside loop reset
  and returns only on scene reload; this is not a universal rule for future
  anomalies.
- Replace procedural clips in place; do not layer production versions over their
  placeholders. Imported serialized clips are assets and must never be destroyed
  by runtime cleanup intended for generated clips.
- Add a plate cue from the plate's pressed-state edge and use the same rule for
  Player and Echo. Add RECALL feedback only after a successful `LoopCompleted`;
  an invalid `R` press remains silent.
- Loop reset stops functional voices and snaps the known baseline silently. The
  plate's forced visual refresh is not an audible release edge. Concrete clip
  ownership and replacement rules live in
  [`AUDIO_ASSET_LEDGER.md`](AUDIO_ASSET_LEDGER.md).
- Do not introduce new buses, global managers, random schedulers, middleware,
  pooling, dynamic occlusion, reverb frameworks, or adaptive music for this
  proof.

## Verification gates

### Objective

- A clean first route lands near 5–8 minutes; puzzle delay is recorded separately
  from audio pacing.
- Every physical source remains explicitly attached to its authored cause and
  expected position; no duplicate source or voice appears.
- The fade changes only its intended ambience source; `WorldSFX`, the listener,
  and the door continue to work.
- Invalid RECALL, wrong puzzle order, pre-reveal exploration, and later RECALLs
  do not trigger the discrepancy.
- Five consecutive resets produce one latest Echo, no stale puzzle occupants,
  no duplicated/restarted/orphaned voices, and no project-authored Console error.
- Echo frame data, timestamps, runtime path, and completion behavior remain
  unchanged with the audio proof enabled or disabled. Puzzle truth and route
  timing remain within their existing M3–M6 acceptance contracts.
- The mix has no clipping and retains the planned digital headroom.
- The complete puzzle remains readable and solvable in mute. Mono, stereo
  headphones, stereo speakers, and the manual reduced-intensity preset are each
  exercised before claiming compatibility.

### Human

Give only a general warning about psychological horror and sudden audio, provide
a manual reduced-intensity test preset or build, and do not reveal exact timing.
This does not require production settings UI. Record device, conservative system
volume, mix preset, and prior familiarity. Stop immediately for physical symptoms.

Ask open questions first: what changed, which sounds had clear causes, where a
sound came from, what the Player expected after the subtraction, whether anything
felt random or broken, and whether the Echo or door stopped obeying its rules.
Then record tension, intention, mechanical trust, cheapness, and physical comfort
separately.

A guided pass verifies delivery. It cannot validate spontaneous discovery or
surprise. Each surprise/timing variant requires a fresh or demonstrably unprimed
participant for that claim. Working approval gates are mechanical trust at least
`4/5`, intentional/environmental reading at least `4/5`, bug or Echo-corruption
reading at most `1/5`, tension increasing at least `2/10`, and zero physical
symptoms. Do not claim horror approval, safe loudness, mono compatibility,
accessibility, or full-game habituation without direct evidence for each claim.

## Delivery sequence

1. `AD0` — integrated direction approved and documented.
2. `AD1` — candidate palette, source/license ledger, delivery specification,
   and toolchain review documented.
3. `AD2` — normal facility prototype, domestic door treatment, plate edges, and
   successful-RECALL feedback implemented, objectively verified, and accepted
   for continued prototyping; no production timbre is locked.
4. `AD3` — assemble the 5–8 minute `Familiarity` audio foundation and one
   chair-timing variant.
5. `AD4` — run objective regression and an unannounced emotional/attribution
   checkpoint.
6. `AD5` — adjust timbre, timing, density, and mix before adding another anomaly
   or narrative content.

## Open decisions

- Whether any prototype-only AD2 clip besides the domestic door pair deserves
  production retention after integrated listening.
- Chair cue on approach or departure after inspection.
- Temporal contracts for footsteps, breathing, interactions, and Echo audio.
- Production RECALL signature.
- Memory motif notes, instrument, owner, and diegetic boundary.
- Guiding voice identity, casting, language, and treatment.
- Concrete content and placement of the one or two possible strong peaks.
- Duration and narrative meaning of true silence at the final cut.

## Deferred from this proof

No new gameplay, puzzle, scene layout, Echo rule, voice, music system, footstep
system, audio middleware, global audio manager, scare scheduler, random event
director, emitter pool, dynamic occlusion, reverb framework, or adaptive score.

# Audio design

This document translates the product direction in
[`Design/Echo Protocol - Game Design Document v0.2.pdf`](Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.2.pdf)
into a testable sonic language. Repository behavior and playtest evidence remain
authoritative for what is implemented and validated.

## Current evidence

- **Human-approved:** the M6.1b facility-air layer and its reveal fade.
- **Human-approved causal rule:** M6.2 door audio follows physical movement. AD2
  replaced the synthetic motor with a user-selected domestic movement/endpoint
  pair and added facility beds, truthful plate edges, and successful-RECALL
  feedback. The integrated palette is approved for continued prototyping, but
  no production timbre is locked.
- **Objectively verified, not emotionally approved:** the M6.3 chair cue plays
  once after reveal. A guided pass proved delivery but produced no terror or
  discomfort and did not validate spontaneous localization.
- **Human-approved visual context:** AV1 provides a closed clinical prototype
  space without changing the audio or gameplay contract.
- **Approved direction, not implemented:** audio becomes the primary horror
  weapon through recurring families, authored variants, spatial doubt, domestic
  intrusion, strong events, and recovery.

The chair is now a secondary proof or motif anchor. The previous `AD3` plan to
stretch it into a 5–8 minute route is superseded. The next milestone is `SD1`, a
focused directional-horror study.

## Creative thesis

Audio is the architecture of memory and the main instrument of fear:

1. Build a coherent facility whose causes, layers, and positions become familiar.
2. Let the Player trust physical sound and exact Echo playback.
3. Recur to a small set of motifs with authored changes in direction, distance,
   material, duration, intensity, and completeness.
4. Make home, presence, and remembered time intrude into the virtual space and
   occasionally seem to cross into the Player's real surroundings.
5. Alternate preparation, impact, uncertain checks, consequences, and recovery.

This is not constant noise, a random scare scheduler, or permission to corrupt
the Echo. Several strong events can occur across 30–45 minutes, but they must be
earned, meaningfully different, and safe.

## Mechanical trust contract

- Doors, plates, goals, RECALL, Player actions, and Echo actions never lie about
  physical state.
- The same functional cause keeps its verified timing and position. Production
  timbre may replace a prototype, but not its causal contract.
- Variants belong to authored horror motifs or explicitly defined scene states,
  not random mechanism feedback.
- If audible Player actions are later replayed, their temporal and spatial
  relationship to the Echo must be specified and verified.
- An anomaly may subtract context, introduce memory, or create spatial doubt. It
  cannot imply that an otherwise identical Echo recording changed arbitrarily.
- Every puzzle and essential narrative fact remains understandable without
  relying on hearing alone.

## Sonic families

| Family | Material | Dramatic use | Current state |
| --- | --- | --- | --- |
| Functional truth | door, plates, RECALL, future interactions | Teach exact causality and prove the system still works | Door, plate edges, and successful RECALL implemented; timbres replaceable |
| Facility / structure | air, fluorescents, ducts, metal, machinery, pressure, glass | Establish the building, distance, and off-route activity | Three prototype ambience layers implemented |
| Presence / body | footsteps, breath, fabric, fingers, weight, brief running | Imply a nearby body without confirming an enemy | Planned |
| Domestic memory | household door, chair, dishes, water, toy, child melody, accident fragments | Make home invade the institution | Domestic door character and chair proof only; broader family planned |
| Time / repetition | repeated footsteps, prior actions elsewhere, an Echo path that sonically continues | Expose omitted or misremembered cycles while playback stays correct | Planned |
| Music / voice | memory motif, guiding voice, intimate fragments | Carry recollection, identity, grief, and resolution | Planned; exact content TBD |

## Authored variation

A recurring motif should not replay one identical file until it becomes harmless.
Build a deliberately limited family of variations along useful axes:

- close, medium, and distant;
- left, right, behind, above, or moving through space;
- dry and domestic versus reverberant and institutional;
- brief, sustained, approaching, departing, or abruptly interrupted;
- restrained, forceful, partial, and complete.

Selection follows narrative state and composition. Do not use unconstrained RNG
or a generic director to make timing unpredictable. Repetition should teach an
identity; variation should change the Player's interpretation of that identity.

## Spatial grammar

### World-locked

World-locked sounds are the default. Their emitter occupies an authored position,
and the stereo image changes correctly when the Player rotates. Doors,
mechanisms, footsteps in the room, distant impacts, and most breathing belong
here. Trustworthy spatial behavior is required before it can create doubt.

### Head-locked or internal

A rare intimate event may stay near one ear or inside the listening perspective
when the view turns. This is an exception for whispers, breath, or fragmented
memory, never a normal physical source and never the only carrier of essential
information. The exact HRTF, spatializer, headphone rendering, and speaker
fallback remain TBD; no middleware or global audio framework is approved yet.

### Trans-screen intent

Some dry domestic sounds should plausibly belong to the Player's home: a knock
behind-right, weight at a household door, movement near a corner, or breath close
to one shoulder. Avoid UI reinforcement, camera shake, or exaggerated game-like
reverb when the purpose is source doubt.

The desired evidence is a spontaneous turn, check of a real door or corner,
removal of one earcup to verify the source, or uncertainty about whether the
sound was inside the game. The mix must never instruct the Player to raise volume.

## Tension and strong events

Use the authored sequence:

`normality → preparation → concentration → contradiction or approach → impact or no-payoff → check → consequence or uncertainty → recovery`

- Quiet construction and strong impact are complementary, not opposing schools.
- Multiple strong events may appear across the complete game; the exact count and
  placement remain TBD. Only a smaller subset should function as chapter-scale
  peaks.
- Strong does not mean maximally loud. Use fast attack, spectral contrast,
  proximity, direction, motion, interruption, and removal of masking layers.
- A check may yield nothing or later reveal a mark, object, route, or remembered
  fragment. Vary consequences so the Player cannot safely ignore every sound or
  predict a reward after each one.
- Routine RECALL, puzzle failure, pause, and arbitrary timers are not automatic
  scare triggers.
- Recovery is authored space, not unused time.

## Five-chapter audio progression

| Chapter | Facility and space | Presence, memory, and music | Pressure |
| --- | --- | --- | --- |
| `Protocol` | Stable, dry, causal facility | Minimal personal material | Safety and first uneasy impact |
| `Familiarity` | Directional checks in known space | Domestic detail enters without explanation | Spatial doubt and small consequences |
| `Recollection` | Home and institution overlap | Presence motifs and first memory fragment | Recurrent variants and uncertain checks |
| `Denial` | Incompatible spaces coexist while mechanisms stay exact | Time/repetition exposes omitted experience | Sustained pressure and authored set pieces |
| `Acceptance` | Facility and memory converge, then lose density | Motif resolves with the remembered truth | Emotional peak, aftermath, and cut to black |

Exact motifs, chapter timing, event density, voice identity, and score language
remain TBD.

## Current planned study: SD1

`SD1` is an approximately five-minute study in the existing AV1 room. It adds no
new puzzle and changes no Echo rule:

1. stable facility baseline;
2. one dry domestic knock behind-right, clearly distinct from the sliding door;
3. authored footsteps travelling left to right and behind;
4. world-locked breathing whose position remains coherent as the Player turns;
5. one exceptional, intelligible head-locked whisper near a single ear;
6. one subtle delayed visual consequence associated with a cue;
7. recovery with truthful mechanisms still available.

The brief is approved direction, not implementation. The whisper line is
`Can you hear me?`, spoken in English and subtitled `¿Me escuchás?` in Spanish.
Its performer, identity, treatment, clip, trigger, intended ear, and exact timing
remain TBD. The visual consequence must be subtle and mechanically inert. Exact
positions, consequence, render technology, and levels also remain TBD. New
assets require the same source, license, hash, full-audition, and
public-repository review used by AD2. `AUDIO_ASSET_LEDGER.md` contains no
accepted SD1 source yet.

## Mix, safety, and accessibility

Recommended player-facing copy:

> Designed for headphones. Set a comfortable volume; details do not require high
> volume.

- Preserve at least `6 dB` of headroom and target a worst-case master peak no
  higher than `-6 dBFS` while prototyping. Digital peaks do not prove physical
  listening safety.
- Build impact through composition and dynamics, never through a request for high
  system volume.
- Plan `Headphones` and `Speakers` presentations, left/right calibration,
  separate ambience/effects/voice controls, and `Reduced Dynamics`. These are
  product requirements, not currently implemented settings.
- Exercise stereo headphones, stereo speakers, mono compatibility, and reduced
  dynamics before claiming support.
- Stop a test for pain, tinnitus, blocked-ear sensation, dizziness, panic, or
  other physical distress.
- Follow current safe-listening guidance from
  [WHO](https://www.who.int/news-room/questions-and-answers/item/deafness-and-hearing-loss-safe-listening)
  and the [WHO/ITU gaming standard](https://www.itu.int/hub/2025/03/new-who-itu-standard-aims-to-prevent-hearing-loss-among-gamers/);
  implementation details should be checked against
  [ITU-T H.872](https://www.itu.int/epublications/publication/itu-t-h-872-2024-10-safe-listening-for-video-gameplay-and-esports).

## Implementation boundaries

- Keep the implemented `Master`, `Ambience`, and `WorldSFX` routing and one active
  `AudioListener` until a concrete feature requires more.
- Functional sound resets with its mechanism; normal ambience continues across
  RECALL. Existing M6 discrepancy state persists until scene reload, but that is
  not a universal anomaly rule.
- Imported assets are never destroyed by runtime cleanup. Replacement remains
  atomic and preserves owner, event, reset, and routing contracts.
- Do not add a generic audio manager, scare scheduler, random director, emitter
  pool, dynamic occlusion framework, reverb framework, adaptive score, or paid
  middleware before SD1 proves a concrete need.
- Do not change `AudioListener`, input, time scale, recording, playback, puzzle
  state, or collision to manufacture fear.

## SD1 verification gates

### Objective

- World-locked sources remain in their authored location through Player rotation;
  the exceptional head-locked cue remains perceptually near its intended ear.
- Each authored event fires only from its explicit state and never duplicates or
  accumulates through five RECALLs.
- Echo frames, timestamps, path, end behavior, puzzle truth, reset order, and
  functional-audio causality remain unchanged.
- The visual consequence changes no collider, path, interaction, or puzzle state.
- The mix has no clipping, retains headroom, and is exercised through headphones,
  speakers, mono, and reduced dynamics at comfortable system levels.
- The experience remains completable in mute and produces no project-authored
  Console errors.

### Human

Warn generally about psychological horror and sudden audio without revealing
timing. Record device, comfortable system volume, presentation mode, prior
familiarity, and physical comfort.

Observe before asking: turns toward virtual or real space, checks of a door or
corner, removal of an earcup, hesitation, and changes in movement. Then ask what
was heard, where it came from, whether it seemed inside or outside the game,
whether it felt authored or broken, whether the Echo stayed trustworthy, and
what produced tension. A guided pass validates delivery only.

Do not claim trans-screen fear, localization, safe loudness, speaker support,
mono support, reduced-dynamics support, or accessibility without direct evidence
for each claim.

## Delivery sequence

1. `AD0–AD2` — direction, legally reviewed prototype palette, integration, and
   human prototype listening complete.
2. `AV1` — clinical visual place objectively verified and human-approved for
   continued prototyping.
3. `SD1` — current planned directional-horror study and perceptual checkpoint.
4. Horror map — define the 30–45 minute escalation and each future puzzle's
   vulnerability before producing more rooms.
5. Later audio integration — replace prototype timbres, expand approved motif
   families, and design chapter set pieces only from validated SD1 evidence.

The old chair-centred `AD3` route is superseded, not implemented. Its useful
normal-audio work remains in AD2; the chair cue can be retained or replaced later
without controlling the roadmap.

## Open decisions

- Exact SD1 clips, triggers, locations, whisper casting/treatment, and subtle
  visual consequence.
- World-locked versus head-locked rendering technology and speaker fallback.
- Temporal contracts for Player/Echo footsteps, breathing, interactions, and
  repeated-action audio.
- Production RECALL signature and final functional timbres.
- Motif palette, memory melody, voice identity, casting, language, and treatment.
- Number, placement, preparation, and aftermath of strong events.
- Exact `Reduced Dynamics` behavior and calibration UX.
- Full-game room, puzzle, and recovery map.

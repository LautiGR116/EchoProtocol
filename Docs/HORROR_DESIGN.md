# Horror design tracker

## Current stage

The mechanical trust foundation is complete through M5.2. M6.1–M6.3, AD2, and
AV1 prove that the project can reveal a persistent object, subtract ambience,
play truthful spatial mechanism audio, deliver a one-shot cue, and present a
closed clinical room without changing Echo playback or puzzle rules.

Human evidence is narrower. The hum transition, causal door audio, integrated
AD2 palette, and AV1 place are approved for continued prototyping. The chair was
noticed and its one-shot delivery worked, but neither created terror or
discomfort. The chair therefore remains a secondary environmental detail or
motif anchor, not a headline scare and not evidence that the horror direction
works.

The previous `AD3` plan to stretch that room into a 5–8 minute chair-centred
route is superseded before implementation. The current planned milestone is
`SD1`: a short directional-horror study that tests whether sound can make the
real Player turn, investigate, and doubt the boundary between the game space and
their physical surroundings. No new puzzle or Echo rule belongs to this study.

## Approved horror thesis

Echoes remember what happened correctly; the protagonist does not. Apparent
unreliability belongs to memory, perception, omitted cycles, or nightmare logic,
never arbitrary playback corruption.

The rule is now confirmed as absolute for recorded data: an Echo never fabricates
or alters a captured frame or supported action. It is not omniscient; the current
prototype captures movement and body yaw only. New semantic or audiovisual
channels require explicit implementation and tests. A later omitted cycle may be
authored as something the protagonist lived but did not retain, provided the
recording itself remains ground truth rather than a changed copy of a known run.

Horror must happen perceptually to the Player, not only symbolically to the
protagonist. An ordinary chair with narrative significance is still just a chair
unless its setup, timing, sensory treatment, and consequence change how the
Player behaves or interprets the space.

The target is not constant noise or a catalogue of random jumpscares. The game
builds a reliable world, teaches recurring motifs, varies them deliberately,
forces uncertain checks, sometimes leaves evidence, and uses several authored
strong events across the full 30–45 minutes. Direction, proximity, movement,
attack, spectral contrast, subtraction, and recovery create force; unsafe
loudness does not.

Sound is the main weapon, not the only one. Level composition, lighting,
materials, reflections, partial human evidence, changed routes, and narrative
context provide checks and consequences. The Echo can remain visually and
mechanically identical while a later environment makes its exact path newly
disturbing.

## Trust contract

- An Echo exactly reproduces a completed recording and ends predictably.
- Reset returns the active experiment to a known baseline.
- Input, playback speed, plate truth, door timing, and goal eligibility do not
  change secretly for an otherwise identical attempt.
- Functional sound remains causally truthful. Horror audio cannot impersonate a
  working mechanism in a way that invalidates a learned puzzle rule.
- Essential puzzle and narrative information must remain available without
  hearing alone.

The perceived arc is `useful Echo → apparently unreliable Echo → feared Echo`.
The implementation remains deterministic while the Player gains reasons to
distrust the protagonist's account of what was recorded.

## Player-facing fear resources

### Spatial doubt

Most physical sounds are `world-locked`: they occupy an authored place and move
correctly between the ears when the Player turns. This makes the virtual room
learnable and lets a dry domestic knock, footsteps behind the Player, or breath
near a shoulder feel spatially credible.

Rare `head-locked/internal` events may remain close to one ear when the view
turns. They are reserved for intimate perception, never normal world physics and
never essential information. The exact binaural/HRTF implementation is TBD and
must be tested rather than assumed.

The desired trans-screen effect is that the Player briefly checks a real door or
corner, turns toward the room behind them, removes one earcup to verify the
source, or cannot immediately decide whether a sound came from the game. These
are possible human observations, not instructions to raise volume.

### Recurring motif families

| Family | Examples | Horror function |
| --- | --- | --- |
| Facility / structure | metal impacts, ducts, machinery, pressure, glass, mechanism strain | Establish a coherent building, then imply activity outside the reachable route |
| Presence / body | extra footsteps, breathing, fabric, fingers, weight behind a door, brief running | Suggest proximity without confirming an enemy |
| Domestic memory | household door, chair scrape, dishes, water, child melody, toy, accident fragments | Let home invade the institution without immediate exposition |
| Time / repetition | the Player's steps later, a prior action in another room, sound following an Echo path and continuing | Connect correct recording to omitted or misremembered experience |

Each motif may receive authored variants in distance, side, elevation, duration,
dryness or reverberation, restraint or violence, and partial or complete form.
Variation follows scene state and dramatic intent; it is not a random scare
director. Truthful door, plate, and RECALL feedback keeps its verified causal
identity.

### Check and consequence

A useful pattern is:

`hear → locate → check → find nothing → continue → receive partial evidence`

Not every check pays off. Some produce nothing; others later reveal a mark,
object, route change, remembered fragment, or contradictory but mechanically
truthful trace. If checks never matter, the Player learns to ignore audio. If
every sound yields an object, the pattern becomes a vending machine. The
consequence schedule must be authored and remains TBD.

### Puzzles as vulnerability

Puzzles should expose the Player instead of pausing the horror. A situation may
require watching a terminal with the room behind them, waiting while a sound
approaches, leaving the Echo out of sight, crossing a known corridor again,
holding position to listen, or pressing RECALL despite anticipating what will
return. Difficulty still comes from understanding and planning, not hidden rules
or frame-tight execution.

No production puzzle is added merely to fill minutes. Its mechanic, spatial
exposure, sensory beat, and recovery role must be defined together.

## Planned five-chapter escalation

This direction is approved but not implemented. Exact durations, rooms, puzzles,
and event counts remain TBD.

1. `Protocol` builds safety: clinical causality, useful RECALL, and the first
   uneasy impact without breaking trust.
2. `Familiarity` creates spatial doubt: learned spaces produce directional
   checks, small consequences, and domestic sounds that do not belong.
3. `Recollection` lets home and presence invade more directly while the Echo
   continues to reproduce the truth.
4. `Denial` sustains pressure through omitted-cycle evidence, stronger recurring
   variants, and authored set pieces that contradict the protagonist's account.
5. `Acceptance` merges facility and memory, reconstructs the traumas, and ends
   with Final C: confrontation, the nightmare ending, and a cut to black without
   showing whether the protagonist wakes.

For roughly 70–80 percent of the experience, physical danger need not be
confirmed. This does not require low intensity: directional pursuit without an
enemy, domestic intrusion, strong impacts, and sustained presence can create
pressure while combat and generic chase horror remain absent.

## Current planned study: SD1

`SD1` is approximately five minutes in the existing space and adds no puzzle:

1. establish the stable facility baseline;
2. place one dry domestic knock behind-right, distinct from the game door;
3. move authored footsteps from left to right and behind the Player;
4. place world-locked breathing whose location remains correct when turning;
5. use one rare, intelligible head-locked whisper near a single ear;
6. give one cue a subtle, delayed visual consequence;
7. leave recovery instead of stacking another event.

The sequence and content are approved as a study brief, not implemented
features. The whisper is `Can you hear me?`, spoken in English and subtitled
`¿Me escuchás?` in Spanish. Its identity, performer, treatment, intended ear,
clip, and trigger remain TBD. The consequence must be subtle and mechanically
inert; its exact form, emitters, spatial technology, mix levels, and timings
remain TBD. The current chair may be present but is not the study's climax.

Objective checks must preserve Echo data, reset, puzzle truth, and causal audio;
differentiate world-locked from head-locked behavior; avoid duplicate triggers;
retain headroom; and exercise headphones, speakers, mono, and a reduced-dynamics
presentation.

The human checkpoint is unannounced beyond a general psychological-horror and
sudden-audio warning. Record spontaneous turns, checks of doors or corners,
source doubt, emotional tension, mechanical trust, and physical comfort. A
guided delivery pass cannot validate surprise or trans-screen fear.

## Safety and accessibility

- Recommended copy: `Designed for headphones. Set a comfortable volume; details
  do not require high volume.`
- Plan `Headphones` and `Speakers` presentations, left/right calibration,
  separate ambience/effects/voice controls, and `Reduced Dynamics`.
- Never instruct the Player to use high volume. A scare that works only by
  raising level fails.
- Stop testing for pain, tinnitus, blocked-ear sensation, dizziness, panic, or
  other physical distress. Emotional discomfort is intended; harm is not.
- Spatialized audio must have non-audio support for essential information.

## Implemented discrepancies and open questions

- Echo anomalies: none.
- Environmental proof: the M6.1 chair reveal persists for the session.
- Audio proofs: facility-air subtraction, truthful door/plate/RECALL cues, and
  the one-shot M6.3 chair proximity sound.
- Emotionally approved headline scares: none.

The protagonist's name, guiding voice, exact accident details, facility reality,
production motif palette, score language, voice content, spatializer choice,
final event roster, puzzle roster, chapter durations, and exact strong-event
placements remain TBD. The chair's approach/departure timing is no longer a
current milestone; retain or replace it only if a later motif needs it.

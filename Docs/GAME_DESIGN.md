# Game design

This document synchronizes the approved product direction in
[`Design/Echo Protocol - Game Design Document v0.2.pdf`](Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.2.pdf)
with the current repository. `ARCHITECTURE.md` and playtest evidence remain
authoritative for implemented and validated behavior.

## High concept

The protagonist wakes in an unfamiliar but strangely recognizable facility and
solves controlled experiments through RECALL. Each reset turns the previous
attempt into an Echo that can cooperate with the Player. Echoes first become
precise, necessary tools; later they reproduce events the protagonist does not
remember, turning temporal mastery into an investigation of guilt, memory, and
denial.

The central narrative rule is approved: Echoes remember what happened
correctly; the protagonist does not. Apparent anomalies must arise from omitted
cycles, repressed actions, altered perception, or dream logic, never arbitrary
playback corruption.

Correct memory means fidelity to data the system actually captured, not magical
omniscience. The current prototype records only movement and body yaw. Future
channels must be implemented explicitly before the Echo can reproduce them. A
later omitted cycle may reveal a truthful event outside the protagonist's
conscious account, but it cannot rewrite a completed recording the Player
already inspected.

## Audience and experience

The planned audience is older teens and adults interested in psychological
horror, narrative mystery, accessible temporal puzzles, and short high-intensity
experiences. Themes include death, guilt, childhood, accidents, loss, grief, and
distorted memory. Strong content should come mainly through implication rather
than combat, gore, or explicit exposition. Target platforms are Windows and
macOS.

## Design pillars

1. Trust before fear: a rule must become dependable before it can create doubt.
2. The mechanic is the horror: RECALL and Echoes remain the central source of
   both mastery and fear.
3. Player-facing horror: a moment must affect the real Player's perception and
   behavior; narrative meaning or a changed prop alone is not enough.
4. Sound is the primary weapon for space, timing, presence, memory, and doubt.
   Authored strong events are valid when earned, varied, and safe; shock does
   not replace construction.
5. Puzzles create vulnerability: they make the Player wait, listen, look away,
   revisit known space, depend on an unseen Echo, or choose RECALL under dread.
6. Small scope, high polish: a short coherent game takes priority over broad
   unfinished content.

Audio is primary, not exclusive. Spatial composition, lighting, architecture,
material progression, reflections, partial figures, narrative evidence, and the
new meaning of an unchanged Echo must reinforce the same authored fear beat.

The intended emotional arc is curiosity, unease, doubt, paranoia, terror or
sadness, and acceptance. Each loop should increase knowledge while reducing
confidence in the protagonist's interpretation.

## Difficulty and fear

Puzzle difficulty should come from understanding, planning, memory, and temporal
coordination rather than opaque rules or frame-tight execution. A correct plan
must have a visible, repeatable margin for success, and timing-critical values
remain configurable for playtesting.

Puzzle situations are not the sole source of runtime and must not become a chain
of interchangeable two-minute plate rooms. Each production puzzle needs a clear
place in the horror escalation and a concrete vulnerability it creates. The
exact room and puzzle roster remains TBD until the complete 30–45 minute fear
map is designed.

Terror may later consume attention, create discomfort, and make the player doubt
their observation. It must not secretly change input, playback speed, pressure
plate truth, door timing, or the result of an otherwise identical attempt. The
player may feel uncertain; the normal system must first remain trustworthy.

## Core loop

Explore, listen, understand, plan, expose yourself to a space, act, activate
RECALL, cooperate with the resulting Echo, verify what changed, and advance.
Progress combines puzzle resolution, traversal, authored audiovisual events,
narrative reconstruction, and recovery rather than measuring duration by puzzle
count alone.

## Implemented and validated

- Greybox first-person walking and mouse look.
- Collision and grounded movement through a CharacterController.
- Center-camera interaction within a three-metre range using `E`.
- An earlier toggle switch remains as an inactive interaction proof.
- Timestamped player movement recording at 20 samples per second, armed until
  first movement with a predictable one-second lead-in.
- `R` ends an attempt, restores the known baseline, and starts the next attempt.
- One translucent greybox Echo replays the latest attempt and disappears at its
  recorded end.
- Player and Echo can hold an orange pressure plate, including together, to open
  a physically blocking sliding door.
- A green exit pad behind the door completes only for the Player, making the
  first record/reset/cooperate loop objectively solvable.
- The current M5.1 variant requires two simultaneous plates, exposes both inputs
  through status lamps, and gives the threshold plate enough physical depth for
  a fair crossing once the AND condition is satisfied.
- The active M5.2 variant requires A before B, remembers the completed first
  step, and opens its door only while B remains occupied. A visible blocker
  prevents a Player-only race from substituting for Echo cooperation.
- M6.1 adds one concrete environmental discrepancy: after the Player completes
  the goal, the next successful RECALL reveals a colliderless chair outside
  direct view. It persists for that Play Mode session while all
  Echo data and puzzle rules remain unchanged.
- A layered prototype facility ambience establishes the normal room. Its
  facility-air layer fades to sustained silence only when the M6.1 chair is
  revealed, while fluorescent and distant-machinery detail remains. It has no
  stinger, randomization, or gameplay effect.
- M6.2 adds a minimal `Ambience` / `WorldSFX` mixer split and one deterministic
  3D motor on the active sliding door. The cue follows physical movement,
  remains continuous through reversal, and stops cleanly on RECALL.
- M6.3 gives the revealed chair one fixed 3D source. First Player proximity plays
  one low procedural settling cue; it cannot trigger before reveal, repeat after
  RECALL, move the chair, or change any puzzle and Echo state.
- AV1 gives the active room a closed ceiling, textureless clinical material
  separation, stable local fixtures, dark trim, and muted domestic chair
  materials. It changes no gameplay geometry, Echo appearance, audio rule, or
  anomaly timing.

Movement, camera, interaction, recording, RECALL/reset, Echo playback, and the
first one-plate puzzle are human-approved. M5.1's dual-plate simultaneity puzzle
and M5.2's sequential relay are also human-approved. M6.1 is objectively verified
and its hum/disappearance behavior is human-approved; the visual-only chair did
not create discomfort by itself. M6.2's causal mechanism-audio baseline is also
objectively verified and human-approved. Its procedural motor timbre was judged
synthetic; AD2 now replaces it with the user-selected domestic movement A and
endpoint B pair. The integrated route is human-approved for continued
prototyping, but every imported timbre remains replaceable.
M6.3 is objectively verified, and a guided human pass confirmed first-play and
no-repeat behavior. It produced no terror or discomfort, so spontaneous
attribution and emotional effectiveness remain unapproved. The chair remains a
secondary environmental proof or motif anchor, not evidence of a successful
headline scare. AV1 is objectively
verified through static scene audit, first-person captures, and five consecutive
RECALLs, and its human visual checkpoint is approved for continued prototyping.

The current prototype retains one latest movement-and-yaw Echo. It does not yet
record semantic interactions, support multiple simultaneous Echoes, show player
arms, provide production UI or audio, save progress, or contain narrative
chapters.

## Approved product structure, not implemented

The game is planned as five chapters:

1. `Protocol` - curiosity; reliable movement, RECALL, and Echo rules in a
   clinical facility.
2. `Familiarity` - unease; small discrepancies and inexplicably familiar
   objects enter otherwise trustworthy spaces.
3. `Recollection` - doubt; personal rooms and memories increasingly invade the
   experiments.
4. `Denial` - paranoia; the protagonist's account and correctly remembered
   events can no longer be reconciled.
5. `Acceptance` - terror and sadness; both connected traumas converge and the
   protagonist confronts the memory.

The approved horror-first progression is normality and trust, spatial doubt,
domestic intrusion, sustained presence and remembered contradiction, then
emotional reconstruction. Exact chapter durations, room count, puzzle roster,
event count, and placement remain TBD. Several authored strong moments may occur
across the full game, but they must grow from recurring motifs and recovery
rather than random scheduling or unsafe loudness.

The high-level narrative links a childhood accident involving a younger sibling
and a later related accident involving the protagonist's child. Exact events,
responsibility, and avoidability remain TBD. Final C is confirmed: the
protagonist confronts the memory, the nightmare ends, and the game cuts to black
without showing whether they wake.

## Planned art, audio, and UI direction

- Visuals move from white, grey, cold blue, glass, and metal toward domestic
  warmth, wood, textiles, photography, mixed light, and finally an impossible
  coexistence of facility and memory. Semi-realistic proportions and composition
  matter more than AAA photorealism.
- Echoes should eventually read as complete human silhouettes with restrained
  transparency and temporal artifacts, never progressive monster deformation.
- Audio should draw from four recurring families: facility/structure,
  presence/body, domestic memory, and time/repetition. Motifs gain authored
  variants in distance, side, elevation, duration, acoustic dryness, intensity,
  and completeness. World-locked sources remain spatially truthful; rare
  head-locked/internal moments may break that expectation without carrying
  essential information.
- Headphones are recommended at a comfortable volume, never a high one. Planned
  UX includes left/right calibration, `Headphones` and `Speakers` presentations,
  separate ambience/effects/voice controls, and `Reduced Dynamics`; none is
  implemented yet.
- Product UI is planned to be minimal and preferably diegetic: clear RECALL and
  interaction feedback, English spoken audio with configurable Spanish/English
  subtitles, pause/settings, and no health bar, minimap, quest tracker, or
  complex permanent HUD.
- A contextual reticle remains a production UX option, not a correction to the
  current greybox interaction, which was human-approved without one.

## MVP content target

- One small facility/hub with limited backtracking and a coherent visual
  identity.
- A small roster of meaningful experiments whose exact count remains TBD. Each
  must recombine Echo rules while serving a distinct vulnerability and horror
  beat; buttons, plates, doors, sensors, platforms, terminals, and timing are a
  vocabulary, not a quota.
- One Echo for the vertical slice; up to two or three is an approved product
  direction but remains unimplemented and requires a concrete validated puzzle.
- Progressive psychological horror, concise narrative, spatial audio, minimal
  UI, autosave between rooms or chapters, and Final C.
- Target playtime: 30-45 minutes, reducible to roughly 20 excellent minutes
  before sacrificing quality.

## Scope exclusions

No combat, weapons, systemic enemies, multiplayer, networking, backend, open
world, crafting, RPG progression, achievements, multiple save slots, complex
inventory, or generic chase-focused horror. Echo manipulation of movable physics
objects is explicitly NO MVP. Sprint is EN PRUEBA and excluded from the opening;
performance targets remain TBD until profiling on target hardware.

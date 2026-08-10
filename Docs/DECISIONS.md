# Decisions

## 2026-08-07 — Greybox before content

**Decision:** Validate movement and the Echo proof before final art or horror.

**Why:** It protects the core mechanic and solo-semester scope.

**Tradeoff:** The project initially looks plain and postpones atmosphere work.

## 2026-08-07 — CharacterController locomotion

**Decision:** Use Unity's CharacterController for the first-person capsule.

**Why:** It provides predictable collision and grounded movement without
Rigidbody force tuning, which suits deliberate puzzle navigation.

**Tradeoff:** Physical pushes and advanced locomotion will need explicit handling
if later approved.

## 2026-08-07 — Reuse the template Input System asset

**Decision:** Use its existing `Player/Move` and `Player/Look` actions for the
first checkpoint.

**Why:** The required bindings already exist and avoid premature input-asset
churn.

**Tradeoff:** Unused template actions remain until a deliberate cleanup.

## 2026-08-07 — Deliberate Git LFS policy

**Decision:** Automatically LFS-track common large source/binary formats, but not
every PNG, JPEG, archive, font, or document.

**Why:** The repository had no LFS objects and its broad template rules already
misclassified a small PNG stored as a normal Git blob.

**Tradeoff:** A genuinely large delivery image must be reviewed and tracked by
path before commit.

## 2026-08-07 — First-hit interaction ray

**Decision:** Interact by casting three metres from camera centre and evaluating
only the first physical collider hit.

**Why:** It is simple, predictable, supports child colliders, and prevents using
switches through walls.

**Tradeoff:** There is no reticle, prompt, highlight, or alternate interaction
shape until human testing proves one is needed.

## 2026-08-07 — Timestamped pose snapshots

**Decision:** Record player position and body rotation every `0.05` seconds,
copy the completed attempt into an immutable snapshot, and interpolate playback
by timestamp.

**Why:** It gives visually stable replay without trying to resimulate keyboard
input or physics and keeps a 60-second attempt small.

**Tradeoff:** Camera pitch and semantic interactions are not recorded yet; add
them only when a real puzzle proves the requirement.

## 2026-08-07 — One latest Echo with a predictable end

**Decision:** Retain only the latest completed attempt, replace its Echo on the
next reset, and make normal playback disappear at its recorded end.

**Why:** One Echo is sufficient for the first temporal puzzle, prevents runtime
accumulation, and establishes the reliable rule against which future apparent
discrepancies can be judged.

**Tradeoff:** Multi-Echo puzzles are unavailable until explicitly designed and
approved.

## 2026-08-07 — Explicit loop baseline

**Decision:** Reset only a serialized list of small `ILoopResettable` targets in
addition to the player and Echo lifecycle.

**Why:** The known baseline is testable and scene-specific without scene-wide
searches, reloads, or a premature reset framework.

**Tradeoff:** Every new mutable puzzle component must be deliberately connected
to the loop controller.

## 2026-08-07 — Physical temporal-actor occupancy

**Decision:** Mark Player and Echo roots with a small `LoopActor` component and
let the pressure plate track physical colliders per actor.

**Why:** The first puzzle needs both bodies to obey the same visible spatial rule
while remaining robust when an Echo disappears, is replaced, or owns multiple
colliders.

**Tradeoff:** New physical temporal actors must opt in with the marker; unmarked
physics objects intentionally do not activate the plate.

## 2026-08-07 — One concrete plate-to-door puzzle

**Decision:** Give the first plate a direct reference to one sliding door and
place a Player-only latching goal beyond it.

**Why:** It proves the complete record/reset/cooperate loop with an objective
finish without inventing a generic event graph before another room needs one.

**Tradeoff:** Combining several mechanisms will require a later, evidence-based
extension instead of configuring arbitrary signal chains now.

## 2026-08-07 — Challenge the plan, not the controls

**Decision:** Make harder puzzles demand temporal planning and divided attention
while keeping their physical rules deterministic and execution windows tunable.

**Why:** Future discomfort and terror will already tax attention and memory. If
the base mechanic also changes secretly or requires near-frame-perfect movement,
failure feels arbitrary instead of frightening.

**Tradeoff:** Timing must be measured and human-tested; difficulty cannot be
created cheaply by shrinking every window.

## 2026-08-07 — Concrete dual-plate AND condition

**Decision:** Add one `DualPlateDoorCondition` for M5.1 instead of a generic
signal graph. M4 keeps direct plate-to-door control; M5.1 owns one door through
two otherwise normal plates.

**Why:** It introduces simultaneity and planning with the smallest rule extension
that the second puzzle actually requires.

**Tradeoff:** A later sequential puzzle may justify another small composition,
but arbitrary boolean authoring remains unavailable.

## 2026-08-08 — Arm recording until first movement

**Decision:** Do not accumulate arbitrary idle time at spawn. Arm each attempt
until the Player moves horizontally by `0.02` metres, then begin the recording
with a fixed one-second lead-in. Preserve every pause after movement begins.

**Why:** Echo playback reaches meaningful actions predictably without erasing
deliberate pressure-plate dwell or requiring the player to reset immediately.

**Tradeoff:** Pressing `R` before any movement produces no new Echo. The start
gate currently observes body translation rather than input intent or semantic
interactions.

## 2026-08-08 — GDD governs product direction, not implementation status

**Decision:** Preserve GDD v0.1 under `Docs/Design/` as the primary source for
product vision, narrative, experience, art, audio, UI, scope, and future
direction. Keep repository code, architecture, and playtests authoritative for
what exists and what has been validated.

**Why:** The GDD was written as preproduction direction and its provisional
schedule now trails the working prototype.

**Tradeoff:** Documentation must label future content explicitly instead of
copying GDD features into the implemented architecture.

## 2026-08-08 — Echoes remember; the protagonist does not

**Decision:** Treat correct Echo memory and unreliable protagonist memory as a
central narrative rule. Apparent future anomalies may express omitted cycles,
repressed action, altered perception, or dream logic, but not random corruption
of otherwise identical playback.

**Why:** The horror depends on investigating memory through the same dependable
system the Player learned to use.

**Tradeoff:** Every discrepancy needs narrative causality and cannot be added as
an interchangeable glitch or arbitrary rules violation.

## 2026-08-08 — Confirm Final C

**Decision:** End with the protagonist confronting the memory, the nightmare
ending, and a cut to black without showing whether they wake.

**Why:** It resolves the emotional and causal arc while retaining one restrained
uncertainty instead of invalidating the story with another reveal.

**Tradeoff:** The ending cannot rely on a shown awakening or a final twist that
reframes the confrontation as irrelevant.

## 2026-08-09 — Concrete sequential A-to-B condition

**Decision:** Implement M5.2 with one scene-specific condition that remembers a
rising A press, accepts only a later rising B press, and holds one door open only
while B remains occupied. Use status lamps and visible geometry to force Echo
cooperation without a timeout or actor-specific rule.

**Why:** It teaches ordered temporal planning with the same physical plate and
door language already validated in M4 and M5.1.

**Tradeoff:** The state machine and anti-solo blocker are deliberately concrete;
they do not provide arbitrary sequences, timers, or a reusable signal graph.

## 2026-08-09 — First discrepancy changes the room, not the Echo

**Decision:** After the Player completes the M5.2 goal, reveal one colliderless
institutional chair on the next successful RECALL. Drive the transition through
direct rising-edge goal and loop-completion events, then preserve the chair until
the scene or Play Mode session reloads.

**Why:** A peripheral object can make the protagonist question whether it was
always present while every recorded frame, puzzle rule, and reset remains
trustworthy. It begins the useful Echo → uneasy Echo arc at minimal cost without
inventing corrupted playback or a generic horror director.

**Tradeoff:** The initial visual-only M6.1 had no audio, lighting change,
explanation, randomization, or mechanical consequence. It was noticed in human
testing but did not create discomfort, motivating the focused audio iteration.

## 2026-08-09 — Silence supports the first discrepancy

**Decision:** Establish one quiet, procedural 2D clinical hum and fade it to
sustained silence when the chair's `Revealed` event fires. Generate the loop at
runtime, use no external asset, and add no stinger, localization, randomness, or
audio manager.

**Why:** The visual-only chair proved readable but not uncomfortable. Removing a
previously stable room tone can direct attention through absence while keeping
the environmental change restrained and every Echo rule unchanged.

**Tradeoff:** A synthetic greybox hum is only an emotional prototype, not final
sound design. Its fade is now human-approved, but the broader discomfort and
production mix still require later evidence.

## 2026-08-09 — Teach truthful spatial audio before another discrepancy

**Decision:** Begin M6.2 with one mixer split (`Ambience` / `WorldSFX`) and one
deterministic 3D motor on the active sliding door. Drive it from physical panel
displacement, keep one voice through reversal, play one endpoint cue only after
real travel, and silence it explicitly on loop reset.

**Why:** Horror audio can exploit a learned soundscape only after normal causes,
locations, and timing are trustworthy. One concrete door rule is cheap to test
and cannot be confused with a general sound system or Echo corruption.

**Tradeoff:** Plates, RECALL, footsteps, music, stronger ambience, and new audio
anomalies remain deferred. Human testing approved the causal greybox rule but
judged its procedural motor synthetic; final material, friction, and creak remain
part of the later production sound-design pass.

## 2026-08-09 — Tie the first localized discrepancy to deliberate proximity

**Decision:** Give the revealed M6.1 chair one fixed 3D source and play a low
procedural settling cue once on the first Player proximity within
`1.3 m`. Keep the source under the hidden chair, outside loop reset, with no
randomness, visible movement, or mechanical consequence.

**Why:** A source attached to an inert object can create a small spatial doubt
without reusing the door, inventing an invisible Echo, or making routine RECALL
itself a scare trigger. Proximity lets the Player initiate the moment through
curiosity rather than scripted camera control.

**Tradeoff:** The cue is a greybox timbre and may be missed by a Player who never
examines the chair. Its emotional effect and attribution require human evidence;
failure should be addressed through timing, range, or timbre, not higher volume
or additional simultaneous anomalies.

The first guided human pass confirmed the one-shot behavior but produced no
terror or discomfort. This validates delivery only; it does not close the
emotional or unprimed-attribution question.

## 2026-08-09 — Build the soundscape before adding another anomaly

**Decision:** Pause isolated horror cues and design one integrated 5–8 minute
`Familiarity` audio foundation in the current M5.2/M6 room. First establish a
credible facility body and truthful functional feedback; then subtract one
learned layer, test one chair cue timing, and leave recovery space. This does not
validate the complete chapter or its future domestic-memory layer. Add no new
gameplay, music, voice, or strong peak in the first pass.

**Why:** The hum, door, and chair prototypes validate routing, causal timing, and
one-shot delivery, but the guided chair pass produced no terror or discomfort.
Fear needs a learned normal soundscape, deliberate contrast, and coherent pacing;
more volume or more disconnected events would not supply that context.

**Tradeoff:** New anomalies and narrative content pause while a small palette is
sourced and mixed. The current chair approach cue remains functional but its
departure-after-close-inspection alternative is **EN PRUEBA**. Production assets
require provenance and license review, and emotional approval still requires a
non-primed checkpoint rather than technical verification alone.

## 2026-08-09 — Use a CC0-first candidate palette for the public repository

**Decision:** Shortlist eight CC0 recordings or effects for nine first-proof
audio roles and track them in `AUDIO_ASSET_LEDGER.md`. Treat every file as a
candidate until its canonical page and license are rechecked, the full recording
is auditioned and hashed, its provenance is credible, and its derivative is
reviewed. Do not import a stock, Student-plan, or generated asset merely because
access is free. Keep Unity AI optional for an abstract experiment only after
explicit consent and terms/quota review; defer Blender and Blender MCP until a
concrete 3D task exists.

**Why:** The repository publishes raw assets, so an end-product license can be
insufficient even when a sound is free to use in a game. Predominantly recorded
material is the lowest-cost path away from the synthetic door and room
placeholders, while a small ledger makes attribution, replacement, and public
redistribution auditable.

**Tradeoff:** AD1 locks roles and candidates, not final timbre. Any source can be
rejected during AD2, the pressure-plate release may need a separate recording,
and the RECALL identity remains TBD. Conservative sourcing costs more review
time but avoids building the soundscape on ambiguous files or unnecessary tools.

## 2026-08-09 — Lock the domestic door direction, not the whole AD2 palette

**Decision:** Use movement option A and endpoint option B from the same CC0
domestic sliding-door recording. Import the other accepted AD2 derivatives only
as replaceable prototype material. Their legal and technical acceptance does not
make them the final sonic identity of the game.

**Why:** The domestic material inside an institutional mechanism creates the
specific ordinary-but-misplaced quality the user preferred, while one recording
keeps motion and endpoint coherent. Continuing to collect alternatives before
hearing the complete Unity route would add selection cost without improving the
decision. The other candidates are sufficient to test density, causality,
subtraction, and reset behavior even though none inspired a strong preference.

**Tradeoff:** AD2 can reach an integrated human checkpoint now, but facility,
plate, and RECALL timbres remain expected replacement targets. Later replacements
must preserve the verified buses, timing, ownership, reset, and Echo-trust rules.

## 2026-08-10 — Establish the place before judging the scare

**Decision:** Add one scene-local AV1 presentation layer to the existing
prototype: textureless clinical materials, a closed render-only ceiling, dark
trim, three stable local fixtures, reduced directional fill, and muted
wood/metal on the already implemented chair. Preserve all gameplay geometry,
colliders, Echo presentation, audio ownership, and anomaly timing.

**Why:** The approved prototype audio and chair discrepancy were being judged in
an open, uniformly grey, externally lit room. A small coherent place is required
before timing or sound can carry meaningful unease.

**Tradeoff:** AV1 improves spatial credibility but is not production art and is
not expected to create terror by itself. Textures, props, fog, post-processing,
flicker, asset packs, and another anomaly remain deferred until the human visual
checkpoint identifies a concrete need.

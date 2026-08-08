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
accumulation, and establishes the reliable rule that future anomalies may break.

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

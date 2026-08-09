# Roadmap

The dated 16-week schedule in GDD v0.1 is provisional and predates the actual
repository progress. Its dependency order remains useful; repository evidence
and human checkpoints determine current status.

## Current milestone

M6.3 — First restrained localized audio discrepancy. Implemented and objectively
verified; a guided human pass confirmed first-play/no-repeat behavior but produced
no terror or discomfort. Unprimed attribution and emotional evidence remain open.

## Implemented in the current milestone

- The hidden M6.1 chair owns one initially inactive `ChairSettlingCue` and 3D
  `AudioSource` routed to `WorldSFX`; no separate anomaly object or manager was
  added.
- The first Player proximity within `1.3 m` plays one low procedural cue. The
  source never activates before the chair reveal and never rearms after playing.
- The cue has no collider, Rigidbody, random timing, visual movement, reset-list
  entry, puzzle output, or Echo dependency.
- Runtime verification covered the real chair reveal, Player proximity,
  repeat proximity, an invalid RECALL, five later successful RECALLs, clip bounds,
  and the final known baseline with one Echo and zero plate occupants.
- The proximity radius remains outside the full Plate A trigger bounds even when
  accounting for the Player controller radius. A RECALL during the short cue lets
  that persistent world-space sound finish without restarting it.

## Completed

- M0: exact Unity version, URP and Input System packages, Git/public remote, LFS,
  live Unity Console, active scene, AI package, and Unity MCP audited.
- M0: Unity MCP create/find/delete smoke test passed.
- M1: concise repository documentation and project-owned folder structure.
- M1: separate development greybox scene.
- M2: grounded movement, mouse look, collision, and cursor capture.
- M2: human movement-feel checkpoint approved at `0.15` look sensitivity.
- M2: center-camera interaction, `E` input, physical occlusion, and a repeatable
  greybox toggle target.
- M2: human interaction checkpoint approved without requiring a reticle or range
  adjustment.
- M3: fixed-interval temporal movement recording, immutable snapshots, and
  interpolated Echo playback.
- M3: known-baseline reset for player, switch, and Echo lifecycle; one retained
  latest attempt and automatic 60-second loop limit.
- M3: human recording, reset, Echo readability, and repeated-loop checkpoint
  approved.
- M4: Player and Echo occupancy, physical sliding door, Player-only exit goal,
  complete temporal room, and known-baseline reset.
- M4: end-to-end playback and five consecutive resets verified; human clarity,
  timing, and feel checkpoint approved.
- M5.1: deterministic dual-plate simultaneity, independent status lamps, and
  fair threshold crossing objectively verified and human-approved.
- M5.2: deterministic sequential relay, ordered status feedback, anti-solo
  geometry, and Echo cooperation objectively verified and human-approved.
- M5: reliable normal puzzle language human-approved through simultaneity and
  sequential planning.
- M6.1: deterministic chair reveal persists outside the loop without changing
  any recorded frame, puzzle rule, path, or timing.
- M6.1b: procedural clinical hum, reveal-only fade, and sustained silence
  objectively verified; the hum and its disappearance are human-approved.
- M6.2: deterministic physical door audio, mixer separation, reversal, midpoint
  reset, and repeated-loop behavior objectively verified. The causal baseline is
  human-approved; its synthetic motor timbre remains a production placeholder.

## Next

- M6.3: run the human proximity-cue checkpoint without announcing its timing.
  Reject it if it reads as a door bug, corrupted Echo, random cheap scare, or
  physical discomfort.
- If a second unprimed pass remains emotionally flat, change the timing or
  replace the procedural timbre before increasing volume or layering another
  anomaly.
- Production audio pass: replace the approved door placeholder with authored or
  licensed material, friction, and creak while preserving its causal timing.
- M7: planned content and narrative pass across the five-chapter structure,
  including the small hub, art progression, audio, minimal UI, and autosave.
- M8: polish, QA, Windows/macOS builds, and presentation.

M7-M8 are product direction, not implemented systems or locked calendar dates.
Scope may be reduced before quality or the central arc is compromised.

## Product gates and risks

- Reset inconsistency would break both puzzles and trust; preserve explicit
  baselines and consecutive-loop verification.
- An environmental discrepancy that reads as broken playback would undermine
  trust; require human attribution to the room rather than the Echo.
- Puzzle difficulty targets planning and coordination around 3/5, with tunable
  margins and diegetic assistance rather than frame-tight execution.
- Narrative ambiguity should invite reconstruction without making causality
  illegible; narrative playtests are required later.
- Art, audio, voice, and licensed assets can consume solo production capacity;
  prefer modular reuse, few lines, and source/license review.
- Performance targets remain TBD until the vertical slice is profiled on target
  hardware.

## Open design decisions

- Protagonist name: TBD; anonymity remains valid unless naming improves voice or
  narrative clarity.
- Guiding voice identity: TBD; it must connect to the traumas without artificial
  exposition.
- Exact events and responsibility in both accidents: TBD; only their high-level
  sibling/child relationship is approved.
- Facility name and literal reality: TBD; keep it institutional and ambiguous
  unless definition strengthens the story.
- Product RECALL parameters: TBD beyond the validated prototype baseline of one
  latest Echo, a 60-second cap, and recording armed until first movement.
- Sprint: EN PRUEBA and excluded from the opening.
- Performance target: TBD after profiling on target Windows/macOS hardware.
- Final moodboard references and production assets: TBD pending coherence,
  source, license, and development-cost review.

## Deferred / cut for now

- Current milestone exclusions: sprint, crouch, jump, stamina, object pickup,
  player arms, pause/settings UI, semantic Echo interactions, multiple Echoes,
  autosave, final production art/audio, narrative content, production scenes,
  and additional horror anomalies.
- NO MVP: Echo manipulation of movable physics objects, combat, weapons,
  systemic enemies, multiplayer/networking/backend, open world, crafting, RPG
  systems, achievements, complex inventory, multiple save slots, and additional
  nonessential ports/localization.
- Avoid a generic horror director, anomaly framework, or puzzle signal graph
  until a concrete approved feature requires one.

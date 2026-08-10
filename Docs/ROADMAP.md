# Roadmap

The dated 16-week schedule in GDD v0.1 is provisional and predates the actual
repository progress. Its dependency order remains useful; repository evidence
and human checkpoints determine current status.

## Current milestone

`AD3` assembly — AV1's smallest clinical visual foundation is objectively
verified and human-approved for continued prototyping. Assemble the integrated
5–8 minute `Familiarity` route from the validated room and audio layers without
changing M5.2 geometry, collision, timings, Echo behavior, audio ownership,
scene variants, or reset contracts.

## Defined in the current milestone

- Preserve the repeatable facility body and truthful functional feedback long
  enough for the Player to learn the normal soundscape.
- Subtract the facility-air layer at the existing reveal, test one chair timing
  variant, and leave recovery space.
- Keep the approach and departure chair variants mutually exclusive.
- Add no new gameplay, voice, music, strong peak, or second anomaly in this pass.

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
- M6.1b: procedural clinical hum and reveal-only fade to a stopped source
  objectively verified; the hum and its disappearance are human-approved. AD3
  will separately test perceptual quiet with residual facility detail, so
  total-room silence is not carried forward as approved.
- M6.2: deterministic physical door audio, mixer separation, reversal, midpoint
  reset, and repeated-loop behavior objectively verified. The causal baseline is
  human-approved; its synthetic motor timbre remains a production placeholder.
- M6.3: deterministic chair-owned proximity cue objectively verified through
  reveal, first playback, no repeat, invalid RECALL, and five later loops. Its
  guided human delivery check passed, but emotional effectiveness remains open.
- AD0: coherent soundscape direction, pacing, trust contract, scope, safety,
  accessibility, and human/objective gates approved as the next production path.
- AD1: eight CC0 source candidates, nine target roles, acceptance ledger,
  delivery specification, public-repository policy, and toolchain assessment
  documented without downloading or importing audio.
- AD2 implementation: six CC0 originals reviewed and hashed, eight standardized
  derivatives imported, sanitized license evidence retained, facility-air and
  door procedurals replaced, two persistent beds added, and truthful plate and
  successful-RECALL cues wired. Importer configuration, explicit references,
  door reversal, plate edge/reset behavior, and five consecutive RECALLs passed
  objective verification. Integrated human listening approved continued
  prototyping while explicitly leaving all production timbres open.
- AV1 implementation: textureless clinical materials, a render-only ceiling and
  trim, three stable local fixtures, and a restrained directional fill added to
  the active prototype. Static diff audit, reference captures, Console checks,
  Echo presentation/state preservation, and five consecutive RECALLs passed.
  The developer then approved the visual foundation for continued prototyping.

## After AD3

- `AD4`: run objective reset/Echo regression and an unannounced human
  attribution/emotional checkpoint.
- `AD5`: adjust timbre, timing, density, and mix before approving another
  anomaly. A single authored peak becomes a later controlled variant only after
  the quiet proof works.
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
  autosave, full-game production art/audio, voice, music, narrative content,
  production scenes, and additional horror anomalies.
- NO MVP: Echo manipulation of movable physics objects, combat, weapons,
  systemic enemies, multiplayer/networking/backend, open world, crafting, RPG
  systems, achievements, complex inventory, multiple save slots, and additional
  nonessential ports/localization.
- Avoid a generic horror director, anomaly framework, or puzzle signal graph
  until a concrete approved feature requires one.

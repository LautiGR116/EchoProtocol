# Roadmap

## Current milestone

M5.1 — Dual-plate timing. Implemented and objectively verified; awaiting human
approval of challenge, fairness, readability, and predictability.

## Implemented in the current milestone

- A concrete `A && B` condition requires an Echo and Player to cooperate across
  two physically separate pressure plates.
- Independent status lamps expose each plate state while the door remains bound
  to the deterministic AND rule.
- The M5.1 door has an instance-only travel/speed override and a threshold plate
  sized to provide a fair physical crossing window.
- The approved M4 scene variant and loop wiring remain intact but inactive.
- Truth table, canonical recording solution, Echo disappearance, full reset, and
  five consecutive loops pass runtime verification.
- Initial spawn idle is trimmed to a fixed one-second lead-in while meaningful
  pauses later in the route remain exact.

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

## Next

- M5.1: human difficulty/fairness approval for the dual-plate timing room.
- M5.2: one predictable sequential-action puzzle after M5.1 is tuned.
- M5: establish enough reliable normal language before anomalies.
- M6: first restrained horror anomalies.

## Deferred / cut for now

- Sprint, crouch, jump, stamina, inventory, combat, final art, horror director,
  generic anomaly framework, and additional scenes.

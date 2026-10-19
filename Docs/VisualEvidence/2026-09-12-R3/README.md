# VS1-R3 — Hub Return evidence

Unity 6000.3.21f1 / URP, 2026-09-12. Scene:
`Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`.

R2/P3 are creator-approved at greybox level by the R3 brief. R3's spatial
recognition, tension, interpretation and emotional effect remain **EN PRUEBA**.

## Implementation

One scene-specific `VS1HubReturnGate` accepts the assigned real Player's physical
trigger crossing beyond P3 B only while the existing relay has Echo A authorization,
a live Echo B hold and a fully open final gate. No Echo channel or existing puzzle
rule changes. R3 unlock and the distant static curtain persist until a fresh session.
R still goes to R1 and resets P3's readers/gates normally.

The actual original Hub is at z=-8; R1 is a different room at z=27.8. R3 joins
P3's western gallery end to that original Hub via a sealed western perimeter
corridor and one new western arrival. Only the two endpoint walls are replaced;
the originals remain inactive in the scene. The corridor has continuous support,
ceiling and solid perimeter. Its roughly 56 m longitudinal leg is a consequence
of the existing scene layout; walking duration and repetition require human review.

The unlocked R3 exit stays open. Two automatic solid leaves at the Hub form an
exit-only arrival: the rear must close before the front can open. Neither E nor a
puzzle is required. The Hub side cannot supply access to P3/R1, even after R3 opens.
R seals the Hub leaf before allowing another forward traversal. Existing SE1
blackout lights/panels and all event states remain untouched; R3 owns its local
arrival fill using the existing clinical palette.

One distant folded curtain sits four metres behind sealed observation glass.
It is static, colliderless, silent and contains no actor. There are no repeated
domestic frames/runner, new figure, audio, camera changes, teleport or checkpoint.

## Verification results

- `positive.checks.txt`: 96 passing assertions, 16 real R presses, including the
  final five consecutive RECALLs. `positive.csv` logs physical poses and world states.
- `geometry.txt`: 150 assertions; 18,328 floor-support samples, solid doors,
  ceiling/perimeter cross-sections, saved references and the 29 reset targets.
- `topology.txt`: six capsule-grid connectivity cases at 0.20 m resolution and
  0.29 m radius. The actual CharacterController route complements the grid.
- `fresh-physical.txt`: real Player blocked at the fresh Hub arrival and P1's
  north glass-lane boundary; checkpoint revision remains zero.
- `echo-only.txt`: a fresh-session EchoPlayback crosses the final trigger once
  using the unmodified frames from `actual-return-recording.csv`; R3 stays closed.
  The route suite also submits the real Echo collider to the trigger callbacks
  while genuine A/B are qualified, explicitly exercising actor rejection.
- `startup.txt` / `console.txt`: final fresh Play without probes passes; Console
  has zero errors/warnings. Play is stopped and the saved scene is not dirty.
- `preservation.txt`: all 2,980 original scene records retained, 400 added;
  only two original wall-active flags and the SceneRoots list change. All 28
  original gameplay scripts remain byte-identical.

The main suite passed twice. The final run includes label/lighting corrections
and full vestibule occupancy. The early retreat test checks that the Hub leaf
stays closed; it does not prove every possible retreat timing during closure.
The immediate reverse attempt and both interlock connectivity states pass.
Unity closed before the supplementary tests began; the saved project was reopened
and those tests then passed. No result from the interrupted tool request is counted.
The three temporary probes are archived in `Source/` and removed from `Assets/`.

## Captures

[Hub baseline](R3_01_Hub_baseline.png) ·
[R3 closed](R3_02_gate_closed.png) ·
[R3 open](R3_03_gate_open.png) ·
[Hub arrival](R3_04_Hub_arrival.png) ·
[Domestic residue](R3_05_domestic_residue.png) ·
[Reverse blocked](R3_06_reverse_blocked.png) ·
[Continuous western return](R3_07_continuous_return.png).

The closed-gate image is a staged camera composition of its real closed collider:
R1 presentation is briefly enabled and ambient approximates post-SE1, then both
are restored. No Player position, puzzle state or R3 unlock is changed. Other
images come from the input-driven Player route. `cameras.txt` records the distinction.
The old R2 evidence README was absent; its new index links the existing R2 results
without claiming an additional R2 test.

## Method and limits

The route harness seeds the domestic checkpoint and approximates SE1's completed
lighting once, as in R2. After setup it uses actual Input System movement/look/E/R,
CharacterController, real reader qualification, recorder and playback. It does not
force SE2/SE3, puzzle completion, R3 unlock or subsequent Player poses. Full P1/SE1
playback is not repeated; their saved references/configuration are audited.

Automated assertions and camera renders are objective evidence. They cannot
approve fear, recovery, intuitive wayfinding or comfort. Screenshots are unedited
1280×720 renders; camera poses are recorded in `cameras.txt`.

## Human playtest

1. Start fresh. Check the known Hub and its inaccessible western arrival. Complete
   P1/SE1, P2, SE2 and SE3 normally; activate R1's station with E.
2. Record A, move to B and hold the correct facing for about 25 seconds. Press R,
   take the alternate route and cross B while the Echo holds it.
3. Without navigation hints, find the gallery's physical exit and follow it to
   the original Hub. Report hesitation, apparent dead ends and excessive walking.
4. Enter the automatic arrival. Try pausing and retreating before each leaf;
   the passage must feel legible, unobstructed and mechanically quiet.
5. Identify the Hub from its new side, inspect the distant curtain, then try the
   reverse entrance. Press R; confirm R1 remains the retry origin and solve P3 again.

Report separately: circuit closure/recognition, navigation, corridor duration,
arrival-door comfort, residual domesticity, tension and interpretation. R3 should
read as SE3's quiet spatial consequence. Headphones are optional at comfortable
volume; this addition requires no audio.

## Reproduction

The saved scene is the authoritative final configuration. `Source/BuildR3.cs`
is the initial staged Editor authoring command; subsequent label/light/occupancy
corrections are reflected in the saved scene, not a one-click reconstruction.
`AuditR3.cs`, `TopologyR3.cs` and `CaptureClosed.cs` run through Unity RunCommand.
To repeat the route, temporarily import `R3ValidationProbe.cs`, create
`/tmp/echo-vs1-r3/`, enter Play and call `R3ValidationProbe.Run("positive")`.
After the resulting recording exists, import `R3EchoOnlyProbe.cs` and
`FreshR3Probe.cs`, start a new Play session and call their `Run()` methods.
Remove the three probes afterward. The harness uses virtual input devices and
Editor-unfocused input as test accommodations; it does not ship in the scene.

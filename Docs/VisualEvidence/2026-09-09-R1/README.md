# VS1-R1 Recovery Hub — validation and human checkpoint

R1 is implemented in `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`.
Its intended recovery effect remains **EN PRUEBA**, pending human judgment.
The creator's SE2/SE3 validation approves **direction and slice legibility**;
no new fear, narrative or audio outcome was reported.

## Implemented scope

A 6.4 × 5.1 m clinical observation room branches west from the return. Its single
2 m entrance at `z=29` enables two-way access, with no connection around P2 or
Gate B. The original west wall remains the pre-SE3 barrier. A central station,
clinical storage and four steady ceiling fixtures establish a room rather than
another corridor. Each fixture has a downward spot and local diffuse fill so
the room remains readable under the existing post-SE1 ambient blackout.

A sealed west glass wall overlooks one static domestic frame and short runner,
with a low warm source roughly 3 m beyond the glass. The residual root has no
collider, actor or audio. No new audio, imported assets, materials, puzzle,
checkpoint, timer, figure, camera behavior, package or Echo rule was added.
Future chapter reuse is planned, not implemented.

`VS1RecoveryHubGate` accepts the assigned active Player and reads SE3's existing
`Revealed` state through a read-only property. Trigger stay handles the Player
already being in the volume when SE3 completes. It enables floor and containment
before removing the original wall. Opening persists through RECALL for the Play
session; it never changes the domestic origin or opens the P2 gate.

## Validation method and limits

Unity 6000.3.21f1 / URP, September 9, 2026. A temporary input harness seeds the
existing domestic recall transform `(-2.75,0.03,22.05)` once, disables that
checkpoint's trigger to prevent a second boundary, and approximates the already
completed SE1 blackout using its existing light/panel references and ambient
multiplier. The original domestic door remains physically closed. The full
P1/SE1 sequence and portazo are not replayed in this pass.

All later movement, turning and R use virtual keyboard/mouse events through the
existing Input System, FirstPersonController, CharacterController, recorder,
EchoPlayback, optical reader and doors. The harness permits mouse-look while
tools take Editor focus, and routes test devices to the Game view. It does not
write Player poses after setup or force SE2/SE3 completion. The actual Echo's
Rigidbody pose is checked each active LateUpdate against its immutable recording
at elapsed playback time, within `0.01 m / 0.1°`; snapshot identity must remain
identical to `LoopController.LastRecording`.

CSV traces record roughly 10 Hz movement and state. Captures are unedited
1280×720 renders from the actual Player camera, FOV 60°, existing URP settings.
No visual capture proves emotional recovery or perceived safety.

## Results

| Check | Evidence / result |
| --- | --- |
| Real complete P2 → SE2 → SE3 → R1 route | `positive.csv`: one gate opening, no subsequent Player pose writes |
| Incomplete return and R | `incomplete.csv`: cold SE2, unrevealed SE3, original wall and hidden hub |
| No Echo / incomplete P2 | `no_echo.csv`: closed gate blocks at `z=24.55`; R keeps the original baseline |
| Completed SE2, R before SE3 return | `armed.csv`: SE2 persists, SE3 remains AwaitingReturn, hub stays sealed |
| Completed R | Hub, SE3 and completed SE2 remain; domestic checkpoint revision stays 1 |
| P2 and Echo | Player preview and true Echo hold pass; after R without hold the gate stops Player at `z=24.55`; sampled Echo poses and snapshot identity pass |
| Two-way circulation | Player enters, passes both sides of the shortened station, tests glass, exits and re-enters through the same aperture |
| Floor and perimeter | `geometry.txt`: 4,224/4,224 floor samples; 258/258 outer boundary rays; final east replacement wall 87/87 |
| Gate B and P1 north bypass | 48/48 closed Gate B rays and 57/57 fixed north-glass rays; existing P1 serialized configuration unchanged |
| Explicit references | R1 8/8; R1 + SE3 + SE1 + checkpoint audit 66/66, none missing |
| Prior scene preservation | `preservation.txt`: all 2,279 original serialized objects retained; only three parent/child-list records changed; no original object removed |
| Echo / reset code | LoopController byte-identical to task start; the only SE3 script change is a read-only state property |

### Player-camera evidence

Before and after use the same westward yaw `270°`, camera height `1.63 m` and
60° FOV at the north return. Their exact positions are in `camera_poses.txt`;
the before position is `(-2.750,1.630,28.938)` and after is
`(-2.679,1.630,28.965)`, about 7.6 cm apart from actual movement.
The warm frame at the left edge of the entrance belongs to preserved SE3.

![Return before completion](R1_01_return_before.png)

![Return after completion](R1_02_return_after.png)

![Clinical recovery hub](R1_03_hub.png)

![Static domestic frame behind sealed glass](R1_04_residual.png)

![The same doorway leads back to the known return](R1_05_exit.png)

### Corrections and unsuccessful tooling attempts

- The first temporary probe could not attach from an Editor assembly; no route
  pass was claimed. It was moved to a temporary runtime assembly, guarded by
  `UNITY_EDITOR`.
- The first route exposed insufficient north clearance between the station and
  cabinets and timed out. The station was shortened; later complete traversal
  passes. That failed run is not counted as a pass.
- The first screenshots were too dark for the recovery brief. Fixture-local
  diffuse fill now lights walls/ceiling under the real post-SE1 ambient level.
- A preliminary south entrance was physically traversable but visually covered
  by the preserved SE3 frames/lamp. It moved to the clear `z=29` sightline; the
  final positive and incomplete routes were repeated on that saved geometry.
- Two MCP refresh calls lost their dynamic command assembly during a script
  domain reload. Import completed; subsequent commands and Play runs succeeded.
  These are tooling failures, not gameplay validation passes.

The full original P1/SE1 sequence is not revalidated end to end here. Preservation,
references and Gate B/north-glass collision supplement the prepared-checkpoint
route; the human run below starts from the normal scene origin.

## Compile, Console and cleanup

All final successful Play runs compiled and completed without gameplay Console
errors. Unity AI reported its existing Account API timeout warning. Earlier
harness errors and domain-reload tool failures are disclosed above and are not
counted as successful tests. The harness source is archived under `Source/`
for reproducibility; it is removed from Assets and from the saved scene.
Final clean startup passes: hub inactive, original wall active, SE3 AwaitingSE2,
SE2 AwaitingCrossing, checkpoint revision 0, and P1/P2/SE1 enabled. Final Console
contains zero errors and zero warnings. Play Mode is stopped and the scene saved.

## Human playtest — recovery, not another scare

1. Start the scene normally and reach the approved domestic checkpoint through
   P1/SE1. Record about 40 seconds facing the P2 reader, then press R.
2. Follow the Echo-held gate, turn east and read SE2's chamber until its normal
   completion. Return west naturally. Do not search for a new interaction.
3. Enter the cool room at the end of the return, pause and look around. Walk
   around the central station, inspect the distant warm frame through the glass,
   then find the same doorway back to the known return.
4. Press R: the domestic checkpoint remains the origin, SE2/SE3 and the opened
   hub persist, and P2 requires its usual reader hold. In a fresh Play session,
   R before the SE3 return must leave the hub unavailable.

Report separately: whether intensity dropped; whether the room felt distinct
and easy to read; whether its entrance/return were obvious; whether the central
station made the space memorable; whether the distant domestic trace stayed
residual or suggested an unwanted new event. Also judge the SE3-to-hub transition
while turning freely. No explicit safe-room guarantee or narrative explanation
is presented. Headphones, if used, stay at a comfortable volume.

Stop here for that human feedback; R1's recovery quality remains unapproved.

## Files changed by R1

- `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`: Unity Editor authoring only.
- `Assets/_Game/Scripts/Horror/VS1RecoveryHubGate.cs` and Unity-generated `.meta`: new gate.
- `Assets/_Game/Scripts/Horror/VS1OmittedPassageEvent.cs`: read-only `IsComplete` property.
- `README.md`, `Docs/ROADMAP.md`, `Docs/ARCHITECTURE.md`, `Docs/HORROR_DESIGN.md`, `Docs/EXPERIENCE_MAP.md`, `Docs/PLAYTEST_LOG.md`: current state, scoped human evidence and R1 validation.
- This evidence directory: five camera captures, route traces/checks, preservation and geometry audits, and archived temporary probe source.

No commit was created. Existing dirty work was preserved; LoopController's pre-existing
working-tree changes were not changed by R1.

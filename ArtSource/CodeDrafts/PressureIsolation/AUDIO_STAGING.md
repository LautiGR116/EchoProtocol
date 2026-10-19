# Pressure isolation audio — proposed source ownership

Staging only, 2026-10-01. No Unity import, runtime compile, listening, spatial
audition or human fear approval is claimed. No new audio is generated or downloaded.

The plant starts as a local machine that can be heard on approach. Real handle
motion produces quiet friction. Enclavamiento gives one local mechanical stop,
the motor loses speed with `MotorLevel01`, and valve B vents while the Player
can walk. Its source follows actual `Pressure01` and stops when drained. The
machine stays stopped when the Player passes, without muting footsteps, doors,
institutional ambience, mixer groups or the listener.

## Four dedicated sources, explicit references

| Field | Placement / ownership | Existing clip | Initial source gain |
| --- | --- | --- | ---: |
| `motorSource` / `motorLoopClip` | Actual pressure motor; reuse the existing source after Director cedes its writes. Spatial 3D, linear 1.5–18 m. | `Assets/_Game/Audio/Clips/Finish/PressureBed.wav` | .20 |
| `controlSource` / `controlLoopClip` | At the isolator spindle. Spatial 3D, linear 1–4 m; authored pitch .9 is a provisional friction treatment. | `Assets/_Game/Audio/Clips/Mechanisms/SFX_DoorMovement_LP.wav` | .07 |
| `latchSource` / `latchClip` | At the physical isolator stop. Spatial 3D, linear 1–7 m. | `Assets/_Game/Audio/Clips/Finish/Relay.wav` | .13 |
| `purgeSource` / `purgeLoopClip` | At valve B's actual discharge outlet; source does not follow the Player. Spatial 3D, linear 1–7 m. | `Assets/_Game/Audio/Clips/Overhaul/ValveFlow_LP.wav` | .14 × pressure |

All sources: `WorldSFX`, `playOnAwake=false`, `dopplerLevel=0`,
`ignoreListenerPause=false`. They must be distinct. Source transforms, rolloff,
distances and routing are authored by the integrator; the component does not
silently override them. Serialize `isolation` and `session` explicitly.

`PressureIsolationAudio.cs.txt` uses `LateUpdate` order 200 after the proposed
isolation order -90. It observes the published state; it neither completes nor
resets the isolation. `CommitCount` gives one latch per live success. A changed
`RestoreRevision`, Awake/Start or enable establishes a baseline without replaying
the latch; `RestoreState(true)` leaves all four sources stopped. Ordinary pause
uses the existing listener pause and does not restart source cursors.

The scene opts in by disabling old `PressureWorkAudio` and the old station
feedback. Its Director branch must skip the old `UpdatePressure` writer and its
machinery playback mutations. Do not assign the institutional machinery bed,
Player foot source or shared campaign cue pool to these dedicated references.
Existing scenes without the opt-in branch retain their old components.

## Provenance and selection limits

- `PressureBed`: derivative of the existing AlaskaRobotics machine recording,
  [Freesound 221566](https://freesound.org/people/AlaskaRobotics/sounds/221566/),
  CC0; exact imported hash/edit in `ArtSource/Audio/Finish/derivatives.json`.
- Control friction and latch: Rolly-SFX,
  [Sliding Door Open 626165](https://freesound.org/people/Rolly-SFX/sounds/626165/),
  CC0. The movement/endpoint selections were approved for the earlier domestic
  door; this new hand-control treatment is not human-approved. Relay is the
  0.6-second derivative recorded in the same Finish manifest.
- Purge: JohanHauskater,
  [gas_leak_3_samples 855703](https://freesound.org/people/JohanHauskater/sounds/855703/),
  CC0; source rights snapshot and exact 6.2-second loop edits/hashes in
  `ArtSource/Audio/Overhaul/`. The retained source file is an MP3 preview, not the
  original lossless recording. This role makes no voice or Echo-channel claim.

`ValveDischarge.wav` is deliberately not part of this sequence: its 4.25-second
tail would outlive the proposed three-second drain. The existing looping gas
source can follow the physical pressure instead. Source gains and pitch are
initial authoring values, not measured output loudness or a timbre verdict.

## Required runtime observations after integration

1. Fresh start / approach: motor plays locally; no latch or purge. Partial grip
   and release sound only while the real handle moves. No Echo is required.
2. Paused grip and paused purge preserve listener-paused cursors; no new start
   counts or latch. Release/document/R cancellation follows the physical state.
3. One valid hold: one latch, motor reaches zero/stops, exactly one purge start
   at B, then stopped purge at drained state. Gauge/door release remains owned
   by the isolation component. Inspect actual source positions and distances.
4. Advance out of the room and RECALL: the isolated plant cannot restart. Check
   `MachineSourcesStopped`, not a mixer mute; unrelated Player effects remain
   available. A later chapter restore must not play a latch or purge.
5. `RestoreState(true)`: zero loop/one-shot playback after observation baseline;
   `RestoreState(false)` may resume the physically running motor but invents no
   prior latch. Confirm no competing legacy source writer remains.

These are future runtime checks, not results. A comfortable-volume human pass
must judge whether the door-derived friction reads as a hand control, whether B
localizes during walking, and whether the subtraction feels consequential.

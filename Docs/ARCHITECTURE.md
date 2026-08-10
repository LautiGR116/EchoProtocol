# Architecture

This document records only implemented or currently approved systems.

## First-person player

The `Player` GameObject composes:

- `CharacterController`: collision-constrained, grounded capsule movement without
  Rigidbody force simulation.
- `PlayerInput`: enables the existing `Player` action map from
  `Assets/InputSystem_Actions.inputactions`.
- `FirstPersonController`: reads `Move` and `Look`, applies gravity and movement,
  rotates the player body horizontally, pitches the child camera vertically, and
  manages cursor capture.
- `LoopActor`: identifies this physical actor as the Player for temporal puzzle
  triggers.
- Child `Main Camera`: the player's view and AudioListener.

The controller exposes speed and sensitivity in the Inspector so movement feel
can change without editing code. Mouse delta is applied without multiplying by
frame time because it already represents movement accumulated for that frame.

## Interaction

- `PlayerInteractor` is a separate component on `Player`. It reads
  `Player/Interact`, casts a three-metre ray from the centre of the child camera,
  and invokes the first `IInteractable` found in the hit collider's ancestry.
- The ray uses the first physical hit across normal world layers, so walls and
  other colliders block interaction instead of being skipped.
- `IInteractable` receives the interacting GameObject, leaving room to
  distinguish Player and Echo actors later without introducing a framework now.
- `PrototypeToggleSwitch` is a greybox validation target. It flips a lever and
  toggles an indicator; it is not the future door or puzzle-state architecture.

`PlayerInteractor` does not activate or deactivate `PlayerInput`; the existing
player input lifecycle remains owned by the player setup.

## Temporal recording

- `EchoRecorder` lives on `Player`. It captures the player's world position and
  body rotation in `LateUpdate` at a `0.05` second interval, after locomotion has
  run. A fresh attempt is armed but does not accumulate arbitrary idle time at
  spawn. Horizontal movement beyond `0.02` metres starts a fixed one-second
  lead-in and the 60-second bounded timeline. Pauses after that point remain
  part of the recording, including pressure-plate dwell.
- `EchoFrame` stores one timestamped pose. `EchoRecording` copies completed
  frames into an immutable snapshot and samples by timestamp with position and
  quaternion interpolation.
- Starting the next recording clears only the recorder's mutable buffer; it
  cannot modify the snapshot already used by playback.

## Loop reset

`LoopController` reads `Player/ResetLoop` from the `PlayerInput` action copy. A
press of `R`, or reaching the recording limit, performs this ordered lifecycle:

1. Finish and snapshot the current attempt.
2. Deactivate and schedule destruction of the previous Echo.
3. Restore each explicitly referenced `ILoopResettable` to its baseline.
4. Reset player position, yaw, camera pitch, and vertical velocity at
   `Player Spawn`.
5. Instantiate and initialize the new Echo from the completed snapshot.
6. Arm a fresh recording at the spawn; its timeline starts on first movement.

After all six steps succeed, `LoopController` emits `LoopCompleted`. Failed
RECALL attempts without a started recording emit nothing.

The current scene explicitly resets two pressure plates, their sequential
condition, the sliding door, its audio state, and the exit goal. The earlier
`PrototypeToggleSwitch` remains available as an isolated
interaction proof but its scene group is inactive and it is not part of the
current puzzle baseline. The loop keeps only the latest recording and one Echo.
It does not reload the scene or take ownership of the shared `PlayerInput`
lifecycle.

## Echo playback

`PF_EchoGreybox` is a translucent primitive capsule with a forward marker. It
uses a kinematic Rigidbody, trigger CapsuleCollider, and Echo-kind `LoopActor`,
so it is nonblocking but can occupy temporal triggers. `EchoPlayback` snaps to
frame zero, advances the immutable recording in `FixedUpdate`, applies the exact
final pose, and disappears predictably when playback ends.

The current recording reproduces player movement and body yaw only. Semantic
interactions and a separate visible head/camera pitch track are intentionally
deferred until a concrete puzzle requires them.

## First temporal puzzle

- `PressurePlate` accepts colliders belonging to Player- or Echo-kind
  `LoopActor` components. It tracks colliders separately while counting unique
  actors, so one actor with multiple colliders cannot release the plate early.
- Actor-disable notifications, trigger-stay repair, and a defensive physics-step
  purge prevent a disappearing Echo or missed trigger exit from leaving stale
  occupancy.
- `SlidingDoor` owns the kinematic moving panel and its physical collider. The
  plate references this one door directly and requests it open only while at
  least one valid actor remains.
- `PrototypePuzzleGoal` is a latching trigger behind the door. It accepts only
  the Player, ignores Echoes, returns to incomplete on loop reset, and emits one
  rising-edge `Completed` event when the Player first enters.

This is intentionally a concrete one-plate/one-door contract, not a generic
signal graph. More expressive puzzle wiring is deferred until another room
requires it.

## Dual-plate timing condition

M5.1 makes a plate's direct door reference optional and adds one concrete
`DualPlateDoorCondition`. Its two plates remain normal physical sensors; the
condition reads their public pressed states and requests its door open only for
`plateA && plateB`. It also drives two independent status lamps and implements
`ILoopResettable`.

The approved M4 variant retains its original direct one-plate/one-door reference.
The M5.1 plates deliberately leave that reference empty, making the AND
condition the sole logical owner of their door. This is not a general boolean
graph, UnityEvent network, or timer framework.

## Sequential plate condition

M5.2 adds one `SequentialPlateDoorCondition` with three explicit phases:
`AwaitingFirst`, `AwaitingSecond`, and `HoldingOpen`. A rising press on plate A
arms the second phase and its lamp. Only a later rising press on B completes the
sequence; B pressed before A, held while A activates, or pressed in the same
physics step is ignored until it is released and pressed again.

There is no sequence timeout. Once valid, the condition requests its door open
only while B remains occupied. Releasing B, disabling its actor, or resetting the
loop returns to `AwaitingFirst`, clears both indicators, and closes the door. The
two plates have no direct door reference, leaving the condition as the only
logical owner. A low scene blocker lengthens the B-to-door route so the Player
cannot replace Echo cooperation with a closing-door race.

## First environmental discrepancy

M6.1 adds one `EnvironmentalDiscrepancyController` with explicit references to
the active loop, Player-only goal, and one initially inactive chair root. The
goal's rising event arms the controller; the next successful `LoopCompleted`
event reveals the chair synchronously after reset and Echo initialization.

The controller is active while its visual child is hidden. It does not implement
`ILoopResettable` and is not listed among puzzle reset targets, so the revealed
state persists through later RECALLs until the scene or Play Mode session reloads.
The chair is built from render-only primitives with no collider, Rigidbody,
`LoopActor`, interaction, light, or puzzle reference. It cannot change
pathing, timing, occupancy, goal eligibility, or recorded Echo frames.

`ClinicalRoomTone` shares the always-active controller GameObject and owns one 2D
`AudioSource`. AD2 replaces its runtime-generated placeholder with the serialized
stereo `AMB_FacilityAir_LP` asset and loops it at volume `0.14`. It listens only
to the discrepancy's rising `Revealed` event,
fades linearly to zero over `0.8` seconds, stops the source, and remains silent
through later RECALLs. A Play Mode or scene restart restores the initial bed. It
never destroys the imported asset and does not touch listener, mixer, global
volume, time scale, puzzle reset list, or Echo lifecycle.

## Normal audio routing

M6.2 adds `Assets/_Game/Audio/MX_Main.mixer` with one default snapshot and three
groups: `Master`, `Ambience`, and `WorldSFX`. All groups remain at their default
level with no effects beyond Unity's attenuation stage and no exposed
parameters. The M6.1 room tone routes to `Ambience`; its component still owns the
same source-level volume and fade without mutating mixer or listener state.

The active M5.2 door root owns one 3D `AudioSource` routed to `WorldSFX` and one
`SlidingDoorAudio`. It references the fixed door root, its kinematic moving body,
the user-selected domestic `SFX_DoorMovement_LP`, and the matching
`SFX_DoorEndpoint` asset explicitly. It no longer creates or destroys runtime
door clips.

`SlidingDoorAudio` runs after the door controller and compares consecutive
Rigidbody positions. It starts the motor only after physical displacement,
keeps the same voice through a direction reversal, and plays one endpoint cue
only after real movement reaches fully open or closed. Repeated `SetOpen` calls
cannot restart it. As an explicit `ILoopResettable` target placed after the door,
it stops immediately after a reset snap and suppresses a false endpoint cue.
It owns no randomization, gameplay decision, global manager, or Echo data.

AD2 adds three truthful functional sources without a global audio manager:

- each active M5.2 plate owns a `PressurePlateAudio` and one 3D `WorldSFX`
  source. `PressurePlate.PressedChanged` emits only on a real occupancy edge;
  forced visual reset emits nothing. Each audio component follows its plate in
  reset order and stops silently;
- `RecallCompletionAudio` owns one 2D `WorldSFX` source and subscribes only to
  successful `LoopController.LoopCompleted`; invalid RECALL remains silent;
- `Facility Ambience AD2` owns a 2D fluorescent loop and a fixed 3D distant
  machinery loop routed to `Ambience`. Both persist across RECALL while the
  subtractable facility-air layer keeps its existing reveal fade.

The three ambience masters use `Compressed In Memory` / Vorbis quality `70`.
Door, plate, and RECALL clips use `Decompress On Load` / PCM. All are preloaded,
preserve `48 kHz`, and have background loading, ambisonics, and forced mono
disabled. Three-dimensional masters are already mono.

## First localized audio discrepancy

M6.3 adds one `ChairSettlingCue` on an `AudioSource` child of the initially
hidden M6.1 chair. Because the chair root owns the source, the component cannot
run before the existing reveal. Once active, it compares only the Player's
planar position against a `1.3 m` radius and plays one deterministic `0.9`-second
mono cue on the first Player proximity.

The source is fixed `0.65 m` above the chair, routes to `WorldSFX`, and uses full
3D blend with linear attenuation from `1.25` to `8 m`. The component generates
and owns one runtime clip, exposes its one-shot state for verification, and
never rearms. It is not an `ILoopResettable`, has no collider or Rigidbody, and
does not subscribe to RECALL, modify the chair, or touch puzzle and Echo state.
If RECALL occurs during playback, the persistent world-space cue finishes from
the chair without restarting.

## Development scene

`Assets/_Game/Scenes/Development/EchoPrototype.unity` is an isolated primitive
greybox used for mechanical validation. It retains the approved M4 and M5.1
rooms and loops as inactive variants. The active M5.2 variant places two plates
on the recording side of a low blocker, keeps two ordered status lamps over one
door, and retains the Player-only goal behind the partition. The active M6.1
discrepancy layer adds its hidden chair near the spawn-side left wall. Only one
puzzle and loop pair is active at a time. M6.2 routes its room tone and active
door through the scene-local mixer; AD2 adds persistent fluorescent/machinery
beds, plate edges, and successful-RECALL feedback; M6.3 retains the inactive
chair-owned procedural source.
Inactive M4 and M5.1 doors have no audio component. The earlier movement and
switch proofs also remain in inactive scene groups. Template URP settings remain
under `Assets/Settings`.

## Not implemented

There is no semantic interaction replay, multi-Echo puzzle, generic puzzle-signal
framework, generic anomaly director, additional visual discrepancy, save system,
global game manager, footstep system, production RECALL signature, music,
voice, reverb, occlusion, or adaptive audio system.

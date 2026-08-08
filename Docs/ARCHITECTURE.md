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

The current scene explicitly resets its pressure plate, sliding door, and exit
goal. The earlier `PrototypeToggleSwitch` remains available as an isolated
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
  the Player, ignores Echoes, and returns to incomplete on loop reset.

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

## Development scene

`Assets/_Game/Scenes/Development/EchoPrototype.unity` is an isolated primitive
greybox used for mechanical validation. It retains the approved M4 room and loop
as inactive variants. The active M5.1 variant adds a second threshold plate and
two condition lamps around the same partition, door, and Player-only goal. Only
one puzzle/loop pair is active at a time. The earlier movement and switch proofs
also remain in inactive scene groups. Template URP settings remain under
`Assets/Settings`.

## Not implemented

There is no semantic interaction replay, multi-Echo puzzle, generic puzzle-signal
framework, anomaly system, save system, or global game manager.

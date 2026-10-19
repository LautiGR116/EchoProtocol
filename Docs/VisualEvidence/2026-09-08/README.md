# SE2 architectural discrepancy — objective evidence

Tests ran September 8–9, 2026 in Unity 6000.3.21f1, URP, in
`VS1_ParallelReturn_Blockout.unity`. The session exposes 22 Unity MCP tools.
Status: implemented and objectively verified; human approval pending.

## Method and limits

A temporary Editor harness seeded the configured domestic recall point once at
`(-2.75, 0.03, 22.05)` and prepared post-SE1 lighting. It then drove virtual
keyboard/mouse input through the actual Input System, FirstPersonController,
CharacterController, recorder, reader, gate, and `R` action. There were no
subsequent harness pose writes: returns were actual LoopController RECALLs.
The P1/SE1 route and portazo were not replayed as part of these focused tests.

The positive route recorded an 18-second hold facing the P2 reader, pressed R,
waited for the truthful Echo to open the gate, walked north, turned east, and
continued facing `90°` to the sealed glass. It did not aim separately at the
figure. The isolated invalid-crossing test used one disclosed downstream start
pose; it is not normal-route evidence. The harness was removed after testing.

## Results

| Check | Observed result |
| --- | --- |
| Normal route | Reveal at approximately `(1.4, 0.03, 29.03)`, yaw `90°`, with live Echo hold |
| Cold baseline | Approximately `0.65 s` of clear view before the whole-room switch |
| Read duration | `3.2 s` of qualified visibility before Complete; no figure withdrawal |
| Look away | Reading stayed exactly `1.0129 s` for the one-second backwards look, then resumed |
| Incomplete R + retry | R at approximately one second restored cold/hidden figure, zero read time and closed P2; a new recorded hold and normal crossing completed successfully |
| Completed R | Warm room and figure persisted at Complete; no new appearance/completion |
| No Echo, normal route | Closed P2 blocked the Player at `z=24.55`; event stayed dormant |
| No Echo, downstream trigger | Player walked across the trigger to `x≈7.41`; no acceptance or reveal |
| Containment | Continuous floor support along both legs; CharacterController stopped before solid glass |
| Echo fidelity | Playback retained LastRecording by reference; one post-reveal Rigidbody sample matched recorded position/yaw with zero error |
| Ownership | No SE2 AudioSource, domestic collider, interaction or Echo-renderer ownership |
| Compile/Console | No compilation or gameplay errors in successful runs; Unity AI Account API timeout was an Editor warning |

An additional September 8 harness run stopped before SE2 because its synthetic
R press was not registered. That run is not counted as passed. The September 9
repeat passed without changing gameplay/input code; the missed press's cause
was not established.

CSV traces retain state/step/loop/hold transitions and approximately 10 Hz
position samples. Full frame traces were temporary test output. These checks
prove delivery and state behavior, not spontaneous human detection or fear.

## Real Player-camera frames

The PNGs are unedited 1280×720 URP renders from the actual Player camera during
the input-driven route, using its configured field of view and post-processing.
The native Game View was also inspected.

- [Cold chamber](SE2_01_cold.png): initial unobstructed frontal view after the turn.
- [Reading while advancing](SE2_02_reading.png): camera approximately
  `(4.63, 1.63, 29.03)`, yaw `90°`, pitch `0°`, at `0.8 s` of qualified reading.
  The glass frames an amber room centrally, with an armchair left, lamp/console
  right, runner, nested doorframes and a backlit human silhouette.
- [Completed at sealed glass](SE2_03_complete.png): stationary warm consequence;
  fine horizontal glass markings remain visible even when the outer frame
  leaves the near-camera field of view.

## Human test

1. Open `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`
   and enter Play. Complete the existing P1/SE1 route to the domestic checkpoint.
2. Move into the P2 reader area, face its east-wall lens until it turns cyan,
   and hold that orientation for 18–20 seconds. Press `R`.
3. Let the Echo hold the reader. Cross the open north gate, follow the passage,
   turn right/east and walk forward normally toward the glass.
4. Judge whether the cold-to-domestic change is unmistakable without searching,
   whether the person reads immediately, and whether it produces spatial doubt
   or discomfort rather than merely looking like a decorated room.
5. To test incomplete RECALL, press R within roughly one second of the reveal:
   it should restore cold state and allow another recorded attempt. After a
   full visible reading, R must preserve the warm room and figure. A fresh
   Play session restores the cold baseline.

Also judge glass accessibility, crossing comfort, duration and overall visual
coherence. This event adds no sound and its approval must not depend on volume.

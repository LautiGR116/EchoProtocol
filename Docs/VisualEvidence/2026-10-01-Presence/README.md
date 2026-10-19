# Presence — recorded body presentation

2026-10-01. Separate `EchoProtocol_Presence.unity`, derived from Reserve.
The selected surface uses the same rigid body and immutable position/yaw take
throughout the campaign. Interior shading and stable coverage replace the
edge-dominated legacy surface. A same-mesh depth helper removes self-overlap
patches; no light, sound, collider, shadow or recorded channel is added.

## Visual selection and scope

- `candidate01-compare`: no self-depth; rejected for hard head/axilla/leg patches
  in SE3. Its continuous route passed434 checks, but that is not visual acceptance.
- `candidate02-compare`: self-depth fixed those patches. The .62 surface remained
  too weak and brown over the warm background. Continuous route PASS436,
  13,992 playback frames, four actual takes, zero gameplay fixtures.
- `surface-candidate02-variants`: explicit cold/warm pose/checkpoint fixtures and
  real W/hold/R takes, then paused camera/material comparisons. Initial warm
  pictures hit a closed P2 gate; preserved separately and not used for selection.
  `WarmPresenceVariants.cs.txt` explicitly opens that gate through its restoration
  API. All variants otherwise retain the same actual take, pose, lights and camera.
- Selected preset: tint(.14,.68,.64), alpha.90, relief.32; edge adds only.03 alpha.
  No animated glow or new channel. Selection is an authored visual judgment,
  not human first-play approval or proof of fear.
- `surface-selected`: **63 focused checks**; actual input produces both takes.
  Explicit pose/checkpoint/lighting/P2-gate fixtures are stated in the log.
  Near approach retains the game postprocessing. Fully faded enabled/disabled
  surfaces have identical RGB values with stochastic postprocessing disabled
  only for that numeric comparison. Front/rear and real-partition views included.
  The initial postprocessed comparison failed on small full-image noise and is
  preserved in `surface-initial-postfx-check`; it was not silently accepted.

The self-depth approximation rejects transparent geometry behind the body;
opaque backgrounds still blend through its alpha. It is not full order-independent
transparency. Front glass and opaque occlusion are inspected in route captures.
Camera.Render images omit OnGUI; Game images from the route include it.
Rigid locomotion and the display's stylized relief remain visible limitations.

## Final verification

Selected-preset continuous route **PASS436**, 276.97 s, four actual immutable
takes, **12,142 playback frames**, zero gameplay fixtures and unexpected pause
resumes. Console: zero errors, four warnings; both shader message arrays empty.
`continuous` and `selected-compare` preserve this run. These are automated-route
and camera checks, not a human duration or approval. The mesh has eight
submeshes/material slots, mirrored in the depth helper; this is not a one-draw
claim or a hardware performance benchmark. Universal macOS build **build_68bd4071a13d** succeeded: 197,322,541 bytes,
31.60s, zero errors,346 warnings from Pipeline-disabled/Sentis inference shader
variants. Both architectures and local signature verified. **Native launch/input
PENDING:** CUA reported the Mac locked and could not unlock it. No launch or
Player.log claim is made. Reserve remains the latest natively checked recovery app.
Human first-use recognition, motion comfort, immersion, listening and fear remain
**EN PRUEBA**. No subjective playtest was entered in PLAYTEST_LOG.

# IsolationControl — original manual machinery

Original project geometry and simple PBR values, authored 2026-10-01 in Blender
5.2 LTS. The module isolates the present machine. It does not show activity
recorded by Echo. No third-party model, image, texture, font, paid service or
add-on is used. No text, units, numbers, light, sound, collider or animation is
exported. State labels and runtime motion belong to Unity integration.

## Files and assembly

Rebuild with `blender -b --python build_isolation_control.py`. The standalone
source is `EchoIsolationControl.blend`; its three assets are separated along X
for editing. Every FBX is exported at its individual origin before preview
lighting is created. `manifest.json` is the numeric source of truth.

| FBX | Meshes | Origin and purpose |
| --- | --- | --- |
| `exports/EP_IC_IsolationPanel.fbx` | `IsolationHousing`, `IsolationLever` | Centre of control, intended at world height 1.500 m. Static cabinet plus moving lever. |
| `exports/EP_IC_ControlPedestal.fbx` | `PedestalStructure`, `PedestalConduit` | Same origin/transform as panel; bottom meets source Z −1.500 m. |
| `exports/EP_IC_PressureGauge.fbx` | `PressureGaugeHousing`, `PressureNeedle` | Independent gauge, XZ dial centre at origin, rear mounting plane Y 0. Reusable for another pressure point. |

Enclosure nominally .50 m wide × .65 m high × .15 m deep. Hinges, gland,
spindle and handle extend the bounds to about .510 × .257 × .693 m. Pedestal
base is .40 × .33 m. Gauge bezel diameter is .208 m; including its connected return tube,
height is .26833 m. Complete pack: **7,656 triangles**.

For the assembled module, keep panel and pedestal roots coincident. Put the
gauge root at `IsolationPanel__GAUGE_MOUNT`, source **(−.120, −.086, .203)**,
with the same orientation. Gauge mounts into the cover; no separate backboard
is needed. The pedestal's conduit meets the panel gland and terminates at the
foot; it can be hidden separately for wall mounting or another cable route.

## Axes and import

Metres; source **X width, −Y front, Z up**. FBX exports use −Z forward/+Y up.
The project has observed Unity's raw import as `(−X, Z, −Y)`. Negate the
imported root's X scale **once**, keep its other import magnitudes/rotation,
and compose the placement rotation with the imported rotation. Corrected
source-to-Unity coordinates are **(X, Z, −Y)**, nominal front Unity +Z.

Every asset includes `ROOT`, `AXIS_X/Y/Z`, `FRONT_BASE/TIP` anchors. Use marker
world-position differences when checking directions: `Transform.forward`
ignores negative scale. For animated pieces, the source axis and numeric
angles below are accompanied by authoritative endpoint anchors. Verify their
actual direction once in Unity rather than deriving a sign from the mirrored
root alone.

## Lever motion

`IsolationLever` is one moving mesh with a true pivot at source
**(−.120, −.150, −.080)**. Its export rotation is zero and its geometry is
already in **MARCHA**, the upper position. Animate this mesh only; housing,
spindle, gauge and support remain static.

- Source rotation axis: **+Y (0, 1, 0)**.
- MARCHA: **0°** relative to export rest.
- AISLADO: **+70°** relative to export rest, suggested duration **1.8 s**.
- Grip reference radius: .300 m. Its centre drops .344146 m over the stroke.
- Exact positions are exported as `LEVER_GRIP_MARCHA` and
  `LEVER_GRIP_AISLADO`; both are static anchors under the asset root.
- `LEVER_PIVOT` and `LEVER_AXIS_POS` document the axis. The moving dog travels
  between the two fixed metal stop pins.
- `LABEL_MARCHA`, `LABEL_AISLADO` and `SERVICE_LABEL` mark the front surfaces of
  the blank label zones. The lower label sits left of the isolated grip so its
  text can remain visible. No words are supplied in the model.

Use a scalar interpolated angle relative to the captured rest rotation. State,
interruption, timing and interaction remain runtime responsibilities.

## Pressure gauge motion

`PressureNeedle` is a separate mesh whose pivot is source
**(0, −.073, 0)**. It is exported at **HIGH**, pointing down/right 45°. The
ivory face, dark geometric ticks, brass connector and open metal bezel are in
`PressureGaugeHousing`. There is no transparent pane to obscure the needle.

- Source rotation axis: **+Y (0, 1, 0)**.
- HIGH: **0°** relative to export rest.
- ZERO: **−270°** relative to export rest; the blade travels across the **upper
  scale**, ending down/left 45°.
- Suggested pressure decay: **3 s**, driven by actual machine pressure.
- Interpolate the scalar angle, **not** `Slerp` between endpoint quaternions:
  shortest-path quaternion interpolation would traverse the lower 90° gap.
- `NEEDLE_PIVOT`, `NEEDLE_AXIS_POS`, `NEEDLE_TIP_HIGH/ZERO`, `FACE_CENTRE`,
  `BACK_MOUNT`, `PRESSURE_PORT` and `UNIT_LABEL` are exported anchors.

The gauge scale is intentionally unnumbered. Unity can provide existing
pressure labels without the model inventing units, readings or other activity.

## Materials, UVs and evidence

Every mesh has a finite `UVMap`, generated by Smart UV Project, and explicit
material slots. No texture is required. Materials are `IC_Enamel`,
`IC_DoorEnamel`, `IC_EdgeMetal`, `IC_DarkMetal`, `IC_Bakelite`, `IC_Rubber`,
`IC_DialIvory`, `IC_DialInk`, `IC_Needle` and `IC_Brass`. Numeric base colour,
roughness and metallic values are in the manifest. There are no procedural
preview-only surface textures; visible wear is sparse modelled edge exposure.

`previews/` contains complete front/oblique/rear assembly, control detail,
HIGH/ZERO gauge close-ups, and MARCHA/AISLADO views from 1.5 m with 60° vertical
FOV at 1280×720. These are Blender studio renders, not Unity screenshots or
human approval. The latter views test silhouette and angular displacement,
not final gameplay lighting or label readability.

`validate_isolation_control.py` and `roundtrip-checks.json` document independent
FBX reimport, metric bounds, exact mesh/material names, UVs, pivots and anchors,
motion endpoints and unobstructed front rays. Unity motion, interaction,
collision and a human visual/readability pass remain integration work.

Final result: **3/3 PASS**, 7,656 triangles, 273,092 FBX bytes. Maximum bounds
error is below 0.00000025 m; motion endpoint errors are below 0.00000006 m.
Front rays first hit the lever/needle in both endpoints; the lower state label
remains exposed in both lever states. Pedestal floor contact is Z −1.500 m.
The validator reuses the existing RecoveryOffice validation core from the
adjacent project source folder. The builder itself is standalone.

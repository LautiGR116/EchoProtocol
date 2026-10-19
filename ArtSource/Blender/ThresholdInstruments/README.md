# Threshold instrument housings

2026-10-01. Two original, static Blender 5.2 LTS props around the existing
Threshold controls. No new screen, button behavior, text, pilot, sound, state or
recorded channel is introduced. The existing Unity interaction and light remain
authoritative. No third-party model, texture, font or paid dependency was used.

## Files and dimensions

`build_threshold_instruments.py` builds `EchoThresholdInstruments.blend`, two FBXs,
the manifest and studio previews. All generated files remain in this directory.

| FBX | Meshes | Body envelope, source width × depth × height |
| --- | --- | --- |
| `EP_TI_WallReviewHousing.fbx` | `WallHousing` | .88 × .10 × .68 m |
| `EP_TI_DeskPreserveHousing.fbx` | `DeskHousing`, `OptionalRearStand` | .38 × .12 × .34 m body; about .38 × .38 × .394 m including stand |

**Origins are at the existing body centres**, not on the floor/table. The wall
unit has a formed rounded shell, captive corner screws, actual recessed central
face, gasket and restrained worn edges. Its full central face is unobstructed.
The desk unit uses the same material/edge language at a smaller scale. An open
metal bezel provides a seat behind the current pilot. No lens or emissive polygon
is exported over that pilot.

The separate rear stand is optional. It extends behind the desk face, with rubber
feet at source Z=−.224. At the existing world origin its contact is approximately
(2.355, .856, −82.6), near the nominal main counter surface. Check the actual
tabletop in the scene and adjust/hide `OptionalRearStand` only if necessary. The
body and existing title/pilot can retain their positions. The support introduces
no required collider or behavior.

## Unity placement and known mirror correction

Source: Blender X width, **−Y front**, Z up; FBX −Z/+Y export. Current Unity
import settings from PressurePlant (`bakeAxisConversion: 0`) yield **(−X,Z,−Y)**.
Negate the imported root's `localScale.x` **once**, preserving all imported scale
magnitudes and the importer rotation. Apply `placementRotation * importedRotation`.
This produces the assembly contract **(X,Z,−Y)**. Do not flatten the importer
rotation or assume that a frontal view proves side coordinates.

For these two existing Threshold bodies, use **yaw 270°**, after the correction,
so source front points toward world −X:

| Prop | Existing world origin |
| --- | --- |
| WallReviewHousing | (2.35, 1.45, −75.7) |
| DeskPreserveHousing | (2.12, 1.08, −82.6) |

Hide the old box renderer while retaining the existing interaction/collider and
explicit references. Use the `TI_*` material slots and PBR values in the manifest.
Every final mesh has packed UVs. No additional lightmap UV channel is authored.

## Explicit anchors

Empties are exported, with source positions in `manifest.json`. Their positions
provide integration checks; they are not new interactive objects:

Names are prefixed by their asset, for example
`DeskPreserveHousing__ANCHOR_EXISTING_PILOT`, to avoid Blender's automatic `.001`
renaming when both sources are open. `anchor_aliases` maps the short names below
to exact exported names.

- `ANCHOR_EXISTING_TITLE` on both props matches each existing label plane/centre.
- `ANCHOR_EXISTING_INSTRUCTIONS` on the wall matches the current instruction copy.
- `ANCHOR_EXISTING_PILOT` on the desk matches the current pilot's centre.
- `ANCHOR_PILOT_FRONT` matches the front of that existing .014 m deep pilot.
- `ANCHOR_TABLE_CONTACT` marks the optional stand's nominal contact height.
- `AXIS_X/Y/Z` each sit .10 m along their source axis.
- `FRONT_TIP − FRONT_BASE` provides a .10 m outward vector.

Read marker positions with `TransformPoint` / world position differences.
Avoid relying only on `Transform.forward` under a mirrored parent, because a
rotation-only direction can omit a negative scale. Both bodies keep all new
geometry behind the existing text/pilot planes. Do not cover them with a new
glass or screen material.

## Evidence

Studio views in `previews/` include plain oblique props and front views with
**preview-only copies** of current labels/pilot at their serialized positions.
Those copies are not exported or saved in the source asset file. They verify
spatial clearance; they do not claim Unity text readability or human approval.

`validate_threshold_instruments.py` reimports both FBXs and checks metric bounds,
finite UVs/vertices, material slots, separated meshes, exact anchor positions and
clearance in front of the body for the existing text/pilot. Results and hashes are
in `roundtrip-checks.json`. Unity still needs to verify the X correction, normals,
actual pilot visibility, interaction hit, tabletop support and camera composition.

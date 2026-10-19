# Domestic floor lamp

Original project model, Blender 5.2 LTS, 2026-10-01. Made for SE2, reusable in the
house. No external models, textures, photographs, fonts, services or addons.
No Unity scene, material, light, collider or gameplay script is authored here.

`exports/EP_DL_FloorLamp.fbx` is a static assembly with floor-centre origin.
The lamp is approximately 1.616 m tall, with a .47 m lower shade diameter and
.33 m shade height. A weighted .33 m base sits on three rubber pads. The turned
timber stem, brass ferrules and low textile shade give an ordinary domestic
silhouette without enlarging it to fill a distant frame. Exact dimensions and
counts are in `manifest.json`.

Final FBX reimport: **PASS**, six meshes, 14,272 triangles, 390,828 bytes.
The core measures .4688 × .4688 × 1.616 m. With cord/socket, the assembly extends
to the rear wall; that extra depth is not the lamp's body size.

## Components and placement

| Mesh | Role |
| --- | --- |
| `LampBody` | Base, pads, stem, socket, switch, harp and shade spider |
| `ShadeOuter` | Linen exterior, bindings and rear seam |
| `ShadeInner` | Separate inward-facing diffuser lining |
| `OpalBulb` | Bulb beneath the harp, separate emission material |
| `CordToRearWall` | Floor-contact cord and fitted bakelite plug |
| `WallSocket` | Optional surface socket plate with a defined wall contact |

The cord runs to a wall **.900 m behind the lamp centre** (Blender +Y), with its
socket centre at .272 m above the floor. Rotate the full assembly to place that
wall behind/alongside the lamp. `FloorLamp__WALL_CONTACT` is the rear plate plane;
`WALL_NORMAL_TIP - WALL_CONTACT` points out of the wall. Do not leave the socket
floating. Cord and socket can be hidden/replaced together if the actual room
requires another route; the main lamp's footprint is reported separately.
Do not scale the lamp taller to solve distant visibility.

`FloorLamp__FLOOR_CONTACT` marks floor height; `BULB_SOURCE` at source
(0,0,1.451) is the nominal light origin. `LIGHT_DOWN_TIP - BULB_SOURCE` gives down.
`SHADE_CENTRE`, `CORD_EXIT`, source axes and `FRONT_BASE/TIP` are also exported.

## Axis, surfaces and light

Metres. Blender X is width, −Y nominal front, Z up. FBX −Z/+Y export uses the same
convention as PressurePlant and ReceptionLife. The observed Unity importer maps
source to (−X,Z,−Y). Negate the imported root's localScale.x **once**, preserve
imported scale magnitudes and rotation, then apply placementRotation times
importedRotation. The corrected assembly map is (X,Z,−Y), front Unity +Z. Use
marker-position differences for directions under negative scale.

All meshes have packed UVs and named `DL_*` PBR materials; values are in the
manifest. The source linen has a subtle procedural preview bump; it is **not
baked or exported**. The modeled hollow shade, thickness bindings and supports
are present in the FBX. No texture files are required.

Exported emission starts **off**. Bind `DL_InnerDiffuser` and `DL_OpalBulb`
emission to the real practical's state and intensity; a smaller linked emission
on `DL_ShadeLinen` can suggest light transmitted through cloth. Turning the
practical off must extinguish all three. The FBX exports no Light objects.
Unity does not receive Blender procedural shading or light transmission. An
opaque shadow-casting shade will block a point light; either exclude the shade
from shadow casting while preserving body contact shadows, or author the
upward/downward pools at its real apertures. Verify the choice from the player
camera and with the actual circuit; studio light values are not calibrated URP
values. The unused rotary switch is appearance only, not a new interaction.

## Inspection and remaining work

`EchoDomesticLighting.blend` contains the clean editable source, before preview
lights and backdrop. `build_domestic_lighting.py` regenerates that file, FBX,
manifest and studio previews. `validate_domestic_lighting.py` reimports the FBX
and checks units/bounds, UVs, materials, surface separation, anchors, floor/cord
contact and the lining's inward normals; results and hash are in
`roundtrip-checks.json`.

Previews include the whole lamp off/on, shade construction, cable-wall contact
and distant tests at 8 m and 13 m with 60° vertical FOV. Those show scale and
silhouette in a neutral studio, **not a Unity Game-view test or human approval**.
At 720p/60° vertical FOV the shade projects to about 37 pixels at 8 m and 22
pixels at 13 m; these are geometric size estimates, not a perceptual test.
The room composition, background contrast, circuit response, light budget and
floor/wall placement still require inspection in the scene. No claim of fear or
human legibility follows from those renders.

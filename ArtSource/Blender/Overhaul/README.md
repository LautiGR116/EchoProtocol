# Overhaul — domestic / reception kit

Original project geometry authored 2026-10-01 in Blender 5.2 LTS. No downloaded
models, third-party textures, add-ons, generated voices or paid services. Source
and exports stay outside `Assets`; the scene integrator imports accepted pieces.

- `EchoOverhaulKit.blend`: source, one named collection per asset. Reception is
  visible initially; unhide another collection's objects to work on that asset.
  All assets are modeled around their own origin, so isolate collections.
- `build_overhaul_kit.py`: repeatable source author and FBX exporter.
- `exports/`: seven FBX files with metric scale and material slots.
- `previews/`: neutral Blender/Cycles studio renders, **not Game-view evidence**.
- `manifest.json`: measured dimensions, material values, object names and pivots.
- `roundtrip-checks.json`: FBX reimport check in Blender; does not establish Unity
  orientation, collision or player-camera quality.

## Placement and articulation

Source axes: X is width, Z is up, **−Y is front**. FBX uses the same `axis_forward=-Z`,
`axis_up=Y` convention as the earlier project kits. The intended front after Unity
import is **+Z**. Retain the imported axis correction when adding scene yaw and
verify from the actual camera before multiplying instances. Do not infer facing
from an individual mesh child's local Euler angles.

All units are metres; import scale 1. No colliders, scripts, lights or cameras are
exported. Use a few fitted BoxColliders in Unity; the furniture silhouettes contain
small rods and textile folds which should not become navigation collision.

| Export | Width × depth × height | Placement / parts |
| --- | --- | --- |
| `EP_OV_ReceptionCounter.fbx` | 2.40 × 1.92 × 1.14 | Floor origin at front-counter center. L return extends behind worker's right. Main top 1.07 m; writing ledge 0.95 m. |
| `EP_OV_KeyMailCabinet.fbx` | 1.21 × 0.23 × 0.95 | Origin at cabinet lower center. Wall mount; back toward −UnityZ. |
| `EP_OV_DressedWindow.fbx` | 2.20 × 0.54 × 2.54 | Floor origin. Window sill 0.75 m; pole 2.55 m. Separate `WindowFrame`, `Curtains`, `Radiator`. Assembly's top is about 2.58 m above root. |
| `EP_OV_DomesticDoor.fbx` | 1.16 × 0.30 × 2.23 | Frame clear width **0.920 m**, clear height 2.107 m before threshold. Separate `DoorFrame` and `DoorLeafPivot`. |
| `EP_OV_Armchair.fbx` | 0.95 × 0.88 × 1.08 | Floor origin. Upholstery, modeled piping/buttons and draped linen. |
| `EP_OV_OpenSuitcase.fbx` | 0.76 × 0.63 × 0.74 | Open pose. Separate `SuitcaseBase`, `SuitcaseLidPivot`, `SuitcaseClothing`. |
| `EP_OV_KeyConsole.fbx` | 0.79 × 0.38 × 0.81 | Top 0.786 m. `ConsoleAndTray` and removable `Key`. |

Door leaf hinge pivot: Blender **(−0.4475, 0, 0)**; nominal Unity **(−0.4475, 0, 0)**.
Handle, plates and hinge barrels belong to the moving leaf. Rotate around vertical
in world/placement space after preserving import correction; do not reset its
position to the model center. The closed leaf is 0.895 m wide and 0.046 m thick
excluding hardware. Add a fitted leaf collider under the pivot.

Suitcase hinge pivot: Blender **(0, +0.235, +0.252)**; nominal Unity
**(0, +0.252, −0.235)**. Lid is already modeled open upright. Clothing is separate
and can be hidden or moved without modifying the case. The loose scarf clears the
front rim and handle.

Key pivot: Blender **(−0.185, −0.025, +0.826)**; nominal Unity
**(−0.185, +0.826, +0.025)**, centered on the key ring.

## Material contract

Named `OV_*` slots are shared intentionally. Exact linear base colors, roughness
and metallic values are in `manifest.json`. Create ordinary URP/Lit material assets
from those values, using `smoothness = 1 − roughness`; flat materials are a valid
first integration. Use cloth roughness around 0.9 and restrained brass metalness.
The window glass is intentionally opaque blue-black for this asset preview: the
integrator must provide a credible exterior before making it transparent.

The studio source uses procedural wood grain and microscopic fabric bump to judge
shape; those Blender nodes are **not baked into the FBX** and are not a claim about
Unity's appearance. Reuse suitable tiling timber/fabric maps if desired, but avoid
mapping floor-plank seams over the counter or chair. Meshes have UVs; large source
cloth panels use normalized face-space UVs.

## Verification and limits

Every FBX was reimported in Blender: finite vertices, material slots, metric bounds
and articulation pivots match source to <0.0001 m. Studio visual review corrected
coplanar counter/cabinet/door joins, curtain/sill intersections, floating envelopes,
seat-pole normal artifacts and scarf/handle intersection. Final previews show
those corrections. These checks do not establish comfort, fear, lighting balance,
performance or navigation in the game. The root agent owns Unity integration.

Regenerate from the repository root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python ArtSource/Blender/Overhaul/build_overhaul_kit.py
/Applications/Blender.app/Contents/MacOS/Blender --background --python ArtSource/Blender/Overhaul/validate_overhaul_kit.py
```

Set `EP_PREVIEW_ONLY=DressedWindow,Armchair` to render selected previews after a
source edit; `EP_PREVIEW_ONLY=none` regenerates source/exports without rendering.

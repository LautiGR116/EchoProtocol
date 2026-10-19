# Reception working-life kit

2026-10-01, Blender 5.2 LTS. Three original static assets, including original
geometric clock numerals. No external models, photographs, font files, texture
downloads, paid services, hair systems, rigging, animation or new game behavior.

The inspected reception capture still presented a centred desk against a blank
wall. These props support an occupiable staff workstation: chair pulled away from
the counter, jacket off to one side, clock away from the central axis. They do
not replace the need for visible working clearance and a believable staff access.
The existing counters, desk and mail cabinet are not rebuilt here.

## Assets

| Export | Main scale and root | Contents |
| --- | --- | --- |
| `EP_RL_ClericalChair.fbx` | About .665 m wide, .625 m deep, .939 m high; floor-centre root. Seat about .49 m high. | Used low-back cloth cushion, upholstered back, steel tubular arms attached under seat, moulded pads, tilt body/height lever, five paired castors. One static mesh. |
| `EP_RL_CoatRackAndJacket.fbx` | About .75 × .68 × 1.831 m; floor-centre root. | Separate `Rack` and `HangingAdultJacket` meshes. Empty adult blazer, open front, lining, asymmetrical relaxed sleeves, lapels, seams, horn buttons, wood hanger actually caught on the rack hook. |
| `EP_RL_WallClock0317.fbx` | Ø.374 m, depth about .087 m; origin at dial centre. | Fixed 03:17 hands, twelve major and sixty total indices, geometric 12/3/6/9 numerals, rolled rim and rear case. No glass plane, second hand, ticking or animation. |

Exact final bounds/counts and material values are authoritative in `manifest.json`.
`build_reception_life.py` regenerates the source `.blend`, exports and previews
inside this folder. The source file stores each asset at its own root, hidden for
isolation, with the chair initially visible.

The chair is sized for the revised 3.2 × 4.8 m administrative booth reported by
the integrator (x ±1.60, z −3.9…0.9); this does **not** prove the surrounding
passage and seat clearance. Keep the
chair slightly rotated/pulled back so its backrest reads above the work surface.
Use only a simple collider if reachable; the wheels do not roll. The rack is a
furnishing, not a recorded person or authored scare trigger. Its garment can be
hidden or reused independently. Avoid placing the pole through the staff chair.

## Coordinates and materials

Metres, Blender **X width, −Y front, Z up**. Export uses FBX −Z/+Y. As measured for
PressurePlant, the current Unity importer initially maps **(−X,Z,−Y)**. To use
the assembly contract **(X,Z,−Y)**, negate imported root `localScale.x` once,
preserve the importer rotation and scale magnitudes, then apply
`placementRotation * importedRotation`. Nominal front becomes Unity +Z.

The clock particularly requires the X correction: a mirrored clock displays the
wrong apparent time even though its face still points toward the player. At
03:17 the hour hand is **98.5°** and minute hand **102°**, clockwise from twelve.
Named source anchors `WallClock0317__HOUR_TIP` and `__MINUTE_TIP` make that
orientation verifiable. Every asset also exports unique `AXIS_X/Y/Z` and
`FRONT_BASE/TIP` empty markers. Use world-position differences for directions
under a mirrored parent, not a rotation-only `Transform.forward` assumption.

Each final mesh has packed UVs and named `RL_*` material slots. Numerical PBR
values are in the manifest; Unity smoothness is approximately 1 minus roughness.
The Blender preview adds a fine procedural weave to chair and jacket material;
**that weave is not baked into FBX**. The modeled cloth volume, folds, piping and
seams remain. There is no required texture import or third-party dependency.
Generate a secondary lightmap UV set in Unity if needed by the chosen lighting.

## Evidence

Studio renders: `ClericalChair.png`, `Chair-rear.png`, `CoatRackAndJacket.png`,
`Jacket-close.png`, `WallClock0317.png`. These establish model inspection, not
Game-view quality or human approval. Revision corrected detached arm supports,
back piping offset, open sleeve shoulders and a floating button; it also reduced
small curve/tick detail that did not improve the viewed silhouette.

`validate_reception_life.py` reimports all three FBXs and checks finite UVs and
vertices, material slots, dimensions, source floor roots and anchor positions.
For the clock it checks both tip angles and actual mesh vertices at those tips.
`roundtrip-checks.json` records those results and hashes. Unity still must verify
facing, shading, clock reading, floor/wall contact, staff circulation, lighting
and composition from the real player camera.

Final reimport result: **3/3 pass**, 45,132 triangles and 1,261,652 bytes across
the three FBXs. Largest metric bounds error is 0.000000209 m. Five current studio
previews were inspected after the correction pass; they are not evidence of the
material response or composition inside Unity.

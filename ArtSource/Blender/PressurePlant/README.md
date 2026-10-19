# Pressure plant — original modular kit

2026-10-01, Blender 5.2 LTS. Project-authored geometry and optional paint maps.
No external model, photograph, paid dependency, scan or generative voice.

`build_pressure_plant.py` rebuilds `EchoPressurePlant.blend`, nine FBX exports,
`manifest.json`, optional textures and previews. All output stays in this directory.
The pump develops the visual vocabulary of the existing original Campaign motor;
its geometry is newly constructed and it has no decorative wheel or gauge.

## Axes and placement

Metres throughout. Blender width X, front **−Y**, up Z. FBX uses the same −Z/+Y
export setting as the previous project kits. **Correction after actual Unity
integration:** with this project's current importer (`bakeAxisConversion: 0`),
the unmodified imported model maps source to **(−X, Z, −Y)**. Its root carries
the import rotation (reported X=270°) and scale (reported 100). The earlier
README incorrectly treated the FBX export basis as the final Unity basis.

The placement contract used by the assembly is **(X, Z, −Y)**. To obtain it,
retain the imported rotation and scale magnitudes, negate the imported root's
**localScale.x exactly once**, then apply the requested placement rotation as
`placementRotation * importedRotation`. Do not replace the importer rotation
with identity or set its scale magnitude to one. Both mappings face front toward
Unity +Z, so frontal views alone failed to expose the mirrored side ports.

This correction is confirmed mathematically against Editor bounds reported by
the integrating agent: the elbow's uncorrected centre is
(-.3225, 0, -.1775), and the receiver centre has X=-.082. Those values agree with
the reimported FBX source bounds under (−X,Z,−Y), and disagree with the former
README assumption. `axis-diagnosis.json` records both mappings and all port
positions/normals. No geometry or FBX was re-exported for this documentation fix.
Unity camera/normal/culling verification remains the integrator's check.

The three large pieces have roots at floor centre. Pipe modules use the precise
centreline ports below; source positions and outward directions are in the manifest.
No animations, scripts, colliders or Unity assets are included.

| FBX suffix (`EP_PP_`) | Main dimensions / mating ports, source coordinates |
| --- | --- |
| `Receiver` | Shell Ø1.20 m, total height 2.35 m; cover/nozzle increase total envelope to about 1.364 × 1.325 m. Inlet (.76, 0, .84), normal +X; top (0, 0, 2.35), normal +Z. |
| `ServiceCabinet` | **Exactly 1.70 wide × 6.00 deep × 2.70 high**. Root at floor centre; return grille at Y=−3. Completely opaque core. |
| `MotorSkid` | About 1.55 × .80 × 1.01 m. Inlet (.79, 0, .535), normal +X; discharge (.41, 0, 1.01), normal +Z. No handwheel, gauge or indicator. |
| `PipeStraight2m` | Axis X, mating planes X=−1/+1. Root at centre. |
| `PipeElbow90` | Centreline radius .32 m with .18 m tangents. Root/inlet (0,0,0), outward −X; outlet (.5,.5,0), outward +Y. |
| `PipeTee` | Root at junction. Ports (−.5,0,0), (.5,0,0), (0,.5,0). Connected internal bores. |
| `PipeWallSupport` | Root at pipe axis X. Wall behind at source Y=+.277; source Z stays vertical. Collar OD .214 m. |
| `PipeAdaptor300` | Axis X, mating planes X=−.15/+.15; use for short connections. |
| `PipeBlindCap` | Mating plane X=0, outward −X; cap extends +X. Closes an unused flange. |

Collector pipe outer diameter is .18 m, bore .164 m, flange diameter .29 m.
The receiver nozzles have .16 m bores. Fasteners remain inside the stated mating
planes. Mating a straight and elbow requires both port positions and outward
directions to agree. Never leave free pipe ends visible in space: mate to another
part, terminate into a constructed wall penetration, or fit the blind cap.

For a variable run, preserve the round cross section and fit straight centre
lengths between fittings; avoid uniformly scaling a whole elbow or flange.
These are visual environment pieces, not pressure-rated engineering components.

## Existing baffle and functional controls

The service cabinet supplies a closed replacement skin for the central baffle.
Its full envelope matches the current Unity box: local centre (0,1.35,0), size
(1.7,2.7,6). Hide/remove the old baffle renderer to prevent overlapping surfaces;
retain its collider if the root and orientation match. The grille has depth and
separate angled louvers backed by opaque geometry, so it adds no through view.
Its front/back can be swapped by yaw according to the entry camera.

The receiver remains separate: place it beyond an end or near a side only after
checking player clearance, sightlines and the route to the actual control. Do not
simply add its diameter to a narrow corridor. Its top and side nozzles connect to
the collector rather than ending in air.

Use the existing Finish `ValveControl` and its functional needle/pilot. This kit
provides its surrounding plant; it contains no replacement interaction or fake
second wheel. Fit a simple collider to the motor/skid where reachable; do not add
triangle collision to fasteners, cooling fins or pipe interiors.

## Materials and UVs

Every final mesh has a packed `UVMap` including custom lathed and swept surfaces.
The manifold has actual open bores; the blind cap and service core are closed.
Each mesh has named `PP_*` slots; exact base colour, roughness and metallic values
are in `manifest.json`. Transfer them to URP deliberately. Unity smoothness is
approximately `1 − roughness`; material FBX import alone does not verify shading.

`textures/PP_Enamel_BaseColor.png` and `PP_Enamel_Roughness.png` are original,
tileable 512px optional maps for `PP_ServiceEnamel`. Roughness is linear data.
They are **not connected to the exported FBX shader**: flat numerical PBR values
are authoritative, so copying only FBXs is valid. No map contains baked lighting.
Replacement panel colour, welds, small exposed edges and oxidation at feet add
physical use cues without heavy all-over rust. No Unity lightmap UV set is authored;
generate one during import if the scene needs it.

## Evidence and limits

`previews/Receiver.png`, `ServiceCabinet.png`, `ServiceCabinet-player.png`,
`MotorSkid.png` and `PipeModules.png` are physically lit studio renders. The
cabinet player preview uses a 1.68 m high camera. They are not Game views or human
approval. Revision corrected a partly buried rating plate, twisting drain/conduit
section frames, a faceted receiver head and a 3 mm cabinet envelope overshoot.

`validate_pressure_plant.py` reimports all FBXs and verifies finite populated UVs,
material slots, metric bounds, root pivots, cabinet envelope and receiver height.
Nine lateral rays at three depths and three heights verify the imported cabinet's
opaque surface; they do not replace the actual Echo/control sightline check.
`roundtrip-checks.json` is the objective file result. Unity integration must still
verify facing, shading, floor contact, collision, route clearance and preservation
of the Echo/control separation. Human play remains necessary to judge tension.

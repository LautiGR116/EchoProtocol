# ShutdownMotor — source candidate

Original derivative of the project's `PressurePlant/MotorSkid`, 2026-10-01.
No external assets, paid services, textures, audio, animation or generated images.
`build_shutdown_motor.py` contains the copied original construction helpers/body
and the newly authored rear cavity/guard attachment/rotor. The original builder
hash is recorded in `manifest.json`; PressurePlant itself was not changed.

**Stable source/export:** `EchoShutdownMotor.blend`,
`exports/EP_SD_MotorSkid.fbx`, `manifest.json`.

- `ShutdownMotorStatic`: **16824 triangles**, fixed body and grille.
- `ShutdownRotor`: **952 triangles**, separately pivoted matte wheel.
- Total **17776 triangles**. Original envelope unchanged:
  X−.76..+.79, Y−.40..+.40, Z0..1.01m. Root at floor centre.
- Inlet source `(.79,0,.535)` and discharge `(.41,0,1.01)` remain exact.
- Rotor pivot `(-.682,0,.535)`, axis source+X. Fixed marker empties identify
  ROOT, three axes, rotor pivot/axis/reference and both ports.

The old solid rear disk/capped cylinder are replaced by an annular cast hood
with a real bore. The original nine horizontal guard bars now reach a fixed
retaining ring, which has four physical standoffs into the hood. Their old short
ends had relied visually on the former opaque disk and would otherwise float.
All geometry outside the explicitly rebuilt rear-cavity region is unchanged.

The wheel is nearly continuous, with a shallow broad pressed recess and one dull
patina sector. It avoids bright blades, repeated stripes and emissions. The front
shroud, small hub and shaft cap are a single moving mesh; the guard never moves.
The original PP materials retain numerical values. Two new matte numeric
materials, `SD_RotorMatte` and `SD_RotorPatina`, are declared in the manifest.
No emission, shader effect, external map or reflection probe is needed.

## Integration contract

Replace only pump A's visual model at Unity `(6.89,0,-36.5)` after selection.
Keep the existing collider, pipe connections and audio source. Apply the same FBX
import/root-X correction as PressurePlant, retaining importer scale/rotation:
source maps to corrected Unity `(X,Z,-Y)`. Rotor pivot then lies at
`(6.208,.535,-36.5)`. Determine the rotation axis from marker endpoints; do not
assume imported local axes match Blender axes. No motor-B animation is implied.

Presentation-only candidate: integrate wheel angle from `MotorLevel01` and scaled
time, provisional1440°/s at full level. It must stop persistently with the existing
2.2s motor envelope, pause with the session and avoid replay on restoration.
The numeric speed is not physically calibrated RPM. Runtime adapter is not part
of this export. See the state contract in
`ArtSource/Design/PressurePresence/ROTOR_SPEC.md`.

The low-contrast wheel needs an actual moving-camera comparison: reject it if
too inconspicuous, apparently slow/reversed or flickering. Do not solve those
problems by adding flashes or brighter contrast. Selection remains with the scene
integrator; neither studio image nor angle counter proves perceptible shutdown.

## Verification and previews

`validate_shutdown_motor.py` independently imports the final FBX and the original
MotorSkid for comparison. `validation.json`: **PASS35** — finite vertices/normals,
UVs/materials, expected bounds/anchors/triangle counts, closed rotor, rays through
four actual grille gaps reaching the rotor, fixed-guard rays, a rotated-wheel
check, and identical protected geometry outside the rear cavity.

`previews/MotorSkid-overview.png`, `Rotor-close.png` and `Rotor-other-rest.png`
are Blender studio images inspected for the open bore, guard attachment and
separate wheel. They are **not** Unity lighting or motion evidence. No audio was
heard, no perceptual comfort/playtest approval is claimed, and no Assets/scene
was changed by this work.

Reproduce with Blender5.2 (two threads were used for these exports/previews):

```sh
/Applications/Blender.app/Contents/MacOS/Blender -b -t 2 --python ArtSource/Blender/ShutdownMotor/build_shutdown_motor.py
/Applications/Blender.app/Contents/MacOS/Blender -b -t 2 --python ArtSource/Blender/ShutdownMotor/validate_shutdown_motor.py
```


Integration update: imported into `EchoProtocol_Shutdown.unity`, pumpA only.
The original PP materials are reused; two new URP materials use manifest linear
colors converted to gamma like the original PressurePlant author. Actual marker
readback gives rotor local axis−X at the specified world pivot. The source
roundtrip scope above remains geometry-only; runtime/camera/continuation evidence
belongs to `Docs/VisualEvidence/2026-10-01-Shutdown/`. Human motion/readability
and listening are still unapproved.

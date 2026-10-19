# Recovery Office

Three original static models for a staffed records-review room, authored in
Blender 5.2 LTS on 2026-10-01. No external models, photographs, fonts, downloaded
textures, paid services or addons. No animation, extra monitor function or new
Echo channel. Unity integration belongs to the scene integrator.

## Models

| FBX | Root and working dimensions | Separate meshes |
| --- | --- | --- |
| `EP_RO_ReviewDesk.fbx` | Floor-centre root. Worktop 1.65 × .78 m; top surface exactly .760 m. Grommet/cord rises to .771 m and rear cord extends overall depth to .812 m. | `DeskStructure`, `DeskRearCable` |
| `EP_RO_TaskLamp.fbx` | Table-contact root. Approximately .47 m high above table; rear-edge cord intentionally extends below root. | `TaskLightBody`, `TaskLightDiffuser`, `TaskLightCable` |
| `EP_RO_ReviewMonitor.fbx` | Screen-centre root. Outer frame .858 × .538 m. Desktop contact is .720 m below root. | `MonitorShellOpen`, `MonitorStand`, `MonitorCable` |

All three together contain **12,496 triangles**, below the requested 20,000.
Final roundtrip result is **3/3 PASS**; all nine monitor-opening rays reach the
display plane unobstructed. The three FBXs total 396,356 bytes.
Exact bounds, per-mesh counts, material values and anchors are in `manifest.json`.
All meshes have packed UVs. The original procedural timber shader appears only
in Blender previews; FBX exports simple numerical PBR materials, **not that grain**.
The existing project WalnutContinuous texture is a suitable integration choice.

The desk has two steel support frames, rubber feet, a suspended right-hand
drawer unit and a modesty panel at the rear. The clear knee width is approximately
1.05 m (source X −.710…+.345), with about .66 m from the front edge to the panel.
The underside of the tabletop is .722 m above floor. Drawers and lamp joints are
static; handles/switch appearance does not add an interaction. The top has no
invented record labels, personal documents or screen content.

## Placement and anchors

Metres; source X width, **−Y front**, Z up. Every export has a named ROOT anchor,
source axes and FRONT_BASE/TIP. Under the observed Unity FBX importer, negate
imported root localScale.x **once**, retain the imported rotation and magnitude,
then apply placementRotation × importedRotation. The corrected map is
(X,Z,−Y), nominal front Unity +Z. Use anchor world-position differences for
directions rather than rotation-only vectors under the negative scale.

- Desk: `ReviewDesk__DESKTOP_CENTRE` is (0,0,.760).
  `TASKLAMP_PLACEMENT` is (−.59,.03,.760) and `MONITOR_PLACEMENT` is
  (−.12,.12,.760), both optional composition references in source coordinates.
- Lamp: `TaskLamp__TABLE_CONTACT` is its root. `LIGHT_SOURCE` and
  `LIGHT_AIM_TIP` define a downward/forward spotlight. `DIFFUSER_CENTRE` locates
  the physically recessed opal disc. The rear tabletop edge is source Y+.36
  from the lamp: the suggested desk placement Y+.03 matches the desk edge Y+.39.
  Route/hide `TaskLightCable` as one separate mesh if another placement is used;
  do not leave the cord floating through the table. Lamp minZ below zero is
  intentional, not a bad table pivot.
- Monitor: `ReviewMonitor__SCREEN_CENTRE` is (0,0,0), and the **display plane is
  source Y=0**. Its .690 × .380 m existing display fits inside the open bezel.
  There is no exported screen/text plane. The rear chassis lies behind the
  display, in +Y. `DESKTOP_CONTACT` is (0,0,−.720); screen world Y1.480 therefore
  puts the stand on world Y.760. For front world +X, use corrected model yaw90.
  At the integrator's current screen plane x−7.23, place the model root at
  (−7.23,1.48,27.8), **not the old housing centre x−7.30**. That keeps the
  existing display and interaction stationary and prevents a visibly floating
  screen. The existing desk placement centre (−7.55,0,27.8), yaw90, still
  contains the stand's footprint. Confirm exact geometry in the scene.

The lamp exports no Light and emission begins off. Bind `RO_LampDiffuser` to
the actual light/circuit state. Place the source at its anchor and aim toward
LIGHT_AIM_TIP. The diffuser should not shadow-block its own light; the metal
head, arms and base can retain shadows. Studio intensity is not calibrated URP.
Preserve the existing R1 display/label and `VS1RecallAnchor` references. The
models add no invisible obstacle, forced camera movement or checkpoint change.

## Source and inspection

`build_recovery_office.py` is standalone and recreates the three FBXs, manifest,
editable `EchoRecoveryOffice.blend` and previews. In the clean saved blend the
three asset roots are separated along X for inspection; each FBX is exported
at its own origin before that separation. Preview lights, floor, tabletop and
reference display are added after saving/export and are not source assets.

`validate_recovery_office.py` reimports all FBXs, checks finite UVs/geometry,
materials, bounds/anchors, counts and the 20k limit. It also casts nine rays from
in front of the monitor to the real display plane, including the display corners,
to detect any obstructing bezel/body geometry. Results/hashes are recorded in
`roundtrip-checks.json`.

Studio previews show desk front/rear, lamp on a reference tabletop, open monitor
front/rear, assembled workstation, and the monitor with an exact-size plain
reference display. The latter is inspection geometry only, never exported.
These images verify model construction; Unity materials, screen/label clearance,
light circuit, chair clearance and player circulation still require Game-view
and route checks. They do not constitute human approval or a playthrough.

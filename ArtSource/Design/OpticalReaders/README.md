# Optical reader: static presentation candidate

Original geometry recipe, 2026-10-01. **Integrated through ReadersProduction in the Readers successor to Pane.**
Selected first-protocol survey PASS62 and full campaign PASS456; build is being
prepared. The source recipe below is unchanged; the integrated author also
corrects fixture spill, the B reminder, and the P2 curtain/plate after camera
review. See Docs/VisualEvidence/2026-10-01-Readers/README.md. No external assets, textures or license dependency.

## Evidence and intended change

Finish's `AuthorOpticalInstruments` creates a solid cylinder as a bezel. In its
coordinates, the bezel front is `p.x-.0095`, the neutral lens front `p.x-.005`,
and the active lens front `p.x-.016`. Thus the nominal idle lens is buried 4.5 mm
behind the solid bezel; only the active, uniformly cyan disc clears it. The
current serialized Study scene retains these dimensions for all six readers.

The Records `campaign-01-P1-Camera.png` shows C as a flat cyan disc. Its final
CSV sample is `AwaitingLaneCheck`, before SE1's extinguishing sequence. This is
not a view of the player's initial approach to A/B, and it cannot establish
whether an unprimed human mistakes the disc for an E button. Current P3 cameras
do not provide a comparable frontal closeup; the shared geometry supports the
same mechanical finding, not an invented visual observation.

The candidate makes a small optical head: a bevelled enamel box, a genuinely
open bezel, an always-present shallow dark optical cap and a recessed cyan
qualification ring. No moving shutter, scan beam, pupil graphic, press travel,
extra light or new indication of recorded data. Position and horizontal body
orientation remain the only optical-reader rules.

## Dimensions

The canonical origin `p` is the old neutral-lens centre, or the preserved active
indicator's position plus world `(+.009,0,0)`. Canonical +Z faces the Player
(world -X); +Y is up. `BuildForFinishIndicator` returns the exact root pose.

| Part | Dimensions / canonical coordinates, metres |
| --- | --- |
| Housing | Width .340, height .400, depth .170; centre Z=-.120; .0035 edge bevel. Bounds Z[-.205,-.035], unchanged from Finish's body. |
| Bezel | Outer diameter .255; front Z=.018; rear Z=-.035; entry bore diameter .184 at Z=.013. Actual open tube, no cap over the lens. |
| Recess seat | Bore radius .092 at Z=-.010, tapering to .070 at Z=-.014. The lens edge and signal base seat into this fixed dark shoulder. |
| Fixed lens | Diameter .148; shallow cap centre Z=-.007, edge Z=-.0125, rear Z=-.016. Opaque Lit material; no layered transparency. |
| Active ring | Outer radius .085, inner .073; front radii .084/.074 at Z=-.004; rear Z=-.014. Front is 22 mm behind bezel lip and 3 mm ahead of the lens centre. |

The signal and lens have no coincident visible surface: the closest lens point
is central, outside the ring's radial footprint. At the shared outer lens edge
the lens is more than 8 mm behind the ring front. The deeper signal base is
intentionally seated into the bezel shoulder. Inactive state exposes that fixed
dark seat, rather than an empty unsupported luminous hoop.

Four small meshes: 108 housing triangles, 960 bezel, 480 lens and 384 signal
(1932 total per head; 48 radial sectors). Explicit finite normals/UVs; triangle
winding follows outward normals and degenerate pole triangles are omitted.
These are source-derived counts/geometry expectations, not a Unity import test.

## Integration without changing object identity or pose

`staged/OpticalReaderMeshRecipe.cs` creates Mesh objects only. It does not create
GameObjects, save assets, edit transforms, add components or mutate gameplay.

For **each explicit real OpticalReader**, resolve its current `activeIndicator`
and the matching Finish `Optical enamel body`, `Optical bezel`, and `Optical
neutral lens` filters. Build once, then bake each fixed mesh to its existing
filter transform using `BakeFixedMeshToExistingTransform` and assign it. Assign
`activeRing` directly to the existing active indicator filter: it is already
baked to that exact transform. Do not bake it a second time.

```csharp
var shape = OpticalReaderMeshRecipe.BuildForFinishIndicator(active.transform, reader.name);
OpticalReaderMeshRecipe.BakeFixedMeshToExistingTransform(shape.housing, shape, body.transform);
OpticalReaderMeshRecipe.BakeFixedMeshToExistingTransform(shape.bezel, shape, bezel.transform);
OpticalReaderMeshRecipe.BakeFixedMeshToExistingTransform(shape.lens, shape, neutral.transform);
body.sharedMesh = shape.housing;
bezel.sharedMesh = shape.bezel;
neutral.sharedMesh = shape.lens;
active.GetComponent<MeshFilter>().sharedMesh = shape.activeRing;
// Persist the meshes as authored assets through the parent Editor author.
```

The old disabled legacy housings and every BoxCollider stay untouched. Match
the three Finish filters explicitly by spatial relation to `p`, not by a broad
name-only replacement that might select unrelated controls. Leave fasteners and
A/B/C identifiers in place. Do not change renderer activation ownership.

Included: P1 lane A/B, service C, P2 Blind Alignment Reader, P3 A/B. **Excluded:**
the `Pressure alignment lens` / `PressureAlignmentFeedback` P–02 placeholder;
it is not an `OpticalReader` and uses a different presentation contract.

The recipe rejects a signal transform other than the expected Finish rotation
(local up facing world -X) and lossy scale (.17,.007,.17), rather than silently
generating the wrong instrument. All six were separately confirmed by root in
the Editor. Mesh baking uses inverse transpose for normals under preserved
nonuniform scales. Existing sources, activeIndicator refs, trigger size/pose,
actor masks, tolerances and times stay intact: P1 40°/.25 s; P2/P3 35°/.35 s.

## One material candidate

Clone materials locally for this edition; leave legacy material assets alone.
URP Lit, opaque, specular highlights enabled, no textures or emission:

| Surface | BaseColor (`Color` arguments) | Metallic | Smoothness |
| --- | --- | --- | --- |
| Housing, muted clinic enamel | (.30,.34,.31,1) | .10 | .35 |
| Bezel, carbon finish | (.025,.031,.029,1) | .20 | .32 |
| Fixed optic, dark amber coating | (.11,.082,.038,1) | 0 | .78 |

Keep the current cyan Unlit material on the signal, including its exact color;
do not add a light or raise intensity to compensate for smaller area. The new
signal occupies roughly a quarter of the old lit disc's area. The maximum 12 mm
radial width projects to approximately 3 pixels at A/B's 2.4 m in a
1280×720/60° vertical-FOV view; the bevel leaves a 10 mm frontal face,
approximately 2.6 pixels before obliquity and antialiasing. Root must compare lit A/B,
close C and P3 in real cameras: there is a genuine distance-legibility tradeoff.

Changing material roughness cannot supply missing incident light. Preserve
SE1 darkness and judge the first encounter separately. Before accepting, the
inactive central lens/recess should read as an instrument under the actual
initial lights; qualification should remain immediately readable without
turning the head into a large luminous button. No human comprehension or
auditory quality claim is made by this source-only proposal.

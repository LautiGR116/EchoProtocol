# Original adult recording surface

2026-10-01. Blender 5.2 LTS. Project-authored geometry; no scan, third-party
character, downloaded model, real-person likeness or paid service.

`exports/EP_AdultRecordedSurface.fbx` is one static adult surface. Source:
`EchoAdultFigure.blend` and `build_overhaul_figure.py`. The figure wears a light
jacket with hood resting behind the neck, trousers and low leather shoes. Hands
hang with fingers gently grouped. The small fixed pose asymmetry is part of the
model; there is **no rig or animation**, and it adds no recorded channel to Echo.

## Integration

- Height approximately **1.76 m**; exact dimensions/counts in `manifest.json`.
- Root at floor between the feet. Source front **−Y**, Z up. FBX −Z/+Y matches the
  other project kits; intended Unity front **+Z** with importer correction retained.
- One MeshRenderer after import, multiple `FIG_*` slots. Replace every slot with
  the same Echo shader for recording playback. For an opaque presence, use the
  named clothing/skin/hair/footwear materials and manifest PBR values.
- Large surfaces are volumetrically unified to remove internal shoulder/neck/leg
  shells that would show through transparency. Small facial, zipper and stitching
  details remain exterior geometry. Do not add collision to clothing triangles.
- The shader's near-camera fade depends on the mesh origin: keep the root at the
  feet, not at the head or renderer bounds center.
- Preview fabric bump is procedural Blender shading, not a baked texture in FBX.

## Review and evidence

`previews/front.png`, `side.png`, `oblique.png`, `back.png` use eye-height cameras.
`head.png` is a closer inspection. `cyan-surface.png` replaces materials with a
single solid cyan material to expose geometry; **it is not Unity's transparent
Echo shader**. These are studio renders, not in-game approval or human playtest.

The first candidate is retained in `previews/rejected-01/`. Its flat shoulder
ends, floating hood rim, simplified face lines and over-dense mesh were rejected.
The final revision reduces total silhouette width to about 0.55 m, slopes the
shoulders down from the neck, starts sleeves below the acromion, and rounds the
upper cranium analytically. Drawn eyes, eyebrows and mouth lines were removed;
restrained orbital and cheek planes remain. It also fits the hood against the
back, recovers opaque material assignments after remeshing, cuts garment
boundaries cleanly and reduces the mesh by more than half. Exact final counts are authoritative in the manifest.

The result is an original stylized adult. It is not a photorealistic scan, and
opaque close facial appearance still deserves critical review in the actual
scene. The intended improvement is a continuous human/clothing silhouette at
1–3 m instead of separate geometric body pieces. Integration must still verify
facing, floor contact, near-camera fading, translucent sorting and actual player
view; no result for those is inferred from these studio renders.

`validate_overhaul_figure.py` reimports the FBX and checks finite vertices,
metric bounds, one mesh, no armature and populated clothing/skin materials.
The resulting `roundtrip-checks.json` is limited to that file-level evidence.

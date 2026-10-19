# Furniture material bake — 2026-10-01

Two original, periodic PBR materials prepared outside `Assets` for integration.
No photography, image generation, downloaded texture, paid service or external
image source was used. The kit's palette and procedural material nodes informed
the direction; its `.blend`, meshes and exports remain unchanged.

| Material | Use | Authoring |
| --- | --- | --- |
| `WalnutContinuous` | Cabinet fronts, counter, console and door trim | Long wandering growth bands, fine vessels and restrained roughness variation. No plank joints, nails or baked shadows. Grain runs along image V. |
| `LinenPlain` | Curtains, throw and tinted chair upholstery | Periodic plain weave with alternating warp/weft, small yarn variation and quiet color variation. Neutral base can receive a green, tobacco or cream tint. Geometry remains responsible for folds. |

Each family has 1024×1024 `Diffuse`, `NormalGL`, `Roughness`, `MetalSmooth` and
`Height` PNGs. Heights are reference data; ordinary Unity Lit integration needs
only Diffuse, NormalGL and MetalSmooth. They are authored procedural materials,
not scans or claims of physical measurement.

## Unity setup

- Diffuse: sRGB, repeat, mipmaps on, anisotropy 4–8. Begin with a white material
  tint for walnut so the previous floor tint does not darken it twice.
- NormalGL: Normal Map, repeat, mipmaps on, **do not flip green**. Initial normal
  strength: 0.6 for walnut, 0.8 for linen; inspect in the actual scene.
- MetalSmooth: linear/non-sRGB. RGB is zero (dielectric); alpha is
  `1 − roughness`. Enable `_METALLICSPECGLOSSMAP` and set `_Smoothness = 1` so
  the authored alpha is not multiplied by the old provisional smoothness.
- Separate Roughness/Height: linear/non-sRGB. Not needed by URP Lit when the
  packed texture is used. Do not feed Roughness into the smoothness slot.
- Walnut nominal tile: 0.6 m. Linen nominal tile: 0.18 m. The kit has normalized
  per-part UVs, so actual density varies with part dimensions. Preview uses one
  walnut tile, two linen tiles and three upholstery tiles per normalized UV.
  Inspect the counter grain direction and curtain weave density before freezing
  tiling; this is a suggested starting point, not verified world density.

The neutral textile is intentionally quiet at distance. Let fine relief and
roughness read near the lamp rather than amplifying albedo until it aliases.
Furniture does not acquire artificial plank seams when this replaces the floor
map. Keep the floor material separate.

## Reproduction and evidence

Run Blender 5.2 in background with `--python bake_materials.py`. It evaluates
original periodic fields, writes deterministic 8-bit PNGs, then constructs
Principled materials and previews using an in-memory copy of
`ArtSource/Blender/Overhaul/EchoOverhaulKit.blend`. All writes remain in this
directory. `FurnitureMaterials.blend` contains the copied preview setup and
packed images; it is not a replacement for the kit.

`manifest.json` records exact hashes, dimensions, extrema, periodic boundary
checks, and the source kit hash before/after. Preview files demonstrate Blender
material application only. Unity import, lighting, color, mip behavior and actual
Player-camera presentation still require integration review.

Final checks: ten output hashes match; all authored values are finite; source
kit hash is unchanged. Mean boundary height steps are below the corresponding
interior steps in both axes for both materials. Walnut roughness is 0.369–0.490;
linen is 0.906–0.977. All ten maps total 2,160,437 bytes; the six runtime maps
total 1,073,042 bytes.

Four second-pass previews were inspected on the actual source kit: armchair,
key console, reception counter and dressed window. The first cloth pass showed
unwanted broad bands; its sinusoidal slub was removed and thread crossing made
continuous. The second pass reads quietly at full-object distance. First-pass
images remain in `Iteration01/` for comparison. This is an agent visual review,
not a human art approval or a Unity Player-camera check.

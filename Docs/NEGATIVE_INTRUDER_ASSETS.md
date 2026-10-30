# El Negativo — assets, 2026-10-30

Original procedural design: closed asymmetric ivory shell, cloth cowl, continuous
coat with curved split tails and full sleeves, eight blunt finger sleeves. No
downloaded/generated-image reference, borrowed character or new voice. The
installer preserves the original Nathan rig, legs and grounded boots; it removes the
original face/everyday clothing triangles only in a separate private derivative.

Generated assets live in `RenderpeoplePrivate/NegativeIntruder/` because they carry
the existing licensed Nathan bind matrices or retained scan geometry. Do not
publish them. Public source contains the original mesh recipes and installer.

`NegativeIntruderIntegration.Apply()` only edits the already open Incursion or
Negative scene and saves generated private assets. It never loads/saves a scene.
The scene-local receipt retains the prior body mesh/material references. Repeating
Apply preserves the installation and composition. Root transform, collider,
recorded Echo, audio and capture/pursuit rules are unchanged. Appearance and motion
require the central Editor review; fear is not established by these assets.

Revision 2 clamps the fractional mask profile before exponentiation and rejects
non-finite vertices, normals, tangents, UVs, bounds, skin weights and bind matrices
before asset persistence. After a revision-1 installation, run
`NegativeIntruderIntegration.RepairGeneratedGeometry()` outside Play and save the
scene. It assigns new validated `*-v2.asset` meshes rather than reusing invalid
native buffers. The source scan and unreferenced first generated assets remain.

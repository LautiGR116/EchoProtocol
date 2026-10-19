# RecordedPresence — selected display surface

Original project-authored shaders and preset, 2026-10-01. Imported into the
separate Presence scene/prefab; compiled in Unity6000.3.21f1 / URP17.3 Forward+.
Selected-preset full route PASS436 and universal macOS build/signature pass; native launch is pending because the Mac locked.

The recorded body now has interior coverage and diffuse relief rather than a
bright outline with an almost absent centre. The same preset applies from P1 to
6B. It displays the existing rigid adult mesh and the actual recorded position /
body yaw. It reproduces no sound, carried object, hand pose or articulated motion.
World figures and the present-Player reserve/hand are separate and unchanged.

`material-parameters.json` contains exact Material.SetColor values (no extra
linear/gamma conversion). Body alpha.90, edge addition.03, record relief.32,
world diffuse gain.65. The modest upper-left view-space normal term is stylized
display relief, not recorded illumination. There is no pulse, noise, scanline,
specular halo, event/chapter input or time-dependent change. World diffuse
includes URP main/additional lights, SH, shadows and cookies. No baked lightmap
or APV variants are claimed.

A child renderer uses the **same MeshFilter mesh and identity local transform**
to write self-depth at queue2990, ColorMask0. The body blends at3000 with ZTest
LEqual, without depth writes. This rejects back layers that previously made
head/axilla/legs into hard patches. Neither surface casts a shadow. The helper
has no collider, light or audio. Both surfaces share the .35–.95m root-to-camera
horizontal fade; at zero alpha the helper is clipped too. The selected focused
fixture gives identical RGB readback with these fully faded surfaces enabled
or disabled, with stochastic postprocessing excluded only from that numeric
comparison. Actual approach pictures retain game postprocessing.

This is a small self-depth approximation, not order-independent transparency.
Opaque backgrounds remain visible through color alpha; transparent surfaces
behind the Echo are depth-rejected. At.90 body coverage that restriction is
small but real. Front glass and opaque partitions remain in front. The final
campaign comparison must inspect actual glass/door views; lateral and human
motion/comfort acceptance cannot be inferred from one still.

Candidate01 (no self-depth) and candidate02 (.62 body) comparisons are preserved
in Docs/VisualEvidence/2026-10-01-Presence. Candidate01 was rejected for layered
patches; candidate02 fixed those but lost colour/relief over warm backgrounds.
The selected preset was compared with several opacity/tint variants in an
explicit paused warm fixture, never presented as a human playthrough. Static
rigid locomotion remains part of the recording display's limits.

`PresenceProduction.Author` saves explicit scene/prefab/material references.
The historical Threshold/Reserve Echo prefab/materials remain recoverable.

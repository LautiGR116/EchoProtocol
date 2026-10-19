# Original domestic reserve

Two original models: compact reserve pack and mural dock. The pack has a real
carrying grip, formed case, protected rear connector and seated bottom gasket.
The dock retains a readable recess/contact/rail silhouette when empty. No third-
party geometry, texture, font or service. No emitted light, collider, audio or
runtime charge mechanic is supplied by the FBX. Unity authors the room/pack labels.

`build_reserve_unit.py` saves editable .blend source, FBX, metric manifest and two
studio previews. `validate_reserve_unit.py` independently reimports the assets:
finite geometry/UV/materials/bounds/anchors, 3.5 mm side rail clearance and .5 mm
seat gap. Total 5,845 triangles. Source Xwidth/-Yfront/Zup; single root X correction
in Unity, retaining imported scale/rotation. Intended integration scale 1.15.

This kit is source art until integrated and checked in a saved Unity scene.
The carrying hand/sleeve is a separately authored project model in ReserveGrip;
its existence does not add an articulated or carried-object Echo channel.

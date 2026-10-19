# Original rotary telephone

`build_telephone.py` creates editable geometry, a separate handset with an actual
pivot, metric placement and cord anchors, six original material values and studio
previews. No external geometry, texture, brand, font or service. `manifest.json`
records the axis contract, bounds, triangles and named parts. Unity applies the
same single root X correction used by RecoveryOffice.

`validate_telephone.py` independently reimports the FBX and checks finite geometry,
UVs, materials, bounds, anchors and the resting handset's tabletop clearance.
The runtime cord is deliberately absent from the static FBX and studio previews;
its endpoints are attached explicitly in Unity. Offhook pose is an optional
present-tense action, never recorded or reconstructed by the Echo.

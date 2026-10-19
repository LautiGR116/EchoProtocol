# Telephone contacts — 2026-10-01

Two CC0 public HQ MP3 previews from cookies+policy's rotary telephone pack:
[ring](https://freesound.org/people/cookies%2Bpolicy/sounds/556499/) and
[soft pickup then hangup](https://freesound.org/people/cookies%2Bpolicy/sounds/556496/).
They are compressed previews, not lossless originals. No account, paid service,
voice, cloning or private material was used. Source-page license evidence and
hashes are in `sources.json` and `Evidence/`.

`build_masters.py` decodes through macOS afconvert and uses NumPy for documented
crops, DC removal, short fades and attenuation. Output is mono 48 kHz / 24-bit
PCM; this does not recover precision lost in the MP3 or 16-bit decode.
`masters.json` gives exact intervals, transformations and signal measurements.
No clipping or non-finite samples. Pickup/hangup attribution follows the uploader's
description and the separated amplitude events, not an agent listening test.

In Offhook, the ring remains at the telephone base; contacts follow its moving
handset. Both sources use normal 3D attenuation and zero Doppler. The ring stops
on first answer. No received voice, added line noise, head-locked sound or Echo
sound reproduction. Human timbre, mix, localization and comfort: **EN PRUEBA**.

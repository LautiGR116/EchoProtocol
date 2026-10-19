# Offhook — objective production evidence

Scene: `Assets/_Game/Scenes/Production/EchoProtocol_Offhook.unity`, derived from
Porter. The phone is optional. It neither unlocks the key nor adds a puzzle gate.

`telephone-checks.txt`: 21 focused checks after one explicitly prepared prior-act
fixture and one Player pose fixture. Actual Input System E/R, existing center-camera
HouseInteraction ray, ring start/stop, answer/power consequence, foreign actor
rejection, motion lockout, pause/resume, two settled poses, coiled-cord endpoints
and table clearance, second E, later optional pickup and chapter exit rejection.
The third pickup ends off hook before cleanup. This is not a continuous playthrough.
The subsequent cord optimization avoids updates while stationary; full-route
coverage below tests that final code.

`01-ringing.png`, `02-offhook-reserve.png`, `03-hungup-reserve.png`: actual Player
camera captures from that test. Camera.Render omits OnGUI. Initial captures
revealed old cup/paper geometry intersecting the new phone; the old table belongings
were disabled only in this edition and these images were recaptured afterward.
The isolated fixture starts with the initial ambient state. The continuous-route
captures are the authority for the later, darker house after SE1; they are stored
separately. Both camera states retain a visible phone silhouette without adding
a phone-specific light.

Independent Blender FBX roundtrip: finite geometry, UVs, materials, bounds, anchors,
handset pivot and 2 mm tabletop clearance. Source `ArtSource/Blender/Telephone/`.
Studio previews intentionally omit the runtime cord and are not game screenshots.
Audio rights, exact source crops and peak/RMS/DC/hash checks are in
`ArtSource/Audio/Telephone/`; the received line is silent, with no recorded voice.

Human pass needed: approach without guidance, decide whether to answer, judge the
lift/cut/silence timing, recognize the handset resting beside its base, test E to
hang up, and continue searching for the key. Assess ring/contact plausibility,
localization and comfort at a comfortable headphone volume; repeat with speakers
and reduced dynamics. Fear, first-use clarity, immersion and listening remain
**EN PRUEBA**. The agent did not hear the clips.

`continuous/`: 77 preserved files from the final-code title-to-ending route,
PASS426 in 274.99 automatic seconds. Four actual recordings, 14,751 checked Echo
playback frames, all four immutable snapshots compared, zero gameplay fixtures
or unexpected pause resumes. Actual second E hangs up the phone. Both new phone
and inherited Porter ending are included. Console: zero errors / four warnings.
Automatic duration is not an estimate of human playtime.

`restored-review.txt`: separate fresh scene and one public `RestoreAtReview`
fixture; answered/key/exit and bedroom reserve restore while phone remains in its
cradle, silent and non-interactive. No preferences changed; this was not the
Continue UI or another full campaign run. Exact disposable probe source is saved
alongside the result.

Build `build_17919827eb7d`: Succeeded, 196,686,065 bytes, 16.007 seconds,
zero errors / 346 warnings. Universal arm64/x86_64 and strict deep signature pass.
After the earlier locked-Mac limitation, native UI became available: explicit app
path launch, title, Enter, 24 W pulses, actual 1.6-second RECALL, Escape pause/resume
and Command-Q passed. Process exited; Player.log error-pattern count zero.
`native-check.json` records the deliberately limited scope. No audio audition or
native whole-campaign/mouse-comfort claim.

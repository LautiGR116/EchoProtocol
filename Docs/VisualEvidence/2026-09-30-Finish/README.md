# Finish edition — evidence and limits

Scene: `Assets/_Game/Scenes/Production/EchoProtocol_Finish.unity`.
Date uses the repository/client calendar: 2026-09-30.

## Objective runs

- `full.result.txt`, `full.checks.txt`, `full.csv`: PASS, 142 checks, 14 real Input
  System RECALLs. Entire campaign from initial menu to ending; movement uses the
  actual CharacterController, camera E ray and recording/playback systems.
  Sampled Echo poses and recording object identity remain unchanged.
- `intro.*`: PASS, 10 checks. Reception letter reached by actual movement and E;
  reading pauses simulation and does not advance the first act.
- `housebare.*`: PASS, 29 checks. Explicit isolated house setup, then actual
  walking/E input, telephone and ordered circuits; exits with zero documents read.
  This is not an independent full-campaign run.
- `checkpoint-checks.txt`: 30 checks of chapter save/restore, invalid checkpoint,
  empty recording and correct origin. Prior user preference restored afterward.
- `pressure-preview-checks.txt`: aligned Player gets a visual preview, wrong yaw
  is rejected, Player preview never supplies mechanical pressure.
- `reader-checks.txt`: P1 trigger/indicator contract retained after render change.
- `practical-checks.txt`: 29 diffuser material slots extinguish with source lights.
- `audio-files.json`: exact hash, mono, PCM, finite signal, no clipped samples for
  all ten new recorded derivatives. These are file checks, not a listening test.
- `configuration.txt`: scene, six optical readers, 17 stations, no missing scripts.

The full run preceded final model orientation, round lenses, lamp tracking and
small identification refinements. Subsequent intro/house/input and targeted
checks cover those changes; the completed full run is not relabeled as a human
playtest of the final binary.

## Visual inspection

`movement/00–40.png` and `frames.csv`: Game-view samples every 12 simulated
seconds during the full input route, including changing positions, chapters,
active audio sources and shadow-light counts. `house-play-*`: final house route
samples after orientation corrections; the close presence occurs in that route.

`30–37-*`: actual Game-view captures of the reception letter, reading panel,
valve, pressure reader and first-act optical reader. Named `staged` means the
Player pose/state was prepared for inspection; it does not prove route traversal.
`34`/`35` document rejected low-contrast lenses. `36` uses the final self-lit lens.
`37` shows work progress at fraction 0.375 in a prepared live circuit interaction.
The redundant E prompt was subsequently hidden while that work is active.

`view-*` and `iteration-1/` are authored inspection-camera renders, not movement
proof. View 1 intersects service geometry and is not used as quality evidence;
`32-valve-detail-staged.png` provides the usable view of that control. Other
historical captures retain the presentation they actually show.

No audio listening was possible in this model session (audio input unsupported).
Fear, attribution, clarity without help, pacing, duration and listening comfort
require a human run; no result for them is inferred from the checks above.

## Native delivery

`build-final.json`: build_2da095c64a76, universal macOS, zero build errors;
345 pre-existing inference-shader warnings and one disabled-runtime-Pipeline
warning. `native-ui-checks.txt` records observed RC2 UI behavior and the defects
found there. `native-headless.json` is the final binary's successful process
boot/shutdown with no graphics; it is not a visual test.

`near-echo-0/1/2-staged.png`: baseline/no Echo, Echo at 0.08 m and at 1.6 m,
rendered with the actual camera pipeline and prefab after the near-plane fix.
The final binary also makes menus opaque. Repeating those last two changes in
native UI was blocked by the macOS lock; manual unlock was requested. No OS lock
or permission was bypassed. Visual first-time control/comfort testing remains
human work.

# Complete edition — objective evidence, 2026-09-30

Production scene: `Assets/_Game/Scenes/Production/EchoProtocol.unity`.

- `full-route.result.txt`: PASS. `full-route.checks.txt`: 70 checks.
- `full-route.csv`: physical input-driven route, original spawn → P1/SE1 →
  domestic checkpoint → P2/SE2/SE3 → R1 → P3 → R3 → D1 → acceptance → exit.
  Ten real R presses; E uses the actual interaction ray. No checkpoint teleport
  establishes this route. Playback poses are checked against the immutable
  snapshot throughout. Runtime was 245 game seconds including negative tests;
  this is not a human-duration or fear measurement.
- `geometry-audit.txt`: 754 supported floor points, sealed initial exit, no
  missing scripts, early final-terminal rejection.
- `audio-signal-checks.txt`: actual sample-processor bounds and mono agreement.
- `pause-cursor-checks.txt`: real Input System Escape freezes/resumes recording
  and preserves cursor-lock preference on simulated focus regain.
- `build-report-final.json`: final universal StandaloneOSX build, zero errors.
  One expected warning: Pipeline disabled in Player. Initial report preserves
  pre-existing package shader warnings from first compilation.
- `standalone-final-checks.txt`: native app checks and their limits. Synthetic
  mouse drags did not verify look; a human mouse/cursor pass remains necessary.
- `15-ending-menu.png`: composited final menu. Other numbered and named captures
  mostly render the Player camera and omit GUI overlays. The final opaque title/help menus were also checked natively.
- `source-preservation.json`: comparison against the pre-goal source archive.

The harness was corrected after an impossible diagonal doorway waypoint, a P2
assertion earlier than the actual recorded approach, and a final waypoint that
stopped just before the completion volume. These failed passes do not count as
successful tests. New world-label sizes/backface visibility, help opacity and an
Escape/cursor ownership conflict were found during inspection and fixed.
After the full pass, only menu readability/title and Escape ownership changed;
the targeted pause test and final native smoke cover those changes.

No human fear, sound comfort, spontaneous comprehension or pacing approval is
claimed. Source backup and five-minute presentation are in OVERNIGHT_REPORT.md.

The build serialized the existing `Assets/InputSystem_Actions.inputactions` as a
preloaded asset in ProjectSettings; this is the only additional project-settings
change beyond the selected production build scene. The baseline scene files,
packages, art and previous work remain byte-identical to the startup archive.
Focused script/document whitespace checks pass; the whole dirty-tree check also
reports pre-existing Unity-serialized blank values in EchoPrototype.unity, which
were preserved. The final ZIP passed CRC checks; see package-check.json.

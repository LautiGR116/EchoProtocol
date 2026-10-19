# Browser playtest

User-requested WebGL port of the saved Incursion / La embestida candidate,
published 2026-10-19: https://lautigr116.itch.io/echo-protocol.
The ordinary development workflow remains Editor-only; publishing builds is an
explicit exception requested for this release.

## Automatic updates from this Mac

The user authorized a local publisher on pushes to `origin/main`, with no paid
services. `Scripts/web_deploy.py install` installs a repository-local pre-push
wrapper (preserving the existing Git LFS hook) and a macOS LaunchAgent. Other
branches do not publish. The hook captures the exact pushed Git commit plus the
saved private scene/prefab/model allowlist; unsaved Editor changes and dirty
tracked files are not part of that release. Private sources remain on this Mac.

The worker confirms the commit is actually on GitHub before building. It uses a
separate cached Unity project at `~/Library/Application Support/EchoProtocol/WebDeploy/project`,
at reduced scheduling priority. It does not drive or close the user's Editor.
Only build success and the required web files are checked; there are no automatic
playthroughs or campaign tests. Identical build inputs skip compilation. If a
newer push arrives during a build, the older output is not submitted.

butler publishes to `lautigr116/echo-protocol:web`, preserving the public page URL.
Its first channel upload must be selected as **played in the browser** in itch's
editor once; subsequent uploads replace that channel. A failed build is retained
for diagnosis and is not retried automatically. Upload interruptions retry up to
three times, reusing the completed build. The prior published build remains live
until itch accepts/processes the replacement. Already-open games need reloading.

Requirements: this Mac awake, user logged in, network, Unity 6000.3.21f1 with Web
support and valid local license, Python 3.12+, Unity CLI, and official butler.
The existing local butler login is used; no API key belongs in Git or a public
log. Runtime configuration, credentials and caches are outside the repository.
After sleep/network loss the queued job resumes when this user session is active.
Only pushes through this checkout's Git hook enqueue releases; pushes from other
computers/GitHub need `enqueue` here. Save the private scene before pushing.

From the repository:

```sh
python3 Scripts/web_deploy.py status     # latest job and published commit
python3 Scripts/web_deploy.py pause      # defer publication; an active build can finish
python3 Scripts/web_deploy.py resume
python3 Scripts/web_deploy.py retry      # explicitly retry the latest failed job
python3 Scripts/web_deploy.py enqueue    # publish current HEAD plus saved private assets
python3 Scripts/web_deploy.py uninstall  # restore original pre-push hook; retain local data
```

`enqueue` still requires HEAD to match remote main. A new push is captured before
network completion; if it is not confirmed within 30 minutes, enqueue again.
The worker wakes on queue changes or once per minute and exits immediately when
idle. Two completed/failed snapshots are retained; older task-owned snapshots are
pruned. No automatic removal of the user's ordinary Builds folder occurs.
Run `install` again after changing the publisher script to update its local copy.
Focused publication-guard checks: `python3 -m unittest discover -s Scripts -p test_web_deploy.py`.

Setup verification on 2026-10-19: 11 focused guard checks passed, including a
real temporary Git snapshot with dirty tracked files, local LFS hydration and
private-asset snapshot independence. The repository's actual push ran the
preserved LFS hook and queued the correct commit. One isolated WebGL build
succeeded (0 errors, 354 existing shader warnings, 103.4 s build phase; roughly
two minutes including Editor startup). No game route was replayed for this setup.

## Rebuild and publish

Use Unity 6000.3.21f1 with Web Build Support. Switch the active target to WebGL,
then run **Echo Protocol > Web > Configure release** and let scripts finish
compiling. This saves the project settings and applies the machine-local Wasm
DiskSize optimization. `EchoWebBuild.BuildTo("Builds/<new-folder>")` builds the
Incursion scene explicitly; the general scene list still points to Coverage.
The scene and licensed body sources must exist locally and remain outside public
Git. Use a fresh output directory; previous builds are preserved.

Zip the **contents** of the web output, with `index.html` at the ZIP root. Include
`Docs/CREDITS.md` as `CREDITS.txt` and the font OFL files from
`ArtSource/Design/Folio/Fonts`. On the existing itch project use type **HTML**,
select **This file will be played in the browser** on the web ZIP and
**Click to launch in fullscreen**. Keep the macOS ZIP as a separate download.
Do not enable mobile-friendly or SharedArrayBuffer: this port uses keyboard,
mouse and single-threaded WebAssembly. itch supplies gzip response headers.

## Port scope

- A Web-only Forward URP profile keeps post-processing and uses 0.85 render scale,
  1024 shadow maps and 30 m shadow distance. Canvas pixel ratio is 1. The PC and
  Mobile pipeline assets are unchanged.
- Gzip, browser-native decompression, caching, Medium managed stripping,
  256 MB initial / 2048 MB maximum Wasm memory; no development/QA define.
- Unity Web does not run `OnAudioFilterRead`. The browser AudioWorklet duplicates
  SessionAudio's shared stereo peak envelope, mono option, reduced dynamics and
  final fade. It never boosts the signal. Playback remains blocked if this audio
  stage fails to initialize.
- `EchoWebAudio.jslib` routes current and future Unity channel gain nodes through
  that master stage. It depends on the channel factory in Unity 6000.3.21f1's
  `WebGLSupport/BuildTools/lib/Audio.js`; revisit this bridge on Editor upgrades.
  Unity still owns source gain, spatial position and the master volume setting.
- Native per-source filter effects are not reproduced by Unity Web; the web mix
  is not claimed to be acoustically identical to the native player.
- Losing browser pointer lock pauses the game. A rejected capture is handled
  without Unity's fatal alert; an explicit click can recover capture and resume.
  Quit returns to a paused browser cover. Keyboard capture is limited to the
  focused canvas. Browser storage auto-sync preserves the existing checkpoint.

## Verification

Final r3 WebGL build succeeded with 0 errors and 354 warnings in 122.5 seconds.
Warnings include existing shader warnings; browser runtime also reports the
existing telephone negative-scale collider and dropped additional-light shadows
under the smaller web shadow budget. No claim of native visual/audio parity.

Focused processor checks passed for peak limits, mono channel identity and the
final fade. A bridge check covered existing/new channels, reconnects, option
changes and cleanup. Desktop Arc local checks covered menu, pump rendering,
movement/camera, one-Escape pause, checkpoint restoration, audio settings and
capture rejection/recovery. Browser audio initialization confirmed the master
worklet was ready. No campaign replay or approval of fear, acting or headphone
comfort is implied by these checks. Other devices/browsers need human feedback.

The public itch page was opened in desktop Arc without an itch login. Run game
loaded the hosted r3 package, its audio-ready gate completed, and the menu, pump
entry, keyboard movement and mouse capture responded inside itch's iframe.
The optional macOS download remains listed. The earlier web candidate is hidden.
The temporary local server, test game instances and task-owned Unity processes
were stopped after verification.

Release archive: `EchoProtocol-La-Embestida-Web-20261019-r3.zip`, 94,075,000 bytes.
SHA-256: `49b4bbc79ffe8a92232235e4a152127ea972c79fc28ddaf9f05198dbe13ec9cc`.

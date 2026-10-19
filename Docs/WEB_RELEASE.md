# Browser playtest

User-requested WebGL port of the saved Incursion / La embestida candidate,
published 2026-10-19: https://lautigr116.itch.io/echo-protocol.
The ordinary development workflow remains Editor-only; publishing builds is an
explicit exception requested for this release.

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

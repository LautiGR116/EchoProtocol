# Overnight goal — preflight, 2026-09-30

Preparation evidence only. No production goal started and no new gameplay
implemented. Execution authority and launch prompt: [OVERNIGHT_GOAL.md](../../OVERNIGHT_GOAL.md).

## Checked locally

- Unity 6000.3.21f1 is open on `/Users/lautigr116/EchoProtocol`, Pipeline port 7800.
  CLI `editor_status`, `eval`, Console, Play/Stop and screen capture all responded.
  Unity MCP tools are not exposed in this chat; the CLI provides a verified path
  for authoring through the running Editor rather than scene YAML.
- Active scene: `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`.
  903 GameObjects including inactive objects; zero missing MonoBehaviour scripts.
  One `VS1DemoEnding`; 17 explicit D1 references and four ending references assigned;
  two D1 ambience sources. See [scene-references.json](scene-references.json).
- Entered Play Mode, captured the composited screen, read status/Console and
  exited. Captured Console reports zero Errors/Exceptions. One Unity AI account
  availability warning is recorded; startup also reported shadow atlas reduction.
  This is an idle-start smoke check, **not** a full route, input or human fear test.
- Final Editor state: ready, not compiling, Play Mode stopped, active scene clean.
  See [edit-status.json](edit-status.json) and [edit-console.json](edit-console.json).
- StandaloneOSX is selected, with VS1 the only enabled build scene. See
  [build-settings.json](build-settings.json). No new build was generated here;
  Demo-20260928-02 remains the latest prior delivery build.
- Blender 5.2.0 LTS responds from
  `/Applications/Blender.app/Contents/MacOS/Blender`.
- Bundled Python includes NumPy and Pillow, but not soundfile/scipy. macOS
  `/usr/bin/afconvert` is available; ffmpeg/ffprobe were not found on PATH. Use
  existing PCM/WAV workflows or afconvert rather than assume those tools exist.
- Integrated image generation is exposed, with its skill available. It was not
  invoked, quota-tested or used to create an asset during this preparation.

## Recovery

Pre-edit current-source backup (including untracked production files):
`/Users/lautigr116/EchoProtocol-backups/overnight-prep-20260930-153119/workspace-source.tgz`.
Its adjacent `manifest.json` records SHA-256
`2e896e8139e1c62a37e99a8f527d885bc5ae4be5a2440b726238ed4946b65d23`,
44,330,387 bytes, 522 archive members and required VS1/D1/ending files.
Caches, builds, Git internals and credential-file extensions were excluded.
This is a source snapshot, not a Git-history backup.

After preparation, 303 archived files under Assets, ArtSource, Packages and
ProjectSettings matched their current contents byte-for-byte. Documentation
updates are intentional and excluded from that comparison. See
[source-preservation.json](source-preservation.json). No commit or push was made.
Create a fresh source backup at goal startup if the user has made later changes.

## Commands confirmed on this machine

```sh
/Users/lautigr116/.unity/bin/unity status
/Users/lautigr116/.unity/bin/unity command --project-path /Users/lautigr116/EchoProtocol editor_status --json
/Users/lautigr116/.unity/bin/unity command --project-path /Users/lautigr116/EchoProtocol get_console_logs --limit 30 --json
/Users/lautigr116/.unity/bin/unity command --project-path /Users/lautigr116/EchoProtocol --query eval --detail full
/Users/lautigr116/.unity/bin/unity command --project-path /Users/lautigr116/EchoProtocol --query build --detail full
```

Only if no Editor is already running:

```sh
open -a /Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app --args -projectPath /Users/lautigr116/EchoProtocol
```

`unity open` did not return promptly during preparation; launching the verified
application directly succeeded. Do not wait indefinitely or launch a second Editor.
`eval --code` ran read-only C# and accessed UnityEditor APIs and project types.
`eval_file` and `run_script` are listed, but were not executed here.

`editor_play` triggers domain reload: an immediate following request failed with
a temporary network error; subsequent requests succeeded. Wait for the ready
state and retry a bounded read instead of treating this as a project failure.

Screenshots: `capture_game_view --source screen --width 1280 --height 720 --json`
returned `data.result.base64`, decoded to [play-screen.png](play-screen.png).
This captures the composited Play Mode backbuffer. Camera capture also worked:
[start-camera.png](start-camera.png). The command's `save_path` was constrained
to Assets even for an absolute Docs path; its temporary output was moved to Docs
and only its newly created folders/metadata removed. For future evidence, decode
the inline base64 to Docs instead of importing screenshots into Assets.

The discovered `build` command queues an Editor build and `build_status` reports
completion; discover its current parameters before execution. Do not call the
legacy `VS1DemoDeliveryEditor.BuildMacOS()` unchanged: it targets Demo-01.

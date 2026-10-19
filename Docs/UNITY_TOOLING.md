# Unity tooling — verified 2026-10-08

## Installed and enabled

- Official Unity Codex plugin: `unity@unity-agent-plugin`, version
  `0.1.8-beta`, with 32 skills. Source:
  https://github.com/Unity-Technologies/unity-agent-plugin.
- Unity CLI: `1.0.0-beta.13`, updated from beta.12 using its official updater.
  Executable: `/Users/lautigr116/.unity/bin/unity`.
- Codex MCP entry `unity_mcp` now launches that CLI's `mcp` command, pinned to
  `/Users/lautigr116/EchoProtocol`. The previous relay configuration has a
  private backup outside this public repository.
- Existing Editor `6000.3.21f1`, Pipeline `0.6.0-exp.1`, URP `17.3.0`, and
  Assistant `2.17.0-pre.1` were retained. No Unity credit subscription or
  paid service was purchased.

## Objective verification

- Plugin listing reports installed and enabled.
- CLI discovers the project with state `ready`.
- A live C# query succeeds and identifies URP and
  `Assets/_Game/Scenes/Production/EchoProtocol_Reconstruction.unity`.
  The scene is not dirty; script compilation is idle.
- Console error query returns zero entries.
- The official MCP server initializes, exposes 149 tools, and successfully
  calls `editor_status` against this Editor.

This verifies the development connection, not campaign behavior or artistic
quality. No gameplay or scene edits were made for these checks.

## For the next production turn

Use the official `unity-cli` skill and the relevant task skills. Prefer direct
CLI commands for routine Editor work; MCP is available when useful. Pass the
project path explicitly, discover the live command schemas, and keep responses
bounded. Installed skills become available on the next turn; verify their
availability before relying on them.

Keep a single agent responsible for Editor mutations. Preserve existing work,
edit scenes through the Editor, and verify from the player camera. The current
selected scene is now Contraprueba, derived from the preserved Reconstruction
base by the subsequent chapter goal. Its behavior/build evidence is separate in
`CONTRAPRUEBA_CANDIDATE.md`. The connection checks above describe the earlier
tooling setup; installing tools alone did not start that development goal.

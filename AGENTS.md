# Echo Protocol repository guidance

## Product guardrails

- Optimize for impact per development cost for one developer.
- Preserve the perceived arc: useful Echo, apparently unreliable Echo, feared
  Echo. The Echo never fabricates or alters data it actually recorded; the
  protagonist's memory, missing context, and the Player's interpretation become
  unreliable. Do not imply that unrecorded channels were reproduced.
- Establish deterministic Echo rules before introducing anomalies.
- Design horror to affect the real Player perceptually, not to depend on the
  narrative significance of an ordinary prop. A detectable change is not yet a
  successful scare.
- Treat sound, music, silence, direction, proximity, and repetition as primary
  horror tools. Prefer authored variants and consequences over random scare
  scheduling or arbitrary mechanical corruption.
- Use puzzles to create vulnerability: make the Player wait, listen, turn away,
  revisit known space, separate from the Echo, or commit to RECALL. Do not add
  short disconnected puzzles merely to fill a 30–45 minute target.
- Recommend headphones only at a comfortable volume. Never require high volume
  or create impact by unsafe loudness; use position, attack, spectrum, contrast,
  movement, and subtraction. Preserve speaker and reduced-dynamics paths.
- Do not introduce paid services, large asset packages, or major frameworks
  without explicit approval.

## Unity workflow

- Use Unity 6000.3 LTS, URP, C#, and the Input System.
- Prefer small MonoBehaviour components and explicit serialized references.
- Use Unity MCP or the Editor for scenes, prefabs, GameObjects, components, and
  serialized references. Do not normally hand-edit `.unity` or `.prefab` YAML.
- Preserve existing worktree changes. Inspect before changing project-wide
  settings or deleting assets.
- A script on disk is not a completed feature: compile, inspect the Console,
  inspect configured objects, test objective behavior, and name any required
  human playtest.

## Source control and security

- This repository is public. Never print or commit passwords, API keys, tokens,
  credentials, private certificates, or machine authentication files.
- Stop and report any suspected project or Git secret before staging or
  committing it.
- Do not commit `Library`, `Temp`, `Logs`, `Obj`, `UserSettings`, or builds.
- Keep commits focused. Never force-push, rewrite history, or discard dirty work
  without explicit approval.
- Review large binary assets and licensing before import; use LFS deliberately.

## Living documentation

Update only the relevant concise document under `Docs/`. Architecture describes
systems that exist or are approved, not speculative frameworks. Record subjective
feedback in `PLAYTEST_LOG.md`; player feedback is evidence, not an automatic
command.

Before proposing feature work, read `README.md` and the current milestone in
`Docs/ROADMAP.md`. Use `GAME_DESIGN.md` for product scope, `HORROR_DESIGN.md` for
the fear arc, `AUDIO_DESIGN.md` for sonic rules, `ARCHITECTURE.md` for implemented
systems, and `PLAYTEST_LOG.md` for human evidence. Keep these states distinct:
implemented, objectively verified, human-approved, approved direction,
planned, `TBD`, `EN PRUEBA`, and `NO MVP`.

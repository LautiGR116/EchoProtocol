# Echo Protocol repository guidance

## Product guardrails

- Optimize for impact per development cost for one developer.
- Preserve the central arc: useful Echo, unreliable Echo, feared Echo.
- Establish deterministic Echo rules before introducing anomalies.
- Keep the first vertical slice greybox-only. Do not add Echo recording before
  the first-person movement checkpoint is human-approved.
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

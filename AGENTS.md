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
- Current production workflow is **Unity Editor only**. Unless the user explicitly
  requests otherwise, do not build, package, sign, or launch standalone versions
  of the game. Older build-delivery instructions in Docs are historical and do
  not override this rule. Deliver saved scripts/assets/scenes for Editor play.
- Keep verification small and tied to the change: check compilation/Console,
  inspect the affected configuration, and use the shortest relevant Editor check
  when behavior or presentation needs it. Reuse existing evidence and checkpoints.
  Repeat a check only after a relevant edit or a concrete failure; do not repeat
  successful routes or equivalent cases by routine.
- Full-campaign runs, broad regression suites, exhaustive capture/comparison
  rounds, and profiling campaigns are not default delivery requirements. Reserve
  them for an explicit user request. Investigate a concrete regression with
  focused Editor checks; do not turn a local fix into a general audit.
- Prioritize visible implementation and fast handoff to the user's playtest.
  Do not create new test harnesses or lengthy reports for routine polish. Report
  the changes, the small checks actually performed, and any remaining human
  judgment briefly. Automated checks cannot approve fear, acting, or comfort.
- Stop task-owned capture/profiling helpers after use. Do not leave game copies
  or auxiliary tools consuming CPU, and preserve the user's Editor session.

## Generative audio workflow

- When ElevenLabs could materially help, brief the user before generation:
  explain the narrative purpose, whether the need is a new voice or a new take,
  identify the exact saved voice/model/text/settings, and define what the user
  should listen for. The user generates and judges performance by ear; objective
  file and signal checks do not replace that human casting decision.
- Keep generated voices and takes traceable. Record plan/usage rights, model,
  voice identity, prompt or performance text, settings, date, and selected master
  before importing an accepted file. Treat unclear rights as prototype-only.
- Never upload, clone, or imitate a minor's voice, and never work around a
  provider safety block. Use consenting adult performers or policy-compliant
  synthetic adult voices when youthful characterization is required.

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

## Codex task recommendation

- In every user-facing response, explicitly recommend whether to continue in
  the current Codex chat or open a new one, and name the recommended Codex model
  and reasoning effort.
- Re-evaluate this recommendation for each request. Prefer the current chat for
  work that depends on the active milestone, scene, decisions, or test evidence;
  recommend a new chat for a large independent task whose separate context would
  materially improve clarity or reduce risk.
- Keep the recommendation concise and visible using the labels `Chat`, `Model`,
  and `Effort`, even when no chat or model change is needed.

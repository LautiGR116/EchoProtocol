# SD1 whisper generation brief — planned, not generated

Status: **planned prototype take; no audio generated or imported yet**.

## Narrative purpose

This is SD1's single exception to world-locked sound: an intelligible adult
whisper that remains near the Player's right ear and briefly challenges whether
the source belongs to the room, the protagonist, or the Player's physical
surroundings. It carries no essential puzzle fact and must not identify a child
or imitate any real person.

This needs a **new prototype voice**, not another take from an established cast
member. Voice identity remains narratively `TBD`.

## Exact first audition

- Provider: ElevenLabs, user-operated generation and selection by ear.
- Saved voice: `Rachel`, voice ID `21m00Tcm4TlvDq8ikWAM`; documented as an adult,
  middle-aged American voice. Treat it as a replaceable neutral casting probe.
- Model: Eleven v3, model ID `eleven_v3`.
- Input text: `[whispers] Can you hear me?`
- Stability: `Natural`.
- Similarity, style exaggeration, speaker boost, and speed: unavailable for v3;
  do not substitute hidden defaults from another model.
- Generate once to receive the two v3 alternatives. Generate one additional
  pair only if neither is intelligible and restrained.
- Export the selected take without added reverb, stereo widening, normalization,
  or horror effects. Post-treatment and level belong to the project pass.

The line heard by the Player must remain exactly `Can you hear me?`; the bracketed
tag is delivery direction and must not be spoken. The subtitle remains
`¿Me escuchás?`.

## Human listening gate

At a comfortable volume, select only a take that is:

- unmistakably adult and not child-like;
- intimate and restrained rather than seductive, theatrical, demonic, or ASMR;
- intelligible on the first listen without raising system volume;
- free of added words, sighs, laughs, inhalations, clicks, metallic artifacts,
  accent changes, or unstable pitch;
- close enough to feel perceptual, but not so breath-heavy that it causes
  physical discomfort.

Reject all takes if the voice identity becomes more narratively specific than
the current design supports. A user-selected take still needs an integrated
right-ear/head-locked audition, speaker fallback, subtitle timing, and signal
review before SD1 can be called implemented.

## Rights and traceability gate

Before import, record generation date, account plan, model, exact voice name and
ID, input, settings, downloaded filename, file hash, selected alternative, and
any project edit. Current ElevenLabs guidance says free-plan output is
non-commercial and requires attribution when published; v3 is described as a
research-preview/alpha model. Unless the generation record proves broader
rights, mark the take **prototype-only**, retain required attribution, and do
not treat it as production-cleared.

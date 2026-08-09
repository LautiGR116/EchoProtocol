# Audio design

Audio is a primary horror system, not decoration. It first teaches a stable,
causal world and later creates doubt through absence, context, distance, and
memory. Loudness cannot substitute for construction.

## Status

- **Human-approved:** the M6.1 clinical hum is audible as a stable baseline and
  its fade to sustained silence at the chair reveal works as intended.
- **Human-approved greybox baseline:** M6.2 separates `Ambience` and `WorldSFX`
  and gives the active sliding door one deterministic 3D motor and endpoint cue.
  Its causal timing is approved; its procedural timbre was judged synthetic and
  remains a placeholder for later material, friction, and creak design.
- **Planned:** footsteps, a RECALL signature, breathing, domestic memory sounds,
  music, voice, stronger discrepancies, and loud peaks.

Approval of one prototype cue does not validate the final mix or prove that the
whole scene creates enough discomfort.

## Trust contract

- Mechanical feedback tells the truth. A door motor plays only while its panel
  physically moves; plates, doors, goals, and RECALL must never lie about state.
- The same cause produces the same timing, pitch, level, and position unless a
  later variation is explicitly designed and tested.
- Echoes remember correctly. When audible actions are eventually recorded, an
  Echo must reproduce their original timestamp, position, surface, and variant.
- Horror can challenge the protagonist's interpretation, remove a familiar
  layer, or place memory in an impossible context. It must not imply arbitrary
  Echo corruption or secretly change puzzle truth.
- Every puzzle remains solvable and every essential narrative fact remains
  available without relying on hearing alone.

## Sound layers

1. **Functional truth:** plates, doors, goals, RECALL, Player, and Echo.
2. **Facility body:** ventilation, fluorescent hum, electricity, pipes, and
   distant machinery.
3. **Human memory:** restrained domestic materials and objects that gradually
   invade the clinical palette.
4. **Discrepancy:** missing layers, unexpected proximity, uncertain source, or
   familiar context that no longer fits the room.
5. **Peak and aftermath:** a rare earned event followed by recovery and reduced
   density, not a random volume spike.

Room tone and music are normally 2D. Physical sources are 3D, fixed to their
cause, use no Doppler unless later justified, and must remain readable in stereo
speakers, headphones, mono, and reduced dynamic range.

## Five-chapter escalation

1. `Protocol`: stable facility, clear mechanisms, readable Echo, no dramatic
   silence or loud peak.
2. `Familiarity`: one missing environmental layer and a quiet domestic detail;
   unease without confirmed threat.
3. `Recollection`: personal materials and a short tonal memory motif emerge;
   Echo context contradicts what the protagonist recalls, not what occurred.
4. `Denial`: incompatible spaces coexist sonically while mechanical feedback
   remains exact. The first strong peak may occur here.
5. `Acceptance`: facility and memory converge, the motif resolves once, density
   falls away, and the final cut may use true silence.

Ambient sound should carry most of the experience. Music remains sparse and
serves resolution or transition rather than continuously announcing danger.

## Strong peaks

- Working maximum: two in a 30–45 minute game, widely separated and never in
  the opening rule-teaching section.
- A peak needs narrative cause, several seconds of preparation, a short event,
  and a clear recovery period. It does not trigger from routine RECALL, puzzle
  failure, pause, or random scheduling.
- Target contrast is at most roughly `10–12 dB` over its local context with
  conservative master headroom and no clipping. Perceived safety is validated
  on real output hardware; source volume alone is not a safety measurement.
- Planned accessibility includes separate ambience/effects controls, reduced
  sudden-sound and low-dynamic-range options, mono compatibility, subtitles or
  visual equivalents, and a content warning that does not reveal exact timing.

Pain, tinnitus, blocked-ear sensation, dizziness, panic, or sustained physical
distress immediately fails a test. Emotional discomfort is intended; physical
harm is not.

## Solo-production limits

Use small scene-local components and explicit references. The current mixer has
only `Master`, `Ambience`, and `WorldSFX`. Do not add a global audio manager,
generic scare scheduler, middleware, emitter pool, dynamic occlusion, reverb
framework, or adaptive-music system until a concrete approved feature and
profiling justify it.

## Next checkpoint

M6.3 should add one restrained environmental audio discrepancy whose effect is
legible only because M6.1 and M6.2 established a stable ambience and truthful
mechanism layer. It must not imitate a door state, falsify an Echo timestamp, or
depend on a random loud peak. Its exact sound and trigger remain unapproved until
the smallest concrete proposal is reviewed and tested.

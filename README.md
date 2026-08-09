# Echo Protocol

Echo Protocol is a first-person psychological horror and temporal puzzle game
about cooperating with recordings of your own past actions. The mechanic begins
as a reliable puzzle tool and later becomes the source of uncertainty.

This is a solo university project targeting a short, polished 30–45 minute game.
The greybox first-person movement and center-camera interaction checkpoints are
human-approved. Mouse sensitivity remains `0.15`. The first temporal recording,
reset, and Echo playback checkpoint is also human-approved. The first
pressure-plate/door puzzle and its timing are now human-approved. M5, normal
puzzle language, is complete: M5.1 and M5.2 are human-approved. M6.1 now adds one
objectively verified audiovisual discrepancy without changing Echo playback or
puzzle rules. The visual-only pass was noticed but did not create discomfort;
the clinical hum and its disappearance are now human-approved. M6.2 adds the
first objectively verified piece of trustworthy spatial audio. Its causal
door-audio baseline is human-approved; the procedural motor was explicitly
judged synthetic and remains a placeholder for the later sound-design pass.

## Development setup

- Unity `6000.3.21f1` (Unity 6.3 LTS)
- Universal Render Pipeline `17.3.0`
- Input System `1.20.0`
- macOS Apple Silicon is the primary development environment

After cloning, install Git LFS locally and open the repository root from Unity
Hub. The development scene is
`Assets/_Game/Scenes/Development/EchoPrototype.unity`.

## Current controls

- Move: `WASD`
- Look: mouse
- Interact: `E`
- Finish/reset the current attempt: `R`
- Release cursor: `Escape`
- Capture cursor again: left click in the Game view

An attempt also resets automatically after 60 seconds in the current prototype.
The recorder remains armed while the Player is idle at spawn. The first real
movement starts the timeline with a fixed one-second lead-in; every later pause,
including holding a pressure plate, is reproduced at full length.

## Current checkpoint

M6.2 established one normal audio rule before adding another anomaly: the active
sliding door emits a deterministic 3D motor only while its panel physically
moves, followed by one restrained endpoint cue. The clinical hum is routed
separately, so its M6.1 fade cannot silence mechanical truth. No plate, RECALL,
footstep, Echo, music, or new horror cue was added in this slice.

This behavior is human-approved as a causal greybox baseline. Its synthetic
timbre is not final door sound. M6.3 is next: one restrained audio discrepancy
that depends on the trusted soundscape without changing Echo or puzzle truth.

### M6.2 playtest route

1. Start a fresh Play Mode session. Confirm the clinical hum is present and the
   closed door is silent.
2. Solve the approved `A → B` sequence. The motor should come from the doorway,
   last exactly as long as the panel moves, and end without a click or startle.
3. Release B while the panel is moving. Its direction may reverse, but the motor
   must remain continuous rather than restart or stack.
4. Trigger RECALL while the panel is moving. The door should snap to baseline and
   the motor stop without a false endpoint hit or residual loop.
5. Reveal the M6.1 chair and repeat the door movement in the resulting silence.
   The mechanism must remain equally clear and must not suggest an Echo failure.

## Documentation

- [Product GDD v0.1](Docs/Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.1.pdf)
- [Game design](Docs/GAME_DESIGN.md)
- [Architecture](Docs/ARCHITECTURE.md)
- [Roadmap](Docs/ROADMAP.md)
- [Horror design](Docs/HORROR_DESIGN.md)
- [Audio design](Docs/AUDIO_DESIGN.md)
- [Decisions](Docs/DECISIONS.md)
- [Playtest log](Docs/PLAYTEST_LOG.md)

The GitHub repository is public. Never commit credentials, tokens, private keys,
local authentication files, or generated Unity cache folders.

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
M6.3 now adds one objectively verified, proximity-triggered chair sound and
has passed a guided human one-shot check. That pass produced no terror or
discomfort, so spontaneous attribution and emotional effectiveness remain open.

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

M6.3 gives the revealed M6.1 chair one quiet, localized settling cue. The cue is
inactive with the hidden chair, plays once when the Player deliberately comes
within `1.3 m`, and never rearms through RECALL. It has no collider, visual
movement, random timing, puzzle effect, or Echo dependency.

### M6.3 playtest route

1. Start a fresh Play Mode session, solve the approved `A → B` sequence, cross
   the green goal, and press `R` to reveal the chair and remove the hum.
2. Walk toward the chair against the left wall. A single low settling sound
   should occur only at close Player proximity, not during the puzzle route.
3. Move away and approach again, then complete two more RECALLs. The cue must not
   repeat.
4. Confirm the door still sounds and behaves exactly as before, and that Echo
   playback remains unchanged.

The checkpoint asks whether the cue felt environmental and intentional, where
it came from, whether it created unease, and whether it suggested a broken door
or corrupted Echo. A guided pass confirmed first-play/no-repeat behavior but did
not create terror; an unprimed emotional and attribution pass remains required.

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

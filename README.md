# Echo Protocol

Echo Protocol is a first-person psychological horror and temporal puzzle game
about cooperating with recordings of your own past actions. The mechanic begins
as a reliable puzzle tool and later becomes the source of uncertainty.

This is a solo university project targeting a short, polished 30–45 minute game.
The greybox first-person movement and center-camera interaction checkpoints are
human-approved. Mouse sensitivity remains `0.15`. The first temporal recording,
reset, and Echo playback checkpoint is also human-approved. The first
pressure-plate/door puzzle and its timing are now human-approved. M5, normal
puzzle language, is the next milestone. Horror anomalies have not started.

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

## Current milestone

M5.1 now reuses the approved Echo, plate, door, and reset rules in a two-plate
timing challenge. Its logic and reset behavior pass objective verification; the
human difficulty/fairness checkpoint is pending before any horror interference
is added.

### M5.1 playtest route

1. Observe that either orange plate alone lights one status lamp but leaves the
   door closed.
2. Record an attempt that reaches the left plate and remains there for roughly
   four to five seconds, then press `R`.
3. In the next attempt, wait on the wide centre plate while the cyan Echo reaches
   the left plate.
4. When both lamps are active, let the door open fully and cross to the green
   goal before the condition ends.

## Documentation

- [Game design](Docs/GAME_DESIGN.md)
- [Architecture](Docs/ARCHITECTURE.md)
- [Roadmap](Docs/ROADMAP.md)
- [Horror design](Docs/HORROR_DESIGN.md)
- [Decisions](Docs/DECISIONS.md)
- [Playtest log](Docs/PLAYTEST_LOG.md)

The GitHub repository is public. Never commit credentials, tokens, private keys,
local authentication files, or generated Unity cache folders.

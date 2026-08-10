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
first objectively verified 3D door-audio source. Its causal timing is
human-approved, while spatial localization was not tested separately. AD2 has
now replaced its synthetic motor with a user-selected domestic sliding-door
recording and matching endpoint; the integrated mix still needs a human Unity
checkpoint. M6.3 now adds one objectively verified,
proximity-triggered chair sound and has passed a guided human one-shot check.
That pass produced no terror or discomfort, so spontaneous attribution,
localization, and emotional effectiveness remain open.

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

The current audio path builds toward an integrated 5–8 minute `Familiarity`
foundation proof in the existing room. It first establishes a believable,
repeatable facility body and credible authored causal feedback, then tests one
learned layer disappearing and one restrained chair discrepancy. It does not
validate the complete chapter or its future domestic-memory layer. No new
gameplay or Echo rule is part of this pass.

AD2 has downloaded and reviewed six CC0 recordings outside the project, retained
sanitized license evidence and hashes, and imported eight edited prototype clips.
It replaces the facility-air and door procedurals, adds fluorescent and distant
machinery beds, gives both plates truthful edge cues, and adds successful-RECALL
feedback. Objective wiring/reset checks pass. Only the domestic door A/B pair is
a strong initial direction; the integrated route is now human-approved for
continued prototyping. Every imported clip, including the door pair, remains
explicitly replaceable and no production timbre is locked.

M6.3 remains objectively verified but emotionally open. Additional isolated
anomalies are paused. Its procedural timbre will be replaced by a credible
material sound; approach versus departure-after-inspection will then be tested
as mutually exclusive timing variants once the normal soundscape exists. The cue
will not be duplicated or made louder to create impact cheaply.

The planned order is:

1. **AD1 complete:** shortlist and legally review a small reference palette;
2. **AD2 implemented and objectively verified:** integrate the modest facility
   bed, domestic door, plate edges, and successful-RECALL cue;
3. **AD2 complete at prototype level:** integrated listening approved continued
   development while leaving every timbre open;
4. **AD3 next:** assemble the chair timing into the 5–8 minute audiovisual route;
5. run an unannounced attribution and emotional checkpoint before adding another
   anomaly, voice, music, or a strong peak.

## Documentation

- [Product GDD v0.1](Docs/Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.1.pdf)
- [Game design](Docs/GAME_DESIGN.md)
- [Architecture](Docs/ARCHITECTURE.md)
- [Roadmap](Docs/ROADMAP.md)
- [Horror design](Docs/HORROR_DESIGN.md)
- [Audio design](Docs/AUDIO_DESIGN.md)
- [Audio asset ledger](Docs/AUDIO_ASSET_LEDGER.md)
- [Decisions](Docs/DECISIONS.md)
- [Playtest log](Docs/PLAYTEST_LOG.md)

The GitHub repository is public. Never commit credentials, tokens, private keys,
local authentication files, or generated Unity cache folders.

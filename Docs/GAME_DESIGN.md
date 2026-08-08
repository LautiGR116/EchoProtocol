# Game design

## High concept

The player solves controlled facility experiments by cooperating with recordings
of their own previous actions. Echoes first behave as dependable temporal tools.
After trust is earned, rare inconsistencies make the player doubt the system and
their own memory.

## Design pillars

1. Echoes are the central puzzle mechanic and the eventual source of horror.
2. Rules are taught through reliable repetition before they are violated.
3. Uncertainty, anticipation, and inconsistency matter more than spectacle.
4. Sound is a primary gameplay and horror system.
5. A short, polished solo project is better than a broad unfinished one.

## Difficulty and fear

Puzzle difficulty should come from understanding, planning, memory, and temporal
coordination rather than opaque rules or frame-tight execution. A correct plan
must have a visible, repeatable margin for success, and timing-critical values
remain configurable for playtesting.

Terror may later consume attention, create discomfort, and make the player doubt
their observation. It must not secretly change input, playback speed, pressure
plate truth, door timing, or the result of an otherwise identical attempt. The
player may feel uncertain; the normal system must first remain trustworthy.

## Core loop

Explore, understand an experiment, plan and record an attempt, reset, cooperate
with the resulting Echo, solve the experiment, then notice that something may be
wrong.

## Current mechanics

- Greybox first-person walking and mouse look.
- Collision and grounded movement through a CharacterController.
- Center-camera interaction within a three-metre range using `E`.
- An earlier toggle switch remains as an inactive interaction proof.
- Timestamped player movement recording at 20 samples per second.
- `R` ends an attempt, restores the known baseline, and starts the next attempt.
- One translucent greybox Echo replays the latest attempt and disappears at its
  recorded end.
- Player and Echo can hold an orange pressure plate, including together, to open
  a physically blocking sliding door.
- A green exit pad behind the door completes only for the Player, making the
  first record/reset/cooperate loop objectively solvable.
- The current M5.1 variant requires two simultaneous plates, exposes both inputs
  through status lamps, and gives the threshold plate enough physical depth for
  a fair crossing once the AND condition is satisfied.
- No horror-anomaly system exists yet.

## MVP content target

- One facility with one coherent visual identity.
- Five to eight compact experiments built by recombining Echoes, buttons, doors,
  sensors, platforms, and timing.
- Progressive psychological horror, concise narrative, strong spatial audio, a
  beginning, escalation, climax, and ending.
- Target playtime: 30–45 minutes, reducible to preserve quality.

## Scope exclusions

No multiplayer, backend, open world, crafting, RPG progression, complex combat,
procedural world, large inventory/dialogue framework, or generic chase-focused
horror.

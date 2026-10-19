# Decisions

## 2026-10-01 — Raise ambition and authorize sustained use

The user considers the game insufficiently ambitious and explicitly wants many
hours of autonomous work, with authorization to use all available quota. The
current Finish build is a recoverable checkpoint, not the final quality bar or
a scope ceiling. The [short goal](OVERNIGHT_GOAL.md) now requests substantial
creative production and reconstruction with exacting self-defined outcomes;
earlier prompts aimed at a single finish pass are superseded. The agent remains
free to choose design, narrative, assets and implementation order. Preserve a
working delivery while pursuing the larger result. Reasoning effort does not
guarantee wall-clock runtime; neither token burning nor repeated sufficient tests
count as product progress. No services or monetary spending are authorized.
The goal was prepared for the user to activate, not dispatched from this chat.

## 2026-09-30 — Finish the campaign's player experience

The expanded campaign is implemented and objectively verified. The user still
wants a finished visual, sonic and psychological-horror experience and reports
substantial remaining usage. Continue autonomous production; the creative
freedom remains. The revised [short goal](OVERNIGHT_GOAL.md) focuses on finishing
the experience, including replacing weak work, rather than satisfying more
asset or puzzle counts. Human fear and comfort evidence remains unavailable;
this does not block autonomous improvements or justify claiming those effects
verified. Recommended next execution: Astra Extra High in Standard. The prompt
was prepared, not dispatched to the execution chat.

## 2026-09-30 — Clarify the full-game goal after its first delivery

The first autonomous run delivered a brief edition with session UX and a physical
ending. The user reports that this did not match the intended goal. Treat that
build as a recoverable checkpoint, not fulfillment of the intended substantial
full-game development. The revised [short goal](OVERNIGHT_GOAL.md) clarifies the
outcome: an expanded campaign with new playable challenges and situations,
developed horror/narrative and substantial audiovisual improvement. Design,
content choices and implementation order remain delegated. No detailed puzzle
recipe or routine review gate is introduced. The continuation was not dispatched
to the execution chat by this documentation change.

**Further user clarification:** be ambitious. Aim for the moodboard's atmosphere
and production quality when achievable; otherwise choose an equally ambitious
independent direction. The moodboard is aspirational rather than a prerequisite
or a reason to halt. A deliverable build alone does not justify shrinking the
intended campaign. This is creative authorization, not a claim that the target
quality has been achieved or visually validated.

## 2026-09-30 — Delegate autonomous full-game production

**Direct user authorization:** prepare an overnight goal and let the agent make
all creative and implementation decisions, including new puzzles, controls,
scenes, Blender assets, downloaded audio and generated images. The user explicitly
expanded this beyond the short demo to the full game, or the most substantial
complete experience the agent can build and verify. See
[the execution contract](OVERNIGHT_GOAL.md).

**Latest clarification:** the user explicitly allows questioning what already
exists and wants a short goal, not a detailed prescribed plan. Previously approved
product decisions, content, puzzles, controls, narrative, ending and systems can
be reconsidered, redesigned, replaced or removed. The agent chooses its direction,
scope and execution order. The earlier detailed overnight contract is superseded;
the current brief specifies the desired outcome and delegates the means.

**Retained:** psychological horror as the intended experience, safe listening
levels, a recoverable copy of prior work, public-repository asset rights and
Editor-authored scenes. Design history is context, not a creative prohibition.
No authorization for spending, external publication, messaging third parties,
destroying the recovery copy or rewriting Git history. Local content and systems
may be replaced or deleted after preserving that copy.

**Evidence boundary:** this authorizes future work; it does not implement or
human-approve it. The goal has not been started in this preparation. New agent
selections and unobserved emotional effects remain EN PRUEBA.

## 2026-08-07 — Greybox before content

**Decision:** Validate movement and the Echo proof before final art or horror.

**Why:** It protects the core mechanic and solo-semester scope.

**Tradeoff:** The project initially looks plain and postpones atmosphere work.

## 2026-08-07 — CharacterController locomotion

**Decision:** Use Unity's CharacterController for the first-person capsule.

**Why:** It provides predictable collision and grounded movement without
Rigidbody force tuning, which suits deliberate puzzle navigation.

**Tradeoff:** Physical pushes and advanced locomotion will need explicit handling
if later approved.

## 2026-08-07 — Reuse the template Input System asset

**Decision:** Use its existing `Player/Move` and `Player/Look` actions for the
first checkpoint.

**Why:** The required bindings already exist and avoid premature input-asset
churn.

**Tradeoff:** Unused template actions remain until a deliberate cleanup.

## 2026-08-07 — Deliberate Git LFS policy

**Decision:** Automatically LFS-track common large source/binary formats, but not
every PNG, JPEG, archive, font, or document.

**Why:** The repository had no LFS objects and its broad template rules already
misclassified a small PNG stored as a normal Git blob.

**Tradeoff:** A genuinely large delivery image must be reviewed and tracked by
path before commit.

## 2026-08-07 — First-hit interaction ray

**Decision:** Interact by casting three metres from camera centre and evaluating
only the first physical collider hit.

**Why:** It is simple, predictable, supports child colliders, and prevents using
switches through walls.

**Tradeoff:** There is no reticle, prompt, highlight, or alternate interaction
shape until human testing proves one is needed.

## 2026-08-07 — Timestamped pose snapshots

**Decision:** Record player position and body rotation every `0.05` seconds,
copy the completed attempt into an immutable snapshot, and interpolate playback
by timestamp.

**Why:** It gives visually stable replay without trying to resimulate keyboard
input or physics and keeps a 60-second attempt small.

**Tradeoff:** Camera pitch and semantic interactions are not recorded yet; add
them only when a real puzzle proves the requirement.

## 2026-08-07 — One latest Echo with a predictable end

**Decision:** Retain only the latest completed attempt, replace its Echo on the
next reset, and make normal playback disappear at its recorded end.

**Why:** One Echo is sufficient for the first temporal puzzle, prevents runtime
accumulation, and establishes the reliable rule against which future apparent
discrepancies can be judged.

**Tradeoff:** Multi-Echo puzzles are unavailable until explicitly designed and
approved.

## 2026-08-07 — Explicit loop baseline

**Decision:** Reset only a serialized list of small `ILoopResettable` targets in
addition to the player and Echo lifecycle.

**Why:** The known baseline is testable and scene-specific without scene-wide
searches, reloads, or a premature reset framework.

**Tradeoff:** Every new mutable puzzle component must be deliberately connected
to the loop controller.

## 2026-08-07 — Physical temporal-actor occupancy

**Decision:** Mark Player and Echo roots with a small `LoopActor` component and
let the pressure plate track physical colliders per actor.

**Why:** The first puzzle needs both bodies to obey the same visible spatial rule
while remaining robust when an Echo disappears, is replaced, or owns multiple
colliders.

**Tradeoff:** New physical temporal actors must opt in with the marker; unmarked
physics objects intentionally do not activate the plate.

## 2026-08-07 — One concrete plate-to-door puzzle

**Decision:** Give the first plate a direct reference to one sliding door and
place a Player-only latching goal beyond it.

**Why:** It proves the complete record/reset/cooperate loop with an objective
finish without inventing a generic event graph before another room needs one.

**Tradeoff:** Combining several mechanisms will require a later, evidence-based
extension instead of configuring arbitrary signal chains now.

## 2026-08-07 — Challenge the plan, not the controls

**Decision:** Make harder puzzles demand temporal planning and divided attention
while keeping their physical rules deterministic and execution windows tunable.

**Why:** Future discomfort and terror will already tax attention and memory. If
the base mechanic also changes secretly or requires near-frame-perfect movement,
failure feels arbitrary instead of frightening.

**Tradeoff:** Timing must be measured and human-tested; difficulty cannot be
created cheaply by shrinking every window.

## 2026-08-07 — Concrete dual-plate AND condition

**Decision:** Add one `DualPlateDoorCondition` for M5.1 instead of a generic
signal graph. M4 keeps direct plate-to-door control; M5.1 owns one door through
two otherwise normal plates.

**Why:** It introduces simultaneity and planning with the smallest rule extension
that the second puzzle actually requires.

**Tradeoff:** A later sequential puzzle may justify another small composition,
but arbitrary boolean authoring remains unavailable.

## 2026-08-08 — Arm recording until first movement

**Decision:** Do not accumulate arbitrary idle time at spawn. Arm each attempt
until the Player moves horizontally by `0.02` metres, then begin the recording
with a fixed one-second lead-in. Preserve every pause after movement begins.

**Why:** Echo playback reaches meaningful actions predictably without erasing
deliberate pressure-plate dwell or requiring the player to reset immediately.

**Tradeoff:** Pressing `R` before any movement produces no new Echo. The start
gate currently observes body translation rather than input intent or semantic
interactions.

## 2026-08-08 — GDD governs product direction, not implementation status

**Decision:** Preserve GDD v0.1 under `Docs/Design/` as the primary source for
product vision, narrative, experience, art, audio, UI, scope, and future
direction. Keep repository code, architecture, and playtests authoritative for
what exists and what has been validated.

**Why:** The GDD was written as preproduction direction and its provisional
schedule now trails the working prototype.

**Tradeoff:** Documentation must label future content explicitly instead of
copying GDD features into the implemented architecture.

**Status update — 2026-08-11:** v0.1 remains archived unchanged. GDD v0.2 is now
the current product document and synchronizes this rule with prototype and
playtest evidence.

## 2026-08-08 — Echoes remember; the protagonist does not

**Decision:** Treat correct Echo memory and unreliable protagonist memory as a
central narrative rule. Apparent future anomalies may express omitted cycles,
repressed action, altered perception, or dream logic, but not random corruption
of otherwise identical playback.

**Why:** The horror depends on investigating memory through the same dependable
system the Player learned to use.

**Tradeoff:** Every discrepancy needs narrative causality and cannot be added as
an interchangeable glitch or arbitrary rules violation.

## 2026-08-08 — Confirm Final C

**Decision:** End with the protagonist confronting the memory, the nightmare
ending, and a cut to black without showing whether they wake.

**Why:** It resolves the emotional and causal arc while retaining one restrained
uncertainty instead of invalidating the story with another reveal.

**Tradeoff:** The ending cannot rely on a shown awakening or a final twist that
reframes the confrontation as irrelevant.

## 2026-08-09 — Concrete sequential A-to-B condition

**Decision:** Implement M5.2 with one scene-specific condition that remembers a
rising A press, accepts only a later rising B press, and holds one door open only
while B remains occupied. Use status lamps and visible geometry to force Echo
cooperation without a timeout or actor-specific rule.

**Why:** It teaches ordered temporal planning with the same physical plate and
door language already validated in M4 and M5.1.

**Tradeoff:** The state machine and anti-solo blocker are deliberately concrete;
they do not provide arbitrary sequences, timers, or a reusable signal graph.

## 2026-08-09 — First discrepancy changes the room, not the Echo

**Decision:** After the Player completes the M5.2 goal, reveal one colliderless
institutional chair on the next successful RECALL. Drive the transition through
direct rising-edge goal and loop-completion events, then preserve the chair until
the scene or Play Mode session reloads.

**Why:** A peripheral object can make the protagonist question whether it was
always present while every recorded frame, puzzle rule, and reset remains
trustworthy. It begins the useful Echo → uneasy Echo arc at minimal cost without
inventing corrupted playback or a generic horror director.

**Tradeoff:** The initial visual-only M6.1 had no audio, lighting change,
explanation, randomization, or mechanical consequence. It was noticed in human
testing but did not create discomfort, motivating the focused audio iteration.

## 2026-08-09 — Silence supports the first discrepancy

**Decision:** Establish one quiet, procedural 2D clinical hum and fade it to
sustained silence when the chair's `Revealed` event fires. Generate the loop at
runtime, use no external asset, and add no stinger, localization, randomness, or
audio manager.

**Why:** The visual-only chair proved readable but not uncomfortable. Removing a
previously stable room tone can direct attention through absence while keeping
the environmental change restrained and every Echo rule unchanged.

**Tradeoff:** A synthetic greybox hum is only an emotional prototype, not final
sound design. Its fade is now human-approved, but the broader discomfort and
production mix still require later evidence.

## 2026-08-09 — Teach truthful spatial audio before another discrepancy

**Decision:** Begin M6.2 with one mixer split (`Ambience` / `WorldSFX`) and one
deterministic 3D motor on the active sliding door. Drive it from physical panel
displacement, keep one voice through reversal, play one endpoint cue only after
real travel, and silence it explicitly on loop reset.

**Why:** Horror audio can exploit a learned soundscape only after normal causes,
locations, and timing are trustworthy. One concrete door rule is cheap to test
and cannot be confused with a general sound system or Echo corruption.

**Tradeoff:** Plates, RECALL, footsteps, music, stronger ambience, and new audio
anomalies remain deferred. Human testing approved the causal greybox rule but
judged its procedural motor synthetic; final material, friction, and creak remain
part of the later production sound-design pass.

## 2026-08-09 — Tie the first localized discrepancy to deliberate proximity

**Decision:** Give the revealed M6.1 chair one fixed 3D source and play a low
procedural settling cue once on the first Player proximity within
`1.3 m`. Keep the source under the hidden chair, outside loop reset, with no
randomness, visible movement, or mechanical consequence.

**Why:** A source attached to an inert object can create a small spatial doubt
without reusing the door, inventing an invisible Echo, or making routine RECALL
itself a scare trigger. Proximity lets the Player initiate the moment through
curiosity rather than scripted camera control.

**Tradeoff:** The cue is a greybox timbre and may be missed by a Player who never
examines the chair. Its emotional effect and attribution require human evidence;
failure should be addressed through timing, range, or timbre, not higher volume
or additional simultaneous anomalies.

The first guided human pass confirmed the one-shot behavior but produced no
terror or discomfort. This validates delivery only; it does not close the
emotional or unprimed-attribution question.

## 2026-08-09 — Build the soundscape before adding another anomaly

**Decision:** Pause isolated horror cues and design one integrated 5–8 minute
`Familiarity` audio foundation in the current M5.2/M6 room. First establish a
credible facility body and truthful functional feedback; then subtract one
learned layer, test one chair cue timing, and leave recovery space. This does not
validate the complete chapter or its future domestic-memory layer. Add no new
gameplay, music, voice, or strong peak in the first pass.

**Why:** The hum, door, and chair prototypes validate routing, causal timing, and
one-shot delivery, but the guided chair pass produced no terror or discomfort.
Fear needs a learned normal soundscape, deliberate contrast, and coherent pacing;
more volume or more disconnected events would not supply that context.

**Tradeoff:** New anomalies and narrative content pause while a small palette is
sourced and mixed. The current chair approach cue remains functional but its
departure-after-close-inspection alternative is **EN PRUEBA**. Production assets
require provenance and license review, and emotional approval still requires a
non-primed checkpoint rather than technical verification alone.

## 2026-08-09 — Use a CC0-first candidate palette for the public repository

**Decision:** Shortlist eight CC0 recordings or effects for nine first-proof
audio roles and track them in `AUDIO_ASSET_LEDGER.md`. Treat every file as a
candidate until its canonical page and license are rechecked, the full recording
is auditioned and hashed, its provenance is credible, and its derivative is
reviewed. Do not import a stock, Student-plan, or generated asset merely because
access is free. Keep Unity AI optional for an abstract experiment only after
explicit consent and terms/quota review; defer Blender and Blender MCP until a
concrete 3D task exists.

**Why:** The repository publishes raw assets, so an end-product license can be
insufficient even when a sound is free to use in a game. Predominantly recorded
material is the lowest-cost path away from the synthetic door and room
placeholders, while a small ledger makes attribution, replacement, and public
redistribution auditable.

**Tradeoff:** AD1 locks roles and candidates, not final timbre. Any source can be
rejected during AD2, the pressure-plate release may need a separate recording,
and the RECALL identity remains TBD. Conservative sourcing costs more review
time but avoids building the soundscape on ambiguous files or unnecessary tools.

## 2026-08-09 — Lock the domestic door direction, not the whole AD2 palette

**Decision:** Use movement option A and endpoint option B from the same CC0
domestic sliding-door recording. Import the other accepted AD2 derivatives only
as replaceable prototype material. Their legal and technical acceptance does not
make them the final sonic identity of the game.

**Why:** The domestic material inside an institutional mechanism creates the
specific ordinary-but-misplaced quality the user preferred, while one recording
keeps motion and endpoint coherent. Continuing to collect alternatives before
hearing the complete Unity route would add selection cost without improving the
decision. The other candidates are sufficient to test density, causality,
subtraction, and reset behavior even though none inspired a strong preference.

**Tradeoff:** AD2 can reach an integrated human checkpoint now, but facility,
plate, and RECALL timbres remain expected replacement targets. Later replacements
must preserve the verified buses, timing, ownership, reset, and Echo-trust rules.

## 2026-08-10 — Establish the place before judging the scare

**Decision:** Add one scene-local AV1 presentation layer to the existing
prototype: textureless clinical materials, a closed render-only ceiling, dark
trim, three stable local fixtures, reduced directional fill, and muted
wood/metal on the already implemented chair. Preserve all gameplay geometry,
colliders, Echo presentation, audio ownership, and anomaly timing.

**Why:** The approved prototype audio and chair discrepancy were being judged in
an open, uniformly grey, externally lit room. A small coherent place is required
before timing or sound can carry meaningful unease.

**Tradeoff:** AV1 improves spatial credibility but is not production art and is
not expected to create terror by itself. Textures, props, fog, post-processing,
flicker, asset packs, and another anomaly remain deferred until the human visual
checkpoint identifies a concrete need.

## 2026-08-11 — Make horror player-facing and sound-led

**Decision:** Treat audio, music, spatial placement, recurrence, variation, and
recovery as the primary horror language. Design moments to affect the real
Player's perception of their surroundings, not merely to give an ordinary prop
narrative significance. The implemented chair remains a secondary environmental
proof or motif anchor; it is no longer the headline scare or the centre of the
next milestone.

Use four recurring families — facility/structure, presence/body, domestic
memory, and time/repetition — with authored variants in direction, distance,
elevation, duration, acoustic character, intensity, and completeness. Several
strong events may occur during the 30–45 minute game, but they are composed set
pieces rather than random triggers. Echo playback and functional sound remain
truthful.

**Why:** The chair reveal and one-shot were detectable but produced no terror or
discomfort. A normal Player reads an unexplained chair as a chair. Directional
domestic sounds, moving presence, repeated motifs, uncertain source checks, and
partial consequences can pressure attention directly while preserving the
central rule that Echoes remember correctly.

**Tradeoff:** This requires more composition, device-specific listening, varied
source material, and human perceptual evidence than a single prop reveal. Exact
motifs, event count, clips, triggers, consequences, binaural/HRTF technology,
voice, and music remain TBD.

## 2026-08-11 — Use puzzles to manufacture vulnerability, not runtime

**Decision:** Design the complete horror escalation before producing more puzzle
rooms. A production puzzle must make the Player wait, listen, turn away, revisit
known space, lose sight of the Echo, or commit to RECALL under perceptual
pressure. Do not reach 30–45 minutes by chaining short plate variants.

**Why:** The current puzzle can be solved in roughly two minutes, but multiplying
that structure would create mechanical padding rather than a horror experience.
Traversal, investigation, audiovisual events, narrative reconstruction, puzzles,
and recovery all contribute to duration.

**Tradeoff:** The exact puzzle count, room map, chapter timing, and future
mechanics remain TBD until each has a distinct horror function. The approved M4,
M5.1, and M5.2 prototypes remain valid mechanical language even if their plate
geometry does not ship.

## 2026-08-11 — Validate directional fear in SD1 at safe levels

**Decision:** Supersede the unimplemented chair-centred `AD3` route with `SD1`,
an approximately five-minute study in the existing room: stable facility
baseline, dry domestic knock behind-right, travelling footsteps, world-locked
breathing, one rare intelligible head-locked whisper, one subtle visual
consequence, and recovery. Add no new puzzle and do not change Echo data or
rules. The Echo's fidelity to every channel it actually records is confirmed as
absolute; it is not allowed to fabricate or alter captured data.

Recommend headphones at a comfortable volume with the message that details do
not require high volume. Plan speaker and mono fallbacks, left/right calibration,
separate ambience/effects/voice control, and `Reduced Dynamics`. Create force
through direction, proximity, attack, spectrum, contrast, movement, and
subtraction rather than unsafe loudness.

**Why:** The next uncertainty is whether authored spatial audio can make the
Player turn, check a real door or corner, remove one earcup, or doubt the source
while the game remains mechanically trustworthy. That must be proven before a
longer route or another puzzle is justified.

**Tradeoff:** World-locked versus head-locked rendering, exact sounds, emitters,
levels, visual consequence, and accessibility implementation remain TBD. The
whisper is defined as `Can you hear me?`, spoken in English with Spanish
subtitle `¿Me escuchás?`; its identity, performer, treatment, ear, and trigger
remain open. The consequence is confirmed as subtle, but its exact visual form
remains open. A guided delivery pass is insufficient; perceptual and emotional
claims require an unannounced human checkpoint and separate headphones,
speakers, mono, and reduced-dynamics evidence.

## 2026-08-12 — Keep itch.io on the source radar; gate Mixamo behind distribution needs

**Decision:** Treat itch.io primarily as a discovery channel, not as one blanket
asset license. Every external asset and service used by the project must have
zero monetary cost. Free tiers, included credits, promotional credits, and
time-limited trials are allowed when they require no charge, do not create an
automatic paid dependency, and grant adequate rights for the intended prototype
use. Do not make purchases, require donations, activate paid tiers, or accept
later license fees. Search itch.io only when a defined production role needs a
prop, texture, font, audio source, or small modular pack. Prefer a specific asset
page that declares `CC0 1.0`, `No AI`, useful source formats, and credible
first-party authorship; retain dated license evidence and hashes before import.
A `free`, `royalty-free`, or platform-level label alone is insufficient for raw
redistribution in this public repository.

Keep Mixamo as a conditional prototyping tool for a future owned humanoid,
auto-rig, or in-place body-animation study. Do not use it for SD1, add a stock
character merely to exploit the library, or commit Mixamo character/animation
source files to the public repository. Revisit only when a concrete humanoid
feature exists and its asset-storage/distribution path is compatible with the
then-current Adobe terms.

**Why:** There is no current goal to publish or commercialize the game, so paid
production dependencies add no justified value to this prototype. itch.io
exposes useful CC0/No-AI filters and has plausible clinical, office, domestic,
audio, and typography candidates, but each publisher controls the actual asset
terms and style quality varies sharply. Mixamo is free and royalty-free for
finished games, but its strongest value begins only when the game needs a
bipedal body; restrictions on redistributing raw character and animation files
remain relevant because this repository itself is public.

**Tradeoff:** Conservative review reduces the speed advantage of asset stores,
and a future Mixamo prototype may need private team storage or a replacement
pipeline before public release. This avoids style drift, unused package weight,
unclear provenance, and accidental publication of restricted source content.

## 2026-08-12 — Freeze SD1 and map one reusable production route

**Decision:** Freeze SD1 as a partial technical sandbox rather than finish its
breathing, whisper, visual consequence, pacing, or room-specific cue polish.
Move to `EM1`: propose the full five-chapter pressure map first, then approve one
continuous 8–10 minute vertical slice built from reusable production-intent
spaces close to the moodboard.

The initial `EM1 v0.1` proposal uses a recognizable observation hub, a
glass-sided test lane, a parallel service return, and one domestic insert across
the complete route. Its candidate slice pairs a non-plate optical Echo relay
with a return through familiar space, directional presence, one persistent
domestic threshold, and recovery. These specifics remain proposed until the
human scope checkpoint.

**Why:** SD1 proved useful authored-audio and mixer contracts, but its disposable
room is too far from the visual target to support reliable production-content
decisions. Refining every cue there would have low transfer value. A small
reused route tests environment production, Echo puzzle design, backtracking,
visual progression, audio, and horror together while avoiding another isolated
prototype.

**Tradeoff:** Directional fear, breathing, whisper, device mix, and the SD1
visual consequence remain emotionally incomplete. The map now carries more
design assumptions, so the spatial spine, first puzzle, first strong event, and
slice budget must be approved before scene production begins.

## 2026-08-14 — Authorize the VS1 spatial blockout before content production

**Decision:** Build a separate, low-cost ProBuilder blockout of VS1 Parallel
Return now, without waiting to lock the full experience map. Reuse the existing
Player and exact-Echo foundation, but implement only the observation hub, glass
test lane, parallel service return, north connector, shallow domestic threshold,
collision, and provisional lighting. Do not implement P1, Strong Event 1,
production dressing, final materials, or new audio in this pass.

**Why:** The disposable SD1 room is too far from the moodboard, but refining a
final scene before testing scale and circulation would also be wasteful. A
separate modular layout makes the route, spatial contrast, backtracking, and
domestic intrusion cheap to evaluate while preserving the original prototype.

**Tradeoff:** The blockout can validate dimensions and navigation but cannot
validate puzzle comprehension, fear, atmosphere, final art cohesion, or the
8–10 minute pacing target. Those claims remain pending a human walkthrough and
later content-specific checkpoints.

## 2026-08-14 — Authorize P1 as a deterministic parallel observation relay

**Decision:** Implement P1 in the approved VS1 blockout with two directional
Echo readers in the glass lane, one Player-only reader on the service wall, and
two staged gates. Echo authorizations A and B latch for the current attempt.
Gate A opens on A; Gate B opens only after A, B, and the Player faces the service
reader away from the glass. Reader A may open Gate A temporarily while the Player
is aligned to demonstrate causality, but this preview never latches A; only the
Echo authorization holds the gate open for the attempt. Use visible reader/relay
indicators, generous facing qualification, no hidden race, and full RECALL reset.
Add no horror event, production dressing, or new audio in this pass.

**Why:** This is the lowest-cost test of whether the accepted parallel layout
creates useful Echo cooperation, divided attention, and deliberate vulnerability
before mood, scare, and production assets obscure mechanical comprehension.

**Tradeoff:** Objective correctness cannot establish discoverability or feel.
Those remain a human checkpoint, and Strong Event 1 stays unapproved until P1
is understood without interpreting failure as altered Echo behavior.

## 2026-08-14 — Build a minimal VS1 visual/audio target before Strong Event 1

**Decision:** Preserve the approved VS1 layout and P1 behavior, then add only the
minimum context needed to judge the slice closer to its moodboard: scene-local
clinical/service/domestic material variants, restrained post-processing, a few
silhouette-readable domestic forms, and truthful functional feedback for P1's
readers and gates. Do not implement Strong Event 1 in the same pass.

**Why:** The blockout already answers circulation and mechanic questions, but a
horror-event judgment made in flat grey geometry with silent puzzle devices
would conflate event quality with missing presentation. This target raises the
context enough for a useful human review while keeping the work cheap and
replaceable.

**Tradeoff:** VT1 can establish coherence, eye guidance, and causal feedback; it
cannot validate fear, final art, production timbre, final mix, or the complete
8–10 minute pacing. Those claims require separate human checkpoints.

## 2026-08-14 — Author Strong Event 1 as an occupied empty lane

**Decision:** After P1 completes and the Echo ends, deliver one world-locked
domestic knock from the threshold. Require the Player to look into the empty
glass lane, then continue north before one independent environmental footstep
source travels along that lane toward the threshold. Briefly subtract the stable
facility beds, restore them in recovery, and leave the threshold lighting more
fully revealed across RECALL. Never attach footsteps to the Echo or imply that
it reproduced an unrecorded audio channel.

**Why:** The sequence uses the accepted puzzle vulnerability and known space,
creates an authored check before the approach, and makes exact Echo absence part
of the evidence. It can be evaluated before Blender production and without new
narrative facts, voice, breathing, an enemy, or a generic scare framework.

**Tradeoff:** Existing CC0 prototypes and primitive visuals can test ordering,
direction, attribution, and recovery but not production timbre or final moodboard
quality. Objective delivery is insufficient; human fear and comfort evidence
determine whether the event survives.

## 2026-08-14 — Model event sources before replacing the rejected knock

**Decision:** Build one small, editable Blender kit for the environmental sources
Strong Event 1 actually depends on: a domestic threshold door, radiator, floor
transition, and perimeter-only observation frame. Anchor the knock emitter to the
door, preserve every existing collider and P1 sightline, and defer broader
dressing and replacement audio until the visual source is human-reviewed.

**Why:** The first human event review proved that correct delivery was not enough:
the prominent cue read like an unexplained door sound because the environment had
no credible door. A selective kit tests attribution and establishes reusable
source geometry at low cost without committing the whole slice to detailed art.

**Tradeoff:** The new geometry can make the intended cause legible, but it cannot
rescue the rejected clip, validate fear, or establish final moodboard quality.
Those still require a visual checkpoint and a later headphone listening session.

## 2026-08-14 — Replace standalone footsteps with lane contraction

**Decision:** Remove the rejected moving-footstep beat from Strong Event 1. After
the Player checks the empty lane and continues north, extinguish three opposed
lane/service fixture pairs from far to near, then extinguish every remaining
clinical/service practical on the final stage. Reduce flat ambient fill to a low
residual blue level and play one short localized frame/relay strain at each lane
source. Keep the camera free, preserve the domestic knock and audio-bed
subtraction, and leave the full blackout as part of the persistent consequence.

**Why:** Human headphone review heard the isolated footsteps as background rather
than a nearby presence. Repeating the event across known physical fixtures gives
the Player three attributable spatial changes and makes the environment contract
without inventing an enemy, corrupting Echo playback, or relying on louder gain.

**Tradeoff:** The construction and persistence are objectively verified, but the
procedural cue is a prototype. A human headphone pass must decide whether the
far-to-near sequence causes a natural turn and discomfort before its timing or
timbre is polished.

## 2026-08-14 — End the first checkpoint with the domestic door opening

**Decision:** Treat the full facility blackout as a failure state, hold a short
period of residual darkness, then open the modeled domestic door slowly inward
with its collider attached to the leaf. Tie warm-light reveal to the aperture and
finish Strong Event 1 only when the door reaches its endpoint. Preserve the open
door and blackout through RECALL.

**Why:** Darkness alone changed exposure but did not create a decisive endpoint.
The slow movement gives the Player one legible source to watch and transforms the
domestic threshold from background dressing into the route forward without
forcing the camera or adding an enemy.

**Tradeoff:** The existing domestic movement loop is pitch-treated as a temporary
creak so timing and attribution can be judged now. Final hinge/friction timbre,
fear, darkness comfort, and the content beyond the door require human approval.

## 2026-08-21 — Make each lighting failure remove a visible layer

**Decision:** Replace the single long ambient fade with three short exposure
steps (`0.55/0.20/0.025`) synchronized to the `z=4/9/14` failures. Extinguish
facility practicals cumulatively (`8/12`, `10/12`, `12/12` dark), then keep both
domestic lights at zero through the dark hold and grow them only with the opening
aperture.

**Why:** The complete blackout and opening-door idea reads, but the intermediate
failures still looked too similar. Removing the already-passed zone at the first
failure and sharply reducing flat fill at every step makes each break perceptible
without louder audio or forced camera motion; the final image is consequently
modeled almost entirely by light coming through the domestic threshold.

**Tradeoff:** `0.025` retains a small navigation floor rather than literal black.
Darkness comfort, step readability in motion, and whether the door spill
dominates from normal play positions remain human visual checkpoints.

## 2026-08-30 — Commit the first checkpoint by closing the door behind the Player

**Decision:** Crossing into the domestic insert updates the runtime RECALL origin
to an interior point. After `1.1 s`, the modeled leaf closes rapidly and produces
one hinge-localized provisional portazo. The checkpoint, closed door, and
single-fire state persist through later RECALLs.

**Why:** The approved warm aperture should become a real commitment rather than
only a visual endpoint. Closing the solved facility behind the Player gives the
first slice a clean boundary and lets the next puzzle start from a stable new
origin without replaying P1.

**Tradeoff:** The procedural impact proves timing, direction, collision, and
reset behavior but not production timbre or fear. Its level is deliberately
bounded; headphones, speakers, reduced dynamics, surprise, and comfort still
require human listening evidence.

## 2026-09-12 — H1.1: approved scope and evaluation gate

**Approved direction (user's strategic-audit approval):** target a complete,
polished 20–25 minute first delivery with one Echo throughout. Preserve the
temporal core and P1/P2/P3. First observe new players on the continuous current
route through R3; only afterward develop a scene where a correct recording is
evidence of what the protagonist denies, followed by Final C. Recorded data
remains exact; do not imply playback of uncaptured channels.

Prioritize existing Unity, Blender and local audio editing. Tripo is deferred
until a separately authorized pilot; Higgsfield is not incorporated. No filler
puzzles, multiple Echoes, general semantic capture, chases or frameworks. This
supersedes the earlier first-delivery duration and multi-Echo direction; older
decision entries and GDD artifacts remain historical.

**Evidence boundary:** direction approval does not approve fear, comprehension,
recovery or pacing. These remain EN PRUEBA where no human evidence exists. H1.1
freezes content for a named local standalone evaluation and separate player/
observer instructions. No H1.2 or new content is authorized at this checkpoint.

## 2026-09-16 — Proceed with creator judgment; defer external playtesting

**Decision:** The user explicitly chooses intuition and their own creative
judgment because there is no time for playtesting. External/unprimed-player
sessions are deferred and are no longer a prerequisite for further work. This
supersedes H1.1's evaluation-first gate and the subsequent request to establish
whether new players participated. Do not repeatedly request that gate.

**Next direction:** proceed to designing the scene where an exact recording
becomes evidence of what the protagonist denies, then Final C. Its concrete
narrative claim and staging still need definition; neither scene is implemented
or complete. Preserve the 20–25 minute scope, one Echo, P1/P2/P3, captured-channel
fidelity and existing pipeline/exclusions.

**Evidence boundary:** creator judgment authorizes creative progress; it is not
an observed independent-player result. Fear, comprehension, recovery and pacing
without specific evidence remain EN PRUEBA, without blocking development. Keep
objective compilation, Console, reference, reset and build checks. Preserve the
H1.1 build/snapshot and optional evaluation guides; no new gameplay change is
implied by this documentation update.

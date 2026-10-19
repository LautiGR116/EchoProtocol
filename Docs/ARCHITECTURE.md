# Architecture

This document records only implemented or currently approved systems.

## Incursion / SE2 static present person — implemented 2026-10-11; emotional effect EN PRUEBA

Human feedback rejected the approach and its footstep performance. In Incursion,
`DomesticPresence` now applies one fixed supported pose: asymmetric feet, slight
weight shift, lowered head and a chest-height hand against the partition. The
elbow and wrist contact were corrected after camera inspection. No approach,
head tracking or figure footsteps remain. The existing solid body leaves the
walking lane clear; the existing practical gives the face a directional shadow.

One short existing contact clip plays after two broadly framed upper-body points
are visibly unobstructed for 0.07 s, including through the sealed transparent
frontage. It requires no face-centre aim, and remains spent through relooks and
RECALL. Ventilation withdrawal and the quieter institutional bed prepare it;
the legacy entry contact and SE2 internal score remain suppressed in this scene.
WorldSFX routing and user audio options are retained. Hidden presentation,
physical traversal and persistence keep their existing owners. No Echo data or
recorded channels are changed.

Focused Editor inspection confirmed silence while hidden, discovery during a
short collision-constrained threshold approach (head at viewport x=0.85), and
one accent after looking away and back. Final compilation succeeded and the
focused Play checks produced no new Console errors. Fear and sound comfort
remain for human play; no build or campaign run.

## Folio — implemented 2026-10-08

`EchoProtocol_Folio.unity` clones Resonance through the Editor and uses
`EP.CampaignCheckpoint.Folio.v1`. One explicitly referenced `FolioUI` owns six
font variants, two original paper textures, cached IMGUI styles, virtual layout,
control focus and keyboard navigation. `GameSession.Folio.cs` renders all session
pages and stacks HUD messages from existing owners. Legacy scenes retain their
renderer when no identity is configured. `FolioDocumentLayout` measures live
Unicode with the selected font; viewport scrolling preserves long documents.
Settings, pause, document close and help retain `GameSession` ownership.

`SD1SubtitleDisplay`, `ContextualHints`, `InteractionFocusHUD` and
`CampaignDirector` suppress their old OnGUI rendering only while Folio is active;
their state/eligibility remains with them. Subtitle text and departure fade have
read-only presentation accessors. Fifty-six campaign TextMeshes (including five activated during progression) use Plex Sans
Medium with persistent atlas materials and existing `DiegeticText` owners.
Transform, support, physics, lighting, audio and puzzle state are preserved.
The native presentation probe and capture diagnostics exist only in QA Players
or the Editor; delivery excludes them. [Identity rules](FOLIO_IDENTITY.md).

## Resonance — implemented 2026-10-08

`EchoProtocol_Resonance.unity` is an Editor-API clone of Lighting with its own
`EP.CampaignCheckpoint.Resonance.v1` save key. `ResonanceSequence` reads explicit
live campaign, loop, SE2, P3, isolation, telephone and final-review references.
It schedules authored consequences of real state transitions and one archive
return region; it writes no puzzle, lighting or recording state. Consumed beats
stay spent through RECALL; loads establish past events as the initial baseline.
The optional `authoredSlam` in `VS1DomesticThresholdCheckpoint` retains the legacy
fallback for older scenes. Existing apparatus owners receive new contact clips.

`ResonanceEmitter` owns a single routed source, smooth level changes and an
optional physical low-pass boundary. Six physical emitters inspect explicitly
referenced existing architectural colliders at 12.5 Hz while playing; obstruction
attenuates gain and bandwidth, with a small rear spectral cue. This is ordinary
3D panning, not HRTF. Two internal score sources use a centered 2D path. The
handset source follows the actual receiver; hanging up fades its one-time line.
The pressure-air loop follows actual motor level and never restarts at zero.
All use the existing mixer and listener-level `SessionAudio` volume, mono and
reduced-dynamics limiter. Pause freezes source playback and authored time.

`ResonanceRouteObserver` exists only in Editor/explicit QA builds. It passively
captures the real listener after `SessionAudio`, using preallocated DSP blocks
and a background disk writer; its state trace records event, position, gain,
obstruction, mode and dropped-block evidence. `ResonanceAcousticProbe` declares
its four pose and one chapter fixtures separately from the full native route.
Delivery excludes both and the input driver. [Evidence and human limits](RESONANCE_CANDIDATE.md).

## Lighting — implemented 2026-10-08

`EchoProtocol_Lighting.unity` is an independent Editor-API clone of Contraprueba,
with `EP.CampaignCheckpoint.Lighting.v1`. Local retained fittings and reflected
light support route/controls after existing outages. Four new P1 ceiling
reflections use `LinkedTaskLight` with the existing circuit sources, for seven
links in this scene; blackout ownership remains unchanged. New retained P3 and
house service references stay outside the switched/transferable light arrays.
The house's warm active reserve, appliance and one-unit representation keep their
existing owner. Exposure, flat ambient, postprocessing and gameplay contracts
are unchanged. The first-act ambient subtraction still applies.

`LightShadowBudget.incrementalShadowChanges` is opt-in for Lighting. Its
single-pass nearest-four selection and enclosed-residence exception preserve
the original rules, but write shadow modes only as ownership changes. Older
scenes keep the legacy path. Stationary, collider-free maintained fitting meshes
are combined by sector/material; no collision is introduced. QA observer uses
an independent sort of the original selection rule after actual camera renders.

`LightingRouteObserver` is Editor/explicit QA-build only. Passive1Hz motion
captures never move or resume gameplay. Paired comparison renders borrow exact
baseline camera poses and restore synchronously; these declared camera fixtures
advance no simulation. `LightingCheckpointSurvey` is Editor-only: actual fresh
Continue loads and explicit public reserve-state actions, render-only normal
height camera fixtures, preference restoration. These do not imply human clarity
or a continuous gameplay route. [Delivery and evidence](LIGHTING_CANDIDATE.md).

## Contraprueba — implemented 2026-10-08

`EchoProtocol_Contraprueba.unity` derives from Reconstruction through Editor APIs
and uses `EP.CampaignCheckpoint.Contraprueba.v1`. P3 is a connected reception,
blind service passage, transfer room and return. `RecombinationRelay` opts into
the third, live A reception: A opens service, B transfers the circuit, and the
Player holds `ReturnWitnessControl` for 1.25 seconds during the recorded return
to A. Release, loss of focus, pause and RECALL cancel partial progress; RECALL
also clears accepted links. Completion retains the exit after playback ends.
Older scene modes remain available. `ProtocolGuidance` reads these actual states.

`PlayerBodyPoseSource` derives presentation channels from actual displacement
and camera pitch during recording: gait phase, local velocity, breathing phase
and head pitch. `EchoFrame` stores their immutable `RecordedBodyPose` alongside
root position/yaw. `EchoRecording` samples them and `EchoPlayback` applies them
to an explicitly referenced, 16-joint skinned adult. `EchoBodyRig` is a pure
function of the sampled pose, with no Animator clock, tracking, audio or random
gestures. These are authored recorded presentation channels, not motion capture,
eye tracking or recordings of Player anatomy. Old takes default to a resting pose.

`ContrapruebaAtmosphere` owns fixed ventilation/contact/shutter sources, lights,
rotor and blind. B subtracts ventilation and clinical light over 2.8 seconds;
the return lamp reveals an already present domestic alcove. The held return
opens the physical blind onto the same take. Pause freezes the transition and
the explicit loop reset restores it. No environmental consequence edits the take.
Full Editor and separate macOS verification routes reach the ending with four
immutable recordings and zero placement fixtures. [Scope and limits](CONTRAPRUEBA_CANDIDATE.md).

## Reconstruction — implemented and objectively verified 2026-10-02

`EchoProtocol_Reconstruction.unity` derives from EncounterArt through Editor
APIs and uses `EP.CampaignCheckpoint.Reconstruction.v1`. Preserved scenes keep
their existing puzzle modes. `PlayerInteractor` resolves one physical focus
snapshot per frame after movement/RECALL. A narrow, occlusion-checked angular
tolerance stabilizes edges. HUD and E use that snapshot; pause and pose reset
require release before another press. `InteractionSurface` binds the moving
telephone receiver to its real owner. The visible body and handset have fitted
colliders. E+R gives priority to RECALL.

P1 opts into live B/C coincidence: A latches, B exists only during actual Echo
alignment, and 0.65 seconds with present Player C commits the exit. P3 opts into
two paired transfers. Echo alignment supplies presence; separate present Player
E terminals confirm A then B. Each acceptance retains its opening until RECALL.
`VS1HubReturnGate` accepts that secured exit after playback ends. No terminal
press, gaze pitch, voice or articulation becomes a recorded channel.

`ContextualHints` reads explicit session/guide/loop references. Only active game
time advances its clock; pause, documents and HUD-suppressed presence do not.
Unique progress states reset delay; attempts can shorten it. Each stage receives
at most two automatic hints, attention then relationship. H pauses; deliberate
J/button requests can reveal the full steps. Automatic hints are optional in
settings. Manual requests suppress already-covered automatic notes.

The new static adult surface derives from the pinned official MakeHuman CC0
mesh and adult morphology. Blender bakes its authored resting pose; Unity gets
one mesh with distinct skin, eyes, hair and clothing, shared with the self-depth
surface and the separate current-world SE2 figure. The Echo remains recorded
root position/body yaw only. The complete input route, negative/retry cases and
fresh save loads pass. The focus-notification contract has an explicit-fixture
check; native OS focus transitions remain unverified. Functional and visual
verification are separate;
no human clarity, credibility, difficulty, listening or fear approval is claimed.

## EncounterArt presentation — implemented 2026-10-02

`EchoProtocol_EncounterArt.unity` derives from Legibility through Editor APIs;
`EP.CampaignCheckpoint.EncounterArt.v1` isolates its saves. A new display prefab
uses the original adult model refined in Blender, eight RecordedWitness material
slots and the identical mesh for self-depth. Root position/body yaw remain the
only recorded channels; no AudioSource, Animator, gaze or articulation is added.
The separate current-world SE2 figure shares the same authored mesh with opaque
materials and fixed front-facing pose. Two supported practicals light the faces.

`EncounterAtmosphere` reads12 explicit references. Two acoustic-only trigger
boxes locate the Player for ambient attenuation; they have no gameplay callbacks.
Fixed ventilation sources and visible rotors coast to zero with actual SE2
presentation or genuine final RECALL. First real SE2 entry after presentation
plays one frame contact from its fixed doorway. Pause freezes these transitions.
Existing `VS1StrongEventOne` and `CampaignRoomTone` retain ownership of their
sources and read the optional gain multiplier; old scenes default to unity gain.
Original colliders, gameplay contracts and orientation references were inspected.
[Delivery evidence and human limits](ENCOUNTER_ART_CANDIDATE.md).

## Legibility presentation — implemented 2026-10-02

`EchoProtocol_Legibility.unity` derives from Candidate through Editor APIs with
the independent `EP.CampaignCheckpoint.Legibility.v1` namespace.
`OrientationFeedback` reads explicit relay/readers/final-review/door references
and updates physical TextMesh surfaces; it writes no gameplay state.
`LinkedTaskLight` follows the original first-act circuit intensity/enabled state,
including the real SE1 blackout. Existing Hub return ownership also controls its
two local task lights and reception direction plate.

`ProtocolGuidance` retains its eight original references and accepts four
optional campaign/house/final/return references. H offers observed rules and
current state without a complete first-act or final route recipe; pause semantics
remain. Only presentation and the basement action wording changed. Recording,
qualification, triggers, collision and source ownership were preserved.
Full route, saved continuation and natural-view evidence have separate scope in
[the Legibility report](LEGIBILITY_CANDIDATE.md).

## Delivery candidate / physical SE2 — implemented2026-10-01

`SE2WalkthroughProduction` and `WalkthroughFinishProduction` author a separate
salon with two accesses, stable collision and light/source ownership. Optional
`VS1ParallelFigureEvent.physicalTraversal` retains genuine P2 admission, measures
continuous entry/return and keeps presentation/observation independent. A single
convex opaque screen or the complete camera frustum exclusion must hide every
presentation and shadow-receiver envelope. RECALL rearms an incomplete visit but
preserves a committed presentation; pose reset invalidates crossing history.
Legacy scenes retain their observation mode. `PlayerFootsteps` consults active,
enabled timber boxes in their own local frame. No Echo channel is added.

`CandidateProduction` derives the delivery scene with its own save namespace.
`ProtocolGuidance` reads eight explicit real first-act references; it writes no
progress. `GameSession` pauses with H, displays the current notes, blocks unrelated
menu shortcuts and returns to the prior pause/play state. Full route, reference
identity, note pause and fresh persistence are checked; exact native scope and
human limits are in the candidate evidence. Scenes are authored via Editor API.

## Conservation porter — implemented 2026-10-01

`PorterProduction.Author()` derives `EchoProtocol_Porter.unity` from Isolation.
It reuses the original monitor, articulated lamp, chair and archive models in a
furnished conservation counter. The existing preserve interaction has a larger
monitor collider at the same desk; final review rules and the isolation save key
are unchanged. `FinalRecordDisplay` only reads `FinalDoorMemory`: unobserved,
witnessed, preserved and key-returned states use three explicit TextMesh surfaces.
Duration comes from the actual witnessed or preserved immutable recording.
The display cannot qualify, alter or conserve a take. Final gameplay and rendered
metadata were checked separately from the earlier continuous Isolation route.

## Physical service isolation — implemented 2026-10-01

`IsolationProduction.Author()` derives `EchoProtocol_Isolation.unity` from Recovery
through Editor APIs. An explicit optional `CampaignDirector.pressureIsolation`
reference replaces that scene's pressure Echo exercise. Recovery retains its
original branch. Physical basement entry starts `PressureIsolation`; an E hold
operates a reachable lever, release returns an incomplete turn, and pause
requires release and a new grip. Looking away alone does not cancel a held lever.
The detent cuts the assigned work circuit, coasts down the local motor and purges
the remote valve. Two actual needle transforms follow pressure, then the existing
bulkhead opens. Floor emergency fixtures use a separate circuit.

`PressureIsolationAudio` owns four distinct world sources: motor, spindle friction,
detent and relief. It observes motion and state, respects pause and restores a
silent baseline for saved completed isolation. It does not modify the listener,
Echo or mixer. The component has provisional authored gains; listening remains
EN PRUEBA.

`EP.CampaignCheckpoint.Isolation.v1` stores chapter and a latched bit. Continue
restores a canonical running or fully drained plant, never a partial lever or
purge animation. The pressure and house chapters suspend recording; the existing
final 6B origin starts the next genuine take. Live transition retains the exact
prior recording. No recorded channels are added. Continuous input and isolated
save/Continue evidence are recorded separately in the dated production status.

## Finish presentation — implemented 2026-09-30

`FinishProduction.Author()` derives `EchoProtocol_Finish.unity` from the preserved
Campaign scene through Editor APIs. Scene-local PBR materials and a dedicated
URP renderer add contact occlusion. Original renderer remains available.
`LightShadowBudget` reserves shadows for four nearby authored lights;
`PracticalEmission` follows actual circuit lights. `StationFeedback` drives real
pressure/work pilots and needles; `PressureAlignmentFeedback` shows alignment
without granting Player pressure. Trigger volumes and immutable Echo samples
retain their gameplay roles. The recorded-surface shader fades the complete visual
at 0.35–0.95 m horizontal camera distance to avoid near-plane intersections; it
does not modify samples, playback transforms or sensor qualification.

`CampaignRoomTone` attenuates institution beds after act one. Finish uses selected
CC0 recorded derivatives for present Player footsteps and environmental contacts.
No Echo audio/action channel is added. Documents now use a paper reading panel;
the new optional Mara letter uses the existing E ray. House progression requires
the telephone and ordered circuits, not collected documents. H contains detailed
mechanical help; headings and prompts leave room for the environment.

Implementation and evidence limits: [FINISH_REVIEW.md](FINISH_REVIEW.md).

## Campaign expansion — implemented 2026-09-30

`Scenes/Production/EchoProtocol_Campaign.unity` is the current expanded scene.
The brief production scene below is retained as a checkpoint. `CampaignDirector`
connects D1 to pressure, house investigation, paired gallery traversal, archive
witness and departure. `CampaignStation` supplies the existing E-ray interface;
its serialized kind identifies one authored interaction. No generic puzzle or
anomaly framework was introduced.

Pressure and gallery qualify the actual live Echo position/body orientation in
explicit scene volumes. Completed valves and paired shutters persist across
RECALL. The house is an announced reading section with recording suspended;
the gallery station starts a new recording origin. House documents use the
session's paused reader. The separate physical presence requires real camera
alignment and unobstructed sight; it never edits a recording or an Echo actor.

Continuation stores only a chapter index in `EP.CampaignCheckpoint.v1` and
restores the beginning of that chapter with completed earlier prerequisites.
It does not restore or invent an old take. The final witness always requires a
new genuine recording. The final accepted `LastRecording` remains unchanged.
The archive identifies it as a current-session take, not proof of the historical
claims in the letters.

`PlayerFootsteps` produces present-tense surface contacts from actual grounded
movement. `FirstPersonController.PoseReset` clears its distance accumulator on
RECALL/placement; the Echo prefab has no footstep component or audio replay.
`SurfaceMapping` serializes texture repeat and reapplies its transient material
property block on load. World-text shaders respect depth and fade at distance.
The static adult Echo mesh and its surface shader add no recorded channels.

`CampaignProduction` authors the scene, materials and explicit references through
Editor APIs. All campaign probes are editor-only and absent from the shipped
scene. Full-route, negative, continuation and staged visual evidence have distinct
files under `VisualEvidence/2026-09-30-Campaign/`.

## Brief-edition checkpoint — session and physical ending (2026-09-30)

The separate production scene `Scenes/Production/EchoProtocol.unity` contains
`GameSession`, `SessionAudio` on the actual listener, and `ProtocolFinale`.
The development VS1 scene remains the earlier reference. The original D1 demo
ending component is disabled only in the new scene. Existing Echo recording and
playback channels remain position and body rotation.

`GameSession` supplies start/pause/restart/quit/credits, optional H help, E target
prompt and actual recording/playback timers. Pausing stops the game clock,
listener and Player input components, saving their enable states for resume.
Focus loss pauses. Active door/SE1/subtitle timers now use scaled game time;
SE1 also explicitly suppresses phase transitions while paused. Settings use
three project-specific PlayerPrefs keys. There is no save-to-disk checkpoint.

`SessionAudio` processes listener samples with linked-channel immediate peak
attenuation and 120 ms release. Normal threshold is 0.65, reduced-impact mode
0.18, with no upward gain; optional mono retains visual puzzle information.
The master volume defaults to 70%. This is objective signal protection, not a
claim of comfortable perceived level on every device.

`ProtocolFinale` accepts the actual Player via the existing E ray only after D1
has been witnessed and its thought has finished. Commit disables recording and
RECALL, retains `LastRecording`, releases D1 ambience ownership, subtracts the
two assigned beds and opens a real solid exit door after 2.5 s. Three authored
position thresholds extinguish passage lights behind the Player. The final
volume ends the session after a thought and fade. Pause cannot advance it.

The scene is authored through Unity Editor APIs, with explicit references and
existing project materials/audio. `OvernightProduction` records construction
and build operations; the input-driven `CompleteEditionProbe` is editor-only
and is not attached to the delivered scene.

## D1 — truthful recorded evidence

`VS1DenialEvidence` is a scene-local component under `VS1 D1 Recorded Evidence`
in VS1. It activates its presentation only after R3 is unlocked and the actual
Player traverses the existing arrival-interior and Hub-clear volumes in order.
The service station uses the existing E ray interaction to call
`LoopController.SetRecallCheckpoint`; it neither teleports nor performs R.
It remains outside the unchanged 29 reset targets, so its origin and witnessed
state persist through later recalls. A new session starts unactivated.

On `LoopCompleted`, D1 inspects only the current immutable `LastRecording`.
A qualifying interval requires a sustained real pose in the glass-side zone,
body orientation within 30°, no sample gap above 1.5 sampling intervals, speed
≤0.15 m/s and drift ≤0.08 m for at least 1 s. Live readiness text is only a
preview; the snapshot independently determines eligibility. During playback,
0.45 s of visible qualified pause from the opposite observer zone delivers the
single thought through a separate `SD1SubtitleDisplay`. Viewport, angle and
occlusion checks exclude the glass, Player and Echo bodies; other solid
occluders reject visibility. D1 never changes frames, playback or camera pose.

Two local steady lights and existing materials make the glass readable after
SE1. During visible approach within 3 m, D1 reduces only the fluorescent and
machinery bed volumes toward 25% of their captured levels, restoring those
levels on interruption, missed/end playback or disable. It does not start audio
sources or alter the mixer. Empty R keeps the existing no-recording behavior;
invalid/missed takes provide retry text. Completion persists without repeated
thought delivery. No Final C exit or accident facts are implemented here.

## VS1 spatial blockout

`Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity` is a separate
production-intent layout study derived from the development prototype. Its active
`VS1 Blockout` hierarchy contains an observation hub, glass test lane, parallel
service return, north connector, and shallow domestic threshold. The kit uses 48
ProBuilder meshes with 48 colliders, eleven project-owned URP materials, explicit
door frames and observation-glass divisions. Twelve panel-aligned downward spot
lights establish a readable cool hub/lane and darker service route; one separate
point light owns the warm domestic endpoint. Cool and service panels use distinct
emissive materials so every visible fixture agrees with its light origin. Only
two spots and the domestic light cast shadows, keeping Play Mode free of the URP
punctual-shadow atlas warning.

A continuous floor infill now closes the two narrow strips that originally ran
beside the observation-glass sill. Dedicated west/east walls close the exposed
outer edges of the north connector. The divider infill passes `200/200` sampled
ground checks, both connector walls block outward waist-height rays, and the
follow-up human perimeter walkthrough approved the containment fix.

A full-height fixed glass partition closes the north end of the Glass Test Lane
after optical reader B. It spans from the west wall to the observation divider,
so the Player can record the complete reader path and see the North Connector
and domestic threshold but cannot use the lane to bypass the two service gates.
The partition blocks `27/27` sampled rays across three heights. A runtime
CharacterController stops at `z=15.57`; the unaffected service route reaches the
connector at `z=18`, crosses to `x=-2.75`, and enters the domestic threshold at
`z=21`.

The scene reuses the existing Player, camera, interaction, `EchoRecorder`,
`LoopActor`, loop foundation, mixer routing, and facility-bed objects. The old
prototype environment, puzzles, discrepancy study, and SD1 study remain present
but inactive so the source scene is not modified and no obsolete room geometry
participates in VS1.

Objective checks pass for scene serialization, all ten standing checkpoints,
continuous floor support, clear capsule space, observation-glass collision, a
grounded runtime Player, active camera/controller/recording components, and a
zero-gameplay-error Play Mode smoke test. P1 is now an implemented mechanical
layer, described below. VT1 adds the limited visual and functional-audio target
described below. Authored horror events, final prop production, final materials,
final lighting, and final audio remain outside this blockout checkpoint and
require separate approval; the later selective event-source kit is documented
below.

## VS1 visual/audio target VT1

`Assets/_Game/Materials/VS1/VT1` contains ten scene-local URP material variants
for clinical floor/wall, service surfaces, dark trim, glass, wood, plaster,
domestic ceiling, textile, and domestic metal. They replace the blockout's flat
assignments without changing its approved collision or puzzle geometry. The
`VS1 VT1 Visual Target` hierarchy adds four low-cost silhouette groups at the
domestic threshold: a console, radiator, curtain, and runner. These are spatial
and material cues, not narratively significant props.

`Assets/_Game/Settings/VS1/VS1_VT1_GlobalVolume.asset` is a scene-local URP
profile with ACES tonemapping, restrained color/white-balance adjustment, bloom,
and vignette. The active camera enables post-processing and SMAA. Existing
panel-owned lighting remains aligned; one warm domestic light and one threshold
spill separate the endpoint without changing the service-route gate logic.

`OpticalReaderAudio` subscribes to each reader's deterministic alignment edge
and plays explicit 3D acquire/release clips. `SlidingDoorAudio` already follows
physical body movement and endpoints; instances on both active service gates use
explicit movement and endpoint clips. All five P1 audio components route through
`WorldSFX` and participate in the active loop reset. The reader clips currently
reuse the accepted pressure-plate edge assets and all assigned timbres remain
provisional and replaceable.

Objective verification covers ten material assets, the four visual groups, the
global profile and camera settings, non-null references on three reader-audio and
two active gate-audio components, the active loop's 19 serialized reset targets,
one acquire/release edge per reader, complete open/close audio cycles on both
gates, and zero gameplay Console errors. The Unity AI package emitted one
account-access warning unrelated to gameplay. Visual coherence, comfortable-
level audibility, and mix feel remain a human Play Mode checkpoint.

## VS1 event-source kit v01

`ArtSource/Blender/VS1/EP_VS1_EventKit_v01.blend` is the editable Blender
`5.2.0 LTS` source generated by
`ArtSource/Blender/VS1/generate_vs1_event_kit.py`. It exports four independent
FBX modules under `Assets/_Game/Art/Models/VS1/EventKit`: a domestic door,
radiator, observation frame, and floor transition. Preview renders live beside
the source under `ArtSource/Blender/VS1/Previews`. Blender's built-in FBX export
is sufficient; the current workflow uses no project-specific add-on or Blender
MCP.

The scene's `VS1 Event Source Kit v01` hierarchy fits the domestic door into the
existing threshold opening, replaces only the primitive radiator's visible form,
adds a transition strip, and repeats five observation-frame bays along the
existing glass divider. The observation module deliberately has no central
mullion: its perimeter posts land on the original divisions and do not cover the
approved Reader A/Gate A line at `(0, 1.6, 3)`. The legacy mullion/header/sill
renderers are hidden while their original colliders remain active. All imported
model files remain collider-free. The scene adds one explicit fitted `BoxCollider`
to the closed domestic-door instance; the other kit modules remain visual-only.

The door uses a scene-local `M_VS1_VT1_DomesticDoor` material variant and the
other parts reuse VT1 materials. The Strong Event 1 knock emitter is anchored to
the modeled door face at `(-2.75, 1.35, 19.78)`. Edit- and Play-Mode audits pass
module presence, door fit, preserved containment, P1/SE1 serialized references,
clear Reader A sightline, enabled runtime state, and zero gameplay Console
errors. A later runtime check also confirms that the Player CharacterController
stops at the domestic door. Final modeling, texturing, and production audio
remain outside this checkpoint.

## VS1 Strong Event 1 — Occupied Lane

`VS1StrongEventOne` is one scene-local authored state machine, not a generic
anomaly director and not an `ILoopResettable`. It observes the approved
`ParallelObservationRelay`, the active `LoopController`, and explicit Player and
world anchors. The event cannot arm before P1 completes. It also waits until the
active Echo has completed or deactivated, so its sounds cannot be interpreted as
an audio channel reproduced by the Echo.

Before the lane sequence starts, the state machine also observes P1 returning to
its incomplete baseline. This is the deterministic signature of a RECALL during
`AwaitingEchoEnd`, `AwaitingLaneCheck`, or `AwaitingContinuation`. It stops the
pending event sources, clears the partial look qualification, and returns to
`AwaitingRelay`; completing P1 again can then deliver a fresh attempt. This
prevents the persistent event from remaining stranded near the north trigger
while the Player and service gates have reset to spawn. Once
`LaneSequenceAdvancing` begins, this rollback is disabled and the authored
blackout/door consequence remains persistent.

After a `1.25 s` grace period, the knock remains armed across a generous
seven-metre north-service zone so normal walking speed cannot skip it. A
world-locked domestic knock plays from the modeled threshold-door face. The
current clip is human-approved at prototype level when attributed to that door.
The state advances only after the Player faces an
explicit empty-lane target for `0.35 s`. Once that qualification is registered
and the knock source has ended, a deterministic three-stage lane contraction
starts immediately. No additional position or continuation trigger participates
in delivery. The
opposed cold-lane and service fixtures at `z=4`, `z=9`, and `z=14` drive a
cumulative far-to-near shutdown at `0.82 s` intervals. The first failure also
removes the hub and entry practicals behind it, leaving `8/12` facility lights
and emissive panels dark; the next stages leave `10/12` and `12/12` dark. A
single mono world-space source moves to each lane fixture and plays the
project-authored frame/relay strain at pitches `1.00/1.07/1.14`. It routes
through `WorldSFX` with linear `3–18 m` attenuation. The existing CC0 SD1 knock
remains the replaceable door cue; the rejected footstep clip is no longer assigned
to SE1.

Both event AudioSources serialize `Play On Awake` as disabled, so neither cue can
leak at scene startup before the event gate. A runtime check after load confirms
phase `AwaitingRelay`, silent sources, lit `z=4/9/14` fixtures, and `0/0` delivery
counts. The obsolete moving-footstep anchors are removed.

During the traversal, the fluorescent and distant-machinery beds fade to a
`0.38` multiplier, then recover over `1.5 s`. In parallel, the scene's flat
ambient-light color, intensity, and reflection intensity follow the three
failures through `0.35`, `0.06`, and a persistent `0.0015` multiplier, with each
transition taking `0.18 s`. This makes the final facility nearly black and
prevents cold uniform fill from competing with the opening domestic door.
This creates impact through subtraction while keeping event-source gain at
`1.0`. Both domestic lights begin at zero. The point source is placed just
inside the threshold, while a narrow spot is aimed diagonally back along the
authored Player route so the aperture, nearby interior, and floor path read as
one warm invitation. After a `0.75 s` dark hold,
`VS1DomesticDoorReveal` rotates a scene-local leaf pivot inward by `76°` over
`5.2 s`. The ten leaf/handle meshes and fitted `BoxCollider` share that pivot;
the casing, jambs, hinges, and threshold remain fixed. Warm and spill intensities
follow door-open progress. A world-space loop reuses the accepted domestic-door
movement source at pitch `0.72 → 0.58` as a provisional friction/creak treatment
and stops at the endpoint.

`VS1DomesticThresholdCheckpoint` owns a trigger just inside the doorway, an
interior recall transform at `(-2.75, 0.03, 22.05)`, and a hinge-localized
`WorldSFX` source. The first Player crossing calls
`LoopController.SetRecallCheckpoint`, waits `1.1 s`, then asks
`VS1DomesticDoorReveal` to close the leaf over `0.18 s`. The impact is a
deterministic runtime mono clip (`48 kHz`, `0.38 s`, peak limited to `0.58`) at a
source gain of `0.58`; it is a prototype transient, not approved production
audio. The fitted leaf collider seals the old route at the endpoint. The
checkpoint, closed leaf, and single-fire count persist through later RECALLs.

`LoopController` now keeps a runtime recall position and rotation initialized
from the original serialized spawn. A committed checkpoint changes only those
runtime values, removes any obsolete active Echo, invalidates the last completed
recording, and cancels/rearms the current recorder at the Player's crossing
pose. This establishes a new recording boundary as well as a new Player spawn;
the first post-checkpoint RECALL cannot replay pre-checkpoint frames.
`CompleteLoop` still resets the same explicit targets, preserves the exact
finished post-boundary recording, spawns its Echo, and arms the next recording
from the new pose. A fresh Play session starts from the original spawn.

Objective tests pass relay and post-Echo gating, required Player check, automatic
delivery without Player displacement after that check, one knock,
three far-to-near localized relay deliveries, cumulative `8/10/12` fixture
extinction, audio-bed duck to `0.38` and recovery, persistent `0.0015`
ambient-light/intensity/reflection state through
RECALL, one slow door-open request, mid-motion sound/collision, a clear final
aperture, threshold commit, one delayed slam, closed-leaf collision, two
consecutive RECALLs at the interior point, non-repetition of the impact, non-null
saved references, restored 60-second recorder and Player/edit baselines, and zero
gameplay Console errors. Door attribution and the near-black warm composition are
human-approved at prototype level; the creak and portazo treatments are not.
Spontaneous checking, surprise, provisional timbre, mix comfort, and device
compatibility still require human evidence.

## VS1-R3 Hub Return

`VS1HubReturnGate` owns one authored final-crossing trigger at `(-5.05,0,43.7)`.
It accepts only the explicitly assigned Player `CharacterController` and
`LoopActor`, using actual trigger observations on both sides of the plane.
Both observations must have genuine `RecombinationRelay` A authorization, live
B Echo alignment, an active Echo and a fully open P3 final gate. Echo colliders
cannot contribute samples. RECALL invalidates crossing history. No existing
puzzle, recording, playback, checkpoint or event code changes.

The first valid crossing opens the independent R3 exit at `(-17.2,0,46.4)`.
`IsOpen` and the curtain/light consequence persist through R; there is no
`ILoopResettable` registration or saved-game state. Fresh Play initializes closed.
R1 remains the checkpoint. P3's original readers/doors/relay remain in the same
29 reset targets and retain their ordinary A latch/B live-hold rules.

`VS1 R3 Hub Return` contains the continuous western corridor at x=-18.6,
z=-9.5…46.4, a transverse arrival at z=-9.5, ceiling, perimeter and fitted
threshold pieces. Only `P3 long return west end` and `Hub West Wall` are retained
inactive and replaced with apertures. No other original serialized gameplay
record changes. This reaches the actual `01 Observation Hub` at z=-8, from a
previously unavailable west arrival; it never joins the earlier route midway.

Two independent `SlidingDoor`s form the automatic exit-only arrival at x=-9.2
and -5.1. Receiving opens the rear only with the front fully closed. The actual
Player entering well clear of the rear requests closure; only its fully closed
state permits the front to open. Full Hub-side clearance closes the front before
reopening the rear. A complete vestibule-occupancy volume catches immediate
reverse attempts and keeps the rear sealed; retreat before rear closure returns
to Receiving. R seals the front before resetting the arrival cycle. These doors
add no interaction, puzzle state, sound or separate checkpoint.

One silent, static, colliderless curtain sits roughly 4 m behind solid observation
glass at x=-5.1,z=-5.55. All materials are reused. R3 local shadowless lighting
preserves clinical orientation without writing any SE1 blackout light/panel or
global lighting state. There is no new figure, domestic frame/runner, voice,
blackout, camera control or strong event.

Evidence: `VisualEvidence/2026-09-12-R3/`. Input-driven route tests cover prepared
domestic checkpoint → P2 → SE2 → SE3 → R1/E → P3 → original Hub, failed
solutions before unlock, Player/Echo discrimination, arrival reversals and five
later R presses. Geometry passes 18,328 support samples, solid boundary/door/
ceiling cross-sections and six capsule-grid reachability cases. All 28 original
gameplay scripts are byte-identical to task start, all original scene records
remain, and the 29 reset references are unchanged. Full P1/SE1 playback is not
repeated; their configuration is verified. R2/P3 are creator-approved greybox;
R3's recognition, timing, tension, interpretation and emotional effect remain
**EN PRUEBA**. No production-content or emotional approval is inferred.

## VS1-R2 Anchor Transfer and P3 Recombination

`VS1RecallAnchor` is an `IInteractable` on the existing R1 central station. It
references the real Player actor, `VS1RecoveryHubGate`, existing loop, recall
transform, display/label and gallery access door. It accepts E only after the
hub opens, once per Play session. The existing camera ray enforces physical
occlusion and range. `SetRecallCheckpoint` is reused unchanged: old Echo and
recording are discarded, the recorder is immediately rearmed at the actual
interaction pose, and the Player is not moved. R returns to
`(-5.7, 0.03, 27.8)`, yaw `315°`, with the station and exit nearby. The first
Echo frame is the actual activation pose in R1, not a fabricated snap to that
transform. The anchor and its north access door are intentionally absent from
loop reset targets. Neither overwrites the domestic door or SE1/SE2/SE3 state.

`RecombinationRelay` owns two existing `OpticalReader`s and two `SlidingDoor`s.
Both readers face +X, accept Player/Echo and qualify within the existing 35° /
0.35 s rule. Player alignment temporarily previews each gate. Only Echo A
latches the alternative-route authorization; it survives leaving A while the
Echo exists. B is never latched: final authorization requires A plus the live
Echo-facing condition at B. When the Echo ends, the latch clears and both gates
close unless a real Player is currently demonstrating a temporary preview.
RECALL resets the readers, both gates and relay via five appended explicit
reset targets (29 total). No previous reset target is removed or reordered.

The R1 north wall is retained inactive and replaced with solid glass/pier/access
pieces; its north skirting is shortened at the aperture. All other authoritative
existing boundaries remain. A vestibule at z=30.2–34 separates quiet recovery
from P3. The recording lane is centred at x=-7.8 with A at z=35.2 and B at z=43;
the alternate route is centred at x=-5.05. A full-height divider has glass at the
readers and 5.3 m of opaque wall between them. A solid terminal wall prevents the
recording lane bypassing the final gate. The payoff runs x=-17.2…-3.8,
z=44.6…48.2, with an enclosed inaccessible lightwell and sealed sightline to the
actual R1 station. Existing materials and eleven shadowless fixture-local lights
are reused; a single textile patch is the only new domestic residue. No audio or
new strong event participates.

Four physical TextMesh labels use `DiegeticText` and a small URP
`DiegeticText.shader` with depth testing. This prevents the default overlay font
from leaking through walls; its runtime material follows the font atlas and is
destroyed with the label. These labels have no global UI or gameplay ownership.

Objective evidence in `VisualEvidence/2026-09-12-R2/`: 66 route assertions,
12 real Input System R presses, E transfer without teleport/automatic R, positive
solve, no Echo, Echo B without A, wrong B yaw, short recording cleanup and R during
separation. The harness performs one prepared domestic checkpoint/SE1-lighting
setup, then uses actual movement/look/E/R, CharacterController, recorder and
playback. It does not force SE2/SE3, author puzzle state or write Player poses
after setup. Active Echo Rigidbody poses and snapshot identity are checked each
LateUpdate against immutable samples within 0.01 m / 0.1°. P1/SE1's full opening
sequence is not replayed; their saved configuration is retained.

Geometry passes 6,402 floor samples, 4,470 boundary rays, both gate cross-sections
and four capsule-grid reachability cases: anchor closed; only recording lane
available; A open/B closed; both gates open. These complement the physical
Player route and do not replace human perimeter testing. All 2,568 original scene
records are retained; seven original records change (R1 aperture/station wiring
and loop reset append). Existing gameplay MonoBehaviours change only in the
loop's appended serialized reset list. `LoopController`, recorder, playback,
P1/P2 and event scripts remain unchanged from task start.

Captures are unedited 1280×720 Player-camera renders at FOV 60; station baseline
and activation share a camera pose. The first visual pass exposed mirrored text,
overlay-font leakage and an unenclosed exterior view; these were corrected before
the final suite. Two Editor label commands partially failed on missing default
font material; explicit font assignment repaired that authoring issue. Successful
final gameplay runs have zero Console errors. Harness source and Editor authoring/
audit commands are archived under `Source/`; temporary runtime probe is removed.
R2/P3 are now creator-approved at greybox level by the R3 brief; unprimed-player
clarity, tension, emotional interpretation and recovery remain EN PRUEBA.

## VS1-R1 Recovery Hub

`VS1RecoveryHubGate` lives on `R1 Player return access trigger` under
`VS1 Systems/VS1 R1 Recovery Hub`. Eight serialized references connect real SE3,
the actual Player actor, baseline, hub, access and residual roots, the original
west-wall collider and the solid observation glass. SE3 exposes read-only
`IsComplete` from its existing `Revealed` phase; its rules remain unchanged.

Trigger entry/stay accepts only the assigned active Player, with active SE3,
completed return and visible overlap. The gate enables hub floor, containment
and presentation before disabling the original wall. It opens once, remains
open through RECALL and component disable/re-enable, and has no loop-reset or
checkpoint participation. A new Play session restores the original baseline.
`LoopController`, recorder, playback and puzzle scripts are unchanged by R1.

The original `P2 North Leg West Wall` is preserved under `R1 Baseline - original
west containment`. The revealed replacement has matching wall extents with a
2 m aperture centred at `z=29.0`. The hub spans approximately `x=-10.45…-4.05`,
`z=25…30.1`: one two-way entrance, continuous floor, solid perimeter, central
observation station, cabinets, four stable panel-owned cool spotlights with local diffuse fill, and a
sealed west glass wall. A static domestic frame/runner and low warm light sit
3 m beyond the glass. This residual root has no colliders, actor or audio.
All materials are existing assets; no package, imported mesh or new audio is used.

The hub never reconnects to the pre-P2 side. P2's gate still requires its normal
reader hold. The original R1 checkpoint retained domestic RECALL; R2 now transfers
it only after explicit station E, as documented above. Local R1 lights are not
in SE1's blackout arrays and do not modify global lighting or audio. Its visual/spatial direction and legibility are now creator-approved; emotional
recovery remains **EN PRUEBA**. Validation and
limits are recorded in `VisualEvidence/2026-09-09-R1/README.md`. Final tests pass
complete/incomplete returns, both RECALL policies, SE2-complete pre-return RECALL,
no-Echo containment, two-way traversal, all eight R1 references, 4,224 floor
samples and 258 perimeter rays. Compilation and gameplay Console checks pass.

## VS1 Strong Event 3 — Omitted Passage

`VS1OmittedPassageEvent` is an optional presentation component on `SE3 Return
Crossing (+Z toward return)`, under `VS1 SE3 Omitted Passage Study (EN PRUEBA)`.
Its five explicit references are the real SE2 component, loop controller,
Player `LoopActor`, clinical baseline and domestic overlay. States are
`AwaitingSE2`, `AwaitingReturn`, and `Revealed`. The component reads genuine
SE2 `Complete`; it never writes SE2, Echo data, puzzle authorization or Player
pose. A directional crossing of the trigger's local Z plane at `(-1.55,1.1,29.1)`
checks the assigned Player's movement and the box cross-section. Echo movement
cannot supply that position. This is just west of the earliest accepted SE2
observation position, so stopping near the turn instead of at the glass still
allows the return reveal. The existing turn frames the north-leg superposition.

`SE3 Clinical Baseline` contains a matching clinical surface veneer; the original
floor, walls, ceiling and P2 gate remain active and authoritative in both states.
`SE3 Domestic Overlap` spans roughly `z=25.3–28.4`, with four progressively
smaller doorframes, a worn runner, skirting, partial curtain, console/unlit lamp,
and two local shadowless warm lights. A restrained cool ceiling reflection and
service profiles preserve the clinical contrast. All materials are reused. It contains no
solid colliders, audio or second figure. The shared corridor remains navigable.

RECALL discards the previous movement sample, so the checkpoint jump cannot
count as a return crossing. It preserves the completed observation and any
already-revealed passage. A completed SE2 recalled **before** walking back stays
armed and waits for a later genuine return; an incomplete SE2 stays clinical.
Unchecking `studyEnabled` or disabling the component restores baseline; enabling
it again restores a revealed consequence without incrementing `RevealCount`.
Stopping Play starts a fresh clinical session. Input-driven Play tests verify
incomplete/complete returns, armed and revealed RECALL, wrong-direction rejection,
near-corner SE2 completion, optional-component restoration, and passage traversal
through the real P2 gate in both directions. Shared floor support passes `420/420`
samples; both side walls pass `36/36` rays. Active Echo Rigidbody poses remain
within `0.01 m / 0.1°` of immutable recorded samples in the instrumented runs.
See `VisualEvidence/2026-09-09-SE3/README.md` for the limits and failed harness
attempts. The creator subsequently approved direction and slice legibility; this is not
final content or evidence of fear.

## VS1 Strong Event 2 — Impossible Domestic Chamber

`VS1ParallelFigureEvent` retains its component type and script identity but now
controls two explicit environmental roots instead of withdrawing a figure. The
old `VS1 SE2 Parallel Figure Study` hierarchy is preserved disabled. The new
`VS1 SE2 Impossible Domestic Chamber` root is at `(8, 0, 29.1)` with `90°` yaw.
The service return extends eastward from the P2 corner at `(-2.75, 0, 29.1)` to
solid endpoint glass at `x=8`, placing the chamber in the normal line of advance.
The glass and route containment remain outside the switched room states.

The baseline is an empty cold service chamber approximately `3.3 m` deep. Its
replacement is an approximately `8 m` domestic interior with warm illumination,
floorboards, a wine runner, armchair, floor lamp, console, repeated doorframes,
and a stationary human silhouette approximately `2 m` tall. The entire cold
root switches off as the domestic root and figure switch on. The event adds no
audio and never writes to the Echo, Player camera, controls, or puzzle condition.

Explicit references connect the loop, `BlindAlignmentRelay`, real Player Camera
transform, cold/domestic roots, figure, look target, and solid visibility glass.
`OnTriggerEnter` and `OnTriggerStay` accept only an active Player actor while
`BlindAlignmentRelay.IsEchoHoldingGate` is true. Continued occupancy avoids
missing an Echo hold that qualifies just after trigger entry. Once accepted,
`AwaitingLook` preserves the cold view until the target remains within `40°`,
inside the Camera viewport, and unobstructed through the designated glass for
`0.65 s`. A nonallocating ray query rejects other intervening world colliders;
figure colliders are permitted. No later Echo state is changed or required by
the environmental presentation.

The reveal begins `Reading`. `QualifiedReadSeconds` accumulates only while the
same target is visibly framed; looking away pauses rather than clears that
reading. At `3.2 s`, `Complete` leaves both the warm architecture and figure
stable. `LoopController.LoopCompleted` restores cold state, clears timers, and
rearms an incomplete event, while a completed event persists through RECALL
until a new Play session. Disabling the component restores incomplete baseline
state but preserves completed presentation. Public phase, visibility, crossing,
appearance, completion, and qualified-reading values support objective checks.

The replacement passes compilation, explicit-reference inspection, continuous
floor support, solid-glass collision, live P2 gating, and normal input-driven
travel from a prepared domestic checkpoint. A recorded 18-second reader hold,
real `R`, gate crossing, northward walk, and ordinary eastward turn reveal the
room at approximately `x=1.4`, with the Player facing `90°`. Incomplete RECALL
at approximately one second of reading restores cold state; a second recorded
attempt completes at `3.2 s` and remains warm after another real RECALL.
Both the blocked no-Echo route and an isolated downstream crossing reject the
event. Looking away pauses reading. The live Echo retains the same recording
snapshot; a post-reveal pose sample matched its recorded position and yaw.

[Evidence and test procedure](VisualEvidence/2026-09-08/README.md) distinguish
the single checkpoint/lighting setup from subsequent collision-constrained
movement. The complete P1/SE1 route was not repeated in this pass. No gameplay
errors were reported in successful runs; Unity AI's Account API timeout is an
Editor warning. The subsequent creator pass approved direction and slice legibility. It did not
report a separate fear, narrative or audio result.

## P2 Blind Alignment

Incursion opts into `BlindAlignmentRelay.requirePoweredClosure` (2026-10-11).
Other scenes retain the original single-gate rule. The physical post is beside
the east-wall reader, clear of the Player's route. `OpticalReader` exposes real
Player/Echo occupancy independently of its existing recorded yaw alignment
(`+X`, `35°`, now `0.18 s` qualification in Incursion).

Presence supplies power. Looking at the reader opens the north gate; averting
the recorded gaze while remaining on the post closes it without losing power.
The eastward onward gate needs power and the first gate fully closed, with
no occupant facing the reader. It also responds during manual rehearsal from
the post, before recording. Raw facing presence inhibits its release during
the existing 0.18 s qualification interval. Leaving the post or ending playback
removes power and prevents this authorization. Occupied doorway volumes hold moving leaves clear of the
Player; they never grant progress. A physical powered crossing of the open
onward gate earns `HasTransferred`, which admits SE2 via
`CanAdmitEchoCrossing`. This earned crossing survives the later end of the take;
waiting for that end before crossing cannot earn it. RECALL clears it and
restores both gates. The new leaf leaves the existing SE3/R1 return opening free.

`P2AirlockPresentation` repeats the actual travel of both leaves on the post
panel and links each numbered door to its own indicators. Brief states distinguish
gaze-held opening, closure in progress, an occupied threshold, no supply, and
exit release. The panel states that closure of 1 releases 2; the existing
contactor confirms release from either side. Post depression and existing door
sounds retain their real mechanism timing. These
are present-world mechanisms, routed through WorldSFX/Ambience, not extra Echo
channels. Closing behind the Player occludes the Echo, filters its station
motor to 850 Hz, and reduces the institutional bed through the existing
`EncounterAtmosphere` ownership path. Emergency light preserves the failure
instruction. The onward gate, its motor and presentation join the explicit
loop reset list. No playback samples, timings or camera controls are altered.

Editor checks: an actual 12.5 s recorded look/avert plan allowed physical entry,
powered closure and SE2 admission with about 3 s remaining. A separate take
left the post before closure: power disappeared while its Echo remained active;
the onward gate stayed closed through playback completion. The R binding
returned the Player to the domestic origin and rearmed recording. Camera review
covered the station, interior state and failure instruction. Emotional effect
and unprimed comprehension of this redesign remain **EN PRUEBA**.

Feedback correction, 2026-10-11: the focused manual rehearsal confirmed
`1 ABIERTA POR MIRADA / 2 BLOQUEADA POR 1`, then `1 CERRANDO`, then
`1 CERRADA / 2 PASO LIBRE` while still on the post outside the airlock. Leaving
the post showed both doors without supply. Camera inspection cleared labels
from the reader housing. The previously verified recorded solution and RECALL
route were not repeated; admission still requires an actual powered Echo crossing.

## P1 Parallel Observation Relay

`OpticalReader` detects Player and/or Echo actors inside an explicit trigger and
requires their horizontal body-forward vector to remain within a configured
facing tolerance for a short qualification period. P1 uses two glass-lane
readers accepting the Echo and one service reader accepting only the Player.
Active cyan lenses and four relay-state indicators expose qualification and
latched state without a hidden timer.

`ParallelObservationRelay` latches Echo reader A and B independently for the
current attempt. Player alignment at reader A requests Gate A only while that
alignment remains valid, providing an immediate through-glass causal preview
without granting Echo authorization. Echo authorization A latches and holds Gate
A open for the attempt. Gate B requires both Echo authorizations plus the Player
aligned with the east-wall service reader, which makes the Player face away from
the glass. `SlidingDoor` moves each referenced kinematic gate between explicit
local baselines. The three readers, relay, and two gates are six explicit
`ILoopResettable` targets on the existing `LoopController`.

Reader A's trigger, housing, and lenses share Gate A's `z=3` plane. From the
recording position, both the sensor and the closed gate project at viewport
`x=0.50`; the gate remains on that horizontal sightline while it rises from its
closed to open baseline. Player alignment fully opens the gate in this frame,
looking away closes it without latching state, and an Echo activation opens and
holds it. A clean RECALL restores the preview, relay, gate request, and Player
spawn. The developer approved the resulting causal response in Play Mode.

Objective staged tests passed wrong-facing rejection, both Echo latches, Gate A
staging, rejection of an incorrectly facing Player, and final Gate B completion.
Five controlled RECALL cycles then restored all reader/relay/gate state and the
Player spawn. The developer approved the current implementation after a guided
walkthrough. Reader discoverability and comprehension remain unverified for an
unprimed Player.

## First-person player

The `Player` GameObject composes:

- `CharacterController`: collision-constrained, grounded capsule movement without
  Rigidbody force simulation.
- `PlayerInput`: enables the existing `Player` action map from
  `Assets/InputSystem_Actions.inputactions`.
- `FirstPersonController`: reads `Move` and `Look`, applies gravity and movement,
  rotates the player body horizontally, pitches the child camera vertically, and
  manages cursor capture.
- `LoopActor`: identifies this physical actor as the Player for temporal puzzle
  triggers.
- Child `Main Camera`: the player's view and AudioListener.

The controller exposes speed and sensitivity in the Inspector so movement feel
can change without editing code. Mouse delta is applied without multiplying by
frame time because it already represents movement accumulated for that frame.

## Interaction

- `PlayerInteractor` is a separate component on `Player`. It reads
  `Player/Interact`, casts a three-metre ray from the centre of the child camera,
  and invokes the first `IInteractable` found in the hit collider's ancestry.
- The ray uses the first physical hit across normal world layers, so walls and
  other colliders block interaction instead of being skipped.
- `IInteractable` receives the interacting GameObject, leaving room to
  distinguish Player and Echo actors later without introducing a framework now.
- `PrototypeToggleSwitch` is a greybox validation target. It flips a lever and
  toggles an indicator; it is not the future door or puzzle-state architecture.

`PlayerInteractor` does not activate or deactivate `PlayerInput`; the existing
player input lifecycle remains owned by the player setup.

## Temporal recording

- `EchoRecorder` lives on `Player`. It captures the player's world position and
  body rotation in `LateUpdate` at a `0.05` second interval, after locomotion has
  run. A fresh attempt is armed but does not accumulate arbitrary idle time at
  spawn. Horizontal movement beyond `0.02` metres starts a fixed one-second
  lead-in and the 60-second bounded timeline. Pauses after that point remain
  part of the recording, including pressure-plate dwell.
- `EchoFrame` stores one timestamped pose. `EchoRecording` copies completed
  frames into an immutable snapshot and samples by timestamp with position and
  quaternion interpolation.
- Starting the next recording clears only the recorder's mutable buffer; it
  cannot modify the snapshot already used by playback.

## Loop reset

`LoopController` reads `Player/ResetLoop` from the `PlayerInput` action copy. A
press of `R`, or reaching the recording limit, performs this ordered lifecycle:

1. Finish and snapshot the current attempt.
2. Deactivate and schedule destruction of the previous Echo.
3. Restore each explicitly referenced `ILoopResettable` to its baseline.
4. Reset player position, yaw, camera pitch, and vertical velocity at
   `Player Spawn`.
5. Instantiate and initialize the new Echo from the completed snapshot.
6. Arm a fresh recording at the spawn; its timeline starts on first movement.

After all six steps succeed, `LoopController` emits `LoopCompleted`. Failed
RECALL attempts without a started recording emit nothing.

The current scene explicitly resets two pressure plates, their sequential
condition, the sliding door, its audio state, and the exit goal. The earlier
`PrototypeToggleSwitch` remains available as an isolated
interaction proof but its scene group is inactive and it is not part of the
current puzzle baseline. The loop keeps only the latest recording and one Echo.
It does not reload the scene or take ownership of the shared `PlayerInput`
lifecycle.

## Echo playback

`PF_EchoGreybox` is a translucent primitive capsule with a forward marker. It
uses a kinematic Rigidbody, trigger CapsuleCollider, and Echo-kind `LoopActor`,
so it is nonblocking but can occupy temporal triggers. `EchoPlayback` snaps to
frame zero, advances the immutable recording in `FixedUpdate`, applies the exact
final pose, and disappears predictably when playback ends.

The current recording reproduces player movement and body yaw only. Semantic
interactions and a separate visible head/camera pitch track are intentionally
deferred until a concrete puzzle requires them.

## First temporal puzzle

- `PressurePlate` accepts colliders belonging to Player- or Echo-kind
  `LoopActor` components. It tracks colliders separately while counting unique
  actors, so one actor with multiple colliders cannot release the plate early.
- Actor-disable notifications, trigger-stay repair, and a defensive physics-step
  purge prevent a disappearing Echo or missed trigger exit from leaving stale
  occupancy.
- `SlidingDoor` owns the kinematic moving panel and its physical collider. The
  plate references this one door directly and requests it open only while at
  least one valid actor remains.
- `PrototypePuzzleGoal` is a latching trigger behind the door. It accepts only
  the Player, ignores Echoes, returns to incomplete on loop reset, and emits one
  rising-edge `Completed` event when the Player first enters.

This is intentionally a concrete one-plate/one-door contract, not a generic
signal graph. More expressive puzzle wiring is deferred until another room
requires it.

## Dual-plate timing condition

M5.1 makes a plate's direct door reference optional and adds one concrete
`DualPlateDoorCondition`. Its two plates remain normal physical sensors; the
condition reads their public pressed states and requests its door open only for
`plateA && plateB`. It also drives two independent status lamps and implements
`ILoopResettable`.

The approved M4 variant retains its original direct one-plate/one-door reference.
The M5.1 plates deliberately leave that reference empty, making the AND
condition the sole logical owner of their door. This is not a general boolean
graph, UnityEvent network, or timer framework.

## Sequential plate condition

M5.2 adds one `SequentialPlateDoorCondition` with three explicit phases:
`AwaitingFirst`, `AwaitingSecond`, and `HoldingOpen`. A rising press on plate A
arms the second phase and its lamp. Only a later rising press on B completes the
sequence; B pressed before A, held while A activates, or pressed in the same
physics step is ignored until it is released and pressed again.

There is no sequence timeout. Once valid, the condition requests its door open
only while B remains occupied. Releasing B, disabling its actor, or resetting the
loop returns to `AwaitingFirst`, clears both indicators, and closes the door. The
two plates have no direct door reference, leaving the condition as the only
logical owner. A low scene blocker lengthens the B-to-door route so the Player
cannot replace Echo cooperation with a closing-door race.

## First environmental discrepancy

M6.1 adds one `EnvironmentalDiscrepancyController` with explicit references to
the active loop, Player-only goal, and one initially inactive chair root. The
goal's rising event arms the controller; the next successful `LoopCompleted`
event reveals the chair synchronously after reset and Echo initialization.

The controller is active while its visual child is hidden. It does not implement
`ILoopResettable` and is not listed among puzzle reset targets, so the revealed
state persists through later RECALLs until the scene or Play Mode session reloads.
The chair is built from render-only primitives with no collider, Rigidbody,
`LoopActor`, interaction, light, or puzzle reference. It cannot change
pathing, timing, occupancy, goal eligibility, or recorded Echo frames.

`ClinicalRoomTone` shares the always-active controller GameObject and owns one 2D
`AudioSource`. AD2 replaces its runtime-generated placeholder with the serialized
stereo `AMB_FacilityAir_LP` asset and loops it at volume `0.14`. It listens only
to the discrepancy's rising `Revealed` event,
fades linearly to zero over `0.8` seconds, stops the source, and remains silent
through later RECALLs. A Play Mode or scene restart restores the initial bed. It
never destroys the imported asset and does not touch listener, mixer, global
volume, time scale, puzzle reset list, or Echo lifecycle.

## Normal audio routing

M6.2 adds `Assets/_Game/Audio/MX_Main.mixer` with one default snapshot and three
groups: `Master`, `Ambience`, and `WorldSFX`. The start snapshot now carries a
provisional `+8 dB` Master gain after a human review found the complete
presentation too quiet; group balances remain unchanged, with no effects beyond
Unity's attenuation stage and no exposed parameters. All 11 active scene
AudioSources route through this mixer, so the boost preserves their relative
balance. Summed-output headroom and device compatibility remain unapproved. The
M6.1 room tone routes to `Ambience`; its component still owns the same
source-level volume and fade without mutating mixer or listener state.

The active M5.2 door root owns one 3D `AudioSource` routed to `WorldSFX` and one
`SlidingDoorAudio`. It references the fixed door root, its kinematic moving body,
the user-selected domestic `SFX_DoorMovement_LP`, and the matching
`SFX_DoorEndpoint` asset explicitly. It no longer creates or destroys runtime
door clips.

`SlidingDoorAudio` runs after the door controller and compares consecutive
Rigidbody positions. It starts the motor only after physical displacement,
keeps the same voice through a direction reversal, and plays one endpoint cue
only after real movement reaches fully open or closed. Repeated `SetOpen` calls
cannot restart it. As an explicit `ILoopResettable` target placed after the door,
it stops immediately after a reset snap and suppresses a false endpoint cue.
It owns no randomization, gameplay decision, global manager, or Echo data.

AD2 adds three truthful functional sources without a global audio manager:

- each active M5.2 plate owns a `PressurePlateAudio` and one 3D `WorldSFX`
  source. `PressurePlate.PressedChanged` emits only on a real occupancy edge;
  forced visual reset emits nothing. Each audio component follows its plate in
  reset order and stops silently;
- `RecallCompletionAudio` owns one 2D `WorldSFX` source and subscribes only to
  successful `LoopController.LoopCompleted`; invalid RECALL remains silent;
- `Facility Ambience AD2` owns a 2D fluorescent loop and a fixed 3D distant
  machinery loop routed to `Ambience`. Both persist across RECALL while the
  subtractable facility-air layer keeps its existing reveal fade.

The three ambience masters use `Compressed In Memory` / Vorbis quality `70`.
Door, plate, and RECALL clips use `Decompress On Load` / PCM. All are preloaded,
preserve `48 kHz`, and have background loading, ambisonics, and forced mono
disabled. Three-dimensional masters are already mono.

## First localized audio discrepancy

M6.3 adds one `ChairSettlingCue` on an `AudioSource` child of the initially
hidden M6.1 chair. Because the chair root owns the source, the component cannot
run before the existing reveal. Once active, it compares only the Player's
planar position against a `1.3 m` radius and plays one deterministic `0.9`-second
mono cue on the first Player proximity.

The source is fixed `0.65 m` above the chair, routes to `WorldSFX`, and uses full
3D blend with linear attenuation from `1.25` to `8 m`. The component generates
and owns one runtime clip, exposes its one-shot state for verification, and
never rearms. It is not an `ILoopResettable`, has no collider or Rigidbody, and
does not subscribe to RECALL, modify the chair, or touch puzzle and Echo state.
If RECALL occurs during playback, the persistent world-space cue finishes from
the chair without restarting.

## AV1 visual foundation

AV1 adds a scene-local, render-only presentation layer under
`Environment/AV1 Visual Foundation`. It contains one closed ceiling, four
baseboards, and three primitive clinical light fixtures. The root has no
Collider, Rigidbody, gameplay MonoBehaviour, reset target, animation, or audio
ownership. Its only MonoBehaviours are URP's additional-light data on the three
spot lights.

Eight textureless URP/Lit materials under
`Assets/_Game/Art/Materials/AV1/` separate clinical walls, linoleum, ceiling,
trim, fixtures, the active M5.2 partition, and the chair's muted wood/metal.
Existing puzzle-semantic orange, green, mechanism, and Echo materials remain
visually unchanged. Unity 6.3 reserialized the Echo material's internal
transparent-pass metadata when saving, while preserving its URP/Unlit shader,
cyan color, `0.35` alpha, render queue, and gameplay contract. The chair keeps
the same hidden root, transform, reveal event, and colliderless contract.

Three stable spot lights provide local pools without shadows. The existing
Directional Light remains the single soft-shadow owner at reduced intensity.
AV1 adds no Volume, fog, post-processing, texture, light bake, probe, pipeline
change, or runtime lighting behavior.

## Development scene

`Assets/_Game/Scenes/Development/EchoPrototype.unity` remains a primitive
mechanical prototype with the AV1 clinical presentation layer. It retains the
approved M4 and M5.1
rooms and loops as inactive variants. The active M5.2 variant places two plates
on the recording side of a low blocker, keeps two ordered status lamps over one
door, and retains the Player-only goal behind the partition. The active M6.1
discrepancy layer adds its hidden chair near the spawn-side left wall. Only one
puzzle and loop pair is active at a time. M6.2 routes its room tone and active
door through the scene-local mixer; AD2 adds persistent fluorescent/machinery
beds, plate edges, and successful-RECALL feedback; M6.3 retains the inactive
chair-owned procedural source.
Inactive M4 and M5.1 doors have no audio component. The earlier movement and
switch proofs also remain in inactive scene groups. Template URP settings remain
under `Assets/Settings`.

## SD1 directional study — partial implementation

`SD1 Directional Study` is a persistent scene-local authored state machine; it
is not an `ILoopResettable`, generic scare director, or puzzle dependency. The
first real M5.2 goal completion arms a stable baseline. Later RECALL counts only
unlock phases: delivery still requires the Player to enter an explicit authored
position, and footsteps plus the pending whisper also require a forward-facing
condition. RECALL itself never directly plays a horror cue.

Two imported mono sources route to `WorldSFX`: a fixed knock at
`(3.8, 1.5, -7.35)` and footsteps moving once from left to right behind the spawn
side. After two human reviews found them too quiet, both provisional AudioSource
volumes are `1.0`; their full-volume distances are `6.0 m` and `7.25 m`, covering
the respective authored trigger distances before linear outer rolloff. This is
configured, not yet mix-approved. A world-space breathing emitter remains near
the west side of the exit bay with no clip; the rejected erotic-reading
treatment cannot fire. The dependent exit-bay `Cold Spill` fade remains coded
but unreachable until replacement breathing is assigned. The SD1 root owns no
collider, Rigidbody, interaction, puzzle state, or reset target.

A camera child reserves one 2D, right-biased source and the study includes the
Spanish subtitle renderer for `¿Me escuchás?`. The whisper has no clip and
cannot fire yet. This explicitly marks voice casting/take selection as pending
rather than silently substituting synthetic speech.

An earlier five-RECALL pass verified delivery and reset behavior with the now
rejected breathing candidate. Its result is superseded for content acceptance.
The saved scene now has no breathing clip, so breathing and the visual
consequence require a fresh objective pass after replacement. M5.2 geometry and
reset targets remain unchanged.

## Not implemented

There is no semantic interaction replay, recorded audio/voice channel, multi-Echo
puzzle, generic puzzle-signal or anomaly framework, accepted voice performance,
physical acoustic occlusion, reverb system or adaptive score. Continuation is a
chapter-start checkpoint, not a save of arbitrary in-progress recordings.

## Optional physical house receiver — Offhook, 2026-10-01

`HouseInteraction` has one optional `HouseTelephone` reference used only for its
Telephone kind. Older scene behavior is unchanged when unassigned. The controller
owns the present handset pose, a sampled coiled cord and two moving contact clips;
`HouseMemory` remains the sole owner of answered/power/key/exit state. First use
calls the existing answer API, stops its base ringer and removes mains. Further E
presses alternate cradle/table poses without restoring power or replaying the call.
The short motion temporarily rejects another E, respects session pause, and does
not lock movement or camera. Foreign actors and inactive chapters are rejected.

Offhook authoring clears the old receiverClick reference to avoid duplicated
contacts; the ringer remains localized at the body and contact audio follows the
handset, with no Doppler. There is no received voice or extra Echo channel. The
optional physical offhook pose is not checkpoint data: a fresh scene starts in
its cradle; final-review Continue restores the existing answered/power consequence
without replaying a pickup or ring. Scene/model/audio authoring is opt-in through
`OffhookProduction`; Porter and earlier scenes remain recoverable.

## Physical carried reserve — Reserve, 2026-10-01

`ReserveProduction` derives a separate scene from Offhook and assigns all four
`HouseMemory` reserve representations: three wall packs and one camera-parented
pack/grip. Existing `ApplyFuseVisuals` remains the sole visibility owner. Leaving
the house, restoring Carried at final review and ordinary RECALL keep the current
Player's reserve; the Echo contains no carried item or articulated recording.

`CarriedReservePresentation` has explicit camera/Player/session references. It
raises the prop over .2 seconds on activation and uses a non-allocating sphere cast
to lower it near solid geometry, excluding the Player. Pause freezes that pose.
It changes neither camera/input nor reserve state. Carried renderers ignore rays
and cast no shadow; no collider, light or audio source is attached.

`PrintedText` supplies shadowed URP lighting to only the new reserve labels via
their existing `DiegeticText` component. Two fixed, shadowed patio-bounce lights at
open room returns preserve local navigation after the supply cut without changing
SE1's global consequence. Their references are included in the residence shadow
budget. This is authored indirect-light approximation, not dynamic GI.

Reserve also opts into an explicit `CampaignDirector.firstActLighting` reference.
Loading any later saved chapter restores SE1's completed ambient/reflection
multiplier and extinguished facility circuits through `RestoreCompletedLighting`.
It settles the state without knock/relay/door playback, releases the old ambience
owner and leaves recording history empty. Earlier scenes without the optional
reference retain their previous continuation behavior.

## Recorded body surface — Presence, 2026-10-01

`PresenceProduction` derives a separate scene and `EchoPresence` prefab from
Reserve. The same existing rigid mesh receives `RecordedPresence`: restrained
URP diffuse response plus fixed view-space display relief, alpha.90 and a small
edge-alpha addition. Position/yaw playback, recorded data and static geometry
are unchanged. No other actor or present-Player material is replaced.

An identity child shares that mesh and writes ColorMask0 self-depth at queue2990;
the color surface blends at3000 with ZTest LEqual. Both use the existing horizontal
.35–.95m proximity fade, with the depth helper clipped when fully hidden. Neither
casts shadows or adds a collider/light/audio source. Eight original submeshes
remain eight slots in both renderers. Transparent geometry behind the Echo is
rejected by this approximation; it is not order-independent transparency.
The preset was selected through same-camera comparisons and a full input route;
human perceptual acceptance remains separate.

## Direct final observation — Encounter, 2026-10-01

`EncounterProduction` derives a separate scene from Presence, opts into
`FinalDoorMemory.requireDirectEncounter` and assigns the actual RecordedPresence
color material. Legacy scenes retain their original branch. The authored inside
band is1.6×0.9m; the origin and exterior qualifying zone remain unchanged.
The present Player opens6B; both actual head/chest rays must clear its physical
leaf, glass and frame during the same immutable qualified pause. No fixed opening
angle is required. The existing0.6s contiguous interval, identity checks, pause/R
invalidation and exact-object preservation remain in force.

Visibility excludes disabled, culled, forced-off or shadow-only renderers and
requires the explicit supported color pass/material with nonzero opacity. Its
ColorMask0 depth sibling cannot count independently. Current authored renderers
have no material-property-block visibility override. `FinalRecordDisplay` changes
only the optional wording and reads the same witnessed/preserved object. This adds
no Echo channel or final-story claim beyond the current session's actual take.

## Physical records return — Records, 2026-10-01

`RecordsProduction` derives Encounter. It replaces the scene's obsolete continuous
west-boundary collider with facade spans and two1.72m clear apertures. The former
archive scenery gains a separate solid envelope and five coarse rack colliders;
a visible permanent partition redirects the old straight route through it. The
existing interlock, recording boundary and campaign systems are unchanged.
Existing room surfaces use persistent SurfaceMapping at the corridor's3.2m scale.

Two static AudioSources sit on the existing powered luminaires, using the traced
mono fluorescent loop, Ambience bus, .06 gain, linear.5–4.5m distance and listener
pause. They have no runtime state writer or Echo/reset participation. Their sources
would need explicit power ownership if the currently static lamps were changed.

## Rooms — explicit domestic spectral boundaries

`HouseRoomAcoustics` in the opt-in Rooms scene owns only the cutoff on two explicit
`AudioLowPassFilter` components, attached to the actual phone/fridge sources.
Camera→source rays query47 authored existing architectural colliders, excluding
props/Player/Echo/triggers. Logarithmic0.20s smoothing, 22000/1600Hz, frozen during
session pause; disabling restores clear cutoff. Source playback/gain/power remain
with HouseMemory/HouseTelephone. No new Echo channel or generic acoustic framework.
Focused input verification PASS37; human listening and native launch pending.

## Study — authored return exposure

The derived Study scene broadens the north study pane, keeps the physical seal
and moves the environmental body/floorboard source/safety to x1.65. Existing
collider identities retain all47 acoustic references. Safety spans y−.03..1.97;
the environmental MeshRenderer receives illumination but does not cast remote
shadows outside the finite watched volume. No HouseMemory rule, Echo prefab,
channel or gaze/exit dependency changes. Actual-input camera branches PASS24/17;
full route PASS446. This is exposure opportunity, not human recognition evidence.

The working Pane successor copies only the study-glass material, enables direct
specular highlights and sets smoothness0.78. Existing tint/alpha/blend/geometry and
source material remain unchanged. No runtime system or reflection capture added.
Actual-input branches PASS25/17; Pane is included in the compiled Readers checkpoint.

## Readers optical presentation — 2026-10-01

`ReadersProduction` derives Pane. `OpticalReaderMeshRecipe` creates four original
static meshes per existing optical head: bevelled enamel housing, open annular
bezel, recessed opaque optic, and qualification ring. Mesh coordinates are baked
to the existing transforms; all six `OpticalReader` references and rules remain
unchanged. The active ring still belongs exclusively to `OpticalReader`; this
adds no runtime presentation controller. Authoring checked all607 original
collider and54 audio-source serializations unchanged.

The B RECALL sign/backplate move down0.52m to uncover the lens. Three existing
P1 fixture cones illuminate the instrument faces, retaining their SE1 references.
Initial actual-input camera survey PASS62; selected full campaign PASS456,
four immutable takes and zero fixtures. The three adjusted lights still extinguish
through their original SE1 ownership.
Camera review also moved the P2 curtain/rail0.95m off the instrument, retaining
its colliders, replacing its flat visible mesh with original pleats, and placing
the same instruction on a smaller printed wall plate.
Pane's local study-glass optical material is included. No new recorded channel,
eye tracking or pupil/scan animation is implied. Human comprehension remains
EN PRUEBA; see the [Readers evidence](VisualEvidence/2026-10-01-Readers/README.md).


## Shutdown machine presentation — 2026-10-01

`ShutdownProduction` derives Readers through Editor APIs. The existing control
cluster stays under its PressureIsolation owner, is lowered15cm and rotated15°;
the shortened pedestal keeps its base on the slab. Cabinet and physical baffle
move together to expose a2.515m raw service aisle. Existing reserve fixtures move
with the composition; the first becomes an aimed cabinet light. Work-circuit
ownership, grip reach/hold time, motor/drain envelope and exit travel are unchanged.

PumpA receives the original `ShutdownMotor` derivative with open rear housing,
fixed supported guard and one separately pivoted rotor. Existing pump collision,
ports and sound anchor remain. `PressureMotorRotor` has explicit isolation,
session and Transform references; LateUpdate reads `MotorLevel01`, integrates
scaled time only while playing, and retains its angle at pause or zero. It writes
no gameplay state, audio, preference or Echo data. A restored isolated motor is
already zero, so it does not replay a coast. Shutdown uses its own checkpoint key.

Focused actual-input route PASS19 with one declared initial fixture. Continuous
campaign PASS462, four immutable takes and zero fixtures; fresh persistence
checks PASS45 across four reloads, with explicit fixtures and prior preferences
restored. Nine physical labels subsequently use the existing lit-ink
shader on one scene-local material; words and interactions are unchanged. Camera-selected layout and
runtime state are distinct from human movement/decay legibility and listening.

The inherited DiegeticText shader owner is explicitly assigned for these plates
and P2, so Awake retains the chosen ink. PrintedText now derives the normal from
the known XY glyph plane/local−Z, independent of TextMesh vertex normals. Stationary
material comparisons cover the live/restored control, P2 and existing Reserve
labels; they do not add input or listening evidence.

## Arrival successor — R1 and oblique return

`EchoProtocol_Arrival.unity` derives Orientation/Shutdown through Editor authoring. R1 workstation roots move as siblings under one placement group; its original recall Transform retains exact world pose/reference. Desk/chair colliders remain outside the anchor's interaction ancestry. Existing local fixtures are repositioned; gallery and shelf labels use physical plates with PrintedText owner bindings.

Arrival rebuilds only R3 south of z7 with mitered static floor/ceiling meshes, separate colliders and fitted walls. Existing rear/front door roots, headers, four referenced boxes and two separately parented motor sources rotate as one assembly about the original front position. Door-local targets, motion, clips, source settings, residual/arrival light roots and the P3 crossing remain intact. The same original reception is visible along the new45° arrival direction; no camera or Player teleport is added. Old west-wall seams/skirting crossing the opening are retired/trimmed.

`VS1HubReturnGate.Inside` and `CampaignDirector.AtReviewAnnouncement` now transform the Player sample into each BoxCollider's local frame and use center/size, checking enabled/active. This prevents rotated world-AABB corners qualifying as occupancy/arrival. No other chapter volumes are changed. Legacy D1 remains disabled. The forward and retreat/re-release input surveys pass17/21 checks with explicit initial fixtures; continuous integration and build are recorded separately in the Arrival evidence folder. Recognition, tension and listening remain human-pending.

### Observation P3 (2026-10-01)

The Observation successor derives Arrival. Its recording readers now occupy the
east lane, requiring body yaw270; the cooperative leaves and real R3 crossing
occupy the west lane. Reader qualifications/masks, relay logic, local leaf targets,
R1 checkpoint and immutable pose/yaw recording remain unchanged. Continuous sealed
glass colliders cover both observation windows; a.55m opaque core preserves the
5.3m interval and an east terminal wall prevents a recording-lane bypass.

B's physical assembly is.35m farther north than its input volume to open the
oblique approach; the input volume staysz43 and R3's crossing plane stays43.7.
A moved existing fixture lights its instrument. Two fixed `SlidingDoorAudio`
owners use the inherited R3 masters/WorldSFX at.13; their resets are appended after
the original29 targets. They report actual leaf movement and never belong to the
Echo. Author/configuration and focused/full-route evidence have distinct scopes
in `VisualEvidence/2026-10-01-Observation/`.

### Courtyard successor — implemented, 2026-10-01

The compact exterior connects the actual residence to the rigidly relocated
Threshold/Porter roots. Existing final origin, local witness/sight boxes, hinged
leaf, exit body and fixed mechanism sources move together; Echo data and review
rules are unchanged. Original ground colliders remain beneath combined original
paving/drainage geometry. Local sky contributions join the existing shadow budget.

`CampaignRoomTone.finalReviewRegion` is an optional local-frame box override
for the relocated final's .20 ambience level; the returned-key .015 target still
has priority. Legacy scenes with no box keep the original coordinate mix.
`CourtyardAir` owns one dedicated fixed3D source, fading by two overlapping
exterior boxes and explicit real architectural partitions. It follows existing
listener/session pause and mixer output; it never owns house power, phone,
refrigerator, Echo or RECALL. Objective camera/input/persistence evidence and
remaining human limits are separated in the Courtyard report.

### Coverage — implementación acotada, 2026-10-09

La escena `EchoProtocol_Coverage` reemplaza el aislamiento del subsuelo por
`MachineCoverEncounter`: bomba accionada, presión, manivela, paso lateral y salida
con referencias explícitas. `RecordedMachineGrip` captura tiempo, destino y
sostener/soltar. `EchoRecording` copia y valida esos eventos junto a los frames;
`EchoPlayback` aplica el estado del mando en el tiempo real de esa misma toma y
libera su dueño al finalizar. No graba ni reproduce audio, mecanismos completos
ni estados del mundo. `RecordedMachineControl` verifica entrada física y alcance.

La bomba real produce ruido y presión; una reja responde a la presión. La manivela
y la palanca requieren E sostenida. El cierre de salida provoca una embestida
autorada de `MachineCoverThreat`, que sigue un pequeño grafo físico y consulta
visibilidad, movimiento presente y marcha real de la bomba. Nunca lee una toma.
`ReplayLastRecording` permite el reintento deliberado desde el menú conservando
la identidad de la grabación; contabiliza reproducción repetida separadamente.
Completar la salida no corta el mando: la bomba sigue la toma hasta su final o
hasta el cierre explícito del registro al entrar a la casa. El paso de regreso
tiene un quiebre físico que permite perder la vista antes de atraer al oyente.

`EP.CampaignCheckpoint.Coverage.v1` conserva únicamente salida completada. Una
carga parcial vuelve al comienzo seguro con grabador vacío y amenaza en reposo;
la salida completada permanece abierta y sin grabación activa. Tres dueños de
audio de puerta se reinician tras restaurar posiciones, evitando sonorizar una
carga como movimiento. Las pistas cambian con el progreso. Evidencia y límites
perceptuales: `COVERAGE_CANDIDATE.md`.

### Embodiment — personajes integrados, 2026-10-09

`EchoProtocol_Embodiment` deriva de Coverage y usa
`EP.CampaignCheckpoint.Embodiment.v1`. El prefab privado `EchoEmbodiment` y SE2
usan Eric; `MachineCoverThreat` usa Nathan con otra escala y postura. Dos
superficies URP/Lit separan cuerpo y ojos. No hay Animator autónomo.

`PlayerBodyPoseSource` captura fase/velocidad de giro y, mediante una referencia
explícita a `RecordedMachineControl`, el punto del mando sostenido en coordenadas
del Player. Esos canales se suman a fase de paso, respiración, inclinación de
cabeza y velocidad local en `RecordedBodyPose`; los structs quedan copiados en
la misma toma inmutable. La captura de sostener/soltar sigue siendo
`RecordedMachineGrip`. No se agregó grabación de voz, mirada, inventario ni audio.

`RecordedCharacterMotion` muestrea clips autorados y deriva apoyo, rodillas,
giro, inclinación, mano y pulso de párpados únicamente de esos datos. Al pausar,
no avanza un reloj propio. La mano del Echo no consulta el mando presente. El
perfil del perseguidor recibe intención desde el tiempo de liberación presente;
su postura, mandíbula durante el grito y contactos acompañan la misma IA,
velocidades, visión, audición y captura de Coverage. Voz y contacto conservan
clips, niveles y mezcla; la voz sigue la cabeza articulada.

`FinalDoorMemory.opaqueDirectWitness` habilita la lista exacta del material
URP/Lit de este cuerpo y exige un pase de color soportado, opaco y visible.
Rechaza material ajeno, transparencia, alfa nula, sombras solas y render oculto.
Los rayos de pecho/cabeza, puerta, zona, pausa y toma siguen siendo obligatorios.
Las escenas anteriores conservan la rama del shader original. Los rótulos de
P1 se bajaron 0,54 m para despejar la cara, sin cambiar texto o validación.

Fuentes licenciadas y sus derivados permanecen excluidos del Git público;
procedencia y recuperación local: `EMBODIMENT_CANDIDATE.md` y `CREDITS.md`.

## Impact — encuentro presente, 2026-10-11

`MachineCoverPresence` recibe referencias explícitas al encuentro, amenaza,
cámara, fuentes y luz. El mando prima un contacto una vez por intento; la
liberación inicia sus capas presentes. Distancia recorrida causa los pasos y
aproximarse al marco causa su golpe. Rayos físicos controlan brillo/atenuación;
pausa congela relojes y fallo/reintento detiene/restablece las capas. No lee
`EchoRecording`. `MachineCoverThreat` conserva visión por rayos y permite
holgura de navegación por esfera, velocidad y alcance de contacto serializados.

`PlayerEchoClearance`, conectado al LoopController activo de la campaña, mueve
sólo al CharacterController presente cuando se acerca al torso reproducido.
Preserva el origen compartido de RECALL hasta separarse y no escribe root,
joints ni toma. La escena Impact configura .64 m; aproximación sostenida y
comparación inmutable se verificaron con entrada normal. HUD Folio separa
RECALL de objetivo y da prioridad al subtítulo; pistas se limpian en cada
cambio de progreso, incluido el reintento. [Alcance y evidencia](IMPACT_CANDIDATE.md).

## Incursion — scoped present pump performance (2026-10-11)

The Incursion scene opts MachineCoverThreat into explicit entry, recognition,
charge, listening and physical capture states. MachineThreatPerformance resets
and acts only the present Nathan anatomy, including planted support, arm drive,
gaze, sampled voice envelope and two-arm catch. Echo pose/replay code is unchanged.
The capture temporarily suppresses Player movement/look and LoopController's
live reset input for about 950 ms; reset restores the controls and camera offset.
MachineCoverEncounter.HideEncounterHud clears the existing HUD only during this
released encounter. MachineCoverPresence lowers pursuit layers during pump
attraction and plays the launch contact with the charge transition. All new
scene references are authored through the Editor; Impact remains available.

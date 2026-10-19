# Audio design

## Resonance / candidata sonora, 2026-10-08 — implementada y objetivamente verificada; escucha EN PRUEBA

Escena derivada de Lighting. Ocho emisores y trece masters mono nuevos; mezcla
existente conservada, con el limitador, volumen, dinámica reducida y mono de
`SessionAudio`. No se añade sonido al registro ni al cuerpo del Echo.

| Momento | Causa y consecuencia sonora |
|---|---|
| Aprendizaje / SE1 | Contactos de metal real en lectores; relés breves siguen las tres luces; puerta conserva sonido doméstico grabado. |
| SE2 | Fricción al atravesar el acceso trasero; breve motivo musical interno al descubrir el cambio. Ventilación se resta por su dueño existente. |
| P3 | Contactos mecánicos en A/B; tres tensiones del metal al transferir, con retraso de 1.35 s para dejar leer la confirmación. |
| Retorno por archivos | Un arrastre se aproxima detrás de la pared al cruzar la zona después de completar P3. No son pasos del Echo. |
| Planta | Aire local mientras funciona. Drenaje → contracción del caño → un único grito adulto distante; recuperación sin bucle vocal. |
| Casa | Al atender, línea en el auricular real: espera breve y llanto. Colgar desvanece la línea; volver a levantarla no repite la actuación. |
| 6B | El mismo motivo interno vuelve tras RECALL; devolver la llave lo resuelve con una versión más baja y lenta. La salida conserva su fundido final. |

Los seis emisores físicos tienen posición, distancia y filtrado suave ante
paredes reales; la cabeza modifica discretamente el espectro posterior. Esto
no es HRTF. Los dos emisores de música son no diegéticos, centrados y sin voces.
La reverberación corta y los decaimientos están editados en los masters para
mantener una paleta estable y limitar el coste. No hay planificador aleatorio.

Las secuencias escuchan estados existentes: no alteran puzzle, iluminación o
registro. RECALL despeja el motivo y cancela un evento pendiente de P3; no rearma
los encuentros ya consumidos. Las cargas fijan los acontecimientos pasados como
estado inicial. La pausa congela tanto tiempos dramáticos como fuentes.

Material, licencia, recortes y modificaciones: `ArtSource/Audio/Resonance/`.
CC0 nuevo de qubodup, AntumDeluge, Brian MacIntosh, SPAudiobooks y Archeos;
el aire deriva del HECKFRICKER ya acreditado. Las voces se obtuvieron como
previews públicos HQ MP3: no son masters originales sin pérdida. Ninguna voz
se clonó o generó. Detalles de verificación y escucha pendiente en
[RESONANCE_CANDIDATE.md](RESONANCE_CANDIDATE.md).

Las descripciones anteriores de la línea telefónica silenciosa corresponden
a Lighting y sus antecedentes. Actuación, credibilidad y miedo en Resonance
permanecen **EN PRUEBA** hasta una escucha humana.


This document translates the product direction in
[`Design/Echo Protocol - Game Design Document v0.2.pdf`](Design/Echo%20Protocol%20-%20Game%20Design%20Document%20v0.2.pdf)
into a testable sonic language. Repository behavior and playtest evidence remain
authoritative for what is implemented and validated.

## Contraprueba — implemented 2026-10-08

P3 uses the existing credited ventilation, relay and door palette. Its sources
belong to the fixed apparatus: A moves the blind, B transfers the circuit and
fades ventilation from source gain 0.13 to zero over 2.8 seconds while the visible
rotor stops. Returning to A exposes quieter room tone and the moving blind;
completion plays the fixed relay contact. No sound is attached to the Echo and
no unrecorded footstep, voice or action is reproduced. RECALL restores machinery,
light and blind; pause freezes their progression.

The actual listener is captured during the chapter in stereo and in the existing
mono/reduced-dynamics path. Signal measurements are reported in the candidate
evidence; they establish neither perceived loudness nor human localization.
Use headphones only at a comfortable volume; speakers and reduced dynamics remain
available. No new voice, service or sound asset was generated or purchased.
[Delivery and listening limits](CONTRAPRUEBA_CANDIDATE.md).

## EncounterArt — implemented 2026-10-02

SE2 and6B add fixed current-world ventilation with visible rotors. The authored
motor envelope reaches zero in1.7s after hidden SE2 presentation and1.1s after
actual final RECALL; source pitch is fixed, not simulated deceleration. Their
local institutional beds fall to22% and8%, respectively, and recover outside.
A single wood strain belongs to the rear doorway frame on genuine SE2 entry;
it is never an Echo step. Existing CC0 fridge/wood masters are reused unchanged,
with provenance in `ArtSource/Audio/EncounterArt/manifest.json`.

Sources are fully spatial, with no Doppler; comfortable-volume intent, existing
mono/reduced dynamics/master/limiter preserved. Signal, wiring, pause and route
checks pass. No listening, mix/localization, comfort or fear approval is claimed.
[Selected evidence](ENCOUNTER_ART_CANDIDATE.md).

## Expanded campaign — 2026-09-30

Thirteen original synthesized masters add pressure machinery, relay contacts,
telephone/circuit tones, wall contacts, close movement, archive resolution and
three Player contact variants on each of two surfaces. Their deterministic source
and per-file checks are in the asset ledger and campaign evidence. No voice was
created or imported. This is implementation, not human timbre approval.

Pressure subtracts machine level as valves complete. The house uses a localized
ring and circuit cue, explicit written order for the speaker path, and a close
movement cue after the visible hall encounter. Gallery authorization sounds at
the physical shutter, not at the Echo body. Player steps belong to current
movement; Echo playback remains silent and contains only position/body rotation.
The existing linked-channel limiter, reduced-dynamics option and mono path remain.

## Brief-edition checkpoint listening paths — 2026-09-30

The production scene adds listener-level peak attenuation, a reduced-impact
option and an optional mono mix for speakers, plus a default 70% master volume.
Neither path boosts quiet signals. Normal linked-channel threshold is 0.65;
reduced mode uses 0.18, with a 120 ms release. Actual sample-processor checks
pass stereo bounds, mono agreement and no upward gain. These tests do not
establish device loudness, timbre or listening comfort.

The new exit reuses the project-owned frame-relay cue and accepted door clips;
no new recording or generated voice is imported. Sound follows physical door
movement and three extinguishing fixtures behind the Player. Acceptance fades
the two assigned facility beds toward 8% over five seconds. The final door
composition reuses the original domestic source model. Human effect is EN PRUEBA.

## H1.1 production direction — 2026-09-12

Use existing Unity, Blender and local audio editing for the approved 20–25 minute
first delivery. Tripo is deferred to a separately authorized pilot; Higgsfield
is not incorporated. H1.1 preserves all audio, mix and timings for observation;
no asset generation or new service is included. Prototype mix, comfort, device
paths and emotional effectiveness retain their existing EN PRUEBA boundaries.

## Current evidence

- **Human-approved:** the M6.1b facility-air layer and its reveal fade.
- **Human-approved causal rule:** M6.2 door audio follows physical movement. AD2
  replaced the synthetic motor with a user-selected domestic movement/endpoint
  pair and added facility beds, truthful plate edges, and successful-RECALL
  feedback. The integrated palette is approved for continued prototyping, but
  no production timbre is locked.
- **Objectively verified, not emotionally approved:** the M6.3 chair cue plays
  once after reveal. A guided pass proved delivery but produced no terror or
  discomfort and did not validate spontaneous localization.
- **Human-approved visual context:** AV1 provides a closed clinical prototype
  space without changing the audio or gameplay contract.
- **VS1-VT1 objectively verified, human listening pending:** all three P1 optical
  readers now use provisional 3D acquire/release cues, and both active service
  gates use movement/endpoint cues. They route through `WorldSFX`, reset with the
  loop, and pass deterministic edge/cycle tests. The reader cues reuse the
  pressure-plate edge assets as replaceable functional placeholders; no new
  production timbre is approved by this wiring pass.
- **VS1-SE1 lane contraction objectively verified, human review pending:** one
  world-locked domestic knock and three far-to-near fixture failures are gated
  after P1 and the end of the Echo. The event requires an empty-lane check, plays
  one project-authored mono frame/relay strain at the real fixtures at
  `z=4/9/14`, ducks the facility beds to `0.38`, restores them over `1.5 s`, and
  leaves the lane dark plus the threshold revealed. The door attribution is
  human-approved at prototype level. The first-checkpoint pass now follows the
  total blackout with a `5.2 s` inward door opening. Its hinge emitter reuses the
  accepted domestic movement loop at descending `0.72 → 0.58` pitch as a
  provisional friction/creak treatment. Crossing the threshold now closes the
  leaf after `1.1 s` and plays one deterministic `0.38 s` mono procedural impact
  from the hinge through `WorldSFX`. Its clip peak is limited to `0.58` and source
  gain to `0.58`; force comes from silence, proximity, and attack rather than
  unsafe loudness. The replacement construction, creak/portazo timbre, emotional
  effect, device mix, reduced-dynamics behavior, and comfort are not yet approved.
- **SD1 partial implementation:** the dry knock, concrete footsteps, and authored
  phase gates pass objective checks. A treated breathing candidate was rejected
  because it read as erotic rather than threatening; its asset and assignment
  were removed. After two reviews found the retained cues too quiet, both
  provisional source volumes are now `1.0` and their full-volume distances are
  `6.0/7.25 m` for knock/footsteps; the revised mix is still `EN PRUEBA`.
  `MX_Main` now adds a provisional global `+8 dB` after the overall presentation
  still read too quietly; all active sources route through it, preserving their
  relative contrast. This gain and its summed-output headroom need human and
  device verification.
  Replacement breathing, the dependent delayed light consequence, the whisper,
  integrated spatial/mix evidence, accessibility modes, and the human fear
  checkpoint remain pending.
- **Approved direction, not implemented:** audio becomes the primary horror
  weapon through recurring families, authored variants, spatial doubt, domestic
  intrusion, strong events, and recovery.

The chair is now a secondary proof or motif anchor. The previous `AD3` plan to
stretch it into a 5–8 minute route is superseded. SD1 is frozen as a partial
technical sandbox; the active milestone is the human review of VS1-SE1.

## Creative thesis

Audio is the architecture of memory and the main instrument of fear:

1. Build a coherent facility whose causes, layers, and positions become familiar.
2. Let the Player trust physical sound and exact Echo playback.
3. Recur to a small set of motifs with authored changes in direction, distance,
   material, duration, intensity, and completeness.
4. Make home, presence, and remembered time intrude into the virtual space and
   occasionally seem to cross into the Player's real surroundings.
5. Alternate preparation, impact, uncertain checks, consequences, and recovery.

This is not constant noise, a random scare scheduler, or permission to corrupt
the Echo. Several strong events can occur within the 20–25 minute delivery, but they must be
earned, meaningfully different, and safe.

## Mechanical trust contract

- Doors, plates, goals, RECALL, Player actions, and Echo actions never lie about
  physical state.
- The same functional cause keeps its verified timing and position. Production
  timbre may replace a prototype, but not its causal contract.
- Variants belong to authored horror motifs or explicitly defined scene states,
  not random mechanism feedback.
- If audible Player actions are later replayed, their temporal and spatial
  relationship to the Echo must be specified and verified.
- An anomaly may subtract context, introduce memory, or create spatial doubt. It
  cannot imply that an otherwise identical Echo recording changed arbitrarily.
- Every puzzle and essential narrative fact remains understandable without
  relying on hearing alone.

## Sonic families

| Family | Material | Dramatic use | Current state |
| --- | --- | --- | --- |
| Functional truth | door, plates, RECALL, future interactions | Teach exact causality and prove the system still works | Door, plate edges, and successful RECALL implemented; timbres replaceable |
| Facility / structure | air, fluorescents, ducts, metal, machinery, pressure, glass | Establish the building, distance, and off-route activity | Three prototype ambience layers implemented |
| Presence / body | footsteps, breath, fabric, fingers, weight, brief running | Imply a nearby body without confirming an enemy | Planned |
| Domestic memory | household door, chair, dishes, water, toy, child melody, accident fragments | Make home invade the institution | Domestic door character and chair proof only; broader family planned |
| Time / repetition | repeated footsteps, prior actions elsewhere, an Echo path that sonically continues | Expose omitted or misremembered cycles while playback stays correct | Planned |
| Music / voice | memory motif, guiding voice, intimate fragments | Carry recollection, identity, grief, and resolution | Planned; exact content TBD |

## Authored variation

A recurring motif should not replay one identical file until it becomes harmless.
Build a deliberately limited family of variations along useful axes:

- close, medium, and distant;
- left, right, behind, above, or moving through space;
- dry and domestic versus reverberant and institutional;
- brief, sustained, approaching, departing, or abruptly interrupted;
- restrained, forceful, partial, and complete.

Selection follows narrative state and composition. Do not use unconstrained RNG
or a generic director to make timing unpredictable. Repetition should teach an
identity; variation should change the Player's interpretation of that identity.

## Spatial grammar

### World-locked

World-locked sounds are the default. Their emitter occupies an authored position,
and the stereo image changes correctly when the Player rotates. Doors,
mechanisms, footsteps in the room, distant impacts, and most breathing belong
here. Trustworthy spatial behavior is required before it can create doubt.

### Head-locked or internal

A rare intimate event may stay near one ear or inside the listening perspective
when the view turns. This is an exception for whispers, breath, or fragmented
memory, never a normal physical source and never the only carrier of essential
information. The exact HRTF, spatializer, headphone rendering, and speaker
fallback remain TBD; no middleware or global audio framework is approved yet.

### Trans-screen intent

Some dry domestic sounds should plausibly belong to the Player's home: a knock
behind-right, weight at a household door, movement near a corner, or breath close
to one shoulder. Avoid UI reinforcement, camera shake, or exaggerated game-like
reverb when the purpose is source doubt.

The desired evidence is a spontaneous turn, check of a real door or corner,
removal of one earcup to verify the source, or uncertainty about whether the
sound was inside the game. The mix must never instruct the Player to raise volume.

## Tension and strong events

Use the authored sequence:

`normality → preparation → concentration → contradiction or approach → impact or no-payoff → check → consequence or uncertainty → recovery`

- Quiet construction and strong impact are complementary, not opposing schools.
- Multiple strong events may appear across the complete game; the exact count and
  placement remain TBD. Only a smaller subset should function as chapter-scale
  peaks.
- Strong does not mean maximally loud. Use fast attack, spectral contrast,
  proximity, direction, motion, interruption, and removal of masking layers.
- A check may yield nothing or later reveal a mark, object, route, or remembered
  fragment. Vary consequences so the Player cannot safely ignore every sound or
  predict a reward after each one.
- Routine RECALL, puzzle failure, pause, and arbitrary timers are not automatic
  scare triggers.
- Recovery is authored space, not unused time.

## Five-chapter audio progression

| Chapter | Facility and space | Presence, memory, and music | Pressure |
| --- | --- | --- | --- |
| `Protocol` | Stable, dry, causal facility | Minimal personal material | Safety and first uneasy impact |
| `Familiarity` | Directional checks in known space | Domestic detail enters without explanation | Spatial doubt and small consequences |
| `Recollection` | Home and institution overlap | Presence motifs and first memory fragment | Recurrent variants and uncertain checks |
| `Denial` | Incompatible spaces coexist while mechanisms stay exact | Time/repetition exposes omitted experience | Sustained pressure and authored set pieces |
| `Acceptance` | Facility and memory converge, then lose density | Motif resolves with the remembered truth | Emotional peak, aftermath, and cut to black |

Exact motifs, chapter timing, event density, voice identity, and score language
remain TBD.

## Current study: SD1

`SD1` is an approximately five-minute study in the existing AV1 room. It adds no
new puzzle and changes no Echo rule:

1. stable facility baseline;
2. one dry domestic knock behind-right, clearly distinct from the sliding door;
3. authored footsteps travelling left to right and behind;
4. world-locked breathing whose position remains coherent as the Player turns;
5. one exceptional, intelligible head-locked whisper near a single ear;
6. one subtle delayed visual consequence associated with a cue;
7. recovery with truthful mechanisms still available.

The knock, travelling footsteps, and authored phase gates are implemented and
objectively verified. Their perceptual and emotional effect is not approved.
The first treated breathing candidate was rejected for an erotic reading and
cannot fire; its world-space placeholder remains unassigned. The dependent light
consequence therefore also remains pending. The whisper line is
`Can you hear me?`, spoken in English and subtitled `¿Me escuchás?` in Spanish.
Its performer, identity, treatment, and clip remain TBD; the current reserved
host is a 2D right-biased source near the camera, subject to direct audition.
New assets require the same source, license, hash, full-audition, and
public-repository review used by AD2. `AUDIO_ASSET_LEDGER.md` records the three
accepted prototype SD1 sources and their replaceable status.

## Mix, safety, and accessibility

Recommended player-facing copy:

> Designed for headphones. Set a comfortable volume; details do not require high
> volume.

- Preserve at least `6 dB` of headroom and target a worst-case master peak no
  higher than `-6 dBFS` while prototyping. Digital peaks do not prove physical
  listening safety.
- Build impact through composition and dynamics, never through a request for high
  system volume.
- Plan `Headphones` and `Speakers` presentations, left/right calibration,
  separate ambience/effects/voice controls, and `Reduced Dynamics`. These are
  product requirements, not currently implemented settings.
- Exercise stereo headphones, stereo speakers, mono compatibility, and reduced
  dynamics before claiming support.
- Stop a test for pain, tinnitus, blocked-ear sensation, dizziness, panic, or
  other physical distress.
- Follow current safe-listening guidance from
  [WHO](https://www.who.int/news-room/questions-and-answers/item/deafness-and-hearing-loss-safe-listening)
  and the [WHO/ITU gaming standard](https://www.itu.int/hub/2025/03/new-who-itu-standard-aims-to-prevent-hearing-loss-among-gamers/);
  implementation details should be checked against
  [ITU-T H.872](https://www.itu.int/epublications/publication/itu-t-h-872-2024-10-safe-listening-for-video-gameplay-and-esports).

## Implementation boundaries

- Keep the implemented `Master`, `Ambience`, and `WorldSFX` routing and one active
  `AudioListener` until a concrete feature requires more.
- Functional sound resets with its mechanism; normal ambience continues across
  RECALL. Existing M6 discrepancy state persists until scene reload, but that is
  not a universal anomaly rule.
- Imported assets are never destroyed by runtime cleanup. Replacement remains
  atomic and preserves owner, event, reset, and routing contracts.
- Do not add a generic audio manager, scare scheduler, random director, emitter
  pool, dynamic occlusion framework, reverb framework, adaptive score, or paid
  middleware before SD1 proves a concrete need.
- Do not change `AudioListener`, input, time scale, recording, playback, puzzle
  state, or collision to manufacture fear.

## SD1 verification gates

### Objective

- World-locked sources remain in their authored location through Player rotation;
  the exceptional head-locked cue remains perceptually near its intended ear.
- Each authored event fires only from its explicit state and never duplicates or
  accumulates through five RECALLs.
- Echo frames, timestamps, path, end behavior, puzzle truth, reset order, and
  functional-audio causality remain unchanged.
- The visual consequence changes no collider, path, interaction, or puzzle state.
- The mix has no clipping, retains headroom, and is exercised through headphones,
  speakers, mono, and reduced dynamics at comfortable system levels.
- The experience remains completable in mute and produces no project-authored
  Console errors.

### Human

Warn generally about psychological horror and sudden audio without revealing
timing. Record device, comfortable system volume, presentation mode, prior
familiarity, and physical comfort.

Observe before asking: turns toward virtual or real space, checks of a door or
corner, removal of an earcup, hesitation, and changes in movement. Then ask what
was heard, where it came from, whether it seemed inside or outside the game,
whether it felt authored or broken, whether the Echo stayed trustworthy, and
what produced tension. A guided pass validates delivery only.

Do not claim trans-screen fear, localization, safe loudness, speaker support,
mono support, reduced-dynamics support, or accessibility without direct evidence
for each claim.

## Delivery sequence

1. `AD0–AD2` — direction, legally reviewed prototype palette, integration, and
   human prototype listening complete.
2. `AV1` — clinical visual place objectively verified and human-approved for
   continued prototyping.
3. `SD1` — current planned directional-horror study and perceptual checkpoint.
4. Horror map — define the 20–25 minute escalation and each future puzzle's
   vulnerability before producing more rooms.
5. Later audio integration — replace prototype timbres, expand approved motif
   families, and design chapter set pieces only from validated SD1 evidence.

The old chair-centred `AD3` route is superseded, not implemented. Its useful
normal-audio work remains in AD2; the chair cue can be retained or replaced later
without controlling the roadmap.

## Open decisions

- Exact SD1 clips, triggers, locations, whisper casting/treatment, and subtle
  visual consequence.
- World-locked versus head-locked rendering technology and speaker fallback.
- Temporal contracts for Player/Echo footsteps, breathing, interactions, and
  repeated-action audio.
- Production RECALL signature and final functional timbres.
- Motif palette, memory melody, voice identity, casting, language, and treatment.
- Number, placement, preparation, and aftermath of strong events.
- Exact `Reduced Dynamics` behavior and calibration UX.
- Full-game room, puzzle, and recovery map.

## Offhook telephone — implemented candidate, 2026-10-01

The optional house call uses a real rotary ring and matching pickup/soft-hangup
contacts from two CC0 cookies+policy recordings. They are public HQ MP3 previews,
not original lossless masters; `ArtSource/Audio/Telephone/` retains source-page
license evidence, hashes and exact edits. Three mono 48 kHz derivatives have
peaks .26/.24/.23 and source gains .30/.38/.38. These figures establish headroom
only, not perceived comfort. No global mix gain changes were made.

Ringing stays at the telephone body every eight seconds until first answer.
Contacts follow the moving handset with zero Doppler; the generic relay click is
unassigned in Offhook to prevent doubling. Descolgar removes mains through the
existing house state. The line itself is silent. Colgar changes only the receiver;
it cannot restore light or cause another ring. No artificial line noise, voice,
head-locked emitter or Echo audio is added. Session pause freezes audio normally.
Timbre, attribution, spatial comfort and emotional timing remain EN PRUEBA.

## Records fixture hum — implemented candidate, 2026-10-01

The traversable records room reuses AUD-AMB-002 as two fixed3D luminaire sources,
Ambience/.06/linear.5–4.5m. Existing main-room beds and preferences remain intact.
Only the currently powered archive fixtures own this sound; no event or Echo audio
is inferred. Selected isolated input checks verify source configuration/anchors and
Player contact cessation at rest. Audible localization, mixture balance, comfort
and the effect of leaving the room remain unlistened and **EN PRUEBA**.

## Rooms — implementación del límite entre habitaciones

En la escena Rooms, teléfono y frigorífico tienen low-pass explícito de22kHz
libre/1,6kHz obstruido, suavizado logarítmico0,20s y pausa de sesión. Sólo se consulta
la arquitectura real; no cambia volumen ni reproducción. PASS37 comprueba estados,
transición/pausa y corte del dueño al atender. Es aproximación al camino directo,
sin difracción/reverberación. Corte y timbre requieren escucha a volumen cómodo;
no hay aprobación humana ni un nuevo archivo de audio. Ver evidencia Rooms.

## Coverage — sonido causal del encuentro, 2026-10-09

La bomba accionada usa el master existente `Finish/PressureBed.wav` en su
posición física. Su envolvente sube en0,18s y baja en0,30s; la descarga sigue
la presión y termina con ella, aunque el archivo de aire sea más largo.
Se retiran los lechos de maquinaria anteriores sólo durante este encuentro.
Los contactos manuales salen de cada mando y los mecanismos de las rejas
suenan desde sus marcos reales. Todos pasan por WorldSFX y SessionAudio.

La amenaza presente conserva el grito adulto CC0 `Shaft_DistantCry`, contactos
de pasos registrados y un golpe cercano. La apertura causa grito, embestida,
atracción hacia la bomba cuando se pierde la vista y recuperación silenciosa
tras escapar. El contacto final puede terminar su cola breve durante el menú
de fallo; el resto del mundo se pausa. Ninguno de esos sonidos sale del Echo.
Sólo el mando sostenido/soltado es un canal nuevo explícitamente grabado.

No hay generación vocal, compra ni nuevo proveedor. Se reutilizan masters y
trazabilidad de Finish/Resonance; mono y dinámica reducida siguen disponibles.
Captura objetiva, límites digitales y juicio humano se distinguen en
`COVERAGE_CANDIDATE.md`. Credibilidad del grito, cercanía, comodidad, contraste
y miedo siguen EN PRUEBA; no se exige subir el volumen.

## Impact — presencia del encuentro, 2026-10-11

La candidata Impact reemplaza sólo la puesta de la amenaza presente: contacto
previo durante el mando, grito adulto cercano a altura original, golpe de marco,
tres pasos alternados, fricción móvil y pulso interno no verbal. La voz/pasos
siguen al cuerpo; paredes reducen brillo/nivel sin anular orientación. No son
canales reproducidos por el Echo ni eventos aleatorios. Bomba .32/purga .18;
WorldSFX y Master +10 dB permanecen. La salida limita picos normales a .5,
conserva mono y el límite .18 de dinámica reducida. No altera volumen guardado.
PCM real a volumen 1: irrupción −20,34 dBFS RMS frente a −32,23 anteriores;
retirada −22,72 frente a −32,71, con pico −6,02 y cero samples a escala completa.
Los promedios usan etapas de distinta duración; no son una aprobación por oído.
Comodidad, contraste, localización espontánea y miedo siguen EN PRUEBA.
[Cadena, fuentes, comparación y prueba](IMPACT_CANDIDATE.md).

## Incursion — attack phrasing and intention (2026-10-11)

Four short CC0 derivatives reuse the existing adult Archeos and Brian MacIntosh
sources: recognition strain, two-effort dry attack, pursuit effort and capture
contact. No generation/pitch shift. Jaw articulation reads the playing present
clip envelope; planted feet drive contacts. The charge starts with a floor support;
listening to the live pump stops pursuit efforts and subtracts most pulse/grain.
Master, user gain, .5 normal/.18 reduced ceilings and mono remain as in Impact.
Performance, fear and comfort remain EN PRUEBA humana. INCURSION_CANDIDATE.md
records the actual capture and final checks; signal checks are not casting approval.

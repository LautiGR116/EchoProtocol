# Horror design tracker

## El intruso, 2026-10-12 — candidata implementada en Incursion

**IMPLEMENTADA; prueba del creador: «Lo jugué y sigue sin asustar».** El objetivo
emocional no está conseguido; la causa dominante y la comodidad no se han
determinado con este feedback. La dirección aprobada fue reautorizar el
regreso de P3 y la bomba como una sola escalada: descubrimiento, preparación
inquietante, irrupción, huida y salida expuesta. Reemplazar material débil de
esos tramos; no agregar otro puzzle ni prolongar la campaña para justificarlo.
La prioridad es un cambio perceptible en actuación, sonido y peligro.

La última entrega de P2/SE2 incorpora lectura causal y figura estática con un
impacto único. No hay prueba humana identificada de esa última versión. El
usuario sigue describiendo poca mejora general y pide un salto de terror.
La revisión de fuentes encuentra otra limitación: las voces de Incursion siguen
siendo ediciones de la misma interpretación de Archeos usada anteriormente.
Más mezcla sobre esa actuación no constituye por sí sola una nueva amenaza.

### Implementación en la candidata

`IntruderStaging` reutiliza un solo cuerpo presente; nunca escribe la toma.
P3 coloca la visita durante el desvío oculto y la descubre junto al Echo real.
La manija responde a la preparación manual; la abertura ahora enfrenta la
palanca, con apoyo de mano y entrada por CharacterController. Metal y voz
ocurren separados; se retira el pulso musical. Una conexión corta del retorno
lleva al cuerpo hacia la salida de la bomba, tras perder al jugador de vista.
El pretil permite cruzar cerca sin convertir ese paso obligatorio en una captura.
Se conserva captura/reintento, cámara libre y reproducción del mando real.

Comprobación acotada en Editor: recepción de P3 con toma real y dos cuerpos en
cámara; preparación grabada del mando, irrupción, desvío y salida completada
sin captura, desde un checkpoint cercano con la manivela previa aceptada.
Se corrigieron el encuadre lateral, la conexión al puesto de desvío, la detección
intermitente junto a la separación y la placa de salida vista al revés.
La revisión final verificó expresión cercana y apoyo de mano al llegar al
puesto; compilación y Console sin errores nuevos. Se desligó el dorso de la
placa de salida del actor: estaba viajando con él. Escena guardada y Editor
fuera de Play, sin auto-tick auxiliar.
No builds, recorrido de campaña ni nueva suite. Actuación, miedo y escucha
integrada quedan para la prueba humana; no se infieren de estas comprobaciones.

### Dirección implementada

1. **Hay alguien más en el regreso.** En la recepción final de P3 se ve al Echo
   verdadero en su posición registrada y, detrás de él, una presencia física
   parcialmente oculta por el marco o la persiana. Es la amenaza de la bomba,
   anticipada aquí. La composición permite descubrir que hay dos cuerpos;
   un roce de mano contra el marco dirige la atención, sin gastar el grito.
   El encuadre deja ver que la presencia sigue allí cuando la reproducción
   real abandona el lugar o termina. No inventar una salida del Echo para
   conseguirlo ni exigir que el jugador espere el final de una toma larga:
   si su trayectoria no proporciona ese contraste, el cambio de ángulo del
   jugador debe revelar ambos cuerpos. La figura presente puede seguirlo con
   la cabeza; el Echo reproduce exclusivamente lo capturado.
2. **El otro lado de la puerta.** En la bomba se conserva una oportunidad segura
   de comprender ruido, presión y mando. Durante la preparación, una manija
   lateral baja lentamente; alguien prueba el cierre y se detiene. Después,
   un contacto pesado contra la hoja. No pasos en bucle, música permanente ni
   mensajes anunciando un susto. La salida y el quiebre de vista se reconocen
   durante esta preparación; el jugador no descubre el camino bajo ataque.
3. **Irrupción corporal.** La liberación existente abre el paso lateral. Primero
   asoman mano, hombro y parte del rostro, físicamente trabados en el hueco;
   después el cuerpo golpea la hoja y entra violentamente. Un ataque metálico
   seco, una nueva exhalación/grito adulto y fricción de cuerpo tienen causas
   visibles. La pose inicial debe resultar desagradable de cerca: peso vencido,
   boca sin sonrisa, cabeza baja que encuentra al jugador, mano que realmente
   se apoya. La crudeza puede aparecer en el contacto, la piel y una herida
   localizada; no confiar el efecto a decorar el suelo con sangre. Cámara
   libre, señal visual antes del contacto y margen efectivo para retroceder.
4. **Escapar cuesta algo.** Huida corta por el retorno conocido hasta el
   quiebre físico existente. La amenaza invade la distancia, golpea un marco
   y frena con peso; evitar un recorrido largo mostrando una marcha mediocre.
   Tras perder vista, el reinicio auténtico de la bomba por el Echo la atrae.
   El plan grabado sigue siendo útil, pero ahora el jugador depende de él bajo
   presión. Rehacer el sonido de aproximación y contacto por superficie; un
   pulso musical genérico no sustituye la actuación. La captura conserva un
   reintento próximo, sin volver a aprender o grabar todo por rutina.
5. **Hay que salir cerca de él.** Después del desvío, la salida exige pasar
   junto al puesto al que lo atrajo la bomba, con una separación física clara.
   Se ve una mano, espalda o porción de rostro mientras el cuerpo atiende la
   máquina. No cambiar a escondidas las reglas de detección ni matar al cruzar
   un paso obligatorio. El ruido cae, se oye su respiración presente y el
   jugador termina de cruzar por decisión propia. Cierre con distancia y
   recuperación hacia la casa; no otro grito automático detrás de la puerta.

### Condiciones de producción

- El salto artístico depende de **una actuación nueva de la amenaza presente**:
  postura, expresión, apoyos, arranque y frenada. Resolver primero una pose
  cercana y la irrupción. Si el modelo sigue pareciendo un adulto neutro o el
  cuerpo se atraviesa con la puerta, no compensarlo acumulando efectos.
  Evitar alterar el cuerpo grabado del Echo para hacer actuar al intruso.
- El recorrido permite reordenar tabiques, abertura y salida de la bomba.
  Debe haber orientación antes del ataque, un refugio provisional y una salida
  que la presencia haga incómoda. Sin laberinto nuevo, inventario o IA general.
- Reutilizar la lógica real de cobertura, presión, grabación, captura y retorno.
  Mantener el significado de RECALL y los estados de progreso; el evento no
  necesita corromper una toma, fabricar audio grabado ni bloquear R por sorpresa.
  Una sola presencia de bomba, con colocación por fases que no aparezca ante
  la cámara ni deje copias visibles en salas anteriores.
- Sonido de impacto muy presente mediante contraste, ataque y proximidad.
  Separar metal, voz y contactos; retirar ambiente antes del pico y dejar
  respiración/roces después. Conservar volumen cómodo, limitador, controles,
  salida para parlantes/mono y dinámica reducida. No aprobar una voz por pico,
  normalización o duración: la interpretación se elige por escucha humana.
- Sólo Unity Editor. Entregar escena/assets/scripts guardados, con compilación,
  Console y una comprobación focal del tramo modificado; repetir únicamente
  ante una edición relevante o un fallo concreto. Sin builds, nuevas suites,
  recorridos completos, comparación audiovisual exhaustiva ni informe largo.
  Entregar pronto una versión jugable del conjunto para la prueba del usuario.

### Fuentes sonoras seleccionadas, pendientes de escucha humana

Páginas y licencia CC0 verificadas el 2026-10-12. Se descargaron los previews HQ
publicados y se importaron seis cortes mono; procedencia, licencia y edición
exacta en [AUDIO_ASSET_LEDGER.md](AUDIO_ASSET_LEDGER.md). La elección de estas
interpretaciones es una candidata de producción, **no aprobación por escucha humana**.

| Fuente | Uso propuesto |
| --- | --- |
| [JohnsonBrandEditing — human male scream multi, 173944](https://freesound.org/people/JohnsonBrandEditing/sounds/173944/) | Comparar actuaciones masculinas y elegir un arranque de ataque distinto del actual. |
| [aldenroth2 — Male Scream, 272023](https://freesound.org/people/aldenroth2/sounds/272023/) | Alternativa de voz breve; descartar si su timbre resulta cómico o impostado. |
| [NachtmahrTV — Metal impact, 556693](https://freesound.org/people/NachtmahrTV/sounds/556693/) | Peso y resonancia de la hoja golpeada durante la irrupción. |
| [alegemaate — Rattling door, 364682](https://freesound.org/people/alegemaate/sounds/364682/) | Contacto de manija antes del ataque; editar una acción, no repetir un loop. |

Referencia de criterio: [Thomas Grip, 9 Years, 9 Lessons on Horror](https://frictionalgames.com/2019-10-9-years-9-lessons-on-horror/).
Se toma la relación entre agencia, imaginación, credibilidad y miedo; esta
secuencia concreta es una propuesta para Echo Protocol, no un hallazgo probado
por esa referencia. El siguiente juicio humano se limita a tres cosas: reacción
ante el intruso, orientación durante el ataque y tensión al salir junto a él.

## Contraprueba selection — 2026-10-08

P3 now makes the useful recording a planned future encounter. Reception A
establishes a visible, truthful presence and opens the service passage; its blind
then closes. The opaque detour separates Player and Echo. Confirming B stops the
same fixed ventilation/rotor and dims the clinical circuit. Returning through
that known passage leads to a warmer, quieter view of the same recorded adult.
The Player must hold the return control while the second A is actually present.
Success preserves the exit, allowing the encounter to settle without a final race.

The domestic wall, furniture and lamp already exist; lighting and the physical
blind change their presentation. The recorded body now includes explicitly
captured locomotion/head presentation, never invented gestures or audio. Real
camera iterations corrected darkness, obstructed faces, prop orientation,
collision and the campaign connection. This is a selected artistic direction
with objective route evidence. Gait naturalness, close credibility, anticipation,
first-play understanding, listening comfort and fear remain **EN PRUEBA**.
[Comparison, evaluation route and criticism](CONTRAPRUEBA_CANDIDATE.md).

## EncounterArt selection — 2026-10-02

SE2 establishes moving ventilation at the doorway, subtracts it during the
hidden change and lets the Player approach a partially exposed, lit face behind
the leaf. The environment retains the stopped rotor and figure afterward. The
final6B RECALL stops its own fixed ventilation; the Player receives the same
recorded body now close and front-facing. Nothing modifies the real take or
adds an unrecorded channel. These are authored present-world consequences.

The shared matte adult, skin/hair/cloth separation, small facial relief and
local practicals replace cian uniformity. Normal-control views and a complete
campaign were reviewed. This is artistic selection and technical verification;
credibility, spontaneous attribution, tension, comfort and fear remain EN PRUEBA.
[Before/after and delivery scope](ENCOUNTER_ART_CANDIDATE.md).

## H1.1 evaluation gate — 2026-09-12

**2026-09-16 update:** the user defers external playtesting and authorizes
progress by intuition and creator judgment. This supersedes the evaluation-first
prerequisite below; it does not add independent-player evidence. Technical
checks remain required; unobserved human effects stay EN PRUEBA without blocking
development. D1 is now implemented; Final C remains the next content step. See DECISIONS.

Approved direction: 20–25 polished minutes with one exact Echo; retain P1/P2/P3
and observe new players through R3 before developing truthful recorded evidence
of denial followed by Final C. No new event is implemented by this approval.
Fear, comprehension, R1 recovery, R3 recognition/interpretation and continuous
pacing remain EN PRUEBA. Use `Evaluation/H1.1/OBSERVER.md` for the unprimed pass;
earlier guided procedures remain technical/creator regressions.

## D1 denial-evidence implementation — 2026-09-16

Authorized implementation: [Del otro lado](DENIAL_SCENE.md). After R3, the Player records
a real pause at the first observation glass and uses normal R to watch it from
the other side. “No me detuve ahí” is contradicted by the actual recorded pause;
body orientation creates the frontal composition without claiming eye tracking
or intent. The exact same Echo becomes an uncomfortable presence through spatial
proximity and environmental subtraction. The creator authorized this staging
with “Hacela”; 74 automated route/reset checks pass. Fear, interpretation
and pacing remain EN PRUEBA for creator judgment. Accident responsibility and
Final C remain outside this scene.

## Current stage

The mechanical trust foundation is complete through M5.2. M6.1–M6.3, AD2, and
AV1 prove that the project can reveal a persistent object, subtract ambience,
play truthful spatial mechanism audio, deliver a one-shot cue, and present a
closed clinical room without changing Echo playback or puzzle rules.

Human evidence is narrower. The hum transition, causal door audio, integrated
AD2 palette, and AV1 place are approved for continued prototyping. The chair was
noticed and its one-shot delivery worked, but neither created terror or
discomfort. The chair therefore remains a secondary environmental detail or
motif anchor, not a headline scare and not evidence that the horror direction
works.

The previous `AD3` plan to stretch that room into a 5–8 minute chair-centred
route is superseded before implementation. `SD1` is now frozen as a partial
technical sandbox: it proved authored triggers, moving spatial audio, routing,
and a provisional level, but not a complete fear sequence. The current `EM1`
milestone proposes the five-chapter pressure map and one reusable 8–10 minute
production slice in `EXPERIENCE_MAP.md`; its specifics await human approval.

## Current slice checkpoint — R1 recovery

The creator validated SE2 and SE3 in the R1 brief: approval covers direction and
slice legibility, with no new claim about fear, narrative or audio. R1 is an
implemented and objectively verified clinical recovery room, **EN PRUEBA** for
its intended human effect.
After the domestic overlap, a two-way side aperture leads into a wider, evenly
lit observation room with a central desk. Domesticity remains a static distant
frame and runner behind sealed glass. No new presence, reveal, audio, blackout,
puzzle or checkpoint follows. The Player can stop, orient and return freely to
the known return; the original P2 gate keeps its rule.

The human pass must judge whether the change in width, light and composition
actually lowers intensity, whether the room is memorable and its return clear,
and whether the distant warm detail stays residual rather than suggesting an
immediate new event. Captures and state tests cannot approve recovery. See
`VisualEvidence/2026-09-09-R1/README.md`.

## Approved horror thesis

Echoes remember what happened correctly; the protagonist does not. Apparent
unreliability belongs to memory, perception, omitted cycles, or nightmare logic,
never arbitrary playback corruption.

The rule is now confirmed as absolute for recorded data: an Echo never fabricates
or alters a captured frame or supported action. It is not omniscient. Contraprueba adds explicitly captured body-presentation
channels to root movement/yaw; preserved older prototypes retain their original
channels. New semantic or audiovisual channels require explicit implementation
and tests. A later omitted cycle may be
authored as something the protagonist lived but did not retain, provided the
recording itself remains ground truth rather than a changed copy of a known run.

Horror must happen perceptually to the Player, not only symbolically to the
protagonist. An ordinary chair with narrative significance is still just a chair
unless its setup, timing, sensory treatment, and consequence change how the
Player behaves or interprets the space.

The target is not constant noise or a catalogue of random jumpscares. The game
builds a reliable world, teaches recurring motifs, varies them deliberately,
forces uncertain checks, sometimes leaves evidence, and uses several authored
strong events within the approved 20–25 minute delivery. Direction, proximity, movement,
attack, spectral contrast, subtraction, and recovery create force; unsafe
loudness does not.

Sound is the main weapon, not the only one. Level composition, lighting,
materials, reflections, partial human evidence, changed routes, and narrative
context provide checks and consequences. The Echo can remain visually and
mechanically identical while a later environment makes its exact path newly
disturbing.

## Trust contract

- An Echo exactly reproduces a completed recording and ends predictably.
- Reset returns the active experiment to a known baseline.
- Input, playback speed, plate truth, door timing, and goal eligibility do not
  change secretly for an otherwise identical attempt.
- Functional sound remains causally truthful. Horror audio cannot impersonate a
  working mechanism in a way that invalidates a learned puzzle rule.
- Essential puzzle and narrative information must remain available without
  hearing alone.

The perceived arc is `useful Echo → apparently unreliable Echo → feared Echo`.
The implementation remains deterministic while the Player gains reasons to
distrust the protagonist's account of what was recorded.

## Player-facing fear resources

### Spatial doubt

Most physical sounds are `world-locked`: they occupy an authored place and move
correctly between the ears when the Player turns. This makes the virtual room
learnable and lets a dry domestic knock, footsteps behind the Player, or breath
near a shoulder feel spatially credible.

Rare `head-locked/internal` events may remain close to one ear when the view
turns. They are reserved for intimate perception, never normal world physics and
never essential information. The exact binaural/HRTF implementation is TBD and
must be tested rather than assumed.

The desired trans-screen effect is that the Player briefly checks a real door or
corner, turns toward the room behind them, removes one earcup to verify the
source, or cannot immediately decide whether a sound came from the game. These
are possible human observations, not instructions to raise volume.

### Recurring motif families

| Family | Examples | Horror function |
| --- | --- | --- |
| Facility / structure | metal impacts, ducts, machinery, pressure, glass, mechanism strain | Establish a coherent building, then imply activity outside the reachable route |
| Presence / body | extra footsteps, breathing, fabric, fingers, weight behind a door, brief running | Suggest proximity without confirming an enemy |
| Domestic memory | household door, chair scrape, dishes, water, child melody, toy, accident fragments | Let home invade the institution without immediate exposition |
| Time / repetition | the Player's steps later, a prior action in another room, sound following an Echo path and continuing | Connect correct recording to omitted or misremembered experience |

Each motif may receive authored variants in distance, side, elevation, duration,
dryness or reverberation, restraint or violence, and partial or complete form.
Variation follows scene state and dramatic intent; it is not a random scare
director. Truthful door, plate, and RECALL feedback keeps its verified causal
identity.

### Check and consequence

A useful pattern is:

`hear → locate → check → find nothing → continue → receive partial evidence`

Not every check pays off. Some produce nothing; others later reveal a mark,
object, route change, remembered fragment, or contradictory but mechanically
truthful trace. If checks never matter, the Player learns to ignore audio. If
every sound yields an object, the pattern becomes a vending machine. The
consequence schedule must be authored and remains TBD.

### Puzzles as vulnerability

Puzzles should expose the Player instead of pausing the horror. A situation may
require watching a terminal with the room behind them, waiting while a sound
approaches, leaving the Echo out of sight, crossing a known corridor again,
holding position to listen, or pressing RECALL despite anticipating what will
return. Difficulty still comes from understanding and planning, not hidden rules
or frame-tight execution.

No production puzzle is added merely to fill minutes. Its mechanic, spatial
exposure, sensory beat, and recovery role must be defined together.

## Planned five-chapter escalation

This direction is approved but only Strong Event 1 has an implemented prototype.
Exact later durations, rooms, puzzles, and event counts remain TBD.

1. `Protocol` builds safety: clinical causality, useful RECALL, and the first
   uneasy impact without breaking trust.
2. `Familiarity` creates spatial doubt: learned spaces produce directional
   checks, small consequences, and domestic sounds that do not belong.
3. `Recollection` lets home and presence invade more directly while the Echo
   continues to reproduce the truth.
4. `Denial` sustains pressure through omitted-cycle evidence, stronger recurring
   variants, and authored set pieces that contradict the protagonist's account.
5. `Acceptance` merges facility and memory, reconstructs the traumas, and ends
   with Final C: confrontation, the nightmare ending, and a cut to black without
   showing whether the protagonist wakes.

For roughly 70–80 percent of the experience, physical danger need not be
confirmed. This does not require low intensity: directional pursuit without an
enemy, domestic intrusion, strong impacts, and sustained presence can create
pressure while combat and generic chase horror remain absent.

## Implemented Strong Event 1 checkpoint

The VS1 `Occupied Lane` prototype follows
`normality → concentration → domestic contradiction → check → contraction →
warm invitation → commitment`. It cannot begin before P1 completes or while the
Echo is active. The dry knock prompts a look into the empty lane; after that look,
three fixture failures travel toward the Player while facility light and ambience
subtract. A slow domestic opening becomes the only warm route. Crossing it
commits the Player to the insert, then a delayed portazo closes the known space
behind them and moves future RECALLs inside the room. Exact Echo data and puzzle
truth remain unchanged.

The integrated first checkpoint was human-approved on September 3; production
timbre and separate fear claims remain open. Broader follow-up can assess a natural threshold commitment, clear
behind-the-Player attribution, surprise without physical discomfort, and an
immediately understandable new RECALL origin. Mere detection does not pass the
fear gate.

## Frozen study: SD1

`SD1` was planned as approximately five minutes in the existing space with no
new puzzle:

1. establish the stable facility baseline;
2. place one dry domestic knock behind-right, distinct from the game door;
3. move authored footsteps from left to right and behind the Player;
4. place world-locked breathing whose location remains correct when turning;
5. use one rare, intelligible head-locked whisper near a single ear;
6. give one cue a subtle, delayed visual consequence;
7. leave recovery instead of stacking another event.

The sequence and content are approved as a study brief, not implemented
features. The whisper is `Can you hear me?`, spoken in English and subtitled
`¿Me escuchás?` in Spanish. Its identity, performer, treatment, intended ear,
clip, and trigger remain TBD. The consequence must be subtle and mechanically
inert; its exact form, emitters, spatial technology, mix levels, and timings
remain TBD. The current chair may be present but is not the study's climax.

Objective checks must preserve Echo data, reset, puzzle truth, and causal audio;
differentiate world-locked from head-locked behavior; avoid duplicate triggers;
retain headroom; and exercise headphones, speakers, mono, and a reduced-dynamics
presentation.

The human checkpoint is unannounced beyond a general psychological-horror and
sudden-audio warning. Record spontaneous turns, checks of doors or corners,
source doubt, emotional tension, mechanical trust, and physical comfort. A
guided delivery pass cannot validate surprise or trans-screen fear.

## Safety and accessibility

- Recommended copy: `Designed for headphones. Set a comfortable volume; details
  do not require high volume.`
- Plan `Headphones` and `Speakers` presentations, left/right calibration,
  separate ambience/effects/voice controls, and `Reduced Dynamics`.
- Never instruct the Player to use high volume. A scare that works only by
  raising level fails.
- Stop testing for pain, tinnitus, blocked-ear sensation, dizziness, panic, or
  other physical distress. Emotional discomfort is intended; harm is not.
- Spatialized audio must have non-audio support for essential information.

## Historical campaign expansion — 2026-09-30 (subsequent production below supersedes roster)

After D1, pressure asks the Player to watch a valve for six seconds while an Echo
maintains a distant chamber; machinery loses masking layers and contact is heard
behind the Player. The house asks for investigation and a return through familiar
rooms. After the bedroom circuit, a physical adult silhouette occupies the hall;
it is backlit, close, solid, and resolves after sustained unobstructed sight with
a quiet movement cue behind the Player. The HUD clears during that encounter.
This is an environmental presence, not an altered Echo take.

The gallery requires three real simultaneous positions across observation glass.
Each success removes a light and opens a physical shutter; opaque panels deny
continuous sight while the recorded path stays exact. Archive closure distinguishes
the current take from the historical statements in four documents. A. Vera and
Mara are the campaign's written characters; no voice or historical audio is claimed.

The full physical route and presence conditions are objectively verified. Fear,
spontaneous attribution and pacing remain unmeasured. These events do not inherit
human approval from the earlier H1.1 slice.

## Implemented discrepancies and open questions

- Echo anomalies: none.
- Environmental proofs: the M6.1 chair reveal persists for the session. The
  first VS1-SE2 side-window figure failed the basic human detection test and is
  preserved disabled; technical delivery did not make it a successful scare.
- Audio proofs: facility-air subtraction, truthful door/plate/RECALL cues, and
  the one-shot M6.3 chair proximity sound.
- Implemented VS1-SE2 replacement: a cold, empty service chamber directly
  ahead after the P2 turn becomes a deep amber domestic interior with a readable
  stationary human silhouette. Live Echo-held P2 authorizes the crossing;
  `0.65 s` of clear natural view reveals the room and `3.2 s` of actual visible
  reading completes it. The warm space and figure then persist through RECALL;
  incomplete RECALL restores a clean cold baseline. The change is environmental,
  adds no audio, and preserves exact Echo playback and free camera/control.
  Objective route, gating, visibility-time and RECALL checks pass from a prepared
  domestic checkpoint; the full P1/SE1 route was not repeated. The later creator pass approves
  direction and slice legibility, without establishing fear or narrative meaning.
- Implemented VS1-SE3 Omitted Passage (optional): the Player
  turns back from completed SE2 and finds four nested domestic doorframes,
  worn rug, skirting, partial curtain and an unlit console lamp occupying the
  same short clinical corridor. The clinical ceiling and walls remain readable;
  amber light marks the far end. The switch occurs at the approach to the turn;
  oblique views can catch a domestic fragment at the edge before the full
  southward composition. Judge that transition in the human pass.
  There is no new figure, sound, forced camera, teleport or puzzle. Only the
  actual Player crossing west after SE2 `Complete` reveals it. RECALL preserves
  a revealed passage; an incomplete SE2 keeps the clinical baseline. This is
  a study of superposed space with human-approved direction and slice legibility,
  not evidence of fear or approved final content.
  Input-driven complete/incomplete returns, RECALL and two-way navigation are
  objectively verified from the prepared domestic checkpoint.
- Emotionally approved headline scares: none.

The exact historical incident, facility's ultimate reality, any future spoken
performance and measured chapter durations remain TBD. The implemented campaign
now has a concrete event/puzzle roster and clinical/domestic visual palette. The chair's approach/departure timing is no longer a
current milestone; retain or replace it only if a later motif needs it.

## Current production route — 2026-10-01

Records retains the first-act spatial events and changes the long R3 return into
a short traverse of the paper archive. The basement uses physical isolation and
autonomous drainage, replacing the repeated pressure Echo exercise. The compact
house retains optional answering/light choices; its presence can activate after
key pickup independently of those choices. Departure requires the key, never a
forced look at the figure. Discovery on a spontaneous return remains unproven;
the existing continuous probe explicitly aims at it and cannot prove discovery.
Final6B now asks the Player to open from inside and directly receive the genuinely
recorded arrival. These changes preserve the data contract and free camera.
Their delivery is objectively checked; emotionally approved headline scares remain
none. This production goal authorizes iteration without inventing human evidence.

## Study — oportunidad de descubrimiento, 2026-10-01

El vidrio sellado más amplio y la figura ambiental más próxima entran ahora en
la vuelta recta desde el dormitorio. Dos ramas por input, sin Aim a la figura,
muestran exposición geométrica de cabeza/centro durante unos0,64s a marcha plena.
El peso de tabla existente empieza unos0,095s antes. Cámaras verifican encuadre y
proximidad en casa atendida/no atendida; no equivalen a reconocimiento o miedo.
La llave sigue bastando para salir; la mirada no es un gate. Se conserva la luz
residual y se retiran sólo sombras remotas de la malla ambiental para no revelar
una aparición fuera del volumen vigilado. Escucha, descubrimiento espontáneo y
respuesta emocional siguen EN PRUEBA. Ver evidencia Study para fixtures y límites.

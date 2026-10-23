# Ronda 4 — cara y actuación del intruso

2026-10-23. Dirección autorizada: tres chats Astra / Ultra, sólo Editor,
sin builds y con comprobaciones puntuales. Base jugable `dbe1c6f0`, rama
`codex/integracion-20261023`; ronda 3 integrada y escena Incursion privada
guardada. No hay nueva prueba humana de esa ronda. Miedo sigue EN PRUEBA.

Coordinador: «Evalúa la próxima iteración de Echo», chat
`01a12cb5-6030-77a1-b02b-93734f7c279b`, proyecto original
`/Users/lautigr116/EchoProtocol`. La integración final se hace allí, en el único
Editor. Cada implementación trabaja en su propio worktree y entrega commits.

## Evidencia y dirección

Inspección de la cámara central: P3 llega al vidrio, pero el rostro parece
una máscara, las manos casi simétricas sugieren una pose y la iluminación
aplana algunas superficies. Es juicio de dirección sobre capturas, no nueva
evidencia humana de miedo. El objetivo es un cambio reconocible del personaje
presente en P3 y bomba, sin extender la campaña ni tocar el cuerpo grabado.

Leer README, hito actual de ROADMAP, AGENTS y documentos relevantes de diseño.
Usar como evidencia de partida, sin repetir las rutas:

- `Builds/round3-integration-20261023/p3-contact-corrected.png` en el original.
- `Docs/P3_INTRUSION_ITERATION.md`, `Docs/PARALLEL_HORROR_ROUND3_20261023.md`.
- `Assets/Editor/EmbestidaProduction.cs`, shader `EmbestidaSurface` y fuentes
  privadas existentes del adulto Nathan. No ejecutar el generador histórico
  completo: mezcla modelo, escena, sonidos y otros sistemas.

## A — cara que sostenga la mirada

Rehacer la lectura cercana del rostro del intruso: volumen de mejillas y boca,
ojos/párpados, tensión y asimetría controlada, piel y coherencia del cuello.
Debe seguir siendo un adulto físicamente creíble y desagradable de mirar;
evitar cara plana, sonrisa involuntaria, ojos de juguete, deformación estirada
o manchas que sustituyan anatomía. Puede corregir la geometría derivada actual
y su sombreado; no basta cambiar un color. Conservar silueta corporal, huesos,
pesos, nombres y proporciones de alcance que consume la actuación.

Propiedad exclusiva: shader `Assets/_Game/Shaders/EmbestidaSurface.shader`,
nuevos assets/componentes de autoría `IntruderFace`,
`Assets/Editor/IntruderFaceIntegration.cs`, `Docs/INTRUDER_FACE_ITERATION.md`.
Derivados del scan sólo en la carpeta privada ya ignorada, sin commitearlos.
Crear una variante de material si el shader afecta otros cuerpos; el Echo,
SE2 y figuras de casa quedan intactos. Preservar original y rollback.

Este frente es de geometría/material, no de pose dinámica. No añadir Update,
LateUpdate, Animator ni otro escritor de mandíbula, cabeza o huesos. Si hacen
falta blendshapes, entregar nombres/rangos y defaults neutros al coordinador;
no imponer dependencia al chat B mientras ambos trabajan.

## B — peso, intención y violencia

Reautorizar la actuación del mismo intruso, especialmente P3 y la irrupción:
quietud que observa, carga de peso, pérdida rápida de distancia, contacto con
corrección de apoyo y una quietud posterior dirigida al jugador. Evitar manos
en espejo, cabeza que oscila por seno, boca permanentemente abierta y ciclo de
carrera genérico. Cabeza/ojos pueden anticipar o retrasar al torso con intención,
sin temblor aleatorio ni seguimiento mecánico constante. Buscar dos o tres
poses fuertes conectadas por movimiento convincente, no más gestos pequeños.

Propiedad exclusiva: `Campaign/MachineThreatPerformance.cs`,
`Campaign/P3Intrusion/P3BodyIntrusion.cs`, `Campaign/EmbestidaRush.cs`,
`Assets/Editor/P3IntrusionIntegration.cs`, nuevos componentes `IntruderActing`,
`Assets/Editor/IntruderActingIntegration.cs`, `Docs/INTRUDER_ACTING_ITERATION.md`.
Dentro de EmbestidaRush, limitar cambios a actuación y observables mínimos;
conservar fuentes/mezcla y contrato físico de Advance/reacción/captura.

Este frente es el único dueño de articulaciones en movimiento, incluidas cara,
mandíbula y mirada. Conservar CharacterController, colisión real, apoyos dentro
del alcance, pausa y transferencia P3→bomba sin duplicar actor. Puede cambiar
tiempos locales de actuación sin endurecer ventanas de puzzle o captura. No
modificar MachineCoverThreat, grabación, rig compartido ni lógica del relay.
No editar IntruderStaging: cualquier pequeño enlace necesario se propone al
coordinador con firma y ubicación precisas.

## C — presencia sonora y revelación

Rehacer cómo se percibe ese cuerpo en P3 y bomba: iluminación local que modele
ojos/manos/rostro sin ocultarlos, silencio y contraste antes del contacto,
material sonoro con peso y esfuerzo/respiración que pertenezcan a la acción.
La presentación debe ayudar a localizar y reaccionar sin mover la cámara.
Eliminar capas que compitan o resten credibilidad. No tratar un grito viejo
con más pitch/volumen como nueva interpretación; puede retirarse si empeora el
momento. Reutilizar primero grabaciones licenciadas; recursos nuevos pequeños
sólo con procedencia/licencia. No generar voces con ElevenLabs: ese flujo pide
casting humano específico y esta ronda puede avanzar sin él.

Propiedad exclusiva: `Audio/MachineCoverPresence.cs`, sólo presentación y audio
de `Campaign/IntruderStaging.cs`, nuevos assets/componentes `IntruderPresence`,
`Assets/Editor/IntruderPresenceIntegration.cs`,
`Docs/INTRUDER_PRESENCE_ITERATION.md`. Las luces modificables son las locales
del intruso/recepción/bomba; sin exposición, postproceso o mezcla global nuevos.
Puede configurar fuentes existentes de P3BodyIntrusion/EmbestidaRush mediante
el integrador, pero sus llamadas/tiempos de Play pertenecen al dueño original.
No crear una segunda reproducción del mismo golpe ni editar los archivos de B.

Conservar WorldSFX, volumen cómodo, mono/parlantes/dinámica reducida, pausa y
fidelidad del Echo. No audio de amenaza atribuido a la reproducción ni nuevas
persecuciones. Sin cambiar colocación, activación o retiro del actor en Staging.

## Contrato común y entrega rápida

- Un worktree por chat; no escribir el original ni los otros worktrees. No push,
  merge, builds, publicación, nuevos Editors/Libraries o copias de modelos.
  Hay poco espacio local: lectura de recursos privados desde el original,
  implementaciones pequeñas; no descargar paquetes ni hacer checkout de LFS
  completo. Las carpetas excluidas por sparse-checkout se leen en el original.
- Sólo el coordinador modifica la escena abierta mediante Unity. Los workers
  entregan integradores idempotentes con preflight, rollback y referencias
  explícitas. No abrir/guardar escenas automáticamente ni editar YAML. La
  falta de Editor propio no autoriza usar el central: reportar qué debe aplicar
  el coordinador, sin afirmar compilación/Play no realizados.
- No tocar README, ROADMAP, este plan, opciones, .gitignore, paquetes, ajustes
  globales, escena privada o documentos de otro frente. Ningún cambio rompe
  compilación por depender de un archivo todavía inexistente del otro chat.
- Un subagente como máximo por chat si acelera una tarea disjunta concreta;
  no cadenas de delegación ni auditorías generales. Implementar pronto y dejar
  la crítica focal al lado del trabajo, no como larga fase previa.
- Verificación propia: diff/referencias y coherencia de la modificación. Central:
  compilación/Console y una presentación breve en P3 + entrada de bomba si se
  modifica, con pausa/handoff sólo si hace falta. Repetir únicamente por un
  cambio relevante o fallo concreto. Sin campañas, suites, nuevo harness ni
  comparación exhaustiva. Primero entregar algo perceptible al playtest humano.
- Resultado: commits, operación exacta de integración, cambios visibles,
  limitaciones honestas y hasta dos dudas humanas. No certificar miedo por
  capturas o contadores. No ampliar alcance para llenar tiempo o consumir cuota.

## Chats iniciados

Los tres se crearon desde `d50783f4`, con modelo `gpt-6-astra` y esfuerzo
`ultra`, y trabajaron en paralelo. Implementación **INTEGRADA en Editor**:

- A, «Intruso: una cara que sostenga la mirada»:
  `01a16a1f-c030-7850-89c4-c1af1f261a6a`, worktree `b772`.
- B, «Intruso: peso, intención y violencia»:
  `01a16a20-1d4f-7892-ab9f-4558660ad5c9`, worktree `bf8a`.
- C, «Intruso: presencia sonora y revelación»:
  `01a16a20-1d4f-7892-ab9f-454dbe964653`, worktree `4134`.

Se excluyeron ArtSource/ y Docs/VisualEvidence/ de esas copias mediante
sparse-checkout; siguen completas en el original. Los tres recibieron ese
aviso y el límite de un único Editor central. La ronda 3 queda recuperable;
Incursion ahora contiene la ronda 4 guardada. Sin builds ni publicación.

## Integración central — 2026-10-23

A, C y B se integraron en `d034e215`, `135d3a2f` y `86764fce`.
`eab163cf` corrige el preflight de cara: acceso Editor a meshes con Read/Write
runtime desactivado y bounds derivados por `updateWhenOffscreen`. No cambia
esos ajustes. Tras la primera vista, se pidieron dos correcciones acotadas:
expresión menos sonriente (`571544e5`) y quietud cargada hacia el vidrio
(`67dbce9a`). El mesh privado se regeneró en el mismo asset, conservando GUID.

- Compilación conjunta completada sin errores; Console sin errores nuevos desde
  cursor 1362 hasta 1390. Los tres integradores se aplicaron en el único Editor.
- P3 se comprobó con relay preparado y toma estacionaria sintética, no mediante
  una resolución natural del puzzle. Un contacto, ambas muñecas <0,00001 m del
  target, izquierda retirada y derecha retenida. La repetición final, justificada
  por la corrección, dio `RightPalmError=0,00000096 m`,
  `MaxSupportError=0,00000383 m`, sin bloqueo. Frames 85→1597→1885.
- Handoff al preview de bomba: P3 dejó de poseer el mismo actor. Entrada real
  desde preparación directa de Release: un contacto, sin aborto, palma a
  0,000173 m; recuperación observada hasta 0,88 s. No recorrido de persecución.
  Los pies coincidieron con sus targets en ese instante; `MaxPlantDrift=0,263 m`
  es acumulado y puede incluir estados previos/traslado. No certifica ausencia
  general de deslizamiento.
- Luz/manos y poses revisadas desde cámara de jugador. Evidencia local en
  `Builds/round4-integration-20261023/`: `p3-settled-corrected.png` final;
  `pump-contact.png` y `pump-recovery.png` anteriores al ajuste estático de cara.
  La bomba no se repitió porque su actuación no volvió a cambiar.

**Juicio del coordinador, no aprobación humana:** perdió la máscara deformada y
la pose de dos palmas permanentes, pero la expresión sigue demasiado cotidiana.
Credibilidad mejorada no equivale a miedo logrado. No hubo escucha subjetiva ni
validación nueva de comodidad/opciones. Para el usuario: juzgar si mirada y carga
provocan retirada, y si contacto/recuperación de bomba tienen peso a volumen cómodo.

Escena privada guardada fuera de Play, con respaldo antes/después y derivados
faciales en `EchoProtocol-backups/parallel-round4-20261023/`. No se publican esos
recursos licenciados. Sin nuevos Editors, builds, pushes ni helpers de captura activos.

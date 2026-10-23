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

Estado: dirección aprobada; chats por iniciar. Implementación de esta ronda
todavía pendiente. El resultado de la ronda 3 sigue disponible en el Editor.

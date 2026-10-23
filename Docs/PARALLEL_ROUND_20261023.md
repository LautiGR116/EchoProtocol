# Integración y ronda paralela — 2026-10-23

## Base integrada (Editor)

Rama local `codex/integracion-20261023`. Bomba `c8597a7c`; casa
`9e69cdae`, `c59cca6b`, `e5d17b0f`; interfaz `fc9db36d`, `d25fd882`,
`7cbd3edb`. Cherry-picks sin conflictos. Sin push, build ni nueva publicación.

Incursion privada conserva la escena de bomba y recibe la casa mediante
`HouseOccupiedIntegration.ValidateCurrentScene/ApplyToCurrentScene`; guardada
con Unity, sin reemplazarla por una escena de otro worktree. Respaldo externo
antes/después: `../EchoProtocol-backups/integration-20261023-142806/`.
Las escenas y modelos privados siguen fuera del repositorio público.

Comprobaciones de integración: compilación/arranque de Incursion sin errores del
juego; menú inspeccionado a 1280×720; callback de acceso a bomba llega a Pressure
con Player y Loop activos, pausa congela tiempo y audio. En fixture aislada de
casa, llave/retorno disparan contacto; lectura congela tiempo y cues. Cierre de
documento iniciado durante juego solicita reanudación y restaura el control en
el siguiente frame. Se distinguió el cierre de un documento abierto ya en pausa
(permanece pausado, por diseño). No se simuló aquí mantener teclas físicas.
Hubo errores del transporte/herramienta (`editor status` de otra versión y una
conexión durante domain reload), sin errores del juego asociados. Play detenido.
No nueva campaña completa ni juicio automático de miedo, escucha o actuación.

## Próxima ronda aprobada: tres frentes separados

Cada chat usa Astra / Ultra; Fast lo activa el usuario en el selector. Deben
partir del commit que contiene este documento, en worktrees nuevos, sin publicar.
El coordinador es «Evalúa la próxima iteración de Echo», thread
`01a12cb5-6030-77a1-b02b-93734f7c279b`.

| Frente | Resultado visible buscado | Archivos propios |
| --- | --- | --- |
| P2: comprender el cierre | La disposición y los estados físicos permiten entender que mirar sostiene la puerta y apartarse libera la segunda; preparar RECALL con intención | `Presentation/P2AirlockPresentation.cs`; `Puzzle/BlindAlignmentRelay.cs` sólo si hace falta exponer estado o corregir fallo; nuevos assets/integrador exclusivos P2 |
| SE2: presencia al otro lado | Composición, postura/contacto y respuesta audiovisual del adulto detrás del vidrio generan impacto y sostienen incomodidad, sin otra persecución | `Horror/DomesticPresence.cs`; nuevos assets/integrador exclusivos SE2 |
| Jugador: contacto e interacción | Corregir dos penetraciones/focos reproducibles y dar peso/material a pasos propios, conservando control | `Player/PlayerEchoClearance.cs`, `Interaction/PlayerInteractor.cs`, `Interaction/InteractionSurface.cs`, `Audio/PlayerFootsteps.cs`; nuevos assets/integrador exclusivos Player |

No tocar bomba, casa, interfaz, mezcla global, rigs ni reproducción/registro del
Echo, escenas compartidas, ProjectSettings o Packages. No editar `VS1ParallelFigureEvent`,
`OpticalReader` ni `SlidingDoor`. No cambiar sensibilidad 0.15 ni controles.
Una dependencia fuera del alcance se informa al coordinador, no se edita en secreto.
P2 y SE2 mantienen sus volúmenes/gates existentes; los assets nuevos tienen prefijos
y carpetas diferentes. La regla del Echo y las condiciones del puzzle se preservan.

## Coordinación y entrega rápida

- Un worktree y una rama por chat, commits propios; nunca merge/push a main.
- Subagentes autorizados: hasta dos por chat, responsabilidades disjuntas y una
  revisión crítica breve. No multiplicar auditorías ni planes antes de implementar.
- Disco limitado: **no abrir tres Editors ni importar tres Library**. Los chats
  trabajan fuentes/assets y entregan operaciones de integración de Editor
  aditivas, idempotentes, con validación y Undo; nunca YAML escrito a mano.
  Pueden leer la escena/recursos privados originales, no modificarlos.
- El coordinador ejecuta compilación y la comprobación mínima de cada frente en
  el único Editor del proyecto principal, después de integrar commits. No afirmar
  verificación en Editor desde los worktrees cuando no se realizó.
- Los nuevos integradores no abren/sustituyen/guardan escenas, ni tocan capítulos
  ajenos. Trabajan exclusivamente sobre los objetos validados de su frente.
- Documentación: un documento corto por frente (`P2_CAUSAL_ITERATION.md`,
  `SE2_PRESENCE_ITERATION.md`, `PLAYER_CONTACT_ITERATION.md`); no editar este plan,
  README ni ROADMAP en paralelo. Entregar commits, archivos, operación de aplicación,
  comprobaciones realmente realizadas y dos puntos para prueba humana.
- Miedo, actuación, comodidad y comprensión espontánea siguen EN PRUEBA humana.

## Chats iniciados

Verificados activos con modelo `gpt-6-astra` y effort `ultra`, todos desde
`5277b28db7d1984178896d4c6487af16996b85e3`. Fast queda a cargo del usuario,
quien confirmó que lo activará; no se afirma activación desde la herramienta.

- P2: `01a169d7-bd82-7c63-9b08-15a2dffb0d18`, worktree `2c5b`.
- SE2: `01a169d8-04e7-7041-ae52-a689324e0a49`, worktree `58ca`.
- Player: `01a169d8-4ea1-7e23-8fc7-36635f143170`, worktree `94f3`.

## Segunda ronda integrada en el Editor

Los tres chats terminaron. Cherry-picks sin conflictos: P2 `06b96fcd`, SE2
`bc4522ef`, Player `1e5496bb`. Se aplicaron sus tres integradores sobre la
Incursion original y se guardó con Unity. Revisión de cámara añadió una luz
local a la primera hoja de P2 (`a23e91eb`), que quedaba demasiado oscura junto
al panel. También se corrigió el GUID de 33 caracteres del nuevo folder P2Causal,
detectado en Console. No se sustituyeron escenas ni se abrieron otros Editors.

Comprobaciones centrales puntuales, mediante estados preparados en Play:

- P2: mirar alimenta/abre; girar en el puesto conserva alimentación, cierra la
  primera hoja y autoriza la segunda; abandonar el puesto cancela autorización.
  Cámara confirma panel y hoja legibles. No se repitió el recorrido con RECALL.
- SE2: revelación con volúmenes fuera de cámara; un descubrimiento y una cesión
  de mano, sin repetición al pausar/reanudar o volver a mirar. Frente y oblicuo
  inspeccionados. La entrada fue preparada, no un recorrido natural completo.
- Player: junto a una pared temporal, separación lateral sin mover el Echo ni
  generar pasos por el empuje. Apoyos en clínico/madera, fuentes alternadas y
  detenidas en pausa. El teléfono descolgado recibe foco en su posición real;
  una caja interpuesta quita foco y bloquea activación. Fixtures retiradas.

Compilación del proyecto completada. Los intentos de inyección de teclado no
produjeron movimiento; para los apoyos se usó desplazamiento acotado del
CharacterController, sin presentar eso como una prueba de controles físicos.
Sin campaña, nueva build, push ni publicación. Play detenido; escena guardada.
Respaldo privado antes/después: `../EchoProtocol-backups/parallel-round2-20261023/`.
Capturas locales: `Builds/round2-integration-20261023/` (sólo imágenes).
Comprensión espontánea, miedo, actuación, timbre y comodidad siguen **EN PRUEBA**.

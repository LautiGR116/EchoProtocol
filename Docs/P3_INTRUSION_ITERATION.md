# P3 — el otro cuerpo invade la distancia · 2026-10-23

**Integrado en Incursion; comprobación focal en Editor central realizada.
Miedo, actuación y escucha EN PRUEBA humana.** Base `4e240f2f`, rama
`codex/p3-body-intrusion`. Sin Editor, Library, build, push ni publicación aquí.

El mismo intruso se reconoce junto al Echo real durante el retorno a A. Tras
0,32 s de visibilidad con el Echo realmente alineado, comprime el cuerpo durante
0,12 s y recorre aproximadamente 1,15 m en 0,46 s. Un pie adelanta y el otro
recoge el peso; ambas manos llegan al vidrio existente, el tronco cede y queda
inmóvil. No espera el final de la toma ni exige sostener más tiempo el mando.
Se retira el roce de descubrimiento. Un apoyo de suela y un contacto seco del
marco reemplazan ese gesto; sin voz, música, bucle ni modificación de mezcla.

## Integración exacta

1. Aplicar el commit de esta rama sobre la integración central y dejar importar
   los scripts. Usar la Incursion privada **ya abierta**, fuera de Play.
2. Menú `Echo Protocol > P3 intrusion > 1. Validate current scene`.
3. Menú `Echo Protocol > P3 intrusion > 2. Apply to current scene`
   (`P3IntrusionIntegration.ApplyToCurrentScene()`). Repetir es un no-op si la
   instalación sigue íntegra; una instalación parcial/alterada se rechaza.
4. La escena queda **sin guardar** para revisión central. Undo revierte toda
   la instalación; una excepción ejecuta rollback. No se crean materiales ni
   assets de rig. Guardar explícitamente después del check central.

Se agrega `P3Intrusion / glass arrest` junto al stand existente, con componente,
targets y dos fuentes propias. Sólo se enlaza el campo opcional `p3Intrusion` de
`IntruderStaging`. Los otros capítulos/escenas conservan el comportamiento previo.
No duplicar Nathan ni agregar otro collider: `A sealed glass` ya es sólido.

## Autoridad y límite físico

- `IntruderStaging` continúa siendo el único dueño de aparición/retiro. Su rama
  P3 llama **o** a `P3BodyIntrusion.Present` **o** al Witness anterior. El nuevo
  componente no tiene Update/LateUpdate. `MachineThreatPerformance`, amenaza,
  Embestida, Echo, Resonance y reglas del relay no se modifican.
- `ResetPump`, la activación de bomba incluso pausada y el retiro/desactivación
  liberan la pose y detienen las dos fuentes. La liberación restaura articulaciones
  y apoyos de la performance existente, **sin restaurar la posición mundial**:
  `MachineCoverThreat.ResetThreat` acaba de colocar allí el cuerpo de bomba.
- Sólo se mueve el CharacterController existente; nunca se desactiva su colisión
  durante el avance. Una obstrucción detiene la aproximación; no produce un golpe
  ficticio. El sonido exige raíz próxima al stop y manos que alcancen sus targets.
  No hay daño, captura, persecución ni movimiento de cámara nuevos.
- RECALL no rearma la irrupción consumida; la persona queda presente como antes.
  La pausa congela tiempo y audio mediante la sesión/listener existentes.

Inicio `(-4.55,.03,35.8)`, final `(-5.68,.03,36.02)`, yaw −90°. El movimiento
sale primero en −X de la lámpara existente y luego converge en Z. Cara interior
del vidrio `x=-6.2825`; muñecas `x=-6.2425` con margen para el volumen de la mano.
La raíz conserva 0,51 m al plinto frente a radio 0,4 + skin 0,035. El jugador
permanece al otro lado y conserva su salida. El integrador comprueba referencias,
vidrio mediante su geometría local aunque esté inactivo, rig, estado del
controlador y fuentes antes de editar. La colisión efectiva se revisa en Play.

## Resultado central

Se corrigió el lookup de huesos para aceptar el prefijo real de Nathan. El
primer check llegó al stop sin atravesar obstáculos, pero la muñeca izquierda
quedó a 11,3 cm del target: no hubo golpe ficticio (`Contacts=0`). El apoyo se
subió de y=1,12 a 1,40 y pasó a z=35,82, conservando el plano del vidrio. Al
reanudar sólo ese extremo corregido se registró un contacto y asentamiento,
sin bloqueo; la cámara mostró ambas manos y el cuerpo próximo al vidrio.
La pose todavía debe juzgarse jugando; esto no aprueba su actuación.

El fixture habilitó la recepción y las fases previas del relay, con un Echo
estacionario sintético detectado realmente en A. No equivale a jugar P3 desde
el inicio. La entrada real de bomba dejó `OwnsPose=false`, ambas fuentes P3
detenidas y el mismo actor en `(-4.40,.03,-46.70)`. No se ejecutó la persecución.
Escena guardada fuera de Play; sin builds ni cambios en la toma/Echo de producción.

## Comprobaciones realmente hechas y entrega al Editor

Lectura focal del código de autoridad/transición y de posiciones/colliders/rig
en la Incursion privada; revisión de diff y referencias serializadas; manifiesto
CC0 local y SHA-256 de los dos WAV existentes contrastados en el proyecto central.
No se ejecutó compilación Unity, Console, Play, escucha ni comprobación de cámara.
Los WAV del worktree son punteros LFS; el integrador reutiliza los originales ya
presentes en el proyecto central. No se copiaron recursos privados.

Estado mínimo para comprobar: retorno de P3 tras aceptar B, Echo auténtico de
vuelta en A; cámara habitual junto a `A / return witness control`
`(-6.59,1.17,36.24)`, mirando hacia A y el intruso. Una toma centrada en A permite
separar las siluetas; la posición de una toma arbitraria no se corrige. El componente
expone `OwnsPose`, `HasContact`, `Settled`, `ApproachBlocked` y `Contacts` para
inspección. Tras observar un único avance/contacto, usar la transición existente
a bomba: `OwnsPose=false`, fuentes P3 detenidas, mismo cuerpo en el stand de bomba.
Pausa breve sólo si se necesita comprobar el nuevo avance; sin recorrido de campaña.

Dos dudas humanas: ¿se reconoce al segundo cuerpo y provoca retirada el avance?
¿el apoyo de manos/peso y la quietud posterior incomodan sin resultar mecánicos,
con sonido a volumen cómodo? Compilación o contadores no responden esto.

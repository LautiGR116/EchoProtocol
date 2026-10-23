# Ronda 3 — terror perceptible, 2026-10-23

## Dirección y base

Autorizada por el usuario: otros tres frentes en paralelo, Astra / Ultra,
sólo Editor, sin builds y con comprobaciones puntuales. Base jugable local:
`1dfb4cc0`, rama `codex/integracion-20261023`, Incursion privada guardada.
La ronda anterior está integrada; comprensión, miedo, actuación y escucha siguen
EN PRUEBA. Esta ronda no presupone una nueva prueba humana ni su aprobación.

El coordinador es «Evalúa la próxima iteración de Echo», thread
`01a12cb5-6030-77a1-b02b-93734f7c279b`. Cada chat implementa, hace una revisión
focal y entrega commits; el coordinador aplica después sobre la escena original.
No merge, push ni publicación desde los chats de implementación.

## 1. SE1 — el apagado llega hasta vos

Convertir la contracción clínica inicial en una amenaza espacial que obliga a
reaccionar. La secuencia actual ya apaga luces y cierra una puerta: ajustar sólo
volúmenes o cambiar un clip no alcanza. Reautorizar progresión lejos/cerca,
contactos materiales sincronizados, contraste y un remate físico visible junto
al espacio del jugador. Puede cambiar montaje, elementos locales y tiempos.
Debe existir una diferencia grande de escala/proximidad entre advertencia y
remate, y una dirección de retirada comprensible con cámara libre.

Conservar P1, espera del final auténtico del Echo, checkpoint y acceso a P2.
No acelerar la toma ni adelantar un fenómeno atribuido a una reproducción que
todavía continúa. No encerrar la cápsula con una hoja, quitar control, usar
oscuridad ilegible o reemplazar impacto por parpadeo repetido. La consecuencia
persiste; no programar una cadena aleatoria de sobresaltos.

Propiedad: `Horror/VS1StrongEventOne.cs`, `VS1DomesticDoorReveal.cs`,
`VS1DomesticThresholdCheckpoint.cs`; componentes/assets nuevos bajo `SE1Pressure`,
integrador `Assets/Editor/SE1PressureIntegration.cs` y
`Docs/SE1_PRESSURE_ITERATION.md`. Fuentes, luces y geometría exclusivas de SE1.
No modificar relay de P1, Echo, P2, SE2 ni mezcla global.

## 2. P3 — el otro cuerpo invade la distancia

La recepción ya muestra al intruso junto al Echo y reproduce un roce. Convertir
ese tableau en una irrupción corta del cuerpo presente: reconocimiento de dos
cuerpos, pérdida repentina de distancia, frenada/contacto contra un límite
físico y permanencia inquietante. La quietud posterior importa tanto como el
ataque. Priorizar silueta, manos, tronco, cabeza y peso sobre una caminata larga.
El remate debe ser legible desde el control de retorno habitual, sin apuntar
a un píxel ni girar la cámara automáticamente. Reemplazar el gesto débil, no
superponer una nueva pista a todos los indicios antiguos.

Sólo actúa la persona presente; Echo y toma permanecen exactos. Usar el intruso
existente, sin duplicarlo en P3 y bomba. Conservar reglas y salida de P3, margen
para reaccionar y espacio transitable. No captura ni persecución nuevas. Restaurar
la pose/propiedad al pasar a bomba; no cambiar su cacería, mezcla ni actuación.
La recepción no puede exigir esperar que termine una grabación larga.

Propiedad: únicamente la rama P3 y enlace opcional de `Campaign/IntruderStaging.cs`;
componentes/assets exclusivos bajo `P3Intrusion`, integrador
`Assets/Editor/P3IntrusionIntegration.cs`, `Docs/P3_INTRUSION_ITERATION.md`.
Leer `MachineThreatPerformance` sin editar sus métodos compartidos. Una nueva
actuación local debe tener propiedad explícita y retirarse al activar la bomba.
No tocar `ContrapruebaAtmosphere`, `RecombinationRelay`, mando de retorno,
`MachineCover*`, `EmbestidaRush`, rig compartido ni `ResonanceSequence`.

## 3. 6B — no quiero abrir

Replantear la recepción final como una llegada físicamente incómoda. El jugador
graba una aproximación y debe abrir desde dentro para verla regresar: la amenaza
surge de haber preparado él mismo esa cercanía. Reautorizar composición del
vestíbulo, profundidad visible desde la hoja, iluminación local y ritmo sonoro
de las acciones presentes para que abrir exponga al cuerpo real a corta distancia.
Hay que producir un antes/después espacial claro desde la cámara normal, no
limitarse a otra indicación o música de reconocimiento.

Conservar origen, grabación/RECALL, observación auténtica, conservación de esa
misma toma, llave y salida. No reinterpretar una toma previa, fabricar gestos,
voz o pasos del Echo, prolongar su reproducción ni simular que ataca. La puerta
la abre el jugador. Todo sonido nuevo tiene fuente presente identificable o es
un recurso interno inequívoco; el silencio del cuerpo grabado es parte del diseño.
Sin otra persecución, puzzle, pasillo largo o espera obligatoria. La confirmación
y salida deben seguir siendo claras después del momento de tensión.

Propiedad: presentación optativa y observables mínimos de
`Campaign/FinalDoorMemory.cs`, componentes/assets exclusivos `FinalReception`,
integrador `Assets/Editor/FinalReceptionIntegration.cs`,
`Docs/FINAL_RECEPTION_ITERATION.md`. Puede recomponer decorado local y luces
propias sin invadir casa, cambiar el recorrido/condiciones grabables, ocultar el
cuerpo a la validación o editar materiales/rigs compartidos. No modificar
`GameSession`, `CampaignDirector`, `House*`, `ResonanceSequence`, Echo o guardados.

## Producción y coordinación

- Tres chats nuevos, tres worktrees desde esta base integrada; Astra / Ultra.
  Fast sólo se activa manualmente si el usuario lo desea; no se afirma activado.
- Hay aproximadamente 2 GiB libres. No abrir más Editors ni crear Libraries,
  copiar modelos privados, descargar paquetes grandes o duplicar builds.
  Los workers leen la Incursion original y sus recursos privados, sin escribirlos.
  Entregan fuentes/assets y un integrador de Editor; esto no equivale a un check
  de Play. El único Editor central se reserva al coordinador.
- Los integradores validan referencias/configuración antes de editar, actúan
  sólo en su frente, son aditivos e idempotentes y tienen Undo/rollback.
  No abrir, sustituir ni guardar escenas automáticamente; no editar YAML.
- Se autoriza un subagente por chat con trabajo disjunto (por ejemplo, montaje
  sonoro o crítica focal). El chat principal dirige y termina la implementación;
  no cadenas de delegación, auditorías generales ni planes interminables.
- Audio: buscar primero grabaciones ya licenciadas; recursos externos pequeños
  sólo con procedencia/licencia registrada y verificada. No servicios pagos ni
  generación de voz sin el procedimiento de AGENTS. Contraste, ataque, espectro
  y distancia antes que nivel maestro. Conservar WorldSFX, listener, pausa,
  mono/parlantes/dinámica reducida. No bucles genéricos ni capas que tapen la acción.
- No editar README, ROADMAP, este plan, opciones, paquetes, ProjectSettings,
  escena compartida ni documentos de otro frente. Si un cambio necesita un dueño
  ajeno, entregar la dependencia concreta al coordinador; no invadir su código.
- Revisar el diff y la coherencia de referencias una vez. La integración central
  hará compilación/Console y un check breve por momento, añadiendo sólo un caso
  de pausa/reset/transición cuando la edición lo justifique. Sin builds, campañas,
  suites amplias, nuevas herramientas de prueba ni capturas exhaustivas.
- Entrega: commit(s), operaciones de integración, qué cambió perceptiblemente,
  checks realmente hechos y como máximo dos dudas humanas. No certificar terror
  o escucha por contadores. Un fallo concreto se corrige antes de entregar.

## Chats iniciados

Los tres arrancaron desde `4e240f2f`, con Astra / Ultra. Los tres entregaron;
sus cambios están **integrados en la Incursion local**, con comprobaciones
centrales focales. Terror, actuación y escucha siguen **EN PRUEBA humana**.

- SE1: `01a169fd-29ca-79e2-a135-88062533c56b`, worktree `f9be`.
- P3: `01a169fd-4dd5-7e42-8798-405ce1187155`, worktree `29c2`.
- 6B: `01a169fd-85d0-7fa0-85eb-c9bd0dfe1c6a`, worktree `9e19`.

Para recuperar espacio se excluyeron `ArtSource/` y `Docs/VisualEvidence/` de
las tres copias mediante sparse-checkout. Permanecen completos en el proyecto
principal, disponibles para lectura. Se avisó a los tres chats. Espacio observado
después: ~1 GiB; se mantiene la prohibición de Libraries/Editors adicionales.

## Integración central — 2026-10-23

6B `8f0ed7d6`, P3 `4842463e` y SE1 `9f22b40d`, sobre la misma base,
sin conflictos entre frentes. Correcciones centrales: preflight de rig P3 con
nombres completos, validación del vidrio inactivo (`1eaf61c1`), alcance real
de la mano de apoyo, separación/bordes de las láminas SE1 y despeje del número
6B. La escena privada se guardó mediante Editor y tiene copia antes/después en
`/Users/lautigr116/EchoProtocol-backups/parallel-round3-20261023/`.

Checks acotados: SE1 completó tres etapas/cuatro contactos y congeló el estado
al pausar; la puerta rechazó cerrar con la cápsula en su barrido y cerró al
despejarse. P3 alcanzó el stop mediante su controlador; se corrigió la mano
inalcanzable, se observó un contacto y asentamiento, y Enter de bomba liberó
pose/audio sin duplicar ni devolver el actor a P3. 6B ocultó el exterior con
hoja cerrada y aceptó la observación de una toma realmente grabada; se revisó
el encuadre y el número corregido desde la cámara. Capturas locales ignoradas
en `Builds/round3-integration-20261023/` (imágenes, no builds).

Límites: se prepararon estados de capítulo, sin recorrido completo. P3 usó un
Echo estacionario sintético de prueba, detectado por el lector real; no valida
la ruta natural del puzzle. Tras fallar el alcance, sólo se repitió el extremo
de contacto corregido. SE1 partió con P1 completado y sin toma activa. La
grabación de 6B sí fue capturada por el grabador existente. No se hizo escucha
comparativa ni se certificó miedo por contadores. Sin builds ni push; Editor
fuera de Play al entregar. No se añaden otras rondas de pruebas de rutina.

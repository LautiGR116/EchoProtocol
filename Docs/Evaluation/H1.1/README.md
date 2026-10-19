# H1.1 — checkpoint de evaluación hasta R3

Preparado el 12–13 de septiembre de 2026. Dirección consolidada y build local
disponible; **verificación de controles parcial**, no playtest humano ni aprobación
del recorrido continuo. Se termina aquí, sin H1.2 ni contenido nuevo.

## Validación posterior del creador — 2026-09-13

El usuario informó «Todo validado, con que seguimos» después de esta entrega.
Se registra **creator-approved** el checkpoint general de controles y recorrido;
los pendientes técnicos de mouse/E quedan aceptados por su reporte. No se
modificó el build ni se repitieron pruebas automáticas. El alcance de participación
de jugadores nuevos, ayudas, tiempos y reacciones emocionales no fue detallado.
La evidencia por dimensión conserva ese límite; ver `Docs/PLAYTEST_LOG.md`.

El informe de preparación que sigue conserva lo verificado y lo pendiente
al entregarse originalmente. Su recomendación de comprobar controles queda
superada por esta confirmación; el próximo paso depende de la evidencia de
una sesión sin conocimiento previo, antes de desarrollar contenido nuevo.

## Cambio de método — 2026-09-16

Por decisión expresa del usuario se avanza con intuición y criterio del creador,
sin exigir playtesting externo por falta de tiempo. Queda superado el requisito
de una sesión con jugadores nuevos antes de seguir. Se conservan este build,
las guías y el informe histórico; no se agregan resultados de pruebas humanas.
El próximo trabajo es definir la escena de evidencia de negación y después
el final C, manteniendo verificaciones técnicas. Ver Docs/DECISIONS.md.

## Abrir y evaluar

Build local macOS universal (arm64 + x86_64):
`Builds/H1.1-20260912-01/EchoProtocol-H1.1-20260912-01.app`.
Abrir con doble clic en Finder, sin Unity Editor. Entregar al jugador únicamente
el `.app` y [PLAYER.md](PLAYER.md) (también `PLAYER.txt` junto al build).
El observador conserva [OBSERVER.md](OBSERVER.md) y la evidencia técnica.
Reabrir el programa inicia una sesión nueva; R conserva consecuencias y cambia
según el checkpoint vigente. Escape libera cursor, no pausa. ⌘Q cierra.
No hay Final C, pantalla final ni guardado de progreso de una sesión a otra.
La evaluación termina en el Hub original; no anunciar ese destino al jugador.

## Identidad y reproducción

- Versión externa: **H1.1-20260912-01**. `bundleVersion` original: `0.1.0`.
- Commit base: `a93c6da6e5d183147423eb2685183a4d4a5968ab`.
- Working tree **sucio**: ya había 16 archivos tracked modificados y muchos
  archivos sin seguimiento al inicio. VS1, R3 y otros scripts/assets esenciales
  no están en ese commit. No se hizo commit, staging ni push.
- Unity `6000.3.21f1`, URP `17.3.0`, Input System `1.20.0`, target existente
  `StandaloneOSX`, Mono, `BuildOptions.None`; sin instalar módulos o servicios.
- Única escena incluida: `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`.
- Build GUID: `85998720667745d0b2d3ec46f52396e0`. Tamaño informado: 149,972,641 bytes.
- SHA256 del manifiesto de los 201 archivos del `.app`:
  `de7f877ab18ea5774056cc6bfb1bd7f90f1c1f284ce15abcfb4001fb70d2ca1f`.
- SHA256 del snapshot local `Evidence/build-inputs.tar.gz`:
  `e06a1dae0877c4d4c0039c95c20e6bcaf4df084bb06be79785a944155690109a`.

`Builds/H1.1-20260912-01/Evidence/` conserva los hashes de entrada y finales,
Git status expandido antes/después, manifiesto del `.app`, snapshot de 299 archivos
de Assets/Packages/ProjectSettings/ArtSource, informe de build, auditoría Editor,
logs y capturas sin editar. Los builds y esa evidencia local están ignorados por
Git; conservar esta carpeta mientras se evalúe esta versión. No distribuir
`EchoProtocol_BurstDebugInformation_DoNotShip`, el snapshot o las guías del observador
como material del jugador.

**Un checkout limpio del commit no reproduce esta entrega.** Para reconstruirla,
extraer el snapshot en otra carpeta vacía (no sobre este working tree), abrir con
la misma versión de Unity y los paquetes fijados, mantener el target macOS y
construir la única escena habilitada hacia otro destino. Esto requiere Unity y
la disponibilidad de los paquetes existentes; ejecutar el `.app` no. No se hizo
una reconstrucción limpia independiente ni se garantiza identidad binaria de
una recompilación. Para repetir la observación usar el `.app` identificado.

Al retomar el 13/09, el `.app` coincidía en sus 201 hashes. El working tree difería
del snapshot en `ProjectSettings.asset` (precarga transitoria retirada) y en
los valores ProBuilder `experimental.enabled=false` y
`editor.stripProBuilderScriptsOnBuild=true`. Se preservaron esas diferencias;
no se atribuyen al jugador ni se reemplazan silenciosamente con el snapshot.

## Cambios y preservación

Se consolidó la aprobación en DECISIONS, GAME_DESIGN, EXPERIENCE_MAP,
HORROR_DESIGN, AUDIO_DESIGN y ROADMAP: 20–25 minutos pulidos, un Echo, P1/P2/P3,
evaluación continua hasta R3 primero; evidencia de negación y Final C después.
Unity/Blender/audio local siguen como pipeline; Tripo queda condicionado a un
piloto autorizado y Higgsfield no se incorpora. Las guías históricas con
soluciones quedaron identificadas como regresiones, no instrucciones H1.1.

La lista de build apuntaba solo a EchoPrototype; se corrigió mediante API del
Editor para incluir únicamente VS1. El proceso de build serializó metadatos
URP en `DefaultVolumeProfile`, `PC_RPAsset` y
`UniversalRenderPipelineGlobalSettings`, precarga transitoria del Input System
y batching Standalone. Se conservaron las copias previas verificadas por hash;
no se ajustaron creativamente esos valores. Las adiciones de Volume son
worldOffset cero y filter cero, junto con limpieza de su lista de componentes.

Los 411 archivos existentes se cotejaron contra el inicio: ninguno eliminado.
**Todos los archivos preexistentes de Assets/_Game y ArtSource siguen idénticos
en bytes**, incluidas escenas, scripts, geometría, triggers, audio y tiempos.
No se agregaron scripts de runtime, assets de contenido, herramientas ni servicios.
README y GDD binarios históricos no se regeneraron; la autoridad vigente está
en Docs/ROADMAP.md y Docs/DECISIONS.md. PLAYTEST_LOG no recibe una aprobación ficticia.

## Verificado y pendiente

| Comprobación | Resultado y alcance |
| --- | --- |
| Compilación/build | Succeeded, cero errores. El primer empaquetado emitió 346 warnings, mayormente shaders Sentis; el empaquetado final usando caché informa 1 warning: Pipeline deshabilitado en Player por falta de RuntimePipelineConfig, conforme al setup existente. Ambos completaron; se retiene el informe final. |
| Escena/referencias | VS1 activa y guardada; 886 objetos, cero scripts faltantes/referencias rotas o nulas activas, 29/29 referencias de reset válidas; una cámara y listener activos en la inspección inicial. Seis nulos pertenecen a variantes inactivas. |
| Console final | Cero errores; una advertencia del servicio Account API del asistente. Unity fuera de Play, sin compilación pendiente y escena no dirty. |
| Arranque standalone | Hub original de VS1 visible, Metal Apple M3 e Input System inicializados; relanzamiento desde Finder. |
| Movimiento / R | Teclado real enviado por automatización: cambio de posición, R vuelve al origen y aparece el Echo. Capturas start/moved/recall. No es validación de sensación de control. |
| Mouse / E | Bindings y referencias auditados. E sin objetivo no produjo excepción. El mouse sintético no produjo giro controlable; **mouse físico y activación positiva de la estación R1 en el .app pendientes**. No se equipara binding correcto con interacción ejecutada. |
| Salida | Menú Quit y, en otro arranque, ⌘Q: proceso finalizado e Input System Shutdown; sin excepciones en el log. Mono emitió avisos de finalización de threads; una sesión anterior redujo automáticamente la resolución del atlas de sombras. |
| Recorrido P1 → R3 | No repetido de extremo a extremo en este `.app`. La evidencia R3 previa parte de un checkpoint doméstico preparado y no cubre el comienzo P1/SE1; no se presenta como una nueva prueba. |
| Comprensión/miedo/recuperación/ritmo | **EN PRUEBA**. No hubo jugador nuevo ni reporte humano de esta versión; 20–25 minutos es objetivo de producto, no duración medida del build. |

## Siguiente paso humano concreto

El responsable realiza primero una comprobación técnica en este mismo `.app`:
mouse físico tras clic, movimiento, R, y E sobre la estación R1 al alcanzarla
por el recorrido normal. Cerrar con ⌘Q. La limitación actual es de verificación:
no se ha demostrado un bloqueo del juego, pero no se certifica aún la operación
completa de controles ni el trayecto. Si falla, registrar equipo, pasos y estado
y resolver solo ese bloqueo antes de la sesión nueva.

Después, relanzar desde cero con una persona que no conozca el recorrido, dar
solo PLAYER y observar con OBSERVER sin pistas. Registrar por tramo tiempos,
intervenciones, bloqueos, reconocimiento del Hub, interpretación y emoción;
volcar esa evidencia humana en PLAYTEST_LOG antes de decidir cambios o H1.2.

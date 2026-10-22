# Folio — menús, pausa y lectura

2026-10-22. **Implementado; compilación verificada. Game View y uso humano pendientes.**

## Tres mejoras visibles

1. **Menús completos y acciones diferenciadas.** Inicio distribuye las filas
   según su contenido y reserva el pie aun con checkpoint y acceso a la bomba.
   «Iniciar desde el comienzo», «Continuar checkpoint», «Reanudar sesión» y
   «Probar la bomba» distinguen sus destinos. Etiquetas, atajos y estados tienen
   columnas propias; créditos tienen más espacio y texto de 18 px.
2. **Navegación con contexto.** Esc retrocede un nivel. C no abre créditos sobre
   ajustes o confirmación. Documentos vuelven a su origen; ayuda conserva pausa
   o sesión. Tab/Shift+Tab y flechas recorren controles con foco visible; las
   flechas del lector sólo desplazan cuando el texto tiene foco. La confirmación
   empieza siempre en «Conservar sesión». Reanudar desde la interfaz espera la
   liberación de E/R, las teclas de cierre y el clic, evitando entrada residual.
3. **Lectura ampliable y opciones claras.** Títulos largos y cuerpo comparten
   un visor medido, con cierre y tamaño siempre accesibles. Texto normal de 22 px
   o ampliado de 27 px sólo en documentos/notas; se conserva el progreso al
   recalcular y el último renglón entra en el recorrido. Rueda, barra, flechas,
   PageUp/PageDown y Home/End usan el mismo límite. Los ajustes explican dinámica
   reducida, mono y pistas; conservan las APIs de audio/ayuda existentes.

## Integración

- Rama local: `codex/interfaz-sesion`, sobre `9dbc67c5`.
- Commit de implementación: `4bda41fb` — `Polish Folio session menus, navigation and reading`.
- El commit siguiente documenta la entrega en este archivo; se puede integrar
  la rama completa o seleccionar primero el commit de implementación.
- Sólo scripts y el `.meta` del nuevo parcial `GameSession.FolioReading.cs`.
  No necesita reemplazar escenas, prefabs ni modelos. Sin merge, push, build
  de juego o despliegue.
- Métodos compartidos modificados en `GameSession.cs`: `Start`, `Update`,
  `ShowDocument`, `SetPaused`, `OnApplicationFocus`, `DrawMenu` (sólo cierre de
  documento heredado) y `CloseSessionNotes`. Auxiliares nuevos:
  `RequestSessionResume`, `MenuKeysHeld`, `CloseDocument`.
- En `GameSession.Folio.cs`: `DrawFolio`, `DrawFolioMain`, `DrawFolioSettings`,
  `DrawFolioCredits`; lector/notas se trasladan al parcial nuevo.
  `DocumentContentHeight` ahora incluye el título medido. `FolioUI` modifica
  registro/foco, botones, toggles y desplazamiento; `Navigate` y `Scroll`
  conservan llamadas existentes mediante parámetros opcionales.
- **Precaución con los chats de bomba/casa:** integrar por hunks si también
  editaron `GameSession`. `DrawFolioHud`, `DrawHUD`, `DrawCoverFailure`, sus
  llamadas/condiciones y reintentos se conservaron. `BeginOrResume` sigue
  inmediato y sin cambios; `SetPaused` conserva restauración de componentes,
  tiempo, audio y cursor, y sólo agrega cancelación de reanudación pendiente.
  Las acciones UI de checkpoint/bomba esperan el resume antes de su callback.
- Preferencia nueva: `EP.LargeReadingText`. Las anteriores conservan sus claves;
  ninguna se borra al empezar otra sesión. No se modificó `GameSession.Web.cs`.

## Comprobaciones y límites

- Unity **6000.3.21f1**, instancia identificada por el worktree `72dc`:
  compilación final de scripts exitosa y assemblies cargados, sin errores C#.
  Cuatro advertencias previas en tests: CS0108, dos CS0252 y CS0164.
  Evidencia local de arranque: `/tmp/echo-ui-editor.log`.
- `git diff --check` pasó; revisión de alcance y comparación exacta de los
  métodos protegidos. Distribución completa revisada en código: acciones
  y340–638 con checkpoint+bomba, antes del pie en y656. Esto **no** acredita
  legibilidad visual ni input en ejecución.
- El Editor no terminó de importar: `No space left on device` en ArtifactDB.
  Se cerraron únicamente las instancias abiertas por este chat y se eliminaron
  sus directorios generados `Library`/`Temp`; quedaron unos 6,1 GiB disponibles
  al comprobar. No se controló el Editor principal ni el del otro worktree.
- No se llegó a Play Mode ni se escribieron PlayerPrefs/checkpoints de prueba;
  los guardados y preferencias del usuario no necesitaron restauración.
- **Pendiente:** Console después de importar; inspección real a 1280×720 y
  1280×800; inicio con checkpoint+bomba; pausa → ajustes → volver → reanudar;
  documento largo normal/ampliado y última línea; mouse/barra/teclado;
  persistencia tras nueva sesión y pérdida/recuperación real de foco.
- `EchoProtocol_Folio.unity` está versionada y usa `EP.CampaignCheckpoint.Folio.v1`.
  Sirve para lector/menús básicos, pero precede al encuentro de bomba de
  Incursion: no acredita ese acceso simultáneo. No se abrió ni modificó una
  escena para aparentar esa comprobación. Los probes Folio existentes se
  inspeccionaron, no ejecutaron: asumen viewport de 332 px y navegación previa.
  No se creó un harness ni se hicieron recorridos de campaña.

## Prueba humana de dos minutos

1. **0:00–0:25.** En inicio, leer las acciones y recorrerlas con Tab/Shift+Tab.
   Si existe checkpoint, comprobar que el pie sigue libre y que se distingue
   de comenzar de cero y del acceso directo a la bomba.
2. **0:25–0:55.** Iniciar; Esc → ajustes. Cambiar volumen con ←/→ y una opción
   con Espacio. Esc debe volver a pausa; reanudar debe requerir otra acción.
3. **0:55–1:35.** Abrir la carta/documento disponible; alternar normal/ampliado.
   Rueda o End deben alcanzar la última línea si desborda. Tab debe llegar al
   cierre sin desplazar el texto; Esc debe regresar al contexto correcto.
4. **1:35–2:00.** Desde pausa abrir «Nueva sesión» y pulsar Enter: debe conservar
   la sesión. Reanudar, cambiar de aplicación y volver: debe quedar pausada.
   Juzgar tamaño, claridad y comodidad; ninguna comprobación automática los aprueba.

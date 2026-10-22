# Folio — menús, pausa y lectura

2026-10-22. **Implementado; compilación y comprobaciones acotadas en Game View verificadas. Aprobación humana pendiente.**

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
- Documentación inicial: `aa05fac4`; el commit de cierre de esta rama actualiza
  las comprobaciones y capturas. Se puede integrar la rama completa o seleccionar
  primero el commit de implementación.
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

- Unity **6000.3.21f1**, Editor del worktree `72dc`: compilación exitosa, sin
  errores C#. Cuatro advertencias previas en tests: CS0108, dos CS0252 y CS0164.
  Console sin errores durante la ruta UI; al recargar apareció una excepción de
  indexación `UnityEditor.Search.SearchDatabase`, fuera de los scripts del juego.
  Log local: `/tmp/echo-ui-capture-editor.log`.
- `git diff 9dbc67c5 --check` pasó. Comparación exacta de métodos protegidos,
  `BeginOrResume`, `SaveSettings` y `Restart`: sin cambios.
- El primer arranque falló por falta de disco. Se coordinó el uso de Unity con
  los chats de bomba y casa y el segundo importó correctamente. Se hidrataron
  los assets LFS comprometidos desde la caché local. No se controlaron sus
  Editors ni se copiaron cambios de sus escenas/modelos privados.
- Play Mode real en la escena versionada `EchoProtocol_Folio.unity`, con frames
  avanzando. Game View y render target comprobados a **1280×720 y 1280×800**.
  Inicio completo, ajustes, pausa, confirmación y lector inspeccionados en PNG.
- **Fixture de composición explícita:** Folio precede a Incursion. Para mostrar
  checkpoint+bomba juntos se usó una clave temporal propia y un encuentro
  inactivo, sólo en memoria, con guardados suprimidos. La captura acredita
  distribución del menú, no funcionamiento del encuentro ni sus accesos.
  Se retiró el fixture y se conservó el checkpoint real.
- Teclado virtual mediante Input System: pausa → ajustes → Esc → pausa;
  reanudar esperó mientras E/R seguían pulsadas. Al soltar, volvió al juego.
  Tab/Shift+Tab, flechas, Enter/Espacio y cambios únicos de opciones comprobados.
  Confirmación abierta en memoria: foco inicial en «Conservar sesión» y Enter
  canceló sin reiniciar. Componentes, tiempo y audio se restauraron al reanudar.
- Documento largo de prueba con caracteres españoles: normal/ampliado, Home/End,
  último renglón visible y cierre de vuelta a pausa. Cambiar tamaño en el final
  conservó el 100 % del recorrido; navegar fuera del lector no desplazó texto.
  Los textos narrativos de producción no se modificaron.
- Valores aplicados y guardados coincidieron. Stop/Play conservó volumen 0,95,
  dinámica reducida, mono, pistas desactivadas y lectura ampliada. Cambiar
  realmente de aplicación a Finder y volver dejó la sesión pausada.
- Se restauraron exactamente presencia/valores originales de las cinco claves
  de preferencias; se borró sólo la clave de prueba. Play y autotick detenidos,
  presets temporales retirados y únicamente nuestro Editor cerrado (PID 47271).
  Escena sin guardar; sin harness, recorridos de campaña ni builds.
- **Pendiente humano:** mouse/rueda/arrastre de barra, PageUp/PageDown y comodidad
  final. La herramienta nativa seleccionaba el Editor principal, por lo que no
  se operó para probar mouse. Créditos quedaron revisados en código, sin captura.

## Capturas locales

PNG reales de Game View, fuera de `Assets`, en una carpeta ignorada por Git:
`Docs/VisualEvidence/2026-10-22-UI-Session/`.

- `01-inicio-1280x720.png`: composición con checkpoint y bomba del fixture.
- `02-ajustes-1280x720.png`: etiquetas, explicaciones y estados.
- `03-documento-normal-1280x720.png`: título largo y lectura normal.
- `04-documento-ampliado-final-1280x720.png`: última línea completa.
- `05-documento-ampliado-1280x800.png`: lector ampliado en 16:10.
- `06-pausa-1280x800.png` y `07-confirmacion-1280x800.png`: retorno y opción segura.

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

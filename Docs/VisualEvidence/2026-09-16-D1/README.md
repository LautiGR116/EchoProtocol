# D1 — Del otro lado · 2026-09-16

Implementación autorizada por «Hacela», después de aprobar avanzar con intuición
y criterio del creador. 74 comprobaciones de recorrido/reintentos/reinicios aprobadas; sesión fresca
sin código de prueba verificada. No es un playtest humano ni
una validación de miedo, ritmo, interpretación o duración total.

## Entrega y base

- Escena: `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity`.
- Componente nuevo: `VS1DenialEvidence`. E fija origen junto al primer vidrio;
  R reproduce la grabación actual y solo una pausa real presenciada permite
  «No me detuve ahí.». P1/P2/P3, R3 y los canales del Echo se conservan.
- Build local: `Builds/D1-20260916-01/EchoProtocol-D1-20260916-01.app`.
  Abrir el `.app` con doble clic; arranca desde el comienzo normal de VS1.
- HEAD base: `a93c6da6e5d183147423eb2685183a4d4a5968ab`, con working tree modificado
  y archivos sin seguimiento. No hubo commit ni push. Un checkout limpio de
  ese commit NO contiene esta entrega ni la escena VS1 sin seguimiento.
- La carpeta local del build conserva `Evidence/git-final.txt`, hashes de los
  archivos fuente, reporte de build y `SourceSnapshot.tar.gz`. El snapshot incluye
  los archivos actuales de Git y los no ignorados sin seguimiento; excluye caches
  y builds. H1.1 queda intacto como base anterior independiente.

## Método y alcance

La prueba temporal `Source/D1ValidationProbe.cs` usa teclado y mouse virtuales
mediante Input System, el movimiento normal, el raycast E y los R reales. Parte
de un **checkpoint doméstico preparado**: sitúa al Player en ese checkpoint,
inhabilita SE1 y su trigger de transición, y aproxima su apagón ya completado.
Desde allí recorre P2 → SE2 → SE3 → R1 → E → P3 → R3 → Hub → D1. No concede los
flags de R3/D1, no teletransporta durante el trayecto y no modifica grabaciones.
La apertura completa P1/SE1 no se volvió a jugar; no presentar este método como
una sesión continua desde el inicio ni como evidencia humana.

Durante el recorrido comprueba cada frame la pose física del Echo contra su
snapshot inmutable (tolerancias 1 cm / 0.1°), la identidad del snapshot, soporte
del Player y exclusión mutua de las hojas de llegada R3. Los scripts temporales
se retiran del proyecto antes del build, conservando su fuente aquí.

## Casos técnicos

- Sesión nueva: D1 oculta y sin origen/evidencia; abrir salida P3 por sí solo no
  la habilita. Recorrer las dos zonas reales de llegada sí la habilita.
- P2/SE2/SE3/R1 y solución auténtica de P3 conservados; R3 no admite Echo como
  Player y sus hojas mantienen el enclavamiento durante retroceso/llegada.
- E por raycast fija D1 una sola vez, sin teletransporte ni R, y descarta la toma
  anterior. Antes de E persiste R1; después, R vuelve al lado de servicio.
- **R sin movimiento** conserva el comportamiento preexistente: no completa un
  ciclo vacío. La prueba inicial esperaba lo contrario y se corrigió; no se
  cambió `LoopController`. Para probar una interrupción de reproducción con R,
  el Player se desplaza ligeramente primero, armando una toma real.
- R prematuro con movimiento, pausa mirando en dirección incorrecta y paso sin
  pausa sostenida: ninguna negación. Mirar hacia otro lado permite que termine
  el Echo sin marcar la escena presenciada y ofrece reintento.
- Interrupción mediante R durante aproximación: limpia revisión y restaura las
  camas ambientales. Una nueva toma válida conserva el inicio real y produce
  una sola frase durante la pausa reproducida visible.
- Fin normal: ambiente restaurado. Cinco R conservan origen/evidencia, no repiten
  la frase, mantienen R3 y reinician P3 normalmente. Límite automático de 60 s
  probado después de completar D1; no se cambió su configuración.

## Presentación y límites

`D1_01_station.png`, `D1_02_real_pause.png` y `D1_03_witness.png` son renders de
la cámara real a 1280×720. Las variantes `_Game.png` capturan también el texto
superpuesto de Game View. Estas imágenes sirven para comprobar composición,
lectura y entrega visual; no prueban una reacción emocional.

Estación/panel, dos luminarias estables y texto provisional usan materiales y
componentes existentes. No se generaron assets, voces ni sonidos. La sustracción
afecta solo las dos camas ambientales existentes; no recrea audio grabado.
El panel de grabación es decorativo, sin collider; el vidrio original mantiene
la barrera física. No se añade salida, contenido ni transición de Final C.

Para recorrer D1 con criterio del creador: después de R3, buscar la estación
junto al primer vidrio por el lado de servicio, usar E y seguir sus indicaciones.
WASD mueve, mouse mira, E interactúa, R hace RECALL cuando existe grabación,
Escape libera el cursor y clic lo captura. Auriculares opcionales a volumen cómodo.
La legibilidad subjetiva, tensión y efecto de la frase quedan EN PRUEBA, sin
imponer una sesión externa de playtesting como condición para seguir.

## Preservación y Console

La comparación con los archivos al inicio de esta implementación conserva todos
los scripts previos y los 29 reset targets. En los objetos preexistentes de la
escena, el guardado añadió únicamente datos auxiliares URP por defecto a una
luz `R3 local cool fill`; no cambió sus valores de luz, posición ni gameplay.
También se añadió la nueva raíz D1 a SceneRoots. El detalle está en la carpeta
local `Evidence/scene-preservation.json` y `old-object-diff.txt`.

La pasada final registra cero errores de runtime. Aparecen avisos del render
Metal de las capturas temporales y del acceso a la cuenta de Unity AI; no son
fallos del recorrido. Las dos primeras pasadas corrigieron expectativas del
harness sobre R sin movimiento; sus registros permanecen en Evidence/FirstPass
y SecondPass. El código de juego no se cambió para satisfacer esas expectativas.

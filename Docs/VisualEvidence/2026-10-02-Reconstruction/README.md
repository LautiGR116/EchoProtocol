# Reconstruction — evidencia y límites, 2026-10-02

Nueva escena derivada de EncounterArt mediante Unity Editor APIs. `baseline.json`
identifica el respaldo; su apariencia fue rechazada por el creador. La selección
actual no equivale a aceptación humana.

## Comparación visual

Vistas desde controles normales. Las parejas representan momentos equivalentes,
no cámaras idénticas ni comparaciones de píxeles.

| Vista | EncounterArt conservada | Reconstruction |
| --- | --- | --- |
| Echo útil, lector A | [Antes](../2026-10-02-EncounterArt/final/campaign-P1-reader-A-playback-Game.png) | [Después](continuous/campaign-P1-reader-A-playback-Game.png) |
| Figura al entrar en SE2 | [Antes](../2026-10-02-EncounterArt/final/campaign-SE2-inside-room-Game.png) | [Después](continuous/campaign-SE2-inside-room-Game.png) |
| Llegada de la toma a 6B | [Antes](../2026-10-02-EncounterArt/final/campaign-21-Echo-at-6B-Game.png) | [Después](continuous/campaign-21-Echo-at-6B-Game.png) |
| Cooperación P1 | Lecturas persistentes de A/B | [C exige coincidencia; panel final corregido](puzzle-p1/02-replanned-coincidence-Game.png) |
| Receptor A en P3 | Autorización óptica sin confirmación del Player | [Confirmación real desde el otro lado](continuous/campaign-Reconstruction-A-confirmed-Game.png) |
| Orientación progresiva | Notas H con información completa | [Atención](puzzle-hints/01-attention-Game.png) · [Relación](puzzle-hints/02-automatic-relation-Game.png) · [Petición explícita](puzzle-hints/03-deliberate-steps-Game.png) |

Nuevo cuerpo bajo las luces reales de P1: [cerca](puzzle-echo/02-near-face-Game.png),
[perfil](puzzle-echo/03-profile-near-Game.png),
[espalda](puzzle-echo/04-rear-silhouette-Game.png) y
[desplazamiento registrado](puzzle-echo/05-moving-original-root-Game.png).
Las capturas no prueban animación corporal: el cuerpo es deliberadamente estático.
Fuente editable y procedencia: `ArtSource/Blender/Reconstruction/` y
`ArtSource/ThirdParty/MakeHumanBase/`.

## Verificación funcional

- `continuous/campaign-full.result.json`: PASS576, título a final,326,84 s de
  recorrido automático, cuatro tomas inmutables,3262 frames de reproducción
  comparados y todos los frames grabados cotejados. Cero fixtures; cuatro R
  y resets reales. No es una estimación de duración humana.
- `continuous/interaction-observer.json`: PASS,24 despachos con identidad
  HUD/acción y533 frames de pausa. El observador no mueve ni interactúa.
- `continuous/encounter-observer.json`: PASS,13772 frames con fuentes fijas,
  518 de pausa; conserva sustracción ambiental, motores y cuerpo sin audio ni
  Animator. No sustituye escuchar la mezcla.
- `interaction-corrected/`: PASS21 en las aproximaciones al teléfono/auricular,
  E sostenida, ayuda/documento, E+R y oclusión. `interaction-reach-final/`:
  PASS26, agrega alejamiento fuera de alcance y cambio al documento vecino.
  Dos fixtures declaradas: contexto/pose inicial y obstáculo temporal.
- `puzzle-p1/result.json`: PASS747, cero fixtures, tres tomas. B demasiado breve,
  llegada tardía, conservación de A, pausa, RECALL, replanificación y apertura
  persistente tras completar. Captura posterior al montaje final de placas.
- `puzzle-p3/result.json`: PASS1556, una fixture inicial de contexto R1/pose,
  tres tomas. Falta de presencia, E sostenida sin duplicación, A sin confirmación,
  B temprano, pausa, dos confirmaciones válidas, fin de reproducción, RECALL y
  cruce tardío. Esa prueba usa una revisión anterior del cuerpo; `continuous/`
  comprueba su integración con el modelo maduro final.
- `puzzle-hints/result.json`: PASS186, cero fixtures, tres R reales y tiempos
  de producción45/65 s. Los reintentos adelantan las notas a25/35 s. Pausa,
  documento, J explícita y apagar/encender no producen repetición.
- `puzzle-echo/result.json`: PASS764, cero fixtures, una toma real, rostro a1,39 m,
  perfil/espalda/desplazamiento; no audio/articulación añadidos y toma inmutable.
- `configured-audit.json`: referencias explícitas correctas, dos receptores,
  diez materiales/profundidad compartida, cero scripts faltantes, shader
  soportado/sin errores,45/65 s authored, clave de guardado independiente y
  única escena habilitada para build.

La campaña continua precede dos últimas correcciones locales: orientación de
las placas de P1 y referencia del contador de reintentos. `puzzle-p1/`,
`puzzle-hints/` y la auditoría final verifican esas correcciones. No se presenta
la captura antigua de las placas como el montaje entregado.

## Fallos e iteraciones preservados

`interaction-baseline/` conserva un arranque de prueba con ruta dentro de pared;
`interaction-baseline-2/` reproduce fallos reales de cobertura del auricular y
prioridad E/R, reparados antes de las pruebas válidas. `interaction-campaign/`
comprobó la interacción nueva con puzzles anteriores; no sustituye `continuous/`.

`puzzle-p1-editor-relay/` falló al final por un error del servicio AI Relay del
Editor, después de pasar sus comportamientos. Se conserva el diagnóstico;
únicamente ese error externo exacto se separa de errores de juego en el probe.
`puzzle-p1-before-display/` precede el montaje final de placas.
`puzzle-hints-accelerated-fixture/` usa umbrales temporales10 s; la prueba final
los reemplaza por tiempos reales. `puzzle-hints-unarmed-test/` intentó R antes
de moverse: el grabador correctamente no produjo una toma. Se corrigió el
recorrido de prueba. `integration-review.txt` detalla las referencias/placas.

## Entrega

`checkpoints/checkpoint-all.result.json`: PASS341, cinco estados guardados por
las APIs reales y diez cargas frescas; preferencias restauradas. Incluye luz
en recibidor/cocina/dormitorio/llevada y control general sin contestar teléfono.
`checkpoints/checkpoint.result.json`: PASS45, dos fixtures de capítulo y una
pose, cuatro cargas; aislamiento en marcha o ya confirmado mientras drena.
Ambas pruebas se ejecutaron en esta escena final. Los helpers conservan su ruta
histórica 2026-10-01; se trasladaron sus resultados a esta carpeta con fecha correcta.

`console-before-build.json`: cero errores. `build-result.json`: build universal,
0 errores/1 advertencia de Pipeline sin configuración de runtime. La preflight
conservada tuvo347 advertencias,346 de paquetes y una variable GUI que ocultaba
Component.tag; se renombró esa variable sin cambiar comportamiento. La recompilación
final y firma estricta pasan. `native-binary-verification.json` registra el binario.

`native-smoke.json`/`native-Player.log`: inicio, movimiento, toma37,6 s/RECALL,
H/J, pausa, créditos y Quit por menú nativo; sin patrones de error. Las capturas
CUA se inspeccionaron en conversación. **OS focus switching no verificado**:
la herramienta operó ventanas en segundo plano; ocultar produjo noWindowsAvailable
para clicks. No se contabilizaron esos intentos como recuperación real de foco ni
como click nativo del interruptor. `interaction-focus-notification/result.json`:
PASS8, una fixture explícita de notificación Unity false/true y controles reales;
pausa/limpieza de foco, E sostenida sin acción y E nueva sobre documento comprobadas.
Este helper es Editor-only y no altera el código de la build.

`package-verification.json` registra CRC, hashes/ejecutable y base intacta.
`backup-reference.json` señala el manifest y la verificación externa de las cuatro
capas fuente; no se duplica dentro del archivo su propio hash.

## Límites humanos

No hay nueva aprobación humana de interacción, anatomía, dificultad, pistas,
orientación, duración, comodidad, escucha o miedo. Las capturas sustentan una
selección artística; los probes prueban estados y consecuencias. Se mantiene
esta distinción y no se añadió opinión automática a PLAYTEST_LOG.

# EncounterArt — evidencia y alcance, 2026-10-02

Candidata independiente de Legibility, creada mediante Unity Editor APIs. La
base se identifica en `baseline.json`; su escena, app y ZIP se conservan.

## Comparación desde el recorrido normal

Capturas `Game` de la campaña continua, con la cámara/control del Player y HUD.
Las parejas corresponden al mismo punto del recorrido, no a una cámara idéntica
ni a una comparación de píxeles. No se colocó el Player para estas capturas.

| Vista | Legibility conservada | EncounterArt seleccionada |
| --- | --- | --- |
| SE2, entrada doméstica | [Antes](../2026-10-02-Legibility/final/campaign-SE2-domestic-entry-Game.png) | [Después](final/campaign-SE2-domestic-entry-Game.png) |
| SE2, entrada real al cuarto | [Antes](../2026-10-02-Legibility/final/campaign-SE2-inside-room-Game.png) | [Después](final/campaign-SE2-inside-room-Game.png) |
| 6B, figura frente al Player | [Antes](../2026-10-02-Legibility/final/campaign-21-Echo-at-6B-Game.png) | [Después](final/campaign-21-Echo-at-6B-Game.png) |
| Echo útil en P1 | La base conserva su superficie cian | [Nueva identidad durante lectura real de A](final/campaign-P1-reader-A-playback-Game.png) |

También se revisaron las vistas seleccionadas de aproximación, refugio, retorno,
entrada lateral de 6B, recepción y conservación. La primera captura `P1-visible`
apunta hacia el Echo, pero el marco opaco aún lo oculta. Se añadió una captura
tras la autorización real de A: no se forzó visibilidad ni estado del lector.

## Verificación seleccionada

- `final/campaign-full.result.json`: PASS563, título a final,327,06 s de
  recorrido automatizado, cuatro tomas inmutables,5608 frames de reproducción
  comparados y todos los frames grabados cotejados. Cero fixtures; cuatro RECALL
  y resets reales, cero reanudaciones inesperadas. La duración no estima una
  primera partida humana.
- `final/encounter-observer.json`: observador pasivo PASS,21313 frames con fuentes
  fijas y855 frames de pausa. Comprueba cuerpo/material/profundidad del Echo,
  ausencia de audio/Animator, una tensión de marco, parada de ambos motores y
  sustracción de ambiente. No reproduce una audición.
- `checkpoints/checkpoint-all.result.json`: PASS341, cinco fixtures por API y
  diez cargas frescas. Preferencias y supresión de guardado restauradas.
- `authoring.txt`:687 colliders originales y10 contratos serializados de
  grabación, puzzles, eventos, puerta y frontera conservados al derivar.
- `configuration.txt`:149 luces,60 fuentes, cero scripts faltantes,12 referencias
  acústicas explícitas, orientación conservada, materiales y profundidad unidos,
  shader soportado/sin mensajes, escena guardada y única en BuildSettings.
- `audio-signal.json` y `ArtSource/Audio/EncounterArt/manifest.json`: dos masters
  CC0 existentes, reutilizados sin modificar; mono48k, muestras finitas, sin
  clipping. Timbre, mezcla, localización y comodidad no se deducen de estos datos.
- `console-before-build.json`: Console sin errores tras campaña y continuación.
  `build-result.json`, `build-warnings.txt` y `native-binary-verification.json`:
  universal Intel/Apple Silicon,0 errores,346 advertencias de paquetes y firma
  local estricta. `native-smoke.json`/`native-Player.log`: prueba nativa breve de
  controles/toma5,3s/RECALL/H/pausa/créditos/Salir, sin excepciones identificadas.
  `package-verification.json`: CRC,203 archivos/hashes/ejecutable y base intacta.
  `backup-reference.json` señala el respaldo fuente externo y su manifest.

## Iteraciones preservadas

`first/` completó la campaña (PASS), pero el observador inicial falló al buscar
un Renderer habilitado después del fin de una reproducción. Se corrigió sólo
el observador. La figura de SE2 de espaldas, la separación de color insuficiente
y el motor demasiado lejano fueron rechazados visualmente; se corrigieron
orientación, cara/ropa, relieve y emplazamiento antes de `final/`.

`selected/` conserva un arranque fallido, aunque su nombre histórico parezca una
selección. Faltaba el bloque creado por Awake de StationFeedback; no había toma
válida. Se detuvo Play, recompiló y reabrió la escena; el arranque saludable y la
campaña final se comprobaron de nuevo. `selected-startup-failure.txt` separa los
hechos de la hipótesis de recarga. No se atribuye ese fallo a la partida final.

`tool-recovery.txt` documenta un bloqueo de una inspección JSON que intentó
serializar un Color de Unity sin proyectarlo a escalares. Se recuperó el Editor
sobre la escena guardada y se corrigió la herramienta. No se alteró el juego.
El inventario anterior de Legibility fue restaurado byte por byte desde su
respaldo tras detectar que una herramienta reutilizada escribió en su ruta.

## Límites

El juicio artístico selecciona rostro parcialmente iluminado y puerta como
ocultación en SE2, y el mismo cuerpo reconocible más próximo en 6B. El ambiente
presente pierde movimiento y energía al producirse el encuentro; la grabación
continúa exacta. No se afirma miedo, presencia humana, comprensión espontánea,
comodidad, calidad de audición ni campaña nativa completa. Esas evaluaciones
siguen EN PRUEBA. Ninguna opinión automática se añadió a PLAYTEST_LOG.

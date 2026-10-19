# Evidencia de campaña — 30/09/2026

## Pruebas objetivas

- `full.result.txt`: PASS. `full.checks.txt`: 142 comprobaciones, título → P1/P2/P3/D1 → presión → casa → galería → archivo → final; 14 RECALLs. `full.csv` registra 522,8 s de ejecución automatizada, **no duración humana**. Input System y CharacterController reales, con comprobación continua de poses inmutables.
- `house-rules.result.txt`: PASS, 65 comprobaciones adicionales. Orden incorrecto, mirada interrumpida, galería sin Echo, orientación final incorrecta, recuperación y cierre. Esta ejecución precede al refinamiento de la presencia; el recorrido integral posterior verificó la presencia física.
- `checkpoint-checks.txt`: 30 PASS; guardado/restauración de los cuatro orígenes, requisitos y ausencia de grabación inventada.
- `rules.checks.txt`: pruebas negativas de presión aprobadas; **la ejecución completa rules falló** más adelante en la aproximación a un documento. No se cuenta como recorrido aprobado. El documento fue desplazado y la ruta integral posterior pasó.
- `house.result.txt` y `expansion.result.txt`: fallos de desarrollo conservados (salida/colisión), corregidos antes del full PASS. `archive.result.txt`: prueba aislada PASS de cierre.
- `material-reload-check.txt`: escala de texturas persistente tras recargar la escena.
- `scene-inspection.txt`: referencias/objetos inspeccionados, cero scripts faltantes.
- `audio-source-checks.json`: 13 WAV originales, 48 kHz mono, cero muestras recortadas; no sustituye escucha humana.
- `build-summary.json` y `native-smoke.txt`: compilación final y alcance nativo exacto.
- `baseline-comparison.json`: 506/518 archivos previos idénticos, 12 cambios deliberados, ninguno ausente.

Después del full PASS se ajustaron texto narrativo, escala persistente de materiales, visibilidad del HUD, distancia de carteles y posición de un reloj sin collider. No se cambiaron las condiciones de los desafíos. La presentación, recarga, checkpoints y build se comprobaron posteriormente.

## Imágenes

`view-0..7.png`: cámaras de inspección artística; no representan una partida.
Las capturas numeradas del recorrido y `27-presence-game.png` pertenecen al juego en ejecución. `28-presence-final-staged.png` y `29-parlor-final-staged.png` son pruebas de presentación **preparadas**, no evidencia de recorrido. El reloj se desplazó lateralmente después de esas capturas para despejar la silueta; la build final contiene ese ajuste.

No hay nueva aprobación humana de miedo, interpretación, duración o comodidad.

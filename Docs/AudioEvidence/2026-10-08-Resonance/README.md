# Resonance — índice de evidencia

2026-10-08. [Informe de candidata y escucha humana pendiente](../../RESONANCE_CANDIDATE.md).

| Evidencia | Alcance |
|---|---|
| `final-native/campaign-full.result.json` | Player macOS final, título a final, PASS 575; cuatro tomas, cero fixtures o reanudaciones ocultas. |
| `final-native/audio-route.json` | Captura pasiva posterior a `SessionAudio`, ocho eventos, posiciones, filtros, ganancias, pausas y cuatro RECALL. |
| `final-native/listener.wav` | Mezcla real completa, 364,715 s, 48 kHz estéreo/PCM16; no normalizada ni con fuentes aisladas. |
| `final-native/mix-analysis.json` | Pico 0,53995; cero recortes, bloques perdidos o muestras no finitas; ventanas de 250 ms. |
| `listening/index.html`, `samples.json` | Seis recortes contiguos idénticos al PCM original. Montaje de 103,084 s con cortes, fundidos de 10 ms y separadores de 1 s; sin normalización. Contiene spoilers. |
| `acoustics/` | 30 comprobaciones, cuatro poses y una situación de casa declaradas; fuente, giro, pared, pausa, mono, dinámica reducida y teléfono interrumpible. No es una ruta continua. |
| `persistence/` | Diez cargas de casa (351 checks) y cuatro de aislamiento (49), preferencias restauradas; no vuelve a disparar encuentros pasados. |
| `retry/` | P3/reintentos, PASS 2743, cinco tomas; una fixture inicial de R1, luego WASD/mouse/E/R/H reales del Input System. |
| `scene-audit.json` | 2.415 entradas por ruta jerárquica: 151 luces, 606 colliders, 1.657 renderers y pertenencia al presupuesto de sombras; sin cambios visuales/físicos existentes. Rutas duplicadas pueden colapsar: no es un censo total. |
| `preserved-lighting-contracts.json` | 38 archivos idénticos al respaldo Lighting: escena, render, Echo, presupuesto de sombras, mixer y AudioManager. |
| `baseline-inventory.json`, `final-inventory.json` | 66 → 74 fuentes incluyendo objetos históricos inactivos; un oyente, 48 kHz, buffer 1024×4. Ocho emisores nuevos en escena activa. |
| `master-analysis.json` | Los trece masters finales; pico máximo 0,48, señal finita, seam y contenido espectral documentados. |
| `final-qa-build.json`, `delivery-build.json` | Ambas construcciones correctas y sin errores; QA con 346 advertencias, entrega con una. La cantidad de advertencias no es una métrica de juego. |
| `delivery-audio-checks.json` | 38 lecturas finales: ocho eventos, cuatro RECALL, reproducción única, modos predeterminados y recortes PCM idénticos con hashes/enlaces válidos. |
| `delivery-assembly.json` | Tablas reales de tipos; la entrega contiene el sistema sonoro y excluye todos los probes y el controlador QA. |
| `delivery-native-controls.json` | Inspección CUA de teclado/clicks en la entrega: inicio, movimiento, RECALL 8,1 s, H/J, pausa, audio, créditos y salida 0. No prueba mouse relativo humano. |
| `delivery-console-errors.json`, logs finales | Cero errores de Console al cierre; logs nativos sin excepciones de juego. Advertencia heredada del collider del auricular conservada. |
| `package.json` | ZIP 89.626.040 bytes; 203 archivos coincidentes, CRC, ejecutable, Intel/ARM, firma local original y extraída; Lighting intacto. |

`pass1-native` conserva el fallo de una aserción que identificaba la edición por
nombre de escena. `pass1-full` completa la campaña pero antecede la corrección
de dos referencias inactivas y del bucle de aire; no representa el sonido final.
`master-analysis-pass1.json` conserva la unión de aire con DC que fue corregida.
El reintento interrumpido por pausa real se conserva en
`Docs/VisualEvidence/2026-10-08-Contraprueba/Resonance-retry` y no se reanudó a
escondidas. El resultado final de reintentos está en `retry/`.

La cadencia final registra media 5,336 ms y p95 11,803 ms en Apple M3. Incluye
observación QA y no aísla CPU/DSP/GPU. El observador de Lighting capturaba más
vistas; no usar ambas medias como benchmark comparativo del audio.

La configuración visual se compara excluyendo el valor instantáneo de
`Light.shadows`, propiedad dinámica del presupuesto según cámara; sí compara
sus miembros y conserva su implementación byte por byte. Las capturas reales
finales incluyen P3, planta, teléfono, cocina, bandeja y salida sin corrección
posterior. Composición inspeccionada no equivale a negros aprobados en otra pantalla.

No se ha realizado una escucha humana ni una primera partida humana de Resonance.
La audibilidad subjetiva, actuación, enmascaramiento, localización, volumen
cómodo y miedo siguen EN PRUEBA. Las métricas de señal no los sustituyen.

# Contraprueba — evidencia y alcance

[Informe y recorrido humano](../../CONTRAPRUEBA_CANDIDATE.md) ·
[Comparación de cámaras](comparison.html).

Los recorridos usan entradas simuladas de Input System. No son partidas humanas.
Los recuentos incluyen comparaciones de frames y no miden calidad artística.

| Evidencia | Resultado y alcance |
| --- | --- |
| `before-P3` | Recorrido de Reconstruction y cámaras originales; una colocación inicial declarada en R1. |
| `campaign-editor` | Título a final, cuatro tomas inmutables, cero colocaciones y cero reanudaciones ocultas. Antes de la corrección final de pesos del modelo. |
| `campaign-native` | Misma campaña completa en un Player macOS separado, Apple M3, cero colocaciones; antes de corregir los pesos de ropa. |
| `campaign-native-final` | Campaña completa con el cuerpo corregido: PASS566, 7.345 frames, cuatro tomas, cero colocaciones/reanudaciones ocultas. Foco real antes de Enter, Apple M3/Metal; proceso termina con código 0. |
| `candidate-retry` | Ventana B perdida, toma A/B sin regreso, bloqueo físico de salida, H/J, RECALL tras A/B y durante E sostenida. Una fixture inicial R1. |
| `speakers-p3` | Capítulo y salida R3 completos, puerta A sólida, cancelación, pausa, salida tardía. Una fixture inicial; mezcla mono/dinámica reducida. |
| `final-echo` | Toma real desde título, vistas cercanas/perfil/espalda y caminando. Cero colocaciones. Piel deformada según los canales registrados; crecimiento máximo de arista observado: 0,00355 m. |
| `final-hints` | Tiempos de producción 45/65 s; atención/relación aceleradas con reintentos reales, pausa/documento excluidos, petición explícita y opción de desactivar. Sin fixtures de reloj. |
| `save-house` | Cinco estados domésticos, diez cargas nuevas; 341 comprobaciones, cinco fixtures explícitas, cero reanudaciones inesperadas, preferencias restauradas. |
| `save-isolation` | Máquina en marcha/confirmada, cuatro cargas; 45 comprobaciones, dos fixtures de capítulo y una de posición, preferencias restauradas. |
| `configured-audit.json` | Escena guardada, referencias explícitas, dos superficies de piel soportadas, 16 huesos, puertas físicas y namespace de guardado independiente. |
| `release-smoke` | App de entrega sin QA: inicio/movimiento, RECALL25,6 s, H/J, pausa, créditos y clicks de audio/restauración; cierre normal con código 0. UI inspeccionada mediante CUA en esta conversación. |
| `package-verification.json` | App universal, firma local estricta, ausencia de tipos QA, ZIP leído por completo y SHA256; ZIP de Reconstruction sin cambios. |

`audio-signal-audit.json` mide la salida real del listener a 48 kHz/16 bit:
estéreo 94,93 s, pico 0,16175; mono/reducida 101,63 s, pico 0,15021 y canales
idénticos muestra por muestra. El archivo estéreo antecede la reparación de la
unión de salida; no cambió el sistema de audio. Los marcadores acompañan los WAV.
Esto no demuestra volumen en hardware, timbre, localización, comodidad ni miedo.

## Iteraciones conservadas

`first-pass-p3` y `second-pass-p3` exponen obstáculos y problemas de composición
corregidos. `third-pass-p3` tuvo una pausa externa con reanudación declarada;
no se usa como evidencia continua. `candidate-p3` pasó el circuito, pero su salida
visual fue reemplazada por la unión física reparada y las cámaras posteriores.
`candidate-echo` pasó fidelidad y deformación, pero la inspección encontró picos
de ropa: lo sustituye `final-echo`, con pesos preservados desde la topología.

`weight-repair-input-interrupted` y `hint-input-interrupted` conservan intentos
interrumpidos por foco/tiempo activo insuficiente. Se hicieron pruebas nuevas
con GameView enfocado antes de iniciar; no se ocultó una reanudación a mitad de
recorrido. La prueba de pistas mantiene sus comprobaciones de tiempo activo y
rechaza pausas inesperadas. `campaign-native-focus-interrupted` conserva una
repetición nativa detenida al perder el foco en P1. La nueva ejecución nativa
espera el título preparado y dos segundos de foco real antes de iniciar;
no reanuda una partida interrumpida. `before-continuous` falló por una espera fija del
observador en SE1; se sustituyó por esperar el estado real, sin alterar el evento.

El juicio humano sobre comprensión, satisfacción, presencia, escucha, ritmo y
terror continúa **EN PRUEBA**. No se registró una nueva aprobación humana.

La build final tiene una advertencia deliberada: Pipeline desactivado en Player.
La compilación inicial también emitió 345 advertencias de shaders/variantes del
paquete de inferencia existente, sin errores de los shaders del juego. El log
nativo conserva el aviso heredado de escala negativa del collider del auricular;
la campaña prueba su interacción. No se ocultaron estos avisos ni se retiraron
paquetes ajenos al capítulo para eliminarlos.

`final-editor-state.json` confirma Edit Mode, escena guardada y compilación
inactiva. La Console conserva dos errores del servidor HTTP de Pipeline durante
descubrimiento de comandos; se separan en `final-console-infrastructure.json`.
No son errores de juego ni se afirma una Console vacía al cierre.

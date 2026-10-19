# Encounter — recibir la propia llegada

2026-10-01. `EchoProtocol_Encounter.unity` deriva de Presence. La revisión final
requiere abrir 6B desde adentro y mirar cabeza y torso a través del vano libre.
La figura llega con la misma toma de posición/orientación que el Player acaba de
grabar. Abrir la puerta es una acción del presente; no agrega un canal al Echo.

La zona interior mide1,6×0,9m: permite aproximarse sin buscar una marca diminuta.
El origen sigue en el vestíbulo y la puerta puede abrirse inmediatamente después
de R. La placa física explica esa preparación. Vidrio, hoja y marco bloquean la
observación; el material invisible de profundidad por sí solo tampoco cuenta.
Se conservan los2s de pausa registrada y0,6s continuos de observación. Pausar rompe
un intervalo incompleto; R reemplaza la toma y anula la observación anterior.

## Evidencia aislada

- `full`: **PASS66**,25,99s, una fixture inicial de casa/posición,1975 muestras de
  reproducción. Movimiento/E/R reales mediante Input System después de la fixture.
  La pausa válida medida en la toma dura **2,07548s**; no exige una espera prolongada.
  Observación, conservación del objeto exacto, llave y salida completan el recorrido.
- `rules`: **PASS147**,107,20s, una fixture inicial, cinco snapshots y11686 muestras
  cotejadas. Toma mirando en otra dirección y toma corta rechazadas; vidrio cerrado
  observado durante0,760s sin acreditación. La toma diagnóstica larga separa esos
  casos de la prueba mínima. Pausa, reemplazo por R y salida final comprobados.
- Las capturas `Camera` provienen de la cámara del Player y omiten OnGUI; `Game`
  incluye la interfaz. El estado de luz de estas fixtures no equivale a recorrer
  toda la campaña. La composición oscura final se evalúa por separado.

## Campaña continua

`continuous`: **PASS436**,270,88s, cuatro tomas inmutables,12101 muestras de
reproducción, cero fixtures y cero reanudaciones ocultas de pausa. Título a salida
mediante controles reales simulados. Incluye las consecuencias previas de luz,
reserva y casa; la captura final mantiene legibles cabeza/torso en el vano abierto.
Es duración de una ruta automática conocida, no estimación de primera partida.

`surface`: **PASS71**,29,30s, una fixture inicial y una fixture explícita de renderer.
Apagar la superficie de color y mantener el proxy de profundidad durante más de
0,75s no acredita; restaurarla permite observar y conservar la misma toma.
Son2328 muestras cotejadas, sin sustitución del registro.

Console final: cero errores, cuatro advertencias. Escena guardada sin cambios
pendientes; material de color y zona comprobados mediante referencias reales.
Build universal macOS **build_d0cd27a3e7aa**:197325757 bytes,16,19s, cero errores,
346 advertencias de Pipeline sin configuración runtime/Sentis/variantes Metal.
Cero advertencias RecordedPresence/RecordedSelfDepth. Firma local estricta y ambas
arquitecturas comprobadas. **Arranque/entrada nativos PENDIENTES**: CUA volvió a
informar que el Mac estaba bloqueado antes de lanzar la app. No se afirma arranque
ni inspección de Player.log. Reserve es el último checkpoint con prueba nativa. Presence/Reserve permanecen preservadas. Claridad espontánea,
confort, ritmo, escucha e impacto emocional siguen **EN PRUEBA**. Ningún resultado
automático se registra como partida humana.

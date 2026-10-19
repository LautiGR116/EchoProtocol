# Folio / evidencia de presentación

`index.html`: cinco pares antes/después y acceso a los PNG completos.
`comparison.png`: hoja de contacto; sólo sus miniaturas se redimensionan.

- `baseline-text-survey.json`: inventario previo, Resonance detenido y limpio.
- `type-*-title.png`, `type-selection.json`: comparación de tres fuentes en
  Game View real a 2940×1678, luego redimensionada. No prueba de ventana 720p.
- `native-ui-07/`: PASS130 final; capturas reales de 1280×800, 1440×810 y 1280×720.
  Lectura/navegación/ajustes y fixtures explícitos de documento largo, HUD,
  continuación guardada y final. El resultado registra página, tamaño y frames.
- `native-ui-06/`: PASS130 previo a conciliar el orden nativo de Shift.
- `native-modifiers.txt`: diagnóstico real de flags de Shift sin transición de
  tecla; el evento GUI llega antes de Update en el mismo frame. Tras la reparación,
  Shift+Tab y Tab alternan inicio/salida por controles nativos.
- `native-ui-04/`: PASS130 previo al ajuste de modificadores nativos.
- `native-ui-05/`: FAIL81 por expectativa demasiado breve de escritura de PNG;
  el archivo apareció después. Se amplió la espera acotada del verificador.
- `native-ui-01/`: intento inicial; falló una expectativa del probe de que la
  carta debía desbordar. La imagen demuestra que entra completa; se corrigió
  la expectativa, sin cambiar texto ni tamaño para satisfacerla.
- `native-ui-02/`: intento contaminado por Tab usado para preparar foco; el
  probe arrancó con selección modificada. Repetido con preparación sin entrada
  de navegación. No se ocultaron ambos resultados fallidos.
- `native-ui-03/`: PASS124 antes de la reparación final de foco.
- `continuous/`: primera ruta PASS575, anterior al último ajuste de foco y a
  cinco señales inicialmente ocultas; perdió 3371 bloques en la copia de
  evidencia de audio. Se preserva, pero su WAV no demuestra captura íntegra.
- `continuous-final/`: interrumpida por pérdida de foco; no hubo reanudación
  oculta. El diagnóstico de captura separa saturación de cambios de formato.
- `continuous-final-02/`: **FAIL** en P3 por pérdida de foco; no campaña completa
  ni reanudación oculta. `capture-audit.json` comprueba sólo el segmento capturado:
  cero bloques perdidos, cambio de formato, saturación de buffers o clipping.
- `editor-final/`: **FAIL** en P3 por pausa/pérdida de foco. No se reanudó por API.
- `runtime-sign-views/`: cinco señales inicialmente ocultas activadas explícitamente
  en Play Mode, diez vistas cercanas de cámara de inspección. Las caras visibles
  (side-1) se revisaron: texto completo, atlas correcto y contraste legible. No es
  la aproximación normal del jugador ni una partida; no se guardó la activación.
- `runtime-signage.json`, `configuration-audit.json`: 56 señales, materiales
  persistentes, referencias de seis fuentes incrustadas/papel/subtítulos/sesión.
- `delivery-build.json`, `delivery-assembly.json`: build y exclusión de QA real.
- `glyph-audit.json`: mapas Unicode de seis archivos de fuente incluidos.
- `preservation-audit.json`: 15105 objetos originales idénticos; sólo cambios
  de tipografía/material de señales, identidad de sesión y clave de guardado.
- `signage-integration.json`: 51 iniciales + cinco dinámicas, medición de su avance conservado.

Los PNG anteriores usan la interfaz legado en estado equivalente de la escena
Folio. La señalética del mundo ya está en Plex; no se afirma comparación de dos
escenas completas idénticas. Las imágenes de UI son evidencia perceptible; las
pruebas con fixtures no equivalen a completar la campaña ni a feedback humano.

`delivery-controls.json`: revisión acotada de controles reales en la entrega;
capturas en el transcript CUA. `package.json`: bytes/licencias/arquitecturas/firma
y ZIP extraído. La recuperación fuente tiene verificación externa en
`/Users/lautigr116/EchoProtocol-backups/folio-checkpoint-20261008/verification.json`.

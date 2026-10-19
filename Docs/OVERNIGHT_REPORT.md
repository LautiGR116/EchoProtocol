# Echo Protocol — edición completa, 30 de septiembre de 2026

Estado: edición breve completa y jugable entregada. Build macOS universal
(Apple Silicon + Intel), compilada y abierta en este Mac.

## Entrega y verificación

- Aplicación: `Builds/EchoProtocol-20260930/EchoProtocol.app`.
- Paquete: `Builds/EchoProtocol-20260930.zip`, con créditos e instrucciones.
- **70 comprobaciones de recorrido completas**, desde inicio hasta final, con
  diez RECALLs, evidencia inmutable y casos negativos de D1. La pasada guiada
  automatizada duró 245 s de juego; no es una medición de duración humana.
- **754 muestras de suelo**, puerta final sólida y cero scripts faltantes.
- Pruebas adicionales de pausa/Escape/cursor y límites reales del procesador
  de audio aprobadas. Console: cero errores/excepciones al terminar las pruebas.
- Build final: **Succeeded, 0 errores, 1 aviso**, 150.048.385 bytes. El aviso
  confirma que Pipeline está deshabilitado en el Player. La compilación inicial
  también tuvo avisos de shaders Sentis de paquetes existentes; no se ocultan.
- En la aplicación: inicio, movimiento grabado, R y reproducción, H legible,
  pausa, ajustes, créditos, reinicio y salida comprobados. La inyección de mouse
  nativo no estableció el giro de cámara; comprobar mouse y comodidad de
  captura/reanudación a mano. El recorrido completo se verificó en el Editor,
  no mediante una segunda pasada completa en el ejecutable.

Evidencia: `Docs/VisualEvidence/2026-09-30-CompleteEdition/`. El juego queda
abierto en el menú inicial. No se hizo commit ni push.

## Dirección elegida

Cerrar una experiencia breve y continua alrededor de una idea: el registro es
preciso aunque el protagonista no quiera reconocerlo. Conservar P1/P2/P3 y la
arquitectura doméstica permite dedicar el trabajo al inicio, comprensión,
controles de sesión y una consecuencia final jugable. No añadir acertijos de
relleno ni hechos sobre un accidente que el recorrido no demuestra.

## Cambios

- Escena de producción separada; original VS1 y builds anteriores conservados.
- Inicio, pausa real, reinicio, salida y créditos. Escape o pérdida de foco
  detienen tiempo, grabación y audio; H muestra ayuda opcional durante el juego.
- Instrucciones físicas en el vestíbulo y lectores, indicadores de registro y
  reproducción, aviso antes del RECALL automático y prompt E cuando hay objetivo.
- Volumen, atenuación de impactos y mezcla mono para parlantes. Son ajustes de
  señal y accesibilidad; su comodidad necesita oído humano.
- D1 ya no lleva directamente a FIN DE LA DEMO. Tras presenciar la pausa real,
  el jugador vuelve al vestíbulo y conserva el registro en una terminal. Se abre
  una salida física; luces y sonido se retiran detrás de él. El cierre reconoce
  la evidencia sin alterarla.

## Presentación en vivo — cinco minutos

Preparar la aplicación abierta en el menú, con volumen cómodo. Evitar spoilers
sobre la aparición hasta que termine la demostración.

1. **0:00–0:35.** Idea: «Cooperás con una grabación exacta de tus movimientos.
   El problema no es si el Echo miente, sino qué hacés con lo que demuestra».
2. **0:35–1:35.** Iniciar. Mostrar las instrucciones y registrar A y B mirando
   hacia el vidrio hasta ver cian. R vuelve al origen y reproduce el recorrido.
3. **1:35–2:35.** Ir por el corredor derecho, cruzar A y mirar C. Mostrar la
   cooperación simultánea y explicar que la orientación también se conserva.
4. **2:35–3:45.** Esperar al final del Echo, mirar la galería vacía y dejar que
   suceda la contracción de luces. Avanzar por el norte hacia la puerta cálida;
   cruzarla y dejar que cierre detrás. No narrar encima del sonido.
5. **3:45–5:00.** Pausar. Explicar la progresión espacial y que el recorrido
   completo vuelve al vestíbulo, confronta una pausa registrada y permite
   conservarla para salir. Mostrar una captura del final si queda tiempo.

La presentación muestra el primer arco completo; no promete terminar todo el
juego en cinco minutos. Tener las capturas locales como respaldo de exposición.

## Límites

No hay guardado entre sesiones: reiniciar o cerrar descarta el progreso.
La producción visual sigue siendo una base modular estilizada. No se implementa
una campaña de 30–45 minutos ni se afirma una duración humana sin medirla.
La prueba automatizada verifica causalidad, recorridos y estado; no demuestra
miedo, claridad espontánea ni comodidad auditiva. Para el viernes, hacer una
pasada humana en la build con mouse/E/R, pausa, lectura y escucha cómodas.

## Recuperación

Fuente anterior al trabajo, incluidos archivos sin seguimiento:
`/Users/lautigr116/EchoProtocol-backups/goal-start-20260930-175635/workspace-source.tgz`.
SHA-256: `af350b439392783e04aa504c175d982a599630cfecb0c85fb339d4cc94b36e1e`.
No se hizo commit ni push. No se reemplazaron builds anteriores.

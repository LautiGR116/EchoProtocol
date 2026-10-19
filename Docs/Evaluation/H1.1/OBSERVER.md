# H1.1 — protocolo del observador (no entregar al jugador)

Evaluar una primera sesión continua desde el arranque normal hasta el regreso
al Hub original en R3. El build termina como recorrido de prototipo, sin Final C
ni pantalla de victoria. Finalizar la observación principal cuando el jugador
haya regresado y explorado el Hub, o cuando decida abandonar. No revelar ese
destino de antemano. No usar las antiguas guías de R2/R3 con soluciones o holds
sugeridos para esta primera sesión.

## Preparación uniforme

1. Elegir una persona que no haya visto soluciones, capturas ni el desarrollo.
   Registrar experiencia previa con puzzles temporales y juegos en primera
   persona, sin explicar la mecánica de Echo. Usar un identificador anónimo.
2. Registrar build `H1.1-20260912-01`, commit y estado sucio según `README.md`,
   fecha, equipo/macOS, resolución, mouse, salida de audio y volumen cómodo.
   Mantener la configuración durante la sesión; registrar cualquier cambio.
   Auriculares son opcionales. No anunciar como disponibles mono, calibración o
   Reduced Dynamics: esos caminos todavía necesitan implementación/verificación.
3. Cerrar y volver a abrir la aplicación. R no equivale a sesión nueva: preserva
   consecuencias y checkpoints. No usar Editor, teletransporte, harness ni
   checkpoint preparado. Dar solamente `PLAYER.md` antes de arrancar.
4. Iniciar cronómetro cuando aparezca el Hub y haya control. Registrar pausas
   externas aparte; Escape libera el cursor, pero no congela la simulación.
   Si se graba pantalla/voz, obtener acuerdo de la persona; si no, tomar notas.

## Registro por tramo

En cada tramo anotar marca de entrada/salida, tiempo transcurrido, intentos/R,
hipótesis y frases textuales espontáneas, mirada/recorrido observable, bloqueos,
intervenciones y resultado. No deducir emociones a partir de un giro de cámara.
Separar conducta observada, relato posterior e interpretación del observador.
Si un tramo no se alcanza, marcar **no alcanzado**, no cero ni aprobado.

| Tramo / límite observable | Qué observar sin orientar |
| --- | --- |
| P1: salida del Hub → relay completo / Gate B cruzada | Descubrimiento de lectores y requisito de mirar; lectura de estados, preview frente a autorización del Echo, planificación de R, prueba de rutas y errores repetidos. Registrar memoria visual inicial del Hub sin pedirle que lo memorice. |
| SE1: P1 completo → cruce doméstico y cierre | Espera después del Echo, giro/comprobación espontáneos ante el golpe, detección y atribución de la contracción, elección de la puerta, visibilidad/navegación, reacción al cierre y comodidad sonora. Si no sucede, registrar posición, orientación y último hecho visible. |
| P2: checkpoint doméstico → gate y esquina | Comprensión del nuevo origen, duración elegida por el jugador para su registro, preview/hold, separación del Echo, fallos y recuperación sin solución verbal. |
| SE2: vista frontal tras esquina → revelación/lectura | Qué mira y cuánto; si detecta transformación arquitectónica, cómo la explica, si la atribuye a entorno, Echo o bug. Separar detección de miedo. |
| SE3: regreso hacia oeste → solapamiento y apertura a R1 | Descubrimiento espontáneo del cambio, vuelta de la mirada, evitación o acercamiento, orientación en espacio conocido, explicación y reacción emocional. |
| R1: entrada a sala clínica → salida hacia galería | Pausa espontánea, exploración y orientación, reconocimiento de función de la sala, cambios de tensión relatados después. Anotar E en el tramo siguiente si ocurre sin pausa. R1 es otra sala, no el Hub inicial. |
| R2/P3: E en estación → gate final y galería | Descubrimiento de E, confirmación del nuevo origen, lectura de A y B, plan de grabación propio, pérdida de vista del Echo, margen de cruce, reintentos y reconocimiento de R1 a distancia. |
| R3: salida oeste → llegada al Hub original | Descubrimiento de salida, tiempo del corredor (separarlo del total R3), dudas/callejones percibidos, hojas automáticas y atascos. Reconocimiento espontáneo del Hub desde el oeste: registrar cuándo, qué indicio lo causó y confianza. Residuo doméstico, interpretación del circuito y tensión final. |

## Intervenciones y bloqueos

No ofrecer pistas ni corregir hipótesis durante el intento autónomo. Si pide
ayuda, primero registrar la petición y preguntar de forma neutral «¿Qué estás
intentando hacer?». Si solicita una solución, registrar el fin del intento sin
ayuda antes de ofrecer cualquier pista; la continuación pasa a **asistida**.
Registrar texto exacto de cada intervención, quién la inició, marca temporal,
tramo, causa, estado previo y si resolvió el bloqueo. Ayudar ante un problema
técnico o malestar y detener la sesión si hace falta; no prolongarla para cumplir
una duración objetivo.

Distinguir bloqueo técnico reproducible (input, crash, collider, estado que no
permite avanzar) de comprensión, orientación, margen del plan o incomodidad.
Registrar último punto alcanzado, secuencia para repetirlo, mensaje/log si existe,
tiempo total y motivo de abandono. No reiniciar silenciosamente ni mezclar una
segunda sesión con la primera. El timer automático del intento es de 60 segundos;
registrar sus efectos sin explicarlo por anticipado ni recomendar holds.

## Preguntas posteriores (primero abiertas)

- «Contame qué pasó y cómo decidías qué hacer.»
- «¿Cómo entendiste RECALL y lo que hacía el Echo? ¿Hubo algo que pareciera fallar?»
- «¿Qué lugares reconociste al volver y qué te hizo reconocerlos?»
- «¿Qué momentos te hicieron sentir algo, y qué sentiste?» Después, si ayuda,
  ubicar por tramo tensión, confusión, miedo, aburrimiento y recuperación, por
  separado; una escala opcional 0–5 nunca reemplaza sus palabras.
- «¿Dónde te trabaste o sobró recorrido? ¿Algo fue incómodo visual o sonoramente?»

No anticipar culpa, negación, Final C ni la interpretación deseada. En una segunda
pasada técnica separada se pueden investigar R3, retroceso de puertas y R desde
el Hub; no contaminar la primera sesión con esas tareas.

## Ficha para copiar por sesión

```text
Participante anónimo / fecha / build / equipo / audio:
Conocimiento previo / instrucciones entregadas / registro consentido:
Inicio / fin / total / pausas externas:
Tramo | entrada | salida | duración | R/intent. | resultado autónomo/asistido/no alcanzado
Hechos observados y frases (con marca temporal):
Intervención | quién/por qué | texto exacto | hora | resultado:
Bloqueo | pasos | último estado | abandono/reinicio:
Hub reconocido: espontáneo/solo después/no; indicio y confianza:
Interpretación posterior del Echo/entorno:
Reacción emocional relatada / recuperación / comodidad:
Hipótesis del observador (separadas de evidencia):
```

Incorporar resultados reales a `Docs/PLAYTEST_LOG.md`, manteniendo separados
implementado, verificado objetivamente y human-approved. La preparación H1.1 no
aporta playtest humano. Revisar esta evidencia con el usuario antes de decidir
el próximo trabajo; no avanzar a H1.2 ni ajustar contenido por anticipado.

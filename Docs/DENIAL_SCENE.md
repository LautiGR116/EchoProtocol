# D1 — Del otro lado

**Estado, 2026-09-16:** implementación autorizada por el creador («Hacela»).
Implementada y verificada técnicamente en VS1 (74 comprobaciones de recorrido,
reintentos y reinicios). Se decide con criterio del creador;
no requiere playtesting externo. H1.1 conserva su build y snapshot. Final C es
el trabajo posterior, no parte de esta escena.

## Lo que sucede

Después de R3, el jugador vuelve al primer vidrio que ya conoce. Registra un
recorrido corto y se detiene frente al vidrio, mirando al pasillo de servicio.
Al usar R vuelve al punto de observación del lado de servicio. Desde allí ve
su propio Echo acercarse al vidrio y detenerse frente a él, reproduciendo
exactamente la postura y la duración que acaba de grabar.

Pensamiento provisional del protagonista: **«No me detuve ahí.»**

La posición sostenida demuestra que sí se detuvo. La orientación del cuerpo
compone la sensación de estar siendo observado; no demuestra mirada ocular,
intención, reconocimiento, culpa ni qué ocurrió en un accidente. La frase se
refiere al recorrido reciente. No se presenta como prueba documental de un
suceso histórico que este sistema nunca grabó.

El jugador puede recordar perfectamente su propia pausa. La contradicción es
con la negación del protagonista, no con la memoria obligatoria del jugador.
El Echo, antes útil, ahora ocupa ante él el lugar de una presencia observadora.
El impacto buscado es cercanía frontal, espera y silencio; está EN PRUEBA.

## Secuencia jugable

1. **Volver y fijar origen.** Solo después de la llegada real de R3 al Hub,
   una pequeña estación local de revisión queda disponible junto al primer
   vidrio, en el lado de servicio. E fija allí el nuevo origen, con confirmación
   visible, usando el contrato ya aprendido en R1. Hasta esa interacción,
   R conserva el origen R1. No se cambia el checkpoint al cruzar R3.
2. **Registrar.** La estación indica brevemente «Registrá tu recorrido al otro
   lado del vidrio». El jugador vuelve por el Hub y entra al inicio del carril
   de grabación. Un pequeño panel al nivel de la vista, orientado a ese lado,
   ofrece un motivo concreto para acercarse y detenerse. La iluminación permite
   ver que el lado de servicio está vacío; el vidrio y el espacio no se sustituyen.
3. **Elegir RECALL.** Cuando existe una pausa realmente registrada frente al
   panel aparece «[R] Revisar recorrido». R funciona también antes de eso; no se
   bloquea la tecla ni se introduce un nuevo puzzle, puerta o combinación. La
   pausa provisional mínima es 1 s dentro de una zona generosa, con posición
   estable y cuerpo orientado hacia el vidrio; tolerancias verificadas técnicamente, efecto subjetivo EN PRUEBA.
4. **Ocupar la otra posición.** R devuelve al jugador al origen de servicio,
   mirando al vidrio. El único Echo recorre el camino real completo, a velocidad
   normal, incluido el lead-in existente. El jugador puede moverse o mirar a
   otro lado. La vista frontal surge del nuevo punto de observación, sin giro
   forzado de cámara, teletransporte del Echo ni seguimiento de la mirada.
5. **Contradecir.** Solo mientras ese Echo reproduce la pausa comprobada y es
   visible desde el punto opuesto, aparece «No me detuve ahí». Sin explicación
   adicional ni respuesta de un narrador. La cercanía y la orientación de su
   propio cuerpo sostienen la escena. El Echo continúa o termina exactamente
   donde corresponde; no prolongar su inmovilidad para fabricar el efecto.
6. **Dejar una consecuencia.** La revisión queda marcada como presenciada,
   persiste a través de R y prepara la transición posterior a Final C. No
   resolver aquí la responsabilidad en los accidentes ni agregar otra amenaza.
   La salida concreta de Final C se define en su propio trabajo.

## Encaje en la escena existente

Inspección de Unity en Edit Mode: `VS1_ParallelReturn_Blockout`, guardada y fuera
de Play. El Hub inicial tiene piso centrado en `(0, -0.1, -8)`. `Observation Glass 1`
está en `(0, 1.65, -1)`, entre montantes en z=-3 y z=1. Gate A y Reader A están
en z=3; la propuesta se mantiene al sur para evitar rehacer P1.

Composición implementada: origen del observador `(0.9, 0.03, -1)`, hacia el
oeste; pausa grabada cerca de `(-0.9, 0.03, -1)`, cuerpo hacia el este. Hay
aproximadamente 1.8 m entre ambos, con el vidrio físico en medio. La zona de
pausa mide 1.1 × 1.7 m en planta; orientación ±30°, desplazamiento máximo 8 cm
y velocidad entre muestras ≤0.15 m/s durante al menos 1 s. El camino rodea
el extremo sur del vidrio por el Hub; no usa Gate A.

Esto reutiliza dos corredores y el vidrio existentes. No abrir la llegada R3
al revés ni entrar al hueco sellado de su cortina. No mover colliders, lectores,
puertas o reset targets de P1/P2/P3 para facilitar este evento. La activación
exige R3 abierto y al Player realmente llegado al Hub; `IsOpen` por sí solo se
activa antes de completar el retorno y no basta.

## Contrato de grabación y reintento

- La escena inspecciona el `LastRecording` del R recién completado. No conserva
  una grabación antigua oculta, no inventa un ciclo omitido ni reproduce dos Echoes.
- Derivar la pausa y orientación de los frames inmutables de ese snapshot,
  comprobando también proximidad temporal y desplazamiento entre muestras.
  Un flag de trigger del intento anterior no demuestra qué terminó grabado.
- El nuevo origen se fija **antes** del recorrido. `SetRecallCheckpoint` borra
  la grabación anterior; llamarlo después de capturar destruiría la evidencia.
- Si R es prematuro, se graba un desvío, se mira en otra dirección o no hay pausa,
  reproducir la toma tal cual y permitir reintento. No emitir la negación como
  si se hubiera demostrado. Indicar que falta registrar la parada frente al panel.
- Si no se presencia la pausa, no marcar la evidencia como vista ni congelar el
  Echo. Mantener disponible la instrucción breve de volver a registrar y usar R.
  Un nuevo R reemplaza normalmente la toma; no afirmar que repite el mismo archivo.
- El límite automático de 60 s sigue vigente. Aplicar al reset automático los
  mismos controles de snapshot; no exigirlo como espera ni cambiar ese tiempo.
- Finalizar la escena no altera los datos del Echo. Persisten el nuevo origen y
  el estado de evidencia presenciada; un arranque nuevo restaura el comienzo.

## Imagen, sonido y coste

Conservar la forma y material actuales del Echo. No deformarlo, añadir ojos,
animación de cabeza, pasos, voz o respiración: esos canales no se graban.
La iluminación local estable desde la activación hace legibles ambos lados en las dos
pasadas; no deshacer el apagón de SE1 ni usar un cambio de decorado como prueba.

La implementación sonora usa sustracción: durante la aproximación visible del Echo,
reducir provisionalmente la cama ambiental existente, sin aumentar otro nivel;
recuperarla al acabar o interrumpirse la revisión. Es un cambio del ambiente
actual, no reproducción de sonido grabado ni un golpe emitido por el Echo.
No reiniciar fuentes detenidas ni modificar el mixer global. También debe poder
leerse con parlantes o sin audio. Volumen cómodo; no stinger ni ataque fuerte.

Implementación: `VS1DenialEvidence` con referencias explícitas, estación/panel
simples, materiales existentes y dos luminarias locales estables. Reutiliza
el contrato E/RECALL y una instancia de `SD1SubtitleDisplay.Show` sin
activar el estudio SD1. Texto provisional, sin generar voz ni assets, servicios,
framework, captura semántica, puzzle P4 o una sala adicional. La presentación de
pensamiento debe distinguirse visualmente del texto técnico de la estación.

## Comprobaciones técnicas realizadas

La prueba con entradas reales de Input System recorre desde un checkpoint
doméstico preparado P2 → SE2 → SE3 → R1 → E → P3 → R3 → Hub → D1. Pasan
74 comprobaciones, incluida la entrega única de la frase durante la pausa real
visible, negativos, interrupción, cinco R y el límite automático de 60 s tras
completar D1. Las poses del Echo se comparan continuamente con su snapshot.
Una sesión fresca sin la herramienta de prueba restaura el comienzo.

R sin movimiento previo no completa un ciclo vacío, como antes. No se cambió
ese contrato para forzar la revisión. El ambiente vuelve a sus volúmenes reales
0.05/0.08; las referencias están completas y no hay scripts ausentes. No se
repitió toda la apertura P1/SE1 ni se atribuye miedo a estas comprobaciones.
Ver [método, capturas y límites](VisualEvidence/2026-09-16-D1/README.md).

**Límite creativo:** esta escena vuelve amenazante una acción propia y establece
negación observable. Por sí sola no resuelve el vínculo hermano/hijo ni constituye
una revelación de culpa. El puente a esa memoria y Final C queda para el siguiente
brief, con hechos del accidente todavía TBD. No alargar este beat para completar
el objetivo global de 20–25 minutos.

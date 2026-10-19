# Orientation — R1 integrado, sin build propia

2026-10-01. Deriva Shutdown por Editor API. El puesto completo gira 30° y retrocede hacia el perímetro; pantalla/collider, escritorio, lámpara/charco, papeles y silla viajan coordinados. Mismo Transform, referencia y pose mundial de RECALL. Dos luminarias existentes se recolocan/orientan; el rebote local reduce su rango. Sin luces, fuentes, estados o puertas nuevos.

La galería tiene placa de esmalte con tinta que recibe luz. El índice de archivos baja al frente de la repisa, como dato ambiental secundario; queda parcialmente oculto por el monitor desde el punto de interacción. Ambos owners DiegeticText conservan PrintedText en Awake.

## Evidencia y límites

- `authoring.txt`: 607 colliders, 54 fuentes, 138 luces conservados; 859 serializaciones y 602 poses de colliders ajenos intactas. La autoría corrigió primero la suposición errónea de nombre único de las cuatro luminarias legacy.
- `r1-input-survey/`: **PASS10, 5,464 s**, una fixture inicial de iluminación/sala/pose; después teclado, mouse y E reales del Input System. Pantalla neutral y puerta cerrada al empezar; E fija una vez el origen sin teleport/RECALL; paso físico hasta P3 y cero resets posteriores. Cámara del jugador, no toma libre; direcciones elegidas para inspección, no descubrimiento espontáneo.
- `r1-input-before-shelf-plate/`: PASS10/5,475 s anterior a la placa secundaria; se conserva como iteración.
- Baseline neutral equivalente en `../2026-10-01-Shutdown/r1-input-survey/`: PASS10/5,505 s. Las tomas NW ya mostraban la salida; no se afirma que fuese invisible.
- `baseline-virtual/`: primeras vistas estacionarias con estado final de Shutdown; no comparables como entrada neutral.
- `oblique-volume-check.txt`: nueve comprobaciones focales de la corrección local de VS1HubReturnGate, cinco fixtures de pose y una caja temporal girada/escalada. Se rechazan dos puntos aceptados por el AABB anterior. Es preparación técnica de Arrival, no otra ruta ni prueba de esclusa.

La integración R1 está seleccionada para el siguiente sucesor Arrival. No se repite aquí la campaña ni se crea una build de microhito: Shutdown continúa siendo el respaldo compilado. Entrada espontánea, recuperación emocional, comodidad y escucha siguen **EN PRUEBA**. Mac bloqueado; sin nueva prueba nativa.

## Baseline real para llegada R3

`arrival-input-survey/`: PASS17/14,313s, una fixture inicial de R3 ganado, iluminación/recorder/pose. Luego recorrido, Esc durante el cierre trasero, congelación de ambas hojas, apertura frontal sólo tras cierre real, guía sólo en Hub y regreso físico bloqueado. Cero resets posteriores. Los dos motores se resuelven por su referencia SlidingDoor, porque viven en Registration como objetos separados. El primer intento (`arrival-initial-owner-assumption/`) falló antes de caminar por asumir que eran hijos de las hojas; es un fallo del arnés, preservado.

La cámara frontal confirma pared/camilla y SUBSUELO como destino visual, además de texto inglés espejado suspendido al atravesar la hoja. Arrival reconstruye este último enlace; la referencia no prueba aburrimiento humano.

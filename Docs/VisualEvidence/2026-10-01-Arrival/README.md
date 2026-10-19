# Arrival — regreso a la recepción

2026-10-01. Deriva Orientation/Shutdown. Reconstruye el último enlace del archivo y gira la esclusa45° para presentar la recepción original en la dirección normal de marcha. Mantiene puertas, motores, volúmenes, referencias y consecuencias; no agrega luces, sonidos, espera ni datos Echo. Integra también la composición final de R1.

## Estado de evidencia

- Authoring Editor API:4076 serializaciones originales conservadas,138 luces/54 fuentes. Escena guardada;607→621 colliders contando retirados inactivos. `authoring.txt` distingue autoría de juego.
- Primera marcha `arrival-input-survey/`: **PASS17/15,074s**, una fixture inicial de retorno ganado/iluminación/pose. Después input físico; pausa durante cierre, fuentes fijas, guía sólo al llegar y regreso bloqueado. Cero resets posteriores. Precede limpieza de juntas.
- Selección visual `arrival-retreat-survey/`: **PASS21/18,216s**, misma fixture inicial; añade retirada inmediata durante SealingFront y reentrada. La hoja posterior permanece sellada; la frontal cierra/reabre y no se repite el subtítulo de llegada. Guard simétrico de las dos hojas observado entre hitos. Cero resets posteriores.
- La revisión de05/06 detectó dos juntas y zócalo del muro anterior cruzando el vano. Se desactivan joints−1/2 y−1/3 y se recorta skirting−1 a z[−8.2,−4.1], sin colisión. La rama final captura el vano limpio y el mostrador real delante.
- Baseline equivalente Orientation:PASS17/14,313s, yaw90, pared/camilla como vista frontal. No se deduce aburrimiento, reconocimiento ni tiempo de partida humano.
- Console final focal:0 errores de juego durante ruta; conserva1 timeout de Pipeline anterior al inicio,2 warnings (AI Account y Graphics Ring Buffer). No es evidencia de rendimiento sostenido.

VS1HubReturnGate y el anuncio local de CampaignDirector usan la caja en coordenadas locales; evita aceptar esquinas exteriores del AABB al girar. El diagnóstico previo de gate pasó9 checks con5 fixtures explícitas, separado de esta ruta.

Reconocimiento espontáneo, tensión, escucha, confort y ritmo siguen **EN PRUEBA**. Shutdown se conserva como respaldo anterior.

## Campaña y app recuperable

Campaña continua **PASS466**, 278.948s automáticos, 4 tomas inmutables y 12913 frames contrastados. Cero fixtures y cero reanudaciones ocultas. Incluye R1 recompuesto, retorno oblicuo limpio y guard físico simétrico de la esclusa; el resto de la campaña se conserva. No mide ritmo de primera partida.

Build `build_2664e8d1c23e`: Succeeded, 198783585bytes, 19108ms, 0errores/346warnings. Ejecutable universal Intel/Apple Silicon y firma local estricta comprobados. Estado nativo: Pendiente: CUA intentó abrir esta ruta exacta y devolvió Mac locked antes del arranque. No se afirma prueba nativa de Arrival; Reserve conserva la última prueba nativa breve.

ZIP y fuente tienen manifests/SHA separados; los checkpoints anteriores permanecen disponibles. P3 queda como siguiente revisión de arquitectura y cooperación, sin cambio incorporado todavía. Este hito no termina la producción sostenida.

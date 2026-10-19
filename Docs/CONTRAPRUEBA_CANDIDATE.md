# Echo Protocol — Contraprueba

**2026-10-08.** Capítulo de referencia integrado en la campaña de Reconstruction.
Escena: `Assets/_Game/Scenes/Production/EchoProtocol_Contraprueba.unity`.
[App macOS](../Builds/EchoProtocol-Contraprueba-20261008/EchoProtocol.app) ·
[Paquete ZIP](../Builds/EchoProtocol-Contraprueba-20261008.zip).
Incluye instrucciones y créditos. Universal Intel/Apple Silicon, firma local ad
hoc comprobada; no es una distribución notarizada. Reconstruction se conserva.

## La transformación

Elegí P3 porque reúne la regla ya aprendida, separación física y un regreso a un
lugar conocido. Permite convertir la planificación en exposición, sin sumar
otra tarea aislada ni falsear el Echo.

El circuito ahora exige **A → B → A**. La primera recepción abre un desvío técnico
y baja una persiana. Transferir en B detiene la ventilación y el rotor, apaga
parte del circuito clínico y revela una alcoba doméstica que ya estaba allí.
Hay que regresar por el pasaje conocido y sostener el mando ante la segunda
presencia real en A. El resultado conserva la salida; no termina en una carrera.

Reconstruí recepción, sala de registro, paso técnico, transferencia y retorno
con una paleta de esmalte verde, yeso marfil, metal usado y luz doméstica cálida.
Mobiliario, cielorraso, ventanas, conductos y mandos dan referencias físicas a
las reglas. Las pasadas desde la cámara corrigieron oscuridad excesiva, objetos
girados, obstáculos, placas delante del rostro y la unión con el archivo.

El adulto tiene un esqueleto de 16 articulaciones y piel/ropa diferenciadas.
Locomoción, respiración de presentación y orientación de cabeza se generan
durante el registro y quedan almacenadas; la reproducción usa esos mismos datos.
No hay mirada dirigida al Player, voz, pasos ni gestos nuevos del Echo. Una
revisión caminando encontró y corrigió pesos que estiraban la ropa hacia las
manos: ahora se conservan por topología, antes de subdividir el modelo.

[Comparación interactiva desde la cámara real](VisualEvidence/2026-10-08-Contraprueba/comparison.html).
Incluye la iteración fallida del cuerpo y su corrección; los encuadres varían
porque cambiaron el espacio y la fase de movimiento. No son imágenes retocadas.

## Comprobado

- Campaña completa en Editor y en un Player macOS QA separado. La pasada nativa
  final incluye el cuerpo corregido: 566 comprobaciones, 7.345 frames observados,
  cuatro tomas inmutables, cero colocaciones y cero reanudaciones ocultas.
- Errores de planificación, puertas sólidas, reintentos, cancelación de E,
  RECALL durante el mando, pausa, salida tardía y pistas progresivas pasan.
  Las pruebas focales de P3 declaran una única fixture inicial en R1.
- Catorce cargas nuevas recuperan casa/reserva/aislamiento; preferencias
  restauradas. Los WAV del listener prueban estéreo y mono/reducida, con canales
  mono idénticos y picos medidos inferiores a 0,162.
- La app de entrega excluye los controladores QA. Prueba nativa por teclado y
  clicks: inicio, movimiento, RECALL de 25,6 s, H/J, pausa, créditos, opciones de
  audio y cierre con código 0. No equivale a otra campaña humana completa.
- Cero errores de compilación/build. La advertencia final indica que Pipeline
  está desactivado en Player, deliberadamente. Compilaciones anteriores también
  registran advertencias de shaders del paquete de inferencia existente. El log
  nativo conserva una advertencia heredada del collider del auricular; el
  recorrido completo atraviesa esa interacción. No hay excepciones de juego. Dos errores de transporte HTTP de la herramienta
  del Editor quedan registrados aparte; no se presenta su Console como vacía.

[Resultados, iteraciones fallidas conservadas y alcance exacto](VisualEvidence/2026-10-08-Contraprueba/README.md).
Los recuentos incluyen comparaciones de frames; no miden calidad artística.

## Recuperación

El ZIP conserva su SHA256 junto al archivo. Fuentes, configuración y evidencia:
`../EchoProtocol-backups/contraprueba-checkpoint-20261008/`, fuera del repositorio.
`RECOVER.txt`, `manifest.json` y `verification.json` describen la recuperación:
Candidate → Legibility → EncounterArt → Reconstruction → Contraprueba, en un
directorio vacío. La comprobación lee todas las capas y compara cada archivo;
no extrae sobre el trabajo actual. No incluye cachés, builds ni guardados de
máquina. La app anterior de Reconstruction conserva su ZIP original sin cambios.

## Evaluación humana

Primero jugar sin guía y anotar qué se entiende, qué obliga a detenerse y cuándo
el Echo deja de sentirse una herramienta. H pausa y ofrece notas; J permite
pedir más. Usar auriculares a volumen cómodo y repetir el regreso con parlantes,
incluida la dinámica reducida. No hace falta subir el volumen.

**Recorrido asistido, con solución:** desde el nuevo origen de R1, recorrer el
carril de registro mirando los lectores: detenerse aproximadamente 6 s en A,
10 s en B y 16 s de vuelta en A. Pulsar R. Recibir A desde el puesto opuesto;
rodear por servicio y transferir B; volver al primer puesto y mantener E durante
la presencia de retorno. Soltar antes cancela. Confirmado el resultado, observar
al Echo y salir hacia el archivo. Si una ventana fue corta, R permite rehacerla.
Estos tiempos son un ejemplo probado, no una duración humana del capítulo.

## Crítica y límites

El capítulo ahora tiene un recorrido con aprendizaje, compromiso, separación y
un regreso perceptiblemente distinto. La apertura de la persiana obliga a recibir
la propia toma, y el sonido tiene una causa visible. El cierre conserva tiempo
para mirar, en lugar de empujar al Player hacia la próxima puerta.

El cuerpo sigue siendo estilizado; la marcha es una presentación procedural,
no una actuación capturada. Su naturalidad de cerca y al desplazarse puede ser
el punto más débil. Tampoco doy por demostrados miedo, satisfacción al resolver,
comprensión inicial, balance de escucha, comodidad o ritmo. Requieren una partida
humana, y no se agregó una aprobación ficticia a `PLAYTEST_LOG.md`.

No hubo compras, servicios de voz, frameworks nuevos, commits ni descarte de
trabajo previo. Créditos y procedencia están en [CREDITS.md](CREDITS.md).

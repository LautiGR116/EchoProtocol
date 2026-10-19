# La embestida — revisión del encuentro, 2026-10-12

**Implementada en Incursion; miedo y actuación EN PRUEBA humana.** El usuario
jugó El intruso y dice que sigue sin asustar. Esta revisión se concentra en la
preparación, irrupción, primer contacto y huida de la bomba.

- Adulto presente rehecho: malla facial hundida y desigual, labio retraído,
  hombro elevado, cráneo menos voluminoso, piel cetrina con daño localizado,
  ropa saturada y un delantal de goma articulado con borde irregular.
  Malla/material/prenda exclusivos del perseguidor; no se editan assets o datos
  del Echo. La postura comprimida continúa en reposo, entrada y persecución.
- Puerta probada desde atrás durante el trabajo manual. La apertura rápida
  descubre al cuerpo mientras acelera en diagonal hacia la caja del mando.
  Palma y peso llegan al contacto físico; rebote corto, recuperación y avance
  sustituyen la pausa de reconocimiento. Movimiento del root por CharacterController;
  la cámara permanece libre. La revelación no captura al jugador y reserva
  1,65 s desde el contacto antes de permitir la captura normal posterior.
- El contacto real dispara el metal amortiguado; un esfuerzo adulto breve entra
  75 ms después. Respiración corporal y fricción sostienen la continuación.
  Se retira el grito largo anterior. Se conservan buses, limitador, volumen y
  opciones mono/dinámica reducida. La medición no aprueba la interpretación.

**Jugar:** abrir `Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity`,
Play → **Jugar · La máquina que te cubre**. Preparación, RECALL, desvío por la
bomba y salida conservan sus reglas. Usar el volumen cómodo habitual.

**Prueba humana:** (1) silueta/rostro y proximidad; (2) continuidad del golpe,
recuperación y necesidad de retirarte; (3) contacto sonoro y orientación de la
salida sin molestia. No hay aprobación emocional inferida de la comprobación.

**Assets:** cuerpo derivado del Nathan ya licenciado, guardado con sus derivados
en `RenderpeoplePrivate/Embestida` y excluido del Git público; mismas restricciones
registradas en `CREDITS.md`. Prenda, escultura, shader, puesta y actuación son
trabajo del proyecto. Tres masters derivados de fuentes CC0 ya trazadas, sin
nuevas descargas, compras ni generación de voz; receta y hashes en
`ArtSource/Design/Embestida/audio-masters.json`. Estado anterior respaldado en
`EchoProtocol-backups/embestida-start-20261012`. Entrega sólo Editor.

**Comprobaciones:** compilación final sin errores ni warnings. Pasadas locales
por el acceso de la bomba verificaron ocultación, aparición, palma sobre la caja
y cámara/marcha libres. El seguimiento alcanzaba al jugador en los giros; se
corrigió su velocidad de avance a 2,1 m/s, conservando el arranque de la embestida.
El reintento final con la misma toma pasó trabajo, irrupción, retirada, desvío
hacia la bomba y salida; la comparación de los frames/eventos grabados no cambió.
Después se corrigió el corte de respiración por el controlador anterior, con
verificación aislada de esa fuente. Console sin errores nuevos del juego; hubo
un timeout de la herramienta de autoría, resuelto al ampliar esa llamada.
Editor queda abierto con Play detenido y escena guardada. Sin builds ni campaña.
La lectura visual confirma cambios en malla, prenda y puesta; no aprueba miedo.

---

# Incursion — encuentro de la bomba, 2026-10-11

Iteración acotada sobre Impact. **Implementada; EN PRUEBA humana**.
El feedback humano rechaza la eficacia del grito, la entrada lateral y la marcha
anterior. No se considera aprobado el miedo por mejorar una medición o un test.

## Puesta y actuación

La amenaza parte oculta detrás del tabique real, rodea su extremo y entra al
corredor visible desde el mando. Sale de la oscuridad,
fija al jugador y carga el peso antes de acelerar. La puerta lateral abre al
liberarla y recibe luz verde; el reconocimiento deja 1,15 s antes de atacar, después de rodear el tabique.
La cámara sigue libre en entrada/persecución. El contacto final bloquea marcha,
mirada y RECALL durante aproximadamente 950 ms, lleva ambas manos al jugador y
provoca una breve reacción de cámara antes del menú. Enter conserva la toma.

MachineThreatPerformance actúa sólo sobre la persona presente: base anatómica
explícita, carrera con brazos opuestos y codos flexionados, apoyos plantados,
pasos tomados de sus transiciones, frenado durante giros, mirada y boca ligadas
al objetivo y la voz. Al oír la bomba sin ver al jugador baja la velocidad,
orienta cabeza/cuerpo hacia ella y retira voces/pulso. No modifica el Echo ni
RecordedCharacterMotion. La escena anterior y sus builds se preservan.

El mando, el modelo visible, la palanca, el collider y las placas quedaron
alineados. El panel ocupa menos encuadre. El cartel de salida tiene escala
física legible. El HUD queda despejado desde la liberación hasta salir o fallar.

## Sonido y límites

Se reinterpreta la misma toma adulta CC0 mediante montaje: reconocimiento de
500 ms, ataque seco de dos esfuerzos de 895 ms y esfuerzos de persecución de
450 ms. Mantienen altura/formantes originales; no hay generación ni clonación.
Un apoyo sonoro coincide con el arranque; el contacto de captura dura 290 ms.
Procedencia, cortes y hashes: ArtSource/Audio/Incursion/masters.json.
Se conservan pasos, texturas, bomba y música de Impact, el bus existente, el
volumen guardado, mono y dinámica reducida. El limitador conserva .5 normal y
.18 reducido. Nivel digital no demuestra comodidad ni eficacia del miedo.

## Pulido local del Echo — Editor, 2026-10-11

Incursion activa captura de apoyos en `PlayerBodyPoseSource`: posiciones y
orientaciones de pies, mezcla de locomoción, reparto de peso e inclinación se
copian en la toma. Los apoyos permanecen anclados mientras son alcanzables;
el paso lateral evita cruzar tobillos y la parada/giro recupera un pie por vez.
La actuación corporal usa los clips existentes, modera brazos al desplazarse
de costado y reparte el giro entre pelvis, torso y cabeza según el giro grabado.
El Echo consume exclusivamente esos datos; no consulta suelo, jugador ni reloj
de actuación presente. Root, tiempos, RECALL y agarres conservan sus reglas.
Tomas sin ese canal y perseguidor presente conservan la rama anterior.

Compilación y Console sin errores. Comprobación corta en Editor: caminata,
desplazamiento lateral, giro y parada de una toma real; ~11 s de su reproducción
desde la cámara del jugador y revisión puntual del giro. La toma sigue siendo
la misma y la pose revisada se repite sin diferencia angular. Sin builds,
campaña ni nuevas herramientas de testing. Naturalidad: **EN PRUEBA humana**.

## Cierre local de 6B — Editor, 2026-10-11

Recepción más cercana al umbral, apertura hacia afuera y luz sobre la llegada.
La preparación y las placas dan una acción por etapa; la pausa real es de 4 s
y la observación directa de 1,35 s. La ventilación presente se retira con R;
el motivo de reconocimiento espera una llegada efectivamente visible.
La aceptación enciende una señal ámbar y recibe respuesta sonora de portería;
pantalla, bandeja y salida muestran la acción siguiente. Sólo este tramo suspende
la nueva grabación al aceptar, evitando que un RECALL automático borre la revisión.
El Echo reproduce la toma de esta sesión sin nuevos canales ni retiming.

Comprobación local: llegada grabada de 6,07 s, R/E, aceptación, conservación de
la misma toma, llave y salida; vistas desde la cámara del jugador. Se corrigió
una superposición de placas y la pérdida de aceptación por RECALL automático.
Compilación y Console del juego sin errores nuevos; controles temporales retirados,
Play detenido y escena guardada. Claridad, inquietud y comodidad: EN PRUEBA humana.

## Prueba humana

Elegí **Jugar · La máquina que te cubre** y mantené tu volumen cómodo habitual.

1. Desde el mando, incluso mirando tarde, ¿se entiende el peligro y encontrás
   la puerta antes de que te alcance?
2. ¿Reconocimiento, arranque, giros y captura se sienten físicos? Marcá un
   instante si un pie, brazo, cuerpo o texto todavía rompe la escena.
3. ¿El nuevo ataque y los pasos sostienen amenaza sin molestia, y reconocés
   por oído cuándo se desvía a la bomba? Mono/reducido siguen disponibles.

## Evidencia y entrega

La escena guardada es `Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity`.
La verificación realizada y el audiovisual están en
`Docs/VisualEvidence/2026-10-11-Incursion/`; el recorrido final registró 54
comprobaciones de captura/Enter y escape. Esto no aprueba miedo ni actuación.

El usuario cambió la entrega a **desarrollo sólo en Unity Editor**, sin nuevas
builds macOS, empaquetado ni recorridos repetidos. Se detienen las aplicaciones
de prueba y las herramientas auxiliares. Las comprobaciones siguientes serán
puntuales, en el Editor y motivadas por cambios o fallos concretos.

El estado inicial se preserva fuera del repositorio en
EchoProtocol-backups/incursion-start-20261011. Sin compras ni nuevos frameworks.

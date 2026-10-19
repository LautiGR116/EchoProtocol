# Shutdown — aislamiento visible y composición seleccionada

2026-10-01. **Composición seleccionada, campaña/continuación y app compilada comprobadas.**
Readers se conserva como checkpoint anterior.

`ShutdownProduction` deriva Readers. Agrupa veinte raíces del mando bajo su
propietario PressureIsolation, antes de moverlas a(5.75,1.35,−35.9) y girarlas15°.
El mando baja15cm; dos meshes de pedestal se acortan en el tramo medio,
con base apoyada en el suelo. La lectura de Editor confirma grip(5.64,1.44,−35.74)
y pedestal con límite inferior y0. El collider real y la representación del
gabinete se desplazan juntos2,75m hacia el este. Bombas, tuberías, receptor,
puerta, motor y purga siguen en su sitio.

Una luminaria de reserva existente se monta en el gabinete y apunta a la bomba;
dos pedestales de reserva acompañan el pasillo nuevo. No se añade luz ni se
modifica el circuito de trabajo. El componente PressureIsolation conserva su
serialización de reglas y referencias, incluidos los ejes de palanca/aguja.
Las fuentes de fricción y detent acompañan únicamente al mando que las produce.

La primera prueba focal usa una fixture explícita de capítulo previo, iluminación
y posición inicial; después sólo teclado/ratón reales. Verifica aproximación,
agarre completo, liberación, apagado y salida física, con cámaras desde el jugador.
Primera integración focal **PASS19**,17,695s, una fixture inicial y cero resets
posteriores. Incluye título quieto, motor previo a la entrada, E breve, E sostenido,
pausa real durante la caída, reposo persistente y travesía completa. No acredita
lectura humana del giro. `layout-initial-machine-light/` corresponde a reserva1.1; la luminaria de
máquina se sube a2.4 para revisión posterior en la ruta continua.

Variante original `ArtSource/Blender/ShutdownMotor/` integrada únicamente en la
bombaA: carcasa abierta, rejilla fija y rotor separado952tri,17.776tri totales.
`PressureMotorRotor` sólo presenta `MotorLevel01` con tiempo escalado; no introduce
audio, preferencias, canal Echo, contador de purga ni otra condición de progreso.
Importación conserva envolvente, pivote/puertos y colliders; posición del rotor
(6.208,.535,−36.5), eje importado local−X. Fuente y roundtrip Blender PASS35.

Las dos primeras pruebas se conservan: una tenía signo de pitch incorrecto en
el helper de ratón; otra exigía apertura completa sólo3.3s tras enclavamiento,
antes de los2s de recorrido de puerta posteriores a3s de purga. Se corrigieron
las expectativas/helpers del probe, sin cambiar los controles ni los tiempos
jugables. El PASS19 registró cero errores durante el recorrido; un timeout del
servidor Pipeline anterior a su inicialización fue identificado separadamente.

Pausa, estado y capturas no reemplazan audición ni prueba perceptual del movimiento.
La legibilidad espontánea, velocidad aparente, comodidad y tensión son EN PRUEBA. Propuesta y cálculos previos en
`ArtSource/Design/PressurePresence/`; no equivalen a una partida humana.


## Campaña y selección de acabado

`continuous/`: **PASS462**,277,353s, cuatro tomas inmutables y13.588 frames
cotejados, cero fixtures y cero reanudaciones ocultas. Incluye el gabinete/pasillo
nuevo, pausa real del rotor, aislamiento, vivienda y final6B. Console al acabar:
cero errores/cuatro advertencias. Las cámaras aceptan la luz local2.4, con carcasa
y superficie interna visibles; no afirman que la velocidad o frenado se perciban
bien en una partida. Esas capturas todavía usan las letras previas autoiluminadas.

Después se asignó tinta `PrintedText` a las nueve leyendas físicas de aislamiento
(un material local que usa el mismo atlas). No cambian palabras, geometría,
interacción ni reglas. El objetivo es que las placas respondan a la luz real y
desaparezcan en sombra; se registra por separado su selección focal final.


`layout-material-overridden/` conserva el PASS19 anterior al detectar un defecto
de autoría: DiegeticText sustituía el material del renderer en Awake. Ese intento
no prueba tinta iluminada. Se corrige la referencia explícita depthTestedShader
en las nueve placas y en P2, que heredaba el mismo problema de Readers. La nueva
prueba exige el shader efectivo en ejecución antes de aceptar la cámara. No se
cambia el componente runtime compartido ni las escenas históricas.


## Tinta efectiva, continuación y límites

`layout-before-plane-normal/`: PASS20,17,602s, una fixture inicial, sin resets
posteriores; comprueba el shader efectivo además de control, pausa de motor y
salida. Las letras ya no son autoiluminadas, pero su iluminación dependía de
normales que TextMesh no expone como un mesh ordinario. El shader PrintedText
ahora usa explícitamente el plano XY/frente local−Z. Esto elimina esa dependencia;
no se afirma que la ausencia de MeshFilter probase por sí sola normales ausentes.

Selección final de esa corrección: `ink-restored-virtual/`,
`ink-running-virtual/` y `reserve-ink-virtual/`. Son cámaras clonadas estacionarias
con poses y fixtures expresadas en cada SCOPE: control en ambos estados, P2 y
base/unidad de reserva bajo luces reales. P2 recupera tinta clara legible, la
reserva conserva tinta oscura y el título del mando queda en sombra al apagarse
su entorno. No son nuevas rutas de input ni prueba humana. No se sube la luz
para mantener visible un rótulo después de cortar su circuito.

`checkpoint.result.json`: **PASS45**, cuatro recargas frescas, dos fixtures de
capítulo y una de posición. E real guarda durante la purga; Continue restaura
motor encendido o reposo inmediato sin repetir latch/purga, conserva R inactivo
y repone exactamente las preferencias previas. Incluye la respuesta del rotor
en ambos estados. La campaña PASS462 precede sólo al acabado de tinta; las
pruebas de material y continuación posteriores se distinguen arriba.

Velocidad aparente, legibilidad espontánea de la parada, audición, confort,
recuperación y miedo siguen **EN PRUEBA**. Ninguna captura ni contador sustituye
una partida humana. La siguiente composición candidata se centra en R1.


## App recuperable

`build_c06938d5008b`: Succeeded,198.744.081 bytes,26.458ms, cero errores y346
advertencias de la familia Sentis/conv y Pipeline. Ejecutable universal Intel/
Apple Silicon y firma local estricta comprobados. La llamada CUA a la ruta exacta
respondió **Mac bloqueado antes de lanzar**: no se atribuye arranque, input ni
Player.log a Shutdown. Reserve conserva la última comprobación nativa breve.
No se repite la solicitud de desbloqueo pendiente.

ZIP recuperable: 79373113bytes; CRC y permiso ejecutable comprobados. SHA256 `902ce2e90d028f610b062b4346021ce8b1fae25b2e5aeae6c6f821df4ad36cd9`. LEEME y créditos incluidos.

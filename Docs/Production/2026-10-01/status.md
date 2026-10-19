# Producción continua — 2026-10-01

El objetivo sigue activo. Readers es el checkpoint compilado más reciente; su
arranque nativo está pendiente por bloqueo del Mac. Reserve es el último probado
con entrada nativa. Las ediciones anteriores permanecen recuperables. Este registro conserva el
orden de producción y distingue las verificaciones de cada checkpoint.

## Primer bloque: vivienda

Implementada en `EchoProtocol_Residence` mediante `ResidenceProduction`, derivada
sin sobrescribir Finish. Se reemplaza el pabellón de cuatro salas por vivienda
compacta, hall de 1,8 m, habitaciones con función, ventana/patio exterior,
reserva de luz transportable, llave disponible físicamente y puerta mecánica.
La reserva no requiere un orden ni un temporizador de mirada. Se puede salir
sin contestar; dejar una luz es una elección, no una contraseña adicional.

Materiales y modelos originales nuevos: papel floral/patio (ImageGen), ventana
vestida/radiador, sillón, maleta y consola (Blender). Kit incluye además mostrador,
casillero y puerta pendientes de completar integración. Fuentes en ArtSource.
Audio: contactos reales de madera, heladera ligada a corriente, válvula ligada
a trabajo, descarga física y peso de tabla, a partir de previews CC0 trazables.
No se afirma haber escuchado el material.

La vivienda pasa 217 comprobaciones con CharacterController/Input System reales;
la ruta alternativa sin teléfono pasa 48. Cero errores actuales de Console.
Se corrigieron el documento obstruido, dos waypoints dentro de muebles y una
línea de visión cortada por la partición del estudio. Se conservan los fallos.
La nueva build Residence `build_35ea81264992` es universal macOS, 0 errores,
346 advertencias de Unity (misma familia de inferencia/servidor deshabilitado).
Firma local comprobada; arranque, movimiento breve, RECALL y pausa nativos
inspeccionados. Log nativo sin errores. No se atribuye ruta completa nativa.

## Puerta 6B y maquinaria conectada

Se sustituyen galería triple y archivo repetido por puerta 6B: grabar una aproximación
real, ocupar el interior, abrir, conservar esa toma y devolver la llave. Escena separada
`EchoProtocol_Threshold` configurada con puerta de visor transparente, corredor
lateral y portería. Figura adulta y materiales de mueble/tejido integrados en
esa edición. Recorrido aislado inicial: 61 checks. Después del segundo pase artístico,
`rules` pasa 118 checks y cinco tomas (9.137 frames cotejados), incluyendo
tomas incorrectas, pausa parcial, R invalidando witness, recuperación y final.
Guardados: 336 checks en cinco decisiones domésticas, diez recargas reales;
prefs originales restauradas. Estas pruebas aisladas preceden a la campaña completa.

El terminal se montó en el mostrador y se abrió una portería con profundidad.
La placa de origen se orientó hacia la aproximación; su cuerpo de texto se
acortó después de que la cámara mostrara letras demasiado pequeñas. La puerta
final tiene audio físico (un inicio/un remate comprobados), sustracción de
ambiente al devolver la llave y fundido de señal sin modificar preferencias.

`EchoProtocol_Plant` añade depósito, colector, bombas y gabinete opaco originales
de Blender, con conexiones físicas y el collider probado del separador anterior.
La primera cámara detectó un error de reflexión X en FBX: corregido en autoría;
se conservan imágenes anteriores. La presión pasa 109 checks de audio causal
en Threshold, incluidas dos descargas para dos válvulas, sin descargas por pausa/R.

La campaña completa **Plant pasa 346 checks**, 316,98 s de recorrido automático,
cinco RECALL/pose resets, cero fixtures y cero reanudaciones ocultas de pausa.
13.174 muestras de reproducción y cinco snapshots inmutables cotejados.
Título, P1/P2/P3, SE1/SE2/SE3, retorno, presión, decisiones domésticas, presencia,
puerta física, testigo final, conservación, llave y salida pasaron de forma continua.
Console al terminar: cero errores, cuatro advertencias. Evidencia preservada en
`VisualEvidence/2026-10-01-Threshold/Plant-continuous`. Es una ruta automática
rápida; no es una duración humana ni aprobación de claridad, sonido o miedo.

## Reception comprobada; siguiente reconstrucción

`EchoProtocol_Reception` pasa **380 checks continuos**, 318,82 s de ruta automática,
cinco tomas y cero fixtures. Incluye fin de registro en R3, guía visible al llegar
al vestíbulo y rearme sólo mediante el origen real de presión. Los nuevos renders
del regreso usan la dirección normal de marcha, sin orientar la cámara al decorado.
El archivo en papel aporta profundidad; la recepción inicial aún necesita una
cabina habitable en lugar de escritorio contra pared. Registration abordará eso.

Prueba aislada de frontera: 36 checks, 65,003 s reales de simulación sin teletransporte
ni otra toma; conserva el snapshot y permite E/R normales en presión. Usa un bypass
explícito de R3 y una colocación de capítulo; no sustituye la ruta continua.
Prueba aislada posterior de audio: 11 checks/202 frames, doce luces en fixture
apagadas/restauradas, fluorescente a cero y maquinaria positiva tras el traspaso;
SE1 no vuelve a escribir. El intento lanzado antes de llegar a presión se conserva
como fallo de precondición. Una observación anterior había expuesto que el fondo
fluorescente revivía fuera de la primera región; se corrigió y se verificó aparte.
Console tras las pruebas: cero errores, dos advertencias.

## Registration en verificación

`EchoProtocol_Registration` reconstruye la recepción como cabina administrativa
habitable y añade los tres modelos originales ReceptionLife. La ruta inicial
pasa **40 checks**: título, carta física por raycast, abrir/cerrar documento,
grabación y cooperación P1, y pausa final real; cero fixtures. SE1 y los capítulos
posteriores quedan fuera de esta pasada. [Resultado](../../VisualEvidence/2026-10-01-Threshold/Registration-first-protocol/initial-protocol.result.json).
La reparación de materiales emisivos usa copias y asignaciones propias de la
escena nueva; conserva los materiales de los respaldos anteriores.

Audio de continuación: **17 checks** con tres fixtures explícitas de Pressure,
House y revisión final. Las luces iniciales siguen encendidas, el fluorescente
queda a cero y la maquinaria conserva ganancia positiva; un único traspaso y
ninguna escritura posterior de SE1. Preferencias sin cambios. Es una prueba de
estado equivalente al checkpoint, no una ronda real Guardar/Continuar.
[Resultado y alcance](../../VisualEvidence/2026-10-01-Reception/checkpoint-circuit-audio.result.json).
Los motores de las tres puertas R3 —salida desbloqueada, hoja trasera de llegada
y salida al Hub— están integrados en sus cabezales con los clips existentes y
ganancia `.13`; su ciclo de movimiento/remate todavía no está comprobado.
No se infiere audición, confort, claridad espontánea ni miedo de estos resultados.

## FirstAct: ruta continua y observación sonora

`EchoProtocol_FirstAct` pasa **391 checks**, 320,55 s, cinco tomas, 15.378 muestras
de reproducción y cero fixtures. Incluye acercamiento real a SE2 y paso físico
por la abertura sur de R1. [Ruta y alcance](../../VisualEvidence/2026-10-01-Threshold/FirstAct-continuous/campaign-full.result.json).
El observador pasivo SE1 pasa **8 checks/31.697 frames**: apagón y traspaso vistos,
fluorescente a cero, un único release. [Observación](../../VisualEvidence/2026-10-01-Threshold/FirstAct-continuous/se1-audio-watch.result.json).

Los [contadores R3](../../VisualEvidence/2026-10-01-Threshold/FirstAct-continuous/door-audio-observation.json)
registran arranques/remates `1/1`, `5/5` y `2/2` para salida, hoja trasera y Hub.
Son tramos motorizados, no pares completos abrir/cerrar. La ruta no retrocede
en la esclusa. El código explica tres movimientos normales de la hoja trasera
(abrir, cerrar, reabrir) y dos intervenciones remotas de `OnRecall`, en Pressure
y archivo: es una explicación compatible con los totales, no un registro
temporal de cada hoja. Esos RECALL ocurren a 21,6 y 67,1 m del emisor de radio
14 m; no prueban un artefacto audible. Evitar el resellado de una esclusa ya
vacía y con el frente cerrado queda como mejora menor. Audición y miedo siguen
sin validación humana.

## Recuperación

Respaldo inicial: `../EchoProtocol-backups/ambition-start-20261001/` (993 archivos).
SHA-256 del archivo fuente:
`1ae48e420200c04bcc49408265968ae6bf11c20880345d1b993e208f78d26a63`.
La build Finish y sus respaldos se conservan. Nuevo respaldo Residence:
`../EchoProtocol-backups/residence-checkpoint-20261001/`, 1315 archivos, SHA-256
`e62cef569677af10a69bc679e0aee17c46712da5c272ac6ba7bff870268058d5`.
Sin commits ni publicación.

Checkpoint Plant: `Builds/EchoProtocol-Plant-20261001/EchoProtocol.app`,
`build_a64f76cd9983`, 191.325.141 bytes, Intel/Apple Silicon, firma local válida,
cero errores. 347 advertencias: incluye cambios Editor pendientes de recompilar
(nueva autoría), Pipeline runtime deshabilitado e inferencia Metal. La compilación
anterior falló por `GUIStyleState.scaledBackgrounds`, disponible sólo en Editor;
se eliminó esa dependencia y el segundo build compiló. Arranque, movimiento,
RECALL de 12,9 s y pausa nativos comprobados; Player.log sin errores y app cerrada.
Créditos por clic no comprobados: la herramienta de clic devolvió `noWindowsAvailable`;
la siguiente edición añade acceso C y su pantalla ya se inspeccionó en Editor.
ZIP Plant SHA-256: `2cfb320f72c7584f911b9bf376713fefcc2ac2090aee1fa0211dd6368b4e9492`.
Respaldo `../EchoProtocol-backups/plant-checkpoint-20261001/`: 1.651 archivos,
SHA-256 `aa752c8fbefd8800a69e1356d8121e573b32315066ee5bf67fdfebe0983fdc91`;
incluye la autoría Reception todavía en preparación y precede a la reparación GUI.
ZIP Residence actualizado sólo en sus créditos:
`b7fe63d2fda59db68198c2ef6a18ff7f49ac2c63111a3565e5e60fa4c0370aec`.


Checkpoint Reception: `Builds/EchoProtocol-Reception-20261001/EchoProtocol.app`,
`build_8f27b265e14e`, 191.889.765 bytes, Intel/Apple Silicon, firma local válida,
cero errores y 346 advertencias de Pipeline runtime/inferencia Metal. Créditos C/Esc,
inicio, movimiento breve por pulsos nativos, RECALL de 10,6 s y pausa comprobados;
Player.log sin errores, app cerrada. No hubo partida humana ni evaluación del mouse.
ZIP SHA-256: `f87697c45027483258ebb117b3b296e95e1811f6a45e6368aeb24425a8822059`.
Respaldo fuente: `../EchoProtocol-backups/reception-checkpoint-20261001/`, 1.791 archivos,
SHA-256 `2394541a396d9943361bf588e92c554a3024c260d66168fae9666bc10d988be7`.

## Límites de evidencia

Las pruebas de dispositivos sintéticos verifican CharacterController, cámara,
raycast e invariantes; no equivalen a una partida humana espontánea. Los renders
Blender no equivalen al juego. Credibilidad, tensión, interpretación, audición de
la mezcla y confort siguen EN PRUEBA. Se continúa la producción con esos límites.


Checkpoint FirstAct: `Builds/EchoProtocol-FirstAct-20261001/EchoProtocol.app`,
`build_fb13eace67c4`, 194.122.197 bytes, universal Intel/Apple Silicon, firma válida,
cero errores y 346 advertencias de la misma familia de tooling/inferencia.
La solicitud inicial coincidió con una recompilación y devolvió Unknown/0 bytes;
se repitió sólo después de confirmar compilación terminada. Arranque, créditos C,
movimiento breve, RECALL de 6,9 s y pausa nativos comprobados; app cerrada,
Player.log sin errores. No hay prueba de mouse ni partida completa nativa.
ZIP SHA-256: `ecccb79bf4c79f53ad4edec3d5553d23eca695eb7fb807a5cf3e4dbb3670542f`.
Respaldo fuente `../EchoProtocol-backups/firstact-checkpoint-20261001/`, 2.050
archivos, SHA-256 `0b45611d8e8d290a07791f1391c1737244321ddd6553a21ff3400a8b8ad0b29f`.
El snapshot incluye los labels opcionales de ancla con defaults anteriores y
precede a la autoría Recovery. La lámpara SE2 mantiene keyword y propiedad de
emisión positivas en runtime; su apariencia tras el apagón aún admite refinamiento.


## Recovery: puesto de revisión y primer regreso

`EchoProtocol_Recovery` conserva FirstAct y sustituye la isla maciza de R1 por
un escritorio original con hueco de piernas, monitor con carcasa y soporte,
lámpara articulada, silla desplazada y archivo de cajones. Tres FBX originales:
12.496 triángulos/396.356 bytes, roundtrip 3/3 y nueve rayos libres en la apertura
del monitor. Fuente, anclajes y previews en `ArtSource/Blender/RecoveryOffice`.
La pantalla, el collider de interacción, el punto RECALL y el acceso permanecen
en sus posiciones funcionales; labels opcionales muestran FIJAR ORIGEN/ORIGEN
FIJADO. Dos luces locales y rebote tenue sustituyen el baño uniforme de cuatro
paneles; piso vinílico y tareas impresas completan el puesto. Se reduce el ámbar
heredado del cuarto original, se cambia la luminaria cúbica del regreso y se
refuerza la emisión de la pantalla de lámpara SE2 con materiales propios.

Ruta continua **PASS391**, 320,03 s automáticos, 16.055 muestras de reproducción,
cinco snapshots, cinco RECALL, cero fixtures. E de R1 sin teletransporte,
retorno real con R, P3, descenso, casa y final pasan. Console: cero errores,
cuatro advertencias. Evidencia en `VisualEvidence/2026-10-01-Threshold/Recovery-continuous`.
Las imágenes reales comprueban presencia, encaje y estados; no prueban calma,
inmersión, miedo ni claridad espontánea. La build Recovery ya está comprobada como checkpoint.

Siguiente intervención aprobada por criterio de producción: aislar físicamente
la maquinaria, despresurizar y atravesar el subsuelo sin otro ejercicio Echo.
Ese siguiente paso se implementó en Isolation, documentado debajo.


Checkpoint Recovery: `Builds/EchoProtocol-Recovery-20261001/EchoProtocol.app`,
`build_7f66a3ede1ec`, 194.827.045 bytes, universal Intel/Apple Silicon, firma válida,
cero errores/346 advertencias. Arranque, movimiento breve, RECALL de 10,5 s y
pausa nativos inspeccionados; Player.log sin errores y app cerrada. Crédito C
no repetido en esta app; ver prueba FirstAct, UI sin cambios aquí.
ZIP SHA-256: `ffef4fbb669b4a99b5b175a1fac7fb41eb61709eab8630522ddafbd75e40c432`.
Fuente: `../EchoProtocol-backups/recovery-checkpoint-20261001/`, 2.196 archivos,
SHA-256 `9cba6ae5eddc2a7c68236963f50915a777280db549c1dbd5a0bfc912e903722d`.
Excluye sólo drafts de código/IsolationControl todavía en creación; permanecen
en disco. El snapshot precede a integrar los nuevos hooks opt-in de aislamiento.


## Isolation: decisión física y salida del subsuelo

`EchoProtocol_Isolation` reemplaza el cuarto ejercicio Echo por un mando manual
que exige sostener E durante 1,8 s. Soltar antes devuelve el mango; pausar cancela
el agarre y exige soltar/reiniciar. Se puede mirar hacia atrás sin perder el agarre
si el contacto continúa al alcance y sin obstáculos. El enclavamiento corta cuatro
luces de trabajo, detiene progresivamente el motor en 2,2 s y descarga la presión
en 3 s desde B. Dos agujas recorren la escala superior hacia cero; la compuerta
libera el paso. Señalización, pedestal, soportes y tres modelos mecánicos originales
sustituyen los mandos anteriores. Luces de emergencia mantienen la ruta.

Cuatro fuentes locales distinguen motor, fricción, enclavamiento y purga. Material
CC0 ya trazado; no generación de voces ni nuevo servicio. La toma de P3 queda intacta
hasta el origen real de 6B. Recovery conserva el sistema anterior mediante una
referencia opcional. Clave de continuación propia: `EP.CampaignCheckpoint.Isolation.v1`.

**Ruta continua PASS421**, 270,97 s automáticos, 17.110 muestras de reproducción,
cuatro tomas/cuatro RECALL, cero fixtures y cero reanudaciones ocultas. Verifica
entrada física, pulsación breve, pausa manteniendo E, nueva pulsación, descarga,
agujas, luces, fuentes inmóviles, ausencia de nuevo Echo y final completo.
Console: cero errores/cuatro advertencias. Evidencia conservada en
`VisualEvidence/2026-10-01-Threshold/Isolation-continuous` (75 archivos + alcance).

La captura de esa ruta mostró el mando ilegible tras el corte. Se añadieron dos
pequeñas luminarias de instrumentos en reserva y se ajustó su posición hasta
leer rótulos, mango y cero sin tapar texto. Es una corrección perceptible posterior
al PASS421; los renders runtime separados usan fixtures visuales explícitos y
están en `VisualEvidence/2026-10-01-Isolation`. No se atribuye a la ruta continua
la comprobación de esa iluminación posterior.

**Persistencia aislada PASS43**, cuatro recargas, dos fixtures de capítulo y uno
de posición: guardado real de entrada y enclavamiento mediante E, Continue desde
escena nueva, presión canónica, puerta, luces, R inactivo, pausa y ninguna repetición
de enclavamiento/purga. Se restauraron las preferencias previas. Esta prueba no
es una partida continua; conserva el formato de evidencia explícito.

Build `build_3059a1ad9417`, `Builds/EchoProtocol-Isolation-20261001/EchoProtocol.app`,
195.213.809 bytes, 15,720 s, universal Intel/Apple Silicon, firma verificada,
cero errores/346 advertencias. Inicio, 24 pulsos W, RECALL real de 1,8 s, pausa,
reanudación y cierre nativos comprobados. Player.log sin coincidencias de error.
No se comprobó aquí mouse, partida nativa completa ni mezcla por oído.
La tensión, comodidad, comprensión espontánea y duración humana siguen EN PRUEBA.

ZIP SHA-256: `1abcf8989b39c77dc6fc3adaaca4c97f374aaba74d1fae737660fa8101da073d`.
Respaldo fuente: `../EchoProtocol-backups/isolation-checkpoint-20261001/`,
2371 archivos seleccionados de código, arte, escenas, documentos y configuración.
Se excluyen caches, builds y el repositorio Git. Los drafts están incluidos como
fuente de producción; su presencia no afirma integración. Manifiesto y SHA del
archivo fuente se conservan junto al respaldo.


## Porter: conservación como lugar de trabajo

`EchoProtocol_Porter` deriva de Isolation. El terminal provisional se sustituye
por carcasa original de monitor, pantalla de estado real, lámpara articulada,
libro de registro, silla retirada y archivo de trabajo. Se conserva el mostrador
y la devolución en portería. El collider del terminal pasa al monitor; la ruta
permanece accesible. `FinalRecordDisplay` sólo lee la toma observada/conservada y
su duración; no crea, modifica ni conserva grabaciones. La carcasa vieja superpuesta
se desactiva. Rótulos, soportes, emisión y tamaño de instrucciones se corrigieron
mediante cámaras del jugador.

Acto final aislado **PASS61**, 29,95 s automáticos, una toma inmutable y 2.726
muestras de reproducción. Un fixture público de casa y una colocación inicial;
luego entrada real E, grabación/RECALL, observación, conservación, llave y salida.
No es una repetición de la campaña entera. Estado final: duración 10,0050545 s,
pantalla 10,0 s y `ReferenceEquals(PreservedOriginal, LastRecording)` verdadero.
La captura inicial posterior confirma SIN TOMA OBSERVADA sin witness ni original.
El último ajuste de texto/emisión se verificó por separado después de PASS61.
Evidencia: `VisualEvidence/2026-10-01-Porter/`.

Console tenía un error de conexión WebSocket de Unity.AI tras diez intentos,
cuatro advertencias de tooling/render y el log PASS. No se observó excepción del
juego. La emisión del difusor se corrigió con su flag RealtimeEmissive y keyword;
ambos positivos en runtime, factor 1.

Build **build_5157a34cca1e**, 195.248.065 bytes, 15,607 s, universal Intel/Apple
Silicon, firma comprobada, cero errores/346 advertencias. **Inspección nativa
pendiente:** CUA encontró el Mac bloqueado y no pudo desbloquearlo; se solicitó
el desbloqueo. No quedó proceso EchoProtocol abierto. Isolation es la última
app con inicio/movimiento/RECALL/pausa nativos comprobados. Esto no impide seguir
produciendo por la API de Unity. El objetivo sigue activo.

Porter ZIP SHA-256: `87a5253c0939d4ffa26725c7932228104a1faf30713ed179c4e6b11d34ff257f`.
Respaldo fuente Porter: `../EchoProtocol-backups/porter-checkpoint-20261001/`, 2458 archivos; SHA y manifiesto junto al archivo. La comprobación nativa pendiente queda registrada en el respaldo.

## Offhook: una llamada con consecuencias físicas

`EchoProtocol_Offhook` deriva de Porter. Nuevo teléfono original de disco con
carcasa, cuna, auricular separado y cable que conserva ambos extremos durante el
movimiento. E descuelga y deja el auricular sobre la mesa; otra E lo devuelve.
La primera acción sigue cortando el suministro general. Colgar no devuelve la
luz ni reinicia el timbre. No se bloquea cámara/movimiento, no se añade una puerta
lógica y la llave sigue disponible sin atender. La línea permanece en silencio;
no se añadió voz ni canal al Echo.

Timbre y contactos proceden del mismo teléfono real, en dos grabaciones CC0 de
cookies+policy. Descargas públicas MP3, licencia y hashes conservados; tres masters
mono con recortes/fades/atenuación reproducibles. El agente no los escuchó.
Geometría original: 17.560 triángulos, seis materiales, roundtrip/pivote/encaje
comprobados. La primera captura reveló una taza y papeles viejos superpuestos al
aparato; se retiraron de esa mesa sólo en esta edición y se recapturó el resultado.

Prueba focal **PASS21**, un fixture previo de capítulo y uno de posición, luego
E/R reales por Input System. Incluye rayo central, timbre, rechazo de otro actor,
pausa durante movimiento, poses, cable, segunda E y salida de la casa. Revisión
restaurada desde escena fresca comprobada por API pública: auricular en cuna,
sin timbre/contacto ni interacción. No se modificaron preferencias en ese check.

Ruta completa **PASS426**, 274,99 s automáticos, cuatro grabaciones, 14.751 muestras
de reproducción, cuatro snapshots inmutables y cero fixtures de juego. Incluye
colgar mediante una segunda E y todo el cierre heredado de Porter. Console:
cero errores/cuatro advertencias. 77 archivos conservados en
`VisualEvidence/2026-10-01-Offhook/continuous`. La casa real tras SE1 es más oscura
que el fixture aislado; las capturas y el alcance mantienen esa diferencia.

Build **build_17919827eb7d**, 196.686.065 bytes, 16,007 s, cero errores/346
advertencias. Universal arm64/x86_64 y firma estricta comprobadas. El Mac volvió
a estar accesible: apertura explícita de la app Offhook, Enter, 24 pulsos W,
RECALL nativo de 1,6 s, pausa/reanudación y cierre comprobados. No quedó proceso;
Player.log sin coincidencias de error. No es una partida nativa completa, prueba
de mouse, audición ni aprobación humana de claridad, comodidad o miedo.

ZIP: `Builds/EchoProtocol-Offhook-20261001.zip`.
SHA-256: `d7c59d5ad487dc7d3c7739b82dd13daf62bf741dc9d045092bb7d1968722638f`.
Las builds anteriores se conservan. El goal sigue activo.

Siguiente carencia observada: las tres bases de reserva todavía parecen placas
provisionales; además `HouseMemory.carriedFuse` está sin referencia y la reserva
transportada no tiene representación física. Continuar con una unidad portátil
y bases domésticas creíbles, conservando la única reserva y sus consecuencias.
Esto es trabajo siguiente identificado, todavía no implementado en Offhook.

Respaldo fuente: `../EchoProtocol-backups/offhook-checkpoint-20261001/`, 2598 archivos;
manifest y SHA junto al archivo. Excluye caches/builds/Git; conserva drafts como
fuente, sin afirmar su integración.


## Reserve: transportar una consecuencia visible

`EchoProtocol_Reserve` deriva de Offhook. La reserva provisional se sustituye por
unidad portátil y tres bases originales con guías/contactos. Una mano actual la
transporta: aparece desde abajo, baja junto a obstáculos y se detiene con pausa.
Hay una sola representación activa; salir de la casa y RECALL no la duplican ni
la agregan al Echo. Sin luz/audio/collider de transporte. El contacto existente
sigue en la base usada. Fuente original pack/dock 5.845 tris; mano/manga 7.688,
con reimportación y fuentes/revisiones conservadas.

La inspección corrigió dos bases separadas del yeso, el solape de cortina del
dormitorio y una unión puntiaguda del pulgar. La tinta ahora recibe iluminación.
El recorrido continuo reveló un recibidor casi negro al retirar la reserva: dos
rebotes fijos de patio, tenues y con sombras, mantienen la lectura de paredes y
vanos. No cambian el ambiente global de SE1. Comparación con/sin rebote y mano
final recapturada con los valores posteriores a SE1 en fixture explícito.

Ruta completa **PASS434**, 276,83 s automáticos, cuatro RECALLs/snapshots inmutables,
13.024 muestras de reproducción y cero fixtures. Se conservan 77 archivos de esa
ruta, anterior a los últimos ajustes de mano/tinta/rebote/continuación/créditos.
No se presenta como prueba final de esos ajustes. Focal final **PASS65**, seis
posiciones y preparación pública de capítulo explícitas, E/W/R reales. Console
final cero errores/una advertencia; shader sin mensajes. Las cámaras finales
comprueban las correcciones perceptibles; no prueban comodidad o miedo humano.

Guardar/cargar con clave temporal aislada y escena fresca conserva Carried, llave,
salida y final sin Echo viejo. Esa prueba descubrió un defecto heredado: Continue
restauraba el ambiente inicial. La nueva referencia explícita restaura la luz
posterior a SE1 sin reproducir knock, relés ni puerta. Repetición positiva:
ambiente .0015, reflexión .000525, Recovery, dueño de mezcla liberado, cero entregas
de knock/relé y fuente fluorescente previa detenida. Preferencias del usuario
intactas; clave temporal eliminada. Se verificó la implementación pública de
Continue, no un clic en ese botón.

Build **build_51d6bd742012**, 197.216.081 bytes, 25,960 s, cero errores/346
advertencias. Universal y firma estricta comprobadas. Prueba nativa: apertura por
ruta exacta, título/créditos, Enter, 24 pulsos W, RECALL de 2,0 s, pausa/reanudación,
cierre y ausencia de proceso. Player.log sin patrones de error. Sin partida nativa
completa, prueba de mouse o audición. Miedo, claridad espontánea y confort siguen
**EN PRUEBA**. Evidencia `VisualEvidence/2026-10-01-Reserve/`.

Siguiente avance elegido: volumen y legibilidad del Echo, actualmente débil sobre
fondos cálidos y plano de frente. Hay un shader candidato sólo en ArtSource; no
está integrado ni verificado en Unity. Mantener la misma representación a lo largo
del arco y todos los datos grabados intactos. El goal sigue activo.

ZIP: `Builds/EchoProtocol-Reserve-20261001.zip`. SHA-256: `08bf7b2b666d01fea87acc2c5b62c9eeaf3e4895e9e759d2d31851cbb6483226`.

Respaldo fuente Reserve: `../EchoProtocol-backups/reserve-checkpoint-20261001/`, 2780 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Presence: cuerpo registrado consistente

Superficie propia en una escena/prefab nuevos: volumen legible en P1, continuidad
en SE3 y rostro/torso presentes por el visor de6B. Se rechazó la primera versión
por manchas de solapamiento y la segunda por perderse sobre el fondo cálido.
La malla auxiliar escribe profundidad propia; la superficie final mantiene un
color estable y fade de cercanía. No cambia geometría, muestras, sonidos, manos
ni carga grabados; posición/yaw siguen siendo los únicos canales.

Ruta final PASS436, cuatro tomas,12.142 frames cotejados, cero fixtures,276,97s
automáticos. Prueba de superficie PASS63 con dos tomas por input y fixtures
explícitas; fade completamente oculto coincide pixel a pixel con desactivar
ambas superficies, excluyendo ruido de postproceso sólo de ese cálculo.
Console:0 errores/4 warnings; shaders sin mensajes. Transparencias posteriores
quedan rechazadas por la aproximación de profundidad; no se afirma OIT ni confort.

Build `build_68bd4071a13d`,197.322.541 bytes,31,60s,0 errores/346 warnings de
Pipeline/Sentis; universal y firma verificadas. **Nativo pendiente:** CUA informó
Mac bloqueado y no pudo desbloquear; no se atribuye arranque ni audición.
Reserve conserva la última prueba nativa. Fuente del encuentro6B siguiente está
preparada fuera de Assets: dirección aprobada, todavía no integrada.

ZIP Presence SHA-256: `03df649b65ba6632f98a2b1ddf2871ce88bf748f4a832fa0d38fbde34959bb0f`. App compilada/firma comprobada; nativo pendiente.

Respaldo fuente Presence: `../EchoProtocol-backups/presence-checkpoint-20261001/`, 3181 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Encounter: recibir la propia llegada

La revisión6B ahora exige abrir desde adentro y ver cabeza/torso por el vano libre.
Se conserva la pausa de2s y el intervalo de0,6s; la franja interior es generosa
(1,6×0,9m). El Player puede preparar la hoja inmediatamente después de R. La placa
explica esa acción; el terminal muestra sólo la toma actual observada/conservada.
Vidrio, marco y hoja bloquean la acreditación. El proxy invisible de profundidad
no cuenta como figura. No se añade canal, altera datos ni fabrica una toma previa.

Campaña continua **PASS436**,270,88s, cuatro snapshots,12101 muestras, cero fixtures
ni reanudaciones ocultas. Aisladas: mínima **PASS66** (pausa registrada2,07548s),
reglas **PASS147** (vidrio cerrado0,760s, toma corta/orientación, pausa y R),
superficie **PASS71** (color desactivado/proxy activo explícitamente). Las imágenes
del recorrido completo confirman la composición con la casa ya oscurecida.
Esto no acredita miedo, claridad, confort ni audición: permanecen **EN PRUEBA**.

Console0 errores/4 advertencias. Build `build_d0cd27a3e7aa`: éxito universal,
197325757 bytes,16,19s,0 errores/346 advertencias Pipeline/Sentis Metal; sin
advertencias de los dos shaders Presence. Firma estricta comprobada. CUA volvió
a reportar Mac bloqueado: arranque e input nativos pendientes, sin lectura de
Player.log atribuida a esta build. Reserve sigue siendo el último respaldo nativo.
Siguiente intervención: atravesar el archivo en el retornoR3 para romper la
composición uniforme de la galería. Es dirección aprobada internamente, aún no escena.

ZIP Encounter SHA-256: `e5084cd822352e756adcf2505bcb3922e539a6c3e59d23c4ffcfaeb88dede443`. App compilada/firma comprobada; entrada nativa pendiente.

Respaldo fuente Encounter: `../EchoProtocol-backups/encounter-checkpoint-20261001/`, 3329 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Records: atravesar el archivo

El retornoR3 pasa por la sala antes decorativa. Dos vanos de1,72m libres y una
partición permanente visible lo hacen transitable sin nuevo puzzle ni evento.
Se añaden suelo/perímetro/estantes sólidos separados del render. La esclusa y
los datos del Echo conservan sus reglas. Las primeras cámaras motivaron corregir
linóleo estirado y orientar el foco sur hacia el vano; no se elevó intensidad.
Dos fuentes fijas de lámpara reutilizan el master CC0 ya trazado en Ambience.

Primera aislada PASS14 preservada; seleccionada **PASS18**,23,43s, con fixture
inicial explícita y marcha real posterior en ambos sentidos. Verifica piso,
partición, cesación de pasos y anclajes/gain/mono de fuentes, no audibilidad.
Campaña completa **PASS446**,275,11s, cuatro tomas,12571 muestras, cero fixtures
ni reanudaciones ocultas. El tramo automático z29→13 agrega3,442s con paradas/
capturas adicionales; no valida el presupuesto humano preliminar de2s. La ruta
geométrica suma5,1m. Claridad, ritmo, confort, mezcla y miedo siguen **EN PRUEBA**.

Console0 errores/4 advertencias. Build universal `build_eb12e7f68c8f`,197349341 bytes,
17,29s,0 errores/346 advertencias Pipeline/Sentis Metal. Firma estricta y arquitecturas
comprobadas. Mac bloqueado en el intento CUA: arranque/controles nativos pendientes;
Reserve sigue siendo el último checkpoint con esa prueba. Siguiente trabajo:
límites acústicos acotados de teléfono/frigorífico; después, exposición natural de
la presencia doméstica sin convertir mirarla en condición para salir.

ZIP Records SHA-256: `93eefb5ccbf2226601c1a9218b052c7a58fc5ee4eabd2cb4e68eab44843f3342`. App compilada/firma comprobada; entrada nativa pendiente.

Respaldo fuente Records: `../EchoProtocol-backups/records-checkpoint-20261001/`, 3462 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Rooms: continuidad acústica entre habitaciones

Dos filtros explícitos siguen el camino directo a teléfono/frigorífico a través
de47 colliders arquitectónicos existentes. Sólo escriben frecuencia de corte;
no alteran reproducción, ganancia, potencia, anclajes ni Echo. Sonda **PASS37**,
21,04s/4119cuadros, una fixture inicial y ningún reset posterior. Pausa real a
1873,226Hz durante72cuadros;34valores de transición; E corta timbre y frigorífico.
No se repite la campaña: última continua Records PASS446, claramente heredada.

Build `build_495c531cb144`,197353981bytes,16,51s,0errores/346warnings Pipeline/Sentis;
universal y firma estricta pasan. Entrada nativa pendiente por Mac bloqueado,
sin repetir intento CUA ni atribuir Player.log. Console0errores/2warnings tras sonda.
La audición, timbre y emoción siguen EN PRUEBA. Próximo: encuadre natural del estudio.

ZIP Rooms SHA-256: `2ed26838952f23bf630d601b7e7d57bfd3b73fb2ccecbe7d7b7982cd34dbfaee`. CRC y ejecutable comprobados; nativo pendiente.

Respaldo fuente Rooms: `../EchoProtocol-backups/rooms-checkpoint-20261001/`, 3490 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Study: exposición durante el regreso

El paño sellado se amplía hacia el norte y el cuerpo/fuente de peso/volumen se
acercan a x1,65. El recorrido y las referencias acústicas continúan intactos.
Safety incluye el calzado; sólo la figura ambiental deja de proyectar sombras
fuera de él. No hay luz nueva, giro forzado, requisito de mirada ni canal del Echo.
Dos ramas por controles, con una fixture inicial cada una: **PASS24/17**, exposición
geométrica0,6360/0,6344s durante marcha sin Aim al cuerpo, máximo delta0,0106/0,0101s.
Cuatro cuadros por rama muestran cabeza/torso y crecimiento de escala. La primera
copia con inversión Metal/salto0,3333s queda preservada y excluida de selección.

Campaña completa **PASS446**,274,66s, cuatro tomas/13043 muestras, cero fixtures ni
reanudaciones ocultas. Esta ruta aún apunta al torso para comprobar persistencia;
la prueba de exposición natural son las ramas focales separadas. Console0errores/
4warnings. Build `build_ed6a158cc23f`,197354349bytes,16,69s,0errores/346warnings
Pipeline/Sentis Metal; universal y firma estricta pasan. CUA vuelve a informar Mac
bloqueado antes de abrir: nativo pendiente y sin Player.log atribuido. Reserve
sigue siendo la última prueba nativa. Todo efecto humano permanece EN PRUEBA.
Próximo: comparar un material local con highlight real en el vidrio sutil del estudio.

ZIP Study SHA-256: `1df19c333a0127d152742031d2a95cd58b253e1bf35ecfa33732b4e232549eb4`. CRC y ejecutable comprobados; entrada nativa pendiente.

Respaldo fuente Study: `../EchoProtocol-backups/study-checkpoint-20261001/`, 3613 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Pane: variante visual seleccionada, aún sin build propia

Material local de Study glass, highlights habilitados y smoothness0,78. Reflejo
real de luces existentes sobre la cabeza, perfil del cuerpo conservado. Ramas
por controles PASS25/17, una fixture cada una. Console0errores/1warning. No cambia
alpha, física, luz, escena reflejada ni Echo; no se repite la campaña de Study.
El salto máximo0,1678s del caso oscuro ocurre después de la visibilidad, en z−61,248;
no es evidencia de rendimiento estable. Pane será base de la próxima mejora de
lectores ópticos; Study sigue siendo el respaldo compilado. El antiguo bezel de
los lectores es macizo y oculta la lente neutral: corregir a anillo hueco/lente
profunda/señal anular, manteniendo todas sus referencias y reglas.

## Readers — instrumentos visibles y montajes corregidos

Deriva Pane sin perder Study recuperable. Seis cabezales ópticos originales
reemplazan discos sólidos por bezel hueco, lente hundida y anillo del mismo color.
La aproximación real mostró además oscuridad sobre las caras, el cartel tapando
B y una cortina atravesando P2. Se ajustan tres luminarias del mismo circuito,
baja el cartel, y cortina/riel se separan0,95m del lector. Pliegues y una pequeña
placa impresa sustituyen el bloque textil plano y la instrucción flotante.

A/B/C: PASS62,3894 frames, un R real, cero fixtures. Primera ruta completa
interrumpida por sobrepaso de waypoint durante una consulta al Editor; CSV y
cámaras preservados. Repetición sin consultas: **PASS456**,277,626s automáticos,
cuatro tomas,13721 frames, cero fixtures y cero reanudaciones ocultas. P2/P3,
apagón, archivo, aislamiento, vivienda y final comprobados. Console0 errores/4
advertencias. Esto no es prueba humana de descubrimiento, ritmo, escucha o miedo.

Build seleccionada `build_50312345f761`: universal198119837B,7827ms,0 errores/1
advertencia de Pipeline Editor deshabilitado en Player. Firma estricta correcta.
CUA volvió a encontrar el Mac bloqueado antes del lanzamiento; no hay input
nativo Readers. Reserve sigue siendo la referencia nativa. Próximo avance:
Shutdown, composición y movimiento visible de la instalación al aislarla.

Respaldo fuente Readers: `../EchoProtocol-backups/readers-checkpoint-20261001/`, 3896 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.


## Shutdown — control, máquina y paso

El mando se baja15cm y gira15° manteniendo sus referencias, apoyo y collider;
el gabinete/baffle abre un pasillo bruto2,515m junto a las bombas. Una luminaria
de reserva existente se monta y orienta hacia la máquina; su potencia seleccionada
es2,4. BombaA incorpora carcasa abierta, rejilla fija y rotor original952tri:
sólo lee MotorLevel01 y se congela al pausar/cero. Motor, purga, puerta y Echo
conservan sus propietarios/reglas; el frenado sigue2,2s y la descarga3s.

Campaña PASS462/277,353s/cuatro tomas/13.588 frames contrastados, cero fixtures,
Console0errores/4advertencias. Focal PASS20/17,602s con una fixture inicial y
cero resets posteriores; incluye pausa durante coast. Continuación PASS45 con
cuatro recargas, dos fixtures de capítulo y una de pose: E real guarda durante
purga, restaura marcha/reposo sin replay y repone preferencias previas.

La inspección detectó dos problemas del acabado de texto: Awake reemplazaba el
material elegido y el shader dependía del atributo normal de los glyphs. Referencias
explícitas y normal del plano XY corrigen nueve placas, P2 y la tinta compartida
de Reserve. La campaña y el focal preceden sólo a esta corrección visual final;
cámaras estacionarias separadas la verifican en P2, reserva y control encendido/
apagado. Sus fixtures no se presentan como otra ruta jugada.

Build build_c06938d5008b: Succeeded198.744.081bytes/26.458ms,0errores/346avisos
de paquetes Sentis/Pipeline. Universal y firma estricta verificados. CUA encontró
el Mac bloqueado antes del lanzamiento; no hay input nativo atribuido a Shutdown.
Reserve conserva esa evidencia breve. Movimiento aparente, escucha, comprensión,
confort y miedo siguen EN PRUEBA. Continúa R1/Orientation; ninguna build cierra
el objetivo sostenido. Detalles: VisualEvidence/2026-10-01-Shutdown/.

ZIP Shutdown SHA-256 `902ce2e90d028f610b062b4346021ce8b1fae25b2e5aeae6c6f821df4ad36cd9`; CRC/ejecutable comprobados. Entrada nativa pendiente por bloqueo del Mac.

Respaldo fuente Shutdown: `../EchoProtocol-backups/shutdown-checkpoint-20261001/`, 4108 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Orientation — R1 seleccionado para Arrival

Puesto oblicuo, acceso despejado, luz local existente y dos placas físicas. Mismo origen mundial de RECALL y mismas referencias/interacciones. Focal final PASS10/5,464s, una fixture inicial y cero resets posteriores; las cámaras comparan estado neutral y marcha real. No se atribuye descubrimiento espontáneo ni recuperación emocional. Sin build propia: Shutdown conserva la entrega recuperable.

Próxima reconstrucción: enlace final de R3 y esclusa girados para que el regreso encuadre la recepción original en la dirección de marcha. Inside de la esclusa pasa de AABB a caja local (PASS9 de diagnóstico con cinco poses explícitas); anuncio de llegada recibe la misma corrección acotada, aún pendiente de recorrido. La escena Arrival todavía no está integrada.

## Arrival — recepción en la marcha de regreso

La última conexión conserva archivo y gira la esclusa45°. Las hojas, headers,
cuatro boxes y motores reales se trasladan juntos; luces/sonidos/reglas no cambian.
Los restos del muro oeste se recortan tras cámara. El mostrador original queda
delante al abrir la hoja. R1 recompuesto está integrado.

Focal17/21 checks con una fixture inicial; la rama21 incluye retirada durante
cierre, reapertura segura y ausencia de repetición del aviso. Campaña completa
PASS466/278.948s/4 tomas/12913 frames, cero fixtures o reanudaciones
ocultas. Build build_2664e8d1c23e Succeeded, 198783585B/19108ms,
0errores/346warnings; universal y firma estricta comprobados.
Estado nativo: Pendiente: CUA intentó abrir esta ruta exacta y devolvió Mac locked antes del arranque. No se afirma prueba nativa de Arrival; Reserve conserva la última prueba nativa breve.

No se afirma experiencia humana. P3 es la siguiente revisión sustancial;
ninguna build cierra el objetivo sostenido.

Paquete Arrival: `Builds/EchoProtocol-Arrival-20261001.zip` (79378503B), CRC/permisos ejecutables comprobados; SHA256 `829a4652f480d4f949e3ea8ee63e9cc2afdc0accb00e4c00deabb05445ecb6a5`.

Respaldo fuente Arrival: `../EchoProtocol-backups/arrival-checkpoint-20261001/`, 4317 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Observation — cooperación vista a través de arquitectura real

La entrada apunta al recorrido que se graba; ventanas y núcleo organizan la
separación del Echo. La primera cámara B motivó reubicar su montaje y una luz
existente. Motores causales usan masters trazados, sin un nuevo canal Echo.

Campaña PASS468/276.471s/4 tomas/12162frames,
cero fixtures/reanudaciones ocultas. Focal PASS41 con una fixture inicial/dosR;
bypass separado en evidencia. Build build_3e5e960d9c2e Succeeded, universal/firma
estricta comprobadas, 346warnings/0errores. Nativo: Pendiente: CUA intentó abrir esta ruta exacta y devolvió Mac locked antes del arranque. No hay prueba nativa de Observation; Reserve conserva la última prueba nativa breve.

Experiencia humana EN PRUEBA. Próxima mejora: salida exterior compacta de la
vivienda hacia portería, que muestre el estado real dejado en la casa.

Paquete Observation: `Builds/EchoProtocol-Observation-20261001.zip` (79388567B), CRC/permisos ejecutables comprobados; SHA256 `8622bc91b11814fb4d35b06c4339726eee4a60bb21b2e17f7f1812968fac7957`.

Respaldo fuente Observation: `../EchoProtocol-backups/observation-checkpoint-20261001/`, 4478 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Courtyard — salir y reconocer la casa real

Patio lateral compacto con la ventana original y luz doméstica persistente.
Portería y final trasladados como una unidad, sin cambiar grabaciones ni reglas
de encuentro. La primera cámara motivó luz local, suelo original con juntas y
canaleta/bajante/desagüe; se corrigió winding de la combinación tras inspección.
Una fuente exterior fija y trazada conserva pausa/mixer y se retira en interiores.

Campaña PASS480/285.498s/4 tomas/11500frames,
cero fixtures/reanudaciones ocultas. Focal final dormitorioPASS30, una fixture
inicial/cero resets; recibidor previoPASS27 antes de corregir winding del suelo.
ContinuePASS341/10cargas/5fixtures, preferencias restauradas.
Build build_399e7e7f4ac5 Succeeded, universal/firma estricta,346warnings/0errores.
Nativo: Prueba breve en la app exacta: título, créditos completos con ahriik/CREDITS.txt, entrada WASD que inicia el registro, RECALL de 7.0 s, pausa Escape, retorno y salida Cmd-Q comprobados en la interfaz. No se recorrió la campaña completa en nativo ni se juzgó la mezcla por oído.

Experiencia humana EN PRUEBA; checkpoint conservado, goal activo. Se prepara
viabilidad de una estancia SE2 transitable como siguiente mejora sustancial.

Respaldo fuente Courtyard: `../EchoProtocol-backups/courtyard-checkpoint-20261001/`, 4714 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; drafts conservados como fuente, sin afirmar integración.

## Candidate — cierre acotado de entrega

El objetivo actualizado reemplaza consumir cuota por cerrar una candidata. SE2
se recorre físicamente; protege la ocultación de presentación y sombras, conserva
el cuarto después de RECALL y no exige mirar. H es contextual y pausa sesión/audio.
La revisión encontró y corrigió una referencia de ayuda al antiguo SE2 inactivo.
Campaña seleccionadaPASS522,4tomas/10574frames/0fixtures; ContinuePASS341/10cargas/
5fixtures. Build f94da6cff01a,206854641B/20453ms/0errores/346warnings; universal y
firma local. Prueba nativa breve incluye12.4s de toma, RECALL, ayuda/modalidad,
pausa y salida; no campaña nativa completa ni audición. Informe breve en
DELIVERY_CANDIDATE.md. Se empaqueta y respalda la candidata para cerrar la etapa.

Respaldo fuente Candidate: `../EchoProtocol-backups/candidate-checkpoint-20261001/`, 5332 archivos; manifest y SHA junto al archivo. Excluye caches/builds/Git; incluye escenas y fuente de recuperación; los drafts históricos no implican integración.

# Readers — presentación óptica seleccionada

2026-10-01. Escena derivada de Pane, compilada y respaldada; Study se conserva como checkpoint anterior.
Seis cabezales reciben carcasa biselada, bezel realmente hueco, lente fija
hundida y señal anular. Los objetos activos conservan sus referencias y transforms.
La autoría comprobó que las serializaciones de los seis OpticalReader, los607
colliders y las54 fuentes de audio permanecen idénticas. El Echo no suma canales.

La pasada previa de Pane, `../2026-10-01-Pane/reader-survey`, recorrió desde el
menú la carta de recepción y A/B/C mediante teclado/ratón de Input System.
PASS62, un RECALL real,3519 frames contrastados con la toma, cero fixtures.
Las seis cámaras nuevas aíslan aproximación inactiva y alineación efectiva;
se orientan deliberadamente hacia cada aparato y no prueban descubrimiento humano.
Revelan dos defectos adicionales a la lente tapada: luz incidente insuficiente y
el cartel de RECALL ocultando B. El resultado del probe conserva una frase de
alcance genérica sobre el retorno de archivos, que no se recorrió en este modo.

En Readers el cartel baja0,52m, junto con su respaldo. Tres luminarias existentes
conservan posición, circuito y propietario: sus conos se abren a110°/85° y se
orientan hacia suelo/pared; la de servicio pasa0,7→1,15. Las otras dos conservan1,55.
No se sube el ambiente global ni se agrega una luz sin fuente visible.

**Comparación seleccionada:** `reader-survey` PASS62, un RECALL real,
3894 frames de playback cotejados, cero fixtures,28,821s de ruta automática.
A/B/C se capturaron inactivos y alineados desde el control real. A tiene una
lente visible en aproximación; B muestra simultáneamente señal y recordatorio;
C conserva un cuerpo legible. La señal usa el mismo material/color anterior,
con menor superficie encendida. Se acepta esta presentación desde las cámaras,
no como prueba humana de reconocimiento espontáneo.

**Estado actual:** integrado y compilado en Editor; campaña completa comprobada.
La primera llamada de autoría superó el timeout de5s del conector y aun así guardó
la escena completa; se comprobó el archivo limpio y el reporte. El único error
observado entonces era ese timeout de Pipeline, no un error de geometría o gameplay.
La build seleccionada se detalla al final; no hay prueba nativa o aceptación humana.

## Corrección de P2 y repetición justificada

La cámara de la primera ruta completa mostró que `Curtain Panel` ocupaba el
frente del aparato P2. La cortina y su riel se desplazan0,95m hacia el sur; sus
colliders existentes acompañan ese desplazamiento. Un mesh original de pliegues
reemplaza el rectángulo plano, dentro del espesor anterior. La instrucción mantiene
sus palabras y pasa a una placa pequeña impresa sobre la pared, fuera de la tela.
Por ello, el reporte inicial de607 colliders idénticos describe la primera autoría
de cabezales; no afirma que los límites mundiales de la cortina sigan en su lugar.

`continuous-initial-hitch` conserva el fallo de P3: el Player cruzó realmente el
plano durante un cuadro demorado, corrigió hacia atrás y contradijo la expectativa
del waypoint. La consulta concurrente al Editor es la causa probable de la pausa
de~0,5s; no se alteraron las reglas de R3 para hacer pasar la prueba. Se repite la
campaña seleccionada sin consultas Unity durante el movimiento.

## Campaña seleccionada

`continuous`: PASS456, 277.626s, 4 tomas, 13721 frames de playback cotejados, cero fixtures y cero reanudaciones ocultas. La ruta incluye P1/P2/P3, apagón de las tres luminarias ajustadas, regreso físico por archivo, aislamiento, vivienda y final6B. Las cámaras P2/P3 muestran los montajes despejados. La prueba usa controles sintetizados y orienta deliberadamente la cámara hacia los lectores; no acredita descubrimiento humano ni escucha.

Console al terminar la ruta seleccionada: cero errores/cuatro advertencias.
La cámara P2 posterior confirma carcasa y lente libres, texto sobre su placa y
pliegues separados del instrumento. La build inicialSucceeded tuvo una advertencia
adicional por comentarios/reportes Editor aún sin recompilar; se recompila antes
de elegir la app archivada. Ese primer reporte se conserva en `build-initial.json`.

## App seleccionada

`build_50312345f761`: Succeeded,198119837 bytes,7827ms, cero errores/una
advertencia del conector Pipeline deshabilitado en Player. La compilación
incremental posterior al refresco ya no incluye la advertencia de código
pendiente. App universal x86_64/arm64, firma local estricta comprobada.

CUA intentó abrir la ruta exacta de Readers y respondió **Mac bloqueado antes
del lanzamiento**. No se atribuye arranque, input o Player.log a esta app. Reserve
sigue siendo el último checkpoint con entrada nativa breve. No se repite la
solicitud de desbloqueo pendiente ni se detiene el trabajo independiente.

ZIP recuperable: 79130813 bytes; CRC y permiso ejecutable verificados.
SHA256 `ca7d22741ea3e8e3b61813016c5e53e350020dbfeac7b627b76ad88216573390`. LEEME y créditos incluidos.


Follow-up found during Shutdown: P2's static renderer material was set to lit
ink here, but its inherited DiegeticText Awake owner still replaced it with
OneSidedText. Thus Readers' in-game P2 lettering was unlit despite its physical
plate; no human approval was claimed. Shutdown corrects the serialized shader
owner. The preserved Readers app/source remain historical evidence.

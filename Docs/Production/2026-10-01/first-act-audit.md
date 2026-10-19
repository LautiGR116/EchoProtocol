# Primer acto: tres intervenciones para la primera partida

2026-10-01. Auditoría de código, autoría y capturas existentes; no se ejecutó
Unity ni se escuchó la mezcla. Se leyó el objetivo actualizado de producción
sostenida. Finish es referencia recuperable; la casa Residence ya ofrece una
escala y materialidad mejores. Las propuestas siguientes no son aprobación
humana ni conclusiones extraídas de una partida nueva.

## 1. Volver a enseñar la cooperación dentro del espacio

**Carencia comprobable:** Finish desactiva `Protocol introduction`, su tablero
y `Protocol start rules`, reduce las reglas de P1 a A/B/C y cambia la indicación
de P2 por «PRESENCIA CONTINUA». El contrato completo queda en H.
`GameSession` muestra el recordatorio de controles durante veinte segundos,
pero «R RECALL» no explica por sí solo que debo recorrer ambos lectores y
volver por el otro lado. La [recepción](../../VisualEvidence/2026-09-30-Finish/D1_00_fresh_Hub.png)
presenta dos accesos equivalentes; sus nombres distinguen zonas sin enseñar la
relación temporal. La aprobación histórica de P1 proviene de un creador que
conocía la solución: `PLAYTEST_LOG`, 14/8, mantiene pendiente la comprensión sin
ayuda. No se afirma que una persona nueva haya fallado en Threshold.

**Intervención recomendada, pequeña:** tres placas ligadas al lugar de uso,
manteniendo el circuito y su útil demostración de puerta A:

- Inicio del lado izquierdo: «REGISTRO / MIRÁ A Y B».
- Junto a B, visible desde su posición válida: «R / VOLVER Y REPRODUCIR».
- Al retornar al origen, acceso derecho: «ACCESO / COOPERÁ CON TU RECORRIDO».

La primera puerta debe permanecer visible junto al lector durante su preview;
la señal cian necesita contraste suficiente desde la posición de lectura. Eso
ya es parte del contrato humano aprobado; no cambiar latch, tolerancias ni
campos grabados. Las placas sustituyen ambigüedad, sin añadir un documento
obligatorio, narrador, objetivo emergente ni párrafo de tutorial. Mantener H.
La misma gramática permite rotular P2 «LA PUERTA NECESITA UNA MIRADA CONTINUA»
sin prometer que cualquier pausa corta alcanzará para cruzar.

**Criterio:** desde cámara de juego, cada placa se lee en su posición de uso y
la preview enseña causa/consecuencia sin abrir un bypass. Una primera partida
debe comprobar si el jugador descubre R y cambia de carril sin indicación externa.

## 2. Entregar el nuevo destino cuando el jugador vuelve al Hub

**Defecto encontrado y corregido en código:** Threshold deshabilita
`VS1DenialEvidence`, que antes activaba la presentación de revisión después de
cruzar dos volúmenes de llegada. `CampaignDirector` había sustituido esa
disponibilidad por `R3.IsOpen`; por eso anunciaba el nuevo registro al salir de
P3, antes del regreso de aproximadamente 56 m. El aviso dura cinco segundos y
ya habría terminado al alcanzar la recepción. El probe continuo encuentra el
terminal por coordenadas; no constituye evidencia de orientación espontánea.
El [retorno histórico](../../VisualEvidence/2026-09-30-Finish/R3_exit_natural.png)
y la confusión del creador registrada el 28/9 hacen que esta regresión merezca
prioridad sobre más decoración.

**Implementado, pendiente de integración y ejecución:** `CampaignDirector`
ahora exige `R3.IsOpen` y al Player activo dentro de `returnArrivalZone` para
mostrar una vez «Subsuelo. El terminal está junto a la salida.» y activar el
GameObject opcional `returnDescentGuide`. Se conservan la habilitación mecánica
del terminal y la rama legacy D1. La guía arranca apagada y se apaga al salir de
Protocol, también mediante restauración de checkpoint. No depende de una toma
ni inventa progreso doméstico.

**Autoría propuesta:** volumen trigger con centro `(-3.45, 1, -9.5)` y tamaño
`(2.4, 2.2, 2.3)`, dentro del Hub, después del frente del interlock. Una placa
«SUBSUELO» y un piloto deben verse al salir mirando hacia el este y dirigir la
vista hacia el terminal sur. El grupo debe contener su luz y su superficie
luminosa; no dejar emisión visible cuando esté apagado. Sin volumen asignado
no hay aviso prematuro; el acceso sigue siendo usable.

**Criterio:** ninguna señal nueva al abrir R3 a distancia; una sola entrega al
entrar físicamente al Hub; sin duplicación tras R, pausa o restauración. Capturar
el mismo punto de salida usado por el recorrido, sin reorientar artificialmente
la cámara para favorecer el letrero.

## 3. Dar una razón perceptible al regreso largo

**Carencia visual:** la [vista longitudinal de R3](../../VisualEvidence/2026-09-30-Finish/R3_07_continuous_return.png)
es un túnel recto de pared, piso y luminarias repetidos hasta el fondo. La ruta
real conserva la recta de `z=46.35` a `z=-9.5` en `x=-18.6`: unos catorce segundos
de marcha ideal a 4 m/s, más giros y vestíbulo. No es una duración humana medida.
Su vacío puede ser intencional; hoy la captura tampoco ofrece un hito que
transforme ese vacío en un lugar reconocible. Frente a la
[ventana, radiador y teléfono de Residence](../../VisualEvidence/2026-10-01-Residence/house-02-phone-before-Camera.png),
la diferencia de elaboración salta a la vista. Una superficie nueva no resuelve
la repetición de volumen y propósito.

**Intervención recomendada:** conservar la conexión física, pero convertir la
recta en tres tramos reconocibles con el kit existente: inicio de galería
clínica, un único paño lateral de archivo sellado con estantes/cajas detrás de
vidrio, y remate de recepción identificado por carpintería y luz del mismo Hub.
Una modulación de techo y dos cambios de agrupación de lámparas bastan para
romper el punto de fuga uniforme. El paño intermedio debe dejar ver un lugar de
trabajo vacío, no otro maniquí. Reutilizar modelos/materiales disponibles,
mantener el paso y las colisiones fuera del dressing. Evitar sumar otro evento:
el tramo funciona como recuperación y reconocimiento después de SE3/P3.

En sonido, proponer sólo una transición física suave desde ventilación de
archivo hacia el ambiente ya conocido de recepción, con fuentes asociadas a
rejillas o luminarias visibles. Eso es dirección propuesta, no diagnóstico de
una mezcla escuchada; no compensar con volumen ni pasos atribuidos al Echo.

**Criterio:** tres capturas desde el recorrido distinguen los tramos y la última
anticipa la recepción. El regreso continuo y el interlock siguen pasando. La
sensación de alivio, amenaza o tedio necesita juicio humano.

## Riesgos del recorrido continuo y orden de ejecución

Primero integrar la guía de retorno, luego las placas iniciales, después el
único paño arquitectónico del regreso. Son cambios locales que no exigen
rehacer P1/P2/P3 ni el Echo. La captura `03-P1.png` y otras del recorrido Finish
preceden la sustitución final de lentes redondas; no usar su caja antigua como
prueba de la geometría actual.

El probe conserva pausas de 22 s en P2 y 25 s en P3 porque son soluciones
conocidas, no tiempos óptimos ni medida de ritmo. El largo retorno consume
tiempo real de la nueva grabación posterior a R; no existe una suspensión
automática en la base heredada, así que un jugador que explore puede agotar los
60 s y volver a R1. La pasada continua rápida no resuelve esa fricción.

**Actualización del mismo día — implementado, pendiente de integración/prueba:**
`ReturnRecordingBoundary` es un componente opt-in que suspende Loop y cancela
únicamente la toma nueva sin cerrar al abrirse R3 mediante su cruce real. No
cambia `LastRecording`, el Echo existente ni la posición del Player. El próximo
checkpoint explícito de Presión rearma normalmente. `CampaignDirector` expone
el estado «FIN DE REGISTRO · R INACTIVO» y la dirección del siguiente origen.
El componente requiere una placa física en la frontera y referencia opcional
desde el director; las escenas legacy sin él conservan su comportamiento.
`ReturnRecordingBoundaryProbe.Run(65, true)` prepara una toma auténtica con
input, omite explícitamente sólo el cruce P3 para probar la frontera aislada,
espera más que el límite sin acelerar el reloj, prueba pausa/R y luego E en
PressureOrigin mediante una fixture espacial declarada. No sustituye el
recorrido completo ni implica que se haya resuelto P3.

La nueva prueba falla ante una pausa inesperada de foco. Eso debe registrarse
como interrupción de la ejecución, no como atasco del juego ni recuperación
silenciosa. No se cambiaron sus pruebas en esta auditoría.

Chat: actual · Model: GPT-6-Astra · Effort: xhigh.

# Study — la presencia entra en el regreso natural

2026-10-01. Escena derivada de Rooms. El paño del estudio se amplía hacia el norte
y la figura queda más cerca detrás del vidrio sellado. El Player conserva cámara,
camino y salida libres; no existe condición nueva de mirada ni canal del Echo.
Figura, fuente existente de peso y volumen conservador mantienen su relación
física. La malla ambiental no proyecta sombras fuera del volumen; el Echo no cambia.

## Selección desde controles y cámara

`natural-return/answered`: **PASS24**, una fixture inicial explícita de capítulo
anterior/iluminación/posición; luego entrada, atención, reserva, llave y salida
mediante Input System. La vuelta mantiene yaw180/pitch0 sin apuntar a la figura.
Centro/cabeza tienen una oportunidad geométrica de0,6360s en146 cuadros, con delta
máximo0,0106s. `unanswered`: **PASS17**, misma libertad con suministro general;
0,6344s en123 cuadros, delta máximo0,0101s. Ambas rutas salen y conservan su elección.
El cue de peso existente comienza aproximadamente0,095s antes de la primera línea
clara de cabeza/centro. No se ha escuchado ni aprobado por oído.

Cuatro cuadros Game por rama se copian al final del cuadro sin soltar W. El proceso
se precalienta antes de entrar; ReadPixels/PNG suceden después de salir. Orientación
Metal corregida sólo al guardar el diagnóstico. `answered-initial-raw` conserva la
primera pasada invertida con un salto de0,3333s por primera copia; no es la selección.
Las duraciones suman deltaTime observado, no tiempo humano ni conteo de píxeles
visibles. Las capturas muestran cabeza/hombros/torso crecer al aproximarse; no
prueban que una persona los descubra o sienta miedo. El vidrio se lee de forma sutil.

Contención comprueba bounds transformados de la malla habilitada, no el proxy legacy
apagado. No hay luz hija activada con la figura; la iluminación residual es permanente.
La rama unanswered también exige shadowCastingMode Off en ejecución. La anterior
answered registra Off en safety.txt pero precede esa assertion adicional. No se
atribuye a la prueba de bounds una certificación de reflexiones/sombras arbitrarias.

## Regresión y checkpoint

`continuous`: **PASS446**,274,66s, cuatro tomas reales/13043 muestras de reproducción,
cero fixtures y cero reanudaciones ocultas. Título a final con los filtros Rooms,
archivo Records, maquinaria y encuentro directo de6B. La mirada deliberada al
estudio en esta prueba se conserva para verificar persistencia/ocultación; la
exposición sin Aim está demostrada sólo por las dos ramas focales anteriores.
Console final: cero errores/cuatro advertencias. Escena guardada y build universal
`build_ed6a158cc23f`:197354349bytes,16,69s, cero errores/346 advertencias Pipeline/Sentis
Metal. Intel/Apple Silicon y firma estricta comprobados.

**Entrada nativa pendiente:** al intentar abrir esta app, CUA informó que el Mac
sigue bloqueado antes del lanzamiento. No se atribuye arranque ni Player.log a
Study. Reserve conserva la última prueba nativa breve. Claridad, descubrimiento,
audición, confort e impacto emocional humanos permanecen **EN PRUEBA**.

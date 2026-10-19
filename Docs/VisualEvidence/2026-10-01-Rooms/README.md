# Rooms — paredes que separan las fuentes domésticas

2026-10-01. `EchoProtocol_Rooms.unity` deriva de Records. Dos filtros explícitos
atenúan agudos cuando el camino directo de cámara a teléfono/frigorífico cruza
arquitectura real. 47 BoxColliders existentes, 22.000/1.600 Hz y transición
logarítmica de0,20s. No cambia ganancias, distancia, reproducción, mixer, posición,
reglas domésticas ni datos del Echo. Es una aproximación espectral; no simula
propagación por portales, difracción o reverberación. No se añade ningún master.

## Evidencia y alcance

`house-acoustics`: **PASS37**,21,04s, una fixture inicial de capítulo anterior,
iluminación y posición; luego entrada, marcha, mirada, E y Escape mediante Input
System. Cero colocaciones posteriores. 4119 cuadros registrados; pausa real con
filtro a1873,226Hz estable durante72 cuadros;34 valores intermedios distintos.
Se comprueban caminos bloqueados/abiertos de ambas habitaciones, conservación de
fuentes/configuración/volumen y corte real del timbre/frigorífico al atender.
Las cámaras son del Player y omiten OnGUI. No prueban audición ni localización.

Compilación y Console: cero errores/dos advertencias al terminar la sonda.
Build universal `build_495c531cb144`:197353981 bytes,16,51s, cero errores/346
advertencias Pipeline/Sentis Metal. Intel/Apple Silicon y firma estricta comprobados.
**Nativo pendiente:** el Mac sigue bloqueado; no se repitió el intento fallido de
Records ni se atribuye un Player.log a Rooms. Reserve sigue siendo la última app
con entrada nativa comprobada. No se repitió la campaña completa para estos dos
filtros: la evidencia continua heredada corresponde expresamente a Records
(PASS446). La siguiente integración de geometría tendrá su propia regresión.

Timbre, balance, reconocimiento de la esquina, comodidad, miedo e inmersión siguen
**EN PRUEBA**. Escuchar con auriculares a volumen cómodo y altavoces/dinámica reducida.

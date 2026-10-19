# Integración Courtyard — 2026-10-01

Orden real: `HouseCourtyardProduction.Author` deriva Observation, después
`CourtyardFinishProduction.LightExterior`, `IntegrateAir` y
`CourtyardRainwaterProduction.Integrate`. Fuente congelada del primer autor en
el nivel superior; integraciones posteriores están aquí y en Assets/Editor.

La primera cámara demostró que el exterior necesitaba luz y escala: los dos
puntos locales de cielo terminaron en4.5/3.5, y el suelo se reemplazó visualmente
por119 piezas minerales recortadas, con juntas/desagüe enrasado. La colisión
continúa siendo el suelo original. Dos meshes combinadas conservan instancias
fuente inactivas para edición. El giro tiene margen para la hoja abierta95°.

La combinación inicial invirtió dos veces el winding de las mallas reflejadas:
la luz mostraba cantos y caras negras. Se corrigieron por Editor API los dos
assets; el autor actual usa el manejo de reflexión de CombineMeshes y verifica
la concordancia de cada triángulo con las normales exteriores. Evidencia
`selected-mesh-diagnostic.txt`; el primer resultado se conserva separado.

RainwaterKit conserva tubo/codo abiertos, canaleta de salida conectada y ranuras
reales, con tapas agregadas en sus extremos visibles. El anchor de entrada está
a menos de2mm del fondo de salida. Fuentes/contratos del Blender original en
`ArtSource/Blender/Courtyard/`, sin dependencias externas nuevas para geometría.

Audio: `CourtyardAir` controla sólo una fuente nueva y dos regiones solapadas.
Master trazado38.039s, ganancia constante y unión de1s; no lo registra Echo. El
listener/mixer/pausa y las fuentes domésticas conservan sus dueños. Se comprobaron
por input real ganancia, posición, vuelta al interior y congelación en pausa;
la audición, localización percibida y comodidad continúan EN PRUEBA.

La cámara focal se alinea ahora a z−52.3, cerca del eje real del portal, para
comparar recibidor encendido/apagado a través de la ventana. Es una dirección de
inspección, no una obligación de mirada añadida al juego. No se modificaron las
cortinas para obtenerla. Ninguna captura o check demuestra reconocimiento humano.

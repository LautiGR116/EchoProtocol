# RainwaterKit — original de Courtyard

Fuente original reproducible, 2026-10-01. Geometría construida con Blender y
primitivas/mallas matemáticas propias; sin recursos externos, servicios ni compra.
`build_rainwater_kit.py` escribe únicamente esta carpeta. `.blend` guarda las
cinco piezas editables con sus nombres; cámaras, suelo y pared de las previews
son contexto y no se exportan. No hay colliders, audio, luces o scripts en FBX.

## Entrega

| FBX bajo exports/ | Tris | Dimensión / root |
|---|---:|---|
| EP_CY_Downpipe27.fbx | 2.880 | Entrada a Z2.700; root en suelo/pared; 84mm exterior/78mm interior |
| EP_CY_GutterStraight1m.fbx | 692 | X−.5…+.5; root a cota del fondo; sección abierta |
| EP_CY_GutterOutlet1m.fbx | 1.064 | Mismo módulo; abertura real y manguito hacia abajo |
| EP_CY_MineralTile0744.fbx | 108 | .744×.744×.040m; bevel6mm; cara superior/rootZ0 |
| EP_CY_Drain1m.fbx | 2.916 | 1.000×.120×.036m; superficie/rootZ0; cuerpo hacia abajo |

Total **7.660 tris**, una malla/un material por FBX. Baldosa modular a .750m
de paso deja juntas de6mm. Repetir comparte mesh/material; para grandes paños,
combinar estáticamente las repeticiones en autoría evita un renderer por baldosa.
No se propone un sistema de colocación runtime.

Dos materiales originales numéricos en manifest: `CY_DullSteel` y `CY_Mineral`.
Son acabados mates simples, sin texturas ni materiales fotográficos. Sustituir
por steel/plaster existentes del proyecto; la geometría conserva UV finitas.
El color y el relieve bajo las luces reales se seleccionan en Unity, no aquí.

## Ejes y colocación

Metros; fuente X=longitud, Z=arriba, **−Y=afuera de la pared**, pared enY0.
FBX usa `axis_forward='-Z', axis_up='Y'`. Seguir la corrección X de los kits
originales anteriores: conservar import rotation/scale, reflejar rootX. La
composición corregida esperada es `(X,Z,−Y)`; marcadores AXIS/ROOT autoritativos.
No hubo importación Unity desde esta tarea.

La bajante tiene anclajes a la pared enZ.38/1.29/2.35, dos uniones intermedias
y un único conducto continuo hasta el codo inferior. Su proyección máxima es
.30m fuera de la pared; boca aZ.07, orientada−Y. La geometría no debe invadir
el paso ni usarse como una nueva colisión estrecha.

Para conectar: Downpipe27 rootZ0; GutterOutlet1m rootZ2.700, mismo rootX/Y y
orientación. Centro del conductoY−.11. El manguito se solapa con la entrada;
el fondo de canaleta tiene un hueco real, no una tapa que atraviesa el tubo.
Unir módulos rectos con pasoX1m. Extremos abiertos para continuar el paño;
no se incluyen tapas terminales, por lo que no presentar un final aislado
cercano a cámara como si estuviera terminado.

Ejemplo geométrico para la fachada oeste existente: root aprox
`(−5.536,0,−60.0)`, yaw−90° después de corrección de importación; −Y local sale
hacia oeste y X de canaleta recorre Z mundial. Esto es una sugerencia, no una
pose comprobada en escena. Conservar contacto de placas con la cara mineral.

La rejilla va **enrasada** en el piso. Cuerpo completo Z[−.036,0]; al posar su
root en el nivel de pavimento no debe agregarse otro cubo bajo ella. La preview
de estudio deja ver su cuerpo contra el plano inferior para inspección; eso
no prescribe un escalón en el recorrido. Mantener collider del piso continuo.

## Comprobación y límites

`validate_rainwater_kit.py` reimporta los cinco FBX y escribe `validation.json`:
finitud, UV, materiales, bounds, anchors, recuentos y rayos cortos de entrada,
boca, salida de canaleta y rejilla. La vista de canaleta motivó separar las
normales planas de sus cantos terminales para evitar bandas por módulo.

Previews `assembled`, `gutter-outlet`, `drain-paving`: inspección de Blender con
iluminación de estudio. No son evidencia de la cámara, mezcla de materiales,
colisión, sombras o visibilidad del patio en Unity. El integrador selecciona
cantidad/colocación y juzga la primera cámara real; no se afirma aprobación
humana ni una mejora de miedo.

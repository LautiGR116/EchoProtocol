# Dirección artística — 1 de octubre de 2026

Estado: propuesta de producción. Inspección de imágenes y autoría, sin aprobación humana. Escena base: Finish. El integrador de escenas es el agente principal.

## Diagnóstico observado

El moodboard del proyecto es `Docs/Design/Media/GDD_v0.2_VisualDirectionBoard.jpg`. Su fuerza reside en construcción reconocible, proporción doméstica, profundidad lateral, luz con fuente visible, materiales que reaccionan a esa luz y pequeños signos de una vida interrumpida. No reside sólo en oscuridad, suciedad ni contraste naranja/verde.

En `D1_00_fresh_Hub.png`, el escritorio funciona como altar aislado ante una pared ciega. Los dos accesos simétricos y textos flotantes anuncian una sala de pruebas. `30-reception-letter-game.png` mejora el objeto cercano, pero su pared carece de uso, interruptores o relación con el resto de la recepción.

En `house-play-03.png`, `movement/24.png` y `view-2.png`, la casa sigue siendo un pabellón: corredor central de 3,7 m, vanos de 2,2 m, recintos de aproximadamente 6 × 12 m y mobiliario aislado. Las paredes terminan en jambas sin hojas. Los archivadores y la cocina repetida debilitan la identidad de vivienda. La oscuridad suprime objetos; no genera por sí sola una composición. Los pilares de sombra y conos idénticos revelan el montaje procedural.

En `25-archive.png`, archivadores equidistantes sin agrupación funcional, iluminación repetida y figura humanoide de partes geométricas producen un escenario demostrativo. La figura merece una fase propia posterior: primero hay que construir un lugar convincente al que pertenezca.

La autoría confirma que las áreas nuevas se construyen con grandes prismas rectos y props aislados. Las texturas PBR ya existentes son reutilizables; el cuello de botella actual es escala, articulación arquitectónica y agrupación. Algunas capturas históricas como `03-P1.png` y `20-pressure.png` preceden correcciones y no representan por sí solas el estado final.

## Dirección elegida

**Un edificio que ha seguido funcionando después de quedar vacío.** Administración de mediados de siglo, ampliaciones técnicas posteriores y una vivienda conservada dentro de su infraestructura. Paleta mineral y verde gris en la institución; tabaco, marfil y madera oscura en la casa. El cian se reserva al instrumento y al Echo. Evitar vestir cada objeto de verde/cian.

La clínica debe parecer utilizable antes de resultar inquietante. La casa debe sentirse más próxima y táctil que la clínica antes de volverse hostil. El cambio del miedo se apoya en perder una relación espacial antes aprendida: vista hacia el escritorio, luz bajo una puerta, ventana fría al fondo de un cuarto, silla que tapa un acceso. El entorno no debe explicar verbalmente su anomalía.

## Tres transformaciones grandes

### 1. Recepción: del altar de tutorial a un lugar administrativo

Construir un puesto de recepción asimétrico alrededor del escritorio existente, sin estrechar las rutas laterales de P1. El mostrador debe tener un frente de paneles de unos 2,4 m, un retorno corto y una abertura lateral creíble para quien trabaja allí. Añadir atrás una vidriera administrativa con paños inferiores opacos, compartimentos de llaves/correspondencia y un reloj. Una silla desplazada del escritorio, un área de espera con dos asientos y un abrigo colgado bastan; no llenar el piso de basura.

Composición desde el inicio: primer plano en penumbra → canto del mostrador y carta → abertura iluminada del registro a la izquierda → profundidad administrativa en segundo término. El acceso derecho puede estar más oscuro mientras conserve lectura de suelo y marco. Reemplazar letreros gigantes por placas rígidas, fijadas a una superficie. Conservar el número y el destino legibles antes de reducir tamaño.

Construcción: marcos con grosor, rodapié continuo, altura de dintel consistente, interruptor junto al vano, uniones de suelo que respondan a la pared. No agregar nuevos sistemas jugables para obtener este primer salto.

### 2. Casa: habitaciones de personas, no cuatro cajas de exposición

El cambio fuerte recomendado es reducir la escala perceptiva. Corredor útil de 1,55–1,8 m, vanos de 0,85–1 m y altura de 2,65–2,8 m. Si el integrador conserva los recintos mayores para la ruta, subdividirlos mediante armarios, almacenamiento o una antecámara con función y cierre real, no mediante paredes sin destino. El paso junto a una hoja abierta obliga naturalmente a girar y reduce vistas simultáneas de toda la casa.

Una habitación recibe una composición central: mesa del teléfono junto a ventana vestida con cortina y radiador; sillón orientado a una mesa baja y no al jugador; silla secundaria frente al sillón pero vacía; libro cerrado/taza/trapo como grupo sobre la mesa. Una franja fría entra por el lateral y una lámpara cálida construye el destino. Desde el corredor, el teléfono debe reconocerse por una silueta y una fuente de luz, no por una etiqueta flotante.

La cocina necesita backsplash, módulos altos, borde de mesada y un conjunto de uso (plato/paño/vaso) en lugar de dos copias de la misma cocina. El dormitorio necesita cama con tela de volumen irregular, cabecera, mesita y un armario doméstico. La cuarta habitación puede ser un estudio con archivo administrativo invadiendo lo doméstico; allí sí tienen sentido los cajones metálicos.

La circulación debe permitir volver a ver un mismo umbral desde otro ángulo. Mantener cajas de colisión simples y márgenes reales para el CharacterController; comprobar la trayectoria desde la cámara a 1,65 m antes de producir toda la casa.

### 3. Transición: el límite construido entre clínica y casa

Crear dos umbrales protagonistas, no otro corredor continuo. El primero muestra desde la institución un trozo de papel floral, el canto de una puerta de madera y una lámpara fuera del eje. El segundo devuelve a lo técnico: un falso cielo doméstico termina junto a una bandeja de cables, tubería y una hoja industrial con número pequeño. Los materiales terminan en perfiles y espesores; no se mezclan por mera superposición de cubos.

Usar un vestíbulo corto con desplazamiento lateral de la apertura: el jugador pierde de vista el Echo al rodear el marco y recupera después una referencia que cree conocer. La superficie de vidrio debe tener profundidad de marco y reflejo contenido; no una gran franja transparente uniforme. Reservar una sola vista larga para su verdadero efecto, después de espacios más cortos.

La presión puede conservar su regla mientras se convierte en sala de instalaciones: ducto principal con derivaciones a los cuatro equipos, un gran depósito vertical que oculte la mitad del recinto y zócalo de mantenimiento. La cámara no debería mostrar las cuatro bombas como objetos equidistantes sin conexión.

## Kit mínimo con impacto alto

Prioridad de producción original en Blender, unidades en metros y frente documentado:

| Pieza | Uso | Dimensiones aproximadas |
| --- | --- | --- |
| Marco y hoja doméstica articulable | Umbrales/oclusiones, no puertas decorativas gigantes | hueco 0,92 × 2,10 m; hoja 0,86 × 2,04 m |
| Ventana, cortinas recogidas y radiador | Composición del teléfono; contraluz lateral | 1,65 × 1,8 m, antepecho 0,75 m |
| Sillón con costuras y tela irregular | Masa reconocible de uso, reemplaza formas de bloque | 0,86 × 0,9 × 0,95 m |
| Mostrador con retorno y casillero | Identidad de recepción y profundidad | 2,4 × 0,72 × 1,06 m |
| Módulos domésticos de zócalo/cornisa y friso | Escala y terminación de paredes | segmentos de 1/2 m |
| Tubería colectora y bandeja de cables | Lógica de instalaciones en transición/presión | segmentos de 2 m |

No hace falta más resolución de textura para justificar estas piezas. Las superficies grandes conservan PBR; piezas pequenas necesitan color, bisel, normales correctas y rugosidad. Textiles pueden usar una textura original generada o un tejido CC0 si aporta un patrón diferente al de la madera.

## Criterios de revisión desde la cámara real

1. Desde spawn se reconocen recepción, ruta inicial y carta sin leer un párrafo.
2. Al entrar en casa aparecen al menos tres planos de profundidad; el teléfono se localiza por composición y sonido.
3. El jugador puede pasar cada umbral sin engancharse y una puerta abierta no parece un decorado atravesable.
4. Las fuentes prácticas visibles poseen su luz; apagar un circuito cambia la composición y deja orientación residual, no una pantalla vacía.
5. Una captura útil muestra construcción y materialidad incluso sin el Echo ni HUD. Capturas estáticas sólo prueban eso; la ruta real debe probar giro, oclusión y circulación.
6. Ninguna mejora se declara aterradora ni aprobada por una persona a partir de estos fotogramas.

Siguiente producción recomendada: construir primero recepción y ventana/teléfono con un kit de cuatro piezas; integrarlos y revisar Game view antes de multiplicar piezas. Luego redimensionar los umbrales domésticos y recomponer el resto de la casa con base en esa escala.

## Inspección posterior: cámara de presión

Estado: diagnóstico de imágenes Finish y propuesta; **no producido ni integrado**. Revisadas `Docs/VisualEvidence/2026-09-30-Finish/view-0.png` (cámara de inspección), `movement/22.png` (ruta real anterior a la corrección de orientación de controles) y `32-valve-detail-staged.png` (Game view cercano con control corregido). `view-1.png` no es evidencia útil porque intersecta geometría. La cámara actual de Residence todavía debe juzgar la propuesta.

La carencia mayor no es el detalle del volante. El control Finish ya tiene un aro, cinco radios, buje, placa con tornillos, caja de manómetro y marcas que resisten una vista cercana. En cambio, el recinto se lee como un salón amplio con objetos demostrativos: cuatro motores equidistantes contra una pared, extremos de tubería suspendidos sin unión visible y una gran baffle opaca que parece un muro negro. La vista durante el recorrido confirma que casi todo el lado de la máquina central es una superficie continua sin escala de mantenimiento. La captura cercana también muestra dos volantes contiguos: uno decorativo del skid Campaign y el control activo Finish. Esa duplicación debilita la lectura de cuál se opera.

La autoría confirma el problema. `Assets/Editor/CampaignProduction.cs:163` coloca siete cilindros rectos transversales de 15,4 m sin codos ni sujeción; cuatro `EP_PressurePump` se repiten cada 3,7 m. La baffle es un prisma de 1,7 × 2,7 × 6 m. Los motores tienen cilindro, aletas, pequeño manómetro y volante: no son cajas, aunque están aislados. Referencias de geometría reutilizable: `ArtSource/Blender/Campaign/build_campaign_kit.py:69` y `ArtSource/Blender/Finish/build_finish_details.py:40`.

Dos sustituciones propuestas, en este orden:

1. **Un equipo central con identidad industrial.** Reemplazar la piel de la baffle por un conjunto dentro de su ocupación aproximada de 1,7 × 6 m: depósito vertical de 1,15–1,25 m de diámetro y 2,35 m de altura hacia la entrada, con tapas redondeadas y tres pies; detrás, carcasa opaca de servicio de unos 1,45 × 4,4 × 2,4 m con dos grandes tapas registrables, juntas, bisagras y una única rejilla profunda. Un puente corto de tubería une ambos volúmenes. La parte posterior conserva una masa que impide ver al Echo desde los controles; no agregar rendijas que invaliden esa separación. La curva del depósito rompe el prisma al entrar, y los paneles dan escala al pasar. Reutilizar el lenguaje de esmalte/acero del motor y las juntas/etiqueteros del `EP_Locker`, sin copiar su fachada como decoración. Es una propuesta de puesta en escena plausible, no un diseño de ingeniería certificado. La colisión necesita ajustarse a la nueva piel y comprobarse desde ambos lados.

2. **Un colector y dos skids conectados a los controles A/B.** Conservar los controles Finish, sus posiciones, aguja y piloto; reutilizar los motores Campaign con una versión sin el volante decorativo duplicado. Una tubería principal longitudinal junto a la pared, con codos de radio visible, bridas, abrazaderas y dos derivaciones, conecta cada motor al cuerpo situado detrás de su control. Un retorno secundario y un único cruce alto hacia el equipo central sustituyen la fila de tubos que termina en el aire. Diámetro visual principal aproximado de 0,16–0,20 m; secundario de 0,08–0,10 m. Las derivaciones tienen principio y final reconocibles. Dos conjuntos en A/B aportan mejor jerarquía que cuatro copias igualmente espaciadas. La nueva geometría necesaria está en las uniones y soportes; remodelar el volante completo tiene poco retorno.

Antes de producir, contrastar tres vistas reales de Residence: entrada a altura de ojos mirando al extremo de la baffle; aproximación al control A con máquina y techo en cuadro; giro desde P—02 hacia los controles. El primer reemplazo debe mejorar la silueta central sin abrir la vista mecánica; el segundo debe hacer comprensible el destino de cada tubo y dejar inequívoco el único volante operable de cada estación. La efectividad del miedo y la tensión de alejarse del Echo siguen necesitando una partida humana.

## Inspección posterior: SE2

Estado: observación de la captura `Docs/VisualEvidence/2026-10-01-Threshold/Reception-continuous/campaign-03-SE2-Camera.png` y lectura de `EchoProtocol_Registration.unity`, `ThresholdProduction.cs` y `VS1ParallelFigureEvent.cs`. Propuesta no producida ni integrada; sin aprobación humana.

**Lo que se ve.** Un túnel azul casi sin construcción lateral termina en una figura centrada, encuadrada por rectángulos sucesivos y un fondo naranja uniforme. El conjunto comunica una aparición al final de un pasillo, pero apenas permite reconocer una habitación que alguien usaría. El sillón y la lámpara son masas marginales, no un grupo doméstico. Dos rayas del vidrio cruzan el cuerpo; se reconocen como barrera, aunque también cortan la lectura. La captura no demuestra cómo se ve al acercarse al vidrio.

**Lo que confirma la autoría.** La sonda captura cerca de x−.51: el vidrio está a unos 8.5 m y la figura a 13.26 m. La cámara de la evidencia está lejos, no frente a un cuarto pequeño. El estado doméstico ocupa 5 × 8 m con techo a 3.2 m; los dos marcos libres tienen anchos de aproximadamente 3.81 y 2.21 m y alturas de 2.85 y 2.65 m. Aún contiene el sillón de prismas, la lámpara de prisma/cilindro, pliegues de cortina rectangulares y el humano de piezas del prototipo. `ThresholdProduction.ReplaceFigure` renovó el Echo y la presencia de `HouseMemory`, no esta figura. Tres luces domésticas puntuales sin sombras y muy anaranjadas iluminan el volumen; `Far amber doorway` es un rectángulo contra la pared, no una abertura con profundidad.

**Dirección propuesta: una sala doméstica vista a través de una instalación clínica.** Conservar los 8 m de profundidad que contradicen la cámara fría de 3.3 m, pero dividir su lectura en lugares reconocibles. El piso permanece continuo, sin pedestal, tarima ni corredor ceremonial. Las coordenadas siguientes son locales al root SE2, cuyo origen mundial es (8,0,29.1), yaw90: local (x,y,z) corresponde a mundo (8+z,y,29.1−x).

1. **Primer plano, estancia ocupable.** A partir de z≈.7, agrupar el sillón `EP_OV_Armchair` a la izquierda (aprox. x−1.0,z1.9), girado 20–30° hacia el interior; una lámpara al lado y una mesa pequeña conforman una zona de lectura. No orientarlos todos al observador. `EP_OV_KeyConsole` puede ocupar la pared derecha en z≈2.7, con un único conjunto pequeño de pertenencias. Reemplazar los dos marcos flotantes por una pared real que vuelve lateralmente, z≈3.6–4.1, con zócalo y dintel alrededor de un vano. El ancho útil del frente puede quedar en 3.6–4 m y el techo en 2.65–2.8 m: no es necesario alterar la caja exterior ni el paso del jugador.
2. **Segundo y tercer planos, paso y habitación posterior.** Un único vano de aproximadamente 1.0–1.1 m desplazado hacia x+.4, con la hoja `EP_OV_DomesticDoor` entreabierta y espesor de pared real, abre hacia el cuarto posterior. La figura adulta existente sustituye las piezas bajo el mismo `figureRoot`, aproximadamente en x+.3,z4.75; su posición final se decide con cámara, preservando cabeza y torso legibles. Al fondo, reemplazar el panel naranja y los pliegues-caja por `EP_OV_DressedWindow`, desplazado lateralmente, con pared, radiador y cortinas reconocibles. La figura queda dentro de una estancia, separada del vano y del fondo; no elevada ni exactamente centrada contra una pantalla luminosa. El vano, hoja y muebles requieren una comprobación visual porque el target del script no acredita que la figura esté despejada.
3. **Luz con origen y profundidad.** La lámpara del primer plano da una zona cálida limitada sobre tela, mesa y suelo; el medio queda más oscuro; la ventana posterior aporta un tono neutro/frío suave sobre una pared lateral. Así la silueta se separa de un volumen, no de un rectángulo naranja. Una luz local con sombras aporta contacto y espesor antes de sumar más fuentes. Reducir la saturación azul del corredor y dar al práctico una carcasa opal/metal reutilizada de recepción; zócalo, junta y un retorno de pared bastan para quitar la apariencia de túnel abstracto. Conservar una referencia equivalente del estado frío (rejilla, pared posterior y encuentro de suelo) para que el aumento de profundidad sea perceptible, no sólo un cambio de color.

**¿Falta un asset central?** No para el primer salto: adulto, sillón, ventana, puerta y consola ya existen y atacan las formas más débiles. No produciría otro mueble protagonista antes de recomponerlos. Si la cámara confirma que la lámpara tiene presencia, sí justificaría una sola lámpara de pie original, de unos 1.55–1.65 m, pantalla textil troncocónica con interior y borde, base pesada y cable que termina en la pared. Sería una fuente de luz creíble compartible con la casa; no un nuevo símbolo ni una función jugable. El kit actual de Campaign tiene `EP_LampTable`, útil como prueba de composición, y Finish tiene un aplique reutilizable.

**Restricciones y evidencia necesaria.** Conservar raíz, vidrio sólido único, trigger, referencias, P2 y comportamiento de RECALL. El trigger abarca mundo x−1.05..7.95 y z28.1..30.1: hoy se puede completar SE2 desde el inicio del corredor. El target real está en local(0,1.55,3.05), antes de la figura en z4.75; el rayo acaba allí. Un decorado con collider delante del target puede bloquear la lectura, mientras una pared detrás puede tapar la figura sin hacer fallar el evento. Mantener el rayo despejado y revisar por separado la visibilidad del adulto; los objetos inaccesibles pueden ser sólo presentación bajo `coldRoomRoot` o `domesticRoomRoot`. No añadir otro vidrio sólido.

Primero capturar los estados frío/doméstico desde la misma posición lejana actual y desde una aproximación real a x5.5 y x7.2, a altura de ojos, antes de producir más assets o modificar tiempos. La nueva composición debe sostener esas tres distancias y una pequeña variación lateral, no sólo una vista promocional. El PASS actual prueba el evento y su persistencia; no prueba percepción de la profundidad imposible, deseo de aproximarse, tensión ni miedo. Eso sigue requiriendo una partida humana sin indicar dónde mirar.

Actualización de producción: el integrador autorizó después la lámpara de pie.
Su fuente original, FBX, manifest y validación están en
`ArtSource/Blender/DomesticLighting/`; altura 1.616 m, pantalla Ø.469 m, interior
y bombilla separados, anclas de foco y conexión de pared. Reimportación Blender
verificada. La recomposición de SE2 y la integración de esa luz siguen siendo
trabajo del integrador, no evidencia aportada por el render de estudio.

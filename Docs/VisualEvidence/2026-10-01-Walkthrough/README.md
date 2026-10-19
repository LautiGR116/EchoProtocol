# Walkthrough — integración en curso

Sucesora independiente de Courtyard. El primer candidato de SE2 permite entrar
en el salón y volver por dos accesos. Conserva la admisión por P2 y el cruce SE3;
el progreso físico se separa de la presentación y de la observación. No añade
canales de Echo, acciones E o sonidos. Las pisadas consultan el suelo activo.

Courtyard permanece como última build recuperable comprobada. Esta escena aún
requiere cámaras, recorrido, revisión de colisión/ocultación, campaña y build.
El autor original está en ArtSource/Design/SE2Walkthrough; su README/contrato son
históricos de preparación. Assets/Editor/SE2WalkthroughProduction.cs contiene la
integración corregida: clave real de CampaignDirector, cajas en coordenadas
mundo, margen del volumen visual y segundo refugio de presentación al norte.

Compilación de runtime/probe previa: sin errores. Autoría por Editor API conserva
referencias P2/SE3/resetTargets y57 fuentes; las dos luminarias de acceso llevan
el total a142 luces. No es evidencia de jugabilidad
ni de respuesta humana. Escucha, comodidad, claridad y miedo EN PRUEBA.

## Primeras iteraciones, conservadas

`first-layout-route/`: el trayecto automático alcanzó el frío tras P2 genuino,
pero su waypoint intentaba cortar el hombro del muro sur. Falló en(2.29,27.44),
sin mover al Player para salvarlo. Se corrigió el waypoint para doblar por el vano.

`first-playable-layout/`: PASS75,95.621s automáticos, dos tomas inmutables y5054
frames, cero fixtures/reanudaciones ocultas. Llega a R1 por entrada/salida físicas.
Sus imágenes NO se seleccionan como acabado: refugio negro y hoja posterior con
muy poca exposición de la figura. El indicador observado de esa pasada usaba el
antiguo foco de lectura del cuarto; no prueba que el cuerpo estuviera visible.

Refinamiento siguiente: lámpara apartada del centro, dos prácticos fijos en los
accesos, hoja posterior a58° y figura .30m al norte. Un target explícito en el
centro real del cuerpo mide observación opcional. El estuche/mesa de servicio
originales(11404 triángulos fuente) reemplazan los cubos. Su luz permanece
constante durante el cambio. No se agrega voz, stinger ni grabación de audio.
La campaña y build de esta sucesora siguen pendientes.

## Ocultación y recuperación

`passage-recall/`: PASS110,132.747s automáticos, cuatro tomas inmutables,
8645 frames contrastados y cero fixtures. Un R real interrumpe el paso después
de revelar la habitación; ésta permanece y una nueva toma genuina de P2 permite
volver. Incluye pausa real y llegada a R1. Es anterior al ajuste de luz posterior
y al volumen adicional que protege las sombras del pasillo.

`occlusion-before-shadow-guard/` conserva el defecto detectado: mirando al norte
desde el refugio sur, el cambio de muebles alteraba una sombra visible fuera del
cuarto. Se añadió un volumen que abarca los receptores de sombra del acceso.
`occlusion-fixtures/` compara veinte posiciones/direcciones de cámara con ambos
estados y con una repetición del mismo estado. Las tres vistas aceptadas por la
protección tienen cero píxeles por encima del margen de variación de captura;
las otras diecisiete se rechazan conservadoramente. Los sólidos activos conservan
identidad y bounds en las veinte. Esto comprueba esas vistas, no todos los ángulos
posibles. Son fixtures explícitas de cámara/presentación, restauradas al terminar.
El script está conservado en ArtSource/Design/SE2Walkthrough/Integration.

La luz posterior existente ahora apunta a la pared de fondo, con el mismo gain.
La cámara muestra el cuerpo recortado sobre el cuarto, no perdido en negro.
`passage-unseen/`: PASS73,93.238s, dos tomas/5390 frames, cero fixtures. Caminando
de espaldas por el acceso norte no se revela ni se observa la figura; la entrada,
salida y R1 funcionan igual. La observación no es requisito oculto de progreso.

`north-start-input-failure/` conserva un intento detenido en el título: el pulso
Enter no inició la sesión. No evaluó SE2 ni produjo una toma. La prueba se repite
desde un Play limpio sin reanudar ni colocar al Player artificialmente.

`passage-north/`: **PASS76**, 97.733s, dos tomas/3426frames,
cero fixtures/reanudaciones ocultas. La entrada norte presenta una vez, registra
una entrada/salida y llega a R1. Un intento real de caminar contra la hoja fija
se detiene antes del cuarto posterior; la cámara de contacto muestra el cuerpo
contra la pared/ventana. No se usa una barrera invisible. Incluye el último
cálculo de cruces físicos por segmento y la base estacionaria durante pausa.

## Campaña y continuidad antes de la candidata

Campaña continua **PASS494**, 299.592s automáticos, cuatro tomas
inmutables/6855frames contrastados, cero fixtures/reanudaciones ocultas.
Llega al final después de recorrer SE2 por el sur, SE3/R1, P3, archivo/recepción,
instalación, casa, patio y encuentro/conservación/devolución final.
Continue separado: **PASS341**, diez cargas frescas, cinco fixtures explícitas,
preferencias originales restauradas.

El usuario cambió el encargo a una candidata final con plazo máximo de seis
horas. Se conserva la escena y toda esta evidencia; la build final saldrá de su
sucesora Candidate, que añade orientación contextual opcional y pausa al leerla.
No se crea otra build intermedia sólo por cerrar este checkpoint. Courtyard
sigue siendo la app recuperable hasta comprobar la candidata.

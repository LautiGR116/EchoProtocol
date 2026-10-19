# Embodiment — personajes integrados, 2026-10-09

Abrí [EchoProtocol.app](../Builds/EchoProtocol-Embodiment-20261009/EchoProtocol.app).
**Iniciar sesión** recorre la campaña; **Jugar · La máquina que te cubre** abre
el encuentro del subsuelo. [Paquete macOS](../Builds/EchoProtocol-Embodiment-20261009.zip),
[comparación visual](VisualEvidence/2026-10-09-Embodiment/index.html) y
[movimiento real de la cámara del Player](VisualEvidence/2026-10-09-Embodiment/motion.mp4).
**Implementado y comprobado objetivamente. Credibilidad y miedo EN PRUEBA humana.**

## Elección y resultado

Se compararon dos estrategias dentro de Unity, con la cámara del Player, FOV 60,
la misma ubicación del Hub y sus luces: mejorar la base MakeHuman existente con
materiales URP/Lit, o integrar adultos escaneados de Renderpeople. La primera
conservaba el rostro simplificado, pelo macizo y ropa lisa que motivaron el rechazo.
La segunda sostiene mejor la cara cercana, las manos y los pliegues; fue seleccionada.
Estas comparaciones iniciales son poses de estudio explícitas en el Editor,
no recorridos jugados. [Imágenes y condiciones](VisualEvidence/2026-10-09-Embodiment/alternatives/comparison.json).

**Eric rigged 001** presenta al Echo y a la figura de SE2: camisa, chaleco, corbata,
pelo corto y una cara adulta reconocible. **Nathan animated 003 walking** presenta
al perseguidor: camiseta, brazos descubiertos, jeans, otra cara y una postura de
aproximación inclinada. Cuerpo y ojos tienen superficies separadas; normal y
brillo conservan detalle sin tratar toda la piel como plástico. Se integraron
pasos, apoyo de pies, rodillas, giro, respiración, contacto con el mando y un pulso
discreto de párpados. La manga de primera persona acompaña la camisa del Echo;
la mano original se conserva.

El perseguidor recibe intención del encuentro presente. Su inclinación, brazos,
mandíbula durante el grito y ritmo de contacto acompañan la embestida. Los tres
emisores existentes están configurados y siguen cabeza/cuerpo; se conservan
grabaciones, niveles y mezcla. Las reglas, velocidades, visión, audición,
descarga, quiebre de vista y reintento pertenecen a Coverage.

El Echo muestrea su pose sólo desde datos de la toma inmutable. Se capturan
explícitamente fase/velocidad de giro y el punto del mando en coordenadas del
Player, además de los canales existentes. La mano reproducida no consulta un
mando presente; no hay mirada autónoma, Animator, voz ni audio grabado nuevos.
6B admite el material opaco exacto del cuerpo, conservando puerta, pausa,
zona y visión física de pecho/cabeza. Los rótulos de P1 bajaron 0,54 m para
despejar la cara; texto y validación permanecen iguales.

## Evidencia final

| Verificación | Resultado y alcance |
| --- | --- |
| Configuración y deformación | PASS120: referencias explícitas, emisores, anatomía, materiales y 96 poses de estudio. Sin valores inválidos; pies dentro del margen de suelo autorado. Rechazo de cuerpos ocultos, sombras solas, transparencia y materiales ajenos en 6B. |
| Captura, persecución y reintento | PASS43 en Editor, 5.101 muestras de reproducción, una toma inmutable, ningún traslado del Player ni reanudación oculta. Se verifica la captura y el reintento con esa toma. |
| Campaña completa en Editor | PASS545; cinco tomas, 12.523 muestras de reproducción, cero colocaciones del Player y cero reanudaciones ocultas. |
| Campaña macOS, título → final | PASS545 en Player de verificación: cinco tomas, 17.883 muestras, cero colocaciones del Player y cero reanudaciones ocultas. Incluye apariciones, encuentro, casa, observación de 6B, conservación y devolución de llave. Entradas normales del Input System; el guardado se suprime sólo para aislar datos del usuario. |
| Pose y pausa del cuerpo macOS | 17.463 cuadros comparados con remuestreo independiente de 88 articulaciones; 375 cuadros durante tres pausas. Diferencia máxima de posición 0,00000321 m, angular cero y deriva en pausa cero. Esto mide fidelidad, no naturalidad humana. |
| Cámara y movimiento | Ocho pares antes/después y 84 cuadros reales del recorrido de Editor. El video es silencioso, 960×540, unos 7,7 s; tres cortes omiten esperas/desplazamientos sin captura. El cuerpo anterior se renderizó como sustitución temporal en el mismo instante, cámara y luz de la candidata. No son dos partidas ni una comparación nativa histórica. |
| Rendimiento breve | Apple M3, Player 1280×720, 43.206 cuadros: media 6,98 ms, p95 14,50 ms, p99 25,32 ms. Se excluyen pausas y el trabajo cercano a capturas. Queda instrumentación de verificación; es cadencia de cuadros, no una medición aislada de GPU ni garantía para otras Macs. |
| Build y paquete | Unity 6000.3.21f1; entrega Succeeded, cero errores y una advertencia. Universal Intel/Apple Silicon; ningún tipo de las 45 clases de pruebas inspeccionadas se entrega. ZIP: CRC, 205 archivos comparados, permisos y firma local estricta correctos, también al extraerlo. Sin notarización Apple. |

La Console final se inspeccionó completa: 383 entradas capturadas; sus 28 errores
son del transporte Unity Pipeline, clasificados en
[final-console-summary.json](VisualEvidence/2026-10-09-Embodiment/final-console-summary.json).
El Player final no registra errores del juego. Conserva las advertencias heredadas
del collider del auricular con escala negativa y del atlas de sombras. La build QA
tiene 347 advertencias de Inference/Burst; la entrega tiene una advertencia por
ausencia del servicio Pipeline en Player. No se presenta la Console como limpia.

Se conserva el fallo anterior del encuentro por una referencia de voz perdida
durante el reemplazo del cuerpo: los tres emisores se restauraron y el recorrido
final pasó. También se conserva un intento de build anticipado durante compilación,
con resultado Unknown y cero bytes; no se contabiliza como build lograda.
[Resultados finales y congelación de 135 entradas de fuente](VisualEvidence/2026-10-09-Embodiment/final-verification.json).

## Límites y partida humana

La cara conserva la expresión de su modelo base. Los párpados y la mandíbula son
actuación limitada; no hay seguimiento ocular, captura facial ni una actuación
cinematográfica completa. La carrera adapta el ciclo de caminar con apoyos y
postura procedurales; puede quedar deslizamiento o rigidez perceptible en giros,
retrocesos y cercanía. El contacto del mando se ajusta a la toma capturada, pero
su lectura como peso/esfuerzo todavía requiere juicio humano. Las imágenes y
las mediciones no demuestran que el personaje asuste.

En una partida, evaluá si el Echo resulta aceptable mientras coopera en P1;
si cara, ojos, manos y pasos sostienen esa aceptación al acercarte en P3 y 6B;
y si la silueta, peso y aproximación del perseguidor producen tensión sin parecer
un muñeco. Probá el regreso y reintento del subsuelo, escuchando a volumen cómodo.
Auriculares opcionales; siguen disponibles parlantes, mono y dinámica reducida.
La claridad de instrucciones y el ajuste de la manivela quedan para el siguiente
ajuste de jugabilidad, según el alcance pedido. No se declara aprobación humana nueva.

## Derechos y recuperación

Modelos gratuitos bajo [términos de Renderpeople](https://renderpeople.com/general-terms-and-conditions/),
no CC0: uso en juego compilado, fuentes y derivados excluidos de Git público.
No compra, paquete, plugin, framework ni subida a un servicio de IA.
[Créditos](CREDITS.md) y [procedencia](../ArtSource/Design/Embodiment/provenance.json)
registran originales, hashes y restricciones.

Coverage, Folio y Resonance conservan sus ZIP anteriores byte por byte. La escena
Coverage conserva su hash. Embodiment tiene escena privada y guardado propio
`EP.CampaignCheckpoint.Embodiment.v1`. No se descartó ni se comprometió el trabajo
previo del repositorio. La recuperación personal conserva las diez capas de
Coverage y una nueva capa de Embodiment, más un archivo separado de originales
licenciados y términos. [Verificación completa y rutas de las once capas](/Users/lautigr116/EchoProtocol-backups/embodiment-checkpoint-20261009/verification.json),
[instrucciones de recuperación](/Users/lautigr116/EchoProtocol-backups/embodiment-checkpoint-20261009/RECOVER.txt).

ZIP final SHA256: `0b5971ea4a0329ca5f6480d6bb37ec1b074e886db5d37ad1176aefa8a521b1f5`.

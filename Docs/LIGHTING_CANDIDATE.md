# Echo Protocol — Lighting

2026-10-08. Pasada de iluminación de la campaña de Contraprueba, motivada por el
feedback humano de sectores demasiado oscuros. Escena independiente:
`Assets/_Game/Scenes/Production/EchoProtocol_Lighting.unity`; guardados
`EP.CampaignCheckpoint.Lighting.v1`. Contraprueba y su paquete original se
conservan. [App macOS](../Builds/EchoProtocol-Lighting-20261008/EchoProtocol.app) ·
[ZIP con instrucciones y créditos](../Builds/EchoProtocol-Lighting-20261008.zip).
Universal Intel/Apple Silicon, firma local ad hoc; no notarizada.

## Cambios perceptibles

La luz ahora permite leer planos de circulación y encontrar controles después
de perder un circuito. P1 conserva referencias bajas mientras se apagan las
luminarias superiores; P3 mantiene un retorno de servicio legible después de la
transferencia. Archivo, hoja de llegada y descenso reciben luz local. La reserva
de aislamiento revela el mando, máquina, pasillo y salida cuando se extingue la
luz de trabajo.

En casa, la contribución de las ventanas revela teléfono, portafusibles y mesa
de la llave. Una pequeña luminaria de servicio mantiene localizable el soporte
de cocina sin la reserva. La reserva trasladable sigue alimentando un único
sector: su luz cálida, lámparas y aparato conservan el cambio real. Patio,
aproximación lateral a6B y salida tienen referencias de suelo y pared. Las
sombras profundas permanecen en zonas sin información imprescindible.

No se elevó exposición, ambiente global ni postprocesado. La pérdida de ambiente
del primer encuentro sigue vigente. No se alteraron puzzles, tiempos, sonido ni
canales del Echo. Los nuevos apoyos pertenecen al espacio y a sus fuentes, con
referencias explícitas. El cuerpo reproduce los mismos datos grabados.

## Evidencia y criterio

[Comparaciones y resultados](VisualEvidence/2026-10-08-Lighting/README.md).
Se separan campaña nativa continua, renders de comparación y fixtures de carga.
Las imágenes son renders sin corrección posterior. Las parejas de campaña usan
la cámara exacta de la base; las vistas de carga usan las mismas coordenadas,
estados y altura normal. Los actores móviles pueden estar en otro instante de
su toma: no se afirma equivalencia temporal de sus poses.

La primera pasada dejaba negra la cocina sin reserva y la hoja de llegada antes
de abrirse. Esas capturas se conservan como evidencia de iteración. También
encarecía el renderizado: se agruparon las piezas estáticas de luminarias y se
redujeron escrituras de sombras, conservando la selección de cuatro fuentes
cercanas y la excepción de paredes de la residencia. El observador QA compara
cada selección con la regla original.

La revisión posterior de la secuencia real, a1Hz, encontró otro defecto que no
mostraba el encuadre del escritorio: la bandeja de devolución y la aproximación
a la salida seguían negras. Se añadió una pequeña luminaria fija de portería y
el rebote del práctico existente del pasillo. Las vistas aisladas mantienen la
cámara exacta observada; la campaña vuelve a comprobar esos puntos antes de E
y antes del fundido. El escritorio y la bandeja son dos destinos distintos.

## Verificación objetiva

La campaña pasa en un Player macOS QA separado, con entradas de movimiento,
mirada, E y R; no hay colocaciones de gameplay ni reanudaciones ocultas. Las
cuatro tomas conservan sus datos. Las vistas de comparación declaran por separado
sus fixtures de cámara. Se inspeccionaron todos los tramos y sus giros, retornos,
estados eléctricos, RECALL y cargas; la última corrección de portería pasa en `final2-native`:576 comprobaciones,
12.148 frames observados, cuatro tomas inmutables, cuatro RECALL, cero fixtures
de gameplay y cero reanudaciones ocultas. La revisión incluye355 vistas reales
a1Hz,74 encuadres comparados con sus giros y112 vistas de cargas/estados.
Bandeja y salida tienen tres capturas nativas adicionales antes de E/fundido.

Diez cargas de casa: PASS341. Cuatro cargas de aislamiento: PASS45. Ambas
restauran preferencias y declaran sus estados/poses iniciales. El reintento de
P3 usa una única fixture previa de R1 y luego entradas reales: un plan sin
retorno no completa; soltar E cancela; RECALL durante el mando borra el progreso,
restaura persiana/maquinaria y conserva referencias locales. Cinco tomas se
comprueban inmutables; PASS2743 incluye sus comparaciones de frames.

La misma app QA final pasa una segunda ruta:576 comprobaciones,15.548 frames,
cuatro tomas, cero fixtures de gameplay/reanudaciones ocultas y662 selecciones de
sombras conformes. La app de entrega excluye QA: tablas reales de tipos leídas,
inicio/movimiento/18,7s RECALL/H/J/pausa/audio UI restaurada/créditos/salida0
inspeccionados por teclado y clicks. CUA no estableció giro relativo de mouse;
la ruta QA sí comprueba las entradas de mirada, y el feel humano sigue abierto.
Cero errores de build. Hay346 advertencias del paquete de inferencia existente.
El log conserva el collider heredado del auricular; no hay excepciones de juego.
La Console conserva un error de una auditoría que intentó cargar una DLL de
herramientas incompatible; usar la versión ya cargada resolvió esa auditoría.

## Rendimiento observado

Apple M3/Metal,1280×720; cadencia de la ruta real, excluyendo pausas y ventanas
de captura. Base repetida: media4,76ms, mediana4,18ms, p958,90ms. Candidata final
repetida: media5,53ms, mediana5,01ms, p9510,12ms, p9914,34ms. El costo medio
sube16,3%; p95 sube1,22ms, con margen frente a16,7ms en esta máquina.

La otra pasada del mismo modelo final fue más lenta: media7,32ms, p9513,73ms,
p9916,98ms. Se conserva: no se oculta el pico ni se atribuye su causa sin prueba.
La optimización previa dio5,55ms de media. No es un benchmark aislado de GPU;
primer uso, carga de fondo y comportamiento térmico no quedaron aislados.
[Todos los valores y alcance](VisualEvidence/2026-10-08-Lighting/performance.json).
Otra pantalla, resolución o hardware requieren comprobación humana.

## Recuperación

El ZIP se verificó entrada por entrada y coincide con los archivos locales;
su app extraída también pasa firma estricta. SHA256:
`7410178f7f72eab540f2bcb972d3c0d922885aed92c84b3d1e467075b9f9fecf`.
Contraprueba conserva su ZIP original sin cambios.

Fuentes y evidencia: `../EchoProtocol-backups/lighting-checkpoint-20261008/`,
fuera del repositorio. `RECOVER.txt`, `manifest.json` y `verification.json`
describen Candidate → Legibility → EncounterArt → Reconstruction → Contraprueba
→ Lighting en un directorio vacío. La comprobación lee las seis capas completas
y compara rutas, bytes y SHA256; no extrae sobre el trabajo actual. Excluye cachés,
builds y guardados de máquina. No hubo commits, compras ni descarte de trabajo.

## Evaluación humana pendiente

La inspección de renders permite seleccionar composición y legibilidad; no
certifica la reproducción de negros de una pantalla real. Jugar con los ajustes
predeterminados, inicialmente sin H, y anotar lugares donde haya que buscar
controles o caminar sin una referencia reconocible. Revisar especialmente P1
tras SE1, el regreso silencioso de P3, llegada cerrada, aislamiento drenado y casa
con la reserva en mano. Volver desde ambos sentidos y probar Continuar.

La diferencia entre circuito cálido activo y apoyo frío debe seguir siendo
comprensible. No usar brillo o volumen elevados para compensar. Primera partida,
claridad espontánea, credibilidad, escucha, comodidad y miedo permanecen
**EN PRUEBA**; no se inventa una aprobación humana.

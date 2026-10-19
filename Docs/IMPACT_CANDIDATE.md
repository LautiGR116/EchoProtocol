# Impact — encuentro de la bomba, 2026-10-11

La candidata parte de Embodiment y de la prueba humana con auriculares del
11 de octubre. Mejora implementada; comodidad, orientación espontánea y miedo
siguen **EN PRUEBA humana**. No se registra aprobación perceptual automática.

Abrí [EchoProtocol.app](../Builds/EchoProtocol-Impact-20261011/EchoProtocol.app).
[Paquete macOS](../Builds/EchoProtocol-Impact-20261011.zip) ·
[comparación audiovisual de 25 s](VisualEvidence/2026-10-11-Impact/index.html).

## Encuentro

La regla segura precede al peligro: sostener la bomba produce ruido que cubre
el trabajo y presión que cierra la reja. La toma conserva arranque, silencio y
retorno del ruido. El mando de salida exige 2,3 s y provoca un contacto detrás
del tabique durante su accionamiento. La liberación causa el grito adulto, una
aproximación que acelera, contacto de marco, pasos alternados, fricción móvil y
un pulso musical interno. El pulso acompaña esta consecuencia; no representa
un canal grabado por el Echo.

La luz de servicio revela cara y hombros al emerger. Un tabique físico en el
retorno obliga a girar por el lateral iluminado y permite cortar la visión.
La amenaza corre a 3,2 m/s frente a los 4 m/s del jugador; tiene un compromiso
inicial de 3,4 s y captura a 1,08 m. Después puede desviarse por el ruido real
de la bomba si pierde de vista al jugador. Pararse expuesto sigue causando
fallo; Enter reproduce exactamente la misma toma para reintentar.

## Cadena sonora real

La mezcla anterior acumulaba tres debilidades: master vocal lejano y filtrado,
bajo nivel de los contactos, y atenuación logarítmica que adelgazaba la amenaza
desde el mando. El bus existente Master ya tenía +10 dB: subirlo globalmente
habría elevado también el resto del juego. Se mantiene ese bus y WorldSFX;
cambian los masters cercanos, niveles y curvas de distancia de estos emisores.

El grito reutiliza el mismo adulto CC0, a su altura original, con ataque de
18 ms, más detalle y reflexión breve. No hubo generación ni clonación de voz.
La voz y los pasos siguen al cuerpo presente; un obstáculo reduce brillo y
nivel, conservando un ataque audible y orientación. La bomba sube de .23 a
.32 y la purga de .12 a .18. El nivel global guardado no se cambia.

SessionAudio sigue al final con envolvente común para ambos canales, sin
ganancia automática. La candidata limita picos normales a .5 (−6,02 dBFS);
dinámica reducida conserva .18 (−14,89 dBFS) y parlantes conserva suma mono.
Los picos digitales no determinan presión acústica ni comodidad de auriculares.
La comparación usa volumen del juego 1, estéreo y dinámica normal en ambas
partidas; no normaliza el resultado. El mute del Editor afecta su monitorización,
no los PCM capturados después del filtro; la escucha nativa se verifica aparte.

Siete masters y sus hashes están en
`ArtSource/Audio/Impact/masters.json`. Procedencia: CC0 existente de Archeos,
Brian MacIntosh, AntumDeluge y contactos de Finish/Overhaul; pulso original
no verbal. El script de composición conserva los tratamientos reproducibles.

## Superposiciones

El aviso de RECALL pasa a la columna del registro, debajo de éste y de la
reserva de luz si corresponde. Subtítulos inmediatos tienen prioridad sobre
la pista opcional; durante la persecución ésta queda en las notas H. Un cambio
de estado limpia también pistas ya vistas durante un intento anterior.
Los paneles son opacos: los carteles físicos ya no se transparentan bajo sus
textos. Este último ajuste visual se inspeccionó en la entrega nativa después
del recorrido instrumentado; no cambia rutas, audio ni actores.

El perseguidor usa radio físico de .40 m y rutas comprobadas con .39 m de
holgura para separar sus hombros del tabique. La navegación visual mantiene
rayos independientes. Al terminar deja la pose de carrera y sus sonidos.
PlayerEchoClearance hace ceder al jugador presente frente al torso realmente
reproducido; no desplaza al Echo, sus articulaciones ni sus datos. Se conserva
el origen compartido explícito de RECALL hasta que el jugador se separa.
No se afirma haber auditado todos los objetos de la campaña.

## Prueba humana breve

Elegí **Jugar · La máquina que te cubre**. Usá auriculares a un volumen cómodo
habitual y los mismos ajustes para comparar. Prepará ruido/silencio/ruido,
repetí con R y accioná los cierres. Tras liberar la salida, buscá el lateral
iluminado y rodeá el tabique. Una captura intencional permite probar Enter.

1. ¿El contacto previo anticipa peligro y la irrupción sobresalta sin resultar
   molesta? ¿Los pasos permiten ubicar la aproximación?
2. ¿Podés identificar por dónde escapar y girar antes de la captura, y el
   perseguidor sostiene tensión hasta perderte detrás del tabique?
3. ¿Hay todavía textos que se pisan o cámara/manos/cuerpos que atraviesan
   superficies? Indicá mando, giro o instante; basta una posición reproducible.

Repetí sólo el tramo de salida con mono/dinámica reducida si esos modos son
relevantes para tu escucha. Naturalidad de la marcha y actuación facial siguen
sujetas a juicio humano; la IA y animación actuales no acreditan miedo por sí solas.

## Recuperación y alcance

La escena nueva `EchoProtocol_Impact.unity` conserva Embodiment y sus builds.
Hereda modelos Renderpeople privados, excluidos del Git público. No se compró
ningún recurso ni se introdujo un framework. El punto de partida de scripts y
escena se conserva en `EchoProtocol-backups/impact-start-20261011` junto al
proyecto. Los fallos intermedios quedan en `Temp/Impact`, sin ocultarlos como PASS.
La nueva evidencia se concentra en el encuentro y las superficies cambiadas;
la campaña completa anterior conserva su evidencia histórica de Embodiment.

## Evidencia final

| Comprobación | Resultado y límite |
| --- | --- |
| Encuentro Editor | PASS43, 4.965 muestras de reproducción. una toma, cero colocaciones ni reanudaciones ocultas. Captura real y Enter; escape, desvío hacia bomba y liberación natural. |
| Encuentro macOS | PASS43, 5.872 muestras, una toma, cero colocaciones ni reanudaciones ocultas. Primera irrupción estéreo; reintento mono/reducido. Volumen nativo guardado .6984, distinto del 1 usado en la comparación. |
| Cuerpo y pausa nativos | 5.664 cuadros comparados con referencia independiente de 88 articulaciones; 206 cuadros en dos pausas. Diferencia máxima .00000383 m, angular cero, deriva cero. Mide fidelidad, no actuación. |
| Contacto/HUD | PASS20 con entrada normal al Echo; W sostenida se detiene a .640 m del torso, toma intacta. HUD combinado se renderiza con fixture explícita de cuenta breve/pista/subtítulo. |
| Mono/reducido real | Ambos canales idénticos en la retirada, pico −14,90 dBFS; no se guardaron los modos de la fixture. Presencia y comodidad con parlantes quedan para oído humano. |
| Configuración | Loop activo y diez referencias explícitas comprobados; cinco emisores espaciales y tres pasos. Siete clips mono/48 kHz, importación Vorbis/decompresión al cargar. |
| Comparación | Antes 10,57 s/después 14,77 s, cámara compuesta y PCM real. Sin normalizar; ocho ms de fundido por corte. H.264/AAC 960×540, 12 cuadros/s reales repetidos a 30. Se inspeccionó un cuadro decodificado y ambos streams de audio, sin saturación. Precede sólo al ajuste final de opacidad. |
| Entrega | macOS universal, firma local estricta y ZIP comprobado; 46 clases de pruebas ausentes. Controles, entrada directa, opacidad y pausa de la entrega se inspeccionaron con teclado nativo, sin código QA. |

Las builds terminaron con cero errores: QA conserva 348 advertencias y entrega
346, de shaders Inference/Burst y del servicio Pipeline deshabilitado en Player.
Los logs nativos no tienen errores del juego. Se mantienen avisos heredados del
collider del auricular con escala negativa, fuera del encuentro, y del atlas de
sombras. La Console conserva un timeout del transporte, dos errores de captura
al detener Play (guardados y corregidos) y un intento de build durante
compilación (reintentado con éxito). No se declara una Console vacía.
[Clasificación](VisualEvidence/2026-10-11-Impact/diagnostics.json).

La cadencia nativa instrumentada fue media 7,48 ms/p95 31,31 ms e incluye
captura JPEG y referencia corporal; no acredita rendimiento limpio ni otras Macs.
El segundo tramo nativo ya usa mono/reducido, por eso su PCM no se presenta
como un A/B estéreo de igual volumen. La comparación publicada usa dos partidas
Editor con los mismos ajustes. No se repitió la campaña completa por rutina.

Delta de fuente verificado: [recuperación privada](/Users/lautigr116/EchoProtocol-backups/impact-checkpoint-20261011/RECOVER.txt) y [hashes](/Users/lautigr116/EchoProtocol-backups/impact-checkpoint-20261011/verification.json). Depende de la cadena privada de Embodiment para los modelos sin cambios; no es un backup independiente del proyecto completo.

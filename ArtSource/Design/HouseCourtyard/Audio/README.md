# Transición casa → patio → portería

2026-10-01. Candidato sin importar/compilar/escuchar. No se modificó el autor
congelado ni Assets. El diagnóstico siguiente proviene de código y medidas del
archivo; no es una selección de timbre ni una validación humana.

## Decisión

**Conviene comparar una capa exterior contenida con el silencio actual.** El
patio cambia físicamente de recinto, pero la mezcla existente conserva el aire
institucional 2D al .055 de su nivel inicial. Motor y fluorescente ya quedan
apagados por sus reglas; no se deben reintroducir. Una toma exterior suave puede
separar el cielo abierto del cierre de portería y dejar oír desde fuera las
consecuencias domésticas. No debe anunciar otro susto ni tapar el teléfono.

El candidato es apropiado por su descripción/procedencia, no por una audición:
ahriik, «Quiet Neighborhood Ambience (light wind, no people)»,
[Freesound 481964](https://freesound.org/people/ahriik/sounds/481964/), CC0 1.0
según `ArtSource/Audio/HouseCourtyard/source.json`. Es el HQ MP3 público de 39.039 s,
mono 48 kHz, no el WAV original. `signal.json` mide peak−33.67dBFS/RMS−52.40dBFS.
Es demasiado bajo para juzgar una fuente a .02 como si hubiera una capa útil.

**Conservar silencio sigue siendo preferible a importar ese archivo sin una
comparación perceptible o a subirlo hasta competir con objetos cercanos.** No
se declara aceptado el ambiente. La primera escucha debe comparar desde el
patio y la ventana, a volumen cómodo; no corregir mediante volumen del sistema.

## Estado real de los dueños

- `CampaignRoomTone`: .055 en casa/patio; .20 dentro de la nueva región final;
  .015 tras devolver llave. Preserva apagado institucional. No tocar sus fuentes
  ni añadir la nueva a `institution`.
- `HouseMemory`: si no se atiende, sigue programando llamadas cada 8 s durante el
  patio. Atender corta suministro general; el frigorífico sólo continúa con
  suministro general o reserva en cocina. `LeaveHouse`, al iniciar revisión,
  detiene el teléfono y sus llamadas; no es un evento del patio.
- `HouseRoomAcoustics`: sólo low-pass de teléfono/frigorífico, 22k/1.6kHz y .20s.
  No atenúa sus ganancias ni ofrece oclusión a otras fuentes. El nuevo vidrio y
  muros de Courtyard se incluyen en sus referencias por el autor separado.
- `GameSession` controla `AudioListener.pause`; `SessionAudio` procesa la salida
  con mono opcional y limitación sin boost, y aplica el final Fade. La nueva
  fuente debe pasar por esa misma ruta, sin preferencias propias.

## Una fuente fija, dos regiones exteriores

`CourtyardAir.cs` es el único dueño del volumen/reproducción de **una fuente
nueva dedicada**, no un gestor. Bucle continuo desde inicio; al salir de su
región queda a cero sin reiniciar el cursor. R no lo reinicia ni lo registra.
OnDisable apaga sólo esa fuente. Sin referencias a HouseMemory, Echo o teléfono.

Configuración propuesta, pendiente de cámara/escucha:

| Campo | Valor |
|---|---|
| source anchor | (−7.0,3.4,−63.6), fijo sobre la esquina exterior |
| listener | Cámara real del Player |
| session | GameSession existente |
| mixer | Grupo Ambience existente, misma referencia que frigorífico |
| source | mono, loop, playOnAwake=false, spatialBlend1, doppler0 |
| source distance | Linear, min3m / max16m; spread75° provisional |
| source effects | ignoreListenerPause/Volume=false; bypassListenerEffects=false |
| source gain | .16 provisional, únicamente con derivado +18dB descrito abajo |
| south box | center(−3.7,1.4,−63.55), size(9.4,3.2,2.1), identity |
| west box | center(−6.9,1.4,−57.55), size(3,3.2,14.1), identity |
| boundary fade | .60m; ambos boxes trigger, superpuestos en la esquina |
| time transition | .20s de extremo a extremo; congelada por pausa |

El source está en el patio abierto y no dentro de la casa o bajo el alero. Su
posición en la esquina deja que el ambiente disminuya al acercarse a la ventana
del teléfono y a portería. Es una aproximación puntual a aire/tránsito distante,
no un emisor móvil ni una panoramización pegada a cabeza. Si la escucha revela
un «aparato en la esquina», este montaje no está seleccionado.

Particiones explícitas: las activas de HouseRoomAcoustics más suelo, techo,
fachada y hoja final trasladados. Sólo geometría arquitectónica real; excluir
aparatos, muebles, bodies, triggers y Echo. El componente consulta Collider.Raycast
contra esa lista y lleva su propio volumen a 0 si el camino directo está cerrado.
No calcula difracción/reverberación. El fade puede dejar una cola máxima de .20 s;
no se promete atenuación físicamente exacta ni aislamiento instantáneo.

Los dos boxes excluyen interiores. Deben solaparse: dos cajas que sólo se toquen
producirían un valle de volumen artificial en la esquina. En el portal x−8.4,
el ambiente exterior ya llega a 0; la interacción final está 1.7 m más adentro y
no se añade ningún relleno que responda al corte de teléfono.

## Derivado propuesto; no producido ni seleccionado aquí

Decodificar el HQ preview a PCM 48 kHz, conservar mono, aplicar **ganancia constante
+18dB** y un solapamiento lineal de 1 s entre cola y comienzo para el bucle. Sin EQ,
compresión, voz, capas, normalización dinámica ni synth. No seleccionar un evento
aislado; conservar la toma ambiente entera salvo ese solapamiento de unión.
Guardar hash del original/derivado, ganancia, longitud y medidas reales nuevas.

Sin contar el solapamiento, +18dB daría peak−15.67 y RMS−34.40dBFS. A source .16,
Master actual +10 dB y preferencia .7, el máximo teórico de esa fuente sola sería
aprox peak−24.69/RMS−43.42dBFS antes de distancia/mono/limitador. La receta es
un punto de comparación con margen; no demuestra audibilidad, comodidad o mezcla
adecuada. El mixer actual se leyó del archivo: no se aplica el +8dB histórico de
la documentación a este cálculo ni se modifica su +10dB vigente.

## Comprobación suficiente para decidir

Una ida/vuelta real casa→esquina→ventana→portal: source y sample cursor fijos al
girar/recall; ganancia cero en interiores y tras oclusión; ganancia gradual al
salir/entrar, sin valle en esquina; pausa congela cursor y transición. Atender o
dejar sonar sigue perteneciendo sólo a HouseMemory. Continue en final comienza
con esta capa a 0; no hay reinicio de llamadas. Preferencias/master quedan iguales.

Después una comparación auditiva del mismo tramo con/sin capa, auriculares a
volumen cómodo y mezcla mono/reduced: ¿se abre el espacio sin ocultar teléfono,
pasos ni cierre?, ¿la esquina parece exterior o una fuente puntual artificial?
La implementación puede conservar esas rutas; su resultado audible sigue sin
validar. No hace falta repetir la campaña completa para elegir esta capa.

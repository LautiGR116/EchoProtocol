# Shutdown — mando, máquina y paso

**Receta lista para primera integración, 2026-10-01.** Derivar
`ReadersProduction.ScenePath` a `EchoProtocol_Shutdown.unity`, checkpoint
`EP.CampaignCheckpoint.Shutdown.v1`. No hay cambios de Assets ni consultas Unity
en esta preparación. El integrador conserva Readers y decide con cámara real.

El mando desciende 15 cm y queda por delante de la bomba, con una orientación
oblicua suave. El gabinete se acerca al recorrido: el jugador acciona junto a la
máquina y después pasa entre ambos. No hay zona nueva de interacción, postura
obligatoria, cámara modificada ni nueva mecánica. Se conserva el corte existente
de trabajo, el descenso del motor, la purga localizada y el paso finalmente libre.

## Coordenadas de autoría

Metros Unity; Y arriba. Valores completos y cálculo reproducible en
[`layout.json`](layout.json) y [`prepare_layout.py`](prepare_layout.py).

| Elemento | Transform propuesto | Contrato |
| --- | --- | --- |
| Conjunto mando A | raíz `(5.75,1.35,-35.9)`, yaw adicional `+15°` | Origen anterior `(5.9,1.5,-36.5)`; modelo importado termina yaw285° |
| Pedestal A | mismo transform que mando | Acortar sólo tramo medio15cm; receta debajo |
| `Plant / ServiceCabinet` | raíz `(2.75,0,-39)`, yaw0 | Mantener tamaño/materiales |
| `Central pressure baffle` | centro `(2.75,1.35,-39)`, size `(1.7,2.7,6)` | Mover collider real junto a su representación; renderer heredado sigue oculto |
| Bomba A/B | `(6.89,0,-36.5)` / `(6.89,0,-44)` | Sin mover; puertos/tubería/colliders conservados |
| Receptor / colector | `(0,0,-34.9)` / x7.3,y1.85 | Sin mover |
| Manómetro B / puerta | `(5.918,1.75,-44)` / planoz−48 | Sin mover |

El gabinete ocupa x1.9..3.6, z−42..−36. Los colliders de bombas empiezan en
x6.115: paso libre bruto **2.515m**. Con centro del jugador x4.65 y radio.32,
quedan .73m al gabinete y1.145m a la bomba, antes de descontar skin. Son márgenes
estáticos calculados; no sustituyen la navegación real.

## Agrupar antes de girar

Crear un Empty en el origen viejo `(5.9,1.5,-36.5)`, rotación identidad y escala1,
bajo el propietario actual de `PressureIsolation`. Reparentar preservando pose
mundial estas piezas **de nivel superior**. Después trasladar y girar el Empty:

- `Isolation original / IsolationPanel` y `ControlPedestal`.
- Pivote suelto `Isolation live IsolationLever`; `Isolation current grip` ya es
  su hijo y no recibe una segunda transformación.
- Primer `Isolation original / PressureGauge` y su pivote suelto
  `Isolation live PressureNeedle`. Seleccionar A por posición/referencia, pues B
  comparte nombres.
- `Isolation control interaction body`, textos A `Isolation gauge unit` y
  `Isolation gauge zero`, ambos detents, contrato, placa y dos header brackets.
- Lámpara de instrumento A completa: bracket, hood, diffuser y
  `Isolation reserve instrument light`. Conservar su dirección relativa.
- `Isolation spindle friction` y `Isolation physical detent`; nueva posición de
  ambas `(5.556815,1.35,-35.848236)` por el mismo transform de conjunto.

No mover el root propietario, pivote/manómetro/lámpara B, entryZone, origen de
continuación, luces de trabajo, motor, purga, puerta ni receptor. No recalcular
`controlLocalAxis` ni `needleLocalAxis`: el padre se crea identidad **antes** del
giro, de modo que se conservan las bases locales de los pivotes y el eje compartido
de A/B. Los campos serializados y objetos de interacción conservan identidad.

### Pedestal bajo, conexiones intactas

Variante del original `ArtSource/Blender/IsolationControl/ControlPedestal`, sólo
`PedestalStructure` y `PedestalConduit`. Mantener X/Y, UV, materiales, pivote y
montaje superior. En coordenadas fuente Blender, deformar Z con:

```text
z >= -0.50:            z' = z
-1.30 < z < -0.50:    z' = z + .15 * (-.50 - z) / .80
z <= -1.30:           z' = z + .15
```

La base, sus tornillos y cartelas se trasladan rígidos; el tramo central del poste
y el conducto se acortan. El montaje superior y `CABLE_PORT=(.090,.025,-.368)`
permanecen exactos. `FLOOR_CONTACT` cambia de z−1.50 a−1.35: al instalar la raíz
en y1.35, apoya en y0. No reducir uniformemente toda la altura: separaría el cable
del prensaestopas. No deformar panel, lámpara, grip ni gauge. Es una receta de
malla pendiente de exportación, no un FBX ya validado.

## Reserva local

Reutilizar referencias y circuito de las cuatro luminarias de reserva; no añadir
luz al mando como efecto del éxito. Primera luminaria pasa de pedestal a soporte
fijo en el costado este del gabinete:

- Caja `(3.69,1.08,-36.15)`; bracket `(3.62,1.08,-36.15)`, size `(.16,.045,.12)`;
  cubre el contacto con el lateral x3.6. Retirar sólo su shoe/poste anteriores.
- Fuente `(3.80,1.08,-36.15)`, Spot hacia `(6.174,.535,-36.5)`; color existente
  `(.70,.81,.71)`, intensidad1.1, rango4.3, exterior90°/interior55°. Mantener la
  misma referencia de `LightShadowBudget`. **Valores provisionales para cámara**:
  cambiar Point a Spot no equivale a mantener la iluminancia del motor.
- La carcasa y el difusor orientan su cara luminosa hacia ese mismo destino;
  soporte y caja explican físicamente el foco. No dejar una fuente sin aparato.
- Segunda y tercera luminarias siguen de pie, raíz `(3.72,.38,-39.7)` y
  `(3.72,.38,-43.9)`, orientadas +X; mover todos sus elementos juntos.
- Luminaria final `(1.58,.38,-47.82)` intacta. Instrumento A sigue al conjunto;
  instrumento B intacto. Ninguna reserva entra en `workLights`.

Las reservas existen antes del corte. Deben permitir distinguir carcasa/grilla y
paso una vez apagado el trabajo, con contraste respecto de las lámparas generales.
No se ha observado todavía el rebote/oclusión real de esta receta.

## Marcha y primera comparación

Ruta propuesta de centros de jugador (Y=.03):

```text
(0,-29.6) → (4.40,-30.3) → (4.40,-35.45) [mando]
→ (4.65,-36.8) → (4.65,-44) → (0,-46) → (0,-49.5)
```

El largo geométrico aumenta sólo .062m respecto de los puntos antiguos: no es una
medición de tiempo jugado. La entrada física amplia sigue cruzándose y no hay
desvío añadido detrás del gabinete. Salida y recorrido posterior no cambian.

Cámara inicial de inspección del mando: ojo `(4.40,1.63,-35.45)`, yaw103.175°,
pitch8.410° hacia abajo. Es un punto de inspección, **no un ajuste de cámara del
jugador**. Con FOV vertical60°, mirando el grip MARCHA, la franja ilustrativa
x4.30..4.80 y z−35.50..−35.00 permite el giro completo: alcance máximo1.619m
frente al1.65m existente. Las36 muestras de proyección dejan el contorno frontal
del motor dentro del encuadre; el mínimo inferior es viewporty.0659. Esto no
comprueba oclusión por panel/pedestal ni lectura en oscuridad.

Primera decisión visual: durante el mismo E, poder leer mando y masa del motor
sin cambiar FOV, y distinguir el paso hacia B después del corte. Cámaras oblicuas
adicionales sugeridas en JSON. Si el gabinete/pedestal tapa la bomba en Unity,
corregir esa composición antes de producir el rotor. No exige repetir una campaña
para elegir la primera posición.

## Contratos y límites

Mantener duración1.8s, retorno del mando.45s, motor2.2s, purga3s, alcance1.65m,
R inactivo y P3 inmutable. El runtime permite mirar/moverse mientras conserva
alcance; esta composición no añade una inmovilización. Sólo las fuentes de fricción
y tope siguen su objeto desplazado: clips, mixer, gains, pitch, rolloff y demás
fuentes quedan como están. Motor fijo `(6.89,.55,-36.5)`, purga B
`(7.3,1.85,-44)`; no silenciar otros efectos ni jugador.

Rotor opcional: [`ROTOR_SPEC.md`](ROTOR_SPEC.md). Especificación de una única
pieza persistente ligada al estado existente, sin datos nuevos del Echo.

Evidencia que motivó esta receta: `Docs/VisualEvidence/2026-10-01-Study/continuous/
campaign-07-pressure-isolation-Camera.png` recorta la maquinaria bajo el mando;
`campaign-07-pressure-drained-Camera.png` muestra pared/reserva desde la pose del
probe. Esas cámaras no prueban que un jugador humano nunca mire la máquina.
Se revisaron autores, meshes fuente, contratos de interacción/audio y proyección
analítica. No hubo escucha, Unity render, prueba de colisiones ni playtest humano
de la composición propuesta.

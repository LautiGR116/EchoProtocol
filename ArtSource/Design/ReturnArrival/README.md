# Arrival — volver a la recepción original

**Borrador de autoría, 2026-10-01.** `ArrivalProduction.cs` deriva la escena
`OrientationProduction.ScenePath` a `EchoProtocol_Arrival.unity`; marcador
persistente `Arrival / remembered reception`; key `EP.CampaignCheckpoint.Arrival.v1`.
No se ejecutó Unity ni se editó Assets durante esta preparación.

El cambio es el último acceso de R3: una aproximación oblicua hace que la
ventanilla ya existente quede adelante cuando abre la hoja Hub. La recepción no
se mueve. Se conserva el archivo, la consecuencia de SE1, la esclusa con sus
mismos cierres y el acceso al subsuelo. No aparece un Echo esperando ni se añaden
sonidos, luces, datos grabados, espera o interacción.

La cámara actual de Shutdown `campaign-06-Hub-return-Camera.png` apunta al muro
lateral/subsuelo con yaw90. Es evidencia de composición, no prueba de que alguien
no reconozca el espacio. La recepción frontal está establecida en la cámara de
inicio; la nueva dirección yaw45 intersecta su mostrador cerca de x−.1 al llegar
al plano z−4.5. La composición propuesta aún requiere cámara real.

## Planta y marcha

Todas las coordenadas son **world X/Z**, pies Y≈.03, ojos≈1.63. Archivo y tramo
hasta z7 se conservan. No son instrucciones de teletransporte; el probe debe
caminar con sus controles normales.

| Punto | X | Z | Dirección al siguiente |
|---|---:|---:|---:|
| Salida del archivo, existente | −18.60000 | 15.00000 | yaw180 |
| Tramo existente | −18.60000 | 13.00000 | yaw180 |
| Costura de nueva arquitectura | −18.60000 | 7.00000 | yaw180 |
| Giro amplio A | −18.60000 | −5.89949 | yaw135 |
| Codo J anterior a esclusa | −10.04975 | −14.44975 | yaw45 |
| Hoja posterior, plano | −7.99914 | −12.39914 | yaw45 |
| Interior, centro de volumen | −6.33744 | −10.73744 | yaw45 |
| Hoja Hub, plano conservado | −5.10000 | −9.50000 | yaw45 |
| Cámara de llegada sugerida | −3.47365 | −7.87365 | yaw45 |

Después se gira libremente hacia Station / Descent, en su posición actual.
El trayecto entre la costura z7 y **el mismo plano frontal** pasa de 30m a
31.991m: +1.991m, unos .50s a 4m/s. Esto sólo compara traslación ideal; giros,
aceleración, pausa y espera de hojas requieren medición real. El tiempo mecánico
no cambia.

Paredes con .20m de espesor y separación libre2.60m; el pasamanos bajo reduce
localmente a2.503m. Las hojas conservan abertura2.40m y jambas visibles la unen
con las paredes. El paso existente de z7 conserva sus pilares. No se exige una
posición/cámara exacta para abrir o avanzar.

## Conjunto rígido de esclusa

Se crea primero grupo identidad en F=(−5.1,0,−9.5), se reparentan los objetos con
`worldPositionStays=true`, y sólo entonces se aplica yaw−45 al grupo.
`p' = F + R(−45°) × (p−F)`. No se sustituyen rotaciones locales de las hojas.
Los cuerpos conservan su compensación local yaw−90 y todos sus targets locales.

| Referencia / objeto | Nueva posición XYZ | Rotación |
|---|---|---|
| `R3 arrival rear` | (−7.99914,0,−12.39914) | world yaw45 |
| `R3 Hub exit only` | (−5.1,0,−9.5) | world yaw45 |
| `arrivalInterior` / `R3 arrival clear of rear` | (−6.33744,1,−10.73744) | yaw−45 |
| `arrivalClear` / `R3 Player fully inside original Hub` | (−4.03934,1,−8.43934) | yaw−45 |
| `arrivalOccupancy` / `R3 entire arrival occupancy` | (−6.54957,1,−10.94957) | yaw−45 |
| `Return / actual arrival zone` | (−3.93327,1,−8.33327) | yaw−45 |

También viajan **ambos headers**, y los dos roots individuales
`R3 arrival rear physical motor` / `R3 Hub exit only physical motor`, inicialmente
hijos de Registration, **no de las puertas**. Se resuelven por
`SlidingDoorAudio.door`; quedan 2.75m sobre cada mecanismo, con los mismos
AudioSource, Rigidbody, clips, bus WorldSFX, volumen y distancias. No siguen la
hoja móvil. Los cuatro volúmenes conservan sizes y referencias.

No viajan `residualRoot`, `arrivalLightRoot` (ambos roots al origen), Registration,
la recepción, el punto de R1, el final crossing de P3 ni su puerta de salida.
El `returnGate` permanece en (−17.2,0,46.4). Los dos rebotes existentes del Hub y
el residuo de cortina mantienen mundo y activación. D1 permanece deshabilitado.

**Dependencia ya integrada por root:** tanto `VS1HubReturnGate.Inside` como
`CampaignDirector.AtReviewAnnouncement` usan posición local del BoxCollider con
sus checks enabled/active, no el AABB mundial. Este autor no modifica runtime.

## Envolvente y colisiones

Se mantienen inactivos los originales; el autor no elimina objetos de origen.
Se retiran sólo `R3 south end`, `R3 arrival floor/ceiling/north wall/south wall`,
`R3 Hub west south pier`, `R3 return wayfinding`, la vieja placa y respaldo
`Reception visible at return end`, y los cuatro listones inferiores del tramo
sur. `Hub West Wall`, `Hub South Wall` y el collider antiguo de
`R3 spine west wall` siguen inactivos/deshabilitados.

Se acortan hacia el norte sin mover su extremo norte:

- `R3 spine floor`: z[7,48.2]; ceiling z[7,48.3]; east wall z[7,43.4].
- `Return west south wall` y su collider separado
  `West original south perimeter`: z[7,14]. El resto del archivo queda intacto.

La nueva planta es una cinta con ingletes en A/J, suelo y techo como mallas
estáticas originales sencillas. Dos MeshColliders separados de los renderers;
paredes/jambas en BoxColliders separados. Piso superiorY0, techo inferiorY3.2.
El último faldón llega 1.5m después de F y se **recorta en x−5**, donde comienzan
los slabs existentes del Hub: evita superponer pisos coplanares.

Cierres del encuentro Hub, con paredes reales de .20m:

- Norte: (−6.08995,−8.51005) → (−5.1,−8.2), extremo sur del pier medio retenido.
- Suroeste: (−4.11005,−10.48995) → (−3.75,−12.1), sobre el pier izquierdo de
  salida sur existente. Sustituye el antiguo pier oeste sur, que atravesaría
  la nueva esclusa si se conservara.

Estos encuentros no atraviesan el paso del subsuelo x±1.7 ni la cámara inicial
(0,−9.6). El suelo del Hub sostiene el cierre sur. La pequeña esquina suroeste
queda tras una pared visible, no tras un bloqueo invisible.

## Luz, sonido y rótulo existente

Se reutilizan exactamente las luminarias y sus referencias/potencia/materiales:
las triples housing/diffuser/Light en (−18.6,−8) y (−14,−9.5) pasan a
(−16.47868,−8.02082) y (−12.94315,−11.55635), orientadas al enlace. Las triples
(−10.5,−9.5) / (−7,−9.5) viajan con la esclusa a
(−8.91838,−13.31838) / (−6.44350,−10.84350). Conservan alturas3.10/3.03/2.75.
Las luminarias de z2/−3 siguen en la espina; ningún aumento de intensidad ni
cambio a PracticalEmission/LightShadowBudget. No se añaden fuentes sonoras.
La nueva relación espacial de sonidos existentes necesita audición humana.

El texto flotante/espejado `RETURN ACCESS / EXIT ONLY` se reutiliza como
**`RETORNO · SOLO SALIDA`** en una placa opaca de1.60×.23×.026m, sobre la cara
Hub del dintel. CentroF+Forward×.214+up×3.07; tinta aForward×.229, normal hacia
Hub, fuente original y `DiegeticText.depthTestedShader=PrintedText`. Desde la
esclusa lo oculta su respaldo y el dintel. Sin rótulo en mitad de la nueva vista,
sin luz ni emisión propia. La lectura desde el Hub depende de luz real.

## Alcance de comprobación y primera integración

El borrador contiene guardas de baseline, destino nuevo, Play detenido y escenas
limpias. Guarda primero como Arrival. Compara serialización de componentes
runtime, luces/audio, colliders no redimensionados, cuerpos, renderers originales
(excepto tinta) y poses ajenas. Sólo cambia key en CampaignDirector. El reporte
se escribirá **al ejecutar** en `2026-10-01-Arrival/authoring.txt`; no existe aún.

Inspección estática/filesystem y cálculo de planta realizados. **No compilado ni
probado en Unity todavía.** Primera integración: cámara real yaw45 a puerta
abierta y desde Hub inicial; caminar los dos giros/costuras; comprobar cierre,
retirada/reentrada, motores fijos y guía una vez. Si el dintel o luz heredada
ocultan la ventanilla, ajustar desde esas cámaras, no sumar prop/gate/texto.
Reconocimiento espontáneo, tensión, confort y mezcla no están validados por
estas coordenadas ni por una futura ruta automática.

## Primera integración del integrador

ArrivalProduction compiló y se ejecutó vía Editor API. Escena Arrival guardada/limpia e informe final creado:4076 serializaciones originales conservadas, 138 luces/54 fuentes,607→621 colliders contando retirados inactivos. El conector excedió5s; lectura posterior del informe y estado de escena confirmó que finalizó. No se repite el autor ni se sobrescribe Orientation. Cámara/marcha siguen pendientes al escribir esta nota.

Primera marcha focal: PASS17/15.074s con una fixture inicial, cero resets posteriores. Las cámaras05/06 muestran recepción/mostrador en el avance normal. La revisión detectó dos juntas y el zócalo del muro retirado atravesando el vano: se desactivan `Hub panel seam -1 2/3` y se recorta `Hub skirting -1` a z[−8.2,−4.1]. Sin colisión ni estado nuevos. Este acabado visual aún necesita su cámara posterior; se prepara rama de retirada durante cierre para la misma geometría.

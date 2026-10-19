# Orientation — composición de R1

Autoría integrada mediante Editor API; **cámara/input focal comprobados; experiencia humana pendiente**. Se derivó la escena `EchoProtocol_Orientation.unity` de `ShutdownProduction.ScenePath`. Reutilizar el puesto de Recovery y la entrada de FirstAct; no cambiar reglas, checkpoint, P3, aperturas, sonido ni estados de SE3.

## Qué resuelve y qué no demuestra la evidencia

`Docs/VisualEvidence/2026-10-01-Orientation/baseline-virtual/entry-west.png` muestra silla/mesa dominando el primer plano y poca continuidad del perímetro. `entry-turned-northwest.png` **ya muestra la salida y el puesto juntos**: no hay evidencia de una salida intrínsecamente invisible. Sí muestra `RETURN GALLERY` grande, suspendido y superpuesto a una luminaria. `from-center-to-gallery.png` confirma esa superposición.

Son cámaras clonadas estacionarias después de completar Shutdown, con puerta abierta y pantalla activada. No demuestran la entrada espontánea, el estado previo a E, recuperación emocional ni audición. El propósito es despejar el primer plano y hacer legible un puesto estable dentro de una habitación, conservando el residuo doméstico distante. Comparar después con pantalla neutral/puerta cerrada y la iluminación heredada de la ruta real.

## Un grupo de trabajo, sin extender E a los muebles

Crear un Empty `Orientation / R1 workstation placement` **dentro de `R1 Clinical Observation Hub`**, en `(-7.55,0,27.80)`, rotación identidad y escala uno. Reparentar con `worldPositionStays=true`, antes de mover/girar. Transformación mundial única:

```text
p' = (-7.50,0,28.50) + RotationY(+30°) * (p - (-7.55,0,27.80))
rotation' = RotationY(+30°) * rotation
```

Incluir el root existente `R1 Central observation station` completo. Sus hijos `R1 Monitor housing`, `R1 Monitor stem`, `R1 Neutral inactive display`, `R2 stable anchor confirmation` y `R2 station instruction` viajan una sola vez por ese root. Preservar los estados inactivos de `R1 Station worktop`, `R1 Station inset` y `R1 Station solid plinth`. **El Renderer de `R1 Monitor housing` está oculto, pero su BoxCollider sigue siendo la superficie real de interacción: no desactivar su GameObject.**

Los siguientes objetos viajan como **hermanos de `R1 Central observation station`**, bajo el nuevo Empty; nunca como hijos de ese root interactuable:

- `Recovery original review desk` y `Recovery desk solid envelope`.
- `Recovery original monitor casing`.
- `Recovery original jointed lamp` y `Recovery desk task pool`.
- Los tres `Recovery reviewed form` y `Recovery desk pen`.
- `Recovery clerk chair` y `Recovery chair collision`.

`PlayerInteractor` resuelve `GetComponentInParent<IInteractable>()` desde el collider golpeado: poner escritorio/silla bajo `VS1RecallAnchor` convertiría superficies nuevas en E. No mover todo `Recovery / staffed records review`: contiene también techo, archivos y pared. No mover `R1 Clinical Observation Hub`, access/residual roots, vidrio, arquitectura, fuentes de audio ni la puerta/lectores de P3.

**Excepción indispensable:** cachear antes el world position/rotation exactos de `R2 R1 recall point` y restaurarlos después del movimiento, conservando su misma referencia Transform. Actualmente: posición `(-5.70,.03,27.80)`, quaternion `(0,.3826835,0,-.9238795)` — yaw 315°. No sustituirlo por el nuevo puesto ni deducirlo del Player.

Conservar las referencias serializadas de `VS1RecallAnchor`: `recoveryHub`, `loop`, `player`, `recallPoint`, `activeDisplay` → `R2 stable anchor confirmation`, `statusLabel` → `R2 station instruction`, `galleryAccess`. E sigue fijando el checkpoint, iniciando la grabación desde la pose real de interacción y abriendo la misma puerta; textos `FIJAR ORIGEN\n[E]` / `ORIGEN\nFIJADO`. Sin teleport ni cambio de almacenamiento.

## Destinos del puesto

Metros, mundo Unity; cifras redondeadas a 1 mm. La transformación rígida anterior manda sobre los redondeos. «Yaw de colocación» conserva la corrección/importación FBX actual; **no reemplazar la rotación importada por Euler puro**. Los colliders parten de yaw 0 y reciben delta +30°.

| Objeto | Posición nueva | Orientación/dimensión conservada |
|---|---|---|
| Review desk | `(-7.500,0,28.500)` | Colocación 90° → 120° |
| Desk solid envelope | `(-7.500,.380,28.500)` | Yaw 30°; tamaño `(.79,.76,1.65)` |
| Monitor casing | `(-7.223,1.480,28.340)` | Colocación 90° → 120° |
| Hidden monitor collider | `(-7.283,1.480,28.375)` | Delta +30°; tamaño `(.10,.48,.80)` |
| Neutral display | `(-7.232,1.480,28.345)` | Delta +30° |
| Active display | `(-7.210,1.480,28.333)` | Delta +30° |
| Station instruction | `(-7.193,1.500,28.323)` | Delta +30°; mismo tamaño |
| Clerk chair | `(-7.510,0,27.143)` | Colocación 300° → 330° |
| Chair collision | `(-7.510,.480,27.143)` | Yaw 30°; tamaño `(.66,.96,.66)` |
| Jointed lamp | `(-7.231,.760,29.026)` | Colocación 90° → 120° |
| Task pool/source | `(-7.083,1.145,28.941)` | Aim `(-7.013,.962,28.900)` |
| Reviewed forms ×3 | `(-7.080,.766/.768/.770,28.847)` | Yaw 38°/40°/42° |
| Desk pen | `(-7.123,.777,28.652)` | Yaw 30° |

La lámpara conserva `PracticalEmission.source`, el diffuser, intensidad 2.3, rango 2.4, cono 100° y color `(1,.84,.61)`. El Light es un objeto separado: mover sólo la malla dejaría un charco de luz sin lámpara. Sus anchors `TaskLamp__LIGHT_SOURCE` / `TaskLamp__LIGHT_AIM_TIP` deben coincidir con fuente/aim después de transformar.

## Luz local candidata, sin lavado global

Reutilizar las dos ternas `Recovery retained fluorescent housing` / `Recovery opal panel` / `Recovery local working light`. Identificar cada terna por **posición original y referencia `PracticalEmission.source`**, porque los nombres están repetidos. No crear luces ni cambiar ambiente, SE1, exposición o mezcla. Conservar sus referencias en `LightShadowBudget`.

| Terna | Posición housing / opal / Light | Aim propuesto | Potencia/rango/conos |
|---|---|---|---|
| A, originalmente x −5.55 | Sin mover: `(-5.55,3.05/3.005/2.98,28.70)` | `(-5.55,.80,29.70)` | Conservar 3 / 5.2 / 105°; inner 75.6° |
| B, originalmente x −8.55 | `(-7.30,3.05/3.005/2.98,27.10)` | `(-7.60,1.00,29.40)` | Conservar 4 / 5.2 / 105°; inner 75.6° |

Conservar color `(.76,.83,.76)`, materiales opal y valores de PracticalEmission. El Light de B ilumina el frente del puesto y el plano norte, en vez de quedar detrás de la pantalla. Mover `Recovery ceiling indirect return` de `(-7.55,2.85,27.60)` a `(-6.65,2.85,27.60)`; conservar color `(.74,.80,.72)` e intensidad 1.4, probar rango 4.2 en vez de 5.6. Es la aproximación indirecta local ya existente; el rango menor intenta contener su alcance al residuo cálido. No es GI calculada ni iluminancia validada.

Conservar apagados `R1 panel-owned stable light`, `R1 recessed light housing`, `R1 stable cool diffuser`. Estos ajustes son un punto de partida de cámara: si lavan el residuo o no mejoran el perímetro, reducir/revertir su contribución local, no compensar con ambient global.

## Leyenda contenida en un soporte

Reutilizar el TextMesh `P3 threshold identity`; reemplazar `RETURN GALLERY` por **`GALERÍA DE RETORNO`**. No mover puerta, header o luminarias de P3.

- Soporte disponible: `R2 Anchor gallery access header`, centro `(-5.5,3.1,30.2)`, tamaño `(2.1,.4,.4)`. Cara sur z 30.0; borde inferior y 2.9, igual al techo de la hoja cerrada.
- Placa nueva sencilla, sin collider: centro `(-5.5,3.035,29.989)`, tamaño `(1.75,.20,.026)`. Dorso z 30.002 entra 2 mm en el header; frente z 29.976. Está por encima de la hoja y dentro del header.
- Mover el TextMesh a `(-5.5,3.035,29.974)`, conservando su rotación actual legible desde el sur. Una línea, centro; ajustar a bounds de texto **≤1.55 m de ancho y ≤.115 m de alto**, fontSize 72. No reutilizar characterSize .035 sin medir el texto nuevo.
- Placa de esmalte mate claro y tinta oscura, usando materiales locales derivados de Recovery; sin emisión. Usar `EchoProtocol/PrintedText` y actualizar **también `DiegeticText.depthTestedShader`**: sólo cambiar el material de Editor se pierde en Awake. Mantener el atlas de la fuente como ya hace el componente.

El montaje alto evita la luminaria que hoy atraviesa el texto. Debe leerse como nombre del lugar sobre una pieza arquitectónica, no como instrucción nueva. Falta comprobar en cámara con luz real que la placa reciba suficiente luz; no resolverlo con texto autoiluminado.

## Marcha propuesta y límites físicos

Waypoints de suelo, y .03; caminar con controles reales, sin giro forzado ni esperas:

```text
codo (-2.70,27.015) → entrada (-4.30,26.95)
→ puesto/checkpoint existente (-5.70,27.80), E
→ (-5.50,29.35) → (-5.50,32.50)
→ (-7.80,32.50) → (-7.80,35.20), continuar P3 existente
```

Desde ojo `(-5.70,1.63,27.80)` al monitor nuevo: distancia aproximada 1.62 m, yaw 289.5°, pitch 5.3° abajo; dentro del alcance actual de interacción 3 m. La línea está sobre la tapa (.76 m) y el respaldo (.939 m); todavía requiere comprobar el collider realmente golpeado. No se cambia la pose mundial del checkpoint para acomodarla.

Cámara de planificación al atravesar `(-4.30,1.63,26.95)`, yaw 315°, pitch 0: el monitor queda hacia yaw 295° y la salida hacia 340°. Son referencias de encuadre amplio, **no una cámara que deba imponer el juego**. Después de E, un giro libre hacia noroeste muestra el destino. No se promete que monitor y puerta entren juntos mientras se apunta exactamente a E.

La apertura de FirstAct en x −4.05, z 26.10..27.80 se conserva. El nuevo collider de mesa ocupa AABB x `[-8.255,-6.745]`, z `[27.588,29.412]`; el mesh llega aproximadamente a z 29.424. Queda una franja bruta de 2.595 m entre el collider y la pared este interior x −4.15. La ruta x −5.50 mantiene 1.245 m al collider de mesa; con radio de Player .32 quedan aproximadamente .925 m antes de skin. La silla queda fuera de entrada→E y de esa franja.

Los gabinetes norte no se mueven: el más próximo conserva unos .18 m hasta el mesh del escritorio, separación de muebles **que no se ofrece como pasillo**. Tampoco se propone circular detrás de la mesa (unos .676 m al muro norte). No se reduce ningún vano ni se añaden colliders de guía. Las cifras derivan de manifests y autoría, no de marcha ejecutada.

## Alcance de la siguiente inspección

Comparar entrada real con pantalla neutral/puerta cerrada; E debe seguir golpeando el monitor y no el escritorio. Confirmar que la nueva composición despeja el acceso, que el perímetro estable y la placa se leen sin destruir el contraste con SE3, y que mesa/lámpara/pantalla siguen apoyadas/alineadas. Verificar el mismo world pose/ref de recall antes y después de autoría. La marcha, primera lectura espontánea, comodidad y efecto de recuperación requieren observación en juego; las cámaras virtuales no los aprueban.

Fuentes: `Assets/Editor/RecoveryProduction.cs`, `FirstActProduction.cs`; `VS1RecallAnchor.cs`, `VS1RecoveryHubGate.cs`, `PlayerInteractor.cs`, `DiegeticText.cs`; escena serializada Readers sólo leída y cámaras virtuales de Shutdown; `ArtSource/Blender/RecoveryOffice/manifest.json`, `ReceptionLife/manifest.json`. No modelo ni sistema runtime nuevo.

## Integración 2026-10-01

`Assets/Editor/OrientationProduction.cs` ejecutado: escena Orientation guardada y limpia. El guard inicial de objetos legacy se corrigió para verificar las cuatro luminarias con el mismo nombre bajo R1, en vez de exigir nombre único. Informe `Docs/VisualEvidence/2026-10-01-Orientation/authoring.txt`: 607 colliders, 54 fuentes y 138 luces conservados; 859 serializaciones y 602 poses de colliders ajenos preservadas. Mismo Transform/pose de RECALL. La llamada de autoría excedió los 5 s del conector, pero el informe final y la escena guardada confirman ejecución completa. Esto no es evidencia de juego.

La iteración final suma placa secundaria en el frente de la repisa (−9.10,1.82,29.696), .75×.10×.012 m, tinta .65×.055 m máxima. Es ambiental y puede quedar oculta por el monitor; se retiró su antigua jerarquía flotante. Survey final PASS10/5.464s con una fixture inicial; ver README de evidencia. Se integra en Arrival, sin build separada.

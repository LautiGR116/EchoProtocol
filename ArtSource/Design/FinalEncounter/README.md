# FinalEncounter — brief técnico de autoría opcional

2026-10-01. **Integración compilada y campaña Encounter PASS436; build universal y firma comprobadas, nativo pendiente por bloqueo del Mac.** Brief inicial basado en lectura de `EchoProtocol_Reserve.unity`, `ThresholdProduction`, `FinalDoorMemory`, `FinalRecordDisplay`, `HouseSwingDoor`, `PlayerInteractor`, `ThresholdCampaignProbe` y `FinalDoorRouteProbe`. En la fase inicial de este brief no se modificaron Assets ni escenas; la integración posterior figura al final. Las distancias siguientes son cálculo geométrico sobre datos serializados, no una prueba de colisión, iluminación o comprensión humana.

Objetivo: el jugador abre 6B desde adentro y observa directamente su llegada real. El visor anticipa la figura; la acreditación ocurre por el vano libre, cerca del umbral. No se cambia la toma, la pose, el reloj, la pausa mínima de 2 s ni la observación continua de 0,6 s. La puerta la mueve el jugador presente. Puede prepararla antes de que llegue el Echo.

## Geometría existente comprobada

Coordenadas Unity en metros; raíz `Threshold / del otro lado` en identidad. Interior: z > −79. La figura mira +Z; el observador mira −Z.

| Elemento | Valor actual |
|---|---|
| Origen de R | (0, .03, −76), yaw 180°; cámara local y=1.6: ojos y≈1.63 |
| Jugador | velocidad 4 m/s; cápsula altura1.8, radio.32, skin.03; alcance de interacción3 m |
| Hueco entre paredes | x[−.56,+.56], plano z=−79; paredes grosor.18, cara interior z=−78.91 |
| Bisagra | (−.48,0,−79); gira +105° sobre Y a50°/s, hacia el exterior |
| Hoja inferior opaca | centro(0,.55,−79), tamaño(.96,1.10,.065); cara interior z=−78.9675 |
| Montantes | centro x±.43,y1.585,z−79; tamaño(.10,.97,.065) |
| Travesaño | centro(0,2.02,−79), tamaño(.96,.14,.065) |
| Visor | centro(0,1.525,−79), tamaño(.76,.85,.022): x±.38, y[1.10,1.95] |
| Manijas visuales | interior(.35,1.02,−78.91), exterior(.35,1.02,−79.09); tamaño(.15,.028,.03) |
| Zona exterior válida | centro(0,1,−80.25), tamaño(1.6,2.2,1.2): x±.8, z[−80.85,−79.65] |
| Zona interior actual | centro(0,1,−77.3), tamaño(2.8,2.2,2.8): x±1.4, z[−78.7,−75.9] |

Las manijas son geometría **sin collider propio**. La interacción se resuelve desde colliders de hoja/montantes/visor hacia `FinalDoorInteraction` en la bisagra. El ray horizontal desde la posición de R atraviesa el visor a unos2.989 m de la cámara: está dentro del alcance3 m. Apuntar a la manija desde exactamente el origen queda en el límite porque el collider real está detrás del mango. Para una entrada robusta, avanzar a z−76.2, usar E en la hoja/visor y acercarse mientras gira; no cambiar el alcance global.

## Franja propuesta y margen

Sólo en la escena nueva, reutilizar `insideWitnessZone` con centro **(0,1,−78.15)** y tamaño **(.70,2.2,.40)**: x±.35, z[−78.35,−77.95]. Punto recomendado de observación(0,.03,−78.15). No mover el origen de R. Se conservan los colliders sólidos y el control de obstrucción de la bisagra.

| z del observador | Distancia horizontal a Echo centrado z−80.25 | Mínima a cualquier raíz exterior válida | Margen ante pared cerrada, descontando radio+skin |
|---|---:|---:|---:|
| −77.95 | 2.30 m | 1.70 m | .61 m |
| −78.15 | 2.10 m | 1.50 m | .41 m |
| −78.35 | 1.90 m | 1.30 m | .21 m |

Toda la franja queda por encima del fade completo de **.95 m**, incluso con el Echo en el borde exterior más próximo. Frente a la captura actual a z≈−77.245, la cabeza/torso se ven aproximadamente1.4 veces mayores en el punto central; no implica que el cuerpo entero entre en pantalla a pitch0. La hoja abre hacia z menores, alejándose de esta franja. Es margen analítico, no certificación física.

La zona exterior admite poses laterales hasta x±.8: algunas líneas de visión pueden quedar tapadas por el marco o la hoja abierta. Los rayos deben rechazar esas oclusiones reales, no ignorar el marco para facilitar el éxito. Comprobar que existe una posición alcanzable dentro de la franja para esos extremos; si no, ajustar **sólo la autoría nueva** de la franja o el área exterior señalizada. La ruta central no demuestra cobertura de todos los extremos.

## Tiempo: no convertir la revisión en una carrera

`CanUse(OpenDoor)` permite abrir desde que la revisión está activa, sin esperar al Echo ni a `HasWitnessed`; R restaura la puerta después de trasladar al jugador al origen. A50°/s,105° requiere **2.10 s** sin obstrucción. Para un ray central x=0, el borde completo de la hoja deja el eje aproximadamente a62°/**1.24 s**; este cálculo conservador incluye el grosor, pero el runtime debe usar visibilidad real, no ese ángulo ni un temporizador.

El desplazamiento origen→franja es2.15 m/**.54 s** nominales, simultáneo a la apertura. La ruta prescrita en ambos probes recorre14.35 m antes de la pausa: **3.59 s de marcha como mínimo a4 m/s**, más giros/lecturas/paradas. Abriendo después de R y antes de la llegada hay margen para completar la apertura durante el trayecto del Echo y observar sus2 s sin añadir espera. No es garantía sobre cualquier ruta humana más corta ni sobre abrir tarde: si se espera al comienzo de la pausa para abrir,1.24+.6=1.84 s deja un margen de reacción inaceptablemente pequeño. La guía debe invitar a preparar la puerta desde el inicio de la reproducción.

Muestreo actual.05 s; `RecordingReady` exige2 s + dos intervalos:≈2.1 s. La comprobación de pausa mínima debe pulsar R al encenderse esa señal y medir el tramo inmóvil que realmente quedó en los frames, en lugar de conservar los4.2 s actuales del probe. Informar el intervalo obtenido; no llamarlo “exactamente2 s” por usar un Wait(2). Se espera un tramo cercano a2.1–2.3 s, con umbral de calificación sin modificar en2 s.

## Cambios mínimos previstos, aún no implementados

- `FinalDoorMemory`: bool serializado opcional `requireDirectEncounter=false`, expuesto sólo lectura como `RequiresDirectEncounter`. False conserva la rama legacy. True requiere la franja anterior, `domesticDoor.IsOpenRequested` y visibilidad de **dos puntos reales** del cuerpo reproducido: pecho1.30 m y cabeza≈1.64 m sobre la raíz. Refactorizar el ray actual para comprobar cada punto, viewport y oclusión. En esta rama el vidrio del visor bloquea la acreditación: no aplicar su excepción transparente. No exigir `IsFullyOpen`; acreditar tan pronto ambos rayos pasan por la abertura y cumplen los mismos.6 s. Mantener identidad de toma, conformidad de pose, qualifiedPauses, pausa/resets y conservación exactamente como están. Un proxy ColorMask0 de Presence no debe contar como “cuerpo visible” si el renderer de color está apagado: revisar ese filtro al integrar la copia final.
- `FinalRecordDisplay`: usar `RequiresDirectEncounter` para la copia nueva de los textos; antes de observar: título `REGISTRO DE SESIÓN`, detalle `6B / POSICIÓN Y ORIENTACIÓN`, acción vacía. Después: `TOMA ACTUAL`, duración real/metadatos actuales y `E  CONSERVAR ORIGINAL`. Conservar `ORIGINAL CONSERVADO`, `DEVOLVÉ LA LLAVE` y `LLAVE RECIBIDA` posteriores. No afirmar “verificación” de un recuerdo. Rama false deja intactos los textos legacy.
- Nueva autoría `FinalEncounterProduction`: copiar la última escena **aceptada** a una escena nueva, activar el bool, ajustar únicamente su zona interior y textos. Placa: `6B / ORIGEN INTERIOR\nREGISTRÁ TU LLEGADA\nR · VOLVER Y RESTABLECER 6B`; ayuda H durante reproducción: `Abrí 6B desde adentro y observá tu llegada.` Conservar ayuda de error/sustitución de toma, señal exterior y navegación lateral. `Objective`/`StatusText` sólo aparecen en H actualmente; no retirar un HUD que no existe. Usar clave de checkpoint nueva y referencias propias; no modificar Reserve ni el prefab/material global. Conservar origen, toma, puerta, audio causal y final posterior.

## Adaptación de las dos rutas

**ThresholdCampaignProbe.FinalAct:** detectar `RequiresDirectEncounter`. En la rama nueva sustituir Wait(4.2) por llegada a `RecordingReady` y R real; después Go(0,−76.2,180), E real para abrir **antes** de esperar la llegada, Go(0,−78.15,180) y observar. Capturar llegada/vano limpio desde esa cámara. Comprobar que la puerta fue solicitada antes del inicio de la pausa cualificada, que se logra el testigo en ese intervalo mínimo real y que no cambian identidad ni frames. Mantener cuatro recalls, cero fixtures y todo el tramo terminal/llave/salida. La rama legacy mantiene sus waypoints y tiempos actuales.

**FinalDoorRouteProbe:** misma rama de preparación temprana en `WitnessArrival`. Full prueba pausa mínima con input real; visual/rules pueden conservar una toma más larga **etiquetada para diagnóstico**, para mirar por el visor cerrado durante>.6 s y demostrar que no acredita, luego abrir y acreditar la misma toma. En `ObserveRejectedTake`, abrir y entrar en la franja también: de lo contrario wrongfacing/short pasarían por estar la puerta cerrada, sin comprobar el defecto de la toma. Preservar prueba de Escape durante un intervalo parcial, sustitución por R, guards y salida física. Añadir comprobación separada de observación desde el origen, marco opaco y vuelta al mirar; sin teletransportes nuevos. La fixture inicial de casa/pose sigue explícita y no se presenta como campaña jugada. Guardar nuevos resultados bajo FinalEncounter, sin sobrescribir evidencia de FinalDoor/Reserve.

Aceptación objetiva: puerta preparada antes de llegada; vistazo por cristal/origen no basta; ambos puntos del cuerpo visibles por aire libre en la franja sí acreditan dentro de una pausa real cercana al mínimo; ni interfaz ni estado producen una toma nueva. Aceptación perceptible pendiente: el jugador descubre el gesto, se acerca y reconoce la llegada sin sentir un trámite de temporizador. Las capturas y el cálculo no sustituyen esa partida humana.

## Decisión de integración posterior

La aceptación se amplió a centro(0,1,−78.05), tamaño(1.6,2.2,.9):
x±.8 y z−78.5..−77.6. La cámara preferida sigue en−78.15, pero no se exige
un punto invisible de40cm. Distancia mínima a cualquier raíz exterior1.15m,
todavía fuera del fade completo. Puerta solicitada y ambos rayos sin obstrucción
siguen siendo necesarios. No se cambiaron pausa, movimiento ni el origen de R.

La placa ahora dice REGISTRÁ TU LLEGADA / VOLVÉ CON R / ABRÍ PARA RECIBIRTE.
Esto señala la preparación de la puerta antes de la llegada; la estrategia del
probe no equivale a que un jugador nuevo la descubra. El material de color queda
referenciado explícitamente para excluir el proxy de profundidad del testigo.

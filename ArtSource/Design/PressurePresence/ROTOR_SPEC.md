# Rotor opcional de la bomba A

Especificación concreta con variante fuente exportada en
`ArtSource/Blender/ShutdownMotor/`; selección en cámara pendiente. Composición primero. No
requiere audio nuevo ni modificación de presión, tiempo, puerta o grabación.

## Abrir la carcasa existente

Original: `ArtSource/Blender/PressurePlant/build_pressure_plant.py`, bloque
MotorSkid. Modelo original del proyecto, una malla unida de15524tri. El supuesto
ventilador está cerrado: `Rear fan housing` es un cilindro con tapas y `Rear fan
recess` otro disco opaco. Añadir una pieza detrás de la rejilla actual no basta:
quedaría ocultada. Derivar una variante de **la bomba A** manteniendo skids, puertos,
tubería, material y collider. B no necesita animación ni un nuevo sonido.

Convención fuente Blender X horizontal / Z arriba; eje de motor+X. Conversión
Unity corregida `(X,Z,-Y)`. Raíz de bomba A `(6.89,0,-36.5)`, yaw0.

| Pieza | Dimensiones fuente propuestas |
| --- | --- |
| Carcasa anular fija | EjeX, centro Y0,Z.535; extremosX−.7125..−.5975; radio exterior.245, bore.190 |
| Rejilla fija | X−.723, radio.006, nueve alturas separadas.033; extremos al aro fijo radio.212, espesor.007 y cuatro standoffs a carcasa |
| Fondo opaco | Plano frontalX−.633, radio.190, material `PP_Recess` |
| Rueda móvil | Pivote `(-.682,0,.535)`, espesor.006, radio máximo.160, cubo central.035 |

La rueda ocupa X−.686..−.679, y el cubo/tapa alcanza X−.6985: queda detrás de la rejilla y delante del fondo;
mantener el hueco real y normales orientadas hacia−X. No escalar/rotar toda la
bomba ni sus conexiones. Pivote Unity final `(6.208,.535,-36.5)`, eje+X. La
variante conserva el sobre original X−.76...79,Y±.40,Z0..1.01 y collider1.55×.86×.80.

## Apariencia y movimiento candidato

Una rueda mate casi continua, reborde fino y cubo, con una asimetría superficial
amplia de contraste bajo. Evitar aspas aisladas muy claras, rayas finas, dientes,
paletas repetidas de alto contraste y emisión. Reutilizar `PP_CastIron`/
`PP_Recess`; la reserva real tiene que revelar el volumen. Presupuesto adicional
<1200tri, un renderer móvil. No introducir luces, estrobo ni shader de efecto.

Primer valor para cámara: **1440°/s × MotorLevel01**, ligado a la caída existente
de2.2s. Es un parámetro de presentación provisional, no una afirmación de RPM
físicas o correspondencia auditiva. La rueda debe ser difícil de resolver en
marcha y ofrecer una breve desaceleración reconocible al final; en el último.55s
recorre aproximadamente99°, hasta reposo. Rechazar el candidato si parece un
ventilador lento, invierte visualmente su giro por aliasing o distrae con parpadeo.
La forma oscura/casi continua limita el contraste temporal; la cámara real decide
si alcanza. No aceptar automáticamente por registrar cambios de ángulo.

## Estado y persistencia mínimos

Un adaptador optativo pequeño con referencias explícitas `PressureIsolation`,
`GameSession` y `Transform rotor`, sin dueño de audio:

- Cuando `session.HasStarted`, motor>0 y no pausa, integrar ángulo mediante
  `Time.deltaTime`; el motor ya funciona antes de `isolation.Active`.
- Pausa/documento congela igual que el audio/mando. No usar tiempo no escalado.
- Leer `MotorLevel01`, no Chapter ni un timer nuevo. Al cero conservar el último
  ángulo: no rearmarse al salir ni al mirar atrás.
- Si `RestoreRevision` cambia, no simular coast ni reproducir eventos. La
  restauración aislada ya entrega motor0; un ángulo canónico de reposo es válido.
  Restauración no aislada permite marcha bajo el estado existente.
- Sin PlayerPrefs, grabaciones, one-shots, partículas, velocidades/posiciones del
  Echo ni nueva condición de puerta.

La variante exportada tiene17776tri, de los cuales952 son el rotor; su roundtrip
Blender pasa35 comprobaciones de geometría. El adaptador de lectura `PressureMotorRotor` ya está integrado en Shutdown;
la evidencia runtime/cámara se registra en Docs/VisualEvidence/2026-10-01-Shutdown/.
No se escuchó el motor ni se validaron percepción de velocidad, parada o confort.

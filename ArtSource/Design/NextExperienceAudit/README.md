# Siguiente experiencia: ver detenerse la instalación

2026-10-01. Propuesta, no implementación. Revisión de cámaras de la campaña Study,
README/ROADMAP/HORROR_DESIGN y código de autoría/estado. Sin Unity, audición ni
partida humana; no se modificaron Assets. Lectores y casa quedan fuera del alcance.

## Recomendación principal

**Convertir el aislamiento en un encuentro cercano con una máquina que se
detiene**, conservando la única maniobra y la duración actual. El salto está en
ver y atravesar la consecuencia física, no en añadir otro puzzle industrial.

Evidencia concreta:

- [Mando después del enclave](../../../Docs/VisualEvidence/2026-10-01-Study/continuous/campaign-07-pressure-isolation-Camera.png): cámara
  (4.446,1.630,-36.438), yaw87.18/pitch1.66. Se leen mando/aguja, pero su fondo
  es prácticamente negro; el cuerpo de la bomba no explica visualmente el corte.
- [Paso junto a B](../../../Docs/VisualEvidence/2026-10-01-Study/continuous/campaign-07-pressure-drained-Camera.png): cámara
  (4.446,1.630,-43.940), yaw180. Predomina una pared plana y el punto brillante
  del pie de emergencia; B y la bomba quedan fuera de esa dirección de marcha.
  Son dos vistas del control automatizado, no prueba de que un humano jamás mire
  la instalación antes/después.
- `PressurePlantProduction.cs` sitúa dos bombas en (6.89,0,-36.5/-44), receptor
  en (0,0,-34.9) y gabinete central en (0,0,-39). El recinto heredado mide16×20m.
  Hay recursos industriales originales buenos; no hace falta otro paquete.
- `PressureIsolation.ApplyWorkCircuit` apaga las luces de trabajo al enclavar;
  `MotorLevel01` cae durante2.2s y `Pressure01` durante3s. El sonido ya responde
  a esos valores, pero el `MotorSkid` exportado es una sola malla estática: su
  ventilador posterior es un disco negro con nueve varillas delante, sin rotor.
  No se ha escuchado aquí el resultado del pitch/volumen o la purga localizada.

Intervención única propuesta, en una escena sucesora:

1. Reencuadrar el mando ligeramente en oblicuo respecto de la primera bomba,
   dejando carcasa y parte del guardamotor dentro del mismo campo de visión al
   mantener E. No cambiar tiempos, compromiso irreversible, aguja ni puerta.
2. Reutilizar el gabinete central para formar un paso de servicio junto a las
   bombas, aproximadamente2m libres en vez de una gran nave que se atraviesa
   mirando una pared. Mantener libre acceso al mando y a la salida; sin rodeo
   adicional ni un segundo gate. Es una recomposición de masas existentes,
   cuyo desplazamiento exacto requiere la cámara y colliders del integrador.
3. Integrar la luz de reserva existente en ese borde de servicio y dirigir una
   fracción sobre carcasa/rejilla. Las luces de trabajo siguen apagándose; queda
   una silueta física suficiente para caminar junto a la máquina detenida. No
   subir ambiente global, añadir neblina o encender la instalación de nuevo.
4. Si ese encuadre funciona, separar sólo el disco posterior/ventilador de la
   primera bomba: aspas visibles detrás de su guardia fija, velocidad proporcional
   al `MotorLevel01` real. La caída coincide con el motor existente y acaba en
   reposo; pausa congela, Continue aislado arranca detenido. Nada gira con RECALL
   ni se registra en el Echo. Es una presentación ambiental pequeña, sin motor
   de animación, física adicional ni audio nuevo.

**Costo medio y localizado:** una revisión del motor original, colocación/luz
del mismo tramo y un adaptador visual opcional que sólo lee el estado existente.
La primera decisión se puede tomar con geometría/luz, antes de rehacer la bomba.
**Beneficio esperado:** una causa física visible mientras se compromete la mano,
una transición de actividad a quietud y proximidad corporal al pasar. La quietud
posterior tiene un referente que el Player acaba de ver funcionar. No se afirma
que esto produzca miedo ni que el silencio actual sea insuficiente por oído.

Riesgos: un rotor minúsculo a ras de suelo podría ser sólo trabajo invisible;
una luz de reserva demasiado fuerte borraría el contraste; mover el gabinete
sin revisar tuberías y circulación dañaría la credibilidad que se busca. No
conviene sumar vapor decorativo si no corresponde a la purga real. Criterio de
selección visual: mando y bomba legibles en un mismo encuadre natural; el final
de movimiento se ve durante la caída real sin una pausa adicional; la marcha
posterior muestra volumen de máquina apagada y una salida clara.

## Alternativa menor: R1 como recuperación espacial real

[R1 actual](../../../Docs/VisualEvidence/2026-10-01-Study/continuous/campaign-04-R1-Camera.png)
presenta bien el escritorio, pero desde la estación la pantalla ocupa el centro
y el resto se pierde en sombra. Frente a la [superposición cálida de SE3](../../../Docs/VisualEvidence/2026-10-01-Study/continuous/campaign-SE3-codo-Camera.png),
la cámara no establece por sí sola el descanso amplio y estable descrito en
HORROR_DESIGN. Esta observación es del encuadre cercano del probe, no de toda
la experiencia al entrar.

Alternativa de costo bajo/medio: reorientar el conjunto escritorio/monitor hacia
un borde, abrir una vista diagonal de entrada a salida y distribuir la luz ya
existente por paredes y circulación, dejando el rincón doméstico distante.
Conservar el checkpoint y su interacción, sin evento, objeto narrativo ni
espera nueva. Su beneficio sería contraste y orientación en el arco; necesita
primero una cámara de entrada, porque el único primer plano actual no basta
para decidir una reconstrucción. **Priorizaría la instalación:** allí el estado
físico importante existe y la presentación todavía puede hacerlo tangible.

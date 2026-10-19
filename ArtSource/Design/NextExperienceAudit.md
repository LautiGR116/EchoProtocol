# Después del patio: atravesar SE2

2026-10-01. Propuesta única, sin implementar. Lectura de GAME_DESIGN,
HORROR_DESIGN, autores/componentes actuales y cámaras de Observation/continuous.
Sin Unity, escucha ni partida humana; no se modificaron Assets, Docs o el kit.

**Recomiendo convertir la vitrina de SE2 en una breve estancia transitable que
cambia de carácter mientras se pierde de vista durante la entrada.** Mantener
el Echo necesario para P2 y el regreso de SE3/R1; sustituir la aprobación por
mirada por un recorrido físico. El jugador se expone al lugar que acaba de ver
frío, en vez de esperar delante de otro cristal a que el sistema lo dé por leído.

## Qué falta, y qué ya funciona

El tramo posterior ganó acciones distintas: mirar/coordinar en P3, recorrer el
archivo y reconocer recepción, enclavar una máquina, elegir si atender y dónde
dejar energía, recibir la llegada real y devolver la llave. El patio añade una
vista exterior de una consecuencia propia. Seguiría conservando ese arco antes
de agregar otro puzzle, más objetos narrativos o un susto al devolver la llave.

La concentración de contemplación protegida está antes: ópticos tras vidrio,
SE2 en vitrina, SE3 como pasillo con el Echo, P3 tras vidrio y figura doméstica
tras vidrio. El final ya rompe esa separación abriendo la puerta real; conviene
reservarle ese encuentro frontal, no reproducirlo en todas las salas.

Evidencia focal:

- `campaign-SE2-cold-approach-Camera.png`: servicio frío al fondo de un tramo
  frontal; el marco completo ocupa el eje de marcha.
- `campaign-03-SE2-Camera.png`: salón creíble y figura al fondo, pero la posición
  real `(5.432,1.630,28.976)`, yaw90, permanece fuera de su vidrio. El cambio
  ya tiene buen contexto; no es prioritario rehacer más muebles de ese decorado.
- `VS1ParallelFigureEvent` exige cruzar con P2 sostenido por el Echo, luego una
  mirada válida para aparecer y **3.2s acumulados de mirada para Complete**.
  `VS1OmittedPassageEvent` depende de Complete y el regreso; `VS1RecoveryHubGate`
  depende a su vez de SE3. Por tanto esa observación sí gobierna la salida.
  No es sólo una medida de un probe ni una lectura opcional de la historia.
- `ThresholdCampaignProbe` prepara22s de espera frente a P2 y usa Until sobre
  SE2.Complete. Demuestra solvencia de esa toma, no que una primera persona
  deduzca cuánto esperar ni por qué no se abrió R1 tras una mirada breve.
- `campaign-SE3-entry/codo` conserva un buen pago espacial: la misma figura
  registrada ahora está en el corredor doméstico. Conviene mantenerlo.

No uso la oscuridad de `campaign-01-P1` como diagnóstico de enseñanza inicial:
esa cámara es al completar la cooperación y coincide con el arranque de SE1.
Tampoco convierto el cronómetro de la ruta automatizada en duración humana.

## Intervención concreta

1. Rehacer sólo el acceso SE2: paños laterales y un paso de servicio, con un
   pequeño retorno/mocheta que permita ver la sala fría al aproximarse y oculte
   su interior al cruzar. No es una puerta con otra condición ni un nuevo E.
2. Usar el volumen doméstico que ya existe: FirstAct construye un salón de
   **4.2×8m**, umbral interior a3.68m y figura a5.18m. Hacer transitable el
   frente, cerca de sillón/lámpara, y añadir una salida lateral hacia la vuelta
   de SE3. Mantener la figura en su estancia posterior, fuera del paso; no
   acercarla arbitrariamente a la cara, animarla ni añadir otra presencia.
3. Compartir suelo, techo y colisión estable entre las presentaciones. La sala
   fría debe tener esa misma envolvente jugable: su pared aparente a3.2m actual
   no puede permanecer como pared invisible dentro del salón. Sólo se alterna
   presentación bajo ocultación real, nunca geometría que encierre al Player.
4. Consumir el cambio una vez después del cruce real de P2 y el acceso. Esperar
   ocultación de **toda** la zona que cambia, incluyendo luces/sombras, antes
   del intercambio; si el jugador camina mirando atrás, no bloquearlo ni forzar
   cámara para conseguir el efecto. La circulación debe seguir funcionando si
   la anomalía no pudo presentarse. Regreso físico, no segundos de mirada,
   habilita SE3/R1. El acontecimiento y la mirada opcional conservan métricas
   distintas para no llamar «visto» a lo que sólo estuvo disponible.
5. Salir otra vez al corredor de SE3 desde una dirección que conserve la vista
   de su codo y del verdadero Echo. Presupuesto inicial: sustituir ida/vuelta
   actual por un pequeño lazo, **sin más de unos2s netos de marcha añadida**.
   Es presupuesto de autoría pendiente de plano, no una ruta ya dimensionada.

El sonido nuevo sería primero **el contacto actual sobre el suelo doméstico**:
reutilizar timber/vinyl del Player según la superficie que realmente quedó.
Ninguna pisada o respiración se atribuye al Echo. `PlayerFootsteps` permite zonas
de madera explícitas; si una zona depende del root doméstico, hay que consultar
enabled/active antes de usarla (hoy sólo lee bounds). No añadir stinger, voz,
pasos ajenos ni otra caída de volumen global que repita SE1. Si hace falta una
fuente local de servicio, se decide después de oír la ruta, no desde esta lectura.

## Ruta de implementación y costo

**Costo medio**, superior a un ajuste de superficie pero menor que otro capítulo:
se reutilizan salón, figura, lámparas, puerta y paleta existentes. Lo costoso es
la circulación/collision y el contrato de ocultación, no nuevos assets.

- Primero un plano/autor de escena sucesora de Courtyard que demuestre el lazo,
  ancho libre≥1.2m y la vista de retorno SE3. No construir mobiliario nuevo.
- Después un modo opt-in acotado de SE2/SE3: separar cambio presentado, cruce
  completado y observación opcional. Legacy conserva su fase/lectura actual.
  Mantener P2, lector, origen, grabación, reset y checkpoint de R1. La duración
  de una toma corta puede seguir fallando por el gate real; no extender Echo.
- Primera decisión por una ida y vuelta real con cámara libre: ver el frío,
  cruzar y descubrir proximidad doméstica, alcanzar SE3 y R1 sin conocer un
  cronómetro invisible. Luego ajustar únicamente los probes afectados y
  preservar Courtyard como build recuperable.

Beneficio esperado: una variación de actividad y vulnerabilidad en el centro
del primer acto; una anomalía ocupa el mismo espacio del Player. También evita
que mirar parezca un botón temporal. No prueba miedo ni reconocimiento espontáneo.

Riesgos decisivos: el cambio puede leerse como pop de assets si se ve un borde;
el nuevo recorrido puede tapar la composición de SE3; entrar demasiado cerca
de la figura puede revelar su rigidez; un lazo largo sería relleno. Si el plano
no resuelve estos límites, no aprobaría la reconstrucción sólo porque pase los
checks. **No recomendaría todavía animar el Echo ni sumar audio de cuerpo**:
sería otra producción transversal y debe declarar con rigor qué se registra.

## Límites del juicio del conjunto

Las cámaras permiten juzgar escala, barreras, composición y continuidad. El
código revela las condiciones y dueños. No he oído la campaña ni inferido
mezcla, sorpresa, cansancio, comprensión inicial o duración humana. El cierre
actual ya devuelve a una acción concreta (original/llave/luz); su grado de
emoción y la suficiencia de los documentos de Mara necesitan una partida,
no otra explicación añadida por anticipado.

# Auditoría de diseño — producción 2026-10-01

Propuesta del agente de diseño. **No implementada ni aprobada por una partida
humana.** Revisión de README, ROADMAP, GAME_DESIGN, HORROR_DESIGN, PLAYTEST_LOG,
CampaignDirector, GameSession, CampaignRouteProbe, autoría de campaña, moodboard
y capturas Finish. No se escuchó audio ni se jugó una partida en esta auditoría.
El integrador conserva autoridad sobre alcance y escenas.

## El problema principal es la repetición de la acción

| Evidencia actual | Consecuencia de diseño inferida |
| --- | --- |
| `CampaignDirector` 145: grabar 30 s; 217: presión sólo por Echo; 230: mirar cada válvula 6 s. | El trabajo de preparación es tiempo vacío, seguido por dos barras de progreso. No hay un sistema de tuberías que se pueda razonar. |
| 166–169: orden fijo cocina/dormitorio/recibidor; 230: mirar 4 s. | Los fusibles tienen forma física, pero funcionan como tres contraseñas en orden. El recorrido es impuesto por el texto. |
| 171 y `UpdateGallery`: tres pausas de 8 s y tres parejas de 1,5 s. | Repite tres veces la misma solución ya aprendida en P1–P3, sin una nueva decisión. |
| 199–202: otro registro quieto, otra mirada de 2 s. | El archivo vuelve a representar D1 después de presión/casa/galería, debilitando su posición como conclusión. |
| `CampaignRouteProbe` 123/135/204/254/271 usa pausas de 22/25/34/27/14 s para resolver. | No prueba duración humana, pero demuestra cuánto del plan depende de esperar más que del espacio o la comprensión. |
| `37-work-feedback-staged.png`: caja casi negra, barra y consigna de mirar. `24-gallery-2.png`: figura cian centrada en una ventana negra con III. | La cámara queda subordinada a una tarea abstracta. Cambiar texturas no resuelve esa composición ni añade significado a la mirada. |

El teléfono apaga las luces y desencadena tareas eléctricas, pero el jugador no
decide qué sacrifica para tener luz. La carta pide dejar encendido el recibidor;
el comportamiento sólo permite completar una secuencia. La figura del pasillo
desaparece automáticamente tras 1,6 s de mirada: el jugador no puede acercarse,
verificar o modificar esa situación. Es una oportunidad emocional perdida.

El PLAYTEST_LOG ya contiene un aviso relevante: la silla se detectó sin producir
malestar y la primera figura lateral ni siquiera se percibió. Un estado de
`presenceSeen` o una captura visible no valida terror. El cambio debe alterar
la decisión y el recorrido, además de estar técnicamente presente.

## Dirección: entrar, ocupar el lugar del otro, salir dejando algo distinto

Concentrar la campaña en tres movimientos, sin una duración obligatoria:

1. **Aprender a necesitarse.** P1 y P2 enseñan el Echo fiable; P3 sólo permanece
   si su separación aporta más que repetir B. La recepción y la primera
   invasión doméstica establecen lugares reconocibles. La vuelta a recepción
   abre el descenso; no hace falta resolver D1 aquí.
2. **La casa que dejaste a oscuras.** Una vivienda explorable que requiere
   recuperar la llave y decidir dónde conservar una luz. La carta de Mara se
   convierte en algo que el jugador puede hacer. El subsuelo puede ser un
   tránsito corto con causa física para el apagado, en vez de dos esperas.
3. **La puerta 6B.** Tras conocer la casa, el jugador ocupa el lado interior de
   una puerta y ve llegar una grabación verdadera de su propia espera exterior.
   Debe abrir y cruzar junto a ella. Conserva la toma y devuelve la llave en un
   cierre físico. Reemplaza la triple galería y el segundo archivo-test.

No añadir un enemigo, una voz de Mara generada apresuradamente, otro símbolo
coleccionable ni un sistema de combate. La escala nueva está en la construcción
del lugar y en el significado de acciones sencillas.

## Cambio grande 1: la reserva de luz

Sustituir las tres comprobaciones temporizadas por una reserva eléctrica
trasladable, con un único fusible grande y tres alojamientos claramente
rotulados. El teléfono puede conservar su apagado inicial. La reserva empieza
en el recibidor: el jugador ya conoce qué hará al retirarla. E extrae/coloca la
pieza con animación física breve; el alojamiento activo, la lámpara y el zumbido
concuerdan. Ningún orden obligatorio ni temporizador de mirar.

La salida doméstica es una puerta mecánica con una llave, no otro circuito de
protocolo. La llave está en un lugar doméstico significativo y visible al
explorar —por ejemplo, junto a una maleta empezada y un recibo de portería—,
jamás invisible hasta encender una bandera lógica. Iluminar el dormitorio
ayuda a localizarla; una persona que se orienta en penumbra puede encontrarla
manteniendo el recibidor encendido. Eso es una elección válida, no un bypass.

El jugador puede tomar la luz segura para investigar mejor, regresar a oscuras,
o dejarla detrás y avanzar con menos información. Un suelo legible y luz
residual estable evitan desorientación ciega. Las fuentes opcionales iluminan
composiciones distintas; nunca se exige oír para encontrar la llave.

La presencia se sitúa en el camino de regreso después de recuperar la llave,
con contraluz propio moderado. No desaparece por mirarla 1,6 s: permanece al
menos mientras se la observa. Se retira sólo después de una oclusión real
autorada (marco/puerta), dejando una consecuencia concreta, como una puerta
interior que antes estaba abierta y ahora está entornada. Si el jugador se
acerca sin apartar la vista, no debe atravesarla ni provocar un despawn visible;
la geometría puede mantenerla detrás de una puerta de vidrio cerrada. Evitar
convertirla en perseguidor o castigo por explorar.

Antes de salir se puede devolver el fusible al recibidor. No evaluar esa acción
con puntos morales ni un mensaje de “final bueno”. La luz visible a la espalda,
el estado final de la casa y una línea sobria en el cierre bastan. La carta
obtiene una respuesta material.

**Contrato técnico:** `HouseLightReserve` posee estado único
`Hall/Kitchen/Bedroom/Carried`; E es idempotente ante repetición; una luz activa
como máximo; inventario discreto sólo mientras se lleva la pieza; llave booleana
y modelo físico; puerta con `HouseKey && player interaction`. Guardar el origen
del capítulo reinicia la casa de manera explícita y recuperable. El Echo sigue
suspendido como ahora; la secuencia de luces no se atribuye a una grabación.

**Riesgo:** convertirlo en un fetch quest de fusibles. Evitarlo dejando la llave
accesible sin una combinación eléctrica obligatoria, eliminando lecturas
obligatorias y usando luz/sonido/recorrido como consecuencia perceptiva. Si no
se consigue ese contraste, conservar sólo una lámpara portátil y la llave:
menos objetos, la misma decisión espacial.

## Cambio grande 2: abrir al propio Echo

Construir un único vestíbulo interior y un corredor exterior que rodea 6B.
Establecer de forma visible el origen de RECALL dentro del vestíbulo. El
jugador sale por una ruta lateral abierta, llega al lado exterior de la puerta,
y hace una pausa real frente al ojo de observación. Puede inspeccionar el
retorno y estimar cuánto necesitará; no recetar “14 segundos”. La lente confirma
la orientación conforme a la regla ya aprendida.

R devuelve al origen interior, siempre el mismo. El Echo repite la ruta lateral
y se aproxima después por el corredor exterior. La composición desde el
interior permite percibir primero el desplazamiento por un vidrio esmerilado,
después la silueta en el ojo y finalmente el cuerpo entero al abrir. El jugador
elige el momento de abrir; mirar no mueve la cámara ni opera la puerta. La
proximidad proviene de su recorrido real, no de una persecución añadida.

La puerta responde a E en todo momento. La verificación requiere una pausa
exterior auténtica y que el jugador la haya visto desde dentro; esa condición
habilita conservar el registro al otro lado. Cruzar temprano o una toma corta
no encierra al jugador: puede regresar y usar R. Al abrir, el Echo no toca la
manija ni cambia su ruta. La toma sigue siendo exclusivamente pose/yaw.

La frase de negación de D1, si se conserva, pertenece a esta escena después de
la experiencia doméstica. No hace falta repetirla antes. Preservar es el último
acto administrativo; devolver la llave en una bandeja y pasar una puerta real
es el último acto humano. No afirmar que el registro demuestra intención,
culpa, las palabras de Mara o qué había detrás de la puerta años antes.

**Contrato técnico:** reutilizar calificación de pausa/snapshot de D1 en un
componente autónomo con referencias a `LoopController`, volumen exterior,
posición interior de observación, puerta y terminal. La toma validada es la
referencia inmutable actual. Abrir es una acción del jugador presente; la
puerta no forma parte de los datos reproducidos. El final se puede resolver con
un recorrido suficientemente largo y repetir sin perder la casa.

**Riesgo:** otra espera disfrazada. La diferencia debe ser verificable en cámara:
la llegada ocupa una puerta doméstica cercana, abrir expone al jugador y salir
obliga a pasar junto a su figura. Si no hay esa relación corporal, recortar la
escena y evitar un segundo panel de cristal idéntico a D1.

## Primer bloque de producción recomendado

Implementar primero la casa: cambia una sección completa sin tocar las reglas
de Echo y permite probar la nueva dirección desde un checkpoint existente.
Resolver el estado de energía, llave, puerta, retirada de barras y tres
composiciones antes de añadir más decorado. Verificar retirada/colocación en
cualquier orden, posibilidad de completar con luz en el hall, pausa, continuar,
ninguna pieza duplicada y recorrido físico libre en todos los estados.

Después integrar la puerta final y retirar la galería de tres pares. No conviene
seguir mejorando visualmente tres repeticiones que se propone eliminar. La
autoría debe generar otra escena recuperable y conservar Finish tal cual.

Para acercarse al moodboard: habitaciones con función, espesores y umbrales;
fuentes de luz legibles; paredes iluminadas que sostengan penumbra; imperfección
local en vez de ruido uniforme; tres escalas de detalle (arquitectura, muebles,
uso). La imagen de referencia muestra pasillos y puertas reconocibles antes que
negrura. No resolver baja calidad del objeto escondiéndolo a oscuras.

La evidencia automática puede probar estados, física, secuencia y lo que aparece
en cámara. Anticipación, ganas de mirar atrás, credibilidad íntima y miedo
continúan pendientes de una partida humana. Eso no bloquea estos cambios.

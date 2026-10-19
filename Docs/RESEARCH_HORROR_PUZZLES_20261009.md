# Benchmark de terror y puzzles para Echo Protocol

**9 de octubre de 2026. Estado: investigación y propuestas de diseño.**

Echo Protocol necesita elevar la credibilidad de sus personajes y convertir sus acertijos en situaciones de terror. La prioridad propuesta es reconstruir un tramo de referencia que combine una deducción interesante, dependencia del Echo y una amenaza perceptible. Ese tramo debe establecer la calidad que luego se extienda a la campaña.

El usuario rechaza como insuficiente la presentación actual del Echo y de las demás personas, considera mejorado pero todavía insuficiente el sonido, y pide más creatividad en los puzzles, intuitividad, persecución e intensidad. Esto sustituye la recomendación anterior de concentrar el siguiente objetivo exclusivamente en orientación. Los conceptos de este documento son propuestas, no funciones existentes ni experiencias humanamente aprobadas.

**Alcance y método**

Comparación documental de 18 juegos, seleccionados por afinidad con terror psicológico, vulnerabilidad, deducción, repetición temporal y cooperación con copias. No constituye un censo de todos los juegos mejor puntuados ni una revisión de partidas completas. Las fichas oficiales respaldan las mecánicas descritas; entrevistas y documentación de sus creadores sustentan las lecciones más específicas. Las aplicaciones a Echo y sus riesgos son juicio de diseño.

Las cifras son porcentajes positivos calculados desde la API pública de reseñas de Steam, consultada el 9 de octubre de 2026: idioma inglés, compras en Steam, todos los períodos; no son notas de crítica ni porcentajes de cuánto miedo produce cada juego. El tamaño de muestra importa: los 644 votos de Clockwinder no equivalen a los 174.131 de Portal 2. Los valores pueden diferir del redondeo o caché de la tienda.

Consulta reproducible: `https://store.steampowered.com/appreviews/APPID?json=1&language=english&purchase_type=steam&filter=all&num_per_page=0`. La información de producto se contrastó con las fichas y `api/appdetails?appids=APPID&l=english`.

**Diagnóstico del proyecto**

Las capturas conservadas del adulto de Contraprueba y su presentación en Lighting muestran piel, pelo y ropa con poco contraste material; el cabello parece una superficie continua y la figura pierde credibilidad de cerca. Esto coincide con la devolución humana. Una imagen estática no permite juzgar la marcha.

La arquitectura actual reproduce posición, giro y canales explícitos de presentación corporal en un rig de 16 articulaciones. No reproduce libremente interacciones ni audio. Resonance añadió trece masters y ocho emisores, con dos actuaciones humanas puntuales. Esa integración está verificada, pero la densidad de contenido y los tests no acreditan una buena interpretación ni miedo.

En 6B existe además evidencia humana concreta: la acción había sido aceptada, pero el jugador seguía esperando una consecuencia. La mejora de creatividad debe incluir confirmaciones y orientación; de lo contrario, un acertijo más elaborado amplificará la confusión.

Fuentes del estado: `README.md`, `Docs/ROADMAP.md`, `Docs/ARCHITECTURE.md`, `Docs/AUDIO_DESIGN.md`, `Docs/PLAYTEST_LOG.md`; capturas `2026-10-08-Contraprueba/final-echo/02-near-face-Game.png` y `2026-10-08-Lighting/final3-native/campaign-Contraprueba-close-return-Game.png`.

**Comparación de terror**

La última columna propone una transferencia de diseño; no afirma que el juego de referencia implemente esa variante.

| Referencia y fuente | Positivas / muestra | Aporte documentado | Aplicación a Echo y riesgo |
| --- | --- | --- | --- |
| [SOMA](https://store.steampowered.com/app/282140/) | 95,8% / 24.675 | Identidad, conciencia y supervivencia sin capacidad de combatir a sus enemigos. | Que una acción útil adquiera un significado perturbador al ver su consecuencia. Evitar depender de cartas para que el encuentro importe. |
| [Amnesia The Bunker](https://store.steampowered.com/app/1944430/) | 94,4% / 6.428 | Amenaza que responde a acciones y sonido, recursos limitados y varias soluciones. | Un mecanismo necesario facilita la tarea y revela tu actividad a una presencia. Evitar copiar todo su inventario y supervivencia. |
| [Alien Isolation](https://store.steampowered.com/app/214490/) | 92,3% / 27.012 | Supervivencia frente a una amenaza dominante; su director destaca la importancia conjunta de imagen, sonido y sentidos de la criatura. | Escuchar para decidir si continuar, esperar o cambiar de ruta. Una presencia consistente vale más que apariciones numerosas sin consecuencias. |
| [Visage](https://store.steampowered.com/app/594330/) | 85,2% / 4.061 | Casa cambiante, exploración lenta, familiaridad doméstica y entidades hostiles. | Construir conocimiento de un lugar y después comprometer su seguridad. El objetivo de Echo debe seguir siendo comprensible incluso durante el desconcierto. |
| [MADiSON](https://store.steampowered.com/app/1670870/) | 85,9% / 1.205 | Cámara instantánea como herramienta para conectar mundos y revelar información. | RECALL puede ser simultáneamente herramienta de investigación y compromiso temido. No trasladar su sistema de eventos aleatorios. |
| [The Mortuary Assistant](https://store.steampowered.com/app/1295920/) | 92,8% / 4.829 | Trabajo manual e identificación de una amenaza sobrenatural mientras se realizan tareas. | Una operación exige atención y deja parte del espacio fuera de vista. El trabajo debe contener decisiones; repetir tareas por duración sería relleno. |
| [Resident Evil 7](https://store.steampowered.com/app/418370/) | 94,8% / 33.626 | Terror en primera persona, proximidad corporal, exploración y entorno doméstico. | Evaluar personajes y encuentros desde la distancia real de juego. La calidad de una cara cercana es inseparable de su iluminación y movimiento. |
| [Silent Hill 2 remake](https://store.steampowered.com/app/2124490/) | 94,8% / 23.898 | Memoria, identidad, criaturas y una presentación audiovisual opresiva. | Hacer que formas y motivos sonoros pertenezcan a la historia de Echo. Evitar una colección de monstruos visualmente inconexos. |
| [SIGNALIS](https://store.steampowered.com/app/1262350/) | 96,8% / 21.597 | Identidad y memoria, tecnología institucional, recursos limitados y acertijos. | Coherencia entre aparato, documento, espacio y amenaza. Su valoración respalda incluir una dirección visual consistente, no obliga al fotorealismo. |

Las notas de Visage y MADiSON justifican incluirlos por afinidad y especialización, sin presentarlos como los máximos absolutos de la clasificación.

**Comparación de puzzles**

| Referencia y fuente | Positivas / muestra | Aporte documentado | Aplicación a Echo y riesgo |
| --- | --- | --- | --- |
| [Portal 2](https://store.steampowered.com/app/620/) | 98,9% / 174.131 | Recombinar herramientas espaciales y pensar cooperativamente. | Una misma regla debe sostener soluciones diferentes. Cambiar A por B sin cambiar el razonamiento no aporta una idea nueva. |
| [The Talos Principle](https://store.steampowered.com/app/257510/) | 95,4% / 17.145 | Sistemas conectados, haces, dispositivos y reproducción temporal. | Diseñar dependencias entre lo que hace tu versión pasada y lo que hacés ahora. La documentación actual de Reawakened distingue además puntos de pausa; no atribuirlos al original. |
| [Outer Wilds](https://store.steampowered.com/app/753640/) | 95,0% / 53.225 | Investigación de un mundo que cambia durante un bucle temporal. | Que el progreso principal sea descubrir una relación o una oportunidad. El jugador debe poder formular una hipótesis concreta sobre dónde investigar. |
| [Return of the Obra Dinn](https://store.steampowered.com/app/653530/) | 96,5% / 18.751 | Exploración y deducción lógica de lo ocurrido. | Usar una grabación como evidencia que se interpreta desde distintos lugares. Evitar que todo se reduzca a presenciar una animación y leer la explicación. |
| [The Witness](https://store.steampowered.com/app/210970/) | 82,5% / 9.073 | Explorar muchas variaciones de un núcleo de reglas. | Cada puzzle debe exigir un descubrimiento identificable. Eliminar pasos que solo alarguen la ejecución de una solución ya entendida. |
| [Braid original](https://store.steampowered.com/app/26800/) | 91,0% / 3.484 | Reglas temporales diferentes que alteran la manera de pensar; reintento tolerante. | Dificultad por causalidad y planificación, con poco coste al probar una idea. Un temporizador más ajustado no sustituye una idea. |
| [The Swapper](https://store.steampowered.com/app/231160/) | 93,7% / 3.439 | Copias corporales, transferencia de conciencia y soluciones compactas. | Que usar tu copia tenga peso físico y emocional. No necesita una gran cantidad de salas ni muchos controles. |
| [The Sexy Brutale](https://store.steampowered.com/app/552590/) | 89,7% / 2.099 | Un día repetido con acontecimientos simultáneos que se investigan y alteran. | Convertir sonidos lejanos en información causal sobre otra habitación. No exigir memorizar una agenda extensa para un juego breve. |
| [The Last Clockwinder](https://store.steampowered.com/app/1755100/) | 96,1% / 644 | Grabaciones de acciones que se convierten en ayudantes para tareas coordinadas. | El acto de construir una buena grabación debe ser parte del puzzle. Adaptar el principio al teclado y mouse; no importar la carga de interacción de VR. |

**Hallazgos de diseño que más importan**

La documentación oficial de [Croteam sobre Time Recorder](https://taloseditor.croteam.com/device_reference/#time-recorder) permite identificar la diferencia más útil con Echo: las copias también reproducen acciones sobre componentes. Mi hipótesis es que ampliar el vocabulario con una o dos interacciones grabables bien elegidas daría más profundidad que añadir lectores de presencia. Supone una ampliación real del sistema y debe demostrarse primero en un prototipo.

El equipo de Outer Wilds describe cómo corrigió recorridos que parecían elecciones arbitrarias mostrando información sobre el destino de cada camino. Su lección para Echo es compatible con el misterio: el jugador puede saber qué intenta averiguar sin conocer la respuesta. Esto refuerza la necesidad de arreglar regresos y confirmaciones. [Mobius Digital sobre orientación](https://www.mobiusdigitalgames.com/news/the-intentionality-of-wandering).

Frictional plantea que resolver un problema debe constituir un acontecimiento narrativo. En Echo, abrir una sala puede obligar a devolver energía a una zona que el jugador ya no quería volver a ocupar. La solución tiene una consecuencia vivida, además de permitir pasar. [Pilares de SOMA](https://frictionalgames.com/2013-12-the-five-foundational-design-pillars-of-soma/).

La descripción de la charla del director de Alien destaca una criatura suficiente para amenazar a un jugador vulnerable y la integración audiovisual de ese encuentro. Eso favorece concentrar recursos en una presencia creíble y situaciones variadas antes de multiplicar enemigos. Se consultó el resumen público, no la grabación completa. [Building Fear in Alien Isolation](https://www.gdcvault.com/play/1021852/Building-Fear-in-Aliens).

La entrevista de Jonathan Blow y Rand Miller muestra el trabajo de explorar variaciones de una regla y la tensión entre puzzle, historia y entorno. Para Echo, cada propuesta debe declarar qué razonamiento nuevo pide y qué aporta al terror; una lista de pasos no alcanza. [Conversación entre los creadores](https://blog.playstation.com/archive/2017/08/24/the-witness-jonathan-blow-talks-game-design-with-myst-co-creator-rand-miller).

**Personajes y herramientas**

Propuesta: comparar dos o tres candidatos del mismo personaje adulto, con idéntica cámara, luz, escala y recorrido dentro de Unity. Evaluar rostro, ojos, manos, pelo, ropa y movimiento. Un giro de exhibición en una web no permite decidir cómo funciona en el juego.

| Vía | Capacidad comprobada o base disponible | Condición práctica | Recomendación |
| --- | --- | --- | --- |
| Tripo | Ofrece generación y rigging; exporta esqueletos en FBX o GLB. | Su ayuda reserva los derechos comerciales al plan pago; el gratuito figura como no comercial. | Candidato para comparar si ya existe acceso adecuado. No comprar una suscripción basándose en ejemplos promocionales. |
| Meshy | Ofrece generación, rigging y exportación para motores. | El plan gratuito permite uso con atribución CC BY 4.0, pero sus descargas gratuitas se limitan actualmente a Meshy 6 Lite; API y descargas de modelos superiores requieren pago. | Primera prueba de integración si se necesita una vía gratuita distribuible. No asumir que evalúa la máxima calidad del servicio. |
| Base humana con anatomía y rig consolidados, trabajada en Blender | El proyecto ya usa una base humana derivada; se puede comparar otra base autorizada o reconstruir materiales y acabado. | Requiere trabajo artístico deliberado en rostro, pelo, ropa y deformación. | Mantener esta vía como control de calidad y alternativa si la generación falla en anatomía o animación. |

Fuentes: [rigging de Tripo](https://www.tripo3d.ai/features/ai-auto-rigging), [uso comercial de Tripo](https://www.tripo3d.ai/help/privacy-policy/how-to-use-tripo-models-commercially), [términos de Tripo](https://www.tripo3d.ai/terms), [condiciones gratuitas de Meshy](https://help.meshy.ai/en/articles/15696428-what-is-included-on-the-free-plan), [funciones de Meshy](https://www.meshy.ai/features).

Character Creator también dispone de integración con Unity y materiales humanos, pero su flujo oficial consultado exige Windows; no lo elegiría como primera vía en esta Mac. [Compatibilidad de Reallusion](https://www.reallusion.com/auto-setup/unity/download.html).

El carácter inquietante debería surgir de una persona reconocible, su proximidad, silueta y puesta en escena. Una deformación grotesca desde la primera aparición puede debilitar el comienzo de cooperación. La presentación debe resistir luz suficiente: esconder defectos en oscuridad extrema repetiría el problema de legibilidad ya reportado.

El personaje nuevo necesita una marcha con peso, contacto de pies y transiciones creíbles. Si se añade una animación o mirada que pertenece al Echo, su estado debe grabarse explícitamente y reproducirse de forma determinista. Un esqueleto importado con animaciones autónomas no cumple automáticamente ese contrato. Las personas del presente pueden tener comportamiento propio y una actuación distinta.

**Dirección sonora y de encuentros**

Resonance ofrece una base utilizable, pero el siguiente objetivo puede cambiar su paleta, su mezcla y sus eventos cuando eso mejore la escena. Las voces necesitan una interpretación convincente: más distorsión sobre un llanto inadecuado no lo vuelve convincente.

La propuesta es construir encuentros con cinco partes: señal inicial, anticipación, acción que compromete al jugador, impacto y consecuencia. Variar esas partes permite incluir gritos y jumpscares sin que cada interacción anuncie el mismo golpe.

Ejemplos propuestos:

- Un grito nace lejos, detrás de una puerta localizable. Al investigar, una respuesta física del lugar demuestra que la amenaza puede aproximarse. El volumen acompaña distancia y causa.
- Una máquina tapa parte del ambiente. Al detenerla para resolver una tarea, el jugador descubre una presencia que estaba demasiado cerca. La resta prepara el contraste; no hace falta elevar peligrosamente el pico.
- Una persecución breve comienza después de una señal que el jugador pudo reconocer. La salida requiere usar un trayecto o una grabación preparada, de modo que recordar bajo presión tenga utilidad.
- El golpe fuerte deja una consecuencia: una ruta cambia, una puerta conserva una marca o una fuente continúa desplazándose. El siguiente minuto hereda tensión en vez de volver inmediatamente al estado anterior.

El sonido debe ayudar a decidir: de qué lado llega algo, si hay una barrera, si acercarse conviene y qué ruido produjo el propio jugador. Para los gritos, distinguir dolor distante, alarma cercana y agresión; no reutilizar una misma actuación con distinto volumen como tres personajes.

Frictional describe una biblioteca de grabaciones como material de partida para diseñar y combinar sonidos según su función. Esto favorece buscar actuaciones y materiales apropiados, editar variantes y escucharlas dentro del encuentro. [Samuel Justice sobre el trabajo sonoro de SOMA](https://frictionalgames.com/2014-08-people-of-frictional-samuel-justice/).

Conservar controles de volumen, mezcla mono y dinámica reducida. El impacto se trabaja con contraste, espectro, distancia, interrupción y ataque. La voz del Echo no existe como canal grabado actual: cualquier nueva voz ambiental debe tener su propia causa, y una futura voz reproducida exigiría captura explícita.

Una amenaza que persigue pertenece al presente. El Echo conserva su toma exacta; no debe volverse un enemigo autónomo que improvise movimientos dentro de esa reproducción. El pedido actual autoriza evaluar persecución aunque documentos históricos la excluyeran; no obliga a convertir toda la campaña en una sucesión de carreras.

**Seis conceptos originales para prototipar**

Estas ideas combinan principios de las referencias. Son alternativas para seleccionar y transformar, no un encargo de implementar las seis.

| Concepto | Descubrimiento y acción | Vulnerabilidad | Ampliación necesaria y riesgo |
| --- | --- | --- | --- |
| La máquina que te cubre | Descubrir que el ruido de trabajo tapa la actividad. Preparar una grabación que accione una máquina por intervalos; cruzar y operar durante esos intervalos. | El silencio te deja expuesto y obliga a decidir cuándo seguir. | Registrar una interacción concreta, modelar ruido de la máquina presente y una amenaza que lo perciba. No atribuir audio inventado al Echo. Coste alto. |
| El contraplano | Una grabación revela, desde un segundo punto de observación, qué ocultaba el cuerpo o el marco de una puerta. Deducir cuál era el lugar relevante antes de intervenir. | Acercarte al punto que tu propio cuerpo parecía vigilar. | Composición, oclusión y evidencia física clara; puede reutilizar canales existentes. Riesgo de resolución por tanteo o de que la pista no sea visible. Coste medio. |
| La dependencia invertida | La salida necesita energía, pero alimentar su circuito vuelve inaccesible un mando. Organizar la intervención pasada y la presente para transferir el control sin perder la salida. | Separarte del Echo y regresar a una zona cuya iluminación y actividad cambiaron por tu solución. | Recombinar estados e interacciones con márgenes generosos. Debe demostrar una deducción diferente del A–B–A actual. Coste medio o alto. |
| La ruta que preparaste | Primero conocer dos trayectos y registrar una secuencia de accesos. Después, una persecución te obliga a aprovechar esa preparación y elegir el siguiente refugio. | Ejecutar un plan propio cuando escuchar y mirar atrás consume atención. | Amenaza presente, puertas y registro de acciones. Aprendizaje antes de presión, checkpoint cercano y reintento que no repita exposición. Coste alto. |
| La huella que falta | Varias huellas presentes permiten contrastar dónde y cuándo pasó tu grabación. Una diferencia espacial identifica un acceso o una causa, sin alterar la toma. | Volver a examinar un lugar conocido desde el lado más expuesto. | Marcas actuales causales, no una grabación falsa. Debe explicar físicamente qué dejó cada huella y evitar un simple juego de buscar diferencias. Coste medio. |
| Conservar o repetir | Una toma válida resuelve un requisito, pero su información también permite descubrir un segundo modo de completar la escena. Entender esa relación permite decidir cuándo conservarla y cuándo rehacerla. | Elegir entrar otra vez en un lugar que ya produjo miedo. | Estado de conservación legible y consecuencias previsibles. No convertir un error de UI en una decisión irreversible ni inflar el final con menús. Coste medio. |

La primera selección sería un prototipo de **contraplano** y otro de **máquina que te cubre**. Contrastan deducción espacial con cooperación temporal y presión sonora. La dependencia invertida solo avanzaría si supera claramente la recombinación ya construida en P3.

El salto de sistema más valioso a estudiar es una lista pequeña de interacciones reproducibles: por ejemplo, operar un mando o sostener una manivela. Registrar objetivo, comienzo, final y condiciones; definir qué sucede si el objeto ya no está disponible. Evitar capturar indiscriminadamente física, inventario y todo el mundo antes de demostrar una situación satisfactoria.

**Intuitividad y dificultad**

Cada prototipo debe permitir responder tres preguntas desde el juego: qué puedo probar, qué cambió cuando lo probé y por qué mi intento no alcanzó. La solución puede requerir deducción; el estado de un mecanismo debe ser perceptible.

Las pistas progresivas sugeridas tienen tres niveles: dirigir la atención, recordar la relación relevante y, a pedido, explicar la acción. Un temporizador que ignora progreso puede interrumpir a quien está razonando; conviene considerar intentos y cambios de estado, pausar durante lectura y evitar pistas repetidas.

Antes de añadir presión, enseñar la regla en condiciones legibles. Durante el encuentro conservar margen para ejecutar una solución correcta. Tras el impacto permitir recuperar orientación. La interacción debe elegir el mismo objeto que anuncia la interfaz, conservar tolerancias razonables y explicar si requiere pulsar o mantener.

**Orden de producción recomendado**

1. **Diseño y prototipos.** Seleccionar las dos propuestas más fuertes, permitir reemplazar P1/P2/P3 cuando convenga, construirlas con presentación suficiente y descartar la que no aporte una deducción nueva. No cerrar con un informe como único resultado del goal de ejecución.
2. **Personaje y escena de referencia.** Comparar candidatos dentro de Unity y llevar el mejor al tramo seleccionado. Incluir a las demás personas en el estándar visual, priorizando las que aparecen cerca.
3. **Encuentro completo.** Integrar sonido, gritos, anticipación, posible persecución y consecuencias en ese tramo. Iterar sobre una experiencia continua con inicio, comprensión, exposición, impacto y recuperación.
4. **Campaña.** Extender el estándar, reemplazar contenido redundante, revisar transiciones y confirmar la ruta final de la misma build. Cerrar también el hueco de verificación posterior a los ajustes finales de Folio.

La evaluación técnica cubre grabaciones fieles, progreso, reintentos, pausa, guardado, interacción y rendimiento. La evaluación humana debe distinguir credibilidad, sorpresa, miedo sostenido, frustración y satisfacción al resolver. Un recorrido automático que conoce la solución no demuestra que el puzzle se entienda.

La señal de avance buscada es concreta: un jugador puede explicar qué descubrió, siente que su preparación importó y cambia voluntariamente su forma de moverse o escuchar ante la amenaza. Si el personaje vuelve a resultar cómico, o el encuentro solo sobresalta por volumen, corresponde iterar. No se puede garantizar miedo universal ni traducirlo a una cantidad de tests, modelos o archivos de audio.

**Próximo objetivo sugerido**

Elevar sustancialmente la calidad de personajes, terror y puzzles de Echo Protocol. Usar este benchmark para seleccionar y prototipar ideas, cuestionar lo que ya existe y combinar cooperación temporal, un personaje creíble y una amenaza audiovisual con consecuencias. El primer hito será un tramo integrado que establezca el estándar; después, extender ese estándar al recorrido completo y reemplazar contenido redundante. Entregar una build macOS y explicar qué se reemplazó, qué mejora se pudo comprobar y qué queda EN PRUEBA humana. Preservar la fidelidad de las grabaciones y una versión recuperable; no tratar el contenido existente como intocable.

Chat: continuar la investigación aquí y la implementación en «Ejecutar el objetivo nocturno». Model: GPT-6.1 Sol. Effort: high para integración; max es una opción para el objetivo de diseño amplio, sin promesa de mayor miedo por sí mismo.

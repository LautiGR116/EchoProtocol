# Echo Protocol — traspaso de contexto, 2026-10-11

Documento de continuidad para renovar los chats de dirección y ejecución.
Es una fotografía del estado y una propuesta de próxima iteración; no acredita
una nueva implementación ni reemplaza la evidencia del proyecto.

## Forma de trabajar

Proyecto local: `/Users/lautigr116/EchoProtocol`. Unity 6000.3 LTS, URP, C# e
Input System; entrega macOS. El usuario busca un juego ambicioso, con decisiones
creativas autónomas, terror perceptivo, buena jugabilidad e inmersión. Prefiere
objetivos claros con libertad de solución, verificación proporcional y resultados
jugables. Gastar cuotas, alargar artificialmente la ejecución o acumular tests no
son criterios de calidad. La antigua entrega universitaria del viernes ya no es
un plazo vigente para este encargo.

- **Dirección:** auditar resultados y feedback, cuestionar decisiones, investigar
  sólo lo que haga falta y preparar el siguiente goal. No implementar el juego
  ni enviar encargos a otros chats sin pedido del usuario.
- **Ejecución:** llevar el goal enviado por el usuario hasta una build revisable.
  Un solo chat modifica el proyecto y opera Unity a la vez.

Chats anteriores, como archivo consultable si falta un dato concreto:
«Auditar rumbo de EchoProtocol» (`01a09602-5b98-7351-baf0-c47a24ae18ec`) y
«Ejecutar el objetivo nocturno» (`01a0f419-c453-7b43-a6e9-5891c1b02983`).
No hace falta releerlos completos. Seguir AGENTS.md, incluyendo recomendar
Chat / Model / Effort en cada respuesta. Recomendación inicial: GPT-6.1 Sol,
high para dirección y max para ejecución; revisable según la tarea.

## Base actual y fuentes

Leer primero README.md, el hito actual de Docs/ROADMAP.md, la entrada más reciente
de Docs/PLAYTEST_LOG.md y Docs/EMBODIMENT_CANDIDATE.md. Consultar GAME_DESIGN,
HORROR_DESIGN, AUDIO_DESIGN y ARCHITECTURE sólo en las secciones relevantes.

La candidata seleccionada es **Embodiment, 2026-10-09**:
`Builds/EchoProtocol-Embodiment-20261009/EchoProtocol.app`.
«Iniciar sesión» abre la campaña; «Jugar · La máquina que te cubre» abre la bomba.
Coverage y las builds anteriores están preservadas.

- Coverage introdujo un encuentro conectado: el ruido de la bomba cubre el
  trabajo manual, pero su presión cierra la reja. Una toma registra marcha,
  pausa y marcha; el jugador se separa del Echo y aprovecha esa preparación.
  Después aparece una amenaza presente, con persecución, quiebre de visión y
  reintento. Ver Docs/COVERAGE_CANDIDATE.md.
- Embodiment reemplazó los personajes por adultos escaneados: Eric para Echo/SE2
  y Nathan para el perseguidor. Mejoró superficies, articulación y contactos.
  **Conservó los clips, niveles y mezcla anteriores**, así como las reglas de
  Coverage. Carrera, contactos y expresión todavía tienen límites perceptivos.
- El informe registra rutas completas PASS545 en Editor y Player de verificación,
  fidelidad de tomas, pausa y build universal. Eso acredita funcionamiento dentro
  del alcance descrito; no acredita miedo, naturalidad ni claridad humana.
  El video comparativo es breve y silencioso, no evidencia sonora.
- Las fuentes Renderpeople y sus derivados tienen restricciones de redistribución
  y están excluidos del repositorio público. Recuperación privada documentada en
  EMBODIMENT_CANDIDATE. Hay muchos cambios locales previos: preservarlos.

El benchmark ya existe: Docs/RESEARCH_HORROR_PUZZLES_20261009.md, con 18 juegos,
fuentes y conceptos originales. Reutilizarlo; no repetir una investigación extensa
para empezar esta iteración.

## Última evidencia humana

El 11 de octubre el usuario dice que le gusta cómo viene el juego y lo considera
mucho más sólido. Señala superposiciones de **textos de interfaz y también cuerpos
u objetos**. Lo escuchó con **auriculares** y encuentra insuficiente el volumen y
el impacto, especialmente en la persecución de la bomba: quiere un sobresalto
fuerte y más terror. Pide seguir iterando con criterio en terror, jugabilidad e
inmersión. La build exacta, posiciones y ajustes de volumen no fueron confirmados.

Es aceptación general de la evolución, no aprobación de todos los personajes,
puzzles o sustos. No reducir su pedido a una discusión abstracta sobre volumen:
hay que comprobar y corregir la presencia sonora real, además de la puesta en
escena. La intensidad emocional de la próxima versión requiere su escucha.

## Hallazgos y preguntas abiertas

- `Assets/_Game/Scripts/Presentation/SessionAudio.cs` limita la señal con umbrales
  .65 normal / .18 reducida y release de .12 s, sin elevar señales débiles.
  Esto justifica revisar la cadena completa; **no demuestra que el limitador sea
  la causa**. Revisar también ganancias, atenuación 3D, mezcla y ajustes guardados.
- `MachineCoverEncounter.Objective` revela marcha/pausa/RECALL en el HUD normal.
  `MachineCoverInteraction` bloquea la manivela sin cobertura. Son decisiones
  vigentes que conviene reconsiderar al trabajar descubrimiento y consecuencias;
  no son fallos mecánicos acreditados por esta última prueba humana.
- No hay una lista reproducible de las superposiciones. Localizarlas desde la
  cámara real, priorizando las que tapan acciones, atraviesan al jugador o rompen
  el encuentro. No dar por corregido lo que no se pudo observar.

## Próxima iteración propuesta: impacto y limpieza del encuentro

Convertir la persecución de la bomba en una secuencia intensa, legible y pulida,
y corregir las superposiciones más visibles. Partir de Embodiment y conservar
su campaña. Esta propuesta se ejecuta cuando el usuario la envíe como goal.

Dar libertad para ajustar mezcla, sonidos, tiempos, proximidad, actuación,
transiciones y espacio cuando mejore el encuentro. Resolver la falta de presencia
sonora a un mismo ajuste de escucha, construir anticipación, irrupción, persecución
y recuperación. Evitar una suma constante de gritos que borre el contraste y las
señales útiles. El jugador debe entender sus posibilidades mientras se siente
amenazado. Mantener reintentos ágiles y controles fiables.

Corregir prioridades y solapamientos del HUD y cruces visibles de cuerpos,
manos y objetos. Conservar el dato reproducido del Echo: cualquier corrección de
pose o interacción debe respetar la toma. El perseguidor es una entidad presente;
su actuación no justifica introducir decisiones autónomas en el Echo grabado.
No hace falta rehacer todos los puzzles ni personajes para cerrar esta iteración.

Verificar compilación y Console, comportamiento configurado y un recorrido del
encuentro con escape, captura/reintento y pausa. Comparar salida sonora real antes
y después con los mismos ajustes, preservando orientación, dinámica reducida y
parlantes; evitar saturación o limitación continua. Comprobar las vistas donde se
reprodujeron superposiciones. Ampliar pruebas sólo por cambios compartidos o fallos
concretos. No volver a recorrer la campaña repetidamente por rutina.

Entregar build macOS, comparación breve audiovisual útil y un informe conciso con
correcciones, límites y pocos puntos para la prueba humana. Terminar cuando esos
resultados sean revisables; no esperar cuotas agotadas ni afirmar que se logró
«hacer saltar de la silla» mediante comprobaciones automáticas.

Se mantienen los límites de AGENTS.md: volumen cómodo y controles de accesibilidad,
reglas deterministas del Echo, assets con derechos trazables, fuentes privadas fuera
de Git público, sin compras no autorizadas. Si se propone ElevenLabs, aplicar su
flujo específico de briefing y selección humana. Usar Editor/MCP/CLI para objetos
y referencias; no reescribir escenas YAML ni descartar cambios locales.

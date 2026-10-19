# Codex para Echo Protocol — evaluación, 2026-09-30

Estado: análisis y recomendaciones; no se instalaron plugins, no se cambiaron
configuraciones y no se inició un goal. Las skills propias descritas aquí todavía
no existen. La recomendación contempla la entrega del viernes 2 de octubre y
el trabajo posterior.

**Actualización posterior del usuario:** el goal limitado a la demo que aparece
abajo fue supersedido por la delegación de producción del juego completo,
incluyendo contenido y assets nuevos. Para ejecutar esta noche usar
[OVERNIGHT_GOAL.md](../OVERNIGHT_GOAL.md). Para ese encargo creativo autónomo
recomiendo Astra high en Standard; Sol sigue siendo la opción general de menor
consumo propuesta en esta evaluación. No se inició ningún goal.

## Recomendación

Para producción sostenida, usar GPT-6.1 Sol en Standard y esfuerzo medium como
base. Subir a high en tareas que atraviesen varios sistemas. Reservar Astra para
decisiones creativas difíciles, diagnóstico profundo o revisión de un hito.
Pro USD 100 tiene sentido si el trabajo será intensivo; medir resultados
verificados y consumo real antes de comprar un nivel mayor.

OpenAI Docs recomienda GPT-6.1 Sol para trabajo complejo y prolongado con menor
coste que Astra. Astra conserva el máximo nivel de capacidad; esa descripción
no constituye un benchmark de Echo Protocol. La elección concreta debe medirse
con tareas de este repositorio. [Modelos](https://learn.chatgpt.com/docs/models)
· [Selección](https://learn.chatgpt.com/docs/model-selection).

La documentación vigente ofrece Pro desde USD 100 y dice que Pro actualmente
no tiene límite de cinco horas. Pueden existir límites semanales. Fast consume
2.5 veces la cuota incluida respecto a Standard para el mismo modelo; para
trabajo nocturno recomiendo Standard. Astra Ultrafast requiere Pro USD 500 o
planes elegibles de organización. No convertir precios de API o créditos en
horas garantizadas de autonomía. [Precios](https://learn.chatgpt.com/docs/pricing).

Al consultar la cuenta todavía figuraba Plus: 45% consumido en la ventana de
cinco horas y 40% en la semanal. Es una lectura puntual anterior al cambio de
plan, no el presupuesto del futuro goal.

## Inspección local

- Stack documentado: Unity 6000.3.21f1, URP 17.3.0, C#, Input System 1.20.0,
  ProBuilder 6.0.8, Blender 5.2.0 LTS y macOS Apple Silicon.
- Codex CLI instalado: 0.159.0. Configuración global observada:
  `gpt-6.1-sol`, esfuerzo `high`, velocidad predeterminada. No se modificó.
- Unity MCP está configurado y habilitado. Unity no estaba abierto y no había
  herramientas Unity invocables en esta sesión. Hace falta verificar conexión
  efectiva antes de dejar una tarea de escenas trabajando sola.
- GitHub figura instalado. También hay herramientas locales de revisión,
  computer use, navegador, documentos, PDF e imágenes.
- D1 y el cierre están implementados. La orientación R3→Hub tiene evidencia
  objetiva y build `Builds/Demo-20260928-02/`; no hay una nueva confirmación humana
  de claridad para esa build. Véase
  [evidencia](../VisualEvidence/2026-09-28-R3-Orientation/README.md).
- Hay cambios y archivos sin seguimiento importantes. Un checkout desde HEAD
  no reproduce la versión actual. Respaldar el estado real antes de aislar trabajo.
- README conserva referencias a hitos/alcance anteriores. Usar el milestone
  actual de ROADMAP y corregir solo los datos necesarios para evitar reanálisis
  de decisiones ya superadas.

## Skills y marketplace: prioridad para este stack

Una skill aporta instrucciones y recursos; un MCP aporta acciones y datos.
Instalar una skill no aumenta la cuota. El ahorro posible viene de evitar
errores, búsquedas repetidas y reconstrucción de procedimientos.
[Concepto oficial](https://developers.openai.com/plugins/concepts/skills).

OpenAI aconseja descripciones cortas, activación precisa y cargar referencias
solo cuando correspondan. También advierte que recetas extensas, demasiadas
skills y lecturas obligatorias excesivas pueden empeorar resultados y consumo.
[Skills y prompts para Astra](https://learn.chatgpt.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra).

| Opción inspeccionada | Prioridad | Decisión para Echo Protocol |
| --- | --- | --- |
| Unity MCP existente + Editor | Muy alta | Confirmar conexión y reutilizarlo para referencias/escena. Es la capacidad central; no sustituirla por otro servicio. |
| Skills propias mediante skill-creator | Muy alta | Crear dos pequeñas para Unity y build; sumar audio cuando se trabaje la mezcla. Es una recomendación, todavía no una instalación. |
| GitHub + revisión local existentes | Alta | Revisar cambios coherentes al cerrar un hito. No hacer revisión completa después de cada ajuste de iluminación. |
| Record & Replay | Media | Útil para enseñar una operación repetitiva de macOS que no tenga vía fiable por Editor/API. No convertir todos los pasos en clics. |
| Imagegen existente | Selectiva | Referencias visuales o texturas cuando haya un objetivo aprobado. No sustituye un modelo 3D utilizable ni su importación. |
| Superpowers, disponible y no instalado | Diferir | Su flujo general introduce aprobación, planes y revisiones amplias. No es mi opción para reducir consumo o dejar este goal autónomo. |
| Context7, disponible y no instalado | Opcional | La ficha ofrece documentación actual. No comprobé cobertura de Unity 6000.3, URP 17.3 ni Input System 1.20; no lo considero requisito. |
| Figma, Sites, herramientas de apps web | Baja para este hito | Pueden servir a otros entregables; no resuelven la escena ni la build Unity. |

La búsqueda del directorio no encontró una opción relevante de Unity, Blender
o ElevenLabs. Es una búsqueda no exhaustiva: no demuestra que no existan
integraciones. Hay más opciones en el [directorio](https://chatgpt.com/plugins).
No se sugirió ni instaló ningún plugin externo.

### Auditoría específica de Superpowers

Se leyeron archivos del paquete del marketplace como material de análisis; no
se adoptaron sus instrucciones. `using-superpowers` tiene activación universal;
`brainstorming` exige aprobación de diseño antes de implementar y otra revisión
del documento. `test-driven-development` aplica TDD a funciones y correcciones,
con excepciones consultadas al humano. `subagent-driven-development` incluye
implementador y dos revisores por tarea, más ciclos de corrección.

Ese procedimiento puede ayudar en una biblioteca C# difícil o una refactorización
grande. Mi evaluación para este proyecto: añade demasiada ceremonia al ajuste
de escenas y puede interrumpir un goal que debería avanzar con dirección ya
aprobada. No hay medición que permita prometer un ahorro de tokens. El código
inspeccionado está en el paquete local del marketplace; el proyecto del autor
está en [obra/superpowers](https://github.com/obra/superpowers).

### Skills propias propuestas

1. **echo-unity-milestone:** abrir proyecto/escena correctos, comprobar conexión,
   preservar el estado, editar por Editor, compilar y revisar Console/referencias;
   ejecutar la prueba de comportamiento afectada y capturar desde Player Camera.
   Reusar las pruebas archivadas cuando sean pertinentes; no construir un harness
   nuevo para cada cambio ni exigir toda la ruta por un rótulo.
2. **echo-build-macos:** comprobar escenas y compilación, producir una app con
   nombre nuevo, registrar inputs/versiones/resultado, verificar arranque y
   preparar entrega. El comando actual `VS1DemoDeliveryEditor.BuildMacOS` contiene
   una ruta fija a Demo-01: no reusarlo a ciegas ni sobrescribir la base aprobada.
3. **echo-audio-check:** inspeccionar duración, formato, picos, clipping y loops,
   verificar importación/routing y trazabilidad de derechos; informar qué queda
   para escucha humana. No atribuir miedo, buena actuación o comodidad a un
   análisis de señal.

Crear las skills con instrucciones mínimas y scripts comprobados, sin duplicar
AGENTS.md ni convertirlas en un framework. La ganancia aún es una hipótesis.

Record & Replay permite convertir una demostración en macOS en una skill y
requiere Computer Use disponible. Lo usaría como alternativa para tareas de
Editor difíciles de describir, después de intentar la integración existente.
[Documentación](https://learn.chatgpt.com/docs/extend/record-and-replay).

## Política de modelos propuesta

| Trabajo concreto | Model | Effort |
| --- | --- | --- |
| Implementación local, claridad de interacción, menús, build | GPT-6.1 Sol | medium |
| Estado y RECALL, fallos difíciles entre componentes, rendimiento medido | GPT-6.1 Sol | high |
| Definir un beat de horror difícil, valorar alternativas narrativas, revisar un hito complejo | GPT-6 Astra | medium; high si lo requiere |
| Inventarios, extracción y documentación repetitiva muy acotada | GPT-6 Luna | low o medium |

No usar Max/Ultra como base permanente. Mayor esfuerzo no garantiza mejor juego.
Si se autoriza trabajo paralelo, reservarlo a inspecciones o archivos independientes;
mantener un único escritor de escena/Editor. La configuración del goal selecciona
un modelo: no asumir cambio automático de modelo entre pasos.

Evaluar un primer hito con Sol: resultado funcional, correcciones necesarias,
tiempo y consumo observado. La diferencia de porcentajes de cuota es aproximada
si hay otras tareas o un reset. Escalar a Astra cuando haya un problema concreto
o una decisión de alto impacto; no repetir todas las tareas con ambos modelos.

## Goals útiles para este juego

Un goal conserva un objetivo y continúa a través de turnos hasta una condición
de parada. Debe tener resultado verificable; no es una promesa de ejecución
infinita. Para trabajo local, mantener el Mac conectado y activar Prevent sleep
while running. [Goals](https://learn.chatgpt.com/docs/long-running-work)
· [Contrato de finalización](https://developers.openai.com/cookbook/examples/codex/using_goals_in_codex).

### Primer goal: entrega del viernes

Borrador para una nueva tarea del mismo proyecto, después de comprobar plan y
conexión. Se propone como contrato de ejecución, no está activo:

> Preparar una demo macOS de Echo Protocol para la entrega del 2 de octubre y
> una demostración en vivo de cinco minutos. Partir del estado actual, incluida
> Demo-20260928-02, la corrección R3→Hub, D1 y el cierre. Leer AGENTS.md y el hito
> actual; preservar los cambios y archivos sin seguimiento mediante una copia
> local recuperable. Verificar acceso real a Unity antes de modificar escenas.
> Revisar el recorrido completo desde una sesión fresca, interacción, RECALL,
> transiciones, D1, Reiniciar y Salir; corregir fallos reproducibles de la entrega.
> Comprobar compilación, Console, referencias y comportamiento relevante tras
> cada corrección. Producir una nueva build identificable con evidencia y controles,
> y un guion de cinco minutos con preparación previa y alternativa si falla la demo.
> Conservar los datos exactos del Echo y el contenido aprobado. Evitar nuevas
> mecánicas, hechos sobre accidentes, grandes refactorizaciones o servicios pagos.
> No publicar ni empujar cambios. Continuar hasta tener los artefactos y pruebas
> técnicas; registrar aparte las cuestiones de ritmo y percepción que requieran
> al creador, sin inventar validación humana ni pedir testers externos.

La autonomía local no comprueba por sí sola actuación, sensación de terror o
legibilidad en una sala real. La revisión humana puede realizarse al recibir la
build; no debe fingirse ni sustituirse por una captura favorable.

### Después de la entrega

Conservar la demo como base recuperable y fijar otro objetivo con un hito creativo
aprobado. Trabajar por resultados jugables: dirección, implementación, evidencia,
build y revisión del creador. Final C requiere definir los hechos narrativos que
siguen TBD antes de delegar su implementación. No pedir al goal que rellene tiempo
o invente trauma para alcanzar una duración.

## Reducir consumo de forma concreta

- Mantener una tarea por hito coherente. Crear una nueva cuando el trabajo sea
  independiente, con un resumen breve de estado; no repetir el GDD entero.
- Consultar documentación según el cambio y señalar evidencia existente.
- Guardar logs completos en archivos y devolver resúmenes de fallos relevantes.
- Reutilizar comandos y checkpoints de diagnóstico; además probar el recorrido
  real cuando el cambio afecte transiciones. El setup no demuestra la transición.
- Evitar reiniciar Unity, reimportar todo, reconstruir el proyecto o rehacer todas
  las pruebas cuando un cambio local no lo exige.
- Abrir problemas por evidencia y modificar una causa concreta. Evitar ciclos
  interminables de planes, auditorías o pulido sin una referencia visual aprobada.

Estas son recomendaciones basadas en el estado inspeccionado. El resultado de
automatización se medirá en hitos verificados; no se promete una cantidad de horas
ni de funciones por plan.

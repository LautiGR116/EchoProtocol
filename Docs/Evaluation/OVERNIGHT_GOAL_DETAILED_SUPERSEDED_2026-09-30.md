> HISTÓRICO: supersedido por la instrucción del usuario de dar libertad creativa
> y evitar un plan detallado obligatorio. No usar como encargo de ejecución.

# Echo Protocol — encargo autónomo para la noche

Fecha: 2026-09-30. Estado: **preparado para ejecutar; goal todavía no iniciado**.
Entrega: viernes 2 de octubre, build macOS y presentación en vivo de cinco minutos.

## Autoridad y resultado

El usuario delegó directamente las decisiones creativas y técnicas y amplió el
alcance: «crear nuevos acertijos interesantes, nuevos controles, nuevas escenas.
El juego completo si querés», incluyendo Blender, descarga de audio y generación
de imágenes. Esto supersede el encargo anterior limitado a la demo y las puertas
de aprobación de diseño, narrativa TBD, roster de puzzles y duración 20–25 minutos.
El agente puede elegir, implementar, evaluar y corregir esas decisiones sin
esperar al usuario. No tiene que presentar propuestas y detenerse.

El resultado buscado es **el juego completo más logrado que se pueda construir y
verificar durante esta ejecución**, aprovechando el trabajo actual. Una primera
build completa es un respaldo intermedio, no el fin del encargo. Seguir mejorando
y ampliando contenido integrado mientras falten objetivos sustanciales. No
prometer horas de ejecución ni una duración de juego que no se haya observado.

La libertad incluye nuevas mecánicas y escenas cuando mejoren el juego. El
criterio de selección es terror psicológico, coherencia y profundidad por coste;
no cantidad de salas, puzzles, documentos o assets. Una campaña breve excelente
es válida, pero no usar la antigua demo como techo ni añadir esperas para alargarla.

## Dirección que gobierna las decisiones

- Echo útil → aparentemente poco fiable → temido. Sus registros son fieles.
  Actualmente registra posición y yaw corporal; no reproduce voz, mirada ni
  acciones semánticas no capturadas. Un canal nuevo exige implementación real,
  reglas enseñadas y prueba de fidelidad. Mantener un Echo es la estrategia inicial
  por profundidad/coste, no una excusa para evitar desafíos nuevos.
- Construir miedo en el jugador mediante anticipación, sonido espacial, silencio,
  separación, exposición, arquitectura y consecuencias escritas. Cada puzzle
  nuevo necesita una operación interesante y una vulnerabilidad distinta; evitar
  repetir P3 cambiando la decoración o agregar una colección de sobresaltos.
- La facility fría se contamina de domesticidad y termina coexistiendo con la
  memoria. Ambos traumas —hermano menor en la infancia e hijo después— convergen.
  Resolver los hechos pendientes con criterio propio y asentarlos como decisión
  autónoma antes de producir su contenido. No presentar D1 como prueba histórica:
  demuestra negación de una pausa realmente registrada en el presente.
- Final C: confrontación del recuerdo, fin de la pesadilla y negro, sin mostrar
  el despertar. Ganar el final mediante acciones y espacio, no sólo textos.
- Conservar el núcleo de RECALL y lo valioso de P1/P2/P3; se puede mejorar su
  claridad, ritmo y presentación. Evitar convertirlo en combate o persecución
  genérica. Controles nuevos sólo cuando sostengan acciones que el juego necesita.
- Audio a volumen cómodo, sin picos como sustituto del miedo. Mantener caminos
  utilizables con parlantes y dinámica reducida. Sensación y casting sin escucha
  humana permanecen EN PRUEBA; no bloquean desarrollo ni autorizan inventar aprobación.

## Orden de ejecución y criterio de cierre

1. **Partir de la fuente real y asegurar recuperación.** Leer AGENTS, README y el
   milestone de ROADMAP. Consultar documentos temáticos al necesitarlos. Preservar
   el working tree completo, incluida la escena sin seguimiento. Revisar la copia
   de preparación y crear otra si hubo cambios posteriores. Comprobar Editor,
   compilación, escena y Console; empezar a construir, sin repetir toda la auditoría.
2. **Cerrar la campaña de punta a punta.** Definir rápidamente arco, hechos
   narrativos y final jugable; construir el tramo que falta después de D1 y las
   conexiones necesarias. Adaptar/reemplazar `VS1DemoEnding`: actualmente desactiva
   Player y loop al presenciar D1 y termina ahí. El nuevo final no puede quedar
   detrás de ese bloqueo. Hacer una build macOS completa temprano, separada de las
   demos anteriores, con inicio claro y Reiniciar/Salir funcionales.
3. **Ampliar con contenido que merezca existir.** Crear al menos un desafío nuevo
   sustancial integrado en la campaña, con preparación, solución, consecuencias y
   salida verificables; no contar un cartel o fundido como acertijo. Diseñar otras
   salas, controles y secuencias si mejoran esa campaña. El mínimo sirve de piso,
   no de cuota máxima. Enseñar cada regla, ofrecer recuperación de errores y evitar
   softlocks, precisión excesiva o recorrer de nuevo minutos de contenido resuelto.
4. **Producir la experiencia de terror.** Integrar ambientación, transiciones,
   motivos sonoros con variantes, narrativa y arte donde el jugador realmente los
   perciba. Usar Blender para geometría/modelos concretos e imagegen para bitmaps
   útiles. Ajustar orientación R3→Hub, lectura de objetivos sin revelar soluciones,
   aspecto del Echo, escala, iluminación, materiales y coherencia entre zonas.
   Mantener builds recuperables después de cada ampliación importante.
5. **Cerrar para entregar.** Incorporar pausa, controles y ajustes de audio/sensibilidad
   que sean necesarios, y un modo de presentación de cinco minutos accesible y
   separado del recorrido normal si éste lo necesita. Guardado sólo si restaura
   coherentemente el estado; no añadir un guardado de posición que rompa progresión.
   Recorrer desde un inicio limpio hasta el final, corregir regresiones, generar
   build final, comprobar arranque y documentar los límites reales de las pruebas.

El goal se completa cuando la campaña elegida está implementada y conectada,
incluye contenido nuevo sustancial y Final C, las mejoras prioritarias de terror
y presentación están integradas, y existe una build final comprobada con entrega
clara. No declararlo terminado tras la primera build ni gastar el resto en
documentar trabajo que falta. Si el presupuesto o las herramientas impiden cerrar
algo, conservar la última build sana y reportarlo como pendiente, sin falsear estado.

## Herramientas, assets y autonomía práctica

- Unity 6000.3.21f1, URP, Input System, componentes pequeños y referencias explícitas.
  Autorizar escenas/prefabs por MCP, Editor o C# ejecutado **dentro del Editor**;
  no editar su YAML. Unity CLI local y `eval` se comprobaron en esta preparación.
  Una vía MCP ausente no exige parar: usar la CLI comprobada. Mantener un único
  escritor sobre la escena y no lanzar dos Editors sobre el mismo proyecto.
- Blender 5.2 está instalado. Usar scripts reproducibles cuando aporten velocidad,
  conservar `.blend`/fuente fuera del runtime y exportar assets seleccionados a
  Assets con materiales URP, escala y colisión comprobadas.
- Audio: el usuario autoriza buscar, descargar, seleccionar e integrar por criterio
  del agente. Preferir la biblioteca existente y nuevas fuentes CC0 verificadas
  para este repo público. Guardar URL canónica, autor, licencia, fecha, hash y
  tratamiento en `AUDIO_ASSET_LEDGER.md`; incluir créditos. Si una fuente requiere
  login/pago o no permite redistribución, elegir otra o crear sonido propio y
  continuar. No reinstaurar la respiración rechazada ni llamar humano-aprobadas a
  nuevas selecciones. Escuchar cuando haya vía real y medir canales, loop, picos y
  mezcla; las mediciones no demuestran miedo ni calidad interpretativa.
- Imágenes: skill `imagegen` y herramienta integrada, sin contratar APIs. Producir
  pocos assets con función concreta —textura, recuerdo, superficie doméstica— y
  copiarlos al proyecto antes de referenciarlos. Si falla, usar fuentes existentes
  o materiales procedurales. No pasar silenciosamente a una API facturada.
- No depender de voces generadas para terminar. No clonar voces de menores ni
  eludir bloqueos de proveedores. No introducir servicios pagos, paquetes enormes,
  frameworks, cuentas ni compras. Descargas/importaciones pequeñas y deliberadas;
  revisar licencia, tamaño y coste de render antes de incorporarlas.
- No hacer push, publicación externa, mensajes a terceros, compras, borrados del
  trabajo previo ni cambios de historia Git. Preservar demos y fuentes existentes.
  No staging/commit si aparecen posibles secretos. Resolver problemas locales de
  forma reversible y seguir con trabajo independiente si falla un recurso opcional.
- Reutilizar harnesses/evidencias útiles, sin confundir checkpoints guiados con un
  recorrido humano completo. Probar novedades y regresiones relevantes, no repetir
  una auditoría o suite enorme tras cada cambio cosmético. Revisar visualmente Game
  View y la UI; un harness que llama métodos no valida clicks ni legibilidad.
- Guardar estado breve en ROADMAP y evidencia por hito para que sobreviva a la
  compactación. Anotar siguiente acción y última build sana. No crear procesos de
  aprobación internos, skills genéricas o documentación especulativa como sustituto
  de construir el juego. Las sesiones con jugadores externos están diferidas.

## Entrega al despertar

Crear un directorio nuevo `Builds/Overnight-<fecha>-<version>/` con `.app`, `.zip`
que preserve la aplicación macOS, instrucciones, controles, créditos y guía de
demo de cinco minutos. No sobrescribir Demo-20260928-01/02. El antiguo
`VS1DemoDeliveryEditor.BuildMacOS()` apunta a Demo-01: cambiar esa ruta o usar una
build nueva mediante el Editor antes de invocarlo.

Dejar `Docs/OVERNIGHT_REPORT.md` con: qué se puede jugar y dónde; contenido nuevo;
decisiones narrativas; pruebas y captura de puntos clave; última build sana;
errores conocidos; qué requiere unos minutos de revisión humana. Actualizar sólo
los documentos afectados. Distinguir implementado, verificado y EN PRUEBA.

## Arranque recomendado

Nuevo chat local del proyecto **EchoProtocol**, sobre
`/Users/lautigr116/EchoProtocol`, con GPT-6 Astra, effort high y velocidad Standard.
No crear un worktree desde HEAD: la producción actual contiene archivos sin
seguimiento que no aparecerían ahí. Cerrar chats que estén editando este proyecto.
Confirmar el plan y cuota en la app; enchufar el Mac, mantenerlo abierto y activar
**Prevent sleep while running**. El goal no evita límites de uso ni garantiza
que una ejecución continúe hasta una hora determinada.

Pegar como goal (usar `/goal` en Codex):

```text
Construí el juego completo de Echo Protocol más logrado que puedas a partir del
working tree actual en /Users/lautigr116/EchoProtocol. Leé y ejecutá
Docs/OVERNIGHT_GOAL.md: contiene mi delegación directa de todas las decisiones
creativas y técnicas y supersede el alcance anterior limitado a la demo. Podés
crear acertijos, controles y escenas, resolver la narrativa, usar Blender,
descargar e integrar audio gratuito con derechos claros y generar imágenes con
la herramienta integrada. No esperes mis aprobaciones rutinarias ni un playtest
externo mientras dormí; resolvé con tu criterio y verificá el resultado.

Primero asegurá una campaña jugable de inicio a Final C y una build recuperable;
después seguí ampliando y puliendo terror, puzzles, arte, audio, transiciones y UX
hasta cumplir el encargo completo. Esa primera build es un checkpoint, no el
objetivo final. Conservá la fidelidad del Echo, el trabajo existente y las demos.
No gastes dinero, publiques ni dependas de herramientas opcionales que fallen.
Entregá una nueva app macOS comprobada, créditos, guía de presentación de cinco
minutos y Docs/OVERNIGHT_REPORT.md con resultados y límites reales de las pruebas.
```

Preflight y comandos comprobados:
[VisualEvidence/2026-09-30-Overnight-Preflight/README.md](VisualEvidence/2026-09-30-Overnight-Preflight/README.md).

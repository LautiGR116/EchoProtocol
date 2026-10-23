# Ronda 5 — el intruso deja de parecer cotidiano

2026-10-23. Cinco chats GPT-6 Astra / Ultra autorizados. Sólo Editor, sin
builds ni publicación y con checks puntuales. Base integrada `d2ea5398`, rama
`codex/integracion-20261023`. La ronda 4 está guardada en Incursion y recuperable.
Esto es dirección aprobada, no implementación ni miedo aprobado.

Coordinador: `01a12cb5-6030-77a1-b02b-93734f7c279b`, original
`/Users/lautigr116/EchoProtocol`. Sólo él usa el Editor central e integra escena.

## Diagnóstico y dirección compartida

La ronda 4 quitó la máscara defectuosa y dio apoyo al cuerpo, pero la vista final
todavía parece un hombre corriente saludando tras un mostrador. Reducir otra vez
la sonrisa unos milímetros o cambiar el color no alcanza. El usuario pide ir
más a fondo. La lectura visual es del coordinador; no hay nueva partida humana
identificada de ronda 4. No inventar un resultado de playtest.

Dirección: **un adulto que contiene una violencia física que termina escapándose**.
La anatomía, ropa, gesto y voz deben contar el mismo estado: mandíbula contenida,
tensión desigual y una mirada difícil de sostener; quietud deliberada seguida
por una invasión brusca y con peso de la distancia. Puede ser macabro o crudo,
con alteración anatómica visible y localizada, sin volver a un rostro derretido,
una máscara plana ni manchas arbitrarias. La amenaza no depende de identificar
una herida o un detalle de lore. Ningún cambio en el Echo grabado.

Evidencia obligatoria, disponible en el original, sin repetir recorridos:
`Builds/round4-integration-20261023/p3-settled-corrected.png`,
`pump-contact.png`, `pump-recovery.png`; leer además la entrega de ronda 4.
Leer AGENTS, README, hito actual de ROADMAP y diseño relevante. La prioridad es
implementar una alternativa fuerte y concreta, no prolongar una auditoría.

## A — anatomía y expresión estática

Dueño exclusivo: `Assets/Editor/IntruderFaceGeometry.cs`, nuevo
`Assets/Editor/IntruderAnatomyIntegration.cs` y auxiliares con ese prefijo,
`Docs/INTRUDER_ANATOMY_ROUND5.md`.

Rediseñar la lectura cercana del rostro: arco de boca inequívocamente contenido,
mandíbula/masetero, órbitas/párpados y transición de cuello. Elegir una asimetría
principal que se reconozca desde la distancia de juego. No limitarse a subir los
coeficientes de todas las gaussianas. Preservar volumen, ojos asentados, UV,
topología, huesos/pesos y cuerpo fuera de cabeza/cuello. Sin blendshapes nuevos,
animación, materiales ni shader: B posee superficie y C todos los huesos.
Conservar UV2 con coordenadas del scan para el shader. Entregar un upgrade
explícito desde la variante ronda 4 ya instalada, con backup privado y rollback;
el Apply antiguo es no-op y no regenera por sí solo. No ejecutar EmbestidaProduction.

## B — piel, ojos y vestuario

Dueño exclusivo: `Assets/_Game/Shaders/EmbestidaSurface.shader`, shaders nuevos
`IntruderDread*`, `Assets/Editor/IntruderSurfaceIntegration.cs` y auxiliares,
assets nuevos pequeños `IntruderDreadSurface`, `Docs/INTRUDER_SURFACE_ROUND5.md`.

Rehacer el acabado del mismo adulto: quitar la lectura saludable/casual y dar
materialidad coherente a piel, boca/ojos y ropa de trabajo. Elegir desgaste,
humedad y contraste por regiones físicas; no una capa uniforme de gris, círculos
negros pintados o sangre aleatoria. Ropa y cara deben pertenecer al mismo cuerpo.
Usar coordenadas originales UV2/UV0 y preservar detalle/licencia del scan.
No cambiar mesh, huesos ni textura original compartida. Una propiedad nueva
opt-in, default apagado, mantiene la ronda 4 y otros actores recuperables.
  Su integrador ajusta sólo materiales del intruso; conserva las referencias
  actuales de Skin.mat/Eyes.mat y respalda sus valores en copias privadas.
  A conserva igualmente el objeto/GUID Face.asset. Ninguno reescribe el array
  del renderer ni un recibo incompatible; no tocar el integrador del otro.

## C — actuación y mirada

Dueño exclusivo: `Campaign/P3Intrusion/P3BodyIntrusion.cs`,
`Campaign/MachineThreatPerformance.cs`, `Campaign/EmbestidaRush.cs`, carpeta
`Campaign/IntruderActing/`, `Assets/Editor/IntruderActingIntegration.cs`, nuevo
upgrade `IntruderPerformanceIntegration.cs`, `Docs/INTRUDER_PERFORMANCE_ROUND5.md`.

Único escritor de articulaciones. Sustituir el gesto social de palma extendida
por presión/apoyo reconocibles y una postura que invade; cabeza/mirada con
intención, no seguimiento suave continuo. Crear contraste grande entre quietud,
carga, aceleración, impacto y recuperación. Puede cambiar poses y tiempos locales
sin endurecer reacción/captura ni mover targets fuera de alcance. Mantener
CharacterController, contacto real, apoyos, pausa y transferencia del mismo actor.
No añadir más microgestos como sustituto de dos o tres poses fuertes. No cambiar
fuentes/mezcla, shader, geometría, intruder staging ni reglas del puzzle.

## D — voz, cuerpo y amenaza sonora

Dueño exclusivo: `Audio/MachineCoverPresence.cs`, nuevos scripts `IntruderDreadAudio`,
`Assets/Editor/IntruderAudioIntegration.cs`, assets en
`Assets/_Game/Audio/IntruderDread/`, manifiesto y doc `Docs/INTRUDER_AUDIO_ROUND5.*`.

Crear una actuación sonora distinta y reconocible: respiración contenida,
expulsión corporal breve y contacto seco que anuncian una fuerza cercana. Buscar
primero en los masters licenciados del original; se autoriza material nuevo
pequeño con fuente/licencia verificadas y trazabilidad. No simular una nueva voz
cambiando sólo pitch/volumen. Puede sustituir clips de las fuentes existentes
en el integrador, pero C/los dueños actuales conservan los disparos de contacto.
No editar Staging ni controlar luces (aunque Presence ya contiene referencias).
Sin segunda reproducción del golpe, bucles vocales ni nueva escritura de volumen
que compita con Staging. Hooks imprescindibles se entregan al coordinador con
firma exacta. WorldSFX, master, limitador, mono y dinámica reducida se conservan.
No ElevenLabs en esta ronda: el casting humano de ese flujo no debe bloquearla.
No certificar timbre o miedo por análisis de señal. Nuevos WAV sólo los necesarios.

## E — descubrimiento y puesta en escena

Dueño exclusivo: presentación visual de `Campaign/IntruderStaging.cs`, nuevo
`Assets/Editor/IntruderRevealIntegration.cs`, scripts opcionales `IntruderReveal`,
`Docs/INTRUDER_REVEAL_ROUND5.md`.

Recomponer P3 y la entrada de bomba para que no parezcan un retrato de oficina:
luz local, contraste de fondo, marco y oclusión parcial que llevan a descubrir
primero un cuerpo y después su intención. El rostro debe poder leerse antes del
contacto, sin oscuridad que esconda los defectos. Puede ajustar utilería local
y luces mediante integrador, con pasos y líneas del puzzle/escape despejados.
No mover raíz/stop/targets del intruso, Echo, receptores, puertas funcionales ni
cámara. No alterar audio, triggers/lógica de aparición/retiro, exposición global,
postproceso o guardado. Sin estrobo. Reutilizar luces; evitar otro Update que
compita con Staging o Presence. Si hace falta un cambio causal, proponerlo con
ubicación exacta al coordinador; no resolverlo invadiendo el archivo de C/D.

## Trabajo paralelo y entrega

- Un worktree por chat, sin escribir el original ni otro worktree. Las copias
  usan sparse-checkout y excluyen medios/escenas grandes. Leer esos recursos
  en el original; no copiarlos ni abrir cinco Editors/Libraries. Tras checkout,
  reducirlo antes de trabajar si todavía no es sparse. No descargar paquetes.
- Sólo el coordinador toca la escena mediante Editor/Unity MCP. Cada frente
  entrega código/assets y operación precisa de upgrade desde ronda 4, con
  preflight, Undo/rollback y sin guardar/abrir escenas automáticamente. No YAML.
  No modificar integradores/recibos de otros frentes ni reejecutar generadores
  históricos. Resolver dependencias mediante contratos pequeños, no frameworks.
- Commits enfocados; sin push/merge/build. Derivados Renderpeople privados no
  van a Git. No modificar README, ROADMAP, este plan, opciones ni ajustes globales.
- Como máximo un subagente por chat para un trabajo disjunto que acelere;
  sin cadenas de delegación, coordinadores extra ni revisiones generales.
- Check propio: diff, referencias y la comprobación local más barata pertinente.
  No afirmar compilación/Play sin Editor propio. Central: una compilación conjunta,
  Console y presentación breve de P3 + entrada de bomba; repetir sólo por edición
  relevante o fallo. Sin suites amplias, nuevo harness ni recorridos completos.
- Entregar pronto commits, operación de integración, diferencia visible concreta
  y una limitación humana. Una cara distinta todavía puede no dar miedo: no
  aprobarla por número de vértices, contadores o retórica. La crítica final central
  contrasta el conjunto con la captura de ronda 4.

## Estado

Los cinco chats se iniciaron desde `abdbae4c`, en Astra / Ultra, y su actividad
fue confirmada. Implementación **EN CURSO**, todavía sin aplicar al Editor:

| Frente | Chat | ID | Worktree |
| --- | --- | --- | --- |
| A | Intruso: anatomía que incomoda | `01a16a37-5652-7801-b152-00b7e6cf5eac` | `7d20` |
| B | Intruso: piel, ojos y ropa de una amenaza | `01a16a37-74c1-72c3-9339-8901089121d7` | `a77b` |
| C | Intruso: quietud que se convierte en… | `01a16a38-1ede-7853-98a8-dca1e451226e` | `a468` |
| D | Intruso: una voz y un cuerpo que amenazan | `01a16a38-1ede-7853-98a8-dc97e9ba09ad` | `aa2f` |
| E | Intruso: descubrirlo demasiado cerca | `01a16a38-4ee0-7211-ad18-eb383cfdad2a` | `0fc4` |

Se redujeron con sparse-checkout los tres worktrees limpios de ronda 4,
conservando commits y originales; las nuevas copias también son reducidas.
A/B recibieron el contrato de referencias invariables; E recibió la dirección
de piel/ropa más oscura para componer una luz que preserve su lectura.
La escena jugable sigue en ronda 4; no se construyó ni publicó ninguna versión.

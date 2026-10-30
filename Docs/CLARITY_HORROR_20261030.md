# Claridad y presión — 2026-10-30

Integración local en **Incursion**, sólo Unity Editor. Dirección autorizada:
hacer más fácil entender cómo jugar y progresar, y reforzar el miedo perceptible.
Tres agentes trabajaron en worktrees separados; los cambios se reúnen en
`codex/claridad-terror-integracion-20261030` y se integran al proyecto principal.

## Implementado

- Inicio y «Cómo jugar»: grabar → R → cooperar. H abre controles antes de jugar;
  durante la partida abre ayuda y pausa. La pausa muestra la próxima acción.
- HUD separado para la toma actual y el Echo anterior, con tiempos y barras.
  Instrucciones por estado, orientación espacial inicial, indicaciones junto a
  lectores y cuentas regresivas de pausas sugeridas. Las sugerencias no cambian
  los requisitos ni la duración de una grabación.
- P2 distingue la mirada del Player de la de un Echo anterior. Las pistas
  automáticas ya no consideran cada reintento como progreso nuevo.
- La bomba explica marcha → pausa → segunda marcha, cobertura real y recuperación
  de una toma sin otro arranque. Durante la amenaza conserva una instrucción
  breve de escape y los subtítulos.
- Una marcha manual real prepara la puerta. Al detenerse la bomba, una pausa
  deja oír su respuesta: golpe, retroceso y hoja que queda cargada. Ocurre una
  vez; RECALL y reintento no vuelven a gastar ese golpe.
- El contacto real de la irrupción provoca una caída única de la luz local;
  la salida sigue iluminada. La primera acometida gana velocidad hacia la
  última posición observada, con colisión y margen de reacción existentes.
- Al apagarse la bomba tras el desvío, la mano permanece apoyada mientras cabeza
  y hombro buscan la última evidencia percibida antes de volver a buscar.

No nuevos assets, voces, dependencias o cambios de volumen maestro. Se conservan
la toma fiel, mezcla mono, dinámica reducida, limitador, pausa y reintento.
`ProgressionClaritySetup.ApplyToOpenScene()` conecta las referencias de orientación
en Incursion; es idempotente y no abre ni guarda escenas por sí mismo.

## Verificación y entrega

Compilación conjunta sin errores ni advertencias de C#. Inicio, controles y HUD
inspeccionados en el Editor; H/Esc y una segunda revisión visual de inicio/P1.
Se reutilizaron dos recorridos breves existentes, con entrada real y sin fixtures:

- P1: **PASS**, 59 comprobaciones, un RECALL y una toma inmutable; ~36 s.
- Bomba aislada: **PASS**, 37 comprobaciones, un RECALL y una toma inmutable;
  ~98 s. Marcha/pausa/marcha, respuesta única de puerta, manivela, rejilla,
  irrupción, desvío y salida. Ninguna reanudación inesperada de la pausa.
- Sin errores nuevos de juego durante esos recorridos. La captura de escucha
  de la bomba no llega a saturación digital; esto no aprueba comodidad ni miedo.

La nueva mirada tras detener la bomba durante el desvío está implementada,
pero ese ramal no se activó en el recorrido rápido. P2/P3 se revisaron en código;
no se repitió la campaña. Escena guardada con las referencias de guía conectadas.
Evidencia local en `Builds/clarity-horror-20261030/`.
Sin builds ni publicación. Los assets privados conservan su exclusión de Git.

Comprensión espontánea, miedo, actuación y comodidad de escucha siguen
**EN PRUEBA humana**; ninguna comprobación técnica los aprueba.

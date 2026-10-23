# IntruderFace — ronda 4, frente A · 2026-10-23

**Integrado y guardado en el Editor central; miedo EN PRUEBA humana.**
Aplicación, corrección de expresión y límites en
[la entrega central](PARALLEL_INTRUDER_ROUND4_20261023.md#integración-central--2026-10-23).
Lo siguiente conserva el alcance y las comprobaciones propias del worktree.
Base `d50783f4`, rama `codex/intruder-face-round4`. Sólo geometría/material del
intruso presente; no se abrió Unity ni se escribió el proyecto original.

La captura P3 de ronda 3 muestra la lectura de máscara. El generador histórico
superpone hundimientos de mejilla, desplazamiento mandibular, labio arrastrado
y cráneo rebajado, incluso sobre el submesh ocular. La lectura numérica del par
privado encuentra hasta **41,8 mm** de desplazamiento acumulado en la cabeza.
Su shader además pinta círculos orbitales/llagas y elimina el normal del pelo.
No se ejecutó ese generador.

El nuevo mesh recupera el volumen adulto del scan en cabeza y ojos, con transición
de cuello entre y=1,575–1,625 m. Conserva el cuerpo existente fuera de esa zona.
Después añade presión asimétrica pequeña: pómulo/masetero, comisuras sin elevación,
volumen del labio, ceja y párpados. Los párpados se asientan sobre la envolvente
original de cada ojo. No añade apertura permanente de mandíbula. Las normales
y tangentes suaves importadas se transforman con la deformación; no se recalculan
partiendo las costuras UV. Se conservan topología, pesos, bindposes y nombres.

Un material privado activa `_IntruderFace=1` en EmbestidaSurface: detalle/color
del scan y respuesta mate con variación regional moderada, sin grandes manchas
que sustituyan anatomía. El ojo privado conserva iris/esclerótica y reduce brillo
plástico/normal. `_IntruderFace=0` mantiene la salida histórica. Ropa, manos,
delantal, Echo, SE2, figuras de casa y luces no cambian.

## Operación central

1. Integrar el commit y esperar importación. Usar la **Incursion ya abierta**,
   fuera de Play. No ejecutar `EmbestidaProduction.Main()`.
2. `Echo Protocol > Intruder face > 1. Validate current scene` comprueba actor
   presente, pareja Nathan/Embestida, pesos/topología, materiales y recibo privado.
3. `Echo Protocol > Intruder face > 2. Apply to current scene`, o
   `IntruderFaceIntegration.ApplyToCurrentScene()`, crea y asigna la variante.
   La escena queda **sin guardar**; sólo se guardan assets privados propios.
4. Un encuadre cercano P3 y la entrada de bomba durante el check central compartido
   bastan para decidir si conservar el pase. Guardar escena explícitamente allí.

Se generan `Face.asset`, `Skin.mat`, `Eyes.mat` e `Installation.asset` bajo
`Assets/_Game/Art/Embodiment/RenderpeoplePrivate/IntruderFace/`, ya ignorado.
El recibo guarda referencias y bounds anteriores. Apply repetido no modifica una
instalación íntegra; después de Undo/Rollback reutiliza los mismos assets. Rechaza
archivos huérfanos, otro actor o referencias alteradas sin sobrescribirlos.

Undo revierte la asignación. `IntruderFaceIntegration.RollbackCurrentScene()`
(menú **3. Roll back current scene**) restaura mesh/materiales/bounds incluso tras
recarga del Editor. Conserva los assets para Redo/reaplicar. Una excepción durante
Apply revierte el renderer y elimina únicamente los assets creados por esa llamada.
No abre/guarda escenas ni copia modelos al worktree. Ningún derivado privado va
al commit; los derechos/procedencia siguen siendo los del recurso privado existente.

## Contrato B y comprobaciones

Sin componente de escena, Update, Animator, escritor de huesos ni blendshapes.
B conserva toda mandíbula/mirada/cabeza en movimiento; no requiere enlace nuevo.
Los ojos mantienen sus pesos originales. No se toca alcance de miembros, rig,
colisión ni lógica de transición. Puede aplicarse antes o después de B/C.

Hecho: lectura de captura/scan privado y código, cotejo directo de los 12.216
vértices y par de índices/UV0/pesos/índices de huesos, submesh ocular de 454
vértices, referencias actuales del renderer y revisión de diff. No compilación,
Console, ejecución del integrador, Play, escucha ni nueva captura en este checkout.
El cambio perceptible y ausencia de penetración de párpados durante la actuación
necesitan la comprobación central; miedo sigue **EN PRUEBA humana**.

Dos dudas humanas: ¿se lee una persona física que sostiene la mirada y provoca
retirada? ¿boca/párpados siguen siendo creíbles durante la entrada y el contacto?

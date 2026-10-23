# Intruso — actuación, ronda 4 · 2026-10-23

**Integrado y guardado en el Editor central. Actuación y miedo EN PRUEBA humana.**
Checks y corrección de quietud en [la entrega central](PARALLEL_INTRUDER_ROUND4_20261023.md#integración-central--2026-10-23).
Lo siguiente conserva la entrega del worktree `codex/intruder-acting-round4`, base `d50783f4`.
No se abrió Unity ni se creó Library; sin compilación, Play, escucha o build.

La captura `p3-contact-corrected.png` motivó separar carga, contacto y quietud:
las dos palmas altas permanentes y la boca sostenida daban lectura de pose.

- P3 conserva el descubrimiento de 0,32 s y avance físico de 0,46 s. Carga
  0,18 s sobre las plantas, con hombro recogido. La mano derecha llega antes;
  la izquierda entra tarde para contener el cuerpo. El golpe exige ambas
  muñecas a menos de 4,5 cm de sus targets y proximidad real al vidrio.
- Tras contacto, el cuerpo cede una vez; entre 0,16 y 0,55 s la izquierda
  abandona el cristal y baja junto a las costillas. La derecha permanece
  apoyada, el cuello recupera la mirada y a 0,68 s queda la quietud final.
  Mandíbula sólo con un pulso corto; no se sostiene abierta ni se vuelve a
  mover la cara neutral del frente A. No hay seno corporal ni ciclo de gestos.
- Los apoyos P3 siguen el avance real, con llegada temprana de un pie y
  recogida del otro. Una obstrucción no sigue animando los pies hasta el stop.
- Bomba interpola claves de carga diagonal, choque del hombro y recuperación.
  Retira la suma anterior de balanceo, compresión permanente, brazos en espejo
  y mandíbula abierta. Mantiene el solver de apoyos y las restricciones de mano.
  `Advance`, fuentes, mezcla, margen de reacción y captura quedan intactos.
- `IntruderActingFocus` es pasivo: sólo lo llama el dueño actual de pose.
  Adquiere un punto por cambio de acción; los ojos anticipan al cuello. En
  quietud, sólo un desplazamiento angular mayor a 24° tras 0,85 s permite
  readquirir. En bomba recibe la atención conocida de la amenaza, sin leer
  una posición oculta nueva. Pausa y liberación P3→bomba conservan su dueño.

## Aplicación exacta

1. Integrar el commit y esperar la importación en el único Editor central,
   con Incursion ya abierta y fuera de Play.
2. Ejecutar `IntruderActingIntegration.ValidateCurrentScene()` o menú
   `Echo Protocol > Intruder acting > 1. Validate current scene`.
3. Ejecutar `IntruderActingIntegration.ApplyToCurrentScene()` o el menú `2`.
   Agrega un componente pasivo al actor y asigna ojos, viewer P3 y las dos
   referencias de actuación. Habilita la nueva pose de Embestida. Repetir es
   no-op; una instalación parcial se rechaza. No abre ni guarda escenas,
   cambia materiales, targets, audio ni archivos privados.
4. Revisar una presentación P3 + entrada de bomba; guardar explícitamente
   sólo después del check. No hace falta reinstalar el P3 de ronda 3.

Rollback: Undo del grupo, o
`IntruderActingIntegration.RestorePreviousActing()` / menú `3` fuera de Play.
Restaura las ramas anteriores de P3/bomba, quita únicamente el componente y
sus enlaces, y conserva cara/audio de los otros frentes. Una excepción de
aplicación revierte el grupo. Las variantes anteriores permanecen en código
para este rollback explícito. No hay dependencia con componentes de A o C.

## Comprobación mínima y límites

Hecho aquí: lectura de evidencia, diff y referencias; contraste de que
`Advance`, reacción, captura y fuentes de Embestida no cambian. Sin prueba
geométrica en Unity. Los targets conservan el stop central y el izquierdo
corregido `(-6.2425, 1.40, 35.82)`; esto no certifica alcance de la nueva pose.

En el instante de contacto P3 revisar `Contacts == 1`,
`ContactRightPalmError` / `ContactLeftPalmError` < 0,045 m y `ApproachBlocked == false`.
Después, `BraceReleased` y `Settled`; `LeftPalmError` crecerá intencionalmente
al soltar la mano. `RightPalmError` y `MaxSupportError` permiten detectar un
apoyo visualmente falso. En bomba, `BreachPalmDistance` y `MaxPlantDrift`
acompañan la lectura de mano/soles; el contacto físico de Advance no se cambia.
Tras el handoff: `OwnsPose == false`, fuentes P3 detenidas y mismo actor en bomba.

Dos dudas humanas: ¿la aproximación y la caída de una mano se leen como peso
e intención, sin gesto de muñeco? ¿la mirada quieta resulta incómoda y la
irrupción provoca retroceso con orientación y volumen cómodos?

## Ajuste único tras el extremo P3 central

El coordinador informó `Contacts=1`, errores de ambas muñecas al contacto
<0,00001 m, `Settled=true`, `BraceReleased=true`, `RightPalmError=0` y
`MaxSupportError≈0,000001 m`. La captura `p3-settled.png` todavía se lee como
saludo amable: torso erguido y palma vertical. Esto es evidencia del primer
pase, no validación del ajuste siguiente.

Sólo desde 0,24 s después del contacto, la quietud carga el torso 13° más hacia
la mano, adelanta el hombro derecho 6°, baja pelvis 2 cm y recoge el mentón
con compensación cervical y ocular. La palma gira 28° dentro del plano del
vidrio, conservando su muñeca y target. Entrada, raíz, targets, pies y bomba
no cambian; el parámetro ocular nuevo tiene default cero para otros usos.
Requiere únicamente el nuevo extremo P3 central para comprobar el apoyo y
la lectura. Sin aplicación ni comprobación Unity en este worktree.

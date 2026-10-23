# Intruso — actuación y mirada, ronda 5 · 2026-10-23

**Implementado en código; pendiente de integración central y juicio humano.**
Frente C, sobre `abdbae4c`. Referencias leídas en el original:
`p3-settled-corrected.png`, `pump-contact.png`, `pump-recovery.png` de ronda 4.
No se abrió Unity, creó Library, compiló, jugó ni generó un build aquí.

- **P3:** carga de 0,12 s y contención hasta 0,26 s, seguida del avance
  existente de 0,46 s. Mano derecha transversal, dedos juntos y codo bajo:
  presión por el talón de la mano. Ambas muñecas conservan targets y postura
  de hombros al contacto. Después, la izquierda se retira entre 0,18–0,42 s;
  el torso queda inclinado 35°, con peso hacia la derecha, a 0,62 s. No gira
  la palma después de plantarla ni vuelve al torso erguido de la captura.
- **Bomba:** carga diagonal quieta durante la espera existente, liberación
  del hombro, compresión al impacto y recuperación todavía inclinada. La
  mano libre queda compacta junto a las costillas y se libera gradualmente
  al comenzar la persecución; mandíbula con esfuerzo breve y contenido.
  `Advance`, contacto sonoro, `ProtectReaction`, captura y velocidad no cambian.
- **Mirada:** calibra los ejes del rig restaurado. Ojos adquieren en 45 ms;
  cabeza gira en 140 ms después del retardo de la acción y sostiene un punto
  del mundo, con mentón bajo. Corrige la orientación del torso antes de mirar
  al punto. Una desviación mayor a 12° tras 1,05 s permite adquirir otro punto
  cuando la acción lo admite; no hay seguimiento continuo del Player.
  En bomba sólo recibe la evidencia conocida que ya suministra la amenaza.

El único opt-in es `IntruderActingFocus.containedViolence`, default `false`.
Las ramas de ronda 4 permanecen recuperables. Focus sigue siendo pasivo;
P3 y MachineThreatPerformance conservan la propiedad exclusiva del mismo rig.
Pausa, apoyos, mano real de bomba, Echo y handoff mantienen sus dueños.
No cambios de materiales, geometría, fuentes/mezcla, Staging ni puzzle.

## Upgrade central exacto

Con Incursion ya abierta, fuera de Play y después de la compilación conjunta:

1. `IntruderPerformanceIntegration.ValidateCurrentScene()` — menú
   `Echo Protocol > Intruder performance > 1. Validate round 4 installation`.
   Exige la instalación completa de IntruderActing, el actor presente exclusivo,
   sus referencias y los targets corregidos existentes.
2. `IntruderPerformanceIntegration.UpgradeCurrentScene()` — menú `2. Upgrade
   to round 5`. Sólo activa el flag con Undo. Repetir es no-op. No abre ni
   guarda escenas ni escribe assets; deja la escena dirty.
3. En el check compartido, mirar P3 al contacto/quietud y bomba al contacto/
   recuperación. Guardar explícitamente después. No ejecutar el Apply histórico
   ni generadores de producción para actualizar esta instalación.

Rollback: Undo del grupo o
`IntruderPerformanceIntegration.RestoreRound4Performance()` — menú `3. Restore
round 4 performance`. Apaga sólo el flag; conserva IntruderActing y los otros
frentes. Una excepción durante el cambio revierte su grupo.

## Comprobación y límite

Hecho aquí: revisión del diff, referencias/call sites y `git diff --check`.
Revisadas las salidas por pausa, contacto real y transferencia de pose; no
verificación runtime. No se añadieron harnesses ni checks de campaña.
Central reutiliza los observables existentes: un contacto P3, errores de ambas
muñecas <0,045 m al contacto, derecha apoyada al quedar quieto; un contacto de
bomba y mano sobre carcasa. Ver la silueta y el alcance después de la nueva
inclinación: el solver no demuestra por sí solo que el apoyo sea creíble.

**Límite humano:** una presión lateral y una mirada más directa pueden seguir
leyéndose como actuación artificial. El conjunto necesita provocar retirada
o incomodidad en juego, a volumen cómodo; estas poses no certifican miedo.

# Descubrimiento del intruso — ronda 5 · 2026-10-23

**Implementado en código; pendiente de integración y comprobación central.**
Base `abdbae4c`. Frente E: seis luces existentes y dos piezas visuales de la
escena; ningún cambio de anatomía, materiales del actor, actuación o sonido.
No se abrió Unity ni se compiló/jugó en este worktree. No builds ni harnesses.

## Diferencia concreta

La referencia es `Builds/round4-integration-20261023/p3-settled-corrected.png`,
con `pump-contact.png` y `pump-recovery.png`, inspeccionadas en el original.
El intruso quedaba delante de un rectángulo floral centrado, con relleno cálido
y una superficie frontal semejante a una mesa de recepción.

- **P3:** el respaldo floral existente pasa de 2,90 a 1,42 m de ancho, centrado
  en `z=34,78`: queda detrás del Echo y descubre el muro estructural detrás del
  intruso. El marco norte visual pasa de 6,5 a 28,5 cm de ancho, con borde
  interior en `z=36,7975`; cara y palmas (`z=35,82–36,28`) quedan libres. La
  llegada oblicua encuentra primero ese borde y luego el cuerpo. Ninguna pieza
  movida tiene collider, comportamiento o hijos. Vidrio, plinto, controles,
  posiciones del actor y blancos permanecen iguales.
- **P3, luz:** la práctica del intruso cambia al lateral norte
  `(-6,06; 2,28; 36,78)`, apunta a `(-5,76; 1,43; 35,98)`, cono 96°/58°,
  alcance 2,55 m e intensidad 1,35. El haz termina antes del fondo floral;
  la ropa oscura entra en una luz fija que alcanza cara y manos. El rebote
  cercano tiene alcance 1,85 m y ganancia 0,24 para sostener las sombras.
  La lámpara conserva su posición real y ganancia 2,8, dirigida al rincón
  doméstico/Echo. La luz del mando conserva ganancia y encendido, pero su
  cono baja de 68° a 46° y apunta exactamente al contacto del mando: menos
  relleno cálido frontal sobre el visitante.
- **Bomba:** la práctica de amenaza se reorienta desde
  `(-5,12; 2,12; -44,12)` hacia `(-4,26; 1,38; -45,20)`, cono 88°/56° y
  alcance 3,8 m. Conserva la envolvente existente 0,25→1,65. `Service / last
  practical` conserva el punto del artefacto de techo, pasa de Point a Spot,
  apunta al umbral `(-4,48; 1,22; -45,95)` y usa intensidad 2,8, cono 88°/52°,
  alcance 3,7 m. Un lavado oblicuo cálido alcanza hombro/cara al cruzar; el haz
  lateral más neutro recibe el cuerpo y la mano al llegar a la carcasa.

Son haces fijos, sin seguimiento ni flashes. Se preservan la luz clínica y su
transición inicial, la exposición, el postproceso, todas las puertas funcionales,
los carteles, el escape y las reglas de reacción/captura. La superficie frontal
de la captura no se movió porque no se identificó como utilería independiente.
No hay nuevos objetos renderizados, luces, colliders ni nuevos dueños de Update.

## Upgrade desde la instalación real de ronda 4

Aplicar el commit en el original y dejar importar junto con los otros frentes.
En la Incursion **ya abierta, fuera de Play**:

1. `Echo Protocol > Intruder reveal round 5 > 1. Validate current scene`
   — `IntruderRevealIntegration.ValidateCurrentScene()`.
2. `2. Upgrade current scene`
   — `IntruderRevealIntegration.UpgradeCurrentScene()`.
3. Comprobación central compartida: compilación/Console y una llegada de P3
   más entrada de bomba. Ver cara/manos en la quietud y antes del golpe, y
   confirmar que el marco no tape la llegada desde el ángulo real. Guardar
   explícitamente después. El integrador no abre ni guarda escenas.

El preflight requiere el recibo de ronda 4 y sus cuatro luces aún coincidentes;
no revisa ni escribe audio, huesos o referencias de mesh/material. Los upgrades
A–D pueden aplicarse antes o después. Se comprueba que las dos piezas sean
visuales, sin collider/behaviour, y se selecciona el marco norte por nombre y
posición: el marco sur tiene el mismo nombre. Apply repetido es no-op si el
recibo propio está íntegro. Una excepción revierte su grupo Undo.

`IntruderReveal / threshold and divided alcove` conserva posiciones, escalas,
orientaciones, colores, intensidades, tipos/conos/rangos/sombras y el valor de
aparición P3 anterior. Es sólo un recibo de datos, sin métodos de ejecución.
Undo restaura esta operación completa. Tras guardar/reabrir, usar
`3. Roll back current scene` — `IntruderRevealIntegration.RollBackCurrentScene()`:
restaura sólo E y conserva A–D. Una edición posterior de estas luces/piezas hace
que la retirada se rechace para no pisarla; reconciliar esa edición primero.

El recibo/integrador viejo `IntruderPresence` permanece intacto: su validación
rechazará las luces nuevas mientras E esté instalado. Para retirar también la
ronda 4, retirar E primero; luego seguir el rollback de sonido si procede.
No reejecutar generadores históricos ni `IntruderPresence.Apply` como upgrade.

## Evidencia y límite

Inspección estática de las capturas reales, referencias/transformaciones de la
escena guardada, dueños de las luces y diff. `git diff --check` es el check local;
compilación y render pertenecen al coordinador. La nueva composición y valores
son una propuesta implementada, todavía sin imagen posterior.

**EN PRUEBA humana:** que descubrir el cuerpo y luego su intención provoque
retirada, y que la cara siga legible con la nueva superficie oscura. Un cambio
visible no certifica miedo; la iluminación real depende de A–C y del ángulo del
Player. Si falla esa lectura, corregir sólo el haz/pieza causante tras la vista
central, sin ampliar la ronda a una campaña de pruebas.

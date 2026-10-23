# P3: aparición, borde y contacto — ronda 6

2026-10-23 · Frente C · base `1e4e3a1a` (juego R5 `e393c391`).
**Implementado en código; integración, render y escucha pendientes.**

Se contrastaron `p3-settled.png`, `pump-contact.png` y `pump-recovery.png` de
`Builds/round5-integration-20261023/` en el original. P3 todavía presenta una
palma ofrecida sobre un primer plano horizontal; la bomba ya tiene más cercanía.
A/B cambian cara y actuación. C sostiene ese cambio con una única puesta:

- El rectángulo floral detrás de los cuerpos se reutiliza como mejilla corta
  del marco norte: centro `(-5.88,1.61,36.96)`, giro Y90°, tamaño local
  `(.024,2.92,.56)`, material existente del jamb. Ocupa x `[-6.16,-5.60]` y
  z `[36.948,36.972]`. Descubrimiento oblicuo con borde profundo, sin fondo
  de retrato ni pieza transversal en el eje del Echo (`x≈-5.05`). No tiene
  collider; Staging consulta su caja visual para respetar la oclusión real.
- **El jamb de B permanece intacto:** centro `(-6.3,1.76,36.94)`, escala
  `(.28,1.98,.285)`, esquina de agarre x `-6.16`, z `36.7975`. La mejilla queda
  detrás de esa esquina; rostro próximo y dedos visibles por el lado sur.
  Contrato final B: stop `(-5.72,.03,36.34)`, muñeca `(-6.12,1.30,≈36.89)`,
  dedos hacia -Z; retención `.34 s`, suelta hasta `.44 s`, quietud en `.58 s`.
- Dos luces existentes: la key P3 pasa a `(-6.05,2.42,36.78)` y apunta a
  `(-5.94,1.42,36.49)`, cono `104°/56°`, alcance `2.55 m`. Conserva intensidad
  `1.35` y su dueño. El cuerpo entra en ese haz fijo; no hay flash ni luz que
  siga la cabeza. El rebote pasa de `.24` a `.31` para sostener órbita y mejilla.
  Lámpara, control, exposición y luces de bomba permanecen iguales.
- El contacto P3 reutiliza `IntruderDread_DryContact.wav`: un golpe amortiguado
  del marco de **0.17 s**, con el gain existente. B conserva el único `Play`.
  Presence observa `P3Visible`, `OwnsPose`, `HasContact`, `Contacts` y la fuente;
  cuando acaba ese contacto real, reproduce una sola recuperación de **0.42 s**.
  Luego silencio corporal. Sin voz extra, loop ni reloj de ataque nuevo.
  La cesión de pose/capítulo cancela la frase; el contador anterior no la rearma.

No se modifican raíz/huesos/targets, vidrio, puertas, controles, progreso ni
registro. La superficie horizontal funcional no se mueve: quitar su lectura
social depende también del agarre y proximidad de B. No se oculta la cara final
para disimular su anatomía. WorldSFX, ganancia maestra, limitador, mono y dinámica
reducida conservan sus dueños. Sin assets de audio nuevos ni derivados privados.

## Integración y retirada

En Incursion **ya abierta y fuera de Play**, después de importar el commit:

1. `Echo Protocol > P3 rupture reveal round 6 > 1. Validate current scene`
   — `P3RuptureRevealIntegration.ValidateCurrentScene()`.
2. `2. Upgrade current scene`
   — `P3RuptureRevealIntegration.UpgradeCurrentScene()`.
3. Comprobación central conjunta de compilación/Console y un punto P3 de
   mano/cara/contacto/aliento; guardar explícitamente después. No otro recorrido.

Puede aplicarse antes o después de A/B. El preflight verifica la instalación R5,
dos luces/propios valores, pieza visual, jamb reservado, fuente y breath owner.
No ejecuta instaladores históricos ni abre/guarda escenas. Repetir Upgrade es
no-op. Una excepción revierte su grupo Undo. El recibo `P3Rupture / short reveal
and physical tail` conserva las dos luces, transformación/materiales del panel
y clip anterior. Los recibos R5 no se reescriben.

Undo revierte la operación; después de guardar/reabrir:
`3. Roll back current scene` — `P3RuptureRevealIntegration.RollBackCurrentScene()`.
Restaura sólo C a R5 y quita sus enlaces opt-in; A/B quedan instalados. Retirar
C antes del reveal R5. Una edición posterior de lo que C escribió bloquea el
rollback para conservarla. El coordinador decide cómo reconciliarla.

## Check y límite

Check local: diff, lectura de referencias/propiedades y cálculo del haz fijo.
El agarre acordado cae a `20.63°/1.13 m` y el proxy de cara próxima a
`5.81°/1.02 m`, dentro del cono interior de 28°. Eso **no verifica el render**.
Hashes/mono/duraciones de los dos masters existentes coinciden con el manifiesto
R5; se leyeron desde el original porque este worktree conserva punteros LFS.
No se abrió Unity ni se compiló/jugó, construyó o publicó. Sin harness nuevo.

**EN PRUEBA humana:** que el agarre, la cara descubierta y la quietud cercana
rompan la lectura cotidiana; timbre y comodidad a volumen cómodo. Un solo punto
central debe comprobar que la mejilla no tape dedos/cara ni el Echo. El salto
de miedo no se deduce de estos checks.

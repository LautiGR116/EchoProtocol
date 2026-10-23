# P3 — presa, desgarro y quietud · ronda 6 · 2026-10-23

**Implementado; pendiente de compilación, integración y plano central. Miedo EN PRUEBA.**
Frente B sobre R5 `e393c391`. Se contrastaron `p3-settled.png`, `pump-contact.png`
y `pump-recovery.png` del original: la palma abierta de P3 leía como saludo.

R6 usa el jamb norte existente `A sealed window jamb`, Transform `483625452`,
centro `(-6.3,1.76,36.94)`, escala `(.28,1.98,.285)`. **C conserva esta pieza.**
La palma derecha carga su cara este; cuatro dedos rodean la esquina interior
`x=-6.16,z=36.7975` hacia su cara sur y el pulgar se opone. La izquierda queda
recogida a las costillas. No se agrega un collider ni se mueve el marco.

- Reconocimiento existente: 0,32 s de vista con el Echo real en A. Carga: 0,20 s.
  Avance con el CharacterController existente: 0,31 s hasta
  `(-5.72,.03,36.34)`. Conserva la salida inicial en X para evitar la lámpara.
- Contacto único: raíz próxima al stop, muñeca alcanzada y **cuatro puntas** junto
  a la superficie real del marco. Ese hecho dispara las dos fuentes existentes;
  C decide sus clips/mezcla. No hay sonido de apoyo previo ni segundo reloj R6.
- Retención hasta 0,34 s tras el contacto; liberación en 0,34–0,44 s, conservando
  la forma de presa al retirar el brazo. El torso termina de acercarse a 0,58 s.
  Cabeza/mirada quedan fijas, sin balanceo, nods ni readquisición periódica.
  Un margen de 0,16 m del centro de cabeza al vidrio limita la inclinación.
- Mandíbula y comisuras mantienen el neutral del rig: R6 P3 no agrega offsets
  que cierren o sonrían sobre la apertura geométrica de A. Bomba sigue en R5.

`Present/OwnsPose/HasContact/Contacts/ReleasePose` conservan su API y autoridad.
No hay Update nuevo, nuevos eventos Echo, daño, captura, cámara ni bloqueo de
controles. RECALL no rearma el contacto; pausa no adelanta tiempos. La transferencia
restaura articulaciones sin restaurar la raíz mundial que bomba ya colocó.
En R6 **LeftPalmError/ContactLeftPalmError no son criterios de contacto**: la mano
izquierda no toca el vidrio. `BraceReleased` indica la suelta derecha a 0,44 s.

## Operación central exacta

Con Incursion ya abierta, fuera de Play, tras importar/compilar los tres commits:

1. `P3RupturePerformanceIntegration.ValidateCurrentScene()` — menú
   `Echo Protocol > P3 rupture performance > 1. Validate current scene`.
2. `P3RupturePerformanceIntegration.UpgradeCurrentScene()` — menú
   `2. Upgrade from round 5`. Opt-in sólo P3; repetir es no-op.
3. Revisar el único contacto y la suelta en el checkpoint compartido. Guardar
   explícitamente después; el integrador no abre ni guarda escenas.

Rollback: Undo del grupo o `P3RupturePerformanceIntegration.RestoreRound5()`
(menú `3. Restore round 5`). Su recibo propio restaura exactamente stop y target
derecho anteriores y apaga el perfil; conserva A/C, fuentes, bomba y recibos R5.
El preflight rechaza instalación parcial/anchors modificados; una excepción
durante la operación revierte el grupo. No reejecutar instaladores históricos:
sus validadores fijan las coordenadas R5 y no conocen este upgrade.

## Comprobación y límite

Aquí: diff y `git diff --check`; cálculo focal con el rig/escena central leídos.
Muñeca derivada: `(-6.12,1.30,36.881551)`. Las cuatro puntas quedan a 0,012 m de
la cara sur del jamb; sus ejes conservan al menos 0,05596 m al vidrio. Stop deja
0,035 m sobre radio+skin frente al plinto; el trayecto no reduce el despeje inicial
R5 a la lámpara. El cálculo no sustituye colisión real ni volumen de piel en Play.
Sin Editor, Library, compilación, escucha, Play ni builds en este worktree.

**Límite humano:** sólo la vista central puede confirmar que el marco parece
recibir peso y que la liberación/cara próxima provoca retirada, en lugar de parecer
una pose de dedos. No se declara miedo ni actuación aprobados por estas medidas.

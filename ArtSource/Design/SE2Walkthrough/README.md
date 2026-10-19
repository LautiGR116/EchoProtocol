> Nota de integración: este texto conserva la propuesta inicial y contiene
> dimensiones/contratos anteriores. El autor vigente es
> `Assets/Editor/SE2WalkthroughProduction.cs`, seguido de
> `Assets/Editor/WalkthroughFinishProduction.cs`; el runtime está en
> `VS1ParallelFigureEvent`. Se integró una sucesora de Courtyard y se revisa
> en `Docs/VisualEvidence/2026-10-01-Walkthrough/`. Los archivos de plano/JSON
> citados como propuesta abajo no llegaron a producirse. Esta receta no es
> evidencia de escena, colisión, presentación ni partida humana.

# SE2 transitable — receta de primera cámara

2026-10-01. **Dirección en preparación, sin autoría Unity ni prueba de este
layout.** Fuente estable leída: `EchoProtocol_Observation.unity`; Courtyard sólo
modifica el tramo doméstico/final. No integrar a esa escena: sucesor independiente
de Courtyard, propuesto `EchoProtocol_Walkthrough.unity`, clave propia
`EP.CampaignCheckpoint.Walkthrough.v1`. Parent integra y decide en cámara.

## Decisión

Conservar la vista inicial de un cuarto clínico a través del paño frontal;
al rodear su mocheta, entrar físicamente en **ese mismo salón**, ahora doméstico,
y salir por el vano lateral norte. Suprimir la lectura quieta obligatoria de
3.2 s. No añadir E, bloqueo, voz, toma ni canal de Echo. Mirar a la figura sigue
siendo voluntario; está en la estancia posterior, fuera del recorrido.

La arquitectura común nunca cambia sus colliders, puertas, suelo o techo durante
la marcha. La presentación y el progreso son estados independientes: si una
condición conservadora de ocultación impide el cambio visual, la marcha aún puede
completar el episodio. No se fuerza la cámara ni se revela al desactivar el script.

## Hallazgo que obliga a reconstruir el cascarón

La escena actual **no tiene una huella navegable común**:

| Estado actual | Límites X / Z | Altura interior |
|---|---|---|
| Frío | x8..11.3 / z26.6..31.6 | 3.1 m bajo techo |
| Doméstico | x8..16 / z27..31.2 | 2.72 m |

Los suelos/paredes de ambos son renderers sin collider. El vidrio y sus bajos
mantienen al Player en el corredor. Retirarlo solo dejaría acceso a escenografía
sin soporte físico. Además, `SetDomesticPresentation` conmuta roots completos,
incluidas luces; `OnDisable` actualmente deriva la presentación de `Complete`.

`existing-geometry.json` y `read_geometry.py` documentan transforms/cajas activos
leídos del YAML; **no** editan la escena. El lector no resuelve las mallas internas
de prefabs y sus AABB no son una ejecución de Physics. Dimensiones de muebles se
contrastaron con `ArtSource/Blender/Overhaul/manifest.json`.

## Plano propuesto, metros mundo

Trasladar rígidamente la cámara SE2 **−4X**: root de (8,0,29.1), yaw90, a
**(4,0,29.1), yaw90**. La estancia doméstica conserva profundidad/anchura; se
reconstruye el frío contra el nuevo cascarón. `plan.svg` y `layout.json` contienen
la primera composición; no son una perspectiva validada.

| Elemento | Posición / huella nueva |
|---|---|
| Salón físicamente accesible | x4.10..7.59, z27.08..31.12; suelo y0 |
| Estancia posterior, inaccesible | x7.77..11.93, z27.08..31.12 |
| Frente / vidrio de observación | x4; conservar el vidrio, postes, sill/header y marcas, traslado rígido |
| Partición posterior | x7.68; abertura z28.28..29.40, header inferior y2.22 |
| Vano de entrada sur | z27, x4.20..5.60; altura libre2.20 |
| Vano lateral norte | z31.20, x5.80..7.20; altura libre2.20 |
| Figura actual | (9.18,0,28.90), mismo modelo/materiales/rotación |
| Sillón, orientación propuesta | (4.95,0,30.40), frente mundo+X; collider permanente aprox1.00×1.12×1.00 |
| Consola | (6.50,0,27.33), orientación actual conservada; collider .82×.85×.42 |
| Lámpara de pie | (4.55,0,29.42), orientar cordón al muro x4; reubicar fuente/anchor junto con el modelo |
| Cruce SE3 | **sin mover** (−1.55,1.1,29.1), yaw270, size(2,2.3,2) |

Para conservar distancia del cuerpo rígido: en la abertura posterior reutilizar
el vidrio/marco como **paño fijo con collider coincidente**. No usar una pared
invisible delante de un vano abierto. El borde delantero de la figura permanece
a más de1.2 m detrás del paño; desde el eje de marcha está a unos2.7 m. Ésta es
una modificación arquitectónica explícita, pendiente de selección en cámara.
El frío tiene una chapa opaca visual sobre ese mismo plano; al revelarse se ve la
estancia profunda, sin cambiar su colisión. No hay interacción con este cierre.

El frío delantero deja de ser un volumen vacío incompatible con los muebles:
una caja de servicio baja y un pequeño mueble metálico cubren exactamente los
colliders constantes del sillón/consola. Son cajas biseladas reutilizables, no
objetos interactuables. La lámpara puede conservar geometría en ambos estados.
No mantener un collider de sillón invisible en una zona fría explorada.

### Corredores y ocultación

El pasillo P2 este se acorta hasta x4. Su suelo/techo quedan x[−1.45,4],
z[28,30.2]. Muros sur z28 y norte z30.2 abren el mismo hueco **x[1.55,3.05]**;
mantienen todo el resto de sus tramos. El ancho bruto de1.50 m admite jambas de
.05 por lado y sigue dejando1.40 m; CharacterController actual tiene radio.32 m.

- Vestíbulo sur: franja oeste x[1.5,3.9], z[25,28.1], más franja de acceso
  x[3.9,5.7], z[25,27]. Mocheta sólida x[3.70,4.07], z[25.95,28.10], y[0,2.90].
  Su extremo obliga a doblar por z25.55; no roza el cuerpo del Player.
- Vestíbulo norte: franja oeste x[1.5,3.9], z[30.1,31.2], más franja lateral
  x[1.5,7.3], z[31.2,33.45]. Mocheta x[3.70,4.07], z[30.10,32.25], y[0,2.90].
  El giro se hace por z32.65. Cerrar norte/este/oeste con pared visible.
- Techo fijo en salón a2.72 m. Vestíbulos cerrados a2.9 m. Ningún techo cambia
  al revelarse; render interior frío/doméstico usa el mismo plano sin z-fighting.
- Las dos mochetas se integran a paredes, no se presentan como mamparas sueltas.
  Retirar zócalos/renderers originales que atraviesen los nuevos vanos; no basta
  partir únicamente sus colliders.

Pockets candidatos de cambio: sur x[1.85,3.35], z[25.55,25.80]; norte
x[1.85,3.35], z[32.25,32.45], y[.2,2.2]. Están en los codos, antes de ver el
salón desde cualquiera de los sentidos. `check_layout.py` sólo hace un muestreo
analítico de rayos2D a cajas y distancias; **no prueba** visibilidad de meshes,
sombras, manos, altura de ojo alternativa ni estabilidad de un frame Unity.
El cambio se difiere si la cámara puede ver cualquiera de sus superficies.

### Recorrido de primera cámara (pies X,Z)

`(−2.75,29.10) → (2.30,29.10) → (2.30,25.55) → (4.90,25.55)`
`→ (4.90,28.70) → (6.50,28.90) → (6.50,32.65) → (2.30,32.65)`
`→ (2.30,29.10) → (−2.75,29.10)`

En (2.30,29.10), yaw90, el frío se ve delante y el giro sur queda lateral;
ninguna mirada fija es requisito. En (4.90,28.70), el giro hacia el este pone el
vano posterior y la figura en la dirección de marcha. En (6.50,28.90), seguir
hacia el vano norte. Al volver al corredor se atraviesa exactamente el plano
SE3 hacia el oeste; se mantiene el codo a R1 y el Echo real de P2.

Longitud ideal **32.51 m**, frente a16.50 m de la ida/vuelta anterior. A4 m/s:
8.13 s frente a4.13 s +.65 s de calificación +3.2 s de lectura =7.98 s. Saldo
ideal≈+.15 s; no incluye giros, vacilación, cámaras ni partida humana. Es una
estimación de coste, no una exigencia de velocidad ni prueba de ritmo.
La toma P2 no se alarga artificialmente: puede terminar y desactivar su Echo
durante la exploración. Conservar ese hecho y comprobar el margen del recorrido
natural; no congelar su último frame para asegurar SE3.

## Presentación, luz y sonido

Primer candidato conservador: **fuentes del cuarto constantes**, separadas de
los roots que cambian. El montaje frío conserva el práctico de techo; la lámpara
de pie existe con acabado austero antes del cambio. Reutilizar los cuatro Light
actuales, sus fuentes físicas y PracticalEmission; no conmutar luz/ganancia al
cambiar renderers. Esto evita que una luz que pierda su mapa de sombras en
`LightShadowBudget` delate el intercambio a través de la pared del codo.

No conservar automáticamente el look naranja anterior: la cámara debe seleccionar
el balance frío/doméstico con esa iluminación común. Si se desea una transición
lumínica posterior, exige primero un límite local comprobable; el presupuesto
global de cuatro sombras no basta para afirmar ocultación. La figura ambiental
no proyecta sombras fuera del recinto y no tiene luces hijas. Nada toca Echo.

Los prácticos clínicos del approach no viajan a posiciones coincidentes: después
del traslado, fijar `SE2 Clinical approach pool/practical` a x0, z29.1 y
`SE2 Clinical barrier pool/practical` a x2.7, z29.1, con sus alturas actuales.
No crear fuentes sonoras nuevas. La diferencia al entrar puede venir del contacto
real del Player sobre madera. Para que siga el estado visual, una zona de madera
hija del root doméstico requiere que `PlayerFootsteps` respete
`enabled && activeInHierarchy` y prueba local del BoxCollider; actualmente sólo
lee `bounds.Contains`. No añadir pasos al Echo ni reproducir muebles no grabados.

## Autoría y referencias que deben conservarse

- Mover el root `VS1 SE2 Impossible Domestic Chamber` completo; reubicar luego los
  dos prácticos de aproximación como arriba. Mantener identidades de componente,
  `loopController`, `alignmentRelay`, `playerView`, lookTarget y visibilityGlass.
- Los nuevos colliders viven bajo un root permanente independiente de frío y
  doméstico. Revestimientos y modelos de estado no poseen colisión mutable.
- Rehacer los cuatro tramos de pared con vanos, suelo/techo P2 este y shell SE2.
  No activar la cámara paralela SE2 antigua: sus objetos existen pero están
  inactivos. No invadir P2 North Leg East Occluder ni el corredor en x−2.75.
- Mantener P2 reader/trigger/puerta/audio/resetTargets y poses de grabación.
  Mantener SE3 returnVolume, presentación, referencias y R1 gate/checkpoint.
- Armar SE2 con su condición P2 real habitual, mediante trigger nuevo
  center(1.55,1.1,29.1), size(4.7,2.2,2). No exigir que Echo siga sosteniendo P2
  durante todo el recorrido después de aceptar el cruce.
- Documentar el traslado de Light y modelo juntos, incluido `__BULB_SOURCE`,
  PracticalEmission y fixture. Ningún source de audio se recoloca por esta receta.

## Runtime mínimo, aún no integrado

`staged/RUNTIME_CONTRACT.md` fija la rama serializada `usePhysicalTraversal=false`
por defecto. El contrato legacy sigue exacto cuando false; SE3 consume el mismo
enum Complete. La nueva rama distingue `HasPresentedDomestic` de Complete,
vigila marcha real/pausa/recall y nunca escribe presentación en OnDisable.

No entrego un autor ejecutable todavía: falta verificar en la primera escena la
huella importada del sillón/lámpara, el plano interno sellado y la ocultación de
los codos, especialmente los renderers/luces que no expresa el lector YAML.
`layout.json` fija el candidato para implementar sin seguir ajustando a píxeles
offline. Un prototipo de cámara es la siguiente evaluación; los estados, físicas
y cambios de runtime anteriores son requisitos, no evidencia de que ya funcionen.

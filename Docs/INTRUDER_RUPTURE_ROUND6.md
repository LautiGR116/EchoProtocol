# IntruderRupture — R6 / frente A · 2026-10-23

**Implementado en código; pendiente de compilación e inspección central.**
No se ejecutó Unity ni Play en este worktree. Miedo y credibilidad **EN PRUEBA**.

Las referencias R5 `p3-settled.png`, `pump-contact.png`, `pump-recovery.png`
se leyeron en el original. Conservan una curva de boca amable incluso cerca;
esta alternativa cambia la abertura y el contorno inferior, no sólo la palidez.

## Cambio

- Abertura de reposo de aproximadamente **37 × 65 mm**, estrecha y vertical.
  Comisuras al centro de la abertura; labio inferior y mentón descendidos,
  mandíbula desviada hacia un lado. La transición al cuello termina antes de
  la ropa. Cráneo, órbitas/ojos R5 y resto del cuerpo quedan en sus posiciones.
- Se recupera el borde oral real de 26 vértices por conectividad. El labio
  superior se identifica siguiendo la piel más allá del repliegue del scan:
  una división por altura cruzaría los labios cerrados. La deformación se
  distribuye por mejilla/mentón/cuello, conservando sus UV y pesos originales.
- Dentición original conservada, estrechada y separada con cada arcada. Se
  sustituye la bolsa oral cerrada por seis anillos unidos al perímetro, con
  labio de entrada, pared, paladar/suelo elevado y fondo a unos 54 mm de profundidad.
  Añade **156 vértices / 284 triángulos**, en un tercer submesh del mismo renderer.
  El borde comparte exactamente posición/pesos con la piel: no es una máscara.
- Material oral con detalle del scan en el borde, transición húmeda a mucosa,
  relieve geométrico y oclusión progresiva. Sin emisión ni negro plano frontal.
  Piel R6 privada; originales Skin/Eyes R5 intactos. `_Apron` continúa **fuera**
  de `UnityPerMaterial`; no se revierte la corrección Metal/SRP.
- Sin cambio sonoro. Mixer/master/limitador/mono/dinámica reducida intactos.

## Integración exacta desde R5

Con el commit importado y **la escena Incursion existente abierta**, fuera de Play:

1. `Echo Protocol > Intruder rupture > 1. Validate current scene`
   (`IntruderRuptureIntegration.ValidateCurrentScene()`). Exige el recibo/malla R5
   exactos, fuentes guardadas, dos materiales y el único actor presente.
2. `Echo Protocol > Intruder rupture > 2. Upgrade current scene to R6`
   (`IntruderRuptureIntegration.UpgradeCurrentScene()`). Crea backups/variante,
   clona Skin y copia la nueva malla **dentro del mismo Face.asset/GUID**.
3. El coordinador inspecciona el punto P3 y una vista breve de bomba ya previstos;
   guarda la escena explícitamente. El integrador guarda sus assets privados,
   pero **nunca abre ni guarda escenas**. Reaplicar es idempotente.

Rollback: `Echo Protocol > Intruder rupture > 3. Roll back current scene to R5`
(`IntruderRuptureIntegration.RollbackCurrentScene()`), luego guardar la escena
explícitamente. Restaura malla/bounds/materiales R5 desde su snapshot; admite Undo
y recuperación de estados conocidos tras recargar una escena no guardada.
No ejecutar instaladores históricos mientras R6 esté aplicado. No se cambió
`IntruderFaceGeometry.cs`, su hash ni los recibos R4/R5. Un generador R6 cambiado
requiere rollback y archivar su carpeta privada antes de regenerar expresamente.

Derivados Renderpeople, copias y recibo se generan sólo al aplicar en
`Assets/_Game/Art/Embodiment/RenderpeoplePrivate/IntruderRupture/` (ignorado):
`BackupR5.asset`, `VariantR6.asset`, `Installation.asset`, `Skin.mat`, `Mouth.mat`.
Los scripts/shaders son autoría del proyecto; no se publica geometría/textura
licenciada ni se copia/modifica el original desde este worktree.

## Contrato y comprobación acotada

B: **jaw/mouthCorners neutrales en P3**; la apertura ya está en reposo. No hay
escritor de huesos nuevo. En bomba se conserva la actuación R5. Envolvente de
articulación prevista: **0 a +7° de apertura adicional**, sin cierre ni elevación
adicional de comisuras; corresponde a los pulsos contenidos de bomba (3° + 4°).
Es un límite de autoría conservador, pendiente de la vista central, no una
certificación de intersecciones en Play. La unión piel/cavidad usa los mismos
pesos en ambos lados y permanece continua al articular.

Check propio: lectura de las tres imágenes; cálculo sobre streams privados R5
(sólo lectura) confirma un borde cerrado de 26 vértices, 1.073 vértices de dientes,
las dimensiones y el coste añadidos. La revisión corrigió la identificación del
labio superior siguiendo cuatro anillos de piel. Hash del generador R5 coincide
con el recibo; diff sin errores de whitespace; revisión de contratos C#/materiales.
Sin compilación, render del resultado ni Play afirmados; sin harnesses/builds.

**Límite humano:** una boca mayor puede leerse como grito o deformación de modelo.
La inspección central debe decidir si conserva tejido creíble y si rompe la
lectura social a distancia real; sólo una persona jugando puede juzgar miedo.

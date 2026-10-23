# Intruso — anatomía contenida, ronda 5 · 2026-10-23

**Implementado en fuentes; integración/compilación central pendientes. Miedo EN
PRUEBA humana.** Frente A, sin Editor, Library, builds ni escritura en el original.
Se inspeccionaron las capturas reales P3 final, contacto y recuperación de bomba
de ronda 4, su entrega y el scan privado existente.

La decisión principal es una mandíbula fuera del eje nasal con el masetero de
scan-X negativo cargado. Una transformación regional continua gira el volumen
inferior 6,5° alrededor de su apoyo posterior; el mentón se desplaza unos 12 mm.
La boca acompaña ese volumen: se comprime el cierre real del scan (y≈1,650 m)
y se sustituye su arco por comisuras descendentes, unos 9 mm bajo ronda 4 en los
puntos inspeccionados. El pase anterior concentraba su corrección más arriba,
cerca del filtrum. No se aumentaron todos los hundimientos anteriores.

La órbita del lado cargado queda más estrecha; la otra mantiene mayor apertura.
Los párpados deslizan alrededor del globo conservando la profundidad radial de
cada capa del scan, incluida la cara interna del borde. El cuello superior
continúa esa carga. Cráneo, volumen de mejillas y los dos ojos se conservan;
no se añade una mandíbula permanentemente abierta ni una máscara superficial.

Se conservan topología, UV0, bindposes, pesos y el cuerpo fuera de cabeza/cuello.
UV2 (canal 2 de Mesh, TEXCOORD2 del shader) recibe las posiciones originales del
scan. Las normales/tangentes importadas se transforman sin dividir costuras.
No hay materiales, blendshapes, componentes de escena ni escritores de huesos
nuevos: B posee superficie y C toda la actuación.

## Upgrade exacto sobre la ronda 4 instalada

1. Integrar las fuentes y terminar la compilación conjunta. Usar **Incursion ya
   abierta, fuera de Play**. No ejecutar `EmbestidaProduction` ni el Apply antiguo.
2. Ejecutar `IntruderAnatomyIntegration.ValidateCurrentScene()` o **Echo Protocol >
   Intruder anatomy > 1. Validate current scene**. Exige el actor presente único,
   la referencia `Face.asset` instalada y el recibo R4, con scan/topología/rig
   compatibles. Rechaza una instalación parcial o cambios de mesh desconocidos.
3. Ejecutar `IntruderAnatomyIntegration.UpgradeCurrentScene()` o **2. Upgrade current
   scene to R5**. Primero guarda `BackupR4.asset`, `VariantR5.asset` e
   `Installation.asset` bajo
   `Assets/_Game/Art/Embodiment/RenderpeoplePrivate/IntruderAnatomy/` (ignorado).
   Luego copia la geometría R5 dentro del **mismo objeto/GUID `IntruderFace/Face.asset`**.
   Conserva `sharedMesh`, materiales, huesos, transforms y el recibo R4. Sólo ajusta
   bounds del renderer. Guarda únicamente sus assets; **no abre ni guarda escena**.
4. Durante el check compartido, comparar el arco de boca, eje mentón/nariz y
   apertura desigual de ojos en P3 y durante entrada/contacto de bomba. El
   coordinador guarda la escena explícitamente si conserva el resultado.

El upgrade repetido es no-op de geometría. A y B pueden aplicarse en cualquier
orden: no se validan ni escriben los materiales. Los bounds dinámicos de
`updateWhenOffscreen` no bloquean el recibo.

**Recuperación:** Undo restaura mesh/bounds en memoria. Para rollback persistente,
usar `IntruderAnatomyIntegration.RollbackCurrentScene()` o **3. Roll back current
scene to R4**; copia el backup dentro del mismo `Face.asset` y lo guarda, incluso
tras recargar el Editor. Después de un Undo, ese menú también persiste el mesh
R4 conocido. No retira el trabajo de B/C. Los snapshots quedan para reaplicar.
Si se edita de nuevo el generador, hacer primero Rollback y archivar fuera de
`Assets` la carpeta privada `IntruderAnatomy` y su `.meta`; Upgrade creará una
instalación nueva con respaldo, sin sobrescribir el anterior. Una excepción
revierte el mesh y sólo retira los assets creados por esa operación.

## Comprobación propia y límite

Diff y contratos revisados. Inspección aritmética puntual sobre las posiciones
privadas, leyendo el original sin copiar sus medios: cuerpo y ojos preservados;
0 de 5.424 triángulos de cabeza con normal geométrica invertida respecto del scan
tras corregir la proyección del borde orbital. Es una comprobación de la
transformación, **no ejecución del C# ni compilación, Console o Play en Unity**.
No hay derivados licenciados ni datos de scan en el commit.

La limitación pendiente es perceptual: una boca más tensa y una cara asimétrica
pueden resultar extrañas sin provocar retirada. Su lectura a distancia y la
credibilidad de labios/párpados con la actuación de C deben juzgarse en el Editor;
el miedo sólo se establece con la prueba humana.

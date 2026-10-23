# Intruso — superficie, ronda 5 · 2026-10-23

**Implementado en código; pendiente de importar y comprobar en el Editor central.**
Frente B. Base integrada `d2ea5398`, checkout del plan `abdbae4c`; no se abrió Unity
ni se generó Library/build en este worktree. Se inspeccionaron las tres capturas
reales de ronda 4 y el atlas original
en el proyecto central, sólo en lectura. No hay nuevo playtest ni aprobación de miedo.

La camiseta clara, los listones del delantal y la piel cálida de esas capturas
mantienen una lectura cotidiana. La alternativa usa un torso de trabajo azul
carbón y goma oscura, con bordes ocre gastados, para separar la cara y las manos
del volumen vestido. El desgaste responde a bordes, rodillas y contacto; la
humedad a cuello, axilas, puños y parte baja del delantal. Se elimina el dibujo
alternado de franjas del acabado anterior; los pliegues siguen siendo geometría.

La piel conserva poros, barba, pelo y variación fotográfica: mejillas/mandíbula
más pálidas y secas, labios oscuros contenidos, humedad pequeña en nariz, borde
inferior de párpados y una comisura. No hay círculos negros, sangre añadida ni
heridas pintadas. Los ojos conservan iris/pupila del scan, esclerótica marfil
apagada y reflejo corneal físico. No emiten luz ni reciben reflejos dibujados.
No se modifica ninguna textura licenciada, malla, UV, hueso, animación o Echo.

## Contrato de materiales

- `EmbestidaSurface` añade `_IntruderDreadSurface`, **0 por defecto**. La ruta
  anterior queda intacta con ese valor; `_IntruderFace` sigue en 1 en la piel.
- `IntruderDreadSurface.hlsl` usa el scan original en `TEXCOORD2` (canal UV 2,
  metros) para las regiones y UV0 para los bordes de la prenda. El sculpt A
  conserva ese canal; las regiones no se desplazan por el mundo al actuar.
- `EchoProtocol/IntruderDreadEyes` conserva atlas/normal y sus coordenadas; el
  upgrade activa la variante, normal 0 y smoothness 0,78. La luz local determina
  el reflejo. C sigue siendo el único escritor de los huesos de mirada.
- El upgrade conserva los objetos/GUID de `IntruderFace/Skin.mat`, `Eyes.mat` y
  `Embestida/RubberBib.mat`. No reemplaza el array del renderer ni escribe el
  recibo de ronda 4. Respalda materiales privados completos, incluido su shader,
  en `RenderpeoplePrivate/IntruderDreadSurface/`, excluido de Git.

## Upgrade desde la ronda 4 guardada

1. Integrar este commit con los demás frentes, dejar terminar la importación y
   revisar una vez compilación/Console en el Editor central. Abrir manualmente
   Incursion, fuera de Play. No ejecutar `EmbestidaProduction` ni el Apply antiguo.
2. Menú **Echo Protocol > Intruder surface > 1. Validate current scene**.
   Preflight: único intruso presente, materiales privados esperados, texturas,
   shaders y UV2; rechaza materiales usados por otro renderer de escenas cargadas.
3. **2. Upgrade current scene**. Guarda respaldos privados y modifica los tres
   materiales en su sitio. No abre ni guarda escenas, no altera referencias,
   mallas, bounds ni huesos. La repetición válida es no-op. Es independiente del
   orden A/B porque no depende de la identidad del mesh facial.
4. Con la comprobación compartida de P3 y entrada/contacto de bomba, comparar con
   `p3-settled-corrected.png`: cara/manos deben distinguirse del traje, los pliegues
   de goma deben responder a luz, y los ojos seguir siendo ojos humanos.
   No hace falta repetir una ruta completa. Guardar la escena central manualmente
   cuando el coordinador haya revisado el conjunto.

**Recuperación:** Undo revierte la operación; **3. Roll back current scene** restaura
el contenido completo de los materiales anteriores y funciona después de recargar
el Editor. Conserva backups para reaplicar. No revierte la anatomía/actuación de
otros frentes. Para volver sólo al acabado corporal anterior durante comparación,
poner `_IntruderDreadSurface=0` en piel y delantal; el rollback completo también
restaura el shader URP de ojos. Restablecer esos dos valores a 1 antes de usar los
menús: los cambios manuales después del upgrade se detectan y no se sobrescriben
automáticamente.

## Comprobación y límite

Revisión de diff/referencias, `git diff --check`, revisión de la rama opt-in y del
contrato URP local. No se afirma compilación de C#/shader, Play ni evaluación visual
del resultado integrado desde este worktree. No se añadieron tests ni harnesses.

**Límite humano:** la lectura de piel agotada, peso del traje y mirada incómoda
depende de la luz y actuación conjuntas. La saturación oscura puede perder pliegues
en la bomba; el check central debe comprobar legibilidad. Incluso un acabado
claramente distinto puede no producir retirada o miedo en el jugador.

### Corrección del fallo de render central

El coordinador encontró cuerpo y delantal invisibles tras la integración, con ojos
visibles y sin errores de shader reportados. Su aislamiento en Play pausado cambió
sólo `_IntruderDreadSurface` a 0 en instancias temporales: ambos reaparecieron.
Esto localiza el fallo en la nueva ruta de superficie, no en anatomía/bounds ni en
la transparencia guardada. No identifica por sí solo la instrucción GPU causante.

El parche limita el exponente de las regiones al rango normal de float, elimina
la microvariación con `fwidth` dentro de la rama y devuelve una estructura por
valor en lugar de modificar tres argumentos `inout`. Conserva paleta, regiones,
materiales, recibos y opt-in. Basta importar los dos archivos de shader; **no
reaplicar el upgrade**. Requiere sólo volver a observar esa vista pausada con
opt-in 1 en el Editor central. Fuente/diff revisados aquí, sin ejecución de Unity;
la corrección visual permanece pendiente de esa comprobación.

El check central de `b54e33f6` recuperó visibilidad, pero mostró el acabado de goma
en cabeza/torso y el de vendas en manos, sin detalle del scan. `_Apron` seguía en
0 para Skin y 1 para RubberBib. La lectura del programa Metal generado confirma
muestreo del atlas y orden correcto de argumentos; no justifica recolorear la cara
para compensarlo. El siguiente parche sustituye únicamente el overwrite condicional
de goma por una mezcla aritmética con `_Apron`, igual al patrón histórico: con 0,
la salida conserva albedo y normal derivados del scan; con 1, usa la goma.
Sin cambio de paleta, materiales o recibos. Importar sólo el include y revisar la
misma vista; no reaplicar. La selección GPU efectiva queda por confirmar en central.

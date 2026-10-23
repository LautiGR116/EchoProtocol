# SE2 — peso contra el vidrio · 2026-10-23

**Integrado y guardado en Incursion; imagen comprobada en Editor. Juicio humano pendiente.**
Base `5277b28d`, rama `codex/se2-glass-pressure`. Sin builds, push, publicación,
Editor/Library nuevos, escenas escritas a mano ni assets privados copiados.

## Qué cambia jugando

El adulto presente pasa de `(7.18, .025, 30.45)` a `(5.08, .025, 29.48)`:
dos metros más cerca del frente, rostro inclinado y hombro cargado debajo de una
palma alta. La otra mano sostiene el cuerpo más abajo. Piernas apoyadas, dedos
extendidos sobre un plano real y superficie más mate/fría, conservando las
texturas de Eric. No camina, persigue ni sigue la cámara.

La palma asienta 4 mm con un contacto breve al descubrirlo; tras 2.4 s cede
3 cm durante .72 s, con una única fricción, y queda vencida. Sin repetición por
relook o RECALL. Pausa detiene el tiempo y la fuente respeta la pausa del listener.
El descenso no exige quedarse mirando y no impide salir.

Restricción real: el vidrio original está en x=4; el envelope existente comienza
en x=4.595. Una placa interior discreta en x=4.65, **fija en ambos estados**,
da apoyo sin ampliar gates. Es sólida en Ignore Raycast para que el evento pueda
leer a través del vidrio transparente. Bordes estrechos y luz lateral constante;
sin nuevos cambios de luz ni sombras del adulto fuera del volumen protegido.
La salida sigue al este/norte del cuerpo; comprobarla en la cámara real.

`DomesticPresence` conserva su detección amplia de dos puntos/0.07 s y delega
la actuación opcional a `SE2GlassPressure`. Sin integrador mantiene el comportamiento
anterior. `VS1ParallelFigureEvent`, sus referencias/envelopes, el rig compartido,
Player/Echo, P2 y mezcla global no se modifican.

## Integración central

1. Cherry-pick del commit de esta rama sobre la integración local. Los dos WAV
   siguen la política LFS existente; están en el almacén local compartido. Si el
   checkout deja punteros: `git lfs checkout Assets/_Game/Audio/SE2Presence/*.wav`.
2. En el único Editor, con **Incursion existente abierta y Play detenido**, ejecutar
   `SE2PresenceIntegration.ValidateCurrentScene()` y luego
   `SE2PresenceIntegration.ApplyToCurrentScene()`; también disponibles en
   `Echo Protocol > SE2 presence`. Requiere el adulto Eric privado ya instalado,
   los dos clips importados, URP Lit y colisión Default/Ignore Raycast activa.
3. La operación valida antes de modificar, añade sólo su jerarquía, mueve al adulto
   separado y desactiva sus sombras; crea dos materiales exclusivos en
   `Assets/_Game/Materials/SE2Presence`. No abre ni guarda escenas. Apply repetido
   es no-op; una instalación parcial se rechaza. Undo restaura la escena anterior;
   los dos materiales exclusivos quedan disponibles para Redo/reaplicar. Guardar
   manualmente después de comprobar la composición.

## Evidencia central

Aplicado sobre la escena original. Entrada preparada con gates existentes: los
volúmenes de presentación quedaron fuera de cámara al revelar. Frente y oblicuo
inspeccionados; descubrimiento y cesión contabilizaron uno cada uno, sin repetir
al pausar/reanudar o volver a mirar. No se hizo entrada natural completa,
Undo/Redo ni escucha humana; el miedo y la calidad de actuación no están aprobados.

## Evidencia del worktree y checks propuestos (no todos ejecutados)

Lectura de referencias/posiciones de Incursion, revisión focalizada y corrección
de captura Undo de la jerarquía completa. Compilación aislada Roslyn de las tres
fuentes contra Unity 6000.3.21f1 y la assembly existente: sin errores. Esto no
equivale a compilar/importar/aplicar dentro del Editor. `git diff --check` correcto.

Audio: dos recortes CC0 ya disponibles, mono PCM16/48 kHz, 0.32/0.72 s,
picos −20/−25 dBFS, ~100 KB, sin clipping/loops. Procedencia, hashes y receta
reproducible en `Assets/_Game/Audio/SE2Presence/Source~/`. Fuente WorldSFX con
ganancia <= la anterior; no se toca mono/dinámica reducida/volumen. Escucha pendiente.

1. **Entrada natural:** comprobar compilación/Console e instalación idempotente
   (Undo→Redo una vez), luego una entrada al SE2. Revelación oculta sin fuga de
   silueta/sombra; `ImpactCount=1`, `SlipCount=1`; cuerpo y manos legibles desde
   cámara normal. Revisar cara/dedos contra placa, sin intersecciones ni alcance
   imposible; guardar sólo tras inspección. No hace falta repetir P1 o campaña.
2. **Oblicuo/cercano:** acercarse desde un lado, pausar durante la cesión, reanudar
   y salir por el paso existente. Pose/audio congelados en pausa, sin nuevo ataque
   al volver a mirar; placa/cuerpo no atrapan al Player. Escucha breve en la opción
   mono/dinámica reducida ya disponible, a volumen cómodo.

Dos preguntas humanas: ¿la cercanía y el apoyo se leen como una persona que ejerce
peso, o como un maniquí ante otra placa? ¿La cesión y su fricción sostienen la
incomodidad después de descubrirlo, con retirada libre y volumen cómodo?
Miedo, material sonoro y actuación siguen **EN PRUEBA humana**.

# SE1 — el apagado llega hasta vos

2026-10-23. Base `4e240f2f`; rama `codex/se1-approaching-pressure`.
**Fuentes implementadas; integración/compilación/Play central pendientes.
Percepción, miedo y comodidad EN PRUEBA.**

Cuatro persianas metálicas cautivas convierten el vidrio conocido de P1 en una
superficie que se cierra hacia el jugador: paño lejano `z=3`, intermedio `z=7`,
y los dos próximos `z=11/15` juntos. El descenso próximo ocupa casi toda la
altura del paño en `0.22 s`; queda cerrado. No son otros tres clics de techo.
La chapa plegada, las guías y los topes reutilizan materiales existentes. Las
piezas móviles están dentro del espesor del vidrio sólido original: no invaden
la cápsula ni bloquean el pasillo. La persiana abierta queda sobre la visual de
los lectores; P1 y su reproducción no cambian.

Tras el knock y la mirada amplia originales, SE1 vuelve a comprobar el final
auténtico del Echo. Los recorridos duran `0.55 / 0.46 / 0.22 s`, con pausas
`0.62 / 0.36 / 0.45 s`. Cada apagado ocurre al llegar al tope. El remate sonoro
sale de los dos marcos próximos a ganancia `0.34` cada uno; sustituye el relé
viajero, sin superponerlo. La reserva cálida es estable: muestra la chapa, el
suelo, el giro norte y la puerta doméstica; se retira al comprometer el
checkpoint. Conserva el hold y la apertura doméstica de `5.2 s`, y la salida P2.
No requiere mantener la cámara orientada durante el avance.

El cierre doméstico conserva su retraso/impacto, pero espera si la cápsula entra
en el barrido completo de la hoja, incluso durante el movimiento. El punto de
RECALL queda fuera de esa guarda. La secuencia no es un target de reset:
RECALL no repite contactos. La restauración existente de capítulos posteriores
asienta las persianas silenciosamente. No se añade reproducción al Echo.

## Integración exacta

1. Aplicar el commit de esta rama sobre la integración central. Mantener abierta
   la Incursion original y esperar la importación/compilación normal del Editor.
2. Fuera de Play: `Echo Protocol → SE1 pressure → 1. Validate current scene`.
3. Ejecutar `Echo Protocol → SE1 pressure → 2. Apply to current scene`, o
   `SE1PressureIntegration.ApplyToCurrentScene()` desde el Editor.
4. Revisar el resultado y guardar **manualmente** la escena original.

El integrador valida SE1/P1/checkpoint, los cuatro vidrios sólidos, sus posiciones
y capas, luces originales, audio mono/WorldSFX, listener/opciones y la separación
del checkpoint respecto del barrido. Añade una jerarquía y tres referencias
opcionales. Una segunda ejecución válida no cambia nada; rechaza instalaciones
parciales. Un único Undo elimina la jerarquía y restaura enlaces; Redo recupera
la configuración completa. Ante excepción revierte el grupo. No abre/guarda
escenas, escribe assets compartidos, modifica YAML ni necesita recursos privados
nuevos. Los materiales existentes deben estar importados en el proyecto central.

## Evidencia y check central mínimo

Se revisaron fuentes, diff, límites numéricos de recorrido, referencias y
coordenadas de la Incursion privada **en lectura**. Los contactos CC0 conservan
la procedencia del repositorio; sólo se añadió un recorte mono de `0.614 s`,
88 KiB, sin ganancia. Pico `−6.375 dBFS`, máximo a `13.54 ms` del inicio:
`Assets/_Game/Audio/SE1Pressure/manifest.json`. Las cuatro fuentes usan WorldSFX,
listener/pausa habituales y las opciones de mono/dinámica reducida existentes;
no se cambia mezcla global.

No se abrió Unity ni se creó Library. **No se realizó compilación, integración,
Play, captura, escucha ni prueba de dispositivos en este worktree.** Tampoco
build, publicación, push o suite nueva.

Para el check central: usar la pose original de comprobación
`Player (3.35, 0.03, 10)`, mirada amplia hacia `(-2.75, 1.35, 14.5)`, con P1
completo y la reproducción real ya terminada. No hace falta un harness nuevo.
Comprobar un avance y continuar al checkpoint/P2; detenerse brevemente en el
barrido de la puerta y salir para comprobar su espera. Una pausa/RECALL durante
el mismo check basta para revisar congelación y ausencia de repetición. Las
exigencias normales de P1 y del Echo no se omiten en producción.

Quedan dos preguntas humanas: ¿el cierre próximo provoca retirada y se entiende
el camino con cámara libre? ¿el contacto se siente material y cómodo con volumen
normal, también en parlantes/dinámica reducida? No hay aprobación humana nueva.

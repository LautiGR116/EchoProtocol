# 6B — no quiero abrir · 2026-10-23

**Fuentes implementadas; integración/compilación/Play pendientes en el Editor
central. Miedo, comodidad y composición final EN PRUEBA humana.**
Base `4e240f2f`; rama `codex/final-reception-close-arrival`.

## Cambio perceptible

La hoja cerrada recibe un panel opaco real que tapa el visor. Al abrirla, el
jugador descubre el cuerpo silencioso entre un vestíbulo de madera profundo y
un segundo marco oscuro, abierto, detrás de la posición grabada. Techo local
a 2,24 m, laterales y líneas de suelo dan escala cercana; la luz lateral modela
la cara/torso y baja la competencia del baño general. No es un nuevo gesto del
Echo. No se mueve, escala, ilumina desde una cámara ni prolonga la toma.

La posición inicial conserva unos 2,65 m hasta el centro de la pausa grabable.
El acercamiento voluntario ya permitido llega a unos 1,63 m: se invita mediante
el detalle del umbral y el tirador existente, sin nuevo gate, texto o espera.
El tamaño del cuerpo en píxeles desde una cámara idéntica **no cambia**; cambian
la ocultación previa, los planos que lo encuadran y su contraste. La lectura
espacial efectiva debe juzgarse en la cámara central.

Quedan intactos origen, zonas, pausa/ángulo/tiempo de testigo, rayos de visión,
material del cuerpo, reproducción, conservación de la misma toma, llave,
confirmación audiovisual, portería y salida. No se modifica `FinalDoorMemory`.
El nuevo panel tiene collider opaco y pertenece a la bisagra original; no se
incluye en la excepción de transparencia. Mantiene la huella horizontal de la
hoja usada por `HouseSwingDoor` para evitar contacto con el Player.

## Integración y rollback

Con Incursion original ya abierta y fuera de Play, ejecutar en orden:

1. `FinalReceptionIntegration.ValidateCurrentScene()` — menú
   **Echo Protocol / Final reception / 1. Validate current scene**.
2. `FinalReceptionIntegration.ApplyToCurrentScene()` — menú **2. Apply**.

No abre, sustituye ni guarda escenas; no crea assets compartidos. Valida primero
layout, referencias, zonas, huella/cápsula, modo de recepción, materiales,
WorldSFX/listener/pausa y confirmación existente. Segunda aplicación válida es
no-op; una instalación parcial o alterada se rechaza sin duplicarla.

Un grupo de Undo incluye ambos árboles nuevos y tres valores locales. El fallo
revierte el grupo. **Undo/Redo** funciona sobre la instalación completa.
`FinalReceptionIntegration.RemoveFromCurrentScene()` / menú **3. Remove**
restaura los valores guardados incluso después de reabrir la escena y elimina
sólo los dos árboles propios. Rechaza valores alterados después de integrar
para no descartar otra edición. La escena queda sin guardar en todos los casos.

`FinalReceptionComposition` guarda propiedad/referencias/rollback; no tiene
Update ni actúa sobre Echo, Player o cámara. Coste añadido: 2 luces locales sin
sombras, 13 cajas físicas fijas y 1 panel físico móvil, más detalles pequeños.

## Sonido y procedencia

No se importa audio ni se genera voz. Se conserva el motor, pestillo y final
de recorrido originales, con su dueño `HouseSwingDoor`. Sólo el motor local
baja de `.14` a `.065`; el pestillo permanece en `.14`. Así la apertura termina
en el silencio real del registro, sin pasos o golpes atribuidos al cuerpo.
WorldSFX, fuente 3D, listener, pausa, mono y dinámica reducida siguen existentes.

Manifiesto de reutilización: `SFX_DoorMovement_LP.wav` y
`SFX_DoorEndpoint.wav`, Rolly-SFX, CC0, entradas AUD-MEC-001/002 de
`Docs/AUDIO_ASSET_LEDGER.md`; `Finish/Relay.wav` deriva del segundo, según
`ArtSource/Audio/Finish/derivatives.json`. Archivos sin editar ni duplicar.
Los cinco materiales reutilizados se referencian sin editar sus assets.

## Revisión realizada y único punto de observación

Lectura de fuentes y jerarquía/valores de Incursion privada, revisión focal por
un subagente, coherencia de 11 bindings y existencia de 5 materiales, aritmética
de colisión y `git diff --check`. Se corrigieron retornos anchos que ocultaban
la placa y lente existentes: ahora sólo son jambas estrechas. Luz libre 2,46 m;
margen lateral de cápsula+skin en el extremo de witness: 55 mm; margen hasta
jamba delantera: 85 mm. Son cálculos de autoría, **no simulación de física**.
No se abrió Editor/Library, compiló, ejecutó Play, hizo build o capturó imagen.

**Un estado para el coordinador:** checkpoint existente **Archive / revisión
6B**, grabar la llegada normal por el pasillo lateral y volver con R. Desde el
origen conservado `(-12.1, .03, -52.4)`, yaw `270°`, abrir con el tirador original
y mirar la llegada auténtica. Cámara normal a y≈1,63. Ese mismo intento muestra
cerrada→abierta y permite acercarse dentro de witness, confirmar, conservar y
salir. Compilación/Console y este check breve se hacen centralmente; no hace
falta otro harness ni un recorrido de campaña.

Dos dudas humanas: ¿abrir revela una presencia corporal incómoda y no un
escaparate? ¿se entiende cruzar a portería/conservar después del encuentro?

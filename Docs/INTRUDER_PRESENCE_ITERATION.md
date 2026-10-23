# Presencia del intruso — ronda 4 · 2026-10-23

**Implementada en código; pendiente de aplicación y check en el Editor central.
Lectura visual, actuación, escucha y miedo EN PRUEBA.** Base `d50783f4`, rama
`codex/intruder-presence-round4`. Sin Editor/Library/build en este worktree.

La captura `Builds/round3-integration-20261023/p3-contact-corrected.png` muestra
palmas blanqueadas, frente plano y escaso relieve del rostro. La escena central
confirma un rebote a altura de manos, muy cerca del contacto, y una lámpara baja
de intensidad 7,5. Se reutilizan las luces: ninguna nueva capa o luz seguidora.

- P3: la luz del intruso pasa al lateral alto delantero, dentro del vidrio,
  en `(-6.10, 2.17, 35.28)`, orientada a `(-5.56, 1.48, 36.02)`. Intensidad
  de aparición 1,05, cono 94°/55°, alcance 3,1 m y sesgo normal de sombra 0,06
  en lugar de 0,4. El rebote cercano baja de 1,25 a 0,18 y la lámpara de 7,5 a
  2,8; su dueño conserva el encendido causal. La iluminación cruza la cara y
  las manos en vez de llenarlas desde abajo. Exige juicio desde cámara real.
- Bomba: la luz existente pasa a `(-4.90, 2.20, -44.08)`, orientada a
  `(-4.08, 1.42, -44.74)`, cono 94°/55° y alcance 3,4 m. Conserva sus ganancias
  inactiva/activa y la luz de retirada. No sigue al actor ni gira la cámara.
- El contacto de carcasa conserva su fuente y ganancia 0,78. Esfuerzo 0,42→0,31,
  respiración 0,58→0,36 y fuente de frenada/intake 0,38→0,30. El esfuerzo
  expulsado sigue al contacto físico por su llamada existente. Se omite la voz
  Launch si ese mismo contacto ocurrió hace menos de 1,2 s: los dos clips
  recortaban casi el mismo esfuerzo del mismo adulto. Reacquire se conserva.
- La respiración pierde ganancia durante Entry y hasta contacto+0,60 s, y se
  recupera mediante la envolvente existente. Staging, Embestida y Presence usan
  **la misma fuente**. Se retira la fricción continua del cuerpo; siguen los
  contactos reales de suela/frenada. Sólo el emisor adicional del retorno de
  bomba baja al 30% durante ese ataque; la máquina causal y su mezcla no cambian.
- P3 conserva exactamente sus dos disparos, apoyo y marco, sin voz ni segunda
  reproducción. El sonido existente del marco todavía requiere escucha humana.

## Integración y retirada

Aplicar el commit en el proyecto original, dejar importar los scripts y usar la
Incursion privada **ya abierta y fuera de Play**:

1. `Echo Protocol > Intruder presence > 1. Validate current scene`
   (`IntruderPresenceIntegration.ValidateCurrentScene()`).
2. `Echo Protocol > Intruder presence > 2. Apply to current scene`
   (`IntruderPresenceIntegration.ApplyToCurrentScene()`).
3. Revisar una llegada de P3 y entrada de bomba; guardar explícitamente después.

Apply repetido es un no-op si la instalación está íntegra. Preflight comprueba
el intruso único, P3/Embestida existentes, fuentes mono WorldSFX, pausa/volumen
del listener, la respiración compartida y las referencias de luces. Un marcador
`IntruderPresence / local contrast` conserva sólo datos de autoría/rollback;
no tiene Update ni reproduce audio. No depende de archivos nuevos de A/B.
Ante excepción se revierte el grupo Undo. Undo revierte todo; después de guardar
y reabrir, `3. Remove from current scene` restaura los valores originales.
Si alguien editó esas luces/ganancias después, se rechaza la retirada para no
pisar el trabajo. No abrir/guardar escenas automáticamente ni ejecutar el
generador histórico `EmbestidaProduction`.

## Recursos y comprobación efectuada

Se reutilizan WAV ya licenciados, sin descarga ni derivado nuevo. El esfuerzo
actual no es otro pitch del antiguo grito de Archeos: procede de aldenroth2,
Freesound 272023, CC0. Contacto/respiración de Embestida conservan los recursos
de NachtmahrTV 556693 y JohnsonBrandEditing 173944. Trazabilidad en el original:
`ArtSource/Design/Embestida/audio-masters.json`, `ArtSource/Audio/Hunt/masters.json`
y `ArtSource/Audio/Impact/masters.json` (Brian MacIntosh/Finish). El subagente
contrastó hashes de los WAV centrales con esos manifiestos. No se importó ni
commiteó material privado de Renderpeople.

Realizado: inspección de la captura indicada, lectura de configuración central
sin escritura, revisión de diff/referencias y `git diff --check`. No ejecutados:
compilación Unity, Console, Play, escucha ni comparación de imagen resultante.
El coordinador debe comprobar compilación y una presentación breve; no hace
falta repetir campaña ni validar rutas equivalentes. Se mantienen WorldSFX,
opciones mono/dinámica reducida, pausa, cámara libre, actor único y toma fiel.

Dos preguntas humanas: ¿se leen ojos/manos y relieve del rostro sin palmas
quemadas? ¿el golpe, el esfuerzo único y la respiración posterior hacen sentir
el peso y orientan la retirada a volumen cómodo?

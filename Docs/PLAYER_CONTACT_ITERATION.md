# Contacto del jugador — 2026-10-23

**Integrado y guardado en Incursion; compilación y checks puntuales de Play realizados.**
Base `5277b28db7d1984178896d4c6487af16996b85e3`, rama
`codex/player-contact-weight`. No se abrió Unity, importó Library, ejecutó build,
publicó ni modificó el proyecto original. Comodidad y peso percibido **EN PRUEBA**.

## Dos causas concretas

1. **Echo junto a pared.** `PlayerEchoClearance` sólo pedía un empuje radial de
   hasta .18 m/frame. La cápsula real de Incursion tiene radio .32 m, skin .03 m
   y clearance .64 m: si el vector radial apunta a una pared, `Move` puede
   consumirlo y mantener el solapamiento aunque exista salida lateral.
   Ahora se prueban radial, diagonal y tangente mediante barrido de cápsula;
   el lado se conserva durante el contacto. El máximo es 6 m/s, limitado a
   .12 m/frame. Si `Move` consume el candidato, hay un solo intento tangencial
   con el presupuesto restante. Siempre mueve el CharacterController; nunca
   el Echo, sus datos, la cámara por separado ni la geometría. Un corredor sin
   espacio sigue sin garantizar separación total. El origen compartido de
   RECALL se conserva y el Echo terminado sigue excluido: se desactiva realmente.

2. **Foco anterior a la superficie/cámara.** El receptor con `InteractionSurface`
   se mueve en `HouseTelephone.LateUpdate`; `PlayerInteractor.Update(100)` había
   resuelto antes, con `autoSyncTransforms=0`. Clearance mueve además el ojo en
   Late(280). La tolerancia de borde aceptaba puntos detrás de otra superficie
   del mismo dueño. Ahora las dos superficies explícitas se sincronizan sólo
   cuando cambia su matriz; Late refresca la misma selección si cambia la pose,
   y un evento después del desplazamiento real de clearance actualiza el ojo.
   El punto corresponde a la primera superficie física visible del dueño.
   Una superficie deshabilitada deja de ceder foco. E sigue en Update con sus
   bloqueos de pausa/RECALL/liberación, alcance 3 m y tolerancia existentes.

Estos son diagnósticos de código/configuración, **no observaciones de cámara**.
No se añadieron head bob, HUD, canales del Echo ni cambios de controles.

## Pasos propios

Late(320) mide movimiento efectivo después del contacto y descarta empujones y
avance cancelado. El apoyo alterna ±.10 m y consulta suelo transitable y material
en el contacto real. Primer apoyo a .42 m; luego stride 1.4 m con intervalo mínimo
.35 s. Reposo limpia fase; pausa, desactivación y RECALL paran ambas colas.
Un segundo emisor estático conserva la cola anterior de madera (.44 s) en su
apoyo cuando el siguiente paso llega a .35 s. Movimiento lento sólo reduce peso;
la ganancia máxima, WorldSFX y las opciones de salida existentes se conservan.

Se reutilizan `Finish/FootVinyl0..2.wav` y `Overhaul/FootWood0..2.wav`, confirmados
por GUID en Incursion. Procedencia existente en
`ArtSource/Audio/Finish/derivatives.json` y `ArtSource/Audio/Overhaul/sources.json`.
Sin importaciones, nuevas licencias, generación ni cambio de masters.

## Aplicación por el coordinador

Cherry-pick del commit entregado sobre la base integrada. Con compilación
terminada, Play detenido y **la Incursion actual ya abierta**:

- Ejecutar `PlayerContactIntegration.ValidateCurrentScene()` y luego
  `PlayerContactIntegration.ApplyToCurrentScene()` (menú `Echo Protocol/Player contact`).
- Requiere un Player con sus componentes existentes, referencias de loop/sesión,
  cámara, fuente WorldSFX, seis clips y zonas de madera válidas. Comprueba
  sensibilidad .15, velocidad 4, alcance 3 y stride 1.4 sin modificarlos.
- Añade únicamente `PlayerContact / foot support` con una fuente copiada de la
  actual y asigna referencias de Player. No modifica colliders ni superficies
  de otros frentes. Valida antes y después, registra Undo y una segunda ejecución
  es no-op; una instalación parcial falla antes de editar. No abre ni guarda
  escenas. El coordinador revisa/guarda con el Editor central.

## Evidencia central

Fixture temporal con Echo quieto y pared: el jugador salió lateralmente sin
cruzar pared, desplazar al Echo ni emitir pasos por el empuje. El teléfono
descolgado recibió foco sobre el receptor actual; una caja interpuesta retiró
foco y `TryInteract` devolvió false sin incrementar activaciones. Fixtures retiradas.

Desplazamientos breves del CharacterController comprobaron material clínico/madera,
alternancia y posiciones fijas de emisores; pausa detuvo ambas fuentes sin
contactos adicionales. La inyección de teclado no movió al jugador, por lo que
esto no se presenta como prueba de controles físicos. No se repitió RECALL ni
se aprobó la escucha o comodidad. Play detenido, sin build.

## Checks originales propuestos (no todos ejecutados)

1. **Contacto y foco:** grabar unos segundos quieto cerca de una pared recta;
   RECALL, salir >.76 m y acercarse por el hueco entre pared y Echo, dejando
   espacio lateral. Confirmar salida consistente, sin atravesar pared y sin
   pasos por empuje quieto. En el teléfono, descolgar/colgar mirando su borde;
   comprobar que foco y punto siguen la superficie actual. Para la oclusión,
   una caja temporal declarada entre ojo y receptor debe quitar foco y bloquear
   E; retirarla al terminar. No modificar geometría permanente. Console sin errores.
2. **Apoyos:** caminar unos pasos clínico→madera, parar, pausar durante una cola,
   reanudar quieto y hacer RECALL tras otro paso. Comprobar material, cola anclada,
   ausencia de contactos/colas resucitadas y cadencia conservada. Escucha breve
   a volumen cómodo con la salida habitual y su opción mono/dinámica reducida.

Revisado: diff, referencias/orden de ejecución y configuración privada por
lectura; `git diff --check` pasa. Una implementación de pasos y una crítica breve
se delegaron con archivos exclusivos. No hay aprobación automática de escucha.

Preguntas humanas: ¿el roce lateral se siente firme y permite salir sin tirón?
¿cada paso parece apoyarse en ese suelo y deja espacio al silencio al detenerte?

# Intruso — frase corporal, ronda 5

2026-10-23. **Código y cuatro derivados implementados; integración, compilación
y escucha en Editor pendientes del coordinador.** No se abrió Unity en este
worktree. Miedo, identidad vocal y comodidad siguen **EN PRUEBA humana**.

## Cambio concreto

La manija que ya carga el cuerpo provoca una sola inhalación cortada. En la
irrupción, el contacto real reproduce un golpe mecánico seco de 0,17 s; la
expulsión breve empieza a tener señal a 0,195 s del contacto y termina a 0,555 s.
La recuperación respiratoria puede empezar a 1,05 s, dejando aire entre gestos.
No se prolonga la actuación con un aliento en bucle. Una reacción de búsqueda,
frenada o captura cancela la recuperación pendiente; la captura/reacción del
Player conserva sus reglas. La salida junto a la máquina recibe un solo aliento
0,9 s después de su contacto existente, si sigue en ese estado.

En P3 se conserva el golpe del vidrio y su dueño; un aliento único a 0,85 s del
contacto real deja al cuerpo recuperarse. No anticipa la figura aún oculta. El
relanzamiento tras la irrupción no duplica el esfuerzo, aunque C prolongue la
recuperación. Búsqueda y readquisición reutilizan las dos frases cortas mediante
los disparos de estado ya existentes, sin temporizador de gritos.

Los cuatro WAV suman **134.576 bytes**, mono 48 kHz PCM16. CC0: otra toma de
[qubodup](https://opengameart.org/content/15-vocal-male-strainhurtpainjump-sounds),
dos gestos distintos de [mikeask](https://opengameart.org/content/breathing-tired)
y contacto de [Brian MacIntosh](https://opengameart.org/content/mechanical-sounds).
Son recortes de originales existentes a pitch/velocidad nativos, sin doblado,
reverberación añadida ni voz generada. Selección por función y forma del material;
**no se afirma haber realizado casting por escucha humana**.
[Manifiesto](INTRUDER_AUDIO_ROUND5.json): fuentes, hashes, recortes, filtros,
envolventes, licencia, metas y límites. Los WAV siguen la política LFS existente.

## Hook único fuera de D — debe aplicarlo el coordinador

Archivo `Assets/_Game/Scripts/Campaign/EmbestidaRush.cs`; agregar junto a sus
referencias serializadas:

```csharp
[SerializeField] EchoProtocol.Audio.MachineCoverPresence discreteBreathOwner;
```

En `LateUpdate()`, **después** del bloque que desplaza `impactPanel` y **antes**
de `bool sustain = ...`, agregar:

```csharp
if (discreteBreathOwner && discreteBreathOwner.OwnsDiscreteBreathing) return;
```

No cambiar `Advance`, `contact.Play()`, `effort.PlayDelayed(.075f)`, reacción ni
captura. `MachineCoverPresence` queda como único dueño de iniciar/detener el
aliento del perfil nuevo. `OwnsDiscreteBreathing` es falso por defecto y también
si faltan referencias o el componente no está activo. Las ediciones anteriores
retienen el sustain histórico. El integrador exige el campo y lo enlaza; no
modifica el archivo de C ni instala un segundo reproductor.

Revisión focal: `IntruderStaging` no inicia/reinicia `breath` con Embestida
instalada; sus `Play/Stop` sostenidos están dentro de `if (!embestida)`. Conserva
los `Stop()` de inicio/reset/deshabilitación. `MachineCoverPresence` sigue siendo
el único escritor de ganancia acústica de esa fuente; no se agrega otro.

Pausa: `PresentDiscreteBreaths` pasa por el guard existente de sesión/delta y
las fuentes respetan `AudioListener.pause`. Reset/captura/deshabilitación paran
las fuentes y borran las recuperaciones pendientes. `ResetPresence` rearma sólo
la preparación/recuperación de bomba; el contador real de contactos P3 evita
repetir una visita ya ocurrida. No hay coroutine ni audio programado adicional.

## Upgrade desde la escena ronda 4 guardada

1. Incorporar estos archivos y el hook anterior en el original. El coordinador
   realiza la compilación conjunta. Mantener Incursion abierta fuera de Play;
   conservar su backup de escena habitual. No ejecutar `EmbestidaProduction` ni
   los Apply históricos.
2. Menú **Echo Protocol → Intruder audio → 1. Validate round-five upgrade**
   (`IntruderAudioIntegration.ValidateCurrentScene()`). Comprueba escena,
   contrato de handoff, P3 real, fuentes compartidas y WorldSFX, masters de
   ronda 4, importación mono/PCM precargada y ausencia de recibos parciales.
3. **2. Upgrade current scene** (`IntruderAudioIntegration.UpgradeCurrentScene()`).
   Cambia clip/pitch/loop de tres fuentes existentes, cinco referencias de
   frases y dos enlaces; activa el perfil. Conserva volumen, filtros, mixer,
   prioridades, distancias, poses y luces. Crea sólo un recibo sin playback.
   Es idempotente y deja la escena **sin guardar**.
4. En el check compartido de P3 y bomba, escuchar un golpe por contacto,
   separación metal→esfuerzo→aliento y silencio posterior. La preparación
   debe dar una sola inhalación; pausa/reintento no deben rearmar sonidos sin
   una nueva acción. Conservar mono/dinámica reducida; no se han revalidado aquí.
   Guardar la escena sólo después del check central. Sin ruta completa.

**Rollback:** Undo del grupo de upgrade, o **3. Roll back current scene**
(`IntruderAudioIntegration.RollBackCurrentScene()`) incluso tras reabrir la
escena guardada. El recibo restaura clips, pitch, loop, enlaces y flag anteriores,
sin guardar automáticamente. Rechaza referencias alteradas para no pisar trabajo
posterior. Retirar primero D antes de usar el Remove de presencia de ronda 4,
cuyo preflight exige los antiguos nombres de clips; ese recibo permanece intacto.

## Comprobación propia y límite

Diff/propiedad revisados; WAV/formato/hashes/metas y separación de muestras
comprobados localmente. No se compiló, jugó ni capturó en este worktree. Ningún
cambio en Staging, EmbestidaRush, P3, luces, escena, Echo u opciones globales.
La respiración íntima doblada/modulada ya tuvo rechazo humano en una entrega
anterior: este recorte seco y único debe juzgarse en la habitación. Puede sonar
demasiado leve o genérico; la coherencia del nuevo esfuerzo con el adulto y el
peso del contacto requieren oído humano a volumen cómodo, no un pico de señal.

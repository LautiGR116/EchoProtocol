# La casa ocupada — 2026-10-22

Estado: **implementado e integrado en la copia privada del worktree; compilación
y comprobaciones puntuales de Editor completadas**.
Miedo, lectura espontánea, mezcla percibida y comodidad siguen **EN PRUEBA humana**.

## Qué cambia jugando

1. La casa concede cinco segundos activos de recuperación. Contestar, mover la
   reserva o avanzar hacia la llave compromete un único peso sobre madera detrás
   del estudio. No atender también permite experimentar la secuencia y salir.
2. Si el recorrido deja espacio, al acercarse a la llave se oye una fricción
   contra la hoja, separada del peso. La llamada conserva su actuación y colgar
   conserva su consecuencia: estas señales esperan al silencio del auricular.
3. Con la llave, el regreso por el pasillo carga la hoja sur del estudio durante
   0,7 s; ésta cede en 0,13 s, recibe un contacto material y queda entreabierta
   bajo tensión antes de asentarse. Una retirada al otro lado deja residuo.
   No gira la cámara ni exige mirar o reconocer la presencia; tampoco inicia otra caza.

Una composición: presión desde una habitación cerrada. Las dos hojas reutilizan
el modelo `EP_OV_DomesticDoor`, materiales `M_OV_*`, herrajes y molduras existentes.
La hoja sur está situada para anticipar el contacto durante la marcha normal;
la lectura espontánea del gesto debe juzgarse jugando.
El cerramiento sólido anterior permanece; no se añade un paso ni un puzzle.
Una luminaria local estable permite leer el movimiento incluso sin reserva en
el recibidor. No parpadea ni altera la alimentación elegida.

## Propiedad y reglas

| Elemento | Dueño |
|---|---|
| Llave, salida, reserva, luces de circuito, frigorífico y timbre | `HouseMemory`, sin cambio de reglas |
| Auricular, cable y contactos al descolgar/colgar | `HouseTelephone`, sin cambios |
| Actuación telefónica | `ResonanceSequence` y su emisor existente; referencia sólo de lectura |
| Hoja nueva, tres fuentes nuevas y temporización doméstica | `HouseOccupiedPresence` |
| Luz de servicio nueva | Fija, sin controlador ni pertenencia a las matrices de reserva |
| Aparición antigua | Desactivada en Awake; no se evalúa si hay reemplazo asignado |
| Filtros de teléfono/frigorífico | `HouseRoomAcoustics`, sin cambios |

La recuperación y las pausas usan tiempo activo, no reloj real. Salir del volumen
antes del contacto cancela ese contacto; abrir la salida nunca espera la actuación.
Salir del capítulo, desactivar la casa o restaurar el repaso final retira los
sonidos pendientes. Un reemplazo mal configurado falla con diagnóstico y no
resucita la aparición anterior. No hay registro, reproducción ni canal nuevo de
Echo. `DomesticPresence` sigue siendo exclusivamente SE2 y no fue modificado.

El retorno tiene prioridad sobre los indicios: un recorrido rápido no acumula
los sonidos omitidos para dispararlos juntos. El roce previo continúa moviendo
la hoja durante la pausa sonora. En pausa de sesión no avanzan ni actuación ni
fuentes; se respeta el listener, las opciones de audio y la dinámica reducida.

## Integración después de la bomba

Rama de entrega: `codex/casa-ocupada`, basada en `9dbc67c5`.
Commits de implementación: `e30c1ed4` (audio) y `6773b395` (runtime/autoría);
la entrega incluye además el commit de este documento.
No contiene escena privada, modelos privados, builds, cambios de paquetes ni
configuración global. No se ejecutó la integración sobre el proyecto principal.

1. Integrar primero los commits del chat de la bomba y luego los commits de esta
   rama con `git cherry-pick` (identificadores en la entrega del chat).
2. En Unity 6000.3.21f1, abrir la **escena vigente después de la bomba**, fuera de
   Play. Debe conservar la casa de Incursion y el estudio ampliado en x1/z−58,3.
3. Ejecutar `Echo Protocol > House occupied > 1. Validate current scene`.
   Rechaza una geometría distinta, referencias faltantes, salida/cámara ajenas,
   materiales/clips ausentes o una instalación parcial antes de modificar.
4. Ejecutar `Echo Protocol > House occupied > 2. Apply to current scene`.
   Crea sólo `House occupied / study pressure` bajo la casa y asigna
   `HouseMemory.occupiedPresence`. Conserva los objetos y referencias anteriores.
5. Revisar el nuevo conjunto y guardar la misma escena con Ctrl/Cmd+S. La operación
   **no abre, reemplaza ni guarda escenas**, ni cambia Build Settings. Repetir
   Validate/Apply sobre una instalación válida no añade nada. Hay un único Undo.

No copiar `EchoProtocol_Incursion.unity` de este worktree sobre la escena del otro
chat. La copia privada sólo sirve para la comprobación aislada. Si el layout
cambia más adelante, corregir las referencias/volúmenes explícitos; no omitir la
validación ni buscar objetos de otros capítulos por coincidencias parciales.

## Recorrido humano de 2–3 minutos

Desde **Continuar / La casa**, si ese guardado ya existe. Para una entrada aislada
sin escribir guardados, el integrador puede usar el helper existente
`CampaignDirector.PrepareHouseProbe()`, restaurar la bomba con
`CoverEncounter.RestoreState(true)` y activar la estación `HouseOrigin`, con
`SuppressSaves = true`; no es un selector permanente del menú:

- Esperar unos segundos y reconocer timbre/frigorífico. En la primera pasada,
  ignorar el teléfono, ir a la llave junto a la maleta y volver por el pasillo
  hacia la salida. Mantener la mirada de marcha habitual; no buscar un píxel.
- Escuchar dónde queda el peso, cómo cambia la proximidad y si el contacto de la
  puerta tiene peso físico. Continuar, abrir con la llave y salir sin mirar al
  estudio. No hay ninguna condición adicional.
- En una segunda pasada opcional de 2–3 minutos, atender, escuchar la actuación
  y colgar; trasladar la reserva y recoger la llave. La llamada debe conservar su actuación y corte; el contacto de retorno
  sigue disponible. Pausar durante la preparación, continuar y comprobar que
  no se consumió durante el menú.

Auriculares a volumen cómodo; también probar parlantes/dinámica reducida. Juzgar
localización, claridad de movimiento, cercanía y tensión por separado. Compilar
o detectar el evento no demuestra que asuste.

## Audio y procedencia

Tres masters propios de esta secuencia, CC0, derivados de grabaciones ya
acreditadas: SoundDesignForYou (madera), ftpalad (pisada de madera), AntumDeluge
(roce), Brian MacIntosh/BMacZero (contacto mecánico). Sin voces ni servicios nuevos.

`Assets/_Game/Audio/Clips/HouseOccupied/HouseOccupiedManifest.json` conserva URLs,
evidencia local de licencia, hashes de entrada/salida y ediciones exactas.
Los previews MP3 de dos fuentes no se presentan como originales sin pérdida.

| Master | Duración | Pico lineal | Ganancia inicial de fuente |
|---|---:|---:|---:|
| Occupied_Weight | 1,58 s | 0,28 | 0,30 |
| Occupied_Contact | 1,76 s | 0,42 | 0,36 |
| Occupied_Retreat | 2,18 s | 0,24 | 0,24 |

Mono PCM24/48 kHz, 804 KB combinados antes de LFS, sin clipping, extremos en cero.
El ataque principal del master aparece a 114 ms y su pico a 129,5 ms: coincide
con el extremo de los 130 ms de movimiento, no con su inicio. Sonidos 3D,
WorldSFX, sin Doppler. Alcance de peso/contacto 12 m y retirada 11 m; el roce
previo usa la fuente de la hoja y alcanza la aproximación a la llave. Espectro
editado en los archivos, sin depender de filtros
que puedan faltar en WebGL. No se hizo build web.

## Comprobación y límites

- Inspección de fuentes: propiedad exclusiva, teléfono opcional, salida por llave,
  cancelación previa al contacto, pausa y retiro del capítulo; revisión cruzada
  de dos subagentes con archivos separados.
- Audio: formato, duración, hashes, pico y ausencia de clipping comprobados.
- `git diff --check`: limpio.
- La primera importación aislada falló por ENOSPC. El chat de la bomba recuperó
  espacio y la apertura bajo coordinación autorizada por el usuario. No se
  usó su Editor principal para autoría de la casa.
- Compilación final: `recompile_status` completado, `failed=false`, sin errores.
  El Editor registró una excepción de su indexador `UnityEditor.Search` durante
  la importación; no es un error del runtime doméstico y no se reparó ese sistema.
- Autoría: Validate y Apply correctos en la copia privada; repetir Apply no hace
  cambios. Una secuencia, tres fuentes propias, referencias válidas y escena
  guardada. Principal intacto por este chat.
- Se intentó la rama sin atender mediante `StudyExposureProbe` existente. La
  navegación se detuvo antes de la llave al exceder su tolerancia a baja cadencia.
  Ese intento no verifica el regreso ni la salida; la captura parcial quedó en
  `Logs/HouseOccupied/partial-unanswered.wav`.
- Comprobación puntual posterior: preparación mediante checkpoint/APIs existentes,
  llave y salida por sus métodos de interacción, marcha por CharacterController.
  No equivale a una ruta humana completa con teclado. La recuperación mantuvo
  cero indicios hasta una acción; pausa de 0,9 s mantuvo tiempo/hoja/contador y
  listener congelados; continuar produjo un contacto y residuo; sin atender,
  la salida abrió completamente y el cuerpo atravesó hasta z−63,5.
  Captura final del contacto: cámara a z−57,32, preparación+ataque a 0,898 s;
  hoja sur visible a la izquierda del encuadre de marcha, sin giro impuesto.
- Una primera captura del retorno mostró el gesto demasiado lateral. Se adelantó
  el inicio del volumen 1,4 m, sin cambiar la duración ni mover la cámara. El
  alcance del roce se corrigió de 7 a 12 m porque no alcanzaba la llave.
- Evidencia local excluida de Git: `Logs/HouseOccupied/focused-result.txt`,
  `contact-player-camera.png` y `focused-listener.wav` con marcas temporales.
  Listener estéreo, dinámica normal: 20,33 s, pico 0,420, cero muestras saturadas;
  `contact-mix-preview.wav` recorta ocho segundos sin alterar ganancia.
  La cámara se renderizó desde el componente del jugador durante una marcha
  preparada; no es una grabación de un recorrido humano.
- **Pendiente humano:** escucha de la mezcla capturada y en juego, audibilidad
  contra el ambiente, lectura espontánea del movimiento, fuerza del contacto,
  comodidad, parlantes/dinámica reducida y segunda pasada atendiendo la llamada.
  La captura del listener y sus niveles no sustituyen esa escucha.

Play quedó detenido, autotick desactivado y el componente temporal de captura
retirado. No se cambió PlayerPrefs durante estas comprobaciones.

No se hizo campaña completa, build, despliegue, merge ni push.

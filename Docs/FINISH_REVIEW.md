# Echo Protocol — acabado de la experiencia

2026-09-30 · encargo `OVERNIGHT_GOAL.md`. Escena actual:
`Assets/_Game/Scenes/Production/EchoProtocol_Finish.unity`.

## Qué cambió al jugar

- Clínica y casa tienen superficies a escala, pintura, linóleo, madera, papel
  envejecido y zócalos. La recepción y las habitaciones incorporan objetos de uso;
  la luz local, la oclusión de contacto y el contraste frío/cálido dan volumen.
- Válvulas con rueda y manómetro, fusibles, portapapeles y lectores circulares
  reemplazan cajas de interacción provisionales. Agujas/pilotos reflejan presión
  y trabajo reales; 29 difusores siguen sus luces. El lector de presión comparte
  la regla visual aprendida y no concede presión al jugador.
- Se retiraron paredes de instrucciones y comentarios que anticipaban o
  explicaban los encuentros. H conserva ayuda completa; encabezados breves,
  acciones contextuales y documentos sobre papel dejan respirar la escena. Una
  indicación discreta muestra el avance real al mantener un control a la vista.
- La carta de Mara en recepción introduce un vínculo y una contradicción. El
  teléfono se atiende mirando el aparato. Los documentos domésticos son contexto
  opcional: la salida depende del teléfono y de los circuitos, no de coleccionarlos.
- Pasos y varios contactos usan derivados de grabaciones CC0 con trazabilidad.
  El fondo institucional se retira en casa/archivo. Durante la espera inicial de
  presión ocurre un contacto ambiental autorizado fuera de campo; no altera el
  registro ni añade reproducción de sonidos o manos al Echo.

La dirección sigue siendo estilizada. No se presenta como equivalencia
fotorealista al moodboard ni como aceptación humana del miedo.

## Iteraciones observadas, no sólo implementación

Se descartó un yeso demasiado áspero y una conversión general a focos que dejaba
paredes negras. Las capturas del recorrido revelaron controles montados de
espaldas; se corrigieron y se revisaron desde sus posiciones de uso. La espera de
presión dejó de enfrentar al jugador a una pared vacía. La revisión final detrás
del vidrio detectó un lente cian demasiado débil: se sustituyó por material
luminoso estable. La prueba nativa también detectó intersección de cámara con el Echo al hacer
RECALL sin alejarse: su shader ahora desvanece la figura a distancia muy corta,
sin moverla ni alterar muestras. Los menús tienen fondo opaco. Se conservaron
las builds de las iteraciones.

[Índice de evidencia](VisualEvidence/2026-09-30-Finish/README.md): capturas durante
movimiento, detalles desde Game view, tomas preparadas identificadas como tales
y registros de pruebas separados. Las imágenes estáticas no se presentan como
prueba del recorrido completo.

## Comprobado

- Campaña completa en Editor: **142 comprobaciones**, **14 RECALLs**, del menú al
  final con Input System, física y E reales. La toma y las poses del Echo siguen
  inmutables. El tiempo de una ruta automatizada no mide una primera partida.
- **10 comprobaciones** de carta inicial, **29** de ruta doméstica sin documentos
  y **30** de continuación. Orientación correcta/incorrecta del nuevo sensor,
  señal P1, apagado material de 29 difusores y cero scripts faltantes comprobados.
- Diez WAV nuevos: hashes concordantes, mono, señal finita y sin muestras
  recortadas. Esto no sustituye escuchar la mezcla final.
- El recorrido integral precedió los últimos ajustes de modelos, lentes y
  emisión. Esos cambios recibieron las verificaciones dirigidas posteriores;
  no se afirma una segunda partida integral de la app final.

**Build final `build_2da095c64a76`:** macOS universal arm64/x86_64, 175.819.537
bytes según Unity, cero errores. Firma local verificada. Las 346 advertencias
corresponden a 345 shaders del paquete de inferencia de Unity y una al servidor
Pipeline deshabilitado en Player; el juego no depende de esos sistemas.

**Prueba nativa con interfaz:** el candidato RC2 pasó menú, opciones mono/dinámica,
créditos, inicio, WASD/registro, H, R/Echo, pausa y cierre, sin errores en su log.
El arrastre de mouse de la herramienta no estableció una variación medible de yaw;
el mouse físico sigue como comprobación humana. Esa prueba detectó los dos últimos
defectos visuales: intersección próxima del Echo y fondo visible tras el menú.

**Tras corregirlos:** la app final pasó arranque/cierre nativo sin interfaz
(`batchmode/nographics`), y el shader se verificó con la cámara real en Unity:
sin cuerpo invadiendo la vista a 0,08 m, visible a 1,6 m. Esto no equivale a repetir
la inspección visual nativa final: macOS estaba bloqueado y la herramienta exigió
desbloqueo manual. Queda expresamente pendiente esa repetición de los dos ajustes.

## Recuperación

Se conservan la campaña anterior y la edición breve, tanto sus escenas como sus
builds. Comparación con el respaldo inicial de este encargo: **710 archivos
idénticos, 14 cambios deliberados y cero archivos ausentes**. Escenas previas y
paquetes permanecen idénticos. Los cambios al renderer global agregan una entrada
para la cámara de Finish; el renderer anterior continúa disponible.

Respaldo inicial: `/Users/lautigr116/EchoProtocol-backups/finish-start-20260930/`.
SHA-256 del archivo fuente: `99ebd016dac46a28135c19fc07e94707c4402a6230325cfb5e290bce1d48db5a`.
Respaldo final fuente: `/Users/lautigr116/EchoProtocol-backups/finish-delivery-20260930/`,
con manifiesto y SHA-256 propios. App, LEEME, créditos e informe:
`Builds/EchoProtocol-Finish-20260930/`; ZIP y archivo `.sha256` junto a la carpeta.
También se conservan RC1 y RC2. Los dos ZIP anteriores conservan sus hashes.
Sin commit, push, descarte de cambios previos ni gasto.

## Lo que requiere una partida humana

Miedo, atribución al Echo, claridad sin instrucciones externas, ritmo, duración,
comodidad visual y escucha espacial/tímbrica siguen **EN PRUEBA**. La herramienta
de esta sesión informó que el modelo no admite entrada de audio; no se simula
una audición. Se recomienda una partida sin ayuda externa, auriculares a volumen
cómodo, y un tramo con parlantes/mono/dinámica reducida. Registrar confusión,
anticipación y si los silencios/contactos se perciben en su contexto, sin subir
volumen para forzar impacto.

No se declara duración humana de 30–45 minutos, rendimiento en otros equipos,
notarización para distribución pública ni aprobación artística final por terceros.

# Rooms — tratamiento acústico doméstico acotado

Estado: integrado en `EchoProtocol_Rooms.unity`, compilado y verificado objetivamente: sonda PASS37 / 4119 cuadros / una fixture inicial. Build universal y firma pasan; entrada nativa y audición pendientes. El integrador conserva la autoría exclusiva de escenas. Los apartados propuestos siguientes conservan la especificación de origen.

## Integración propuesta

- Copiar `staged/HouseRoomAcoustics.cs` a `Assets/_Game/Scripts/Audio/` y `staged/RoomsProduction.cs` a `Assets/Editor/` cuando termine el checkpoint Records.
- Compilar y ejecutar `Echo Protocol/Author domestic room acoustics` fuera de Play.
- Deriva `RecordsProduction.ScenePath` a `EchoProtocol_Rooms.unity`; checkpoint `EP.CampaignCheckpoint.Rooms.v1`. Records no se guarda de nuevo.
- Agrega un componente, dos `AudioLowPassFilter` y referencias a colliders existentes. No crea medios ni modifica materiales, geometría, clips, importadores o mixer.

## Contrato

El timbre procede de `HouseMemory.telephoneAudio`, el frigorífico de `HouseMemory.kitchenApplianceAudio`. La cámara y la sesión se toman de las referencias explícitas de esa misma casa. El filtro se agrega al GameObject de cada fuente; la autoría rechaza fuentes compartidas, bypass de efectos, audio no completamente 3D o filtros previos.

La obstrucción consulta **sólo** los colliders arquitectónicos listados explícitamente por la autoría dentro de `Residence / habitable reconstruction`. Incluye particiones, cerramientos, suelo/techo, vidrio fijo del estudio y hoja real de salida; excluye mobiliario, aparatos, manos, Echo, zonas y triggers. Los nombres son exactos y se exige la presencia de todos los grupos. `Collider.Raycast` evita que un nuevo prop cambie accidentalmente el resultado. No hay propagación por portales, reverberación ni simulación de difracción: es una aproximación espectral al camino directo.

La inspección de sólo lectura del Records guardado encontró **47 BoxColliders activos** correspondientes a todos los grupos exigidos, cero grupos faltantes y cero filtros low-pass existentes. Ambas fuentes son únicas y tienen `BypassEffects=0`; mantienen volumen 0,30/0,10 y rango 1–7 m. Esto verifica referencias de autoría en el archivo, no la ejecución del nuevo componente.

Paso libre **22.000 Hz**; camino obstruido **1.600 Hz** provisional. La transición ocupa **0,20 s** para todo el intervalo, interpolado en frecuencia logarítmica. Así hay un tiempo acotado y se evita que casi toda la variación perceptible ocurra al final de una interpolación lineal en Hz. El paso libre conserva la configuración solicitada de máxima apertura; un filtro a 22 kHz no se describe como bypass matemáticamente idéntico al master.

Sólo escribe `cutoffFrequency`. No escribe volumen, pitch, mezcla, posición, rango, reproducción, potencia ni preferencias. `HouseMemory` sigue cortando el timbre al atender y el frigorífico al retirar su alimentación. No se trata maquinaria, contacto del auricular, pasos ni presencia. No se registra ni reproduce ningún canal del Echo.

La inicialización calcula el estado físico actual sin un transitorio claro al comenzar. Durante pausa no cambian objetivos ni transición; al reanudar consulta la posición real. Deshabilitar el componente devuelve los dos filtros a 22 kHz sin tocar sus fuentes. La hoja de salida se consulta en su pose real; no se inventa un estado acústico de puerta.

## Criterio de integración y límites

Comparar la llegada por el recibidor, la entrada al salón y la aproximación a la cocina, con las fuentes reales y controles existentes. Las propiedades `TelephoneObstructed`, `RefrigeratorObstructed` y los dos `*CutoffHz` permiten observar el tratamiento sin inferir audición. Debe haber paso claro dentro de la habitación, obstrucción contra partición real, transición suave, pausa estable y los mismos cortes de alimentación/repeticiones de antes.

Es especialmente importante comprobar el vano: un único camino directo no representa el sonido que dobla una esquina. La suavización evita saltos inmediatos, pero el corte de 1.600 Hz y su claridad/localización requieren escucha a volumen cómodo. No hay aprobación de miedo, inmersión, timbre, inteligibilidad ni balance por inspeccionar código o medir el filtro.

## Sonda focal preparada, todavía no ejecutada

`staged/HouseRoomAcousticsProbe.cs` se puede copiar a `Assets/_Game/Tests/` y ejecutar con `HouseRoomAcousticsProbe.Run()` en Play fresco de Rooms. Una sola fixture inicial prepara el capítulo anterior, la iluminación completada y la posición de entrada. La entrada a casa, toda la marcha, las miradas, la pausa/reanudación y la llamada final usan Input System; no hay escrituras de cursor, reanudación automática, setters de audio ni nuevas colocaciones.

Ruta: hall `(0,−54)` bloqueado → vano `(0,−52.35)` claro → salón `(−3,−52.35)` → hall `(0,−56)` ambos bloqueados → vano cocina `(0,−58.2)` → cocina `(−3.5,−58.2)` → regreso físico y atención al teléfono. Los estados geométricos de esos puntos fueron consultados por el integrador contra colliders reales, sin mover al Player. La sonda confirma extremos, muestras intermedias y una pausa por Escape durante el cambio. Si la cadencia de cuadros impide capturar el intervalo de 0,20 s, falla explícitamente; no declara cubierta esa pausa.

Produce cámaras, `filter.csv`, `checks.txt` y `result.txt` bajo `Docs/VisualEvidence/2026-10-01-Rooms/house-acoustics/`. Compara por cuadro anclajes, referencias, volumen, pitch, mixer y demás ajustes de las dos fuentes. Al final espera un inicio real de timbre, pulsa E y observa el corte de timbre/frigorífico y luz de cocina bajo `HouseMemory`. La limpieza final pausa intencionalmente la sesión; restaura dispositivos y supresión de guardado. No pretende comprobar la campaña, escucha o descubrimiento humano.

## Integración seleccionada

Runtime, autoría y sonda se copiaron a Assets sin modificar el contrato. Ver `Docs/VisualEvidence/2026-10-01-Rooms/README.md` para alcance, pausa intermedia y límites. No se infiere una partida humana ni audición.

# Echo Protocol — entrega de la campaña ampliada

**Implementada, jugable de inicio a final y verificada objetivamente.** La build macOS final es `build_8317d27c09c9`, Unity 6000.3.21f1. La escena actual es `Assets/_Game/Scenes/Production/EchoProtocol_Campaign.unity`. La edición breve anterior queda conservada como checkpoint.

## Contenido producido

| Tramo | Juego y función dramática |
| --- | --- |
| Protocolo | P1–P3/D1 integrados, reglas deterministas, nuevo aspecto humano del Echo y materialidad clínica. |
| Presión | Una toma mantiene presión; dos válvulas exigen seis segundos de observación cada una. Perder presión o apartarse interrumpe; lo completado persiste. Al cesar maquinaria aparecen sonidos detrás del jugador. |
| La casa | Cuatro ambientes, tres documentos, teléfono y circuitos cocina → dormitorio → pasillo. Regreso por espacio conocido, luces sustraídas, silla desplazada y presencia física cercana. Grabación explícitamente suspendida. |
| Acompañamiento | Tres pausas grabadas y tres coincidencias reales desde un pasillo paralelo. Compuertas y apagados separan visualmente al jugador del Echo. |
| Archivo y salida | Toma actual observada a través del vidrio, carta contextual, conservación íntegra del registro y salida física con cierre. La toma actual no se presenta como prueba de aquella noche. |

Continuación desde el inicio de cada capítulo nuevo, lectura pausada, ayuda, ajustes, reinicio y créditos. El guardado conserva progreso de capítulo; no inventa ni restaura una grabación inexistente. Echo sigue registrando solamente posición y orientación corporal: no voz, intención ni acciones de las manos.

Se produjeron **17 modelos originales en Blender**, fuentes editables, superficies procedurales, una pintura de patio generada con ImageGen y un shader para la figura del Echo. La dirección combina clínica fría, papel/madera domésticos, ámbar y repetición arquitectónica. Es estilizada; no se declara equivalencia fotorealista con el moodboard.

**13 sonidos originales**: siete mecanismos/atmósferas y seis variantes de pasos del jugador. Se conservan los CC0 anteriores; el Echo no reproduce pasos. Posición, espera, pérdida de capas y proximidad sostienen el terror. Mezcla mono, dinámica reducida y volumen cómodo siguen disponibles. Fuentes y derechos: `AUDIO_ASSET_LEDGER.md`, `CREDITS.md` y `ArtSource/Images/Campaign/PROVENANCE.md`. No se generaron voces ni se compraron paquetes o servicios.

## Verificación y correcciones

- **142 comprobaciones** del recorrido integral en Editor: desde título hasta final, 14 RECALLs, física e interacción reales y poses inmutables. La ejecución automatizada duró 522,8 s; no mide una primera partida humana.
- **65 comprobaciones adicionales** de casa/galería/archivo y **30 de continuación**. Las negativas de presión verificaron pérdida de Echo, pausa, mirada interrumpida, toma corta y recuperación con válvula completada conservada.
- Se corrigieron aproximaciones bloqueadas por muebles, el final que podía perderse al atravesar su volumen, el HUD que tapaba el encuentro, la escala de textura perdida al recargar y alineaciones visuales. Las ejecuciones fallidas se conservan y no cuentan como aprobadas.
- Cero scripts faltantes y cero errores/excepciones actuales en Console. Trece WAV de 48 kHz sin clipping; esto no aprueba su actuación perceptiva.
- **Build universal macOS (arm64/x86_64), cero errores**, firma local verificada. No está notarizada. Hay 346 advertencias: 345 de shaders del paquete de inferencia de Unity y una por Pipeline de automatización deshabilitado en Player. El juego no depende de esos sistemas.
- Prueba nativa de menú, inicio, movimiento, RECALL/Echo, ayuda, pausa, ajustes, créditos, reinicio y cierre; log sin errores. La ruta completa se verificó en Editor, no se repitió manualmente entera en la app.
- Tras la ruta integral se refinaron textos/HUD/materiales/carteles y un reloj sin collider. La recarga, presentación, guardados y empaquetado se comprobaron después; no cambiaron las reglas jugables.

Índice de evidencia: `VisualEvidence/2026-09-30-Campaign/README.md`.

## Entrega y resguardo

Aplicación y LEEME: `Builds/EchoProtocol-Campaign-20260930/`. ZIP y SHA-256 están junto a esa carpeta. La build breve anterior y todas las escenas previas se mantienen.

Respaldo inicial externo: `/Users/lautigr116/EchoProtocol-backups/campaign-expansion-start-20260930/workspace-source.tgz`, SHA-256 `51674c10dd445d3d2113f9c7f932873f52dd8647da332b31d32279202c70c7fd`. Comparación posterior: 506 archivos idénticos, 12 cambios deliberados y cero archivos ausentes; las escenas previas, paquetes y assets ajenos permanecen idénticos a ese respaldo. El respaldo final está en `/Users/lautigr116/EchoProtocol-backups/campaign-delivery-20260930/`, con manifiesto y hash propio. No se hizo commit, push ni reescritura de historial.

## Pendientes honestos

**EN PRUEBA humana:** miedo, interpretación de A. Vera/Mara, claridad sin asistencia, ritmo, duración y comodidad de controles/escucha. Prueba recomendada: una partida sin instrucciones externas, auriculares a volumen cómodo, seguida de una pasada breve con parlantes/mono/dinámica reducida. Anotar dónde hubo confusión, tensión, alivio y atribución equivocada al Echo; no subir volumen para provocar impacto. No hay casting pendiente porque esta versión no usa diálogo hablado.

También quedan perfilado en otros equipos, build Windows, notarización para distribución pública y pulido artístico posterior según observación. No se declara duración de 30–45 minutos, aprobación emocional ni acabado AAA.

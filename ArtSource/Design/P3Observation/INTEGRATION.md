# Observation — entrega de autoría

Entrega inicial preservada en estas notas. La fuente actual incorpora la corrección
de atlas y la revisión B posterior a cámaras; ver `Docs/VisualEvidence/2026-10-01-Observation/authoring.txt`.
SHA-256 **del candidato inicial, anterior a esas correcciones**: `84119aef98c12e31f9f2d17fbbcc6ef59d2f5718ee368a766cedf43fa13e7c10`.
Sólo revisión estática: no se ejecutó Unity desde esta tarea ni se verificó imagen,
colisión dinámica o audición del candidato.

Deriva `ArrivalProduction.ScenePath` a
`Assets/_Game/Scenes/Production/EchoProtocol_Observation.unity`; rechaza sobrescribir
escena, arte o reporte existentes. Clave `EP.CampaignCheckpoint.Observation.v1`,
marcador persistente `Observation / paired observation gallery`. Arquitectura y
motores quedan bajo el ciclo de actividad de la galería P3.

**Corrección preventiva comunicada al integrador para su copia:** en
`PrintedIdentifier`, eliminar el `Require` que compara el atlas actual con
`ink.mainTexture`; después de `RequestCharactersInTexture`, asignar
`ink.mainTexture=label.font.material.mainTexture; EditorUtility.SetDirty(ink);`.
El pedido de glifos de tamaño72 puede recrear el atlas dinámico. El integrador incorporó este cambio a ambas copias de la fuente.

## Poses y trayecto

| Conjunto | Destino mundial |
|---|---|
| Trigger A / B | (−5.05,0,35.2 /43), yaw270; tamaño/tiempo/tolerancia originales |
| Cabeza óptica A / B | Giro180 alrededor de (−6.3,0,35.2 /43), sus11 partes agrupadas; lente del lado oriental |
| Puerta y dintel A / B | x−7.8, z35.2 /43; Rigidbody y targets locales conservados |
| Indicador autorización A | (−7.8,3.07,34.96), misma referencia/estado |
| Cruce real R3 | (−7.8,0,43.7), mismo plano y sentido; llegada Arrival intacta |
| Terminal grabación | (−5.05,1.6,44.6), tamaño(2.5,3.2,.2) |
| Núcleo opaco | (−6.3,1.6,39.65), tamaño(.55,3.2,5.3) |
| Motor A / B | (−7.8,3.05,35.2 /43), fijo al cabezal |

R1 recall conserva (−5.7,.03,27.8), yaw315, referencia mundial y puerta de acceso.
Ruta de grabación propuesta: (−5.05,32.5) → A → B, orientar270 en lectores y R.
Ruta de cooperación: (−7.8,32.5) → antes de A(−7.8,34.2) → (−7.8,39.5) →
antes de B(−7.8,42.05) → (−7.8,43.1) → (−7.8,46.3), luego retorno existente.
Los puntos antes de puertas permiten mirar oblicuamente alrededor del soporte del
instrumento; no se plantea observarlo exactamente de frente a través de ese soporte.
Verificar torso1.15m y cabeza1.65m en cámaras reales.

El núcleo mantiene z37..42.3 sin visión continua. El paso oriental mínimo es2.125m
entre sólidos; el zócalo visual reduce localmente a≈2.094m. El occidental es2.525m.
Jambas de puerta dejan2.33m para una hoja2.30m. Los colliders de los dos paños de
vidrio originales siguen íntegros; su renderer se sustituye por base, vidrio,
dintel y soporte. El collider del núcleo original se deshabilita y se reemplaza
por uno explícito acorde a la nueva masa. El muro terminal oriental sella el final
para impedir acceso al cruce R3 desde grabación. No se añaden interactuables.

Las hojas usan una malla biselada original normalizada al Transform heredado
(2.3,2.9,.22), sin multiplicar de nuevo esas dimensiones. Los paneles de relieve
son hijos visuales del Rigidbody existente, sin nuevos colliders o controllers.
Cinco luminarias completas cambian lateralmente de carril; sus intensidades,
referencias y propietarios se conservan. No se agregan luces.

## Sonido y preservación

Sólo dos `SlidingDoorAudio` nuevos. Se heredan masters movimiento/final,
`mechanismVolume`, fade y bus del owner R3 ya trazado. Fuentes físicas en cabezales,
rolloff lineal2..14m, sin doppler, obedecen pausa. Se agregan al final de
`LoopController.resetTargets`, después de las puertas/relay originales: el reset
no debe interpretar el retorno instantáneo del cuerpo como movimiento reproducido.
No se modifica ni mueve el motor donante; no hay clip o canal Echo nuevo.

El autor compara serializaciones originales de referencias, reglas, colliders,
luces, fuentes, rigidbodies y componentes no afectados; comprueba poses ajenas,
estados activeSelf y orden original de reset. Excluye sólo cambios intencionales
(materiales/renderers, key y los dos reset owners). La nueva jerarquía de las
cabezas ópticas hace que sigan el ciclo P3, en vez de mantener piezas bajo Finish.

Selección pendiente del integrador: cámara inicial/post-SE1, ventanas y núcleo,
resolución por input real, intento de carrera desde preview Player y R durante
movimiento. La comprobación estructural del autor no sustituye esos resultados.

## Revisión del integrador

La primera ruta localizada pasó41 checks, pero la imagen B ocultaba parte del cuerpo
y dejaba el lector oscuro. Hoja/header/jambas/guías/motor y montaje óptico B avanzan
.35Z; el trigger B conserva43/yaw270 y R3 conserva43.7. La luminaria oriental40.5
pasa a43.15. Estado de validación y cámaras posteriores en el reporte de Observation.

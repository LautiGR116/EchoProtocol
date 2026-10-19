# Patio real de 6B — factibilidad y contrato de autoría

2026-10-01. Dirección aprobada, todavía sin escena/cámara de este candidato.
Lectura de fuentes y serialización de Arrival; Observation sólo cambia P3.
No Unity, audición ni partida humana desde esta tarea. El plano representa
coordenadas, no una perspectiva validada.

## Solución compacta

Sustituir el conector cerrado de 11.5 m entre vivienda y revisión por una galería
de patio en L al oeste. La casa permanece exactamente donde está. Se conserva
la ventana real del salón, su puerta al recibidor, los objetos y los circuitos.
Portería y todo el final se trasladan rígidamente al lado opuesto del patio.
La nueva marcha ofrece la ventana antes del giro a portería; mirar dentro sigue
siendo una elección. No hay cierre nuevo, mirada requerida, interacción ni espera.

| Punto de marcha X,Z | Función |
|---|---|
| (0,−62.5) | Hoja doméstica existente, misma llave y apertura |
| (0,−63.9) | Librar físicamente la punta de la hoja abierta antes de girar |
| (−6.3,−63.9) | Giro amplio por la parte trasera de la casa |
| (−6.85,−54.2) | Ventana entra en el campo anterior derecho al caminar norte |
| (−6.85,−52.6) | Continuar hasta el acceso lateral a portería |
| (−8.4,−52.4) | Portal final, ahora abierto hacia el patio |

Los puntos hasta el portal suman **20.58 m**, frente a 11.5 m hasta el portal
anterior. Diferencia 9.08 m, unos **2.27 s** a moveSpeed=4 serializado, sin contar
giros/observación humana. El recorrido interior de revisión conserva longitud y
reglas. La franja occidental mide 3 m brutos, unos 2.8 m entre caras; no contiene
muebles ni pilares. La esquina sur conserva 2.1 m brutos. La hoja existente abre 95° y su punta llega
a z≈−63.48: el giro occidental pasa por z−63.9 para librar esa geometría real.

Punto diagnóstico libre: cámara (−6.85,1.63,−52.4), mirando aproximadamente este
a la pared real del recibidor (.91,.95,−52.27). El rayo atraviesa la ventana cerca
de (−5.4,1.50,−52.376) y la puerta del salón en x−1,z≈−52.30. Es una línea
analítica coherente con los vanos, no un Collider.Raycast ejecutado ni prueba de
visibilidad final del tejido/marco. El hall ilumina esa pared desde su fuente
actual (.68,2.05,−50.5), a unos 2.1 m; no se representa mediante otra lámpara.

El marco original tiene ancho exterior 1.64 m y claro 1.46 m; sus cortinas dejan
una zona central. La ventana no se ensancha ni se mueve. La vista elegida pasa
por debajo del travesaño de 1.58 m. Si la cámara demuestra obstrucción, revisar
esa composición antes de cambiar la casa o atribuir un estado visible.

## Arquitectura y colisión

Conservar la salida doméstica y todas las paredes interiores. Retirar únicamente
los objetos `Service connection floor/ceiling/wall`, sus zócalos/cornisas y el quad
occidental `Courtyard opposite facade` en (−9.4,2.5,−52). El quad oriental queda.
La luminaria `Service passage fixture` y `Residence Fluorescent` de z−68.5 viajan
juntas a (−7.15,2.45/2.60,−57.6), bajo un alero pequeño fijado al muro del patio.
Mismos Light, color, intensidad y rango; ninguna fuente de audio nueva.

- Suelo sur: x[−8.4,1], z[−64.6,−62.5], cara superior y=0.
- Suelo oeste: x[−8.4,−5.4], z[−62.5,−50.5], cara superior y=0.
- Límite sur z−64.6; límite este x1 desde z−64.6 a−62.5.
- Límite oeste x−8.4 desde z−64.6 a−54.9. Desde ahí la fachada del final
  trasladado cierra hasta el borde norte; su portal queda centrado en z−52.4.
- Cierre norte del patio z−50.5, x[−8.4,−5.4]. No se comunica con Presión.
- Completar sólo el cascarón trasero que antes no se veía: pared x−5.4 entre
  z−62.5 y−60.5; pared posterior z−62.5 entre x−5.4 y−1. El espacio interior
  jugable conserva su pared trasera z−60.6; no se abre otra habitación.
- Revestimiento exterior de yeso/pintura mineral sobre la cara oeste existente,
  respetando el vano z[−52.9,−51.1]. Evita ver papel floral en la fachada.
- Vidrio físico sellado en x−5.4, y[.86,2.46], z[−52.9,−51.1]. El modelo
  original conserva sus vidrios/renderers; este collider coincide con el vano,
  cierra sus márgenes y sirve al tratamiento acústico explícito.

Cada nueva masa visible tiene collider independiente equivalente donde es
transitable; revestimientos/alero y drenaje son sólo visuales. No hay bloqueador
invisible en un vano abierto. El patio tiene cielo visible y paredes terminadas;
no conserva un plano fotográfico a distancia de contacto.

## Traslado rígido de todo el final

Agrupar, con identidad y `worldPositionStays=true`, los roots completos:

1. `Threshold / del otro lado` — construcción, review, origen, zonas, hoja,
   interacciones, luces, fuentes, mecanismos y tramo de salida.
2. `Porter / inhabited conservation station` — muebles, monitor/display,
   lámparas y las tres luces añadidas después, fuera del root anterior.

Pivote anterior P=(0,0,−74); nuevo P′=(−8.4,0,−52.4); giro yaw+90°.
Para todo punto: **T(x,y,z)=(z+65.6, y, −52.4−x)**; rotaciones Q′=yaw90×Q.
No multiplicar dimensiones, cambiar pivotes de puertas o reescribir sus targets.
Agrupar antes de girar mantiene la base local de los Rigidbody y sus componentes.

| Referencia física | Nueva posición XYZ |
|---|---|
| `6B review origin` / Director.origins[3] | (−10.4,.03,−52.4), yaw270 |
| `Review origin instrument` | (−10.1,1.45,−54.75) |
| `6B review door / hinge` | (−13.4,0,−51.92) |
| `Inside 6B / actual witness` | (−12.45,1,−52.4), yaw90 |
| `6B outside stillness` | (−14.65,1,−52.4), yaw90 |
| `Facing the front of 6B` | (−13.4,1,−52.4), yaw90 |
| `Original conservation terminal` | (−17,1.6,−54.56) |
| `Return key to porter's tray` | (−18.02,.93,−51) |
| `Porter exit door mechanism` | (−18.4,0,−52.4) |
| `Actual departure after returning key` | (−22.4,1,−52.4), yaw90 |

El final ocupa aproximadamente x[−24.4,−8.4], z[−57.1,−47.9]. Presión termina
en x−8: queda separación física entre ambos conjuntos. Los volúmenes se rotan
con sus transforms, no se convierten en AABB. Se conservan tamaños de zonas,
pausa 2 s, observación .6 s, material visible explícito, fade y separación corporal.

Hay fuentes **sueltas dentro del root Threshold**, no hijas de sus puertas:
`6B review hinge sound`, `6B review latch`, `Final current-world contact` y
`Porter exit physical sound`. Los dos roots completos las incluyen. No mover
globalmente audio, Player, HouseMemory, Echo prefab, mixer, presupuesto de
sombras ni materiales compartidos. El autor deberá fallar si una referencia
espacial del final está fuera de esos roots.

## Cambios de integración separados

**Runtime mínimo pendiente del integrador:** `CampaignRoomTone` usa z<−74 para
su nivel de revisión .20. El traslado dejaría ese tramo en .055, aunque todo lo
demás funcione. El integrador confirmó que añadirá un BoxCollider opcional
`finalReviewRegion`, con prueba local, que selecciona .20 sólo dentro del final;
legacy sin referencia mantiene exactamente sus bandas. KeyReturned continúa
teniendo prioridad .015; ni motor aislado ni fluorescente pueden resucitar.
El autor exige ese campo antes de modificar una escena y luego lo conecta.

**Sólo serialización de escena, sin runtime nuevo:**

- `HouseRoomAcoustics.architecturalPartitions`: retirar referencias a piezas
  del conector desactivadas, añadir las nuevas masas reales y el vidrio exterior.
  Hoy la ventana de salón no forma parte de las 47 referencias. Los dos filtros,
  dueños, niveles y fuentes permanecen. Un teléfono no atendido podrá oírse desde
  más cerca, filtrado por vidrio; no atribuirlo al Echo.
- `LightShadowBudget.enclosedResidence`: ampliar al patio (x hasta−8.55, z hasta−64.75), sin
  convertir sombras en luz nueva. Añadir a `residenceLights` las dos luces del
  final cuyos rangos alcanzan el patio: `Inside final practical` y
  `Review instrument task light`; evitar fugas de su nuevo muro vecino.
- Clave nueva `EP.CampaignCheckpoint.Courtyard.v1`. Director.origins[3] conserva
  su referencia al origen movido. Guardado/Continue consultan esa referencia,
  no una coordenada fija. R sigue creando únicamente la toma real de la nueva
  ruta; no se transforman grabaciones de otra escena.

`FinalDoorMemory.Inside`, facing, rayos, HouseSwingDoor y SlidingDoor trabajan con
refs/bases locales y admiten el giro. `SessionAudio` usa Fade, no Z. La rama
legacy de CampaignDirector con coordenadas−115/−125 no gobierna el final activo
de esta edición. El único hardcode ambiental relevante hallado es RoomTone.
Los probes ThresholdCampaignProbe/FinalDoorRouteProbe sí usan waypoints y yaw
absolutos: el integrador debe transformarlos por T/+90 sólo en la rama Courtyard,
incluyendo fixtures iniciales, abertura temprana, observación y salida.

Riesgo audible existente que ahora queda más próximo: `HouseMemory.LeaveHouse`
detiene el teléfono al fijar el origen final. Se conserva por ahora; no puede
afirmarse que ese corte pase inadvertido sin escuchar. No alterarlo mediante
otra fuente o un escritor de volumen. El frigorífico conserva su potencia real.

## Criterio de primera selección

Primero cámaras/control por la ruta corta: salida doméstica → esquina → ventana
frontal oblicua → vista libre por salón → portal de revisión. Comparar la misma
vista con reserva en Hall y fuera de Hall, sin cambiar ambiente global. Ver
materia iluminada real, no sólo un booleano, y descartar que el rebote frío tape
la diferencia. Confirmar colisión en bordes, vuelta libre a casa antes de Review
y conservación de la escena final trasladada. La ventana puede ignorarse y el
final sigue siendo accesible. No se afirma reconocimiento espontáneo, miedo, comodidad ni audición.

## Entrega estática

`HouseCourtyardProduction.cs` deriva Observation, no sobrescribe salidas, conserva
por comparación las serializaciones existentes y comprueba el traslado rígido
de cada transform original. Agrega sólo masas/colisión, dos materiales locales,
la región ambiental y referencias arquitectónicas; no agrega luces ni audio.
El código requiere compilación/autoría/cámara por el integrador.

`plan.svg` contiene planta y recorrido; `reference-poses.json` conserva la lectura
focal de los objetos serializados en Arrival. No incluye interiores de prefab y
no sustituye la inspección de referencias que hace el autor mediante Editor.

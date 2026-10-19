# Courtyard — salida junto a la casa real

**Implementada y comprobada objetivamente.** Sucesora de Observation. La salida doméstica conduce a un
patio lateral y a la portería trasladada íntegramente. Se puede mirar el teléfono
y el recibidor reales por la ventana existente y volver antes de iniciar la
revisión. No hay mirada obligatoria, figura nueva, duplicación de casa ni canal
nuevo del Echo. La luz depende de la decisión doméstica ya tomada.

`authoring.txt` registra el traslado rígido y4189 serializaciones preservadas.
`refinements.txt` registra la corrección por cámara: el primer exterior era
excesivamente negro; se añadieron dos contribuciones locales suaves del cielo,
pavimento con juntas y un sistema continuo de canaleta/bajante/desagüe. Los
masters originales y recetas quedan en ArtSource; el suelo físico permanece.

Una fuente3D exterior provisional usa el master CC0 trazado en el ledger. Sus dos
regiones, particiones y dueño de ganancia no modifican sonidos de la casa ni
RECALL. `CampaignRoomTone.finalReviewRegion` mantiene la mezcla de portería en
su nuevo marco local. Escucha, comodidad y selección tímbrica **EN PRUEBA**.

## Evidencia disponible

- `first-layout/courtyard-hall`: PASS21,37.472s, una fixture inicial y cero resets
  posteriores. Ruta física con recibidor encendido. Primeras cámaras muy oscuras.
- `first-light/courtyard-bedroom`: PASS24,42.270s, una fixture inicial y cero resets.
  Traslado real de reserva al dormitorio; primer candidato de luz exterior.
- `checkpoint-all.*`: PASS341,10 cargas frescas, cinco fixtures declaradas,
  cero reanudaciones ocultas. Conserva general/recibidor/cocina/dormitorio/llevada,
  restaura portería y permite tomas nuevas mediante movimiento/R. Preferencias
  originales restauradas. Incluye fuente exterior muda en Continue. Anterior al
  kit visual; no es recorrido continuo.

Las cámaras se obtienen mediante Render desde el Player y direcciones de inspección
predefinidas; no prueban reconocimiento espontáneo ni incluyen OnGUI. Tiempo de
pruebas automáticas no equivale a duración humana. El resultado de campaña/build y el límite nativo se registran abajo; Observation
se conserva como respaldo anterior.

## Resultado seleccionado

La rama dormitorio seleccionada pasa30 comprobaciones,44.085s automáticos, una
fixture inicial declarada y cero resets posteriores. Incluye volver libremente a
la casa, pausa real por Escape con cursor de audio detenido, fuente exterior fija
y muda en casa/portería. La rama previa de recibidor con audio pasa27,40.154s; sus
cámaras de suelo anterior están expresamente bajo `pre-winding-correction/`.
Los primeros suelos combinados se rechazaron por winding invertido y se corrigieron
por Editor API; `selected-mesh-diagnostic.txt` registra12852/46548 triángulos
coherentes con sus normales exteriores. La campaña siguiente usa esos assets.

Campaña continua **PASS480**, 285.498s automáticos,
4 tomas inmutables y11500 frames contrastados,
cero fixtures y cero reanudaciones ocultas. Comienza en el título y llega al final
con el recibidor realmente encendido, cruzando el patio y todo el final trasladado.
Sus vistas exteriores permiten comparar la casa con la rama dormitorio; las
direcciones son de inspección, sin atribuir mirada espontánea.

Build `build_399e7e7f4ac5`: Succeeded,206439729B/24176ms,
0errores/346warnings. Universal Intel/Apple Silicon y firma local
estricta comprobados. Estado nativo: Prueba breve en la app exacta: título, créditos completos con ahriik/CREDITS.txt, entrada WASD que inicia el registro, RECALL de 7.0 s, pausa Escape, retorno y salida Cmd-Q comprobados en la interfaz. No se recorrió la campaña completa en nativo ni se juzgó la mezcla por oído.

Timbre, mezcla espacial, comodidad, claridad de primera partida, reconocimiento
y miedo siguen **EN PRUEBA**. Esta build es un checkpoint de producción sostenida.
Siguiente avance en estudio: la estancia SE2 transitable, conservando el Echo y
el regreso de SE3. No está implementado en Courtyard.

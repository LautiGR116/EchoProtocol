# P3 — una sala de observación con dos recorridos

Propuesta de dirección, 2026-10-01. Sólo inspección de imágenes, fuentes y escena
serializada; sin Unity, modificación de Assets ni modelo nuevo. Derivar un futuro
sucesor del checkpoint Arrival cuando esté conservado. No se acepta todavía una
composición ni un resultado humano.

## Carencia sustentada

En `2026-10-01-Orientation/r1-input-survey/06-gallery-entry.png`, el bloque negro
que ocupa el centro es **la hoja cerrada P3 Gate A alternative route**, no el
tabique central. El objeto tiene2.3m de ancho×2.9m de alto y material antiguo
`M_VS1_VT1_Trim`, BaseColor(.02,.03,.04), metallic.32. La salida R1 x−5.5 apunta
al carril de servicio x−5.05 y a esa hoja z35.2. El primer recorrido útil de A→B
queda lateralmente al oeste, x−7.8. La imagen muestra el lector A de perfil,
parcialmente escondido por la puerta. Esto es una debilidad de composición y
jerarquía espacial, no evidencia de incomprensión humana.

`Shutdown/continuous/campaign-reader-P3-A-aligned-Camera.png` y su pareja B
muestran instrumentos ya legibles, pero fondo repetido de muro y fluorescente.
El vidrio A/B sigue siendo realmente transparente (ClearBarrier, alpha.055);
el muro plano al otro lado y la ausencia de marco/zócalo visible en esas tomas
hacen que la separación se lea poco. No es una sustitución accidental por pintura.
Las cámaras de lectores no documentan la visión del Echo desde el carril contrario.

`RecombinationRelay` mantiene la regla aprobada: Player ofrece preview; sólo Echo
A retiene autorización, Echo B sostiene la segunda puerta, y terminar la toma o R
lo reinicia. `EXPERIENCE_MAP.md` reserva los5.3m opacos para perder la observación
continua. Ambos aspectos tienen valor; no deben borrarse para hacer un espacio
completamente abierto.

## Dirección recomendada

**Intercambiar la posición de los carriles y reconstruirlos como una sala de
observación longitudinal y su circulación de servicio.** La entrada existente de
R1 sigue donde está: desemboca directamente en el recorrido de grabación oriental,
con A al lado y B más adelante. Después de R, el jugador elige el paso occidental:
primero ve su Echo por A, lo pierde tras un cuerpo arquitectónico cerrado de5.3m,
y vuelve a encontrarlo sosteniendo B. Ninguna nueva mirada obligatoria, susto,
voz, temporizador o dato grabado.

Se conserva el perímetro x[−9.2,−3.8], el vestíbulo z30.2..34, las cotas z de
A/B, el paso final y el archivo. El cambio principal es arquitectónico/topológico
lateral, no una nueva mecánica. El cuarto sigue siendo una instalación que observa
recorridos humanos: dos ventanas selladas con antepecho y dintel, separadas por un
núcleo técnico opaco; dos hojas verticales en portales completos, con guías y
hueco superior coherente, en vez de rectángulos negros aislados. No llenar los
carriles de escritorios ni repetir la utilería del archivo.

### Esquema de primera autoría

| Elemento | Actual | Propuesto |
|---|---|---|
| R1 estación/recall/puerta norte | Pose mundial actual | Exactamente igual |
| Player/Echo grabando A | (−7.8,35.2), yaw90 | (−5.05,35.2), yaw270 |
| Player/Echo grabando B | (−7.8,43), yaw90 | (−5.05,43), yaw270 |
| Puerta A / ruta de cooperación | (−5.05,35.2) | (−7.8,35.2) |
| Puerta B / hold final | (−5.05,43) | (−7.8,43) |
| Cruce Player final R3 | (−5.05,43.7) | (−7.8,43.7); mismo planoZ/sentido |
| Tabique central opaco | x−6.3, z37..42.3 | Mismo intervalo y bloqueo |
| Ventanas A/B | x−6.3, z34..37 /42.3..44.6 | Mismo plano sellado; vanos con base/dintel reales |
| Terminal del carril grabable | Oeste, z44.6 | Este, z44.6; sellar x−6.3..−3.8 |
| R3 permanently unlocked exit | (−17.2,46.4) | Igual |

Son coordenadas de intención, no receta ya validada. La sala conserva altura3.2m.
Al oeste hay aproximadamente2.7m entre paredes; una hoja2.3m conserva su ancho y
se completa con jambas visibles, sin estrechar la circulación. El carril oriental
queda cerca de2.3m. Las dos áreas siguen separadas físicamente: sin abrir puertas
laterales entre ellas ni añadir pasos por el norte del carril de grabación.
Los lectores, carcasa/lente/anillo/identificador y su pose requerida deben viajar
como conjuntos explícitos; conservar sizes, acceptedActors, .35s, tolerancia y
las referencias activas, no simular su estado con nueva lógica.

El primer ensayo de cámara debe mostrar desde la entrada R1 un pasillo abierto
a la derecha del núcleo, A en su cara visible, y la puerta de servicio de A al
otro lado del vidrio. Desde cooperación, cabeza/torso de Echo deben verse por A
y por B a distancia mayor al fade; el centro sigue oculto por materia real.
Reconstruir la presentación del cierre cambia su material/relieve y guías; el
Rigidbody y los targets locales de SlidingDoor conservan movimiento y autoridad.

## Sonido causal, dentro de esta misma intervención

La escena actual no tiene `SlidingDoorAudio` ni `OpticalReaderAudio` asociados a
los P3 A/B (sí existen para P1/P2 y las tres hojas R3). Esto es ausencia de owners
verificada por serialización, **no una escucha**. Los dos mecanismos P3 pueden
usar el mismo componente/masters trazables ya integrados para R3, en cabezales
fijos, con nivel conservador a comparar. Así la apertura B es una consecuencia
localizable incluso durante la pérdida visual central; no es sonido reproducido
por el Echo ni un canal nuevo. Si se añade el componente, registrarlo en los
resetTargets explícitos y comprobar que R no reproduce movimiento ficticio.
No hace falta añadir otro bed, música o evento sonoro.

## Beneficio, coste y riesgo

Beneficio esperado: el primer recorrido nace de la puerta de entrada sin presentar
primero un bloqueo; el segundo se reconoce como el otro lado de una sala concreta.
El mismo ejercicio A→B convierte la separación visual en una experiencia espacial
legible. Es una hipótesis de percepción más valiosa que volver a retocar sólo
carteles, color o intensidad.

Coste medio: una autoría sucesora, grupos de dos lectores/dos puertas, nueva
carpintería arquitectónica pequeña, muro terminal y cruce R3 desplazados2.75m,
y adaptación lateral de probes. No requiere nuevos sistemas o paquete/modelo
grande. Puede arrancar con mallas simples y materiales actuales antes de decidir
si alguna pieza original merece Blender.

Riesgo principal: el swap no debe permitir correr desde un preview Player A al
otro carril antes de que cierre la puerta; conservar z del comienzo del separador,
distancia lateral entre poses y puertas, velocidad/tolerancia y comprobar la
imposibilidad física con un intento real. Tampoco permitir llegar a R3 por detrás
del cierre B. Cambiar los vanos no debe dejar huecos de collider ni tapar con
marcos el torso que se pretende ver. La legibilidad del vidrio/Echo se decide con
la iluminación post-SE1 real, no una fixture de luz inicial.

Alternativa menor: conservar carriles y sólo vestir la puerta A como cierre físico.
Es barata, pero mantiene la entrada apuntando al recorrido inicialmente bloqueado
y A lateral. La reservaría si la primera cámara del swap demuestra que exige
rehacer R1 u otro espacio; no empezaría una tercera variante de patio/atrium.

Fuentes: BuildR2.cs y FinishGeometry.cs en `Docs/VisualEvidence/2026-09-12-R2/Source`,
escena Arrival serializada, RecombinationRelay.cs, OpticalReader.cs y cámaras
citadas. No audición, prueba espontánea, miedo ni confort humano evaluados.

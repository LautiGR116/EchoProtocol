# Echo Protocol — reconstrucción de interacción, presencia y cooperación

**Reconstruction, 2026-10-02.** Escena independiente:
`Assets/_Game/Scenes/Production/EchoProtocol_Reconstruction.unity`.
Deriva de EncounterArt, cuya app, ZIP y fuentes recuperables se conservan.
App y ZIP: `Builds/EchoProtocol-Reconstruction-20261002`. Firma local y paquete
comprobados; recuperación fuente externa descrita abajo.

| Área | Cambio perceptible |
| --- | --- |
| Interacción | Un único objetivo para retícula, texto y E. Cobertura física del teléfono y su auricular móvil; tolerancia pequeña con obstáculos comprobados. Pulsar y mantener se distinguen; pausa y RECALL limpian el foco y exigen soltar E. R tiene prioridad ante E simultánea. |
| Echo | Anatomía adulta reconstruida, mandíbula/nariz/orejas/párpados, manos proporcionadas, postura menos abierta, ropa continua y superficies separadas. Base gráfica MakeHuman CC0 trazada; pose y acabado derivados en Blender. Se revisó de cerca, de perfil, por detrás y moviéndose en P1, además de SE2 y 6B. |
| P1 | A conserva una lectura; B exige presencia real. El Player debe coincidir en C durante 0,65 s mientras su Echo mira B. Llegar tarde conserva A pero no inventa una presencia pasada. Una nueva toma puede ampliar la pausa. |
| P3 | Dos recepciones desde el otro lado del vidrio: el Echo aporta presencia y el Player confirma A y B con E. Cada enlace aceptado asegura su apertura hasta RECALL. Reemplaza la espera larga en B y la carrera final por dos ventanas planificadas. |
| Orientación y sonido | Placas físicas de reglas/estado, paneles de recepción con pilotos y contactos discretos, luces locales y una guía de suelo en P3. Se conserva la sustracción ambiental de SE2/6B y su ventilación fija. |
| Pistas | Primera nota de atención y segunda de relación, sin contar pausas/documentos. El estado y los intentos determinan la espera; no se repiten en la misma etapa. H abre notas pausadas; J o botón solicita más hasta los pasos explícitos. Se pueden desactivar en pausa. |

La figura continúa siendo una representación **estilizada y estática**. Reproduce
sólo posición y orientación corporal registradas; no se añadieron gestos, mirada,
voz, pasos ni acciones atribuidos a una grabación que no los contiene.

## Evidencia técnica

- Campaña de título a final mediante Input System: PASS, cuatro tomas inmutables,
  cero colocaciones de prueba y cero reanudaciones ocultas. Los 24 despachos de E
  coinciden con el objeto presentado por el HUD. Observador ambiental PASS.
- Pruebas separadas: alcance/obstáculos/cambio de objetivo, teléfono y reserva;
  P1 fallido y replanificado; P3 rechazos, pausa, dos confirmaciones, RECALL y
  salida después de terminar el Echo. Las fixtures se declaran en cada resultado.
- Pistas con tiempos de producción y R reales: atención anticipada tras dos
  reintentos y relación tras el tercero; pausa/documento excluidos, petición
  explícita y desactivación sin repetición. Sin fixtures de reloj.
- Auditoría de referencias, shader y escena guardada PASS. La revisión detectó y
  corrigió placas de canto en P1 y una referencia de pistas a un loop antiguo.
  Las pruebas posteriores cubren ambas correcciones.
- Cinco estados domésticos y diez cargas nuevas: PASS341. Aislamiento en marcha
  o confirmado durante vaciado y cuatro cargas: PASS45. Preferencias restauradas.
- Build universal Intel/Apple Silicon, firma local estricta y ZIP verificados.
  Cero errores; una advertencia de Pipeline desactivado en Player, deliberado.
  Nativa: inicio, movimiento, toma37,6 s/RECALL, H/J, pausa, créditos y cierre;
  log sin excepciones. No es una campaña nativa completa.
- La notificación de pérdida/recuperación de foco y E sostenida pasan en una
  prueba con fixture explícita. **El cambio real de aplicación sigue pendiente**:
  CUA envía teclas en segundo plano y no permitió establecer esa transición.
  Tampoco se afirma click nativo comprobado en el interruptor de pistas.
- Fuentes: `../EchoProtocol-backups/reconstruction-checkpoint-20261002/`, con
  manifest, verificación e instrucciones. Recuperar Candidate → Legibility →
  EncounterArt → Reconstruction en un directorio vacío. ZIP y escena de
  EncounterArt conservan sus hashes. No se extrae sobre el trabajo actual.

[Comparaciones, resultados y alcance exacto](VisualEvidence/2026-10-02-Reconstruction/README.md).
Los recuentos incluyen comparaciones de frames; no son una medida de calidad.
No se hicieron commits, se reescribió historial ni se descartó trabajo previo.
No hubo compras, servicios de voz ni nuevos frameworks.

## Pendiente humano — EN PRUEBA

Una primera partida sin guía debe decidir si el nuevo cuerpo resulta creíble e
inquietante, si P1 enseña la coincidencia, si P3 permite planificar sin ensayo
ciego, si el foco se siente estable y si las pistas llegan cuando ayudan.
También faltan escucha, balance, localización, comodidad, ritmo, duración y
miedo. Probar auriculares a volumen cómodo y parlantes, incluida dinámica
reducida. La ruta automática y las capturas no demuestran esas cualidades.

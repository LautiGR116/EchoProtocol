# Folio — candidata de identidad gráfica

**Candidata Folio seleccionada.** Parte de Resonance y
conserva su app, ZIP y recuperación. Escena:
`Assets/_Game/Scenes/Production/EchoProtocol_Folio.unity`.
Guardados propios: `EP.CampaignCheckpoint.Folio.v1`.

El cambio perceptible es una interfaz que pertenece al archivo y a las cartas
de la historia. Cormorant Garamond da voz a títulos y fragmentos; Plex Sans
ordena controles, subtítulos y señales; Plex Serif acompaña la correspondencia
personal. Papeles cálidos/fríos, fibra tenue y un pliegue doméstico conservan el
texto vivo. Inicio, pausa, ajustes, ayuda, créditos, final y documentos comparten
márgenes y estados. El HUD separa interacción, pistas y subtítulos. Se corrigieron
cancelación de reinicio, foco cuando desaparece el control de más pistas y
navegación inversa con modificadores nativos recibidos como banderas IMGUI.

[Guía breve](FOLIO_IDENTITY.md) ·
[Cinco comparaciones antes/después](VisualEvidence/2026-10-08-Folio/index.html) ·
[Hoja de contacto](VisualEvidence/2026-10-08-Folio/comparison.png).
Las anteriores usan el renderer legado en estado equivalente de Folio; las
señales del mundo ya están en Plex. Las capturas de UI emplean fixtures explícitos.

## Comprobación objetiva

- Interfaz nativa final: **PASS130**, ventanas reales 1280×800, 1440×810 y
  1280×720. Tab/Shift+Tab/flechas, Enter/Espacio, volumen, opciones, pausa,
  créditos, ayuda, lectura, documento de varias páginas, cierre y reanudación.
  La carta inicial y el consentimiento entran completos; el fixture largo
  permite alcanzar su último párrafo. Se registran dimensiones y fotogramas.
- Primera campaña nativa (antes de las correcciones finales de foco y cinco
  señales inicialmente ocultas): **PASS575**, cuatro tomas inmutables, cero colocaciones
  y cero reanudaciones ocultas. La captura de audio de esa ruta perdió 3371
  bloques de copia y no demuestra captura íntegra. Un intento posterior fue
  detenido por pérdida de foco; no se ocultó ni se reanudó por API.
- Los dos intentos nativos de la escena final y el intento de Editor terminaron
  **FAIL por pérdida de foco/pausa**, sin reanudación oculta; no se afirma campaña
  final completa. El segundo segmento de audio sí verifica cero bloques perdidos
  y cero clipping; no prueba la mezcla final entera. Las cinco señales dinámicas
  se comprobaron en configuración y vistas explícitas cercanas de Play Mode,
  descartadas al detener el Editor. No sustituyen aproximación normal del jugador.
- Controles reales de la app de entrega: Shift+Tab/Tab, inicio, ayuda completa,
  movimiento, RECALL, pausa, cancelar reinicio, ajustes por mouse, créditos y salida.
  Comprobación acotada en recepción; no aprobación humana ni campaña completa.
- Preservación: **15105 objetos originales idénticos**. Sólo cambian font/avance/
  interlineado/material de 56 señales, identidad de sesión, clave de edición y
  un nuevo objeto de estilos. Luces, soportes, física y referencias de puzzles
  conservan sus bytes. 339 fuentes/assets de audio/settings existentes coinciden
  con el manifiesto de Resonance; las excepciones son cinco renderers de UI y
  la selección de escena de build. Audio, Echo, controles y puzzles no se reescriben.
- Mapas Unicode de los seis archivos incluidos: ningún carácter requerido
  ausente. Licencias OFL, fuentes originales sin modificar, revisiones y hashes
  trazables. No servicio de pago ni nuevo framework.

Detalle y fallos conservados:
[Índice de evidencia](VisualEvidence/2026-10-08-Folio/README.md).

## Entrega y recuperación

Destino de app: `Builds/EchoProtocol-Folio-20261008/EchoProtocol.app`.
Destino de ZIP: `Builds/EchoProtocol-Folio-20261008.zip`.
Build final: cero errores y una advertencia; Console de Editor sin errores.
Comprobados Intel/Apple Silicon, firma local estricta, exclusión
de QA, 205 archivos/licencias del ZIP y su extracción. Firma ad hoc, no notarizada.
ZIP: 90.965.959 bytes; SHA256:
`ca6b0676b3a1ecafd765d290f767e450444e3b065290b9693e3e1aa2d979d12f`.

Respaldo fuente:
`/Users/lautigr116/EchoProtocol-backups/folio-checkpoint-20261008/`.
`RECOVER.txt`, `manifest.json` y `verification.json` describen ocho capas en orden:
Candidate → Legibility → EncounterArt → Reconstruction → Contraprueba → Lighting
→ Resonance → Folio. Restaurar en directorio vacío; no sobre trabajo sucio.
La comprobación lee los bytes de todas las capas y compara cada archivo final.
No incluye cachés, builds, autenticación ni guardados de máquina.

## Juicio y prueba humana pendiente

La revisión visual del agente selecciona esta dirección: separa mejor la voz
personal del sistema, reduce ruido y evita competir con la lectura. Esa selección
no prueba miedo ni comodidad prolongada. La textura es deliberadamente tenue;
no se busca que una marca de papel actúe como susto.

Hacer una primera partida sin guía externa, a volumen cómodo. Juzgar lectura
sostenida de cartas, descubrimiento de foco/scroll, legibilidad periférica,
orientación por señales, mouse y si la ayuda interrumpe la tensión. Escuchar
localización, interpretación vocal y contraste con mono/dinámica reducida cuando
corresponda. Ritmo, inmersión y miedo de toda la campaña siguen **EN PRUEBA**;
no hay feedback ni aprobación humana nuevos. Las cifras automáticas no sustituyen
esa partida ni prueban la duración objetivo de 30–45 minutos.

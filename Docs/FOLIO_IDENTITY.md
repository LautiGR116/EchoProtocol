# Folio — identidad gráfica

Dirección implementada sobre Resonance: un archivo preciso contiene huellas
personales. Grafito, marfil y salvia para el sistema; cobre reservado a la
correspondencia de Mara. La trama de fondo acompaña, no transporta información.
Los papeles tienen fibra tenue; el personal agrega un pliegue y borde cálido.
No se simulan textos ilegibles, manuscritos artificiales ni sellos de autoridad.

| Función | Fuente | Regla |
| --- | --- | --- |
| Título y encabezados | Cormorant Garamond Semibold | 44–82 px; pocos niveles, espacio alrededor |
| Fragmentos narrativos | Cormorant Garamond Italic | 28–35 px; frases breves |
| Controles, HUD, archivo institucional | IBM Plex Sans Regular/Medium | 16–23 px; metadatos 13–14 px |
| Carta personal | IBM Plex Serif Regular | 22 px, avance vertical 31 px y separación de párrafos |
| Señalética física | IBM Plex Sans Medium | 56 señales de campaña, incluidas cinco que aparecen durante la progresión; conserva soporte, posición y función |

Medidas referidas a la composición base 1280×720. La escala uniforme se centra
en otras ventanas; 16:10 mantiene margen vertical. Ajustes con porcentaje y
estado escrito, foco por regla lateral, hover y pulsación con cambios de fondo.
Tab/Shift+Tab o flechas recorren controles; Enter/Espacio activa; izquierda/derecha
ajusta volumen en 5%. Esc vuelve desde ajustes y créditos; cancelar reinicio
conserva pausa. Transición visual de 120 ms, sin bloquear entrada.

Cartas y documentos conservan texto real y original. El cuerpo se mide con su
fuente y pasa a un visor con rueda, flechas, PageUp/PageDown y Home/End si excede
el espacio. La carta inicial entra entera. Los documentos institucionales y
personales comparten márgenes, pero distinguen tinta/material y voz editorial.

HUD con superficies opacas de contraste: subtítulo abajo, pista encima e
interacción antes de ambos. Estados de registro arriba; la reserva de luz es un
estado separado. El sistema consume el estado de los componentes existentes;
no inventa objetivos ni canales del Echo. La interfaz conserva IMGUI y una
identidad de escena (`FolioUI`), con estilos almacenados y referencias explícitas.

Se compararon Cormorant, Barlow Condensed y Plex Serif sobre el menú en ejecución.
Cormorant distingue mejor archivo y recuerdo; Barlow acercaba el juego a una
convención de thriller, Plex Serif separaba menos el título de las cartas.
Las comparaciones iniciales son Game View a 2940×1678 redimensionadas; las pruebas
finales se hacen en ventanas nativas, con medidas reales registradas. Esta
selección es juicio visual del agente, no aprobación humana.

Fuentes, licencia y procedencia: [CREDITS.md](CREDITS.md) y
`ArtSource/Design/Folio/Fonts/`. Glyph audit lee los mapas Unicode de los TTF
incluidos, sin depender de reemplazo del sistema. La comodidad sostenida,
lectura periférica, mouse y respuesta emocional requieren una partida humana.

El foco conserva el modificador de Shift+Tab recibido por IMGUI además del
estado de teclado, para entradas nativas cuyo modificador llega como bandera.
Las flechas ofrecen la misma navegación inversa.

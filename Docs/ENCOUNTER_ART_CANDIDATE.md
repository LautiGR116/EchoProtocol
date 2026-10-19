# Echo Protocol — SE2, 6B y presencia del Echo

**Candidata seleccionada, 2026-10-02:** `EchoProtocol_EncounterArt.unity`, derivada
de Legibility mediante Unity Editor. App y ZIP:
`Builds/EchoProtocol-EncounterArt-20261002`. Legibility permanece recuperable.

| Encuentro | Cambio perceptible elegido | Intención |
| --- | --- | --- |
| SE2 | Ventilación visible junto al umbral; se detiene con la presentación oculta. Figura frontal, rostro parcialmente iluminado y ropa mate detrás de la puerta entreabierta. Una tensión de madera en el marco al entrar realmente. | Establecer una fuente antes de quitarla; pasar de observar desde refugio a acercarse a un cuerpo parcialmente oculto. El cuarto conserva la figura y el motor detenido después. |
| 6B | Ventilación fija en la recepción, luminaria sobre la figura y nueva superficie de piel, cabello y ropa. El RECALL final apaga el motor y reduce el ambiente local. | La cooperación inicial y el enfrentamiento final usan la misma identidad. Al llegar dentro, el Player recibe su propia pausa registrada mientras el espacio presente pierde movimiento. |
| Echo completo | Modelo propio refinado: mangas más estrechas, volumen de tela irregular, relieve de ojos/cejas y proporción de cabeza revisada. Ocho materiales separados, relieve mate, profundidad compartida y apenas un borde frío. | Reducir el aspecto de silueta cian uniforme y hacerlo pertenecer a la luz de cada lugar. Se mantiene un cuerpo estático sin mirada ni articulación inventadas. |

Se rechazó una primera puesta de SE2 de espaldas y un motor demasiado distante;
la selección final se revisó de nuevo en P1, SE2 y6B dentro del recorrido completo.
Los sonidos nuevos en estos lugares son del **mundo presente**: ventilación y
marco. Se reutilizaron dos masters CC0 trazados, sin gasto ni servicios nuevos.
El Echo sigue reproduciendo exclusivamente posición y orientación corporal.
Puzzles, orientación, señales y687 colliders originales permanecen.

## Comprobado

- Campaña continua, título a final: **PASS563**, cuatro tomas inmutables,
  5608 frames de playback comparados, cero fixtures. SE2 se cruzó y observó;
  final recibido, conservado y llave devuelta.
- Observador pasivo: fuentes fijas,855 frames de pausa, una tensión de marco,
  ambos motores detenidos y cuerpo sin AudioSource/Animator. Cinco guardados
  por API y diez cargas frescas: **PASS341**, preferencias restauradas.
- Escena guardada, referencias inspeccionadas, cero scripts faltantes; shader
  soportado sin mensajes y Console sin errores antes de build.
- Build universal Intel/Apple Silicon y firma local estricta: PASS. Cero errores;
  346 advertencias de paquetes Sentis/Pipeline, preservadas en el ledger.
- App nativa: inicio, movimiento, toma real de5,3s/RECALL, H, pausa, créditos y
  Salir; log sin marcadores de excepciones. No es una campaña nativa completa.
- ZIP: CRC,203 archivos cotejados por hash y permiso ejecutable comprobados.
  Legibility escena/ZIP sin cambios. Respaldo incremental recuperable en
  `../EchoProtocol-backups/encounter-art-checkpoint-20261002/`, sobre Candidate
  y el overlay Legibility; manifest e instrucciones de recuperación incluidos.

[Antes/después, resultados y fallos intermedios](VisualEvidence/2026-10-02-EncounterArt/README.md).
No se hicieron commits ni se descartó trabajo previo.

## Juicio y prueba humana pendiente

La selección da lectura al rostro donde antes quedaba negro y distingue ropa,
piel y cabello donde había cian uniforme. Éste es juicio de composición; el
cuerpo sigue siendo una representación estilizada y estática. Su credibilidad,
anticipación, vulnerabilidad y miedo necesitan una partida humana sin guiar.

Falta escuchar la ventilación antes y después del encuentro, el contacto del
marco y el contraste con los sonidos existentes: comprobar localización,
balance, atribución, repetición y comodidad con auriculares a volumen cómodo y
parlantes. Mono, dinámica reducida, volumen predeterminado y limitador se
conservan. No se afirma audición, miedo, comodidad ni duración humana demostrados
por las pruebas automáticas. La etapa cierra con esos límites explícitos.

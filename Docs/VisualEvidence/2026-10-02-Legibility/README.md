# Legibility — evidencia y alcance, 2026-10-02

Selección: `Assets/_Game/Scenes/Production/EchoProtocol_Legibility.unity`.
[Informe de entrega](../../LEGIBILITY_CANDIDATE.md). Escena derivada mediante
Editor APIs; scripts de autoría conservados como texto en
`ArtSource/Design/Legibility/`. No hubo edición manual de YAML.

## Antes / después desde controles normales

| Vista | Candidate | Legibility seleccionada |
| --- | --- | --- |
| P1 / lectura A | [antes](before/campaign-orientation-P1-A-preview-Game.png) | [después](final/campaign-orientation-P1-A-preview-Game.png) |
| Retorno al Hub | [antes](../2026-10-01-Candidate/continuous/campaign-06-Hub-return-Game.png) | [después](final/campaign-06-Hub-return-Game.png) |
| Origen final | [antes](../2026-10-01-Candidate/continuous/campaign-20-final-origin-Game.png) | [después](final/campaign-20-final-origin-Game.png) |

Más vistas naturales del recorrido final:
[entrada P1](final/campaign-orientation-P1-entry-Game.png),
[terminal Hub](final/campaign-orientation-Hub-terminal-Game.png),
[corredor lateral 6B](final/campaign-orientation-6B-side-route-Game.png),
[pausa exterior aceptada](final/campaign-orientation-6B-pause-ready-Game.png),
[mando después de R](final/campaign-orientation-6B-recall-door-Game.png).
La revisión específica inspeccionó
[B y recordatorio R](p1-readers/initial-protocol-reader-B-aligned-Game.png) y
[C con cooperación aceptada](p1-readers/initial-protocol-reader-C-aligned-Game.png).
Son capturas GameView de controles reales, no colocaciones para la imagen.

Los «antes» de P1 provienen de una nueva prueba de Candidate que terminó con
FAIL por pérdida de foco en P3: sus vistas anteriores siguen siendo válidas,
pero no cuenta como campaña completa. Hub/6B usan su campaña continua anterior;
`preserved-baseline.json` comprueba que la escena Candidate coincide con el
respaldo y que su ZIP anterior permanece intacto.

## Resultados seleccionados

- `final/campaign-full.result.json`: **PASS559**, 321,87 s de ruta automatizada,
  cuatro snapshots inmutables, 6170 frames de reproducción comparados y cuatro
  RECALL; cero fixtures o reanudaciones inesperadas. Título, carta real, P1,
  SE1/P2/SE2/SE3/R1, P3/R3/archivo/Hub, instalación, casa/patio, 6B visto desde
  adentro, conservación, llave y salida. Los segundos NO estiman duración humana.
- `p1-readers/initial-protocol.result.json`: **PASS79**, una toma real y
  1394 frames comparados; cero fixtures. Termina voluntariamente al completar
  P1, por eso `complete=false` no es un fallo de la campaña.
- `checkpoints/checkpoint-all.result.json`: **PASS341**, diez cargas frescas,
  cinco fixtures explícitas (general/hall/kitchen/bedroom/carried). Preferencias
  y supresión de guardado restauradas. Copia exacta del output original bajo
  `2026-10-01-Legibility/`; no se sobrescribió evidencia previa.
- H inmoviliza toma, Echo y movimiento, incluidos Hub/origen final. La lectura
  local A no fabrica autorización conjunta; C muestra cooperación real. Las
  tres luces nuevas se apagan con el SE1 original. Exterior/interior 6B leen
  preparación y testimonio reales.
- `authoring.txt`: 758 contratos serializados originales y 687 poses de
  colliders preservados. `configuration.txt`: referencias explícitas,
  147 luces/57 fuentes, cero scripts faltantes, escena guardada y única escena
  habilitada para build. Sin nuevas reglas ni canales del Echo.
- `build-summary.json`: universal macOS, **Succeeded**, cero errores.
  `build-warnings.txt`: 346 advertencias agrupadas de Sentis/Pipeline.
  Verificación local `codesign --verify --deep --strict` exit0.
- `native-check.json` y `Player.log`: prueba CUA breve de la app compilada,
  toma real 7,5 s/RECALL, notas, pausa, créditos y salida; sin excepciones
  encontradas. Notas Mono de finalización de threads conservadas. Las imágenes
  nativas se inspeccionaron en sesión; no se afirma tener PNGs nativos guardados.
- `package-verification.json`: CRC, hashes de todos los archivos del paquete,
  ejecutable y preservación del ZIP Candidate.

## Iteraciones rechazadas preservadas

`after-first/`: el juego aceptó la pausa exterior; la prueba falló porque su
límite superior no descontaba 0,025 s añadidos por captura. El probe registra
ahora ese overhead medido, sin cambiar el umbral del juego.
`selected/` es un nombre histórico, **no la selección final**: falló una lectura
del display antes de su LateUpdate aun con testimonio real confirmado. La
aserción se lee después del frame renderizado. Ninguna se cuenta como PASS.
La placa exterior que quedaba recortada y el soporte que ocultaba A se corrigieron
antes de `final/`; la revisión de lectores posterior no cambió la escena.

## Límite de la evidencia

Composición y lectura física fueron revisadas desde vistas naturales. No hubo
una partida humana nueva ni audición: orientación espontánea sin H, comprensión
de RECALL/6B, mezcla/localización, comodidad, ritmo, duración y miedo quedan
EN PRUEBA. El recorrido automático tampoco equivale a una campaña nativa
completa, notarización o certificación de hardware.

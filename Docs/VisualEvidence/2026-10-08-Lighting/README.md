# Lighting — evidencia y límites

2026-10-08. Base: Contraprueba preservada. Objetivo: referencias reconocibles de
circulación, controles y pistas con ajustes predeterminados, conservando oscuridad,
contraste y consecuencias eléctricas. No modificar reglas ni Echo.

- `baseline-native/`: campaña macOS continua de la escena original, PASS570,
  cuatro tomas inmutables, cuatro RECALL, cero fixtures/reanudaciones ocultas.
- `pass1-native/`: primera pasada continua PASS570. Conservada; no es la entrega.
  Comparaciones muestran mejoras y dos defectos: hoja de llegada y cocina apagada.
- `baseline-checkpoints/`, `candidate-checkpoints/`, `final-checkpoints/`,
  `verified-checkpoints/`: cargas
  aisladas, dos estados de aislamiento y cinco estados de casa/reserva; renders
  a altura1,63m y coordenadas/objetivos idénticos. Cada pasada declara12 cargas,
  112 cámaras y9 acciones públicas de estado, cero colocaciones de gameplay.
  Las acciones de casa se usan sólo para conservar la llave física sin recoger
  en las vistas adicionales. Se restauran las preferencias originales.
- `authored-pass-1.txt`, `refinement-2.json`, `refinement-3.json`, `refinement-4.json`: decisiones y
  revisiones. La primera autoría comprueba729 colliders y1008 contratos de
  componentes existentes sin cambios (presupuesto de sombras declarado aparte).
- Archivos de configuración base: ambient/post/exposure registrados antes de
  trabajar. La escena Volume está vacía; el perfil URP predeterminado es neutro.
  No hubo aumento global de exposición/ambiente ni nuevos cambios automáticos.

`motion/` y `motion.csv` capturan la cámara real de la ruta a1Hz. `paired/`
contiene74 puntos comparables de campaña y tres giros suplementarios por punto.
`final2-native/` añade tres puntos reales nuevos de bandeja/salida, sin pareja
original:77 puntos totales. Pasa576 comprobaciones,12.148 frames observados,
cuatro tomas inmutables, cero fixtures de gameplay y cero reanudaciones ocultas.
Los355 fotogramas de movimiento fueron inspeccionados.663 selecciones de sombras
coinciden con la regla original. La comparación validada ofrece408 parejas:
74×4 direcciones y112 vistas de cargas/estados. En la
candidata, un render toma temporalmente la posición/rotación/FOV exactos de la
base y los restituye en la misma llamada; no avanza simulación ni coloca al
Player. Estos renders son fixtures visuales explícitas; la campaña normal usa
Input System y cero fixtures de gameplay. `*.comparison.json` conserva la cámara
base, mientras el JSON primario describe cámara/estado del recorrido actual.

La cadencia medida excluye pausas y0,25/0,5s alrededor de trabajo de captura.
Apple M3/Metal,1280×720, misma ruta. Es costo de frame observado en esta máquina,
con ejecución del Editor y herramientas; no un benchmark aislado de GPU ni una
promesa para otro hardware. Los renders no miden miedo, escucha o comodidad.

Advertencias de compilación del paquete de inferencia instalado pueden figurar
en el informe; no son excepciones del juego. El collider heredado del auricular
negativamente escalado figura en el log nativo. Se conserva el diagnóstico y la
ruta atraviesa la interacción; esta pasada sólo corrige iluminación.

Pantalla real, contraste de negros, primera orientación sin guía y eficacia del
terror requieren una partida humana. No se atribuye aprobación humana a estos
resultados. Ver [informe](../../LIGHTING_CANDIDATE.md).

`final-native/` conserva la pasada anterior a la última corrección de portería.
Su ruta pasa, pero el movimiento reveló la bandeja y salida oscuras: no se
selecciona para entrega. `final-controls-iteration/` muestra antes/después de esa
corrección solamente, con cámara prestada de esos fotogramas y ambiente bajo.
Son fixtures visuales sin estado final activado; la verificación continua debe
confirmarlas. `persistence/` conserva los resultados de10 cargas de casa y4 de
presión, obtenidos en la escena Lighting; sus fixtures se declaran en cada JSON.

`p3-retry/`: PASS2743, cinco tomas inmutables, una fixture inicial previa en R1;
movimientos, transferencia, retorno, E sostenida y RECALL usan entradas reales.
No es una ruta desde el título ni una medición humana de la dificultad.
Las cinco referencias locales de P3 seguían activas al terminar.

`final3-native/`: misma app QA final repetida sin cambios de escena; PASS576,
15.548 frames, cuatro tomas inmutables, cero fixtures/reanudaciones ocultas,
662 selecciones de sombras y74 parejas exactas adicionales. `baseline-repeat-native/`
repite la app base. Ambas terminan con código0. La selección visual y el HTML
usan `final2-native/`, ya inspeccionada por completo; la repetición confirma la
misma autoría y se conserva para medir variación.

La app de entrega es universal, firma local estricta comprobada y sin símbolos
QA; `delivery-assembly.json` lee las tablas reales de tipos. El control nativo
breve comprueba18,7s RECALL, movimiento, H/J, pausa, audio UI restaurada, créditos
y salida0. CUA no estableció el movimiento relativo del mouse en esa prueba;
la ruta nativa QA usa entradas sintéticas de mouse en Input System. No se confunde con juego humano.
El intento inicial de auditoría cargó una DLL de herramientas incompatible y
produjo un error de Editor; se conserva en `delivery-console-errors.json`.
Usar la Mono.Cecil ya cargada resolvió la auditoría. No fue una excepción del juego.

Rendimiento completo: `performance.json`. Base repetida media4,76ms/p958,90ms;
final repetida5,53ms/10,12ms, costo medio+16,3%. La otra final midió7,32ms/13,73ms
(p9916,98ms) y se conserva. No se atribuye causalmente su variación. Hay margen
frente a16,7ms en el p95 observado, sin promesa para otra máquina/resolución.
`package.json` verifica203 archivos del ZIP, permisos, CRC, checksum, ambas
arquitecturas, firma estricta original/extraída y el ZIP preservado de Contraprueba.

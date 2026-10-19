# Candidata de entrega — seleccionada

Escena `EchoProtocol_Candidate.unity`, derivada de Walkthrough. Conserva su
campaña física y añade notas contextuales opcionales con H. Consultarlas pausa
mundo, toma, Echo y audio. H/Esc vuelve al juego, o al menú si se abrieron desde
pausa. Las referencias son explícitas; las notas sólo consultan los estados.
No cambian reglas, eventos, puertas ni contenido de las grabaciones.

Walkthrough completa PASS494 con cuatro tomas inmutables/6855frames, cero
fixtures; ContinuePASS341 con diez recargas/cinco fixtures. Su evidencia
es anterior al cambio de ayuda y está conservada en su propia carpeta.

La candidata tiene su propia clave de guardado. Campaña, notas, build y prueba
nativa breve están comprobadas con el alcance indicado abajo. Courtyard conserva
su app y respaldo anteriores. El plazo de esta etapa es el registrado en
Docs/OVERNIGHT_GOAL.md; no se reinicia por crear esta escena.

Escucha, comodidad, comprensión espontánea, duración humana y miedo EN PRUEBA.

## Recorrido seleccionado

**PASS522**, 298.230s automáticos, cuatro tomas inmutables,
10574frames contrastados, cero fixtures/reanudaciones ocultas. Incluye la
pausa real con H en inicio/P2/SE2/P3, movimiento y R durante la lectura, retorno
con H y Escape y vuelta al menú si las notas se abrieron desde pausa. Las ocho
referencias se comparan con los sistemas reales de la partida.
Las cuatro capturas Game de notas se inspeccionaron: texto completo, contexto
correcto y margen de lectura. No es evidencia de comprensión espontánea.

`pre-binding-correction/` conserva una pasada interrumpida: el texto de SE2
consultaba el estudio histórico desactivado y seguía hablando de la puerta P2.
Se reparó por Editor API; la ejecución seleccionada comprueba la identidad real.
`configuration.txt`: cero scripts faltantes, escena guardada,142 luces/57 fuentes.
No se alteraron sus gains ni se añadió audio en esta candidata.

Continue seleccionado: PASS341,14.433s, diez escenas frescas y cinco fixtures
explícitas; preferencias de usuario y supresión de guardado restauradas.
La compilación final no tiene errores; el Console consultado después de la
recompilación quedó vacío. Los probes escucharon errores de runtime y pasaron.

Después de la campaña sólo se cerró una entrada del menú: las teclas C/Enter
no pueden actuar detrás de las notas abiertas. H/Esc y el botón conservan su
comportamiento comprobado. Esa corrección de interfaz está comprobada en la app
exacta; no modifica la ruta ni los estados de mundo.

## Build seleccionada y límite nativo

`build_f94da6cff01a`: Succeeded, 206854641B, 20453ms,
0 errores/346 advertencias:345 de shaders Sentis y1 de Pipeline deshabilitado
en Player sin configuración runtime. El detalle está en `build-warnings.txt`.
Universal Intel/Apple Silicon; firma local `codesign --verify --deep --strict`
correcta. No se afirma notarización ni prueba sobre ambos tipos de hardware.

Prueba breve de la app exacta: título/créditos, movimiento y toma12.4s, RECALL, H, C/Enter bloqueados detrás de notas, retorno a pausa, reanudación y salida comprobados. No campaña nativa completa ni audición. Player.log tiene cero coincidencias de errores de juego; conserva
notas de finalización de threads Mono al salir. Las capturas nativas se vieron
inline por CUA, sin afirmar PNG guardados. `native-check.json` declara el alcance.

App y paquete: `Builds/EchoProtocol-Candidate-20261001/` y su ZIP hermano.
SHA/CRC/modo ejecutable en `package.json`; respaldo fuente externo en
`../EchoProtocol-backups/candidate-checkpoint-20261001/`, con manifest y SHA.
La revisión artística y el procedimiento humano prioritario están en
[DELIVERY_CANDIDATE.md](../../DELIVERY_CANDIDATE.md).

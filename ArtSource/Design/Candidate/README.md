# Cierre de candidata — 2026-10-01

Autor de escena: `Assets/Editor/CandidateProduction.cs`. Deriva de Walkthrough;
la reparación explícita selecciona el evento físico activo de SE2 en vez del
estudio histórico desactivado. No cambia geometría ni reglas de campaña.

`ProtocolGuidance` sólo lee referencias a P1, SE1, checkpoint doméstico, P2,
SE2/SE3 y R1/P3. GameSession muestra estas notas al pedir H y usa la misma pausa
que el menú para conservar toma, Echo, Player y audio. Al abrir desde pausa,
volver devuelve a pausa. No adelanta estados ni revela futuros encuentros.

La revisión de captura detectó la referencia equivocada inicial antes de aceptar
la candidata. `pre-binding-correction/` conserva esa ejecución interrumpida;
el probe final compara las ocho identidades con los sistemas reales del juego.
El script adjunto inspecciona la escena guardada por Editor API sin modificarla.

La nota general sobre R evita afirmar que sólo las estaciones fijan orígenes:
el umbral doméstico también tiene un checkpoint automático existente.

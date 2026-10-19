# Pane — respuesta óptica local del vidrio

2026-10-01. Variante de Study seleccionada en cámara, todavía sin build propia.
`PaneProduction` copia el material sólo para `Study fixed glazing`: habilita
highlights de las luces existentes y cambia smoothness0,92→0,78. Alpha0,16, tinte,
cola, profundidad y geometría conservados. No hay textura, nueva luz, reflexión de
escena, shader nuevo ni modificación del material compartido anterior.

`natural-return/answered`: PASS25; `unanswered`: PASS17. Una fixture inicial por
rama y controles posteriores reales, sin Aim al cuerpo durante el retorno. Las
cuatro capturas por rama muestran el reflejo por encima de la cabeza y el perfil
continuo de cuerpo/cabeza. La puerta y la elección doméstica permanecen libres.
El reflejo se desplaza con el punto de observación; su lectura como vidrio sigue
siendo valoración de cámara, no una prueba humana de descubrimiento o miedo.

Oportunidad geométrica0,6394/0,6377s, no una medición de píxeles visibles. El máximo
delta del caso answered es0,1678s en z−61,248, después de perder de vista el cuerpo;
no se presenta como prueba de rendimiento estable. Unanswered: máximo0,0132s.
Console posterior: cero errores/una advertencia. La campaña continua más reciente
sigue siendo Study PASS446; no se repite para estos dos parámetros de un material.

La escena Pane queda como base de la próxima integración de presentación; Study
compilada/firmada/respaldada sigue siendo el checkpoint recuperable. Entrada
nativa y experiencia humana permanecen pendientes. La prueba de acercamiento
voluntario al marco se mantiene como revisión posterior si cambia esta superficie.

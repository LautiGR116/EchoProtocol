# La máquina que te cubre — candidata, 2026-10-09

Abrí `Builds/EchoProtocol-Coverage-20261009/EchoProtocol.app` y elegí
**Jugar · La máquina que te cubre**. El mismo encuentro está conectado después
del regreso al Hub y antes de la casa. **Nueva sesión** permite jugar la campaña
desde el comienzo. El ZIP incluye instrucciones, créditos y licencias.

## Qué cambió y qué se descubre

El subsuelo ahora pide preparar una operación, salir del lado seguro y aprovechar
esa preparación bajo una amenaza presente. La bomba tapa los golpes de una
manivela, pero su presión mantiene cerrada la reja. Soltar el mando baja esa
presión. El jugador puede probar esa relación sin peligro antes de comprometerse.

El plan combina marcha, pausa y otra marcha. El Echo reproduce exactamente cuándo
se sostuvo y soltó el mando; la bomba real genera su sonido y su presión.
Mientras tanto, el jugador afloja el cierre, cruza durante la descarga y libera
la salida. Esa última acción abre también un paso lateral y provoca una embestida.
Una pared real corta la vista en el regreso: después, el ruido de la bomba puede
atraer al perseguidor. El ruido no vuelve invisible al jugador que sigue a la vista.

Hay manivela, palancas, aguja de presión, pilotos, iluminación local, contactos
materiales, grito, aproximación y recuperación. Captura, aceptación y siguiente
paso tienen texto concreto. Al fallar se puede reintentar la misma toma o preparar
otra; las pistas de H/J responden al progreso. No se renovó el personaje completo:
se reutiliza el adulto existente, como alcance explícito para este goal.

## Comprobado objetivamente

| Comprobación final | Resultado y alcance |
| --- | --- |
| Tramo, captura, reintento, pausa y final íntegro de la toma | PASS43, 5.788 muestras de reproducción. Una toma inmutable y un R; el reintento usa esa misma toma. Entradas normales del Input System, ninguna colocación de prueba ni reanudación oculta. |
| Campaña completa, título → final | PASS541 en 428,5 s de recorrido automatizado; cinco tomas inmutables, 14.830 muestras de reproducción, ninguna colocación de prueba ni reanudación oculta. Incluye la casa y 6B. Ese tiempo no estima una primera partida humana. |
| Guardado/carga parcial y completada | PASS52, ocho escenas recién cargadas y diez estados de prueba explícitos mediante API pública, separados del recorrido. Se restauraron las preferencias previas. Una carga parcial vuelve a un comienzo seguro; una salida completada permanece abierta. |
| Contrato del nuevo canal | PASS7: copia defensiva, límites temporales, destino aislado y rechazo de tiempos inválidos. La fidelidad durante reproducción también se comprueba en los recorridos. |
| Escena configurada | PASS, 72 referencias explícitas y un mando grabable. Audio situado en la máquina, mandos, marcos o amenaza correspondiente. |
| Build final y código entregado | Unity 6000.3.21f1: Succeeded, cero errores y una advertencia. App universal Intel/Apple Silicon; ningún código de las pruebas se incluye en la entrega. |
| Controles nativos de la app entregada | Botón del tramo, movimiento, R, H, cierre de ayuda, pausa y Salir comprobados mediante teclado/ratón reales. E sostenida y la resolución completa se comprueban en el Player de verificación mediante entradas normales del Input System. |
| Audio real capturado | Estéreo en campaña y mono/dinámica reducida en captura/reintento; cero bloques perdidos, buffer agotado o muestras recortadas digitalmente. No hubo normalización. Esto no mide volumen del dispositivo ni comodidad humana. |
| ZIP | 205 archivos comparados byte por byte, CRC, permisos ejecutables y firma local estricta correctos, también al extraerlo. La app no está notarizada. |

La consola final contiene 72 errores del transporte de **Unity Pipeline**,
clasificados y conservados; ninguno procede del juego. Los recorridos finales
no registran errores del juego. La campaña conserva dos advertencias heredadas:
un collider del auricular con escala negativa y reducción del atlas de sombras.
El Player de verificación tuvo además advertencias del paquete Inference durante
su build. No se presentan esos avisos como una consola completamente limpia.

Una iteración nativa previa falló porque el perseguidor seguía viendo al jugador
a través del paso lateral. Se conserva como FAIL: la versión final agrega el
quiebre físico y pasó tanto captura/reintento como campaña. También permanecen
las pruebas intermedias, con su alcance y errores, sin convertirlas en aprobaciones.

## Qué falta juzgar jugando — EN PRUEBA

Probá primero sin abrir todas las pistas. Evaluá si descubrís la diferencia entre
ruido y presión, si tu grabación se siente como un plan propio y si sabés por dónde
volver cuando aparece la amenaza. Después juzgá si la embestida, el grito y la
cercanía producen tensión creíble, y si el reintento conserva esa tensión sin
frustración. Escuchá a volumen cómodo; auriculares son opcionales y hay salida
mono y dinámica reducida. Claridad, actuación, miedo, comodidad y ritmo no tienen
aprobación humana nueva. La calidad del personaje sigue limitada al modelo previo.

## Entrega y recuperación

- App: `Builds/EchoProtocol-Coverage-20261009/EchoProtocol.app`.
- ZIP: `Builds/EchoProtocol-Coverage-20261009.zip`, SHA256
  `5e36228a8457253ef66abc22ec5dd292dc9615b2dd7b968f008b52ccb2025f88`.
- Escena: `Assets/_Game/Scenes/Production/EchoProtocol_Coverage.unity`.
- Guardado propio: `EP.CampaignCheckpoint.Coverage.v1`; conserva las ediciones anteriores.
- Fuentes recuperables: `/Users/lautigr116/EchoProtocol-backups/coverage-checkpoint-20261009/`.
  Seguir `RECOVER.txt` y verificar todas las capas contra `manifest.json`, en un
  directorio vacío. Folio, Resonance y la base previa a esta intervención permanecen
  recuperables. No hubo descarte de cambios, staging ni commits.
- [Capturas reales y evidencias](VisualEvidence/2026-10-09-Coverage/index.html).
  Los controles de construcción y comprobación quedan en
  `ArtSource/Design/Coverage/Tools/`; la escena guardada es la referencia final.

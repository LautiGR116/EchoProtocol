# Rama opt-in acotada de VS1ParallelFigureEvent

Propuesta de contrato para implementar después de seleccionar el cascarón.
No es código compilado ni una escena configurada. Evitar otro MonoBehaviour que
escriba los mismos roots; el evento actual sigue siendo el único propietario.

## Referencias nuevas serializadas

```
bool usePhysicalTraversal = false;
FirstPersonController traversalPlayer; // actor real y PoseReset; referencia explícita
BoxCollider[] concealedEntryPockets;   // 2 cajas trigger, geometría fija
BoxCollider interiorVisit;            // centro(6.10,1.1,28.80), size(1.70,2.2,1.80)
BoxCollider[] outwardExits;           // los 2 vanos, local+Z hacia fuera
Renderer[] changingSurfaces;          // referencias de color explícitas, no proxy
Collider[] permanentOpaqueScreens;    // sólo arquitectura opaca; nunca el vidrio
```

Propiedades de lectura `UsesPhysicalTraversal`, `HasPresentedDomestic`,
`HasPhysicallyVisited`, `TraversalCompletionCount` y `PresentationCount` separan
evidencia. Conservar `CurrentPhase`, counters legacy y `QualifiedReadSeconds`;
éste queda0 en la nueva rama, no se simula una lectura.

Salida sur: box center(4.90,1.1,27), size(1.40,2.2,.30), yaw180.
Salida norte: center(6.50,1.1,31.20), size(1.40,2.2,.30), yaw0.
Ambas usan local+Z hacia fuera. La salida se cuenta sólo después de visitar
interiorVisit. Se acepta entrar/salir por cualquiera de los sentidos, incluido
retroceder después de pisar el salón: no fabricar una nueva tarea de circulación.

## Semántica

1. Awake valida rama elegida y raíces distintas. Sólo inicialización de esta escena
   establece el frío. La rama false conserva validación/Update/OnDisable originales.
2. Aceptación: el Player real cruza trigger mientras `IsEchoHoldingGate`, igual que
   ahora; pasa a AwaitingLook (nombre histórico, ya no significa esperar mirada).
   Sin registrar al Echo como visitante ni repetir el armado por OnTriggerStay.
3. Presentación: al atravesar un pocket oculto y con el cambio efectivamente fuera
   de vista, intercambiar sólo renderers/materiales/roots de presentación seguros,
   una vez. Las luces/colliders constantes viven fuera. Marca
   HasPresentedDomestic=true; no modifica datos ni estado Echo.
4. Progreso: pisar interiorVisit arma HasPhysicallyVisited y phase Reading; cruzar
   un outwardExit en dirección exterior completa una vez. No hay tiempo de espera,
   orientación, E, `IsPlayerLookingThroughGlass` ni crédito automático por reveal.
5. Complete no implica aparecer. Si un caso excepcional mantuvo una superficie
   visible e impidió el intercambio, la marcha puede completar en frío. Registrar
   ese hecho para evaluar la arquitectura; no impedir salida ni forzar revelado.
6. La presentación revelada permanece durante la escena, incluso tras RECALL
   incompleto. RECALL sólo vacía historial de marcha/visita incompleta y rearma
   aceptación; jamás vuelve a frío a la vista. Si Complete ya ocurrió, permanece.
7. OnDisable/OnEnable no cambian presentación en opt-in. Desuscriben/suscriben y
   vacían la posición previa. OnDestroy no revela ni restaura roots. No ejecutar
   SetDomesticPresentation(CurrentPhase==Complete) en esta rama.
8. Pausa/título/documento: no aceptar progreso ni cambiar presentación cuando
   Time.timeScale<=0 o el input de locomoción esté bloqueado por GameSession.
   Usar su referencia/estado existente; no confundir AudioListener.pause con todo
   bloqueo de juego. Sin muestras de tiempo real para completar pasos.

## Marcha, RECALL y seguridad

- Leer sólo el Transform del Player explícito; no escoger el actor por el collider
  que llegue primero. Volume.Contains usa BoxCollider local, enabled/active, punto
  Player+up. No usar AABB para nuevas zonas.
- Para cada salida, guardar posición previa en su espacio local. Acreditar
  cruce −Z→+Z por interpolación del segmento real y comprobar x/y dentro del box.
  Exigir HasPhysicallyVisited antes del cruce. No completar por estar teletransportado
  al lado exterior o por una `bounds.Contains` desde el inicio.
- Suscribir FirstPersonController.PoseReset y LoopCompleted para invalidar historial.
  Un tramo que incluye teleport no es marcha. Al reactivar/retomar después de pausa,
  la primera muestra sólo inicializa; no conservar un segmento cruzando estados.
- No inyectar pose, R, E ni `CompleteLoop`; los probes llegarán por input real.
- No cambiar un collider ni SetActive del cascarón durante el cambio. Los muebles
  tienen collider común coincidente con una forma visible en ambos estados.
- Ocultación: una prueba de punto objetivo es insuficiente. La autoría fija codos
  opacos y la protección runtime debe cubrir todas las superficies cambiantes,
  incluidos suelo/techo y parte posterior. El vidrio no es un oclusor opaco.
  Un proxy invisible no acredita cuerpo. No tratar un buffer de rayos saturado como
  invisibilidad. Hasta seleccionar geometría, diferir antes que revelar a la vista.
- Guardar estado de presentación separado permite ese diferimiento sin convertirlo
  en una nueva obligación de mirar hacia otro lado. No hace falta framework de visión.

## Cambios de rutas existentes, sin nueva batería grande

`ThresholdCampaignProbe` detecta UsesPhysicalTraversal, camina los waypoints del
plano, captura el frío con encuadre natural (sin requisito de Aim sostenido), el
salón y salida. La rama legacy queda exacta. Comprueba integridad P2/Echo como ya
lo hace, visita/cruce reales y SE3 original. No afirma lectura visual por Complete.

Un focal moderado basta antes del full: un acceso normal, una retirada/entrada
por la salida y un R o pausa en el codo. Registrar por separado renderers/luces/
colliders y PresentationCount. Si la cámara no permite cambiar, la salida debe
seguir disponible; conservar la captura del caso para decidir la geometría.
No afirmar espontaneidad, miedo, localización sonora ni duración humana.

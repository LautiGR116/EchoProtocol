# Ronda 6 — el intruso se aferra y se abalanza

Dirección aprobada el 2026-10-23. Tres chats GPT-6 Astra / Ultra, worktrees separados,
sólo Editor, sin builds ni push/publicación. Base `e393c391`, rama central
`codex/integracion-20261023`. Integrado y guardado; terror no aprobado.
Coordinador: `01a12cb5-6030-77a1-b02b-93734f7c279b`, `/Users/lautigr116/EchoProtocol`.

## Dirección común

El usuario todavía ve al intruso demasiado cotidiano. La captura R5 muestra un
hombre con comisuras amables y palma ofrecida detrás de un mostrador. Esta ronda
cambia tres lecturas grandes: **una boca que parece abierta a la fuerza, una mano
que se aferra a un borde y una acometida que termina demasiado cerca**. No añade
una explicación de posesión ni altera al Echo. Más palidez/milímetros no alcanza.

Ver las imágenes del original: `Builds/round5-integration-20261023/p3-settled.png`,
`pump-contact.png`, `pump-recovery.png`. Leer AGENTS, README, ROADMAP, cierre R5
y diseño relevante. No se identificó nueva partida humana; esto es dirección y
evidencia visual. Objetivo: implementar pronto una alternativa fuerte y jugable.

## A — una cara inequívocamente anormal

Posee nuevos `Assets/Editor/IntruderRupture*.cs`, los shaders `EmbestidaSurface`,
`IntruderDreadSurface`, `IntruderDreadEyes`, nuevos auxiliares `IntruderRupture*`,
y `Docs/INTRUDER_RUPTURE_ROUND6.md`. Puede crear derivados privados pequeños.

Rehacer la silueta: apertura vertical estrecha de boca, mandíbula descendida y
descentrada, comisuras sin sonrisa y órbitas tensas. Una anomalía grande legible
a distancia de P3. Resolver cavidad/labios/interior con volumen real, no óvalo
negro ni máscara flotante. Puede añadir geometría oral si el scan cerrado no
alcanza. Preservar textura, rig/pesos/UV necesarios, ojos asentados y cuello.
Evitar apariencia de modelo roto/derretido; no hace falta sangre aleatoria.

No editar `IntruderFaceGeometry.cs` ni invalidar su hash/recibo R5. Crear upgrade
R6 desde el Face.asset instalado, backup R5 y rollback propios; mantener los
objetos/GUID existentes cuando sea viable. No tocar atlas/scan compartido, Echo,
huesos/poses, luces o cámara. Mantener `_Apron` fuera de UnityPerMaterial: corrige
el binding Metal/SRP. No reejecutar instaladores históricos.
Contrato B: boca abierta en geometría de reposo; P3 no la cierra ni sonríe con
jaw/mouthCorners. Informar rango seguro si es necesario. La cara también cambia
en bomba; conservar su compatibilidad. Un único diseño contundente, no variantes.

## B — quitar el saludo; aferrarse y atacar la distancia

Posee `Campaign/P3Intrusion/P3BodyIntrusion.cs`, `Campaign/IntruderActing/`, nuevos
`Assets/Editor/P3RupturePerformance*.cs` y `Docs/P3_RUPTURE_ACTING_ROUND6.md`.
Único escritor de huesos/raíz/targets P3. Perfil opt-in y acotado a P3; preservar
actuación de bomba salvo compatibilidad necesaria con la nueva cara.

Eliminar la palma ofrecida: mano que agarre un borde real, dedos y peso visibles.
Tres poses: retención, liberación brusca, quietud demasiado cercana. Cabeza/mirada
con intención; quitar balanceo social y microgestos sustitutivos. Puede reubicar
stop/targets dentro de colliders/espacio existente para proximidad real. No mover
Player/Echo ni atravesar vidrio. Conservar CharacterController, contacto, apoyos,
pausa/RECALL y transferencia del mismo actor a bomba; sin nueva muerte/captura P3.

Conservar API `Present`, `OwnsPose`, `HasContact`, `Contacts`, `ReleasePose`. El
contacto único dispara las fuentes existentes; C elige clips/configuración. C
observa esos hechos y `IntruderStaging.P3Visible`, sin escribir poses. Informar
el borde elegido a C, que no lo moverá. Neutralizar offsets sociales de jaw y
mouthCorners en este perfil P3 para respetar A. Upgrade desde R5 con preflight,
Undo/rollback propios; no reescribir recibos anteriores ni lógica de puzzle.

## C — descubrir una amenaza, no un retrato

Posee sólo presentación P3 en `Campaign/IntruderStaging.cs` y
`Audio/MachineCoverPresence.cs`, nuevos `Campaign/P3RupturePresentation/`,
`Assets/Editor/P3RuptureReveal*.cs`, audio `Audio/P3Rupture/`,
`Docs/P3_RUPTURE_REVEAL_ROUND6.md` y manifiesto de audio si corresponde.

Romper el encuadre de mostrador: mano/borde parcialmente ocultos primero, rostro
que se descubre con la acometida y queda visible de cerca. Reusar luces y oclusión
física; retirar fondo banal sin esconder cara/mano ni el Echo verdadero. No mover
lectores, puertas, vidrio funcional, targets de B ni recorrido grabado. La
colocación inicial continúa fuera de vista. C no escribe huesos ni raíz.

Un ataque seco material del vidrio/marco, esfuerzo adulto breve si ayuda, después
quietud/aliento cercano. Disparar por los hechos reales B, sin doble golpe ni
otro reloj de ataque. Contraste, espectro y proximidad; no volumen maestro. CC0
existente primero; nuevos clips pequeños con licencia/procedencia trazables.
Sin ElevenLabs/casting pendiente, estrobo, overlay de susto o cámara forzada.
Mantener WorldSFX, limitador, mono/dinámica reducida y audio de bomba. No nuevos
eventos en la grabación ni cambios de progreso. Upgrade reversible desde R5.

## Coordinación, velocidad y entrega

- Sólo escribir en el worktree propio. Activar sparse-checkout como R5: `/*` con
  exclusiones ArtSource, Docs/VisualEvidence, Docs/AudioEvidence, Assets/_Game/Art,
  Scenes, Prefabs y Fonts. Leer medios/escena privados en original. No copiarlos,
  abrir otros Editors/Libraries ni instalar paquetes. No push/merge/build.
- Sólo el coordinador opera la escena central. Integradores con preflight,
  upgrade y rollback, sin abrir/guardar escena automáticamente. No YAML ni
  generadores históricos. No tocar archivos de otro frente o sus recibos.
- Como máximo un subagente disjunto por chat, sin cadenas. Informar pronto los
  contratos que bloqueen, avanzar en lo independiente. Una propuesta fuerte.
- Check propio mínimo: diff y configuración/cálculo pertinente. No declarar
  compilación/Play sin Editor. Sin nuevos harnesses, suites, campañas o informes largos.
- Central: compilación conjunta y un punto P3 de imagen/contacto/audio. Cara nueva:
  una vista breve en bomba, sin repetir persecución. Repetir sólo por edición
  relevante/fallo. Reusar checkpoints/evidencia. Entregar rápido a prueba humana.
- Si la cara/mano siguen leyendo un hombre saludando a distancia real, señalarlo
  y corregir el defecto dominante; no aprobar por contadores. Miedo EN PRUEBA.
- Entregar commits, operación exacta de integración, cambio visible y un límite
  humano. Conservar trabajo ajeno, licencias y derivados privados fuera de Git.

## Estado

Tres frentes terminados en GPT-6 Astra / Ultra e integrados en Incursion.
Escena/derivados privados guardados, Editor detenido; sin build ni publicación.

| Frente | Chat | Worktree |
| --- | --- | --- |
| A — cara | `01a16a5b-5747-7511-82a9-583b74801169` | `/Users/lautigr116/.codex/worktrees/b9e0/EchoProtocol` |
| B — actuación | `01a16a5b-7e0f-7292-9341-5e81941629d4` | `/Users/lautigr116/.codex/worktrees/a9a5/EchoProtocol` |
| C — revelación | `01a16a5b-a55c-75f0-aaed-566fa5044a9f` | `/Users/lautigr116/.codex/worktrees/e3ed/EchoProtocol` |

B comunicó a C que reserva el jamb norte existente `A sealed window jamb`
(Transform 483625452) como borde de agarre; C no debe moverlo ni ocultarlo.
Coordenadas y tiempos definitivos están en la entrega B. El contrato facial mantiene
jaw/mouthCorners neutrales en el nuevo perfil P3.

## Cierre central

Commits originales A `7d983990`, B `922dcf94`, C `ba963450`; integrados como
`1a639bc8`, `02a850ab`, `ef126289`. Corrección A `937a3c3a` integrada como
`ebfb3ce1`. Dos ajustes centrales: preflight C acepta el jamb activo dentro de
R1 todavía inactivo por campaña; menor brillo oral tras ver el primer plano.

La primera toma reveló un fallo real: `CopySerialized` conservaba el layout
nativo anterior y el SkinnedMeshRenderer dejaba de dibujar al intruso. Reimportar
no bastó. Reconstruir streams con `Clear(false)`/setters y reenlazar el mismo
Face.asset corrigió el render; fingerprint/GUID/backups conservados. La reparación
persistente se aplicó y se comprobó en Play. P3 se repitió sólo por ese fallo.

Comprobación final acotada: compilación sin errores; P3 avanzó 50→108 frames,
contacto único, sin bloqueo, suelta completa y aliento posterior observado.
Vista breve de bomba: cesión de pose P3, contacto único y cara visible también
articulada de cerca. Console sin errores nuevos desde cursor 11739 hasta 11741.
Se reutilizaron los snippets/checkpoints R5: P3 usa un Echo estacionario sintético
y estado preparado; no verifica descubrimiento espontáneo ni resolución natural
del puzzle. No campaña, persecución completa, suite ni harness nuevos.

Evidencia local en `Builds/round6-integration-20261023/`: `p3-contact.png`,
`p3-settled.png`, `pump-contact.png`, `pump-recovery.png`, estados JSON y captures
del listener. P3 2,88 s / bomba 2,47 s, picos −6,02 dBFS, sin muestras a escala
completa. Son comprobaciones de señal; no escucha humana ni comodidad certificada.
Backups antes/después en `/Users/lautigr116/EchoProtocol-backups/parallel-round6-20261023/`.
Capturadores retirados, Play detenido, escena limpia y guardada.

Juicio visual del coordinador: se perdió la sonrisa/palma ofrecida y la abertura
oral es evidente a distancia de P3. La credibilidad de esa deformación y si la
irrupción provoca retirada siguen **EN PRUEBA humana**, junto con timbre/comfort.

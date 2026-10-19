# Vidrio sellado del estudio — candidato posterior a Study

**Integrado y seleccionado objetivamente en la escena Pane; build propia pendiente.**
La autoría root `Assets/Editor/PaneProduction.cs` deriva Study y aplica la copia local.
Ramas de controles/cámara PASS25/17. El reflejo se mantiene sobre la cabeza, sin tapar
su perfil en las capturas seleccionadas. Study permanece como build recuperable.
La propuesta original siguiente conserva su razonamiento y no implica aprobación humana.

## Diagnóstico concreto

En las cámaras seleccionadas de Study, z−55,5/−56,5, marco, antepecho, habitación y silueta se leen; la lámina casi no deja una señal propia. `FinishProduction.cs:101` creó `M_ObservationGlass` a partir del vidrio anterior y desactivó explícitamente los highlights. El material actual usa URP Lit transparente, alpha0,16 uniforme, tinte azul verdoso, smoothness0,92, metal0 y `_SPECULARHIGHLIGHTS_OFF`. No tiene mapa normal ni de superficie.

La rugosidad **no crea iluminación ni un reflejo ausente**. Habilitar highlights permite respuesta especular directa, pero su existencia depende de posiciones, normales y luz recibida. `Study window residual`, a (4,8;2,3;−58,8), queda detrás de la cara que ve el Player en el hall: no garantiza un highlight frontal. La luz residual del servicio sí está del lado del hall. Los puntos exactos iluminados se deben comprobar en las dos ramas con las luces existentes.

Tampoco hay una captura del cuarto en el reflejo: el renderer actual incorpora SSAO, no SSR, y la escena no contiene ReflectionProbes. El cubemap ambiental no demuestra reflejos del cuerpo o del mobiliario. No se añadirá ninguno.

## Una variante local propuesta

Crear **una copia** del material y asignarla sólo al renderer `Study fixed glazing` en una edición posterior. Nombre sugerido `M_StudySealedGlass`; conservar malla, collider, referencias, cola transparente, culling y profundidad actuales.

| Propiedad | Candidato único |
| --- | --- |
| Shader y workflow | URP Lit, Metallic con metal0; dieléctrico estándar del shader |
| Highlight directo | habilitado |
| Smoothness | 0,78; más ancho y de menor pico que 0,92, todavía vidrio claro |
| Color / alpha | conservar exactamente `(0.035,0.18,0.23,0.16)` para aislar la nueva respuesta especular |
| Reflejo ambiental | conservar el valor1 heredado; no representa una captura actual de esta habitación |
| Bump / normal / refraction / emisión | ninguno |
| Especular preservado | conservar el modo heredado; el highlight no depende de alpha |
| Cast shadows del vidrio | conservar desactivado; recepción de sombras sin cambios |

**No preparar textura en este primer candidato.** La comparación cambia sólo highlight directo y smoothness. Conserva toda la transparencia actual de cabeza/torso y evita usar suciedad, película o líneas de borde para ocultar una limitación de iluminación. Primero debe verse qué aporta la respuesta del material a las fuentes existentes.

`MaterialRecipe.cs.txt` contiene una función propuesta que devuelve **un material transitorio**; no tiene MenuItem, AssetDatabase, búsqueda de escenas, generación de textura ni asignación a renderers. No se ejecutó. El integrador decidirá su guardado y la escena sucesora después del checkpoint Study.

## Qué aceptar o descartar

Comparar el mismo retorno yaw180 y ambas ramas de alimentación, además de un acercamiento voluntario al marco. Buscar una lámina visible de forma localizada al aproximarse, manteniendo la pared posterior y el perfil continuo de cabeza/hombros. No aumentar exposición global ni añadir luz para salvar este candidato.

Descartar si el highlight tapa rostro/torso, si aparece una película gris uniforme o si el vidrio pasa a ser más dominante que la persona. Con especular preservado, bajar alpha **no** atenúa el highlight: no debe usarse como supuesto control de brillo. La corrección pertinente sería revisar la respuesta especular local, no oscurecer el cuerpo ni su habitación.

Una cara sin suficiente luz frontal puede seguir siendo casi invisible aunque el material esté bien configurado. Si ocurre, conservar Study es preferible a fabricar un reflejo, introducir emisión o aumentar una opacidad que vuelva a velar la figura. La lectura de estos archivos sólo fundamenta el candidato; el resultado exige comparación real en Unity y no acredita descubrimiento o miedo humano.

## Fuentes locales consultadas

- `Assets/_Game/Art/Finish/M_ObservationGlass.mat` y `Assets/Editor/FinishProduction.cs`.
- `Assets/_Game/Scenes/Production/EchoProtocol_Study.unity`, `Assets/Settings/PC_Renderer.asset` y cámaras seleccionadas de Study.
- URP instalado `com.unity.render-pipelines.universal@8aa8d736cba6`: `Shaders/LitInput.hlsl`, `ShaderLibrary/BRDF.hlsl`, `Editor/ShaderGUI/BaseShaderGUI.cs`. El alpha del mapa multiplica el alpha del material; el modo `_ALPHAPREMULTIPLY_ON` conserva la contribución especular mientras aplica alpha al difuso. No se presupone un shader distinto ni soporte externo.

Evidencia de integración y límites: `Docs/VisualEvidence/2026-10-01-Pane/README.md`.

# Records — paso físico por el archivo

**Estado: integrado y compilado por el agente raíz; revisión de cámaras en curso.** El borrador inicial preparado fuera de Assets se conserva como referencia. Deriva `EncounterProduction.ScenePath` hacia `Assets/_Game/Scenes/Production/EchoProtocol_Records.unity`, con raíz `Records / physical return passage` y clave `EP.CampaignCheckpoint.Records.v1`. Archivo: [`staged/RecordsProduction.cs`](staged/RecordsProduction.cs).

El tramo central deja de ser una vitrina lateral: el Player entra por el vano norte, camina dentro del archivo y sale por el vano sur. Una partición opaca de revoco y madera, fija desde el inicio, cierra la antigua recta de forma visible. Se reutilizan estantes, cajas, vidrios, lámparas y materiales existentes. No hay mecanismo, puzzle, evento, voz ni canal nuevo del Echo. La esclusa de recepción y el resto del recorrido permanecen intactos.

## Planta y puntos de marcha

Coordenadas mundiales, pies `y≈.03`; los puntos son objetivos de marcha, nunca teleports:

| Punto | x | z | Yaw sugerido |
| --- | ---: | ---: | ---: |
| Aproximación | −18.60 | 29.00 | 180 |
| Frente al vano norte | −18.60 | 27.00 | 180 |
| Dentro del vano norte | −21.15 | 27.00 | 270 |
| Bajo la lámpara norte | −21.15 | 24.50 | 180 |
| Bajo la lámpara sur | −21.15 | 17.50 | 180 |
| Vano sur interior | −21.15 | 15.00 | 180 |
| Retorno a galería | −18.60 | 15.00 | 90 |
| Continuidad anterior | −18.60 | 13.00 | 180 |

Después se mantiene la marcha anterior por `(-18.6,7)`, `(-18.6,-9.5)` y la esclusa. Entre z29 y z13, la ruta ortogonal mide **21.1 m frente a 16 m: +5.1 m, o +1.275 s de traslación a 4 m/s**. Giros, desaceleración y recorrido humano no fueron medidos; el objetivo de incremento total es aproximadamente 2 s.

Los vanos nominales de 1.8 m dejan **1.72 m libres entre jambas**, con altura mínima 2.43 m. El pasillo junto a estantes tiene 2.33 m, localmente 2.28 m junto al saliente del alféizar. Un muestreo matemático del trazado cada ≤1 cm contra rectángulos de los nuevos obstáculos arroja distancia mínima de centro 0.86 m: margen 0.51 m sobre radio .32 + skin .03. Esto **no es un sweep de Unity ni una prueba de CharacterController**.

## Colisión que se reemplaza y agrega

- Se deshabilita únicamente el `BoxCollider` original **R3 spine west wall**, centro`(-20,1.6,18.7)`, tamaño`(.2,3.2,59.2)`; su renderer ya estaba deshabilitado. La nueva colisión conserva su extensión z[−10.9,48.3], salvo vanos z[14.1,15.9] y[26.1,27.9].
- Se conservan inactivos los cuatro renders continuos que cruzarían esos vanos: **Records window solid sill**, **Records window sill cap**, **Records fixed glazing**, **Records glazing transom**. Se reconstruyen tramos visibles/colliders separados. Vidrio central: z[15.9,26.1]. Se conservan los cinco montantes originales y el dintel alto continuo.
- El archivo anterior era escenografía sin colisión. Se agrega piso`(-22,-.1,21)/(4,.2,14)`, techo`(-22,3.1,21)/(4,.2,14)`, pared posterior`(-24,1.5,21)/(.2,3,14)` y testeros`(-22,1.5,14|28)/(4,3,.2)`.
- Cinco volúmenes de estantes, centro`(-22.9,1.2,z)`, tamaño`(.94,2.4,1.76)`, z15.5/18/20.5/23/25.5; no cientos de colliders sobre cada caja.
- Partición visible y sólida en`(-18.6,1.6,25.4)`, tamaño`(2.8,3.2,.2)`, unida a los límites de galería. Reutiliza el rótulo ordinario del archivo, que estaba situado en el nuevo vano. No hay bloqueo invisible ni cartel de puzzle.

La presentación nueva y los colliders están en grupos separados. Ningún collider de esclusa, puzzle, origen o grabación se edita. La operación usa API del Editor, nunca YAML.

## Fuentes físicas y nivel inicial

Se reutiliza **AUD-AMB-002**, `AMB_Fluorescent_LP.wav`, de JoelMcDaniel: [Fluorescent Light Buzzing](https://freesound.org/people/JoelMcDaniel/sounds/830440/), CC0. Procedencia y preparación en [`Docs/AUDIO_ASSET_LEDGER.md`](../../../Docs/AUDIO_ASSET_LEDGER.md). Importer GUID`ab6c64bce0c5b40e4b2cf92ff8852472`; el autor verifica identidad, mono, 12.85 s y precarga. No se modifica ni genera audio.

Medición del WAV importado: mono, 48 kHz, 24 bit, 12.85 s; **RMS −27.45 dBFS, pico −12.04 dBFS**. El snapshot guardado de `MX_Main` tiene Master +10 dB; Ambience no tiene atenuación adicional. `GameSession` usa listener .7 por defecto y respeta la preferencia guardada.

| Ganancia de fuente | RMS estimado bajo lámpara | Pico estimado |
| --- | ---: | ---: |
| .012 inicial, descartada por margen excesivo | −60.6 dBFS | −45.2 dBFS |
| **.06 propuesta** | **−46.6 dBFS** | **−31.2 dBFS** |
| Bed histórico .05 en 2D, misma cadena | −46.57 dBFS | −31.16 dBFS |

La estimación local usa oído`(-21.15,1.63,24.5)` y fuente`(-21.35,2.8,24.5)`: distancia 1.187 m, ganancia lineal .828 entre min .5 / max 4.5 m. No incluye reparto estéreo, suma de fuentes, transitorios simultáneos ni limitación final. **No demuestra audibilidad ni calidad tímbrica por oído.**

Dos emisores ocupan las lámparas reales, y 2.80 / z 17.5 y 24.5. `Ambience`, volumen .06, espacialidad 1, rolloff lineal .5–4.5 m, doppler 0, loop continuo, pausa del listener respetada. No se modifican buses, preferencias, SE1 ni fuentes del Player. Las lámparas de esta edición están siempre encendidas; sus loops no participan en RECALL. Si esa potencia cambia en el futuro, el sonido deberá seguirla. Se reorienta sólo el foco norte existente hacia el piso de aproximación, manteniendo su carcasa y emisor.

No se atribuye a R3 un supuesto motor 2D: **Distant Machinery Bed** es 3D, está en mundo`(6.75,3.25,8.5)` y tiene rolloff lineal hasta 20 m; el eje de retorno x−18.6 queda fuera de ese alcance.

## Cámaras y límites pendientes

Con cámara real a ojos≈1.63 y FOV vertical 60: entrada`(-18.6,29)/yaw180`; interior`(-21.15,24.5)/yaw180`; vista lateral`(-21.15,21)/yaw150`; salida`(-21.15,15)/yaw90`. La vista lateral mira la galería sur todavía no recorrida: **no prueba reconocer la propia trayectoria**. Una mirada hacia el bolsillo norte al entrar puede ser útil; debe decidirse con imágenes reales.

Falta comprobar marcha libre por ambos vanos, retorno antes de la esclusa, suelo/techo, claridad de la partición y lectura de la abertura cálida. Para el sonido: comparar quieto bajo lámpara, giro de 180° y salida de su alcance; los pasos deben seguir exclusivamente el desplazamiento del Player. La proporción sonora, sensación de cercanía, duración espontánea y efecto de aislamiento requieren escucha y partida humanas. El borrador original no escribió Assets ni aporta prueba humana. La integración posterior se distingue abajo.

## Integración y revisión de cámara

`Assets/Editor/RecordsProduction.cs` ya deriva y guarda la escena Records con API
del Editor. Primera ruta aislada PASS14: fixture inicial explícita, controles
reales simulados después, ida/vuelta y suelo sin caída. Imágenes preservadas en
`Docs/VisualEvidence/2026-10-01-Records/route-initial`.

La lectura de esas imágenes motivó orientar el foco sur al suelo junto al vano
sin subir intensidad, y corregir SurfaceMapping del linóleo/envolvente antes
estirados. El piso ahora usa la repetición de3,2m de la galería. Se reforzaron los
checks del diagnóstico para verificar realmente restauración de luz, clip mono,
ganancia/posición física de fuentes y avance del empuje contra la pared.
Segunda pasada PASS18,23,43s; campaña completa PASS446,275,11s, cuatro tomas y
cero fixtures. La evidencia continua está en Docs/VisualEvidence/2026-10-01-Records.
El segmento automático medido z29→13 agregó3,442s, incluyendo paradas/capturas;
no acredita el objetivo preliminar de menos de2s en una marcha humana. La autoría
conserva5,1m adicionales calculados. No hubo audición ni aprobación humana.

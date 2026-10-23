# P2 — testigo lateral del cierre, 2026-10-23

**Integrado y guardado en Incursion; comprobación central puntual realizada.**
Base `5277b28db7d1984178896d4c6487af16996b85e3`, rama
`codex/p2-causal-demonstration`. Comprensión espontánea y comodidad **EN PRUEBA humana**.

La evidencia era no entender por qué apartar la mirada abre la segunda puerta.
Los estados `ABIERTA POR MIRADA`, `UMBRAL OCUPADO`, `CERRANDO`, las miniaturas y
su movimiento real **ya existían**. Esta iteración cambia su composición:
el panel testigo queda al norte del puesto, mirando al jugador, junto a la
primera puerta real. Consultarlo requiere apartarse del lector orientado al
este, manteniendo la presencia sobre la misma plataforma. El ensayo manual ya
existente produce entonces el cierre visible de 1 y la liberación de 2.
La segunda puerta permanece oculta por la esquina; su miniatura repite el
movimiento real, no una animación de ejemplo.

Dos contactos móviles, junto al lector y al panel, siguen
`OpticalReader.HasFacingPresence`, incluida la breve calificación. Una barra
continua conserva la alimentación al girar; el perno sólo se retrae con
`BlindAlignmentRelay.IsExitAuthorized`. Forma, movimiento y alimentación
separada complementan el color. Las marcas de esquina acompañan el descenso
real del puesto. Un conducto une visualmente lector y panel; una luz local fija
mantiene legibilidad. No hay secuencia automática, nuevos textos, objetivo ni
paso obligatorio de tutorial.

Se retienen los contactos del lector y motores originales. El contactor ya
existente se sitúa en el panel para localizar allí su confirmación de salida;
no hay clips nuevos, volumen mayor ni cambios al mezclador. El reset de
`P2AirlockPresentation` reinicia el nuevo componente visual. Lector, puertas,
alimentación, seguridad de umbrales, cruce válido y datos del Echo no cambian.

## Aplicación en el Editor central

Cherry-pick del commit entregado sobre la base integrada. Con **Incursion ya
abierta y fuera de Play**, ejecutar:

```csharp
P2CausalIntegration.ValidateCurrentScene();
P2CausalIntegration.ApplyToCurrentScene();
```

También disponible en `Echo Protocol > P2 Causal`. Valida el relay alimentado,
referencias/recorridos de ambas hojas, volúmenes, geometría del puesto y lector,
salida WorldSFX y materiales existentes. Necesita las hojas cerradas en Edit
Mode. Una instalación completa es un no-op; una parcial aborta. Tiene Undo
agrupado y rollback; no abre, reemplaza ni guarda escenas. Añade objetos sin
colisión, mueve sólo presentación de P2 y deja la escena sucia para revisión.
El coordinador decide guardar después del chequeo. No importa assets privados.

## Evidencia central

En el Editor principal se comprobó mirar/girar/abandonar el puesto y la relación
real entre alimentación, cierre y autorización de salida. Se inspeccionó la cámara
y se añadió una luz fija sobre la primera hoja, inicialmente demasiado oscura.
Se corrigió el GUID inválido de la carpeta nueva. No se repitió el recorrido
completo con RECALL ni se aprobó comprensión espontánea. Detalle de alcance en
`PARALLEL_ROUND_20261023.md`.

## Evidencia del worktree y checks propuestos (no todos ejecutados)

Revisión focal de fuentes y `git diff --check`. Compilación aislada de las
fuentes afectadas y dependencias directas con Roslyn/módulos de Unity
6000.3.21f1: **sin errores**; salida temporal eliminada. Un primer comando
incluyó assemblies duplicados y se corrigió su lista de referencias. No se
abrió Unity ni se creó Library, build, player o harness. Esto no verifica
serialización aplicada, Console del proyecto, encuadre ni escucha.

1. Tras compilar/aplicar, revisar Console y ensayar desde el puesto: mirar
   lector, girar hacia el panel sin caminar, volver a mirar. La primera hoja
   cierra junto al testigo, la alimentación sigue y el perno libera la segunda
   sólo al cierre real. Al volver al lector se revierte. Confirmar encuadre y
   texto a la resolución habitual; pausa congela el movimiento.
2. Una toma real mirar/esperar/apartarse conservando presencia permite entrada,
   cierre alimentado, salida y `HasTransferred`; RECALL restaura el testigo.
   Una toma que abandona el puesto antes del cierre pierde alimentación y no
   obtiene cruce válido. Reutilizar la ruta/checkpoint de P2, sin campaña.

Dos preguntas humanas: **¿Podés anticipar qué hará cada puerta antes de RECALL
sin explicación externa? ¿El giro en el mismo puesto te permitió preparar tu
toma, o seguís interpretando que hay que caminar/esperar a que termine?**

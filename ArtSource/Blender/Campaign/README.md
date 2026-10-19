# Kit de campaña

Modelado original mediante Blender 5.2 LTS, unidades en metros. No contiene
mallas, texturas ni voces descargadas de terceros.

Desde la raíz del proyecto, ejecutar en orden con Blender `--background --python`:

1. `build_campaign_kit.py`: doce modelos base; guarda `EchoCampaignKit.blend`.
2. `build_dressing.py`: cinco modelos de detalle; guarda `EchoDressing.blend`.
3. `build_recorded_figure.py`: reemplaza la figura base por el adulto final con
   abrigo continuo y pliegues; guarda `EchoRecordedFigure.blend`.

FBX finales en `Assets/_Game/Art/Models/Campaign/`. La instancia Unity conserva
la rotación de conversión del importador y aplica encima la orientación de escena.
El actor del Echo mantiene la misma pose gráfica fija; no representa una nueva
pista de animación grabada. La superficie translúcida se asigna en Unity.

La escena se configura con `CampaignProduction.Author()` mediante Editor APIs.
Ese método reconstruye sólo la escena nueva de campaña a partir del checkpoint
breve; no debe ejecutarse sobre cambios manuales de la campaña sin resguardarlos.

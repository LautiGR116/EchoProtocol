using UnityEngine;
namespace EchoProtocol.Campaign
{
    // Physical instrument feedback. The recorded trajectory is never changed by presentation.
    public sealed class StationFeedback:MonoBehaviour
    {
        [SerializeField] CampaignStation station;
        [SerializeField] CampaignDirector director;
        [SerializeField] Renderer pilot;
        [SerializeField] Transform needle;
        MaterialPropertyBlock block;Quaternion baseline;
        void Awake(){block=new MaterialPropertyBlock();if(needle)baseline=needle.localRotation;}
        void Update()
        {
            if(!station||!director||!pilot)return;
            bool valve=station.Kind==StationKind.ValveA||station.Kind==StationKind.ValveB;
            float value=valve?director.Pressure:station.Caption.Contains("ESTABLE")?1:director.WorkingStation==station?director.WorkFraction:0;
            Color c=Color.Lerp(new Color(.26f,.09f,.025f),new Color(.14f,.64f,.39f),value);
            pilot.GetPropertyBlock(block);block.SetColor("_BaseColor",c);block.SetColor("_EmissionColor",c*.6f);pilot.SetPropertyBlock(block);
            if(needle)needle.localRotation=baseline*Quaternion.Euler(0,0,Mathf.Lerp(-105,105,value));
        }
    }
}

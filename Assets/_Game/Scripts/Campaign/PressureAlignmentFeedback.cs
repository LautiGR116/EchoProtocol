using UnityEngine;
namespace EchoProtocol.Campaign
{
    // Preview reports actual alignment; only an Echo can supply pressure in CampaignDirector.
    public sealed class PressureAlignmentFeedback:MonoBehaviour
    {
        [SerializeField] CampaignDirector director;
        [SerializeField] Renderer surface;
        MaterialPropertyBlock block;
        void Update()
        {
            if(!director||!surface)return;
            if(block==null)block=new MaterialPropertyBlock();
            bool aligned=director.PressureSubjectAligned;
            surface.GetPropertyBlock(block);
            block.SetColor("_BaseColor",aligned?new Color(.12f,.58f,.51f):new Color(.18f,.115f,.04f));
            block.SetColor("_EmissionColor",aligned?new Color(.06f,.38f,.32f):Color.black);
            surface.SetPropertyBlock(block);
        }
    }
}

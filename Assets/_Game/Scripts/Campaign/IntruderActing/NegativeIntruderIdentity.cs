using UnityEngine;

namespace EchoProtocol.Campaign.IntruderActing
{
    // Scene-local installation receipt; no Update and no Echo behaviour.
    [DisallowMultipleComponent]
    public sealed class NegativeIntruderIdentity : MonoBehaviour
    {
        public int revision;
        public SkinnedMeshRenderer body;
        public Mesh priorBody;
        public Material[] priorMaterials;
        public GameObject silhouette;
    }
}

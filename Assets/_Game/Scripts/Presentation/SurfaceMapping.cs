using UnityEngine;

namespace EchoProtocol.Presentation
{
    // MaterialPropertyBlock itself is transient. Persist the authored repeat and reapply on load.
    [ExecuteAlways]
    public sealed class SurfaceMapping:MonoBehaviour
    {
        [SerializeField] private Renderer surface;
        [SerializeField] private Vector2 repeat=Vector2.one;
        private MaterialPropertyBlock block;
        public void Configure(Renderer target,Vector2 tiling){surface=target;repeat=tiling;Apply();}
        private void OnEnable()=>Apply();
        private void OnValidate()=>Apply();
        public void Apply()
        {
            if(!surface)return;
            if(block==null)block=new MaterialPropertyBlock();
            surface.GetPropertyBlock(block);block.SetVector("_BaseMap_ST",new Vector4(repeat.x,repeat.y,0,0));surface.SetPropertyBlock(block);
        }
    }
}

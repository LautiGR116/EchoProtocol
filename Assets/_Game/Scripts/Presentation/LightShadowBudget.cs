using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace EchoProtocol.Presentation
{
    // Reserve punctual shadow maps for the four nearest visible light volumes.
    // Other lights retain their authored color/intensity and circuit state.
    [ExecuteAlways]
    public sealed class LightShadowBudget:MonoBehaviour
    {
        [SerializeField] Light[] lights;
        [SerializeField] BoxCollider enclosedResidence;
        [SerializeField] Light[] residenceLights;
        [SerializeField] bool incrementalShadowChanges;
        readonly System.Collections.Generic.HashSet<Light> selected = new System.Collections.Generic.HashSet<Light>();
        readonly System.Collections.Generic.HashSet<Light> previous = new System.Collections.Generic.HashSet<Light>();
        readonly Light[] nearest = new Light[4];
        readonly float[] distances = new float[4];
        bool initialized;
        void OnEnable(){initialized=false;RenderPipelineManager.beginCameraRendering+=BeforeCamera;}
        void OnDisable(){RenderPipelineManager.beginCameraRendering-=BeforeCamera;if(lights!=null)foreach(var l in lights)if(l)l.shadows=LightShadows.Soft;}
        void BeforeCamera(ScriptableRenderContext context,Camera camera)
        {
            if(lights==null)return;
            if(incrementalShadowChanges){ApplyIncremental(camera);return;}
            foreach(var l in lights)if(l)l.shadows=LightShadows.None;
            for(int slot=0;slot<4;slot++)
            {
                Light nearest=null;float distance=144;
                foreach(var l in lights)
                {if(!l||!l.enabled||!l.gameObject.activeInHierarchy||l.shadows!=LightShadows.None)continue;float d=(l.transform.position-camera.transform.position).sqrMagnitude;if(d<distance){distance=d;nearest=l;}}
                if(nearest){nearest.shadows=LightShadows.Soft;nearest.shadowCustomResolution=256;}
            }
            // Small enclosed rooms must never gain light through a wall when another lamp is nearer.
            if(enclosedResidence && enclosedResidence.bounds.Contains(camera.transform.position) && residenceLights!=null)
                foreach(var light in residenceLights)if(light && light.enabled && light.gameObject.activeInHierarchy)
                    {light.shadows=LightShadows.Soft;light.shadowCustomResolution=256;}
        }
        // Same four-nearest selection and enclosed-room exception as the legacy
        // path, but visit the array once and change shadow mode only on a change
        // of ownership. The opt-in scene keeps older editions on their old path.
        void ApplyIncremental(Camera camera)
        {
            if(!initialized){foreach(var l in lights)if(l)l.shadows=LightShadows.None;previous.Clear();initialized=true;}
            selected.Clear();
            for(int i=0;i<4;i++){nearest[i]=null;distances[i]=144;}
            Vector3 position=camera.transform.position;
            foreach(var l in lights)
            {
                if(!l||!l.enabled||!l.gameObject.activeInHierarchy)continue;
                float d=(l.transform.position-position).sqrMagnitude;
                for(int slot=0;slot<4;slot++)if(d<distances[slot])
                {for(int tail=3;tail>slot;tail--){distances[tail]=distances[tail-1];nearest[tail]=nearest[tail-1];}distances[slot]=d;nearest[slot]=l;break;}
            }
            foreach(var l in nearest)if(l)selected.Add(l);
            if(enclosedResidence&&enclosedResidence.bounds.Contains(position)&&residenceLights!=null)
                foreach(var l in residenceLights)if(l&&l.enabled&&l.gameObject.activeInHierarchy)selected.Add(l);
            foreach(var l in previous)if(l&&!selected.Contains(l))l.shadows=LightShadows.None;
            foreach(var l in selected)if(!previous.Contains(l)){l.shadows=LightShadows.Soft;l.shadowCustomResolution=256;}
            previous.Clear();foreach(var l in selected)previous.Add(l);
        }
    }
}

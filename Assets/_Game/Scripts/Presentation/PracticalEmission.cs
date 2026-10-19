using UnityEngine;
namespace EchoProtocol.Presentation
{
    // A visible diffuser cannot stay lit after its authored circuit has failed.
    [ExecuteAlways]
    [DefaultExecutionOrder(-150)]
    public sealed class PracticalEmission:MonoBehaviour
    {
        [SerializeField] Light source;
        [SerializeField] Renderer surface;
        [SerializeField] int materialIndex;
        [SerializeField] Color emission=new Color(.8f,.95f,.88f);
        [SerializeField] Color litColor=new Color(.62f,.73f,.68f);
        MaterialPropertyBlock block;
        Light baselineSource;
        float baselineIntensity;
        static readonly Color UnlitColor=new Color(.065f,.073f,.07f);
        public float CurrentIntensityFactor{get;private set;}

        // Capture authored intensity before normal-order event components dim
        // their lights. A source that starts at zero acquires a baseline only
        // when it first receives positive power, never by dividing by zero.
        void Awake(){CaptureBaseline();}
        void CaptureBaseline()
        {
            if(baselineSource!=source){baselineSource=source;baselineIntensity=0f;}
            if(source&&baselineIntensity<=Mathf.Epsilon&&source.intensity>Mathf.Epsilon)
                baselineIntensity=source.intensity;
        }
        void LateUpdate()
        {
            if(!surface){CurrentIntensityFactor=0f;return;}
            CaptureBaseline();
            if(block==null)block=new MaterialPropertyBlock();
            CurrentIntensityFactor=source&&source.enabled&&source.gameObject.activeInHierarchy&&baselineIntensity>Mathf.Epsilon
                ?Mathf.Clamp01(source.intensity/baselineIntensity):0f;
            surface.GetPropertyBlock(block,materialIndex);
            block.SetColor("_EmissionColor",emission*CurrentIntensityFactor);
            block.SetColor("_BaseColor",Color.Lerp(UnlitColor,litColor,CurrentIntensityFactor));
            surface.SetPropertyBlock(block,materialIndex);
        }
    }
}

Shader "EchoProtocol/DiegeticText"
{
    Properties { _MainTex ("Font atlas", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; half fog : TEXCOORD1; half distanceFade : TEXCOORD2; };
            Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; o.color=v.color; o.fog=ComputeFogFactor(o.positionCS.z); o.distanceFade=saturate((12-distance(TransformObjectToWorld(v.positionOS.xyz),_WorldSpaceCameraPos))/5); return o; }
            half4 frag(Varyings i) : SV_Target { return half4(MixFog(i.color.rgb,i.fog), i.color.a*i.distanceFade*SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a); }
            ENDHLSL
        }
    }
}

Shader "EchoProtocol/RecordedSurface"
{
    Properties { _Color ("Echo color", Color) = (0.16,0.58,0.56,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half fog:TEXCOORD2; };
            Varyings vert(Attributes v)
            {
                Varyings o;o.positionWS=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                half edge=pow(1-saturate(dot(normalize(i.normalWS),GetWorldSpaceNormalizeViewDir(i.positionWS))),2.1);
                half3 color=_Color.rgb*(.42+edge*.9);
                // Fade the complete visual near the camera; recorded pose and sensors are unchanged.
                float3 origin=TransformObjectToWorld(float3(0,0,0));
                half nearFade=smoothstep(.35,.95,distance(origin.xz,_WorldSpaceCameraPos.xz));
                return half4(MixFog(color,i.fog),(.17+edge*.52)*nearFade);
            }
            ENDHLSL
        }
    }
}

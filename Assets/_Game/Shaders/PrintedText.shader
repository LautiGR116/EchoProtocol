Shader "EchoProtocol/PrintedText"
{
    Properties { _MainTex ("Font atlas", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; float3 positionWS : TEXCOORD1; half3 normalWS : TEXCOORD2; half fog : TEXCOORD3; half distanceFade : TEXCOORD4; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                // TextMesh glyphs lie in the local XY plane and face local -Z.
                // Their generated vertex stream need not contain mesh normals.
                o.normalWS=TransformObjectToWorldNormal(float3(0,0,-1));
                o.uv=v.uv; o.color=v.color;
                o.fog=ComputeFogFactor(o.positionCS.z);
                o.distanceFade=saturate((12-distance(o.positionWS,_WorldSpaceCameraPos))/5);
                return o;
            }
            half3 Diffuse(Light light,half3 normal)
            { return light.color*(light.distanceAttenuation*light.shadowAttenuation*saturate(dot(normal,light.direction))); }
            half4 frag(Varyings i) : SV_Target
            {
                half3 normal=normalize(i.normalWS);
                half3 illumination=SampleSH(normal);
                illumination+=Diffuse(GetMainLight(TransformWorldToShadowCoord(i.positionWS)),normal);
                #ifdef _ADDITIONAL_LIGHTS
                InputData inputData=(InputData)0;
                inputData.positionWS=i.positionWS;
                inputData.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for(uint lightIndex=0;lightIndex<min(URP_FP_DIRECTIONAL_LIGHTS_COUNT,MAX_VISIBLE_LIGHTS);lightIndex++)
                    illumination+=Diffuse(GetAdditionalLight(lightIndex,i.positionWS,half4(1,1,1,1)),normal);
                #endif
                uint count=GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                    illumination+=Diffuse(GetAdditionalLight(lightIndex,i.positionWS,half4(1,1,1,1)),normal);
                LIGHT_LOOP_END
                #endif
                half3 ink=i.color.rgb*illumination;
                return half4(MixFog(ink,i.fog),i.color.a*i.distanceFade*SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a);
            }
            ENDHLSL
        }
    }
}

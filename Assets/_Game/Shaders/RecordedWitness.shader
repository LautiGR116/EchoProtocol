// Project-authored matte adult display, 2026-10-02. Same immutable root motion.
// A display surface for position/body-yaw playback, not captured appearance.
Shader "EchoProtocol/RecordedWitness"
{
    Properties
    {
        [MainColor] _Color ("Recorded surface tint", Color) = (0.14,0.68,0.64,1)
        _BodyOpacity ("Body opacity", Range(0,1)) = 0.90
        _EdgeOpacity ("Additional edge opacity", Range(0,0.15)) = 0.03
        _RecordTint ("Restrained record identity", Color) = (0.08,0.30,0.28,1)
        _Weave ("Fixed cloth relief", Range(0,0.2)) = 0.06
        _WorldLightGain ("Current-world diffuse response", Range(0,1)) = 0.65
        _RecordLevel ("Stylized record relief", Range(0,0.5)) = 0.32
        _NearFadeStart ("Near fade hidden distance", Float) = 0.35
        _NearFadeEnd ("Near fade full distance", Float) = 0.95
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        Pass
        {
            Name "RecordedPresenceForward"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex RecordedVertex
            #pragma fragment RecordedFragment
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            // Transparent geometry must sample the real main shadow map rather
            // than an opaque surface's screen-space shadow at this pixel.
            #define _SURFACE_TYPE_TRANSPARENT 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _RecordTint;
                half _Weave;
                half _BodyOpacity;
                half _EdgeOpacity;
                half _WorldLightGain;
                half _RecordLevel;
                float _NearFadeStart;
                float _NearFadeEnd;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 fogAndVertexLight : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings RecordedVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogAndVertexLight.x = ComputeFogFactor(position.positionCS.z);
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                    output.fogAndVertexLight.yzw = VertexLighting(position.positionWS, output.normalWS);
                #endif
                return output;
            }

            half3 RecordedDiffuse(Light light, half3 normalWS, uint meshLayers)
            {
                #if defined(_LIGHT_LAYERS)
                    if (!IsMatchingLightLayer(light.layerMask, meshLayers)) return half3(0,0,0);
                #endif
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                return light.color * (attenuation * saturate(dot(normalWS, light.direction)));
            }

            half4 RecordedFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 originWS = TransformObjectToWorld(float3(0,0,0));
                float farDistance = max(_NearFadeStart + 0.001, _NearFadeEnd);
                half nearFade = smoothstep(_NearFadeStart, farDistance,
                    distance(originWS.xz, _WorldSpaceCameraPos.xz));
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                half3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // LIGHT_LOOP_BEGIN's URP17.3 clustered path consumes these exact
                // InputData fields; GetAdditionalLightsCount alone returns 0 in F+.
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                half4 shadowMask = half4(1,1,1,1); // Dynamic, unbaked display body.
                float4 shadowCoord = float4(0,0,0,0);
                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #endif
                uint meshLayers = GetMeshRenderingLayer();
                half3 illumination = max(half3(0,0,0), SampleSH(normalWS));
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);
                illumination += RecordedDiffuse(mainLight, normalWS, meshLayers);

                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    #if USE_CLUSTER_LIGHT_LOOP
                        [loop] for (uint lightIndex = 0;
                            lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS);
                            ++lightIndex)
                        {
                            CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                            Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
                            illumination += RecordedDiffuse(light, normalWS, meshLayers);
                        }
                    #endif
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
                        illumination += RecordedDiffuse(light, normalWS, meshLayers);
                    LIGHT_LOOP_END
                #endif
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                    illumination += input.fogAndVertexLight.yzw;
                #endif

                // Diffuse only: no glass/specular highlight or additive halo.
                // Hue-preserving shoulder bounds bright practical-light response.
                half peak = max(illumination.r, max(illumination.g, illumination.b));
                half3 worldResponse = illumination / (1.0h + peak);

                // Explicit, modest display relief: a fixed upper-left view-space
                // normal shading term, not a world light or recorded channel.
                // It has no time, chapter, event, actor-state or movement input.
                half3 normalVS = normalize(mul((half3x3)UNITY_MATRIX_V, normalWS));
                half relief = 0.4h + 0.6h * saturate(dot(normalVS,
                    normalize(half3(-0.35h,0.60h,0.72h))));
                // Fixed millimetre weave fades when underresolved; no time or actor input.
                float weavePhase = dot(input.positionWS, float3(700.0, 940.0, 680.0));
                half weaveVisibility = saturate(1.0 - fwidth(weavePhase));
                half weave = 1.0 - _Weave * weaveVisibility * (0.5 + 0.5 * sin(weavePhase));
                half3 color = _Color.rgb * (_WorldLightGain * worldResponse + _RecordLevel * relief) * weave;
                // A modest cool record undertone and edge preserve first-use identity.
                half recordEdge = pow(1.0 - saturate(dot(normalWS, viewWS)), 3.0);
                color += _RecordTint.rgb * (0.028 + 0.065 * recordEdge);

                // Interior coverage dominates; the edge adds no color/luminance.
                half edge = 1.0h - saturate(dot(normalWS, viewWS));
                half alpha = saturate(_BodyOpacity + _EdgeOpacity * edge * edge) * _Color.a * nearFade;
                return half4(MixFog(color, input.fogAndVertexLight.x), alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}

// Self-occlusion for the recorded skinned display, using the color pass vertex path.
// Queue before its transparent color; never casts a world shadow.
Shader "EchoProtocol/RecordedArticulatedDepth"
{
    Properties
    {
        _NearFadeStart ("Near fade hidden distance", Float) = 0.35
        _NearFadeEnd ("Near fade full distance", Float) = 0.95
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "RecordedSelfDepth"
            Tags { "LightMode"="UniversalForwardOnly" }
            ColorMask 0
            ZWrite On
            ZTest LEqual
            Offset 1, 1
            Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _NearFadeStart;
                float _NearFadeEnd;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vertex(Attributes input)
            {
                Varyings output=(Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input,output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=position.positionCS;
                return output;
            }
            half4 Fragment(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 originWS=TransformObjectToWorld(float3(0,0,0));
                // Exactly the color surface's root-distance fade. A fully hidden
                // Echo must not leave an invisible occluder in front of glass.
                float nearFade=smoothstep(_NearFadeStart,max(_NearFadeStart+.001,_NearFadeEnd),distance(originWS.xz,_WorldSpaceCameraPos.xz));
                clip(nearFade-.0001);
                return 0;
            }
            ENDHLSL
        }
    }
    Fallback Off
}

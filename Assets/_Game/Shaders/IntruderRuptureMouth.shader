Shader "EchoProtocol/IntruderRuptureMouth"
{
    Properties
    {
        _BaseMap("Licensed lip detail", 2D) = "white" {}
        _BaseColor("Lip tint", Color) = (1,1,1,1)
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "IntruderDreadSurface.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            // Own binding, like EmbestidaSurface. Borrowed Lit depth/shadow
            // passes do not share this buffer layout on Metal.
            float4 _BaseColor;
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; float2 oral:TEXCOORD1; float3 rest:TEXCOORD2; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float2 uv:TEXCOORD2; float2 oral:TEXCOORD3; float3 rest:TEXCOORD4; float fog:TEXCOORD5; };
            V vert(A a)
            {
                V o; VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);
                o.positionCS=p.positionCS; o.world=p.positionWS; o.normal=TransformObjectToWorldNormal(a.normalOS);
                o.uv=a.uv; o.oral=a.oral; o.rest=a.rest; o.fog=ComputeFogFactor(p.positionCS.z); return o;
            }
            half4 frag(V i):SV_Target
            {
                float depth=saturate(i.oral.y);
                float3 scan=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
                // The first ring matches the skin shader at the anatomical
                // boundary; mucosa appears only as the wall turns into depth.
                IntruderDreadSurfaceData edge=EvaluateIntruderDreadSurface(i.rest,i.uv,scan,_BaseColor.rgb,0,float3(0,0,1));
                float3 lip=EvaluateIntruderRuptureSkin(i.rest,scan,edge.albedo);
                float ridge=sin(i.oral.x*150.796+depth*7)*sin(depth*36+i.oral.x*12.566);
                float3 mucosa=float3(.105,.038,.034)*(1+.13*ridge);
                float3 albedo=lerp(lip,mucosa,smoothstep(0,.30,depth));
                albedo*=lerp(1,.20,smoothstep(.28,1,depth));
                float3 n=normalize(i.normal);
                InputData input=(InputData)0; input.positionWS=i.world; input.normalWS=n;
                input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
                input.shadowCoord=TransformWorldToShadowCoord(i.world); input.bakedGI=SampleSH(n);
                input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS); input.shadowMask=half4(1,1,1,1);
                SurfaceData surface=(SurfaceData)0; surface.albedo=albedo; surface.alpha=1;
                surface.occlusion=lerp(1,.12,smoothstep(.10,.85,depth));
                surface.smoothness=lerp(.42,.65,smoothstep(0,.3,depth))-.09*saturate(ridge);
                surface.specular=half3(.04,.04,.04);
                half4 c=UniversalFragmentPBR(input,surface); c.rgb=MixFog(c.rgb,i.fog); return c;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}

Shader "EchoProtocol/EmbestidaSurface"
{
    Properties
    {
        _BaseMap("Existing licensed scan detail", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1,1,1,1)
        _BumpMap("Surface normal", 2D) = "bump" {}
        _BumpScale("Normal strength", Float) = .55
        _Apron("Original rubber garment", Float) = 0
        _IntruderFace("Intruder face / preserve scanned skin", Range(0,1)) = 0
        _IntruderRupture("Round 6 / forced mouth skin (opt in)", Range(0,1)) = 0
        _IntruderDreadSurface("Round 5 / contained violence surface (opt in)", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
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
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor; float4 _BaseMap_ST; float _BumpScale; float _IntruderFace; float _IntruderDreadSurface; float _IntruderRupture;
            CBUFFER_END
            // Deliberately opt this two-renderer shader out of the SRP Batcher.
            // Borrowed Lit depth/shadow passes have a different material layout;
            // Metal otherwise reused RubberBib's binding for Skin in P3.
            float _Apron;
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 tangentOS:TANGENT; float2 uv:TEXCOORD0; float3 rest:TEXCOORD2; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float4 tangent:TEXCOORD2; float2 uv:TEXCOORD3; float3 rest:TEXCOORD4; float fog:TEXCOORD5; };
            V vert(A a)
            {
                V o; VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);
                VertexNormalInputs n=GetVertexNormalInputs(a.normalOS,a.tangentOS);
                o.positionCS=p.positionCS; o.world=p.positionWS; o.normal=n.normalWS;
                o.tangent=float4(n.tangentWS,a.tangentOS.w*GetOddNegativeScale());
                o.uv=a.uv; o.rest=a.rest; o.fog=ComputeFogFactor(p.positionCS.z); return o;
            }
            float spot(float3 p,float3 c,float3 width) { float3 d=(p-c)/width; return exp(-dot(d,d)*2); }
            half4 frag(V i):SV_Target
            {
                float3 p=i.rest; float3 scan=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
                float l=dot(scan,float3(.26,.58,.16));
                float skin=smoothstep(1.53,1.60,p.y)*(1-smoothstep(.15,.24,abs(p.x)));
                skin=max(skin,smoothstep(.67,.75,abs(p.x)));
                float grain=sin(p.x*217+sin(p.y*173))*sin(p.z*263+p.y*291);
                float stain=saturate(.42+.25*sin(p.y*18+p.x*23)+.19*sin(p.x*74+p.y*39+p.z*41));
                float3 cloth=lerp(float3(.105,.14,.12),float3(.32,.30,.20),saturate(l*1.8))*(.60+.40*stain);
                // Water-dark cuffs, knees and hem, with a dirty ochre work stripe.
                float knees=spot(p,float3(.12,.48,.06),float3(.15,.16,.18))+spot(p,float3(-.12,.48,.06),float3(.15,.16,.18));
                cloth*=1-.33*saturate(knees);
                float stripe=(1-smoothstep(.014,.025,abs(p.y-1.28)))*(1-smoothstep(.20,.25,abs(p.x)));
                cloth=lerp(cloth,float3(.38,.31,.16)*(l+.25),stripe*.65);
                float3 flesh=lerp(scan,float3(l*.93,l*.94,l*.81),.85)*1.12;
                float eyes=spot(p,float3(-.030,1.719,.102),float3(.037,.027,.040))+spot(p,float3(.030,1.719,.102),float3(.037,.027,.040));
                float cheek=spot(p,float3(-.049,1.673,.104),float3(.023,.037,.03))+spot(p,float3(.049,1.673,.104),float3(.023,.037,.03));
                flesh=lerp(flesh,float3(.063,.040,.041),saturate(eyes)*.82);
                flesh=lerp(flesh,float3(.20,.15,.13),saturate(cheek)*.4);
                float split=spot(p,float3(.024,1.649,.114),float3(.013,.029,.035));
                float knuckles=smoothstep(.77,.86,abs(p.x))*(.42+.58*stain);
                flesh=lerp(flesh,float3(.14,.045,.032),saturate(split*.75+knuckles*.55));
                float scalp=smoothstep(1.755,1.80,p.y)*(1-smoothstep(.042,.078,abs(p.x)));
                flesh=lerp(flesh,float3(.27,.28,.235)*(1+grain*.035),scalp);
                float sore=spot(p,float3(-.052,1.683,.105),float3(.023,.042,.03));
                flesh=lerp(flesh,float3(.21,.067,.049)*(1+grain*.16),sore*.70);
                flesh*=1+grain*.025;
                float3 base=lerp(cloth,flesh,skin)*_BaseColor.rgb;
                float drip=saturate(sin(p.x*143+sin(p.x*57)*2)+.35)*smoothstep(1.22,.63,p.y);
                base=lerp(base,lerp(float3(.26,.21,.115),float3(.09,.075,.045),stain*.5+drip*.4)*(1+grain*.035),_Apron);
                base=lerp(base,float3(.34,.32,.24)*(.72+stain*.28),_Apron*step(.65,abs(p.x)));
                float3 n=normalize(i.normal); float3 t=normalize(i.tangent.xyz);
                float3 bump=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
                bump=lerp(bump,float3(0,0,1),max(scalp,_Apron));
                float smoothness=lerp(lerp(.20,.36,skin)+stain*.04,.14,_Apron);
                // Opt-in material on the private intruder only. TEXCOORD2 is the
                // unmodified Nathan rest position in metres, unaffected by acting.
                // Keep the historical surface and rubber garment path unchanged.
                if (_IntruderFace > 0 && _Apron <= 0)
                {
                    float faceNeck=smoothstep(1.56,1.61,p.y)*(1-smoothstep(.12,.16,abs(p.x)));
                    float faceWeight=saturate(_IntruderFace)*faceNeck;
                    // Let the sculpt and lighting describe the sockets and folds:
                    // retain photographic skin/lip variation, stubble and pores;
                    // no painted eye circles, cheek sores or flat scalp colour.
                    // Reduce the healthy pink cast without inventing bruises or
                    // flattening the scanned light/dark variation into makeup.
                    float scanLuminance=dot(scan,float3(.2126,.7152,.0722));
                    float3 quietSkin=lerp(scan,scanLuminance.xxx,.23);
                    base=lerp(base,quietSkin*_BaseColor.rgb,faceWeight);
                    float3 scanBump=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
                    bump=lerp(bump,scanBump,faceWeight);
                    // A restrained dielectric response: dry cheek/neck, slightly
                    // less rough forehead/nose and lips. No painted highlights.
                    float forehead=spot(p,float3(0,1.749,.096),float3(.060,.026,.038));
                    float nose=spot(p,float3(0,1.698,.133),float3(.018,.030,.037));
                    float lips=spot(p,float3(0,1.651,.115),float3(.032,.008,.022));
                    float faceSmoothness=.25+.11*saturate(forehead)+.16*saturate(nose)+.06*saturate(lips);
                    faceSmoothness=lerp(faceSmoothness,.20,smoothstep(1.76,1.80,p.y));
                    smoothness=lerp(smoothness,faceSmoothness,faceWeight);
                }
                // Explicit opt-in on the existing private intruder materials.
                // Original scan coordinates survive the R5 anatomy upgrade;
                // no actor, bone, UV or shared texture is modified here.
                if (_IntruderDreadSurface > 0)
                {
                    IntruderDreadSurfaceData dread = EvaluateIntruderDreadSurface(
                        p, i.uv, scan, _BaseColor.rgb, _Apron, bump);
                    float weight = saturate(_IntruderDreadSurface);
                    base = lerp(base, dread.albedo, weight);
                    bump = lerp(bump, dread.normalTS, weight);
                    smoothness = lerp(smoothness, dread.smoothness, weight);
                }
                // R6 is assigned only on the private skin clone. Preserve the
                // R5 binding fix: _Apron remains outside UnityPerMaterial.
                base=lerp(base,EvaluateIntruderRuptureSkin(p,scan,base),saturate(_IntruderRupture)*(1-saturate(_Apron)));
                n=normalize(t*bump.x+cross(n,t)*i.tangent.w*bump.y+n*bump.z);
                InputData input=(InputData)0; input.positionWS=i.world; input.normalWS=n;
                input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
                input.shadowCoord=TransformWorldToShadowCoord(i.world); input.bakedGI=SampleSH(n);
                input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);input.shadowMask=half4(1,1,1,1);
                SurfaceData surface=(SurfaceData)0;surface.albedo=base;surface.alpha=1;surface.occlusion=1;
                surface.smoothness=smoothness;surface.specular=half3(.04,.04,.04);
                half4 c=UniversalFragmentPBR(input,surface);c.rgb=MixFog(c.rgb,i.fog);return c;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}

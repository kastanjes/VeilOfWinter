
Shader "URP/SnowShader_NoInteraction"
{
    Properties
    {
        [Header(Snow)]
        [HDR]_Color("Snow Color", Color) = (0.8, 0.8, 0.9, 1)
        _MainTex("Snow Texture", 2D) = "white" {}
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.5
        _SnowTextureScale("Snow Texture Scale", Range(0,5)) = 0.3

        [Header(Normal)]
        _Normal("Snow Normal", 2D) = "bump" {}
        _SnowNormalStrength("Snow Normal Strength", Range(0,1)) = 0.3

        [Header(Sparkles)]
        _SparkleScale("Sparkle Scale", Range(0,10)) = 10
        _SparkCutoff("Sparkle Cutoff", Range(0,2)) = 0.8
        _SparkleNoise("Sparkle Noise", 2D) = "gray" {}

        [Header(Rim)]
        _RimPower("Rim Power", Range(0,20)) = 5
        [HDR]_RimColor("Rim Color", Color) = (0.5,0.5,0.5,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
            };

            sampler2D _MainTex, _Normal, _SparkleNoise;
            float4 _Color;
            float _SnowTextureOpacity, _SnowTextureScale;
            float _SnowNormalStrength;
            float _SparkleScale, _SparkCutoff;
            float _RimPower;
            float4 _RimColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                OUT.uv = IN.uv * _SnowTextureScale;

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;

                OUT.normalWS = normalWS;
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.tangentWS = tangentWS;
                OUT.bitangentWS = bitangentWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 snowTex = tex2D(_MainTex, IN.uv).rgb;
                float3 finalSnow = lerp(_Color.rgb, snowTex * _Color.rgb, _SnowTextureOpacity);

                float3 normalTS = UnpackNormal(tex2D(_Normal, IN.uv));
                float3 normalWS = normalize(
                    normalTS.x * IN.tangentWS +
                    normalTS.y * IN.bitangentWS +
                    normalTS.z * IN.normalWS
                );

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 lighting = finalSnow * mainLight.color * NdotL;

                float rim = 1.0 - dot(normalWS, normalize(IN.viewDirWS));
                float3 rimLight = _RimColor.rgb * pow(rim, _RimPower);

                float sparkleMask = step(_SparkCutoff, tex2D(_SparkleNoise, IN.worldPos.xz * _SparkleScale).r);
                float3 sparkleColor = sparkleMask * 4.0;

                float3 finalColor = lighting + rimLight + sparkleColor;
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

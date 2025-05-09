Shader "URP/SnowSparkleCustomizable"
{
    Properties
    {
        _SnowColor("Snow Color", Color) = (1,1,1,1)
        [HDR]_ShadowTint("Shadow Tint", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowStrength("Shadow Strength", Range(0, 1)) = 1.0

        _MainTex("Snow Texture", 2D) = "white" {}
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.6
        _SnowTextureScale("Snow Texture Scale", Float) = 0.4

        _Normal("Snow Normal", 2D) = "bump" {}
        _SnowNormalStrength("Snow Normal Strength", Range(0,1)) = 0.8

        _SparkleNoise("Sparkle Noise", 2D) = "gray" {}
        _SparkleScale("Sparkle Scale", Float) = 2.0
        _SparkCutoff("Sparkle Cutoff", Float) = 0.8
        _SparkleIntensity("Sparkle Intensity", Float) = 4

        _DetailTex("Snow Detail Texture", 2D) = "white" {}
        _DetailOpacity("Snow Detail Opacity", Range(0, 1)) = 0.4
        _DetailScale("Snow Detail Scale", Float) = 2.0

        [HDR]_RimColor("Rim Color Snow", Color) = (1,1,1,1)
        _RimPower("Rim Power", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uvWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
                float4 shadowCoord : TEXCOORD6;
            };

            sampler2D _MainTex, _Normal, _SparkleNoise, _DetailTex;
            float4 _SnowColor, _ShadowTint, _RimColor;
            float _SnowTextureOpacity, _SnowTextureScale, _SnowNormalStrength;
            float _SparkleScale, _SparkCutoff, _SparkleIntensity;
            float _RimPower, _ShadowStrength;
            float _DetailOpacity, _DetailScale;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.uvWS = worldPos.xz;

                float3 tangentWS = normalize(TransformObjectToWorldDir(IN.tangentOS.xyz));
                float3 bitangentWS = cross(OUT.normalWS, tangentWS) * IN.tangentOS.w;
                OUT.tangentWS = tangentWS;
                OUT.bitangentWS = bitangentWS;

                OUT.shadowCoord = TransformWorldToShadowCoord(worldPos);
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float3 normalMap = UnpackNormal(tex2D(_Normal, IN.uvWS * _SnowTextureScale));
                float3 blendedNormal = normalize(
                    normalMap.r * IN.tangentWS +
                    normalMap.g * IN.bitangentWS +
                    normalMap.b * IN.normalWS
                );
                blendedNormal = normalize(lerp(IN.normalWS, blendedNormal, _SnowNormalStrength));

                float3 baseTex = tex2D(_MainTex, IN.uvWS * _SnowTextureScale).rgb;
                float3 detailTex = tex2D(_DetailTex, IN.uvWS * _DetailScale).rgb;
                float3 baseSnow = lerp(_SnowColor.rgb, baseTex * _SnowColor.rgb, _SnowTextureOpacity);
                baseSnow = lerp(baseSnow, detailTex * baseSnow, _DetailOpacity);

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(blendedNormal, lightDir));
                float shadowAtten = MainLightRealtimeShadow(IN.shadowCoord);
                shadowAtten = lerp(1, shadowAtten, _ShadowStrength);

                float3 litColor = baseSnow * mainLight.color.rgb * NdotL * shadowAtten;
                litColor = lerp(litColor, _ShadowTint.rgb, 1 - shadowAtten);

                float rim = 1.0 - dot(IN.viewDirWS, blendedNormal);
                float3 rimLight = _RimColor.rgb * pow(rim, _RimPower);

                float sparkle = tex2D(_SparkleNoise, IN.uvWS * _SparkleScale).r;
                float sparkleMask = step(_SparkCutoff, sparkle);
                float3 sparkleLight = sparkleMask * _SparkleIntensity;

                float3 finalColor = litColor + rimLight + sparkleLight;
                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

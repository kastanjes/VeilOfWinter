
Shader "URP/WorldSpaceSnowSparkle"
{
    Properties
    {
        _Color("Snow Color", Color) = (1,1,1,1)
        _MainTex("Snow Texture", 2D) = "white" {}
        _SnowTextureScale("Snow Texture Scale", Float) = 0.3
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.3

        _Noise("Snow Noise", 2D) = "gray" {}
        _NoiseScale("Noise Scale", Float) = 0.1
        _NoiseWeight("Noise Weight", Float) = 0.1

        [HDR]_RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Float) = 4.0

        _SparkleNoise("Sparkle Noise", 2D) = "white" {}
        _SparkleScale("Sparkle Scale", Float) = 10
        _SparkCutoff("Sparkle Cutoff", Float) = 0.9
        _SparkleIntensity("Sparkle Intensity", Float) = 4
        [HDR]_SparkleColor("Sparkle Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            sampler2D _MainTex, _Noise, _SparkleNoise;
            float4 _Color, _RimColor, _SparkleColor;
            float _SnowTextureScale, _SnowTextureOpacity;
            float _NoiseScale, _NoiseWeight;
            float _RimPower;
            float _SparkleScale, _SparkCutoff, _SparkleIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPos = worldPos;
                OUT.normalWS = normalize(normalWS);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uvNoise = IN.worldPos.xz * _NoiseScale;
                float2 uvSnow = IN.worldPos.zx * _SnowTextureScale;
                float2 uvSparkle = IN.worldPos.xz * _SparkleScale;

                float3 noiseTex = tex2D(_Noise, uvNoise).rgb;
                float3 snowTex = tex2D(_MainTex, uvSnow).rgb;

                float3 snowBase = lerp(_Color.rgb, snowTex * _Color.rgb, _SnowTextureOpacity);

                // Rim light
                float rim = 1.0 - dot(normalize(IN.viewDirWS), normalize(IN.normalWS)) * noiseTex.r;
                float3 rimLight = _RimColor.rgb * pow(rim, _RimPower);

                // Sparkle
                float sparkle = tex2D(_SparkleNoise, uvSparkle).r;
                float sparkleMask = step(_SparkCutoff, sparkle);
                float3 sparkleLight = _SparkleColor.rgb * sparkleMask * _SparkleIntensity;

                float3 finalColor = snowBase + rimLight + sparkleLight;
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

Shader "Custom/WorldSpaceSparkle"
{
    Properties
    {
        _SparkleNoise("Sparkle Noise", 2D) = "white" {}
        _SparkleScale("Sparkle Scale", Float) = 10
        _SparkCutoff("Sparkle Cutoff", Float) = 0.8
        [HDR]_SparkleColor("Sparkle Color", Color) = (1,1,1,1)
        _SparkleIntensity("Sparkle Intensity", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _SparkleNoise;
            float _SparkleScale;
            float _SparkCutoff;
            float4 _SparkleColor;
            float _SparkleIntensity;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 world = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(world);
                OUT.worldPos = world.xyz;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 sparkleUV = IN.worldPos.xz * _SparkleScale;
                float sparkleVal = tex2D(_SparkleNoise, sparkleUV).r;

                float sparkleMask = step(_SparkCutoff, sparkleVal);
                float3 finalColor = _SparkleColor.rgb * (sparkleMask * _SparkleIntensity);

                return float4(finalColor, 1.0); // ✅ this includes alpha (RGBA)

            }
            ENDHLSL
        }
    }
}

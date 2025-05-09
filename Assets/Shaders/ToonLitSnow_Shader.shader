
Shader "Toon/Lit Snow" {
    Properties{
        [Header(Main)]  
        _Noise("Snow Noise", 2D) = "gray" {}    
        _NoiseScale("Noise Scale", Range(0,2)) = 0.1
        _NoiseWeight("Noise Weight", Range(0,2)) = 0.1
        [HDR]_ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)
        [Space]
        [Header(Tesselation)]
        _MaxTessDistance("Max Tessellation Distance", Range(10,100)) = 50
        _Tess("Tessellation", Range(1,32)) = 20
        [Space]
        [Header(Snow)]
        [HDR]_Color("Snow Color", Color) = (0.5,0.5,0.5,1)
        _MainTex("Snow Texture", 2D) = "white" {}       
        _SnowHeight("Snow Height", Range(0,2)) = 0.3
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.3
        _SnowTextureScale("Snow Texture Scale", Range(0,2)) = 0.3

        [Space]
        [Header(Snow Path)]
        _PathBlending("Path Color Blending", Range(0,3)) = 2
        _SnowPathStrength("Snow Path Smoothness", Range(0,4)) = 2
        [HDR]_PathColorIn("Snow Path Color", Color) = (1,1,1,1)
        [HDR]_PathColorOut("Snow Path Color2", Color) = (0.5,0.5,1,1)
        
        [Space]
        [Header(Sparkles)]
        _SparkleScale("Sparkle Scale", Range(0,10)) = 10
        _SparkCutoff("Sparkle Cutoff", Range(0,10)) = 0.9
        _SparkleNoise("Sparkle Noise", 2D) = "gray" {}
        [Space]
        [Header(Rim)]
        _RimPower("Rim Power", Range(0,20)) = 20
        [HDR]_RimColor("Rim Color Snow", Color) = (0.5,0.5,0.5,1)
    }

    SubShader{
        Tags{ "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf ToonRamp vertex:vert addshadow nolightmap
        #pragma target 4.0

        sampler2D _MainTex, _Noise, _SparkleNoise;
        float4 _Color, _RimColor;
        float _RimPower;
        float _SnowTextureScale, _NoiseScale;
        float _SnowHeight, _SnowPathStrength;
        float4 _PathColorIn, _PathColorOut;
        float _PathBlending;
        float _NoiseWeight;
        float _SparkleScale, _SparkCutoff;
        float _SnowTextureOpacity;
        float4 _ShadowColor;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (atten + (_ShadowColor * (1-atten)));
            c.a = 0;
            return c;
        }

        void vert(inout appdata_full v) {}

        void surf(Input IN, inout SurfaceOutput o) {
            float3 noisetexture = tex2D(_Noise, IN.worldPos.zx * _NoiseScale);
            float3 snowtexture = tex2D(_MainTex, IN.worldPos.zx * _SnowTextureScale);

            half rim = 1.0 - dot(normalize(IN.viewDir), o.Normal) * noisetexture;
            float3 coloredRim =  _RimColor * pow(rim, _RimPower);

            float3 mainColors = lerp(_Color, snowtexture * _Color, _SnowTextureOpacity);
            o.Albedo = mainColors;

            float sparklesStatic = tex2D(_SparkleNoise, IN.worldPos.xz * _SparkleScale).r;
            float cutoffSparkles = step(_SparkCutoff, sparklesStatic);
            o.Emission = coloredRim + (cutoffSparkles * 4);
        }
        ENDCG
    }

    Fallback "Diffuse"
}

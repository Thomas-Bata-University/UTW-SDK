Shader "Custom/BiomeSingleTexture"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _MaskMap("Mask (R:AO G:Metal B:Rough)", 2D) = "black" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Tiling("Tiling", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskMap;
        sampler2D _NormalMap;
        float _Tiling;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex * _Tiling;

            fixed4 albedo = tex2D(_MainTex, uv);
            fixed4 mask = tex2D(_MaskMap, uv);
            fixed3 normalTex = UnpackNormal(tex2D(_NormalMap, uv));

            o.Albedo = albedo.rgb;
            o.Normal = normalTex;
            o.Metallic = mask.g;
            o.Smoothness = 1.0 - mask.b; // obráceně: B=roughness
            o.Occlusion = mask.r;
        }
        ENDCG
    }

    FallBack "Diffuse"
}

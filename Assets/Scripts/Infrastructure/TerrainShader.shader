Shader "Custom/BiomeShader"
{
    Properties
    {
        _ElevationMap ("Elevation Map", 2D) = "white" {}
        _TemperatureMap ("Temperature Map", 2D) = "white" {}
        _MoistureMap ("Moisture Map", 2D) = "white" {}
        _NoiseMap ("Noise Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _ElevationMap;
        sampler2D _TemperatureMap;
        sampler2D _MoistureMap;
        sampler2D _NoiseMap;

        struct Input
        {
            float2 uv_ElevationMap;
        };

        float3 get_biome_color(float elevation, float temperature, float moisture, float noise)
        {
            // Add noise to parameters
            temperature += noise * 0.2;
            moisture += noise * 0.2;
            
            // High elevation = mountain/snow
            if(elevation > 0.8) return float3(0.9, 0.9, 0.9); // Snow
            
            // Water detection
            if(elevation < 0.3) return float3(0.2, 0.4, 0.9); // Water

            // Biome decision matrix
            if(temperature < 0.3)
            {
                // Cold regions
                if(moisture < 0.4) return float3(0.6, 0.7, 0.6); // Tundra
                return float3(0.3, 0.5, 0.8); // Taiga
            }
            else if(temperature < 0.6)
            {
                // Temperate regions
                if(moisture < 0.3) return float3(0.8, 0.7, 0.5); // Savanna
                if(moisture < 0.6) return float3(0.4, 0.6, 0.3); // Forest
                return float3(0.2, 0.4, 0.2); // Rainforest
            }
            else
            {
                // Hot regions
                if(moisture < 0.2) return float3(0.9, 0.8, 0.5); // Desert
                if(moisture < 0.5) return float3(0.6, 0.7, 0.2); // Grassland
                return float3(0.1, 0.5, 0.1); // Jungle
            }
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample maps
            float elevation = tex2D(_ElevationMap, IN.uv_ElevationMap).r;
            float temperature = tex2D(_TemperatureMap, IN.uv_ElevationMap).r;
            float moisture = tex2D(_MoistureMap, IN.uv_ElevationMap).r;
            float noise = tex2D(_NoiseMap, IN.uv_ElevationMap * 10.0).r;
            
            // Get biome color
            float3 biomeColor = get_biome_color(elevation, temperature, moisture, noise);
            
            o.Albedo = biomeColor;
            o.Metallic = 0.0;
            o.Smoothness = 0.0;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
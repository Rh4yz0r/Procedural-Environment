Shader "Custom/DiffuseSplatMap"
{
    Properties
    {
        _Control ("Control (RGBA)", 2D) = "red" {}
        _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        _Splat0 ("Layer 0 (R)", 2D) = "white" {}
            // used in fallback on old cards & base map
        _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
    }
       
    SubShader
    {
            Tags
            {
                "Queue" = "Geometry-100"
                "RenderType" = "Opaque"
            }
        CGPROGRAM
        #pragma surface surf Lambert
        struct Input
        {
            float2 uv_Control : TEXCOORD0;
            float2 uv_Splat0 : TEXCOORD1;
        };
 
        sampler2D _Control;
        sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 splat_control = tex2D (_Control, IN.uv_Control);
            fixed3 col;
            col  = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb;
            col += splat_control.g * tex2D (_Splat1, IN.uv_Splat0).rgb;
            col += splat_control.b * tex2D (_Splat2, IN.uv_Splat0).rgb;
            col += splat_control.a * tex2D (_Splat3, IN.uv_Splat0).rgb;
            o.Albedo = col;
            o.Alpha = 0.0;
        }
        ENDCG  
    }
 
    // Fallback to Diffuse
    Fallback "Diffuse"
}
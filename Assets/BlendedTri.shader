Shader "Unlit/BlendedTri"
{
    Properties {
        _SplatTex ("Splat (RGBA)", 2D) = "red" {}
        
        _Splat0 ("Splat0 (R)", 2D) = "white" {}
        [NoScaleOffset] _Splat0_NormalMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _Splat0_OcclusionMap("Occlusion", 2D) = "white" {}
        
        _Splat1 ("Splat1 (G)", 2D) = "white" {}
        [NoScaleOffset] _Splat1_NormalMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _Splat1_OcclusionMap("Occlusion", 2D) = "white" {}
        
        _Splat2 ("Splat2 (B)", 2D) = "white" {}
        [NoScaleOffset] _Splat2_NormalMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _Splat2_OcclusionMap("Occlusion", 2D) = "white" {}
        
        _Splat3 ("Splat3 (A)", 2D) = "white" {}
        [NoScaleOffset] _Splat3_NormalMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _Splat3_OcclusionMap("Occlusion", 2D) = "white" {}
        
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        
        /*_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0*/
    }
    SubShader {
        Tags
            {
                "Queue" = "Geometry-100"
                "RenderType" = "Opaque"
            }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        #pragma surface surf Lambert
        
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "UnityStandardUtils.cginc"

        // flip UVs horizontally to correct for back side projection
        #define TRIPLANAR_CORRECT_PROJECTED_U

        // offset UVs to prevent obvious mirroring
        // #define TRIPLANAR_UV_OFFSET

        // Reoriented Normal Mapping
        // http://blog.selfshadow.com/publications/blending-in-detail/
        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }

        sampler2D _SplatTex;
        float4 _SplatTex_ST;

        sampler2D _Splat0;
        float4 _Splat0_ST;
        sampler2D _Splat0_NormalMap;
        sampler2D _Splat0_OcclusionMap;

        sampler2D _Splat1;
        float4 _Splat1_ST;
        sampler2D _Splat1_NormalMap;
        sampler2D _Splat1_OcclusionMap;

        sampler2D _Splat2;
        float4 _Splat2_ST;
        sampler2D _Splat2_NormalMap;
        sampler2D _Splat2_OcclusionMap;

        sampler2D _Splat3;
        float4 _Splat3_ST;
        sampler2D _Splat3_NormalMap;
        sampler2D _Splat3_OcclusionMap;
        
        /*sampler2D _MainTex;
        float4 _MainTex_ST;

        sampler2D _BumpMap;
        sampler2D _OcclusionMap;*/

        half _Glossiness;
        half _Metallic;
        
        half _OcclusionStrength;

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 triblend = saturate(pow(IN.worldNormal, 4));
            triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            float2 uvX = IN.worldPos.zy * _SplatTex_ST.xy + _SplatTex_ST.zy;
            float2 uvY = IN.worldPos.xz * _SplatTex_ST.xy + _SplatTex_ST.zy;
            float2 uvZ = IN.worldPos.xy * _SplatTex_ST.xy + _SplatTex_ST.zy;

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY += 0.33;
            uvZ += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            uvX.x *= axisSign.x;
            uvY.x *= axisSign.y;
            uvZ.x *= -axisSign.z;
        #endif


            fixed4 splat_control_x = tex2D (_SplatTex, uvX);
            fixed4 splat_control_y = tex2D (_SplatTex, uvY);
            fixed4 splat_control_z = tex2D (_SplatTex, uvZ);

            fixed3 colX;
            colX  = splat_control_x.r * tex2D (_Splat0, uvX).rgb;
            colX += splat_control_x.g * tex2D (_Splat1, uvX).rgb;
            colX += splat_control_x.b * tex2D (_Splat2, uvX).rgb;
            colX += splat_control_x.a * tex2D (_Splat3, uvX).rgb;

            fixed3 colY;
            colY  = splat_control_y.r * tex2D (_Splat0, uvY).rgb;
            colY += splat_control_y.g * tex2D (_Splat1, uvY).rgb;
            colY += splat_control_y.b * tex2D (_Splat2, uvY).rgb;
            colY += splat_control_y.a * tex2D (_Splat3, uvY).rgb;

            fixed3 colZ;
            colZ  = splat_control_z.r * tex2D (_Splat0, uvZ).rgb;
            colZ += splat_control_z.g * tex2D (_Splat1, uvZ).rgb;
            colZ += splat_control_z.b * tex2D (_Splat2, uvZ).rgb;
            colZ += splat_control_z.a * tex2D (_Splat3, uvZ).rgb;

            fixed3 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

            //------------------------------------------------------------------------------------

            half occX;
            occX  = splat_control_x.r * tex2D (_Splat0_OcclusionMap, uvX).g;
            occX += splat_control_x.g * tex2D (_Splat1_OcclusionMap, uvX).g;
            occX += splat_control_x.b * tex2D (_Splat2_OcclusionMap, uvX).g;
            occX += splat_control_x.a * tex2D (_Splat3_OcclusionMap, uvX).g;

            half occY;
            occY  = splat_control_y.r * tex2D (_Splat0_OcclusionMap, uvY).g;
            occY += splat_control_y.g * tex2D (_Splat1_OcclusionMap, uvY).g;
            occY += splat_control_y.b * tex2D (_Splat2_OcclusionMap, uvY).g;
            occY += splat_control_y.a * tex2D (_Splat3_OcclusionMap, uvY).g;

            half occZ;
            occZ  = splat_control_z.r * tex2D (_Splat0_OcclusionMap, uvZ).g;
            occZ += splat_control_z.g * tex2D (_Splat1_OcclusionMap, uvZ).g;
            occZ += splat_control_z.b * tex2D (_Splat2_OcclusionMap, uvZ).g;
            occZ += splat_control_z.a * tex2D (_Splat3_OcclusionMap, uvZ).g;

            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

            //---------------------------------------------------------------------------------------------------------------------

            half3 tnormalX;
            tnormalX  = splat_control_x.r * UnpackNormal (tex2D(_Splat0_NormalMap, uvX));
            tnormalX += splat_control_x.g * UnpackNormal (tex2D(_Splat1_NormalMap, uvX));
            tnormalX += splat_control_x.b * UnpackNormal (tex2D(_Splat2_NormalMap, uvX));
            tnormalX += splat_control_x.a * UnpackNormal (tex2D(_Splat3_NormalMap, uvX));

            half3 tnormalY;
            tnormalY  = splat_control_y.r * UnpackNormal (tex2D(_Splat0_NormalMap, uvY));
            tnormalY += splat_control_y.g * UnpackNormal (tex2D(_Splat1_NormalMap, uvY));
            tnormalY += splat_control_y.b * UnpackNormal (tex2D(_Splat2_NormalMap, uvY));
            tnormalY += splat_control_y.a * UnpackNormal (tex2D(_Splat3_NormalMap, uvY));

            half3 tnormalZ;
            tnormalZ  = splat_control_z.r * UnpackNormal (tex2D(_Splat0_NormalMap, uvZ));
            tnormalZ += splat_control_z.g * UnpackNormal (tex2D(_Splat1_NormalMap, uvZ));
            tnormalZ += splat_control_z.b * UnpackNormal (tex2D(_Splat2_NormalMap, uvZ));
            tnormalZ += splat_control_z.a * UnpackNormal (tex2D(_Splat3_NormalMap, uvZ));
            
            // albedo textures
            /*fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colY = tex2D(_MainTex, uvY);
            fixed4 colZ = tex2D(_MainTex, uvZ);
            fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;*/

            // occlusion textures
            /*half occX = tex2D(_OcclusionMap, uvX).g;
            half occY = tex2D(_OcclusionMap, uvY).g;
            half occZ = tex2D(_OcclusionMap, uvZ).g;
            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);*/

            // tangent space normal maps
            /*half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
            half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
            half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));*/

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            tnormalX.x *= axisSign.x;
            tnormalY.x *= axisSign.y;
            tnormalZ.x *= -axisSign.z;
        #endif

            half3 absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
            tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

            // apply world space sign to tangent space Z
            tnormalX.z *= axisSign.x;
            tnormalY.z *= axisSign.y;
            tnormalZ.z *= axisSign.z;

            // sizzle tangent normals to match world normal and blend together
            half3 worldNormal = normalize(
                tnormalX.zyx * triblend.x +
                tnormalY.xzy * triblend.y +
                tnormalZ.xyz * triblend.z
                );

            // set surface ouput properties
            o.Albedo = col.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Occlusion = occ;

            // convert world space normals into tangent normals
            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
        }
        ENDCG
    }
    
    // Fallback to Diffuse
    Fallback "Diffuse"
}

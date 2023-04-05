Shader "Unlit/TriplanarEnvironmentGround"
{
    Properties {
        /*_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        */

        
        _splat ("Albedo (RGB)", 2D) = "white" {}
        
        _splatR ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _splatR_BumpMap("Normal Map", 2D) = "bump" {}
        _splatR_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _splatR_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _splatR_OcclusionMap("Occlusion", 2D) = "white" {}
        _splatR_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        
        _splatG ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _splatG_BumpMap("Normal Map", 2D) = "bump" {}
        _splatG_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _splatG_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _splatG_OcclusionMap("Occlusion", 2D) = "white" {}
        _splatG_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        
        _splatB ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _splatB_BumpMap("Normal Map", 2D) = "bump" {}
        _splatB_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _splatB_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _splatB_OcclusionMap("Occlusion", 2D) = "white" {}
        _splatB_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        
        _splatA ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _splatA_BumpMap("Normal Map", 2D) = "bump" {}
        _splatA_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _splatA_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _splatA_OcclusionMap("Occlusion", 2D) = "white" {}
        _splatA_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

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

        /*
        sampler2D _MainTex;
        float4 _MainTex_ST;

        sampler2D _BumpMap;
        sampler2D _OcclusionMap;

        half _Glossiness;
        half _Metallic;
        
        half _OcclusionStrength;
        */


        sampler2D _Splat;
        float4 _Splat_ST;
        
        sampler2D _SplatR;
        float4 _SplatR_ST;
        sampler2D _SplatR_BumpMap;
        sampler2D _SplatR_OcclusionMap;
        half _SplatR_Glossiness;
        half _SplatR_Metallic;
        half _SplatR_OcclusionStrength;

        sampler2D _SplatG;
        float4 _SplatG_ST;
        sampler2D _SplatG_BumpMap;
        sampler2D _SplatG_OcclusionMap;
        half _SplatG_Glossiness;
        half _SplatG_Metallic;
        half _SplatG_OcclusionStrength;

        sampler2D _SplatB;
        float4 _SplatB_ST;
        sampler2D _SplatB_BumpMap;
        sampler2D _SplatB_OcclusionMap;
        half _SplatB_Glossiness;
        half _SplatB_Metallic;
        half _SplatB_OcclusionStrength;

        sampler2D _SplatA;
        float4 _SplatA_ST;
        sampler2D _SplatA_BumpMap;
        sampler2D _SplatA_OcclusionMap;
        half _SplatA_Glossiness;
        half _SplatA_Metallic;
        half _SplatA_OcclusionStrength;
        

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
            
            if(_Splat_ST.r > 0)
            {
                // work around bug where IN.worldNormal is always (0,0,0)!
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                // calculate triplanar blend
                half3 triblend = saturate(pow(IN.worldNormal, 4));
                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                // calculate triplanar uvs
                // applying texture scale and offset values ala TRANSFORM_TEX macro
                float2 uvX = IN.worldPos.zy * _SplatR_ST.xy + _SplatR_ST.zy;
                float2 uvY = IN.worldPos.xz * _SplatR_ST.xy + _SplatR_ST.zy;
                float2 uvZ = IN.worldPos.xy * _SplatR_ST.xy + _SplatR_ST.zy;

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

                // albedo textures
                fixed4 colX = tex2D(_SplatR, uvX);
                fixed4 colY = tex2D(_SplatR, uvY);
                fixed4 colZ = tex2D(_SplatR, uvZ);
                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                // occlusion textures
                half occX = tex2D(_SplatR_OcclusionMap, uvX).g;
                half occY = tex2D(_SplatR_OcclusionMap, uvY).g;
                half occZ = tex2D(_SplatR_OcclusionMap, uvZ).g;
                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _SplatR_OcclusionStrength);

                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(_SplatR_BumpMap, uvX));
                half3 tnormalY = UnpackNormal(tex2D(_SplatR_BumpMap, uvY));
                half3 tnormalZ = UnpackNormal(tex2D(_SplatR_BumpMap, uvZ));

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
                o.Metallic = _SplatR_Metallic;
                o.Smoothness = _SplatR_Glossiness;
                o.Occlusion = occ;

                // convert world space normals into tangent normals
                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            }
            else if(_Splat_ST.g  > 0)
            {
                // work around bug where IN.worldNormal is always (0,0,0)!
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                // calculate triplanar blend
                half3 triblend = saturate(pow(IN.worldNormal, 4));
                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                // calculate triplanar uvs
                // applying texture scale and offset values ala TRANSFORM_TEX macro
                float2 uvX = IN.worldPos.zy * _SplatG_ST.xy + _SplatG_ST.zy;
                float2 uvY = IN.worldPos.xz * _SplatG_ST.xy + _SplatG_ST.zy;
                float2 uvZ = IN.worldPos.xy * _SplatG_ST.xy + _SplatG_ST.zy;

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

                // albedo textures
                fixed4 colX = tex2D(_SplatG, uvX);
                fixed4 colY = tex2D(_SplatG, uvY);
                fixed4 colZ = tex2D(_SplatG, uvZ);
                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                // occlusion textures
                half occX = tex2D(_SplatG_OcclusionMap, uvX).g;
                half occY = tex2D(_SplatG_OcclusionMap, uvY).g;
                half occZ = tex2D(_SplatG_OcclusionMap, uvZ).g;
                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _SplatG_OcclusionStrength);

                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(_SplatG_BumpMap, uvX));
                half3 tnormalY = UnpackNormal(tex2D(_SplatG_BumpMap, uvY));
                half3 tnormalZ = UnpackNormal(tex2D(_SplatG_BumpMap, uvZ));

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
                o.Metallic = _SplatG_Metallic;
                o.Smoothness = _SplatG_Glossiness;
                o.Occlusion = occ;

                // convert world space normals into tangent normals
                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            }
            else if(_Splat_ST.b  > 0)
            {
                // work around bug where IN.worldNormal is always (0,0,0)!
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                // calculate triplanar blend
                half3 triblend = saturate(pow(IN.worldNormal, 4));
                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                // calculate triplanar uvs
                // applying texture scale and offset values ala TRANSFORM_TEX macro
                float2 uvX = IN.worldPos.zy * _SplatB_ST.xy + _SplatB_ST.zy;
                float2 uvY = IN.worldPos.xz * _SplatB_ST.xy + _SplatB_ST.zy;
                float2 uvZ = IN.worldPos.xy * _SplatB_ST.xy + _SplatB_ST.zy;

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

                // albedo textures
                fixed4 colX = tex2D(_SplatB, uvX);
                fixed4 colY = tex2D(_SplatB, uvY);
                fixed4 colZ = tex2D(_SplatB, uvZ);
                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                // occlusion textures
                half occX = tex2D(_SplatB_OcclusionMap, uvX).g;
                half occY = tex2D(_SplatB_OcclusionMap, uvY).g;
                half occZ = tex2D(_SplatB_OcclusionMap, uvZ).g;
                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _SplatB_OcclusionStrength);

                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(_SplatB_BumpMap, uvX));
                half3 tnormalY = UnpackNormal(tex2D(_SplatB_BumpMap, uvY));
                half3 tnormalZ = UnpackNormal(tex2D(_SplatB_BumpMap, uvZ));

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
                o.Metallic = _SplatB_Metallic;
                o.Smoothness = _SplatB_Glossiness;
                o.Occlusion = occ;

                // convert world space normals into tangent normals
                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            }
            else if(_Splat_ST.a  > 0)
            {
                // work around bug where IN.worldNormal is always (0,0,0)!
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                // calculate triplanar blend
                half3 triblend = saturate(pow(IN.worldNormal, 4));
                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                // calculate triplanar uvs
                // applying texture scale and offset values ala TRANSFORM_TEX macro
                float2 uvX = IN.worldPos.zy * _SplatA_ST.xy + _SplatA_ST.zy;
                float2 uvY = IN.worldPos.xz * _SplatA_ST.xy + _SplatA_ST.zy;
                float2 uvZ = IN.worldPos.xy * _SplatA_ST.xy + _SplatA_ST.zy;

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

                // albedo textures
                fixed4 colX = tex2D(_SplatA, uvX);
                fixed4 colY = tex2D(_SplatA, uvY);
                fixed4 colZ = tex2D(_SplatA, uvZ);
                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                // occlusion textures
                half occX = tex2D(_SplatA_OcclusionMap, uvX).g;
                half occY = tex2D(_SplatA_OcclusionMap, uvY).g;
                half occZ = tex2D(_SplatA_OcclusionMap, uvZ).g;
                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _SplatA_OcclusionStrength);

                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(_SplatA_BumpMap, uvX));
                half3 tnormalY = UnpackNormal(tex2D(_SplatA_BumpMap, uvY));
                half3 tnormalZ = UnpackNormal(tex2D(_SplatA_BumpMap, uvZ));

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
                o.Metallic = _SplatA_Metallic;
                o.Smoothness = _SplatA_Glossiness;
                o.Occlusion = occ;

                // convert world space normals into tangent normals
                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            }
            
            
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}

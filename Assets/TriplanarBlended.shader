Shader "Unlit/TriplanarBlended"
{
    Properties 
    {
                        _BlendMap ("BlendMap (RGBA)", 2D) = "red" {}
        //----------------------------------------------------------------
                        _Splat0_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _Splat0_BumpMap("Normal Map", 2D) = "bump" {}
                        _Splat0_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma]         _Splat0_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _Splat0_OcclusionMap("Occlusion", 2D) = "white" {}
                        _Splat0_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        //----------------------------------------------------------------
                        _Splat1_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _Splat1_BumpMap("Normal Map", 2D) = "bump" {}
                        _Splat1_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma]         _Splat1_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _Splat1_OcclusionMap("Occlusion", 2D) = "white" {}
                        _Splat1_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        //----------------------------------------------------------------
                        _Splat2_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _Splat2_BumpMap("Normal Map", 2D) = "bump" {}
                        _Splat2_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma]         _Splat2_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _Splat2_OcclusionMap("Occlusion", 2D) = "white" {}
                        _Splat2_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        //----------------------------------------------------------------
                        _Splat3_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _Splat3_BumpMap("Normal Map", 2D) = "bump" {}
                        _Splat3_Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma]         _Splat3_Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _Splat3_OcclusionMap("Occlusion", 2D) = "white" {}
                        _Splat3_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
    }
    SubShader 
    {
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
        
        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------
        sampler2D _BlendMap;
        float4    _BlendMap_ST;
        //------------------------------------------------------------------------
        sampler2D _Splat0_MainTex;
        float4    _Splat0_MainTex_ST;

        sampler2D _Splat0_BumpMap;
        sampler2D _Splat0_OcclusionMap;
        half      _Splat0_Glossiness;
        half      _Splat0_Metallic;
        half      _Splat0_OcclusionStrength;
        //------------------------------------------------------------------------
        sampler2D _Splat1_MainTex;
        float4    _Splat1_MainTex_ST;

        sampler2D _Splat1_BumpMap;
        sampler2D _Splat1_OcclusionMap;
        half      _Splat1_Glossiness;
        half      _Splat1_Metallic;
        half      _Splat1_OcclusionStrength;
        //------------------------------------------------------------------------
        sampler2D _Splat2_MainTex;
        float4    _Splat2_MainTex_ST;

        sampler2D _Splat2_BumpMap;
        sampler2D _Splat2_OcclusionMap;
        half      _Splat2_Glossiness;
        half      _Splat2_Metallic;
        half      _Splat2_OcclusionStrength;
        //------------------------------------------------------------------------
        sampler2D _Splat3_MainTex;
        float4    _Splat3_MainTex_ST;

        sampler2D _Splat3_BumpMap;
        sampler2D _Splat3_OcclusionMap;
        half      _Splat3_Glossiness;
        half      _Splat3_Metallic;
        half      _Splat3_OcclusionStrength;
        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------
        
        struct Input {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_Control : TEXCOORD0;
            float2 uv_Splat0 : TEXCOORD1;
            INTERNAL_DATA
        };

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //---------------------------------------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------SPLAT0----------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------
            
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 splat0_triblend = saturate(pow(IN.worldNormal, 4));
            splat0_triblend /= max(dot(splat0_triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            float2 splat0_uvX = IN.worldPos.zy * _Splat0_MainTex_ST.xy + _Splat0_MainTex_ST.zy;
            float2 splat0_uvY = IN.worldPos.xz * _Splat0_MainTex_ST.xy + _Splat0_MainTex_ST.zy;
            float2 splat0_uvZ = IN.worldPos.xy * _Splat0_MainTex_ST.xy + _Splat0_MainTex_ST.zy;

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY += 0.33;
            uvZ += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 splat0_axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat0_uvX.x *= splat0_axisSign.x;
            splat0_uvY.x *= splat0_axisSign.y;
            splat0_uvZ.x *= -splat0_axisSign.z;
        #endif

            // albedo textures
            fixed4 splat0_colX = tex2D(_Splat0_MainTex, splat0_uvX);
            fixed4 splat0_colY = tex2D(_Splat0_MainTex, splat0_uvY);
            fixed4 splat0_colZ = tex2D(_Splat0_MainTex, splat0_uvZ);
            fixed4 splat0_col = splat0_colX * splat0_triblend.x + splat0_colY * splat0_triblend.y + splat0_colZ * splat0_triblend.z;

            // occlusion textures
            half splat0_occX = tex2D(_Splat0_OcclusionMap, splat0_uvX).g;
            half splat0_occY = tex2D(_Splat0_OcclusionMap, splat0_uvY).g;
            half splat0_occZ = tex2D(_Splat0_OcclusionMap, splat0_uvZ).g;
            half splat0_occ = LerpOneTo(splat0_occX * splat0_triblend.x + splat0_occY * splat0_triblend.y + splat0_occZ * splat0_triblend.z, _Splat0_OcclusionStrength);

            // tangent space normal maps
            half3 splat0_tnormalX = UnpackNormal(tex2D(_Splat0_BumpMap, splat0_uvX));
            half3 splat0_tnormalY = UnpackNormal(tex2D(_Splat0_BumpMap, splat0_uvY));
            half3 splat0_tnormalZ = UnpackNormal(tex2D(_Splat0_BumpMap, splat0_uvZ));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat0_tnormalX.x *= splat0_axisSign.x;
            splat0_tnormalY.x *= splat0_axisSign.y;
            splat0_tnormalZ.x *= -splat0_axisSign.z;
        #endif

            half3 splat0_absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            splat0_tnormalX = blend_rnm(half3(IN.worldNormal.zy, splat0_absVertNormal.x), splat0_tnormalX);
            splat0_tnormalY = blend_rnm(half3(IN.worldNormal.xz, splat0_absVertNormal.y), splat0_tnormalY);
            splat0_tnormalZ = blend_rnm(half3(IN.worldNormal.xy, splat0_absVertNormal.z), splat0_tnormalZ);

            // apply world space sign to tangent space Z
            splat0_tnormalX.z *= splat0_axisSign.x;
            splat0_tnormalY.z *= splat0_axisSign.y;
            splat0_tnormalZ.z *= splat0_axisSign.z;
            
            // sizzle tangent normals to match world normal and blend together
            half3 splat0_worldNormal = normalize(
                splat0_tnormalX.zyx * splat0_triblend.x +
                splat0_tnormalY.xzy * splat0_triblend.y +
                splat0_tnormalZ.xyz * splat0_triblend.z
                );

            //---------------------------------------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------SPLAT0----------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------
            
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 splat1_triblend = saturate(pow(IN.worldNormal, 4));
            splat1_triblend /= max(dot(splat1_triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            float2 splat1_uvX = IN.worldPos.zy * _Splat1_MainTex_ST.xy + _Splat1_MainTex_ST.zy;
            float2 splat1_uvY = IN.worldPos.xz * _Splat1_MainTex_ST.xy + _Splat1_MainTex_ST.zy;
            float2 splat1_uvZ = IN.worldPos.xy * _Splat1_MainTex_ST.xy + _Splat1_MainTex_ST.zy;

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY += 0.33;
            uvZ += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 splat1_axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat1_uvX.x *= splat1_axisSign.x;
            splat1_uvY.x *= splat1_axisSign.y;
            splat1_uvZ.x *= -splat1_axisSign.z;
        #endif

            // albedo textures
            fixed4 splat1_colX = tex2D(_Splat1_MainTex, splat1_uvX);
            fixed4 splat1_colY = tex2D(_Splat1_MainTex, splat1_uvY);
            fixed4 splat1_colZ = tex2D(_Splat1_MainTex, splat1_uvZ);
            fixed4 splat1_col = splat1_colX * splat1_triblend.x + splat1_colY * splat1_triblend.y + splat1_colZ * splat1_triblend.z;

            // occlusion textures
            half splat1_occX = tex2D(_Splat1_OcclusionMap, splat1_uvX).g;
            half splat1_occY = tex2D(_Splat1_OcclusionMap, splat1_uvY).g;
            half splat1_occZ = tex2D(_Splat1_OcclusionMap, splat1_uvZ).g;
            half splat1_occ = LerpOneTo(splat1_occX * splat1_triblend.x + splat1_occY * splat1_triblend.y + splat1_occZ * splat1_triblend.z, _Splat1_OcclusionStrength);

            // tangent space normal maps
            half3 splat1_tnormalX = UnpackNormal(tex2D(_Splat1_BumpMap, splat1_uvX));
            half3 splat1_tnormalY = UnpackNormal(tex2D(_Splat1_BumpMap, splat1_uvY));
            half3 splat1_tnormalZ = UnpackNormal(tex2D(_Splat1_BumpMap, splat1_uvZ));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat1_tnormalX.x *= splat1_axisSign.x;
            splat1_tnormalY.x *= splat1_axisSign.y;
            splat1_tnormalZ.x *= -splat1_axisSign.z;
        #endif

            half3 splat1_absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            splat1_tnormalX = blend_rnm(half3(IN.worldNormal.zy, splat1_absVertNormal.x), splat1_tnormalX);
            splat1_tnormalY = blend_rnm(half3(IN.worldNormal.xz, splat1_absVertNormal.y), splat1_tnormalY);
            splat1_tnormalZ = blend_rnm(half3(IN.worldNormal.xy, splat1_absVertNormal.z), splat1_tnormalZ);

            // apply world space sign to tangent space Z
            splat1_tnormalX.z *= splat1_axisSign.x;
            splat1_tnormalY.z *= splat1_axisSign.y;
            splat1_tnormalZ.z *= splat1_axisSign.z;
            
            // sizzle tangent normals to match world normal and blend together
            half3 splat1_worldNormal = normalize(
                splat1_tnormalX.zyx * splat1_triblend.x +
                splat1_tnormalY.xzy * splat1_triblend.y +
                splat1_tnormalZ.xyz * splat1_triblend.z
                );

            //---------------------------------------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------SPLAT0----------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------
            
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 splat2_triblend = saturate(pow(IN.worldNormal, 4));
            splat2_triblend /= max(dot(splat2_triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            float2 splat2_uvX = IN.worldPos.zy * _Splat2_MainTex_ST.xy + _Splat2_MainTex_ST.zy;
            float2 splat2_uvY = IN.worldPos.xz * _Splat2_MainTex_ST.xy + _Splat2_MainTex_ST.zy;
            float2 splat2_uvZ = IN.worldPos.xy * _Splat2_MainTex_ST.xy + _Splat2_MainTex_ST.zy;

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY += 0.33;
            uvZ += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 splat2_axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat2_uvX.x *= splat2_axisSign.x;
            splat2_uvY.x *= splat2_axisSign.y;
            splat2_uvZ.x *= -splat2_axisSign.z;
        #endif

            // albedo textures
            fixed4 splat2_colX = tex2D(_Splat2_MainTex, splat2_uvX);
            fixed4 splat2_colY = tex2D(_Splat2_MainTex, splat2_uvY);
            fixed4 splat2_colZ = tex2D(_Splat2_MainTex, splat2_uvZ);
            fixed4 splat2_col = splat2_colX * splat2_triblend.x + splat2_colY * splat2_triblend.y + splat2_colZ * splat2_triblend.z;

            // occlusion textures
            half splat2_occX = tex2D(_Splat2_OcclusionMap, splat2_uvX).g;
            half splat2_occY = tex2D(_Splat2_OcclusionMap, splat2_uvY).g;
            half splat2_occZ = tex2D(_Splat2_OcclusionMap, splat2_uvZ).g;
            half splat2_occ = LerpOneTo(splat2_occX * splat2_triblend.x + splat2_occY * splat2_triblend.y + splat2_occZ * splat2_triblend.z, _Splat2_OcclusionStrength);

            // tangent space normal maps
            half3 splat2_tnormalX = UnpackNormal(tex2D(_Splat2_BumpMap, splat2_uvX));
            half3 splat2_tnormalY = UnpackNormal(tex2D(_Splat2_BumpMap, splat2_uvY));
            half3 splat2_tnormalZ = UnpackNormal(tex2D(_Splat2_BumpMap, splat2_uvZ));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat2_tnormalX.x *= splat2_axisSign.x;
            splat2_tnormalY.x *= splat2_axisSign.y;
            splat2_tnormalZ.x *= -splat2_axisSign.z;
        #endif

            half3 splat2_absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            splat2_tnormalX = blend_rnm(half3(IN.worldNormal.zy, splat2_absVertNormal.x), splat2_tnormalX);
            splat2_tnormalY = blend_rnm(half3(IN.worldNormal.xz, splat2_absVertNormal.y), splat2_tnormalY);
            splat2_tnormalZ = blend_rnm(half3(IN.worldNormal.xy, splat2_absVertNormal.z), splat2_tnormalZ);

            // apply world space sign to tangent space Z
            splat2_tnormalX.z *= splat2_axisSign.x;
            splat2_tnormalY.z *= splat2_axisSign.y;
            splat2_tnormalZ.z *= splat2_axisSign.z;
            
            // sizzle tangent normals to match world normal and blend together
            half3 splat2_worldNormal = normalize(
                splat2_tnormalX.zyx * splat2_triblend.x +
                splat2_tnormalY.xzy * splat2_triblend.y +
                splat2_tnormalZ.xyz * splat2_triblend.z
                );

            //---------------------------------------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------SPLAT0----------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------
            
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 splat3_triblend = saturate(pow(IN.worldNormal, 4));
            splat3_triblend /= max(dot(splat3_triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            float2 splat3_uvX = IN.worldPos.zy * _Splat3_MainTex_ST.xy + _Splat3_MainTex_ST.zy;
            float2 splat3_uvY = IN.worldPos.xz * _Splat3_MainTex_ST.xy + _Splat3_MainTex_ST.zy;
            float2 splat3_uvZ = IN.worldPos.xy * _Splat3_MainTex_ST.xy + _Splat3_MainTex_ST.zy;

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY += 0.33;
            uvZ += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 splat3_axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat3_uvX.x *= splat3_axisSign.x;
            splat3_uvY.x *= splat3_axisSign.y;
            splat3_uvZ.x *= -splat3_axisSign.z;
        #endif

            // albedo textures
            fixed4 splat3_colX = tex2D(_Splat3_MainTex, splat3_uvX);
            fixed4 splat3_colY = tex2D(_Splat3_MainTex, splat3_uvY);
            fixed4 splat3_colZ = tex2D(_Splat3_MainTex, splat3_uvZ);
            fixed4 splat3_col = splat3_colX * splat3_triblend.x + splat3_colY * splat3_triblend.y + splat3_colZ * splat3_triblend.z;

            // occlusion textures
            half splat3_occX = tex2D(_Splat3_OcclusionMap, splat3_uvX).g;
            half splat3_occY = tex2D(_Splat3_OcclusionMap, splat3_uvY).g;
            half splat3_occZ = tex2D(_Splat3_OcclusionMap, splat3_uvZ).g;
            half splat3_occ = LerpOneTo(splat3_occX * splat3_triblend.x + splat3_occY * splat3_triblend.y + splat3_occZ * splat3_triblend.z, _Splat3_OcclusionStrength);

            // tangent space normal maps
            half3 splat3_tnormalX = UnpackNormal(tex2D(_Splat3_BumpMap, splat3_uvX));
            half3 splat3_tnormalY = UnpackNormal(tex2D(_Splat3_BumpMap, splat3_uvY));
            half3 splat3_tnormalZ = UnpackNormal(tex2D(_Splat3_BumpMap, splat3_uvZ));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            splat3_tnormalX.x *= splat3_axisSign.x;
            splat3_tnormalY.x *= splat3_axisSign.y;
            splat3_tnormalZ.x *= -splat3_axisSign.z;
        #endif

            half3 splat3_absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            splat3_tnormalX = blend_rnm(half3(IN.worldNormal.zy, splat3_absVertNormal.x), splat3_tnormalX);
            splat3_tnormalY = blend_rnm(half3(IN.worldNormal.xz, splat3_absVertNormal.y), splat3_tnormalY);
            splat3_tnormalZ = blend_rnm(half3(IN.worldNormal.xy, splat3_absVertNormal.z), splat3_tnormalZ);

            // apply world space sign to tangent space Z
            splat3_tnormalX.z *= splat3_axisSign.x;
            splat3_tnormalY.z *= splat3_axisSign.y;
            splat3_tnormalZ.z *= splat3_axisSign.z;
            
            // sizzle tangent normals to match world normal and blend together
            half3 splat3_worldNormal = normalize(
                splat3_tnormalX.zyx * splat3_triblend.x +
                splat3_tnormalY.xzy * splat3_triblend.y +
                splat3_tnormalZ.xyz * splat3_triblend.z
                );

            //---------------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------

            float3 worldScale = float3(
                length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
                );

            float3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;
            
            float2 pos = IN.worldPos.xz - baseWorldPos.xz;
            pos.xy += 5 * worldScale.xz;
            
            fixed4 splat_control = tex2D (_BlendMap, pos/ 10 / worldScale.xz);

            fixed3 color;
            color  = splat_control.r * splat0_col;
            color += splat_control.g * splat1_col;
            color += splat_control.b * splat2_col;
            color += splat_control.a * splat3_col;

            half metallic;
            metallic  = splat_control.r * _Splat0_Metallic;
            metallic += splat_control.g * _Splat1_Metallic;
            metallic += splat_control.b * _Splat2_Metallic;
            metallic += splat_control.a * _Splat3_Metallic;

            half smoothness;
            smoothness  = splat_control.r * _Splat0_Glossiness;
            smoothness += splat_control.g * _Splat1_Glossiness;
            smoothness += splat_control.b * _Splat2_Glossiness;
            smoothness += splat_control.a * _Splat3_Glossiness;

            half occlusion;
            occlusion  = splat_control.r * splat0_occ;
            occlusion += splat_control.g * splat1_occ;
            occlusion += splat_control.b * splat2_occ;
            occlusion += splat_control.a * splat3_occ;

            float3 normal;
            normal  = splat_control.r * WorldToTangentNormalVector(IN, splat0_worldNormal);
            normal += splat_control.g * WorldToTangentNormalVector(IN, splat1_worldNormal);
            normal += splat_control.b * WorldToTangentNormalVector(IN, splat2_worldNormal);
            normal += splat_control.a * WorldToTangentNormalVector(IN, splat3_worldNormal);
            normal = normalize(normal);

            o.Albedo = color.rgb;
            o.Metallic = metallic;
            o.Smoothness = smoothness;
            o.Occlusion = occlusion;
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

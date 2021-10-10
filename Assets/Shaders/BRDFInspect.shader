Shader "PCTK/BRDFInspec"
{
    Properties
    {
        [MainTexture] _Albedo ("Albedo", 2D) = "black" {}
        _Mask ("Mask", 2D) = "black" {}
        _NormalMap("NormalMap", 2D) = "blue" {}
        _RenderMode("RenderMod", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 objectNormal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            int4 DecodeRenderMod(uint data)
            {
                int albedo   = (data) & 0x01;
                int r_m_h    = (data >> 1) & 0x01;
                int normal   = (data >> 2) & 0x01;
                int detailNormal = (data >> 3) & 0x01;
                return int4(albedo, r_m_h, normal, detailNormal);
            }

            sampler2D _Albedo;
            float4 _Albedo_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Albedo);
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.objectNormal));
                return o;
            }

            sampler2D _Mask;
            sampler2D _NormalMap;
            int _RenderMode;

            float4 frag(v2f i) : SV_Target
            {
                int4 mod = DecodeRenderMod(_RenderMode);
                // sample the texture
                float3 albedo = tex2D(_Albedo, i.uv);
                float4 m_o_d_r = tex2D(_Mask, i.uv);
                //Output roughness metallic
                float4 mask = float4(1 - m_o_d_r.a, m_o_d_r.r, 0, 1);
                float4 mapNormal = tex2D(_NormalMap, i.uv);
                mapNormal = float4(UnpackNormal(mapNormal), 0);
                mapNormal = (mapNormal + 1) / 2;
                float4 normal = float4((i.normal + 1) / 2, 1);
                float4 col = float4(albedo * mod.x, 1) + mask * mod.y + normal * mod.z + mapNormal * mod.w;
#if !UNITY_COLORSPACE_GAMMA
                col.rgb = GammaToLinearSpace(col.rgb);
#endif
                return col;
            }
            ENDCG
        }
    }
}

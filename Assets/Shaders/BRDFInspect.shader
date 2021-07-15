Shader "PCTK/BRDFInspec"
{
    Properties
    {
        [MainTexture] _Albedo ("Albedo", 2D) = "black" {}
        _Mask ("Mask", 2D) = "black" {}
        _Normal("Normal", 2D) = "blue" {}
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            int3 DecodeRenderMod(uint data)
            {
                int albedo   = (data) & 0x01;
                int r_m_d    = (data >> 1) & 0x01;
                int normal   = (data >> 2) & 0x01;
                return int3(albedo, r_m_d, normal);
            }

            sampler2D _Albedo;
            float4 _Albedo_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Albedo);
                return o;
            }

            sampler2D _Mask;
            sampler2D _Normal;
            int _RenderMode;

            float4 frag(v2f i) : SV_Target
            {
                int3 mod = DecodeRenderMod(_RenderMode);
                // sample the texture
                float4 albedo = tex2D(_Albedo, i.uv);
                float4 m_o_d_r = tex2D(_Mask, i.uv);
                //Output roughness metallic stencil
                float4 mask = float4(1 - m_o_d_r.a, m_o_d_r.r, 1, 0);
                float4 normal = tex2D(_Normal, i.uv);
                normal = float4(UnpackNormal(normal), 0);
                normal = (normal + 1) / 2;
                float4 col = albedo * mod.x + mask * mod.y + normal * mod.z;
                return col;
            }
            ENDCG
        }
    }
}

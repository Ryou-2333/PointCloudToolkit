Shader "PCTK/DepthInspect"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 EncodeDepth(float depth)
            {
                uint factor1 = 64;
                uint factor2 = factor1 * factor1;
                uint factor3 = factor2 * factor1;
                uint factor4 = factor3 * factor1;
                float depth0 = (float)floor(depth * factor1) / factor1;
                float depth1 = (float)(floor(depth * factor2) - depth0 * factor2) / factor1;
                float depth2 = (float)(floor(depth * factor3) - depth0 * factor3 - depth1 * factor2) / factor1;
                float depth3 = (float)(floor(depth * factor4) - depth0 * factor4 - depth1 * factor3 - depth2 * factor2) / factor1;
                return float4(depth0, depth1, depth2, depth3);
            }

            half4 frag(v2f i) : SV_Target
            {
                float customDepth = i.vertex.z;
                float4 encoded = EncodeDepth(customDepth);
#if !UNITY_COLORSPACE_GAMMA
                encoded.rgb = GammaToLinearSpace(encoded.rgb);
#endif
                float depth = float4(customDepth * 8, customDepth * 8, customDepth * 8, 1);
                return depth * depth * depth;
            }
            ENDCG
        }
    }
}

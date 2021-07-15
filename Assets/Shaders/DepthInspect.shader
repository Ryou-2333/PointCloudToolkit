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

            half4 EncodeDepth(float depth)
            {
                float factor1 = 128;
                float factor2 = factor1 * factor1;
                float factor3 = factor2 * factor1;
                float factor4 = factor3 * factor1;
                float depth0 = floor(depth * factor1) / factor1;
                float depth1 = (floor(depth * factor2) % factor1) / factor1;
                float depth2 = (floor(depth * factor3) % factor1) / factor1;
                float depth3 = (floor(depth * factor4) % factor1) / factor1;
                return half4(depth0, depth1, depth2, depth3);
            }

            half4 frag(v2f i) : SV_Target
            {
                float customDepth = i.vertex.z;
                half4 encoded = EncodeDepth(customDepth);
                return encoded;
            }
            ENDCG
        }
    }
}

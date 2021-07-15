#include "UnityCG.cginc"

// Uniforms
half _PointSize;
float4x4 _Transform;
int _RenderMod;

StructuredBuffer<float4> _PosRCBuffer;
StructuredBuffer<float4> _HMRABuffer;

#define MAX_BRIGHTNESS 16

uint EncodeColor(half3 rgb)
{
    half y = max(max(rgb.r, rgb.g), rgb.b);
    y = clamp(ceil(y * 255 / MAX_BRIGHTNESS), 1, 255);
    rgb *= 255 * 255 / (y * MAX_BRIGHTNESS);
    uint4 i = half4(rgb, y);
    return i.x | (i.y << 8) | (i.z << 16) | (i.w << 24);
}

half3 DecodeColor(uint packed)
{
    uint brightness = (packed >> 24) & 0xff;
    half k = MAX_BRIGHTNESS * brightness / 255.0 / 255.0;
    half r = ((packed) & 0xff) * k;
    half g = ((packed >> 8) & 0xff) * k;
    half b = ((packed >> 16) & 0xff) * k;
    return half3(r, g, b);
}

int DecodeRenderModRaw(uint data)
{
    return (data) & 0x01;
}

int4 DecodeRenderModParam(uint data)
{
    int h = (data >> 1) & 0x01;
    int m = (data >> 2) & 0x01;
    int r = (data >> 3) & 0x01;
    int a = (data >> 4) & 0x01;
    return int4(h, m, r, a);
}

// Vertex input attributes
struct Attributes
{
    uint vertexID : SV_VertexID;
};

// Fragment varyings
struct Varyings
{
    float4 position : SV_POSITION;
    half3 rawColor : COLOR;
    half3 h_m_r : TEXCOORD0;
    half3 albedo : TEXCOORD1;
    UNITY_FOG_COORDS(0)
};

// Vertex phase
Varyings Vertex(Attributes input)
{
    // Retrieve vertex attributes.
    float4 pos_rc = _PosRCBuffer[input.vertexID];
    float4 pos = mul(_Transform, float4(pos_rc.xyz, 1));
    half3 rawCol = DecodeColor(asuint(pos_rc.w));
    float4 h_m_r_a = _HMRABuffer[input.vertexID];
    half3 h_m_r = h_m_r_a.xyz;
    half3 albedo = DecodeColor(asuint(h_m_r_a.w));
    // Color space convertion & applying tint
#if !UNITY_COLORSPACE_GAMMA
    rawCol = GammaToLinearSpace(rawCol);
    albedo = GammaToLinearSpace(albedo);
#endif
    // Set vertex output.
    Varyings o;
    o.position = UnityObjectToClipPos(pos);
    o.rawColor = rawCol;
    o.h_m_r = h_m_r;
    o.albedo = albedo;

    UNITY_TRANSFER_FOG(o, o.position);
    return o;
}

// Geometry phase
[maxvertexcount(36)]
void Geometry(point Varyings input[1], inout TriangleStream<Varyings> outStream)
{
    float4 origin = input[0].position;
    float2 extent = abs(UNITY_MATRIX_P._11_22 * _PointSize);

    // Copy the basic information.
    Varyings o = input[0];

    // Determine the number of slices based on the radius of the
    // point on the screen.
    float radius = extent.y / origin.w * _ScreenParams.y;
    uint slices = min((radius + 1) / 5, 4) + 2;

    // Slightly enlarge quad points to compensate area reduction.
    // Hopefully this line would be complied without branch.
    if (slices == 2) extent *= 1.2;

    // Top vertex
    o.position.y = origin.y + extent.y;
    o.position.xzw = origin.xzw;
    outStream.Append(o);

    UNITY_LOOP for (uint i = 1; i < slices; i++)
    {
        float sn, cs;
        sincos(UNITY_PI / slices * i, sn, cs);

        // Right side vertex
        o.position.xy = origin.xy + extent * float2(sn, cs);
        outStream.Append(o);

        // Left side vertex
        o.position.x = origin.x - extent.x * sn;
        outStream.Append(o);
    }

    // Bottom vertex
    o.position.x = origin.x;
    o.position.y = origin.y - extent.y;
    outStream.Append(o);

    outStream.RestartStrip();
}

half4 Fragment(Varyings input) : SV_Target
{
    half3 rc = input.rawColor * DecodeRenderModRaw(asuint(_RenderMod));
    half3 h_m_r = input.h_m_r * DecodeRenderModParam(asuint(_RenderMod)).xyz;
    half3 albedo = input.albedo * DecodeRenderModParam(asuint(_RenderMod)).w;
    half3 white = half3(1, 1, 1);
    half3 c = rc + h_m_r.x * white + h_m_r.y * white + h_m_r.z * white + albedo;

    half4 color = half4(c, 1);
    UNITY_APPLY_FOG(input.fogCoord, color);
    return color;
}


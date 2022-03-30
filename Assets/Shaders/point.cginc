#include "UnityCG.cginc"

// Uniforms
half _PointSize;
float4x4 _Transform;
int _RenderMod;

StructuredBuffer<float4> _PosRCBuffer;
StructuredBuffer<float4> _MRDABuffer;
StructuredBuffer<float4> _NormalBuffer;

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

int DecodeRenderModNormal(uint data)
{
    return (data >> 5) & 0x01;
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
    half2 m_r : TEXCOORD0;
    half3 albedo : TEXCOORD1;
    half3 normal : TEXCOORD2;
    half3 detailedNormal : TEXCOORD3;
    float4 worldPos : TEXCOORD4;
    UNITY_FOG_COORDS(0)
};

// Vertex phase
Varyings Vertex(Attributes input)
{
    // Retrieve vertex attributes.
    float4 pos_rc = _PosRCBuffer[input.vertexID];
    float4 pos = mul(_Transform, float4(pos_rc.xyz, 1));
    half3 rawCol = DecodeColor(asuint(pos_rc.w));
    half4 m_r_d_a = _MRDABuffer[input.vertexID];
    half2 m_r = m_r_d_a.xy;
    half3 detailedNormal = DecodeColor(asuint(m_r_d_a.z));
    half3 albedo = DecodeColor(asuint(m_r_d_a.w));
    half3 normal = _NormalBuffer[input.vertexID].xyz;
    // Set vertex output.
    Varyings o;
    o.position = UnityObjectToClipPos(pos);
    o.rawColor = rawCol;
    o.m_r = m_r;
    o.albedo = albedo;
    o.normal = normal;
    o.detailedNormal = detailedNormal;
    o.worldPos = pos_rc;
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

float mod(float x, float m) {
    return (x % m + m) % m;
}

half GetSteped(float x, float step)
{
    half s = mod(x, step * 5);
    half ss = mod(s, step);
    s = s - ss;
    return s * 0.2 / step;
}

half4 Fragment(Varyings input) : SV_Target
{
    half3 rc = input.rawColor * DecodeRenderModRaw(asuint(_RenderMod));
    half2 m_r = input.m_r * DecodeRenderModParam(asuint(_RenderMod)).yz;
    half3 albedo = input.albedo * DecodeRenderModParam(asuint(_RenderMod)).w;
    half3 detailedNormal = input.detailedNormal * DecodeRenderModParam(asuint(_RenderMod)).x;
    half3 normal = input.normal * DecodeRenderModNormal(asuint(_RenderMod));
    float4 direction = normalize(input.worldPos);
    //direction = (direction + 1) / 2;
    float kernaleSize = 0.2;
    float angle_H = atan2(direction.y, direction.x);
    float angle_P = asin(direction.z);
    float3 W0 = float3(-direction.y, direction.x, 0);
    float3 U0 = cross(W0, direction);
    float3 U = float3(0, 1, 0);
    float angle_B = atan2(dot(W0, U) / abs(W0), dot(U0, U) / abs(U0));
    half3 directionColor = half3(GetSteped(angle_H, kernaleSize), GetSteped(angle_P, kernaleSize), GetSteped(angle_B, kernaleSize));
    directionColor = directionColor * DecodeRenderModParam(asuint(_RenderMod)).x;

    half3 white = half3(1, 1, 1);
    half3 c = rc + /*detailedNormal*/directionColor + m_r.x * white + m_r.y * white + albedo + normal;

    half4 color = half4(c, 1);
    UNITY_APPLY_FOG(input.fogCoord, color);
    // Color space convertion & applying tint
#if !UNITY_COLORSPACE_GAMMA
    color.rgb = GammaToLinearSpace(color.rgb);
#endif
    return color;
}


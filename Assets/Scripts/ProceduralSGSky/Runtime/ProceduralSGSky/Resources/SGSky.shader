// ==================================================================================================
//  This shader is a copy of sky-procedural available in legacy Unity
//  It's been ported to HDRP in order to have a basic procedural sky
//  It has been left mostly untouched but has been adapted to run per-pixel instead of per vertex
// ==================================================================================================
Shader "Hidden/HDRP/Sky/SGSky"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma editor_sync_compilation
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #pragma multi_compile _ _ENABLE_SUN_DISK

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Sky/SkyUtils.hlsl"

    uniform int _SGLength;
    uniform float3 _DirArray[128]; //Dir vector x, y, z
    uniform float4 _FeatureArray[128]; //Amplitude, Sharpness

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID, UNITY_RAW_FAR_CLIP_VALUE);
        return output;
    }

    struct SG
    {
        float3 Axis;
        float3 TintColor;
        float Sharpness;
    };

    float lum(float3 color)
    {
        return dot(color, float3(0.21, 0.71, 0.07));
    }

    float3 GetSkyColor(float3 dir)
    {
        float3 c = float3(0, 0, 0);

        for (int i = 0; i < _SGLength; i++)
        {
            SG sg;
            sg.Axis = _DirArray[i];
            sg.TintColor = _FeatureArray[i].rgb;
            sg.Sharpness = _FeatureArray[i].a;
            float lumi = lum(sg.TintColor);
            //Evaluate SG
            float cosAngle = dot(dir, sg.Axis);
            c += (sg.TintColor * exp(sg.Sharpness * (cosAngle - 1.0f)));
        }

        return c;
    }

    float3 ApplyExposure(float3 c, float exposure)
    {
        return ClampToFloat16Max(c * exposure);
    }

    float4 RenderSky(Varyings input, float exposure)
    {
        float3 viewDirWS = GetSkyViewDirWS(input.positionCS.xy);

        // Reverse it to point into the scene
        float3 dir = normalize(-viewDirWS);
        float3 c = ApplyExposure(GetSkyColor(dir), exposure);
        return float4(c, 1.0);
    }


    float4 FragBaking(Varyings input) : SV_Target
    {
        return RenderSky(input, 1.0);
    }

    float4 FragRender(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float4 color = RenderSky(input, 1.0);
        return color;
    }
    ENDHLSL

    SubShader
    {
        // For cubemap
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FragBaking
            ENDHLSL
        }

        // For fullscreen Sky
        Pass
        {
            ZWrite Off
            ZTest LEqual
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FragRender
            ENDHLSL
        }
    }
    Fallback Off
}

Shader "Hidden/Shader/CustomFog"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;

        UNITY_VERTEX_INPUT_INSTANCE_ID
        
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;

        float2 texcoord   : TEXCOORD0;

        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);

        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

        return output;

    }

    float _Intensity;
    float _StartFallOff;
    float _EndFallOff;
    TEXTURE2D_X(_InputTexture);

    float4 Frag(Varyings input) : SV_Target
    { 
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;

        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;

        float depth = LoadCameraDepth(input.positionCS.xy);
        float rise = (1 / (_EndFallOff - _StartFallOff));

        float offset = 1 - rise * _EndFallOff;

        float opacity = 1-  clamp(rise * depth + offset,0,1);

        float final = 1-depth;

        return float4(final,final,final, 1);
        return float4(lerp(outColor, float3(0,0,0), depth), 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "CustomFog"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment Frag
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}

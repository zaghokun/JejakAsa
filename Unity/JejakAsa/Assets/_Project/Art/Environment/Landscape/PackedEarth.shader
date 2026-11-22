Shader "JejakAsa/Packed Earth"
{
    Properties
    {
        [MainTexture] _BaseMap("Soil texture", 2D) = "white" {}
        [MainColor] _BaseColor("Earth tint", Color) = (.72,.61,.43,1)
        _DetailAlbedoMapScale("Texture strength", Range(0,1)) = .24
        _Smoothness("Smoothness", Range(0,1)) = .04
        _Metallic("Metallic", Range(0,1)) = 0
        [Normal] _BumpMap("Surface normal", 2D) = "bump" {}
        _BumpScale("Normal strength", Range(0,1)) = .35
        [HideInInspector] _SpecColor("Specular", Color) = (.2,.2,.2,1)
        [HideInInspector] _Cutoff("Cutoff", Float) = .5
        [HideInInspector] _Cull("Cull", Float) = 2
        [HideInInspector] _EmissionColor("Emission", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "UniversalMaterialType"="Lit" }
        Pass
        {
            Name "EarthForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            void InitializeEarthSurfaceData(float2 uv, out SurfaceData surfaceData)
            {
                InitializeStandardLitSurfaceData(uv, surfaceData);
                // Keep source texture detail subtle beside the low-poly characters.
                surfaceData.albedo = lerp(_BaseColor.rgb, surfaceData.albedo, _DetailAlbedoMapScale);
            }
            #define InitializeStandardLitSurfaceData InitializeEarthSurfaceData
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack "Universal Render Pipeline/Lit"
}

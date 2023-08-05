Shader "Hidden/Renderers/FinPassA"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _ColorMap("ColorMap", 2D) = "white" {}

        // Transparency
     //   _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector]_StencilRef("_StencilRef", Float) = 0
     [HideInInspector]_StencilWriteMask("_StencilWriteMask", Float) = 0
      [HideInInspector]_StencilReadMask("_StencilReadMask", Float) = 255
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    // #pragma enable_d3d11_debug_symbols

    //enable GPU instancing support
    #pragma multi_compile_instancing

    ENDHLSL

    SubShader
    {

    Stencil 
    {
       ReadMask [_StencilReadMask]
        Ref [_StencilRef]
       Comp Equal
       Pass Keep
    }

    ColorMask  0
    ZWrite Off

    Pass 
    {
        Name "FirstPass"
        Cull Back
        ZTest Less
    }



        }
    
}

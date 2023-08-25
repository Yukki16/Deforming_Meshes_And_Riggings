
Shader "DepthTest"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,0)
        _ColorMap("ColorMap", 2D) = "white" {}
    }

    SubShader
    {

    

    ColorMask  A
        Pass 
    {
    
        Name "SecondPass"
    Cull Front
    ZTest Greater
    ZWrite On

    }



    }
    
}
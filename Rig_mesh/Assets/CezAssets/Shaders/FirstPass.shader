Shader "FirstPass"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,0)
        _ColorMap("ColorMap", 2D) = "white" {}
    }

    SubShader
    {

    

    ColorMask  A
    //ZWrite On

    Pass 
    {
        Name "FirstPass"
        Cull Back
        //ZTest Greater
    }



        }
    
}

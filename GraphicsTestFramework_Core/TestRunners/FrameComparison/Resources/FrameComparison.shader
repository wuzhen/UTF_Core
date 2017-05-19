Shader "Hidden/FrameComparison"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReferenceTex("Texture", 2D) = "white" {}
    }

    CGINCLUDE
            
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        sampler2D _ReferenceTex;

        float4 frag(v2f_img i) : SV_Target
        {
            return abs(tex2D(_MainTex, i.uv) - tex2D(_ReferenceTex, i.uv));
        }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

                #pragma vertex vert_img
                #pragma fragment frag

            ENDCG
        }
    }
}

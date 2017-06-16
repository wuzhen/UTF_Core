Shader "Hidden/CameraDebug"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MinMax ("MinMax", Vector) = (0,0,0,0)
		_MotionMultiplier ("Motion Vector Multiplier", float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		// Depth
		Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragDepth
            ENDCG
        }

		// Normals
		Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragNormals
            ENDCG
        }

		// Motion Vectors
		Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragMotionVectors
            ENDCG
        }
	}

	CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _CameraDepthTexture;
	sampler2D _CameraDepthNormalsTexture;
	sampler2D _CameraMotionVectorsTexture;
    float4 _MinMax;
	float _MotionMultiplier;

	// Convert a motion vector into RGBA color.
	float4 VectorToColor(float2 mv)
	{
		float phi = atan2(mv.x, mv.y);
		float hue = (phi / UNITY_PI + 1.0) * 0.5;

		float r = abs(hue * 6.0 - 3.0) - 1.0;
		float g = 2.0 - abs(hue * 6.0 - 2.0);
		float b = 2.0 - abs(hue * 6.0 - 4.0);
		float a = length(mv);

		return saturate(float4(r, g, b, a));
	}
 
    fixed4 fragDepth (v2f_img i) : SV_Target
    {
        float4 depth = tex2D(_CameraDepthTexture, i.uv);
        float adjustedDepth = smoothstep(_MinMax.x, _MinMax.y, depth);
        return fixed4(adjustedDepth, adjustedDepth, adjustedDepth, 1);
    }

	fixed4 fragNormals (v2f_img i) : SV_Target
    {
        float4 normals = tex2D(_CameraDepthNormalsTexture, i.uv);
        return fixed4(normals.rgb, 1);
    }

	fixed4 fragMotionVectors (v2f_img i) : SV_Target
    {
        float4 movecs = tex2D(_CameraMotionVectorsTexture, i.uv) * _MotionMultiplier;
		float4 rgbMovecs = VectorToColor(movecs.rg);
        return fixed4(rgbMovecs.rgb, 1);
    }

	ENDCG
}

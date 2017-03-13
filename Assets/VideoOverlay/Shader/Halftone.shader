Shader "Hidden/VideoOverlayEffects/Halftone"
{
    Properties
    {
        _MainTex("", 2D) = "black"{}
        _SourceTex("", 2D) = "black"{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _SourceTex;

    half4 _Color;   // given as gamma
    half4 _BGColor; // given as gamma

    float4 _UV2Grid;
    float4 _Grid2UV;
    float _Radius;

    float Halftone(float2 p, half2 offs)
    {
        const float sy = 0.86602540378; // sqrt(1-0.5^2)
        const float rcp_sy = 1.15470053838; // 1/sy

        // fill vertical gap by scaling
        p.y *= rcp_sy;

        // column, row
        half2 p_i = floor(p) + offs;

        // odd line offset
        half2 odd = half2(frac(p_i.y * 0.5), 0);

        // center point
        float2 p_c = floor(p - odd + offs) + odd;

        // difference from the center
        float2 diff = p - p_c - 0.5;

        // cancel the vertical scaling factor
        diff.y *= sy;

        // sample the source
        float2 uv = mul(float2x2(_Grid2UV), p_c * float2(1, sy));
        half r = (1 - Luminance(tex2D(_SourceTex, uv).rgb)) * 0.71;

        return saturate((r - length(diff)) * _Radius);
    }

    half4 frag(v2f_img i) : SV_Target
    {
        float2 p = mul(float2x2(_UV2Grid), i.uv);

        // Apply halftone to the center and the neighbor points.
        float t =  Halftone(p, half2(-0.5, -1));
        t = max(t, Halftone(p, half2( 0.5, -1)));
        t = max(t, Halftone(p, half2(-1.0,  0)));
        t = max(t, Halftone(p, half2(-1.0,  0)));
        t = max(t, Halftone(p, half2( 0.0,  0)));
        t = max(t, Halftone(p, half2( 1.0,  0)));
        t = max(t, Halftone(p, half2(-0.5,  1)));
        t = max(t, Halftone(p, half2( 0.5,  1)));

        // Color blending.
        half4 src = tex2D(_MainTex, i.uv);
        src.rgb = lerp(src.rgb, _BGColor.rgb, _BGColor.a);
        half3 rgb = lerp(src.rgb, _Color.rgb, _Color.a * t);

        #if !defined(UNITY_COLORSPACE_GAMMA)
        rgb = GammaToLinearSpace(rgb);
        #endif

        return half4(rgb, src.a);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma multi_compile __ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}

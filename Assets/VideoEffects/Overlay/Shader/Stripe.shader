Shader "Hidden/VideoEffects/Overlay/Stripe"
{
    Properties
    {
        _MainTex("", 2D) = "black"{}
        _SourceTex("", 2D) = "black"{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "SimplexNoiseGrad3D.cginc"

    sampler2D _MainTex;
    sampler2D _SourceTex;

    half4 _Color;   // given as gamma
    half4 _BGColor; // given as gamma

    float4 _Trans;
    float4 _InvTrans;

    half _Repeat;
    half _Thin;
    half _Width;
    half3 _Noise; // freq, amp, offs

    half4 frag(v2f_img i) : SV_Target
    {
        float2 p = mul(float2x2(_Trans), i.uv - 0.5);

        // Noise field (ddx, ddy, ddz, value)
        float4 n = snoise(float3(p * _Noise.x, _Noise.z)) * _Noise.y;

        // Distance function
        float d = frac(p.y * _Repeat + n.w) - 0.5;

        // Project to the nearest line and get luminance of the source texture.
        float2 duv = float2(0, d / (_Repeat + n.y));
        duv = mul(float2x2(_InvTrans), duv);
        half l = Luminance(tex2D(_SourceTex, i.uv - duv).rgb);

        // Get anti-aliased line with scaling back to the screen coordinate.
        float ssc = _ScreenParams.y / (_Repeat + n.y);
        half t = saturate(_Thin + (l * _Width * 0.5 - abs(d)) * ssc);

        // Color blending.
        half4 src = tex2D(_MainTex, i.uv);
        #if !defined(UNITY_COLORSPACE_GAMMA)
        src.rgb = LinearToGammaSpace(src.rgb);
        #endif
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

Shader "Hidden/Stripes"
{
    Properties
    {
        _MainTex("", 2D) = "black"{}
        _VideoTex("", 2D) = "black"{}
        _Color("", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _VideoTex;

    half4 _Color;
    half _SourceOpacity;

    half _Interval;
    half _Angle;
    half2 _Width; // min, max
    half4 _Wave; // speed, mod, freq, height

    float2 SinCos(float theta)
    {
        float sn, cs;
        sincos(theta, sn, cs);
        return float2(sn, cs);
    }

    half Wave(float2 uv, half w)
    {
        float2 coord = uv / _Interval;

        float phase1 = _Wave.x * _Time.y + _Wave.y * sin(0.2 * _Time.y);
        float phase2 = _Wave.z * dot(coord, SinCos(_Angle + phase1));

        float potent = dot(coord, SinCos(_Angle)) + sin(phase2) * _Wave.w;

        half lineWidth = lerp(_Width.x, _Width.y, w);
        half modPotent = abs(frac(potent) - 0.5) / _Interval;

        return saturate(lineWidth - modPotent);
    }

    half4 frag(v2f_img i) : SV_Target
    {
        half4 col = tex2D(_MainTex, i.uv);
        col.rgb = LinearToGammaSpace(col.rgb) * _SourceOpacity;

        fixed lum = Luminance(tex2D(_VideoTex, i.uv).rgb);
        fixed param = Wave(i.uv, lum) * _Color.a;

        col.rgb = lerp(col.rgb, _Color.rgb, param);
        col.rgb = GammaToLinearSpace(col.rgb);

        return col;
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

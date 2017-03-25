Shader "Hidden/Voxelizer/PostFx"
{
    Properties
    {
        _MainTex("", 2D) = "white"{}
        _FillColor("", Color) = (0, 0, 1, 1)
        _LineColor("", Color) = (0, 0, 0, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    float3 _FillColor;
    float3 _LineColor;

    fixed RobertsCross(float2 uv)
    {
        float4 duv = float4(0, 0, _MainTex_TexelSize.xy);
        half n11 = dot(tex2D(_MainTex, uv + duv.xy), 1);
        half n12 = dot(tex2D(_MainTex, uv + duv.zy), 1);
        half n21 = dot(tex2D(_MainTex, uv + duv.xw), 1);
        half n22 = dot(tex2D(_MainTex, uv + duv.zw), 1);
        half g = length(half2(n11 - n22, n12 - n21));
        return saturate(g * 10);
    }

    fixed4 frag(v2f_img i) : SV_Target
    {
        fixed edge = RobertsCross(i.uv);
        return fixed4(lerp(_FillColor, _LineColor, edge), 1);
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

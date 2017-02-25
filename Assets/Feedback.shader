Shader "Hidden/Feedback"
{
    Properties
    {
        _MainTex("", 2D) = "black"{}
        _PrevTex("", 2D) = "black"{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _PrevTex;

    fixed4 frag(v2f_img i) : SV_Target
    {
        fixed4 c1 = tex2D(_PrevTex, i.uv);
        fixed4 c2 = tex2D(_MainTex, i.uv);
        return max(c1 * 0.92, c2);
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

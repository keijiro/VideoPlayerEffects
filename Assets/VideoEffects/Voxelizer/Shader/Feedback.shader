Shader "Hidden/VideoEffects/Voxelizer/Feedback"
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
    float _Convergence;

    fixed4 frag(v2f_img i) : SV_Target
    {
        fixed l0 = tex2D(_PrevTex, i.uv).r;
        fixed l1 = Luminance(tex2D(_MainTex, i.uv).rgb);
        fixed li = lerp(l1, l0, exp(_Convergence * unity_DeltaTime.x));
        return (l1 - l0) > 0.025 ? l1 : li;
    }

    v2f_img vert_debug(appdata_img v)
    {
        // a little bit messy way to fit the quad to the screen
        v.vertex.z -= 1;
        v2f_img o;
        o.pos = UnityViewToClipPos(v.vertex);
        o.pos.xy /= abs(o.pos.xy);
        o.uv = v.texcoord;
        return o;
    }

    fixed4 frag_debug(v2f_img i) : SV_Target
    {
        return tex2D(_PrevTex, i.uv.xy).r;
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
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_debug
            #pragma fragment frag_debug
            ENDCG
        }
    }
}

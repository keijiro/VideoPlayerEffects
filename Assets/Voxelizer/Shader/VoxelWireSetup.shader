Shader "Voxelizer/Wire Setup"
{
    Properties
    {
        [HideInInspector] _ModTex("", 2D) = "black"{}
    }

    CGINCLUDE

    #include "Common.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD1;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
    };

    v2f vert(appdata v)
    {
        fixed hash = frac(dot(v.uv, float2(2.7348759, 9.40534412)));

        ModifyVertex(v.vertex.xyz, v.normal, v.uv);

        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = fixed4(v.normal * 0.5 + 0.5, hash);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        return i.color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}

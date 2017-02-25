Shader "Voxelizer/Solid"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Glossiness("Smoothness", Range(0, 1)) = 0.0
        _Metallic("Metallic", Range(0, 1)) = 0.0
        [HideInInspector] _ModTex("", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows addshadow
        #pragma target 3.0

        #include "Common.cginc"

        struct Input { float dummy; };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;

        void vert(inout appdata_full v)
        {
            ModifyVertex(v.vertex.xyz, v.normal, v.texcoord1.xy);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }

        ENDCG
    }
    FallBack "Diffuse"
}

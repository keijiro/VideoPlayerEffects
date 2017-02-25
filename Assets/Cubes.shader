Shader "Cubes"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Glossiness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0, 1)) = 0

        [HideInInspector]
        _ModTex("", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert addshadow nolightmap
        #pragma target 3.0

        struct Input {
            float dummy;
        };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;

        sampler2D _ModTex;
        half _Scale;
        half2 _Extent;

        void vert(inout appdata_full v)
        {
            float2 uv = v.texcoord1.xy;

            float3 samp = tex2Dlod(_ModTex, float4(uv, 0, 0)).xyz;

            float l = pow(saturate((Luminance(samp) - 0.05) / 0.95), 0.25);

            float3 dpos = float3(_Extent * (uv - 0.5f), -0.1f * l);

            float scale = _Scale * l;

            v.vertex.xyz = v.vertex.xyz * scale + dpos;
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

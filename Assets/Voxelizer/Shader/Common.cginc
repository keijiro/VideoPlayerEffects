#include "UnityCG.cginc"
#include "SimplexNoise2D.cginc"

sampler2D _ModTex;
half _Scale;
half2 _Extent;

// Quaternion multiplication
// http://mathworld.wolfram.com/Quaternion.html
float4 QMul(float4 q1, float4 q2)
{
    return float4(
        q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
        q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

// Vector rotation with a quaternion
// http://mathworld.wolfram.com/Quaternion.html
float3 RotateVector(float3 v, float4 r)
{
    float4 r_c = r * float4(-1, -1, -1, 1);
    return QMul(r, QMul(float4(v, 0), r_c)).xyz;
}

// Rotation with a given angle and axis 
float4 RotationAngleAxis(float angle, float3 axis)
{
    float sn, cs;
    sincos(angle * 0.5, sn, cs);
    return float4(axis * sn, cs);
}

void ModifyVertex(inout float3 vp, inout float3 norm, float2 uv)
{
    float3 sn = snoise(uv * 8.3 + float2(0, _Time.y * 0.6));

    float samp = tex2Dlod(_ModTex, float4(uv, 0, 0)).r;

    float l = pow(saturate((samp - 0.05) / 0.95), 0.25);

    float3 dpos = float3(_Extent * (uv - 0.5f) + sn.xy * _Scale * 0.3 * (1 - l), -0.1f * l);

    float scale = _Scale * l * (1 + sn.z * 0.4);

    float4 rot1 = RotationAngleAxis(sn.x * 0.5 * (1 - l) , float3(0, 1, 0));
    float4 rot2 = RotationAngleAxis(sn.y * 0.5 * (1 - l) , float3(1, 0, 0));

    vp = RotateVector(vp, QMul(rot1, rot2)) * scale + dpos;
    norm = RotateVector(norm, QMul(rot1, rot2));
}

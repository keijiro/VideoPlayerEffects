#include "UnityCG.cginc"
#include "SimplexNoise2D.cginc"

sampler2D _ModTex;
half _Threshold;
half2 _Extent;
half _ZMove;
half _Scale;
half2 _NoiseParams; // (frequency, speed)
half3 _NoiseAmp;    // (position, rotation, scale)

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

// Vertex modifier function
void ModifyVertex(inout float3 position, inout float3 normal, float2 uv)
{
    // Modifier amount
    half amount = tex2Dlod(_ModTex, float4(uv, 0, 0)).r;
    amount = saturate((amount - _Threshold) / (1 - _Threshold));

    // Reference point in the noise field
    half2 noise_pos = uv * _NoiseParams.x;
    noise_pos.y = noise_pos.y * _Extent.y / _Extent.x + _NoiseParams.y * _Time.y;

    // (noise grad x, noise grad y, noise value)
    half3 nfield = snoise(noise_pos);

    // Displacement
    half3 disp = half3(_Extent * (uv - 0.5f), _ZMove * amount);
    disp.xy += nfield.xy * (_NoiseAmp.x * (1 - amount));

    // Rotation
    float3 raxis = normalize(float3(nfield.xy, 0*nfield.z));
    float4 rot = RotationAngleAxis(_NoiseAmp.y * (1 - amount), raxis);

    // Scaling
    float scale = _Scale * amount * (1 + _NoiseAmp.z * nfield.z);

    // Apply modification
    position = RotateVector(position, rot) * scale + disp;
    normal = RotateVector(normal, rot);
}

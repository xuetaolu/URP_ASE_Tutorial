#ifndef SHADER_LIB_COMMON_INCLUDED
#define SHADER_LIB_COMMON_INCLUDED
#include "UnityCG.cginc"
float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}

float Remap(float In, float InMin, float InMax, float OutMin, float OutMax)
{
    return OutMin + (In - InMin) * (OutMax - OutMin) / (InMax - InMin);
}

float SphericalGaussianPow(float x, float n)
{
    float n2 = n * 1.4427f + 1.4427f; // 1.4427f --> 1/ln(2)
    return exp2(x*n2-n2);
}

// in_x 通常 >= 0
// in_pre_compute_a = -log(in_start_max)
// 当 x = 0 时，返回 in_start_max
// 当 x > 0 时，返回从 in_start_max 指数衰减
// https://graphtoy.com/?f1(x,t)=0.9716-pow(E,%20-0.02881%20-x)&v1=true&f2(x,t)=x&v2=true&f3(x,t)=(0.9716-pow(E,%20-0.02881%20-x))/x&v3=true&f4(x,t)=&v4=false&f5(x,t)=&v5=false&f6(x,t)=&v6=false&grid=1&coords=0.007173222870844816,0.027355005643491415,1.810513572381306
float ExpDamping(float in_x, float in_start_max, float in_pre_compute_a)
{
    const float local_exponent = min(/*log(in_start_max)*/ in_pre_compute_a - in_x, 80.0);
    
    return abs(in_x) > 0.01 ? (in_start_max - exp(local_exponent)) / in_x : in_start_max;
}

float ExpDamping(float in_x, float in_start_max)
{
    // 2023-12-25 结合后效雾的逻辑，in_pre_compute_a 应该是 log(in_start_max)，而不是 -log(in_start_max)
    return ExpDamping(in_x, in_start_max, log(in_start_max));
}

// 参考 ASE 的 ReconstructWorldPositionFromDepth ASE function
float4 ReconstructWorldPositionFromDepth(float2 _screenPos01, float _rawDepth)
{

    // 拼凑 _clipPos，前方是 +Z
    float4 _clipPos;
    {
	    #ifdef UNITY_REVERSED_Z
	    float _frontDepth = ( 1.0 - _rawDepth );
	    #else
	    float _frontDepth = _rawDepth;
	    #endif

        _clipPos = float4(_screenPos01.xy * 2 - 1, _frontDepth*2 - 1, 1.0);
    }


    float4 _viewPos = mul(unity_CameraInvProjection, _clipPos);
    _viewPos.xyz /= _viewPos.w;

    // InvertDepthDir， unity 的camera space正前方是 -Z，和拼凑的 _clipPos 前方相反
    _viewPos.z *= -1;

    float4 _worldPos = mul(unity_CameraToWorld, float4(_viewPos.xyz, 1.0));
    
    return _worldPos;
}

// 输入的图片是 sRGB，亮度偏高的，需要做一遍 GammaToLinearSpace 压低
float4 tex2DsRGB(sampler2D _sampler2D, float2 uv)
{
    float4 res = tex2D(_sampler2D, uv);
    res.rgb = GammaToLinearSpace(res.rgb);
    return res;
}

#endif
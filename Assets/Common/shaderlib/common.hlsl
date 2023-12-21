#ifndef SHADER_LIB_COMMON_INCLUDED
#define SHADER_LIB_COMMON_INCLUDED

float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}

float Remap(float In, float InMin, float InMax, float OutMin, float OutMax)
{
    return OutMin + (In - InMin) * (OutMax - OutMin) / (InMax - InMin);
}

#endif
#ifndef XUE_HOUDINI_SH_INCLUDED
#define XUE_HOUDINI_SH_INCLUDED


// 详见：https://catlikecoding.com/unity/tutorials/rendering/part-5/
// Spherical Harmonics Bands 部分
// l0, l1, l2 等参数已经预乘了其他常数。
half HoudiniSpaceShadeSH9(half3 direction, half4 l0l1, half4 l2_1stTo4th, half l2_5th)
{
    // l0l1.x   为 l0 的 Y0_0
    // l0l1.yzw 为 l1 的 Y1_-1, Y1_0, Y1_1
    half3 l0l1result = dot(direction.yzx, l0l1.yzw) + l0l1.x;

    half x1, x2;
    // 4 of the quadratic (L2) polynomials
    half4 vB = direction.xyzz * direction.yzzx;
    x1 = dot(l2_1stTo4th, vB);

    // Final (5th) quadratic (L2) polynomial
    half vC = direction.x*direction.x - direction.y*direction.y;
    x2 = l2_5th * vC;
    
    return l0l1result + x1 + x2;
}

/**
 * direction 为归一化方向
 * l0, l1, l2 等参数是预乘了常量的参数
 * 关于 l2 的 Y2_0 的预乘详见：
 *   https://catlikecoding.com/unity/tutorials/rendering/part-5/
 *   Spherical Harmonics Bands 部分
 */
half UnitySpaceShadeSH9(half3 direction, half4 l0l1, half4 l2_1stTo4th, half l2_5th )
{
    // 做了调整不需要下面这样
    // half3 houdiniDirection = half3(-direction.x, -direction.z, direction.y);
    
    // houdini 中计算sh参数，
    //   极角(θ) 0~π 对应 +Z -> -Z，
    //   方位角(φ) 0~2π 对应 -X -> +Y -> +X -> -Y -> -X。
    // 对应Unity中
    //    极角(θ) 0~π 对应 +Z -> -Z，
    //    方位角(φ) 0~2π 对应 +X -> +Y -> -X -> -Y -> +X。
    // 则Unity中sh计算过程涉及的 θ, φ 符合数学上球坐标与笛卡尔坐标对应关系。
    // 可以直接使用直角坐标系表达式算球谐函数
    // 直角坐标中的表达式参考：https://zh.wikipedia.org/wiki/%E7%90%83%E8%B0%90%E5%87%BD%E6%95%B0
    
    return HoudiniSpaceShadeSH9(direction, l0l1, l2_1stTo4th, l2_5th);
}



#endif
#include "genship_cloud_common.hlsl"

// #define _ProjectionParams float4(1.00, 0.25, 6000.68506, 0.00017) // _58._m0
// #define UNITY_MATRIX_M _58._m1
// static const matrix UNITY_MATRIX_M = {
//     float4(1.00, 0.00, 0.00, 0.00),
//     float4(0.00, 1.00, 0.00, 0.00),
//     float4(0.00, 0.00, 1.00, 0.00),
//     float4(0.00, 0.00, 0.00, 1.00)
// }; // _58._m1
// #define transpose(UNITY_MATRIX_VP) _58._m2
// UNITY_MARRIX_V
// UNITY_MATRIX_VP
// static const matrix transpose(UNITY_MATRIX_VP) = {
//     float4(-1.11146, 1.95758E-08, 0.57462, 0.57457         ),
//     float4(5.51985E-09, 2.41421, -8.32307E-09, -8.32238E-09),
//     float4(-0.78026, -1.08062E-08, -0.81853, -0.81846      ),
//     float4(-1.93807, -1207.10669, 3.5313, 4.03098          )
// }; //_58._m2
#define _RolePos_maybe  float3(-3.48413, 195.00, 2.47919) // _58._m3
#define _UpDir  float3(0.00, 1.00, 0.00         ) // _58._m4
#define _58__m5  float3(0.00972, 0.02298, 0.06016) // _58._m5
#define _58__m6  float3(0.00972, 0.02298, 0.06016) // _58._m6
#define _58__m7  float3(0.0538, 0.09841, 0.2073  ) // _58._m7
#define _58__m8  float3(0.0538, 0.09841, 0.2073  ) // _58._m8
#define _58__m9  0.49336  // _58._m9
#define _58__m10 0.20     // _58._m10
#define _58__m11 float3(0.00837, 0.10516, 0.26225) // _58._m11
#define _58__m12 0.50 // _58._m12
#define _58__m13 0.30 // _58._m13
#define _58__m14 float3(0.00688, -0.84638, -0.53253) // _58._m14
#define _58__m15 float3(0.01938, 0.00651, 0.02122  ) // _58._m15
#define _58__m16 4.09789 // _58._m16
#define _58__m17 0.80205 // _58._m17
#define _58__m18 8.30078 // _58._m18
#define _58__m19 float3(0.01938, 0.00651, 0.02122) // _58._m19
#define _58__m20 0.01039 // _58._m20
#define _58__m21 float3(0.31638, 0.70655, 0.633) // _58._m21
#define _58__m22 float3(0.29669, 0.64985, 1.00 ) // _58._m22
#define _58__m23 3.29897 // _58._m23
#define _58__m24 0.19794 // _58._m24
#define _58__m25 0.50    // _58._m25
#define _58__m26 262.33862 // _58._m26
#define _58__m27 float3(0.05199, 0.10301, 0.13598) // _58._m27
#define _58__m28 float3(0.10391, 0.41824, 0.88688) // _58._m28
#define _58__m29 float3(0.00, 0.03576, 0.12083   ) // _58._m29
#define _58__m30 float3(0.02281, 0.05716, 0.14666) // _58._m30
#define _58__m31 0.0881      // _58._m31
#define _58__m32 0.11        // _58._m32
#define _58__m33 1.00        // _58._m33
#define _58__m34 0.8299      // _58._m34
#define _MaskMapScale float2( 2.00, 4.00 ) // _58._m35
// float2 _MaskMapScale;
#define _58__m36 3.00        // _58._m36
#define _58__m37 6.00        // _58._m37
#define _58__m38 1.00        // _58._m38

sampler2D _IrradianceMap;

float FastAcosForAbsCos(float in_abs_cos) {
    float _local_tmp = ((in_abs_cos * -0.0187292993068695068359375 + 0.074261002242565155029296875) * in_abs_cos - 0.212114393711090087890625) * in_abs_cos + 1.570728778839111328125;
    return _local_tmp * sqrt(1.0 - in_abs_cos);
}

float FastAcos(float in_cos) {
    float local_abs_cos = abs(in_cos);
    float local_abs_acos = FastAcosForAbsCos(local_abs_cos);
    return in_cos < 0.0 ?  UNITY_PI - local_abs_acos : local_abs_acos;
}

v2f vert (appdata v)
{

    float4 Vertex_Position = v.vertex;
    float4 Vertex_1 = v.color;
    float4 Vertex_2 = float4( v.uv, 0,0 ) ;
    
    // Vertex_3.x = 171.435
    // Vertex_3.y = 17.8982 ~ 153.264
    // Vertex_3.z = 0.4
    // Vertex_3.w = 0.6
    float4 Vertex_3 = fixed4( v.uv2, v.uv3 );
    v2f o;
    

    
    
    float2 _29;
    bool _32;

    float3 _35;
    float3 _36;
    float3 _37;
    float _39;
    float _40;
    float3 _41;
    float3 _42;

    float2 _44;
    float _45;
    float _46;

    float _miu;

    float _50;
    float _51;
    float _52;
    

    float4 _WorldPos = mul(UNITY_MATRIX_M, Vertex_Position);
    float4 _clipPos = mul(UNITY_MATRIX_VP, _WorldPos);
    _clipPos.z = _clipPos.w;
    o.vertex = _clipPos;



    // #define _RolePos_maybe  float3(-3.48413, 195.00, 2.47919) // _58._m3
    float3 _worldPos_relativeToRole = _WorldPos.xyz - _RolePos_maybe;
    
    float3 _relativeToRoleDir = normalize(_worldPos_relativeToRole);
    
    _miu = clamp(dot(_UpDir, _relativeToRoleDir), -1.0, 1.0);
    
    float _angle_up_to_down_1_n1 = (UNITY_HALF_PI - FastAcos(_miu)) * UNITY_INV_HALF_PI;

    o.Varying_RelativeToRoleDirXYZ_Angle1_n1.w = _angle_up_to_down_1_n1;
    o.Varying_RelativeToRoleDirXYZ_Angle1_n1.xyz = _relativeToRoleDir;
    


    
    float _47;
    _47 = (_MaskMapScale.x * _MaskMapScale.y) + (-1.0);
    _47 = (Vertex_1.y * _47) + 0.5;
    _47 = floor(_47);
    
    float4 _26;
    _26.x = _47 * _MaskMapScale.x;
    _32 = _26.x >= (-_26.x);
    _26.x = _32 ? _MaskMapScale.x : (-_MaskMapScale.x);
    _40 = 1.0 / _26.x;
    _40 = _47 * _40;
    _47 /= _MaskMapScale.x;
    float4 _33;
    _33.y = floor(_47);
    _47 = frac(_40);
    _33.x = _47 * _26.x;
    _26.xy = _33.xy + Vertex_2.xy;

    o.Varying_0.xy = float2(_26.x / _MaskMapScale.x, _26.y / _MaskMapScale.y);
    _47 = _58__m26 * _58__m37;

    _26.xy = _47 * float2(1.2, 0.8);

    o.Varying_0.zw = Vertex_2.xy * _58__m36 + _26.xy;



    // Vertex_3.x = 171.435
    // Vertex_3.y = 17.8982 ~ 153.264
    // Vertex_3.z = 0.4
    // Vertex_3.w = 0.6
    
    _47 = (-Vertex_3.w) + 1.0;
    _47 = 1.0 / _47;
    
    _26.x = max(Vertex_3.x, 9.9999997473787516355514526367188e-06);
    _26.x = Vertex_3.y / _26.x;
    _26.x *= _58__m33;
    _40 = (_26.x * _58__m38) + (-Vertex_3.w);
    _26.x *= _58__m38;
    _47 *= _40;
    _47 = clamp(_47, 0.0, 1.0);
    _40 = (_47 * (-2.0)) + 3.0;
    _47 *= _47;
    _47 = ((-_40) * _47) + 1.0;
    _40 = 1.0 / Vertex_3.z;
    _26.x = _40 * _26.x;
    _26.x = clamp(_26.x, 0.0, 1.0);
    _40 = (_26.x * (-2.0)) + 3.0;
    _26.x *= _26.x;
    _26.x *= _40;
    _47 = ((-_26.x) * _47) + 1.0;
    o.Varying_2.w = _47;
    o.Varying_2.yz = Vertex_1.zw;
    _42.x = dot(_relativeToRoleDir, _58__m14);
    _35.z = (_42.x * 0.5) + 0.5;
    o.Varying_2.x = _35.z * _58__m34;
    _50 = max(_58__m10, 9.9999997473787516355514526367188e-05);
    _50 = 1.0 / _50;
    _29.x = _50 * abs(_angle_up_to_down_1_n1);
    _29.y = 0.5;
    _44.y = 0.5;
    _47 = tex2Dlod(_IrradianceMap, float4(_29, 0.0, 0.0)).x;
    _42.z = (_42.x * _58__m9) + (-_58__m9);
    _42.x = (_42.x * _58__m31) + (-_58__m31);
    float2 _533 = _42.xz + (1.0);
    _42 = float3(_533.x, _42.y, _533.y);
    float2 _539 = max(_42.xz, (0.0));
    _42 = float3(_539.x, _42.y, _539.y);
    _36.x = _42.z * _42.z;
    _50 = _42.z * _36.x;
    _36 = _58__m7 + (-_58__m8);
    _36 = ((_50) * _36) + _58__m8;
    _37 = _58__m5 + (-_58__m6);
    _37 = ((_50) * _37) + _58__m6;
    _36 += (-_37);
    _36 = ((_47) * _36) + _37;
    _50 = max(_58__m13, 9.9999997473787516355514526367188e-05);
    _50 = 1.0 / _50;
    _44.x = _50 * abs(_angle_up_to_down_1_n1);
    _47 = tex2Dlod(_IrradianceMap, float4(_44, 0.0, 0.0)).y;
    _37 = float3(_58__m11.x * _58__m12, _58__m11.y * _58__m12, _58__m11.z * _58__m12);
    _37 = (_47) * _37;
    _35.x = abs(_58__m14.y) + (-0.20000000298023223876953125);
    _35.x *= 3.3333332538604736328125;
    _35.x = clamp(_35.x, 0.0, 1.0);
    _50 = (_35.x * (-2.0)) + 3.0;
    _35.x *= _35.x;
    _35.x *= _50;
    _50 = _35.z;
    _50 = clamp(_50, 0.0, 1.0);
    _50 += (-0.300000011920928955078125);
    _50 *= 1.4285714626312255859375;
    _50 = max(_50, 0.0);
    _51 = (_50 * (-2.0)) + 3.0;
    _50 *= _50;
    _52 = _50 * _51;
    _50 = ((-_51) * _50) + 1.0;
    _35.x = (_35.x * _50) + _52;
    _36 = (_37 * _35.xxx) + _36;
    _47 = dot(_relativeToRoleDir, _UpDir);
    _35.x = abs(_47) * _58__m18;
    _33.x = dot(float3(_58__m14.x, _58__m14.y, _58__m14.z), _relativeToRoleDir);
    _33.x = (_33.x * 0.5) + 0.5;
    _33.x = clamp(_33.x, 0.0, 1.0);
    _41.x = log2(_33.x);
    _45 = _41.x * _35.x;
    _37 = _35.xxx * float3(0.100000001490116119384765625, 0.00999999977648258209228515625, 0.5);
    float2 _750 = _41.xx * _37.xy;
    _41 = float3(_750.x, _41.y, _750.y);
    float2 _755 = exp2(_41.xz);
    _41 = float3(_755.x, _41.y, _755.y);
    _41.y = exp2(_45);
    _41 = min(_41, (1.0));
    _41.x = (_41.x * 0.119999997317790985107421875) + _41.y;
    _41.x = (_41.z * 0.02999999932944774627685546875) + _41.x;
    _41.x *= _58__m20;
    _41 = _41.xxx * _58__m19;
    _39 = _33.x + (-0.5);
    _35.x = log2(_33.x);
    _35.x *= _37.z;
    _35.x = exp2(_35.x);
    _35.x = _47 * _35.x;
    _35.x = clamp(_35.x, 0.0, 1.0);
    _35.x *= _58__m20;
    _37 = _35.xxx * _58__m15;
    _47 = _39 + _39;
    _47 = max(_47, 0.0);
    _33.x = (_47 * (-2.0)) + 3.0;
    _47 *= _47;
    _47 *= _33.x;
    float3 _853 = (_41 * (_47)) + _36;
    _33 = float4(_853.x, _853.y, _853.z, _33.w);
    o.Varying_3 = _33.xyz;
    _47 = dot(_58__m21, _relativeToRoleDir);
    _47 = clamp(_47, 0.0, 1.0);
    _35.x = dot(_relativeToRoleDir, _58__m21);
    _35.x = (_35.x * 0.5) + 0.5;
    float2 _886 = _35.xz + (-float2(_58__m17, _58__m17));
    _35 = float3(_886.x, _35.y, _886.y);
    _50 = _47 * _47;
    _50 = _47 * _50;
    _50 = _47 * _50;
    _50 = (_50 * _47) + (-0.5);
    _50 += _50;
    _50 = max(_50, 0.0);
    _36.x = (_50 * (-2.0)) + 3.0;
    _50 *= _50;
    _50 *= _36.x;
    _50 *= _58__m24;
    _36.x = _58__m25 + (-0.5);
    _36.x = ((-abs(_36.x)) * 2.0) + 1.0;
    _50 *= _36.x;
    _36 = (_50) * _58__m22;
    _50 = max(_58__m23, 0.0);
    _50 = min(_50, 0.800000011920928955078125);
    _36 = (_36 * (_50)) + _37;
    _50 = (-_58__m32) + 0.699999988079071044921875;
    _50 *= 2.5000002384185791015625;
    _50 = clamp(_50, 0.0, 1.0);
    _51 = (_50 * (-2.0)) + 3.0;
    _50 *= _50;
    _50 *= _51;
    o.Varying_4 = (_50) * _36;
    _36.x = (-_58__m17) + 1.0;
    _36.x = 1.0 / _36.x;
    float2 _995 = _35.xz * _36.xx;
    _35 = float3(_995.x, _35.y, _995.y);
    float2 _1000 = clamp(_35.xz, (0.0), (1.0));
    _35 = float3(_1000.x, _35.y, _1000.y);
    _36.x = (_35.x * (-2.0)) + 3.0;
    _35.x *= _35.x;
    _35.x *= _36.x;
    _35.x *= _58__m23;
    _35.x *= 0.100000001490116119384765625;
    _35.x *= _35.x;
    _36 = _35.xxx * _58__m22;
    _35.x = (_35.z * (-2.0)) + 3.0;
    _46 = _35.z * _35.z;
    _35.x = _46 * _35.x;
    _35.x *= _58__m16;
    _35.x *= 0.125;
    _35.x *= _35.x;
    _36 = (_58__m19 * _35.xxx) + _36;
    o.Varying_5 = (_50) * _36;
    _35.x = _42.x * _42.x;
    _35.x = _42.x * _35.x;
    _42 = _58__m27 + (-_58__m28);
    o.Varying_6 = (_35.xxx * _42) + _58__m28;
    _42 = _58__m29 + (-_58__m30);
    o.Varying_7 = (_35.xxx * _42) + _58__m30;
    
    return o;
}
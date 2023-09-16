#include "genship_water_common.hlsl"

// #define _Time float4(10.69632, 213.92641, 427.85281, 641.77924 )//_151._m0
// #define _Time _Time//_151._m0
// #define _WorldSpaceCameraPos float3(-76.56767, 199.90689, 87.00145            )//_151._m1
// #define _ProjectionParams float4(1.00, 0.25, 6000.00, 0.00017              )//_151._m2
// #define _ZBufferParams float4(-23999.00, 24000.00, -3.99983, 4.00       )//_151._m3
const float4 _WorldPosXY_Offset = float4(1934.36584, 0.00, -1266.34216, 0.00       ); //_151._m4
#define _LightDir float4(0.12266, 0.55406, 0.82339, 0.00           )//_151._m5
// static const float4 UNITY_MATRIX_V_T[] = {float4(0.63206, -0.34567, 0.69355, 0.00         ),
//                         float4(0.00, 0.895, 0.44607, 0.00               ),
//                         float4(0.77492, 0.28194, -0.56569, 0.00         ),
//                         float4(-19.02394, -229.91272, 13.14762, 1.00    )};//_151._m6

float4 _Color_Far_2 = float4(0.50353, 0.31069, 0.31797, 1.30           ); //_151._m7
float4 _GlossColor = float4(2.92204, 1.56181, 0.57585, 1.62808        ); //_151._m8
#define _151__m9  float3(0.13963, 0.31927, 0.93732              ) //_151._m9
#define _151__m10 float3(0.05565, -0.29114, -0.95506            ) //_151._m10
float4 _151__m11 = float4(0.045, 0.00214, 0.00, 0.00              ); // _151._m11
#define _151__m12 float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
float4 _Color_Far = float4(0.05891, 0.20904, 0.43325, 0.90         ); // _151._m13
float4 _151__m14 = float4(0.27672, 0.01464, -0.23447, 0.00        ); // _151._m14


#define _151__m15 float4(0.00335, -0.66724, 0.00042, -0.00671    ) // _151._m15
#define _151__m16 float4(0.39681, 0.34829, 0.44667, 0.00017      ) // _151._m16
#define _151__m17 float4(-0.001, 9.00, -0.001, 1.19927           ) // _151._m17
float4 _Color_D = float4(1.00, 1.00, 1.00, 16.00                 ); // _151._m18
#define _151__m19 float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
float4 _151__m20 = float4(0.9716, -0.02881, 1.00, 0.00            ); // _151._m20
#define _151__m21 float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
const float4 _WorldPosXY_Offset_Negative = float4(-1934.36584, 0.00, 1266.34216, 0.00     ); // _151._m22
float4 _Color_C = float4(1.00, 1.00, 1.00, 0.07213               ); // _151._m23
static const float4 _151__m24 = float4(1.00, -1.00, 10000.00, 0.00             ); // _151._m24
#define _151__m25 float4(1.00, 1.00, 1.00, -16.00                ) // _151._m25
float4 _151__m26 = float4(0.00, 0.00, 0.00, 0.00                  ); // _151._m26
#define _151__m27 float4(0.00, 0.00, 0.00, 0.00                  ) // _151._m27
float _CausticScale = 0.25;  // _151._m28
float _CausticSpeed = 0.131; // _151._m29

float4 _CausticColor = float4(0.60632, 0.5298, 0.44146, 1.00); // _151._m30
float _CausticVisibleHeightFactor = 3.33333; // _151._m31
float _CausticDistanceFade = 0.01667; // _151._m32
float _CausticNormalDisturbance = 0.096;   // _151._m33
float4 _Noise2D_R_ScaleSpeed = float4(0.20, 0.15, 0.01, 0.01);
// #define _Noise2D_R_ScaleSpeed float4(0.20, 0.15, 0.01, 0.01) // _151._m34
float _FoamLineSpeed = -1.28;                  // _151._m35
float4 _FoamColor = float4(1.00, 1.00, 1.00, 1.00); // _151._m36
float _FoamLineAreaSize = 0.30;   // _151._m37
float _FoamLineFadeDistance = 205.00; // _151._m38
float _FoamLineSinFrequency = 19.00;  // _151._m39
#define _FoamLineAreaBaseMulti 0.30   // _151._m40
#define _FoamLinePosSpeed 0.00   // _151._m41
float _FoamLineVisibleDistance = 10.00;  // _151._m42
float _FoamLineFadeDiv = 20.00;  // _151._m43
float _WorldPosXY_Speed1X = -0.02;  // _151._m44
float _WorldPosXY_Speed1Y = -0.01;  // _151._m45
float _WorldPosXY_Speed2X =  0.05;   // _151._m46
float _WorldPosXY_Speed2Y = -0.04;  // _151._m47
float _NormalScale1 = 0.70;   // _151._m48
#define _151__m49 float4(0.00, 0.00, 0.00, 0.00) // _151._m49
#define _151__m50 float4(0.00, 0.00, 0.00, 0.00) // _151._m50
#define _151__m51 0 // _151._m51
static const float4 _151__m52[] = {float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00)};//_151._m52
static const float4 _151__m53[] = {float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00)};//_151._m53
static const matrix _Matrix_custom_V_maybe = {float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00)};//_151._m54
static const float4 _151__m55[] = {float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00),
                         float4(0.00, 0.00, 0.00, 0.00)};//_151._m55
#define _151__m56 float4(0.00, 0.00, 0.00, 0.00)// _151._m56
#define _151__m57 float4(0.00, 0.00, 0.00, 0.00)// _151._m57
#define _DebugValueMaybe 0.00      // _151._m58
float _GrabTextureFade = 0.00;      // _151._m59
#define _SurfNormalScale 0.15      // _151._m60
float _WaterAlpha = 2.5641;    // _151._m61
float _GlossFactor = 5.00;      // _151._m62
float _FixNDotH_Power = 332.79999; // _151._m63
float _WaterSurfAlpha = 0.40;      // _151._m64
float _GlossPosAdjust = 2.38;      // _151._m65
float4 _WaterSurfColor = float4(0.11131, 1.00, 0.9415, 0.00      ); // _151._m66
float4 _WaterBottomDarkColor = float4(0.01694, 0.1433, 0.26481, 0.00   ); // _151._m67
float _WaterSurfAlphaPower = 1.50;     // _151._m68
float _WaterBottomDarkPower = 1.00;     // _151._m69
float _WaterBottomDarkFactor = 0.06667;  // _151._m70
// #define unity_SpecCube0_HDR float4(1.00, 1.00, 0.00, 0.00         ) // _151._m71
float4 _SurfNormalScale2 = float4(0.10238, 0.09815, 0.59876, 0.00); // _151._m72
float _SSRNormalDisturbance1 = 0.60;    // _151._m73
float _SSRNormalDisturbance2 = 1.51515; // _151._m74
float _SSRAlpha = 0.80;    // _151._m75
#define _SSREnable 1.00    // _151._m76
float _ReflectFactor = 0.50;    // _151._m77
float _ReflectWaterViewYDisappearFactor = 0.93;    // _151._m78
float _ReflectWaterDepthFactor = 0.38462; // _151._m79
float4 _WaterSurfColorBlend = float4(1.00, 1.00, 1.00, 0.00); // _151._m80
float4 _WaterBottomDarkColorBlend = float4(1.00, 1.00, 1.00, 0.00); // _151._m81
#define _ReflectEnable 1.00 // _151._m82
float _WorldPosXY_Scale = 0.05; // _151._m83
float _CausticColorDisappearOfWaterDepth = 0.87; // _151._m84
float _CausticColorDisappearPower = 2.49; // _151._m85
#define _EnableShadow 1.00 // _151._m86
#define _EyeDepthBias 0.00 // _151._m87 
#define _151__m88 float4(1.00, 1.00, 1.00, 1.00) // _151._m88
#define _151__m89 1.00 // _151._m89

sampler2D _DepthTexture ;
sampler2D _ScreenMaskMap ;
// samplerCUBE unity_SpecCube0_;
sampler2D _Noise2D_R;
sampler2D _NormalMap1;
sampler2D _NormalMap2;
sampler3D _Noise3DMap;
sampler3D _20_sampler3D;
sampler2D _21_sampler2D;
sampler2D _22_sampler2D;
sampler2D _GrabTexture;
sampler2D _ScreenReflectTexture;

float3 UnpackNormalWithScaleNotNormalize(float3 in_packedNormal, float in_scale)
{
    float3 out_normal;
    out_normal.xy = (in_packedNormal.xy * 2 - 1) * in_scale;
    out_normal.z  = (in_packedNormal.z * 2 - 1);
    return out_normal;
}

float SphericalGaussianPow(float x, float n)
{
    float n2 = n * 1.4427f + 1.4427f; // 1.4427f --> 1/ln(2)
    return exp2(x*n2-n2);
}

float GenshipCaustic(float3 in_pos) {
    float3 _step1;
    _step1.x = dot(in_pos, float3(-2.0, 3.0, 1.0));
    _step1.y = dot(in_pos, float3(-1.0, -2.0, 2.0));
    _step1.z = dot(in_pos, float3(2.0, 1.0, 2.0));

    float3 _step2;
    _step2.x = dot(_step1, float3(-0.8, 1.2, 0.4));
    _step2.y = dot(_step1, float3(-0.4, -0.8, 0.8));
    _step2.z = dot(_step1, float3(0.8, 0.4, 0.8));

    float3 _step3;
    _step3.x = dot(_step2, float3(-0.6, 0.9, 0.3));
    _step3.y = dot(_step2, float3(-0.3, -0.6, 0.6));
    _step3.z = dot(_step2, float3(0.6, 0.3, 0.6));

    float3 _hnf1 = 0.5 - frac(_step1);
    float3 _hnf2 = 0.5 - frac(_step2);
    float3 _hnf3 = 0.5 - frac(_step3);
    
    float _min_dot_result = min(dot(_hnf3, _hnf3), min(dot(_hnf2, _hnf2), dot(_hnf1, _hnf1)));

    float _local_127 = (_min_dot_result * _min_dot_result * 7.0);
    float _causticNoise3DResult = _local_127 * _local_127;
    
    return _causticNoise3DResult;
}

fixed4 frag (v2f i) : SV_Target
{
    float4 Output_0;
    
    float _35;
    float3 _41;
    float _42;
    // bool _43;
    float3 _47;
    // float _48;
    float _49;
    bool _50;
    float3 _51;
    float2 _54;
    float3 _57;
    float4 _64;
    float4 _66;
    float _74;
    float _75;
    float3 _76;
    float3 _77;
    float3 _81;
    float3 _83;
    float3 _84;
    float3 _85;
    float2 _86;
    // float3 _87;
    float3 _89;
    float _90;
    bool _91;
    // float _92;
    // float _93;
    // bool _94;
    // float _95;
    float3 _100;
    float _101;
    // float3 _103;
    // float _104;
    float2 _105;
    float _106;
    bool _107;
    // float2 _108;
    float2 _109;
    // float _110;
    bool2 _113;
    float _114;
    float _116;
    float _119;
    // float _122;
    float _123;
    // float _124;
    bool _125;
    float _128;
    float _129;
    float _130;
    float _132;
    float _133;
    bool _134;
    float _142;
    float _143;
    
    // #define _LightDir float4(0.12266, 0.55406, 0.82339, 0.00           )//_151._m5
    float3 _lightDir = ((-i.Varying_WorldPosXYZ.xyz) * _LightDir.www) + _LightDir.xyz;
    
    float3 _lightDir1 = _LightDir.w < 0.5 ? _LightDir.xyz : normalize(_lightDir);

    // #define _151__m49 float4(0.00, 0.00, 0.00, 0.00) // _151._m49
    _89.x = 1.0 / (dot(_lightDir, _lightDir) * _151__m49.x + 1.0);
    _89.x = clamp(lerp(-0.04, 1.0, _89.x), 0.0, 1.0);

    // #define _GlossColor float4(2.92204, 1.56181, 0.57585, 1.62808        ) //_151._m8
    float4 _45;
    _45.xyz = _89.xxx * _GlossColor.xyz;

    float3 _glossColor1 = _LightDir.w < 0.5 ? _GlossColor.xyz : _45.xyz;

    float2 _screenPos = i.Varying_NonStereoScreenPos.xy / i.Varying_NonStereoScreenPos.w;
    
    float2 _worldPosXZ1 = i.Varying_WorldPosXYZ.xz + _WorldPosXY_Offset.xz;
    
    float2 _worldPosXYScale = _worldPosXZ1 * float2(_WorldPosXY_Scale, _WorldPosXY_Scale);
    
    float2 _NormalMap1_UV = (_Time.yy * float2(_WorldPosXY_Speed1X, _WorldPosXY_Speed1Y)) + _worldPosXYScale;
    
    float3 _normalSample1 = tex2Dlod(_NormalMap1, float4(_NormalMap1_UV, 0.0, 0.0)).xyz;
    

    float2 _NormalMap2_UV = (_Time.yy * float2(_WorldPosXY_Speed2X, _WorldPosXY_Speed2Y)) + _worldPosXYScale;

    float3 _normalSample2 = tex2Dlod(_NormalMap2, float4(_NormalMap2_UV, 0.0, 0.0)).xyz;
    

    float3 _normal1 = UnpackNormalWithScaleNotNormalize(_normalSample1, _NormalScale1);
    float3 _normal2 = UnpackNormalWithScaleNotNormalize(_normalSample2, _NormalScale1);
    
    float3 _surfNormal = normalize(_normal1.xzy + _normal2.xzy);
    

    float _rawDepth = tex2D(_DepthTexture, _screenPos).x;
    float _depthTextureEyeDepth = LinearEyeDepth(_rawDepth);

    float _backDotV = i.Varying_ViewDirXYZ_BackDotVW.w;
    float _frontDotV = -_backDotV;

    // _depthTextureEyeDepth / dot(_front, -_lookAtDir) * -_lookAtDir
    // _depthTextureEyeDepth / dot(_front, _lookAtDir) * _lookAtDir
    // _lookAtDir * (_depthTextureEyeDepth / dot(_front, _lookAtDir))
    //   注：dot(_front, _lookAtDir) 是 _surfDepth
    float3 _lookThroughAtTerrainDir = _depthTextureEyeDepth / _frontDotV * i.Varying_ViewDirXYZ_BackDotVW.xyz;
    // _57 = _lookThroughAtTerrainDir;

    float3 _lookThroughAtTerrainWorldPos = _WorldSpaceCameraPos.xyz + _lookThroughAtTerrainDir;


    float3 _viewDir = _WorldSpaceCameraPos.xyz - i.Varying_WorldPosXYZ.xyz;

    float3 _viewDirNormalize = normalize(_viewDir);

    // float3 _terrainToSurfDir = -i.Varying_ViewDirXYZ_BackDotVW.xyz * (_depthTextureEyeDepth / _frontDotV) + (i.Varying_WorldPosXYZ.xyz - _WorldSpaceCameraPos.xyz);
    // float3 _terrainToSurfDir = -_lookThroughAtTerrainDir + _lookAtDir;
    float3 _lookAtDir = i.Varying_WorldPosXYZ.xyz - _WorldSpaceCameraPos.xyz;
    float3 _terrainToSurfDir = _lookAtDir - _lookThroughAtTerrainDir;
    
    float _terrainToSurfLength = length(_terrainToSurfDir);

    // _clipPos.w 就是 -_viewPos.z
    // 因为 无论 DX 还是 opengl，UNITY_MATRIX_P[3u] = float4(0, 0, -1, 0)
    float _surfEyeDepth = i.Varying_NonStereoScreenPos.w + _EyeDepthBias;

    float _terrainMoreEyeDepth = clamp(_depthTextureEyeDepth - _surfEyeDepth, 0, 1);

    float2 _nonStereoScreenPosOffset = _terrainMoreEyeDepth * _surfNormal.xz * _SurfNormalScale;
    
    float2 _screenPos2 = (_nonStereoScreenPosOffset + i.Varying_NonStereoScreenPos.xy) / i.Varying_NonStereoScreenPos.w;

    float _rawDepth2 = tex2D(_DepthTexture, _screenPos2).x;
    float _depthTextureEyeDepth2 = LinearEyeDepth(_rawDepth2);

    float _terrainMoreEyeDepth2 = clamp(_depthTextureEyeDepth2 - _surfEyeDepth, 0.0, 1.0);

    float2 _screenPos3 = (_terrainMoreEyeDepth2 * _nonStereoScreenPosOffset + i.Varying_NonStereoScreenPos.xy) / i.Varying_NonStereoScreenPos.w;
    
    float3 _grabTextureSample = tex2D(_GrabTexture, _screenPos3).xyz;

    float _rawDepth3 = tex2D(_DepthTexture, _screenPos3).x;
    float _depthTextureEyeDepth3 = LinearEyeDepth(_rawDepth3);
    

    // _lookAtDir * eyeDepth / dot( _lookAtDir, _front)
    float3 _lookThroughDir3 = i.Varying_ViewDirXYZ_BackDotVW.xyz * _depthTextureEyeDepth3 / _frontDotV;
    
    float3 _lookThroughWorldPos3 = _WorldSpaceCameraPos + _lookThroughDir3;

    float3 _back = UNITY_MATRIX_V[2u].xyz;
    float3 _front = -_back;

    float _if_output_A_0 = 0.0;
    float _if_output_A_1 = 0.0;
    // const float4 _WorldPosXY_Offset_Negative = float4(-1934.36584, 0.00, 1266.34216, 0.00     ); // _151._m22
    if (0.01 < _WorldPosXY_Offset_Negative.w)
    {
        float _local_127;
        // #define _151__m27 float4(0.00, 0.00, 0.00, 0.00                  ) // _151._m27
        if (_151__m27.y < 0.5)
        {
            // static const float4 _151__m24 = float4(1.00, -1.00, 10000.00, 0.00             ); // _151._m24
            float3 _tmpVec3 = _lookThroughWorldPos3.xyz + (-_WorldPosXY_Offset_Negative.xyz);
            float _local_135 = 1.0 - clamp(length(_tmpVec3) * _151__m24.z + _151__m24.w, 0.0, 1.0);
            _local_127 = _local_135 * _local_135;
        }
        else
        {
            float _tmpFloat = _lookThroughWorldPos3.y + (-_WorldPosXY_Offset_Negative.y);
            _local_127 = smoothstep(0, 1, _tmpFloat / _WorldPosXY_Offset_Negative.w);
        }

        // #define _151__m27 float4(0.00, 0.00, 0.00, 0.00                  ) // _151._m27
        _if_output_A_0 = _local_127 * float(0.05 <= _151__m27.x);
        _if_output_A_1 = _local_127 * float(_151__m27.x <= 0.95);
    }
    

    
    float _lookThroughDir3_length = length(_lookThroughDir3);
    
    // #define _151__m15 float4(0.00335, -0.66724, 0.00042, -0.00671    ) // _151._m15
    float _lookThroughDir3_length_SO1 = clamp(_lookThroughDir3_length * _151__m15.z + _151__m15.w, 0.0, 1.0);
    // #define _151__m25 float4(1.00, 1.00, 1.00, -16.00                ) // _151._m25
    float _lookThroughDir3_length_SO2 = clamp(_lookThroughDir3_length * _151__m25.z + _151__m25.w, 0.0, 1.0);
    

    float _lerp_127 = lerp(_lookThroughDir3_length_SO1, _lookThroughDir3_length_SO2, _if_output_A_0);

    
    // y = 0 / 1 \ 0, x ∈ [0, 2]
    float _lerp_127_curve = _lerp_127 * (-_lerp_127 + 2.0);
    
    float _lookThroughDir3xz_length = length(_lookThroughDir3.xz);
    

    // _ProjectionParams.z = far plane
    float _far_plane = _ProjectionParams.z * 0.9999;
    bool _isOutOfFarPlane = dot(_lookThroughDir3, _front) >= _far_plane;
    // #define _151__m14 float4(0.27672, 0.01464, -0.23447, 0.00        ) // _151._m14
    float _switch_value_1 = _isOutOfFarPlane ? _lerp_127_curve * _151__m14.w : _lerp_127_curve;
    
    // #define _151__m17 float4(-0.001, 9.00, -0.001, 1.19927           ) // _151._m17
    _132 = clamp(_lookThroughDir3xz_length * _151__m17.x + _151__m17.y, 0.0, 1.0);
    _74 = clamp(_WorldSpaceCameraPos.y * _151__m17.z + _151__m17.w, 0.0, 1.0);
    
    float _switch_value_2 = _isOutOfFarPlane ? _74 : _132;
    
    // #define _Color_Far_2 float4(0.50353, 0.31069, 0.31797, 1.30           ) //_151._m7
    // #define _151__m26 float4(0.00, 0.00, 0.00, 0.00                  ) // _151._m26
    _132 = lerp(_Color_Far_2.w, _151__m26.w, _if_output_A_0);
    _132 = pow(_switch_value_1 + 1.0e-4, _132);

    // #define _Color_Far float4(0.05891, 0.20904, 0.43325, 0.90         ) // _151._m13
    // #define _151__m21 float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
    float _min_132 = min(min(_132, _Color_Far.w * _151__m21.x), 1.0);
    // _132 = _min_132;
    
    
    // #define _151__m15 float4(0.00335, -0.66724, 0.00042, -0.00671    ) // _151._m15
    float _lookThroughDir3y_length_SO = clamp(_lookThroughWorldPos3.y * _151__m15.x + _151__m15.y, 0.0, 1.0);
    // _139 = _lookThroughDir3y_length_SO;
    
    // y = 0 / 1 \ 0, x ∈ [0, 2]
    float _lookThroughDir3y_length_SO_curve = _lookThroughDir3y_length_SO * (-_lookThroughDir3y_length_SO + 2.0);

    // #define _151__m14 float4(0.27672, 0.01464, -0.23447, 0.00        ) // _151._m14
    // #define _Color_Far float4(0.05891, 0.20904, 0.43325, 0.90         ) // _151._m13
    float3 _color_77_0 = _lookThroughDir3y_length_SO_curve * _151__m14.xyz + _Color_Far.xyz;
    float3 _color_77_1 = lerp(_color_77_0, _151__m26.xyz, _if_output_A_0);
    
    // #define _151__m12 float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
    // #define _151__m16 float4(0.39681, 0.34829, 0.44667, 0.00017      ) // _151._m16
    float _lookThroughDir3_length_OS = clamp((_lookThroughDir3_length - _151__m12.w) * _151__m16.w, 0.0, 1.0);

    float3 _color_77_2 = lerp(_color_77_1, _151__m16.xyz, _lookThroughDir3_length_OS);

    // #define _151__m19 float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
    float _lookThroughDir3xz_length_SO = clamp(_lookThroughDir3xz_length * _151__m19.z + _151__m19.w, 0.0, 1.0);
    
    // #define _151__m11 float4(0.045, 0.00214, 0.00, 0.00              ) // _151._m11
    // #define _151__m20 float4(0.9716, -0.02881, 1.00, 0.00            ) // _151._m20
    float2 _exponent_81 = min(((-_151__m11.xz) * _lookThroughDir3.y) + _151__m20.yw, 80.0) * 1.44269502162933349609375;

    float2 _pow_81 = exp2(_exponent_81);

    float2 _div_80 = (_151__m20.xz - _pow_81) / (_lookThroughDir3.y * _151__m11.xz);
    
    float2 _switch_80;
        _switch_80.x = abs(_lookThroughDir3.y * _151__m11.x) > 0.01 ? _div_80.x : _151__m20.x;
        _switch_80.y = abs(_lookThroughDir3.y * _151__m11.z) > 0.01 ? _div_80.y : _151__m20.z;

    // #define _151__m11 float4(0.045, 0.00214, 0.00, 0.00              ) // _151._m11
    // #define _Color_C float4(1.00, 1.00, 1.00, 0.07213               ) // _151._m23
    float _lerp_140 = lerp(_151__m11.y, _Color_C.w, _if_output_A_1);
    
    float _tmp_114;
    {
        _tmp_114 = _lookThroughDir3_length * _lerp_140;
        _tmp_114 = _tmp_114 * (-_switch_80.x);
        _tmp_114 = exp2(_tmp_114);
        _tmp_114 = (-_tmp_114) + 1.0;
        _tmp_114 = max(_tmp_114, 0.0);
    }

    // #define _151__m12 float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
    float _lookThroughDir3_length_SO_1 = clamp(_lookThroughDir3_length * _151__m12.x + _151__m12.y, 0, 1);
    
    // static const float4 _151__m24 = float4(1.00, -1.00, 10000.00, 0.00             ); // _151._m24
    float _lookThroughDir3_length_SO_2 = clamp(_lookThroughDir3_length * _151__m24.x + _151__m24.y, 0, 1);
    

    float _lerp_127_1 = lerp(_lookThroughDir3_length_SO_1, _lookThroughDir3_length_SO_2, _if_output_A_1);
    
    // #define _151__m12 float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
    float _lerp_80 = lerp(_151__m12.z, _151__m25.x, _if_output_A_1);

    // y = 1 \ -1 / 1, x ∈ [-1, 1]
    float _lerp_127_1_curve = (_lerp_127_1 * (2.0-_lerp_127_1)) + (-1.0);
    
    float _min_114 = min(_Color_Far.w, _tmp_114 * (_lerp_80 * _lerp_127_1_curve + 1.0));
    

    float _max_80 = max(0.0, 1.0-exp2(-_lookThroughDir3_length * _151__m11.w * _switch_80.y));
    
    
    // #define _151__m19 float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
    float _lookThroughDir3_length_SO = clamp(_lookThroughDir3_length * _151__m19.x + _151__m19.y, 0.0, 1.0);

    // #define _151__m21 float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
    float _min_138 = min((2.0-_lookThroughDir3_length_SO) * _lookThroughDir3_length_SO * _max_80, _151__m21.y);


    float _vec2_76_x = _switch_value_2 * _min_114;
    float _vec2_76_y = _lookThroughDir3xz_length_SO * _min_138;

    float3 _color_81 = lerp(_Color_Far_2.xyz, _Color_C.xyz, _if_output_A_1);

    float3 _color_76 = _Color_D.xyz * _vec2_76_y + _vec2_76_x * _color_81  + (1 - _vec2_76_x) * (_min_132 * _color_77_2);

    float3 _grabTextureSample_Mod = max((_grabTextureSample + -_color_76)/max((1-_vec2_76_y) * (1-_vec2_76_x) * (1-_min_132), 1e-4), (0.0));

    
    // #define _GrabTextureFade 0.00      // _151._m59
    float3 _grabTextureColor = lerp(_grabTextureSample, _grabTextureSample_Mod, _GrabTextureFade);
    // _100 = _grabTextureColor;

    
    float _terrainMoreEyeDepth4 = _depthTextureEyeDepth3 - i.Varying_NonStereoScreenPos.w;

    float _viewDir_length = length(_viewDir);

    // > 100 后，100~150 最小值从 0变成10
    float _min_moreEyeDepth4 = clamp(_viewDir_length * 0.2 - 20.0, 0.0, 10.0);
    float _terrainMoreEyeDepth4_amend = max(_terrainMoreEyeDepth4, _min_moreEyeDepth4);

    
    // #define _151__m9  float3(0.13963, 0.31927, 0.93732              ) //_151._m9
    // #define _151__m10 float3(0.05565, -0.29114, -0.95506            ) //_151._m10
    // #define _151__m49 float4(0.00, 0.00, 0.00, 0.00) // _151._m49
    float3 _lightDirOrUkDir = _151__m49.x == 0.0 ? (_151__m10.y < 0.0 ? _151__m9 : _151__m10) : _lightDir1;

    float3 _surfNormal_moreUp_normalize = normalize(float3( _surfNormal.x, 1.0, _surfNormal.z ));

    float3 _H = normalize(_viewDirNormalize + _lightDirOrUkDir);

    float _fixNDotH_clamp01 = clamp(dot(_surfNormal_moreUp_normalize, _H), 0.0, 1.0);
    
    // #define _FixNDotH_Power 332.79999 // _151._m63
    float _fixNDotH_pow = SphericalGaussianPow( _fixNDotH_clamp01, _FixNDotH_Power );
    
    
    // #define _GlossPosAdjust 2.38      // _151._m65
    float _gloss_factor1_maybe = max(_GlossPosAdjust * (-_viewDirNormalize.y) + 1.0, 0.05) * max(_GlossPosAdjust * _lightDirOrUkDir.y - 1.0, 0.05) * _fixNDotH_pow;

    // #define _WaterSurfAlpha 0.40      // _151._m64
    float _gloss_factor2 = clamp(lerp(-0.1, 0, _terrainMoreEyeDepth4_amend) * _WaterSurfAlpha, 0.0, 1.0) * _gloss_factor1_maybe;

    float3 _causticGlossColor = _glossColor1 * _GlossFactor;
    
    float _screenSpaceShadow = tex2D(_ScreenMaskMap, _screenPos).x;

    float _shadowAtten = (_EnableShadow == 1.0) ? _screenSpaceShadow : 1.0;
    
    float3 _causticPos3DInput;
        _causticPos3DInput.xy = (_Time.y * _CausticSpeed * float2(_WorldPosXY_Speed1X, _WorldPosXY_Speed1Y) * 25.0) + _lookThroughAtTerrainWorldPos.xz * _CausticScale + _terrainToSurfLength * _CausticNormalDisturbance * _surfNormal.xz;
        _causticPos3DInput.z  = _Time.y * _CausticSpeed;
    
    float _causticNoise3DResult = GenshipCaustic(_causticPos3DInput);

    
    float _causticVisibleHeightFactor = clamp(_terrainToSurfDir.y * _CausticVisibleHeightFactor, 0.0, 1.0);
    // _95 = _causticVisibleHeightFactor;


    float _lookAtTerrain_length_adjust_clamp01 = clamp(length(_lookThroughAtTerrainDir) * _CausticDistanceFade, 0, 1);
    float _causticVisibleDistanceFactor = 1-_lookAtTerrain_length_adjust_clamp01;

    float _causticVisibleFactor = _causticVisibleHeightFactor * _causticVisibleDistanceFactor;

    
    
    float3 _causticColor = _causticNoise3DResult * _GlossColor.xyz * _CausticColor.xyz * _causticVisibleFactor * _shadowAtten;

    float _causticColorDisappear = pow(clamp(_terrainToSurfLength * _CausticColorDisappearOfWaterDepth, 0.0, 1.0) + 1e-4,  _CausticColorDisappearPower);
    
    float3 _transmissionCausticColor = _causticColor * (1-_causticColorDisappear);

    float3 _transmissionColor = _grabTextureColor * (1+_transmissionCausticColor);

    float3 _waterSurfAlpha = min(pow(clamp(_terrainMoreEyeDepth4_amend * _WaterSurfAlpha, 0.0, 1.0) + 1e-4, _WaterSurfAlphaPower), 1.0);

    float _waterBottomDarkFactor = min(pow(clamp(_terrainMoreEyeDepth4_amend * _WaterBottomDarkFactor, 0.0, 1.0) + 1e-4, _WaterBottomDarkPower), 1.0);

    float3 _waterSurfColor0 = lerp(_WaterSurfColor.xyz, _WaterSurfColorBlend.xyz, _WaterSurfColorBlend.w);

    float3 _waterSurfColor = lerp(1.0, _waterSurfColor0, _waterSurfAlpha);

    float3 _transmissionSurfColor = _transmissionColor * _waterSurfColor;

    float3 _waterBottomDarkColor0 = lerp(_WaterBottomDarkColor.xyz, _WaterBottomDarkColorBlend.xyz, _WaterBottomDarkColorBlend.w);

    float3 _waterColor0 = lerp(_transmissionSurfColor, _waterBottomDarkColor0, _waterBottomDarkFactor);

    float3 _if_waterColor = _waterColor0;

    
    // #define _DebugValueMaybe 0.00      // _151._m58
    // #define _151__m50 float4(0.00, 0.00, 0.00, 0.00) // _151._m50
    bool __tmp = 0.0 != _151__m50.x && 0.0 != _DebugValueMaybe;
    if (__tmp)
    {
        // 这部分是不是调试用的？ 暂时不逆向
        
        _57 = max(_waterColor0, (9.9999997473787516355514526367188e-05));
        // _100 = i.Varying_WorldPosXYZ.yyy * _Matrix_custom_V_maybe[1u].xyz;
        // _100 = (_Matrix_custom_V_maybe[0u].xyz * i.Varying_WorldPosXYZ.xxx) + _100;
        // _100 = (_Matrix_custom_V_maybe[2u].xyz * i.Varying_WorldPosXYZ.zzz) + _100;
        // _100 += _Matrix_custom_V_maybe[3u].xyz;

        float3 _customViewPos = mul(_Matrix_custom_V_maybe, float4(i.Varying_WorldPosXYZ.xyz, 1.0)).xyz;
        _100 = _customViewPos;

        
        _77.xy = (i.Varying_WorldPosXYZ.xz * _151__m56.xy) + _151__m56.zw;

        
        _101 = tex3D(_Noise3DMap, _customViewPos).x;

        
        _116 = tex2D(_21_sampler2D, _77.xy).x;
        _100.x = (_101 * _151__m52[1u].z) + _151__m52[1u].w;
        _100.x = clamp(_100.x, 0.0, 1.0);
        _77 = (-i.Varying_WorldPosXYZ.xyz) + _151__m52[0u].xyz;
        _132 = dot(_77, _77);
        _132 = sqrt(_132);
        _132 = (_132 * _151__m52[1u].x) + _151__m52[1u].y;
        _132 = clamp(_132, 0.0, 1.0);
        _100.x = _132 * _100.x;
        _100.x = _116 * _100.x;
        _114 = dot(_57, float3(0.21267290413379669189453125, 0.715152204036712646484375, 0.072175003588199615478515625));
        _77 = _57 + (-(_114));
        _81 = (_151__m53[0u].xyz * _77) + (_114);
        _81 = max(_81, (0.0));
        _81 = log2(_81);
        _85.x = _81.x * _151__m53[0u].w;
        _85.y = _81.y * _151__m53[1u].w;
        _85.z = _81.z * _151__m53[2u].w;
        _81 = exp2(_85);
        _81 = (_81 * _151__m53[1u].xyz) + _151__m53[2u].xyz;
        _134 = 1 < _151__m51;
        if (_134)
        {
            _134 = _100.x < 0.00196078442968428134918212890625;
            if (_134)
            {
                _85 = i.Varying_WorldPosXYZ.yyy * _151__m55[1u].xyz;
                _85 = (_151__m55[0u].xyz * i.Varying_WorldPosXYZ.xxx) + _85;
                _85 = (_151__m55[2u].xyz * i.Varying_WorldPosXYZ.zzz) + _85;
                _85 += _151__m55[3u].xyz;
                _86 = (i.Varying_WorldPosXYZ.xz * _151__m57.xy) + _151__m57.zw;
                _133 = tex3D(_20_sampler3D, _85).x;
                _142 = tex2D(_22_sampler2D, _86).x;
                _132 = (_133 * _151__m52[3u].z) + _151__m52[3u].w;
                _132 = clamp(_132, 0.0, 1.0);
                _85 = (-i.Varying_WorldPosXYZ.xyz) + _151__m52[2u].xyz;
                _143 = dot(_85, _85);
                _143 = sqrt(_143);
                _143 = (_143 * _151__m52[3u].x) + _151__m52[3u].y;
                _143 = clamp(_143, 0.0, 1.0);
                _132 *= _143;
                _100.x = _142 * _132;
                _77 = (_151__m53[3u].xyz * _77) + (_114);
                _77 = max(_77, (0.0));
                _77 = log2(_77);
                _85.x = _77.x * _151__m53[3u].w;
                _85.y = _77.y * _151__m53[4u].w;
                _85.z = _77.z * _151__m53[5u].w;
                _77 = exp2(_85);
                _81 = (_77 * _151__m53[4u].xyz) + _151__m53[5u].xyz;
            }
        }
        _77 = (-_57) + _81;
        _if_waterColor = (_100.xxx * _77) + _57;
    }

    // _51 = _if_waterColor;

    float3 _surfNormal2 = normalize(_surfNormal.xyz * _SurfNormalScale2.xzy);

    float3 _reflectDir = normalize(reflect(-_viewDirNormalize, _surfNormal2));

    
    float4 _unity_SpecCube0Sample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, _reflectDir, 0.0);
    // float4 data = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, _reflectDir, 0.0);
    // float alpha = unity_SpecCube0_HDR.w * (data.a - 1.0) + 1.0;
    // float3 _decodeHdr = unity_SpecCube0_HDR.x * pow(alpha, unity_SpecCube0_HDR.y) * data.xyz;
    float3 _decodeHdr = DecodeHDR(_unity_SpecCube0Sample, unity_SpecCube0_HDR);
    

    float2 _screenReflectUV = (_surfNormal2.xz * clamp(_terrainMoreEyeDepth4_amend * _SSRNormalDisturbance2, 0.0, 1.0) * _SSRNormalDisturbance1) + _screenPos;

    
    float4 _ssrSample = tex2D(_ScreenReflectTexture, _screenReflectUV);
 
    float _ssrAlpha = clamp(_ssrSample.w * _SSREnable * _SSRAlpha, 0.0, 1.0);


    float3 _reflectColor = lerp(_decodeHdr, _ssrSample.xyz, _ssrAlpha);
    
    float _reflectFactor01 = clamp(clamp(_terrainMoreEyeDepth4_amend * _ReflectWaterDepthFactor, 0.0, 1.0) * max(1.0 - _ReflectWaterViewYDisappearFactor * _viewDirNormalize.y, 0.05) * _ReflectFactor * _ReflectEnable, 0.0, 1.0);

    float3 _waterColor_2 = lerp(_if_waterColor, _reflectColor, _waterSurfAlpha * _reflectFactor01);

    float _brightness = max(_lightDir1.y, 0.0) * _shadowAtten;

    float3 _glossColor_2 = _brightness * _glossColor1 + i.Varying_GlossColorAdd.xyz;

    float _foamLineArea_oneMinus = min(i.Varying_ColorXYW.x * _FoamLineAreaSize, max(_terrainToSurfDir.y, 0.0)) / (_FoamLineAreaSize * i.Varying_ColorXYW.x + 1e-4);
    float _foamLineArea = 1 - _foamLineArea_oneMinus;
    // _122 = _foamLineArea;

    float2 _noise2D_R_UV = (_Noise2D_R_ScaleSpeed.xy * _worldPosXZ1) + frac(_Time.y * _Noise2D_R_ScaleSpeed.zw);
    // _47.xy = _noise2D_R_UV;
    float _noise2D_R_Sample = tex2D(_Noise2D_R, _noise2D_R_UV).x;
    // _49 = _noise2D_R_Sample;
    // _92 = _worldPosXZ1.y + _worldPosXZ1.x;
    // _92 *= _FoamLinePosSpeed;
    // _92 = (_FoamLineSpeed * _Time.y) + (_worldPosXZ1.y + _worldPosXZ1.x) * _FoamLinePosSpeed;
    // _108.x = (-_122) + 1.0;
    // _108.x = _foamLineArea_oneMinus;
    // _103.x = (_FoamLineSinFrequency * _foamLineArea) + (_FoamLineSpeed * _Time.y) + (_worldPosXZ1.y + _worldPosXZ1.x) * _FoamLinePosSpeed;
    // _103.x = sin(_103.x);
    float _foamLine0 = sin((_FoamLineSinFrequency * _foamLineArea) + (_FoamLineSpeed * _Time.y) + (_worldPosXZ1.y + _worldPosXZ1.x) * _FoamLinePosSpeed);
    // _103.x = _foamLine0;
    float _foamLine_add_noise_n1_1 = (_FoamLineAreaBaseMulti * _foamLineArea) + _foamLine0 + (_noise2D_R_Sample * 2.0) + (-1.0);
    // _103.x = _foamLine_add_noise_n1_1;
    // _93 = (_noise2D_R_Sample * 2.0) + (-1.0);
    // _92 = _foamLine_add_noise_n1_1 + (_noise2D_R_Sample * 2.0) + (-1.0);
    // _92 = _foamLine_add_noise_n1_1;
    // _94 = _foamLine_add_noise_n1_1 >= _foamLineArea_oneMinus;
    // _92 = float(_foamLine_add_noise_n1_1 >= _foamLineArea_oneMinus);
    float _foamLineNoise = _noise2D_R_Sample * float(_foamLine_add_noise_n1_1 >= _foamLineArea_oneMinus);
    // _47.x = _foamLineNoise;
    
    // _103.x = _FoamLineFadeDistance + 1e-4;
    // _103.x = _viewDir_length / (_FoamLineFadeDistance + 1e-4);
    // _103.x = clamp(_viewDir_length / (_FoamLineFadeDistance + 1e-4), 0.0, 1.0);
    float _tmp_124 = 1.0 - clamp(_viewDir_length / (_FoamLineFadeDistance + 1e-4), 0.0, 1.0);
    // _124 = _tmp_124;
    
    // _103.x = i.Varying_ColorXYW.y * (1-min(_viewDir_length * 0.01, 1.0)) * _FoamColor.w;
    // _76.x = (_viewDir_length - _FoamLineVisibleDistance) / _FoamLineFadeDiv;
    float _tmp_76 =  i.Varying_ColorXYW.y * (1-min(_viewDir_length * 0.01, 1.0)) * _FoamColor.w * (_viewDir_length - _FoamLineVisibleDistance) / _FoamLineFadeDiv * _foamLineNoise * _foamLineArea;
    // _76.x = _tmp_76;
    
    // _76.x = _foamLineNoise * _76.x;
    // _76.x = _foamLineArea * _76.x;
    
    // _76.x = _tmp_124 * _tmp_76;
    float _foamLine_2 = clamp(_tmp_124 * _tmp_76, 0.0, 1.0);
    // float _tmp_76_1 = _foamLine_2;
    // _76.x = _tmp_76_1;
    
    // _103 = (_FoamColor.xyz * _glossColor_2) + (-_waterColor_2);
    // _76 = (_tmp_76_1 * ((_FoamColor.xyz * _glossColor_2) + (-_waterColor_2))) + _waterColor_2;
    float3 _waterColor_3 = lerp(_waterColor_2, _FoamColor.xyz * _glossColor_2, _foamLine_2);
    // _76 = _waterColor_3;
    float3 _waterColor_4 = _causticGlossColor * _gloss_factor2 + _waterColor_3;
    float3 _if_waterColor_5 = _waterColor_4;
   
    // _35 = _terrainMoreEyeDepth4_amend * _WaterAlpha;
    // _35 *= i.Varying_ColorXYW.w;
    float _output_alpha = clamp(_terrainMoreEyeDepth4_amend * _WaterAlpha, 0.0, 1.0) * i.Varying_ColorXYW.w;
    // _35 = _output_alpha;
    // _43 = _DebugValueMaybe == 1.0;
    bool __tmp2 = _DebugValueMaybe == 1.0;
    if (__tmp2)
    {
        _41 = i.Varying_WorldPosXYZ.yyy * _Matrix_custom_V_maybe[1u].xyz;
        _41 = (_Matrix_custom_V_maybe[0u].xyz * i.Varying_WorldPosXYZ.xxx) + _41;
        _41 = (_Matrix_custom_V_maybe[2u].xyz * i.Varying_WorldPosXYZ.zzz) + _41;
        _41 += _Matrix_custom_V_maybe[3u].xyz;
        float2 _2560 = (i.Varying_WorldPosXYZ.xz * _151__m56.xy) + _151__m56.zw;
        _47 = float3(_2560.x, _2560.y, _47.z);
        _42 = tex3D(_Noise3DMap, _41).x;
        _90 = tex2D(_21_sampler2D, _47.xy).x;
        _41.x = (_42 * _151__m52[1u].z) + _151__m52[1u].w;
        _41.x = clamp(_41.x, 0.0, 1.0);
        _47 = (-i.Varying_WorldPosXYZ.xyz) + _151__m52[0u].xyz;
        _105.x = dot(_47, _47);
        _105.x = sqrt(_105.x);
        _105.x = (_105.x * _151__m52[1u].x) + _151__m52[1u].y;
        _105.x = clamp(_105.x, 0.0, 1.0);
        _41.x = _105.x * _41.x;
        _41.x = _90 * _41.x;
        _91 = 1 < _151__m51;
        if (_91)
        {
            _107 = _41.x < 0.00196078442968428134918212890625;
            if (_107)
            {
                _47 = i.Varying_WorldPosXYZ.yyy * _151__m55[1u].xyz;
                _47 = (_151__m55[0u].xyz * i.Varying_WorldPosXYZ.xxx) + _47;
                _47 = (_151__m55[2u].xyz * i.Varying_WorldPosXYZ.zzz) + _47;
                _47 += _151__m55[3u].xyz;
                _105 = (i.Varying_WorldPosXYZ.xz * _151__m57.xy) + _151__m57.zw;
                _49 = tex3D(_20_sampler3D, _47).x;
                _106 = tex2D(_22_sampler2D, _105).x;
                _123 = (_49 * _151__m52[3u].z) + _151__m52[3u].w;
                _123 = clamp(_123, 0.0, 1.0);
                _47 = (-i.Varying_WorldPosXYZ.xyz) + _151__m52[2u].xyz;
                _47.x = dot(_47, _47);
                _47.x = sqrt(_47.x);
                _47.x = (_47.x * _151__m52[3u].x) + _151__m52[3u].y;
                _47.x = clamp(_47.x, 0.0, 1.0);
                _123 *= _47.x;
                _105.x = _106 * _123;
            }
            else
            {
                _105.x = _41.x;
            }
        }
        else
        {
            _105.x = _41.x;
        }
        // _47 = (_waterColor_4 * _151__m88.xyz) + (-_waterColor_4);
        // _if_waterColor_5 = (_105.xxx * ((_waterColor_4 * _151__m88.xyz) + (-_waterColor_4))) + _waterColor_4;
        _if_waterColor_5 = lerp(_waterColor_4, _waterColor_4 * _151__m88.xyz, _105.x);
        if (_91)
        {
            _91 = _41.x < 0.00196078442968428134918212890625;
            if (_91)
            {
                _89 = i.Varying_WorldPosXYZ.yyy * _151__m55[1u].xyz;
                _89 = (_151__m55[0u].xyz * i.Varying_WorldPosXYZ.xxx) + _89;
                _89 = (_151__m55[2u].xyz * i.Varying_WorldPosXYZ.zzz) + _89;
                _89 += _151__m55[3u].xyz;
                float2 _2793 = (i.Varying_WorldPosXYZ.xz * _151__m57.xy) + _151__m57.zw;
                _47 = float3(_2793.x, _2793.y, _47.z);
                _90 = tex3D(_20_sampler3D, _89).x;
                _106 = tex2D(_22_sampler2D, _47.xy).x;
                _89.x = (_90 * _151__m52[3u].z) + _151__m52[3u].w;
                _89.x = clamp(_89.x, 0.0, 1.0);
                _47 = (-i.Varying_WorldPosXYZ.xyz) + _151__m52[2u].xyz;
                _123 = dot(_47, _47);
                _123 = sqrt(_123);
                _123 = (_123 * _151__m52[3u].x) + _151__m52[3u].y;
                _123 = clamp(_123, 0.0, 1.0);
                _89.x = _123 * _89.x;
                _41.x = _106 * _89.x;
            }
        }
        // _89.x = (_35 * _151__m89) + (-_35);
        // Output_0.w = (_41.x * ((_35 * _151__m89) + (-_35))) + _35;
        Output_0.w = lerp(_output_alpha, _output_alpha * _151__m89, _41.x);
    }
    else
    {
        Output_0.w = _output_alpha;
    }

    // _87 = _if_waterColor_5;
    
    // float3 _lookAtDir = i.Varying_WorldPosXYZ.xyz - _WorldSpaceCameraPos;
    // _41 = _lookAtDir;

    float _if_output_B_0 = 0;
    float _if_output_B_1 = 0;
    
    if (0.01 < _WorldPosXY_Offset_Negative.w)
    {
        _125 = _151__m27.y < 0.5;
        if (_125)
        {
            _47 = i.Varying_WorldPosXYZ.xyz + (-_WorldPosXY_Offset_Negative.xyz);
            _123 = dot(_47, _47);
            _123 = sqrt(_123);
            _123 = (_123 * _151__m24.z) + _151__m24.w;
            _123 = clamp(_123, 0.0, 1.0);
            _123 = (-_123) + 1.0;
            _76.x = _123 * _123;
        }
        else
        {
            _123 = i.Varying_WorldPosXYZ.y + (-_WorldPosXY_Offset_Negative.y);
            _47.x = 1.0 / _WorldPosXY_Offset_Negative.w;
            _123 *= _47.x;
            _123 = clamp(_123, 0.0, 1.0);
            _47.x = (_123 * (-2.0)) + 3.0;
            _123 *= _123;
            _75 = _123 * _47.x;
            _76.x = _75;
        }
        _125 = _151__m27.x >= 0.05;
        _123 = float(_125);
        _123 *= _76.x;
        _50 = 0.95 >= _151__m27.x;
        _47.x = float(_50);
        _47.x *= _76.x;
        _if_output_B_0 = _123;
        _if_output_B_1 = _47.x;
    }

    // _124 = _if_output_B_0;
    // _48  = _if_output_B_1;

    
    // _51.x = dot(_lookAtDir, _lookAtDir);
    // _51.x = sqrt(_51.x);
    float _lookAtDir_length = length(_lookAtDir);
    // _51.x = _lookAtDir_length;
    // _95 = (_lookAtDir_length * _151__m15.z) + _151__m15.w;
    float _lookAtDir_length_SO_1 = clamp(_lookAtDir_length * _151__m15.z + _151__m15.w, 0.0, 1.0);
    // _95 = _lookAtDir_length_SO_1;
    // _109.x = (_lookAtDir_length * _151__m25.z) + _151__m25.w;
    float _lookAtDir_length_S0_2 = clamp(_lookAtDir_length * _151__m25.z + _151__m25.w, 0.0, 1.0);
    // _109.x = _lookAtDir_length_S0_2;
    // _76.x = (-_lookAtDir_length_SO_1) + _lookAtDir_length_S0_2;
    // _76.x = (_if_output_B_0 * ((-_lookAtDir_length_SO_1) + _lookAtDir_length_S0_2)) + _lookAtDir_length_SO_1;
    float _lookAtDir_lenght_SO = lerp(_lookAtDir_length_SO_1, _lookAtDir_length_S0_2, _if_output_B_0);
    // _76.x = _lookAtDir_lenght_SO;
    // _103.x = (-_lookAtDir_lenght_SO) + 2.0;

    // y = 0 / 1, x ∈ [0, 1]
    float _lookAtDir_lenght_SO_curve = ((-_lookAtDir_lenght_SO) + 2.0) * _lookAtDir_lenght_SO;
    // _76.x = _lookAtDir_lenght_SO_curve;
    // _95 = dot(_lookAtDir.xz, _lookAtDir.xz);
    // _95 = sqrt(_95);
    float _lookAtDirXZ_length = length(_lookAtDir.xz);
    // _95 = _lookAtDirXZ_length;
    // _109.x = (_lookAtDirXZ_length * _151__m17.x) + _151__m17.y;
    float _lookAtDirXZ_length_SO_1 = clamp(_lookAtDirXZ_length * _151__m17.x + _151__m17.y, 0.0, 1.0);
    // _109.x = _lookAtDirXZ_length_SO_1;
    // _128 = _lookAtDir_lenght_SO_curve * _151__m14.w;
    float _surfEyeDepth2 = -dot(_lookAtDir, _back);
    // -_35 = _surfEyeDepth2;
    float _switch_value_3 = _surfEyeDepth2 >= _far_plane ? _lookAtDir_lenght_SO_curve * _151__m14.w : _lookAtDir_lenght_SO_curve;
    float _switch_value_4 = _surfEyeDepth2 >= _far_plane ? _74 : _lookAtDirXZ_length_SO_1;
    // _76.x = _switch_value_3;
    // _103.x = _switch_value_4;
    
    // _35 = (_if_output_B_0 * (_151__m26.w - _Color_Far_2.w)) + _Color_Far_2.w;
    float _tmp_35 = lerp(_Color_Far_2.w, _151__m26.w, _if_output_B_0);
    // _35 = _tmp_35;
    // _110 = _switch_value_3 + 1e-4;
    // _110 = log2(_switch_value_3 + 1e-4);
    // _35 = log2(_switch_value_3 + 1e-4) * _tmp_35;
    // _35 = exp2(log2(_switch_value_3 + 1e-4) * _tmp_35);
    float _swtich_value_3_pow = pow(_switch_value_3 + 1e-4, _tmp_35);
    // _35 = _swtich_value_3_pow;

    // _35 = min(_Color_Far.w * _151__m21.x, _swtich_value_3_pow);
    
    // #define _151__m21 float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
    float _tmp_35_1 = min(1.0, min(_Color_Far.w * _151__m21.x, _swtich_value_3_pow));
    // _35 = _tmp_35_1;
    // _109.x = (i.Varying_WorldPosXYZ.y * _151__m15.x) + _151__m15.y;
    float _worldPosY_SO = clamp(i.Varying_WorldPosXYZ.y * _151__m15.x + _151__m15.y, 0.0, 1.0);
    // _109.x = _worldPosY_SO;
    // _76.x = (-_worldPosY_SO) + 2.0;
    
    // y = 0 / 1, x ∈ [0, 1]
    float _worldPosY_SO_curve = (-_worldPosY_SO + 2.0) * _worldPosY_SO;
    // _76.x = _worldPosY_SO_curve;
    float3 _color_57 = (_worldPosY_SO_curve * _151__m14.xyz) + _Color_Far.xyz;
    // _57 = _color_57;
    // float3 _3124 = (-_color_57) + _151__m26.xyz;
    // _64.xyz = (-_color_57) + _151__m26.xyz;
    // _57 = (_if_output_B_0.xxx * ((-_color_57) + _151__m26.xyz)) + _color_57;
    float3 _color_57_1 = lerp(_color_57, _151__m26.xyz, _if_output_B_0.x);
    // _57 = _color_57_1;
    // _109.x = _lookAtDir_length + (-_151__m12.w);
    // _109.x = (_lookAtDir_length - _151__m12.w) * _151__m16.w;
    float _lookAtDir_length_OS = clamp((_lookAtDir_length - _151__m12.w) * _151__m16.w, 0.0, 1.0);
    // _109.x = _lookAtDir_length_OS;
    // float3 _3156 = (-_color_57_1) + _151__m16.xyz;
    // _64.xyz = _151__m16.xyz - _color_57_1;
    // _57 = (_lookAtDir_length_OS * (_151__m16.xyz - _color_57_1)) + _color_57_1;
    float3 _color_57_2 = lerp(_color_57_1, _151__m16.xyz, _lookAtDir_length_OS);
    // _57 = _color_57_2;
    // _95 = (_lookAtDirXZ_length * _151__m19.z) + _151__m19.w;
    float _lookAtDirXZ_length_SO = clamp(_lookAtDirXZ_length * _151__m19.z + _151__m19.w, 0.0, 1.0);
    // _95 = _lookAtDirXZ_length_SO;
    // _109.x = (_if_output_B_1 * (-_151__m11.y + _Color_C.w)) + _151__m11.y;
    float _lerp_109 = lerp(_151__m11.y, _Color_C.w, _if_output_B_1);
    // _109.x = _lerp_109;
    // float2 _3187 = _lookAtDir.y * _151__m11.xz;
    float2 _tmp_64 = _lookAtDir.y * _151__m11.xz;
    // _64.xy = _tmp_64;
    // _113 = (0.01 < abs(_64.xyxy)).xy;
    // _113.x = abs(_tmp_64.x) > 0.01;
    // _113.y = abs(_tmp_64.y) > 0.01;
    // float2 _3205 = ((-_151__m11.xz) * _lookAtDir.y) + _151__m20.yw;
    // _66.xy = (-_151__m11.xz) * _lookAtDir.y + _151__m20.yw;
    // float2 _3210 = min((-_151__m11.xz) * _lookAtDir.y + _151__m20.yw, (80.0));
    // _66.xy = min((-_151__m11.xz) * _lookAtDir.y + _151__m20.yw, 80.0);
    // float2 _3215 = _66.xy * 1.4427;
    // _66.xy = _66.xy * 1.4427;
    // float2 _3220 = exp2(_66.xy * 1.4427);
    // _66.xy = exp2(_66.xy * 1.4427);

    // exp(a) = exp2(a/ln(2))
    // 1 / ln(2) ≈ 1.4427f
    // _66.xy = exp2(_66.xy * 1/log(2));
    _66.xy = exp(min((-_151__m11.xz) * _lookAtDir.y + _151__m20.yw, 80.0));
    // float2 _3229 = (-_66.xy) + _151__m20.xz;
    _66.xy = (-_66.xy) + _151__m20.xz;;
    // float2 _3236 = _66.xy / _tmp_64.xy;
    // _64.xy = _66.xy / _tmp_64.xy;
    _64.x = abs(_tmp_64.x) > 0.01 ? _66.x / _tmp_64.x : _151__m20.x;
    _64.y = abs(_tmp_64.y) > 0.01 ? _66.y / _tmp_64.y : _151__m20.z;
    
    _109.x = _lerp_109 * _lookAtDir_length;
    _109.x *= (-_64.x);
    _109.x = exp2(_109.x);
    _109.x = (-_109.x) + 1.0;
    _109.x = max(_109.x, 0.0);
    _128 = (_lookAtDir_length * _151__m12.x) + _151__m12.y;
    _128 = clamp(_128, 0.0, 1.0);
    _129 = (_lookAtDir_length * _151__m24.x) + _151__m24.y;
    _129 = clamp(_129, 0.0, 1.0);
    _76.x = (-_128) + _129;
    _76.x = (_if_output_B_1 * _76.x) + _128;
    _119 = (-_76.x) + 2.0;
    _128 = (_if_output_B_1 * ((-_151__m12.z) + _151__m25.x)) + _151__m12.z;
    _130 = (_76.x * _119) + (-1.0);
    _128 = (_128 * _130) + 1.0;
    _76.x = _128 * _109.x;
    _109.x = min(_76.x, _Color_Far.w);
    _128 = _lookAtDir_length * _151__m11.w;
    _128 *= (-_64.y);
    _128 = exp2(_128);
    _128 = (-_128) + 1.0;
    _128 = max(_128, 0.0);
    _51.x = (_lookAtDir_length * _151__m19.x) + _151__m19.y;
    _51.x = clamp(_51.x, 0.0, 1.0);
    _76.x = (-_51.x) + 2.0;
    _76.x = _51.x * _76.x;
    _76.x = _128 * _76.x;
    _51.x = min(_76.x, _151__m21.y);
    _76.x = _switch_value_4 * _109.x;
    _76.y = _lookAtDirXZ_length_SO * _51.x;
    // _51 = ((_if_output_B_1) * (_Color_C.xyz -_Color_Far_2.xyz)) + _Color_Far_2.xyz;
    _51 = lerp(_Color_Far_2.xyz, _Color_C.xyz, _if_output_B_1);
    _83 = (_tmp_35_1) * _color_57_2;
    _84 = ((-_color_57_2) * (_tmp_35_1)) + _51;
    _83 = (_76.xxx * _84) + _83;
    _35 = (-_tmp_35_1) + 1.0;
    _54 = (-_76.xy) + (1.0);
    _35 *= _54.x;
    _76 = (_Color_D.xyz * _76.yyy) + _83;
    _35 = _54.y * _35;
    // float3 _3459 = ((_35) * _if_waterColor_5) + _76;
    // Output_0 = float4(_3459.x, _3459.y, _3459.z, Output_0.w);
    Output_0.xyz = ((_35) * _if_waterColor_5) + _76;
    
    fixed4 col = fixed4(0,0,0,1);
    col = Output_0;
    // 直接显示 _causticNoise3DResult 结果
    // col = float4(_causticNoise3DResult, _causticNoise3DResult, _causticNoise3DResult, 1);
    // col = float4(_causticColor, 1);
    // col = float4(_glossColorVertex, 1);
    // col = float4(_glossColor_2, 1);
    // col = float4(i.Varying_ColorXYW.xyw, 1);
    // col = float4(frac(_noise2D_R_UV), 0, 1);
    // col = float4(_noise2D_R_Sample.xxx, 1);
    // col = float4(_foamLine0.xxx, 1);
    // col = float4(_foamLineArea_oneMinus.xxx, 1);
    // col = float4(_foamLineArea.xxx * _foamLine0, 1);
    // col = float4(_tmp_76.xxx, 1);
    // col = float4(_tmp_76_1.xxx, 1);
    // col = float4(_tmp_35.xxx, 1);
    // col = float4(_lookAtDir_length_OS.xxx, 1);
    return col;
}
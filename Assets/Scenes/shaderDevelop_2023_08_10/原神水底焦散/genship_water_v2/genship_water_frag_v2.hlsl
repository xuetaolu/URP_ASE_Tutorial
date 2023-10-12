#include "genship_water_common_v2.hlsl"

//---- 光源方向，光源颜色
// 主灯光位置/朝向，即阴影计算的灯光方向
// #define _LightDir float4(0.12266, 0.55406, 0.82339, 0.00           )//_151._m5
#define _LightDir _MainLightPosition // float4(_MainLightPosition.xyz, 0)
// 这两个估计是照明计算的太阳 or 月亮方向
// 现直接用 _MainLightPosition 代替
// #define _PointLightDir1  float3(0.13963, 0.31927, 0.93732              ) //_151._m9
// #define _PointLightDir2 float3(0.05565, -0.29114, -0.95506            ) //_151._m10
#define _PointLightDir1 (_MainLightPosition.xyz)
#define _PointLightDir2 float3(_MainLightPosition.x, -_MainLightPosition.y, _MainLightPosition.z)

// float4 _LightColor = float4(2.92204, 1.56181, 0.57585, 1.62808        ); //_151._m8
#define _LightColor _MainLightColor

float _LightIntensity = 5.00;      // _151._m62
float _NDotHPower = 332.79999; // _151._m63
float _GlossFactorLookAtHorizontallyAndLightHight = 2.38;      // _151._m65

#define _UseMainLightPosAsSunPoint float4(0.00, 0.00, 0.00, 0.00) // _151._m49
//------------


//---- 水属性
float _WaterAlpha = 2.5641;    // _151._m61
float _WaterSmoothness = 0.40;      // _151._m64

// 水 Color 颜色
float4 _ColorBase = float4(0.05891, 0.20904, 0.43325, 0.90         ); // _151._m13
float4 _ColorHeightAdd = float4(0.27672, 0.01464, -0.23447, 0.00        ); // _151._m14
float4 _WaterSurfColor = float4(0.11131, 1.00, 0.9415, 0.00      ); // _151._m66
float4 _WaterSurfColorBlend = float4(1.00, 1.00, 1.00, 0.00); // _151._m80
float4 _WaterBottomDarkColor = float4(0.01694, 0.1433, 0.26481, 0.00   ); // _151._m67
float4 _WaterBottomDarkColorBlend = float4(1.00, 1.00, 1.00, 0.00); // _151._m81
float _WaterBottomDarkPower = 1.00;     // _151._m69
float _WaterBottomDarkFactor = 0.06667;  // _151._m70
//--------------


//---- 双层法线扰动
float4 _WorldPosXY_Offset = float4(1934.36584, 0.00, -1266.34216, 0.00       ); //_151._m4
float _WorldPosXY_Scale = 0.05; // _151._m83

float _WorldPosXY_Speed1X = -0.02;  // _151._m44
float _WorldPosXY_Speed1Y = -0.01;  // _151._m45
float _WorldPosXY_Speed2X =  0.05;   // _151._m46
float _WorldPosXY_Speed2Y = -0.04;  // _151._m47

float _NormalMapScale = 0.70;   // _151._m48
float _SurfNormalScale = 0.15;      // _151._m60
//-------------


//---- Caustic 水底焦散
float _CausticScale = 0.25;  // _151._m28
float _CausticSpeed = 0.131; // _151._m29
float4 _CausticColor = float4(0.60632, 0.5298, 0.44146, 1.00); // _151._m30
float _CausticNormalDisturbance = 0.096;   // _151._m33

// 焦散可见参数
float _CausticVisibleHeight = 3.33333; // _151._m31
float _CausticVisibleDistance = 0.01667; // _151._m32
float _CausticVisibleWaterDepth = 0.87; // _151._m84
float _CausticVisiblePower = 2.49; // _151._m85
//--------------------


//---- FoamLine 岸边浮沫线
float4 _Noise2D_R_ScaleSpeed = float4(0.20, 0.15, 0.01, 0.01); // _151._m34
float _FoamLineSpeed = -1.28;                  // _151._m35
float4 _FoamColor = float4(1.00, 1.00, 1.00, 1.00); // _151._m36
float _FoamLineAreaSize = 0.30;   // _151._m37
float _FoamLineFadeDistance = 205.00; // _151._m38
float _FoamLineSinFrequency = 19.00;  // _151._m39
#define _FoamLineAreaBaseMulti 0.30   // _151._m40
#define _FoamLinePosSpeed 0.00   // _151._m41
float _FoamLineVisibleDistance = 10.00;  // _151._m42
float _FoamLineFadeDiv = 20.00;  // _151._m43
//-----------------------


//---- 反射，和 SSR
float _ReflectFactor = 0.50;    // _151._m77
float _WaterReflectPower = 1.50;     // _151._m68
float4 _SurfNormalReflectScale = float4(0.10238, 0.09815, 0.59876, 0.00); // _151._m72
float _SSRNormalDisturbance = 0.60;    // _151._m73
float _SSRNormalDisturbanceWaterDepthRelevant = 1.51515; // _151._m74
float _SSRAlpha = 0.80;    // _151._m75
float _ReflectWaterViewYDisappearFactor = 0.93;    // _151._m78
float _ReflectWaterDepthFactor = 0.38462; // _151._m79
//---------------


//---- 远处雾化，w 越大则越远才会雾化
// float4 _COLOR_FAR_FOG = float4(0.50353, 0.31069, 0.31797, 1.30           ); //_151._m7
float3 _ColorFarFog; float _ColorFarFogW;
#define _COLOR_FAR_FOG float4(_ColorFarFog.xyz, _ColorFarFogW)
float4 _ColorVeryFar = float4(0.39681, 0.34829, 0.44667, 0.00017      ); // _151._m16
float4 _ColorFarExp = float4(1.00, 1.00, 1.00, 16.00                 ); // _151._m18

// Exp 衰减，雾效
float4 _ExpDampingScaleXYZ = float4(0.045, 0.00214, 0.00, 0.00              ); // _151._m11
float4 _ExpDampingStartXZ = float4(0.9716, -0.02881, 1.00, 0.00            ); // _151._m20

// 一些依据距离做 ST 的参数
#define _STArgs_BaseColorXY_And__ float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
#define _STArgs_BaseColorXY_VeryFarZW float4(0.00335, -0.66724, 0.00042, -0.00671    ) // _151._m15
#define _STArgs_CameraY_ZW_DistanceXY float4(-0.001, 9.00, -0.001, 1.19927           ) // _151._m17
#define _STArgs_ExpXY_ZW float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
#define _FarExpMaxX_VeryFarExpMaxY float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
//------------


//// -- misc
float _GrabTextureFade = 0.00;      // _151._m59

float _SSREnable = 1.00;    // _151._m76
#define _EyeDepthBias 0.00 // _151._m87
#define _ReflectEnable 1.00 // _151._m82
#define _EnableShadow 1.00 // _151._m86
//// ---------


sampler2D _CameraDepthTexture;
sampler2D _Noise2D_R;
sampler2D _NormalMap1;
sampler2D _NormalMap2;
sampler3D _Noise3DMap;
sampler3D _20_sampler3D;
sampler2D _21_sampler2D;
sampler2D _22_sampler2D;
sampler2D _CameraOpaqueTexture;
sampler2D _GlobalSSRTexture;
// samplerCUBE unity_SpecCube0;
// #define unity_SpecCube0_HDR float4(1.00, 1.00, 0.00, 0.00         ) // _151._m71

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

float LinearEyeDepth(float _in_rawDepth)
{
    return LinearEyeDepth(_in_rawDepth, _ZBufferParams);
}


// in_x 通常 >= 0
// in_pre_compute_a = -log(in_start_max)
// 当 x = 0 时，返回 in_start_max
// 当 x > 0 时，返回从 in_start_max 指数衰减
float ExpDamping(float in_x, float in_start_max, float in_pre_compute_a)
{
    const float local_exponent = min(/*-log(in_start_max)*/ in_pre_compute_a - in_x, 80.0);
    
    return abs(in_x) > 0.01 ? (_ExpDampingStartXZ.z - exp(local_exponent)) / in_x : in_start_max;
}

float ExpDamping(float in_x, float in_start_max)
{
    return ExpDamping(in_x, in_start_max, -log(in_start_max));
}

float Curve01(float in_x, float2 in_ST)
{
    const float local_SO = clamp(in_x * in_ST.x + in_ST.y, 0, 1);
    return local_SO * (2-local_SO);
}


fixed4 frag (v2f i) : SV_Target
{
    float4 Output_0;
    

    float3 _worldPos = i.Varying_WorldPosXYZ.xyz;

    
    // #define _LightDir float4(0.12266, 0.55406, 0.82339, 0.00           )//_151._m5
    float3 _lightDirNotNormalize = ((-i.Varying_WorldPosXYZ.xyz) * _LightDir.www) + _LightDir.xyz;

    
    float3 _lightDirNormalize;
    {
        _lightDirNormalize = _LightDir.w < 0.5 ? _LightDir.xyz : normalize(_lightDirNotNormalize);
    } 

    // 这里应该是点光源特殊处理颜色，暂不分析
    float3 _lightColorFix;
    {
        float3 _89;
        // #define _UseMainLightPosAsSunPoint float4(0.00, 0.00, 0.00, 0.00) // _151._m49
        _89.x = 1.0 / (dot(_lightDirNotNormalize, _lightDirNotNormalize) * _UseMainLightPosAsSunPoint.x + 1.0);
        _89.x = clamp(lerp(-0.04, 1.0, _89.x), 0.0, 1.0);
        
        // #define _LightColor float4(2.92204, 1.56181, 0.57585, 1.62808        ) //_151._m8
        float4 _45;
        _45.xyz = _89.xxx * _LightColor.xyz;
        _lightColorFix = _LightDir.w < 0.5 ? _LightColor.xyz : _45.xyz;
    }
    

    
    float2 _worldPosXZ1 = i.Varying_WorldPosXYZ.xz + _WorldPosXY_Offset.xz;
    
    
    float3 _surfNormal;
    {
        float2 _worldPosXYScale = _worldPosXZ1 * float2(_WorldPosXY_Scale, _WorldPosXY_Scale);
        
        float2 _NormalMap1_UV = (_Time.yy * float2(_WorldPosXY_Speed1X, _WorldPosXY_Speed1Y)) + _worldPosXYScale;
        float3 _normalSample1 = tex2Dlod(_NormalMap1, float4(_NormalMap1_UV, 0.0, 0.0)).xyz;
        
        float2 _NormalMap2_UV = (_Time.yy * float2(_WorldPosXY_Speed2X, _WorldPosXY_Speed2Y)) + _worldPosXYScale;
        float3 _normalSample2 = tex2Dlod(_NormalMap2, float4(_NormalMap2_UV, 0.0, 0.0)).xyz;
        
        float3 _normal1 = UnpackNormalWithScaleNotNormalize(_normalSample1, _NormalMapScale);
        float3 _normal2 = UnpackNormalWithScaleNotNormalize(_normalSample2, _NormalMapScale);
        _surfNormal = normalize(_normal1.xzy + _normal2.xzy);
    }
    
    float2 _screenPos = i.Varying_NonStereoScreenPos.xy / i.Varying_NonStereoScreenPos.w;
    
    
    float _depthTextureEyeDepth;
    {
        float _rawDepth = tex2D(_CameraDepthTexture, _screenPos).x;
        _depthTextureEyeDepth = LinearEyeDepth(_rawDepth);
    }
    
    // float3 _viewDir = _WorldSpaceCameraPos - _worldPos;
    // V 是 _WorldSpaceCameraPos - _worldPos
    float _backDotV = i.Varying_ViewDirXYZ_BackDotVW.w;
    float _frontDotV = -_backDotV;

    // _depthTextureEyeDepth / dot(_front, -_lookAtDir) * -_lookAtDir
    // _depthTextureEyeDepth / dot(_front, _lookAtDir) * _lookAtDir
    // _lookAtDir * (_depthTextureEyeDepth / dot(_front, _lookAtDir))
    //   注：dot(_front, _lookAtDir) 是 _surfDepth
    float3 _lookThroughAtTerrainDir = _depthTextureEyeDepth / _frontDotV * i.Varying_ViewDirXYZ_BackDotVW.xyz;


    float3 _lookThroughAtTerrainWorldPos = _WorldSpaceCameraPos.xyz + _lookThroughAtTerrainDir;

    
    float3 _viewDirNormalize;
    float _viewDir_length;
    {
        float3 _viewDir = _WorldSpaceCameraPos.xyz - i.Varying_WorldPosXYZ.xyz;
        _viewDirNormalize = normalize(_viewDir);
        _viewDir_length = length(_viewDir);
    }
    
    
    

    // float3 _terrainToSurfDir = -i.Varying_ViewDirXYZ_BackDotVW.xyz * (_depthTextureEyeDepth / _frontDotV) + (i.Varying_WorldPosXYZ.xyz - _WorldSpaceCameraPos.xyz);
    // float3 _terrainToSurfDir = -_lookThroughAtTerrainDir + _lookAtDir;
    float3 _lookAtDir = i.Varying_WorldPosXYZ.xyz - _WorldSpaceCameraPos.xyz;
    
    float3 _terrainToSurfDir = _lookAtDir - _lookThroughAtTerrainDir;
    
    float _terrainToSurfLength = length(_terrainToSurfDir);

    // 折射
    float3 _grabTextureSample;
    float3 _lookThroughDir3;
    float _terrainMoreEyeDepth4;
    {
        // _clipPos.w 就是 -_viewPos.z, -_viewPos.z 就是摄像机正前方向
        // 因为 无论 DX 还是 opengl，UNITY_MATRIX_P[3u] = float4(0, 0, -1, 0)
        float _surfEyeDepth = i.Varying_NonStereoScreenPos.w + _EyeDepthBias;

        float _terrainMoreEyeDepth = clamp(_depthTextureEyeDepth - _surfEyeDepth, 0, 1);

        float2 _nonStereoScreenPosOffset = _terrainMoreEyeDepth * _surfNormal.xz * _SurfNormalScale;
        
        float2 _screenPos2 = ( /* _terrainMoreEyeDepth* */ _nonStereoScreenPosOffset + i.Varying_NonStereoScreenPos.xy) / i.Varying_NonStereoScreenPos.w;

        // float _rawDepth2 = tex2D(_DepthTexture, _screenPos2).x;
        float _rawDepth2 = tex2D(_CameraDepthTexture, _screenPos2).x;
        float _depthTextureEyeDepth2 = LinearEyeDepth(_rawDepth2);

        float _terrainMoreEyeDepth2 = clamp(_depthTextureEyeDepth2 - _surfEyeDepth, 0.0, 1.0);

        float2 _screenPos3 = (_terrainMoreEyeDepth2 * _nonStereoScreenPosOffset + i.Varying_NonStereoScreenPos.xy) / i.Varying_NonStereoScreenPos.w;
        
        _grabTextureSample = tex2D(_CameraOpaqueTexture, _screenPos3).xyz;
        
        float _rawDepth3 = tex2D(_CameraDepthTexture, _screenPos3).x;
        float _depthTextureEyeDepth3 = LinearEyeDepth(_rawDepth3);

        _terrainMoreEyeDepth4 = _depthTextureEyeDepth3 - i.Varying_NonStereoScreenPos.w;

        // _lookAtDir * eyeDepth / dot( _lookAtDir, _front)
        _lookThroughDir3 = i.Varying_ViewDirXYZ_BackDotVW.xyz * _depthTextureEyeDepth3 / _frontDotV;
    }



    float3 _back = UNITY_MATRIX_V[2u].xyz;
    float3 _front = -_back;

    // _ProjectionParams.z = far plane
    float _far_plane = _ProjectionParams.z * 0.9999;
    


    //                                                               -0.001        1.19927
    float _WorldSpaceCameraPosY_SO = clamp(_WorldSpaceCameraPos.y * _STArgs_CameraY_ZW_DistanceXY.z + _STArgs_CameraY_ZW_DistanceXY.w, 0.0, 1.0);

    float3 _grabTextureColor;
    {
        float _lookThroughDir3_length = length(_lookThroughDir3);
        float _lookThroughDir3xz_length = length(_lookThroughDir3.xz);
        float3 _lookThroughWorldPos3 = _WorldSpaceCameraPos + _lookThroughDir3;
        
        bool _isOutOfFarPlane = dot(_lookThroughDir3, _front) >= _far_plane;
        
        float3 _baseColor_77;
        {
            float _curveOf_color_77 = Curve01(_lookThroughWorldPos3.y, _STArgs_BaseColorXY_VeryFarZW.xy); // 0.00335, -0.66724
            
            float3 _color_77_0 = _curveOf_color_77 * _ColorHeightAdd.xyz + _ColorBase.xyz;

            // #define _STArgs_BaseColorXY_And__ float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
            // #define _ColorVeryFar float4(0.39681, 0.34829, 0.44667, 0.00017      ) // _151._m16
            //                                                                   1.00           0.00017
            float _lookThroughDir3_length_OS = clamp((_lookThroughDir3_length - _STArgs_BaseColorXY_And__.w) * _ColorVeryFar.w, 0.0, 1.0);

            // 注：这里实际 _ColorVeryFar.xyz 影响非常小，因为需要 _lookThroughDir3_length 非常大 接近 > 1000 才有效果
            _baseColor_77 = lerp(_color_77_0, _ColorVeryFar.xyz, _lookThroughDir3_length_OS);
        }
        

        // 自变量变化速度 0.045 倍，从 0.9716 开始衰减
        //                                                             0.045                     0.9716
        float _exp_damping_80_1 = ExpDamping(_lookThroughDir3.y * _ExpDampingScaleXYZ.x, _ExpDampingStartXZ.x, -log(_ExpDampingStartXZ.x) /*_ExpDampingStartXZ.y*/);
        // 自变量变化速度     0 倍，从      1 开始衰减
        //                                                             0.00                      1.00
        float _exp_damping_80_2 = ExpDamping(_lookThroughDir3.y * _ExpDampingScaleXYZ.z, _ExpDampingStartXZ.z, -log(_ExpDampingStartXZ.z) /*_ExpDampingStartXZ.w*/);
        
        float _curveOf_base_color_w = Curve01(_lookThroughDir3_length, _STArgs_BaseColorXY_And__.xy); // 0.00391, -0.0625
        
        float _exp_damping_tmp_114;
        {
            //                                                0.00214
            _exp_damping_tmp_114 = _lookThroughDir3_length * _ExpDampingScaleXYZ.y;
            _exp_damping_tmp_114 = _exp_damping_tmp_114 * (-_exp_damping_80_1);
            _exp_damping_tmp_114 = 1.0 - exp2(_exp_damping_tmp_114);
            _exp_damping_tmp_114 = max(_exp_damping_tmp_114, 0.0);
        }
        
        // #define _STArgs_BaseColorXY_And__ float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
        float _base_color_w = min(
            _ColorBase.w,
            _exp_damping_tmp_114 * lerp(1.0, _curveOf_base_color_w, _STArgs_BaseColorXY_And__.z)); // _STArgs_BaseColorXY_And__.z = 1
        

        float _exp01 = max(0.0, 1.0-exp2(-_lookThroughDir3_length * _ExpDampingScaleXYZ.w * _exp_damping_80_2));
        
        
        // // #define _STArgs_ExpXY_ZW float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
        float _curveOf_exp = Curve01(_lookThroughDir3_length, _STArgs_ExpXY_ZW.xy); // 1.0, 0.0

        // #define _FarExpMaxX_VeryFarExpMaxY float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
        float _curveExp = min(_curveOf_exp * _exp01, _FarExpMaxX_VeryFarExpMaxY.y);

        // #define _STArgs_CameraY_ZW_DistanceXY float4(-0.001, 9.00, -0.001, 1.19927           ) // _151._m17
        //                                                                        -0.001        9.00
        float _lookThroughDir3xz_length_SO_0 = clamp(_lookThroughDir3xz_length * _STArgs_CameraY_ZW_DistanceXY.x + _STArgs_CameraY_ZW_DistanceXY.y, 0.0, 1.0);
        
        float _SO_10 = _isOutOfFarPlane ? _WorldSpaceCameraPosY_SO : _lookThroughDir3xz_length_SO_0;
        
        float _far_factor_1 = _SO_10 * _base_color_w;
        
        // #define _STArgs_ExpXY_ZW float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
        //                                                                       -0.01         2.50 
        float _lookThroughDir3xz_length_SO_1 = clamp(_lookThroughDir3xz_length * _STArgs_ExpXY_ZW.z + _STArgs_ExpXY_ZW.w, 0.0, 1.0);
        // 但实际是 0 
        float _far_exp_factor_2 = _lookThroughDir3xz_length_SO_1 * _curveExp;


        float _very_far01;
        {
            float _curveOf_very_far01 = Curve01(_lookThroughDir3_length, _STArgs_BaseColorXY_VeryFarZW.zw); // 0.00042, -0.00671

            float _curveOf_very_far01_fix = _isOutOfFarPlane
                ? _curveOf_very_far01 * _ColorHeightAdd.w // _ColorHeightAdd.w = 0
                : _curveOf_very_far01;
            
            _very_far01 = min(min(
                pow(_curveOf_very_far01_fix + 1.0e-4, _COLOR_FAR_FOG.w), // _COLOR_FAR_FOG.w = 1.3
                _ColorBase.w * _FarExpMaxX_VeryFarExpMaxY.x),                          // _ColorBase.w = 0.9, _FarExpMaxX_VeryFarExpMaxY.x == 1.0
                1.0);
        }
        
        // 实际 _very_far01 * _baseColor_77 无意义，因为 _far_factor_1 为 0 其为 0
        float3 _color_far = lerp( _very_far01 * _baseColor_77, _COLOR_FAR_FOG.xyz, _far_factor_1);
            _color_far += _ColorFarExp.xyz * _far_exp_factor_2; // _ColorFarExp 实际无效果
        
        float3 _grabTextureSample_Mod = max((_grabTextureSample - _color_far)/max((1-_far_exp_factor_2) * (1-_far_factor_1) * (1-_very_far01), 1e-4), (0.0));

        
        // #define _GrabTextureFade 0.00      // _151._m59
        _grabTextureColor = lerp(_grabTextureSample, _grabTextureSample_Mod, _GrabTextureFade);
    }

    
    
    float4 _shadowCoord = TransformWorldToShadowCoord(_worldPos);
    
    // float _screenSpaceShadow = tex2D(_ScreenMaskMap, _screenPos).x;
    // float _shadowAtten = (_EnableShadow == 1.0) ? _screenSpaceShadow : 1.0;
    float _shadowAtten = MainLightRealtimeShadow(_shadowCoord);
    

    float _min_moreEyeDepth4 = clamp(_viewDir_length * 0.2 - 20.0, 0.0, 10.0);
    // > 100 后，100~150 最小值从 0变成10
    // 靠近时，水面内部 >=1，边缘 0
    // 远离时，水面内部和边缘都是 >=1
    // lerp( 有浮沫线，没有，_terrainMoreEyeDepth4_amend ), 因为远处时都是 >=1 所以远就没有浮沫线
    // 水深，近距离观察岸边浅，但远距离观察岸边和内部都认为比较深
    float _terrainMoreEyeDepth4_amend = max(_terrainMoreEyeDepth4, _min_moreEyeDepth4);

    



    // 水深，近距离观察岸边浅，但远距离观察岸边和内部都认为比较深
    // 反射，近距离观察岸边无反射，远距离观察全都有反射
    float _waterReflectFactor = min(pow(clamp(_terrainMoreEyeDepth4_amend * _WaterSmoothness, 0.0, 1.0) + 1e-4, _WaterReflectPower), 1.0);

    
    // 计算水底焦散在这里，计算的是水底透射颜色
    float3 _waterTransmissionColor;
    {
        // 最终水底焦散颜色结果
        float3 _transmissionCausticColor;
        {
            // 水底焦散噪声 
            float _causticNoise3DResult;
            {
                float3 _causticPos3DInput;
                _causticPos3DInput.xy = (_Time.y * _CausticSpeed * float2(_WorldPosXY_Speed1X, _WorldPosXY_Speed1Y) * 25.0) + _lookThroughAtTerrainWorldPos.xz * _CausticScale + _terrainToSurfLength * _CausticNormalDisturbance * _surfNormal.xz;
                _causticPos3DInput.z  = _Time.y * _CausticSpeed;

                _causticNoise3DResult = GenshipCaustic(_causticPos3DInput);
            }
            
            // 水底焦散 高度、距离 可见系数
            // _CausticVisibleHeight
            // _CausticVisibleDistance
            float _causticVisibleFactor;
            {
                float _causticVisibleHeightFactor = clamp(_terrainToSurfDir.y * _CausticVisibleHeight, 0.0, 1.0);

                float _lookAtTerrain_length_adjust_clamp01 = clamp(length(_lookThroughAtTerrainDir) * _CausticVisibleDistance, 0, 1);
                float _causticVisibleDistanceFactor = 1-_lookAtTerrain_length_adjust_clamp01;
                
                _causticVisibleFactor = _causticVisibleHeightFactor * _causticVisibleDistanceFactor;
            }
            
            
            float3 _causticColor = _causticNoise3DResult * _LightColor.xyz * _CausticColor.xyz * _causticVisibleFactor * _shadowAtten;
            
            // 水底焦散 水深、指数 可见系数
            // _CausticVisibleWaterDepth
            // _CausticVisiblePower
            float _causticColorDisappear = pow(clamp(_terrainToSurfLength * _CausticVisibleWaterDepth, 0.0, 1.0) + 1e-4,  _CausticVisiblePower);
            
            _transmissionCausticColor = _causticColor * (1-_causticColorDisappear);
        }
        
        
        // 水底焦散受抓屏的颜色影响
        float3 _transmissionColor = _grabTextureColor + _grabTextureColor * _transmissionCausticColor;

        float _waterBottomDarkFactor = min(pow(clamp(_terrainMoreEyeDepth4_amend * _WaterBottomDarkFactor, 0.0, 1.0) + 1e-4, _WaterBottomDarkPower), 1.0);
        
        float3 _waterSurfColor0 = lerp(_WaterSurfColor.xyz, _WaterSurfColorBlend.xyz, _WaterSurfColorBlend.w);

        // _waterReflectFactor 为 0，无反射，则全透射
        // _waterReflectFactor 为 1，高反射，透射仅过滤 _waterSurfColor 颜色
        float3 _surfTransmissionFactor = lerp(1.0, _waterSurfColor0, _waterReflectFactor);

        float3 _transmissionSurfColor = _transmissionColor * _surfTransmissionFactor;
        
        float3 _waterBottomDarkColor0 = lerp(_WaterBottomDarkColor.xyz, _WaterBottomDarkColorBlend.xyz, _WaterBottomDarkColorBlend.w);
        
        _waterTransmissionColor = lerp(_transmissionSurfColor, _waterBottomDarkColor0, _waterBottomDarkFactor);
    }


    float3 _reflectColor;
    {
        float3 _surfNormal2 = normalize(_surfNormal.xyz * _SurfNormalReflectScale.xzy);

        float3 _reflectDir = normalize(reflect(-_viewDirNormalize, _surfNormal2));

        
        float4 _unity_SpecCube0Sample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, _reflectDir, 0.0);
        // float4 data = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, _reflectDir, 0.0);
        // float alpha = unity_SpecCube0_HDR.w * (data.a - 1.0) + 1.0;
        // float3 _decodeHdr = unity_SpecCube0_HDR.x * pow(alpha, unity_SpecCube0_HDR.y) * data.xyz;
        // float3 _decodeHdr = DecodeHDR(_unity_SpecCube0Sample, unity_SpecCube0_HDR);
        float3 _decodeHdr = DecodeHDREnvironment(_unity_SpecCube0Sample, _GlossyEnvironmentCubeMap_HDR);
        

        float2 _screenReflectUV = (_surfNormal2.xz * clamp(_terrainMoreEyeDepth4_amend * _SSRNormalDisturbanceWaterDepthRelevant, 0.0, 1.0) * _SSRNormalDisturbance) + _screenPos;

        
        float4 _ssrSample = tex2D(_GlobalSSRTexture, _screenReflectUV);
     
        float _ssrAlpha = clamp(_ssrSample.w * _SSREnable * _SSRAlpha, 0.0, 1.0);


        _reflectColor = lerp(_decodeHdr, _ssrSample.xyz, _ssrAlpha);
    }

    float _reflectFactor01;
    {
        // 俯视看不见反射
        float _reflectFactorOfViewDir = max(1.0 - _ReflectWaterViewYDisappearFactor * _viewDirNormalize.y, 0.05);

        // 岸边太浅看不见反射
        float _reflectFactorOfWaterDepth = clamp(_terrainMoreEyeDepth4_amend * _ReflectWaterDepthFactor, 0.0, 1.0);
            
         _reflectFactor01 = clamp(_reflectFactorOfWaterDepth * _reflectFactorOfViewDir * _ReflectFactor * _ReflectEnable, 0.0, 1.0);
    }
    
    
    // _waterReflectFactor 为 0，无反射，全透射颜色
    // _waterReflectFactor 为 1，高反射，无透射颜色
    float3 _waterColorTransmissionOrReflection = lerp(_waterTransmissionColor, _reflectColor, _waterReflectFactor * _reflectFactor01);

    // 测试发现 _lightingColor 没有值，几乎均为 0
    float3 _lightingColor;
    {
        // #define _PointLightDir1  float3(0.13963, 0.31927, 0.93732              ) //_151._m9
        // #define _PointLightDir2 float3(0.05565, -0.29114, -0.95506            ) //_151._m10
        // #define _UseMainLightPosAsSunPoint float4(0.00, 0.00, 0.00, 0.00) // _151._m49
        float3 _sunLightOrMoonLight = _UseMainLightPosAsSunPoint.x == 0.0 ? (_PointLightDir2.y < 0.0 ? _PointLightDir1 : _PointLightDir2) : _lightDirNormalize;

        float3 _surfNormal_moreUp_normalize = normalize(float3( _surfNormal.x, 1.0, _surfNormal.z ));

        float3 _H = normalize(_viewDirNormalize + _sunLightOrMoonLight);

        float _fixNDotH_clamp01 = clamp(dot(_surfNormal_moreUp_normalize, _H), 0.0, 1.0);
        
        // #define _NDotHPower 332.79999 // _151._m63
        float _fixNDotH_pow = SphericalGaussianPow( _fixNDotH_clamp01, _NDotHPower );
        
        
        // #define _GlossFactorLookAtHorizontallyAndLightHight 2.38      // _151._m65
        // _viewDirNormalize.y 相关，_sunLightOrMoonLight.y 相关
        // 平视时 _viewDirNormalize.y 最终结果大
        // 灯源高时，_sunLightOrMoonLight.y 最终结果大
        float _glossFactor_viewDirYLightYRelevant = max(1.0 - _GlossFactorLookAtHorizontallyAndLightHight * _viewDirNormalize.y, 0.05) * max(_GlossFactorLookAtHorizontallyAndLightHight * _sunLightOrMoonLight.y - 1.0, 0.05);

        // #define _WaterSmoothness 0.40      // _151._m64
        float _glossFactor_waterDepth = clamp(lerp(-0.1, 0, _terrainMoreEyeDepth4_amend) * _WaterSmoothness, 0.0, 1.0);;

        // float3 _foamLightColor = _lightColorFix * _LightIntensity;

        _lightingColor = _lightColorFix * _LightIntensity * _glossFactor_waterDepth * _glossFactor_viewDirYLightYRelevant * _fixNDotH_pow;
    }

    
    float _foamLineMixFactor;
    float3 _foamLineColor;
    {
        float _brightness = max(_lightDirNormalize.y, 0.0) * _shadowAtten;

        float3 _foamLightColor = _brightness * _lightColorFix + i.Varying_GlossColorAdd.xyz;

        float _foamLineArea_oneMinus = min(i.Varying_ColorXYW.x * _FoamLineAreaSize, max(_terrainToSurfDir.y, 0.0)) / (_FoamLineAreaSize * i.Varying_ColorXYW.x + 1e-4);
        float _foamLineArea = 1 - _foamLineArea_oneMinus;

        float2 _noise2D_R_UV = (_Noise2D_R_ScaleSpeed.xy * _worldPosXZ1) + frac(_Time.y * _Noise2D_R_ScaleSpeed.zw);

        float _noise2D_R_Sample = tex2D(_Noise2D_R, _noise2D_R_UV).x;

        float _foamLine0 = sin((_FoamLineSinFrequency * _foamLineArea) + (_FoamLineSpeed * _Time.y) + (_worldPosXZ1.y + _worldPosXZ1.x) * _FoamLinePosSpeed);

        float _foamLine_add_noise_n1_1 = (_FoamLineAreaBaseMulti * _foamLineArea) + _foamLine0 + (_noise2D_R_Sample * 2.0) + (-1.0);

        float _foamLineNoise = _noise2D_R_Sample * float(_foamLine_add_noise_n1_1 >= _foamLineArea_oneMinus);

        float _tmp_124 = 1.0 - clamp(_viewDir_length / (_FoamLineFadeDistance + 1e-4), 0.0, 1.0);

        float _tmp_76 =  i.Varying_ColorXYW.y * (1-min(_viewDir_length * 0.01, 1.0)) * _FoamColor.w * (_viewDir_length - _FoamLineVisibleDistance) / _FoamLineFadeDiv * _foamLineNoise * _foamLineArea;

        _foamLineMixFactor = clamp(_tmp_124 * _tmp_76, 0.0, 1.0);

        _foamLineColor = _FoamColor.xyz * _foamLightColor;
    }
    
    float3 _waterColorTransmissionAndRefelctionMixFoamLine = lerp(_waterColorTransmissionOrReflection, _foamLineColor, _foamLineMixFactor);

    float3 _waterColor_Final = 0 /*_lightingColor*/ + _waterColorTransmissionAndRefelctionMixFoamLine;
        _waterColor_Final += _lightingColor;
    
   
    // 岸边 alpha 为 0
    float _output_alpha = clamp(_terrainMoreEyeDepth4_amend * _WaterAlpha, 0.0, 1.0) * i.Varying_ColorXYW.w;
    Output_0.w = _output_alpha;
    
    
    // 可以不用混合表面雾
    float3 _color_mix_far_fog;
    {
        float _lookAtDir_length = length(_lookAtDir);
        float _lookAtDirXZ_length = length(_lookAtDir.xz);
        // _worldPos 直接用 _worldPos
        
        float _surfEyeDepth2 = -dot(_lookAtDir, _back);
        bool _isOutOfFarPlane_B = _surfEyeDepth2 >= _far_plane;
        
        float3 _baseColor_57;
        {
            float _curveOf_baseColor_57 = Curve01(_worldPos.y, _STArgs_BaseColorXY_VeryFarZW.xy); // 0.00335, -0.66724
            
            float3 _color_57_0 = (_curveOf_baseColor_57 * _ColorHeightAdd.xyz) + _ColorBase.xyz;

            // #define _STArgs_BaseColorXY_And__ float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
            // #define _ColorVeryFar float4(0.39681, 0.34829, 0.44667, 0.00017      ) // _151._m16
            //                                                       1.00           0.00017
            float _lookAtDir_length_OS = clamp((_lookAtDir_length - _STArgs_BaseColorXY_And__.w) * _ColorVeryFar.w, 0.0, 1.0);

            // 注：这里实际 _ColorVeryFar.xyz 影响非常小，因为需要 _lookThroughDir3_length 非常大 接近 > 1000 才有效果
            _baseColor_57 = lerp(_color_57_0, _ColorVeryFar.xyz, _lookAtDir_length_OS);
        }
        

        // 自变量变化速度 0.045 倍，从 0.9716 开始衰减
        //                                                       0.045                     0.9716
        float _exp_damping_66_1 = ExpDamping(_lookAtDir.y * _ExpDampingScaleXYZ.x, _ExpDampingStartXZ.x, -log(_ExpDampingStartXZ.x) /*_ExpDampingStartXZ.y*/);
        // 自变量变化速度     0 倍，从      1 开始衰减
        //                                                       0.00                      1.00
        float _exp_damping_66_2 = ExpDamping(_lookAtDir.y * _ExpDampingScaleXYZ.z, _ExpDampingStartXZ.z, -log(_ExpDampingStartXZ.z) /*_ExpDampingStartXZ.w*/);
        
        
        float _curveOf_base_color_w_B = Curve01(_lookAtDir_length, _STArgs_BaseColorXY_And__.xy); // 0.00391, -0.0625

        float _exp_damping_tmp_109;
        {
            //                                          0.00214
            _exp_damping_tmp_109 = _lookAtDir_length * _ExpDampingScaleXYZ.y;
            _exp_damping_tmp_109 = _exp_damping_tmp_109 * (-_exp_damping_66_1);
            _exp_damping_tmp_109 = 1.0 - exp2(_exp_damping_tmp_109);
            _exp_damping_tmp_109 = max(_exp_damping_tmp_109, 0.0);
        }
        
        // #define _STArgs_BaseColorXY_And__ float4(0.00391, -0.0625, 1.00, 1.00            ) // _151._m12
        float _base_color_w_B = min(
            _ColorBase.w,
            _exp_damping_tmp_109 * lerp(1.0, _curveOf_base_color_w_B, _STArgs_BaseColorXY_And__.z)); // _STArgs_BaseColorXY_And__.z = 1.0


        float _exp01_B = max(0.0, 1.0 - exp2(-_lookAtDir_length * _ExpDampingScaleXYZ.w * _exp_damping_66_2));
        
        // // #define _STArgs_ExpXY_ZW float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
        float _curveOf_exp_B = Curve01(_lookAtDir_length, _STArgs_ExpXY_ZW.xy); // 1.0, 0.0
        
        // #define _FarExpMaxX_VeryFarExpMaxY float4(1.00, 0.90, 0.00, 0.00                  ) // _151._m21
        float _curveExp_B = min(_curveOf_exp_B * _exp01_B, _FarExpMaxX_VeryFarExpMaxY.y);

        // #define _STArgs_CameraY_ZW_DistanceXY float4(-0.001, 9.00, -0.001, 1.19927           ) // _151._m17
        //                                                            -0.001        9.00
        float _lookAtDirXZ_length_SO_0 = clamp(_lookAtDirXZ_length * _STArgs_CameraY_ZW_DistanceXY.x + _STArgs_CameraY_ZW_DistanceXY.y, 0.0, 1.0);
        
        float _SO_10_B = _isOutOfFarPlane_B ? _WorldSpaceCameraPosY_SO : _lookAtDirXZ_length_SO_0;
        
        float _far_factor_1_B = _SO_10_B * _base_color_w_B;
        
        // #define _STArgs_ExpXY_ZW float4(1.00, 0.00, -0.01, 2.50                 ) // _151._m19
        //                                                            -0.01         2.50 
        float _lookAtDirXZ_length_SO_1 = clamp(_lookAtDirXZ_length * _STArgs_ExpXY_ZW.z + _STArgs_ExpXY_ZW.w, 0.0, 1.0);
        // 但实际是 0 
        float _far_exp_factor_2_B = _lookAtDirXZ_length_SO_1 * _curveExp_B;


        float _very_far01_B;
        {
            float _curveOf_very_far01_B = Curve01(_lookAtDir_length, _STArgs_BaseColorXY_VeryFarZW.zw);

            float _curveOf_very_far01_B_fix = _isOutOfFarPlane_B
                ? _curveOf_very_far01_B * _ColorHeightAdd.w // _ColorHeightAdd.w = 0
                : _curveOf_very_far01_B;
            
            _very_far01_B = min(min(
                pow(_curveOf_very_far01_B_fix + 1e-4, _COLOR_FAR_FOG.w), // _COLOR_FAR_FOG.w = 1.3
                _ColorBase.w * _FarExpMaxX_VeryFarExpMaxY.x),                          // _ColorBase.w = 0.9, _FarExpMaxX_VeryFarExpMaxY.x == 1.0
                1.0);
        }
        
        // 实际 _very_far01_B * _baseColor_57 无意义，因为 _far_factor_1_B 为 0 其为 0
        float3 _color_far_B = lerp(_very_far01_B * _baseColor_57, _COLOR_FAR_FOG.xyz, _far_factor_1_B);
            _color_far_B += _ColorFarExp.xyz * _far_exp_factor_2_B;

            _color_far_B += (1- _far_exp_factor_2_B) * (1.0 - _very_far01_B) * (1 - _far_factor_1_B) * _waterColor_Final;

        _color_mix_far_fog = _color_far_B;
    }

    
    // 可以不用 mix fog 的版本
    Output_0.xyz = _waterColor_Final;
    Output_0.xyz = _color_mix_far_fog;
    
    fixed4 col = fixed4(0,0,0,1);
    col = Output_0;

    // col = float4(_reflectFactor01.xxx, 1.0);
    // col = float4(_waterColor_Final.rgb, 1.0);
    // col = float4(_waterColorTransmissionAndRefelctionMixFoamLine.rgb, 1.0);
    // col = float4(_reflectColor.rgb, 1.0);
    // col = float4(_ssrSample.aaa, 1.0);
    // col = float4(_lightingColor.rgb, 1.0);
    // col = float4(_fixNDotH_pow.xxx, 1.0);
    // col = float4(_glossFactor_viewDirYLightYRelevant.xxx, 1.0);
    // col = float4(_viewDirNormalize.y.xxx, 1.0);
    // col = float4(_glossFactor_waterDepth.xxx, 1.0);
    // col = float4(_lightingColor.rgb, 1.0);
    
    
    
    return col;
}
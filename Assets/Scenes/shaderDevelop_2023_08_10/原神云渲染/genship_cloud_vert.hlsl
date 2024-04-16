#include "genship_cloud_common.hlsl"

#define _RolePos_maybe                        float3(-3.48413, 195.00, 2.47919) // _58._m3
#define _UpDir                                float3(0.00, 1.00, 0.00         ) // _58._m4

// _IrradianceMapR Rayleigh Scatter
float3 _upPartSunColor; 
float3 _upPartSkyColor; 
float3 _downPartSunColor; 
float3 _downPartSkyColor; 
float _mainColorSunGatherFactor;
float _IrradianceMapR_maxAngleRange;

// _IrradianceMapG Mie Scatter
float3 _SunAdditionColor;
float _SunAdditionIntensity;
float _IrradianceMapG_maxAngleRange;

// Sun Disk
float _sun_disk_power_999; // _58._m18
float3 _sun_color; // _58._m19
float _sun_color_intensity; // _58._m20 
float3 _sun_shine_color; // _58._m15

// Moon
float _moon_intensity_slider; // _58._m25
float3 _moon_shine_color;           // _58._m22
float _moon_intensity_max;    // _58._m24

// Transmission
float _SunTransmission;
float _MoonTransmission;      // _58._m23
float _TransmissionLDotVStartAt;

float _DisturbanceNoiseOffset;
// #define _DisturbanceNoiseOffset              262.33862 // _58._m26

float3 _CloudColor_Bright_Center;
float3 _CloudColor_Bright_Around;
float3 _CloudColor_Dark_Center;
float3 _CloudColor_Dark_Around;
float _LDotV_damping_factor_cloud;

#define _LightingOcclusion                              0.11        // _58._m32 // const
#define _58__m33                              1.00        // _58._m33 // const
float _CloudMoreBright; //                              0.8299      // _58._m34
// #define _CloudMoreBright                              0.8299      // _58._m34
#define _MaskMapGridSize                      float2( 2.00, 4.00 ) // _58._m35 // const
#define _DisturbanceNoiseScale                3.00        // _58._m36 // const
#define _DisturbanceNoiseOffsetScale               6.00        // _58._m37 // const
#define _58__m38                              1.00        // _58._m38 // const

sampler2D _IrradianceMap;

float3 _sun_dir;
float3 _moon_dir; 

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

    // Vertex_1.y = {0, 0.28235, 0.42745, 0.56863, 0.8549, 1.0}
    // Vertex_GridIndexY_FeatherZW.x = ? 无效
    // Vertex_GridIndexY_FeatherZW.y = {0, 0.28235, 0.42745, 0.56863, 0.8549, 1.0}  即 { 0/7, 1/7, 2/7, 3/7, 5/7, 6/7, 7/7 }  表示在哪个图集网格内
    // Vertex_GridIndexY_FeatherZW.zw 一样，表示边缘羽化参数
    float4 Vertex_GridIndexY_FeatherZW = v.color;
    
    float4 Vertex_uv = float4( v.uv, 0,0 ) ;
    
    // Vertex_AnimParams.x = 104.096           |  171.435           动画总时长
    // Vertex_AnimParams.y = 4.70795 ~ 102.966 |  17.8982 ~ 153.264 当前动画时间

    // Vertex_AnimParams.y / Vertex_AnimParams.x = {0.04, 0.12 0.13, 0.15, 0.23, 0.25, 0.37, 0.42, 0.49, 0.52, 0.53, ..., 0.989} 动画时间百分比
    
    // Vertex_AnimParams.z = 0.4  动画时间百分比 0.4 处完全淡入
    // Vertex_AnimParams.w = 0.6  动画时间百分比 0.6 处完全淡出
    float4 Vertex_AnimParams = fixed4( v.uv2, v.uv3 );
    v2f o;
    
    

    float4 _worldPos = mul(UNITY_MATRIX_M, Vertex_Position);

    // o.vertex
    {
        float4 _clipPos = mul(UNITY_MATRIX_VP, _worldPos);
         _clipPos.z = _clipPos.w;
        #if defined(UNITY_REVERSED_Z)
        #if UNITY_REVERSED_Z == 1
            _clipPos.z = 0;
        #endif
        #endif
        o.vertex = _clipPos;
    }
    

    
    // #define _RolePos_maybe  float3(-3.48413, 195.00, 2.47919) // _58._m3
    float3 _viewDir = normalize(_worldPos.xyz - _RolePos_maybe);
    
    float _VDotSun = dot(_viewDir, _sun_dir);
    float _VDotSunRemap01 = _VDotSun * 0.5 + 0.5;
    float _VDotSunRemap01Clamp = clamp(_VDotSunRemap01, 0.0, 1.0);

    float _rawUpDotDir = dot(_UpDir, _viewDir);

    float _miu = clamp(_rawUpDotDir, -1.0, 1.0);

    float _angle_up_to_down_1_n1 = (UNITY_HALF_PI - FastAcos(_miu)) * UNITY_INV_HALF_PI;
    
    float _VDotMoonRemap01 = dot(_viewDir, _moon_dir) * 0.5 + 0.5;
    
    
    float _VDotSunDampingA = max(0, lerp( 1, _VDotSun, _mainColorSunGatherFactor ));
    float _VDotSunDampingCloud = max(0, lerp( 1, _VDotSun, _LDotV_damping_factor_cloud ));
    
    float _VDotSunDampingA_pow3 = _VDotSunDampingA * _VDotSunDampingA * _VDotSunDampingA;
    float _VDotSunDampingCloud_pow3 = _VDotSunDampingCloud * _VDotSunDampingCloud * _VDotSunDampingCloud;
    
    // #define _sun_disk_power_999 8.30078 // _58._m18
    float _VDotUp_Multi999 = abs(_rawUpDotDir) * _sun_disk_power_999;



    
    // o.Varying_ViewDirAndAngle1_n1
    {
        o.Varying_ViewDirAndAngle1_n1.w = _angle_up_to_down_1_n1;
        o.Varying_ViewDirAndAngle1_n1.xyz = _viewDir;
    }

    // o.Varying_MaskMapUvXY_DisturbanceNoiseUvZW
    {
        // #define _MaskMapGridSize float2( 2.00, 4.00 ) // _58._m35
        // 测试发现，_MaskMapGridSize.x < 0 时需要改成 +1, _MaskMapGridSize.x < 0 才是正确的左右翻转效果
        // 原神则均是 -1.0
        float _gridIndex_0_7;
        _gridIndex_0_7 = _MaskMapGridSize.x * _MaskMapGridSize.y + (_MaskMapGridSize.x >= 0 ? -1.0 : +1.0); // 7

        // Vertex_1.y         = {0,     0.28235, 0.42745, 0.56863, 0.8549, 1.0}
        // Vertex_1.y * 7     = {0,     1.97645, 2.99215, 3.98041, 5.9843, 7.0}
        // floor(_gridIndex_0_7 + 0.5)  = {0,     2,       3,       4,       6,      7}
        _gridIndex_0_7 = (Vertex_GridIndexY_FeatherZW.y * _gridIndex_0_7); // 0~7
        _gridIndex_0_7 = floor(_gridIndex_0_7 + 0.5);
        
        
        float _gridSizeX = abs(_MaskMapGridSize.x);
        float _gridIndex_x = frac(_gridIndex_0_7 / _gridSizeX) * _gridSizeX;
        
        float _gridIndex_y = floor(_gridIndex_0_7 / _MaskMapGridSize.x);

        float2 _gridRootIntUVAtLeftDown = float2(_gridIndex_x, _gridIndex_y);

        float2 _gridUVStartAtLeftDown = _gridRootIntUVAtLeftDown + Vertex_uv.xy;

        o.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.xy = _gridUVStartAtLeftDown/_MaskMapGridSize;

        // #define _58__m26 262.33862 // _58._m26
        // #define _58__m37 6.00        // _58._m37
        // #define _58__m36 3.00        // _58._m36
        o.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.zw = Vertex_uv.xy * _DisturbanceNoiseScale + float2(1.2, 0.8) * _DisturbanceNoiseOffset * _DisturbanceNoiseOffsetScale;
      
    }
    
    
    // o.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX
    {
        // #define _CloudMoreBright 0.8299      // _58._m34
        // 
        o.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.x = _VDotSunRemap01 * _CloudMoreBright;

        o.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.yz = Vertex_GridIndexY_FeatherZW.zw;
        
        // #define _58__m33 1.00        // _58._m33
        // #define _58__m38 1.00        // _58._m38
        float _Vertex_y_present_fix = Vertex_AnimParams.y / max(Vertex_AnimParams.x, 1.0e-05) * _58__m33 * _58__m38;

        // 1.0 - smoothstep(0, 0.4, x) * (1.0 - smoothstep(0.6, 1.0, x))
        // o.Varying_2.w 是 _Vertex_y_present_fix 01 映射到 1 \ 0 / 1 的平滑图案，其中 0.4 处达到最低位 0, 0.6 处离开最低位 0
        // 动画时间百分比 0.4 处完全淡入，0.6 处完全淡出
        o.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.w = 1.0 - smoothstep(0, Vertex_AnimParams.z, _Vertex_y_present_fix) * (1.0 - smoothstep(Vertex_AnimParams.w, 1.0, _Vertex_y_present_fix));
    }

    
    // o.Varying_BColor_1
    // o.Varying_BColor_2
    {
        // #define _CloudColor_Bright_Center float3(0.05199, 0.10301, 0.13598) // _58._m27
        // #define _CloudColor_Bright_Around float3(0.10391, 0.41824, 0.88688) // _58._m28
        // #define _CloudColor_Dark_Center float3(0.00, 0.03576, 0.12083   ) // _58._m29
        // #define _CloudColor_Dark_Around float3(0.02281, 0.05716, 0.14666) // _58._m30
        o.Varying_CloudColor_Bright = lerp(_CloudColor_Bright_Around, _CloudColor_Bright_Center, _VDotSunDampingCloud_pow3);
        o.Varying_CloudColor_Dark = lerp(_CloudColor_Dark_Around, _CloudColor_Dark_Center, _VDotSunDampingCloud_pow3);
    }


    
    // o.Varying_IrradianceColor
    {
        // #define _IrradianceMapR_maxAngleRange 0.20     // _58._m10
        // _irradianceMapR 最左边是 0 度的，最右边是 0.2*90°=18° 的，即只记录水平朝向的值，更高，更低的值都是 18° 的值。
        float2 _irradianceMap_R_uv;
            _irradianceMap_R_uv.x = abs(_angle_up_to_down_1_n1) / max(_IrradianceMapR_maxAngleRange, 1.0e-04);
            _irradianceMap_R_uv.y = 0.5;
        

        float _irradianceMapR = tex2Dlod(_IrradianceMap, float4(_irradianceMap_R_uv, 0.0, 0.0)).x;
        
        // #define _downPartSunColor  float3(0.0538, 0.09841, 0.2073  ) // _58._m7
        // #define _downPartSkyColor  float3(0.0538, 0.09841, 0.2073  ) // _58._m8
        // _downPartColor 这里指 _irradianceMapR 为 1 的颜色，理解成太阳 disk 颜色
        float3 _downPartColor = lerp(_downPartSkyColor, _downPartSunColor, _VDotSunDampingA_pow3);
        
        // #define _upPartSunColor  float3(0.00972, 0.02298, 0.06016) // _58._m5
        // #define _upPartSkyColor  float3(0.00972, 0.02298, 0.06016) // _58._m6
        // _upPartColor 这里指 _irradianceMapR 为 0 的颜色，理解成太阳散射到附近大气的颜色
        float3 _upPartColor = lerp(_upPartSkyColor, _upPartSunColor, _VDotSunDampingA_pow3);

        // sun color
        float3 _mainColor = lerp( _upPartColor, _downPartColor, _irradianceMapR );

        

        float2 _irradianceMap_G_uv;
        // #define _IrradianceMapG_maxAngleRange 0.30 // _58._m13
        // _irradianceMapR 最左边是 0 度的，最右边是 0.3*90°=27° 的，即只记录水平朝向的值，更高，更低的值都是 27° 的值。
            _irradianceMap_G_uv.x = abs(_angle_up_to_down_1_n1) / max(_IrradianceMapG_maxAngleRange, 1.0e-04);
            _irradianceMap_G_uv.y = 0.5;

        float _irradianceMapG = tex2Dlod(_IrradianceMap, float4(_irradianceMap_G_uv, 0.0, 0.0)).y;

        // #define _SunAdditionColor float3(0.00837, 0.10516, 0.26225) // _58._m11
        // #define _SunAdditionIntensity 0.50 // _58._m12
        // sky color
        float3 _sunAdditionPartColor = _irradianceMapG * _SunAdditionColor * _SunAdditionIntensity;


        // smoothstep(0, 1, clamp( (abs(x)-0.2) * 10/3, 0, 1))
        // 从 0.2 处离开0，平滑上升，0.5 处开始达到最大 1.0 
        float _upFactor = smoothstep(0, 1, clamp((abs(_sun_dir.y) - 0.2) * 10/3, 0, 1));
        
        // smoothstep(0, 1, max((clamp(x, 0.0, 1.0)-1)/0.7 + 1, 0.0))
        // y=x 直线，固定 (1, 1) 点不动，旋转，使其斜率变成 1/0.7，加速衰减，并 smooth
        float _VDotSunFactor = smoothstep(0, 1, (_VDotSunRemap01Clamp-1)/0.7 + 1);

        // 意思是优先判断高度，高的地方就是全额 _sunAdditionPartColor
        //       lightDirY > 0.5 处是 1.0 
        //       lightDirY < 0.2 处是 _VDotSunFactor
        float _sunAdditionPartFactor = lerp(_VDotSunFactor, 1.0, _upFactor);

        float3 _additionPart = _sunAdditionPartColor * _sunAdditionPartFactor;

        float3 _sumIrradianceRGColor = _mainColor + _additionPart;

        
        // #define _sun_color_intensity 0.01039 // _58._m20
        // #define _sun_color float3(0.01938, 0.00651, 0.02122) // _58._m19
        float _sun_disk = dot(
                            min( 1, pow(_VDotSunRemap01Clamp, _VDotUp_Multi999 * float3(1, 0.1, 0.01))),
                            float3(1, 0.12, 0.03))
                            * _sun_color_intensity * _sun_color;
        
        float _LDotDirClampn11_smooth = smoothstep(0, 1, 2 * _VDotSunRemap01Clamp - 1.0);
        o.Varying_DayPartColor = _sun_disk * _LDotDirClampn11_smooth + _sumIrradianceRGColor;
    }


    // #define _LightingOcclusion 0.11        // _58._m32 // const
    // 那么 _adjust_1_to_0_for_0d3_to_0d7 = 1; // const
    // smoothstep(0, 1, -2.5*(x-0.3)+1), y=-x 移动到(0.3, 1) 固定(0.3, 1) 旋转至斜率 -2.5，从 0.3 开始离开最高1，从0.7到达最低0，并 smooth
    float _adjust_1_to_0_for_0d3_to_0d7 = smoothstep(0, 1, -2.5*(_LightingOcclusion-0.3)+1);
    
    // o.Varying_TwoPartColor
    {
        // #define _moon_dir float3(0.31638, 0.70655, 0.633) // _58._m21
        float _VDotMoonClamp01 = clamp(dot(_moon_dir, _viewDir), 0.0, 1.0);
        
        // #define _moon_shine_color float3(0.29669, 0.64985, 1.00 ) // _58._m22
        // #define _MoonTransmission 3.29897 // _58._m23
        // #define _moon_intensity_max 0.19794 // _58._m24
        
        // 上箭头形状，0.5 最高 是 1，左右 0、1 是 0
        // -abs(x-0.5)*2+1
        float _moon_slider_value = -abs(_moon_intensity_slider - 0.5) * 2.0 + 1.0;
        float _VDotMoonPow5_mod_smooth = smoothstep(0, 1, 2 * pow(_VDotMoonClamp01, 5) - 1.0);
        float3 _moonShine = _moon_slider_value * _VDotMoonPow5_mod_smooth * _moon_shine_color * clamp(_MoonTransmission, 0.0, 0.8) * _moon_intensity_max;
        
        // #define _sun_color_intensity 0.01039 // _58._m20
        // #define _sun_shine_color float3(0.01938, 0.00651, 0.02122  ) // _58._m15
        float3 _sunShine = clamp(pow( _VDotSunRemap01Clamp, _VDotUp_Multi999 * 0.5 ) * _rawUpDotDir, 0.0, 1.0) * _sun_shine_color * _sun_color_intensity;
        
        float3 _twoPartColor = _moonShine + _sunShine;
        
        o.Varying_ShineColor = _adjust_1_to_0_for_0d3_to_0d7 * _twoPartColor;
    }


    
    // o.Varying_MoreFadeTwoPartColor
    {
        // _35.z = _VDotSunRemap01; // _35.z 实际有用途，就是后面的 _VDotSunRemap01 实际需要被 减 和除，做和 _VDotMoonRemap01 类似的 加速 Fade 运算

        // #define _TransmissionLDotVStartAt 0.80205 // _58._m17
        // #define _MoonTransmission 3.29897 // _58._m23
        // #define _moon_shine_color float3(0.29669, 0.64985, 1.00 ) // _58._m22
        float _VDotMoonMoreFadeMulti = smoothstep(_TransmissionLDotVStartAt, 1.0, _VDotMoonRemap01) * _MoonTransmission * 0.1;
        float3 _moonTransmission = _VDotMoonMoreFadeMulti * _VDotMoonMoreFadeMulti * _moon_shine_color;
        
        // 原版没有 clamp，需要自行保证 _VDotSunRemap01 = saturate(_VDotSunRemap01)
        // #define _SunTransmission 4.09789 // _58._m16
        // #define _sun_color float3(0.01938, 0.00651, 0.02122) // _58._m19
        float _LDirDotDirMoreFadeMulti = smoothstep(_TransmissionLDotVStartAt, 1.0, _VDotSunRemap01) * _SunTransmission * 0.125;
        float _sunTransmission = _LDirDotDirMoreFadeMulti * _LDirDotDirMoreFadeMulti * _sun_color;
        
        float3 _moreFadeTwoPartColor = _moonTransmission + _sunTransmission;

        o.Varying_TransmissionColor = _adjust_1_to_0_for_0d3_to_0d7 * _moreFadeTwoPartColor;
    }

    
    
    return o;
}
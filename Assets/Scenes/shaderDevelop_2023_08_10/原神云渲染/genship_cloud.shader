Shader "genship/genship_cloud"
{
    Properties
    {
        [Header(_IrradianceMapR Rayleigh Scatter)]
        _upPartSunColor ("高空近太阳颜色", Color) = (0.00972, 0.02298, 0.06016)
        _upPartSkyColor ("高空远太阳颜色", Color) = (0.00972, 0.02298, 0.06016)
        _downPartSunColor ("水平线近太阳颜色", Color) = (0.0538, 0.09841, 0.2073  )
        _downPartSkyColor ("水平线远太阳颜色", Color) = (0.0538, 0.09841, 0.2073  )
        _mainColorSunGatherFactor ("近太阳颜色聚集程度", Range(0, 1)) = 0.49336 // _58._m9
        _IrradianceMapR_maxAngleRange ("天空主色垂直变化范围", Range(0, 1)) = 0.20     // _58._m10
        
        [Header(_IrradianceMapG Mie Scatter)]
        _SunAdditionColor ("太阳追加点颜色", Color) = (0.00837, 0.10516, 0.26225) // _58._m11
        _SunAdditionIntensity ("太阳追加点颜色强度", Range(0, 3)) = 0.50 // _58._m12
        _IrradianceMapG_maxAngleRange ("太阳追加点垂直变化范围", Range(0, 1)) = 0.30 // _58._m13
        
        [Header(Sun Disk)]
        _sun_disk_power_999 ("太阳圆盘power", Range(0, 1000)) = 8.30078 // _58._m18
        _sun_color ("太阳圆盘颜色", Color) = (0.01938, 0.00651, 0.02122) // _58._m19
        _sun_color_intensity ("太阳圆盘颜色强度", Range(0, 10)) = 0.01039 // _58._m20
        _sun_shine_color ("_sun_shine_color", Color) = (0.01938, 0.00651, 0.02122  ) // _58._m15
        
        _IrradianceMap ("_IrradianceMap", 2D) = "white" {}
        
        [Header(Moon)]
        _moon_intensity_slider ("月亮大小0.5最大", Range(0, 1)) = 0.50    // _58._m25
        _moon_shine_color ("_moon_shine_color", Color) = (0.29669, 0.64985, 1.00 ) // _58._m22
        _moon_intensity_max ("_moon_intensity_max", Range(0, 1)) = 0.19794 // _58._m24s
        
        [Header(Transmission)]
        _SunTransmission ("_SunTransmission", Range(0, 10)) = 4.09789 // _58._m16
        _MoonTransmission ("_MoonTransmission", Range(0, 10)) = 3.29897 // _58._m23
        _TransmissionLDotVStartAt ("_TransmissionLDotVStartAt", Range(0, 1)) = 0.80205 // _58._m17
        
        [Header(Cloud)]
        
        _CloudColor_Bright_Center ("云亮部近太阳颜色", Color) = (0.05199, 0.10301, 0.13598) // _58._m27
        _CloudColor_Bright_Around ("云亮部远太阳颜色", Color) = (0.10391, 0.41824, 0.88688) // _58._m28
        _CloudColor_Dark_Center ("云暗部近太阳颜色", Color) = (0.00, 0.03576, 0.12083   ) // _58._m29
        _CloudColor_Dark_Around ("云暗部远太阳颜色", Color) = (0.02281, 0.05716, 0.14666) // _58._m30
        
        _LDotV_damping_factor_cloud ("云近太阳颜色聚集程度", float) = 0.0881 // _58._m31
        
        _CloudMoreBright ("云增亮", Range(0, 1)) = 0.8299 // _58._m34
        
        _DisturbanceNoiseOffset ("云扰动贴图偏移值", float) = 262.33862 // _58._m26
        _DisturbanceScale ("云扰动贴图偏移值缩放", float) = 0.0123
        
//        _affectWidthColorB ("_affectWidthColorB", Range(0, 1)) = 0.09804
//        _affectWidthColorAlpha ("_affectWidthColorAlpha", Range(0, 1)) = 0.09804
//        _w_input ("_w_input", Range(0, 1)) = 0.09804
        
        
        
        _NoiseMapRGB ("_NoiseMapRGB", 2D) = "white" {}
        _MaskMapRGBA ("_MaskMapRGBA", 2D) = "white" {}
        
        [Header(Misc)]
        _sun_dir ("_sun_dir", Vector) = (0.00688, -0.84638, -0.53253)
        _moon_dir ("_moon_dir", Vector) = (0.31638, 0.70655, 0.633)

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "genship_cloud_vert.hlsl"
            #include "genship_cloud_frag.hlsl"

            ENDCG
        }
    }
}

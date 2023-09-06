Shader "xue/genship_skybox"
{
    Properties
    {
        // /* day 白天: sun sky 属性 start
        _sunScatterColorLookAt("sunScatterColorLookAt", Color) = (0.00326,0.18243,0.63132,1)
        _sunScatterColorBeside("sunScatterColorBeside", Color) = (0.02948,0.1609,0.27936,1)
        _sunOrgColorLookAt("sunOrgColorLookAt", Color) = (0.30759,0.346,0.24592,1)
        _sunOrgColorBeside("sunOrgColorBeside", Color) = (0.04305,0.26222,0.46968,1)
        
        _sun_disk_power_999("sun_disk_power_999", Range(0, 1000)) = 1000
        _sun_color("sun_color", Color) = (0.90625, 0.43019, 0.11743, 1)
        _sun_color_intensity("sun_color_intensity", Range(0, 3)) = 1.18529
        
        _LDotV_damping_factor("LDotV_damping_factor", Range(0, 1)) = 0.31277
        _sun_scatter("sun_scatter", Range(0, 1)) = 0.44837
        _sky_color("sky_color", Color) = (0.90409,0.7345,0.13709, 1)
        _sky_color_intensity("sky_color_intensity", Range(0, 3)) = 1.48499
        _sky_scatter("sky_scatter", Range(0, 1)) = 0.69804
        
        [NoScaleOffset]_IrradianceMap("TransmissionRGMap", 2D) = "white" {}
        // -------------------------- */ 
        
        // /* night 夜晚: moon 属性 
        _moon_size("c_moon_size", Range(0, 1)) = 0.19794
        _moon_intensity_control01("c_moon_intensity_control01", Range(0, 4)) = 3.29897
        _moon_intensity_max("c_moon_intensity_max", Range(0, 1)) = 0.19794
        _moon_intensity_slider("c_moon_intensity_slider", Range(0, 1)) = 0.5
        _moon_color("moon_color", Color) = (0.15519, 0.18858, 0.2653, 1)
        // -------------------------- */
        
        // /* night 夜晚: star 属性 
        _starColorIntensity("starColorIntensity", Range(0, 10)) = 0.8466
        _starIntensityLinearDamping("starIntensityLinearDamping", Range(0, 1)) = 0.80829
        
        _StarDotMap("StarDotMap", 2D) = "white" {}
        [HideInInspector]_StarDotMap_ST("StarDotMap_ST", Vector) = (10,10,0,0)
        
        _StarColorLut("StarColorLut", 2D) = "white" {}
        [HideInInspector] _StarColorLut_ST("_NoiseMap_ST", Vector) = (0.5,1,0,0)
        
        _NoiseMap("NoiseMap", 2D) = "white" {}
        [HideInInspector] _NoiseMap_ST("_NoiseMap_ST", Vector) = (25,25,0,0)
        _NoiseSpeed("c_NoiseSpeed", Range( 0 , 1)) = 0.293
        // -------------------------- */
        
        // /* sun & moon dir
        _sun_dir("sun_dir", Vector) = (-0.26102,0.12177,-0.95762, 0)
        _moon_dir("moon_dir", Vector) = (-0.33274, -0.11934, 0.93544, 0)
        // -------------------------- */
        
        // /* misc 
        [Toggle]_star_part_enable("star_part_enable", Float) = 1
        [Toggle]_sun_part_enable("sun_part_enable", Float) = 1
        [Toggle]_moon_part_enable("moon_part_enable", Float) = 1
        // -------------------------- */
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // /* day 白天: sun sky 属性 start
            float3 _sunScatterColorLookAt;
            float3 _sunScatterColorBeside;
            float3 _sunOrgColorLookAt;
            float3 _sunOrgColorBeside;

            float _sun_disk_power_999;
            float3 _sun_color;
            float _sun_color_intensity;

            float _LDotV_damping_factor;
            float _sun_scatter;
            float3 _sky_color;
            float _sky_color_intensity;
            float _sky_scatter;
            
            sampler2D _IrradianceMap;
            // -------------------------- */
            
            // /* night 夜晚: moon 属性 
            float _moon_size;
            float _moon_intensity_control01;
            float _moon_intensity_max;
            float _moon_intensity_slider;
            float3 _moon_color;
            // -------------------------- */
            
            // /* night 夜晚: star 属性 
            float _starColorIntensity;
            float _starIntensityLinearDamping;
            
            sampler2D _StarDotMap;
            float4 _StarDotMap_ST;

            sampler2D _StarColorLut;
            float4 _StarColorLut_ST;
            
            sampler2D _NoiseMap;
            float4 _NoiseMap_ST;
            float _NoiseSpeed;
            // -------------------------- */
            
            // /* sun & moon dir
            float3 _moon_dir;
            float3 _sun_dir;
            // -------------------------- */
            
            // /* misc 
            float _star_part_enable;
            float _sun_part_enable;
            float _moon_part_enable;
            // -------------------------- */
            
			#include "UnityCG.cginc"
            #include ".\sky_common.hlsl"

            #define _UpDir float3(0,1,0)
            

            float FastAcosForAbsCos(float in_abs_cos) {
                float _local_tmp = ((in_abs_cos * -0.0187292993068695068359375 + 0.074261002242565155029296875) * in_abs_cos - 0.212114393711090087890625) * in_abs_cos + 1.570728778839111328125;
                return _local_tmp * sqrt(1.0 - in_abs_cos);
            }

            float FastAcos(float in_cos) {
                float local_abs_cos = abs(in_cos);
                float local_abs_acos = FastAcosForAbsCos(local_abs_cos);
                return in_cos < 0.0 ?  UNITY_PI - local_abs_acos : local_abs_acos;
            }

            // 兼容原本的 GetFinalMiuResult(float u)
            // 真正的含义是 acos(u) 并将 angle 映射到 up 1，middle 0，down -1
            float GetFinalMiuResult(float u)
            {

                float _acos = FastAcos(u);
                
                // tmp0 = HALF_PI - tmp0;
                // float _angle_up_to_down_1_n1 = (HALF_PI - tmp0) * INV_HALF_PI;
                float angle1_to_n1 = (UNITY_HALF_PI - _acos) * UNITY_INV_HALF_PI;
                return angle1_to_n1;
            }

            float3 GetLightDir()
            {
                // return normalize(_MainLightPosition.xyz);
                return normalize(_sun_dir);
            }

            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 Varying_StarColorUVAndNoise_UV : TEXCOORD0;
                float4 Varying_NoiseUV_large          : TEXCOORD1;
                float4 Varying_ViewDirAndAngle1_n1    : TEXCOORD2;
                float4 Varying_IrradianceColor    : TEXCOORD3;
                
                float4 vertex : SV_POSITION;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                float3 _worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0)).xyz;
				float4 _clippos = mul( UNITY_MATRIX_VP, float4(_worldPos, 1.0) );
                o.vertex = _clippos;

                o.Varying_StarColorUVAndNoise_UV.xy = TRANSFORM_TEX(v.uv, _StarDotMap);
                o.Varying_StarColorUVAndNoise_UV.zw = v.uv * 20.0;

                float4 _timeScaleValue = _Time.y * _NoiseSpeed * float4(0.4, 0.2, 0.1, 0.5);
                
                o.Varying_NoiseUV_large.xy = (v.uv * _NoiseMap_ST.xy) + _timeScaleValue.xy;
                o.Varying_NoiseUV_large.zw = (v.uv * _NoiseMap_ST.xy * 2.0) + _timeScaleValue.zw;


                // 实际用低位效果与 云 一致，
                #define _RolePos_maybe float3(-3.48413, 195.00, 2.47919)
                float3 _viewDir = normalize(_worldPos.xyz - _RolePos_maybe /*_WorldSpaceCameraPos*/);


                float _VDotSun = dot(_sun_dir, _viewDir.xyz);
                float _VDotSunRemap01Clamp = clamp((_VDotSun * 0.5) + 0.5, 0.0, 1.0);  // f(x)
                
                float _miu = clamp( dot(_UpDir, _viewDir.xyz), -1, 1 );
                
                float _angle_up_to_down_1_n1 = (UNITY_HALF_PI - FastAcos(_miu)) * UNITY_INV_HALF_PI;

                // o.Varying_ViewDirAndAngle1_n1
                {
                    o.Varying_ViewDirAndAngle1_n1.xyz = _viewDir;
                    o.Varying_ViewDirAndAngle1_n1.w = _angle_up_to_down_1_n1;
                }

                // 注1：_irradianceMapR 关联应该是天空颜色，因为 _irradianceMapR 采样为 0 时，是有值的，lerp(_sunScatterColorBeside, _sunScatterColorLookAt, _VDotSunDampingA_pow3)
                //   故输入的 4 个颜色，分两组应该分上下，其中上部分(抬头)对应 _irradianceMapR 采样为0，下部分(地平线)对应 _irradianceMapR 采样为1
                // 注2：_irradianceMapG 关联应该是太阳disk追加颜色，因为 _irradianceMapG 采样为 0 时，是没值的，

                // _irradianceMapR 最左边是 0 度的，最右边是 _sky_scatter 0.2 *90°=18° 的，即只记录水平朝向的值，更高，更低的值都是 18° 的值。
                //   如果 _sun_scatter 小，例如 0.01，则 1° 以上就没值了，表示 _sunPartColor (sun color) 只有水平地方有
                //   如果 _sun_scatter 小，例如 1.0，则 90° 应该还有值，表示 _sunPartColor (sun color) 高处也有
                float2 _irradianceMap_R_uv;
                    _irradianceMap_R_uv.x = abs(_angle_up_to_down_1_n1) / max(_sun_scatter, 1.0e-04);
                    _irradianceMap_R_uv.y = 0.5;
                
                float _irradianceMapR = tex2Dlod(_IrradianceMap, float4( _irradianceMap_R_uv, 0.0, 0.0 )).x;
                
                float _VDotSunDampingA = max(0, lerp( 1, _VDotSun, _LDotV_damping_factor ));
                
                float _VDotSunDampingA_pow3 = _VDotSunDampingA * _VDotSunDampingA * _VDotSunDampingA;

                float3 _sunOrgColor_adapt_LDotV = lerp(_sunOrgColorBeside, _sunOrgColorLookAt, _VDotSunDampingA_pow3);
                float3 _sunScatterColor_adapt_LDotV = lerp(_sunScatterColorBeside, _sunScatterColorLookAt, _VDotSunDampingA_pow3);
                
                float3 _sunPartColor = lerp(_sunScatterColor_adapt_LDotV, _sunOrgColor_adapt_LDotV, _irradianceMapR);

                
                // _irradianceMapG 最左边是 0 度的，最右边是 _sky_scatter 0.3 *90°=27° 的，即只记录水平朝向的值，更高，更低的值都是 27° 的值。
                //   如果 _sky_scatter 小，例如 0.01，则 1° 以上就没值了，表示 _skyPartColor (sky color) 只有水平地方有
                //   如果 _sky_scatter 小，例如 1.0，则 90° 应该还有值，表示 _skyPartColor (sky color) 高处也有
                float2 _irradianceMap_G_uv;
                    _irradianceMap_G_uv.x = abs(_angle_up_to_down_1_n1) / max(_sky_scatter, 1.0e-04);
                    _irradianceMap_G_uv.y = 0.5;
                float _irradianceMapG = tex2Dlod(_IrradianceMap, float4( _irradianceMap_G_uv, 0.0, 0.0 )).y;
                // sky color
                float3 _skyPartColor = _irradianceMapG * _sky_color * _sky_color_intensity;

                

                // smoothstep(0, 1, clamp( (abs(x)-0.2) * 10/3, 0, 1))
                // 从 0.2 处离开0，平滑上升，0.5 处开始达到最大 1.0 
                float _upFactor = smoothstep(0, 1, clamp((abs(_sun_dir.y) - 0.2) * 10/3, 0, 1));
                
                // smoothstep(0, 1, max((clamp(x, 0.0, 1.0)-1)/0.7 + 1, 0.0))
                // y=x 直线，固定 (1, 1) 点不动，旋转，使其斜率变成 1/0.7，加速衰减，并 smooth
                float _VDotSunFactor = smoothstep(0, 1, (_VDotSunRemap01Clamp-1)/0.7 + 1);

                // 意思是优先判断高度，高的地方就是全额 _skyPartColor
                //       lightDirY > 0.5 处是 1.0 
                //       lightDirY < 0.2 处是 _VDotSunFactor
                float _skyPartFactor = lerp(_VDotSunFactor, 1.0, _upFactor);


                float3 _sumIrradianceRGColor = _skyPartColor * _skyPartFactor + _sunPartColor;
                o.Varying_IrradianceColor.xyz = _sumIrradianceRGColor;
                o.Varying_IrradianceColor.w = _VDotSunDampingA_pow3;  // 这个实际没用
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                
                float3 _viewDirNormalize = normalize(i.Varying_ViewDirAndAngle1_n1.xyz);
                float _VDotUp = dot(_viewDirNormalize, _UpDir);
                
                float _VDotUp_Multi999 = abs(_VDotUp) * _sun_disk_power_999;

                // float3 _lightDir = GetLightDir();
                float3 _lightDir = _sun_dir;
                float _VDotSun = dot(_lightDir, _viewDirNormalize);
                float _MoonDotV = dot(_moon_dir, _viewDirNormalize);

                _MoonDotV = clamp(_MoonDotV, 0.0, 1.0);
                
                float _LDotV_remap01 = (_VDotSun * 0.5) + 0.5;
                _LDotV_remap01 = clamp(_LDotV_remap01, 0.0, 1.0);

                float _LDotV_Pow_0_1  = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.1);
                float _LDotV_Pow_0_01 = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.01);
                float _LDotV_Pow      = pow(_LDotV_remap01, _VDotUp_Multi999);
                _LDotV_Pow_0_1        = min(_LDotV_Pow_0_1, 1.0);
                _LDotV_Pow_0_01       = min(_LDotV_Pow_0_01, 1.0);
                _LDotV_Pow            = min(_LDotV_Pow, 1.0);

                float _LDotV_Pow_Scale = (_LDotV_Pow_0_01 * 0.03) + (_LDotV_Pow_0_1 * 0.12) + _LDotV_Pow.x;
                float3 _sun_disk = _LDotV_Pow_Scale * _sun_color_intensity * _sun_color;

                float _LDotV_smooth = smoothstep(0, 1, _VDotSun);
                
                float3 _sun_part_color = (_LDotV_smooth * _sun_disk * _sun_part_enable) + i.Varying_IrradianceColor.xyz;
                
                float _moon_size_rcp = 1.0 / max(_moon_size * 0.1, 0.00001);

                float _moon_disk = (_MoonDotV - 1.0) * _moon_size_rcp + 1.0;
                _moon_disk = max(_moon_disk, 0.0);
                
                float _moon_disk_pow2 = _moon_disk * _moon_disk;
                float _moon_disk_pow4 = _moon_disk_pow2 * _moon_disk_pow2;
                float _moon_disk_pow6 = _moon_disk_pow4 * _moon_disk_pow2;
                
                // 上箭头形状，0.5 最高 是 1，左右 0、1 是 0
                float _moon_slider_value = -abs(_moon_intensity_slider - 0.5) * 2.0 + 1.0;
                // #define _moon_intensity_max  (0.19794)       // _43._m9
                float _moon_intensity = _moon_slider_value * _moon_intensity_max * _moon_disk_pow6;

                float3 _moon_part_color = _moon_intensity * _moon_color;
                
                float _is_no_moon_here = float((_moon_intensity * _moon_part_enable) <= 0.05);
                
                _moon_part_color = clamp(_moon_intensity_control01, 0.0, 1.0) * _moon_part_color;
                float3 _sun_moon_color = (_moon_part_color * _moon_part_enable) + _sun_part_color;

                float _starExistNoise1 = tex2D(_NoiseMap, i.Varying_NoiseUV_large.xy).r;
                float _starExistNoise2 = tex2D(_NoiseMap, i.Varying_NoiseUV_large.zw).r;
                float _starSample = tex2D(_StarDotMap, i.Varying_StarColorUVAndNoise_UV.xy).r;
                float _star = _starSample * _starExistNoise2 * _starExistNoise1;
                float _miuResult = i.Varying_ViewDirAndAngle1_n1.w * 1.5;
                _miuResult = clamp(_miuResult, 0.0, 1.0);
                float _star_intensity = _star * _miuResult;
                _star_intensity *= 3.0;
                
                // _starIntensityLinearDamping 星星亮度直线下降，越大越不亮
                float _starColorNoise = tex2D(_NoiseMap, i.Varying_StarColorUVAndNoise_UV.zw).r;
                float _starIntensityDamping = (_starColorNoise - _starIntensityLinearDamping) / (1.0 -_starIntensityLinearDamping);
                _starIntensityDamping = clamp(_starIntensityDamping, 0.0, 1.0);
                _star_intensity = _starIntensityDamping * _star_intensity;
                
                float2 _starColorLutUV;
                _starColorLutUV.x = (_starColorNoise * _StarColorLut_ST.x) + _StarColorLut_ST.z;
                _starColorLutUV.y = 0.5;
                float3 _starColorLut = tex2D(_StarColorLut, _starColorLutUV).xyz;
                float3 _starColor = _starColorLut * _starColorIntensity;

                float3 _finalStarColor = _star_intensity * _starColor;
                _finalStarColor = _is_no_moon_here * _finalStarColor;

                float3 _finalColor = _finalStarColor * _star_part_enable + _sun_moon_color;
                // _finalColor = Gamma22ToLinear(_finalColor);
                return float4(_finalColor, 1);
            }
            ENDHLSL
        }
    }
}

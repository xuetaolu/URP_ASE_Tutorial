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
        
        [NoScaleOffset]_TransmissionRGMap("TransmissionRGMap", 2D) = "white" {}
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
            
            sampler2D _TransmissionRGMap;
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
            
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

            #define _const_up_vector float3(0,1,0)
            
            float MiuLut(float u) {
                u = abs(u);
                return ((u * (-0.0187292993068695068359375) + 0.074261002242565155029296875) 
                    * u + (-0.212114393711090087890625))
                        * u + 1.570728778839111328125;
            }

            float GetFinalMiuResult(float u) {
                float _miuLut = MiuLut(u);
                float _sqrtOneMinusMiu = sqrt(1.0 - abs(u));
                float _sqrtOneMinusMiu_multi_lut = _sqrtOneMinusMiu * _miuLut;
                float tmp0 = u < 0 ? (_sqrtOneMinusMiu_multi_lut * (-2.0)) + 3.1415927410125732421875 : 0.0;

                tmp0 = (_sqrtOneMinusMiu_multi_lut) + tmp0;
                tmp0 = (-tmp0) + 1.57079637050628662109375;
                float finalMiuResult = tmp0 * 0.6366198062896728515625;
                return finalMiuResult;
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
                float4 Varying_ViewDirAndMiuResult    : TEXCOORD2;
                float4 Varying_ColorAndLDotDamping    : TEXCOORD3;
                
                float4 vertex : SV_POSITION;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                float3 _worldPos = TransformObjectToWorld( v.vertex.xyz );
				float4 _clippos = TransformWorldToHClip( _worldPos );
                o.vertex = _clippos;

                o.Varying_StarColorUVAndNoise_UV.xy = TRANSFORM_TEX(v.uv, _StarDotMap);
                o.Varying_StarColorUVAndNoise_UV.zw = v.uv * 20.0;

                float4 _timeScaleValue = _Time.y * _NoiseSpeed * float4(0.4, 0.2, 0.1, 0.5);
                
                o.Varying_NoiseUV_large.xy = (v.uv * _NoiseMap_ST.xy) + _timeScaleValue.xy;
                o.Varying_NoiseUV_large.zw = (v.uv * _NoiseMap_ST.xy * 2.0) + _timeScaleValue.zw;

                float3 _viewDir = normalize(_worldPos.xyz - _WorldSpaceCameraPos);
                // float3 _viewDir = normalize(_worldPos.xyz);
                o.Varying_ViewDirAndMiuResult.xyz = float3( _viewDir.x, _viewDir.y, _viewDir.z );

                float3 _lightDir = GetLightDir();
                float _LDotV = dot(_lightDir, _viewDir.xyz);
                float _miu = clamp( dot(_const_up_vector, _viewDir.xyz), -1, 1 );
                float finalMiuResult = GetFinalMiuResult(_miu);
                o.Varying_ViewDirAndMiuResult.w = finalMiuResult;
                
                float _lightDir_y_remap = clamp((abs(_lightDir.y) - 0.2) * 10.0/3.0, 0.0, 1.0);
                _lightDir_y_remap = smoothstep(0, 1, _lightDir_y_remap);
                
                float _LDotV_remap = clamp((_LDotV * 0.5) + 0.5, 0.0, 1.0);  // f(x)
                _LDotV_remap = max(_LDotV_remap * 1.4285714626312255859375 - 0.42857145581926658906013, 0);         // g(x)

                float _LDotV01_smooth = smoothstep(0, 1, _LDotV_remap);

                // 优先考虑 LDotV 作为太阳强度权重，其次使用 太阳光 Y 的高度
                float _sun_T_color_Instensity = lerp(_LDotV01_smooth, 1, _lightDir_y_remap);

                float _sky_T = tex2Dlod(_TransmissionRGMap, float4( abs(finalMiuResult)/max(_sky_scatter, 0.0001), 0.5, 0.0, 0.0 )).y;
                float3 _sky_T_color = _sky_T * _sky_color * _sky_color_intensity;
                float _sun_T = tex2Dlod(_TransmissionRGMap, float4( abs(finalMiuResult)/max(_sun_scatter, 0.0001), 0.5, 0.0, 0.0 )).x;

                float _cubic_LDotV_damping = lerp( 1, _LDotV, _LDotV_damping_factor );
                _cubic_LDotV_damping = max(_cubic_LDotV_damping, 0);
                _cubic_LDotV_damping = _cubic_LDotV_damping * _cubic_LDotV_damping * _cubic_LDotV_damping;

                float3 _sunOrgColor_adapt_LDotV = lerp(_sunOrgColorBeside, _sunOrgColorLookAt, _cubic_LDotV_damping);
                float3 _sunScatterColor_adapt_LDotV = lerp(_sunScatterColorBeside, _sunScatterColorLookAt, _cubic_LDotV_damping);
                
                float3 _sunFinalColor = lerp(_sunScatterColor_adapt_LDotV, _sunOrgColor_adapt_LDotV, _sun_T);
                float3 _final_color = _sky_T_color * _sun_T_color_Instensity + _sunFinalColor;
                o.Varying_ColorAndLDotDamping = float4(_final_color, _cubic_LDotV_damping);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // float4 value = float4(0,0,0,1);
                // value.rgb = i.Varying_ColorAndLDotDamping.rgb;
                // return value;
                
                float3 _viewDirNormalize = normalize(i.Varying_ViewDirAndMiuResult.xyz);
                float _VDotUp = dot(_viewDirNormalize, _const_up_vector);
                
                float _VDotUp_Multi999 = abs(_VDotUp) * _sun_disk_power_999;

                float3 _lightDir = GetLightDir();
                float _LDotV = dot(_lightDir, _viewDirNormalize);
                float _MoonDotV = dot(_moon_dir, _viewDirNormalize);

                _MoonDotV = clamp(_MoonDotV, 0.0, 1.0);
                
                float _LDotV_remap01 = (_LDotV * 0.5) + 0.5;
                _LDotV_remap01 = clamp(_LDotV_remap01, 0.0, 1.0);

                float _LDotV_Pow_0_1  = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.1);
                float _LDotV_Pow_0_01 = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.01);
                float _LDotV_Pow      = pow(_LDotV_remap01, _VDotUp_Multi999);
                _LDotV_Pow_0_1        = min(_LDotV_Pow_0_1, 1.0);
                _LDotV_Pow_0_01       = min(_LDotV_Pow_0_01, 1.0);
                _LDotV_Pow            = min(_LDotV_Pow, 1.0);

                float _LDotV_Pow_Scale = (_LDotV_Pow_0_01 * 0.03) + (_LDotV_Pow_0_1 * 0.12) + _LDotV_Pow.x;
                float3 _sun_disk = _LDotV_Pow_Scale * _sun_color_intensity * _sun_color;

                float _LDotV_smooth = smoothstep(0, 1, _LDotV);
                
                float3 _sun_part_color = (_LDotV_smooth * _sun_disk * _sun_part_enable) + i.Varying_ColorAndLDotDamping.xyz;
                
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
                float _miuResult = i.Varying_ViewDirAndMiuResult.w * 1.5;
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

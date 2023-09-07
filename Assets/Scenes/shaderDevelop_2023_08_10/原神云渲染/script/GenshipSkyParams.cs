// @author : xue
// @created : 2023,09,07,14:51
// @desc:

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神云渲染.script
{
    [ExecuteAlways]
    public class GenshipSkyParams : MonoBehaviour
    {
        public bool m_alwaysUpdate;
        [Header("_IrradianceMapR Rayleigh Scatter")]
        public Color _upPartSunColor = new Color(0.1000239f, 0.1459537f, 0.3625664f);
        public Color _upPartSkyColor = new Color(0.1041798f, 0.105746f, 0.1888139f);
        public Color _downPartSunColor = new Color(0.8657666f, 0.1706658f, 0.01984066f);
        public Color _downPartSkyColor = new Color(0.4774929f, 0.3099873f, 0.2036164f);
        [Range(0, 1)]
        public float _mainColorSunGatherFactor = 0.3044474f;
        [Range(0, 1)]
        public float _IrradianceMapR_maxAngleRange = 0.4586129f;

        [Header("_IrradianceMapG Mie Scatter")]
        public Color _SunAdditionColor = new Color(0.975133f, 0.7354551f, 0.3996257f);
        [Range(0, 3)]
        public float _SunAdditionIntensity = 1.206624f;
        [Range(0, 1)]
        public float _IrradianceMapG_maxAngleRange = 0.6607388f;

        [Header("Sun Disk")] 
        [Range(0, 1000)]
        public float _sun_disk_power_999 = 351.1657f;
        public Color _sun_color = new Color(0.891852f, 0.4535326f, 0.1378029f);
        [Range(0, 10)]
        public float _sun_color_intensity = 1.163067f;
        public Color _sun_shine_color = new Color(0.9558311f, 0.5051994f, 0.3502946f);

        [Header("Moon")] 
        [Range(0, 1)] 
        public float _moon_intensity_slider = 0.5f;
        public Color _moon_color = new Color(0.1884468f, 0.3297098f, 0.4767046f);
        public Color _moon_shine_color = new Color(0.07832335f, 0.0869457f, 0.103671f);

        [Header("Star")] 
        [Range(0, 1)]
        public float _starColorIntensity = 0.7692237f;
        [Range(0, 1)] 
        public float _starIntensityLinearDamping = 0.002921373f;

        [Header("Cloud Transmission")] 
        [Range(0, 10)]
        public float _SunTransmission = 8.325501f;
        [Range(0, 10)]
        public float _MoonTransmission = 1.706602f;
        [Range(0, 1)]
        public float _TransmissionLDotVStartAt = 0.8990098f;
        
        [Header("Cloud Color")]
        public Color _CloudColor_Bright_Center = new Color(0.8181017f, 0.3913187f, 0.2937731f);
        public Color _CloudColor_Bright_Around = new Color(0.907616f, 0.5119854f, 0.3548318f);
        public Color _CloudColor_Dark_Center = new Color(0.2369859f, 0.2321503f, 0.2780426f);
        public Color _CloudColor_Dark_Around = new Color(0.04807252f, 0.1417156f, 0.3348806f);
        [Range(0, 1)]
        public float _LDotV_damping_factor_cloud = 0.2860512f;
        [Range(0, 1)]
        public float _CloudMoreBright = 0.481812f;
        public float _DisturbanceNoiseOffset = 55.26278f;


        [Header("Misc")]
        public Vector3 _sun_dir = new Vector3(0.4804857f, 0.1503147f, -0.8640248f);
        public Vector3 _moon_dir = new Vector3(-0.7006048f, -0.06914657f, -0.7101912f);


        [Header("OtherControl")] 
        public Transform _sunTransform;
        public Transform _moonTransform;

        [Header("Materials")] 
        public List<Renderer> _skyRenderers = new List<Renderer>();
        public List<Renderer> _cloudRenderers = new List<Renderer>();

        private void SetCommonProperties(MaterialPropertyBlock mpb)
        {
            mpb.SetColor(nameof(_upPartSunColor), _upPartSunColor);
            mpb.SetColor(nameof(_upPartSkyColor), _upPartSkyColor);
            mpb.SetColor(nameof(_downPartSunColor), _downPartSunColor);
            mpb.SetColor(nameof(_downPartSkyColor), _downPartSkyColor);
            
            mpb.SetFloat(nameof(_mainColorSunGatherFactor), _mainColorSunGatherFactor);
            mpb.SetFloat(nameof(_IrradianceMapR_maxAngleRange), _IrradianceMapR_maxAngleRange);
            
            
            mpb.SetColor(nameof(_SunAdditionColor), _SunAdditionColor);
            mpb.SetFloat(nameof(_SunAdditionIntensity), _SunAdditionIntensity);
            mpb.SetFloat(nameof(_IrradianceMapG_maxAngleRange), _IrradianceMapG_maxAngleRange);
            
            mpb.SetFloat(nameof(_sun_disk_power_999), _sun_disk_power_999);
            mpb.SetColor(nameof(_sun_color), _sun_color);
            mpb.SetFloat(nameof(_sun_color_intensity), _sun_color_intensity);
            
            mpb.SetFloat(nameof(_moon_intensity_slider), _moon_intensity_slider);
            mpb.SetColor(nameof(_moon_color), _moon_color);
            
            
            mpb.SetVector(nameof(_sun_dir), _sun_dir);
            mpb.SetVector(nameof(_moon_dir), _moon_dir);
        }

        private void SetSkySphereProperties(MaterialPropertyBlock mpb)
        {
            mpb.SetFloat(nameof(_starColorIntensity), _starColorIntensity);
            mpb.SetFloat(nameof(_starIntensityLinearDamping), _starIntensityLinearDamping);
        }
        
        private void SetCloudProperties(MaterialPropertyBlock mpb)
        {
            mpb.SetColor(nameof(_sun_shine_color), _sun_shine_color);
            mpb.SetColor(nameof(_moon_shine_color), _moon_shine_color);
            
            mpb.SetFloat(nameof(_SunTransmission), _SunTransmission);
            mpb.SetFloat(nameof(_MoonTransmission), _MoonTransmission);
            mpb.SetFloat(nameof(_TransmissionLDotVStartAt), _TransmissionLDotVStartAt);
            
            mpb.SetColor(nameof(_CloudColor_Bright_Center), _CloudColor_Bright_Center);
            mpb.SetColor(nameof(_CloudColor_Bright_Around), _CloudColor_Bright_Around);
            mpb.SetColor(nameof(_CloudColor_Dark_Center), _CloudColor_Dark_Center);
            mpb.SetColor(nameof(_CloudColor_Dark_Around), _CloudColor_Dark_Around);
            
            mpb.SetFloat(nameof(_LDotV_damping_factor_cloud), _LDotV_damping_factor_cloud);
            mpb.SetFloat(nameof(_CloudMoreBright), _CloudMoreBright);
            mpb.SetFloat(nameof(_DisturbanceNoiseOffset), _DisturbanceNoiseOffset);
        }

        private void Update()
        {
            if (!m_alwaysUpdate)
                return;
            
            Vector3 pos = transform.position;
            if (_sunTransform != null)
            {
                Vector3 sun_dir = (_sunTransform.position - pos).normalized;
                _sun_dir = sun_dir;
            }

            if (_moonTransform != null)
            {
                Vector3 moon_dir = (_moonTransform.position - pos).normalized;
                _moon_dir = moon_dir;
            }

            foreach (var renderer1 in _skyRenderers)
            {
                if (renderer1 == null)
                    continue;
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                renderer1.GetPropertyBlock(mpb);

                SetCommonProperties(mpb);
                SetSkySphereProperties(mpb);

                renderer1.SetPropertyBlock(mpb);
            }
            
            foreach (var renderer1 in _cloudRenderers)
            {
                if (renderer1 == null)
                    continue;
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                renderer1.GetPropertyBlock(mpb);

                SetCommonProperties(mpb);
                SetCloudProperties(mpb);

                renderer1.SetPropertyBlock(mpb);
            }
        }


    }
}
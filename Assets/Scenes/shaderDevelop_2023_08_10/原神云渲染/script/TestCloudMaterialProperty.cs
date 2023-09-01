// @author : xue
// @created : 2023,09,01,10:12
// @desc:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.shaderDevelop_2023_08_10.原神云渲染.script
{
    public class SpecialAttribute : Attribute
    {
        
    }

    [ExecuteAlways]
    public class TestCloudMaterialProperty : MonoBehaviour
    {
        public static List<Tuple<string, string, Type>> s_propertyMap = new List<Tuple<string, string, Type>>()
        {
            new ("_sunScatterColorLookAt", "_sunScatterColorLookAt",            typeof(Color)),
           
            new ("_sunScatterColorBeside", "_sunScatterColorBeside",            typeof(Color)),
            new ("_sunOrgColorLookAt",     "_sunOrgColorLookAt",            typeof(Color)),
            new ("_sunOrgColorBeside",     "_sunOrgColorBeside",            typeof(Color)),
            
            // new ("_LDotV_damping_factor", "_LDotDir_n11_RemapDownAt0_A", typeof(float)),
            
            new ("_sun_dir" , "_sun_dir",                         typeof(Vector4)),
            new ("_moon_dir", "_moon_dir",                                 typeof(Vector4)),
        };
        
        
        public Renderer m_skyboxRenderer;

        public List<Renderer> m_cloudRenderers = new List<Renderer>();

        public bool m_setCloudSpecialProperty = true;
        
        
        [Special] public float _LDotDir_n11_RemapDownAt0_A;
        
        [Special] public float _IrradianceMapR_maxAngleRange;
        [Special] public Color _IrradianceMapG_Color;
        [Special] public float _IrradianceMapG_Intensity;
        [Special] public float _IrradianceMapG_maxAngleRange;
        
        [Special] public Vector4 _58__m15;
        [Special] public float _58__m16;
        [Special] public float _58__m17;
        [Special] public float _UpIrradianceFadePow;
        [Special] public Vector4 _58__m19;
        [Special] public float _58__m20;
        
        
        [Special] public Vector4 _58__m22;
        [Special] public float _58__m23;
        [Special] public float _58__m24;
        [Special] public float _01_RemapTo_Center1_TwoSide0;
        [Special] public float _DisturbanceNoiseOffset2;


        [Special] public Color _CloudColor_Bright_Center = new Color(0.05199f, 0.10301f, 0.13598f);
        [Special] public Color _CloudColor_Bright_Around = new Color(0.10391f, 0.41824f, 0.88688f);
        [Special] public Color _CloudColor_Dark_Center = new Color(0.00f, 0.03576f, 0.12083f   );
        [Special] public Color _CloudColor_Dark_Around = new Color(0.02281f, 0.05716f, 0.14666f);

        [Special] public float _LDotDir_n11_RemapDownAt0_B = 0.0881f;

        [Special] public float _58__m34;

        private void Update()
        {

            if (m_cloudRenderers == null || m_cloudRenderers.Select(v => v != null).Count() <= 0)
                return;
            
            MaterialPropertyBlock toMpb = new MaterialPropertyBlock();

            if (m_skyboxRenderer != null)
            {
                MaterialPropertyBlock fromMpb = new MaterialPropertyBlock();
                m_skyboxRenderer.GetPropertyBlock(fromMpb);
                Material fromMaterial = m_skyboxRenderer.sharedMaterial;
                
                foreach (var args in s_propertyMap)
                {
                    (string fromProperty, string toProperty, Type type) = args;

                    if (type == typeof(Color))
                    {
                        toMpb.SetColor(toProperty, fromMpb.HasProperty(fromProperty) ? fromMpb.GetColor(fromProperty) : fromMaterial.GetColor(fromProperty));
                    }
                    else if (type == typeof(Vector4))
                    {
                        toMpb.SetVector(toProperty, fromMpb.HasProperty(fromProperty) ? fromMpb.GetVector(fromProperty): fromMaterial.GetVector(fromProperty));
                    }
                    else if (type == typeof(float))
                    {
                        toMpb.SetFloat(toProperty, fromMpb.HasProperty(fromProperty) ? fromMpb.GetFloat(fromProperty): fromMaterial.GetFloat(fromProperty));
                    }
                }
            }

            if (m_setCloudSpecialProperty)
            {
                FieldInfo[] fieldInfos = GetType().GetDeclaredFields();

                foreach (var fieldInfo in fieldInfos)
                {
                    SpecialAttribute specialAttribute = fieldInfo.GetCustomAttribute
                    (typeof(SpecialAttribute) , false) as SpecialAttribute;

                    if (specialAttribute != null)
                    {
                        Type type = fieldInfo.FieldType;

                        if (type == typeof(float))
                        {
                            toMpb.SetFloat(fieldInfo.Name, (float)fieldInfo.GetValue(this));
                        }
                        else if (type == typeof(Color))
                        {
                            toMpb.SetColor(fieldInfo.Name, (Color)fieldInfo.GetValue(this));
                        }
                        else if (type == typeof(Vector4))
                        {
                            toMpb.SetVector(fieldInfo.Name, (Vector4)fieldInfo.GetValue(this));
                        }
                    }
                }
                
                // toMpb.SetColor(nameof(_CloudColor_Bright_Center), _CloudColor_Bright_Center);
                // toMpb.SetColor(nameof(_CloudColor_Bright_Around), _CloudColor_Bright_Around);
                // toMpb.SetColor(nameof(_CloudColor_Dark_Center), _CloudColor_Dark_Center);
                // toMpb.SetColor(nameof(_CloudColor_Dark_Around), _CloudColor_Dark_Around);
                
                // toMpb.SetFloat(nameof(_LDotDir_n11_RemapDownAt0_B), _LDotDir_n11_RemapDownAt0_B);
            }


            foreach (var renderer in m_cloudRenderers)
            {
                if (renderer == null)
                    continue;
                
                if (toMpb.isEmpty)
                {
                    renderer.SetPropertyBlock(null);
                }
                else
                {
                    renderer.SetPropertyBlock(toMpb);
                }
            }
        }
    }
}
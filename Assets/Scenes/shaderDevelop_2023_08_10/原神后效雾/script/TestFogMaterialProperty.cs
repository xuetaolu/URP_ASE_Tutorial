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

    [ExecuteAlways]
    public class TestFogMaterialProperty : MonoBehaviour
    {
        public static List<Tuple<string, string, Type>> s_propertyMap = new List<Tuple<string, string, Type>>()
        {
            // new ("_sunScatterColorLookAt", "_sunScatterColorLookAt",            typeof(Color)),
            // new ("_sunScatterColorBeside", "_sunScatterColorBeside",            typeof(Color)),
            // new ("_sunOrgColorLookAt",     "_sunOrgColorLookAt",            typeof(Color)),
            // new ("_sunOrgColorBeside",     "_sunOrgColorBeside",            typeof(Color)),
            
            // new ("_upPartSunColor",       "_upPartSunColor",            typeof(Color)),
            // new ("_upPartSkyColor",       "_upPartSkyColor",            typeof(Color)),
            // new ("_downPartSunColor",     "_downPartSunColor",            typeof(Color)),
            // new ("_downPartSkyColor",     "_downPartSkyColor",            typeof(Color)),
            
            // new ("_mainColorSunGatherFactor", "_mainColorSunGatherFactor", typeof(float)),
            
            // new ("_sun_dir" , "_sun_dir",                         typeof(Vector4)),
            // new ("_moon_dir", "_moon_dir",                                 typeof(Vector4)),
        };
        
        
        public Renderer m_render;


        [Special] public Color _FogMainColor;
        
        [Special] public float _FogDistancePow;
        [Special] public Vector4 _ExpDampingScaleXZ_AffectYW;
        [Special] public Color _FogDistanceColor;
        [Special] public Vector4 _SkyFogDistanceScaleW_;
        [Special] public Vector4 _TerrainYSO_XY_TerrainDistanceSO_ZW_;
        [Special] public Color _FogColorC;
        [Special] public Vector4 _64__m10;
        [Special] public Vector4 _ExpDampingStartXZ_;
        

        private void Update()
        {

            if (m_render == null)
                return;
            
            MaterialPropertyBlock toMpb = new MaterialPropertyBlock();

            if (m_render != null)
            {
                MaterialPropertyBlock fromMpb = new MaterialPropertyBlock();
                m_render.GetPropertyBlock(fromMpb);
                Material fromMaterial = m_render.sharedMaterial;
                
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


            FieldInfo[] fieldInfos = GetType().GetDeclaredFields();
            
            foreach (var fieldInfo in fieldInfos)
            {
                SpecialAttribute specialAttribute = fieldInfo.GetCustomAttribute
                (typeof(SpecialAttribute) , false) as SpecialAttribute;
            
                if (specialAttribute != null)
                {
                    Type type = fieldInfo.FieldType;
            
                    string propertyName = specialAttribute.shaderPropertyName ?? fieldInfo.Name;
            
                    if (type == typeof(float))
                    {
                        toMpb.SetFloat(propertyName, (float)fieldInfo.GetValue(this));
                    }
                    else if (type == typeof(Color))
                    {
                        toMpb.SetColor(propertyName, (Color)fieldInfo.GetValue(this));
                    }
                    else if (type == typeof(Vector4))
                    {
                        toMpb.SetVector(propertyName, (Vector4)fieldInfo.GetValue(this));
                    }
                }
            }
                


            m_render.SetPropertyBlock(toMpb);
        }
    }
}
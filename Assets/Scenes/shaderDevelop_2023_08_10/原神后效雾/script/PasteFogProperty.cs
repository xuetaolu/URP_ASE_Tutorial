// @author : xue
// @created : 2023,09,01,14:25
// @desc:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神云渲染.script
{
    [ExecuteAlways]
    public class PasteFogProperty : MonoBehaviour
    {
        public TestFogMaterialProperty m_target;

        private void Start()
        {
            m_target = GetComponent<TestFogMaterialProperty>();
        }

        [TextArea(4, 10)] 
        public string m_text;

        public static Dictionary<int, string> m_propertyIdMap = new Dictionary<int, string>()
        {
            // { 3, "_FogMainColor" }
            { 4, "_ExpDampingScaleXZ_AffectYW" },
            { 6, "_FogDistanceColor" },
            { 7, "_SkyFogDistanceScaleW_" },
            { 8, "_TerrainYSO_XY_TerrainDistanceSO_ZW_" },
            { 9, "_FogColorC" },
            { 10, "_64__m10" },
            { 13, "_ExpDampingStartXZ_" },
            
        };
        
        private bool ProcessSpecial(int index, string[] fourpart)
        {
            string[] values = Regex.Split(fourpart[1].Trim(), @"[, ]+");

            // #define _FogMainColorA float4(_FogMainColor.xyz, _FogDistancePow.x)
            if (index == 3)
            {
                Vector4 value = ConvertStringsToVector(values);
                ProcessProperty("_FogMainColor", value);
                ProcessProperty("_FogDistancePow", value.w);
                return true;
            }

            return false;
        }

        public void Paste()
        {
            
            string[] lines = Regex.Split(m_text.Trim(), @"[\n\r]+");
            foreach (string line in lines)
            {
                string[] fourpart = Regex.Split(line.Trim(), @"(?<!,)[ ]+");
                
                if (fourpart.Count() != 4)
                    continue;

                int index;
                {
                    Match match = Regex.Match(fourpart[0].Trim(), @"\d+");
                    index = int.Parse(match.Value);
                }

                if (ProcessSpecial(index, fourpart))
                    continue;
                
                if (!m_propertyIdMap.ContainsKey(index))
                    continue;

                string propertyName = m_propertyIdMap[index];

                // value
                {
                    string[] values = Regex.Split(fourpart[1].Trim(), @"[, ]+");

                    if (values.Count() == 1)
                    {
                        float value = float.Parse(values[0]);
                        ProcessProperty(propertyName, value);
                    }
                    else
                    {
                        Vector4 value = ConvertStringsToVector(values);
                        ProcessProperty(propertyName, value);
                    }
                }
            }
        }



        private Vector4 ConvertStringsToVector(string[] values)
        {
            // Vector4 value = Vector4.zero;
            (float x, float y, float z, float w) = (0, 0, 0, 1);

            if (values.Count() >= 4)
            {
                w = float.Parse(values[3]);
            }
            if (values.Count() >= 3)
            {
                z = float.Parse(values[2]);
            }
            if (values.Count() >= 2)
            {
                y = float.Parse(values[1]);
            }
            if (values.Count() >= 1)
            {
                x = float.Parse(values[0]);
            }

            return new Vector4(x,y,z,w);
        }


        public void ProcessProperty(string name, Vector4 value)
        {
            if (m_target == null)
                return;

            FieldInfo fieldInfo = m_target.GetType().GetField(name);

            if (fieldInfo.FieldType == typeof(Color))
            {
                Color c = new Color(value.x, value.y, value.z, value.w);
                fieldInfo.SetValue(m_target, c);
            }
            else if (fieldInfo.FieldType == typeof(Vector4))
            {
                fieldInfo.SetValue(m_target, value);
            }
        }
        
        public void ProcessProperty(string name, float value)
        {
            if (m_target == null)
                return;
            
            m_target.GetType().GetField(name).SetValue(m_target, value);
        }
    }
}
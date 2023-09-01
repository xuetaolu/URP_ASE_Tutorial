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
    public class PasteCloudProperty : MonoBehaviour
    {
        public TestCloudMaterialProperty m_target;

        private void Start()
        {
            m_target = GetComponent<TestCloudMaterialProperty>();
        }

        [TextArea(4, 10)] 
        public string m_text;

        public static Dictionary<int, string> m_propertyIdMap = new Dictionary<int, string>()
        {
            { 9 , "_LDotDir_n11_RemapDownAt0_A" },
            { 10, "_IrradianceMapR_maxAngleRange" },
            { 11, "_IrradianceMapG_Color" },
            { 12, "_IrradianceMapG_Intensity" },
            { 13, "_IrradianceMapG_maxAngleRange" },
            { 15, "_58__m15" },
            { 16, "_58__m16" },
            { 17, "_58__m17" },
            { 18, "_UpIrradianceFadePow" },
            { 19, "_58__m19" },
            { 20, "_58__m20" },
            { 22, "_58__m22" },
            { 23, "_58__m23" },
            { 24, "_58__m24" },
            { 25, "_01_RemapTo_Center1_TwoSide0" },
            { 26, "_DisturbanceNoiseOffset2" },
            

            { 27, "_CloudColor_Bright_Center" },
            { 28, "_CloudColor_Bright_Around" },
            { 29, "_CloudColor_Dark_Center" },
            { 30, "_CloudColor_Dark_Around" },
            { 31, "_LDotDir_n11_RemapDownAt0_B" },
            
            { 34, "_58__m34" },
            
        };

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
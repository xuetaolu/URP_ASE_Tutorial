// @author : xue
// @created : 2023,09,11,10:23
// @desc:

using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散.script
{
    [ExecuteAlways]
    public class CalcVectorLength : MonoBehaviour
    {
        [TextArea(4,6)]
        public string m_text;
        public float length;

        public void Update()
        {
            String[] numbers = null;
            if (m_text!=null)    
                numbers = Regex.Split(m_text.Trim(), @"[\t\n\r,; ]+");

            int numlength = numbers != null ? numbers.Length : 0;
            if (2 <= numlength && numlength <= 4)
            {
                float[] nums = numbers.Select(v => float.Parse(v)).ToArray();
                if (numlength == 2)
                {
                    Vector2 vector2 = new Vector2(nums[0], nums[1]);
                    length = vector2.magnitude;
                }
                else if (numlength == 3)
                {
                    Vector3 vector3 = new Vector3(nums[0], nums[1], nums[2]);
                    length = vector3.magnitude;
                }
                else if (numlength == 4)
                {
                    Vector4 vector4 = new Vector4(nums[0], nums[1], nums[2], nums[3]);
                    length = vector4.magnitude;
                }
            }
            else
            {
                length = 0;
            }
        }
    }
}
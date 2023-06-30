// @author : xue
// @created : 2023,06,30,14:10
// @desc:

using System.Collections.Generic;
using UnityEngine;

namespace XBRZ
{
    /// <summary>
    /// XBRZ 工具
    /// </summary>
    public class XbrzUtils
    {
        public static readonly string s_xBRZShaderName = "xue/xBRZ";

        /// <summary>
        /// xBRZ 缩放倍率，x2 x4 x6
        /// </summary>
        public class EScaleMethod
        {
            public static readonly EScaleMethod X2 = new EScaleMethod() {_id = 0, _scale = 2};
            public static readonly EScaleMethod X4 = new EScaleMethod() {_id = 1, _scale = 4};
            public static readonly EScaleMethod X6 = new EScaleMethod() {_id = 2, _scale = 6};

            private int _id;
            private int _scale;

            private EScaleMethod()
            {
            }

            public int id => _id;
            public int scale => _scale;

            // public static implicit operator int(EScaleMethod eScaleMethod) {
            //     return eScaleMethod._id;
            // }
        }
        public static readonly int EScaleMethod_Num = 3;

        public static readonly string[] s_xBRZMaterialKeywrods = { "_SCALE_X2", "_SCALE_X4", "_SCALE_X6" };

        public static readonly Dictionary<int, EScaleMethod> s_scaleIntToEScaleMethod = new Dictionary<int, EScaleMethod>()
        {
            {2, EScaleMethod.X2},
            {4, EScaleMethod.X4},
            {6, EScaleMethod.X6},
        };

        /// <summary>
        /// xBRZ 材质
        /// </summary>
        private static Material[] s_xBRZMaterials;

        private static Material GetMaterial(EScaleMethod scaleMethod)
        {
            if (s_xBRZMaterials == null)
            {
                Shader shader = Shader.Find(s_xBRZShaderName);
                // Init xBRZ Materials
                int materialNum = EScaleMethod_Num;
                s_xBRZMaterials = new Material[materialNum];
                for (int i = 0; i < materialNum; i++)
                {

                    Material mat = new Material(shader);
                    mat.name = scaleMethod.ToString();
                    mat.hideFlags = HideFlags.HideAndDontSave;


                    mat.EnableKeyword(s_xBRZMaterialKeywrods[i]);
                    s_xBRZMaterials[i] = mat;
                }
                
            }

            Material outMaterial = s_xBRZMaterials[scaleMethod.id];
            return outMaterial;

        }

        /// <summary>
        /// XBRZ，将 source 放大至 target
        /// </summary>
        /// <param name="source">原始像素画 Texture2D</param>
        /// <param name="target">目标 RenderTarget</param>
        /// <param name="scaleMethod">缩放方法</param>
        public static void DoScale(Texture2D source, RenderTexture target, EScaleMethod scaleMethod)
        {
            #if UNITY_EDITOR
            if (source.filterMode != FilterMode.Point)
                Debug.LogError($"source {source} filterMode is not {FilterMode.Point}");
            #endif
            Material material = GetMaterial(scaleMethod);
            
            Graphics.Blit(source, target, material);
        }
        
        /// <summary>
        /// XBRZ，将 source 放大至 target
        /// </summary>
        /// <param name="source">原始像素画 Texture2D</param>
        /// <param name="target">目标 RenderTarget</param>
        /// <param name="scale">缩放倍率 x2 x4 x6</param>
        public static void DoScale(Texture2D source, RenderTexture target, int scale)
        {
            if (s_scaleIntToEScaleMethod.TryGetValue(scale, out var scaleMethod))
            {
                DoScale(source, target, scaleMethod);
            }
            else
            {
                Debug.LogError($"Unsupport scale value \"{scale}\", the scale value must be one of 2, 4, 6");
            }
        }
    }
}
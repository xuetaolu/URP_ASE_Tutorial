// @author : xue
// @created : 2023,08,15,14:44
// @desc:

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.shaderDevelop_2023_08_10.动森天空HDR
{
    [ExecuteAlways]
    public class ACSkyBoxShaderParams : MonoBehaviour
    {
        #region Material Property

        public float _ScatterMie;
        public float _ScatterInstensity;

        public Vector2 UV1_T = new Vector2(0, -11.35793f);
        public float UV2_Rotate_Degree = 81.96083351016598f;
        public Vector2 UV2_T = new Vector2(-0.065f, 0.92515f);

        [ColorUsage(false, true)] 
        public Color SkyColorRGB = new Color(6.78572f, 3.07114f, 7.2585f, 1.0f);

        [Range(-1, 0)] 
        public float GalaxyAdditionA_Minus_0_to_n1 = -0.20113f;

        [Range(0, 100)] 
        public float GalaxyAdditionB_Scale = 26.03729f;
        
        [ColorUsage(false, true)] 
        public Color MieRedHdrColor = new Color(68.36447f, 6.83645f, 2.78375f, 1.00f);

        [Range(0, 1)] 
        public float GalaxyFactor01 = 0.53596f;

        [ColorUsage(false, true)] 
        public Color MainLightColorXYZ = new Color(0.62483f, 0.74478f, 0.67517f, 1.0f);

        [Range(0, 1)] 
        public float FinalMainLightAffectFactor = 0.06961f;

        #endregion

        public List<Material> m_materials = new List<Material>();
        
        
        
        #region SunDir

        public Transform m_sunTrans;
        public Vector3 SunLookAtDir = Vector3.down;
        public Vector3 SunDir = Vector3.up;
        
        private void UpdateSunDir()
        {
            if (m_sunTrans == null)
                return;
            var forward = m_sunTrans.forward;
            SunDir = -forward;
            SunLookAtDir = forward;

            Shader.SetGlobalVector("_SunLookAtDir", SunLookAtDir);
            Shader.SetGlobalVector("_SunDir", SunDir);
        }

        #endregion


        #region Sun Hdr RT

        public RenderTexture m_sunHdrRT;
        public Material m_material;

        public bool m_alwaysUpdateSunHdrRT;

        public void RenderRT()
        {
            if (m_sunHdrRT == null || m_material == null)
                return;
            Graphics.Blit(null, m_sunHdrRT, m_material);
        }

        #endregion
        
        
        private void Update()
        {
            UpdateSunDir();
            
            if(m_alwaysUpdateSunHdrRT)
                RenderRT();
            
            foreach (var mat in m_materials)
            {
                if (mat == null)
                    continue;

                UpdateMaterial(mat);
            }

            
        }
        
        private void UpdateMaterial(Material mat)
        {
            mat.SetFloat("_ScatterMie", _ScatterMie);
            mat.SetFloat("_ScatterInstensity", _ScatterInstensity);

            if (mat.HasProperty("_UV2_RS_Matrix"))
            {
                float cos_theta = Mathf.Cos(Mathf.Deg2Rad * -UV2_Rotate_Degree);
                float sin_theta = Mathf.Sin(Mathf.Deg2Rad * -UV2_Rotate_Degree);
                mat.SetVector("_UV2_RS_Matrix", new Vector4(cos_theta, sin_theta, -sin_theta, cos_theta));
            }

            if (mat.HasProperty("_UV1_T"))
                mat.SetVector("_UV1_T", UV1_T);

            if (mat.HasProperty("_UV2_T"))
                mat.SetVector("_UV2_T", UV2_T);
            
            if (mat.HasProperty("_SunHdrMap") && m_sunHdrRT != null)
                mat.SetTexture("_SunHdrMap", m_sunHdrRT);
            
            if (mat.HasProperty("_SkyColorRGB"))
                mat.SetColor("_SkyColorRGB", SkyColorRGB);
            if (mat.HasProperty("_GalaxyAdditionA_Minus_0_to_n1"))
                mat.SetFloat("_GalaxyAdditionA_Minus_0_to_n1", GalaxyAdditionA_Minus_0_to_n1);
            if (mat.HasProperty("_GalaxyAdditionB_Scale"))
                mat.SetFloat("_GalaxyAdditionB_Scale", GalaxyAdditionB_Scale);
            if (mat.HasProperty("_MieRedHdrColor"))
                mat.SetColor("_MieRedHdrColor", MieRedHdrColor);
            if (mat.HasProperty("_GalaxyFactor01"))
                mat.SetFloat("_GalaxyFactor01", GalaxyFactor01);
            if (mat.HasProperty("_MainLightColorXYZ"))
                mat.SetColor("_MainLightColorXYZ", MainLightColorXYZ);
            if (mat.HasProperty("_FinalMainLightAffectFactor"))
                mat.SetFloat("_FinalMainLightAffectFactor", FinalMainLightAffectFactor);
        }
    }
}
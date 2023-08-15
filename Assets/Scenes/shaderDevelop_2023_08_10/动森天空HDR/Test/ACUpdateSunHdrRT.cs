// @author : xue
// @created : 2023,08,15,16:12
// @desc:

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Scenes.shaderDevelop_2023_08_10.动森天空HDR
{
    [ExecuteAlways]
    public class ACUpdateSunHdrRT : MonoBehaviour
    {
        public RenderTexture m_sunHdrRT;
        public Material m_material;

        public bool m_alwaysUpdate;

        public void Update()
        {
            if(m_alwaysUpdate)
                RenderRT();
        }

        public void RenderRT()
        {
            if (m_sunHdrRT == null || m_material == null)
                return;
            Graphics.Blit(null, m_sunHdrRT, m_material);
        }
    }
}
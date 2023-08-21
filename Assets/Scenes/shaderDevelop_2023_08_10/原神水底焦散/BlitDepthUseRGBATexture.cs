// @author : xue
// @created : 2023,08,21,17:27
// @desc:

using System;
using Common;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散
{
    [ExecuteAlways]
    public class BlitDepthUseRGBATexture: MonoBehaviour
    {
        public Texture m_texture;
        public RenderTexture m_depthRT;
        public bool m_awalyUpdate;

        private Material _material;

        private Material material
        {
            get
            {
                if (_material == null)
                {
                    Shader shader = Shader.Find("Unlit/DrawDepthUseRGBATexture");
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }

                return _material;
            }
        }
        public void OnEnable()
        {
            BlitOnce();
        }

        private void Update()
        {
            if (!m_awalyUpdate)
                return;

            BlitOnce();
        }

        private void BlitOnce()
        {
            if (m_texture == null || m_depthRT == null || material == null)
                return;
            
            Graphics.Blit(m_texture, m_depthRT, material);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                objcleaner.Destroy(_material);
                _material = null;
            }
        }
    }
}
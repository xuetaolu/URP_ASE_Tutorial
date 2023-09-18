// @author : xue
// @created : 2023,08,21,17:27
// @desc:

using System;
using Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散
{
    [ExecuteAlways]
    public class BlitR32UseRGBATexture: MonoBehaviour
    {
        public Texture m_texture;
        public RenderTexture m_R32RT;
        public bool m_awalyUpdate;
        // public bool m_keeyRawDepth;

        private Material _material;

        private Material material
        {
            get
            {
                if (_material == null)
                {
                    Shader shader = Shader.Find("Hidden/DrawR32UseRGBATexture");
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
            if (m_texture == null || m_R32RT == null || material == null)
                return;

            Graphics.Blit(m_texture, m_R32RT, material);
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
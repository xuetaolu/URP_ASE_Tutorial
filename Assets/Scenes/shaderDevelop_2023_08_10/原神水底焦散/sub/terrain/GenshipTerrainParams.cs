// @author : xue
// @created : 2023,09,18,16:38
// @desc:

using System;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散.sub.terrain
{
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class GenshipTerrainParams : MonoBehaviour
    {
        private MaterialPropertyBlock _mpb;
        public Transform m_root;
        public float m_scale = 2;
        public MaterialPropertyBlock mpb {
            get
            {
                 if (_mpb == null)
                    _mpb = new MaterialPropertyBlock();
                 return _mpb;
            }
        }

        private void Update()
        {
            Renderer renderer1 = GetComponent<Renderer>();
            Transform trans = transform;
            MaterialPropertyBlock mpb1 = mpb;
            if (trans.hasChanged)
            {
                renderer1.GetPropertyBlock(mpb1);
                Vector3 localPosition = trans.position;
                if (m_root != null)
                    localPosition = trans.position - m_root.position;
                mpb1.SetVector("_LocalTerrainOffset", new Vector4(localPosition.x / m_scale, localPosition.z / m_scale, 1.0f, 0));
                renderer1.SetPropertyBlock(mpb1);
            }
        }
    }
}
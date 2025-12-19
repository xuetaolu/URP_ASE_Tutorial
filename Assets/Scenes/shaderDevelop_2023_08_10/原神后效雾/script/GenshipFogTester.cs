// @author : xue
// @created : 2023,12,25,18:25
// @desc:

using System;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神后效雾.script
{
    [ExecuteAlways]
    public class GenshipFogTester : MonoBehaviour
    {
        public Transform moon;

        private void Update()
        {
            Renderer _renderer = GetComponent<Renderer>();

            if (_renderer != null && moon != null)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                
                _renderer.GetPropertyBlock(mpb);
                
                mpb.SetVector("_MoonPos_maybe_Pos", moon.position);
                
                _renderer.SetPropertyBlock(mpb);
            }
        }
    }
}
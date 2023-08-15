// @author : xue
// @created : 2023,08,15,10:29
// @desc:

using System;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.动森天空HDR
{
    [ExecuteAlways]
    public class SunDirBehaviour : MonoBehaviour
    {
        public Vector3 SunLookAtDir = Vector3.down;
        public Vector3 SunDir = Vector3.up;
        private void Update()
        {
            var forward = transform.forward;
            SunDir = -forward;
            SunLookAtDir = forward;
            
            Shader.SetGlobalVector("_SunLookAtDir", SunLookAtDir);
            Shader.SetGlobalVector("_SunDir", SunDir);
        }
    }
}
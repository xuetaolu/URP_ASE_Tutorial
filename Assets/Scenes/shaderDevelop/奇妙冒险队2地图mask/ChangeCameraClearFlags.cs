// @author : xue
// @created : 2023,02,14,17:12
// @desc:

using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Scenes.shaderDevelop.奇妙冒险队2地图mask
{
    [ExecuteAlways]
    public class ChangeCameraClearFlags : MonoBehaviour
    {
        public CameraClearFlags cameraClearFlags = CameraClearFlags.Color;

        private void Start()
        {
            var camera = GetComponent<Camera>();
            if (camera != null)
                cameraClearFlags = camera.clearFlags;
        }

        private void Update()
        {
            var camera = GetComponent<Camera>();
            if (camera != null && camera.clearFlags != cameraClearFlags)
                camera.clearFlags = cameraClearFlags;
        }
    }
}
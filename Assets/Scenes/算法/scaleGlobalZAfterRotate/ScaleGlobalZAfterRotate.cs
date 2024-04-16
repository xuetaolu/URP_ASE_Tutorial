// @author : xue
// @created : 2024,02,22,16:38
// @desc:

using System;
using UnityEngine;

namespace Scenes.算法.scaleGlobalZAfterRotate
{
    [ExecuteAlways]
    public class ScaleGlobalZAfterRotate : MonoBehaviour
    {
        public Transform templateTransform;

        public Transform previewTransform;

        public float scaleZ;

        private void Update()
        {
            if (templateTransform == null || previewTransform == null)
                return;

            // previewTransform.localScale = templateTransform.localScale;
            previewTransform.localRotation = templateTransform.localRotation;


            Vector3 scale = templateTransform.localRotation * new Vector3(1, 1, scaleZ);

            previewTransform.localScale = Vector3.Scale(templateTransform.localScale, scale);
        }
    }
}
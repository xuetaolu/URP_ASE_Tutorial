// @author : xue
// @created : 2023,08,15,14:31
// @desc:

using System;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.动森天空HDR
{
    [ExecuteAlways]
    public class RotateToLookAtDir : MonoBehaviour
    {
        public bool m_reverseX;

        public Vector3 m_lookAtDir = Vector3.forward;

        private void Update()
        {
            Vector3 targetLookAt = m_lookAtDir;
            if (m_reverseX)
                targetLookAt.x = -targetLookAt.x;
            Quaternion q = Quaternion.LookRotation(targetLookAt, Vector3.up);

            transform.rotation = q;
        }
    }
}
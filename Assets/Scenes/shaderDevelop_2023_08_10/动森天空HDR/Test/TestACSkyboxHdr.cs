// @author : xue
// @created : 2023,08,10,14:55
// @desc:

using System;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.动森天空HDR
{
    [ExecuteAlways]
    public class TestACSkyboxHdr : MonoBehaviour
    {
        public Renderer _renderer;

        public Transform target;
        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (target == null)
                return;
            
            Material material = null;
            if (_renderer != null)
                material = _renderer.sharedMaterial;

            if (material == null)
                return;
            // 0.55817, 0.03907, 0.8288
            Vector3 targetDirection = target.position - transform.position;
            targetDirection = targetDirection.normalized;
            
            material.SetVector("_SunDirection", targetDirection);
        }
    }
}
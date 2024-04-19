// @author : xue
// @created : 2024,04,17,16:49
// @desc:

using System;
using System.Collections.Generic;
using UnityEngine;

namespace flower_effect.script
{
    /// <summary>
    /// 粒子系统贴花补充额外信息组件
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystemRenderer))]
    public class ParticleSystemDecal : MonoBehaviour
    {
        // public static readonly string s_TransformScale = "_TransformScale";
        // public static readonly int s_TransformScaleId = Shader.PropertyToID(s_TransformScale);
        
        public static readonly string s_WorldToLocalMatrix = "_WorldToLocalMatrix";
        public static readonly int s_WorldToLocalMatrixId = Shader.PropertyToID(s_WorldToLocalMatrix);
        
        private MaterialPropertyBlock _materialPropertyBlock;
        public MaterialPropertyBlock materialPropertyBlock
        {
            get
            {
                if (_materialPropertyBlock == null)
                {
                    _materialPropertyBlock = new MaterialPropertyBlock();
                }

                return _materialPropertyBlock;
            }
        }
        
        private bool _initedFirstTime = false;

        private void Update()
        {
            // 因为 Particle System 的 unity_WorldToObject 不准确，故需要手动传入一个准确的 _WorldToLocalMatrix
            // 用于补充修复因 transform Rotation Scale 导致的 _worldToMesh_matrix 旋转缩放不准确问题
            
	        // 粒子生成时，预先依据 transform 的 Rotation Scale 预先把 Mesh 进行缩放旋转，再按 rotation3D sizeXYZ centerPos 布局。
	        // 将此预先的 Rotation Scale 定义为 _meshPreRS_matrix, _meshPreRS_matrix 就是 (float3x3)_LocalToWorldMatrix
	        // 他的逆 _meshPreRS_matrix_inv 就是 (float3x3)_WorldToLocalMatrix
            // if (transform.hasChanged || !_initedFirstTime)
            {
                ParticleSystemRenderer psr = GetComponent<ParticleSystemRenderer>();

                MaterialPropertyBlock mpb = materialPropertyBlock;

                mpb.SetMatrix(s_WorldToLocalMatrixId, transform.worldToLocalMatrix);
                
                psr.SetPropertyBlock(mpb);
                
                _initedFirstTime = true;
            }
        }
    }
}
// @author : xue
// @created : 2022,09,15,13:45
// @desc:

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPExtend.Scripts.Runtime.ClipPostProcessOutline.pass;
using URPExtend.Scripts.Runtime.RendererFeature;

namespace URPExtend.Scripts.Runtime.ClipPostProcessOutline
{
    [ExecuteAlways]
    public class ClipPostProcessOutline : MonoBehaviour
    {
        public bool m_awalyRefreshMaterial;
        public bool m_autoUpdateViewportRect = true;

        [Tooltip("x, y 是相对左下角的偏移值，默认为最小值0，最大为1； z,w 是长方形向右向上的宽高，默认为最大值1。")]
        public Vector4 m_ViewportRect = new Vector4(0, 0, 1, 1);

        public Vector2Int m_fillColorTextureSize = new Vector2Int(512, 512);

        public Material m_fillColorMaterial;
        public Material m_blurColorMaterial;
        public Material m_drawOutlineMaterial;

        private Material _fillColorMaterial;
        private Material _blurColorMaterial;
        private Material _blurColorMaterial2;
        private Material _drawOutlineMaterial;

        // public Renderer m_renderer;
        public List<Renderer> m_rendereres = new List<Renderer>();

        [Range(0.1f, 10)] public float m_blurPixelSize = 1.0f;

        private RenderTargetHandle m_fillColorRenderTargetHandle;

        private FillColorPass m_fillColorPass;

        private BlurColorPass m_blurColorPass;

        private DrawOutlinePass m_drawOutlinePass;

        private Bounds m_cacheBounds = new Bounds();

        private Vector3[] m_cache8Corner = new Vector3[8];

        private void Start()
        {
        }

        private Material CopyRuntimeMaterial(Material mat)
        {
            var result = new Material(mat);
            result.hideFlags = HideFlags.DontSave;
            return result;
        }

        public void Init()
        {
            _initMaterial();


            m_fillColorRenderTargetHandle.Init("_OutlineFillColorTexture");

            m_fillColorPass = new FillColorPass(m_fillColorRenderTargetHandle);

            m_blurColorPass = new BlurColorPass(m_fillColorRenderTargetHandle);

            m_drawOutlinePass = new DrawOutlinePass(m_fillColorRenderTargetHandle);
        }

        private void _initMaterial()
        {
            _fillColorMaterial = CopyRuntimeMaterial(m_fillColorMaterial);
            _blurColorMaterial = CopyRuntimeMaterial(m_blurColorMaterial);
            _blurColorMaterial2 = CopyRuntimeMaterial(m_blurColorMaterial);
            _drawOutlineMaterial = CopyRuntimeMaterial(m_drawOutlineMaterial);
        }

        public void Clear()
        {
            _clearMaterial();
        }

        private void _clearMaterial()
        {
            CoreUtils.Destroy(_fillColorMaterial);
            CoreUtils.Destroy(_blurColorMaterial);
            CoreUtils.Destroy(_blurColorMaterial2);
            CoreUtils.Destroy(_drawOutlineMaterial);
        }

        public void OnEnable()
        {
            CallbackRendererFeature.s_addRendererPassesEvent += AddRenderPasses;
            Init();
        }

        public void OnDisable()
        {
            CallbackRendererFeature.s_addRendererPassesEvent -= AddRenderPasses;
            Clear();
        }

        private bool _isExistRenderer()
        {
            for (int i = 0; i < m_rendereres.Count; i++)
            {
                var item = m_rendereres[i];
                if (item != null)
                    return true;
            }

            return false;
        }

        public void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;

            if (!_isExistRenderer())
            {
                return;
            }

            UpdateViewportRect(renderer, ref renderingData);

            // 太小了，不渲染
            if (m_ViewportRect.Equals(Vector4.zero))
            {
                // Debug.Log($"太小了，不渲染 {m_ViewportRect}");
                return;
            }

#if UNITY_EDITOR
            if (m_awalyRefreshMaterial)
            {
                _clearMaterial();
                _initMaterial();
            }
#endif

            EnqueuePass(renderer);
        }

        /// <summary>
        /// 全展开不要触发 GC ?
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private bool GetBestViewportRect(Camera camera, Bounds bounds, ref Rect rect)
        {
            Vector3 cen = bounds.center;
            Vector3 ext = bounds.extents;
            Camera cam = camera;

            var cameraTrans = cam.transform;
            var cameraFroward = cameraTrans.forward;
            var cameraPosition = cameraTrans.position;

            m_cache8Corner[0] = new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z);
            m_cache8Corner[1] = new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z);
            m_cache8Corner[2] = new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z);
            m_cache8Corner[3] = new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z);
            m_cache8Corner[4] = new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z);
            m_cache8Corner[5] = new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z);
            m_cache8Corner[6] = new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z);
            m_cache8Corner[7] = new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z);

            bool initedFirstPoint = false;
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;

            for (int i = 0; i < m_cache8Corner.Length; i++)
            {
                ref var corner = ref m_cache8Corner[i];

                // 在相机后面不要
                if (Vector3.Dot(cameraFroward, corner - cameraPosition) < 0.1)
                {
                    continue;
                }

                Vector2 point = cam.WorldToViewportPoint(corner);

                if (!initedFirstPoint)
                {
                    min = max = point;
                    initedFirstPoint = true;
                }
                else
                {
                    min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
                    max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
                }
            }

            if (initedFirstPoint)
            {
                rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateViewportRect(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_autoUpdateViewportRect)
                return;

            if (renderingData.cameraData.isPreviewCamera)
                return;

            // if (renderingData.cameraData.isSceneViewCamera)
            // return;

            Rect rect = Rect.zero;
            int encapsulateCount = 0;
            for (int i = 0; i < m_rendereres.Count; i++)
            {
                var item = m_rendereres[i];
                if (item == null)
                    continue;

                var bounds = item.bounds;
                var camera = renderingData.cameraData.camera;

                var existBestRect = GetBestViewportRect(camera, bounds, ref rect);

                if (!existBestRect)
                    continue;

                encapsulateCount++;

                if (encapsulateCount <= 1)
                {
                    m_cacheBounds.SetMinMax(new Vector3(rect.x, rect.y, 0),
                        new Vector3(rect.x + rect.width, rect.y + rect.height, 0));
                }
                else
                {
                    m_cacheBounds.Encapsulate(new Vector3(rect.x, rect.y, 0));
                    m_cacheBounds.Encapsulate(new Vector3(rect.x + rect.width, rect.y + rect.height, 0));
                }
            }

            if (encapsulateCount > 0)
            {
                Vector2 min = m_cacheBounds.min;
                Vector2 max = m_cacheBounds.max;

                var expandXFactor = m_blurPixelSize / m_fillColorTextureSize.x;
                var expandYFactor = m_blurPixelSize / m_fillColorTextureSize.y;
                var width = max.x - min.x;
                var height = max.y - min.y;
                var expandX = width * expandXFactor;
                var expandY = height * expandYFactor;

                min = new Vector2(Mathf.Max(0, min.x - expandX), Mathf.Max(0, min.y - expandY));
                max = new Vector2(Mathf.Min(1, max.x + expandX), Mathf.Min(1, max.y + expandY));

                m_ViewportRect = new Vector4(min.x, min.y, max.x - min.x, max.y - min.y);
            }
            else
            {
                m_ViewportRect = Vector4.zero;
            }
        }

        private void EnqueuePass(ScriptableRenderer renderer)
        {
            m_fillColorPass.fillColorMaterial = _fillColorMaterial;
            m_fillColorPass.renderers = m_rendereres;
            m_fillColorPass.textureSize = m_fillColorTextureSize;
            m_fillColorPass.viewportRect = m_ViewportRect;
            renderer.EnqueuePass(m_fillColorPass);


            m_blurColorPass.blurColorMaterial = _blurColorMaterial;
            m_blurColorPass.blurColorMaterial2 = _blurColorMaterial2;
            m_blurColorPass.textureSize = m_fillColorTextureSize;
            m_blurColorPass.blurPixelSize = m_blurPixelSize;
            renderer.EnqueuePass(m_blurColorPass);


            m_drawOutlinePass.drawOutlineMaterial = _drawOutlineMaterial;
            m_drawOutlinePass.viewportRect = m_ViewportRect;
            m_drawOutlinePass.renderer = renderer;
            renderer.EnqueuePass(m_drawOutlinePass);
        }

        public void OnDestroy()
        {
        }
    }
}
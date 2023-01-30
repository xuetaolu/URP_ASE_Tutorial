// @author : xue
// @created : 2023,01,30,15:23
// @desc:

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scenes.渲染特性支持
{
    // [ExecuteAlways]
    public class MsaaCountSupportDetector : MonoBehaviour
    {
        // 最大 msaa 8， 2^3
        public static readonly int s_maxMsaaLog2 = 3;

        private Material m_defaultMaterial;

        [SerializeField]
        private List<float> m_resultList = new List<float>();
        [SerializeField]
        private List<Texture2D> m_texture2DList = new List<Texture2D>();
        [NonSerialized]
        private bool m_inited;
        
        [NonSerialized]
        private Dictionary<int, RenderTextureHandle> m_rtHanles;

        public Material defaultMaterial
        {
            get
            {
                if (m_defaultMaterial == null)
                {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");
                    m_defaultMaterial = new Material(shader);
                    m_defaultMaterial.hideFlags = HideFlags.HideAndDontSave;
                    m_defaultMaterial.SetColor("_Color", Color.white);
                    m_defaultMaterial.SetFloat("_SrcBlend", (int)BlendMode.One);
                    m_defaultMaterial.SetFloat("_DstBlend", (int)BlendMode.Zero);
                    m_defaultMaterial.SetFloat("_ZWrite", 0);
                    m_defaultMaterial.SetFloat("_ZTest", (int)CompareFunction.Always);
                    m_defaultMaterial.SetFloat("_Cull", (int)CullMode.Off);
                    m_defaultMaterial.SetFloat("_ZBias", 0);
                }

                return m_defaultMaterial;
            }
        }
        
        private void Awake()
        {
            m_rtHanles = new Dictionary<int, RenderTextureHandle>();
            for (int i = 0; i <= s_maxMsaaLog2; i++)
            {
                int msaa = 1 << i;
                m_rtHanles.Add(msaa, new RenderTextureHandle() {m_msaa = msaa});
            }
        }


        public bool IsSupportMsaaCount(int msaa)
        {
            if (m_rtHanles.TryGetValue(msaa, out var rtHandle))
            {
                return rtHandle.isResultMatch();
            }

            return false;
        }

        public float GetResult(int msaa)
        {
            if (m_rtHanles.TryGetValue(msaa, out var rtHandle))
            {
                return rtHandle.result;
            }
            return 0;
        }

        public class RenderTextureHandle
        {
            private RenderTexture m_renderTexture;
            public float result;
            public int m_msaa;
            


            public static float s_colorBias = 2.0f/255;

            public RenderTexture renderTexture
            {
                get
                {
                    CheckInit(ref m_renderTexture);
                    return m_renderTexture;
                }
            }

            private void CheckInit(ref RenderTexture renderTexture)
            {
                if (renderTexture == null)
                    renderTexture = RenderTexture.GetTemporary(1, 256, 0, RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear, m_msaa);
            }

            public void DestoryRenderTexture()
            {
                checkDestory(ref m_renderTexture);
            }

            private void checkDestory(ref RenderTexture renderTexture)
            {
                if (renderTexture != null)
                    RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = null;
            }

            public bool isResultMatch()
            {
                return Mathf.Abs(result - 1.0f / m_msaa) <= s_colorBias;
            }
        }
        
        public void Update()
        {
            // m_inited = false;
            Init();
        }

        public void Init()
        {
            if (m_inited) return;
            
            DrawRenderTextures();
            
            clearResult();
            foreach (var rtHandle in m_rtHanles.Values)
            {
                var texture2D = readRenderTexture(rtHandle.renderTexture);
                float result = -1;
                for (int h = 0; h < texture2D.height; h++) {
                    var color = texture2D.GetPixel(0, h);
                    float red = color.r;
                    if (result < 0 || (red > 0 && red < result))
                    {
                        result = red;
                    }
                }

                rtHandle.result = result;
                m_resultList.Add(result);
                m_texture2DList.Add(texture2D);
            }

            releaseRenderTextures();
            m_inited = true;

        }

        private void clearResult()
        {
            m_resultList.Clear();
            for (int i = 0; i < m_texture2DList.Count; i++)
            {
                DestroyImmediate(m_texture2DList[i]);
            }
            m_texture2DList.Clear();
        }

        private Texture2D readRenderTexture(RenderTexture renderTexture)
        {
            int width, height;
            (width, height) = (renderTexture.width, renderTexture.height);
            
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);;
            var backup = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            
            RenderTexture.active = backup;
            return texture2D;
        }


        private void releaseRenderTextures()
        {
            foreach (var rtHandle in m_rtHanles.Values)
            {
                rtHandle.DestoryRenderTexture();
            }
        }


        public void DrawRenderTextures()
        {
            foreach (var rtHandle in m_rtHanles.Values)
            {
                var rt = rtHandle.renderTexture;
                drawTriangle(rt, defaultMaterial );
            }
        }

        private void drawTriangle(RenderTexture rt, Material mat)
        {
            
            if (mat == null) return;
            if (rt == null) return;
            var backup = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);

            GL.PushMatrix();
            GL.LoadOrtho();

            mat.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            // GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
            RenderTexture.active = backup;
        }

        private void OnDestroy()
        {
            if (m_defaultMaterial != null)
                DestroyImmediate(m_defaultMaterial);
            clearResult();
        }
    }
}
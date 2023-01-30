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
    [ExecuteAlways]
    public class MsaaCountSupportDetector : MonoBehaviour
    {

        private Material m_defaultMaterial;

        [SerializeField]
        private List<float> m_resultList = new List<float>();
        [SerializeField]
        private List<Texture2D> m_texture2DList = new List<Texture2D>();
        [NonSerialized]
        private bool m_inited;

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
        
        public Dictionary<int, RenderTextureHandle> m_rtHanles = new Dictionary<int, RenderTextureHandle>()
        {
            {1 , new RenderTextureHandle() {m_msaa = 1}},
            {2 , new RenderTextureHandle() {m_msaa = 2}},
            {4,  new RenderTextureHandle() {m_msaa = 4}},
            {8 , new RenderTextureHandle() {m_msaa = 8}},
        };

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
            // private RenderTexture m_renderTexture2;
            // private RenderTexture m_renderTexture3;
            public float result;
            // public float result2;
            // public float result3;
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
                    renderTexture = RenderTexture.GetTemporary(1, 4, 0, RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear, m_msaa);
            }

            public void DestoryRenderTexture()
            {
                checkDestory(ref m_renderTexture);
                // checkDestory(ref m_renderTexture2);
                // checkDestory(ref m_renderTexture3);
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
            m_inited = false;
            Init();
        }

        public void Init()
        {
            if (m_inited) return;
            
            
            DrawRenderTextures();
            
            m_resultList.Clear();
            m_texture2DList.Clear();
            foreach (var rtHandle in m_rtHanles.Values)
            {
                var texture2D = readRenderTexture(rtHandle.renderTexture);
                var color = texture2D.GetPixel(0, 0);
                rtHandle.result = color.r;
                m_resultList.Add(color.r);
                m_texture2DList.Add(texture2D);
            }

            releaseRenderTextures();
            m_inited = true;

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
                drawHalfScreen(rt, defaultMaterial, 1.0f / rtHandle.m_msaa );
            }
        }

        private void drawHalfScreen(RenderTexture rt, Material mat, float present = 0.5f)
        {
            
            if (mat == null) return;
            if (rt == null) return;
            var backup = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);

            GL.PushMatrix();
            GL.LoadOrtho();

            mat.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(present, 0, 0);
            GL.Vertex3(present, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
            RenderTexture.active = backup;
        }

        private void OnDestroy()
        {
            if (m_defaultMaterial != null)
                DestroyImmediate(m_defaultMaterial);
        }
    }
}
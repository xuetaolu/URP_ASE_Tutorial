using System;
using System.Collections;
using System.Collections.Generic;
using Scenes.渲染特性支持;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// [ExecuteAlways]
public class CheckRenderFeatureSupport : MonoBehaviour
{
    public MsaaCountSupportDetector m_msaaCountSupportDetector;
    
    // Start is called before the first frame update
    private void Awake()
    {
        m_msaaCountSupportDetector = GetComponent<MsaaCountSupportDetector>();
        if (m_msaaCountSupportDetector == null)
            m_msaaCountSupportDetector = transform.AddComponent<MsaaCountSupportDetector>();
    }

    void Start()
    {
        m_msaaCountSupportDetector.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void OnGUI()
    {
        int maxMsaaLog = MsaaCountSupportDetector.s_maxMsaaLog2;
        using (new GUILayout.VerticalScope())
        {
            for (int i = 0; i <= maxMsaaLog; i++)
            {
                GUILayout.Label(GetMSAASupportInfo(1 << i, false));
                GUILayout.Label(GetMSAASupportInfo(1 << i, true));
            }
        }

        GUILayout.Label("bydetector: ");
        using (new GUILayout.VerticalScope())
        {
            for (int i = 0; i <= maxMsaaLog; i++)
            {
                GUILayout.Label(GetMSAASupportInfoByDetector(1 << i));
            }
        }
    }

    private int checkMSAASupportCount(int msaa, bool isHdr)
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.colorFormat = isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        desc.msaaSamples = msaa;
        desc.height = 4;
        desc.width = 4;
        desc.dimension = TextureDimension.Tex2D;
        desc.mipCount = 0;
        // desc.depthBufferBits = 24;
        // desc.depthStencilFormat = 
        int result = SystemInfo.GetRenderTextureSupportedMSAASampleCount(desc);
        return result;
    }
    
    private int checkMSAASupportCountByDetector(int msaa)
    {
        while (!m_msaaCountSupportDetector.IsSupportMsaaCount(msaa) && msaa > 0)
        {
            msaa >>= 1;
        }

        return msaa;
    }
    
    private bool checkMSAASupport(int msaa, bool isHdr)
    {
        return checkMSAASupportCount(msaa, isHdr) == msaa;
    }

    public String GetMSAASupportInfo(int msaa, bool isHdr)
    {
        int supportCount = checkMSAASupportCount(msaa, isHdr);
        return $"msaa: {msaa,4}, isHdr: {isHdr,5}, support: {supportCount == msaa,5}, count: {supportCount,4}";
    }    
    
    public String GetMSAASupportInfoByDetector(int msaa)
    {
        int supportCount = checkMSAASupportCountByDetector(msaa);
        float result = m_msaaCountSupportDetector.GetResult(msaa);
        return $"msaa: {msaa,4}, support: {supportCount == msaa,5}, count: {supportCount,4}, result: {result}";
    }
}

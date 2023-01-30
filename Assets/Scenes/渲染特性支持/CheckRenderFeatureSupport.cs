using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class CheckRenderFeatureSupport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void OnGUI()
    {
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Label(GetMSAASupportInfo(1 << 0, false));
            GUILayout.Label(GetMSAASupportInfo(1 << 1, false));
            GUILayout.Label(GetMSAASupportInfo(1 << 2, false));
            GUILayout.Label(GetMSAASupportInfo(1 << 3, false));
            GUILayout.Label(GetMSAASupportInfo(1 << 0, true));
            GUILayout.Label(GetMSAASupportInfo(1 << 1, true));
            GUILayout.Label(GetMSAASupportInfo(1 << 2, true));
            GUILayout.Label(GetMSAASupportInfo(1 << 3, true));
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

    private bool checkMSAASupport(int msaa, bool isHdr)
    {
        return checkMSAASupportCount(msaa, isHdr) == msaa;
    }

    public String GetMSAASupportInfo(int msaa, bool isHdr)
    {
        int supportCount = checkMSAASupportCount(msaa, isHdr);
        return $"msaa: {msaa,4}, isHdr: {isHdr,5}, support: {supportCount == msaa,5}, count: {supportCount,4}";
    }
}

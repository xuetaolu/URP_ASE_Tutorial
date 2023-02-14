using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ClearColorOnly : MonoBehaviour
{
    public Color color;
    private Camera camera
    {
        get => GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += beginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= beginCameraRendering;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void beginCameraRendering(ScriptableRenderContext scriptableRenderContext, Camera camera1)
    {
        var local_camera = camera;
        if (local_camera != null && local_camera.Equals(camera1))
        {
            if (local_camera.targetTexture != null)
            {
                var cb = CommandBufferPool.Get();
                cb.Clear();
                cb.SetRenderTarget(local_camera.targetTexture);
                cb.ClearRenderTarget(false, true, color);
                scriptableRenderContext.ExecuteCommandBuffer(cb);
                CommandBufferPool.Release(cb);
            }
        }
    }
}

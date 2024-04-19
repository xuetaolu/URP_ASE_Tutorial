using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class EnableDepth : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnDisable()
    {
        GetComponent<Camera>().depthTextureMode &= ~DepthTextureMode.Depth;
    }
}

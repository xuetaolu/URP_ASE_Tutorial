using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
[ExecuteAlways]
public class ZfightingTest : MonoBehaviour
{
    public float m_nearMulti = 10f;

    public float m_maxNearMulti = 30f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var camera = GetComponent<Camera>();

        if (camera != null)
        {
            transform.localPosition = new Vector3(0, 0, -m_nearMulti * camera.nearClipPlane);
        }
    }

    private void OnGUI()
    {
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Label("maxNearMulti:");
            m_maxNearMulti = float.Parse(GUILayout.TextField(m_maxNearMulti.ToString(CultureInfo.InvariantCulture)));
            GUILayout.Label("nearMulti:");
            m_nearMulti = float.Parse(GUILayout.TextField(m_nearMulti.ToString(CultureInfo.InvariantCulture)));
            // m_nearMulti = GUILayout.HorizontalSlider(m_nearMulti, 1, m_maxNearMulti);
            
            var camera = GetComponent<Camera>();
            if (camera != null)
            {
                GUILayout.Label("nearplane:");
                camera.nearClipPlane = float.Parse(GUILayout.TextField(camera.nearClipPlane.ToString(CultureInfo.InvariantCulture)));
                GUILayout.Label("farplane:");
                camera.farClipPlane = float.Parse(GUILayout.TextField(camera.farClipPlane.ToString(CultureInfo.InvariantCulture)));

            }
        }
        
    }
}

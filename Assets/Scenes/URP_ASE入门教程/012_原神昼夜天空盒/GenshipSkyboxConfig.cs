using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class GenshipSkyboxConfig : MonoBehaviour
{
    public Transform m_sun;

    public Transform m_moon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Material GetMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            return renderer.sharedMaterial;
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        Material material = GetMaterial();
        if (material != null)
        {
            Vector3 pos = transform.position;
            if (m_sun != null)
            {
                Vector3 sun_dir = (m_sun.position - pos).normalized;
                material.SetVector("_sun_dir", sun_dir);
            }

            if (m_moon != null)
            {
                Vector3 moon_dir = (m_moon.position - pos).normalized;
                material.SetVector("_moon_dir", moon_dir);
            }
        }

        
    }
}

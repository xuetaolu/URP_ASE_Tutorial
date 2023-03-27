using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class DepthPlaneAlign : MonoBehaviour
{
    [SerializeField] public Camera m_camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_camera == null)
        {
            m_camera = transform.parent.GetComponent<Camera>();
        }

        if (m_camera != null)
        {
            var near = m_camera.nearClipPlane;
            var far = m_camera.farClipPlane;
            var fov = m_camera.fieldOfView;
            var aspect = m_camera.aspect;
            float width = Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f) * near * aspect;
            float height = width / aspect*2;
            
            float width_far = Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f) * far * aspect;
            float height_far = width_far / aspect * 2;

            var c_tran = m_camera.transform;
            var c_pos = c_tran.position;
            var alignWorldPos = c_pos + c_tran.forward * near + c_tran.right * width;
            var targetWorldPos = c_pos + c_tran.forward * far - c_tran.right * width_far;
            
            transform.position = alignWorldPos;
            transform.localScale = new Vector3(20000, height*0.8f,1);
            transform.LookAt(targetWorldPos, c_tran.up);
            transform.Rotate(c_tran.up, 90);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Scenes.Closet3D;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Closet3D : MonoBehaviour
{
    public Transform m_closetPointTrans;

    public Transform m_boxTrans;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_closetPointTrans == null || m_boxTrans == null)
            return;

        m_closetPointTrans.position = GetClosetPointToBox(transform.position, m_boxTrans, Vector3.one*0.5f);
    }

    Vector3 GetClosetPointToBox(Vector3 worldTargetPos, Transform boxTrans, Vector3 boxSize)
    {
        Vector3 objectTargetPos = boxTrans.InverseTransformPoint(worldTargetPos);
        Vector3 closetObjectPos = IQCloset3D.closestPointToBox(objectTargetPos, boxSize, true);
        Vector3 closetWorldPos = boxTrans.TransformPoint(closetObjectPos);
        return closetWorldPos;
    }

    private void OnDrawGizmos()
    {
        if (m_closetPointTrans == null)
            return;
        Gizmos.DrawLine(transform.position, m_closetPointTrans.position);
    }
}

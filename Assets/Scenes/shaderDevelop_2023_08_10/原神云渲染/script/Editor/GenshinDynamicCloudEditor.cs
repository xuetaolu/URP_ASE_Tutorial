// @author : xue
// @created : 2024,04,12,15:15
// @desc:

using UnityEditor;
using UnityEngine;

namespace flower_scene.dynamic_cloud
{
    [CustomEditor(typeof(GenshinDynamicCloud))]
    public class GenshinDynamicCloudEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GenshinDynamicCloud dynamicCloud = target as GenshinDynamicCloud;

            // 相当于 OnValidate 功能
            if (this.DrawDefaultInspector() && dynamicCloud.alwayRebuild)
            {
                dynamicCloud.InitClouds();
            }

            GUI.enabled = !dynamicCloud.alwayRebuild;
            if (GUILayout.Button("Rebuild"))
            {
                dynamicCloud.InitClouds();
                dynamicCloud.Update();
            }
            GUI.enabled = true;



        }
    }
}
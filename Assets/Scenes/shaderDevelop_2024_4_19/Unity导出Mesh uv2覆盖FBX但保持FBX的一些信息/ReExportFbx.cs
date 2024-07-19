// @author : xue
// @created : 2024,07,16,15:07
// @desc:
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Common;
using UnityEditor;
using Object = UnityEngine.Object;

namespace flower_scene.fbx
{
    /// <summary>
    /// 将选中的 fbx 导入unity 后的顶点数据重新按 fbx 中的结构导出
    /// </summary>
    public class ReExportFbx
    {
        [MenuItem("Assets/将 Unity 数据覆盖到 fbx(实验性功能)")]
        public static void ReExportFbxMethod()
        {
            bool needRefresh = false;
            try
            {
                // 获取选择的 fbx 所有文件
                List<string> fbxPaths = new List<string>();
                var fileSet = SelectUtils.GetSelectedFiles();
                foreach (var file in fileSet)
                {
                    if (!file.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                         continue;
                    fbxPaths.Add(file);
                }
                
                fbxPaths.Sort();
                int length = fbxPaths.Count;
                
                for (int i = 0; i < fbxPaths.Count; i++)
                {
                    var fbxPath = fbxPaths[i];
                    if (EditorUtility.DisplayCancelableProgressBar("批处理 fbx", fbxPath, (float)i / length))
                        break;
                    needRefresh |= (new ReExportFbxProcess(fbxPath)).Process();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (needRefresh)
                {
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}

#endif
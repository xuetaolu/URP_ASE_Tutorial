// @author : xue
// @created : 2025,07,16,11:07
// @desc:
#if UNITY_EDITOR
using System.IO;
using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;
namespace xue
{
    public class HEUtils
    {
        public static void InitSession(ref HEU_SessionBase session, bool reset = false)
        {
            if (reset)
                ResetSession(ref session);
            session = HEU_SessionManager.GetOrCreateDefaultSession();
        }
        private static void ResetSession(ref HEU_SessionBase session)
        {
            session = HEU_SessionManager.GetOrCreateDefaultSession();
            if (session == null || !session.IsSessionValid())
            {
                Debug.LogError("No session available for Houdini Engine!");
                return;
            }
            // 清除掉 session 全部东西
            if (!session.RestartSession())
            {
                Debug.LogError("Failed to restart session!");
                return;
            }
        }
        public static bool GetFloatProperty(Material mat, string propertyName, out float value)
        {
            value = 0;
            var shader = mat != null ? mat.shader : null;
            if (mat != null && mat.HasProperty(propertyName))
            {
                value = mat.GetFloat(propertyName);
                return true;
            }
            if (shader != null){
                var index = shader.FindPropertyIndex(propertyName);
                if (index >= 0)
                {
                    value = shader.GetPropertyDefaultFloatValue(index);
                    return true;
                }
            }
            return false;
        }

        public static void SaveHipFile(HEU_SessionBase session, string hipName)
        {
            if (string.IsNullOrWhiteSpace(hipName))
                hipName = "debug";
            string projectPath = Application.dataPath;
            var hipPath = Path.GetFullPath($"{projectPath}/../hip/{hipName}.hip");
            // 确保文件夹存在
            Directory.CreateDirectory(Path.GetDirectoryName(hipPath));
            if (!session.SaveHIPFile(hipPath, false))
            {
                Debug.LogError("Failed to save HIP file!");
            }
            else
            {
                Debug.Log($"Save Hip file to {hipPath} success!");
            }
        }
    }
}
#endif
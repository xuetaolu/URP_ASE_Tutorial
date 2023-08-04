// @author : xue
// @created : 2023,08,04,9:47
// @desc:

using UnityEditor;

namespace Scenes.逆向变换矩阵调研
{
    [CustomEditor(typeof(ReverseProjection))]
    public class ReverseProjectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
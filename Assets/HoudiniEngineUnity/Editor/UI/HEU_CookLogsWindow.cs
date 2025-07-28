using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Text;

namespace HoudiniEngineUnity
{
    public class HEU_CookLogsWindow : EditorWindow
    {
        private HEU_OutputLogUIComponent _outputLogUIComponent = null;

        private GUIContent _titleContent = new GUIContent("Cook Log", "Cook logs displayed here");

        private const float _bottomPadding = 75;

        [MenuItem("HoudiniEngine/Cook Progress Logs")]
        private static void Init()
        {
            bool bUtility = false;
            bool bFocus = false;
            string title = "Houdini Cook Logs";

            HEU_CookLogsWindow window = EditorWindow.GetWindow<HEU_CookLogsWindow>(bUtility, title, bFocus);
            InitSize(window);
        }

        public static void InitSize(HEU_CookLogsWindow window)
        {
            window.minSize = new Vector2(300, 150);
        }

        private void SetupUI()
        {
            if (_outputLogUIComponent == null)
            {
                _outputLogUIComponent = new HEU_OutputLogUIComponent(_titleContent, OnClearLog);
            }

            _outputLogUIComponent.SetupUI();
        }

        private void OnGUI()
        {
            HEU_SessionBase sessionBase = HEU_SessionManager.GetDefaultSession();

            if (sessionBase == null)
            {
                return;
            }

            SetupUI();

            if (_outputLogUIComponent != null)
            {
                float setHeight = this.position.size.y - _bottomPadding;
                _outputLogUIComponent.SetHeight(setHeight);
                _outputLogUIComponent.OnGUI(HEU_CookLogs.Instance.GetCookLogString());
            }


            if (GUILayout.Button("Delete Log File"))
            {
                HEU_CookLogs.Instance.DeleteCookingFile();
            }
        }

        private void OnClearLog()
        {
            HEU_CookLogs.Instance.ClearCookLog();
        }

        private void OnInspectorUpdate()
        {
            if (HEU_PluginSettings.WriteCookLogs)
            {
                Repaint();
            }
        }
    }
} // HoudiniEngineUnity
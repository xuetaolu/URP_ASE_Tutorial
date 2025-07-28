using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace HoudiniEngineUnity
{
    // Wrapper class for reusable Message log component
    public class HEU_OutputLogUIComponent
    {
        private GUIContent _titleContent;
        private GUIStyle _eventMessageStyle;

        private GUIStyle _backgroundStyle;

        private Vector2 _eventMessageScrollPos = new Vector2();
        private Action _onClearCallback;
        private float _height;

        private string _message;

        public HEU_OutputLogUIComponent(GUIContent titleContent, Action onClearCallback, float height = 120)
        {
            _titleContent = titleContent;
            _onClearCallback = onClearCallback;
            _height = height;
        }

        public void SetupUI()
        {
            _backgroundStyle = new GUIStyle(GUI.skin.box);
            RectOffset br = _backgroundStyle.margin;
            br.top = 10;
            br.bottom = 6;
            br.left = 4;
            br.right = 4;
            _backgroundStyle.margin = br;

            _eventMessageStyle = new GUIStyle(EditorStyles.textArea);
            _eventMessageStyle.richText = true;

            _eventMessageStyle.normal.textColor = new Color(1f, 1f, 1f, 1f);
            _eventMessageStyle.normal.background = HEU_GeneralUtility.MakeTexture(1, 1, new Color(0, 0, 0, 1f));
        }

        public void SetHeight(float height)
        {
            _height = height;
        }

        private void SetScrollToBottom()
        {
            float lineHeight = _eventMessageStyle.lineHeight;
            int maxDisplayedLines = Mathf.CeilToInt(_height / lineHeight);
            int numLines = _message.Split('\n').Length;
            _eventMessageScrollPos.y = numLines * lineHeight;
        }

        public void OnGUI(string message)
        {
            if (_message != message)
            {
                _message = message;
                SetScrollToBottom();
            }

            using (new EditorGUILayout.VerticalScope(_backgroundStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(_titleContent);

                    if (GUILayout.Button("Clear"))
                    {
                        if (_onClearCallback != null)
                        {
                            _onClearCallback();
                        }
                    }
                }

                using (var scrollViewScope =
                       new EditorGUILayout.ScrollViewScope(_eventMessageScrollPos, GUILayout.Height(_height)))
                {
                    _eventMessageScrollPos = scrollViewScope.scrollPosition;
                    GUILayout.Label(message, _eventMessageStyle);
                }
            }
        }
    }
}
#if UNITY_EDITOR

namespace BrainFailProductions.PolyFew
{


    using UnityEngine;
    using UnityEditor;
    using System;

    public class PopupToggleTemplate : PopupWindowContent
    {

        public ToggleDefinition[] togglesDefinitions;
        public Vector2 windowSize;
        public Action OnPopupOpen;
        public Action OnPopupClose;


        public PopupToggleTemplate(ToggleDefinition[] togglesDefinitions, Vector2 windowSize, Action OnPopupOpen, Action OnPopupClose)
        {
            this.togglesDefinitions = togglesDefinitions;
            this.windowSize = windowSize;
            this.OnPopupOpen = OnPopupOpen;
            this.OnPopupClose = OnPopupClose;
        }



        public class ToggleDefinition
        {
            public GUIContent content;
            public int rightPadding;
            public int bottomPadding;
            public Action<bool> Setter;
            public Func<bool> Getter;
            public bool isButton;
            public Func<bool> ButtonDrawer;

            public ToggleDefinition(GUIContent content, int rightPadding, int bottomPadding, Action<bool> setter = null, Func<bool> getter = null, bool isButton = false, Func<bool> ButtonDrawer = null)
            {
                this.content = content;
                this.rightPadding = rightPadding;
                this.bottomPadding = bottomPadding;
                Setter = setter;
                Getter = getter;
                this.isButton = isButton;
                this.ButtonDrawer = ButtonDrawer;
            }
        }




        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public override void OnGUI(Rect rect)
        {

            if (togglesDefinitions != null && togglesDefinitions.Length > 0)
            {

                GUILayout.BeginArea(new Rect(rect), EditorStyles.miniButton);


                for (int a = 0; a < togglesDefinitions.Length; a++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                    ToggleDefinition definition = togglesDefinitions[a];

                    GUILayout.BeginHorizontal();
                    bool result = false;

                    if (!definition.isButton)
                    {
                        EditorGUILayout.LabelField(definition.content, GUILayout.Width(definition.rightPadding));
                        result = EditorGUILayout.Toggle("", definition.Getter(), GUILayout.Width(25));

                    }
                    else
                    {
                        definition.ButtonDrawer?.Invoke();
                    }

                    GUILayout.EndHorizontal();

                    if (!definition.isButton) { definition.Setter(result); }
                    else
                    {
                        //definition.Setter(result);
                    }
                    


                    EditorGUILayout.EndVertical();

                    if (a != togglesDefinitions.Length - 1) { GUILayout.Space(definition.bottomPadding); }
                }


                GUILayout.EndArea();

            }

        }



        public override void OnOpen() { OnPopupOpen?.Invoke(); }

        public override void OnClose() { OnPopupClose?.Invoke(); }

    }

}


#endif
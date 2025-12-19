#if UNITY_2021_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Reflection;
using System;

namespace HoudiniEngineUnity
{
    [Inspector(typeof(HEU_UnitButton))]
    public class HEU_UnitButtonInspector : Inspector
    {
        public HEU_UnitButtonInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return 16;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var attribute = metadata.GetAttribute<HEU_UnitButtonAttribute>(true);
            if (attribute != null)
            {
                string functionName = attribute.functionName;
                string btnLabel = attribute.buttonLabel;
                int btnWidth = attribute.buttonWidth;

                var buttonPosition = new Rect(
                    position.x,
                    position.y,
                    position.width + btnWidth,
                    16
                );

                if (GUI.Button(buttonPosition, btnLabel, new GUIStyle(UnityEditor.EditorStyles.miniButton)))
                {
                    if (attribute != null)
                    {
                        object typeObject = metadata.parent.value;
                        GraphReference reference = GraphWindow.activeReference;
                        typeObject.GetType().GetMethod(functionName).Invoke(typeObject, new object[1] { reference });
                    }
                }
            }
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

public class tools : MonoBehaviour
{
    [MenuItem("CONTEXT/TimelineClip/Open Custom Tool")]
    private static void OpenCustomTool(MenuCommand command)
    {
        // TimelineClip timelineClip = (TimelineClip)command.context;
        //
        // if (timelineClip != null)
        // {
        //     // 在这里打开你的自定义工具
        //     Debug.Log("Custom Tool Opened for TimelineClip: " + timelineClip.displayName);
        // }
    }
}
#endif

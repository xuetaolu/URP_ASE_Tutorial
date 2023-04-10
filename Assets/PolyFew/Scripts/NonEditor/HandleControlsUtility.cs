//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////


#if UNITY_EDITOR

using UnityEngine;


namespace BrainFailProductions.PolyFew
{

    public class HandleControlsUtility : System.IDisposable
    {


        public static HandleControlsUtility handleControls;


        public static readonly string s_xAxisMoveHandleHash = "xAxisFreeMoveHandleHash";

        public static readonly string s_yAxisMoveHandleHash = "yAxisFreeMoveHandleHash";

        public static readonly string s_zAxisMoveHandleHash = "zAxisFreeMoveHandleHash";

        public static readonly string s_FreeMoveHandleHash = "FreeMoveHandleHash";

        public static readonly string s_xzAxisMoveHandleHash = "xzAxisFreeMoveHandleHash";

        public static readonly string s_xyAxisMoveHandleHash = "xyAxisFreeMoveHandleHash";

        public static readonly string s_yzAxisMoveHandleHash = "yzAxisFreeMoveHandleHash";


        public static readonly string s_xRotateHandleHash = "xRotateHandleHash";

        public static readonly string s_yRotateHandleHash = "yRotateHandleHash";

        public static readonly string s_zRotateHandleHash = "zRotateHandleHash";

        public static readonly string s_camRotateHandleHash = "cameraAxisRotateHandleHash";

        public static readonly string s_xyzRotateHandleHash = "xyzRotateHandleHash";


        public static readonly int[] handleControlsIds;


        public enum HandleControls
        {
            xAxisMoveHandle = 0,

            yAxisMoveHandle,

            zAxisMoveHandle,

            xyAxisMoveHandle,

            xzAxisMoveHandle,

            yzAxisMoveHandle,

            allAxisMoveHandle,



            xAxisRotateHandle,

            yAxisRotateHandle,

            zAxisRotateHandle,

            camera,

            allAxisRotateHandle,

            unknown
        }



        static HandleControlsUtility()
        {
            if (handleControlsIds == null)
            {
                handleControlsIds = new int[12];

                handleControlsIds[0] = GetHandlesControlId(s_xAxisMoveHandleHash);
                handleControlsIds[1] = GetHandlesControlId(s_yAxisMoveHandleHash);
                handleControlsIds[2] = GetHandlesControlId(s_zAxisMoveHandleHash);
                handleControlsIds[3] = GetHandlesControlId(s_xyAxisMoveHandleHash);
                handleControlsIds[4] = GetHandlesControlId(s_xzAxisMoveHandleHash);
                handleControlsIds[5] = GetHandlesControlId(s_yzAxisMoveHandleHash);
                handleControlsIds[6] = GetHandlesControlId(s_FreeMoveHandleHash);
                handleControlsIds[7] = GetHandlesControlId(s_xRotateHandleHash);
                handleControlsIds[8] = GetHandlesControlId(s_yRotateHandleHash);
                handleControlsIds[9] = GetHandlesControlId(s_zRotateHandleHash);
                handleControlsIds[10] = GetHandlesControlId(s_camRotateHandleHash);
                handleControlsIds[11] = GetHandlesControlId(s_xyzRotateHandleHash);

            }

        }



        public int GetControlId(HandleControls handleControl)
        {
            int index = (int)handleControl;
            if (index >= handleControlsIds.Length) { return -1; }
            return handleControlsIds[index];
        }



        public HandleControls GetControlFromId(int handleControlId)
        {
            HandleControls control = HandleControls.unknown;

            for (int a = 0; a < handleControlsIds.Length; a++)
            {
                if (handleControlsIds[a] == handleControlId) { control = (HandleControls)a; break; }
            }

            return control;
        }



        public HandleControls GetCurrentSelectedControl()
        {
            return GetControlFromId(GUIUtility.hotControl);
        }



        public static int GetHandlesControlId(string controlName)
        {
            return GUIUtility.GetControlID(controlName.GetHashCode(), FocusType.Passive);
        }



        public HandleType GetManipulatedHandleType()
        {
            var selectedControl = GetCurrentSelectedControl();

            switch (selectedControl)
            {
                case HandleControls.xAxisMoveHandle:
                case HandleControls.yAxisMoveHandle:
                case HandleControls.zAxisMoveHandle:
                case HandleControls.allAxisMoveHandle:
                    Debug.Log("Pos selected");
                    return HandleType.position;

                case HandleControls.xAxisRotateHandle:
                case HandleControls.yAxisRotateHandle:
                case HandleControls.zAxisRotateHandle:
                case HandleControls.allAxisRotateHandle:
                    Debug.Log("Rot selected");
                    return HandleType.rotation;

                default:
                    Debug.Log("None selected");
                    return HandleType.none;
            }
        }



        public HandleType GetHandleType(HandleControls selectedControl)
        {

            switch (selectedControl)
            {
                case HandleControls.xAxisMoveHandle:
                case HandleControls.yAxisMoveHandle:
                case HandleControls.zAxisMoveHandle:
                case HandleControls.allAxisMoveHandle:
                    return HandleType.position;

                case HandleControls.xAxisRotateHandle:
                case HandleControls.yAxisRotateHandle:
                case HandleControls.zAxisRotateHandle:
                case HandleControls.allAxisRotateHandle:
                    return HandleType.rotation;

                default:
                    return HandleType.none;
            }
        }



        public enum HandleType
        {
            position,
            rotation,
            none
        }


        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

}


#endif
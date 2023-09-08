// @author : xue
// @created : 2023,09,08,15:58
// @desc:

using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散.script
{
    [ExecuteAlways]
    public class ReverseCameraTrans : MonoBehaviour
    {
        public enum MatrixType
        {
            MATRIX_V,
            MATRIX_VP_Opengl,
        }
        
        [TextArea(4,6)]
        public string matrix_strs;
        
        public bool needTransposition;

        public MatrixType matrixType = MatrixType.MATRIX_V;

        public Vector3 cameraWorldPos;
        public Vector3 cameraWorldRotate;

        public Vector3 _real_right;
        public Vector3 _right;
        
        public Vector3 _real_up;
        public Vector3 _up;
        
        public Vector3 _real_forward;
        public Vector3 _forward;
        
        
        
        

        private void Update()
        {
            Matrix4x4 toWorldMatrixInv = Matrix4x4.identity;
            
            if (matrix_strs == null)
                return;

            String[] numbers = Regex.Split(matrix_strs.Trim(), @"[\t\n\r,; ]+");
            if (numbers.Length == 16)
            {
                for (int i = 0; i < numbers.Length; i++)
                {
                    int row = i / 4;
                    int column = i % 4;
                    float number = float.Parse(numbers[i]);
                    // projectMatrix[i] = number;
                    toWorldMatrixInv[row, column] = number;
                }
            }
            else
            {
                return;
            }

            if (needTransposition)
                toWorldMatrixInv = toWorldMatrixInv.transpose;

            Matrix4x4 toWorldMatrix = toWorldMatrixInv.inverse;


            Vector4 worldPosV4 = toWorldMatrix * new Vector4(0, 0, 0, 1);
            worldPosV4 /= worldPosV4.w;

            Vector3 worldPos = worldPosV4;
 
            cameraWorldPos = worldPos;

            Matrix4x4 toWorldRotateMatrix = toWorldMatrix;
            toWorldRotateMatrix[0,3] = 0;
            toWorldRotateMatrix[1,3] = 0;
            toWorldRotateMatrix[2,3] = 0;
            
            toWorldRotateMatrix[3,0] = 0;
            toWorldRotateMatrix[3,1] = 0;
            toWorldRotateMatrix[3,2] = 0;
            
            toWorldRotateMatrix[3,3] = 1;

            Vector3 right; 
            Vector3 up; 
            Vector3 forward;
               
            if (matrixType == MatrixType.MATRIX_VP_Opengl)
            {
                Vector4 rightV4 = (toWorldMatrix * new Vector4(100, 0, -1, 1));
                Vector4 upV4 = (toWorldMatrix * new Vector4(0, 100, -1, 1));
                Vector4 forwardV4 = (toWorldMatrix * new Vector4(0, 0, 1, 1));
                right = rightV4 / rightV4.w;
                up = upV4 / upV4.w;
                forward = forwardV4 / forwardV4.w;
                
                right -= worldPos;
                up -= worldPos;
                forward -= worldPos;
                right = right.normalized;
                up = up.normalized;
                forward = forward.normalized;
            }
            else
            {
                right = ((Vector3)(toWorldRotateMatrix * Vector3.right)).normalized;
                up = ((Vector3)(toWorldRotateMatrix * Vector3.up)).normalized;
                forward = ((Vector3)(toWorldRotateMatrix * Vector3.back)).normalized;
            }


            var transform1 = transform;
            _real_right = transform1.right;
            _real_up = transform1.up;
            _real_forward = transform1.forward;
            
            _right = right;
            _up = up;
            _forward = forward;

            Quaternion q = Quaternion.LookRotation(forward, up);
            cameraWorldRotate = q.eulerAngles;
        }


    }
}
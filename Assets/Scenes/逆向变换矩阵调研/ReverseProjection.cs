// @author : xue
// @created : 2023,08,04,9:43
// @desc:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace Scenes.逆向变换矩阵调研
{
    [ExecuteAlways]
    public class ReverseProjection : MonoBehaviour
    {
        public enum ProjectMode
        {
            Opengl,
            DirectX,
        }

        [TextArea(4,6)]
        public string matrix_strs;

        public bool needTransposition;

        public ProjectMode projectMode;
        public float projectFar;
        public float projectNear;
        public float projectFov;
        public float projectAspect;

        public Matrix4x4 projectMatrix;

        private static Vector3[] clipPositions = new Vector3[]
        {
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1), // 中上多一个
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, 1),
            new Vector3(-1, -1, 1),

            new Vector3(1, 1, 0f),
            new Vector3(0, 1, 0), // 中上多一个
            new Vector3(-1, 1, 0f),
            new Vector3(1, -1, 0f),
            new Vector3(-1, -1, 0f),

            new Vector3(1, 1, -1), 
            new Vector3(0, 1, -1), // 中上多一个
            new Vector3(-1, 1, -1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1),

        };

        private static Vector3[][] clipPosLines = new[]
        {
            new[] { new Vector3(1, 1, 1), new Vector3(1, 1, 0f), /*new Vector3(1, 1, -1),*/ },
            new[] { new Vector3(-1, 1, 1), new Vector3(-1, 1, 0f), /*new Vector3(-1, 1, -1),*/ },
            new[] { new Vector3(1, -1, 1), new Vector3(1, -1, 0f), /*new Vector3(1, -1, -1),*/ },
            new[] { new Vector3(-1, -1, 1), new Vector3(-1, -1, 0f), /*new Vector3(-1, -1, -1),*/ },
            new[] { new Vector3(0, 1, 1), new Vector3(0, 1, 0), /*new Vector3(0, 1, -1),*/ }, //中上多一个
        };

        Dictionary<Vector3, Vector3> dictionary = new Dictionary<Vector3, Vector3>()
            {
           };
        

        private void Update()
        {
            projectMatrix = Matrix4x4.identity;
            
            // String[] splits = Regex.Split(matrix_strs, @"[\n\r]+");
            // String[] fourLines = GetFourLines(splits);
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
                    projectMatrix[row, column] = number;
                }
            }


            if (needTransposition)
                projectMatrix = projectMatrix.transpose;


            

            Matrix4x4 projectInv = projectMatrix.inverse;

            foreach (var clipPosV3 in clipPositions)
            {
                // Vector4 clipPos = kv.Key;
                // Transform trans = kv.Value;
                // if (trans == null)
                //     return;
                Vector4 clipPos = new Vector4(clipPosV3.x, clipPosV3.y, clipPosV3.z, 1.0f);
                Vector4 worldPosV4 = projectInv * clipPos;
                worldPosV4 = worldPosV4 / worldPosV4.w;

                worldPosV4 = transform.localToWorldMatrix * worldPosV4;
                // trans.position = worldPosV4;
                dictionary[clipPosV3] = worldPosV4;
            }

            // clipPos:
            //   opengl 最简单，最远是 1，最近是 -1；右上是 (1, 1)
            //   DirectX，最远是 0，最近是 1， (Z 深度颠倒)，右上是 (1, -1) (上下颠倒)
            Vector3 clipPosV3FarRightUp = projectMode == ProjectMode.Opengl ? new Vector3(1, 1, 1) : new Vector3(1, -1, 0);
            Vector3 clipPosV3NearRightUp = projectMode == ProjectMode.Opengl ? new Vector3(1, 1, -1) : new Vector3(1, -1, 1);
            
            Vector4 viewPosFarRightUp = projectInv * new Vector4(clipPosV3FarRightUp.x, clipPosV3FarRightUp.y, clipPosV3FarRightUp.z, 1.0f);
            viewPosFarRightUp /= viewPosFarRightUp.w;
            
            Vector4 viewPosNearRightUp = projectInv * new Vector4(clipPosV3NearRightUp.x, clipPosV3NearRightUp.y, clipPosV3NearRightUp.z, 1.0f);
            viewPosNearRightUp /= viewPosNearRightUp.w;

            // unity viewPos Z 取反
            projectFar = -viewPosFarRightUp.z;
            projectNear = -viewPosNearRightUp.z;

            projectFov = Mathf.Rad2Deg * Mathf.Atan2(viewPosFarRightUp.y , -viewPosFarRightUp.z) * 2.0f;
            projectAspect = viewPosFarRightUp.x / viewPosFarRightUp.y;


        }
        

        Color GetColor01FromVector3n11(Vector3 v3)
        {
            Vector3 colorV3 = (Vector3)(v3 * 0.5f) + Vector3.one * 0.5f;
            Color color = new Color(colorV3.x, colorV3.y, colorV3.z, 1.0f);
            return color;
        }

        private void OnDrawGizmos()
        {
            foreach (var kv in dictionary)
            {
                Vector3 clipPos = kv.Key;
                Color color = GetColor01FromVector3n11(clipPos);
                Gizmos.DrawIcon(kv.Value, $"{clipPos.x}, {clipPos.y}, {clipPos.z}", true, color);
            }

            foreach (var clipPoses in clipPosLines)
            {
                for (int i = 1; i < clipPoses.Length; i++)
                {
                    Vector3 cp1 = clipPoses[i - 1];
                    Vector3 cp2 = clipPoses[i];

                    if (!dictionary.ContainsKey(cp1) || !dictionary.ContainsKey(cp2))
                        continue;
                    
                    Vector3 position1 = dictionary[cp1];
                    Vector3 position2 = dictionary[cp2];
                    Gizmos.color = GetColor01FromVector3n11(0.5f * (cp1 + cp2));
                    Gizmos.DrawLine(position1, position2);
                    Gizmos.color = Color.white;
                }
            }
        }
    }
}
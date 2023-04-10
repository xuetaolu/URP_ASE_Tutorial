using UnityEngine;

namespace BrainFailProductions.PolyFew.AsImpL.MathUtil
{
    /// <summary>
    /// Vertex structure used for triangulation.
    /// </summary>
    /// <seealso cref="Triangulation"/>
    public class Vertex
    {
        private Vertex prevVertex;
        private Vertex nextVertex;
        private float triangleArea;
        private bool triangleHasChanged;

        /// <summary>
        /// Coordinates in 3D space.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Saved index in the original list.
        /// </summary>
        public int OriginalIndex { get; private set; }

        /// <summary>
        /// Reference to the previous vertex this vertex is attached to.
        /// </summary>
        public Vertex PreviousVertex
        {
            get
            {
                return prevVertex;
            }

            set
            {
                triangleHasChanged = prevVertex != value;
                prevVertex = value;
            }
        }

        /// <summary>
        /// Reference to the next vertex this vertex is attached to.
        /// </summary>
        public Vertex NextVertex
        {
            get
            {
                return nextVertex;
            }

            set
            {
                triangleHasChanged = nextVertex != value;
                nextVertex = value;
            }
        }

        /// <summary>
        /// Area of the triangle this vertex belogs to,
        /// automatically computed each time the connected vertices change.
        /// </summary>
        public float TriangleArea
        {
            get
            {
                if (triangleHasChanged)
                {
                    ComputeTriangleArea();
                }
                return triangleArea;
            }
        }


        /// <summary>
        /// Construct a Vertex by defining its index in the original list and its position in 3D space.
        /// </summary>
        /// <param name="originalIndex">Index in the original list.</param>
        /// <param name="position">Position in 3D space.</param>
        public Vertex(int originalIndex, Vector3 position)
        {
            OriginalIndex = originalIndex;
            Position = position;
        }


        /// <summary>
        /// Get 2D position of this vertex on the plane defined by the given normal.
        /// </summary>
        /// <param name="planeNormal">Normal of the plane used to project 3D vertices in 2D.</param>
        /// <returns></returns>
        public Vector2 GetPosOnPlane(Vector3 planeNormal)
        {
            Quaternion planeRotation = new Quaternion();
            planeRotation.SetFromToRotation(planeNormal, Vector3.back);

            Vector3 projPos = planeRotation * Position;
            Vector2 pos_2d_xy = new Vector2(projPos.x, projPos.y);

            return pos_2d_xy;
        }


        private void ComputeTriangleArea()
        {
            Vector3 side1 = PreviousVertex.Position - Position;
            Vector3 side2 = NextVertex.Position - Position;
            Vector3 crossProd = Vector3.Cross(side1, side2);
            triangleArea = crossProd.magnitude / 2f;
        }

    }
}

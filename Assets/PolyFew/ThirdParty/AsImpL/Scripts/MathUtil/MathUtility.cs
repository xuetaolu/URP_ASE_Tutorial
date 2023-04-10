using UnityEngine;

namespace BrainFailProductions.PolyFew.AsImpL.MathUtil
{
    /// <summary>
    /// Collection of useful mathematical methods.
    /// </summary>
    public static class MathUtility
    {
        /// <summary>
        /// Clamp list indices.
        /// </summary>
        /// <param name="index">Index to be clamped.</param>
        /// <param name="listSize">Size of the list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Will even work if index is larger/smaller than listSize, so can loop multiple times
        /// From https://www.habrador.com/tutorials/math/9-useful-algorithms/
        /// </remarks>
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }


        //p is the testpoint, and the other points are corners in the triangle

        /// <summary>
        /// Check if a point is inside a triangle.
        /// </summary>
        /// <param name="p1">First corner in the triangle.</param>
        /// <param name="p2">Second corner in the triangle.</param>
        /// <param name="p3">Third corner in the triangle.</param>
        /// <param name="p">Test point</param>
        /// <returns>The result is true if the test point is inside the triangle.</returns>
        /// <remarks>
        /// From https://www.habrador.com/tutorials/math/9-useful-algorithms/
        /// http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        /// </remarks>
        public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

            float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
            float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }


        /// <summary>
        /// Check if a triangle oriented clockwise or counter-clockwise.
        /// </summary>
        /// <param name="v1">First vectex in 2D</param>
        /// <param name="v2">Second vectex in 2D</param>
        /// <param name="v3">Third vectex in 2D</param>
        /// <returns>True if the triangle is oriented clockwise.</returns>
        /// <remarks>
        /// This comes from:
        /// https://www.habrador.com/tutorials/math/9-useful-algorithms/
        /// https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        /// https://en.wikipedia.org/wiki/Curve_orientation
        /// </remarks>
        public static bool IsTriangleOrientedClockwise(Vector2 v1, Vector2 v2, Vector2 v3)
        {

            float determinant = v1.x * v2.y + v3.x * v1.y + v2.x * v3.y - v1.x * v3.y - v3.x * v2.y - v2.x * v1.y;

            bool isClockWise = determinant > 0f;
            return isClockWise;
        }


        /// <summary>
        /// Compute the normal of a vertex of a triangle defined by its 3 vertices.
        /// </summary>
        /// <param name="vert">Vertex of the triangle where the normal is coputed.</param>
        /// <param name="vNext">Next vertex (counter-clockwise)</param>
        /// <param name="vPrev">Previous vertex (counter-clockwise)</param>
        /// <returns>The computed normal.</returns>
        public static Vector3 ComputeNormal(Vector3 vert, Vector3 vNext, Vector3 vPrev)
        {
            Vector3 n = Vector3.Cross(vPrev - vert, vNext - vert);
            n.Normalize();
            return n;
        }

    }
}

using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Mathematical utility algorithms and related data.
/// Derived from Erik Nordeus's tutorial: https://www.habrador.com/tutorials/math/10-triangulation/
/// </summary>
namespace BrainFailProductions.PolyFew.AsImpL.MathUtil
{
    public static class Triangulation
    {
        /// <summary>
        /// Triangulate a convex polygon maximizing its triangles areas.
        /// </summary>
        /// <param name="vertices">Vertices of the polygon (assumed to be convex). </param>
        /// <param name="preserveOriginalVertices">If true a copy is made to preserve the original list of vertices.</param>
        /// <returns>List of computed triangles.</returns>
        public static List<Triangle> TriangulateConvexPolygon(List<Vertex> vertices, bool preserveOriginalVertices = true)
        {
            List<Vertex> poly = preserveOriginalVertices ? new List<Vertex>(vertices) : vertices;
            List<Triangle> triangles = new List<Triangle>();

            while (true)
            {
                if (poly.Count == 3)
                {
                    triangles.Add(new Triangle(poly[0], poly[1], poly[2]));
                    return triangles;
                }
                Vertex currVert = FindMaxAreaEarVertex(poly);
                triangles.Add(ClipTriangle(currVert, poly));
            }

            // Trivial triangulation ()
            //for (int i = 2; i < convexHullpoints.Count; i++)
            //{
            //    Vertex a = convexHullpoints[0];
            //    Vertex b = convexHullpoints[i - 1];
            //    Vertex c = convexHullpoints[i];

            //    triangles.Add(new Triangle(a, b, c));
            //}

            //return triangles;
        }


        /// <summary>
        /// Triangulate a ploygon using ear clipping algorithm.
        /// </summary>
        /// <param name="origVertices">Vertices of the polygon in 3D (assumed to be convex or concave).</param>
        /// <param name="planeNormal">This is basically a 2D algorithm, this is the normal of the plane used to project 3D vertices in 2D.</param>
        /// <param name="meshName"></param>
        /// <param name="preserveOriginalVertices"></param>
        /// <returns>List of triangles as a result of the triangulation process.</returns>
        /// <remarks>
        /// Derived (adapted/refactored) from:
        /// https://www.habrador.com/tutorials/math/10-triangulation/.
        /// The points on the polygon should be ordered counter-clockwise.
        /// The ear clipping algorithm is O(n*n),
        /// another common algorithm is dividing it into trapezoids and it's O(n log n),
        /// one can maybe do it in O(n) time but no such version is known.
        /// Assumes we have at least 4 points.
        /// </remarks>
        public static List<Triangle> TriangulateByEarClipping(List<Vertex> origVertices, Vector3 planeNormal, string meshName, bool preserveOriginalVertices = true)
        {
            //The list with triangles the method returns
            List<Triangle> triangles = new List<Triangle>();
            List<Vertex> vertices = preserveOriginalVertices ? new List<Vertex>(origVertices) : origVertices;

            //Find the next and previous vertex
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);
                int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

                vertices[i].PreviousVertex = vertices[prevPos];

                vertices[i].NextVertex = vertices[nextPos];
            }

            List<Vertex> earVertices = FindEarVertices(vertices, planeNormal);

            // Triangulate
            while (true)
            {
                //This means we have just one triangle left
                if (vertices.Count == 3)
                {
                    //The final triangle
                    triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
                    break;
                }
                if (earVertices.Count == 0)
                {
                    // try the opposite plane
                    planeNormal = -planeNormal;
                    earVertices = FindEarVertices(vertices, planeNormal);
                }
                if (earVertices.Count == 0)
                {
                    Debug.LogWarningFormat("Cannot find a proper reprojection for mesh '{0}'. Using fallback polygon triangulation.", meshName);
                    Vertex earVertex = vertices[0];
                    Triangle newTriangle = ClipTriangle(earVertex, vertices);
                    triangles.Add(newTriangle);
                }
                else
                {
                    //Make a triangle of the first ear with maximum area
                    Vertex earVertex = FindMaxAreaEarVertex(earVertices);//earVertices[0];
                    Triangle newTriangle = ClipEar(earVertex, earVertices, vertices, planeNormal);
                    triangles.Add(newTriangle);
                }
            }

            return triangles;
        }


        public static Triangle ClipTriangle(Vertex vertex, List<Vertex> vertices)
        {
            Vertex vertexPrev = vertex.PreviousVertex;
            Vertex vertexNext = vertex.NextVertex;

            // Define the triangle with the given vertex
            Triangle newTriangle = new Triangle(vertexPrev, vertex, vertexNext);

            // Remove the vertex from the list
            vertices.Remove(vertex);

            // Update the previous vertex and next vertex (join them with an edge)
            vertexPrev.NextVertex = vertexNext;
            vertexNext.PreviousVertex = vertexPrev;

            return newTriangle;
        }


        private static Triangle ClipEar(Vertex earVertex, List<Vertex> earVertices, List<Vertex> vertices, Vector3 planeNormal)
        {
            Vertex earVertexPrev = earVertex.PreviousVertex;
            Vertex earVertexNext = earVertex.NextVertex;

            Triangle newTriangle = ClipTriangle(earVertex, vertices);

            //Remove the vertex from the lists
            earVertices.Remove(earVertex);

            //...see if we have found a new ear by investigating the two vertices that was part of the ear

            if (IsVertexEar(earVertexPrev, vertices, planeNormal))
            {
                earVertices.Add(earVertexPrev);
            }
            else
            {
                earVertices.Remove(earVertexPrev);
            }
            if (IsVertexEar(earVertexNext, vertices, planeNormal))
            {
                earVertices.Add(earVertexNext);
            }
            else
            {
                earVertices.Remove(earVertexNext);
            }
            return newTriangle;
        }


        /// <summary>
        /// Find the vertex of the ear with the maximum area.
        /// </summary>
        /// <param name="earVertices">List of ear vertices</param>
        /// <returns>The vertex of the ear with the maximum area.</returns>
        private static Vertex FindMaxAreaEarVertex(List<Vertex> earVertices)
        {
            Vertex maxEarVertex = earVertices[0];
            foreach (Vertex v in earVertices)
            {
                if (v.TriangleArea < maxEarVertex.TriangleArea)
                {
                    maxEarVertex = v;
                }
            }
            return maxEarVertex;
        }


        private static List<Vertex> FindEarVertices(List<Vertex> vertices, Vector3 planeNormal)
        {
            //Have to find the ears after we have found if the vertex is reflex or convex
            List<Vertex> earVertices = new List<Vertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                if (IsVertexEar(vertices[i], vertices, planeNormal))
                {
                    earVertices.Add(vertices[i]);
                }
            }

            return earVertices;
        }


        // Check if a vertex if reflex (concave)
        private static bool IsVertexReflex(Vertex v, Vector3 vNormal)
        {
            //This is a reflex vertex if its triangle is oriented clockwise
            Vector2 a = v.PreviousVertex.GetPosOnPlane(vNormal);
            Vector2 b = v.GetPosOnPlane(vNormal);
            Vector2 c = v.NextVertex.GetPosOnPlane(vNormal);

            return !MathUtility.IsTriangleOrientedClockwise(a, b, c);
        }


        //Check if a vertex is an ear
        private static bool IsVertexEar(Vertex v, List<Vertex> vertices, Vector3 planeNormal)
        {
            //A reflex vertex can't be an ear!
            if (IsVertexReflex(v, planeNormal))
            {
                return false;
            }

            //This triangle to check point in triangle
            Vector2 a = v.PreviousVertex.GetPosOnPlane(planeNormal);
            Vector2 b = v.GetPosOnPlane(planeNormal);
            Vector2 c = v.NextVertex.GetPosOnPlane(planeNormal);

            bool hasPointInside = false;

            for (int i = 0; i < vertices.Count; i++)
            {
                //We only need to check if a reflex vertex is inside of the triangle
                if (v != vertices[i] && v.PreviousVertex != vertices[i] && v.NextVertex != vertices[i] && IsVertexReflex(vertices[i], planeNormal))
                {
                    Vector2 p = vertices[i].GetPosOnPlane(planeNormal);

                    //This means inside and not on the hull
                    if (MathUtility.IsPointInTriangle(a, b, c, p))
                    {
                        hasPointInside = true;

                        break;
                    }
                }
            }

            if (!hasPointInside)
            {
                return true;
            }
            return false;
        }

    }
}

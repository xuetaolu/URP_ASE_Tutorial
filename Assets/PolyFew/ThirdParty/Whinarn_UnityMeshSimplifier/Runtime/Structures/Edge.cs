#region License
/*
MIT License
Copyright(c) 2020 Normand Côté
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityMeshSimplifier
{
    public class Edge
    {
        public enum QState
        {
            QIsNotCalculated = 0,
            QIsCalculated = 1,
            QPenaltyIsCalculated = 2,
            ErrorIsCalculated = 3
        }

        public Triangle containingTriangle;
        public int vertexA;
        public int vertexB;
        public int vSmall;
        public int vLarge;
        public ulong key;
        public int hashCode;
        public double error;
        public double errorKeyed; // error at the time of insertion in the sorted list
        public Vector3d p; // point that minimize the error on this edge
        public SymmetricMatrix q; // quadrics error matrix
        public SymmetricMatrix qTwice; // quadrics error matrix from triangles touching both vertices of the edge (to perform unioning)
        public SymmetricMatrix qPenaltyBorderVertexA; // quadrics error matrix to protect the edge and applied at vertex A
        public SymmetricMatrix qPenaltyBorderVertexB; // same for vertex B
        public bool isDeleted;
        public bool isBorder2D;
        public bool isUVSeam;
        public bool isUVFoldover;
        public int index;
        public QState flagCalculateQstate;

        public readonly int ova;
        public readonly int ovb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Edge(int vertexA, int vertexB)
        {
            InitIndex(vertexA, vertexB);
            ova = vertexA;
            ovb = vertexB;
            isDeleted = false;
            error = double.MaxValue; // until the error is calculated, this edge would fall at the end the sorted list.
            errorKeyed = double.MaxValue;
            isBorder2D = false;
            isUVSeam = false;
            isUVFoldover = false;
            index = -1;
            flagCalculateQstate = Edge.QState.QIsNotCalculated;
            q = new SymmetricMatrix(0);
            qTwice = new SymmetricMatrix(0);
            qPenaltyBorderVertexA = new SymmetricMatrix(0);
            qPenaltyBorderVertexB = new SymmetricMatrix(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitIndex(int vertexA, int vertexB)
        {
            if (vertexA == vertexB)
                throw new System.ArgumentException("Vertices cannot be the same on one edge", "vertexA and vertexB");
            if (vertexA < vertexB)
            {
                this.vSmall = vertexA;
                this.vLarge = vertexB;
            }
            else
            {
                this.vSmall = vertexB;
                this.vLarge = vertexA;
            }

            this.vertexA = vertexA;
            this.vertexB = vertexB;

            hashCode = Tuple.Create(this.vSmall, this.vLarge).GetHashCode();

            key = CalculateKey(this.vSmall, this.vLarge);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReplaceVertex(int oldVertex, int newVertex)
        {
            if (oldVertex == vertexA)
                InitIndex(newVertex, vertexB);
            else if (oldVertex == vertexB)
                InitIndex(vertexA, newVertex);
            else
                throw new System.ArgumentException("Vertex does not exist on this edge", "oldVertex");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKey(int vertexA, int vertexB)
        {
            if (vertexA < vertexB)
                return (((ulong)vertexA) << 32) | (uint)vertexB;
            else
                return (((ulong)vertexB) << 32) | (uint)vertexA;
        }


        public override string ToString()
        {
            return string.Format("edge ({0}, {1}) err({2:0.###E+000}, {3:0.###E+000}) {4}{5}{6}", vSmall, vLarge, errorKeyed, error,
                isDeleted ? " deleted" : "", isBorder2D ? " border2D" : "", error == errorKeyed ? "" : (error < errorKeyed ? " Lag" : " Lead"));
        }

    }
}

public static class EdgeListExt
{
    private static EdgeErrorkeyedComparer eec = new EdgeErrorkeyedComparer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddSortedFromPosition<Edge>(this List<Edge> edgeList, int position, Edge edge)
    {
        int index = edgeList.BinarySearch(position, edgeList.Count - position, edge, (System.Collections.Generic.IComparer<Edge>)eec);
        if (index < 0)
            index = ~index;
        edgeList.Insert(index, edge);
    }
}

internal class EdgeErrorkeyedComparer : IComparer<UnityMeshSimplifier.Edge>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(UnityMeshSimplifier.Edge e0, UnityMeshSimplifier.Edge e1)
    {
        int result = 0;
        if (e0.errorKeyed < e1.errorKeyed)
            result = -1;
        else if (e0.errorKeyed > e1.errorKeyed)
            result = 1;

        return result;
    }
}
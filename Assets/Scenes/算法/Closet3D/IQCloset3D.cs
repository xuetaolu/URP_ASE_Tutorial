// The MIT License
// Copyright © 2021 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


// Closest point on a 3D box. For closest points on other primitives, check
//
//    https://www.shadertoy.com/playlist/wXsSzB

// @author : xue
// @created : 2023,06,28,10:25
// @desc:

using Freya;
using UnityEngine;

namespace Scenes.Closet3D
{
    public class IQCloset3D
    {
        /// <summary>
        /// Returns the closest point o, a 3D box
        ///   p is the point we are at
        ///   b is the box radius (3 half side lengths)
        ///   The box is axis aligned and centered at the origin. For a box rotated 
        ///   by M,you need to transform p and the returned point by inverse(M).
        /// </summary>
        /// <param name="p">is the point we are at</param>
        /// <param name="b">is the box radius (3 half side lengths)</param>
        /// <param name="allowInner">点在内部时是否直接返回输入位置</param>
        /// <returns></returns>
        public static Vector3 closestPointToBox( Vector3 p, Vector3 b, bool allowInner = false )
        {
            Vector3 d = p.Abs() - b;
            if (d.Max() <= 0 && allowInner)
                return p;
            float  m = Mathf.Min(0.0f,Mathf.Max(d.x,Mathf.Max(d.y,d.z)));
            return p - Vector3.Scale(
                new Vector3(d.x >= m ? d.x : 0.0f,
                        d.y >= m ? d.y : 0.0f,
                        d.z >= m ? d.z : 0.0f),
                p.Sign()) ;
        }
        
        // // Alternative implementation
        // public static Vector3  closestPointToBox2( Vector3 p, Vector3 b )
        // {
        //     Vector3 d = p.Abs() - b;
        //     Vector3 s = p.Sign();
        //
        //     // interior
        //     Vector3 q; float ma;
        //                  { q=p; q.x=s.x*b.x; ma=d.x; }
        //     if( d.y>ma ) { q=p; q.y=s.y*b.y; ma=d.y; }
        //     if( d.z>ma ) { q=p; q.z=s.z*b.z; ma=d.z; }
        //     if( ma<0.0 ) return q;
        //
        //     // exterior
        //     return p - Vector3.Scale(s,Vector3.Max(d,Vector3.zero)) ;
        // }
    }
}
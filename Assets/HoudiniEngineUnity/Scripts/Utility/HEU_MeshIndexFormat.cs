/*
 * Copyright (c) <2020> Side Effects Software Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * 2. The name of Side Effects Software may not be used to endorse or
 *    promote products derived from this software without specific prior
 *    written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
 * NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using UnityEngine;

namespace HoudiniEngineUnity
{
    /// <summary>
    /// Wraps UnityEngine.Rendering.IndexFormat which isn't available in older Unity versions.
    /// </summary>
    [System.Serializable]
    public class HEU_MeshIndexFormat
    {
#if UNITY_2017_3_OR_NEWER
        // Store the type of the index buffer size. By default use 16-bit, but will change to 32-bit if 
        // for large vertex count.
        public UnityEngine.Rendering.IndexFormat _indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
#endif

        /// <summary>
        /// Calculate the index format based on number of vertices.
        /// </summary>
        public void CalculateIndexFormat(int numVertices)
        {
            uint maxVertexCount = ushort.MaxValue;
            uint vertexCount = Convert.ToUInt32(numVertices);
            if (vertexCount > maxVertexCount)
            {
#if UNITY_2017_3_OR_NEWER
                // For vertex count larger than 16-bit, use 32-bit buffer
                _indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
				HEU_Logger.LogErrorFormat("Vertex count {0} which is above Unity maximum of {1}.\nUse Unity 2017.3+ or reduce this in Houdini.",
					vertexCount, maxVertexCount);
#endif
            }
        }

        /// <summary>
        /// Set the given mesh's index format based on current index format setting.
        /// </summary>
        /// <param name="mesh"></param>
        public void SetFormatForMesh(Mesh mesh)
        {
#if UNITY_2017_3_OR_NEWER
            mesh.indexFormat = _indexFormat;
#endif
        }
    }
}
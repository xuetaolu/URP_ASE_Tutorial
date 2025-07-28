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

using UnityEngine;
using System.Collections.Generic;

// Expose internal classes/functions
#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HoudiniEngineUnityEditor")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityEditorTests")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityPlayModeTests")]
#endif


namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_StringHandle = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_PartId = System.Int32;

    /// <summary>
    /// Holds all parameter data for an asset.
    /// </summary>
    public interface IHEU_Curve
    {
        /// <summary>The gameobject containing the curve mesh </summary>
        GameObject TargetGameObject { get; set; }

        /// <summary>The GeoID of the node in Houdini </summary>
        HAPI_NodeId GeoID { get; }

        /// <summary>The PartId of the node in Houdini </summary>
        HAPI_NodeId PartID { get; }

        /// <summary> The CVs of the curve </summary>
        List<CurveNodeData> CurveNodeData { get; }

        /// <summary> The parameters of the curve. </summary>
        HEU_Parameters Parameters { get; }

        /// <summary> The name of the curve. </summary>
        string CurveName { get; }

        /// <summary>Whether or not this is an input curve</summary>
        bool IsInputCurve { get; }

        /// <summary>Whether or not this is a part curve </summary>
        bool IsPartCurve { get; }

        /// <summary> The input curve info. Only valid if it is an input curve. </summary>
        HEU_InputCurveInfo InputCurveInfo { get; }

        /// <summary> Whether or not it is an editable curve.. </summary>
        bool IsEditable();

        /// <summary> Whether or not it is an geo curve (i.e. curve node in Houdini). </summary>
        bool IsGeoCurve();

        /// <summary>
        /// Sets the curve name
        /// </summary>
        /// <param name="name">The name to set it to</param>
        void SetCurveName(string name);

        /// <summary>
        /// Sets a curve point
        /// </summary>
        /// <param name="pointIndex">The point index to set</param>
        /// <param name="newPosition">The position to set at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset after setting</param>
        void SetCurvePoint(int pointIndex, Vector3 newPosition, bool bRecookAsset = false);

        /// <summary>
        /// Sets a curve point
        /// </summary>
        /// <param name="pointIndex">The point index to set</param>
        /// <param name="curveData">The data to set at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset after setting</param>
        void SetCurvePoint(int pointIndex, CurveNodeData curveData, bool bRecookAsset = false);

        /// <summary>
        /// Sets a curve point using the list
        /// </summary>
        /// <param name="curveNodeData">The list of points to set it</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset after setting</param>
        void SetCurveNodeData(List<CurveNodeData> curveNodeData, bool bRecookAsset = false);

        /// <summary>
        /// Gets a curve point at a index
        /// </summary>
        /// <param name="pointIndex">The index of the point</param>
        /// <returns>The position of the curve point</returns>
        Vector3 GetCurvePoint(int pointIndex);

        /// <summary>
        /// Gets the curve points
        /// </summary>
        /// <returns>A list containing all point transforms</returns>
        List<CurveNodeData> GetAllPointTransforms();

        /// <summary>
        /// Gets all the point positions
        /// </summary>
        /// <returns>A list containing all point positions</returns>
        List<Vector3> GetAllPoints();

        /// <summary>
        /// Gets the number of points
        /// </summary>
        /// <returns>Gets the number of points</returns>
        int GetNumPoints();

        /// <summary>
        /// Inserts a curve point at the index
        /// </summary>
        /// <param name="index">The index to insert the newpoint</param>
        /// <param name="position">The position of the new point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void InsertCurvePoint(int index, Vector3 position, bool bRecookAsset = false);

        /// <summary>
        /// Inserts a curve point at the index
        /// </summary>
        /// <param name="index">The index to insert the new point</param>
        /// <param name="curveData">The transform of the new point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void InsertCurvePoint(int index, CurveNodeData curveData, bool bRecookAsset = false);

        /// <summary>
        /// Add a curve point to the end
        /// </summary>
        /// <param name="position">The position of the new point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void AddCurvePointToEnd(Vector3 position, bool bRecookAsset = false);

        /// <summary>
        /// Add a curve point to the end
        /// </summary>
        /// <param name="curveData">The transform of the new point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void AddCurvePointToEnd(CurveNodeData curveData, bool bRecookAsset = false);

        /// <summary>
        /// Removes a curve point at the index
        /// </summary>
        /// <param name="pointIndex">The index to remove the point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void RemoveCurvePoint(int pointIndex, bool bRecookAsset = false);

        /// <summary>
        /// Clears all points on the curve
        /// </summary>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void ClearCurveNodeData(bool bRecookAsset = false);


        /// <summary>
        /// Project curve points onto collider or layer.
        /// </summary>
        /// <param name="rayDirection">Direction to cast ray</param>
        /// <param name="rayDistance">Maximum ray cast distance</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void ProjectToColliders(Vector3 rayDirection, float rayDistance, bool bRecookAsset = false);


        /// <summary>
        /// Sets the curve geometry visibility
        /// </summary>
        /// <param name="bVisible">Whether or not the curve is visible</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetCurveGeometryVisibility(bool bVisible, bool bRecookAsset = false);


        /// <summary>
        /// Gets a transformed point in world space
        /// </summary>
        /// <param name="pointIndex">The index of the point</param>
        /// <returns>The transformed point</returns>
        Vector3 GetTransformedPoint(int pointIndex);

        /// <summary>
        /// Gets a list of the transformed points in world space
        /// </summary>
        /// <returns>The transformed points</returns>
        List<Vector3> GetTransformedPoints();

        /// <summary>
        /// Gets the curve node data by value.
        /// </summary>
        /// <returns>A list containing the curve transforms, copied by value.</returns>
        List<CurveNodeData> DuplicateCurveNodeData();
    }
} // HoudiniEngineUnity
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
    /// PDG asset link link state
    /// </summary>
    public enum HEU_LinkStateWrapper
    {
        INACTIVE,
        LINKING,
        LINKED,
        ERROR_NOT_LINKED
    }

    /// <summary>
    /// PDG asset link
    /// </summary>
    public interface IHEU_PDGAssetLink
    {
        bool AutoCook { get; set; }

        bool UseHEngineData { get; set; }

        // Filter strings
        bool UseTOPNodeFilter { get; set; }
        bool UseTOPOutputFilter { get; set; }
        string TopNodeFilter { get; set; }
        string TopOutputFilter { get; set; }

        HEU_HoudiniAsset ParentAsset { get; }
        string AssetPath { get; }

        GameObject AssetGO { get; }

        string AssetName { get; }

        HAPI_NodeId AssetID { get; }

        List<HEU_TOPNetworkData> TopNetworks { get; }

        string[] TopNetworkNames { get; }

        int SelectedTOPNetwork { get; }

        HEU_LinkStateWrapper PDGLinkState { get; }

        // The root gameobject to place all loaded geometry under
        GameObject LoadRootGameObject { get; }

        // The root directory for generated output
        string OutputCachePathRoot { get; }

        void Setup(HEU_HoudiniAsset hdaAsset);

        /// <summary>
        /// Reset all TOP network and node state.
        /// Should be done after the linked HDA has rebuilt.
        /// </summary>
        void Reset();

        /// <summary>
        /// Refresh this object's internal state by querying and populating TOP network and nodes
        /// from linked HDA.
        /// </summary>
        void Refresh();

        List<KeyValuePair<int, HEU_TOPNodeData>> GetNonHiddenTOPNodes(HEU_TOPNetworkData topNetwork);


        /// <summary>
        /// Set the TOP network at the given index as currently selected TOP network
        /// </summary>
        /// <param name="newIndex">Index of the TOP network</param>
        void SelectTOPNetwork(int newIndex);


        /// <summary>
        /// Set the TOP node at the given index in the given TOP network as currently selected TOP node
        /// </summary>
        /// <param name="network">Container TOP network</param>
        /// <param name="newIndex">Index of the TOP node to be selected</param>
        void SelectTOPNode(HEU_TOPNetworkData network, int newIndex);

        HEU_TOPNetworkData GetSelectedTOPNetwork();

        HEU_TOPNodeData GetSelectedTOPNode();

        HEU_TOPNetworkData GetTOPNetwork(int index);

        /// <summary>
        /// Dirty the specified TOP node and clear its work item results.
        /// </summary>
        /// <param name="topNode"></param>
        void DirtyTOPNode(HEU_TOPNodeData topNode);

        /// <summary>
        /// Cook the specified TOP node.
        /// </summary>
        /// <param name="topNode"></param>
        void CookTOPNode(HEU_TOPNodeData topNode);

        /// <summary>
        /// Dirty the currently selected TOP network and clear all work item results.
        /// </summary>
        void DirtyAll();

        /// <summary>
        /// Cook the output TOP node of the currently selected TOP network.
        /// </summary>
        void CookOutput();

        /// <summary>
        /// Pause the PDG cook of the currently selected TOP network
        /// </summary>
        void PauseCook();

        /// <summary>
        /// Cancel the PDG cook of the currently selected TOP network
        /// </summary>
        void CancelCook();

        HEU_SessionBase GetHAPISession();

        HEU_TOPNodeData GetTOPNode(HAPI_NodeId nodeID);

        string GetTOPNodeStatus(HEU_TOPNodeData topNode);
    }
} // HoudiniEngineUnity
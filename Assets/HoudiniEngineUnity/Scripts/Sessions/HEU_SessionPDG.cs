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

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX)
#define HOUDINIENGINEUNITY_ENABLED
#endif

using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_StringHandle = System.Int32;
    using HAPI_NodeId = System.Int32;
    using HAPI_PDG_WorkItemId = System.Int32;
    using HAPI_PDG_GraphContextId = System.Int32;


    /// <summary>
    /// Session wrapper for HAPI PDG calls.
    /// </summary>
    public static class HEU_SessionPDG
    {
#if HOUDINIENGINEUNITY_ENABLED

        // SESSION ----------------------------------------------------------------------------------------------------

        public static bool GetPDGGraphContexts(this HEU_SessionBase session,
            [Out] HAPI_StringHandle[] context_names_array, [Out] HAPI_PDG_GraphContextId[] context_id_array, int start,
            int length, bool bLogError)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetPDGGraphContexts(ref session.GetSessionData()._HAPISession,
                context_names_array, context_id_array, start, length);
            session.HandleStatusResult(result, "Getting PDG Graph Contexts", false, bLogError);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetPDGGraphContextsCount(this HEU_SessionBase session, out int num_contexts)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_GetPDGGraphContextsCount(ref session.GetSessionData()._HAPISession,
                    out num_contexts);
            session.HandleStatusResult(result, "Getting number of PDG Graph Contexts", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool CookPDG(this HEU_SessionBase session, HAPI_NodeId cook_node_id, int generate_only,
            int blocking)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_CookPDG(ref session.GetSessionData()._HAPISession, cook_node_id,
                generate_only, blocking);
            session.HandleStatusResult(result, "Cooking PDG", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetPDGEvents(this HEU_SessionBase session, HAPI_PDG_GraphContextId graph_context_id,
            [Out] HAPI_PDG_EventInfo[] event_array, int length, out int event_count, out int remaining_events)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetPDGEvents(ref session.GetSessionData()._HAPISession,
                graph_context_id, event_array, length, out event_count, out remaining_events);
            session.HandleStatusResult(result, "Getting PDG Events", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetPDGState(this HEU_SessionBase session, HAPI_PDG_GraphContextId graph_context_id,
            out int pdg_state)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetPDGState(ref session.GetSessionData()._HAPISession,
                graph_context_id, out pdg_state);
            session.HandleStatusResult(result, "Getting PDG State", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool CreateWorkItem(this HEU_SessionBase session, HAPI_NodeId node_id,
            out HAPI_PDG_WorkItemId workitem_id, string name, int index)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_CreateWorkItem(ref session.GetSessionData()._HAPISession,
                node_id, out workitem_id, name.AsByteArray(), index);
            session.HandleStatusResult(result, "Creating work item", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemInfo(this HEU_SessionBase session, HAPI_PDG_GraphContextId graph_context_id,
            HAPI_PDG_WorkItemId workitem_id, ref HAPI_PDG_WorkItemInfo workitem_info)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemInfo(ref session.GetSessionData()._HAPISession,
                graph_context_id, workitem_id, out workitem_info);
            session.HandleStatusResult(result, "Getting WorkItem", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool SetWorkItemIntAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, int[] values_array, int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_SetWorkItemIntAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), values_array,
                length);
            session.HandleStatusResult(result, "Setting work item int attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool SetWorkItemFloatAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, float[] values_array, int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_SetWorkItemFloatAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), values_array,
                length);
            session.HandleStatusResult(result, "Setting work item float attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool SetWorkItemStringAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, int data_index, string value)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_SetWorkItemStringAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), data_index,
                value.AsByteArray());
            session.HandleStatusResult(result, "Setting work item string attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool CommitWorkItems(this HEU_SessionBase session, HAPI_NodeId node_id)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_CommitWorkItems(ref session.GetSessionData()._HAPISession, node_id);
            session.HandleStatusResult(result, "Committing work items", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetNumWorkItems(this HEU_SessionBase session, HAPI_NodeId node_id, out int num)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_GetNumWorkItems(ref session.GetSessionData()._HAPISession, node_id, out num);
            session.HandleStatusResult(result, "Getting number of work items", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItems(this HEU_SessionBase session, HAPI_NodeId node_id,
            [Out] HAPI_PDG_WorkItemId[] workitem_ids, int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItems(ref session.GetSessionData()._HAPISession, node_id,
                workitem_ids, length);
            session.HandleStatusResult(result, "Getting work items", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemAttributeSize(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, out int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemAttributeSize(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), out length);
            session.HandleStatusResult(result, "Getting work item attribute size", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemIntAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, [Out] int[] values_array, int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemIntAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), values_array,
                length);
            session.HandleStatusResult(result, "Getting work item int attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemFloatAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, [Out] float[] values_array, int length)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemFloatAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), values_array,
                length);
            session.HandleStatusResult(result, "Getting work item float attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemStringAttribute(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, string data_name, [Out] HAPI_StringHandle[] values, int length)
        {
            Debug.AssertFormat(values.Length >= length, "StringBuilder must be atleast of size {0}.", length);
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemStringAttribute(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, data_name.AsByteArray(), values,
                length);
            session.HandleStatusResult(result, "Getting work item string attribute", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool GetWorkItemOutputFiles(this HEU_SessionBase session, HAPI_NodeId node_id,
            HAPI_PDG_WorkItemId workitem_id, [Out] HAPI_PDG_WorkItemOutputFile[] resultinfo_array, int resultinfo_count)
        {
            HAPI_Result result = HEU_HAPIFunctions.HAPI_GetWorkItemOutputFiles(
                ref session.GetSessionData()._HAPISession, node_id, workitem_id, resultinfo_array, resultinfo_count);
            session.HandleStatusResult(result, "Getting work item output file", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool DirtyPDGNode(this HEU_SessionBase session, HAPI_NodeId node_id, bool clean_results)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_DirtyPDGNode(ref session.GetSessionData()._HAPISession, node_id, clean_results);
            session.HandleStatusResult(result, "Dirtying PDG Node", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool PausePDGCook(this HEU_SessionBase session, HAPI_PDG_GraphContextId graph_context_id)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_PausePDGCook(ref session.GetSessionData()._HAPISession, graph_context_id);
            session.HandleStatusResult(result, "Pausing PDG Cook", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

        public static bool CancelPDGCook(this HEU_SessionBase session, HAPI_PDG_GraphContextId graph_context_id)
        {
            HAPI_Result result =
                HEU_HAPIFunctions.HAPI_CancelPDGCook(ref session.GetSessionData()._HAPISession, graph_context_id);
            session.HandleStatusResult(result, "Cancel PDG Cook", false, true);
            return (result == HAPI_Result.HAPI_RESULT_SUCCESS);
        }

#endif
    }
} // HoudiniEngineUnity
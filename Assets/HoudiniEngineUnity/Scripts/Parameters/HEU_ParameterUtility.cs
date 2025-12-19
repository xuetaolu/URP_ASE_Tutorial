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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_ParmId = System.Int32;


    /// <summary>
    /// Contains utility functions for working with parameters
    /// </summary>
    public static class HEU_ParameterUtility
    {
        public static bool GetToggle(HEU_HoudiniAsset asset, string paramName, out bool outValue)
        {
            outValue = false;

            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetBoolParameterValue(paramName, out outValue);
        }

        public static bool SetToggle(HEU_HoudiniAsset asset, string paramName, bool setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetBoolParameterValue(paramName, setValue);
        }

        public static bool GetInt(HEU_HoudiniAsset asset, string paramName, out int outValue)
        {
            outValue = 0;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetIntParameterValue(paramName, out outValue);
        }

        public static bool SetInt(HEU_HoudiniAsset asset, string paramName, int setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetIntParameterValue(paramName, setValue);
        }

        public static bool GetFloat(HEU_HoudiniAsset asset, string paramName, out float outValue)
        {
            outValue = 0;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetFloatParameterValue(paramName, out outValue);
        }

        public static bool GetFloats(HEU_HoudiniAsset asset, string paramName, out float[] outValues)
        {
            outValues = null;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetFloatParameterValues(paramName, out outValues);
        }

        public static bool SetFloat(HEU_HoudiniAsset asset, string paramName, float setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetFloatParameterValue(paramName, setValue);
        }

        public static bool SetFloats(HEU_HoudiniAsset asset, string paramName, float[] setValues)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetFloatParameterValues(paramName, setValues);
        }

        public static bool GetString(HEU_HoudiniAsset asset, string paramName, out string outValue)
        {
            outValue = null;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetStringParameterValue(paramName, out outValue);
        }

        public static bool SetString(HEU_HoudiniAsset asset, string paramName, string setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetStringParameterValue(paramName, setValue);
        }

        public static bool SetChoice(HEU_HoudiniAsset asset, string paramName, int setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetChoiceParameterValue(paramName, setValue);
        }

        public static bool GetChoice(HEU_HoudiniAsset asset, string paramName, out int outValue)
        {
            outValue = 0;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetChoiceParameterValue(paramName, out outValue);
        }

        public static bool SetInputNode(HEU_HoudiniAsset asset, string paramName, GameObject obj, int index)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetAssetRefParameterValue(paramName, obj);
        }

        public static bool GetInputNode(HEU_HoudiniAsset asset, string paramName, int index, out GameObject obj)
        {
            obj = null;
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetAssetRefParameterValue(paramName, out obj);
        }

        public static bool GetColor(HEU_HoudiniAsset asset, string paramName, out Color getValue)
        {
            getValue = Color.black;

            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.GetColorParameterValue(paramName, out getValue);
        }

        public static bool SetColor(HEU_HoudiniAsset asset, string paramName, Color setValue)
        {
            if (asset == null || asset.Parameters == null)
            {
                return false;
            }

            return asset.Parameters.SetColorParameterValue(paramName, setValue);
        }

        public static int GetParameterIndexFromName(HEU_SessionBase session, HAPI_ParmInfo[] parameters,
            string parameterName)
        {
            if (parameters != null && parameters.Length > 0)
            {
                int numParameters = parameters.Length;
                for (int i = 0; i < numParameters; ++i)
                {
                    if (HEU_SessionManager.GetString(parameters[i].nameSH, session).Equals(parameterName))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int GetParameterIndexFromNameOrTag(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo[] parameters, string parameterName)
        {
            int parameterIndex = GetParameterIndexFromName(session, parameters, parameterName);
            if (parameterIndex < 0)
            {
                // Try to find tag instead
                parameterIndex = HEU_Defines.HEU_INVALID_NODE_ID;
                session.GetParmWithTag(nodeID, parameterName, ref parameterIndex);
            }

            return parameterIndex;
        }

        public static int FindTextureParamByNameOrTag(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo[] parameters, string parameterName, string useTextureParmName)
        {
            int outParmId = GetParameterIndexFromNameOrTag(session, nodeID, parameters, parameterName);
            if (outParmId < 0)
            {
                return outParmId;
            }

            // Check if the matching "use" parameter exists.
            int foundUseParmId = GetParameterIndexFromNameOrTag(session, nodeID, parameters, useTextureParmName);
            if (foundUseParmId >= 0)
            {
                // Found a valid "use" parameter. Check if it is disabled.
                int[] useValue = new int[1];
                int intValuesIndex = parameters[foundUseParmId].intValuesIndex;

                if (session.GetParamIntValues(nodeID, useValue, parameters[foundUseParmId].intValuesIndex, 1))
                {
                    if (useValue.Length > 0 && useValue[0] == 0)
                    {
                        // We found the texture, but the use tag is disabled, so don't use it!
                        return -1;
                    }
                }
            }

            // Finally, make sure that the found texture parm is not empty!
            int[] parmValueHandle = new int[1];
            if (session.GetParamStringValues(nodeID, parmValueHandle, parameters[outParmId].stringValuesIndex, 1))
            {
                string parmValue = HEU_SessionManager.GetString(parmValueHandle[0], session);
                if (string.IsNullOrEmpty(parmValue))
                {
                    return -1;
                }
            }

            return outParmId;
        }

        public static bool GetParameterFloatValue(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo[] parameters, string parameterName, float defaultValue, out float returnValue)
        {
            int parameterIndex = GetParameterIndexFromNameOrTag(session, nodeID, parameters, parameterName);
            if (parameterIndex < 0 || parameterIndex >= parameters.Length)
            {
                returnValue = defaultValue;
                return false;
            }

            int valueIndex = parameters[parameterIndex].floatValuesIndex;
            float[] value = new float[1];

            if (session.GetParamFloatValues(nodeID, value, valueIndex, 1))
            {
                returnValue = value[0];
                return true;
            }

            returnValue = defaultValue;
            return false;
        }

        public static bool GetParameterColor3Value(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo[] parameters, string parameterName, Color defaultValue, out Color outputColor)
        {
            int parameterIndex = GetParameterIndexFromNameOrTag(session, nodeID, parameters, parameterName);
            if (parameterIndex < 0 || parameterIndex >= parameters.Length)
            {
                outputColor = defaultValue;
                return false;
            }

            if (parameters[parameterIndex].size < 3)
            {
                HEU_Logger.LogError("Parameter size not large enough to be a Color3");
                outputColor = defaultValue;
                return false;
            }

            int valueIndex = parameters[parameterIndex].floatValuesIndex;
            float[] value = new float[3];

            if (session.GetParamFloatValues(nodeID, value, valueIndex, 3))
            {
                outputColor = new Color(value[0], value[1], value[2], 1f);
                return true;
            }

            outputColor = defaultValue;
            return false;
        }
    }
} // HoudiniEngineUnity
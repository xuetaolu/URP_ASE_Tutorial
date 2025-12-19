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
    /// Houdini Input curve ramp interpolation type
    /// </summary>
    public enum HEU_HoudiniRampInterpolationTypeWrapper
    {
        CONSTANT = 0,
        LINEAR = 1,
        CATMULL_ROM = 2
    };

    /// <summary>
    /// Wrapper class to represent a ramp point
    /// </summary>
    public class HEU_RampPointWrapper
    {
        public float Position { get; set; }
        public HEU_HoudiniRampInterpolationTypeWrapper Interpolation { get; set; }

        public HEU_RampPointWrapper(float position = 0,
            HEU_HoudiniRampInterpolationTypeWrapper interpolation = HEU_HoudiniRampInterpolationTypeWrapper.LINEAR)
        {
            this.Position = position;
            this.Interpolation = interpolation;
        }
    }

    /// <summary>
    /// Wrapper class to represent a Houdini float ramp
    /// </summary>
    public class HEU_FloatRampPointWrapper : HEU_RampPointWrapper
    {
        public float Value { get; set; }

        public HEU_FloatRampPointWrapper(float position = 0, float value = 0,
            HEU_HoudiniRampInterpolationTypeWrapper interpolation = HEU_HoudiniRampInterpolationTypeWrapper.LINEAR)
            : base(position, interpolation)
        {
            this.Value = value;
        }
    };

    /// <summary>
    /// Wrapper class to represent a Houdini color ramp
    /// </summary>
    public class HEU_ColorRampPointWrapper : HEU_RampPointWrapper
    {
        public Color Value { get; set; }

        public HEU_ColorRampPointWrapper(float position = 0, Color value = new Color(),
            HEU_HoudiniRampInterpolationTypeWrapper interpolation = HEU_HoudiniRampInterpolationTypeWrapper.LINEAR)
            : base(position, interpolation)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// Wrapper class to represent a parmeter tuple (of any type).
    /// Supported types are bool, int, float, string, float ramp, color ramp
    /// </summary>
    public class HEU_ParameterTupleWrapper
    {
        public bool[] BoolValues
        {
            get => _boolValues;
            set => _boolValues = value;
        }

        public int[] IntValues
        {
            get => _intValues;
            set => _intValues = value;
        }

        public float[] FloatValues
        {
            get => _floatValues;
            set => _floatValues = value;
        }

        public string[] StringValues
        {
            get => _stringValues;
            set => _stringValues = value;
        }

        public HEU_FloatRampPointWrapper[] FloatRampValues
        {
            get => _floatRampValues;
            set => _floatRampValues = value;
        }

        public HEU_ColorRampPointWrapper[] ColorRampValues
        {
            get => _colorRampValues;
            set => _colorRampValues = value;
        }

        internal bool[] _boolValues;
        internal int[] _intValues;
        internal float[] _floatValues;
        internal string[] _stringValues;
        internal HEU_FloatRampPointWrapper[] _floatRampValues;
        internal HEU_ColorRampPointWrapper[] _colorRampValues;

        public HEU_ParameterTupleWrapper()
        {
        }

        public HEU_ParameterTupleWrapper(bool[] boolValues)
        {
            this.BoolValues = boolValues;
        }

        public HEU_ParameterTupleWrapper(int[] intValues)
        {
            this.IntValues = intValues;
        }

        public HEU_ParameterTupleWrapper(float[] floatValues)
        {
            this.FloatValues = floatValues;
        }

        public HEU_ParameterTupleWrapper(string[] stringValues)
        {
            this.StringValues = stringValues;
        }

        public HEU_ParameterTupleWrapper(HEU_FloatRampPointWrapper[] floatRampValues)
        {
            this.FloatRampValues = floatRampValues;
        }

        public HEU_ParameterTupleWrapper(HEU_ColorRampPointWrapper[] colorRampValues)
        {
            this.ColorRampValues = colorRampValues;
        }
    }

    /// <summary>
    /// Holds all parameter data for an asset.
    /// </summary>
    public interface IHEU_Parameters
    {
        /// <summary>Whether or not to show parameters in the inspector</summary>
        bool ShowParameters { get; set; }

        /// <summary>The node ID of the HDA</summary>
        HAPI_NodeId NodeID { get; }

        /// <summary>The list of root parameter ids</summary>
        List<int> RootParameters { get; }

        /// <summary>The list of parameter modifiers</summary>
        List<HEU_ParameterModifier> ParameterModifiers { get; }

        /// <summary>Whether or not parameters are valid</summary>
        bool AreParametersValid();

        /// <summary>Get the list of parameters</summary>
        List<HEU_ParameterData> GetParameters();

        /// <summary>Gets the parameter at index</summary>
        HEU_ParameterData GetParameter(int listIndex);

        /// <summary>Gets the parameter of the name</summary>
        HEU_ParameterData GetParameter(string name);

        /// <summary>Gets the parameter with the parameter ID</summary>
        HEU_ParameterData GetParameterWithParmID(HAPI_ParmId parmID);

        /// <summary>Remove parameter at the index</summary>
        void RemoveParameter(int listIndex);

        /// <summary>
        /// Returns true if the parameter values have changed.
        /// Checks locally stored vs. values in the arrays from Houdini.
        /// </summary>
        /// <returns>True if parameter values have changed.</returns>
        bool HaveParametersChanged();

        /// <summary>
        /// Resets all parameters to their default values.
        /// </summary>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>True if it successfully reset.</returns>
        bool ResetAllToDefault(bool bRecookAsset = false);

        // Parameter getters and setters ====

        /// <summary>
        /// Sets a HDA float parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to set. </param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetFloatParameterValue(string parameterName, float value, int atIndex = 0, bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA float parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <param name="atIndex">The index of the parameter tuple to get. </param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetFloatParameterValue(string parameterName, out float value, int atIndex = 0);

        /// <summary>
        /// Sets HDA floats parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="values">The values of the parameter</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetFloatParameterValues(string parameterName, float[] values, bool bRecookAsset = false);

        /// <summary>
        /// Gets HDA float parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="values">The values of the parameter, if fetched</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetFloatParameterValues(string parameterName, out float[] values);


        /// <summary>
        /// Sets a HDA color parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to set.</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetColorParameterValue(string parameterName, Color value, bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA color parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetColorParameterValue(string parameterName, out Color value);

        /// <summary>
        /// Sets a HDA int parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to set. </param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetIntParameterValue(string parameterName, int value, int atIndex = 0, bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA int parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <param name="atIndex">The index of the parameter tuple to get. </param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetIntParameterValue(string parameterName, out int value, int atIndex = 0);

        /// <summary>
        /// Sets HDA int parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="values">The values of the parameter</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetIntParameterValues(string parameterName, int[] values, bool bRecookAsset = false);

        /// <summary>
        /// Gets HDA int parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="values">The values of the parameter, if fetched</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetIntParameterValues(string parameterName, out int[] values);

        /// <summary>
        /// Sets HDA choice parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetChoiceParameterValue(string parameterName, int value, bool bRecookAsset = false);


        /// <summary>
        /// Gets a HDA choice parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetChoiceParameterValue(string parameterName, out int value);

        /// <summary>
        /// Sets a HDA bool parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to set.</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetBoolParameterValue(string parameterName, bool value, int atIndex = 0, bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA bool parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <param name="atIndex">The index of the parameter tuple to set.</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetBoolParameterValue(string parameterName, out bool value, int atIndex = 0);

        /// <summary>
        /// Set a HDA string parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to set.</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetStringParameterValue(string parameterName, string value, int atIndex = 0, bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA string parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <param name="atIndex">The index of the parameter tuple to get. </param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetStringParameterValue(string parameterName, out string value, int atIndex = 0);

        /// <summary>
        /// Sets HDA string parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="values">The values of the parameter</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetStringParameterValues(string parameterName, string[] values, bool bRecookAsset = false);

        /// <summary>
        /// Gets HDA string parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="values">The values of the parameter, if fetched</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetStringParameterValues(string parameterName, out string[] values);

        /// <summary>
        /// Sets a HDA asset ref parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="atIndex">The index of the parameter tuple to get. </param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetAssetRefParameterValue(string parameterName, GameObject value, int atIndex = 0,
            bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA asset ref parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">The value of the parameter, if fetched</param>
        /// <param name="atIndex">The index of the parameter tuple to get. </param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetAssetRefParameterValue(string parameterName, out GameObject value, int atIndex = 0);

        /// <summary>
        /// Sets HDA asset ref parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="values">The values of the parameter</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetAssetRefParameterValues(string parameterName, GameObject[] values, bool bRecookAsset = false);

        /// <summary>
        /// Gets HDA asset ref parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="values">The values of the parameter, if fetched</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetAssetRefParameterValues(string parameterName, out GameObject[] values);

        /// <summary>
        /// Sets number of ramp parameter points
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="numPoints">The number of points to set</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetRampParameterNumPoints(string parameterName, int numPoints, bool bRecookAsset = false);

        /// <summary>
        /// Gets number of ramp points in the HDA ramp parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter tuple</param>
        /// <param name="numPOints">Number of ramp points</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetRampParameterNumPoints(string parameterName, out int numPoints);

        /// <summary>
        /// Sets a HDA float ramp parammeter value
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="pointIndex">The specified point</param>
        /// <param name="pointPosition">The position of the point</param>
        /// <param name="pointValue">The value of the point</param>
        /// <param name="interpolationType">The interpolation type of the point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetFloatRampParameterPointValue(
            string parameterName,
            int pointIndex,
            float pointPosition,
            float pointValue,
            HEU_HoudiniRampInterpolationTypeWrapper interpolationType = HEU_HoudiniRampInterpolationTypeWrapper.LINEAR,
            bool bRecookAsset = false);

        /// <summary>
        /// Gets a HDA float ramp parammeter value
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="pointIndex">The specified point</param>
        /// <param name="pointPosition">The position of the point</param>
        /// <param name="pointValue">The value of the point</param>
        /// <param name="interpolationType">The interpolation type of the point</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetFloatRampParameterPointValue(
            string parameterName,
            int pointIndex,
            out float pointPosition,
            out float pointValue,
            out HEU_HoudiniRampInterpolationTypeWrapper interpolationType
        );

        /// <summary>
        /// Sets HDA float ramp parammeter values
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="rampPoints">The points to set the parameter to</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetFloatRampParameterPoints(
            string parameterName,
            HEU_FloatRampPointWrapper[] rampPoints,
            bool bRecookAsset = false
        );

        /// <summary>
        /// Gets HDA float ramp parammeter values
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="rampPoints">The parameter ramp points</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetFloatRampParameterPoints(
            string parameterName,
            out HEU_FloatRampPointWrapper[] rampPoints
        );

        /// <summary>
        /// Sets a HDA color ramp parammeter value
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="pointIndex">The specified point</param>
        /// <param name="pointPosition">The position of the point</param>
        /// <param name="pointValue">The value of the point</param>
        /// <param name="interpolationType">The interpolation type of the point</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetColorRampParameterPointValue(
            string parameterName,
            int pointIndex,
            float pointPosition,
            Color pointValue,
            HEU_HoudiniRampInterpolationTypeWrapper interpolation = HEU_HoudiniRampInterpolationTypeWrapper.LINEAR,
            bool bRecookAsset = false
        );

        /// <summary>
        /// Gets a HDA color ramp parammeter value
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="pointIndex">The specified point</param>
        /// <param name="pointPosition">The position of the point</param>
        /// <param name="pointValue">The value of the point</param>
        /// <param name="interpolationType">The interpolation type of the point</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetColorRampParameterPointValue(
            string parameterName,
            int pointIndex,
            out float pointPosition,
            out Color pointValue,
            out HEU_HoudiniRampInterpolationTypeWrapper interpolationType
        );

        /// <summary>
        /// Sets HDA color ramp parammeter values
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="rampPoints">The specified points to set</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetColorRampParameterPoints(
            string parameterName,
            HEU_ColorRampPointWrapper[] rampPoints,
            bool bRecookAsset = false
        );

        /// <summary>
        /// Gets HDA color ramp parammeter values
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="rampPoints">The specified points to get</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetColorRampParameterPoints(
            string parameterName,
            out HEU_ColorRampPointWrapper[] rampPoints
        );

        /// <summary>
        /// Trigger the button with te parameter name
        /// </summary>
        /// <param name="parameterName">The name of the parameter to trigger</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool TriggerButtonParameter(
            string parameterName
        );


        /// <summary>
        /// Sets parameter tuples using the HEU_ParameterTupleWrapper class
        /// </summary>
        /// <param name="parameterTuples">A dictionary of parameter tuples to set it</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool SetParameterTuples(
            Dictionary<string, HEU_ParameterTupleWrapper> parameterTuples,
            bool bRecookAsset = false
        );

        /// <summary>
        /// Gets parameter tuples using the HEU_ParameterTupleWrapper class
        /// </summary>
        /// <param name="parameterTuples">A dictionary of parameter tuples</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetParameterTuples(
            out Dictionary<string, HEU_ParameterTupleWrapper> parameterTuples
        );
    }
} // HoudiniEngineUnity
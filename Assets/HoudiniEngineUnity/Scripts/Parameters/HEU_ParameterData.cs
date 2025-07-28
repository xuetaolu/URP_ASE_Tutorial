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
    using HAPI_StringHandle = System.Int32;
    using HAPI_ParmId = System.Int32;

    /// <summary>
    /// Represents a parameter (HAPI_ParmInfo), with data storage for the modifiable values.
    /// Note that this is not derived from ScriptableObject due to limitation in Unity's
    /// serialization system where we are unable to access properties in a ScriptablObject
    /// from the UI (see HEU_ParametersUI::DrawParmProperty).
    /// Additionally, due to Unity's serialization not serializing child classes
    /// of parent classes that themselves are not derived from ScriptableObject,
    /// we are using this class to store all data types (int, float, string, etc).
    /// Also note that storing these in an array will create inline serialization
    /// as opposed to serializing just the references (i.e. duplicates will be 
    /// made if there 2 references to same object in a list).
    /// So don't store more than 1 references to these. Currently these only live in HEU_Parameters.
    /// If you change the name of the members here, also make sure to update the property serialization
    /// queries in HEU_ParameterUI, as they are string-based.
    /// </summary>
    [System.Serializable]
    public sealed class HEU_ParameterData : IEquivable<HEU_ParameterData>
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Stored data

        // Index value on Unity side on the asset parameter list
        public int _unityIndex;

        // Cacheing these as they require a look up via HAPI_ParmInfo
        public string _name;
        public string _labelName;
        public string _help;

        // List of childrens' indices. Used for fast look up when drawing UI.
        public List<int> _childParameterIDs = new List<int>();

        public int _choiceValue;

        // Store as array values to handle any sized parameters
        public int[] _intValues;
        public float[] _floatValues;
        public string[] _stringValues;

        // Storing toggle as bool so that the UI can display it as a toggle.
        // Tried using _intValues above, but Inspector would display as an int.
        public bool _toggle;

        public Color _color;
        public Gradient _gradient;
        public AnimationCurve _animCurve;

        // Choices
        public GUIContent[] _choiceLabels;
        public string[] _choiceStringValues;
        public int[] _choiceIntValues;

        // Cacheing the HAPI_ParmInfo allows us to query meta data that only changes on Houdini side.
        public HAPI_ParmInfo _parmInfo;

        // Editor UI specific
        public bool _showChildren = false;

        // Cache
        public string _fileTypeInfo;

        // Folder list children processed
        public int _folderListChildrenProcessed;

        // Folder list tab selected index
        public int _tabSelectedIndex;

        // Input node info
        public HEU_InputNode _paramInputNode;

        // Flags whether this should be treated as an asset path
        public bool _hasAssetPathTag;


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Properties & Functions

        public HAPI_ParmId ParmID
        {
            get { return _parmInfo.id; }
        }

        public HAPI_ParmId ParentID
        {
            get { return _parmInfo.parentId; }
        }

        public int ChildIndex
        {
            get { return _parmInfo.childIndex; }
        }

        public int ParmSize
        {
            get { return _parmInfo.size; }
        }

        public bool IsInt()
        {
            return _parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_INT;
        }

        public bool IsFloat()
        {
            return _parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT;
        }

        public bool IsString()
        {
            return _parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_STRING;
        }

        public bool IsPathFile()
        {
            return (_parmInfo.type >= HAPI_ParmType.HAPI_PARMTYPE_PATH_START
                    && _parmInfo.type <= HAPI_ParmType.HAPI_PARMTYPE_PATH_END);
        }

        public bool HasMin()
        {
            return _parmInfo.hasMin;
        }

        public bool HasMax()
        {
            return _parmInfo.hasMax;
        }

        public bool HasUIMin()
        {
            return _parmInfo.hasUIMin;
        }

        public bool HasUIMax()
        {
            return _parmInfo.hasUIMax;
        }

        public int IntMin
        {
            get { return Mathf.RoundToInt(_parmInfo.min); }
        }

        public int IntMax
        {
            get { return Mathf.RoundToInt(_parmInfo.max); }
        }

        public int IntUIMin
        {
            get { return Mathf.RoundToInt(_parmInfo.UIMin); }
        }

        public int IntUIMax
        {
            get { return Mathf.RoundToInt(_parmInfo.UIMax); }
        }

        public float FloatMin
        {
            get { return _parmInfo.min; }
        }

        public float FloatMax
        {
            get { return _parmInfo.max; }
        }

        public float FloatUIMin
        {
            get { return _parmInfo.UIMin; }
        }

        public float FloatUIMax
        {
            get { return _parmInfo.UIMax; }
        }

        public bool IsContainer()
        {
            return (_childParameterIDs != null) ? (_childParameterIDs.Count > 0) : false;
        }

        public bool IsMultiParam()
        {
            return _parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST;
        }

        public bool IsRamp()
        {
            return (_parmInfo.rampType > HAPI_RampType.HAPI_RAMPTYPE_INVALID
                    && _parmInfo.rampType < HAPI_RampType.HAPI_RAMPTYPE_MAX);
        }

        public bool IsFloatRamp()
        {
            return _parmInfo.rampType == HAPI_RampType.HAPI_RAMPTYPE_FLOAT;
        }

        public bool IsColorRamp()
        {
            return _parmInfo.rampType == HAPI_RampType.HAPI_RAMPTYPE_COLOR;
        }

        public bool IsToggle()
        {
            return (_parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE);
        }

        public bool IsColor()
        {
            return (_parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_COLOR);
        }

        public bool IsFolder()
        {
            return (_parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER);
        }

        public bool IsButton()
        {
            return (_parmInfo.type == HAPI_ParmType.HAPI_PARMTYPE_BUTTON);
        }

        public Vector3 ToVector3()
        {
            if (IsFloat() && _floatValues.Length >= 3)
            {
                return new Vector3(_floatValues[0], _floatValues[1], _floatValues[2]);
            }

            return Vector3.zero;
        }

        public bool IsAssetPath()
        {
            return _hasAssetPathTag;
        }

        public bool IsEquivalentTo(HEU_ParameterData other)
        {
            bool bResult = true;

            string header = "HEU_ParameterData";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._name, other._name, ref bResult, header, "_name");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._labelName, other._labelName, ref bResult, header, "_labelName");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._help, other._help, ref bResult, header, "_help");

            // Skip child parameter Ids values, but not count
            HEU_TestHelpers.AssertTrueLogEquivalent(this._childParameterIDs.Count, other._childParameterIDs.Count, ref bResult, header,
                "_childParameterIds");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._intValues, other._intValues, ref bResult, header, "_intValues");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._floatValues, other._floatValues, ref bResult, header, "_floatValues");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._stringValues, other._stringValues, ref bResult, header, "_stringValues");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._toggle, other._toggle, ref bResult, header, "_toggle");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._color, other._color, ref bResult, header, "_color");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._gradient.ToTestObject(), other._gradient.ToTestObject(), ref bResult, header, "_gradient");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._animCurve.ToTestObject(), other._animCurve.ToTestObject(), ref bResult, header,
                "_animCurve");

            // Skip choiceLabels 

            HEU_TestHelpers.AssertTrueLogEquivalent(this._choiceStringValues, other._choiceStringValues, ref bResult, header, "_choiceStringValues");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._choiceIntValues, other._choiceIntValues, ref bResult, header, "_choiceIntValues");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._parmInfo.ToTestObject(), other._parmInfo.ToTestObject(), ref bResult, header, "_parmInfo");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showChildren, other._showChildren, ref bResult, header, "_showChildren");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._fileTypeInfo, other._fileTypeInfo, ref bResult, header, "_fileTypeInfo");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._folderListChildrenProcessed, other._folderListChildrenProcessed, ref bResult, header,
                "_folderListChildrenProcessed");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._tabSelectedIndex, other._tabSelectedIndex, ref bResult, header, "_tabSelectedIndex");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._paramInputNode, other._paramInputNode, ref bResult, header, "_paramInputNode");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._hasAssetPathTag, other._hasAssetPathTag, ref bResult, header, "_hasAssetPathTag");

            return bResult;
        }
    }
} // HoudiniEngineUnity
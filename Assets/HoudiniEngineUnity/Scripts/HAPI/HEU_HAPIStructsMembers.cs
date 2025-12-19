/*
 * Copyright (c) <2021> Side Effects Software Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * Produced by:
 *      Side Effects Software Inc
 *      123 Front Street West, Suite 1401
 *      Toronto, Ontario
 *      Canada   M5J 2M2
 *      416-504-9876
 */


// Manual extensions to auto generated HEU_HAPIStructs.cs

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HoudiniEngineUnity
{
    using HAPI_Bool = System.Boolean;
    using HAPI_UInt8 = System.Byte;
    using HAPI_Int8 = System.SByte;
    using HAPI_Int16 = System.Int16;
    using HAPI_Int64 = System.Int64;
    using HAPI_ProcessId = System.Int32;
    using HAPI_SessionId = System.Int64;
    using HAPI_StringHandle = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_NodeId = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_PartId = System.Int32;
    using HAPI_PDG_WorkitemId = System.Int32;
    using HAPI_PDG_GraphContextId = System.Int32;
    using HAPI_HIPFileId = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;

    public partial struct HAPI_Transform
    {
        public HAPI_Transform(bool initializeFields)
        {
            position = new float[HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE];
            rotationQuaternion = new float[HEU_HAPIConstants.HAPI_QUATERNION_VECTOR_SIZE];
            scale = new float[HEU_HAPIConstants.HAPI_SCALE_VECTOR_SIZE];
            shear = new float[HEU_HAPIConstants.HAPI_SHEAR_VECTOR_SIZE];

            rstOrder = HAPI_RSTOrder.HAPI_SRT;

            if (initializeFields)
                Init();
        }

        public void Init()
        {
            for (int n = 0; n < HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE; n++)
                position[n] = 0.0f;

            for (int n = 0; n < HEU_HAPIConstants.HAPI_QUATERNION_VECTOR_SIZE; n++)
            {
                if (n == 3)
                    rotationQuaternion[n] = 1.0f;
                else
                    rotationQuaternion[n] = 0.0f;
            }

            for (int n = 0; n < HEU_HAPIConstants.HAPI_SCALE_VECTOR_SIZE; n++)
                scale[n] = 1.0f;

            for (int n = 0; n < HEU_HAPIConstants.HAPI_SHEAR_VECTOR_SIZE; n++)
                shear[n] = 0.0f;
        }
    };


    public partial struct HAPI_TransformEuler
    {
        public HAPI_TransformEuler(bool initializeFields)
        {
            position = new float[HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE];
            rotationEuler = new float[HEU_HAPIConstants.HAPI_EULER_VECTOR_SIZE];
            scale = new float[HEU_HAPIConstants.HAPI_SCALE_VECTOR_SIZE];
            shear = new float[HEU_HAPIConstants.HAPI_SHEAR_VECTOR_SIZE];

            rotationOrder = 0;
            rstOrder = 0;

            if (initializeFields)
                Init();
        }

        public void Init()
        {
            for (int n = 0; n < HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE; n++)
                position[n] = 0.0f;

            for (int n = 0; n < HEU_HAPIConstants.HAPI_EULER_VECTOR_SIZE; n++)
            {
                rotationEuler[n] = 0.0f;
            }

            for (int n = 0; n < HEU_HAPIConstants.HAPI_SCALE_VECTOR_SIZE; n++)
                scale[n] = 1.0f;

            for (int n = 0; n < HEU_HAPIConstants.HAPI_SHEAR_VECTOR_SIZE; n++)
                shear[n] = 0.0f;
        }
    };

    public partial struct HAPI_ParmInfo
    {
        public bool isInt()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_INT_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_INT_END)
                   || type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST
                   || type == HAPI_ParmType.HAPI_PARMTYPE_FOLDERLIST_RADIO;
        }

        public bool isFloat()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_FLOAT_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_FLOAT_END);
        }

        public bool isString()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_STRING_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_STRING_END)
                   || type == HAPI_ParmType.HAPI_PARMTYPE_LABEL
                   || type == HAPI_ParmType.HAPI_PARMTYPE_PATH_FILE_DIR;
        }

        public bool isPath()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_PATH_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_PATH_END)
                   || type == HAPI_ParmType.HAPI_PARMTYPE_PATH_FILE_DIR;
        }

        public bool isNode()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_NODE_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_NODE_END);
        }

        public bool isNonValue()
        {
            return (type >= HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_START &&
                    type <= HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_END);
        }
    };

    public partial struct HAPI_GeoInfo
    {
        public int getGroupCountByType(HAPI_GroupType type)
        {
            switch (type)
            {
                case HAPI_GroupType.HAPI_GROUPTYPE_POINT: return pointGroupCount;
                case HAPI_GroupType.HAPI_GROUPTYPE_PRIM: return primitiveGroupCount;
                default: return 0;
            }
        }
    };

    public partial struct HAPI_PartInfo
    {
        public int getElementCountByAttributeOwner(HAPI_AttributeOwner owner)
        {
            switch (owner)
            {
                case HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX: return vertexCount;
                case HAPI_AttributeOwner.HAPI_ATTROWNER_POINT: return pointCount;
                case HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM: return faceCount;
                case HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL: return 1;
                default: return 0;
            }
        }

        public int getElementCountByGroupType(HAPI_GroupType type)
        {
            switch (type)
            {
                case HAPI_GroupType.HAPI_GROUPTYPE_POINT: return pointCount;
                case HAPI_GroupType.HAPI_GROUPTYPE_PRIM: return faceCount;
                default: return 0;
            }
        }

        public void init()
        {
            if (attributeCounts == null) attributeCounts = new int[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_MAX];
        }

        public int pointAttributeCount
        {
            get
            {
                init();
                return attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_POINT];
            }
            set
            {
                init();
                attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_POINT] = value;
            }
        }

        public int primitiveAttributeCount
        {
            get
            {
                init();
                return attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM];
            }
            set
            {
                init();
                attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM] = value;
            }
        }

        public int vertexAttributeCount
        {
            get
            {
                init();
                return attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX];
            }
            set
            {
                init();
                attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX] = value;
            }
        }

        public int detailAttributeCount
        {
            get
            {
                init();
                return attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL];
            }
            set
            {
                init();
                attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL] = value;
            }
        }
    };

    public partial struct HAPI_AttributeInfo
    {
        public HAPI_AttributeInfo(string ignored = null)
        {
            exists = false;
            owner = HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
            storage = HAPI_StorageType.HAPI_STORAGETYPE_INVALID;
            originalOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
            count = 0;
            tupleSize = 0;
            totalArrayElements = 0;
            typeInfo = HAPI_AttributeTypeInfo.HAPI_ATTRIBUTE_TYPE_INVALID;
        }
    };

    public partial struct HAPI_Keyframe
    {
        public HAPI_Keyframe(float t, float v, float in_tangent, float out_tangent)
        {
            time = t;
            value = v;
            inTangent = in_tangent;
            outTangent = out_tangent;
        }
    };

    public partial struct HAPI_BoxInfo
    {
        public HAPI_BoxInfo(bool initialize_fields)
        {
            center = new float[HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE];
            size = new float[HEU_HAPIConstants.HAPI_SCALE_VECTOR_SIZE];
            rotation = new float[HEU_HAPIConstants.HAPI_EULER_VECTOR_SIZE];
        }
    };

    public partial struct HAPI_SphereInfo
    {
        public HAPI_SphereInfo(bool initialize_fields)
        {
            center = new float[HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE];
            radius = 0.0f;
        }
    };

    public partial struct HAPI_Viewport
    {
        public HAPI_Viewport(bool initializeFields)
        {
            position = new float[HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE];
            rotationQuaternion = new float[HEU_HAPIConstants.HAPI_QUATERNION_VECTOR_SIZE];

            offset = 0;

            if (initializeFields)
                Init();
        }

        public void Init()
        {
            for (int n = 0; n < HEU_HAPIConstants.HAPI_POSITION_VECTOR_SIZE; n++)
                position[n] = 0.0f;

            for (int n = 0; n < HEU_HAPIConstants.HAPI_QUATERNION_VECTOR_SIZE; n++)
            {
                if (n == 3)
                    rotationQuaternion[n] = .0f;
                else
                    rotationQuaternion[n] = 0.0f;
            }

            offset = 0;
        }
    }

    public partial struct HAPI_InputCurveInfo
    {
        public void FillData(HEU_InputCurveInfo curveInfo)
        {
            if (curveInfo == null)
            {
                return;
            }

            closed = curveInfo.closed;
            curveType = curveInfo.curveType;
            order = HEU_Curve.GetOrderForCurveType(curveInfo.order, curveInfo.curveType);
            reverse = curveInfo.reverse;
            inputMethod = curveInfo.inputMethod;
            breakpointParameterization = curveInfo.breakpointParameterization;
        }
    }
}
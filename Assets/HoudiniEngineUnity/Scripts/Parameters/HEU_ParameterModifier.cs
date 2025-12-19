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
    // Modifier action
    public enum HEU_ModifierActionWrapper
    {
        MULTIPARM_INSERT, // Insert _modifierValue number of params from _instanceIndex onwards
        MULTIPARM_REMOVE, // Remove _modifierValue number of params from _instanceIndex onwards
        MULTIPARM_CLEAR, // Clear all instances
        SET_FLOAT, // Set float value for parameter
        SET_INT // Set int value for parameter
    }

    /// <summary>
    /// Helper that contains a request for parameter modification.
    /// Currently used to modifier multiparms after the UI has drawn.
    /// </summary>
    [System.Serializable]
    public class HEU_ParameterModifier : IEquivable<HEU_ParameterModifier>
    {
        [SerializeField] private int _parameterIndex;

        public int ParameterIndex
        {
            get => _parameterIndex;
            set => _parameterIndex = value;
        }

        // Modifier action
        internal enum ModifierAction
        {
            MULTIPARM_INSERT, // Insert _modifierValue number of params from _instanceIndex onwards
            MULTIPARM_REMOVE, // Remove _modifierValue number of params from _instanceIndex onwards
            MULTIPARM_CLEAR, // Clear all instances
            SET_FLOAT, // Set float value for parameter
            SET_INT // Set int value for parameter
        }

        [SerializeField] internal ModifierAction _action;

        public HEU_ModifierActionWrapper Action
        {
            get => ModifierAction_InternalToWrapper(_action);
            set => _action = ModifierAction_WrapperToInternal(value);
        }

        // Instance index of the parameter instance (for multiparm)
        [SerializeField] private int _instanceIndex;

        public int InstanceIndex
        {
            get => _instanceIndex;
            set => _instanceIndex = value;
        }


        // General value for the action (eg. number of new instances for INSERT, number of instances to REMOVE)
        [SerializeField] private int _modifierValue;

        public int ModifierValue
        {
            get => _modifierValue;
            set => _modifierValue = value;
        }

        [SerializeField] private float _floatValue;

        public float FloatValue
        {
            get => _floatValue;
            set => _floatValue = value;
        }

        [SerializeField] private int _intValue;

        public int IntValue
        {
            get => _intValue;
            set => _intValue = value;
        }

        public static HEU_ParameterModifier GetNewModifier(HEU_ModifierActionWrapper action, int parameterIndex,
            int instanceIndex, int modifierValue)
        {
            return GetNewModifier(ModifierAction_WrapperToInternal(action), parameterIndex, instanceIndex,
                modifierValue);
        }

        internal static HEU_ParameterModifier GetNewModifier(ModifierAction action, int parameterIndex,
            int instanceIndex, int modifierValue)
        {
            HEU_ParameterModifier newModifier = new HEU_ParameterModifier();
            newModifier._action = action;
            newModifier._parameterIndex = parameterIndex;
            newModifier._instanceIndex = instanceIndex;
            newModifier._modifierValue = modifierValue;

            return newModifier;
        }

        public bool IsEquivalentTo(HEU_ParameterModifier other)
        {
            bool bResult = true;

            string header = "HEU_ParameterModifier";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._parameterIndex, other._parameterIndex, ref bResult, header,
                "_parameterIndex");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._action, other._action, ref bResult, header, "_action");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._instanceIndex, other._instanceIndex, ref bResult, header,
                "_instanceIndex");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._modifierValue, other._modifierValue, ref bResult, header,
                "_modifierValue");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._floatValue, other._floatValue, ref bResult, header,
                "_floatValue");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._intValue, other._intValue, ref bResult, header, "_intValue");

            return bResult;
        }

        internal static HEU_ModifierActionWrapper ModifierAction_InternalToWrapper(ModifierAction action)
        {
            switch (action)
            {
                case ModifierAction.MULTIPARM_INSERT:
                    return HEU_ModifierActionWrapper.MULTIPARM_INSERT;
                case ModifierAction.MULTIPARM_REMOVE:
                    return HEU_ModifierActionWrapper.MULTIPARM_REMOVE;
                case ModifierAction.MULTIPARM_CLEAR:
                    return HEU_ModifierActionWrapper.MULTIPARM_CLEAR;
                case ModifierAction.SET_FLOAT:
                    return HEU_ModifierActionWrapper.SET_FLOAT;
                case ModifierAction.SET_INT:
                    return HEU_ModifierActionWrapper.SET_INT;
                default:
                    return HEU_ModifierActionWrapper.MULTIPARM_INSERT;
            }
        }

        internal static ModifierAction ModifierAction_WrapperToInternal(HEU_ModifierActionWrapper action)
        {
            switch (action)
            {
                case HEU_ModifierActionWrapper.MULTIPARM_INSERT:
                    return ModifierAction.MULTIPARM_INSERT;
                case HEU_ModifierActionWrapper.MULTIPARM_REMOVE:
                    return ModifierAction.MULTIPARM_REMOVE;
                case HEU_ModifierActionWrapper.MULTIPARM_CLEAR:
                    return ModifierAction.MULTIPARM_CLEAR;
                case HEU_ModifierActionWrapper.SET_FLOAT:
                    return ModifierAction.SET_FLOAT;
                case HEU_ModifierActionWrapper.SET_INT:
                    return ModifierAction.SET_INT;
                default:
                    return ModifierAction.MULTIPARM_INSERT;
            }
        }
    }
} // HoudiniEngineUnity
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

// Expose internal classes/functions
#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HoudiniEngineUnityEditor")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityEditorTests")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityPlayModeTests")]
#endif

namespace HoudiniEngineUnity
{
    /// <summary>
    /// Object to store instance input UI state so that we can check if UI changed
    /// and apply modifications for just this object instead of for the entire asset.
    /// Used by HEU_InstanceInputUI.
    /// </summary>
    [System.Serializable]
    internal class HEU_InstanceInputUIState : ScriptableObject, IEquivable<HEU_InstanceInputUIState>
    {
        // Whether to show all instance inputs to expanded form
        public bool _showInstanceInputs = true;

        // For pagination, the number of inputs to show per page
        public int _numInputsToShowUI = 5;

        // The current page to show
        public int _inputsPageIndexUI = 0;

        internal void CopyTo(HEU_InstanceInputUIState dest)
        {
            dest._showInstanceInputs = _showInstanceInputs;
            dest._numInputsToShowUI = _numInputsToShowUI;
            dest._inputsPageIndexUI = _inputsPageIndexUI;
        }

        public bool IsEquivalentTo(HEU_InstanceInputUIState other)
        {
            bool bResult = true;

            string header = "HEU_InstanceInputUIState";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._showInstanceInputs, other._showInstanceInputs, ref bResult, header, "_showInstanceInputs");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._numInputsToShowUI, other._numInputsToShowUI, ref bResult, header, "_numInputsToShowUI");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputsPageIndexUI, other._inputsPageIndexUI, ref bResult, header, "_inputPageIndexUI");
            return bResult;
        }
    }
} // HoudiniEngineUnity
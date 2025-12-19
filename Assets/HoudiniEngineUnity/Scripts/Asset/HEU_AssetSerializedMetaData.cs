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
using System.Collections.Generic;
using System.Linq;
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
    // Super hacky way to force save data when access is limited such as undo deletion events
    internal class HEU_AssetSerializedMetaData : ScriptableObject, IEquivable<HEU_AssetSerializedMetaData>
    {
        [SerializeField] private bool _softDeleted = false;

        public bool SoftDeleted
        {
            get => _softDeleted;
            set => _softDeleted = value;
        }

        // Map of (Curve name) -> List of curve node data for saving scale/rotation values between rebuilds.
        [SerializeField] private Dictionary<string, List<CurveNodeData>> _savedCurveNodeData =
            new Dictionary<string, List<CurveNodeData>>();

        public Dictionary<string, List<CurveNodeData>> SavedCurveNodeData => _savedCurveNodeData;

        [SerializeField] private Dictionary<string, HEU_InputCurveInfo> _savedInputCurveInfo =
            new Dictionary<string, HEU_InputCurveInfo>();

        public Dictionary<string, HEU_InputCurveInfo> SavedInputCurveInfo => _savedInputCurveInfo;

        public bool IsEquivalentTo(HEU_AssetSerializedMetaData other)
        {
            bool bResult = true;

            string header = "HEU_AssetSerializedMetaData";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            // These things shouldn't be tested because they're specifically "hacky"

            return bResult;
        }
    }
}
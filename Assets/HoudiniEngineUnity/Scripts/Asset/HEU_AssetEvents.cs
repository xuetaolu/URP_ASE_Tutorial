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
using System;
using System.Collections.Generic;
using UnityEngine.Events;

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
    /// Asset Event Classes since UnityEvent doesn't directly support generics. 
    /// </summary>
    public enum HEU_AssetEventType
    {
        UNKNOWN,
        RELOAD,
        COOK,
        BAKE_NEW,
        BAKE_UPDATE
    };


    public class HEU_AssetEventData
    {
        public HEU_HoudiniAsset Asset;
        public bool CookSuccess;
        public List<GameObject> OutputObjects;

        public HEU_AssetEventType EventType;

        public HEU_AssetEventData(HEU_HoudiniAsset asset, bool successful, List<GameObject> outputObjects)
        {
            this.Asset = asset;
            this.CookSuccess = successful;
            this.OutputObjects = outputObjects;
        }
    }

    // Class for holding reload event data
    public class HEU_ReloadEventData : HEU_AssetEventData
    {
        public HEU_ReloadEventData(HEU_HoudiniAsset asset, bool successful, List<GameObject> outputObjects) : base(
            asset, successful, outputObjects)
        {
            this.EventType = HEU_AssetEventType.RELOAD;
        }
    }

    // Class for holding cook event data
    public class HEU_CookedEventData : HEU_AssetEventData
    {
        public HEU_CookedEventData(HEU_HoudiniAsset asset, bool successful, List<GameObject> outputObjects) : base(
            asset, successful, outputObjects)
        {
            this.EventType = HEU_AssetEventType.COOK;
        }
    }

    // Class for holding bake event data
    public class HEU_BakedEventData : HEU_AssetEventData
    {
        public bool IsNewBake = false;

        public HEU_BakedEventData(HEU_HoudiniAsset asset, bool successful, List<GameObject> outputObjects,
            bool isNewBake) : base(asset, successful, outputObjects)
        {
            this.IsNewBake = isNewBake;
            this.EventType = isNewBake ? HEU_AssetEventType.BAKE_NEW : HEU_AssetEventType.BAKE_UPDATE;
        }
    }


    // Data regarding the PreAssetEvent
    public class HEU_PreAssetEventData
    {
        // The asset that triggered the event
        public HEU_HoudiniAsset Asset;

        // The type of the event (Cook/Bake/Rebuild)
        public HEU_AssetEventType AssetType;

        public HEU_PreAssetEventData(HEU_HoudiniAsset asset, HEU_AssetEventType assetType)
        {
            this.Asset = asset;
            this.AssetType = assetType;
        }
    }

    /// <summary>
    /// Callback when asset is reloaded.
    /// <param name="ReloadEventData">The reload data.</param>
    /// </summary>
    [Serializable]
    public class HEU_ReloadDataEvent : UnityEvent<HEU_ReloadEventData>
    {
    }

    /// <summary>
    /// Callback when asset is cooked.
    /// <param name="CookedEventData">The reload data.</param>
    /// </summary>
    [Serializable]
    public class HEU_CookedDataEvent : UnityEvent<HEU_CookedEventData>
    {
    }

    /// <summary>
    /// Callback when asset is baked.
    /// <param name="CookedEventData">The reload data.</param>
    /// </summary>
    [Serializable]
    public class HEU_BakedDataEvent : UnityEvent<HEU_BakedEventData>
    {
    }

    /// <summary>
    /// Callback before a Reload/Cook/Bake event
    /// <param name="HEU_PreAssetEventData">The asset data.</param>
    /// </summary>
    [Serializable]
    public class HEU_PreAssetEvent : UnityEvent<HEU_PreAssetEventData>
    {
    }
} // HoudiniEngineUnity
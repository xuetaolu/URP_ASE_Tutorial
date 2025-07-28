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
    // Derived class to support dictionary serialization
    [System.Serializable]
    public class HEU_OutputAttributeDictionary : HEU_SerializableDictionary<string, HEU_OutputAttribute>
    {
    }

    /// <summary>
    /// Contains Houdini attributes data (HEU_OutpuAttribute) for generated gameobjects.
    /// Query the attributes by name.
    /// </summary>
    public class HEU_OutputAttributesStore : MonoBehaviour
    {
        [SerializeField] private HEU_OutputAttributeDictionary _attributes = new HEU_OutputAttributeDictionary();

        /// <summary>
        /// Add the given attribute to the internal map by name.
        /// </summary>
        /// <param name="attribute">Attribute data to store</param>
        public void SetAttribute(HEU_OutputAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute._name))
            {
                HEU_Logger.LogWarningFormat("Unable to store attribute with empty name!", attribute._name);
                return;
            }

            _attributes.Add(attribute._name, attribute);
        }

        /// <summary>
        /// Returns the attribute specified by name, or null if not found.
        /// </summary>
        /// <param name="name">Name of attribute</param>
        public HEU_OutputAttribute GetAttribute(string name)
        {
            HEU_OutputAttribute attr = null;
            _attributes.TryGetValue(name, out attr);
            return attr;
        }

        /// <summary>
        /// Clear the store so nothing exists.
        /// </summary>
        public void Clear()
        {
            _attributes.Clear();
        }
    }
}
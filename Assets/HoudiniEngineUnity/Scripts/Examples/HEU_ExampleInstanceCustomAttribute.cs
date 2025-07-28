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
using System.Text;

// Bring in Houdini Engine Unity API
using HoudiniEngineUnity;

[ExecuteInEditMode]
public class HEU_ExampleInstanceCustomAttribute : MonoBehaviour
{
    /// <summary>
    /// Example to show how to use the HEU_OutputAttributeStore component to query
    /// attribute data and set it on instances.
    /// This should be used with HEUInstanceAttributesStore.hda.
    /// This function is called after HDA is cooked.
    /// </summary>
    private void InstancerCallback()
    {
        // Acquire the attribute storage component (HEU_OutputAttributesStore).
        // HEU_OutputAttributesStore contains a dictionary of attribute names to attribute data (HEU_OutputAttribute).
        // HEU_OutputAttributesStore is added to the generated gameobject when an attribute with name 
        // "hengine_attr_store" is created at the detail level.
        HEU_OutputAttributesStore attrStore = gameObject.GetComponent<HEU_OutputAttributesStore>();
        if (attrStore == null)
        {
            HEU_Logger.LogWarning("No HEU_OutputAttributesStore component found!");
            return;
        }

        // Query for the health attribute (HEU_OutputAttribute).
        // HEU_OutputAttribute contains the attribute info such as name, class, storage, and array of data.
        // Use the name to get HEU_OutputAttribute.
        // Can use HEU_OutputAttribute._type to figure out what the actual data type is.
        // Note that data is stored in array. The size of the array corresponds to the data type.
        // For instances, the size of the array is the point cound.
        HEU_OutputAttribute healthAttr = attrStore.GetAttribute("health");
        if (healthAttr != null)
        {
            LogAttr(healthAttr);
        }

        // Query for the vector size attribute
        HEU_OutputAttribute sizeAttr = attrStore.GetAttribute("size");
        if (sizeAttr != null)
        {
            LogAttr(sizeAttr);
        }

        // Query for the stringdata attribute
        HEU_OutputAttribute stringAttr = attrStore.GetAttribute("stringdata");
        if (stringAttr != null)
        {
            LogAttr(stringAttr);
        }

        // Example of how to map the attribute array values to instances
        // Get the generated instances as children of this gameobject.
        // Note that this will include the current parent as first element (so its number of children + 1 size)
        Transform[] childTrans = transform.GetComponentsInChildren<Transform>();
        int numChildren = childTrans.Length;
        // Starting at 1 to skip parent transform
        for (int i = 1; i < numChildren; ++i)
        {
            HEU_Logger.LogFormat("Instance {0}: name = {1}", i, childTrans[i].name);

            // Can use the name to match up indices
            string instanceName = "Instance" + i;
            if (childTrans[i].name.EndsWith(instanceName))
            {
                // Now apply health as scale value
                Vector3 scale = childTrans[i].localScale;

                // Health index is -1 due to child indices off by 1 because of parent
                scale.y = healthAttr._intValues[i - 1];

                childTrans[i].localScale = scale;
            }
        }
    }

    private static void LogArray<T>(string name, T[] arr, int tupleSize)
    {
        int index = 0;
        int count = arr.Length / tupleSize;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < count; ++i)
        {
            sb.AppendFormat("{0}[{1}] = ", name, i);

            if (tupleSize > 1)
            {
                sb.Append("[");

                for (int j = 0; j < tupleSize; ++j)
                {
                    index = i * tupleSize + j;

                    if (j != 0)
                    {
                        sb.Append(",");
                    }

                    sb.AppendFormat("{0}", arr[index]);
                }

                sb.AppendLine("]");
            }
            else
            {
                sb.AppendFormat("{0}\n", arr[i]);
            }
        }

        HEU_Logger.Log(sb.ToString());
    }

    private static void LogAttr(HEU_OutputAttribute outAttr)
    {
        HEU_Logger.LogFormat("Found {0} attribute:", outAttr._name);

        if (outAttr._intValues != null)
        {
            LogArray(outAttr._name, outAttr._intValues, outAttr._tupleSize);
        }
        else if (outAttr._floatValues != null)
        {
            LogArray(outAttr._name, outAttr._floatValues, outAttr._tupleSize);
        }
        else if (outAttr._stringValues != null)
        {
            LogArray(outAttr._name, outAttr._stringValues, outAttr._tupleSize);
        }
    }
}
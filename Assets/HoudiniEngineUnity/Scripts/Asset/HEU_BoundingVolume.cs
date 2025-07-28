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

using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Helper class for using the bounding volume input
/// Attach this to an object and then use it as an input to select all objects within
/// </summary>
public class HEU_BoundingVolume : MonoBehaviour
{
    /// <summary>
    /// The bounding collider
    /// </summary>
    public Collider BoundingCollider
    {
        get { return GetComponent<Collider>(); }
    }


    /// <summary>
    /// Gets all intersecting objects in the bounding collider
    /// <returns>A list of all intersecting objects</returns>
    /// </summary>
    public List<GameObject> GetAllIntersectingObjects()
    {
        if (BoundingCollider == null)
        {
            return null;
        }

        List<GameObject> intersectingObjects = new List<GameObject>();
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj == this.gameObject)
            {
                continue;
            }

            if (!obj.GetComponent<Collider>())
            {
                continue;
            }

            if (BoundingCollider.bounds.Intersects(obj.GetComponent<Collider>().bounds))
            {
                intersectingObjects.Add(obj);
            }
        }

        return intersectingObjects;
    }
}
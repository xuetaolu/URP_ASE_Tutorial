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
using System.Text;
using UnityEngine;

namespace HoudiniEngineUnity
{
    public static class HEU_Extensions
    {
        // List extensions
        public static List<R> Map<T, R>(this IEnumerable<T> self, Func<T, R> selector)
        {
            return self.Select(selector).ToList();
        }

        public static T Reduce<T>(this IEnumerable<T> self, Func<T, T, T> func)
        {
            return self.Aggregate(func);
        }

        public static List<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            return self.Where(predicate).ToList();
        }

        public static bool IsValidIndex<T>(this List<T> self, int index)
        {
            return index >= 0 && index < self.Count;
        }

        public static bool IsEquivalentList<T>(this List<T> self, List<T> other)
        {
            if (self.Count != other.Count)
            {
                return false;
            }

            for (int i = 0; i < self.Count; i++)
            {
                if (!self[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Transforms
        public static bool ApproximatelyEquals(this Quaternion quatA, Quaternion value)
        {
            float difference = 1 - Mathf.Abs(Quaternion.Dot(quatA, value));
            return difference < Quaternion.kEpsilon;
        }

        public static List<U> ConvertList<T, U>(this List<T> self)
        {
            return self.Cast<U>().ToList();
        }

        // For testing uses only
        public static List<IEquivable<T>> ConvertListToEquivable<T>(this List<T> self)
        {
            return self.Cast<IEquivable<T>>().ToList();
        }

        public static IEquivable<T>[] ConvertArrayToEquivable<T>(this T[] self)
        {
            return self.Cast<IEquivable<T>>().ToArray();
        }

        public static bool ApproximatelyEquals(this float self, float other, float epsilon = 1E-03F)
        {
            return Mathf.Abs(self - other) < epsilon;
        }

        public static byte[] AsByteArray(this string self)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(self + '\0');
            return bytes;
        }

        public static string AsString(this byte[] buffer)
        {
            if (buffer == null)
            {
                return "";
            }

            return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length - 1);
        }

        // Vector3
        public static Vector3 SwapXAndY(this Vector3 self)
        {
            float tmp = self.x;
            self.x = self.y;
            self.y = tmp;

            return self;
        }

        public static Vector3 SwapXAndZ(this Vector3 self)
        {
            float tmp = self.x;
            self.x = self.z;
            self.z = tmp;

            return self;
        }

        public static Vector3 SwapYAndZ(this Vector3 self)
        {
            float tmp = self.y;
            self.y = self.z;
            self.z = tmp;

            return self;
        }

        // Matrix4x4
        public static Vector3 DecomposeToPosition(this Matrix4x4 self)
        {
            return self.GetColumn(3);
        }

        public static Quaternion DecomposeToRotation(this Matrix4x4 self)
        {
            return Quaternion.LookRotation(self.GetColumn(2), self.GetColumn(1));
        }

        public static Vector3 DecomposeToScale(this Matrix4x4 self)
        {
            return new Vector3(self.GetColumn(0).magnitude, self.GetColumn(1).magnitude, self.GetColumn(2).magnitude);
        }
    }


    public static class ArrayExtensions
    {
        /// <summary>
        /// Set the given array with the given value for every element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="defaultValue"></param>
        public static void Init<T>(this T[] array, T defaultValue)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = defaultValue;
                }
            }
        }

        /// <summary>
        /// Set the given list with the given value for every element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="defaultValue"></param>
        public static void Init<T>(this List<T> array, T defaultValue)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    array[i] = defaultValue;
                }
            }
        }

        public static void CopyToWithResize<T>(this T[] srcArray, ref T[] destArray)
        {
            if (srcArray == null)
            {
                destArray = null;
            }
            else
            {
                if (destArray == null || destArray.Length != srcArray.Length)
                {
                    destArray = new T[srcArray.Length];
                }

                Array.Copy(srcArray, destArray, srcArray.Length);
            }
        }

        public static bool IsEquivalentArray<T>(this T[] arr, T[] other)
        {
            if ((arr == null) != (other == null))
            {
                return false;
            }

            if (arr.Length != other.Length)
            {
                return false;
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (!arr[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }


        public static bool IsNull<T>(this T[] arr)
        {
            return arr == null;
        }
    }

    public static class DictionaryExtensions
    {
        public static void AddOrSet<T, U>(this Dictionary<T, U> dict, T key, U value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
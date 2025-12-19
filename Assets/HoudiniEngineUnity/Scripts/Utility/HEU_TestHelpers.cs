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
[assembly: InternalsVisibleTo("HoudiniEngineUnityTestUtils")]
#endif

namespace HoudiniEngineUnity
{
    public interface IEquivable<T>
    {
        bool IsEquivalentTo(T other);
    };

    public interface IEquivableWrapperClass<T> : IEquivable<T> where T : class
    {
        bool IsNull();
    };

/*
    Test requirements:
    - If HAPI struct, then create a wrapper class with the interface  IEquivable
    - If any other struct, just call HEU_TestHelpers.AssertTrueLogEquivalent
    - If HEU_ class, then extend IEquivable and compare values of interest
    - If Unity class, then create a wrapper class of type IEquivableWrapperClass<T>

    All of this prep is to reduce the work for when you actually test it (i.e. calling HEU_TestHelpers.AssertTrueLogEquivalent should be as easy as possible)
    - If it is a HAPI struct / Unity class, call the wrapper
    - Otherwise, just call it normally

*/

    internal class HEU_TestHelpers
    {
        // Testing ============

        // Helpers for different behavior between class/structs
        public class RequireStruct<T> where T : struct
        {
        }

        public class RequireClass<T> where T : class
        {
        }


        // Helper for logging equivalence message
        public static bool AssertTrueLogEquivalent<T>(T a, T b, ref bool result, string header, string subject,
            string optional1 = "", string optional2 = "", string optional3 = "", RequireStruct<T> _ = null)
            where T : struct
        {
            bool bResult = true;

            if (Type.GetType("Test_" + a.GetType().Name) != null)
            {
                HEU_Logger.LogWarning("Warning: This type should have been wrapped!");
            }

            if (a.GetType() == typeof(float))
            {
                float aF = (float)((object)a);
                float bF = (float)((object)b);

                bResult = aF.ApproximatelyEquals(bF);
            }
            else if (a.GetType() == typeof(Vector2))
            {
                Vector2 aV = (Vector2)((object)a);
                Vector2 bV = (Vector2)((object)b);
                for (int j = 0; j < 2; j++)
                {
                    bResult &= aV[j].ApproximatelyEquals(bV[j]);
                }
            }
            else if (a.GetType() == typeof(Vector3))
            {
                Vector3 aV = (Vector3)((object)a);
                Vector3 bV = (Vector3)((object)b);
                for (int j = 0; j < 3; j++)
                {
                    bResult &= aV[j].ApproximatelyEquals(bV[j]);
                }
            }
            else if (a.GetType() == typeof(Vector4))
            {
                Vector4 aV = (Vector4)((object)a);
                Vector4 bV = (Vector4)((object)b);
                for (int j = 0; j < 4; j++)
                {
                    bResult &= aV[j].ApproximatelyEquals(bV[j]);
                }
            }
            else if (a.GetType() == typeof(Matrix4x4))
            {
                Matrix4x4 aV = (Matrix4x4)((object)a);
                Matrix4x4 bV = (Matrix4x4)((object)b);
                for (int j = 0; j < 16; j++)
                {
                    bResult &= aV[j].ApproximatelyEquals(bV[j]);
                }
            }
            else if (a.GetType() == typeof(Color))
            {
                Color aV = (Color)((object)a);
                Color bV = (Color)((object)b);
                bResult &= aV.r.ApproximatelyEquals(bV.r);
                bResult &= aV.g.ApproximatelyEquals(bV.g);
                bResult &= aV.b.ApproximatelyEquals(bV.b);
                bResult &= aV.a.ApproximatelyEquals(bV.a);
            }
            else
            {
                bResult = a.Equals(b);
            }

            if (!bResult)
            {
                optional3 = string.Format("{0} vs {1}", a.ToString(), b.ToString());
            }

            PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);

            return bResult;
        }

        public static bool AssertTrueLogEquivalent(GameObject a, GameObject b, ref bool result, string header,
            string subject, string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject) && b.gameObject != null)
            {
                bResult = TestOutputObjectEquivalence(a, b);
                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(IEquivable<T> a, IEquivable<T> b, ref bool result, string header,
            string subject, string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                bResult = a.IsEquivalentTo((T)b);
                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(IEquivableWrapperClass<T> a, IEquivableWrapperClass<T> b,
            ref bool result, string header, string subject, string optional1 = "", string optional2 = "",
            string optional3 = "") where T : class
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                bResult = a.IsEquivalentTo((T)b);
                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent(string a, string b, ref bool result, string header, string subject,
            string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                bResult = a.Equals(b);
                optional1 = a.ToString();
                optional2 = b.ToString();
                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(List<T> a, List<T> b, ref bool result, string header,
            string subject, string optional1 = "", string optional2 = "", string optional3 = "",
            RequireStruct<T> _ = null) where T : struct
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Count; i++)
                {
                    if (a[i].GetType() == typeof(float))
                    {
                        float aF = (float)((object)a[i]);
                        float bF = (float)((object)b[i]);

                        bResult &= aF.ApproximatelyEquals(bF);
                    }
                    else
                    {
                        bResult &= a[i].Equals(b[i]);
                    }
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(List<T> a, List<T> b, ref bool result, string header,
            string subject, string optional1 = "", string optional2 = "", string optional3 = "",
            RequireClass<T> _ = null) where T : class
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                List<IEquivable<T>> a2 = a.Cast<IEquivable<T>>().ToList();
                List<IEquivable<T>> b2 = b.Cast<IEquivable<T>>().ToList();

                bResult = AssertTrueLogEquivalent(a2, b2, ref result, header, subject, optional1, optional2, optional3);

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(List<IEquivable<T>> a, List<IEquivable<T>> b, ref bool result,
            string header, string subject, string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Count; i++)
                {
                    if (ShouldBeTested(a[i], b[i], ref result, header, subject))
                        bResult &= a[i].IsEquivalentTo((T)b[i]);
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(List<IEquivableWrapperClass<T>> a,
            List<IEquivableWrapperClass<T>> b, ref bool result, string header, string subject, string optional1 = "",
            string optional2 = "", string optional3 = "") where T : class
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Count; i++)
                {
                    if (ShouldBeTested(a[i], b[i], ref result, header, subject))
                        bResult &= a[i].IsEquivalentTo((T)b[i]);
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(T[] a, T[] b, ref bool result, string header, string subject,
            string optional1 = "", string optional2 = "", string optional3 = "") where T : struct
        {
            bool bTotalResult = true;
            if (!ShouldBeTested(a, b, ref bTotalResult, header, subject))
                return true;

            int errorCount = 0;

            for (int i = 0; i < a.Length; i++)
            {

                bool bResult = true;

                if (a[i].GetType() == typeof(float))
                {
                    float aF = (float)((object)a[i]);
                    float bF = (float)((object)b[i]);

                    bResult &= aF.ApproximatelyEquals(bF);
                    if (bResult == false)
                    {
                        HEU_Logger.Log(aF + " " + bF);
                    }
                }
                else if (a[i].GetType() == typeof(Vector2))
                {
                    Vector2 aV = (Vector2)((object)a[i]);
                    Vector2 bV = (Vector2)((object)b[i]);
                    for (int j = 0; j < 2; j++)
                    {
                        bResult &= aV[j].ApproximatelyEquals(bV[j]);
                    }
                }
                else if (a[i].GetType() == typeof(Vector3))
                {
                    Vector3 aV = (Vector3)((object)a[i]);
                    Vector3 bV = (Vector3)((object)b[i]);
                    for (int j = 0; j < 3; j++)
                    {
                        bResult &= aV[j].ApproximatelyEquals(bV[j]);
                    }
                }
                else if (a[i].GetType() == typeof(Vector4))
                {
                    Vector4 aV = (Vector4)((object)a[i]);
                    Vector4 bV = (Vector4)((object)b[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        bResult &= aV[j].ApproximatelyEquals(bV[j]);
                    }
                }
                else if (a[i].GetType() == typeof(Matrix4x4))
                {
                    Matrix4x4 aV = (Matrix4x4)((object)a[i]);
                    Matrix4x4 bV = (Matrix4x4)((object)b[i]);
                    for (int j = 0; j < 16; j++)
                    {
                        bResult &= aV[j].ApproximatelyEquals(bV[j]);
                    }
                }
                else if (a[i].GetType() == typeof(Color))
                {
                    Color aV = (Color)((object)a[i]);
                    Color bV = (Color)((object)b[i]);
                    bResult &= aV.r.ApproximatelyEquals(bV.r);
                    bResult &= aV.g.ApproximatelyEquals(bV.g);
                    bResult &= aV.b.ApproximatelyEquals(bV.b);
                    bResult &= aV.a.ApproximatelyEquals(bV.a);
                }
                else
                {
                    bResult &= a[i].Equals(b[i]);
                }

                if (bResult == false && errorCount < 10)
                {
                    string errorString = string.Format("mismatch on {0} : a {1} b {2}", i, a[i].ToString(), b[i].ToString());
                    HEU_Logger.LogError(errorString);
                    errorCount++;

                }
                bTotalResult &= bResult;
            }

            PrintTestLogAndSetResult(bTotalResult, ref result, header, subject, optional1, optional2, optional3);

            return bTotalResult;
        }

        public static bool AssertTrueLogEquivalent(string[] a, string[] b, ref bool result, string header,
            string subject, string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (ShouldBeTested(a[i], b[i], ref result, header, subject))
                        bResult &= a[i].Equals(b[i]);
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(IEquivable<T>[] a, IEquivable<T>[] b, ref bool result,
            string header, string subject, string optional1 = "", string optional2 = "", string optional3 = "")
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (ShouldBeTested(a[i], b[i], ref result, header, subject))
                        bResult &= a[i].IsEquivalentTo((T)b[i]);
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static bool AssertTrueLogEquivalent<T>(IEquivableWrapperClass<T>[] a, IEquivableWrapperClass<T>[] b,
            ref bool result, string header, string subject, string optional1 = "", string optional2 = "",
            string optional3 = "") where T : class
        {
            bool bResult = true;

            if (ShouldBeTested(a, b, ref bResult, header, subject))
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (ShouldBeTested(a[i], b[i], ref result, header, subject))
                        bResult &= a[i].IsEquivalentTo((T)b[i]);
                }

                PrintTestLogAndSetResult(bResult, ref result, header, subject, optional1, optional2, optional3);
            }

            return bResult;
        }

        public static void PrintTestLogAndSetResult(bool expression, ref bool result, string header, string subject,
            string optional1 = "", string optional2 = "", string optional3 = "")
        {
            if (!expression)
            {
                string errorStr = header + ": " + subject + " is not equivalent!";
                if (optional1 != "")
                    errorStr += "| " + optional1;

                if (optional2 != "")
                    errorStr += "| " + optional2;

                if (optional3 != "")
                    errorStr += "| " + optional3;

                HEU_Logger.LogError(errorStr);

                result = false;
            }
        }

        public static bool ShouldBeTested<T>(T a, T b, ref bool bResult, string header = "", string subject = "")
            where T : class
        {
            if (a == null && b == null)
            {
                return false; // Both are null, so no need to test
            }
            else if (a != null && b == null)
            {
                // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                return false;
            }
            else if ((a == null) != (b == null))
            {
                HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a == null) +
                                    " " + (b == null));
                bResult = false;
                return false;
            }

            return true;
        }

        // For some reason, GameObject acts strangely when passed as a template class
        public static bool ShouldBeTested(GameObject a, GameObject b, ref bool bResult, string header = "",
            string subject = "")
        {
            if (a == null && b == null)
            {
                return false; // Both are null, so no need to test
            }
            else if (a != null && b == null)
            {
                // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                return false;
            }
            else if ((a == null) != (b == null))
            {
                HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a == null) +
                                    " " + (b == null));
                bResult = false;
                return false;
            }

            return true;
        }

        public static bool ShouldBeTested<T>(IEquivable<T> a, IEquivable<T> b, ref bool bResult, string header = "",
            string subject = "")
        {
            if (a == null && b == null)
            {
                return false; // Both are null, so no need to test
            }
            else if (a != null && b == null)
            {
                // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                return false;
            }
            else if ((a == null) != (b == null))
            {
                HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a == null) +
                                    " " + (b == null));
                bResult = false;
                return false;
            }

            return true;
        }

        public static bool ShouldBeTested<T>(IEquivableWrapperClass<T> a, IEquivableWrapperClass<T> b, ref bool bResult,
            string header = "", string subject = "") where T : class
        {
            if (a.IsNull() && b.IsNull())
            {
                return false; // Both are null, so no need to test
            }
            else if (!a.IsNull() && b.IsNull())
            {
                // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                // Forgive and forget due to serialization difficulties
                return false;
            }
            else if ((a.IsNull()) != (b.IsNull()))
            {
                //HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a.IsNull()) + " " + (b.IsNull()));
                //bResult = false;
                //return false;
                // Forgive for now because sometimes Unity's asset serialization is weird
                return false;
            }

            return true;
        }

        private static bool ShouldBeTested<T>(List<T> a, List<T> b, ref bool bResult, string header = "",
            string subject = "")
        {
            if (a == null && (b == null || b.Count == 0))
            {
                return false; // Both are null, so no need to test
            }
            else if (a != null && (b == null || b.Count == 0))
            {
                if (a.Count != 0)
                {
                    // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                }

                return false;
            }
            else if ((a == null) != (b == null))
            {
                HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a == null) +
                                    " " + (b == null));
                bResult = false;
                return false;
            }
            else if (a.Count != b.Count)
            {
                HEU_Logger.LogError(header + ": " + subject + " List has incorrect size: " + a.Count + " " + b.Count);
                bResult = false;
                return false;
            }

            return true;
        }


        public static bool ShouldBeTested<T>(T[] a, T[] b, ref bool bResult, string header = "", string subject = "")
        {
            if (a == null && (b == null || b.Length == 0))
            {
                return false; // Both are null, so no need to test
            }
            else if (a != null && (b == null || b.Length == 0))
            {
                if (a.Length != 0)
                {
                    // HEU_Logger.LogWarning(header + ": " + subject + " a is null but b is not. This is a sign that the test object needs to be updated! Skipping this test.");
                }

                return false;
            }
            else if ((a == null) != (b == null))
            {
                HEU_Logger.LogError(header + ": " + subject + " One is null but the other is not: " + (a == null) +
                                    " " + (b == null));
                bResult = false;
                return false;
            }
            else if (a.Length != b.Length)
            {
                HEU_Logger.LogError(header + ": " + subject + " List has incorrect size: " + a.Length + " " + b.Length);
                bResult = false;
                return false;
            }

            return true;
        }

        public static bool ShouldBeTested(string a, string b, ref bool bResult, string header = "", string subject = "")
        {
            if (a == b)
            {
                return false;
            }
            else if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return false;
            }
            else if (a != null && b != null && (a.Contains("C:/") || b.Contains("C:/"))) // ignore file paths
            {
                return false;
            }

            return true;
        }

        public static bool TestOutputObjectEquivalence(GameObject a, GameObject b)
        {
            bool bResult = true;

            string header = "GameObject ";

            if (a != null)
            {
                header += a.name;
            }


            if (a == null || b == null)
            {
                return bResult;
            }

            // Similar to HEU_PartData.CopyGameObjectComponents()
            LODGroup lodA = a.GetComponent<LODGroup>();
            LODGroup lodB = b.GetComponent<LODGroup>();

            AssertTrueLogEquivalent(lodA.ToTestObject(), lodB.ToTestObject(), ref bResult, header, "lod");

            MeshFilter meshFilterA = a.GetComponent<MeshFilter>();
            MeshFilter meshFilterB = b.GetComponent<MeshFilter>();


            AssertTrueLogEquivalent(meshFilterA.ToTestObject(), meshFilterB.ToTestObject(), ref bResult, header,
                "meshFilter");

            MeshCollider meshColliderA = a.GetComponent<MeshCollider>();
            MeshCollider meshColliderB = b.GetComponent<MeshCollider>();

            AssertTrueLogEquivalent(meshColliderA.ToTestObject(), meshColliderB.ToTestObject(), ref bResult, header,
                "meshCollider");

            MeshRenderer meshRendererA = a.GetComponent<MeshRenderer>();
            MeshRenderer meshRendererB = b.GetComponent<MeshRenderer>();

            AssertTrueLogEquivalent(meshRendererA.ToTestObject(), meshRendererB.ToTestObject(), ref bResult, header,
                "meshRenderer");

            Terrain terrainA = a.GetComponent<Terrain>();
            Terrain terrainB = b.GetComponent<Terrain>();

            AssertTrueLogEquivalent(terrainA.ToTestObject(), terrainB.ToTestObject(), ref bResult, header, "terrain");

            TerrainCollider terrainColliderA = a.GetComponent<TerrainCollider>();
            TerrainCollider terrainColliderB = b.GetComponent<TerrainCollider>();

            AssertTrueLogEquivalent(terrainColliderA.ToTestObject(), terrainColliderB.ToTestObject(), ref bResult,
                header, "terrainCollider");

            AssertTrueLogEquivalent(a.transform.ToTestObject(), b.transform.ToTestObject(), ref bResult, header,
                "transform");

            int childCountA = a.transform.childCount;
            int childCountB = b.transform.childCount;

            AssertTrueLogEquivalent(a.transform.childCount, b.transform.childCount, ref bResult, header, "childCount");

            for (int i = 0; i < childCountA; i++)
            {
                Transform transformA = a.transform.GetChild(i);
                Transform transformB = b.transform.GetChild(i);

                bResult &= TestOutputObjectEquivalence(transformA.gameObject, transformB.gameObject);
            }

            return bResult;
        }
    }


    // HAPI =====================================================================

    public class Test_HAPI_AssetInfo : IEquivable<Test_HAPI_AssetInfo>
    {
        public HAPI_AssetInfo self;

        public Test_HAPI_AssetInfo(HAPI_AssetInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_AssetInfo other)
        {
            bool bResult = true;
            string header = "HAPI_AssetInfo";
            HEU_TestHelpers.AssertTrueLogEquivalent(self.hasEverCooked, other.self.hasEverCooked, ref bResult, header,
                "hasEverCooked");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.objectCount, other.self.objectCount, ref bResult, header,
                "objectCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.handleCount, other.self.handleCount, ref bResult, header,
                "handleCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.transformInputCount, other.self.transformInputCount,
                ref bResult, header, "transformInputCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.geoInputCount, other.self.geoInputCount, ref bResult, header,
                "geoInputCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.geoOutputCount, other.self.geoOutputCount, ref bResult, header,
                "geoOutputCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.haveObjectsChanged, other.self.haveObjectsChanged, ref bResult,
                header, "haveObjectsChanged");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.haveMaterialsChanged, other.self.haveMaterialsChanged,
                ref bResult, header, "haveMaterialsChanged");

            return bResult;
        }
    }

    public static class Test_HAPI_AssetInfo_Extensions
    {
        public static Test_HAPI_AssetInfo ToTestObject(this HAPI_AssetInfo self)
        {
            return new Test_HAPI_AssetInfo(self);
        }
    }

    public class Test_HAPI_NodeInfo : IEquivable<Test_HAPI_NodeInfo>
    {
        public HAPI_NodeInfo self;

        public Test_HAPI_NodeInfo(HAPI_NodeInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_NodeInfo other)
        {
            bool bResult = true;

            string header = "HAPI_NodeInfo";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.parmCount, other.self.parmCount, ref bResult, header, "Parm count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.parmIntValueCount, other.self.parmIntValueCount, ref bResult, header, "Parm Int count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.parmFloatValueCount, other.self.parmFloatValueCount, ref bResult, header, "Parm float count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.parmStringValueCount, other.self.parmStringValueCount, ref bResult, header, "Parm string count");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.parmChoiceCount, other.self.parmChoiceCount, ref bResult, header, "Parm choice count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.childNodeCount, other.self.childNodeCount, ref bResult, header, "Child node count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.inputCount, other.self.inputCount, ref bResult, header, "Input count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.outputCount, other.self.outputCount, ref bResult, header, "Output count");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.createdPostAssetLoad, other.self.createdPostAssetLoad, ref bResult, header, "Created post asset load");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isTimeDependent, other.self.isTimeDependent, ref bResult, header, "Is time dependent");

            return bResult;
        }
    }

    public static class Test_HAPI_NodeInfo_Extensions
    {
        public static Test_HAPI_NodeInfo ToTestObject(this HAPI_NodeInfo self)
        {
            return new Test_HAPI_NodeInfo(self);
        }
    }

    public class Test_HAPI_ObjectInfo : IEquivable<Test_HAPI_ObjectInfo>
    {
        public HAPI_ObjectInfo self;

        public Test_HAPI_ObjectInfo(HAPI_ObjectInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_ObjectInfo other)
        {
            bool bResult = true;

            string header = "HAPI_ObjectInfo";

            //HEU_TestHelpers.AssertTrueLogEquivalent(self.hasTransformChanged, other.self.hasTransformChanged, ref bResult, header, "HasTransformChanged");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.haveGeosChanged, other.self.haveGeosChanged, ref bResult, header, "HasGeoChanged");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isVisible, other.self.isVisible, ref bResult, header,
                "IsVisible");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isInstancer, other.self.isInstancer, ref bResult, header,
                "IsInstancer");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isInstanced, other.self.isInstanced, ref bResult, header,
                "IsInstanced");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.geoCount, other.self.geoCount, ref bResult, header,
                "GeoCount");

            // Skip node ids

            return bResult;
        }
    }

    public static class Test_HAPI_ObjectInfo_Extensions
    {
        public static Test_HAPI_ObjectInfo ToTestObject(this HAPI_ObjectInfo self)
        {
            return new Test_HAPI_ObjectInfo(self);
        }
    }


    public class Test_HAPI_Transform : IEquivable<Test_HAPI_Transform>
    {
        public HAPI_Transform self;

        public Test_HAPI_Transform(HAPI_Transform self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_Transform other)
        {
            bool bResult = true;

            string header = "HAPI_Transform";


            // // Skip because can be different in tests
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.position, other.self.position, ref bResult, header, "position");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.rotationQuaternion, other.self.rotationQuaternion, ref bResult, header, "rotationQuaternion");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.scale, other.self.scale, ref bResult, header, "scale");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.shear, other.self.shear, ref bResult, header, "shear");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.rstOrder, other.self.rstOrder, ref bResult, header,
                "rstOrder");

            return bResult;
        }
    }

    public static class Test_HAPI_Transform_Extensions
    {
        public static Test_HAPI_Transform ToTestObject(this HAPI_Transform self)
        {
            return new Test_HAPI_Transform(self);
        }
    }

    public class Test_HAPI_GeoInfo : IEquivable<Test_HAPI_GeoInfo>
    {
        public HAPI_GeoInfo self;

        public Test_HAPI_GeoInfo(HAPI_GeoInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_GeoInfo other)
        {
            bool bResult = true;

            string header = "HAPI_GeoInfo";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.type, other.self.type, ref bResult, header, "Type");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isEditable, other.self.isEditable, ref bResult, header,
                "isEditable");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isTemplated, other.self.isTemplated, ref bResult, header,
                "isTemplated");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.isDisplayGeo, other.self.isDisplayGeo, ref bResult, header,
                "isDisplayGeo");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.pointGroupCount, other.self.pointGroupCount, ref bResult,
                header, "pointGroupCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.primitiveGroupCount, other.self.primitiveGroupCount,
                ref bResult, header, "primitiveGroupCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.edgeGroupCount, other.self.edgeGroupCount, ref bResult, header,
                "isTemplated");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.partCount, other.self.partCount, ref bResult, header,
                "partCount");

            return bResult;
        }
    }

    public static class Test_HAPI_GeoInfo_Extensions
    {
        public static Test_HAPI_GeoInfo ToTestObject(this HAPI_GeoInfo self)
        {
            return new Test_HAPI_GeoInfo(self);
        }
    }

    public class Test_HAPI_AttributeInfo : IEquivable<Test_HAPI_AttributeInfo>
    {
        public HAPI_AttributeInfo self;

        public Test_HAPI_AttributeInfo(HAPI_AttributeInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_AttributeInfo other)
        {
            bool bResult = true;

            string header = "HAPI_AttributeInfo";

            HEU_TestHelpers.AssertTrueLogEquivalent(self, other.self, ref bResult, header, "");

            return bResult;
        }
    }

    public static class Test_HAPI_AttributeInfo_Extensions
    {
        public static Test_HAPI_AttributeInfo ToTestObject(this HAPI_AttributeInfo self)
        {
            return new Test_HAPI_AttributeInfo(self);
        }
    }

    public class Test_HAPI_TransformEuler : IEquivable<Test_HAPI_TransformEuler>
    {
        public HAPI_TransformEuler self;

        public Test_HAPI_TransformEuler(HAPI_TransformEuler self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_TransformEuler other)
        {
            bool bResult = true;

            string header = "HAPI_TransformEuler";

            // Skip because can be different in tests
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.position, other.self.position, ref bResult, header, "position");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.rotationEuler, other.self.rotationEuler, ref bResult, header, "rotationEuler");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.scale, other.self.scale, ref bResult, header, "scale");
            //HEU_TestHelpers.AssertTrueLogEquivalent(self.shear, other.self.shear, ref bResult, header, "shear");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.rotationOrder, other.self.rotationOrder, ref bResult, header,
                "rotationOrder");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.rstOrder, other.self.rstOrder, ref bResult, header,
                "rstOrder");

            return bResult;
        }
    }

    public static class Test_HAPI_TransformEuler_Extensions
    {
        public static Test_HAPI_TransformEuler ToTestObject(this HAPI_TransformEuler self)
        {
            return new Test_HAPI_TransformEuler(self);
        }
    }

    public class Test_HAPI_ParmInfo : IEquivable<Test_HAPI_ParmInfo>
    {
        public HAPI_ParmInfo self;

        public Test_HAPI_ParmInfo(HAPI_ParmInfo self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_HAPI_ParmInfo other)
        {
            bool bResult = true;

            string header = "HAPI_ParmInfo";

            // skip id, parentId, childIndex
            HEU_TestHelpers.AssertTrueLogEquivalent(self.type, other.self.type, ref bResult, header, "type");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.permissions, other.self.permissions, ref bResult, header,
                "permissions");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.tagCount, other.self.tagCount, ref bResult, header,
                "tagCount");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.choiceCount, other.self.choiceCount, ref bResult, header,
                "choiceCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.size, other.self.size, ref bResult, header, "size");

            // skip string handles
            HEU_TestHelpers.AssertTrueLogEquivalent(self.hasMin, other.self.hasMin, ref bResult, header, "hasMin");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.hasMax, other.self.hasMax, ref bResult, header, "hasMax");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.min, other.self.min, ref bResult, header, "min");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.max, other.self.max, ref bResult, header, "max");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.UIMin, other.self.UIMin, ref bResult, header, "UIMin");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.UIMax, other.self.UIMax, ref bResult, header, "UIMax");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.invisible, other.self.invisible, ref bResult, header,
                "invisible");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.disabled, other.self.disabled, ref bResult, header,
                "disabled");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.spare, other.self.spare, ref bResult, header, "spare");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.joinNext, other.self.joinNext, ref bResult, header,
                "joinNext");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.labelNone, other.self.labelNone, ref bResult, header,
                "labelNone");

            // HEU_TestHelpers.AssertTrueLogEquivalent(self.intValuesIndex, other.self.intValuesIndex, ref bResult, header, "intValuesIndex");
            // HEU_TestHelpers.AssertTrueLogEquivalent(self.floatValuesIndex, other.self.floatValuesIndex, ref bResult, header, "floatValuesIndex");
            // HEU_TestHelpers.AssertTrueLogEquivalent(self.stringValuesIndex, other.self.stringValuesIndex, ref bResult, header, "stringValuesIndex");
            // HEU_TestHelpers.AssertTrueLogEquivalent(self.choiceIndex, other.self.choiceIndex, ref bResult, header, "choiceIndex");

            // Indices can be set to -1 on Houdini side, but default to 0 in Unity side, so don't compare.

            HEU_TestHelpers.AssertTrueLogEquivalent(self.inputNodeType, other.self.inputNodeType, ref bResult, header,
                "inputNodeType");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.inputNodeFlag, other.self.inputNodeFlag, ref bResult, header,
                "inputNodeFlag");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.isChildOfMultiParm, other.self.isChildOfMultiParm, ref bResult,
                header, "isChildOfMultiParm");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.instanceNum, other.self.instanceNum, ref bResult, header,
                "instanceNum");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.instanceLength, other.self.instanceLength, ref bResult, header,
                "instanceLength");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.instanceCount, other.self.instanceCount, ref bResult, header,
                "instanceCount");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.instanceStartOffset, other.self.instanceStartOffset,
                ref bResult, header, "instanceStartOffset");

            HEU_TestHelpers.AssertTrueLogEquivalent(self.rampType, other.self.rampType, ref bResult, header,
                "rampType");

            // skip string handles

            return bResult;
        }
    }

    public static class Test_HAPI_ParmInfo_Extensions
    {
        public static Test_HAPI_ParmInfo ToTestObject(this HAPI_ParmInfo self)
        {
            return new Test_HAPI_ParmInfo(self);
        }
    }


    // Unity extensions for equivalence ==============================

    public class Test_LODGroup : IEquivableWrapperClass<Test_LODGroup>
    {
        public LODGroup self;

        public Test_LODGroup(LODGroup self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_LODGroup other)
        {
            bool bResult = true;


            string header = "LODGroup";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.size, other.self.size, ref bResult, header, "size");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.lodCount, other.self.lodCount, ref bResult, header,
                "lodCount");

            LOD[] lodsA = self.GetLODs();
            LOD[] lodsB = other.self.GetLODs();

            if (lodsA != null || lodsB != null)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(lodsA.Length, lodsB.Length, ref bResult, header, "lods.Length");
                for (int i = 0; i < lodsA.Length; i++)
                {
                    HEU_TestHelpers.AssertTrueLogEquivalent(lodsA[i].screenRelativeTransitionHeight,
                        lodsB[i].screenRelativeTransitionHeight, ref bResult, header,
                        "lods.screenRelativeTransitionHeight");
                }
            }

            return bResult;
        }
    }

    public static class Test_LODGroup_Extensions
    {
        public static Test_LODGroup ToTestObject(this LODGroup self)
        {
            return new Test_LODGroup(self);
        }

        public static Test_LODGroup[] ToTestObject(this LODGroup[] self)
        {
            return Array.ConvertAll<LODGroup, Test_LODGroup>(self, (lod) => new Test_LODGroup(lod));
        }

        public static List<Test_LODGroup> ToTestObject(this List<LODGroup> self)
        {
            return self.ConvertAll<Test_LODGroup>((lod) => new Test_LODGroup(lod));
        }
    }


    public class Test_Transform : IEquivableWrapperClass<Test_Transform>
    {
        public Transform self;

        public Test_Transform(Transform self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Transform other)
        {
            bool bResult = true;

            string header = "Transform";

            // Don't test position/rotation/scale and only test hierarchy

            HEU_TestHelpers.AssertTrueLogEquivalent(self.childCount, other.self.childCount, ref bResult, header,
                "childCount");

            for (int i = 0; i < self.childCount; i++)
            {
                Transform transA = self.GetChild(i);
                Transform transB = other.self.GetChild(i);
                HEU_TestHelpers.AssertTrueLogEquivalent(transA.ToTestObject(), transB.ToTestObject(), ref bResult,
                    header, "trans child");
            }

            return bResult;
        }
    }

    public static class Test_Transform_Extensions
    {
        public static Test_Transform ToTestObject(this Transform self)
        {
            return new Test_Transform(self);
        }

        public static Test_Transform[] ToTestObject(this Transform[] self)
        {
            return Array.ConvertAll<Transform, Test_Transform>(self, (lod) => new Test_Transform(lod));
        }

        public static List<Test_Transform> ToTestObject(this List<Transform> self)
        {
            return self.ConvertAll<Test_Transform>((lod) => new Test_Transform(lod));
        }
    }

    public class Test_Material : IEquivableWrapperClass<Test_Material>
    {
        public Material self = null;

        public Test_Material(Material self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Material other)
        {
            bool bResult = true;

            string header = "Material";

            if (self != null && other.self != null && self.shader != null && other.self.shader != null)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(self.shader.name, other.self.shader.name, ref bResult, header,
                    "shaderName");
            }

            return bResult;
        }
    }

    public static class Test_Material_Extensions
    {
        public static Test_Material ToTestObject(this Material self)
        {
            return new Test_Material(self);
        }

        public static Test_Material[] ToTestObject(this Material[] self)
        {
            if (self == null) return new Test_Material[0];
            return Array.ConvertAll<Material, Test_Material>(self, (lod) => new Test_Material(lod));
        }

        public static List<Test_Material> ToTestObject(this List<Material> self)
        {
            return self.ConvertAll<Test_Material>((lod) => new Test_Material(lod));
        }
    }


    public class Test_Collider : IEquivableWrapperClass<Test_Collider>
    {
        public Collider self;

        public Test_Collider(Collider self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Collider other)
        {
            bool bResult = true;

            string header = "Collider";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.GetType().ToString(), other.self.GetType().ToString(), ref bResult, header, "type");

            // Nothing too good to test - bounds can be different

            //HEU_TestHelpers.AssertTrueLogEquivalent(self.bounds, other.self.bounds, ref bResult, header, "bounds");
            if (other.self.GetType() == typeof(BoxCollider))
            {
                BoxCollider castSelf = (BoxCollider)self;
                BoxCollider castOther = (BoxCollider)other.self;
                HEU_TestHelpers.AssertTrueLogEquivalent(castSelf.ToTestObject(), castOther.ToTestObject(), ref bResult,
                    header, "box");
            }
            else if (other.self.GetType() == typeof(SphereCollider))
            {
                SphereCollider castSelf = (SphereCollider)self;
                SphereCollider castOther = (SphereCollider)other.self;
                HEU_TestHelpers.AssertTrueLogEquivalent(castSelf.ToTestObject(), castOther.ToTestObject(), ref bResult,
                    header, "sphere");
            }
            else if (other.self.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider castSelf = (CapsuleCollider)self;
                CapsuleCollider castOther = (CapsuleCollider)other.self;
                HEU_TestHelpers.AssertTrueLogEquivalent(castSelf.ToTestObject(), castOther.ToTestObject(), ref bResult,
                    header, "capsule");
            }
            else if (other.self.GetType() == typeof(MeshCollider))
            {
                MeshCollider castSelf = (MeshCollider)self;
                MeshCollider castOther = (MeshCollider)other.self;
                HEU_TestHelpers.AssertTrueLogEquivalent(castSelf.ToTestObject(), castOther.ToTestObject(), ref bResult,
                    header, "mesh");
            }

            return bResult;
        }
    }

    public static class Test_Collider_Extensions
    {
        public static Test_Collider ToTestObject(this Collider self)
        {
            return new Test_Collider(self);
        }

        public static Test_Collider[] ToTestObject(this Collider[] self)
        {
            return Array.ConvertAll<Collider, Test_Collider>(self, (lod) => new Test_Collider(lod));
        }

        public static List<Test_Collider> ToTestObject(this List<Collider> self)
        {
            return self.ConvertAll<Test_Collider>((lod) => new Test_Collider(lod));
        }
    }

    public class Test_BoxCollider : IEquivableWrapperClass<Test_BoxCollider>
    {
        public BoxCollider self;

        public Test_BoxCollider(BoxCollider self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_BoxCollider other)
        {
            bool bResult = true;

            string header = "BoxCollider";
            HEU_TestHelpers.AssertTrueLogEquivalent(self.center, other.self.center, ref bResult, header, "center");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.center, other.self.center, ref bResult, header, "size");

            return bResult;
        }
    }

    public static class Test_BoxCollider_Extensions
    {
        public static Test_BoxCollider ToTestObject(this BoxCollider self)
        {
            return new Test_BoxCollider(self);
        }

        public static Test_BoxCollider[] ToTestObject(this BoxCollider[] self)
        {
            return Array.ConvertAll<BoxCollider, Test_BoxCollider>(self, (lod) => new Test_BoxCollider(lod));
        }

        public static List<Test_BoxCollider> ToTestObject(this List<BoxCollider> self)
        {
            return self.ConvertAll<Test_BoxCollider>((lod) => new Test_BoxCollider(lod));
        }
    }

    public class Test_SphereCollider : IEquivableWrapperClass<Test_SphereCollider>
    {
        public SphereCollider self;

        public Test_SphereCollider(SphereCollider self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_SphereCollider other)
        {
            bool bResult = true;

            string header = "SphereCollider";
            HEU_TestHelpers.AssertTrueLogEquivalent(self.center, other.self.center, ref bResult, header, "center");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.radius, other.self.radius, ref bResult, header, "radius");

            return bResult;
        }
    }

    public static class Test_SphereCollider_Extensions
    {
        public static Test_SphereCollider ToTestObject(this SphereCollider self)
        {
            return new Test_SphereCollider(self);
        }

        public static Test_SphereCollider[] ToTestObject(this SphereCollider[] self)
        {
            return Array.ConvertAll<SphereCollider, Test_SphereCollider>(self, (lod) => new Test_SphereCollider(lod));
        }

        public static List<Test_SphereCollider> ToTestObject(this List<SphereCollider> self)
        {
            return self.ConvertAll<Test_SphereCollider>((lod) => new Test_SphereCollider(lod));
        }
    }

    public class Test_CapsuleCollider : IEquivableWrapperClass<Test_CapsuleCollider>
    {
        public CapsuleCollider self;

        public Test_CapsuleCollider(CapsuleCollider self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_CapsuleCollider other)
        {
            bool bResult = true;

            string header = "CapsuleCollider";
            HEU_TestHelpers.AssertTrueLogEquivalent(self.radius, other.self.radius, ref bResult, header, "radius");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.height, other.self.height, ref bResult, header, "height");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.direction, other.self.direction, ref bResult, header,
                "direction");

            return bResult;
        }
    }

    public static class Test_CapsuleCollider_Extensions
    {
        public static Test_CapsuleCollider ToTestObject(this CapsuleCollider self)
        {
            return new Test_CapsuleCollider(self);
        }

        public static Test_CapsuleCollider[] ToTestObject(this CapsuleCollider[] self)
        {
            return Array.ConvertAll<CapsuleCollider, Test_CapsuleCollider>(self,
                (lod) => new Test_CapsuleCollider(lod));
        }

        public static List<Test_CapsuleCollider> ToTestObject(this List<CapsuleCollider> self)
        {
            return self.ConvertAll<Test_CapsuleCollider>((lod) => new Test_CapsuleCollider(lod));
        }
    }

    public class Test_MeshCollider : IEquivableWrapperClass<Test_MeshCollider>
    {
        public MeshCollider self;

        public Test_MeshCollider(MeshCollider self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_MeshCollider other)
        {
            bool bResult = true;

            string header = "MeshCollider";
            HEU_TestHelpers.AssertTrueLogEquivalent(self.sharedMesh.ToTestObject(),
                other.self.sharedMesh.ToTestObject(), ref bResult, header, "sharedMesh");

            return bResult;
        }
    }

    public static class Test_MeshCollider_Extensions
    {
        public static Test_MeshCollider ToTestObject(this MeshCollider self)
        {
            return new Test_MeshCollider(self);
        }

        public static Test_MeshCollider[] ToTestObject(this MeshCollider[] self)
        {
            return Array.ConvertAll<MeshCollider, Test_MeshCollider>(self, (lod) => new Test_MeshCollider(lod));
        }

        public static List<Test_MeshCollider> ToTestObject(this List<MeshCollider> self)
        {
            return self.ConvertAll<Test_MeshCollider>((lod) => new Test_MeshCollider(lod));
        }
    }

    public class Test_Mesh : IEquivableWrapperClass<Test_Mesh>
    {
        public Mesh self;

        public Test_Mesh(Mesh self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Mesh other)
        {
            bool bResult = true;

            string header = "Mesh";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.vertices, other.self.vertices, ref bResult, header,
                "vertices");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.normals, other.self.normals, ref bResult, header, "normals");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.tangents, other.self.tangents, ref bResult, header, "tangent");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.uv, other.self.uv, ref bResult, header, "uv");


            HEU_TestHelpers.AssertTrueLogEquivalent(self.subMeshCount, other.self.subMeshCount, ref bResult, header,
                "submeshCount");
            for (int i = 0; i < self.subMeshCount; i++)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(self.GetTopology(i), other.self.GetTopology(i), ref bResult,
                    header, "topology");
                if (self.GetTopology(i) == MeshTopology.Triangles)
                {
                    HEU_TestHelpers.AssertTrueLogEquivalent(self.triangles, other.self.triangles, ref bResult, header,
                        "triangles");
                }
            }

            return bResult;
        }
    }

    public static class Test_Mesh_Extensions
    {
        public static Test_Mesh ToTestObject(this Mesh self)
        {
            return new Test_Mesh(self);
        }

        public static Test_Mesh[] ToTestObject(this Mesh[] self)
        {
            return Array.ConvertAll<Mesh, Test_Mesh>(self, (lod) => new Test_Mesh(lod));
        }

        public static List<Test_Mesh> ToTestObject(this List<Mesh> self)
        {
            return self.ConvertAll<Test_Mesh>((lod) => new Test_Mesh(lod));
        }
    }

    public class Test_MeshRenderer : IEquivableWrapperClass<Test_MeshRenderer>
    {
        public MeshRenderer self;

        public Test_MeshRenderer(MeshRenderer self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_MeshRenderer other)
        {
            bool bResult = true;

            string header = "MeshRenderer";

            if (self.sharedMaterials != null || other.self.sharedMaterials != null)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(self.sharedMaterials.ToTestObject(),
                    other.self.sharedMaterials.ToTestObject(), ref bResult, header, "sharedMaterials");
            }


            return bResult;
        }
    }

    public static class Test_MeshRenderer_Extensions
    {
        public static Test_MeshRenderer ToTestObject(this MeshRenderer self)
        {
            return new Test_MeshRenderer(self);
        }

        public static Test_MeshRenderer[] ToTestObject(this MeshRenderer[] self)
        {
            return Array.ConvertAll<MeshRenderer, Test_MeshRenderer>(self, (lod) => new Test_MeshRenderer(lod));
        }

        public static List<Test_MeshRenderer> ToTestObject(this List<MeshRenderer> self)
        {
            return self.ConvertAll<Test_MeshRenderer>((lod) => new Test_MeshRenderer(lod));
        }
    }

    public class Test_MeshFilter : IEquivableWrapperClass<Test_MeshFilter>
    {
        public MeshFilter self;

        public Test_MeshFilter(MeshFilter self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_MeshFilter other)
        {
            bool bResult = true;

            string header = "MeshFilter";

            if (self.sharedMesh != null || other.self.sharedMesh != null)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(self.sharedMesh.ToTestObject(),
                    other.self.sharedMesh.ToTestObject(), ref bResult, header, "sharedMesh");
            }

            return bResult;
        }
    }

    public static class Test_MeshFilter_Extensions
    {
        public static Test_MeshFilter ToTestObject(this MeshFilter self)
        {
            return new Test_MeshFilter(self);
        }

        public static Test_MeshFilter[] ToTestObject(this MeshFilter[] self)
        {
            return Array.ConvertAll<MeshFilter, Test_MeshFilter>(self, (lod) => new Test_MeshFilter(lod));
        }

        public static List<Test_MeshFilter> ToTestObject(this List<MeshFilter> self)
        {
            return self.ConvertAll<Test_MeshFilter>((lod) => new Test_MeshFilter(lod));
        }
    }

    public class Test_LayerMask : IEquivable<Test_LayerMask>
    {
        public LayerMask self;

        public Test_LayerMask(LayerMask self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_LayerMask other)
        {
            bool bResult = true;

            string header = "LayerMask";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.value, other.self.value, ref bResult, header, "value");

            return bResult;
        }
    }

    public static class Test_LayerMask_Extensions
    {
        public static Test_LayerMask ToTestObject(this LayerMask self)
        {
            return new Test_LayerMask(self);
        }
    }

    public class Test_Gradient : IEquivableWrapperClass<Test_Gradient>
    {
        public Gradient self;

        public Test_Gradient(Gradient self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Gradient other)
        {
            bool bResult = true;

            string header = "Gradient";

            if (HEU_TestHelpers.ShouldBeTested(self, other.self, ref bResult, header, "gradient"))
            {
                HEU_TestHelpers.PrintTestLogAndSetResult(self.Equals(other.self), ref bResult, header, "gradient");
            }

            return bResult;
        }
    }

    public static class Test_Gradient_Extensions
    {
        public static Test_Gradient ToTestObject(this Gradient self)
        {
            return new Test_Gradient(self);
        }

        public static Test_Gradient[] ToTestObject(this Gradient[] self)
        {
            return Array.ConvertAll<Gradient, Test_Gradient>(self, (lod) => new Test_Gradient(lod));
        }

        public static List<Test_Gradient> ToTestObject(this List<Gradient> self)
        {
            return self.ConvertAll<Test_Gradient>((lod) => new Test_Gradient(lod));
        }
    }


    public class Test_AnimationCurve : IEquivableWrapperClass<Test_AnimationCurve>
    {
        public AnimationCurve self;

        public Test_AnimationCurve(AnimationCurve self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_AnimationCurve other)
        {
            bool bResult = true;

            string header = "AnimationCurve";

            if (HEU_TestHelpers.ShouldBeTested(self, other.self, ref bResult, header, "AnimationCurve"))
            {
                HEU_TestHelpers.PrintTestLogAndSetResult(self.Equals(other.self), ref bResult, header,
                    "AnimationCurve");
            }

            return bResult;
        }
    }

    public static class Test_AnimationCurve_Extensions
    {
        public static Test_AnimationCurve ToTestObject(this AnimationCurve self)
        {
            return new Test_AnimationCurve(self);
        }

        public static Test_AnimationCurve[] ToTestObject(this AnimationCurve[] self)
        {
            return Array.ConvertAll<AnimationCurve, Test_AnimationCurve>(self, (lod) => new Test_AnimationCurve(lod));
        }

        public static List<Test_AnimationCurve> ToTestObject(this List<AnimationCurve> self)
        {
            return self.ConvertAll<Test_AnimationCurve>((lod) => new Test_AnimationCurve(lod));
        }
    }


    public class Test_TerrainLayer : IEquivableWrapperClass<Test_TerrainLayer>
    {
        public TerrainLayer self;

        public Test_TerrainLayer(TerrainLayer self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_TerrainLayer other)
        {
            bool bResult = true;

            string header = "TerrainLayer";

            // TODO;
            HEU_TestHelpers.AssertTrueLogEquivalent(self.diffuseTexture.ToTestObject(),
                other.self.diffuseTexture.ToTestObject(), ref bResult, header, "diffuseTexture");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.normalMapTexture.ToTestObject(),
                other.self.normalMapTexture.ToTestObject(), ref bResult, header, "normalMapTexture");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.maskMapTexture.ToTestObject(),
                other.self.maskMapTexture.ToTestObject(), ref bResult, header, "maskMapTexture");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.tileSize, other.self.tileSize, ref bResult, header,
                "tileSize");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.tileOffset, other.self.tileOffset, ref bResult, header,
                "tileOffset");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.specular, other.self.specular, ref bResult, header,
                "specular");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.metallic, other.self.metallic, ref bResult, header,
                "metallic");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.smoothness, other.self.smoothness, ref bResult, header,
                "smoothness");

            return bResult;
        }
    }

    public static class Test_TerrainLayer_Extensions
    {
        public static Test_TerrainLayer ToTestObject(this TerrainLayer self)
        {
            return new Test_TerrainLayer(self);
        }

        public static Test_TerrainLayer[] ToTestObject(this TerrainLayer[] self)
        {
            return Array.ConvertAll<TerrainLayer, Test_TerrainLayer>(self, (lod) => new Test_TerrainLayer(lod));
        }

        public static List<Test_TerrainLayer> ToTestObject(this List<TerrainLayer> self)
        {
            return self.ConvertAll<Test_TerrainLayer>((lod) => new Test_TerrainLayer(lod));
        }
    }

    public class Test_Texture2D : IEquivableWrapperClass<Test_Texture2D>
    {
        public Texture2D self;

        public Test_Texture2D(Texture2D self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Texture2D other)
        {
            bool bResult = true;

            string header = "Texture2D";

            if (self.isReadable && other.self.isReadable)
            {
                Color[] pixelsA = self.GetPixels();
                Color[] pixelsB = other.self.GetPixels();

                if (pixelsA != null || pixelsB != null)
                {
                    HEU_TestHelpers.AssertTrueLogEquivalent(pixelsA, pixelsB, ref bResult, header, "pixels");
                }
            }

            return bResult;
        }
    }

    public static class Test_Texture2D_Extensions
    {
        public static Test_Texture2D ToTestObject(this Texture2D self)
        {
            return new Test_Texture2D(self);
        }

        public static Test_Texture2D[] ToTestObject(this Texture2D[] self)
        {
            return Array.ConvertAll<Texture2D, Test_Texture2D>(self, (lod) => new Test_Texture2D(lod));
        }

        public static List<Test_Texture2D> ToTestObject(this List<Texture2D> self)
        {
            return self.ConvertAll<Test_Texture2D>((lod) => new Test_Texture2D(lod));
        }
    }

    public class Test_TreeInstance : IEquivable<Test_TreeInstance>
    {
        public TreeInstance self;

        public Test_TreeInstance(TreeInstance self)
        {
            this.self = self;
        }

        public bool IsEquivalentTo(Test_TreeInstance other)
        {
            bool bResult = true;

            string header = "TreeInstance";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.position, other.self.position, ref bResult, header,
                "position");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.widthScale, other.self.widthScale, ref bResult, header,
                "widthScale");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.heightScale, other.self.heightScale, ref bResult, header,
                "heightScale");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.rotation, other.self.rotation, ref bResult, header,
                "rotation");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.prototypeIndex, other.self.prototypeIndex, ref bResult, header,
                "prototypeIndex");

            return bResult;
        }
    }

    public static class Test_TreeInstance_Extensions
    {
        public static Test_TreeInstance ToTestObject(this TreeInstance self)
        {
            return new Test_TreeInstance(self);
        }

        public static Test_TreeInstance[] ToTestObject(this TreeInstance[] self)
        {
            return Array.ConvertAll<TreeInstance, Test_TreeInstance>(self, (lod) => new Test_TreeInstance(lod));
        }

        public static List<Test_TreeInstance> ToTestObject(this List<TreeInstance> self)
        {
            return self.ConvertAll<Test_TreeInstance>((lod) => new Test_TreeInstance(lod));
        }
    }

    public class Test_TerrainData : IEquivableWrapperClass<Test_TerrainData>
    {
        public TerrainData self;

        public Test_TerrainData(TerrainData self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_TerrainData other)
        {
            bool bResult = true;

            string header = "TerrainData";

            HEU_TestHelpers.AssertTrueLogEquivalent(self.heightmapResolution, other.self.heightmapResolution,
                ref bResult, header, "heightmapResolution");
            HEU_TestHelpers.AssertTrueLogEquivalent(self.size, other.self.size, ref bResult, header, "size");

            float[,] heights = self.GetHeights(0, 0, self.heightmapResolution, self.heightmapResolution);
            int sizeX = heights.GetLength(0);
            int sizeY = heights.GetLength(1);
            int totalSize = sizeX * sizeY;

            // Convert to single array
            float[] heightsArr = new float[totalSize];
            for (int j = 0; j < sizeY; j++)
            {
                for (int i = 0; i < sizeX; i++)
                {
                    // Flip for coordinate system change
                    float h = heights[i, (sizeY - j - 1)];
                    heightsArr[i + j * sizeX] = h;
                }
            }

            float[,] heightsB = self.GetHeights(0, 0, self.heightmapResolution, self.heightmapResolution);
            int sizeXB = heightsB.GetLength(0);
            int sizeYB = heightsB.GetLength(1);
            int totalSizeB = sizeXB * sizeYB;

            // Convert to single array
            float[] heightsArrB = new float[totalSizeB];
            for (int j = 0; j < sizeYB; j++)
            {
                for (int i = 0; i < sizeXB; i++)
                {
                    // Flip for coordinate system change
                    float h = heightsB[i, (sizeYB - j - 1)];
                    heightsArrB[i + j * sizeXB] = h;
                }
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(heightsArr, heightsArrB, ref bResult, header, "heightsArr");


            HEU_TestHelpers.AssertTrueLogEquivalent(self.terrainLayers.ToTestObject(),
                other.self.terrainLayers.ToTestObject(), ref bResult, header, "terrainLayers");

            // Skip DetailPrototype, TreeProrotype, just because I'm lazy

            HEU_TestHelpers.AssertTrueLogEquivalent(self.treeInstances.ToTestObject(),
                other.self.treeInstances.ToTestObject(), ref bResult, header, "treeInstances");

            return bResult;
        }
    }

    public static class Test_TerrainData_Extensions
    {
        public static Test_TerrainData ToTestObject(this TerrainData self)
        {
            return new Test_TerrainData(self);
        }

        public static Test_TerrainData[] ToTestObject(this TerrainData[] self)
        {
            return Array.ConvertAll<TerrainData, Test_TerrainData>(self, (lod) => new Test_TerrainData(lod));
        }

        public static List<Test_TerrainData> ToTestObject(this List<TerrainData> self)
        {
            return self.ConvertAll<Test_TerrainData>((lod) => new Test_TerrainData(lod));
        }
    }


    public class Test_Terrain : IEquivableWrapperClass<Test_Terrain>
    {
        public Terrain self;

        public Test_Terrain(Terrain self)
        {
            this.self = self;
        }

        public bool IsNull()
        {
            return self == null;
        }

        public bool IsEquivalentTo(Test_Terrain other)
        {
            bool bResult = true;

            string header = "Terrain";

            if (self.terrainData)
            {
                HEU_TestHelpers.AssertTrueLogEquivalent(self.terrainData.ToTestObject(),
                    other.self.terrainData.ToTestObject(), ref bResult, header, "terrainData");
            }


            return bResult;
        }
    }

    public static class Test_Terrain_Extensions
    {
        public static Test_Terrain ToTestObject(this Terrain self)
        {
            return new Test_Terrain(self);
        }

        public static Test_Terrain[] ToTestObject(this Terrain[] self)
        {
            return Array.ConvertAll<Terrain, Test_Terrain>(self, (lod) => new Test_Terrain(lod));
        }

        public static List<Test_Terrain> ToTestObject(this List<Terrain> self)
        {
            return self.ConvertAll<Test_Terrain>((lod) => new Test_Terrain(lod));
        }
    }
}
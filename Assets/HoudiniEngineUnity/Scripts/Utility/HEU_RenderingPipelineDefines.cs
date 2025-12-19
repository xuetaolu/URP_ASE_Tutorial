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
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoudiniEngineUnity
{
// Helper written by: https://gist.github.com/cjaube/944b0d5221808c2a761d616f29deaf49

// To use:
//    #if UNITY_PIPELINE_URP
//    // code for URP
//    #elif UNITY_PIPELINE_HDRP
//    // code for HDRP
//    #else
//    // code for Stardard Pipeline
//    #endif

    public enum HEU_PipelineType
    {
        Unsupported,
        BiRP,
        URP,
        HDRP
    }

#if UNITY_EDITOR && HOUDINIENGINEUNITY_ENABLED
    [InitializeOnLoad]
#endif
    public class HEU_RenderingPipelineDefines
    {
        static HEU_RenderingPipelineDefines()
        {
            UpdateDefines();
        }

        /// <summary>
        /// Update the unity pipeline defines for URP
        /// </summary>
        private static void UpdateDefines()
        {
            var pipeline = GetPipeline();

            if (pipeline == HEU_PipelineType.URP)
            {
                AddDefine("UNITY_PIPELINE_URP");
            }
            else
            {
                RemoveDefine("UNITY_PIPELINE_URP");
            }

            if (pipeline == HEU_PipelineType.HDRP)
            {
                AddDefine("UNITY_PIPELINE_HDRP");
            }
            else
            {
                RemoveDefine("UNITY_PIPELINE_HDRP");
            }
        }


        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        public static HEU_PipelineType GetPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return HEU_PipelineType.HDRP;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                         srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return HEU_PipelineType.URP;
                }
                else return HEU_PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
        if (GraphicsSettings.renderPipelineAsset != null) {
            // SRP not supported before 2019
            return HEU_PipelineType.Unsupported;
        }
#endif
            // no SRP
            return HEU_PipelineType.BiRP;
        }

        /// <summary>
        /// Add a custom define
        /// </summary>
        /// <param name="define"></param>
        /// <param name="buildTargetGroup"></param>
        private static void AddDefine(string define)
        {
            var definesList = GetDefines();
            if (!definesList.Contains(define))
            {
                definesList.Add(define);
                SetDefines(definesList);
            }
        }

        /// <summary>
        /// Remove a custom define
        /// </summary>
        /// <param name="_define"></param>
        /// <param name="_buildTargetGroup"></param>
        public static void RemoveDefine(string define)
        {
            var definesList = GetDefines();
            if (definesList.Contains(define))
            {
                definesList.Remove(define);
                SetDefines(definesList);
            }
        }

        public static List<string> GetDefines()
        {
#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Split(';').ToList();
#else
        return new List<string>();
#endif
        }

        public static void SetDefines(List<string> definesList)
        {
#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines = string.Join(";", definesList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
#endif
        }
    }
}
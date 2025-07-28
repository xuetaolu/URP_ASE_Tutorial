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
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_PartId = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_StringHandle = System.Int32;


    // Wrapper around Unity's Material, with some helper functions.
    public class HEU_MaterialData : ScriptableObject, IHEU_MaterialData, IEquivable<HEU_MaterialData>
    {
        // Where the material originated from
        internal enum Source
        {
            DEFAULT,
            HOUDINI,
            UNITY,
            SUBSTANCE
        }

        // PUBLIC ========================================================
        public Material Material => _material;

        public HEU_MaterialSourceWrapper MaterialSource => MaterialSource_WrapperToInternal(_materialSource);

        public int MaterialKey
        {
            get => _materialKey;
            set => _materialKey = value;
        }

        /// <summary>
        /// For this object's _material, we update the shader attributes and 
        /// fetch the textures from Houdini.
        /// </summary>
        /// <param name="materialInfo">This material's info from Houdini</param>
        /// <param name="assetCacheFolderPath">Path to asset's cache folder</param>
        public bool UpdateMaterialFromHoudini(HAPI_MaterialInfo materialInfo, string assetCacheFolderPath)
        {
            if (_material == null)
            {
                return false;
            }

            HEU_SessionBase session = HEU_SessionManager.GetOrCreateDefaultSession();

            HAPI_NodeInfo nodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(materialInfo.nodeId, ref nodeInfo))
            {
                return false;
            }

            // Get all parameters of this material
            HAPI_ParmInfo[] parmInfos = new HAPI_ParmInfo[nodeInfo.parmCount];
            if (!HEU_GeneralUtility.GetArray1Arg(materialInfo.nodeId, session.GetParams, parmInfos, 0,
                    nodeInfo.parmCount))
            {
                return false;
            }

            // Assign transparency shader or non-transparent.

            bool isTransparent = IsTransparentMaterial(session, materialInfo.nodeId, parmInfos);
            if (isTransparent)
            {
                _material.shader = HEU_MaterialFactory.FindPluginShader(HEU_PluginSettings.DefaultTransparentShader);
            }
            else
            {
                _material.shader = HEU_MaterialFactory.FindPluginShader(HEU_PluginSettings.DefaultStandardShader);
            }

            if (HEU_PluginSettings.UseLegacyShaders)
            {
                return UseLegacyShaders(materialInfo, assetCacheFolderPath, session, nodeInfo, parmInfos);
            }

            // Diffuse texture - render & extract
            int diffuseMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_OGL_TEX1_ATTR, HEU_Defines.MAT_OGL_TEX1_ATTR_ENABLED);
            if (diffuseMapParmIndex < 0)
            {
                diffuseMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_BASECOLOR_ATTR, HEU_Defines.MAT_BASECOLOR_ATTR_ENABLED);
            }

            if (diffuseMapParmIndex >= 0 && diffuseMapParmIndex < parmInfos.Length)
            {
                string diffuseTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[diffuseMapParmIndex]);
                _material.mainTexture = HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                    parmInfos[diffuseMapParmIndex].id, diffuseTextureFileName, assetCacheFolderPath, false);
            }

            Color diffuseColor;
            if (!HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_DIFF_ATTR, Color.white, out diffuseColor))
            {
                HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_DIFF_ATTR, Color.white, out diffuseColor);
            }

            float alpha;
            GetMaterialAlpha(session, materialInfo.nodeId, parmInfos, 1f, out alpha);

            if (isTransparent)
            {
                int opacityMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                    parmInfos, HEU_Defines.MAT_OGL_OPACITY_MAP_ATTR, HEU_Defines.MAT_OGL_OPACITY_MAP_ATTR_ENABLED);
                if (opacityMapParmIndex < 0)
                {
                    opacityMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                        parmInfos, HEU_Defines.MAT_OPACITY_MAP_ATTR, HEU_Defines.MAT_OPACITY_MAP_ATTR_ENABLED);
                }

                if (opacityMapParmIndex >= 0 && opacityMapParmIndex < parmInfos.Length)
                {
                    string opacityTextureFileName = GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId,
                        parmInfos[opacityMapParmIndex]);
                    _material.SetTexture(HEU_Defines.UNITY_SHADER_OPACITY_MAP,
                        HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                            parmInfos[opacityMapParmIndex].id, opacityTextureFileName, assetCacheFolderPath, false));
                }
            }

            diffuseColor.a = alpha;
            _material.SetColor(HEU_Defines.UNITY_SHADER_COLOR, diffuseColor);

            if (HEU_PluginSettings.UseSpecularShader)
            {
                Color specular;
                Color defaultSpecular = new Color(0.2f, 0.2f, 0.2f, 1);
                if (!HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                        HEU_Defines.MAT_OGL_SPEC_ATTR, defaultSpecular, out specular))
                {
                    HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                        HEU_Defines.MAT_SPEC_ATTR, defaultSpecular, out specular);
                }

                _material.SetColor(HEU_Defines.UNITY_SHADER_SPEC_COLOR, specular);

                int specMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_OGL_SPEC_MAP_ATTR, HEU_Defines.MAT_OGL_SPEC_MAP_ATTR_ENABLED);
                if (specMapParmIndex < 0)
                {
                    specMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                        HEU_Defines.MAT_SPEC_MAP_ATTR, HEU_Defines.MAT_SPEC_MAP_ATTR_ENABLED);
                }

                if (specMapParmIndex >= 0 && specMapParmIndex < parmInfos.Length)
                {
                    string specTextureFileName =
                        GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[specMapParmIndex]);
                    _material.SetTexture(HEU_Defines.UNITY_SHADER_SPEC_MAP,
                        HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                            parmInfos[specMapParmIndex].id, specTextureFileName, assetCacheFolderPath, false));
                }
            }
            else
            {
                float metallic = 0;
                if (!HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                        HEU_Defines.MAT_OGL_METALLIC_ATTR, 0f, out metallic))
                {
                    HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                        HEU_Defines.MAT_METALLIC_ATTR, 0f, out metallic);
                }

                _material.SetFloat(HEU_Defines.UNITY_SHADER_METALLIC, metallic);

                int metallicMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                    parmInfos, HEU_Defines.MAT_OGL_METALLIC_MAP_ATTR, HEU_Defines.MAT_OGL_METALLIC_MAP_ATTR_ENABLED);
                if (metallicMapParmIndex < 0)
                {
                    metallicMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                        parmInfos, HEU_Defines.MAT_METALLIC_MAP_ATTR, HEU_Defines.MAT_METALLIC_MAP_ATTR_ENABLED);
                }


                if (metallicMapParmIndex >= 0 && metallicMapParmIndex < parmInfos.Length)
                {
                    string metallicTextureFileName = GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId,
                        parmInfos[metallicMapParmIndex]);
                    _material.SetTexture(HEU_Defines.UNITY_SHADER_METALLIC_MAP,
                        HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                            parmInfos[metallicMapParmIndex].id, metallicTextureFileName, assetCacheFolderPath, false));
                }
            }

            // Normal map - render & extract texture
            int normalMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_NORMAL_ATTR, HEU_Defines.MAT_NORMAL_ATTR_ENABLED);
            if (normalMapParmIndex < 0)
            {
                normalMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_OGL_NORMAL_ATTR, "");
            }

            if (normalMapParmIndex >= 0 && normalMapParmIndex < parmInfos.Length)
            {
                string normalTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[normalMapParmIndex]);
                Texture2D normalMap = HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                    parmInfos[normalMapParmIndex].id, normalTextureFileName, assetCacheFolderPath, true);
                if (normalMap != null)
                {
                    _material.SetTexture(HEU_Defines.UNITY_SHADER_BUMP_MAP, normalMap);
                }
            }

            // Emission
            Color emission;
            Color defaultEmission = new Color(0, 0, 0, 0);
            if (!HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_EMISSIVE_ATTR, defaultEmission, out emission))
            {
                HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_EMISSIVE_ATTR, defaultEmission, out emission);
            }

            _material.SetColor(HEU_Defines.UNITY_SHADER_EMISSION_COLOR, emission);

            int emissionMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_OGL_EMISSIVE_MAP_ATTR, HEU_Defines.MAT_OGL_EMISSIVE_MAP_ATTR_ENABLED);
            if (emissionMapParmIndex < 0)
            {
                emissionMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_EMISSIVE_MAP_ATTR, HEU_Defines.MAT_EMISSIVE_MAP_ATTR_ENABLED);
            }

            if (emissionMapParmIndex >= 0 && emissionMapParmIndex < parmInfos.Length)
            {
                string emissionTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[emissionMapParmIndex]);
                _material.SetTexture(HEU_Defines.UNITY_SHADER_EMISSION_MAP,
                    HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                        parmInfos[emissionMapParmIndex].id, emissionTextureFileName, assetCacheFolderPath, false));
            }

            // Smoothness (need to invert roughness!)
            float roughness;
            float defaultRoughness = 0.5f;
            if (!HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_ROUGH_ATTR, defaultRoughness, out roughness))
            {
                HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_ROUGH_ATTR, defaultRoughness, out roughness);
            }

            // Clamp shininess to non-zero as results in very hard shadows. Unity's UI does not allow zero either.
            _material.SetFloat(HEU_Defines.UNITY_SHADER_SMOOTHNESS, Mathf.Max(0.03f, 1.0f - roughness));

            int roughMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_OGL_ROUGH_MAP_ATTR, HEU_Defines.MAT_OGL_ROUGH_MAP_ATTR_ENABLED);
            if (roughMapParmIndex < 0)
            {
                roughMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_ROUGH_MAP_ATTR, HEU_Defines.MAT_ROUGH_MAP_ATTR_ENABLED);
            }

            if (roughMapParmIndex >= 0 && roughMapParmIndex < parmInfos.Length)
            {
                string roughTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[roughMapParmIndex]);
                _material.SetTexture(HEU_Defines.UNITY_SHADER_SMOOTHNESS_MAP,
                    HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                        parmInfos[roughMapParmIndex].id, roughTextureFileName, assetCacheFolderPath, false,
                        invertTexture: true));
            }

            // Occlusion (only has ogl map) 
            int occlusionMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                parmInfos, HEU_Defines.MAT_OGL_OCCLUSION_MAP_ATTR, HEU_Defines.MAT_OGL_ROUGH_MAP_ATTR_ENABLED);

            if (occlusionMapParmIndex >= 0 && occlusionMapParmIndex < parmInfos.Length)
            {
                string occlusionTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[occlusionMapParmIndex]);
                _material.SetTexture(HEU_Defines.UNITY_SHADER_OCCLUSION_MAP,
                    HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                        parmInfos[occlusionMapParmIndex].id, occlusionTextureFileName, assetCacheFolderPath, false));
            }

            return true;
        }

        /// <summary>
        /// Returns true if this material was pre-existing in Unity and not generated from Houdini at cook time.
        /// </summary>
        public bool IsExistingMaterial()
        {
            return _materialSource == Source.UNITY || _materialSource == Source.SUBSTANCE;
        }


        // ===============================================================

        // Actual Unity material
        [SerializeField] internal Material _material;

        [SerializeField] internal Source _materialSource;

        // The ID generated by this plugin for managing on the Unity side.
        // All HEU_MaterialData will have a unique ID, either same as _materialHoudiniID for Houdini materials.
        // or hash of material path

        // The ID used to uniquely identify a material.
        // For Houdini materials, this is the ID returned by the material info.
        // For existing Unity materials (via unity_material attribute), this is 
        // the hash of the material path on project (eg. Assets/Materials/materialname.mat)
        [SerializeField] internal int _materialKey = HEU_Defines.HEU_INVALID_MATERIAL;

        private bool UseLegacyShaders(HAPI_MaterialInfo materialInfo, string assetCacheFolderPath,
            HEU_SessionBase session, HAPI_NodeInfo nodeInfo, HAPI_ParmInfo[] parmInfos)
        {
            // Diffuse texture - render & extract
            int diffuseMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_OGL_TEX1_ATTR, HEU_Defines.MAT_OGL_TEX1_ATTR_ENABLED);
            if (diffuseMapParmIndex < 0)
            {
                diffuseMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_BASECOLOR_ATTR, HEU_Defines.MAT_BASECOLOR_ATTR_ENABLED);
                if (diffuseMapParmIndex < 0)
                {
                    diffuseMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id,
                        parmInfos, HEU_Defines.MAT_MAP_ATTR, "");
                }
            }

            if (diffuseMapParmIndex >= 0 && diffuseMapParmIndex < parmInfos.Length)
            {
                string diffuseTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[diffuseMapParmIndex]);
                _material.mainTexture = HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                    parmInfos[diffuseMapParmIndex].id, diffuseTextureFileName, assetCacheFolderPath, false);
            }

            // Normal map - render & extract texture
            int normalMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                HEU_Defines.MAT_NORMAL_ATTR, HEU_Defines.MAT_NORMAL_ATTR_ENABLED);
            if (normalMapParmIndex < 0)
            {
                normalMapParmIndex = HEU_ParameterUtility.FindTextureParamByNameOrTag(session, nodeInfo.id, parmInfos,
                    HEU_Defines.MAT_OGL_NORMAL_ATTR, "");
            }

            if (normalMapParmIndex >= 0 && normalMapParmIndex < parmInfos.Length)
            {
                string normalTextureFileName =
                    GetTextureFileNameFromMaterialParam(session, materialInfo.nodeId, parmInfos[normalMapParmIndex]);
                Texture2D normalMap = HEU_MaterialFactory.RenderAndExtractImageToTexture(session, materialInfo,
                    parmInfos[normalMapParmIndex].id, normalTextureFileName, assetCacheFolderPath, true);
                if (normalMap != null)
                {
                    _material.SetTexture(HEU_Defines.UNITY_SHADER_BUMP_MAP, normalMap);
                }
            }

            // Assign shader properties

            // Clamp shininess to non-zero as results in very hard shadows. Unity's UI does not allow zero either.

            float shininess;
            if (!HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_ROUGH_ATTR, 0f, out shininess))
            {
                HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_ROUGH_ATTR, 0f, out shininess);
            }

            _material.SetFloat(HEU_Defines.UNITY_SHADER_SHININESS, Mathf.Max(0.03f, 1.0f - shininess));

            Color diffuseColor;
            if (!HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_DIFF_ATTR, Color.white, out diffuseColor))
            {
                HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_DIFF_ATTR, Color.white, out diffuseColor);
            }

            float alpha;
            if (!HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_ALPHA_ATTR, 1f, out alpha))
            {
                HEU_ParameterUtility.GetParameterFloatValue(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_ALPHA_ATTR, 1f, out alpha);
            }

            diffuseColor.a = alpha;
            _material.SetColor(HEU_Defines.UNITY_SHADER_COLOR, diffuseColor);

            Color specular;
            if (!HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_OGL_SPEC_ATTR, Color.black, out specular))
            {
                HEU_ParameterUtility.GetParameterColor3Value(session, materialInfo.nodeId, parmInfos,
                    HEU_Defines.MAT_SPEC_ATTR, Color.black, out specular);
            }

            _material.SetColor(HEU_Defines.UNITY_SHADER_SPEC_COLOR, specular);

            return true;
        }

        /// <summary>
        /// Return the file name for the given material node's parameter.
        /// </summary>
        /// <param name="session">Current session</param>
        /// <param name="nodeID">Material node ID</param>
        /// <param name="parmInfo">Parameter on material to query</param>
        /// <returns>Given parameter's string value</returns>
        internal static string GetTextureFileNameFromMaterialParam(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo parmInfo)
        {
            string textureFileName = "default_texture.png";

            HAPI_StringHandle stringValue;
            string paramName = HEU_SessionManager.GetString(parmInfo.nameSH, session);
            if (session.GetParmStringValue(nodeID, paramName, 0, true, out stringValue))
            {
                string paramStrValue = HEU_SessionManager.GetString(stringValue, session);

                // The returned string needs to be cleaned up:
                // eg. opdef:/Sop/testgeometry_pighead?lowres.jpg -> Sop_testgeometry_pighead_lowres.jpg
                textureFileName = paramStrValue;

                int lastColon = textureFileName.LastIndexOf(':');
                if (lastColon > 0 && (lastColon + 1) < textureFileName.Length)
                {
                    textureFileName = textureFileName.Substring(lastColon + 1);
                }

                // Remove starting / after removing :: above
                textureFileName = textureFileName.TrimStart('/');

                textureFileName = textureFileName.Replace("?", "_");
                textureFileName = textureFileName.Replace("/", "_");


                // Filename is too long! Shorten it!
                int lastDot = textureFileName.LastIndexOf('.');
                if (lastDot != -1)
                {
                    string baseName = textureFileName.Substring(0, lastDot);

                    while (baseName.Length > 50 && baseName.IndexOf('_') != -1)
                    {
                        baseName = baseName.Substring(baseName.IndexOf('_') + 1);
                    }

                    textureFileName = baseName + textureFileName.Substring(lastDot);
                }

                //HEU_Logger.LogFormat("Texture File Name: {0}, {1}", paramStrValue, textureFileName);
            }

            return textureFileName;
        }

        /// <summary>
        /// Retruns true if the material (via its parameters) is a transparent material or not.
        /// </summary>
        /// <param name="session">Current Houdini session</param>
        /// <param name="nodeID">The material node ID</param>
        /// <param name="parameters">Parameter array containing material info</param>
        /// <returns>True if the material is transparent</returns>
        internal static bool IsTransparentMaterial(HEU_SessionBase session, HAPI_NodeId nodeID,
            HAPI_ParmInfo[] parameters)
        {
            float alpha;
            GetMaterialAlpha(session, nodeID, parameters, 1, out alpha);
            return alpha < 0.95f;
        }

        // Gets the alpha of the material
        // Checks ogl_use_alpha_transparency to make sure that it's enabled.
        internal static bool GetMaterialAlpha(HEU_SessionBase session, HAPI_NodeId nodeID, HAPI_ParmInfo[] parameters,
            float defaultValue, out float alpha)
        {
            int foundUseParmId = HEU_ParameterUtility.GetParameterIndexFromNameOrTag(session, nodeID, parameters,
                HEU_Defines.MAT_OGL_TRANSPARENCY_ATTR_ENABLED);
            if (foundUseParmId >= 0)
            {
                // Found a valid "use" parameter. Check if it is disabled.
                int[] useValue = new int[1];
                int intValuesIndex = parameters[foundUseParmId].intValuesIndex;

                if (session.GetParamIntValues(nodeID, useValue, parameters[foundUseParmId].intValuesIndex, 1))
                {
                    if (useValue.Length > 0 && useValue[0] == 0)
                    {
                        // We found the texture, but the use tag is disabled, so don't use it!
                        alpha = defaultValue;
                        return false;
                    }
                }
            }

            if (HEU_ParameterUtility.GetParameterFloatValue(session, nodeID, parameters, HEU_Defines.MAT_OGL_ALPHA_ATTR,
                    defaultValue, out alpha))
            {
                return true;
            }

            if (HEU_ParameterUtility.GetParameterFloatValue(session, nodeID, parameters, HEU_Defines.MAT_ALPHA_ATTR,
                    defaultValue, out alpha))
            {
                return true;
            }

            if (HEU_ParameterUtility.GetParameterFloatValue(session, nodeID, parameters,
                    HEU_Defines.MAT_OGL_TRANSPARENCY_ATTR, defaultValue, out alpha))
            {
                alpha = 1 - alpha;
                return true;
            }

            alpha = defaultValue;
            return false;
        }

        /// <summary>
        /// Returns null if the given image info supports a Unity friendly image format.
        /// Otherwise returns a file format that we know Unity supports.
        /// </summary>
        /// <param name="imageInfo">Image info containing the current image file format</param>
        /// <returns></returns>
        internal static string GetSupportedFileFormat(HEU_SessionBase session, ref HAPI_ImageInfo imageInfo)
        {
            string desiredFileFormatName = null;

            string imageInfoFileFormat = HEU_SessionManager.GetString(imageInfo.imageFileFormatNameSH, session);

            if (!imageInfoFileFormat.Equals(HEU_HAPIConstants.HAPI_PNG_FORMAT_NAME)
                && !imageInfoFileFormat.Equals(HEU_HAPIConstants.HAPI_JPEG_FORMAT_NAME)
                && !imageInfoFileFormat.Equals(HEU_HAPIConstants.HAPI_BMP_FORMAT_NAME)
                && !imageInfoFileFormat.Equals(HEU_HAPIConstants.HAPI_TGA_FORMAT_NAME))
            {
                desiredFileFormatName = HEU_HAPIConstants.HAPI_PNG_FORMAT_NAME;
            }

            return desiredFileFormatName;
        }

        public bool IsEquivalentTo(HEU_MaterialData other)
        {
            bool bResult = true;

            string header = "HEU_MaterialData";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._material.ToTestObject(), other._material.ToTestObject(),
                ref bResult, header, "_material");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._materialSource, other._materialSource, ref bResult, header,
                "_materialSource");

            // Skip _materialKey

            return bResult;
        }

        internal static Source MaterialSource_WrapperToInternal(HEU_MaterialSourceWrapper materialSource)
        {
            switch (materialSource)
            {
                case HEU_MaterialSourceWrapper.DEFAULT:
                    return Source.DEFAULT;
                case HEU_MaterialSourceWrapper.HOUDINI:
                    return Source.HOUDINI;
                case HEU_MaterialSourceWrapper.UNITY:
                    return Source.UNITY;
                case HEU_MaterialSourceWrapper.SUBSTANCE:
                    return Source.SUBSTANCE;
                default:
                    return Source.DEFAULT;
            }
        }

        internal static HEU_MaterialSourceWrapper MaterialSource_WrapperToInternal(Source materialSource)
        {
            switch (materialSource)
            {
                case Source.DEFAULT:
                    return HEU_MaterialSourceWrapper.DEFAULT;
                case Source.HOUDINI:
                    return HEU_MaterialSourceWrapper.HOUDINI;
                case Source.UNITY:
                    return HEU_MaterialSourceWrapper.UNITY;
                case Source.SUBSTANCE:
                    return HEU_MaterialSourceWrapper.SUBSTANCE;
                default:
                    return HEU_MaterialSourceWrapper.DEFAULT;
            }
        }
    }
} // HoudiniEngineUnity
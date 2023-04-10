// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEngine;

namespace UnityEditor
{
    namespace BrainFailProductions.BatchFew
    {
        internal class BatchFewShaderGUI : ShaderGUI
        {
            public enum WorkflowMode
            {
                Specular,
                Metallic,
                Dielectric
            }

            public enum BlendMode
            {
                Opaque,
                Cutout,
                Fade,
                // Old school alpha-blending mode, fresnel does not affect amount of transparency
                Transparent
                // Physically plausible transparency mode, implemented as alpha pre-multiply
            }

            public enum SmoothnessMapChannel
            {
                SpecularMetallicAlpha,
                AlbedoAlpha,
            }

            private static class Styles
            {
                public static GUIContent uvSetLabel = new GUIContent("UV Set");
                public static GUIContent detailMode = new GUIContent("Detail Texture Mode");
                public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
                public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
                public static GUIContent specularMapText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
                public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
                public static GUIContent smoothnessMapChannelText = new GUIContent("Source", "Smoothness texture and channel");
                public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");
                public static GUIContent reflectionsText = new GUIContent("Reflections", "Glossy Reflections");
                public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
                public static GUIContent heightMapText = new GUIContent("Height Map", "Height Map (G)");
                public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
                public static GUIContent emissionText = new GUIContent("Emission", "Emission (RGB)");
                public static GUIContent detailMaskText = new GUIContent("Detail Mask", "Mask for Secondary Maps (A)");
                public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
                public static GUIContent detailNormalMapText = new GUIContent("Normal Map", "Normal Map");
                public static GUIContent propertyMapText = new GUIContent("Attributes Texture", "Texture which holds attributes for individual materials");
                public static GUIContent emissionMode = new GUIContent("Emission Mode", "None, color, or array based emission");
                public static GUIContent parallaxMode = new GUIContent("Parallax Mode", "None, Offset, or POM");
                public static GUIContent parallaxSteps = new GUIContent("Parallax Steps", "Number of taps to perform in Parallax Occlusion Mapping, more is more expensive");
                public static string primaryMapsText = "Main Maps";
                public static string secondaryMapsText = "Secondary Maps";
                public static string forwardText = "Forward Rendering Options";
                public static string renderingMode = "Rendering Mode";
                public static string advancedText = "Advanced Options";
                public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
                public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
            }

            MaterialProperty blendMode = null;
            MaterialProperty albedoMap = null;
            MaterialProperty specularMap = null;
            MaterialProperty metallicMap = null;
            MaterialProperty smoothnessMapChannel = null;
            MaterialProperty highlights = null;
            MaterialProperty reflections = null;
            MaterialProperty bumpMap = null;
            MaterialProperty occlusionMap = null;
            MaterialProperty heightMap = null;
            MaterialProperty emissionMap = null;
            MaterialProperty detailMask = null;
            MaterialProperty detailAlbedoMap = null;
            MaterialProperty detailNormalMap = null;
            MaterialProperty uvSetSecondary = null;
            MaterialProperty attrImg = null;
            MaterialProperty detailMode = null;
            MaterialProperty emissionMode = null;
            MaterialProperty parallaxMode = null;
            MaterialProperty parallaxSteps = null;
            MaterialProperty detailAlbedoSingle = null;
            MaterialProperty detailNormalSingle = null;

            MaterialEditor m_MaterialEditor;
            WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
            bool m_FirstTimeApply = true;

            public void FindProperties(MaterialProperty[] props)
            {
                blendMode = FindProperty("_Mode", props);
                albedoMap = FindProperty("_MainTex", props);
                //alphaCutoff = FindProperty("_Cutoff", props);
                specularMap = FindProperty("_SpecGlossMap", props, false);
                metallicMap = FindProperty("_MetallicGlossMap", props, false);
                parallaxMode = FindProperty("_ParallaxMode", props);
                parallaxSteps = FindProperty("_ParallaxSteps", props);

                if (specularMap != null) { m_WorkflowMode = WorkflowMode.Specular; }

                else if (metallicMap != null) { m_WorkflowMode = WorkflowMode.Metallic; }

                else { m_WorkflowMode = WorkflowMode.Dielectric; }


                smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
                highlights = FindProperty("_SpecularHighlights", props, false);
                reflections = FindProperty("_GlossyReflections", props, false);
                bumpMap = FindProperty("_BumpMap", props);
                heightMap = FindProperty("_ParallaxMap", props);
                occlusionMap = FindProperty("_OcclusionMap", props);
                emissionMap = FindProperty("_EmissionMap", props);
                emissionMode = FindProperty("_EmissionMode", props);
                detailMask = FindProperty("_DetailMask", props);
                detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
                detailNormalMap = FindProperty("_DetailNormalMap", props);
                detailAlbedoSingle = FindProperty("_DetailAlbedoSingle", props);
                detailNormalSingle = FindProperty("_DetailNormalSingle", props);
                uvSetSecondary = FindProperty("_UVSec", props);
                detailMode = FindProperty("_DetailMode", props);
                attrImg = FindProperty("_AttrImg", props);

            }

            public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
            {
                FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
                m_MaterialEditor = materialEditor;
                Material material = materialEditor.target as Material;

                // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
                // material to a standard shader.
                // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
                if (m_FirstTimeApply)
                {
                    MaterialChanged(material, m_WorkflowMode);
                    m_FirstTimeApply = false;
                }
                ShaderPropertiesGUI(material);
            }

            public void ShaderPropertiesGUI(Material material)
            {
                // Use default labelWidth
                EditorGUIUtility.labelWidth = 0f;

                // Detect any changes to the material
                EditorGUI.BeginChangeCheck();
                {
                    BlendModePopup();

                    // Primary properties
                    GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                    m_MaterialEditor.TexturePropertySingleLine(Styles.propertyMapText, attrImg);
                    DoAlbedoArea(material);

                    m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap);
                    m_MaterialEditor.ShaderProperty(parallaxMode, Styles.parallaxMode.text);

                    if (material.GetFloat("_ParallaxMode") == 2)
                    {
                        m_MaterialEditor.ShaderProperty(parallaxSteps, Styles.parallaxSteps.text);
                    }

                    if (m_WorkflowMode != WorkflowMode.Metallic)
                    {
                        DoSpecularMetallicArea();
                        m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap);
                        m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap);
                    }

                    m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);

                    DoEmissionArea(material);
                    EditorGUILayout.Space();

                    // Secondary properties
                    GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                    m_MaterialEditor.ShaderProperty(detailMode, Styles.detailMode.text);
                    if (detailMode.floatValue > 0.5f)
                    {
                        m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoSingle);
                        m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalSingle);
                    }
                    else
                    {
                        m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                        m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap);
                    }
                    m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                    // Third properties
                    GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                    if (highlights != null) { m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText); }

                    if (reflections != null) { m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText); }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var obj in blendMode.targets) { MaterialChanged((Material)obj, m_WorkflowMode); }
                }

                EditorGUILayout.Space();

                GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
                m_MaterialEditor.RenderQueueField();
                m_MaterialEditor.EnableInstancingField();
                m_MaterialEditor.DoubleSidedGIField();
            }

            internal void DetermineWorkflow(MaterialProperty[] props)
            {
                if (FindProperty("_SpecGlossMap", props, false) != null) { m_WorkflowMode = WorkflowMode.Specular; }

                else if (FindProperty("_MetallicGlossMap", props, false) != null) { m_WorkflowMode = WorkflowMode.Metallic; }

                else { m_WorkflowMode = WorkflowMode.Dielectric; }
            }


            public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
            {
                base.AssignNewShaderToMaterial(material, oldShader, newShader);

                if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
                {
                    SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                    return;
                }

                BlendMode blendMode = BlendMode.Opaque;
                if (oldShader.name.Contains("/Transparent/Cutout/"))
                {
                    blendMode = BlendMode.Cutout;
                }
                else if (oldShader.name.Contains("/Transparent/"))
                {
                    // NOTE: legacy shaders did not provide physically based transparency
                    // therefore Fade mode
                    blendMode = BlendMode.Fade;
                }
                material.SetFloat("_Mode", (float)blendMode);

                DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
                MaterialChanged(material, m_WorkflowMode);
            }

            void BlendModePopup()
            {
                EditorGUI.showMixedValue = blendMode.hasMixedValue;
                var mode = (BlendMode)blendMode.floatValue;

                EditorGUI.BeginChangeCheck();
                mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                    blendMode.floatValue = (float)mode;
                }

                EditorGUI.showMixedValue = false;
            }

            void DoAlbedoArea(Material material)
            {
                m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap);
                if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
                {
                    // m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
                }
            }

            void DoEmissionArea(Material material)
            {
                m_MaterialEditor.ShaderProperty(emissionMode, Styles.emissionMode.text);
                // Texture and HDR color controls
                if (material.GetFloat("_EmissionMode") == 2)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.emissionText, emissionMap);
                }

                // change the GI flag and fix it up with emissive as black if necessary
                //m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);

            }

            void DoSpecularMetallicArea()
            {
                if (m_WorkflowMode == WorkflowMode.Specular)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap);
                }
                else if (m_WorkflowMode == WorkflowMode.Metallic)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap);
                }


                int indentation = 2; // align with labels of texture properties
                ++indentation;
                if (smoothnessMapChannel != null)
                    m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
            }

            static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
            {
                switch (blendMode)
                {
                    case BlendMode.Opaque:
                        material.SetOverrideTag("RenderType", "");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        break;
                    case BlendMode.Cutout:
                        material.SetOverrideTag("RenderType", "TransparentCutout");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                        break;
                    case BlendMode.Fade:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        break;
                    case BlendMode.Transparent:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        break;
                }
            }

            static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
            {
                int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
                if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                    return SmoothnessMapChannel.AlbedoAlpha;
                else
                    return SmoothnessMapChannel.SpecularMetallicAlpha;
            }

            static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
            {
                // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
                // (MaterialProperty value might come from renderer material property block)
                SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
                if (workflowMode == WorkflowMode.Specular && material.HasProperty("_SpecGlossMap"))
                    SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
                else if (workflowMode == WorkflowMode.Metallic && material.HasProperty("_MetallicGlossMap"))
                    SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));

                material.DisableKeyword("_PARALLAXMAP");
                material.DisableKeyword("_POM");
                if (material.GetTexture("_ParallaxMap"))
                {
                    float pm = material.GetFloat("_ParallaxMode");
                    if (pm == 1)
                    {
                        material.EnableKeyword("_PARALLAXMAP");
                    }
                    else if (pm == 2)
                    {
                        material.EnableKeyword("_POM");
                    }
                }

                SetKeyword(material, "_DETAIL_MULX2", material.GetFloat("_DetailMode") == 0 && (material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap")));
                SetKeyword(material, "_DETAIL_SINGLE", material.GetFloat("_DetailMode") == 1 && (material.GetTexture("_DetailAlbedoSingle") || material.GetTexture("_DetailNormalSingle")));

                // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
                // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
                // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
                //MaterialEditor.FixupEmissiveFlag(material);
                //bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                //SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
                float emode = material.GetFloat("_EmissionMode");
                SetKeyword(material, "_EMISSION", emode == 2 && material.GetTexture("_EmissionMap") != null);
                SetKeyword(material, "_EMISSION_COLOR", emode == 1);

                if (material.HasProperty("_SmoothnessTextureChannel"))
                {
                    SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
                }
            }

            public static void MaterialChanged(Material material, WorkflowMode workflowMode)
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

                SetMaterialKeywords(material, workflowMode);
            }

            static void SetKeyword(Material m, string keyword, bool state)
            {
                if (state)
                    m.EnableKeyword(keyword);
                else
                    m.DisableKeyword(keyword);
            }



        }
    }
}
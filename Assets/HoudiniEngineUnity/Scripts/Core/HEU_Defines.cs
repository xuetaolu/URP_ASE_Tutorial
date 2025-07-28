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

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs
    using HAPI_SessionId = System.Int64;
    using HAPI_Int64 = System.Int64;
    using HAPI_StringHandle = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_NodeId = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_PartId = System.Int32;
    using HAPI_PDG_WorkitemId = System.Int32;
    using HAPI_PDG_GraphContextId = System.Int32;

    /// <summary>
    /// Definitions for Houdini Engine for Unity
    /// </summary>
    public class HEU_Defines
    {
        // Unity-Only Constants ---------------------------------------------

        // Menu
        public const string HEU_PRODUCT_NAME = "HoudiniEngine";

        // Used for log messages.
        public const string HEU_NAME = "Houdini Engine";

        public static string HEU_PLUGIN_PATH = Application.dataPath + "/HoudiniEngineUnity";

        public static string HEU_TEXTURES_PATH = HEU_PLUGIN_PATH + "/Textures";
        public static string HEU_BAKED_ASSETS_PATH = Application.dataPath + "/Baked Assets";
        public static string HEU_ENGINE_ASSETS = Application.dataPath + "/HoudiniEngineAssets";

        public const string HAPI_PATH = "HAPI_PATH";

        // Asset environment path prefix
        public const string HEU_ENVPATH_PREFIX = "HEU_ENVPATH_";
        public const string HEU_ENVPATH_KEY = "$";

        public const HAPI_NodeId HEU_INVALID_NODE_ID = -1;

        public const string HEU_DEFAULT_ASSET_NAME = "HoudiniAssetRoot";

        // Session
        public const string HEU_SESSION_PIPENAME = "hapi";
        public const string HEU_SESSION_LOCALHOST = "localhost";
        public const int HEU_SESSION_PORT = 9090;
        public const float HEU_SESSION_TIMEOUT = 10000f;
        public const bool HEU_SESSION_AUTOCLOSE = true;

        public const int HAPI_MAX_PAGE_SIZE = 20000;
        public const int HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW = 3;
        public const int HAPI_MAX_VERTICES_PER_FACE = 3;

        public const bool HAPI_CURVE_REFINE_TO_LINEAR = true;
        public const float HAPI_CURVE_LOD = 8.0f;

        public const float HAPI_VOLUME_POSITION_MULT = 2.0f;
        public const float HAPI_VOLUME_SURFACE_MAX_PT_PER_C = 64000; // Max points per container. 65000 is Unity max.
        public const float HAPI_VOLUME_SURFACE_DELTA_MULT = 1.2f;
        public const float HAPI_VOLUME_SURFACE_PT_SIZE_MULT = 1800.0f;
        public const string DEFAULT_TOP_NODE_FILTER = "HE_";
        public const string DEFAULT_TOP_OUTPUT_FILTER = "HE_OUT_";

        // Shared Constants -------------------------------------------------
        //
        // Similar to the auto-generated HEU_HAPIConstants, but Unity plugin might need some more
        public const string HAPI_ATTRIB_ORIENT = "orient";
        public const string HAPI_ATTRIB_ROTATION = "rot";
        public const string HAPI_ATTRIB_SCALE = "scale";
        public const string HAPI_ATTRIB_ALPHA = "Alpha";
        public const string HAPI_HANDLE_TRANSFORM = "xform";
        public const int HAPI_MAX_UVS = 8;

        public const string HAPI_OBJMERGE_TRANSFORM_PARAM = "xformtype";
        public const string HAPI_OBJMERGE_PACK_GEOMETRY = "pack";
        public const string HAPI_OBJPATH_1_PARAM = "objpath1";

        // Messages
        public const string NO_EXISTING_SESSION = "No existing session.";
        public const string HEU_ERROR_TITLE = "Houdini Engine Error";
        public const string HEU_INSTALL_INFO = "Houdini Engine Installation Info";

        // Storage
        public const string PLUGIN_STORE_KEYS = "HoudiniEnginePluginKeys";
        public const string PLUGIN_STORE_DATA = "HoudiniEnginePluginData";
        public const string PLUGIN_SESSION_DATA = "HoudiniEngineSession";
        public const string PLUGIN_SETTINGS_FILE = "heu_settings.ini";
        public const string PLUGIN_SESSION_FILE = "heu_session.txt";
        public const string COOK_LOGS_FILE = "cook_logs_file.txt";

        // Collision
        public const string DEFAULT_COLLISION_GEO = "collision_geo";
        public const string DEFAULT_RENDERED_COLLISION_GEO = "rendered_collision_geo";
        public const string DEFAULT_RENDERED_CONVEX_COLLISION_GEO = "rendered_convex_collision_geo";
        public const string DEFAULT_CONVEX_COLLISION_GEO = "convex";
        public const string DEFAULT_SIMPLE_COLLISION_GEO = "collision_geo_simple";
        public const string DEFAULT_SIMPLE_RENDERED_COLLISION_GEO = "rendered_collision_geo_simple";
        public const string DEFAULT_SIMPLE_RENDERED_CONVEX_COLLISION_GEO = "rendered_convex_collision_geo_simple";
        public const string DEFAULT_COLLISION_TRIGGER = "trigger";

        // Materials
        public const string DEFAULT_UNITY_MATERIAL_ATTR = "unity_material";
        public const string DEFAULT_UNITY_SUBMATERIAL_NAME_ATTR = "unity_sub_material_name";
        public const string DEFAULT_UNITY_SUBMATERIAL_INDEX_ATTR = "unity_sub_material_index";

        // Heightfield layer attributes
        public const string DEFAULT_UNITY_HEIGHTFIELD_TEXTURE_DIFFUSE_ATTR = "unity_hf_texture_diffuse";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TEXTURE_MASK_ATTR = "unity_hf_texture_mask";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TEXTURE_NORMAL_ATTR = "unity_hf_texture_normal";
        public const string DEFAULT_UNITY_HEIGHTFIELD_NORMAL_SCALE_ATTR = "unity_hf_normal_scale";
        public const string DEFAULT_UNITY_HEIGHTFIELD_METALLIC_ATTR = "unity_hf_metallic";
        public const string DEFAULT_UNITY_HEIGHTFIELD_SMOOTHNESS_ATTR = "unity_hf_smoothness";
        public const string DEFAULT_UNITY_HEIGHTFIELD_SPECULAR_ATTR = "unity_hf_specular";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TILE_OFFSET_ATTR = "unity_hf_tile_offset";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TILE_SIZE_ATTR = "unity_hf_tile_size";

        public const string DEFAULT_UNITY_HEIGHTFIELD_TERRAINDATA_FILE_ATTR = "unity_hf_terraindata_file";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TERRAINDATA_EXPORT_FILE_ATTR = "unity_hf_terraindata_export_file";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TERRAINLAYER_FILE_ATTR = "unity_hf_terrainlayer_file";
        public const string DEFAULT_UNITY_HEIGHTFIELD_TERRAINDATA_EXPORT_PATH = "unity_hf_terraindata_export_path";
        public const string DEFAULT_UNITY_HEIGHTFIELD_HEIGHT_RANGE = "unity_hf_height_range";
        public const string DEFAULT_UNITY_HEIGHTFIELD_YPOS = "unity_hf_ypos";

        public const string HEIGHTFIELD_TREEPROTOTYPE = "unity_hf_tree_prototype";

        public const string HEIGHTFIELD_TREEINSTANCE_PROTOTYPEINDEX = "unity_hf_treeinstance_prototypeindex";
        public const string HEIGHTFIELD_TREEINSTANCE_HEIGHTSCALE = "unity_hf_treeinstance_heightscale";
        public const string HEIGHTFIELD_TREEINSTANCE_WIDTHSCALE = "unity_hf_treeinstance_widthscale";
        public const string HEIGHTFIELD_TREEINSTANCE_LIGHTMAPCOLOR = "unity_hf_treeinstance_lightmapcolor";

        public const string HEIGHTFIELD_DETAIL_RESOLUTION_PER_PATCH = "unity_hf_detail_resolution_patch";

        // Deprecated, use "tile" instead
        public const string HEIGHTFIELD_UNITY_TILE = "unity_hf_tile";
        public const string HEIGHTFIELD_DETAIL_DISTANCE = "unity_hf_detail_distance";
        public const string HEIGHTFIELD_DETAIL_DENSITY = "unity_hf_detail_density";

        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_PREFAB = "unity_hf_detail_prototype_prefab";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_TEXTURE = "unity_hf_detail_prototype_texture";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_BENDFACTOR = "unity_hf_detail_prototype_bendfactor";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_DRYCOLOR = "unity_hf_detail_prototype_drycolor";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_HEALTHYCOLOR = "unity_hf_detail_prototype_healthycolor";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_MAXHEIGHT = "unity_hf_detail_prototype_maxheight";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_MAXWIDTH = "unity_hf_detail_prototype_maxwidth";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_MINHEIGHT = "unity_hf_detail_prototype_minheight";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_MINWIDTH = "unity_hf_detail_prototype_minwidth";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_NOISESPREAD = "unity_hf_detail_prototype_noisespread";
        public const string HEIGHTFIELD_DETAIL_PROTOTYPE_RENDERMODE = "unity_hf_detail_prototype_rendermode";

        public const string HEIGHTFIELD_LAYER_ATTR_TYPE = "unity_hf_layer_type";
        public const string HEIGHTFIELD_LAYER_TYPE_DETAIL = "detail";

        // General Heightfield
        public const string HAPI_HEIGHTFIELD_TILE_ATTR = "tile";
        public const string HAPI_HEIGHTFIELD_LAYERNAME_HEIGHT = "height";
        public const string HAPI_HEIGHTFIELD_LAYERNAME_MASK = "mask";


        // Material Attributes

        // Diffuse
        public const string MAT_OGL_DIFF_ATTR = "ogl_diff";
        public const string MAT_DIFF_ATTR = "basecolor";

        public const string MAT_OGL_TEX1_ATTR = "ogl_tex1";
        public const string MAT_OGL_TEX1_ATTR_ENABLED = "ogl_use_tex1";

        public const string MAT_BASECOLOR_ATTR = "basecolor_texture";
        public const string MAT_BASECOLOR_ATTR_ENABLED = "basecolor_useTexture";

        public const string MAT_MAP_ATTR = "map";

        public const string MAT_OGL_NORMAL_ATTR = "ogl_normalmap";
        public const string MAT_NORMAL_ATTR = "baseNormal_texture";
        public const string MAT_NORMAL_ATTR_ENABLED = "baseBumpAndNormal_enable";

        // Specular
        public const string MAT_OGL_SPEC_ATTR = "ogl_spec";
        public const string MAT_SPEC_ATTR = "reflect";

        public const string MAT_OGL_SPEC_MAP_ATTR = "ogl_specmap";
        public const string MAT_OGL_SPEC_MAP_ATTR_ENABLED = "ogl_use_specmap";
        public const string MAT_SPEC_MAP_ATTR = "reflect_texture";
        public const string MAT_SPEC_MAP_ATTR_ENABLED = "reflect_useTexture";

        // Roughness
        public const string MAT_OGL_ROUGH_ATTR = "ogl_rough";
        public const string MAT_ROUGH_ATTR = "rough";

        public const string MAT_OGL_ROUGH_MAP_ATTR = "ogl_roughmap";
        public const string MAT_OGL_ROUGH_MAP_ATTR_ENABLED = "ogl_use_roughmap";

        public const string MAT_ROUGH_MAP_ATTR = "rough_texture";
        public const string MAT_ROUGH_MAP_ATTR_ENABLED = "rough_useTexture";

        // Metallic
        public const string MAT_OGL_METALLIC_ATTR = "ogl_metallic";
        public const string MAT_METALLIC_ATTR = "metallic";

        public const string MAT_OGL_METALLIC_MAP_ATTR = "ogl_metallicmap";
        public const string MAT_OGL_METALLIC_MAP_ATTR_ENABLED = "ogl_use_metallicmap";

        public const string MAT_METALLIC_MAP_ATTR = "metallic_texture";

        public const string MAT_METALLIC_MAP_ATTR_ENABLED = "metallic_useTexture";

        // Emissive
        public const string MAT_OGL_EMISSIVE_ATTR = "ogl_emit";
        public const string MAT_EMISSIVE_ATTR = "emitcolor";
        public const string MAT_OGL_EMISSIVE_MAP_ATTR = "ogl_emissionmap";
        public const string MAT_OGL_EMISSIVE_MAP_ATTR_ENABLED = "ogl_use_emissionmap";
        public const string MAT_EMISSIVE_MAP_ATTR = "emitcolor_texture";
        public const string MAT_EMISSIVE_MAP_ATTR_ENABLED = "emitcolor_useTexture";

        // Alpha
        public const string MAT_OGL_ALPHA_ATTR = "ogl_alpha";
        public const string MAT_ALPHA_ATTR = "opac";

        public const string MAT_OGL_OPACITY_MAP_ATTR = "ogl_opacitymap";
        public const string MAT_OGL_OPACITY_MAP_ATTR_ENABLED = "ogl_use_opacitymap";

        public const string MAT_OPACITY_MAP_ATTR = "opaccolor_texture";
        public const string MAT_OPACITY_MAP_ATTR_ENABLED = "opaccolor_useTexture";
        public const string MAT_OGL_TRANSPARENCY_ATTR = "ogl_transparency";
        public const string MAT_OGL_TRANSPARENCY_ATTR_ENABLED = "ogl_use_alpha_transparency";


        // Occlusion (No regular occlusion, just maps)
        public const string MAT_OGL_OCCLUSION_MAP_ATTR = "ogl_occlusionmap";
        public const string MAT_OGL_OCCLUSION_MAP_ATTR_ENABLED = "ogl_use_occlusionmap";

        // Curve Parameters
        public const string CURVE_COORDS_PARAM = "coords";
        public const string CURVE_TYPE_PARAM = "type";
        public const string CURVE_METHOD_PARAM = "method";
        public const string CURVE_CLOSE_PARAM = "close";
        public const string CURVE_REVERSE_PARAM = "reverse";

        // Attribute store
        public const string HENGINE_STORE_ATTR = "hengine_attr_store";

        // Unity Attributes
        public const string DEFAULT_UNITY_TAG_ATTR = "unity_tag";
        public const string DEFAULT_UNITY_SCRIPT_ATTR = "unity_script";
        public const string DEFAULT_UNITY_INSTANCE_ATTR = "unity_instance";
        public const string UNITY_USE_INSTANCE_FLAGS_ATTR = "unity_use_instance_flags";
        public const string DEFAULT_UNITY_INPUT_MESH_ATTR = "unity_input_mesh_name";
        public const string DEFAULT_UNITY_STATIC_ATTR = "unity_static";
        public const string DEFAULT_UNITY_LAYER_ATTR = "unity_layer";
        public const string DEFAULT_UNITY_MESH_READABLE = "unity_mesh_readable";

        public const string DEFAULT_INSTANCE_PREFIX_ATTR = "instance_prefix";

        // Unity Shaders
        public const string UNITY_SHADER_COLOR = "_Color";
        public const string UNITY_SHADER_SPEC_COLOR = "_SpecColor";
        public const string UNITY_SHADER_SPEC_MAP = "_SpecMap";
        public const string UNITY_SHADER_METALLIC = "_Metallic";
        public const string UNITY_SHADER_METALLIC_MAP = "_MetallicMap";

        public const string UNITY_SHADER_BUMP_MAP = "_BumpMap";
        public const string UNITY_SHADER_EMISSION_COLOR = "_EmissionColor";
        public const string UNITY_SHADER_EMISSION_MAP = "_EmissionMap";

        public const string UNITY_SHADER_SMOOTHNESS = "_Smoothness";
        public const string UNITY_SHADER_SMOOTHNESS_MAP = "_SmoothnessMap";
        public const string UNITY_SHADER_SHININESS = "_Shininess"; // Deprecated. Use Smoothness instead..
        public const string UNITY_SHADER_OCCLUSION = "_Occlusion";
        public const string UNITY_SHADER_OCCLUSION_MAP = "_OcclusionMap";

        public const string UNITY_SHADER_OPACITY = "_Opacity";
        public const string UNITY_SHADER_OPACITY_MAP = "_OpacityMap";

        // Unity tags
        public const string UNITY_EDITORONLY_TAG = "EditorOnly";
        public const string UNITY_HDADATA_NAME = "HDA_Data";

        public const string HOUDINI_SHADER_PREFIX = "Houdini/";

        public const string DEFAULT_STANDARD_SHADER = "HoudiniStandard";
        public const string DEFAULT_VERTEXCOLOR_SHADER = DEFAULT_STANDARD_SHADER;
        public const string DEFAULT_TRANSPARENT_SHADER = "HoudiniStandardAlpha";
        public const string DEFAULT_STANDARD_SHADER_SPECULAR = "HoudiniSpecular";
        public const string DEFAULT_STANDARD_SHADER_SPECULAR_LEGACY = "Legacy/SpecularVertexColor";
        public const string DEFAULT_VERTEXCOLOR_SHADER_SPECULAR = DEFAULT_STANDARD_SHADER_SPECULAR;
        public const string DEFAULT_VERTEXCOLOR_SHADER_SPECULAR_LEGACY = DEFAULT_STANDARD_SHADER_SPECULAR_LEGACY;
        public const string DEFAULT_TRANSPARENT_SHADER_SPECULAR = "HoudiniSpecularAlpha";
        public const string DEFAULT_TRANSPARENT_SHADER_SPECULAR_LEGACY = "Legacy/AlphaSpecularVertexColor";
        public const string DEFAULT_CURVE_SHADER = "LineShader";
        public const string DEFAULT_TERRAIN_SHADER = "Nature/Terrain/Standard";

        public const string DEFAULT_STANDARD_SHADER_HDRP = "HDRP/StandardLit";
        public const string DEFAULT_VERTEXCOLOR_SHADER_HDRP = DEFAULT_STANDARD_SHADER_HDRP;
        public const string DEFAULT_TRANSPARENT_SHADER_HDRP = "HDRP/StandardLitAlpha";

        public const string DEFAULT_STANDARD_SHADER_HDRP_SPECULAR = "HDRP/SpecularLit";
        public const string DEFAULT_VERTEXCOLOR_SHADER_HDRP_SPECULAR = DEFAULT_STANDARD_SHADER_HDRP_SPECULAR;
        public const string DEFAULT_TRANSPARENT_SHADER_HDRP_SPECULAR = "HDRP/SpecularLitAlpha";

        public const string DEFAULT_CURVE_SHADER_HDRP = "HDRP/Color";
        public const string DEFAULT_TERRAIN_SHADER_HDRP = "HDRP/TerrainLit";

        public const string DEFAULT_STANDARD_SHADER_URP = "URP/StandardLit";
        public const string DEFAULT_VERTEXCOLOR_SHADER_URP = DEFAULT_STANDARD_SHADER_URP;
        public const string DEFAULT_TRANSPARENT_SHADER_URP = "URP/StandardLitAlpha";

        public const string DEFAULT_STANDARD_SHADER_URP_SPECULAR = "URP/SpecularLit";
        public const string DEFAULT_VERTEXCOLOR_SHADER_URP_SPECULAR = DEFAULT_STANDARD_SHADER_URP_SPECULAR;
        public const string DEFAULT_TRANSPARENT_SHADER_URP_SPECULAR = "URP/SpecularLitAlpha";

        public const string DEFAULT_CURVE_SHADER_URP = "URP/Color";
        public const string DEFAULT_TERRAIN_SHADER_URP = "Universal Render Pipeline/Terrain/Lit";

        public const string DEFAULT_UNITY_BUILTIN_RESOURCES = "Resources/unity_builtin_extra";

        // Built-in terrain material paths:
        public const string DEFAULT_TERRAIN_MATERIAL_PATH = "Resources/unity_builtin_extra::name::Default-Terrain-Standard";

        public const string DEFAULT_TERRAIN_MATERIAL_PATH_HDRP =
            "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Material/DefaultHDTerrainMaterial.mat";

        public const string DEFAULT_TERRAIN_MATERIAL_PATH_URP = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/TerrainLit.mat";

        public const string DEFAULT_MATERIAL = "HEU_DEFAULT_MATERIAL";
        public static int DEFAULT_MATERIAL_KEY = DEFAULT_MATERIAL.GetHashCode();
        public const string EDITABLE_MATERIAL = "HEU_EDITABLE_MATERIAL";
        public static int EDITABLE_MATERIAL_KEY = EDITABLE_MATERIAL.GetHashCode();
        public const int HEU_INVALID_MATERIAL = -1;

        // Asset Database Names
        public const string HEU_ASSET_CACHE_PATH = "HoudiniEngineAssetCache";
        public const string HEU_WORKING_PATH = "Working";
        public const string HEU_BAKED_PATH = "Baked";

        // Baked Names
        public const string HEU_BAKED_HDA = "_bakedHDA";
        public const string HEU_BAKED_CLONE = "_bakedClone";

        // Instance Names
        public const string HEU_INSTANCE = "_Instance";
        public const string HEU_INSTANCE_PATTERN = HEU_INSTANCE + "\\d*\\z";
        public const string HEU_INSTANCE_SPLIT_ATTR = "unity_split_attr";

        // Geometry
        public const string HEU_DEFAULT_GEO_GROUP_NAME = "main_geo";

        // LODs
        public const string HEU_DEFAULT_LOD_NAME = "lod";
        public const string HEU_UNITY_LOD_TRANSITION_ATTR = "lod_screensizes";

        // Subasset
        public const string HEU_SUBASSET = "SUBASSET::";

        // HEngine Tools
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
	public const string HEU_HENGINE_TOOLS_SHIPPED_FOLDER = "<HFS>" + HEU_HoudiniVersion.HOUDINI_FRAMEWORKS_PATH + "/Resources/engine/tools";
#else
        public const string HEU_HENGINE_TOOLS_SHIPPED_FOLDER = "<HFS>/engine/tools";
#endif
        public const string HEU_HENGINE_SHIPPED_SHELF = "Default";
        public const string HEU_PATH_KEY_PROJECT = "<PROJECT_PATH>";
        public const string HEU_PATH_KEY_PLUGIN = "<PLUGIN_PATH>";
        public const string HEU_PATH_KEY_HFS = "<HFS>";
        public const string HEU_PATH_KEY_TOOL = "HOUDINI_TOOL_PATH";

        // User Messages
        public const string HEU_USERMSG_NONEDITOR_NOT_SUPPORTED = "Houdini Engine does not support non-Editor asset creation at this time!";

        // Textures
        public const string HEU_TERRAIN_SPLAT_DEFAULT = "Textures/heu_terrain_default_splat";

        // Folder names
        public const string HEU_FOLDER_MESHES = "Meshes";
        public const string HEU_FOLDER_MATERIALS = "Materials";
        public const string HEU_FOLDER_TERRAIN = "Terrain";
        public const string HEU_FOLDER_TILE = "Tile";
        public const string HEU_FOLDER_TEXTURES = "Textures";

        // Extensions
        public const string HEU_EXT_ASSET = ".asset";
        public const string HEU_EXT_MAT = ".mat";
        public const string HEU_EXT_TERRAINDATA = ".terraindata";
        public const string HEU_EXT_TERRAINLAYER = ".terrainlayer";

        // Keys
#if UNITY_STANDALONE_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
	public const string HEU_KEY_CTRL = "Command";
#else
        public const string HEU_KEY_CTRL = "Ctrl";
#endif
    }
} // HoudiniEngineUnity
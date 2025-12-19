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

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX)
#define HOUDINIENGINEUNITY_ENABLED
#endif

using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_PartId = System.Int32;

    public class HEU_SyncedEventData
    {
        public bool CookSuccess;
        public HEU_ThreadedTaskLoadGeo.HEU_LoadData TopNodeData;
        public HEU_BaseSync OutputObject;

        public HEU_SyncedEventData(bool bSuccess, HEU_ThreadedTaskLoadGeo.HEU_LoadData bTopNodeData,
            HEU_BaseSync bOutputObject)
        {
            CookSuccess = bSuccess;
            TopNodeData = bTopNodeData;
            OutputObject = bOutputObject;
        }
    }

    [ExecuteInEditMode] // Needed to get OnDestroy callback when deleted in Editor
    public class HEU_BaseSync : MonoBehaviour
    {
        #region FUNCTIONS

        #region SETUP

        private void Awake()
        {
#if HOUDINIENGINEUNITY_ENABLED
            if (_sessionID != HEU_SessionData.INVALID_SESSION_ID)
            {
                HEU_SessionBase session = HEU_SessionManager.GetSessionWithID(_sessionID);
                if (session == null || !HEU_HAPIUtility.IsNodeValidInHoudini(session, _cookNodeID))
                {
                    // Reset session and node IDs if these don't exist (could be from scene load).
                    _sessionID = HEU_SessionData.INVALID_SESSION_ID;
                    _cookNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
                }
            }
#endif
        }

        private void OnDestroy()
        {
            DeleteSessionData();
        }

        public virtual void DeleteSessionData()
        {
            if (_cookNodeID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                HEU_SessionBase session = GetHoudiniSession(false);
                if (session != null)
                {
                    HAPI_NodeId deleteID = _cookNodeID;

                    if (_deleteParent)
                    {
                        deleteID = GetParentNodeID(session);
                    }

                    session.DeleteNode(deleteID);
                }

                _cookNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            }
        }

        public virtual void DestroyGeneratedData()
        {
            DestroyOutputs();
        }

        protected virtual void Initialize()
        {
            _generateOptions._generateNormals = true;
            _generateOptions._generateTangents = true;
            _generateOptions._generateUVs = false;
            _generateOptions._useLODGroups = true;
            _generateOptions._splitPoints = false;

            _initialized = true;
        }

        #endregion

        #region UTILITY

        public virtual HEU_SessionBase GetHoudiniSession(bool bCreateIfNotFound)
        {
            HEU_SessionBase session = (_sessionID != HEU_SessionData.INVALID_SESSION_ID)
                ? HEU_SessionManager.GetSessionWithID(_sessionID)
                : null;

            if (session == null || !session.IsSessionValid())
            {
                if (bCreateIfNotFound)
                {
                    session = HEU_SessionManager.GetOrCreateDefaultSession();
                    if (session != null && session.IsSessionValid())
                    {
                        _sessionID = session.GetSessionData().SessionID;
                    }
                }
            }

            return session;
        }

        private HAPI_NodeId GetParentNodeID(HEU_SessionBase session)
        {
            HAPI_NodeInfo nodeInfo = new HAPI_NodeInfo();
            return (session.GetNodeInfo(_cookNodeID, ref nodeInfo, false)) ? nodeInfo.parentId : -1;
        }

        public void Log(string msg)
        {
            lock (_log)
            {
                _log.AppendLine(msg);
            }
        }

        public void ClearLog()
        {
            lock (_log)
            {
                _log = new StringBuilder();
            }
        }

        public void Error(string error)
        {
            _error.Append(error);
        }

        public bool IsLoaded()
        {
            return _cookNodeID != HEU_Defines.HEU_INVALID_NODE_ID && _firstSyncComplete;
        }

        #endregion

        #region SYNC

        public virtual void StartSync()
        {
            if (_syncing)
            {
                return;
            }

            if (!_initialized)
            {
                Initialize();
            }

            HEU_SessionBase session = GetHoudiniSession(true);
            if (session == null)
            {
                Log("ERROR: No session found!");
                return;
            }

            Log("Starting sync");
            _syncing = true;
            _sessionID = session.GetSessionData().SessionID;

            SetupLoadTask(session);
        }

        protected virtual void SetupLoadTask(HEU_SessionBase session)
        {
        }

        public virtual void StopSync()
        {
            if (!_syncing)
            {
                return;
            }

            Log("Stopped sync");
            _syncing = false;

            if (_loadTask != null)
            {
                _loadTask.Stop();
            }
        }

        public virtual void Resync()
        {
            if (_syncing)
            {
                return;
            }

            Unload();
            StartSync();
        }

        public virtual void Bake()
        {
            if (_syncing)
            {
                return;
            }

            string outputPath = HEU_AssetDatabase.CreateUniqueBakePath(this.gameObject.name);

            GameObject parentObj = HEU_GeneralUtility.CreateNewGameObject(this.gameObject.name);

            foreach (HEU_GeneratedOutput generatedOutput in _generatedOutputs)
            {
                GameObject obj = HEU_GeneralUtility.CreateNewGameObject(generatedOutput._outputData._gameObject.name);

                generatedOutput.WriteOutputToAssetCache(obj, outputPath, generatedOutput.IsInstancer);

                obj.transform.parent = parentObj.transform;
            }

            string prefabPath = HEU_AssetDatabase.AppendPrefabPath(outputPath, parentObj.name);
            GameObject prefabGO = HEU_EditorUtility.SaveAsPrefabAsset(prefabPath, parentObj);
            if (prefabGO != null)
            {
                HEU_EditorUtility.SelectObject(prefabGO);

                HEU_Logger.LogFormat("Exported prefab to {0}", outputPath);
            }

            GameObject.DestroyImmediate(parentObj);
        }

        public virtual void Unload()
        {
            if (_syncing)
            {
                StopSync();

                if (_loadTask != null)
                {
                    _loadTask.Stop();
                }
            }

            DeleteSessionData();
            DestroyGeneratedData();

            Log("Unloaded!");
        }

        public virtual void Reset()
        {
            if (_syncing)
            {
                StopSync();

                if (_loadTask != null)
                {
                    _loadTask.Stop();
                    _loadTask.Reset();
                }
            }

            DeleteSessionData();
            DestroyGeneratedData();
            _log = new StringBuilder();
        }

        #endregion

        #region CALLBACKS

        public virtual void OnLoadComplete(HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData)
        {
            Log(loadData._logStr.ToString());
            _cookNodeID = loadData._cookNodeID;

            if (loadData._loadStatus == HEU_ThreadedTaskLoadGeo.HEU_LoadData.LoadStatus.SUCCESS)
            {
                DestroyOutputs();

                GenerateObjects(loadData);
            }

            _firstSyncComplete = true;
            _syncing = false;

            bool bSuccess = loadData._loadStatus == HEU_ThreadedTaskLoadGeo.HEU_LoadData.LoadStatus.SUCCESS;
            if (_onSynced != null)
            {
                _onSynced.Invoke(new HEU_SyncedEventData(bSuccess, loadData, this));
            }
        }

        public virtual void GenerateObjects(HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData)
        {
            if (loadData._loadedObjects != null)
            {
                int numObjects = loadData._loadedObjects.Count;
                for (int i = 0; i < numObjects; ++i)
                {
                    GenerateGeometry(loadData, i);
                }
            }
        }

        public virtual void GenerateGeometry(HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData, int objIndex)
        {
            HEU_ThreadedTaskLoadGeo.HEU_LoadObject loadObject = loadData._loadedObjects[objIndex];

            if (loadObject._meshBuffers != null && loadObject._meshBuffers.Count > 0)
            {
                GenerateMesh(loadData._cookNodeID, loadObject._meshBuffers);
            }

            if (loadObject._terrainBuffers != null && loadObject._terrainBuffers.Count > 0)
            {
                GenerateTerrain(loadData._cookNodeID, loadObject._terrainBuffers);
            }

            if (loadObject._instancerBuffers != null && loadObject._instancerBuffers.Count > 0)
            {
                GenerateAllInstancers(loadData._cookNodeID, loadObject._instancerBuffers, loadData);
            }
        }

        public void OnStopped(HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData)
        {
            _syncing = false;

            Log(loadData._logStr.ToString());
            _cookNodeID = loadData._cookNodeID;
            if (_onSynced != null)
            {
                _onSynced.Invoke(new HEU_SyncedEventData(false, null, this));
            }
        }

        #endregion

        #region GENERATE

        private void GenerateTerrain(HAPI_NodeId cookNodeId, List<HEU_LoadBufferVolume> terrainBuffers)
        {
            HEU_SessionBase session = GetHoudiniSession(true);
            Transform parent = this.gameObject.transform;

            // Directory to store generated terrain files.
            string outputTerrainpath = GetOutputCacheDirectory();
            outputTerrainpath = HEU_Platform.BuildPath(outputTerrainpath, "Terrain");

            int numVolumes = terrainBuffers.Count;
            for (int t = 0; t < numVolumes; ++t)
            {
                if (terrainBuffers[t]._heightMap != null)
                {
                    GameObject newGameObject =
                        HEU_GeneralUtility.CreateNewGameObject("heightfield_" + terrainBuffers[t]._tileIndex);

                    HAPI_PartId partId = terrainBuffers[t]._id;

                    Transform newTransform = newGameObject.transform;
                    newTransform.parent = parent;

                    HEU_GeneratedOutput generatedOutput = new HEU_GeneratedOutput();
                    generatedOutput._outputData._gameObject = newGameObject;

                    Terrain terrain = HEU_GeneralUtility.GetOrCreateComponent<Terrain>(newGameObject);

#if !HEU_TERRAIN_COLLIDER_DISABLED
                    TerrainCollider collider = HEU_GeneralUtility.GetOrCreateComponent<TerrainCollider>(newGameObject);
#endif
                    // The TerrainData and TerrainLayer files needs to be saved out if we create them.
                    // Try user specified path, otherwise use the cache folder
                    string exportTerrainDataPath = terrainBuffers[t]._terrainDataExportPath;
                    if (string.IsNullOrEmpty(exportTerrainDataPath))
                    {
                        // This creates the relative folder path from the Asset's cache folder: {assetCache}/{geo name}/Terrain/Tile{tileIndex}/...
                        exportTerrainDataPath = HEU_Platform.BuildPath(outputTerrainpath,
                            HEU_Defines.HEU_FOLDER_TERRAIN, HEU_Defines.HEU_FOLDER_TILE + terrainBuffers[t]._tileIndex);
                    }

                    bool bFullExportTerrainDataPath = HEU_Platform.DoesFileExist(exportTerrainDataPath);

                    if (!string.IsNullOrEmpty(terrainBuffers[t]._terrainDataPath))
                    {
                        // Load the source TerrainData, then make a unique copy of it in the cache folder

                        TerrainData sourceTerrainData =
                            HEU_AssetDatabase.LoadAssetAtPath(terrainBuffers[t]._terrainDataPath, typeof(TerrainData))
                                as TerrainData;
                        if (sourceTerrainData == null)
                        {
                            HEU_Logger.LogWarningFormat("TerrainData, set via attribute, not found at: {0}",
                                terrainBuffers[t]._terrainDataPath);
                        }

                        if (bFullExportTerrainDataPath)
                        {
                            terrain.terrainData =
                                HEU_AssetDatabase.CopyAndLoadAssetAtGivenPath(sourceTerrainData, exportTerrainDataPath,
                                    typeof(TerrainData)) as TerrainData;
                        }
                        else
                        {
                            terrain.terrainData =
                                HEU_AssetDatabase.CopyUniqueAndLoadAssetAtAnyPath(sourceTerrainData,
                                    exportTerrainDataPath, typeof(TerrainData)) as TerrainData;
                        }

                        if (terrain.terrainData != null)
                        {
                            // Store path so that it can be deleted on clean up
                            AddGeneratedOutputFilePath(HEU_AssetDatabase.GetAssetPath(terrain.terrainData));
                        }
                    }

                    if (terrain.terrainData == null)
                    {
                        terrain.terrainData = new TerrainData();

                        if (bFullExportTerrainDataPath)
                        {
                            string folderPath = HEU_Platform.GetFolderPath(exportTerrainDataPath, true);
                            HEU_AssetDatabase.CreatePathWithFolders(folderPath);
                            HEU_AssetDatabase.CreateAsset(terrain.terrainData, exportTerrainDataPath);
                        }
                        else
                        {
                            string assetPathName = "TerrainData" + HEU_Defines.HEU_EXT_ASSET;
                            HEU_AssetDatabase.CreateObjectInAssetCacheFolder(terrain.terrainData, exportTerrainDataPath,
                                null, assetPathName, typeof(TerrainData), true);
                        }
                    }

                    TerrainData terrainData = terrain.terrainData;

#if !HEU_TERRAIN_COLLIDER_DISABLED
                    collider.terrainData = terrainData;
#endif

                    HEU_TerrainUtility.SetTerrainMaterial(terrain, terrainBuffers[t]._specifiedTerrainMaterialName);

#if UNITY_2018_3_OR_NEWER
                    terrain.allowAutoConnect = true;
                    // This has to be set after setting material
                    terrain.drawInstanced = true;
#endif

                    int heightMapSize = terrainBuffers[t]._heightMapWidth;

                    terrainData.heightmapResolution = heightMapSize;
                    if (terrainData.heightmapResolution != heightMapSize)
                    {
                        HEU_Logger.LogErrorFormat(
                            "Unsupported terrain size: {0}. Terrain resolution should be a power of 2 + 1.",
                            heightMapSize);
                        continue;
                    }

                    // The terrainData.baseMapResolution is not set here, but rather left to whatever default Unity uses
                    // The terrainData.alphamapResolution is set later when setting the alphamaps.

                    // 32 is the default for resolutionPerPatch
                    const int detailResolution = 1024;
                    const int resolutionPerPatch = 32;
                    terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);

                    terrainData.SetHeights(0, 0, terrainBuffers[t]._heightMap);

                    // Note that Unity uses a default height range of 600 when a flat terrain is created.
                    // Without a non-zero value for the height range, user isn't able to draw heights.
                    // Therefore, set 600 as the value if height range is currently 0 (due to flat heightfield).
                    float heightRange = terrainBuffers[t]._heightRange;
                    if (heightRange == 0)
                    {
                        heightRange = 600;
                    }

                    terrainData.size = new Vector3(terrainBuffers[t]._terrainSizeX, heightRange,
                        terrainBuffers[t]._terrainSizeY);

                    terrain.Flush();

                    // Set position
                    HAPI_Transform hapiTransformVolume = new HAPI_Transform(true);
                    hapiTransformVolume.position[0] += terrainBuffers[t]._position[0];
                    hapiTransformVolume.position[1] += terrainBuffers[t]._position[1];
                    hapiTransformVolume.position[2] += terrainBuffers[t]._position[2];
                    HEU_HAPIUtility.ApplyLocalTransfromFromHoudiniToUnity(ref hapiTransformVolume, newTransform);

                    // Set layers
                    Texture2D defaultTexture = HEU_VolumeCache.LoadDefaultSplatTexture();
                    int numLayers = terrainBuffers[t]._splatLayers.Count;

#if UNITY_2018_3_OR_NEWER

                    // Create TerrainLayer for each heightfield layer.
                    // Note that height and mask layers are ignored (i.e. not created as TerrainLayers).
                    // Since height layer is first, only process layers from 2nd index onwards.
                    if (numLayers > 1)
                    {
                        // Keep existing TerrainLayers, and either update or append to them
                        TerrainLayer[] existingTerrainLayers = terrainData.terrainLayers;

                        // Total layers are existing layers + new alpha maps
                        List<TerrainLayer> finalTerrainLayers = new List<TerrainLayer>(existingTerrainLayers);

                        for (int m = 1; m < numLayers; ++m)
                        {
                            TerrainLayer terrainlayer = null;

                            int terrainLayerIndex = -1;

                            bool bSetTerrainLayerProperties = true;

                            HEU_LoadBufferVolumeLayer layer = terrainBuffers[t]._splatLayers[m];

                            // Look up TerrainLayer file via attribute if user has set it
                            if (!string.IsNullOrEmpty(layer._layerPath))
                            {
                                terrainlayer =
                                    HEU_AssetDatabase.LoadAssetAtPath(layer._layerPath, typeof(TerrainLayer)) as
                                        TerrainLayer;
                                if (terrainlayer == null)
                                {
                                    HEU_Logger.LogWarningFormat("TerrainLayer, set via attribute, not found at: {0}",
                                        layer._layerPath);
                                    continue;
                                }
                                else
                                {
                                    // Always check if its part of existing list so as not to add it again
                                    terrainLayerIndex =
                                        HEU_TerrainUtility.GetTerrainLayerIndex(terrainlayer, existingTerrainLayers);
                                }
                            }

                            if (terrainlayer == null)
                            {
                                terrainlayer = new TerrainLayer();
                                terrainLayerIndex = finalTerrainLayers.Count;
                                finalTerrainLayers.Add(terrainlayer);
                            }
                            else
                            {
                                // For existing TerrainLayer, make a copy of it if it has custom layer attributes
                                // because we don't want to change the original TerrainLayer.
                                if (layer._hasLayerAttributes)
                                {
                                    // Copy the TerrainLayer file
                                    TerrainLayer prevTerrainLayer = terrainlayer;
                                    terrainlayer = HEU_AssetDatabase.CopyAndLoadAssetAtAnyPath(terrainlayer,
                                        outputTerrainpath, typeof(TerrainLayer), true) as TerrainLayer;
                                    if (terrainlayer != null)
                                    {
                                        if (terrainLayerIndex >= 0)
                                        {
                                            // Update the TerrainLayer reference in the list with this copy
                                            finalTerrainLayers[terrainLayerIndex] = terrainlayer;
                                        }
                                        else
                                        {
                                            // Newly added
                                            terrainLayerIndex = finalTerrainLayers.Count;
                                            finalTerrainLayers.Add(terrainlayer);
                                        }

                                        // Store path for clean up later
                                        AddGeneratedOutputFilePath(HEU_AssetDatabase.GetAssetPath(terrainlayer));
                                    }
                                    else
                                    {
                                        HEU_Logger.LogErrorFormat(
                                            "Unable to copy TerrainLayer '{0}' for generating Terrain. "
                                            + "Using original TerrainLayer. Will not be able to set any TerrainLayer properties.",
                                            layer._layerName);
                                        terrainlayer = prevTerrainLayer;
                                        bSetTerrainLayerProperties = false;
                                        // Again, continuing on to keep proper indexing.
                                    }
                                }
                                else
                                {
                                    // Could be a layer in Assets/ but not part of existing layers in TerrainData
                                    terrainLayerIndex = finalTerrainLayers.Count;
                                    finalTerrainLayers.Add(terrainlayer);
                                    bSetTerrainLayerProperties = false;
                                }
                            }

                            terrainlayer.name = layer._layerName;

                            if (bSetTerrainLayerProperties)
                            {
                                if (!string.IsNullOrEmpty(layer._diffuseTexturePath))
                                {
                                    terrainlayer.diffuseTexture =
                                        HEU_MaterialFactory.LoadTexture(layer._diffuseTexturePath);
                                }

                                if (terrainlayer.diffuseTexture == null)
                                {
                                    terrainlayer.diffuseTexture = defaultTexture;
                                }

                                terrainlayer.diffuseRemapMin = Vector4.zero;
                                terrainlayer.diffuseRemapMax = Vector4.one;

                                if (!string.IsNullOrEmpty(layer._maskTexturePath))
                                {
                                    terrainlayer.maskMapTexture =
                                        HEU_MaterialFactory.LoadTexture(layer._maskTexturePath);
                                }

                                terrainlayer.maskMapRemapMin = Vector4.zero;
                                terrainlayer.maskMapRemapMax = Vector4.one;

                                terrainlayer.metallic = layer._metallic;

                                if (!string.IsNullOrEmpty(layer._normalTexturePath))
                                {
                                    terrainlayer.normalMapTexture =
                                        HEU_MaterialFactory.LoadTexture(layer._normalTexturePath);
                                }

                                terrainlayer.normalScale = layer._normalScale;

                                terrainlayer.smoothness = layer._smoothness;
                                terrainlayer.specular = layer._specularColor;
                                terrainlayer.tileOffset = layer._tileOffset;

                                if (layer._tileSize.magnitude == 0f && terrainlayer.diffuseTexture != null)
                                {
                                    // Use texture size if tile size is 0
                                    layer._tileSize = new Vector2(terrainlayer.diffuseTexture.width,
                                        terrainlayer.diffuseTexture.height);
                                }

                                terrainlayer.tileSize = layer._tileSize;
                            }

                            // In order to retain the new TerrainLayer, it must be saved to the AssetDatabase.
                            string layerFileNameWithExt = terrainlayer.name;
                            if (!layerFileNameWithExt.EndsWith(HEU_Defines.HEU_EXT_TERRAINLAYER))
                            {
                                layerFileNameWithExt += HEU_Defines.HEU_EXT_TERRAINLAYER;
                            }

                            HEU_AssetDatabase.CreateObjectInAssetCacheFolder(terrainlayer, exportTerrainDataPath, null,
                                layerFileNameWithExt, null, true);
                        }

                        terrainData.terrainLayers = finalTerrainLayers.ToArray();
                    }

#else
					// Need to create SplatPrototype for each layer in heightfield, representing the textures.
					SplatPrototype[] splatPrototypes = new SplatPrototype[numLayers];
					for (int m = 0; m < numLayers; ++m)
					{
						splatPrototypes[m] = new SplatPrototype();

						HEU_LoadBufferVolumeLayer layer = terrainBuffers[t]._splatLayers[m];

						Texture2D diffuseTexture = null;
						if (!string.IsNullOrEmpty(layer._diffuseTexturePath))
						{
							diffuseTexture = HEU_MaterialFactory.LoadTexture(layer._diffuseTexturePath);
						}
						if (diffuseTexture == null)
						{
							diffuseTexture = defaultTexture;
						}
						splatPrototypes[m].texture = diffuseTexture;

						splatPrototypes[m].tileOffset = layer._tileOffset;
						if (layer._tileSize.magnitude == 0f && diffuseTexture != null)
						{
							// Use texture size if tile size is 0
							layer._tileSize = new Vector2(diffuseTexture.width, diffuseTexture.height);
						}
						splatPrototypes[m].tileSize = layer._tileSize;

						splatPrototypes[m].metallic = layer._metallic;
						splatPrototypes[m].smoothness = layer._smoothness;

						if (!string.IsNullOrEmpty(layer._normalTexturePath))
						{
							splatPrototypes[m].normalMap = HEU_MaterialFactory.LoadTexture(layer._normalTexturePath);
						}
					}
					terrainData.splatPrototypes = splatPrototypes;
#endif

                    // Set the splatmaps
                    if (terrainBuffers[t]._splatMaps != null)
                    {
                        // Set the alphamap size before setting the alphamaps to get correct scaling
                        // The alphamap size comes from the first alphamap layer
                        int alphamapResolution = terrainBuffers[t]._heightMapWidth;
                        if (numLayers > 1)
                        {
                            alphamapResolution = terrainBuffers[t]._splatLayers[1]._heightMapWidth;
                        }

                        terrainData.alphamapResolution = alphamapResolution;

                        terrainData.SetAlphamaps(0, 0, terrainBuffers[t]._splatMaps);
                    }

                    // Set the tree scattering
                    if (terrainBuffers[t]._scatterTrees != null)
                    {
                        HEU_TerrainUtility.ApplyScatterTrees(terrainData, terrainBuffers[t]._scatterTrees,
                            terrainBuffers[t]._tileIndex);
                    }

                    // Set the detail layers
                    if (terrainBuffers[t]._detailPrototypes != null)
                    {
                        HEU_TerrainUtility.ApplyDetailLayers(terrain, terrainData, terrainBuffers[t]._detailProperties,
                            terrainBuffers[t]._detailPrototypes, terrainBuffers[t]._detailMaps);
                    }

                    terrainBuffers[t]._generatedOutput = generatedOutput;
                    _generatedOutputs.Add(generatedOutput);


                    ApplyAttributeModifiersOnGameObjectOutput(session, cookNodeId, partId, ref newGameObject);
                    SetOutputVisiblity(terrainBuffers[t]);
                }
            }

#if UNITY_2018_3_OR_NEWER
            HEU_AssetDatabase.SaveAndRefreshDatabase();
#endif
        }

        private void GenerateMesh(HAPI_NodeId cookNodeId, List<HEU_LoadBufferMesh> meshBuffers)
        {
            HEU_SessionBase session = GetHoudiniSession(true);

            Transform parent = this.gameObject.transform;

            int numBuffers = meshBuffers.Count;
            for (int m = 0; m < numBuffers; ++m)
            {
                if (meshBuffers[m]._geoCache != null)
                {
                    GameObject newGameObject =
                        HEU_GeneralUtility.CreateNewGameObject("mesh_" + meshBuffers[m]._geoCache._partName);

                    HAPI_PartId partId = meshBuffers[m]._geoCache.PartID;

                    Transform newTransform = newGameObject.transform;
                    newTransform.parent = parent;

                    HEU_GeneratedOutput generatedOutput = new HEU_GeneratedOutput();
                    generatedOutput._outputData._gameObject = newGameObject;

                    bool hasGeo = true;

                    HAPI_GeoInfo geoInfo = new HAPI_GeoInfo();
                    if (!session.GetGeoInfo(cookNodeId, ref geoInfo, false))
                    {
                        hasGeo = false;
                    }

                    bool bResult = false;
                    int numLODs = meshBuffers[m]._LODGroupMeshes != null ? meshBuffers[m]._LODGroupMeshes.Count : 0;
                    if (numLODs > 1)
                    {
                        bResult = HEU_GenerateGeoCache.GenerateLODMeshesFromGeoGroups(session,
                            meshBuffers[m]._LODGroupMeshes,
                            meshBuffers[m]._geoCache, generatedOutput, meshBuffers[m]._defaultMaterialKey,
                            meshBuffers[m]._bGenerateUVs, meshBuffers[m]._bGenerateTangents,
                            meshBuffers[m]._bGenerateNormals, meshBuffers[m]._bPartInstanced);
                    }
                    else if (numLODs == 1)
                    {
                        bResult = HEU_GenerateGeoCache.GenerateMeshFromSingleGroup(session,
                            meshBuffers[m]._LODGroupMeshes[0],
                            meshBuffers[m]._geoCache, generatedOutput, meshBuffers[m]._defaultMaterialKey,
                            meshBuffers[m]._bGenerateUVs, meshBuffers[m]._bGenerateTangents,
                            meshBuffers[m]._bGenerateNormals, meshBuffers[m]._bPartInstanced);

                        if (hasGeo)
                        {
                            HEU_GeneralUtility.UpdateGeneratedAttributeStore(session, _cookNodeID, meshBuffers[m]._id,
                                generatedOutput._outputData._gameObject);
                        }
                    }
                    else
                    {
                        // Set return state to false if no mesh and no colliders (i.e. nothing is generated)
                        bResult = (meshBuffers[m]._geoCache._colliderInfos.Count > 0);
                    }

                    if (bResult)
                    {
                        HEU_GenerateGeoCache.UpdateColliders(meshBuffers[m]._geoCache, generatedOutput._outputData);

                        meshBuffers[m]._generatedOutput = generatedOutput;
                        _generatedOutputs.Add(generatedOutput);

                        SetOutputVisiblity(meshBuffers[m]);

                        if (hasGeo)
                        {
                            ApplyAttributeModifiersOnGameObjectOutput(session, cookNodeId, partId, ref newGameObject);
                        }
                    }
                    else
                    {
                        HEU_GeneratedOutput.DestroyGeneratedOutput(generatedOutput);
                    }
                }
            }
        }

        private void GenerateAllInstancers(HAPI_NodeId cookNodeId, List<HEU_LoadBufferInstancer> instancerBuffers,
            HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData)
        {
            int numBuffers = instancerBuffers.Count;
            for (int m = 0; m < numBuffers; ++m)
            {
                GenerateInstancer(cookNodeId, instancerBuffers[m], loadData._idBuffersMap);
            }
        }

        private void GenerateInstancer(HAPI_NodeId cookNodeId, HEU_LoadBufferInstancer instancerBuffer,
            Dictionary<HAPI_NodeId, HEU_LoadBufferBase> idBuffersMap)
        {
            if (instancerBuffer._generatedOutput != null)
            {
                // Already generated
                return;
            }

            HEU_SessionBase session = GetHoudiniSession(true);

            Transform parent = this.gameObject.transform;

            GameObject instanceRootGO = HEU_GeneralUtility.CreateNewGameObject("instance_" + instancerBuffer._name);

            HAPI_PartId partId = instancerBuffer._id;

            Transform instanceRootTransform = instanceRootGO.transform;
            instanceRootTransform.parent = parent;
            instanceRootTransform.localPosition = Vector3.zero;
            instanceRootTransform.localRotation = Quaternion.identity;
            instanceRootTransform.localScale = Vector3.one;

            instancerBuffer._generatedOutput = new HEU_GeneratedOutput();
            instancerBuffer._generatedOutput._outputData._gameObject = instanceRootGO;

            instancerBuffer._generatedOutput.IsInstancer = true;
            _generatedOutputs.Add(instancerBuffer._generatedOutput);

            if (instancerBuffer._instanceNodeIDs != null && instancerBuffer._instanceNodeIDs.Length > 0)
            {
                GenerateInstancesFromNodeIDs(cookNodeId, instancerBuffer, idBuffersMap, instanceRootTransform);
            }
            else if (instancerBuffer._assetPaths != null && instancerBuffer._assetPaths.Length > 0)
            {
                GenerateInstancesFromAssetPaths(instancerBuffer, instanceRootTransform);
            }

            ApplyAttributeModifiersOnGameObjectOutput(session, cookNodeId, partId, ref instanceRootGO);

            SetOutputVisiblity(instancerBuffer);
        }

        private void GenerateInstancesFromNodeIDs(HAPI_NodeId cookNodeId, HEU_LoadBufferInstancer instancerBuffer,
            Dictionary<HAPI_NodeId, HEU_LoadBufferBase> idBuffersMap,
            Transform instanceRootTransform)
        {
            // For single collision geo override
            GameObject singleCollisionGO = null;

            // For multi collision geo overrides, keep track of loaded objects
            Dictionary<string, GameObject> loadedCollisionObjectMap = new Dictionary<string, GameObject>();

            if (instancerBuffer._collisionAssetPaths != null && instancerBuffer._collisionAssetPaths.Length == 1)
            {
                // Single collision override
                if (!string.IsNullOrEmpty(instancerBuffer._collisionAssetPaths[0]))
                {
                    HEU_AssetDatabase.ImportAsset(instancerBuffer._collisionAssetPaths[0],
                        HEU_AssetDatabase.HEU_ImportAssetOptions.Default);
                    singleCollisionGO =
                        HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._collisionAssetPaths[0], typeof(GameObject))
                            as GameObject;
                }

                if (singleCollisionGO == null)
                {
                    // Continue on but log error
                    HEU_Logger.LogErrorFormat("Collision asset at path {0} not found for instance {1}.",
                        instancerBuffer._collisionAssetPaths[0], instancerBuffer._name);
                }
            }

            int numInstances = instancerBuffer._instanceNodeIDs.Length;
            for (int i = 0; i < numInstances; ++i)
            {
                HEU_LoadBufferBase sourceBuffer = null;
                if (!idBuffersMap.TryGetValue(instancerBuffer._instanceNodeIDs[i], out sourceBuffer) ||
                    sourceBuffer == null)
                {
                    HEU_Logger.LogErrorFormat("Part with id {0} is missing. Unable to setup instancer!",
                        instancerBuffer._instanceNodeIDs[i]);
                    return;
                }

                // If the part we're instancing is itself an instancer, make sure it has generated its instances
                if (sourceBuffer._bInstanced && sourceBuffer._generatedOutput == null)
                {
                    HEU_LoadBufferInstancer sourceBufferInstancer = instancerBuffer as HEU_LoadBufferInstancer;
                    if (sourceBufferInstancer != null)
                    {
                        GenerateInstancer(cookNodeId, sourceBufferInstancer, idBuffersMap);
                    }
                }

                GameObject sourceGameObject = sourceBuffer._generatedOutput._outputData._gameObject;
                if (sourceGameObject == null)
                {
                    HEU_Logger.LogErrorFormat("Output gameobject is null for source {0}. Unable to instance for {1}.",
                        sourceBuffer._name, instancerBuffer._name);
                    continue;
                }

                GameObject collisionSrcGO = null;
                if (singleCollisionGO != null)
                {
                    // Single collision geo
                    collisionSrcGO = singleCollisionGO;
                }
                else if (instancerBuffer._collisionAssetPaths != null
                         && (i < instancerBuffer._collisionAssetPaths.Length)
                         && !string.IsNullOrEmpty(instancerBuffer._collisionAssetPaths[i]))
                {
                    // Mutliple collision geo (one per instance).
                    if (!loadedCollisionObjectMap.TryGetValue(instancerBuffer._collisionAssetPaths[i],
                            out collisionSrcGO))
                    {
                        collisionSrcGO =
                            HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._collisionAssetPaths[i],
                                typeof(GameObject)) as GameObject;
                        if (collisionSrcGO == null)
                        {
                            HEU_Logger.LogErrorFormat("Unable to load collision asset at {0} for instancing!",
                                instancerBuffer._collisionAssetPaths[i]);
                        }
                        else
                        {
                            loadedCollisionObjectMap.Add(instancerBuffer._collisionAssetPaths[i], collisionSrcGO);
                        }
                    }
                }

                int numTransforms = instancerBuffer._instanceTransforms.Length;
                for (int j = 0; j < numTransforms; ++j)
                {
                    CreateNewInstanceFromObject(sourceGameObject, (j + 1), instanceRootTransform,
                        ref instancerBuffer._instanceTransforms[j],
                        instancerBuffer._instancePrefixes, instancerBuffer._name, collisionSrcGO);
                }
            }
        }

        private void GenerateInstancesFromAssetPaths(HEU_LoadBufferInstancer instancerBuffer,
            Transform instanceRootTransform)
        {
            // For single asset, this is set when its imported
            GameObject singleAssetGO = null;

            // For multi assets, keep track of loaded objects so we only need to load once for each object
            Dictionary<string, GameObject> loadedAssetObjectMap = new Dictionary<string, GameObject>();

            // For single collision geo override
            GameObject singleCollisionGO = null;

            // For multi collision geo overrides, keep track of loaded objects
            Dictionary<string, GameObject> loadedCollisionObjectMap = new Dictionary<string, GameObject>();

            // Temporary empty gameobject in case the specified Unity asset is not found
            GameObject tempGO = null;

            if (instancerBuffer._assetPaths.Length == 1)
            {
                // Single asset path
                if (!string.IsNullOrEmpty(instancerBuffer._assetPaths[0]))
                {
                    HEU_AssetDatabase.ImportAsset(instancerBuffer._assetPaths[0],
                        HEU_AssetDatabase.HEU_ImportAssetOptions.Default);
                    singleAssetGO =
                        HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._assetPaths[0], typeof(GameObject)) as
                            GameObject;
                }

                if (singleAssetGO == null)
                {
                    HEU_Logger.LogErrorFormat("Asset at path {0} not found. Unable to create instances for {1}.",
                        instancerBuffer._assetPaths[0], instancerBuffer._name);
                    return;
                }
            }

            if (instancerBuffer._collisionAssetPaths != null && instancerBuffer._collisionAssetPaths.Length == 1)
            {
                // Single collision override
                if (!string.IsNullOrEmpty(instancerBuffer._collisionAssetPaths[0]))
                {
                    HEU_AssetDatabase.ImportAsset(instancerBuffer._collisionAssetPaths[0],
                        HEU_AssetDatabase.HEU_ImportAssetOptions.Default);
                    singleCollisionGO =
                        HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._collisionAssetPaths[0], typeof(GameObject))
                            as GameObject;
                }

                if (singleCollisionGO == null)
                {
                    // Continue on but log error
                    HEU_Logger.LogErrorFormat(
                        "Collision asset at path {0} not found. Unable to create instances for {1}.",
                        instancerBuffer._collisionAssetPaths[0], instancerBuffer._name);
                }
            }

            int numInstancesCreated = 0;
            int numInstances = instancerBuffer._instanceTransforms.Length;
            for (int i = 0; i < numInstances; ++i)
            {
                // Reset to the single asset for each instance allows which is null if using multi asset
                // therefore forcing the instance asset to be found
                GameObject unitySrcGO = singleAssetGO;

                GameObject collisionSrcGO = null;

                if (unitySrcGO == null)
                {
                    // If not using single asset, then there must be an asset path for each instance

                    if (string.IsNullOrEmpty(instancerBuffer._assetPaths[i]))
                    {
                        continue;
                    }

                    if (!loadedAssetObjectMap.TryGetValue(instancerBuffer._assetPaths[i], out unitySrcGO))
                    {
                        // Try loading it
                        unitySrcGO =
                            HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._assetPaths[i], typeof(GameObject)) as
                                GameObject;

                        if (unitySrcGO == null)
                        {
                            HEU_Logger.LogErrorFormat("Unable to load asset at {0} for instancing!",
                                instancerBuffer._assetPaths[i]);

                            // Even though the source Unity object is not found, we should create an object instance info to track it
                            if (tempGO == null)
                            {
                                tempGO = HEU_GeneralUtility.CreateNewGameObject();
                            }

                            unitySrcGO = tempGO;
                        }

                        // Adding to map even if not found so we don't flood the log with the same error message
                        loadedAssetObjectMap.Add(instancerBuffer._assetPaths[i], unitySrcGO);
                    }
                }

                if (singleCollisionGO != null)
                {
                    // Single collision geo
                    collisionSrcGO = singleCollisionGO;
                }
                else if (instancerBuffer._collisionAssetPaths != null
                         && (i < instancerBuffer._collisionAssetPaths.Length)
                         && !string.IsNullOrEmpty(instancerBuffer._collisionAssetPaths[i]))
                {
                    // Mutliple collision geo (one per instance).
                    if (!loadedCollisionObjectMap.TryGetValue(instancerBuffer._collisionAssetPaths[i],
                            out collisionSrcGO))
                    {
                        collisionSrcGO =
                            HEU_AssetDatabase.LoadAssetAtPath(instancerBuffer._collisionAssetPaths[i],
                                typeof(GameObject)) as GameObject;
                        if (collisionSrcGO == null)
                        {
                            HEU_Logger.LogErrorFormat("Unable to load collision asset at {0} for instancing!",
                                instancerBuffer._collisionAssetPaths[i]);
                        }
                        else
                        {
                            loadedCollisionObjectMap.Add(instancerBuffer._collisionAssetPaths[i], collisionSrcGO);
                        }
                    }
                }

                CreateNewInstanceFromObject(unitySrcGO, (numInstancesCreated + 1), instanceRootTransform,
                    ref instancerBuffer._instanceTransforms[i],
                    instancerBuffer._instancePrefixes, instancerBuffer._name, collisionSrcGO);

                numInstancesCreated++;
            }

            if (tempGO != null)
            {
                HEU_GeneralUtility.DestroyImmediate(tempGO, bRegisterUndo: false);
            }
        }

        private void CreateNewInstanceFromObject(GameObject assetSourceGO, int instanceIndex, Transform parentTransform,
            ref HAPI_Transform hapiTransform, string[] instancePrefixes, string instanceName,
            GameObject collisionSourceGO)
        {
            GameObject newInstanceGO = null;

            if (HEU_EditorUtility.IsPrefabAsset(assetSourceGO))
            {
                newInstanceGO = HEU_EditorUtility.InstantiatePrefab(assetSourceGO) as GameObject;
                newInstanceGO.transform.parent = parentTransform;
            }
            else
            {
                newInstanceGO = HEU_EditorUtility.InstantiateGameObject(assetSourceGO, parentTransform, false, false);
            }

            if (collisionSourceGO != null)
            {
                HEU_GeneralUtility.ReplaceColliderMeshFromMeshFilter(newInstanceGO, collisionSourceGO);
            }

            // To get the instance output name, we pass in the instance index. The actual name will be +1 from this.
            HEU_GeneralUtility.RenameGameObject(newInstanceGO,
                HEU_GeometryUtility.GetInstanceOutputName(instanceName, instancePrefixes, instanceIndex));

            HEU_GeneralUtility.CopyFlags(assetSourceGO, newInstanceGO, true);

            Transform instanceTransform = newInstanceGO.transform;
            HEU_HAPIUtility.ApplyLocalTransfromFromHoudiniToUnityForInstance(ref hapiTransform, instanceTransform);

            // When cloning, the instanced part might have been made invisible, so re-enable renderer to have the cloned instance display it.
            HEU_GeneralUtility.SetGameObjectRenderVisiblity(newInstanceGO, true);
            HEU_GeneralUtility.SetGameObjectChildrenRenderVisibility(newInstanceGO, true);
            HEU_GeneralUtility.SetGameObjectColliderState(newInstanceGO, true);
            HEU_GeneralUtility.SetGameObjectChildrenColliderState(newInstanceGO, true);
        }

        private void ApplyAttributeModifiersOnGameObjectOutput(HEU_SessionBase session, HAPI_NodeId geoID,
            HAPI_PartId partId, ref GameObject go)
        {
            HEU_GeneralUtility.AssignUnityTag(session, geoID, partId, go);
            HEU_GeneralUtility.AssignUnityLayer(session, geoID, partId, go);
            HEU_GeneralUtility.MakeStaticIfHasAttribute(session, geoID, partId, go);
        }

        #endregion

        #region OUTPUT

        private void DestroyOutputs()
        {
            if (_generatedOutputs != null)
            {
                for (int i = 0; i < _generatedOutputs.Count; ++i)
                {
                    HEU_GeneratedOutput.DestroyGeneratedOutput(_generatedOutputs[i]);
                    _generatedOutputs[i] = null;
                }

                _generatedOutputs.Clear();
            }

            if (_outputCacheFilePaths != null && _outputCacheFilePaths.Count > 0)
            {
                foreach (string filepath in _outputCacheFilePaths)
                {
                    HEU_AssetDatabase.DeleteAssetAtPath(filepath);
                }

                _outputCacheFilePaths.Clear();
            }
        }

        private void SetOutputVisiblity(HEU_LoadBufferBase buffer)
        {
            bool bVisibility = !buffer._bInstanced;

            if (HEU_GeneratedOutput.HasLODGroup(buffer._generatedOutput))
            {
                foreach (HEU_GeneratedOutputData childOutput in buffer._generatedOutput._childOutputs)
                {
                    HEU_GeneralUtility.SetGameObjectRenderVisiblity(childOutput._gameObject, bVisibility);
                    HEU_GeneralUtility.SetGameObjectColliderState(childOutput._gameObject, bVisibility);
                }
            }
            else
            {
                HEU_GeneralUtility.SetGameObjectRenderVisiblity(buffer._generatedOutput._outputData._gameObject,
                    bVisibility);
                HEU_GeneralUtility.SetGameObjectColliderState(buffer._generatedOutput._outputData._gameObject,
                    bVisibility);
            }
        }

        private string GetOutputCacheDirectory()
        {
            if (string.IsNullOrEmpty(_outputCacheDirectory))
            {
                // Get a unique working folder if none set
                _outputCacheDirectory = HEU_AssetDatabase.CreateAssetCacheFolder(this.name, this.GetHashCode());
            }

            return _outputCacheDirectory;
        }

        public void SetOutputCacheDirectory(string directory)
        {
            _outputCacheDirectory = directory;
        }

        private void AddGeneratedOutputFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path) && !_outputCacheFilePaths.Contains(path))
            {
                _outputCacheFilePaths.Add(path);
            }
        }

        #endregion

        #region UPDATE

        public virtual void SyncUpdate()
        {
        }

        #endregion

        #endregion

        #region DATA

        public HAPI_NodeId _cookNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

        public long _sessionID = HEU_SessionData.INVALID_SESSION_ID;

        public string _nodeName;

        public bool _initialized;

        public bool _syncing;

        public bool _deleteParent;

        public List<HEU_GeneratedOutput> _generatedOutputs = new List<HEU_GeneratedOutput>();

        // Directory to write out generated files
        public string _outputCacheDirectory = "";

        // List of generated file paths, so the files can be cleaned up on dirty
        public List<string> _outputCacheFilePaths = new List<string>();

        public HEU_GenerateOptions _generateOptions = new HEU_GenerateOptions();

        public StringBuilder _log = new StringBuilder();

        public StringBuilder _error = new StringBuilder();

        public bool _sessionSyncAutoCook = true;

        private System.Action<HEU_SyncedEventData> _onSynced;

        public System.Action<HEU_SyncedEventData> OnSynced
        {
            get => _onSynced;
            set => _onSynced = value;
        }

        protected HEU_ThreadedTaskLoadGeo _loadTask;

        protected int _totalCookCount = 0;

        protected bool _firstSyncComplete = false;

        #endregion
    }

    [System.Serializable]
    public struct HEU_GenerateOptions
    {
        public bool _generateUVs;
        public bool _generateTangents;
        public bool _generateNormals;
        public bool _useLODGroups;
        public bool _splitPoints;
    }
} // HoudiniEngineUnity
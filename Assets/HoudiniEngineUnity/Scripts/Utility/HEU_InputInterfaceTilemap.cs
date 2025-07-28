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

// Special thanks to rendereverything for helping to write this input interface.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoudiniEngineUnity
{
    using HAPI_NodeId = System.Int32;

    [System.Serializable]
    public class HEU_InputInterfaceTilemapSettings
    {
        public bool _createGroupsForTiles = true;
        public bool _exportUnusedTiles = true;
        public bool _applyTileColor = true;
        public bool _applyTilemapOrientation = true;
    };

    public class HEU_InputInterfaceTilemap : HEU_InputInterface
    {
        private HEU_InputInterfaceTilemapSettings settings;


#if UNITY_EDITOR
        /// <summary>
        /// Registers this input inteface for Unity Tilemap2D on
        /// the callback after scripts are reloaded in Unity.
        /// </summary>
        [InitializeOnLoadMethod]
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            HEU_InputInterfaceTilemap inputInterface = new HEU_InputInterfaceTilemap();
            HEU_InputUtility.RegisterInputInterface(inputInterface);
        }
#endif

        private HEU_InputInterfaceTilemap() : base(priority: DEFAULT_PRIORITY)
        {
        }

        public void Initialize(HEU_InputInterfaceTilemapSettings settings)
        {
            if (settings == null)
            {
                settings = new HEU_InputInterfaceTilemapSettings();
            }

            this.settings = settings;
        }

        public override bool CreateInputNodeWithDataUpload(HEU_SessionBase session, int connectNodeID,
            GameObject inputObject, out int inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            if (!HEU_HAPIUtility.IsNodeValidInHoudini(session, connectNodeID))
            {
                Debug.LogError("Connection node is invalid.");
                return false;
            }

            HEU_InputDataTilemap inputTilemap = GenerateTilemapDataFromGameObject(inputObject);

            string inputName = null;
            HAPI_NodeId newNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            session.CreateInputNode(out newNodeID, inputName);

            if (newNodeID == HEU_Defines.HEU_INVALID_NODE_ID ||
                !HEU_HAPIUtility.IsNodeValidInHoudini(session, newNodeID))
            {
                Debug.LogError("Failed to create new input node in Houdini session!");
                return false;
            }

            inputNodeID = newNodeID;
            if (!session.CookNode(inputNodeID, false))
            {
                Debug.LogError("New input node failed to cook!");
                return false;
            }

            return UploadData(session, inputNodeID, inputTilemap);
        }

        public override bool IsThisInputObjectSupported(GameObject inputObject)
        {
            if (inputObject != null)
            {
                if (inputObject.GetComponent<Tilemap>() != null)
                    return true;
            }

            return false;
        }

        private bool UploadData(HEU_SessionBase session, HAPI_NodeId inputNodeID, HEU_InputData inputData)
        {
            if (settings == null)
            {
                HEU_Logger.LogError("Tilemap Settings not found!");
                return false;
            }

            HEU_InputDataTilemap inputTilemap = inputData as HEU_InputDataTilemap;
            if (inputTilemap == null)
            {
                HEU_Logger.LogError(
                    "Expected HEU_InputDataTilemap type for inputData, but received unssupported type.");
                return false;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> colors = new List<Vector3>();

            List<string> tileNames = new List<string>();
            List<Vector3> tileSizes = new List<Vector3>();
            List<Vector3Int> tileCoords = new List<Vector3Int>();

            Tilemap tileMap = inputTilemap._tilemap;

            if (!tileMap.gameObject.activeInHierarchy)
            {
                HEU_Logger.LogWarning(
                    "Tilemap inputs must be active in the hierarchy in order to properly send input data");
            }

            Grid gridLayout = tileMap.layoutGrid;

            Matrix4x4 orientation = tileMap.orientationMatrix;
            Vector3 orientationPosition = orientation.DecomposeToPosition();
            Vector3 orientationRotationEuler = orientation.DecomposeToRotation().eulerAngles;
            orientationRotationEuler.y = -orientationRotationEuler.y;
            orientationRotationEuler.z = -orientationRotationEuler.z;
            Quaternion orientationRotation = Quaternion.Euler(orientationRotationEuler);
            Vector3 orientationScale = orientation.DecomposeToScale();

            List<float> pointOrient = new List<float>();
            List<Vector3> pointScale = new List<Vector3>();

            TileBase[] tileArray = tileMap.GetTilesBlock(tileMap.cellBounds);

            int tileCount = 0;
            Vector3 anchorOffset = tileMap.tileAnchor;
            anchorOffset.Scale(gridLayout.cellSize);

            Vector3 pointPos;

            Vector3Int boundsMin = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int boundsMax = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

            foreach (Vector3Int tilePos in tileMap.cellBounds.allPositionsWithin)
            {
                if (tileMap.HasTile(tilePos))
                {
                    boundsMin = Vector3Int.Min(tilePos, boundsMin);
                    boundsMax = Vector3Int.Max(tilePos, boundsMax);
                }
            }

            boundsMax += Vector3Int.one;
            BoundsInt tileMapBounds = new BoundsInt { min = boundsMin, max = boundsMax };

            foreach (Vector3Int tilePos in tileMapBounds.allPositionsWithin)
            {
                if (!settings._exportUnusedTiles && !tileMap.HasTile(tilePos))
                    continue;

                Vector3Int usedTilePos = tilePos;
                //For Houdini (to use Labs Wang Tile tools, we need to reverse point order on the x axis)
                //so we just iterate in reverse order on the x                
                //usedTilePos.x = tileMapBounds.size.x - 1 - tilePos.x + 2 * tileMapBounds.min.x;

                tileCount++;
                pointPos = tileMap.CellToLocal(usedTilePos) + anchorOffset;
                if (settings._applyTilemapOrientation)
                {
                    pointPos += orientationPosition;
                    pointOrient.Add(orientationRotation[0]);
                    pointOrient.Add(orientationRotation[1]);
                    pointOrient.Add(orientationRotation[2]);
                    pointOrient.Add(orientationRotation[3]);

                    pointScale.Add(orientationScale);
                }

                vertices.Add(pointPos);

                if (tileMap.HasTile(usedTilePos))
                {
                    Tile tile = tileMap.GetTile<Tile>(usedTilePos);
                    tileNames.Add(tile.name);
                    if (settings._applyTileColor)
                        colors.Add(new Vector3(tile.color.r, tile.color.g, tile.color.b));
                    tileSizes.Add(new Vector3(tile.sprite.rect.size.x / tile.sprite.pixelsPerUnit,
                        tile.sprite.rect.size.y / tile.sprite.pixelsPerUnit, 0.0f));
                }
                else
                {
                    tileNames.Add("");
                    if (settings._applyTileColor)
                        colors.Add(Vector3.zero);
                    tileSizes.Add(Vector3.zero);
                }

                tileCoords.Add(usedTilePos);
            }

            HAPI_PartInfo partInfo = new HAPI_PartInfo();
            partInfo.faceCount = 0;
            partInfo.vertexCount = 0;
            partInfo.pointCount = tileCount;
            partInfo.pointAttributeCount = 1;
            partInfo.vertexAttributeCount = 0;
            partInfo.primitiveAttributeCount = 0;
            partInfo.detailAttributeCount = 1;

            if (tileSizes.Count > 0)
                partInfo.pointAttributeCount++;

            if (settings._applyTileColor && colors.Count > 0)
                partInfo.pointAttributeCount++;

            if (tileCoords.Count > 0)
                partInfo.pointAttributeCount++;

            if (pointOrient.Count > 0)
                partInfo.pointAttributeCount++;

            if (pointScale.Count > 0)
                partInfo.pointAttributeCount++;

            if (!settings._createGroupsForTiles && tileNames.Count > 0)
                partInfo.pointAttributeCount++;


            HAPI_GeoInfo displayGeoInfo = new HAPI_GeoInfo();
            if (!session.GetDisplayGeoInfo(inputNodeID, ref displayGeoInfo))
            {
                return false;
            }

            HAPI_NodeId displayNodeID = displayGeoInfo.nodeId;
            if (!session.SetPartInfo(displayNodeID, 0, ref partInfo))
            {
                Debug.LogError("Failed to set input part info. ");
                return false;
            }

            if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_POSITION, 3, vertices.ToArray(), ref partInfo, true))
            {
                Debug.LogError("Failed to set point positions.");
                return false;
            }

            if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0, "unity_tile_size", 2,
                    tileSizes.ToArray(), ref partInfo, false))
            {
                Debug.Log("Failed to set tile size attributes. ");
                return false;
            }

            if (settings._applyTileColor)
            {
                if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0,
                        HEU_HAPIConstants.HAPI_ATTRIB_COLOR, 3, colors.ToArray(), ref partInfo, false))
                {
                    Debug.Log("Failed to set tile color attributes. ");
                    return false;
                }
            }


            if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0, "unity_tile_pos", 2,
                    tileCoords.ToArray(), ref partInfo))
            {
                Debug.Log("Failed to set point tile coordinates attributes.");
                return false;
            }

            if (settings._createGroupsForTiles)
            {
                //Get a list of unique tiles used
                TileBase[] usedTiles = new TileBase[tileMap.GetUsedTilesCount()];
                tileMap.GetUsedTilesNonAlloc(usedTiles);

                //Set point groups based on tile type
                int[] pointGroupMembership = new int[tileCount];
                foreach (TileBase tileType in usedTiles)
                {
                    if (!session.AddGroup(displayNodeID, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT, tileType.name))
                        return false;

                    int index = 0;
                    foreach (string tileName in tileNames)
                    {
                        if (tileName.Equals(tileType.name))
                            pointGroupMembership[index] = 1;
                        else
                            pointGroupMembership[index] = 0;
                        index++;
                    }

                    if (!session.SetGroupMembership(displayNodeID, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT,
                            tileType.name, pointGroupMembership, 0, tileCount))
                        return false;
                }
            }
            else
            {
                if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0, "unity_tile_name",
                        tileNames.ToArray(), ref partInfo))
                {
                    Debug.Log("Failed to set point tile name attributes.");
                    return false;
                }
            }

            if (!HEU_InputMeshUtility.SetMeshDetailAttribute(session, displayNodeID, 0, "unity_tile_bounds", 2,
                    tileMapBounds.size, ref partInfo))
            {
                Debug.Log("Failed to set detail tile map bounds attribute.");
                return false;
            }

            if (settings._applyTilemapOrientation)
            {
                if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0,
                        HEU_Defines.HAPI_ATTRIB_ORIENT, 4, pointOrient.ToArray(), ref partInfo))
                {
                    Debug.LogError("Failed to set point rotations.");
                    return false;
                }

                if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0,
                        HEU_Defines.HAPI_ATTRIB_SCALE, 3, pointScale.ToArray(), ref partInfo, false))
                {
                    Debug.LogError("Failed to set point scales.");
                    return false;
                }
            }

            return session.CommitGeo(displayNodeID);
        }

        public class HEU_InputDataTilemap : HEU_InputData
        {
            public Tilemap _tilemap;
            public Transform _transform;
        }

        public HEU_InputDataTilemap GenerateTilemapDataFromGameObject(GameObject inputObject)
        {
            HEU_InputDataTilemap inputTilemap = new HEU_InputDataTilemap();

            Tilemap tileMap = inputObject.GetComponent<Tilemap>();
            if (tileMap != null)
            {
                inputTilemap._tilemap = tileMap;
                inputTilemap._transform = inputObject.transform;
            }

            return inputTilemap;
        }
    }
}
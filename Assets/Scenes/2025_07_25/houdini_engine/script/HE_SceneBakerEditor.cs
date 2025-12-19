// @author : xue
// @created : 2025,07,17,17:07
// @desc:
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using HAPI_NodeId = System.Int32;

namespace xue
{
    [CustomEditor(typeof(HE_SceneBaker))]
    public class HE_SceneBakerEditor : Editor
    {
        private HEU_SessionBase _session;
        private HE_SceneBaker _target;
        private void OnEnable()
        {
            _target = target as HE_SceneBaker;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // if (GUILayout.Button("查找 Renderers"))
            // {
            //     _target.SearchRenderers();
            // }
            if (GUILayout.Button("重置 Session"))
            {
                HEUtils.InitSession(ref _session, reset:true);
            }
            // if (GUILayout.Button("同步场景"))
            // {
            //     CheckSessionValid();
            //     if (!HEU_HAPIUtility.IsNodeValidInHoudini(_session, _target.sceneSopNode))
            //         _target.heScene.SyncScene(_session, out _target.sceneSopNode, out _target.sceneParentNode);
            // }
            if (GUILayout.Button("重新同步场景"))
            {
                _target.SearchRenderers();
                CheckSessionValid();
                ReSyncScene();
                if (!ReCreateHda(cooked:false)) return;
                UpdateHdaParam();
                FetchHdaResult();
            }
            if (GUILayout.Button("update hda param"))
            {
                CheckSessionValid();
                UpdateHdaParam();
                FetchHdaResult();
            }

            if (GUILayout.Button("FetchHdaResult"))
            {
                CheckSessionValid();
                if (_session == null || !_session.IsSessionValid()
                                     || !HEU_HAPIUtility.IsNodeValidInHoudini(_session, _target.hdaNodeId))
                {
                    Debug.LogError("Session OR hda is invalid!");
                }
                else
                {
                    FetchHdaResult();
                }
            }

            if (GUILayout.Button("保存 HIP"))
            {
                CheckSessionValid();
                HEUtils.SaveHipFile(_session, "scene_baker");
            }
        }

        private bool ReCreateHda(bool cooked = true)
        {
            _target._lastParam = null; // 强制后续更新参数
            // 创建hda
            // 删除旧的 HDA
            if (_target.hdaParentId != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                _session.DeleteNode(_target.hdaParentId);
                _target.hdaParentId = HEU_Defines.HEU_INVALID_NODE_ID;
                _target.hdaNodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            }
            var hdaPath = Path.GetFullPath("Assets/HoudiniEngineUnity/HDAs/scene_baker.hda");
            if (!HEU_HAPIUtility.LoadHDAFile(_session, hdaPath, out var assetLibraryId, out string[] assetNames))
            {
                Debug.LogError($"Failed to load asset library from {hdaPath}");
                return false;
            }
            if (!_session.CreateNode(-1, assetNames[0], null, false, out var hdaNodeId))
            {
                Debug.LogError($"Failed to instantiate asset {assetNames[0]}");
                return false;
            }
            if (hdaNodeId == HEU_Defines.HEU_INVALID_NODE_ID ||
                !HEU_HAPIUtility.IsNodeValidInHoudini(_session, hdaNodeId))
            {
                Debug.LogError("HDA Node ID is invalid!");
                return false;
            }
            var hdaNodeInfo = new HAPI_NodeInfo();
            if (!_session.GetNodeInfo(hdaNodeId, ref hdaNodeInfo))
            {
                Debug.LogError($"Failed to get node info for asset {assetNames[0]}");
                return false;
            }
            _target.hdaNodeId = hdaNodeId;
            _target.hdaParentId = hdaNodeInfo.parentId;
            if (!_session.ConnectNodeInput(hdaNodeId, 0, _target.sceneSopNode))
            {
                Debug.LogError($"Failed to connect merge node to asset {assetNames[0]}");
                return false;
            }
            if (cooked)
            {
                if (!HEU_HAPIUtility.CookNodeInHoudini(_session, hdaNodeId, false, assetNames[0]))
                {
                    Debug.LogError($"Failed to cook asset {assetNames[0]}");
                    return false;
                }
            }
            return true;
        }

        private void UpdateHdaParam(bool cooked = true)
        {
            if (_target._lastParam == null || !_target._lastParam.Equals(_target.param))
            {
                CheckSessionValid();
                if (_session == null || !_session.IsSessionValid()
                                     || !HEU_HAPIUtility.IsNodeValidInHoudini(_session, _target.hdaNodeId))
                {
                    Debug.LogError("Session OR hda is invalid!");
                    return;
                }
                HEU_SessionBase session = _session;
                HAPI_NodeId hdaNodeId = _target.hdaNodeId;
                var param = _target.param;
                session.SetParamFloatValue(hdaNodeId, "percentage", 0, param.percentToKeep);
                session.SetParamFloatValue(hdaNodeId, "cuspangle", 0, param.cuspAngle);
                _target._lastParam = _target.param.Copy();
                if (cooked)
                    RecookHdaNode();
            }
        }
        private void RecookHdaNode()
        {
            HEU_SessionBase session = _session;
            HAPI_NodeId hdaNodeId = _target.hdaNodeId;
            var cookOptions = HEU_HAPIUtility.GetDefaultCookOptions(session);
            cookOptions.cookTemplatedGeos = false;
            cookOptions.splitGeosByGroup = false;
            cookOptions.maxVerticesPerPrimitive = -1;
            // session.CookNodeWithOptions(hdaNodeId, cookOptions);
            HEU_HAPIUtility.CookNodeInHoudiniWithOptions(session, hdaNodeId, cookOptions, "cook hda");
        }
        private bool FetchHdaResult()
        {
            HEU_SessionBase session = _session;
            HAPI_NodeId hdaNodeId = _target.hdaNodeId;
            if (hdaNodeId == HEU_Defines.HEU_INVALID_NODE_ID)
            {
                Debug.LogError("HDA Node ID is invalid!");
                return false;
            }
            HAPI_NodeInfo hdaNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(hdaNodeId, ref hdaNodeInfo))
            {
                Debug.LogError("Failed to get HDA node info!");
                return false;
            }
            HAPI_GeoInfo geoInfo = new HAPI_GeoInfo();
            if (!session.GetGeoInfo(hdaNodeId, ref geoInfo))
            {
                Debug.LogError("Failed to get geo info!");
                return false;
            }
            var partCount = geoInfo.partCount;
            if (partCount == 0)
            {
                Debug.LogError("No display geo parts found!");
                return false;
            }
            else if (partCount > 1)
            {
                Debug.Log($"part count {partCount}");
            }
            HAPI_PartInfo partInfo = new HAPI_PartInfo();
            if (!session.GetPartInfo(hdaNodeId, 0, ref partInfo))
            {
                Debug.LogError("Failed to get part info!");
                return false;
            }
            var pointCount = partInfo.pointCount;
            var mesh = new Mesh();
            // mesh.vertices;
            {
                HAPI_AttributeInfo attrInfoP = new HAPI_AttributeInfo();
                attrInfoP.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
                float[] _posAttr = null;
                HEU_GeneralUtility.GetAttribute(session, hdaNodeId, 0, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                    ref attrInfoP, ref _posAttr, session.GetAttributeFloatData);
                var points = new Vector3[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    points[i] = new Vector3(-_posAttr[i * 3], _posAttr[i * 3 + 1], _posAttr[i * 3 + 2]);
                }
                mesh.vertices = points;
            }
            // mesh.normals;
            {
                HAPI_AttributeInfo attrInfoNormal = new HAPI_AttributeInfo();
                attrInfoNormal.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
                float[] _normalAttr = null;
                HEU_GeneralUtility.GetAttribute(session, hdaNodeId, 0, HEU_HAPIConstants.HAPI_ATTRIB_NORMAL,
                    ref attrInfoNormal, ref _normalAttr, session.GetAttributeFloatData);
                var normals = new Vector3[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    normals[i] = new Vector3(-_normalAttr[i * 3], _normalAttr[i * 3 + 1], _normalAttr[i * 3 + 2]);
                }
                mesh.normals = normals;
            }
            // mesh.uv
            {
                HAPI_AttributeInfo attrInfoUV = new HAPI_AttributeInfo();
                attrInfoUV.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
                float[] _uvAttr = null;
                HEU_GeneralUtility.GetAttribute(session, hdaNodeId, 0, HEU_HAPIConstants.HAPI_ATTRIB_UV,
                    ref attrInfoUV, ref _uvAttr, session.GetAttributeFloatData);
                var uvs = new Vector2[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    uvs[i] = new Vector2(_uvAttr[i * 2], _uvAttr[i * 2 + 1]);
                }
                mesh.uv = uvs;
            }
            // mesh.indexces
            {
                var vertexCount = partInfo.vertexCount;
                int[] indexces = new int[ vertexCount ];
                HAPI_AttributeInfo attrInfoPointNum = new HAPI_AttributeInfo();
                attrInfoPointNum.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX;
                _session.GetVertexList(hdaNodeId, 0, indexces, 0, vertexCount);
                mesh.indexFormat = indexces.Length >= 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.SetIndices(indexces, MeshTopology.Triangles, 0);
            }
            mesh.UploadMeshData(markNoLongerReadable:true);
            // 保存mesh到资源
            string assetPath = null;
            if (_target.meshAsset != null)
            {
                assetPath = AssetDatabase.GetAssetPath(_target.meshAsset);
            }
            else
            {
                assetPath = string.IsNullOrWhiteSpace(_target.meshAssetDefaultPath)
                    ? "Assets/Scenes/2025_07_25/houdini_engine/bake_mesh.asset"
                    : _target.meshAssetDefaultPath;
            }
            var meshName = Path.GetFileNameWithoutExtension(assetPath);
            mesh.name = meshName;
            var asset = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            var needFixImportNewAsset = false;
            string orgAssetObjName = asset != null ? asset.name : null;
            if (asset != null)
            {
                // EditorUtility.CopySerialized(mesh, asset); // 有bug，改成重新 CreateAsset 再走系统文件替换
                var tmpPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                AssetDatabase.CreateAsset(mesh, tmpPath);
                // 系统文件api替换
                File.Delete(assetPath);
                File.Move(tmpPath, assetPath);
                File.Delete(tmpPath);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
            }
            AssetDatabase.Refresh();
            // ping 一下
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var newAsset = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (orgAssetObjName != null && orgAssetObjName != newAsset.name)
            {
                newAsset.name = orgAssetObjName;
                AssetDatabase.SaveAssetIfDirty(newAsset);
            }

            // EditorGUIUtility.PingObject(newAsset);
            _target.meshAsset = newAsset;
            return true;
        }

        private void ReSyncScene()
        {
            if (HEU_HAPIUtility.IsNodeValidInHoudini(_session, _target.sceneSopNode))
            {
                _session.DeleteNode(_target.sceneSopNode);
            }
            if (HEU_HAPIUtility.IsNodeValidInHoudini(_session, _target.sceneParentNode))
            {
                _session.DeleteNode(_target.sceneParentNode);
            }
            _target.sceneSopNode = HEU_Defines.HEU_INVALID_NODE_ID;
            _target.sceneParentNode = HEU_Defines.HEU_INVALID_NODE_ID;
            _target.ReAddMeshToHeScene();
            _target.heScene.SyncScene(_session, out _target.sceneSopNode, out _target.sceneParentNode);
        }

        public void CheckSessionValid()
        {
            HEUtils.InitSession(ref _session, reset:false);
            _target.CheckSessionChange(_session);
        }
    }
}
#endif
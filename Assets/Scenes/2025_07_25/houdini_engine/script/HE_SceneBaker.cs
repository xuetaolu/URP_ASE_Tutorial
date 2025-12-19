// @author : xue
// @created : 2025,07,17,17:07
// @desc:
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;
using HAPI_NodeId = System.Int32;

namespace xue
{
    public class HE_SceneBaker : MonoBehaviour
    {
        [Serializable]
        public class HdaParam
        {
            [Range(1, 100)] public float percentToKeep = 3;
            [Range(0, 180)] public float cuspAngle = 60;

            public override bool Equals(object obj)
            {
                if (obj is HdaParam other)
                {
                    return Mathf.Approximately(percentToKeep, other.percentToKeep) &&
                           Mathf.Approximately(cuspAngle, other.cuspAngle);
                }

                return false;
            }
            public HdaParam Copy()
            {
                return new HdaParam()
                {
                    percentToKeep = percentToKeep,
                    cuspAngle = cuspAngle
                };
            }
        }
        public class HE_MeshInfo : HE_MeshInfoBase
        {
            [SyncToHoudini] public string gameObjectName;
            [SyncToHoudini] public Vector3 worldPosition;
            [SyncToHoudini] public bool isFloor;
        }
        public HE_Scene heScene;
        public long sessionID;
        public HAPI_NodeId sceneSopNode;
        public HAPI_NodeId sceneParentNode;
        public HAPI_NodeId hdaNodeId;
        public HAPI_NodeId hdaParentId;
        public GameObject rendererRoot;
        public List<GameObject> searchedRenderers;
        public HdaParam param;
        [NonSerialized] public HdaParam _lastParam;
        public Mesh meshAsset;
        public string meshAssetDefaultPath = "Assets/Scenes/2025_07_25/houdini_engine/bake_mesh.asset";

        public void CheckSessionChange(HEU_SessionBase session)
        {
            if (session == null || session.IsSessionValid() == false)
            {
                Debug.LogError("session is null or invalid");
                return;
            }
            if (session.GetSessionData().SessionID != sessionID)
            {
                ResetOnSessionChange(session);
            }
        }

        public void ResetOnSessionChange(HEU_SessionBase session)
        {
            sessionID = session.GetSessionData().SessionID;
            sceneSopNode = HEU_Defines.HEU_INVALID_NODE_ID;
            sceneParentNode = HEU_Defines.HEU_INVALID_NODE_ID;
            hdaNodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            hdaParentId = HEU_Defines.HEU_INVALID_NODE_ID;
            heScene.ResetOnSessionChange(session);
        }
        public void SearchRenderers()
        {
            if (rendererRoot == null)
            {
                // 弹窗
                EditorUtility.DisplayDialog("提示", "未设置 rendererRoot，请设置", "OK");
                return;
            }
            var processedMeshFilters = new HashSet<MeshRenderer>(); // 处理过的 MeshFilter
            var toAddMeshRenderers = new List<MeshRenderer>();
            var roots = new List<GameObject> { rendererRoot };
            foreach (var root in roots)
            {
                var lodGroups = root.GetComponentsInChildren<LODGroup>();
                foreach (var lodGroup in lodGroups)
                {
                    var lods = lodGroup.GetLODs();
                    var index = 0;
                    foreach (var lod in lods)
                    {
                        if (lod.renderers == null)
                            continue;
                        foreach (var renderer in lod.renderers)
                        {
                            if (renderer == null)
                                continue;
                            var meshRenderer = renderer as MeshRenderer;
                            if (meshRenderer != null)
                            {
                                if (index == 0)
                                {
                                    toAddMeshRenderers.Add(meshRenderer);
                                }
                                processedMeshFilters.Add(meshRenderer);
                            }
                        }
                        index++;
                    }
                }
            }
            foreach (var root in roots)
            {
                var meshRenderers = root.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    if (processedMeshFilters.Contains(meshRenderer))
                        continue;
                    toAddMeshRenderers.Add(meshRenderer);
                }
            }
            var matchGOs = new List<GameObject>();
            uint matchLayerMark = 0xFFFFFFFF;
            foreach (var mr in toAddMeshRenderers)
            {
                if (((1 << mr.gameObject.layer) & matchLayerMark) == 0)
                    continue;
                var mf = mr.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    matchGOs.Add(mf.gameObject);
                }
            }
            // 序列化到组件
            searchedRenderers.Clear();
            searchedRenderers.AddRange(matchGOs);
            ReAddMeshToHeScene();
        }

        public void ReAddMeshToHeScene()
        {
            heScene.ClearAllMesh();
            foreach (var go in searchedRenderers)
            {
                var gameObjectName = go.name;
                var worldPosition = go.transform.position;
                var isFloor = false;
                var mr = go.GetComponent<MeshRenderer>();
                var mat = mr != null ? mr.sharedMaterial : null;
                if (mat != null && mat.shader != null && mat.shader.name.Equals("xue/he_floor", StringComparison.OrdinalIgnoreCase))
                {
                    isFloor = true;
                }
                HE_MeshInfoBase meshInfoBase = new HE_MeshInfo()
                {
                    mesh = go.GetComponent<MeshFilter>().sharedMesh,
                    transform = go.transform,
                    layer = go.layer,
                    gameObjectName = gameObjectName,
                    worldPosition = worldPosition,
                    isFloor = isFloor,
                };
                heScene.AddMesh(meshInfoBase);
            }
        }
    }
}
#endif
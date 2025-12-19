// @author : xue
// @created : 2025,07,17,17:07
// @desc:
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;
using HAPI_NodeId = System.Int32;
namespace xue
{
    [Serializable]
    public struct HE_MeshSopInfo
    {
        /// <summary>
        /// 一般是 Mesh Asset 资源
        /// </summary>
        public Mesh mesh;
        /// <summary>
        /// houdini 中 Input GEO Object Node 里面的 Null Node ID
        /// </summary>
        public HAPI_NodeId meshNodeID;
        /// <summary>
        /// 所在 geometry 节点的名称，用于作为路径用于 Object Merge Node 等节点
        /// </summary>
        public string geoNodeName;
    }

    public class SyncToHoudiniAttribute : Attribute
    { }

    class SyncInfo
    {
        public string name;
        public Type type;
        public int tupleSize;
        public List<object> values;
        public static bool IsSupportedType(Type t)
        {
            if (t == typeof(int) || t == typeof(float) || t == typeof(string) || t == typeof(bool) 
                || t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4)
                )
                return true;
            return false;
        }
        public SyncInfo(string name, Type type)
        {
            this.name = name;
            this.type = type;
            this.tupleSize = GetTypeTupleSize(type);
            values = new List<object>();
        }
        private int GetTypeTupleSize(Type type)
        {
            if (type == typeof(Vector2))
                return 2;
            else if (type == typeof(Vector3))
                return 3;
            else if (type == typeof(Vector4))
                return 4;
            else
                return 1;
        }
        public void AddValue(object value)
        {
            if (tupleSize == 1)
            {
                if (this.type == typeof(bool))
                {
                    bool v = (bool)value;
                    values.Add(v ? 1 : 0);
                }
                else
                {
                    values.Add(value);
                }
            }
            else
            {
                if (this.type == typeof(Vector2))
                { 
                    var v = (Vector2)value;
                    values.Add(v.x);
                    values.Add(v.y);
                }
                else if (this.type == typeof(Vector3))
                {
                    var v = (Vector3)value;
                    values.Add(v.x);
                    values.Add(v.y);
                    values.Add(v.z);
                }
                else if (this.type == typeof(Vector4))
                {
                    var v = (Vector4)value;
                    values.Add(v.x);
                    values.Add(v.y);
                    values.Add(v.z);
                    values.Add(v.w);
                }
                else
                {
                    Debug.LogError("Unsupported type: " + this.type);
                }
            }
        }
        public bool IsStringType()
        {
            return this.type == typeof(string);
        }
        public bool IsIntType()
        {
            return (this.type == typeof(int)
                    || this.type == typeof(uint)
                    || this.type == typeof(long)
                    || this.type == typeof(ulong)
                    || this.type == typeof(short)
                    || this.type == typeof(ushort)
                    || this.type == typeof(byte)
                    || this.type == typeof(sbyte)
                    || this.type == typeof(char)
                    || this.type == typeof(bool));
        }
        public int[] ToIntArray()
        {
            return this.values.Select(Convert.ToInt32).ToArray();
        }
        public float[] ToFloatArray()
        {
            return this.values.Select(Convert.ToSingle).ToArray();
        }
        public string[] ToStringArray()
        {
            return this.values.Select(x => x == null ? "" : x.ToString()).ToArray();
        }
    }

    [Serializable]
    public class HE_MeshInfoBase
    {
        public Mesh mesh;
        public Transform transform;
        [SyncToHoudini]
        public int layer;
        public virtual bool IsValid()
        {
            return mesh != null && transform != null;
        }
    }
    // 
    public class PointInfo
    {
        public Vector3 position;
        public Quaternion orient;
        public Vector3 scale;
        public int _piece_id_;
        public HE_MeshInfoBase meshInfo;
        // public static readonly string s_ATTRIBUTE_APPLY_GROUP_STRING;
        // public static readonly int s_POINT_ATTR_COUNT;
        public PointInfo (Transform transform, int piece_id, [NotNull] HE_MeshInfoBase meshInfo)
        { 
            this.position = transform.position;
            this.orient = transform.rotation;
            this.scale = transform.lossyScale;
            this._piece_id_ = piece_id;
            this.meshInfo = meshInfo;
        }
        public Quaternion houdini_orient
        {
            get
            {
                var eular = orient.eulerAngles;
                return Quaternion.Euler(eular.x, -eular.y, -eular.z);
            }
        }
        public float[] houdini_orient_floats
        {
            get
            {
                var houdni_orient = houdini_orient;
                return new []{ houdni_orient.x, houdni_orient.y, houdni_orient.z, houdni_orient.w };
            }
        }
    }

    [Serializable]
    public class HE_Scene
    {
        public long sessionID;
        [NonSerialized] public List<HE_MeshInfoBase> meshes = new List<HE_MeshInfoBase>();
        public List<HE_MeshSopInfo> meshSopInfos = new List<HE_MeshSopInfo>();
        private static List<HE_MeshInfoBase> s_validMeshInfos = new List<HE_MeshInfoBase>();
        private static Dictionary<Mesh, HE_MeshSopInfo> s_meshToSopInfo = new Dictionary<Mesh, HE_MeshSopInfo>();
        public void ClearAllMesh()
        {
            meshes.Clear();
        }
        public void AddMesh(HE_MeshInfoBase mesh)
        {
            meshes.Add(mesh);
        }
        public void SyncScene(HEU_SessionBase session, out HAPI_NodeId outSopNodeId, out HAPI_NodeId outParentId)
        {
            outSopNodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            outParentId = HEU_Defines.HEU_INVALID_NODE_ID;
            if (session == null || session.IsSessionValid() == false)
            {
                Debug.LogError("session is null or invalid");
                return;
            }
            if (session.GetSessionData().SessionID != sessionID)
            {
                ResetOnSessionChange(session);
            }
            // a. 准备要同步的合法 HE_MeshInfo
            var validMeshInfos = s_validMeshInfos;
            validMeshInfos.Clear();
            {
                foreach (var m in meshes)
                {
                    if (m != null && m.IsValid())
                        validMeshInfos.Add(m);
                }
            }
            // b. 先上传复用的 Mesh 资源，确保 validMeshInfos 的 mesh 均能在 meshToSopInfo 找到对应的 sopInfo
            var meshToSopInfo = s_meshToSopInfo;
            meshToSopInfo.Clear();
            {
                // 每个 mesh 只需要同一个 sopInfo
                var meshSet = new HashSet<Mesh>();
                foreach (var m in validMeshInfos)
                {
                    meshSet.Add(m.mesh);
                }
                // 旧的 sopInfo 看看有没有可以接着用的
                for (int i = meshSopInfos.Count - 1; i >= 0; i--)
                {
                    var sopInfo = meshSopInfos[i];
                    if (meshSet.Contains(sopInfo.mesh))
                    {
                        // 可能可以复用，但要检查是否合法
                        if (HEU_HAPIUtility.IsNodeValidInHoudini(session, sopInfo.meshNodeID))
                        {
                            meshToSopInfo.Add(sopInfo.mesh, sopInfo);
                        }
                        else // 不合法，删除
                        {
                            meshSopInfos.RemoveAt(i);
                        }
                    }
                }
                // meshToSopInfo 还缺少的 mesh 的 sopInfo 补充
                foreach (var m in meshSet)
                {
                    if (meshToSopInfo.ContainsKey(m) == false)
                    {
                        var uploadResult = UploadMesh(session, m, out var sopNodeId, out var geometryNodeId);
                        if (!uploadResult) // 发射错误，直接退出
                            return;
                        session.GetNodePath(geometryNodeId, HEU_Defines.HEU_INVALID_NODE_ID, out var path);
                        // path 都是 /obj/xxxxxx 的格式
                        var geoNodeName = path.Split('/').Last();
                        var sopInfo = new HE_MeshSopInfo
                        {
                            mesh = m,
                            meshNodeID = sopNodeId,
                            geoNodeName = geoNodeName,
                        };
                        meshToSopInfo.Add(m, sopInfo);
                        meshSopInfos.Add(sopInfo);
                    }
                }
            }
            // c. validMeshInfos 的 mesh 均能在 meshToSopInfo 找到对应的 sopInfo
            //   将每个公共 mesh 通过 copy to point 创建全部实例
            {
                // c.1. 全部实例信息记录到 points 中，用于后续一口气 copy to points
                var piece_id = 0;                           // piece_id 用于表示具体 point 用源集合哪个模型
                var pointInfos = new List<PointInfo>();     // 用于 copy to points 的 points
                var pieceGeoNameList = new List<string>();  // 用于 copy to points 的源模型集合
                {
                    var groupedMeshInfos = validMeshInfos
                        .GroupBy(m => m.mesh)
                        .ToDictionary(
                            group => group.Key,     // The key of the dictionary is the mesh name
                            group => group.ToList() // The value is a List of all HE_MeshInfo in that group
                        );
                    foreach (var group in groupedMeshInfos)
                    { 
                        var sopInfo = meshToSopInfo[group.Key];
                        var meshGeoNodeName = sopInfo.geoNodeName;
                        pieceGeoNameList.Add(meshGeoNodeName);
                        foreach (var meshInfo in group.Value)
                        {
                            pointInfos.Add(new PointInfo(meshInfo.transform, piece_id, meshInfo));
                        }
                        piece_id++;
                    }
                }
                // c.2. 创建源集合 object_merge 节点
                HAPI_NodeId objectMergeId = HEU_Defines.HEU_INVALID_NODE_ID;
                HAPI_NodeId objectMergeParentId = HEU_Defines.HEU_INVALID_NODE_ID;
                {
                    if (!session.CreateNode(-1, "SOP/object_merge", "MERGE_SCENE", false, out objectMergeId))
                    {
                        Debug.LogError("Failed to create object_merge node!");
                        return;
                    }
                    HAPI_NodeInfo objectMergeNodeInfo = new HAPI_NodeInfo();
                    if (!session.GetNodeInfo(objectMergeId, ref objectMergeNodeInfo))
                    {
                        Debug.LogError("Failed to get merge node info!");
                    }
                    objectMergeParentId = objectMergeNodeInfo.parentId;
                    session.SetNodeDisplay(objectMergeParentId, 0);
                    //    
                    {
                        session.SetParamIntValue(objectMergeId, "numobj", 0, pieceGeoNameList.Count);
                        for (int i = 0; i < pieceGeoNameList.Count; i++)
                        {
                            var geoName = pieceGeoNameList[i];
                            session.SetParamStringValue(objectMergeId, $"objpath{i+1}", $"../../{geoName}", 0);
                        }
                        session.SetParamIntValue(objectMergeId, "pack", 0, 1);
                        session.SetParamIntValue(objectMergeId, "pivot", 0, 1);
                    }
                }
                // c.3. 创建 points node
                HAPI_NodeId pointsId = HEU_Defines.HEU_INVALID_NODE_ID;
                var hasSyncExtraFieldNames = new List<string>();
                {
                    // 收集额外的属性
                    var extraFields = new HashSet<FieldInfo>();
                    {
                        var hasCheckMeshInfoType = new HashSet<Type>();
                        foreach (var pointInfo in pointInfos)
                        {
                            var meshInfoType = pointInfo.meshInfo.GetType();
                            if (hasCheckMeshInfoType.Add(meshInfoType))
                            {
                                var fields = meshInfoType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                // 获取有没有 SyncToHoudiniAttribute
                                foreach (var field in fields)
                                {
                                    if (field.GetCustomAttribute<SyncToHoudiniAttribute>() != null)
                                    {
                                        extraFields.Add(field);
                                    }
                                }
                            }
                        }
                    }
                    var extraSyncInfos = new List<SyncInfo>();
                    {
                        foreach (var extraField in extraFields)
                        {
                            if (!SyncInfo.IsSupportedType(extraField.FieldType))
                            {
                                Debug.LogError("SyncInfo: " + extraField.Name + " " + extraField.FieldType + " not supported");
                                continue;
                            }
                            var syncInfo = new SyncInfo(extraField.Name, extraField.FieldType);
                            var allCanAccess = true;
                            foreach (var pointInfo in pointInfos)
                            {
                                try
                                {
                                    object value = extraField.GetValue(pointInfo.meshInfo);
                                    syncInfo.AddValue(value);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e);
                                    allCanAccess = false;
                                    break;
                                }
                            }
                            if (allCanAccess)
                            {
                                extraSyncInfos.Add(syncInfo);
                            }
                            else
                            {
                                Debug.LogError($"Some of the extra sync infos cannot be accessed. skip PointInfo field: {extraField.FieldType.Name}");
                            }
                        }
                    }
                    // 开始创建 points
                    session.CreateNode(objectMergeParentId, "null", "points", false, out  pointsId);
                    HAPI_PartInfo partInfo = new HAPI_PartInfo();
                    partInfo.faceCount = 0;
                    partInfo.vertexCount = 0;
                    partInfo.pointCount = pointInfos.Count;
                    partInfo.pointAttributeCount = 4 + extraSyncInfos.Count;
                    partInfo.vertexAttributeCount = 0;
                    partInfo.primitiveAttributeCount = 0;
                    partInfo.detailAttributeCount = 0;
                    session.SetPartInfo(pointsId, 0, ref partInfo);
                    HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                        HEU_HAPIConstants.HAPI_ATTRIB_POSITION, 3, pointInfos.Select(p => p.position).ToArray(),
                        ref partInfo, true);
                    // To flip a quaternion around the x-axis, you need to negate the x, y, and z components of the quaternion.
                    // The w component remains unchanged.
                    HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                        "orient", 4, pointInfos.SelectMany(p => p.houdini_orient_floats).ToArray(),
                        ref partInfo);
                    HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                        HEU_HAPIConstants.HAPI_ATTRIB_SCALE, 3, pointInfos.Select(p => p.scale).ToArray(),
                        ref partInfo, false);
                    HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                        nameof(PointInfo._piece_id_), 1, pointInfos.Select(p => p._piece_id_).ToArray(),
                        ref partInfo);
                    for (int i = 0; i < extraSyncInfos.Count; i++)
                    {
                        var extraSyncInfo = extraSyncInfos[i];
                        if (extraSyncInfo.IsStringType())
                        {
                            var array = extraSyncInfo.ToStringArray();
                            if (array == null || array.Length != pointInfos.Count)
                            {
                                var arraySize = array == null ? "null" : array.Length.ToString();
                                Debug.LogError("Invalid string array length for extra sync info " + extraSyncInfo.name
                                    + $"target is {pointInfos.Count} but give {arraySize}");
                                continue;
                            }
                            HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                                extraSyncInfo.name, array,
                                ref partInfo);
                            hasSyncExtraFieldNames.Add(extraSyncInfo.name);
                        }
                        else if (extraSyncInfo.IsIntType())
                        {
                            var array = extraSyncInfo.ToIntArray();
                            if (array == null || array.Length != pointInfos.Count)
                            {
                                var arraySize = array == null ? "null" : array.Length.ToString();
                                Debug.LogError("Invalid int array length for extra sync info " + extraSyncInfo.name 
                                    + $"target is {pointInfos.Count} but give {arraySize}");
                                continue;
                            }
                            HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                                extraSyncInfo.name, extraSyncInfo.tupleSize, array,
                                ref partInfo);
                            hasSyncExtraFieldNames.Add(extraSyncInfo.name);
                        }
                        else
                        {
                            var array = extraSyncInfo.ToFloatArray();
                            var tupleSize = extraSyncInfo.tupleSize;
                            if (array == null || array.Length != pointInfos.Count * tupleSize)
                            {
                                var arraySize = array == null ? "null" : array.Length.ToString();
                                Debug.LogError("Invalid float array length for extra sync info " + extraSyncInfo.name
                                    + $"target is {pointInfos.Count} but give {arraySize}");
                                continue;
                            }
                            HEU_InputMeshUtility.SetMeshPointAttribute(session, pointsId, 0,
                                extraSyncInfos[i].name, extraSyncInfo.tupleSize, array,
                                ref partInfo);
                            hasSyncExtraFieldNames.Add(extraSyncInfo.name);
                        }
                    }
                    session.CommitGeo(pointsId);
                }
                // c.4. 创建 copy to points 节点
                {
                    session.CreateNode(objectMergeParentId, "copytopoints", null, false, out var copytopointsId);
                    session.SetParamIntValue(copytopointsId, "targetattribs", 0, 1); 
                    session.SetParamIntValue(copytopointsId, "applyto1", 0, 2); // prim
                    session.SetParamStringValue(copytopointsId, "applyattribs1", string.Join(" ",hasSyncExtraFieldNames), 0); 
                    session.SetParamIntValue(copytopointsId, "useidattrib", 0, 1);
                    session.SetParamStringValue(copytopointsId, "idattrib", nameof(PointInfo._piece_id_), 0);
                    session.ConnectNodeInput(copytopointsId, 0, objectMergeId);
                    session.ConnectNodeInput(copytopointsId, 1, pointsId);
                    session.SetNodeDisplay(copytopointsId, 1);
                    // 写入返回的 node id
                    outSopNodeId = copytopointsId;
                    outParentId = objectMergeParentId;
                }
            }
        }
        private bool UploadMesh(HEU_SessionBase session, Mesh mesh, out HAPI_NodeId sopNodeId, out HAPI_NodeId geometryNodeId)
        {
            sopNodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            geometryNodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            if (!session.CreateInputNode(out var inputNodeID, mesh.name))
            {
                Debug.LogError("Create input node failed");
                return false;
            }
            HAPI_NodeInfo sopNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(inputNodeID, ref sopNodeInfo))
            {
                return false;
            }
            geometryNodeId = sopNodeInfo.parentId;
            session.SetNodeDisplay(geometryNodeId, 0);
            // Upload
            HAPI_PartInfo partInfo = new HAPI_PartInfo();
            partInfo.faceCount = mesh.triangles.Length / 3; // 三角形个数 -> houdini 面个数
            partInfo.vertexCount = mesh.triangles.Length; // 顶点索引个数 -> houdini 顶点个数
            partInfo.pointCount = mesh.vertices.Length; // 顶点位置个数 -> houdini 点个数
            partInfo.pointAttributeCount = 1; // 点属性个数，只有一个 v@P
            partInfo.vertexAttributeCount = 0;
            partInfo.primitiveAttributeCount = 0;
            partInfo.detailAttributeCount = 0;
            
            // 如果有顶点颜色，houdini 对应顶点也有颜色
            bool hasVertexColor = mesh.colors != null && mesh.colors.Length == mesh.vertices.Length;
            if (hasVertexColor)
            {
                partInfo.vertexAttributeCount++;
            }
            // uv
            bool hasUV = mesh.uv != null && mesh.uv.Length == mesh.vertices.Length;
            if (hasUV)
            {
                partInfo.vertexAttributeCount++;
            }
            
            // 更新 null sop 的 partInfo，修改点，顶点，面的个数
            if (!session.SetPartInfo(inputNodeID, 0, ref partInfo))
            {
                HEU_Logger.LogError("Failed to set input part info. ");
                return false;
            }
            // 每个面都是三角形
            int[] faceCounts = new int[partInfo.faceCount];
            for (int i = 0; i < partInfo.faceCount; ++i)
            {
                faceCounts[i] = 3; // numVertsPerFace; 每个面都是三角形
            }
            if (!HEU_GeneralUtility.SetArray2Arg(inputNodeID, 0, session.SetFaceCount, faceCounts, 0, partInfo.faceCount))
            {
                HEU_Logger.LogError("Failed to set input geometry face counts.");
                return false;
            }
            // houdini 中vertex的point索引和 unity 顶点索引一致
            int[] faceIndices = mesh.triangles;
            if (!HEU_GeneralUtility.SetArray2Arg(inputNodeID, 0, session.SetVertexList, faceIndices, 0, partInfo.vertexCount))
            {
                HEU_Logger.LogError("Failed to set input geometry indices.");
                return false;
            }
            // houdini 的point位置是unity的顶点位置
            Vector3[] vertices = mesh.vertices;
            if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, inputNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_POSITION, 3, vertices, ref partInfo, true))
            {
                HEU_Logger.LogError("Failed to set input geometry position.");
                return false;
            }

            if (hasVertexColor)
            {
                var colors = mesh.colors.Select(c => new Vector3(c.r, c.g, c.b)).ToArray();
                if (!HEU_InputMeshUtility.SetMeshVertexAttribute(session, inputNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_COLOR, 3, colors, faceIndices, ref partInfo, false))
                {
                    HEU_Logger.LogError("Failed to set input geometry color.");
                    return false;
                }
            }

            if (hasUV)
            {
                var uvs = mesh.uv.Select(uv => new Vector3(uv.x, uv.y, 0)).ToArray();
                if (!HEU_InputMeshUtility.SetMeshVertexAttribute(session, inputNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_UV, 3, uvs, faceIndices, ref partInfo, false))
                {
                    HEU_Logger.LogError("Failed to set input geometry uv.");
                    return false;
                }
            }


            // 确认更新 null sop 信息
            if (!session.CommitGeo(inputNodeID))
            {
                HEU_Logger.LogError("Failed to commit input geometry.");
                return false;
            }
            
            sopNodeId = inputNodeID;
            return true;
        }
        public void ResetOnSessionChange(HEU_SessionBase session)
        {
            sessionID = session.GetSessionData().SessionID;
            meshSopInfos.Clear();
        }
    }
}
#endif
// @author : xue
// @created : 2024,07,11,17:07
// @desc:
#if UNITY_EDITOR

using System.Collections.Generic;
using Autodesk.Fbx;
using UnityEditor;
using UnityEngine;

namespace flower_scene.fbx
{
    
    /// <summary>
    /// 具体将 Unity 数据覆盖到 fbx 的实现
    /// 注1：保留了原始 fbx 的层次结构和旋转
    /// 注2：fbx 的 mesh 全部替换，完全三角化，且只导出顶点位置、法线、uv、uv2、color
    /// 注3：保持材质、贴图等信息，但不导出 submesh ，不导出 蒙皮信息(若有)， mesh 会变成一个且是原本的第 0 号位材质
    /// </summary>
    public class ReExportFbxProcess
    {
        public class MeshDataHelper
        {
            public Mesh mesh;
            public FbxMesh fbxMesh;
            public FbxNode fbxNode;

            public MeshDataHelper(Mesh mesh)
            {
                this.mesh = mesh;
            }
        }
        
        private int _traverseFbxMeshCount;

        public string fbxPath;

        public Dictionary<string, MeshDataHelper> unityMeshDict = new Dictionary<string, MeshDataHelper>();
        public ReExportFbxProcess(string fbxPath)
        {
            this.fbxPath = fbxPath;
        }

        public bool Process()
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

            if (modelImporter == null)
                return false;

            if (!modelImporter.generateSecondaryUV)
            {
                Debug.Log($"跳过，没有开启 generateSecondaryUV, {fbxPath}");
                return false;
            }
            
            float unityImportPositionScale = 1.0f;
            
            if (modelImporter.useFileScale)
            {
                unityImportPositionScale = modelImporter.fileScale; // 1 unit(cm) in 3d软件 to 0.01 unit(m) in unity
            }

            // 遍历全部导入的 Mesh
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

            foreach (Object asset in assets)
            {
                if (asset is Mesh)
                {
                    Mesh mesh = asset as Mesh;
                    // 判断是否蒙皮
                    bool isSkinnendMesh = mesh.boneWeights is { Length: > 0 };
                    if (isSkinnendMesh)
                    {
                        Debug.Log($"跳过，其中 mesh {mesh.name} 是蒙皮模型, {fbxPath}");
                        return false;
                    }

                    MeshDataHelper meshDataHelper = new MeshDataHelper(mesh);
                    unityMeshDict.Add(mesh.name, meshDataHelper);
                }
            }

            bool modifyUV2Success = false;

            // 遍历 fbx 文件本身  
            using (var manager = FbxManager.Create())
            {
                FbxIOSettings ios = FbxIOSettings.Create(manager, Globals.IOSROOT);
                manager.SetIOSettings(ios);
                
                FbxScene scene = null;
                
                // Create an importer.
                using (var importer = FbxImporter.Create(manager, "MyImporter"))
                {
                    // Specify the path to the FBX file
                    string filePath = fbxPath;

                    // Initialize the importer.
                    if (importer.Initialize(filePath, -1, manager.GetIOSettings()))
                    {
                        // Create a new scene so that it can be populated by the imported file.
                        scene = FbxScene.Create(manager, "MyScene");

                        // Import the contents of the file into the scene.
                        if (importer.Import(scene))
                        {
                            // The file is imported; now process the scene.
                            TraverseScene(scene);
                            
                            if (unityMeshDict.Count != _traverseFbxMeshCount)
                                Debug.LogError(
                                    $"unity mesh count: {unityMeshDict.Count}, traverse fbx mesh count: {_traverseFbxMeshCount}");
                            
                            modifyUV2Success = ProcessDirectUseUnityMesh(unityImportPositionScale);

                        }
                        else
                        {
                            Debug.LogError("Failed to import file: " + filePath);
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to initialize the importer for file: " + filePath);
                    }
                }

                if (modifyUV2Success)
                {
                    // 保存新fbx，名字追加后缀 _modify
                    int lastDotIndex = fbxPath.LastIndexOf('.');
                    // string newFbxPath = fbxPath.Insert(lastDotIndex, "_modify");
                    string newFbxPath = fbxPath; // 如果要覆盖现有的 fbx 则取消注释这一行
                    using (var exporter = FbxExporter.Create(manager, "MyExporter"))
                    {
                        if (exporter.Initialize(newFbxPath, -1, manager.GetIOSettings()))
                        {
                            Debug.Log("Exporting file: " + newFbxPath);
                            exporter.Export(scene);
                        }
                    }
                }
            }

            if (modifyUV2Success)
                modelImporter.generateSecondaryUV = false;

            return modifyUV2Success;
        }

        private int[] s_insertedVertexIndexs = { 0, 2, 1 }; // 因为 unity 导入会做 X 轴反转，三角形顺序也会反转

        private bool ProcessDirectUseUnityMesh(float unityImportPositionScale)
        {
            foreach (var kv in unityMeshDict)
            {
                MeshDataHelper meshDataHelper = kv.Value;
                Mesh mesh = meshDataHelper.mesh;
                FbxMesh fbxMesh = meshDataHelper.fbxMesh;
                FbxNode fbxNode = meshDataHelper.fbxNode;

                if (fbxMesh == null)
                {
                    Debug.LogError($"fbx mesh is null, mesh name: {mesh.name}, fbx path: {fbxPath}");
                    continue;
                }

                var fbxMaterial0 = fbxNode.GetMaterial(0);
                
                GetLocalTransform(fbxNode, out Matrix4x4 transformUnity, out Matrix4x4 rotationUnity3X3);
                
                Matrix4x4 transformUnityInverse = transformUnity.inverse;
                
                Matrix4x4 rotationInvert3x3 = rotationUnity3X3.inverse; // 用于旋转方向

                FbxMesh originalMesh = fbxMesh;
                FbxNode parentNode = fbxNode;
                {
                    // Step 1: 创建一个新的 FbxMesh 实例
                    FbxMesh clonedMesh = FbxMesh.Create(parentNode, originalMesh.GetName());

                    // Control Point
                    int controlPointsCount = mesh.vertices.Length;
                    clonedMesh.InitControlPoints(controlPointsCount);
                    for (int i = 0; i < controlPointsCount; i++)
                    {
                        Vector3 vertex = mesh.vertices[i];

                        // 对于顶点位置，
                        // 1. 先逆 unityImportPositionScale 缩放回去 
                        // 2. 需要 x 取反，
                        // 3. 然后乘以 unity 读取 fbxNode 几何信息变换矩阵的 逆
                        vertex /= unityImportPositionScale;
                        vertex.x *= -1;
                        Vector3 fbxPoint = transformUnityInverse * new Vector4(vertex.x, vertex.y, vertex.z, 1.0f); // 注意需要用 Vector4 左乘 4x4 矩阵
                        FbxVector4 controlPoint = ToFbxVector4(fbxPoint);
                        clonedMesh.SetControlPointAt(controlPoint, i);
                    }

                    // Polygon
                    int polygonCount = mesh.triangles.Length / 3;
                    for (int poly = 0; poly < polygonCount; poly++)
                    {
                        clonedMesh.BeginPolygon(-1, -1, -1, false);
                        // 因为 unity 导入会做 X 轴反转，三角形顺序也会反转
                        foreach (int i in s_insertedVertexIndexs) // 0, 2, 1
                        {
                            clonedMesh.AddPolygon(mesh.triangles[poly * 3 + i]);
                        }

                        clonedMesh.EndPolygon();
                    }

                    // color normal uv 等属性
                    // 注：color normal uv 需要在第一层 layer，只有 uv2 才能在第二层 layer
                    {
                        FbxLayer firstLayer = null;
                        
                        // 补充材质
                        if (fbxMaterial0 != null)
                        {
                            clonedMesh.CreateLayer();
                            firstLayer = clonedMesh.GetLayer(clonedMesh.GetLayerCount() - 1);
                            var elementMaterial = FbxLayerElementMaterial.Create(fbxMesh, fbxNode.GetName());
                            elementMaterial.SetMappingMode(FbxLayerElement.EMappingMode.eAllSame);
                            elementMaterial.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                            FbxLayerElementArrayTemplateFbxSurfaceMaterial directArray = elementMaterial.GetDirectArray();
                            directArray.SetCount(1); // 需要用 eAllSame 模式好确保一个 mesh 就一个材质，避免出现 No Name 材质
                            directArray.SetAt(0, 0);
                            
                            firstLayer.SetMaterials(elementMaterial);
                        }
                        
                        {
                            Color[] colors = mesh.colors;
                            if (colors != null && colors.Length > 0)
                            {
                                FbxLayer clonedLayer = firstLayer;
                                if (clonedLayer == null)
                                {
                                    clonedMesh.CreateLayer(); 
                                    clonedLayer = clonedMesh.GetLayer(clonedMesh.GetLayerCount() - 1);
                                    firstLayer = clonedLayer;
                                }
                                FbxLayerElementVertexColor layerElement =
                                    FbxLayerElementVertexColor.Create(clonedMesh, "color");
                                layerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                                layerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                                FbxLayerElementArrayTemplateFbxColor directArray = layerElement.GetDirectArray();
                                directArray.SetCount(colors.Length);
                                for (int i = 0; i < colors.Length; i++)
                                {
                                    Color color = colors[i];
                                    FbxColor fbxColor = new FbxColor(color.r, color.g, color.b, color.a);
                                    directArray.SetAt(i, fbxColor);
                                }

                                clonedLayer.SetVertexColors(layerElement);
                            }
                        }

                        {
                            Vector3[] normals = mesh.normals;
                            if (normals != null && normals.Length > 0)
                            {
                                FbxLayer clonedLayer = firstLayer;
                                if (clonedLayer == null)
                                {
                                    clonedMesh.CreateLayer(); 
                                    clonedLayer = clonedMesh.GetLayer(clonedMesh.GetLayerCount() - 1);
                                    firstLayer = clonedLayer;
                                }

                                
                                FbxLayerElementNormal layerElement = FbxLayerElementNormal.Create(clonedMesh, "normal");
                                layerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                                layerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                                FbxLayerElementArrayTemplateFbxVector4 directArray = layerElement.GetDirectArray();
                                directArray.SetCount(normals.Length);
                                for (int i = 0; i < normals.Length; i++)
                                {
                                    Vector3 normal = normals[i];
                                    normal.x *= -1;
                                    Vector3 fbxNormal = rotationInvert3x3 * new Vector4(normal.x, normal.y, normal.z, 0);
                                    fbxNormal = fbxNormal.normalized * normal.magnitude;
                                    FbxVector4 fbxVector4 = ToFbxVector4(fbxNormal);
                                    directArray.SetAt(i, fbxVector4);
                                }

                                clonedLayer.SetNormals(layerElement);
                            }
                        }

                        // 暂时不支持导出 tangents，没调研 tangents.w 的定义和 fbx 中 tangent bitangent 的关系

                        {
                            Vector2[] uvs = mesh.uv;
                            if (uvs != null && uvs.Length > 0)
                            {
                                FbxLayer clonedLayer = firstLayer;
                                if (clonedLayer == null)
                                {
                                    clonedMesh.CreateLayer(); 
                                    clonedLayer = clonedMesh.GetLayer(clonedMesh.GetLayerCount() - 1);
                                    firstLayer = clonedLayer;
                                }
                                FbxLayerElementUV layerElement = FbxLayerElementUV.Create(clonedMesh, "uv");
                                layerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                                layerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                                FbxLayerElementArrayTemplateFbxVector2 directArray = layerElement.GetDirectArray();
                                directArray.SetCount(uvs.Length);
                                for (int i = 0; i < uvs.Length; i++)
                                {
                                    Vector2 uv = uvs[i];
                                    FbxVector2 fbxVector2 = new FbxVector2(uv.x, uv.y);
                                    directArray.SetAt(i, fbxVector2);
                                }

                                clonedLayer.SetUVs(layerElement);
                            }
                        }
                        {
                            Vector2[] uv2s = mesh.uv2;
                            if (uv2s != null && uv2s.Length > 0)
                            {
                                clonedMesh.CreateLayer();
                                FbxLayer clonedLayer = clonedMesh.GetLayer(clonedMesh.GetLayerCount() - 1);
                                FbxLayerElementUV layerElement = FbxLayerElementUV.Create(clonedMesh, "uv2");
                                layerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                                layerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                                FbxLayerElementArrayTemplateFbxVector2 directArray = layerElement.GetDirectArray();
                                directArray.SetCount(uv2s.Length);
                                for (int i = 0; i < uv2s.Length; i++)
                                {
                                    Vector2 uv = uv2s[i];
                                    FbxVector2 fbxVector2 = new FbxVector2(uv.x, uv.y);
                                    directArray.SetAt(i, fbxVector2);
                                }

                                clonedLayer.SetUVs(layerElement);
                            }
                        }
                    }
                    
                    // 保存新的 mesh
                    fbxNode.SetNodeAttribute(clonedMesh);
                }
            }

            return true;
        }

        private FbxVector4 ToFbxVector4(Vector3 v)
        {
            return new FbxVector4(v.x, v.y, v.z, 1.0);
        }

        /// <summary>
        /// 获取 3dsmax 中 mesh 本地的转换矩阵
        /// </summary>
        /// <param name="fbxNode"></param>
        /// <returns></returns>
        private void GetLocalTransform(FbxNode fbxNode, out Matrix4x4 TRS, out Matrix4x4 rotation3x3)
        {
            Vector3 translation = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;
            bool posFound, rotFound, sclFound;
            posFound = rotFound = sclFound = false;
            var property = fbxNode.GetFirstProperty();
            while (property.IsValid())
            {
                switch (property.GetName())
                {
                    case "GeometricTranslation":
                        if (!posFound)
                        {
                            translation = ToUnityVector3(property.GetFbxDouble3());
                            posFound = true;
                        }

                        break;
                    case "GeometricRotation":
                        if (!rotFound)
                        {
                            FbxDouble3 fbxDouble3 = property.GetFbxDouble3();
                            // rotation = Quaternion.Euler(ToUnityVector3(property.GetFbxDouble3()));
                            // eEulerXYZ 始终表示“绕 X 旋转，然后绕 Y，然后绕 Z”旋转
                            rotation = Quaternion.Euler(0, 0, (float)fbxDouble3.Z);
                            rotation *= Quaternion.Euler(0, (float)fbxDouble3.Y, 0);
                            rotation *= Quaternion.Euler((float)fbxDouble3.X, 0, 0);
                            rotFound = true;
                        }

                        break;
                    case "GeometricScaling":
                        if (!sclFound)
                        {
                            scale = ToUnityVector3(property.GetFbxDouble3());
                            sclFound = true;
                        }

                        break;
                    default:
                        break;
                }
                
                if (posFound && rotFound && sclFound)
                    break;

                property = fbxNode.GetNextProperty(property);
            }

            TRS = Matrix4x4.TRS(translation, rotation, scale);
            rotation3x3 = Matrix4x4.Rotate(rotation);
        }

        private Vector3 ToUnityVector3(FbxDouble3 double3)
        {
            return new Vector3((float)double3.X, (float)double3.Y, (float)double3.Z);
        }

        private void FindFbxMesh(FbxNode node)
        {
            FbxMesh fbxMesh = node.GetMesh();
            if (fbxMesh == null)
                return;

            _traverseFbxMeshCount++;

            string meshName = node.GetName();

            if (unityMeshDict.TryGetValue(meshName, out var meshDataHelper))
            {
                meshDataHelper.fbxNode = node;
                meshDataHelper.fbxMesh = fbxMesh;
            }
            else
            {
                Debug.LogError($"fbx mesh name: {meshName} not found in unity mesh dict, fbxFile: {fbxPath}");
            }
        }

        private void TraverseNode(FbxNode node)
        {
            // 遍历全部模型
            for (int i = 0; i < node.GetChildCount(); i++)
            {
                var child = node.GetChild(i);
                FindFbxMesh(child);
                TraverseNode(child);
            }
        }

        private void TraverseScene(FbxScene scene)
        {
            _traverseFbxMeshCount = 0;
            var root = scene.GetRootNode();

            TraverseNode(root);
        }
    }
}

#endif
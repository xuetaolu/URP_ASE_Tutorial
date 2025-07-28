// @author : xue
// @created : 2024,04,12,14:47
// @desc:

using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace xue.dynamic_cloud
{
    /// <summary>
    /// 维护的单个云信息
    /// </summary>
    public class Cloud
    {
        // 云基础大小是 2000*1000，
        public static readonly Vector3[] s_DUMMY_MESH_VERTEX = new []
        {
            new Vector3(-1000f, 0f, 0f), // bottom left
            new Vector3(-1000f,  1000f, 0f), // top left
            new Vector3( 1000f, 0f, 0f), // bottom right
            new Vector3( 1000f,  1000f, 0f), // top right
        };
        
        public static readonly Vector2[] s_DUMMY_MESH_UV = new []
        {
            new Vector2(0, 0), // bottom left
            new Vector2(0, 1), // top left
            new Vector2( 1, 0), // bottom right
            new Vector2( 1, 1), // top right
        };
        
        public static readonly int[] s_DUMMY_MESH_TRIANGLES = new []
        {
            0, 1, 2,
            2, 1, 3
        };

        public static readonly int s_MAX_CLOUD_GRID_INDEX = 7;
        
        /// <summary>
        /// 可见阈值 0.4 ~ 0.6
        /// </summary>
        public static readonly Vector2[] s_DUMMY_MESH_UV3 = Enumerable.Repeat(new Vector2(0.1f, 0.9f), 4).ToArray();

        private Vector3 _position;
        private Quaternion _rotation = Quaternion.identity;
        private Vector3 _scale;
        private bool _vertexDirty = true;

        public Vector3 position
        {
            get => _position;
            set
            {
                if (value.Equals(_position))
                    return;
                _position = value;
                _vertexDirty = true;
            }
        }
        
        public Quaternion rotation
        {
            get => _rotation;
            set
            {
                if (value.Equals(_rotation))
                    return;
                _rotation = value;
                _vertexDirty = true;
            }
        }
        
        public Vector3 scale
        {
            get => _scale;
            set
            {
                if (value.Equals(_scale))
                    return;
                _scale = value;
                _vertexDirty = true;
            }
        }


        public static int s_VERTEX_COUNT = 4;
        /// <summary>
        /// r = 1 无效
        /// g = 1 / 7 * random01 表示 0 1 2 3 4 5 6 7 共 8 个云片的随机一个
        /// b/a 原神是一样，表示云边缘羽化的宽度 默认是 0.09804
        /// </summary>
        public Color[] color = new Color[s_VERTEX_COUNT];
        
        /// <summary>
        /// 顶点位置
        /// </summary>
        public Vector3[] vertices = new Vector3[s_VERTEX_COUNT];
        public Vector2[] uv = new Vector2[s_VERTEX_COUNT];
        
        /// <summary>
        /// 动画时间参数，
        /// x 总动画时间
        /// y 当前播放时间
        /// y / x 在 0~1 之间
        /// </summary>
        public Vector2[] uv2 = new Vector2[s_VERTEX_COUNT];
        
        /// <summary>
        /// 动画淡入淡出参数 在 0~1 之间，默认 0.4  0.6 表示 0~0.4 淡入，0.6~1 淡出
        /// </summary>
        public Vector2[] uv3 = new Vector2[s_VERTEX_COUNT];

        /// <summary>
        /// 云片图集
        /// </summary>
        public int cloudGridIndex = 0;


        /// <summary>
        /// 当前的动画时间
        /// </summary>
        public float animTime = 0;
        
        /// <summary>
        /// 动画时长
        /// </summary>
        public float animDuration = 1;

        /// <summary>
        /// 动画时长中，头尾渐变的时长
        /// </summary>
        public float animFadeDuration = 0.4f;
        
        // /// <summary>
        // /// 游戏 1s 播多少时间
        // /// </summary>
        // public float animSpeed = 1f;
        
        /// <summary>
        /// 边缘羽化动画曲线
        /// </summary>
        public AnimationCurve edgeFeatherCurve;

        /// <summary>
        /// 云边缘羽化的宽度 0~1
        /// </summary>
        public float edgeFeather = 0.09804f;

        /// <summary>
        /// 初始生成需要用到的是否高位云
        /// </summary>
        public bool isTop;
        
        /// <summary>
        /// 初始生成需要用到的水平角
        /// </summary>
        public float fai;

        public Cloud()
        {
            // Init Static Vertex Data
            for (int i = 0; i < s_VERTEX_COUNT; i++)
            {
                Array.Copy(s_DUMMY_MESH_UV, uv, s_VERTEX_COUNT);
                // Array.Copy(s_DUMMY_MESH_UV3, uv3, s_VERTEX_COUNT);
            }

            UpdateDynamicVertexData();
        }
        
        public void UpdateState(float deltaTime)
        {
            animTime = Mathf.Clamp(animTime + deltaTime, 0, animDuration);
            float animTime01 = Mathf.Clamp01(animTime / Mathf.Max(animDuration, 0.00001f));
            if (edgeFeatherCurve != null)
            {
                edgeFeather = Mathf.Clamp01(edgeFeatherCurve.Evaluate(Mathf.Clamp01(animTime01)));
            }
        }
        
        public void UpdateDynamicVertexData()
        {
            for (int i = 0; i < s_VERTEX_COUNT; i++)
            {
                UpdateVertices();
                CalcColor(ref color[i], i);
                CalcAnimParamXY(ref uv2[i], i);
                CalcAnimFadeParam(ref uv3[i], i);
            }
        }

        /// <summary>
        /// 计算动画淡入淡出参数
        /// </summary>
        /// <param name="vector2"></param>
        /// <param name="i"></param>
        private void CalcAnimFadeParam(ref Vector2 vector2, int i)
        {
            float _animDuration = Mathf.Max(animDuration, 0.00001f);
            vector2.x = Mathf.Clamp(animFadeDuration / _animDuration, 0, 0.5f);
            vector2.y = 1 - vector2.x;
        }

        /// <summary>
        /// 计算顶点位置
        /// </summary>
        private void UpdateVertices()
        {
            if (!_vertexDirty)
                return;

            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            for (int i = 0; i < s_VERTEX_COUNT; i++)
            {
                ref Vector3 vector3 = ref s_DUMMY_MESH_VERTEX[i];
                Vector4 vector4 = new Vector4(vector3.x, vector3.y, vector3.z, 1);

                vertices[i] = matrix * vector4;
            }

            _vertexDirty = false;
        }
        
        
        /// <summary>
        /// r = 1 无效
        /// g = 1 / 7 * random01 表示 0 1 2 3 4 5 6 7 共 8 个云片的随机一个
        /// b/a 原神是一样，表示云边缘羽化的宽度 默认是 0.09804
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void CalcColor(ref Color color, int index)
        {
            color.r = 1; // 没用
            color.g = cloudGridIndex / 7f;
            color.b = CalcEdgeFactor(index);
            color.a = color.b;
        }
        
        
        
        /// <summary>
        /// 计算云边缘羽化的宽度
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float CalcEdgeFactor(int i)
        {
            return edgeFeather;
        }
        

        /// <summary>
        /// 计算动画参数
        /// x 总动画时长
        /// y 当前播放时间
        /// </summary>
        /// <param name="i"></param>
        /// <returns> y / x 在 0~1 之间</returns>
        private void CalcAnimParamXY(ref Vector2 uv, int i)
        {
            uv.x = animDuration;
            uv.y = animTime;
        }
    }

    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public class GenshinDynamicCloud : MonoBehaviour
    {
        
        public List<Cloud> clouds = new List<Cloud>();
        private bool _clouds_inited;

        [Header("底部云片数量")]
        [Min(0)]
        public int baseCloudCount = 6;

        [Header("底部云片高度")]
        [Min(0)]
        public float baseCloudHeight = 500;

        [Header("底部云片距离")]
        [Min(0)]
        public float baseCloudDistance = 5000;
        
        [Header("是否追加顶部云片")]
        public bool appendTopCount = true;

        [Header("云片随机缩放大小范围最小值")]
        [Range(0, 2)]
        public float minScale = 1.05f;
        
        [Header("云片随机缩放大小范围最大值")]
        [Range(0, 2)]
        public float maxScale = 1.75f;
        
        [Header("底部云片方位角随机角度范围")]
        public float baseFaiNoise = 18;
        
        [Header("顶部云片方位角随机角度范围")]
        [Range(0, 45)] 
        public float topFaiNoise = 36;

        [Header("顶部云片仰角角度")]
        [Range(0, 45)] 
        public float topTheta = 8f;
        
        [Header("顶部云片仰角随机角度范围")]
        [Range(0, 45)] 
        public float topThetaNoise = 4;

        [Header("云片动画总时长")]
        [Min(0)]
        public float animDuration = 60;
        
        [Header("云片动画淡入淡出时长")]
        [Min(0)]
        public float animFadeDuration = 20;

        [Range(0, 10)] 
        public float animSpeed = 1.0f;

        public bool enableDebugAnimTime01;
        
        [Range(0, 1)] 
        public float debugAnimTime01 = 0;
        
        [Header("随动画时间01变换的边缘羽化强度")]
        public AnimationCurve edgeFeatherCurve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.001f, 0.1f),
            new Keyframe(0.999f, 0.1f),
            new Keyframe(1.0f, 0f)
        );        

        
        
        private MeshFilter _meshFilter;
        private MeshFilter meshFilter
        {
            get
            {
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();
                }

                return _meshFilter;
            }
        }


        private Mesh _runtimeMesh;
        private Mesh runtimeMesh
        {
            get
            {
                if (_runtimeMesh == null)
                {
                    _runtimeMesh = new Mesh();
                    _runtimeMesh.hideFlags = HideFlags.DontSave;
                    _runtimeMesh.name = "GenshinDyanmicCloud";
                    _runtimeMesh.MarkDynamic();
                }

                return _runtimeMesh;
            }
        }

        private bool _cloudMeshInited;
        
        private Color[] color;
        private Vector3[] vertices;
        private Vector2[] uv;

        /// <summary>
        /// 可见因子，uv2.y / uv2.x 的结果用于和 uv3 比较
        /// </summary>
        private Vector2[] uv2;

        /// <summary>
        /// 可见阈值 0.4 ~ 0.6
        /// </summary>
        private Vector2[] uv3;
        
        private int[] triangles;
        
        [Header("编辑器改变生成参数自动重新生成")]
        public bool alwayRebuild;

        public void Update()
        {
            Mesh mesh = runtimeMesh;
            meshFilter.sharedMesh = mesh;

            if (!_clouds_inited || clouds == null || (clouds.Count == 0 && baseCloudCount > 0)) 
            {
                InitClouds(); 
            }

            // Apply clouds data to mesh
            {
                ApplyToMesh(); 
            }

        }

        private void ApplyToMesh()
        {
            int cloudCount = clouds.Count;
            int vertexCount = cloudCount * Cloud.s_VERTEX_COUNT;
            // 云模型的顶点数组内存开辟
            if (!_cloudMeshInited || vertices == null || vertices.Length != vertexCount)
            {
                vertices = new Vector3[vertexCount];
                color = new Color[vertexCount];
                uv = new Vector2[vertexCount];
                uv2 = new Vector2[vertexCount];
                uv3 = new Vector2[vertexCount];
                // duplicate from Cloud.s_DUMMY_MESH_TRIANGLES
                var array = Enumerable.Repeat(Cloud.s_DUMMY_MESH_TRIANGLES, cloudCount).ToArray();
                var array2 = Enumerable.Repeat(Cloud.s_DUMMY_MESH_TRIANGLES, cloudCount).SelectMany(x => x);
                
                // 依据 dummy mesh 的 triangles { 0, 1, 2, 2, 1, 3 } 生成 { 0, 1, 2, 2, 1, 3,  4, 5, 6, 6, 5, 7 .... }
                {
                    int flatteningArrayIndex = 0; // 当前在扁平化第几个数组，即第几个 mesh
                    triangles = Enumerable.Repeat(Cloud.s_DUMMY_MESH_TRIANGLES, cloudCount)
                    .SelectMany(
                        x =>
                        {
                            int currentFlatteningIndex = flatteningArrayIndex;
                            flatteningArrayIndex++;
                            
                            // 当前的 mesh 的 triangles 索引均 + 4，即 0, 1, 2, 2, 1, 3 -> 4, 5, 6, 6, 5, 7
                            return x.Select(
                                item => item + currentFlatteningIndex * Cloud.s_VERTEX_COUNT
                            );
                        }
                    ).ToArray();
                }

                // 变了顶点数量需要 Clear 不然会报错，因为改 index 可能没对应的 vertices，改 vertices 有提示 index 用到相应的 vertices
                Mesh mesh = runtimeMesh;
                mesh.Clear();
                
                _cloudMeshInited = true;
            }

            // 更新 vertices, color, uv2, uv3 动态云顶点数据
            {
                for (int i = 0; i < cloudCount; i++)
                {
                    Cloud cloud = clouds[i];
                    bool _enableDebugAnimTime01 = false;
                    #if UNITY_EDITOR
                    _enableDebugAnimTime01 = enableDebugAnimTime01;
                    #endif
                    if (_enableDebugAnimTime01)
                    {
                        cloud.animTime = debugAnimTime01 * cloud.animDuration;
                        cloud.UpdateState(0);
                    }
                    else
                    {
                        cloud.UpdateState(Time.deltaTime * animSpeed);
                        if (cloud.animTime >= cloud.animDuration)
                        {
                            InitOneCloud(cloud);
                        }
                    }

                    cloud.UpdateDynamicVertexData();

                    int start = i * Cloud.s_VERTEX_COUNT;
                    
                    Array.Copy(cloud.vertices, 0, vertices, start, Cloud.s_VERTEX_COUNT);
                    Array.Copy(cloud.color, 0, color, start, Cloud.s_VERTEX_COUNT);
                    Array.Copy(cloud.uv, 0, uv, start, Cloud.s_VERTEX_COUNT);
                    Array.Copy(cloud.uv2, 0, uv2, start, Cloud.s_VERTEX_COUNT);
                    Array.Copy(cloud.uv3, 0, uv3, start, Cloud.s_VERTEX_COUNT);
                    
                }
            }

            // 应用云数据
            {
                Mesh mesh = runtimeMesh;
                mesh.vertices = vertices;
                mesh.colors = color;
                mesh.uv = uv;
                mesh.uv2 = uv2;
                mesh.uv3 = uv3;
                mesh.triangles = triangles;

                mesh.UploadMeshData(false);
            }

            
            
        }

        /// <summary>
        /// 初始化一个云片
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="randomInitTime">随机动画正在播放的时间</param>
        public void InitOneCloud(Cloud cloud, bool randomInitTime = false)
        {
            float fai = cloud.fai;
            Vector3 sphericalPosition = new Vector3(0, 0, baseCloudDistance); // 初始位置在 z 轴
            if (!cloud.isTop)
            {
                sphericalPosition = Quaternion.Euler(0, fai + baseFaiNoise * Random.Range(-0.5f, 0.5f), 0) * sphericalPosition; // 旋转角度
            }
            else
            {
                // top 需要旋转 X 轴
                sphericalPosition = Quaternion.Euler(-(topTheta) + topThetaNoise * Random.Range(-0.5f, 0.5f), fai + topFaiNoise * Random.Range(-0.5f, 0.5f), 0) * sphericalPosition; // 旋转角度
            }

            cloud.position = sphericalPosition + Vector3.up * baseCloudHeight;
            cloud.rotation = Quaternion.LookRotation(sphericalPosition);
            cloud.scale = Vector3.one * Random.Range(minScale, maxScale);
            cloud.cloudGridIndex = Random.Range(0, Cloud.s_MAX_CLOUD_GRID_INDEX + 1);
            cloud.edgeFeatherCurve = edgeFeatherCurve;
            cloud.animDuration = animDuration;
            cloud.animTime = 0;
            cloud.animFadeDuration = animFadeDuration;
            if (randomInitTime)
            {
                cloud.animTime = Random.Range(0, animDuration);
            }
        }

        /// <summary>
        /// 重新初始化所有云片
        /// </summary>
        public void InitClouds()
        {
            if (clouds == null)
            {
                clouds = new List<Cloud>();
            }
            else
            {
                clouds.Clear();
            }
            
            // 生成下面的云
            for (int i = 0; i < baseCloudCount; i++)
            {
                Cloud cloud = new Cloud()
                {
                    fai = (float)i / baseCloudCount * 360,
                    isTop = false
                };
                clouds.Add(cloud);
                InitOneCloud(cloud, true);
            }

            if (appendTopCount)
            {
                for (int i = 0; i < baseCloudCount; i++)
                {
                    Cloud cloud = new Cloud()
                    {
                        fai = (float)i / baseCloudCount * 360,
                        isTop = true
                    };
                    clouds.Add(cloud);
                    InitOneCloud(cloud, true);
                }
            }

            // data 变了， 模型需要重新生成
            _cloudMeshInited = false;
            
            _clouds_inited = true;  
        }

        private void OnDestroy()
        {
            if (_runtimeMesh != null)
            {
                objcleaner.Destroy(_runtimeMesh);
                _runtimeMesh = null;
            }
        }
    }
}
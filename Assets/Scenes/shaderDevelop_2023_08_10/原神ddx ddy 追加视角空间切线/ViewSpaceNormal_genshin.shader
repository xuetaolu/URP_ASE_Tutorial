Shader "Unlit/ViewSpaceNormal_genshin"
{
    Properties
    {
    	_Bump("_Bump", Range(0, 0.99)) = 0.2
        _NormalMap ("_NormalMap", 2D) = "bump" {}
    	[Toggle(_ENABLE_NORMALMAP)]_ENABLE_NORMALMAP("_ENABLE_NORMALMAP", float) = 0
        _DiffuseStrength ("_DiffuseStrength", Range(0, 1)) = 1.0
        _SpecularStrength ("_SpecularStrength", Range(0, 1)) = 1.0
    	_EnvironmentStrength ("_EnviromentStrength", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ENABLE_NORMALMAP

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _NormalMap;
            float _Bump;
            float _DiffuseStrength;
            float _SpecularStrength;
            float _EnvironmentStrength;

            float diffuse(float3 n,float3 l,float p) { return pow(max(dot(n,l),0.0),p); }
            float specular(float3 n,float3 l,float3 e,float s) {    
                float nrm = (s + 8.0) / (3.1415 * 8.0);
                return pow(max(dot(reflect(e,n),l),0.0),s) * nrm;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewSpaceNormal : TEXCOORD1;
            	float3 worldPos : TEXCOORD2;
            	float3 viewSpacePos : TEXCOORD3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
            	float3 _worldPos = mul(UNITY_MATRIX_M, v.vertex).xyz;
            	o.worldPos = _worldPos;
                o.uv = v.uv;
                float3 worldSpaceNormal = UnityObjectToWorldNormal(v.normal);
                o.viewSpaceNormal = mul((float3x3)UNITY_MATRIX_V, worldSpaceNormal);
            	o.viewSpacePos = UnityObjectToViewPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            	
				float3 _viewSpaceLightDir = mul((float3x3)UNITY_MATRIX_V, _WorldSpaceLightPos0);
                fixed4 col = fixed4(0, 0, 0, 1);
            	

            	float3 _viewSpaceNormal = i.viewSpaceNormal;
            	float2 _normalMapUV = i.uv;
				float2 _normalMapSampleXY = tex2D(_NormalMap, _normalMapUV).xy;
            	
            	float _customNormalMapZ = max(-_Bump + 1.0, 0.001);
            	

				float3 _mapNormal = float3(_normalMapSampleXY * 2.0 - 1.0, _customNormalMapZ);
            	float3 _normalMapNormalize = normalize(_mapNormal);

            	

				float2 _ddx_uv = ddx(_normalMapUV); // ddx(uv)
			    float2 _ddy_uv = ddy(_normalMapUV); // ddy(uv)
				
				float3 _worldPos = i.worldPos;

				float3 _viewSpacePos = i.viewSpacePos;
            	

				float3 _viewDir_dx = ddx(_viewSpacePos); // v0
				float3 _viewDir_dy = ddy(_viewSpacePos); // v1


            	// 叉乘推导方式(cross 函数编译解析)：
            	// a×b = float3( a.y*b.z - b.y*a.z, -(a.x*b.z - b.x*a.z), a.x*b.y - b.x*a.y );
            	//     = float3( a.y*b.z - a.z*b.y, a.z*b.x - a.x*b.z   , a.x*b.y - a.y*b.x );
            	//     = float3( a.y*b.z, a.z*b.x, a.x*b.y ) - float3( a.z*b.y, a.x*b.z, a.y*b.x )
            	//     = a.yzx * b.zxy - a.zxy * b.yzx
            	// 所以 cross(a, b) = a.yzx * b.zxy - a.zxy * b.yzx;
            	
            	
				// float3 _As_U = (_viewDir_dy.yzx * _viewSpaceNormal.zxy) - (_viewDir_dy.zxy * _viewSpaceNormal.yzx);
            	                                     // v1         // N
            	float3 _viewDyCrossNormal_AsU = cross(_viewDir_dy, _viewSpaceNormal);
            	// 预计就是 正常的右方方向，UV 的 U 方向
            	
				// float3 _As_V = (_viewSpaceNormal.yzx * _viewDir_dx.zxy) - (_viewSpaceNormal.zxy * _viewDir_dx.yzx);
            	                                     // N          // v0
            	float3 _normalCorssViewDx_AsV = cross(_viewSpaceNormal, _viewDir_dx);
            	// 预计就是 正常的上方方向，UV 的 V 方向

            	// 在 uv 坐标中，向量 u' 方向为 (1, 0)， 向量 v' 方向为 (0, 1)
            	// 存在线性组合
            	//   ddx(uv).x * u' + ddx(uv).y * v' = ddx(uv)
            	//   ddy(uv).x * u' + ddy(uv).y * v' = ddy(uv)
            	// 假设在视角空间，对齐 uv 坐标方向的朝向分别为 U', V'
            	// 也有线性组合
            	//   ddx(uv).x * U' + ddx(uv).y * V' = U = ddx(_viewSpacePos)
            	//   ddy(uv).x * U' + ddy(uv).y * V' = V = ddy(_viewSpacePos)
            	// 写成矩阵的形式为
            	//   [ ddx(uv).x  ddx(uv).y ]     [ U']    [ U ]
            	//   [ ddy(uv).x  ddy(uv).y ]  *  [ V'] =  [ V ]
            	// 简单假设左边矩阵为正交单位矩阵，其逆矩阵为其转置，则有
            	//   [ U']   [ ddx(uv).x  ddy(uv).x ]   [ U ]
            	//   [ V'] = [ ddx(uv).y  ddy(uv).y ] * [ V ]
            	// 分开写则是：
            	//      U' = U * ddx(uv).x + V * ddy(uv).x
            	//      V' = U * ddx(uv).y + V * ddy(uv).y
            	
            	
            	                       // U                   // ddx(uv).x   // V                   // ddy(uv).x
			    float3 _normalX_dir = (_viewDyCrossNormal_AsU * _ddx_uv.x) + _normalCorssViewDx_AsV * _ddy_uv.x;
                                       // U                   // ddx(uv).y   // V                   // ddy(uv).y
			    float3 _normalY_dir = (_viewDyCrossNormal_AsU * _ddx_uv.y) + _normalCorssViewDx_AsV * _ddy_uv.y;



    //         	float _maxLength_rcp;
				// {
				//     float _90 = dot(_normalX_dir, _normalX_dir);
				//     float _93 = dot(_normalY_dir, _normalY_dir);
				//     _90 = max(_90, _93);
				//     _90 = sqrt(_90);
				//     _90 = 1.0 / _90;
				// 	_maxLength_rcp = _90;
				// }
            	
			    // float3 _normalX = (_maxLength_rcp) * _normalX_dir;
				// float3 _normalY = (_maxLength_rcp) * _normalY_dir;

            	// 因为后续做单位化所以没必要提前约束长度。
            	float3 _normalX = _normalX_dir;
            	float3 _normalY = _normalY_dir;

				_normalX = normalize(_normalX); // normalMap X
				_normalY = normalize(_normalY); // normalMap Y
				
			    // _92 = 0.9900000095367431640625 >= _normalMapNormalize.z; // 这里没用，应该用来判断是完整平的地方
            	
				float3 _viewSpaceNormalFix = _normalMapNormalize.x * _normalX 
					+ _normalMapNormalize.y * _normalY 
					+ _normalMapNormalize.z * _viewSpaceNormal;
            	
            	
            	float3 _outputViewSpaceNormal = i.viewSpaceNormal;
            	#if _ENABLE_NORMALMAP
            	_outputViewSpaceNormal = normalize(_viewSpaceNormalFix);
            	#endif
            	// col.xyz = _outputViewSpaceNormal;

				float3 _fixWorldNormal = mul(_outputViewSpaceNormal, (float3x3)UNITY_MATRIX_V);
            	
                float3 _lighting = 0;
				{
	                float n = _outputViewSpaceNormal;
            		float l = _viewSpaceLightDir;
            		float3 e = -normalize(_WorldSpaceCameraPos - _worldPos);
	                _lighting += float3(diffuse(n,l,1.0) * _LightColor0.xyz/*WHITE*/) * _DiffuseStrength;
	                _lighting += float3(specular(n,l,e,8.0) * _LightColor0.xyz/*WHITE*/ * 1.5 ) * _SpecularStrength;
            		_lighting += ShadeSH9(half4(_fixWorldNormal, 1.0)) * _EnvironmentStrength;
				}
            	col.xyz = _lighting;

            	// col.xyz = _fixWorldNormal;
            	// col.xyz = _outputViewSpaceNormal;
            	
                return col;
            }
            ENDCG
        }
    }
}

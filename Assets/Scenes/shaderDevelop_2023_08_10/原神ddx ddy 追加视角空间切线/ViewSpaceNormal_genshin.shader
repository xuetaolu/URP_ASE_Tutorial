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

            	

				float2 _normalMapUV_dx = ddx(_normalMapUV);
			    float2 _normalMapUV_dy = ddy(_normalMapUV);
				
				float3 _worldPos = i.worldPos;

				float3 _viewSpacePos = i.viewSpacePos;
            	

				float3 _viewDir_dx = ddx(_viewSpacePos);
				float3 _viewDir_dy = ddy(_viewSpacePos);
            	
				float3 _UK_x = (_viewSpaceNormal.zxy * _viewDir_dy.yzx) - (_viewSpaceNormal.yzx * _viewDir_dy.zxy);
            	
				float3 _UK_y = (_viewSpaceNormal.yzx * _viewDir_dx.zxy) - (_viewSpaceNormal.zxy * _viewDir_dx.yzx);
            	
			    float3 _59 = (_UK_x * _normalMapUV_dx.xxx) + _UK_y * _normalMapUV_dy.xxx;

			    float3 _49 = (_UK_x * _normalMapUV_dx.yyy) + _UK_y * _normalMapUV_dy.yyy;


			    float _90 = dot(_59, _59);
			    float _93 = dot(_49, _49);
			    _90 = max(_90, _93);
			    _90 = sqrt(_90);
			    _90 = 1.0 / _90;

			    float3 _52 = (_90) * _59;
				_49 = (_90) * _49;


				_52 = normalize(_52); // normalMap X


				_49 = normalize(_49); // normalMap Y

				
			    // _92 = 0.9900000095367431640625 >= _normalMapNormalize.z; // 这里没用，应该用来判断是完整平的地方
            	
				float3 _viewSpaceNormalFix = _normalMapNormalize.x * _52 
					+ _normalMapNormalize.y * _49 
					+ _normalMapNormalize.z * _viewSpaceNormal;
            	
            	
            	float3 _outputViewSpaceNormal = i.viewSpaceNormal;
            	#if _ENABLE_NORMALMAP
            	_outputViewSpaceNormal = normalize(_viewSpaceNormalFix);
            	#endif
            	// col.xyz = _outputViewSpaceNormal;

				float3 _fixWorldNormal = mul(_outputViewSpaceNormal, (float3x3)UNITY_MATRIX_V);
            	
                float3 _lighting = 0;
                float n = _outputViewSpaceNormal;
            	float l = _viewSpaceLightDir;
            	float3 e = -normalize(_WorldSpaceCameraPos - _worldPos);
                _lighting += float3(diffuse(n,l,1.0) * _LightColor0.xyz/*WHITE*/) * _DiffuseStrength;
                _lighting += float3(specular(n,l,e,8.0) * _LightColor0.xyz/*WHITE*/ * 1.5 ) * _SpecularStrength;
            	_lighting += ShadeSH9(half4(_fixWorldNormal, 1.0)) * _EnvironmentStrength;
            	col.xyz = _lighting;

            	// col.xyz = _fixWorldNormal;
            	// col.xyz = _outputViewSpaceNormal;
            	
                return col;
            }
            ENDCG
        }
    }
}

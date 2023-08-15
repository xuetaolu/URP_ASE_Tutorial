Shader "AC/AC_skybox"
{
    Properties
    {
        //VS
        // 不变
//        _UV1_RS_Matrix ("_UV1_RS_Matrix", Vector) = (1.70, 0.00, 0.00, 15.00) //vp_c5_1._m0[3] 
        // 变
        _UV1_T  ("_UV1_T", Vector) = (0.00, -11.35793, 0.00, 0.00) //vp_c5_1._m0[4] 
        _UV2_RS_Matrix  ("_UV2_RS_Matrix", Vector) = (0.13985, -0.99015, 0.99015, 0.13985) //vp_c5_1._m0[5] 
        _UV2_T  ("_UV2_T", Vector) = (-0.065, 0.92515, 0.00, 0.00) //vp_c5_1._m0[6] 
        // 不变
//        _RoateRadiusX ("_RoateRadiusX", float) = -0.05772
//        _worldPosRotateOnly_YBaisY ("_worldPosRotateOnly_YBaisY", float) = 0.00
        
        
        //PS
        // 变
        [HDR]_SkyColorRGB ("_SkyColorRGB", Color) = (6.78572, 3.07114, 7.2585, 1.0)
        _GalaxyAdditionA_Minus_0_to_n1 ("_GalaxyAdditionA_Minus_0_to_n1", float) = -0.20113
        _GalaxyAdditionB_Scale ("_GalaxyAdditionB_Scale", float) = 26.03729
        _GalaxyAdditionB_RelativeToNight ("_GalaxyAdditionB_RelativeToNight", float) = 0.1

        // 变 这个和 HDR 渲染一样的参数
        [Header(0_3)]
        _ScatterMie ("_ScatterMie", Range(0, 1)) = 0.02928
        [Header(0_3_to_0_9)]
        _ScatterInstensity ("_ScatterInstensity", Range(0, 1)) = 0.92808
         
        // 变
        [HDR]_MieRedHdrColor ("_MieRedHdrColor", Color) = (68.36447, 6.83645, 2.78375, 1.00)
        _GalaxyFactor01 ("_GalaxyFactor01", float) = 0.53596
        _MainLightAffectLumenScale ("_MainLightAffectLumenScale", float) = 98.60019
        _MainLightColorXYZ ("_MainLightColorXYZ", Color) = (0.62483, 0.74478, 0.67517, 1.0)
        _MainLightAroundIntensity ("_MainLightAroundIntensity", float) = 0.10
        
        _FinalMainLightAffectFactor ("_FinalMainLightAffectFactor", float) = 0.06961
        
        _HdrMaskMap ("_HdrMaskMap", 2D) = "white" {}
        _StarMap ("_StarMap", 2D) = "black" {}
        _ColorStarMap ("_ColorStarMap", 2D) = "black" {}
        _GalaxyMap ("_GalaxyMap", 2D) = "black" {}
        _SunHdrMap ("_SunHdrMap", 2D) = "black" {}
        _EnvSingleMap ("_EnvSingleMap", 2D) = "black" {}
        
//        _MainTex ("tex2D", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            // VS
            static const float4 _UV1_RS_Matrix = float4(1.70, 0.00, 0.00, 15.00);
            float4 _UV1_T;
            float4 _UV2_RS_Matrix;
            float4 _UV2_T;
            static const float _RoateRadiusX = -0.05772;
            static const float _worldPosRotateOnly_YBaisY = 0.00;

            //PS
            #define _FastAcosParams  float4(1.00000E-07, -0.01873, -0.21211, 1.57073)// fp_c1_1._m0[0]
            #define _PI_X__  float4(3.14159, -1.90, 0.05039, 4.67697)// fp_c1_1._m0[1]
            #define _ZWGrayGB  float4(10.92821, 26.68021, 0.58661, 0.11448)// fp_c1_1._m0[2]

            #define _WorldLightToDir _WorldSpaceLightPos0
            #define _Hdr_UV_RS_Matrix  float4(1.00, 0.00, 0.00, 1.00)// fp_c6_1._m0[1] 
            #define _Hdr_UV_T  float4(0.00, 0.00, 0.00, 0.00)// fp_c6_1._m0[2] 
            #define _NightGroundColorRGB  float4(0.01, 0.12, 0.40, 1.00)// fp_c6_1._m0[9] 
            #define _GalaxyDarkColorRGB float4(0.00, 0.01, 0.09, 1.00)// fp_c6_1._m0[10]
            #define _GalaxyBrightColorRGB float4(0.09989, 0.07829, 0.21404, 1.00)// fp_c6_1._m0[11]
            float _ScatterMie;
            float _ScatterInstensity;
            #define fp_c7__m6 float4(10.00, _ScatterMie, _ScatterInstensity, 0.00) // fp_c7_1._m0[6] 
            #define _StarInstensityY_ float4(-0.05772, 0.00, 0.00, 0.00) // fp_c7_1._m0[7]
            
            float3 _SunLookAtDir;
            
            #define fp_c7__m22 float4( 0.00, 0.00, 0.00, 0.00)// fp_c7_1._m0[22]
            #define _MainLightAffectDiskIntensity fp_c7__m22.x           // 0.00 不变

            #define fp_c7__m24 float4(400.00, 0.00, 0.00, 0.00)// fp_c7_1._m0[24]
            #define _P_DotNegateWorldLightDir_Factor       fp_c7__m24.z  // 不变 0
            #define _V_DotNegateWorldLightDir_Factor   fp_c7__m24.w  // 不变 0

            float3 _SkyColorRGB;
            float _GalaxyAdditionA_Minus_0_to_n1;
            float _GalaxyAdditionB_Scale;
            float _GalaxyAdditionB_RelativeToNight;
            
            float3 _MieRedHdrColor;
            float _GalaxyFactor01;
            float _MainLightAffectLumenScale;
            float3 _MainLightColorXYZ;
            float _MainLightAroundIntensity;
            float _FinalMainLightAffectFactor;
            
            sampler2D _HdrMaskMap;
            sampler2D _StarMap;
            sampler2D _ColorStarMap;
            sampler2D _GalaxyMap;
            sampler2D _SunHdrMap;
            sampler2D _EnvSingleMap;
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 Varying_worldPos : TEXCOORD0;
                float3 Varying_WorldRotateAroundX : TEXCOORD1;
                float4 Varying_ColorStarUV_XY_StarUV_ZW : TEXCOORD2;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 Vertex_Position = v.vertex;
                float2 Vertex_UV = v.uv;
                

                float3 _worldPos = mul(UNITY_MATRIX_M, Vertex_Position).xyz;
                // _worldPos.x = Vertex_Position.x * _Matrix3x4_M_1.x;
                // _worldPos.y = Vertex_Position.x * _Matrix3x4_M_2.x;
                // _worldPos.z = Vertex_Position.x * _Matrix3x4_M_3.x;
                //
                // _worldPos.x = Vertex_Position.y * _Matrix3x4_M_1.y + _worldPos.x;
                // _worldPos.y = Vertex_Position.y * _Matrix3x4_M_2.y + _worldPos.y;
                // _worldPos.z = Vertex_Position.y * _Matrix3x4_M_3.y + _worldPos.z;
                //
                // _worldPos.x = Vertex_Position.z * _Matrix3x4_M_1.z + _worldPos.x;
                // _worldPos.y = Vertex_Position.z * _Matrix3x4_M_2.z + _worldPos.y;
                // _worldPos.z = Vertex_Position.z * _Matrix3x4_M_3.z + _worldPos.z;
                //
                // _worldPos.x = _worldPos.x + _Matrix3x4_M_1.w;
                // _worldPos.y = _worldPos.y + _Matrix3x4_M_2.w;
                // _worldPos.z = _worldPos.z + _Matrix3x4_M_3.w;

                o.Varying_worldPos.xyz = _worldPos.xyz;

                // float3 _viewPos = mul(UNITY_MATRIX_V, _worldPos);;
                // _viewPos.x = _worldPos.x * _Matrix3x4_V_1.x;
                // _viewPos.y = _worldPos.x * _Matrix3x4_V_2.x;
                // _viewPos.z = _worldPos.x * _Matrix3x4_V_3.x;
                //
                // _viewPos.x = _worldPos.y * _Matrix3x4_V_1.y + _viewPos.x;
                // _viewPos.y = _worldPos.y * _Matrix3x4_V_2.y + _viewPos.y;
                // _viewPos.z = _worldPos.y * _Matrix3x4_V_3.y + _viewPos.z;
                //
                // _viewPos.x = _worldPos.z * _Matrix3x4_V_1.z + _viewPos.x;
                // _viewPos.y = _worldPos.z * _Matrix3x4_V_2.z + _viewPos.y;
                // _viewPos.z = _worldPos.z * _Matrix3x4_V_3.z + _viewPos.z;
                //
                // _viewPos.x = _viewPos.x + _Matrix3x4_V_1.w;
                // _viewPos.y = _viewPos.y + _Matrix3x4_V_2.w;
                // _viewPos.z = _viewPos.z + _Matrix3x4_V_3.w;


                // o.vertex = mul(UNITY_MATRIX_P, _viewPos);
                // float4 _tmp1;
                // _tmp1.w = _viewPos.x * _Matrix4x4_P_4.x;
                // _tmp1.z = _viewPos.x * _Matrix4x4_P_3.x;
                // _tmp1.y = _viewPos.x * _Matrix4x4_P_2.x;
                // _tmp1.x = _viewPos.x * _Matrix4x4_P_1.x;
                //
                // _tmp1.w = _viewPos.y * _Matrix4x4_P_4.y + _tmp1.w;
                // _tmp1.z = _viewPos.y * _Matrix4x4_P_3.y + _tmp1.z;
                // _tmp1.y = _viewPos.y * _Matrix4x4_P_2.y + _tmp1.y;
                // _tmp1.x = _viewPos.y * _Matrix4x4_P_1.y + _tmp1.x;
                //
                // _tmp1.w = _viewPos.z * _Matrix4x4_P_4.z + _tmp1.w;
                // _tmp1.z = _viewPos.z * _Matrix4x4_P_3.z + _tmp1.z;
                // _tmp1.y = _viewPos.z * _Matrix4x4_P_2.z + _tmp1.y;
                // _tmp1.x = _viewPos.z * _Matrix4x4_P_1.z + _tmp1.x;
                //
                // o.vertex.w = _tmp1.w + _Matrix4x4_P_4.w;
                // o.vertex.z = _tmp1.z + _Matrix4x4_P_3.w;
                // o.vertex.y = _tmp1.y + _Matrix4x4_P_2.w;
                // o.vertex.x = _tmp1.x + _Matrix4x4_P_1.w;


                // float3 _minus_object_pos = -UNITY_MATRIX_M._13_23_33;
                // _minus_object_pos.x = -_Matrix3x4_M_1.w;
                // _minus_object_pos.y = -_Matrix3x4_M_2.w;
                // _minus_object_pos.z = -_Matrix3x4_M_3.w;
                

                // float3 _worldPosRotateOnly;
                // _worldPosRotateOnly.x = _worldPos.x + _minus_object_pos.x;
                // #define _worldPosRotateOnly_YBaisY float4(400.00, 0.00, 0.00, 0.00)//vp_c6_1._m0[24]
                // float _122 = _worldPos.y + _worldPosRotateOnly_YBaisY;
                // _worldPosRotateOnly.y = _worldPos.y + _worldPosRotateOnly_YBaisY + _minus_object_pos.y;
                // _worldPosRotateOnly.z = _worldPos.z + _minus_object_pos.z;
                // _worldPosRotateOnly = _worldPos + _minus_object_pos;
                //float3 _worldPosRotateOnly = mul((float3x3)UNITY_MATRIX_M, Vertex_Position);
                float3 _worldPosRotateOnly = _worldPos - UNITY_MATRIX_M._14_24_34;
                
                _worldPosRotateOnly.y += _worldPosRotateOnly_YBaisY;
                

                //    三维旋转矩阵，绕 X
                //    1      0      0      X
                //    0   cosθ  -sinθ   *  Y 
                //    0   sinθ   cosθ      Z
                // 
                // =  X
                //    cosθ * Y - sinθ * Z
                //    sinθ * Y + cosθ * Z 

                //    逆旋转则是
                //    1      0      0      X
                //    0   cosθ   sinθ   *  Y 
                //    0  -sinθ   cosθ      Z
                // =  X
                //    cosθ * Y  + sinθ * Z
                //   -sinθ * Y  + cosθ * Z 

                float _Uk_cos = cos(_RoateRadiusX.x);
                float _Uk_sin = sin(_RoateRadiusX.x);
                // float _142 = _Uk_sin * _worldPosRotateOnly.z;
                // float _158 = _Uk_cos * _worldPosRotateOnly.y + _Uk_sin * _worldPosRotateOnly.z;
                // float _160 = -_worldPosRotateOnly.y;
                // float _144 = _Uk_cos * _worldPosRotateOnly.z;
                // float _162 = -_Uk_sin * _worldPosRotateOnly.y + _Uk_cos * _worldPosRotateOnly.z;

                o.Varying_WorldRotateAroundX.x = _worldPosRotateOnly.x;
                o.Varying_WorldRotateAroundX.y = _Uk_cos * _worldPosRotateOnly.y + _Uk_sin * _worldPosRotateOnly.z;
                o.Varying_WorldRotateAroundX.z = -_Uk_sin * _worldPosRotateOnly.y + _Uk_cos * _worldPosRotateOnly.z;

                // _RS_Matrix    T
                //   x   z    |  x
                //   y   w    |  y


                // float _74 = Vertex_UV.y * _UV2_RS_Matrix.z;
                // float _76 = Vertex_UV.y * _UV2_RS_Matrix.w;

                //   _UV2_RS_Matrix        T 
                //  0.13985  0.99015 |  -0.065
                // -0.99015  0.13985 | 0.92515

                // 变 #define _UV2_RS_Matrix  float4(0.13985, -0.99015, 0.99015, 0.13985)//vp_c5_1._m0[5] 
                // 变 #define _UV2_T  float4(-0.065, 0.92515, 0.00, 0.00)//vp_c5_1._m0[6] 
                o.Varying_ColorStarUV_XY_StarUV_ZW.zw = Vertex_UV.x * _UV2_RS_Matrix.xy + Vertex_UV.y * _UV2_RS_Matrix.zw + _UV2_T.xy;


                //   _UV1_RS_Matrix        T
                //   1.70     0.00   |    0.00
                //   0.00    15.00   | -11.35793

                // 不变 #define _UV1_RS_Matrix  float4(1.70, 0.00, 0.00, 15.00) //vp_c5_1._m0[3] 
                // 变 #define _UV1_T  float4(0.00, -11.35793, 0.00, 0.00)//vp_c5_1._m0[4] 
                o.Varying_ColorStarUV_XY_StarUV_ZW.xy = Vertex_UV.x * _UV1_RS_Matrix.xy + Vertex_UV.y * _UV1_RS_Matrix.zw + _UV1_T.xy;
                
                return o;
            }

            #define INV_PI      0.31830988618379067154
            #define INV_TWO_PI  0.15915494309189533577
            
            // 当 0 < x < 1 时
            //   (((x*(-0.01873)+0.074261002242565155029296875)*x+(-0.21211))*x+1.57073)*sqrt(1.0-x)
            // ≈ acos(x)  
            float FastAcosForAbsCos(float in_abs_cos) {
                // #define _FastAcosParams  float4(1.00000E-07, -0.01873, -0.21211, 1.57073)// fp_c1_1._m0[0] 
                float _local_tmp = ((in_abs_cos * _FastAcosParams.y + 0.074261002242565155029296875) * in_abs_cos + _FastAcosParams.z) * in_abs_cos + _FastAcosParams.w;
                return _local_tmp * sqrt(1.0 - in_abs_cos);
            }

            float FastAcos(float in_cos) {
                // #define _PI_X__  float4(3.14159, -1.90, 0.05039, 4.67697)// fp_c1_1._m0[1] 
                float local_abs_cos = abs(in_cos);
                float local_abs_acos = FastAcosForAbsCos(local_abs_cos);
                return in_cos < 0.0 ?  _PI_X__.x - local_abs_acos : local_abs_acos;
            }

            float2 UV_FlipY(float2 uv)
            {
                uv.y = 1-uv.y;
                return uv;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = float4(0,0,0,1);

                float3 _worldRotateAroundX_normalize = normalize(i.Varying_WorldRotateAroundX.xyz);
                
                // bool _82 = _worldRotateAroundX_normalize.z > 0.0;
                float _horizontal_square = _worldRotateAroundX_normalize.z * _worldRotateAroundX_normalize.z 
                    + _worldRotateAroundX_normalize.x * _worldRotateAroundX_normalize.x;


                // float _98 = sqrt(_horizontal_square);
                // #define _FastAcosParams  float4(1.00000E-07, -0.01873, -0.21211, 1.57073)// fp_c1_1._m0[0] 
                // 方位角 cos，但方向是从 +X(右) 到 前(+Z) 的方位角 φ fai
                float _cos_fai = sqrt(_horizontal_square) > _FastAcosParams.x ? _worldRotateAroundX_normalize.x * rsqrt(_horizontal_square) : 0.0;
                

                // 单位长度 1 的情况下，y 就是极角 θ theta 的 cos
                float _cos_theta = _worldRotateAroundX_normalize.y;
                

                // float _abs_cos_fai = abs(_cos_fai);
                // float _abs_cos_theta = abs(_cos_theta);
                // float _abs_acos_fai = FastAcosForAbsCos(_abs_cos_fai);
                // float _abs_acos_theta = FastAcosForAbsCos(_abs_cos_theta);

                // #define _PI_X__  float4(3.14159, -1.90, 0.05039, 4.67697)// fp_c1_1._m0[1] 
                // float _acos_fai = (_cos_fai < 0.0 ?  _PI_X__.x - _abs_acos_fai : _abs_acos_fai);
                // float _acos_theta = (_cos_theta < 0.0 ? _PI_X__.x - _abs_acos_theta : _abs_acos_theta);
                
                float _acos_fai = FastAcos(_cos_fai);
                float _acos_theta = acos(_cos_theta);

                // 动森 +X 与 Unity +X 对调，所以这里相当于unity的 _rightForwardLeftBackRight_n0_5_to_0_5
                // 所以 _acos_fai 从 0~PI 得翻转成 PI~0
                float _leftForwardRightBackLeft_n0_5_to_0_5 = ( _worldRotateAroundX_normalize.z > 0.0 ? 1 : -1 ) * (UNITY_PI - _acos_fai) * INV_TWO_PI;
                
                float _up_to_down_01 = _acos_theta * INV_PI;


                // #define _Hdr_UV_RS_Matrix  float4(1.00, 0.00, 0.00, 1.00)// fp_c6_1._m0[1] 
                // float _176 = _up_to_down_01 * _Hdr_UV_RS_Matrix.z;
                // float _178 = _up_to_down_01 * _Hdr_UV_RS_Matrix.w;
                // float _180 = _leftForwardRightBackLeft_n0_5_to_0_5 * _Hdr_UV_RS_Matrix.x + _176;
                // float _182 = _leftForwardRightBackLeft_n0_5_to_0_5 * _Hdr_UV_RS_Matrix.y + _178;


                // _RS_Matrix    T
                //   x   z    |  x
                //   y   w    |  y

                //   x z    X
                //   y w  * Y
                // 
                // = x*X + z*Y
                //   y*X + w*X 
                //
                // = X * | x | + Y * | z |
                //       | y |       | w |

                float2 _orgHdrUV = float2( _leftForwardRightBackLeft_n0_5_to_0_5, _up_to_down_01 );

                // #define _Hdr_UV_RS_Matrix  float4(1.00, 0.00, 0.00, 1.00)// fp_c6_1._m0[1] 
                // #define _Hdr_UV_T  float4(0.00, 0.00, 0.00, 0.00)// fp_c6_1._m0[2] 
                float2 _hdr_uv = _orgHdrUV.x * _Hdr_UV_RS_Matrix.xy + _orgHdrUV.y * _Hdr_UV_RS_Matrix.zw + _Hdr_UV_T.xy;
                    // _hdr_uv.x = _leftForwardRightBackLeft_n0_5_to_0_5 * _Hdr_UV_RS_Matrix.x + _up_to_down_01 * _Hdr_UV_RS_Matrix.z + _Hdr_UV_T.x;
                    // _hdr_uv.y = _leftForwardRightBackLeft_n0_5_to_0_5 * _Hdr_UV_RS_Matrix.y + _up_to_down_01 * _Hdr_UV_RS_Matrix.w + _Hdr_UV_T.y;

                float3 _hdrMaskSampleNight = tex2D(_HdrMaskMap, UV_FlipY(_hdr_uv)).rrr;
                float3 _sunHdrSample = tex2D(_SunHdrMap, UV_FlipY(_hdr_uv)).xyz;
                float3 _starMapSampleUV = tex2D(_StarMap, UV_FlipY(i.Varying_ColorStarUV_XY_StarUV_ZW.zw)).rrr;
                float4 _GalaxyMapSample = tex2D(_GalaxyMap, UV_FlipY(i.Varying_ColorStarUV_XY_StarUV_ZW.zw)).xyzw;


                // 方便上半球就能采样整张 Cubemap？，而不是正常上半球只能采样上半张 Cubemap？
                float _up_to_down_02 = _acos_theta * INV_PI * 2;

                float3 _ColorStarSample = tex2D(_ColorStarMap, UV_FlipY(i.Varying_ColorStarUV_XY_StarUV_ZW.xy)).xyz;
                float2 _cubeMapUV = UV_FlipY(float2(_leftForwardRightBackLeft_n0_5_to_0_5, _up_to_down_02));
                float3 _envMapSample = tex2Dlod(_EnvSingleMap, float4(_cubeMapUV, 3.0, 0)).xyz;
                float4 _starMapSampleAsCubeMap = tex2D(_StarMap, _cubeMapUV).rrrr;


                // 变 #define _SunLookAtDir float4(0.55817, 0.03907, 0.8288, 0.00)// fp_c7_1._m0[10]
                float _VDot_SunDir = dot( _worldRotateAroundX_normalize, -_SunLookAtDir.xyz );
                float _VDot_WorldLightDir = dot( _worldRotateAroundX_normalize, float3( -_WorldLightToDir.x, -_WorldLightToDir.y, _WorldLightToDir.z ) );
                

                // 不变 #define _PI_X__  float4(3.14159, -1.90, 0.05039, 4.67697)// fp_c1_1._m0[1] 
                float _VDot_UkDir2 = _VDot_WorldLightDir * _VDot_WorldLightDir;
                float _VDot_SunDir2 = _VDot_SunDir * _VDot_SunDir;
                




                // _PI_X__.z = 0.05039 不变
                // 0~1 -> 0.05 ~ 0.1
                float _517 = _VDot_UkDir2 * _PI_X__.z + 0.0503876097500324249267578125;
                float _515 = _VDot_SunDir2 * _PI_X__.z + 0.0503876097500324249267578125;

                // _PI_X__.y = -1.90 不变
                // -1~1 -> 3.8 ~ 0
                float _493 = abs(_VDot_WorldLightDir * _PI_X__.y + 1.9024999141693115234375);
                float _499 = abs(_VDot_SunDir * _PI_X__.y + 1.9024999141693115234375);
                // -> 0.15 ~ 0.38 ~ +无穷
                float _535 = pow(_493, -1.5);
                float _541 = pow(_499, -1.5);

                float _VDot_WorldLightDir_Remap_only_infite_when_1 = _517 * _535;
                float _VDot_SunDir_Remap_only_infite_when_1 = _515 * _541;

                float3 _worldPos_normalize = normalize(i.Varying_worldPos.xyz);

                float3 color;
                {
                // 0~1 -> 0.75 ~ 1.5
                float _VDot_UkDir_Remap_0_75_to_1_5 = _VDot_UkDir2 * 0.75 + 0.75;
                float _VDot_SunDir_Remap_0_75_to_1_5 = _VDot_SunDir2 * 0.75 + 0.75;
                
                    // 变 这个和 HDR 渲染一样的参
                    // #define fp_c7__m6 float4(10.00, 0.02928, 0.92808, 0.00) // fp_c7_1._m0[6]
                    // fp_c7__m6.y 米氏散射 有多红
                    // fp_c7__m6.z 太阳圆盘亮度？
                    // #define _MieRedHdrColor float4(68.36447, 6.83645, 2.78375, 0.00) // fp_c7_1._m0[8] 

                    float3 _mieRedColor = fp_c7__m6.y * _MieRedHdrColor.xyz;

                    // 不变 #define _PI_X__  float4(3.14159, -1.90, 0.05039, 4.67697)// fp_c1_1._m0[1] 
                    // 不变 #define _ZWGrayGB  float4(10.92821, 26.68021, 0.58661, 0.11448)// fp_c1_1._m0[2] 
                    // _PI_X__.w = 4.67697 不变
                    // _ZWGrayGB.xy = 10.92821, 26.68021 不变
                    // float3(0.01545, 0.03, 0.1) * float3( _PI_X__.w, _ZWGrayGB.xy) = ( 0.072259187, 0.3278463, 2.668021 ) // 这是控制太阳色谱分布？
                    
                    // 天空太阳大范围颜色
                    float3 _sunAroundColor = _VDot_SunDir_Remap_0_75_to_1_5 * _sunHdrSample.xyz * _mieRedColor.xyz * float3(0.01545, 0.03, 0.1) * float3( _PI_X__.w, _ZWGrayGB.xy);
                    
                    // 太阳圆盘
                    // 变 fp_c7__m6.z 太阳圆盘亮度？
                    float3 _sunDiskColor = _sunHdrSample.xyz * fp_c7__m6.z * _MieRedHdrColor.xyz * _VDot_SunDir_Remap_only_infite_when_1 * 0.0015;
                    float3 _sunSumColor = _sunAroundColor + _sunDiskColor;

                    // 不变 #define _GalaxyBrightColorRGB float4(0.09989, 0.07829, 0.21404, 1.00)// fp_c6_1._m0[11]
                    // 不变 #define _GalaxyDarkColorRGB float4(0.00, 0.01, 0.09, 1.00)// fp_c6_1._m0[10]
                    // float3 _v3_433 = ;

                    // 变 #define fp_c7__m20 float4(0.53596, 98.60019, 153.02422, 0.00)// fp_c7_1._m0[20]
                    // 变 #define fp_c6__m13 float4(-0.20113, 26.03729, 0.10, 0.00)// fp_c6_1._m0[13]
                    // float3 _galaxyColorBase = (_GalaxyBrightColorRGB.xyz - _GalaxyDarkColorRGB.xyz) * _GalaxyMapSample.xyz * _GalaxyMapSample.w * _GalaxyFactor01 + _GalaxyDarkColorRGB.xyz;
                    float3 _galaxyColorBase = lerp( _GalaxyDarkColorRGB.xyz, _GalaxyBrightColorRGB.xyz, _GalaxyMapSample.xyz * _GalaxyMapSample.w * _GalaxyFactor01 );

                    // _GalaxyFactor01 = 0.53596 变
                    // _GalaxyAdditionB_RelativeToNight = 0.10 不变
                    // _GalaxyAdditionA_Minus_0_to_n1 = -0.20113 变
                    // _GalaxyAdditionB_Scale = 26.03729 变
                    float3 _galaxyAdditionFactorA = clamp(
                        _starMapSampleUV.xyz - _hdrMaskSampleNight.xyz // 有 star 的地方               // 0~1
                        - _ColorStarSample.xyz                  // 随机减少 _ColorStarSample 值 // -1 ~ 1
                        + _GalaxyAdditionA_Minus_0_to_n1        // 固定减小                     // -1.2 ~ 0.8
                        + _GalaxyFactor01 - 1.0             // 是晚上                       // -1.2 ~ 0.8   or   -2.2 ~ -0.2
                        ,
                        0.0, 1.0);                              // Clamp01                      //  0 ~ 0.8
                    float _galaxyAdditionFactorB = (_GalaxyAdditionB_RelativeToNight - _GalaxyFactor01) * _GalaxyAdditionB_Scale + _GalaxyAdditionB_Scale;
                    float3 _finalGalaxyColor = _GalaxyMapSample.xyz * _galaxyAdditionFactorA * _galaxyAdditionFactorB + _galaxyColorBase.xyz;






                // #define _WorldLightToDir float4(0.35086, -0.89062, 0.2893, 0.00) // fp_c5_1._m0[23]
                float _PDot_WorldLightDir_negateY01 = clamp(dot( _worldPos_normalize, float3(_WorldLightToDir.x, -_WorldLightToDir.y, _WorldLightToDir.z) ), 0.0, 1.0);
                float _PDot_WorldLightDir01 = clamp(dot( _worldPos_normalize, _WorldLightToDir.xyz ), 0.0, 1.0);
                float _VDot_NegateWorldLightDir01 = clamp(dot(_worldRotateAroundX_normalize, -_WorldLightToDir.xyz), 0.0, 1.0);


                // 不变 #define fp_c7__m24 float4(400.00, 0.00, 0.00, 0.00)// fp_c7_1._m0[24]
                // #define _P_DotNegateWorldLightDir_Factor fp_c7__m24.z  // 不变 0
                // float3 _v3_473 = _PDot_WorldLightDir_negateY01 * _SkyColorRGB.xyz * _P_DotNegateWorldLightDir_Factor * INV_PI;
                // float3 _v3_481 = _PDot_WorldLightDir01 * _SkyColorRGB.xyz + _v3_473.xyz;
                // float3 _v3_481 = (_PDot_WorldLightDir01 + _PDot_WorldLightDir_negateY01 * _P_DotNegateWorldLightDir_Factor * INV_PI);


                // float _VDot_NegateWorldLightDir01 = clamp(_443, 0.0, 1.0);
                

                // float3 _v3_471 = _VDot_NegateWorldLightDir01 * _SkyColorRGB.xyz;
                // 不变 _V_DotNegateWorldLightDir_Factor = 0
                // float3 _v3_485 = _VDot_NegateWorldLightDir01 * _V_DotNegateWorldLightDir_Factor * 0.25;

                

                // float3 _ambientSkyColor = (_v3_485.xyz + _v3_481.xyz) * _SkyColorRGB.xyz;
                float3 _ambientSkyColor = (
                        _PDot_WorldLightDir01                                                       // _worldPos_normalize ≈ (0.914, -0.012, -0.405)
                      + _VDot_NegateWorldLightDir01 * _V_DotNegateWorldLightDir_Factor * 0.25       // 0
                      + _PDot_WorldLightDir_negateY01 * _P_DotNegateWorldLightDir_Factor * INV_PI   // 0
                    ) * _SkyColorRGB.xyz;



                    float3 _ambientStarColor = _envMapSample.xyz + _ambientSkyColor.xyz;

                    float _sunOrStarFactor = _starMapSampleAsCubeMap.w * _StarInstensityY_.y;
                    // float3 _sunOrStarColor = (_ambientStarColor.xyz * _starMapSampleAsCubeMap.xyz - _sunSumColor.xyz) * _sunOrStarFactor + _sunSumColor.xyz;
                    float3 _sunOrStarColor = lerp( _sunSumColor, _ambientStarColor * _starMapSampleAsCubeMap.xyz, _sunOrStarFactor );

                    // float3 _dayOrNightColor = (_hdrMaskSampleNight.xyz * _NightGroundColorRGB.xyz + _finalGalaxyColor.xyz - _sunOrStarColor.xyz) * _GalaxyFactor01 + _sunOrStarColor.xyz;
                    float3 _galaxyFinalColor = _hdrMaskSampleNight.xyz * _NightGroundColorRGB.xyz + _finalGalaxyColor;
                    float3 _dayOrNightColor = lerp(_sunOrStarColor, _galaxyFinalColor, _GalaxyFactor01);

                    // #define _MainLightAroundIntensity _MainLightColorXYZ.w  // 0.10 不变
                    // #define _MainLightAffectDiskIntensity fp_c7__m22.x    // 0.00 不变
                    float3 _mainLightAffectDiskColor = _VDot_WorldLightDir_Remap_only_infite_when_1 * _MainLightAffectDiskIntensity * _MainLightColorXYZ.xyz * _SkyColorRGB.xyz;
                    float3 _moonAffectAroundColor = _MainLightAroundIntensity * _MainLightColorXYZ.xyz * _SkyColorRGB.xyz * _VDot_UkDir_Remap_0_75_to_1_5;
                    // 变 #define _FinalMainLightAffectFactor float4(0.30, 0.06961, 1.00, 4.92808)// fp_c7_1._m0[23]
                    float3 _mainLightAffectColor = (_moonAffectAroundColor + _mainLightAffectDiskColor) * _FinalMainLightAffectFactor;


                    // Gray = R*0.299 + G*0.587 + B*0.114
                    // #define _ZWGrayGB  float4(10.92821, 26.68021, 0.58661, 0.11448)// fp_c1_1._m0[2] 
                    // float _667 = _mainLightAffectColor.x * 0.298911988735198974609375;
                    // float _681 = _mainLightAffectColor.y * _ZWGrayGB.z + _667;
                    // float _finalSkyColorGray = _mainLightAffectColor.z * _ZWGrayGB.w + _681;

                    // #define fp_c7__m20 float4(0.53596, 98.60019, 153.02422, 0.00)// fp_c7_1._m0[20]
                    // #define _MainLightAffectLumenScale fp_c7__m20.y
                    float _lumenScale01 = clamp( dot( _mainLightAffectColor.xyz * _MainLightAffectLumenScale, float3(0.298911988735198974609375, _ZWGrayGB.zw) ), 0.0, 1.0);
                    
                    
                    color.xyz = _mainLightAffectColor.xyz * _lumenScale01 + _dayOrNightColor.xyz;
                }
                


                col.xyz = color.xyz;
                // col.xyz = _sunHdrSample;
                // col.rg = float2( 0, _up_to_down_01 );
                // col.rg = i.Varying_ColorStarUV_XY_StarUV_ZW.xy;
                // col.xy = frac(float2( _leftForwardRightBackLeft_n0_5_to_0_5, _up_to_down_01 ));
                // col.x = frac(_leftForwardRightBackLeft_n0_5_to_0_5);
                col.w = 1.0;
                
                return col;
            }
            ENDCG
        }
    }
}

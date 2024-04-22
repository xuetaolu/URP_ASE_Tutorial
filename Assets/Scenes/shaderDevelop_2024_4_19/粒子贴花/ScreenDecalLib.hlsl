#ifndef SCREEN_DECAL_LIB_INCLUDED
#define SCREEN_DECAL_LIB_INCLUDED

// viewRay.w 是 surfEyeDepth
void VertexViewRayParams(in float3 pos, out float4 viewRay, out float3 viewRayStartPos)
{
	float4x4 ViewToObjectMatrix = mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V);
    float3 _viewPos = TransformWorldToView(TransformObjectToWorld(pos)); 
    viewRay.w = -_viewPos.z; 
    viewRay.xyz = unity_OrthoParams.w > 0 ? mul((float3x3)ViewToObjectMatrix, float3(0, 0, _viewPos.z)): mul((float3x3)ViewToObjectMatrix, _viewPos.xyz); 
    viewRayStartPos.xyz = unity_OrthoParams.w > 0 ? mul(ViewToObjectMatrix, float4(_viewPos.xy, 0, 1)).xyz : mul(ViewToObjectMatrix, float4(0,0,0,1)).xyz ;
}

float4x4 _WorldToLocalMatrix;
// viewRay.w 是 surfEyeDepth
// 注 particle system 的 UNITY_MATRIX_M 有旋转，但没有缩放，不一定有位移
void VertexViewRayParams(in float3 vertexPos, in float3 centerPos, in float3 sizeXYZ, in float3 rotation3D, out float4 viewRay, out float3 viewRayStartPos)
{
	float4x4 _meshPreRS_matrix_inv = _WorldToLocalMatrix;

	// 粒子生成时，预先依据 transform 的 Rotation Scale 预先把 Mesh 进行缩放旋转，再按 rotation3D sizeXYZ centerPos 布局。
	// 将此预先的 Rotation Scale 定义为 _meshPreRS_matrix, _meshPreRS_matrix 就是 (float3x3)_LocalToWorldMatrix
	// 他的逆 _meshPreRS_matrix_inv 就是 (float3x3)_WorldToLocalMatrix
	// _meshPreRS_matrix_inv 只有旋转和缩放，没有位移
	_meshPreRS_matrix_inv._m03_m13_m23 = 0;

	float size_x_rcp = rcp(sizeXYZ.x);
	float size_y_rcp = rcp(sizeXYZ.y);
	float size_z_rcp = rcp(sizeXYZ.z);

	// scale
	float4x4 S_Inv = {
	    size_x_rcp, 0, 0, 0,
	    0, size_y_rcp, 0, 0,
	    0, 0, size_z_rcp, 0,
	    0, 0, 0, 1
	};

	// position
	float4x4 T_Inv = {
	    1, 0, 0, -centerPos.x,
	    0, 1, 0, -centerPos.y,
	    0, 0, 1, -centerPos.z,
	    0, 0, 0, 1,
	};

	float cos_rot_x = cos(-rotation3D.x);
	float sin_rot_x = sin(-rotation3D.x);
	float cos_rot_y = cos(-rotation3D.y);
	float sin_rot_y = sin(-rotation3D.y);
	float cos_rot_z = cos(-rotation3D.z);
	float sin_rot_z = sin(-rotation3D.z);
	
	// rotation z
	float4x4 Rz_Inv = {
	     cos_rot_z, sin_rot_z, 0, 0,
	    -sin_rot_z, cos_rot_z, 0, 0,
	             0,         0, 1, 0,
	             0,         0, 0, 1
	};
	
	// rotation y
	float4x4 Ry_Inv = {
	     cos_rot_y, 0, sin_rot_y, 0,
	             0, 1,         0, 0,
	    -sin_rot_y, 0, cos_rot_y, 0,
	             0, 0,         0, 1
	};
	
	// rotation x
	float4x4 Rx_Inv = {
	    1,         0,          0, 0,
	    0, cos_rot_x, -sin_rot_x, 0,
	    0, sin_rot_x,  cos_rot_x, 0,
	    0,         0,          0, 1
	};
	

	// // Rzyx = mul(Rz_Inv, mul(Ry_Inv, Rx_Inv));
	// // 直接写出正确等价的结果 Rzyx
	// float4x4 Rzyx;
	// Rzyx = float4x4 (
	// 	 cos_rot_z*cos_rot_y,  cos_rot_z*sin_rot_y*sin_rot_x+sin_rot_z*cos_rot_x,  cos_rot_z*sin_rot_y*cos_rot_x+sin_rot_z*-sin_rot_x, 0,
	// 	-sin_rot_z*cos_rot_y, -sin_rot_z*sin_rot_y*sin_rot_x+cos_rot_z*cos_rot_x, -sin_rot_z*sin_rot_y*cos_rot_x+cos_rot_z*-sin_rot_x, 0,
	// 	          -sin_rot_y,                                cos_rot_y*sin_rot_x,                                 cos_rot_y*cos_rot_x, 0,
	// 	                   0,                                                  0,                                                   0, 1
	// );
	

	
	// 因为 TRS = T * Ry * Rx * Rz * S
	// 所以 TRS_Inv = S_Inv * Rz_Inv * Ry_Inv * Rx_Inv * T_Inv;
	float4x4 _worldToParticleIgnoreMeshPreRS_matrix = mul(S_Inv, mul(Rz_Inv, mul(Ry_Inv, mul(Rx_Inv, T_Inv))));

	// float4x4 _worldToParticleIgnoreMeshPreRS_matrix = {
 //         size_x_rcp * cos_rot_z*cos_rot_y,               cos_rot_z*sin_rot_y*sin_rot_x+sin_rot_z*cos_rot_x,  cos_rot_z*sin_rot_y*cos_rot_x+sin_rot_z*-sin_rot_x,   cos_rot_z*cos_rot_y*-centerPos.x+cos_rot_z*sin_rot_y*sin_rot_x+sin_rot_z*cos_rot_x*-centerPos.y+cos_rot_z*sin_rot_y*cos_rot_x+sin_rot_z*-sin_rot_x*-centerPos.z,
 //                     -sin_rot_z*cos_rot_y, size_y_rcp * -sin_rot_z*sin_rot_y*sin_rot_x+cos_rot_z*cos_rot_x, -sin_rot_z*sin_rot_y*cos_rot_x+cos_rot_z*-sin_rot_x,  -sin_rot_z*cos_rot_y*-centerPos.x+-sin_rot_z*sin_rot_y*sin_rot_x+cos_rot_z*cos_rot_x*-centerPos.y+-sin_rot_z*sin_rot_y*cos_rot_x+cos_rot_z*-sin_rot_x*-centerPos.z,
 //                               -sin_rot_y,                                             cos_rot_y*sin_rot_x,                    size_z_rcp * cos_rot_y*cos_rot_x,  -sin_rot_y*-centerPos.x+cos_rot_y*sin_rot_x*-centerPos.y+cos_rot_y*cos_rot_x*-centerPos.z,
 //                                        0,                                                               0,                                                   0,                                                                                        1
	// };
	
	// 加上考虑逆变换 Mesh 预先的 Rotation Scale，即追加左乘 _particlePreRS_matrix_inv
	float4x4 _worldToMesh_matrix = mul(_meshPreRS_matrix_inv, _worldToParticleIgnoreMeshPreRS_matrix);
	// _worldToMesh_matrix = _worldToParticleIgnoreMeshPreRS_matrix;
	// _worldToPartical_matrix =_worldToObject_matrix;
	
	float4x4 ViewToObjectMatrix = mul(_worldToMesh_matrix, UNITY_MATRIX_I_V);
    float3 _viewPos = TransformWorldToView(TransformObjectToWorld(vertexPos)); 
    viewRay.w = -_viewPos.z; 
    viewRay.xyz = unity_OrthoParams.w > 0 ? mul((float3x3)ViewToObjectMatrix, float3(0, 0, _viewPos.z)): mul((float3x3)ViewToObjectMatrix, _viewPos.xyz); 
    viewRayStartPos.xyz = unity_OrthoParams.w > 0 ? mul(ViewToObjectMatrix, float4(_viewPos.xy, 0, 1)).xyz : mul(ViewToObjectMatrix, float4(0,0,0,1)).xyz ;
}

float LinearEyeDepthPerspOrOrtho(float rawDepth)
{
	float persp = LinearEyeDepth(rawDepth, _ZBufferParams); 
    float ortho = (_ProjectionParams.z-_ProjectionParams.y)*(1-rawDepth)+_ProjectionParams.y; 
    float _linearEyeDepth = unity_OrthoParams.w > 0 ? ortho : persp; 
	return _linearEyeDepth;
}

#endif
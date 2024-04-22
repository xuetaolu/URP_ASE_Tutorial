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
	// 因为 TRS = T * M_rs * Ry * Rx * Rz * S
	// 粒子生成过程
	// 1. 计算粒子的 T R S，
	// 2. 顶点位置/顶点中心 位置 先做旋转缩放变化 R S
	// 3. 再依据 Transform 的 Rotation Scale 做变换 M_rs
	// 4. 再做 T 变换，注: 这个 T 记录在 centerPos，且已经考虑了 Transform 的 Rotation Scale 的作用

	// 故其逆矩阵 TRS_Inv = S_Inv * Rz_Inv * Ry_Inv * Rx_Inv * M_rs_Inv * T_Inv;

	// Transform 的 M_rs 的逆矩阵，没有位移，只有旋转和缩放
	float4x4 M_rs_Inv = _WorldToLocalMatrix;
	M_rs_Inv._m03_m13_m23 = 0;

	// scale
	float4x4 S_Inv = {
	    rcp(sizeXYZ.x), 0, 0, 0,
	    0, rcp(sizeXYZ.y), 0, 0,
	    0, 0, rcp(sizeXYZ.z), 0,
	    0, 0, 0, 1
	};

	// position
	float4x4 T_Inv = {
	    1, 0, 0, -centerPos.x,
	    0, 1, 0, -centerPos.y,
	    0, 0, 1, -centerPos.z,
	    0, 0, 0, 1,
	};

	// rotation z
	float4x4 Rz_Inv = {
	    cos(-rotation3D.z), sin(-rotation3D.z), 0, 0,
	    - sin(-rotation3D.z), cos(-rotation3D.z), 0, 0,
	    0, 0, 1, 0,
	    0, 0, 0, 1
	};
	
	// rotation x
	float4x4 Rx_Inv = {
	    1, 0, 0, 0,
	    0, cos(-rotation3D.x), -sin(-rotation3D.x), 0,
	    0, sin(-rotation3D.x), cos(-rotation3D.x), 0,
	    0, 0, 0, 1
	};
	
	// rotation y
	float4x4 Ry_Inv = {
	    cos(-rotation3D.y), 0, sin(-rotation3D.y), 0,
	    0, 1, 0, 0,
	    - sin(-rotation3D.y), 0, cos(-rotation3D.y), 0,
	    0, 0, 0, 1
	};
	
	// 因为 TRS = T * M_rs * Ry * Rx * Rz * S
	// 所以 TRS_Inv = S_Inv * Rz_Inv * Ry_Inv * Rx_Inv * M_rs_Inv * T_Inv;
	float4x4 _worldToMesh_matrix;
	_worldToMesh_matrix = mul(S_Inv, mul(Rz_Inv, mul(Ry_Inv, mul(Rx_Inv, mul(M_rs_Inv, T_Inv)))));
	
	float4x4 ViewToMeshMatrix = mul(_worldToMesh_matrix, UNITY_MATRIX_I_V);
    float3 _viewPos = TransformWorldToView(vertexPos); 
    viewRay.w = -_viewPos.z; 
    viewRay.xyz = unity_OrthoParams.w > 0 ? mul((float3x3)ViewToMeshMatrix, float3(0, 0, _viewPos.z)): mul((float3x3)ViewToMeshMatrix, _viewPos.xyz); 
    viewRayStartPos.xyz = unity_OrthoParams.w > 0 ? mul(ViewToMeshMatrix, float4(_viewPos.xy, 0, 1)).xyz : mul(ViewToMeshMatrix, float4(0,0,0,1)).xyz ;
}

float LinearEyeDepthPerspOrOrtho(float rawDepth)
{
	float persp = LinearEyeDepth(rawDepth, _ZBufferParams); 
    float ortho = (_ProjectionParams.z-_ProjectionParams.y)*(1-rawDepth)+_ProjectionParams.y; 
    float _linearEyeDepth = unity_OrthoParams.w > 0 ? ortho : persp; 
	return _linearEyeDepth;
}

#endif
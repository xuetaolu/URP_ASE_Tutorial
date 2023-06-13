#ifndef COMPAT_VERTEX_IN_OUT
	#define COMPAT_VERTEX_IN_OUT

	struct in_vertex
	{
		float4 position : POSITION;
		float2 texCoord : TEXCOORD0;
		float2 t1 : TEXCOORD1;
	};

	#define COMPAT_IN_VERTEX in_vertex VIN
	#define COMPAT_IN_FRAGMENT out_vertex VOUT

#endif // COMPAT_VERTEX_IN_OUT
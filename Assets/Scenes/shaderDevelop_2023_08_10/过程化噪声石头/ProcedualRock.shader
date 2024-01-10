Shader "Unlit/ProcedualRock"
{
    Properties
    {
        [Toggle] _ShowNoise ("_ShowNoise", float) = 0
        _BaseColor ("_BaseColor", Color) = (0.42,0.3,0.2)
        _BaseColor2 ("_BaseColor2", Color) = (0.51,0.41,0.32)
        _DiffuseStrength ("_DiffuseStrength", Range(0, 1)) = 1.0
        _SpecularStrength ("_SpecularStrength", Range(0, 1)) = 1.0
        _EnvironmentStrength ("_EnviromentStrength", Range(0, 1)) = 1.0
        _Noise3DWorldPosScale ("_Noise3DWorldPosScale", Range(0.1, 10)) = 1.0
        _NormalDisturb ("_NormalDisturb", Range(0.1, 10)) = 1.0
        
        _NoiseSpeed ("_NoiseSpeed", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            float _ShowNoise;
            float3 _BaseColor;
            float3 _BaseColor2;
            float _DiffuseStrength;
            float _SpecularStrength;
            float _EnvironmentStrength;
            float _Noise3DWorldPosScale;
            float _NormalDisturb;
            float3 _NoiseSpeed;

            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

// Fork of "Wet stone" by TDM. https://shadertoy.com/view/ldSSzV
// 2024-01-10 06:05:15

/*
"Wet stone" by Alexander Alekseev aka TDM - 2014
License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
Contact: tdmaav@gmail.com
*/
            static const int NUM_STEPS = 32;
            static const int AO_SAMPLES = 4;
            static const float2 AO_PARAM = float2(1.2, 3.5);
            static const float2 CORNER_PARAM = float2(0.25, 40.0);
            static const float INV_AO_SAMPLES = 1.0 / float(AO_SAMPLES);
            static const float TRESHOLD 	= 0.1;
            static const float EPSILON 	= 0.001;//1e-3;
            static const float LIGHT_INTENSITY = 0.25;
            static const float3 RED 		= float3(1.0,0.7,0.7) * LIGHT_INTENSITY;
            static const float3 ORANGE 	= float3(1.0,0.67,0.43) * LIGHT_INTENSITY;
            static const float3 BLUE 	= float3(0.54,0.77,1.0) * LIGHT_INTENSITY;
            static const float3 WHITE 	= float3(1.2,1.07,0.98) * LIGHT_INTENSITY;

            static const float DISPLACEMENT = 0.1;

            // math
            // matrix3x3 fromEuler(float3 ang) {
	           //  float2 a1 = float2(sin(ang.x),cos(ang.x));
            //     float2 a2 = float2(sin(ang.y),cos(ang.y));
            //     float2 a3 = float2(sin(ang.z),cos(ang.z));
            //     matrix3x3 m;
            //     m[0] = float3(a1.y*a3.y+a1.x*a2.x*a3.x,a1.y*a2.x*a3.x+a3.y*a1.x,-a2.y*a3.x);
	           //  m[1] = float3(-a2.y*a1.x,a1.y*a2.y,a2.x);
	           //  m[2] = float3(a3.y*a1.x*a2.x+a1.y*a3.x,a1.x*a3.x-a1.y*a3.y*a2.x,a2.y*a3.y);
	           //  return m;
            // }
            float3 saturation(float3 c, float t) {
                return lerp((dot(c,float3(0.2126,0.7152,0.0722))),c,t);
            }
            float hash11(float p) {
                return frac(sin(p * 727.1)*435.545);
            }
            float hash12(float2 p) {
	            float h = dot(p,float2(127.1,311.7));	
                return frac(sin(h)*437.545);
            }
            float3 hash31(float p) {
	            float3 h = float3(127.231,491.7,718.423) * p;	
                return frac(sin(h)*435.543);
            }

            // 3d noise
            float noise_3(in float3 p) {
                float3 i = floor(p);
                float3 f = frac(p);	
	            float3 u = f*f*(3.0-2.0*f);
                
                float2 ii = i.xy + i.z * (5.0);
                float a = hash12( ii + float2(0.0,0.0) );
	            float b = hash12( ii + float2(1.0,0.0) );    
                float c = hash12( ii + float2(0.0,1.0) );
	            float d = hash12( ii + float2(1.0,1.0) ); 
                float v1 = lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
                
                ii += (5.0);
                a = hash12( ii + float2(0.0,0.0) );
	            b = hash12( ii + float2(1.0,0.0) );    
                c = hash12( ii + float2(0.0,1.0) );
	            d = hash12( ii + float2(1.0,1.0) );
                float v2 = lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
                    
                return max(lerp(v1,v2,u.z),0.0);
            }

            // fBm
            float fbm3(float3 p, float a, float f) {
                return noise_3(p);
            }

            float fbm3_high(float3 p, float a, float f) {
                float ret = 0.0;    
                float amp = 1.0;
                float frq = 1.0;
                for(int i = 0; i < 5; i++) {
                    float n = pow(noise_3(p * frq),2.0);
                    ret += n * amp;
                    frq *= f;
                    amp *= a * (pow(n,0.2));
                }
                return ret;
            }



            // Normalize that account for vectors with zero length
            float3 SafeNormalize(float3 inVec)
            {
                float dp3 = max(0.0001, dot(inVec, inVec));
                return inVec * rsqrt(dp3);
            }
            
            void Unity_NormalFromHeight_World_float(float In, float Strength, float3 Position, float3 worldNormal/*float3x3 TangentMatrix*/, out float3 Out)
            {
                
                        #if defined(SHADER_STAGE_RAY_TRACING)
                        #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                        #endif
                float3 worldDerivativeX = ddx(Position);
                float3 worldDerivativeY = ddy(Position);
            
                float3 crossX = cross(worldNormal/*TangentMatrix[2].xyz*/, worldDerivativeX);
                float3 crossY = cross(worldDerivativeY, worldNormal/*TangentMatrix[2].xyz*/);
                float d = dot(worldDerivativeX, crossY);
                float sgn = d < 0.0 ? (-1.0f) : 1.0f;
                float surface = sgn / max(0.000000000000001192093f, abs(d));
            
                float dHdx = ddx(In);
                float dHdy = ddy(In);
                float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
                Out = SafeNormalize(worldNormal/*TangentMatrix[2].xyz*/ - (Strength * surfGrad));
            }

            float3 Unity_NormalFromHeight_World(float In, float3 worldPos, float3 worldNormal)
            {
                float3 Out;
                Unity_NormalFromHeight_World_float(In, 0.5, worldPos, worldNormal, Out);
                return Out;
                // float3 worldDirivativeX = ddx(worldPos * 100);
                // float3 worldDirivativeY = ddy(worldPos * 100);
                // float3 crossX = cross(worldNormal, worldDirivativeX);
                // float3 crossY = cross(worldNormal, worldDirivativeY);
                // float3 d = abs(dot(crossY, worldDirivativeX));
                // float3 inToNormal = ((((In + ddx(In)) - In) * crossY) + (((In + ddy(In)) - In) * crossX)) * sign(d);
                // inToNormal.y *= -1.0;
                // float3 output = normalize((d * worldNormal) - inToNormal);
                // return output;
            }

            float diffuse(float3 n,float3 l,float p) { return pow(max(dot(n,l),0.0),p); }
            float specular(float3 n,float3 l,float3 e,float s) {    
                float nrm = (s + 8.0) / (3.1415 * 8.0);
                return pow(max(dot(reflect(e,n),l),0.0),s) * nrm;
            }

            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD2;
                float3 objPos : TEXCOORD3;
                float2 uv : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.objPos = v.vertex;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);

                float3 _worldPos = i.worldPos;
                float3 _worldNormal = normalize(i.worldNormal);
                
                float _noise3D = fbm3_high(i.objPos*4.0 * _Noise3DWorldPosScale,0.4,2.96);
                // _noise3D = 1-length(i.uv *2 - 1);

                float3 _fixWorldNormal = Unity_NormalFromHeight_World(_noise3D*0.5*_NormalDisturb, _worldPos, _worldNormal);

                col.xyz = _worldNormal;
                
                col.xyz = _fixWorldNormal;
                // col.xyz = _noise3D;

                float c = 1-_noise3D;
                float3 p = _worldPos;
                float3 n = _fixWorldNormal;
                float3 l = _WorldSpaceLightPos0;
                float3 e = -normalize(_WorldSpaceCameraPos - _worldPos);
                c = min(c + pow(noise_3(float3(p.x*20.0,p.x*20.0,p.z*20.0)),70.0) * 8.0, 1.0);
                float ic = pow(1.0-c,0.5);
                float3 base = _BaseColor/*float3(0.42,0.3,0.2)*/;
                float3 sand = _BaseColor2/*float3(0.51,0.41,0.32)*/;
                float3 color = lerp(base,sand,c);

                // float f = pow(1.0 - max(dot(n,-e),0.0), 5.0) * 0.75 * ic;
                float3 _lighting = 0;
                
                _lighting += float3(diffuse(n,l,1.0) * _LightColor0.xyz/*WHITE*/) * _DiffuseStrength;
                _lighting += float3(specular(n,l,e,8.0) * _LightColor0.xyz/*WHITE*/ * 1.5 ) * _SpecularStrength;
                _lighting += ShadeSH9(half4(_fixWorldNormal, 1.0)) * _EnvironmentStrength;
                color *= _lighting;
                
                // n = normalize(n - normalize(p) * 0.4);    
                // color += float3(specular(n,l,e,80.0) * WHITE * 1.5 * ic);    
                // color = lerp(color,(1.0),f); 
                
                // color *= sqrt(abs(p.y*0.5+0.5)) * 0.4 + 0.6;
                // color *= (n.y * 0.5 + 0.5) * 0.4 + 0.6; 
                
                col.xyz = color;
                if (_ShowNoise)
                    col.xyz = _noise3D;
                return col;
            }
            ENDCG
        }
    }
}

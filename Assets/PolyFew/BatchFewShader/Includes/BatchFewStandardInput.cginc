// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// This code in no way belongs to BrainFailProductions. I have just made a small change to make
// it work with texture arrays


#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "Includes/BatchFewStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "Includes/BatchFewStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || defined(_PARALLAXMAP) || defined(_POM))
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif

//---------------------------------------
half        _Cutoff;

UNITY_DECLARE_TEX2DARRAY(_MainTex);
float4      _MainTex_ST;

sampler2D _AttrImg;
float4    _AttrImg_TexelSize;

UNITY_DECLARE_TEX2DARRAY(_DetailAlbedoMap);
float4      _DetailAlbedoMap_ST;

UNITY_DECLARE_TEX2DARRAY(_BumpMap);

UNITY_DECLARE_TEX2DARRAY(_DetailMask);
UNITY_DECLARE_TEX2DARRAY(_DetailNormalMap);

UNITY_DECLARE_TEX2DARRAY(_SpecGlossMap);
UNITY_DECLARE_TEX2DARRAY(_MetallicGlossMap);

UNITY_DECLARE_TEX2DARRAY(_OcclusionMap);

UNITY_DECLARE_TEX2DARRAY(_ParallaxMap);
half        _Parallax;
half        _UVSec;

UNITY_DECLARE_TEX2DARRAY(_EmissionMap);

sampler2D _DetailAlbedoSingle;
sampler2D _DetailNormalSingle;

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half4 color     : COLOR;
    half3 normal    : NORMAL;
    float4 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID

};

float4 SampleAttrImg(float prop, float index)
{
   return tex2Dlod(_AttrImg, float4((prop + 0.5) * _AttrImg_TexelSize.x, (index + 0.5) * _AttrImg_TexelSize.y, 0, 0));
}

float4 TexCoords(VertexInput v)
{
    
    float4 uvScaleOffset = SampleAttrImg(0, v.color.r * 255);

    float4 texcoord;
    texcoord.z = v.color.a * 255;
    texcoord.w = v.color.r * 255;
    texcoord.xy = v.uv0.xy * (uvScaleOffset.xy+1) + uvScaleOffset.zw;

    return texcoord;
}

half DetailMask(float3 uv)
{
    return UNITY_SAMPLE_TEX2DARRAY(_DetailMask, uv).a;
}


half3 Albedo(float4 texcoords, half4 attribs)
{
    half3 albedo = UNITY_SAMPLE_TEX2DARRAY (_MainTex, texcoords.xyz).rgb * SampleAttrImg(2, texcoords.w).rgb;
#if _DETAIL
    #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords);
    #endif
    //float4 uvmod = SampleAttrImg(5, texcoords.w);
    texcoords.xy = texcoords.xy * SampleAttrImg(5, texcoords.w).xy + SampleAttrImg(5, texcoords.w).zw;
    half3 detailAlbedo = UNITY_SAMPLE_TEX2DARRAY (_DetailAlbedoMap, texcoords.xyz).rgb;
    #if _DETAIL_MULX2
        albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
#elif _DETAIL_SINGLE
   #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords);
    #endif

    //float4 uvmod = SampleAttrImg(5, texcoords.w);
    
    texcoords.xy = texcoords.xy * SampleAttrImg(5, texcoords.w).xy + SampleAttrImg(5, texcoords.w).zw;
    half3 detailAlbedo = tex2D (_DetailAlbedoSingle, texcoords.xy).rgb;
    albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);

#endif
    return albedo;
}

half Alpha(float4 uv)
{
    return UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv.xyz).a;
}

half Occlusion(float4 uv, half4 attribs)
{
      return UNITY_SAMPLE_TEX2DARRAY(_OcclusionMap, uv.xyz).g * attribs.y;
}

half4 SpecularGloss(float4 uv, half4 attribs)
{
   half4 sg = attribs.wwzz;
   #ifdef _SPECGLOSSMAP
       #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
           sg.rgb = UNITY_SAMPLE_TEX2DARRAY(_SpecGlossMap, uv.xyz).rgb;
           sg.a = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv.xyz).a * attribs.z;
       #else
           sg = UNITY_SAMPLE_TEX2DARRAY(_SpecGlossMap, uv.xyz);
       #endif
       sg.a *= SampleAttrImg(4, uv.w).a;
   #else
       sg.rgb = SampleAttrImg(4, uv.w).rgb;
       #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
          sg.a = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv.xyz).a * attribs.z;
       #else
          sg.a = SampleAttrImg(1, uv.w).b;
       #endif
   #endif

   return sg;
}


half2 MetallicGloss(float4 uv, half4 attribs)
{
    half2 mg = attribs.wz;

    #ifdef _METALLICGLOSSMAP
        #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            mg.r = UNITY_SAMPLE_TEX2DARRAY(_MetallicGlossMap, uv.xyz).r;
            mg.g = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv.xyz).a * attribs.z;
        #else
            mg = UNITY_SAMPLE_TEX2DARRAY(_MetallicGlossMap, uv.xyz).ra;
            mg.g *= attribs.z;
        #endif
    #else
        mg.r = attribs.w;
        #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            mg.g = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv.xyz).a;
        #else
            mg.g = attribs.z;
        #endif
    #endif

    return mg;
}

half3 Emission(float4 uv)
{
#if _EMISSION
    return UNITY_SAMPLE_TEX2DARRAY(_EmissionMap, uv.xyz).rgb;
#endif
#if _EMISSION_COLOR
    return SampleAttrImg(3, uv.w).xyz;
#endif
return half3(0,0,0);
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords, half4 attribs)
{
    half3 normalTangent = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY (_BumpMap, texcoords.xyz), attribs.r);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords);
        
    texcoords.xy = texcoords.xy * SampleAttrImg(5, texcoords.w).xy + SampleAttrImg(5, texcoords.w).zw;
    half3 detailNormalTangent = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY (_DetailNormalMap, texcoords.xyz), SampleAttrImg(6, texcoords.w).r);
    #if _DETAIL_LERP
        normalTangent = lerp(
            normalTangent,
            detailNormalTangent,
            mask);
    #else
        normalTangent = lerp(
            normalTangent,
            BlendNormals(normalTangent, detailNormalTangent),
            mask);
    #endif
#endif

#if _DETAIL_SINGLE && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords);
    //float uvmod = SampleAttrImg(5, texcoords.w);
    texcoords.xy = texcoords.xy * SampleAttrImg(5, texcoords.w).xy + SampleAttrImg(5, texcoords.w).zw;
    half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalSingle, texcoords.xy), SampleAttrImg(6, texcoords.w).r);
    #if _DETAIL_LERP
        normalTangent = lerp(
            normalTangent,
            detailNormalTangent,
            mask);
    #else
        normalTangent = lerp(
            normalTangent,
            BlendNormals(normalTangent, detailNormalTangent),
            mask);
    #endif
#endif

    return normalTangent;
}
#endif


#if (UNITY_VERSION >= 201810 && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (SHADER_TARGET_SURFACE_ANALYSIS && !SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))) || (UNITY_VERSION < 201810 && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL))) 
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
#elif defined(SHADER_API_D3D9)
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) half4(0,1,0,0) 
#elif defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) texCUBEgrad (tex,coord,float3(dx.x,dx.y,0),float3(dy.x,dy.y,0))
#elif defined(SHADER_API_GLES)
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) half4(1,1,0,0)
#elif defined(SHADER_API_D3D11_9X)
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) half4(0,1,1,0) 
#else
  #define BACTHFEW_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) half4(0,0,1,0) 
#endif


#if (UNITY_VERSION >= 201810 && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (SHADER_TARGET_SURFACE_ANALYSIS && !SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))) || (UNITY_VERSION < 201810 && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL))) 
     #define BATCHFEW_SAMPLE_TEX2D_LOD(tex,coord, lod) tex.SampleLevel (sampler##tex,coord, lod)
     #define BATCHFEW_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord, lod) tex.SampleLevel (sampler##samplertex,coord, lod)
#else
     #define BATCHFEW_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2D (tex,coord,0,lod)
     #define BATCHFEW_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord,lod) tex2D (tex,coord,0,lod)
#endif


float ComputeMipLevel(float2 uv, float2 textureSize)
{
   uv *= textureSize;
   float2  dx_vtc        = ddx(uv);
   float2  dy_vtc        = ddy(uv);
   float delta_max_sqr   = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
   return 0.5 * log2(delta_max_sqr);
}

int _ParallaxSteps;

float4 _ParallaxMap_TexelSize;

inline float2 POM( float2 uvs,
                   float3 normalWorld, float3 viewWorld, float3 viewDirTan, 
                   float parallax, float texIndex)
{
   float curvFix = 1.0;

   float2 curv = float2(0, 0);
   float refPlane = 0;

   float3 result = 0;
   int stepIndex = 0;
   //int numSteps = ( int )lerp( maxSamples, minSamples, dot( normalWorld, viewWorld ) );
   int numSteps = _ParallaxSteps; // artifacts when using variable step slope above
   float layerHeight = 1.0 / numSteps;
   float2 plane = parallax * ( viewDirTan.xy / viewDirTan.z );
   uvs += refPlane * plane;
   float2 deltaTex = -plane * layerHeight;
   float2 prevTexOffset = 0;
   float prevRayZ = 1.0f;
   float prevHeight = 0.0f;
   float2 currTexOffset = deltaTex;
   float currRayZ = 1.0f - layerHeight;
   float currHeight = 0.0f;
   float intersection = 0;
   float2 finalTexOffset = 0;
   float mipLevel = ComputeMipLevel(uvs, _ParallaxMap_TexelSize.zw);


   while ( stepIndex < numSteps + 1 )
   {
      result.z = dot( curv, currTexOffset * currTexOffset );

      currHeight = BATCHFEW_SAMPLE_TEX2D_LOD(_ParallaxMap, float3(uvs + currTexOffset, texIndex), mipLevel ).r * ( 1 - result.z );

      if ( currHeight > currRayZ )
      {
         stepIndex = numSteps + 1;
      }
      else
      {
         stepIndex++;
         prevTexOffset = currTexOffset;
         prevRayZ = currRayZ;
         prevHeight = currHeight;
         currTexOffset += deltaTex;
         currRayZ -= layerHeight * ( 1 - result.z ) * (1+curvFix);
      }
   }
   int sectionSteps = 10;
   int sectionIndex = 0;
   float newZ = 0;
   float newHeight = 0;
   while ( sectionIndex < sectionSteps )
   {
      intersection = ( prevHeight - prevRayZ ) / ( prevHeight - currHeight + currRayZ - prevRayZ );
      finalTexOffset = prevTexOffset + intersection * deltaTex;
      newZ = prevRayZ - intersection * layerHeight;

      newHeight = BATCHFEW_SAMPLE_TEX2D_LOD(_ParallaxMap, float3(uvs + finalTexOffset, texIndex), mipLevel ).r * ( 1 - result.z );
      

      if ( newHeight > newZ )
      {
         currTexOffset = finalTexOffset;
         currHeight = newHeight;
         currRayZ = newZ;
         deltaTex = intersection * deltaTex;
         layerHeight = intersection * layerHeight;
      }
      else
      {
         prevTexOffset = finalTexOffset;
         prevHeight = newHeight;
         prevRayZ = newZ;
         deltaTex = ( 1 - intersection ) * deltaTex;
         layerHeight = ( 1 - intersection ) * layerHeight;
      }
      sectionIndex++;
   }
   #ifdef UNITY_PASS_SHADOWCASTER
   if ( unity_LightShadowBias.z == 0.0 )
   {
   #endif
      if ( result.z > 1 )
         clip( -1 );
   #ifdef UNITY_PASS_SHADOWCASTER
   }
   #endif
   return uvs + finalTexOffset;
}


float4 Parallax (float4 texcoords, half3 i_viewDirForParallax, float3 i_eyeVec, float4 tangentToWorld[3])
{
#if defined(_PARALLAXMAP)

    half h = UNITY_SAMPLE_TEX2DARRAY (_ParallaxMap, texcoords.xyz).g;
    
    float2 offset = ParallaxOffset1Step (h, SampleAttrImg(6, texcoords.w).b, i_viewDirForParallax);
    return float4(texcoords.xy + offset, texcoords.zw);
#endif
#if defined(_POM)
    float parallax = SampleAttrImg(6, texcoords.w).b;
    half3 normal = (0,0,1);


    half3 worldNormal = half3(dot(tangentToWorld[0], normal), dot(tangentToWorld[1],normal), dot(tangentToWorld[2],normal));

    float2 offset = POM(texcoords.xy, worldNormal, i_eyeVec, i_viewDirForParallax, parallax, texcoords.z); 
    return float4(offset, texcoords.zw);
#endif
return texcoords;
}


#endif // UNITY_STANDARD_INPUT_INCLUDED

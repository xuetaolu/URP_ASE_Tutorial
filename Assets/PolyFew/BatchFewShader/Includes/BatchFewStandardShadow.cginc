// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// This code in no way belongs to BrainFailProductions. I have just made a small change to make
// it work with texture arrays

#ifndef UNITY_STANDARD_SHADOW_INCLUDED
#define UNITY_STANDARD_SHADOW_INCLUDED


#include "UnityCG.cginc"
#include "Includes/BatchFewStandardConfig.cginc"
#include "Includes/BatchFewStandardUtils.cginc"

#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
    #define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
#define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif


half        _Cutoff;
UNITY_DECLARE_TEX2DARRAY(_MainTex);
float4      _MainTex_ST;
#ifdef UNITY_STANDARD_USE_DITHER_MASK
sampler3D   _DitherMaskLOD;
#endif

// Handle PremultipliedAlpha from Fade or Transparent shading mode
#ifdef _SPECGLOSSMAP
UNITY_DECLARE_TEX2DARRAY(_SpecGlossMap);
#endif
#ifdef _METALLICGLOSSMAP
UNITY_DECLARE_TEX2DARRAY(_MetallicGlossMap);
#endif

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && (defined(_PARALLAXMAP) || defined(_POM))
UNITY_DECLARE_TEX2DARRAY(_ParallaxMap);
#endif

half MetallicSetup_ShadowGetOneMinusReflectivity(half3 uv)
{
    half metallicity = 0;
    #ifdef _METALLICGLOSSMAP
        metallicity = UNITY_SAMPLE_TEX2DARRAY(_MetallicGlossMap, uv).r;
    #endif
    return OneMinusReflectivityFromMetallic(metallicity);
}

half SpecularSetup_ShadowGetOneMinusReflectivity(half3 uv)
{
    half3 specColor = half3(0,0,0);
    #ifdef _SPECGLOSSMAP
        specColor = UNITY_SAMPLE_TEX2DARRAY(_SpecGlossMap, uv).rgb;
    #endif
    return (1 - SpecularStrength(specColor));
}

// SHADOW_ONEMINUSREFLECTIVITY(): workaround to get one minus reflectivity based on UNITY_SETUP_BRDF_INPUT
#define SHADOW_JOIN2(a, b) a##b
#define SHADOW_JOIN(a, b) SHADOW_JOIN2(a,b)
#define SHADOW_ONEMINUSREFLECTIVITY SHADOW_JOIN(UNITY_SETUP_BRDF_INPUT, _ShadowGetOneMinusReflectivity)

struct VertexInput
{
    float4 vertex   : POSITION;
    float3 normal   : NORMAL;
    float4 uv0      : TEXCOORD0;
    float4 color    : COLOR;
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS) && (defined(_PARALLAXMAP) || defined(_POM))
        half4 tangent   : TANGENT;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float4 tex : TEXCOORD1;

        #if (defined(_PARALLAXMAP) || defined(_POM))
            half4 tangentToWorldAndParallax[3]: TEXCOORD2;  // [3x3:tangentToWorld | 1x3:viewDirForParallax]
        #endif
    #endif
};
#endif

#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
struct VertexOutputStereoShadowCaster
{
    UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
// some platforms, and then things don't go well.


void vertShadowCaster (VertexInput v,
    #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    out VertexOutputShadowCaster o,
    #endif
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
    out VertexOutputStereoShadowCaster os,
    #endif
    out float4 opos : SV_POSITION)
{
    UNITY_SETUP_INSTANCE_ID(v);
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
    #endif
    TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex.xy = TRANSFORM_TEX(v.uv0.xy * (v.uv0.zw+1), _MainTex);
        o.tex.z = v.color.a * 255;
        o.tex.w = v.color.r * 255;

        #if (defined(_PARALLAXMAP) || defined(_POM))
            TANGENT_SPACE_ROTATION;
            half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
            o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
            o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
            o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
        #endif
    #endif
}

sampler2D _AttrImg;
float4 _AttrImg_TexelSize;
float4 SampleAttrImg(float prop, float index)
{
   return tex2Dlod(_AttrImg, float4(prop * _AttrImg_TexelSize.x, index * _AttrImg_TexelSize.y, 0, 0));
}

half4 fragShadowCaster (
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    VertexOutputShadowCaster i
#endif
#ifdef UNITY_STANDARD_USE_DITHER_MASK
    , UNITY_VPOS_TYPE vpos : VPOS
#endif
    ) : SV_Target
{
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        #if (defined(_PARALLAXMAP) || defined(_POM)) && (SHADER_TARGET >= 30)
            //On d3d9 parallax can also be disabled on the fwd pass when too many    sampler are used. See EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT. Ideally we should account for that here as well.
            half3 viewDirForParallax = normalize( half3(i.tangentToWorldAndParallax[0].w,i.tangentToWorldAndParallax[1].w,i.tangentToWorldAndParallax[2].w) );
            fixed h = UNITY_SAMPLE_TEX2DARRAY (_ParallaxMap, i.tex).g;
            half2 offset = ParallaxOffset1Step (h, SampleAttrImg(6, i.tex.w).b, viewDirForParallax);
            i.tex.xy += offset;
        #endif

        half alpha = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.tex).a;
        #if defined(_ALPHATEST_ON)
            clip (alpha - _Cutoff);
        #endif
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #if defined(_ALPHAPREMULTIPLY_ON)
                half outModifiedAlpha;
                PreMultiplyAlpha(half3(0, 0, 0), alpha, SHADOW_ONEMINUSREFLECTIVITY(i.tex), outModifiedAlpha);
                alpha = outModifiedAlpha;
            #endif
            #if defined(UNITY_STANDARD_USE_DITHER_MASK)
                // Use dither mask for alpha blended shadows, based on pixel position xy
                // and alpha level. Our dither texture is 4x4x16.
                half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
                clip (alphaRef - 0.01);
            #else
                clip (alpha - _Cutoff);
            #endif
        #endif
    #endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

    SHADOW_CASTER_FRAGMENT(i)
}

#endif // UNITY_STANDARD_SHADOW_INCLUDED

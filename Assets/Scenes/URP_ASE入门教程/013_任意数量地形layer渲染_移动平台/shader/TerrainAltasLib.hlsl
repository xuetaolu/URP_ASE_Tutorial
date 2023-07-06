#ifndef TERRAIN_ALTAS_LIB_INCLUDED
#define TERRAIN_ALTAS_LIB_INCLUDED

#define MAX_LAYER_COUNT 16
// #define MAIN_WEIGHT_COUNT 2

#define SetArrayFourAt(weights, startIndex, rgba) \
    weights[startIndex]   = rgba.r; \
    weights[startIndex+1] = rgba.g; \
    weights[startIndex+2] = rgba.b; \
    weights[startIndex+3] = rgba.a; \

#define GetArrayFourAt(weights, startIndex) \
    float4(weights[startIndex], \
    weights[startIndex+1], \
    weights[startIndex+2], \
    weights[startIndex+3]);

uniform float4 _LayerTiles1;
uniform float4 _LayerTiles2;
uniform float4 _LayerTiles3;
uniform float4 _LayerTiles4;

float GetTileScaleAtIndex(int index)
{
    float array[MAX_LAYER_COUNT];

    SetArrayFourAt(array,  0,  _LayerTiles1);
    SetArrayFourAt(array,  4,  _LayerTiles2);
    SetArrayFourAt(array,  8,  _LayerTiles3);
    SetArrayFourAt(array, 12,  _LayerTiles4);

    return array[index];
}

// cosine based palette, 4 vec3 params
// https://iquilezles.org/articles/palettes/
float3 IQPalette( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
{
    return a + b*cos( 6.28318*(c*t+d) );
}

float invLerp(float from, float to, float value){
  return (value - from) / (to - from);
}

float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value){
  float rel = invLerp(origFrom, origTo, value);
  return lerp(targetFrom, targetTo, rel);
}

#endif
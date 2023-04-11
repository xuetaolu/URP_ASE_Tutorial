#ifndef MY_NODE_LIBRARY
#define MY_NODE_LIBRARY
// 参考：
// https://docs.unity3d.com/Packages/com.unity.shadergraph@16.0/manual/Blackbody-Node.html
// http://www.vendian.org/mncharity/dir3/blackbody/
void Unity_Blackbody_float(float Temperature, out float3 Out)
{
    float3 color = float3(255.0, 255.0, 255.0);
    color.x = 56100000. * pow(Temperature,(-3.0 / 2.0)) + 148.0;
    color.y = 100.04 * log(Temperature) - 623.6;
    if (Temperature > 6500.0) color.y = 35200000.0 * pow(Temperature,(-3.0 / 2.0)) + 184.0;
    color.z = 194.18 * log(Temperature) - 1448.6;
    color = clamp(color, 0.0, 255.0)/255.0;
    if (Temperature < 1000.0) color *= Temperature/1000.0;
    Out = color;
}

#endif
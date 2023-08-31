#define _45__m0 0.11   // _45._m0
#define _45__m1 1.00   // _45._m1
#define _45__m2 0.0123 // _45._m2
#define _45__m3 1.00   // _45._m3
#define _45__m4 1.00   // _45._m4

sampler2D _NoiseMapRGB;
sampler2D _MaskMapRGBA;

fixed4 frag (v2f i) : SV_Target
{
    float4 Output_0;
    float4 Output_1;
    
    float _25;
    float3 _27;
    float2 _30;
    float2 _31;
    float4 _33;
    float3 _34;
    float _35;
    bool _38;
    float3 _39;
    float _40;
    float _41;
    float _42;

    
    _25 = _45__m1 * 10.0;
    _25 = clamp(_25, 0.0, 1.0);
    _35 = (_25 * (-2.0)) + 3.0;
    _25 *= _25;
    _25 *= _35;
    _25 *= _45__m3;
    _27.x = i.Varying_RelativeToRoleDirXYZ_Angle1_n1.w + 0.100000001490116119384765625;
    _27.x *= 5.0;
    _27.x = clamp(_27.x, 0.0, 1.0);
    _39.x = (_27.x * (-2.0)) + 3.0;
    _27.x *= _27.x;
    _27.x *= _39.x;
    float2 _128 = float2(i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.y + i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.w, i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.z + i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.w);
    _39 = float3(_128.x, _128.y, _39.z);
    float2 _134 = min(_39.xy, (1.0));
    _39 = float3(_134.x, _134.y, _39.z);
    _35 = (-_45__m4) + 1.0;
    _41 = _35 + _39.x;
    _39.x = (-i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.y) + i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.w;
    _39.x = max(_39.x, 0.0);
    _35 += _39.x;
    _41 = (-_35) + _41;
    _41 = 1.0 / _41;
    float3 _171 = tex2D(_NoiseMapRGB, i.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.zw).xyz;
    _33 = float4(_171.x, _171.y, _171.z, _33.w);
    _31 = _33.xy + (-0.5);
    _31 = _33.zz * _31;
    _30 = (_31 * float2(_45__m2, _45__m2)) + i.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.xy;
    _33 = tex2D(_MaskMapRGBA, _30);
    _35 = (-_35) + _33.z;
    _35 = _41 * _35;
    _35 = clamp(_35, 0.0, 1.0);
    _41 = (_35 * (-2.0)) + 3.0;
    _35 *= _35;
    _35 *= _41;
    _35 *= _33.w;
    _35 = _27.x * _35;
    _27.x = (_35 * _25) + (-0.00999999977648258209228515625);
    _25 *= _35;
    _38 = _27.x < 0.0;
    if ((int(_38) * (-1)) != 0)
    {
        // discard;
    }
    _27.x = (-_39.x) + _39.y;
    _39.x = (-_39.x) + _33.z;
    _27.x = 1.0 / _27.x;
    _27.x *= _39.x;
    _27.x = clamp(_27.x, 0.0, 1.0);
    _39.x = (_27.x * (-2.0)) + 3.0;
    _27.x *= _27.x;
    _27.x = ((-_39.x) * _27.x) + 1.0;
    _27.x = (_27.x * 4.0) + (-_33.y);
    _27.x = (i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.w * _27.x) + _33.y;
    _39 = i.Varying_BColor_1 + (-i.Varying_BColor_2);
    _39 = (_33.xxx * _39) + i.Varying_BColor_2;
    _27 = (i.Varying_MoreFadeTwoPartColor * _27.xxx) + _39;
    _34 = i.Varying_BColor_1 * (_45__m0);
    _27 = (_34 * (0.4000000059604644775390625)) + _27;
    _27 = (i.Varying_TwoPartColor * _33.xxx) + _27;
    _42 = i.Varying_DesityRefW_ColorzwYZ_LDotDir01FixX.x + 1.0;
    _27 = (_27 * (_42)) + (-i.Varying_IrradianceColor);
    _42 = _45__m0 + (-0.4000000059604644775390625);
    _42 *= 3.333333492279052734375;
    _42 = clamp(_42, 0.0, 1.0);
    _34.x = (_42 * (-2.0)) + 3.0;
    _42 *= _42;
    _42 *= _34.x;
    _34.x = i.Varying_RelativeToRoleDirXYZ_Angle1_n1.w * 10.0;
    _34.x = clamp(_34.x, 0.0, 1.0);
    _40 = (_34.x * (-2.0)) + 3.0;
    _34.x *= _34.x;
    _34.x *= _40;
    _34.x = min(_34.x, 1.0);
    _40 = (-_34.x) + 1.0;
    _42 = (_42 * _40) + _34.x;
    _27 = ((_42) * _27) + i.Varying_IrradianceColor;
    Output_0 = float4(_27.x, _27.y, _27.z, Output_0.w);
    Output_1 = float4(_27.x, _27.y, _27.z, Output_1.w);
    Output_0.w = _25;
    Output_1.w = _25;
    
    fixed4 col = Output_0.xyzw;
    // col = fixed4(1,0,0,1);
    // col = float4(i.Varying_1.w, 0,0,1);
    return col;
}
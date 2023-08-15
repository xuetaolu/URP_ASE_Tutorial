Shader "AC/AC_skybox_hdr"
{
    Properties
    {
//        _MainTex ("Texture", 2D) = "white" {}
//        _SunDirection ("_SunDirection", Vector) = (0.55817, 0.03907, 0.8288, 0.00 )
        [Header(0_3)]
        _ScatterMie ("_ScatterMie", Range(0, 1)) = 0.02928
        [Header(0_3_to_0_9)]
        _ScatterIntensity ("_ScatterIntensity", Range(0, 1)) = 0.92808
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
//        Cull Off

        Pass
        {
            CGPROGRAM
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            /// 声明替换宏
            #define support_buffer_m5_m0 1.0 // support_buffer_1._m5[0]

            // 不变的
            #define fp_c1__m0 float4(-4.04496E+07, 0.0001, -4.24974E+07, 1.00) // fp_c1_1._m0[0]
            #define fp_c1__m1 float4(3.83, 0.459, -0.00287, 0.91097) // fp_c1_1._m0[1]
            #define fp_c1__m2 float4(4.11984, 33.52735, 0.00, 0.00) // fp_c1_1._m0[2]

            // 变的
            // #define _Property_10X_ float4(10.00, 0.02928, 0.92808, 0.00) // fp_c3_1._m0[6]
            float _ScatterMie;
            float _ScatterIntensity;
            #define _Property_10X_ float4(10.00, _ScatterMie, _ScatterIntensity, 0.00) // fp_c3_1._m0[6]
            
            float3 _SunLookAtDir; // float4(0.55817, 0.03907, 0.8288, 0.00 ) // fp_c3_1._m0[10]
            // X 取反
            #define _SUN_LOOK_AT_DIRECTION float4( -_SunLookAtDir.x, _SunLookAtDir.y, _SunLookAtDir.z, 0 )

            // 不变的
            #define _ScreenSizeXY float4(256.00, 128.00, 0.00, 1.34525E-43 ) // fp_c4_1._m0[0]

            #define PI          3.14159265358979323846
            #define TWO_PI      6.28318530717958647693

            #define fma(a,b,c) (a*b+c)
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,1);
                float4 Output_RGBA = 0;

                // float _35 = gl_FragCoord.y;
                // float _37 = support_buffer_m5_m0;
                //     float _927 = gl_FragCoord.y / _37;
                // float _screenPosY = gl_FragCoord.y / support_buffer_m5_m0;
                // float _41 = gl_FragCoord.x;
                // float _43 = support_buffer_m5_m0;
                //     float _934 = gl_FragCoord.x / support_buffer_m5_m0;
                // float _screenPosX = gl_FragCoord.x / support_buffer_m5_m0;
                //     float _938 = 1.0 / _ScreenSizeXY.y;
                // float _47 = _938;
                //     float _941 = 1.0 / _ScreenSizeXY.x;
                // float _49 = _941;
                //     float _944 = _screenPosY / _ScreenSizeXY.y;
                // float _51 = _944;
                // float _screenPos01Y = clamp(_screenPosY / _ScreenSizeXY.y, 0.0, 1.0);
                //     float _951 = _screenPosX / _ScreenSizeXY.x;
                // float _55 = _951;
                // float _screenPos01X = clamp(_screenPosX / _ScreenSizeXY.x, 0.0, 1.0);
                //     float _956 = _screenPos01Y * PI;

                float _screenPos01Y = 1 - i.uv.y;
                float _screenPos01X = i.uv.x;
                // 
                // “极角”（polar angle） theta θ
                float _theta = _screenPos01Y * PI;
                //     float _959 = _screenPos01X * TWO_PI;

                // “方位角”（azimuth angle） phi φ
                float _phi = _screenPos01X * TWO_PI;

                // 根号(x?+z?) = sin θ
                float _sqrt_x2_add_z2 = sin(_theta);

                // 高度 Y = cos θ
                float _Y = cos(_theta);
                float _Z_unit = cos(_phi);
                float _X_unit = sin(_phi);
                //     float _970 = _sqrt_x2_add_z2 * _Z_unit;
                float _Z = _sqrt_x2_add_z2 * _Z_unit;
                //     float _973 = _sqrt_x2_add_z2 * _X_unit;
                float _X = _sqrt_x2_add_z2 * _X_unit;
                //     float _976 = _Z * _Z;
                float _Z2 = _Z * _Z;

                //     float _981 = _Property_10X_.x + 6360.0;

                // #define _Property_10X_ float4(10.00, 0.02928, 0.92808, 0.00) // fp_c3_1._m0[6]

                // 假定地球是圆的，半径为6378.1公里。
                #define EARTH_RADIUS 6360.0
                float _atmosphere_A_radius = _Property_10X_.x + EARTH_RADIUS;

                float _Y2_add_Z2 = fma(_Y, _Y, _Z2);

                // #define fp_c1__m0 float4(-4.04496E+07, 0.0001, -4.24974E+07, 1.00) // fp_c1_1._m0[0]
                float _81 = fma(_atmosphere_A_radius, _atmosphere_A_radius, fp_c1__m0.x);

                float _XYZ2 = fma(_X, _X, _Y2_add_Z2);
                //     float _997 = _Y * 2.0;
                // float _85 = _Y * 2.0;
                //     float _1000 = _Y * 2.0 * _atmosphere_A_radius;
                float _87 = _Y * 2.0 * _atmosphere_A_radius;

                //     float _1003 = _81 * 4.0;
                // float _89 = _81 * 4.0;
                //     float _1006 = _81 * 4.0 * _XYZ2;
                float _91 = _81 * 4.0 * _XYZ2;
                float _93 = -_91;
                float _95 = fma(_87, _87, _93);
                bool _99 = _95 < fp_c1__m0.y;
                float _101 = 0.0;
                float _103 = _theta;
                float _105 = _phi;
                float _107 = _95;
                float _109 = _87;
                if (_99)
                {
                    _101 = 0.0;
                }
                float _111 = _101;
                if (_99)
                {
                    _103 = 0.0;
                }
                float _113 = _103;
                if (_99)
                {
                    _105 = 0.0;
                }
                float _115 = _105;
                if (!_99)
                {
                    float _117 = sqrt(_95);
                        float _1039 = 1.0 / _XYZ2;
                    float _119 = _1039;
                    float _121 = -_87;
                    float _123 = -_117;
                        float _1046 = _121 + _123;
                    float _125 = _1046;
                        float _1049 = _125 * _119;
                    float _127 = _1049;
                        float _1052 = _Y * _127;
                    float _129 = _1052;
                        float _1055 = _Z * 0.5;
                    float _131 = _1055;
                        float _1058 = _131 * _127;
                    float _133 = _1058;
                        float _1060 = _X * 0.5;
                    float _135 = _1060;
                        float _1063 = _135 * _127;
                    float _137 = _1063;
                    float _139 = fma(_129, 0.5, _atmosphere_A_radius);
                    _113 = _139;
                    _111 = _133;
                    _115 = _137;
                    _107 = _117;
                    _109 = _125;
                }
                bool _141 = _113 == 0.0;
                bool _143 = _111 == 0.0;
                bool _145 = _143 && _141;
                bool _147 = _115 == 0.0;
                bool _149 = _147 && _145;
                float _151 = _107;
                float _153 = _109;
                if (!_149)
                {
                    float _155 = -_atmosphere_A_radius;
                        float _1094 = _155 + _113;
                    float _157 = _1094;
                    _151 = _157;
                }
                float _159 = _151;
                if (!_149)
                {
                        float _1103 = _Z * _111;
                    float _161 = _1103;
                    _153 = _161;
                }
                float _163 = _153;
                if (!_149)
                {
                    float _165 = fma(_Y, _151, _153);
                    _163 = _165;
                }
                float _167 = _163;
                if (!_149)
                {
                    float _169 = fma(_X, _115, _163);
                    _167 = _169;
                }
                bool _171 = _167 > 0.0;
                bool _173 = !_149;
                bool _175 = _171 && _173;
                float _177 = _167;
                if (_175)
                {
                    _159 = _111;
                }
                float _179 = 0.0;
                float _181 = _159;
                if (_175)
                {
                    _179 = _113;
                }
                float _183 = _179;
                float _185 = _179;
                if (_175)
                {
                    _177 = _115;
                }
                float _187 = _177;
                if (!_175)
                {
                    float _189 = rsqrt(_XYZ2);
                        float _1157 = _Z * _189;
                    float _191 = _1157;
                        float _1160 = _Y * _189;
                    float _193 = _1160;
                        float _1163 = _191 * _191;
                    float _195 = _1163;
                        float _1166 = _X * _189;
                    float _197 = _1166;
                    float _199 = fma(_193, _193, _195);
                    float _201 = fma(_atmosphere_A_radius, _atmosphere_A_radius, fp_c1__m0.z);
                    float _203 = fma(_197, _197, _199);
                        float _1182 = _atmosphere_A_radius * 2.0;
                    float _205 = _1182;
                        float _1185 = _205 * _193;
                    float _207 = _1185;
                        float _1187 = _201 * 4.0;
                    float _209 = _1187;
                        float _1190 = _209 * _203;
                    float _211 = _1190;
                    float _213 = -_211;
                    float _215 = fma(_207, _207, _213);
                    bool _217 = _215 < fp_c1__m0.y;
                    float _219 = _211;
                    float _221 = _199;
                    if (_217)
                    {
                        _219 = 0.0;
                    }
                    _181 = _219;
                    if (_217)
                    {
                        _183 = 0.0;
                    }
                    _185 = _183;
                    if (_217)
                    {
                        _221 = 0.0;
                    }
                    _187 = _221;
                    if (!_217)
                    {
                        float _223 = sqrt(_215);
                            float _1221 = 1.0 / _203;
                        float _225 = _1221;
                        float _227 = -_207;
                            float _1226 = _227 + _223;
                        float _229 = _1226;
                            float _1229 = _229 * _225;
                        float _231 = _1229;
                            float _1232 = _193 * _231;
                        float _233 = _1232;
                            float _1234 = _191 * 0.5;
                        float _235 = _1234;
                            float _1237 = _235 * _231;
                        float _237 = _1237;
                            float _1239 = _197 * 0.5;
                        float _239 = _1239;
                            float _1242 = _239 * _231;
                        float _241 = _1242;
                        float _243 = fma(_233, 0.5, _atmosphere_A_radius);
                        _185 = _243;
                        _181 = _237;
                        _187 = _241;
                    }
                }
                float _245 = -_atmosphere_A_radius;
                    float _1255 = _245 + _185;
                float _247 = _1255;
                    float _1258 = _181 * _181;
                float _249 = _1258;
                    float _1261 = _atmosphere_A_radius * _atmosphere_A_radius;
                float _251 = _1261;
                float _253 = sqrt(_251);
                float _255 = fma(_247, _247, _249);
                float _257 = fma(_187, _187, _255);
                float _259 = rsqrt(_257);
                float _261 = sqrt(_257);
                float _263 = _253;
                if (_175)
                {
                    float _265 = -_253;
                        float _1283 = _265 + (-0.0);
                    float _267 = _1283;
                    _263 = _267;
                }
                    float _1287 = 1.0 / _263;
                float _269 = _1287;
                    float _1290 = _259 * _181;
                float _271 = _1290;
                    float _1293 = _261 * 0.20000000298023223876953125;
                float _273 = _1293;
                    float _1296 = _247 * _259;
                float _275 = _1296;
                    float _1299 = _271 * _273;
                float _277 = _1299;
                    float _1302 = _atmosphere_A_radius * _275;
                float _279 = _1302;
                    float _1305 = _275 * _273;
                float _281 = _1305;
                    float _1308 = _259 * _187;
                float _283 = _1308;
                    float _1310 = _277 * 0.5;
                float _285 = _1310;
                float _287 = -_279;
                float _289 = fma(_269, _287, fp_c1__m0.w);
                    float _1321 = _271 * _277;
                float _291 = _1321;
                float _293 = fma(_281, 0.5, _atmosphere_A_radius);
                    float _1327 = _283 * _273;
                float _295 = _1327;
                    float _1330 = _271 * _285;
                float _297 = _1330;
                    float _1333 = _277 + _285;
                float _299 = _1333;
                float _301 = fma(_289, 5.25, -6.80000019073486328125);
                float _303 = fma(_275, _281, _291);
                float _305 = -_SUN_LOOK_AT_DIRECTION.x;
                    float _1348 = _285 * _305;
                float _307 = _1348;
                    float _1351 = _285 * _285;
                float _309 = _1351;
                float _311 = fma(_275, _293, _297);
                    float _1357 = _295 * 0.5;
                float _313 = _1357;
                    float _1360 = _299 * _299;
                float _315 = _1360;
                    float _1363 = _277 + _299;
                float _317 = _1363;
                float _319 = fma(_289, _301, fp_c1__m1.x);
                    float _1371 = _281 + _293;
                float _321 = _1371;
                float _323 = fma(_283, _295, _303);
                float _325 = -_SUN_LOOK_AT_DIRECTION.y;
                float _327 = fma(_293, _325, _307);
                float _329 = fma(_283, _313, _311);
                float _331 = fma(_293, _293, _309);
                    float _1393 = _317 * _317;
                float _333 = _1393;
                float _335 = fma(_289, _319, fp_c1__m1.y);
                    float _1401 = _277 + _317;
                float _337 = _1401;
                float _339 = fma(_321, _321, _315);
                    float _1408 = _281 + _321;
                float _341 = _1408;
                float _343 = -_SUN_LOOK_AT_DIRECTION.x;
                    float _1414 = _277 * _343;
                float _345 = _1414;
                float _347 = fma(_289, _335, fp_c1__m1.z);
                    float _1422 = _277 + _337;
                float _349 = _1422;
                float _351 = -_253;
                    float _1426 = _351 + 6360.0;
                float _353 = _1426;
                float _355 = -_SUN_LOOK_AT_DIRECTION.z;
                float _357 = fma(_313, _355, _327);
                    float _1436 = _295 + _313;
                float _359 = _1436;
                float _361 = fma(_341, _341, _333);
                    float _1443 = _281 + _341;
                float _363 = _1443;
                float _365 = fma(_313, _313, _331);
                float _367 = -_SUN_LOOK_AT_DIRECTION.y;
                float _369 = fma(_281, _367, _345);
                float _371 = rsqrt(_365);
                    float _1459 = _349 * _349;
                float _373 = _1459;
                    float _1462 = _337 * _337;
                float _375 = _1462;
                    float _1465 = _353 * 0.036294214427471160888671875;
                float _377 = _1465;
                float _379 = fma(_359, _359, _339);
                    float _1472 = _347 * 1.44269502162933349609375;
                float _381 = _1472;
                    float _1475 = _295 + _359;
                float _383 = _1475;
                    float _1478 = _281 + _363;
                float _385 = _1478;
                float _387 = fma(_363, _363, _375);
                float _389 = rsqrt(_379);
                float _391 = exp2(_377);
                    float _1489 = _295 + _383;
                float _393 = _1489;
                float _395 = exp2(_381);
                float _397 = -_SUN_LOOK_AT_DIRECTION.z;
                float _399 = fma(_295, _397, _369);
                float _401 = fma(_385, _385, _373);
                    float _1505 = _371 * _357;
                float _403 = _1505;
                float _405 = fma(_383, _383, _361);
                float _407 = sqrt(_365);
                    float _1514 = _295 + _393;
                float _409 = _1514;
                float _411 = rsqrt(_405);
                    float _1519 = _357 + _399;
                float _413 = _1519;
                float _415 = sqrt(_379);
                float _417 = fma(_393, _393, _387);
                float _419 = sqrt(_405);
                float _421 = -_403;
                    float _1531 = _421 + 1.0;
                float _423 = _1531;
                    float _1534 = _413 * _389;
                float _425 = _1534;
                    float _1537 = _399 + _413;
                float _427 = _1537;
                    float _1540 = _391 * 0.25;
                float _429 = _1540;
                    float _1543 = _429 * _395;
                float _431 = _1543;
                float _433 = rsqrt(_417);
                float _435 = fma(_409, _409, _401);
                float _437 = sqrt(_417);
                float _439 = fma(_423, 5.25, -6.80000019073486328125);
                    float _1556 = _399 + _427;
                float _441 = _1556;
                float _443 = -_425;
                    float _1560 = _443 + 1.0;
                float _445 = _1560;
                float _447 = fma(_423, _439, fp_c1__m1.x);
                float _449 = _407;
                float _451 = _415;
                if (_175)
                {
                    float _453 = -_407;
                        float _1573 = _453 + (-0.0);
                    float _455 = _1573;
                    _449 = _455;
                }
                    float _1578 = _427 * _411;
                float _457 = _1578;
                    float _1580 = 1.0 / _449;
                float _459 = _1580;
                if (_175)
                {
                    float _461 = -_415;
                        float _1586 = _461 + (-0.0);
                    float _463 = _1586;
                    _451 = _463;
                }
                    float _1591 = _399 + _441;
                float _465 = _1591;
                    float _1594 = _441 * _433;
                float _467 = _1594;
                    float _1596 = 1.0 / _451;
                float _469 = _1596;
                float _471 = fma(_423, _447, fp_c1__m1.y);
                float _473 = fma(_445, 5.25, -6.80000019073486328125);
                    float _1606 = _329 + _323;
                float _475 = _1606;
                float _477 = _419;
                if (_175)
                {
                    float _479 = -_419;
                        float _1613 = _479 + (-0.0);
                    float _481 = _1613;
                    _477 = _481;
                }
                float _483 = fma(_423, _471, fp_c1__m1.z);
                    float _1622 = 1.0 / _477;
                float _485 = _1622;
                float _487 = fma(_445, _473, fp_c1__m1.x);
                float _489 = -_329;
                float _491 = fma(_459, _489, fp_c1__m0.w);
                float _493 = -_469;
                float _495 = fma(_475, _493, fp_c1__m0.w);
                    float _1644 = _323 + _475;
                float _497 = _1644;
                float _499 = fma(_445, _487, fp_c1__m1.y);
                float _501 = rsqrt(_435);
                float _503 = _437;
                if (_175)
                {
                    float _505 = -_437;
                        float _1658 = _505 + (-0.0);
                    float _507 = _1658;
                    _503 = _507;
                }
                float _509 = fma(_491, 5.25, -6.80000019073486328125);
                    float _1664 = 1.0 / _503;
                float _511 = _1664;
                float _513 = fma(_445, _499, fp_c1__m1.z);
                float _515 = -_485;
                float _517 = fma(_497, _515, fp_c1__m0.w);
                    float _1679 = _323 + _497;
                float _519 = _1679;
                float _521 = -_457;
                    float _1683 = _521 + 1.0;
                float _523 = _1683;
                float _525 = fma(_491, _509, fp_c1__m1.x);
                    float _1691 = _323 + _519;
                float _527 = _1691;
                float _529 = fma(_523, 5.25, -6.80000019073486328125);
                float _531 = -_511;
                float _533 = fma(_519, _531, fp_c1__m0.w);
                float _535 = fma(_523, _529, fp_c1__m1.x);
                float _537 = sqrt(_435);
                float _539 = fma(_523, _535, fp_c1__m1.y);
                float _541 = fma(_495, 5.25, -6.80000019073486328125);
                float _543 = fma(_491, _525, fp_c1__m1.y);
                float _545 = fma(_495, _541, fp_c1__m1.x);
                float _547 = fma(_491, _543, fp_c1__m1.z);
                    float _1732 = _465 * _501;
                float _549 = _1732;
                float _551 = fma(_523, _539, fp_c1__m1.z);
                float _553 = fma(_495, _545, fp_c1__m1.y);
                float _555 = fma(_495, _553, fp_c1__m1.z);
                float _557 = fma(_517, 5.25, -6.80000019073486328125);
                float _559 = _537;
                if (_175)
                {
                    float _561 = -_537;
                        float _1756 = _561 + (-0.0);
                    float _563 = _1756;
                    _559 = _563;
                }
                float _565 = -_467;
                    float _1762 = _565 + 1.0;
                float _567 = _1762;
                    float _1764 = 1.0 / _559;
                float _569 = _1764;
                float _571 = fma(_517, _557, fp_c1__m1.x);
                float _573 = -_549;
                    float _1773 = _573 + 1.0;
                float _575 = _1773;
                float _577 = fma(_567, 5.25, -6.80000019073486328125);
                float _579 = fma(_517, _571, fp_c1__m1.y);
                float _581 = fma(_567, _577, fp_c1__m1.x);
                float _583 = -_569;
                float _585 = fma(_527, _583, fp_c1__m0.w);
                float _587 = fma(_575, 5.25, -6.80000019073486328125);
                float _589 = fma(_517, _579, fp_c1__m1.z);
                float _591 = fma(_533, 5.25, -6.80000019073486328125);
                    float _1803 = _483 * 1.44269502162933349609375;
                float _593 = _1803;
                    float _1805 = _547 * 1.44269502162933349609375;
                float _595 = _1805;
                float _597 = fma(_575, _587, fp_c1__m1.x);
                float _599 = fma(_533, _591, fp_c1__m1.x);
                float _601 = fma(_567, _581, fp_c1__m1.y);
                float _603 = fma(_575, _597, fp_c1__m1.y);
                float _605 = fma(_533, _599, fp_c1__m1.y);
                float _607 = fma(_567, _601, fp_c1__m1.z);
                float _609 = exp2(_593);
                float _611 = fma(_575, _603, fp_c1__m1.z);
                float _613 = exp2(_595);
                float _615 = fma(_533, _605, fp_c1__m1.z);
                float _617 = fma(_585, 5.25, -6.80000019073486328125);
                    float _1855 = _Property_10X_.z * 0.01884955726563930511474609375;
                float _619 = _1855;
                float _621 = fma(_585, _617, fp_c1__m1.x);
                float _623 = -_407;
                    float _1864 = _623 + 6360.0;
                float _625 = _1864;
                float _627 = -_613;
                    float _1869 = _609 + _627;
                float _629 = _1869;
                float _631 = fma(_Property_10X_.y, fp_c1__m1.w, _619);
                float _633 = fma(_Property_10X_.y, fp_c1__m2.x, _619);
                float _635 = fma(_585, _621, fp_c1__m1.y);
                float _637 = fma(_Property_10X_.y, fp_c1__m2.y, _619);
                    float _1894 = _625 * 0.036294214427471160888671875;
                float _639 = _1894;
                float _641 = -_415;
                    float _1898 = _641 + 6360.0;
                float _643 = _1898;
                float _645 = -_419;
                    float _1902 = _645 + 6360.0;
                float _647 = _1902;
                    float _1904 = _551 * 1.44269502162933349609375;
                float _649 = _1904;
                float _651 = fma(_585, _635, fp_c1__m1.z);
                    float _1911 = _513 * 1.44269502162933349609375;
                float _653 = _1911;
                    float _1913 = _555 * 1.44269502162933349609375;
                float _655 = _1913;
                    float _1915 = _643 * 0.036294214427471160888671875;
                float _657 = _1915;
                    float _1917 = _647 * 0.036294214427471160888671875;
                float _659 = _1917;
                    float _1919 = _607 * 1.44269502162933349609375;
                float _661 = _1919;
                    float _1921 = _589 * 1.44269502162933349609375;
                float _663 = _1921;
                    float _1923 = _611 * 1.44269502162933349609375;
                float _665 = _1923;
                float _667 = exp2(_639);
                float _669 = exp2(_653);
                    float _1929 = _615 * 1.44269502162933349609375;
                float _671 = _1929;
                float _673 = exp2(_655);
                    float _1933 = _651 * 1.44269502162933349609375;
                float _675 = _1933;
                float _677 = -_437;
                    float _1937 = _677 + 6360.0;
                float _679 = _1937;
                float _681 = exp2(_649);
                float _683 = exp2(_663);
                float _685 = exp2(_657);
                float _687 = exp2(_661);
                float _689 = exp2(_665);
                float _691 = exp2(_671);
                    float _1951 = _679 * 0.036294214427471160888671875;
                float _693 = _1951;
                float _695 = exp2(_675);
                float _697 = -_537;
                    float _1957 = _697 + 6360.0;
                float _699 = _1957;
                float _701 = exp2(_659);
                    float _1962 = _629 * _667;
                float _703 = _1962;
                float _705 = -_673;
                    float _1967 = _669 + _705;
                float _707 = _1967;
                    float _1969 = _699 * 0.036294214427471160888671875;
                float _709 = _1969;
                float _711 = exp2(_693);
                float _713 = -_683;
                    float _1976 = _681 + _713;
                float _715 = _1976;
                float _717 = fma(_703, 0.25, _431);
                    float _1982 = _261 * 0.001257861615158617496490478515625;
                float _719 = _1982;
                    float _1985 = _685 * _707;
                float _721 = _1985;
                float _723 = -_695;
                    float _1990 = _689 + _723;
                float _725 = _1990;
                float _727 = exp2(_709);
                float _729 = -_691;
                    float _1997 = _687 + _729;
                float _731 = _1997;
                    float _2000 = _701 * _715;
                float _733 = _2000;
                float _735 = -_717;
                    float _2005 = _631 * _735;
                float _737 = _2005;
                float _739 = -_717;
                    float _2010 = _633 * _739;
                float _741 = _2010;
                float _743 = fma(_721, 0.25, _431);
                float _745 = -_717;
                    float _2018 = _637 * _745;
                float _747 = _2018;
                    float _2021 = _711 * _731;
                float _749 = _2021;
                    float _2023 = _737 * 1.44269502162933349609375;
                float _751 = _2023;
                    float _2025 = _741 * 1.44269502162933349609375;
                float _753 = _2025;
                float _755 = -_743;
                    float _2030 = _631 * _755;
                float _757 = _2030;
                    float _2032 = _747 * 1.44269502162933349609375;
                float _759 = _2032;
                float _761 = fma(_733, 0.25, _431);
                    float _2038 = _727 * _725;
                float _763 = _2038;
                float _765 = exp2(_751);
                float _767 = -_743;
                    float _2045 = _633 * _767;
                float _769 = _2045;
                    float _2047 = _757 * 1.44269502162933349609375;
                float _771 = _2047;
                float _773 = exp2(_753);
                float _775 = -_761;
                    float _2054 = _631 * _775;
                float _777 = _2054;
                float _779 = fma(_749, 0.25, _431);
                float _781 = fma(_763, 0.25, _431);
                    float _2062 = _769 * 1.44269502162933349609375;
                float _783 = _2062;
                float _785 = exp2(_759);
                    float _2067 = _719 * _667;
                float _787 = _2067;
                float _789 = exp2(_771);
                    float _2072 = _719 * _685;
                float _791 = _2072;
                float _793 = -_761;
                    float _2077 = _633 * _793;
                float _795 = _2077;
                float _797 = -_743;
                    float _2082 = _637 * _797;
                float _799 = _2082;
                    float _2084 = _777 * 1.44269502162933349609375;
                float _801 = _2084;
                    float _2087 = _773 * _787;
                float _803 = _2087;
                    float _2090 = _765 * _787;
                float _805 = _2090;
                    float _2092 = _795 * 1.44269502162933349609375;
                float _807 = _2092;
                    float _2094 = _799 * 1.44269502162933349609375;
                float _809 = _2094;
                float _811 = exp2(_801);
                    float _2099 = _787 * _785;
                float _813 = _2099;
                float _815 = fma(_789, _791, _805);
                float _817 = exp2(_783);
                float _819 = -_761;
                    float _2110 = _637 * _819;
                float _821 = _2110;
                    float _2113 = _719 * _701;
                float _823 = _2113;
                float _825 = exp2(_809);
                float _827 = -_779;
                    float _2120 = _631 * _827;
                float _829 = _2120;
                float _831 = -_779;
                    float _2125 = _633 * _831;
                float _833 = _2125;
                float _835 = -_779;
                    float _2130 = _637 * _835;
                float _837 = _2130;
                    float _2132 = _821 * 1.44269502162933349609375;
                float _839 = _2132;
                float _841 = fma(_811, _823, _815);
                float _843 = exp2(_807);
                float _845 = -_781;
                    float _2143 = _631 * _845;
                float _847 = _2143;
                float _849 = -_781;
                    float _2148 = _633 * _849;
                float _851 = _2148;
                    float _2150 = _829 * 1.44269502162933349609375;
                float _853 = _2150;
                    float _2152 = _833 * 1.44269502162933349609375;
                float _855 = _2152;
                float _857 = -_781;
                    float _2157 = _637 * _857;
                float _859 = _2157;
                    float _2159 = _837 * 1.44269502162933349609375;
                float _861 = _2159;
                float _863 = fma(_817, _791, _803);
                float _865 = exp2(_839);
                float _867 = fma(_791, _825, _813);
                    float _2172 = _719 * _711;
                float _869 = _2172;
                    float _2174 = _847 * 1.44269502162933349609375;
                float _871 = _2174;
                float _873 = exp2(_853);
                    float _2178 = _851 * 1.44269502162933349609375;
                float _875 = _2178;
                float _877 = exp2(_855);
                    float _2182 = _859 * 1.44269502162933349609375;
                float _879 = _2182;
                float _881 = exp2(_861);
                float _883 = exp2(_871);
                float _885 = fma(_843, _823, _863);
                float _887 = exp2(_875);
                float _889 = fma(_823, _865, _867);
                float _891 = exp2(_879);
                    float _2201 = _719 * _727;
                float _893 = _2201;
                float _895 = fma(_873, _869, _841);
                float _897 = fma(_877, _869, _885);
                float _899 = fma(_869, _881, _889);
                float _901 = _727;
                if (_175)
                {
                    _901 = _Property_10X_.w;
                }
                float _903 = fma(_883, _893, _895);
                float _905 = _901;
                if (!_175)
                {
                    _905 = 1.0;
                }
                float _907 = fma(_887, _893, _897);
                float _909 = fma(_893, _891, _899);
                    float _2239 = _905 * _903;
                float _911 = _2239;
                    float _2242 = _905 * _907;
                float _913 = _2242;
                    float _2245 = _905 * _909;
                float _915 = _2245;
                Output_RGBA.x = _911;
                Output_RGBA.y = _913;
                Output_RGBA.z = _915;
                Output_RGBA.w = 1.0;

                col = Output_RGBA;
                // col.rg = i.uv;
                
                return col;
            }
            ENDCG
        }
    }
}
